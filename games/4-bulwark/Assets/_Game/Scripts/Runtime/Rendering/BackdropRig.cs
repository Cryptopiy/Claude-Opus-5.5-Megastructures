using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Rendering
{
    /// <summary>
    /// Camera-following far geometry with locked rotation (imported from Abyss): the BwFarField dome (exact follow, gridStep 0)
    /// and far gate towers (grid snap). Rotation never follows the camera, so a move shifts nothing but true parallax. No second
    /// camera (BwGrade grades the last camera of a stack only).
    ///
    /// Add it to the backdrop's root GameObject in Build and set the fields; it updates in LateUpdate at order +150, after
    /// CameraRig, from this frame's final camera position. CaptureHarness calls <see cref="FollowAll"/> after SetPoseOverride
    /// and before rendering, so every backdrop follows the capture pose. Per-axis following lets a backdrop keep a fixed height.
    /// </summary>
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public sealed class BackdropRig : MonoBehaviour
    {
        [Tooltip("Snap step of the followed position in metres (0 = follow exactly).")]
        public float gridStep = 100f;

        [Tooltip("Axes that follow the camera; the others keep anchor's value.")]
        public bool followX = true, followY = false, followZ = true;

        [Tooltip("World position used for axes that do not follow, and added to the followed (snapped) camera position for those that do.")]
        public Vector3 anchor;

        [Tooltip("Fixed world rotation (rotation is locked; it never follows the camera).")]
        public Quaternion lockedRotation = Quaternion.identity;

        static readonly List<BackdropRig> s_all = new List<BackdropRig>(8);

        /// <summary>Move every live backdrop to follow the given camera position (capture poses).</summary>
        public static void FollowAll(Vector3 cameraPos)
        {
            for (int i = 0; i < s_all.Count; i++) if (s_all[i] != null) s_all[i].Follow(cameraPos);
        }

        void OnEnable() { if (!s_all.Contains(this)) s_all.Add(this); }
        void OnDisable() { s_all.Remove(this); }

        static Vector3 CameraPosition()
        {
            var cc = BwServices.CameraControl;
            Camera cam = cc != null ? cc.Camera : GameState.MainCamera;
            return cam != null ? cam.transform.position : GameState.CameraPos;
        }

        /// <summary>Configure in one call (Build time).</summary>
        public void Configure(float step, bool x, bool y, bool z, Vector3 anchorPos, Quaternion rotation)
        {
            gridStep = step; followX = x; followY = y; followZ = z; anchor = anchorPos; lockedRotation = rotation;
            Follow(CameraPosition());
        }

        /// <summary>Snapped position for a camera position (pure; QA uses it to check the 0.5 px parallax rule).</summary>
        public Vector3 TargetPosition(Vector3 cameraPos)
        {
            return new Vector3(
                followX ? anchor.x + Snap(cameraPos.x, gridStep) : anchor.x,
                followY ? anchor.y + Snap(cameraPos.y, gridStep) : anchor.y,
                followZ ? anchor.z + Snap(cameraPos.z, gridStep) : anchor.z);
        }

        /// <summary>Move to follow the given camera position (rotation locked).</summary>
        public void Follow(Vector3 cameraPos)
        {
            transform.SetPositionAndRotation(TargetPosition(cameraPos), lockedRotation);
        }

        void LateUpdate()
        {
            Follow(CameraPosition());
        }

        /// <summary>Round to the nearest multiple of step (exact value when step is 0). Double precision so far positions stay exact.</summary>
        public static float Snap(float v, float step)
        {
            if (step <= 0f) return v;
            return (float)(System.Math.Round(v / (double)step) * step);
        }
    }
}
