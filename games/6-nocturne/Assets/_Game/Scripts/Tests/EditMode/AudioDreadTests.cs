using System;
using NUnit.Framework;

namespace Megastructure.Tests
{
    /// <summary>
    /// The sound of the D-034 dread layer (AU, Phase 4 wave A fifth start; docs/DREAD.md section 1 and the cues of sections 2-6;
    /// docs/AUDIO.md section 11): the Gulf echo taps against the DREAD 1.1 table (0.93 / 2.57 s at the tongue tip, 1.87 / 1.63 s
    /// at mid-bridge, 3.5 s at the boulevard mouth) and the echo bus's impulse response, the sub-bass swell envelopes under KS04,
    /// KS09, KS10, KS13 and KS17 (rise 6-10 s, decay 8 s, 35-55 Hz with harmonics to about 160 Hz), the silence drops 2.0 s before
    /// KS09 and KS17 (18-24 dB over 0.6 s, back over 1.5 s; rain and the city bed only), the cue times (horns s 440 / 700, the
    /// panel's creak at s 470 after its flight time, strikes every 25-40 s in the Gulf, groans every 20-60 s from s 120, the tram
    /// chime before the launch, the rumbles under the lightning glows, the hull's course and Doppler), the unit loudness of every
    /// voice, the 14.3 / 14.10 gates on renders through the cues (true peak, sample step, 20 ms envelope step, finite output), and
    /// callback-size invariance with the one-shots and the echo bus running.
    /// Fix wave (2026-11-21, panel p4_01): C07 the drops take the pad (-14 dB), the passes, the koi and the wind (-4 dB) down too, so
    /// everything but E falls >= 10 dB; C08 no horn, strike or groan within 15 m of a train / tram meet, horn 2 at s 740; C09 the
    /// swells, the hull and the koi carry a small-speaker layer above 200 Hz; C53 the chime clears T and the X beds by 6 dB; C49 the
    /// city hum keeps under 15 % of its power below 30 Hz; the hull and the panel read DR's DreadCourse.
    /// </summary>
    public sealed class AudioDreadTests
    {
        const int Fs = 48000;
        const double V = DreadScore.RefSpeed;

        static double KeyS(string prefix)
        {
            foreach (var k in NocturneConstants.KeyShots) if (k.tag.StartsWith(prefix, StringComparison.Ordinal)) return k.s;
            throw new ArgumentException(prefix);
        }

        // ------------------------------------------------------------------ 1.1 echo
        [Test]
        public void EchoTaps_MatchTheDreadTable()
        {
            void Delay(double x, int k, double expected)
            {
                DreadScore.EchoTap(k, x, out double d, out double g, out _, out _);
                Assert.AreEqual(expected, d, 0.006, "tap " + k + " at x " + x);
                Assert.Greater(g, 0.0, "tap " + k + " audible at x " + x);
            }
            Delay(375.0, 0, 0.93); Delay(375.0, 1, 2.57);    // tongue tip
            Delay(535.0, 0, 1.87); Delay(535.0, 1, 1.63);    // bridge middle
            Delay(215.3, 1, 3.50);                           // boulevard mouth: one long echo off face B
            DreadScore.EchoTap(0, 215.3, out double d0, out double g0, out _, out _);
            Assert.Less(d0, 0.005, "face A delay ~0 at the mouth"); Assert.AreEqual(0.0, g0, 1e-12, "a tap under 30 ms merges with the room");
            // the face-to-face path 600 m: AB2 at 1 200 m = 3.50 s; at mid-Gulf the arrivals come every ~1.75 s (600 m / 343)
            DreadScore.EchoTap(2, 515.3, out double d2, out _, out _, out _);
            Assert.AreEqual(1200.0 / 343.0, d2, 1e-9);
            DreadScore.EchoTap(0, 515.3, out double da, out _, out _, out _);
            Assert.AreEqual(600.0 / 343.0, da, 1e-9); Assert.AreEqual(600.0 / 343.0, d2 - da, 1e-9);
            // attenuation 1 / (1 + d / 150) and a low-pass that falls with the path
            double lastLp = double.MaxValue;
            for (double x = 260.0; x <= 770.0; x += 10.0)   // every tap longer than 0.15 s (the merge ramp is 0.03-0.15 s)
            {
                DreadScore.FaceDistances(x, out double dA, out double dB);
                DreadScore.EchoTap(0, x, out _, out double ga, out double lpa, out double ia);
                DreadScore.EchoTap(1, x, out _, out double gb, out double lpb, out double ib);
                Assert.AreEqual(1.0 / (1.0 + dA / 150.0), ga, 1e-9, "face A gain at x " + x);
                Assert.AreEqual(1.0 / (1.0 + dB / 150.0), gb, 1e-9, "face B gain at x " + x);
                Assert.AreEqual(DreadScore.FaceAX, ia); Assert.AreEqual(DreadScore.FaceBX, ib);
                Assert.AreEqual(dA < dB, lpa > lpb, "the nearer face is brighter at x " + x);
                Assert.Less(lpa, lastLp + 1e-9); lastLp = lpa;
            }
            DreadScore.EchoTap(1, 805.0, out double dm, out double gm, out _, out _);
            Assert.AreEqual(1.0 / (1.0 + 10.3 / 150.0) * NocMath.Smootherstep(DreadScore.EchoMergeSeconds, DreadScore.EchoFullSeconds, dm), gm, 1e-9, "a short tap merges smoothly");
            // heard in the Gulf only
            Assert.AreEqual(0.0, DreadScore.EchoWeight(150.0)); Assert.AreEqual(1.0, DreadScore.EchoWeight(400.0)); Assert.AreEqual(0.0, DreadScore.EchoWeight(900.0));
            Assert.LessOrEqual(DreadScore.MaxEchoSeconds, 3.6);
        }

        [Test]
        public void EchoBus_ImpulseArrivesAtTheTapDelays()
        {
            foreach (double x in new[] { 375.0, 535.0, 700.0 })
            {
                var ir = DreadProbe.EchoImpulse(x, Fs, 3.8);
                double first = double.MaxValue;
                for (int k = 0; k < DreadScore.EchoTapCount; k++)
                {
                    DreadScore.EchoTap(k, x, out double d, out double g, out _, out _);
                    DreadProbe.Arrival(ir, Fs, d, out double t, out double e);
                    Assert.AreEqual(d, t, 0.001, "tap " + k + " arrival at x " + x);
                    Assert.Greater(e, 1e-6 * g * g, "tap " + k + " carries energy");
                    first = Math.Min(first, d);
                }
                double pre = 0.0;
                for (int i = 1; i < (int)((first - 0.07) * Fs); i++) pre += ir[i];
                Assert.Less(pre, 1e-12, "nothing before the first echo at x " + x);
            }
        }

        [Test]
        public void EchoBus_GlidesWithTheWalkerWithoutClicks()
        {
            // a 220 Hz tone through the bus while the listener walks the bridge at 2.5 m/s, then teleports 200 m
            var bus = new EchoBus(Fs, Fs / NocMixer.Block);
            var f = new ScoreFrame { Fs = Fs, InvFs = 1.0 / Fs, Block = NocMixer.Block, X = 450.0, Z = 195.0, Yaw = 90.0 };
            int n = 12 * Fs;
            double pl = 0, pl2 = 0, maxD2 = 0, maxD2Jump = 0;
            for (int i = 0; i < n; i++)
            {
                if (i % NocMixer.Block == 0) { f.N = i; f.X = 450.0 + 2.5 * i / (double)Fs + (i >= 9 * Fs ? 200.0 : 0.0); bus.Control(in f); }
                double u = (i % NocMixer.Block) / (double)NocMixer.Block;
                double x = 0.1 * Math.Sin(2.0 * Math.PI * 220.0 * i / Fs);
                bus.Tick(u, x, out double l, out _);
                if (i > 4 * Fs)   // every tap (<= 3.5 s) carries the tone by now: its own onset is not part of the check
                {
                    double d2 = Math.Abs(l - 2.0 * pl + pl2);
                    if (i < 9 * Fs) maxD2 = Math.Max(maxD2, d2); else maxD2Jump = Math.Max(maxD2Jump, d2);
                }
                pl2 = pl; pl = l;
            }
            // the second difference of a 220 Hz sine of amplitude a is a (2 pi 220 / fs)^2 = 8.3e-4 a; three taps of gain <= 1 stay
            // under 3 x that unless something kinks (harness echojump: walking 6.3e-5, the jump 7.9e-5, limit 2.5e-4)
            double sine = 0.1 * Math.Pow(2.0 * Math.PI * 220.0 / Fs, 2);
            Assert.Less(maxD2, 3.0 * sine, "gliding taps do not click");
            Assert.Less(maxD2Jump, 3.0 * sine, "a 200 m jump fades the taps instead of snapping");
        }

