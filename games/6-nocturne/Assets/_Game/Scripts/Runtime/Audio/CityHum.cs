// CityHum: the H layer of MASTER_PROMPT 14.3 (brown noise LP 90 Hz, 50 and 100 Hz partials, neon buzz 2.4-3.6 kHz;
// docs/research/R5 4.3), the distant city. Four parts with per-beat power shares (Rule-P blended, they sum to 1):
//   brown: white -> leaky integrator (5 Hz corner) -> TPT LP 90 Hz, decorrelated L / R;
//   deep : the abyss's voice in beats 3-4 (S4's "deep hum"), an independent brown -> LP 45 Hz;
//   mains: one double 50 Hz master phase with +-0.02 Hz grid wander, 100 Hz at 0 dB, 50 Hz at -6, 150 Hz at -12
//          (transformer magnetostriction hums at twice the line frequency), DC-blocker loss compensated per partial;
//   buzz : harmonics 24-36 of 100 Hz under a Gaussian at 3.0 kHz (sigma 350 Hz), hashed phases, +-15 % flutter at 0.2-0.7 Hz;
//          beat 6 (the wall of signs, H -33) shifts the shares to 40/5/25/30 so the wall buzzes.
// Unit RMS at gain 1 (impulse energy of each noise chain including the 20 Hz master DC blocker; tones analytically).
// Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    public sealed class CityHum
    {
        public const int BuzzPartials = 13;
        public const double BrownLpHz = 90.0, DeepLpHz = 45.0, LeakHz = 5.0;
        // power shares per beat: brown, deep, mains, buzz
        static readonly double[] ShBrown = { 0.65, 0.62, 0.35, 0.40, 0.65, 0.40, 0.62 };
        static readonly double[] ShDeep = { 0.05, 0.08, 0.50, 0.45, 0.10, 0.05, 0.08 };
        static readonly double[] ShMains = { 0.20, 0.20, 0.10, 0.10, 0.15, 0.25, 0.20 };
        static readonly double[] ShBuzz = { 0.10, 0.10, 0.05, 0.05, 0.10, 0.30, 0.10 };

        readonly double m_fs, m_leak, m_brownNorm, m_deepNorm, m_mainsNorm, m_buzzNorm, m_g50, m_g100, m_g150;
        NoiseD m_nl, m_nr, m_ndl, m_ndr;
        double m_il, m_ir, m_idl, m_idr;
        SvfD m_ll, m_lr, m_dl, m_dr;
        double m_ph50;
        readonly double[] m_bAmp = new double[BuzzPartials], m_bPsi = new double[BuzzPartials], m_bFl = new double[BuzzPartials];
        readonly double[] m_flAmp = new double[BuzzPartials];
        GainRamp m_kb, m_kd, m_km, m_kz;
        double m_drift = 1.0;

        public CityHum(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            m_nl = NoiseD.Stream(seed, 0x4801u); m_nr = NoiseD.Stream(seed, 0x4802u);
            m_ndl = NoiseD.Stream(seed, 0x4803u); m_ndr = NoiseD.Stream(seed, 0x4804u);
            m_leak = Math.Exp(-NocAudio.TwoPi * LeakHz / fs);
            m_ll.Set(BrownLpHz, 0.7071, fs); m_lr.CopyCoefficients(in m_ll);
            m_dl.Set(DeepLpHz, 0.7071, fs); m_dr.CopyCoefficients(in m_dl);
            m_brownNorm = BrownNorm(fs, BrownLpHz, m_leak);
            m_deepNorm = BrownNorm(fs, DeepLpHz, m_leak);
            ulong sd = DspHash.Mix(seed ^ 0xB022UL);
            double p = 0;
            for (int k = 0; k < BuzzPartials; k++)
            {
                double f = (24 + k) * 100.0, a = Math.Exp(-0.5 * ((f - 3000.0) / 350.0) * ((f - 3000.0) / 350.0));
                m_bAmp[k] = a; p += 0.5 * a * a * (1.0 + 0.5 * 0.15 * 0.15);
                m_bPsi[k] = NocAudio.Hash01(sd, (ulong)k); m_bFl[k] = 0.2 + 0.5 * NocAudio.Hash01(sd + 1UL, (ulong)k);
            }
            m_buzzNorm = 1.0 / Math.Sqrt(p);
            m_g50 = 0.5 / DcBlockD.Magnitude(50, NocturneConstants.DcBlockHz, fs);
            m_g100 = 1.0 / DcBlockD.Magnitude(100, NocturneConstants.DcBlockHz, fs);
            m_g150 = 0.25 / DcBlockD.Magnitude(150, NocturneConstants.DcBlockHz, fs);
            m_mainsNorm = 1.0 / Math.Sqrt(0.5 * (0.25 + 1.0 + 0.0625));
            m_kb.Init(0.05, controlRate); m_kd.Init(0.05, controlRate); m_km.Init(0.05, controlRate); m_kz.Init(0.05, controlRate);
        }

        /// <summary>Unit-RMS normaliser of white (variance 1/3) -> leaky integrator -> LP fc -> DC 20 Hz (impulse energy).</summary>
        public static double BrownNorm(double fs, double fc, double leak)
        {
            var lp = new SvfD(); lp.Set(fc, 0.7071, fs); var dc = new DcBlockD(); dc.Init(NocturneConstants.DcBlockHz, fs);
            double y = 0, e = 0;
            int n = (int)(4 * fs);
            for (int i = 0; i < n; i++) { y = leak * y + (i == 0 ? 1.0 : 0.0); double o = dc.Tick(lp.Low(y)); e += o * o; }
            return 1.0 / Math.Sqrt(e / 3.0);
        }

        public void Control(in ScoreFrame f)
        {
            double s = f.S;
            m_kb.Step(Math.Sqrt(NocMath.BeatBlend(ShBrown, s)) * m_brownNorm);
            m_kd.Step(Math.Sqrt(NocMath.BeatBlend(ShDeep, s)) * m_deepNorm);
            m_km.Step(Math.Sqrt(NocMath.BeatBlend(ShMains, s)) * m_mainsNorm);
            m_kz.Step(Math.Sqrt(NocMath.BeatBlend(ShBuzz, s)) * m_buzzNorm);
            m_drift = 1.0 + 0.0004 * Math.Sin(NocAudio.TwoPi * 0.013 * f.T);
            for (int k = 0; k < BuzzPartials; k++) m_flAmp[k] = m_bAmp[k] * (1.0 + 0.15 * Math.Sin(NocAudio.TwoPi * (m_bFl[k] * f.T + m_bPsi[k])));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            m_il = m_leak * m_il + m_nl.Next(); m_ir = m_leak * m_ir + m_nr.Next();
            m_idl = m_leak * m_idl + m_ndl.Next(); m_idr = m_leak * m_idr + m_ndr.Next();
            double kb = m_kb.At(u), kd = m_kd.At(u);
            double bl = kb * m_ll.Low(m_il) + kd * m_dl.Low(m_idl);
            double br = kb * m_lr.Low(m_ir) + kd * m_dr.Low(m_idr);
            m_ph50 += 50.0 * m_drift / m_fs; if (m_ph50 >= 1.0) m_ph50 -= 1.0;
            double ph = m_ph50;
            double mains = m_km.At(u) * (m_g50 * SineTable.Lookup(ph) + m_g100 * NocAudio.Sin01(2.0 * ph) + m_g150 * NocAudio.Sin01(3.0 * ph));
            double buzz = 0.0, kz = m_kz.At(u);
            if (kz > 1e-6)
            {
                for (int k = 0; k < BuzzPartials; k++) buzz += m_flAmp[k] * NocAudio.Sin01(2.0 * (24 + k) * ph + m_bPsi[k]);
                buzz *= kz;
            }
            l = bl + mains + buzz; r = br + mains + buzz;
        }

        public void Flush()
        {
            m_ll.Flush(); m_lr.Flush(); m_dl.Flush(); m_dr.Flush();
            if (Math.Abs(m_il) < 1e-30) m_il = 0; if (Math.Abs(m_ir) < 1e-30) m_ir = 0;
        }
    }
}
