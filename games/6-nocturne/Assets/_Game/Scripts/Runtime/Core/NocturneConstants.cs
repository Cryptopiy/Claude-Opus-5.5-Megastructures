// NocturneConstants: the numbers of MASTER_PROMPT 14.12 and 14.4 (plus the 14.2.4 schedules, 14.5 gaze keys, 14.10 key
// shots and 14.11 UI timings) shared by code, tests and shaders. One static class, no UnityEngine dependency.
// Tools/gen_hlsl.py and the editor ShaderGen export every `public const double|float|int NAME = <literal>;` field to
// Shaders/Generated/NocConstantsGen.hlsl as `#define NC_NAME value`: keep const fields literal (computed values are
// static readonly). A number that differs from Part 14 is a logged decision in DEVELOPMENT.md. Look v2 (D-027, D-028): the
// atmosphere, post, light and key-shot numbers follow docs/LOOK_V2.md sections 2, 3, 6, 7 and 8 (PA); the facade and wet-floor
// blocks below stay byte-identical (CityField and the v1 paths read them; FA and SU keep their own constants classes).
namespace Megastructure
{
    public static class NocturneConstants
    {
        // ------------------------------------------------------------------ player (14.12 row 1, Part 9)
        public const double EyeHeight = 1.7;
        public const double WalkSpeed = 1.5;
        public const double BriskSpeed = 2.5;
        /// <summary>Cinematic autowalk speed = the brisk speed (Part 9, Part 3.1 (a)).</summary>
        public const double CinematicSpeed = 2.5;
        /// <summary>Acceleration smoothing of manual walking and of the QA walker start (s).</summary>
        public const double Accel = 0.2;
        /// <summary>Cinematic start ramp from rest (s, Part 9: at most 0.5 s).</summary>
        public const double CineAccelTime = 0.5;
        public const double PlayerRadius = 0.35;
        public const double PlayerHeight = 1.8;
        /// <summary>Head bob amplitude (m, 14.1 conventions; never roll) and the Part 9 cap.</summary>
        public const double HeadBob = 0.008;
        public const double HeadBobMax = 0.015;
        /// <summary>Footstep strides (m) at 1.5 and 2.5 m/s (plan 3.6: lerp(0.75, 0.85, InverseLerp(1.5, 2.5, v))).</summary>
        public const double StrideWalk = 0.75;
        public const double StrideBrisk = 0.85;
        /// <summary>Camera sphere check radius (14.4) and the safety volume (y below this snaps back).</summary>
        public const double CameraSphere = 0.2;
        public const double SafetyY = -3.0;
        /// <summary>Safety: more than this outside the lane union snaps back (The Ring's Safety).</summary>
        public const double SafetyOutside = 1.0;

        // ------------------------------------------------------------------ path (14.4, 14.12 row 2)
        public const double PathLength = 1500.0;
        /// <summary>Length of the sharp polyline P0..P8 (14.4: 1 536.05).</summary>
        public const double SharpLength = 1536.06;
        /// <summary>x = s - EastLegOffset on the two east legs (14.12).</summary>
        public const double EastLegOffset = 184.70;
        public const double C1Radius = 24.0, C2Radius = 30.0, C3Radius = 30.0;
        public const double C1TangentIn = 171.00, C1TangentOut = 208.70;
        public const double C2TangentIn = 1056.44, C2TangentOut = 1103.56, C2Mid = 1080.0;
        public const double C3TangentIn = 1236.44, C3TangentOut = 1283.56, C3Mid = 1260.0;

        /// <summary>Sharp waypoints P0..P8 (x, z) of 14.4.</summary>
        public static readonly double[] WaypointX = { 0.0, 0.0, 0.0, 215.30, 375.30, 695.30, 901.74, 901.74, 1148.18 };
        public static readonly double[] WaypointZ = { 0.0, 120.0, 195.0, 195.0, 195.0, 195.0, 195.0, 387.88, 387.88 };
        /// <summary>Sharp cumulative distance at each waypoint (IMPORT_PLAN 3.1).</summary>
        public static readonly double[] SharpCum = { 0.0, 120.0, 195.0, 410.30, 570.30, 890.30, 1096.74, 1289.62, 1536.06 };
        /// <summary>Fillet radius at each waypoint (0 = no fillet; collinear P1, P3, P4, P5 get none).</summary>
        public static readonly double[] CornerRadii = { 0.0, 0.0, 24.0, 0.0, 0.0, 0.0, 30.0, 30.0, 0.0 };

        // ------------------------------------------------------------------ beats (14.12 row 3, 14.6)
        public const int BeatCount = 7;
        public static readonly double[] BeatBoundaries = { 0.0, 120.0, 400.0, 560.0, 880.0, 1080.0, 1260.0, 1500.0 };
        public static readonly string[] BeatNames = { "alley", "boulevard", "edge", "skybridge", "garden", "signs", "heart" };
        public static readonly double[] BeatSecondsBrisk = { 48.0, 112.0, 64.0, 128.0, 80.0, 72.0, 96.0 };
        public static readonly double[] BeatSecondsWalk = { 80.0, 186.67, 106.67, 213.33, 133.33, 120.0, 160.0 };
        public const double GoalTriggerS = 1497.0;
        public const double GoalS = 1500.0;
        /// <summary>Beat content activation (14.6): switched on 8 m before a boundary, off 12 m behind it.</summary>
        public const double ActivateLead = 8.0;
        public const double Hysteresis = 12.0;
        /// <summary>Rule P (14.6): every per-beat parameter eases with smootherstep over 24 m around a boundary.</summary>
        public const double BlendWindow = 24.0;

        // ------------------------------------------------------------------ autoplay expectations (14.10, IMPORT_PLAN 4.4 d)
        public const double CineTriggerTime = 599.05, CineStopTime = 601.45, CineTimeTol = 2.0;
        public const double WalkTriggerTime = 998.1, WalkStopTime = 1002.1, WalkTimeTol = 3.0;
        public const double EndingSeconds = 17.0;

