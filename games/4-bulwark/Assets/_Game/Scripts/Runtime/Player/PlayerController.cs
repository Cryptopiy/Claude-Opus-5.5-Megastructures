// PlayerController: the walker in lane coordinates (MASTER_PROMPT Part 9, 14.4, 14.11; D-01). Thread's controller
// generalised on BwPath: state (s, lateral) in double, s = filleted plan distance (BwPath), lateral = signed distance to
// the right of the heading. The floor is the analytic BwPath.FloorY(s) (no physics stepping) and the eye is BwPath.EyeY(s),
// the 2.5 m triangular-kernel average of the floor (smoothed in DISTANCE, Part 9). Primary safety is analytic (s and the
// capsule-centre lateral are clamped to BwPath.Lane(s)).
// Physical check (D-01, 14.4): a CharacterController (radius 0.35, slope 45, step 0.3, minMoveDistance 0; capsule from
// 0.30 to 1.90 m above the floor so floor and stair colliders are never touched) follows the analytic target with
// cc.Move(target - pos). If its xz differs from the target by more than 1 cm the move counts as a rejection and (s, lateral)
// are projected back from where the capsule really is; y always comes from the analytic floor. Rejections must stay 0.
// From The Ring: the safety snap (a safety volume on layer 31 returns the player to the last safe lane point at the current
// s, with a warning), the first-movement event (hides the hint).
// Drive mode (AutoWalker, WallBot, tests): ds/dt = the commanded speed EXACTLY (speed is measured along the path, Part 9;
// no ForwardToS factor, no acceleration smoothing when DriveNoSmoothing), the lateral follows the target exactly
// (feed-forward; D-02), brisk comes from the mode (DriveBrisk), never from a speed threshold (3.1 e).
// Ending (ADDENDUM A1 00 LEAD DECISION 1): at s >= 1498.5 input and drive are replaced by the board StopRamp to rest at
// s 1500 in the elevator cabin; then the RIDE (EndingPath): s advances at 2.5 per ride second at every speed (doors, the 75 s
// descent down the lee face, the transfer, the 35 s shuttle west, the doors), the eye rides in the cabin (CharacterController
// off, movement input ignored, look free), EndingRide is written every frame; then the platform walk (input or drive on
// EndingPath's lane) to the Keeper's Lantern: at s >= EndingPath.SGoal the StopRamp brings the walker to rest at SRest and UI
// and audio get BeginEnding. -ride-s S (QA) starts the run at ending s S (skips the walk).
// Execution order: CameraRig.Update (-50) -> AutoWalker.Update (-40) -> PlayerController.Update (-30) ->
// CameraRig.LateUpdate -> ProgressDirector.LateUpdate (500). Zero allocations per frame.
using System;
using UnityEngine;

namespace Megastructure.Player
{
    [DefaultExecutionOrder(-30)]
    public class PlayerController : MonoBehaviour, IPlayerState
    {
        public static PlayerController Instance { get; private set; }

        // ---------------------------------------------------------------- state (double)
        double m_s, m_lat;
        double m_velT, m_velR;          // plan velocity in the lane frame (m/s): along the tangent, to the right
        double m_accT, m_accR;          // critically damped velocity state (acceleration smoothing 0.2 s)
        double m_speed;                 // horizontal ground speed (m/s)
        bool m_brisk;
        FootstepCadence m_steps;
        Vector3 m_eyePos;
        Vector3 m_feetPos;
        CharacterController m_cc;

        // drive (AutoWalker / WallBot / tests)
        bool m_driveActive;
        double m_driveSpeed, m_driveLat;

        // goal / stop ramp (14.11)
        bool m_finished, m_resting;
        StopRamp m_ramp;
        double m_rampT;

        // ending: board ramp into the cabin, the ride clock, the platform walk
        bool m_boarding, m_riding;
        StopRamp m_boardRamp;
        double m_boardT, m_rideT;
        double m_cabinSpeed;

        int m_rejections, m_snaps;
        bool m_loggedReject, m_movedRaised;
        double m_lastSafeS, m_lastSafeLat;

