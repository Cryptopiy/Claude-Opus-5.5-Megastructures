// OfflineAudioRenderer: renders the score (NocMixer, the same DSP OnAudioFilterRead runs) without an audio device (MASTER_PROMPT
// 11.9; IMPORT_PLAN 8.9; 14.7 H8 "NocturneScore.Render(s0, s1, speed) renders offline to WAV"), the block loop and sink interface
// of Thread's OfflineAudioRenderer with the walk speed as a parameter (Part 3.1 (c): the score is rendered at 1.5 AND 2.5 m/s).
// The motion is exact (WalkMotion: s = S0 + v t on the 14.4 path, the listener on the centre line, the cinematic gaze schedule
// for the yaw, the -rain override or the 14.2.4 schedule, optionally the stop ramp and the ending's audio fade of 14.11), the
// footsteps are scheduled by distance inside the DSP, so the render does not depend on the block size. A pre-roll (speed 0
// at S0, discarded) fills the reverbs before the first written sample. Output goes to IAudioBlockSink objects (WAV stream,
// meters, the 3 s window analysis), so a 1 000 s render never sits in memory. Pure C#, no UnityEngine.
using System;

namespace Megastructure
{
    public struct AudioBlockInfo
    {
        public double t0, t1;          // seconds since the render started (after the pre-roll)
        public double s0, s1;          // progress at the block start and end
        public double endingTime;      // seconds since the goal trigger at the block start (negative before it)
        public long frame0;            // index of the first frame
    }

    public interface IAudioBlockSink
    {
        void Begin(int sampleRate, int channels);
        /// <summary>Interleaved stereo; frames valid frames.</summary>
        void Write(float[] interleaved, int frames, in AudioBlockInfo info);
        void End();
    }

    /// <summary>Receives the per-layer sums of squares of each block (NocMixer stems; index NocturneScore.R .. F).</summary>
    public interface IAudioStemSink
    {
        void Stems(double[] sumSq, int frames, in AudioBlockInfo info);
    }

    public sealed class OfflineRenderSettings
    {
        public int SampleRate = (int)NocturneConstants.AudioSampleRate;
        public int Seed = 1;
        public int BlockFrames = 1024;
        public double S0 = 0.0, S1 = NocturneConstants.GoalS;
        /// <summary>Walk speed of the render (1.5 or 2.5 m/s).</summary>
        public double Speed = NocturneConstants.CinematicSpeed;
        /// <summary>Footsteps every PlayerMath.Stride(speed) metres (the controller's rule), scheduled inside the DSP.</summary>
        public bool Footsteps = true;
        /// <summary>Rain density override 0..1 (the player's -rain X, D-020); NaN = the 14.2.4 schedule (NocRainState.RhoAt).</summary>
        public double RainOverride = double.NaN;
        /// <summary>Fixed duration in seconds (&gt; 0: overrides (S1 - S0) / Speed; with Speed 0 the render stands at S0, as the
        /// -audio-dump comparison does while the player stands still).</summary>
        public double Seconds = 0.0;
        /// <summary>Seconds rendered and discarded before the first written sample (standing at S0): fills the reverbs.</summary>
        public double PrerollSeconds = 0.0;
        /// <summary>Walk to the goal and play the ending: the stop ramp to s 1 500 (T = 6 / v0, 14.11) and the 6 s audio fade,
        /// EndTotal (17 s) after the goal trigger.</summary>
        public bool Ending = false;
        /// <summary>Meter the stems (IAudioStemSink gets the per-layer sums of squares of each block).</summary>
        public bool MeterStems = false;
        /// <summary>Solo mask over the stems (NocMixer.SoloMask: bit NocturneScore.R .. F; -1 = the whole mix).</summary>
        public int SoloMask = -1;
    }

    /// <summary>Exact motion of an offline render (constant speed, optional stop ramp and ending fade).</summary>
    public sealed class WalkMotion : IAudioMotion
    {
        public double S0, V0, RainOverride = double.NaN;
        public long PrerollFrames;
        public bool Ending;
        double m_goalT = double.NaN;

