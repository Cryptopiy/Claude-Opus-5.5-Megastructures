// KoiTone: the K layer of MASTER_PROMPT 14.3 ("Koi tone": sines E1 41.2 Hz and A1 55.0 Hz with slow FM (0.07 Hz, depth
// 3 Hz) and partials at 123.6, 164.8 and 247.2 Hz at -6, -10 and -14 dB, audible on laptop speakers). The partials are the
// 3rd, 4th and 6th harmonics of E1 and follow its FM; A1 swings against E1 (opposite FM phase), so the pair breathes. The
// image sits at the koi head's azimuth (KoiMath orbit, NocturneScore.KoiDistance) with half width. The level (the
// distance law 1 / (1 + (d / 400)^2), normalised to -33 dBFS at its peak, rising faintly from s 1 200) is NocturneScore's;
// this class is unit RMS at gain 1, the 20 Hz DC blocker's loss compensated per partial (-0.91 dB at 41.2 Hz).
// Fix wave C09 (ADR request against the 14.3 partial list, docs/AUDIO.md 11): on laptops and phones (high-pass 200-400 Hz) only
// the 247.2 Hz partial was left, at -14 dB, and it lay on the pad's 2 x B2 / 3 x E2, so the koi fused into the chord. The 6th
// partial rises to -8 dB and the 8th, 9th and 12th (329.6 E4, 370.8 F#4, 494.4 Hz B4: tones of C9 Em9) join at -10 / -10 / -9 dB,
// most of the new power in the pad's 500 Hz octave, where it has room (dread_analysis f06: the koi under the pad in each of its
// octaves at both speeds); the upper four carry their own slow vibrato (0.13 Hz, +-0.6 %, against the shared FM) so they move
// apart from the static pad partials.
// Double phases, increments at control rate. Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    public sealed class KoiTone
    {
        public const double E1 = 41.2, A1 = 55.0, FmHz = 0.07, FmDepthHz = 3.0;
        /// <summary>Partials of E1 (41.2, 123.6, 164.8, 247.2, 329.6, 370.8, 494.4 Hz) and their levels (dB re E1); A1 at 0 dB is the last voice.</summary>
        public static readonly double[] Ratio = { 1.0, 3.0, 4.0, 6.0, 8.0, 9.0, 12.0 };
        public static readonly double[] PartialDb = { 0.0, -6.0, -10.0, -8.0, -10.0, -10.0, -9.0 };
        /// <summary>The upper partials' own vibrato (from index UpperFrom): rate (Hz) and depth (fraction).</summary>
        public const int UpperFrom = 3;
        public const double UpperVibHz = 0.13, UpperVibDepth = 0.006;
        const int NP = 7, NV = NP + 1;
        readonly double m_fs;
        readonly double[] m_amp = new double[NV], m_ph = new double[NV], m_inc = new double[NV];
        readonly double m_norm;
        GainRamp m_l, m_r;

        public KoiTone(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            double p = 0;
            for (int i = 0; i < NP; i++)
            {
                m_amp[i] = NocAudio.Db(PartialDb[i]) / DcBlockD.Magnitude(E1 * Ratio[i], NocturneConstants.DcBlockHz, fs);
                p += 0.5 * NocAudio.Db(PartialDb[i]) * NocAudio.Db(PartialDb[i]);
            }
            m_amp[NP] = 1.0 / DcBlockD.Magnitude(A1, NocturneConstants.DcBlockHz, fs);
            p += 0.5;
            m_norm = 1.0 / Math.Sqrt(p);
            for (int i = 0; i < NV; i++) m_ph[i] = NocAudio.Hash01(DspHash.Mix(seed ^ 0x6015UL), (ulong)i);
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
            double vib = 1.0 + UpperVibDepth * Math.Sin(NocAudio.TwoPi * UpperVibHz * f.T);
            for (int i = 0; i < NP; i++) m_inc[i] = (E1 + fm) * Ratio[i] * (i >= UpperFrom ? vib : 1.0) / m_fs;
            m_inc[NP] = (A1 - fm) / m_fs;
            Head(f.S, out double hx, out double hz);
            NocAudio.Pan(0.5 * f.PanTo(hx, hz), out double pl, out double pr);
            m_l.Step(pl * m_norm); m_r.Step(pr * m_norm);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            double x = 0;
            for (int i = 0; i < NV; i++)
            {
                x += m_amp[i] * SineTable.Lookup(m_ph[i]);
                double p = m_ph[i] + m_inc[i]; if (p >= 1.0) p -= 1.0; m_ph[i] = p;
            }
            l = m_l.At(u) * x; r = m_r.At(u) * x;
        }
    }
}
