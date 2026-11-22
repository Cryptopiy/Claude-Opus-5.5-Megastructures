// NocSchedule: every parametric value of 14.2.4 as a pure function of the progress s (Part 3.1 (c): parameters are keyed to
// metres, never to time). The knot curves and per-beat tables live in NocturneConstants; Rule P of 14.6 (24 m smootherstep
// around a boundary) is NocMath.BeatBlend. The AtmosphereDriver (one writer per frame) applies slew limits on top.
// NocFogMath: the double-precision C# mirror of Shaders/Include/NocAtmosphere.hlsl (14.2.2, H3, D-006, R2 section 2): sky
// gradient with the D-006 horizon blend, the abyss of H3 (AT: deep glow and three textured haze sheets), cloud deck (LOOK_V2 3.1 tint model: glow Gaussians with a
// tint per glow over a periodic 4-octave noise, fogged like geometry), the HazeLow share of the alley layer, path-integral fog
// (height term, erf haze sheets, capped alley haze), cloud mix, NocFloor. The
// AtmosphereDriver fills a NocAtmoState from s and the scene clock and uploads exactly those numbers; AtmosphereTests checks
// T-FOG, monotonicity, horizon agreement and the horizon luma profile on this mirror. No UnityEngine dependency.
using System;

namespace Megastructure
{
    public static class NocSchedule
    {
        /// <summary>Vertical FOV in degrees at 16:9 (14.2.4 schedule, Hor+).</summary>
        public static double Fov(double s) => NocMath.Knots(NocturneConstants.FovKnotS, NocturneConstants.FovKnotV, s);
        /// <summary>Manual exposure (EV, postExposure) at s.</summary>
        public static double ExposureEV(double s) => NocMath.Knots(NocturneConstants.EvKnotS, NocturneConstants.EvKnotV, s);
        /// <summary>Fog sigma_beat at s (1/m), the 14.12 schedule (5.0e-3 in the alley).</summary>
        public static double FogSigma(double s) => NocMath.Knots(NocturneConstants.FogKnotS, NocturneConstants.FogKnotV, s);
        /// <summary>D-006: the regional sigma under the alley haze layer (4.4e-4 for s &lt; 124, the schedule elsewhere).</summary>
        public static double FogSigmaRegional(double s) => s < NocturneConstants.AlleyHazeEndS ? NocturneConstants.RegionalSigma : FogSigma(s);
        /// <summary>D-006: the alley haze extra sigma_a - sigma_r (4.56e-3 to s 96, eased to 0 at s 124 with the 14.6 window).</summary>
        public static double AlleyHaze(double s) => Math.Max(0.0, FogSigma(s) - FogSigmaRegional(s));
        /// <summary>Rain density rho at s (0..1).</summary>
        public static double RainRho(double s) => NocMath.BeatBlend(NocturneConstants.RainRho, s);
        public static double BloomThreshold(double s) => NocMath.BeatBlend(NocturneConstants.BloomThreshold, s);
        public static double BloomIntensity(double s) => NocMath.BeatBlend(NocturneConstants.BloomIntensity, s);
        public static double BloomScatter(double s) => NocMath.BeatBlend(NocturneConstants.BloomScatter, s);
        /// <summary>Saturation in the URP -100..100 convention (+8 = factor 1.08).</summary>
        public static double Saturation(double s) => NocMath.BeatBlend(NocturneConstants.Saturation, s);
        public static double Vignette(double s) => NocMath.BeatBlend(NocturneConstants.Vignette, s);
        public static double Contrast(double s) => NocturneConstants.Contrast;

        /// <summary>Piecewise-LINEAR knot schedule (constant outside the knots): the window shares of LOOK_V2 4 are linear in s.</summary>
        public static double LinearKnots(double[] ks, double[] kv, double s)
        {
            int n = ks.Length;
            if (s <= ks[0]) return kv[0];
            if (s >= ks[n - 1]) return kv[n - 1];
            int i = 0;
            while (i < n - 2 && s >= ks[i + 1]) i++;
            double span = ks[i + 1] - ks[i];
            return span > 0 ? kv[i] + (kv[i + 1] - kv[i]) * (s - ks[i]) / span : kv[i + 1];
        }
        /// <summary>Warm share w_f(s) of lit windows (LOOK_V2 4): 0.32 to s 400, 0.20 at s 880, 0 from s 1 250 (_NocFacade.y).</summary>
        public static double WarmShare(double s) => LinearKnots(NocturneConstants.WarmKnotS, NocturneConstants.WarmKnotV, s);
        /// <summary>Neutral (4 000 K) share n_f(s) (LOOK_V2 4): 0.30 to s 880, 0.15 from s 1 250 (_NocFacade.w).</summary>
        public static double NeutralShare(double s) => LinearKnots(NocturneConstants.NeutralKnotS, NocturneConstants.NeutralKnotV, s);
        /// <summary>R6 amber factor k_a(s) = w_f(s) / 0.32: 1 to s 400, 0.625 at s 880, 0 from s 1 250.</summary>
        public static double AmberK(double s) => WarmShare(s) / NocturneConstants.AmberRef;
        /// <summary>R6: the screen wall's amber stop weight (1 -> 0 over s 1 230-1 290, smoothstep; the rest goes to SignWhite).</summary>
        public static double ScreenAmber(double s) => 1.0 - NocMath.Smoothstep(NocturneConstants.ScreenAmberS0, NocturneConstants.ScreenAmberS1, s);
    }

    /// <summary>Linear RGB triple in double (the mirror's colour type).</summary>
    public struct C3
    {
        public double r, g, b;
        public C3(double r, double g, double b) { this.r = r; this.g = g; this.b = b; }
        public static C3 operator +(C3 a, C3 b) => new C3(a.r + b.r, a.g + b.g, a.b + b.b);
        public static C3 operator -(C3 a, C3 b) => new C3(a.r - b.r, a.g - b.g, a.b - b.b);
        public static C3 operator *(C3 a, double k) => new C3(a.r * k, a.g * k, a.b * k);
        public static C3 Lerp(C3 a, C3 b, double t) => new C3(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t);
        public double Luma => NocMath.Luma709(r, g, b);
        public override string ToString() => $"({r:F5}, {g:F5}, {b:F5})";
    }

    /// <summary>
    /// Cloud-glow centres (LOOK_V2 3.1; Gaussians sigma 220 m over the boulevard axis, the sign wall, the spire and the koi): a
    /// segment (ax, az) - (bx, bz) with an optional ring radius (distance to the segment minus the radius), amplitude, sigma and
    /// a tint (the deck colour over that district: TintHot CloudHot, TintCool CloudCool, TintBase CloudBase). Tint Shadow marks
    /// the optional Gulf shadow: it multiplies the base deck by (1 - 0.35 g) instead of joining the glow union (amplitudes stay
    /// positive). Beat builders may move or add centres (Set); the AtmosphereDriver uploads all of them every frame. At most 8.
    /// </summary>
    public static class NocCloudGlow
    {
        public const int Max = 8;
        public const int TintHot = 0, TintCool = 1, TintBase = 2, Shadow = 3;
        public static readonly double[] Seg = new double[4 * Max];   // ax, az, bx, bz
        public static readonly double[] Par = new double[4 * Max];   // amplitude, sigma, ring radius, tint (uploaded as _NocGlowPar)
        public static int Count;

        static NocCloudGlow() { Defaults(); }

        /// <summary>The LOOK_V2 3.1 default set: boulevard axis (two legs, CloudHot), sign wall face, spire and koi orbit (R 260
        /// about the spire, CloudCool). The Gulf shadow is off by default. Never a warm glow near the Old Quarter.</summary>
        public static void Defaults()
        {
            Count = 0;
            double sg = NocturneConstants.CloudGlowSigma;
            Set(0, 0.0, 120.0, 0.0, 195.0, 0.85, sg, 0.0, TintHot);           // boulevard axis, north leg (P1 - C1)
            Set(1, 0.0, 195.0, 215.3, 195.0, 0.95, sg, 0.0, TintHot);         // boulevard axis, east leg (C1 - P3): the lid of KS04-KS06
            Set(2, 874.0, 275.0, 874.0, 520.0, 0.60, sg, 0.0, TintCool);      // screen wall face x 874 (14.4)
            Set(3, NocturneConstants.SpireX, NocturneConstants.SpireZ, NocturneConstants.SpireX, NocturneConstants.SpireZ, 0.90, sg, 0.0, TintCool);   // spire
            Set(4, NocturneConstants.SpireX, NocturneConstants.SpireZ, NocturneConstants.SpireX, NocturneConstants.SpireZ, 0.55, sg, 260.0, TintCool); // koi orbit
        }

        /// <summary>Sets glow i (0..7); Count grows to cover it.</summary>
        public static void Set(int i, double ax, double az, double bx, double bz, double amplitude, double sigma, double ringRadius, int tint = TintHot)
        {
            if (i < 0 || i >= Max) return;
            Seg[4 * i] = ax; Seg[4 * i + 1] = az; Seg[4 * i + 2] = bx; Seg[4 * i + 3] = bz;
            Par[4 * i] = amplitude; Par[4 * i + 1] = sigma; Par[4 * i + 2] = ringRadius; Par[4 * i + 3] = tint;
            if (i + 1 > Count) Count = i + 1;
        }

        /// <summary>The optional Gulf shadow of LOOK_V2 3.1 (segment (515, -800) - (515, 1 200), sigma 220, a 1.0) as glow i.</summary>
        public static void SetGulfShadow(int i) => Set(i, 515.0, -800.0, 515.0, 1200.0, 1.0, NocturneConstants.CloudGlowSigma, 0.0, Shadow);
    }

