// Beat06Layout: the pure geometry and timeline of beat 6, the window gallery (MASTER_PROMPT 14.5.6, 14.7 H3; Addendum A1 1.8
// with LEAD DECISIONS 2-4). Double precision, no UnityEngine (Beat06Tests check every number here). Coordinates: lateral is
// metres from the walker axis toward the sea (BwPath.World), y world, s the path coordinate (the gallery leg is straight).
//
// Hall (14.5.6): s 1070-1370, floor BwPath.FloorY (ramp 0.12, -54 at 1070, -90 at 1370), west wall at lateral -5.5, ceiling
// floor + 28, the glazed trench lateral 3.0-6.0, 8 m deep, rail at 3.0 (tops 1.1 / 0.55), the glass from the trench bottom
// to the ceiling (36 m) at lateral 6.0 (inner face) .. 8.4 (outer face). Ten panes 20 m wide at s 1085 + 30 k between 10 m
// piers; each pane's opening runs straight through the glass and flares 45 deg for 3.5 m to the dam's sea face (lateral
// 11.9): the "conical reveals 3.5 m deep" on the water side (they hold the survey lamps, A1 1.8).
//
// Encounter (A1 1.8, re-staged for the real Tripo head, LEAD DECISION 4): the main eye's socket sits 20.6 m inside the head's
// hull along its axis (brow, mantle and arm roots), so with the head 0.3 m clear of the outer glass inside the pane band the
// eye CENTRE can come no nearer than 23.3 m to the outer face (orientation search over heading/pitch/roll in 5 deg steps,
// eye axis within 40 deg of the glass normal: best heading 190, pitch 10, roll 10). The hold is therefore at 25.5 m (1.5 m
// margin for the body's 2.2 m heave and sway): eye centre lateral 33.9 at z 1268, y -75 instead of A1's 24.4; the iris (12 m)
// spans 2 atan(6 / 33.9) = 20.1 deg = 678 px at 4K (339 at 1080p) at KS17 instead of 1296. Fix wave (critic S2-2): the main
// eyeballs are scaled x1.63 in their sockets inside the hall (Beat06Encounter.MainEyeScale): 651 px at 1080p = 1302 at 4K.
using System;

namespace Megastructure.Beats
{
    public static class Beat06Layout
    {
        public const double S0 = 1070.0, S1 = 1370.0;
        public const double West = -5.5, Rail = 3.0, GlassIn = 6.0, GlassOut = 8.4, RevealEnd = 11.9;
        public const double Height = 28.0, Trench = 8.0;
        public const int Panes = 10;
        public const double PaneS0 = 1085.0, PanePitch = 30.0, PaneWidth = 20.0, PaneBottom = -8.0, PaneTop = 28.0;
        public const double RailTop = 1.1, RailMid = 0.55, RailSetback = 0.06, RailDiameter = 0.10;
        public const double BollardS0 = 1080.0, BollardPitch = 20.0, BollardHeight = 0.9;
        public const int Bollards = 15;
        public const double HatchS0 = 1100.0, HatchPitch = 30.0, HatchW = 1.4, HatchH = 2.3;

        public static double PaneS(int k) => PaneS0 + PanePitch * k;
        /// <summary>Pane k (0-9) whose 20 m span contains s, else -1.</summary>
        public static int PaneAt(double s)
        {
            for (int k = 0; k < Panes; k++) if (Math.Abs(s - PaneS(k)) <= 0.5 * PaneWidth) return k;
            return -1;
        }
        public static double FloorY(double s) => BwPath.FloorY(s);

