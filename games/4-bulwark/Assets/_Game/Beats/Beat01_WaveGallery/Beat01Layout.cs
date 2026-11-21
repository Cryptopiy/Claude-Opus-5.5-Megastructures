// Beat01Layout: the pure geometry and timing of beat 1, the wave gallery (MASTER_PROMPT 14.5.1, 14.4; Addendum A1 K0).
// Double precision, no UnityEngine (Beat01Tests checks it). Coordinates: world, z = s on the straight gallery leg (heading 0),
// path centre x -1.5 (BulwarkConst.StartX).
// Cross-section (decision, Wave A row "Beat 1 ribs and props vs the corridor lane"): the 14.5.1 clear profile 4.0 x 3.6 m
// (top corners r 0.3) is the profile of the RIBS (0.4 m wide at z 3 + 6 k); between the ribs the walls and the ceiling are
// recessed by the rib projection 0.15 m (panels at x -3.65 / 0.65, ceiling 3.75), so "ribs 0.15 proud" holds while no rib
// enters the capsule reach of the corridor lane (lane x -3.15..0.15 + PLAYER_R 0.35 = the rib faces x -3.5 / 0.5). West-wall
// props (pipes, cable tray, band, valve wheels) stand on the recessed panels and stay behind the rib plane (protrusion 0.15,
// spec "at most 0.25"). Each wave slit sits in a 1.6 m cast pilaster flush with the rib plane, so its mouth is at x 0.5 and
// the channel is 1.2 m deep to the baffle (x 1.7, 14.12 SlitX + SlitDepth).
// Niches: 1.0 x 2.1 at z 6 + 24 k on the west wall, back at x -3.85 (0.35 m behind the rib plane). The 14.5.1 depth 0.8
// does not fit the 0.6 m shell wall (x -3.5 .. -4.1) and a back beyond x -3.85 takes exterior ambient on the niche cheeks
// (interior box face x -4.1 with the 0.75 m soft edge; at x -3.95 the cheek corners keep about 25 % exterior ambient, at
// -3.85 about 4 %; DEVELOPMENT.md 2026-11-19 int-05 row).
// Door (T2): gap(t) = 4.0 smootherstep(t / 3.0) after the trigger at s 104 (t on the ambient clock since the walker first
// reached s 104), with the progress-keyed fallback 4.0 smootherstep((s - 104) / 7.5) (the graybox rule, identical at every
// speed and the only rule in capture mode, so stills are reproducible): gap = max(timed, progress).
using System;

namespace Megastructure.Beats
{
    public static class Beat01Layout
    {
        // ---- cross-section (m)
        public const double X0 = BulwarkConst.GalleryWallXMin;            // -3.5 west rib face (clear profile)
        public const double X1 = BulwarkConst.GalleryWallXMax;            // 0.5 east rib face
        public const double H = BulwarkConst.GalleryClearHeight;          // 3.6 rib soffit
        public const double R = BulwarkConst.GalleryCornerRadius;         // 0.3 top corners
        public const double Recess = BulwarkConst.RibProud;               // 0.15 panels behind the rib plane
        public const double PanelX0 = X0 - Recess, PanelX1 = X1 + Recess, PanelH = H + Recess;   // -3.65, 0.65, 3.75
        public const double RibHalf = 0.5 * BulwarkConst.RibWidth;        // 0.2
        public const double Bevel = 0.02;                                 // rib edge chamfer (14.5 common: 1-3 cm within 3 m)
        public const int ArcSegments = 8;
        public const double ShellX0 = BulwarkConst.ShellXMin, ShellX1 = BulwarkConst.ShellXMax, ShellTop = BulwarkConst.ShellYMax;
        public const double SouthWallZ = -0.4;                            // inner face of the south end wall (graybox: z -0.6 .. -0.4)
        public const double DoorZ = BulwarkConst.DoorPlaneZ, DoorZ1 = BulwarkConst.DoorWallNorthZ;   // 120, 120.7
        public const double FloorTop = 0.0, FloorBottom = -0.5;

