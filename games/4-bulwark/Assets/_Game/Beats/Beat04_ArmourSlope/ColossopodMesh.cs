// ColossopodMesh: the 40 m armour unit of the reef in its unit frame (ReefLayout.ArmDir, ArmLength, ArmR0/R1, HubRadius).
// LOD0 (belt, islets and every unit with x < 400: about 1600 triangles) and LOD1 (the far shoal: about 300 triangles), both
// smooth-shaded truncated cones with a 0.35 m 45-degree chamfer round the flat end face and a hub sphere rounding the crotches.
// Vertex colours for BwArmour: R = AO (0.55 in the crotch rising to 1 at 40 % of the arm), G = edge wear (the chamfer),
// B = gauge bands (the banded arm of a gauge unit, from 40 % of the arm to its end; BwArmour paints SafetyOchre stripes).
// Vertex A = a per-unit tone (0..255, 128 neutral; BwArmour varies the unit's albedo by it: no two units cast alike).
// AppendClipped copies a unit into a world-space chunk and drops every triangle whose three vertices lie below
// ReefLayout.ClipY (the sea hides them): an awash unit costs only its visible tip, so the far shoal needs no separate horn mesh
// and units and "horns" come from one shape (T7 crossover by construction).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Beats
{
    public static class ColossopodMesh
    {
        /// <summary>Builds one unit in its own frame. gaugeArm = index of the banded arm (-1 none); stripLocal = the direction
        /// (unit frame) the painted tide-gauge strip of that arm faces (the road, west, for a walker to read it).</summary>
        public static MeshBuilder Build(int lod, int gaugeArm) => Build(lod, gaugeArm, Vector3.zero);

        /// <summary>Gauge strip: within this angle of the strip direction, over the last GaugeStripLength m of the arm.</summary>
        public const float GaugeStripHalfDeg = 24f, GaugeStripLength = 9f;

        public static MeshBuilder Build(int lod, int gaugeArm, Vector3 stripLocal)
        {
            var mb = new MeshBuilder();
            int sides = lod == 0 ? 20 : 8;
            float[] ts = lod == 0
                ? new[] { 0f, 4f, 8f, 12f, 16f, 20f, ReefLayout.ArmLength - ReefLayout.EndChamfer }
                : new[] { 0f, 11f, ReefLayout.ArmLength - ReefLayout.EndChamfer };
            for (int a = 0; a < 4; a++) Arm(mb, ReefLayout.ArmDir[a], sides, ts, a == gaugeArm, stripLocal);
            int lon = lod == 0 ? 20 : 8, lat = lod == 0 ? 10 : 4;
            Hub(mb, ReefLayout.HubRadius, lon, lat);
            return mb;
        }

        static void Arm(MeshBuilder mb, Vector3 dir, int sides, float[] ts, bool gauge, Vector3 stripLocal)
        {
            Vector3 h = Mathf.Abs(dir.y) < 0.9f ? Vector3.up : Vector3.right;
            Vector3 u = Vector3.Cross(h, dir).normalized, v = Vector3.Cross(dir, u);
            float L = ReefLayout.ArmLength, c = ReefLayout.EndChamfer;
            float slope = (ReefLayout.ArmR0 - ReefLayout.ArmR1) / L;          // radius loss per metre: the normal tilts forward
            int first = mb.VertexCount;
            // the strip direction projected into the arm's cross-section (no strip when it is along the arm)
            Vector3 sd = stripLocal - dir * Vector3.Dot(stripLocal, dir);
            bool strip = gauge && sd.sqrMagnitude > 1e-4f;
            if (strip) sd.Normalize();
            float cosStrip = Mathf.Cos(GaugeStripHalfDeg * Mathf.Deg2Rad);
            var tsx = new List<float>(ts);
            if (strip) { tsx.Add(L - c - GaugeStripLength); tsx.Sort(); }   // a ring where the strip starts
            ts = tsx.ToArray();
            for (int k = 0; k < ts.Length; k++)
            {
                float t = ts[k];
                float r = ReefLayout.ArmRadiusAt(t);
                byte ao = (byte)Mathf.RoundToInt(255f * Mathf.Lerp(0.55f, 1f, Mathf.SmoothStep(0f, 1f, t / (0.4f * L))));
                for (int j = 0; j < sides; j++)
                {
                    float ph = 2f * Mathf.PI * j / sides;
                    Vector3 rad = u * Mathf.Cos(ph) + v * Mathf.Sin(ph);
                    bool onStrip = strip && t >= L - c - GaugeStripLength - 1e-3f && Vector3.Dot(rad, sd) >= cosStrip;
                    mb.Color = new Color32(ao, 0, (byte)(onStrip ? 255 : 0), 255);
                    Vector3 n = (rad + dir * slope).normalized;
                    mb.Vertex(dir * t + rad * r, n, new Vector2(t, ph));
                }
            }
            for (int k = 0; k + 1 < ts.Length; k++)
                for (int j = 0; j < sides; j++)
                {
                    int jn = (j + 1) % sides;
                    int A = first + k * sides + j, B = first + (k + 1) * sides + j, C = first + (k + 1) * sides + jn, D = first + k * sides + jn;
                    mb.Quad(A, D, C, B);
                }
            // chamfer ring: (t = L - c, r1) -> (t = L, r1 - c), 45 deg, edge wear
            int ring0 = mb.VertexCount;
            // (it starts on the cone's last ring exactly: r(L - c), so no annular slit opens between them)
            float ra = ReefLayout.ArmRadiusAt(L - c);
            for (int k = 0; k < 2; k++)
            {
                float t = k == 0 ? L - c : L;
                float r = k == 0 ? ra : ra - c;
                mb.Color = new Color32(255, (byte)(k == 0 ? 160 : 255), 0, 255);
                for (int j = 0; j < sides; j++)
                {
                    float ph = 2f * Mathf.PI * j / sides;
                    Vector3 rad = u * Mathf.Cos(ph) + v * Mathf.Sin(ph);
                    Vector3 n = (rad + dir).normalized;
                    mb.Vertex(dir * t + rad * r, n, new Vector2(t, ph));
                }
            }
            for (int j = 0; j < sides; j++)
            {
                int jn = (j + 1) % sides;
                mb.Quad(ring0 + j, ring0 + jn, ring0 + sides + jn, ring0 + sides + j);
            }
            // end face (flat disc facing dir), worn rim
            mb.Color = new Color32(255, 90, 0, 255);
            int cen = mb.Vertex(dir * L, dir, new Vector2(L, 0f));
            int rim = mb.VertexCount;
            for (int j = 0; j < sides; j++)
            {
                float ph = 2f * Mathf.PI * j / sides;
                Vector3 rad = u * Mathf.Cos(ph) + v * Mathf.Sin(ph);
                mb.Vertex(dir * L + rad * (ra - c), dir, new Vector2(L, ph));
            }
            // Cross(p_j - c, p_j+1 - c) is along Cross(u, v) = dir: (c, p_j, p_j+1) is clockwise seen from the front (+dir)
            for (int j = 0; j < sides; j++) mb.Tri(cen, rim + j, rim + (j + 1) % sides);
            mb.Color = new Color32(255, 0, 0, 255);
        }

        static void Hub(MeshBuilder mb, float r, int lon, int lat)
        {
            mb.Color = new Color32(140, 0, 0, 255);
            MeshKit.Sphere(mb, Vector3.zero, r, lon, lat);
            mb.Color = new Color32(255, 0, 0, 255);
        }

        /// <summary>Appends src transformed by m into dst, keeping only the triangles with a vertex above clipY (world).
        /// Returns the triangles appended.</summary>
        public static int AppendClipped(MeshBuilder dst, MeshBuilder src, Matrix4x4 m, float clipY, List<int> remap, byte tone = 128)
        {
            int nv = src.Positions.Count;
            remap.Clear();
            for (int i = 0; i < nv; i++) remap.Add(-1);
            var world = new Vector3[nv];
            for (int i = 0; i < nv; i++) world[i] = m.MultiplyPoint3x4(src.Positions[i]);
            int tris = 0;
            var idx = src.Indices;
            for (int t = 0; t + 2 < idx.Count; t += 3)
            {
                int a = idx[t], b = idx[t + 1], c = idx[t + 2];
                if (world[a].y < clipY && world[b].y < clipY && world[c].y < clipY) continue;
                // the hub sphere's pole triangles have a 1e-6 m edge in the unit frame (sin(pi) is not 0 in float); at world
                // coordinates of 100-2600 m float rounding collapses it (the smoke run's mesh check: degenerate triangles)
                if (Vector3.Cross(world[b] - world[a], world[c] - world[a]).sqrMagnitude < 1e-10f) continue;
                int ra = Map(dst, src, m, world, remap, a, tone), rb = Map(dst, src, m, world, remap, b, tone), rc = Map(dst, src, m, world, remap, c, tone);
                dst.Indices.Add(ra); dst.Indices.Add(rb); dst.Indices.Add(rc);
                tris++;
            }
            return tris;
        }

        static int Map(MeshBuilder dst, MeshBuilder src, Matrix4x4 m, Vector3[] world, List<int> remap, int i, byte tone)
        {
            if (remap[i] >= 0) return remap[i];
            dst.Positions.Add(world[i]);
            Vector3 n = m.MultiplyVector(src.Normals[i]);
            dst.Normals.Add(n.sqrMagnitude > 1e-20f ? n.normalized : Vector3.up);
            var col = src.Colors[i];
            col.a = tone;                                   // per-unit tone (BwArmour: vertex A)
            dst.Colors.Add(col);
            dst.UV0.Add(src.UV0[i]);
            remap[i] = dst.Positions.Count - 1;
            return remap[i];
        }
    }
}
