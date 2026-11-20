// PadVoice and PadEngine: the synthesiser pad of MASTER_PROMPT 14.3 "Pad" and 14.7 H8 (docs/research/R5 3.1-3.5).
// PadVoice = one oscillator pair: a PolyBLEP saw at -4 cents and a 2-point polyBLAMP triangle at +4 cents (the D-011 reading of
// the "integrated triangle": no leaky integrator, no start-phase DC), double phases, each with its own 0.03 Hz drift of
// +-2 cents (the triangle's at 1.13 x 0.03 Hz). PadEngine = five slots x two banks of PadVoice per chord (14.3 table), voices
// matched with the common tones pinned to their slot first and the rest by pitch order (R5 3.5: pure pitch order moves a
// common tone in 8 of the 10 changes), a changing slot fades its old bank against its new bank with an equal-power law on
// the sum-to-1 chord weight g_c(s) = smootherstep over [s_c - 20, s_c + 20] (C10: [1 472, 1 500], so the open fifth is
// complete at the stop); one stereo Simper TPT low-pass for all voices (500-1 600 Hz from the 14.1 intensity curve, a 0.04 Hz
// +-0.3 octave LFO, 20 ms log smoothing), then the chorus (14 / 19 ms, 0.6 ms). Phases advance with t only; only gains depend
// on s, so the pad is continuous at any speed, standing still and walking back. A bank whose note must change while it
// still sounds (a jump of s) first fades to silence (50 ms), then takes the note. Allocation-free after construction.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure
{
    /// <summary>One pad voice: saw (-4 cents) + triangle (+4 cents) with independent 0.03 Hz drifts, double phases.</summary>
    public struct PadVoice
    {
        public const double SawMix = 0.55, TriMix = 0.85;
        public const double DetuneCents = 4.0, DriftCents = 2.0, DriftHz = 0.03, DriftRatio = 1.13;

        public int Note;                    // MIDI note, -1 = none
        public double PhS, PhT;             // phases in turns [0, 1)
        public double IncS, IncT;           // per-sample increments (the polyBLEP residuals use these exact values)
        public double DriftPhS, DriftPhT;   // drift start phases (hashed)
        public double Norm, PanL, PanR;     // unit-RMS normaliser of the bank and its equal-power pan
        public GainRamp Gain;
        public int Pending;                 // note waiting for silence (-2 = none)

        /// <summary>Control rate: increments for time t (detune and drift).</summary>
        public void Pitch(double t, double invFs)
        {
            if (Note < 0) return;
            double hz = NocAudio.MidiHz(Note);
            double dS = DriftCents * Math.Sin(NocAudio.TwoPi * (DriftHz * t + DriftPhS));
            double dT = DriftCents * Math.Sin(NocAudio.TwoPi * (DriftHz * DriftRatio * t + DriftPhT));
            IncS = hz * Math.Exp((-DetuneCents + dS) * (Math.Log(2.0) / 1200.0)) * invFs;
            IncT = hz * Math.Exp((DetuneCents + dT) * (Math.Log(2.0) / 1200.0)) * invFs;
        }

        /// <summary>One sample of the pair (before gain and pan); the phases always advance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Tick(bool sound)
        {
            double x = 0.0;
            if (sound) x = SawMix * Blep.Saw(PhS, IncS) + TriMix * Blep.Tri(PhT, IncT);
            PhS += IncS; if (PhS >= 1.0) PhS -= 1.0;
            PhT += IncT; if (PhT >= 1.0) PhT -= 1.0;
            return x;
        }
    }

    public sealed class PadEngine
    {
        public const int Slots = 5, Banks = 2 * Slots;
        public const double FilterQ = 0.75, LfoHz = 0.04, LfoOctaves = 0.3, CutoffSmoothSeconds = 0.02, GainSeconds = 0.05;
        const double SilentGain = 1e-4;

        /// <summary>Voice plan: Plan[c, slot] = MIDI note of chord c in that slot (-1 none); Run counts the note changes per slot.</summary>
        public readonly int[,] Plan, Run;
        readonly PadVoice[] m_v = new PadVoice[Banks];
        readonly double[] m_target = new double[Banks];
        readonly double m_fs, m_invFs;
        SvfD m_lpL, m_lpR;
        LogFreqSmoother m_fc;
        public readonly StereoChorus Chorus;
        // unit-RMS normaliser table: note 24..96 x cutoff grid 300..2 400 Hz (log), analytic harmonic sums (R5 3.5 BankNorm)
        const int NoteLo = 24, NoteHi = 96, CutN = 25;
        const double CutLo = 300.0, CutHi = 2400.0;
        readonly double[] m_norm = new double[(NoteHi - NoteLo + 1) * CutN];
        double m_cutoff, m_baseCutoff;
        int m_chord = -1;
        double m_weight;

        /// <summary>Diagnostics for tests: the active chord window, its weight, the current base cutoff and the smoothed cutoff.</summary>
        public int Chord => m_chord;
        public double Weight => m_weight;
        public double Cutoff => m_cutoff;
        public double BaseCutoff => m_baseCutoff;

        public PadEngine(double fs, double controlRate, uint seed)
        {
            m_fs = fs; m_invFs = 1.0 / fs;
            Plan = BuildPlan(NocturneScore.Chords, Slots, out Run);
            m_fc.Init(NocturneScore.PadCutoffHz(0.0), CutoffSmoothSeconds, controlRate);
            m_lpL.Set(NocturneScore.PadCutoffHz(0.0), FilterQ, fs); m_lpR.CopyCoefficients(in m_lpL);
            Chorus = new StereoChorus(fs);
            for (int b = 0; b < Banks; b++)
            {
                m_v[b].Note = -1; m_v[b].Pending = -2;
                m_v[b].PhS = NocAudio.Hash01(seed, (ulong)(2 * b)); m_v[b].PhT = NocAudio.Hash01(seed, (ulong)(2 * b + 1));
                m_v[b].DriftPhS = NocAudio.Hash01(seed + 7u, (ulong)b); m_v[b].DriftPhT = NocAudio.Hash01(seed + 9u, (ulong)b);
                m_v[b].Gain.Init(GainSeconds, controlRate);
            }
            for (int n = NoteLo; n <= NoteHi; n++)
                for (int c = 0; c < CutN; c++)
                    m_norm[(n - NoteLo) * CutN + c] = UnitNorm(NocAudio.MidiHz(n), CutLo * Math.Pow(CutHi / CutLo, c / (double)(CutN - 1)), fs);
        }

        /// <summary>Common tones pinned to their slot; the remaining notes by pitch order (silent slots sort last); R5 3.5.</summary>
        public static int[,] BuildPlan(int[][] chords, int slots, out int[,] run)
        {
            int nc = chords.Length;
            var plan = new int[nc, slots];
            run = new int[nc, slots];
            for (int i = 0; i < slots; i++) plan[0, i] = i < chords[0].Length ? chords[0][i] : -1;
            var slotFree = new bool[slots];
            var noteFree = new bool[16];
            var order = new int[slots];
            for (int c = 1; c < nc; c++)
            {
                int[] next = chords[c];
                for (int i = 0; i < slots; i++) slotFree[i] = true;
                for (int j = 0; j < next.Length; j++) noteFree[j] = true;
                for (int i = 0; i < slots; i++)
                {
                    int j = Array.IndexOf(next, plan[c - 1, i]);
                    if (plan[c - 1, i] >= 0 && j >= 0 && noteFree[j]) { plan[c, i] = next[j]; slotFree[i] = false; noteFree[j] = false; }
                }
                int nf = 0;
                for (int i = 0; i < slots; i++) if (slotFree[i]) order[nf++] = i;
                for (int a = 1; a < nf; a++)
                    for (int b = a; b > 0 && Key(plan[c - 1, order[b]]) < Key(plan[c - 1, order[b - 1]]); b--)
                    { int t = order[b]; order[b] = order[b - 1]; order[b - 1] = t; }
                int k = 0;
                for (int j = 0; j < next.Length; j++) if (noteFree[j]) plan[c, order[k++]] = next[j];   // next[] ascending
                for (; k < nf; k++) plan[c, order[k]] = -1;
                for (int i = 0; i < slots; i++) run[c, i] = run[c - 1, i] + (plan[c, i] != plan[c - 1, i] ? 1 : 0);
            }
            return plan;
        }
        static int Key(int note) => note < 0 ? int.MaxValue : note;

        /// <summary>Pan by pitch (not by slot): low notes centred, a held note never moves.</summary>
        public static double PanOf(int note) => NocMath.Smootherstep(36.0, 52.0, note) * 0.6 * (2.0 * NocAudio.Hash01(0xC0FFEEUL, (ulong)note) - 1.0);

        /// <summary>Normaliser so one bank (saw + triangle through the low-pass at fc) has RMS 1 / sqrt(5): five banks sum to unit power.</summary>
        public static double UnitNorm(double hz, double fc, double fs)
        {
            double p = 0.0;
            for (int k = 1; k * hz < 0.45 * fs; k++)
            {
                double aS = PadVoice.SawMix * 2.0 / (Math.PI * k);
                double aT = (k & 1) == 1 ? PadVoice.TriMix * 8.0 / (Math.PI * Math.PI * k * k) : 0.0;
                p += 0.5 * (aS * aS + aT * aT) * SvfD.LowPower(k * hz / fc, FilterQ);
            }
            return 1.0 / Math.Sqrt(Math.Max(1e-30, p) * Slots);
        }

        double NormAt(int note, double fc)
        {
            int n = Math.Max(NoteLo, Math.Min(NoteHi, note));
            double x = Math.Log(Math.Max(CutLo, Math.Min(CutHi, fc)) / CutLo) / Math.Log(CutHi / CutLo) * (CutN - 1);
            int i = Math.Min(CutN - 2, (int)x);
            double f = x - i;
            int o = (n - NoteLo) * CutN;
            return m_norm[o + i] + f * (m_norm[o + i + 1] - m_norm[o + i]);
        }

        /// <summary>The bank (0..9) that holds slot `slot` of chord c.</summary>
        public int BankOf(int c, int slot) => 2 * slot + (Run[c, slot] & 1);

        /// <summary>Control rate (every 32 samples): chord pair and weight from s, bank targets, pitches from t, the cutoff.</summary>
        public void Control(double s, double t)
        {
            int c = NocturneScore.ChordIndex(s, out double w);
            m_chord = c; m_weight = w;
            for (int b = 0; b < Banks; b++) m_target[b] = 0.0;
            int pc = c > 0 ? c - 1 : 0;
            for (int i = 0; i < Slots; i++)
            {
                int oldNote = Plan[pc, i], newNote = Plan[c, i];
                int bOld = BankOf(pc, i), bNew = BankOf(c, i);
                if (oldNote == newNote || c == 0)
                {
                    Want(bNew, newNote, newNote >= 0 ? 1.0 : 0.0);
                    continue;
                }
                double gO = Math.Cos(0.5 * Math.PI * w), gN = Math.Sin(0.5 * Math.PI * w);   // equal power (R5 3.5)
                Want(bOld, oldNote, oldNote >= 0 ? gO : 0.0);
                Want(bNew, newNote, newNote >= 0 ? gN : 0.0);
            }
            m_baseCutoff = NocturneScore.PadCutoffHz(s);
            double lfo = LfoHz * t - Math.Floor(LfoHz * t);
            m_cutoff = m_fc.Next(m_baseCutoff * Math.Pow(2.0, LfoOctaves * Math.Sin(NocAudio.TwoPi * lfo)));
            m_lpL.Set(m_cutoff, FilterQ, m_fs); m_lpR.CopyCoefficients(in m_lpL);
            for (int b = 0; b < Banks; b++)
            {
                ref PadVoice v = ref m_v[b];
                // a bank only changes its note while silent: a jump of s first fades the old note out
                if (v.Pending != -2)
                {
                    if (v.Gain.Cur < SilentGain) { Assign(b, v.Pending); v.Pending = -2; }
                    else m_target[b] = 0.0;
                }
                v.Gain.Step(m_target[b]);
                if (v.Note >= 0) v.Norm = NormAt(v.Note, m_baseCutoff);
                v.Pitch(t, m_invFs);
            }
        }

        void Want(int b, int note, double gain)
        {
            ref PadVoice v = ref m_v[b];
            if (v.Note != note && note >= 0)
            {
                if (v.Note < 0 || v.Gain.Cur < SilentGain) { Assign(b, note); v.Pending = -2; }
                else { v.Pending = note; gain = 0.0; }
            }
            else if (v.Note == note) v.Pending = -2;
            m_target[b] = gain;
        }

        void Assign(int b, int note)
        {
            ref PadVoice v = ref m_v[b];
            v.Note = note;
            if (note < 0) return;
            v.Norm = NormAt(note, m_baseCutoff > 0 ? m_baseCutoff : NocturneScore.PadCutoffHz(0.0));
            NocAudio.Pan(PanOf(note), out v.PanL, out v.PanR);
        }

        /// <summary>Jump the smoothers to the targets of (s, t) (offline renders that start mid-walk).</summary>
        public void Snap(double s, double t)
        {
            Control(s, t);
            for (int b = 0; b < Banks; b++)
            {
                ref PadVoice v = ref m_v[b];
                if (v.Pending != -2) { Assign(b, v.Pending); v.Pending = -2; }
                v.Gain.Snap(m_target[b]);
            }
            m_fc.Snap(m_cutoff);
        }

        /// <summary>One stereo sample (u = position inside the control block, 0..1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(double u, out double outL, out double outR)
        {
            double l = 0.0, r = 0.0;
            for (int b = 0; b < Banks; b++)
            {
                ref PadVoice v = ref m_v[b];
                bool sound = v.Note >= 0 && !v.Gain.Silent;
                double x = v.Tick(sound);
                if (!sound) continue;
                x *= v.Gain.At(u) * v.Norm;
                l += v.PanL * x; r += v.PanR * x;
            }
            l = m_lpL.Low(l); r = m_lpR.Low(r);
            Chorus.Tick(ref l, ref r);
            outL = l; outR = r;
        }

        public void Flush() { m_lpL.Flush(); m_lpR.Flush(); }

        /// <summary>Bank state for tests: note and current gain.</summary>
        public void Bank(int b, out int note, out double gain) { note = m_v[b].Note; gain = m_v[b].Gain.Cur; }
    }
}