        /// <summary>Seconds of the walk (after the pre-roll) at which s reaches the goal trigger (s 1 497); NaN when never.</summary>
        public double GoalTime => V0 > 1e-9 && S0 < NocturneScore.GoalTriggerS ? (NocturneScore.GoalTriggerS - S0) / V0 : (S0 >= NocturneScore.GoalTriggerS ? 0.0 : double.NaN);

        /// <summary>Stop ramp of 14.11: v = v0 (1 - smoothstep(0, T, t)), T = 6 / v0, so it stops 3 m after the trigger (s 1 500).</summary>
        public double SAt(double t)
        {
            if (t <= 0.0) return S0;
            if (!Ending || double.IsNaN(GoalTime) || V0 <= 1e-9) return S0 + V0 * t;
            double tg = GoalTime;
            if (t <= tg) return S0 + V0 * t;
            double T = 6.0 / V0, x = Math.Min(t - tg, T) / T;
            // integral of v0 (1 - (3 x^2 - 2 x^3)) dt over [0, x T] = v0 T (x - x^3 + x^4 / 2)
            return NocturneScore.GoalTriggerS + V0 * T * (x - x * x * x + 0.5 * x * x * x * x);
        }

        public void At(long n, double fs, out double s, out double v, out double x, out double z, out double yaw, out double sceneT)
        {
            double t = (n - PrerollFrames) / fs;
            s = SAt(t);
            double h = 1.0 / fs;
            v = t <= 0.0 ? 0.0 : (SAt(t + h) - s) / h;
            NocLanes.Path.PointAt(NocMath.Clamp(s, 0.0, NocturneConstants.GoalS), out x, out z);
            GazeSchedule.Evaluate(NocMath.Clamp(s, 0.0, NocturneConstants.GoalS), out yaw, out _);
            sceneT = n / fs;
        }

        public double Rho(double s) => double.IsNaN(RainOverride) ? NocturneScore.RainRhoSchedule(s) : RainOverride;

        /// <summary>The ending's audio gain (UiTimeline.EndAudio): 1 until 1.5 s after the trigger, smootherstep to 0 at 7.5 s.</summary>
        public double Master(long n, double fs)
        {
            if (!Ending || double.IsNaN(GoalTime)) return 1.0;
            double te = (n - PrerollFrames) / fs - GoalTime;
            return 1.0 - NocMath.Smootherstep(NocturneConstants.EndPictureHold, NocturneConstants.EndPictureHold + NocturneConstants.EndAudioFade, te);
        }
    }

    /// <summary>RMS per window (block resolution) and per beat of s (audio_analysis-like numbers inside the player).</summary>
    public sealed class RmsSink : IAudioBlockSink
    {
        public readonly double[] BeatSumSq = new double[NocturneConstants.BeatCount];
        public readonly long[] BeatSamples = new long[NocturneConstants.BeatCount];
        public double SumSq, Peak;
        public long Samples;
        public double Rms => Samples > 0 ? Math.Sqrt(SumSq / Samples) : 0.0;
        public static double Db(double rms) => 20.0 * Math.Log10(Math.Max(rms, 1e-12));
        public double BeatDb(int b) => BeatSamples[b] > 0 ? Db(Math.Sqrt(BeatSumSq[b] / BeatSamples[b])) : double.NaN;
        public void Begin(int sampleRate, int channels) { }
        public void Write(float[] x, int frames, in AudioBlockInfo info)
        {
            int n = frames * OfflineAudioRenderer.Channels;
            int b = NocMath.BeatIndex(0.5 * (info.s0 + info.s1));
            double ss = 0;
            for (int i = 0; i < n; i++) { double v = x[i]; ss += v * v; double a = v < 0 ? -v : v; if (a > Peak) Peak = a; }
            SumSq += ss; Samples += n;
            BeatSumSq[b] += ss; BeatSamples[b] += n;
        }
        public void End() { }
    }

    /// <summary>Peak / RMS / DC meter sink (audio tests).</summary>
    public sealed class MeterSink : IAudioBlockSink
    {
        public double Peak, SumSq, Sum;
        public long Samples;
        public double Rms => Samples > 0 ? Math.Sqrt(SumSq / Samples) : 0.0;
        public double Dc => Samples > 0 ? Sum / Samples : 0.0;
        public double PeakDbfs => 20.0 * Math.Log10(Math.Max(Peak, 1e-12));
        public void Begin(int sampleRate, int channels) { Peak = 0; SumSq = 0; Sum = 0; Samples = 0; }
        public void Write(float[] x, int frames, in AudioBlockInfo info)
        {
            int n = frames * OfflineAudioRenderer.Channels;
            for (int i = 0; i < n; i++)
            {
                double v = x[i];
                double a = v < 0 ? -v : v;
                if (a > Peak) Peak = a;
                SumSq += v * v; Sum += v;
            }
            Samples += n;
        }
        public void End() { }
    }

