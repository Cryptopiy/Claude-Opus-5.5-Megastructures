// DripField: the D layer of MASTER_PROMPT 14.3 (drips with resonances: awning 1.6 kHz, puddle 300-900 Hz, leaf 3-6 kHz;
// docs/research/R5 4.2). Emitters are virtual and keyed to s: a lattice of 1.2 m along the walked centre line, each slot an
// emitter with the probability, type mix, side, height and mean interval of its beat (alley awnings and puddles, boulevard
// shop awnings and puddles, sparse puddles on the Edge and the plaza, rail drips on the bridge deck, dense leaves in the
// garden, arcade drips under the wall of signs), all from hashes of the slot index. Event k of emitter e falls at the
// jittered lattice time t_k = mu_e (k + 0.5 + 0.8 (hash(e, k) - 0.5)), a pure function of (e, k), scheduled every 8 control
// blocks with sample-accurate delays, so the field is the same for any callback size. Only emitters within 14 m sound
// (gain 1 / (1 + (d / 4)^2), +-4 dB by hash, equal-power pan from the azimuth to the listener's yaw, rear sources darker).
// Voices: puddle = Minnaert bubble chirp (f(t) = f0 (1 + 0.1 beta t) integrated into the phase, beta = 0.044 f0 + 0.0015
// f0^1.5, T60 <= 250 ms) + a 2 ms tick; awning = two damped modes (1.6 kHz +-8 %, x1.5-1.8 at -8 dB); leaf = 3-6 kHz tick +
// 150-210 Hz tap; all start at phase 0 (no click). 24 voices, the quietest stolen. Per-beat normalisers (DripNorm, from
// the harness's 600 s calibration render) make the layer unit RMS at gain 1 in every beat. Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    public sealed class DripField
    {
        public const int Voices = 24;
        public const double Spacing = 1.2, ActiveRadius = 14.0, RefDistance = 4.0, Jitter = 0.8, Xi = 0.1;
        public const int ScheduleBlocks = 8;
        public enum Kind { None = 0, Puddle = 1, Awning = 2, Leaf = 3, Rail = 4 }

        // per beat: emitter probability per slot, shares (puddle, awning, leaf, rail), mean interval range (s), lateral range (m)
        static readonly double[] Prob = { 0.85, 0.60, 0.35, 0.50, 0.95, 0.60, 0.40 };
        static readonly double[,] Share =
        {
            { 0.45, 0.55, 0.00, 0.00 }, { 0.65, 0.35, 0.00, 0.00 }, { 1.00, 0.00, 0.00, 0.00 }, { 0.35, 0.00, 0.00, 0.65 },
            { 0.15, 0.00, 0.85, 0.00 }, { 0.50, 0.50, 0.00, 0.00 }, { 1.00, 0.00, 0.00, 0.00 },
        };
        static readonly double[] MuLo = { 0.5, 0.8, 1.5, 1.2, 0.35, 0.8, 1.5 };
        static readonly double[] MuHi = { 2.5, 3.5, 5.0, 4.0, 1.8, 3.5, 5.0 };
        static readonly double[] LatLo = { 1.0, 2.5, 3.0, 1.5, 1.5, 3.0, 3.0 };
        static readonly double[] LatHi = { 2.4, 9.0, 10.0, 2.6, 9.0, 10.0, 11.0 };
        /// <summary>Per-beat normalisers (unit RMS at gain 1; measured by Tools/audio/harness "calibrate", 600 s at 2.5 m/s).</summary>
        public static readonly double[] DripNorm = { 12.9, 30.7, 36.8, 21.8, 29.9, 46.4, 60.2 };

        struct Voice
        {
            public Kind K;
            public int Delay;                  // samples until the voice starts
            public double A1, E1, M1, Ph1, Inc1, IncSlope;   // mode 1 (bubble or first mode)
            public double A2, E2, M2, Ph2, Inc2;             // mode 2 (tick, second mode, tap)
            public double GL, GR;
            public double Level => (A1 * E1 + A2 * E2) * (GL + GR);
        }

        readonly Voice[] m_v = new Voice[Voices];
        readonly double m_fs;
        readonly ulong m_seed;
        int m_sched;
        SvfD m_darkL, m_darkR;
        double m_norm = 1.0;
        GainRamp m_normRamp;

        public DripField(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            m_seed = DspHash.Mix(seed ^ 0xD819UL);
            m_darkL.Set(9000.0, 0.7071, fs); m_darkR.CopyCoefficients(in m_darkL);
            m_normRamp.Init(0.2, controlRate);
        }

        /// <summary>Emitter of lattice slot i: whether it exists, its kind, position (path s, lateral, height) and mean interval.</summary>
        public static bool Emitter(ulong seed, long i, out Kind kind, out double se, out double lateral, out double height, out double mu)
        {
            kind = Kind.None; lateral = 0; height = 0; mu = 1;
            se = (i + 0.5 + 0.6 * (NocAudio.Hash01(seed, (ulong)(4 * i)) - 0.5)) * Spacing;
            if (se < 0.0 || se > NocturneConstants.GoalS + 10.0) return false;
            int b = NocMath.BeatIndex(se);
            if (NocAudio.Hash01(seed, (ulong)(4 * i + 1)) >= Prob[b]) return false;
            double h = NocAudio.Hash01(seed, (ulong)(4 * i + 2)), acc = 0;
            for (int k = 0; k < 4; k++) { acc += Share[b, k]; if (h < acc) { kind = (Kind)(k + 1); break; } }
            if (kind == Kind.None) kind = Kind.Puddle;
            double hs = NocAudio.Hash01(seed, (ulong)(4 * i + 3));
            double side = hs < 0.5 ? -1.0 : 1.0, mag = LatLo[b] + (LatHi[b] - LatLo[b]) * (2.0 * (hs < 0.5 ? hs : hs - 0.5));
            lateral = side * mag;
            height = kind == Kind.Puddle ? 0.0 : (kind == Kind.Leaf ? 2.0 + 6.0 * hs : 2.6 + 1.2 * hs);
            double hm = NocAudio.Hash01(seed ^ 0x77UL, (ulong)i);
            mu = MuLo[b] + (MuHi[b] - MuLo[b]) * hm;
            return true;
        }

        public void Control(in ScoreFrame f)
        {
            m_normRamp.Step(DripNorm[f.Beat]);
            if (m_sched++ % ScheduleBlocks != 0) return;
            int win = ScheduleBlocks * f.Block;
            double t0 = f.T, t1 = f.T + win * f.InvFs;
            long i0 = (long)Math.Floor((f.S - ActiveRadius) / Spacing) - 1, i1 = (long)Math.Ceiling((f.S + ActiveRadius) / Spacing) + 1;
            for (long i = Math.Max(0, i0); i <= i1; i++)
            {
                if (!Emitter(m_seed, i, out Kind kind, out double se, out double lat, out double hgt, out double mu)) continue;
                NocLanes.Path.CorridorPoint(NocMath.Clamp(se, 0.0, NocturneConstants.GoalS), lat, out double ex, out double ez);
                double dx = ex - f.X, dz = ez - f.Z, dy = hgt - NocturneConstants.EyeHeight;
                double d = Math.Sqrt(dx * dx + dz * dz + dy * dy);
                if (d > ActiveRadius) continue;
                long k0 = (long)Math.Floor(t0 / mu) - 1, k1 = (long)Math.Floor(t1 / mu) + 1;
                for (long k = Math.Max(0, k0); k <= k1; k++)
                {
                    ulong eid = unchecked((ulong)i * 0x9E3779B1UL + 0x51UL);
                    double tk = mu * (k + 0.5 + Jitter * (NocAudio.Hash01(m_seed ^ eid, (ulong)k) - 0.5));
                    if (tk < t0 || tk >= t1) continue;
                    double g = 1.0 / (1.0 + (d / RefDistance) * (d / RefDistance));
                    g *= NocAudio.Db(-4.0 + 8.0 * NocAudio.Hash01(m_seed ^ eid ^ 0xC3UL, (ulong)k));
                    g *= 0.55 + 0.45 * f.FrontTo(ex, ez);
                    Trigger(kind, (int)Math.Floor((tk - t0) * m_fs), g, f.PanTo(ex, ez), NocAudio.Hash01(m_seed ^ eid ^ 0x1FUL, (ulong)k), NocAudio.Hash01(m_seed ^ eid ^ 0x2FUL, (ulong)k));
                }
            }
        }

        void Trigger(Kind kind, int delay, double gain, double pan, double h1, double h2)
        {
            int best = 0; double bl = double.MaxValue;
            for (int i = 0; i < Voices; i++)
            {
                if (m_v[i].K == Kind.None) { best = i; bl = -1; break; }
                double lv = m_v[i].Level;
                if (lv < bl) { bl = lv; best = i; }
            }
            ref Voice v = ref m_v[best];
            v = default(Voice);
            v.K = kind; v.Delay = Math.Max(0, delay);
            NocAudio.Pan(pan, out v.GL, out v.GR);
            double fs = m_fs;
            switch (kind)
            {
                case Kind.Puddle:
                {
                    double f0 = 300.0 + 600.0 * h1;
                    double beta = Math.Max(0.044 * f0 + 0.0015 * Math.Pow(f0, 1.5), 6.9078 / 0.25);
                    v.Inc1 = f0 / fs; v.IncSlope = f0 * Xi * beta / (fs * fs); v.M1 = Math.Exp(-beta / fs); v.A1 = gain; v.E1 = 1.0;
                    v.Inc2 = (2500.0 + 2000.0 * h2) / fs; v.M2 = NocAudio.T60Mul(0.002, fs); v.A2 = 0.25 * gain; v.E2 = 1.0;
                    break;
                }
                case Kind.Awning:
                case Kind.Rail:
                {
                    double fA = (kind == Kind.Awning ? 1600.0 : 2400.0) * (0.92 + 0.16 * h1), ratio = 1.5 + 0.3 * h2;
                    v.Inc1 = fA / fs; v.M1 = NocAudio.T60Mul(kind == Kind.Awning ? 0.060 : 0.110, fs); v.A1 = gain; v.E1 = 1.0;
                    v.Inc2 = fA * ratio / fs; v.M2 = NocAudio.T60Mul(kind == Kind.Awning ? 0.035 : 0.070, fs); v.A2 = 0.4 * gain; v.E2 = 1.0;
                    break;
                }
                default:
                {
                    v.Inc1 = (3000.0 + 3000.0 * h1) / fs; v.M1 = NocAudio.T60Mul(0.010, fs); v.A1 = 0.8 * gain; v.E1 = 1.0;
                    v.Inc2 = (150.0 + 60.0 * h2) / fs; v.M2 = NocAudio.T60Mul(0.025, fs); v.A2 = 0.5 * gain; v.E2 = 1.0;
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            double sl = 0, sr = 0;
            for (int i = 0; i < Voices; i++)
            {
                ref Voice v = ref m_v[i];
                if (v.K == Kind.None) continue;
                if (v.Delay > 0) { v.Delay--; continue; }
                double x = v.A1 * v.E1 * NocAudio.Sin01(v.Ph1) + v.A2 * v.E2 * NocAudio.Sin01(v.Ph2);
                v.Ph1 += v.Inc1; if (v.Ph1 >= 1.0) v.Ph1 -= 1.0;
                v.Inc1 += v.IncSlope;
                v.Ph2 += v.Inc2; if (v.Ph2 >= 1.0) v.Ph2 -= 1.0;
                v.E1 *= v.M1; v.E2 *= v.M2;
                sl += v.GL * x; sr += v.GR * x;
                if (v.E1 * v.A1 < 1e-5 && v.E2 * v.A2 < 1e-5) v.K = Kind.None;
            }
            double g = m_normRamp.At(u);
            l = g * m_darkL.Low(sl); r = g * m_darkR.Low(sr);
        }

        public void Flush() { m_darkL.Flush(); m_darkR.Flush(); }
    }
}
