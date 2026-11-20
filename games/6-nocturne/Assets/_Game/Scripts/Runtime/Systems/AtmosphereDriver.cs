// AtmosphereDriver: the single per-frame writer of the atmosphere globals, the grade parameters, the sky-fill light and the
// wet-floor source set (IMPORT_PLAN 8.4: The Ring's AtmosphereDriver pattern, folded into Thread's ProgressDirector, which
// calls Apply once per frame in a deterministic order; no own execution order). Every parametric value is a function of s
// (NocSchedule, Rule P and the 14.6 fog windows); the clock only drives motion (sheet drift +-15 m / 120 s, cloud scroll
// 3 m/s), so -freeze-clock freezes both. The slew limiters of 14.6 (EV 0.30 /s, bloom 0.1 /s) sit on top and never bind at
// 2.5 m/s (peak EV slope 0.031 EV/s); capture stills bypass them (NocState.Capture without SequenceMode), sequences keep them.
// Fog (14.2.2, H3, D-006): NocFogMath.StateAt splits sigma into the regional sigma and the height-capped alley haze, places the
// drifting sheets and their sky elevations, wraps the cloud scroll per octave and the abyss scroll (_NocAbyss: sheet billows and
// the deep glow, AT); the same numbers feed the C# mirror and tests.
// Lights (LOOK_V2 6): one directional sky fill (the deck above as a soft key: CloudHot / CloudBase / CloudCool normalised to max
// 1 per beat, pitch -60 yaw 35, no shadows), the ambient trilight (sky NightMid x 2.1, a cool-neutral equator, ground AbyssGlow
// x 0.25 / x 0.35 over the abyss; the garden set in beat 5) x the per-beat multiplier, the analytic koi term (0 outside beat 7).
// One global LightCal (in _NocFloor.w, fixed at 1.0) is applied in the shader to every light, ambient and koi term; the ambient
// and fill are display targets, uploaded as value / LightCal. AmbientAt / SkyFillAt / SkyFillColourAt return exactly what Apply
// uploads (SU's NocSurfaceMath.WallRadiance reads them: one copy of the rig).
// City and rain (A2b): _NocFacade (x window epoch phase, y warm share, z time, w neutral share: LOOK_V2 4 w_f(s) / n_f(s) on the
// Look v2 facade path, the 14.2.5 WarmFraction by Rule P and w 0 on v1), _NocTraffic (lane time, H7) and the rain state
// (NocRainState: rho = the -rain X override or the 14.2.4 schedule, the integrated drift; D-005) for NocRainPass.
using UnityEngine;

namespace Megastructure
{
    public sealed class AtmosphereDriver
    {
        /// <summary>Global light calibration (14.2.3): multiplies every light and ambient term in the shader. Fixed at 1.0 (LOOK_V2 6).</summary>
        public static float LightCal = 1f;
        /// <summary>Haze sheets on (14.8 item 9 cuts them by setting this false).</summary>
        public static bool SheetsOn = true;
        /// <summary>Analytic koi light strength (14.2.3): set by beat 7 (0 elsewhere) with the 12 spine points below.</summary>
        public static float KoiK;
        public static readonly Vector4[] KoiSpine = new Vector4[NocturneConstants.KoiSpinePoints];

