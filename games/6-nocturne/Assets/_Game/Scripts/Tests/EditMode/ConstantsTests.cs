using System;
using NUnit.Framework;

namespace Megastructure.Tests
{
    /// <summary>
    /// The 14.12 / 14.2 contract numbers (IMPORT_PLAN section 8 "constants, palette"): beat table and runtime closure, the
    /// 14.2.4 schedules, T-FOG, the 14.2.1 palette (hex -> linear), the 14.11 UI timeline and the -progress / -dither flags.
    /// </summary>
    public sealed class ConstantsTests
    {
        [Test]
        public void BeatTableClosesTo1500mAnd600s()
        {
            var b = NocturneConstants.BeatBoundaries;
            Assert.AreEqual(NocturneConstants.BeatCount + 1, b.Length);
            CollectionAssert.AreEqual(new double[] { 0, 120, 400, 560, 880, 1080, 1260, 1500 }, b);
            double brisk = 0, walk = 0;
            for (int i = 0; i < NocturneConstants.BeatCount; i++)
            {
                double len = b[i + 1] - b[i];
                Assert.AreEqual(len / 2.5, NocturneConstants.BeatSecondsBrisk[i], 0.01, "beat " + (i + 1) + " at 2.5 m/s");
                Assert.AreEqual(len / 1.5, NocturneConstants.BeatSecondsWalk[i], 0.01, "beat " + (i + 1) + " at 1.5 m/s");
                brisk += NocturneConstants.BeatSecondsBrisk[i]; walk += NocturneConstants.BeatSecondsWalk[i];
            }
            Assert.AreEqual(600.0, brisk, 0.01); Assert.AreEqual(1000.0, walk, 0.02);
            Assert.AreEqual(NocturneConstants.PathLength, b[b.Length - 1]);
            Assert.AreEqual(3.0, NocturneConstants.GoalS - NocturneConstants.GoalTriggerS, 1e-12);
            Assert.AreEqual(1497.0 / 2.5 + 0.25, NocturneConstants.CineTriggerTime, 1e-9, "trigger = 598.8 + T/2 of the 0.5 s ramp");
            Assert.AreEqual(1497.0 / 1.5 + 0.1, NocturneConstants.WalkTriggerTime, 1e-9, "trigger = 998.0 + T/2 of the 0.2 s ramp");
        }

        [Test]
        public void BeatIndexAtBoundaries()
        {
            Assert.AreEqual(0, NocMath.BeatIndex(0)); Assert.AreEqual(0, NocMath.BeatIndex(119.999));
            Assert.AreEqual(1, NocMath.BeatIndex(120)); Assert.AreEqual(2, NocMath.BeatIndex(400)); Assert.AreEqual(3, NocMath.BeatIndex(560));
            Assert.AreEqual(4, NocMath.BeatIndex(880)); Assert.AreEqual(5, NocMath.BeatIndex(1080)); Assert.AreEqual(6, NocMath.BeatIndex(1260));
            Assert.AreEqual(6, NocMath.BeatIndex(1500));
        }

        [Test]
        public void FovSchedule14_2_4()
        {
            Assert.AreEqual(62, NocSchedule.Fov(40), 1e-9); Assert.AreEqual(64, NocSchedule.Fov(160), 1e-9);
            Assert.AreEqual(66, NocSchedule.Fov(300), 1e-9); Assert.AreEqual(68, NocSchedule.Fov(500), 1e-9);
            Assert.AreEqual(66, NocSchedule.Fov(700), 1e-9); Assert.AreEqual(62, NocSchedule.Fov(1000), 1e-9);
            Assert.AreEqual(66, NocSchedule.Fov(1200), 1e-9); Assert.AreEqual(62, NocSchedule.Fov(1480), 1e-9);
            Assert.AreEqual(63, NocSchedule.Fov(105), 1e-9, "midpoint of 80-130");
            Assert.AreEqual(62.1, NocSchedule.Fov(88), 0.06, "KS02 vFOV 62.1");
            Assert.AreEqual(63.8, NocSchedule.Fov(117), 0.06, "KS03 vFOV 63.8 (D-029: KS03 at s 117)");
            Assert.AreEqual(66.9, NocSchedule.Fov(428), 0.06, "KS08 vFOV 66.9");
            // peak slope at 2.5 m/s: 14.2.4 quotes 0.3 deg/s; the smootherstep peak of 66 -> 62 / 62 -> 66 over 60 m is
            // 1.875 x 4 / 60 x 2.5 = 0.3125 deg/s (the spec rounded); Part 9's limit is 1 deg/s
            double slope = NocMath.KnotsMaxSlope(NocturneConstants.FovKnotS, NocturneConstants.FovKnotV) * 2.5;
            Assert.AreEqual(0.3125, slope, 1e-9, "vFOV peak slope at 2.5 m/s");
            Assert.Less(slope, 1.0, "Part 9: FOV changes slower than 1 deg/s");
        }

