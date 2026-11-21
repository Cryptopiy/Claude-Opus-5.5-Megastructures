// BulwarkSchedule: the atmosphere / light / post / camera parameters as PURE functions of progress s (MASTER_PROMPT
// 14.2.3-14.2.5, 14.6 Rule P, 14.12). AtmosphereDriver evaluates it once per frame and is the one writer of the results
// (globals, key light, camera, GradeState). No time, no state, no Unity objects, no allocation.
//
// Rows (each crossfades over 30 m centred on the boundary unless 14.6 / 14.2 gives its own window):
//   * vFOV (14.6 windows), far plane (14.12 switches), EV (14.6 windows and in-beat ramps; bright-to-dark rule of 14.2.5:
//     -0.81 / -0.52 / +0.05 / +0.64 at s 920 / 926 / 934 / 945), bloom threshold / intensity / scatter, saturation,
//     contrast, white balance, vignette, dither peak (14.2.5 table), the beat 5 lamp boost 2^(1.3 (1 - S5((s-920)/40))).
//   * Sun (14.2.3 / 14.12): beat 2 az 100 el 6; beat 3 az 98 + 0.05 (s-420), el 12 + 0.025 (s-420) to (109, 17.5) at s 640
//     (s 604: 107.2 / 16.6); linear (time-lapse) to (150, 48) at 760 and (176, 56) at 920; beat 6 az 215 + 0.255 (s-1070),
//     el 48 -> 20 (shaft angle); beat 7 az 291.5 -> 305 over s 1370-1420, el(s) = -0.9 - 1.2 S5((s-1420)/80).
//   * Key: StormKey x0.35 (hidden) -> BreakGold x(0.35 -> 3.4) by S5((s-520)/55), then -> 2.6 (dimming S5((s-640)/60)) ->
//     NoonKey x4.2 rising with S5((s-690)/50) -> off inside (s 960-1004, after the look-back to the crest is blocked:
//     14.6, Part 10 #36) -> DuskKey x0.9 from the tunnel mouth (s 1415-1445), no shadows in beat 7. Ambient: sky trilight
//     of the sky state x LightCal factor; interior ambient = lamp colour x 0.06 (SodiumWarm beat 1 and tunnel, Tungsten
//     beat 5, cyan x0.08 beat 6). Published twice: the s-keyed blend (door, portal, mouth: _BwAmb*) and, for the
//     region-based BwAmbientAt, the exterior trilight and the interior ambient separately with the interior boxes.
//   * Fog and sky (14.2.4): sigma0 1.3e-4 / 9e-5 / 6e-5 / 4.5e-5, sigma_s 6.5e-4 -> 2.0e-4 (s 520-640) -> 1.0e-4 -> 0,
//     the beat 1 dust 6e-3 / m inside the gallery box; sky states storm -> gold (sun break) -> noon (s 690-740 window) ->
//     dusk (switched inside the dam, s 1000-1300, where no exterior is visible); u_sun(s) and the 12 km lowland hole;
//     sea state Hs / Tp 2.8 / 9, 2.0 / 8, 3.2 / 11 (windows 405-435, 680-720).
// LightCal (14.2.3 "calibrate light, not exposure"): LightCalKey multiplies every key, LightCalAmb* the sky trilight of each
// sky state; values from the measured graybox roads (DEVELOPMENT.md, the atmosphere report).
using System;
using UnityEngine;

namespace Megastructure
{
    public struct BwParams
    {
        // post (GradeState)
        public float Ev, WbTemp, Tint, Saturation, Contrast, BloomThreshold, BloomIntensity, BloomScatter, Vignette, DitherPeak;
        // camera
        public float Vfov, Far;
        // sun / key light: direction toward the sun (world, defined at every s), az / el (deg), linear key colour,
        // key intensity (0 = key off), shadows, disc radiance (rgb, clamped) and disc visibility
        public Vector3 SunDir;
        public float SunAzDeg, SunElDeg;
        public Color SunColor;
        public float SunIntensity;
        public bool SunShadows;
        public Color SunDisc;
        public float SunDiscVis;
        // sun break (14.2.3, 14.7 H2): u_sun, weight of the hole dimming, lit-disc centre (world x, z) and radius
        public float SunHole, BreakWeight, HoleX, HoleZ, HoleRadius;
        // sky model (BulwarkAtmosphere.hlsl BwSkyColor) and the ambient trilight (linear)
        public Color SkyZenith, SkyUpper, SkyMid, SkyHorizon, SkyGlow, SkyHalo;
        public float GlowExp, GlowElevRad, HaloWidthRad;
        // low-sun in-scatter below the visible horizon (BwFogColorRaw): depression scale (rad), low-sun weight, glow kept,
        // glow depression scale (rad)
        public float FogLowDepRad, FogLowWeight, FogLowGlow, FogLowGlowDepRad;
        public Color AmbSky, AmbMid, AmbGround;
        // the exterior trilight alone and the interior ambient alone (region-based ambient, far field)
        public Color AmbExtSky, AmbExtMid, AmbExtGround, AmbInt;
        // sky-state weights (sum 1) and the interior weight of the ambient
        public float WStorm, WGold, WNoon, WDusk, Interior;
        // fog (14.2.2 / 14.2.4)
        public float FogSigma0, FogScaleHeight, StormSigma;
        public Color FogShadow;
        public float FogDeckWeight;
        public float DustSigma;
        public Color DustColor;
        // clouds (BwFarField deck at 6.8 km): coverage thresholds, alpha, contrast, colour and colour mix, the weight of the
        // fair-weather field (0 storm deck at 12 m/s, 1 fair-weather clouds at 4 m/s; FlowOffsets moves both)
        public float CloudLo, CloudHi, CloudAlpha, CloudContrast, CloudColorMix, CloudFair;
        public Color CloudColor;
        // far field: city lights N(s), mist-river strength; sea state and far-sea body colour / slope variance
        public float CityN, MistRivers, SeaHs, SeaTp, SeaS2;
        public Color SeaBody;
        // beat 5 lamp boost (14.2.5 bright-to-dark rule)
        public float LampBoost;
        // surface state published as _BwSurface (materials owner's contract): rain wetness of exposed concrete and the road
        public float Wetness;
    }

