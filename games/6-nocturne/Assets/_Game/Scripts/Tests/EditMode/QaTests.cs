// QaTests: EditMode tests of the pure helpers of the runtime QA harness (MASTER_PROMPT 11.2, 11.8,
// 14.10): the rectilinear projection used for the framing acceptances, the key-shot subjects against
// the positions the spec states, the capture plan (every key shot at both resolutions, wide shots with
// their in-game FOV version, framing variants, tag format beatNN_<name>_<tag>, sequence windows),
// the image metrics, the perf summary, the soak growth rule and the JSON writer.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class QaTests
    {
        static QaView View169(double yaw, double pitch, double fov) => new QaView(0, 1.7, 0, yaw, pitch, fov, 1920, 1080);

        static KeyShot Key(string tag)
        {
            int i = ShotPlan.FindKeyShot(tag);
            Assert.GreaterOrEqual(i, 0, "key shot " + tag + " missing from NocturneConstants.KeyShots");
            return NocturneConstants.KeyShots[i];
        }

        static QaView KeyView(KeyShot k, int w, int h) => new QaView(k.x, k.y, k.z, k.yaw, k.pitch, k.fov, w, h);

        static QaSubject Find(List<QaSubject> list, string name)
        {
            for (int i = 0; i < list.Count; i++) if (list[i].name == name) return list[i];
            Assert.Fail("subject " + name + " missing");
            return null;
        }

        // ------------------------------------------------------------------ projection

        [Test]
        public void Projection_ForwardMapsToCentre()
        {
            var v = View169(37.0, -12.0, 64.0);
            Assert.IsTrue(QaProjection.ProjectDir(v, QaProjection.AzElDir(37.0, -12.0), out double x, out double y));
            Assert.AreEqual(0.0, x, 1e-9);
            Assert.AreEqual(0.0, y, 1e-9);
        }

        [Test]
        public void Projection_VerticalFovEdgeIsNdcOne_HorizontalFollowsAspect()
        {
            var v = View169(0, 0, 62.0);
            Assert.IsTrue(QaProjection.ProjectDir(v, QaProjection.AzElDir(0, 31.0), out _, out double y));
            Assert.AreEqual(1.0, y, 1e-9);
            double tanH = Math.Tan(31.0 * NocMath.Deg) * 1920.0 / 1080.0;   // Hor+: vertical FOV fixed
            double az = Math.Atan(tanH) / NocMath.Deg;
            Assert.IsTrue(QaProjection.ProjectDir(v, QaProjection.AzElDir(az, 0), out double x, out _));
            Assert.AreEqual(1.0, x, 1e-9);
        }

        [Test]
        public void Projection_BehindCameraIsRejected()
        {
            var v = View169(0, 0, 64.0);
            Assert.IsFalse(QaProjection.ProjectDir(v, new D3(0, 0, -1), out _, out _));
        }

        [Test]
        public void Projection_BasisMatchesUnityEulerConvention()
        {
            double[] yaws = { 0, 32, -32.7, 145, 327.3 };
            double[] pitches = { 0, 12, -55, 65, 89 };
            for (int i = 0; i < yaws.Length; i++)
            {
                QaProjection.Basis(yaws[i], pitches[i], out D3 f, out D3 r, out D3 u);
                Quaternion q = Quaternion.Euler(-(float)pitches[i], (float)yaws[i], 0f);
                Vector3 uf = q * Vector3.forward, ur = q * Vector3.right, uu = q * Vector3.up;
                Assert.AreEqual(uf.x, f.x, 1e-5); Assert.AreEqual(uf.y, f.y, 1e-5); Assert.AreEqual(uf.z, f.z, 1e-5);
                Assert.AreEqual(ur.x, r.x, 1e-5); Assert.AreEqual(ur.y, r.y, 1e-5); Assert.AreEqual(ur.z, r.z, 1e-5);
                Assert.AreEqual(uu.x, u.x, 1e-5); Assert.AreEqual(uu.y, u.y, 1e-5); Assert.AreEqual(uu.z, u.z, 1e-5);
            }
        }

        [Test]
        public void Projection_PanStepMovesTheCentreByThePixelsAsked()
        {
            double step = QaProjection.PanStepDeg(0.1, 64.0, 1080);
            var v = View169(0, 0, 64.0);
            Assert.IsTrue(QaProjection.ProjectDir(v, QaProjection.AzElDir(0, step), out _, out double y));
            Assert.AreEqual(0.1, y * 540.0, 1e-6);
        }

        // ------------------------------------------------------------------ key-shot subjects vs the spec (14.10 notes)

        // ------------------------------------------------------------------ capture plan: key shots

        static List<StillShot> KeyPlan() => ShotPlan.Build("keyshots").stills;

        static StillShot FindStill(List<StillShot> list, string tag)
        {
            for (int i = 0; i < list.Count; i++) if (list[i].tag == tag) return list[i];
            return null;
        }

        // ------------------------------------------------------------------ capture plan: beats and tags

        [Test]
        public void Plan_TagsFilesAndSequenceNamesAreUnique()
        {
            var plan = ShotPlan.Build("everything", new CapturePlanOptions { encode4k = true });
            var seen = new HashSet<string>();
            foreach (StillShot st in plan.stills) Assert.IsTrue(seen.Add(st.File), "duplicate " + st.File);
            var seq = new HashSet<string>();
            foreach (SequenceSpec sq in plan.sequences) Assert.IsTrue(seq.Add(sq.name), "duplicate " + sq.name);
            Assert.IsNotNull(plan.parity);
        }

        // ------------------------------------------------------------------ capture plan: sequences

        static SequenceSpec FindSeq(List<SequenceSpec> list, string name)
        {
            for (int i = 0; i < list.Count; i++) if (list[i].name == name) return list[i];
            return null;
        }

        [Test]
        public void Sequences_WalkPoseFollowsThePathCentreLookingForward()
        {
            SequenceSpec sq = FindSeq(ShotPlan.Build("transitions").sequences, "seq_b56");
            for (int n = 0; n < sq.frames; n += 97)
            {
                sq.FramePose(n, out double s, out double x, out double y, out double z, out double yaw, out double pitch, out double fov);
                Assert.AreEqual(sq.s0 + NocturneConstants.WalkSpeed * n / 30.0, s, 1e-9);
                D3 p = NocMath.PathPoint(s);
                Assert.AreEqual(p.x, x, 1e-9); Assert.AreEqual(p.y + NocturneConstants.EyeHeight, y, 1e-9); Assert.AreEqual(p.z, z, 1e-9);
                Assert.AreEqual(NocMath.Heading(s) / NocMath.Deg, yaw, 1e-9);
                Assert.AreEqual(NocturneConstants.GazeDefaultPitch, pitch, 1e-9);
                Assert.AreEqual(NocMath.Fov(s), fov, 1e-9);
            }
        }

        // ------------------------------------------------------------------ metrics, perf, soak, json

        [Test]
        public void CaptureMetrics_LumaClipMagentaDiffAndPacking()
        {
            // 2 x 2 RGBA: magenta, white / black, grey (row 0 first)
            byte[] rgba = { 255, 0, 255, 255, 255, 255, 255, 255, 0, 0, 0, 255, 128, 128, 128, 255 };
            byte[] prev = new byte[16];
            byte[] rgb = new byte[12];
            var r = new CaptureResult();
            CaptureMetrics.Compute(rgba, prev, 2, 2, rgb, null, true, new int[256], r);
            double expectLuma = (CaptureMetrics.WR * (255 + 255 + 0 + 128) + CaptureMetrics.WG * (0 + 255 + 0 + 128) + CaptureMetrics.WB * (255 + 255 + 0 + 128)) / (255.0 * 4);
            Assert.AreEqual(expectLuma, r.meanLuma, 1e-12);
            Assert.AreEqual(1, r.magentaPixels);
            Assert.AreEqual(0.5, r.clippedFraction, 1e-12);
            Assert.AreEqual((510 + 765 + 384) / 12.0, r.meanAbsDiff, 1e-12);
            // flipped: source row 0 lands in destination row 1
            Assert.AreEqual(new byte[] { 0, 0, 0, 128, 128, 128, 255, 0, 255, 255, 255, 255 }, rgb);
            Assert.IsFalse(r.Black);
        }

        [Test]
        public void PerfSummary_AveragesLowsAndBudget()
        {
            var ms = new float[1000];
            for (int i = 0; i < ms.Length; i++) ms[i] = 10f;
            for (int i = 0; i < 10; i++) ms[i * 100] = 40f;   // ten slow frames = the slowest 1 %
            PerfSummary s = PerfSummary.From(ms, ms.Length, 33.0);
            Assert.AreEqual(1000, s.frames);
            Assert.AreEqual(10, s.framesOverBudget);
            Assert.AreEqual(40.0, s.maxMs, 1e-6);
            Assert.AreEqual(1000.0 * 1000 / (990 * 10 + 10 * 40), s.avgFps, 1e-6);
            Assert.AreEqual(25.0, s.low1Fps, 1e-6, "mean of the slowest 1 % = 40 ms");
            Assert.AreEqual(25.0, s.low01Fps, 1e-6);
        }

        [Test]
        public void Soak_GrowthUsesWarmupMinimumAndTailMinimum()
        {
            double[] t = { 0, 10, 20, 30, 40, 50, 60, 70 };
            long[] v = { 900, 100, 120, 110, 300, 250, 260, 255 };
            // warm-up 10 s: baseline = min over t in [10, 40] = 100; tail = min of the last three = 250
            Assert.AreEqual(150.0, SoakRunner.Growth(v, t, v.Length, 10.0), 1e-9);
        }

        [Test]
        public void QaMode_PrecedenceAndSpeed()
        {
            try
            {
                CliArgs.Override(new[] { "Nocturne.exe", "-autotest", "-wallbot" });
                Assert.AreEqual(QaRunMode.WallBot, QaMode.Resolve());
                CliArgs.Override(new[] { "Nocturne.exe", "-autotest", "-capture", "x" });
                Assert.AreEqual(QaRunMode.Capture, QaMode.Resolve());
                CliArgs.Override(new[] { "Nocturne.exe", "-soak", "120", "-autotest" });
                Assert.AreEqual(QaRunMode.Soak, QaMode.Resolve());
                CliArgs.Override(new[] { "Nocturne.exe", "-autotest", "-brisk" });
                Assert.AreEqual(QaRunMode.AutoTest, QaMode.Resolve());
                Assert.AreEqual(NocturneConstants.BriskSpeed, QaMode.DriveSpeed, 1e-6);
                Assert.AreEqual("brisk", QaMode.SpeedName(QaMode.DriveSpeed));
                CliArgs.Override(new[] { "Nocturne.exe" });
                Assert.AreEqual(QaRunMode.None, QaMode.Resolve());
                Assert.AreEqual("walk", QaMode.SpeedName(QaMode.DriveSpeed));
            }
            finally { CliArgs.Override(null); }
        }

        // ============================================================================ Nocturne plan (14.10)
        [Test]
        public void KeyShots_AllTwentyAtFourResolutions()
        {
            var plan = ShotPlan.Build("keyshots");
            Assert.AreEqual(20, NocturneConstants.KeyShots.Length, "14.10: 20 key shots");
            Assert.AreEqual(20 * 4, plan.stills.Count);
            foreach (var v in new[] { "1080", "4k", "1610", "219" })
                Assert.AreEqual(20, plan.stills.FindAll(st => st.variant == v).Count, v);
            foreach (var st in plan.stills)
            {
                Assert.AreEqual(st.s, NocturneConstants.KeyShots[st.keyShotIndex].s, 0.0);
                Assert.IsTrue(st.tag.StartsWith("KS"), st.tag);
            }
            var fast = ShotPlan.Build("keyshots1080");
            Assert.AreEqual(20, fast.stills.Count);
        }

        [Test]
        public void KeyShots_EyesOnTheCentreLineAtTheirS()
        {
            foreach (var k in NocturneConstants.KeyShots)
            {
                D3 p = NocMath.PathPoint(Math.Min(k.s, NocLanes.Path.Length));
                Assert.AreEqual(p.x, k.x, 0.02, k.tag + " x");
                Assert.AreEqual(p.z, k.z, 0.02, k.tag + " z");
                Assert.AreEqual(1.7, k.y, 1e-9, k.tag + " eye height");
                Assert.AreEqual(NocSchedule.Fov(k.s), k.fov, 0.06, k.tag + " vFOV is the schedule value");
            }
        }

        [Test]
        public void KeyShots_PoseIsTheGazeScheduleAtS()
        {
            foreach (var k in NocturneConstants.KeyShots)
            {
                GazeSchedule.Evaluate(k.s, out double yaw, out double pitch);
                Assert.AreEqual(0.0, NocMath.DeltaAngle(k.yaw, yaw), 0.6, k.tag + " yaw (14.10: the pose is the cinematic gaze at s)");
                Assert.AreEqual(k.pitch, pitch, 0.6, k.tag + " pitch");
            }
        }

        [Test]
        public void BeatStills_StartMidEndAndFreeLookOf14_10()
        {
            var st = ShotPlan.BeatStills();
            // 3 path stills + 3 free-look yaws + pitch +60 per beat, plus pitch -60 in beats 3 and 4, BV's two DREAD 6 open states
            // in beat 2 and FX's four D-034 camera-layer poses
            Assert.AreEqual(7 * 7 + 2 + 2 + ShotPlan.CameraLayerPoses.GetLength(0), st.Count);
            Assert.IsTrue(st.Exists(x => x.tag == "beat03_edge_look_down60"));
            Assert.IsTrue(st.Exists(x => x.tag == "beat04_skybridge_look_down60"));
            Assert.IsFalse(st.Exists(x => x.tag == "beat01_alley_look_down60"));
            StillShot dwell = st.Find(x => x.tag == "beat02_boulevard_dread_dwell"), lobby = st.Find(x => x.tag == "beat02_boulevard_dread_lobby");
            Assert.IsNotNull(dwell); Assert.IsNotNull(lobby);
            Assert.AreEqual(212.0, dwell.s, 0.0); Assert.AreEqual(QaDriveMath.HeadingDeg(212.0) - 10.0, dwell.yaw, 1e-9); Assert.AreEqual(2.0, dwell.pitch, 0.0);
            Assert.AreEqual(362.0, lobby.s, 0.0); Assert.AreEqual(QaDriveMath.HeadingDeg(362.0) - 36.5, lobby.yaw, 1e-9);
            // FX (C25): look-ups at the layer's vFOV; the vertigo eye leans 0.35 m along the horizontal gaze
            StillShot up05 = st.Find(x => x.tag.EndsWith("_cam_lookup_ks05", StringComparison.Ordinal));
            Assert.IsNotNull(up05); Assert.AreEqual(63.0, up05.pitch, 0.0); Assert.AreEqual(66.0, up05.fov, 0.0); Assert.IsFalse(up05.gameFov);
            Assert.IsTrue(st.Exists(x => x.tag == "beat03_edge_cam_lookup_ks09"));
            Assert.IsTrue(st.Exists(x => x.tag.EndsWith("_cam_lookup_ks17", StringComparison.Ordinal)));
            StillShot vert = st.Find(x => x.tag == "beat03_edge_cam_vertigo_ks10");
            Assert.IsNotNull(vert); Assert.AreEqual(74.0, vert.fov, 0.0); Assert.AreEqual(-32.0, vert.pitch, 0.0);
            D3 e = NocMath.CorridorToWorld(547.0, 0, NocturneConstants.EyeHeight);
            Assert.AreEqual(0.35, Math.Sqrt((vert.x - e.x) * (vert.x - e.x) + (vert.z - e.z) * (vert.z - e.z)), 1e-9, "0.35 m lean");
            Assert.AreEqual(0.35, (vert.x - e.x) * Math.Sin(vert.yaw * NocMath.Deg) + (vert.z - e.z) * Math.Cos(vert.yaw * NocMath.Deg), 1e-9, "along the gaze");
        }

        [Test]
        public void Sequences_WindowsOf14_10AndTheEnding()
        {
            var seqs = ShotPlan.TransitionSequences(1920, 1080);
            Assert.AreEqual(ShotPlan.TransitionWindows.GetLength(0) + 1, seqs.Count, "the 14.10 windows + AL's C14 transom clip");
            foreach (var q in seqs) if (q.name != "seq_al_transom") Assert.GreaterOrEqual(q.Seconds, ShotPlan.MinWindowSeconds - 0.1, q.name);
            SequenceSpec tr = FindSeq(seqs, "seq_al_transom");
            Assert.IsNotNull(tr); Assert.AreEqual(ShotPlan.MotionFps, tr.fps); Assert.AreEqual(100.0, tr.s0, 0.0); Assert.AreEqual(110.0, tr.s1, 0.0);
            Assert.AreEqual(401, tr.frames, "(110 - 100) m / 1.5 m/s x 60 fps + 1");
            var end = ShotPlan.EndSequence(1920, 1080);
            Assert.AreEqual(NocturneConstants.GoalTriggerS - ShotPlan.EndLeadIn, end.s0, 1e-9);
            Assert.AreEqual(NocturneConstants.EndTotal + ShotPlan.EndTail, ShotPlan.EndSecondsAfterTrigger, 1e-9);
        }

        [Test]
        public void Pose_ParserOfAbyssKeyShots()
        {
            var plan = ShotPlan.Build("pose:test:100:1.7:0:45:10:60");
            Assert.AreEqual(1, plan.stills.Count);
            var p = plan.stills[0];
            Assert.AreEqual("pose_test", p.tag);
            Assert.AreEqual(100.0, p.x, 0); Assert.AreEqual(45.0, p.yaw, 0); Assert.AreEqual(60.0, p.fov, 0);
            Assert.AreEqual(0, ShotPlan.Build("pose:bad:1:2").stills.Count);
            Assert.AreEqual(1, ShotPlan.Build("pose:bad:1:2").unknownSets.Count);
            // FA's request: an optional ninth field s replaces the path projection (parameters, fog, vFOV <= 0, ambient)
            var q = ShotPlan.Build("pose:fs1000:508:120:4585:0:0:0:300").stills;
            Assert.AreEqual(1, q.Count);
            Assert.AreEqual(300.0, q[0].s, 0.0);
            Assert.AreEqual(NocMath.Fov(300.0), q[0].fov, 1e-12, "fov 0: the schedule at the given s");
            Assert.AreEqual(ShotPlan.AmbientForS(300.0), q[0].ambient, 1e-12);
            Assert.AreEqual(0, ShotPlan.Build("pose:x:1:2:3:0:0:60:-5").stills.Count, "s outside the path");
            Assert.AreEqual(0, ShotPlan.Build("pose:x:1:2:3:0:0:60:abc").stills.Count);
        }

        [Test]
        public void KeyFrameId_IsTheKsNumber()
        {
            Assert.AreEqual("KS19", QaFraming.KeyFrameId("KS19_goal_window_4k"));
            Assert.AreEqual("KS00", QaFraming.KeyFrameId("KS00_alley_start"));
            Assert.AreEqual("", QaFraming.KeyFrameId("beat01_alley_mid"));
        }

        [Test]
        public void AutoTest_ExpectedTimesOf14_10()
        {
            Assert.AreEqual(998.1, AutoTestRunner.ExpectedTrigger(1.5, false), 1e-9);
            Assert.AreEqual(599.05, AutoTestRunner.ExpectedTrigger(2.5, true), 1e-9);
            Assert.AreEqual(4.0, AutoTestRunner.StopRampSeconds(1.5), 1e-12);
            Assert.AreEqual(2.4, AutoTestRunner.StopRampSeconds(2.5), 1e-12);
            Assert.AreEqual(1002.1, AutoTestRunner.ExpectedTrigger(1.5, false) + AutoTestRunner.StopRampSeconds(1.5), 1e-9);
            Assert.AreEqual(601.45, AutoTestRunner.ExpectedTrigger(2.5, true) + AutoTestRunner.StopRampSeconds(2.5), 1e-9);
        }

        [Test]
        public void AutoTest_RunnerClockCrossingsAndRoll()
        {
            Assert.AreEqual(0.0, AutoTestRunner.ExpectedCrossing(0, 2.5, true), 0.0);
            Assert.AreEqual(48.25, AutoTestRunner.ExpectedCrossing(120, 2.5, true), 1e-9, "cinematic beat 2 (14.1: 48 s + the 0.25 s ramp loss)");
            Assert.AreEqual(504.25, AutoTestRunner.ExpectedCrossing(1260, 2.5, true), 1e-9);
            Assert.AreEqual(80.1, AutoTestRunner.ExpectedCrossing(120, 1.5, false), 1e-9);
            Assert.AreEqual(AutoTestRunner.ExpectedTrigger(1.5, false), AutoTestRunner.ExpectedCrossing(NocturneConstants.GoalTriggerS, 1.5, false), 1e-9);
            foreach (float yaw in new[] { 0f, 37f, 145f, -90f })
                foreach (float pitch in new[] { 0f, 30f, -60f, 85f })
                    Assert.AreEqual(0.0, AutoTestRunner.RollDeg(Quaternion.Euler(-pitch, yaw, 0f) * Vector3.right), 1e-4, "no roll at yaw " + yaw + " pitch " + pitch);
            Assert.AreEqual(0.5, AutoTestRunner.RollDeg(Quaternion.Euler(0f, 20f, 0.5f) * Vector3.right), 1e-4);
            Assert.AreEqual(0.01, AutoTestRunner.RollMaxManual, 0.0);
            Assert.AreEqual(0.3, AutoTestRunner.RollMaxCinematic, 0.0);
        }

        [Test]
        public void Soak_LoopsTwoMetresBeforeTheGoalTrigger()
        {
            Assert.AreEqual(1495.0, SoakRunner.LoopEndS, 1e-12, "goal trigger 1497 - 2 m (audit S2 item 4)");
        }

        [Test]
        public void SequenceNamesOf14_12MapOntoShotPlanSets()
        {
            CollectionAssert.AreEqual(new[] { "boundaries", "statics", "signs", "rain", "koi", "traffic", "encode" }, ShotPlan.SequenceSets);
            foreach (string n in ShotPlan.SequenceSets)
            {
                var plan = ShotPlan.Build(n);
                Assert.IsEmpty(plan.unknownSets, n);
                Assert.IsNotEmpty(plan.sequences, n);
                Assert.IsEmpty(plan.stills, n);
            }
            var statics = ShotPlan.Build("statics").sequences;
            Assert.AreEqual(7, statics.Count);
            foreach (var q in statics) { Assert.AreEqual(120, q.frames, q.name); Assert.AreEqual("static", q.kind); }
            var signs = ShotPlan.Build("signs").sequences;
            Assert.AreEqual(1, signs.Count); Assert.AreEqual(1800, signs[0].frames); Assert.AreEqual(30, signs[0].fps);
            Assert.AreEqual(1150.0, signs[0].s0, 0.0, "KS16 pose");
            var rainSet = ShotPlan.Build("rain").sequences;
            var rain = rainSet[0];
            Assert.AreEqual(120, rain.frames); Assert.AreEqual(60, rain.fps); Assert.AreEqual(SeqMode.Static, rain.mode);
            // AL (C38): the alley steam clip rides with the rain set (clock running), kind "steam" (the static gate needs a frozen clock)
            Assert.AreEqual(2, rainSet.Count);
            SequenceSpec steam = FindSeq(rainSet, "seq_al_steam_s20");
            Assert.IsNotNull(steam); Assert.AreEqual(120, steam.frames); Assert.AreEqual(60, steam.fps); Assert.IsTrue(steam.advanceAmbient);
            Assert.AreEqual("steam", steam.kind); Assert.AreEqual(20.0, steam.s0, 0.0); Assert.AreEqual(ShotPlan.AmbientForS(20.0), steam.ambient0, 1e-9);
            foreach (var q in ShotPlan.Build("koi").sequences) { Assert.AreEqual(60, q.fps); Assert.AreEqual(720, q.frames); Assert.AreEqual(2.5, q.speed, 0.0); Assert.IsTrue(q.yawWorld); }
            foreach (var q in ShotPlan.Build("traffic").sequences) { Assert.AreEqual(480, q.frames, q.name); }
            var b12 = FindSeq(ShotPlan.Build("boundaries").sequences, "seq_b12");
            Assert.IsNotNull(b12);
            CollectionAssert.AreEqual(new[] { 120.0 }, b12.boundaryS);
            Assert.AreEqual(10, ShotPlan.Build("boundaries").sequences.Count, "9 windows + seq_al_transom (C14)");
            foreach (var q in ShotPlan.Build("encode").sequences) Assert.AreEqual(q.name == "enc_trans_mouth" ? 1800 : 600, q.frames, q.name);
            var goal = FindSeq(ShotPlan.Build("encode").sequences, "enc_beat07_goal");
            Assert.IsNotNull(goal, "LOOK_V2 7: the KS19 wall (PA)");
            Assert.AreEqual(SeqMode.Static, goal.mode, "static at KS19: a fixed rect stays on the wall");
            Assert.AreEqual(1500.0, goal.s0, 0.0); Assert.AreEqual(60, goal.fps); Assert.IsTrue(goal.advanceAmbient);
            Assert.IsNotNull(FindSeq(ShotPlan.Build("encode", new CapturePlanOptions { encode4k = true }).sequences, "enc_beat07_goal_4k"));
            Assert.IsNotNull(FindSeq(ShotPlan.Build("encode").sequences, "enc_beat01_wall"), "LOOK_V2 7: the KS00 walls (PA)");
            Assert.IsNotNull(FindSeq(ShotPlan.Build("encode").sequences, "enc_beat02_axis"), "14.7 H1 (6): the sky of KS04");
        }

        [Test]
        public void CliSequenceFlagSelectsTheCaptureSet()
        {
            try
            {
                CliArgs.Override(new[] { "Nocturne.exe", "-sequence", "Statics", "-freeze-clock", "-rain", "0" });
                Assert.AreEqual("statics", CliArgs.Sequence);
                Assert.AreEqual("statics", CliArgs.CaptureSet);
                Assert.IsTrue(CliArgs.Capturing);
                Assert.AreEqual(QaRunMode.Capture, QaMode.Resolve());
                Assert.AreEqual(0.0, CliArgs.Rain, 0.0);
                Assert.IsTrue(CliArgs.FreezeClock);
                CliArgs.Override(new[] { "Nocturne.exe", "-render-audio", "a.wav", "-autotest" });
                Assert.AreEqual(QaRunMode.RenderAudio, QaMode.Resolve());
                CliArgs.Override(new[] { "Nocturne.exe", "-tier", "2", "-dither", "temporal", "-progress", "0.5", "-clock", "12.5" });
                Assert.AreEqual(2, CliArgs.Tier);
                Assert.AreEqual(DitherMode.Temporal, CliArgs.Dither);
                Assert.AreEqual(750.0, CliArgs.StartS, 1e-9, "-progress is the fraction s / 1500");
                Assert.AreEqual(12.5, CliArgs.ClockStart, 0.0);
                CliArgs.Override(new[] { "Nocturne.exe", "-dither", "off", "-freeze-rain", "-audio-dump", "d.wav" });
                Assert.AreEqual(DitherMode.Off, CliArgs.Dither);
                Assert.IsTrue(CliArgs.FreezeRain);
                Assert.AreEqual("d.wav", CliArgs.AudioDump);
            }
            finally { CliArgs.Override(null); }
        }

        /// <summary>Part 2.5 / audit S2 item 3: Tools/run-player.ps1 routes exactly ShotPlan.HeavySets through gpu-guard and asks
        /// 6500 MiB for ShotPlan.FourKSets.</summary>
        [Test]
        public void RunPlayer_RoutingListsMatchShotPlan()
        {
            string ps = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, "Tools", "run-player.ps1");
            string text = System.IO.File.ReadAllText(ps);
            CollectionAssert.AreEquivalent(ShotPlan.HeavySets, PsArray(text, "$heavySets"));
            CollectionAssert.AreEquivalent(ShotPlan.FourKSets, PsArray(text, "$fourKSets"));
            foreach (string n in ShotPlan.SequenceSets) Assert.IsTrue(ShotPlan.IsHeavy(n), n);
            Assert.IsFalse(ShotPlan.IsHeavy("keyshots1080"));
            Assert.IsFalse(ShotPlan.IsHeavy("beats"));
            Assert.IsTrue(ShotPlan.Uses4K("keyshots", false));
            Assert.IsTrue(ShotPlan.Uses4K("encode", true));
            Assert.IsFalse(ShotPlan.Uses4K("encode", false));
            StringAssert.Contains("6500", text);
        }

        static List<string> PsArray(string text, string name)
        {
            var m = Regex.Match(text, Regex.Escape(name) + @"\s*=\s*@\(([^)]*)\)");
            Assert.IsTrue(m.Success, name + " not found in run-player.ps1");
            var list = new List<string>();
            foreach (Match q in Regex.Matches(m.Groups[1].Value, "\"([^\"]*)\"")) list.Add(q.Groups[1].Value);
            return list;
        }

        [Test]
        public void Visibility_ProjectsAndClipsBoxes()
        {
            var eye = new Vector3(0f, 1.7f, 0f);
            var rot = Quaternion.identity;
            var r = NocVisibility.Check("box", new Bounds(new Vector3(0f, 1.7f, 20f), new Vector3(2f, 2f, 2f)), 0.01f, eye, rot, 60f, 16f / 9f, 0.05f);
            Assert.IsFalse(r.Behind); Assert.AreEqual(0.5f, r.Cx, 1e-4f); Assert.AreEqual(0.5f, r.Cy, 1e-4f); Assert.IsTrue(r.Pass); Assert.IsTrue(r.MarginOk);
            var back = NocVisibility.Check("behind", new Bounds(new Vector3(0f, 1.7f, -20f), Vector3.one), 0f, eye, rot, 60f, 16f / 9f, 0.05f);
            Assert.IsTrue(back.Behind);
            var cut = NocVisibility.Check("cut", new Bounds(new Vector3(0f, 1.7f, 0f), new Vector3(4f, 4f, 4f)), 0f, eye, rot, 60f, 16f / 9f, 0.05f);
            Assert.IsTrue(cut.Oversize, "near plane cuts the box");
            Assert.IsTrue(NocVisibility.RayBox(eye, Vector3.forward, new Vector3(-1, 0, 10), new Vector3(1, 3, 12), out float tin, out float tout));
            Assert.AreEqual(10f, tin, 1e-4f); Assert.AreEqual(12f, tout, 1e-4f);
        }

        [Test]
        public void Json_EscapesStringsAndWritesInvariantNumbers()
        {
            var j = new QaJson();
            j.BeginObject().Prop("a", 1.5).Prop("s", "q\"\\\n").Prop("nan", double.NaN).Prop("b", true);
            j.Key("arr").BeginArray(true).Value(1).Value(2.25).EndArray();
            j.EndObject();
            string s = j.ToString();
            StringAssert.Contains("\"a\": 1.5", s);
            StringAssert.Contains("\"s\": \"q\\\"\\\\\\n\"", s);
            StringAssert.Contains("\"nan\": null", s);
            StringAssert.Contains("\"arr\": [1, 2.25]", s);
            Assert.AreEqual('}', s[s.Length - 1]);
        }

        // ------------------------------------------------------------------ Look v2 texture sets (TEXTURE_PLAN 7.5, 7.7; TQ)

        /// <summary>texpans: 7 key-shot pans of 0.5 px per frame of the OUTPUT height (so -capture-ss 2 pans the same picture
        /// distance and -seq-res 3840x2160 halves the angle), 30 frames centred on the key-shot yaw, ambient held.</summary>
        [Test]
        public void TexPans_HalfPixelPansAtTheSevenKeyShots()
        {
            var q = ShotPlan.Build("texpans").sequences;
            Assert.AreEqual(7, q.Count);
            Assert.IsEmpty(ShotPlan.Build("texpans").stills);
            string[] ks = { "KS00", "KS04", "KS05", "KS08", "KS09", "KS15", "KS17" };
            for (int i = 0; i < ks.Length; i++)
            {
                SequenceSpec sq = q[i];
                KeyShot k = Key(ShotPlan.TexPanShots[i]);
                Assert.AreEqual("seq_texpan_" + ks[i].ToLowerInvariant(), sq.name);
                Assert.AreEqual("texpan", sq.kind); Assert.AreEqual("texpans", sq.set); Assert.AreEqual(SeqMode.PanYaw, sq.mode);
                Assert.AreEqual(30, sq.frames); Assert.AreEqual(30, sq.fps); Assert.IsFalse(sq.advanceAmbient);
                sq.FramePose(15, out double s, out double x, out double y, out double z, out double yaw, out double pitch, out double fov);
                Assert.AreEqual(k.s, s, 0.0); Assert.AreEqual(k.x, x, 0.0); Assert.AreEqual(k.z, z, 0.0);
                Assert.AreEqual(k.yaw, yaw, 1e-12, "frame 15 is the key-shot yaw"); Assert.AreEqual(k.pitch, pitch, 0.0); Assert.AreEqual(k.fov, fov, 0.0);
                // the step moves the image centre by 0.5 px of the 1080p output
                var v = new QaView(0, 0, 0, 0, 0, k.fov, 1920, 1080);
                Assert.IsTrue(QaProjection.ProjectDir(v, QaProjection.AzElDir(sq.panStepDeg, 0), out double nx, out _));
                Assert.AreEqual(0.5, nx * 960.0, 1e-3, sq.name);
            }
            var q4 = ShotPlan.Build("texpans", new CapturePlanOptions { seqWidth = 3840, seqHeight = 2160 }).sequences;
            Assert.AreEqual(0.5 * q[0].panStepDeg, q4[0].panStepDeg, 1e-6, "0.5 px at 2160 rows");
        }

        /// <summary>dolly: the 30 deg line to face B's planar part from 800 m to 25 m (geometric steps, every pose in the open rift,
        /// looking at the target) and the grazing pass over the bridge deck (yaw 160 = 70 deg off face B's normal, beat 4).</summary>
        [Test]
        public void Dolly_ThirtyDegreeLineAndGrazingPass()
        {
            var q = ShotPlan.Build("dolly").sequences;
            Assert.AreEqual(2, q.Count);
            SequenceSpec a = FindSeq(q, "seq_dolly_face30"), b = FindSeq(q, "seq_dolly_graze");
            Assert.IsNotNull(a); Assert.IsNotNull(b);
            Assert.AreEqual(SeqMode.Poses, a.mode); Assert.AreEqual("dolly", a.kind); Assert.AreEqual(300, a.frames);
            // the target is on face B's planar part: x_B = 815.3 and dx/dz = 0 at z -100 (|z - 195| < 300)
            Assert.AreEqual(ShotPlan.DollyFaceX, EdgeDims.XB(ShotPlan.DollyFaceZ), 1e-9);
            Assert.AreEqual(0.0, EdgeDims.DXdZ(1, ShotPlan.DollyFaceZ), 0.0);
            double prev = double.PositiveInfinity;
            for (int n = 0; n < a.frames; n++)
            {
                a.FramePose(n, out double s, out double x, out double y, out double z, out double yaw, out double pitch, out double fov);
                double dx = ShotPlan.DollyFaceX - x, dz = ShotPlan.DollyFaceZ - z, d = Math.Sqrt(dx * dx + dz * dz);
                Assert.AreEqual(a.poseDist[n], d, 1e-9);
                Assert.Less(d, prev); prev = d;
                Assert.AreEqual(30.0, Math.Acos(dx / d) / NocMath.Deg, 1e-9, "30 deg to the normal (-x points out of face B)");
                Assert.AreEqual(Math.Atan2(dx, dz) / NocMath.Deg, yaw, 1e-9, "looking at the target");
                Assert.AreEqual(0.0, pitch, 0.0); Assert.AreEqual(ShotPlan.DollyFaceY, y, 0.0);
                Assert.Greater(x, EdgeDims.XA(z) + 20.0, "in front of face A at frame " + n);
                Assert.Less(x, EdgeDims.XB(z), "in front of face B");
                Assert.AreEqual(ShotPlan.DollyS, s, 0.0);
            }
            Assert.AreEqual(800.0, a.poseDist[0], 1e-9); Assert.AreEqual(25.0, a.poseDist[a.frames - 1], 1e-9);
            double r0 = a.poseDist[1] / a.poseDist[0], r1 = a.poseDist[a.frames - 1] / a.poseDist[a.frames - 2];
            Assert.AreEqual(r0, r1, 1e-9, "geometric steps"); Assert.AreEqual(0.9884, r0, 1e-4);
            // no cross bridge between the eye line and the target: every bridge lies outside z -500..-100 or 30 m off y 150
            for (int i = 0; i < EdgeDims.BridgeZ.Length; i++)
                Assert.IsTrue(EdgeDims.BridgeZ[i] < -510 || EdgeDims.BridgeZ[i] > -90 || Math.Abs(EdgeDims.BridgeY[i] - ShotPlan.DollyFaceY) > 30, "bridge " + i);
            // the grazing pass
            Assert.AreEqual(251, b.frames);
            for (int n = 0; n < b.frames; n++)
            {
                b.FramePose(n, out double s, out double x, out double y, out double z, out double yaw, out double pitch, out double fov);
                Assert.AreEqual(390.0 + n, x, 1e-9); Assert.AreEqual(14.0, y, 0.0); Assert.AreEqual(195.0, z, 0.0);
                Assert.AreEqual(x + NocturneConstants.EastLegOffset, s, 1e-9);
                Assert.Greater(s, 560.0); Assert.Less(s, 880.0);
                Assert.AreEqual(70.0, yaw - 90.0, 1e-12, "70 deg off face B's normal (+x = yaw 90)");
                Assert.Greater(x, SkybridgeDims.GateAX1); Assert.Less(x, SkybridgeDims.GateBX0);
            }
            // the tubes' tops stay under the bottom row: elevation of the top of the south tube from the eye
            double topEl = Math.Atan2(SkybridgeDims.TubeAxisY + SkybridgeDims.TubeR - ShotPlan.GrazeY, ShotPlan.GrazeZ - SkybridgeDims.TubeSZ) / NocMath.Deg;
            Assert.Less(topEl, ShotPlan.GrazePitch - 0.5 * ShotPlan.DollyFov, "tube top below the frame");
        }

        /// <summary>texclose: each of the 8 near poses frames its surface from 1.5 m along the centre ray (checked against the beat
        /// layouts the meshes are built from; a moved surface fails here first) and stays clear of its neighbours.</summary>
        [Test]
        public void TexClose_PosesAreOnePointFiveMetresFromTheirSurfaces()
        {
            var stills = ShotPlan.Build("texclose").stills;
            Assert.AreEqual(10, stills.Count, "8 surfaces at 1.5 m + AL's 2 empty rooms");
            var names = new HashSet<string>();
            foreach (StillShot st in stills)
            {
                Assert.AreEqual("texclose", st.set); Assert.AreEqual(1920, st.width); Assert.AreEqual(1080, st.height);
                Assert.AreEqual(ShotPlan.TexCloseFov, st.fov, 0.0); Assert.IsTrue(names.Add(st.tag));
            }
            double halfV = Math.Tan(0.5 * ShotPlan.TexCloseFov * NocMath.Deg);   // 0.601: 0.90 m half-height at 1.5 m
            double halfH = 16.0 / 9.0 * halfV;                                   // 1.068: 1.60 m half-width at 1.5 m
            foreach (ShotPlan.TexClosePose p in ShotPlan.TexClosePoses)
            {
                D3 d = QaProjection.AzElDir(p.yaw, p.pitch);
                double t;
                switch (p.name)
                {
                    case "tc_alley_wall":
                        t = (AlleyLayout.HalfWidth(p.z) - p.x) / d.x;
                        Assert.IsTrue(Math.Abs(p.z - 16.0) > 2.2 && Math.Abs(p.z - 38.5) > 2.2, "no east door in the frame");
                        Assert.Less(p.y + t * halfV, AlleyLayout.BladeSignY, "under the blade signs");
                        break;
                    case "tc_podium_panel":
                        t = (BoulevardLayout.SouthZ - p.z) / d.z;
                        Assert.Less(p.x + halfH * t, -42.0, "west of the arcade (the podium face runs to the floor)");
                        Assert.Greater(p.x - halfH * t, BoulevardLayout.SouthX0);
                        break;
                    case "tc_plaza":
                        t = -p.y / d.y;
                        Assert.Less(Math.Abs(p.z - HeartLayout.EastLegZ), 12.0, "inside the lane");
                        Assert.Less(Math.Sqrt((p.x - HeartLayout.SX) * (p.x - HeartLayout.SX) + (p.z - HeartLayout.SZ) * (p.z - HeartLayout.SZ)), HeartLayout.PlazaR);
                        Assert.AreEqual(p.x + HeartLayout.EastLegOffset, p.s, 0.01);
                        break;
                    case "tc_bridge_deck":
                        t = -p.y / d.y;
                        Assert.Less(Math.Abs(p.z - SkybridgeDims.AxisZ), SkybridgeDims.DeckHalfWidth - 1.0);
                        // the top corners of the frame meet the deck inside its edges (TQ_r01 at pitch -50 saw past the north edge)
                        foreach (double sx in new[] { -1.0, 1.0 })
                        {
                            QaProjection.Basis(p.yaw, p.pitch, out D3 f, out D3 r, out D3 u);
                            D3 c = f + u * halfV + r * (sx * halfH);
                            Assert.Less(c.y, 0.0, "top corner looks down");
                            double tc = -p.y / c.y;
                            Assert.Less(Math.Abs(p.z + tc * c.z - SkybridgeDims.AxisZ), SkybridgeDims.DeckHalfWidth - 0.15, "corner on the deck");
                        }
                        Assert.Greater(p.x, 541.3, "east of the KS13 glass strip (s 706-726)");
                        break;
                    case "tc_tenement_brick":
                        t = (HeartLayout.TenX0 - p.x) / d.x;
                        Assert.Greater(p.y - t * halfV, HeartLayout.WinSill + HeartLayout.WinH, "above the ground-floor windows");
                        Assert.Less(p.y + t * halfV, HeartLayout.Storey + HeartLayout.WinSill, "below the first-floor sills");
                        Assert.Greater(p.z - halfH * t, HeartLayout.TenZ0); Assert.Less(p.z + halfH * t, HeartLayout.GoalZC - 1.3, "clear of the goal balcony (z 386.6-389.2)");
                        break;
                    case "tc_noodle_bar":
                        t = (-AlleyLayout.HalfWidth(p.z) - AlleyLayout.CounterRecess - p.x) / d.x;
                        double hitY = p.y + t * d.y;
                        Assert.Greater(hitY, 0.5); Assert.Less(hitY, AlleyLayout.CounterH, "the centre ray meets the counter front");
                        Assert.Greater(p.z, AlleyLayout.CounterZ0); Assert.Less(p.z, AlleyLayout.CounterZ1);
                        break;
                    case "tc_trunk":
                    {
                        GardenTreeSpec tr = default;
                        foreach (GardenTreeSpec g in GardenDims.Trees) if (g.Name == "T7") tr = g;
                        Assert.AreEqual("T7", tr.Name);
                        double r = GardenTree.TrunkRadius(tr.R0, (float)p.y, GardenDims.TrunkTopFrac * tr.Height);
                        double hd = Math.Sqrt((p.x - tr.X) * (p.x - tr.X) + (p.z - tr.Z) * (p.z - tr.Z));
                        t = hd - r;
                        Assert.Less(Math.Abs(p.x - tr.X), 1e-6, "the centre ray passes through the trunk axis");
                        Assert.Less(GardenDims.ButtressGain * Math.Exp(-p.y / GardenDims.ButtressHeight) * r, 0.2, "buttress lobes under 0.2 m");
                        Assert.Greater(tr.CrownBottom, p.y + 5.0, "below the crown");
                        break;
                    }
                    case "tc_glass_tube":
                        t = (SkybridgeDims.TubeSZ + SkybridgeDims.TubeR - p.z) / d.z;
                        Assert.AreEqual(SkybridgeDims.TubeAxisY, p.y, 0.0);
                        Assert.Less(p.z, SkybridgeDims.AxisZ - SkybridgeDims.DeckHalfWidth, "outside the deck");
                        Assert.Greater(p.x, SkybridgeDims.TubeX0 + 10.0); Assert.Less(p.x, SkybridgeDims.TubeX1 - 10.0);
                        break;
                    case "tc_empty_room_e":
                    case "tc_empty_room_w":
                    {
                        // AL's request: in the alley lane, facing the window wall of its side
                        double hw = AlleyLayout.HalfWidth(p.z);
                        Assert.Less(Math.Abs(p.x), hw, p.name + " stands in the lane");
                        double side = p.name.EndsWith("_e", StringComparison.Ordinal) ? 1.0 : -1.0;
                        Assert.Greater(d.x * side, 0.95, p.name + " faces its window wall");
                        // dist is AL's horizontal distance from the eye to the window wall (x = +-HalfWidth)
                        Assert.AreEqual(p.dist, Math.Abs(side * hw - p.x), 0.05, p.name + ": dist to the window wall");
                        Assert.AreEqual(p.s, p.z, 1e-9, "alley s = z");
                        continue;
                    }
                    default: Assert.Fail("unknown texclose pose " + p.name); t = 0; break;
                }
                Assert.AreEqual(p.dist, t, 0.05, p.name + " at " + p.dist + " m");
                Assert.AreEqual(ShotPlan.TexCloseDist, t, 0.06, p.name);
            }
        }

        [Test]
        public void TextureSets_UniqueNamesAndPosesMode()
        {
            var plan = ShotPlan.Build("everything,texpans,dolly,texclose,dread,cinematic,cinematic60", new CapturePlanOptions { encode4k = true });
            Assert.IsEmpty(plan.unknownSets);
            var seen = new HashSet<string>();
            foreach (StillShot st in plan.stills) Assert.IsTrue(seen.Add(st.File), "duplicate " + st.File);
            var seq = new HashSet<string>();
            foreach (SequenceSpec sq in plan.sequences) Assert.IsTrue(seq.Add(sq.name), "duplicate " + sq.name);
            foreach (SequenceSpec sq in plan.sequences) StringAssert.StartsWith(sq.name.StartsWith("enc_") ? "enc_" : "seq_", sq.name, "Tools/qa finds seq_* folders");
        }

        /// <summary>Part 2.5: ShotPlan.RunPlayerRoute mirrors run-player.ps1's rules (its trigger list is parsed from the script), and
        /// texpans / dolly are heavy sets and -capture-ss 2 is a 4K job in both (TQ's REQUEST LEAD, carried by FX, D-032 9), and
        /// GuardNote refuses only a run that was not routed so.</summary>
        [Test]
        public void RunPlayerRoute_MirrorsTheScriptAndGuardsTheTextureSets()
        {
            string ps = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, "Tools", "run-player.ps1");
            string text = System.IO.File.ReadAllText(ps);
            var m = Regex.Match(text, @"@\((""-soak""[^)]*)\)\s*-contains");
            Assert.IsTrue(m.Success, "run-player's heavy flag list");
            var flags = new List<string>();
            foreach (Match f in Regex.Matches(m.Groups[1].Value, "\"([^\"]*)\"")) flags.Add(f.Groups[1].Value);
            CollectionAssert.AreEquivalent(new[] { "-soak", "-perf-log", "-autotest", "-autotest-until", "-wallbot", "-sequence" }, flags);
            foreach (string f in flags) { ShotPlan.RunPlayerRoute(new[] { "x.exe", f, "1" }, out bool h, out bool k4); Assert.IsTrue(h, f); Assert.IsFalse(k4, f); }
            foreach (string g in new[] { "texpans", "dolly" })
            {
                CollectionAssert.Contains(PsArray(text, "$heavySets"), g, "run-player routes " + g + " through gpu-guard");
                CollectionAssert.Contains(ShotPlan.HeavySets, g);
            }
            // run-player's -capture-ss rule: a value >= 2 sets both $heavy and $is4k
            var ssRule = Regex.Match(text, @"\$opt -eq ""-capture-ss""[^\r\n]*-ge 2\)\s*\{\s*\$heavy = \$true; \$is4k = \$true\s*\}");
            Assert.IsTrue(ssRule.Success, "run-player routes -capture-ss >= 2 as a 4K job");
            ShotPlan.RunPlayerRoute(new[] { "x.exe", "-capture", "d", "-capture-set", "texclose", "-capture-ss", "1" }, out bool h1, out bool k1);
            Assert.IsFalse(h1 || k1, "-capture-ss 1 is a plain run");

            ShotPlan.RunPlayerRoute(new[] { "x.exe", "-capture", "d", "-capture-set", "keyshots1080" }, out bool heavy, out bool fourK);
            Assert.IsFalse(heavy); Assert.IsFalse(fourK);
            ShotPlan.RunPlayerRoute(new[] { "x.exe", "-capture", "d" }, out heavy, out fourK);
            Assert.IsTrue(heavy && fourK, "-capture without a set renders the key shots (4K)");
            ShotPlan.RunPlayerRoute(new[] { "x.exe", "-capture", "d", "-capture-set", "texpans", "-seq-res", "3840x2160" }, out heavy, out fourK);
            Assert.IsTrue(heavy && fourK);
            ShotPlan.RunPlayerRoute(new[] { "x.exe", "-capture", "d", "-capture-set", "keyshots" }, out heavy, out fourK);
            Assert.IsTrue(heavy && fourK);

            string[] texpans = { "x.exe", "-capture", "d", "-capture-set", "texpans" };
            ShotPlan.RunPlayerRoute(texpans, out heavy, out fourK);
            Assert.IsTrue(heavy && !fourK, "texpans: heavy, 1080p");
            Assert.IsNull(ShotPlan.GuardNote(texpans, "texpans", 1), "texpans needs no -sequence workaround");
            Assert.IsNull(ShotPlan.GuardNote(new[] { "x.exe", "-capture", "d", "-capture-set", "texpans", "-sequence", "texpans" }, "texpans", 1));
            Assert.IsNull(ShotPlan.GuardNote(new[] { "x.exe", "-capture", "d", "-capture-set", "texclose" }, "texclose", 1), "8 stills: no guard");
            string[] ss2 = { "x.exe", "-capture", "d", "-capture-set", "keyshots1080", "-capture-ss", "2" };
            ShotPlan.RunPlayerRoute(ss2, out heavy, out fourK);
            Assert.IsTrue(heavy && fourK, "-capture-ss 2 is a 4K job");
            Assert.IsNull(ShotPlan.GuardNote(ss2, "keyshots1080", 2), "-capture-ss 2 needs no -capture-4k workaround");
            Assert.IsNotNull(ShotPlan.GuardNote(new[] { "x.exe", "-capture", "d", "-capture-set", "keyshots1080" }, "keyshots1080", 2), "a supersampled run without the 4K route is refused");
            Assert.IsNotNull(ShotPlan.GuardNote(new[] { "x.exe", "-capture", "d", "-capture-set", "keyshots1080" }, "dolly", 1), "a heavy set without the heavy route is refused");
            Assert.IsNull(ShotPlan.GuardNote(new[] { "x.exe", "-capture", "d", "-capture-set", "keyshots1080", "-capture-ss", "2", "-capture-4k" }, "keyshots1080", 2));
            Assert.IsNull(ShotPlan.GuardNote(new[] { "x.exe", "-capture", "d", "-capture-set", "dolly", "-capture-ss", "2", "-capture-4k" }, "dolly", 2), "-capture-4k routes heavy and 4K");
            Assert.IsTrue(ShotPlan.NeedsGuard("dolly") && ShotPlan.NeedsGuard("texpans") && !ShotPlan.NeedsGuard("texclose"));
            Assert.AreEqual(3840, CaptureHarness.MaxSsRenderWidth); Assert.AreEqual(2, CaptureHarness.MaxSupersample);
        }

        /// <summary>FA's facade sheet (reflection, so the plan never depends on FA's file): the facadesheet set lists every pose of
        /// NocFacadeSheet.Poses(), the mirrored sheet constants equal FA's, and with -facade-sheet the dolly's 30 deg line runs to the
        /// front face of subject 0's frontal tower, in front of every other tower and of face A at full flare.</summary>
        [Test]
        public void FacadeSheet_SetAndDollyFollowTheSheet()
        {
            Type t = typeof(ShotPlan).Assembly.GetType("Megastructure.NocFacadeSheet", false);
            if (t == null) Assert.Ignore("NocFacadeSheet is not in this build (FA)");
            var poses = (System.Collections.IList)t.GetMethod("Poses").Invoke(null, null);
            var plan = ShotPlan.Build("facadesheet");
            Assert.IsEmpty(plan.unknownSets);
            Assert.AreEqual(poses.Count, plan.stills.Count);
            foreach (StillShot st in plan.stills) { Assert.AreEqual("facadesheet", st.set); StringAssert.StartsWith("pose_fs", st.tag); }
            foreach (string n in new[] { "X0", "RowZ", "Half", "EyeY" })
                Assert.AreEqual(Convert.ToDouble(t.GetField(n).GetRawConstantValue()), ShotPlan.SheetConst(n, double.NaN), 0.0, n);
            Assert.AreEqual(ShotPlan.SheetX0, ShotPlan.SheetConst("X0", 0), 0.0, "mirrored fallback = FA's value");
            Assert.AreEqual(ShotPlan.SheetRowZ, ShotPlan.SheetConst("RowZ", 0), 0.0);

            var q = ShotPlan.Build("dolly", new CapturePlanOptions { facadeSheet = true }).sequences;
            SequenceSpec a = FindSeq(q, "seq_dolly_sheet30");
            Assert.IsNotNull(a);
            Assert.IsNull(FindSeq(q, "seq_dolly_face30"));
            double tx = ShotPlan.SheetX0, tz = ShotPlan.SheetRowZ - ShotPlan.SheetHalf;
            for (int n = 0; n < a.frames; n++)
            {
                a.FramePose(n, out double s, out double x, out double y, out double z, out double yaw, out double pitch, out double fov);
                double dx = tx - x, dz = tz - z, d = Math.Sqrt(dx * dx + dz * dz);
                Assert.AreEqual(a.poseDist[n], d, 1e-9);
                Assert.AreEqual(30.0, Math.Acos(dz / d) / NocMath.Deg, 1e-9, "30 deg to the face normal (-z)");
                Assert.AreEqual(Math.Atan2(dx, dz) / NocMath.Deg, yaw, 1e-9);
                Assert.Less(z, tz, "south of the row: no other tower between");
                Assert.Greater(x, EdgeDims.XA(z) + 20.0); Assert.Less(x, EdgeDims.XB(z) - 20.0);
                Assert.AreEqual(ShotPlan.SheetS, s, 0.0);
            }
            Assert.AreEqual(800.0, a.poseDist[0], 1e-9);
        }

        /// <summary>TEXTURE_PLAN 7.7: A1's gpuAvgMs (571 319 705 ms) came from wrapped 64-bit timestamps (1.8e12-1.7e13 ms) and
        /// duplicate reads; a GPU sample is valid only when finite, positive and at most 4 x its frame interval + 2 ms.</summary>
        [Test]
        public void PerfGpu_RejectsWrappedTimestampsAndKeepsRealFrames()
        {
            Assert.IsTrue(PerfGpu.Valid(0.273, 0.519), "A1's median sample");
            Assert.IsFalse(PerfGpu.Valid(1.7061487e13, 0.52), "A1's worst sample: a wrapped timestamp");
            Assert.IsFalse(PerfGpu.Valid(1.794612e12, 0.52));
            Assert.IsFalse(PerfGpu.Valid(0.0, 1.0)); Assert.IsFalse(PerfGpu.Valid(-1.0, 1.0)); Assert.IsFalse(PerfGpu.Valid(double.NaN, 1.0));
            Assert.IsFalse(PerfGpu.Valid(double.PositiveInfinity, 1.0));
            Assert.IsTrue(PerfGpu.Valid(30.0, 16.7), "a GPU-bound 30 ms frame at a 16.7 ms interval (query lag)");
            Assert.IsFalse(PerfGpu.Valid(80.0, 16.7), "longer than 4 frame intervals + 2 ms");
            Assert.IsTrue(PerfGpu.Valid(900.0, -1.0), "unknown interval: only the absolute cap");
            Assert.IsFalse(PerfGpu.Valid(1500.0, -1.0));
            var ms = new float[] { 0.3f, -1f, 0.2f, 0.4f, -1f };
            PerfGpu.Stats(ms, ms.Length, out int n, out double mean, out double median, out double p99, out double max);
            Assert.AreEqual(3, n); Assert.AreEqual(0.3, mean, 1e-6); Assert.AreEqual(0.3, median, 1e-6); Assert.AreEqual(0.4, max, 1e-6); Assert.AreEqual(0.4, p99, 1e-6);
            PerfGpu.Stats(new float[] { -1f, -1f }, 2, out n, out mean, out _, out _, out _);
            Assert.AreEqual(0, n); Assert.IsTrue(double.IsNaN(mean));
            Assert.IsTrue(PerfGpu.Consistent(0.273, 0.568));
            Assert.IsFalse(PerfGpu.Consistent(571319705.16, 0.568), "A1's summary");
            Assert.IsFalse(PerfGpu.Consistent(double.NaN, 0.568));
            Assert.IsTrue(PerfGpu.Consistent(10.4, 10.0), "GPU-bound: within 5 % + 0.05 ms of the frame time");
        }

        /// <summary>QA fix wave C23: the cinematic windows are the runtime WalkPlanner's own camera at 2.5 m/s (from rest at s 0, the
        /// follower and its rate limits, the sway), one pose per frame at 30 and 60 fps, covering each window, with the boundaries
        /// they cross; the ending window runs through the stop to s 1500.</summary>
        [Test]
        public void Cinematic_WindowsFollowTheRuntimePlanner()
        {
            foreach (int fps in new[] { ShotPlan.SeqFps, ShotPlan.MotionFps })
            {
                var list = ShotPlan.Build(fps == ShotPlan.SeqFps ? "cinematic" : "cinematic60").sequences;
                Assert.AreEqual(ShotPlan.CinematicWindows.GetLength(0), list.Count);
                ShotPlan.CinematicTrace(fps, out double[] ts, out double[] ty, out double[] tp, out double[] tt);
                Assert.AreEqual(NocturneConstants.GoalS, ts[ts.Length - 1], 1e-3, "the trace ends standing at the goal");
                // the trace is the planner itself: compare with a fresh run at a few frames
                var plan = new WalkPlanner(NocLanes.Path, ShotPlan.CinematicSeed);
                NocLanes.Path.PointAt(0.0, out double px, out double pz);
                GazeSchedule.Evaluate(0.0, out double y0, out double p0);
                plan.Begin(WalkMode.Cinematic, px, pz, 0.0, 0.0, 0.0, NocMath.WrapDeg(y0), p0, NocturneConstants.CinematicSpeed);
                double dt = 1.0 / fps, time = 0.0;
                for (int k = 1; k <= 30 * fps; k++)
                {
                    plan.Step(dt, plan.TargetX, plan.TargetZ, time); time += dt;
                    if (k % fps == 0) { Assert.AreEqual(plan.D, ts[k], 1e-9); Assert.AreEqual(plan.OutYaw, ty[k], 1e-9); Assert.AreEqual(plan.OutPitch, tp[k], 1e-9); }
                }
                for (int i = 0; i < list.Count; i++)
                {
                    SequenceSpec q = list[i];
                    Assert.AreEqual(SeqMode.Cinematic, q.mode); Assert.AreEqual("cinematic", q.kind); Assert.AreEqual(fps, q.fps);
                    StringAssert.StartsWith(fps == ShotPlan.SeqFps ? "seq_cin_s" : "seq_cin60_s", q.name);
                    double a = ShotPlan.CinematicWindows[i, 0], b = ShotPlan.CinematicWindows[i, 1];
                    Assert.AreEqual(a, q.s0, 2.5 / fps + 1e-9, q.name + " starts at its window");
                    if (b < NocturneConstants.PathLength)
                    {
                        Assert.AreEqual(b, q.s1, 2.5 / fps + 1e-9, q.name + " ends at its window");
                        Assert.AreEqual((b - a) / NocturneConstants.CinematicSpeed * fps, q.frames, 2.0, q.name + ": 2.5 m/s");
                    }
                    else Assert.AreEqual(NocturneConstants.GoalS, q.s1, 1e-3, "the ending window stands at the goal");
                    for (int n = 1; n < q.frames; n++)
                    {
                        Assert.Greater(q.poseS[n], q.poseS[n - 1] - 1e-12, "s never runs back");
                        double ds = q.poseS[n] - q.poseS[n - 1];
                        Assert.LessOrEqual(ds, NocturneConstants.CinematicSpeed / fps + 1e-9);
                        Assert.LessOrEqual(Math.Abs(NocMath.DeltaAngle(q.poseYaw[n - 1], q.poseYaw[n])), (WalkPlanner.YawRateMax + 20.0) / fps, "yaw rate (follower + sway)");
                        Assert.LessOrEqual(Math.Abs(q.posePitch[n] - q.posePitch[n - 1]), (WalkPlanner.PitchRateMax + 20.0) / fps, "pitch rate");
                    }
                    Assert.AreEqual(q.ambient0 + (q.frames - 1) / (double)fps, q.AmbientAt(q.frames - 1), 1e-9, "the clock runs with the walk");
                    foreach (double bs in q.boundaryS) { Assert.Greater(bs, q.s0); Assert.LessOrEqual(bs, q.s1); }
                    q.FramePose(q.frames / 2, out double s, out double x, out double y, out double z, out double yaw, out double pitch, out double fov);
                    D3 c = NocMath.CorridorToWorld(s, 0.0, NocturneConstants.EyeHeight);
                    Assert.AreEqual(c.x, x, 1e-9); Assert.AreEqual(c.z, z, 1e-9); Assert.AreEqual(NocMath.Fov(s), fov, 1e-9);
                }
            }
        }

        /// <summary>D-043 (THE ARK, FX): the ark set has the Ark stills (set "ark", path poses with yaw relative to the heading) and
        /// its sequences: three cinematic windows and two walks of kind "dread" (the photosensitivity gate) and AB's four 60 fps
        /// bridge walks of kind "traffic" over s 560-880; it is a heavy set in ShotPlan and in run-player.</summary>
        [Test]
        public void Ark_SetHasTheStillsTheCinematicWindowsAndTheBridgeWalks()
        {
            var plan = ShotPlan.Build("ark");
            Assert.AreEqual(ShotPlan.ArkPoses.GetLength(0), plan.stills.Count);
            foreach (StillShot st in plan.stills)
            {
                Assert.AreEqual("ark", st.set); StringAssert.StartsWith("ark_s", st.tag);
                Assert.AreEqual(1.7, st.y, 1e-9, st.tag + " eye height");
            }
            StillShot bow = plan.stills.Find(x => x.tag == "ark_s0494_bow_gaze");
            Assert.IsNotNull(bow); Assert.AreEqual(QaDriveMath.HeadingDeg(494.0) - 80.0, bow.yaw, 1e-9); Assert.AreEqual(16.0, bow.pitch, 0.0);
            var seqs = plan.sequences;
            Assert.AreEqual(9, seqs.Count);
            foreach (SequenceSpec q in seqs) { Assert.AreEqual("ark", q.set); StringAssert.StartsWith("seq_ark_", q.name); }
            Assert.AreEqual(5, seqs.FindAll(q => q.kind == "dread").Count);
            var bridge = seqs.FindAll(q => q.kind == "traffic");
            Assert.AreEqual(4, bridge.Count);
            Assert.AreEqual(560.0, bridge[0].s0, 1e-9); Assert.AreEqual(880.0, bridge[3].s1, 1e-9);
            foreach (SequenceSpec q in bridge) { Assert.AreEqual(ShotPlan.MotionFps, q.fps); Assert.AreEqual(1921, q.frames, q.name); }
            SequenceSpec belly = seqs.Find(q => q.name == "seq_ark_cin_belly");
            Assert.IsNotNull(belly); Assert.AreEqual(SeqMode.Cinematic, belly.mode); Assert.AreEqual(ShotPlan.SeqFps, belly.fps);
            Assert.AreEqual(740.0, belly.s0, 0.1); Assert.AreEqual(846.0, belly.s1, 0.1);
            CollectionAssert.Contains(ShotPlan.HeavySets, "ark");
            string ps = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, "Tools", "run-player.ps1");
            CollectionAssert.Contains(PsArray(System.IO.File.ReadAllText(ps), "$heavySets"), "ark");
        }

        /// <summary>DR's dread set (QA fix wave C22): the hull, shadow and panel walks plus a cinematic window over every sheet lightning
        /// and darkness wave, kind "dread" (the photosensitivity gate of Tools/qa/sequences.py).</summary>
        [Test]
        public void Dread_SetHasTheWalksAndTheLightEvents()
        {
            var list = ShotPlan.Build("dread").sequences;
            Assert.AreEqual(5 + ShotPlan.SheetLightningS.Length + ShotPlan.DarknessWaveS.Length, list.Count);
            foreach (SequenceSpec q in list) { Assert.AreEqual("dread", q.kind); Assert.AreEqual("dread", q.set); StringAssert.StartsWith("seq_dread_", q.name); }
            SequenceSpec panel = list.Find(q => q.name == "seq_dread_panel");
            Assert.AreEqual(468.0, panel.s0, 1e-9); Assert.AreEqual(NocturneConstants.CinematicSpeed, panel.speed, 1e-9); Assert.AreEqual(ShotPlan.MotionFps, panel.fps);
            SequenceSpec pass = list.Find(q => q.name == "seq_dread_hull_pass");
            Assert.AreEqual(544.0, pass.s0, 1e-9); Assert.AreEqual(30.0, pass.yaw, 0.0); Assert.AreEqual(40.0, pass.pitch, 0.0); Assert.IsTrue(pass.yawWorld);
            Assert.AreEqual(289, pass.frames, "(556 - 544) m / 2.5 m/s x 60 fps + 1");
            Assert.AreEqual(ShotPlan.SignsAmbient0, ShotPlan.SignsSequence(1920, 1080).ambient0, 0.0, "SG: the KS16 hold starts at t 445");
            foreach (double s in ShotPlan.SheetLightningS)
            {
                SequenceSpec q = list.Find(x => x.name == "seq_dread_lightning_s" + ((int)s).ToString("D4"));
                Assert.IsNotNull(q, "lightning at s " + s); Assert.AreEqual(SeqMode.Cinematic, q.mode);
                Assert.Less(q.s0, s); Assert.Greater(q.s1, s);
            }
            foreach (double s in ShotPlan.DarknessWaveS)
            {
                SequenceSpec q = list.Find(x => x.name == "seq_dread_darkwave_s" + ((int)s).ToString("D4"));
                Assert.IsNotNull(q); Assert.GreaterOrEqual(q.frames / (double)q.fps, 29.0, "30 s of the wave");
            }
            CollectionAssert.Contains(ShotPlan.HeavySets, "dread"); CollectionAssert.Contains(ShotPlan.HeavySets, "cinematic");
        }

        /// <summary>HT's request: -autotest-until at or past the path end is no until (the end-of-walk checks apply).</summary>
        [Test]
        public void AutoTestUntil_PastThePathEndIsNoUntil()
        {
            Assert.IsTrue(double.IsNaN(AutoTestRunner.EffectiveUntil(double.NaN)));
            Assert.IsTrue(double.IsNaN(AutoTestRunner.EffectiveUntil(NocturneConstants.PathLength)));
            Assert.IsTrue(double.IsNaN(AutoTestRunner.EffectiveUntil(2000.0)));
            Assert.AreEqual(880.0, AutoTestRunner.EffectiveUntil(880.0), 0.0);
            Assert.AreEqual(1499.0, AutoTestRunner.EffectiveUntil(1499.0), 0.0);
            // AT's request: an until at or past the goal trigger is reached in the stop ramp: the window runs trigger..stop
            double trig = AutoTestRunner.ExpectedTrigger(NocturneConstants.CinematicSpeed, true, 0.0), stop = trig + 2.4;
            AutoTestRunner.ExpectedUntil(1499.0, 0.0, NocturneConstants.CinematicSpeed, true, trig, stop, out double lo, out double hi);
            Assert.AreEqual(trig, lo, 1e-9); Assert.AreEqual(stop, hi, 1e-9);
            AutoTestRunner.ExpectedUntil(880.0, 0.0, NocturneConstants.CinematicSpeed, true, trig, stop, out lo, out hi);
            Assert.AreEqual(880.0 / NocturneConstants.CinematicSpeed + 0.5 * NocturneConstants.CineAccelTime, lo, 1e-9); Assert.AreEqual(lo, hi, 0.0);
        }

        /// <summary>run-player (QA fix wave): a comma list of sets is classified per set (HT's request), the disk guard refuses a start
        /// under 5 GB free (C24), and the facadesheet set gets -facade-sheet (C20).</summary>
        [Test]
        public void RunPlayer_SplitsSetListsAndGuardsTheDisk()
        {
            string ps = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, "Tools", "run-player.ps1");
            string text = System.IO.File.ReadAllText(ps);
            StringAssert.Contains("@($val.Split(',') | Where-Object { $heavySets -contains $_ }).Count -gt 0", text);
            StringAssert.Contains("@($val.Split(',') | Where-Object { $fourKSets -contains $_ }).Count -gt 0", text);
            Assert.IsTrue(Regex.IsMatch(text, @"\[double\]\$MinFreeGB = 5\.0"), "refuse under 5 GB free");
            StringAssert.Contains("exit 94", text);
            StringAssert.Contains("-facade-sheet", text);
            Assert.IsNull(ShotPlan.GuardNote(new[] { "x.exe", "-capture", "d", "-capture-set", "statics,boundaries" }, "statics,boundaries", 1));
        }
    }
}