        // ------------------------------------------------------------------ 1.2 swells
        [Test]
        public void Swells_RiseAndDecayUnderTheKeyShots()
        {
            CollectionAssert.AreEqual(new[] { "KS04", "KS09", "KS10", "KS13", "KS17" }, DreadScore.SwellNames);
            for (int k = 0; k < DreadScore.SwellS.Length; k++)
            {
                double s0 = DreadScore.SwellS[k];
                Assert.AreEqual(KeyS(DreadScore.SwellNames[k]), s0, 1e-9, DreadScore.SwellNames[k] + " s");
                double rise = DreadScore.M(DreadScore.SwellRiseSeconds), hold = DreadScore.M(DreadScore.SwellHoldSeconds), dec = DreadScore.M(DreadScore.SwellDecaySeconds);
                Assert.That(rise / V, Is.InRange(6.0, 10.0), "rise 6-10 s at the video speed");
                Assert.AreEqual(8.0, dec / V, 1e-9, "decay 8 s");
                Assert.AreEqual(0.0, DreadScore.SwellEnvelope(k, s0 - rise - 0.01), 1e-12);
                Assert.AreEqual(1.0, DreadScore.SwellEnvelope(k, s0), 1e-9, "peak at the reveal");
                Assert.AreEqual(1.0, DreadScore.SwellEnvelope(k, s0 + hold), 1e-9);
                Assert.AreEqual(0.0, DreadScore.SwellEnvelope(k, s0 + hold + dec + 0.01), 1e-12);
                double last = 0.0;
                for (double s = s0 - rise; s <= s0; s += 0.1) { double e = DreadScore.SwellEnvelope(k, s); Assert.GreaterOrEqual(e, last - 1e-12); last = e; }
                last = 1.0;
                for (double s = s0 + hold; s <= s0 + hold + dec; s += 0.1) { double e = DreadScore.SwellEnvelope(k, s); Assert.LessOrEqual(e, last + 1e-12); last = e; }
                // 35-55 Hz with harmonics to about 160 Hz: at least one partial in 100-170 Hz (laptops, phones, the encoder)
                Assert.That(DreadScore.SwellHz[k], Is.InRange(35.0, 55.0));
                bool laptop = false;
                for (int h = 1; h <= DreadScore.SwellPartials; h++)
                {
                    double fh = h * DreadScore.SwellHz[k];
                    if (DreadScore.SwellPartialGain(k, h) > 0.0) { Assert.LessOrEqual(fh, DreadScore.SwellTopHz + 1e-9); if (fh >= 100.0) laptop = true; }
                }
                Assert.IsTrue(laptop, DreadScore.SwellNames[k] + " has a partial a laptop plays");
                Assert.LessOrEqual(DreadScore.SwellPeakDb[k], -35.0, "under the 14.3 rows (beat medians -29..-31 dBFS)");
            }
        }

        [Test]
        public void Swell_RenderedLevelFollowsItsEnvelope()
        {
            // KS09 at 2.5 m/s, the dread stem with the swells only: quiet 9 s before, at its peak level at the reveal
            var bins = RenderStem(DreadScore.SwellS[1] - 30.0, 16.0, 2.5, 2, 0.5, out _);
            double before = Max(bins, 0, 4), peak = Max(bins, 22, 26);   // s 490-495 and s 517.5-522.5
            Assert.AreEqual(DreadScore.SwellPeakDb[1], peak, 1.5, "KS09 swell peak (dBFS)");
            Assert.Less(before, -70.0, "no swell 10 s before");
            for (int i = 1; i < 24; i++) Assert.GreaterOrEqual(bins[i], bins[i - 1] - 1.0, "rising into the reveal at bin " + i);
        }

        // ------------------------------------------------------------------ 1.4 silence drops
        [Test]
        public void SilenceDrops_TwoSecondsBeforeKS09AndKS17()
        {
            Assert.AreEqual(KeyS("KS09"), DreadScore.DropS[0], 1e-9); Assert.AreEqual(KeyS("KS17"), DreadScore.DropS[1], 1e-9);
            Assert.That(-DreadScore.DropDepthDb, Is.InRange(18.0, 24.0));
            foreach (double k in DreadScore.DropS)
            {
                double a = k - 2.0 * V;
                Assert.AreEqual(0.0, DreadScore.DropDb(a - 0.01), 1e-12, "starts 2.0 s before the reveal");
                Assert.AreEqual(DreadScore.DropDepthDb, DreadScore.DropDb(a + 0.6 * V), 1e-9, "full depth after 0.6 s");
                for (double s = a + 0.6 * V; s <= k; s += 0.05) Assert.That(-DreadScore.DropDb(s), Is.InRange(18.0, 24.0), "held to the reveal");
                Assert.AreEqual(0.0, DreadScore.DropDb(k + 1.5 * V + 0.01), 1e-12, "back over 1.5 s");
                double last = 0.0;
                for (double s = a; s <= a + 0.6 * V; s += 0.01) { double d = DreadScore.DropDb(s); Assert.LessOrEqual(d, last + 1e-12); last = d; }
                for (double s = k; s <= k + 1.5 * V; s += 0.01) { double d = DreadScore.DropDb(s); Assert.GreaterOrEqual(d, last - 1e-12); last = d; }
            }
            Assert.AreEqual(1.0, DreadScore.DropGain(300.0), 1e-12);
        }