    public static class BulwarkSchedule
    {
        static readonly double[] B = { 120.0, 420.0, 700.0, 920.0, 1070.0, 1370.0 };   // inner beat boundaries

        // 14.2.5 rows, beats 1..7 (the values live in BulwarkConst, the game table)
        static readonly double[] BloomThr = BulwarkConst.BloomThreshold;
        static readonly double[] BloomInt = BulwarkConst.BloomIntensity;
        static readonly double[] BloomSca = BulwarkConst.BloomScatter;
        static readonly double[] Sat = BulwarkConst.Saturation;
        static readonly double[] Cont = BulwarkConst.Contrast;
        static readonly double[] Wb = BulwarkConst.WhiteBalance;
        static readonly double[] Vig = BulwarkConst.Vignette;
        static readonly double[] Dither = BulwarkConst.DitherPeak;
        static readonly double[] Fov = BulwarkConst.VFov;
        // 14.6 vFOV windows per boundary
        static readonly double[] FovW0 = { 105, 405, 685, 890, 1055, 1355 };
        static readonly double[] FovW1 = { 135, 435, 715, 920, 1085, 1385 };

        // ------------------------------------------------------------------ LightCal (14.2.3; DEVELOPMENT.md)
        /// <summary>Multiplier of every key intensity (14.2.3 key x intensity values are kept as written).</summary>
        public const double LightCalKey = 1.0;
        /// <summary>Sky-trilight multipliers per sky state (the "beat factor" of 14.2.3). Dusk 0.9: the lowland in the earth's
        /// shadow at 15-40 (14.10.3) with the ledge and rails read by the key and the lantern.</summary>
        public const double LightCalAmbStorm = 3.8, LightCalAmbGold = 2.0, LightCalAmbNoon = 1.0, LightCalAmbDusk = 0.9;   // X2 S3-1: the Wave B rain wetness darkened the dry-calibrated road (101.3 -> 68.8)
        /// <summary>Radiance scale of the dusk sky stops (the 14.2.1 tokens keep their colour; LightCal of the dusk sky): 0.82 keeps
        /// beat 7's level views at s 1435 (EV +0.405 inside the mouth ramp, sky over 54.6 % of the frame) under the 14.10.3 mean
        /// of 90 with the lowland at 15-40 (DEVELOPMENT.md). The dusk shadowed haze: DuskDeep x0.6.</summary>
        public const float DuskSkyGain = 0.82f, DuskFogShadow = 0.6f;
        /// <summary>Radiance scale of the storm sky stops (LightCal of the storm sky, X2 S3-1): +0.38 EV, storm slot only (gold keeps its stops); the trilight is built from these stops, so it rises with them.</summary>
        public const float StormSkyGain = 1.3f;
        /// <summary>Interior ambient = lamp colour x 0.06 (14.2.3); beat 6 cyan x 0.08 (14.5.6).</summary>
        public const double InteriorAmbient = BulwarkConst.InteriorAmbient, HallAmbient = 0.08;

        // ------------------------------------------------------------------ windows (m of s)
        public const double SkyNoonS0 = 690.0, SkyNoonS1 = 740.0;     // 14.6: sky storm -> noon, own window
        public const double SkyDuskS0 = 1000.0, SkyDuskS1 = 1300.0;   // inside the dam: no exterior visible (14.6 look-back rule)
        public const double KeyOffS0 = 960.0, KeyOffS1 = 1004.0;      // key out on the stairs, once the look-back to the crest is
                                                                      // blocked (s >= 960, 14.6); 44 m: key step <= 0.02 per 0.1 m
        public const double MouthS0 = 1415.0, MouthS1 = 1445.0;       // tunnel mouth (EV window of 14.6)
        public const double DoorAmbS0 = 100.0, DoorAmbS1 = 122.0;     // gallery -> crest ambient while the door opens (s 104-111.5); 22 m since X2 S3-1
                                                                      // (storm ambient x1.9: the 0.01 per 0.1 m ambient step, 0.0091 at its peak)
        public const double PortalAmbS0 = 920.0, PortalAmbS1 = 940.0;
        public const double KeyB3Dim = 1.0 - BulwarkConst.KeyB3End / BulwarkConst.KeyB3Peak;   // 3.4 -> 2.6 (0.2353)
        public const double FarSeaS2 = 0.0133;                        // 14.7 H1: smooth sea variance beyond the clipmap (3 km)
        public const double HoleCentreZ0 = BulwarkConst.PlumeZ;       // the hole is placed 22 km from the plume axis z 562
        // low-sun in-scatter below the visible horizon (BwFogColorRaw; DEVELOPMENT.md): fades by 1 - exp(-(dep / 1.5 deg)^2) once
        // the sun is below 1 deg (full) .. 6 deg (none) of elevation; keeps 5 % of the sun-side glow over 8 deg
        public const double FogLowDepDeg = 1.5, FogLowSunEl0 = 1.0, FogLowSunEl1 = 6.0, FogLowGlow = 0.05, FogLowGlowDepDeg = 8.0;
        // far-field motion (m/s): storm deck, fair-weather clouds, mist rivers, outlet jets (14.5.7 "mist rivers 4 m/s")
        public const double FlowStormWind = BulwarkConst.StormCloudSpeed, FlowFairWind = 4.0, FlowMist = 4.0, FlowJet = BulwarkConst.JetSpeed;
        /// <summary>Soft half-width (m) of the faces of the interior boxes (region-based ambient).</summary>
        public const double InteriorEdge = 0.75;

