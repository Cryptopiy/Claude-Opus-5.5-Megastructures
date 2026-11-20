// NocMixer: the one DSP of the Nocturne score (MASTER_PROMPT 14.3, 14.7 H8; DEVELOPMENT D-011; docs/research/R5 2 and 7;
// docs/AUDIO.md), called by OnAudioFilterRead (NocSynth, generator on a clip-less AudioSource) and by the EditMode / offline
// renderer (OfflineAudioRenderer) through the same Process. Control runs at absolute sample indices n = 32 k, carried across
// calls, and every event (drips, patter, footsteps, passes, the bell) is a pure function of an event or sample index, so the
// output is identical for any callback size. Progress comes from an IAudioMotion model (offline: exact) or from the main
// thread's snapshot NocAudioParams, extrapolated by its speed for at most 0.25 s (the 50 ms gain ramps hide the small jumps).
//   layers (unit RMS at gain 1; NocturneScore gives the gains as functions of s and t):
//     R RainBed, D DripField, H CityHum, T TrafficWash, W WindBed, P PadEngine (+ its own FDN: 6 s, 40 ms, size 2.5),
//     K KoiTone, X = AlleyBed / CanyonBed + TramPass / EdgeBed / TubeHum + TubeTrain x7 / LeafRain / ScreenHum / LampBuzz,
//     F footsteps (FootstepBank voices)
//   -> sends into the environment FDN (14.3 places, blended over the 14.6 windows) -> sum -> 20 Hz DC blocker -> master gain
//      (fades, pause duck) -> TruePeakLimiter (knee -4.5 dBFS, ceiling -1.5 dBFS, 5 ms look-ahead, 80 ms release, BS.1770
//      true-peak detector) -> output.
// The pad enters over 6 s once s first passes 8 (S1) and the pad, koi, traffic, wind and the X beds release over 8 s from the
// goal trigger (s 1 497; S8: the last seconds are rain, hum and the lamp's buzz). Stems (per-layer sums of squares) are metered
// on demand for the analysis. NaN guard: a non-finite output resets the FDNs and the limiter and plays silence for the block.
// No UnityEngine; allocation-free in Process.
using System;

namespace Megastructure
{
    /// <summary>Control values of one frame (main thread writes, audio thread reads a copy).</summary>
    public struct NocAudioParams
    {
        /// <summary>Master gain 0..1 (UiDirector.FadeAudio x pause duck).</summary>
        public float master;
        /// <summary>Rain density 0..1 at s (NocRainState.RhoAt: the -rain X override, else the 14.2.4 schedule; D-020).</summary>
        public float rain;
        /// <summary>Progress (m) and the scene time (s) of the frame.</summary>
        public double s, t;
        /// <summary>Walk speed (m/s, smoothed ds/dt) for the extrapolation between frames; listener position (m) and yaw (deg).</summary>
        public double speed, x, z, yaw;
        /// <summary>Publication counter (a new snapshot when it changes).</summary>
        public int seq;
        /// <summary>Monotone footstep counter: each increment starts one footstep voice with the gain, pan and surface below.</summary>
        public int footsteps;
        public float footGain, footPan;
        public int footSurface;
    }

    /// <summary>Exact motion for offline renders: progress, speed, listener and yaw at absolute sample n; rain density; master.</summary>
    public interface IAudioMotion
    {
        void At(long n, double fs, out double s, out double v, out double x, out double z, out double yaw, out double sceneT);
        double Rho(double s);
        double Master(long n, double fs);
    }

    public sealed class NocMixer
    {
        public const int Block = 32;
        public const double LimiterThresholdDb = -4.5;
        public const double FootDb = 0.0;                 // the bank's -30 dBFS peaks at gain 1 (14.3)
        public const double SendR = 0.12, SendD = 1.0, SendT = 0.25, SendW = 0.08, SendK = 0.2, SendX = 0.3, SendPass = 0.6, SendF = 1.0;
        /// <summary>Pad output normaliser (after the chorus and the pad FDN: unit RMS at gain 1; measured by the harness "calibrate").</summary>
        public static double PadOutNorm = 0.68;

