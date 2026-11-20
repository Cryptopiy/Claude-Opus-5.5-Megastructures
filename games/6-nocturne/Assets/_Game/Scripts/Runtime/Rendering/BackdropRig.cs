using UnityEngine;

namespace Megastructure
{
    /// <summary>
    /// Camera-following far geometry with locked rotation (Part 7.3, 8.9; imported from Abyss): the NocSky dome (r 11 000,
    /// gridStep 0 = exact follow, IMPORT_PLAN 7.6) and far backdrops (grid-snapped so world-fixed patterns stay fixed). Rotation never follows the camera, so a 10 m move shifts nothing but true parallax.
    ///
    /// Add it to the backdrop's root GameObject in a builder and set the fields; it updates in LateUpdate at order +150,
    /// after CameraRig (+100), from this frame's final camera position. <see cref="Follow"/> can also be called directly
    /// (EditMode, PrepareCapture). Per-axis following lets a backdrop keep a fixed height (lattice plane) while tracking xz.
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

        /// <summary>Configure in one call (Build time).</summary>
        public void Configure(float step, bool x, bool y, bool z, Vector3 anchorPos, Quaternion rotation)
        {
            gridStep = step; followX = x; followY = y; followZ = z; anchor = anchorPos; lockedRotation = rotation;
            Follow(CurrentCameraPos());
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
            Follow(CurrentCameraPos());
        }

        /// <summary>The gameplay camera (NocRig.Camera, else Camera.main); a capture camera calls Follow directly.</summary>
        static Vector3 CurrentCameraPos()
        {
            Camera cam = NocRig.Camera != null ? NocRig.Camera : Camera.main;
            return cam != null ? cam.transform.position : Vector3.zero;
        }

        /// <summary>Round to the nearest multiple of step (exact value when step is 0). Double precision so 28 km stays exact.</summary>
        public static float Snap(float v, float step)
        {
            if (step <= 0f) return v;
            return (float)(System.Math.Round(v / (double)step) * step);
        }
    }
}
