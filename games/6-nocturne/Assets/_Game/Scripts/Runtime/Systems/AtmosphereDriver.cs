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
// City and rain (A2b): _NocFacade (x window epoch phase, y warm share, z time, w neutral share: LOOK_V2 4 w_f(s) / n_f(s)),
// _NocTraffic (lane time, H7) and the rain state (NocRainState: rho = the -rain X override or the 14.2.4 schedule, the integrated
// drift; D-005) for NocRainPass.
// QA (AT, Phase 4): -sheet-fog X replaces the geometry strength of the haze sheets (_NocFog.z, default NocAbyss.SheetFogStrength;
// 0 when the sheets are cut), read once in Init, so one build can A/B the layering of the abyss (H3) without a shader change.
// Fix wave (AT, panel p4_01): _NocAir = (pool amplitude (C11), sink veil V (C11, 0 with the sheets cut), koi air gain x KoiK
// (C02-air), alley lift (C44)), _NocKoiAir = the koi's spine points, _NocFlash = the D-034 5.4 sheet lightning (C26): one glow at
// each DreadScore.LightningS (AU's schedule, one source), launched when s first crosses it and then on real seconds (DreadClock,
// the hull's rule; capture mode, stills and sequences, uses the cinematic pace (s - s_i) / 2.5 like DreadDriver), envelope NocFogMath.FlashEnvelope (rise 0.4 s, decay 1.5 s),
// peak +15 % of the deck in a 450 m Gaussian 550 m ahead on the path heading, or on DR's hull (the live _NocDeckOcc) when the
// launched hull is within 900 m (s 553: the flash outlines it). QA overrides read once in Init: -abyss-pool X, -abyss-sink X,
// -koi-air X, -slit-lift X, -flash X (a scale of the flash amplitude; 0 = off), so one build can A/B them.
// The Ark (D-043, AT): UpdateArk takes DR's course (HullCourse, the same tau rule as DreadDriver) into NocArkAir.Update: _NocArk*
// (the deck occluder: the Ark shadows the city glow on the cloud base; the glow of its lights in the haze; six slow searchlight
// cones, kept off the walker and the bridge, with pools on the haze sheets); the s 553 / 790 sheet lightning centres on the live
// Ark (stretched along it: the deck flashes round its silhouette). QA overrides -ark-air X (glow and beams), -ark-occ X (occluder).
using UnityEngine;

namespace Megastructure
{
    public sealed class AtmosphereDriver
    {
        /// <summary>Global light calibration (14.2.3): multiplies every light and ambient term in the shader. Fixed at 1.0 (LOOK_V2 6).</summary>
        public static float LightCal = 1f;
        /// <summary>Haze sheets on (14.8 item 9 cuts them by setting this false).</summary>
        public static bool SheetsOn = true;
        /// <summary>Geometry strength of the haze sheets (_NocFog.z): NocAbyss.SheetFogStrength, or the QA override -sheet-fog X (Init).</summary>
        public static double SheetFog = NocAbyss.SheetFogStrength;
        /// <summary>Analytic koi light strength (14.2.3): set by beat 7 (0 elsewhere) with the 12 spine points below.</summary>
        public static float KoiK;
        public static readonly Vector4[] KoiSpine = new Vector4[NocturneConstants.KoiSpinePoints];
        /// <summary>Fix wave (AT): the _NocAir values (defaults NocAbyss; QA overrides in Init) and the flash amplitude scale.</summary>
        public static double PoolAmp = NocAir.PoolAmp, SinkV = NocAir.SinkV, KoiAirGain = NocAir.KoiAirGain, SlitLift = NocAir.SlitLift, FlashScale = 1.0;

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
        static readonly int s_air = Shader.PropertyToID("_NocAir");
        static readonly int s_flash = Shader.PropertyToID("_NocFlash");
        static readonly int s_koiAir = Shader.PropertyToID("_NocKoiAir");
        static readonly int s_deckOcc = Shader.PropertyToID("_NocDeckOcc");
        static readonly int s_ark = Shader.PropertyToID("_NocArk");
        static readonly int s_arkB = Shader.PropertyToID("_NocArkB");
        static readonly int s_arkG = Shader.PropertyToID("_NocArkG");
        static readonly int s_arkL = Shader.PropertyToID("_NocArkL");
        static readonly int s_arkS = Shader.PropertyToID("_NocArkS");
        static readonly int s_arkBeamP = Shader.PropertyToID("_NocArkBeamP");
        static readonly int s_arkBeamD = Shader.PropertyToID("_NocArkBeamD");
        /// <summary>D-043 QA overrides (read once in Init): -ark-air X scales the glow and the searchlights (0 = off), -ark-occ X the
        /// deck occluder (0 = off), so one build can A/B AT's part of the Ark.</summary>
        public static double ArkAirScale = 1.0, ArkOccScale = 1.0;
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
        readonly DreadLaunchState[] m_flash = new DreadLaunchState[DreadScore.LightningS.Length];
        DreadLaunchState m_ark;
        readonly Vector4[] m_arkP = new Vector4[NocArkAir.Beams];
        readonly Vector4[] m_arkD = new Vector4[NocArkAir.Beams];
        /// <summary>D-043: this frame's Ark tau (s since its launch; DR's rule) and whether it is the sheet-lightning target.</summary>
        public double ArkTau { get; private set; }

