// LeafRain: the X layer of beat 5, the hanging garden (MASTER_PROMPT 14.3 "X ... leaf rain"; docs/research/R5 4.8): rain on
// the crowns of seventeen trees above the path, a dense soft patter (2 000 impulses per second at rho 1 in a 2-6 kHz band)
// plus a rustle (noise band-passed 2-4 kHz with an 8-20 Hz amplitude flutter that wanders with the crown sway, period 7 s,
// 14.5.5). The leaf drips themselves are DripField's garden emitters (D). S6 (s 876-896) lifts it (NocturneScore). Unit RMS
// at gain 1 (analytic patter power; rustle by the flutter's mean square). Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    public sealed class LeafRain
    {
        public const double Rate = 2000.0, BandHz = 3500.0, RustleHz = 2900.0, RustleQ = 1.4, PatterShare = 0.6;
        readonly double m_fs;
        Patter m_pat;
        NoiseD m_nl, m_nr;
        SvfD m_rl, m_rr;
        double m_flPh;
        readonly double m_rustleNorm;
        GainRamp m_fl;
        readonly ulong m_seed;

        public LeafRain(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            m_seed = DspHash.Mix(seed ^ 0x1EAFUL);
            m_pat = new Patter(m_seed, 1.5);   // softer than rain on stone: dense, low crest factor
            m_nl = NoiseD.Stream(seed, 0x1E01u); m_nr = NoiseD.Stream(seed, 0x1E02u);
            m_rl.Set(RustleHz, RustleQ, fs); m_rr.CopyCoefficients(in m_rl);
            // white (variance 1/3) through BandNorm: pi fc / (Q fs) / 3; the flutter's mean square is 0.5^2 + 0.3^2 / 2 = 0.295
            m_rustleNorm = 1.0 / Math.Sqrt(Math.PI * RustleHz / (RustleQ * fs) / 3.0 * (0.25 + 0.045));
            m_fl.Init(0.0, controlRate);
        }

        public void Control(in ScoreFrame f)
        {
            m_pat.Set(Rate * Math.Max(0.05, f.Rho), BandHz, 0.0, 3000.0, m_fs);
            // flutter 8-20 Hz, its rate wandering with the crown sway (7 s): 0.5 + 0.3 sin(phase)
            double rate = 8.0 + 12.0 * NocAudio.SmoothRandom01(m_seed, f.T / 7.0);
            m_flPh += rate * f.Block * f.InvFs; m_flPh -= Math.Floor(m_flPh);
            m_fl.Step(0.5 + 0.3 * Math.Sin(NocAudio.TwoPi * m_flPh));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, long n, out double l, out double r)
        {
            m_pat.Tick(n, m_fs, out double pl, out double pr);
            double k = Math.Sqrt(1.0 - PatterShare) * m_rustleNorm * m_fl.At(u), kp = Math.Sqrt(PatterShare);
            l = kp * pl + k * m_rl.BandNorm(m_nl.Next());
            r = kp * pr + k * m_rr.BandNorm(m_nr.Next());
        }

        public void Flush() { m_pat.Flush(); m_rl.Flush(); m_rr.Flush(); }
    }
}
