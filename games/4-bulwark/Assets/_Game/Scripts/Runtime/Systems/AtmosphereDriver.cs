// AtmosphereDriver: the ONE writer of the shared atmosphere (MASTER_PROMPT 14.2.2-14.2.5; D-09 / C-13). Imported from
// The Ring's AtmosphereDriver. Every frame ProgressDirector.Apply() calls Tick(s, dt) (also before every capture), which
// evaluates the pure BulwarkSchedule at s, applies the slew limiters (EV 0.30 EV/s, WB 6 units/s, bloom intensity 0.1/s,
// vFOV 1 deg/s; bypassed for capture stills, teleports over 20 m and the first frame; kept for capture sequences) and writes:
//   * every global of BulwarkAtmosphere.hlsl from one AtmoGlobals (the C# twin BwAtmosphereMath reads the same struct):
//     sun, disc, sky stops and their Fritsch-Carlson slopes, glow, aureole, azimuth-mean fog colour, the low-sun
//     in-scatter, the ambient trilight (s-keyed, plus the exterior trilight, the interior ambient and the interior boxes
//     of the region-based BwAmbientAt), fog / storm slab / earth shadow, the beat 1 dust box, the sun-break hole, the
//     far-field cloud / city / sea values, the beat 5 lamp boost and the surface state _BwSurface (rain wetness, interior
//     factor, lamp gain: the materials contract); per camera also _BwFlow (far-field motion offsets from the unwrapped double
//     clock) and _BwOpening (the exterior radiance clamp of 14.7 H8 for eyes inside the dam)
//   * the key light (RenderSettings.sun, per-light shadow bias with usePipelineSettings = false)
//   * the camera (vFOV, near 0.05, far plane by s)
//   * GradeState (EV, WB, tint, saturation, contrast, bloom, vignette, dither peak) - no URP Volume overrides (Part 8.2)
// Per camera (RenderPipelineManager.beginCameraRendering, capture cameras too): _BwCam and _BwFarCam (the eye altitude and
// sin^2 of the dips to the lowland, sea and crest spheres, computed in double: 14.7 H4).
// Constant shadows (4 cascades, 4096, 150 m) live in the pipeline asset (PipelineConfigurator): no shadow-distance slew.
// Allocation-free per frame.
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Megastructure.Rendering;

namespace Megastructure
{
    [DefaultExecutionOrder(500)]
    [DisallowMultipleComponent]
    public sealed class AtmosphereDriver : MonoBehaviour
    {
        public static AtmosphereDriver Instance { get; private set; }

        public Light key;
        public Camera cam;

        /// <summary>Optional vFOV override for capture poses (NaN = schedule). Owner: CameraRig.SetPoseOverride.</summary>
        public static float FovOverride = float.NaN;
        /// <summary>Capture sequences keep the slews (H6: time and progress agree); stills bypass them. Owner: CaptureHarness.</summary>
        public static bool SequenceMode;
        /// <summary>Global beam fade (BwBeam), owned by the beat that shows beams; the driver only publishes it.</summary>
        public static float BeamFade = 1f;
        /// <summary>The last schedule targets, the globals written and the applied slewed values (QA, continuity tests).</summary>
        public static BwParams Last;
        public static AtmoGlobals LastGlobals;
        public static float AppliedEv, AppliedWb, AppliedBloom, AppliedFov;