        public void Init(Light skyFill)
        {
            SheetFog = System.Math.Max(0.0, CliArgs.GetDouble("-sheet-fog", NocAbyss.SheetFogStrength));
            PoolAmp = System.Math.Max(0.0, CliArgs.GetDouble("-abyss-pool", NocAir.PoolAmp));
            SinkV = System.Math.Max(0.0, System.Math.Min(0.9, CliArgs.GetDouble("-abyss-sink", NocAir.SinkV)));
            KoiAirGain = System.Math.Max(0.0, CliArgs.GetDouble("-koi-air", NocAir.KoiAirGain));
            SlitLift = System.Math.Max(0.0, CliArgs.GetDouble("-slit-lift", NocAir.SlitLift));
            FlashScale = System.Math.Max(0.0, System.Math.Min(1.0, CliArgs.GetDouble("-flash", 1.0)));
            ArkAirScale = System.Math.Max(0.0, CliArgs.GetDouble("-ark-air", 1.0));
            ArkOccScale = System.Math.Max(0.0, System.Math.Min(1.0, CliArgs.GetDouble("-ark-occ", 1.0)));
            for (int i = 0; i < m_flash.Length; i++) m_flash[i] = default;
            m_ark = default;
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
            if (SheetsOn) st.SheetStrength = SheetFog;               // NocAbyss.SheetFogStrength unless -sheet-fog X (QA A/B)
            NocRainState.Update(NocClock.Rain, s);                  // rain (H5, D-005): rho (schedule or the -rain X override), drift
            st.Rho = NocRainState.Rho;
            Shader.SetGlobalVector(ShaderIds.Fog, new Vector4((float)st.SigmaR, (float)st.AlleyExtra, (float)st.SheetStrength, (float)st.Rho));
            Shader.SetGlobalVector(ShaderIds.FogSheets, new Vector4((float)st.SheetY[0], (float)st.SheetY[1], (float)st.SheetY[2], (float)st.SheetWidth));
            Shader.SetGlobalVector(ShaderIds.SheetE, new Vector4((float)st.SheetE[0], (float)st.SheetE[1], (float)st.SheetE[2], (float)st.SheetAmp));
            Shader.SetGlobalVector(ShaderIds.Cloud, new Vector4((float)st.Cloud1X, (float)st.Cloud1Z, (float)st.Cloud2X, (float)st.Cloud2Z));
            Shader.SetGlobalVector(s_cloud2, new Vector4((float)st.Cloud3X, (float)st.Cloud3Z, (float)st.Cloud4X, (float)st.Cloud4Z));
            Shader.SetGlobalVector(s_abyss, new Vector4((float)st.SheetScrollX, (float)st.SheetScrollZ, (float)st.DeepScrollX, (float)st.DeepScrollZ));
            // fix wave (AT): pools, sink, koi air, alley lift (_NocAir), the koi's spine points, the sheet lightning (_NocFlash)
            st.PoolAmp = PoolAmp;
            st.SinkV = SheetsOn && SheetFog > 0.0 ? SinkV : 0.0;
            st.SlitLift = SlitLift;
            st.KoiAir = KoiAirGain * KoiK;
            for (int k = 0; k < KoiSpine.Length; k++) { st.KoiPts[3 * k] = KoiSpine[k].x; st.KoiPts[3 * k + 1] = KoiSpine[k].y; st.KoiPts[3 * k + 2] = KoiSpine[k].z; }
            UpdateArk(s, dt, st);
            UpdateFlash(s, dt, st);
            UploadArk(st);
            Shader.SetGlobalVector(s_air, new Vector4((float)st.PoolAmp, (float)st.SinkV, (float)st.KoiAir, (float)st.SlitLift));
            Shader.SetGlobalVector(s_flash, new Vector4((float)st.FlashX, (float)st.FlashZ, (float)st.FlashSigma, (float)st.FlashAmp));
            Shader.SetGlobalVectorArray(s_koiAir, KoiSpine);
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
            // w_f / n_f of LOOK_V2 4 (the v1 facade path is deleted, TP-2)
            double ep = NocClock.Time / NocturneConstants.FacadeEpoch;
            FacadeShares(s, out double wf, out double nf);
            Shader.SetGlobalVector(ShaderIds.Facade, new Vector4((float)(ep - 65536.0 * System.Math.Floor(ep / 65536.0)),
                (float)wf, (float)NocClock.Wrapped(3600.0), (float)nf));
            Shader.SetGlobalVector(ShaderIds.Traffic, new Vector4((float)NocTraffic.WrapTime(NocClock.Time), (float)NocClock.Wrapped(3600.0), 0f, 0f));

            // time (wrapped in double: no precision loss after hours)
            Shader.SetGlobalVector(ShaderIds.Time, new Vector4((float)NocClock.Wrapped(3600.0),
                (float)(NocClock.Rain - 3600.0 * System.Math.Floor(NocClock.Rain / 3600.0)), (float)s, NocMath.BeatIndex(s)));
        }

