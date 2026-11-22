// NocturneScore: the score of Nocturne as pure functions of progress s (m) and time t (s) (MASTER_PROMPT 14.3, 14.6, 14.7 H7 /
// H8; docs/AUDIO.md). Everything the DSP plays is keyed here: the 14.3 layer table (R D H T W P K X, RMS dBFS of unit-RMS
// layers) blended over the 24 m Rule-P windows, the silences and swells S1-S8, the per-beat trims (QA/audio-trims.json), the
// chord table with its change windows, the pad cutoff from the 14.1 intensity curve, the reverb places and their 14.6
// windows, the koi distance law, the tram and train schedules (launch keyed to s, then real time), the footstep surfaces and
// the rain surface colours. NocturneScore.Render(s0, s1, speed, path) renders offline to a WAV (OfflineAudioRenderer).
// Pure C#, no UnityEngine, allocation-free after the static tables.
using System;

namespace Megastructure
{
    public static class NocturneScore
    {
        // ------------------------------------------------------------------ layers (14.3)
        public const int R = 0, D = 1, H = 2, T = 3, W = 4, P = 5, K = 6, X = 7, LayerCount = 8;
        /// <summary>Footsteps (not in the 14.3 table: -30 dBFS peaks), the D-034 dread layer E (docs/DREAD.md, DreadScore; not in the
        /// 14.3 table: its levels are dBFS at the listener) and the number of stems.</summary>
        public const int F = 8, E = 9, StemCount = 10;
        public static readonly string[] LayerNames = { "R rain", "D drips", "H hum", "T traffic", "W wind", "P pad", "K koi", "X beat", "F steps", "E dread" };
        const double N = double.NaN;

        /// <summary>14.3 layer table, RMS dBFS before the limiter (rows beats 1-7, columns R D H T W P K X; NaN = "-").</summary>
        public static readonly double[,] LayerDb =
        {
            { -40, -46, -42, -52,   N, -40,   N, -45 },
            { -39, -47, -40, -38,   N, -37,   N, -35 },
            { -40, -52, -45, -41, -39, -34,   N, -60 },
            { -39, -55, -47, -42, -36, -35,   N, -35 },
            { -41, -47, -50, -56, -52, -36,   N, -40 },
            { -40, -50, -33, -45, -50, -35, -60, -60 },
            { -41, -50, -44, -48, -48, -32, -33, -60 },
        };

        /// <summary>The 14.3 Sum column (power sum of each row).</summary>
        public static double SumDb(int beat)
        {
            double p = 0;
            for (int l = 0; l < LayerCount; l++) { double v = LayerDb[beat, l]; if (!double.IsNaN(v)) p += Math.Pow(10.0, v / 10.0); }
            return 10.0 * Math.Log10(p);
        }

        /// <summary>Per-beat mix trims (dB, every layer of the beat; logged in QA/audio-trims.json; 14.3 allows logged trims).</summary>
        public static readonly double[] BeatTrimDb = { 0.25, -0.39, -0.08, -0.13, -0.21, 0.03, 0.49 };
        /// <summary>Beat-7 pad trim outside S7 (R5 6 (b): the koi law puts K at -33 only inside S7, which the gate excludes).</summary>
        public const double PadTrimBeat7Db = 1.0;

        static readonly double[][] s_amp = BuildAmp();
        static double[][] BuildAmp()
        {
            var a = new double[LayerCount][];
            for (int l = 0; l < LayerCount; l++)
            {
                a[l] = new double[NocturneConstants.BeatCount];
                for (int b = 0; b < NocturneConstants.BeatCount; b++)
                {
                    double v = LayerDb[b, l];
                    a[l][b] = double.IsNaN(v) ? 0.0 : Math.Pow(10.0, (v + BeatTrimDb[b] + (l == P && b == 6 ? PadTrimBeat7Db : 0.0)) / 20.0);
                }
            }
            return a;
        }

        /// <summary>Table amplitude of layer l at s with the 24 m Rule-P blends (14.6), before the S-windows.</summary>
        public static double TableAmp(int l, double s) => NocMath.BeatBlend(s_amp[l], s);