        /// <summary>LOOK_V2 6 sky-fill intensity per beat (the alley 0.010: no shadow maps, a strong fill would light it as roofless).</summary>
        public static readonly double[] SkyFill = { 0.010, 0.035, 0.035, 0.030, 0.015, 0.025, 0.030 };
        /// <summary>LOOK_V2 6 ambient multiplier per beat (the alley is enclosed; beat 5 uses the garden set x 1.0).</summary>
        public static readonly double[] AmbientMul = { 0.75, 1.0, 1.0, 0.9, 1.0, 0.9, 0.85 };
        /// <summary>LOOK_V2 6 ground ambient = AbyssGlow x this (x 0.35 over the abyss in beats 3-4: warm underlight on soffits).</summary>
        public static readonly double[] GroundMul = { 0.25, 0.25, 0.35, 0.35, 0.25, 0.25, 0.25 };
        /// <summary>Sky-fill colour per beat: 0 = CloudHot (beats 1-2), 1 = CloudBase (3-5), 2 = CloudCool (6-7), each normalised to max 1.</summary>
        public static readonly int[] FillTone = { 0, 0, 1, 1, 1, 2, 2 };
        /// <summary>LOOK_V2 6: ambient sky = NightMid x 2.1, equator (0.0205, 0.0225, 0.0262) linear (#27292D), garden sky NightMid x 1.6,
        /// garden equator CloudBase x 0.6 + Lantern x 0.010, garden ground Moss x 0.12.</summary>
        public const double AmbSkyGain = 2.1, GardenSkyGain = 1.6, GardenEqCloud = 0.6, GardenEqLantern = 0.010, GardenGroundMoss = 0.12;
        public static readonly Color AmbEquator = new Color(0.0205f, 0.0225f, 0.0262f, 1f);
        /// <summary>Sky-fill direction (LOOK_V2 6): pitch -60 (down), yaw 35 deg: north wall NdotL 0.41, face B 0.29, up 0.87.</summary>
        public const float FillPitch = 60f, FillYaw = 35f;
        static readonly int s_cloud2 = Shader.PropertyToID("_NocCloud2");
        static readonly int s_amber = Shader.PropertyToID("_NocAmber");
        static readonly int s_abyss = Shader.PropertyToID("_NocAbyss");
        /// <summary>This frame's R6 amber factor k_a(s) and the screen wall's amber-stop weight (uploaded as _NocAmber.xy; beat code
        /// that mirrors a shader colour on the CPU reads these, e.g. NocScreenMath.AmberWeight).</summary>
        public static double FrameAmberK = 1.0, FrameScreenAmber = 1.0;

        /// <summary>The atmosphere numbers of the current frame (also read by QA and tests).</summary>
        public readonly NocAtmoState State = new NocAtmoState();

        Light m_fill;
        bool m_init;
        double m_ev, m_bloom;
        readonly Vector4[] m_glowSeg = new Vector4[NocCloudGlow.Max];
        readonly Vector4[] m_glowPar = new Vector4[NocCloudGlow.Max];

        public void Init(Light skyFill)
        {
            m_fill = skyFill;
            if (m_fill != null)
            {
                m_fill.shadows = LightShadows.None;
                m_fill.transform.rotation = Quaternion.Euler(FillPitch, FillYaw, 0f);
                m_fill.color = SkyFillColourAt(0).gamma;     // Light.color takes gamma values (URP linearises them)
            }
            m_init = false;
        }

