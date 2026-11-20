// BeatBeds: the continuous beat-specific X sources of MASTER_PROMPT 14.3 ("X beat-specific: steam and vending hum; tram;
// trains; leaf rain; screen hum") as beds, with the passes (TramPass, TubeTrain) and LeafRain on top (D-011 / R5 6 (a): a
// transient-only X misses the median gate by 1.5-1.8 dB). One class per place, each unit RMS at gain 1 on average:
//   AlleyBed  (beat 1): the alley's 40 AC units as four along-s voices (blade-pass 40-75 Hz + 2nd harmonic + a shared fan
//             noise, a unit's voice handed over only at zero gain), the four steam vents of 14.5.1 (s 24, 58, 86, 104: hiss
//             HP 2.5 kHz with irregular puffs), the three vending machines at x 2.2, z 62-67 (100 / 200 Hz transformer hum,
//             a 50 Hz compressor cycling on 36 s / off 24 s with 2 s ramps) and the noodle bar's extractor fan (z 40-52);
//   CanyonBed (beat 2): the boulevard canyon's breath (pink -> band 160 Hz Q 0.5, a 0.06 Hz +-1.5 dB swell), the sodium lamps'
//             ballast buzz (100 / 200 / 300 Hz) and the tram wire's faint sing (1.4 kHz, Q 10);
//   EdgeBed   (beat 3, X -60): a faint updraft whistle (2.2 kHz, Q 5) over the rift;
//   TubeHum   (beat 4): the linear-motor drone of the two tubes beside the deck (60.0 Hz N, 60.37 Hz S: a slow beat;
//             harmonics 1 / 0.7 / 0.45 / 0.3), the rail whine (1.43 / 1.91 kHz, Q 20) and the structure's rumble (LP 110 Hz);
//   ScreenHum (beat 6, X -60): the wall's 100 Hz family with a 0.1 Hz flutter;
//   LampBuzz  (beat 7, X -60): the goal lamp's 100 Hz buzz at (1 157.2, 5.0, 387.9) (S8 "the last seconds are rain, hum and
//             the lamp's 100 Hz buzz (-60)"), distance law 1 / (1 + (d / 8)^2) normalised at the stop (s 1 500).
// Spatial sources use ScoreFrame.PanTo and darken behind the listener. Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    public sealed class AlleyBed
    {
        public const double AcSpacing = 3.0, AcShare = 0.55, VentShare = 1.6, VendShare = 1.3, BarShare = 1.1;
        static readonly double[] VentS = { 24.0, 58.0, 86.0, 104.0 };
        readonly double m_fs;
        readonly ulong m_seed;
        // AC voices
        readonly double[] m_acPh = new double[4], m_acInc = new double[4];
        readonly int[] m_acUnit = { -1, -1, -1, -1 };
        GainRamp m_ac0l, m_ac0r, m_ac1l, m_ac1r, m_ac2l, m_ac2r, m_ac3l, m_ac3r;
        NoiseD m_fan; PinkD m_fanPink; SvfD m_fanBp;
        readonly double m_fanNorm;
        // vents
        NoiseD m_vl, m_vr; SvfD m_vhL, m_vhR, m_vlL, m_vlR;
        readonly double m_ventNorm;
        GainRamp m_ventL, m_ventR;
        // vending
        double m_ph50;
        GainRamp m_vendL, m_vendR, m_comp;
        // bar
        NoiseD m_bn; SvfD m_bbp; double m_bph; readonly double m_barNorm;
        GainRamp m_barL, m_barR;

        public AlleyBed(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            m_seed = DspHash.Mix(seed ^ 0xA11EUL);
            m_fan = NoiseD.Stream(seed, 0xA101u); m_fanBp.Set(350.0, 1.0, fs);
            m_fanNorm = PinkBandNorm(fs, 350.0, 1.0);
            m_vl = NoiseD.Stream(seed, 0xA102u); m_vr = NoiseD.Stream(seed, 0xA103u);
            m_vhL.Set(2500.0, 0.7071, fs); m_vhR.CopyCoefficients(in m_vhL); m_vlL.Set(9000.0, 0.7071, fs); m_vlR.CopyCoefficients(in m_vlL);
            m_ventNorm = WhiteBandNorm(fs);
            m_bn = NoiseD.Stream(seed, 0xA104u); m_bbp.Set(280.0, 1.2, fs);
            m_barNorm = 1.0 / Math.Sqrt(Math.PI * 280.0 / (1.2 * fs) / 3.0);
            m_ac0l.Init(0.05, controlRate); m_ac0r.Init(0.05, controlRate); m_ac1l.Init(0.05, controlRate); m_ac1r.Init(0.05, controlRate);
            m_ac2l.Init(0.05, controlRate); m_ac2r.Init(0.05, controlRate); m_ac3l.Init(0.05, controlRate); m_ac3r.Init(0.05, controlRate);
            m_ventL.Init(0.05, controlRate); m_ventR.Init(0.05, controlRate);
            m_vendL.Init(0.05, controlRate); m_vendR.Init(0.05, controlRate); m_comp.Init(0.05, controlRate);
            m_barL.Init(0.05, controlRate); m_barR.Init(0.05, controlRate);
        }

        static double PinkBandNorm(double fs, double fc, double q)
        {
            var pk = new PinkD(); var bp = new SvfD(); bp.Set(fc, q, fs); double e = 0;
            for (int i = 0; i < (int)(1.5 * fs); i++) { double y = bp.BandNorm(pk.Next(i == 0 ? 1.0 : 0.0)); e += y * y; }
            return 1.0 / Math.Sqrt(e / 3.0);
        }

        static double WhiteBandNorm(double fs)
        {
            var hp = new SvfD(); hp.Set(2500.0, 0.7071, fs); var lp = new SvfD(); lp.Set(9000.0, 0.7071, fs); double e = 0;
            for (int i = 0; i < (int)(0.2 * fs); i++) { double y = lp.Low(hp.High(i == 0 ? 1.0 : 0.0)); e += y * y; }
            return 1.0 / Math.Sqrt(e / 3.0);
        }

        /// <summary>AC unit i of the alley lattice: s, lateral (wall side), height, blade-pass frequency.</summary>
        public static void AcUnit(ulong seed, long i, out double s, out double lat, out double h, out double hz)
        {
            s = (i + 0.5 + 0.5 * (NocAudio.Hash01(seed, (ulong)(3 * i)) - 0.5)) * AcSpacing;
            double u = NocAudio.Hash01(seed, (ulong)(3 * i + 1));
            lat = u < 0.5 ? -2.3 : 2.3;
            h = 3.0 + 12.0 * NocAudio.Hash01(seed, (ulong)(3 * i + 2));
            hz = 40.0 + 35.0 * (u < 0.5 ? 2.0 * u : 2.0 * u - 1.0);
        }

        public void Control(in ScoreFrame f, double weight)
        {
            double s = f.S;
            // AC voices: voice v takes the unit i = v (mod 4) nearest in s; its gain window closes at the hand-over (2 spacings)
            for (int v = 0; v < 4; v++)
            {
                long i = (long)Math.Round((s / AcSpacing - 0.5 - v) / 4.0) * 4 + v;
                AcUnit(m_seed, i, out double us, out double lat, out double h, out double hz);
                double ds = s - us;
                // a voice's window closes before the hand-over point (2 spacings minus the 0.75 m position jitter)
                double win = 1.0 - NocMath.Smoothstep(0.0, 5.0, Math.Abs(ds));
                NocLanes.Path.CorridorPoint(NocMath.Clamp(us, 0.0, 120.0), lat, out double ux, out double uz);
                double dx = ux - f.X, dz = uz - f.Z, dy = h - NocturneConstants.EyeHeight, d = Math.Sqrt(dx * dx + dz * dz + dy * dy);
                double g = weight * win * Math.Sqrt(AcShare) * 2.0 / (1.0 + (d / 6.0) * (d / 6.0)) * (1.0 - NocMath.Smootherstep(112.0, 124.0, us));
                NocAudio.Pan(f.PanTo(ux, uz), out double pl, out double pr);
                if (i != m_acUnit[v] && win < 1e-3) m_acUnit[v] = (int)i;
                if (i == m_acUnit[v]) m_acInc[v] = hz / m_fs;
                else g = 0.0;                                   // the new unit takes over only at zero gain
                Step(v, g * pl, g * pr);
            }
            // vents: one hiss source, summed per-vent gains (only one vent is near at a time)
            double vl = 0, vr = 0;
            for (int k = 0; k < VentS.Length; k++)
            {
                double side = NocAudio.Hash01(m_seed, (ulong)(100 + k)) < 0.5 ? -1.8 : 1.8;
                NocLanes.Path.CorridorPoint(VentS[k], side, out double vx, out double vz);
                double dx = vx - f.X, dz = vz - f.Z, d = Math.Sqrt(dx * dx + dz * dz + 2.0);
                double puff = 0.55 + 0.45 * NocAudio.SmoothRandom01(m_seed + (ulong)k, 0.9 * f.T);
                puff *= puff;
                double g = weight * Math.Sqrt(VentShare) * puff / (1.0 + (d / 4.0) * (d / 4.0)) * (0.6 + 0.4 * f.FrontTo(vx, vz));
                NocAudio.Pan(f.PanTo(vx, vz), out double pl, out double pr);
                vl += g * pl; vr += g * pr;
            }
            m_ventL.Step(vl * m_ventNorm); m_ventR.Step(vr * m_ventNorm);
            // vending: (2.2, 64.5), compressor 36 s on / 24 s off with 2 s ramps
            {
                NocLanes.Path.CorridorPoint(64.5, 2.2, out double mx, out double mz);
                double dx = mx - f.X, dz = mz - f.Z, d = Math.Sqrt(dx * dx + dz * dz + 0.5);
                double g = weight * Math.Sqrt(VendShare) / (1.0 + (d / 3.0) * (d / 3.0));
                NocAudio.Pan(f.PanTo(mx, mz), out double pl, out double pr);
                m_vendL.Step(g * pl); m_vendR.Step(g * pr);
                double c = f.T / 60.0 + 0.3; c -= Math.Floor(c);
                double on = NocMath.Smoothstep(0.0, 2.0 / 60.0, c) * (1.0 - NocMath.Smoothstep(0.6, 0.6 + 2.0 / 60.0, c));
                m_comp.Step(on);
            }
            // noodle bar extractor fan: (-2.6, 46)
            {
                NocLanes.Path.CorridorPoint(46.0, -2.6, out double bx, out double bz);
                double dx = bx - f.X, dz = bz - f.Z, d = Math.Sqrt(dx * dx + dz * dz + 1.0);
                double g = weight * Math.Sqrt(BarShare) / (1.0 + (d / 4.0) * (d / 4.0));
                NocAudio.Pan(f.PanTo(bx, bz), out double pl, out double pr);
                m_barL.Step(g * pl); m_barR.Step(g * pr);
            }
        }

        void Step(int v, double l, double r)
        {
            switch (v)
            {
                case 0: m_ac0l.Step(l); m_ac0r.Step(r); break;
                case 1: m_ac1l.Step(l); m_ac1r.Step(r); break;
                case 2: m_ac2l.Step(l); m_ac2r.Step(r); break;
                default: m_ac3l.Step(l); m_ac3r.Step(r); break;
            }
        }

        bool Silent => m_ac0l.Silent && m_ac1l.Silent && m_ac2l.Silent && m_ac3l.Silent && m_ac0r.Silent && m_ac1r.Silent && m_ac2r.Silent && m_ac3r.Silent
                       && m_ventL.Silent && m_ventR.Silent && m_vendL.Silent && m_vendR.Silent && m_barL.Silent && m_barR.Silent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            l = 0; r = 0;
            if (Silent) return;
            double fan = m_fanNorm * m_fanBp.BandNorm(m_fanPink.Next(m_fan.Next()));
            for (int v = 0; v < 4; v++)
            {
                double ph = m_acPh[v];
                double x = 0.55 * NocAudio.Sin01(ph) + 0.30 * NocAudio.Sin01(2.0 * ph) + 0.75 * fan;   // unit-ish: 0.5 (0.55^2 + 0.3^2) + 0.56
                m_acPh[v] = ph + m_acInc[v] >= 1.0 ? ph + m_acInc[v] - 1.0 : ph + m_acInc[v];
                switch (v)
                {
                    case 0: l += m_ac0l.At(u) * x; r += m_ac0r.At(u) * x; break;
                    case 1: l += m_ac1l.At(u) * x; r += m_ac1r.At(u) * x; break;
                    case 2: l += m_ac2l.At(u) * x; r += m_ac2r.At(u) * x; break;
                    default: l += m_ac3l.At(u) * x; r += m_ac3r.At(u) * x; break;
                }
            }
            // vents
            double hl = m_vlL.Low(m_vhL.High(m_vl.Next())), hr = m_vlR.Low(m_vhR.High(m_vr.Next()));
            l += m_ventL.At(u) * hl; r += m_ventR.At(u) * hr;
            // vending: 100 / 200 Hz hum + the 50 Hz compressor
            m_ph50 += 50.0 / m_fs; if (m_ph50 >= 1.0) m_ph50 -= 1.0;
            double vend = 0.8 * NocAudio.Sin01(2.0 * m_ph50) + 0.45 * NocAudio.Sin01(4.0 * m_ph50) + m_comp.At(u) * (0.9 * SineTable.Lookup(m_ph50) + 0.3 * NocAudio.Sin01(3.0 * m_ph50));
            l += m_vendL.At(u) * vend; r += m_vendR.At(u) * vend;
            // bar fan
            m_bph += 117.0 / m_fs; if (m_bph >= 1.0) m_bph -= 1.0;
            double bar = 0.8 * m_barNorm * m_bbp.BandNorm(m_bn.Next()) + 0.5 * SineTable.Lookup(m_bph) + 0.25 * NocAudio.Sin01(2.0 * m_bph);
            l += m_barL.At(u) * bar; r += m_barR.At(u) * bar;
        }

        public void Flush() { m_fanPink.Flush(); m_fanBp.Flush(); m_vhL.Flush(); m_vhR.Flush(); m_vlL.Flush(); m_vlR.Flush(); m_bbp.Flush(); }
    }

    public sealed class CanyonBed
    {
        readonly double m_fs, m_norm;
        NoiseD m_nl, m_nr, m_nw;
        PinkD m_pl, m_pr;
        SvfD m_bl, m_br, m_wire;
        double m_ph;
        GainRamp m_g;
        readonly double m_wireNorm;
        public const double RoarHz = 160.0, RoarQ = 0.5, WireHz = 1400.0, WireQ = 10.0;

        public CanyonBed(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            m_nl = NoiseD.Stream(seed, 0xC201u); m_nr = NoiseD.Stream(seed, 0xC202u); m_nw = NoiseD.Stream(seed, 0xC203u);
            m_bl.Set(RoarHz, RoarQ, fs); m_br.CopyCoefficients(in m_bl); m_wire.Set(WireHz, WireQ, fs);
            var pk = new PinkD(); var bp = new SvfD(); bp.Set(RoarHz, RoarQ, fs); var dc = new DcBlockD(); dc.Init(NocturneConstants.DcBlockHz, fs); double e = 0;
            for (int i = 0; i < (int)(1.5 * fs); i++) { double y = dc.Tick(bp.BandNorm(pk.Next(i == 0 ? 1.0 : 0.0))); e += y * y; }
            m_norm = 1.0 / Math.Sqrt(e / 3.0);
            m_wireNorm = 1.0 / Math.Sqrt(Math.PI * WireHz / (WireQ * fs) / 3.0);
            m_g.Init(0.05, controlRate);
        }

        public void Control(in ScoreFrame f, double weight)
        {
            double swell = NocAudio.Db(1.5 * Math.Sin(NocAudio.TwoPi * (0.06 * f.T + 0.2)));
            m_g.Step(weight * swell);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            double g = m_g.At(u);
            if (g <= 0.0) { l = 0; r = 0; return; }
            m_ph += 100.0 / m_fs; if (m_ph >= 1.0) m_ph -= 1.0;
            double ballast = 0.16 * SineTable.Lookup(m_ph) + 0.09 * NocAudio.Sin01(2.0 * m_ph) + 0.06 * NocAudio.Sin01(3.0 * m_ph);
            double wire = 0.06 * m_wireNorm * m_wire.BandNorm(m_nw.Next());
            double k = 0.975 * m_norm;   // the roar carries ~95 % of the power, ballast + wire the rest
            l = g * (k * m_bl.BandNorm(m_pl.Next(m_nl.Next())) + ballast + wire);
            r = g * (k * m_br.BandNorm(m_pr.Next(m_nr.Next())) + ballast + wire);
        }

        public void Flush() { m_pl.Flush(); m_pr.Flush(); m_bl.Flush(); m_br.Flush(); m_wire.Flush(); }
    }

    public sealed class EdgeBed
    {
        NoiseD m_n; SvfD m_bp; readonly double m_norm; GainRamp m_g; readonly ulong m_seed;
        public EdgeBed(double fs, double controlRate, uint seed)
        {
            m_n = NoiseD.Stream(seed, 0xE301u); m_bp.Set(2200.0, 5.0, fs);
            m_norm = 1.0 / Math.Sqrt(Math.PI * 2200.0 / (5.0 * fs) / 3.0);
            m_seed = DspHash.Mix(seed ^ 0xED6EUL);
            m_g.Init(0.05, controlRate);
        }
        public void Control(in ScoreFrame f, double weight) { m_g.Step(weight * m_norm * (0.4 + 1.2 * NocAudio.SmoothRandom01(m_seed, 0.11 * f.T))); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r) { double g = m_g.At(u); double x = g > 0 ? g * m_bp.BandNorm(m_n.Next()) : 0.0; l = x; r = x; }
        public void Flush() { m_bp.Flush(); }
    }

    public sealed class TubeHum
    {
        readonly double m_fs;
        double m_phN, m_phS;
        NoiseD m_nw, m_nr; PinkD m_pr; SvfD m_w1, m_w2, m_rum;
        readonly double m_wNorm1, m_wNorm2, m_rumNorm;
        GainRamp m_nL, m_nR, m_sL, m_sR, m_g;
        static readonly double[] Harm = { 1.0, 0.7, 0.45, 0.3 };
        readonly double m_toneNorm;

        public TubeHum(double fs, double controlRate, uint seed)
        {
            m_fs = fs;
            m_nw = NoiseD.Stream(seed, 0x7B01u); m_nr = NoiseD.Stream(seed, 0x7B02u);
            m_w1.Set(1430.0, 20.0, fs); m_w2.Set(1910.0, 20.0, fs); m_rum.Set(110.0, 0.7071, fs);
            m_wNorm1 = 1.0 / Math.Sqrt(Math.PI * 1430.0 / (20.0 * fs) / 3.0);
            m_wNorm2 = 1.0 / Math.Sqrt(Math.PI * 1910.0 / (20.0 * fs) / 3.0);
            var pk = new PinkD(); var lp = new SvfD(); lp.Set(110.0, 0.7071, fs); var dc = new DcBlockD(); dc.Init(NocturneConstants.DcBlockHz, fs); double e = 0;
            for (int i = 0; i < (int)(1.5 * fs); i++) { double y = dc.Tick(lp.Low(pk.Next(i == 0 ? 1.0 : 0.0))); e += y * y; }
            m_rumNorm = 1.0 / Math.Sqrt(e / 3.0);
            double p = 0; for (int k = 0; k < 4; k++) p += 0.5 * Harm[k] * Harm[k] / Math.Pow(DcBlockD.Magnitude(60.0 * (k + 1), NocturneConstants.DcBlockHz, fs), 2);
            m_toneNorm = 1.0 / Math.Sqrt(p);
            m_nL.Init(0.05, controlRate); m_nR.Init(0.05, controlRate); m_sL.Init(0.05, controlRate); m_sR.Init(0.05, controlRate); m_g.Init(0.05, controlRate);
        }

        public void Control(in ScoreFrame f, double weight)
        {
            NocAudio.Pan(0.7 * f.PanTo(f.X, NocturneScore.TubeNorthZ), out double nl, out double nr);
            NocAudio.Pan(0.7 * f.PanTo(f.X, NocturneScore.TubeSouthZ), out double sl, out double sr);
            m_nL.Step(nl); m_nR.Step(nr); m_sL.Step(sl); m_sR.Step(sr);
            m_g.Step(weight);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            double g = m_g.At(u);
            m_phN += 60.0 / m_fs; if (m_phN >= 1.0) m_phN -= 1.0;
            m_phS += 60.37 / m_fs; if (m_phS >= 1.0) m_phS -= 1.0;
            if (g <= 0.0) { l = 0; r = 0; return; }
            double tn = 0, ts = 0;
            for (int k = 0; k < 4; k++) { tn += Harm[k] * NocAudio.Sin01((k + 1) * m_phN); ts += Harm[k] * NocAudio.Sin01((k + 1) * m_phS); }
            double whine = 0.05 * (m_wNorm1 * m_w1.BandNorm(m_nw.Next()) + 0.7 * m_wNorm2 * m_w2.BandNorm(m_nw.Next()));
            double rum = 0.55 * m_rumNorm * m_rum.Low(m_pr.Next(m_nr.Next()));
            double kt = 0.55 * m_toneNorm;                       // tones ~ 60 % of the power, rumble ~ 30 %, whine ~ 0.4 %
            l = g * (kt * (m_nL.At(u) * tn + m_sL.At(u) * ts) * 0.7071 + rum + whine);
            r = g * (kt * (m_nR.At(u) * tn + m_sR.At(u) * ts) * 0.7071 + rum + whine);
        }

        public void Flush() { m_w1.Flush(); m_w2.Flush(); m_rum.Flush(); m_pr.Flush(); }
    }

    public sealed class ScreenHum
    {
        readonly double m_fs; double m_ph; GainRamp m_g; readonly double m_norm;
        public ScreenHum(double fs, double controlRate, uint seed) { m_fs = fs; m_g.Init(0.05, controlRate); m_norm = 1.0 / Math.Sqrt(0.5 * (1.0 + 0.36 + 0.16 + 0.0625)); }
        public void Control(in ScoreFrame f, double weight) { m_g.Step(weight * m_norm * (1.0 + 0.1 * Math.Sin(NocAudio.TwoPi * 0.1 * f.T))); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            m_ph += 100.0 / m_fs; if (m_ph >= 1.0) m_ph -= 1.0;
            double g = m_g.At(u);
            double x = g > 0 ? g * (SineTable.Lookup(m_ph) + 0.6 * NocAudio.Sin01(2 * m_ph) + 0.4 * NocAudio.Sin01(3 * m_ph) + 0.25 * NocAudio.Sin01(4 * m_ph)) : 0.0;
            l = x; r = x;
        }
    }

    public sealed class LampBuzz
    {
        public const double LampX = 1157.2, LampY = 5.0, LampZ = 387.9, RefDistance = 8.0;
        readonly double m_fs, m_norm, m_atStop; double m_ph; GainRamp m_l, m_r;
        public LampBuzz(double fs, double controlRate, uint seed)
        {
            m_fs = fs; m_l.Init(0.05, controlRate); m_r.Init(0.05, controlRate);
            m_norm = 1.0 / Math.Sqrt(0.5 * (1.0 + 0.49 + 0.25 + 0.09));
            m_atStop = Law(NocturneConstants.GoalS);
        }
        static double Law(double s)
        {
            D3 e = NocMath.PathPoint(s);
            double dx = LampX - e.x, dy = LampY - NocturneConstants.EyeHeight, dz = LampZ - e.z, d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            return 1.0 / (1.0 + (d / RefDistance) * (d / RefDistance));
        }
        /// <summary>Control: the plaza lamps' floor plus the goal lamp's distance law; the X column of beat 7 is reached at the stop.</summary>
        public void Control(in ScoreFrame f, double weight)
        {
            // a floor of the twenty plaza lamps' ballasts (0.7) under the goal lamp's own law (1.0 at the stop)
            double g = weight * m_norm * (0.7 + Law(f.S) / m_atStop) / 1.7;
            NocAudio.Pan(f.PanTo(LampX, LampZ), out double pl, out double pr);
            m_l.Step(g * pl); m_r.Step(g * pr);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double l, out double r)
        {
            m_ph += 100.0 / m_fs; if (m_ph >= 1.0) m_ph -= 1.0;
            if (m_l.Silent && m_r.Silent) { l = 0; r = 0; return; }
            double x = SineTable.Lookup(m_ph) + 0.7 * NocAudio.Sin01(2 * m_ph) + 0.5 * NocAudio.Sin01(3 * m_ph) + 0.3 * NocAudio.Sin01(5 * m_ph);
            l = m_l.At(u) * x; r = m_r.At(u) * x;
        }
    }
}