        /// <summary>Membership weight of beat b at s (Rule P: smootherstep over the 24 m around each boundary; the weights sum to 1).</summary>
        public static double BeatWeight(int b, double s)
        {
            var bb = NocturneConstants.BeatBoundaries;
            double h = 0.5 * NocturneConstants.BlendWindow;
            double lo = b == 0 ? 1.0 : NocMath.Smootherstep(bb[b] - h, bb[b] + h, s);
            double hi = b == NocturneConstants.BeatCount - 1 ? 0.0 : NocMath.Smootherstep(bb[b + 1] - h, bb[b + 1] + h, s);
            return lo - hi;
        }

        // ------------------------------------------------------------------ silences and swells (14.3 S1-S8)
        /// <summary>The S-windows the level gate excludes (S3, S4, S6, S7; logged separately).</summary>
        public static readonly double[] GateExcludeS0 = { 400.0, 540.0, 876.0, 1340.0 };
        public static readonly double[] GateExcludeS1 = { 420.0, 554.0, 896.0, 1380.0 };
        public const double S1End = 8.0, PadAttackSeconds = 6.0, PadReleaseSeconds = 8.0;
        public const double GoalTriggerS = 1497.0;

        /// <summary>Smootherstep plateau over [a, b] with ramps of `ramp` metres centred on the edges.</summary>
        public static double Bump(double s, double a, double b, double ramp)
        {
            double h = 0.5 * ramp;
            return NocMath.Smootherstep(a - h, a + h, s) * (1.0 - NocMath.Smootherstep(b - h, b + h, s));
        }

        /// <summary>True when s lies in an excluded S-window (S3, S4, S6, S7).</summary>
        public static bool InGateExclusion(double s)
        {
            for (int i = 0; i < GateExcludeS0.Length; i++) if (s >= GateExcludeS0[i] && s <= GateExcludeS1[i]) return true;
            return false;
        }

        /// <summary>S-window gain (linear) of layer l at s (S1 .. S7; S8 is the chord table and the release).</summary>
        public static double SwellGain(int l, double s)
        {
            double db = 0.0;
            // S1 s 0-8: only rain and hum (the pad enters by its own 6 s attack at s 8)
            double s1 = NocMath.Smootherstep(4.0, 14.0, s);
            // S2 s 104-124: the reverb opens (the 100-130 window), traffic +6 dB
            if (l == T) db += 6.0 * Bump(s, 104.0, 124.0, 6.0);
            // S3 s 400-420: the pad drops 6 dB, the wind rises
            double b3 = Bump(s, 400.0, 420.0, 6.0);
            if (l == P) db -= 6.0 * b3;
            if (l == W) db += 5.0 * b3;
            // S4 s 540-554: everything ducks 5 dB except wind and deep hum (+3 dB)
            double b4 = Bump(s, 540.0, 554.0, 4.0);
            if (l == W || l == H) db += 3.0 * b4; else db -= 5.0 * b4;
            // S6 s 876-896: wind and traffic fall 12 dB, leaf rain rises
            double b6 = Bump(s, 876.0, 896.0, 6.0);
            if (l == W || l == T) db -= 12.0 * b6;
            // S7 s 1 340-1 380: the koi tone swells (its distance law peaks here) and ducks the pad 3 dB
            if (l == P) db -= (3.0 + PadTrimBeat7Db) * Bump(s, 1340.0, 1380.0, 8.0);
            double g = Math.Pow(10.0, db / 20.0);
            if (l == T || l == W || l == K || l == X) g *= s1;
            return g;
        }

        /// <summary>Leaf-rain rise inside S6 (dB, applied to the beat-5 X source only).</summary>
        public static double LeafRainSwellDb(double s) => 4.0 * Bump(s, 876.0, 896.0, 6.0);

        /// <summary>Full amplitude of layer l at s (table x trims x swells); K is the koi law instead of the table.</summary>
        public static double LayerAmp(int l, double s)
        {
            if (l == K) return KoiAmp(s) * SwellGain(K, s);
            if (l == P) return TableAmp(P, s) * SwellGain(P, s) * Math.Pow(10.0, PadTrimDb(s) / 20.0);
            return TableAmp(l, s) * SwellGain(l, s);
        }

        /// <summary>Release after the goal trigger (S8 "the last seconds are rain, hum and the lamp's buzz"): (1 - te / 8)^2.</summary>
        public static double ReleaseGain(double secondsSinceGoal)
        {
            if (secondsSinceGoal <= 0.0) return 1.0;
            double u = 1.0 - secondsSinceGoal / PadReleaseSeconds;
            return u <= 0.0 ? 0.0 : u * u;
        }