        public readonly int SampleRate;
        readonly double m_fs, m_cr;
        long m_n;
        int m_inBlock;
        // snapshot
        NocAudioParams m_p;
        long m_nSnap;
        int m_lastSeq = int.MinValue;
        bool m_haveSnap;
        public IAudioMotion Motion;
        /// <summary>Offline: footsteps scheduled by distance inside the DSP (the player sends the controller's stride events).</summary>
        public bool AutoSteps;
        // layers
        readonly RainBed m_rain; readonly DripField m_drips; readonly CityHum m_hum; readonly TrafficWash m_traffic; readonly WindBed m_wind;
        readonly PadEngine m_pad; readonly KoiTone m_koi; readonly LeafRain m_leaf; readonly AlleyBed m_alley; readonly CanyonBed m_canyon;
        readonly EdgeBed m_edge; readonly TubeHum m_tube; readonly ScreenHum m_screen; readonly LampBuzz m_lamp; readonly TramPass m_tram;
        readonly TubeTrain[] m_trains;
        readonly FootstepBank m_bank; readonly FootstepVoices m_steps; StepScheduler m_sched;
        readonly ulong m_stepSeed;
        int m_lastFoot;
        // gains
        GainRamp m_gR, m_gD, m_gH, m_gT, m_gW, m_gP, m_gK, m_gX, m_gF, m_gPass, m_master;
        // reverbs, master
        readonly FdnReverb8 m_env, m_padFdn;
        DcBlockD m_dcL, m_dcR;
        readonly TruePeakLimiter m_lim = new TruePeakLimiter();
        // gates
        long m_entryN = long.MinValue, m_goalN = long.MinValue;
        bool m_seen;
        double m_lastS;
        ScoreFrame m_f;
        int m_flushCount;
        // stems
        public bool MeterStems;
        public readonly double[] StemSumSq = new double[NocturneScore.StemCount];
        public long StemFrames;
        public int NanResets { get; private set; }
        public double LimiterMinGain => m_lim.MinGain;
        public void ResetLimiterMinGain() { m_lim.MinGain = 1.0; }

        public NocMixer(int sampleRate, int seed)
        {
            SampleRate = sampleRate > 0 ? sampleRate : (int)NocturneConstants.AudioSampleRate;
            m_fs = SampleRate; m_cr = m_fs / Block;
            uint sd = unchecked((uint)seed);
            SineTable.Warm();
            m_rain = new RainBed(m_fs, m_cr, sd); m_drips = new DripField(m_fs, m_cr, sd); m_hum = new CityHum(m_fs, m_cr, sd);
            m_traffic = new TrafficWash(m_fs, m_cr, sd); m_wind = new WindBed(m_fs, m_cr, sd); m_pad = new PadEngine(m_fs, m_cr, sd);
            m_koi = new KoiTone(m_fs, m_cr, sd); m_leaf = new LeafRain(m_fs, m_cr, sd); m_alley = new AlleyBed(m_fs, m_cr, sd);
            m_canyon = new CanyonBed(m_fs, m_cr, sd); m_edge = new EdgeBed(m_fs, m_cr, sd); m_tube = new TubeHum(m_fs, m_cr, sd);
            m_screen = new ScreenHum(m_fs, m_cr, sd); m_lamp = new LampBuzz(m_fs, m_cr, sd); m_tram = new TramPass(m_fs, m_cr, sd);
            m_trains = new TubeTrain[NocturneScore.Vehicles.Length - 1];
            for (int i = 0; i < m_trains.Length; i++) m_trains[i] = new TubeTrain(i + 1, m_fs, m_cr, sd);
            m_bank = new FootstepBank(SampleRate, sd); m_steps = new FootstepVoices(m_bank);
            m_stepSeed = DspHash.Mix(sd ^ 0x57E9UL);
            m_gR.Init(0.05, m_cr); m_gD.Init(0.05, m_cr); m_gH.Init(0.05, m_cr); m_gT.Init(0.05, m_cr); m_gW.Init(0.05, m_cr);
            m_gP.Init(0.05, m_cr); m_gK.Init(0.05, m_cr); m_gX.Init(0.05, m_cr); m_gF.Init(0.05, m_cr); m_gPass.Init(0.05, m_cr);
            m_gLeaf.Init(0.05, m_cr); m_gLamp.Init(0.05, m_cr);
            m_master.Init(0.02, m_cr);
            m_env = new FdnReverb8(m_fs, NocturneScore.EnvReverbSize, NocturneScore.EnvReverbMaxPreDelay) { BlendSeconds = NocturneScore.ReverbBlendSeconds };
            NocturneScore.Reverb(0.0, out double rt, out double pre, out double damp, out double wet);
            m_env.SetTargets(rt, pre, wet, damp); m_env.Snap();
            m_padFdn = new FdnReverb8(m_fs, NocturneScore.PadReverbSize, 0.1) { BlendSeconds = NocturneScore.ReverbBlendSeconds };
            m_padFdn.SetTargets(NocturneScore.PadReverbRt60, NocturneScore.PadReverbPreMs, NocturneScore.PadReverbWetDb, NocturneScore.PadReverbDampHz);
            m_padFdn.Snap();
            m_dcL.Init(NocturneConstants.DcBlockHz, m_fs); m_dcR.Init(NocturneConstants.DcBlockHz, m_fs);
            m_lim.Configure(m_fs, LimiterThresholdDb, NocturneConstants.LimiterCeilingDb, NocturneConstants.LimiterLookAheadMs, NocturneConstants.LimiterReleaseMs, true);
            m_f.Fs = m_fs; m_f.InvFs = 1.0 / m_fs; m_f.Block = Block;
        }

