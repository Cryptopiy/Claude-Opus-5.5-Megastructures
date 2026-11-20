// AudioAnalysis: the numbers of the 14.3 / 14.7 H8 / 14.10 audio gates, computed on a render as it streams (MASTER_PROMPT 14.3
// "Sum is ... the target of the median of the beat's 3 s RMS windows outside the swells S3, S4, S6 and S7 (logged separately),
// +-1.5 dB"; "true peak <= -1 dBFS, DC < 0.001, sample step <= 0.25"; 14.7 H8 "no click at chord changes (step < 0.05 in
// 20 ms)"). WalkAnalysis meters 100 ms bins (mean-over-channels mean square of the mix and of every stem, s at the bin centre,
// sample peak, ITU true peak, DC, the largest sample step), then: per beat the median of the 3 s windows (30 contiguous bins)
// outside the excluded S-windows, the S-window levels, and per chord change the largest sample step and the largest change of
// the 20 ms RMS envelope inside the change window. The same rules as Tools/qa/audio_analysis.py (100 ms blocks assigned by s,
// runs of usable blocks cut into 3 s windows). Used by AudioScoreTests and Tools/audio/harness. Pure C#.
using System;
using System.Collections.Generic;

namespace Megastructure
{
    public sealed class WalkAnalysis : IAudioBlockSink, IAudioStemSink
    {
        public const double BinSeconds = 0.1, WindowSeconds = 3.0, EnvSeconds = 0.02;
        public readonly List<double> BinS = new List<double>(), BinMs = new List<double>(), BinPeak = new List<double>();
        public readonly List<double[]> BinStems = new List<double[]>();
        public double SamplePeak, Dc, MaxStep, MaxStepAt;
        public long Frames, NonFinite;
        public readonly TruePeakMeter TruePeak = new TruePeakMeter();
        // chord-change click metering (14.7 H8): max sample step and max 20 ms RMS-envelope step per change window
        public readonly double[] ChordMaxStep = new double[NocturneScore.ChordChangeS.Length];
        public readonly double[] ChordMaxEnvStep = new double[NocturneScore.ChordChangeS.Length];
        /// <summary>The same step relative to the envelope (|e_k - e_k-1| / e_k-1): the level-independent click measure of a stem.</summary>
        public readonly double[] ChordMaxEnvRel = new double[NocturneScore.ChordChangeS.Length];
        int m_fs;
        double m_binSs, m_binPeak; long m_binFrames, m_binLen;
        double m_binS0;
        double m_pl, m_pr; bool m_have;
        double m_sum;
        double m_envSs; long m_envFrames, m_envLen; double m_lastEnv = -1; int m_envChord = -1;
        double[] m_pendingStems;

        public void Begin(int sampleRate, int channels)
        {
            m_fs = sampleRate; m_binLen = (long)Math.Round(BinSeconds * sampleRate); m_envLen = (long)Math.Round(EnvSeconds * sampleRate);
            TruePeak.Reset();
        }

        static int ChordWindowAt(double s)
        {
            for (int c = 1; c < NocturneScore.ChordChangeS.Length; c++)
            {
                double h = NocturneScore.ChordHalfWindow[c];
                if (s >= NocturneScore.ChordChangeS[c] - h && s <= NocturneScore.ChordChangeS[c] + h) return c;
            }
            return -1;
        }

        public void Write(float[] x, int frames, in AudioBlockInfo info)
        {
            double ds = frames > 0 ? (info.s1 - info.s0) / frames : 0.0;
            for (int i = 0; i < frames; i++)
            {
                double l = x[2 * i], r = x[2 * i + 1];
                if (double.IsNaN(l) || double.IsNaN(r) || double.IsInfinity(l) || double.IsInfinity(r)) { NonFinite++; continue; }
                double s = info.s0 + ds * i;
                if (m_binFrames == 0) m_binS0 = s;
                double ms = 0.5 * (l * l + r * r);
                m_binSs += ms; m_binFrames++;
                double a = Math.Max(Math.Abs(l), Math.Abs(r));
                if (a > m_binPeak) m_binPeak = a;
                if (a > SamplePeak) SamplePeak = a;
                m_sum += 0.5 * (l + r);
                TruePeak.Push(l, r);
                double st = 0;
                if (m_have) { st = Math.Max(Math.Abs(l - m_pl), Math.Abs(r - m_pr)); if (st > MaxStep) { MaxStep = st; MaxStepAt = (Frames + i) / (double)m_fs; } }
                m_pl = l; m_pr = r; m_have = true;
                // chord windows: sample steps and the 20 ms envelope
                int c = ChordWindowAt(s);
                if (c >= 0 && st > ChordMaxStep[c]) ChordMaxStep[c] = st;
                if (c != m_envChord) { m_envChord = c; m_lastEnv = -1; m_envSs = 0; m_envFrames = 0; }
                if (c >= 0)
                {
                    m_envSs += ms; m_envFrames++;
                    if (m_envFrames >= m_envLen)
                    {
                        double env = Math.Sqrt(m_envSs / m_envFrames);
                        if (m_lastEnv >= 0)
                        {
                            ChordMaxEnvStep[c] = Math.Max(ChordMaxEnvStep[c], Math.Abs(env - m_lastEnv));
                            if (m_lastEnv > 1e-7) ChordMaxEnvRel[c] = Math.Max(ChordMaxEnvRel[c], Math.Abs(env - m_lastEnv) / m_lastEnv);
                        }
                        m_lastEnv = env; m_envSs = 0; m_envFrames = 0;
                    }
                }
                if (m_binFrames >= m_binLen) FlushBin(s);
            }
            Frames += frames;
        }