        // ---------------------------------------------------------------- survey lamps (A1 1.8 table)
        /// <summary>Lamp mounts: s of the pier centre, lateral on the pier's outer face (+0.3), height above the floor at s.
        /// Fix wave (critic S2-1): lamp A is mounted LOW, at the pressed arm's height (floor + 3; the arm's band crosses pane 5
        /// at eye height, Beat06Encounter), so its light reaches the suckers over 2-8 m of water instead of 8-20 m: the beam c
        /// of red (0.34 /m) leaves e^-0.7..e^-2.7 of the red there instead of e^-2.7..e^-6.8, so the underside reads red near
        /// the lamp and teal farther along the arm (A1 1.8 "the red underside, pale hooked suckers").</summary>
        public const double LampAS = 1220.0, LampBS = 1250.0, LampLateral = RevealEnd + 0.3;
        public const double LampAHeight = 3.0, LampBHeight = 3.6;
        /// <summary>Lamp aims (deg). Lamp A is back at the A1 aim (yaw 20, pitch +5): from its low mount it grazes ALONG the dam
        /// face under the pressed arm (the arm lies on the plane GlassPlaneLateral, 0.7 m outside the lamp), so the sucker rims
        /// facing it catch the light in rows along pane 5.</summary>
        public const double LampAYaw = 20.0, LampAPitch = 5.0, LampBYaw = 45.0, LampBPitch = -8.0;
        public const double LampHalfAngle = 28.0, LampRange = 45.0;
        public const double LampAClick = 1204.0, LampAFull = 1213.0, LampAOff0 = 1236.0, LampAOff1 = 1245.0;
        public const double LampBClick = 1247.0, LampBFull = 1256.0, LampBOff0 = 1278.0, LampBOff1 = 1287.0;
        /// <summary>Log-linear ramp floor: the ramps run from 1 % to 100 % of the luminance (2 decades over 9 m of s).</summary>
        public const double RampFloor = 0.01;

        /// <summary>Lamp gain 0..1 at s: off before the click, a log-linear rise (RampFloor -> 1) from the click to full with a
        /// soft start over the first 15 %, full, then the mirrored fall. Continuous in s (no step anywhere but at the click
        /// itself, where the gain starts from 0 through the soft start).</summary>
        public static double LampGain(double s, double click, double full, double off0, double off1)
        {
            if (s <= click || s >= off1) return 0.0;
            if (s < full) return Ramp((s - click) / (full - click));
            if (s <= off0) return 1.0;
            return Ramp((off1 - s) / (off1 - off0));
        }

        static double Ramp(double x)
        {
            x = Clamp01(x);
            double g = Math.Pow(RampFloor, 1.0 - x);
            return g * Smooth(0.0, 0.15, x);
        }

        public static double LampA(double s) => LampGain(s, LampAClick, LampAFull, LampAOff0, LampAOff1);
        public static double LampB(double s) => LampGain(s, LampBClick, LampBFull, LampBOff0, LampBOff1);

        // ---------------------------------------------------------------- water state keyed to s
        /// <summary>Upper-veil gain while the arm crosses above panes 1-3 (A1 1.8 "the light goes out"; the arm itself is
        /// CrossArmAt, drawn by the portal, and its shadow cuts the shafts): 1 -> 0.62 over s 1122-1140, held while the arm
        /// lies above the panes (to s 1172, so KS16 at 1166 still has it), back to 1 by 1190 as it drifts out.</summary>
        public static double VeilGain(double s) => 1.0 - 0.38 * Smooth(1122.0, 1140.0, s) * (1.0 - Smooth(1172.0, 1190.0, s));

        /// <summary>Displacement of the snow along +s (m): the bulk flow 0 -> 0.3 m/s over s 1104-1112, decaying to 0 by 1160
        /// (A1 1.8, "something huge displaces the water"), integrated in s at the cinematic 2.5 m/s (keyed to s).</summary>
        public static double SnowBulk(double s)
        {
            // v(s) = 0.3 a(s) with a = smooth rise 1104-1112 times smooth fall 1112-1160; displacement = integral v ds / 2.5
            const int N = 64;
            double s0 = 1104.0, s1 = Math.Min(Math.Max(s, s0), 1160.0);
            if (s1 <= s0) return 0.0;
            double h = (s1 - s0) / N, acc = 0.0;
            for (int i = 0; i < N; i++)
            {
                double x = s0 + (i + 0.5) * h;
                acc += 0.3 * Smooth(1104.0, 1112.0, x) * (1.0 - Smooth(1112.0, 1160.0, x)) * h;
            }
            return acc / 2.5;
        }

        /// <summary>Amber of the shaft tops near the end of the hall (14.5.6, s 1340-1370).</summary>
        public static double Amber(double s) => Smooth(1340.0, 1370.0, s);