        // ---- ribs, slits, lamps, niches (14.5.1)
        public static int RibCount => (int)Math.Floor((DoorZ - BulwarkConst.RibZ0) / BulwarkConst.RibPitch) + 1;   // z 3 .. 117: 20
        public static double RibZ(int k) => BulwarkConst.RibZ0 + BulwarkConst.RibPitch * k;
        public static double SlitZ(int k) => BulwarkConst.SlitZ0 + BulwarkConst.SlitPitch * k;           // 6 + 12 k, k 0-9
        public static double LampZ(int k) => BulwarkConst.GalleryLampPitch * k;                          // 12 k, k 1-9
        public const double PilasterHalf = 0.8;                           // slit pilaster 1.6 m wide (between ribs 3.2 .. 8.8 m apart)
        public const double SlitHalf = 0.5 * BulwarkConst.SlitWidth;      // 0.15
        public const double SlitBack = BulwarkConst.SlitX + BulwarkConst.SlitDepth;   // 1.7 baffle face
        public const double HoodX1 = SlitBack + 0.25;                     // 1.95 baffle back (outside the shell, on the deck)
        public const double HoodHalfZ = 0.6, HoodY0 = BulwarkConst.SlitY0 - 0.4, HoodY1 = BulwarkConst.SlitY1 + 0.4;
        public static int NicheCount
        {
            get { int n = 0; for (double z = NicheZ0; z + 0.5 * BulwarkConst.NicheW < DoorZ; z += BulwarkConst.NichePitch) n++; return n; }
        }
        public const double NicheZ0 = 6.0;                                // 6 + 24 k: the valve wheels of z 30 and 78 sit in niches
        public static double NicheZ(int k) => NicheZ0 + BulwarkConst.NichePitch * k;
        public const double NicheBackX = -3.85;                           // see the header (ambient box arithmetic)
        public const double NicheHalf = 0.5 * BulwarkConst.NicheW, NicheTop = BulwarkConst.NicheH;

        // ---- lamps: 0.45 x 0.2 x 0.2 caged lamps on the centre line, hung 0.2 m below the ceiling panel (KS01 audit box
        // y 3.35 .. 3.55); pivot at the ceiling for the sway
        public const double LampX = BulwarkConst.StartX;
        public const double LampY0 = H - 0.25, LampY1 = H - 0.05;         // 3.35 .. 3.55
        public const double LampPivotY = PanelH;                          // 3.75
        public const double LampLightY = LampY0 - 0.25;                   // real light 0.25 m under the glass
        public const float LampRange = 9f, LampIntensity = 1.6f;          // 14.5.1 "four nearest lamps are real point lights (9 m, 1.6)"
        // ---- the door-bay lamp (Wave X resume): a tenth caged lamp of the same design between the last rib (z 117) and the
        // door housing (z 119.4). The lamp of z 108 is 11.4 m from the housing and reaches 9 m, so the last bay was lit by the
        // opening alone (KS03 mean luma 65 against 70-120; seq_b01_b02 step 2.37 levels per frame at s 117.5 where the dark
        // bay walls leave the frame edges). Raised 0.1 m so its glass bottom (y 3.45) projects above the door head from KS01:
        // y = 1.7 + 1.75 x 112 / 110.3 = 3.48 > 3.40 (the door subject stays unoccluded).
        public const int LampTotal = BulwarkConst.GalleryLampCount + 1;
        public const double DoorLampZ = 118.3, DoorLampRaise = 0.1;
        /// <summary>Door-bay lamp real light: 2.0 (interiors-18: KS03 21:9 mean luma 67.9 at 1.6), placed 0.05 m lower than the
        /// raise (y 3.15, 0.60 m under the ceiling panel) so the soffit hot spot stays at the 1.6 / 0.55^2 of build 18
        /// (2.0 / 0.60^2 = 5.6 against 5.3; pose_dbay ceiling p99 182 levels).</summary>
        public const float DoorLampIntensity = 2.0f;
        public const double DoorLampLightDrop = 0.05;
        /// <summary>Real-light intensity of lamp index i.</summary>
        public static float LampIntensityAt(int i) => i < BulwarkConst.GalleryLampCount ? LampIntensity : DoorLampIntensity;
        /// <summary>z of lamp index i (0-based; 0..8 = the nine lamps of 14.5.1, 9 = the door-bay lamp).</summary>
        public static double LampZAt(int i) => i < BulwarkConst.GalleryLampCount ? LampZ(i + 1) : DoorLampZ;
        /// <summary>Vertical offset of lamp index i (0 for the nine, DoorLampRaise for the door-bay lamp).</summary>
        public static double LampRaiseAt(int i) => i < BulwarkConst.GalleryLampCount ? 0.0 : DoorLampRaise;