    /// <summary>
    /// AT constants of the abyss (H3) and the deck form (INT-11), the twins of the "#define NA_" lines of NocAtmosphere.hlsl
    /// (AtmosphereTests.AbyssConstants_HlslMatchesCSharp parses the shader and compares every value). The haze sheets act on geometry
    /// with strength SheetFogStrength (x sigma x the Gaussian layers, H3) and show in the sky (rays that hit nothing below the horizon)
    /// as art-directed bands at e_k (the far edge of each layer: exp(-(x / 1.6)^2) above e_k, exp(-(x / 5.5)^2) below) plus the layer
    /// seen from above, (1 - exp(-K_k / sin|e|)) x its fog visibility V_k, both times a billow coverage smoothstep(0.45, 0.56, n) on
    /// the sheet plane, composited deep to near over the base gradient and the deep glow of the lit city (plane y -2000).
    /// </summary>
    public static class NocAbyss
    {
        public const double SheetFogStrength = 1.5;
        public const double BandA0 = 0.75, BandA1 = 0.85, BandA2 = 0.95, BandUp = 1.6, BandDn = 5.5;
        public const double TopK0 = 0.5, TopK1 = 0.5, TopK2 = 0.5, TopRamp = 12.0;
        /// <summary>Coverage half-width max(0.10, 10 g) around n 0.5 (g = the noise change per pixel); octaves faded below 6..12 px.</summary>
        public const double CovHalf = 0.10, CovAA = 10.0, SheetFadePx0 = 6.0, SheetFadePx1 = 12.0, SheetMaxA = 0.95, SheetDetail = 0.20, SheetThick = 0.35;
        public const double SheetL0 = 1200.0, SheetL1 = 400.0, SheetL2 = 133.333333, SheetL3 = 44.444444;
        public const double SheetW0 = 0.30, SheetW1 = 0.30, SheetW2 = 0.25, SheetW3 = 0.15;
        /// <summary>Sheet veils (fifth start, AT_r01 / r02): each sheet multiplies what lies under it, c = lerp(c, c x veil_k x shade,
        /// a_k): dark veils over glowing depths (0.66 / 0.55 / 0.55; the near one lighter and a little cooler: it lies along the
        /// horizon band), so holes show the brighter deep.</summary>
        public const double Veil0R = 0.65, Veil0G = 0.66, Veil0B = 0.68;
        public const double Veil1R = 0.54, Veil1G = 0.55, Veil1B = 0.56;
        public const double Veil2R = 0.54, Veil2G = 0.55, Veil2B = 0.55;
        /// <summary>Lift of the depths under the sheets: c x (1 + 0.7 L(e)), L = smoothstep(3, 30, -e) (1 - smoothstep(30, 80, -e)).</summary>
        public const double DeepLift = 0.7, LiftE0 = 3.0, LiftE1 = 30.0, LiftE2 = 80.0, LiftNadir = 1.0;
        /// <summary>D-034 occluders (DREAD 2.1 hull in the deck, 3.1 shadow in the haze plane y -650): soft ellipse, beam 0.15 x length,
        /// edge from 0.6 of the radius; identity while the strength is 0.</summary>
        public const double OccBeam = 0.15, OccEdge = 0.6, HazeOccY = -650.0;
        /// <summary>Base gradient below the horizon: lerp(AbyssNear, AbyssGlow, 1 - (1 - |e| / 90)^1.3) (monotone, flat at the nadir).</summary>
        public const double AbyssExp = 1.3;
        public const double DeepY = -2000.0, DeepAmp = 0.5, DeepL0 = 675.0, DeepL1 = 225.0, DeepW0 = 0.55, DeepW1 = 0.45, DeepE0 = 10.0, DeepE1 = 70.0;
        /// <summary>Scroll periods (m): 64 cells of the largest octave, an integer number of every smaller octave's period.</summary>
        public const double SheetScrollPeriod = 76800.0, DeepScrollPeriod = 43200.0;
        /// <summary>Deck form f = x / sqrt(1 + x^2), x = 4.5 (n - 0.5); base x (1 + 0.5 f), tint x (1 + 0.7 f) (fifth start: 0.5 left
        /// +-2-3 codes of billows in the KS05 / KS04 lids; 0.8 / 0.95 darkened KS02's slit below its window); cloud-mix wisps 70 m x f.</summary>
        public const double DeckK = 4.5, DeckBaseAmp = 0.5, DeckTintAmp = 0.7, CloudWisp = 70.0;
        /// <summary>Wind of the sheet billows (m/s along (0.8, 0.6)) and of the deep glow (m/s along (-0.6, 0.8)) (C# only: the
        /// driver uploads the wrapped offsets as _NocAbyss).</summary>
        public const double SheetWind = 1.2, DeepWind = 0.5;

        // ---------------------------------------------------------------- fix wave (panel p4_01, 2026-11-21; NocAtmosphere.hlsl)
        /// <summary>C11 under-city glow pools on the plane y -2 100: value noise 337.5 / 112.5 m (weights 0.7 / 0.3, rotations 1 / 3,
        /// seeds 905 / 906), pool = smoothstep(0.45, 0.85, n), amber (1.20, 0.98, 0.70) or neutral by a 337.5 m lattice (rotation 4,
        /// seed 907); c x (1 + A f0 (pool tint - 0.24)), weight smoothstep(12, 22, -e) x A (A = NocAir.PoolAmp).</summary>
        public const double PoolY = -2100.0, PoolL0 = 337.5, PoolL1 = 112.5, PoolW0 = 0.7, PoolW1 = 0.3;
        public const double PoolLo = 0.45, PoolHi = 0.85, PoolMean = 0.24, PoolE0 = 12.0, PoolE1 = 22.0;
        /// <summary>C11: the pools' weight falls by 0.8 over -e 45..80 (the nadir keeps the deep glow; AT-t01 KS13 twin mean 69.4).</summary>
        public const double PoolNadir = 0.8, PoolE2 = 45.0, PoolE3 = 80.0;
        public const double PoolAmbR = 1.20, PoolAmbG = 0.98, PoolAmbB = 0.70;
        /// <summary>C11 sink veil ramp: NocFog's T x (1 - V S01((-40 - y) / 360)) (V = NocAir.SinkV).</summary>
        public const double SinkY0 = -40.0, SinkH = 360.0;
        /// <summary>C02-air: Gaussian glow radius (m) of each of the koi's 12 spine points in the air.</summary>
        public const double KoiAirR = 70.0;
        /// <summary>C02-air: no glow on fragments within 60 m of a spine point (smoothstep 60..110 m: the body keeps HT's colour);
        /// the air colour lerp(luma(KoiGlow), KoiGlow, 0.55) (AT_r01f: patch hue / sat and the KS18 dark cast failed with KoiGlow).</summary>
        public const double KoiAirIn = 60.0, KoiAirOut = 110.0, KoiAirSat = 0.55;
        /// <summary>C02-air: a point's glow x smoothstep(30, 70, h), h its distance from the ray (AT_r02f / r03f: the additive body
        /// showed the glowing air behind it on its orange patches).</summary>
        public const double KoiAirH0 = 30.0, KoiAirH1 = 70.0;
        /// <summary>C26: the hull's ellipse keeps 1 - 0.9 of the flash (it outlines the hull).</summary>
        public const double FlashHull = 0.9;
        /// <summary>C44: the alley's light on the deck over the KS02 slit: x (1 + lift exp(-r^2 / (2 70^2))) around (0, 80).</summary>
        public const double SlitX = 0.0, SlitZ = 80.0, SlitSigma = 70.0;
    }

    /// <summary>
    /// Fix-wave (AT, panel p4_01) values that reach the shader only through the AtmosphereDriver (_NocAir, _NocFlash), so they tune
    /// without a shader recompile (no "#define NA_" twin; QA overrides -abyss-pool / -abyss-sink / -koi-air / -slit-lift / -flash).
    /// </summary>
    public static class NocAir
    {
        /// <summary>C11: pool amplitude A (lobes x up to about 2, gaps x 0.76); sink veil V (half the contrast below y -400).</summary>
        public const double PoolAmp = 1.0, SinkV = 0.5;
        /// <summary>C02-air: radiance added by one spine point on a ray through its centre (x KoiGlow x KoiK). AT_r01f / r02f at 0.03:
        /// KS17 sky 50-80 px above the body +14..+18 codes over the far sky at the head and mid-body, +28 over the back, KS18 inside
        /// the arc +23 (critic's window 12-20): 0.025; AT_r04f (hollow 30..70 m) KS18 patch sat 0.59 (HT's floor 0.60): 0.022.</summary>
        public const double KoiAirGain = 0.022;
        /// <summary>C44: the alley lift (AT_r01f at 0.25: KS02 slit 51.7 -> 55.9, KS04 / KS05 lids -0.3 / -0.1; AT_r04f: the 21:9
        /// rect (the same 6 % of a wider frame) takes in the slit's walls, 52.5 with the sky inside it 54-57): 0.30.</summary>
        public const double SlitLift = 0.30;
        /// <summary>C26 sheet lightning (DREAD 5.4): peak +15 % of the deck, sigma 450 m, rise 0.4 s, decay 1.5 s, 550 m ahead of the
        /// walker, or on the launched hull within 900 m.</summary>
        public const double FlashAmp = 0.15, FlashSigma = 450.0, FlashRise = 0.4, FlashDecay = 1.5, FlashAhead = 550.0, FlashHullReach = 900.0;
    }

