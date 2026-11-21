// AudioTests: T-AUDIO unit checks of the audio (MASTER_PROMPT 14.3, 14.10.1, 11.9; ADDENDUM_A1 section 2; LEAD DECISIONS 1
// and 3). Imported from Thread's AudioTests (the DSP, footstep and WAV tests keep their names); Wave X adds the score (beat
// levels, S1-S8), the kraken voice (levels per kind, the 80-800 Hz band, infrasound limits, ducking, spatialisation, the
// KrakenCall conversion) and the ending ride (levels, cues). Everything renders offline through the pure BwDsp (no audio
// device, deterministic seed).
using System;
using System.IO;
using Megastructure.Audio;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;   // Unity's documented pattern: NUnit's Is plus AllocatingGCMemory

namespace Megastructure.Tests
{
    public class AudioTests
    {
        const int Seed = 1;
        const int Fs = 48000;
        static double Db(double lin) { return 20.0 * Math.Log10(Math.Max(1e-15, lin)); }

        static BwAudioParams Loud()
        {
            var p = BulwarkScore.Evaluate(300.0, 0.0, 2.5);
            p.master = 1f; p.duck = 1f; p.w = 2f; p.bed = 2f;   // far above any score level: the limiter must hold
            return p;
        }

        [Test]
        public void Dsp_ProcessIsAllocationFree()
        {
            var dsp = new BwDsp(Fs, Seed);
            var p = BulwarkScore.Evaluate(200.0, 0.0, 2.5);
            var buf = new float[1024 * 2];
            dsp.Process(buf, 2, in p);   // warm-up (JIT)
            Assert.That(() => { for (int i = 0; i < 8; i++) dsp.Process(buf, 2, in p); }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void Dsp_IsDeterministicPerSeed()
        {
            var a = new BwDsp(Fs, Seed); var b = new BwDsp(Fs, Seed); var c = new BwDsp(Fs, Seed + 1);
            var p = BulwarkScore.Evaluate(250.0, 0.0, 2.5);
            var x = new float[2048]; var y = new float[2048]; var z = new float[2048];
            for (int k = 0; k < 20; k++) { a.Process(x, 2, in p); b.Process(y, 2, in p); c.Process(z, 2, in p); }
            for (int i = 0; i < x.Length; i++) Assert.That(y[i], Is.EqualTo(x[i]));
            bool differs = false;
            for (int i = 0; i < x.Length; i++) if (x[i] != z[i]) differs = true;
            Assert.That(differs, Is.True, "another seed gives another stream");
        }

        [Test]
        public void Dsp_StableAtDeviceRates([Values(44100, 48000, 96000)] int fs)
        {
            var dsp = new BwDsp(fs, Seed);
            var p = BulwarkScore.Evaluate(500.0, 0.0, 2.5);
            var buf = new float[1024 * 2];
            for (int k = 0; k < fs / 1024 * 3; k++)
            {
                dsp.Process(buf, 2, in p);
                for (int i = 0; i < buf.Length; i++) Assert.That(float.IsNaN(buf[i]) || float.IsInfinity(buf[i]), Is.False);
            }
            Assert.That(dsp.NanResets, Is.EqualTo(0));
        }

        /// <summary>Limiter (C-24, RESEARCH C 5): asymptote -1.42 dBFS, the sample peak of a hot signal stays below -1 dBFS.</summary>
        [Test]
        public void Limiter_AsymptoteMinus1p42AndPeakBelowMinus1dBFS()
        {
            var dsp = new BwDsp(Fs, Seed);
            var p = Loud();
            dsp.SnapTo(in p);
            var buf = new float[1024 * 2];
            double peak = 0;
            for (int k = 0; k < 200; k++)
            {
                dsp.Process(buf, 2, in p);
                for (int i = 0; i < buf.Length; i++) peak = Math.Max(peak, Math.Abs(buf[i]));
            }
            Assert.That(Db(peak), Is.LessThanOrEqualTo(BwAudioSpec.LimiterCeilingDb + 0.01), "sample peak at most the -1.42 dBFS asymptote");
            Assert.That(Db(peak), Is.LessThan(BwAudioSpec.TruePeakMaxDb), "below -1 dBFS");
        }

        [Test]
        public void Svf_StableNearNyquist([Values(44100, 48000)] int fs)
        {
            var f = new SvfTpt();
            f.Set(14000.0, 0.7071, fs);
            var rng = new XorShift32(7);
            float peak = 0f;
            for (int i = 0; i < fs * 2; i++) peak = Math.Max(peak, Math.Abs(f.Low(rng.NextBipolar())));
            Assert.That(peak, Is.LessThan(4f), "the trapezoidal SVF must not blow up at 14 kHz (Chamberlin does)");
        }

        [Test]
        public void Params_AreContinuousInS()
        {
            BwAudioParams prev = BulwarkScore.Evaluate(0.0, 0.0, 2.5);
            for (double s = 0.1; s <= 1500.0; s += 0.1)
            {
                var p = BulwarkScore.Evaluate(s, 0.0, 2.5);
                Assert.That(Math.Abs(Db(p.w + 1e-9) - Db(prev.w + 1e-9)), Is.LessThan(6.0), "wind level jump at s=" + s);
                prev = p;
            }
        }

        [Test]
        public void Footsteps_BankMeetsRecipe()
        {
            var bank = new FootstepBank(Fs, Seed);
            for (int s = 0; s < FootstepBank.SurfaceCount; s++)
                for (int v = 0; v < BwAudioSpec.FootstepVariants; v++)
                {
                    float[] c = bank.Clip(s, v);
                    float peak = 0f;
                    for (int i = 0; i < c.Length; i++) peak = Math.Max(peak, Math.Abs(c[i]));
                    Assert.That(Db(peak), Is.EqualTo(BwAudioSpec.FootPeakDb).Within(0.05), $"surface {s} variant {v} peak");
                    Assert.That(c[0], Is.EqualTo(0f)); Assert.That(c[c.Length - 1], Is.EqualTo(0f));
                }
        }

        [Test]
        public void Footsteps_CadenceAndSurfaces()
        {
            Assert.That(Megastructure.Player.PlayerMath.Stride(1.5, false), Is.EqualTo(0.75).Within(1e-9));
            Assert.That(Megastructure.Player.PlayerMath.Stride(2.5, false), Is.EqualTo(0.85).Within(1e-9));
            Assert.That(Megastructure.Player.PlayerMath.Stride(2.5, true), Is.EqualTo(0.90).Within(1e-9));
        }

        /// <summary>T-AUDIO smoke: a short offline render at both speeds (the full walks are QA runs, Tools/qa/audio_analysis.py).</summary>
        [Test]
        public void Render_ShortWalkAtBothSpeeds([Values(1.5, 2.5)] double speed)
        {
            var st = new OfflineRenderSettings { S0 = 100.0, S1 = 130.0, Speed = speed, Ending = false, Seed = Seed };
            var meter = new WalkLevelMeter();
            var info = OfflineAudioRenderer.Render(st, meter);
            Assert.That(info.NanResets, Is.EqualTo(0));
            Assert.That(meter.NonFinite, Is.EqualTo(0));
            Assert.That(Db(meter.Peak), Is.LessThan(BwAudioSpec.TruePeakMaxDb));
            Assert.That(Math.Abs(meter.Dc(0)), Is.LessThan(0.001), "DC < 0.001");
            Assert.That(info.Steps, Is.GreaterThan(0), "footsteps were mixed");
        }

        /// <summary>Collects the mono energy of an offline render in fixed windows (for the intro checks).</summary>
        sealed class WindowMeter : IAudioBlockSink
        {
            public readonly double[] SumSq;
            public double Peak;
            readonly double m_window;
            int m_fs;
            long m_frame;
            public WindowMeter(double seconds, double window) { m_window = window; SumSq = new double[(int)Math.Ceiling(seconds / window) + 1]; }
            public void Begin(int sampleRate, int channels) { m_fs = sampleRate; }
            public void Write(float[] x, int frames, in AudioBlockInfo info)
            {
                for (int i = 0; i < frames; i++, m_frame++)
                {
                    int w = (int)(m_frame / (m_window * m_fs));
                    double l = x[2 * i], r = x[2 * i + 1];
                    if (w < SumSq.Length) SumSq[w] += 0.5 * (l * l + r * r);
                    Peak = Math.Max(Peak, Math.Max(Math.Abs(l), Math.Abs(r)));
                }
            }
            public void End() { }
            public double RmsDb(int w, int fs) => 10.0 * Math.Log10(Math.Max(1e-30, SumSq[w] / (m_window * fs)));
        }

        /// <summary>
        /// 14.11 / T9: "Black 0-1.0 s with the first wave impact heard at 0.5 s". The bed follows the picture (UiTimeline.Picture = 0
        /// until 1.0 s), the impact bus has its own envelope (open from t 0), so the 0.5 s impact is loud in the black while the
        /// 0-0.45 s window is silent and nothing else is heard before 1.0 s.
        /// </summary>
        [Test]
        public void Intro_ImpactIsHeardInTheBlackAt0p5s()
        {
            Assert.AreEqual(0f, Megastructure.UI.UiTimeline.Picture(0.5f), 0f, "picture black at 0.5 s");
            Assert.AreEqual(1f, Megastructure.UI.UiTimeline.ImpactBus(0.5f), 0f, "impact bus open at 0.5 s");
            Assert.AreEqual(0.5f, Megastructure.UI.UiTimeline.IntroImpact, 0f);
            // the SeaState lattice booms are off here: this checks the 14.11 intro impact alone (the lattice may start one at 0-3 s)
            var st = new OfflineRenderSettings { S0 = 0.0, S1 = 15.0, Speed = 2.5, Ending = false, Intro = true, Seed = Seed, TailSeconds = 0.0, Impacts = false };
            const double win = 0.05;
            var meter = new WindowMeter(OfflineAudioRenderer.Duration(st), win);
            var info = OfflineAudioRenderer.Render(st, meter);
            Assert.That(info.IntroImpactTime, Is.InRange(0.5 - 1024.0 / Fs, 0.5), "impact queued in the block that reaches 0.5 s");
            double before = -999, during = -999;
            for (int w = 0; w < 9; w++) before = Math.Max(before, meter.RmsDb(w, Fs));          // 0.00-0.45 s
            for (int w = 10; w < 14; w++) during = Math.Max(during, meter.RmsDb(w, Fs));        // 0.50-0.70 s
            double late = meter.RmsDb(18, Fs);                                                  // 0.90-0.95 s (still black)
            UnityEngine.Debug.Log($"[BW] intro impact: max RMS before 0.45 s {before:F1} dBFS, 0.50-0.70 s {during:F1} dBFS, 0.90-0.95 s {late:F1} dBFS, peak {Db(meter.Peak):F2} dBFS");
            Assert.That(before, Is.LessThan(-90.0), "silence before the impact");
            Assert.That(during, Is.GreaterThan(-30.0), "the impact is heard over black");
            Assert.That(late, Is.GreaterThan(-60.0), "the boom still rings at 0.9 s, before the picture starts");
            Assert.That(Db(meter.Peak), Is.LessThan(BwAudioSpec.TruePeakMaxDb));
        }

        /// <summary>The impact bus sits after the master fade: master 0 does not silence it; impactBus 0 does.</summary>
        [Test]
        public void Dsp_ImpactBusIgnoresTheMasterFade()
        {
            double Energy(float master, float bus)
            {
                var dsp = new BwDsp(Fs, Seed);
                var p = BulwarkScore.Evaluate(10.0, 0.0, 0.0);
                p.master = master; p.impactBus = bus;
                dsp.SnapTo(in p);
                dsp.TriggerImpact(1f, 0.3f);
                var buf = new float[1024 * 2];
                double e = 0;
                for (int k = 0; k < 24; k++) { dsp.Process(buf, 2, in p); for (int i = 0; i < buf.Length; i++) e += buf[i] * (double)buf[i]; }
                Assert.That(dsp.ImpactsStarted, Is.EqualTo(1));
                return e;
            }
            double busOnly = Energy(0f, 1f), none = Energy(0f, 0f), both = Energy(1f, 1f);
            Assert.That(busOnly, Is.GreaterThan(1.0), "audible with the bed muted");
            Assert.That(none, Is.LessThan(1e-12), "the impact bus envelope gates it");
            Assert.That(both, Is.GreaterThan(0.9 * busOnly), "the impact is still there with the bed up");
            Assert.That(Energy(1f, 0f), Is.GreaterThan(1.0), "the bed sounds when the picture is up");
        }

        [Test]
        public void Dsp_ImpactVoicesAreAllocationFree()
        {
            var dsp = new BwDsp(Fs, Seed);
            var p = BulwarkScore.Evaluate(200.0, 0.0, 2.5);
            var buf = new float[1024 * 2];
            dsp.TriggerImpact(0.8f, -0.2f);
            dsp.Process(buf, 2, in p);   // warm-up (JIT)
            Assert.That(() => { dsp.TriggerImpact(1f, 0.3f, true); for (int i = 0; i < 8; i++) dsp.Process(buf, 2, in p); }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void Wav_RoundTrip([Values(WavFormat.Pcm16, WavFormat.Float32)] WavFormat format)
        {
            string path = Path.Combine(Path.GetTempPath(), "bw_wav_" + format + ".wav");
            var x = new float[4800 * 2];
            for (int i = 0; i < 4800; i++) { x[2 * i] = (float)(0.5 * Math.Sin(2 * Math.PI * 440 * i / 48000.0)); x[2 * i + 1] = -x[2 * i]; }
            WavWriter.Write(path, x, x.Length, 2, 48000, format);
            float[] y = WavWriter.Read(path, out int ch, out int rate);
            Assert.That(ch, Is.EqualTo(2)); Assert.That(rate, Is.EqualTo(48000)); Assert.That(y.Length, Is.EqualTo(x.Length));
            float tol = format == WavFormat.Pcm16 ? 2.5f / 32768f : 0f;
            for (int i = 0; i < x.Length; i++) Assert.That(y[i], Is.EqualTo(x[i]).Within(tol));
            File.Delete(path);
        }
        // ================================================================================== Wave X: score, kraken, ride

        /// <summary>BwBeds generators are unit-RMS, so the score's dB levels are exact (0 dBFS RMS at gain 1, +-0.5 dB).</summary>
        [Test]
        public void Beds_LayersAreUnitRms()
        {
            for (int l = 0; l < BulwarkScore.LayerCount; l++)
                Assert.That(BwAudioExamples.LayerUnitRmsDb(l, 12.0), Is.EqualTo(0.0).Within(0.5), BulwarkScore.LayerNames[l]);
        }

        /// <summary>T-AUDIO: a 50 m render in the middle of every beat (2.5 m/s, footsteps, SeaState booms) hits the 14.3 target:
        /// +-1.5 dB, beat 1 (s 2-97, 38 s) +-3 dB (its level is mostly the 9 s booms).</summary>
        [Test]
        public void Score_BeatLevelsMatchTheTargets()
        {
            for (int b = 0; b < FrameworkConst.BeatCount; b++)
            {
                double mid = b == 6 ? 1400.0 : 0.5 * (BulwarkConst.BeatS[b] + BulwarkConst.BeatS[b + 1]);
                // beat 1: the whole gallery up to the door (38 s, 4 booms on average: the booms are most of its level)
                double s0 = b == 0 ? 2.0 : mid - 25.0, s1 = b == 0 ? 97.0 : mid + 25.0;
                var st = new OfflineRenderSettings { S0 = s0, S1 = s1, Speed = 2.5, Ending = false, Seed = Seed, StartRamp = 0.0, TailSeconds = 0.0 };
                var meter = new WalkLevelMeter();
                OfflineAudioRenderer.Render(st, meter);
                double target = b == 6 ? -25.0 : BulwarkConst.TargetRmsDb[b];
                double tol = b == 0 ? 3.0 : 1.5;
                UnityEngine.Debug.Log($"[BW] score beat {b + 1}: {meter.RmsDb:F2} dBFS (target {target:F1})");
                Assert.That(meter.RmsDb, Is.EqualTo(target).Within(tol), "beat " + (b + 1));
                Assert.That(Db(meter.Peak), Is.LessThan(BwAudioSpec.TruePeakMaxDb));
            }
        }

        /// <summary>14.3 silences and swells keyed to s: S1 no booms 100-108, S2 the world rises from the door (104 -> 111.5),
        /// S4 portal -20 dB within 1.5 m, S5 hall hush (all but P under -50), S7 the conduit drops at the mouth.</summary>
        [Test]
        public void Score_SilencesAndSwells()
        {
            Assert.That(Db(BulwarkScore.Evaluate(104.0, 0.0, 2.5).impactTrim), Is.LessThan(-50.0), "S1: no booms at s 104");
            Assert.That(Db(BulwarkScore.Evaluate(60.0, 0.0, 2.5).impactTrim), Is.GreaterThan(-3.0), "booms in beat 1");
            Assert.That(Db(BulwarkScore.Evaluate(103.0, 0.0, 2.5).w + 1e-12), Is.LessThan(-80.0), "S2: no wind before the door");
            Assert.That(Db(BulwarkScore.Evaluate(112.0, 0.0, 2.5).w), Is.GreaterThan(-32.0), "S2: the wind is up by s 111.5");
            Assert.That(BulwarkScore.Evaluate(100.0, 0.0, 2.5).seaLp, Is.LessThan(300f), "beat 1 sea muffled");
            Assert.That(BulwarkScore.Evaluate(112.0, 0.0, 2.5).seaLp, Is.GreaterThan(650f), "S2: the sea opens");
            var a = BulwarkScore.Evaluate(918.5, 0.0, 2.5); var c = BulwarkScore.Evaluate(920.0, 0.0, 2.5);
            Assert.That(Db(c.sea) - Db(a.sea), Is.LessThan(-15.0), "S4: the portal drops the world");
            var h = BulwarkScore.Evaluate(1080.0, 0.0, 2.5);
            foreach (var g in new[] { h.w, h.sea, h.falls, h.bed, h.tonal, h.conduit, h.voice }) Assert.That(Db(g + 1e-12), Is.LessThan(-50.0), "S5 hush");
            Assert.That(Db(h.pad), Is.GreaterThan(-50.0), "S5 keeps P");
            Assert.That(Db(BulwarkScore.Evaluate(1427.0, 0.0, 2.5).conduit) - Db(BulwarkScore.Evaluate(1420.0, 0.0, 2.5).conduit), Is.LessThan(-3.0), "S7");
            Assert.That(BulwarkScore.ListenerIndoors(60.0) && !BulwarkScore.ListenerIndoors(300.0) && BulwarkScore.ListenerIndoors(1000.0) && !BulwarkScore.ListenerIndoors(1450.0));
        }

        /// <summary>LEAD DECISION 3: every KrakenCallKind is synthesised at its recipe level (strength 1 at the reference
        /// distance, +-2.5 dB), keeps the largest share of its energy in the 80-800 Hz growl band (at least 30 %), keeps the
        /// infrasound under -30 dBFS RMS in any 3 s window, never clips (sample peak under -1 dBFS) and is NaN-free.</summary>
        [Test]
        public void Kraken_EveryKindIsCalibratedAudibleAndClean()
        {
            for (int k = 0; k < KrakenRecipe.KindCount; k++)
            {
                var r = BwAudioExamples.RenderKraken(k, null, false, Seed, 1.0, true);
                string name = KrakenRecipe.Names[k];
                Assert.That(r.NanResets, Is.EqualTo(0), name);
                Assert.That(r.MaxRms3Db, Is.EqualTo(r.LevelTargetDb).Within(2.5), name + " level");
                Assert.That(r.PeakDb, Is.LessThan(BwAudioSpec.TruePeakMaxDb), name + " peak");
                Assert.That(r.MaxStep, Is.LessThan(0.25), name + " step");
                var x = r.Buffer;
                var hp = new SvfTpt(); hp.Set(80, 0.7071, Fs); var lp = new SvfTpt(); lp.Set(800, 0.7071, Fs);
                var s1 = new SvfTpt(); s1.Set(20, 0.7071, Fs); var s2 = new SvfTpt(); s2.Set(20, 0.7071, Fs);
                double tot = 0, band = 0, win = 0, subMax = 0; int w = 0;
                for (int i = 0; i < x.Length / 2; i++)
                {
                    float m = 0.5f * (x[2 * i] + x[2 * i + 1]);
                    tot += m * m;
                    float bb = lp.Low(hp.High(m)); band += bb * bb;
                    float sb = s2.Low(s1.Low(m)); win += sb * sb;
                    if (++w == 3 * Fs) { subMax = Math.Max(subMax, win / w); win = 0; w = 0; }
                }
                double bandDb = 10.0 * Math.Log10(band / tot), subDb = 10.0 * Math.Log10(subMax + 1e-30);
                UnityEngine.Debug.Log($"[BW] kraken {name}: 3 s RMS {r.MaxRms3Db:F1} (target {r.LevelTargetDb:F1}), peak {r.PeakDb:F1}, 80-800 Hz {bandDb:F1} dB of the energy, <20 Hz max {subDb:F1} dBFS");
                Assert.That(bandDb, Is.GreaterThan(-5.3), name + ": 80-800 Hz share");
                Assert.That(subDb, Is.LessThan(-30.0), name + ": infrasound");
            }
        }

        /// <summary>2.6: a call ducks the beds by 6-10 dB (the duck releases afterwards), and the bus is allocation-free.</summary>
        [Test]
        public void Kraken_DucksTheBedsAndIsAllocationFree()
        {
            for (int k = 0; k < KrakenRecipe.KindCount; k++)
            {
                var r = BwAudioExamples.RenderKraken(k, null, true, Seed);
                Assert.That(-r.MinDuckDb, Is.InRange(5.9, 10.1), KrakenRecipe.Names[k]);
            }
            var dsp = new BwDsp(Fs, Seed);
            var p = BulwarkScore.Evaluate(350.0, 0.0, 2.5);
            var buf = new float[1024 * 2];
            dsp.TriggerKraken(new KrakenEvent { kind = KrakenRecipe.Call, strength = 1f, duration = 8f, x = 500f, y = -8f, z = 600f });
            dsp.Process(buf, 2, in p);
            Assert.That(() =>
            {
                dsp.TriggerKraken(new KrakenEvent { kind = KrakenRecipe.SuckerPops, strength = 1f, duration = 6f, underwater = true, x = 20f, y = 0f, z = 350f });
                for (int i = 0; i < 8; i++) dsp.Process(buf, 2, in p);
            }, Is.Not.AllocatingGCMemory());
            Assert.That(dsp.Kraken.ActiveCount, Is.EqualTo(2));
        }

        /// <summary>2.2: a source to the right is louder on the right, and the pan follows the listener yaw.</summary>
        [Test]
        public void Kraken_IsSpatialisedAtThePosition()
        {
            double Side(double yaw, double ex, double ez)
            {
                var dsp = new BwDsp(Fs, Seed);
                var p = BwAudioParams.Silent(); p.master = 1f; p.impactBus = 1f; p.lyaw = (float)yaw;
                dsp.SnapTo(in p);
                dsp.TriggerKraken(new KrakenEvent { kind = KrakenRecipe.Call, strength = 1f, duration = 6f, x = (float)ex, y = 0f, z = (float)ez });
                var buf = new float[1024 * 2];
                double l = 0, r = 0;
                for (int k = 0; k < 47 * 5; k++) { dsp.Process(buf, 2, in p); for (int i = 0; i < 1024; i++) { l += buf[2 * i] * (double)buf[2 * i]; r += buf[2 * i + 1] * (double)buf[2 * i + 1]; } }
                return 10.0 * Math.Log10(r / l);
            }
            Assert.That(Side(0.0, 300.0, 0.0), Is.GreaterThan(6.0), "east of a north-facing listener: right");
            Assert.That(Side(180.0, 300.0, 0.0), Is.LessThan(-6.0), "facing south the same source is on the left");
        }

        /// <summary>KrakenCalls -> KrakenEvent: T0 ahead is a delay; slaps and roar outdoors add d / 343 s; under water not; a late
        /// call is shortened and dropped under 1 s.</summary>
        [Test]
        public void Kraken_CallConversion()
        {
            var listener = new UnityEngine.Vector3(0f, 0f, 0f);
            var call = new KrakenCall { Kind = KrakenCallKind.Slap, Position = new UnityEngine.Vector3(343f, 0f, 0f), Strength = 1f, T0 = 10.0, Duration = 4f };
            Assert.That(AudioDirector.ToEvent(in call, listener, 9.5, out var e), Is.True);
            Assert.That(e.delay, Is.EqualTo(1.5f).Within(0.01f), "0.5 s ahead + 1.0 s of air");
            call.Underwater = true;
            AudioDirector.ToEvent(in call, listener, 9.5, out e);
            Assert.That(e.delay, Is.EqualTo(0.5f).Within(0.01f), "no air delay under water");
            var voice = new KrakenCall { Kind = KrakenCallKind.Call, Position = new UnityEngine.Vector3(600f, 0f, 0f), Strength = 0.5f, T0 = 10.0, Duration = 8f };
            AudioDirector.ToEvent(in voice, listener, 12.0, out e);
            Assert.That(e.delay, Is.EqualTo(0f)); Assert.That(e.duration, Is.EqualTo(6f).Within(0.01f), "late: the rest of the call");
            Assert.That(AudioDirector.ToEvent(in voice, listener, 17.5, out _), Is.False, "under 1 s left: dropped");
            Assert.That(KrakenRecipe.LevelDb(0, 1200.0, 1.0, false), Is.EqualTo(KrakenRecipe.RefDb[0] - 3.01).Within(0.05), "-3 dB per doubling");
            Assert.That(KrakenRecipe.AirLowpassHz(1464.0), Is.EqualTo(743.0).Within(2.0), "2.2 air low-pass");
        }

        /// <summary>LEAD DECISION 1 ride: the descent cruise at -33.5 +-1.5 dBFS (2.8), the platform quiet at -46 +-2.5 (the
        /// silence), a clean render (peak under -1 dBFS, steps under 0.25) and the five cues of the phase changes.</summary>
        [Test]
        public void Ride_LevelsCuesAndSilence()
        {
            Assert.That(BwAudioExamples.RideSteadyRmsDb(2, 110f, 0f, 3000f, 0, 6.0), Is.EqualTo(-33.5).Within(1.5), "descent cruise");
            var r = BwAudioExamples.RenderRide(null, Seed, true);
            Assert.That(r.NanResets, Is.EqualTo(0));
            Assert.That(r.PeakDb, Is.LessThan(BwAudioSpec.TruePeakMaxDb));
            Assert.That(r.MaxStep, Is.LessThan(0.25));
            // the platform: the last 8 s of the render (12 s after the arrival)
            var x = r.Buffer;
            int n = x.Length / 2, from = n - 8 * Fs;
            double e = 0; for (int i = from; i < n; i++) e += 0.5 * (x[2 * i] * (double)x[2 * i] + x[2 * i + 1] * (double)x[2 * i + 1]);
            double platform = 10.0 * Math.Log10(e / (n - from));
            UnityEngine.Debug.Log($"[BW] ride: max 3 s RMS {r.MaxRms3Db:F1} dBFS, platform {platform:F1} dBFS, peak {r.PeakDb:F1}");
            Assert.That(platform, Is.EqualTo(-46.0).Within(2.5), "the platform keeps only the foot ambience");
            int cues = 0, prev = 0;
            for (double t = -1.0; t < BulwarkScore.SyntheticRideArrival + 5.0; t += 0.05)
            {
                var s = BulwarkScore.SyntheticRide(t);
                if (RideCues.ForTransition(prev, s.Phase) >= 0) cues++;
                prev = s.Phase;
            }
            Assert.That(cues, Is.EqualTo(5), "doors, motor, coupling, motor, doors");
            Assert.That(BulwarkScore.Evaluate(1500.0, 0.0, 0.0, BulwarkScore.SyntheticRide(70.0)).earHz, Is.LessThan(8000f), "the ear closes low in the descent");
            Assert.That(BulwarkScore.Evaluate(1500.0, 0.0, 0.0, BulwarkScore.SyntheticRide(85.0)).earHz, Is.GreaterThan(18000f), "and pops open at the foot");
        }

        /// <summary>Fix wave S2-1 (small speakers): every outdoor kind (and the strike) keeps at least 1/8 of its energy above
        /// 250 Hz (4th-order high-pass) in the dry render at its reference distance: a laptop plays it.</summary>
        [Test]
        public void Kraken_OutdoorKindsReachSmallSpeakers()
        {
            int[] kinds = { KrakenRecipe.Call, KrakenRecipe.SurfaceRoar, KrakenRecipe.Slap, KrakenRecipe.DamShudder, KrakenRecipe.Stinger, KrakenRecipe.Strike };
            foreach (int k in kinds)
            {
                var r = BwAudioExamples.RenderKraken(k, null, false, Seed, 1.0, true);
                var x = r.Buffer;
                var h1 = new SvfTpt(); h1.Set(250, 0.7071, Fs); var h2 = new SvfTpt(); h2.Set(250, 0.7071, Fs);
                var h3 = new SvfTpt(); h3.Set(250, 0.7071, Fs); var h4 = new SvfTpt(); h4.Set(250, 0.7071, Fs);
                double tot = 0, hi = 0, hiR = 0;
                for (int i = 0; i < x.Length / 2; i++)
                {
                    float l = x[2 * i], rr = x[2 * i + 1];
                    tot += 0.5 * (l * (double)l + rr * (double)rr);
                    float a = h2.High(h1.High(l)), b = h4.High(h3.High(rr));
                    hi += 0.5 * (a * (double)a + b * (double)b);
                }
                hiR = 10.0 * Math.Log10(hi / tot);
                UnityEngine.Debug.Log($"[BW] kraken {KrakenRecipe.Names[k]}: {hiR:F1} dB of the energy above 250 Hz");
                Assert.That(hiR, Is.GreaterThan(-9.0), KrakenRecipe.Names[k] + ": energy above 250 Hz");
            }
        }

        /// <summary>Fix wave S3-4: a DamShudder published while the listener is in the window gallery is the strike (kind 8:
        /// glass ring and crack, no wall low-pass); elsewhere it stays the dam shudder.</summary>
        [Test]
        public void Kraken_StrikeInTheWindowGallery()
        {
            var listener = new UnityEngine.Vector3(0f, 0f, 0f);
            var c = new KrakenCall { Kind = KrakenCallKind.DamShudder, Position = new UnityEngine.Vector3(30f, 0f, 0f), Strength = 1f, T0 = 10.0, Duration = 4f };
            AudioDirector.ToEvent(in c, listener, 10.0, out var e, 1300.0);
            Assert.That(e.kind, Is.EqualTo(KrakenRecipe.Strike), "s 1300: the strike");
            AudioDirector.ToEvent(in c, listener, 10.0, out e, 400.0);
            Assert.That(e.kind, Is.EqualTo(KrakenRecipe.DamShudder), "s 400: the dam shudder");
            AudioDirector.ToEvent(in c, listener, 10.0, out e);
            Assert.That(e.kind, Is.EqualTo(KrakenRecipe.DamShudder), "no listener s: unchanged");
            Assert.That(KrakenRecipe.AirLowpassHz(7331.0, KrakenRecipe.Stinger), Is.EqualTo(KrakenRecipe.StingerAirFloorHz), "S2-2: the stinger is not dulled to 250 Hz");
            Assert.That(KrakenRecipe.LevelDb(KrakenRecipe.Stinger, 7331.0, 1.0, false), Is.InRange(-32.0, -29.0), "S2-2: the stinger on the platform");
        }

        /// <summary>Fix wave S4-5: while paused the kraken bus fades out and its voices stand still (a 6 s call survives a 10 s
        /// pause and sounds again on resume).</summary>
        [Test]
        public void Kraken_FreezesWhilePaused()
        {
            var dsp = new BwDsp(Fs, Seed);
            var p = BwAudioParams.Silent(); p.master = 1f; p.impactBus = 1f;
            dsp.SnapTo(in p);
            dsp.TriggerKraken(new KrakenEvent { kind = KrakenRecipe.Call, strength = 1f, duration = 6f, x = 300f, y = 0f, z = 300f });
            var buf = new float[1024 * 2];
            double Run(int blocks, in BwAudioParams q)
            {
                double e = 0; int n = 0;
                for (int b = 0; b < blocks; b++) { dsp.Process(buf, 2, in q); for (int i = 0; i < buf.Length; i++) { e += buf[i] * (double)buf[i]; n++; } }
                return 10.0 * Math.Log10(Math.Max(1e-30, e / n));
            }
            Run(94, in p);                        // 2 s of the call
            var q = p; q.paused = 1f;
            Run(47, in q);                        // the 0.3 s fade
            double paused = Run(470, in q);       // 10 s paused
            Assert.That(paused, Is.LessThan(-100.0), "silent while paused");
            Assert.That(dsp.Kraken.ActiveCount, Is.EqualTo(1), "the voice waits");
            double resumed = Run(94, in p);
            Assert.That(resumed, Is.GreaterThan(-45.0), "and sounds again on resume");
        }

        /// <summary>R1 / fix wave S3-3: the ride state of the QA and offline renders is the player's EndingPath profile (the
        /// shuttle lasts BulwarkConst.RideShuttleTime), and S4-6: the A1 2.8 sunset hush drops the motor 6 dB between 5467 and
        /// 3421 m.</summary>
        [Test]
        public void Ride_FollowsTheEndingPathAndHushes()
        {
            double shuttle = 0.0;
            for (double t = 0.0; t < BulwarkScore.SyntheticRideArrival + 5.0; t += 0.01)
                if (BulwarkScore.SyntheticRide(t).Phase == (int)EndingRidePhase.Shuttle) shuttle += 0.01;
            Assert.That(shuttle, Is.EqualTo(BulwarkConst.RideShuttleTime).Within(0.05), "the shuttle phase lasts the ride constant");
            Assert.That(BulwarkScore.SyntheticRideArrival, Is.EqualTo(EndingPath.TauShuttle1).Within(1e-9));
            Assert.That(RideSound.HushWeight(4500.0), Is.EqualTo(1.0).Within(1e-9));
            Assert.That(RideSound.HushWeight(5910.0), Is.EqualTo(0.0).Within(1e-9));
            Assert.That(RideSound.HushWeight(2000.0), Is.EqualTo(0.0).Within(1e-9));
            double motorIn = BwAudioExamples.RideSteadyRmsDb(2, 110f, 0f, 4500f, 127 & ~1, 4.0);
            double motorOut = BwAudioExamples.RideSteadyRmsDb(2, 110f, 0f, 3000f, 127 & ~1, 4.0);
            Assert.That(motorIn - motorOut, Is.EqualTo(-RideSound.HushDb).Within(0.5), "the motor hushes");
            double airIn = BwAudioExamples.RideSteadyRmsDb(2, 110f, 0f, 4500f, 127 & ~4, 4.0);
            double airOut = BwAudioExamples.RideSteadyRmsDb(2, 110f, 0f, 3000f, 127 & ~4, 4.0);
            Assert.That(airIn - airOut, Is.EqualTo(0.0).Within(0.5), "the air rush does not");
        }

        /// <summary>Fix wave S3-3: the offline renderer feeds the show's call table (K2 voice at s 301, its rise at s 306 with the
        /// air delay) into the kraken bus and lists the events for audio_analysis.py.</summary>
        [Test]
        public void Offline_FeedsTheShowCalls()
        {
            var st = new OfflineRenderSettings { S0 = 295.0, S1 = 312.0, Speed = 2.5, Ending = false, Seed = Seed, Kraken = true, TailSeconds = 0.0 };
            var info = OfflineAudioRenderer.Render(st);
            Assert.That(info.NanResets, Is.EqualTo(0));
            Assert.That(info.KrakenKind.Count, Is.EqualTo(2), "K2_voice, K2_rise");
            Assert.That(info.KrakenKind[0], Is.EqualTo(KrakenRecipe.Call));
            Assert.That(info.KrakenKind[1], Is.EqualTo(KrakenRecipe.SurfaceRoar));
            Assert.That(info.KrakenT[1] - info.KrakenT[0], Is.GreaterThan(2.0), "5 m of s later plus the air delay");
        }

        /// <summary>The offline impact lattice is the twin of SeaState's (same times, strengths and membership).</summary>
        [Test]
        public void ImpactLattice_MatchesSeaState()
        {
            for (long n = -3; n < 40; n++)
            {
                SeaState.LatticeImpact(SeaState.LatticeCrest, 0xC0A57u, n, out double t0, out double st, out double mem, out _, out _);
                ImpactLattice.Impact(ImpactLattice.Crest, 0xC0A57u, n, out double a0, out double ast, out double amem);
                Assert.That(a0, Is.EqualTo(t0)); Assert.That(ast, Is.EqualTo(st)); Assert.That(amem, Is.EqualTo(mem));
            }
            foreach (double s in new[] { 0.0, 690.0, 700.0, 800.0, 930.0, 1000.0 })
                Assert.That(ImpactLattice.ReefWeight(s), Is.EqualTo(SeaState.ReefWeight(s)).Within(1e-12));
        }
    }
}