        void FlushBin(double sEnd)
        {
            BinS.Add(0.5 * (m_binS0 + sEnd));
            BinMs.Add(m_binSs / Math.Max(1, m_binFrames));
            BinPeak.Add(m_binPeak);
            if (m_pendingStems != null) { BinStems.Add(m_pendingStems); m_pendingStems = null; }
            m_binSs = 0; m_binFrames = 0; m_binPeak = 0;
        }

        /// <summary>Stems of a render block (the harness renders with BlockFrames = one 100 ms bin, so the blocks are the bins).</summary>
        public void Stems(double[] sumSq, int frames, in AudioBlockInfo info)
        {
            var a = new double[sumSq.Length];
            for (int i = 0; i < a.Length; i++) a[i] = sumSq[i] / Math.Max(1, frames);
            if (BinStems.Count < BinMs.Count) BinStems.Add(a); else m_pendingStems = a;
        }

        public void End() { Dc = Frames > 0 ? m_sum / Frames : 0.0; }

        public static double Db(double ms) => 10.0 * Math.Log10(Math.Max(ms, 1e-24));

        /// <summary>Median (dB) of the 3 s windows of beat b outside the excluded S-windows, the window count; NaN when none.</summary>
        public double BeatMedianDb(int b, out int windows) => BeatMedianDb(b, BinMs, out windows);

        /// <summary>The same median for a stem (index NocturneScore.R .. F).</summary>
        public double StemMedianDb(int b, int stem, out int windows)
        {
            var v = new List<double>(BinStems.Count);
            for (int i = 0; i < BinStems.Count; i++) v.Add(BinStems[i][stem]);
            return BeatMedianDb(b, v, out windows);
        }

        double BeatMedianDb(int b, List<double> ms, out int windows)
        {
            int win = (int)Math.Round(WindowSeconds / BinSeconds);
            var w = new List<double>();
            int run = 0; double acc = 0;
            int n = Math.Min(ms.Count, BinS.Count);
            for (int i = 0; i < n; i++)
            {
                double s = BinS[i];
                bool use = NocMath.BeatIndex(s) == b && !NocturneScore.InGateExclusion(s) && s < NocturneConstants.GoalS - 0.01;
                if (!use) { run = 0; acc = 0; continue; }
                acc += ms[i]; run++;
                if (run == win) { w.Add(acc / win); run = 0; acc = 0; }
            }
            windows = w.Count;
            if (w.Count == 0) return double.NaN;
            w.Sort();
            double med = w.Count % 2 == 1 ? w[w.Count / 2] : 0.5 * (w[w.Count / 2 - 1] + w[w.Count / 2]);
            return Db(med);
        }

        /// <summary>Mean level (dB) of the bins inside [s0, s1] (S-window logging) and their max bin peak (dBFS).</summary>
        public double RangeDb(double s0, double s1, out double peakDb)
        {
            double acc = 0, pk = 0; int k = 0;
            for (int i = 0; i < BinS.Count; i++) if (BinS[i] >= s0 && BinS[i] <= s1) { acc += BinMs[i]; pk = Math.Max(pk, BinPeak[i]); k++; }
            peakDb = 20.0 * Math.Log10(Math.Max(pk, 1e-12));
            return k > 0 ? Db(acc / k) : double.NaN;
        }

