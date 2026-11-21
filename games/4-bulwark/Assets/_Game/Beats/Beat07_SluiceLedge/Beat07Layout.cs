// Beat07Layout: the pure geometry of beat 7, the sluice tunnel and the lee ledge (MASTER_PROMPT 14.5.7, 14.4, 14.12), in
// double and without UnityEngine, so the EditMode tests (Beat07Tests) check every dimension the builder uses.
//   * Tunnel: from the hall end wall (s 1373, its doorway) along A3 and the -18 deg diagonal to the lee face; 3.2 m wide
//     (walls at the lane +-1.25 + PLAYER_R 0.35: the lane is the 14.4 "collider gap minus 0.70"), 3.4 m high, floor y -90.
//     The floor is wet concrete (14.5.7 "wet floor"); the walls run 0.3 m below it (no seam at the floor line).
//   * East side (lateral +1.6): the continuous grating panel 2.8 m tall over the 43 m of the diagonal (A3 end .. the mouth,
//     s 1379.875 .. 1422.85), a 0.6 m lintel above it; behind it the conduit (8 m wide, 6 m high, the sea at 14 m/s): water
//     surface 0.35 m below the tunnel floor (supercritical, Froude 14 / sqrt(9.81 x 1.45) = 3.7), cold wall lights on its far wall,
//     a culvert head wall at its south end and a drop shaft under a floor grate at its north end, a solid end wall.
//   * Mouth: the oblique cut through the lee face x -6 at s 1422.85: the tunnel walls cross the face at z 1415.23 and
//     1425.57 (10.34 m, "an oblique cut 10.4 m long"); every tunnel surface is clipped to x >= FaceX.
//   * Ledge: slab x -12.4..-6.0 from the apron edge z 1415.2 (DEVELOPMENT.md "Tunnel mouth floor": the lane covers a
//     triangle beyond the face for s 1419.0-1422.8) to z 1503, 0.8 m thick on brackets every 6 m; kerb 0.12 high at the
//     drop edge; rail 1.10 (posts 0.12 every 2 m, round rails 0.10 with tops 1.10 / 0.55) on the drop side and both ends;
//     mooring rings 0.3 m every 6 m in pockets of the face; the near lee face (real geometry, x FaceX, z 1000..1800, y
//     -120..0) with the mouth hole; beyond it the analytic face of BwFarField.
//   * Lantern (14.11): (-9.2, -90, 1499.0), iron post 1.4 on a 0.4 base plate, octagonal glass drum 0.30 with a cage.
// Everything outside the capsule reach of the lane (lane edge + 0.35; Wave A decision "walls follow the lane's capsule
// envelope, rails outside the capsule reach").
using System;

namespace Megastructure.Beats
{
    public static class Beat07Layout
    {
        // ---------------------------------------------------------------- tunnel
        /// <summary>First s of the tunnel (the hall end wall and its doorway, 14.6 "hall end wall at s 1373").</summary>
        public const double TunnelS0 = BulwarkConst.HallEndWallS;
        /// <summary>Floor (walking surface) of the tunnel and the ledge.</summary>
        public const double FloorY = BulwarkConst.FloorYTunnel;
        /// <summary>Half width of the tunnel lane (+-1.25).</summary>
        public const double LaneHalf = 0.5 * BulwarkConst.LaneTunnelWidth;
        /// <summary>Lateral of the tunnel walls: lane edge + PLAYER_R = 1.60 (3.2 m wide, 14.5.7).</summary>
        public const double WallLateral = LaneHalf + BulwarkConst.PlayerR;
        public const double CeilingY = FloorY + BulwarkConst.TunnelHeight;
        /// <summary>Depth the tunnel walls run on below the floor (no open seam at the floor line); bearer pitch (unused).</summary>
        public const double TroughDepth = 0.30, DeckBearerPitch = 1.0;
        /// <summary>The lee face as built (3 mm west of the analytic plane x -6.0 so no surface is coplanar with the graybox).</summary>
        public const double FaceX = BulwarkConst.LeeFaceX - 0.003;

