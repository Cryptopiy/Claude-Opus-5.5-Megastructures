// ProgressDirector: continuous progress s and the per-frame world update (MASTER_PROMPT 14.6). Skeleton of Thread's
// ProgressDirector: Thread's ApplyGlobals body (helix, sinking ground, sun, planet, sea, beacon uniforms) is NOT imported;
// the one writer of globals, lights, camera optics and GradeState is AtmosphereDriver (14.2.2, D-09 / C-13).
// Each frame (LateUpdate 500, after the player and camera moved): BwClock.Tick, then Apply() =
//   s (player or capture override) -> GameState.S -> AtmosphereDriver.Tick(s, dt) -> FrameState -> every IWorldUpdater.
// The capture harness sets OverrideActive/OverrideS and the camera pose, then calls Apply() before every render, so
// updaters re-submit instanced draws and the globals match the shot. beginCameraRendering writes _BwScreen (thin-line
// minimum width per render target, 14.7 H7) and binds the blue-noise tile globally (_BwBlueNoise, tier fades D-08).
// Zero allocations per frame.
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Megastructure.Rendering;

namespace Megastructure
{
    [DefaultExecutionOrder(500)]
    public class ProgressDirector : MonoBehaviour
    {
        public static ProgressDirector Instance { get; private set; }
        /// <summary>Progress used for all parameters this frame.</summary>
        public static double S { get; private set; }
        public static FrameState Frame;

        /// <summary>Capture / test override of s (parameters only; the camera pose is set separately).</summary>
        public bool OverrideActive;
        public double OverrideS;

        WorldContext m_ctx;
        double m_lastTime = double.NaN;

        /// <summary>Thin-line rule (14.7 H7): w_min = max(1.5, 1.75 H / 1080) px.</summary>
        public static float MinLineWidthPx(float renderHeight) => Mathf.Max(1.5f, 1.75f * renderHeight / 1080f);

        void Awake()
        {
            Instance = this;
            S = CliArgs.StartS;
            GameState.S = S;
        }

        void OnEnable() { RenderPipelineManager.beginCameraRendering += OnBeginCamera; }
        void OnDisable() { RenderPipelineManager.beginCameraRendering -= OnBeginCamera; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Init(WorldContext ctx) { m_ctx = ctx; }

        void LateUpdate()
        {
            BwClock.Tick(Time.timeAsDouble);
            Apply();
        }

        /// <summary>Recompute everything for the current camera pose (the capture harness calls this before rendering).</summary>
        public void Apply()
        {
            double s = OverrideActive ? OverrideS : (BwServices.Player != null ? BwServices.Player.S : S);
            S = s;
            GameState.S = s;
            double now = Time.timeAsDouble;
            float dt = double.IsNaN(m_lastTime) ? 0f : (float)Math.Max(0.0, Math.Min(0.25, now - m_lastTime));
            m_lastTime = now;
            var atmo = AtmosphereDriver.Instance;
            if (atmo != null)
            {
                try { atmo.Tick(s, dt); }
                catch (Exception e) { Debug.LogError($"[BW-FAIL] AtmosphereDriver.Tick: {e}"); }
            }

            Camera cam = m_ctx != null && m_ctx.Camera != null ? m_ctx.Camera : Camera.main;
            Frame.s = s;
            Frame.ambientTime = BwClock.Ambient;
            Frame.beat = BwPath.BeatAt(s);
            Frame.capture = GameState.CaptureMode;
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
                    try { ups[i].OnFrame(in Frame); }
                    catch (Exception e) { Debug.LogError($"[BW-FAIL] updater {ups[i].GetType().Name}: {e.Message}"); }
                }
            }
        }

        static void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
        {
            float W = Mathf.Max(1, cam.pixelWidth), H = Mathf.Max(1, cam.pixelHeight);
            Shader.SetGlobalVector(ShaderIds.BwScreen, new Vector4(W, H, MinLineWidthPx(H), 1f / H));
            var noise = BlueNoise.Get();
            if (noise != null) Shader.SetGlobalTexture(ShaderIds.BwBlueNoise, noise);
        }
    }
}
