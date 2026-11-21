// WalkPlanner: pure walk logic of the AutoWalker (no Unity objects, allocation-free; EditMode-testable). Imported from The
// Ring's WalkPlanner and trimmed per D-02 / Part 9 / 3.1 (a)-(f):
//   * constant speed: v_cmd = cruise (2.5 m/s cinematic = the Shift speed; 1.5 m/s for the 1.5 m/s autotest) after a 0.5 s
//     smootherstep start ramp from the walker's current progress speed v0 (a ramp from rest loses exactly 0.25 s);
//     NO braking, holds, glances, releases, zones, dwell, catch-up or MaxLag wait; the planner does not integrate s itself
//     (PlayerController integrates ds/dt = v_cmd exactly and stops with its StopRamp at the goal);
//   * lateral target = BwPath.CinematicLateral(s) + the starting offset decayed over DISTANCE (e-folding 4 m), so the path
//     is identical at any speed; the player follows it exactly (feed-forward);
//   * look: GazeSchedule evaluated at s + lead, lead = (2 / omega) v; critically damped follower omega 2.5 rad/s (itself
//     rate-limited, so its velocity state stays consistent); seeded handheld sway of at most 0.2 deg per axis (0.28 deg
//     combined, <= 0.3) faded in over the start ramp; then the 18 / 13 deg/s limits act on the FINAL look (follower + sway),
//     so the camera never turns faster than the limits, and the shown sway (output - follower) stays inside the sway bound;
//     never roll. ForwardGaze (optional, e.g. -gaze forward) looks along the heading at pitch 0 instead.
// Deleted from The Ring: Phase.Brake/Hold, StartBrake, NextStopD, BeginHold, EndHold, BeginRelease, BrakeTime, the glance and
// release state, _evalFloorS, MaxLag and the own integration of d; CineAccelTime 1.5 -> 0.5; LookOmega 3 -> 2.5.
using System;

namespace Megastructure.Player
{
    public sealed class WalkPlanner
    {
        public const double LookOmega = FrameworkConst.GazeOmega;
        public const double RampTime = FrameworkConst.StartRamp;
        public const double MaxYawRate = FrameworkConst.GazeMaxYawRate;
        public const double MaxPitchRate = FrameworkConst.GazeMaxPitchRate;
        public const double SwayMax = FrameworkConst.GazeSwayMaxDeg;

        public double Cruise { get; private set; }
        public double V0 { get; private set; }
        /// <summary>Seconds since Begin (the planner's own clock: the sum of the dt it was stepped with).</summary>
        public double Elapsed { get; private set; }
        /// <summary>Commanded progress speed this step (m/s).</summary>
        public double SpeedCommand { get; private set; }
        /// <summary>Lateral target (lane coordinates, m) at the progress passed to the last Step.</summary>
        public double LateralTarget { get; private set; }
        /// <summary>Final look this step (world yaw, pitch; deg): follower + sway, rate-limited to 18 / 13 deg/s.</summary>
        public double OutYaw { get; private set; }
        public double OutPitch { get; private set; }
        /// <summary>The critically damped follower before the sway (QA: OutYaw - FollowYaw is the shown sway).</summary>
        public double FollowYaw => m_yaw;
        public double FollowPitch => m_pitch;
        public bool ForwardGaze { get; set; }

        double m_s0, m_lat0, m_rampAt;
        double m_yaw, m_yawV, m_pitch, m_pitchV;
        readonly float m_seedX, m_seedY;

        public WalkPlanner(int seed)
        {
            uint h = BwMath.Pcg((uint)seed);
            m_seedX = 17.3f + 97f * BwMath.U01(h);
            m_seedY = 51.9f + 97f * BwMath.U01(BwMath.Pcg(h + 1u));
        }

        /// <summary>Start ramp v(t) = v0 + (cruise - v0) smootherstep(t / 0.5).</summary>
        public static double RampSpeed(double v0, double cruise, double t) => v0 + (cruise - v0) * BwMath.Smootherstep01(t / RampTime);

        /// <summary>Distance covered by the ramp after t: closed form of the integral (x^6 - 3x^5 + 2.5x^4 for smootherstep).</summary>
        public static double RampDistance(double v0, double cruise, double t)
        {
            double x = Math.Max(0.0, Math.Min(1.0, t / RampTime));
            double x4 = x * x * x * x;
            double ss = x4 * x * x - 3.0 * x4 * x + 2.5 * x4;
            double d = RampTime * (v0 * x + (cruise - v0) * ss);
            if (t > RampTime) d += cruise * (t - RampTime);
            return d;
        }

        /// <summary>
        /// Starts a walk at progress s with the walker's current progress speed v0, its current lateral offset and look.
        /// </summary>
        public void Begin(double s, double v0, double lateral, double yaw, double pitch, double cruise)
        {
            Cruise = Math.Max(0.1, cruise);
            V0 = Math.Max(0.0, Math.Min(v0, Cruise));
            Elapsed = 0;
            m_rampAt = 0;
            m_s0 = s;
            m_lat0 = lateral - BwPath.CinematicLateral(s);
            m_yaw = yaw; m_pitch = pitch; m_yawV = 0; m_pitchV = 0;
            OutYaw = yaw; OutPitch = pitch;
            SpeedCommand = V0;
            LateralTarget = lateral;
        }