        // ------------------------------------------------------------------ atmosphere (14.2.2, 14.12 row 4-5)
        public const double FogDownFactor = 1.8;
        public const double FogDownDepth = 1400.0;
        public const double SheetY0 = -250.0, SheetY1 = -700.0, SheetY2 = -1500.0, SheetWidth = 45.0, SheetStrength = 0.5;
        public const double CloudBaseY = 460.0;
        /// <summary>Floor #0A0C0F (LOOK_V2 2.1): blue-black, luma 11.8 (codes 10, 12, 15); the grade applies exposure and vignette
        /// relative to it (LOOK_V2 7), so the dithered 1st percentile of a Floor plateau stays >= 9. Crush threshold Floor + 3.</summary>
        public const float FloorLinR = 0.00304f, FloorLinG = 0.00368f, FloorLinB = 0.00478f;
        public const double FloorLuma = 11.8;
        public const double SkyDomeRadius = 11000.0;
        /// <summary>H3 cloud mix of geometry toward the ceiling colour: smoothstep(380, 560, y) (shaft tops vanish, no roof edge).</summary>
        public const double CloudMixY0 = 380.0, CloudMixY1 = 560.0;
        /// <summary>The cloud deck is seen for e &gt; 4 deg (14.2.2): its weight eases in over 3..5 deg (its fog is ~0.996 there).</summary>
        public const double CloudFadeE0 = 3.0, CloudFadeE1 = 5.0;
        /// <summary>Cloud deck (LOOK_V2 3.1, tint model): base = CloudBase (1 + 1.2 (n - 0.5)) (1 - 0.35 g_gulf), glow W = soft union
        /// of Gaussians (sigma 220 m) with a tint per glow, cloud = lerp(base, tint (1 + 1.0 (n - 0.5)), W) (PA r01: the 3.1 tint
        /// amplitude 0.5 left the lit lid of KS05 / KS18 smooth, band std 0.6-0.9 code; 1.0 gives +-11 % std, about +-3.5 codes,
        /// the +-5-6 codes of billows that 3.1 asks for). n = 4 octaves 900 / 300 /
        /// 110 / 40 m (weights 0.36 / 0.19 / 0.30 / 0.15, std 0.111), the 110 and 40 m octaves domain-warped by 60 m (2 n900 - 1),
        /// each octave faded to its mean below 4..8 px of wavelength; 3 m/s scroll, lattice periodic in 64 cells (exact wrap).</summary>
        public const double CloudGlowSigma = 220.0, CloudBaseAmp = 1.2, CloudTintAmp = 1.0, CloudGulfShade = 0.35;
        public const double CloudNoise1 = 900.0, CloudNoise2 = 300.0, CloudNoise3 = 110.0, CloudNoise4 = 40.0;
        public const double CloudNoiseW1 = 0.36, CloudNoiseW2 = 0.19, CloudNoiseW3 = 0.30, CloudNoiseW4 = 0.15;
        public const double CloudWarp = 60.0, CloudFadePx0 = 4.0, CloudFadePx1 = 8.0, CloudScroll = 3.0;
        public const int CloudNoisePeriod = 64;
        /// <summary>D-006: the cloud term inside NocSkyColor is fogged with min(sigma, 6e-4) (KS02 slit).</summary>
        public const double CloudSkySigmaMax = 6.0e-4;
        /// <summary>D-006: HazeHorizon -> AbyssNear over 0..-6 deg below the horizon (no 6.7-code step).</summary>
        public const double HorizonBlendDeg = 6.0;
        /// <summary>H3 sky modulation for rays that hit nothing below the horizon: 1 + 0.15 sum exp(-((e - e_k)/5)^2) (LOOK_V2 3.1:
        /// +15 %, about +-5 codes of visible bands), e_k = -atan((eye - y_k) / 2 500 m) (-5.7, -15.6, -31 deg undrifted).</summary>
        public const double SheetSkyAmp = 0.15, SheetSkyWidthDeg = 5.0, SheetSkyDistance = 2500.0;
        /// <summary>Haze sheets drift +-15 m with period 120 s (H3); the sheet sum is skipped while min(y0, y1) &gt; -60 m (R2 2.2).</summary>
        public const double SheetDrift = 15.0, SheetDriftPeriod = 120.0, SheetSkipY = -60.0;
        /// <summary>D-006 alley haze: sigma_a - sigma_r as an extra layer weighted 1 - smoothstep(18, 30, y) (podia 22 / 31 m),
        /// over the regional sigma 4.4e-4 (LOOK_V2 3.1; the alley total stays 5.0e-3); it ends with the 14.6 fog window at s 124.
        /// HazeLow share of the layer inside the sky function: lerp(c, HazeLow, 0.6 sa m / (sr + sa m)), m the layer's mean along
        /// the ray to min(6 500 m, the cloud hit) (LOOK_V2 3.1).</summary>
        public const double AlleyHazeY0 = 18.0, AlleyHazeY1 = 30.0, RegionalSigma = 4.4e-4, AlleyHazeEndS = 124.0;
        public const double HazeLowShare = 0.6, HazeLowPath = 6500.0;

        // ------------------------------------------------------------------ wet floor (14.7 H4, D-003, R1 section 2)
        /// <summary>Fresnel F0 of the water film, reflectivity in puddles / elsewhere, puddle area fraction (14.2.5 NocWet).</summary>
        public const double WetF0 = 0.03, WetReflPuddle = 0.95, WetReflDry = 0.5, WetPuddleFraction = 0.35;
        /// <summary>GGX alpha in puddles / elsewhere (14.2.5 "roughness 0.08 / 0.35"); lobe variance sigma2 = alpha^2 + 0.004.</summary>
        public const double WetAlphaPuddle = 0.08, WetAlphaDry = 0.35, WetSigmaFloor = 0.004;
        /// <summary>Ripple slope scale (H4) and the R1 2.6 ring parameters (cell, speed m/s, start radius, life s, wavelength, period x 1/rho).</summary>
        public const double WetRippleSlope = 0.12, RippleCell = 0.6, RippleSpeed = 0.23, RippleR0 = 0.03, RippleLife = 0.9, RippleLambda = 0.04, RipplePeriod = 1.1;
        /// <summary>Puddle noise scales (m) and the 65th-percentile threshold of the 2-octave fBm (measured by WetFloorTests).</summary>
        public const double PuddleScale0 = 4.0, PuddleScale1 = 1.3, PuddleThreshold = 0.5688;
        /// <summary>Analytic sources (H4): at most 32 active, bins of 20 m of s, window s - 20..s + 60, crossfade 12 m; L_eff &lt;= 3.</summary>
        public const int WetSourceMax = 32, WetWallMax = 4;
        public const double WetBinSize = 20.0, WetWindowBack = 20.0, WetWindowAhead = 60.0, WetCrossfade = 12.0, WetLeffMax = 3.0;
        /// <summary>Specular AA of the mirror lobe (R1 2.5): pixel-filter variance, trace clamp, lateral floor in px.</summary>
        public const double WetPixVar = 0.25, WetAaClamp = 0.36, WetLatMinPx = 0.75;

