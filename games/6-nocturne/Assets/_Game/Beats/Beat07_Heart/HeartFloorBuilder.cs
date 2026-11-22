// HeartFloorBuilder: the walking surface of beat 7 (MASTER_PROMPT 14.4 "Heart", 14.5.7: plaza circle R 400 about S = (1 330, 388),
// NocWet plaza stone; geometry agreement 6: G4 floors everything north of the terrace, including the C3 corner and the gap between
// the promenade and the circle). One 2 m slab whose outline is the circle joined to the fill x 914 .. circle, z 360 (the arcade's
// north face) .. 520 (the wall's north end): the union is star-shaped about S with a monotone outline angle, so a fan from S
// triangulates it exactly (no T-junction where the fill meets the circle). Top y 0, bottom y -2, outward side walls. Vertices
// lie up to 431 m from the mesh origin S (float step 3e-5 m; world coordinates < 2 km keep the fixed origin of 14.4).
// HT (INT-12, Phase 4 wave A): the brass inlay (17 ring lines 0.12 m every 20 m from R 60 to 380, 48 radial seams every 7.5 deg
// beyond R 58, faded below 6 px pitch) is a second draw of the slab's own top fan (TopFan: the identical vertices and triangles
// under the identical transform) with Megastructure/NocInlay, a transparent lit overlay that passes ZTest LEqual exactly on the
// floor: no co-planar offset, no z-fighting, NocWet's mirror untouched between the lines. InlayCoverage is its C# twin.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class HeartFloorBuilder
    {
        public static readonly Vector3 Origin = new Vector3((float)HeartLayout.SX, 0f, (float)HeartLayout.SZ);

        /// <summary>The outline (world x, z), counter-clockwise seen from above: the circle from its west point at z 360 the long way
        /// round (south, east, north) to its west point at z 520, then the fill corners (914, 520) and (914, 360).</summary>
        public static List<Vector2> Outline()
        {
            double R = HeartLayout.PlazaR, sx = HeartLayout.SX, sz = HeartLayout.SZ;
            double a0 = Math.PI - Math.Asin((HeartLayout.FillZ0 - sz) / R);          // z 360 on the west side (184.01 deg)
            double a1 = Math.PI - Math.Asin((HeartLayout.FillZ1 - sz) / R) + 2.0 * Math.PI;   // z 520 on the west side (520.73 deg)
            int n = (int)Math.Ceiling((a1 - a0) / (2.0 * Math.PI) * HeartLayout.PlazaSegments);
            var pts = new List<Vector2>(n + 4);
            for (int i = 0; i <= n; i++)
            {
                double a = a0 + (a1 - a0) * i / n;
                pts.Add(new Vector2((float)(sx + R * Math.Cos(a)), (float)(sz + R * Math.Sin(a))));
            }
            pts.Add(new Vector2((float)HeartLayout.FillX0, (float)HeartLayout.FillZ1));
            pts.Add(new Vector2((float)HeartLayout.FillX0, (float)HeartLayout.FillZ0));
            return pts;
        }

        /// <summary>The slab mesh (relative to Origin): fan top, fan bottom, outward side walls.</summary>
        public static MeshBuilder Slab()
        {
            var outline = Outline();
            var mb = new MeshBuilder();
            float t = (float)HeartLayout.FloorThickness;
            int n = outline.Count;
            Vector3 o = Origin;
            TopFan(mb, outline);
            // bottom (normal -y)
            int cBot = mb.Vertex(new Vector3(0f, -t, 0f), Vector3.down, new Vector2(o.x, o.z));
            int first = mb.VertexCount;
            for (int i = 0; i < n; i++) mb.Vertex(new Vector3(outline[i].x - o.x, -t, outline[i].y - o.z), Vector3.down, outline[i]);
            for (int i = 0; i < n; i++) mb.Tri(cBot, first + i, first + (i + 1) % n);
            // side walls, outward = the edge direction turned right (clockwise) for a CCW outline
            for (int i = 0; i < n; i++)
            {
                Vector2 p = outline[i], q = outline[(i + 1) % n];
                Vector2 e = q - p;
                var outward = new Vector3(e.y, 0f, -e.x);
                Vector3 a = new Vector3(p.x - o.x, 0f, p.y - o.z), b = new Vector3(q.x - o.x, 0f, q.y - o.z);
                G4Kit.OrientedQuad(mb, a, b, b + Vector3.down * t, a + Vector3.down * t, outward);
            }
            return mb;
        }

        /// <summary>The top of the slab (normal +y): MeshBuilder front faces are where cross(b - a, c - a) points; a CCW outline seen
        /// from above gives cross(v_i - c, v_i+1 - c) pointing down, so the fan is (c, v_i+1, v_i). Slab and the inlay overlay both
        /// call it first on an empty builder, so their top vertices and triangles are identical.</summary>
        public static void TopFan(MeshBuilder mb, List<Vector2> outline)
        {
            Vector3 o = Origin;
            int n = outline.Count;
            int cTop = mb.Vertex(Vector3.zero, Vector3.up, new Vector2(o.x, o.z));
            int first = mb.VertexCount;
            for (int i = 0; i < n; i++) mb.Vertex(new Vector3(outline[i].x - o.x, 0f, outline[i].y - o.z), Vector3.up, outline[i]);
            for (int i = 0; i < n; i++) mb.Tri(cTop, first + (i + 1) % n, first + i);
        }

        /// <summary>The inlay overlay's mesh: the slab's top fan alone.</summary>
        public static MeshBuilder InlayFan()
        {
            var mb = new MeshBuilder();
            TopFan(mb, Outline());
            return mb;
        }

        /// <summary>C# twin of NocInlay's pattern at the world point (x, z) for metres-per-pixel footprints fwR (radial) and fwT
        /// (tangential): x = total coverage, y = the seams' share (Beat07HeartTests).</summary>
        public static Vector2 InlayCoverage(double x, double z, double fwR, double fwT)
        {
            double dx = x - HeartLayout.SX, dz = z - HeartLayout.SZ;
            double r = Math.Max(Math.Sqrt(dx * dx + dz * dz), 1e-3);
            fwR = Math.Max(fwR, 1e-5); fwT = Math.Max(fwT, 1e-5);
            double pitch = HeartLayout.InlayRingPitch, w = HeartLayout.InlayRingW;
            double rk = Math.Round(r / pitch, MidpointRounding.AwayFromZero) * pitch;
            double inRange = rk >= HeartLayout.InlayRing0 - 1e-3 && rk <= HeartLayout.InlayRing1 + 1e-3 ? 1.0 : 0.0;
            double aRing = Cover(r - rk, fwR, -0.5 * w, 0.5 * w) * Smooth(HeartLayout.InlayFadePx0, HeartLayout.InlayFadePx1, pitch / fwR) * inRange;
            double dth = 2.0 * Math.PI / HeartLayout.InlaySeams, ang = Math.Atan2(dz, dx);
            double arc = (ang - Math.Round(ang / dth, MidpointRounding.AwayFromZero) * dth) * r;
            double ws = HeartLayout.InlaySeamW;
            double aSeam = Cover(arc, fwT, -0.5 * ws, 0.5 * ws) * Smooth(HeartLayout.InlayFadePx0, HeartLayout.InlayFadePx1, r * dth / fwT)
                * (r >= HeartLayout.InlaySeamR0 ? 1.0 : 0.0);
            aSeam *= 1.0 - aRing;
            return new Vector2((float)(aRing + aSeam), (float)aSeam);
        }

        static double Cover(double x, double fw, double x0, double x1) => Math.Max(0.0, Math.Min(1.0, (Math.Min(x + 0.5 * fw, x1) - Math.Max(x - 0.5 * fw, x0)) / Math.Max(fw, 1e-6)));
        static double Smooth(double e0, double e1, double x) { double u = Math.Max(0.0, Math.Min(1.0, (x - e0) / (e1 - e0))); return u * u * (3.0 - 2.0 * u); }

        /// <summary>True when (x, z) lies on this floor (circle or fill); used by the coverage tests.</summary>
        public static bool Covers(double x, double z)
        {
            double dx = x - HeartLayout.SX, dz = z - HeartLayout.SZ;
            double R = HeartLayout.PlazaR;
            if (dx * dx + dz * dz <= R * R) return true;
            if (z < HeartLayout.FillZ0 || z > HeartLayout.FillZ1 || x < HeartLayout.FillX0) return false;
            double xw = HeartLayout.SX - Math.Sqrt(Math.Max(0.0, R * R - dz * dz));
            return x <= xw;
        }

        public static void Build(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            double sMid = 0.5 * (NocturneConstants.BeatBoundaries[6] + NocturneConstants.BeatBoundaries[7]);
            G4Kit.Add(ctx, root, "heartFloor", Slab(), lib != null ? lib.FloorFor(sMid) : null, Origin);
            // HT (INT-12): the brass inlay, the same top fan drawn again with NocInlay (queue 2501, after the floor)
            Material inlay = lib != null ? G4Kit.FromShader("Megastructure/NocInlay", "Beat07_Inlay") : null;
            if (inlay == null) return;
            Color b = HeartLayout.InlayBrass;
            inlay.SetVector("_Centre", new Vector4((float)HeartLayout.SX, 0f, (float)HeartLayout.SZ, 0f));
            inlay.SetVector("_Ring", new Vector4((float)HeartLayout.InlayRingPitch, (float)HeartLayout.InlayRingW, (float)HeartLayout.InlayRing0, (float)HeartLayout.InlayRing1));
            inlay.SetVector("_Seam", new Vector4(HeartLayout.InlaySeams, (float)HeartLayout.InlaySeamW, (float)HeartLayout.InlaySeamR0, (float)HeartLayout.InlaySeamTint));
            inlay.SetVector("_FadePx", new Vector4((float)HeartLayout.InlayFadePx0, (float)HeartLayout.InlayFadePx1, 0f, 0f));
            inlay.SetVector("_Brass", new Vector4(b.r, b.g, b.b, 1f));
            inlay.SetFloat("_Smooth", (float)HeartLayout.InlaySmooth);
            G4Kit.Add(ctx, root, "heartFloorInlay", InlayFan(), inlay, Origin);
        }
    }
}
