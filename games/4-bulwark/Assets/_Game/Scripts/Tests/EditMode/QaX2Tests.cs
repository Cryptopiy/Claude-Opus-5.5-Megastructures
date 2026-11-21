// QaX2Tests: the lead QA fixes of wave X2 (lead task S2-1, S3-5): the QA gate sees the new ending. Pure plan / audit checks:
// seq_end keyed to the real goal (EndingPath.SGoal - 3) over the whole EndTotal, the ride sequences re-keyed to EndingPath,
// the kraken sequences, the WalkGaze mode (the AutoWalker look, within the 18 / 13 deg/s limits), the ending still variants
// (16:10, 21:9), the VisibilityAudit without the whale and tolerant of the KK / KS22-KS29 rows, the perf poses, the soak's
// final-loop rule and the T-PREC pan step.
using System;
using System.Collections.Generic;
using Megastructure.QA;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class QaX2Tests
    {
        [Test]
        public void EndSequence_KeyedToTheRealGoal_CoversEndTotal()
        {
            var q = ShotPlan.EndSequence(1920, 1080);
            Assert.AreEqual(SeqMode.RealEnd, q.mode);
            Assert.AreEqual(EndingPath.SGoal - ShotPlan.EndLeadIn, q.s0, 1e-9, "3 m before the Keeper's Lantern trigger");
            Assert.Greater(q.s0, BwPath.Length, "on the lantern platform, past the walked path (the old 1495.5 was the cabin door)");
            double seconds = q.frames / (double)q.fps;
            Assert.GreaterOrEqual(seconds, ShotPlan.EndLeadIn / q.speed + FrameworkConst.EndTotal, "frames reach the trigger and the whole EndTotal");
            Assert.AreEqual(FrameworkConst.EndTotal + ShotPlan.EndTail, q.afterGoal, 1e-9);
        }

        [Test]
        public void RideSequences_ReKeyedToEndingPath()
        {
            var list = ShotPlan.RideSequences(960, 540, true);
            var names = new List<string>();
            foreach (var q in list)
            {
                names.Add(q.name);
                Assert.AreEqual(SeqMode.RealWalk, q.mode, q.name);
                Assert.GreaterOrEqual(q.s0, EndingPath.S0 - 1e-9, q.name);
                Assert.LessOrEqual(q.s1, EndingPath.SGoal + 1e-9, q.name);
                Assert.Greater(q.frames, 0, q.name);
            }
            CollectionAssert.AreEqual(new[] { "seq_ride_depart", "seq_ride_arrive", "seq_shuttle", "seq_turnback" }, names);
            var shuttle = list[2];
            Assert.AreEqual(60, shuttle.fps, "the shuttle at 60 fps (lamp strobing)");
            Assert.AreEqual(EndingPath.STransfer1 - 0.5, shuttle.s0, 1e-9);
            Assert.AreEqual(EndingPath.SShuttle1, shuttle.s1, 1e-9);
            Assert.GreaterOrEqual(shuttle.frames / 60.0, BulwarkConst.RideShuttleTime, "the whole shuttle run");
            var turn = list[3];
            Assert.AreEqual(ShotPlan.TurnbackAfterGoal, turn.afterGoal, 1e-9, "alone: through the goal and t_g 0-45");
            var withEdges = ShotPlan.RideSequences(960, 540, false)[3];
            Assert.IsTrue(double.IsNaN(withEdges.afterGoal), "with seq_end in the same run the turn-back stops before the goal");
            Assert.Less(withEdges.s1, EndingPath.SGoal);

            // one goal per run: "all" puts seq_end last and the turn-back before the goal
            var plan = ShotPlan.Build("ride,edges");
            Assert.AreEqual("seq_end", plan.sequences[plan.sequences.Count - 1].name);
            foreach (var q in plan.sequences) if (q.name == "seq_turnback") Assert.IsTrue(double.IsNaN(q.afterGoal));
        }

        [Test]
        public void KrakenSequences_A1Windows_WalkGaze()
        {
            var list = ShotPlan.KrakenSequences(1920, 1080);
            Assert.AreEqual(7, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual("seq_k" + (i + 1), list[i].name);
                Assert.AreEqual(SeqMode.WalkGaze, list[i].mode);
                Assert.AreEqual(30, list[i].fps);
                Assert.Less(list[i].s1, BwPath.Length);
            }
        }

        [Test]
        public void TransitionSequences_UseTheAutoWalkerGaze()
        {
            foreach (var q in ShotPlan.TransitionSequences(1920, 1080)) Assert.AreEqual(SeqMode.WalkGaze, q.mode, q.name);
        }

        [Test]
        public void WalkGaze_FollowsTheSchedule_WithinTheRateLimits()
        {
            var q = ShotPlan.GazeSeq("t", "t", "", 1060.0, 1110.0, 30, 1920, 1080);
            double dt = 1.0 / q.fps, maxYaw = 0, maxPitch = 0;
            q.FramePose(0, out double s0, out _, out _, out _, out double y0, out double p0, out _);
            Assert.AreEqual(1060.0, s0, 1e-9);
            double py = y0, pp = p0;
            for (int n = 1; n < q.frames; n++)
            {
                q.FramePose(n, out double s, out double x, out double y, out double z, out double yaw, out double pitch, out double fov);
                Assert.IsFalse(double.IsNaN(x + y + z + yaw + pitch + fov), "finite pose at frame " + n);
                maxYaw = Math.Max(maxYaw, Math.Abs(BwMath.DeltaDeg(py, yaw)) / dt);
                maxPitch = Math.Max(maxPitch, Math.Abs(pitch - pp) / dt);
                py = yaw; pp = pitch;
            }
            Assert.LessOrEqual(maxYaw, FrameworkConst.GazeMaxYawRate + 1e-6, "yaw rate (deg/s)");
            Assert.LessOrEqual(maxPitch, FrameworkConst.GazeMaxPitchRate + 1e-6, "pitch rate (deg/s)");
            // the look moves (the s 1070-1100 turn to the window hall): not the old fixed yaw 0 / pitch 0
            q.FramePose(q.frames - 1, out _, out _, out _, out _, out double yEnd, out _, out _);
            Assert.Greater(Math.Abs(BwMath.DeltaDeg(0.0, yEnd)), 20.0, "the gaze turned toward the hall");
        }

        [Test]
        public void SequenceEye_ContinuesIntoTheEndingTimeline()
        {
            SequenceSpec.EyeAt(BwPath.Length, out double x0, out double y0, out double z0);
            SequenceSpec.EyeAt(BwPath.Length + 1e-4, out double x1, out double y1, out double z1);
            Assert.Less(Math.Abs(x1 - x0) + Math.Abs(y1 - y0) + Math.Abs(z1 - z0), 0.5, "no jump at s 1500");
            SequenceSpec.EyeAt(EndingPath.SRest, out double xr, out double yr, out double zr);
            Assert.AreEqual(EndingPath.RestX, xr, 1e-6);
            Assert.AreEqual(EndingPath.RestZ, zr, 1e-6);
            Assert.AreEqual(EndingPath.EyeY(EndingPath.SRest), yr, 1e-6);
            Assert.AreEqual(EndingPath.HeadingDeg(EndingPath.SRest), SequenceSpec.HeadingAt(EndingPath.SRest), 1e-9);
        }

        static StillShot S(string tag, int w, int h, bool gameFov = true)
            => new StillShot { tag = tag, set = "ending", baseTag = tag, variant = "1080", width = w, height = h, s = 1847.5, fov = 66, gameFov = gameFov };

        [Test]
        public void EndingVariants_AddSixteenTenAndTwentyOneNine()
        {
            var src = new List<StillShot>
            {
                S("b08_Ending_KS28_dam_reveal", 1920, 1080), S("b08_Ending_KS28_dam_reveal_219", 2520, 1080),
                S("b08_Ending_KS28_dam_reveal_4k", 3840, 2160), S("b08_Ending_KS28c_crest_zoom", 1920, 1080, false),
            };
            var outList = ShotPlan.EndingVariants(src);
            var by = new Dictionary<string, StillShot>();
            foreach (var st in outList) by[st.tag] = st;
            Assert.AreEqual(1920, by["b08_Ending_KS28_dam_reveal"].width, "the 1080p still keeps its size");
            Assert.AreEqual(2560, by["b08_Ending_KS28_dam_reveal_219"].width, "2520x1080 corrected to 2560x1080");
            Assert.AreEqual(1680, by["b08_Ending_KS28_dam_reveal_1610"].width);
            Assert.AreEqual(1050, by["b08_Ending_KS28_dam_reveal_1610"].height);
            Assert.AreEqual(3840, by["b08_Ending_KS28_dam_reveal_4k"].width);
            Assert.IsFalse(by.ContainsKey("b08_Ending_KS28c_crest_zoom_1610"), "QA zooms (not the game FOV) get no variants");
            Assert.AreEqual(5, outList.Count);
        }

        [Test]
        public void VisibilityAudit_NoWhale_TolerantOfTheNewRows()
        {
            var ids = new List<string> { "KK1", "KK2", "KK3", "KK4", "KS17", "KS22", "KS23", "KS24", "KS25", "KS26", "KS27", "KS28", "KS29" };
            foreach (var k in BulwarkConst.KeyShots) ids.Add(VisibilityAudit.KeyId(k.tag));
            foreach (string id in ids)
            {
                var subs = new List<VisibilityAudit.Subject>();
                Assert.DoesNotThrow(() => VisibilityAudit.SubjectsForShot(id, subs), id);
                foreach (var sub in subs) StringAssert.DoesNotContain("whale", sub.Name, id + ": the whale is removed (A1 4.9)");
            }
            Assert.AreEqual("KK1", VisibilityAudit.KeyId("KK1_arm_tower"));
            Assert.AreEqual("KS04", VisibilityAudit.KeyId("KS04_crest_sea"));
            // KS29: the stinger proxy (end clock t_g 30) and the crest line are subjects
            var s29 = new List<VisibilityAudit.Subject>();
            VisibilityAudit.SubjectsForShot("KS29", s29);
            bool crest = false, kraken = false;
            foreach (var sub in s29) { crest |= sub.Name == "crest_line"; kraken |= sub.Name.StartsWith("kraken_", StringComparison.Ordinal); }
            Assert.IsTrue(crest && kraken, "KS29 audits the crest line and the stinger");
        }

        [Test]
        public void PerfPoses_Resolve()
        {
            foreach (string n in new[] { "KS17", "ride", "KS28", "KS27" })
            {
                Assert.IsTrue(PerfPoseRunner.TryPose(n, out var p), n);
                Assert.IsFalse(double.IsNaN(p.x + p.y + p.z + p.yaw + p.pitch), n);
            }
            Assert.IsTrue(PerfPoseRunner.TryPose("KS28", out var k28));
            Assert.AreEqual(EndingPath.SRest, k28.s, 1e-9);
            Assert.AreEqual(BulwarkConst.EndLookPitch, k28.pitch, 1e-9);
            Assert.IsFalse(PerfPoseRunner.TryPose("nope", out _));
        }

        [Test]
        public void Soak_FinalLoopRunsIntoTheEnding()
        {
            double loop = FrameworkConst.TriggerTimeAt(2.5);
            Assert.IsFalse(SoakRunner.IsFinalLoop(0.0, loop, 1800.0), "1800 s: the first loop wraps before the goal");
            Assert.IsTrue(SoakRunner.IsFinalLoop(loop, loop, 1800.0), "the second loop runs into the ending");
            Assert.LessOrEqual(2.0 * loop + FrameworkConst.EndTotal, 1800.0, "two loops and the ending fit in 1800 s at 2.5 m/s");
        }

        [Test]
        public void TPrec_PanIsATenthOfAPixelPerFrame()
        {
            var list = ShotPlan.TPrecSequences(1920, 1080);
            Assert.AreEqual(3, list.Count);
            var pan = list[1];
            Assert.AreEqual(SeqMode.PanYaw, pan.mode);
            Assert.AreEqual(0.1 * 66.0 / 1080.0, pan.panStepDeg, 1e-9, "0.1 px of the vertical px pitch at vFOV 66");
            Assert.AreEqual(120, pan.frames);
            Assert.IsFalse(pan.advanceAmbient, "frozen clock");
            Assert.IsTrue(list[2].fixedLook);
            Assert.IsTrue(EndingPath.InRide(list[2].s0), "the cabin frames are in the ride");
        }

        [Test]
        public void PoseProgress_NeverFiresTheGoal()
        {
            if (GameState.GoalReached) Assert.Ignore("goal state set by another test");
            Assert.Less(CaptureHarness.PoseProgressS(EndingPath.SRest), EndingPath.SGoal);
            Assert.AreEqual(1200.0, CaptureHarness.PoseProgressS(1200.0), 1e-12);
        }
    }
}
