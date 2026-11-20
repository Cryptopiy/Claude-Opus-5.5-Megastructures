// Footsteps: the steps of MASTER_PROMPT 14.3 "Footsteps" (one per stride, 0.75 m at 1.5 m/s = 2.00/s, 0.85 m at 2.5 m/s =
// 2.94/s; alternate feet, +-0.8 dB, pitch +-4 %, peak -30 dBFS) in the pattern of Thread's FootstepBank (variants rendered
// once at construction, 1 ms attack, every "ms" read as T60, cosine fade to an exact zero, each variant normalised to its
// exact peak) with the recipes of docs/research/R5 4.7:
//   wet concrete (s 0-120): heel thump (noise LP 150 Hz, 25 ms) + scuff (BP 2.5 kHz, 15 ms, -8 dB) + splash (BP 1.2 kHz Q 1.2
//                noise, 90 ms) + 8-20 micro-droplets (1-3 ms windowed sines at 1.5-5 kHz, 10-80 ms, falling level);
//   wet stone (120-560, 1 080-1 260): click (0.5 ms impulse into BP 3 kHz) + 200 Hz damped sine (60 ms) + splash at -10 dB;
//   steel deck (560-880): rings 310 Hz (250 ms) and 820 Hz (150 ms) + 85 Hz thump (90 ms) + scuff;
//   glass strip (706-726): 640 Hz ring (350 ms) with partials x1.73 and x2.9 (-14, -20 dB) + tick, no splash;
//   garden (880-1 080): soft noise BP 900 Hz Q 0.8 (120 ms, 15 ms attack) + 120 Hz thud (40 ms);
//   plaza stone (1 260-1 500): the wet stone recipe with the splash 3 dB lower.
// FootstepVoices mixes up to 4 overlapping steps inside the DSP (linear-interpolated pitch), so they pass through the
// environment reverb and the one limiter. StepScheduler places steps by distance at sample accuracy for the offline render
// (the player uses the controller's own stride events). Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    public sealed class FootstepBank
    {
        public const int Surfaces = 6, Variants = 8;
        public const double PeakDb = -30.0, Attack = 0.001, FadeOut = 0.03;
        public static readonly double[] ClipSeconds = { 0.26, 0.22, 0.46, 0.52, 0.26, 0.22 };
        readonly float[][][] m_clips;
        public readonly int SampleRate;

        public FootstepBank(int sampleRate, uint seed)
        {
            SampleRate = sampleRate;
            m_clips = new float[Surfaces][][];
            for (int s = 0; s < Surfaces; s++)
            {
                m_clips[s] = new float[Variants][];
                for (int v = 0; v < Variants; v++) m_clips[s][v] = Render((FootSurface)s, v, seed, sampleRate);
            }
        }

        public float[] Get(FootSurface s, int variant) => m_clips[(int)s][((variant % Variants) + Variants) % Variants];

        static double Env(double t, double t60) => Math.Exp(-6.907755278982137 * t / t60);

        /// <summary>One variant of a surface (exact zero ends, peak normalised to -30 dBFS).</summary>
        public static float[] Render(FootSurface surface, int variant, uint seed, int fs)
        {
            int n = (int)(ClipSeconds[(int)surface] * fs);
            var x = new double[n];
            var rng = NoiseD.Stream(seed, 0x5700u + (uint)((int)surface * 16 + variant));
            ulong hs = DspHash.Mix(((ulong)seed << 16) ^ (ulong)((int)surface * 16 + variant) ^ 0xF00DUL);
            double J(int k, double amount) => 1.0 + amount * (2.0 * NocAudio.Hash01(hs, (ulong)k) - 1.0);
            switch (surface)
            {
                case FootSurface.WetConcrete:
                    Thump(x, fs, ref rng, 150.0 * J(1, 0.1), 0.025 * J(2, 0.15), 1.0);
                    Band(x, fs, ref rng, 2500.0 * J(3, 0.1), 1.0, 0.0, 0.015 * J(4, 0.2), NocAudio.Db(-8.0));
                    Band(x, fs, ref rng, 1200.0 * J(5, 0.08), 1.2, 0.003, 0.090 * J(6, 0.15), 1.1);
                    Droplets(x, fs, hs, 8 + (int)(13 * NocAudio.Hash01(hs, 7)), 0.4);
                    break;
                case FootSurface.WetStone:
                case FootSurface.PlazaStone:
                    Click(x, fs, ref rng, 3000.0 * J(1, 0.1), 1.6);
                    Ring(x, fs, 200.0 * J(2, 0.06), 0.060 * J(3, 0.15), 0.9, 0.002);
                    Band(x, fs, ref rng, 1200.0 * J(5, 0.08), 1.2, 0.003, 0.090 * J(6, 0.15), NocAudio.Db(surface == FootSurface.PlazaStone ? -13.0 : -10.0) * 1.1);
                    Droplets(x, fs, hs, 4 + (int)(6 * NocAudio.Hash01(hs, 7)), surface == FootSurface.PlazaStone ? 0.12 : 0.18);
                    break;
                case FootSurface.Steel:
                    Ring(x, fs, 310.0 * J(1, 0.03), 0.250 * J(2, 0.1), 1.0, 0.001);
                    Ring(x, fs, 820.0 * J(3, 0.03), 0.150 * J(4, 0.1), 0.55, 0.001);
                    Ring(x, fs, 85.0 * J(5, 0.04), 0.090 * J(6, 0.1), 0.8, 0.002);
                    Band(x, fs, ref rng, 2500.0 * J(7, 0.1), 1.0, 0.0, 0.015, 0.35);
                    break;
                case FootSurface.Glass:
                    Ring(x, fs, 640.0 * J(1, 0.02), 0.350 * J(2, 0.1), 1.0, 0.001);
                    Ring(x, fs, 640.0 * 1.73 * J(3, 0.02), 0.250, NocAudio.Db(-14.0), 0.001);
                    Ring(x, fs, 640.0 * 2.9 * J(4, 0.02), 0.180, NocAudio.Db(-20.0), 0.001);
                    Click(x, fs, ref rng, 4500.0 * J(5, 0.1), 0.6);
                    break;
                default:   // garden
                    Band(x, fs, ref rng, 900.0 * J(1, 0.1), 0.8, 0.015, 0.120 * J(2, 0.15), 1.0);
                    Ring(x, fs, 120.0 * J(3, 0.06), 0.040 * J(4, 0.15), 0.7, 0.004);
                    break;
            }
            // cosine fade to an exact zero, peak normalised, exact zero ends
            int fo = Math.Min(n - 1, (int)(FadeOut * fs));
            for (int i = n - fo; i < n; i++) x[i] *= 0.5 + 0.5 * Math.Cos(Math.PI * (i - (n - fo)) / Math.Max(1, fo - 1));
            double pk = 0; for (int i = 0; i < n; i++) pk = Math.Max(pk, Math.Abs(x[i]));
            double k = pk > 1e-12 ? NocAudio.Db(PeakDb) / pk : 0.0;
            var o = new float[n];
            for (int i = 0; i < n; i++) o[i] = (float)(x[i] * k);
            o[0] = 0f; o[n - 1] = 0f;
            return o;
        }

        static void Thump(double[] x, int fs, ref NoiseD rng, double lpHz, double t60, double gain)
        {
            var lp = new SvfD(); lp.Set(lpHz, 0.7071, fs);
            double norm = 1.0 / Math.Sqrt(Math.PI * lpHz / (2.0 * fs) / 3.0);
            for (int i = 0; i < x.Length; i++)
            {
                double t = i / (double)fs;
                x[i] += gain * norm * lp.Low(rng.Next()) * Math.Min(1.0, t / Attack) * Env(t, t60);
            }
        }

        static void Band(double[] x, int fs, ref NoiseD rng, double hz, double q, double attack, double t60, double gain)
        {
            var bp = new SvfD(); bp.Set(hz, q, fs);
            double norm = 1.0 / Math.Sqrt(Math.PI * hz / (q * fs) / 3.0);
            double at = Math.Max(Attack, attack);
            for (int i = 0; i < x.Length; i++)
            {
                double t = i / (double)fs;
                x[i] += gain * norm * bp.BandNorm(rng.Next()) * Math.Min(1.0, t / at) * Env(t, t60);
            }
        }

        static void Click(double[] x, int fs, ref NoiseD rng, double hz, double gain)
        {
            var bp = new SvfD(); bp.Set(hz, 1.5, fs);
            int w = Math.Max(2, (int)(0.0005 * fs));
            for (int i = 0; i < x.Length; i++)
            {
                double e = i < w ? Math.Sin(Math.PI * i / w) : 0.0;
                x[i] += gain * 6.0 * bp.BandNorm(e * rng.Next());
            }
        }

        static void Ring(double[] x, int fs, double hz, double t60, double gain, double attack)
        {
            double ph = 0, inc = hz / fs;
            for (int i = 0; i < x.Length; i++)
            {
                double t = i / (double)fs;
                x[i] += gain * Math.Sin(NocAudio.TwoPi * ph) * Math.Min(1.0, t / attack) * Env(t, t60);
                ph += inc; if (ph >= 1.0) ph -= 1.0;
            }
        }

        static void Droplets(double[] x, int fs, ulong hs, int count, double level)
        {
            for (int c = 0; c < count; c++)
            {
                double at = 0.010 + 0.070 * NocAudio.Hash01(hs ^ 0xD0UL, (ulong)c);
                double len = 0.001 + 0.002 * NocAudio.Hash01(hs ^ 0xD1UL, (ulong)c);
                double hz = 1500.0 + 3500.0 * NocAudio.Hash01(hs ^ 0xD2UL, (ulong)c);
                double a = level * (1.0 - 0.8 * (at - 0.010) / 0.070) * (0.5 + 0.5 * NocAudio.Hash01(hs ^ 0xD3UL, (ulong)c));
                int i0 = (int)(at * fs), w = Math.Max(2, (int)(len * fs));
                for (int j = 0; j < w && i0 + j < x.Length; j++)
                    x[i0 + j] += a * Math.Sin(NocAudio.TwoPi * hz * j / fs) * Math.Sin(Math.PI * j / w);
            }
        }
    }

    /// <summary>Up to 4 overlapping steps mixed in the DSP (linear interpolation for the +-4 % pitch, equal-power pan).</summary>
    public sealed class FootstepVoices
    {
        public const int Voices = 4;
        readonly FootstepBank m_bank;
        readonly float[][] m_clip = new float[Voices][];
        readonly double[] m_pos = new double[Voices], m_rate = new double[Voices], m_gl = new double[Voices], m_gr = new double[Voices];
        readonly int[] m_delay = new int[Voices];
        int m_next;

        public FootstepVoices(FootstepBank bank) { m_bank = bank; }

        /// <summary>Starts a step `delay` samples from now (gain linear, pan -1..1, pitch ratio, variant index).</summary>
        public void Start(FootSurface surface, int variant, double gain, double pan, double pitch, int delay)
        {
            int v = m_next; m_next = (m_next + 1) % Voices;
            m_clip[v] = m_bank.Get(surface, variant);
            m_pos[v] = 0.0; m_rate[v] = pitch; m_delay[v] = Math.Max(0, delay);
            NocAudio.Pan(pan, out double l, out double r);
            m_gl[v] = gain * l; m_gr[v] = gain * r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(out double l, out double r)
        {
            l = 0; r = 0;
            for (int v = 0; v < Voices; v++)
            {
                var c = m_clip[v];
                if (c == null) continue;
                if (m_delay[v] > 0) { m_delay[v]--; continue; }
                double p = m_pos[v];
                int i = (int)p;
                if (i + 1 >= c.Length) { m_clip[v] = null; continue; }
                double f = p - i, x = c[i] + f * (c[i + 1] - c[i]);
                l += m_gl[v] * x; r += m_gr[v] * x;
                m_pos[v] = p + m_rate[v];
            }
        }
    }

    /// <summary>Distance-keyed step cadence (R5 4.7): stride L(v) = PlayerMath.Stride(v); one step each time the walked distance
    /// passes a stride, placed at its sample inside the control block; variation by a hash of the step index.</summary>
    public struct StepScheduler
    {
        double m_phase;
        long m_index;
        public long Index => m_index;
        public const double Pan = 0.12;

        /// <summary>Advance by ds metres over a block of `blockFrames` samples; returns the sample offset of a step or -1.</summary>
        public int Advance(double ds, double v, int blockFrames)
        {
            if (ds <= 0.0) return -1;
            double L = PlayerMath.Stride(v);
            double before = m_phase;
            m_phase += ds / L;
            if (m_phase < 1.0) return -1;
            m_phase -= 1.0;
            if (m_phase >= 1.0) m_phase = 0.0;
            m_index++;
            double frac = (1.0 - before) / (ds / L);
            return (int)Math.Min(blockFrames - 1, Math.Max(0.0, Math.Floor(frac * blockFrames)));
        }

        public static void Variation(long index, ulong seed, out double gainDb, out double pitch, out double pan, out int variant)
        {
            gainDb = -0.8 + 1.6 * NocAudio.Hash01(seed, (ulong)index);
            pitch = 0.96 + 0.08 * NocAudio.Hash01(seed + 1UL, (ulong)index);
            pan = (index & 1) == 0 ? -Pan : Pan;
            variant = (int)(FootstepBank.Variants * NocAudio.Hash01(seed + 2UL, (ulong)index));
        }
    }
}