        // ---- west wall fittings (outside the capsule reach: x <= -3.5)
        public const double PipeR = 0.06;                                 // two pipes 0.12 m on brackets
        public const double PipeX = PanelX0 + 0.075;                      // axis -3.575: outer surface -3.515
        public static readonly double[] PipeY = { 2.38, 2.62 };
        public const double TrayY0 = 3.0, TrayY1 = 3.1, TrayDepth = 0.14; // cable tray x -3.65 .. -3.51
        public const double BandY0 = 1.05, BandY1 = 1.15;                 // ochre band at y 1.1 (paint, 6 mm proud of the panel)
        public const double DrainX0 = 0.2, DrainX1 = 0.48, DrainDepth = 0.14;   // floor drain gutter along the east wall
        public const double ValveY = 1.3;                                 // valve wheel axis height in the niche

        // ---- door (14.5.1, 14.12)
        public const double DoorX0 = BulwarkConst.DoorOpeningXMin, DoorX1 = BulwarkConst.DoorOpeningXMax;   // -3.5 .. 0.5
        public const double DoorCx = 0.5 * (DoorX0 + DoorX1);             // -1.5
        public const double LeafW = BulwarkConst.DoorLeafW, LeafH = BulwarkConst.DoorLeafH, LeafT = BulwarkConst.DoorLeafT;   // 2.0, 3.5, 0.7
        public const double OpenH = BulwarkConst.DoorOpeningHeight;      // 3.4
        public const double PocketX0 = DoorX0 - LeafW - 0.2, PocketX1 = DoorX1 + LeafW + 0.2;   // -5.7 .. 2.7 housings
        public const double HousingZ0 = DoorZ - 0.6, HousingZ1 = DoorZ1 + 0.3;                    // 119.4 .. 121.0
        public const double HandWheelR = 0.5 * BulwarkConst.DoorHandWheel; // 0.3 (on the east panel beside the door)
        public const double HandWheelZ = 118.4, HandWheelY = 1.35;

        /// <summary>Progress-keyed door gap (m) at s (the fallback and the capture rule).</summary>
        public static double DoorGapProgress(double s)
            => BulwarkConst.DoorWidth * BwMath.Smootherstep01((s - BulwarkConst.DoorTriggerS) / (BulwarkConst.DoorOpenTime * BulwarkConst.Cine));

        /// <summary>Timed door gap (m) t seconds after the trigger (14.5.1: 4.0 smootherstep(t / 3.0)); 0 before it.</summary>
        public static double DoorGapTimed(double t) => t <= 0.0 ? 0.0 : BulwarkConst.DoorWidth * BwMath.Smootherstep01(t / BulwarkConst.DoorOpenTime);

        /// <summary>The door gap of a walk: max of the timed rule (t since the trigger, NaN = not triggered) and the progress fallback.</summary>
        public static double DoorGap(double s, double tSinceTrigger)
        {
            double p = DoorGapProgress(s);
            if (double.IsNaN(tSinceTrigger)) return p;
            return Math.Max(p, DoorGapTimed(tSinceTrigger));
        }

        // ---- lamp sway (14.5.1 "Moving": +-1.2 deg, 0.8 Hz, decay 3 s per impact; A1 K0: +-0.6 deg at s 66, no boom)
        /// <summary>Sway angle (deg) t seconds after a kick of amplitude a (deg): a e^(-t/3) sin(2 pi 0.8 t), ramped in over
        /// 0.06 s (reaches 1.2 +-0.3 deg within 0.1 s at a = 1.6: acceptance 4).</summary>
        public static double SwayDeg(double t, double a)
        {
            if (t <= 0.0) return 0.0;
            double ramp = Math.Min(1.0, t / 0.06);
            return a * ramp * Math.Exp(-t / BulwarkConst.LampSwayDecay) * Math.Sin(2.0 * Math.PI * BulwarkConst.LampSwayHz * t + 0.5 * Math.PI * ramp);
        }

