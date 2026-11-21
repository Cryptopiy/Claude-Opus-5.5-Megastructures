// CameraRig: first-person look and the camera pose (MASTER_PROMPT Part 9, 8.9, 14.4, 14.12). Imported from Thread
// (D-09 / C-10) with The Ring's 0.2 m camera probe and QA counters.
// Mouse look from raw Mouse.current.delta counts (no smoothing, no acceleration, Y not inverted), gamepad right stick as
// a rate; world yaw (0 = +Z, 90 = +X) and pitch (up positive), pitch clamped to +-88, NEVER roll. The camera sits at the
// player's eye (BwPath.EyeY: distance-smoothed, never a time filter). Optics (vFOV with its 30 m crossfades and 1 deg/s
// slew, near 0.05, far plane by s) are written by AtmosphereDriver, the one writer; a capture pose's free vFOV goes through
// AtmosphereDriver.FovOverride. The AutoWalker drives the look through SetLook (no input is integrated meanwhile); the
// capture harness overrides the whole pose. After the goal trigger (the Keeper's Lantern on the platform, ADDENDUM A1 3.10)
// the input freezes and the END CAMERA runs: ease to (90, 0) over the lantern by t_g 1.5, then the tilt up the whole dam face
// to (90, +28) by t_g 8.5 (smootherstep, peak 7.5 deg/s), held while the picture fades (UiDirector).
// Camera safety (The Ring, 14.4): each LateUpdate a 0.2 m sphere at the eye must overlap no collider; otherwise it is
// pushed out (Physics.ComputePenetration) and Violations counts the frame (QA expects 0). MaxRollDeg must stay 0.
// Update (-50) integrates the look before AutoWalker (-40) and PlayerController (-30) read it; LateUpdate (-50) places the
// camera after the player moved and before ProgressDirector (500). Zero allocations per frame.
using UnityEngine;