        [Test]
        public void SilenceDrop_DucksRainAndTheCityBedOnlyWithoutClicks()
        {
            double k = DreadScore.DropS[0];
            var st = new OfflineRenderSettings { S0 = k - 15.0, Speed = 2.5, Seconds = 8.0, Seed = 1, Footsteps = false };
            var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
            mixer.MeterStems = true;
            int bin = Fs / 10, nb = 80;
            var buf = new float[bin * 2];
            var stem = new double[nb, NocturneScore.StemCount];
            double env = -1, maxEnvStep = 0, maxStep = 0, prev = 0;
            var tp = new TruePeakMeter();
            for (int b = 0; b < nb; b++)
            {
                Array.Clear(mixer.StemSumSq, 0, mixer.StemSumSq.Length);
                mixer.Process(buf, 2);
                for (int q = 0; q < NocturneScore.StemCount; q++) stem[b, q] = 10.0 * Math.Log10(Math.Max(1e-30, mixer.StemSumSq[q] / bin));
                for (int w = 0; w < 5; w++)
                {
                    double ss = 0;
                    for (int i = w * bin / 5; i < (w + 1) * bin / 5; i++)
                    {
                        double l = buf[2 * i], r = buf[2 * i + 1]; ss += 0.5 * (l * l + r * r);
                        maxStep = Math.Max(maxStep, Math.Abs(l - prev)); prev = l; tp.Push(l, r);
                    }
                    double e = Math.Sqrt(ss / (bin / 5));
                    if (env >= 0) maxEnvStep = Math.Max(maxEnvStep, Math.Abs(e - env));
                    env = e;
                }
            }
            // bins: s = k - 15 + 0.25 b; the ducked stretch [k - 3.4, k] is b 47-60, the reference [k - 12, k - 7] is b 12-32
            foreach (int q in new[] { NocturneScore.R, NocturneScore.H, NocturneScore.T })
            {
                double refDb = MeanDb(stem, q, 12, 32), duck = MeanDb(stem, q, 48, 59);
                Assert.Less(duck - refDb, -16.0, NocturneScore.LayerNames[q] + " ducks");
            }
            // C07: the pad (after its reverb) and the wind duck by their own depths; the footsteps are off in this render
            {
                double refDb = MeanDb(stem, NocturneScore.P, 12, 32), duck = MeanDb(stem, NocturneScore.P, 48, 59);
                Assert.AreEqual(DreadScore.DropPadDb, duck - refDb, 2.0, "the pad ducks by DropPadDb (tail included)");
                refDb = MeanDb(stem, NocturneScore.W, 12, 32); duck = MeanDb(stem, NocturneScore.W, 48, 59);
                Assert.AreEqual(DreadScore.DropWindDb, duck - refDb, 1.5, "the wind ducks by DropWindDb (the breath of the place stays)");
                refDb = MeanDb(stem, NocturneScore.E, 12, 32); duck = MeanDb(stem, NocturneScore.E, 48, 59);
                Assert.Greater(duck, refDb, "the swell rises into the drop");
            }
            Assert.Less(maxEnvStep, AudioGates.ChordEnvStepLimit, "20 ms envelope step through the drop");
            Assert.LessOrEqual(maxStep, AudioGates.StepLimit);
            Assert.LessOrEqual(tp.PeakDb, AudioGates.TruePeakLimitDb);
        }

        [Test]
        public void SilenceDrops_TheRestFallsTenDecibels_C07()
        {
            Assert.That(DreadScore.DropPadDb, Is.InRange(-15.0, -10.0), "the pad: -10..-15 dB (C07)");
            Assert.That(DreadScore.DropWindDb, Is.InRange(-6.0, -2.0), "the wind: about -4 dB");
            foreach (double k in DreadScore.DropS)
            {
                double full = k - DreadScore.M(DreadScore.DropLeadSeconds - DreadScore.DropFallSeconds);
                Assert.AreEqual(1.0, DreadScore.DropShape(full), 1e-9);
                Assert.AreEqual(Math.Pow(10.0, DreadScore.DropPadDb / 20.0), DreadScore.DropGainAt(full, DreadScore.DropPadDb), 1e-9);
                Assert.AreEqual(1.0, DreadScore.DropGainAt(k + DreadScore.M(DreadScore.DropReturnSeconds) + 0.01, DreadScore.DropPadDb), 1e-12);
                // everything but E (the whole output with the E stem muted, footsteps on): the drop's full depth vs 3-8 s before
                var st = new OfflineRenderSettings { S0 = k - 25.0, Speed = 2.5, Seconds = 11.0, Seed = 1, Footsteps = true, PrerollSeconds = 4.0 };
                var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
                mixer.SoloMask = ~(1 << NocturneScore.E);
                int bin = Fs / 4, nb = 44;
                var buf = new float[bin * 2];
                var lv = new double[nb];
                for (int b = 0; b < 16; b++) mixer.Process(buf, 2);   // the 4 s pre-roll (standing at k - 25): the reverbs fill
                for (int b = 0; b < nb; b++)
                {
                    mixer.Process(buf, 2);
                    double ss = 0; for (int i = 0; i < bin; i++) ss += 0.5 * (buf[2 * i] * buf[2 * i] + buf[2 * i + 1] * buf[2 * i + 1]);
                    lv[b] = ss / bin;
                }
                // s = k - 25 + 0.625 (b + 0.5): the reference s k - 17..k - 8 is b 12-26, the full depth s k - 3.4..k is b 35-39
                double refP = 0, dP = 0; for (int b = 12; b <= 26; b++) refP += lv[b]; for (int b = 35; b <= 39; b++) dP += lv[b];
                double drop = 10.0 * Math.Log10((dP / 5.0) / (refP / 15.0));
                Assert.LessOrEqual(drop, -10.0, "the rest of the mix falls >= 10 dB at s " + k + " (C07: was -1.2 / -0.8 dB)");
            }
        }

        [Test]
        public void ChimeClearing_TrafficDucksAroundTheDoorChime_C53()
        {
            double c = DreadScore.ChimeS;
            Assert.AreEqual(226.25, c, 1e-9, "BV's door-close time");
            Assert.AreEqual(0.0, DreadScore.ChimeDuckShape(c - DreadScore.M(DreadScore.ChimeDuckLeadSeconds) - 0.01), 1e-12);
            Assert.AreEqual(1.0, DreadScore.ChimeDuckShape(c + DreadScore.M(1.0)), 1e-9, "full while the two notes sound");
            Assert.AreEqual(0.0, DreadScore.ChimeDuckShape(c + DreadScore.M(DreadScore.ChimeDuckHoldSeconds + DreadScore.DropReturnSeconds) + 0.01), 1e-12);
            Assert.AreEqual(-40.0, DreadScore.ChimeDb, 1e-9, "the chime is 3 dB up (C53)");
            var st = new OfflineRenderSettings { S0 = c - 15.0, Speed = 2.5, Seconds = 9.0, Seed = 1, Footsteps = false };
            var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
            mixer.MeterStems = true;
            int bin = Fs / 4, nb = 36;
            var buf = new float[bin * 2];
            var t = new double[nb];
            for (int b = 0; b < nb; b++)
            {
                Array.Clear(mixer.StemSumSq, 0, mixer.StemSumSq.Length);
                mixer.Process(buf, 2);
                t[b] = mixer.StemSumSq[NocturneScore.T] / bin;
            }
            // s = c - 15 + 0.625 (b + 0.5): before s c - 14..c - 6 is b 1-13; the chime s c + 0.6..c + 6 is b 25-33
            double a0 = 0, a1 = 0; for (int b = 1; b <= 13; b++) a0 += t[b]; for (int b = 25; b <= 33; b++) a1 += t[b];
            double duck = 10.0 * Math.Log10((a1 / 9.0) / (a0 / 13.0));
            Assert.AreEqual(DreadScore.ChimeDuckDb, duck, 2.0, "T ducks by ChimeDuckDb under the chime");
            Assert.That(DreadScore.ChimeDuckDb, Is.InRange(-10.0, -5.0));
        }

