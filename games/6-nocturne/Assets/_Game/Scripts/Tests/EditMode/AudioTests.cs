using System;
using System.IO;
using NUnit.Framework;

namespace Megastructure.Tests
{
    /// <summary>
    /// DSP block tests (the pattern of Thread's AudioTests, IMPORT_PLAN section 8): Simper SVF stability up to 20 kHz, FDN decay,
    /// DC blocker, WAV writer round trip, The Ring's look-ahead limiter ceiling (-1.5 dBFS, 14.3 / 14.12), and the offline render of
    /// the generator chain at 1.5 and 2.5 m/s (peak below the ceiling, no DC, deterministic).
    /// </summary>
    public sealed class AudioTests
    {
        const int Fs = 48000;

        [Test]
        public void Svf_StableUpTo20kHzWithModulation()
        {
            var svf = new SvfTpt();
            var rng = new XorShift32(7u);
            double peak = 0;
            for (int i = 0; i < Fs * 2; i++)
            {
                double fc = 20.0 + 19980.0 * (0.5 + 0.5 * Math.Sin(i * 0.001));
                svf.Set(fc, 0.7, Fs);
                float y = svf.Low(rng.NextBipolar());
                Assert.IsFalse(float.IsNaN(y) || float.IsInfinity(y), "SVF diverged at sample " + i);
                peak = Math.Max(peak, Math.Abs(y));
            }
            Assert.Less(peak, 4.0);
            svf.Set(20000.0, 0.7, Fs);
            for (int i = 0; i < Fs; i++) Assert.IsFalse(float.IsNaN(svf.High(1f)), "20 kHz high-pass");
        }

        [Test]
        public void Fdn_ImpulseDecays()
        {
            var fdn = new FdnReverb8(Fs, AudioSpec.ReverbSize, AudioSpec.ReverbMaxPreDelay);
            fdn.SetTargets(1.5, 20.0, -10.0, 6000.0);
            fdn.Snap();
            double early = 0, late = 0;
            for (int i = 0; i < Fs * 6; i++)
            {
                fdn.Tick(i == 0 ? 1f : 0f, i == 0 ? 1f : 0f, out float l, out float r);
                double e = l * l + r * r;
                if (i < Fs) early += e; else if (i >= 5 * Fs) late += e;
                Assert.IsFalse(float.IsNaN(l));
            }
            Assert.Greater(early, 0.0, "the reverb answers");
            Assert.Less(late, early * 1e-4, "RT60 1.5 s: the tail after 5 s is 60 dB+ down");
        }

        [Test]
        public void DcBlocker_RemovesOffset()
        {
            var dc = new DcBlocker();
            dc.Init(NocturneConstants.DcBlockHz, Fs);
            float y = 0;
            for (int i = 0; i < Fs; i++) y = dc.Process(0.5f);
            Assert.Less(Math.Abs(y), 1e-4f);
        }

        [Test]
        public void Wav_RoundTrip()
        {
            string path = Path.Combine(Path.GetTempPath(), "noc_wav_" + Guid.NewGuid().ToString("N") + ".wav");
            try
            {
                var x = new float[2 * 1000];
                for (int i = 0; i < x.Length; i++) x[i] = (float)Math.Sin(i * 0.01) * 0.5f;
                WavWriter.WriteFloat32(path, x, x.Length, 2, Fs);
                var y = WavWriter.Read(path, out int ch, out int rate);
                Assert.AreEqual(2, ch); Assert.AreEqual(Fs, rate); Assert.AreEqual(x.Length, y.Length);
                for (int i = 0; i < x.Length; i++) Assert.AreEqual(x[i], y[i], 1e-7f);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Test]
        public void Limiter_CeilingMinus1_5dBFS()
        {
            var lim = new DspLimiter();
            lim.Configure(Fs, -6.0, NocturneConstants.LimiterCeilingDb, NocturneConstants.LimiterLookAheadMs, NocturneConstants.LimiterReleaseMs);
            double ceiling = Math.Pow(10.0, NocturneConstants.LimiterCeilingDb / 20.0);
            Assert.AreEqual(NocturneConstants.LimiterCeiling, ceiling, 1e-4);
            var rng = new XorShift32(3u);
            double peak = 0;
            for (int i = 0; i < Fs * 2; i++)
            {
                double g = i < Fs ? 4.0 : 0.3;   // +12 dBFS bursts, then quiet
                double l = rng.NextBipolar() * g, r = rng.NextBipolar() * g;
                lim.Process(ref l, ref r);
                peak = Math.Max(peak, Math.Max(Math.Abs(l), Math.Abs(r)));
            }
            Assert.LessOrEqual(peak, ceiling + 1e-9, "sample peak never above -1.5 dBFS");
            Assert.AreEqual((int)Math.Round(NocturneConstants.LimiterLookAheadMs * 0.001 * Fs) - 1, lim.LatencySamples);
        }

        [Test]
        public void OfflineRender_PeakBelowCeilingNoDcAtBothSpeeds()
        {
            foreach (double speed in new[] { NocturneConstants.WalkSpeed, NocturneConstants.CinematicSpeed })
            {
                var m = new MeterSink();
                var st = new OfflineRenderSettings { S0 = 100.0, S1 = 100.0 + 10.0 * speed, Speed = speed, Seed = 2 };
                long frames = OfflineAudioRenderer.Render(st, m);
                Assert.AreEqual(10 * Fs, frames, 2, "10 s rendered");
                Assert.Greater(m.Rms, 1e-6, "the bed and the footsteps are audible");
                Assert.LessOrEqual(m.PeakDbfs, NocturneConstants.LimiterCeilingDb + 1e-6, "peak below -1.5 dBFS (14.3)");
                Assert.Less(Math.Abs(m.Dc), 1e-3, "no DC offset (11.9)");
            }
        }

        [Test]
        public void OfflineRender_IsDeterministic()
        {
            var a = new MeterSink(); var b = new MeterSink();
            OfflineAudioRenderer.Render(new OfflineRenderSettings { S0 = 0, S1 = 10, Seed = 5 }, a);
            OfflineAudioRenderer.Render(new OfflineRenderSettings { S0 = 0, S1 = 10, Seed = 5 }, b);
            Assert.AreEqual(a.SumSq, b.SumSq, 0.0);
            Assert.AreEqual(a.Peak, b.Peak, 0.0);
        }
    
        /// <summary>-audio-dump comparison: a fixed-length render standing at s (speed 0) has no footsteps and the asked length;
        /// RmsSink attributes blocks to the beat of their s (14.3 per-beat RMS).</summary>
        [Test]
        public void OfflineRender_StandingWindowAndPerBeatRms()
        {
            var st = new OfflineRenderSettings { S0 = 130.0, Speed = 0.0, Seconds = 2.0, Footsteps = true };
            Assert.AreEqual(2.0, OfflineAudioRenderer.Duration(st), 1e-12);
            var rms = new RmsSink();
            long frames = OfflineAudioRenderer.Render(st, rms);
            Assert.AreEqual((long)(2.0 * st.SampleRate), frames);
            Assert.Greater(rms.BeatSamples[1], 0, "s 130 is beat 2");
            for (int b = 0; b < NocturneConstants.BeatCount; b++) if (b != 1) Assert.AreEqual(0, rms.BeatSamples[b]);
            Assert.Less(rms.Peak, Math.Pow(10.0, -1.0 / 20.0), "peak below -1 dBFS");
            Assert.IsFalse(double.IsNaN(rms.BeatDb(1)));
        }
    }
}