        // ---------------------------------------------------------------- IPlayerState
        public double S => m_s;
        public double Lateral => m_lat;
        public float Speed => (float)PathSpeed;
        public float GroundSpeed => (float)m_speed;
        public bool Brisk => m_brisk;
        public bool Finished => m_finished;
        public Vector3 EyePosition => m_eyePos;
        public bool Frozen { get; set; }

        // ---------------------------------------------------------------- extra public state (QA, AutoWalker, UI)
        public bool Resting => m_resting;
        public bool HasMoved { get; private set; }
        /// <summary>CharacterController results that differed from the analytic target by more than 1 cm (must stay 0).</summary>
        public int Rejections => m_rejections;
        /// <summary>Safety-volume snaps (must stay 0).</summary>
        public int SafetySnaps => m_snaps;
        public Vector3 FeetPosition => m_feetPos;
        /// <summary>Drive speed applied exactly, without acceleration smoothing (cinematic and test walks).</summary>
        public bool DriveNoSmoothing { get; set; }
        /// <summary>Brisk state while driven (cinematic = true from frame 1; never derived from the speed).</summary>
        public bool DriveBrisk { get; set; }
        public bool DriveActive => m_driveActive;
        /// <summary>Progress speed ds/dt (m/s) this frame.</summary>
        public double PathSpeed { get; private set; }
        public int FootstepCount => m_steps.count;
        public double StepPhase => m_steps.phase;
        public double StopRampDuration => m_finished ? m_ramp.T : 0.0;
        public double StopRampElapsed => m_finished ? m_rampT : 0.0;
        /// <summary>Boarding (the stop ramp into the cabin) or riding (s driven by the ride clock): movement input is ignored.</summary>
        public bool Riding => m_boarding || m_riding;
        public bool Boarding => m_boarding;
        /// <summary>Ride clock (s since the walker came to rest in the cabin; EndingPath.TauL).</summary>
        public double RideTime => m_rideT;
        /// <summary>Speed of the cabin (m/s) this frame (0 outside the ride).</summary>
        public double CabinSpeed => m_cabinSpeed;
        /// <summary>On the lantern platform (after the ride).</summary>
        public bool OnFoot => !Riding && m_s >= EndingPath.SFoot0 - 1e-9;

        // ---------------------------------------------------------------- lifecycle
        void Awake()
        {
            Instance = this;
            BwServices.Player = this;
            m_steps = FootstepCadence.Create();
            m_cc = GetComponent<CharacterController>();
            if (m_cc == null) m_cc = gameObject.AddComponent<CharacterController>();
            ConfigureController(m_cc);
            SetState(CliArgs.StartS, 0.0);
            double rideS = CliArgs.GetDouble("-ride-s", double.NaN);   // QA: start in the ending (e.g. 1500.01 = the ride)
            if (!double.IsNaN(rideS) && rideS > BwPath.Length) Teleport(rideS, 0.0);
        }

        /// <summary>14.4 / D-01 CharacterController settings; the capsule spans 0.30-1.90 m above the feet.</summary>
        public static void ConfigureController(CharacterController cc)
        {
            cc.radius = (float)FrameworkConst.PlayerRadius;
            cc.slopeLimit = (float)FrameworkConst.CcSlopeLimit;
            cc.stepOffset = (float)FrameworkConst.CcStepOffset;
            cc.minMoveDistance = 0f;
            cc.skinWidth = 0.02f;
            cc.height = 1.6f;
            cc.center = new Vector3(0f, 1.1f, 0f);
        }

        void OnDestroy()
        {
            if (ReferenceEquals(BwServices.Player, this)) BwServices.Player = null;
            if (Instance == this) Instance = null;
        }

        // ---------------------------------------------------------------- API
        public void Teleport(double s, double lateral)
        {
            m_boarding = false; m_boardT = 0;
            m_riding = EndingPath.InRide(s);
            m_rideT = m_riding ? EndingPath.TauL(s) : 0.0;
            if (s < FrameworkConst.GoalTriggerS) { m_finished = false; m_resting = false; m_rampT = 0; }
            SetState(s, lateral);
            m_steps = FootstepCadence.Create();
            if (m_s >= FrameworkConst.GoalTriggerS && m_s <= BwPath.Length + 1e-9 && !m_finished)
            {
                // on the trigger stretch before the cabin: board from here (the ramp to s 1500)
                m_boarding = true;
                m_boardRamp = StopRamp.Begin(m_s, PlayerTuning.StopRampMinSpeed, BwPath.Length);
            }
            else if (m_finished)
            {
                m_resting = true;   // teleported while the ending runs: stay where placed
            }
        }