        /// <summary>Glass-reflection sway amplitude (deg) while something presses or strikes (A1 1.8: +-0.05 deg at 0.6 Hz).</summary>
        public static double SwayDeg(double s)
        {
            double press = Smooth(1205.0, 1212.0, s) * (1.0 - Smooth(1238.0, 1246.0, s));
            double eye = Smooth(1251.0, 1256.0, s) * (1.0 - Smooth(1278.0, 1287.0, s)) * 0.5;
            double strike = Smooth(1299.0, 1300.5, s) * (1.0 - Smooth(1302.0, 1312.0, s)) * 1.6;
            return 0.05 * Math.Min(1.6, Math.Max(press, Math.Max(eye, strike)));
        }

        // ---------------------------------------------------------------- the creature's path (keyed to s)
        /// <summary>Main-eye hold: centre lateral 33.9 (25.5 m off the outer glass), y -75, s 1268 (see the header).</summary>
        public const double EyeHoldLateral = GlassOut + 25.5, EyeHoldY = -75.0, EyeHoldS = 1268.0;
        /// <summary>Orientation of the body for the whole encounter (Unity Euler: pitch, heading, roll; the search optimum).</summary>
        public const double BodyHeading = 190.0, BodyPitch = 10.0, BodyRoll = 10.0;

        /// <summary>Key poses of the main eye centre (s, lateral, y, z along s): far and deep, the dim approach seen through
        /// pane 5 (33-48 m, A1 "eyes in the dark"), the hold abeam pane 6, the slow withdrawal, the sink along a 30 deg slope
        /// (KS18: yaw 63.1, pitch -17.7 from the KS18 eye: at -27 the trench edge hides it), gone. A C1 Hermite (EyeAt): peak 7.8 m/s at
        /// 2.5 m/s walking
        /// (LEAD DECISION 2; Beat06Tests).</summary>
        public static readonly double[] EyeKeys =
        {
            // s,     lateral, y,      z
            1070.0,  190.0,  -150.0, 1330.0,
            1190.0,  150.0,  -130.0, 1262.0,
            1224.0,   75.0,   -95.0, 1262.0,
            1241.0,   47.0,   -79.5, 1264.5,
            1252.0, EyeHoldLateral, EyeHoldY, EyeHoldS,
            1280.0, EyeHoldLateral, EyeHoldY + 1.0, EyeHoldS + 3.0,
            1306.0,   43.0,   -88.0, 1328.0,
            1320.0,   49.0,  -100.0, 1345.0,
            1338.0,   55.0,  -122.0, 1353.0,
            1350.0,   60.0,  -140.0, 1357.0,
            1372.0,   75.0,  -172.0, 1364.0,
        };
        /// <summary>Keys with zero velocity (the start, the hold from 1252 to 1280, the end). The approach keys (1224, 1241) and
        /// the hold start (1252) are timed so that the sucker arm, which hangs off the body, also stays under 8 m/s: its points
        /// within 60 m of the walker peak at 7.75 m/s at 2.5 m/s (player sweep gallery-mot33, every 1 m of s, -b6-debug-motion).</summary>
        static bool HoldKey(int i, int n) => i == 0 || i == n - 1 || i == 4 || i == 5;

        /// <summary>Eye centre (lateral, y, z) at s: a C1 cubic Hermite through the keys (central-difference tangents, zero at
        /// the hold keys), so the eye never stops at a passing key and never jerks. Allocation-free.</summary>
        public static void EyeAt(double s, out double lateral, out double y, out double z)
        {
            var k = EyeKeys;
            int n = k.Length / 4;
            if (s <= k[0]) { lateral = k[1]; y = k[2]; z = k[3]; return; }
            if (s >= k[4 * (n - 1)]) { lateral = k[4 * n - 3]; y = k[4 * n - 2]; z = k[4 * n - 1]; return; }
            int i = 0;
            while (i + 2 < n && s > k[4 * (i + 1)]) i++;
            double a = k[4 * i], b = k[4 * (i + 1)], h = b - a, t = (s - a) / h;
            double h00 = 2 * t * t * t - 3 * t * t + 1, h10 = t * t * t - 2 * t * t + t, h01 = -2 * t * t * t + 3 * t * t, h11 = t * t * t - t * t;
            lateral = h00 * k[4 * i + 1] + h10 * h * Tangent(i, 0, n) + h01 * k[4 * (i + 1) + 1] + h11 * h * Tangent(i + 1, 0, n);
            y = h00 * k[4 * i + 2] + h10 * h * Tangent(i, 1, n) + h01 * k[4 * (i + 1) + 2] + h11 * h * Tangent(i + 1, 1, n);
            z = h00 * k[4 * i + 3] + h10 * h * Tangent(i, 2, n) + h01 * k[4 * (i + 1) + 3] + h11 * h * Tangent(i + 1, 2, n);
        }

