// Beat01AlleyTests: the acceptance dimensions of beat 1 (MASTER_PROMPT 14.5.1 Acceptance (1) and (5), the KS00-KS02 projection
// numbers of 14.5.1 / 14.10) measured on the BUILT geometry: Beat01_Alley.BuildParts() runs the real builders without a scene and
// the tests ray-cast their triangles (G1TestGeo, below; also used by Beat02BoulevardTests):
//   widths 4.4 / 6.0 / 4.6 / 5.0 +-0.02 at the listed s (horizontal rays at y 1.0, or above an opening), podia 22.0 / 31.0 +-0.05
//   (vertical rays onto the roofs), the KS00 slit 33 +-3 px at 1080p (ray sweep from the eye to the mouth, projected through the
//   KS00 camera), the KS02 slit half-angles 6.19 / 4.29 deg (ray sweeps across the roof edges at s 88) and the roof-edge, catwalk
//   and tower-flank projections, the KS01 window / sign / counter projections, every mesh valid, <= 480 k triangles, and the
//   walkability clearances: along both lane edges and the centre (s 0-120, every 0.25 m) the camera sphere (r 0.2 at y 1.7) and
//   the player capsule (r 0.35, y 0..1.8) never enter a triangle (solid parts: AL's steam, pool decals and mouth haze excluded); the
//   wet sources of the beat never exceed 32 at any s; AL (wave B): the kit counts, the framed windows and the modelled rooms;
//   AL (fix wave): the catwalk work floodlights aim at the KS02 texture regions and their baked wash (D-039 (6)) triples the walls
//   there and leaves the catwalk walls dark; the empty rooms are enclosed with the CC0 plaster (C37); the vents' steam (C38).
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Megastructure.Tests
{
    /// <summary>Ray and distance queries on G1Parts (world triangles, binned along z / x for speed).</summary>
    internal sealed class G1TestGeo
    {
        public readonly List<Vector3> A = new List<Vector3>(), B = new List<Vector3>(), C = new List<Vector3>();
        public readonly List<string> Part = new List<string>();

        public G1TestGeo(G1Parts parts, Func<string, bool> include = null)
        {
            foreach (var p in parts.Parts)
            {
                if (include != null && !include(p.Name)) continue;
                var pos = p.Mb.Positions; var idx = p.Mb.Indices;
                for (int i = 0; i + 2 < idx.Count; i += 3)
                {
                    A.Add(pos[idx[i]] + p.Origin); B.Add(pos[idx[i + 1]] + p.Origin); C.Add(pos[idx[i + 2]] + p.Origin);
                    Part.Add(p.Name);
                }
            }
        }

        public int Count => A.Count;

        /// <summary>Nearest hit along o + t d (d unit), both faces; returns false when nothing within maxT.</summary>
        public bool Ray(Vector3 o, Vector3 d, float maxT, out float t, out string part)
        {
            t = maxT; part = null;
            bool hit = false;
            for (int i = 0; i < A.Count; i++)
            {
                if (RayTri(o, d, A[i], B[i], C[i], out float ti) && ti > 1e-5f && ti < t) { t = ti; part = Part[i]; hit = true; }
            }
            return hit;
        }

        static bool RayTri(Vector3 o, Vector3 d, Vector3 a, Vector3 b, Vector3 c, out float t)
        {
            t = 0f;
            Vector3 e1 = b - a, e2 = c - a, p = Vector3.Cross(d, e2);
            float det = Vector3.Dot(e1, p);
            if (Mathf.Abs(det) < 1e-12f) return false;
            float inv = 1f / det;
            Vector3 s = o - a;
            float u = Vector3.Dot(s, p) * inv;
            if (u < -1e-6f || u > 1f + 1e-6f) return false;
            Vector3 q = Vector3.Cross(s, e1);
            float v = Vector3.Dot(d, q) * inv;
            if (v < -1e-6f || u + v > 1f + 1e-6f) return false;
            t = Vector3.Dot(e2, q) * inv;
            return true;
        }

        /// <summary>Smallest distance from segment p0-p1 to any triangle whose bounds come within `reach` (returns reach + 1 if none).</summary>
        public float SegmentDistance(Vector3 p0, Vector3 p1, float reach, out string part)
        {
            part = null;
            float best = reach + 1f;
            Vector3 mn = Vector3.Min(p0, p1) - Vector3.one * reach, mx = Vector3.Max(p0, p1) + Vector3.one * reach;
            for (int i = 0; i < A.Count; i++)
            {
                Vector3 a = A[i], b = B[i], c = C[i];
                if (Mathf.Max(a.x, Mathf.Max(b.x, c.x)) < mn.x || Mathf.Min(a.x, Mathf.Min(b.x, c.x)) > mx.x) continue;
                if (Mathf.Max(a.z, Mathf.Max(b.z, c.z)) < mn.z || Mathf.Min(a.z, Mathf.Min(b.z, c.z)) > mx.z) continue;
                if (Mathf.Max(a.y, Mathf.Max(b.y, c.y)) < mn.y || Mathf.Min(a.y, Mathf.Min(b.y, c.y)) > mx.y) continue;
                float dd = SegTri(p0, p1, a, b, c);
                if (dd < best) { best = dd; part = Part[i]; }
            }
            return best;
        }

        /// <summary>Distance segment-triangle by dense sampling of the segment (exact point-triangle distance per sample).</summary>
        static float SegTri(Vector3 p0, Vector3 p1, Vector3 a, Vector3 b, Vector3 c)
        {
            int n = (p1 - p0).sqrMagnitude < 1e-8f ? 1 : 24;
            float best = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                Vector3 p = n == 1 ? p0 : Vector3.Lerp(p0, p1, i / (float)(n - 1));
                best = Mathf.Min(best, (p - ClosestOnTri(p, a, b, c)).magnitude);
            }
            return best;
        }

        // Ericson, Real-Time Collision Detection 5.1.5
        static Vector3 ClosestOnTri(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a, ac = c - a, ap = p - a;
            float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0f && d2 <= 0f) return a;
            Vector3 bp = p - b;
            float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0f && d4 <= d3) return b;
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f) return a + ab * (d1 / (d1 - d3));
            Vector3 cp = p - c;
            float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0f && d5 <= d6) return c;
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f) return a + ac * (d2 / (d2 - d6));
            float va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f) return b + (c - b) * ((d4 - d3) / ((d4 - d3) + (d5 - d6)));
            float den = 1f / (va + vb + vc);
            return a + ab * (vb * den) + ac * (vc * den);
        }

        public static KeyShot Shot(string tag) => Array.Find(NocturneConstants.KeyShots, k => k.tag == tag);

        public static QaView View(KeyShot k, int w = 1920, int h = 1080) => new QaView(k.x, k.y, k.z, k.yaw, k.pitch, k.fov, w, h);

        /// <summary>Projected NDC rectangle of world points (x0, y0, x1, y1).</summary>
        public static Vector4 Rect(QaView v, IEnumerable<Vector3> pts)
        {
            double x0 = 9, y0 = 9, x1 = -9, y1 = -9;
            foreach (var p in pts)
            {
                Assert.IsTrue(QaProjection.ProjectPoint(v, new D3(p.x, p.y, p.z), out double nx, out double ny), "point behind the camera: " + p);
                x0 = Math.Min(x0, nx); x1 = Math.Max(x1, nx); y0 = Math.Min(y0, ny); y1 = Math.Max(y1, ny);
            }
            return new Vector4((float)x0, (float)y0, (float)x1, (float)y1);
        }

        public static IEnumerable<Vector3> BoxCorners(Vector3 mn, Vector3 mx)
        {
            for (int i = 0; i < 8; i++)
                yield return new Vector3((i & 1) == 0 ? mn.x : mx.x, (i & 2) == 0 ? mn.y : mx.y, (i & 4) == 0 ? mn.z : mx.z);
        }

        public static string R(Vector4 r) => FormattableString.Invariant($"x {r.x:F3}..{r.z:F3}, y {r.y:F3}..{r.w:F3}");

        /// <summary>The projected rectangle lies inside the spec rectangle grown by tol (NDC).</summary>
        public static void Inside(Vector4 got, double sx0, double sx1, double sy0, double sy1, double tol, string what)
        {
            Debug.Log(FormattableString.Invariant($"[NOC] {what}: {R(got)} (spec x {sx0:F2}..{sx1:F2}, y {sy0:F2}..{sy1:F2})"));
            Assert.GreaterOrEqual(got.x, sx0 - tol, what + " x0"); Assert.LessOrEqual(got.z, sx1 + tol, what + " x1");
            Assert.GreaterOrEqual(got.y, sy0 - tol, what + " y0"); Assert.LessOrEqual(got.w, sy1 + tol, what + " y1");
        }

        /// <summary>Walkability: the camera sphere and the player capsule at the lane edges and the centre never enter a triangle.</summary>
        public void LaneClearance(double s0, double s1, double step, Func<string, bool> capsuleIgnores, out double minCam, out double minCap, out string worst)
        {
            minCam = minCap = 9; worst = "";
            float cam = (float)NocturneConstants.CameraSphere, rad = (float)NocturneConstants.PlayerRadius, eye = (float)NocturneConstants.EyeHeight;
            for (double s = s0; s <= s1 + 1e-9; s += step)
            {
                double h = NocLanes.LaneHalfWidth(s);
                foreach (double lat in new[] { -h, 0.0, h })
                {
                    Vector3 p = NocLanes.CorridorToWorld(s, lat, 0.0);
                    float dc = SegmentDistance(p + Vector3.up * eye, p + Vector3.up * eye, cam + 0.05f, out string pc);
                    if (dc < minCam) { minCam = dc; if (dc < cam) worst = FormattableString.Invariant($"camera at s {s:F2} lat {lat:F2}: {dc:F3} m to {pc}"); }
                    // on a slanted lane edge (the alley flanks, the mouth funnel) the controller is held 0.38 m from the slanted
                    // collider face measured along its normal, so at the sampled lateral = lane(s) the clearance to a wall that
                    // runs parallel to the edge is rad cos(theta): the sample is normalised by cos(theta) of the local edge slope
                    double slope = Math.Abs(NocLanes.LaneHalfWidth(s + 0.1) - NocLanes.LaneHalfWidth(Math.Max(0.0, s - 0.1))) / 0.2;
                    float cosT = (float)(1.0 / Math.Sqrt(1.0 + slope * slope));
                    float dk = SegmentDistance(p + Vector3.up * rad, p + Vector3.up * (1.8f - rad), rad + 0.05f, out string pk);
                    float dn = lat == 0.0 ? dk : dk / cosT;
                    if (dn < minCap && (pk == null || !capsuleIgnores(pk))) { minCap = dn; if (dn < rad - 2e-3f) worst = FormattableString.Invariant($"capsule at s {s:F2} lat {lat:F2}: {dk:F3} m to {pk} (edge slope {slope:F3})"); }
                }
            }
        }
    }

    public class Beat01AlleyTests
    {
        static G1Parts s_parts;
        static G1TestGeo s_geo;

        static G1TestGeo s_walls;

        static G1Parts Parts => s_parts ?? (s_parts = Beat01_Alley.BuildParts());
        /// <summary>AL (wave B): steam plumes, light-pool decals and the mouth haze card are not solid (no collider, transparent, the
        /// steam fades within 2.5 m of the camera): the clearance test measures the solid parts only.</summary>
        static bool NonSolid(string n) => n == "steam" || n == "barSteam" || n == "lightPools" || n == "mouthGlow" || n == "workLampPools";
        static G1TestGeo Geo => s_geo ?? (s_geo = new G1TestGeo(Parts, n => !NonSolid(n)));
        /// <summary>The masses only (walls, roofs, dead end, towers, bar, niche, side doors): the profile and slit measurements must not hit a
        /// 6 mm cable or a fire-escape rod that happens to cross the measuring ray (those are checked by the clearance test).</summary>
        static G1TestGeo Walls => s_walls ?? (s_walls = new G1TestGeo(Parts, n =>
            n.StartsWith("alleyWall") || n.StartsWith("alleyRoofs") || n.StartsWith("deadEnd") || n.StartsWith("tower") || n.StartsWith("vend") || n.StartsWith("bar") ||
            n.StartsWith("door")));

        static double Width(double s, double y)
        {
            var o = new Vector3(0f, (float)y, (float)s);
            Assert.IsTrue(Walls.Ray(o, Vector3.left, 10f, out float tl, out string pl), "no west wall at s " + s);
            Assert.IsTrue(Walls.Ray(o, Vector3.right, 10f, out float tr, out string pr), "no east wall at s " + s);
            return tl + tr;
        }

        [Test]
        public void Widths_MatchTheProfile_At_TheListedS()
        {
            // (s, y of the ray, expected clear width): 14.5.1 Acceptance (1) +-0.02
            var cases = new (double s, double y, double w)[]
            {
                (2, 1.0, 4.4), (13, 1.0, 4.4), (25, 1.0, 4.4),
                (37, 1.0, 6.0), (45, 6.0, 6.0), (55, 1.0, 6.0),
                (66.2, 2.6, 4.6), (68, 1.0, 4.6), (70, 1.0, 4.6),
                (90, 1.0, 4.4), (111, 1.0, 4.4),
                (119.99, 1.0, 5.0),
            };
            foreach (var c in cases)
            {
                double w = Width(c.s, c.y);
                Debug.Log(FormattableString.Invariant($"[NOC] alley clear width at s {c.s:F2} (y {c.y:F1}): {w:F4} m (spec {c.w:F1})"));
                Assert.AreEqual(c.w, w, 0.02, "clear width at s " + c.s);
            }
            Assert.AreEqual(4.4, AlleyLayout.ClearWidth(10), 1e-9);
            Assert.AreEqual(6.0, AlleyLayout.ClearWidth(46), 1e-9);
            Assert.AreEqual(4.6, AlleyLayout.ClearWidth(68), 1e-9);
            Assert.AreEqual(5.0, AlleyLayout.ClearWidth(120), 1e-9);
        }

        [Test]
        public void Podia_Are_22_And_31()
        {
            foreach (var c in new (float x, float z, double h)[] { (-10f, 60f, 22.0), (-20f, 10f, 22.0), (10f, 60f, 31.0), (20f, 100f, 31.0) })
            {
                Assert.IsTrue(Walls.Ray(new Vector3(c.x, 300f, c.z), Vector3.down, 400f, out float t, out string part), "no roof at " + c.x);
                double y = 300.0 - t;
                Debug.Log(FormattableString.Invariant($"[NOC] podium roof at x {c.x:F0} z {c.z:F0}: y {y:F3} ({part})"));
                Assert.AreEqual(c.h, y, 0.05, "podium height at x " + c.x);
            }
        }

        [Test]
        public void KS00_Slit_Is33px()
        {
            var k = G1TestGeo.Shot("KS00_alley_start");
            var eye = new Vector3((float)k.x, (float)k.y, (float)k.z);
            // sweep the yaw in the eye plane: the slit = the yaw interval whose rays reach the mouth (z 120) unobstructed
            double lo = 0, hi = 0;
            for (int side = -1; side <= 1; side += 2)
            {
                double a = 0.0, b = 0.05;     // radians: a reaches, b blocked
                for (int it = 0; it < 40; it++)
                {
                    double m = 0.5 * (a + b);
                    var d = new Vector3((float)(side * Math.Sin(m)), 0f, (float)Math.Cos(m));
                    float tMouth = (float)((AlleyLayout.MouthZ - eye.z) / Math.Cos(m));
                    if (Walls.Ray(eye, d, tMouth, out _, out _)) b = m; else a = m;
                }
                if (side < 0) lo = a; else hi = a;
            }
            var v = G1TestGeo.View(k);
            QaProjection.ProjectDir(v, new D3(-Math.Sin(lo), 0, Math.Cos(lo)), out double x0, out _);
            QaProjection.ProjectDir(v, new D3(Math.Sin(hi), 0, Math.Cos(hi)), out double x1, out _);
            double px = (x1 - x0) * 0.5 * 1920.0;
            Debug.Log(FormattableString.Invariant($"[NOC] KS00 slit: half-angles {lo / NocMath.Deg:F4} / {hi / NocMath.Deg:F4} deg, {px:F2} px at 1080p (spec 33 +-3), {2 * px:F1} px at 4K"));
            Assert.AreEqual(33.0, px, 3.0, "KS00 slit width at 1080p");
            // the slit's bottom: the mouth floor at z 120 projects to y +0.03 (14.5.1)
            QaProjection.ProjectPoint(v, new D3(0, 0, AlleyLayout.MouthZ), out _, out double yb);
            Assert.AreEqual(0.03, yb, 0.01, "slit bottom (mouth floor) y");
        }

        [Test]
        public void KS02_SlitHalfAngles_RoofEdges_Catwalk()
        {
            var k = G1TestGeo.Shot("KS02_alley_slit");
            var eye = new Vector3((float)k.x, (float)k.y, (float)k.z);
            // half-angles from the vertical in the plane z = 88: the largest tilt whose upward ray clears the podium top
            double[] spec = { 6.19, 4.29 };
            for (int side = -1; side <= 1; side += 2)
            {
                double a = 0.0, b = 0.3;
                for (int it = 0; it < 40; it++)
                {
                    double m = 0.5 * (a + b);
                    var d = new Vector3((float)(side * Math.Sin(m)), (float)Math.Cos(m), 0f);
                    if (Walls.Ray(eye, d, 60f, out _, out string part)) b = m; else a = m;
                }
                double deg = a / NocMath.Deg;
                Debug.Log(FormattableString.Invariant($"[NOC] KS02 slit half-angle {(side < 0 ? "west" : "east")}: {deg:F3} deg (spec {spec[(side + 1) / 2]:F2})"));
                Assert.AreEqual(spec[(side + 1) / 2], deg, 0.02, "KS02 half-angle");
            }
            var v = G1TestGeo.View(k);
            var west = new List<Vector3>(); var east = new List<Vector3>();
            for (double z = 88; z <= 117.0; z += 1.0) { west.Add(new Vector3(-(float)AlleyLayout.HalfWidth(z), 22f, (float)z)); east.Add(new Vector3((float)AlleyLayout.HalfWidth(z), 31f, (float)z)); }
            var rw = G1TestGeo.Rect(v, west); var re = G1TestGeo.Rect(v, east);
            Debug.Log("[NOC] KS02 west roof edge (z 88..117): " + G1TestGeo.R(rw) + " (spec x -0.10..-0.07); east: " + G1TestGeo.R(re) + " (spec x +0.06..+0.07)");
            Assert.AreEqual(-0.10, rw.x, 0.015, "west roof edge at the zenith row"); Assert.AreEqual(-0.07, rw.z, 0.01, "west roof edge at the frame bottom");
            Assert.AreEqual(0.07, re.z, 0.01, "east roof edge at the zenith row"); Assert.AreEqual(0.06, re.x, 0.015, "east roof edge at the frame bottom");
            // catwalk: x +-0.21, y -0.93..-0.70 (14.5.1); z 96.0-97.5 (D-029, GB-12): its bottom edge inside the 3 % margin
            float hw = (float)AlleyLayout.HalfWidth(96.8);
            var cw = G1TestGeo.Rect(v, G1TestGeo.BoxCorners(new Vector3(-hw, (float)AlleyLayout.CatwalkY0, (float)AlleyFixtures.CatwalkZ0),
                new Vector3(hw, (float)AlleyLayout.CatwalkY1, (float)AlleyFixtures.CatwalkZ1)));
            G1TestGeo.Inside(cw, -0.21, 0.21, -0.93, -0.70, 0.02, "KS02 catwalk");
            Assert.Greater(cw.y, -0.94, "catwalk bottom edge inside the 3 % margin (GB-12)");
            Assert.Greater(cw.z, 0.18, "catwalk spans the slit");
            // the west tower flank (x -30) enters the slit near the zenith (the 14.5.1 'tower flank in the slit')
            var fl = G1TestGeo.Rect(v, new[] { new Vector3(-30f, 300f, 110f), new Vector3(-30f, 400f, 100f), new Vector3(-30f, 550f, 90f) });
            Debug.Log("[NOC] KS02 west tower flank (x -30, y 300-550): " + G1TestGeo.R(fl) + " (spec x -0.20..-0.10, y +0.58..+0.66)");
            Assert.Greater(fl.y, 0.55, "flank near the zenith row");
        }

        [Test]
        public void KS01_Window_Sign_Counter()
        {
            var v = G1TestGeo.View(G1TestGeo.Shot("KS01_alley_noodle"));
            float xw = -(float)AlleyLayout.HalfWidth(45);
            var win = G1TestGeo.Rect(v, G1TestGeo.BoxCorners(new Vector3(xw, 1.0f, 43f), new Vector3(xw, 2.3f, 47f)));
            G1TestGeo.Inside(win, -0.15, -0.02, -0.27, 0.02, 0.015, "KS01 bar window");
            var sign = G1TestGeo.Rect(v, G1TestGeo.BoxCorners(new Vector3(xw + 0.06f, 3.0f, 42f), new Vector3(xw + 0.06f, 4.4f, 48f)));
            G1TestGeo.Inside(sign, -0.21, 0.04, 0.06, 0.57, 0.02, "KS01 sign");
            var ctr = G1TestGeo.Rect(v, G1TestGeo.BoxCorners(new Vector3(xw - 0.03f, 0f, 43f), new Vector3(xw - 0.03f, 1.0f, 47f)));
            G1TestGeo.Inside(ctr, -0.30, 0.10, -0.51, -0.22, 0.02, "KS01 counter front");
        }

        [Test]
        public void Meshes_Valid_And_UnderBudget()
        {
            long tris = 0;
            foreach (var p in Parts.Parts)
            {
                if (p.Mb.VertexCount == 0) continue;
                var r = MeshValidation.Validate(p.Mb, p.Name);
                Assert.IsTrue(r.Ok, r.ToString());
                tris += p.Mb.TriangleCount;
                foreach (var q in p.Mb.Positions)
                    Assert.LessOrEqual(new Vector2(q.x, q.z).magnitude, 200f, p.Name + ": vertex beyond 200 m of its chunk origin (7.5)");
            }
            Debug.Log(FormattableString.Invariant($"[NOC] beat 1: {Parts.Parts.Count} parts, {tris} triangles (budget 480 000), {Parts.Signs.Count} signs, {Parts.Lights.Count} lights"));
            Assert.LessOrEqual(tris, 480000);
            Assert.LessOrEqual(Parts.Lights.Count, 20, "14.2.3 beat 1: at most 20 realtime local lights");
        }

        [Test]
        public void Walkability_CameraAndCapsuleClear()
        {
            Geo.LaneClearance(0.0, 120.0, 0.25, n => false, out double cam, out double cap, out string worst);
            Debug.Log(FormattableString.Invariant($"[NOC] beat 1 clearance: camera sphere min {cam:F3} m (>= 0.2), capsule min {cap:F3} m (>= 0.35) {worst}"));
            Assert.GreaterOrEqual(cam, NocturneConstants.CameraSphere, worst);
            Assert.GreaterOrEqual(cap, NocturneConstants.PlayerRadius - 2e-3, worst);
        }

        /// <summary>AL (wave B): the 14.5.1 kit counts and the INT-06 / DREAD 6 content on the built parts.</summary>
        [Test]
        public void Kit_Windows_Rooms_Counts()
        {
            var p = Parts;
            // 140 cables of 12 segments x 6 sides (2 triangles each)
            Assert.AreEqual(AlleyLayout.CableCount * 12 * 6 * 2, p.Find("cables").Mb.TriangleCount, "140 catenary cables (14.5.1)");
            for (int i = 0; i < AlleyFixtures.AcCount; i++)
            {
                AlleyFixtures.Ac(i, out int side, out float z, out float y, out _);
                Assert.AreNotEqual(0, side, "AC unit " + i + " placed");
                Assert.Greater(y, 5.0f, "AC unit above the doors and awnings");
                Assert.IsFalse(z > AlleyFixtures.ClearZ0 && z < AlleyFixtures.ClearZ1, "AC unit outside the KS02 clear band");
            }
            // every grid window is a framed window now (INT-06), about 45 % lit
            Assert.Greater(AlleyWindows.Built, 200, "framed windows");
            double lit = AlleyWindows.Lit / (double)AlleyWindows.Built;
            Debug.Log(FormattableString.Invariant($"[NOC] beat 1 windows: {AlleyWindows.Built} framed, {AlleyWindows.Lit} lit ({lit:P1}); parts {p.Parts.Count}"));
            Assert.AreEqual(AlleyLayout.WinLit, lit, 0.1, "lit share");
            Assert.Greater(p.Find("alleyWindowTrim").Mb.TriangleCount, 0, "sills and heads");
            // the noodle bar room and the two empty rooms are modelled; steam rises at the vents and the pots
            Assert.Greater(p.Find("barRoom").Mb.TriangleCount, 1000, "noodle bar interior");
            for (int r = 0; r < AlleyRooms.Count; r++) Assert.Greater(p.Find("roomInterior" + r).Mb.TriangleCount, 200, "empty room " + r);
            Assert.AreEqual(4 * 2 * 2, p.Find("steam").Mb.TriangleCount, "four vents, two plumes each");
            Assert.AreEqual(3 * 2, p.Find("barSteam").Mb.TriangleCount, "three pots");
            Assert.Greater(p.Find("lightPools").Mb.TriangleCount, 40, "pools of every lamp (INT-21)");
        }

        /// <summary>Fix wave, D-039 (6): the C# twin of NocAlleyPool's spot mode, E = sum I N.L cone / (d^2 + 0.25), on a wall point.</summary>
        static float WorkWash(Vector3 p)
        {
            var n = new Vector3(p.x > 0f ? -1f : 1f, 0f, 0f);
            float cosOut = Mathf.Cos(AlleyFixtures.WorkLampOuterDeg * Mathf.Deg2Rad), cosIn = Mathf.Cos(AlleyFixtures.WorkLampInnerDeg * Mathf.Deg2Rad);
            float e = 0f;
            for (int k = 0; k < 2; k++)
            {
                AlleyFixtures.WorkLamp(k, out Vector3 pos, out Vector3 aim);
                Vector3 L = pos - p;
                float d2 = L.sqrMagnitude;
                L /= Mathf.Sqrt(d2);
                float c = Mathf.Clamp01((Vector3.Dot(-L, aim) - cosOut) / (cosIn - cosOut));
                c = c * c * (3f - 2f * c);
                e += AlleyFixtures.WorkLampIntensity * Mathf.Max(0f, Vector3.Dot(n, L)) * c / (d2 + 0.25f);
            }
            return e;
        }

        [Test]
        public void WorkLamps_WashTheKS02Walls_NotTheCatwalk()
        {
            var p = Parts;
            Assert.AreEqual(4, p.Find("workLampPools").Mb.TriangleCount, "one wash panel per wall");
            Assert.Greater(p.Find("workLampLens").Mb.TriangleCount, 0, "lit lenses");
            Assert.Greater(p.Find("workLampFixture").Mb.TriangleCount, 0, "housings, guards, yokes, hangers");
            float hw = (float)AlleyLayout.HalfWidth(89.6);
            float lum = AlleyFixtures.WorkLampPoolColour().grayscale;
            for (int k = 0; k < 2; k++)
            {
                AlleyFixtures.WorkLamp(k, out Vector3 pos, out Vector3 aim);
                Assert.Greater(pos.y, 8.5f, "under the catwalk, far above the walk");
                // the aim meets the opposite wall inside the KS02 texture region (y 3.3-4.8, z 88.7-90.6 beside the eye)
                float side = k == 0 ? 1f : -1f;
                float t = (side * hw - pos.x) / aim.x;
                Vector3 hit = pos + aim * t;
                Assert.That(hit.y, Is.InRange(2.5f, 4.8f), "lamp " + k + " aim height on the opposite wall");
                Assert.That(hit.z, Is.InRange(88.5f, 91.0f), "lamp " + k + " aim z on the opposite wall");
            }
            foreach (float side in new[] { -1f, 1f })
            {
                // x ~1.7 at the KS02 regions' centre (AL_f01: I 350 flattened the walls to a grey wash), none beside the catwalk (the slit stays dark)
                float mid = 1f + lum * WorkWash(new Vector3(side * hw, 4.0f, 89.6f));
                float high = 1f + lum * WorkWash(new Vector3(side * hw, 9.5f, 96.5f));
                float far = 1f + lum * WorkWash(new Vector3(side * hw, 4.0f, 80.0f));
                Debug.Log(FormattableString.Invariant($"[NOC] work wash side {side}: x{mid:0.00} at the KS02 region, x{high:0.00} at the catwalk, x{far:0.00} at z 80"));
                Assert.That(mid, Is.InRange(1.4f, 2.1f), "the wash at the KS02 region");
                Assert.Less(high, 1.05f, "no wash on the walls beside the catwalk");
                Assert.Less(far, 1.6f, "the wash fades before z 80");
            }
            Assert.LessOrEqual(Parts.Lights.Count, 20, "baked, not a 21st realtime light (14.2.3, D-039 (6))");
        }

        [Test]
        public void FixWave_EnclosedRooms_Plaster_SteamOpacity()
        {
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(Megastructure.EditorTools.NocPaths.LibraryAsset);
            var room = AlleyMaterials.Make(G1Mat.AlleyRoomB, lib);
            if (room == null) Assert.Ignore("Megastructure/NocAlleyRoom not found");
            Assert.AreEqual(1f, room.GetFloat("_Interior"), "the empty rooms take their own lamp only (C37)");
            Assert.AreEqual(AlleyMaterials.InteriorAmbient, room.GetFloat("_InteriorAmb"), 1e-6f);
            if (lib != null && lib.Plaster != null && lib.Plaster.GetTexture("_AlbedoTex2") != null)
            {
                Assert.AreEqual(1f, room.GetFloat("_WallTexOn"), "the CC0 plaster on the walls");
                Assert.AreSame(lib.Plaster.GetTexture("_AlbedoTex2"), room.GetTexture("_WallTex"));
            }
            var bar = AlleyMaterials.Make(G1Mat.AlleyRoomBar, lib);
            Assert.AreEqual(0f, bar.GetFloat("_Interior"), "the noodle bar keeps the template's lighting");
            Assert.AreEqual(0f, bar.GetFloat("_WallTexOn"));
            var steam = AlleyMaterials.Make(G1Mat.Steam, lib);
            Assert.AreEqual(AlleyMaterials.VentOpacity, steam.GetFloat("_Opacity"), 1e-6f, "C38");
            Assert.Greater(steam.GetFloat("_Scatter"), 0.5f, "the plumes in-scatter the lamps");
            var wash = AlleyMaterials.Make(G1Mat.AlleyWorkPool, lib);
            Assert.AreEqual(1f, wash.GetFloat("_Spot"));
            Assert.AreEqual(AlleyFixtures.WorkLampIntensity, wash.GetVector("_SpotA").w, 1e-3f);
        }

        [Test]
        public void WetRegistry_SourcesAndWalls()
        {
            NocWetRegistry.Clear();
            try
            {
                Beat01_Alley.RegisterWet();
                Beat02_Boulevard.RegisterWet();
                int max = NocWetRegistry.MaxActive();
                Debug.Log(FormattableString.Invariant($"[NOC] beats 1-2 wet sources: {NocWetRegistry.TotalSources} total, at most {max} active at any s (cap 32)"));
                Assert.LessOrEqual(max, 31, "beats 1-2 stay under the cap of 32 (Look v2: + the west door bulkheads)");
                for (double s = 0; s <= 400; s += 2) { NocWetRegistry.Collect(s); Assert.LessOrEqual(NocWetRegistry.ActiveWalls, 4); Assert.AreEqual(0, NocWetRegistry.Dropped); }
                NocWetRegistry.Collect(60);
                Assert.AreEqual(2, NocWetRegistry.ActiveWalls, "the alley walls x -+2.2");
            }
            finally { NocWetRegistry.Clear(); }
        }
    }
}
