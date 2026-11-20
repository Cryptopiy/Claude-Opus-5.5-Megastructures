using System;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    /// <summary>
    /// The 1 500 m path, its lanes and the cinematic schedule (rewrite of The Ring's PathScheduleTests, IMPORT_PLAN 4.4 d and
    /// section 8): FilletPath length and the 14.4 tangent points / arc midpoints / conversion table, NocLanes, the WalkPlanner
    /// timings at 2.5 m/s (cinematic) and 1.5 m/s (QA), the stop ramp, the gaze follower rate limits and sway.
    /// </summary>
    public sealed class PathScheduleTests
    {
        const double Tol = 0.02;   // 14.10: 0.02 m unless stated
        static FilletPath P => NocLanes.Path;

        // ------------------------------------------------------------------ path (14.4)
        [Test]
        public void PathLengthIs1500()
        {
            Assert.AreEqual(1500.00, P.Length, 0.05, "filleted length (14.4)");
            Assert.AreEqual(NocturneConstants.SharpLength, P.SharpEnd, 1e-9, "sharp end");
        }

        [Test]
        public void TangentPointsMatch14_4()
        {
            double[,] expected = { { 171.00, 208.70, 24 }, { 1056.44, 1103.56, 30 }, { 1236.44, 1283.56, 30 } };
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(P.Arc(i, out double d0, out double d1, out double r, out _, out _), "arc " + i);
                Assert.AreEqual(expected[i, 0], d0, Tol, $"C{i + 1} tangent in");
                Assert.AreEqual(expected[i, 1], d1, Tol, $"C{i + 1} tangent out");
                Assert.AreEqual(expected[i, 2], r, 1e-9, $"C{i + 1} radius");
            }
            Assert.IsFalse(P.Arc(3, out _, out _, out _, out _, out _), "exactly three fillets (collinear P1, P3, P4, P5 get none)");
            Assert.AreEqual(37.70, 24.0 * Math.PI / 2.0, Tol, "C1 arc length");
        }

        [Test]
        public void ArcMidpointsAndCentresMatch14_4()
        {
            P.Arc(1, out _, out _, out _, out double c2x, out double c2z);
            P.Arc(2, out _, out _, out _, out double c3x, out double c3z);
            Assert.AreEqual(871.74, c2x, Tol); Assert.AreEqual(225.0, c2z, Tol);
            Assert.AreEqual(931.74, c3x, Tol); Assert.AreEqual(357.88, c3z, Tol);
            P.PointAt(1080.0, out double x, out double z);
            Assert.AreEqual(892.95, x, Tol, "C2 midpoint x at s 1080"); Assert.AreEqual(203.79, z, Tol, "C2 midpoint z");
            P.PointAt(1260.0, out x, out z);
            Assert.AreEqual(910.53, x, Tol, "C3 midpoint x at s 1260"); Assert.AreEqual(379.09, z, Tol, "C3 midpoint z");
        }

        [Test]
        public void WaypointsAndHeadingsMatch14_4()
        {
            double x, z;
            P.PointAt(0, out x, out z); Assert.AreEqual(0, x, Tol); Assert.AreEqual(0, z, Tol);
            P.PointAt(120, out x, out z); Assert.AreEqual(0, x, Tol); Assert.AreEqual(120, z, Tol);
            P.PointAt(400, out x, out z); Assert.AreEqual(215.30, x, Tol, "P3 at s 400"); Assert.AreEqual(195, z, Tol);
            P.PointAt(560, out x, out z); Assert.AreEqual(375.30, x, Tol, "P4 at s 560");
            P.PointAt(880, out x, out z); Assert.AreEqual(695.30, x, Tol, "P5 at s 880");
            P.PointAt(1500, out x, out z); Assert.AreEqual(1148.18, x, Tol, "goal x at s 1500"); Assert.AreEqual(387.88, z, Tol);
            for (double s = 0; s <= 171; s += 3) Assert.AreEqual(0.0, P.HeadingDeg(s), 1e-6, "heading 0 to s 171");
            for (double s = 208.8; s <= 1056; s += 7) Assert.AreEqual(90.0, P.HeadingDeg(s), 1e-6, "heading 90 on the east leg");
            for (double s = 208.8; s <= 1056; s += 7) { P.PointAt(s, out x, out _); Assert.AreEqual(s - NocturneConstants.EastLegOffset, x, Tol, "x = s - 184.70"); }
            for (double s = 1103.7; s <= 1236.3; s += 5) Assert.AreEqual(0.0, P.HeadingDeg(s), 1e-6, "heading 0 on the north leg");
            for (double s = 1283.7; s <= 1500; s += 5) Assert.AreEqual(90.0, P.HeadingDeg(s), 1e-6, "heading 90 to the goal");
        }

        [Test]
        public void SharpConversionTable()
        {
            Assert.AreEqual(410.30, P.SharpS(400.0), Tol, "P3 sharp");
            Assert.AreEqual(890.30, P.SharpS(880.0), Tol, "P5 sharp");
            Assert.AreEqual(1536.06, P.SharpS(P.Length), Tol, "P8 sharp");
            for (double d = 0; d <= P.Length; d += 13.7) Assert.AreEqual(d, P.DAtSharp(P.SharpS(d)), 1e-6, "round trip");
        }

        [Test]
        public void ProjectionRoundTripAndContinuity()
        {
            double prevX = 0, prevZ = 0;
            for (double s = 0; s <= P.Length; s += 0.25)
            {
                P.CorridorPoint(s, 1.3, out double x, out double z);
                double sp = P.Project(x, z, s, 25.0, out double dist);
                Assert.AreEqual(s, sp, 1e-6, "projection of a lane point at s " + s);
                Assert.AreEqual(1.3, dist, 1e-6);
                P.PointAt(s, out x, out z);
                if (s > 0) Assert.AreEqual(0.25, Math.Sqrt((x - prevX) * (x - prevX) + (z - prevZ) * (z - prevZ)), 1e-4, "arc-length parametrisation");
                prevX = x; prevZ = z;
            }
        }

        // ------------------------------------------------------------------ lanes (14.4)
        [Test]
        public void LaneHalfWidthsMatch14_4()
        {
            Assert.AreEqual(1.85, NocLanes.LaneHalfWidth(10), 1e-9, "alley 4.4 m");
            Assert.AreEqual(2.65, NocLanes.LaneHalfWidth(46), 1e-9, "bar 6.0 m");
            Assert.AreEqual(1.95, NocLanes.LaneHalfWidth(68), 1e-9, "4.6 m");
            Assert.AreEqual(2.15, NocLanes.LaneHalfWidth(120), 1e-9, "mouth 5.0 m");
            Assert.AreEqual(10.0, NocLanes.LaneHalfWidth(150), 1e-9, "crossing");
            Assert.AreEqual(11.0, NocLanes.LaneHalfWidth(300), 1e-9, "median");
            Assert.AreEqual(12.0, NocLanes.LaneHalfWidth(420), 1e-9, "tongue plaza (w 75 -> min 12)");
            Assert.AreEqual(2.15, NocLanes.LaneHalfWidth(520), 1e-9, "tongue pier (w 3.2 -> 2.15)");
            Assert.AreEqual(2.15, NocLanes.LaneHalfWidth(700), 1e-9, "bridge");
            Assert.AreEqual(6.0, NocLanes.LaneHalfWidth(950), 1e-9, "garden");
            Assert.AreEqual(10.0, NocLanes.LaneHalfWidth(1150), 1e-9, "promenade");
            Assert.AreEqual(12.0, NocLanes.LaneHalfWidth(1350), 1e-9, "plaza");
            Assert.AreEqual(3.0, NocLanes.LaneHalfWidth(1500), 1e-9, "funnel to 3.0 at the tenement");
            Assert.AreEqual(75.0, NocLanes.TongueHalfWidth(225.3), 1e-9, "T-PLAN w(225.3)");
            Assert.AreEqual(3.2, NocLanes.TongueHalfWidth(330.0), 1e-9, "T-PLAN w(330)");
            Assert.AreEqual(2.5, NocLanes.TongueHalfWidth(375.3), 1e-9, "T-PLAN w(375.3)");
        }

        [Test]
        public void LanesAreContinuousAndInsideTheFillets()
        {
            double prev = NocLanes.LaneHalfWidth(0);
            for (double s = 0.01; s <= P.Length; s += 0.01)
            {
                double h = NocLanes.LaneHalfWidth(s);
                Assert.Less(Math.Abs(h - prev), 0.05, "lane jump at s " + s.ToString("F2"));
                Assert.Greater(h, 1.5, "lane narrower than the player at s " + s.ToString("F2"));
                prev = h;
                if (P.OnArc(s))
                {
                    P.Arc(s < 300 ? 0 : (s < 1150 ? 1 : 2), out _, out _, out double r, out _, out _);
                    Assert.Less(h + NocturneConstants.PlayerRadius, r, "lane must stay inside the fillet radius at s " + s.ToString("F1"));
                }
            }
        }

        [Test]
        public void DistanceOutsideLanes()
        {
            Assert.AreEqual(0.0, NocLanes.DistanceOutsideLanes(NocLanes.CentreAt(300), 300), 1e-6);
            Assert.AreEqual(0.0, NocLanes.DistanceOutsideLanes(NocLanes.CorridorToWorld(300, 10.9, 0), 300), 1e-6);
            Assert.AreEqual(1.0, NocLanes.DistanceOutsideLanes(NocLanes.CorridorToWorld(300, 12.0, 0), 300), 1e-4);
            Assert.AreEqual(2.0, NocLanes.DistanceOutsideLanes(NocLanes.CorridorToWorld(700, -4.15, 0), 700), 1e-4, "bridge");
            Assert.Greater(NocLanes.DistanceOutsideLanes(new Vector3(0, 0, -2), 0), 1.9, "behind the start");
        }

        // ------------------------------------------------------------------ stop ramp (14.11)
        [Test]
        public void StopRampCoversExactly3mWithT6OverV0()
        {
            foreach (double v0 in new[] { 2.5, 1.5 })
            {
                var r = StopRamp.Begin(NocturneConstants.GoalTriggerS, v0);
                Assert.AreEqual(6.0 / v0, r.T, 1e-9, "T = 6 / v0");
                Assert.AreEqual(1500.0, r.PositionAt(r.T), 1e-9);
                Assert.AreEqual(1500.0, r.PositionAt(r.T * 0.999999), 1e-4);
                Assert.AreEqual(v0, r.SpeedAt(0), 1e-12);
                Assert.AreEqual(0.0, r.SpeedAt(r.T), 1e-12);
                // closed form equals the numerical integral of v(t)
                double s = r.s0; int n = 100000; double dt = r.T / n;
                for (int i = 0; i < n; i++) s += r.SpeedAt((i + 0.5) * dt) * dt;
                Assert.AreEqual(1500.0, s, 1e-6);
            }
            Assert.AreEqual(2.4, 6.0 / 2.5, 1e-12); Assert.AreEqual(4.0, 6.0 / 1.5, 1e-12);
        }

        // ------------------------------------------------------------------ walk timings (14.10 Autoplay, IMPORT_PLAN 4.4 d)
        static WalkPlanner Simulate(WalkMode mode, double speed, double dt, out double maxDev, out double maxYawRate, out double maxPitchRate, out double maxSway, out double peakYawRate, out double peakPitchRate)
        {
            var w = new WalkPlanner(P, 1);
            w.Begin(mode, 0, 0, 0, 0, 0, 0, -2, speed);
            maxDev = 0; maxYawRate = 0; maxPitchRate = 0; maxSway = 0; peakYawRate = 0; peakPitchRate = 0;
            double t = 0, py = w.FollowerYaw, pp = w.FollowerPitch;
            for (int i = 0; i < 1200000 && !w.Finished; i++)
            {
                w.Step(dt, w.TargetX, w.TargetZ, t);
                t += dt;
                double vy = Math.Abs(NocMath.DeltaAngle(py, w.FollowerYaw)) / dt, vp = Math.Abs(w.FollowerPitch - pp) / dt;
                maxYawRate = Math.Max(maxYawRate, vy); maxPitchRate = Math.Max(maxPitchRate, vp);
                py = w.FollowerYaw; pp = w.FollowerPitch;
                double sx = NocMath.DeltaAngle(w.FollowerYaw, w.OutYaw), sy = w.OutPitch - w.FollowerPitch;
                maxSway = Math.Max(maxSway, Math.Sqrt(sx * sx + sy * sy));
                if (w.WalkTime > 1.0 && !w.Triggered) maxDev = Math.Max(maxDev, Math.Abs(w.Speed - speed));
            }
            return w;
        }

        [Test]
        public void CinematicTimingsAt2_5()
        {
            var w = Simulate(WalkMode.Cinematic, 2.5, 1.0 / 60.0, out double dev, out double yr, out double pr, out double sway, out _, out _);
            Assert.IsTrue(w.Finished);
            Assert.AreEqual(NocturneConstants.CineTriggerTime, w.TriggerTime, 0.02, "trigger at 599.05 s (spec tolerance 2 s)");
            Assert.AreEqual(NocturneConstants.CineStopTime, w.StopTime, 0.02, "stop at 601.45 s");
            Assert.AreEqual(1500.0, w.D, 1e-9, "stops exactly at s 1500");
            Assert.Less(dev, 1e-9, "constant 2.5 m/s after the 0.5 s ramp (no slowing, no holds)");
            Assert.LessOrEqual(yr, NocturneConstants.YawRateMax + 1e-6, "yaw rate limit");
            Assert.LessOrEqual(pr, NocturneConstants.PitchRateMax + 1e-6, "pitch rate limit");
            Assert.LessOrEqual(sway, NocturneConstants.SwayMax + 1e-9, "sway radius <= 0.3 deg");
            Assert.Greater(yr, 10.0, "the rate limit is exercised (14.5 peaks about 19.3 / 17.9 deg/s)");
            TestContext.WriteLine(FormattableString.Invariant($"cinematic: trigger {w.TriggerTime:F3} s, stop {w.StopTime:F3} s, peak yaw rate {yr:F2} deg/s, peak pitch rate {pr:F2} deg/s, sway {sway:F3} deg"));
        }

        [Test]
        public void QaTimingsAt1_5()
        {
            var w = Simulate(WalkMode.AutoTest, 1.5, 1.0 / 60.0, out double dev, out _, out _, out _, out _, out _);
            Assert.AreEqual(NocturneConstants.WalkTriggerTime, w.TriggerTime, 0.02, "trigger at 998.1 s (spec tolerance 3 s)");
            Assert.AreEqual(NocturneConstants.WalkStopTime, w.StopTime, 0.02, "stop at 1002.1 s");
            Assert.AreEqual(1500.0, w.D, 1e-9);
            Assert.Less(dev, 1e-9);
        }

        [Test]
        public void TimingsDoNotDependOnFrameRate()
        {
            var a = Simulate(WalkMode.Cinematic, 2.5, 1.0 / 30.0, out _, out _, out _, out _, out _, out _);
            var b = Simulate(WalkMode.Cinematic, 2.5, 1.0 / 144.0, out _, out _, out _, out _, out _, out _);
            Assert.AreEqual(a.TriggerTime, b.TriggerTime, 0.01);
            Assert.AreEqual(a.StopTime, b.StopTime, 0.01);
        }

        [Test]
        public void GazeKeysAreOrderedAndNeverJumpWithinAMetre()
        {
            var k = NocturneConstants.GazeKeys;
            for (int i = 1; i < k.Length; i++)
            {
                Assert.Greater(k[i].s, k[i - 1].s, "keys ordered by s at " + k[i].s);
                bool differ = k[i].yaw != k[i - 1].yaw || k[i].pitch != k[i - 1].pitch;
                if (differ) Assert.GreaterOrEqual(k[i].s - k[i - 1].s, 1.0, "two different poses closer than 1 m at s " + k[i].s);
            }
            Assert.AreEqual(0.0, k[0].s); Assert.AreEqual(1500.0, k[k.Length - 1].s);
        }

        [Test]
        public void FollowerReadabilityAtKeyShots()
        {
            // 14.5 "readable" = within 8 deg yaw and 6 deg pitch of the key pose; the follower is simulated over the whole walk
            var w = new WalkPlanner(P, 1);
            w.Begin(WalkMode.Cinematic, 0, 0, 0, 0, 0, 0, -2, 2.5);
            var shots = NocturneConstants.KeyShots;
            var worstYaw = new double[shots.Length]; var worstPitch = new double[shots.Length];
            double dt = 1.0 / 60.0, t = 0;
            int next = 0;
            string report = "";
            while (!w.Finished && next < shots.Length)
            {
                w.Step(dt, w.TargetX, w.TargetZ, t); t += dt;
                while (next < shots.Length && w.D >= shots[next].s - 1e-9)
                {
                    double dy = Math.Abs(NocMath.DeltaAngle(shots[next].yaw, w.FollowerYaw)), dp = Math.Abs(shots[next].pitch - w.FollowerPitch);
                    report += FormattableString.Invariant($"{shots[next].tag}: dyaw {dy:F1} dpitch {dp:F1}\n");
                    worstYaw[next] = dy; worstPitch[next] = dp;
                    next++;
                }
            }
            TestContext.WriteLine(report);
            Assert.AreEqual(shots.Length, next, "every key shot on the path was reached");
            for (int i = 0; i < shots.Length; i++)
            {
                Assert.LessOrEqual(worstYaw[i], 8.0, shots[i].tag + ": follower yaw within 8 deg of the key pose (14.5 readable)");
                Assert.LessOrEqual(worstPitch[i], 6.0, shots[i].tag + ": follower pitch within 6 deg of the key pose");
            }
        }
    }
}