        static double Tangent(int i, int c, int n)
        {
            if (HoldKey(i, n)) return 0.0;
            var k = EyeKeys;
            return (k[4 * (i + 1) + 1 + c] - k[4 * (i - 1) + 1 + c]) / (k[4 * (i + 1)] - k[4 * (i - 1)]);
        }

        /// <summary>Pupil roundness 1 (dark, round) .. 0 (slit): it snaps to the slit over 3 m of s (1.2 s at 2.5 m/s) as lamp
        /// B reaches full (s 1256), opens again as the lamp dies (1287-1292).</summary>
        public static double Pupil(double s) => 1.0 - Smooth(1254.5, 1257.5, s) * (1.0 - Smooth(1285.0, 1292.0, s));

        /// <summary>The slow blink at s 1272 (A1 1.2: close 1.2 s, hold 0.6 s, open 1.5 s, here in s at 2.5 m/s); -1 = automatic.
        /// Fix wave (critic S3-9): the lids close again as it withdraws (s 1284-1288, behind pier 6|7 from 1278 for the walker)
        /// and stay closed while the scaled eyeball shrinks back to its socket size (EyeScaleAt, 1288-1296); they open again
        /// over 1302-1308, so the sinking eyes are open at KS18 (1320).</summary>
        public static double Blink(double s)
        {
            if (s < 1240.0 || s > 1310.0) return -1.0;
            if (s < 1272.0) return 0.0;
            if (s < 1275.0) return Smooth(1272.0, 1275.0, s);
            if (s < 1276.5) return 1.0;
            if (s < 1280.25) return 1.0 - Smooth(1276.5, 1280.25, s);
            if (s < 1284.0) return 0.0;
            if (s < 1302.0) return Smooth(1284.0, 1288.0, s);
            return 1.0 - Smooth(1302.0, 1308.0, s);
        }

        /// <summary>Main eyeball scale factor at s (0 = the manifest size, 1 = Beat06Encounter.MainEyeScale): full for the
        /// approach and the hold, back to the socket size under the closed lids over s 1288-1296 (critic S3-9: at p1290 the
        /// scaled eyeball, seen from the side as it withdraws, stood out of the head's silhouette).</summary>
        public static double EyeScaleAt(double s) => 1.0 - Smooth(1288.0, 1296.0, s);

        /// <summary>Withdraw preset weight: 0 (the withdrawal is the body's path; a Withdraw weight beside the glass arm's
        /// GlideAlong would halve the rig's glass clamp by the preset normalisation).</summary>
        public static double Withdraw(double s) => 0.0;

