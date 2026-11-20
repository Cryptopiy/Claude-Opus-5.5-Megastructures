// NocCameraBatches: per-camera submission of the code-drawn instanced fields (NocNeon signs, NocStreak traffic, beacons and
// fireflies; Abyss InstancedBatch, <= 511 instances per call, explicit bounds; D-008, R3 2.3). Graphics.RenderMeshInstanced is
// issued from RenderPipelineManager.beginCameraRendering with RenderParams.camera = that camera, i.e. before URP culls it, so
// every Game camera gets the fields exactly once per render, the capture harness' SubmitRenderRequest cameras included
// (lesson 3.2.8: code-drawn fields must be re-submitted for each capture camera). Batches registered here are NOT put into
// InstancedRegistry (whose driver draws for all cameras in LateUpdate) - one route per batch, never both.
// Lifetime: a batch remembers the GameBootstrap that was loading when it was registered; batches of an earlier load (scene
// restart) are dropped at the next camera. Zero allocations per frame.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure
{
    public static class NocCameraBatches
    {
        static readonly List<InstancedBatch> s_batches = new List<InstancedBatch>(64);
        static readonly List<Object> s_owner = new List<Object>(64);
        static bool s_installed;

        /// <summary>Registered batches (QA statistics, warm-up).</summary>
        public static IReadOnlyList<InstancedBatch> Batches => s_batches;
        public static int Count => s_batches.Count;
        /// <summary>Instances submitted for the last Game camera (QA, perf log).</summary>
        public static int LastInstances { get; private set; }

        /// <summary>Seals and registers a batch for per-camera submission (build time).</summary>
        public static void Register(InstancedBatch b)
        {
            if (b == null || s_batches.Contains(b)) return;
            if (!b.IsSealed) b.Seal();
            Install();
            s_batches.Add(b);
            s_owner.Add(GameBootstrap.Instance);
        }

        public static void Clear() { s_batches.Clear(); s_owner.Clear(); LastInstances = 0; }

        static void Install()
        {
            if (s_installed) return;
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            s_installed = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            if (s_installed) RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            s_installed = false;
            s_batches.Clear();
            s_owner.Clear();
        }

        static void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
        {
            if (cam == null || (cam.cameraType != CameraType.Game && cam.cameraType != CameraType.SceneView)) return;
            Object live = GameBootstrap.Instance;
            int n = 0;
            for (int i = s_batches.Count - 1; i >= 0; i--)
            {
                if (s_owner[i] != live) { s_batches.RemoveAt(i); s_owner.RemoveAt(i); continue; }   // a batch of an earlier load
                InstancedBatch b = s_batches[i];
                if (!b.Visible || b.Mesh == null) continue;
                b.DrawFor(cam);
                n += b.Count;
            }
            LastInstances = n;
        }
    }
}
