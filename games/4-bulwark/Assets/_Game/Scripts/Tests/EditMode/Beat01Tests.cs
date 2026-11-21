// Beat01Tests: EditMode tests of beat 1, the wave gallery (MASTER_PROMPT 14.5.1 acceptance (1), (4), (5), 14.10.4 T2,
// Wave A ruling "ribs and props vs the corridor lane", Addendum A1 K0): the pure layout (Beat01Layout) against the spec.
using System;
using Megastructure.Beats;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class Beat01Tests
    {
        const double Tol = 0.02;

        // ============================================================================ acceptance (1): dimensions
        [Test]
        public void Corridor_Is4p0By3p6By120AndTheDoor4p0By3p4()
        {
            Beat01Layout.ClearSize(out double w, out double h);
            Assert.AreEqual(BulwarkConst.GalleryClearWidth, w, Tol, "clear width 4.0 (rib profile)");
            Assert.AreEqual(BulwarkConst.GalleryClearHeight, h, Tol, "clear height 3.6");
            Assert.AreEqual(BulwarkConst.GalleryLength, Beat01Layout.DoorZ, Tol, "gallery 120 m to the door plane");
            Assert.AreEqual(BulwarkConst.DoorWidth, Beat01Layout.DoorX1 - Beat01Layout.DoorX0, Tol, "door opening 4.0");
            Assert.AreEqual(BulwarkConst.DoorOpeningHeight, Beat01Layout.OpenH, Tol, "door opening 3.4");
            Assert.AreEqual(2.0 * Beat01Layout.LeafW, BulwarkConst.DoorWidth, Tol, "two 2.0 m leaves close the opening");
        }

        [Test]
        public void Ribs_SlitsLampsNiches_FollowTheSpecPitches()
        {
            Assert.AreEqual(20, Beat01Layout.RibCount, "ribs at z 3 + 6 k inside z 0..120");
            for (int k = 0; k < Beat01Layout.RibCount; k++)
            {
                Assert.AreEqual(3.0 + 6.0 * k, Beat01Layout.RibZ(k), 1e-9);
                Assert.Less(Beat01Layout.RibZ(k) + Beat01Layout.RibHalf, Beat01Layout.HousingZ0, "rib before the door housing");
            }
            for (int k = 0; k < BulwarkConst.SlitCount; k++)
            {
                double z = Beat01Layout.SlitZ(k);
                Assert.AreEqual(6.0 + 12.0 * k, z, 1e-9);
                // the slit pilaster lies between two ribs (no rib through a slit)
                double prev = Math.Floor((z - 3.0) / 6.0) * 6.0 + 3.0;
                Assert.Greater(z - Beat01Layout.PilasterHalf, prev + Beat01Layout.RibHalf - 1e-9, "pilaster clear of the rib before");
                Assert.Less(z + Beat01Layout.PilasterHalf, prev + 6.0 - Beat01Layout.RibHalf + 1e-9, "pilaster clear of the rib after");
            }
            for (int k = 1; k <= BulwarkConst.GalleryLampCount; k++) Assert.AreEqual(12.0 * k, Beat01Layout.LampZ(k), 1e-9);
            Assert.AreEqual(5, Beat01Layout.NicheCount, "niches at z 6 + 24 k before the door");
            bool v1 = false, v2 = false;
            for (int n = 0; n < Beat01Layout.NicheCount; n++)
            {
                double z = Beat01Layout.NicheZ(n);
                v1 |= Math.Abs(z - BulwarkConst.ValveWheelZ1) < 1e-9;
                v2 |= Math.Abs(z - BulwarkConst.ValveWheelZ2) < 1e-9;
            }
            Assert.IsTrue(v1 && v2, "the valve wheels of z 30 and 78 sit in niches");
            Assert.AreEqual(BulwarkConst.SlitX + BulwarkConst.SlitDepth, Beat01Layout.SlitBack, 1e-9, "slit channel 1.2 m deep from x 0.5");
        }

        [Test]
        public void LampBox_MatchesTheKs01AuditSubject()
        {
            Assert.AreEqual(BulwarkConst.GalleryClearHeight - 0.25, Beat01Layout.LampY0, 1e-9);
            Assert.AreEqual(BulwarkConst.GalleryClearHeight - 0.05, Beat01Layout.LampY1, 1e-9);
            Assert.AreEqual(BulwarkConst.StartX, Beat01Layout.LampX, 1e-9, "on the centre line");
            Assert.Less(Beat01Layout.LampY1, Beat01Layout.LampPivotY, "hung below the ceiling panel");
        }

        [Test]
        public void DoorBayLamp_SitsBetweenTheLastRibAndTheHousingAndClearsTheDoorFromKs01()
        {
            Assert.AreEqual(BulwarkConst.GalleryLampCount + 1, Beat01Layout.LampTotal);
            Assert.AreEqual(12.0 * BulwarkConst.GalleryLampCount, Beat01Layout.LampZAt(BulwarkConst.GalleryLampCount - 1), 1e-9, "index 8 is the lamp of z 108");
            double z = Beat01Layout.LampZAt(BulwarkConst.GalleryLampCount);
            double lastRib = Beat01Layout.RibZ(Beat01Layout.RibCount - 1);
            Assert.Greater(z - 0.1, lastRib + Beat01Layout.RibHalf, "hood clear of the last rib");
            Assert.Less(z + 0.1, Beat01Layout.HousingZ0, "hood clear of the door housing");
            // KS01 eye (y 1.7, z 8): the glass bottom projected onto the door plane stays above the 3.4 m opening head
            double y0 = Beat01Layout.LampY0 + Beat01Layout.DoorLampRaise;
            double yAtDoor = 1.7 + (y0 - 1.7) * (Beat01Layout.DoorZ - 8.0) / (z - 0.1 - 8.0);
            Assert.Greater(yAtDoor, Beat01Layout.OpenH, "door-bay lamp does not occlude the KS01 door subject");
            Assert.Less(Beat01Layout.LampY1 + Beat01Layout.DoorLampRaise, Beat01Layout.PanelH, "glass under the ceiling panel");
        }

        // ============================================================================ Part 10 / Wave A: capsule reach
        [Test]
        public void RibsAndProps_StayOutsideTheCapsuleReachOfTheCorridorLane()
        {
            Beat01Layout.CapsuleReach(out double x0, out double x1);
            Assert.AreEqual(-3.5, x0, 1e-9); Assert.AreEqual(0.5, x1, 1e-9);
            Assert.LessOrEqual(Beat01Layout.X0, x0 + 1e-9, "west rib face");
            Assert.GreaterOrEqual(Beat01Layout.X1, x1 - 1e-9, "east rib face");
            Assert.LessOrEqual(Beat01Layout.WestFittingsMaxX, x0 + 1e-9, "west props (pipes, tray, band, valve wheels) behind the rib plane");
            Assert.GreaterOrEqual(Beat01Layout.EastFittingsMinX, x1 - 1e-9, "east pilasters and the hand wheel behind the rib plane");
            Assert.AreEqual(BulwarkConst.RibProud, Beat01Layout.X0 - Beat01Layout.PanelX0, 1e-9, "ribs 0.15 proud of the panels");
            Assert.LessOrEqual(Beat01Layout.WestFittingsMaxX - Beat01Layout.PanelX0, 0.25 + 1e-9, "props protrude at most 0.25 m");
            // niche back inside the shell and inside the interior ambient box's full-weight zone for the cheeks (header arithmetic)
            Assert.Greater(Beat01Layout.NicheBackX, BulwarkConst.ShellXMin);
            Assert.GreaterOrEqual(Beat01Layout.NicheBackX, -3.85 - 1e-9);
        }

        // ============================================================================ acceptance (5) / T2: the door
        [Test]
        public void Door_GapIsAtLeast1p3m4sAfterTheTriggerAndOpenBeforeTheWalkerAtBothSpeeds()
        {
            foreach (double v in new[] { BulwarkConst.Cine, BulwarkConst.Walk })
            {
                double t = BulwarkConst.DoorGapCheckTime, s = BulwarkConst.DoorTriggerS + v * t;
                double g = Beat01Layout.DoorGap(s, t);
                Assert.GreaterOrEqual(g, BulwarkConst.DoorGapCheckMin, "gap 4.0 s after the trigger at " + v + " m/s");
                double tReach = (119.65 - BulwarkConst.DoorTriggerS) / v;
                Assert.AreEqual(BulwarkConst.DoorWidth, Beat01Layout.DoorGap(119.65, tReach), 1e-6, "fully open when the walker meets the plane at " + v);
                // the progress fallback alone (frozen clock) opens it too
                Assert.AreEqual(BulwarkConst.DoorWidth, Beat01Layout.DoorGap(119.65, double.NaN), 1e-6, "progress fallback");
            }
            Assert.AreEqual(0.0, Beat01Layout.DoorGap(103.9, 0.0), 1e-12, "closed before the trigger");
            // the timed rule is the 14.5.1 law: 4.0 smootherstep(t / 3)
            Assert.AreEqual(2.0, Beat01Layout.DoorGapTimed(1.5), 1e-9);
            // monotone in t and s (no door flutter)
            double prev = 0.0;
            for (int i = 0; i <= 200; i++)
            {
                double t = i * 0.03, g = Beat01Layout.DoorGap(BulwarkConst.DoorTriggerS + 1.5 * t, t);
                Assert.GreaterOrEqual(g, prev - 1e-12);
                prev = g;
            }
        }

        // ============================================================================ Wave X2 (sea S3-3): the slit burst
        [Test]
        public void SlitBurst_SpillFlareFollowsTheBreathEnvelopeWithoutFlashing()
        {
            Assert.AreEqual(1.0, Beat01WaveGallery.SpillGain(0f), 1e-6, "rest");
            Assert.AreEqual(1.0 + Beat01WaveGallery.SlitBurstSpill, Beat01WaveGallery.SpillGain(1f), 1e-6, "full burst");
            Assert.AreEqual(1.0 + Beat01WaveGallery.SlitBurstSpill, Beat01WaveGallery.SpillGain(0.5f), 1e-6, "a 0.45-strength impact flares fully");
            // the burst lives inside the impact's 3 s (0.6 s in, 2.4 s out): one rise and one fall per impact, so the flare
            // cannot flash faster than the impacts (WaveImpacts.Lifetime 3 s; <= 1 Hz needs a rise of >= 0.5 s)
            Assert.AreEqual(3.0, WaveImpacts.Lifetime, 1e-6);
            Assert.GreaterOrEqual(Beat01WaveGallery.SprayPerSlit * BulwarkConst.SlitCount, 100, "spray clumps");
        }

        // ============================================================================ acceptance (4): lamp sway, no roll
        [Test]
        public void LampSway_Reaches1p2Within0p1sAndDecays()
        {
            double a = Beat01Layout.ImpactKickDeg(0.75), peak = 0.0;
            Assert.AreEqual(BulwarkConst.LampSwayDeg, a, 1e-9);
            for (int i = 0; i <= 100; i++) peak = Math.Max(peak, Math.Abs(Beat01Layout.SwayDeg(i * 0.001, a)));
            Assert.AreEqual(1.2, peak, 0.3, "sway within 0.1 s");
            Assert.Less(Math.Abs(Beat01Layout.SwayDeg(9.0, a)), 0.06, "decay 3 s");
            Assert.AreEqual(0.0, Beat01Layout.SwayDeg(-1.0, a), 1e-12);
            // K0 (A1 1.6): +-0.6 deg at s 66
            double k0 = 0.0;
            for (int i = 0; i <= 2000; i++) k0 = Math.Max(k0, Math.Abs(Beat01Layout.SwayDeg(i * 0.002, Beat01Layout.K0KickDeg)));
            Assert.AreEqual(0.6, k0, 0.05);
            // the slit storm light stays within +-25 %
            Assert.LessOrEqual(Beat01Layout.SlitFlare(0.6, 1.0, 0.0), 0.25 + 1e-9);
            Assert.AreEqual(0.0, Beat01Layout.SlitFlare(3.5, 1.0, 0.0), 1e-12);
        }

        [Test]
        public void Profile_IsContinuousAndInsideTheShell()
        {
            for (int i = 0; i < Beat01Layout.ProfileCount; i++)
            {
                Beat01Layout.Profile(i, Beat01Layout.Recess, out double x, out double y, out double nx, out double ny);
                Assert.GreaterOrEqual(x, BulwarkConst.ShellXMin + 0.3, "panel inside the shell");
                Assert.LessOrEqual(x, BulwarkConst.ShellXMax - 0.3);
                Assert.LessOrEqual(y, BulwarkConst.ShellYMax - 0.3, "ceiling slab at least 0.3 m");
                Assert.AreEqual(1.0, Math.Sqrt(nx * nx + ny * ny), 1e-9);
                if (i > 0)
                {
                    Beat01Layout.Profile(i - 1, Beat01Layout.Recess, out double px, out double py, out _, out _);
                    Assert.Less(Math.Sqrt((x - px) * (x - px) + (y - py) * (y - py)), 4.5, "segment");
                }
            }
        }
    }
}