        /// <summary>Interior boxes (world min / max, metres): the beat 1 gallery shell up to the door plane; the gate-tower atrium
        /// from the portal plane to the north arch; the stairs and the exit hall (below the road); the window gallery and
        /// the sluice tunnel up to 0.5 m inside the lee face. Region-based ambient (BwAmbientAt) and its twin.</summary>
        public static readonly double[] InteriorBoxes =
        {
            BulwarkConst.ShellXMin, -0.5, BulwarkConst.ShellZMin, BulwarkConst.ShellXMax, BulwarkConst.ShellYMax, BulwarkConst.DoorPlaneZ,
            -7.0, -3.0, BulwarkConst.GateTowerZ0, 7.0, 9.5, 956.0,
            -6.0, -60.0, 930.0, 7.0, -1.5, 1072.0,
            BulwarkConst.LeeFaceX + 0.5, -100.0, 1030.0, 15.5, -24.0, 1424.0,   // window hall + tunnel: the 28 m ceiling (y -26 at s 1070) and the inner glass face (x 14.75 at s 1370) fit (Wave X2 gallery; 4 boxes is the _BwIntBox limit)
        };

        /// <summary>Far-field motion offsets (m) at the UNWRAPPED ambient clock t (s), computed in double: x storm-deck wind,
        /// y fair-weather wind, z mist-river flow, w jet flow. Pure functions of t (captures are deterministic, nothing
        /// slides when the sky state changes) without the 3600 s wrap of _BwTime (Part 10 #33).</summary>
        public static Vector4 FlowOffsets(double t)
            => new Vector4((float)(FlowStormWind * t), (float)(FlowFairWind * t), (float)(FlowMist * t), (float)(FlowJet * t));

        static double S5(double a, double b, double x) => BwMath.Smootherstep(a, b, x);

        /// <summary>Rule P row: per-beat values crossfaded with smootherstep over 30 m centred on each boundary.</summary>
        public static double RowP(double[] v, double s)
        {
            double r = v[0];
            for (int i = 0; i < B.Length; i++) r += (v[i + 1] - v[i]) * S5(B[i] - 15.0, B[i] + 15.0, s);
            return r;
        }

        /// <summary>Row with explicit windows per boundary (vFOV).</summary>
        public static double RowW(double[] v, double[] w0, double[] w1, double s)
        {
            double r = v[0];
            for (int i = 0; i < B.Length; i++) r += (v[i + 1] - v[i]) * S5(w0[i], w1[i], s);
            return r;
        }

        /// <summary>Exposure (EV) of 14.2.5 / 14.6: windows 104-130, 400-440, 690-740, 912-952, 1055-1085, the beat-6 ramp
        /// +0.4 -> +1.0, 1355-1385 and the tunnel mouth 1415-1445.</summary>
        public static double Ev(double s)
        {
            double ev = 0.3;
            ev += -0.3 * S5(104, 130, s);     // 1 -> 2: +0.3 -> 0
            ev += -0.3 * S5(400, 440, s);     // 2 -> 3: 0 -> -0.3
            ev += 0.3 * (S5(450, 470, s) - S5(510, 530, s));   // KS07 hold under the storm (K3 at the gates, s 470-510): +0.3 EV, back to -0.3 before the sun break (fix X2 S3-2)
            ev += -0.6 * S5(690, 740, s);     // 3 -> 4: -0.3 -> -0.9
            ev += 1.6 * S5(912, 952, s);      // 4 -> 5: -0.9 -> +0.7 (after the portal plane)
            ev += -0.3 * S5(1055, 1085, s);   // 5 -> 6: +0.7 -> +0.4
            ev += 0.6 * S5(1085, 1355, s);    // beat 6: +0.4 -> +1.0
            ev += -0.2 * S5(1355, 1385, s);   // 6 -> 7: +1.0 -> +0.8
            ev += -0.5 * S5(1415, 1445, s);   // tunnel mouth: +0.8 -> +0.3
            ev += BulwarkConst.EndEvGain * S5(BulwarkConst.EndEvS0, BulwarkConst.EndEvS1, s);   // the ride into the earth's shadow: +0.3 -> +1.1
            return ev;
        }

        /// <summary>Lamp boost of the bright-to-dark rule (14.2.5): 2^(1.3 (1 - S5((s-920)/40))), x2.46 up to the plane.</summary>
        public static double LampBoost(double s) => Math.Pow(2.0, BulwarkConst.PortalLampBoostEv * (1.0 - BwMath.Smootherstep01((s - 920.0) / BulwarkConst.PortalLampBoostWindow)));

        /// <summary>Rain wetness of exposed concrete and the road (_BwSurface.x; 14.2.6 "road darkened outside the sun-break
        /// window"): wet through the storm, drying with the sun break over s 520-640, dry from there on (0 at s >= 640, so
        /// the 700 cut of the materials owner's formula is continuous). The materials owner's formula starts at 0 below
        /// s 120; it is 1 there instead, because the road is seen through the opening door from s 104 and a 0 -> 1 step at
        /// s 120 would darken it in one frame (Part 10 #12): interiors keep their own wetness per material (_WetFollow 0) or
        /// mask it per pixel with BwInteriorWeight (DEVELOPMENT.md).</summary>
        public static double Wetness(double s) => s >= 700.0 ? 0.0 : 1.0 - BwMath.Smootherstep01((s - 520.0) / 120.0);