        // ------------------------------------------------------------------ cue times (DREAD 1.1, 1.3, 2.1, 3.2, 5.4, 6)
        [Test]
        public void Cues_AreKeyedToTheirSAndTimes()
        {
            var ev = DreadScore.Events;
            int horns = 0, creaks = 0, chimes = 0, rumbles = 0;
            double lastStrike = double.NaN, lastGroan = double.NaN;
            for (int i = 0; i < ev.Length; i++)
            {
                var e = ev[i];
                if (i > 0) Assert.GreaterOrEqual(e.S, ev[i - 1].S, "sorted");
                Assert.AreEqual(i, e.Seq);
                switch (e.Kind)
                {
                    case DreadKind.Horn: Assert.AreEqual(DreadScore.HornS[horns++], e.S); break;
                    case DreadKind.Creak:
                    {
                        creaks++; Assert.AreEqual(470.0, e.S);
                        D3 p = NocMath.PathPoint(470.0);
                        double d = Math.Sqrt(Math.Pow(e.X - p.x, 2) + Math.Pow(e.Y - 1.7, 2) + Math.Pow(e.Z - p.z, 2));
                        Assert.AreEqual(d / 343.0, e.Delay, 1e-9, "heard after its flight time");
                        // from the panel's centre at the tear (PanelCourse.Centre(0): 0.45 m off face B's plane; integration round)
                        Assert.AreEqual(DreadScore.PanelX, e.X); Assert.AreEqual(DreadScore.FaceBX, e.X, 0.5); Assert.AreEqual(150.0, e.Y, 1e-3);
                        break;
                    }
                    case DreadKind.Chime:
                        chimes++; Assert.AreEqual(NocturneScore.Vehicles[0].SLaunch - 1.5 * V, e.S, 1e-9, "1.5 s before the tram leaves");
                        break;
                    case DreadKind.Rumble:
                    {
                        Assert.AreEqual(DreadScore.LightningS[rumbles++], e.S);
                        int b = NocMath.BeatIndex(e.S);
                        Assert.IsFalse(b == 0 || b == 4, "no glow in beats 1 and 5");
                        Assert.AreEqual(DreadScore.RumbleLagSeconds, e.Delay);
                        break;
                    }
                    case DreadKind.Strike:
                        Assert.That(e.S, Is.InRange(DreadScore.StrikeS0, DreadScore.StrikeS1));
                        Assert.IsFalse(e.S > DreadScore.ArkQuietS0 && e.S < DreadScore.ArkQuietS1, "no face strike in the Ark's stretch");
                        Assert.IsTrue(e.X == DreadScore.FaceAX || e.X == DreadScore.FaceBX, "inside a Gulf face");
                        if (!double.IsNaN(lastStrike)) Assert.GreaterOrEqual(e.S - lastStrike, 25.0 * V - 1e-9, "strikes 25-40 s apart");
                        lastStrike = e.S; break;
                    case DreadKind.Groan:
                        Assert.GreaterOrEqual(e.S, 120.0, "groans from s 120 on");
                        Assert.IsFalse(e.S > DreadScore.ArkQuietS0 && e.S < DreadScore.ArkQuietS1, "no face groan in the Ark's stretch (its own groans)");
                        Assert.IsFalse(e.S > 1340.0 && e.S < 1380.0, "the koi's S7 stays free");
                        if (!double.IsNaN(lastGroan)) { Assert.GreaterOrEqual(e.S - lastGroan, 20.0 * V - 1e-9); }
                        Assert.AreEqual(0, e.Ark);
                        lastGroan = e.S; break;
                }
            }
            Assert.AreEqual(3, horns); Assert.AreEqual(1, creaks); Assert.AreEqual(1, chimes); Assert.AreEqual(DreadScore.LightningS.Length, rumbles);
            Assert.AreEqual(new[] { 440.0, 740.0, 1090.0 }, DreadScore.HornS, "the Ark's calls: horn 2 at s 740 (C08: s 700 sat on T4's meet), horn 3 as the stern leaves");
            for (int i = 1; i < DreadScore.LightningS.Length; i++) Assert.GreaterOrEqual(DreadScore.LightningS[i] - DreadScore.LightningS[i - 1], 25.0 * V, "glows at most once per 25 s");
            // C08: no horn, strike or groan within 15 m of a vehicle meet at 2.5 m/s (the tram at s 272, T1-T7)
            Assert.AreEqual(272.0, NocturneScore.MeetS(0, 2.5), 0.1, "the tram meet");
            foreach (var e in ev)
            {
                if (e.Kind != DreadKind.Horn && e.Kind != DreadKind.Strike && e.Kind != DreadKind.Groan) continue;
                for (int i = 0; i < NocturneScore.Vehicles.Length; i++)
                    Assert.GreaterOrEqual(Math.Abs(e.S - NocturneScore.MeetS(i, 2.5)), DreadScore.MeetClearM, e.Kind + " at s " + e.S + " vs " + NocturneScore.Vehicles[i].Name);
            }
            // the panel's creak comes from DR's PanelCourse
            // (integration round, DR's request: from the panel's centre at the tear, PanelCourse.Centre(0) = (814.85, 150, 800))
            UnityEngine.Vector3 pc0 = PanelCourse.Centre(0.0);
            Assert.AreEqual(PanelCourse.LaunchS, DreadScore.PanelS); Assert.AreEqual(pc0.x, DreadScore.PanelX); Assert.AreEqual(pc0.y, DreadScore.PanelY); Assert.AreEqual(pc0.z, DreadScore.PanelZ);
            Assert.AreEqual(814.85, DreadScore.PanelX, 1e-3); Assert.AreEqual(PanelCourse.Y0, DreadScore.PanelY, 1e-3); Assert.AreEqual(PanelCourse.Z0, DreadScore.PanelZ, 1e-3);
            // D-043: no glow and no rumble inside the silence before the Ark's bow (s at 2.5 m/s)
            double q0 = DreadScore.ArkBowOverS + DreadScore.ArkDropFall0 * V, q1 = DreadScore.ArkBowOverS + DreadScore.ArkDropRet1 * V;
            foreach (double s in DreadScore.LightningS)
            {
                double sr = s + DreadScore.M(DreadScore.RumbleLagSeconds);
                Assert.IsFalse(s > q0 && s < q1, "glow at s " + s + " in the Ark's silence");
                Assert.IsFalse(sr > q0 && sr < q1, "rumble at s " + sr + " in the Ark's silence");
            }
            // strikes and groans fill the intervals (no gap of more than 2 x 40 s / 2 x 60 s: one skipped slot at most)
            int strikes = 0, groans = 0; double gPrev = double.NaN;
            foreach (var e in ev)
            {
                if (e.Kind == DreadKind.Strike) strikes++;
                if (e.Kind != DreadKind.Groan) continue;
                groans++;
                if (!double.IsNaN(gPrev) && !(gPrev < DreadScore.ArkQuietS0 && e.S > DreadScore.ArkQuietS1)) Assert.LessOrEqual(e.S - gPrev, 2.0 * (60.0 * V + 30.0), "a groan gap spans at most one cleared slot");
                gPrev = e.S;
            }
            Assert.GreaterOrEqual(strikes, 1, "face strikes in beat 3 (the Ark's stretch has its own groans)"); Assert.GreaterOrEqual(groans, 5, "face groans outside the Ark's stretch");
            Assert.GreaterOrEqual(DreadScore.ArkGroanTau.Length, 5, "the Ark's groans");
        }

        [Test]
        public void Voices_AreUnitLoudnessAtGainOne()
        {
            foreach (DreadKind k in Enum.GetValues(typeof(DreadKind)))
            {
                double db = 20.0 * Math.Log10(DreadProbe.UnitRms(k, Fs));
                Assert.AreEqual(0.0, db, 0.5, k + ": peak 1 s RMS at gain 1");
            }
        }

