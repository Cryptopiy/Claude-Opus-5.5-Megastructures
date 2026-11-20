// RainBed: the R layer of MASTER_PROMPT 14.3 (pink noise high-passed at 800 Hz, gust gain 0.6 + 0.4 smooth-random at 0.1 Hz)
// plus the near-field patter of docs/research/R5 4.1 (heavy-tailed impulses placed by a hash of the absolute sample index,
// coloured by the surface: concrete and awnings in the alley, stone on the boulevard, the Edge and the plaza, the steel deck
// and the glass tubes of the bridge (metal pings), leaves in the garden). The rain density rho of 14.2.4 (or -rain X) sets
// the patter rate; the layer level is the 14.3 column, scaled by the power ratio rho / rho_schedule when -rain overrides
// it (0 = off). Unit RMS at gain 1 by construction: the bed by the impulse energy of pink -> HP 800 -> the master DC
// blocker, the patter analytically (impulse train through band-passes of known noise bandwidth), the gust by its measured
// mean square. ScoreFrame is the control-rate context every layer receives. Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    /// <summary>Control-rate context of the score (one per 32-sample block at an absolute sample index).</summary>
    public struct ScoreFrame
    {
        public long N;               // absolute sample index of the block start
        public double Fs, InvFs;     // sample rate
        public int Block;            // samples per control block (32)
        public double T;             // DSP time n / fs (phases, LFOs, vehicle clocks)
        public double SceneT;        // scene time (the rain-gust coupling of beat 4)
        public double S, V;          // progress (m) and walk speed (m/s)
        public double X, Z, Yaw;     // listener position (m) and yaw (deg, 0 = +Z, 90 = +X)
        public double Rho, RhoRatio; // rain density and its ratio to the 14.2.4 schedule (the -rain override)
        public int Beat;             // beat index 0..6 of s

        /// <summary>Equal-power pan position (-1 left .. +1 right) of a source at (x, z) for this listener.</summary>
        public double PanTo(double x, double z)
        {
            double dx = x - X, dz = z - Z;
            if (dx * dx + dz * dz < 1e-6) return 0.0;
            double az = Math.Atan2(dx, dz) - Yaw * NocMath.Deg;
            return Math.Sin(az);
        }

        /// <summary>1 in front of the listener, 0 directly behind (rear sources are darkened a little).</summary>
        public double FrontTo(double x, double z)
        {
            double dx = x - X, dz = z - Z;
            if (dx * dx + dz * dz < 1e-6) return 1.0;
            double az = Math.Atan2(dx, dz) - Yaw * NocMath.Deg;
            return 0.5 + 0.5 * Math.Cos(az);
        }
    }

    /// <summary>Impulse patter: hashed impulses (amplitude U^e, e = 3 for rain, 1.5 for the softer leaf rain) at `rate` per second, split between a band-pass (Q 1.2) and three
    /// high-Q "ping" modes (metal, glass), stereo by a hashed pan. Unit RMS at gain 1 analytically.</summary>
    public struct Patter
    {
        SvfD m_bpL, m_bpR, m_p0L, m_p0R, m_p1L, m_p1R, m_p2L, m_p2R;
        double m_rate, m_share, m_norm, m_pingNorm, m_bandHz, m_pingHz;
        readonly ulong m_seed;
        readonly double m_exp, m_ea2;
        public const double BandQ = 1.2, PingT60 = 0.045;
        static readonly double[] PingRatio = { 1.0, 1.37, 1.83 };

        public Patter(ulong seed, double exponent = 3.0) : this() { m_seed = seed; m_exp = exponent; m_ea2 = 1.0 / (2.0 * exponent + 1.0); }

        /// <summary>Control: impulse rate (1/s), band centre (Hz), share of impulses that ping and the ping base frequency.</summary>
        public void Set(double rate, double bandHz, double pingShare, double pingHz, double fs)
        {
            m_rate = Math.Max(0.0, rate);
            m_share = NocMath.Clamp01(pingShare);
            if (Math.Abs(bandHz - m_bandHz) > 0.5) { m_bpL.Set(bandHz, BandQ, fs); m_bpR.CopyCoefficients(in m_bpL); m_bandHz = bandHz; }
            if (Math.Abs(pingHz - m_pingHz) > 0.5)
            {
                m_p0L.Set(pingHz * PingRatio[0], Q(pingHz * PingRatio[0]), fs); m_p0R.CopyCoefficients(in m_p0L);
                m_p1L.Set(pingHz * PingRatio[1], Q(pingHz * PingRatio[1]), fs); m_p1R.CopyCoefficients(in m_p1L);
                m_p2L.Set(pingHz * PingRatio[2], Q(pingHz * PingRatio[2]), fs); m_p2R.CopyCoefficients(in m_p2L);
                m_pingHz = pingHz;
            }
            // power of an impulse train (probability p per sample, E[a^2] = E[U^2e] = 1 / (2e + 1)) through a band-pass of peak 1:
            // sigma^2 pi fc / (Q fs); the pan halves the mean-over-channels power (x 2)
            double p = m_rate / fs;
            double pBand = p * (1.0 - m_share) * m_ea2 * Math.PI * m_bandHz / (BandQ * fs) * 0.5;
            double pPing = 0.0;
            for (int k = 0; k < 3; k++) { double f = m_pingHz * PingRatio[k]; pPing += p * m_share / 3.0 * m_ea2 * Math.PI * f / (Q(f) * fs) * 0.5; }
            double tot = pBand + pPing;
            m_norm = tot > 1e-30 ? 1.0 / Math.Sqrt(tot) : 0.0;
            m_pingNorm = m_norm;
        }

        static double Q(double f) => Math.PI * f * PingT60 / 6.907755278982137;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(long n, double fs, out double l, out double r)
        {
            double il = 0, ir = 0, ql = 0, qr = 0;
            int mode = -1;
            if (m_rate > 0.0 && NocAudio.Hash01(m_seed, (ulong)n) < m_rate / fs)
            {
                double u = NocAudio.Hash01(m_seed ^ 0xA11UL, (ulong)n);
                double a = m_exp == 3.0 ? u * u * u : Math.Pow(u, m_exp);
                double pan = NocAudio.Hash01(m_seed ^ 0xFA4UL, (ulong)n);
                double gl = Math.Sqrt(1.0 - pan), gr = Math.Sqrt(pan);
                if (NocAudio.Hash01(m_seed ^ 0x9E7UL, (ulong)n) < m_share)
                {
                    mode = (int)(3.0 * NocAudio.Hash01(m_seed ^ 0x3D1UL, (ulong)n));
                    ql = a * gl; qr = a * gr;
                }
                else { il = a * gl; ir = a * gr; }
            }
            l = m_norm * m_bpL.BandNorm(il); r = m_norm * m_bpR.BandNorm(ir);
            double x0l = mode == 0 ? ql : 0.0, x0r = mode == 0 ? qr : 0.0;
            double x1l = mode == 1 ? ql : 0.0, x1r = mode == 1 ? qr : 0.0;
            double x2l = mode == 2 ? ql : 0.0, x2r = mode == 2 ? qr : 0.0;
            if (m_share > 0.0)
            {
                l += m_pingNorm * (m_p0L.BandNorm(x0l) + m_p1L.BandNorm(x1l) + m_p2L.BandNorm(x2l));
                r += m_pingNorm * (m_p0R.BandNorm(x0r) + m_p1R.BandNorm(x1r) + m_p2R.BandNorm(x2r));
            }
        }

        public void Flush() { m_bpL.Flush(); m_bpR.Flush(); m_p0L.Flush(); m_p0R.Flush(); m_p1L.Flush(); m_p1R.Flush(); m_p2L.Flush(); m_p2R.Flush(); }
    }

    public sealed class RainBed
    {
        /// <summary>Share of the layer's power in the near-field patter (R5 4.1: about 20 %).</summary>
        public const double PatterShare = 0.20;
        /// <summary>Patter impulses per second at rho 1 (R5 4.1).</summary>
        public const double PatterRate = 900.0;
        public const double HighPassHz = 800.0, GustBase = 0.6, GustDepth = 0.4, GustRate = 0.1;

        readonly double m_fs;
        NoiseD m_nl, m_nr;
        PinkD m_pl, m_pr;
        SvfD m_hl, m_hr;
        Patter m_pat;
        readonly ulong m_gustSeed;
        readonly double m_bedNorm, m_gustNorm;
        GainRamp m_gust, m_bed, m_patGain;

        public RainBed(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            m_nl = NoiseD.Stream(seed, 0x1201u); m_nr = NoiseD.Stream(seed, 0x1202u);
            m_hl.Set(HighPassHz, 0.7071, fs); m_hr.CopyCoefficients(in m_hl);
            m_pat = new Patter(DspHash.Mix(seed ^ 0xBAD0UL));
            m_gustSeed = DspHash.Mix(seed ^ 0x5EEDUL);
            m_bedNorm = BedNorm(fs);
            m_gustNorm = 1.0 / Math.Sqrt(GustMeanSquare(m_gustSeed));
            m_gust.Init(0.0, controlRate); m_bed.Init(0.05, controlRate); m_patGain.Init(0.05, controlRate);
        }

        /// <summary>Unit-RMS normaliser of pink (Kellet) -> HP 800 Hz (Q 0.707) -> DC 20 Hz, from the impulse energy (white variance 1/3).</summary>
        public static double BedNorm(double fs)
        {
            var pink = new PinkD(); var hp = new SvfD(); hp.Set(HighPassHz, 0.7071, fs); var dc = new DcBlockD(); dc.Init(NocturneConstants.DcBlockHz, fs);
            double e = 0;
            int n = (int)(1.5 * fs);
            for (int i = 0; i < n; i++) { double y = dc.Tick(hp.High(pink.Next(i == 0 ? 1.0 : 0.0))); e += y * y; }
            return 1.0 / Math.Sqrt(e / 3.0);
        }

        /// <summary>Gust gain 0.6 + 0.4 n(0.1 t) (a pure function of t, quintic value noise).</summary>
        public static double Gust(ulong seed, double t) => GustBase + GustDepth * NocAudio.SmoothRandom01(seed, GustRate * t);

        static double GustMeanSquare(ulong seed)
        {
            double m = 0; int k = 0;
            for (double t = 0; t < 20000.0; t += 0.7, k++) { double g = Gust(seed, t); m += g * g; }
            return m / Math.Max(1, k);
        }

        public void Control(in ScoreFrame f)
        {
            NocturneScore.RainSurface(f.S, out double band, out double pingShare, out double pingHz);
            m_pat.Set(PatterRate * Math.Max(0.0, f.Rho), band, pingShare, pingHz, m_fs);
            m_gust.Step(Gust(m_gustSeed, f.T) * m_gustNorm);
            double on = f.Rho > 1e-4 ? 1.0 : 0.0;
            m_bed.Step(Math.Sqrt(1.0 - PatterShare) * m_bedNorm * on);
            m_patGain.Step(Math.Sqrt(PatterShare) * on);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, long n, out double l, out double r)
        {
            double g = m_gust.At(u), gb = g * m_bed.At(u), gp = g * m_patGain.At(u);
            l = gb * m_hl.High(m_pl.Next(m_nl.Next()));
            r = gb * m_hr.High(m_pr.Next(m_nr.Next()));
            m_pat.Tick(n, m_fs, out double pl, out double pr);
            l += gp * pl; r += gp * pr;
        }

        public void Flush() { m_pl.Flush(); m_pr.Flush(); m_hl.Flush(); m_hr.Flush(); m_pat.Flush(); }
    }
}
