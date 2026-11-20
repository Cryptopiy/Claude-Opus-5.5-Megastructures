// NotchEastBlock: the east end of the garden notch closed by a building (GE, D-029 GB-03 / GB-06 / GB-14; GateDims part 3). Before,
// the notch was open toward the Heart (x 914, z 115..275) and the void between the garden and the plaza disc read as a floating
// sheet (GB-14); every pose of beats 2-6 that looked through the notch saw the koi, the spire and the Heart sky (GB-03).
//   footprint: from the garden's east edge x 914 to the arc R 366 about the spire (concentric with the plaza; the koi's parts stay
//   inside R 356), z 115..275; y -360..560 (face B's level-0 depth), except z 236..275 where the block stands on the arcade's roof (y 38): the arcade
//   (x 914..990, z 236..360, 38 m, G4) keeps its shopfronts and the block rises behind its parapet line;
//   faces: NocFacade facade mode, u = metres along each face (the west face is the courtyard's east wall seen from the garden, the
//   arc face fronts the plaza's west rim, the south and north faces close the notch's corners); the top cap at the face top (roof
//   mode), the bottom cap at y -360 (under the plaza and the garden); the plaza slab runs into the arc face (the disc's rim lies inside the block);
//   concrete ledges 0.8 m proud on the west and south faces at y 38, 104, 262, 360, 456 (clear of every lane portal);
//   one 30 m lane tunnel per boulevard lane in the west face (lit 4 m mouth, dark back wall), framed like the lintel's: the lanes'
//   D-019 ramps end inside the block (x 1 015.9; past the arc face their alpha is below 0.05).
// Where the block touches another mass its faces are kept (closed volume) but never seen: the west face below y 0 lies on face B's
// level-0 back face (x 914, y -360..-0.5) and the terrace slab's east face; the lower north face at z 236 lies on the arcade's south
// face for y -40..38 and is emitted only below y -40.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class NotchEastBlock
    {
        public const uint IdWest = 0x6300u, IdArc = 0x6301u, IdSouth = 0x6302u, IdNorth = 0x6303u, IdTop = 0x6304u;
        /// <summary>Beat root: beat 6 (the block is passed at the garden's end and stands behind the promenade's south end).</summary>
        public const int Beat = 5;
        /// <summary>Lowest y of the west face (the block's bottom).</summary>
        public const double WestLow = GateDims.EbY0;
        /// <summary>The arcade's bottom (SignsLayout.ArcY0): below it the lower north face at z 236 is exposed.</summary>
        public static double ArcadeBottom => SignsLayout.ArcY0;

        public static void Build(List<ReliefPart> parts)
        {
            double x0 = GateDims.EbX0, z0 = GateDims.EbZ0, z1 = GateDims.EbZ1, zA = GateDims.EbZArcade, yA = GateDims.EbArcadeTop;
            double yb = GateDims.EbY0, top = EdgeDims.FaceTop;
            var origin = ReliefKit.P(980.0, 0.0, 195.0);
            var fac = new ReliefPart("NotchEastBlock_facade", ReliefMat.Facade, origin, Beat);
            var conc = new ReliefPart("NotchEastBlock_concrete", ReliefMat.Concrete, origin, Beat);
            var lit = new ReliefPart("NotchEastBlock_tunnelLight", ReliefMat.TunnelLight, origin, Beat);
            parts.Add(fac); parts.Add(conc); parts.Add(lit);
            var fw = new FacadeWriter(fac.Mb, origin);
            var o = origin; var cb = conc.Mb;
            byte F = FacadeWriter.ModeFacade;
            var holes = ReliefKit.LaneHoles();

            // west face (the courtyard's east wall), u = z - 115: lower part z 115..236 from y -360, upper part z 115..275 from y 38
            ReliefKit.HoledFaceX(fw, x0, -1f, z0, zA, WestLow, yA, null, z => z - z0, IdWest, F, 0f, (float)(z1 - z0));
            ReliefKit.HoledFaceX(fw, x0, -1f, z0, z1, yA, top, holes, z => z - z0, IdWest, F, 0f, (float)(z1 - z0));

            // the arc face (toward the spire), smooth normals, u = arc length from z 115; lower part to z 236, upper to z 275
            List<double> zs = GateDims.ArcZs(z0, z1);
            double zStar = ArcadeCornerZ();
            foreach (double zz in new[] { zA, zStar }) if (!zs.Contains(zz)) zs.Add(zz);
            zs.Sort();
            var u = new double[zs.Count];
            for (int i = 1; i < zs.Count; i++)
            {
                double dx = GateDims.ArcX(zs[i]) - GateDims.ArcX(zs[i - 1]), dz = zs[i] - zs[i - 1];
                u[i] = u[i - 1] + Math.Sqrt(dx * dx + dz * dz);
            }
            float uArc = (float)u[zs.Count - 1];
            for (int i = 0; i + 1 < zs.Count; i++)
            {
                double za = zs[i], zb = zs[i + 1], xa = GateDims.ArcX(za), xb = GateDims.ArcX(zb);
                Vector3 na = ArcNormal(xa, za), nb = ArcNormal(xb, zb);
                double ylo = zb <= zA + 1e-6 ? yb : yA;
                // the upper part of the arc face is split at y 38 so its vertex rows match the lower part (no T-junction)
                if (ylo < yA)
                    fw.Quad(ReliefKit.P(xa, yb, za), ReliefKit.P(xa, yA, za), ReliefKit.P(xb, yA, zb), ReliefKit.P(xb, yb, zb), na, na, nb, nb,
                        (float)u[i], (float)u[i], (float)u[i + 1], (float)u[i + 1], IdArc, F, 0f, uArc, na + nb);
                fw.Quad(ReliefKit.P(xa, yA, za), ReliefKit.P(xa, top, za), ReliefKit.P(xb, top, zb), ReliefKit.P(xb, yA, zb), na, na, nb, nb,
                    (float)u[i], (float)u[i], (float)u[i + 1], (float)u[i + 1], IdArc, F, 0f, uArc, na + nb);
            }

            // south face z 115 (x 914 .. arc), facing -z, split at y 38; north faces: z 275 above the arcade, z 236 below the arcade
            double xs1 = GateDims.ArcX(z0), xn1 = GateDims.ArcX(z1), xm1 = GateDims.ArcX(zA);
            FaceZ(fw, z0, Vector3.back, x0, xs1, yb, yA, IdSouth);
            FaceZ(fw, z0, Vector3.back, x0, xs1, yA, top, IdSouth);
            FaceZ(fw, z1, Vector3.forward, x0, xn1, yA, top, IdNorth);
            FaceZ(fw, zA, Vector3.forward, x0, xm1, yb, ArcadeBottom, IdNorth);
            // east of the arcade (x 990 .. the arc, which lies east of x 990 up to z 252.5): the lower part's north face beside the
            // arcade's south-east corner and the upper part's underside over the open corner
            double xArcE = SignsLayout.ArcX1;
            if (xm1 > xArcE + 1e-6)
            {
                FaceZ(fw, zA, Vector3.forward, xArcE, xm1, ArcadeBottom, yA, IdNorth);
                for (int i = 0; i + 1 < zs.Count; i++)
                {
                    double za = zs[i], zb = zs[i + 1];
                    if (za < zA - 1e-6 || zb > zStar + 1e-6) continue;
                    if (GateDims.ArcX(zb) - xArcE < 1e-3)        // the last strip closes in a point at z 252.5
                        EdgeMesh.Tri(cb, o, ReliefKit.P(xArcE, yA, za), ReliefKit.P(GateDims.ArcX(za), yA, za), ReliefKit.P(xArcE, yA, zb), Vector3.down);
                    else
                        EdgeMesh.FlatQuad(cb, o, ReliefKit.P(xArcE, yA, za), ReliefKit.P(GateDims.ArcX(za), yA, za), ReliefKit.P(GateDims.ArcX(zb), yA, zb), ReliefKit.P(xArcE, yA, zb), Vector3.down);
                }
            }

            // top cap (roof mode) and bottom cap (concrete), as trapezoid strips between the arc samples (the arc is concave seen
            // from the block: no fan), z 115..275 at the top, z 115..236 at the bottom
            for (int i = 0; i + 1 < zs.Count; i++)
            {
                double za = zs[i], zb = zs[i + 1], xa = GateDims.ArcX(za), xb = GateDims.ArcX(zb);
                fw.Flat(ReliefKit.P(x0, top, za), ReliefKit.P(xa, top, za), ReliefKit.P(xb, top, zb), ReliefKit.P(x0, top, zb), Vector3.up,
                    (float)x0, (float)xa, (float)xb, (float)x0, IdTop, FacadeWriter.ModeRoof, (float)x0, (float)xs1);
                if (zb <= zA + 1e-6)
                    EdgeMesh.FlatQuad(cb, o, ReliefKit.P(x0, yb, za), ReliefKit.P(xa, yb, za), ReliefKit.P(xb, yb, zb), ReliefKit.P(x0, yb, zb), Vector3.down);
            }

            // ledges on the west and south faces (0.8 m proud, 0.6 m thick; the west ledges stop 1 m short of the notch walls' planes,
            // the south ledges run from 1 m into face B's mass to the arc)
            foreach (double yl in GateDims.EbLedgeY)
            {
                double ya = yl - 0.5 * GateDims.EbLedgeT, ybb = yl + 0.5 * GateDims.EbLedgeT, pr = GateDims.EbLedgeProud;
                double zHi = yl < yA ? zA : z1;
                ReliefKit.Band(cb, o, x0 - pr, x0 + 0.2, ya, ybb, z0 + 1.0, zHi - 1.0);
                ReliefKit.Band(cb, o, x0 - 1.0, xs1 - 1.0, ya + 0.03, ybb + 0.03, z0 - pr, z0 + 0.2);
            }

            // lane tunnels (30 m, lit mouth, dark back wall) and their frames
            foreach (var h in holes)
            {
                ReliefKit.TunnelX(lit.Mb, cb, o, x0, 1.0, GateDims.PortalBlockDepth, GateDims.PortalLit, h, true);
                ReliefKit.FrameX(cb, o, x0, -1.0, h, GateDims.PortalFrame, 0.4);
            }
        }

        /// <summary>The z (> 236) where the arc meets the arcade's east end x 990 (252.5 for R 366; 236 when the arc lies west of it).</summary>
        public static double ArcadeCornerZ()
        {
            double dx = GateDims.SX - SignsLayout.ArcX1;
            if (GateDims.EbR <= dx) return GateDims.EbZArcade;
            double zc = GateDims.SZ - Math.Sqrt(GateDims.EbR * GateDims.EbR - dx * dx);
            return Math.Max(GateDims.EbZArcade, Math.Min(GateDims.EbZ1, zc));
        }

        /// <summary>Unit horizontal normal of the arc face at (x, z): toward the spire (the block lies outside the circle).</summary>
        static Vector3 ArcNormal(double x, double z)
        {
            var n = new Vector3((float)(GateDims.SX - x), 0f, (float)(GateDims.SZ - z));
            return n.normalized;
        }

        static void FaceZ(FacadeWriter fw, double z, Vector3 n, double xa, double xb, double ya, double yb2, uint id)
        {
            fw.Flat(ReliefKit.P(xa, ya, z), ReliefKit.P(xa, yb2, z), ReliefKit.P(xb, yb2, z), ReliefKit.P(xb, ya, z), n,
                (float)xa, (float)xa, (float)xb, (float)xb, id, FacadeWriter.ModeFacade, (float)xa, (float)xb);
        }
    }
}
