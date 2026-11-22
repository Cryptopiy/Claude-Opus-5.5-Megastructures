// DreadAnalysis: measurement helpers of the D-034 dread layer shared by AudioDreadTests and Tools/audio/harness "dread": the
// loudness of each one-shot voice at gain 1 (the largest 1 s RMS while it sounds, the unit its DreadScore level refers to),
// the impulse response of the Gulf echo bus at a listener x (arrival times and energies of its taps), and the voice of a
// kind rendered alone. Pure C#; these allocate (test and tool use only, never on the audio thread).
using System;

namespace Megastructure
{
    public static class DreadProbe
    {
        public const double WindowSeconds = 1.0, HopSeconds = 0.1;

        static ScoreFrame Frame(double fs, double s, double x, double z, double yaw)
        {
            return new ScoreFrame { Fs = fs, InvFs = 1.0 / fs, Block = NocMixer.Block, S = s, X = x, Z = z, Yaw = yaw, T = 0.0, SceneT = 0.0 };
        }

        /// <summary>Renders one voice of the kind (the first event of that kind in the table, or a groan) at gain 1, mono sum of
        /// its stereo output; returns the samples.</summary>
        public static double[] RenderVoice(DreadKind kind, double fs, out DreadEvent e)
        {
            e = default(DreadEvent);
            foreach (var ev in DreadScore.Events) if (ev.Kind == kind) { e = ev; break; }
            return RenderVoice(in e, fs);
        }

        /// <summary>Renders the voice of one event at gain 1 (no onset delay), mono sum.</summary>
        public static double[] RenderVoice(in DreadEvent e, double fs)
        {
            DreadKind kind = e.Kind;
            double cr = fs / NocMixer.Block;
            D3 p = NocMath.PathPoint(NocMath.Clamp(e.S, 0.0, NocturneConstants.GoalS));
            var f = Frame(fs, e.S, p.x, p.z, 90.0);
            var start = e; start.Delay = 0.0;
            int n = (int)(e.Seconds * fs) + 64;
            var y = new double[n];
            double l, r, m;
            switch (kind)
            {
                case DreadKind.Horn: { var v = new HornVoice(fs, cr); v.Start(in start, 0, in f, 1.0); for (int i = 0; i < n; i++) { if (i % NocMixer.Block == 0) v.St.Control(in f); v.Tick(i, out l, out r, out m); y[i] = m; } break; }
                case DreadKind.Strike: { var v = new StrikeVoice(fs, cr); v.Start(in start, 0, in f, 1.0, 200.0); for (int i = 0; i < n; i++) { if (i % NocMixer.Block == 0) v.St.Control(in f); v.Tick(i, out l, out r, out m); y[i] = m; } break; }
                case DreadKind.Groan: { var v = new GroanVoice(fs, cr); v.Start(in start, 0, in f, 1.0, p.x, p.z + 30.0, 40.0); for (int i = 0; i < n; i++) { if (i % NocMixer.Block == 0) v.St.Control(in f); v.Tick(i, out l, out r, out m); y[i] = m; } break; }
                case DreadKind.Creak: { var v = new CreakVoice(fs, cr); v.Start(in start, 0, in f, 1.0, 200.0); for (int i = 0; i < n; i++) { if (i % NocMixer.Block == 0) v.Control(in f, i); v.Tick(i, out l, out r, out m); y[i] = m; } break; }
                case DreadKind.Chime: { var v = new ChimeVoice(fs, cr); v.Start(in start, 0, in f, 1.0); for (int i = 0; i < n; i++) { if (i % NocMixer.Block == 0) v.St.Control(in f); v.Tick(i, out l, out r, out m); y[i] = m; } break; }
                case DreadKind.Rumble: { var v = new RumbleVoice(fs, cr); v.Start(in start, 0, in f, 1.0); for (int i = 0; i < n; i++) { if (i % NocMixer.Block == 0) v.St.Control(in f); v.Tick(i, out l, out r, out m); y[i] = m; } break; }
            }
            return y;
        }

        /// <summary>Largest RMS over 1 s windows (hop 0.1 s) of a signal.</summary>
        public static double PeakWindowRms(double[] y, double fs)
        {
            int w = (int)(WindowSeconds * fs), h = (int)(HopSeconds * fs);
            double best = 0.0;
            for (int a = 0; a + w <= y.Length; a += h)
            {
                double ss = 0.0;
                for (int i = a; i < a + w; i++) ss += y[i] * y[i];
                best = Math.Max(best, Math.Sqrt(ss / w));
            }
            return best;
        }

        /// <summary>The unit of a kind: the peak 1 s RMS of its voice at gain 1 (1.0 when normalised).</summary>
        public static double UnitRms(DreadKind kind, double fs) => PeakWindowRms(RenderVoice(kind, fs, out _), fs);

        /// <summary>Impulse response of the echo bus for a listener standing at x (z 195, facing +x): stereo power per sample.</summary>
        public static double[] EchoImpulse(double x, double fs, double seconds)
        {
            var bus = new EchoBus(fs, fs / NocMixer.Block);
            var f = Frame(fs, 0.0, x, 195.0, 90.0);
            int n = (int)(seconds * fs);
            var e = new double[n];
            for (int i = 0; i < n; i++)
            {
                if (i % NocMixer.Block == 0) { f.N = i; bus.Control(in f); }
                double u = (i % NocMixer.Block) / (double)NocMixer.Block;
                bus.Tick(u, i == 0 ? 1.0 : 0.0, out double l, out double r);
                e[i] = 0.5 * (l * l + r * r);
            }
            return e;
        }

        /// <summary>Arrival time (s, energy centroid inside +-60 ms of the expected delay) and energy of each tap in an impulse response.</summary>
        public static void Arrival(double[] e, double fs, double expected, out double t, out double energy)
        {
            int a = Math.Max(0, (int)((expected - 0.06) * fs)), b = Math.Min(e.Length - 1, (int)((expected + 0.06) * fs));
            double sum = 0.0, mom = 0.0;
            for (int i = a; i <= b; i++) { sum += e[i]; mom += e[i] * i; }
            t = sum > 0.0 ? mom / sum / fs : double.NaN; energy = sum;
        }
    }
}