        /// <summary>Stem level (dB) over [s0, s1].</summary>
        public double StemRangeDb(int stem, double s0, double s1)
        {
            double acc = 0; int k = 0;
            for (int i = 0; i < BinStems.Count && i < BinS.Count; i++) if (BinS[i] >= s0 && BinS[i] <= s1) { acc += BinStems[i][stem]; k++; }
            return k > 0 ? Db(acc / k) : double.NaN;
        }
    }
    /// <summary>Click meter (14.7 H8 "no click"): the largest |second difference| x[n] - 2 x[n-1] + x[n-2] (a broadband, high-pass
    /// measure a discontinuity of value or slope cannot hide from) inside an s-range and outside it. A low-passed pad has a tiny
    /// second difference; a click at a chord change would raise the inside value far above the steady outside value.</summary>
    public sealed class ClickMeter : IAudioBlockSink
    {
        public double InS0, InS1;
        public double MaxInside, MaxOutside;
        double m_l1, m_l2, m_r1, m_r2; int m_have;
        public void Begin(int sampleRate, int channels) { MaxInside = 0; MaxOutside = 0; m_have = 0; }
        public void Write(float[] x, int frames, in AudioBlockInfo info)
        {
            double ds = frames > 0 ? (info.s1 - info.s0) / frames : 0.0;
            for (int i = 0; i < frames; i++)
            {
                double l = x[2 * i], r = x[2 * i + 1];
                if (m_have >= 2)
                {
                    double d = Math.Max(Math.Abs(l - 2.0 * m_l1 + m_l2), Math.Abs(r - 2.0 * m_r1 + m_r2));
                    double s = info.s0 + ds * i;
                    if (s >= InS0 && s <= InS1) { if (d > MaxInside) MaxInside = d; }
                    else if (d > MaxOutside) MaxOutside = d;
                }
                m_l2 = m_l1; m_l1 = l; m_r2 = m_r1; m_r1 = r; if (m_have < 2) m_have++;
            }
        }
        public void End() { }
    }

    /// <summary>A scripted main thread for the player-path renders: progress s and the published speed at time t (s).</summary>
    public interface ISnapshotScript
    {
        void At(double t, out double s, out double speed);
    }

    /// <summary>Constant walk from S0 at V (the -audio-dump proxy of AudioScoreTests).</summary>
    public sealed class ConstantWalkScript : ISnapshotScript
    {
        public double S0, V;
        public void At(double t, out double s, out double speed) { s = S0 + V * Math.Max(0.0, t); speed = t > 0.0 ? V : 0.0; }
    }

    /// <summary>Piecewise-constant walk from S0: segment k runs from T[k] at velocity V[k] (negative = walking back; the published
    /// speed is max(0, v), as AudioDirector publishes it). The continuity test of 14.7 H8 ("continuous at any speed").</summary>
    public sealed class PiecewiseWalkScript : ISnapshotScript
    {
        public double S0;
        public double[] T, V;
        public void At(double t, out double s, out double speed)
        {
            s = S0; speed = 0.0;
            for (int k = 0; k < T.Length; k++)
            {
                double a = T[k], e = k + 1 < T.Length ? T[k + 1] : double.PositiveInfinity;
                if (t <= a) break;
                s += V[k] * (Math.Min(t, e) - a);
                if (t <= e) { speed = V[k]; break; }
            }
        }
    }

    /// <summary>The renders behind the 14.3 / 14.7 H8 gates, shared by AudioScoreTests and Tools/audio/harness ("gates"), so the
    /// EditMode numbers and the harness numbers are the same code: one steady stretch per beat (outside the S-windows, the 24 m
    /// blends and the chord windows) for the level gate, and the player path (NocMixer driven by main-thread snapshots at a frame
    /// rate, callbacks of a device size, the DSP's own extrapolation) for the -audio-dump comparison.</summary>
    public static class AudioGates
    {
        /// <summary>Start s of the level-gate stretch of each beat (24 s at the render speed from here stays inside the beat).</summary>
        public static readonly double[] GateS0 = { 20.0, 200.0, 430.0, 600.0, 930.0, 1130.0, 1395.0 };
        public const double GateSeconds = 24.0, GatePrerollSeconds = 4.0;
        /// <summary>Level tolerance of 14.3 (dB), the sample-step limit (footsteps and the tram bell excepted: 0.25 for the mix),
        /// the click limit of 14.7 H8 (20 ms RMS envelope step), the true-peak and DC limits.</summary>
        public const double LevelTolDb = 1.5, StepLimit = 0.25, ChordEnvStepLimit = 0.05, TruePeakLimitDb = -1.0, DcLimit = 0.001;

