// Beat05Layout: the pure geometry of beat 5, the gate tower atrium and the descent (MASTER_PROMPT 14.5.5, 14.4; Addendum A1
// 1.4 "in beat 5 >= 1 m outside the porthole outer faces"). Double precision, no UnityEngine (Beat05Tests checks it).
// Lane-driven pieces follow BwPath (FloorY with the exact 270 treads and risers, TreadLineY, Lane) like the graybox they
// replace: walls PLAYER_R outside the lane's capsule envelope (stairs 2.9 + 2 x 0.35 = 3.6 wide, exit hall 3.7 + 0.7 =
// 4.4), the stair soffit at tread line + 2.6, the hall ceiling at floor + 4.0; the round 0.05 m handrails lie in
// 0.16 x 0.10 m grooves of both stair walls (Wave A ruling: a projecting rail would enter the capsule reach).
// Portholes: three 0.6 m bores at landings 5, 8, 11 (s 968.49, 988.29, 1008.09), centred at eye height (landing + 1.7: eye
// depths 8.3, 19.1, 29.9 m below the mean sea y -8), bored perpendicular to the east stair wall THROUGH to the sea face
// (x 6.0 above y -20, batter x = 6 + (-20 - y) / 8 below): the 14.5.5 "2.1 m deep" would leave 1.8-4.2 m of concrete
// between the glass and the sea (the stair wall stands 3.9-6.0 m behind the face at those depths), so the glass sits
// 0.05 m inside the face and the bore is as long as the wall is thick (lead decision 4: consistent geometry; the kraken
// arm of K6 passes >= 1 m outside these outer faces).
using System;

namespace Megastructure.Beats
{
    public static class Beat05Layout
    {
        public const double LaneStartS = 934.0;                           // stair mouth
        public const double EndS = BulwarkConst.GalleryStartS;            // 1070: the exit hall ends at the gallery doorway (beat 6's wall)
        public const double WallOffset = 0.35;                            // wall face beyond the lane envelope = PLAYER_R
        public const double FloorMargin = 0.75;                           // floor strip beyond the capsule-centre lane
        public const double GrooveDepth = 0.10, GrooveHalf = 0.08;        // handrail groove 0.16 x 0.10
        public const double WellDeckUnderside = -0.5, WellSkirt = 0.12;  // atrium floor slab over the stairs
        public const double TowerInX0 = -5.5, TowerInX1 = 5.5, TowerInZ0 = 920.5, TowerInZ1 = 955.5;   // tower inner faces (0.5 walls)
        public const double CeilingY0 = BulwarkConst.AtriumHeight, CeilingY1 = BulwarkConst.AtriumHeight + 0.3;   // 9.0 .. 9.3
        public static readonly double[] BeamZ = { 921.5, 925.0, 929.0, 933.0 };   // ceiling beams between the pendant rows
        public const double BeamHalf = 0.25, BeamDepth = 0.8;
        public const double PendantRodR = 0.012;
        public const double GuardX = 0.55;                                // guard rail line between portal and arch (graybox)

