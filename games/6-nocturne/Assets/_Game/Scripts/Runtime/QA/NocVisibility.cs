using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    /// <summary>
    /// The pure maths of Abyss's CompositionLint (IMPORT_PLAN 8.18): project a subject's world bounds through a pinhole camera
    /// (vertical FOV, aspect) with near-plane clipping of the box edges, judge centre / size / 3 % margin (Part 11.2), and the
    /// depth-grid occlusion test (an 8 x 8 grid of rays over the in-frame rectangle against the subject box, read against a
    /// linear eye-depth buffer), for screen-space checks against a captured depth buffer. The EditMode 11.13 visibility and
    /// reachability audit itself is VisibilityAudit (spec subjects VisibilitySubjects, spec occluders VisibilitySpecWorld, built
    /// occluders VisibilityRaycaster, follower VisibilityFollower; run by VisibilityAuditTests, which writes
    /// QA/reports/visibility.json and visibility.md). Abyss's beat hooks
    /// (BeatBase.CollectSubjects), its logged-conflict table and its QaJson are not imported; reports use Thread's QaJson.
    /// Screen fractions use a top-left origin like the Python tools.
    /// </summary>
    public static class NocVisibility
    {
        public const float CentreMin = 0.05f, CentreMax = 0.95f, Margin = 0.03f;
        /// <summary>Minimum unoccluded fraction of a subject's in-frame footprint.</summary>
        public const float MinVisible = 0.30f;
        public const int VisGrid = 8;

        public struct SubjectResult
        {
            public string Name;
            public bool Behind;
            public float X0, Y0, X1, Y1;   // projected rectangle (unclamped fractions, top-left origin)
            public float Cx, Cy;
            /// <summary>Visible size in fractions of the frame height: max(visible height, visible width x aspect).</summary>
            public float Size, MinSize;
            public bool CentreOk, SizeOk, MarginOk, Pass;
            /// <summary>Projected box larger than the frame in an axis, or cut by the near plane.</summary>
            public bool Oversize;
            public float InCx, InCy;
            public bool InFramePass;
            /// <summary>Unoccluded fraction of the in-frame footprint (0..1), or -1 without a depth buffer.</summary>
            public float Visible;
            public int VisSamples;
            public bool FramePass => Pass || InFramePass;
            public bool VisibilityOk => Visible < 0f || Visible >= MinVisible;
            public bool LintPass => FramePass && VisibilityOk;
        }

        /// <summary>Project a world point (fractions, top-left origin). False when it lies behind the near plane.</summary>
        public static bool ProjectPoint(Vector3 p, Vector3 eye, Quaternion rot, float vfovDeg, float aspect, float near, out float u, out float v)
        {
            Vector3 c = Quaternion.Inverse(rot) * (p - eye);
            u = v = 0f;
            if (c.z < near) return false;
            float t = Mathf.Tan(0.5f * vfovDeg * Mathf.Deg2Rad);
            u = 0.5f + 0.5f * c.x / (c.z * t * Mathf.Max(1e-4f, aspect));
            v = 0.5f - 0.5f * c.y / (c.z * t);
            return true;
        }

        static readonly Vector3[] s_c = new Vector3[8];

        /// <summary>Project a world box (edges clipped by the near plane). False when it lies entirely behind the near plane.</summary>
        public static bool ProjectBounds(Bounds b, Vector3 eye, Quaternion rot, float vfovDeg, float aspect, float near,
                                         out float x0, out float y0, out float x1, out float y1, out bool partlyBehind)
        {
            Quaternion inv = Quaternion.Inverse(rot);
            Vector3 mn = b.min, mx = b.max;
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) != 0 ? mx.x : mn.x, (i & 2) != 0 ? mx.y : mn.y, (i & 4) != 0 ? mx.z : mn.z);
                s_c[i] = inv * (corner - eye);
            }
            float t = Mathf.Tan(0.5f * vfovDeg * Mathf.Deg2Rad);
            float ta = t * Mathf.Max(1e-4f, aspect);
            float u0 = float.MaxValue, v0 = float.MaxValue, u1 = float.MinValue, v1 = float.MinValue;
            bool any = false;
            partlyBehind = false;
            for (int i = 0; i < 8; i++)
            {
                if (s_c[i].z >= near) Add(s_c[i], t, ta, ref u0, ref v0, ref u1, ref v1, ref any);
                else partlyBehind = true;
            }
            for (int i = 0; i < 8; i++)
                for (int bit = 1; bit <= 4; bit <<= 1)
                {
                    if ((i & bit) != 0) continue;
                    Vector3 a = s_c[i], c = s_c[i | bit];
                    if ((a.z >= near) == (c.z >= near)) continue;
                    float k = (near - a.z) / (c.z - a.z);
                    Add(Vector3.LerpUnclamped(a, c, k), t, ta, ref u0, ref v0, ref u1, ref v1, ref any);
                }
            x0 = u0; y0 = v0; x1 = u1; y1 = v1;
            if (!any) { x0 = y0 = x1 = y1 = 0f; partlyBehind = true; }
            return any;
        }

        static void Add(Vector3 p, float t, float ta, ref float u0, ref float v0, ref float u1, ref float v1, ref bool any)
        {
            float z = Mathf.Max(p.z, 1e-6f);
            float u = 0.5f + 0.5f * p.x / (z * ta);
            float v = 0.5f - 0.5f * p.y / (z * t);
            if (u < u0) u0 = u;
            if (u > u1) u1 = u;
            if (v < v0) v0 = v;
            if (v > v1) v1 = v;
            any = true;
        }

        /// <summary>Composition check of one subject box (centre inside 5-95 %, size, 3 % margin, in-frame rule for oversize boxes).</summary>
        public static SubjectResult Check(string name, Bounds box, float minScreenFraction, Vector3 eye, Quaternion rot, float vfovDeg, float aspect, float near)
        {
            var r = new SubjectResult { Name = name, MinSize = minScreenFraction, Visible = -1f };
            bool visible = ProjectBounds(box, eye, rot, vfovDeg, aspect, near, out r.X0, out r.Y0, out r.X1, out r.Y1, out bool partly);
            r.Behind = !visible;
            if (!visible) return r;
            r.Cx = 0.5f * (r.X0 + r.X1);
            r.Cy = 0.5f * (r.Y0 + r.Y1);
            float vx0 = Mathf.Clamp01(r.X0), vx1 = Mathf.Clamp01(r.X1), vy0 = Mathf.Clamp01(r.Y0), vy1 = Mathf.Clamp01(r.Y1);
            r.Size = Mathf.Max(vy1 - vy0, (vx1 - vx0) * aspect);
            r.CentreOk = r.Cx >= CentreMin && r.Cx <= CentreMax && r.Cy >= CentreMin && r.Cy <= CentreMax;
            r.SizeOk = minScreenFraction <= 0f || r.Size >= minScreenFraction;
            r.MarginOk = !partly && r.X0 >= Margin && r.X1 <= 1f - Margin && r.Y0 >= Margin && r.Y1 <= 1f - Margin;
            r.Pass = r.CentreOk && r.SizeOk;
            r.Oversize = partly || r.X1 - r.X0 > 1f || r.Y1 - r.Y0 > 1f;
            r.InCx = 0.5f * (vx0 + vx1);
            r.InCy = 0.5f * (vy0 + vy1);
            r.InFramePass = r.Oversize && vx1 > vx0 && vy1 > vy0 && r.SizeOk
                            && r.InCx >= CentreMin && r.InCx <= CentreMax && r.InCy >= CentreMin && r.InCy <= CentreMax;
            return r;
        }

        /// <summary>Depth-grid occlusion test (linear eye depth in metres per pixel, texture order: row 0 = bottom).</summary>
        public static void Visibility(ref SubjectResult r, Bounds b, Vector3 eye, Quaternion rot, float vfovDeg, float aspect, float near,
                                      float[] depth, int w, int h)
        {
            r.Visible = -1f;
            r.VisSamples = 0;
            if (depth == null || w <= 0 || h <= 0 || depth.Length < w * h || r.Behind) return;
            float x0 = Mathf.Clamp01(r.X0), x1 = Mathf.Clamp01(r.X1), y0 = Mathf.Clamp01(r.Y0), y1 = Mathf.Clamp01(r.Y1);
            if (x1 <= x0 || y1 <= y0) { r.Visible = 0f; return; }
            float t = Mathf.Tan(0.5f * vfovDeg * Mathf.Deg2Rad), ta = t * Mathf.Max(1e-4f, aspect);
            int hit = 0, vis = 0;
            for (int iy = 0; iy < VisGrid; iy++)
                for (int ix = 0; ix < VisGrid; ix++)
                {
                    float u = x0 + (x1 - x0) * (ix + 0.5f) / VisGrid;
                    float v = y0 + (y1 - y0) * (iy + 0.5f) / VisGrid;
                    Vector3 dir = rot * new Vector3((2f * u - 1f) * ta, (1f - 2f * v) * t, 1f);
                    if (!RayBox(eye, dir, b.min, b.max, out float tIn, out float tOut)) continue;
                    float entry = Mathf.Max(tIn, near);
                    if (tOut < entry) continue;
                    hit++;
                    int px = Mathf.Clamp((int)(u * w), 0, w - 1);
                    int py = Mathf.Clamp((int)((1f - v) * h), 0, h - 1);
                    if (depth[py * w + px] >= entry * 0.99f - 0.05f) vis++;
                }
            r.VisSamples = hit;
            r.Visible = hit > 0 ? (float)vis / hit : 0f;
        }

        /// <summary>Slab test of the ray o + t d against the box [mn, mx].</summary>
        public static bool RayBox(Vector3 o, Vector3 d, Vector3 mn, Vector3 mx, out float tIn, out float tOut)
        {
            tIn = float.NegativeInfinity;
            tOut = float.PositiveInfinity;
            for (int a = 0; a < 3; a++)
            {
                float oa = o[a], da = d[a], lo = mn[a], hi = mx[a];
                if (Mathf.Abs(da) < 1e-9f)
                {
                    if (oa < lo || oa > hi) return false;
                    continue;
                }
                float t0 = (lo - oa) / da, t1 = (hi - oa) / da;
                if (t0 > t1) { float k = t0; t0 = t1; t1 = k; }
                if (t0 > tIn) tIn = t0;
                if (t1 < tOut) tOut = t1;
                if (tIn > tOut) return false;
            }
            return tOut >= 0f;
        }

        /// <summary>Writes a list of results as JSON (Thread's QaJson) for QA/reports/visibility.json.</summary>
        public static void Write(QaJson j, string shot, IList<SubjectResult> results)
        {
            j.BeginObject().Prop("shot", shot).Key("subjects").BeginArray();
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                j.BeginObject(true).Prop("name", r.Name).Prop("behind", r.Behind).Prop("cx", r.Cx).Prop("cy", r.Cy).Prop("size", r.Size)
                 .Prop("marginOk", r.MarginOk).Prop("visible", r.Visible).Prop("pass", r.LintPass).EndObject();
            }
            j.EndArray().EndObject();
        }
    }
}
