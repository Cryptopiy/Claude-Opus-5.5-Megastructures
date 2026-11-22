// DreadVoices: the sound sources of the D-034 dread layer (docs/DREAD.md; DreadScore holds the score) and DreadLayer, the
// stem E of NocMixer. Every source is unit RMS at gain 1 over its nominal length (normalisers measured by Tools/audio/harness
// "dread"), so the levels in DreadScore are dBFS at the listener. Rules of the wave-A DSP (docs/AUDIO.md 2): control every 32
// samples at absolute n = 32 k, one-shots fired when s crosses their s and started at an exact sample (trigger + flight time),
// noise streams seeded by the event index, phases of the continuous tones set from the absolute sample index, every gain on a
// 50 ms ramp or a smooth per-sample envelope, nothing allocates after the constructor, no UnityEngine.
//   HornVoice    a far ship horn: a PolyBLEP saw fifth with a 30-cent scoop, low-passed (distance), 7 s
//   StrikeVoice  a metal strike inside a face: six inharmonic damped modes (complex rotators, they start at zero phase)
//   GroanVoice   a hull groan: a resonant comb (30-70 Hz, slow downward bend, damped loop) excited by band noise, the
//                stick-slip creak grains at 250-480 Hz on top (they carry it on laptop speakers), a low boom on some
//   CreakVoice   the panel tearing off face B: accelerating friction grains through four bending resonators; never an impact
//   ChimeVoice   the tram's two-tone door chime (D5, Bb4 bell partials)
//   RumbleVoice  a far low rumble under a lightning glow (brown noise LP 85 Hz with a rolling 190 Hz band), no crack
//   SubSwell     the five key-shot swells (three-voice partials, slow beating, a brown-noise pressure bed) and their
//                small-speaker layer (C09: chord-tone harmonics 5-12 of the fundamental, 180-500 Hz, on the same envelope)
//   ArkDrone     the Ark (D-043): the body's infrasonic stack and pressure bed as a line source along the 5 km hull, the bow wave
//                and four engine pods as point sources with their long Doppler glides, small-speaker harmonics at 220-330 Hz;
//                it also reports the silence before the bow and the cover under the belly (NocMixer ducks the city under it)
//   MachineHum   the elevator cabins and the face-B gantries near the path
//   BridgeGust   beat 4: cable whistle and deck creaks at the extremes of the 0.12 Hz sway, scaled by the rain-slant gust
//   EchoBus      the Gulf echoes: three taps (A1, B1, AB2) read from one mono delay line, gliding with the listener, each
//                low-passed by its path and panned to its face
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    /// <summary>Shared one-shot state: onset sample, pan ramps, the distance low-pass.</summary>
    public struct OneShotState
    {
        public bool Active;
        public long OnN;
        public double X, Z, Lp;
        public GainRamp PanL, PanR;
        double m_lpy;

        public void Init(double cr) { PanL.Init(0.05, cr); PanR.Init(0.05, cr); }

        public void Begin(long onN, double x, double z, double lpHz, in ScoreFrame f)
        {
            Active = true; OnN = onN; X = x; Z = z; m_lpy = 0.0;
            Lp = 1.0 - Math.Exp(-2.0 * Math.PI * Math.Min(lpHz, 0.45 * f.Fs) / f.Fs);
            NocAudio.Pan(f.PanTo(x, z) * 0.85, out double pl, out double pr);
            PanL.Snap(pl); PanR.Snap(pr);
        }

        public void Control(in ScoreFrame f)
        {
            NocAudio.Pan(f.PanTo(X, Z) * 0.85, out double pl, out double pr);
            PanL.Step(pl); PanR.Step(pr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LowPass(double x) { m_lpy += Lp * (x - m_lpy); return m_lpy; }

        /// <summary>Seconds since the onset at sample n (negative before it).</summary>
        public double Age(long n, double invFs) => (n - OnN) * invFs;
    }

    // ====================================================================== horn
    public sealed class HornVoice
    {
        public OneShotState St;
        double m_fs, m_invFs, m_f1, m_ph1, m_ph2, m_amp, m_dur;
        SvfD m_lp1;
        /// <summary>Unit normaliser (harness "dread": peak 1 s RMS at gain 1 -> 0 dB).</summary>
        public const double Norm = 0.7408;

        public HornVoice(double fs, double cr) { m_fs = fs; m_invFs = 1.0 / fs; St.Init(cr); }

        public void Start(in DreadEvent e, long onN, in ScoreFrame f, double gain)
        {
            St.Begin(onN, e.X, e.Z, 2500.0, in f);
            m_f1 = e.Hz; m_ph1 = 0.13; m_ph2 = 0.61; m_dur = e.Seconds;
            m_lp1.Set(560.0, 1.1, m_fs); m_lp1.Clear();
            // saw power through the low-pass: sum (2 / (pi k))^2 / 2 |H(k f)|^2, the fifth at -4 dB
            double p = 0.0, g2 = NocAudio.Db(-4.0);
            for (int k = 1; k * m_f1 < 0.45 * m_fs; k++)
            {
                double a = 2.0 / (Math.PI * k);
                p += 0.5 * a * a * SvfD.LowPower(k * m_f1 / 560.0, 1.1);
                if (k * 1.5 * m_f1 < 0.45 * m_fs) p += g2 * g2 * 0.5 * a * a * SvfD.LowPower(k * 1.5 * m_f1 / 560.0, 1.1);
            }
            m_amp = gain * Norm / Math.Sqrt(Math.Max(1e-12, p * 0.62));   // 0.62: the envelope's mean square over its 7 s
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(long n, out double l, out double r, out double mono)
        {
            l = 0; r = 0; mono = 0;
            if (!St.Active || n < St.OnN) return;
            double t = St.Age(n, m_invFs);
            if (t >= m_dur) { St.Active = false; return; }
            double env = NocMath.Smootherstep(0.0, 0.7, t) * (1.0 - NocMath.Smootherstep(m_dur - 2.5, m_dur, t));
            double cents = -30.0 * (1.0 - NocMath.Smoothstep(0.0, 0.45, t)) + 3.0 * Math.Sin(NocAudio.TwoPi * 0.31 * t);
            double ratio = Math.Pow(2.0, cents / 1200.0);
            double dt1 = m_f1 * ratio * m_invFs, dt2 = 1.5 * dt1 * 1.0006;
            double x = Blep.Saw(m_ph1, dt1) + NocAudio.Db(-4.0) * Blep.Saw(m_ph2, dt2);
            m_ph1 += dt1; if (m_ph1 >= 1.0) m_ph1 -= 1.0;
            m_ph2 += dt2; if (m_ph2 >= 1.0) m_ph2 -= 1.0;
            double y = St.LowPass(m_lp1.Low(x));
            mono = m_amp * env * y;
            l = mono * St.PanL.Prev; r = mono * St.PanR.Prev;
        }
    }

    // ====================================================================== metal strike
    public sealed class StrikeVoice
    {
        public OneShotState St;
        static readonly double[] Ratio = { 1.0, 2.32, 4.25, 6.63, 9.38, 12.53 };
        static readonly double[] Amp = { 1.0, 0.8, 0.6, 0.45, 0.3, 0.2 };
        static readonly double[] T60 = { 4.0, 3.0, 2.2, 1.6, 1.1, 0.8 };
        readonly double[] m_re = new double[6], m_im = new double[6], m_cr = new double[6], m_ci = new double[6];
        double m_fs, m_invFs, m_amp, m_dur;
        /// <summary>Unit normaliser (harness "dread": peak 1 s RMS at gain 1 -> 0 dB).</summary>
        public const double Norm = 1.346;

        public StrikeVoice(double fs, double cr) { m_fs = fs; m_invFs = 1.0 / fs; St.Init(cr); }

        public void Start(in DreadEvent e, long onN, in ScoreFrame f, double gain, double distance)
        {
            St.Begin(onN, e.X, e.Z, DreadScore.EchoLowPassHz(distance), in f);
            m_dur = e.Seconds;
            double p = 0.0;
            for (int k = 0; k < 6; k++)
            {
                double fk = e.Hz * Ratio[k];
                double rr = fk < 0.45 * m_fs ? NocAudio.T60Mul(T60[k], m_fs) : 0.0, w = NocAudio.TwoPi * fk * m_invFs;
                m_cr[k] = rr * Math.Cos(w); m_ci[k] = rr * Math.Sin(w); m_re[k] = Amp[k]; m_im[k] = 0.0;
                // mean power over the first 0.5 s of A e^{-t / tau} sin: A^2 / 2 x tau' (1 - e^{-2 T / tau'}) / (2 T)
                double tau = T60[k] / 6.907755278982137;
                p += 0.5 * Amp[k] * Amp[k] * tau * (1.0 - Math.Exp(-1.0 / tau)) / 1.0;
            }
            m_amp = gain * Norm / Math.Sqrt(Math.Max(1e-12, p));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(long n, out double l, out double r, out double mono)
        {
            l = 0; r = 0; mono = 0;
            if (!St.Active || n < St.OnN) return;
            double t = St.Age(n, m_invFs);
            if (t >= m_dur) { St.Active = false; return; }
            double y = 0.0;
            for (int k = 0; k < 6; k++)
            {
                double re = m_re[k], im = m_im[k];
                m_re[k] = re * m_cr[k] - im * m_ci[k]; m_im[k] = re * m_ci[k] + im * m_cr[k];
                y += m_im[k];
            }
            double tail = 1.0 - NocMath.Smootherstep(m_dur - 1.0, m_dur, t);
            mono = m_amp * tail * St.LowPass(y);
            l = mono * St.PanL.Prev; r = mono * St.PanR.Prev;
        }
    }

    // ====================================================================== structural groan
    public sealed class GroanVoice
    {
        public OneShotState St;
        readonly double[] m_buf = new double[16384];
        int m_w;
        double m_fs, m_invFs, m_f0, m_bend, m_amp, m_dur, m_loop, m_grainRate, m_boom;
        double m_boomRe, m_boomIm, m_boomCr, m_boomCi;
        NoiseD m_n1, m_n2;
        PinkD m_pk;
        SvfD m_ex, m_creak, m_creak2;
        ulong m_seed;
        /// <summary>Unit normaliser (harness "dread": peak 1 s RMS at gain 1 -> 0 dB).</summary>
        public const double Norm = 4.584;

        public GroanVoice(double fs, double cr) { m_fs = fs; m_invFs = 1.0 / fs; St.Init(cr); }

        public void Start(in DreadEvent e, long onN, in ScoreFrame f, double gain, double x, double z, double distance)
        {
            St.Begin(onN, x, z, DreadScore.EchoLowPassHz(distance) * 0.8, in f);
            Array.Clear(m_buf, 0, m_buf.Length); m_w = 0; m_loop = 0.0;
            m_seed = 0x6A0A0UL + (ulong)e.Seq;
            m_f0 = e.Hz; m_dur = e.Seconds;
            m_bend = 0.06 + 0.08 * NocAudio.Hash01(m_seed, 1);
            m_grainRate = 9.0 + 13.0 * NocAudio.Hash01(m_seed, 2);
            m_boom = NocAudio.Hash01(m_seed, 3) < 0.35 ? 1.0 : 0.0;
            m_n1 = NoiseD.Stream(0x6A0Au, (uint)e.Seq * 2u + 1u); m_n2 = NoiseD.Stream(0x6A0Au, (uint)e.Seq * 2u + 2u); m_pk = new PinkD();
            m_ex.Set(2.2 * m_f0, 0.8, m_fs); m_ex.Clear();
            double ch = 250.0 + 230.0 * NocAudio.Hash01(m_seed, 4);
            m_creak.Set(ch, 5.0, m_fs); m_creak.Clear(); m_creak2.Set(ch * 1.71, 6.0, m_fs); m_creak2.Clear();
            double wb = NocAudio.TwoPi * 0.75 * m_f0 * m_invFs, rb = NocAudio.T60Mul(2.0, m_fs);
            m_boomCr = rb * Math.Cos(wb); m_boomCi = rb * Math.Sin(wb); m_boomRe = 1.0; m_boomIm = 0.0;
            m_amp = gain * Norm;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(long n, out double l, out double r, out double mono)
        {
            l = 0; r = 0; mono = 0;
            if (!St.Active || n < St.OnN) return;
            double t = St.Age(n, m_invFs);
            if (t >= m_dur) { St.Active = false; return; }
            double env = NocMath.Smootherstep(0.0, 0.9, t) * (1.0 - NocMath.Smootherstep(m_dur - 1.6, m_dur, t));
            double load = 0.6 + 0.4 * NocAudio.SmoothRandom01(m_seed, 1.3 * t);
            // comb: y = x + g LP(y[n - L]); L from the bent pitch
            double fz = m_f0 * (1.0 - m_bend * NocMath.Smootherstep(0.0, m_dur, t));
            double L = m_fs / fz;
            double rp = m_w - L; if (rp < 0) rp += m_buf.Length;
            int i0 = (int)rp; double fr = rp - i0; int i1 = i0 + 1; if (i1 >= m_buf.Length) i1 = 0;
            double dl = m_buf[i0] + fr * (m_buf[i1] - m_buf[i0]);
            m_loop += 0.09 * (dl - m_loop);
            double ex = m_ex.BandNorm(m_pk.Next(m_n1.Next())) * load;
            double y = ex + 0.93 * m_loop;
            m_buf[m_w] = y; if (++m_w >= m_buf.Length) m_w = 0;
            // creak grains (stick-slip): raised-cosine grains at 9-22 Hz with hashed amplitudes
            double gp = m_grainRate * t; long gi = (long)gp; double gf = gp - gi;
            double ga = NocAudio.Hash01(m_seed + 17UL, (ulong)gi); ga = ga * ga;
            double gw = 0.5 - 0.5 * Math.Cos(NocAudio.TwoPi * gf); gw *= gw; gw *= gw;
            double nz = m_n2.Next();
            double cr = (m_creak.BandNorm(nz) + 0.6 * m_creak2.BandNorm(nz)) * ga * gw * 3.0;
            double boom = 0.0;
            if (m_boom > 0.0)
            {
                double re = m_boomRe, im = m_boomIm;
                m_boomRe = re * m_boomCr - im * m_boomCi; m_boomIm = re * m_boomCi + im * m_boomCr;
                boom = m_boomIm * 0.5 * NocMath.Smoothstep(0.0, 0.04, t);
            }
            double s = 0.22 * y + 0.8 * cr * load + boom;
            mono = m_amp * St.LowPass(s) * env;
            l = mono * St.PanL.Prev; r = mono * St.PanR.Prev;
        }
    }

    // ====================================================================== the panel tearing (DREAD 3.2)
    public sealed class CreakVoice
    {
        public OneShotState St;
        static readonly double[] ResHz = { 310.0, 520.0, 870.0, 1350.0 };
        static readonly double[] ResQ = { 14.0, 12.0, 10.0, 8.0 };
        static readonly double[] ResA = { 1.0, 0.8, 0.55, 0.35 };
        SvfD m_r0, m_r1, m_r2, m_r3, m_low;
        NoiseD m_n;
        double m_fs, m_invFs, m_amp, m_dur, m_pulse, m_lastBend = -1;
        ulong m_seed;
        /// <summary>Unit normaliser (harness "dread": peak 1 s RMS at gain 1 -> 0 dB).</summary>
        public const double Norm = 39.5;

        public CreakVoice(double fs, double cr) { m_fs = fs; m_invFs = 1.0 / fs; St.Init(cr); }

        public void Start(in DreadEvent e, long onN, in ScoreFrame f, double gain, double distance)
        {
            St.Begin(onN, e.X, e.Z, DreadScore.EchoLowPassHz(distance) * 1.4, in f);
            m_seed = 0xC2EA0UL + (ulong)e.Seq; m_dur = e.Seconds; m_pulse = 0.0; m_lastBend = -1;
            m_n = NoiseD.Stream(0xC2EAu, (uint)e.Seq + 1u);
            SetRes(1.0); m_r0.Clear(); m_r1.Clear(); m_r2.Clear(); m_r3.Clear();
            m_low.Set(95.0, 1.5, m_fs); m_low.Clear();
            m_amp = gain * Norm;
        }

        void SetRes(double k)
        {
            m_r0.Set(ResHz[0] * k, ResQ[0], m_fs); m_r1.Set(ResHz[1] * k, ResQ[1], m_fs);
            m_r2.Set(ResHz[2] * k, ResQ[2], m_fs); m_r3.Set(ResHz[3] * k, ResQ[3], m_fs);
        }

        public void Control(in ScoreFrame f, long n)
        {
            St.Control(in f);
            if (!St.Active || n < St.OnN) return;
            double t = St.Age(n, m_invFs);
            double bend = 1.0 - 0.12 * NocMath.Smootherstep(0.0, m_dur, t);
            if (Math.Abs(bend - m_lastBend) > 1e-4) { SetRes(bend); m_lastBend = bend; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(long n, out double l, out double r, out double mono)
        {
            l = 0; r = 0; mono = 0;
            if (!St.Active || n < St.OnN) return;
            double t = St.Age(n, m_invFs);
            if (t >= m_dur) { St.Active = false; return; }
            double env = NocMath.Smootherstep(0.0, 0.3, t) * (1.0 - NocMath.Smootherstep(m_dur - 1.2, m_dur, t));
            // friction grains: rate 18 -> 120 Hz over 2.8 s (the tear accelerates), each a 2 ms decaying pulse
            double rate = 18.0 * Math.Pow(120.0 / 18.0, Math.Min(1.0, t / 2.8));
            double gp = rate * t;
            long gi = (long)Math.Floor(gp * 1.0);
            double gf = gp - gi;
            double a = NocAudio.Hash01(m_seed, (ulong)gi);
            double pulse = a * Math.Exp(-gf / Math.Max(1e-3, 0.002 * rate)) * (0.5 + 0.5 * m_n.Next());
            m_pulse += 0.5 * (pulse - m_pulse);
            double x = m_pulse;
            double y = ResA[0] * m_r0.BandNorm(x) + ResA[1] * m_r1.BandNorm(x) + ResA[2] * m_r2.BandNorm(x) + ResA[3] * m_r3.BandNorm(x);
            y += 0.6 * m_low.BandNorm(m_n.Next() * 0.5) * env;
            mono = m_amp * env * St.LowPass(y);
            l = mono * St.PanL.Prev; r = mono * St.PanR.Prev;
        }
    }

    // ====================================================================== the tram's door chime (DREAD 6)
    public sealed class ChimeVoice
    {
        public OneShotState St;
        static readonly double[] Ratio = { 1.0, 2.0, 3.01, 4.2 };
        static readonly double[] Amp = { 1.0, 0.35, 0.18, 0.08 };
        static readonly double[] T60 = { 2.4, 1.5, 1.0, 0.6 };
        readonly double[] m_re = new double[8], m_im = new double[8], m_cr = new double[8], m_ci = new double[8];
        double m_fs, m_invFs, m_amp, m_dur;
        public const double SecondNoteDelay = 0.55;
        /// <summary>Unit normaliser (harness "dread": peak 1 s RMS at gain 1 -> 0 dB).</summary>
        public const double Norm = 1.033;

        public ChimeVoice(double fs, double cr) { m_fs = fs; m_invFs = 1.0 / fs; St.Init(cr); }

        public void Start(in DreadEvent e, long onN, in ScoreFrame f, double gain)
        {
            St.Begin(onN, e.X, e.Z, 3500.0, in f);
            m_dur = e.Seconds;
            double p = 0.0;
            for (int q = 0; q < 2; q++)
            {
                double f0 = q == 0 ? e.Hz : e.Hz * Math.Pow(2.0, -4.0 / 12.0);
                for (int k = 0; k < 4; k++)
                {
                    int j = 4 * q + k;
                    double fk = f0 * Ratio[k], rr = NocAudio.T60Mul(T60[k], m_fs), w = NocAudio.TwoPi * fk * m_invFs;
                    m_cr[j] = rr * Math.Cos(w); m_ci[j] = rr * Math.Sin(w); m_re[j] = Amp[k]; m_im[j] = 0.0;
                    double tau = T60[k] / 6.907755278982137;
                    p += 0.5 * Amp[k] * Amp[k] * tau * (1.0 - Math.Exp(-2.0 / tau)) / 2.0;
                }
            }
            m_amp = gain * Norm / Math.Sqrt(Math.Max(1e-12, p));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(long n, out double l, out double r, out double mono)
        {
            l = 0; r = 0; mono = 0;
            if (!St.Active || n < St.OnN) return;
            double t = St.Age(n, m_invFs);
            if (t >= m_dur) { St.Active = false; return; }
            double y = 0.0;
            int top = t >= SecondNoteDelay ? 8 : 4;
            for (int j = 0; j < top; j++)
            {
                double re = m_re[j], im = m_im[j];
                m_re[j] = re * m_cr[j] - im * m_ci[j]; m_im[j] = re * m_ci[j] + im * m_cr[j];
                y += m_im[j];
            }
            double tail = 1.0 - NocMath.Smootherstep(m_dur - 0.8, m_dur, t);
            mono = m_amp * tail * St.LowPass(y);
            l = mono * St.PanL.Prev; r = mono * St.PanR.Prev;
        }
    }

    // ====================================================================== far rumble under a lightning glow (DREAD 5.4)
    public sealed class RumbleVoice
    {
        public OneShotState St;
        NoiseD m_n1, m_n2;
        PinkD m_pk;
        SvfD m_lp, m_roll;
        double m_fs, m_invFs, m_amp, m_dur, m_brown;
        ulong m_seed;
        /// <summary>Unit normaliser (harness "dread": peak 1 s RMS at gain 1 -> 0 dB).</summary>
        public const double Norm = 1.961;

        public RumbleVoice(double fs, double cr) { m_fs = fs; m_invFs = 1.0 / fs; St.Init(cr); }

        public void Start(in DreadEvent e, long onN, in ScoreFrame f, double gain)
        {
            St.Begin(onN, e.X, e.Z, 900.0, in f);
            m_seed = 0x2B3B0UL + (ulong)e.Seq; m_dur = e.Seconds; m_brown = 0.0;
            m_n1 = NoiseD.Stream(0x2B3Bu, (uint)e.Seq * 2u + 1u); m_n2 = NoiseD.Stream(0x2B3Bu, (uint)e.Seq * 2u + 2u); m_pk = new PinkD();
            m_lp.Set(85.0, 0.7, m_fs); m_lp.Clear(); m_roll.Set(220.0, 0.9, m_fs); m_roll.Clear();
            m_amp = gain * Norm;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(long n, out double l, out double r, out double mono)
        {
            l = 0; r = 0; mono = 0;
            if (!St.Active || n < St.OnN) return;
            double t = St.Age(n, m_invFs);
            if (t >= m_dur) { St.Active = false; return; }
            double env = NocMath.Smootherstep(0.0, 1.0, t) * Math.Exp(-Math.Max(0.0, t - 1.0) / 2.2) * (1.0 - NocMath.Smootherstep(m_dur - 1.5, m_dur, t));
            m_brown = 0.9985 * m_brown + 0.055 * m_n1.Next();
            double roll = 0.4 + 0.6 * NocAudio.SmoothRandom01(m_seed, 0.9 * t);
            double y = m_lp.Low(m_brown) + 0.5 * roll * m_roll.BandNorm(m_pk.Next(m_n2.Next()));
            mono = m_amp * env * y;
            l = mono * St.PanL.Prev; r = mono * St.PanR.Prev;
        }
    }

    // ====================================================================== sub-bass swells (DREAD 1.2)
    public sealed class SubSwell
    {
        const int K = 5, H = DreadScore.SwellPartials, U = DreadScore.SwellUpperMax;
        readonly double[] m_ph = new double[K * H * 3], m_inc = new double[K * H * 3], m_a = new double[K * H];
        // the small-speaker layer: U partials per swell, a centre and a detuned side voice each (C09)
        readonly double[] m_uph = new double[K * U * 2], m_uinc = new double[K * U * 2], m_ua = new double[K * U];
        readonly double[] m_uScale = new double[K];
        readonly bool[] m_on = new bool[K];
        readonly double[] m_norm = new double[K];
        GainRamp[] m_g = new GainRamp[K];
        NoiseD m_n;
        SvfD m_lpL;
        double m_brown, m_noiseNorm, m_fs;
        public const double Detune = 0.0016, SideAmp = 0.3, NoiseShare = 0.35;
        /// <summary>The small-speaker layer's side voice: detuned by 0.35 % (a 0.8-1.7 Hz beating at 220-490 Hz: the pressure
        /// breathes instead of standing as an organ note), at 0.6 of the centre.</summary>
        public const double UpperDetune = 0.0035, UpperSide = 0.6;
        public const double Norm = 1.0;

        public SubSwell(double fs, double cr, uint seed)
        {
            m_fs = fs;
            m_n = NoiseD.Stream(seed, 0x5B5Bu);
            m_lpL.Set(55.0, 0.7, fs);
            for (int k = 0; k < K; k++)
            {
                m_g[k].Init(0.05, cr);
                double p = 0.0;
                for (int h = 1; h <= H; h++)
                {
                    double a = DreadScore.SwellPartialGain(k, h);
                    m_a[k * H + h - 1] = a;
                    double fh = h * DreadScore.SwellHz[k], dc = DcBlockD.Magnitude(fh, NocturneConstants.DcBlockHz, fs);
                    p += (0.5 + 0.5 * SideAmp * SideAmp) * a * a * dc * dc;   // heard through the master's 20 Hz DC blocker
                    int j = 3 * (k * H + h - 1);
                    m_inc[j] = fh / fs; m_inc[j + 1] = fh * (1.0 - Detune) / fs; m_inc[j + 2] = fh * (1.0 + Detune) / fs;
                }
                m_norm[k] = 1.0 / Math.Sqrt(p + NoiseShare * NoiseShare);
                // the small-speaker layer: amplitudes 1 / sqrt(rank), unit RMS, at SwellUpperDb re the sub; the whole swell keeps its
                // peak at SwellPeakDb (the sub gives up 10 log10(1 + 10^(dB / 10)))
                double pu = 0.0;
                int[] hs = DreadScore.SwellUpperH[k];
                for (int i = 0; i < U; i++)
                {
                    double a = i < hs.Length ? DreadScore.SwellUpperAmp[k][i] : 0.0;
                    m_ua[k * U + i] = a;
                    pu += 0.5 * (1.0 + UpperSide * UpperSide) * a * a;
                    double fh = i < hs.Length ? hs[i] * DreadScore.SwellHz[k] : 0.0;
                    int j = 2 * (k * U + i);
                    m_uinc[j] = fh / fs; m_uinc[j + 1] = fh * (1.0 + UpperDetune) / fs;
                }
                double rel = NocAudio.Db(DreadScore.SwellUpperDb[k]), share = 1.0 / Math.Sqrt(1.0 + rel * rel);
                m_norm[k] *= share;
                m_uScale[k] = pu > 0.0 ? rel / Math.Sqrt(pu) / m_norm[k] * share : 0.0;
            }
            // unit-RMS brown pressure bed: leaky integrator then LP 55 Hz, normalised by its impulse energy (white variance 1/3)
            var lp = new SvfD(); lp.Set(55.0, 0.7, fs); var dcb = new DcBlockD(); dcb.Init(NocturneConstants.DcBlockHz, fs); double b = 0, e = 0;
            int nn = (int)(4.0 * fs);
            for (int i = 0; i < nn; i++) { b = 0.9985 * b + (i == 0 ? 1.0 : 0.0); double y = dcb.Tick(lp.Low(b)); e += y * y; }
            m_noiseNorm = 1.0 / Math.Sqrt(e / 3.0);
        }

        public bool Silent { get { for (int k = 0; k < K; k++) if (!m_g[k].Silent) return false; return true; } }

        public void Control(in ScoreFrame f, double gain)
        {
            for (int k = 0; k < K; k++)
            {
                double env = DreadScore.SwellEnvelope(k, f.S);
                double target = env > 0.0 ? gain * Norm * m_norm[k] * NocAudio.Db(DreadScore.SwellPeakDb[k]) * env : 0.0;
                if (!m_on[k] && target > 0.0)
                {
                    // phases from the absolute sample index: the same tone whatever happened before
                    for (int j = 3 * k * H; j < 3 * (k + 1) * H; j++) { double c = m_inc[j] * f.N; m_ph[j] = c - Math.Floor(c); }
                    for (int j = 2 * k * U; j < 2 * (k + 1) * U; j++) { double c = m_uinc[j] * f.N; m_uph[j] = c - Math.Floor(c); }
                    m_on[k] = true;
                }
                m_g[k].Step(target);
                if (m_on[k] && m_g[k].Silent) m_on[k] = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            l = 0; r = 0;
            bool any = false;
            for (int k = 0; k < K; k++)
            {
                if (!m_on[k]) continue;
                any = true;
                double g = m_g[k].At(u), cl = 0, cr = 0;
                for (int h = 0; h < H; h++)
                {
                    double a = m_a[k * H + h];
                    int j = 3 * (k * H + h);
                    if (a > 0.0)
                    {
                        double c = NocAudio.Sin01(m_ph[j]), sl = NocAudio.Sin01(m_ph[j + 1]), sr = NocAudio.Sin01(m_ph[j + 2]);
                        cl += a * (c + SideAmp * sl); cr += a * (c + SideAmp * sr);
                    }
                    m_ph[j] += m_inc[j]; if (m_ph[j] >= 1.0) m_ph[j] -= 1.0;
                    m_ph[j + 1] += m_inc[j + 1]; if (m_ph[j + 1] >= 1.0) m_ph[j + 1] -= 1.0;
                    m_ph[j + 2] += m_inc[j + 2]; if (m_ph[j + 2] >= 1.0) m_ph[j + 2] -= 1.0;
                }
                double ul = 0, ur = 0;
                for (int i = 0; i < U; i++)
                {
                    double a = m_ua[k * U + i];
                    if (a <= 0.0) continue;
                    int j = 2 * (k * U + i);
                    double c = NocAudio.Sin01(m_uph[j]), sd = NocAudio.Sin01(m_uph[j + 1]);
                    ul += a * (c + UpperSide * sd); ur += a * (c - UpperSide * sd);
                    m_uph[j] += m_uinc[j]; if (m_uph[j] >= 1.0) m_uph[j] -= 1.0;
                    m_uph[j + 1] += m_uinc[j + 1]; if (m_uph[j + 1] >= 1.0) m_uph[j + 1] -= 1.0;
                }
                double us = m_uScale[k];
                l += g * (cl + us * ul); r += g * (cr + us * ur);
            }
            m_brown = 0.9985 * m_brown + m_n.Next();
            double nb = m_lpL.Low(m_brown) * m_noiseNorm;
            if (!any) return;
            double gs = 0.0;
            for (int k = 0; k < K; k++) if (m_on[k]) gs += m_g[k].At(u);
            l += NoiseShare * gs * nb; r += NoiseShare * gs * nb;
        }

        public void Flush() { m_lpL.Flush(); if (Math.Abs(m_brown) < 1e-30) m_brown = 0; }
    }

    // ====================================================================== THE ARK (D-043; replaces the 2.1 hull)
    /// <summary>The Ark's sound (DreadScore's Ark course): a pressure you feel more than hear. Body: the 13.75 Hz stack x 1..12 (LP
    /// 120 Hz) with a detuned side voice on harmonics 2-6 and a brown pressure bed (LP 40 Hz), breathing over 22 s, as a line source
    /// (the share of the sky's half-circle the 5 km hull fills); bow wave: 27.5 Hz x {2..6} and an air band at 70 Hz at the bow with
    /// its Doppler (the long glide down as it passes over: about +8.5 % far ahead, 1.00 overhead, -7.5 % behind); four engine pods along the
    /// belly (27.5 Hz x 1..6, an exhaust band, a slow throb each) passing over one by one with their own Doppler glides, a distance
    /// low-pass each; small-speaker harmonics 9 / 12 / 8 x 27.5 Hz (B3 E4 A3) on the bow and the pods (C09). It also reports the
    /// silence before the bow (Drop) and the cover under the belly (Cover) for NocMixer.</summary>
    public sealed class ArkDrone
    {
        public const int BodyH = 12, SideH = 5, BowH = 5, PodH = 6, UpH = 3, Pods = 4;
        public const double BodyHz = 13.75, EngineHz = 27.5, SideDetune = 0.0021, BreathHz = 0.045;
        static readonly int[] BowHarm = { 2, 3, 4, 5, 6 };
        static readonly double[] BowAmp = { 1.0, 0.8, 0.6, 0.35, 0.3 };
        public static readonly int[] UpHarm = { 9, 12, 8 };
        static readonly double[] UpAmp = { 1.0, 0.6, 0.4 };
        static readonly double[] PodThrobHz = { 0.41, 0.47, 0.53, 0.37 };
        public static readonly int[] BodyUpHarm = { 8, 12, 16 };
        static readonly double[] BodyUpAmp = { 1.0, 0.55, 0.4 };
        public const double BodyUpDetune = 0.0035, BodyUpSide = 0.6;
        readonly double[] m_phBU = new double[6];
        double m_normBU, m_bodyUpRel;
        GainRamp m_gBU;
        VehicleClock m_clock;
        readonly double[] m_phB = new double[BodyH], m_aB = new double[BodyH], m_phS = new double[SideH];
        readonly double[] m_phW = new double[BowH], m_phWU = new double[UpH];
        readonly double[] m_phP = new double[Pods * PodH], m_aP = new double[PodH], m_phPU = new double[Pods * UpH];
        readonly double[] m_incP = new double[Pods], m_lpA = new double[Pods], m_lpY = new double[Pods];
        double m_incB, m_incW, m_fs, m_invFs, m_normB, m_normW, m_normP, m_normU, m_upRel, m_brown, m_brownNorm, m_tSec;
        GainRamp m_gB, m_gW, m_gWU, m_pBL, m_pBR, m_pWL, m_pWR;
        readonly GainRamp[] m_gP = new GainRamp[Pods], m_gPU = new GainRamp[Pods], m_pPL = new GainRamp[Pods], m_pPR = new GainRamp[Pods];
        NoiseD m_nB, m_nW; PinkD m_pkW; SvfD m_bandW, m_lpBed;
        readonly NoiseD[] m_nP = new NoiseD[Pods]; readonly PinkD[] m_pkP = new PinkD[Pods]; readonly SvfD[] m_bandP = new SvfD[Pods];
        bool m_on;
        public double Tau = -1.0, BowDoppler = 1.0, BodyGain, BowGain, PodGainMax, Drop, Cover;
        public readonly double[] PodDoppler = new double[Pods];
        public const double Norm = 1.0;

        public ArkDrone(double fs, double cr, uint seed)
        {
            m_fs = fs; m_invFs = 1.0 / fs;
            m_nB = NoiseD.Stream(seed, 0x4A21u); m_nW = NoiseD.Stream(seed, 0x4A22u);
            m_bandW.Set(70.0, 0.7, fs); m_lpBed.Set(40.0, 0.7, fs);
            for (int k = 0; k < Pods; k++)
            {
                m_nP[k] = NoiseD.Stream(seed, 0x4A30u + (uint)k); m_bandP[k].Set(150.0 * (1.0 + 0.07 * k), 0.7, fs);
                m_gP[k].Init(0.05, cr); m_gPU[k].Init(0.05, cr); m_pPL[k].Init(0.05, cr); m_pPR[k].Init(0.05, cr);
            }
            // powers through the master's 20 Hz DC blocker (the infrasonic fundamental is felt, not counted)
            double pb = 0, pw = 0, pp = 0, pu = 0;
            for (int h = 1; h <= BodyH; h++)
            {
                double f = h * BodyHz, dc = DcBlockD.Magnitude(f, NocturneConstants.DcBlockHz, fs);
                m_aB[h - 1] = (h == 1 ? 0.3 : 1.0) / Math.Pow(h, 0.6) / (1.0 + Math.Pow(f / 120.0, 4));
                double side = h >= 2 && h <= 1 + SideH ? 0.5 : 0.0;
                pb += 0.5 * m_aB[h - 1] * m_aB[h - 1] * (1.0 + side * side) * dc * dc;
            }
            for (int h = 0; h < BowH; h++) { double dc = DcBlockD.Magnitude(BowHarm[h] * EngineHz, NocturneConstants.DcBlockHz, fs); pw += 0.5 * BowAmp[h] * BowAmp[h] * dc * dc; }
            for (int h = 1; h <= PodH; h++) { double f = h * EngineHz, dc = DcBlockD.Magnitude(f, NocturneConstants.DcBlockHz, fs); m_aP[h - 1] = 1.0 / Math.Sqrt(h) / (1.0 + (f / 200.0) * (f / 200.0)); pp += 0.5 * m_aP[h - 1] * m_aP[h - 1] * dc * dc; }
            for (int h = 0; h < UpH; h++) pu += 0.5 * UpAmp[h] * UpAmp[h];
            m_normB = 1.0 / Math.Sqrt(pb + 0.3 * 0.3); m_normW = 1.0 / Math.Sqrt(pw + 0.4 * 0.4); m_normP = 1.0 / Math.Sqrt(pp + 0.5 * 0.5);
            m_normU = 1.0 / Math.Sqrt(pu); m_upRel = NocAudio.Db(DreadScore.HullUpperDb);
            double pbu = 0.0; for (int h = 0; h < 3; h++) pbu += 0.5 * (1.0 + BodyUpSide * BodyUpSide) * BodyUpAmp[h] * BodyUpAmp[h];
            m_normBU = 1.0 / Math.Sqrt(pbu); m_bodyUpRel = NocAudio.Db(DreadScore.ArkBodyUpperDb); m_gBU.Init(0.05, cr);
            // unit-RMS brown pressure bed: leaky integrator then LP 40 Hz (white variance 1/3), through the DC blocker
            var lp = new SvfD(); lp.Set(40.0, 0.7, fs); var dcb = new DcBlockD(); dcb.Init(NocturneConstants.DcBlockHz, fs); double b = 0, e = 0;
            int nn = (int)(4.0 * fs);
            for (int i = 0; i < nn; i++) { b = 0.9985 * b + (i == 0 ? 1.0 : 0.0); double y = dcb.Tick(lp.Low(b)); e += y * y; }
            m_brownNorm = 1.0 / Math.Sqrt(e / 3.0);
            m_gB.Init(0.05, cr); m_gW.Init(0.05, cr); m_gWU.Init(0.05, cr); m_pBL.Init(0.05, cr); m_pBR.Init(0.05, cr); m_pWL.Init(0.05, cr); m_pWR.Init(0.05, cr);
        }

        static double SwellNow(double s) { double sw = 0.0; for (int k = 0; k < DreadScore.SwellS.Length; k++) sw = Math.Max(sw, DreadScore.SwellEnvelope(k, s)); return sw; }

        public bool Silent
        {
            get
            {
                if (!m_gB.Silent || !m_gW.Silent || !m_gWU.Silent || !m_gBU.Silent) return false;
                for (int k = 0; k < Pods; k++) if (!m_gP[k].Silent || !m_gPU[k].Silent) return false;
                return true;
            }
        }

        public void Control(in ScoreFrame f, double gain)
        {
            m_clock.Update(f.N, f.S, f.Fs, DreadScore.ArkLaunchS);
            double tau = m_clock.Tau(f.N, f.Fs);
            Tau = tau;
            double gb = 0.0, gw = 0.0, dopW = 1.0, pmax = 0.0;
            double lx = f.X, ly = NocturneConstants.EyeHeight, lz = f.Z;
            Drop = DreadScore.ArkDropShape(tau, lz); Cover = DreadScore.ArkCover(tau, lx, lz);
            double amp = gain * Norm * NocAudio.Db(DreadScore.ArkPeakDb + DreadScore.ArkGardenTrimDb * NocturneScore.BeatWeight(4, f.S));
            bool live = tau >= 0.0 && tau < DreadScore.ArkEndTau1;
            double fade = live ? NocMath.Smoothstep(0.0, 8.0, tau) * (1.0 - NocMath.Smootherstep(DreadScore.ArkEndTau0, DreadScore.ArkEndTau1, tau)) : 0.0;
            double u = DreadScore.ArkVz(tau), bow = DreadScore.ArkBowZ(tau), stern = DreadScore.ArkSternZ(tau);
            double zLo = Math.Min(bow, stern), zHi = Math.Max(bow, stern);
            if (live)
            {
                // body: the share of the half-circle the hull subtends across the listener's zenith plane
                double dx = DreadScore.ArkX - lx, dy = DreadScore.ArkBellyY + 40.0 - ly, d = Math.Sqrt(dx * dx + dy * dy);
                double ang = Math.Atan((zHi - lz) / d) - Math.Atan((zLo - lz) / d);
                gb = 0.8 * Math.Pow(Math.Max(0.0, ang / Math.PI), 0.8) * fade;
                NocAudio.Pan(f.PanTo(DreadScore.ArkX, NocMath.Clamp(lz, zLo, zHi)) * 0.4, out double bl, out double br);
                m_pBL.Step(bl); m_pBR.Step(br);
                // the bow wave
                double wy = DreadScore.ArkBellyY + 60.0, wz = DreadScore.ArkZBehindBow(tau, 40.0);
                double ex = DreadScore.ArkX - lx, ey = wy - ly, ez = wz - lz, re = Math.Sqrt(ex * ex + ey * ey + ez * ez);
                gw = DreadScore.ArkBowGain / (1.0 + (re / DreadScore.ArkBowR0) * (re / DreadScore.ArkBowR0)) * fade;
                dopW = DreadScore.PointDoppler(DreadScore.ArkX, wy, wz, u, lx, ly, lz);
                NocAudio.Pan(f.PanTo(DreadScore.ArkX, wz) * 0.7, out double wl, out double wr);
                m_pWL.Step(wl); m_pWR.Step(wr);
                // the engine pods
                for (int k = 0; k < Pods; k++)
                {
                    double pz = DreadScore.ArkZBehindBow(tau, DreadScore.ArkPodFromBow[k]), py = DreadScore.ArkBellyY - 15.0;
                    double qx = DreadScore.ArkX + (k % 2 == 0 ? -90.0 : 90.0) - lx, qy = py - ly, qz = pz - lz, rq = Math.Sqrt(qx * qx + qy * qy + qz * qz);
                    double g = DreadScore.ArkPodGain[k] * 0.7 / (1.0 + (rq / DreadScore.ArkPodR0) * (rq / DreadScore.ArkPodR0)) * fade;
                    double dop = DreadScore.PointDoppler(DreadScore.ArkX, py, pz, u, lx, ly, lz);
                    PodDoppler[k] = dop; m_incP[k] = EngineHz * dop * m_invFs;
                    double lpHz = 2400.0 / (1.0 + rq / 150.0);
                    m_lpA[k] = 1.0 - Math.Exp(-2.0 * Math.PI * lpHz * m_invFs);
                    m_gP[k].Step(amp * g); m_gPU[k].Step(amp * g * m_upRel * m_normU * (1.0 - DreadScore.ArkUpperSwellDuck * SwellNow(f.S)));
                    NocAudio.Pan(f.PanTo(DreadScore.ArkX + (k % 2 == 0 ? -90.0 : 90.0), pz) * 0.6, out double pl, out double pr);
                    m_pPL[k].Step(pl); m_pPR[k].Step(pr);
                    if (g > pmax) pmax = g;
                }
            }
            else for (int k = 0; k < Pods; k++) { m_gP[k].Step(0.0); m_gPU[k].Step(0.0); }
            BodyGain = gb; BowGain = gw; BowDoppler = dopW; PodGainMax = pmax;
            double sw = 0.0; for (int k = 0; k < DreadScore.SwellS.Length; k++) sw = Math.Max(sw, DreadScore.SwellEnvelope(k, f.S));
            double upDuck = 1.0 - DreadScore.ArkUpperSwellDuck * sw;
            m_gB.Step(amp * gb); m_gW.Step(amp * gw); m_gWU.Step(amp * gw * m_upRel * m_normU * upDuck); m_gBU.Step(amp * gb * m_bodyUpRel * m_normBU * upDuck);
            if (!m_on && (gb > 0.0 || gw > 0.0 || pmax > 0.0))
            {
                m_on = true;
                for (int h = 0; h < BodyH; h++) { double c = (h + 1) * BodyHz * m_invFs * f.N; m_phB[h] = c - Math.Floor(c); }
                for (int h = 0; h < SideH; h++) { double c = (h + 2) * BodyHz * (1.0 + SideDetune) * m_invFs * f.N; m_phS[h] = c - Math.Floor(c); }
                for (int h = 0; h < BowH; h++) m_phW[h] = NocAudio.Hash01(0xA4C0UL, (ulong)h);
                for (int h = 0; h < UpH; h++) m_phWU[h] = NocAudio.Hash01(0xA4C1UL, (ulong)h);
                for (int h = 0; h < 6; h++) m_phBU[h] = NocAudio.Hash01(0xA4C4UL, (ulong)h);
                for (int j = 0; j < Pods * PodH; j++) m_phP[j] = NocAudio.Hash01(0xA4C2UL, (ulong)j);
                for (int j = 0; j < Pods * UpH; j++) m_phPU[j] = NocAudio.Hash01(0xA4C3UL, (ulong)j);
                m_pBL.Snap(m_pBL.Cur); m_pBR.Snap(m_pBR.Cur); m_pWL.Snap(m_pWL.Cur); m_pWR.Snap(m_pWR.Cur);
                for (int k = 0; k < Pods; k++) { m_pPL[k].Snap(m_pPL[k].Cur); m_pPR[k].Snap(m_pPR[k].Cur); }
            }
            if (m_on && Silent && gb == 0.0 && gw == 0.0 && pmax == 0.0) m_on = false;
            m_incB = BodyHz * m_invFs; m_incW = EngineHz * dopW * m_invFs;
            m_tSec = f.T;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r, out double mono)
        {
            l = 0; r = 0; mono = 0;
            if (!m_on) return;
            // body + its detuned side voice (a slow beating: the pressure breathes) + the brown bed
            double b = 0.0;
            for (int h = 0; h < BodyH; h++) { b += m_aB[h] * NocAudio.Sin01(m_phB[h]); m_phB[h] += (h + 1) * m_incB; if (m_phB[h] >= 1.0) m_phB[h] -= Math.Floor(m_phB[h]); }
            double sd = 0.0;
            for (int h = 0; h < SideH; h++) { sd += m_aB[h + 1] * NocAudio.Sin01(m_phS[h]); m_phS[h] += (h + 2) * m_incB * (1.0 + SideDetune); if (m_phS[h] >= 1.0) m_phS[h] -= Math.Floor(m_phS[h]); }
            m_brown = 0.9985 * m_brown + m_nB.Next();
            double bed = m_lpBed.Low(m_brown) * m_brownNorm;
            double breath = 1.0 + 0.15 * NocAudio.Sin01(BreathHz * m_tSec);
            double gb = m_gB.At(u) * m_normB * breath;
            double bL = gb * (b + 0.5 * sd + 0.3 * bed), bR = gb * (b - 0.5 * sd + 0.3 * bed);
            // the body's small-speaker layer (A3 E4 A4 with a detuned side voice: decorrelated left / right)
            double gbu = m_gBU.At(u) * breath;
            if (gbu != 0.0)
            {
                double uc = 0.0, us = 0.0;
                for (int h = 0; h < 3; h++)
                {
                    double f0 = BodyUpHarm[h] * EngineHz * m_invFs;
                    uc += BodyUpAmp[h] * NocAudio.Sin01(m_phBU[2 * h]); us += BodyUpAmp[h] * NocAudio.Sin01(m_phBU[2 * h + 1]);
                    m_phBU[2 * h] += f0; if (m_phBU[2 * h] >= 1.0) m_phBU[2 * h] -= 1.0;
                    m_phBU[2 * h + 1] += f0 * (1.0 + BodyUpDetune); if (m_phBU[2 * h + 1] >= 1.0) m_phBU[2 * h + 1] -= 1.0;
                }
                bL += gbu * (uc + BodyUpSide * us); bR += gbu * (uc - BodyUpSide * us);
            }
            // bow wave
            double w = 0.0;
            for (int h = 0; h < BowH; h++) { w += BowAmp[h] * NocAudio.Sin01(m_phW[h]); m_phW[h] += BowHarm[h] * m_incW; if (m_phW[h] >= 1.0) m_phW[h] -= Math.Floor(m_phW[h]); }
            w += 0.4 * m_bandW.BandNorm(m_pkW.Next(m_nW.Next())) * 1.4;
            double wu = 0.0;
            for (int h = 0; h < UpH; h++) { wu += UpAmp[h] * NocAudio.Sin01(m_phWU[h]); m_phWU[h] += UpHarm[h] * m_incW; if (m_phWU[h] >= 1.0) m_phWU[h] -= Math.Floor(m_phWU[h]); }
            double wo = m_gW.At(u) * m_normW * w + m_gWU.At(u) * wu;
            l = bL * m_pBL.At(u) + wo * m_pWL.At(u); r = bR * m_pBR.At(u) + wo * m_pWR.At(u);
            // engine pods
            for (int k = 0; k < Pods; k++)
            {
                double gp = m_gP[k].At(u), gq = m_gPU[k].At(u);
                if (gp == 0.0 && gq == 0.0) continue;
                double e = 0.0, inc = m_incP[k];
                int j0 = k * PodH;
                for (int h = 0; h < PodH; h++) { int j = j0 + h; e += m_aP[h] * NocAudio.Sin01(m_phP[j]); m_phP[j] += (h + 1) * inc; if (m_phP[j] >= 1.0) m_phP[j] -= Math.Floor(m_phP[j]); }
                e += 0.5 * m_bandP[k].BandNorm(m_pkP[k].Next(m_nP[k].Next())) * 1.4;
                double up = 0.0; int ju = k * UpH;
                for (int h = 0; h < UpH; h++) { int j = ju + h; up += UpAmp[h] * NocAudio.Sin01(m_phPU[j]); m_phPU[j] += UpHarm[h] * inc; if (m_phPU[j] >= 1.0) m_phPU[j] -= Math.Floor(m_phPU[j]); }
                double throb = 1.0 + 0.2 * NocAudio.Sin01(PodThrobHz[k] * m_tSec + 0.25 * k);
                double x = gp * m_normP * throb * e + gq * up;
                m_lpY[k] += m_lpA[k] * (x - m_lpY[k]);
                l += m_lpY[k] * m_pPL[k].At(u); r += m_lpY[k] * m_pPR[k].At(u);
            }
            mono = 0.5 * (l + r);
        }

        public void Flush()
        {
            m_bandW.Flush(); m_pkW.Flush(); m_lpBed.Flush();
            for (int k = 0; k < Pods; k++) { m_bandP[k].Flush(); m_pkP[k].Flush(); if (Math.Abs(m_lpY[k]) < 1e-30) m_lpY[k] = 0.0; }
            if (Math.Abs(m_brown) < 1e-30) m_brown = 0.0;
        }
    }

    // ====================================================================== elevator cabins and gantries (DREAD 2.2)
    public sealed class MachineHum
    {
        readonly int m_i;
        double m_ph1, m_ph2, m_ph3, m_phW, m_f, m_fs, m_invFs, m_t;
        GainRamp m_gL, m_gR;
        NoiseD m_n; SvfD m_bp;
        bool m_on;
        public double Gain;
        public const double Norm = 1.0;

        public MachineHum(int i, double fs, double cr, uint seed)
        {
            m_i = i; m_fs = fs; m_invFs = 1.0 / fs; m_f = DreadScore.HumHz[i];
            m_gL.Init(0.05, cr); m_gR.Init(0.05, cr);
            m_n = NoiseD.Stream(seed, 0x3A70u + (uint)i); m_bp.Set(m_f * 2.6, 2.0, fs);
        }

        public void Control(in ScoreFrame f, double gain)
        {
            // DR's cabin / gantry on the picture's clock (DreadCabinCourse via DreadScore.HumSource)
            double g = DreadScore.HumSource(m_i, f.SceneT, f.X, NocturneConstants.EyeHeight, f.Z, out double x, out double y, out double z);
            if (g < 0.01) g = 0.0;
            Gain = g;
            double amp = gain * Norm * NocAudio.Db(DreadScore.HumDb(m_i)) * g;
            NocAudio.Pan(f.PanTo(x, z) * 0.8, out double pl, out double pr);
            if (!m_on && amp > 0.0)
            {
                m_on = true;
                double c = m_f * m_invFs * f.N; m_ph1 = c - Math.Floor(c); m_ph2 = 2.0 * m_ph1 % 1.0; m_ph3 = 3.0 * m_ph1 % 1.0; m_phW = 0.0;
            }
            m_gL.Step(amp * pl); m_gR.Step(amp * pr);
            if (m_on && amp == 0.0 && m_gL.Silent && m_gR.Silent) m_on = false;
            m_t = f.T;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            l = 0; r = 0;
            if (!m_on) return;
            double inc = m_f * m_invFs * (1.0 + 0.005 * NocAudio.Sin01(0.07 * m_t + 0.13 * m_i));
            double whineAm = 0.6 + 0.4 * NocAudio.Sin01(0.8 * m_t);
            double y = 0.75 * NocAudio.Sin01(m_ph1) + 0.45 * NocAudio.Sin01(m_ph2) + 0.25 * NocAudio.Sin01(m_ph3)
                     + 0.18 * whineAm * NocAudio.Sin01(m_phW) + 0.35 * m_bp.BandNorm(m_n.Next());
            m_ph1 += inc; if (m_ph1 >= 1.0) m_ph1 -= 1.0;
            m_ph2 += 2.0 * inc; if (m_ph2 >= 1.0) m_ph2 -= 1.0;
            m_ph3 += 3.0 * inc; if (m_ph3 >= 1.0) m_ph3 -= 1.0;
            m_phW += 8.2 * inc; if (m_phW >= 1.0) m_phW -= 1.0;
            l = m_gL.At(u) * y; r = m_gR.At(u) * y;
        }

        public void Flush() { m_bp.Flush(); }
    }

    // ====================================================================== the bridge in the gusts (DREAD 4.3)
    public sealed class BridgeGust
    {
        GainRamp m_gW, m_gC;
        NoiseD m_n1, m_n2, m_n3;
        SvfD m_whL, m_whR, m_c1, m_c2;
        double m_fs, m_invFs, m_lastHz, m_sceneT;
        readonly ulong m_seed;
        public double GustN, Creak;
        public const double Norm = 1.0;

        public BridgeGust(double fs, double cr, uint seed)
        {
            m_fs = fs; m_invFs = 1.0 / fs; m_seed = DspHash.Mix(seed ^ 0xB61DUL);
            m_gW.Init(0.05, cr); m_gC.Init(0.05, cr);
            m_n1 = NoiseD.Stream(seed, 0xB601u); m_n2 = NoiseD.Stream(seed, 0xB602u); m_n3 = NoiseD.Stream(seed, 0xB603u);
            m_whL.Set(1200.0, 40.0, fs); m_whR.CopyCoefficients(in m_whL);
            m_c1.Set(160.0, 4.0, fs); m_c2.Set(410.0, 6.0, fs);
        }

        public bool Silent => m_gW.Silent && m_gC.Silent;

        public void Control(in ScoreFrame f, double gain, ulong windSeed)
        {
            double g4 = NocturneScore.RainGustWeight(f.S);
            double gust = WindBed.Gust(windSeed, f.T, f.SceneT, 1.0);
            double gn = NocMath.Clamp01((gust - 0.4) / 0.6);
            GustN = gn;
            double ph = NocAudio.TwoPi * DreadScore.SwayHz * f.SceneT;
            double c = -Math.Cos(2.0 * ph); c = c > 0.0 ? c * c * c * c * c * c : 0.0;
            double sway = DreadScore.SwayAmplitude(f.S, f.SceneT);
            Creak = c * sway;
            double amp = gain * Norm * NocAudio.Db(DreadScore.GustPeakDb) * g4;
            m_gW.Step(amp * gn * gn * 0.8);
            m_gC.Step(amp * c * sway);
            double hz = 1050.0 + 550.0 * gn;
            if (Math.Abs(hz - m_lastHz) > 2.0) { m_whL.Set(hz, 40.0, m_fs); m_whR.CopyCoefficients(in m_whL); m_lastHz = hz; }
            m_sceneT = f.SceneT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            l = 0; r = 0;
            if (Silent) return;
            double gw = m_gW.At(u), gc = m_gC.At(u);
            double wl = m_whL.BandNorm(m_n1.Next()), wr = m_whR.BandNorm(m_n2.Next());
            double nz = m_n3.Next();
            double c1 = m_c1.BandNorm(nz), c2 = m_c2.BandNorm(nz);
            // stick-slip grains at 22 Hz with hashed amplitudes (time from the scene clock: the sway's own clock)
            double gp = 22.0 * (m_sceneT + u * NocMixer.Block * m_invFs); long gi = (long)Math.Floor(gp); double gf = gp - gi;
            double ga = NocAudio.Hash01(m_seed, unchecked((ulong)gi)); ga *= ga;
            double gwin = 0.5 - 0.5 * Math.Cos(NocAudio.TwoPi * gf); gwin *= gwin;
            double cr = (c1 + 0.7 * c2) * ga * gwin * 2.4;
            l = gw * wl * 4.0 + gc * cr; r = gw * wr * 4.0 + gc * cr;
        }

        public void Flush() { m_whL.Flush(); m_whR.Flush(); m_c1.Flush(); m_c2.Flush(); }
    }

    // ====================================================================== the Gulf echoes (DREAD 1.1)
    public sealed class EchoBus
    {
        readonly float[] m_buf;
        readonly int m_mask;
        int m_w;
        readonly double m_fs;
        readonly double[] m_d0 = new double[DreadScore.EchoTapCount], m_d1 = new double[DreadScore.EchoTapCount];
        readonly double[] m_lpC = new double[DreadScore.EchoTapCount], m_lpY = new double[DreadScore.EchoTapCount];
        readonly double[] m_fade = new double[DreadScore.EchoTapCount], m_fade0 = new double[DreadScore.EchoTapCount];
        readonly double[] m_target = new double[DreadScore.EchoTapCount];
        readonly bool[] m_snap = new bool[DreadScore.EchoTapCount];
        GainRamp[] m_gL = new GainRamp[DreadScore.EchoTapCount], m_gR = new GainRamp[DreadScore.EchoTapCount];
        bool m_init;
        double m_fadeStep;
        public double Weight;

        public EchoBus(double fs, double cr)
        {
            m_fs = fs;
            int need = (int)Math.Ceiling((DreadScore.MaxEchoSeconds + 0.2) * fs) + 8, size = 1;
            while (size < need) size <<= 1;
            m_buf = new float[size]; m_mask = size - 1;
            for (int k = 0; k < DreadScore.EchoTapCount; k++) { m_gL[k].Init(0.05, cr); m_gR[k].Init(0.05, cr); m_fade[k] = 1.0; m_fade0[k] = 1.0; }
            m_fadeStep = 1.0 / Math.Max(1.0, 0.02 * cr);   // 20 ms
        }

        /// <summary>Delay of tap k now (samples, the block's end value).</summary>
        public double DelaySamples(int k) => m_d1[k];

        public void Control(in ScoreFrame f)
        {
            Weight = DreadScore.EchoWeight(f.X);
            for (int k = 0; k < DreadScore.EchoTapCount; k++)
            {
                DreadScore.EchoTap(k, f.X, out double delay, out double gain, out double lp, out double ix);
                double target = Math.Min(delay * m_fs, m_mask - 4);
                m_target[k] = target;
                m_d0[k] = m_d1[k];
                m_fade0[k] = m_fade[k];
                if (!m_init) { m_d0[k] = target; m_d1[k] = target; }
                else if (m_snap[k])
                {
                    // fading out for a jump: once a whole block has been silent, move the head; then fade back in
                    if (m_fade[k] == 0.0 && m_fade0[k] == 0.0) { m_d0[k] = target; m_d1[k] = target; m_snap[k] = false; }
                    else m_fade[k] = Math.Max(0.0, m_fade[k] - m_fadeStep);
                }
                else if (Math.Abs(target - m_d1[k]) > 0.05 * m_fs) { m_snap[k] = true; m_fade[k] = Math.Max(0.0, m_fade[k] - m_fadeStep); }
                else
                {
                    m_d1[k] += 0.5 * (target - m_d1[k]);   // glide (continuous walking moves the tap by < 1 sample per block)
                    m_fade[k] = Math.Min(1.0, m_fade[k] + m_fadeStep);
                }
                // the tap's low-pass glides too (a coefficient step would kink the waveform); a moving head snaps it while silent
                double c = 1.0 - Math.Exp(-2.0 * Math.PI * Math.Min(lp, 0.45 * m_fs) / m_fs);
                if (!m_init || (m_fade[k] == 0.0 && m_fade0[k] == 0.0)) m_lpC[k] = c; else m_lpC[k] += 0.05 * (c - m_lpC[k]);
                NocAudio.Pan(f.PanTo(ix, f.Z) * 0.9, out double pl, out double pr);
                double g = gain * Weight;
                if (!m_init) { m_gL[k].Snap(g * pl); m_gR[k].Snap(g * pr); }
                else { m_gL[k].Step(g * pl); m_gR[k].Step(g * pr); }
            }
            m_init = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, double x, out double l, out double r)
        {
            m_buf[m_w] = (float)x;
            l = 0; r = 0;
            for (int k = 0; k < DreadScore.EchoTapCount; k++)
            {
                double d = m_d0[k] + u * (m_d1[k] - m_d0[k]);
                double rp = m_w - d;
                double fl = Math.Floor(rp); int i0 = (int)fl; double fr = rp - fl;
                double a = m_buf[i0 & m_mask], b = m_buf[(i0 + 1) & m_mask];
                double y = a + fr * (b - a);
                m_lpY[k] += m_lpC[k] * (y - m_lpY[k]);
                double fade = m_fade0[k] + u * (m_fade[k] - m_fade0[k]);
                double o = m_lpY[k] * fade;
                l += o * m_gL[k].At(u); r += o * m_gR[k].At(u);
            }
            m_w = (m_w + 1) & m_mask;
        }

        public void Clear() { Array.Clear(m_buf, 0, m_buf.Length); for (int k = 0; k < m_lpY.Length; k++) m_lpY[k] = 0.0; }
        public void Flush() { for (int k = 0; k < m_lpY.Length; k++) if (Math.Abs(m_lpY[k]) < 1e-30) m_lpY[k] = 0.0; }
    }

    // ====================================================================== the stem E
    public sealed class DreadLayer
    {
        public const double SendEnv = 0.25, SendEchoEnv = 0.1;
        readonly HornVoice m_horn; readonly StrikeVoice[] m_strikes; readonly GroanVoice[] m_groans; readonly CreakVoice m_creak;
        readonly ChimeVoice m_chime; readonly RumbleVoice[] m_rumbles;
        readonly SubSwell m_swell; readonly ArkDrone m_ark; readonly MachineHum[] m_hums; readonly BridgeGust m_gust;
        readonly GroanVoice[] m_arkGroans;
        readonly bool[] m_arkFired;
        double m_lastTau = -1.0;
        readonly EchoBus m_echo;
        readonly bool[] m_fired;
        readonly ulong m_windSeed;
        bool m_seen; double m_lastS;
        public int Fired, Skipped;
        /// <summary>Solo of the parts (tests and the harness): bit 0 one-shots (the Ark's groans and calls too), 1 swells, 2 the Ark's
        /// drone, 3 hums, 4 gust, 5 echo.</summary>
        public int PartMask = -1;

        public DreadLayer(double fs, double cr, uint seed)
        {
            m_horn = new HornVoice(fs, cr);
            m_strikes = new[] { new StrikeVoice(fs, cr), new StrikeVoice(fs, cr) };
            m_groans = new[] { new GroanVoice(fs, cr), new GroanVoice(fs, cr) };
            m_creak = new CreakVoice(fs, cr); m_chime = new ChimeVoice(fs, cr);
            m_rumbles = new[] { new RumbleVoice(fs, cr), new RumbleVoice(fs, cr) };
            m_swell = new SubSwell(fs, cr, seed); m_ark = new ArkDrone(fs, cr, seed);
            m_arkGroans = new[] { new GroanVoice(fs, cr), new GroanVoice(fs, cr) };
            m_arkFired = new bool[DreadScore.ArkGroanTau.Length];
            m_hums = new MachineHum[DreadScore.HumCount];
            for (int i = 0; i < m_hums.Length; i++) m_hums[i] = new MachineHum(i, fs, cr, seed);
            m_gust = new BridgeGust(fs, cr, seed);
            m_echo = new EchoBus(fs, cr);
            m_fired = new bool[DreadScore.Events.Length];
            m_windSeed = DspHash.Mix(seed ^ 0x3171DUL);   // WindBed's own seed: the same gusts as the wind layer
        }

        public ArkDrone Ark => m_ark;
        /// <summary>The silence before the Ark's bow (0..1) and the cover under its belly (0..1), for NocMixer (the last control block's).</summary>
        public double ArkDrop => m_ark.Drop;
        public double ArkCover => m_ark.Cover;
        public EchoBus Echo => m_echo;
        public BridgeGust Gust => m_gust;

        public void Control(in ScoreFrame f, double gain)
        {
            double s = f.S;
            var ev = DreadScore.Events;
            bool jump = !m_seen || Math.Abs(s - m_lastS) > 5.0;
            for (int i = 0; i < ev.Length; i++)
            {
                if (ev[i].Ark != 0) continue;   // the Ark's calls fire on its clock (ArkGroans)
                if (jump) { m_fired[i] = s >= ev[i].S; continue; }
                if (m_fired[i]) { if (s < ev[i].S - NocturneScore.VehicleResetBelow) m_fired[i] = false; continue; }
                if (m_lastS < ev[i].S && s >= ev[i].S) { m_fired[i] = true; Fire(in ev[i], in f, gain); }
            }
            m_seen = true; m_lastS = s;
            m_horn.St.Control(in f);
            for (int i = 0; i < m_strikes.Length; i++) m_strikes[i].St.Control(in f);
            for (int i = 0; i < m_groans.Length; i++) m_groans[i].St.Control(in f);
            m_creak.Control(in f, f.N); m_chime.St.Control(in f);
            for (int i = 0; i < m_rumbles.Length; i++) m_rumbles[i].St.Control(in f);
            m_swell.Control(in f, gain);
            m_ark.Control(in f, gain);
            ArkGroans(in f, gain, jump);
            for (int i = 0; i < m_arkGroans.Length; i++) m_arkGroans[i].St.Control(in f);
            for (int i = 0; i < m_hums.Length; i++) m_hums[i].Control(in f, gain);
            m_gust.Control(in f, gain, m_windSeed);
            m_echo.Control(in f);
        }

        /// <summary>The Ark's groans on its own clock (DreadScore.ArkGroanTau): fired when tau first crosses a slot, from the belly over
        /// the listener, heard after the flight time; a jump (or the clock's own reset) marks the past ones done.</summary>
        void ArkGroans(in ScoreFrame f, double gain, bool jump)
        {
            double tau = m_ark.Tau;
            var slots = DreadScore.ArkGroanTau;
            bool tj = jump || m_lastTau < 0.0 || tau < m_lastTau || tau - m_lastTau > 1.0;
            for (int k = 0; k < slots.Length; k++)
            {
                if (tau < 0.0) { m_arkFired[k] = false; continue; }
                if (tj) { m_arkFired[k] = tau >= slots[k]; continue; }
                if (m_arkFired[k] || tau < slots[k]) continue;
                m_arkFired[k] = true;
                var v = !m_arkGroans[0].St.Active ? m_arkGroans[0] : (!m_arkGroans[1].St.Active ? m_arkGroans[1] : null);
                if (v == null) { Skipped++; continue; }
                double d = DreadScore.ArkGroanSource(k, tau, f.X, f.Z, out double x, out _, out double z);
                var e = new DreadEvent { S = f.S, Kind = DreadKind.Groan, Seq = 100 + k, Hz = DreadScore.ArkGroanHz(k), Db = DreadScore.ArkGroanLevelDb(d), Seconds = DreadScore.ArkGroanSeconds(k) };
                v.Start(in e, f.N + (long)Math.Round(d / DreadScore.SoundSpeed * f.Fs), in f, NocAudio.Db(e.Db) * gain, x, z, d);
                Fired++; ArkGroansFired++;
            }
            // the Ark's calls on the same clock (DreadScore.ArkCallTau of the table's s)
            var ev = DreadScore.Events;
            for (int i = 0; i < ev.Length; i++)
            {
                if (ev[i].Ark == 0) continue;
                double ct = DreadScore.ArkCallTau(ev[i].S);
                if (tau < 0.0) { m_fired[i] = false; continue; }
                if (tj) { m_fired[i] = tau >= ct; continue; }
                if (m_fired[i] || tau < ct) continue;
                m_fired[i] = true; Fire(in ev[i], in f, gain);
            }
            m_lastTau = tau;
        }
        public int ArkGroansFired;

        void Fire(in DreadEvent e, in ScoreFrame f, double gain)
        {
            long on = f.N + (long)Math.Round(e.Delay * f.Fs);
            double g = NocAudio.Db(e.Db) * gain;
            switch (e.Kind)
            {
                case DreadKind.Horn:
                {
                    if (m_horn.St.Active) { Skipped++; break; }
                    if (e.Ark != 0 && m_ark.Tau >= 0.0)
                    {
                        // the Ark's call from its bow / stern now: heard after the flight time, at its distance
                        DreadScore.ArkHornSource(e.Ark, m_ark.Tau, f.X, f.Z, out double hx, out double hy, out double hz, out double hd, out _);
                        if (hd > DreadScore.ArkHornMaxDist) { Skipped++; break; }
                        var h = e; h.X = hx; h.Y = hy; h.Z = hz; h.Delay = hd / DreadScore.SoundSpeed;
                        m_horn.Start(in h, f.N + (long)Math.Round(h.Delay * f.Fs), in f, NocAudio.Db(e.Db + DreadScore.ArkHornDistanceDb(hd)) * gain);
                    }
                    else m_horn.Start(in e, on, in f, g);
                    Fired++; break;
                }
                case DreadKind.Strike:
                {
                    var v = !m_strikes[0].St.Active ? m_strikes[0] : (!m_strikes[1].St.Active ? m_strikes[1] : null);
                    if (v == null) { Skipped++; break; }
                    double dx = e.X - f.X, dy = e.Y - NocturneConstants.EyeHeight, dz = e.Z - f.Z;
                    v.Start(in e, on, in f, g, Math.Sqrt(dx * dx + dy * dy + dz * dz)); Fired++; break;
                }
                case DreadKind.Groan:
                {
                    var v = !m_groans[0].St.Active ? m_groans[0] : (!m_groans[1].St.Active ? m_groans[1] : null);
                    if (v == null) { Skipped++; break; }
                    double d = DreadScore.NearestMass(f.S, e.Seq, out double x, out _, out double z);
                    double gd = NocAudio.Db(-8.0 * Math.Log10(1.0 + d / 100.0));
                    v.Start(in e, on + (long)Math.Round(d / DreadScore.SoundSpeed * f.Fs), in f, g * gd, x, z, d); Fired++; break;
                }
                case DreadKind.Creak:
                {
                    if (m_creak.St.Active) { Skipped++; break; }
                    double dx = e.X - f.X, dy = e.Y - NocturneConstants.EyeHeight, dz = e.Z - f.Z;
                    m_creak.Start(in e, on, in f, g, Math.Sqrt(dx * dx + dy * dy + dz * dz)); Fired++; break;
                }
                case DreadKind.Chime: if (!m_chime.St.Active) { m_chime.Start(in e, on, in f, g); Fired++; } else Skipped++; break;
                case DreadKind.Rumble:
                {
                    var v = !m_rumbles[0].St.Active ? m_rumbles[0] : (!m_rumbles[1].St.Active ? m_rumbles[1] : null);
                    if (v == null) { Skipped++; break; }
                    v.Start(in e, on, in f, g); Fired++; break;
                }
            }
        }

        /// <summary>One sample of the stem: dry + echoes (l, r) and the send into the environment reverb (el, er).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, long n, out double l, out double r, out double el, out double er)
        {
            int pm = PartMask;
            double dl = 0, dr = 0, echoIn = 0, a, b, m;
            if ((pm & 1) != 0)
            {
                if (m_horn.St.Active) { m_horn.Tick(n, out a, out b, out m); dl += a; dr += b; echoIn += m; }
                for (int k = 0; k < m_strikes.Length; k++) if (m_strikes[k].St.Active) { m_strikes[k].Tick(n, out a, out b, out m); dl += a; dr += b; echoIn += m; }
                for (int k = 0; k < m_groans.Length; k++) if (m_groans[k].St.Active) { m_groans[k].Tick(n, out a, out b, out m); dl += a; dr += b; echoIn += 0.8 * m; }
                if (m_creak.St.Active) { m_creak.Tick(n, out a, out b, out m); dl += a; dr += b; echoIn += m; }
                if (m_chime.St.Active) { m_chime.Tick(n, out a, out b, out m); dl += a; dr += b; }
                for (int k = 0; k < m_rumbles.Length; k++) if (m_rumbles[k].St.Active) { m_rumbles[k].Tick(n, out a, out b, out m); dl += a; dr += b; echoIn += 0.3 * m; }
            }
            // the Ark's groans tick whatever the solo mask (their voices free themselves at the end)
            for (int k = 0; k < m_arkGroans.Length; k++) if (m_arkGroans[k].St.Active) { m_arkGroans[k].Tick(n, out a, out b, out m); if ((pm & 1) != 0) { dl += a; dr += b; echoIn += 0.8 * m; } }
            m_swell.Tick(u, out a, out b);
            if ((pm & 2) != 0) { dl += a; dr += b; }
            m_ark.Tick(u, out a, out b, out m);
            if ((pm & 4) != 0) { dl += a; dr += b; echoIn += 0.4 * m; }
            if ((pm & 8) != 0) for (int k = 0; k < m_hums.Length; k++) { m_hums[k].Tick(u, out a, out b); dl += a; dr += b; }
            m_gust.Tick(u, out a, out b);
            if ((pm & 16) != 0) { dl += a; dr += b; }
            m_echo.Tick(u, echoIn, out a, out b);
            if ((pm & 32) == 0) { a = 0; b = 0; }
            l = dl + a; r = dr + b;
            el = SendEnv * dl + SendEchoEnv * a; er = SendEnv * dr + SendEchoEnv * b;
        }

        public void Flush()
        {
            m_swell.Flush(); m_ark.Flush(); m_gust.Flush(); m_echo.Flush();
            for (int i = 0; i < m_hums.Length; i++) m_hums[i].Flush();
        }

        public void Clear() { m_echo.Clear(); }
    }
}
