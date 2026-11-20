using System;

namespace Megastructure
{
    /// <summary>Walk modes of the AutoWalker.</summary>
    public enum WalkMode { None = 0, AutoTest = 1, Cinematic = 2 }

    /// <summary>
    /// Pure walk logic of the AutoWalker (no Unity objects, allocation-free; usable from EditMode tests), imported from The
    /// Ring's WalkPlanner with the cinematic edits (a)-(f) of Part 3.1 / IMPORT_PLAN 4.4:
    /// (a) constant cruise speed (cinematic 2.5 m/s, QA any speed), a smootherstep start ramp (0.5 s cinematic, 0.2 s QA,
    ///     time lost = T / 2), phases Accel / Cruise / Stop / Done only: no holds, no braking, no glances;
    ///     at the goal trigger (s 1497) the 14.11 stop ramp v(t) = v0 (1 - smoothstep(0, T, t)), T = 6 / v0, takes over and
    ///     lands exactly on s 1500 (the trigger crossing inside a frame is resolved exactly, so T = 6 / v0 holds);
    /// (b) gaze keyed to s in metres (GazeSchedule), critically damped follower omega 2.5 rad/s, then yaw &lt;= 22 deg/s and
    ///     pitch &lt;= 18 deg/s (Thread's LimitRate);
    /// (c) the handheld sway (time, &lt;= 0.3 degrees radius, value noise) and the start lateral-residual decay are the only
    ///     time couplings;
    /// (e) no brisk flag is derived from the speed here; (f) no lateral offset (GazeSchedule.LateralOffset = 0).
    /// All distance / speed state is double (The Ring's float integrator drifted ~0.5 m over 1 500 m, IMPORT_PLAN 3.4).
    /// s is the filleted arc length (14.4), so the planner's distance d is the progress s.
    /// The owner feeds the player's position each step and moves the player to (TargetX, TargetZ); the planner waits when the
    /// player lags the target by more than MaxLag measured along the tangent (a wall contact never stalls it sideways).
    /// </summary>
    public sealed class WalkPlanner
    {
        public readonly FilletPath Path;

        public const double QaAccelTime = NocturneConstants.Accel;            // 0.2 s
        public const double CineAccelTime = NocturneConstants.CineAccelTime;  // 0.5 s (was 1.5 in The Ring)
        public const double LookOmega = NocturneConstants.LookOmega;          // 2.5 rad/s (was 3)
        public const double YawRateMax = NocturneConstants.YawRateMax;        // 22 deg/s
        public const double PitchRateMax = NocturneConstants.PitchRateMax;    // 18 deg/s
        public const double SwayMax = NocturneConstants.SwayMax;              // 0.3 deg
        const double SwayAmp = 0.2;                                           // per axis before the 0.3 clamp
        public const double MaxLag = 0.6;
        const double LatDecay = 1.5;                                          // s, decay of the starting lateral offset

        enum Phase { Accel, Cruise, Stop, Done }

        public WalkMode Mode { get; private set; }
        /// <summary>Progress s = path distance (m) and plan speed ds/dt (m/s).</summary>
        public double D => _d;
        public double Speed => _v;
        /// <summary>Where the player's feet should be this frame (x, z).</summary>
        public double TargetX { get; private set; }
        public double TargetZ { get; private set; }
        /// <summary>Camera look output (degrees; yaw world, pitch up positive).</summary>
        public double OutYaw { get; private set; }
        public double OutPitch { get; private set; }
        /// <summary>The goal trigger (s 1497) fired and the stop ramp runs (or has finished).</summary>
        public bool Triggered => _phase == Phase.Stop || _phase == Phase.Done && _rampStarted;
        public bool Finished => _phase == Phase.Done;
        /// <summary>Walk time since Begin (s) and the walk time at which the trigger was crossed (NaN before).</summary>
        public double WalkTime => _walkT;
        public double TriggerTime { get; private set; } = double.NaN;
        public double StopTime { get; private set; } = double.NaN;
        /// <summary>The stop ramp (valid once Triggered).</summary>
        public StopRamp Ramp => _ramp;
        /// <summary>Lateral target in QA mode (m, right of travel): fixed value, or an edge of the lane with an inset (wall bot).</summary>
        public double LateralFixed;
        public int LateralEdgeSide;     // 0 = use LateralFixed, -1 left edge, +1 right edge
        public double LateralInset = 0.40;