namespace Megastructure.Player
{
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour, ICameraControl
    {
        public static CameraRig Instance { get; private set; }
        /// <summary>Frames in which the camera sphere overlapped a collider by more than 1 mm (QA expects 0).</summary>
        public static int Violations;
        /// <summary>Largest penetration depth seen (m).</summary>
        public static float MaxPenetration;
        /// <summary>Largest |roll| of the camera transform seen (degrees; 0.00 +-0.01 manual, at most 0.3 cinematic).</summary>
        public static float MaxRollDeg;

        public const float ProbeRadius = (float)FrameworkConst.CameraProbeRadius;
        const float PenetrationTolerance = 0.001f;

        [Tooltip("Degrees per raw mouse count (Part 9: raw input, no acceleration).")]
        public float mouseDegPerCount = PlayerTuning.MouseDegPerCount;
        [Tooltip("Gamepad right-stick look rate at full deflection (deg/s, quadratic response).")]
        public float stickDegPerSec = PlayerTuning.StickDegPerSec;

        Camera m_cam;
        float m_yaw, m_pitch;
        bool m_lookInput;
        int m_lastDriveFrame = -10;
        double m_endYawV, m_endPitchV, m_endT;

        bool m_override;
        Vector3 m_ovPos;
        float m_ovYaw, m_ovPitch;

        bool m_captureMode;
        SphereCollider m_probe;
        readonly Collider[] m_buf = new Collider[32];

        // ---------------------------------------------------------------- ICameraControl
        public Camera Camera => m_cam;
        public float Yaw => m_yaw;
        public float Pitch => m_pitch;
        public bool PoseOverridden => m_override;
        public bool LookInputThisFrame => m_lookInput;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStatics() { Violations = 0; MaxPenetration = 0f; MaxRollDeg = 0f; }

        void Awake()
        {
            Instance = this;
            m_cam = GetComponent<Camera>();
            BwServices.CameraControl = this;
            GameState.MainCamera = m_cam;
            m_captureMode = CliArgs.CaptureDir != null;
            double s = CliArgs.StartS;
            m_cam.nearClipPlane = (float)FrameworkConst.CameraNear;
            m_cam.farClipPlane = (float)BulwarkSchedule.Far(s);
            m_cam.fieldOfView = (float)BulwarkSchedule.Vfov(s);
            m_yaw = (float)PlayerMath.WrapDeg(BwPath.HeadingDeg(s));
            m_pitch = 0f;

            var go = new GameObject("CameraProbe");
            go.hideFlags = HideFlags.DontSave;
            go.layer = 2;   // Ignore Raycast
            go.transform.position = new Vector3(0f, -1000f, 0f);
            m_probe = go.AddComponent<SphereCollider>();
            m_probe.isTrigger = true;
            m_probe.radius = ProbeRadius;
            ApplyPose();
        }

        void Start()
        {
            if (!m_captureMode) BwInput.SetCursorLocked(true);
        }

        void OnDestroy()
        {
            if (ReferenceEquals(BwServices.CameraControl, this)) BwServices.CameraControl = null;
            if (Instance == this) Instance = null;
            if (m_probe != null) Destroy(m_probe.gameObject);
        }

        void OnApplicationFocus(bool focus)
        {
            bool paused = BwServices.Fade != null && BwServices.Fade.Paused;
            if (focus && !paused && !m_captureMode) BwInput.SetCursorLocked(true);
        }

        // ---------------------------------------------------------------- API
        public void SetLook(float worldYawDeg, float pitchDeg)
        {
            m_yaw = (float)PlayerMath.WrapDeg(worldYawDeg);
            m_pitch = Mathf.Clamp(pitchDeg, -PlayerTuning.PitchLimit, PlayerTuning.PitchLimit);
            m_lastDriveFrame = Time.frameCount;
        }

        public void SetPoseOverride(Vector3 position, float worldYawDeg, float pitchDeg, float fovDeg)
        {
            m_override = true;
            m_ovPos = position;
            m_ovYaw = (float)PlayerMath.WrapDeg(worldYawDeg);
            m_ovPitch = Mathf.Clamp(pitchDeg, -PlayerTuning.PitchLimit, PlayerTuning.PitchLimit);
            AtmosphereDriver.FovOverride = fovDeg > 0f ? fovDeg : float.NaN;   // 14.10.2: key shots use the schedule vFOV
            if (fovDeg > 0f && m_cam != null) m_cam.fieldOfView = fovDeg;
            ApplyPose();   // immediately, so a capture can render right after this call
        }

        public void ClearPoseOverride()
        {
            m_override = false;
            AtmosphereDriver.FovOverride = float.NaN;
            ApplyPose();
        }

        // ---------------------------------------------------------------- frame
        void Update()
        {
            m_lookInput = false;
            bool paused = BwServices.Fade != null && BwServices.Fade.Paused;
            if (paused) return;
            if (GameState.GoalReached && !m_override)
            {
                LookAtLantern(Time.deltaTime);
                return;
            }
            m_endT = 0;
            m_lookInput = BwInput.LookActive(PlayerTuning.LookThresholdCounts, PlayerTuning.StickThreshold);
            Vector2 d = BwInput.LookDegrees(mouseDegPerCount, stickDegPerSec);
            // While the AutoWalker drives the look it sets yaw/pitch absolutely each frame; sub-threshold mouse noise is
            // not integrated on top of it (above-threshold input cancels the autowalk in its own Update).
            if (Time.frameCount - m_lastDriveFrame <= 1) return;
            m_yaw = (float)PlayerMath.WrapDeg(m_yaw + d.x);
            m_pitch = Mathf.Clamp(m_pitch + d.y, -PlayerTuning.PitchLimit, PlayerTuning.PitchLimit);
            RideAssist(Time.deltaTime);
        }

        /// <summary>Manual play in the ride (ADDENDUM A1 00): the look stays free, but once the player has not looked for 1.5 s the
        /// ride keys (GazeSchedule.RideKeys: down-out in the descent, back at the dam on the shuttle) take the look over gently
        /// (critically damped, omega 1.2, faded in over 2 s, at most 12 / 9 deg/s); any look input hands it back at once.</summary>
        void RideAssist(float dt)
        {
            var pc = PlayerController.Instance;
            if (pc == null || !pc.Riding || m_captureMode || dt <= 0f) { m_rideIdle = 0f; m_assistYawV = 0; m_assistPitchV = 0; return; }
            if (m_lookInput) { m_rideIdle = 0f; m_assistYawV = 0; m_assistPitchV = 0; return; }
            m_rideIdle += dt;
            if (m_rideIdle < RideAssistDelay) return;
            GazeSchedule.Evaluate(System.Math.Min(EndingPath.EndS, pc.S + 2.0), out double ty, out double tp);
            double w = BwMath.Smoothstep(RideAssistDelay, RideAssistDelay + 2.0, m_rideIdle);
            double y = BwMath.CriticalDampAngle(m_yaw, ref m_assistYawV, ty, 1.2, dt);
            double p = BwMath.CriticalDamp(m_pitch, ref m_assistPitchV, tp, 1.2, dt);
            double dy = BwMath.Clamp(BwMath.DeltaDeg(m_yaw, y) * w, -12.0 * dt, 12.0 * dt);
            double dp = BwMath.Clamp((p - m_pitch) * w, -9.0 * dt, 9.0 * dt);
            m_yaw = (float)PlayerMath.WrapDeg(m_yaw + dy);
            m_pitch = Mathf.Clamp((float)(m_pitch + dp), -PlayerTuning.PitchLimit, PlayerTuning.PitchLimit);
        }

        public const float RideAssistDelay = 1.5f;
        float m_rideIdle;
        double m_assistYawV, m_assistPitchV;

        /// <summary>The end camera (ADDENDUM A1 3.10): pitch target 0 until t_g 1.5, then EndLookPitch x smootherstep over 7 s
        /// (KS28 at 8.5: foot to crest), held to t_g 18, then + EndLift over 10 s (the stinger above the crest);
        /// yaw target 90 (the dam beyond the lantern); critically damped (omega 2.5) and rate limited 18 / 13 deg/s.</summary>
        public static void EndLookTarget(double tg, out double yaw, out double pitch)
        {
            yaw = BulwarkConst.EndLookYaw;
            pitch = BulwarkConst.EndLookPitch * BwMath.Smootherstep01((tg - BulwarkConst.EndTiltStart) / BulwarkConst.EndTiltTime)
                  + BulwarkConst.EndLift * BwMath.Smootherstep01((tg - BulwarkConst.EndLiftStart) / BulwarkConst.EndLiftTime);
        }

        void LookAtLantern(float dt)
        {
            m_endT += dt;
            EndLookTarget(m_endT, out double yawT, out double pitchT);
            double y = BwMath.CriticalDampAngle(m_yaw, ref m_endYawV, yawT, FrameworkConst.GazeOmega, dt);
            double p = BwMath.CriticalDamp(m_pitch, ref m_endPitchV, pitchT, FrameworkConst.GazeOmega, dt);
            double maxY = FrameworkConst.GazeMaxYawRate * dt, maxP = FrameworkConst.GazeMaxPitchRate * dt;
            double dy = BwMath.Clamp(BwMath.DeltaDeg(m_yaw, y), -maxY, maxY);
            double dp = BwMath.Clamp(p - m_pitch, -maxP, maxP);
            m_yaw = (float)PlayerMath.WrapDeg(m_yaw + dy);
            m_pitch = Mathf.Clamp((float)(m_pitch + dp), -PlayerTuning.PitchLimit, PlayerTuning.PitchLimit);
        }

        void LateUpdate()
        {
            ApplyPose();
        }

        void ApplyPose()
        {
            if (m_cam == null) return;
            if (m_override)
            {
                transform.SetPositionAndRotation(m_ovPos, Quaternion.Euler(-m_ovPitch, m_ovYaw, 0f));
            }
            else
            {
                var player = BwServices.Player;
                Vector3 pos = player != null ? player.EyePosition : transform.position;
                // no push-out in the ending: the cabin rides through the ledge's corridor slabs, and the cabin and the platform
                // have no colliders (ADDENDUM A1 00; PlayerController ride mode)
                var pc = PlayerController.Instance;
                bool ending = pc != null && (pc.Riding || pc.S > BwPath.Length);
                if (Application.isPlaying && !m_captureMode && !ending) pos = PushOut(pos);
                transform.SetPositionAndRotation(pos, Quaternion.Euler(-m_pitch, m_yaw, 0f));   // never roll
            }
            GameState.CameraPos = transform.position;
            GameState.CameraRot = transform.rotation;
            float roll = Mathf.Abs(Mathf.DeltaAngle(0f, transform.eulerAngles.z));
            if (roll > MaxRollDeg) MaxRollDeg = roll;
        }

        Vector3 PushOut(Vector3 pos)
        {
            if (m_probe == null) return pos;
            bool violated = false;
            for (int iter = 0; iter < 3; iter++)
            {
                int n = Physics.OverlapSphereNonAlloc(pos, ProbeRadius, m_buf, ~0, QueryTriggerInteraction.Ignore);
                bool any = false;
                for (int i = 0; i < n; i++)
                {
                    var c = m_buf[i];
                    if (c == null || c == m_probe || c is CharacterController) continue;
                    Transform ct = c.transform;
                    if (Physics.ComputePenetration(m_probe, pos, Quaternion.identity, c, ct.position, ct.rotation, out Vector3 dir, out float dist)
                        && dist > PenetrationTolerance)
                    {
                        pos += dir * (dist + 0.0005f);
                        if (dist > MaxPenetration) MaxPenetration = dist;
                        any = true;
                    }
                }
                if (!any) break;
                violated = true;
            }
            if (violated) Violations++;
            return pos;
        }
    }
}
