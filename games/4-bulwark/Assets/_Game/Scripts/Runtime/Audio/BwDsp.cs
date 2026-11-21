// BwDsp: the Unity-free audio engine of Bulwark (MASTER_PROMPT 14.3; lessons 3.2 #14; RESEARCH C). Building blocks
// imported from Thread's ThreadDsp (lines 1-332: XorShift32 streams, Kellet pink noise, the Simper/Cytomic SVF, the DC
// blocker, the 4096-entry sine table, the 8-line Householder FDN reverb with Jot damping and pre-delay crossfade) with
// the FDN extended by explicit delay arrays and a per-transition blend time, plus a brown-noise block and The Ring's
// stereo-linked look-ahead limiter (DspLimiter). Thread's score class (L1-L8) is NOT imported: the score is BulwarkScore
// (layers W/S/B/F/T/P/V/C as pure functions of s, t and speed) feeding BwAudioParams.
// The engine BwDsp (bottom of this file) runs the master chain: score bed -> footsteps mixed inside the DSP (so they pass
// through the reverb and the limiter and the player dump matches the offline render) -> DC block 20 Hz -> FDN reverb ->
// master gain (UI fade x pause duck) -> + impact bus -> look-ahead limiter. The impact bus (14.3 B boom voices, 14.11 "the
// first wave impact heard at 0.5 s" in the black) is a post-fader bus with its own envelope and its own room reverb: the bed
// follows the picture fade, the impacts do not. Rules: no UnityEngine, no allocation in Process, double phase accumulators,
// seeded PRNG streams, every parameter smoothed per sample.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure.Audio
{
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
        public const int Size = BwAudioSpec.SineTableSize;
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
        double m_blend = BwAudioSpec.ReverbBlendSeconds;   // cross-blend time of the current transition (s)
        float m_dampC, m_wetNorm;

        public FdnReverb8(double sampleRate, double sizeFactor, double maxPreDelaySeconds)
            : this(sampleRate, ScaledDelays(sampleRate, sizeFactor), maxPreDelaySeconds) { }

        /// <summary>Delay lengths of the 8 lines for a room size factor (base primes 1427..3539 at 48 kHz, scaled, nearest prime).</summary>
        public static int[] ScaledDelays(double sampleRate, double sizeFactor)
        {
            var d = new int[N];
            for (int i = 0; i < N; i++) d[i] = NearestPrime((int)Math.Round(BasePrimes48k[i] * sizeFactor * sampleRate / 48000.0));
            return d;
        }

        /// <summary>Explicit delay lengths (frames) of the 8 lines (RESEARCH C 4: derive them from the room).</summary>
        public FdnReverb8(double sampleRate, int[] delays, double maxPreDelaySeconds)
        {
            m_fs = sampleRate;
            int total = 0;
            for (int i = 0; i < N; i++)
            {
                int l = Math.Max(16, delays[i]);
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

        /// <summary>Blend time of the next parameter change (>= 2 s through doorways, 0.6 s at hard edges, 14.6).</summary>
        public double BlendSeconds { get { return m_blend; } set { m_blend = Math.Max(0.01, value); } }

        public void SetTargets(double rt60, double preDelayMs, double wetDb, double dampHz)
        {
            m_tRt = Math.Max(0.05, rt60);
            m_tDamp = Math.Max(200.0, dampHz);
            m_tWet = BwAudioSpec.Db(wetDb);
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
            double a = 1.0 - Math.Exp(-n * 4.6 / (m_blend * m_fs));
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
    /// <summary>Brown (red) noise: leaky integrator of white noise, normalised to unit RMS (1/f^2 below the leak corner).</summary>
    public struct BrownNoise
    {
        public float y;
        public float leak;        // 1 - 2 pi fc / fs
        public float norm;        // unit RMS
        public void Init(double cornerHz, double fs)
        {
            leak = (float)(1.0 - 2.0 * Math.PI * Math.Max(1.0, cornerHz) / fs);
            // variance of y = 1/3 (uniform white) * s^2 / (1 - leak^2) with input scale s = (1 - leak)
            double s = 1.0 - leak;
            norm = (float)(1.0 / Math.Sqrt(Math.Max(1e-12, s * s / 3.0 / (1.0 - (double)leak * leak))));
            y = 0f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Next(float white)
        {
            y = leak * y + (1f - leak) * white;
            return y * norm;
        }
    }

    /// <summary>
    /// Stereo-linked look-ahead peak limiter (Signalsmith design): required gain per sample with a tanh knee between
    /// threshold and ceiling, moving minimum over the look-ahead window, instant attack / exponential release that
    /// never exceeds the moving minimum, box smoothing of the same length, audio delayed by L - 1 samples. For sample
    /// values this guarantees |y| &lt;= ceiling; Bulwark: ceiling -1.42 dBFS, threshold -4, look-ahead 16 ms, release 250 ms (BwAudioSpec; RESEARCH C 5),
    /// after the reverb, so the true peak stays at or below -1 dBFS. Imported from The Ring's AudioDsp (C-24).
    /// Buffers are sized for 192 kHz up front so a sample-rate change never allocates on the audio thread.
    /// </summary>
    public sealed class DspLimiter
    {
        const int Cap = 4096;   // 16 ms look-ahead at 192 kHz = 3072 frames
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

    // ==================================================================================== parameters and the engine

    /// <summary>
    /// One frame of score parameters, published by the main thread (AudioDirector) through the BwSynth mailbox and read by
    /// the audio thread. Produced by BulwarkScore.Evaluate(s, t, speed) plus the UI / pause gains. Linear gains; levels are
    /// smoothed per sample inside the engine, so a torn read only mixes two consecutive frames' targets.
    /// </summary>
    public struct BwAudioParams
    {
        public double s, speed;
        /// <summary>Master envelope 0..1 of the bed (UiDirector.FadeAudio: intro fade-in with the picture, ending fade-out).</summary>
        public float master;
        /// <summary>Impact bus envelope 0..1 (UiDirector.FadeImpacts): post-fader, open from t 0 of the intro so the 0.5 s wave
        /// impact plays over black (14.11); closes with the ending and the restart / quit fade.</summary>
        public float impactBus;
        /// <summary>Pause duck gain (1 = none; -12 dB over 0.4 s while paused).</summary>
        public float duck;
        /// <summary>1 while the pause menu is open (fix wave S4-5): the kraken bus and the ride fade out over 0.3 s, then their
        /// voices, delays and the ride clock freeze until the pause ends (they resume where they stopped).</summary>
        public float paused;
        /// <summary>Layer gains of 14.3 (linear; 1 = 0 dBFS RMS, BwBeds): w = W wind, sea = S sea, bed = T room tone (brown LP
        /// 160 Hz), falls = F fall roar, tonal = T ballast sines (beat 5), pad = P pressure, voice = the city hum of the ledge,
        /// conduit = C conduit. 0 = silent.</summary>
        public float w, sea, bed, falls, tonal, pad, voice, conduit;
        /// <summary>Wind band centre (Hz) and gustiness 0..1; wind low-pass (Hz, 0 = open; beat 5 400 Hz); pans -1..1 (east = +).</summary>
        public float windHz, gust, windLp, windPan, seaPan;
        /// <summary>Sea low-pass (Hz: 700, beat 1 muffled 250).</summary>
        public float seaLp;
        /// <summary>Gain of the impact bus per s (14.3 B levels per beat, S1: no booms in s 100-108); multiplies impactBus.</summary>
        public float impactTrim;
        /// <summary>The ending ride (RideSound; BulwarkScore from EndingRide): ride layer mix 0..1, phase (EndingRidePhase as int),
        /// cabin speed (m/s), signed acceleration (m/s^2), altitude above the lowland (m), distance from the face (m).</summary>
        public float rideMix, rideSpeed, rideAccel, rideAlt, rideDist;
        public int ridePhase;
        /// <summary>The "ear" low-pass of the world (Hz; >= 18000 = open): closes as the cabin sinks, pops open at the foot.</summary>
        public float earHz;
        /// <summary>The foot of the dam (the lantern platform): ambience gain 0..1 (wind -48, crickets -56) and the outlet jets (linear).</summary>
        public float foot, jets;
        /// <summary>RMS level of the beds at this s (dBFS, power sum of the layer gains; BulwarkScore): the kraken audibility floor.</summary>
        public float bedDb;
        /// <summary>Reverb targets: T60 (s), pre-delay (ms), wet (dB), damping (Hz), blend time of this change (s).</summary>
        public float rt60, preDelayMs, wetDb, dampHz, blendSeconds;
        /// <summary>Listener pose for the kraken spatialiser: world eye position (m), view yaw (deg, 0 = +Z, 90 = +X), and 1 while
        /// the listener is inside the concrete (gallery, tower, tunnel: the kraken is heard through the wall).</summary>
        public float lx, ly, lz, lyaw, indoors;

        public static BwAudioParams Silent()
        {
            return new BwAudioParams { master = 0f, duck = 1f, bedDb = -120f, windHz = 300f, seaLp = 700f, impactTrim = 1f, rideAlt = 6000f, earHz = 20000f, rt60 = 1.0f, preDelayMs = 10f, wetDb = -60f, dampHz = 6000f, blendSeconds = (float)BwAudioSpec.ReverbBlendSeconds };
        }
    }

    /// <summary>A footstep request from the main thread (surface, pan, gain), passed to the audio thread lock-free.</summary>
    public struct StepEvent
    {
        public int surface;
        public float pan, gain;
    }

    /// <summary>A wave impact (14.3 B boom, SeaState.Impacts): strength 0.3-1.0, pan -1..1, subOnly = the beat 6 31 Hz sub.</summary>
    public struct ImpactEvent
    {
        public float strength, pan;
        public bool subOnly;
    }

    /// <summary>Single-producer single-consumer ring of events (main thread writes, audio thread reads; no locks).</summary>
    public class SpscRing<T> where T : struct
    {
        readonly T[] m_buf = new T[64];
        int m_write, m_read;   // monotonically increasing; Volatile for the cross-thread hand-over

        public bool Push(in T e)
        {
            int w = System.Threading.Volatile.Read(ref m_write), r = System.Threading.Volatile.Read(ref m_read);
            if (w - r >= m_buf.Length) return false;   // full: drop (never block)
            m_buf[w & (m_buf.Length - 1)] = e;
            System.Threading.Volatile.Write(ref m_write, w + 1);
            return true;
        }

        public bool Pop(out T e)
        {
            int r = System.Threading.Volatile.Read(ref m_read), w = System.Threading.Volatile.Read(ref m_write);
            if (r == w) { e = default; return false; }
            e = m_buf[r & (m_buf.Length - 1)];
            System.Threading.Volatile.Write(ref m_read, r + 1);
            return true;
        }
    }

    public sealed class StepEventRing : SpscRing<StepEvent> { }
    public sealed class ImpactEventRing : SpscRing<ImpactEvent> { }

    /// <summary>
    /// One wave impact of 14.3 (layer B): sines 41 and 63 Hz decaying with T60 1.6 s (beat 6: only a 31 Hz sub), a 3 ms noise
    /// click through a 180 Hz low-pass, and a brown rumble decaying with T60 0.9 s; 4 ms raised-cosine attack (no click from the
    /// sines). Allocation-free; the voice frees itself once every part is below -80 dB.
    /// </summary>
    public struct BoomVoice
    {
        public bool active, sub;
        public int n;                       // samples since the start
        public float gl, gr;                // strength x boom gain x equal-power pan
        public double ph1, ph2;             // phases in cycles
        public float envTone, envRumble;
        public SvfTpt click, rumbleLp;
        public BrownNoise brown;
    }

    /// <summary>
    /// The master chain (allocation-free Process): score bed (reference generators for the W wind and B bed layers; the audio
    /// owner adds the other BulwarkScore generators here) -> footsteps (FootstepBank clips mixed inside the DSP) -> DC block
    /// -> FDN reverb -> master gain (UI fade x pause duck) -> + impact bus (boom voices -> DC block -> own FDN with the same
    /// room targets -> impact envelope x pause duck) -> look-ahead limiter. Deterministic per seed.
    /// </summary>
    public sealed class BwDsp
    {
        public const int Block = 32;
        const int MaxVoices = 8;
        const int MaxBooms = BwAudioSpec.MaxBoomVoices;

        readonly double m_fs;
        XorShift32 m_rngStep, m_rngBoom;
        readonly BwBeds m_beds;
        readonly RideSound m_ride;
        SvfTpt m_earL, m_earR;
        float m_earHz = -1f;
        double m_earLog;
        DcBlocker m_dcL, m_dcR, m_idcL, m_idcR;
        readonly FdnReverb8 m_reverb, m_impactReverb;
        readonly DspLimiter m_limiter = new DspLimiter();
        readonly FootstepBank m_bank;
        readonly StepEventRing m_steps = new StepEventRing();
        readonly ImpactEventRing m_impacts = new ImpactEventRing();
        readonly int[] m_vClip = new int[MaxVoices], m_vPos = new int[MaxVoices], m_vSurf = new int[MaxVoices];
        readonly float[] m_vGl = new float[MaxVoices], m_vGr = new float[MaxVoices];
        int m_variantCounter;

        // impact (boom) voices, 14.3 layer B
        readonly BoomVoice[] m_boom = new BoomVoice[MaxBooms];
        // the kraken bus (KrakenVoice.cs, ADDENDUM_A1 section 2)
        readonly KrakenBus m_kraken;
        SvfTpt m_clickProto, m_rumbleProto;
        BrownNoise m_brownProto;
        readonly float m_boomGain, m_boomDecay, m_rumbleDecay, m_clickGain, m_rumbleGain;
        readonly int m_boomAttack, m_clickLen;
        readonly double m_dph1, m_dph2, m_dphSub;

        // smoothed gains (per sample one-pole)
        float m_master, m_duck = 1f, m_impactG;
        float m_pauseG = 1f;
        readonly float m_pauseStep;
        readonly float m_smooth;
        public int NanResets { get; private set; }
        /// <summary>Impacts started so far (diagnostics, tests).</summary>
        public int ImpactsStarted { get; private set; }

        public double SampleRate => m_fs;
        public StepEventRing Steps => m_steps;
        public DspLimiter Limiter => m_limiter;
        public KrakenBus Kraken => m_kraken;

        public BwDsp(int sampleRate, int seed)
        {
            m_fs = sampleRate > 0 ? sampleRate : BwAudioSpec.SampleRate;
            SineTable.Warm();
            m_beds = new BwBeds(m_fs, seed);
            m_ride = new RideSound(m_fs, seed);
            m_rngStep = XorShift32.Stream(seed, 0x5374u);
            m_rngBoom = XorShift32.Stream(seed, 0xB00Du);
            m_dcL.Init(BwAudioSpec.DcBlockHz, m_fs); m_dcR.Init(BwAudioSpec.DcBlockHz, m_fs);
            m_idcL.Init(BwAudioSpec.DcBlockHz, m_fs); m_idcR.Init(BwAudioSpec.DcBlockHz, m_fs);
            m_reverb = new FdnReverb8(m_fs, BwAudioSpec.ReverbSizeHall, BwAudioSpec.ReverbMaxPreDelay);
            m_impactReverb = new FdnReverb8(m_fs, BwAudioSpec.ReverbSizeHall, BwAudioSpec.ReverbMaxPreDelay);
            m_limiter.Configure(m_fs, BwAudioSpec.LimiterThresholdDb, BwAudioSpec.LimiterCeilingDb, BwAudioSpec.LimiterLookaheadMs, BwAudioSpec.LimiterReleaseMs);
            m_bank = new FootstepBank((int)m_fs, seed);
            m_smooth = (float)(1.0 - Math.Exp(-1.0 / (BwAudioSpec.SmoothTau * m_fs)));
            m_pauseStep = (float)(1.0 / (0.3 * m_fs));
            for (int i = 0; i < MaxVoices; i++) m_vClip[i] = -1;

            // boom recipe (14.3 B): tone peak BoomPeakDb at strength 1 over the summed sine weights
            m_boomGain = (float)(BwAudioSpec.Db(BwAudioSpec.BoomPeakDb) / (1.0 + BwAudioSpec.BoomTone2Weight));
            m_boomDecay = (float)BwAudioSpec.T60Mul(BwAudioSpec.BoomDecayT60, m_fs);
            m_rumbleDecay = (float)BwAudioSpec.T60Mul(BwAudioSpec.BoomRumbleT60, m_fs);
            m_clickGain = (float)BwAudioSpec.BoomClickGain;
            m_rumbleGain = (float)BwAudioSpec.BoomRumbleGain;
            m_boomAttack = Math.Max(1, (int)(BwAudioSpec.BoomAttackMs * 0.001 * m_fs));
            m_clickLen = Math.Max(2, (int)(BwAudioSpec.BoomClickMs * 0.001 * m_fs));
            m_dph1 = BwAudioSpec.BoomHz1 / m_fs; m_dph2 = BwAudioSpec.BoomHz2 / m_fs; m_dphSub = BwAudioSpec.BoomSubHz / m_fs;
            m_clickProto.Set(BwAudioSpec.BoomClickLpHz, 0.7071, m_fs);
            m_rumbleProto.Set(BwAudioSpec.BoomRumbleLpHz, 0.7071, m_fs);
            m_brownProto.Init(20.0, m_fs);
            m_kraken = new KrakenBus(m_fs, seed);
        }

        /// <summary>Main thread: queue a kraken event (KrakenCalls -> AudioDirector, or the offline renderer).</summary>
        public void TriggerKraken(in KrakenEvent e) { m_kraken.Trigger(e); }

        /// <summary>Main thread: a one-shot cue of the ending ride (doors, coupling, motor start).</summary>
        public void TriggerRide(RideCue c) { m_ride.Cue(c); }

        /// <summary>Main thread: queue a footstep (mixed by the audio thread at the next block).</summary>
        public void TriggerStep(FootSurface surface, float pan, float gain)
        {
            m_steps.Push(new StepEvent { surface = (int)surface, pan = pan, gain = gain });
        }

        /// <summary>Main thread: queue a wave impact (boom on the impact bus; started at the next block).</summary>
        public void TriggerImpact(float strength, float pan, bool subOnly = false)
        {
            m_impacts.Push(new ImpactEvent { strength = strength, pan = pan, subOnly = subOnly });
        }

        /// <summary>Jump the smoothed state to the targets (device rebuild, an offline render that starts mid-walk).</summary>
        public void SnapTo(in BwAudioParams p)
        {
            m_beds.Snap(in p);
            m_master = p.master; m_duck = p.duck; m_impactG = p.impactBus * p.impactTrim;
            m_reverb.BlendSeconds = Math.Max(0.01, p.blendSeconds);
            m_reverb.SetTargets(p.rt60, p.preDelayMs, p.wetDb, p.dampHz);
            m_reverb.Snap();
            m_impactReverb.BlendSeconds = Math.Max(0.01, p.blendSeconds);
            m_impactReverb.SetTargets(p.rt60, p.preDelayMs, p.wetDb, p.dampHz);
            m_impactReverb.Snap();
        }

        /// <summary>Fills an interleaved buffer (any channel count; channels beyond 2 get silence). Allocation-free.</summary>
        public void Process(float[] data, int channels, in BwAudioParams p)
        {
            Array.Clear(data, 0, data.Length);
            if (channels <= 0) return;
            int frames = data.Length / channels;
            m_reverb.BlendSeconds = Math.Max(0.01, p.blendSeconds);
            m_reverb.SetTargets(p.rt60, p.preDelayMs, p.wetDb, p.dampHz);
            m_impactReverb.BlendSeconds = Math.Max(0.01, p.blendSeconds);
            m_impactReverb.SetTargets(p.rt60, p.preDelayMs, p.wetDb, p.dampHz);
            while (m_steps.Pop(out StepEvent e)) StartVoice(e);
            while (m_impacts.Pop(out ImpactEvent ie)) StartBoom(ie);

            for (int start = 0; start < frames; start += Block)
            {
                int n = Math.Min(Block, frames - start);
                m_reverb.Control(n);
                m_impactReverb.Control(n);
                m_kraken.Control(n, p.lx, p.ly, p.lz, p.lyaw, p.indoors > 0.5f, p.bedDb);
                m_beds.Control(in p);
                m_ride.Control();
                // the "ear" low-pass follows the score in the log domain with a 0.13 s time constant (the 0.4 s ear pop at the foot)
                double earT = Math.Log(p.earHz > 0f ? Math.Min(p.earHz, 20000f) : 20000.0);
                if (m_earLog <= 0.0) m_earLog = earT;
                m_earLog += (earT - m_earLog) * (1.0 - Math.Exp(-n / (0.13 * m_fs)));
                float ear = (float)Math.Exp(m_earLog);
                if (Math.Abs(ear - m_earHz) > 0.002f * ear) { m_earHz = ear; m_earL.Set(Math.Min(ear, 0.45 * m_fs), 0.7071, m_fs); m_earR.Set(Math.Min(ear, 0.45 * m_fs), 0.7071, m_fs); }
                for (int k = 0; k < n; k++)
                {
                    m_master += m_smooth * (p.master - m_master);
                    m_duck += m_smooth * (p.duck - m_duck);
                    m_impactG += m_smooth * (p.impactBus * p.impactTrim - m_impactG);
                    m_pauseG = p.paused > 0.5f ? Math.Max(0f, m_pauseG - m_pauseStep) : Math.Min(1f, m_pauseG + m_pauseStep);
                    bool live = m_pauseG > 0f;   // paused and faded: the kraken voices and the ride clock stand still

                    // ---- score beds (14.3 layers, BwBeds) and the ending ride (RideSound), through the "ear" low-pass
                    float l = 0f, r = 0f;
                    m_beds.Tick(in p, ref l, ref r);
                    if (live)
                    {
                        float rl = 0f, rr = 0f;
                        m_ride.Tick(in p, ref rl, ref rr);
                        l += m_pauseG * rl; r += m_pauseG * rr;
                    }
                    if (m_earHz < 17000f) { l = m_earL.Low(l); r = m_earR.Low(r); }

                    // ---- footsteps (inside the DSP: through the reverb and the limiter)
                    for (int v = 0; v < MaxVoices; v++)
                    {
                        int c = m_vClip[v];
                        if (c < 0) continue;
                        float[] clip = m_bank.Clip(m_vSurf[v], c);
                        int pos = m_vPos[v];
                        if (pos >= clip.Length) { m_vClip[v] = -1; continue; }
                        float x = clip[pos];
                        l += x * m_vGl[v]; r += x * m_vGr[v];
                        m_vPos[v] = pos + 1;
                    }

                    l = m_dcL.Process(l); r = m_dcR.Process(r);
                    m_reverb.Tick(l, r, out float wl, out float wr);
                    // the kraken ducks the beds, the ride, the footsteps and the impacts (2.6: 6-10 dB, 1.5 s in, 3 s out)
                    double kl = 0.0, kr = 0.0;
                    if (live) { m_kraken.Tick(m_master, out kl, out kr); kl *= m_pauseG; kr *= m_pauseG; }
                    float kd = m_kraken.DuckGain;
                    double g = m_master * m_duck;
                    double ol = (l + wl) * g * kd, or = (r + wr) * g * kd;
                    ol += kl * m_duck; or += kr * m_duck;

                    // ---- impact bus: AFTER the master fade, with its own envelope (the 0.5 s intro impact sounds over black)
                    float il = 0f, ir = 0f;
                    for (int b = 0; b < MaxBooms; b++) if (m_boom[b].active) TickBoom(ref m_boom[b], ref il, ref ir);
                    il = m_idcL.Process(il); ir = m_idcR.Process(ir);
                    m_impactReverb.Tick(il, ir, out float iwl, out float iwr);
                    double gi = m_impactG * m_duck * kd;
                    ol += (il + iwl) * gi; or += (ir + iwr) * gi;

                    if (double.IsNaN(ol) || double.IsNaN(or) || double.IsInfinity(ol) || double.IsInfinity(or))
                    {
                        ol = 0; or = 0; NanResets++;
                        m_reverb.Clear(); m_dcL.Clear(); m_dcR.Clear(); m_beds.Clear(); m_earL.Clear(); m_earR.Clear();
                        m_impactReverb.Clear(); m_idcL.Clear(); m_idcR.Clear(); m_kraken.Clear();
                        for (int b = 0; b < MaxBooms; b++) m_boom[b].active = false;
                    }
                    m_limiter.Process(ref ol, ref or);
                    int i = (start + k) * channels;
                    data[i] = (float)ol;
                    if (channels > 1) data[i + 1] = (float)or;
                }
                m_beds.Flush(); m_ride.Flush(); m_earL.Flush(); m_earR.Flush(); m_reverb.Flush(); m_impactReverb.Flush();
                m_kraken.Flush();
                for (int b = 0; b < MaxBooms; b++) if (m_boom[b].active) { m_boom[b].click.Flush(); m_boom[b].rumbleLp.Flush(); }
            }
        }

        /// <summary>One sample of one boom voice (14.3 B): tone x T60 1.6 s with a 4 ms attack, 3 ms click (LP 180 Hz), rumble x T60 0.9 s.</summary>
        void TickBoom(ref BoomVoice v, ref float l, ref float r)
        {
            int n = v.n;
            float att = n < m_boomAttack ? 0.5f - 0.5f * (float)Math.Cos(Math.PI * n / m_boomAttack) : 1f;
            float tone = v.sub ? SineTable.Lookup(v.ph1)
                               : SineTable.Lookup(v.ph1) + (float)BwAudioSpec.BoomTone2Weight * SineTable.Lookup(v.ph2);
            v.ph1 += v.sub ? m_dphSub : m_dph1; if (v.ph1 >= 1.0) v.ph1 -= 1.0;
            v.ph2 += m_dph2; if (v.ph2 >= 1.0) v.ph2 -= 1.0;
            float x = tone * v.envTone * att;
            v.envTone *= m_boomDecay;
            float white = m_rngBoom.NextBipolar();
            float burst = n < m_clickLen ? white * (0.5f - 0.5f * (float)Math.Cos(2.0 * Math.PI * n / (m_clickLen - 1))) : 0f;
            x += m_clickGain * v.click.Low(burst);
            if (!v.sub) x += m_rumbleGain * v.envRumble * att * v.rumbleLp.Low(v.brown.Next(white));
            v.envRumble *= m_rumbleDecay;
            l += x * v.gl; r += x * v.gr;
            v.n = n + 1;
            if (v.envTone < 1e-4f && v.envRumble < 1e-4f) v.active = false;   // both below -80 dB
        }

        void StartBoom(in ImpactEvent e)
        {
            int slot = -1;
            float quietest = float.MaxValue;
            for (int b = 0; b < MaxBooms; b++)
            {
                if (!m_boom[b].active) { slot = b; break; }
                if (m_boom[b].envTone < quietest) { quietest = m_boom[b].envTone; slot = b; }   // steal the most decayed voice
            }
            float strength = Math.Max(0f, Math.Min(1.5f, e.strength));
            float pan = Math.Max(-1f, Math.Min(1f, e.pan));
            float g = strength * m_boomGain;
            ref BoomVoice v = ref m_boom[slot];
            v.active = true; v.sub = e.subOnly; v.n = 0;
            v.ph1 = 0.0; v.ph2 = 0.0; v.envTone = 1f; v.envRumble = 1f;
            v.gl = g * (float)Math.Sqrt(0.5 * (1.0 - pan));
            v.gr = g * (float)Math.Sqrt(0.5 * (1.0 + pan));
            v.click = m_clickProto; v.click.Clear();
            v.rumbleLp = m_rumbleProto; v.rumbleLp.Clear();
            v.brown = m_brownProto;
            ImpactsStarted++;
        }

        void StartVoice(in StepEvent e)
        {
            int slot = -1;
            for (int v = 0; v < MaxVoices; v++) if (m_vClip[v] < 0) { slot = v; break; }
            if (slot < 0) slot = 0;   // steal the oldest slot (never happens at walking cadence)
            int surf = Math.Max(0, Math.Min(FootstepBank.SurfaceCount - 1, e.surface));
            int variant = (int)(m_rngStep.NextUInt() % (uint)BwAudioSpec.FootstepVariants);
            if (variant == m_variantCounter % BwAudioSpec.FootstepVariants) variant = (variant + 1) % BwAudioSpec.FootstepVariants;   // no immediate repeat
            m_variantCounter = variant;
            double jitterDb = (m_rngStep.Next01() * 2.0 - 1.0) * BwAudioSpec.FootLevelJitterDb;
            float gain = e.gain * (float)BwAudioSpec.Db(jitterDb);
            float pan = Math.Max(-1f, Math.Min(1f, e.pan));
            m_vSurf[slot] = surf;
            m_vClip[slot] = variant;
            m_vPos[slot] = 0;
            m_vGl[slot] = gain * (float)Math.Sqrt(0.5 * (1.0 - pan));
            m_vGr[slot] = gain * (float)Math.Sqrt(0.5 * (1.0 + pan));
        }
    }
}
