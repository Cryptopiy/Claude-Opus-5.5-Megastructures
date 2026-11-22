// NocDspBlocks: the double-precision DSP blocks of the Nocturne score (MASTER_PROMPT 14.3, 14.7 H8; docs/research/R5 sections
// 2-5, whose compiled code blocks these are, with the fixes listed in docs/AUDIO.md): counter hashes and equal-power pan
// (NocAudio), the double Simper/Cytomic TPT SVF (SvfD), the double DC blocker (DcBlockD), PolyBLEP saw and 2-point polyBLAMP
// triangle (Blep), the log-frequency smoother, the stereo chorus (14 / 19 ms, 0.6 ms depth, per-sample LFO from the sine
// table, 4-point Hermite reads), the look-ahead limiter with the ITU-R BS.1770-4 Annex 2 true-peak detector (TruePeakLimiter)
// and the matching true-peak meter. Pure C#, no UnityEngine, no allocation after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    /// <summary>Shared helpers of the score: hashes keyed to event or sample indices (never to the callback), pan, pitch.</summary>
    public static class NocAudio
    {
        public const double TwoPi = 2.0 * Math.PI;
        public const double InvLn2 = 1.4426950408889634;
        public const double Sqrt2 = 1.4142135623730951;

        public static double Db(double db) => Math.Pow(10.0, db / 20.0);
        public static double ToDb(double amplitude) => 20.0 * Math.Log10(Math.Max(amplitude, 1e-12));
        public static double MidiHz(int n) => 440.0 * Math.Pow(2.0, (n - 69) / 12.0);

        /// <summary>Counter-based hash to [0, 1) (splitmix64 finaliser): schedules never depend on how the callback slices time.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Hash01(ulong a, ulong b)
        {
            unchecked
            {
                ulong z = a * 0x9E3779B97F4A7C15UL + b * 0xD1B54A32D192ED03UL + 0x632BE59BD9B4E019UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                z ^= z >> 31;
                return (z >> 11) * (1.0 / 9007199254740992.0);
            }
        }

        /// <summary>Hash to [-1, 1).</summary>
        public static double HashBi(ulong a, ulong b) => 2.0 * Hash01(a, b) - 1.0;

        /// <summary>Equal-power pan, unity on both channels at the centre: the mean-over-channels power does not depend on p.</summary>
        public static void Pan(double p, out double l, out double r)
        {
            p = p < -1.0 ? -1.0 : (p > 1.0 ? 1.0 : p);
            double a = 0.25 * Math.PI * (1.0 + p);
            l = Sqrt2 * Math.Cos(a);
            r = Sqrt2 * Math.Sin(a);
        }

        /// <summary>Quintic value noise in [0, 1] with one knot per unit of t (a pure function of t: the picture can share it).</summary>
        public static double SmoothRandom01(ulong seed, double t)
        {
            double fl = Math.Floor(t);
            long i = (long)fl;
            double f = t - fl, u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
            double a = Hash01(seed, unchecked((ulong)i)), b = Hash01(seed, unchecked((ulong)(i + 1)));
            return a + (b - a) * u;
        }

        /// <summary>Two-octave smooth random in [-1, 1] (about `rate` independent values per second).</summary>
        public static double SmoothRandom(ulong seed, double t, double rate)
        {
            double x = rate * t;
            return (2.0 * SmoothRandom01(seed, x) - 1.0) * 0.8 + (2.0 * SmoothRandom01(seed ^ 0x5A5Aul, 2.31 * x + 0.37) - 1.0) * 0.2;
        }

        /// <summary>Exponential decay multiplier per sample for a T60 (s).</summary>
        public static double T60Mul(double t60, double fs) => Math.Exp(-6.907755278982137 / (Math.Max(1e-4, t60) * fs));

        /// <summary>Sine of a non-negative phase in turns (any magnitude below 5e5: the table index is masked) through the 4096 table.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sin01(double phase) => SineTable.Lookup(phase);
    }

    /// <summary>White noise source in double: xorshift32 advanced exactly once per sample (partition-invariant).</summary>
    public struct NoiseD
    {
        uint m_s;
        public NoiseD(uint seed) { m_s = seed != 0u ? seed : 0x9E3779B9u; }
        public static NoiseD Stream(uint seed, uint stream) => new NoiseD(XorShift32.Stream(unchecked((int)seed), stream).State);
        /// <summary>Uniform in [-1, 1): variance 1/3.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Next()
        {
            uint x = m_s; x ^= x << 13; x ^= x >> 17; x ^= x << 5; m_s = x;
            return unchecked((int)x) * (1.0 / 2147483648.0);
        }
    }

    /// <summary>Paul Kellet's refined pinking filter in double (input uniform white in [-1, 1); output about unit RMS).</summary>
    public struct PinkD
    {
        double b0, b1, b2, b3, b4, b5, b6;
        public const double UnitRms = 1.0 / 1.755;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Next(double w)
        {
            b0 = 0.99886 * b0 + w * 0.0555179; b1 = 0.99332 * b1 + w * 0.0750759; b2 = 0.96900 * b2 + w * 0.1538520;
            b3 = 0.86650 * b3 + w * 0.3104856; b4 = 0.55000 * b4 + w * 0.5329522; b5 = -0.7616 * b5 - w * 0.0168980;
            double p = b0 + b1 + b2 + b3 + b4 + b5 + b6 + w * 0.5362;
            b6 = w * 0.115926;
            return p * UnitRms;
        }
        public void Flush()
        {
            if (Math.Abs(b0) < 1e-30) b0 = 0; if (Math.Abs(b1) < 1e-30) b1 = 0; if (Math.Abs(b2) < 1e-30) b2 = 0;
            if (Math.Abs(b3) < 1e-30) b3 = 0; if (Math.Abs(b4) < 1e-30) b4 = 0; if (Math.Abs(b5) < 1e-30) b5 = 0;
        }
    }

    /// <summary>Simper/Cytomic linear trapezoidal SVF (= Zavalishin's TPT SVF) with double state; stable under per-sample modulation.</summary>
    public struct SvfD
    {
        double ic1, ic2, a1, a2, a3, k;

        public void Set(double fc, double q, double fs)
        {
            double g = Math.Tan(Math.PI * Math.Min(Math.Max(fc, 5.0), 0.45 * fs) / fs);
            k = 1.0 / Math.Max(q, 0.05);
            a1 = 1.0 / (1.0 + g * (g + k)); a2 = g * a1; a3 = g * a2;
        }
        public void CopyCoefficients(in SvfD o) { a1 = o.a1; a2 = o.a2; a3 = o.a3; k = o.k; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Low(double x)
        {
            double v3 = x - ic2, v1 = a1 * ic1 + a2 * v3, v2 = ic2 + a2 * ic1 + a3 * v3;
            ic1 = 2.0 * v1 - ic1; ic2 = 2.0 * v2 - ic2;
            return v2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double High(double x)
        {
            double v3 = x - ic2, v1 = a1 * ic1 + a2 * v3, v2 = ic2 + a2 * ic1 + a3 * v3;
            ic1 = 2.0 * v1 - ic1; ic2 = 2.0 * v2 - ic2;
            return x - k * v1 - v2;
        }
        /// <summary>Band-pass with a 0 dB peak at fc.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double BandNorm(double x)
        {
            double v3 = x - ic2, v1 = a1 * ic1 + a2 * v3, v2 = ic2 + a2 * ic1 + a3 * v3;
            ic1 = 2.0 * v1 - ic1; ic2 = 2.0 * v2 - ic2;
            return k * v1;
        }
        public void Clear() { ic1 = 0; ic2 = 0; }
        public void Flush() { if (Math.Abs(ic1) < 1e-30) ic1 = 0.0; if (Math.Abs(ic2) < 1e-30) ic2 = 0.0; }
        public bool Finite => !(double.IsNaN(ic1) || double.IsNaN(ic2) || double.IsInfinity(ic1) || double.IsInfinity(ic2));

        /// <summary>|H|^2 of the analog prototype low-pass at frequency ratio w = f / fc (Q q).</summary>
        public static double LowPower(double w, double q)
        {
            double a = 1.0 - w * w;
            return 1.0 / (a * a + w * w / (q * q));
        }
    }

    /// <summary>DC blocker y = x - x1 + R y1, R = exp(-2 pi fc / fs), in double (R = 0.99738543 at 20 Hz / 48 kHz).</summary>
    public struct DcBlockD
    {
        double x1, y1, r;
        public void Init(double fc, double fs) { r = Math.Exp(-2.0 * Math.PI * fc / fs); x1 = 0; y1 = 0; }
        public double R => r;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Tick(double x) { double y = x - x1 + r * y1; x1 = x; y1 = y; return y; }
        public void Clear() { x1 = 0; y1 = 0; }
        public void Flush() { if (Math.Abs(y1) < 1e-30) y1 = 0; }
        /// <summary>|H| at f for the blocker at fc.</summary>
        public static double Magnitude(double f, double fc, double fs) => DcBlocker.Magnitude(f, fc, fs);
    }

    /// <summary>PolyBLEP saw and 2-point polyBLAMP triangle (R5 3.1): ph in [0, 1), dt = the oscillator's own increment (&lt; 0.5).</summary>
    public static class Blep
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double PolyBlep(double t, double dt)
        {
            if (t < dt) { double x = t / dt; return x + x - x * x - 1.0; }
            if (t > 1.0 - dt) { double x = (t - 1.0) / dt; return x * x + x + x + 1.0; }
            return 0.0;
        }
        /// <summary>Residual of a unit slope change (per sample) at phase 0: (1 - |x|)^3 / 6 for |x| &lt; 1 sample.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double PolyBlamp(double t, double dt)
        {
            if (t < dt) { double x = 1.0 - t / dt; return x * x * x * (1.0 / 6.0); }
            if (t > 1.0 - dt) { double x = 1.0 + (t - 1.0) / dt; return x * x * x * (1.0 / 6.0); }
            return 0.0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Saw(double ph, double dt) => 2.0 * ph - 1.0 - PolyBlep(ph, dt);
        /// <summary>-1 at ph 0, +1 at ph 0.5; the corners change slope by +-8 dt per sample.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Tri(double ph, double dt)
        {
            double h = ph + 0.5; if (h >= 1.0) h -= 1.0;
            return 1.0 - 4.0 * Math.Abs(ph - 0.5) + 8.0 * dt * (PolyBlamp(ph, dt) - PolyBlamp(h, dt));
        }
    }

    /// <summary>One-pole on log2(f) at control rate (the pad cutoff: 20 ms).</summary>
    public struct LogFreqSmoother
    {
        double m_y, m_a;
        public void Init(double hz, double tauSeconds, double updatesPerSecond)
        {
            m_y = Math.Log(hz) * NocAudio.InvLn2;
            m_a = 1.0 - Math.Exp(-1.0 / (tauSeconds * updatesPerSecond));
        }
        public void Snap(double hz) { m_y = Math.Log(hz) * NocAudio.InvLn2; }
        public double Next(double targetHz)
        {
            m_y += m_a * (Math.Log(Math.Max(targetHz, 1.0)) * NocAudio.InvLn2 - m_y);
            return Math.Pow(2.0, m_y);
        }
    }

    /// <summary>Control-rate one-pole for gains (50 ms by default): value at the block start and end, linear ramp inside.</summary>
    public struct GainRamp
    {
        public double Prev, Cur;
        double m_a;
        public void Init(double tauSeconds, double updatesPerSecond) { m_a = tauSeconds <= 0 ? 1.0 : 1.0 - Math.Exp(-1.0 / (tauSeconds * updatesPerSecond)); }
        public void Step(double target)
        {
            Prev = Cur;
            Cur += m_a * (target - Cur);
            if (Math.Abs(Cur) < 1e-12 && Math.Abs(target) < 1e-12) Cur = 0.0;
        }
        public void Snap(double v) { Prev = v; Cur = v; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double At(double u) => Prev + u * (Cur - Prev);
        public bool Silent => Prev == 0.0 && Cur == 0.0;
    }

    /// <summary>
    /// Stereo chorus (14.3: 14 and 19 ms, depth 0.6 ms): per-sample LFO (0.17 / 0.23 Hz, incommensurate, 1.1-1.5 cents) from the
    /// sine table and 4-point Hermite reads (R5 3.4: per-callback modulation zippers at -24 dB, per-sample Hermite at -79 dB).
    /// </summary>
    public sealed class StereoChorus
    {
        const int Size = 16384, Mask = Size - 1;      // 85 ms at 192 kHz
        readonly double[] m_l = new double[Size], m_r = new double[Size];
        int m_w;
        double m_phL, m_phR;
        readonly double m_incL, m_incR, m_dL, m_dR, m_depth;
        public double Dry = 1.0, Wet = 0.7;
        /// <summary>Width of the wet taps: 1 = left tap to the left, right tap to the right (the 14.3 chorus); below 1 each side gets
        /// (1 + w) / 2 of its own tap and (1 - w) / 2 of the other's, so the two combs (14 / 19 ms) no longer pull L and R apart in
        /// phase (fix wave C51: the garden pad was anti-correlated, -0.62 at 200-300 Hz). Use WetFor(width) to keep the wet power.</summary>
        public double Width = 1.0;

        /// <summary>The wet gain that keeps the average wet power of Wet 0.7 at full width for a narrower width.</summary>
        public static double WetFor(double width, double wet = 0.7)
        {
            double a = 0.5 * (1.0 + width), b = 0.5 * (1.0 - width);
            return wet / Math.Sqrt(a * a + b * b);
        }

        public StereoChorus(double fs, double delayLms = 14.0, double delayRms = 19.0, double depthMs = 0.6, double rateL = 0.17, double rateR = 0.23)
        {
            m_dL = delayLms * 1e-3 * fs; m_dR = delayRms * 1e-3 * fs; m_depth = depthMs * 1e-3 * fs;
            m_incL = rateL / fs; m_incR = rateR / fs; m_phR = 0.25;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Hermite(double[] b, double pos)
        {
            double fl = Math.Floor(pos); int i = (int)fl; double f = pos - fl;
            double xm1 = b[(i - 1) & Mask], x0 = b[i & Mask], x1 = b[(i + 1) & Mask], x2 = b[(i + 2) & Mask];
            double c1 = 0.5 * (x1 - xm1), c2 = xm1 - 2.5 * x0 + 2.0 * x1 - 0.5 * x2, c3 = 0.5 * (x2 - xm1) + 1.5 * (x0 - x1);
            return ((c3 * f + c2) * f + c1) * f + x0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(ref double l, ref double r)
        {
            int w = m_w;
            m_l[w] = l; m_r[w] = r;
            double dl = m_dL + m_depth * SineTable.Lookup(m_phL);
            double dr = m_dR + m_depth * SineTable.Lookup(m_phR);
            m_phL += m_incL; if (m_phL >= 1.0) m_phL -= 1.0;
            m_phR += m_incR; if (m_phR >= 1.0) m_phR -= 1.0;
            double tl = Hermite(m_l, w + Size - dl), tr = Hermite(m_r, w + Size - dr);
            double wa = 0.5 * (1.0 + Width), wb = 0.5 * (1.0 - Width);
            l = Dry * l + Wet * (wa * tl + wb * tr); r = Dry * r + Wet * (wa * tr + wb * tl);
            m_w = (w + 1) & Mask;
        }

        public void Clear() { Array.Clear(m_l, 0, Size); Array.Clear(m_r, 0, Size); }
    }

    /// <summary>
    /// Look-ahead limiter (The Ring's DspLimiter, Signalsmith design) with the ITU-R BS.1770-4 Annex 2 true-peak detector (R5 5):
    /// required gain with a tanh knee from the threshold to the ceiling, moving minimum over L = 5 ms, instant attack, exponential
    /// release (80 ms) never above the minimum, box average of length L; the detector looks at the four 4x polyphase outputs plus
    /// the sample (6 samples of extra latency). Below the knee it is a pure delay. Buffers sized for 192 kHz up front.
    /// </summary>
    public sealed class TruePeakLimiter
    {
        const int Cap = 1024;
        int m_len;
        double m_thrDb, m_ceilDb, m_thrLin, m_rel;
        readonly double[] m_dl = new double[Cap], m_dr = new double[Cap], m_qv = new double[Cap], m_box = new double[Cap];
        readonly long[] m_qi = new long[Cap];
        int m_wp, m_qh, m_qn, m_bp;
        double m_boxSum, m_gRel = 1.0;
        long m_n;
        /// <summary>Lowest gain applied since the last reset of this field (diagnostics: the mix should never engage it).</summary>
        public double MinGain = 1.0;
        public static readonly int[] Tp0 = { 14, 90, -161, 272, -487, 1125, 7964, -838, 390, -218, 122, -68 };
        public static readonly int[] Tp1 = { -239, 240, -424, 730, -1364, 3810, 6388, -1641, 832, -477, 271, -155 };
        readonly double[] m_tpH = new double[48], m_tpL = new double[16], m_tpR = new double[16];
        int m_tpPos;
        bool m_tp;

        public int LatencySamples => m_len - 1 + (m_tp ? 6 : 0);

        public void Configure(double fs, double thresholdDb, double ceilingDb, double lookaheadMs, double releaseMs, bool truePeak = true)
        {
            m_tp = truePeak;
            FillTaps(m_tpH);
            Array.Clear(m_tpL, 0, 16); Array.Clear(m_tpR, 0, 16);
            m_tpPos = 0;
            m_len = Math.Max(2, Math.Min(Cap, (int)Math.Round(lookaheadMs * 1e-3 * fs)));
            m_thrDb = thresholdDb; m_ceilDb = ceilingDb; m_thrLin = Math.Pow(10.0, thresholdDb / 20.0);
            m_rel = 1.0 - Math.Exp(-1.0 / (releaseMs * 1e-3 * fs));
            for (int i = 0; i < Cap; i++) { m_dl[i] = 0.0; m_dr[i] = 0.0; m_box[i] = 1.0; m_qv[i] = 1.0; m_qi[i] = 0; }
            m_wp = m_qh = m_qn = m_bp = 0; m_boxSum = m_len; m_gRel = 1.0; m_n = 0; MinGain = 1.0;
        }

        /// <summary>The 48 ITU taps (phase 0, 1, then 1 and 0 reversed) divided by 8192.</summary>
        public static void FillTaps(double[] h)
        {
            for (int j = 0; j < 12; j++)
            {
                h[j] = Tp0[j] / 8192.0; h[12 + j] = Tp1[j] / 8192.0;
                h[24 + j] = Tp1[11 - j] / 8192.0; h[36 + j] = Tp0[11 - j] / 8192.0;
            }
        }

        double Required(double p)
        {
            if (p <= m_thrLin) return 1.0;
            double pDb = 20.0 * Math.Log10(p), range = m_ceilDb - m_thrDb;
            return Math.Pow(10.0, (m_thrDb + range * Math.Tanh((pDb - m_thrDb) / range) - pDb) / 20.0);
        }

        public void Process(ref double l, ref double r)
        {
            if (!m_tp) { double al = l < 0 ? -l : l, ar = r < 0 ? -r : r; ProcessWithDetector(al > ar ? al : ar, ref l, ref r); return; }
            int p = m_tpPos;
            m_tpL[p] = l; m_tpR[p] = r;
            double m = 0.0;
            for (int ph = 0; ph < 4; ph++)
            {
                double a = 0.0, b = 0.0; int o = 12 * ph;
                for (int j = 0; j < 12; j++) { int k = (p - j) & 15; a += m_tpH[o + j] * m_tpL[k]; b += m_tpH[o + j] * m_tpR[k]; }
                if (a < 0) a = -a; if (b < 0) b = -b; if (a > m) m = a; if (b > m) m = b;
            }
            int d = (p - 6) & 15;
            double dl = m_tpL[d], dr = m_tpR[d];
            double sl = dl < 0 ? -dl : dl, sr = dr < 0 ? -dr : dr;
            if (sl > m) m = sl; if (sr > m) m = sr;
            m_tpPos = (p + 1) & 15;
            ProcessWithDetector(m, ref dl, ref dr);
            l = dl; r = dr;
        }

        void ProcessWithDetector(double detector, ref double l, ref double r)
        {
            int L = m_len;
            double req = Required(detector);
            while (m_qn > 0 && m_qi[m_qh] <= m_n - L) { if (++m_qh == L) m_qh = 0; m_qn--; }
            while (m_qn > 0) { int last = m_qh + m_qn - 1; if (last >= L) last -= L; if (m_qv[last] >= req) m_qn--; else break; }
            int t = m_qh + m_qn; if (t >= L) t -= L;
            m_qv[t] = req; m_qi[t] = m_n; m_qn++;
            double m = m_qv[m_qh];
            m_gRel = m < m_gRel ? m : m_gRel + (m - m_gRel) * m_rel;
            m_boxSum += m_gRel - m_box[m_bp]; m_box[m_bp] = m_gRel; if (++m_bp == L) m_bp = 0;
            if ((m_n & 0xFFFF) == 0) { double s = 0; for (int i = 0; i < L; i++) s += m_box[i]; m_boxSum = s; }   // drift guard
            double g = m_boxSum / L;
            if (g < MinGain) MinGain = g;
            m_dl[m_wp] = l; m_dr[m_wp] = r; if (++m_wp == L) m_wp = 0;
            l = m_dl[m_wp] * g; r = m_dr[m_wp] * g;
            m_n++;
        }
    }

    /// <summary>ITU-R BS.1770-4 Annex 2 true-peak meter (4x, the same 48 taps; the sample peak is included), streaming, per channel.</summary>
    public sealed class TruePeakMeter
    {
        readonly double[] m_h = new double[48];
        readonly double[] m_x0 = new double[16], m_x1 = new double[16];
        int m_pos;
        public double Peak;
        public TruePeakMeter() { TruePeakLimiter.FillTaps(m_h); }
        public void Reset() { Array.Clear(m_x0, 0, 16); Array.Clear(m_x1, 0, 16); m_pos = 0; Peak = 0; }
        public double PeakDb => 20.0 * Math.Log10(Math.Max(Peak, 1e-12));

        public void Push(double l, double r)
        {
            int p = m_pos;
            m_x0[p] = l; m_x1[p] = r;
            double m = Math.Max(Math.Abs(l), Math.Abs(r));
            for (int ph = 0; ph < 4; ph++)
            {
                double a = 0, b = 0; int o = 12 * ph;
                for (int j = 0; j < 12; j++) { int k = (p - j) & 15; a += m_h[o + j] * m_x0[k]; b += m_h[o + j] * m_x1[k]; }
                a = Math.Abs(a); b = Math.Abs(b);
                if (a > m) m = a; if (b > m) m = b;
            }
            if (m > Peak) Peak = m;
            m_pos = (p + 1) & 15;
        }
    }

    /// <summary>Incoherent finite line source (R5 4.6): I ~ (atan(x2 / d) - atan(x1 / d)) / d; the amplitude relative to the abeam
    /// peak of the same vehicle at dRef, plus the subtended angle (brightness and width cues).</summary>
    public static class LineSource
    {
        public static double Angle(double x1, double x2, double d) => Math.Atan(x2 / d) - Math.Atan(x1 / d);
        public static double Gain(double x1, double x2, double d, double length, double dRef)
        {
            if (x2 <= x1) return 0.0;
            double a = Angle(x1, x2, d), aRef = 2.0 * Math.Atan(0.5 * length / dRef);
            return Math.Sqrt(Math.Max(0.0, (a / d) / (aRef / dRef)));
        }
    }
}