        /// <summary>Exterior radiance clamp for an eye at world position p (14.7 H8 "exterior radiance clamped to 12 inside
        /// openings"): 12 while the eye is inside an interior box (the gallery, the tower, the stairs and hall, the window
        /// gallery and the tunnel), 1e4 outside, log-interpolated by the eye's interior weight with a 2 m soft edge, so the
        /// sun disc seen through the tunnel mouth (radiance 60) is clamped to 12 until the eye has stepped out.</summary>
        public static double OpeningClamp(double px, double py, double pz)
        {
            double w = InteriorWeightSoft(px, py, pz, 2.0);
            return Math.Exp(Math.Log(1e4) + (Math.Log(BulwarkConst.ClampOpening) - Math.Log(1e4)) * w);
        }

        /// <summary>Interior weight of a world point from InteriorBoxes with a soft half-width e (m): the max over the boxes
        /// of the product of the per-axis smooth inside-ness (the same rule as BwInteriorWeight with its own edge).</summary>
        public static double InteriorWeightSoft(double px, double py, double pz, double e)
        {
            double w = 0.0;
            var b = InteriorBoxes;
            for (int k = 0; k + 5 < b.Length; k += 6)
            {
                double ix = Math.Min(Sat01((px - b[k]) / (2 * e) + 0.5), Sat01((b[k + 3] - px) / (2 * e) + 0.5));
                double iy = Math.Min(Sat01((py - b[k + 1]) / (2 * e) + 0.5), Sat01((b[k + 4] - py) / (2 * e) + 0.5));
                double iz = Math.Min(Sat01((pz - b[k + 2]) / (2 * e) + 0.5), Sat01((b[k + 5] - pz) / (2 * e) + 0.5));
                w = Math.Max(w, ix * iy * iz);
            }
            return w * w * (3.0 - 2.0 * w);
        }

        static double Sat01(double x) => x < 0.0 ? 0.0 : (x > 1.0 ? 1.0 : x);

        /// <summary>Far plane (14.12): 150 / 12000 / 300 / 600 / 12000 switching at s 100, 934, 1030, 1373.</summary>
        public static double Far(double s)
        {
            if (s < FrameworkConst.FarSwitch1) return FrameworkConst.FarBeat1;
            if (s < FrameworkConst.FarSwitch2) return FrameworkConst.FarOutdoor;
            if (s < FrameworkConst.FarSwitch3) return FrameworkConst.FarBeat5;
            if (s < FrameworkConst.FarSwitch4) return FrameworkConst.FarBeat6;
            return FrameworkConst.FarOutdoor;
        }

        public static double Vfov(double s) => RowW(Fov, FovW0, FovW1, s);
        public static double DitherPeak(double s) => RowP(Dither, s);

        // ------------------------------------------------------------------ sun (14.2.3, 14.12 "Sun")
        /// <summary>u_sun(s) = smootherstep((s-520)/55) (1 - 0.35 smootherstep((s-640)/60)) (14.2.3).</summary>
        public static double USun(double s) => BwMath.Smootherstep01((s - BulwarkConst.SunBreakS0) / BulwarkConst.SunBreakW0)
                                                 * (1.0 - BulwarkConst.SunBreakDim * BwMath.Smootherstep01((s - BulwarkConst.SunBreakS1) / BulwarkConst.SunBreakW1));

        /// <summary>Sun azimuth / elevation in degrees at every s (the sky, the shafts and the key use it; 14.2.3).</summary>
        public static void Sun(double s, out double azDeg, out double elDeg)
        {
            if (s > BwPath.Length)
            {
                // the ending (ADDENDUM A1 3.7): az 305; el -2.10 - 0.10 F in the descent (F = descended fraction), then
                // -2.20 -> -2.35 linearly over the transfer, the shuttle and the platform walk to the goal
                azDeg = BulwarkConst.DuskSunAz;
                elDeg = EndingSunEl(s);
                return;
            }
            if (s <= BulwarkConst.SunB3S1)
            {
                // beat 2 (100, 6) -> beat 3 path, Rule P at s 420 (405-435)
                double u = s - BulwarkConst.SunB3S0;
                double az3 = BulwarkConst.SunB3Az0 + BulwarkConst.SunB3AzRate * u, el3 = BulwarkConst.SunB3El0 + BulwarkConst.SunB3ElRate * u;
                double w = S5(405.0, 435.0, s);
                azDeg = BwMath.Lerp(BulwarkConst.SunB2Az, az3, w);
                elDeg = BwMath.Lerp(BulwarkConst.SunB2El, el3, w);
                return;
            }
            if (s <= 760.0)
            {
                double t = (s - BulwarkConst.SunB3S1) / (760.0 - BulwarkConst.SunB3S1);
                azDeg = BwMath.Lerp(BulwarkConst.SunB3AzAt640, BulwarkConst.SunB4Az760, t);
                elDeg = BwMath.Lerp(BulwarkConst.SunB3ElAt640, BulwarkConst.SunB4El760, t);
                return;
            }
            if (s <= 920.0)
            {
                double t = (s - 760.0) / 160.0;
                azDeg = BwMath.Lerp(BulwarkConst.SunB4Az760, BulwarkConst.SunB4Az920, t);
                elDeg = BwMath.Lerp(BulwarkConst.SunB4El760, BulwarkConst.SunB4El920, t);
                return;
            }
            if (s <= 1070.0)
            {
                // beat 5: no sun in view; carried linearly to the beat 6 start
                double t = (s - 920.0) / 150.0;
                azDeg = BwMath.Lerp(BulwarkConst.SunB4Az920, BulwarkConst.SunB6Az0, t);
                elDeg = BwMath.Lerp(BulwarkConst.SunB4El920, BulwarkConst.SunB6El0, t);
                return;
            }
            // beat 6 (shaft angle) and beat 7, crossfaded over s 1355-1385 (Rule P)
            double az6 = BulwarkConst.SunB6Az0 + BulwarkConst.SunB6AzRate * (s - 1070.0);
            double el6 = BulwarkConst.SunB6El0 + (BulwarkConst.SunB6El1 - BulwarkConst.SunB6El0) * (s - 1070.0) / 300.0;
            double rate7 = (BulwarkConst.SunB7Az1 - BulwarkConst.SunB7Az0) / (BulwarkConst.SunB7AzS1 - BulwarkConst.SunB7AzS0);
            double az7 = s >= BulwarkConst.SunB7AzS1 ? BulwarkConst.SunB7Az1 : BulwarkConst.SunB7Az0 + rate7 * (s - BulwarkConst.SunB7AzS0);
            double el7 = SunElB7(s);
            double w7 = S5(1355.0, 1385.0, s);
            azDeg = BwMath.Lerp(az6, az7, w7);
            elDeg = BwMath.Lerp(el6, el7, w7);
        }

