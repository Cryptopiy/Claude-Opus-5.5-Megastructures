using System;
using UnityEngine;

namespace Megastructure
{
    /// <summary>
    /// QA and cinematic autowalk (Part 9, 11.8, 14.10 Autoplay), imported from The Ring; the logic lives in
    /// <see cref="WalkPlanner"/>, this component feeds it the player state and applies its output. The walker moves the
    /// CharacterController through PlayerController.DriveExternal, so every collider applies.
    /// - QA (<see cref="BeginQa"/>, AutoTestRunner / WallBot / SoakRunner through IPlayerState.SetDrive): constant speed
    ///   (1.5 m/s, 2.5 with -brisk, -speed X), 0.2 s start, gaze forward along the path at pitch 0, lateral target for the wall bot.
    /// - Cinematic (C key, -cinematic, or the autotest with -cinematic): starts from the current progress, constant 2.5 m/s after a
    ///   0.5 s ramp, the 14.5 gaze keys (keyed to s) with the critically damped, rate-limited follower and the handheld sway.
    /// Both stop with the 14.11 stop ramp at s 1500. Nocturne edits: no hold list, no -speed brisk string, no restart path
    /// (GameBootstrap.Restarting), no QA hook forcing a speed (Part 3.1 (c)); the cinematic cruise is always 2.5 m/s.
    /// Runs before the PlayerController (execution order -120). Allocation-free per frame.
    /// </summary>
    [DefaultExecutionOrder(-120)]
    public sealed class AutoWalker : MonoBehaviour
    {
        /// <summary>The walk path (the 14.4 filleted centre line).</summary>
        public static FilletPath Path => NocLanes.Path;

        public static AutoWalker Instance { get; private set; }
        /// <summary>True while the walker drives the player.</summary>
        public static bool Active { get; private set; }
        public static WalkMode Mode { get; private set; }

        WalkPlanner _plan;
        int _seed;
        WalkMode _forced;
        double _forcedSpeed = NocturneConstants.WalkSpeed;
        double _forcedLateral;
        int _forcedEdge;
        bool _restart;
        PlayerController _pc;

        /// <summary>External control (QA harness): force a walk mode from the current progress; None returns to normal.
        /// A request while a walk is running restarts it with the new mode and speed on the next frame.</summary>
        public static void Request(WalkMode mode, double speed = NocturneConstants.WalkSpeed)
        {
            if (Instance == null) return;
            Instance._forced = mode;
            Instance._forcedSpeed = speed;
            Instance._restart = true;
        }

        /// <summary>QA entry point (IPlayerState.SetDrive): constant-speed walk along the path, gaze forward at pitch 0.
        /// lateral: fixed lateral target (m, right positive; NaN = 0); edgeSide -1 / +1 walks 0.40 m inside that lane edge
        /// (wall bot, 14.10). fromS (NaN = current progress) places the player on the path first.</summary>
        public static bool BeginQa(double speed, int seed, double fromS, double lateral, int edgeSide = 0)
        {
            var w = Instance;
            if (w == null || NocRig.Player == null) return false;
            double lat = double.IsNaN(lateral) ? 0.0 : lateral;
            speed = NocMath.Clamp(speed, 0.1, 5.0);
            // Thread's bots call SetDrive every frame: an unchanged request keeps the running walk (no restart)
            if (w._forced == WalkMode.AutoTest && Mode == WalkMode.AutoTest && double.IsNaN(fromS) &&
                Math.Abs(w._forcedSpeed - speed) < 1e-9 && Math.Abs(w._forcedLateral - lat) < 1e-9 && w._forcedEdge == edgeSide) return true;
            if (w._plan == null || w._seed != seed) { w._plan = new WalkPlanner(Path, seed); w._seed = seed; }
            if (!double.IsNaN(fromS) && System.Math.Abs(fromS - NocState.S) > 1.0) PlaceAt(fromS);
            w._forcedLateral = lat;
            w._forcedEdge = edgeSide;
            Request(WalkMode.AutoTest, speed);
            return true;
        }