        public void SetDrive(bool active, float speed, float lateralTarget)
        {
            m_driveActive = active;
            m_driveSpeed = speed;
            m_driveLat = lateralTarget;
            if (!active) { DriveNoSmoothing = false; DriveBrisk = false; }
        }

        void SetState(double s, double lateral)
        {
            m_s = PlayerMath.ClampEndingS(s);
            m_lat = PathLateral(m_s, lateral);
            m_velT = m_velR = m_accT = m_accR = 0;
            m_speed = 0; PathSpeed = 0;
            m_lastSafeS = m_s; m_lastSafeLat = m_lat;
            UpdatePose(true);
        }

        // ---------------------------------------------------------------- frame
        void Update()
        {
            double dt = Math.Min(Time.deltaTime, PlayerTuning.MaxStepDt);
            bool ready = GameBootstrap.Instance == null || GameBootstrap.Ready;
            bool paused = BwServices.Fade != null && BwServices.Fade.Paused;

            if (Frozen)
            {
                m_velT = m_velR = m_accT = m_accR = 0; m_speed = 0; PathSpeed = 0;
            }
            else if (dt > 0 && ready && !paused)
            {
                if (m_finished) StepStopRamp(dt);
                else if (m_boarding) StepBoard(dt);
                else if (m_riding) StepRide(dt);
                else StepMove(dt);
            }
            GameState.PlayerPos = m_feetPos;
            PublishRide();
        }

        // ---------------------------------------------------------------- the ending (EndingPath)
        static bool InEnding(double s) => s > BwPath.Length + 1e-9;

        /// <summary>Floor point at (s, lateral) on the walked path or the ending timeline.</summary>
        static void PathWorld(double s, double lat, out double x, out double y, out double z)
        {
            if (InEnding(s)) EndingPath.World(s, lat, out x, out y, out z);
            else BwPath.World(s, lat, out x, out y, out z);
        }

        static double PathHeading(double s) => InEnding(s) ? EndingPath.HeadingDeg(s) : BwPath.HeadingDeg(s);
        static double PathEyeY(double s) => InEnding(s) ? EndingPath.EyeY(s) : BwPath.EyeY(s);
        static double PathLateral(double s, double lat) => InEnding(s) ? EndingPath.ClampLateral(s, lat) : PlayerMath.ClampLateral(s, lat);
        static void PathLane(double s, out double lo, out double hi)
        {
            if (InEnding(s)) EndingPath.Lane(s, out lo, out hi);
            else BwPath.Lane(s, out lo, out hi);
        }

        /// <summary>The board ramp from the trigger at 1498.5 to the rest at s 1500 in the cabin; then the ride clock starts.</summary>
        void StepBoard(double dt)
        {
            double sOld = m_s, latOld = m_lat;
            m_boardT += dt;
            m_s = m_boardRamp.PositionAt(m_boardT);
            m_lat = PlayerMath.ClampLateral(m_s, m_lat);
            m_velT = m_boardRamp.SpeedAt(m_boardT);
            m_velR = 0;
            UpdatePose(false);
            Account(sOld, latOld, dt);
            if (m_boardRamp.Done(m_boardT))
            {
                m_s = BwPath.Length;
                m_boarding = false;
                m_riding = true;
                m_rideT = 0;
                m_velT = m_velR = m_accT = m_accR = 0; m_speed = 0; PathSpeed = 0;
                UpdatePose(false);
                Debug.Log($"[BW] ending: at rest in the cabin at s={m_s:F3} after {m_boardT:F2} s; the ride starts (platform at s {EndingPath.SFoot0:F1})");
            }
        }