        // globals only this driver writes (the ShaderIds of the framework hold the first set)
        static readonly int IdSunDisc = Shader.PropertyToID("_BwSunDisc");
        static readonly int IdSkyUpper = Shader.PropertyToID("_BwSkyUpper");
        static readonly int IdSkyMid = Shader.PropertyToID("_BwSkyMid");
        static readonly int IdSkyGlow = Shader.PropertyToID("_BwSkyGlow");
        static readonly int IdSkyHalo = Shader.PropertyToID("_BwSkyHalo");
        static readonly int IdSkyShape = Shader.PropertyToID("_BwSkyShape");
        static readonly int IdFogShadow = Shader.PropertyToID("_BwFogShadow");
        static readonly int IdDustMin = Shader.PropertyToID("_BwDustMin");
        static readonly int IdDustMax = Shader.PropertyToID("_BwDustMax");
        static readonly int IdDustColor = Shader.PropertyToID("_BwDustColor");
        static readonly int IdFarCam = Shader.PropertyToID("_BwFarCam");
        static readonly int IdFar0 = Shader.PropertyToID("_BwFar0");
        static readonly int IdCloud0 = Shader.PropertyToID("_BwCloud0");
        static readonly int IdCloud1 = Shader.PropertyToID("_BwCloud1");
        static readonly int IdSea0 = Shader.PropertyToID("_BwSea0");
        static readonly int IdSeaState = Shader.PropertyToID("_BwSeaState");
        static readonly int IdLampBoost = Shader.PropertyToID("_BwLampBoost");
        static readonly int IdSkyTanH = Shader.PropertyToID("_BwSkyTanH");
        static readonly int IdSkyTanM = Shader.PropertyToID("_BwSkyTanM");
        static readonly int IdSkyTanU = Shader.PropertyToID("_BwSkyTanU");
        static readonly int IdSkyTanZ = Shader.PropertyToID("_BwSkyTanZ");
        static readonly int IdFogLow = Shader.PropertyToID("_BwFogLow");
        static readonly int IdAmbExtSky = Shader.PropertyToID("_BwAmbExtSky");
        static readonly int IdAmbExtMid = Shader.PropertyToID("_BwAmbExtMid");
        static readonly int IdAmbExtGround = Shader.PropertyToID("_BwAmbExtGround");
        static readonly int IdAmbInt = Shader.PropertyToID("_BwAmbInt");
        static readonly int IdIntBox = Shader.PropertyToID("_BwIntBox");
        static readonly int IdFlow = Shader.PropertyToID("_BwFlow");
        /// <summary>_BwSurface (declared in BwCommon.hlsl; materials contract): x rain wetness, y interior factor, z lamp gain.</summary>
        static readonly int IdSurface = Shader.PropertyToID("_BwSurface");
        /// <summary>_BwOpening (BulwarkAtmosphere.hlsl), per camera: x exterior radiance clamp (14.7 H8), y eye interior weight.</summary>
        static readonly int IdOpening = Shader.PropertyToID("_BwOpening");
        static readonly Vector4[] s_intBoxes = BuildInteriorBoxes();

        /// <summary>The interior boxes of BulwarkSchedule.InteriorBoxes as the 8 float4 of _BwIntBox (min, max per box).</summary>
        static Vector4[] BuildInteriorBoxes()
        {
            var b = BulwarkSchedule.InteriorBoxes;
            var v = new Vector4[8];
            for (int k = 0; k < 4; k++)
            {
                v[2 * k] = new Vector4((float)b[6 * k], (float)b[6 * k + 1], (float)b[6 * k + 2], 0f);
                v[2 * k + 1] = new Vector4((float)b[6 * k + 3], (float)b[6 * k + 4], (float)b[6 * k + 5], 0f);
            }
            return v;
        }

        float m_ev, m_wb, m_bloom, m_fov;
        bool m_init;
        double m_prevS;
        float m_lastFar = -1f, m_lastNear = -1f;
        bool m_logged, m_keyShadows = true, m_keyShadowInit;
        DitherMode m_ditherMode;
        double m_ditherOverride;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { FovOverride = float.NaN; SequenceMode = false; BeamFade = 1f; Instance = null; }

        void Awake()
        {
            Instance = this;
            m_ditherMode = CliArgs.DitherMode;
            m_ditherOverride = CliArgs.DitherLsbOverride;
            m_scheduleS = CliArgs.GetDouble("-atmo-s", double.NaN);
            if (!double.IsNaN(m_scheduleS)) Debug.LogWarning("[BW] AtmosphereDriver: QA override -atmo-s " + m_scheduleS.ToString("F1") + " (the schedule ignores the walker's s)");
        }

        /// <summary>QA only: -atmo-s S evaluates the schedule at S whatever the camera's s (a key-shot framing under another
        /// s of the same beat, e.g. KS20 at s 1470 for the 14.5.7 light-blob count while pose tokens cannot reach the ledge).</summary>
        double m_scheduleS = double.NaN;
        /// <summary>QA / capture: t_g of the after-goal sun sink for a still (NaN = the live ending time). Beat08 sets it per shot.</summary>
        public static double EndSinkOverride = double.NaN;

