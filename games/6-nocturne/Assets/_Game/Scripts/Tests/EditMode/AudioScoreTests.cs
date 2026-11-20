using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    /// <summary>
    /// The score of Nocturne (AU, Phase 4 wave A; MASTER_PROMPT 14.3, 14.6, 14.7 H8, Part 10 item 25, 11.9; docs/AUDIO.md):
    /// the 14.3 layer table and its Sum column, the per-beat level gate (median of the 3 s RMS windows outside S3/S4/S6/S7 within
    /// +-1.5 dB of Sum) at 2.5 and 1.5 m/s, true peak / DC / sample-step limits, the chord table and its sum-to-1 smootherstep
    /// gains, the voice plan (common tones stay on one voice), the pad's click-free changes (bank notes change only in silence,
    /// bounded gain slew, equal power, 20 ms envelope steps &lt; 0.05), the S-windows, the koi law, the reverb places and their
    /// 14.6 windows, the rain coupling to the picture, the tram / train schedules, footstep cadence and variation, the player
    /// path against the offline render (the EditMode form of -audio-dump), continuity at any speed, block-size invariance and
    /// zero allocations. The renders use AudioGates (Runtime/Audio/AudioAnalysis.cs), the same code as Tools/audio/harness "gates".
    /// </summary>
    public sealed class AudioScoreTests
    {
        const int Fs = 48000;

        // ------------------------------------------------------------------ the 14.3 table
        [Test]
        public void LayerTable_SumColumnIsThePowerSumAndTrimsAreSmall()
        {
            double[] sum = { -34.9, -30.4, -31.3, -29.6, -33.3, -30.1, -28.9 };
            for (int b = 0; b < NocturneConstants.BeatCount; b++)
            {
                Assert.AreEqual(sum[b], NocturneScore.SumDb(b), 0.06, "14.3 Sum column, beat " + (b + 1));
                Assert.AreEqual(NocturneConstants.TargetRmsDb[b], sum[b], 1e-9, "NocturneConstants.TargetRmsDb");
                Assert.LessOrEqual(Math.Abs(NocturneScore.BeatTrimDb[b]), 1.0, "a logged trim, not a re-mix (beat " + (b + 1) + ")");
            }
            for (int c = 0; c < NocturneScore.PadChordTrimDb.Length; c++)
                Assert.LessOrEqual(Math.Abs(NocturneScore.PadChordTrimDb[c]), 2.0, "pad chord trim C" + c);
            // the table values themselves (rows 1 and 7 spot-checked against the bible)
            Assert.AreEqual(-40.0, NocturneScore.LayerDb[0, NocturneScore.R]); Assert.AreEqual(-45.0, NocturneScore.LayerDb[0, NocturneScore.X]);
            Assert.AreEqual(-32.0, NocturneScore.LayerDb[6, NocturneScore.P]); Assert.AreEqual(-33.0, NocturneScore.LayerDb[6, NocturneScore.K]);
            Assert.IsTrue(double.IsNaN(NocturneScore.LayerDb[0, NocturneScore.W]), "no wind in the alley");
        }

        [Serializable]
        sealed class TrimsFile { public double[] beatTrimDb; public double[] padChordTrimDb; public double padTrimBeat7Db; }

        [Test]
        public void AudioTrimsJson_MatchesTheScore()
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "QA", "audio-trims.json"));
            Assert.IsTrue(File.Exists(path), "14.3: the per-beat trims are logged in QA/audio-trims.json");
            var t = JsonUtility.FromJson<TrimsFile>(File.ReadAllText(path));
            Assert.AreEqual(NocturneScore.BeatTrimDb.Length, t.beatTrimDb.Length);
            for (int b = 0; b < t.beatTrimDb.Length; b++) Assert.AreEqual(NocturneScore.BeatTrimDb[b], t.beatTrimDb[b], 1e-9, "beat trim " + (b + 1));
            Assert.AreEqual(NocturneScore.PadChordTrimDb.Length, t.padChordTrimDb.Length);
            for (int c = 0; c < t.padChordTrimDb.Length; c++) Assert.AreEqual(NocturneScore.PadChordTrimDb[c], t.padChordTrimDb[c], 1e-9, "pad trim C" + c);
            Assert.AreEqual(NocturneScore.PadTrimBeat7Db, t.padTrimBeat7Db, 1e-9);
        }

        // ------------------------------------------------------------------ the level gate (14.3, 14.7 H8)
        static void LevelGate(double speed)
        {
            var sb = new System.Text.StringBuilder();
            for (int b = 0; b < NocturneConstants.BeatCount; b++)
            {
                var an = AudioGates.BeatStretch(b, speed);
                double med = an.BeatMedianDb(b, out int windows);
                double tgt = NocturneConstants.TargetRmsDb[b];
                sb.AppendLine(FormattableString.Invariant($"beat {b + 1}: median {med:0.00} dBFS, target {tgt:0.0}, dev {med - tgt:+0.00;-0.00}, {windows} windows, TP {an.TruePeak.PeakDb:0.00} dBTP, DC {an.Dc:0.0000000}, step {an.MaxStep:0.000}"));
                Assert.GreaterOrEqual(windows, 7, "beat " + (b + 1) + ": 3 s windows outside the S-windows");
                Assert.AreEqual(tgt, med, AudioGates.LevelTolDb, "14.3 level gate, beat " + (b + 1) + " at " + speed + " m/s");
                Assert.LessOrEqual(an.TruePeak.PeakDb, AudioGates.TruePeakLimitDb, "true peak <= -1 dBFS, beat " + (b + 1));
                Assert.Less(Math.Abs(an.Dc), AudioGates.DcLimit, "DC < 0.001, beat " + (b + 1));
                Assert.LessOrEqual(an.MaxStep, AudioGates.StepLimit, "sample step <= 0.25, beat " + (b + 1));
                Assert.AreEqual(0, an.NonFinite, "no NaN / Inf samples");
            }
            Debug.Log("[NOC] AudioScoreTests level gate at " + speed + " m/s\n" + sb);
        }

        [Test, Timeout(900000)]
        public void LevelGate_PerBeatMedianWithin1_5dB_At2_5()
        {
            LevelGate(NocturneConstants.CinematicSpeed);
        }

        [Test, Timeout(900000)]
        public void LevelGate_PerBeatMedianWithin1_5dB_At1_5()
        {
            LevelGate(NocturneConstants.WalkSpeed);
        }

        [Test]
        public void LevelGate_DeviceRate44_1kHzMatches48kHz()
        {
            // 14.3 / Part 3.1 lesson 14: "48 kHz (the device may run at 44.1)": the same score at 44.1 kHz
            var st48 = new OfflineRenderSettings { S0 = 200.0, S1 = 230.0, Speed = 2.5, Seed = 3, PrerollSeconds = 3.0 };
            var st44 = new OfflineRenderSettings { S0 = 200.0, S1 = 230.0, Speed = 2.5, Seed = 3, PrerollSeconds = 3.0, SampleRate = 44100 };
            var a = new MeterSink(); var b = new MeterSink();
            OfflineAudioRenderer.Render(st48, a); OfflineAudioRenderer.Render(st44, b);
            Assert.AreEqual(RmsSink.Db(a.Rms), RmsSink.Db(b.Rms), 0.5, "44.1 kHz level within 0.5 dB of 48 kHz");
            Assert.LessOrEqual(b.PeakDbfs, NocturneConstants.LimiterCeilingDb + 1e-6);
        }

        // ------------------------------------------------------------------ chords (14.3 "Pad", 14.7 H8)
        [Test]
        public void ChordTable_MatchesTheBibleAndMidiHz()
        {
            int[][] want =
            {
                new[] { 38, 45, 53, 60, 64 }, new[] { 46, 53, 57, 60, 62 }, new[] { 43, 50, 53, 58, 60 }, new[] { 39, 46, 50, 55, 57 },
                new[] { 41, 48, 57, 64, 67 }, new[] { 38, 45, 53, 60, 67 }, new[] { 38, 45, 54, 61, 64 }, new[] { 47, 54, 57, 62, 64 },
                new[] { 43, 50, 54, 57, 61 }, new[] { 40, 47, 50, 55, 66 }, new[] { 38, 45, 50 },
            };
            double[] change = { 0, 124, 286, 402, 540, 700, 880, 1080, 1262, 1350, 1486 };
            Assert.AreEqual(want.Length, NocturneScore.Chords.Length);
            for (int c = 0; c < want.Length; c++)
            {
                CollectionAssert.AreEqual(want[c], NocturneScore.Chords[c], "chord C" + c);
                Assert.AreEqual(change[c], NocturneScore.ChordChangeS[c], "change s of C" + c);
            }
            Assert.AreEqual(73.416, NocAudio.MidiHz(38), 0.01, "D2");
            Assert.AreEqual(110.0, NocAudio.MidiHz(45), 1e-9, "A2");
            Assert.AreEqual(174.614, NocAudio.MidiHz(53), 0.01, "F3");
        }

        [Test]
        public void ChordGains_SumToOneWithSmootherstepWindows()
        {
            int n = NocturneScore.ChordChangeS.Length;
            for (double s = 0.0; s <= NocturneConstants.GoalS; s += 0.25)
            {
                double sum = 0.0;
                for (int c = 0; c < n; c++) { double g = NocturneScore.ChordGain(c, s); Assert.GreaterOrEqual(g, 0.0); sum += g; }
                Assert.AreEqual(1.0, sum, 1e-12, "sum of g_c at s " + s);
            }
            for (int c = 1; c < n; c++)
            {
                double sc = NocturneScore.ChordChangeS[c], h = NocturneScore.ChordHalfWindow[c];
                Assert.AreEqual(c < n - 1 ? 20.0 : 14.0, h, 1e-12, "40 m windows (C10: 28 m, complete at s 1 500)");
                Assert.AreEqual(0.5, NocturneScore.ChordGain(c, sc), 1e-12, "half way at the change point");
                Assert.AreEqual(0.0, NocturneScore.ChordGain(c, sc - h - 0.01), 1e-12);
                Assert.AreEqual(1.0, NocturneScore.ChordGain(c, sc + h + 0.01) + (c + 1 < n ? NocturneScore.ChordGain(c + 1, sc + h + 0.01) : 0.0), 1e-12);
                for (double u = 0.05; u < 1.0; u += 0.1)
                    Assert.AreEqual(NocMath.Smootherstep(sc - h, sc + h, sc - h + 2 * h * u), NocturneScore.ChordGain(c, sc - h + 2 * h * u), 1e-12, "smootherstep");
            }
            Assert.AreEqual(1.0, NocturneScore.ChordGain(10, NocturneConstants.GoalS), 1e-12, "S8: the open fifth is complete at the stop");
        }

        [Test]
        public void VoicePlan_CommonTonesStayOnTheirVoice()
        {
            var plan = PadEngine.BuildPlan(NocturneScore.Chords, PadEngine.Slots, out int[,] run);
            int nc = NocturneScore.Chords.Length;
            for (int c = 0; c < nc; c++)
            {
                // each row is the chord's notes, once each, silent slots -1
                var notes = new System.Collections.Generic.List<int>();
                for (int i = 0; i < PadEngine.Slots; i++) if (plan[c, i] >= 0) notes.Add(plan[c, i]);
                notes.Sort();
                CollectionAssert.AreEqual(NocturneScore.Chords[c], notes, "plan row C" + c);
                if (c == 0) continue;
                for (int i = 0; i < PadEngine.Slots; i++)
                {
                    int prev = plan[c - 1, i];
                    if (prev >= 0 && Array.IndexOf(NocturneScore.Chords[c], prev) >= 0)
                        Assert.AreEqual(prev, plan[c, i], "common tone " + prev + " stays on slot " + i + " at C" + (c - 1) + "->C" + c);
                }
            }
            Assert.AreEqual(3, NocturneScore.Chords[10].Length, "C10 is the open fifth D5 (3 voices)");
        }

        /// <summary>Drives a PadEngine through s(t) and checks every control block: a bank changes its note only while silent
        /// (gain &lt; 1e-4), a gain moves by at most one 50 ms one-pole step per block, and in the steady part of a chord the bank
        /// gains are equal-power (sum of squares = the chord's voice count).</summary>
        static void PadSweep(Func<double, double> sOfT, double seconds, string label)
        {
            const double cr = Fs / 32.0;
            var pad = new PadEngine(Fs, cr, 11u);
            var note = new int[PadEngine.Banks]; var gain = new double[PadEngine.Banks];
            double maxSlew = 1.0 - Math.Exp(-1.0 / (PadEngine.GainSeconds * cr)) + 1e-12;
            pad.Snap(sOfT(0.0), 0.0);
            for (int b = 0; b < PadEngine.Banks; b++) pad.Bank(b, out note[b], out gain[b]);
            int steady = 0;
            for (long k = 1; k < (long)(seconds * cr); k++)
            {
                double t = k / cr, s = sOfT(t);
                pad.Control(s, t);
                double p = 0.0;
                for (int b = 0; b < PadEngine.Banks; b++)
                {
                    pad.Bank(b, out int nn, out double g);
                    if (nn != note[b]) Assert.Less(gain[b], 1e-4, label + ": bank " + b + " changed note " + note[b] + "->" + nn + " while sounding at s " + s.ToString("0.00"));
                    Assert.LessOrEqual(Math.Abs(g - gain[b]), maxSlew, label + ": gain slew of bank " + b + " at s " + s.ToString("0.00"));
                    note[b] = nn; gain[b] = g; p += g * g;
                }
                // steady: the chord has been complete (w = 1, outside every change window) for the last 1.5 s, with no jump of s
                int c = NocturneScore.ChordIndex(s, out double w);
                bool settled = t > 1.5 && w >= 1.0;
                for (double back = 0.25; settled && back <= 1.5; back += 0.25)
                {
                    double sb = sOfT(t - back);
                    settled = NocturneScore.ChordIndex(sb, out double wb) == c && wb >= 1.0 && Math.Abs(sb - s) < 10.0;
                }
                if (settled)
                {
                    Assert.AreEqual(NocturneScore.Chords[c].Length, p, 0.02, label + ": equal power in the steady part at s " + s.ToString("0.00"));
                    steady++;
                }
            }
            Assert.Greater(steady, 100, label + ": steady blocks checked");
        }

        [Test]
        public void PadEngine_ChangesOnlyInSilenceWithBoundedSlewAndEqualPower()
        {
            PadSweep(t => 4.0 * t, NocturneConstants.GoalS / 4.0, "walk 4 m/s 0-1500");
            PadSweep(t => 266.0 + 1.5 * t, 30.0, "C1->C2 at 1.5 m/s");
            PadSweep(t => 1466.0 + 0.4 * t, 80.0, "C9->C10 at 0.4 m/s");
            // standing inside a change window, walking back across it, and jumps of s (a restart, -progress)
            PadSweep(t => t < 5 ? 286.0 : t < 25 ? 286.0 - 1.5 * (t - 5) : t < 30 ? 1000.0 : 60.0, 40.0, "stand, walk back, jumps");
        }

        [Test]
        public void PadSolo_NoClickAtTheChordChanges()
        {
            var sb = new System.Text.StringBuilder();
            foreach (double v in new[] { 2.5, 1.5 })
                for (int c = 1; c < NocturneScore.ChordChangeS.Length; c++)
                {
                    if (v < 2.0 && c != 1 && c != 8 && c != 10) continue;   // 1.5 m/s: the first, the beat-7 and the last change
                    var cm = AudioGates.PadChange(c, v, out WalkAnalysis an);
                    sb.AppendLine(FormattableString.Invariant($"v {v} C{c - 1}->C{c}: env step {an.ChordMaxEnvStep[c]:0.00000}, sample step {an.ChordMaxStep[c]:0.00000}, d2 in {cm.MaxInside:0.000000} out {cm.MaxOutside:0.000000}"));
                    Assert.Less(an.ChordMaxEnvStep[c], AudioGates.ChordEnvStepLimit, "14.7 H8: 20 ms envelope step < 0.05 at C" + (c - 1) + "->C" + c);
                    Assert.Less(an.ChordMaxStep[c], 0.02, "pad sample step inside the change window");
                    Assert.Less(cm.MaxInside, 3.0 * cm.MaxOutside + 1e-4, "second difference inside the window against the steady pad");
                    Assert.AreEqual(0, an.NonFinite);
                }
            Debug.Log("[NOC] AudioScoreTests pad changes\n" + sb);
        }

        [Test]
        public void FullMix_ChordChangesWithoutEnvelopeSteps()
        {
            foreach (int c in new[] { 5, 10 })   // C4->C5 (trains T4 / T6 pass) and C9->C10 (the open fifth, the goal)
            {
                double h = NocturneScore.ChordHalfWindow[c], sc = NocturneScore.ChordChangeS[c];
                var an = AudioGates.RenderStretch(sc - h - 1.0, (2.0 * h + 2.0) / 2.5, 2.5, 1, 3.0);
                Assert.Less(an.ChordMaxEnvStep[c], AudioGates.ChordEnvStepLimit, "mix: 20 ms envelope step at C" + (c - 1) + "->C" + c);
                Assert.LessOrEqual(an.MaxStep, AudioGates.StepLimit);
                Assert.LessOrEqual(an.TruePeak.PeakDb, AudioGates.TruePeakLimitDb);
            }
        }

        // ------------------------------------------------------------------ silences and swells (14.3 S1-S8)
        static double Db(double g) => 20.0 * Math.Log10(Math.Max(g, 1e-30));

        [Test]
        public void Swells_S1ToS8()
        {
            const int R = NocturneScore.R, H = NocturneScore.H, T = NocturneScore.T, W = NocturneScore.W, P = NocturneScore.P, K = NocturneScore.K, X = NocturneScore.X;
            // S1 s 0-8: only rain and hum (traffic, X and the pad are out; the pad enters by its 6 s attack once s passes 8)
            Assert.AreEqual(0.0, NocturneScore.SwellGain(T, 2.0), 1e-12); Assert.AreEqual(0.0, NocturneScore.SwellGain(X, 2.0), 1e-12);
            Assert.AreEqual(1.0, NocturneScore.SwellGain(R, 2.0), 1e-12); Assert.AreEqual(1.0, NocturneScore.SwellGain(H, 2.0), 1e-12);
            Assert.AreEqual(8.0, NocturneScore.S1End); Assert.AreEqual(6.0, NocturneScore.PadAttackSeconds);
            Assert.AreEqual(0.0, NocturneScore.AttackGain(0.0), 1e-12); Assert.AreEqual(1.0, NocturneScore.AttackGain(6.0), 1e-12);
            // S2 s 104-124: traffic +6 dB
            Assert.AreEqual(6.0, Db(NocturneScore.SwellGain(T, 114.0)), 1e-6);
            // S3 s 400-420: the pad drops 6 dB, the wind rises
            Assert.AreEqual(-6.0, Db(NocturneScore.SwellGain(P, 410.0)), 1e-6); Assert.Greater(Db(NocturneScore.SwellGain(W, 410.0)), 3.0);
            // S4 s 540-554: everything ducks 5 dB except wind and deep hum (+3 dB)
            Assert.AreEqual(-5.0, Db(NocturneScore.SwellGain(R, 547.0)), 1e-6); Assert.AreEqual(-5.0, Db(NocturneScore.SwellGain(P, 547.0)), 1e-6);
            Assert.AreEqual(3.0, Db(NocturneScore.SwellGain(W, 547.0)), 1e-6); Assert.AreEqual(3.0, Db(NocturneScore.SwellGain(H, 547.0)), 1e-6);
            // S6 s 876-896: wind and traffic fall 12 dB, leaf rain rises
            Assert.AreEqual(-12.0, Db(NocturneScore.SwellGain(W, 886.0)), 1e-6); Assert.AreEqual(-12.0, Db(NocturneScore.SwellGain(T, 886.0)), 1e-6);
            Assert.Greater(NocturneScore.LeafRainSwellDb(886.0), 3.0);
            // S7 s 1 340-1 380: the koi swells to -33 dBFS and ducks the pad 3 dB (re the beat-7 table row)
            double padIn = Db(NocturneScore.LayerAmp(P, 1360.0)) - NocturneScore.PadTrimDb(1360.0) - NocturneScore.BeatTrimDb[6];
            Assert.AreEqual(NocturneScore.LayerDb[6, P] - 3.0, padIn, 1e-6, "S7 pad duck");
            Assert.AreEqual(NocturneScore.KoiPeakDb + NocturneScore.BeatTrimDb[6], Db(NocturneScore.KoiAmp(NocturneScore.KoiPeakS())), 0.01, "koi peak -33 dBFS");
            // S8: the open fifth and the release after the goal trigger
            Assert.AreEqual(1497.0, NocturneScore.GoalTriggerS);
            Assert.AreEqual(1.0, NocturneScore.ReleaseGain(0.0), 1e-12); Assert.AreEqual(0.0, NocturneScore.ReleaseGain(8.0), 1e-12);
            // every swell is smooth: over 0.25 m no layer moves by more than 8 % of its largest amplitude on the walk (a 4 m
            // smootherstep ramp of -5 dB moves 5 %; a discontinuity of 1 dB at full level would move 12 %)
            for (int l = 0; l < NocturneScore.LayerCount; l++)
            {
                double max = 0.0;
                for (double s = 0.0; s <= NocturneConstants.GoalS; s += 0.25) max = Math.Max(max, NocturneScore.LayerAmp(l, s));
                for (double s = 0.0; s < NocturneConstants.GoalS; s += 0.25)
                {
                    double a = NocturneScore.LayerAmp(l, s), b = NocturneScore.LayerAmp(l, s + 0.25);
                    Assert.LessOrEqual(Math.Abs(b - a), 0.08 * max, "layer " + NocturneScore.LayerNames[l] + " step at s " + s);
                }
            }
            // the gate excludes exactly S3, S4, S6 and S7
            CollectionAssert.AreEqual(new[] { 400.0, 540.0, 876.0, 1340.0 }, NocturneScore.GateExcludeS0);
            CollectionAssert.AreEqual(new[] { 420.0, 554.0, 896.0, 1380.0 }, NocturneScore.GateExcludeS1);
        }

        [Test]
        public void KoiLaw_PeaksInsideS7AndRisesFaintlyFrom1200()
        {
            double ps = NocturneScore.KoiPeakS();
            Assert.That(ps, Is.InRange(1350.0, 1380.0), "14.3: peak at s 1 350-1 380");
            Assert.AreEqual(0.0, NocturneScore.KoiAmp(1199.0), 1e-12, "nothing before the teaser");
            Assert.Less(Db(NocturneScore.KoiAmp(1220.0)), -55.0, "faint at s 1 220");
            Assert.That(Db(NocturneScore.KoiAmp(1250.0)), Is.InRange(-60.0, -42.0), "the teaser with the cloud glow at s 1 250");
            Assert.Less(NocturneScore.KoiAmp(1250.0), NocturneScore.KoiAmp(1300.0)); Assert.Less(NocturneScore.KoiAmp(1300.0), NocturneScore.KoiAmp(1350.0));
            Assert.AreEqual(41.2, KoiTone.E1); Assert.AreEqual(55.0, KoiTone.A1); Assert.AreEqual(0.07, KoiTone.FmHz); Assert.AreEqual(3.0, KoiTone.FmDepthHz);
            // the law 1 / (1 + (d / 400)^2) of the koi distance
            double d = NocturneScore.KoiDistance(1300.0);
            Assert.AreEqual(1.0 / (1.0 + (d / 400.0) * (d / 400.0)), NocturneScore.KoiLaw(1300.0), 1e-12);
        }

        // ------------------------------------------------------------------ reverb (14.3 places, 14.6 windows)
        [Test]
        public void Reverb_PlacesAndTheirWindows()
        {
            double[] mid = { 50, 250, 480, 720, 980, 1170, 1400 };
            double[] rt = { 0.9, 2.8, 4.5, 3.5, 1.6, 2.2, 3.8 }, pre = { 8, 30, 250, 60, 20, 25, 70 }, damp = { 0.6, 0.45, 0.5, 0.5, 0.7, 0.5, 0.45 };
            for (int i = 0; i < mid.Length; i++)
            {
                NocturneScore.Reverb(mid[i], out double r, out double p, out double dh, out _);
                Assert.AreEqual(rt[i], r, 1e-9, NocturneScore.PlaceNames[i] + " RT60");
                Assert.AreEqual(pre[i], p, 1e-9, NocturneScore.PlaceNames[i] + " pre-delay");
                Assert.AreEqual(NocturneScore.DampingHz(damp[i]), dh, 1e-6, NocturneScore.PlaceNames[i] + " damping");
            }
            // 14.6: 0.9 -> 2.8 s over s 100-130, 2.8 -> 4.5 s over 380-420; the others over 40 m centred on the boundary
            NocturneScore.Reverb(100.0, out double a0, out _, out _, out _); NocturneScore.Reverb(130.0, out double a1, out _, out _, out _);
            Assert.AreEqual(0.9, a0, 1e-9); Assert.AreEqual(2.8, a1, 1e-9);
            NocturneScore.Reverb(380.0, out double b0, out _, out _, out _); NocturneScore.Reverb(420.0, out double b1, out _, out _, out _);
            Assert.AreEqual(2.8, b0, 1e-9); Assert.AreEqual(4.5, b1, 1e-9);
            double[] bound = { 560, 880, 1080, 1260 };
            for (int i = 0; i < bound.Length; i++)
            {
                NocturneScore.Reverb(bound[i] - 20.0, out double u0, out _, out _, out _); NocturneScore.Reverb(bound[i] + 20.0, out double u1, out _, out _, out _);
                Assert.AreEqual(rt[i + 2], u0, 1e-9); Assert.AreEqual(rt[i + 3], u1, 1e-9);
            }
            // smooth (Part 10 item 25 "abrupt reverb changes"): the steepest window is 0.9 -> 2.8 s over 30 m, whose smootherstep
            // slope peaks at 1.875 x 1.9 / 30 = 0.119 s per m; no 0.5 m step above 0.065 s and no 0.05 m step above 0.0065 s
            // (a discontinuity would not shrink with the step)
            for (double s = 0; s < NocturneConstants.GoalS; s += 0.05)
            {
                NocturneScore.Reverb(s, out double x0, out double p0, out _, out _);
                NocturneScore.Reverb(s + 0.5, out double x1, out _, out _, out _);
                NocturneScore.Reverb(s + 0.05, out double x2, out double p2, out _, out _);
                Assert.LessOrEqual(Math.Abs(x1 - x0), 0.065, "reverb RT60 step over 0.5 m at s " + s);
                Assert.LessOrEqual(Math.Abs(x2 - x0), 0.0065, "reverb RT60 step over 0.05 m at s " + s);
                Assert.LessOrEqual(Math.Abs(p2 - p0), 1.875 * 220.0 / 40.0 * 0.05 + 1e-9, "pre-delay step over 0.05 m at s " + s);
            }
        }

        // ------------------------------------------------------------------ rain coupling, vehicles, footsteps
        [Test]
        public void RainCoupling_EqualsThePicture()
        {
            for (double s = 0.0; s <= NocturneConstants.GoalS; s += 1.0)
            {
                Assert.AreEqual(NocSchedule.RainRho(s), NocturneScore.RainRhoSchedule(s), 1e-12, "rain density at s " + s);
                Assert.AreEqual(NocRainMath.Gust(s), NocturneScore.RainGustWeight(s), 1e-12, "beat-4 gust weight at s " + s);
            }
        }

        [Test]
        public void Vehicles_LaunchAtTheirSAndPassesDifferLittleBetweenSpeeds()
        {
            var v = NocturneScore.Vehicles;
            Assert.AreEqual("tram", v[0].Name); Assert.AreEqual(230.0, v[0].SLaunch, "14.5.2 tram launch s 230");
            Assert.AreEqual(7.0, v[0].Speed); Assert.AreEqual(38.0, v[1].Speed);
            // 14.7 H7: passes differ by 14 m (tram) and 7 m (trains) between 1.5 and 2.5 m/s
            double dTram = NocturneScore.MeetS(0, 2.5) - NocturneScore.MeetS(0, 1.5);
            Assert.AreEqual(14.0, Math.Abs(dTram), 0.5, "tram pass offset");
            for (int i = 1; i < v.Length; i++)
            {
                if (v[i].TrackY != NocturneScore.TubeAxisY) continue;   // the bridge tubes (the face tubes pass far below / above)
                double d = Math.Abs(NocturneScore.MeetS(i, 2.5) - NocturneScore.MeetS(i, 1.5));
                Assert.LessOrEqual(d, 7.5, v[i].Name + " pass offset");
            }
            // capture-mode tau and the reset rule
            Assert.AreEqual(0.0, NocturneScore.CaptureTau(0, 200.0)); Assert.AreEqual(4.0, NocturneScore.CaptureTau(0, 240.0), 1e-12);
            var clock = new VehicleClock();
            clock.Update(0, 229.0, Fs, 230.0); Assert.IsFalse(clock.Launched);
            clock.Update(32, 230.01, Fs, 230.0); Assert.IsTrue(clock.Launched);
            clock.Update(64, 230.02, Fs, 230.0); Assert.AreEqual(32.0 / Fs, clock.Tau(64, Fs), 1e-12, "real time after the launch");
            for (int k = 0; k < 400; k++) clock.Update(96 + 32 * k, 230.0 - 0.04 * k, Fs, 230.0);   // walking back 16 m
            Assert.IsFalse(clock.Launched, "reset when s falls 12 m below s_launch");
        }

        [Test]
        public void PassGeometry_DopplerAndPan()
        {
            var tr = NocturneScore.Vehicles[1];   // T1 westbound in the north bridge tube (open between x 384 and 683)
            // listener on the deck at x 400: the nose at x 600 is coming west (approaching, pitch up)
            var fa = new ScoreFrame { X = 400.0, Z = 195.0, Yaw = 90.0, Fs = Fs };
            var a = PassGeometry.Evaluate(in tr, (tr.X0 - 600.0) / tr.Speed, in fa, 50.0, 0.0);
            // listener at x 650 after the pass: the tail at x 500 is going away (receding, pitch down)
            var fr = new ScoreFrame { X = 650.0, Z = 195.0, Yaw = 90.0, Fs = Fs };
            var r = PassGeometry.Evaluate(in tr, (tr.X0 - (500.0 - tr.Length)) / tr.Speed, in fr, 50.0, 0.0);
            Assert.Greater(a.Doppler, 1.05, "approaching: higher pitch"); Assert.Less(r.Doppler, 0.95, "receding: lower pitch");
            Assert.LessOrEqual(a.Doppler, 343.0 / (343.0 - 38.0) + 1e-6, "bounded by c / (c - v)");
            Assert.GreaterOrEqual(r.Doppler, 343.0 / (343.0 + 38.0) - 1e-6, "bounded by c / (c + v)");
            Assert.Greater(a.Gain, 0.0); Assert.Greater(r.Gain, 0.0);
        }

        [Test]
        public void Footsteps_CadenceVariationAndPeak()
        {
            foreach (var (v, rate) in new[] { (1.5, 2.00), (2.5, 2.94) })
            {
                var sch = new StepScheduler();
                double blocks = 60.0 * Fs / 32.0, ds = v * 32.0 / Fs;
                int steps = 0;
                for (int k = 0; k < (int)blocks; k++) if (sch.Advance(ds, v, 32) >= 0) steps++;
                Assert.AreEqual(rate, steps / 60.0, 0.02, "14.3 stride cadence at " + v + " m/s");
            }
            for (long i = 0; i < 400; i++)
            {
                StepScheduler.Variation(i, 77UL, out double g, out double pitch, out double pan, out int variant);
                Assert.That(g, Is.InRange(-0.8, 0.8)); Assert.That(pitch, Is.InRange(0.96, 1.04));
                Assert.AreEqual((i & 1) == 0 ? -StepScheduler.Pan : StepScheduler.Pan, pan, "alternate feet");
                Assert.That(variant, Is.InRange(0, FootstepBank.Variants - 1));
            }
            var bank = new FootstepBank(Fs, 5u);
            for (int s = 0; s < FootstepBank.Surfaces; s++)
                for (int k = 0; k < FootstepBank.Variants; k++)
                {
                    var clip = bank.Get((FootSurface)s, k);
                    double pk = 0; foreach (float x in clip) pk = Math.Max(pk, Math.Abs(x));
                    Assert.AreEqual(FootstepBank.PeakDb, Db(pk), 0.3, "peak -30 dBFS, " + (FootSurface)s + " variant " + k);
                    Assert.AreEqual(0f, clip[clip.Length - 1], 1e-6f, "exact zero end");
                }
        }

        // ------------------------------------------------------------------ the player path (-audio-dump), continuity, invariance
        [Test]
        public void PlayerPath_MatchesTheOfflineRender()
        {
            foreach (double v in new[] { 2.5, 1.5 })
            {
                double s0 = 310.0, secs = 10.0;
                var off = new MeterSink();
                OfflineAudioRenderer.Render(new OfflineRenderSettings { S0 = s0, S1 = s0 + secs * v, Speed = v, Seed = 4, PrerollSeconds = 3.0, Footsteps = false }, off);
                var pl = new MeterSink();
                AudioGates.RenderPlayerPath(Fs, 4, secs, 1024, 60.0, new ConstantWalkScript { S0 = s0, V = v }, 3.0, pl);
                // 14.7 H8: -audio-dump within 1 dB of the offline render; the shared DSP makes it far closer
                Assert.AreEqual(RmsSink.Db(off.Rms), RmsSink.Db(pl.Rms), 0.2, "player path vs offline at " + v + " m/s");
                Assert.LessOrEqual(pl.PeakDbfs, NocturneConstants.LimiterCeilingDb + 1e-6);
            }
        }

        [Test]
        public void PlayerPath_ContinuousAtAnySpeedStandingWalkingBackAndSprinting()
        {
            // across the C1->C2 window: stand, walk 2.5, stop, walk back 1.5, stand, 10 m/s, stop
            var sc = new PiecewiseWalkScript { S0 = 276.0, T = new[] { 0.0, 2.0, 8.0, 10.0, 16.0, 18.0, 21.0 }, V = new[] { 0.0, 2.5, 0.0, -1.5, 0.0, 10.0, 0.0 } };
            var an = new WalkAnalysis();
            AudioGates.RenderPlayerPath(Fs, 2, 24.0, 512, 60.0, sc, 3.0, an);
            Assert.AreEqual(0, an.NonFinite);
            Assert.LessOrEqual(an.MaxStep, AudioGates.StepLimit, "sample step");
            Assert.Less(an.ChordMaxEnvStep[2], AudioGates.ChordEnvStepLimit, "20 ms envelope step in the C1->C2 window");
            Assert.LessOrEqual(an.TruePeak.PeakDb, AudioGates.TruePeakLimitDb);
        }

        [Test]
        public void Mixer_OutputIndependentOfTheCallbackSize()
        {
            int total = 4 * Fs;
            float[] reference = null;
            foreach (var pat in new[] { new[] { 1024 }, new[] { 441 }, new[] { 37, 2048, 512, 1, 333 } })
            {
                var st = new OfflineRenderSettings { S0 = 590.0, Speed = 2.5, Seconds = 4.0, Seed = 3 };
                var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
                var outBuf = new float[total * 2];
                int pos = 0, k = 0;
                while (pos < total)
                {
                    int n = Math.Min(pat[k++ % pat.Length], total - pos);
                    var b = new float[n * 2];
                    mixer.Process(b, 2);
                    Array.Copy(b, 0, outBuf, pos * 2, n * 2);
                    pos += n;
                }
                if (reference == null) { reference = outBuf; continue; }
                double md = 0; for (int i = 0; i < outBuf.Length; i++) md = Math.Max(md, Math.Abs(outBuf[i] - reference[i]));
                Assert.AreEqual(0.0, md, 1e-6, "callback pattern " + string.Join(",", pat));
            }
        }

        [Test]
        public void Mixer_ProcessAllocatesNothing()
        {
            var mixer = new NocMixer(Fs, 1) { Motion = new WalkMotion { S0 = 590.0, V0 = 2.5 }, AutoSteps = true };
            var buf = new float[2048];
            for (int i = 0; i < 200; i++) mixer.Process(buf, 2);
            var m2 = new NocMixer(Fs, 1);
            var p = new NocAudioParams { master = 1f, s = 1300.0, speed = 2.5, rain = 1f };
            for (int i = 0; i < 200; i++) { p.seq++; p.s += 0.05; if (i % 30 == 0) p.footsteps++; m2.Process(buf, 2, in p); }
            // is the per-thread counter live in this runtime?
            long c0 = GC.GetAllocatedBytesForCurrentThread();
            var probe = new byte[4096];
            long c1 = GC.GetAllocatedBytesForCurrentThread();
            GC.KeepAlive(probe);
            if (c1 - c0 < 4096) Assert.Pass("GC.GetAllocatedBytesForCurrentThread is not live here; Tools/audio/harness alloc measures 0 bytes on .NET");
            long a0 = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) mixer.Process(buf, 2);
            for (int i = 0; i < 1000; i++) { p.seq++; p.s += 0.05; if (i % 30 == 0) p.footsteps++; m2.Process(buf, 2, in p); }
            long a1 = GC.GetAllocatedBytesForCurrentThread();
            Assert.AreEqual(0, a1 - a0, "NocMixer.Process allocates nothing (offline and player paths)");
        }
    }
}
