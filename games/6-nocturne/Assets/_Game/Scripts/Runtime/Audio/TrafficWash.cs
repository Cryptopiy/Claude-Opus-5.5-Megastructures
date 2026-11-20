// TrafficWash: the T layer of MASTER_PROMPT 14.3 (band-pass 200-900 Hz; docs/research/R5 4.4): the rivers of vehicles on
// the boulevard's ten lanes (14 m/s at heights 60-400 m), the traffic layers of the Gulf and the city's far streets.
// Decorrelated pink -> TPT HP 200 Hz (Q 0.6) -> TPT LP 900 Hz (Q 0.6), a slow level 0.7 + 0.3 smooth-random (0.07 Hz) and
// near pass-bys: a jittered lattice of events (mean interval per beat, 5-14 s) that lift the wash by +2..+4 dB for 4-8 s with
// the low-pass opening 900 -> 1 300 Hz and the image sweeping across (a vehicle passing overhead). Every event is a pure
// function of its index (partition-invariant). Unit RMS at gain 1: the chain's impulse energy (incl. the DC blocker) and the
// measured mean square of the level x bump envelope. S2 (+6 dB) and S6 (-12 dB) are NocturneScore swells.
// WindBed: the W layer (14.3: band-pass 300 and 900 Hz gusts; R5 4.5): pink -> BandNorm 300 Hz (Q 0.8) + 900 Hz (Q 1.4,
// -9 dB) with LFO-wandering centres (+-12 %, 0.05 / 0.07 Hz) that brighten with the gust; gust 0.55 + 0.45 smooth-random
// (0.08 Hz); in beat 4 the gust follows the rain slant of the picture (14.5.4 "rain gusts, slant 0.28 rad, period 17 s",
// NocRainMath.Slope): 0.40 + 0.60 |sin(2 pi t / 17)|^1.5, blended by NocRainMath.Gust(s). Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    public sealed class TrafficWash
    {
        public const double HpHz = 200.0, LpHz = 900.0, LpOpenHz = 1300.0, Q = 0.6;
        static readonly double[] MeanInterval = { 14.0, 6.0, 9.0, 8.0, 14.0, 9.0, 10.0 };
        readonly double m_fs, m_norm;
        NoiseD m_nl, m_nr;
        PinkD m_pl, m_pr;
        SvfD m_hl, m_hr, m_ll, m_lr;
        readonly ulong m_seed;
        GainRamp m_gl, m_gr;
        double m_lp;

        public TrafficWash(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            m_seed = DspHash.Mix(seed ^ 0x7AFFUL);
            m_nl = NoiseD.Stream(seed, 0x7A01u); m_nr = NoiseD.Stream(seed, 0x7A02u);
            m_hl.Set(HpHz, Q, fs); m_hr.CopyCoefficients(in m_hl);
            m_ll.Set(LpHz, Q, fs); m_lr.CopyCoefficients(in m_ll);
            m_lp = LpHz;
            m_norm = ChainNorm(fs) / Math.Sqrt(EnvelopeMeanSquare(m_seed));
            m_gl.Init(0.05, controlRate); m_gr.Init(0.05, controlRate);
        }

        static double ChainNorm(double fs)
        {
            var pink = new PinkD(); var hp = new SvfD(); hp.Set(HpHz, Q, fs); var lp = new SvfD(); lp.Set(LpHz, Q, fs);
            var dc = new DcBlockD(); dc.Init(NocturneConstants.DcBlockHz, fs);
            double e = 0; int n = (int)(1.5 * fs);
            for (int i = 0; i < n; i++) { double y = dc.Tick(lp.Low(hp.High(pink.Next(i == 0 ? 1.0 : 0.0)))); e += y * y; }
            return 1.0 / Math.Sqrt(e / 3.0);
        }

        /// <summary>Pass-by bump of the lattice at time t for mean interval mu: lift (linear, >= 1), opening 0..1 and the sweep -1..1.</summary>
        public static void Bump(ulong seed, double t, double mu, out double lift, out double open, out double sweep)
        {
            lift = 1.0; open = 0.0; sweep = 0.0;
            long k0 = (long)Math.Floor(t / mu) - 1;
            for (long k = Math.Max(0, k0); k <= k0 + 2; k++)
            {
                double tk = mu * (k + 0.5 + 0.8 * (NocAudio.Hash01(seed, (ulong)k) - 0.5));
                double len = 4.0 + 4.0 * NocAudio.Hash01(seed ^ 0x11UL, (ulong)k);
                double x = (t - tk) / (0.5 * len);
                if (x <= -1.0 || x >= 1.0) continue;
                double w = 0.5 + 0.5 * Math.Cos(Math.PI * x);
                double db = 2.0 + 2.0 * NocAudio.Hash01(seed ^ 0x22UL, (ulong)k);
                double l = Math.Pow(10.0, db * w / 20.0);
                if (l > lift) { lift = l; open = w; double dir = NocAudio.Hash01(seed ^ 0x33UL, (ulong)k) < 0.5 ? -1.0 : 1.0; sweep = 0.7 * dir * x; }
            }
        }

        /// <summary>Level 0.7 + 0.3 n(0.07 t) (a pure function of t).</summary>
        public static double Level(ulong seed, double t) => 0.7 + 0.3 * NocAudio.SmoothRandom01(seed ^ 0x4CUL, 0.07 * t);

        static double EnvelopeMeanSquare(ulong seed)
        {
            double m = 0; int k = 0;
            for (double t = 0; t < 6000.0; t += 0.13, k++) { Bump(seed, t, 9.0, out double lift, out _, out _); double g = Level(seed, t) * lift; m += g * g; }
            return m / Math.Max(1, k);
        }

        public void Control(in ScoreFrame f)
        {
            double mu = NocMath.BeatBlend(MeanInterval, f.S);
            Bump(m_seed, f.T, mu, out double lift, out double open, out double sweep);
            double g = m_norm * Level(m_seed, f.T) * lift;
            NocAudio.Pan(sweep * open, out double pl, out double pr);
            m_gl.Step(g * (1.0 + open * (pl - 1.0))); m_gr.Step(g * (1.0 + open * (pr - 1.0)));
            double lp = LpHz + (LpOpenHz - LpHz) * open;
            if (Math.Abs(lp - m_lp) > 2.0) { m_ll.Set(lp, Q, m_fs); m_lr.CopyCoefficients(in m_ll); m_lp = lp; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            l = m_gl.At(u) * m_ll.Low(m_hl.High(m_pl.Next(m_nl.Next())));
            r = m_gr.At(u) * m_lr.Low(m_hr.High(m_pr.Next(m_nr.Next())));
        }

        public void Flush() { m_pl.Flush(); m_pr.Flush(); m_hl.Flush(); m_hr.Flush(); m_ll.Flush(); m_lr.Flush(); }
    }

    public sealed class WindBed
    {
        public const double F1 = 300.0, Q1 = 0.8, F2 = 900.0, Q2 = 1.4, Band2Db = -9.0, LfoDepth = 0.12;
        readonly double m_fs, m_norm, m_b2;
        NoiseD m_nl, m_nr;
        PinkD m_pl, m_pr;
        SvfD m_al, m_ar, m_bl, m_br;
        readonly ulong m_seed;
        GainRamp m_g;
        readonly double m_gustNorm;

        public WindBed(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            m_seed = DspHash.Mix(seed ^ 0x3171DUL);
            m_nl = NoiseD.Stream(seed, 0x3101u); m_nr = NoiseD.Stream(seed, 0x3102u);
            m_b2 = NocAudio.Db(Band2Db);
            // pink (unit RMS) through BandNorm: about pink density x pi fc / (2 Q) per band; measured by the impulse energy
            m_norm = BandsNorm(fs, m_b2);
            m_gustNorm = 1.0 / Math.Sqrt(GustMeanSquare(m_seed));
            m_al.Set(F1, Q1, fs); m_ar.CopyCoefficients(in m_al); m_bl.Set(F2, Q2, fs); m_br.CopyCoefficients(in m_bl);
            m_g.Init(0.0, controlRate);
        }

        static double BandsNorm(double fs, double b2)
        {
            var pa = new PinkD(); var a = new SvfD(); a.Set(F1, Q1, fs); var b = new SvfD(); b.Set(F2, Q2, fs);
            var dc = new DcBlockD(); dc.Init(NocturneConstants.DcBlockHz, fs);
            double e = 0; int n = (int)(1.5 * fs);
            for (int i = 0; i < n; i++) { double x = pa.Next(i == 0 ? 1.0 : 0.0); double y = dc.Tick(a.BandNorm(x) + b2 * b.BandNorm(x)); e += y * y; }
            return 1.0 / Math.Sqrt(e / 3.0);
        }

        /// <summary>Gust 0..1-ish at time t and beat-4 weight g4 (the picture's rain slant period, 17 s).</summary>
        public static double Gust(ulong seed, double t, double sceneT, double g4)
        {
            double a = 0.55 + 0.45 * NocAudio.SmoothRandom01(seed, 0.08 * t);
            double sl = Math.Abs(Math.Sin(NocAudio.TwoPi * sceneT / NocturneConstants.RainGustPeriod));
            double b = 0.40 + 0.60 * sl * Math.Sqrt(sl);
            return a + (b - a) * g4;
        }

        static double GustMeanSquare(ulong seed)
        {
            // both forms have nearly the same mean square; the normaliser uses their average
            double ma = 0, mb = 0; int k = 0;
            for (double t = 0; t < 20000.0; t += 0.61, k++) { double a = Gust(seed, t, t, 0.0), b = Gust(seed, t, t, 1.0); ma += a * a; mb += b * b; }
            return 0.5 * (ma + mb) / Math.Max(1, k);
        }

        public void Control(in ScoreFrame f)
        {
            double g4 = NocturneScore.RainGustWeight(f.S);
            double gust = Gust(m_seed, f.T, f.SceneT, g4);
            m_g.Step(gust * m_gustNorm * m_norm);
            double w1 = Math.Sin(NocAudio.TwoPi * (0.05 * f.T + 0.13)), w2 = Math.Sin(NocAudio.TwoPi * (0.07 * f.T + 0.61));
            double bright = 0.85 + 0.3 * gust;
            m_al.Set(F1 * bright * (1.0 + LfoDepth * w1), Q1, m_fs); m_ar.CopyCoefficients(in m_al);
            m_bl.Set(F2 * bright * (1.0 + LfoDepth * w2), Q2, m_fs); m_br.CopyCoefficients(in m_bl);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            double g = m_g.At(u);
            double xl = m_pl.Next(m_nl.Next()), xr = m_pr.Next(m_nr.Next());
            l = g * (m_al.BandNorm(xl) + m_b2 * m_bl.BandNorm(xl));
            r = g * (m_ar.BandNorm(xr) + m_b2 * m_br.BandNorm(xr));
        }

        public void Flush() { m_pl.Flush(); m_pr.Flush(); m_al.Flush(); m_ar.Flush(); m_bl.Flush(); m_br.Flush(); }
    }
}