        /// <summary>Sun elevation of the ending timeline (deg; ADDENDUM A1 3.7): continuous with SunElB7 at s 1500 (-2.10).</summary>
        public static double EndingSunEl(double s)
        {
            if (s <= EndingPath.SDescent1) return BulwarkConst.SunRideEl0 - BulwarkConst.SunRideElDescent * EndingPath.DescentFraction(s);
            double u = Math.Max(0.0, Math.Min(1.0, (s - EndingPath.SDescent1) / (EndingPath.SGoal - EndingPath.SDescent1)));
            return BulwarkConst.SunFootEl0 - BulwarkConst.SunFootElDrop * u;
        }

        /// <summary>The after-goal sink (3.7 / 3.10): -0.47 smootherstep((t_g - 9) / 7) deg (crest dark at t_g 14.0).</summary>
        public static double EndSunSink(double tg) => tg < 0 ? 0.0 : -BulwarkConst.EndSunSink * BwMath.Smootherstep01((tg - BulwarkConst.EndSunSinkStart) / BulwarkConst.EndSunSinkTime);

        /// <summary>Fraction of the 0.53 deg sun disc above the horizon seen from altitude h (m; geometric dip acos(R / (R + h))):
        /// the circular-segment area, 1 when the lower limb clears the horizon, 0 when the upper limb is below it.</summary>
        public static double SunDiscFraction(double elDeg, double altitude)
        {
            double h = Math.Max(0.0, altitude);
            double dip = Math.Acos(BulwarkConst.EarthRadius / (BulwarkConst.EarthRadius + h)) / BwMath.Deg;
            double x = (elDeg + dip) / SunDiscRadiusDeg;
            if (x >= 1.0) return 1.0;
            if (x <= -1.0) return 0.0;
            return 1.0 - (Math.Acos(x) - x * Math.Sqrt(1.0 - x * x)) / Math.PI;
        }
        public const double SunDiscRadiusDeg = 0.265;

        /// <summary>el(s) = -0.9 - 1.2 smootherstep((s-1420)/80) (14.5.7, 14.12).</summary>
        public static double SunElB7(double s) => BulwarkConst.SunB7El0 - BulwarkConst.SunB7ElDrop * BwMath.Smootherstep01((s - BulwarkConst.SunB7ElS0) / BulwarkConst.SunB7ElWindow);

        /// <summary>Unit vector toward a direction of azimuth az (0 = +Z, 90 = +X) and elevation el (degrees).</summary>
        public static Vector3 Dir(double azDeg, double elDeg)
        {
            double a = azDeg * BwMath.Deg, e = elDeg * BwMath.Deg;
            return new Vector3((float)(Math.Cos(e) * Math.Sin(a)), (float)Math.Sin(e), (float)(Math.Cos(e) * Math.Cos(a)));
        }

        /// <summary>Key intensity before LightCal (14.2.3): 0.35 -> 3.4 -> 2.6 (beat 3), 4.2 (beat 4), off inside, 0.9 (beat 7).</summary>
        public static double Key(double s)
        {
            // the ending keeps the dusk key: the earth shadow is per surface altitude (BwSunBreak in BulwarkAtmosphere.hlsl), so
            // the cabin passes into the shadow at ~4386 m while the wall above stays lit (3.7)
            // below the shadow line the key lights only the upper wall: there it is the alpenglow, raised x2 once the cabin is in
            // the shadow (s 1585-1640: the upper limb is gone at s 1593), so the lit band reads against the violet wall
            if (s > BwPath.Length) return BulwarkConst.KeyB7 * (1.0 + BulwarkConst.EndAlpenglowGain * S5(1585.0, 1640.0, s));
            if (s >= 1200.0) return BulwarkConst.KeyB7 * S5(MouthS0, MouthS1, s);
            double k3 = (BulwarkConst.KeyB2 + (BulwarkConst.KeyB3Peak - BulwarkConst.KeyB2) * BwMath.Smootherstep01((s - BulwarkConst.SunBreakS0) / BulwarkConst.SunBreakW0))
                        * (1.0 - KeyB3Dim * BwMath.Smootherstep01((s - BulwarkConst.SunBreakS1) / BulwarkConst.SunBreakW1));
            double k = BwMath.Lerp(k3, BulwarkConst.KeyB4, BwMath.Smootherstep01((s - BulwarkConst.SunB4KeyS0) / BulwarkConst.SunB4KeyWindow));
            return k * (1.0 - S5(KeyOffS0, KeyOffS1, s));
        }

        /// <summary>Interior weight of the ambient: 1 in the gallery, tower, stairs, hall and tunnel; 0 outside.</summary>
        public static double InteriorWeight(double s)
        {
            if (s < 500.0) return 1.0 - S5(DoorAmbS0, DoorAmbS1, s);
            if (s < 1200.0) return S5(PortalAmbS0, PortalAmbS1, s);
            return 1.0 - S5(MouthS0, MouthS1, s);
        }

