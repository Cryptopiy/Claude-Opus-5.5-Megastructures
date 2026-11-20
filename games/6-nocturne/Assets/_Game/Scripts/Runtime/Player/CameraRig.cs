// CameraRig: first-person look and the camera pose (MASTER_PROMPT Part 9, 8.9, 14.2.4 vFOV(s)), imported from Thread.
// Mouse look from raw mouse delta counts read through NocInput (no smoothing, no acceleration, Y not inverted), gamepad right stick as a
// rate; world yaw (from +Z toward +X) and pitch (up positive), pitch clamped to +-88, never roll. The camera sits at the
// player's eye; the vertical FOV is NocSchedule.Fov(s) every frame (Hor+ is Unity's default for a vertical FOV). The
// AutoWalker drives the look through SetLook; the capture harness overrides the whole pose.
// Nocturne deltas: near 0.05 / far 12 000 (14.12; Thread 0.1 / 250 000), SnapEye (The Ring's Teleport calls it), the 0.2 m
// camera sphere check against the corridor colliders counted for QA (Part 10 item 8), NocRig.Camera / Yaw / Pitch written.
// Order: AutoWalker (-120) sets the look absolutely, PlayerController (-100) moves with the current yaw, Update (-50)
// integrates manual look input; LateUpdate places the camera after the player moved and before ProgressDirector (500)
// computes the uniforms. Zero allocations per frame.
using System;
using UnityEngine;