        Phase _phase = Phase.Done;
        double _d, _v, _vCruise;
        double _tau, _phaseT, _dStart, _vStart;
        double _walkT;
        double _latResid;
        StopRamp _ramp; double _rampT; bool _rampStarted;
        double _yaw, _yawV, _pitch, _pitchV;
        readonly double _seedX, _seedY;

        public WalkPlanner(FilletPath path, int seed)
        {
            Path = path;
            uint h = NocMath.Pcg((uint)seed);
            _seedX = 17.3 + 97.0 * NocMath.U01(h);
            _seedY = 51.9 + 97.0 * NocMath.U01(NocMath.Pcg(h + 1u));
        }

        /// <summary>Starts a walk from the player's position (x, z), progress hint, velocity and look.</summary>
        public void Begin(WalkMode mode, double px, double pz, double hintS, double vx, double vz, double yaw, double pitch, double cruise)
        {
            Mode = mode;
            _d = Path.Project(px, pz, hintS, 25.0, out double dist);
            if (dist > 10.0) _d = Path.Project(px, pz, 0.0, -1.0, out dist);
            _latResid = Path.Lateral(px, pz, _d) - LateralTarget(_d);
            _vCruise = NocMath.Clamp(cruise, 0.1, 5.0);
            Path.TangentAt(_d, out double tx, out double tz);
            double v0 = NocMath.Clamp(vx * tx + vz * tz, 0.0, _vCruise);
            _walkT = 0.0;
            TriggerTime = double.NaN; StopTime = double.NaN; _rampStarted = false;
            if (_d >= NocturneConstants.GoalTriggerS) { StartRamp(Math.Max(v0, 0.0), _d, 0.0); }
            else StartAccel(v0, mode == WalkMode.Cinematic ? CineAccelTime : QaAccelTime);
            _yaw = yaw; _pitch = pitch; _yawV = 0.0; _pitchV = 0.0;
            OutYaw = yaw; OutPitch = pitch;
            UpdateTarget();
        }

        public void Stop() { Mode = WalkMode.None; _phase = Phase.Done; }

        double LateralTarget(double d)
        {
            if (Mode == WalkMode.Cinematic) return GazeSchedule.LateralOffset(d);
            if (LateralEdgeSide != 0) return LateralEdgeSide * Math.Max(0.0, NocLanes.LaneHalfWidth(d) - LateralInset);
            double h = NocLanes.LaneHalfWidth(d);
            return NocMath.Clamp(LateralFixed, -h, h);
        }

        /// <summary>One frame. (px, pz) = where the player is now; t = scene time (sway).</summary>
        public void Step(double dt, double px, double pz, double t)
        {
            if (Mode == WalkMode.None || dt <= 0.0) return;
            Path.TangentAt(_d, out double tx, out double tz);
            double lag = (TargetX - px) * tx + (TargetZ - pz) * tz;   // along the tangent only (IMPORT_PLAN 4.6)
            bool blocked = lag > MaxLag && _phase != Phase.Stop;
            if (!blocked) Advance(dt);
            if (_latResid != 0.0 && _v > 0.01) _latResid *= Math.Exp(-dt / LatDecay);
            UpdateTarget();
            Look(dt, t);
        }

        void UpdateTarget()
        {
            double lat = LateralTarget(_d) + _latResid;
            Path.CorridorPoint(_d, lat, out double x, out double z);
            TargetX = x; TargetZ = z;
        }

        // ---------------- speed profile ----------------
        /// <summary>Integral of smootherstep from 0 to x (x in 0..1): x^6 - 3x^5 + 2.5x^4.</summary>
        public static double SsInt(double x) { double x2 = x * x, x4 = x2 * x2; return x4 * x2 - 3.0 * x4 * x + 2.5 * x4; }

        void StartAccel(double vStart, double time)
        {
            _phase = Phase.Accel; _tau = 0.0; _phaseT = Math.Max(0.05, time); _dStart = _d; _vStart = vStart; _v = vStart;
        }

        void StartRamp(double v0, double s0, double elapsed)
        {
            _phase = Phase.Stop; _rampStarted = true;
            _ramp = StopRamp.Begin(s0, v0);
            _rampT = elapsed;
            TriggerTime = _walkT - elapsed;
            _d = _ramp.PositionAt(_rampT);
            _v = _ramp.SpeedAt(_rampT);
            if (_ramp.Done(_rampT)) { _phase = Phase.Done; _v = 0.0; StopTime = TriggerTime + _ramp.T; }
        }

