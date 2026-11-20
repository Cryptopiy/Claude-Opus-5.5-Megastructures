// NocDsp: the Unity-free DSP building blocks of Nocturne (IMPORT_PLAN 8.7): Thread's ThreadDsp.cs blocks (lines 1-334:
// XorShift32, PinkKellet, SvfTpt (Simper/Cytomic trapezoidal SVF, stable up to 20 kHz), DcBlocker, SineTable, FdnReverb8) and
// The Ring's AudioDsp.cs DspHash, DspBrown, DspSmooth, DspSmoothRandom and the look-ahead DspLimiter (ceiling -1.5 dBFS, 5 ms
// look-ahead, 80 ms release for 14.3). Thread's score class ThreadDsp (drone / pad / hum / stars, lines 335-1079) is game
// content and is not imported. No UnityEngine, no allocation after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    /// <summary>The audio constants the imported blocks need (Thread's AudioSpec subset; the score constants live in NocturneConstants).</summary>
    public static class AudioSpec
    {
        public const int SineTableSize = 4096;
        /// <summary>FDN parameter cross-blend time (s).</summary>
        public const double ReverbBlendSeconds = 8.0;
        public const double ReverbSize = 2.2;
        public const double ReverbMaxPreDelay = 0.25;
        public const double SmoothTau = 0.02;
        public static double Db(double db) => Math.Pow(10.0, db / 20.0);
    }

    // ==================================================================================== building blocks

    /// <summary>xorshift32 (Marsaglia). The state is never 0.</summary>
    public struct XorShift32
    {
        uint m_s;
        public XorShift32(uint seed) { m_s = seed != 0u ? seed : 0x9E3779B9u; }

        /// <summary>Independent stream for (seed, stream id): the seed is hashed (murmur3 finaliser) so streams decorrelate.</summary>
        public static XorShift32 Stream(int seed, uint stream)
        {
            unchecked
            {
                uint x = (uint)seed * 0x9E3779B1u ^ (stream * 0x85EBCA77u + 0x27D4EB2Fu);
                x ^= x >> 16; x *= 0x7FEB352Du; x ^= x >> 15; x *= 0x846CA68Bu; x ^= x >> 16;
                return new XorShift32(x);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt() { uint x = m_s; x ^= x << 13; x ^= x >> 17; x ^= x << 5; m_s = x; return x; }
        /// <summary>Uniform in [-1, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextBipolar() { return unchecked((int)NextUInt()) * (1.0f / 2147483648.0f); }
        /// <summary>Uniform in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Next01() { return (NextUInt() >> 8) * (1.0f / 16777216.0f); }
        public double NextDouble01() { return (NextUInt() >> 1) * (1.0 / 2147483648.0); }
        public uint State { get { return m_s; } }
    }

    /// <summary>Paul Kellet's refined pinking filter (linear; input uniform white in [-1, 1)), output scaled to about unit RMS.</summary>
    public struct PinkKellet
    {
        public float b0, b1, b2, b3, b4, b5, b6;
        public const float UnitRms = 1f / 1.755f;   // R5 section 3: RMS 1.755 for uniform white in [-1, 1)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Next(float white)
        {
            b0 = 0.99886f * b0 + white * 0.0555179f;
            b1 = 0.99332f * b1 + white * 0.0750759f;
            b2 = 0.96900f * b2 + white * 0.1538520f;
            b3 = 0.86650f * b3 + white * 0.3104856f;
            b4 = 0.55000f * b4 + white * 0.5329522f;
            b5 = -0.7616f * b5 - white * 0.0168980f;
            float pink = b0 + b1 + b2 + b3 + b4 + b5 + b6 + white * 0.5362f;
            b6 = white * 0.115926f;
            return pink * UnitRms;
        }
        public void Flush()
        {
            if (Math.Abs(b0) < 1e-20f) b0 = 0f; if (Math.Abs(b1) < 1e-20f) b1 = 0f; if (Math.Abs(b2) < 1e-20f) b2 = 0f;
            if (Math.Abs(b3) < 1e-20f) b3 = 0f; if (Math.Abs(b4) < 1e-20f) b4 = 0f; if (Math.Abs(b5) < 1e-20f) b5 = 0f;
        }
    }

    /// <summary>
    /// Simper/Cytomic linear trapezoidal state-variable filter (stable for every fc below Nyquist).
    /// low/band/high outputs; BandNorm = k * band has a 0 dB peak at fc.
    /// </summary>
    public struct SvfTpt
    {
        public float ic1, ic2, a1, a2, a3, k;

        public void Set(double fc, double q, double fs)
        {
            double f = Math.Min(Math.Max(fc, 1.0), 0.45 * fs);
            double g = Math.Tan(Math.PI * f / fs);
            double kk = 1.0 / Math.Max(q, 0.05);
            double A1 = 1.0 / (1.0 + g * (g + kk));
            k = (float)kk; a1 = (float)A1; a2 = (float)(g * A1); a3 = (float)(g * g * A1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Low(float x)
        {
            float v3 = x - ic2;
            float v1 = a1 * ic1 + a2 * v3;
            float v2 = ic2 + a2 * ic1 + a3 * v3;
            ic1 = 2f * v1 - ic1;
            ic2 = 2f * v2 - ic2;
            return v2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float BandNorm(float x)
        {
            float v3 = x - ic2;
            float v1 = a1 * ic1 + a2 * v3;
            float v2 = ic2 + a2 * ic1 + a3 * v3;
            ic1 = 2f * v1 - ic1;
            ic2 = 2f * v2 - ic2;
            return k * v1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float High(float x)
        {
            float v3 = x - ic2;
            float v1 = a1 * ic1 + a2 * v3;
            float v2 = ic2 + a2 * ic1 + a3 * v3;
            ic1 = 2f * v1 - ic1;
            ic2 = 2f * v2 - ic2;
            return x - k * v1 - v2;
        }

        public void Clear() { ic1 = 0f; ic2 = 0f; }
        public void Flush() { if (Math.Abs(ic1) < 1e-20f) ic1 = 0f; if (Math.Abs(ic2) < 1e-20f) ic2 = 0f; }

        /// <summary>|H| of the low-pass at frequency f (bilinear image of 1 / (s^2 + s/Q + 1)).</summary>
        public static double LowMagnitude(double f, double fc, double q, double fs)
        {
            double fcc = Math.Min(Math.Max(fc, 1.0), 0.45 * fs);
            double g = Math.Tan(Math.PI * fcc / fs);
            double w = Math.Tan(Math.PI * Math.Min(f, 0.4999 * fs) / fs) / g;
            double re = 1.0 - w * w, im = w / Math.Max(q, 0.05);
            return 1.0 / Math.Sqrt(re * re + im * im);
        }
    }

    /// <summary>DC blocker y = x - x1 + R y1, R = exp(-2 pi fc / fs).</summary>
    public struct DcBlocker
    {
        public float x1, y1, r;
        public void Init(double fc, double fs) { r = (float)Math.Exp(-2.0 * Math.PI * fc / fs); x1 = 0f; y1 = 0f; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Process(float x) { float y = x - x1 + r * y1; x1 = x; y1 = y; return y; }
        public void Clear() { x1 = 0f; y1 = 0f; }

        /// <summary>|H| at frequency f for the blocker at fc.</summary>
        public static double Magnitude(double f, double fc, double fs)
        {
            double R = Math.Exp(-2.0 * Math.PI * fc / fs), w = 2.0 * Math.PI * f / fs, c = Math.Cos(w);
            double num = 2.0 - 2.0 * c, den = 1.0 - 2.0 * R * c + R * R;
            return den > 1e-30 ? Math.Sqrt(num / den) : 1.0;
        }
    }

    /// <summary>4096-entry sine table with a guard sample; Lookup(phase in [0,1)) with linear interpolation.</summary>
    public static class SineTable
    {
        public const int Size = AudioSpec.SineTableSize;
        public static readonly float[] T = Build();
        static float[] Build()
        {
            var t = new float[Size + 1];
            for (int i = 0; i <= Size; i++) t[i] = (float)Math.Sin(2.0 * Math.PI * i / Size);
            return t;
        }
        /// <summary>Call on the main thread (Awake) so the static initialiser never runs on the audio thread.</summary>
        public static void Warm() { Lookup(0.25); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lookup(double phase01)
        {
            double p = phase01 * Size;
            int i = (int)p;
            float fr = (float)(p - i);
            i &= Size - 1;
            return T[i] + fr * (T[i + 1] - T[i]);
        }
    }

    /// <summary>
    /// 8-line feedback delay network (R5 section 4): base primes 1427..3539 samples at 48 kHz times a fixed size
    /// factor (re-primed once), Householder feedback (y_i = x_i - 0.25 sum), unity-DC one-pole damping per line,
    /// per-line gains g_i = 10^(-3 M_i / (RT60 fs)), stereo pre-delay with a 1 s two-tap crossfade (no read
    /// pointer ever moves), outputs from two Hadamard rows. RT60, damping and wet are smoothed in double at
    /// control rate (every sub-block) with tau = 8 s / 4.6 (99 percent in 8 s).
    /// </summary>
    public sealed class FdnReverb8
    {
        const int N = 8;
        static readonly int[] BasePrimes48k = { 1427, 1783, 1973, 2099, 2579, 2879, 3221, 3539 };
        readonly float[] m_buf;
        readonly int[] m_off = new int[N], m_len = new int[N], m_pos = new int[N];
        readonly float[] m_lp = new float[N], m_gain = new float[N], m_v = new float[N];
        readonly float[] m_pre;
        readonly int m_preFrames;
        readonly double m_fs, m_meanLen;
        int m_preW, m_tapA, m_tapB, m_tPre;
        float m_xf = 1f;
        readonly float m_xfStep;
        double m_rt, m_damp, m_wet;              // smoothed (double: an 8 s float one-pole stalls short of its target)
        double m_tRt = 1.0, m_tDamp = 7000.0, m_tWet;
        float m_dampC, m_wetNorm;
        /// <summary>Parameter cross-blend time (s): Thread's 8 s by default; the score uses 0.5 s because its s-keyed 40 m
        /// blends are already smooth (R5 1).</summary>
        public double BlendSeconds = AudioSpec.ReverbBlendSeconds;

        public FdnReverb8(double sampleRate, double sizeFactor, double maxPreDelaySeconds)
        {
            m_fs = sampleRate;
            int total = 0;
            for (int i = 0; i < N; i++)
            {
                int l = NearestPrime((int)Math.Round(BasePrimes48k[i] * sizeFactor * m_fs / 48000.0));
                m_len[i] = l; m_off[i] = total; total += l;
            }
            m_buf = new float[total];
            m_meanLen = total / (double)N;
            m_preFrames = (int)(maxPreDelaySeconds * m_fs) + 2;
            m_pre = new float[2 * m_preFrames];
            m_xfStep = (float)(1.0 / m_fs);
            m_rt = m_tRt; m_damp = m_tDamp; m_wet = m_tWet;
            UpdateCoefficients();
        }

        public void SetTargets(double rt60, double preDelayMs, double wetDb, double dampHz)
        {
            m_tRt = Math.Max(0.05, rt60);
            m_tDamp = Math.Max(200.0, dampHz);
            m_tWet = AudioSpec.Db(wetDb);
            int p = Math.Min(Math.Max(0, (int)(preDelayMs * 0.001 * m_fs)), m_preFrames - 2);
            if (p != m_tPre && m_xf >= 1f) { m_tapA = m_tapB; m_tapB = p; m_xf = 0f; m_tPre = p; }
        }

        /// <summary>Jump to the targets (offline renders that start mid-walk, device rebuilds).</summary>
        public void Snap()
        {
            m_rt = m_tRt; m_damp = m_tDamp; m_wet = m_tWet;
            m_tapA = m_tapB = m_tPre; m_xf = 1f;
            UpdateCoefficients();
        }

        /// <summary>Control-rate update for a sub-block of n frames.</summary>
        public void Control(int n)
        {
            double dRt = m_tRt - m_rt, dDamp = m_tDamp - m_damp, dWet = m_tWet - m_wet;
            if (Math.Abs(dRt) < 1e-9 * m_rt && Math.Abs(dDamp) < 1e-9 * m_damp && Math.Abs(dWet) <= 1e-9 * Math.Max(m_wet, 1e-6))
                return;   // converged: the coefficients are already current
            double a = 1.0 - Math.Exp(-n * 4.6 / (Math.Max(0.01, BlendSeconds) * m_fs));
            m_rt += a * dRt;
            m_damp += a * dDamp;
            m_wet += a * dWet;
            UpdateCoefficients();
        }

        void UpdateCoefficients()
        {
            for (int i = 0; i < N; i++)
                m_gain[i] = (float)Math.Min(Math.Pow(10.0, -3.0 * m_len[i] / (m_rt * m_fs)), 0.9995);
            m_dampC = m_damp >= 0.45 * m_fs ? 1f : (float)(1.0 - Math.Exp(-2.0 * Math.PI * m_damp / m_fs));
            double tau = m_rt * m_fs / 6.9078;
            m_wetNorm = (float)(m_wet * Math.Sqrt(2.0 * N * m_meanLen / Math.Max(tau, 1.0)));   // tail energy ~ wet^2
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(float inL, float inR, out float outL, out float outR)
        {
            int pw = m_preW;
            m_pre[2 * pw] = inL; m_pre[2 * pw + 1] = inR;
            int ra = pw - m_tapA; if (ra < 0) ra += m_preFrames;
            int rb = pw - m_tapB; if (rb < 0) rb += m_preFrames;
            float dL, dR;
            if (m_xf < 1f)
            {
                float xf = m_xf + m_xfStep; if (xf > 1f) xf = 1f; m_xf = xf;
                float aL = m_pre[2 * ra], aR = m_pre[2 * ra + 1];
                dL = aL + xf * (m_pre[2 * rb] - aL);
                dR = aR + xf * (m_pre[2 * rb + 1] - aR);
            }
            else { dL = m_pre[2 * rb]; dR = m_pre[2 * rb + 1]; }
            if (++pw >= m_preFrames) pw = 0;
            m_preW = pw;

            float[] buf = m_buf; float[] lp = m_lp; float[] v = m_v; float[] gain = m_gain;
            float dc = m_dampC, sum = 0f;
            for (int i = 0; i < N; i++)
            {
                float y = buf[m_off[i] + m_pos[i]];
                float l = lp[i] + dc * (y - lp[i]);
                lp[i] = l;
                float g = gain[i] * l;
                v[i] = g; sum += g;
            }
            float h = 0.25f * sum;
            float hl = 0.5f * dL, hr = 0.5f * dR;
            for (int i = 0; i < N; i++)
            {
                float x = v[i] - h + ((i & 1) == 0 ? hl : hr);
                if (x < 1e-15f && x > -1e-15f) x = 0f;     // no denormals in the loop
                int p = m_pos[i];
                buf[m_off[i] + p] = x;
                if (++p >= m_len[i]) p = 0;
                m_pos[i] = p;
            }
            float c = 0.35355339f * m_wetNorm;
            outL = c * (v[0] + v[1] - v[2] - v[3] + v[4] + v[5] - v[6] - v[7]);
            outR = c * (v[0] - v[1] - v[2] + v[3] + v[4] - v[5] - v[6] + v[7]);
        }

        public void Clear()
        {
            Array.Clear(m_buf, 0, m_buf.Length);
            Array.Clear(m_pre, 0, m_pre.Length);
            for (int i = 0; i < N; i++) { m_lp[i] = 0f; m_v[i] = 0f; }
        }

        public void Flush() { for (int i = 0; i < N; i++) if (Math.Abs(m_lp[i]) < 1e-20f) m_lp[i] = 0f; }

        public int TotalDelayFrames { get { return m_buf.Length; } }

        static int NearestPrime(int n)
        {
            for (int d = 0; ; d++)
            {
                if (IsPrime(n - d)) return n - d;
                if (IsPrime(n + d)) return n + d;
            }
        }
        static bool IsPrime(int n)
        {
            if (n < 2) return false;
            for (int k = 2; k * k <= n; k++) if (n % k == 0) return false;
            return true;
        }
    }

    // ==================================================================================== the score


    // ==================================================================================== The Ring's blocks (AudioDsp.cs)

    /// <summary>Seed mixing (splitmix64 finaliser) so every noise source gets an independent, deterministic stream.</summary>
    public static class DspHash
    {
        public static ulong Mix(ulong z)
        {
            unchecked
            {
                z += 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        public static uint Seed(uint a, uint b, uint c = 0u)
        {
            unchecked
            {
                ulong z = Mix(((ulong)a << 32) ^ b);
                z = Mix(z ^ c);
                uint s = (uint)(z ^ (z >> 32));
                return s == 0u ? 0x9E3779B9u : s;
            }
        }

        /// <summary>Uniform [0, 1) from (seed, index).</summary>
        public static double U01(uint seed, long i)
        {
            unchecked
            {
                ulong z = Mix((ulong)i * 0x9E3779B97F4A7C15UL + seed * 0xD1B54A32D192ED03UL);
                return (z >> 11) * (1.0 / 9007199254740992.0);
            }
        }
    }


    /// <summary>Leaky-integrator brown noise (leak 0.02, noisehack): -6 dB/oct above about 150 Hz at 48 kHz.</summary>
    public struct DspBrown
    {
        double _last;
        public double Next(double white) { _last = (_last + 0.02 * white) / 1.02; return _last * 3.5; }
        public void Reset() { _last = 0.0; }
    }

    /// <summary>
    /// Linear trapezoidal (TPT / zero-delay-feedback) state-variable filter, Simper's optimised form
    /// (cytomic.com SvfLinearTrapOptimised2.pdf; equivalent to Zavalishin's TPT SVF with k = 2R = 1/Q).
    /// Stable under per-sample coefficient modulation. BandNorm has 0 dB gain at fc.
    /// </summary>

    /// <summary>One-pole smoother y += a (target - y), a = 1 - exp(-1 / (tau fs)).</summary>
    public struct DspSmooth
    {
        public double V;
        double _a;
        public void Set(double tau, double fs) { _a = tau <= 0.0 ? 1.0 : 1.0 - Math.Exp(-1.0 / (tau * fs)); }
        public void SetPerStep(double tau, double stepsPerSecond) { _a = tau <= 0.0 ? 1.0 : 1.0 - Math.Exp(-1.0 / (tau * stepsPerSecond)); }
        public double Next(double target) { V += _a * (target - V); return V; }
    }

    /// <summary>
    /// Smooth random (interpolated value noise, 3 octaves 1, 1/2, 1/4, quintic), a pure function of time so the main
    /// thread (RingState.Gust, grass sway) and the audio thread compute the identical W1 gust (research 05 section 6.6).
    /// </summary>
    public static class DspSmoothRandom
    {
        static double Value(uint seed, double t)
        {
            double fl = Math.Floor(t);
            long i = (long)fl;
            double f = t - fl;
            double u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
            double a = DspHash.U01(seed, i) * 2.0 - 1.0, b = DspHash.U01(seed, i + 1) * 2.0 - 1.0;
            return a + (b - a) * u;
        }

        /// <summary>[-1, 1] with about rateHz independent values per second.</summary>
        public static double Get(uint seed, double timeSec, double rateHz)
        {
            double t = timeSec * rateHz;
            unchecked
            {
                return (Value(seed, t) + 0.5 * Value(seed + 0x51u, 2.0 * t + 0.37) + 0.25 * Value(seed + 0xA3u, 4.0 * t + 0.71)) / 1.75;
            }
        }

        public static double Get01(uint seed, double timeSec, double rateHz) => 0.5 + 0.5 * Get(seed, timeSec, rateHz);
    }


    /// <summary>
    /// Stereo-linked look-ahead peak limiter (Signalsmith design): required gain per sample with a tanh knee between
    /// threshold and ceiling, moving minimum over the look-ahead window, instant attack / exponential release that
    /// never exceeds the moving minimum, box smoothing of the same length, audio delayed by L - 1 samples. For sample
    /// values this guarantees |y| &lt;= ceiling; the -1.2 dBFS internal ceiling keeps the true peak under -1 dBTP.
    /// Buffers are sized for 192 kHz up front so a sample-rate change never allocates on the audio thread.
    /// </summary>
    public sealed class DspLimiter
    {
        const int Cap = 1024;   // 5 ms at 192 kHz = 960
        int _len;
        double _thrDb, _ceilDb, _thrLin, _relCoef;
        readonly double[] _dl = new double[Cap], _dr = new double[Cap];
        readonly double[] _qv = new double[Cap];
        readonly long[] _qi = new long[Cap];
        readonly double[] _box = new double[Cap];
        int _wp, _qh, _qn, _bp;
        double _boxSum, _gRel = 1.0;
        long _n;
        public double MinGain = 1.0;          // diagnostics: lowest gain applied since the last reset of this field

        public int LatencySamples => _len - 1;

        public void Configure(double fs, double thresholdDb, double ceilingDb, double lookaheadMs, double releaseMs)
        {
            _len = (int)Math.Round(lookaheadMs * 0.001 * fs);
            if (_len < 2) _len = 2; else if (_len > Cap) _len = Cap;
            _thrDb = thresholdDb; _ceilDb = ceilingDb; _thrLin = Math.Pow(10.0, thresholdDb / 20.0);
            _relCoef = 1.0 - Math.Exp(-1.0 / (releaseMs * 0.001 * fs));
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < Cap; i++) { _dl[i] = 0.0; _dr[i] = 0.0; _box[i] = 1.0; _qv[i] = 1.0; _qi[i] = 0; }
            _wp = 0; _qh = 0; _qn = 0; _bp = 0; _boxSum = _len; _gRel = 1.0; _n = 0;
        }

        double Required(double p)
        {
            if (p <= _thrLin) return 1.0;
            double pDb = 20.0 * Math.Log10(p), range = _ceilDb - _thrDb;
            double outDb = _thrDb + range * Math.Tanh((pDb - _thrDb) / range);
            return Math.Pow(10.0, (outDb - pDb) / 20.0);
        }

        public void Process(ref double l, ref double r)
        {
            int L = _len;
            double al = l < 0 ? -l : l, ar = r < 0 ? -r : r;
            double req = Required(al > ar ? al : ar);
            while (_qn > 0 && _qi[_qh] <= _n - L) { _qh++; if (_qh == L) _qh = 0; _qn--; }
            while (_qn > 0)
            {
                int last = _qh + _qn - 1; if (last >= L) last -= L;
                if (_qv[last] >= req) _qn--; else break;
            }
            int t = _qh + _qn; if (t >= L) t -= L;
            _qv[t] = req; _qi[t] = _n; _qn++;
            double m = _qv[_qh];
            _gRel = m < _gRel ? m : _gRel + (m - _gRel) * _relCoef;
            _boxSum += _gRel - _box[_bp]; _box[_bp] = _gRel; _bp++; if (_bp == L) _bp = 0;
            if ((_n & 0xFFFF) == 0) { double s = 0; for (int i = 0; i < L; i++) s += _box[i]; _boxSum = s; }
            double g = _boxSum / L;
            if (g < MinGain) MinGain = g;
            _dl[_wp] = l; _dr[_wp] = r; _wp++; if (_wp == L) _wp = 0;
            l = _dl[_wp] * g; r = _dr[_wp] * g;
            _n++;
        }
    }
}