        // ------------------------------------------------------------------ lit template (14.2.3)
        /// <summary>Analytic koi light: 12 spine points, falloff 1 / (1 + (d / 120)^2), I = KoiGlow x 0.07 (LOOK_V2 6 named x 0.7, an R4
        /// exception; PA r01 calibration for the v2 albedos, which are 7 x v1: at x 0.7 the plaza floor under the orbit received a
        /// luminance of 0.76 x 0.7 x 0.606 = 0.32, ten times a 900 x 60 m hologram of radiance 0.3 seen at 400 m (0.34 sr x 0.3 /
        /// pi = 0.032), and painted the plaza and ring towers cyan (KS17 floor 42 codes sat 0.41, near wall 100). At x 0.07 the
        /// floor under the koi gains about +8-10 codes of cool light, the towers +3-6).</summary>
        public const int KoiSpinePoints = 12;
        public const double KoiLightRange = 120.0, KoiLightGain = 0.07;
        /// <summary>Specular anti-aliasing of the lit template (8.11 item 6): screen-space variance 0.25, threshold 0.18.</summary>
        public const double SpecAaVariance = 0.25, SpecAaThreshold = 0.18;
        /// <summary>Smoothness ceiling of lit materials (8.11 item 6: at most 0.95 except analytic water and glass).</summary>
        public const double SmoothnessMax = 0.95;

        // ------------------------------------------------------------------ facade (14.7 H2, 14.2.5 NocFacade, D-004, R1 section 3)
        /// <summary>Residential window pitch (column x floor, m) and window size; a stratum band is 24 residential floors.</summary>
        public const double FacadeColumn = 2.4, FacadeFloor = 3.6, FacadeWinW = 2.0, FacadeWinH = 2.4, FacadeBand = 86.4;
        /// <summary>Stratum pitches and windows (m): commercial 4.8 x 4.32 (20 rows per band; spec 4.2), mechanical slits 9 x 6.1714
        /// (14 rows; spec 6), sky lobby 18 x 14.4 (6 rows; spec 14): each band holds a whole number of rows (DEVELOPMENT D-017).</summary>
        public const double FacadeComP = 4.8, FacadeComH = 4.32, FacadeComWinW = 4.4, FacadeComWinH = 3.2;
        public const double FacadeMechP = 9.0, FacadeMechH = 6.171428571, FacadeMechWinW = 8.4, FacadeMechWinH = 0.8;
        public const double FacadeLobbyP = 18.0, FacadeLobbyH = 14.4, FacadeLobbyWinW = 17.0, FacadeLobbyWinH = 12.0;
        /// <summary>Section bays of the cut faces (mode 1, 14.2.5 NocSection: slab lines every 4 m, columns 9 m, 40 % lit): 22 rows of
        /// 4 m in an 88 m band, bay window 8.2 x 3.2 m.</summary>
        public const double FacadeSecP = 9.0, FacadeSecH = 4.0, FacadeSecBand = 88.0, FacadeSecWinW = 8.2, FacadeSecWinH = 3.2, FacadeSecLit = 0.40;
        /// <summary>Stratum shares (residential, commercial, mechanical, sky lobby) and lit factors; sum of share x factor = 1, so the
        /// mean lit probability stays 0.34.</summary>
        public const double FacadeShareRes = 0.70, FacadeShareCom = 0.15, FacadeShareMech = 0.10;
        public const double FacadeFactorRes = 1.0, FacadeFactorCom = 1.2, FacadeFactorMech = 0.4, FacadeFactorLobby = 1.6;
        /// <summary>Lit probability (mean), epoch (s) and the toggle ramp (s) of the stateless windows (H2).</summary>
        public const double FacadeLit = 0.34, FacadeEpoch = 150.0, FacadeToggle = 1.2;
        /// <summary>Per-axis footprint fade band (px of pitch): smoothstep(2.5, 5.5, pitch_px).</summary>
        public const double FacadeFadePx0 = 2.5, FacadeFadePx1 = 5.5;
        /// <summary>Zone level (R1 3.3, D-017): 24 columns x 2 floors (lit floors read as bands; 8 x 4 read as a square mosaic in the
        /// first captures), p_zone = lit x factor x (0.55 + 0.9 U).</summary>
        public const int FacadeZoneCols = 24, FacadeZoneRows = 2;
        public const double FacadeZoneLo = 0.55, FacadeZoneSpan = 0.9;
        /// <summary>Hue thresholds on U (cold 82 %, cyan 8 %, magenta 6 %, white 4 %) and the warm band delta (R1 3.4).</summary>
        public const double FacadeHueCold = 0.82, FacadeHueCyan = 0.90, FacadeHueMagenta = 0.96, FacadeWarmBand = 0.03;
        /// <summary>Lit window radiance = hue token x FacadeWindowGain x (0.6 + 0.8 U) (calibrated at the first capture, D-017: the mean
        /// facade lands near the 14.10 budget of luma 45).</summary>
        public const double FacadeWindowGain = 0.20;
        /// <summary>City specular occlusion (D-017): a vertical facade reflects at the elevation of the view ray; below ~35 deg that ray
        /// meets other towers, not the sky: env x lerp(0.12, 1, smoothstep(sin 3 deg, sin 35 deg, view elevation)).</summary>
        public const double FacadeOccMin = 0.12, FacadeOccSin0 = 0.0523, FacadeOccSin1 = 0.5736;
        /// <summary>Vertical edge lights: true half-width on each face (m), drawn at least 2 px per face, fraction of towers, gain.</summary>
        public const double FacadeEdgeHalf = 0.3, FacadeEdgeMinPx = 2.0, FacadeEdgeFraction = 0.12, FacadeEdgeGain = 1.0;
        /// <summary>Warm window fraction per beat (14.2.5; beat 2 0.10, beat 7 0: the goal window is the only warm blob), Rule P.</summary>
        public static readonly double[] WarmFraction = { 0.08, 0.10, 0.08, 0.06, 0.06, 0.05, 0.0 };