        // ------------------------------------------------------------------ THE ARK (D-043, Logs/ark/ARK.md)
        [Test]
        public void Ark_CourseIsTheArksNumbers_D043()
        {
            // DR's course (HullCourse of the abyss copy, D-043; ArkCourseMirror until the merge): 5 000 x 440 m, belly y 330, x 515.3,
            // from the north (+z) toward -z bow first, 27 m/s cruise after the approach, launched at s 380
            Assert.AreEqual(5000.0, DreadScore.ArkLength); Assert.LessOrEqual(DreadScore.ArkBeam, 440.0); Assert.AreEqual(330.0, DreadScore.ArkBellyY);
            Assert.AreEqual(515.3, DreadScore.ArkX, 1e-9); Assert.That(DreadScore.ArkCruise, Is.InRange(22.0, 28.0));
            Assert.AreEqual(-1.0, DreadScore.ArkDir);
            Assert.AreEqual(DreadScore.WalkAxisZ, DreadScore.ArkBowZ(DreadScore.ArkBowOverTau), 1e-6, "the bow over z 195 at ArkBowOverTau");
            Assert.That(DreadScore.ArkBowOverS, Is.InRange(585.0, 640.0), "the bow passes over early on the bridge (ARK.md: about s 590-640)");
            Assert.That(DreadScore.ArkSternOverS, Is.InRange(880.0, 1080.0), "the stern leaves the bridge's sky while the walker is in the garden (beat 5)");
            Assert.AreEqual(DreadScore.WalkAxisZ, DreadScore.ArkSternZ(DreadScore.ArkSternOverTau), 1e-6);
            Assert.AreEqual(DreadScore.ArkLength, DreadScore.ArkSternZ(7.0) - DreadScore.ArkBowZ(7.0), 1e-9, "the stern trails north of the bow");
            Assert.AreEqual(0.0, DreadScore.ArkVelocity(0.0));
            for (double tau = 1.0; tau < 300.0; tau += 1.0)
            {
                double dz = DreadScore.ArkBowZ(tau + 0.5) - DreadScore.ArkBowZ(tau - 0.5);
                Assert.AreEqual(DreadScore.ArkVz(tau), dz, 0.05, "the velocity is the course's derivative at tau " + tau);
                Assert.Less(dz, 0.0, "it moves south");
            }
            Assert.That(DreadScore.ArkVelocity(DreadScore.ArkBowOverTau), Is.InRange(27.0, 30.0), "about 28 m/s over the walker");
            // the long Doppler of the bow wave: higher while it comes (from the north), 1 overhead, lower once it has passed south
            double v = DreadScore.ArkVz(DreadScore.ArkBowOverTau);
            double up = DreadScore.PointDoppler(515.3, 390.0, 195.0 + 900.0, v, 455.0, 1.7, 195.0);
            double over = DreadScore.PointDoppler(515.3, 390.0, 195.0, v, 455.0, 1.7, 195.0);
            double dn = DreadScore.PointDoppler(515.3, 390.0, 195.0 - 900.0, v, 455.0, 1.7, 195.0);
            Assert.Greater(up, 1.06); Assert.AreEqual(1.0, over, 1e-9); Assert.Less(dn, 0.945);
            Assert.LessOrEqual(up, 343.0 / (343.0 + v) + 1e-9); Assert.GreaterOrEqual(dn, 343.0 / (343.0 - v) - 1e-9);
        }

        [Test]
        public void Ark_MirrorIsDrsCourse_D043()
        {
            // the numbers DR's HullCourse (abyss copy, 20:41) gives; after the merge ArkC is HullCourse itself and this still holds
            Assert.AreEqual(380.0, DreadScore.ArkLaunchS); Assert.AreEqual(86.4, DreadScore.ArkBowOverTau, 1e-9); Assert.AreEqual(596.0, DreadScore.ArkBowOverS, 1e-9);
            Assert.AreEqual(270.3, DreadScore.ArkSternOverTau, 0.1); Assert.AreEqual(1055.7, DreadScore.ArkSternOverS, 0.3);
            Assert.AreEqual(195.0 + 3422.0, DreadScore.ArkBowZ(1e-9), 2.0, "the bow 3.4 km north at the launch");
            Assert.AreEqual(72.0, DreadScore.ArkVelocity(1e-9), 1e-6); Assert.AreEqual(28.4, DreadScore.ArkVelocity(DreadScore.ArkBowOverTau), 0.1);
        }

        [Test]
        public void Ark_DroneGrowsOutOfTheHazeAndGlidesDownOverhead_D043()
        {
            var before = RenderStem(DreadScore.ArkLaunchS - 10.0, 3.5, 2.5, 4, 0.5, out _);
            Assert.Less(Max(before, 0, before.Length - 1), -90.0, "no Ark before its launch at s 380");
            var far = RenderStem(436.0, 6.0, 2.5, 4, 0.5, out _);
            var bow = RenderStem(DreadScore.ArkBowOverS - 4.0, 8.0, 2.5, 4, 0.5, out _);
            var under = RenderStem(740.0, 8.0, 2.5, 4, 0.5, out NocMixer mu);
            double lFar = Max(far, 4, far.Length - 1), lBow = Max(bow, 4, bow.Length - 1), lUnder = Max(under, 4, under.Length - 1);
            UnityEngine.Debug.Log(FormattableString.Invariant($"[AU] Ark drone: s 440 {lFar:0.0}, bow pass {lBow:0.0}, under the belly s 745 {lUnder:0.0} dBFS"));
            Assert.Greater(lBow - lFar, 8.0, "it grows out of the haze: >= 8 dB from s 440 to the bow's pass");
            Assert.That(lUnder, Is.InRange(DreadScore.ArkPeakDb - 5.0, DreadScore.ArkPeakDb + 3.0), "under the belly (dBFS)");
            Assert.Greater(mu.Dread.Ark.Tau, 0.0); Assert.AreEqual(1.0, mu.Dread.Ark.Cover, 0.02, "under the belly mid-bridge");
            // the bow wave's Doppler glides down through the pass (sampled from the drone itself, every 0.5 s over 32 s)
            var st = new OfflineRenderSettings { S0 = DreadScore.ArkBowOverS - 40.0, Speed = 2.5, Seconds = 32.0, Seed = 1, Footsteps = false };
            var mx = OfflineAudioRenderer.CreateMixer(st, out _);
            mx.SoloMask = 1 << NocturneScore.E; mx.Dread.PartMask = 4;
            var buf = new float[2 * Fs / 2];
            double first = double.NaN, last = 0.0, prev = 9.0;
            for (int i = 0; i < 64; i++)
            {
                mx.Process(buf, 2);
                double d = mx.Dread.Ark.BowDoppler;
                if (double.IsNaN(first)) first = d;
                Assert.LessOrEqual(d, prev + 1e-9, "the glide only falls (bin " + i + ")"); prev = d; last = d;
            }
            Assert.Greater(first, 1.04, "16 s before the pass: higher"); Assert.Less(last, 0.97, "16 s after: lower");
        }

        [Test]
        public void Ark_SilenceFallsBeforeTheBowPassesOverhead_D043()
        {
            double tb = DreadScore.ArkBowOverTau, z = DreadScore.WalkAxisZ;
            // swept on the Ark's own clock: 0 until 9 s before the bow is over (at its speed then), 1 from 7 s before to 2 s after, 0 from 7 s
            double last = 0.0;
            for (double tau = tb - 20.0; tau <= tb + 15.0; tau += 0.01)
            {
                double t = DreadScore.ArkBowSeconds(tau, z), e = DreadScore.ArkDropShape(tau, z);
                if (t <= DreadScore.ArkDropFall0 || t >= DreadScore.ArkDropRet1) Assert.AreEqual(0.0, e, 1e-12, "outside the silence at bow " + t + " s");
                else if (t >= DreadScore.ArkDropFall1 && t <= DreadScore.ArkDropRet0) Assert.AreEqual(1.0, e, 1e-9, "held through the pass at bow " + t + " s");
                if (t < DreadScore.ArkDropFall1) { Assert.GreaterOrEqual(e, last - 1e-12, "it only falls in"); last = e; }
            }
            Assert.AreEqual(0.0, DreadScore.ArkBowSeconds(tb, z), 1e-9);
            // keyed to the bow, not to s: a listener the bow reaches 10 s later is held then
            Assert.AreEqual(1.0, DreadScore.ArkDropShape(tb + 10.0, DreadScore.ArkBowZ(tb + 10.0)), 1e-9);
            Assert.AreEqual(0.0, DreadScore.ArkDropShape(0.0, z));
            // everything but E (footsteps on) falls >= 10 dB in the hold against 18-28 s before the pass
            var st = new OfflineRenderSettings { S0 = DreadScore.ArkBowOverS - 75.0, Speed = 2.5, Seconds = 34.0, Seed = 1, Footsteps = true, PrerollSeconds = 4.0 };
            var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
            mixer.SoloMask = ~(1 << NocturneScore.E);
            int bin = Fs / 4, nb = 136;
            var buf = new float[bin * 2];
            var lv = new double[nb];
            for (int b = 0; b < 16; b++) mixer.Process(buf, 2);
            double maxStep = 0, prev = 0;
            for (int b = 0; b < nb; b++)
            {
                mixer.Process(buf, 2);
                double ss = 0;
                for (int i = 0; i < bin; i++) { ss += 0.5 * (buf[2 * i] * buf[2 * i] + buf[2 * i + 1] * buf[2 * i + 1]); maxStep = Math.Max(maxStep, Math.Abs(buf[2 * i] - prev)); prev = buf[2 * i]; }
                lv[b] = ss / bin;
            }
            // t = 0.25 (b + 0.5) s from the walk's start at s_bow - 75; the Ark's clock ran on through the 4 s pre-roll (real time),
            // so the bow is over at t 26: the reference bow -18..-10 s (after KS09's drop) is b 32-63, the hold bow -5..+2.5 s b 84-113
            double refP = 0, dP = 0; for (int b = 32; b <= 63; b++) refP += lv[b]; for (int b = 84; b <= 113; b++) dP += lv[b];
            double drop = 10.0 * Math.Log10((dP / 30.0) / (refP / 32.0));
            UnityEngine.Debug.Log(FormattableString.Invariant($"[AU] the Ark's silence: the rest {drop:0.0} dB"));
            Assert.LessOrEqual(drop, -10.0, "the rest of the mix falls >= 10 dB before the bow passes overhead");
            Assert.LessOrEqual(maxStep, AudioGates.StepLimit);
        }