namespace Megastructure
{
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour, ICameraControl
    {
        public static CameraRig Instance { get; private set; }
        /// <summary>Frames in which the 0.2 m camera sphere touched a corridor collider (QA expects 0).</summary>
        public static int SphereContacts { get; private set; }

        [Tooltip("Degrees per raw mouse count (Part 9: raw input, no acceleration).")]
        public float mouseDegPerCount = PlayerTuning.MouseDegPerCount;
        [Tooltip("Gamepad right-stick look rate at full deflection (deg/s, quadratic response).")]
        public float stickDegPerSec = PlayerTuning.StickDegPerSec;

        Camera m_cam;
        float m_yaw, m_pitch;
        bool m_lookInput;
        int m_lastDriveFrame = -10;

        bool m_override;
        Vector3 m_ovPos;
        float m_ovYaw, m_ovPitch, m_ovFov;

        bool m_captureMode;

        // ---------------------------------------------------------------- ICameraControl
        public Camera Camera => m_cam;
        public float Yaw => m_yaw;
        public float Pitch => m_pitch;
        public bool PoseOverridden => m_override;
        public bool LookInputThisFrame => m_lookInput;
        /// <summary>True while the AutoWalker drives the look (sub-threshold mouse noise is ignored then).</summary>
        public bool ExternalLook { get; set; }

        void Awake()
        {
            Instance = this;
            SphereContacts = 0;
            m_cam = GetComponent<Camera>();
            NocServices.CameraControl = this;
            NocRig.Camera = m_cam;
            m_captureMode = CliArgs.Capturing;
            // 14.12: near 0.05 m, far 12 000 m (the scene builder sets the same; enforced here so nothing drifts)
            m_cam.nearClipPlane = NocturneConstants.CameraNear;
            m_cam.farClipPlane = NocturneConstants.CameraFar;

            double s = CliArgs.StartS;
            GazeSchedule.Evaluate(s, out double yaw, out double pitch);
            m_yaw = (float)NocMath.WrapDeg(yaw);
            // the opening frame equals the first cinematic gaze key (KS00 pitch -2); the QA walk looks forward at pitch 0
            m_pitch = (float)(CliArgs.AutoTest && !CliArgs.Cinematic ? NocturneConstants.GazeDefaultPitch : pitch);
            ApplyPose();
        }

        void Start()
        {
            if (!m_captureMode && !CliArgs.AutoTest) NocInput.SetCursorLocked(true);
        }

        void OnDestroy()
        {
            if (ReferenceEquals(NocServices.CameraControl, this)) NocServices.CameraControl = null;
            if (NocRig.Camera == m_cam) NocRig.Camera = null;
            if (Instance == this) Instance = null;
        }

        void OnApplicationFocus(bool focus)
        {
            bool paused = NocServices.Fade != null && NocServices.Fade.Paused;
            if (focus && !paused && !m_captureMode && !CliArgs.AutoTest) NocInput.SetCursorLocked(true);
        }

        // ---------------------------------------------------------------- API
        public void SetLook(float worldYawDeg, float pitchDeg)
        {
            m_yaw = (float)NocMath.WrapDeg(worldYawDeg);
            m_pitch = Mathf.Clamp(pitchDeg, -PlayerTuning.PitchLimit, PlayerTuning.PitchLimit);
            m_lastDriveFrame = Time.frameCount;
            NocRig.Yaw = m_yaw; NocRig.Pitch = m_pitch;
        }

        public void SetPoseOverride(Vector3 position, float worldYawDeg, float pitchDeg, float fovDeg)
        {
            m_override = true;
            m_ovPos = position;
            m_ovYaw = (float)NocMath.WrapDeg(worldYawDeg);
            m_ovPitch = Mathf.Clamp(pitchDeg, -PlayerTuning.PitchLimit, PlayerTuning.PitchLimit);
            m_ovFov = fovDeg;
            ApplyPose();   // immediately, so a capture can render right after this call
        }

        public void ClearPoseOverride()
        {
            m_override = false;
            ApplyPose();
        }

        /// <summary>The Ring's Teleport calls this after moving the feet: the eye follows at once (no smoothing to reset here).</summary>
        public void SnapEye() { ApplyPose(); }

        // ---------------------------------------------------------------- frame
        void Update()
        {
            m_lookInput = false;
            bool paused = NocServices.Fade != null && NocServices.Fade.Paused;
            if (paused || NocState.Paused) return;
            if (CliArgs.AutoTest || m_captureMode) return;   // QA: input is ignored entirely
            m_lookInput = NocInput.LookActive(PlayerTuning.LookThresholdCounts, PlayerTuning.StickThreshold);
            Vector2 d = NocInput.LookDegrees(mouseDegPerCount, stickDegPerSec);
            // While the AutoWalker drives the look it sets yaw/pitch absolutely each frame; sub-threshold mouse noise is not
            // integrated on top of it (above-threshold input cancels the autowalk in the PlayerController).
            if (ExternalLook || Time.frameCount - m_lastDriveFrame <= 1) return;
            m_yaw = (float)NocMath.WrapDeg(m_yaw + d.x);
            m_pitch = Mathf.Clamp(m_pitch + d.y, -PlayerTuning.PitchLimit, PlayerTuning.PitchLimit);
            NocRig.Yaw = m_yaw; NocRig.Pitch = m_pitch;
        }

        void LateUpdate()
        {
            ApplyPose();
            if (!m_override && Physics.CheckSphere(transform.position, (float)NocturneConstants.CameraSphere, PlayerTuning.CorridorMask, QueryTriggerInteraction.Ignore))
            {
                SphereContacts++;
                if (SphereContacts <= 5) Debug.LogWarning(FormattableString.Invariant($"[NOC] camera sphere touches a corridor collider at {transform.position}"));
            }
        }

        void ApplyPose()
        {
            if (m_cam == null) return;
            if (m_override)
            {
                transform.SetPositionAndRotation(m_ovPos, Quaternion.Euler(-m_ovPitch, m_ovYaw, 0f));
                var p = NocServices.Player;
                double s = p != null ? p.S : ProgressDirector.S;
                m_cam.fieldOfView = m_ovFov > 0f ? m_ovFov : (float)NocSchedule.Fov(s);
                return;
            }
            var player = NocServices.Player;
            Vector3 pos = player != null ? player.EyePosition : transform.position;
            transform.SetPositionAndRotation(pos, Quaternion.Euler(-m_pitch, m_yaw, 0f));   // never roll
            m_cam.fieldOfView = (float)NocSchedule.Fov(player != null ? player.S : ProgressDirector.S);
            NocRig.Yaw = m_yaw; NocRig.Pitch = m_pitch;
        }
    }
}