        // ------------------------------------------------------------------ window and amber schedules (LOOK_V2 4, R6; read by NocFacadeMath)
        /// <summary>Warm share w_f(s) of lit windows, piecewise LINEAR in s (LOOK_V2 4): 0.32 to s 400, 0.20 at s 880, 0 from s 1 250
        /// (peak 0.054 per 100 m = 0.14 % of lit windows per second at 2.5 m/s). Uploaded as _NocFacade.y.</summary>
        public static readonly double[] WarmKnotS = { 0.0, 400.0, 880.0, 1250.0, 1500.0 };
        public static readonly double[] WarmKnotV = { 0.32, 0.32, 0.20, 0.0, 0.0 };
        /// <summary>Neutral (4 000 K) share n_f(s), piecewise linear: 0.30 to s 880, 0.15 from s 1 250. Uploaded as _NocFacade.w.</summary>
        public static readonly double[] NeutralKnotS = { 0.0, 880.0, 1250.0, 1500.0 };
        public static readonly double[] NeutralKnotV = { 0.30, 0.30, 0.15, 0.15 };
        /// <summary>R6: every city-wide SodiumAmber use x k_a(s) = w_f(s) / 0.32 (the remainder goes to SignWhite); the screen wall's
        /// amber stop turns to SignWhite over s 1 230-1 290; per-item hash bands of 0.06 in k_a keep the change pop-free.</summary>
        public const double AmberRef = 0.32, AmberBand = 0.06, ScreenAmberS0 = 1230.0, ScreenAmberS1 = 1290.0;
        /// <summary>Screen-wall ramp (LOOK_V2 10, D-010): after the k 0.8 luminance normalisation the stops are scaled so their geometric-
        /// mean luminance is 0.27 (v1's D-010 stops had 0.255-0.310; the v2 stops alone have 0.404 because of the ice stop), so the
        /// wall displays about 70 codes at beat 6 (KS16 target 60-85; PA r01 measured 84-91 and a frame mean of 68 > 62).</summary>
        public const double ScreenRampY = 0.27;

        // ------------------------------------------------------------------ city field (14.7 H2 tiers, 14.4 places)
        /// <summary>Ring centre (the middle of the path's bounding box): ring 1 to 1.8 km (120 m cells), ring 2 to 6.5 km (240 m).</summary>
        public const double CityCentreX = 575.0, CityCentreZ = 195.0, CityCell1 = 120.0, CityCell2 = 240.0, CityRing1 = 1800.0, CityRing2 = 6500.0;
        /// <summary>Shafts y -3 300..560 (14.0), three stacked boxes with setbacks.</summary>
        public const double CityShaftY0 = -3300.0, CityShaftY1 = 560.0;
        /// <summary>Footprint half-sizes (m), cell margin (m), empty-cell fraction, rotated fraction and angle range (deg), yaw quantum.</summary>
        public const double CityHalfMin = 17.0, CityHalfMax = 32.0, CityCellMargin = 6.0, CityEmptyFraction = 0.06;
        public const double CityRotFraction = 0.25, CityRotMin = 15.0, CityRotMax = 45.0, CityYawQuantum = 0.5;
        /// <summary>Setbacks per side (m) and ledge heights (m): every ledge is above the eye, so no roof face is ever seen.</summary>
        public const double CitySetbackMin = 3.0, CitySetbackMax = 9.0, CityLedgeA0 = 30.0, CityLedgeA1 = 140.0, CityLedgeGap = 60.0, CityLedgeB1 = 360.0;
        /// <summary>Exclusions: margin behind the Gulf faces (terraces, FaceRib) and the Heart radius (plaza R 400, ring towers to R ~600).</summary>
        public const double CityGulfMargin = 150.0, CityHeartRadius = 620.0;
        /// <summary>Camera region of the walk (path box + lane, m): faces that face away from all of it are never built (exact).</summary>
        public const double CityCamX0 = -20.0, CityCamX1 = 1170.0, CityCamZ0 = -20.0, CityCamZ1 = 410.0;
        /// <summary>Aviation beacons (AviationRed, slow 0.2 Hz pulse): fraction of towers within range, heights, size (m), gain.</summary>
        public const double CityBeaconFraction = 0.2, CityBeaconRange = 4000.0, CityBeaconHz = 0.2, CityBeaconSize = 1.2, CityBeaconGain = 2.5;
        public static readonly double[] CityBeaconY = { 150.0, 260.0, 370.0 };