        [Test]
        public void Ark_CallsAndGroansComeFromTheArk_D043()
        {
            var g = DreadScore.ArkGroanTau;
            Assert.GreaterOrEqual(g.Length, 5);
            Assert.That(g[0] - DreadScore.ArkBowOverTau, Is.InRange(0.0, 2.0), "the first groan as the bow passes, inside the silence");
            for (int k = 1; k < g.Length; k++)
            {
                Assert.GreaterOrEqual(g[k] - g[k - 1], DreadScore.ArkGroanGapMin - 1e-9, "slow: >= 22 s apart");
                double s = DreadScore.ArkLaunchS + V * g[k];
                for (int i = 0; i < NocturneScore.Vehicles.Length; i++) Assert.GreaterOrEqual(Math.Abs(s - NocturneScore.MeetS(i, V)), DreadScore.ArkGroanMeetClearM, "Ark groan at s " + s);
            }
            for (int k = 0; k < g.Length; k++) Assert.That(DreadScore.ArkGroanHz(k), Is.InRange(20.0, 32.0));
            Assert.LessOrEqual(g[g.Length - 1], DreadScore.ArkSternOverTau + 40.0);
            // overhead: a groan under the middle of the ship comes from the belly 328 m up, at ArkGroanDb
            double d = DreadScore.ArkGroanSource(1, 140.0, 515.3, 195.0, out _, out double y, out _);
            Assert.AreEqual(DreadScore.ArkBellyY + 12.0, y, 1e-9); Assert.That(d, Is.InRange(330.0, 420.0));
            Assert.AreEqual(DreadScore.ArkGroanDb, DreadScore.ArkGroanLevelDb(DreadScore.ArkBellyY - 1.7), 1e-9);
            // the calls: bow far south, bow far north, the stern leaving north; chord tones of the chord sounding when heard
            int n = 0;
            foreach (var e in DreadScore.Events)
            {
                if (e.Kind != DreadKind.Horn) continue;
                Assert.AreEqual(DreadScore.HornPart[n], e.Ark);
                if (n == 0) Assert.Greater(e.Z, 195.0 + 1000.0, "call 1 from the bow coming out of the northern haze");
                else Assert.Less(e.Z, 195.0 - 200.0, "calls 2 and 3 from the south, the ship going away");
                double heard = e.S + e.Delay * V;
                int c = NocturneScore.ChordIndex(heard, out double w);
                foreach (double hz in new[] { e.Hz, 1.5 * e.Hz })
                {
                    int pc = ((int)Math.Round(69.0 + 12.0 * Math.Log(hz / 440.0, 2.0)) % 12 + 12) % 12;
                    bool tone = false;
                    foreach (int m in NocturneScore.Chords[c]) if (m % 12 == pc) tone = true;
                    if (!tone && w < 0.999 && c > 0) foreach (int m in NocturneScore.Chords[c - 1]) if (m % 12 == pc) tone = true;
                    Assert.IsTrue(tone, "horn " + n + " " + hz + " Hz is a chord tone at s " + heard);
                }
                n++;
            }
            Assert.AreEqual(3, n);
            // on the Ark's clock: at 1.5 m/s too every call comes from within 3.5 km and lands on a chord tone
            n = 0;
            foreach (var e in DreadScore.Events)
            {
                if (e.Kind != DreadKind.Horn) continue;
                double ct = DreadScore.ArkCallTau(e.S), sw = DreadScore.ArkLaunchS + 1.5 * ct;
                D3 p = NocMath.PathPoint(sw);
                DreadScore.ArkHornSource(e.Ark, ct, p.x, p.z, out _, out _, out _, out double hd, out _);
                Assert.LessOrEqual(hd, DreadScore.ArkHornMaxDist, "call " + n + " at 1.5 m/s (s " + sw + ")");
                double heard = sw + hd / DreadScore.SoundSpeed * 1.5;
                int c = NocturneScore.ChordIndex(heard, out double w);
                foreach (double hz in new[] { e.Hz, 1.5 * e.Hz })
                {
                    int pc = ((int)Math.Round(69.0 + 12.0 * Math.Log(hz / 440.0, 2.0)) % 12 + 12) % 12;
                    bool tone = false;
                    foreach (int m in NocturneScore.Chords[c]) if (m % 12 == pc) tone = true;
                    if (!tone && w < 0.999 && c > 0) foreach (int m in NocturneScore.Chords[c - 1]) if (m % 12 == pc) tone = true;
                    Assert.IsTrue(tone, "1.5 m/s: horn " + n + " " + hz + " Hz is a chord tone at s " + heard);
                }
                n++;
            }
            // the first call is heard after its flight from the bow (about 5 s at 1.7 km)
            double delay0 = 0.0; foreach (var e in DreadScore.Events) if (e.Kind == DreadKind.Horn) { delay0 = e.Delay; break; }
            var bins = RenderStem(438.0, 14.0, 2.5, 1, 0.5, out _);
            int on = -1; for (int i = 0; i < bins.Length; i++) if (bins[i] > -70.0) { on = i; break; }
            Assert.GreaterOrEqual(on, 0, "the call sounds");
            Assert.That(0.5 * on, Is.InRange(0.8 + 0.6 * delay0, 0.8 + delay0 + 1.5), "after the flight time (" + delay0 + " s)");
        }

        [Test]
        public void Ark_CityComesBackWhenTheSternLeaves_D043()
        {
            Assert.AreEqual(0.0, DreadScore.ArkCover(DreadScore.ArkBowOverTau - 10.0, 455.0, 195.0), 1e-12, "not yet under");
            Assert.AreEqual(1.0, DreadScore.ArkCover(DreadScore.ArkBowOverTau + 30.0, 455.0, 195.0), 1e-9, "under the belly");
            Assert.AreEqual(0.0, DreadScore.ArkCover(DreadScore.ArkSternOverTau + 10.0, 455.0, 195.0), 1e-12, "the stern has gone north");
            Assert.That(DreadScore.ArkCoverDb, Is.InRange(-10.0, -4.0));
            // the mixer reads the drone's cover (the city hum and the traffic wash duck under the belly)
            var st = new OfflineRenderSettings { S0 = 745.0, Speed = 2.5, Seconds = 2.0, Seed = 1, Footsteps = false };
            var mx = OfflineAudioRenderer.CreateMixer(st, out _);
            var buf = new float[2 * Fs];
            mx.Process(buf, 2);
            Assert.AreEqual(1.0, mx.Dread.ArkCover, 0.02);
        }

