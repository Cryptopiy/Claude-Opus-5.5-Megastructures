// KoiTone: the K layer of MASTER_PROMPT 14.3 ("Koi tone": sines E1 41.2 Hz and A1 55.0 Hz with slow FM (0.07 Hz, depth
// 3 Hz) and partials at 123.6, 164.8 and 247.2 Hz at -6, -10 and -14 dB, audible on laptop speakers). The partials are the
// 3rd, 4th and 6th harmonics of E1 and follow its FM; A1 swings against E1 (opposite FM phase), so the pair breathes. The
// image sits at the koi head's azimuth (KoiMath orbit, NocturneScore.KoiDistance) with half width. The level (the
// distance law 1 / (1 + (d / 400)^2), normalised to -33 dBFS at its peak, rising faintly from s 1 200) is NocturneScore's;
// this class is unit RMS at gain 1, the 20 Hz DC blocker's loss compensated per partial (-0.91 dB at 41.2 Hz).
// Double phases, increments at control rate. Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    public sealed class KoiTone
    {
        public const double E1 = 41.2, A1 = 55.0, FmHz = 0.07, FmDepthHz = 3.0;
        static readonly double[] Ratio = { 1.0, 3.0, 4.0, 6.0 };           // of E1 (41.2, 123.6, 164.8, 247.2 Hz)
        static readonly double[] PartialDb = { 0.0, -6.0, -10.0, -14.0 };
        readonly double m_fs;
        readonly double[] m_amp = new double[5], m_ph = new double[5], m_inc = new double[5];
        readonly double m_norm;
        GainRamp m_l, m_r;

        public KoiTone(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            double p = 0;
            for (int i = 0; i < 4; i++)
            {
                m_amp[i] = NocAudio.Db(PartialDb[i]) / DcBlockD.Magnitude(E1 * Ratio[i], NocturneConstants.DcBlockHz, fs);
                p += 0.5 * NocAudio.Db(PartialDb[i]) * NocAudio.Db(PartialDb[i]);
            }
            m_amp[4] = 1.0 / DcBlockD.Magnitude(A1, NocturneConstants.DcBlockHz, fs);
            p += 0.5;
            m_norm = 1.0 / Math.Sqrt(p);
            for (int i = 0; i < 5; i++) m_ph[i] = NocAudio.Hash01(DspHash.Mix(seed ^ 0x6015UL), (ulong)i);
            m_l.Init(0.1, controlRate); m_r.Init(0.1, controlRate);
        }

        /// <summary>Head of the koi (x, z) at s, for the image (the level is NocturneScore.KoiAmp).</summary>
        public static void Head(double s, out double x, out double z)
        {
            D3 h = KoiMath.OrbitPoint(KoiMath.HeadAzimuth(KoiMath.PhaseU(s)));
            x = h.x; z = h.z;
        }

        public void Control(in ScoreFrame f)
        {
            double fm = FmDepthHz * Math.Sin(NocAudio.TwoPi * FmHz * f.T);
            for (int i = 0; i < 4; i++) m_inc[i] = (E1 + fm) * Ratio[i] / m_fs;
            m_inc[4] = (A1 - fm) / m_fs;
            Head(f.S, out double hx, out double hz);
            NocAudio.Pan(0.5 * f.PanTo(hx, hz), out double pl, out double pr);
            m_l.Step(pl * m_norm); m_r.Step(pr * m_norm);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            double x = 0;
            for (int i = 0; i < 5; i++)
            {
                x += m_amp[i] * SineTable.Lookup(m_ph[i]);
                double p = m_ph[i] + m_inc[i]; if (p >= 1.0) p -= 1.0; m_ph[i] = p;
            }
            l = m_l.At(u) * x; r = m_r.At(u) * x;
        }
    }
}