    /// <summary>
    /// D-043 (AT): the Ark in the haze. Every value reaches the shader through the AtmosphereDriver (_NocArk, _NocArkB, _NocArkG,
    /// _NocArkL, _NocArkBeamP / D), so it tunes without a shader recompile (no "#define NA_" twin). The Ark's pose comes from DR's
    /// course (HullCourse: axis x, centre z of tau, half-length, half-beam, belly y; the same tau rule as DreadDriver); Update fills
    /// the state from it (a pure function: tests and the driver share it).
    /// </summary>
    public static class NocArkAir
    {
        /// <summary>Deck occluder (the Ark shadows the city glow on the cloud base; its back is in the deck): strength (ramped in over
        /// OccRampTau s after the launch, x DR's Emerge), penumbra and soft-end lengths (m), the base deck's darkening share.</summary>
        public const double OccW = 0.85, OccPen = 160.0, OccEnd = 700.0, OccBase = 0.6, OccRampTau = 20.0;
        /// <summary>The glow of its lights in the haze: a tube of radius GlowR about the axis GlowUp m over the belly (inside the
        /// hull: the walker, 330 m under the belly, sees only its tail), I = GlowGain (radiance across it), ramped in over GlowRampTau.</summary>
        public const double GlowUp = 60.0, GlowR = 180.0, GlowGain = 0.025, GlowRampTau = 15.0;
        /// <summary>Searchlights (analytic cones in the haze): DR's six emitters (HullCourse.Searchlight: origin on the keel, a slow
        /// outward sweep, period 28-40 s, its own fade near the walk line) drawn here; each lamp lights BeamOnTau s after the launch
        /// over BeamOnRamp s, from the bow aft (s 470-545 at 2.5 m/s: the beams wake once the dark mass has read), at most Beams.</summary>
        public const int Beams = 6;
        public static readonly double[] BeamOnTau = { 36.0, 42.0, 48.0, 54.0, 60.0, 66.0 };
        public const double BeamOnRamp = 9.0;
        /// <summary>Cone: apex radius r0 (m), fall-off length along the beam (m), radiance across the beam at the apex (I = gain (r0 / r)
        /// exp(-u / L)), pool gain on the haze sheets, the lit spot where a beam meets a surface (NocFog: + gain x SpotGain x min(1,
        /// (SpotR / r)^2) exp(-u / L) exp(-d^2 / r^2) of SignWhite, before the fog: the faces it sweeps).</summary>
        public const double BeamR0 = 6.0, BeamFall = 1600.0, BeamGain = 0.40, PoolGain = 0.0, SpotGain = 0.0, SpotR = 40.0;
        // AT_ark perf A/B (21:05, 1080p beat 4): the pools (in the fog colour of every fogged fragment below the horizon) and the spots
        // (every fragment) showed in no capture (the beams fade before they reach a sheet or a face): both off (gain 0 skips their
        // uniform branches); the beams and the glow carry the scene.
        /// <summary>Never at the walker: a beam fades out while its axis lies within 3 r + ClearWalker m of the eye (over ClearRamp m).</summary>
        public const double ClearWalker = 150.0, ClearRamp = 200.0;
        /// <summary>Never on the bridge or the pier: a beam fades while its axis at y BridgeY lies within BridgeClear0..1 m of the walk
        /// line z 195 over x BridgeX0..BridgeX1 (+-50 m soft; DR's own lamp fade acts on top).</summary>
        public const double BridgeZ = 195.0, BridgeX0 = 200.0, BridgeX1 = 720.0, BridgeY = 60.0, BridgeClear0 = 140.0, BridgeClear1 = 300.0;
        /// <summary>Sheet lightning on the Ark: the glow (sigma FlashSigma across) is stretched x FlashStretch along it, centred on
        /// its axis at the walker's z, clamped to the hull (FlashBowBack m behind the bow, a quarter length ahead of the stern), when that
        /// centre lies within FlashReach m of the walker (s 553: behind the bow 0.9 km south; s 790: overhead).</summary>
        public const double FlashStretch = 2.2, FlashReach = 1000.0, FlashBowBack = 250.0;

        /// <summary>Fill st's Ark fields from DR's pose: axis x, centre z, half-length, half-beam, belly y, tau since the launch (0 =
        /// parked: all zero), DR's emerge weight (0..1), vis (0 = hidden: all zero). The beams are set by SetBeam (all off here).</summary>
        public static void Update(NocAtmoState st, double axisX, double centreZ, double half, double halfBeam, double bellyY,
                                  double tau, double emerge, double vis)
        {
            bool on = tau > 0.0 && vis > 0.0;
            st.ArkX = axisX; st.ArkZ = centreZ; st.ArkHalf = half;
            st.ArkW = on ? OccW * NocFogMath.S01(tau / OccRampTau) * NocMath.Clamp01(emerge) * vis : 0.0;
            st.ArkHalfBeam = halfBeam; st.ArkPen = OccPen; st.ArkEnd = Math.Min(OccEnd, 0.3 * half); st.ArkBase = OccBase;
            st.ArkGlowY = bellyY + GlowUp; st.ArkGlowR = GlowR;
            st.ArkGlow = on ? GlowGain * NocFogMath.S01(tau / GlowRampTau) * vis : 0.0;
            st.ArkPool = PoolGain; st.ArkFall = BeamFall; st.ArkR0 = BeamR0; st.ArkSpot = SpotGain;
            st.ArkBeamsOn = 0.0;
            for (int i = 0; i < 4 * Beams; i++) { st.ArkBeamP[i] = 0.0; st.ArkBeamD[i] = 0.0; }
        }

        /// <summary>Beam i from DR's emitter (origin, unit direction down, DR's gain 0..1, tan of the half-angle): its gain x BeamGain x
        /// the lamp's ignition (BeamOnTau, BeamOnRamp) x vis x the walker and bridge clearances.</summary>
        public static void SetBeam(NocAtmoState st, int i, double ax, double ay, double az, double dx, double dy, double dz, double drGain,
                                   double tanHalf, double tau, double vis, double eyeX, double eyeY, double eyeZ)
        {
            if (i < 0 || i >= Beams) return;
            double n = 1.0 / Math.Max(Math.Sqrt(dx * dx + dy * dy + dz * dz), 1e-9);
            dx *= n; dy *= n; dz *= n;
            double g = tau > 0.0 && vis > 0.0 && dy < -0.2 ? BeamGain * NocMath.Clamp01(drGain) * NocFogMath.S01((tau - BeamOnTau[i]) / BeamOnRamp) * vis : 0.0;
            if (g > 0.0) g *= ClearOfWalker(ax, ay, az, dx, dy, dz, tanHalf, eyeX, eyeY, eyeZ) * ClearOfBridge(ax, ay, az, dx, dy, dz);
            st.ArkBeamP[4 * i] = ax; st.ArkBeamP[4 * i + 1] = ay; st.ArkBeamP[4 * i + 2] = az; st.ArkBeamP[4 * i + 3] = g;
            st.ArkBeamD[4 * i] = dx; st.ArkBeamD[4 * i + 1] = dy; st.ArkBeamD[4 * i + 2] = dz; st.ArkBeamD[4 * i + 3] = tanHalf;
            if (g > 0.0) st.ArkBeamsOn = 1.0;
        }

        /// <summary>0..1: the beam's distance from the eye (to its axis half-line) against 3 r + ClearWalker (r at that point).</summary>
        public static double ClearOfWalker(double ax, double ay, double az, double dx, double dy, double dz, double tanHalf, double ex, double ey, double ez)
        {
            double wx = ex - ax, wy = ey - ay, wz = ez - az;
            double u = Math.Max(0.0, wx * dx + wy * dy + wz * dz);
            double qx = wx - u * dx, qy = wy - u * dy, qz = wz - u * dz;
            double d = Math.Sqrt(qx * qx + qy * qy + qz * qz), r = BeamR0 + u * tanHalf;
            return NocFogMath.S01((d - 3.0 * r - ClearWalker) / ClearRamp);
        }

        /// <summary>0..1: the beam's axis at y BridgeY against the walk line z 195 over the pier and the bridge (x 200..720).</summary>
        public static double ClearOfBridge(double ax, double ay, double az, double dx, double dy, double dz)
        {
            if (dy > -0.2 || ay <= BridgeY) return 1.0;
            double u = (ay - BridgeY) / -dy, x = ax + dx * u, z = az + dz * u;
            double wx = NocFogMath.S01((x - BridgeX0 + 50.0) / 100.0) * NocFogMath.S01((BridgeX1 + 50.0 - x) / 100.0);
            return 1.0 - wx * (1.0 - NocFogMath.S01((Math.Abs(z - BridgeZ) - BridgeClear0) / (BridgeClear1 - BridgeClear0)));
        }

        /// <summary>Sheet-lightning centre on the Ark (see FlashReach): true when the Ark is the target; x on its axis, z at the walker's
        /// z clamped to the hull (FlashBowBack behind the bow .. a quarter length short of the stern; either direction of travel).</summary>
        public static bool FlashCentre(double walkerX, double walkerZ, double axisX, double bowZ, double sternZ, out double x, out double z)
        {
            double len = Math.Abs(sternZ - bowZ), sg = sternZ >= bowZ ? 1.0 : -1.0;
            double a = Math.Max(FlashBowBack, Math.Min(0.75 * len, (walkerZ - bowZ) * sg));   // from the bow aft, a quarter length short of the stern
            x = axisX; z = bowZ + sg * a;
            double ddx = x - walkerX, ddz = z - walkerZ;
            return ddx * ddx + ddz * ddz <= FlashReach * FlashReach;
        }
    }

    /// <summary>The atmosphere globals of one frame (what the AtmosphereDriver uploads; the mirror evaluates the same numbers).</summary>
    public sealed class NocAtmoState
    {
        public double SigmaR = NocturneConstants.RegionalSigma, AlleyExtra, SheetStrength = NocAbyss.SheetFogStrength, Rho;
        public readonly double[] SheetY = { NocturneConstants.SheetY0, NocturneConstants.SheetY1, NocturneConstants.SheetY2 };
        public double SheetWidth = NocturneConstants.SheetWidth;
        public readonly double[] SheetE = new double[3];
        /// <summary>Sky-band scale of the sheets (_NocSheetE.w): 1, or 0 when the sheets are cut (14.8 item 9).</summary>
        public double SheetAmp = 1.0;
        /// <summary>Abyss scroll (m): sheet billows (wrapped to NocAbyss.SheetScrollPeriod) and deep glow (DeepScrollPeriod) (_NocAbyss).</summary>
        public double SheetScrollX, SheetScrollZ, DeepScrollX, DeepScrollZ;
        /// <summary>D-034 hooks (_NocDeckOcc / _NocHazeOcc: centre x, centre z, half-length along z, strength); zero = identity.</summary>
        public double DeckOccX, DeckOccZ, DeckOccHalf, DeckOccW, HazeOccX, HazeOccZ, HazeOccHalf, HazeOccW;
        /// <summary>Fix wave (_NocAir): pool amplitude, sink density K_s (0 when the sheets are cut), koi air gain x KoiK (0 outside
        /// beat 7; set by the driver), alley lift.</summary>
        public double PoolAmp = NocAir.PoolAmp, SinkV = NocAir.SinkV, KoiAir, SlitLift = NocAir.SlitLift;
        /// <summary>The koi's 12 spine points (x, y, z) for the air glow (_NocKoiAir), used when KoiAir &gt; 0.</summary>
        public readonly double[] KoiPts = new double[3 * NocturneConstants.KoiSpinePoints];
        /// <summary>Sheet lightning (_NocFlash): centre in the deck (m), sigma (m), amplitude (0 = off).</summary>
        public double FlashX, FlashZ, FlashSigma = NocAir.FlashSigma, FlashAmp;
        /// <summary>D-043 the Ark (NocArkAir.Update): _NocArk (axis x, centre z, half-length, occluder strength), _NocArkB (half-beam,
        /// penumbra, soft ends, base share), _NocArkG (glow axis y, radius, gain, flash stretch), _NocArkL (beams on, pool gain,
        /// fall-off, r0), the six beams (apex xyz + gain, unit axis xyz + tan of the half-angle). Zero strength / gains = identity.</summary>
        public double ArkX, ArkZ, ArkHalf, ArkW, ArkHalfBeam, ArkPen = NocArkAir.OccPen, ArkEnd = NocArkAir.OccEnd, ArkBase = NocArkAir.OccBase;
        public double ArkGlowY, ArkGlowR = NocArkAir.GlowR, ArkGlow, ArkFlashStretch = 1.0, ArkBeamsOn, ArkPool = NocArkAir.PoolGain, ArkFall = NocArkAir.BeamFall, ArkR0 = NocArkAir.BeamR0, ArkSpot = NocArkAir.SpotGain, ArkSpotR = NocArkAir.SpotR;
        public readonly double[] ArkBeamP = new double[4 * NocArkAir.Beams];
        public readonly double[] ArkBeamD = new double[4 * NocArkAir.Beams];
        /// <summary>Cloud scroll of the four octaves in their rotated frames (m), each wrapped to its own 64-cell period.</summary>
        public double Cloud1X, Cloud1Z, Cloud2X, Cloud2Z, Cloud3X, Cloud3Z, Cloud4X, Cloud4Z;
        public double CamX, CamY = NocturneConstants.EyeHeight, CamZ;
        /// <summary>Radians per pixel used as the sky-noise blur (2 tan(vFOV/2) / H; 1080p vFOV 66 by default).</summary>
        public double PixelAngle = 2.0 * Math.Tan(33.0 * Math.PI / 180.0) / 1080.0;