        // ------------------------------------------------------------------ 2.2 the hums on DR's cabins and gantries (AU-28)
        [Test]
        public void Hums_SitOnDrsCabinsAndGantries()
        {
            // integration round (DR's request, AU-28): one hum per DR shaft (its loudest cabin) and per gantry, from DreadCabinCourse
            Assert.AreEqual(DreadCabinCourse.ShaftCount + DreadCabinCourse.GantryCount, DreadScore.HumCount);
            Assert.AreEqual(DreadScore.HumCount, DreadScore.HumHz.Length);
            Assert.AreEqual(10, DreadScore.HumCount, "8 shafts + 2 gantries");
            var cab = DreadCabinCourse.Cabins();
            double lx = 300.0, ly = NocturneConstants.EyeHeight, lz = 195.0;
            foreach (double t in new[] { 0.0, 37.5, 410.0, 1234.5, 65530.0 })
            {
                for (int i = 0; i < DreadScore.HumCount; i++)
                {
                    double g = DreadScore.HumSource(i, t, lx, ly, lz, out double x, out double y, out double z);
                    Assert.That(g, Is.InRange(0.0, 1.0));
                    if (DreadScore.HumIsGantry(i))
                    {
                        UnityEngine.Vector3 gp = DreadBuilder.GantryPos(i - DreadCabinCourse.ShaftCount, t);
                        Assert.AreEqual(gp.x, x, 1e-3); Assert.AreEqual(gp.y, y, 1e-3); Assert.AreEqual(gp.z, z, 1e-2, "gantry " + i + " at t " + t);
                        continue;
                    }
                    Assert.AreEqual(DreadCabinCourse.Shafts[i, 0], x); Assert.AreEqual(DreadCabinCourse.Shafts[i, 1], z);
                    bool onCabin = false;
                    for (int c = 0; c < DreadCabinCourse.CabinsPerShaft; c++)
                        if (Math.Abs(DreadCabinCourse.CabinY(in cab[i * DreadCabinCourse.CabinsPerShaft + c], t, out _) - y) < 1e-9) onCabin = true;
                    Assert.IsTrue(onCabin, "hum " + i + " sits on one of its shaft's cabins at t " + t);
                }
            }
            // the picture's twin: DreadCabins (meshes) reads the same cabins; the course's hash is NocHash.H3 / U01
            var pic = DreadCabins.Cabins();
            Assert.AreEqual(cab.Length, pic.Count);
            for (int k = 0; k < cab.Length; k++) { Assert.AreEqual(pic[k].Speed, cab[k].Speed); Assert.AreEqual(pic[k].Phase, cab[k].Phase); Assert.AreEqual(pic[k].Dir, cab[k].Dir); }
            for (uint a = 0; a < 9; a++) for (uint b = 0; b < 5; b++)
                    Assert.AreEqual(NocHash.H3(a, b, 0xD4EADu, 0), DreadCabinCourse.H3(a, b, 0xD4EADu));
            // no gain step from a cabin's hidden wrap or a change of the loudest cabin (60 fps, 20 min), anywhere on face A / B
            foreach (double px in new[] { 230.0, 515.0, 800.0 })
                for (int i = 0; i < DreadScore.HumCount; i++)
                {
                    double prev = DreadScore.HumSource(i, 0.0, px, ly, lz, out _, out _, out _), worst = 0.0;
                    for (double t = 1.0 / 60.0; t < 1200.0; t += 1.0 / 60.0)
                    {
                        double g = DreadScore.HumSource(i, t, px, ly, lz, out _, out _, out _);
                        worst = Math.Max(worst, Math.Abs(g - prev)); prev = g;
                    }
                    Assert.Less(worst, 0.002, "hum " + i + " gain step per frame at x " + px);
                }
            // the loudest hum on the walk (reported: DR's shafts stand 135 m or more from the walk, so the hums are far and faint)
            double loud = -999.0, loudS = 0.0;
            for (double s = 0.0; s <= NocturneConstants.GoalS; s += 5.0)
            {
                D3 p = NocMath.PathPoint(s);
                for (int i = 0; i < DreadScore.HumCount; i++)
                {
                    double g = DreadScore.HumSource(i, s / 2.5, p.x, ly, p.z, out _, out _, out _);
                    double db = g > 0.0 ? DreadScore.HumDb(i) + 20.0 * Math.Log10(g) : -999.0;
                    if (db > loud) { loud = db; loudS = s; }
                }
            }
            UnityEngine.Debug.Log(FormattableString.Invariant($"[AU] loudest machine hum on the walk (2.5 m/s): {loud:0.0} dBFS at s {loudS:0}"));
            Assert.Less(loud, DreadScore.GantryHumDb + 1e-9);
        }

        // ------------------------------------------------------------------ C09 small speakers, C49 the hum's infrasound
        [Test]
        public void SmallSpeakerLayer_SwellsCarryChordTonesAbove200Hz_C09()
        {
            for (int k = 0; k < DreadScore.SwellS.Length; k++)
            {
                int c = NocturneScore.ChordIndex(DreadScore.SwellS[k], out double w);
                if (w < 0.5 && c > 0) c--;
                var pcs = new System.Collections.Generic.HashSet<int>();
                foreach (int note in NocturneScore.Chords[c]) pcs.Add(((note % 12) + 12) % 12);
                Assert.LessOrEqual(DreadScore.SwellUpperH[k].Length, DreadScore.SwellUpperMax);
                Assert.AreEqual(DreadScore.SwellUpperH[k].Length, DreadScore.SwellUpperAmp[k].Length);
                foreach (int h in DreadScore.SwellUpperH[k])
                {
                    double f = h * DreadScore.SwellHz[k];
                    Assert.That(f, Is.InRange(DreadScore.SwellUpperLoHz, DreadScore.SwellUpperHiHz), DreadScore.SwellNames[k] + " harmonic " + h);
                    double midi = 69.0 + 12.0 * Math.Log(f / 440.0, 2.0);
                    int pc = (((int)Math.Round(midi)) % 12 + 12) % 12;
                    Assert.Less(Math.Abs(midi - Math.Round(midi)), 0.2, DreadScore.SwellNames[k] + " harmonic " + h + " within 20 cents of a note");
                    Assert.IsTrue(pcs.Contains(pc), DreadScore.SwellNames[k] + " harmonic " + h + " is a tone of " + NocturneScore.ChordNames[c]);
                }
                // rendered: the swell's power above 200 Hz (4th-order Butterworth high-pass) is >= 4 % of its total
                var y = RenderSolo(DreadScore.SwellS[k] - 4.0, 4.0, 2.5, 1 << NocturneScore.E, 2);
                double all = 0, hi = 0;
                var h1 = new SvfD(); h1.Set(200.0, 0.5412, Fs); var h2 = new SvfD(); h2.Set(200.0, 1.3066, Fs);
                for (int i = Fs; i < y.Length / 2; i++) { double m = 0.5 * (y[2 * i] + y[2 * i + 1]); double q = h2.High(h1.High(m)); all += m * m; hi += q * q; }
                Assert.Greater(10.0 * Math.Log10(hi / all), -14.0, DreadScore.SwellNames[k] + ": share above 200 Hz (dB; partials 1-5 alone: about -20)");
            }
            foreach (int h in ArkDrone.UpHarm) Assert.That(h * ArkDrone.EngineHz, Is.InRange(180.0, 500.0), "the Ark's small-speaker harmonic " + h);
        }

