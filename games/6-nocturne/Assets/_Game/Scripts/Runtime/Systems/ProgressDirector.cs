// ProgressDirector: continuous progress s and every parameter derived from it (MASTER_PROMPT 14.6, 14.2.4), imported from
// Thread. Once per frame (after the camera moved): ticks NocClock, writes NocState.S / T / Dt / FrameIndex, calls the
// AtmosphereDriver (the single writer of globals, grade and sky fill), fills FrameState and calls every IWorldUpdater.
// Thread's sun, ground sink, deck, bank and thread uniforms are not imported. -progress is a fraction (CliArgs.StartS).
// The beginCameraRendering hook writes _NocScreen (render height, minimum line width) for every camera, capture cameras
// included. Zero allocations per frame.
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure
{
    [DefaultExecutionOrder(500)]
    public class ProgressDirector : MonoBehaviour
    {
        public static ProgressDirector Instance { get; private set; }
        /// <summary>Progress used for all parameters this frame.</summary>
        public static double S { get; private set; }
        public static FrameState Frame;

        /// <summary>Capture/test override of s (parameters only; the camera pose is set separately).</summary>
        public bool OverrideActive;
        public double OverrideS;
        /// <summary>The directional sky-fill light (scene object, set by the SceneBuilder).</summary>
        public Light skyFill;

        WorldContext m_ctx;
        readonly AtmosphereDriver m_atmo = new AtmosphereDriver();
        public AtmosphereDriver Atmosphere => m_atmo;

        void Awake()
        {
            Instance = this;
            S = CliArgs.StartS;
            m_atmo.Init(skyFill);
            m_atmo.Apply(S, 0.0);   // valid uniforms before the first frame
        }

        void OnEnable() { RenderPipelineManager.beginCameraRendering += OnBeginCamera; }
        void OnDisable() { RenderPipelineManager.beginCameraRendering -= OnBeginCamera; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Init(WorldContext ctx) { m_ctx = ctx; }

        void LateUpdate()
        {
            NocClock.Tick(Time.timeAsDouble);
            float dt = NocState.Paused ? 0f : Time.deltaTime;
            NocState.Dt = dt;
            if (NocRig.WorldReady && !NocState.Paused) NocState.T += dt;
            NocState.FrameIndex++;
            Apply(dt);
        }

        /// <summary>Recompute everything for the current pose (the capture harness calls this before rendering; dt 0 = no slew).</summary>
        public void Apply(double dt = 0.0)
        {
            double s = OverrideActive ? OverrideS : (!double.IsNaN(NocRig.ForcedS) ? NocRig.ForcedS : (NocServices.Player != null ? NocServices.Player.S : S));
            S = s;
            NocState.S = s;
            NocState.Beat = NocMath.BeatIndex(s);
            m_atmo.Apply(s, dt);
            NocDarkWave.Apply(s);   // D-034 5.1 (FX): a pure function of s

            Camera cam = m_ctx != null && m_ctx.Camera != null ? m_ctx.Camera : Camera.main;
            Frame.s = s;
            Frame.time = NocClock.Time;
            Frame.rainTime = NocClock.Rain;
            Frame.beat = NocState.Beat;
            if (cam != null)
            {
                Frame.cameraPos = cam.transform.position;
                Frame.cameraForward = cam.transform.forward;
                Frame.renderHeight = cam.pixelHeight;
                Frame.fovV = cam.fieldOfView;
            }
            if (m_ctx != null)
            {
                var ups = m_ctx.Updaters;
                for (int i = 0; i < ups.Count; i++)
                {
                    if (i == NocAlloc.BisectSkipUpdater) continue;   // -alloc-bisect diagnostics only (-1 otherwise)
                    try { ups[i].OnFrame(in Frame); }
                    catch (Exception e) { Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] updater {ups[i].GetType().Name}: {e.Message}")); }
                }
            }
        }

        static void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
        {
            float H = Mathf.Max(1, cam.pixelHeight);
            float minW = Mathf.Max((float)NocturneConstants.LineMinWidthFloorPx, (float)NocturneConstants.LineMinWidthPx * H / 1080f);
            Shader.SetGlobalVector(ShaderIds.Screen, new Vector4(H, minW, 1f / H, 0f));
        }
    }
}