        /// <summary>C26 (DREAD 5.4): the sheet-lightning glow of this frame into st.Flash* (amplitude 0 when none is lit). Each glow
        /// launches when s first crosses its DreadScore.LightningS and then runs on real seconds (DreadClock.Step, every frame, so a
        /// pause holds it); stills use the cinematic pace. The glows never overlap (>= 62.5 m apart).</summary>
        void UpdateFlash(double s, double dt, NocAtmoState st)
        {
            st.FlashAmp = 0.0;
            st.FlashSigma = NocAir.FlashSigma;
            double[] ls = DreadScore.LightningS;
            bool still = NocState.Capture;                              // DreadDriver's rule: capture mode is a function of s
            for (int i = 0; i < ls.Length && i < m_flash.Length; i++)
            {
                double tau = still ? DreadClock.CaptureTau(s, ls[i]) : DreadClock.Step(ref m_flash[i], s, System.Math.Max(0.0, dt), ls[i]);
                double env = NocFogMath.FlashEnvelope(tau);
                if (env <= 0.0 || FlashScale <= 0.0) continue;
                bool hull = FlashCentre(ls[i], out double cx, out double cz);
                if (st.ArkW > 0.0)                                              // D-043: on the live Ark (this frame's pose)
                {
                    D3 p = NocMath.PathPoint(s);
                    if (NocArkAir.FlashCentre(p.x, p.z, st.ArkX, HullCourse.BowZ(ArkTau), HullCourse.SternZ(ArkTau), out double ax, out double az))
                    { cx = ax; cz = az; st.ArkFlashStretch = NocArkAir.FlashStretch; }
                }
                else if (hull)
                {
                    Vector4 occ = Shader.GetGlobalVector(s_deckOcc);           // DR's live hull (DreadDriver writes it every frame)
                    if (occ.w > 0f) { cx = occ.x; cz = occ.y; }
                }
                st.FlashX = cx; st.FlashZ = cz;
                st.FlashAmp = NocAir.FlashAmp * FlashScale * env;
            }
        }

        /// <summary>D-043: the Ark's pose from DR's course (HullCourse: the same tau rule as DreadDriver - capture mode the cinematic pace,
        /// play DreadClock.Step on the frame dt, launch at the first crossing of LaunchS) into st's Ark fields (NocArkAir.Update).</summary>
        void UpdateArk(double s, double dt, NocAtmoState st)
        {
            double tau = NocState.Capture ? HullCourse.TauAtS(s) : DreadClock.Step(ref m_ark, s, System.Math.Max(0.0, dt), HullCourse.LaunchS);
            ArkTau = tau;
            double vis = HullCourse.HiddenAt(tau) || DreadBuilder.Driver == null ? 0.0 : 1.0;   // no Ark without DR's objects (NOC_DREAD_OFF, tier 2)
            D3 p = NocMath.PathPoint(s);
            NocArkAir.Update(st, HullCourse.AxisX, HullCourse.CentreZ(tau), 0.5 * HullCourse.Length, HullCourse.HalfBeam, HullCourse.BellyY(tau),
                             tau, HullCourse.Emerge(tau), vis);
            st.ArkW *= ArkOccScale;
            st.ArkGlow *= ArkAirScale;
            double tanHalf = System.Math.Tan(HullCourse.SearchHalfAngle * System.Math.PI / 180.0);
            int n = System.Math.Min(HullCourse.SearchCount, NocArkAir.Beams);
            for (int i = 0; i < n && vis > 0.0 && ArkAirScale > 0.0; i++)
            {
                HullCourse.Searchlight(i, tau, NocClock.Time, out Vector3 o, out Vector3 d, out double g);      // DR's emitters
                NocArkAir.SetBeam(st, i, o.x, o.y, o.z, d.x, d.y, d.z, g * ArkAirScale, tanHalf, tau, vis, p.x, NocturneConstants.EyeHeight, p.z);
            }
            st.ArkFlashStretch = 1.0;
        }