        /// <summary>Sky-state weights (storm, gold, noon, dusk), sum 1.</summary>
        public static void SkyWeights(double s, out double storm, out double gold, out double noon, out double dusk)
        {
            double g = S5(BulwarkConst.SunBreakS0, BulwarkConst.SunBreakS0 + BulwarkConst.SunBreakW0, s);
            double n = S5(SkyNoonS0, SkyNoonS1, s);
            dusk = S5(SkyDuskS0, SkyDuskS1, s);
            noon = n * (1.0 - dusk);
            gold = g * (1.0 - n) * (1.0 - dusk);
            storm = (1.0 - g) * (1.0 - n) * (1.0 - dusk);
        }

        /// <summary>Haze sigma0 (1/m at sea level): 1.3e-4 (beats 1-2) -> 9e-5 (405-435) -> 6e-5 (690-740) -> 4.5e-5 (hidden window).</summary>
        public static double HazeSigma(double s)
        {
            double[] h = BulwarkConst.HazeSigma;
            if (s > BwPath.Length)
            {
                // the ending (3.7): 4.5e-5 to F 0.5, smootherstep to 2.0e-4 at F 1.0, 2.0e-4 at the foot and on the platform
                double F = EndingPath.DescentFraction(s);
                return h[6] + (BulwarkConst.HazeFoot - h[6]) * BwMath.Smootherstep01((F - 0.5) / 0.5);
            }
            return h[1] + (h[2] - h[1]) * S5(405, 435, s) + (h[3] - h[2]) * S5(SkyNoonS0, SkyNoonS1, s) + (h[6] - h[3]) * S5(SkyDuskS0, SkyDuskS1, s);
        }

        /// <summary>Storm slab sigma_s: 6.5e-4 -> 2.0e-4 (s 520-640, the thinning of 14.2.4) -> 1.0e-4 (690-740) -> 0.</summary>
        public static double StormSigma(double s)
        {
            double[] st = BulwarkConst.StormSigma;
            return st[1] + (BulwarkConst.StormSigmaB3End - st[1]) * S5(BulwarkConst.StormSigmaB3S0, BulwarkConst.StormSigmaB3S1, s)
                   + (st[3] - BulwarkConst.StormSigmaB3End) * S5(SkyNoonS0, SkyNoonS1, s) + (st[6] - st[3]) * S5(SkyDuskS0, SkyDuskS1, s);
        }

        /// <summary>Sea state (14.2.4 / 14.6): Hs 2.8 -> 2.0 (405-435) -> 3.2 (680-720); Tp 9 -> 8 -> 11.</summary>
        public static void SeaState(double s, out double hs, out double tp)
        {
            double a = S5(405, 435, s), b = S5(680, 720, s);
            hs = BulwarkConst.SeaHs[1] + (BulwarkConst.SeaHs[2] - BulwarkConst.SeaHs[1]) * a + (BulwarkConst.SeaHs[3] - BulwarkConst.SeaHs[2]) * b;
            tp = BulwarkConst.SeaTp[1] + (BulwarkConst.SeaTp[2] - BulwarkConst.SeaTp[1]) * a + (BulwarkConst.SeaTp[3] - BulwarkConst.SeaTp[2]) * b;
        }

        /// <summary>City lights N(s) = smoothstep((s - 1425) / 50) (14.5.7, 14.12).</summary>
        public static double CityN(double s) => BwMath.Smoothstep(BulwarkConst.CityLightS0, BulwarkConst.CityLightS0 + BulwarkConst.CityLightWindow, s);

        // ------------------------------------------------------------------ palette (looked up once; Evaluate must not allocate)
        static bool s_colours;
        static Color s_stormZen, s_stormHor, s_noonZen, s_noonHor, s_duskDeep, s_duskViolet, s_duskRose, s_duskAmber;
        static Color s_gold, s_stormKey, s_noonKey, s_duskKey, s_sodium, s_tungsten, s_seaTeal, s_seaStorm, s_foam;
        static Color C(Pal p) => BulwarkPalette.Linear(p);
        static void EnsureColours()
        {
            if (s_colours) return;
            s_stormZen = C(Pal.StormZenith); s_stormHor = C(Pal.StormHorizon);
            s_noonZen = C(Pal.NoonZenith); s_noonHor = C(Pal.NoonHorizon);
            s_duskDeep = C(Pal.DuskDeep); s_duskViolet = C(Pal.DuskViolet); s_duskRose = C(Pal.DuskRose); s_duskAmber = C(Pal.DuskAmber);
            s_gold = C(Pal.BreakGold); s_stormKey = C(Pal.StormKey); s_noonKey = C(Pal.NoonKey); s_duskKey = C(Pal.DuskKey);
            s_sodium = C(Pal.SodiumWarm); s_tungsten = C(Pal.Tungsten); s_seaTeal = C(Pal.SeaTeal); s_seaStorm = C(Pal.SeaStorm);
            s_foam = C(Pal.FoamWhite);
            s_colours = true;
        }

        static Color L(Color a, Color b, float t) => new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, 1f);
        static Color Mul(Color a, float k) => new Color(a.r * k, a.g * k, a.b * k, 1f);
        static Color Mix4(Color a, Color b, Color c, Color d, float wa, float wb, float wc, float wd)
            => new Color(a.r * wa + b.r * wb + c.r * wc + d.r * wd, a.g * wa + b.g * wb + c.g * wc + d.g * wd, a.b * wa + b.b * wb + c.b * wc + d.b * wd, 1f);
        static float Mix4(float a, float b, float c, float d, float wa, float wb, float wc, float wd) => a * wa + b * wb + c * wc + d * wd;
        static Color Grey(Color c) { float l = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b; return new Color(l, l, l, 1f); }
        static Color Normalised(Color c) { float m = Mathf.Max(c.r, Mathf.Max(c.g, c.b)); return m > 1e-6f ? Mul(c, 1f / m) : c; }