        /// <summary>Pad entry (S1: the pad enters over 6 s once s first passes 8): smoothstep in amplitude.</summary>
        public static double AttackGain(double secondsSinceEntry) => NocMath.Smoothstep(0.0, PadAttackSeconds, secondsSinceEntry);

        // ------------------------------------------------------------------ pad chords (14.3 table, R5 3.5)
        public static readonly double[] ChordChangeS = { 0, 124, 286, 402, 540, 700, 880, 1080, 1262, 1350, 1486 };
        public static readonly string[] ChordNames = { "Dm9", "Bbmaj9", "Gm11", "Ebmaj7#11", "Fmaj9", "Dm11", "Dmaj9", "Bm11", "Gmaj9#11", "Em9", "D5" };
        public static readonly int[][] Chords =
        {
            new[] { 38, 45, 53, 60, 64 }, new[] { 46, 53, 57, 60, 62 }, new[] { 43, 50, 53, 58, 60 }, new[] { 39, 46, 50, 55, 57 },
            new[] { 41, 48, 57, 64, 67 }, new[] { 38, 45, 53, 60, 67 }, new[] { 38, 45, 54, 61, 64 }, new[] { 47, 54, 57, 62, 64 },
            new[] { 43, 50, 54, 57, 61 }, new[] { 40, 47, 50, 55, 66 }, new[] { 38, 45, 50 },
        };
        /// <summary>Half-width of each change window (m): 40 m smootherstep crossfades; C10 uses 14 m so it is complete at s 1 500 (D-011).</summary>
        public static readonly double[] ChordHalfWindow = { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 14 };

        /// <summary>The chord c whose window is active at s and the weight w of c against c - 1 (sum-to-1 weights g_c = w, g_c-1 = 1 - w).</summary>
        public static int ChordIndex(double s, out double w)
        {
            int c = 0;
            for (int i = 1; i < ChordChangeS.Length; i++) if (s >= ChordChangeS[i] - ChordHalfWindow[i]) c = i;
            w = c == 0 ? 1.0 : NocMath.Smootherstep(ChordChangeS[c] - ChordHalfWindow[c], ChordChangeS[c] + ChordHalfWindow[c], s);
            return c;
        }

        /// <summary>Per-chord level trims of the pad (dB): the five voices of each chord, normalised per bank, still differ by up to
        /// +-1.5 dB after the chorus and the pad FDN (measured by Tools/audio/calibrate.py on the 2.5 m/s walk, run walk25_r03).
        /// Fix wave C51 (2026-11-21): the pad FDN's wet side narrowed to NocMixer.PadWetWidth moved the chords by +0.31 +0.92 +0.83
        /// +0.61 +0.11 +0.61 -1.45 +0.76 -0.31 +1.48 dB (pad solo, f05 vs c51); the trims take those shifts back (all within 2 dB).</summary>
        public static readonly double[] PadChordTrimDb = { 0.19, -1.12, -1.73, -0.51, 1.19, 0.19, 0.65, -0.66, -1.24, -1.58, 0.0 };

        /// <summary>The pad trim at s (dB): the chord weights g_c(s) blend the per-chord trims.</summary>
        public static double PadTrimDb(double s)
        {
            int k = ChordIndex(s, out double w);
            return k == 0 ? PadChordTrimDb[0] : (1.0 - w) * PadChordTrimDb[k - 1] + w * PadChordTrimDb[k];
        }

        /// <summary>Chord gain g_c(s) of chord c (sum over c is 1 at every s).</summary>
        public static double ChordGain(int c, double s)
        {
            int k = ChordIndex(s, out double w);
            if (c == k) return w;
            if (c == k - 1) return 1.0 - w;
            return 0.0;
        }

        // ------------------------------------------------------------------ the 14.1 intensity curve -> pad cutoff (R5 3.3)
        static readonly double[] IntensityStart = { 3, 5, 8, 8, 5, 5, 9 };
        static readonly double[] IntensityEnd = { 5, 8, 10, 7, 4, 6, 6 };

        /// <summary>Emotion intensity 0-10 at s: linear inside each beat, smootherstep-blended over the 24 m around the boundaries.</summary>
        public static double Intensity(double s)
        {
            var bb = NocturneConstants.BeatBoundaries;
            double v = 0.0;
            for (int b = 0; b < NocturneConstants.BeatCount; b++)
            {
                double w = BeatWeight(b, s);
                if (w <= 0.0) continue;
                double u = (s - bb[b]) / (bb[b + 1] - bb[b]);
                v += w * (IntensityStart[b] + (IntensityEnd[b] - IntensityStart[b]) * NocMath.Clamp01(u));
            }
            return v;
        }