        [Test]
        public void EvScheduleAndSlew()
        {
            // LOOK_V2 7: -0.1 / +0.1 / 0.0 / 0.0 / -0.2 / -0.1 / 0.0 per beat (exposure relative to the Floor)
            Assert.AreEqual(-0.1, NocSchedule.ExposureEV(50), 1e-9); Assert.AreEqual(0.1, NocSchedule.ExposureEV(200), 1e-9);
            Assert.AreEqual(0.0, NocSchedule.ExposureEV(500), 1e-9); Assert.AreEqual(0.0, NocSchedule.ExposureEV(700), 1e-9);
            Assert.AreEqual(-0.2, NocSchedule.ExposureEV(1000), 1e-9); Assert.AreEqual(-0.1, NocSchedule.ExposureEV(1200), 1e-9);
            Assert.AreEqual(0.0, NocSchedule.ExposureEV(1400), 1e-9);
            double slope = NocMath.KnotsMaxSlope(NocturneConstants.EvKnotS, NocturneConstants.EvKnotV) * 2.5;
            Assert.AreEqual(0.03125, slope, 0.001, "peak EV slope 0.031 EV/s at 2.5 m/s (LOOK_V2 7)");
            Assert.Less(slope, NocturneConstants.EvSlew, "the 0.30 EV/s slew never binds");
        }

        [Test]
        public void FogTransmittanceT_FOG()
        {
            // T-FOG at the Look v2 sigma of beats 2-4 (LOOK_V2 3.1: 3.6e-4 -> 4.4e-4; d50 1 892 m, T 0.90 at 750 m, 0.82 at 1 km,
            // 0.46 at 2 km), the 14.2.2 form exp(-(sigma_eff d)^2) unchanged
            const double s = 4.4e-4;
            Assert.AreEqual(0.992, NocFogMath.Transmittance(s, 200, 0), 0.002);
            Assert.AreEqual(0.897, NocFogMath.Transmittance(s, 750, 0), 0.002);
            Assert.AreEqual(0.824, NocFogMath.Transmittance(s, 1000, 0), 0.002);
            Assert.AreEqual(0.461, NocFogMath.Transmittance(s, 2000, 0), 0.002);
            Assert.AreEqual(0.175, NocFogMath.Transmittance(s, 3000, 0), 0.002);
            Assert.AreEqual(0.008, NocFogMath.Transmittance(s, 5000, 0), 0.002);
            Assert.AreEqual(0.0003, NocFogMath.Transmittance(s, 6500, 0), 0.002);
            // straight down from the eye (y 1.7): d = 1.7 - y
            Assert.AreEqual(0.314, NocFogMath.Transmittance(s, 1001.7, -1000), 0.002);
            Assert.AreEqual(0.033, NocFogMath.Transmittance(s, 1501.7, -1500), 0.002);
            Assert.AreEqual(0.002, NocFogMath.Transmittance(s, 2001.7, -2000), 0.002);
            Assert.AreEqual(0.0001, NocFogMath.Transmittance(s, 2501.7, -2500), 0.002);
            Assert.AreEqual(0.0, NocFogMath.Transmittance(s, 3301.7, -3300), 0.002);
            Assert.AreEqual(1892, NocFogMath.D50(4.4e-4), 1.0, "d50 beats 2-4");
            Assert.AreEqual(167, NocFogMath.D50(5.0e-3), 1.0, "d50 beat 1");
            Assert.AreEqual(1388, NocFogMath.D50(6.0e-4), 1.0, "d50 beat 5");
            Assert.AreEqual(5.0e-3, NocSchedule.FogSigma(50), 1e-12); Assert.AreEqual(4.4e-4, NocSchedule.FogSigma(500), 1e-12);
            Assert.AreEqual(4.4e-4, NocSchedule.FogSigmaRegional(50), 1e-12, "the alley total stays 5.0e-3 over the regional 4.4e-4");
            Assert.AreEqual(4.56e-3, NocSchedule.AlleyHaze(50), 1e-12);
            Assert.AreEqual(6.0e-4, NocSchedule.FogSigma(950), 1e-12); Assert.AreEqual(4.5e-4, NocSchedule.FogSigma(1200), 1e-12);
            Assert.AreEqual(3.4e-4, NocSchedule.FogSigma(1400), 1e-12);
        }