        // ---- stairs (14.5.5 / 14.12)
        public static int Landings => BulwarkConst.StairFlights - 1;      // 14
        /// <summary>Landing k (1..14) follows flight k - 1: s range and floor y (-3.6 k).</summary>
        public static void Landing(int k, out double s0, out double s1, out double y)
        {
            s0 = BwPath.StairFlightStartS(k - 1) + BulwarkConst.StairFlightRun;
            s1 = s0 + BulwarkConst.StairLanding;
            y = -BulwarkConst.StairFlightDrop * k;
        }
        public static double LandingCentre(int k) { Landing(k, out double a, out double b, out _); return 0.5 * (a + b); }
        public static readonly int[] PortholeLanding = { 5, 8, 11 };
        public const double PortholeR = 0.5 * BulwarkConst.PortholeDiameter;   // 0.3
        public const double PortholeEye = 1.7;                            // centre above the landing (eye height)
        public const double GlassInset = 0.05;
        /// <summary>x of the sea face at height y (14.4: x 6.0 to y -20, then batter 1:8).</summary>
        public static double SeaFaceX(double y) => y >= BulwarkConst.SeaFaceBatterY ? BulwarkConst.SeaFaceX : BulwarkConst.SeaFaceX + (BulwarkConst.SeaFaceBatterY - y) / 8.0;
        public const double LampNicheHalf = 0.25, LampNicheY0 = 1.95, LampNicheY1 = 2.35, LampNicheDepth = 0.14;   // landing lamp niche (west wall)
        public const float LandingLampRange = 8f, LandingLampIntensity = 1.0f, HallLampRange = 8f, HallLampIntensity = 1.6f;
        /// <summary>Landing lamp spot (Wave X resume): 0.08 m in front of the wall plane at the niche centre, aimed 45 deg down
        /// into the well, full cone 150 deg (inner 105): nothing above 30 deg over the horizontal on the well side, nothing
        /// straight up (the soffit 0.45 m above the niche centre stays out of the cone). The spot drops the portal lamp boost
        /// the point light had (x1.57 at landing 1, s 940; x1.0 from landing 4) and no longer lights the soffit: intensity
        /// 0.8 -> 1.0 keeps the flights warm (KS14 mean luma 58 with the points, 49 with 0.8 spots).</summary>
        public const double LandingSpotInset = 0.08;
        public const float LandingSpotAngle = 150f;
        public const float PendantRange = 13f, PendantIntensity = 12.5f;   // 5.6 m above the floor: the 14.5.5 "Tungsten x3.0" glass, a stronger real light so the 9 m atrium reads (KS13 mean luma 27 at 3.0 / 8 m, 32 at 6.5 / 11 m, 35.5 at 9.0 / 13 m; 12.5 in the Wave X resume: the inner walls that replace the bright portal jambs on the walk in were 95-110 levels against 140 outside, seq_b04_b05 -3.0 levels per frame at s 918)
        /// <summary>Wet-streak weight (0..1, vertex B of the stair walls): 0 to landing 6, growing to 1 at landing 14.</summary>
        public static double Wetness(double s)
        {
            double a = LandingCentre(6), b = LandingCentre(14);
            return BwMath.Smootherstep01((s - a) / (b - a));
        }
        public const double NosingDepth = 0.03, NosingDrop = 0.03, NosingProud = 0.004;
        public static double ExitHallLampS(int k) => BulwarkConst.ExitHallS0 + 4.0 + BulwarkConst.ExitHallLampPitch * k;
        public static int ExitHallLamps { get { int n = 0; while (ExitHallLampS(n) < BulwarkConst.ExitHallS1 - 1.0) n++; return n; } }

        /// <summary>Absolute ceiling y: stairs and A2 tread line + 2.6; exit hall floor + 4.0.</summary>
        public static double CeilingY(double s)
        {
            if (s < BwPath.A2EndS) return BwPath.TreadLineY(s) + BulwarkConst.StairHeadroom;
            return BwPath.FloorY(s) + BulwarkConst.ExitHallHeight;
        }

        /// <summary>First s where the stair soffit lies at or below the atrium floor slab underside (open well before it).</summary>
        public static double WellClosedS()
        {
            double lo = LaneStartS, hi = BwPath.StairsStartS + BulwarkConst.StairFlightRun;
            for (int it = 0; it < 60; it++) { double m = 0.5 * (lo + hi); if (CeilingY(m) > WellDeckUnderside) lo = m; else hi = m; }
            return hi;
        }

        /// <summary>Lane limits widened to the capsule footprint (min / max of BwPath.Lane over s +- PLAYER_R + 1 cm).</summary>
        public static void LaneEnvelope(double s, out double lo, out double hi)
        {
            BwPath.Lane(s, out lo, out hi);
            double r = BulwarkConst.PlayerR + 0.01;
            for (int k = -8; k <= 8; k++)
            {
                if (k == 0) continue;
                BwPath.Lane(s + r * k / 8.0, out double l, out double h);
                if (l < lo) lo = l;
                if (h > hi) hi = h;
            }
        }

        /// <summary>Stair width between the wall faces at s (14.5.5: 3.6 on the flights).</summary>
        public static double WallSpan(double s) { LaneEnvelope(s, out double lo, out double hi); return hi - lo + 2.0 * WallOffset; }

        /// <summary>Lateral of the atrium guard rail along the open well: 0.25 m outside the stairwell's east wall.</summary>
        public static double WellRailLateral() { LaneEnvelope(WellClosedS(), out _, out double hi); return hi + WallOffset + 0.25; }

        /// <summary>Number of chevrons painted on landing k (a wordless depth counter): k.</summary>
        public static int Chevrons(int k) => k;
    }
}