        /// <summary>Pad low-pass base cutoff 500 x 3.2^((I - 3) / 7) Hz: intensity 3 -> 500 Hz, 10 -> 1 600 Hz (14.7 H8 range).</summary>
        public static double PadCutoffHz(double s) => 500.0 * Math.Pow(3.2, (Intensity(s) - 3.0) / 7.0);

        // ------------------------------------------------------------------ reverb places (14.3) and their windows (14.6)
        public static readonly string[] PlaceNames = { "alley", "boulevard", "edge", "bridge", "garden", "signs", "plaza" };
        public static readonly double[] PlaceRt60 = { 0.9, 2.8, 4.5, 3.5, 1.6, 2.2, 3.8 };
        public static readonly double[] PlacePreDelayMs = { 8, 30, 250, 60, 20, 25, 70 };
        public static readonly double[] PlaceDamping = { 0.6, 0.45, 0.5, 0.5, 0.7, 0.5, 0.45 };
        /// <summary>Wet level of the environment FDN per place (dB re dry; not in the spec: small rooms drier, open spaces wetter).</summary>
        public static readonly double[] PlaceWetDb = { -14, -12, -10, -11, -14, -12, -11 };
        /// <summary>Blend windows between consecutive places: 14.6 (100-130, 380-420), else 40 m centred on the boundary (14.3).</summary>
        public static readonly double[] PlaceBlend0 = { 100, 380, 540, 860, 1060, 1240 };
        public static readonly double[] PlaceBlend1 = { 130, 420, 580, 900, 1100, 1280 };

        /// <summary>Damping 0..1 (14.3) to the FDN's loop low-pass (Hz): 12 kHz x (1 - d)^1.5 (0.45 -> 4.9 kHz, 0.7 -> 2.0 kHz).</summary>
        public static double DampingHz(double d) => 12000.0 * Math.Pow(Math.Max(0.02, 1.0 - d), 1.5);

        /// <summary>Environment reverb at s: RT60 (s), pre-delay (ms), damping (Hz), wet (dB); smootherstep blends (14.6).</summary>
        public static void Reverb(double s, out double rt60, out double preMs, out double dampHz, out double wetDb)
        {
            rt60 = PlaceRt60[0]; preMs = PlacePreDelayMs[0]; double damp = PlaceDamping[0]; wetDb = PlaceWetDb[0];
            for (int i = 0; i < PlaceBlend0.Length; i++)
            {
                double w = NocMath.Smootherstep(PlaceBlend0[i], PlaceBlend1[i], s);
                if (w <= 0.0) break;
                rt60 += w * (PlaceRt60[i + 1] - PlaceRt60[i]);
                preMs += w * (PlacePreDelayMs[i + 1] - PlacePreDelayMs[i]);
                damp += w * (PlaceDamping[i + 1] - PlaceDamping[i]);
                wetDb += w * (PlaceWetDb[i + 1] - PlaceWetDb[i]);
            }
            dampHz = DampingHz(damp);
        }

        /// <summary>The pad FDN (14.3: decay 6 s, pre-delay 40 ms; size 2.5, damping 4 kHz, wet -3 dB re dry; R5 3.6).</summary>
        public const double PadReverbRt60 = 6.0, PadReverbPreMs = 40.0, PadReverbDampHz = 4000.0, PadReverbWetDb = -3.0, PadReverbSize = 2.5;
        public const double EnvReverbSize = 2.2, EnvReverbMaxPreDelay = 0.30, ReverbBlendSeconds = 0.5;

        // ------------------------------------------------------------------ koi tone (14.3 "Koi tone" K)
        public const double KoiPeakDb = -33.0, KoiLawDistance = 400.0, KoiTeaserS0 = 1200.0, KoiTeaserS1 = 1290.0;
        static readonly double s_koiLawPeak = KoiLawPeak();