        [Test]
        public void SkyGradientIsMonotoneAndMatchesStops()
        {
            NocFogMath.SkyGradient(0, out double r, out double g, out double b);
            var hz = NocturnePalette.Linear(NocPal.HazeHorizon);
            Assert.AreEqual(hz.r, r, 1e-6); Assert.AreEqual(hz.b, b, 1e-6);
            NocFogMath.SkyGradient(90, out r, out g, out b);
            var zn = NocturnePalette.Linear(NocPal.NightZenith);
            Assert.AreEqual(zn.b, b, 1e-6);
            NocFogMath.SkyGradient(-90, out r, out g, out b);
            var ag = NocturnePalette.Linear(NocPal.AbyssGlow);
            Assert.AreEqual(ag.r, r, 1e-6);
            // continuity: no step at the horizon (D-006)
            NocFogMath.SkyGradient(0.01, out double r1, out _, out _);
            NocFogMath.SkyGradient(-0.01, out double r2, out _, out _);
            Assert.AreEqual(r1, r2, 1e-4);
        }

        [Test]
        public void PaletteMatches14_2_1()
        {
            var t = NocturnePalette.Table;
            Assert.AreEqual(Enum.GetValues(typeof(NocPal)).Length, t.Length, "every token in the table");
            Assert.AreEqual(43, t.Length, "28 tokens of 14.2.1 + 15 of LOOK_V2 2.2");
            for (int i = 0; i < t.Length; i++)
            {
                Assert.AreEqual(i, (int)t[i].id, "table order = enum order");
                var c = NocturnePalette.Linear(t[i].id);
                Assert.AreEqual(NocturnePalette.SpecLinear[i, 0], c.r, 6e-5, t[i].id + " r");
                Assert.AreEqual(NocturnePalette.SpecLinear[i, 1], c.g, 6e-5, t[i].id + " g");
                Assert.AreEqual(NocturnePalette.SpecLinear[i, 2], c.b, 6e-5, t[i].id + " b");
            }
            Assert.AreEqual(NocturneConstants.FloorLinR, NocturnePalette.Linear(NocPal.Floor).r, 6e-5, "Floor token = FloorLin");
            Assert.AreEqual(NocturneConstants.FloorLinG, NocturnePalette.Linear(NocPal.Floor).g, 6e-5);
            Assert.AreEqual(NocturneConstants.FloorLinB, NocturnePalette.Linear(NocPal.Floor).b, 6e-5);
            // LOOK_V2 2.1: Floor #0A0C0F, codes (10, 12, 15), blue-black (B - R +5 codes), luma 11.8; crush threshold Floor + 3
            Assert.AreEqual("#0A0C0F", Array.Find(t, e => e.id == NocPal.Floor).hex);
            double luma = 255.0 * NocMath.Luma709(10 / 255.0, 12 / 255.0, 15 / 255.0);
            Assert.AreEqual(NocturneConstants.FloorLuma, luma, 0.1, "Floor luma 11.8");
            // R7: NeonViolet stays in the enum (indices stable) but is retired; NeonMagenta sits outside the violet band (345 deg)
            Assert.AreEqual(11, (int)NocPal.NeonViolet);
            Assert.AreEqual(42, (int)NocPal.GlassDark);
        }

