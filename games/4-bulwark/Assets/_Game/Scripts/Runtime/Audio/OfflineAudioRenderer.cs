// OfflineAudioRenderer: renders the whole walk offline at 48 kHz through the SAME engine as the player (BwDsp, footsteps
// mixed inside the DSP), for T-AUDIO (MASTER_PROMPT 14.3, 11.9) and the -audio-dump comparison (within 1 dB). Imported
// from Thread's OfflineAudioRenderer (AudioBlockInfo, IAudioBlockSink, settings / info, the motion model, the meters);
// adapted: Render(speed) at 1.5 and 2.5 m/s, the goal trigger at 1498.5 with the StopRamp to s 1500, the 14.11 ending
// (3.0 s dwell, 4.0 s smootherstep fade, 18.5 s in all) on the master and impact-bus envelopes, Bulwark beat boundaries, and
// optionally the 14.11 intro (Intro: bed with the picture fade, impact bus open from t 0, the wave impact at 0.5 s in the black).
// The motion model is the walker's: a 0.5 s smootherstep start ramp from rest, then the constant speed (Part 9).
// Wave X: the wave-impact booms of the player (SeaState lattices on the ambient clock) are rendered too (Impacts, default on)
// through ImpactLattice, the audio twin of SeaState.LatticeImpact / LatticeMember (same salts, jitter, strengths and Rule-P
// membership), so the per-beat levels of a render include the B layer of 14.3 like the player does.
// Fix wave X2 (S3-3): Kraken (off by default: the T-AUDIO bed tests measure the score alone) feeds the kraken show's real call table (KrakenDirectorChoreo.Calls, keyed to s)
// into the same KrakenBus as the player (listener pose = the walker's eye and heading at s, the air delay d / 343 s for slaps
// and water roar, a DamShudder in beat 6 = the strike) and lists the events in the sidecar ("events": dump time, kind), so
// audio_analysis.py gates them like a player dump. The window gallery's own calls (Beat06Layout, another assembly) are not
// fed: the player dump of beat 6 covers them.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Megastructure.Player;
using Megastructure.UI;

namespace Megastructure.Audio
{
    public struct AudioBlockInfo
    {
        public double t0, t1;          // seconds since the render started
        public double s0, s1;          // progress at the block start and end
        public double endingTime;      // seconds since the goal trigger at the block start (negative before it)
        public long frame0;            // index of the first frame
    }

    public interface IAudioBlockSink
    {
        void Begin(int sampleRate, int channels);
        /// <summary>interleaved stereo; frames valid frames.</summary>
        void Write(float[] interleaved, int frames, in AudioBlockInfo info);
        void End();
    }

    public sealed class OfflineRenderSettings
    {
        public double S0 = 0.0;
        public double S1 = BwPath.Length;
        public double Speed = FrameworkConst.CinematicSpeed;
        public int SampleRate = BwAudioSpec.SampleRate;
        public int Seed = 1;
        public bool Footsteps = true;
        /// <summary>Ambient time at the render start (the intro clock), seconds.</summary>
        public double IntroTime = 0.0;
        /// <summary>Run the 14.11 ending after the goal trigger (master fade, then silence).</summary>
        public bool Ending = true;
        /// <summary>Play the 14.11 intro from t 0 = LoadComplete (a fresh start, like the -audio-dump of the player): the bed follows
        /// UiTimeline.Picture (black 0-1.0 s, in by 3.5 s), the impact bus UiTimeline.ImpactBus (open from t 0), and the first wave
        /// impact sounds at 0.5 s, in the black.</summary>
        public bool Intro = false;
        /// <summary>Start ramp from rest (Part 9: 0.5 s); 0 = constant speed from t 0.</summary>
        public double StartRamp = FrameworkConst.StartRamp;
        public double TailSeconds = 0.5;
        public int BlockFrames = 1024;
        public bool SnapAtStart = true;
        /// <summary>Render the SeaState wave-impact booms (ImpactLattice) on the impact bus, as the player does.</summary>
        public bool Impacts = true;
        /// <summary>Feed the kraken show's call table (KrakenDirectorChoreo.Calls) into the kraken bus (fix wave S3-3).</summary>
        public bool Kraken = false;

        public OfflineRenderSettings Clone() { return (OfflineRenderSettings)MemberwiseClone(); }
    }