        void Advance(double dt)
        {
            double dPrev = _d, vPrev = _v;
            _walkT += dt;
            switch (_phase)
            {
                case Phase.Accel:
                {
                    _tau += dt;
                    double x = Math.Min(1.0, _tau / _phaseT);
                    _d = _dStart + _phaseT * (_vStart * x + (_vCruise - _vStart) * SsInt(x));
                    _v = _vStart + (_vCruise - _vStart) * NocMath.Smootherstep01(x);
                    if (x >= 1.0)
                    {
                        // carry the remainder of this frame at cruise speed (exact integral of the profile)
                        double extra = _tau - _phaseT;
                        _d += _vCruise * extra;
                        _v = _vCruise;
                        _phase = Phase.Cruise;
                    }
                    break;
                }
                case Phase.Cruise:
                    _v = _vCruise;
                    _d += _v * dt;
                    break;
                case Phase.Stop:
                    _rampT += dt;
                    _d = _ramp.PositionAt(_rampT);
                    _v = _ramp.SpeedAt(_rampT);
                    if (_ramp.Done(_rampT)) { _d = Math.Max(_d, NocturneConstants.GoalS); _v = 0.0; _phase = Phase.Done; StopTime = TriggerTime + _ramp.T; }
                    return;
                default:
                    _v = 0.0;
                    return;
            }
            // goal trigger: resolve the crossing of s 1497 inside this frame exactly (the speed is constant across it in
            // cruise; in the accel phase the linear estimate is within a frame of the exact crossing)
            double trig = NocturneConstants.GoalTriggerS;
            if (_d >= trig)
            {
                double vAt = _v;
                double over = _d - trig;
                double elapsed = vAt > 1e-6 ? over / vAt : 0.0;
                if (elapsed > dt) elapsed = dt;
                StartRamp(vAt, trig, elapsed);
            }
        }

        // ---------------- gaze ----------------
        void Look(double dt, double t)
        {
            double s = _d;
            double heading = Path.HeadingDeg(Math.Min(s, Path.Length));
            double ty, tp;
            if (Mode == WalkMode.AutoTest)
            {
                ty = heading; tp = NocturneConstants.GazeDefaultPitch;   // QA: forward along the path
            }
            else GazeSchedule.Evaluate(s, heading, out ty, out tp);

            double y0 = _yaw, p0 = _pitch;
            _yaw = NocMath.CriticalDampAngle(_yaw, ref _yawV, ty, LookOmega, dt);
            _pitch = NocMath.CriticalDamp(_pitch, ref _pitchV, tp, LookOmega, dt);
            NocMath.LimitRate(ref _yaw, ref _yawV, y0, YawRateMax, dt);
            NocMath.LimitRate(ref _pitch, ref _pitchV, p0, PitchRateMax, dt);
            if (_yaw > 360.0 || _yaw < -360.0) { double w = NocMath.WrapDeg(_yaw); _yaw = w; }

            double oy = _yaw, op = _pitch;
            if (Mode == WalkMode.Cinematic)
            {
                // deterministic handheld sway: 3 octaves of value noise of the scene time, seeded, clamped to a 0.3 deg radius
                double tt = t % 3600.0;
                double sx = Octaves(tt, _seedX) * SwayAmp, sy = Octaves(tt, _seedY) * SwayAmp;
                double r2 = sx * sx + sy * sy;
                if (r2 > SwayMax * SwayMax) { double k = SwayMax / Math.Sqrt(r2); sx *= k; sy *= k; }
                oy += sx; op += sy;
            }
            OutYaw = oy; OutPitch = op;
        }

        /// <summary>The follower state without the sway (tests: rate limits).</summary>
        public double FollowerYaw => _yaw;
        public double FollowerPitch => _pitch;

        static double Octaves(double t, double seed)
        {
            // smooth random in [-1, 1]: about 0.35, 0.8 and 1.7 Hz, weights 1, 0.5, 0.25
            double n = (NocMath.ValueNoise(t * 0.35, seed) * 2.0 - 1.0)
                     + 0.5 * (NocMath.ValueNoise(t * 0.8, seed + 31.7) * 2.0 - 1.0)
                     + 0.25 * (NocMath.ValueNoise(t * 1.7, seed + 63.1) * 2.0 - 1.0);
            return n / 1.75;
        }
    }
}