        [Test]
        public void WindowSharesAndAmberSchedule_LookV2_4_R6()
        {
            // w_f: 0.32 to s 400, 0.20 at 880, 0 from 1 250 (piecewise linear); n_f 0.30 to s 880, 0.15 from 1 250
            Assert.AreEqual(0.32, NocSchedule.WarmShare(0), 1e-12); Assert.AreEqual(0.32, NocSchedule.WarmShare(400), 1e-12);
            Assert.AreEqual(0.26, NocSchedule.WarmShare(640), 1e-12, "linear between the knots");
            Assert.AreEqual(0.20, NocSchedule.WarmShare(880), 1e-12); Assert.AreEqual(0.10, NocSchedule.WarmShare(1065), 1e-12);
            Assert.AreEqual(0.0, NocSchedule.WarmShare(1250), 1e-12); Assert.AreEqual(0.0, NocSchedule.WarmShare(1500), 1e-12);
            Assert.AreEqual(0.30, NocSchedule.NeutralShare(880), 1e-12); Assert.AreEqual(0.15, NocSchedule.NeutralShare(1250), 1e-12);
            Assert.AreEqual(0.15, NocSchedule.NeutralShare(1500), 1e-12);
            // peak rate 0.054 per 100 m = 0.14 % of lit windows per second at 2.5 m/s (H2 allows 0.3 %/s)
            double peak = 0;
            for (double s = 0; s < 1500; s += 1) peak = Math.Max(peak, Math.Abs(NocSchedule.WarmShare(s + 1) - NocSchedule.WarmShare(s)));
            Assert.AreEqual(0.054, peak * 100, 0.001);
            Assert.Less(peak * 2.5 * 100, 0.3);
            // R6: k_a = w_f / 0.32 (1 to s 400, 0.625 at 880, 0 from 1 250); the screen wall's amber stop 1 -> 0 over s 1 230-1 290
            Assert.AreEqual(1.0, NocSchedule.AmberK(200), 1e-12); Assert.AreEqual(0.625, NocSchedule.AmberK(880), 1e-12);
            Assert.AreEqual(0.0, NocSchedule.AmberK(1260), 1e-12);
            Assert.AreEqual(1.0, NocSchedule.ScreenAmber(1230), 1e-12); Assert.AreEqual(0.0, NocSchedule.ScreenAmber(1290), 1e-12);
            Assert.AreEqual(0.5, NocSchedule.ScreenAmber(1260), 1e-12);
        }

        [Test]
        public void PostTables_LookV2_7()
        {
            CollectionAssert.AreEqual(new[] { 0.9, 0.9, 0.9, 0.9, 0.85, 0.9, 0.9 }, NocturneConstants.BloomThreshold);
            CollectionAssert.AreEqual(new[] { 0.40, 0.45, 0.40, 0.40, 0.35, 0.40, 0.45 }, NocturneConstants.BloomIntensity);
            CollectionAssert.AreEqual(new[] { 0.70, 0.75, 0.75, 0.72, 0.70, 0.75, 0.75 }, NocturneConstants.BloomScatter);
            CollectionAssert.AreEqual(new[] { 0.0, 4, 2, 0, 0, 4, 4 }, NocturneConstants.Saturation);
            CollectionAssert.AreEqual(new[] { 0.16, 0.14, 0.14, 0.15, 0.16, 0.14, 0.15 }, NocturneConstants.Vignette);
            Assert.AreEqual(0.0, NocturneConstants.Contrast);
            Assert.AreEqual(30.0, NocturneConstants.BloomClamp);
            Assert.AreEqual(1.6, NocturneConstants.NeonGain, 1e-12, "signs x1.6 (LOOK_V2 2.3)");
            Assert.AreEqual(1.6f, NocturnePalette.EmissionSign); Assert.AreEqual(2.0f, NocturnePalette.EmissionTube);
            Assert.AreEqual(3.5f, NocturnePalette.EmissionGoalLamp, "the only core above 3.0");
            Assert.AreEqual(0.55f, NocturnePalette.EmissionKoiBody); Assert.AreEqual(1.6f, NocturnePalette.EmissionKoiRim);
            Assert.AreEqual(0.07, NocturneConstants.KoiLightGain, 1e-12, "LOOK_V2 6's x 0.7 recalibrated for the v2 albedos (PA r01)");
            Assert.AreEqual(0.3, NocturneConstants.RainTint, 1e-12, "drops take the local hue at x0.3 (CRIT0 #28)");
            Assert.AreEqual(0.15, NocturneConstants.SheetSkyAmp, 1e-12);
            // neon weights sum to 1 with NeonViolet at 0 (R7)
            Assert.AreEqual(1.0, NocturneConstants.NeonWAmber + NocturneConstants.NeonWNeutral + NocturneConstants.NeonWCyan + NocturneConstants.NeonWRed
                                 + NocturneConstants.NeonWWhite + NocturneConstants.NeonWMagenta, 1e-12);
        }