        /// <summary>A new start ramp from rest (the platform walk after the ride) without touching the look state.</summary>
        public void RestartRamp(double s, double v0)
        {
            V0 = Math.Max(0.0, Math.Min(v0, Cruise));
            m_rampAt = Elapsed;
            m_s0 = s;
            m_lat0 = 0;
            SpeedCommand = V0;
        }

        /// <summary>One frame: dt, the walker's progress s now, ambient time t (sway).</summary>
        public void Step(double dt, double s, double t)
        {
            if (dt <= 0) return;
            Elapsed += dt;
            SpeedCommand = RampSpeed(V0, Cruise, Elapsed - m_rampAt);
            double decay = Math.Exp(-Math.Max(0.0, s - m_s0) / FrameworkConst.LateralDecayMetres);
            LateralTarget = BwPath.CinematicLateral(s) + m_lat0 * decay;
            Look(dt, s, t);
        }

        void Look(double dt, double s, double t)
        {
            double ty, tp;
            if (ForwardGaze)
            {
                ty = BwPath.HeadingDeg(s); tp = 0.0;
            }
            else
            {
                // in the ride s advances at RideSRate whatever the walking speed (EndingPath), so the lead uses that rate there
                double v = EndingPath.InRide(s) || (s >= BwPath.Length && s < EndingPath.SFoot0) ? BulwarkConst.RideSRate : SpeedCommand;
                double lead = 2.0 / LookOmega * v;
                double sl = s + lead;
                if (s <= BwPath.Length && sl > BwPath.Length) sl = BwPath.Length;   // the walked path keeps its last key until the ride starts
                GazeSchedule.Evaluate(Math.Min(EndingPath.EndS, sl), out ty, out tp);
            }
            double y0 = m_yaw, p0 = m_pitch;
            m_yaw = BwMath.CriticalDampAngle(m_yaw, ref m_yawV, ty, LookOmega, dt);
            m_pitch = BwMath.CriticalDamp(m_pitch, ref m_pitchV, tp, LookOmega, dt);
            LimitRate(ref m_yaw, ref m_yawV, y0, MaxYawRate, dt);
            LimitRate(ref m_pitch, ref m_pitchV, p0, MaxPitchRate, dt);
            if (m_yaw > 360.0 || m_yaw < -360.0) m_yaw = BwMath.WrapDeg(m_yaw);

            // deterministic handheld sway: 3 octaves of value noise of the ambient time, seeded; |Octaves| <= 1, so each axis
            // is within SwayAxis (0.2 deg) and the combined offset within 0.283 deg (the clamp keeps any change <= 0.3)
            float tt = (float)(t % 3600.0);
            double sx = Octaves(tt, m_seedX) * SwayAxis, sy = Octaves(tt, m_seedY) * SwayAxis;
            double m = Math.Sqrt(sx * sx + sy * sy);
            if (m > SwayMax) { sx *= SwayMax / m; sy *= SwayMax / m; }
            double fadeIn = BwMath.Smoothstep(0.0, RampTime, Elapsed);

            // Final rate limit AFTER the sway: the camera shows follower + sway, and that is what must stay within 18 / 13 deg/s
            // (T-NUM 14.10.1). The follower is already limited, so the output trails follower + sway by at most the sway
            // itself: each step moves toward the target, and the target never leaves the +-SwayAxis band round the follower.
            double wantYaw = m_yaw + sx * fadeIn, wantPitch = m_pitch + sy * fadeIn;
            double dy = Clamp(BwMath.DeltaDeg(OutYaw, wantYaw), MaxYawRate * dt);
            double dp = Clamp(wantPitch - OutPitch, MaxPitchRate * dt);
            OutYaw = BwMath.WrapDeg(OutYaw + dy);
            OutPitch += dp;
        }

        /// <summary>Per-axis sway amplitude (deg): combined at most SwayAxis x sqrt 2 = 0.283 deg (Part 9: at most 0.3).</summary>
        public const double SwayAxis = 0.2;

        static double Clamp(double x, double lim) => x > lim ? lim : (x < -lim ? -lim : x);

        static void LimitRate(ref double x, ref double v, double x0, double maxRate, double dt)
        {
            double step = x - x0, lim = maxRate * dt;
            if (step > lim) { x = x0 + lim; v = maxRate; }
            else if (step < -lim) { x = x0 - lim; v = -maxRate; }
        }

        static double Octaves(float t, float seed)
        {
            // smooth random in [-1, 1]: about 0.35, 0.8 and 1.7 Hz, weights 1, 0.5, 0.25
            double n = (BwMath.ValueNoise(t * 0.35f, seed) * 2f - 1f)
                     + 0.5 * (BwMath.ValueNoise(t * 0.8f, seed + 31.7f) * 2f - 1f)
                     + 0.25 * (BwMath.ValueNoise(t * 1.7f, seed + 63.1f) * 2f - 1f);
            return n / 1.75;
        }
    }
}