        public NocAtmoState() { NocFogMath.UpdateSheetE(this); }
    }

    public static class NocFogMath
    {
        // ---------------------------------------------------------------- 14.2.2 checked form (T-FOG; also used by older tests)
        /// <summary>sigma_eff = sigma_beat (1 + 1.8 smoothstep(0, 1400, -y)), y = world height of the fragment (14.2.2).</summary>
        public static double SigmaEff(double sigmaBeat, double y) => sigmaBeat * Down(y);

        /// <summary>Transmittance T = exp(-(sigma_eff d)^2) of a fragment at distance d (m) and height y (m), sheets off.</summary>
        public static double Transmittance(double sigmaBeat, double d, double y)
        {
            double k = SigmaEff(sigmaBeat, y) * d;
            return Math.Exp(-k * k);
        }

        /// <summary>Distance at which T = 0.5 for a horizontal ray (d50 = sqrt(ln 2) / sigma).</summary>
        public static double D50(double sigmaBeat) => Math.Sqrt(Math.Log(2.0)) / sigmaBeat;

        // ---------------------------------------------------------------- helpers (twins of NocAtmosphere.hlsl)
        public static double S01(double t) { t = NocMath.Clamp01(t); return t * t * (3.0 - 2.0 * t); }
        public static double Smoother01(double t) { t = NocMath.Clamp01(t); return t * t * t * (t * (t * 6.0 - 15.0) + 10.0); }
        public static double Down(double y) => 1.0 + NocturneConstants.FogDownFactor * S01(-y / NocturneConstants.FogDownDepth);