        // ------------------------------------------------------------------ signs and streaks (14.2.5 NocNeon / NocStreak, H2, H7)
        /// <summary>NocNeon: stroke minimum 1.75 px, strokes above 3 px footprint (TQ-08: at 6 px the boulevard blades, 3-6 px strokes at
        /// 100-200 m, read as flat lit panels at 1080p and 1.8-2.0 levels above the -capture-ss 2 reference; 1.75 px drawn strokes with
        /// coverage AA hold down to 1.5 px), else flat clamped to 2.2 px with the Neutral curve's ratio at the stroke peak (start 0.76,
        /// the grade's), breathing +-8 % at &lt;= 0.2 Hz.</summary>
        public const double NeonMinPx = 1.75, NeonFlatPx = 3.0, NeonClampPx = 2.2, NeonBreathAmp = 0.08, NeonBreathHz0 = 0.05, NeonBreathHz1 = 0.2;
        public const double NeonToneStart = 0.76;
        /// <summary>Neon emission HDR factor (LOOK_V2 2.3: signs x1.6) and stroke width as a fraction of the glyph cell width.</summary>
        public const double NeonGain = 1.6, NeonStroke = 0.055;
        /// <summary>NocNeon colour weights (LOOK_V2 10, R6, R7): SodiumAmber 0.28 x k_a, WinNeutral 0.24, NeonCyan 0.18, AviationRed 0.14,
        /// SignWhite 0.12 + 0.28 (1 - k_a), NeonMagenta 0.04, NeonViolet 0; the dark panel face 0.015 x the colour desaturated 60 %.</summary>
        public const double NeonWAmber = 0.28, NeonWNeutral = 0.24, NeonWCyan = 0.18, NeonWRed = 0.14, NeonWWhite = 0.12, NeonWMagenta = 0.04;
        public const double NeonPanel = 0.015, NeonPanelDesat = 0.6;
        /// <summary>In-air halos (LOOK_V2 6 "Light in the air", NocHalos / NocStreak kind 3): Gaussian sigma 0.5 m (lights of range <= 10 m)
        /// or 0.8 m (>= 12 m), the quad spans +-3 sigma, peak 5 % of the head radiance x rho_eff = min(2, sigma_local / 4.4e-4) x
        /// (0.6 + 0.4 rho_rain), faded out below 3 px of sigma (1.5 -> 3 px).</summary>
        public const double HaloPeak = 0.05, HaloSigmaRef = 4.4e-4, HaloSigmaSmall = 0.5, HaloSigmaLarge = 0.8, HaloExtent = 3.0, HaloFadePx0 = 1.5, HaloFadePx1 = 3.0;
        /// <summary>NocStreak: minimum 2.2 px both axes, energy x (true/drawn)^2, streak length max(v x 0.12 s, footprint), light width (m).</summary>
        public const double StreakMinPx = 2.2, StreakSeconds = 0.12, StreakWidth = 0.35;
        /// <summary>Traffic (H7): speed 14 m/s +-3 %, spacing 28 m +-30 %, ramps e 0.04 (lanes) / 0.06 (layers), lane start x -4 000,
        /// time wrapped by a double modulo of 65 536 s (every speed is quantised so v P / L is an integer: no jump at the wrap).</summary>
        public const double TrafficSpeed = 14.0, TrafficSpeedJitter = 0.03, TrafficSpacing = 28.0, TrafficJitter = 0.3;
        public const double TrafficRampLane = 0.04, TrafficRampLayer = 0.06, TrafficLaneX0 = -4000.0, TrafficTimeWrap = 65536.0;
        public const double TrafficTailGain = 1.6, TrafficHeadGain = 2.2;

        // ------------------------------------------------------------------ rain (14.7 H5, D-005, R2 section 1)
        /// <summary>21 whole cells around the circle (no seam at south), cell height 0.8 z, 6 slots per cell at probability 0.9 (D-005).</summary>
        public const int RainCells = 21, RainSlots = 6;
        public const double RainCellH = 0.8, RainSlotP = 0.9;
        /// <summary>Drop capsule 0.30 m long, 0.012 m wide (drawn at least 2.2 px at 1080p), cell margins (m), soft presence band.</summary>
        public const double RainDropLen = 0.30, RainDropWidth = 0.012, RainMinPx = 2.2, RainMarginH = 0.09, RainMarginV = 0.16, RainPresenceBand = 0.06;
        /// <summary>Zenith fade of each layer by its streak length: smoothstep(3, 8 px at 1080p).</summary>
        public const double RainFadePx0 = 3.0, RainFadePx1 = 8.0;
        /// <summary>Brightness 0.35 (0.04 + 0.9 clamp(Lloc / 0.8, 0, 1.5)) x smoothstep(0, 0.15, Lloc), tint 30 % (Look v2, CRIT0 #28:
        /// the drops take the local light's hue at about x0.3 saturation, no coloured sparks), clamp 0.8 (exposed).</summary>
        public const double RainB0 = 0.35, RainB1 = 0.04, RainB2 = 0.9, RainLlocRef = 0.8, RainLlocMax = 1.5, RainLumaGate = 0.15, RainTint = 0.3, RainTintMax = 2.5, RainClamp = 0.8;
        /// <summary>Density noise rho (0.55 + 0.45 vnoise), 11 lattice cells per turn; tilt 0.10 sin(0.05 t); beat-4 gusts 0.28 rad, 17 s.</summary>
        public const double RainNoiseBase = 0.55, RainNoiseAmp = 0.45, RainTiltAmp = 0.10, RainTiltRate = 0.05, RainGust = 0.28, RainGustPeriod = 17.0;
        public const int RainNoiseCells = 11;
        /// <summary>The Lloc pre-pass runs at 1/8 resolution.</summary>
        public const int RainLightDiv = 8;

        // ------------------------------------------------------------------ camera (14.12 row 5)
        public const float CameraNear = 0.05f;
        public const float CameraFar = 12000f;
        public const int Msaa = 4;

        // ------------------------------------------------------------------ schedules of 14.2.4 (knots: s, value; smootherstep between knots)
        /// <summary>vFOV schedule (one eased curve, 14.2.4).</summary>
        public static readonly double[] FovKnotS = { 0, 80, 130, 200, 250, 400, 460, 560, 610, 860, 940, 1060, 1120, 1400, 1470, 1500 };
        public static readonly double[] FovKnotV = { 62, 62, 64, 64, 66, 66, 68, 68, 66, 66, 62, 62, 66, 66, 62, 62 };
        /// <summary>EV schedule (manual postExposure, LOOK_V2 7: -0.1 / +0.1 / 0.0 / 0.0 / -0.2 / -0.1 / 0.0 per beat; EV is applied
        /// relative to the Floor). Peak slope 0.031 EV/s at 2.5 m/s.</summary>
        public static readonly double[] EvKnotS = { 0, 110, 140, 380, 410, 560, 600, 860, 900, 1060, 1110, 1250, 1290, 1500 };
        public static readonly double[] EvKnotV = { -0.1, -0.1, 0.1, 0.1, 0.0, 0.0, 0.0, 0.0, -0.2, -0.2, -0.1, -0.1, 0.0, 0.0 };
        /// <summary>Fog sigma schedule (14.12 row 4 with LOOK_V2 3.1: 5.0e-3 for s &lt; 96, 4.4e-4 for 124-860, 6.0e-4 for 880-1060,
        /// 4.5e-4 for 1110-1250, 3.4e-4 from 1290).</summary>
        public static readonly double[] FogKnotS = { 0, 96, 124, 860, 880, 1060, 1110, 1250, 1290, 1500 };
        public static readonly double[] FogKnotV = { 5.0e-3, 5.0e-3, 4.4e-4, 4.4e-4, 6.0e-4, 6.0e-4, 4.5e-4, 4.5e-4, 3.4e-4, 3.4e-4 };
        /// <summary>Per-beat rain density rho (14.2.4), blended with Rule P.</summary>
        public static readonly double[] RainRho = { 0.55, 0.80, 0.80, 1.00, 0.35, 0.70, 0.60 };
        public static readonly double[] RainLayerZ = { 3.0, 6.0, 12.0, 20.0 };
        public const double RainFallSpeed = 9.0;
        /// <summary>Per-beat post (LOOK_V2 7): bloom threshold / intensity / scatter, saturation (+%), vignette (relative to the Floor).</summary>
        public static readonly double[] BloomThreshold = { 0.9, 0.9, 0.9, 0.9, 0.85, 0.9, 0.9 };
        public static readonly double[] BloomIntensity = { 0.40, 0.45, 0.40, 0.40, 0.35, 0.40, 0.45 };
        public static readonly double[] BloomScatter = { 0.70, 0.75, 0.75, 0.72, 0.70, 0.75, 0.75 };
        public static readonly double[] Saturation = { 0, 4, 2, 0, 0, 4, 4 };
        public static readonly double[] Vignette = { 0.16, 0.14, 0.14, 0.15, 0.16, 0.14, 0.15 };
        public const double Contrast = 0.0;
        /// <summary>Bloom prefilter HDR clamp (14.2.4 clamp column; Thread used 64).</summary>
        public const double BloomClamp = 30.0;
        /// <summary>Slew limits of the atmosphere writer (14.6): EV per second, bloom per second, vFOV degrees per second.</summary>
        public const double EvSlew = 0.30, BloomSlew = 0.1, FovSlew = 1.0;