    public sealed class OfflineRenderInfo
    {
        public int SampleRate, Channels = 2;
        public long Frames;
        public double Seconds;
        public double TriggerTime = -1.0;
        /// <summary>Seconds of the intro impact's block start (-1 when the render has no intro).</summary>
        public double IntroImpactTime = -1.0;
        public int Steps;
        public int Impacts;
        public int NanResets;
        public readonly List<double> SPerSecond = new List<double>();
        /// <summary>Kraken events of the render: start time (s, the air delay included) and KrakenRecipe kind.</summary>
        public readonly List<double> KrakenT = new List<double>();
        public readonly List<int> KrakenKind = new List<int>();
    }

    public static class OfflineAudioRenderer
    {
        public const int Channels = 2;

        /// <summary>Progress at time t: ramp, constant speed, then the 14.11 stop ramp (closed forms, frame-rate independent).</summary>
        public static double MotionS(OfflineRenderSettings st, double t, out double endingTime, out bool moving)
        {
            double v = Math.Max(1e-6, st.Speed);
            double ramp = Math.Max(0.0, st.StartRamp);
            double trig = FrameworkConst.GoalTriggerS;
            endingTime = -1.0;
            double s = st.S0 + (ramp > 0 ? WalkPlanner.RampDistance(0.0, v, t) : v * t);
            bool useEnding = st.Ending && st.S1 >= trig && st.S0 < trig;
            if (!useEnding) { moving = s < st.S1; return Math.Min(s, st.S1); }
            double tTrig = TriggerTime(st);
            if (t < tTrig) { moving = true; return s; }
            endingTime = t - tTrig;
            var stop = StopRamp.Begin(trig, v);
            moving = !stop.Done(endingTime);
            return stop.PositionAt(endingTime);
        }

        /// <summary>Time of the goal trigger for the settings (ramp loss included: 0.25 s for a 0.5 s ramp from rest).</summary>
        public static double TriggerTime(OfflineRenderSettings st)
        {
            double v = Math.Max(1e-6, st.Speed);
            double d = FrameworkConst.GoalTriggerS - st.S0;
            return st.StartRamp > 0 && d > 0 ? d / v + 0.5 * st.StartRamp : d / v;
        }

        /// <summary>Total render length in seconds for the settings.</summary>
        public static double Duration(OfflineRenderSettings st)
        {
            double trig = FrameworkConst.GoalTriggerS;
            if (st.Ending && st.S1 >= trig && st.S0 < trig)
                return TriggerTime(st) + FrameworkConst.EndTotal + st.TailSeconds;
            double v = Math.Max(1e-6, st.Speed);
            return Math.Max(0.0, st.S1 - st.S0) / v + 0.5 * st.StartRamp + st.TailSeconds;
        }

        /// <summary>The 14.11 audio envelope of the ending: 1 during the 3 s dwell, smootherstep to 0 over 4 s, then 0.</summary>
        public static float EndingEnvelope(double endingTime)
        {
            if (endingTime < 0) return 1f;
            return (float)(1.0 - BwMath.Smootherstep(FrameworkConst.EndDwell, FrameworkConst.EndDwell + FrameworkConst.EndFade, endingTime));
        }