        // ---------------------------------------------------------------- grating panel and conduit (14.5.7)
        public static double PanelS0 => BwPath.A3EndS;                           // 1379.875
        public const double PanelS1 = BulwarkConst.TunnelMouthS;                 // 1422.85
        public static double PanelLength => PanelS1 - PanelS0;                   // 42.975 ("43 m")
        public const double PanelTopY = FloorY + BulwarkConst.GratingPanelHeight; // -87.2
        /// <summary>Panel modules (frame posts behind the grating plane, on the conduit side).</summary>
        public const double PanelModule = 2.0;
        /// <summary>Lateral of the conduit's tunnel-side wall (the panel plane) and of its far wall (8 m wide, 14.5.7).</summary>
        public const double ConduitV0 = WallLateral, ConduitV1 = WallLateral + 0.3 + BulwarkConst.ConduitWidth;   // 1.6 .. 9.9
        public const double ConduitFloorY = FloorY - 1.8;                        // -91.8
        public const double ConduitCeilingY = ConduitFloorY + BulwarkConst.ConduitHeight;   // -85.8 (6 m)
        public const double WaterY = FloorY - 0.35;                              // mean surface -90.35 (depth 1.45 m; every
                                                                                 // crest on the panel side stays under the floor)
        public const double FlowSpeed = BulwarkConst.ConduitSpeed;               // 14 m/s
        /// <summary>Conduit u range (u = metres along the tunnel tangent from the panel's south end): the culvert head wall at
        /// u -4 (water emerges below it), the drop shaft from u 37.5, the end wall at the panel's north end.</summary>
        public const double CulvertU = -4.0, ShaftU0 = 37.5, CulvertSoffitY = FloorY + 1.3;
        public static double EndU => PanelLength;
        /// <summary>Cold lights on the conduit's far wall: every 6 m (between the tunnel cages), 1.2 m bars 3 m above the water.</summary>
        public const double ColdLampPitch = 6.0, ColdLampU0 = 2.0, ColdLampY = WaterY + 3.0, ColdLampLength = 1.2;

        // ---------------------------------------------------------------- lamps
        /// <summary>SodiumWarm cages every 6 m in the tunnel ceiling (14.5.7), s 1376 .. 1418, at lateral -0.55.</summary>
        public const double LampS0 = 1376.0, LampLateral = -0.55;
        public static double LampS(int k) => LampS0 + BulwarkConst.TunnelLampPitch * k;
        public static int LampCount => (int)Math.Floor((BulwarkConst.TunnelMouthS - 4.0 - LampS0) / BulwarkConst.TunnelLampPitch) + 1;
        public const double LampRange = 7.0, LampIntensity = 1.6;
        /// <summary>Threshold reinforcement (road-tunnel practice, CIE 88): the cage nearest the mouth (s 1418, 4.85 m inside it)
        /// burns at x2 so the jamb seen against the dusk sky is not a black cut-out: the west wall reveal at the frame edge
        /// (51 px/frame at 2.5 m/s, sky 150-175 against wall 56-87) stays under the 1.2 levels/frame step (seq_s1405_1450).</summary>
        public const double ThresholdLampGain = 2.0;
        public static double LampIntensityAt(int k) => k == LampCount - 1 ? LampIntensity * ThresholdLampGain : LampIntensity;

        // ---------------------------------------------------------------- mouth, ledge
        /// <summary>z where the tunnel wall at <paramref name="lateral"/> crosses the plane x = <paramref name="x"/> (bisection on
        /// the diagonal and A4, where x falls monotonically with s).</summary>
        public static double WallCrossZ(double lateral, double x = BulwarkConst.LeeFaceX)
        {
            double lo = BulwarkConst.TunnelMouthS - 15.0, hi = BwPath.A4EndS;
            for (int it = 0; it < 64; it++)
            {
                double m = 0.5 * (lo + hi);
                BwPath.World(m, lateral, out double wx, out _, out _);
                if (wx > x) lo = m; else hi = m;
            }
            BwPath.World(0.5 * (lo + hi), lateral, out _, out _, out double z);
            return z;
        }

        /// <summary>The mouth opening in the face plane: z of the west (left) and east (right) walls at FaceX.</summary>
        public static void Mouth(out double z0, out double z1) { z0 = WallCrossZ(-WallLateral, FaceX); z1 = WallCrossZ(WallLateral, FaceX); }

        /// <summary>Reach (m from the opening centre) of the sky light through the mouth (BwMouthLight; the threshold zone):
        /// at 24 m the opening's form factor is 0.019 facing it squarely and below 0.007 for tunnel surfaces (their line of
        /// sight runs 72 deg off the face normal); the term fades out over the last 30 % of the reach.</summary>
        public const double MouthLightReach = 24.0;