        /// <summary>The ride: s from the ride clock at RideSRate (any walking speed); input ignored, look free.</summary>
        void StepRide(double dt)
        {
            double sOld = m_s;
            m_rideT += dt;
            m_s = EndingPath.SOfTau(m_rideT);
            m_lat = 0;
            if (m_s >= EndingPath.SFoot0)
            {
                m_s = EndingPath.SFoot0;
                m_riding = false;
                m_velT = m_velR = m_accT = m_accR = 0;
                Debug.Log($"[BW] ending: ride complete at s={m_s:F3} (ride time {m_rideT:F2} s); the platform walk");
            }
            UpdatePose(false);
            PathSpeed = dt > 0 ? (m_s - sOld) / dt : 0;
            m_speed = 0;   // the walker stands; the cabin moves (CabinSpeed)
        }

        void PublishRide()
        {
            if (!InEnding(m_s) && !m_boarding && !m_riding)
            {
                if (EndingRide.Phase != EndingRidePhase.None) EndingRide.Reset();
                m_cabinSpeed = 0;
                return;
            }
            EndingRidePhase ph;
            double prog;
            if (m_boarding) { ph = EndingRidePhase.Boarding; prog = 0.0; }
            else if (m_riding && m_s <= BwPath.Length) { ph = EndingRidePhase.Boarding; prog = 0.0; }
            else ph = EndingPath.PhaseAt(m_s, out prog);
            if (EndingPath.InRide(m_s))
            {
                EndingPath.Cabin(m_s, out _, out _, out _, out double vx, out double vy);
                m_cabinSpeed = Math.Sqrt(vx * vx + vy * vy);
            }
            else m_cabinSpeed = 0;
            EndingRide.Set(ph, (float)prog, (float)EndingPath.Altitude(m_eyePos.y), (float)EndingPath.DistanceFromFace(m_eyePos.x), (float)m_cabinSpeed);
        }

        /// <summary>World plan distance per unit of s at a lateral offset (1 on straights; 1 + lat / r on arcs).</summary>
        static double MetresPerS(double s, double lat)
        {
            const double h = 0.05;
            double s0 = InEnding(s) ? Math.Max(EndingPath.SFoot0, Math.Min(EndingPath.EndS - h, s)) : Math.Max(0.0, Math.Min(BwPath.Length - h, s));
            PathWorld(s0, lat, out double x0, out _, out double z0);
            PathWorld(s0 + h, lat, out double x1, out _, out double z1);
            double d = Math.Sqrt((x1 - x0) * (x1 - x0) + (z1 - z0) * (z1 - z0)) / h;
            return d > 1e-3 ? d : 1.0;
        }