        public static OfflineRenderInfo Render(OfflineRenderSettings st, params IAudioBlockSink[] sinks)
        {
            int fs = st.SampleRate > 0 ? st.SampleRate : BwAudioSpec.SampleRate;
            int B = Math.Max(16, st.BlockFrames);
            var info = new OfflineRenderInfo { SampleRate = fs };
            var dsp = new BwDsp(fs, st.Seed);
            float[] buf = new float[B * Channels];
            double dur = Duration(st);
            long totalFrames = (long)Math.Ceiling(dur * fs);
            var cadence = FootstepCadence.Create();

            for (int i = 0; i < sinks.Length; i++) sinks[i].Begin(fs, Channels);
            double nextSecond = 0.0;
            bool first = true;
            for (long f = 0; f < totalFrames; f += B)
            {
                int n = (int)Math.Min(B, totalFrames - f);
                double t0 = f / (double)fs, t1 = (f + n) / (double)fs;
                double s0 = MotionS(st, t0, out double end0, out _);
                double s1 = MotionS(st, t1, out _, out _);
                while (nextSecond <= t1 && nextSecond <= dur)
                {
                    info.SPerSecond.Add(MotionS(st, nextSecond, out _, out _));
                    nextSecond += 1.0;
                }
                if (end0 >= 0.0 && info.TriggerTime < 0.0) info.TriggerTime = t0 - end0;

                double speed = n > 0 ? (s1 - s0) / ((double)n / fs) : 0.0;
                var p = BulwarkScore.Evaluate(s0, st.IntroTime + t0, speed);
                float end = EndingEnvelope(end0);
                p.master = end * (st.Intro ? UiTimeline.Picture((float)t0) : 1f);
                p.impactBus = end * (st.Intro ? UiTimeline.ImpactBus((float)t0) : 1f);
                p.duck = 1f;
                if (first) { if (st.SnapAtStart) dsp.SnapTo(in p); first = false; }
                if (st.Intro && info.IntroImpactTime < 0.0 && t1 > UiTimeline.IntroImpact)
                {
                    dsp.TriggerImpact(BulwarkScore.IntroImpactStrength, BulwarkScore.IntroImpactPan, BulwarkScore.ImpactSubOnly(s0));
                    info.IntroImpactTime = t0;
                }

                if (st.Impacts)
                {
                    double ta = st.IntroTime + t0, tb = st.IntroTime + t1;
                    info.Impacts += ImpactLattice.Scan(ta, tb, s0, dsp);
                }

                // the listener pose for the kraken spatialiser (the walker's eye, facing along the path)
                BwPath.EyeWorld(Math.Min(s0, BwPath.Length), 0.0, out double ex, out double ey, out double ez);
                p.lx = (float)ex; p.ly = (float)ey; p.lz = (float)ez; p.lyaw = (float)BwPath.HeadingDeg(Math.Min(s0, BwPath.Length));
                p.indoors = BulwarkScore.ListenerIndoors(s0) ? 1f : 0f;
                if (st.Kraken) FeedKraken(dsp, info, s0, s1, t0, ex, ey, ez, f == 0);

                if (st.Footsteps && s1 > s0 && cadence.Advance(s1 - s0, s1, speed))
                {
                    dsp.TriggerStep(PlayerMath.Surface(s1), cadence.Pan, PlayerMath.FootstepGain(speed));
                    info.Steps++;
                }

                if (buf.Length != n * Channels) buf = new float[n * Channels];   // only the last block can be shorter
                dsp.Process(buf, Channels, in p);
                var bi = new AudioBlockInfo { t0 = t0, t1 = t1, s0 = s0, s1 = s1, endingTime = end0, frame0 = f };
                for (int i = 0; i < sinks.Length; i++) sinks[i].Write(buf, n, in bi);
                info.Frames += n;
            }
            for (int i = 0; i < sinks.Length; i++) sinks[i].End();
            info.Seconds = info.Frames / (double)fs;
            info.NanResets = dsp.NanResets;
            return info;
        }

        /// <summary>Starts the show's calls whose s lies in (s0, s1] (the first block: [s0, s1]) as the player's AudioDirector
        /// would (KrakenCall -> ToEvent at the listener's eye; T0 = now).</summary>
        static void FeedKraken(BwDsp dsp, OfflineRenderInfo info, double s0, double s1, double t0, double ex, double ey, double ez, bool first)
        {
            var calls = Megastructure.Creature.KrakenDirectorChoreo.Calls;
            var eye = new UnityEngine.Vector3((float)ex, (float)ey, (float)ez);
            for (int i = 0; i < calls.Length; i++)
            {
                double cs = calls[i].S;
                if (!(cs <= s1 && (cs > s0 || (first && cs >= s0)))) continue;
                var q = calls[i];
                var c = new KrakenCall { Kind = q.Kind, Position = q.Position, Strength = q.Strength, T0 = t0, Duration = q.Duration, Underwater = q.Underwater };
                if (!KrakenCallEvents.ToEvent(in c, eye, t0, out KrakenEvent e, cs)) continue;
                dsp.TriggerKraken(in e);
                info.KrakenT.Add(t0 + e.delay);
                info.KrakenKind.Add(e.kind);
            }
        }

        /// <summary>Renders to a WAV file (PCM16 or float32) plus a JSON sidecar next to it; extra sinks see the same blocks.</summary>
        public static OfflineRenderInfo RenderToWav(OfflineRenderSettings st, string wavPath, WavFormat format, params IAudioBlockSink[] extra)
        {
            using (var w = new WavStreamWriter(wavPath, Channels, st.SampleRate, format, st.Seed))
            {
                var sinks = new IAudioBlockSink[1 + (extra != null ? extra.Length : 0)];
                sinks[0] = w;
                if (extra != null) Array.Copy(extra, 0, sinks, 1, extra.Length);
                var info = Render(st, sinks);
                WriteSidecar(Path.ChangeExtension(wavPath, ".json"), st, info, wavPath);
                return info;
            }
        }