        /// <summary>Write everything for progress s. dt = frame time (0 = first frame or a still: no slew).</summary>
        public void Apply(double s, double dt)
        {
            bool bypass = !m_init || dt <= 0 || (NocState.Capture && !NocState.SequenceMode);
            double ev = NocSchedule.ExposureEV(s), bloom = NocSchedule.BloomIntensity(s);
            if (bypass) { m_ev = ev; m_bloom = bloom; m_init = true; }
            else
            {
                m_ev = NocMath.Slew(m_ev, ev, NocturneConstants.EvSlew, dt);
                m_bloom = NocMath.Slew(m_bloom, bloom, NocturneConstants.BloomSlew, dt);
            }

            // grade
            GradeState.ExposureEV = (float)m_ev;
            GradeState.BloomThreshold = (float)NocSchedule.BloomThreshold(s);
            GradeState.BloomIntensity = (float)m_bloom;
            GradeState.BloomScatter = (float)NocSchedule.BloomScatter(s);
            GradeState.Vignette = (float)NocSchedule.Vignette(s);
            GradeState.Saturation = (float)NocSchedule.Saturation(s);
            GradeState.Contrast = (float)NocSchedule.Contrast(s);

            // fog and sky (14.2.2, H3, D-006): regional sigma, alley haze, sheets, cloud scroll, rain rho (x the -rain multiplier)
            var st = State;
            NocFogMath.StateAt(st, s, NocClock.Time, 1.0, SheetsOn);
            NocRainState.Update(NocClock.Rain, s);                  // rain (H5, D-005): rho (schedule or the -rain X override), drift
            st.Rho = NocRainState.Rho;
            Shader.SetGlobalVector(ShaderIds.Fog, new Vector4((float)st.SigmaR, (float)st.AlleyExtra, (float)st.SheetStrength, (float)st.Rho));
            Shader.SetGlobalVector(ShaderIds.FogSheets, new Vector4((float)st.SheetY[0], (float)st.SheetY[1], (float)st.SheetY[2], (float)st.SheetWidth));
            Shader.SetGlobalVector(ShaderIds.SheetE, new Vector4((float)st.SheetE[0], (float)st.SheetE[1], (float)st.SheetE[2], (float)st.SheetAmp));
            Shader.SetGlobalVector(ShaderIds.Cloud, new Vector4((float)st.Cloud1X, (float)st.Cloud1Z, (float)st.Cloud2X, (float)st.Cloud2Z));
            Shader.SetGlobalVector(s_cloud2, new Vector4((float)st.Cloud3X, (float)st.Cloud3Z, (float)st.Cloud4X, (float)st.Cloud4Z));
            Shader.SetGlobalVector(s_abyss, new Vector4((float)st.SheetScrollX, (float)st.SheetScrollZ, (float)st.DeepScrollX, (float)st.DeepScrollZ));
            for (int i = 0; i < NocCloudGlow.Max; i++)
            {
                m_glowSeg[i] = new Vector4((float)NocCloudGlow.Seg[4 * i], (float)NocCloudGlow.Seg[4 * i + 1], (float)NocCloudGlow.Seg[4 * i + 2], (float)NocCloudGlow.Seg[4 * i + 3]);
                m_glowPar[i] = new Vector4((float)NocCloudGlow.Par[4 * i], (float)NocCloudGlow.Par[4 * i + 1], (float)NocCloudGlow.Par[4 * i + 2], (float)NocCloudGlow.Par[4 * i + 3]);
            }
            Shader.SetGlobalVectorArray(ShaderIds.GlowSeg, m_glowSeg);
            Shader.SetGlobalVectorArray(ShaderIds.GlowPar, m_glowPar);
            Shader.SetGlobalFloat(ShaderIds.GlowCount, NocCloudGlow.Count);
            FrameAmberK = NocSchedule.AmberK(s);
            FrameScreenAmber = NocSchedule.ScreenAmber(s);
            Shader.SetGlobalVector(s_amber, new Vector4((float)FrameAmberK, (float)FrameScreenAmber, 0f, 0f));
            Shader.SetGlobalVector(ShaderIds.Floor, new Vector4(NocturneConstants.FloorLinR, NocturneConstants.FloorLinG, NocturneConstants.FloorLinB, LightCal));

            // ambient trilight (LOOK_V2 6), uploaded as value / LightCal (display targets)
            AmbientAt(s, out Color sky, out Color eq, out Color gr);
            float inv = 1f / Mathf.Max(LightCal, 1e-3f);
            Shader.SetGlobalColor(ShaderIds.AmbSky, sky * inv);
            Shader.SetGlobalColor(ShaderIds.AmbEquator, eq * inv);
            Shader.SetGlobalColor(ShaderIds.AmbGround, gr * inv);
            Shader.SetGlobalFloat(ShaderIds.KoiK, KoiK);
            Shader.SetGlobalVectorArray(ShaderIds.KoiSpine, KoiSpine);

            if (m_fill != null)
            {
                m_fill.intensity = (float)SkyFillAt(s) * inv;
                m_fill.color = SkyFillColourAt(s).gamma;
                m_fill.shadows = LightShadows.None;
            }

            // wet-floor sources and wall planes of the window around s (H4; nothing is sorted per frame)
            NocWetRegistry.Upload(s);

            // city lights (H2, H7): window epoch phase t / 150 and traffic time, both wrapped mod 65 536 in double; the window shares
            // w_f / n_f of LOOK_V2 4 on the Look v2 facade path (-facade-v 2), the 14.2.5 warm fraction by Rule P on v1
            double ep = NocClock.Time / NocturneConstants.FacadeEpoch;
            FacadeShares(s, NocLookGlobals.FacadeV, out double wf, out double nf);
            Shader.SetGlobalVector(ShaderIds.Facade, new Vector4((float)(ep - 65536.0 * System.Math.Floor(ep / 65536.0)),
                (float)wf, (float)NocClock.Wrapped(3600.0), (float)nf));
            Shader.SetGlobalVector(ShaderIds.Traffic, new Vector4((float)NocTraffic.WrapTime(NocClock.Time), (float)NocClock.Wrapped(3600.0), 0f, 0f));

            // time (wrapped in double: no precision loss after hours)
            Shader.SetGlobalVector(ShaderIds.Time, new Vector4((float)NocClock.Wrapped(3600.0),
                (float)(NocClock.Rain - 3600.0 * System.Math.Floor(NocClock.Rain / 3600.0)), (float)s, NocMath.BeatIndex(s)));
        }

