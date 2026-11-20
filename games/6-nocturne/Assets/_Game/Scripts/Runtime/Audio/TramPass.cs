// TramPass: the tram T_A of MASTER_PROMPT 14.5.2 (beat 2, X): launched at s 230 from the terminus (x 205) on the north track
// z 227, west at 7 m/s, x(tau) = 205 - 7 tau (14.7 H7; abeam at s 272.0 at 2.5 m/s, 258.2 at 1.5), body 60 m. The pass is the
// finite line source of PassGeometry (the part still inside the terminus x 205-215.3 is muffled 20 dB) with rolling noise
// (pink -> band 600 Hz, LP 900 Hz far -> 4 kHz near), motor whine 420 / 840 Hz (-12 dB, Doppler), a 1.8 kHz rail sing, and the
// tram bell: two strikes ("ding-ding", 0.28 s apart) of a modal bell (partials 1 : 2.0 : 2.76 : 5.40 : 8.93 at 1.3 kHz, T60
// 1.2 s on the fundamental, shorter above) at tau 8.5 s, when the head is about 80 m before abeam at either walk speed; the
// bell (and only the bell) is exempt from the 14.3 step limit. Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    public sealed class TramPass
    {
        /// <summary>RMS dBFS of the tram abeam at 32 m (the 14.5.2 pass distance), before the X-layer swells.</summary>
        public const double PeakDb = -28.0, RefDistance = 32.0, FarHz = 900.0, NearHz = 4000.0;
        public const double BellTau = 8.5, BellGap = 0.28, BellHz = 1300.0, BellPeakDb = -26.0, BellRefDistance = 86.0;
        static readonly double[] WhineHz = { 420.0, 840.0, 1800.0 };
        static readonly double[] WhineDb = { 0.0, -4.0, -8.0 };
        static readonly double[] BellRatio = { 1.0, 2.0, 2.76, 5.40, 8.93 };
        static readonly double[] BellAmp = { 1.0, 0.5, 0.4, 0.25, 0.15 };
        static readonly double[] BellT60 = { 1.2, 0.8, 0.6, 0.35, 0.2 };
        readonly NocturneScore.Vehicle m_v;
        readonly double m_fs;
        VehicleClock m_clock;
        PassCore m_core;
        // two strike voices x five partials (a second strike never resets the ringing first one)
        readonly double[] m_bPh = new double[10], m_bE = new double[10], m_bMul = new double[5];
        readonly double[] m_bGL = new double[2], m_bGR = new double[2];
        double m_bAmp;
        int m_strikes;
        public PassGeometry Last;
        public double Tau { get; private set; }
        public bool Active { get; private set; }

        public TramPass(double fs, double controlRate, uint seed)
        {
            m_v = NocturneScore.Vehicles[0]; m_fs = fs;
            m_core = new PassCore(fs, controlRate, seed, 0x7700u, 600.0, 0.8, WhineHz, WhineDb, -12.0, -10.0);
            for (int i = 0; i < 5; i++) m_bMul[i] = NocAudio.T60Mul(BellT60[i], fs);
            double peak = 0; for (int i = 0; i < 5; i++) peak += BellAmp[i];
            m_bAmp = NocAudio.Db(BellPeakDb) / peak;
        }

        public void Control(in ScoreFrame f, double vListenerX)
        {
            m_clock.Update(f.N, f.S, f.Fs, m_v.SLaunch);
            Tau = m_clock.Tau(f.N, f.Fs);
            if (!m_clock.Launched) m_strikes = 0;
            bool on = Tau >= 0.0 && Tau < 100.0;
            if (!on) { Active = !m_core.Silent || m_bE[0] > 1e-7 || m_bE[5] > 1e-7; m_core.Control(0.0, FarHz, 0.0, 0.0, 1.0, m_fs); return; }
            Active = true;
            var g = PassGeometry.Evaluate(in m_v, Tau, in f, RefDistance, vListenerX);
            Last = g;
            double lp = FarHz * Math.Pow(NearHz / FarHz, g.Bright) * (0.75 + 0.25 * g.Front);
            double dh = Math.Abs(g.NoseX), far = 1.0 / (1.0 + (dh / 300.0) * (dh / 300.0));   // air and the canyon's far blocks
            m_core.Control(NocAudio.Db(PeakDb) * g.Gain * far, lp, g.Width, g.Pan, g.Doppler, m_fs);
            // the bell: strike k at tau 8.5 + 0.28 k (k = 0, 1), from the head
            double next = BellTau + BellGap * m_strikes;
            if (m_strikes < 2 && Tau >= next && Tau < next + 1.0)
            {
                double xh = m_v.HeadX(Tau);
                double dx = xh - f.X, dz = m_v.TrackZ - f.Z, d = Math.Max(5.0, Math.Sqrt(dx * dx + dz * dz));
                double a = m_bAmp * (m_strikes == 0 ? 1.0 : 0.8) * BellRefDistance / d;
                int o = 5 * m_strikes;
                for (int i = 0; i < 5; i++) { m_bE[o + i] = a * BellAmp[i]; m_bPh[o + i] = 0.0; }
                NocAudio.Pan(f.PanTo(xh, m_v.TrackZ), out m_bGL[m_strikes], out m_bGR[m_strikes]);
                m_strikes++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            m_core.Tick(u, out l, out r);
            for (int v = 0; v < 2; v++)
            {
                int o = 5 * v;
                if (m_bE[o] < 1e-7) continue;
                double x = 0;
                for (int i = 0; i < 5; i++)
                {
                    x += m_bE[o + i] * SineTable.Lookup(m_bPh[o + i]);
                    m_bPh[o + i] += BellHz * BellRatio[i] / m_fs; if (m_bPh[o + i] >= 1.0) m_bPh[o + i] -= 1.0;
                    m_bE[o + i] *= m_bMul[i];
                }
                l += m_bGL[v] * x; r += m_bGR[v] * x;
            }
        }

        public void Flush() { m_core.Flush(); }
    }
}