        // ------------------------------------------------------------------ dither (14.7 H1, 14.12 row 9, D-007)
        /// <summary>Peak of the TPDF in 8-bit codes for display luma Y' &lt;= DitherY0.</summary>
        public const double DitherPeak = 2.0;
        /// <summary>Encode-test ladder maximum (11.12, 14.7 H1).</summary>
        public const double DitherLadder = 2.5;
        /// <summary>amp = peak x lerp(1, DitherBrightScale, saturate((Y' - DitherY0) / DitherYRange)).</summary>
        public const double DitherY0 = 0.35, DitherYRange = 0.4, DitherBrightScale = 0.6;
        /// <summary>D-007 chroma component: +-1 code, constant per 2 x 2 pixel block.</summary>
        public const double DitherChroma = 1.0;
        public const int DitherTile = 64;
        public const double DitherSigma = 1.5;

        // ------------------------------------------------------------------ lights (14.12 row 10)
        public const int MaxLocalLights = 32;

        // ------------------------------------------------------------------ audio (14.3, 14.12 row 10)
        public const double AudioSampleRate = 48000.0;
        /// <summary>Limiter ceiling -1.5 dBFS = 0.8414 linear; peak gate -1 dBFS.</summary>
        public const double LimiterCeilingDb = -1.5;
        public const double LimiterCeiling = 0.8414;
        public const double LimiterLookAheadMs = 5.0, LimiterReleaseMs = 80.0;
        public const double DcBlockHz = 20.0;
        public static readonly double[] TargetRmsDb = { -34.9, -30.4, -31.3, -29.6, -33.3, -30.1, -28.9 };

        // ------------------------------------------------------------------ UI (14.11, 14.12 row 11)
        public const string Title = "NOCTURNE";
        public const string Hint = "WASD  move    Mouse  look    Shift  faster    C  cinematic";
        public const string EndLine = "One lamp is still lit.";
        public const double FadeInStart = 0.6, FadeInEnd = 3.1;
        public const double TitleIn0 = 0.8, TitleIn1 = 2.0, TitleOut0 = 4.0, TitleOut1 = 5.4;
        public const double HintAt = 6.0, HintDur = 6.0, HintFade = 0.6, HintGone = 0.4;
        /// <summary>Ending (from the goal trigger): 1.5 s picture, 4.0 s fade (audio 6 s), black to 7.0 s, line 2 + 6 + 2 s, total 17.0 s.</summary>
        public const double EndPictureHold = 1.5, EndFade = 4.0, EndAudioFade = 6.0, EndLineIn0 = 7.0, EndTextIn = 2.0, EndTextHold = 6.0, EndTextOut = 2.0, EndTotal = 17.0;
        public const double TitleFontPx = 67.0, TitleTrackingPx = 9.0, TitleCentre = 0.33, TitleAlpha = 0.92;
        public const double EndFontPx = 51.0, EndCentre = 0.33;
        public const double HintAlpha = 0.7, HintBottom = 0.06;