        /// <summary>C# twin of BwMouthLight's form factor: Lambert's polygon formula for the mouth rectangle (x = FaceX, z0..z1,
        /// FloorY..CeilingY) seen from p with unit normal n (0 west of the face).</summary>
        public static double MouthFormFactor(double px, double py, double pz, double nx, double ny, double nz)
        {
            if (px <= FaceX) return 0.0;
            Mouth(out double z0, out double z1);
            double[,] v = { { FaceX, FloorY, z0 }, { FaceX, FloorY, z1 }, { FaceX, CeilingY, z1 }, { FaceX, CeilingY, z0 } };
            double fx = 0, fy = 0, fz = 0;
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) & 3;
                double ax = v[i, 0] - px, ay = v[i, 1] - py, az = v[i, 2] - pz;
                double bx = v[j, 0] - px, by = v[j, 1] - py, bz = v[j, 2] - pz;
                double la = Math.Sqrt(ax * ax + ay * ay + az * az), lb = Math.Sqrt(bx * bx + by * by + bz * bz);
                ax /= la; ay /= la; az /= la; bx /= lb; by /= lb; bz /= lb;
                double cx = ay * bz - az * by, cy = az * bx - ax * bz, cz = ax * by - ay * bx;
                double l = Math.Sqrt(cx * cx + cy * cy + cz * cz);
                if (l < 1e-12) continue;
                double g = Math.Acos(Math.Max(-1.0, Math.Min(1.0, ax * bx + ay * by + az * bz))) / l;
                fx += cx * g; fy += cy * g; fz += cz * g;
            }
            return Math.Max(0.0, Math.Min(1.0, (nx * fx + ny * fy + nz * fz) / (2.0 * Math.PI)));
        }

        public const double LedgeX0 = BulwarkConst.LedgeXMin, LedgeX1 = FaceX;   // -12.4 .. the face
        // the slab ends at the top station of the elevator (ADDENDUM A1 00): the cabin shaft lies beyond z 1494.0
        public const double LedgeZ0 = 1415.2, LedgeZ1 = BulwarkConst.LedgeEndZ;  // apron edge .. 1494.0
        /// <summary>The gap in the north kerb and rail for the cabin's 1.6 m south door (x -10.0..-8.4) plus 5 cm each side.</summary>
        public const double DoorGapX0 = -10.05, DoorGapX1 = -8.35;
        public const double LedgeThickness = BulwarkConst.LedgeThickness;
        /// <summary>Kerb along the drop edge (x -12.4 .. -12.17, 0.12 high): outside the capsule reach (lane -11.8 - 0.35).</summary>
        public const double KerbX1 = -12.17, KerbH = 0.12;
        /// <summary>Rail axis on the drop side (posts 0.12 and rails 0.10 inside x -12.40 .. -12.22: inner face beyond -12.15).</summary>
        public const double RailX = -12.31, RailZS = LedgeZ0 + 0.09, RailZN = LedgeZ1 - 0.09;
        public const double RailTop = BulwarkConst.LedgeRail, RailMid = 0.55, RailDiameter = BulwarkConst.LedgeRailDiameter;
        public const double PostSize = BulwarkConst.LedgeRailPost, PostPitch = BulwarkConst.LedgeRailPostPitch;
        /// <summary>Brackets (corbels) under the slab every 6 m: 2.6 m deep at the face, 0.5 m at x -11.6.</summary>
        public const double BracketPitch = 6.0, BracketWidth = 0.5, BracketDepthFace = 2.6, BracketX1 = -11.6;
        /// <summary>Mooring rings 0.3 m (diameter) every 6 m in 0.12 m deep pockets of the face, ring centre 0.75 m above the slab.</summary>
        public const double RingPitch = 6.0, RingZ0 = 1430.0, RingDiameter = 0.3, RingY = FloorY + 0.75, PocketDepth = 0.12;

        // ---------------------------------------------------------------- near lee face (real geometry)
        public const double FaceZ0 = 1000.0, FaceZ1 = 1800.0, FaceY0 = -120.0, FaceY1 = 0.0;

        // ---------------------------------------------------------------- lantern (14.5.7, 14.11)
        public const double LanternX = BulwarkConst.LanternX, LanternY = BulwarkConst.LanternY, LanternZ = BulwarkConst.LanternZ;
        public const double LanternPost = BulwarkConst.LanternPost, LanternBase = BulwarkConst.LanternBase, LanternGlass = BulwarkConst.LanternGlass;
        /// <summary>Glass drum: octagon 0.30 across flats, 0.34 tall, on a 0.03 base ring at the post top; roof 0.14.</summary>
        public const double DrumHeight = 0.34, DrumRing = 0.03, RoofHeight = 0.14;
        public static double DrumCentreY => LanternY + LanternPost + DrumRing + 0.5 * DrumHeight;

        // ---------------------------------------------------------------- helpers (double)
        /// <summary>Tunnel frame at the panel's south end: origin (world x, z), unit tangent and right vectors (plan).</summary>
        public static void ConduitFrame(out double ox, out double oz, out double tx, out double tz, out double rx, out double rz)
        {
            BwPath.Point(PanelS0, out ox, out oz);
            BwPath.Tangent(PanelS0, out tx, out tz);
            BwPath.Right(PanelS0, out rx, out rz);
        }

        /// <summary>World point of conduit coordinates (u along the diagonal from the panel's south end, v lateral, y).</summary>
        public static void ConduitWorld(double u, double v, double y, out double x, out double wy, out double z)
        {
            ConduitFrame(out double ox, out double oz, out double tx, out double tz, out double rx, out double rz);
            x = ox + u * tx + v * rx;
            z = oz + u * tz + v * rz;
            wy = y;
        }
    }
}