        /// <summary>Latency of the limiter look-ahead and the true-peak detector (samples).</summary>
        public int LatencySamples => m_lim.LatencySamples;
        public long SampleIndex => m_n;
        public PadEngine Pad => m_pad;
        public TramPass Tram => m_tram;
        public TubeTrain Train(int i) => m_trains[i];
        public ScoreFrame Frame => m_f;

        /// <summary>Player path: renders interleaved output with this frame's snapshot (any channel count; 0 and 1 carry the mix).</summary>
        public void Process(float[] data, int channels, in NocAudioParams p)
        {
            if (p.seq != m_lastSeq || !m_haveSnap) { m_p = p; m_lastSeq = p.seq; m_nSnap = m_n; m_haveSnap = true; }
            else { m_p.master = p.master; }
            int nf = p.footsteps - m_lastFoot;
            if (!AutoSteps && nf > 0 && m_haveSnap)
            {
                for (int k = 0; k < Math.Min(nf, 2); k++)
                {
                    long idx = m_lastFoot + k + 1;
                    StepScheduler.Variation(idx, m_stepSeed, out double gdb, out double pitch, out _, out int variant);
                    m_steps.Start((FootSurface)Math.Max(0, Math.Min(5, p.footSurface)), variant, NocAudio.Db(gdb) * Math.Max(0.0, p.footGain), p.footPan, pitch, 0);
                }
            }
            m_lastFoot = p.footsteps;
            Process(data, channels);
        }

        /// <summary>Renders interleaved output (the motion model or the last snapshot drives s).</summary>
        public void Process(float[] data, int channels)
        {
            if (channels <= 0) return;
            int frames = data.Length / channels;
            for (int i = 0; i < frames; i++)
            {
                if (m_inBlock == 0) Control();
                double u = m_inBlock / (double)Block;
                Tick(u, out double ol, out double or);
                int o = i * channels;
                data[o] = (float)ol;
                if (channels > 1) data[o + 1] = (float)or;
                for (int c = 2; c < channels; c++) data[o + c] = 0f;
                m_n++;
                if (++m_inBlock == Block) m_inBlock = 0;
            }
        }