        // ------------------------------------------------------------------ cinematic gaze (14.5, all keys relative to the path heading)
        /// <summary>Gaze keys (s, yaw relative to the heading at s, pitch), smootherstep between consecutive keys; the repeated
        /// (585, -2, +2) at the beat 3/4 seam is kept once (IMPORT_PLAN 4.4 b).</summary>
        public static readonly GazeKey[] GazeKeys =
        {
            // 14.5.1 alley
            new GazeKey(0, 0, -2), new GazeKey(8, 0, -2), new GazeKey(20, -6, 2), new GazeKey(34, -14, 4), new GazeKey(40, -14, 4),
            new GazeKey(54, 12, 5), new GazeKey(66, 0, 10), new GazeKey(76, 0, 40), new GazeKey(88, 0, 66), new GazeKey(95.5, 0, 66),
            // QA-F03 (integration round): the descent from the KS02 slit ends at s 115 (was 110: 7.8 s instead of 5.8 s), so the
            // lit wall lamp and the slit sky leave the frame at 0.74 x the old pitch rate: cinematic luma step at s 107.9 0.99 levels
            // / frame (was 1.51; 14.10 limit 1.2); KS02 readable 6.52 s, the KS03 pose (s 117, pitch 14) reached 2 m before its shot
            new GazeKey(115, 0, 14), new GazeKey(118, 0, 14),
            // 14.5.2 boulevard. D-040 (C10): KS04 pitch 22 -> 27. D-034 4.1 (C10 / C25): the long look-up before KS05, rise from the
            // KS06 tram glance to +63 over s 270-286 (6.4 s, peak 13.5 deg/s), hold to s 296 (4 s at the top: the bridge sits in the
            // lower third, the towers run into the deck), ease to the 14.10 KS05 pose +52 by s 304 (KS05 readable 301.7-339.3)
            new GazeKey(128, 12, 24), new GazeKey(150, 25, 32), new GazeKey(171, 30, 30), new GazeKey(209, 0, 24), new GazeKey(250, 0, 27),
            new GazeKey(258, -33, 14), new GazeKey(270, -33, 14), new GazeKey(286, 0, 63), new GazeKey(296, 0, 63), new GazeKey(304, 0, 52), new GazeKey(330, 0, 52),
            new GazeKey(350, 10, 30), new GazeKey(380, 0, 12), new GazeKey(400, 0, 4),
            // 14.5.3 edge
            // D-043 (THE ARK, FX, on DR's course: the Ark comes DOWN the rift from the north haze, bow first, over z 195 at s 596): the
            // KS09 long look-up of the 1 800 m hull (D-034 4.1, s 476-499 up to +58 NE) is replaced by the look up the rift at the
            // coming bow: after the falling panel's tear (s 470-476) the gaze turns to rel -80 / +16 (world 10), where the bow's belly
            // sits 14.6-15.2 deg up at s 494-498 (1.2-1.3 km out; off the frame centre < 1.3 deg), held 7.8 s, then down to the KS09
            // pose by s 520 (KS09 readable 516.0-533.8 = 7.12 s; peak 8.3 / 7.7 deg/s)
            new GazeKey(402, 0, 2), new GazeKey(428, -62, 24), new GazeKey(436, -62, 24), new GazeKey(450, -20, 10), new GazeKey(470, -50, -6),
            new GazeKey(476, -50, -8), new GazeKey(492, -80, 16), new GazeKey(500, -80, 16),
            // D-029 (GB-04): the plunge pose -95 / -32 (V's pose search: 40.1 % drop on the plan in KS10). C12 (D-040): held s 538-548
            // and forward by s 568, so the gaze leaves the drop before gate A's tunnel (x 372-384 = s 556.7-568.7); KS10 readable
            // 536.0-554.3 (7.3 s), peak yaw 20.98 deg/s
            new GazeKey(520, -70, -18), new GazeKey(528, -70, -18), new GazeKey(538, -95, -32), new GazeKey(548, -95, -32), new GazeKey(568, -2, 2),
            // D-043: out of gate A's tunnel (s 568.7) the gaze lifts left into the Ark's bow coming over the bridge's head (rel -58..-55 /
            // +40..+36, world 32-35: the bow 270-130 m out and 50-68 deg up at s 575-590, leaving the frame's top as it passes over),
            // with no turn while the bridge lamp head the walker passes at s 585.0 (SkybridgeDims lamp 1: 1.85 m left, 2.35 m over the
            // eye) and its halo leave the frame's left edge (14.10 luma step <= 1.2 levels / frame: keys 576 / 582 with the turn at
            // s 582 measured 1.262 (FX_arkseq), 576 / 586 at +42 1.186 (FX_arkseq2), world 26-28 1.70 (FX_arkseq4); this sinking
            // hold had the lowest step of four variants rendered frame by frame (FX_lamp01: 0.77 vs 0.88 for the 1.186 one)), then down
            // to KS11 (readable 594.7-609.5) as the bow passes overhead (s 596, 72 deg up); KS11's hold and the turn north after it
            // moved 1-3 m later (KS12 readable 643.7-667.7)
            new GazeKey(576, -58, 40), new GazeKey(586, -55, 36),
            // 14.5.4 skybridge
            new GazeKey(595, -14, 2), new GazeKey(603, -14, 2), new GazeKey(617, -50, 4), new GazeKey(627, -90, -10),
            new GazeKey(660, -90, -20), new GazeKey(680, -40, -4), new GazeKey(700, 40, -14), new GazeKey(716, 0, -75), new GazeKey(724, 0, -75),
            // D-043: the long look-up under the Ark's belly (ARK.md, FX): up from the KS13 glass to the north (rel -95 / +28 over s
            // 748-766: the endless lit belly comes out of the northern haze toward the walker, the stern 3.1-3.3 km out at 6 deg near the
            // frame's lower edge, its rows converging on it; 6.2 s held), then up to near the zenith, a little east of north (rel
            // -78..-76 / +74..+76 over s 782-822, world 12-14: chosen from 15 poses on DR's Ark, QA/captures/FX_zen01 z015_76: the
            // belly and its gondolas fill about 85 % of the frame, the bridge's upper tube a thin line across it; world 55 put the
            // belly's east edge through the centre, world 315-355 a large haze-lit slab on the left), held through s 822 so that
            // lamp 17 (s 821.8, left) leaves the frame before the turn down (FX_arkseq3 with the turn at s 814: a 1.351-level luma step
            // at s 823.8; lamp model: 1.93 -> no crossing above 1.53 NDC/s in s 740-860), sliding south overhead; down to the KS14 pose
            // by s 841 (KS14 readable 6.57 s)
            new GazeKey(748, -50, 10), new GazeKey(766, -95, 28), new GazeKey(782, -95, 28), new GazeKey(806, -78, 74), new GazeKey(822, -76, 76),
            new GazeKey(841, 0, 18), new GazeKey(846, 0, 18), new GazeKey(860, 0, 6),
            // 14.5.5 garden
            new GazeKey(880, 0, 6), new GazeKey(930, 0, 20), new GazeKey(970, 0, 50), new GazeKey(1000, 0, 50), new GazeKey(1040, 10, 10),
            new GazeKey(1060, -10, 6),
            // 14.5.6 signs
            new GazeKey(1100, -20, 12), new GazeKey(1125, -30, 26), new GazeKey(1150, -35, 34), new GazeKey(1180, -40, 40), new GazeKey(1210, -20, 20),
            new GazeKey(1236, 0, 8),
            // 14.5.7 heart
            // D-034 4.1 (C25): the KS17 long look-up after the reveal hold: +18 -> +60 over s 1310-1327 (6.8 s, peak 11 deg/s), 2.4 s at
            // the top under the koi, down to the KS18 pose by s 1346 (KS17 readable 1273.8-1316.5, KS18 1342.0-1378.4)
            new GazeKey(1250, 25, 6), new GazeKey(1284, 10, 14), new GazeKey(1296, 6, 18), new GazeKey(1310, 6, 18), new GazeKey(1327, -10, 60),
            new GazeKey(1333, -10, 60), new GazeKey(1346, -20, 46),
            new GazeKey(1366, -20, 46), new GazeKey(1400, 0, 12), new GazeKey(1440, 4, 14), new GazeKey(1470, 6, 24), new GazeKey(1500, 6, 28),
        };
        /// <summary>Gaze follower (14.5): critically damped omega (rad/s), yaw / pitch rate limits (deg/s), sway radius (deg).</summary>
        public const double LookOmega = 2.5, YawRateMax = 22.0, PitchRateMax = 18.0, SwayMax = 0.3;
        /// <summary>QA walk (autotest): look forward along the heading at this pitch.</summary>
        public const double GazeDefaultPitch = 0.0;