        void StepMove(double dt)
        {
            double sOld = m_s, latOld = m_lat;
            m_lastSafeS = sOld; m_lastSafeLat = latOld;
            double sNew, latNew;
            if (m_driveActive)
            {
                // ---- drive: progress speed exact (Part 9), lateral feed-forward
                m_brisk = DriveBrisk;
                double v = m_driveSpeed;
                if (!DriveNoSmoothing)
                {
                    PlayerMath.CritDamp(ref m_velT, ref m_accT, v, FrameworkConst.AccelSmoothTime, dt);
                    v = m_velT;
                }
                else { m_velT = v; m_accT = 0; }
                sNew = ClampWalkS(m_s + v * dt);
                latNew = PathLateral(sNew, m_driveLat);
                m_velR = dt > 0 ? (latNew - m_lat) / dt : 0; m_accR = 0;
            }
            else
            {
                // ---- player input, relative to the camera yaw in the lane frame
                Vector2 input = BwInput.Move();
                m_brisk = BwInput.Brisk();
                double speed = m_brisk ? FrameworkConst.BriskSpeed : FrameworkConst.WalkSpeed;
                var cam = BwServices.CameraControl;
                double heading = PathHeading(m_s);
                double rel = ((cam != null ? cam.Yaw : heading) - heading) * BwMath.Deg;
                double c = Math.Cos(rel), sn = Math.Sin(rel);
                double tgtT = speed * (input.y * c - input.x * sn);
                double tgtR = speed * (input.y * sn + input.x * c);
                if (!m_movedRaised && input.sqrMagnitude > PlayerTuning.CancelMove * PlayerTuning.CancelMove)
                {
                    m_movedRaised = true;
                    BwEvents.RaisePlayerMoved();
                    BwServices.Fade?.HideHint();
                }
                PlayerMath.CritDamp(ref m_velT, ref m_accT, tgtT, FrameworkConst.AccelSmoothTime, dt);
                PlayerMath.CritDamp(ref m_velR, ref m_accR, tgtR, FrameworkConst.AccelSmoothTime, dt);
                // forward metres -> s at this lateral (arcs): ds = forward / MetresPerS
                sNew = ClampWalkS(m_s + m_velT * dt / MetresPerS(m_s, m_lat));
                latNew = PathLateral(sNew, m_lat + m_velR * dt);
                bool onFoot = InEnding(m_s);
                if ((sNew <= (onFoot ? EndingPath.SFoot0 : 0.0) && m_velT < 0) || (sNew >= (onFoot ? EndingPath.EndS : BwPath.Length) && m_velT > 0)) { m_velT = 0; m_accT = 0; }
                PathLane(sNew, out double lo, out double hi);
                if ((latNew >= hi && m_velR > 0) || (latNew <= lo && m_velR < 0)) { m_velR = 0; m_accR = 0; }
            }

            m_s = sNew; m_lat = latNew;
            UpdatePose(false);
            Account(sOld, latOld, dt);

            // ---- board trigger (once): input and drive are replaced by the ramp into the cabin (rest at s 1500), then the ride
            if (!InEnding(m_s) && !m_boarding && m_s >= FrameworkConst.GoalTriggerS)
            {
                double v0 = Math.Max(0.0, PathSpeed);
                m_boardRamp = StopRamp.Begin(m_s, v0, BwPath.Length);
                m_boardT = 0;
                m_boarding = true;
                Debug.Log($"[BW] board trigger at s={m_s:F3} v0={v0:F3} m/s, stop ramp T={m_boardRamp.T:F3} s into the cabin");
            }
            // ---- goal trigger at the Keeper's Lantern (once): the stop ramp of 14.11, then the end sequence
            if (!m_finished && InEnding(m_s) && m_s >= EndingPath.SGoal)
            {
                double v0 = Math.Max(0.0, PathSpeed);
                m_ramp = StopRamp.Begin(m_s, v0, EndingPath.SRest);
                m_rampT = 0;
                m_finished = true;
                Debug.Log($"[BW] goal trigger at s={m_s:F3} v0={v0:F3} m/s, stop ramp T={m_ramp.T:F3} s");
                BwServices.Fade?.BeginEnding();
                BwServices.Audio?.BeginEnding();
            }
        }

        /// <summary>s clamp of a free walk: the walked path 0..1500, or the platform walk SFoot0..EndS.</summary>
        double ClampWalkS(double s) => InEnding(m_s) ? Math.Max(EndingPath.SFoot0, Math.Min(EndingPath.EndS, s)) : PlayerMath.ClampS(s);

        void StepStopRamp(double dt)
        {
            if (m_resting) { m_velT = m_velR = 0; m_speed = 0; PathSpeed = 0; return; }
            double sOld = m_s, latOld = m_lat;
            m_rampT += dt;
            m_s = m_ramp.PositionAt(m_rampT);
            m_lat = PathLateral(m_s, m_lat);
            m_velT = m_ramp.SpeedAt(m_rampT);
            m_velR = 0;
            UpdatePose(false);
            Account(sOld, latOld, dt);
            if (m_ramp.Done(m_rampT))
            {
                m_s = m_ramp.rest;
                m_resting = true;
                m_velT = m_velR = m_accT = m_accR = 0;
                m_speed = 0; PathSpeed = 0;
                UpdatePose(false);
                Debug.Log($"[BW] player at rest at s={m_s:F3} after {m_rampT:F2} s of stop ramp");
            }
        }

        /// <summary>Speeds, footsteps and the movement flag from the change of (s, lateral) this frame.</summary>
        void Account(double sOld, double latOld, double dt)
        {
            PathWorld(sOld, latOld, out double x0, out _, out double z0);
            PathWorld(m_s, m_lat, out double x1, out _, out double z1);
            double dist = Math.Sqrt((x1 - x0) * (x1 - x0) + (z1 - z0) * (z1 - z0));
            PathSpeed = dt > 0 ? (m_s - sOld) / dt : 0;
            m_speed = dt > 0 ? dist / dt : 0;
            if (dist > 1e-6) HasMoved = true;
            if (m_steps.Advance(m_s - sOld, m_s, Math.Abs(PathSpeed)))
            {
                var audio = BwServices.Audio;
                if (audio != null) audio.Footstep(PlayerMath.Surface(m_s), m_steps.Pan, PlayerMath.FootstepGain(m_speed));
            }
        }

