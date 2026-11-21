// WalkerTests: the cinematic walker contract of D-02 / IMPORT_MANIFEST B 5.7 (MASTER_PROMPT Part 9, 3.1 a-f, 14.10.4) on the
// pure parts (WalkPlanner, GazeSchedule, StopRamp, the offline motion model, the CINE = BRISK alias, the computed trigger
// times, the QA frame policy). The player-side checks (progress speed 2.500 from t 0.5 s, trigger times, roll <= 0.01 deg) are
// asserted again by AutoTestRunner in the built player.
using System;
using Megastructure.Audio;
using Megastructure.Player;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class WalkerTests
    {
        [Test]
        public void Ramp_LosesExactlyAQuarterSecond()
        {
            const double v = 2.5;
            Assert.AreEqual(v * (10.0 - 0.25), WalkPlanner.RampDistance(0.0, v, 10.0), 1e-9);
            Assert.AreEqual(v, WalkPlanner.RampSpeed(0.0, v, 0.5), 1e-12, "full speed after 0.5 s");
            Assert.AreEqual(0.0, WalkPlanner.RampSpeed(0.0, v, 0.0), 1e-12);
        }

        [Test]
        public void TriggerTimes_Of14_10_4([Values(1.5, 2.5)] double v)
        {
            // the audio render's model ends at the BOARD trigger 1498.5 (the walked path; ADDENDUM A1 00 moved the goal past the ride)
            var st = new OfflineRenderSettings { Speed = v };
            double t = OfflineAudioRenderer.TriggerTime(st);
            if (v > 2.0) Assert.AreEqual(599.65, t, 1e-6);
            else Assert.AreEqual(999.25, t, 1e-6, "0.5 s ramp at 1.5 m/s; the spec's 999.15 +- 4 s holds");
            // the lantern goal: walk + board ramp + the fixed 143 s ride + the platform walk with its own 0.5 s ramp
            double expect = 1540.0 / v + 143.5;
            Assert.AreEqual(expect, FrameworkConst.TriggerTimeAt(v), 1e-3, "1540 / v + 143.5 s");
        }

        [Test]
        public void StopRamp_RestsAtS1500()
        {
            var r = StopRamp.Begin(FrameworkConst.GoalTriggerS, 2.5);
            Assert.AreEqual(1.2, r.T, 1e-9, "T = 2 x 1.5 / 2.5");
            Assert.AreEqual(FrameworkConst.GoalRestS, r.PositionAt(r.T), 1e-9);
            double prev = FrameworkConst.GoalTriggerS;
            for (double t = 0; t <= r.T; t += 0.01) { double s = r.PositionAt(t); Assert.GreaterOrEqual(s, prev - 1e-12); prev = s; }
        }

        /// <summary>T-NUM (14.10.1; fix wave S2-6 / R4): the key schedules THEMSELVES (not only the rate-limited follower) never ask
        /// for more than 18 / 13 deg/s: the walk keys at 2.5 m/s, the ride keys at the ride's 2.5 s per second and the platform walk
        /// at 2.5 m/s. The monotone cubic never dips or overshoots beyond the two keys of a segment, and passes every key exactly.</summary>
        [Test]
        public void Gaze_KeyScheduleRatesWithinLimits_NoDip()
        {
            foreach (var keys in new[] { GazeSchedule.Keys, GazeSchedule.RideKeys })
            {
                const double ds = 0.01, v = 2.5;
                double maxY = 0, maxP = 0, sMaxY = 0, sMaxP = 0;
                GazeSchedule.Evaluate(keys[0].S + 1e-7, out double py, out double pp);
                for (double s = keys[0].S + 1e-7 + ds; s <= keys[keys.Length - 1].S; s += ds)
                {
                    GazeSchedule.Evaluate(s, out double y, out double p);
                    double ry = Math.Abs(BwMath.DeltaDeg(py, y)) / ds * v, rp = Math.Abs(p - pp) / ds * v;
                    if (ry > maxY) { maxY = ry; sMaxY = s; }
                    if (rp > maxP) { maxP = rp; sMaxP = s; }
                    py = y; pp = p;
                }
                Assert.LessOrEqual(maxY, FrameworkConst.GazeMaxYawRate, "key yaw rate peak at s " + sMaxY.ToString("F1"));
                Assert.LessOrEqual(maxP, FrameworkConst.GazeMaxPitchRate, "key pitch rate peak at s " + sMaxP.ToString("F1"));
                for (int i = 0; i + 1 < keys.Length; i++)
                {
                    var a = keys[i]; var b = keys[i + 1];
                    double dy = BwMath.DeltaDeg(a.Yaw, b.Yaw);
                    for (int j = 1; j < 50; j++)
                    {
                        double s = a.S + (b.S - a.S) * j / 50.0;
                        if (s <= BwPath.Length && keys == GazeSchedule.RideKeys) continue;
                        GazeSchedule.Evaluate(s, out double y, out double p);
                        double off = BwMath.DeltaDeg(a.Yaw, y);
                        Assert.That(off, Is.InRange(Math.Min(0.0, dy) - 1e-6, Math.Max(0.0, dy) + 1e-6), "yaw dip in keys " + a.S + "-" + b.S + " at s " + s);
                        Assert.That(p, Is.InRange(Math.Min(a.Pitch, b.Pitch) - 1e-6, Math.Max(a.Pitch, b.Pitch) + 1e-6), "pitch dip in keys " + a.S + "-" + b.S + " at s " + s);
                    }
                    if (a.S > BwPath.Length || keys == GazeSchedule.Keys)
                    {
                        GazeSchedule.Evaluate(a.S, out double ky, out double kp);
                        Assert.AreEqual(0.0, BwMath.DeltaDeg(a.Yaw, ky), 1e-9, "key yaw at s " + a.S);
                        Assert.AreEqual(a.Pitch, kp, 1e-9, "key pitch at s " + a.S);
                    }
                }
            }
        }

        [Test]
        public void Gaze_KeysAreValid()
        {
            var problems = GazeSchedule.Validate();
            Assert.IsEmpty(problems, string.Join("; ", problems));
        }

        /// <summary>The ride (ADDENDUM A1 00): the follower over the whole ending at both walking speeds. The ride advances s at
        /// 2.5 per second whatever the speed, the platform walk at the walking speed after a new 0.5 s ramp; the final look never
        /// turns faster than 18 / 13 deg/s, and the follower reaches the look-back of the shuttle (90, +18) and the lantern (90, 0).</summary>
        [Test]
        public void Gaze_RideFollowerReachesTheKeysWithinTheRateLimits([Values(1.5, 2.5)] double speed)
        {
            var w = new WalkPlanner(1);
            w.Begin(1450.0, speed, 0.0, -27.5, -7.0, speed);
            const double dt = 1.0 / 60.0;
            double s = 1450.0, t = 0.0, yaw = w.OutYaw, pitch = w.OutPitch, maxY = 0, maxP = 0;
            bool rode = false, restarted = false;
            double y1800 = double.NaN, p1800 = double.NaN;
            while (s < EndingPath.SGoal && t < 2000.0)
            {
                w.Step(dt, s, t);
                if (s < BwPath.Length) s = Math.Min(BwPath.Length, s + w.SpeedCommand * dt);              // the walk into the cabin
                else if (s < EndingPath.SFoot0) { rode = true; s = Math.Min(EndingPath.SFoot0, s + BulwarkConst.RideSRate * dt); }   // the ride
                else
                {
                    if (!restarted) { w.RestartRamp(s, 0.0); restarted = true; }
                    s += w.SpeedCommand * dt;                                                           // the platform walk
                }
                t += dt;
                maxY = Math.Max(maxY, Math.Abs(BwMath.DeltaDeg(yaw, w.OutYaw)) / dt);
                maxP = Math.Max(maxP, Math.Abs(w.OutPitch - pitch) / dt);
                yaw = w.OutYaw; pitch = w.OutPitch;
                if (double.IsNaN(y1800) && s >= 1822.0) { y1800 = yaw; p1800 = pitch; }
                Assert.IsFalse(double.IsNaN(yaw) || double.IsNaN(pitch), "NaN look at s " + s);
            }
            Assert.IsTrue(rode && restarted, "rode and walked on");
            Assert.LessOrEqual(maxY, FrameworkConst.GazeMaxYawRate + 1e-6, "yaw rate");
            Assert.LessOrEqual(maxP, FrameworkConst.GazeMaxPitchRate + 1e-6, "pitch rate");
            Assert.AreEqual(0.0, BwMath.DeltaDeg(90.0, y1800), 4.0, "the shuttle look-back faces the dam at s 1822 (KS26)");
            Assert.AreEqual(32.0, p1800, 3.0, "and looks up it to the crest");
            Assert.AreEqual(0.0, BwMath.DeltaDeg(90.0, yaw), 6.0, "facing the lantern and the dam at the goal");
        }

        /// <summary>
        /// T-NUM (14.10.1) and Part 9: the FINAL camera look (follower + sway, what CameraRig.SetLook receives) never turns faster
        /// than 18 / 13 deg/s, from the first frame on and at both walk speeds and two seeds; the shown sway (output - follower)
        /// stays within 0.3 deg combined, and it is really there (not a trivially zero sway).
        /// </summary>
        [Test]
        public void Gaze_FinalRateNeverExceedsTheLimits_SwayStaysWithin0p3([Values(1.5, 2.5)] double speed, [Values(1, 7)] int seed)
        {
            var w = new WalkPlanner(seed);
            w.Begin(0.0, 0.0, 0.0, 0.0, 0.0, speed);
            const double dt = 1.0 / 60.0;
            double s = 0.0, yaw = w.OutYaw, pitch = w.OutPitch, t = 0;
            double maxYaw = 0, maxPitch = 0, maxSway = 0, maxFollowYaw = 0, maxFollowPitch = 0;
            double fy = w.FollowYaw, fp = w.FollowPitch;
            while (s < FrameworkConst.GoalTriggerS)
            {
                w.Step(dt, s, t);
                s += w.SpeedCommand * dt;
                t += dt;
                maxYaw = Math.Max(maxYaw, Math.Abs(BwMath.DeltaDeg(yaw, w.OutYaw)) / dt);
                maxPitch = Math.Max(maxPitch, Math.Abs(w.OutPitch - pitch) / dt);
                maxFollowYaw = Math.Max(maxFollowYaw, Math.Abs(BwMath.DeltaDeg(fy, w.FollowYaw)) / dt);
                maxFollowPitch = Math.Max(maxFollowPitch, Math.Abs(w.FollowPitch - fp) / dt);
                double sy = BwMath.DeltaDeg(w.FollowYaw, w.OutYaw), sp = w.OutPitch - w.FollowPitch;
                maxSway = Math.Max(maxSway, Math.Sqrt(sy * sy + sp * sp));
                yaw = w.OutYaw; pitch = w.OutPitch; fy = w.FollowYaw; fp = w.FollowPitch;
            }
            Assert.LessOrEqual(maxYaw, FrameworkConst.GazeMaxYawRate + 1e-6, "final yaw rate (after the sway)");
            Assert.LessOrEqual(maxPitch, FrameworkConst.GazeMaxPitchRate + 1e-6, "final pitch rate (after the sway)");
            Assert.LessOrEqual(maxSway, FrameworkConst.GazeSwayMaxDeg + 1e-9, "shown sway amplitude");
            Assert.Greater(maxSway, 0.05, "the handheld sway is present");
            UnityEngine.Debug.Log($"[BW] gaze at {speed:F1} m/s seed {seed}: final peaks yaw {maxYaw:F2} / pitch {maxPitch:F2} deg/s, " +
                                  $"follower peaks {maxFollowYaw:F2} / {maxFollowPitch:F2} deg/s, max shown sway {maxSway:F3} deg");
        }

        /// <summary>The sway alone could push a rate-limited follower past the limit; the final limiter must absorb it.</summary>
        [Test]
        public void Gaze_FinalLimiterAbsorbsTheSwayAtTheLimit()
        {
            // start looking north (yaw 0) where the keys hold yaw 80 (s 150-166): the follower turns 80 deg at its 18 deg/s
            // limit for seconds while the sway keeps pushing (at most +-0.2 deg on top)
            var w = new WalkPlanner(3);
            w.Begin(150.0, FrameworkConst.CinematicSpeed, 0.0, 0.0, -8.0, FrameworkConst.CinematicSpeed);
            const double dt = 1.0 / 60.0;
            double s = 150.0, yaw = w.OutYaw, t = 0, maxYaw = 0;
            int atLimit = 0;
            for (int i = 0; i < 60 * 40; i++)
            {
                w.Step(dt, s, t);
                s += w.SpeedCommand * dt; t += dt;
                double r = Math.Abs(BwMath.DeltaDeg(yaw, w.OutYaw)) / dt;
                maxYaw = Math.Max(maxYaw, r);
                if (r > FrameworkConst.GazeMaxYawRate - 1e-6) atLimit++;
                yaw = w.OutYaw;
            }
            Assert.LessOrEqual(maxYaw, FrameworkConst.GazeMaxYawRate + 1e-6);
            Assert.Greater(atLimit, 60, "the limiter was exercised for at least a second");
            UnityEngine.Debug.Log($"[BW] gaze 0 -> 80 deg turn: peak {maxYaw:F3} deg/s, {atLimit} frames at the limit");
        }

        /// <summary>14.12 CINE = BRISK: the cinematic speed is the Shift speed, one source (no literal of its own).</summary>
        [Test]
        public void CineSpeed_IsAnAliasOfBrisk()
        {
            Assert.AreEqual(BulwarkConst.Brisk, BulwarkConst.Cine, 0.0);
            Assert.AreEqual(BulwarkConst.Brisk, FrameworkConst.CinematicSpeed, 0.0);
            Assert.AreEqual(FrameworkConst.BriskSpeed, FrameworkConst.CinematicSpeed, 0.0);
            var f = typeof(BulwarkConst).GetField("Cine");
            Assert.IsNotNull(f, "BulwarkConst.Cine");
            Assert.IsFalse(f.IsLiteral, "Cine must not be a literal of its own");
            string src = System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath, "_Game/Scripts/Runtime/Core/BulwarkConst.cs"));
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(src, @"\bdouble\s+Cine\s*=\s*Brisk\s*;"), "declared as Cine = Brisk");
        }

        /// <summary>14.10.4 trigger times are computed from the path and the speeds (FrameworkConst), not typed in.</summary>
        [Test]
        public void TriggerTimes_AreComputedFromThePath()
        {
            // ADDENDUM A1 00: walk to the board trigger + board ramp + the 143 s ride (R1) + the 38.5 m platform walk (new ramp)
            double walk15 = BulwarkConst.GoalTriggerS / BulwarkConst.Walk + 0.5 * BulwarkConst.StartRamp;
            double board15 = 2.0 * (BulwarkConst.GoalRestS - BulwarkConst.GoalTriggerS) / BulwarkConst.Walk;
            double foot15 = (EndingPath.SGoal - EndingPath.SFoot0) / BulwarkConst.Walk + 0.5 * BulwarkConst.StartRamp;
            Assert.AreEqual(143.0, FrameworkConst.RideTotalTime, 1e-12, "4 + 75 + 6 + 55 + 3 s");
            Assert.AreEqual(walk15 + board15 + FrameworkConst.RideTotalTime + foot15, FrameworkConst.ManualTriggerTime, 1e-9);
            Assert.AreEqual(1170.1667, FrameworkConst.ManualTriggerTime, 1e-3, "1540 / 1.5 + 143.5");
            Assert.AreEqual(759.5, FrameworkConst.CineTriggerTime, 1e-3, "1540 / 2.5 + 143.5");
            Assert.AreEqual(55.5, FrameworkConst.EndTotal, 1e-12, "the end sequence of ADDENDUM A1 3.10, the look held to t_g 40 (stinger)");
            Assert.AreEqual(FrameworkConst.ManualTriggerTime + FrameworkConst.EndTotal, FrameworkConst.ManualDoneTime, 1e-12);
            Assert.AreEqual(815.0, FrameworkConst.CineDoneTime, 1e-3, "759.5 + 55.5");
            Assert.AreEqual(FrameworkConst.ManualTriggerTime, FrameworkConst.TriggerTimeAt(FrameworkConst.WalkSpeed), 1e-12);
            Assert.AreEqual(OfflineAudioRenderer.TriggerTime(new OfflineRenderSettings { Speed = 1.5 }), walk15, 1e-9, "the audio render's model is the walked path");
        }

        /// <summary>Part 2.5.3 / 11.7: every run is capped at 60 unless a perf run explicitly asks to be uncapped (-perf-log + -uncapped).</summary>
        [Test]
        public void FramePolicy_CapsUnlessAPerfRunAsksForUncapped()
        {
            int vs, fps;
            QualityBootstrap.FramePolicy(qa: false, perfLog: true, uncappedFlag: false, soak: false, out vs, out fps);
            Assert.AreEqual(FrameworkConst.QaFrameCap, fps, "-perf-log alone keeps the 60 fps cap");
            Assert.AreEqual(0, vs, "vSync off so targetFrameRate applies");
            QualityBootstrap.FramePolicy(true, true, false, false, out vs, out fps);
            Assert.AreEqual(FrameworkConst.QaFrameCap, fps, "autotest + -perf-log stays capped");
            QualityBootstrap.FramePolicy(false, true, true, false, out vs, out fps);
            Assert.AreEqual(-1, fps, "explicit uncapped perf run"); Assert.AreEqual(0, vs);
            QualityBootstrap.FramePolicy(true, true, true, true, out vs, out fps);
            Assert.AreEqual(FrameworkConst.QaFrameCap, fps, "the soak is never uncapped");
            QualityBootstrap.FramePolicy(false, false, true, false, out vs, out fps);
            Assert.AreEqual(-1, fps, "-uncapped without -perf-log is normal play"); Assert.AreEqual(1, vs, "VSync on in play");
            QualityBootstrap.FramePolicy(true, false, false, false, out vs, out fps);
            Assert.AreEqual(FrameworkConst.QaFrameCap, fps, "QA run capped"); Assert.AreEqual(0, vs);
            Assert.IsFalse(QualityBootstrap.UncappedRequested(true, false, false));
            Assert.IsTrue(QualityBootstrap.UncappedRequested(true, true, false));
        }

        [Test]
        public void Lateral_StaysInsideTheLaneWithMargin()
        {
            for (double s = 0.0; s <= BwPath.Length; s += 0.5)
            {
                BwPath.Lane(s, out double lo, out double hi);
                double lat = BwPath.CinematicLateral(s);
                Assert.GreaterOrEqual(lat, lo + FrameworkConst.LateralMargin - 1e-9, "s=" + s);
                Assert.LessOrEqual(lat, hi - FrameworkConst.LateralMargin + 1e-9, "s=" + s);
            }
        }

        [Test]
        public void ProgressSpeed_IsConstantAfterTheRamp()
        {
            var w = new WalkPlanner(1);
            w.Begin(100.0, 0.0, 0.0, 0.0, 0.0, FrameworkConst.CinematicSpeed);
            for (int i = 0; i < 600; i++)
            {
                w.Step(1.0 / 60.0, 100.0, 0.0);
                if (w.Elapsed > FrameworkConst.StartRamp + 1e-9) Assert.AreEqual(FrameworkConst.CinematicSpeed, w.SpeedCommand, 1e-12);
            }
        }

        [Test]
        public void Obstacles_AreKeyedByNameAndJudgedOnlyOnTheCinematicPath()
        {
            // PathGuide.AddObstacle (FRAMEWORK.md 4.0): a named box (re-adding replaces it); ObstacleAt is the analytic capsule
            // test; ObstaclesOnCinematicPath lists only boxes the cinematic capsule touches (the autotest failure rule).
            const string a = "test_obstacle_cart_like", b = "test_obstacle_on_path";
            // the registry is static and beats register their own props (the crest cart): test on an empty registry and
            // restore it afterwards
            var saved = new System.Collections.Generic.List<(string, UnityEngine.Bounds)>();
            foreach (var o in PathGuide.Obstacles) saved.Add((o.Name, o.Box));
            PathGuide.ClearObstacles();
            try
            {
                // a cart-like box at x 1.35..2.65, z 328.6..331.4 (the 14.5.2 cart): the cinematic path passes at x -1.5
                var cart = new UnityEngine.Bounds(new UnityEngine.Vector3(2.0f, 0.95f, 330f), new UnityEngine.Vector3(1.3f, 1.9f, 2.8f));
                int before = PathGuide.Obstacles.Count;   // 0
                PathGuide.AddObstacle(cart, a);
                PathGuide.AddObstacle(cart, a);
                Assert.AreEqual(before + 1, PathGuide.Obstacles.Count, "same name replaces");
                // lateral = x + 1.5 on the straight crest (heading north, right = +x)
                Assert.IsTrue(PathGuide.ObstacleAt(330.0, 2.0 + 1.5, 0f, out var hit), "capsule at the cart");
                Assert.AreEqual(a, hit.Name);
                Assert.IsTrue(PathGuide.ObstacleAt(330.0, 1.35 + 1.5 - 0.30, 0f, out _), "capsule edge 0.05 m into the box (radius 0.35)");
                Assert.IsFalse(PathGuide.ObstacleAt(330.0, 1.35 + 1.5 - 0.40, 0f, out _), "capsule 0.05 m clear of the box");
                Assert.IsFalse(PathGuide.ObstacleAt(330.0, BwPath.CinematicLateral(330.0), 0f, out _), "cinematic lateral is clear");
                var on = new System.Collections.Generic.List<string>();
                Assert.AreEqual(0, PathGuide.ObstaclesOnCinematicPath(0.25, on), string.Join("; ", on));
                // a box on the cinematic path at s 50 (gallery centre line x -1.5)
                PathGuide.AddObstacle(new UnityEngine.Bounds(new UnityEngine.Vector3(-1.5f, 1.0f, 50f), new UnityEngine.Vector3(0.4f, 2.0f, 0.4f)), b);
                Assert.AreEqual(1, PathGuide.ObstaclesOnCinematicPath(0.25, on), "one obstacle on the cinematic path");
                StringAssert.StartsWith(b, on[0]);
                Assert.IsTrue(PathGuide.RemoveObstacle(b));
                Assert.AreEqual(0, PathGuide.ObstaclesOnCinematicPath(0.25, on));
            }
            finally
            {
                PathGuide.RemoveObstacle(a); PathGuide.RemoveObstacle(b);
                foreach (var (n, box) in saved) PathGuide.AddObstacle(box, n);
            }
        }
    }
}
