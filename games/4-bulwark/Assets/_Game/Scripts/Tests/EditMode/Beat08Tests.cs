// Beat08Tests: the new ending (ADDENDUM A1 00 LEAD DECISION 1, section 3.7): the ride timeline and its motion profiles, the
// sunset made by the descent, the platform walk and its lane, the final look-back framing and the geometry clearances of
// Beat08Ending (EndingPath in BwPath.cs; BulwarkConst ending rows; BulwarkSchedule ending rows).
using System;
using Megastructure.Beats;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class Beat08Tests
    {
        [Test]
        public void Timeline_MatchesTheLeadDecision()
        {
            Assert.AreEqual(75.0, EndingPath.TauDescent1 - EndingPath.TauDescent0, 1e-12, "75 s descent");
            Assert.AreEqual(55.0, EndingPath.TauShuttle1 - EndingPath.TauTransfer1, 1e-12, "55 s shuttle (fix wave R1)");
            Assert.AreEqual(1857.5, EndingPath.SFoot0, 1e-9, "1500 + 2.5 x 143");
            Assert.AreEqual(40.0, EndingPath.Foot.Length, 1e-3, "the walk of about 40 m");
            Assert.AreEqual(1897.5, EndingPath.SRest, 1e-3);
            Assert.AreEqual(EndingPath.SRest - 1.5, EndingPath.SGoal, 1e-12);
            Assert.AreEqual(5909.0, EndingPath.DescentLength, 1e-9, "floor -90 -> -5999");
            Assert.AreEqual(3500.0, BulwarkConst.ShuttleDistance, 1e-12);
        }

        [Test]
        public void Descent_IsASmoothSCurvePeakingAt110()
        {
            double prevD = 0, prevV = 0, maxV = 0, maxA = 0;
            const double dt = 1.0 / 240.0;
            for (double t = dt; t <= BulwarkConst.RideDescentTime + 1e-9; t += dt)
            {
                double d = EndingPath.Descent(t, out double v);
                Assert.GreaterOrEqual(d, prevD - 1e-9, "monotonic at t " + t);
                Assert.AreEqual((d - prevD) / dt, 0.5 * (v + prevV), 0.05, "speed is the derivative at t " + t);
                maxA = Math.Max(maxA, Math.Abs(v - prevV) / dt);
                maxV = Math.Max(maxV, v);
                prevD = d; prevV = v;
            }
            Assert.AreEqual(EndingPath.DescentLength, prevD, 1e-6, "arrives at the foot");
            Assert.AreEqual(110.0, maxV, 1e-6, "peak 110 m/s");
            Assert.AreEqual(Math.PI / 2 * 110.0 / EndingPath.DescentRamp, maxA, 0.05, "peak acceleration pi/2 vmax / Ta = 8.12 m/s^2");
            Assert.AreEqual(21.282, EndingPath.DescentRamp, 1e-3, "Ta = 75 - 5909 / 110");
            Assert.AreEqual(0.0, EndingPath.ProfileAccel(0.0, 5909, 75, EndingPath.DescentRamp), 1e-12, "starts without a jolt");
        }

        [Test]
        public void Shuttle_Runs3500mIn55sWith20sRamps()
        {
            Assert.AreEqual(3500.0, EndingPath.Shuttle(55.0, out double v1), 1e-9);
            Assert.AreEqual(0.0, v1, 1e-12);
            Assert.AreEqual(100.0, EndingPath.ShuttleVMax, 1e-9, "3500 / (55 - 20) = 100 m/s");
            Assert.AreEqual(1000.0, EndingPath.Shuttle(20.0, out double vr), 1e-9, "each ramp 1000 m");
            Assert.AreEqual(100.0, vr, 1e-9, "cruise from t 20");
            Assert.AreEqual(2500.0, EndingPath.Shuttle(35.0, out _), 1e-9, "cruise 15 s x 100 m/s");
            double aPeak = 0;
            for (double t = 0; t <= 55.0; t += 0.01) aPeak = Math.Max(aPeak, Math.Abs(EndingPath.ProfileAccel(t, 3500.0, 55.0, 20.0)));
            Assert.AreEqual(Math.PI / 2 * 5.0, aPeak, 0.01, "mean 5.0 m/s^2 (0.51 g), cosine peak pi/2 x 5 = 7.85");
            EndingPath.Cabin(EndingPath.SShuttle1 + 0.01, out double x, out double y, out double z, out _, out _);
            Assert.AreEqual(EndingPath.SpotX - 3500.0, x, 1e-6, "at the platform");
            Assert.AreEqual(BulwarkConst.CabinFloorFootY, y, 1e-9);
            Assert.AreEqual(EndingPath.SpotZ, z, 1e-12);
        }

        [Test]
        public void Cabin_IsContinuousAlongTheWholeRide()
        {
            EndingPath.World(1500.0, 0.0, out double px, out double py, out double pz);
            BwPath.World(1500.0, 0.0, out double wx, out double wy, out double wz);
            Assert.AreEqual(wx, px, 1e-9); Assert.AreEqual(wy, py, 1e-9); Assert.AreEqual(wz, pz, 1e-9);
            double maxStep = 0;
            const double ds = 2.5 / 60.0;   // one 60 fps frame of the ride clock
            for (double s = 1500.0 + ds; s < EndingPath.SFoot0 + 10.0; s += ds)
            {
                EndingPath.World(s, 0.0, out double x, out double y, out double z);
                double step = Math.Sqrt((x - px) * (x - px) + (y - py) * (y - py) + (z - pz) * (z - pz));
                maxStep = Math.Max(maxStep, step);
                px = x; py = y; pz = z;
            }
            Assert.LessOrEqual(maxStep, 110.0 / 60.0 + 1e-3, "no jump larger than the peak ride speed (descent 110 m/s) x 1/60 s");
            Assert.AreEqual(BulwarkConst.Eye, EndingPath.EyeY(1600.0) - EndingPath.CabinFloorY(1600.0), 1e-12, "eye 1.7 above the cabin floor");
        }

        [Test]
        public void Sun_SetsDuringTheDescent()
        {
            Assert.AreEqual(BulwarkSchedule.SunElB7(1500.0), BulwarkSchedule.EndingSunEl(1500.0), 1e-9, "continuous with beat 7");
            BulwarkSchedule.Sun(1500.0 + 1e-6, out double az, out _);
            Assert.AreEqual(305.0, az, 1e-9);
            Assert.AreEqual(1.0, BulwarkSchedule.SunDiscFraction(BulwarkSchedule.EndingSunEl(1505.0), EndingPath.Altitude(EndingPath.EyeY(1505.0))), 1e-9, "full disc at the top");
            // ADDENDUM 3.7 altitudes: the lower limb touches at 5467 m, the centre at 4388 m (KS23, s 1571.2), gone at 3421 m
            double sC = FindAltitude(4388.0);
            Assert.AreEqual(1571.2, sC, 0.5, "the centre sets at s 1571.2");
            Assert.AreEqual(0.5, BulwarkSchedule.SunDiscFraction(BulwarkSchedule.EndingSunEl(sC), 4388.0), 0.03, "half the disc at KS23");
            double sL = FindAltitude(5467.0), sG = FindAltitude(3421.0);
            Assert.AreEqual(1.0, BulwarkSchedule.SunDiscFraction(BulwarkSchedule.EndingSunEl(sL), 5467.0), 0.02, "lower limb touching");
            Assert.AreEqual(0.0, BulwarkSchedule.SunDiscFraction(BulwarkSchedule.EndingSunEl(sG), 3421.0), 0.02, "upper limb gone");
            Assert.Less(sL, sC); Assert.Less(sC, sG);
            Assert.AreEqual(-2.35, BulwarkSchedule.EndingSunEl(EndingPath.SGoal), 1e-9, "-2.35 at the goal");
            Assert.AreEqual(-2.82, BulwarkSchedule.EndingSunEl(EndingPath.SGoal) + BulwarkSchedule.EndSunSink(BulwarkConst.EndSunSinkStart + BulwarkConst.EndSunSinkTime), 1e-9, "-2.82 at the end of the sink");
            // the shadow line on the wall: R (1/cos el - 1) = 4386 m at -2.126, 5363 m at -2.35
            Assert.AreEqual(4386.0, ShadowAltitude(-2.126), 3.0);
            Assert.AreEqual(5363.0, ShadowAltitude(-2.35), 4.0);
            Assert.Greater(ShadowAltitude(-2.82), 6000.0 + 1.0, "the crest dark after the sink");
            // the penumbra the shaders use (BwEarthLitN = this disc fraction): the 2.4 km alpenglow band of 3.7 at el -2.35
            Assert.AreEqual(0.814, BulwarkSchedule.SunDiscFraction(-2.35, 6000.0), 0.01, "crest fraction 0.81 at el -2.35");
            Assert.AreEqual(0.0, BulwarkSchedule.SunDiscFraction(-2.35, 4200.0), 1e-9, "upper limb below the horizon at 4.2 km");
            Assert.Greater(BulwarkSchedule.SunDiscFraction(-2.35, 4600.0), 0.03, "the band reaches below 4.6 km");
            Assert.AreEqual(0.0, BulwarkSchedule.SunDiscFraction(-2.76, 6000.0), 1e-9, "the crest dark at el -2.752");
            double tDark = BulwarkConst.EndSunSinkStart;
            while (BulwarkSchedule.SunDiscFraction(BulwarkSchedule.EndingSunEl(EndingPath.SGoal) + BulwarkSchedule.EndSunSink(tDark), 6000.0) > 0.0) tDark += 0.05;
            Assert.AreEqual(19.0, tDark, 0.3, "the crest goes dark at t_g 19 (the sink over 9-23 s)");
            Assert.Less(tDark, BulwarkConst.EndLiftStart + BulwarkConst.EndLiftTime, "dark before the tilt-up ends");
        }

        static double FindAltitude(double h)
        {
            double lo = EndingPath.SDescent0, hi = EndingPath.SDescent1;
            for (int i = 0; i < 80; i++)
            {
                double m = 0.5 * (lo + hi);
                if (EndingPath.Altitude(EndingPath.EyeY(m)) > h) lo = m; else hi = m;
            }
            return 0.5 * (lo + hi);
        }

        static double ShadowAltitude(double elDeg) => BulwarkConst.EarthRadius * (1.0 / Math.Cos(elDeg * Math.PI / 180.0) - 1.0);

        [Test]
        public void Haze_ThickensToTheFoot()
        {
            Assert.AreEqual(BulwarkConst.HazeSigma[6], BulwarkSchedule.HazeSigma(1520.0), 1e-12, "4.5e-5 to F 0.5");
            Assert.AreEqual(BulwarkConst.HazeFoot, BulwarkSchedule.HazeSigma(EndingPath.SRest), 1e-12, "2e-4 at the foot");
            double prev = 0;
            for (double s = 1500.5; s < EndingPath.SRest; s += 0.5) { double h = BulwarkSchedule.HazeSigma(s); Assert.GreaterOrEqual(h, prev - 1e-15); prev = h; }
        }

        [Test]
        public void PlatformWalk_LaneIsSmoothAndClearOfTheProps()
        {
            double prevLo = 0, prevHi = 0;
            for (double s = EndingPath.SFoot0; s <= EndingPath.SRest; s += 0.05)
            {
                EndingPath.Lane(s, out double lo, out double hi);
                Assert.LessOrEqual(lo, hi);
                if (s > EndingPath.SFoot0) Assert.LessOrEqual(Math.Abs(hi - prevHi) / 0.05, 0.6 + 1e-3, "edge slope at s " + s);
                prevLo = lo; prevHi = hi;
                // the capsule (lane edge + 0.35) stays off the plinth, the lanterns and the kerbs
                for (int e = -1; e <= 1; e += 2)
                {
                    EndingPath.World(s, e > 0 ? hi : lo, out double x, out _, out double z);
                    Assert.Greater(x, Beat08Ending.PlazaX0 + 0.3 + BulwarkConst.PlayerR, "west kerb");
                    Assert.Less(z, Beat08Ending.PlazaZ1 - 0.3 - BulwarkConst.PlayerR, "north kerb");
                }
            }
            // the walker stays clear of the Keeper's Lantern (base 0.4 m) at the rest
            double dx = EndingPath.KeeperX - EndingPath.RestX, dz = EndingPath.KeeperZ - EndingPath.RestZ;
            Assert.Greater(Math.Sqrt(dx * dx + dz * dz), BulwarkConst.PlayerR + 0.3, "lantern beyond the capsule");
            Assert.AreEqual(90.0, EndingPath.HeadingDeg(EndingPath.SRest), 1e-6, "facing east at the rest");
        }

        [Test]
        public void FinalLook_FramesTheWholeFace()
        {
            // KS28: from the rest eye, pitch 28 at vFOV 66: the crest below the top with >= 3 % margin at 16:9 1080p
            double eyeY = EndingPath.EyeY(EndingPath.SRest);
            double d = BulwarkConst.LeeFaceX - EndingPath.RestX;
            double crestEl = Math.Atan2(BulwarkConst.RoadY - eyeY, d) * 180.0 / Math.PI;
            Assert.AreEqual(59.9, crestEl, 0.2, "crest at 59.9 deg");
            double top = BulwarkConst.EndLookPitch + 33.0;
            double pxPerDeg = 1080.0 / (2.0 * Math.Atan(Math.Tan(33.0 * Math.PI / 180.0)) * 180.0 / Math.PI);
            // exact projection of the crest row at pitch 28 (the image plane is not linear in angle)
            double t = Math.Tan((crestEl - BulwarkConst.EndLookPitch) * Math.PI / 180.0) / Math.Tan(33.0 * Math.PI / 180.0);
            double row = 540.0 - 540.0 * t;
            Assert.Greater(row, 0.03 * 1080.0, "crest at row " + row.ToString("0.0") + " inside the 3 % margin");
            Assert.Greater(top, crestEl);
            Assert.Greater(pxPerDeg, 0);
            // the wall base: the foot 3480 m away at -0.04 deg sits inside the frame bottom (-5 deg)
            double baseEl = Math.Atan2(BulwarkConst.LowlandY - eyeY, d) * 180.0 / Math.PI;
            Assert.Greater(baseEl, BulwarkConst.EndLookPitch - 33.0, "the foot of the wall in the frame");
        }

        [Test]
        public void ReturnCar_ClimbsFromTheFootAfterTheWalkerLeft()
        {
            Assert.Less(Beat08Ending.ReturnCarAltitude(EndingPath.SFoot0, -1.0), 0.0, "still at the platform with the walker");
            Assert.AreEqual(0.0, Beat08Ending.ReturnCarAltitude(EndingPath.SFoot0 + 2.0, -1.0), 1e-9, "departs");
            double prev = -1.0;
            for (double tg = 0.0; tg <= 20.0; tg += 0.5)
            {
                double h = Beat08Ending.ReturnCarAltitude(EndingPath.SRest, tg);
                Assert.GreaterOrEqual(h, prev);
                prev = h;
            }
            double h28 = Beat08Ending.ReturnCarAltitude(EndingPath.SRest, 8.5);
            Assert.That(h28, Is.InRange(1200.0, 1700.0), "a third of the way up the rail line in KS28");
        }

        [Test]
        public void Stills_HavePosesInsideTheCabinOrOnThePlatform()
        {
            var list = Beat08Ending.Stills();
            Assert.GreaterOrEqual(list.Count, 10);
            foreach (var st in list)
            {
                if (st.s > BwPath.Length && st.s < EndingPath.SFoot0)
                {
                    EndingPath.CabinOffset(st.s, out double dx, out double dy);
                    double lx = st.x - dx, ly = st.y - dy;
                    Assert.That(lx, Is.InRange(BulwarkConst.CabinX0 + 0.3, BulwarkConst.CabinX1 - 0.3), st.tag + " x inside the cabin");
                    Assert.That(st.z, Is.InRange(BulwarkConst.CabinZ0 + 0.3, BulwarkConst.CabinZ1 - 0.3), st.tag + " z inside the cabin");
                    Assert.AreEqual(EndingPath.FloorTopY + BulwarkConst.Eye, ly, 1e-3, st.tag + " eye 1.7 above the cabin floor");
                }
                Assert.AreEqual(st.tag.EndsWith("_zoom") ? 8.0 : 66.0, st.fov, 1e-9, st.tag + ": the game vFOV (the _zoom QA stills: 8 deg)");
            }
        }
    }
}