    public static class OfflineAudioRenderer
    {
        public const int Channels = 2;

        public static double Duration(OfflineRenderSettings st)
        {
            if (st.Seconds > 0) return st.Seconds;
            double d = Math.Max(0.0, st.S1 - st.S0) / Math.Max(0.05, st.Speed);
            if (st.Ending && st.Speed > 0.05 && st.S1 >= NocturneScore.GoalTriggerS)
                d = Math.Max(0.0, NocturneScore.GoalTriggerS - st.S0) / st.Speed + NocturneConstants.EndTotal;
            return d;
        }

        /// <summary>The DSP of a render, built and positioned (motion, footsteps, stems); the pre-roll is not yet run.</summary>
        public static NocMixer CreateMixer(OfflineRenderSettings st, out WalkMotion motion)
        {
            int fs = st.SampleRate > 0 ? st.SampleRate : (int)NocturneConstants.AudioSampleRate;
            var mixer = new NocMixer(fs, st.Seed);
            motion = new WalkMotion { S0 = st.S0, V0 = Math.Max(0.0, st.Speed), RainOverride = st.RainOverride, Ending = st.Ending, PrerollFrames = (long)Math.Round(Math.Max(0.0, st.PrerollSeconds) * fs) };
            mixer.Motion = motion;
            mixer.AutoSteps = st.Footsteps;
            mixer.MeterStems = st.MeterStems;
            mixer.SoloMask = st.SoloMask;
            return mixer;
        }

        /// <summary>Renders [S0, S1] at the given speed into the sinks; returns the frames rendered (pre-roll excluded).</summary>
        public static long Render(OfflineRenderSettings st, params IAudioBlockSink[] sinks)
        {
            int fs = st.SampleRate > 0 ? st.SampleRate : (int)NocturneConstants.AudioSampleRate;
            int B = Math.Max(16, st.BlockFrames);
            var mixer = CreateMixer(st, out WalkMotion motion);
            var buf = new float[B * Channels];
            // pre-roll: standing at S0 (the motion holds s = S0 for t <= 0), discarded
            long pre = motion.PrerollFrames;
            for (long f0 = 0; f0 < pre; f0 += B)
            {
                int n = (int)Math.Min(B, pre - f0);
                if (n < B) { var tail = new float[n * Channels]; mixer.Process(tail, Channels); }
                else mixer.Process(buf, Channels);
            }
            long total = (long)Math.Ceiling(Duration(st) * fs);
            foreach (var k in sinks) k.Begin(fs, Channels);
            double goal = motion.GoalTime;
            for (long f0 = 0; f0 < total; f0 += B)
            {
                int n = (int)Math.Min(B, total - f0);
                double t0 = f0 / (double)fs, t1 = (f0 + n) / (double)fs;
                if (mixer.MeterStems) { Array.Clear(mixer.StemSumSq, 0, mixer.StemSumSq.Length); mixer.StemFrames = 0; }
                if (n < B)
                {
                    var tail = new float[n * Channels];
                    mixer.Process(tail, Channels);
                    Array.Clear(buf, 0, buf.Length);
                    Array.Copy(tail, buf, tail.Length);
                }
                else mixer.Process(buf, Channels);
                var info = new AudioBlockInfo { t0 = t0, t1 = t1, s0 = motion.SAt(t0), s1 = motion.SAt(t1), endingTime = double.IsNaN(goal) ? -1 : t0 - goal, frame0 = f0 };
                foreach (var k in sinks)
                {
                    k.Write(buf, n, in info);
                    if (mixer.MeterStems && k is IAudioStemSink ss) ss.Stems(mixer.StemSumSq, n, in info);
                }
            }
            foreach (var k in sinks) k.End();
            return total;
        }
    }
}
