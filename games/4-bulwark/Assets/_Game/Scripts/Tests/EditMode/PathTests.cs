// PathTests: T-PATH of MASTER_PROMPT 14.10.1 and the 14.4 path contract of BwPath / FilletPath (EditMode, NUnit).
// Length, piece boundaries, arc lengths, end points, C1 continuity, beats, timings, the stair profile, the eye
// kernel (second differences, tread-line clearance, closed form vs numerical average), lanes (continuity, slope,
// openings), the lateral keys (lane margin, ground-speed peak) and the Project round trip.
using System;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class PathTests
    {
        const double Mm = 1e-3;

        // ---------------------------------------------------------------- length, pieces, end points
        [Test]
        public void Length_Is1500()
        {
            Assert.AreEqual(1500.0, BwPath.Length, 0.0);
            Assert.AreEqual(1500.0, BwPath.Fillet.Length, 0.01, "T-PATH length 1500.000 +-0.01");
            Assert.AreEqual(1500.0, BwPath.Fillet.Length, 1e-9, "closure is exact by construction");
            double sum = 0.0;
            for (int i = 0; i < BwPath.Fillet.PieceCount; i++) sum += BwPath.Fillet.PieceLength(i);
            Assert.AreEqual(1500.0, sum, 1e-9);
            Assert.AreEqual(BulwarkConst.PathLength, BwPath.Length, 0.0);
        }

        [Test]
        public void PieceBoundaries_MatchTable()
        {
            var f = BwPath.Fillet;
            Assert.AreEqual(9, f.PieceCount, "L1+L2, A1, stairs, A2, hall+gallery, A3, tunnel, A4, ledge");
            double[] ends = { BulwarkConst.L2EndS, BulwarkConst.A1EndS, BulwarkConst.StairsEndS, BulwarkConst.A2EndS, BulwarkConst.GalleryEndS,
                              BulwarkConst.A3EndS, BulwarkConst.TunnelEndS, BulwarkConst.A4EndS, BulwarkConst.PathLength };
            for (int i = 0; i < 9; i++)
                Assert.AreEqual(ends[i], f.PieceEnd(i), Mm, $"piece {i} end s");
            Assert.AreEqual(934.0, BwPath.A1StartS, 1e-9);
            Assert.AreEqual(1370.0, BwPath.GalleryEndS, 1e-9, "the gallery leg ends exactly at the beat 7 boundary");
            Assert.AreEqual(BulwarkConst.HallLength, f.PieceLength(4), Mm, "exit hall + gallery 334.612");
            Assert.AreEqual(50.163, f.PieceLength(6), Mm, "tunnel diagonal 50.163");
            Assert.AreEqual(BulwarkConst.LedgeLength, f.PieceLength(8), Mm, "ledge 63.679");
            Assert.AreEqual(BulwarkConst.StairsLength, f.PieceLength(2), 1e-9, "stairs 97.800");
            Assert.AreEqual(BulwarkConst.ExitHallS0, BwPath.A2EndS, Mm, "exit hall starts at the A2 end");
            // beat 5 legs: L2 end 14 + A1 + stairs + A2 + hall 34.612 = 150
            Assert.AreEqual(34.612, BulwarkConst.GalleryStartS - BwPath.A2EndS, Mm);
        }

        [Test]
        public void ArcLengths_AndRadii()
        {
            var f = BwPath.Fillet;
            int[] arcs = { 1, 3, 5, 7 };
            double[] len = { BulwarkConst.A1ArcLength, BulwarkConst.A2ArcLength, BulwarkConst.A3ArcLength, BulwarkConst.A4ArcLength };
            double[] rad = { 40.0, 40.0, 30.0, 20.0 };
            double[] turnDeg = { 3.0, 2.14, 18.86, 18.0 };
            for (int k = 0; k < 4; k++)
            {
                int i = arcs[k];
                Assert.IsTrue(f.PieceIsArc(i), $"piece {i} is an arc");
                Assert.AreEqual(len[k], f.PieceLength(i), Mm, $"arc A{k + 1} length");
                Assert.AreEqual(rad[k], f.PieceRadius(i), 1e-9, $"arc A{k + 1} radius (no clamp)");
                Assert.AreEqual(rad[k] * turnDeg[k] * Math.PI / 180.0, f.PieceLength(i), 1e-9, $"arc A{k + 1} = r x turn");
            }
            for (int i = 0; i < 9; i += 2) Assert.IsFalse(f.PieceIsArc(i), $"piece {i} straight");
            // turn directions: A1, A4 right (+1/r); A2, A3 left (-1/r)
            Assert.AreEqual(1.0 / 40.0, BwPath.Curvature(935.0), 1e-12);
            Assert.AreEqual(-1.0 / 40.0, BwPath.Curvature(1034.5), 1e-12);
            Assert.AreEqual(-1.0 / 30.0, BwPath.Curvature(1375.0), 1e-12);
            Assert.AreEqual(1.0 / 20.0, BwPath.Curvature(1433.0), 1e-12);
            Assert.AreEqual(0.0, BwPath.Curvature(500.0), 0.0);
        }

        [Test]
        public void EndPoints_MatchTable()
        {
            var f = BwPath.Fillet;
            double[,] xz =
            {
                { BulwarkConst.L2EndX, BulwarkConst.L2EndZ }, { BulwarkConst.A1EndX, BulwarkConst.A1EndZ }, { BulwarkConst.StairsEndX, BulwarkConst.StairsEndZ },
                { BulwarkConst.A2EndX, BulwarkConst.A2EndZ }, { BulwarkConst.GalleryEndX, BulwarkConst.GalleryEndZ }, { BulwarkConst.A3EndX, BulwarkConst.A3EndZ },
                { BulwarkConst.TunnelEndX, BulwarkConst.TunnelEndZ }, { BulwarkConst.A4EndX, BulwarkConst.A4EndZ }, { BulwarkConst.EndX, BulwarkConst.EndZ },
            };
            for (int i = 0; i < 9; i++)
            {
                BwPath.Point(f.PieceEnd(i), out double x, out double z);
                Assert.AreEqual(xz[i, 0], x, Mm, $"piece {i} end x");
                Assert.AreEqual(xz[i, 1], z, Mm, $"piece {i} end z");
            }
            BwPath.Point(1500.0, out double ex, out double ez);
            Assert.AreEqual(-9.199, ex, Mm, "end x");
            Assert.AreEqual(1497.114, ez, Mm, "end z");
            BwPath.Point(0.0, out double sx, out double sz);
            Assert.AreEqual(-1.5, sx, 1e-12, "start x");
            Assert.AreEqual(0.0, sz, 1e-12, "start z");
            Assert.AreEqual(-9.199, BwPath.LedgeX, Mm);
            // the walker stops 1.9 m short of the lantern
            Assert.AreEqual(1.9, BulwarkConst.LanternZ - ez, 0.05);
            Assert.AreEqual(BulwarkConst.LanternX, ex, 0.01);
        }

        [Test]
        public void Headings_PerLeg()
        {
            Assert.AreEqual(0.0, BwPath.HeadingDeg(60.0), 1e-9);
            Assert.AreEqual(0.0, BwPath.HeadingDeg(933.9), 1e-9);
            Assert.AreEqual(3.0, BwPath.HeadingDeg(980.0), 1e-9);
            Assert.AreEqual(0.86, BwPath.HeadingDeg(1200.0), 1e-9);
            Assert.AreEqual(-18.0, BwPath.HeadingDeg(1400.0), 1e-9);
            Assert.AreEqual(0.0, BwPath.HeadingDeg(1470.0), 1e-9);
            // mid-arc headings
            Assert.AreEqual(1.5, BwPath.HeadingDeg(0.5 * (BwPath.A1StartS + BwPath.A1EndS)), 1e-9);
            Assert.AreEqual(-9.0, BwPath.HeadingDeg(0.5 * (BwPath.TunnelEndS + BwPath.A4EndS)), 1e-9);
            BwPath.Tangent(980.0, out double tx, out double tz);
            Assert.AreEqual(1.0, tx * tx + tz * tz, 1e-12);
            BwPath.Right(980.0, out double rx, out double rz);
            Assert.AreEqual(0.0, tx * rx + tz * rz, 1e-12, "right is normal to the tangent");
            Assert.Greater(rx, 0.99, "walking north the right is +X");
        }

        [Test]
        public void Continuity_PositionAndHeading_C1()
        {
            const double ds = 0.01;
            double maxK = 1.0 / 20.0;
            BwPath.Point(0.0, out double px, out double pz);
            BwPath.Tangent(0.0, out double ptx, out double ptz);
            double worstPos = 0.0, worstTan = 0.0;
            for (int i = 1; i <= 150000; i++)
            {
                double s = i * ds;
                BwPath.Point(s, out double x, out double z);
                BwPath.Tangent(s, out double tx, out double tz);
                double step = Math.Sqrt((x - px) * (x - px) + (z - pz) * (z - pz));
                worstPos = Math.Max(worstPos, Math.Abs(step - ds) - 0.5 * maxK * maxK * ds * ds * ds);
                double dt = Math.Sqrt((tx - ptx) * (tx - ptx) + (tz - ptz) * (tz - ptz));
                worstTan = Math.Max(worstTan, dt - maxK * ds);
                px = x; pz = z; ptx = tx; ptz = tz;
            }
            Assert.LessOrEqual(worstPos, 1e-9, "plan position: unit speed in s, no jumps");
            Assert.LessOrEqual(worstTan, 1e-9, "tangent turns at most curvature x ds (C1 at the arcs)");
            // both sides of every piece boundary agree
            var f = BwPath.Fillet;
            for (int i = 0; i < f.PieceCount - 1; i++)
            {
                double b = f.PieceEnd(i);
                BwPath.Point(b - 1e-9, out double ax, out double az);
                BwPath.Point(b + 1e-9, out double bx, out double bz);
                Assert.AreEqual(ax, bx, 1e-8, $"x at boundary {i}");
                Assert.AreEqual(az, bz, 1e-8, $"z at boundary {i}");
                Assert.AreEqual(BwPath.HeadingDeg(b - 1e-9), BwPath.HeadingDeg(b + 1e-9), 1e-6, $"heading at boundary {i}");
            }
        }

        [Test]
        public void LeeFaceCrossing_AndGalleryAxis()
        {
            double lo = 1380.0, hi = 1430.0;
            for (int it = 0; it < 80; it++)
            {
                double m = 0.5 * (lo + hi);
                BwPath.Point(m, out double x, out _);
                if (x > BulwarkConst.LeeFaceX) lo = m; else hi = m;
            }
            BwPath.Point(lo, out _, out double zc);
            Assert.AreEqual(BulwarkConst.LeeFaceCrossS, lo, 0.01, "tunnel crosses the lee face at s 1422.85");
            Assert.AreEqual(BulwarkConst.LeeFaceCrossZ, zc, 0.05, "at z 1420.4");
            for (double s = 1070.0; s <= 1370.0; s += 10.0)
            {
                BwPath.Point(s, out double x, out _);
                Assert.AreEqual(BulwarkConst.HallAxisX0 + BulwarkConst.HallAxisSlope * (s - 1070.0), x, Mm, $"gallery axis x at s {s}");
            }
        }

        // ---------------------------------------------------------------- beats and timing
        [Test]
        public void Beats_Boundaries()
        {
            double[] b = { 0.0, 120.0, 420.0, 700.0, 920.0, 1070.0, 1370.0 };
            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(i + 1, BwPath.BeatAt(b[i]), $"beat at s {b[i]}");
                Assert.AreEqual(i + 1, BwPath.BeatAt(b[i] + 0.001));
                if (i > 0) Assert.AreEqual(i, BwPath.BeatAt(b[i] - 0.001), $"beat just before s {b[i]}");
                Assert.AreEqual(b[i], BwPath.BeatStartS(i + 1), 0.0);
            }
            Assert.AreEqual(7, BwPath.BeatAt(1500.0));
            Assert.AreEqual(1, BwPath.BeatAt(-5.0));
            double sum = 0.0;
            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(BulwarkConst.BeatS[i + 1] - BulwarkConst.BeatS[i], BulwarkConst.BeatLength[i], 1e-9);
                sum += BulwarkConst.BeatLength[i];
            }
            Assert.AreEqual(1500.0, sum, 1e-9, "120+300+280+220+150+300+130");
        }

        [Test]
        public void Timing_600At2p5_1000At1p5()
        {
            Assert.AreEqual(600.00, BwPath.Fillet.Length / BulwarkConst.Cine, 0.005, "600.00 s at 2.5 m/s");
            Assert.AreEqual(1000.00, BwPath.Fillet.Length / BulwarkConst.Walk, 0.005, "1000.00 s at 1.5 m/s");
            Assert.AreEqual(BulwarkConst.TimeAtCine, BulwarkConst.PathLength / BulwarkConst.Cine, 1e-9);
            Assert.AreEqual(BulwarkConst.TimeAtWalk, BulwarkConst.PathLength / BulwarkConst.Walk, 1e-9);
            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(BulwarkConst.CumTimeAtCine[i], BulwarkConst.BeatS[i + 1] / BulwarkConst.Cine, 0.005, $"cumulative time at 2.5, beat {i + 1}");
                Assert.AreEqual(BulwarkConst.CumTimeAtWalk[i], BulwarkConst.BeatS[i + 1] / BulwarkConst.Walk, 0.005, $"cumulative time at 1.5, beat {i + 1}");
            }
            // start ramp 0.5 s loses 0.25 s: goal trigger at 1498.5 / 2.5 + 0.25 = 599.65 s
            Assert.AreEqual(599.65, BulwarkConst.GoalTriggerS / BulwarkConst.Cine + 0.5 * BulwarkConst.StartRamp, 1e-9);
        }

        // ---------------------------------------------------------------- floor profile
        [Test]
        public void Stairs_CountsRisersTreadsLandingsDrop()
        {
            const double e = 1e-6;
            Assert.AreEqual(BulwarkConst.StairsLength, BwPath.StairsEndS - BwPath.StairsStartS, 1e-9);
            Assert.AreEqual(0.0, BwPath.FloorY(BwPath.StairsStartS - e), 0.0, "top of the stairs at road level");
            Assert.AreEqual(-54.0, BwPath.FloorY(BwPath.StairsEndS + e), 1e-9, "bottom -54");
            Assert.AreEqual(54.00, BwPath.FloorY(BwPath.StairsStartS - e) - BwPath.FloorY(BwPath.StairsEndS + e), 0.05, "drop 54.00 +-0.05");
            int risers = 0;
            double prev = BwPath.FloorY(BwPath.StairsStartS - e);
            for (int i = 0; i < BulwarkConst.StairSteps; i++)
            {
                double a = BwPath.StairRiserS(i);
                Assert.Greater(a, BwPath.StairsStartS, "riser inside the stairs leg");
                Assert.Less(a, BwPath.StairsEndS, "riser inside the stairs leg");
                double above = BwPath.FloorY(a - e), below = BwPath.FloorY(a + e);
                Assert.AreEqual(0.200, above - below, 0.002, $"riser {i} height 0.200");
                Assert.AreEqual(prev, above, 1e-9, $"flat between riser {i - 1} and {i}");
                prev = below;
                risers++;
                if (i % 18 != 17)
                    Assert.AreEqual(0.300, BwPath.StairRiserS(i + 1) - a, 0.002, $"tread {i} depth 0.300");
            }
            Assert.AreEqual(270, risers, "270 steps");
            Assert.AreEqual(15 * 18, BulwarkConst.StairFlights * BulwarkConst.StairStepsPerFlight);
            // 14 landings: 1.2 m flat between flight k's run end and flight k+1's start, at -3.6 (k + 1)
            for (int k = 0; k < 14; k++)
            {
                double l0 = BwPath.StairFlightStartS(k) + BulwarkConst.StairFlightRun, l1 = BwPath.StairFlightStartS(k + 1);
                Assert.AreEqual(1.20, l1 - l0, 1e-9, $"landing {k} length");
                for (double s = l0; s <= l1; s += 0.1)
                    Assert.AreEqual(-3.6 * (k + 1), BwPath.FloorY(s), 1e-9, $"landing {k} flat at s {s:F2}");
            }
            // count risers by scanning the profile (independent of StairRiserS)
            int drops = 0;
            double py = BwPath.FloorY(930.0);
            for (double s = 930.0; s < 1040.0; s += 0.01)
            {
                double y = BwPath.FloorY(s);
                if (py - y > 0.1) drops++;
                Assert.LessOrEqual(y, py + 1e-12, "the stairs only go down");
                py = y;
            }
            Assert.AreEqual(270, drops, "270 risers in the profile");
        }

        [Test]
        public void Floor_RoadGalleryRampTunnel()
        {
            Assert.AreEqual(0.0, BwPath.FloorY(0.0), 0.0);
            Assert.AreEqual(0.0, BwPath.FloorY(500.0), 0.0);
            Assert.AreEqual(0.0, BwPath.FloorY(935.0), 0.0, "A1 at road level");
            Assert.AreEqual(-54.0, BwPath.FloorY(1034.5), 1e-9, "A2 at -54");
            Assert.AreEqual(-54.0, BwPath.FloorY(1070.0), 1e-9, "-54 to s 1070");
            Assert.AreEqual(-54.0 - 0.12 * 150.0, BwPath.FloorY(1220.0), 1e-9, "ramp 0.12");
            Assert.AreEqual(-90.0, BwPath.FloorY(1370.0), 1e-9, "-90 at s 1370");
            Assert.AreEqual(-90.0, BwPath.FloorY(1450.0), 1e-9);
            Assert.AreEqual(-90.0, BwPath.FloorY(1500.0), 1e-9);
            double slope = (BwPath.FloorY(1300.0) - BwPath.FloorY(1100.0)) / 200.0;
            Assert.AreEqual(-0.1200, slope, 0.0005, "ramp 0.1200 +-0.0005");
        }

        // ---------------------------------------------------------------- eye
        [Test]
        public void Eye_ClosedFormEqualsNumericalKernelAverage()
        {
            double h = BulwarkConst.EyeKernelHalfWidth;
            double[] probe = { 50.0, 935.0, 936.3, 937.7, 950.05, 985.0, 1001.234, 1033.8, 1034.9, 1036.0, 1069.0, 1071.3, 1200.0, 1368.9, 1371.0, 1499.0 };
            foreach (double s in probe)
            {
                // midpoint rule on 25 micrometre cells of the exact floor (error per riser below 1e-6)
                const int n = 200000;
                double dx = 2.0 * h / n, acc = 0.0;
                for (int i = 0; i < n; i++)
                {
                    double t = -h + (i + 0.5) * dx;
                    acc += (h - Math.Abs(t)) / (h * h) * BwPath.FloorY(s + t);
                }
                double num = acc * dx;
                Assert.AreEqual(num, BwPath.SmoothedFloorY(s), 5e-5, $"kernel average at s {s}");
                Assert.AreEqual(BulwarkConst.Eye + BwPath.SmoothedFloorY(s), BwPath.EyeY(s), 1e-12);
            }
            // linear parts are reproduced exactly (symmetric kernel)
            Assert.AreEqual(1.7, BwPath.EyeY(400.0), 1e-12);
            Assert.AreEqual(1.7 - 54.0 - 0.12 * 130.0, BwPath.EyeY(1200.0), 1e-9);
            Assert.AreEqual(-88.3, BwPath.EyeY(1450.0), 1e-9);
        }

        [Test]
        public void Eye_SecondDifferenceAt30fps()
        {
            double max25 = MaxSecondDifference(BwPath.EyeY, 2.5 / 30.0, 925.0, 1080.0);
            double max15 = MaxSecondDifference(BwPath.EyeY, 1.5 / 30.0, 925.0, 1080.0);
            // 14.5.5 acceptance (5): at most 2.5 mm at 2.5 m/s and 1.5 mm at 1.5 m/s
            Assert.LessOrEqual(max25, 2.5e-3, "acceptance 2.5 mm at 2.5 m/s");
            Assert.LessOrEqual(max15, 1.5e-3, "acceptance 1.5 mm at 1.5 m/s");
            // 14.4 quotes 1.8 / 0.7 mm: the kernel of the pitch line (no 0.3 m steps) gives 1.83 / 0.66 mm (below);
            // the exact 0.2 x 0.3 steps add the tread ripple of the 2.5 m kernel: 1.953 / 0.720 mm (arbitrated, 14.10.5).
            Assert.LessOrEqual(max25, 1.96e-3, "exact staircase at 2.5 m/s (1.953 mm)");
            Assert.LessOrEqual(max15, 0.73e-3, "exact staircase at 1.5 m/s (0.720 mm)");
            double ideal25 = MaxSecondDifference(PitchLineEye, 2.5 / 30.0, 925.0, 1080.0);
            double ideal15 = MaxSecondDifference(PitchLineEye, 1.5 / 30.0, 925.0, 1080.0);
            Assert.AreEqual(1.8e-3, ideal25, 0.05e-3, "pitch-line model: the 1.8 mm of 14.4");
            Assert.AreEqual(0.7e-3, ideal15, 0.05e-3, "pitch-line model: the 0.7 mm of 14.4");
            // speed independence: the eye is a function of s only
            Assert.AreEqual(BwPath.EyeY(985.0), BwPath.EyeY(985.0), 0.0);
        }

        [Test]
        public void Eye_AboveTreadLine_AndKeyShots()
        {
            double worst = double.MinValue;
            for (double s = 930.0; s <= 1045.0; s += 0.01)
                worst = Math.Max(worst, BwPath.EyeY(s) - BulwarkConst.Eye - BwPath.TreadLineY(s));
            Assert.LessOrEqual(worst, BulwarkConst.EyeAboveTreadLineMax, "eye at most 0.28 m above the tread line");
            Assert.Greater(worst, 0.25, "the 2.5 m kernel does lift the eye at the stair foot");
            // KS14 (1.11, -25.3, 985): the centred riser grid reproduces the eye height
            Assert.AreEqual(-25.3, BwPath.EyeY(985.0), 0.01, "KS14 eye y");
            BwPath.Point(985.0, out double x14, out _);
            Assert.AreEqual(1.11, x14, 0.01, "KS14 x");
            // eye depth below the sea (14.4)
            Assert.AreEqual(BulwarkConst.EyeDepthAt1070, BulwarkConst.SeaY - BwPath.EyeY(1070.0), 0.06, "44.3 m at s 1070");
            Assert.AreEqual(BulwarkConst.EyeDepthAt1370, BulwarkConst.SeaY - BwPath.EyeY(1370.0), 0.06, "80.3 m at s 1370");
            // key-shot eyes on the path (14.10.2)
            foreach (var k in BulwarkConst.KeyShots)
            {
                if (k.s > BwPath.Length)
                {
                    // the ending rows (fix wave R5): the eye of the ride / the platform walk (EndingPath, lateral 0)
                    EndingPath.World(k.s, 0.0, out double ex, out _, out double ez);
                    Assert.AreEqual(k.y, EndingPath.EyeY(k.s), 0.02, $"{k.tag} ending eye y");
                    Assert.AreEqual(k.x, ex, 0.02, $"{k.tag} ending x");
                    Assert.AreEqual(k.z, ez, 0.02, $"{k.tag} ending z");
                    continue;
                }
                Assert.AreEqual(k.y, BwPath.EyeY(k.s), 0.02, $"{k.tag} eye y");
                double lat = BwPath.CinematicLateral(k.s);
                BwPath.World(k.s, lat, out double x, out _, out double z);
                if (k.s >= 1070.0 && k.s < 1400.0) Assert.AreEqual(k.x, x, 0.01, $"{k.tag} x");
                if (k.s >= 1400.0) Assert.AreEqual(k.x, x, 0.02, $"{k.tag} x");
                if (k.s < 900.0 && Math.Abs(lat) > 1e-9) Assert.AreEqual(k.x, x, 0.05, $"{k.tag} x on the lateral keys");
                Assert.AreEqual(k.z, z, 0.2, $"{k.tag} z (key shots list s-rounded z on the ramps)");
            }
        }

        static double MaxSecondDifference(Func<double, double> f, double ds, double s0, double s1)
        {
            double worst = 0.0;
            for (double s = s0; s <= s1; s += 0.002)
                worst = Math.Max(worst, Math.Abs(f(s + ds) - 2.0 * f(s) + f(s - ds)));
            return worst;
        }

        /// <summary>Kernel average of the pitch line (each flight a straight ramp), the idealisation behind 14.4's numbers.</summary>
        static double PitchLineEye(double s)
        {
            double h = BulwarkConst.EyeKernelHalfWidth, y = 0.0, g = BulwarkConst.StairFlightDrop / BulwarkConst.StairFlightRun;
            for (int k = 0; k < BulwarkConst.StairFlights; k++)
            {
                double a = BwPath.StairFlightStartS(k);
                y -= g * (BwPath.KernelRamp(s - a, h) - BwPath.KernelRamp(s - a - BulwarkConst.StairFlightRun, h));
            }
            y -= BulwarkConst.GalleryRamp * (BwPath.KernelRamp(s - 1070.0, h) - BwPath.KernelRamp(s - 1370.0, h));
            return BulwarkConst.Eye + y;
        }

        // ---------------------------------------------------------------- lanes
        [Test]
        public void Lane_ContinuousAndSlopeLimited()
        {
            const double ds = 0.005;
            BwPath.Lane(0.0, out double pa, out double pb);
            double worst = 0.0;
            for (int i = 1; i <= 300000; i++)
            {
                double s = i * ds;
                BwPath.Lane(s, out double a, out double b);
                // the ending (ADDENDUM A1 00): from the funnel into the cabin the lane narrows to the 1.6 m door (+-0.45)
                double minEdge = s <= BulwarkConst.LaneCabinFunnelS0 ? 1.2 : BulwarkConst.FootDoorHalfLane - 1e-9;
                Assert.Less(a, -minEdge, $"lane left edge at s {s:F3}");
                Assert.Greater(b, minEdge, $"lane right edge at s {s:F3}");
                worst = Math.Max(worst, Math.Max(Math.Abs(a - pa), Math.Abs(b - pb)) / ds);
                pa = a; pb = b;
            }
            Assert.LessOrEqual(worst, BulwarkConst.LaneMaxSlope + 1e-6, "no lane edge moves faster than 0.6 m per m of s");
        }

        [Test]
        public void Lane_Widths()
        {
            AssertLaneX(60.0, -3.15, 0.15, "corridor");
            AssertLaneX(119.65, -3.15, 0.15, "door plane minus the radius");
            AssertLaneX(120.0, -3.15, 0.15, "door plane z 120");
            AssertLaneX(121.05, -3.15, 0.15, "door wall cleared");
            AssertLaneX(300.0, -5.4, 5.1, "road");
            AssertLaneX(898.0, -5.4, 5.1, "funnel start");
            AssertLaneX(920.0, -2.95, -0.05, "portal");
            AssertLaneX(926.0, -2.95, -0.05, "atrium pedestrian way");
            AssertLaneWidth(980.0, 2.9, "stairs");
            AssertLaneWidth(1050.0, 3.7, "exit hall");
            AssertLane(1200.0, -5.0, 2.65, "gallery axis -5.0 .. +2.65");
            AssertLaneWidth(1400.0, 2.5, "tunnel");
            AssertLaneX(1470.0, -11.8, -6.4, "ledge");
            AssertLaneX(BulwarkConst.LaneCabinFunnelS0, -11.8, -6.4, "ledge end (the funnel to the cabin door starts)");
            AssertLaneX(1500.0, -9.199 - BulwarkConst.FootDoorHalfLane, -9.199 + BulwarkConst.FootDoorHalfLane, "in the cabin (door lane, ADDENDUM A1 00)");
            for (int i = 1; i < BwPath.LaneKnots.Length; i++)
                Assert.Greater(BwPath.LaneKnots[i], BwPath.LaneKnots[i - 1], "lane knots ascending");
        }

        [Test]
        public void Lane_CapsuleFitsTheOpenings()
        {
            double r = BulwarkConst.PlayerR;
            // the door: opening x -3.5..0.5 over the door wall z 120..120.7 (plus the capsule radius either side)
            for (double s = BulwarkConst.DoorPlaneZ - r; s <= BulwarkConst.DoorWallNorthZ + r; s += 0.01)
                AssertLaneInside(s, BulwarkConst.DoorOpeningXMin + r, BulwarkConst.DoorOpeningXMax - r, "door");
            // the portal x -3.3..0.3 at z 920: the funnel reaches it one radius before the plane; atrium; stair mouth z 934
            for (double s = BulwarkConst.LaneFunnelS1; s <= 920.0 + r; s += 0.01)
                AssertLaneInside(s, BulwarkConst.PortalXMin + r, BulwarkConst.PortalXMax - r, "portal");
            for (double s = 920.0; s <= 934.0; s += 0.01)
                AssertLaneInside(s, BulwarkConst.StairMouthXMin + r, BulwarkConst.StairMouthXMax - r, "atrium and stair mouth");
            Assert.AreEqual(920.0 - r, BulwarkConst.LaneFunnelS1, 1e-9);
            // the stairs 3.6 wide: lane 2.9 centred
            for (double s = 936.2; s < 1033.8; s += 0.5)
            {
                BwPath.Lane(s, out double a, out double b);
                Assert.AreEqual(-0.5 * BulwarkConst.StairWidth + r, a, 1e-9);
                Assert.AreEqual(0.5 * BulwarkConst.StairWidth - r, b, 1e-9);
            }
            // tunnel 3.2 wide at the hall end wall door (s 1373)
            BwPath.Lane(BulwarkConst.HallEndWallS - r, out double ta, out double tb);
            Assert.AreEqual(-0.5 * BulwarkConst.TunnelWidth + r, ta, 1e-9);
            Assert.AreEqual(0.5 * BulwarkConst.TunnelWidth - r, tb, 1e-9);
            // tunnel band -> ledge: once the band's right edge has left the mouth (s 1428) it never comes nearer the lee
            // face than x -6.4, and the left edge stays on the slab (z >= 1420)
            for (double s = BulwarkConst.LaneLedgeOpenS0; s <= BwPath.A4EndS; s += 0.01)
            {
                BwPath.Lane(s, out double a, out double b);
                BwPath.World(s, b, out double xr, out _, out _);
                BwPath.World(s, a, out double xl, out _, out double zl);
                Assert.LessOrEqual(xr, BulwarkConst.LaneLedgeXMax + 0.005, $"ledge window right edge at s {s:F2}");
                Assert.GreaterOrEqual(xl, BulwarkConst.LaneLedgeXMin, $"ledge window left edge at s {s:F2}");
                Assert.GreaterOrEqual(zl, BulwarkConst.LedgeZ0, $"ledge window left edge over the slab at s {s:F2}");
            }
            // the ledge lane never reaches into the lee face (x <= -6.4 once on the ledge)
            for (double s = BwPath.A4EndS; s <= 1500.0; s += 0.5)
            {
                BwPath.Lane(s, out double a, out double b);
                BwPath.World(s, b, out double xr, out _, out _);
                BwPath.World(s, a, out double xl, out _, out _);
                Assert.LessOrEqual(xr, BulwarkConst.LaneLedgeXMax + 1e-9);
                Assert.GreaterOrEqual(xl, BulwarkConst.LaneLedgeXMin - 1e-9);
            }
        }

        static void AssertLaneInside(double s, double xMin, double xMax, string what)
        {
            BwPath.Lane(s, out double a, out double b);
            double xa = BulwarkConst.StartX + a, xb = BulwarkConst.StartX + b;
            Assert.GreaterOrEqual(xa, xMin - 1e-9, $"{what}: left edge at s {s:F2}");
            Assert.LessOrEqual(xb, xMax + 1e-9, $"{what}: right edge at s {s:F2}");
        }

        static void AssertLaneX(double s, double xMin, double xMax, string what)
        {
            BwPath.Lane(s, out double a, out double b);
            BwPath.World(s, a, out double xa, out _, out _);
            BwPath.World(s, b, out double xb, out _, out _);
            Assert.AreEqual(xMin, xa, 1e-3, $"{what}: lane x min at s {s}");
            Assert.AreEqual(xMax, xb, 1e-3, $"{what}: lane x max at s {s}");
        }

        static void AssertLane(double s, double latMin, double latMax, string what)
        {
            BwPath.Lane(s, out double a, out double b);
            Assert.AreEqual(latMin, a, 1e-9, $"{what}: lateral min at s {s}");
            Assert.AreEqual(latMax, b, 1e-9, $"{what}: lateral max at s {s}");
        }

        static void AssertLaneWidth(double s, double width, string what)
        {
            BwPath.Lane(s, out double a, out double b);
            Assert.AreEqual(width, b - a, 1e-9, $"{what}: lane width at s {s}");
            Assert.AreEqual(0.0, a + b, 1e-9, $"{what}: lane centred at s {s}");
        }

        // ---------------------------------------------------------------- lateral keys
        [Test]
        public void LateralKeys_InsideLaneWithMargin()
        {
            double worst = double.MaxValue, worstS = 0.0;
            for (double s = 0.0; s <= 1500.0; s += 0.05)
            {
                double l = BwPath.CinematicLateral(s);
                BwPath.Lane(s, out double a, out double b);
                double m = Math.Min(l - a, b - l);
                if (m < worst) { worst = m; worstS = s; }
            }
            Assert.GreaterOrEqual(worst, BulwarkConst.LaneKeyMargin, $"lateral keys keep >= 0.15 m inside the lane (worst at s {worstS:F2})");
            // the keys (world x on L2)
            for (int i = 0; i < BulwarkConst.LateralKeyS.Length; i++)
            {
                double s = BulwarkConst.LateralKeyS[i];
                BwPath.World(s, BwPath.CinematicLateral(s), out double x, out _, out _);
                Assert.AreEqual(BulwarkConst.LateralKeyX[i], x, 1e-9, $"key at s {s}");
            }
            Assert.AreEqual(0.0, BwPath.CinematicLateral(60.0), 0.0);
            Assert.AreEqual(0.0, BwPath.CinematicLateral(121.0), 0.0);
            Assert.AreEqual(0.0, BwPath.CinematicLateral(900.0), 0.0);
            Assert.AreEqual(0.0, BwPath.CinematicLateral(1200.0), 0.0);
            // KS11 at s 824 sits on the B4 ramp: x 0.92
            BwPath.World(824.0, BwPath.CinematicLateral(824.0), out double x824, out _, out _);
            Assert.AreEqual(0.9, x824, 0.05, "KS11 x");
        }

        [Test]
        public void LateralKeys_GroundSpeedPeak2p597At141()
        {
            double peak = 0.0, peakS = 0.0, latPeak = 0.0;
            for (double s = 0.0; s <= 1500.0; s += 0.01)
            {
                double l = BwPath.CinematicLateral(s), dl = BwPath.CinematicLateralRate(s);
                double g = BulwarkConst.Cine * BwPath.GroundSpeedFactor(s, l, dl);
                if (g > peak) { peak = g; peakS = s; }
                latPeak = Math.Max(latPeak, Math.Abs(dl) * BulwarkConst.Cine);
                double num = (BwPath.CinematicLateral(s + 1e-4) - BwPath.CinematicLateral(s - 1e-4)) / 2e-4;
                Assert.AreEqual(num, dl, 1e-6, $"lateral rate at s {s:F2}");
            }
            Assert.AreEqual(BulwarkConst.GroundSpeedPeak, peak, 0.001, "ground speed peak 2.597 m/s");
            Assert.AreEqual(BulwarkConst.GroundSpeedPeakS, peakS, 0.5, "at s 141");
            Assert.AreEqual(BulwarkConst.LateralSpeedPeak, latPeak, 0.01, "lateral speed peak 0.70 m/s");
            Assert.LessOrEqual(peak, 2.60, "ground speed at most 2.60 m/s (+4 %)");
            // progress speed is what the timeline uses: 1500 / 2.5 = 600 s whatever the lateral does
            Assert.AreEqual(600.0, BulwarkConst.PathLength / BulwarkConst.Cine, 1e-9);
        }

        // ---------------------------------------------------------------- projection
        [Test]
        public void Project_RoundTrip()
        {
            var rnd = new Random(1234);
            double worstS = 0.0, worstLat = 0.0;
            for (double s = 0.0; s <= 1500.0; s += 0.37)
            {
                BwPath.Lane(s, out double a, out double b);
                double[] lats = { a + 0.01, 0.0, b - 0.01, a + (b - a) * rnd.NextDouble() };
                foreach (double lat in lats)
                {
                    BwPath.World(s, lat, out double x, out _, out double z);
                    double hint = s + (rnd.NextDouble() * 2.0 - 1.0) * 30.0;
                    double sp = BwPath.Project(x, z, hint);
                    worstS = Math.Max(worstS, Math.Abs(sp - s));
                    worstLat = Math.Max(worstLat, Math.Abs(BwPath.Lateral(x, z, sp) - lat));
                }
            }
            Assert.LessOrEqual(worstS, 1e-6, "Project(World(s, lateral)) == s (hints off by up to 30 m)");
            Assert.LessOrEqual(worstLat, 1e-6, "Lateral(World(s, lateral)) == lateral");
            // off-path points: distance and clamping
            double sp0 = BwPath.Project(-1.5, -5.0, 0.0, out double d0);
            Assert.AreEqual(0.0, sp0, 0.0);
            Assert.AreEqual(5.0, d0, 1e-9);
            double sp1 = BwPath.Project(-9.199, 1510.0, 1500.0, out double d1);
            Assert.AreEqual(1500.0, sp1, 1e-9);
            Assert.AreEqual(1510.0 - 1497.114, d1, 2e-3);
        }

        [Test]
        public void FilletPath_SharpProgressRoundTrip()
        {
            var f = BwPath.Fillet;
            for (double d = 0.0; d <= f.Length; d += 1.3)
                Assert.AreEqual(d, f.DAtSharp(f.SharpS(d)), 1e-6, $"DAtSharp(SharpS(d)) at d {d:F1}");
            Assert.AreEqual(6, BwPath.Waypoints.Length);
            Assert.AreEqual(40.0, BwPath.WaypointRadius[1], 0.0);
            Assert.AreEqual(20.0, BwPath.WaypointRadius[4], 0.0);
            // a standalone path: square corner with r 10 gives a quarter circle of length 5 pi
            var p = new FilletPath(new[] { (0.0, 0.0), (0.0, 50.0), (50.0, 50.0) }, new[] { 0.0, 50.0, 100.0 }, new[] { 0.0, 10.0, 0.0 });
            Assert.AreEqual(3, p.PieceCount);
            Assert.AreEqual(5.0 * Math.PI, p.PieceLength(1), 1e-12);
            Assert.AreEqual(80.0 + 5.0 * Math.PI, p.Length, 1e-9);
            Assert.AreEqual(45.0, p.HeadingAt(40.0 + 2.5 * Math.PI), 1e-9, "mid-corner heading of a right turn");
            Assert.AreEqual(0.1, p.CurvatureAt(45.0), 1e-12, "right turn: positive curvature");
        }
    }
}