        // ------------------------------------------------------------------ key shots (14.10 poses; luma window = the beat's mean-luma window of
        // LOOK_V2 8.1 in codes / 255: 1: 14-40, 2: 24-54, 3: 24-50, 4: 24-50, 5: 20-44, 6: 32-64, 7: 18-60; the beat comes from s)
        // D-029: KS03 at s 117 (blade x 90 fully visible; 65 % hidden at s 116), KS13 at s 718 (follower lag 0.43 deg; 2.12 at 716),
        // KS10 world yaw -5 / pitch -32 (the GB-04 gaze keys).
        public static readonly KeyShot[] KeyShots =
        {
            new KeyShot("KS00_alley_start", 0, 0, 1.7, 0, 0, -2, 62, 0.0549, 0.1569),
            new KeyShot("KS01_alley_noodle", 36, 0, 1.7, 36, -14, 4, 62, 0.0549, 0.1569),
            new KeyShot("KS02_alley_slit", 88, 0, 1.7, 88, 0, 66, 62.1, 0.0549, 0.1569),
            new KeyShot("KS03_boulevard_mouth", 117, 0, 1.7, 117, 0, 14, 63.8, 0.0549, 0.1569),
            new KeyShot("KS04_boulevard_axis", 250, 65.30, 1.7, 195, 90, 27, 66, 0.0941, 0.2118),   // D-040: pitch 22 -> 27 (C10)
            new KeyShot("KS06_tram", 264, 79.30, 1.7, 195, 57, 14, 66, 0.0941, 0.2118),
            new KeyShot("KS05_boulevard_up", 310, 125.30, 1.7, 195, 90, 52, 66, 0.0941, 0.2118),
            new KeyShot("KS07_edge_window", 402, 217.30, 1.7, 195, 90, 2, 66, 0.0941, 0.1961),
            new KeyShot("KS08_edge_flank", 428, 243.30, 1.7, 195, 28, 24, 66.9, 0.0941, 0.1961),
            new KeyShot("KS09_edge_gulf", 520, 335.30, 1.7, 195, 20, -18, 68, 0.0941, 0.1961),
            new KeyShot("KS10_edge_plunge", 548, 363.30, 1.7, 195, 355, -32, 68, 0.0941, 0.1961),
            new KeyShot("KS11_bridge_train", 600, 415.30, 1.7, 195, 76, 2, 66.1, 0.0941, 0.1961),
            new KeyShot("KS12_bridge_void", 660, 475.30, 1.7, 195, 0, -20, 66, 0.0941, 0.1961),
            new KeyShot("KS13_bridge_glass", 718, 533.30, 1.7, 195, 90, -75, 66, 0.0941, 0.1961),
            new KeyShot("KS14_garden_gate", 846, 661.30, 1.7, 195, 90, 18, 66, 0.0941, 0.1961),
            new KeyShot("KS15_garden_canopy", 970, 785.30, 1.7, 195, 90, 50, 62, 0.0784, 0.1725),
            new KeyShot("KS16_signs_wall", 1150, 901.74, 1.7, 271.44, 325, 34, 66, 0.1255, 0.2510),
            new KeyShot("KS17_heart_reveal", 1296, 944.18, 1.7, 387.88, 96, 18, 66, 0.0706, 0.2353),
            new KeyShot("KS18_koi_pass", 1350, 998.18, 1.7, 387.88, 70, 46, 66, 0.0706, 0.2353),
            new KeyShot("KS19_goal_window", 1500, 1148.18, 1.7, 387.88, 96, 28, 62, 0.0706, 0.2353),
        };

        // ------------------------------------------------------------------ world (14.4)
        public const double SpireX = 1330.0, SpireZ = 388.0, PlazaRadius = 400.0;
        public const double GoalWindowX = 1156.0, GoalWindowY0 = 4.4, GoalWindowY1 = 5.9, GoalWindowZ0 = 387.3, GoalWindowZ1 = 388.5;
        public const double GoalLampX = 1155.6, GoalLampY = 5.2, GoalLampZ = 387.9;
        /// <summary>GB-18 (D-029): the goal lamp's 6 px core displays at hue 28-48 deg. GoalLamp x 3.5 through the Neutral curve's mix
        /// toward white displays at hue 28.3 (the edge; the graybox measured 27.6), so the core emission is GoalLamp with its blue
        /// channel x 0.3: hue 32.7, saturation 0.42, luma 210 at beat 7's grade (GradeTests.GoalLampCore_HueInWindow).</summary>
        public const double GoalLampCoreBlue = 0.3, GoalLampCoreHueMin = 28.0, GoalLampCoreHueMax = 48.0;

        // ------------------------------------------------------------------ thin lines (14.2.5 NocLine)
        public const double LineMinWidthPx = 1.75, LineMinWidthFloorPx = 1.5, LineEnergyFloor = 0.35;
    }

    /// <summary>A cinematic gaze key: progress s (m), yaw relative to the path heading at s (deg), pitch (deg, up positive).</summary>
    public readonly struct GazeKey
    {
        public readonly double s, yaw, pitch;
        public GazeKey(double s, double yaw, double pitch) { this.s = s; this.yaw = yaw; this.pitch = pitch; }
    }

    /// <summary>A 14.10 key shot (Thread's struct shape): tag, s, eye (x, y, z), world yaw / pitch, vFOV, mean-luma window (0..1).</summary>
    public struct KeyShot
    {
        public string tag;
        public double s, x, y, z, yaw, pitch, fov, lumaMin, lumaMax;
        public bool wide;
        public KeyShot(string tag, double s, double x, double y, double z, double yaw, double pitch, double fov, double lumaMin, double lumaMax, bool wide = false)
        { this.tag = tag; this.s = s; this.x = x; this.y = y; this.z = z; this.yaw = yaw; this.pitch = pitch; this.fov = fov; this.lumaMin = lumaMin; this.lumaMax = lumaMax; this.wide = wide; }
    }
}
