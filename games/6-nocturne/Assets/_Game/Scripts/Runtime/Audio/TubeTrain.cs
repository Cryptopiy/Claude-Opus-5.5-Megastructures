// TubeTrain: the trains T1-T7 of MASTER_PROMPT 14.5.4 (beat 4, X): 7 cars of 24 m plus a 6 m nose (174 m), 38 m/s in the
// glass tubes N (z 207) and S (z 183) beside the deck and the upper (y +118) and lower (y -142) tubes between the Gulf faces;
// each launched when s first crosses its s_launch, then on real time tau (14.7 H7; NocturneScore.Vehicles), reset when s
// falls 12 m below it; a jump of s past the launch uses the capture rule tau = (s - s_launch) / 2.5. VehicleClock and PassCore
// are shared with TramPass. The pass is an incoherent finite line source (docs/research/R5 4.6): the level of the segment in
// the open section of the tube (portal to portal) is (atan(x2 / d) - atan(x1 / d)) / d relative to the abeam peak, the parts
// inside the gate blocks or the faces are muffled 20 dB; brightness (air rush LP 700 -> 2 500 Hz, the glass caps it) and
// stereo width come from the subtended angle, the image from the segment's angular midpoint. Doppler (the task's "panned,
// Doppler"): the tonal parts (linear-motor whine 280 / 560 / 1 120 Hz) and the noise band centres move by c / (c + v_r) with
// v_r the intensity-weighted mean radial velocity of the radiating segment, closed form
// v_r = u d (1 / r1 - 1 / r2) / (atan(x2 / d) - atan(x1 / d)) (a 174 m train abeam at 12 m: approaching +12 %, receding -10 %).
// Near passes add the nose pressure whoosh (Gaussian sigma 0.25 s as the nose passes the foot point) and soft bogie thumps
// (85 Hz, T60 80 ms, bogies at +-8.5 m of each car centre) and the tube's own 52 Hz resonance. Allocation-free.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    /// <summary>Launch keyed to s, then real time (14.7 H7).</summary>
    public struct VehicleClock
    {
        public bool Launched;
        public long LaunchN;
        bool m_seen;
        double m_lastS;

        public void Update(long n, double s, double fs, double sLaunch)
        {
            bool jump = m_seen && Math.Abs(s - m_lastS) > 5.0;
            if (!m_seen || jump)
            {
                m_seen = true; m_lastS = s;
                if (s >= sLaunch) { Launched = true; LaunchN = n - (long)Math.Round((s - sLaunch) / NocturneConstants.CinematicSpeed * fs); }
                else Launched = false;
                return;
            }
            m_lastS = s;
            if (!Launched) { if (s >= sLaunch) { Launched = true; LaunchN = n; } }
            else if (s < sLaunch - NocturneScore.VehicleResetBelow) Launched = false;
        }

        /// <summary>Seconds since the launch (negative when not launched).</summary>
        public double Tau(long n, double fs) => Launched ? (n - LaunchN) / fs : -1.0;
    }

    /// <summary>Line-source geometry of a vehicle at one control block: gain, brightness, width, pan and Doppler ratio.</summary>
    public struct PassGeometry
    {
        public double Gain, Bright, Width, Pan, Doppler, NoseX, Perp, Front;

        public const double SoundSpeed = 343.0, MuffleDb = -20.0;

        public static PassGeometry Evaluate(in NocturneScore.Vehicle v, double tau, in ScoreFrame f, double dRef, double vListenerX)
        {
            PassGeometry g = default(PassGeometry);
            double xh = v.HeadX(tau), xt = xh - v.Dir * v.Length;
            double lo = Math.Min(xh, xt), hi = Math.Max(xh, xt);
            double dy = NocturneConstants.EyeHeight - v.TrackY, dz = f.Z - v.TrackZ;
            double d = Math.Max(1.0, Math.Sqrt(dy * dy + dz * dz));
            g.Perp = d; g.NoseX = xh - f.X;
            double oLo = Math.Max(lo, v.OpenX0), oHi = Math.Min(hi, v.OpenX1);
            double open = oHi > oLo ? LineSource.Gain(oLo - f.X, oHi - f.X, d, v.Length, dRef) : 0.0;
            // the hidden parts (inside the terminus, the gate blocks or the faces) as muffled line sources
            double hid = 0.0;
            if (lo < v.OpenX0) { double a = LineSource.Gain(lo - f.X, Math.Min(hi, v.OpenX0) - f.X, d, v.Length, dRef); hid += a * a; }
            if (hi > v.OpenX1) { double a = LineSource.Gain(Math.Max(lo, v.OpenX1) - f.X, hi - f.X, d, v.Length, dRef); hid += a * a; }
            double m = NocAudio.Db(MuffleDb);
            g.Gain = Math.Sqrt(open * open + m * m * hid);
            if (oHi > oLo)
            {
                double x1 = oLo - f.X, x2 = oHi - f.X;
                double a1 = Math.Atan(x1 / d), a2 = Math.Atan(x2 / d), ang = a2 - a1, aMax = 2.0 * Math.Atan(0.5 * v.Length / d);
                g.Bright = NocMath.Clamp01(ang / Math.Max(1e-6, aMax));
                g.Width = Math.Min(1.0, ang / Math.PI);
                double xc = d * Math.Tan(0.5 * (a1 + a2));
                g.Pan = f.PanTo(f.X + xc, v.TrackZ) * (1.0 - 0.5 * g.Width);
                g.Front = f.FrontTo(f.X + xc, v.TrackZ);
                // intensity-weighted mean radial velocity of the open segment (closed form)
                double u = v.Dir * v.Speed - vListenerX;
                double r1 = Math.Sqrt(x1 * x1 + d * d), r2 = Math.Sqrt(x2 * x2 + d * d);
                double vr = ang > 1e-9 ? u * d * (1.0 / r1 - 1.0 / r2) / ang : 0.0;
                g.Doppler = SoundSpeed / (SoundSpeed + vr);
            }
            else { g.Bright = 0.0; g.Width = 0.0; g.Pan = 0.0; g.Front = 1.0; g.Doppler = 1.0; }
            return g;
        }
    }

    /// <summary>Noise + tonal core of a pass (air or rolling noise, whine partials, a low resonance), unit RMS at gain 1.</summary>
    public struct PassCore
    {
        NoiseD m_nl, m_nr, m_nc;
        PinkD m_pl, m_pr, m_pc;
        SvfD m_bl, m_br, m_ll, m_lr, m_rl, m_rr, m_res;
        double m_noiseNorm, m_resNorm, m_bandHz, m_q, m_lpLast, m_bandLast;
        readonly double[] m_ph, m_inc, m_amp;
        readonly double[] m_f;
        GainRamp m_gL, m_gR, m_gTone, m_gRes;
        double m_toneNorm;
        public double ToneDb, ResDb;

        public PassCore(double fs, double controlRate, uint seed, uint stream, double bandHz, double q, double[] toneHz, double[] toneDb, double toneRelDb, double resRelDb) : this()
        {
            m_nl = NoiseD.Stream(seed, stream + 1u); m_nr = NoiseD.Stream(seed, stream + 2u); m_nc = NoiseD.Stream(seed, stream + 3u);
            m_bandHz = bandHz; m_q = q;
            m_bl.Set(bandHz, q, fs); m_br.CopyCoefficients(in m_bl);
            m_ll.Set(2000.0, 0.7071, fs); m_lr.CopyCoefficients(in m_ll);
            m_res.Set(52.0, 2.0, fs);
            m_f = (double[])toneHz.Clone();
            m_ph = new double[m_f.Length]; m_inc = new double[m_f.Length]; m_amp = new double[m_f.Length];
            double p = 0;
            for (int i = 0; i < m_f.Length; i++) { m_amp[i] = NocAudio.Db(toneDb[i]); p += 0.5 * m_amp[i] * m_amp[i]; m_ph[i] = NocAudio.Hash01(seed + stream, (ulong)i); }
            m_toneNorm = 1.0 / Math.Sqrt(Math.Max(1e-12, p));
            ToneDb = toneRelDb; ResDb = resRelDb;
            // pink (unit RMS, density ~ 1 / f) into BandNorm(fc, Q): measured by the impulse energy, white variance 1/3
            m_noiseNorm = ImpulseNorm(fs, bandHz, q);
            m_resNorm = ImpulseNorm(fs, 52.0, 2.0);
            m_gL.Init(0.05, controlRate); m_gR.Init(0.05, controlRate); m_gTone.Init(0.05, controlRate); m_gRes.Init(0.05, controlRate);
        }

        static double ImpulseNorm(double fs, double fc, double q)
        {
            var pk = new PinkD(); var bp = new SvfD(); bp.Set(fc, q, fs); double e = 0;
            int n = (int)(1.5 * fs);
            for (int i = 0; i < n; i++) { double y = bp.BandNorm(pk.Next(i == 0 ? 1.0 : 0.0)); e += y * y; }
            return 1.0 / Math.Sqrt(e / 3.0);
        }

        /// <summary>Control: overall gain, the LP cutoff (brightness), stereo width (decorrelated share), pan, Doppler ratio.</summary>
        public void Control(double gain, double lpHz, double width, double pan, double doppler, double fs)
        {
            if (Math.Abs(lpHz - m_lpLast) > 3.0) { m_ll.Set(lpHz, 0.7071, fs); m_lr.CopyCoefficients(in m_ll); m_lpLast = lpHz; }
            double band = m_bandHz * doppler;
            if (Math.Abs(band - m_bandLast) > 0.5) { m_bl.Set(band, m_q, fs); m_br.CopyCoefficients(in m_bl); m_bandLast = band; }
            for (int i = 0; i < m_f.Length; i++) m_inc[i] = m_f[i] * doppler / fs;
            NocAudio.Pan(pan, out double pl, out double pr);
            // tones: sum of shares noise 1, tone ToneDb, resonance ResDb; normalised to unit power
            double t = NocAudio.Db(ToneDb), r = NocAudio.Db(ResDb), k = gain / Math.Sqrt(1.0 + t * t + r * r);
            m_gL.Step(k * pl); m_gR.Step(k * pr);
            m_gTone.Step(t * m_toneNorm); m_gRes.Step(r * m_resNorm);
            m_width = width;
        }
        double m_width;

        public bool Silent => m_gL.Silent && m_gR.Silent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            if (Silent) { l = 0; r = 0; AdvancePhases(); return; }
            double c = m_pc.Next(m_nc.Next());
            double a = m_pl.Next(m_nl.Next()), b = m_pr.Next(m_nr.Next());
            double w = m_width, wc = Math.Sqrt(1.0 - w), wd = Math.Sqrt(w);
            double xl = m_noiseNorm * m_ll.Low(m_bl.BandNorm(wc * c + wd * a));
            double xr = m_noiseNorm * m_lr.Low(m_br.BandNorm(wc * c + wd * b));
            double tone = 0;
            for (int i = 0; i < m_f.Length; i++)
            {
                tone += m_amp[i] * SineTable.Lookup(m_ph[i]);
                double p = m_ph[i] + m_inc[i]; if (p >= 1.0) p -= 1.0; m_ph[i] = p;
            }
            double mono = m_gTone.At(u) * tone + m_gRes.At(u) * m_res.BandNorm(c);
            l = m_gL.At(u) * (xl + mono); r = m_gR.At(u) * (xr + mono);
        }

        void AdvancePhases() { for (int i = 0; i < m_f.Length; i++) { double p = m_ph[i] + m_inc[i]; if (p >= 1.0) p -= 1.0; m_ph[i] = p; } }

        public void Flush() { m_pl.Flush(); m_pr.Flush(); m_pc.Flush(); m_bl.Flush(); m_br.Flush(); m_ll.Flush(); m_lr.Flush(); m_res.Flush(); }
    }

    public sealed class TubeTrain
    {
        /// <summary>RMS dBFS of a whole train abeam at 12 m (the bridge tubes), before the X-layer swells. Fix wave C08 / AU-06: 3 dB
        /// down (-21 -> -24, whoosh -24 -> -27, thump -30 -> -33): the passes were the five loudest seconds of the walk, +8.5..+9 dB over
        /// the beat-4 median, louder than any money shot, burying the dread cues near them and loading the edit's make-up limiter.</summary>
        public const double PeakDb = -24.0, RefDistance = 12.0, FarHz = 700.0, NearHz = 2500.0;
        public const double WhooshSigmaS = 0.25, WhooshDb = -27.0, ThumpDb = -33.0, ThumpHz = 85.0, ThumpT60 = 0.08;
        static readonly double[] WhineHz = { 280.0, 560.0, 1120.0 };
        static readonly double[] WhineDb = { 0.0, -6.0, -14.0 };
        public readonly int Index;
        readonly NocturneScore.Vehicle m_v;
        readonly double m_fs;
        VehicleClock m_clock;
        PassCore m_core;
        NoiseD m_wn;
        SvfD m_wbl, m_wbr;
        GainRamp m_wg, m_wgl, m_wgr;
        double m_wNorm;
        // bogie thumps (4 voices)
        readonly double[] m_thE = new double[4], m_thPh = new double[4], m_thGL = new double[4], m_thGR = new double[4];
        readonly double m_thMul;
        readonly double[] m_bogie;           // bogie offsets behind the nose (m)
        readonly double[] m_lastRel;
        public PassGeometry Last;
        public double Tau { get; private set; }

        public TubeTrain(int vehicleIndex, double fs, double controlRate, uint seed)
        {
            Index = vehicleIndex; m_v = NocturneScore.Vehicles[vehicleIndex]; m_fs = fs;
            m_core = new PassCore(fs, controlRate, seed, 0x7000u + 16u * (uint)vehicleIndex, 500.0, 0.6, WhineHz, WhineDb, -10.0, -8.0);
            m_wn = NoiseD.Stream(seed, 0x7F00u + (uint)vehicleIndex);
            m_wbl.Set(550.0, 0.7, fs); m_wbr.CopyCoefficients(in m_wbl);
            m_wNorm = 1.0 / Math.Sqrt(Math.PI * 550.0 / (0.7 * fs) / 3.0);
            m_wg.Init(0.03, controlRate); m_wgl.Init(0.05, controlRate); m_wgr.Init(0.05, controlRate);
            m_thMul = NocAudio.T60Mul(ThumpT60, fs);
            m_bogie = new double[14];
            for (int c = 0; c < 7; c++) { double centre = 6.0 + 24.0 * c + 12.0; m_bogie[2 * c] = centre - 8.5; m_bogie[2 * c + 1] = centre + 8.5; }
            m_lastRel = new double[14];
        }

        public bool Active { get; private set; }

        public void Control(in ScoreFrame f, double vListenerX)
        {
            m_clock.Update(f.N, f.S, f.Fs, m_v.SLaunch);
            Tau = m_clock.Tau(f.N, f.Fs);
            bool on = Tau >= 0.0 && Tau < 60.0;
            if (!on)
            {
                Active = !m_core.Silent;
                m_core.Control(0.0, FarHz, 0.0, 0.0, 1.0, m_fs);
                m_wg.Step(0.0);
                return;
            }
            Active = true;
            var g = PassGeometry.Evaluate(in m_v, Tau, in f, RefDistance, vListenerX);
            Last = g;
            double gain = NocAudio.Db(PeakDb) * g.Gain;
            double lp = FarHz * Math.Pow(NearHz / FarHz, g.Bright) * (0.75 + 0.25 * g.Front);
            m_core.Control(gain, lp, g.Width, g.Pan, g.Doppler, m_fs);
            // nose whoosh while the nose passes the foot point inside the open section (near tubes only)
            double xh = m_v.HeadX(Tau);
            bool inOpen = xh >= m_v.OpenX0 && xh <= m_v.OpenX1;
            double sx = WhooshSigmaS * m_v.Speed, z = g.NoseX / sx;
            double wh = inOpen && g.Perp < 40.0 ? Math.Exp(-0.5 * z * z) * (RefDistance / g.Perp) : 0.0;
            m_wg.Step(NocAudio.Db(WhooshDb) * wh * m_wNorm);
            NocAudio.Pan(f.PanTo(xh, m_v.TrackZ), out double pl, out double pr);
            m_wgl.Step(pl); m_wgr.Step(pr);
            if (g.Perp < 40.0)
                for (int b = 0; b < m_bogie.Length; b++)
                {
                    double xb = xh - m_v.Dir * m_bogie[b], rel = xb - f.X;
                    bool inside = xb >= m_v.OpenX0 && xb <= m_v.OpenX1;
                    if (inside && m_lastRel[b] != 0.0 && Math.Sign(rel) != Math.Sign(m_lastRel[b])) Thump(b, RefDistance / g.Perp, f.PanTo(xb, m_v.TrackZ));
                    m_lastRel[b] = rel == 0.0 ? 1e-9 : rel;
                }
        }

        void Thump(int b, double gain, double pan)
        {
            int v = 0; double lo = double.MaxValue;
            for (int i = 0; i < 4; i++) if (m_thE[i] < lo) { lo = m_thE[i]; v = i; }
            m_thE[v] = NocAudio.Db(ThumpDb) * gain * (0.8 + 0.4 * NocAudio.Hash01((ulong)Index, (ulong)b));
            m_thPh[v] = 0.0;
            NocAudio.Pan(pan, out m_thGL[v], out m_thGR[v]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            m_core.Tick(u, out l, out r);
            double wg = m_wg.At(u);
            double wn = m_wn.Next();
            if (wg > 0.0) { double w = wg * m_wbl.BandNorm(wn); l += m_wgl.At(u) * w; r += m_wgr.At(u) * w; }
            for (int i = 0; i < 4; i++)
            {
                if (m_thE[i] < 1e-6) continue;
                double x = m_thE[i] * SineTable.Lookup(m_thPh[i]);
                m_thPh[i] += ThumpHz / m_fs; if (m_thPh[i] >= 1.0) m_thPh[i] -= 1.0;
                m_thE[i] *= m_thMul;
                l += m_thGL[i] * x; r += m_thGR[i] * x;
            }
        }

        public void Flush() { m_core.Flush(); m_wbl.Flush(); }
    }
}