        // ------------------------------------------------------------------ control (every 32 samples at n = 32 k)
        void Control()
        {
            double s, v, x, z, yaw, sceneT, rho, master;
            if (Motion != null)
            {
                Motion.At(m_n, m_fs, out s, out v, out x, out z, out yaw, out sceneT);
                rho = Motion.Rho(s); master = Motion.Master(m_n, m_fs);
            }
            else
            {
                double dt = Math.Min(0.25, Math.Max(0.0, (m_n - m_nSnap) / m_fs));
                s = Math.Max(0.0, m_p.s + m_p.speed * dt);
                v = m_p.speed;
                NocLanes.Path.PointAt(NocMath.Clamp(s, 0.0, NocturneConstants.GoalS), out double px, out double pz);
                NocLanes.Path.PointAt(NocMath.Clamp(m_p.s, 0.0, NocturneConstants.GoalS), out double qx, out double qz);
                x = m_p.x + (px - qx); z = m_p.z + (pz - qz);
                yaw = m_p.yaw; sceneT = m_p.t + dt; rho = m_p.rain; master = m_p.master;
            }
            ref ScoreFrame f = ref m_f;
            double dsBlock = m_seen ? s - m_lastS : 0.0;
            f.N = m_n; f.T = m_n / m_fs; f.SceneT = sceneT; f.S = s; f.V = v; f.X = x; f.Z = z; f.Yaw = yaw;
            f.Beat = NocMath.BeatIndex(s);
            double sched = NocturneScore.RainRhoSchedule(s);
            f.Rho = Math.Max(0.0, rho); f.RhoRatio = sched > 1e-6 ? f.Rho / sched : 1.0;
            // gates: the pad entry (S1) and the goal release (S8)
            if (!m_seen) { if (s >= NocturneScore.S1End) m_entryN = m_n - (long)(NocturneScore.PadAttackSeconds * m_fs); if (s >= NocturneScore.GoalTriggerS) m_goalN = m_n; }
            else
            {
                if (m_entryN == long.MinValue && s >= NocturneScore.S1End) m_entryN = m_n;
                if (s < NocturneScore.S1End - 1.0) m_entryN = long.MinValue;
                if (m_goalN == long.MinValue && s >= NocturneScore.GoalTriggerS) m_goalN = m_n;
                if (s < NocturneScore.GoalTriggerS - 3.0) m_goalN = long.MinValue;
            }
            double attack = m_entryN == long.MinValue ? 0.0 : NocturneScore.AttackGain((m_n - m_entryN) / m_fs);
            double release = m_goalN == long.MinValue ? 1.0 : NocturneScore.ReleaseGain((m_n - m_goalN) / m_fs);
            // listener velocity along x (Doppler of the passes)
            NocLanes.Path.TangentAt(NocMath.Clamp(s, 0.0, NocturneConstants.GoalS), out double tx, out _);
            double vx = v * tx;
            // layers
            m_rain.Control(in f); m_drips.Control(in f); m_hum.Control(in f); m_traffic.Control(in f); m_wind.Control(in f);
            m_pad.Control(s, f.T);
            m_koi.Control(in f);
            double w0 = NocturneScore.BeatWeight(0, s), w1 = NocturneScore.BeatWeight(1, s), w2 = NocturneScore.BeatWeight(2, s), w3 = NocturneScore.BeatWeight(3, s);
            double w4 = NocturneScore.BeatWeight(4, s), w5 = NocturneScore.BeatWeight(5, s), w6 = NocturneScore.BeatWeight(6, s);
            m_alley.Control(in f, w0 * XAmp(0)); m_canyon.Control(in f, w1 * XAmp(1)); m_edge.Control(in f, w2 * XAmp(2));
            m_tube.Control(in f, w3 * XAmp(3)); m_screen.Control(in f, w5 * XAmp(5)); m_lamp.Control(in f, w6 * XAmp(6));
            m_leaf.Control(in f);
            m_gLeaf.Step(w4 * XAmp(4) * NocAudio.Db(NocturneScore.LeafRainSwellDb(s)) * NocturneScore.SwellGain(NocturneScore.X, s) * release);
            m_tram.Control(in f, vx);
            for (int i = 0; i < m_trains.Length; i++) m_trains[i].Control(in f, vx);
            // gains
            double rr = Math.Sqrt(Math.Max(0.0, Math.Min(4.0, f.RhoRatio)));
            m_gR.Step(NocturneScore.LayerAmp(NocturneScore.R, s) * rr);
            m_gD.Step(NocturneScore.LayerAmp(NocturneScore.D, s) * Math.Min(1.0, rr * 1.5));
            m_gH.Step(NocturneScore.LayerAmp(NocturneScore.H, s));
            m_gT.Step(NocturneScore.LayerAmp(NocturneScore.T, s) * release);
            m_gW.Step(NocturneScore.LayerAmp(NocturneScore.W, s) * release);
            m_gP.Step(NocturneScore.LayerAmp(NocturneScore.P, s) * PadOutNorm * attack * release);
            m_gK.Step(NocturneScore.LayerAmp(NocturneScore.K, s) * release);
            double xs = NocturneScore.SwellGain(NocturneScore.X, s);
            m_gX.Step(xs * release);
            m_gPass.Step(xs * release);
            m_gLamp.Step(xs);
            m_gF.Step(NocturneScore.SwellGain(NocturneScore.F, s) * NocAudio.Db(FootDb));
            m_master.Step(Math.Max(0.0, Math.Min(1.5, master)));
            // reverb by place (14.3, 14.6 windows)
            NocturneScore.Reverb(s, out double rt, out double pre, out double damp, out double wet);
            m_env.SetTargets(rt, pre, wet, damp); m_env.Control(Block); m_padFdn.Control(Block);
            // offline footsteps by distance
            if (AutoSteps && m_seen && dsBlock > 0.0)
            {
                int at = m_sched.Advance(dsBlock, v, Block);
                if (at >= 0)
                {
                    StepScheduler.Variation(m_sched.Index, m_stepSeed, out double gdb, out double pitch, out double pan, out int variant);
                    m_steps.Start(NocturneScore.SurfaceAt(s), variant, NocAudio.Db(gdb) * PlayerMath.FootstepGain(v), pan, pitch, at);
                }
            }
            m_seen = true; m_lastS = s;
            if (++m_flushCount >= 64) { m_flushCount = 0; FlushAll(); }
        }

