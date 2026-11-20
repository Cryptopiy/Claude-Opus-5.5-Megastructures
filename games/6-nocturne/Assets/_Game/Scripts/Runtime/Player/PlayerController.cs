using System;
using UnityEngine;

namespace Megastructure
{
    /// <summary>
    /// First-person movement on the flat walk plane (Part 9, 14.4), imported from The Ring's PlayerController and joined to
    /// Thread's services: implements <see cref="IPlayerState"/> (so Thread's AutoTestRunner / WallBot / SoakRunner / CaptureHarness
    /// work unchanged) on a CharacterController (r 0.35, h 1.8).
    /// Input through <see cref="NocInput"/> only (the single reader): WASD / arrows / left stick, Shift or a shoulder for brisk,
    /// C toggles the cinematic autowalk, Esc (or gamepad Start) raises NocEvents pause; the fullscreen keys belong to
    /// QualityBootstrap alone. Speeds 1.5 / 2.5 m/s with critically damped acceleration smoothing (0.2 s), gravity with ground
    /// snapping, no jump or crouch. Footsteps every PlayerMath.Stride(v) metres (alternating feet) raise NocEvents.Footstep and
    /// IAudioHooks.Footstep(surface(s), pan, gain). Progress s is the projection of the feet on the filleted centre line with a
    /// 25 m hint window (monotone across corners), kept in double.
    /// Goal (14.11): at s >= 1497 input and the autowalk are replaced by the stop ramp (T = 6 / v0 at the trigger), which lands on
    /// s 1500; NocEvents.GoalReached, IFadeUI.BeginEnding and IAudioHooks.BeginEnding fire once. When the AutoWalker drives, its
    /// planner runs the same ramp (it resolves the crossing exactly); manual walking uses this controller's ramp.
    /// Safety volume (14.4): y &lt; -3 or more than 1 m outside the lane union snaps back to the last safe point.
    /// In -autotest, -capture and -wallbot the user input is ignored entirely. Allocation-free per frame.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour, IPlayerState
    {
        public static PlayerController Instance { get; private set; }
        /// <summary>Number of safety-volume snaps (QA expects 0).</summary>
        public static int SafetySnaps { get; private set; }
        /// <summary>NocState.T when the goal trigger fired (NaN before).</summary>
        public static double TimeAtGoal { get; private set; } = double.NaN;

        // ---------------- numbers ----------------
        const float AccelOmega = (float)(4.74 / NocturneConstants.Accel);   // critically damped: 95 % of the target speed after 0.2 s
        const float Gravity = 9.81f;
        const float StickVel = 2f;
        const float MaxFall = 25f;
        const float SnapDist = 0.35f;
        const float Skin = PlayerTuning.ControllerSkin;   // = PlayerTuning.WallGap: the reachable centre is the 14.4 lane
        const float StepOffset = 0.3f;
        const float SlopeLimit = 45f;

        CharacterController _cc;
        bool _inputBlocked;
        Vector2 _vel;
        double _vvx, _vvy;   // velocity state of the critically damped acceleration (per axis)
        float _vy;
        double _strideAcc; float _stillT;
        int _foot;
        bool _movedRaised;
        Vector3 _extDelta; bool _extPending;
        Vector3 _lastSafe; double _lastSafeS; bool _hasSafe;
        bool _subscribed;

        double _s, _lat, _sPrev;
        // exact placement s (PlayerMath.AnchoredS): held until the feet leave the placement point
        double _anchorS; float _anchorX, _anchorZ; bool _anchored;
        float _speed;
        bool _finished, _frozen, _manualRamp;
        StopRamp _ramp; double _rampT; double _rampLat;
        float _stepPhase, _bobWeight;
        bool _grounded;

        // ---------------- IPlayerState ----------------
        public double S => _s;
        public double Lateral => _lat;
        public float Speed => _speed;
        public bool Brisk => AutoWalker.Mode == WalkMode.Cinematic || (!_inputBlocked && NocInput.Brisk());
        public bool Finished => _finished;
        public Vector3 EyePosition
        {
            get
            {
                double bob = _bobWeight > 0f ? PlayerMath.HeadBob(_stepPhase, _speed) * _bobWeight : 0.0;
                return transform.position + new Vector3(0f, (float)(NocturneConstants.EyeHeight + bob), 0f);
            }
        }
        public bool Frozen { get => _frozen; set => _frozen = value; }
        public bool Grounded => _grounded;

        public static void ResetStatics() { SafetySnaps = 0; TimeAtGoal = double.NaN; }

        void Awake()
        {
            Instance = this;
            ResetStatics();
            _cc = GetComponent<CharacterController>();
            _cc.radius = (float)NocturneConstants.PlayerRadius;
            _cc.height = (float)NocturneConstants.PlayerHeight;
            _cc.skinWidth = Skin;
            _cc.center = new Vector3(0f, 0.5f * (float)NocturneConstants.PlayerHeight + Skin, 0f);
            _cc.stepOffset = StepOffset;
            _cc.slopeLimit = SlopeLimit;
            _cc.minMoveDistance = 0f;
            NocRig.Player = transform;
            NocRig.Controller = _cc;
            NocServices.Player = this;
            _inputBlocked = CliArgs.AutoTest || CliArgs.Capturing || CliArgs.WallBot || CliArgs.SoakSeconds > 0;
            // start pose: -progress / -beat, on the centre line, facing the path heading
            double s0 = PlayerMath.ClampS(CliArgs.StartS);
            PlaceFeet(s0, 0.0);
            _s = s0; _sPrev = s0; _lat = 0.0;
            Anchor(s0);
        }

        /// <summary>A -beat N start sits exactly on the boundary (s0), not a float step short of it, until the feet move.</summary>
        void Anchor(double s)
        {
            Vector3 p = transform.position;
            _anchorS = s; _anchorX = p.x; _anchorZ = p.z; _anchored = true;
        }

        void Start()
        {
            NocEvents.PauseChanged += OnPauseChanged;
            NocEvents.LoadComplete += OnLoadComplete;
            _subscribed = true;
        }

        void OnDestroy()
        {
            if (_subscribed)
            {
                NocEvents.PauseChanged -= OnPauseChanged;
                NocEvents.LoadComplete -= OnLoadComplete;
            }
            if (ReferenceEquals(NocServices.Player, this)) NocServices.Player = null;
            if (Instance == this) Instance = null;
        }

        void OnLoadComplete()
        {
            if (CliArgs.Cinematic && !_inputBlocked && !NocState.Cinematic) SetCinematic(true);
        }

        void OnPauseChanged(bool paused)
        {
            NocState.Paused = paused;
            var a = NocServices.Audio;
            if (a != null) a.SetPauseDuck(paused);
        }

        /// <summary>Cinematic autowalk on / off (NocState.Cinematic + NocEvents.CinematicChanged, only on change).</summary>
        public static void SetCinematic(bool on)
        {
            if (NocState.Cinematic == on) return;
            NocState.Cinematic = on;
            NocEvents.RaiseCinematic(on);
            Debug.Log("[NOC] cinematic " + (on ? "on" : "off"));
        }

        /// <summary>Movement for this frame from the AutoWalker or a QA bot (world delta in metres, y ignored).</summary>
        public void DriveExternal(Vector3 horizontalDelta)
        {
            _extDelta = horizontalDelta;
            _extPending = true;
        }

        // ---------------- IPlayerState: teleport and drive ----------------
        /// <summary>Teleport to the corridor coordinate (s, lateral), facing the path heading (camera look keeps its pitch).</summary>
        public void Teleport(double s, double lateral)
        {
            s = PlayerMath.ClampS(s);
            lateral = PlayerMath.ClampLateral(s, lateral);
            PlaceFeet(s, lateral);
            _s = s; _sPrev = s; _lat = lateral;
            Anchor(s);
            _vel = Vector2.zero; _vvx = 0.0; _vvy = 0.0; _vy = 0f; _speed = 0f;
            NocRig.PlayerVelocity = Vector3.zero;
            var rig = CameraRig.Instance;
            if (rig != null) { rig.SetLook(NocLanes.Path.HeadingAt(s), rig.Pitch); rig.SnapEye(); }
            _lastSafe = transform.position; _lastSafeS = s; _hasSafe = true;
        }

        void PlaceFeet(double s, double lateral)
        {
            Vector3 feet = NocLanes.CorridorToWorld(s, lateral, 0.0);
            bool en = _cc != null && _cc.enabled;
            if (_cc != null) _cc.enabled = false;
            transform.SetPositionAndRotation(feet, Quaternion.identity);
            if (_cc != null) _cc.enabled = en;
            Physics.SyncTransforms();
        }

        /// <summary>Thread QA drive (AutoTestRunner, WallBot, SoakRunner): constant plan speed and a lateral target through the
        /// AutoWalker (Part 9: the same walker in tests). lateralTarget: NaN = centre line; +-1000 = right / left lane edge
        /// 0.40 m inside (wall bot); otherwise a fixed lateral offset.</summary>
        public void SetDrive(bool active, float speed, float lateralTarget)
        {
            if (!active) { AutoWalker.Request(WalkMode.None); return; }
            int edge = 0; double lat = lateralTarget;
            if (!float.IsNaN(lateralTarget) && Math.Abs(lateralTarget) >= 999f) { edge = lateralTarget > 0 ? 1 : -1; lat = 0.0; }
            AutoWalker.BeginQa(speed, CliArgs.Seed, double.NaN, lat, edge);
        }

        /// <summary>The player's capsule (bottom and top sphere centres, radius) at a corridor coordinate (PathGuide / WallBot probes).</summary>
        public static void Capsule(double s, double lateral, out Vector3 p0, out Vector3 p1, out float r)
        {
            Vector3 feet = NocLanes.CorridorToWorld(s, lateral, 0.0);
            r = (float)NocturneConstants.PlayerRadius;
            p0 = feet + Vector3.up * (r + Skin + 0.02f);
            p1 = feet + Vector3.up * ((float)NocturneConstants.PlayerHeight + Skin - r);
        }

        // ---------------- frame ----------------
        void Update()
        {
            if (NocRig.Player == null) { NocRig.Player = transform; NocRig.Controller = _cc; }
            if (!NocRig.WorldReady) { _extPending = false; return; }
            float dt = Mathf.Min(Time.deltaTime, (float)PlayerTuning.MaxStepDt);
            if (dt <= 0f) return;
            SystemInput();
            if (NocState.Paused || _frozen)
            {
                _vel = Vector2.zero; _vvx = 0.0; _vvy = 0.0; _extPending = false;
                NocRig.PlayerVelocity = Vector3.zero;
                return;
            }

            Vector3 delta;
            bool manual = false;
            if (_finished && _manualRamp)
            {
                // manual walking reached the trigger: the stop ramp drives along the path at the trigger's lateral offset
                _rampT += dt;
                double sT = _ramp.PositionAt(_rampT);
                Vector3 target = NocLanes.CorridorToWorld(sT, _rampLat, 0.0);
                delta = target - transform.position; delta.y = 0f;
                _extPending = false;
                _vel = Vector2.zero; _vvx = 0.0; _vvy = 0.0;
            }
            else if (_extPending)
            {
                delta = _extDelta; delta.y = 0f;
                _extPending = false;
                _vel = new Vector2(delta.x, delta.z) / dt;   // continuity when the user takes over
                _vvx = 0.0; _vvy = 0.0;
            }
            else
            {
                Vector2 target = Vector2.zero;
                if (!_inputBlocked && !AutoWalker.Active && !_finished)
                {
                    Vector2 mv = NocInput.Move();
                    if (mv.sqrMagnitude > 0.01f)
                    {
                        manual = true;
                        if (!_movedRaised) { _movedRaised = true; NocEvents.RaisePlayerMoved(); var f = NocServices.Fade; if (f != null) f.HideHint(); }
                    }
                    float speed = (float)(NocInput.Brisk() ? NocturneConstants.BriskSpeed : NocturneConstants.WalkSpeed);
                    float yaw = NocRig.Yaw * Mathf.Deg2Rad;
                    Vector2 fwd = new Vector2(Mathf.Sin(yaw), Mathf.Cos(yaw));
                    Vector2 right = new Vector2(Mathf.Cos(yaw), -Mathf.Sin(yaw));
                    target = (fwd * mv.y + right * mv.x) * speed;
                }
                _vel.x = (float)NocMath.CriticalDamp(_vel.x, ref _vvx, target.x, AccelOmega, dt);
                _vel.y = (float)NocMath.CriticalDamp(_vel.y, ref _vvy, target.y, AccelOmega, dt);
                if (target.sqrMagnitude < 1e-8f && _vel.sqrMagnitude < 1e-6f) { _vel = Vector2.zero; _vvx = 0.0; _vvy = 0.0; }
                delta = new Vector3(_vel.x * dt, 0f, _vel.y * dt);
            }

            // gravity + ground snapping
            bool wasGrounded = _cc.isGrounded;
            if (wasGrounded && _vy <= 0f) _vy = -StickVel;
            else _vy = Mathf.Max(_vy - Gravity * dt, -MaxFall);
            delta.y = _vy * dt;
            Vector3 before = transform.position;
            _cc.Move(delta);
            if (wasGrounded && !_cc.isGrounded && _vy <= 0f) SnapDown();
            _grounded = _cc.isGrounded;
            Vector3 after = transform.position;
            Vector3 hv = (after - before) / dt; hv.y = 0f;
            NocRig.PlayerVelocity = hv;

            UpdateProgress(dt);
            Footsteps(hv, dt, manual);
            Safety(after);
            Goal();
        }

        void UpdateProgress(float dt)
        {
            Vector3 p = transform.position;
            _sPrev = _s;
            double s = NocLanes.Path.Project(p.x, p.z, _s, 25.0, out double dist);
            if (dist > 15.0) s = NocLanes.Path.Project(p.x, p.z, _s, -1.0, out dist);
            if (_anchored)
            {
                double dx = p.x - _anchorX, dz = p.z - _anchorZ;
                double dxz = Math.Sqrt(dx * dx + dz * dz);
                s = PlayerMath.AnchoredS(s, true, _anchorS, dxz);
                if (dxz >= PlayerMath.AnchorRadius) _anchored = false;
            }
            _s = s;
            _lat = NocLanes.Path.Lateral(p.x, p.z, s);
            _speed = (float)((_s - _sPrev) / Math.Max(1e-4, dt));
        }

        void Goal()
        {
            if (_finished || _s < NocturneConstants.GoalTriggerS) return;
            _finished = true;
            NocState.GoalReached = true;
            TimeAtGoal = NocState.T;
            if (!AutoWalker.Active)
            {
                // manual: ramp from the exact trigger point with the current speed (T = 6 / v0)
                _manualRamp = true;
                _ramp = StopRamp.Begin(NocturneConstants.GoalTriggerS, Math.Max(0.0, _speed));
                _rampT = _speed > 1e-3 ? (_s - NocturneConstants.GoalTriggerS) / _speed : 0.0;
                _rampLat = _lat;
            }
            Debug.Log(FormattableString.Invariant($"[NOC] goal trigger at s={_s:F3} t={NocState.T:F2} speed={_speed:F2}"));
            NocEvents.RaiseGoal();
            var f = NocServices.Fade; if (f != null) f.BeginEnding();
            var a = NocServices.Audio; if (a != null) a.BeginEnding();
        }

        void SnapDown()
        {
            Vector3 o = transform.position + Vector3.up * 0.3f;
            if (Physics.Raycast(o, Vector3.down, out RaycastHit hit, 0.3f + SnapDist, PlayerTuning.CorridorMask, QueryTriggerInteraction.Ignore))
            {
                float drop = hit.distance - 0.3f;
                if (drop > 0f) _cc.Move(Vector3.down * (drop + 0.01f));
            }
        }

        void Footsteps(Vector3 hv, float dt, bool manual)
        {
            double speed = hv.magnitude;
            double stride = PlayerMath.Stride(speed);
            if (_grounded && speed > 0.05)
            {
                _stillT = 0f;
                _strideAcc += speed * dt;
                if (_strideAcc >= stride)
                {
                    _strideAcc -= stride;
                    if (_strideAcc > stride) _strideAcc = 0.0;
                    FootSurface surf = NocEvents.SurfaceAt(_s);
                    NocEvents.RaiseFootstep(surf, _foot, (float)speed);
                    var a = NocServices.Audio;
                    if (a != null) a.Footstep(surf, _foot == 0 ? -PlayerTuning.FootstepPan : PlayerTuning.FootstepPan, PlayerMath.FootstepGain(speed));
                    _foot ^= 1;
                }
            }
            else
            {
                _stillT += dt;
                if (_stillT > 0.4f) _strideAcc = Math.Min(_strideAcc, 0.5 * stride);   // first step comes soon after a stop
            }
            _stepPhase = (float)NocMath.Clamp01(_strideAcc / stride);
            float bobTarget = (manual && _grounded && !AutoWalker.Active && !_inputBlocked && !NocState.Capture) ? Mathf.Clamp01((float)(speed / NocturneConstants.WalkSpeed)) : 0f;
            _bobWeight += (bobTarget - _bobWeight) * (1f - Mathf.Exp(-dt / 0.15f));
        }

        void Safety(Vector3 pos)
        {
            double outside = NocLanes.DistanceOutsideLanes(pos, _s);
            if (pos.y < NocturneConstants.SafetyY || outside > NocturneConstants.SafetyOutside)
            {
                SafetySnaps++;
                double s = _s;
                Vector3 target = _hasSafe && Math.Abs(_lastSafeS - s) < 6.0 ? _lastSafe : NocLanes.CentreAt(s);
                if (SafetySnaps <= 5) Debug.LogWarning(FormattableString.Invariant($"[NOC] safety snap {SafetySnaps} at s {s:F1} (y {pos.y:F2}, outside {outside:F2} m)"));
                bool en = _cc.enabled;
                _cc.enabled = false;
                transform.position = target + Vector3.up * 0.02f;
                _cc.enabled = en;
                Physics.SyncTransforms();
                _vel = Vector2.zero; _vvx = 0.0; _vvy = 0.0; _vy = 0f;
                UpdateProgress(Time.deltaTime);
                return;
            }
            if (_grounded && outside <= 0.0 && pos.y > -0.5f) { _lastSafe = pos; _lastSafeS = _s; _hasSafe = true; }
        }

        // ---------------- system keys (pause, cinematic; fullscreen is QualityBootstrap's) ----------------
        void SystemInput()
        {
            if (_inputBlocked) return;
            if (NocInput.PausePressed()) NocEvents.RaisePause(!NocState.Paused);
            if (NocState.Paused || _finished) return;
            if (NocInput.CinematicToggle()) { SetCinematic(!NocState.Cinematic); return; }
            if (NocState.Cinematic && NocInput.AnyCancelInput(PlayerTuning.LookThresholdCounts, PlayerTuning.StickThreshold)) SetCinematic(false);
        }
    }
}