        /// <summary>Starts the cinematic walk as a QA request (non-cancellable, used by -autotest -cinematic).</summary>
        public static void BeginCinematicQa()
        {
            if (Instance == null) return;
            Request(WalkMode.Cinematic, NocturneConstants.CinematicSpeed);
        }

        /// <summary>Places the player's feet on the path at progress s, facing the path direction.</summary>
        public static void PlaceAt(double s)
        {
            var pc = PlayerController.Instance;
            if (pc != null) pc.Teleport(s, 0.0);
        }

        /// <summary>The planner (QA reads speed, trigger and stop times).</summary>
        public WalkPlanner Planner => _plan;

        void Awake()
        {
            Instance = this;
            Active = false; Mode = WalkMode.None;
            _pc = GetComponent<PlayerController>();
        }

        void Start()
        {
            if (_plan == null) { _seed = CliArgs.Seed; _plan = new WalkPlanner(Path, _seed); }
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; Active = false; Mode = WalkMode.None; }
        }

        WalkMode Wanted()
        {
            if (_forced != WalkMode.None) return _forced;
            if (NocState.Cinematic) return WalkMode.Cinematic;
            return WalkMode.None;
        }

        void Update()
        {
            if (!NocRig.WorldReady || NocRig.Player == null || _plan == null) return;
            if (_pc == null) _pc = PlayerController.Instance;
            var want = Wanted();
            if (want != Mode || _restart)
            {
                _restart = false;
                if (Mode != WalkMode.None) Deactivate();
                if (want != WalkMode.None) Activate(want);
            }
            if (Mode == WalkMode.None || NocState.Paused) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 pos = NocRig.Player.position;
            _plan.Step(dt, pos.x, pos.z, NocClock.Time);

            // drive the controller toward the target (catch-up limited to 1.5 x the cruise speed)
            double dx = _plan.TargetX - pos.x, dz = _plan.TargetZ - pos.z;
            double maxStep = 1.5 * System.Math.Max(_plan.Speed, NocturneConstants.WalkSpeed) * dt;
            double len = System.Math.Sqrt(dx * dx + dz * dz);
            if (len > maxStep && len > 1e-9) { dx *= maxStep / len; dz *= maxStep / len; }
            if (_pc != null) _pc.DriveExternal(new Vector3((float)dx, 0f, (float)dz));

            var rig = CameraRig.Instance;
            if (rig != null) { rig.ExternalLook = true; rig.SetLook((float)_plan.OutYaw, (float)_plan.OutPitch); }
        }

        void Activate(WalkMode m)
        {
            double cruise = m == WalkMode.Cinematic ? NocturneConstants.CinematicSpeed : _forcedSpeed;
            Vector3 pos = NocRig.Player.position;
            Vector3 pv = NocRig.PlayerVelocity;
            var rig = CameraRig.Instance;
            double yaw = rig != null ? rig.Yaw : NocRig.Yaw, pitch = rig != null ? rig.Pitch : NocRig.Pitch;
            _plan.LateralFixed = m == WalkMode.AutoTest ? _forcedLateral : 0.0;
            _plan.LateralEdgeSide = m == WalkMode.AutoTest ? _forcedEdge : 0;
            _plan.Begin(m, pos.x, pos.z, NocState.S, pv.x, pv.z, yaw, pitch, cruise);
            Mode = m;
            Active = true;
            if (rig != null) rig.ExternalLook = true;
            Debug.Log("[NOC] AutoWalker " + m + " from s " + _plan.D.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " at " + cruise.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " m/s");
        }

        void Deactivate()
        {
            _plan.Stop();
            Mode = WalkMode.None;
            Active = false;
            var rig = CameraRig.Instance;
            if (rig != null) rig.ExternalLook = false;
        }
    }
}