        void UploadArk(NocAtmoState st)
        {
            Shader.SetGlobalVector(s_ark, new Vector4((float)st.ArkX, (float)st.ArkZ, (float)st.ArkHalf, (float)st.ArkW));
            Shader.SetGlobalVector(s_arkB, new Vector4((float)st.ArkHalfBeam, (float)st.ArkPen, (float)st.ArkEnd, (float)st.ArkBase));
            Shader.SetGlobalVector(s_arkG, new Vector4((float)st.ArkGlowY, (float)st.ArkGlowR, (float)st.ArkGlow, (float)st.ArkFlashStretch));
            Shader.SetGlobalVector(s_arkL, new Vector4((float)st.ArkBeamsOn, (float)st.ArkPool, (float)st.ArkFall, (float)st.ArkR0));
            Shader.SetGlobalVector(s_arkS, new Vector4((float)st.ArkSpot, (float)st.ArkSpotR, 0f, 0f));
            for (int i = 0; i < NocArkAir.Beams; i++)
            {
                m_arkP[i] = new Vector4((float)st.ArkBeamP[4 * i], (float)st.ArkBeamP[4 * i + 1], (float)st.ArkBeamP[4 * i + 2], (float)st.ArkBeamP[4 * i + 3]);
                m_arkD[i] = new Vector4((float)st.ArkBeamD[4 * i], (float)st.ArkBeamD[4 * i + 1], (float)st.ArkBeamD[4 * i + 2], (float)st.ArkBeamD[4 * i + 3]);
            }
            Shader.SetGlobalVectorArray(s_arkBeamP, m_arkP);
            Shader.SetGlobalVectorArray(s_arkBeamD, m_arkD);
        }

        /// <summary>Centre (x, z) of the sheet-lightning glow at s_i: 550 m ahead of the walker on the path heading, or on the hull's
        /// centre (DR's HullCourse at the cinematic pace) when the hull has launched and lies within 900 m (returns true: the glow
        /// outlines it; the driver then takes the live hull position from _NocDeckOcc).</summary>
        public static bool FlashCentre(double si, out double x, out double z)
        {
            D3 p = NocMath.PathPoint(si);
            double h = NocMath.Heading(si);
            x = p.x + NocAir.FlashAhead * System.Math.Sin(h);
            z = p.z + NocAir.FlashAhead * System.Math.Cos(h);
            if (si <= HullCourse.LaunchS) return false;
            if (HullCourse.Hidden(HullCourse.SternZAtS(si))) return false;
            // D-043: on the Ark (DR's course API, D-040: read, not copy), at the walker's z clamped to the hull, within 1 km
            if (!NocArkAir.FlashCentre(p.x, p.z, HullCourse.AxisX, HullCourse.BowZAtS(si), HullCourse.SternZAtS(si), out double ax, out double az)) return false;
            x = ax; z = az;
            return true;
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

        /// <summary>Window shares uploaded as _NocFacade.y / .w: w_f(s) / n_f(s) of LOOK_V2 4 (linear in s). The v1 branch (14.2.5
        /// WarmFraction by Rule P) went with the v1 facade path (TP-2, FX's request).</summary>
        public static void FacadeShares(double s, out double warm, out double neutral)
        {
            warm = NocSchedule.WarmShare(s); neutral = NocSchedule.NeutralShare(s);
        }

        /// <summary>Blend weight of the garden ambient set (beat 5 with Rule P at both boundaries).</summary>
        public static double GardenWeight(double s)
        {
            double w = NocturneConstants.BlendWindow * 0.5;
            return NocMath.Smootherstep(880.0 - w, 880.0 + w, s) * (1.0 - NocMath.Smootherstep(1080.0 - w, 1080.0 + w, s));
        }
    }
}