        /// <summary>Sucker arm on the glass (A1 1.8): GlideAlong weight of the masked limb, a slow 20 s rise from the black over
        /// s 1150-1200, then pressed flat across panes 5-9 for the rest of the hall: it follows the body's slow path (<= 7.8
        /// m/s) and never lets go in view (a release would swing ~250 m of arm through the panes faster than 8 m/s, LEAD
        /// DECISION 2; measured in the player: 20 m/s over s 1290-1320 with a 30 m fade).</summary>
        public static double GlassArm(double s) => Smooth(1150.0, 1200.0, s);
        /// <summary>Aim height above the floor at s 1236 for the slide of the sucker arm (Beat06Encounter, which now cancels the
        /// rig's slow 41 s slide sweep as well as its fixed turn). With the sweep cancelled an aim of floor + 8 puts the arm's
        /// axis at u 450 at floor + 7 at s 1236 (player log -b6-debug, gallery-h8: u450 (1233, 19.4, -67), floor -73.9): the
        /// pressed underside (radius ~9 m) lies across pane 5 at eye height, lamp A (floor + 10, pitch +5, half-angle 28)
        /// grazes its north part (the lit sucker rows of pose p1225), and the far arm stays under the sea (u800 y -13; aim 13
        /// gave u800 y +6, out of the water).</summary>
        public static double GlassArmHeight(double s) => 8.0;
        /// <summary>Slide direction of the sucker arm on the glass: toward -s (the walker), rising at this angle (deg).</summary>
        public const double GlassArmSlideDeg = 15.0;
        /// <summary>Lateral of the plane the sucker arm is pressed against (the rig keeps the masked limb >= its radius water-side
        /// of it): 1 m outside the dam's sea face (RevealEnd 11.9), not 1 m outside the glass. Critic S2-4: on the old plane
        /// (lateral 9.4) the pressed underside passed through the piers between the reveals and, while the GlideAlong weight
        /// was still rising (s 1150-1200), the limb's flank showed over the pier jambs and the ceiling beam (KS16). On this
        /// plane the arm lies across the reveal openings on the dam face, seen through them; no part of it can enter a pier.</summary>
        public const double GlassPlaneLateral = RevealEnd + 1.0;
        /// <summary>Where the pressed-arm plane starts (lateral, m): out in the black (T_G e^-0.085 x 58 = 0.007 at the dam face).
        /// Critic S2-4: a GlideAlong weight below 1 only pushes the limb part of the way out of the plane (KrakenRig: Pn += n
        /// (r - dist) w), so while the weight rose over s 1150-1200 the limb could cross the hall (KS16). The weight is now 1
        /// for the whole hall and the arm "rises out of the black" by the PLANE coming in: GlassPlaneAt moves it from here to
        /// GlassPlaneLateral over s 1150-1200 (GlassArm), at most 57.1 x 1.875 / 50 = 2.14 m per m of s = 5.4 m/s at 2.5 m/s.</summary>
        public const double GlassArmFarLateral = 70.0;
        /// <summary>Lateral of the pressed-arm plane at s (GlassArmFarLateral -> GlassPlaneLateral with GlassArm).</summary>
        public static double GlassPlaneAt(double s) => GlassArmFarLateral + (GlassPlaneLateral - GlassArmFarLateral) * GlassArm(s);

        // ---------------------------------------------------------------- the arm above panes 1-3 (A1 1.8, s 1122-1172)
        /// <summary>"The light goes out": an arm lies along the dam face above panes 1-3 (y -40 = 32 m depth, axis 9-13 m
        /// beyond the outer glass), its tip sliding south from beyond pane 0 to pane 3 while the walker passes, then drifting
        /// out and up into the black. Keys (s, tip s, tip lateral, tip y); a C1 Hermite with zero tangents at both ends; the
        /// rest of the arm hangs off the tip rigidly (CrossArmKnee / CrossArmRoot offsets), so every point moves with the
        /// tip: peak 7.4 m/s at 2.5 m/s (LEAD DECISION 2: <= 8; Beat06Tests). KS16 (s 1166, yaw 70, pitch 40): the tip is at
        /// s 1184 (yaw 44 from the KS16 eye, axis elevation 53 deg), so the underside crosses the whole view at 26-30 m slant
        /// (A1 "a soft dark band against the bright upper veil"). The portal draws it as a round-cone SDF in the water
        /// (BwWater.hlsl BwWArmTrace), never as a mesh: no root, no body to stage, nothing that can enter the hall.</summary>
        public static readonly double[] CrossArmKeys =
        {
            // s,     tip s,   lateral, y
            1076.0,   990.0,   70.0,   -40.0,
            1118.0,  1052.0,   26.0,   -40.0,
            1146.0,  1128.0,   20.0,   -40.0,
            1172.0,  1196.0,   21.0,   -40.0,
            1208.0,  1228.0,   78.0,   -26.0,
        };
        /// <summary>Window in which the portal draws the arm (outside it the arm is parked 60-70 m beyond the glass: T_G e^-5).</summary>
        public const double CrossArmS0 = 1072.0, CrossArmS1 = 1212.0;
        /// <summary>Radii (m): the tip, the mid 30 m back, the knee 90 m back, the root 190 m back (9 m near the crown).</summary>
        public const double CrossArmTipR = 1.2, CrossArmMidR = 3.2, CrossArmKneeR = 6.5, CrossArmRootR = 9.0;
        /// <summary>Mid, knee and root relative to the tip (s, lateral, y): the free end droops 4 m and curls 1.5 m toward the
        /// glass, then the arm runs back north along the face and out into the black.</summary>
        public static readonly double[] CrossArmMid = { -30.0, 1.5, 4.0 };
        public static readonly double[] CrossArmKnee = { -90.0, 8.0, 2.0 };
        /// <summary>Slow sway of the free end on the ambient clock (amplitude m, period s; <= 0.27 m/s): it lives while the
        /// walker stands still.</summary>
        public const double CrossArmSwayY = 1.0, CrossArmSwayYPeriod = 23.0, CrossArmSwayL = 0.6, CrossArmSwayLPeriod = 31.0;
        public static readonly double[] CrossArmRoot = { -170.0, 95.0, 4.0 };