        public static BwParams Evaluate(double s) => Evaluate(s, -1.0);

        /// <summary>The schedule at s; tg >= 0 = seconds since the goal (the sun sinks after it, ADDENDUM A1 3.10).</summary>
        public static BwParams Evaluate(double s, double tg)
        {
            EnsureColours();
            var p = new BwParams();
            // ---------------- post and camera (14.2.5, 14.6)
            p.Ev = (float)Ev(s);
            p.WbTemp = (float)RowP(Wb, s);
            p.Tint = 0f;
            p.Saturation = (float)RowP(Sat, s);
            p.Contrast = (float)RowP(Cont, s);
            p.BloomThreshold = (float)RowP(BloomThr, s);
            p.BloomIntensity = (float)RowP(BloomInt, s);
            p.BloomScatter = (float)RowP(BloomSca, s);
            p.Vignette = (float)RowP(Vig, s);
            p.DitherPeak = (float)DitherPeak(s);
            p.Vfov = (float)Vfov(s);
            p.Far = (float)Far(s);
            p.LampBoost = (float)LampBoost(s);
            p.Wetness = (float)Wetness(s);

            // ---------------- sky state weights
            SkyWeights(s, out double ws, out double wg, out double wn, out double wd);
            float fs = (float)ws, fg = (float)wg, fn = (float)wn, fd = (float)wd;
            p.WStorm = fs; p.WGold = fg; p.WNoon = fn; p.WDusk = fd;

            // ---------------- sun, key, disc, break (14.2.3)
            Sun(s, out double az, out double el);
            double key0 = Key(s);
            if (tg >= 0.0 && s > BwPath.Length) el += EndSunSink(tg);
            p.SunAzDeg = (float)az; p.SunElDeg = (float)el;
            p.SunDir = Dir(az, el);
            double usun = USun(s);
            p.SunHole = (float)usun;
            p.SunIntensity = (float)(key0 * LightCalKey);
            p.SunShadows = s < BulwarkConst.BeatS[6];                      // beat 7: no shadows (14.2.3)
            Color key = L(s_stormKey, s_gold, (float)S5(BulwarkConst.SunBreakS0, BulwarkConst.SunBreakS0 + BulwarkConst.SunBreakW0, s));
            key = L(key, s_noonKey, (float)S5(SkyNoonS0, SkyNoonS1, s));
            if (s >= 1200.0) key = s_duskKey;
            p.SunColor = key;
            float vis = (float)S5(540.0, 590.0, s);                       // the storm deck hides the disc until the break
            p.SunDiscVis = vis;
            p.SunDisc = Mul(Normalised(key), (float)BulwarkConst.SunDiscRadiance);
            p.BreakWeight = (float)(S5(BulwarkConst.SunBreakS0, BulwarkConst.SunBreakS0 + BulwarkConst.SunBreakW0, s) * (1.0 - S5(SkyNoonS0, SkyNoonS1, s)));
            p.HoleRadius = (float)(BulwarkConst.CloudHoleRadius * BwMath.Smootherstep01((s - BulwarkConst.CloudHoleS0) / BulwarkConst.CloudHoleWindow));
            double ha = BulwarkConst.CloudHoleAz * BwMath.Deg;
            p.HoleX = (float)(BulwarkConst.LeeFaceX + BulwarkConst.CloudHoleDistance * Math.Sin(ha));
            p.HoleZ = (float)(HoleCentreZ0 + BulwarkConst.CloudHoleDistance * Math.Cos(ha));

            // ---------------- sky stops per state (14.2.1 sky states), blended by the weights
            Color zS = s_stormZen, hS = s_stormHor, mS = L(zS, hS, 0.45f), uS = L(zS, hS, 0.225f);
            Color zN = s_noonZen, hN = s_noonHor, mN = L(zN, hN, 0.45f), uN = L(zN, hN, 0.225f);
            // dusk horizon away from the sun: 0.7 x the rose / violet mix (the anti-sun horizon sits in the earth's shadow)
            float dg = DuskSkyGain;
            Color hD = Mul(L(s_duskRose, s_duskViolet, 0.45f), 0.7f * dg);
            float sg = StormSkyGain;
            p.SkyZenith = Mix4(Mul(zS, sg), Mul(zS, 1.04f), zN, Mul(s_duskDeep, dg), fs, fg, fn, fd);
            p.SkyUpper = Mix4(Mul(uS, sg), Mul(uS, 1.04f), uN, Mul(s_duskViolet, dg), fs, fg, fn, fd);
            p.SkyMid = Mix4(Mul(mS, sg), Mul(mS, 1.04f), mN, Mul(s_duskRose, dg), fs, fg, fn, fd);
            p.SkyHorizon = Mix4(Mul(hS, sg), hS, hN, hD, fs, fg, fn, fd);
            p.SkyGlow = Mix4(Mul(hS, 1.06f * sg), L(hS, Mul(s_gold, 0.75f), 0.4f), Mul(hN, 1.04f), Mul(s_duskAmber, dg), fs, fg, fn, fd);
            p.SkyHalo = Mix4(Mul(hS, 0f), Mul(s_gold, 0.35f * (float)usun), Mul(s_noonKey, 0.3f), Mul(s_duskAmber, 0.45f * dg), fs, fg, fn, fd);
            // dusk glow: azimuth lobe ((1 + cos daz) / 2)^6 (half at 39 deg from the sun), 8 deg elevation scale
            p.GlowExp = Mix4(2f, 3f, 2f, 6f, fs, fg, fn, fd);
            p.GlowElevRad = (float)(Mix4(10f, 14f, 10f, 8f, fs, fg, fn, fd) * BwMath.Deg);
            p.HaloWidthRad = Mix4(0.2f, 0.12f, 0.09f, 0.06f, fs, fg, fn, fd);
            // low sun: rays below the visible horizon see haze lit only by grazing, extinguished sunlight (BwFogColorRaw)
            p.FogLowDepRad = (float)(FogLowDepDeg * BwMath.Deg);
            p.FogLowWeight = (float)(1.0 - BwMath.Smoothstep(FogLowSunEl0, FogLowSunEl1, el));
            p.FogLowGlow = (float)FogLowGlow;
            p.FogLowGlowDepRad = (float)(FogLowGlowDepDeg * BwMath.Deg);

            // ---------------- ambient trilight: sky (cosine-weighted stop mix) x LightCal, interior lamp colour x 0.06
            float kAmb = (float)Mix4((float)LightCalAmbStorm, (float)LightCalAmbGold, (float)LightCalAmbNoon, (float)LightCalAmbDusk, fs, fg, fn, fd);
            // cosine-weighted sky stops; the dusk dome is dominated by its violet upper half (DuskRose sits low), and the
            // dusk ambient is kept 30 % toward grey so lit surfaces never turn magenta
            float zw = 0.35f + 0.15f * fd, mw = 0.2f - 0.1f * fd, hw = 0.1f - 0.05f * fd;
            Color skyUp = Mix4(p.SkyZenith, p.SkyUpper, p.SkyMid, p.SkyHorizon, zw, 0.35f, mw, hw);
            skyUp = L(skyUp, Grey(skyUp), 0.3f * fd);
            Color skySide = Mix4(p.SkyUpper, p.SkyMid, p.SkyHorizon, p.SkyGlow, 0.2f, 0.4f, 0.3f, 0.1f);
            float sunUp = Mathf.Max(0f, Mathf.Sin((float)(el * BwMath.Deg))) * p.SunIntensity;
            Color bounce = L(Mul(skyUp, 0.18f), Mul(key, 0.18f), Mathf.Clamp01(sunUp / (sunUp + 0.5f)));
            bounce = Mul(bounce, 1f + 0.18f * sunUp);
            Color aSky = Mul(skyUp, kAmb), aMid = Mul(L(skySide, bounce, 0.5f), kAmb), aGround = Mul(bounce, kAmb);
            // the window hall: 0.7 x the cyan spill + 0.3 x neutral at the same luminance (X2 S2-5: R >= 0.09 G, no coloured plastic)
            Color cyan = Normalised(s_seaTeal);
            cyan = L(cyan, Grey(cyan), 0.3f);
            Color lamp = s < 500.0 ? s_sodium : (s < 1200.0 ? L(s_tungsten, cyan, (float)S5(1055.0, 1085.0, s)) : L(cyan, s_sodium, (float)S5(1355.0, 1385.0, s)));
            float lampK = (float)(s < 1200.0 ? BwMath.Lerp(InteriorAmbient, HallAmbient, S5(1055.0, 1085.0, s)) : BwMath.Lerp(HallAmbient, InteriorAmbient, S5(1355.0, 1385.0, s)));
            Color aInt = Mul(lamp, lampK);
            float wi = (float)InteriorWeight(s);
            p.Interior = wi;
            p.AmbSky = L(aSky, aInt, wi);
            p.AmbMid = L(aMid, aInt, wi);
            p.AmbGround = L(aGround, Mul(aInt, 0.8f), wi);
            p.AmbExtSky = aSky; p.AmbExtMid = aMid; p.AmbExtGround = aGround; p.AmbInt = aInt;

            // ---------------- fog (14.2.2 / 14.2.4)
            p.FogSigma0 = (float)HazeSigma(s);
            p.FogScaleHeight = (float)BulwarkConst.HazeScaleHeight;
            p.StormSigma = (float)StormSigma(s);
            // shadowed haze: the earth shadow at dusk (lit by the dusk sky only, DuskDeep x0.8) and the haze under the storm
            // deck (diffuse light)
            p.FogShadow = Mix4(Mul(s_stormZen, 0.6f), Mul(s_stormZen, 0.6f), Mul(s_noonZen, 0.6f), Mul(s_duskDeep, DuskFogShadow), fs, fg, fn, fd);
            p.FogDeckWeight = fs + fg;
            p.DustSigma = (float)(BulwarkConst.HazeSigma[0] * (1.0 - S5(100.0, 140.0, s)));
            p.DustColor = Mul(s_sodium, 0.03f);

            // ---------------- clouds (deck at 6.8 km; storm overcast texture -> broken -> fair-weather puffs -> thin dusk streaks)
            p.CloudLo = Mix4(-1.0f, -0.2f, 0.56f, 0.6f, fs, fg, fn, fd);
            p.CloudHi = Mix4(0.0f, 0.55f, 0.76f, 0.82f, fs, fg, fn, fd);
            p.CloudAlpha = Mix4(1.0f, 1.0f, 0.85f, 0.45f, fs, fg, fn, fd);
            p.CloudContrast = Mix4(0.35f, 0.35f, 0.3f, 0.3f, fs, fg, fn, fd);
            p.CloudColorMix = Mix4(0f, 0.3f, 1f, 1f, fs, fg, fn, fd);
            p.CloudColor = Mix4(hS, L(hS, s_gold, 0.3f), L(hN, s_foam, 0.6f), Mul(L(s_duskRose, s_duskAmber, 0.4f), 0.8f), fs, fg, fn, fd);
            p.CloudFair = fn + fd;

            // ---------------- far field and sea
            p.CityN = (float)CityN(s);
            p.MistRivers = 1f;
            SeaState(s, out double hsea, out double tp);
            p.SeaHs = (float)hsea; p.SeaTp = (float)tp;
            p.SeaS2 = (float)FarSeaS2;
            p.SeaBody = Mul(L(s_seaStorm, s_seaTeal, (float)S5(SkyNoonS0, SkyNoonS1, s)), 0.6f);
            return p;
        }
    }
}