        [Test]
        public void Koi_ReachesLaptopBands_C09()
        {
            bool e4 = false, b4 = false;
            for (int i = 0; i < KoiTone.Ratio.Length; i++) { if (KoiTone.Ratio[i] == 8.0) e4 = true; if (KoiTone.Ratio[i] == 12.0) b4 = true; }
            Assert.IsTrue(e4 && b4, "the 8th (329.6 Hz) and 12th (494.4 Hz) partials");
            var y = RenderSolo(1350.0, 6.0, 2.5, 1 << NocturneScore.K, -1);
            double all = 0, hi = 0;
            var h1 = new SvfD(); h1.Set(200.0, 0.5412, Fs); var h2 = new SvfD(); h2.Set(200.0, 1.3066, Fs);
            for (int i = Fs; i < y.Length / 2; i++) { double m = 0.5 * (y[2 * i] + y[2 * i + 1]); double q = h2.High(h1.High(m)); all += m * m; hi += q * q; }
            Assert.Greater(all, 1e-9, "the koi sounds in S7");
            Assert.Greater(10.0 * Math.Log10(hi / all), -11.0, "koi share above 200 Hz (dB; was -18)");
        }

        [Test]
        public void CityHum_KeepsItsPowerAboveThirtyHertz_C49()
        {
            foreach (double s0 in new[] { 60.0, 420.0, 600.0, 1200.0 })
            {
                var y = RenderSolo(s0, 6.0, 2.5, 1 << NocturneScore.H, -1);
                double all = 0, lo = 0;
                var l1 = new SvfD(); l1.Set(30.0, 0.5412, Fs); var l2 = new SvfD(); l2.Set(30.0, 1.3066, Fs);
                for (int i = 2 * Fs; i < y.Length / 2; i++) { double m = 0.5 * (y[2 * i] + y[2 * i + 1]); double q = l2.Low(l1.Low(m)); all += m * m; lo += q * q; }
                Assert.Less(lo / all, 0.15, "H power below 30 Hz at s " + s0 + " (C49: was 20-56 %)");
            }
        }

        // ------------------------------------------------------------------ gates through the cues
        [Test]
        public void Gates_ThroughTheCuesAtBothSpeeds()
        {
            // the creak (s 470) and the horn (s 440) at 2.5; the horn at the C4->C5 change (s 700) at 1.5; KS17's drop and swell
            foreach (var w in new[] { new[] { 436.0, 2.5, 16.0 }, new[] { 694.0, 1.5, 12.0 }, new[] { 1280.0, 2.5, 12.0 }, new[] { 222.0, 2.5, 10.0 } })
            {
                var an = AudioGates.RenderStretch(w[0], w[2], w[1], 1, 3.0, true);
                Assert.AreEqual(0, an.NonFinite);
                Assert.LessOrEqual(an.TruePeak.PeakDb, AudioGates.TruePeakLimitDb, "true peak from s " + w[0]);
                Assert.LessOrEqual(an.MaxStep, AudioGates.StepLimit, "sample step from s " + w[0]);
                for (int c = 0; c < NocturneScore.ChordChangeS.Length; c++) Assert.Less(an.ChordMaxEnvStep[c], AudioGates.ChordEnvStepLimit, "20 ms envelope step, C" + c);
                // the dread layer never covers the pad: its 1 s level stays under the pad's except while a one-shot sounds
                double eMax = -300.0;
                for (int i = 0; i < an.BinStems.Count; i++) eMax = Math.Max(eMax, WalkAnalysis.Db(an.BinStems[i][NocturneScore.E]));
                Assert.Less(eMax, -30.0, "E peaks (100 ms) from s " + w[0]);
            }
        }

        [Test]
        public void DreadLayer_CallbackSizeInvariantAndAllocationFree()
        {
            int total = 6 * Fs;
            float[] reference = null;
            foreach (var pat in new[] { new[] { 1024 }, new[] { 37, 2048, 512, 1, 333 } })
            {
                var st = new OfflineRenderSettings { S0 = 466.0, Speed = 2.5, Seconds = 6.0, Seed = 3 };   // the creak fires at s 470
                var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
                mixer.SoloMask = 1 << NocturneScore.E;
                var outBuf = new float[total * 2];
                int pos = 0, k = 0;
                var b = new float[2 * 2048];
                while (pos < total)
                {
                    int n = Math.Min(pat[k++ % pat.Length], total - pos);
                    var view = n == 2048 ? b : new float[n * 2];
                    mixer.Process(view, 2);
                    Array.Copy(view, 0, outBuf, pos * 2, n * 2);
                    pos += n;
                }
                Assert.GreaterOrEqual(mixer.Dread.Fired, 1, "the creak fired");
                if (reference == null) { reference = outBuf; continue; }
                double md = 0; for (int i = 0; i < outBuf.Length; i++) md = Math.Max(md, Math.Abs(outBuf[i] - reference[i]));
                Assert.AreEqual(0.0, md, 1e-6, "callback pattern " + string.Join(",", pat));
            }
            // zero bytes per Process through a cue
            var m2 = OfflineAudioRenderer.CreateMixer(new OfflineRenderSettings { S0 = 436.0, Speed = 2.5, Seconds = 20.0, Seed = 1 }, out _);
            var buf = new float[2048];
            for (int i = 0; i < 100; i++) m2.Process(buf, 2);
            long c0 = GC.GetAllocatedBytesForCurrentThread();
            var probe = new byte[4096];
            long c1 = GC.GetAllocatedBytesForCurrentThread();
            GC.KeepAlive(probe);
            if (c1 - c0 < 4096) Assert.Pass("GC.GetAllocatedBytesForCurrentThread is not live here; Tools/audio/harness alloc measures 0 bytes on .NET");
            long a0 = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 400; i++) m2.Process(buf, 2);   // s 441.3-462.6: the horn (fired at s 440) sounding, echoed
            long a1 = GC.GetAllocatedBytesForCurrentThread();
            Assert.AreEqual(0, a1 - a0, "NocMixer.Process with the dread layer allocates nothing");
        }

        // ------------------------------------------------------------------ helpers
        /// <summary>Renders the E stem with a part mask (DreadLayer.PartMask) in bins of binSec seconds (dBFS per bin).</summary>
        static double[] RenderStem(double s0, double seconds, double speed, int partMask, double binSec, out NocMixer mixer)
        {
            var st = new OfflineRenderSettings { S0 = s0, Speed = speed, Seconds = seconds, Seed = 1, Footsteps = false };
            mixer = OfflineAudioRenderer.CreateMixer(st, out _);
            mixer.MeterStems = true; mixer.SoloMask = 1 << NocturneScore.E; mixer.Dread.PartMask = partMask;
            int bin = (int)(binSec * Fs), nb = (int)(seconds / binSec);
            var buf = new float[bin * 2];
            var res = new double[nb];
            for (int b = 0; b < nb; b++)
            {
                Array.Clear(mixer.StemSumSq, 0, mixer.StemSumSq.Length);
                mixer.Process(buf, 2);
                res[b] = 10.0 * Math.Log10(Math.Max(1e-30, mixer.StemSumSq[NocturneScore.E] / bin));
            }
            return res;
        }

        /// <summary>Renders the output with a solo mask (and a dread part mask) from s0: interleaved stereo.</summary>
        static float[] RenderSolo(double s0, double seconds, double speed, int soloMask, int partMask)
        {
            var st = new OfflineRenderSettings { S0 = s0, Speed = speed, Seconds = seconds, Seed = 1, Footsteps = false };
            var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
            mixer.SoloMask = soloMask; mixer.Dread.PartMask = partMask;
            var y = new float[(int)(seconds * Fs) * 2];
            mixer.Process(y, 2);
            return y;
        }

        static double Max(double[] a, int i0, int i1) { double m = -300.0; for (int i = i0; i <= i1 && i < a.Length; i++) m = Math.Max(m, a[i]); return m; }

        static double MeanDb(double[,] stem, int q, int b0, int b1)
        {
            double p = 0; int n = 0;
            for (int b = b0; b <= b1; b++) { p += Math.Pow(10.0, stem[b, q] / 10.0); n++; }
            return 10.0 * Math.Log10(p / n);
        }
    }
}