        /// <summary>Kick amplitude (deg) of an impact of strength 0..1 (1.2 deg at the typical 0.75 impact, capped at 1.5).</summary>
        public static double ImpactKickDeg(double strength) => Math.Min(1.5, BulwarkConst.LampSwayDeg * Math.Max(0.0, strength) / 0.75);

        public const double K0S = 66.0, K0KickDeg = 0.6;                  // A1 1.6 K0: lamps sway +-0.6 deg at s 66 (no boom)

        // ---- slit storm light (+-25 % with the impact clock)
        /// <summary>Slit light gain at t seconds after an impact of strength st at distance dz (m) along the wall: 1 + 0.25 st
        /// x (0.6 s rise, 2.4 s fall) x a 30 m falloff. Base 1; the sum of impacts is clamped to 1.25.</summary>
        public static double SlitFlare(double t, double st, double dz)
        {
            if (t < 0.0 || t > 3.0) return 0.0;
            double env = t < 0.6 ? BwMath.Smootherstep01(t / 0.6) : 1.0 - BwMath.Smootherstep01((t - 0.6) / 2.4);
            return 0.25 * Math.Max(0.0, Math.Min(1.0, st)) * env * Math.Exp(-(dz * dz) / (2.0 * 30.0 * 30.0));
        }

        // ---- the clear profile: rib (offset 0) and panel (offset Recess) polylines, west floor -> ceiling -> east floor
        /// <summary>Point i (0..ProfileCount-1) of the inner profile offset outward by d (m): x, y and the inward normal.</summary>
        public static void Profile(int i, double d, out double x, out double y, out double nx, out double ny)
        {
            // 0: west foot, 1: west arc start, 1..1+A: west arc, 2+A..: east arc, last: east foot
            int a = ArcSegments;
            double cW = X0 + R, cE = X1 - R, cy = H - R, rr = R + d;
            if (i <= 0) { x = X0 - d; y = 0.0; nx = 1; ny = 0; return; }
            if (i <= a + 1)
            {
                double t = (i - 1) / (double)a;                         // 0 -> 1: 180 deg -> 90 deg
                double ang = Math.PI * (1.0 - 0.5 * t);
                x = cW + rr * Math.Cos(ang); y = cy + rr * Math.Sin(ang); nx = -Math.Cos(ang); ny = -Math.Sin(ang); return;
            }
            if (i <= 2 * a + 2)
            {
                double t = (i - (a + 2)) / (double)a;                   // 0 -> 1: 90 deg -> 0 deg
                double ang = 0.5 * Math.PI * (1.0 - t);
                x = cE + rr * Math.Cos(ang); y = cy + rr * Math.Sin(ang); nx = -Math.Cos(ang); ny = -Math.Sin(ang); return;
            }
            x = X1 + d; y = 0.0; nx = -1; ny = 0;
        }

        public static int ProfileCount => 2 * ArcSegments + 4;

        /// <summary>Clear width and height of the rib profile (acceptance 1: 4.0 x 3.6 +-0.02).</summary>
        public static void ClearSize(out double w, out double h)
        {
            double minX = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            for (int i = 0; i < ProfileCount; i++)
            {
                Profile(i, 0.0, out double x, out double y, out _, out _);
                minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
            }
            w = maxX - minX; h = maxY;
        }

        /// <summary>Lane reach of the corridor (capsule surface): lane x limits +- PLAYER_R.</summary>
        public static void CapsuleReach(out double xMin, out double xMax)
        {
            xMin = BulwarkConst.LaneCorridorXMin - BulwarkConst.PlayerR;
            xMax = BulwarkConst.LaneCorridorXMax + BulwarkConst.PlayerR;
        }

        /// <summary>Innermost x of every west-wall fitting (pipes, tray, band, valve wheels, niche lining) and outermost x of the
        /// east fittings (slit pilasters, hand wheel): the capsule-clearance test compares them with CapsuleReach.</summary>
        public static double WestFittingsMaxX => Math.Max(PipeX + PipeR, Math.Max(PanelX0 + TrayDepth, Math.Max(PanelX0 + 0.006, NicheBackX + 0.25)));
        public static double EastFittingsMinX => Math.Min(BulwarkConst.SlitX, PanelX1 - 0.12);
    }
}