        /// <summary>Distance (m) from the listener at the path point of s (eye 1.7) to the koi head (KoiMath orbit and head azimuth).</summary>
        public static double KoiDistance(double s)
        {
            D3 h = KoiMath.OrbitPoint(KoiMath.HeadAzimuth(KoiMath.PhaseU(s)));
            D3 e = NocMath.PathPoint(s);
            double dx = h.x - e.x, dy = h.y - NocturneConstants.EyeHeight, dz = h.z - e.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>Unnormalised koi law 1 / (1 + (d / 400)^2) times the teaser ramp (it rises faintly from s 1 200).</summary>
        public static double KoiLaw(double s)
        {
            double d = KoiDistance(s) / KoiLawDistance;
            return NocMath.Smootherstep(KoiTeaserS0, KoiTeaserS1, s) / (1.0 + d * d);
        }

        static double KoiLawPeak()
        {
            double m = 1e-9;
            for (double s = KoiTeaserS0; s <= NocturneConstants.GoalS; s += 0.5) m = Math.Max(m, KoiLaw(s));
            return m;
        }

        /// <summary>Koi tone amplitude at s: the law normalised so its peak is -33 dBFS (peak at s 1 350-1 380).</summary>
        public static double KoiAmp(double s) => KoiLaw(s) / s_koiLawPeak * Math.Pow(10.0, (KoiPeakDb + BeatTrimDb[6]) / 20.0);

        /// <summary>s of the koi law's peak (tests: 14.3 "peak at s 1 350-1 380").</summary>
        public static double KoiPeakS()
        {
            double best = 0, bs = 0;
            for (double s = KoiTeaserS0; s <= NocturneConstants.GoalS; s += 0.25) { double v = KoiLaw(s); if (v > best) { best = v; bs = s; } }
            return bs;
        }

        // ------------------------------------------------------------------ vehicles (14.5.2, 14.5.4, 14.7 H7)
        /// <summary>A vehicle on a straight track parallel to x: launched when s first crosses SLaunch, then x(tau) = X0 + Dir V tau.</summary>
        public struct Vehicle
        {
            public string Name;
            public double SLaunch, X0, Dir, Speed, Length, TrackY, TrackZ, OpenX0, OpenX1;
            public bool Train;
            public Vehicle(string name, double sl, double x0, double dir, double v, double len, double y, double z, double ox0, double ox1, bool train)
            { Name = name; SLaunch = sl; X0 = x0; Dir = dir; Speed = v; Length = len; TrackY = y; TrackZ = z; OpenX0 = ox0; OpenX1 = ox1; Train = train; }
            /// <summary>Head (nose) x at tau seconds after the launch; the body trails behind the direction of travel.</summary>
            public double HeadX(double tau) => X0 + Dir * Speed * Math.Max(0.0, tau);
        }

        public const double TrainSpeed = 38.0, TrainLength = 174.0, TramSpeed = 7.0, TramLength = 60.0;
        public const double TubeNorthZ = 207.0, TubeSouthZ = 183.0, TubeAxisY = 2.0, UpperTubeY = 118.0, UpperTubeZ = 195.0, LowerTubeY = -142.0, LowerTubeZ = 209.0;
        public const double BridgeTubeX0 = 384.0, BridgeTubeX1 = 683.0, FaceAX = 215.3, FaceBX = 815.3;
        /// <summary>Reset rule of 14.7 H7: if s falls 12 m below s_launch the vehicle resets.</summary>
        public const double VehicleResetBelow = 12.0;

        /// <summary>The tram T_A (14.5.2: launch s 230 from the terminus x 205 on z 227, west at 7 m/s) and T1-T7 (14.5.4).</summary>
        public static readonly Vehicle[] Vehicles =
        {
            new Vehicle("tram", 230.0, 205.0, -1, TramSpeed, TramLength, 1.8, 227.0, -4000.0, 205.0, false),
            new Vehicle("T1", 584.5, BridgeTubeX1, -1, TrainSpeed, TrainLength, TubeAxisY, TubeNorthZ, BridgeTubeX0, BridgeTubeX1, true),
            new Vehicle("T3", 639.5, BridgeTubeX1, -1, TrainSpeed, TrainLength, TubeAxisY, TubeNorthZ, BridgeTubeX0, BridgeTubeX1, true),
            new Vehicle("T4", 690.7, BridgeTubeX0, +1, TrainSpeed, TrainLength, TubeAxisY, TubeSouthZ, BridgeTubeX0, BridgeTubeX1, true),
            new Vehicle("T5", 600.0, FaceBX, -1, TrainSpeed, TrainLength, UpperTubeY, UpperTubeZ, FaceAX, FaceBX, true),
            new Vehicle("T6", 695.0, FaceAX, +1, TrainSpeed, TrainLength, LowerTubeY, LowerTubeZ, FaceAX, FaceBX, true),
            new Vehicle("T2", 752.0, BridgeTubeX1, -1, TrainSpeed, TrainLength, TubeAxisY, TubeSouthZ, BridgeTubeX0, BridgeTubeX1, true),
            new Vehicle("T7", 800.0, BridgeTubeX0, +1, TrainSpeed, TrainLength, TubeAxisY, TubeNorthZ, BridgeTubeX0, BridgeTubeX1, true),
        };

        /// <summary>Capture-mode tau of 14.7 H7: max(0, (s - s_launch) / 2.5).</summary>
        public static double CaptureTau(int v, double s) => Math.Max(0.0, (s - Vehicles[v].SLaunch) / NocturneConstants.CinematicSpeed);

        /// <summary>At a constant walk speed v0 from s 0: tau of vehicle i when the walker is at s.</summary>
        public static double TauAtConstantSpeed(int i, double s, double speed) => Math.Max(0.0, (s - Vehicles[i].SLaunch) / Math.Max(0.05, speed));

        /// <summary>Walker s at which the head of vehicle i is abeam (head x = walker x on the east leg x = s - 184.70), constant speed.</summary>
        public static double MeetS(int i, double speed)
        {
            var v = Vehicles[i];
            // x_w(s) = s - 184.70; x_h = X0 + Dir V (s - SL) / speed  ->  s = (X0 + 184.70 - Dir V SL / speed) / (1 - Dir V / speed)
            double k = v.Dir * v.Speed / speed;
            return (v.X0 + NocturneConstants.EastLegOffset - k * v.SLaunch) / (1.0 - k);
        }

        // ------------------------------------------------------------------ rain coupling (the picture's rules, mirrored so the DSP has
        // no dependency on the rendering code; AudioScoreTests checks they equal NocSchedule.RainRho and NocRainMath.Gust)
        /// <summary>Rain density rho of the 14.2.4 schedule at s (Rule-P blended per beat).</summary>
        public static double RainRhoSchedule(double s) => NocMath.BeatBlend(NocturneConstants.RainRho, s);

        /// <summary>Gust weight of beat 4 (Rule P over 24 m around s 560 and 880): the picture's rain slant gusts.</summary>
        public static double RainGustWeight(double s)
        {
            double w = 0.5 * NocturneConstants.BlendWindow;
            return NocMath.Smootherstep(560.0 - w, 560.0 + w, s) * (1.0 - NocMath.Smootherstep(880.0 - w, 880.0 + w, s));
        }

        // ------------------------------------------------------------------ surfaces
        /// <summary>Footstep surface at s (14.4 "Surfaces"; the controller's own rule, NocEvents.SurfaceAt).</summary>
        public static FootSurface SurfaceAt(double s) => NocEvents.SurfaceAt(s);

        /// <summary>Rain surface colour at s: patter band centre (Hz) and the metal / glass ping share (rain "by surface").</summary>
        public static void RainSurface(double s, out double patterHz, out double pingShare, out double pingHz)
        {
            // per beat: alley concrete + awnings, boulevard stone, Edge stone (wind-driven), bridge steel deck + glass tubes,
            // garden leaves (soft, low), signs stone, plaza stone
            double[] hz = s_patterHz, ps = s_pingShare, ph = s_pingHz;
            patterHz = NocMath.BeatBlend(hz, s); pingShare = NocMath.BeatBlend(ps, s); pingHz = NocMath.BeatBlend(ph, s);
        }
        static readonly double[] s_patterHz = { 4200, 3600, 3400, 5200, 2600, 3600, 3400 };
        static readonly double[] s_pingShare = { 0.25, 0.0, 0.0, 0.45, 0.0, 0.0, 0.0 };
        static readonly double[] s_pingHz = { 2300, 2600, 2600, 3900, 3000, 2800, 2600 };

        // ------------------------------------------------------------------ offline render (14.7 H8)
        /// <summary>Renders [s0, s1] at a constant speed to a 32-bit float WAV (stereo, 48 kHz); returns the frames written.</summary>
        public static long Render(double s0, double s1, double speed, string path, int seed = 1, bool footsteps = true)
        {
            var st = new OfflineRenderSettings { S0 = s0, S1 = s1, Speed = speed, Seed = seed, Footsteps = footsteps };
            using (var wav = new WavStreamWriter(path, OfflineAudioRenderer.Channels, st.SampleRate, WavFormat.Float32))
                return OfflineAudioRenderer.Render(st, wav);
        }
    }
}