        [Test]
        public void UiTimeline14_11()
        {
            Assert.AreEqual(0f, UiTimeline.Picture(0.5f)); Assert.AreEqual(1f, UiTimeline.Picture(3.1f), 1e-6f);
            Assert.AreEqual(0f, UiTimeline.Title(0.7f)); Assert.AreEqual(1f, UiTimeline.Title(3f), 1e-6f); Assert.AreEqual(0f, UiTimeline.Title(5.4f), 1e-6f);
            Assert.AreEqual(0f, UiTimeline.Hint(5.9f)); Assert.AreEqual(1f, UiTimeline.Hint(9f), 1e-6f); Assert.AreEqual(0f, UiTimeline.Hint(12f), 1e-6f);
            Assert.AreEqual(1f, UiTimeline.EndPicture(1.5f), 1e-6f, "1.5 s of picture");
            Assert.AreEqual(0f, UiTimeline.EndPicture(5.5f), 1e-6f, "4.0 s fade");
            Assert.AreEqual(0f, UiTimeline.EndAudio(7.5f), 1e-6f, "audio 6 s");
            Assert.AreEqual(0f, UiTimeline.EndLine(7.0f), 1e-6f); Assert.AreEqual(1f, UiTimeline.EndLine(9.0f), 1e-6f);
            Assert.AreEqual(1f, UiTimeline.EndLine(15.0f), 1e-6f); Assert.AreEqual(0f, UiTimeline.EndLine(17.0f), 1e-6f);
            Assert.AreEqual(17f, UiTimeline.EndTotal, 1e-6f);
            Assert.LessOrEqual(UiTimeline.HintKill, 0.4f);
        }

        [Test]
        public void ProgressIsAFractionAndDitherFlags()
        {
            try
            {
                CliArgs.Override(new[] { "x", "-progress", "0.5" });
                Assert.AreEqual(750.0, CliArgs.StartS, 1e-9, "-progress X is the fraction s/1500 (14.12)");
                CliArgs.Override(new[] { "x", "-beat", "3" });
                Assert.AreEqual(400.0, CliArgs.StartS, 1e-9);
                CliArgs.Override(new[] { "x" });
                Assert.AreEqual(DitherMode.Static, CliArgs.Dither, "static by default (14.7 H1)");
                CliArgs.Override(new[] { "x", "-dither", "temporal" });
                Assert.AreEqual(DitherMode.Temporal, CliArgs.Dither);
                CliArgs.Override(new[] { "x", "-dither", "off" });
                Assert.AreEqual(DitherMode.Off, CliArgs.Dither);
                CliArgs.Override(new[] { "x", "-autotest-until", "400" });
                Assert.IsTrue(CliArgs.AutoTest); Assert.AreEqual(400.0, CliArgs.AutoTestUntil, 1e-9);
            }
            finally { CliArgs.Override(null); }
        }

        [Test]
        public void DitherFrozenByDefault()
        {
            var saved = NocClock.Dither;
            try
            {
                NocClock.Reset(); NocClock.Dither = DitherMode.Static;
                for (int i = 0; i < 1000; i++) NocClock.Tick(i / 60.0);
                Assert.AreEqual(0, NocClock.Frame, "static: the R2 offset never moves");
                R2Sequence.TileOffset(R2Sequence.DitherFrame(NocClock.Frame, NocClock.FreezeDither), 64, out int ox, out int oy);
                Assert.AreEqual(32, ox); Assert.AreEqual(32, oy);
                NocClock.Reset(); NocClock.Dither = DitherMode.Temporal;
                for (int i = 0; i < 10; i++) NocClock.Tick(i / 60.0);
                Assert.AreEqual(10, NocClock.Frame, "temporal: advances per frame");
            }
            finally { NocClock.Dither = saved; NocClock.Reset(); }
        }
    }
}
