// Beat05Tests: EditMode tests of beat 5, the gate tower atrium and the descent (MASTER_PROMPT 14.5.5 acceptance (1), (3),
// (5), 14.2.5 the portal lamp boost, Addendum A1 1.4 porthole outer faces): the pure layout (Beat05Layout) and the BwPath
// stair profile it builds on, against the spec numbers.
using System;
using Megastructure.Beats;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class Beat05Tests
    {
        const double Tol = 0.02;

        // ============================================================================ acceptance (1)
        [Test]
        public void Stairs_Have270StepsOf0p2By0p3In15FlightsWith14Landings()
        {
            Assert.AreEqual(270, BulwarkConst.StairSteps);
            Assert.AreEqual(15, BulwarkConst.StairFlights);
            Assert.AreEqual(14, Beat05Layout.Landings);
            for (int i = 0; i < BulwarkConst.StairSteps; i++)
            {
                double r = BwPath.StairRiserS(i);
                double drop = BwPath.FloorY(r - 1e-6) - BwPath.FloorY(r + 1e-6);
                Assert.AreEqual(0.200, drop, 0.002, "riser " + i);
                if (i % BulwarkConst.StairStepsPerFlight != 0)
                    Assert.AreEqual(0.300, r - BwPath.StairRiserS(i - 1), 0.002, "tread before riser " + i);
            }
            for (int k = 1; k <= Beat05Layout.Landings; k++)
            {
                Beat05Layout.Landing(k, out double a, out double b, out double y);
                Assert.AreEqual(1.20, b - a, 1e-9, "landing " + k);
                Assert.AreEqual(y, BwPath.FloorY(a + 0.01), 1e-9, "landing " + k + " floor");
                Assert.AreEqual(y, BwPath.FloorY(b - 0.01), 1e-9, "landing " + k + " floor (end)");
            }
            Assert.AreEqual(54.00, BwPath.FloorY(BwPath.StairsStartS - 0.01) - BwPath.FloorY(BwPath.StairsEndS + 0.01), 0.05, "drop 54");
            Assert.AreEqual(34.612, BulwarkConst.ExitHallS1 - BulwarkConst.ExitHallS0, Tol, "exit hall 34.612");
        }

        [Test]
        public void Stairwell_Is3p6Wide_HeadroomAtLeast2p4_ExitHall4p4By4p0()
        {
            double wellS = Beat05Layout.WellClosedS();
            double minHead = double.MaxValue;
            for (double s = BwPath.StairsStartS + 0.01; s < BwPath.StairsEndS - 0.01; s += 0.05)
            {
                double span = Beat05Layout.WallSpan(s);
                Assert.AreEqual(BulwarkConst.StairWidth, span, Tol, "stair width at s " + s.ToString("0.00"));
                if (s >= wellS) minHead = Math.Min(minHead, Beat05Layout.CeilingY(s) - BwPath.FloorY(s));
            }
            // under the closing slab's skirt (0.12 below the deck underside) at the closing line
            double skirt = Beat05Layout.WellDeckUnderside - Beat05Layout.WellSkirt - BwPath.FloorY(wellS);
            Assert.GreaterOrEqual(Math.Min(minHead, skirt), BulwarkConst.StairHeadroomMin, "headroom >= 2.4");
            for (double s = BulwarkConst.LaneHallOpenS1 + 0.5; s < BulwarkConst.ExitHallS1 - 1.0; s += 0.5)
            {
                Assert.AreEqual(BulwarkConst.ExitHallWidth, Beat05Layout.WallSpan(s), Tol, "exit hall width at " + s.ToString("0.0"));
                Assert.AreEqual(BulwarkConst.ExitHallHeight, Beat05Layout.CeilingY(s) - BwPath.FloorY(s), 1e-9, "exit hall height");
            }
            Assert.AreEqual(BulwarkConst.StairHandrailHeight, 1.00, 0.01, "handrail 1.00 above the tread line (axis + radius)");
        }

        // ============================================================================ acceptance (3): portholes
        [Test]
        public void Portholes_AreThreeAtLandings5_8_11WithTheSpecEyeDepthsAndReachTheSeaFace()
        {
            Assert.AreEqual(3, Beat05Layout.PortholeLanding.Length);
            for (int i = 0; i < 3; i++)
            {
                int k = Beat05Layout.PortholeLanding[i];
                double sc = Beat05Layout.LandingCentre(k);
                Assert.AreEqual(BulwarkConst.PortholeS[i], sc, 1.0, "porthole " + i + " s");
                double y = BwPath.FloorY(sc) + Beat05Layout.PortholeEye;
                Assert.AreEqual(BulwarkConst.PortholeEyeDepth[i], BulwarkConst.SeaY - y, 0.05, "eye depth below the mean sea");
                // the bore runs from the east wall to the sea face: the wall is never thicker than the face allows
                Beat05Layout.LaneEnvelope(sc, out _, out double hi);
                BwPath.World(sc, hi + Beat05Layout.WallOffset, out double wx, out _, out _);
                double len = Beat05Layout.SeaFaceX(y) - wx;
                Assert.Greater(len, BulwarkConst.PortholeDepth - 1e-6, "bore at least the 14.5.5 2.1 m deep");
                Assert.Less(len, 8.0, "bore length plausible");
            }
            Assert.AreEqual(6.0, Beat05Layout.SeaFaceX(-10.0), 1e-9);
            Assert.AreEqual(6.0 + 10.0 / 8.0, Beat05Layout.SeaFaceX(-30.0), 1e-9, "batter 1:8 below y -20");
            // deeper water is darker (luma 161 > 125 > 98)
            Assert.Greater(Beat05GateTower.PortholeIntensity[0], Beat05GateTower.PortholeIntensity[1]);
            Assert.Greater(Beat05GateTower.PortholeIntensity[1], Beat05GateTower.PortholeIntensity[2]);
        }

        [Test]
        public void Chevrons_CountTheLandingsAndWetStreaksGrowFromLanding6To14()
        {
            for (int k = 1; k <= 14; k++) Assert.AreEqual(k, Beat05Layout.Chevrons(k));
            Assert.AreEqual(0.0, Beat05Layout.Wetness(Beat05Layout.LandingCentre(3)), 1e-9);
            Assert.AreEqual(0.0, Beat05Layout.Wetness(Beat05Layout.LandingCentre(6)), 1e-9);
            Assert.AreEqual(1.0, Beat05Layout.Wetness(Beat05Layout.LandingCentre(14)), 1e-9);
            double prev = 0.0;
            for (double s = BwPath.StairsStartS; s < BwPath.StairsEndS; s += 0.5)
            {
                double w = Beat05Layout.Wetness(s);
                Assert.GreaterOrEqual(w, prev - 1e-12, "monotone");
                prev = w;
            }
        }

        [Test]
        public void Drip_FallsOnLanding9BesideTheChevronsAndInsideTheWalls()
        {
            int k = Beat05GateTower.DripLanding;
            Assert.AreEqual(9, k, "14.5.5: a drip at landing 9");
            double sc = Beat05Layout.LandingCentre(k);
            Beat05Layout.LaneEnvelope(sc, out _, out double hi);
            double patchR = Beat05GateTower.StainHalfLat;              // the stain quad's half width across the path (Wave X2 S4-1)
            Assert.Less(Beat05GateTower.StainHalfS, 0.5 * BulwarkConst.StairLanding, "stain quad on the flat landing");
            Assert.Less(Beat05GateTower.DripLateral + patchR, hi + Beat05Layout.WallOffset, "patch inside the east wall");
            int n = Beat05Layout.Chevrons(k);
            double pitch = 2.6 / n, w = Math.Min(0.24, 0.8 * pitch), lastEdge = 1.3 - 0.5 * pitch + 0.5 * w;
            Assert.Greater(Beat05GateTower.DripLateral - patchR, lastEdge, "patch clear of the chevron paint (no co-planar overlap)");
            Assert.GreaterOrEqual(Beat05GateTower.DripPeriod, 1.0, "drip at most 1 Hz");
        }

        [Test]
        public void Atrium_FitsInsideTheTowerAndKeepsTheKs13SightLine()
        {
            Assert.AreEqual(BulwarkConst.AtriumWidth - 1.0, Beat05Layout.TowerInX1 - Beat05Layout.TowerInX0, 1e-9, "12 m tower, 0.5 m walls");
            Assert.AreEqual(BulwarkConst.AtriumHeight, Beat05Layout.CeilingY0, 1e-9, "9 m high");
            foreach (double bz in Beat05Layout.BeamZ)
            {
                Assert.Less(bz + Beat05Layout.BeamHalf, BulwarkConst.StairMouthZ, "beams over the atrium only");
                foreach (double pz in BulwarkConst.PendantZ)
                    Assert.Greater(Math.Abs(bz - pz), Beat05Layout.BeamHalf + 0.25, "pendants hang between the beams");
                // KS13 (-1.5, 1.7, 926) pitch +14: the beams (bottom 8.2) do not hide the north arch top (y 5.5 at z 955.5)
                double yAtBeam = 1.7 + (bz - 926.0) * (5.5 - 1.7) / (955.5 - 926.0);
                if (bz > 926.0) Assert.Less(yAtBeam, Beat05Layout.CeilingY0 - Beat05Layout.BeamDepth, "arch sight line below beam " + bz);
            }
            // guard rail outside the capsule reach of the atrium lane (x -2.95 .. -0.05 + 0.35)
            Assert.Greater(Beat05Layout.GuardX - 0.05, -0.05 + BulwarkConst.PlayerR);
            // portal lamp boost (14.2.5): x2.46 at the plane, 1 from s 960
            Assert.AreEqual(Math.Pow(2.0, 1.3), BulwarkSchedule.LampBoost(920.0), 1e-9);
            Assert.AreEqual(1.0, BulwarkSchedule.LampBoost(960.0), 1e-9);
        }

        // ============================================================================ Wave X resume: landing lamp spots
        [Test]
        public void LandingSpots_KeepTheSoffitOverTheNicheOutOfTheCone()
        {
            double half = 0.5 * Beat05Layout.LandingSpotAngle;   // 75 deg
            const double aimDown = 45.0;                          // aimed 45 deg down into the well
            Assert.Less(half, 90.0 + aimDown, "straight up (135 deg off the axis) is outside the cone");
            Assert.LessOrEqual(half - aimDown, 30.0 + 1e-9, "nothing above 30 deg over the horizontal on the well side");
            for (int k = 1; k <= Beat05Layout.Landings; k++)
            {
                double sc = Beat05Layout.LandingCentre(k), y = BwPath.FloorY(sc);
                double lampY = y + 0.5 * (Beat05Layout.LampNicheY0 + Beat05Layout.LampNicheY1);
                double soffit = Beat05Layout.CeilingY(sc) - lampY;
                Assert.Greater(soffit, 0.3, "soffit above the niche centre at landing " + k);
                // the first soffit point inside the cone lies soffit / tan 30 across the well (0.78 m at 0.45 m)
                double firstLit = soffit / Math.Tan((half - aimDown) * Math.PI / 180.0);
                Assert.Greater(firstLit, 0.5, "no lamp hot spot on the soffit within 0.5 m of the wall at landing " + k);
            }
        }
    }
}