        /// <summary>Renders [s0, s0 + seconds x speed] (pre-roll standing at s0) into a WalkAnalysis.</summary>
        public static WalkAnalysis RenderStretch(double s0, double seconds, double speed, int seed = 1, double preroll = GatePrerollSeconds, bool stems = false, int soloMask = -1, bool footsteps = true)
        {
            var st = new OfflineRenderSettings
            {
                S0 = s0, S1 = s0 + seconds * speed, Speed = speed, Seed = seed, PrerollSeconds = preroll, MeterStems = stems,
                BlockFrames = stems ? 4800 : 1024, SoloMask = soloMask, Footsteps = footsteps,
            };
            if (speed <= 0.0) st.Seconds = seconds;
            var an = new WalkAnalysis();
            OfflineAudioRenderer.Render(st, an);
            return an;
        }

        /// <summary>Pad solo through chord change c at a speed: 20 m steady before the window, the window, 4 m after (footsteps off).
        /// Returns the click meter (inside = the change window) and fills the WalkAnalysis (sample and 20 ms envelope steps).</summary>
        public static ClickMeter PadChange(int c, double speed, out WalkAnalysis an, int seed = 1)
        {
            double h = NocturneScore.ChordHalfWindow[c], sc = NocturneScore.ChordChangeS[c];
            double s0 = sc - h - 20.0, s1 = Math.Min(NocturneConstants.GoalS, sc + h + 4.0);
            var st = new OfflineRenderSettings
            {
                S0 = s0, S1 = s1, Speed = speed, Seed = seed, PrerollSeconds = 2.0, SoloMask = 1 << NocturneScore.P, Footsteps = false,
            };
            var cm = new ClickMeter { InS0 = sc - h, InS1 = sc + h };
            an = new WalkAnalysis();
            OfflineAudioRenderer.Render(st, an, cm);
            return cm;
        }

        /// <summary>The level-gate stretch of beat b at a speed: the median of its 3 s windows and its window count.</summary>
        public static WalkAnalysis BeatStretch(int b, double speed, int seed = 1) => RenderStretch(GateS0[b], GateSeconds, speed, seed);

        /// <summary>Player path: a NocMixer fed by snapshots published at `fps` (s, speed, listener, yaw, rain, scene time), rendered
        /// in callbacks of `callbackFrames`; `preroll` seconds standing at the script's s(0) are discarded. No footsteps.</summary>
        public static long RenderPlayerPath(int fs, int seed, double seconds, int callbackFrames, double fps, ISnapshotScript script, double preroll, params IAudioBlockSink[] sinks)
        {
            var mixer = new NocMixer(fs, seed);
            var buf = new float[callbackFrames * OfflineAudioRenderer.Channels];
            var p = new NocAudioParams { master = 1f };
            double frameDt = 1.0 / Math.Max(1.0, fps), nextFrame = -preroll;
            long pre = (long)Math.Round(preroll * fs), total = (long)Math.Round(seconds * fs), n = 0;
            foreach (var k in sinks) k.Begin(fs, OfflineAudioRenderer.Channels);
            while (n < pre + total)
            {
                double tNow = (n - pre) / (double)fs;
                // the main thread has published every frame up to now (the last one wins)
                while (nextFrame <= tNow)
                {
                    script.At(nextFrame, out double s, out double v);
                    double sc = NocMath.Clamp(s, 0.0, NocturneConstants.GoalS);
                    NocLanes.Path.PointAt(sc, out p.x, out p.z);
                    GazeSchedule.Evaluate(sc, out p.yaw, out _);
                    p.s = Math.Max(0.0, s); p.speed = Math.Max(0.0, v); p.t = nextFrame + preroll;
                    p.rain = (float)NocturneScore.RainRhoSchedule(p.s);
                    p.seq++;
                    nextFrame += frameDt;
                }
                mixer.Process(buf, OfflineAudioRenderer.Channels, in p);
                long f0 = n - pre;
                n += callbackFrames;
                if (f0 + callbackFrames <= 0) continue;
                // the written part of this callback (a callback straddling the pre-roll end is written from its first frame after it)
                int skip = f0 < 0 ? (int)(-f0) : 0;
                int frames = (int)Math.Min(callbackFrames - skip, total - Math.Max(0, f0));
                if (frames <= 0) break;
                float[] w = buf;
                if (skip > 0) { w = new float[frames * OfflineAudioRenderer.Channels]; Array.Copy(buf, skip * OfflineAudioRenderer.Channels, w, 0, w.Length); }
                double t0 = Math.Max(0, f0) / (double)fs, t1 = t0 + frames / (double)fs;
                script.At(t0, out double s0, out _); script.At(t1, out double s1, out _);
                var info = new AudioBlockInfo { t0 = t0, t1 = t1, s0 = s0, s1 = s1, endingTime = -1, frame0 = Math.Max(0, f0) };
                foreach (var k in sinks) k.Write(w, frames, in info);
            }
            foreach (var k in sinks) k.End();
            return total;
        }
    }
}
