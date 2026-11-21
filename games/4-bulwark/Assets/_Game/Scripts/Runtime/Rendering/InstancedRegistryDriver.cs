using UnityEngine;

namespace Megastructure.Rendering
{
    /// <summary>
    /// Issues every registered <see cref="InstancedBatch"/> once per frame (LateUpdate, order +200, after CameraRig and before
    /// CaptureHarness 32000). Graphics.RenderMeshInstanced submits for the current frame only, for all cameras
    /// (RenderParams.camera = null). In capture mode it does not submit: CaptureHarness calls InstancedRegistry.SubmitAllFor
    /// right before each render request (C-12). Lives on the Game GameObject.
    /// </summary>
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent]
    public sealed class InstancedRegistryDriver : MonoBehaviour
    {
        public static InstancedRegistryDriver Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        void LateUpdate()
        {
            if (GameState.CaptureMode) return;
            InstancedRegistry.DrawAll();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            InstancedRegistry.Clear();
        }
    }
}