        /// <summary>Abramowitz-Stegun 7.1.26 (the HLSL form; |err| &lt; 1.5e-7).</summary>
        public static double ErfAS(double x)
        {
            double s = x < 0 ? -1.0 : 1.0;
            x = Math.Abs(x);
            double t = 1.0 / (1.0 + 0.3275911 * x);
            double p = ((((1.061405429 * t - 1.453152027) * t + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t;
            return s * (1.0 - p * Math.Exp(-x * x));
        }

        public static double SheetMean(double y0, double y1, double yk, double w)
        {
            double dy = y1 - y0;
            if (Math.Abs(dy) < 1.0) { double m = (0.5 * (y0 + y1) - yk) / w; return Math.Exp(-m * m); }
            return 0.88622692545 * w * (ErfAS((y1 - yk) / w) - ErfAS((y0 - yk) / w)) / dy;
        }

        /// <summary>C11 sink veil of a surface at height y (twin of NocFog's factor): V S01((-40 - y) / 360); T' = T (1 - veil).</summary>
        public static double SinkVeil(NocAtmoState st, double y) => st.SinkV * S01((NocAbyss.SinkY0 - y) / NocAbyss.SinkH);

        /// <summary>NocFog's transmittance with the C11 sink veil: FogT x (1 - SinkVeil).</summary>
        public static double FogTV(NocAtmoState st, double x, double y, double z) => FogT(st, x, y, z) * (1.0 - SinkVeil(st, y));

        public static double CapQ(double y)
        {
            double h = NocturneConstants.AlleyHazeY1 - NocturneConstants.AlleyHazeY0;
            double t = (y - NocturneConstants.AlleyHazeY0) / h;
            if (t <= 0) return 0;
            if (t >= 1) return h * (t - 0.5);
            return h * (t * t * t - 0.5 * t * t * t * t);
        }

        public static double CapMean(double y0, double y1)
        {
            double dy = y1 - y0;
            if (Math.Abs(dy) < 0.01) return 1.0 - S01((y0 - NocturneConstants.AlleyHazeY0) / (NocturneConstants.AlleyHazeY1 - NocturneConstants.AlleyHazeY0));
            return 1.0 - (CapQ(y1) - CapQ(y0)) / dy;
        }

        /// <summary>Physical extinction density along a ray (the integrand the path forms above integrate exactly), 1/m.</summary>
        public static double Density(NocAtmoState st, double y, double sigmaBase)
        {
            double x = NocMath.Clamp01(-y / NocturneConstants.FogDownDepth);
            double rho = sigmaBase * (1.0 + NocturneConstants.FogDownFactor * (9 * x * x - 8 * x * x * x));
            for (int k = 0; k < 3; k++)
            {
                double m = (y - st.SheetY[k]) / st.SheetWidth;
                rho += sigmaBase * st.SheetStrength * Down(st.SheetY[k]) * Math.Exp(-m * m);
            }
            double cap = 1.0 - S01((y - NocturneConstants.AlleyHazeY0) / (NocturneConstants.AlleyHazeY1 - NocturneConstants.AlleyHazeY0));
            return rho + st.AlleyExtra * cap;
        }

        /// <summary>Optical depth tau from the eye (st.Cam*) to (x, y, z); T = exp(-tau^2). Twin of NocOpticalDepthS.</summary>
        public static double OpticalDepth(NocAtmoState st, double x, double y, double z, double sigmaBase)
        {
            double dx = x - st.CamX, dy = y - st.CamY, dz = z - st.CamZ;
            double d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double y0 = st.CamY, y1 = y;
            double tau = sigmaBase * Down(y1) * d;
            if (st.SheetStrength > 0 && Math.Min(y0, y1) < NocturneConstants.SheetSkipY)
            {
                double sh = 0;
                for (int k = 0; k < 3; k++) sh += Down(st.SheetY[k]) * SheetMean(y0, y1, st.SheetY[k], st.SheetWidth);
                tau += sigmaBase * st.SheetStrength * sh * d;
            }
            if (st.AlleyExtra > 0) tau += st.AlleyExtra * CapMean(y0, y1) * d;
            return tau;
        }
        public static double OpticalDepth(NocAtmoState st, double x, double y, double z) => OpticalDepth(st, x, y, z, st.SigmaR);
        public static double FogT(NocAtmoState st, double x, double y, double z) { double t = OpticalDepth(st, x, y, z); return Math.Exp(-t * t); }

        // ---------------------------------------------------------------- cloud deck
        /// <summary>Value noise with a lattice periodic in `period` cells (twin of NocValueNoiseP).</summary>
        public static double ValueNoiseP(double x, double y, int period)
        {
            double fx = Math.Floor(x), fy = Math.Floor(y);
            int ix = (int)fx, iy = (int)fy;
            ix = ((ix % period) + period) % period; iy = ((iy % period) + period) % period;
            int jx = (ix + 1) % period, jy = (iy + 1) % period;
            double tx = x - fx, ty = y - fy;
            double ux = tx * tx * tx * (tx * (tx * 6 - 15) + 10);
            double uy = ty * ty * ty * (ty * (ty * 6 - 15) + 10);
            double a = NocMath.U01(NocMath.Pcg2(ix, iy)), b = NocMath.U01(NocMath.Pcg2(jx, iy));
            double c = NocMath.U01(NocMath.Pcg2(ix, jy)), d = NocMath.U01(NocMath.Pcg2(jx, jy));
            return NocMath.Lerp(NocMath.Lerp(a, b, ux), NocMath.Lerp(c, d, ux), uy);
        }

        /// <summary>Glow union of the deck (twin of NocCloudGlowG): W = 1 - prod(1 - g_i), the g-weighted tint, and the Gulf
        /// shadow union (glows with tint Shadow), g_i = saturate(a_i exp(-0.5 (dist_i / sigma_i)^2)).</summary>
        public static double CloudGlow(double x, double z, out C3 tint, out double gulf)
        {
            double keep = 1.0, keepG = 1.0, gsum = 0.0;
            var tsum = new C3(0, 0, 0);
            for (int i = 0; i < NocCloudGlow.Count; i++)
            {
                double ax = NocCloudGlow.Seg[4 * i], az = NocCloudGlow.Seg[4 * i + 1];
                double abx = NocCloudGlow.Seg[4 * i + 2] - ax, abz = NocCloudGlow.Seg[4 * i + 3] - az;
                double h = NocMath.Clamp01(((x - ax) * abx + (z - az) * abz) / Math.Max(abx * abx + abz * abz, 1e-6));
                double px = x - ax - abx * h, pz = z - az - abz * h;
                double dist = Math.Abs(Math.Sqrt(px * px + pz * pz) - NocCloudGlow.Par[4 * i + 2]);
                double q = dist / Math.Max(NocCloudGlow.Par[4 * i + 1], 1.0);
                double g = NocMath.Clamp01(NocCloudGlow.Par[4 * i] * Math.Exp(-0.5 * q * q));
                double t = NocCloudGlow.Par[4 * i + 3];
                if (t > 2.5) { keepG *= 1.0 - g; continue; }
                keep *= 1.0 - g; gsum += g;
                tsum = tsum + GlowTint(t) * g;
            }
            tint = tsum * (1.0 / Math.Max(gsum, 1e-4));
            gulf = 1.0 - keepG;
            return 1.0 - keep;
        }
        /// <summary>Glow W only (the soft union of the non-shadow glows).</summary>
        public static double CloudGlowG(double x, double z) => CloudGlow(x, z, out _, out _);
        static C3 GlowTint(double t) => t < 0.5 ? Pal(NocPal.CloudHot) : (t < 1.5 ? Pal(NocPal.CloudCool) : Pal(NocPal.CloudBase));

        /// <summary>Fade of one octave of wavelength L (m) at the pixel footprint blurM (m): smoothstep(4, 8, L / blurM) (LOOK_V2 3.1:
        /// no octave below 8 px of wavelength keeps its full amplitude, none below 4 px is drawn).</summary>
        public static double OctaveFade(double L, double blurM) =>
            NocMath.Smoothstep(NocturneConstants.CloudFadePx0, NocturneConstants.CloudFadePx1, L / Math.Max(blurM, 1e-6));

        /// <summary>Deck noise n in 0..1, mean 0.5 (twin of NocCloudNoise): octaves 900 / 300 / 110 / 40 m, weights 0.36 / 0.19 /
        /// 0.30 / 0.15, each rotated (+23 / -41 / +71 / -59 deg) and scrolled 3 m/s; the 110 and 40 m octaves are domain-warped by
        /// 60 m x (2 n900 - 1, 2 n300 - 1) (the 2-D form of LOOK_V2 3.1's 60 m x (2 n900 - 1)); faded octaves sit at 0.5.</summary>
        public static double CloudNoise(NocAtmoState st, double x, double z, double blurM)
        {
            int p = NocturneConstants.CloudNoisePeriod;
            double L1 = NocturneConstants.CloudNoise1, L2 = NocturneConstants.CloudNoise2, L3 = NocturneConstants.CloudNoise3, L4 = NocturneConstants.CloudNoise4;
            Rot1(x, z, out double ax, out double az);
            Rot2(x, z, out double bx, out double bz);
            double n1 = NocMath.Lerp(0.5, ValueNoiseP((ax + st.Cloud1X) / L1, (az + st.Cloud1Z) / L1, p), OctaveFade(L1, blurM));
            double n2 = NocMath.Lerp(0.5, ValueNoiseP((bx + st.Cloud2X) / L2, (bz + st.Cloud2Z) / L2, p), OctaveFade(L2, blurM));
            double wx = x + NocturneConstants.CloudWarp * (2.0 * n1 - 1.0), wz = z + NocturneConstants.CloudWarp * (2.0 * n2 - 1.0);
            Rot3(wx, wz, out double cx, out double cz);
            Rot4(wx, wz, out double dx, out double dz);
            double n3 = NocMath.Lerp(0.5, ValueNoiseP((cx + st.Cloud3X) / L3, (cz + st.Cloud3Z) / L3, p), OctaveFade(L3, blurM));
            double n4 = NocMath.Lerp(0.5, ValueNoiseP((dx + st.Cloud4X) / L4, (dz + st.Cloud4Z) / L4, p), OctaveFade(L4, blurM));
            return NocturneConstants.CloudNoiseW1 * n1 + NocturneConstants.CloudNoiseW2 * n2 + NocturneConstants.CloudNoiseW3 * n3 + NocturneConstants.CloudNoiseW4 * n4;
        }

        // octave rotations (+23 / -41 / +71 / -59 degrees; the shader uses the same float constants)
        static void Rot1(double x, double z, out double rx, out double rz) { rx = 0.92050485 * x - 0.39073113 * z; rz = 0.39073113 * x + 0.92050485 * z; }
        static void Rot2(double x, double z, out double rx, out double rz) { rx = 0.75470958 * x + 0.65605903 * z; rz = -0.65605903 * x + 0.75470958 * z; }
        static void Rot3(double x, double z, out double rx, out double rz) { rx = 0.32556815 * x - 0.94551858 * z; rz = 0.94551858 * x + 0.32556815 * z; }
        static void Rot4(double x, double z, out double rx, out double rz) { rx = 0.51503807 * x + 0.85716730 * z; rz = -0.85716730 * x + 0.51503807 * z; }

        /// <summary>Deck billow form f in (-1, 1) (twin of NocDeckForm): x / sqrt(1 + x^2), x = 4.5 (n - 0.5); never flat (H1).</summary>
        public static double DeckForm(double n) { double x = NocAbyss.DeckK * (n - 0.5); return x / Math.Sqrt(1.0 + x * x); }

        /// <summary>Deck colour (LOOK_V2 3.1 tint model with the AT form, twin of NocCloudColorN): base = CloudBase (1 + 0.5 f)
        /// (1 - 0.35 g_gulf), cloud = lerp(base, tint (1 + 0.5 f), W); f = DeckForm(n).</summary>
        public static C3 CloudColor(NocAtmoState st, double x, double z, double blurM, out double f)
        {
            f = DeckForm(CloudNoise(st, x, z, blurM));
            double W = CloudGlow(x, z, out C3 tint, out double gulf);
            var bas = Pal(NocPal.CloudBase) * ((1.0 + NocAbyss.DeckBaseAmp * f) * (1.0 - NocturneConstants.CloudGulfShade * gulf));
            double noArk = st.ArkW > 0 ? 0.0 : 1.0;
            if (st.DeckOccW > 0)                                                   // D-034 hull (identity at 0; the Ark replaces it)
            {
                double occ = Occluder(x, z, st.DeckOccX, st.DeckOccZ, st.DeckOccHalf, st.DeckOccW) * noArk;
                W *= 1.0 - occ;
                bas = bas * (1.0 - 0.5 * occ);
            }
            double ark = 0.0;
            if (st.ArkW > 0)                                                       // D-043 the Ark shadows the city glow
            {
                ark = ArkShape(st, x, z);
                double occ = NocMath.Clamp01(st.ArkW) * ark;
                W *= 1.0 - occ;
                bas = bas * (1.0 - st.ArkBase * occ);
            }
            var col = C3.Lerp(bas, tint * (1.0 + NocAbyss.DeckTintAmp * f), W);
            if (st.SlitLift > 0) col = col * (1.0 + st.SlitLift * SlitShape(x, z));              // C44 alley lift
            if (st.FlashAmp > 0)                                                                  // C26 sheet lightning
            {
                double qx = (x - st.FlashX) / Math.Max(st.FlashSigma, 1.0), qz = (z - st.FlashZ) / (Math.Max(st.FlashSigma, 1.0) * Math.Max(st.ArkFlashStretch, 1.0));
                double fl = st.FlashAmp * Math.Exp(-0.5 * (qx * qx + qz * qz)) * (0.75 + 0.25 * f);
                if (st.DeckOccW > 0) fl *= 1.0 - NocAbyss.FlashHull * Occluder(x, z, st.DeckOccX, st.DeckOccZ, st.DeckOccHalf, 1.0) * noArk;
                fl *= 1.0 - NocAbyss.FlashHull * ark;
                col = col * (1.0 + fl);
            }
            return col;
        }

        /// <summary>D-043 twin of NocArkShape: a soft stadium along z (unit strength), the half-beam narrowing to 0.35 over the soft
        /// ends, penumbra st.ArkPen.</summary>
        public static double ArkShape(NocAtmoState st, double x, double z)
        {
            double along = S01((st.ArkHalf - Math.Abs(z - st.ArkZ)) / Math.Max(st.ArkEnd, 1.0));
            double hb = st.ArkHalfBeam * (0.35 + 0.65 * Math.Sqrt(along));
            return along * (1.0 - NocMath.Smoothstep(hb - 0.5 * st.ArkPen, hb + st.ArkPen, Math.Abs(x - st.ArkX)));
        }

        /// <summary>D-043 twin of NocTubeIn x the Ark's two terms (NocArkAir): the in-scatter (linear) along the unit ray (dx, dy, dz)
        /// from st.Cam over 0..len of the glow tube (cold window light) and the six searchlight cones (SignWhite).</summary>
        public static C3 ArkAir(NocAtmoState st, double dx, double dy, double dz, double len)
        {
            var acc = new C3(0, 0, 0);
            if (st.ArkGlow > 0)
            {
                LineApproach(st.CamX - st.ArkX, st.CamY - st.ArkGlowY, st.CamZ - st.ArkZ, dx, dy, dz, 0, 0, 1, out double t0, out double u0, out double h2, out double sn);
                double r = Math.Max(st.ArkGlowR, 1.0);
                if (h2 < 6.0 * r * r)
                {
                    double win = S01((st.ArkHalf - Math.Abs(u0)) / Math.Max(st.ArkEnd, 1.0));
                    acc = acc + C3.Lerp(Pal(NocPal.WinNeutral), Pal(NocPal.WinCold), 0.6) * TubeIn(st, st.ArkGlow * win, r, h2, t0, sn, len, dy);
                }
            }
            if (st.ArkBeamsOn > 0)
            {
                double r0 = Math.Max(st.ArkR0, 1.0), sum = 0;
                for (int i = 0; i < NocArkAir.Beams; i++)
                {
                    double g = st.ArkBeamP[4 * i + 3];
                    if (g <= 0) continue;
                    LineApproach(st.CamX - st.ArkBeamP[4 * i], st.CamY - st.ArkBeamP[4 * i + 1], st.CamZ - st.ArkBeamP[4 * i + 2], dx, dy, dz,
                                 st.ArkBeamD[4 * i], st.ArkBeamD[4 * i + 1], st.ArkBeamD[4 * i + 2], out double t0, out double u0, out double h2, out double sn);
                    double r = r0 + Math.Max(u0, 0.0) * st.ArkBeamD[4 * i + 3];
                    if (h2 >= 6.0 * r * r) continue;
                    double I = g * (r0 / r) * Math.Exp(-Math.Max(u0, 0.0) / Math.Max(st.ArkFall, 1.0)) * NocMath.Smoothstep(-r0, 2.0 * r0, u0);
                    sum += TubeIn(st, I, r, h2, t0, sn, len, dy);
                }
                acc = acc + Pal(NocPal.SignWhite) * sum;
            }
            return acc;
        }

        /// <summary>D-043 twin of NocArkSpot: the searchlights' lit spot (linear SignWhite) on a surface at (x, y, z).</summary>
        public static C3 ArkSpot(NocAtmoState st, double x, double y, double z)
        {
            double sum = 0.0, r0 = Math.Max(st.ArkR0, 1.0), R2 = st.ArkSpotR * st.ArkSpotR;
            for (int i = 0; i < NocArkAir.Beams; i++)
            {
                double g = st.ArkBeamP[4 * i + 3];
                if (g <= 0) continue;
                double wx = x - st.ArkBeamP[4 * i], wy = y - st.ArkBeamP[4 * i + 1], wz = z - st.ArkBeamP[4 * i + 2];
                double u = wx * st.ArkBeamD[4 * i] + wy * st.ArkBeamD[4 * i + 1] + wz * st.ArkBeamD[4 * i + 2];
                double r = r0 + Math.Max(u, 0.0) * st.ArkBeamD[4 * i + 3];
                double qx = wx - u * st.ArkBeamD[4 * i], qy = wy - u * st.ArkBeamD[4 * i + 1], qz = wz - u * st.ArkBeamD[4 * i + 2];
                double d2 = qx * qx + qy * qy + qz * qz;
                if (u > 0.0 && d2 < 9.0 * r * r) sum += g * Math.Min(1.0, R2 / (r * r)) * Math.Exp(-u / Math.Max(st.ArkFall, 1.0)) * Math.Exp(-d2 / (r * r));
            }
            return Pal(NocPal.SignWhite) * (st.ArkSpot * sum);
        }

        /// <summary>Twin of NocLineApproach: the ray (unit d) from the eye at w = eye - a against the line (point a, unit b).</summary>
        public static void LineApproach(double wx, double wy, double wz, double dx, double dy, double dz, double bx, double by, double bz,
                                        out double t0, out double u0, out double h2, out double sn)
        {
            double c = dx * bx + dy * by + dz * bz, s2 = Math.Max(1.0 - c * c, 1e-4);
            double dd = dx * wx + dy * wy + dz * wz, e = bx * wx + by * wy + bz * wz;
            t0 = (e * c - dd) / s2;
            u0 = e + t0 * c;
            double qx = wx + t0 * dx - u0 * bx, qy = wy + t0 * dy - u0 * by, qz = wz + t0 * dz - u0 * bz;
            h2 = qx * qx + qy * qy + qz * qz;
            sn = Math.Sqrt(s2);
        }

        /// <summary>Twin of NocTubeIn (fog at the closest approach with the eye's sigma x Down).</summary>
        public static double TubeIn(NocAtmoState st, double I, double r, double h2, double t0, double sn, double len, double dy)
        {
            double k = sn / r, tf = Math.Max(t0, 0.0), ft = st.SigmaR * Down(st.CamY + tf * dy) * tf, q = h2 / (r * r);
            double x0 = t0 * k, x1 = (len - t0) * k, seg = Math.Min(x0, x1) < 2.5 ? ErfAS(x1) + ErfAS(x0) : 2.0;
            return I * Math.Exp(-q - ft * ft) * (1.0 - NocMath.Smoothstep(4.0, 6.0, q)) * seg / (2.0 * Math.Max(sn, 0.1));
        }
        public static C3 CloudColor(NocAtmoState st, double x, double z, double blurM) => CloudColor(st, x, z, blurM, out _);

        /// <summary>C44 shape: exp(-r^2 / (2 SlitSigma^2)) around (SlitX, SlitZ).</summary>
        public static double SlitShape(double x, double z)
        {
            double qx = (x - NocAbyss.SlitX) / NocAbyss.SlitSigma, qz = (z - NocAbyss.SlitZ) / NocAbyss.SlitSigma;
            return Math.Exp(-0.5 * (qx * qx + qz * qz));
        }

        /// <summary>C26 envelope of one glow tau seconds after its start (DREAD 5.4): smoothstep rise over 0.4 s, smoothstep decay over
        /// 1.5 s; 0 outside (0, 1.9).</summary>
        public static double FlashEnvelope(double tau)
        {
            if (tau <= 0 || tau >= NocAir.FlashRise + NocAir.FlashDecay) return 0.0;
            return tau < NocAir.FlashRise ? S01(tau / NocAir.FlashRise) : 1.0 - S01((tau - NocAir.FlashRise) / NocAir.FlashDecay);
        }

        /// <summary>C02-air (twin of NocKoiAir): the koi's in-scatter (linear, x KoiGlow) along the unit ray (dx, dy, dz) from st.Cam over
        /// 0..len: sum over the spine points of exp(-h^2 / (2 R^2)) (erf((len - t0) / (sqrt 2 R)) + erf(t0 / (sqrt 2 R))), x KoiAir / 2.</summary>
        public static C3 KoiAir(NocAtmoState st, double dx, double dy, double dz, double len)
        {
            if (st.KoiAir <= 0) return new C3(0, 0, 0);
            double k = 0.70710678118654752 / NocAbyss.KoiAirR, g = 0;
            for (int i = 0; i < NocturneConstants.KoiSpinePoints; i++)
            {
                double px = st.KoiPts[3 * i] - st.CamX, py = st.KoiPts[3 * i + 1] - st.CamY, pz = st.KoiPts[3 * i + 2] - st.CamZ;
                double t0 = px * dx + py * dy + pz * dz, h2 = Math.Max(px * px + py * py + pz * pz - t0 * t0, 0.0);
                g += KoiAirOwn(px, py, pz, dx * len, dy * len, dz * len) * NocMath.Smoothstep(NocAbyss.KoiAirH0, NocAbyss.KoiAirH1, Math.Sqrt(h2))
                   * Math.Exp(-h2 * k * k) * (ErfAS((len - t0) * k) + ErfAS(t0 * k));
            }
            return KoiAirColour() * (0.5 * st.KoiAir * g);
        }

        /// <summary>C02-air: smoothstep(60, 110, |p - f|), p the spine point and f the fragment (both relative to the eye).</summary>
        public static double KoiAirOwn(double px, double py, double pz, double fx, double fy, double fz)
        {
            double ax = px - fx, ay = py - fy, az = pz - fz;
            return NocMath.Smoothstep(NocAbyss.KoiAirIn, NocAbyss.KoiAirOut, Math.Sqrt(ax * ax + ay * ay + az * az));
        }

        /// <summary>C02-air colour: lerp(luma(KoiGlow), KoiGlow, 0.55).</summary>
        public static C3 KoiAirColour()
        {
            var kg = Pal(NocPal.KoiGlow);
            double l = 0.2126 * kg.r + 0.7152 * kg.g + 0.0722 * kg.b;
            return new C3(l + (kg.r - l) * NocAbyss.KoiAirSat, l + (kg.g - l) * NocAbyss.KoiAirSat, l + (kg.b - l) * NocAbyss.KoiAirSat);
        }

        // ---------------------------------------------------------------- the abyss (H3, AT; twins of NocAtmosphere.hlsl)
        /// <summary>Periodic value noise with an integer seed added after the wrap (twin of NocValueNoisePS).</summary>
        public static double ValueNoisePS(double x, double y, int period, int sx, int sy)
        {
            double fx = Math.Floor(x), fy = Math.Floor(y);
            int ix = (int)fx, iy = (int)fy;
            ix = ((ix % period) + period) % period; iy = ((iy % period) + period) % period;
            int jx = (ix + 1) % period, jy = (iy + 1) % period;
            double tx = x - fx, ty = y - fy;
            double ux = tx * tx * tx * (tx * (tx * 6 - 15) + 10);
            double uy = ty * ty * ty * (ty * (ty * 6 - 15) + 10);
            double a = NocMath.U01(NocMath.Pcg2(ix + sx, iy + sy)), b = NocMath.U01(NocMath.Pcg2(jx + sx, iy + sy));
            double c = NocMath.U01(NocMath.Pcg2(ix + sx, jy + sy)), d = NocMath.U01(NocMath.Pcg2(jx + sx, jy + sy));
            return NocMath.Lerp(NocMath.Lerp(a, b, ux), NocMath.Lerp(c, d, ux), uy);
        }

        static readonly double[] s_rot6c = { 0.95630476, 0.60181502, 0.78801075, 0.32556815, 0.43837115, 0.87461971 };
        static readonly double[] s_rot6s = { 0.29237170, -0.79863551, 0.61566148, -0.94551858, 0.89879405, -0.48480962 };
        /// <summary>Rotation i of the abyss lattices (+17 / -53 / +38 / -71 / +64 / -29 deg; twin of NocRot6).</summary>
        public static void Rot6(double x, double z, int i, out double rx, out double rz)
        {
            double c = s_rot6c[i], s = s_rot6s[i];
            rx = c * x - s * z; rz = s * x + c * z;
        }

        static readonly double[] s_sheetL = { NocAbyss.SheetL0, NocAbyss.SheetL1, NocAbyss.SheetL2, NocAbyss.SheetL3 };
        static readonly double[] s_sheetW = { NocAbyss.SheetW0, NocAbyss.SheetW1, NocAbyss.SheetW2, NocAbyss.SheetW3 };

        /// <summary>Billow noise of sheet k at plane point (x, z) (m), footprint blurM (m) (twin of NocSheetNoise): 4 octaves
        /// 1200 / 400 / 133.3 / 44.4 m, weights 0.30 / 0.30 / 0.25 / 0.15, faded to their mean below 6..12 px; the scroll (sx, sz)
        /// is added in each octave's rotated frame (exact wrap at NocAbyss.SheetScrollPeriod); nFine = the 44 m octave alone (the
        /// shading detail); g = blurM sum(w_o fade_o / L_o), the noise change per pixel (scale).</summary>
        public static double SheetNoise(double x, double z, double sx, double sz, double blurM, int k, out double nFine, out double g)
        {
            double n = 0; nFine = 0.5; g = 0;
            for (int o = 0; o < 4; o++)
            {
                double L = s_sheetL[o], fd = NocMath.Smoothstep(NocAbyss.SheetFadePx0, NocAbyss.SheetFadePx1, L / Math.Max(blurM, 1e-6)), v = 0.5;
                if (fd > 0)
                {
                    Rot6(x, z, o + k, out double rx, out double rz);   // o + k <= 5
                    v = NocMath.Lerp(0.5, ValueNoisePS((rx + sx) / L, (rz + sz) / L, NocturneConstants.CloudNoisePeriod, 37 * (k + 1) + 11 * o, 53 * (k + 1) + 7 * o), fd);
                }
                n += s_sheetW[o] * v;
                g += s_sheetW[o] * fd / L;
                nFine = v;
            }
            g *= blurM;
            return n;
        }

        /// <summary>Band profile of a sheet in elevation (twin of NocSheetBand): exp(-(x / 1.6)^2) above e_k, exp(-(x / 5.5)^2) below.</summary>
        public static double SheetBand(double x) { double q = x > 0 ? x / NocAbyss.BandUp : x / NocAbyss.BandDn; return Math.Exp(-q * q); }

        static readonly double[] s_bandA = { NocAbyss.BandA0, NocAbyss.BandA1, NocAbyss.BandA2 };
        static readonly double[] s_topK = { NocAbyss.TopK0, NocAbyss.TopK1, NocAbyss.TopK2 };
        static readonly C3[] s_veil = { new C3(NocAbyss.Veil0R, NocAbyss.Veil0G, NocAbyss.Veil0B),
            new C3(NocAbyss.Veil1R, NocAbyss.Veil1G, NocAbyss.Veil1B), new C3(NocAbyss.Veil2R, NocAbyss.Veil2G, NocAbyss.Veil2B) };

        /// <summary>D-034 analytic occluder (twin of NocOccluder): w (1 - smoothstep(0.6, 1, r)), r = the radius of the ellipse centred at
        /// (cx, cz) with half-axes (0.15 half, half) (at least 1 m); w clamped to 0..1.</summary>
        public static double Occluder(double x, double z, double cx, double cz, double half, double w)
        {
            double hx = Math.Max(NocAbyss.OccBeam * half, 1.0), hz = Math.Max(half, 1.0);
            double qx = (x - cx) / hx, qz = (z - cz) / hz;
            return NocMath.Clamp01(w) * (1.0 - NocMath.Smoothstep(NocAbyss.OccEdge, 1.0, Math.Sqrt(qx * qx + qz * qz)));
        }

        /// <summary>Lift of the depths under the sheets (twin of NocAbyssLift): smoothstep(3, 30, -e) (1 - 0.8 smoothstep(30, 80, -e)).</summary>
        public static double AbyssLift(double e) =>
            NocMath.Smoothstep(NocAbyss.LiftE0, NocAbyss.LiftE1, -e) * (1.0 - NocAbyss.LiftNadir * NocMath.Smoothstep(NocAbyss.LiftE1, NocAbyss.LiftE2, -e));

        /// <summary>Elevation-only opacity of sheet k for a downward unit ray with y component dy (&lt; 0) at elevation e (deg), before
        /// the billow coverage: (A_k band(e - e_k) + (1 - exp(-K_k / sin|e|)) V_k smoothstep over e_k .. e_k - 12) x the sheet scale.</summary>
        public static double SheetOpacityE(NocAtmoState st, int k, double dy, double e, out double t)
        {
            double sy = Math.Max(-dy, 1e-4);
            t = (st.CamY - st.SheetY[k]) / sy;
            double tau = st.SigmaR * Down(st.SheetY[k]) * t;
            double V = Math.Exp(-tau * tau);
            return (s_bandA[k] * SheetBand(e - st.SheetE[k]) + (1.0 - Math.Exp(-s_topK[k] / sy)) * V * S01((st.SheetE[k] - e) / NocAbyss.TopRamp)) * st.SheetAmp;
        }

        /// <summary>The abyss (twin of NocAbyss): c = the base gradient of the unit ray (dx, dy, dz), dy &lt; 0, at elevation e (deg);
        /// the lift (1 + 1.2 L(e)), the deep glow of the lit city (plane y -2000, +-50 % where e &lt; -10), the D-034 shadow, then sheets
        /// 2, 1, 0 as dark veils: c = lerp(c, c x veil_k x shade, a_k).</summary>
        public static C3 Abyss(NocAtmoState st, C3 c, double dx, double dy, double dz, double e, double blurRad)
        {
            double sy = Math.Max(-dy, 1e-4);
            c = c * (1.0 + NocAbyss.DeepLift * AbyssLift(e));
            double dw = NocMath.Smoothstep(NocAbyss.DeepE0, NocAbyss.DeepE1, -e);
            if (dw > 0)
            {
                double t = (st.CamY - NocAbyss.DeepY) / sy;
                double hx = st.CamX + dx * t, hz = st.CamZ + dz * t;
                double blurM = blurRad * t / Math.Max(sy, 0.05);
                double f0 = OctaveFade(NocAbyss.DeepL0, blurM), f1 = OctaveFade(NocAbyss.DeepL1, blurM);
                Rot6(hx, hz, 2, out double ax, out double az);
                Rot6(hx, hz, 5, out double bx, out double bz);
                int p = NocturneConstants.CloudNoisePeriod;
                ax += st.DeepScrollX; az += st.DeepScrollZ; bx += st.DeepScrollX; bz += st.DeepScrollZ;    // scroll in the rotated frames
                double n = NocAbyss.DeepW0 * NocMath.Lerp(0.5, ValueNoisePS(ax / NocAbyss.DeepL0, az / NocAbyss.DeepL0, p, 900, 300), f0)
                         + NocAbyss.DeepW1 * NocMath.Lerp(0.5, ValueNoisePS(bx / NocAbyss.DeepL1, bz / NocAbyss.DeepL1, p, 901, 301), f1);
                c = c * (1.0 + NocAbyss.DeepAmp * 2.0 * (n - 0.5) * dw);
            }
            double pw = NocMath.Smoothstep(NocAbyss.PoolE0, NocAbyss.PoolE1, -e) * (1.0 - NocAbyss.PoolNadir * NocMath.Smoothstep(NocAbyss.PoolE2, NocAbyss.PoolE3, -e)) * st.PoolAmp;   // C11 pools
            if (pw > 0)
            {
                double t = (st.CamY - NocAbyss.PoolY) / sy;
                double hx = st.CamX + dx * t, hz = st.CamZ + dz * t;
                double blurM = blurRad * t / Math.Max(sy, 0.05);
                double f0 = OctaveFade(NocAbyss.PoolL0, blurM), f1 = OctaveFade(NocAbyss.PoolL1, blurM);
                if (f0 > 0)
                {
                    int p = NocturneConstants.CloudNoisePeriod;
                    Rot6(hx, hz, 1, out double ax, out double az);
                    Rot6(hx, hz, 3, out double bx, out double bz);
                    Rot6(hx, hz, 4, out double cx, out double cz);
                    ax += st.DeepScrollX; az += st.DeepScrollZ; bx += st.DeepScrollX; bz += st.DeepScrollZ; cx += st.DeepScrollX; cz += st.DeepScrollZ;
                    double n = NocAbyss.PoolW0 * ValueNoisePS(ax / NocAbyss.PoolL0, az / NocAbyss.PoolL0, p, 905, 305)
                             + NocAbyss.PoolW1 * NocMath.Lerp(0.5, ValueNoisePS(bx / NocAbyss.PoolL1, bz / NocAbyss.PoolL1, p, 906, 306), f1);
                    double sel = NocMath.Smoothstep(0.35, 0.65, ValueNoisePS(cx / NocAbyss.PoolL0, cz / NocAbyss.PoolL0, p, 907, 307));
                    double pool = NocMath.Smoothstep(NocAbyss.PoolLo, NocAbyss.PoolHi, n), k = pw * f0;
                    c = new C3(c.r * (1.0 + k * (pool * (1.0 + (NocAbyss.PoolAmbR - 1.0) * sel) - NocAbyss.PoolMean)),
                               c.g * (1.0 + k * (pool * (1.0 + (NocAbyss.PoolAmbG - 1.0) * sel) - NocAbyss.PoolMean)),
                               c.b * (1.0 + k * (pool * (1.0 + (NocAbyss.PoolAmbB - 1.0) * sel) - NocAbyss.PoolMean)));
                }
            }
            if (st.HazeOccW > 0)                                                   // D-034 shadow (identity at 0)
            {
                double to = (st.CamY - NocAbyss.HazeOccY) / sy;
                c = c * (1.0 - Occluder(st.CamX + dx * to, st.CamZ + dz * to, st.HazeOccX, st.HazeOccZ, st.HazeOccHalf, st.HazeOccW));
            }
            for (int k = 2; k >= 0; k--)
            {
                double aE = SheetOpacityE(st, k, dy, e, out double t);
                if (aE <= 0.002) continue;
                double hx = st.CamX + dx * t, hz = st.CamZ + dz * t;
                double n = SheetNoise(hx, hz, st.SheetScrollX, st.SheetScrollZ, blurRad * t / Math.Max(sy, 0.05), k, out double nFine, out double g);
                double hw = Math.Max(NocAbyss.CovHalf, NocAbyss.CovAA * g);
                double a = Math.Min(NocMath.Smoothstep(0.5 - hw, 0.5 + hw, n) * aE, NocAbyss.SheetMaxA);
                double shade = 1.0 + NocAbyss.SheetDetail * 2.0 * (nFine - 0.5) - NocAbyss.SheetThick * NocMath.Clamp01((n - 0.5 - hw) * 4.0);
                var v = s_veil[k];
                c = C3.Lerp(c, new C3(c.r * v.r * shade, c.g * v.g * shade, c.b * v.b * shade), a);
            }
            return c;
        }

        // ---------------------------------------------------------------- sky
        /// <summary>
        /// Sky gradient (linear RGB) for elevation e (degrees), without the cloud deck and the sheet modulation: e >= 0:
        /// HazeHorizon (0) -> NightMid (35) -> NightZenith (90), smoothstep in linear space; e &lt; 0: HazeHorizon blends to
        /// A(e) = lerp(AbyssNear, AbyssGlow, 1 - (1 - |e| / 90)^1.3) by smootherstep over 0..-6 degrees (D-006, R2 2.4; AT).
        /// </summary>
        public static void SkyGradient(double elevationDeg, out double r, out double g, out double b)
        {
            var c = SkyGradientC(elevationDeg, null);
            r = c.r; g = c.g; b = c.b;
        }

        /// <summary>The base gradient (twin of NocSkyGradient); the sheets and the deep glow need the ray (Abyss, applied by SkyColor).
        /// st is unused (kept for the callers).</summary>
        public static C3 SkyGradientC(double e, NocAtmoState st)
        {
            if (e >= 0)
            {
                var c = C3.Lerp(Pal(NocPal.HazeHorizon), Pal(NocPal.NightMid), NocMath.Smoothstep(0, 35, e));
                return C3.Lerp(c, Pal(NocPal.NightZenith), NocMath.Smoothstep(35, 90, e));
            }
            double x = 1.0 - NocMath.Clamp01(-e / 90.0);
            var a = C3.Lerp(Pal(NocPal.AbyssNear), Pal(NocPal.AbyssGlow), 1.0 - Math.Pow(x, NocAbyss.AbyssExp));   // AT: faster rise, flat at -90
            double wb = Smoother01(-e / NocturneConstants.HorizonBlendDeg);
            return C3.Lerp(Pal(NocPal.HazeHorizon), a, wb);
        }

        /// <summary>Sky colour of a direction (any length) seen from st.Cam; twin of NocSkyColorBlur (blurRad = pixel angle).</summary>
        public static C3 SkyColor(NocAtmoState st, double dx, double dy, double dz, double blurRad)
        {
            double len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (len < 1e-12) return SkyGradientC(0, st);
            dx /= len; dy /= len; dz /= len;
            double e = Math.Asin(NocMath.Clamp(dy, -1, 1)) * 180.0 / Math.PI;
            var c = SkyGradientC(e, st);
            if (e < 0) c = Abyss(st, c, dx, dy, dz, e, blurRad);                     // H3 (AT): deep glow, three sheets
            if (e > NocturneConstants.CloudFadeE0)
            {
                double t = (NocturneConstants.CloudBaseY - st.CamY) / Math.Max(dy, 1e-3);
                double hx = st.CamX + dx * t, hy = st.CamY + dy * t, hz = st.CamZ + dz * t;
                double blurM = blurRad * t / Math.Max(dy, 0.05);
                var cc = CloudColor(st, hx, hz, blurM);
                double tau = OpticalDepth(st, hx, hy, hz, Math.Min(st.SigmaR, NocturneConstants.CloudSkySigmaMax));
                double T = Math.Exp(-tau * tau);
                c = C3.Lerp(c, cc, T * NocMath.Smoothstep(NocturneConstants.CloudFadeE0, NocturneConstants.CloudFadeE1, e));
            }
            c = HazeLowMix(st, c, dy);
            return new C3(Math.Max(c.r, 0), Math.Max(c.g, 0), Math.Max(c.b, 0));
        }

        /// <summary>LOOK_V2 3.1: the warm low haze of the alley layer inside the sky function, c = lerp(c, HazeLow, 0.6 sa m / (sr +
        /// sa m)), m = the layer's mean along the ray to min(6 500 m, the cloud hit) (y_end = y_cam + min(6 500 dy, 460 - y_cam);
        /// m = 1 for dy &lt;= 0). The path length cancels, so the share is continuous across the cloud fade. dy = the unit ray's y.</summary>
        public static C3 HazeLowMix(NocAtmoState st, C3 c, double dy)
        {
            if (st.AlleyExtra <= 0) return c;
            double yEnd = st.CamY + Math.Min(NocturneConstants.HazeLowPath * dy, NocturneConstants.CloudBaseY - st.CamY);
            double sam = st.AlleyExtra * CapMean(st.CamY, yEnd);
            return C3.Lerp(c, Pal(NocPal.HazeLow), NocturneConstants.HazeLowShare * sam / Math.Max(st.SigmaR + sam, 1e-12));
        }
        public static C3 SkyColor(NocAtmoState st, double dx, double dy, double dz) => SkyColor(st, dx, dy, dz, st.PixelAngle);

        /// <summary>Sky colour at elevation e (deg) and azimuth az (deg, 0 = +Z north, 90 = +X east).</summary>
        public static C3 SkyAt(NocAtmoState st, double eDeg, double azDeg)
        {
            double e = eDeg * Math.PI / 180.0, a = azDeg * Math.PI / 180.0;
            return SkyColor(st, Math.Cos(e) * Math.Sin(a), Math.Sin(e), Math.Cos(e) * Math.Cos(a));
        }

        // ---------------------------------------------------------------- geometry
        /// <summary>Cloud mix of geometry (twin of NocCloudMix): lerp(col, cloud, smoothstep(380, 560, y + 70 f)), f = the deck form at
        /// (x, z): towers dissolve into hanging bellies and gaps (AT, INT-11).</summary>
        public static C3 CloudMix(NocAtmoState st, C3 col, double x, double y, double z)
        {
            if (y <= NocturneConstants.CloudMixY0 - NocAbyss.CloudWisp) return col;
            double dx = x - st.CamX, dy = y - st.CamY, dz = z - st.CamZ;
            double blurM = st.PixelAngle * Math.Sqrt(dx * dx + dy * dy + dz * dz);
            var cc = CloudColor(st, x, z, blurM, out double f);
            return C3.Lerp(col, cc, NocMath.Smoothstep(NocturneConstants.CloudMixY0, NocturneConstants.CloudMixY1, y + NocAbyss.CloudWisp * f));
        }

        /// <summary>NocFog: cloud mix, then col T + NocSkyColor(ray) (1 - T). Twin of the HLSL NocFog.</summary>
        public static C3 Fog(NocAtmoState st, C3 col, double x, double y, double z)
        {
            col = CloudMix(st, col, x, y, z);
            double T = FogTV(st, x, y, z);                                                       // + C11 sink veil
            if (T < 0.99995) col = C3.Lerp(SkyColor(st, x - st.CamX, y - st.CamY, z - st.CamZ), col, T);
            if (st.KoiAir > 0)                                                                    // C02-air
            {
                double dx = x - st.CamX, dy = y - st.CamY, dz = z - st.CamZ, l = Math.Sqrt(dx * dx + dy * dy + dz * dz), il = 1.0 / Math.Max(l, 1e-3);
                col = col + KoiAir(st, dx * il, dy * il, dz * il, l);
            }
            return col;
        }

        /// <summary>NocFloor(col) = FloorLin + col (1 - FloorLin): the last operation of every shader (14.2.2).</summary>
        public static double Floor(double c, double floorLin) => floorLin + c * (1.0 - floorLin);
        public static C3 Floor(C3 c) => new C3(Floor(Math.Max(c.r, 0), NocturneConstants.FloorLinR), Floor(Math.Max(c.g, 0), NocturneConstants.FloorLinG),
                                               Floor(Math.Max(c.b, 0), NocturneConstants.FloorLinB));

        // ---------------------------------------------------------------- per-frame state (the AtmosphereDriver uploads this)
        /// <summary>Sheet elevations e_k = -atan((eye - y_k) / 2 500 m) in degrees (H3: where a ray crosses a sheet 2.5 km out).</summary>
        public static void UpdateSheetE(NocAtmoState st)
        {
            for (int k = 0; k < 3; k++)
                st.SheetE[k] = -Math.Atan((NocturneConstants.EyeHeight - st.SheetY[k]) / NocturneConstants.SheetSkyDistance) * 180.0 / Math.PI;
        }

        static readonly double[] SheetBaseY = { NocturneConstants.SheetY0, NocturneConstants.SheetY1, NocturneConstants.SheetY2 };

        /// <summary>Fills st for progress s and scene time t (s): sigma (D-006 split), sheets drift, cloud scroll, rain rho.</summary>
        public static void StateAt(NocAtmoState st, double s, double t, double rainMul = 1.0, bool sheets = true)
        {
            st.SigmaR = NocSchedule.FogSigmaRegional(s);
            st.AlleyExtra = NocSchedule.AlleyHaze(s);
            st.SheetStrength = sheets ? NocAbyss.SheetFogStrength : 0.0;
            st.SheetAmp = sheets ? 1.0 : 0.0;
            st.SinkV = sheets ? NocAir.SinkV : 0.0;                   // C11 sink veil rides with the sheets (14.8 item 9 cuts both)
            st.PoolAmp = NocAir.PoolAmp;
            st.SlitLift = NocAir.SlitLift;
            st.Rho = NocSchedule.RainRho(s) * rainMul;
            double[] y = SheetBaseY;   // static: StateAt runs every frame (AtmosphereDriver), no allocation
            double ph = 2.0 * Math.PI * (t / NocturneConstants.SheetDriftPeriod - Math.Floor(t / NocturneConstants.SheetDriftPeriod));
            for (int k = 0; k < 3; k++) st.SheetY[k] = y[k] + NocturneConstants.SheetDrift * Math.Sin(ph + k * (2.0 * Math.PI / 3.0));
            st.SheetWidth = NocturneConstants.SheetWidth;
            UpdateSheetE(st);
            // cloud scroll: wind (0.6, 0.8) at 3 m/s, each octave wrapped to its own lattice period (exact, no jump)
            double dist = NocturneConstants.CloudScroll * t;
            int np = NocturneConstants.CloudNoisePeriod;
            double p1 = np * NocturneConstants.CloudNoise1, p2 = np * NocturneConstants.CloudNoise2;
            double p3 = np * NocturneConstants.CloudNoise3, p4 = np * NocturneConstants.CloudNoise4;
            Rot1(0.6 * dist, 0.8 * dist, out double w1x, out double w1z);   // the wind in each octave's rotated frame
            Rot2(0.6 * dist, 0.8 * dist, out double w2x, out double w2z);
            Rot3(0.6 * dist, 0.8 * dist, out double w3x, out double w3z);
            Rot4(0.6 * dist, 0.8 * dist, out double w4x, out double w4z);
            st.Cloud1X = Wrap(w1x, p1); st.Cloud1Z = Wrap(w1z, p1);
            st.Cloud2X = Wrap(w2x, p2); st.Cloud2Z = Wrap(w2z, p2);
            st.Cloud3X = Wrap(w3x, p3); st.Cloud3Z = Wrap(w3z, p3);
            st.Cloud4X = Wrap(w4x, p4); st.Cloud4Z = Wrap(w4z, p4);
            // abyss scroll (AT): sheet billows 1.2 m/s along (0.8, 0.6), deep glow 0.5 m/s along (-0.6, 0.8) in each octave's rotated
            // frame, wrapped to the common period of their octaves (exact: every octave's 64-cell lattice repeats there)
            double ds = NocAbyss.SheetWind * t, dd = NocAbyss.DeepWind * t;
            st.SheetScrollX = Wrap(0.8 * ds, NocAbyss.SheetScrollPeriod); st.SheetScrollZ = Wrap(0.6 * ds, NocAbyss.SheetScrollPeriod);
            st.DeepScrollX = Wrap(-0.6 * dd, NocAbyss.DeepScrollPeriod); st.DeepScrollZ = Wrap(0.8 * dd, NocAbyss.DeepScrollPeriod);
        }

        public static double Wrap(double v, double period) => v - period * Math.Floor(v / period);

        // ---------------------------------------------------------------- palette in double
        static readonly C3[] s_pal = BuildPal();
        static C3[] BuildPal()
        {
            var t = NocturnePalette.Table;
            var r = new C3[t.Length];
            foreach (var e in t)
            {
                string h = e.hex.TrimStart('#');
                r[(int)e.id] = new C3(NocturnePalette.SrgbToLinear(Convert.ToInt32(h.Substring(0, 2), 16) / 255.0),
                                      NocturnePalette.SrgbToLinear(Convert.ToInt32(h.Substring(2, 2), 16) / 255.0),
                                      NocturnePalette.SrgbToLinear(Convert.ToInt32(h.Substring(4, 2), 16) / 255.0));
            }
            return r;
        }
        /// <summary>Linear colour of a palette token in double (the exact value the PAL_* macros round to 6 decimals).</summary>
        public static C3 Pal(NocPal p) => s_pal[(int)p];
    }
}