        /// <summary>Tip of the arm above panes 1-3 at s (tip s, lateral, y): C1 Hermite through CrossArmKeys. Allocation-free.</summary>
        public static void CrossArmAt(double s, out double tipS, out double lateral, out double y)
        {
            var k = CrossArmKeys;
            int n = k.Length / 4;
            if (s <= k[0]) { tipS = k[1]; lateral = k[2]; y = k[3]; return; }
            if (s >= k[4 * (n - 1)]) { tipS = k[4 * n - 3]; lateral = k[4 * n - 2]; y = k[4 * n - 1]; return; }
            int i = 0;
            while (i + 2 < n && s > k[4 * (i + 1)]) i++;
            double a = k[4 * i], b = k[4 * (i + 1)], h = b - a, t = (s - a) / h;
            double h00 = 2 * t * t * t - 3 * t * t + 1, h10 = t * t * t - 2 * t * t + t, h01 = -2 * t * t * t + 3 * t * t, h11 = t * t * t - t * t;
            tipS = h00 * k[4 * i + 1] + h10 * h * ArmTangent(i, 0, n) + h01 * k[4 * (i + 1) + 1] + h11 * h * ArmTangent(i + 1, 0, n);
            lateral = h00 * k[4 * i + 2] + h10 * h * ArmTangent(i, 1, n) + h01 * k[4 * (i + 1) + 2] + h11 * h * ArmTangent(i + 1, 1, n);
            y = h00 * k[4 * i + 3] + h10 * h * ArmTangent(i, 2, n) + h01 * k[4 * (i + 1) + 3] + h11 * h * ArmTangent(i + 1, 2, n);
        }

        static double ArmTangent(int i, int c, int n)
        {
            if (i == 0 || i == n - 1) return 0.0;
            var k = CrossArmKeys;
            return (k[4 * (i + 1) + 1 + c] - k[4 * (i - 1) + 1 + c]) / (k[4 * (i + 1)] - k[4 * (i - 1)]);
        }

        /// <summary>Visible range of the encounter's creature (it is hidden elsewhere; the crest's kraken is another owner's).</summary>
        public const double CreatureS0 = 1066.0, CreatureS1 = 1372.0;

