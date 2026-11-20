// HeartFloorBuilder: the walking surface of beat 7 (MASTER_PROMPT 14.4 "Heart", 14.5.7: plaza circle R 400 about S = (1 330, 388),
// NocWet plaza stone; geometry agreement 6: G4 floors everything north of the terrace, including the C3 corner and the gap between
// the promenade and the circle). One 2 m slab whose outline is the circle joined to the fill x 914 .. circle, z 360 (the arcade's
// north face) .. 520 (the wall's north end): the union is star-shaped about S with a monotone outline angle, so a fan from S
// triangulates it exactly (no T-junction where the fill meets the circle). Top y 0, bottom y -2, outward side walls. Ring lines
// and radial seams are Phase 4. Vertices lie up to 431 m from the mesh origin S (float step 3e-5 m; world coordinates < 2 km
// keep the fixed origin of 14.4).
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
            // top (normal +y): MeshBuilder front faces are where cross(b - a, c - a) points; a CCW outline seen from above gives
            // cross(v_i - c, v_i+1 - c) pointing down, so the fan is (c, v_i+1, v_i)
            int cTop = mb.Vertex(Vector3.zero, Vector3.up, new Vector2(o.x, o.z));
            int first = mb.VertexCount;
            for (int i = 0; i < n; i++) mb.Vertex(new Vector3(outline[i].x - o.x, 0f, outline[i].y - o.z), Vector3.up, outline[i]);
            for (int i = 0; i < n; i++) mb.Tri(cTop, first + (i + 1) % n, first + i);
            // bottom (normal -y)
            int cBot = mb.Vertex(new Vector3(0f, -t, 0f), Vector3.down, new Vector2(o.x, o.z));
            first = mb.VertexCount;
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
        }
    }
}