        /// <summary>T-AUDIO renders at both speeds (1.5 and 2.5 m/s, 14.3: the score assumes no speed).</summary>
        public static string RenderBothSpeeds(string dir, int seed, WavFormat format)
        {
            Directory.CreateDirectory(dir);
            var sb = new StringBuilder();
            foreach (double v in new[] { FrameworkConst.WalkSpeed, FrameworkConst.CinematicSpeed })
            {
                var st = new OfflineRenderSettings { Speed = v, Seed = seed };
                string name = "walk_" + v.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', 'p') + ".wav";
                var meter = new WalkLevelMeter();
                var info = RenderToWav(st, Path.Combine(dir, name), format, meter);
                sb.Append(name).Append(": ").Append(info.Seconds.ToString("0.0", CultureInfo.InvariantCulture)).Append(" s, trigger ")
                  .Append(info.TriggerTime.ToString("0.00", CultureInfo.InvariantCulture)).Append(" s, peak ")
                  .Append(BwAudioSpec.ToDb(meter.Peak).ToString("0.00", CultureInfo.InvariantCulture)).Append(" dBFS\n");
            }
            return sb.ToString();
        }

        public static void WriteSidecar(string path, OfflineRenderSettings st, OfflineRenderInfo info, string wavPath)
        {
            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"wav\": \"").Append(Path.GetFileName(wavPath)).Append("\",\n");
            sb.Append("  \"sampleRate\": ").Append(info.SampleRate).Append(",\n");
            sb.Append("  \"seconds\": ").Append(info.Seconds.ToString("0.000", inv)).Append(",\n");
            sb.Append("  \"speed\": ").Append(st.Speed.ToString("0.000", inv)).Append(",\n");
            sb.Append("  \"s0\": ").Append(st.S0.ToString("0.000", inv)).Append(", \"s1\": ").Append(st.S1.ToString("0.000", inv)).Append(",\n");
            sb.Append("  \"seed\": ").Append(st.Seed).Append(",\n");
            sb.Append("  \"triggerTime\": ").Append(info.TriggerTime.ToString("0.000", inv)).Append(",\n");
            sb.Append("  \"steps\": ").Append(info.Steps).Append(",\n");
            sb.Append("  \"nanResets\": ").Append(info.NanResets).Append(",\n");
            sb.Append("  \"events\": [");
            for (int i = 0; i < info.KrakenT.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append("{\"what\": \"kraken\", \"kind\": ").Append(info.KrakenKind[i]).Append(", \"t\": ").Append(info.KrakenT[i].ToString("0.000", inv)).Append('}');
            }
            sb.Append("],\n");
            sb.Append("  \"sPerSecond\": [");
            for (int i = 0; i < info.SPerSecond.Count; i++) { if (i > 0) sb.Append(", "); sb.Append(info.SPerSecond[i].ToString("0.###", inv)); }
            sb.Append("]\n}\n");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }
    }

    /// <summary>
    /// Audio twin of SeaState's impact lattices (Runtime/Systems/SeaState.cs: LatticeImpact, LatticeMember, ReefWeight): two
    /// jittered lattices on the ambient clock, 9 s (crest, salt 0xC0A57) and 6 s (reef, salt 0x4EEF5, beat 4 with the Rule-P
    /// window 680-720 and back at 920-950), jitter +-0.35 of the interval, strength 0.3-1.0. Kept equal to SeaState by
    /// AudioTests.ImpactLattice_MatchesSeaState. Pan 0.4 (the sea face on the right, ahead of the walker).
    /// </summary>
    public static class ImpactLattice
    {
        public const double Crest = 9.0, Reef = 6.0, Jitter = 0.35, Pan = 0.4;
        const uint SaltA = 0xC0A57u, SaltB = 0x4EEF5u;

        public static double ReefWeight(double s) => BwMath.Smootherstep(680.0, 720.0, s) * (1.0 - BwMath.Smootherstep(920.0, 950.0, s));

        public static void Impact(double interval, uint salt, long n, out double t0, out double strength, out double member)
        {
            uint h0 = BwMath.Pcg((uint)n ^ salt), h1 = BwMath.Pcg(h0 + 0x9E37u), h2 = BwMath.Pcg(h1 + 0x7F4Au);
            double u0 = (h0 >> 8) * (1.0 / 16777216.0), u1 = (h1 >> 8) * (1.0 / 16777216.0), u2 = (h2 >> 8) * (1.0 / 16777216.0);
            t0 = interval * (n + Jitter * (2.0 * u0 - 1.0));
            strength = BulwarkConst.ImpactStrengthMin + (BulwarkConst.ImpactStrengthMax - BulwarkConst.ImpactStrengthMin) * u1 * (0.55 + 0.45 * u1);
            member = u2;
        }

        /// <summary>Triggers every impact with t0 in (ta, tb] at progress s on the DSP; returns the count.</summary>
        public static int Scan(double ta, double tb, double s, BwDsp dsp)
        {
            int count = 0;
            double w = ReefWeight(s);
            for (int lat = 0; lat < 2; lat++)
            {
                double interval = lat == 0 ? Crest : Reef;
                uint salt = lat == 0 ? SaltA : SaltB;
                long n0 = (long)Math.Floor(ta / interval) - 1, n1 = (long)Math.Ceiling(tb / interval) + 1;
                for (long n = n0; n <= n1; n++)
                {
                    Impact(interval, salt, n, out double t0, out double strength, out double member);
                    if (t0 <= ta || t0 > tb) continue;
                    bool isMember = lat == 1 ? member < w : member >= w;
                    if (!isMember) continue;
                    dsp.TriggerImpact((float)strength, (float)Pan, BulwarkScore.ImpactSubOnly(s));
                    count++;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// Level meter of a walk (T-AUDIO): per-beat RMS, sliding 3 s RMS per second, sample peak, DC per channel and the largest
    /// sample-to-sample step. True peak (4x oversampled) is measured by Tools/qa/audio_analysis.py on the WAV.
    /// </summary>
    public sealed class WalkLevelMeter : IAudioBlockSink
    {
        public readonly double[] BeatSumSq = new double[FrameworkConst.BeatCount];
        public readonly long[] BeatFrames = new long[FrameworkConst.BeatCount];
        public readonly double[] Sum = new double[2];
        public double Peak, MaxStep, MaxStepS, SumSqAll;
        public long Frames;
        public int NonFinite;
        public readonly List<double> Rms3sPerSecondDb = new List<double>();
        double[] m_win;
        int m_winPos, m_winLen, m_fs;
        double m_winSum;
        long m_nextSecond;
        float m_prevL, m_prevR;

        public void Begin(int sampleRate, int channels)
        {
            m_fs = sampleRate;
            m_win = new double[Math.Max(1, 3 * sampleRate)];
            m_winPos = 0; m_winLen = 0; m_winSum = 0; m_nextSecond = sampleRate;
        }

        public void Write(float[] x, int frames, in AudioBlockInfo info)
        {
            int beat = Math.Max(1, Math.Min(FrameworkConst.BeatCount, BwPath.BeatAt(info.s0))) - 1;
            for (int i = 0; i < frames; i++)
            {
                float l = x[2 * i], r = x[2 * i + 1];
                if (float.IsNaN(l) || float.IsNaN(r) || float.IsInfinity(l) || float.IsInfinity(r)) { NonFinite++; continue; }
                double e = 0.5 * ((double)l * l + (double)r * r);
                SumSqAll += 2.0 * e;
                if (info.endingTime < 0) { BeatSumSq[beat] += 2.0 * e; BeatFrames[beat]++; }
                Sum[0] += l; Sum[1] += r;
                float a = Math.Max(Math.Abs(l), Math.Abs(r));
                if (a > Peak) Peak = a;
                float st = Math.Max(Math.Abs(l - m_prevL), Math.Abs(r - m_prevR));
                if (st > MaxStep) { MaxStep = st; MaxStepS = info.s0; }
                m_prevL = l; m_prevR = r;
                m_winSum += e - m_win[m_winPos];
                m_win[m_winPos] = e;
                if (++m_winPos == m_win.Length) m_winPos = 0;
                if (m_winLen < m_win.Length) m_winLen++;
                Frames++;
                if (Frames >= m_nextSecond)
                {
                    Rms3sPerSecondDb.Add(10.0 * Math.Log10(Math.Max(1e-30, m_winSum / Math.Max(1, m_winLen))));
                    m_nextSecond += m_fs;
                }
            }
        }

        public void End() { }

        public double BeatRmsDb(int b) => BeatFrames[b] > 0 ? 10.0 * Math.Log10(Math.Max(1e-30, BeatSumSq[b] / (2.0 * BeatFrames[b]))) : double.NegativeInfinity;
        public double RmsDb => Frames > 0 ? 10.0 * Math.Log10(Math.Max(1e-30, SumSqAll / (2.0 * Frames))) : double.NegativeInfinity;
        public double Dc(int ch) => Frames > 0 ? Sum[ch] / Frames : 0.0;
    }
}
