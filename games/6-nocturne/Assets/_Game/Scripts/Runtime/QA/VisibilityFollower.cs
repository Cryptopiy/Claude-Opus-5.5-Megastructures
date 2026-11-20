// VisibilityFollower: the camera pose the AutoWalker's cinematic follower really has at a progress s (MASTER_PROMPT 11.13, 14.5
// "Gaze"). It runs the RUNTIME WalkPlanner (the class AutoWalker drives in the player: GazeSchedule keys with smootherstep, the
// critically damped follower omega 2.5 rad/s, then the 22 / 18 deg/s rate limits; NocMath.CriticalDampAngle / LimitRate) in
// Cinematic mode at 60 fps, feeding it its own target each frame (the CharacterController reaches the target in a free lane), so
// nothing is mirrored. Two runs per key shot: the 11.13 run (start 40 m earlier at a constant 2.5 m/s with the follower at rest on
// the schedule pose there) and one whole cinematic walk from s 0 (the start ramp of 0.5 s, the first frame on the KS00 key) that
// is sampled at every key-shot s. The handheld sway (<= 0.3 deg, a time noise) is reported separately and left out of the lag.
// Also measures the readable window of each key shot (camera within 8 deg yaw and 6 deg pitch of the key pose, 14.5) on the walk.
using System;
using System.Collections.Generic;

namespace Megastructure
{
    public static class VisibilityFollower
    {
        public const double Fps = 60.0;
        public const double LeadIn = 40.0;                         // m (11.13)
        public const double ReadableYaw = 8.0, ReadablePitch = 6.0; // deg (14.5 "Readable")
        public const int Seed = 1;

        /// <summary>Follower pose (no sway) at s after a cinematic run from sFrom; sFrom &lt;= 0 starts the real walk from rest at s 0.</summary>
        public static void PoseAt(double sFrom, double s, out double yaw, out double pitch)
        {
            var targets = new[] { s };
            var y = new double[1];
            var p = new double[1];
            Run(sFrom, targets, y, p, null, null);
            yaw = y[0]; pitch = p[0];
        }

        /// <summary>
        /// Runs the runtime planner from sFrom to the largest of sTargets (ascending) and writes the follower yaw / pitch at each
        /// target (linear interpolation inside the crossing frame). Optional trace: every frame's (s, yaw, pitch).
        /// </summary>
        public static void Run(double sFrom, IList<double> sTargets, double[] yaw, double[] pitch, List<double> traceS, List<double> traceYaw, List<double> tracePitch = null)
        {
            var path = NocLanes.Path;
            var plan = new WalkPlanner(path, Seed);
            double s0 = Math.Max(0.0, sFrom);
            path.PointAt(s0, out double px, out double pz);
            path.TangentAt(s0, out double tx, out double tz);
            double v0 = sFrom > 0.0 ? NocturneConstants.CinematicSpeed : 0.0;
            GazeSchedule.Evaluate(s0, out double y0, out double p0);
            plan.Begin(WalkMode.Cinematic, px, pz, s0, tx * v0, tz * v0, NocMath.WrapDeg(y0), p0, NocturneConstants.CinematicSpeed);
            double dt = 1.0 / Fps;
            double prevS = plan.D, prevY = plan.FollowerYaw, prevP = plan.FollowerPitch;
            int next = 0;
            while (next < sTargets.Count && sTargets[next] <= prevS + 1e-9) { yaw[next] = prevY; pitch[next] = prevP; next++; }
            double t = 0.0;
            int guard = 0;
            while (next < sTargets.Count && guard++ < 2000000)
            {
                plan.Step(dt, plan.TargetX, plan.TargetZ, t);
                t += dt;
                double s = plan.D, yw = plan.FollowerYaw, pt = plan.FollowerPitch;
                if (traceS != null) { traceS.Add(s); traceYaw.Add(yw); tracePitch?.Add(pt); }
                while (next < sTargets.Count && sTargets[next] <= s + 1e-9)
                {
                    double k = s > prevS + 1e-12 ? NocMath.Clamp01((sTargets[next] - prevS) / (s - prevS)) : 1.0;
                    yaw[next] = prevY + NocMath.DeltaAngle(prevY, yw) * k;
                    pitch[next] = prevP + (pt - prevP) * k;
                    next++;
                }
                if (plan.Finished) break;
                prevS = s; prevY = yw; prevP = pt;
            }
            for (; next < sTargets.Count; next++) { yaw[next] = plan.FollowerYaw; pitch[next] = plan.FollowerPitch; }
        }

        /// <summary>Readable window of a pose on a traced walk: the contiguous s range around sKey where the follower stays within
        /// 8 deg yaw and 6 deg pitch of (yawKey, pitchKey). NaN when the follower is not readable at sKey itself.</summary>
        public static void ReadableWindow(List<double> traceS, List<double> traceYaw, List<double> tracePitch, double sKey, double yawKey, double pitchKey,
                                          out double sA, out double sB)
        {
            sA = sB = double.NaN;
            int n = traceS.Count;
            if (n == 0) return;
            int i0 = 0;
            while (i0 < n - 1 && traceS[i0] < sKey) i0++;
            bool Ok(int i) => Math.Abs(NocMath.DeltaAngle(yawKey, traceYaw[i])) <= ReadableYaw && Math.Abs(tracePitch[i] - pitchKey) <= ReadablePitch;
            if (!Ok(i0)) return;
            int a = i0, b = i0;
            while (a > 0 && Ok(a - 1)) a--;
            while (b < n - 1 && Ok(b + 1)) b++;
            sA = traceS[a]; sB = traceS[b];
        }
    }
}