        /// <summary>The LOOK_V2 6 ambient trilight at s (linear, before the / LightCal of the upload): sky NightMid x 2.1, equator
        /// (0.0205, 0.0225, 0.0262), ground AbyssGlow x GroundMul (0.25, 0.35 in beats 3-4), blended toward the garden set (sky
        /// NightMid x 1.6, equator CloudBase x 0.6 + Lantern x 0.010, ground Moss x 0.12) by GardenWeight, all x AmbientMul (Rule P).</summary>
        public static void AmbientAt(double s, out Color sky, out Color equator, out Color ground)
        {
            Color nightMid = NocturnePalette.Linear(NocPal.NightMid);
            float g = (float)GardenWeight(s);
            float mul = (float)NocMath.BeatBlend(AmbientMul, s);
            Color gEq = NocturnePalette.Linear(NocPal.CloudBase) * (float)GardenEqCloud + NocturnePalette.Linear(NocPal.Lantern) * (float)GardenEqLantern;
            sky = Color.Lerp(nightMid * (float)AmbSkyGain, nightMid * (float)GardenSkyGain, g) * mul;
            equator = Color.Lerp(AmbEquator, gEq, g) * mul;
            ground = Color.Lerp(NocturnePalette.Linear(NocPal.AbyssGlow) * (float)NocMath.BeatBlend(GroundMul, s),
                                NocturnePalette.Linear(NocPal.Moss) * (float)GardenGroundMoss, g) * mul;
            sky.a = equator.a = ground.a = 1f;
        }

        /// <summary>The LOOK_V2 6 sky-fill intensity at s (Rule P), before the / LightCal of the upload.</summary>
        public static double SkyFillAt(double s) => NocMath.BeatBlend(SkyFill, s);

        /// <summary>The LOOK_V2 6 sky-fill colour at s (linear, max channel 1): CloudHot (beats 1-2), CloudBase (3-5), CloudCool (6-7),
        /// each normalised to max 1, blended per channel across the beat boundaries (Rule P).</summary>
        public static Color SkyFillColourAt(double s)
        {
            double r = NocMath.BeatBlend(FillChannel(0), s), g = NocMath.BeatBlend(FillChannel(1), s), b = NocMath.BeatBlend(FillChannel(2), s);
            return new Color((float)r, (float)g, (float)b, 1f);
        }
        static double[][] s_fillCh;
        static double[] FillChannel(int ch)
        {
            if (s_fillCh == null)
            {
                var tones = new[] { NocPal.CloudHot, NocPal.CloudBase, NocPal.CloudCool };
                var t = new double[3][];
                for (int c = 0; c < 3; c++) t[c] = new double[FillTone.Length];
                for (int b = 0; b < FillTone.Length; b++)
                {
                    var p = NocFogMath.Pal(tones[FillTone[b]]);
                    double m = System.Math.Max(p.r, System.Math.Max(p.g, p.b));
                    t[0][b] = p.r / m; t[1][b] = p.g / m; t[2][b] = p.b / m;
                }
                s_fillCh = t;
            }
            return s_fillCh[ch];
        }

        /// <summary>Window shares uploaded as _NocFacade.y / .w: Look v2 facades (facadeV 2) w_f(s) / n_f(s) (LOOK_V2 4, linear in s);
        /// v1 the 14.2.5 WarmFraction by Rule P and 0 (the v1 shader reads only y).</summary>
        public static void FacadeShares(double s, int facadeV, out double warm, out double neutral)
        {
            if (facadeV == 2) { warm = NocSchedule.WarmShare(s); neutral = NocSchedule.NeutralShare(s); }
            else { warm = NocMath.BeatBlend(NocturneConstants.WarmFraction, s); neutral = 0.0; }
        }

        /// <summary>Blend weight of the garden ambient set (beat 5 with Rule P at both boundaries).</summary>
        public static double GardenWeight(double s)
        {
            double w = NocturneConstants.BlendWindow * 0.5;
            return NocMath.Smootherstep(880.0 - w, 880.0 + w, s) * (1.0 - NocMath.Smootherstep(1080.0 - w, 1080.0 + w, s));
        }
    }
}