        GainRamp m_gLeaf, m_gLamp;

        static double XAmp(int b)
        {
            double v = NocturneScore.LayerDb[b, NocturneScore.X];
            return double.IsNaN(v) ? 0.0 : NocAudio.Db(v + NocturneScore.BeatTrimDb[b]) * XBedNorm[b];
        }

        /// <summary>Per-beat normalisers of the X beds (unit RMS at gain 1; measured by the harness "calibrate").</summary>
        public static readonly double[] XBedNorm = { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

        void FlushAll()
        {
            m_rain.Flush(); m_drips.Flush(); m_hum.Flush(); m_traffic.Flush(); m_wind.Flush(); m_pad.Flush(); m_leaf.Flush();
            m_alley.Flush(); m_canyon.Flush(); m_edge.Flush(); m_tube.Flush(); m_tram.Flush();
            for (int i = 0; i < m_trains.Length; i++) m_trains[i].Flush();
            m_env.Flush(); m_padFdn.Flush(); m_dcL.Flush(); m_dcR.Flush();
        }

        // ------------------------------------------------------------------ one sample
        /// <summary>Solo mask over the stems (bit NocturneScore.R .. F; tests and the harness): a cleared bit mutes that layer.</summary>
        public int SoloMask = -1;
        readonly DcBlockD[] m_stemDc = new DcBlockD[2 * NocturneScore.StemCount];
        bool m_stemDcInit;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void Meter(int stem, double l, double r)
        {
            // stems are metered after their own 20 Hz DC blocker: what the master DC blocker lets through (brown hum, koi)
            double a = m_stemDc[2 * stem].Tick(l), b = m_stemDc[2 * stem + 1].Tick(r);
            StemSumSq[stem] += 0.5 * (a * a + b * b);
        }

        void Tick(double u, out double outL, out double outR)
        {
            long n = m_n;
            int mask = SoloMask;
            bool meter = MeterStems;
            if (meter && !m_stemDcInit) { for (int i = 0; i < m_stemDc.Length; i++) m_stemDc[i].Init(NocturneConstants.DcBlockHz, m_fs); m_stemDcInit = true; }
            double l, r, sl = 0, sr = 0, dl = 0, dr = 0, xl, xr, g;
            // R
            l = 0; r = 0;
            if ((mask & (1 << NocturneScore.R)) != 0) m_rain.Tick(u, n, out l, out r);
            g = (mask & (1 << NocturneScore.R)) != 0 ? m_gR.At(u) : 0.0;
            xl = g * l; xr = g * r; dl += xl; dr += xr; sl += SendR * xl; sr += SendR * xr;
            if (meter) Meter(NocturneScore.R, xl, xr);
            // D
            l = 0; r = 0;
            if ((mask & (1 << NocturneScore.D)) != 0) m_drips.Tick(u, out l, out r);
            g = (mask & (1 << NocturneScore.D)) != 0 ? m_gD.At(u) : 0.0;
            xl = g * l; xr = g * r; dl += xl; dr += xr; sl += SendD * xl; sr += SendD * xr;
            if (meter) Meter(NocturneScore.D, xl, xr);
            // H
            l = 0; r = 0;
            if ((mask & (1 << NocturneScore.H)) != 0) m_hum.Tick(u, out l, out r);
            g = (mask & (1 << NocturneScore.H)) != 0 ? m_gH.At(u) : 0.0;
            xl = g * l; xr = g * r; dl += xl; dr += xr;
            if (meter) Meter(NocturneScore.H, xl, xr);
            // T
            l = 0; r = 0;
            if ((mask & (1 << NocturneScore.T)) != 0) m_traffic.Tick(u, out l, out r);
            g = (mask & (1 << NocturneScore.T)) != 0 ? m_gT.At(u) : 0.0;
            xl = g * l; xr = g * r; dl += xl; dr += xr; sl += SendT * xl; sr += SendT * xr;
            if (meter) Meter(NocturneScore.T, xl, xr);
            // W
            g = (mask & (1 << NocturneScore.W)) != 0 ? m_gW.At(u) : 0.0;
            xl = 0; xr = 0;
            if (!m_gW.Silent && g > 0.0)
            {
                m_wind.Tick(u, out l, out r);
                xl = g * l; xr = g * r; dl += xl; dr += xr; sl += SendW * xl; sr += SendW * xr;
            }
            if (meter) Meter(NocturneScore.W, xl, xr);
            // P (own FDN)
            m_pad.Tick(u, out l, out r); g = (mask & (1 << NocturneScore.P)) != 0 ? m_gP.At(u) : 0.0;
            double pl = g * l, pr = g * r;
            m_padFdn.Tick((float)pl, (float)pr, out float pwl, out float pwr);
            pl += pwl; pr += pwr;
            dl += pl; dr += pr;
            if (meter) Meter(NocturneScore.P, pl, pr);
            // K
            g = (mask & (1 << NocturneScore.K)) != 0 ? m_gK.At(u) : 0.0;
            xl = 0; xr = 0;
            if (!m_gK.Silent && g > 0.0)
            {
                m_koi.Tick(u, out l, out r);
                xl = g * l; xr = g * r; dl += xl; dr += xr; sl += SendK * xl; sr += SendK * xr;
            }
            if (meter) Meter(NocturneScore.K, xl, xr);
            // X: beds (each carries its own beat weight and level) x the X swells, leaf rain and the lamp, the passes on top
            double gx = m_gX.At(u), bl = 0, br = 0, lal = 0, lar = 0, ll = 0, lr = 0, ql = 0, qr = 0;
            double mx = (mask & (1 << NocturneScore.X)) != 0 ? 1.0 : 0.0;
            double gl = m_gLamp.At(u), gp = m_gPass.At(u);
            if (mx > 0.0)
            {
                m_alley.Tick(u, out l, out r); bl += l; br += r;
                m_canyon.Tick(u, out l, out r); bl += l; br += r;
                m_edge.Tick(u, out l, out r); bl += l; br += r;
                m_tube.Tick(u, out l, out r); bl += l; br += r;
                m_screen.Tick(u, out l, out r); bl += l; br += r;
                m_lamp.Tick(u, out lal, out lar);
                double lg = m_gLeaf.At(u);
                if (!m_gLeaf.Silent) { m_leaf.Tick(u, n, out l, out r); ll = lg * l; lr = lg * r; }
                if (m_tram.Active) { m_tram.Tick(u, out l, out r); ql += l; qr += r; }
                for (int i = 0; i < m_trains.Length; i++) if (m_trains[i].Active) { m_trains[i].Tick(u, out l, out r); ql += l; qr += r; }
            }
            xl = mx * (gx * bl + gp * ql + ll + gl * lal); xr = mx * (gx * br + gp * qr + lr + gl * lar);
            dl += xl; dr += xr;
            sl += mx * (SendX * (gx * bl + ll) + SendPass * gp * ql); sr += mx * (SendX * (gx * br + lr) + SendPass * gp * qr);
            if (meter) Meter(NocturneScore.X, xl, xr);
            // F
            l = 0; r = 0;
            if ((mask & (1 << NocturneScore.F)) != 0) m_steps.Tick(out l, out r);
            g = (mask & (1 << NocturneScore.F)) != 0 ? m_gF.At(u) : 0.0;
            xl = g * l; xr = g * r; dl += xl; dr += xr; sl += SendF * xl; sr += SendF * xr;
            if (meter) Meter(NocturneScore.F, xl, xr);
            // environment reverb, DC, master, limiter
            m_env.Tick((float)sl, (float)sr, out float el, out float er);
            double ol = m_dcL.Tick(dl + el), or = m_dcR.Tick(dr + er);
            double m = m_master.At(u);
            ol *= m; or *= m;
            m_lim.Process(ref ol, ref or);
            if (double.IsNaN(ol) || double.IsNaN(or) || double.IsInfinity(ol) || double.IsInfinity(or))
            {
                ol = 0; or = 0; NanResets++;
                m_env.Clear(); m_padFdn.Clear(); m_dcL.Clear(); m_dcR.Clear();
                m_lim.Configure(m_fs, LimiterThresholdDb, NocturneConstants.LimiterCeilingDb, NocturneConstants.LimiterLookAheadMs, NocturneConstants.LimiterReleaseMs, true);
            }
            if (meter) StemFrames++;
            outL = ol; outR = or;
        }
    }
}