        void UpdatePose(bool teleport)
        {
            PathWorld(m_s, m_lat, out double fx, out double fy, out double fz);
            var target = new Vector3((float)fx, (float)fy, (float)fz);
            bool ending = InEnding(m_s) || m_riding;
            if (m_cc != null && Application.isPlaying && m_cc.enabled == ending) m_cc.enabled = !ending;   // none in the cabin or on the platform
            if (ending)
            {
                transform.position = target;
                m_feetPos = target;
                m_eyePos = new Vector3(target.x, (float)PathEyeY(m_s), target.z);
                return;
            }
            if (m_cc != null && m_cc.enabled && Application.isPlaying && !teleport && !Frozen)
            {
                Vector3 delta = target - transform.position;
                m_cc.Move(delta);
                Vector3 got = transform.position;
                double ex = got.x - target.x, ez = got.z - target.z;
                if (ex * ex + ez * ez > PlayerTuning.RejectionTolerance * PlayerTuning.RejectionTolerance)
                {
                    m_rejections++;
                    if (!m_loggedReject)
                    {
                        m_loggedReject = true;
                        Debug.LogWarning($"[BW] CharacterController rejected a move at s={m_s:F2} lateral={m_lat:F3} by {Math.Sqrt(ex * ex + ez * ez):F3} m (the analytic lane should prevent this)");
                    }
                    ProjectBack(got.x, got.z);
                    BwPath.World(m_s, m_lat, out fx, out fy, out fz);
                    target = new Vector3((float)fx, (float)fy, (float)fz);
                    m_velT = m_velR = m_accT = m_accR = 0;
                }
                if ((transform.position - target).sqrMagnitude > 1e-8f)
                {
                    transform.position = target;   // y always from the analytic floor
                    Physics.SyncTransforms();
                }
            }
            else
            {
                transform.position = target;
                if (Application.isPlaying) Physics.SyncTransforms();
            }
            m_feetPos = target;
            m_eyePos = new Vector3(target.x, (float)BwPath.EyeY(m_s), target.z);
        }

        /// <summary>(s, lateral) of a world xz point: projection onto the path plus the signed distance to the right.</summary>
        void ProjectBack(double x, double z)
        {
            double s = PlayerMath.ClampS(BwPath.Project(x, z, m_s));
            BwPath.Point(s, out double cx, out double cz);
            BwPath.Tangent(s, out double tx, out double tz);
            double lat = (x - cx) * tz - (z - cz) * tx;   // right of the heading = (tz, -tx)
            m_s = s;
            m_lat = PlayerMath.ClampLateral(s, lat);
        }

        // ---------------------------------------------------------------- safety snap (The Ring)
        void OnTriggerEnter(Collider other)
        {
            if (other == null || other.gameObject.layer != PlayerTuning.SafetyLayer) return;
            m_snaps++;
            PathGuide.ReportKill();
            Debug.LogWarning($"[BW] safety volume '{other.name}' entered at s={m_s:F2}: snapping to the last safe lane point");
            SetState(m_lastSafeS, m_lastSafeLat);
        }

        // ---------------------------------------------------------------- QA helpers
        /// <summary>Capsule of the walker at a lane coordinate (0.30 to 1.90 m above the floor), for redundant checks.</summary>
        public static void Capsule(double s, double lateral, out Vector3 p0, out Vector3 p1, out float radius)
        {
            BwPath.World(s, lateral, out double x, out double y, out double z);
            radius = (float)FrameworkConst.PlayerRadius - 0.04f;
            var b = new Vector3((float)x, (float)y, (float)z);
            p0 = b + Vector3.up * (0.30f + radius);
            p1 = b + Vector3.up * (1.90f - radius);
        }

        public static bool CapsuleBlockedAt(double s, double lateral)
        {
            Capsule(s, lateral, out Vector3 p0, out Vector3 p1, out float r);
            return Physics.CheckCapsule(p0, p1, r, PlayerTuning.CorridorMask, QueryTriggerInteraction.Ignore);
        }
    }
}