        void OnEnable() { RenderPipelineManager.beginCameraRendering += OnBeginCamera; }
        void OnDisable() { RenderPipelineManager.beginCameraRendering -= OnBeginCamera; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        void Start()
        {
            ResolveReferences();
            RenderSettings.fog = false;                 // 8.4: one fog model, in our shaders only
            if (key != null)
            {
                RenderSettings.sun = key;               // the key is always URP's main light
                key.type = LightType.Directional;
                key.shadows = LightShadows.Soft;
                m_keyShadows = true; m_keyShadowInit = true;
                var data = key.GetUniversalAdditionalLightData();
                if (data != null) data.usePipelineSettings = false;   // per-light biases
            }
            LogGraphics();
        }

        void ResolveReferences()
        {
            if (cam == null) cam = BwServices.CameraControl != null ? BwServices.CameraControl.Camera : Camera.main;
            if (key == null)
            {
                var gb = GameBootstrap.Instance;
                if (gb != null) key = gb.sun;
                if (key == null) key = RenderSettings.sun;
                if (key == null) Debug.LogWarning("[BW] AtmosphereDriver: no key light assigned or found");
            }
        }

        void LogGraphics()
        {
            if (m_logged) return;
            m_logged = true;
            string depth = "unknown";
            var urp = UniversalRenderPipeline.asset;
            if (urp != null)
            {
                var list = urp.rendererDataList;
                if (list.Length > 0 && list[0] is UniversalRendererData rd)
                {
                    DepthFormat att = rd.depthAttachmentFormat, tex = rd.depthTextureFormat;
                    depth = "attachment " + att + ", texture " + tex;
                }
            }
            Debug.Log("[BW] Graphics " + SystemInfo.graphicsDeviceType + " (" + SystemInfo.graphicsDeviceName + "), reversed Z " + SystemInfo.usesReversedZBuffer
                      + ", camera depth " + depth + ", pipeline " + (urp != null ? urp.name : "none")
                      + ", dither " + m_ditherMode + (m_ditherOverride >= 0 ? " peak override " + m_ditherOverride.ToString("F2") : " per beat"));
        }

        /// <summary>The per-frame update. ProgressDirector.Apply() calls it with the frame's s (also before each capture).</summary>
        public void Tick(double s, float dt)
        {
            if (cam == null || key == null) ResolveReferences();
            if (!double.IsNaN(m_scheduleS)) s = m_scheduleS;
            // after the goal at the Keeper's Lantern the sun sinks on (ADDENDUM A1 3.10: the crest goes dark at t_g 14)
            double tg = GameState.GoalReached && !GameState.CaptureMode ? Megastructure.UI.UiDirector.EndingTime : -1.0;
            if (!double.IsNaN(EndSinkOverride)) tg = EndSinkOverride;
            var p = BulwarkSchedule.Evaluate(s, tg);
            Last = p;
            bool capture = GameState.CaptureMode;
            bool bypass = !m_init || !GameBootstrap.Ready || (capture && !SequenceMode) || System.Math.Abs(s - m_prevS) > FrameworkConst.TeleportJump;
            if (bypass) { m_ev = p.Ev; m_wb = p.WbTemp; m_bloom = p.BloomIntensity; m_fov = p.Vfov; }
            else
            {
                m_ev = BwMath.Slew(m_ev, p.Ev, (float)FrameworkConst.EvSlew, dt);
                m_wb = BwMath.Slew(m_wb, p.WbTemp, (float)FrameworkConst.WbSlew, dt);
                m_bloom = BwMath.Slew(m_bloom, p.BloomIntensity, (float)FrameworkConst.BloomSlew, dt);
                m_fov = BwMath.Slew(m_fov, p.Vfov, (float)FrameworkConst.FovSlew, dt);
            }
            m_init = true; m_prevS = s;
            AppliedEv = m_ev; AppliedWb = m_wb; AppliedBloom = m_bloom; AppliedFov = m_fov;

            WriteGlobals(in p);
            ApplyLight(in p);
            ApplyGrade(in p);
            ApplyCamera(in p);
        }

        static Vector4 V(Color c, float w) => new Vector4(c.r, c.g, c.b, w);
        static Vector4 V(D3 v, double w) => new Vector4((float)v.x, (float)v.y, (float)v.z, (float)w);

        void WriteGlobals(in BwParams p)
        {
            Vector3 cp = cam != null ? cam.transform.position : GameState.CameraPos;
            var g = AtmoGlobals.From(in p, new D3(cp.x, cp.y, cp.z));
            LastGlobals = g;
            Shader.SetGlobalVector(ShaderIds.BwSunDir, V(g.SunDir, g.SunRadius));
            Shader.SetGlobalVector(ShaderIds.BwSunColor, V(g.SunColor, g.DiscClamp));
            Shader.SetGlobalVector(IdSunDisc, V(g.SunDisc, g.DiscVis));
            Shader.SetGlobalVector(ShaderIds.BwSkyZenith, V(g.Zenith, 1.0));
            Shader.SetGlobalVector(IdSkyUpper, V(g.Upper, 0.0));
            Shader.SetGlobalVector(IdSkyMid, V(g.Mid, 0.0));
            Shader.SetGlobalVector(ShaderIds.BwSkyHorizon, V(g.Horizon, g.MidMix));
            Shader.SetGlobalVector(IdSkyTanH, V(g.TanH, 0.0));
            Shader.SetGlobalVector(IdSkyTanM, V(g.TanM, 0.0));
            Shader.SetGlobalVector(IdSkyTanU, V(g.TanU, 0.0));
            Shader.SetGlobalVector(IdSkyTanZ, V(g.TanZ, 0.0));
            Shader.SetGlobalVector(IdSkyGlow, V(g.Glow, g.GlowExp));
            Shader.SetGlobalVector(IdSkyHalo, V(g.Halo, g.HaloWidth));
            Shader.SetGlobalVector(IdSkyShape, new Vector4((float)g.GlowElev, (float)g.FogMean.x, (float)g.FogMean.y, (float)g.FogMean.z));
            Shader.SetGlobalVector(IdFogLow, new Vector4((float)g.LowDep, (float)g.LowW, (float)g.LowGlow, (float)g.LowGlowDep));
            Shader.SetGlobalVector(ShaderIds.BwAmbSky, V(g.AmbSky, 1.0));
            Shader.SetGlobalVector(ShaderIds.BwAmbMid, V(g.AmbMid, 1.0));
            Shader.SetGlobalVector(ShaderIds.BwAmbGround, V(g.AmbGround, 1.0));
            Shader.SetGlobalVector(IdAmbExtSky, V(g.AmbExtSky, 1.0));
            Shader.SetGlobalVector(IdAmbExtMid, V(g.AmbExtMid, 1.0));
            Shader.SetGlobalVector(IdAmbExtGround, V(g.AmbExtGround, 1.0));
            Shader.SetGlobalVector(IdAmbInt, V(g.AmbInt, g.IntEdge));
            Shader.SetGlobalVectorArray(IdIntBox, s_intBoxes);
            Shader.SetGlobalVector(ShaderIds.BwFog0, new Vector4((float)g.S0, (float)g.H, (float)g.SigmaS, (float)g.Desat));
            Shader.SetGlobalVector(ShaderIds.BwFog1, new Vector4((float)g.SlabLo, (float)g.SlabHi, (float)g.SlabEdge, (float)g.AltOffset));
            Shader.SetGlobalVector(IdFogShadow, V(g.FogShadow, g.DeckW));
            Shader.SetGlobalVector(IdDustMin, V(g.DustMin, g.DustSigma));
            Shader.SetGlobalVector(IdDustMax, V(g.DustMax, 0.0));
            Shader.SetGlobalVector(IdDustColor, V(g.DustColor, 1.0));
            Shader.SetGlobalVector(ShaderIds.BwSunHole, new Vector4((float)g.BreakW, (float)g.HoleX, (float)g.HoleZ, (float)g.HoleR));
            Shader.SetGlobalVector(IdFar0, new Vector4(p.CityN, p.MistRivers, p.CloudFair, p.SunHole));
            Shader.SetGlobalVector(IdCloud0, new Vector4(p.CloudLo, p.CloudHi, p.CloudAlpha, p.CloudContrast));
            Shader.SetGlobalVector(IdCloud1, V(p.CloudColor, p.CloudColorMix));
            Shader.SetGlobalVector(IdSea0, V(p.SeaBody, p.SeaS2));
            Shader.SetGlobalVector(IdSeaState, new Vector4(p.SeaHs, p.SeaTp, p.WStorm + p.WGold, p.WDusk));
            Shader.SetGlobalVector(IdLampBoost, new Vector4(p.LampBoost, p.Interior, 0f, 0f));
            // surface state (materials owner's contract, BwCommon.hlsl): rain wetness by s, the interior factor of the
            // environment reflection (the same s-keyed weight as the ambient: door 100-120, portal 920-940, mouth 1415-1445),
            // the lamp gain (the 14.2.5 portal boost 2^(1.3 (1 - S5((s-920)/40))), 1 from s 960)
            Shader.SetGlobalVector(IdSurface, new Vector4(p.Wetness, p.Interior, p.LampBoost, 0f));
            Shader.SetGlobalFloat(ShaderIds.BwBeamFade, BeamFade);
            if (cam != null) SetBwCam(cam);
        }

        static Quaternion LightRot(Vector3 towardLight)
        {
            Vector3 f = -towardLight;
            if (f.sqrMagnitude < 1e-8f) f = Vector3.down;
            Vector3 up = Mathf.Abs(f.y) > 0.999f ? Vector3.forward : Vector3.up;
            return Quaternion.LookRotation(f, up);
        }

        void ApplyLight(in BwParams p)
        {
            if (key == null) return;
            bool on = p.SunIntensity > 1e-4f;
            if (key.enabled != on) key.enabled = on;
            if (!on) return;
            key.transform.rotation = LightRot(p.SunDir);
            key.color = p.SunColor.gamma;
            key.intensity = p.SunIntensity;
            if (!m_keyShadowInit || m_keyShadows != p.SunShadows)
            {
                m_keyShadows = p.SunShadows; m_keyShadowInit = true;
                key.shadows = p.SunShadows ? LightShadows.Soft : LightShadows.None;
            }
        }

        void ApplyGrade(in BwParams p)
        {
            GradeState.ExposureEV = m_ev;
            GradeState.WhiteBalance = m_wb;
            GradeState.Tint = p.Tint;
            GradeState.Saturation = p.Saturation;
            GradeState.Contrast = p.Contrast;
            GradeState.BloomThreshold = p.BloomThreshold;
            GradeState.BloomIntensity = m_bloom;
            GradeState.BloomScatter = p.BloomScatter;
            GradeState.Vignette = p.Vignette;
            GradeState.DitherPeak = (float)BwGradeMath.DitherPeak(m_ditherMode, m_ditherOverride, p.DitherPeak);
        }

        void ApplyCamera(in BwParams p)
        {
            if (cam == null) return;
            float fov = float.IsNaN(FovOverride) ? m_fov : FovOverride;
            if (cam.fieldOfView != fov) cam.fieldOfView = fov;
            float near = (float)FrameworkConst.CameraNear;
            if (m_lastNear != near) { m_lastNear = near; cam.nearClipPlane = near; }
            if (m_lastFar != p.Far) { m_lastFar = p.Far; cam.farClipPlane = p.Far; }
        }

        /// <summary>_BwFarCam for an eye at world height y: altitude and sin^2 of the dips (double; 14.7 H4).</summary>
        public static Vector4 FarCam(double worldY)
        {
            double h = BulwarkConst.AltitudeOffset + worldY;
            double sL = h > 0.0 ? BwCurvature.Sin2Dip(h) : -1.0;
            return new Vector4((float)h, (float)sL, (float)AtmoGlobals.SphereSin2(h, BulwarkConst.SeaSphereOffset),
                               (float)AtmoGlobals.SphereSin2(h, BulwarkConst.AltitudeOffset));
        }

        static void SetBwCam(Camera c)
        {
            Vector3 pos = c.transform.position;
            float h = Mathf.Max(1f, c.pixelHeight);
            float pix = 2f * Mathf.Tan(0.5f * c.fieldOfView * Mathf.Deg2Rad) / h;
            Shader.SetGlobalVector(ShaderIds.BwCam, new Vector4(pos.x, pos.y, pos.z, pix));
            Shader.SetGlobalVector(IdFarCam, FarCam(pos.y));
            // exterior radiance clamp of 14.7 H8 for this eye (12 inside an interior box, 1e4 outside, 2 m soft edge)
            double wIn = BulwarkSchedule.InteriorWeightSoft(pos.x, pos.y, pos.z, 2.0);
            Shader.SetGlobalVector(IdOpening, new Vector4((float)BulwarkSchedule.OpeningClamp(pos.x, pos.y, pos.z), (float)wIn, 0f, 0f));
            // far-field motion from the unwrapped double clock, right before the camera renders (after any BwClock.Set of a
            // capture): no 3600 s seam (Part 10 #33)
            Shader.SetGlobalVector(IdFlow, BulwarkSchedule.FlowOffsets(BwClock.Ambient));
        }

        static void OnBeginCamera(ScriptableRenderContext ctx, Camera c)
        {
            if (c != null) SetBwCam(c);
        }
    }
}