        // ---------------------------------------------------------------- kraken calls (A1 1.8 / 2.4; the audio owner reads them)
        public enum CallId { FarVoice, GlassCreak, SuckersAttach, CloseVoice, SuckersRegrip, EyeSwell, EyeGroan, NearVoice, WithdrawVoice, DamShudder, DeepGroan }
        /// <summary>(s of the event, duration at 2.5 m/s in s, strength) per CallId, in s order. Fix wave (critic S3-7 / S3-8):
        /// strengths lowered to the A1 2.5 ceilings (the x2 autotest measured 2-4 dB over), the close voice moved to 1220 (it
        /// leads the first eyes, 1228-1232, by 3-8 s), the release at 1236 replaced by a slow re-grip (the arm never lets go in
        /// view), an underwater call at 1262 (the hold) and a receding voice at 1288 (the withdrawal: no gap over 3 s at 1.5 m/s
        /// before the strike, critic S2-6).</summary>
        public static readonly double[] Calls =
        {
            1104.0, 8.0, 0.38,    // far voice through the glass (Call, underwater)
            1196.0, 18.0, 0.45,   // the glass groans as the arm presses (GlassCreak; s-keyed sustain to 1245)
            1206.0, 12.0, 0.6,    // suckers attach in a travelling wave (SuckerPops; s-keyed sustain to 1245)
            1220.0, 6.0, 0.6,     // the close voice begins (Call, underwater)
            1236.0, 6.0, 0.5,     // the suckers re-grip, slowly (SuckerPops)
            1251.0, 14.0, 0.7,    // it is there, lit: silence, then the swell (EyeSwell; s-keyed sustain to 1287)
            1256.0, 6.0, 0.6,     // glass groan + close voice as the pupil narrows (GlassCreak)
            1262.0, 6.0, 0.6,     // the close call during the hold (Call, underwater)
            1288.0, 8.0, 0.45,    // the voice recedes with the withdrawal (Call, underwater)
            1300.0, 4.0, 0.8,     // the strike ahead (DamShudder)
            1312.0, 8.0, 0.6,     // the deepest groan (Call, underwater)
        };
        /// <summary>s until which a call is held (an s-keyed sustain, then CallRelease seconds of release); 0 = its own
        /// duration only. The glass and the suckers hold while the arm is pressed and lamp A lights it (to 1245), the swell
        /// while the eye holds (to 1287).</summary>
        public static readonly double[] CallHoldUntil = { 0.0, 1245.0, 1245.0, 0.0, 0.0, 1287.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        public const double CallRelease = 3.0;
        /// <summary>Walking-speed range the call durations are scaled over (the table is tuned at 2.5 m/s).</summary>
        public const double CallSpeedMin = 1.5, CallSpeedMax = 2.5;

        /// <summary>Duration (s) of call i published at its key while walking at speed (m/s): the table's duration scaled by
        /// 2.5 / clamp(speed, 1.5, 2.5), at least the s-keyed hold (CallHoldUntil, reached at that speed) plus the release.</summary>
        public static double CallDuration(int i, double speed)
        {
            double v = Math.Min(Math.Max(double.IsNaN(speed) ? CallSpeedMax : speed, CallSpeedMin), CallSpeedMax);
            double d = Calls[3 * i + 1] * CallSpeedMax / v;
            double hold = CallHoldUntil[i];
            if (hold > Calls[3 * i]) d = Math.Max(d, (hold - Calls[3 * i]) / v + CallRelease);
            return d;
        }

        // ---------------------------------------------------------------- the cinematic gaze over the encounter (critic S2-3)
        /// <summary>Gaze keys (s, yaw, pitch; yaw from the path tangent toward the sea) that hold on the arm and the eye: the
        /// eye centre's bearings from the walker (Beat06Layout.EyeAt), the pressed arm at KK4 (pitch -6 splits the arm at eye
        /// height and the eyes rising from -13), the hold abeam (1268: 87, 2), the closest point (1280-1286: 104), the slow
        /// withdrawal and the sink (KS18 1320: 62, -18). R4: GazeSchedule.cs has one writer this wave (the ending builder),
        /// who applies these in GazeSchedule.Keys (s 1190-1370); Beat06Tests simulates the walker's follower on them.</summary>
        public static readonly double[] GazeKeys =
        {
            1166.0,  70.0,  40.0,
            1190.0,  40.0,   6.0,
            1214.0,  60.0,   4.0,
            1232.0,  63.0,  -6.0,
            1252.0,  65.0,   0.0,
            1268.0,  87.0,   2.0,
            1280.0, 104.0,   4.0,
            1286.0, 104.0,   3.0,
            1298.0,  76.0,  -3.0,
            1310.0,  61.0, -10.0,
            1320.0,  62.0, -18.0,
            1348.0,  25.0,  -4.0,
            1370.0,   0.0,   0.0,
        };

        // ---------------------------------------------------------------- helpers
        public static double Smooth(double a, double b, double x)
        {
            double t = Clamp01((x - a) / (b - a));
            return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
        }
        public static double Clamp01(double x) => x < 0.0 ? 0.0 : (x > 1.0 ? 1.0 : x);
    }
}
