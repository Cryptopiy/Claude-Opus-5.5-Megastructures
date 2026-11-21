// Beat04ArmourSlope: the gate tower's exterior (14.5.4 "Tower: slab x -6..6, z 920-956, 80 m, window slits, 12 m crown,
// portal x -3.3..0.3 (3.6 x 3.4), arch x 0.8..5.5 (4.7 wide, 5.5 high), Tungsten glow"; KS12: top at +49 deg from s 852).
// The shell keeps the graybox's inner faces (0.5 m walls: x -5.5 / 5.5, z 920.5 / 955.5, roof underside 79.5), so the
// beat 5 atrium (graybox part beat5 or its builder) fits inside unchanged. Exterior: four rows of deep window slits (0.7 x
// 8 m, closed 0.35 m behind the face by a dark ConcreteDark back: no see-through into the hollow shaft) on the south,
// east and west faces above the atrium (y 14-58); corner pilasters 0.6 m proud from the ground to the crown (outside the
// lane funnel: x -6.6..-6 / 6..6.6 and z 919.4-920 only beyond |x| 5.4); the 12 m crown (y 68-80) proud by 0.4 m with a
// drip course at y 67.6-68 and a ring of louvre slits; a lit Tungsten lamp box over the portal (BwEmissive x3, the
// "Tungsten glow" with the atrium lamps behind it). The portal and both arches register as sky openings of the
// LocalLightPool (the graybox did this under part beat4).
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat04ArmourSlope
    {
        public const double TowerWall = 0.5, CrownY0 = 68.0, CrownProud = 0.4, PilasterProud = 0.6;
        public const double SlitW = 0.7, SlitH = 8.0, SlitRecess = 0.35;
        public static readonly double[] SlitRowY = { 14.0, 26.0, 38.0, 50.0 };
        public static readonly double[] SlitSouthX = { -4.2, -1.5, 3.15 };
        public static readonly double[] SlitSideZ = { 926.0, 938.0, 950.0 };   // between the side pilasters (z 920, 932, 944, 956)
        public const float OpeningReach = 40f;
        public static readonly double[] StringCourseY = { 12.0, 24.0, 36.0, 48.0, 60.0 };

        void BuildTower(WorldContext ctx, Transform root)
        {
            var shell = new MeshBuilder();
            var dark = new MeshBuilder();
            double tz0 = BulwarkConst.TowerZ0, tz1 = BulwarkConst.TowerZ1, th = BulwarkConst.TowerHeight, t = TowerWall;
            double px0 = BulwarkConst.PortalXMin, px1 = BulwarkConst.PortalXMax, ph = BulwarkConst.PortalHeight;
            double ax0 = BulwarkConst.ArchXMin, ax1 = BulwarkConst.ArchXMax, ah = BulwarkConst.ArchHeight;

            // ---- south face z 920..920.5: openings at the foot, slit rows above
            SouthNorthFace(shell, dark, tz0, tz0 + t, true, px0, px1, ph, ax0, ax1, ah, th);
            // ---- north face z 955.5..956: the bright arch only (the north face is never seen from beat 4; no slits)
            SouthNorthFace(shell, dark, tz1 - t, tz1, false, px0, px1, ph, ax0, ax1, ah, th);
            // ---- side walls (x -6..-5.5 and 5.5..6) with slit columns
            SideWall(shell, dark, -6.0, -6.0 + t, tz0 + t, tz1 - t, th, -1);
            SideWall(shell, dark, 6.0 - t, 6.0, tz0 + t, tz1 - t, th, +1);
            // ---- roof slab
            BoxMinMax(shell, -6.0 + t, th - t, tz0 + t, 6.0 - t, th, tz1 - t);

            // ---- the crown: a 0.4 m proud band y 68-80 on all four sides, a drip course under it, louvre slits
            var crown = new MeshBuilder();
            double cp = CrownProud;
            // (each band 2 cm into the wall: no co-planar faces; the side bands stop at the south / north bands)
            MeshKit.ChamferBox(crown, C(-6.0 - cp, CrownY0, tz0 - cp, 6.0 + cp, th + 0.3, tz0 + 0.02), S(-6.0 - cp, CrownY0, tz0 - cp, 6.0 + cp, th + 0.3, tz0 + 0.02), 0.03f);
            MeshKit.ChamferBox(crown, C(-6.0 - cp, CrownY0, tz1 - 0.02, 6.0 + cp, th + 0.3, tz1 + cp), S(-6.0 - cp, CrownY0, tz1 - 0.02, 6.0 + cp, th + 0.3, tz1 + cp), 0.03f);
            MeshKit.ChamferBox(crown, C(-6.0 - cp, CrownY0, tz0 + 0.02, -6.0 + 0.02, th + 0.3, tz1 - 0.02), S(-6.0 - cp, CrownY0, tz0 + 0.02, -6.0 + 0.02, th + 0.3, tz1 - 0.02), 0.03f);
            MeshKit.ChamferBox(crown, C(6.0 - 0.02, CrownY0, tz0 + 0.02, 6.0 + cp, th + 0.3, tz1 - 0.02), S(6.0 - 0.02, CrownY0, tz0 + 0.02, 6.0 + cp, th + 0.3, tz1 - 0.02), 0.03f);
            // drip course 0.25 m further out, 0.4 m tall
            double dp = cp + 0.25;
            MeshKit.ChamferBox(crown, C(-6.0 - dp, CrownY0 - 0.4, tz0 - dp, 6.0 + dp, CrownY0 + 0.02, tz0 - cp + 0.01), S(-6.0 - dp, CrownY0 - 0.4, tz0 - dp, 6.0 + dp, CrownY0 + 0.02, tz0 - cp + 0.01), 0.02f);
            MeshKit.ChamferBox(crown, C(-6.0 - dp, CrownY0 - 0.4, tz1 + cp - 0.01, 6.0 + dp, CrownY0 + 0.02, tz1 + dp), S(-6.0 - dp, CrownY0 - 0.4, tz1 + cp - 0.01, 6.0 + dp, CrownY0 + 0.02, tz1 + dp), 0.02f);
            MeshKit.ChamferBox(crown, C(-6.0 - dp, CrownY0 - 0.4, tz0 - cp + 0.01, -6.0 - cp + 0.01, CrownY0 + 0.02, tz1 + cp - 0.01), S(-6.0 - dp, CrownY0 - 0.4, tz0 - cp + 0.01, -6.0 - cp + 0.01, CrownY0 + 0.02, tz1 + cp - 0.01), 0.02f);
            MeshKit.ChamferBox(crown, C(6.0 + cp - 0.01, CrownY0 - 0.4, tz0 - cp + 0.01, 6.0 + dp, CrownY0 + 0.02, tz1 + cp - 0.01), S(6.0 + cp - 0.01, CrownY0 - 0.4, tz0 - cp + 0.01, 6.0 + dp, CrownY0 + 0.02, tz1 + cp - 0.01), 0.02f);
            // coping on the crown top (a 0.3 m cap round the roof)
            BoxMinMax(crown, -6.0 + t, th, tz0 + t, 6.0 - t, th + 0.05, tz1 - t);
            // louvres: panels of six dark slats (1.0 m wide, 0.12 m tall, 0.15 m proud) in the crown band, y 71-77
            for (int k = 0; k < 4; k++)
            {
                double x = -4.5 + 3.0 * k;
                for (int sl = 0; sl < 6; sl++)
                {
                    double y = CrownY0 + 3.0 + 1.1 * sl;
                    MeshKit.ChamferBox(dark, C(x - 0.5, y, tz0 - cp - 0.15, x + 0.5, y + 0.12, tz0 - cp + 0.05), S(x - 0.5, y, tz0 - cp - 0.15, x + 0.5, y + 0.12, tz0 - cp + 0.05), 0.01f);
                }
            }
            for (int k = 0; k < 9; k++)
            {
                double z = tz0 + 2.0 + 4.0 * k;
                for (int sl = 0; sl < 6; sl++)
                {
                    double y = CrownY0 + 3.0 + 1.1 * sl;
                    MeshKit.ChamferBox(dark, C(6.0 + cp - 0.05, y, z - 0.5, 6.0 + cp + 0.15, y + 0.12, z + 0.5), S(6.0 + cp - 0.05, y, z - 0.5, 6.0 + cp + 0.15, y + 0.12, z + 0.5), 0.01f);
                    MeshKit.ChamferBox(dark, C(-6.0 - cp - 0.15, y, z - 0.5, -6.0 - cp + 0.05, y + 0.12, z + 0.5), S(-6.0 - cp - 0.15, y, z - 0.5, -6.0 - cp + 0.05, y + 0.12, z + 0.5), 0.01f);
                }
            }

            // ---- corner pilasters (ground to the crown), proud of the south / north faces and the sides
            double pp = PilasterProud;
            foreach (double zc in new[] { tz0, tz1 })
            {
                bool south = zc == tz0;
                double za = south ? tz0 - pp : tz1 - 0.45, zb = south ? tz0 + 0.45 : tz1 + pp;   // inner faces inside the 0.5 m walls
                MeshKit.ChamferBox(crown, C(-6.0 - pp, 0.0, za, -5.55, CrownY0 - 0.38, zb), S(-6.0 - pp, 0.0, za, -5.55, CrownY0 - 0.38, zb), 0.03f);
                MeshKit.ChamferBox(crown, C(5.55, 0.0, za, 6.0 + pp, CrownY0 - 0.38, zb), S(5.55, 0.0, za, 6.0 + pp, CrownY0 - 0.38, zb), 0.03f);
            }
            // intermediate side pilasters every 12 m (z 932, 944) on both sides
            foreach (double zc in new[] { 932.0, 944.0 })
            {
                MeshKit.ChamferBox(crown, C(-6.0 - pp, 0.0, zc - 0.5, -6.0 + 0.02, CrownY0 - 0.38, zc + 0.5), S(-6.0 - pp, 0.0, zc - 0.5, -6.0 + 0.02, CrownY0 - 0.38, zc + 0.5), 0.03f);
                MeshKit.ChamferBox(crown, C(6.0 - 0.02, 0.0, zc - 0.5, 6.0 + pp, CrownY0 - 0.38, zc + 0.5), S(6.0 - 0.02, 0.0, zc - 0.5, 6.0 + pp, CrownY0 - 0.38, zc + 0.5), 0.03f);
            }
            // string courses: 0.4 m bands 0.12 m proud round the shaft at y 12, 24, 36, 48, 60 (between the slit rows): the
            // storeys a walker counts (the corners run into the pilasters)
            foreach (double yb in StringCourseY)
            {
                double y0 = yb - 0.2, y1 = yb + 0.2, q = 0.12;
                MeshKit.ChamferBox(crown, C(-6.0, y0, tz0 - q, 6.0, y1, tz0 + 0.02), S(-6.0, y0, tz0 - q, 6.0, y1, tz0 + 0.02), 0.02f);
                MeshKit.ChamferBox(crown, C(-6.0, y0, tz1 - 0.02, 6.0, y1, tz1 + q), S(-6.0, y0, tz1 - 0.02, 6.0, y1, tz1 + q), 0.02f);
                MeshKit.ChamferBox(crown, C(6.0 - 0.02, y0, tz0 + 0.03, 6.0 + q, y1, tz1 - 0.03), S(6.0 - 0.02, y0, tz0 + 0.03, 6.0 + q, y1, tz1 - 0.03), 0.02f);
                MeshKit.ChamferBox(crown, C(-6.0 - q, y0, tz0 + 0.03, -6.0 + 0.02, y1, tz1 - 0.03), S(-6.0 - q, y0, tz0 + 0.03, -6.0 + 0.02, y1, tz1 - 0.03), 0.02f);
            }
            // portal lintel: a 0.35 m band 0.25 m proud of the south face over the portal
            double fr = 0.35;
            MeshKit.ChamferBox(crown, C(px0 - fr, ph, tz0 - 0.25, px1 + fr, ph + fr, tz0 + 0.02), S(px0 - fr, ph, tz0 - 0.25, px1 + fr, ph + fr, tz0 + 0.02), 0.02f);
            // (no jamb pieces at the foot: the portal lane's capsule reaches x -3.5..0.5, so nothing proud below the lintel)

            Add(ctx, root, shell, "b4_tower_shell", m_concrete);
            Add(ctx, root, crown, "b4_tower_crown", m_concrete);
            Add(ctx, root, dark, "b4_tower_slits", m_dark);

            // ---- the Tungsten lamp over the portal (emissive box 1.6 x 0.22 x 0.3 under a steel hood)
            var lamp = new MeshBuilder();
            float lx = (float)(0.5 * (px0 + px1)), ly = (float)(ph + fr + 0.45), lz = (float)(tz0 - 0.4);
            MeshKit.ChamferBox(lamp, new Vector3(lx, ly, lz), new Vector3(1.6f, 0.22f, 0.3f), 0.03f);
            // a Tungsten soffit strip under the portal lintel (inside the 0.5 m reveal, 1 cm into the lintel: no co-planar face):
            // the warm line that marks the door from the road (KS12 "portal glow near the bottom")
            MeshKit.ChamferBox(lamp, new Vector3(lx, (float)ph - 0.025f, (float)tz0 + 0.25f), new Vector3((float)(px1 - px0) - 0.4f, 0.07f, 0.22f), 0.01f);
            Add(ctx, root, lamp, "b4_portal_lamp", m_lamp, false);
            var hood = new MeshBuilder();
            MeshKit.ChamferBox(hood, new Vector3(lx, ly + 0.2f, lz - 0.02f), new Vector3(1.8f, 0.08f, 0.5f), 0.01f);
            MeshKit.ChamferBox(hood, new Vector3(lx, ly + 0.12f, (float)tz0 - 0.12f), new Vector3(1.2f, 0.2f, 0.25f), 0.01f);
            Add(ctx, root, hood, "b4_portal_hood", m_steel);

            // ---- sky openings (LocalLightPool) for the atrium and the vehicle passage
            var pool = Megastructure.Rendering.LocalLightPool.Ensure(ctx);
            if (pool != null)
            {
                pool.AddOpening("beat4_portal", (float)px0, (float)px1, 0f, (float)ph, (float)tz0, false, OpeningReach, true);
                pool.AddOpening("beat4_arch_s", (float)ax0, (float)ax1, 0f, (float)ah, (float)tz0, false, OpeningReach, true);
                pool.AddOpening("beat4_arch_n", (float)ax0, (float)ax1, 0f, (float)ah, (float)tz1, true, OpeningReach, true);
            }
        }

        /// <summary>A south (or north) face slab between z0 and z1 with the portal and arch openings (the north face: the arch
        /// only) and, on the south face, the slit rows (dark backs 0.35 m behind the outer face).</summary>
        static void SouthNorthFace(MeshBuilder mb, MeshBuilder dark, double z0, double z1, bool south,
                                   double px0, double px1, double ph, double ax0, double ax1, double ah, double th)
        {
            // foot band y 0..max(ah) split round the openings; above it the full width with the slit rows
            if (south)
            {
                BoxMinMax(mb, -6.0, 0.0, z0, px0, ah, z1);
                BoxMinMax(mb, px0, ph, z0, px1, ah, z1);
                BoxMinMax(mb, px1, 0.0, z0, ax0, ah, z1);
            }
            else BoxMinMax(mb, -6.0, 0.0, z0, ax0, ah, z1);
            BoxMinMax(mb, ax1, 0.0, z0, 6.0, ah, z1);
            double y = ah;
            if (south)
            {
                foreach (double ry in SlitRowY)
                {
                    BoxMinMax(mb, -6.0, y, z0, 6.0, ry, z1);
                    double x = -6.0;
                    foreach (double sx in SlitSouthX)
                    {
                        BoxMinMax(mb, x, ry, z0, sx - 0.5 * SlitW, ry + SlitH, z1);
                        // dark back of the slit, its outer face 0.35 m in, its inner face on the wall's inner plane
                        BoxMinMax(dark, sx - 0.5 * SlitW, ry, z0 + SlitRecess, sx + 0.5 * SlitW, ry + SlitH, z1);
                        x = sx + 0.5 * SlitW;
                    }
                    BoxMinMax(mb, x, ry, z0, 6.0, ry + SlitH, z1);
                    y = ry + SlitH;
                }
            }
            BoxMinMax(mb, -6.0, y, z0, 6.0, th, z1);
        }

        static void SideWall(MeshBuilder mb, MeshBuilder dark, double x0, double x1, double z0, double z1, double th, int side)
        {
            double y = 0.0;
            foreach (double ry in SlitRowY)
            {
                BoxMinMax(mb, x0, y, z0, x1, ry, z1);
                double z = z0;
                foreach (double sz in SlitSideZ)
                {
                    BoxMinMax(mb, x0, ry, z, x1, ry + SlitH, sz - 0.5 * SlitW);
                    double bx0 = side > 0 ? x0 : x0, bx1 = side > 0 ? x1 - SlitRecess : x1;
                    if (side < 0) { bx0 = x0 + SlitRecess; bx1 = x1; }
                    BoxMinMax(dark, bx0, ry, sz - 0.5 * SlitW, bx1, ry + SlitH, sz + 0.5 * SlitW);
                    z = sz + 0.5 * SlitW;
                }
                BoxMinMax(mb, x0, ry, z, x1, ry + SlitH, z1);
                y = ry + SlitH;
            }
            BoxMinMax(mb, x0, y, z0, x1, th, z1);
        }

        static void BoxMinMax(MeshBuilder mb, double x0, double y0, double z0, double x1, double y1, double z1)
        {
            if (x1 - x0 < 1e-4 || y1 - y0 < 1e-4 || z1 - z0 < 1e-4) return;
            mb.Box(C(x0, y0, z0, x1, y1, z1), S(x0, y0, z0, x1, y1, z1));
        }

        static Vector3 C(double x0, double y0, double z0, double x1, double y1, double z1) => new Vector3((float)(0.5 * (x0 + x1)), (float)(0.5 * (y0 + y1)), (float)(0.5 * (z0 + z1)));
        static Vector3 S(double x0, double y0, double z0, double x1, double y1, double z1) => new Vector3((float)(x1 - x0), (float)(y1 - y0), (float)(z1 - z0));
    }
}
