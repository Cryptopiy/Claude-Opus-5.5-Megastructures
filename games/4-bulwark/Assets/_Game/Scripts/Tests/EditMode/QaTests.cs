// QaTests: EditMode tests of the pure helpers of the runtime QA harness (MASTER_PROMPT 11.2, 11.8, 14.10.2, 14.10.4).
// Imported from Thread's QaTests: the projection core, the plan grammar, the metrics, the perf summary, the soak growth
// rule, the QA mode and the JSON writer keep their tests; Thread's KF1-KF7 subject tests and the wide / extra-still tests are
// removed; the key-shot, beat-still, sequence and autotest tests are rewritten for Bulwark (BulwarkConst.KeyShots, 14.6
// windows, 0.5 / 0.1 px pans, 120-frame statics, 14.10.4 times).
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Megastructure.QA;
using Megastructure.Rendering;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class QaTests
    {
        static QaView View169(double yaw, double pitch, double fov) => new QaView(0, 1.7, 0, yaw, pitch, fov, 1920, 1080);

        static StillShot FindStill(List<StillShot> list, string tag)
        {
            for (int i = 0; i < list.Count; i++) if (list[i].tag == tag) return list[i];
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
            double tanH = Math.Tan(31.0 * BwMath.Deg) * 1920.0 / 1080.0;   // Hor+: vertical FOV fixed
            double az = Math.Atan(tanH) / BwMath.Deg;
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

        [Test]
        public void Projection_DiscBoxMatchesAnIndependentDenseSampling()
        {
            // a 2 degree disc at infinity, 10 degrees right and 5 up of the view axis, against a dense independent sampling
            var v = View169(0, 0, 64);
            D3 dir = QaProjection.AzElDir(10, 5);
            QaSubject disc = QaProjection.DiscAtInfinity(v, "disc", dir, 2.0, 720);
            double x0 = double.MaxValue, x1 = double.MinValue, y0 = double.MaxValue, y1 = double.MinValue;
            QaProjection.Basis(10, 5, out D3 f, out D3 r, out D3 u);
            double rad = 2.0 * BwMath.Deg;
            for (int i = 0; i < 20000; i++)
            {
                double a = 2 * Math.PI * i / 20000.0;
                D3 d = f * Math.Cos(rad) + (r * Math.Cos(a) + u * Math.Sin(a)) * Math.Sin(rad);
                if (!QaProjection.ProjectDir(v, d, out double nx, out double ny)) continue;
                double px = QaProjection.PixelX(v, nx), py = QaProjection.PixelYTop(v, ny);
                x0 = Math.Min(x0, px); x1 = Math.Max(x1, px); y0 = Math.Min(y0, py); y1 = Math.Max(y1, py);
            }
            Assert.AreEqual(x0, disc.x0, 0.5); Assert.AreEqual(x1, disc.x1, 0.5);
            Assert.AreEqual(y0, disc.y0, 0.5); Assert.AreEqual(y1, disc.y1, 0.5);
        }

        [Test]
        public void KeyFrameId_StripsVariantSuffixes()
        {
            Assert.AreEqual("KS04", QaFraming.KeyFrameId("b02_Crest_KS04_crest_sea_1680x1050"));
            Assert.AreEqual("KS15", QaFraming.KeyFrameId("b06_WindowGallery_KS15_glass_4k"));
            Assert.AreEqual("KS01", QaFraming.KeyFrameId("KS01_gal_far"));
            Assert.AreEqual("", QaFraming.KeyFrameId("b01_WaveGallery_mid"));
        }

        // ------------------------------------------------------------------ capture plan: key shots (14.10.2)

        [Test]
        public void KeyShots_EveryShotAtBothResolutions()
        {
            var plan = ShotPlan.Build("keyshots").stills;
            foreach (KeyShot k in BulwarkConst.KeyShots)
            {
                string tag = ShotPlan.KeyTag(k);
                StillShot a = FindStill(plan, tag), b = FindStill(plan, tag + "_4k");
                Assert.IsNotNull(a, tag); Assert.IsNotNull(b, tag + "_4k");
                Assert.AreEqual(1920, a.width); Assert.AreEqual(1080, a.height);
                Assert.AreEqual(3840, b.width); Assert.AreEqual(2160, b.height);
                Assert.IsTrue(a.framing && b.framing);
            }
        }

        [Test]
        public void KeyShots_TableFovMatchesGameFov()
        {
            // 14.10.2: a key shot uses the schedule vFOV at its s (the vFOV is the only value a capture may use)
            foreach (KeyShot k in BulwarkConst.KeyShots)
                Assert.AreEqual(BulwarkSchedule.Vfov(k.s), k.fov, 0.05, k.tag + " fov vs vFOV(s)");
        }

        [Test]
        public void KeyShots_FramingVariantsAt1610And219()
        {
            var plan = ShotPlan.Build("keyshots").stills;
            foreach (KeyShot k in BulwarkConst.KeyShots)
            {
                StillShot a = FindStill(plan, ShotPlan.KeyTag(k) + "_1610"), b = FindStill(plan, ShotPlan.KeyTag(k) + "_219");
                Assert.IsNotNull(a); Assert.IsNotNull(b);
                Assert.AreEqual(1680, a.width); Assert.AreEqual(1050, a.height);
                Assert.AreEqual(2560, b.width); Assert.AreEqual(1080, b.height);
                Assert.AreEqual(k.fov, a.fov, 1e-9, "vertical FOV kept (Hor+)");
            }
        }

        [Test]
        public void KeyShots_TagBeatMatchesTheBeatOfS()
        {
            foreach (KeyShot k in BulwarkConst.KeyShots)
            {
                int beat = BwPath.BeatAt(k.s);
                StringAssert.StartsWith(ShotPlan.BeatPrefix(beat - 1) + "_", ShotPlan.KeyTag(k));
            }
        }

        [Test]
        public void Tags_FollowBeatNameTagPattern()
        {
            var rx = new Regex("^b(0[1-7])_([A-Za-z]+)_([A-Za-z0-9_]+)$");
            var plan = ShotPlan.Build("keyshots,beats");
            foreach (StillShot st in plan.stills)
            {
                Match m = rx.Match(st.tag);
                Assert.IsTrue(m.Success, "tag " + st.tag + " is not bNN_<Name>_<tag>");
                int nn = int.Parse(m.Groups[1].Value);
                Assert.AreEqual(FrameworkConst.BeatNames[nn - 1], m.Groups[2].Value, st.tag);
                Assert.AreEqual(st.tag + ".png", st.File);
            }
        }

        [Test]
        public void BeatStills_StartMidEndAndFreeLook()
        {
            var plan = ShotPlan.Build("beats").stills;
            Assert.AreEqual(7 * 8 + 1, plan.Count, "start, mid, end, 5 free looks per beat, beat 3 also pitch -90");
            var b = FrameworkConst.BeatBoundaries;
            for (int i = 0; i < FrameworkConst.BeatCount; i++)
            {
                string pre = ShotPlan.BeatPrefix(i);
                StillShot start = FindStill(plan, pre + "_start"), mid = FindStill(plan, pre + "_mid"), end = FindStill(plan, pre + "_end");
                Assert.IsNotNull(start); Assert.IsNotNull(mid); Assert.IsNotNull(end);
                Assert.AreEqual(b[i] + 2, start.s, 1e-9);
                Assert.AreEqual(0.5 * (b[i] + b[i + 1]), mid.s, 1e-9);
                Assert.AreEqual(b[i + 1] - 2, end.s, 1e-9);
                double heading = BwPath.HeadingDeg(mid.s);
                Assert.AreEqual(BwMath.WrapDeg(heading + 90), FindStill(plan, pre + "_look_r").yaw, 1e-9);
                Assert.AreEqual(BwMath.WrapDeg(heading - 90), FindStill(plan, pre + "_look_l").yaw, 1e-9);
                Assert.AreEqual(60.0, FindStill(plan, pre + "_look_up").pitch, 1e-9);
                Assert.AreEqual(-60.0, FindStill(plan, pre + "_look_down").pitch, 1e-9);
                foreach (StillShot st in new[] { start, mid, end })
                {
                    SequenceSpec.EyeAt(st.s, out double x, out double y, out double z);
                    Assert.AreEqual(x, st.x, 1e-9); Assert.AreEqual(z, st.z, 1e-9); Assert.AreEqual(y, st.y, 1e-9);
                    Assert.AreEqual(BulwarkSchedule.Vfov(st.s), st.fov, 1e-9);
                    Assert.AreEqual(1920, st.width); Assert.AreEqual(1080, st.height);
                }
            }
            Assert.IsNotNull(FindStill(plan, ShotPlan.BeatPrefix(2) + "_look_down90"));
        }

        [Test]
        public void Pose_TokenParsesAndClampsBeforeTheGoal()
        {
            Assert.IsTrue(ShotPlan.TryPose("pose:test:-9.2:-88.3:1499:0:-5:0", out StillShot s, out string err), err);
            Assert.LessOrEqual(s.s, ShotPlan.MaxPoseS, "the goal trigger never fires");
            Assert.AreEqual("custom", s.set);
            Assert.AreEqual(BulwarkSchedule.Vfov(s.s), s.fov, 1e-9, "fov 0 = schedule vFOV");
            Assert.IsFalse(ShotPlan.TryPose("pose:bad:1:2", out _, out _));
            // the s of a free pose comes from its position over the whole path (the ledge, not the crest above it)
            Assert.IsTrue(ShotPlan.TryPose("pose:ledge:-9.2:-88.3:1449.1:-27.5:-7:0", out StillShot l, out err), err);
            Assert.AreEqual(1452.0, l.s, 0.05, "KS20 eye resolves to s 1452");
            Assert.IsTrue(ShotPlan.TryPose("pose:crest:4.2:1.7:160:80:-8:0", out StillShot c, out err), err);
            Assert.AreEqual(160.0, c.s, 0.05, "KS04 eye resolves to s 160");
            Assert.IsTrue(ShotPlan.TryPose("pose:stairs:1.11:-25.3:985:3:-24:0", out StillShot st2, out err), err);
            Assert.AreEqual(985.0, st2.s, 0.6, "KS14 eye resolves to the stairs");
        }

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

        [Test]
        public void Plan_SetNamesAndFilters()
        {
            var p = ShotPlan.Build("stills,bogus");
            Assert.IsNotNull(p.parity);
            Assert.AreEqual(0, p.sequences.Count);
            CollectionAssert.Contains(p.unknownSets, "bogus");
            var all = ShotPlan.Build("all");
            Assert.IsFalse(all.sequences.Exists(s => s.set == "encode"), "encode clips only with 'encode' or 'everything'");
            var only = ShotPlan.Build("keyshots,sequences", new CapturePlanOptions { only = new[] { "KS04", "seq_static_b02" } });
            Assert.IsTrue(only.stills.TrueForAll(s => s.tag.Contains("KS04")));
            // the filter keeps every sequence whose name contains one of the strings: seq_static_b02 and the KS04 statics / pan
            Assert.IsTrue(only.sequences.TrueForAll(q => q.name.Contains("KS04") || q.name.StartsWith("seq_static_b02", StringComparison.Ordinal)));
            Assert.AreEqual(1, only.sequences.FindAll(q => q.name.StartsWith("seq_static_b02", StringComparison.Ordinal)).Count);
            CollectionAssert.IsSubsetOf(new[] { "seq_static_KS04", "seq_plumes_KS04", "seq_pan_KS04_sea" }, only.sequences.ConvertAll(q => q.name));
        }

        [Test]
        public void QaRigs_RegisteredSetExpandsIntoItsShotsAndNeverIntoAll()
        {
            const string set = "testrig";
            QaRigs.Unregister(set);
            try
            {
                Assert.IsFalse(QaRigs.Requested("QA/x", "keyshots,all", set), "a rig set is never part of all");
                Assert.IsTrue(QaRigs.Requested("QA/x", "beats, TestRig", set), "names are trimmed and case-insensitive");
                Assert.IsFalse(QaRigs.Requested(null, set, set), "only capture runs build a rig");
                CollectionAssert.Contains(ShotPlan.Build(set).unknownSets, set, "unregistered: an unknown set (a capture failure)");
                Assert.IsFalse(QaRigs.Register("materials", null, null), "built-in set names are refused");
                Assert.IsFalse(QaRigs.Register("a,b", null, null));

                var still = new StillShot { tag = "rig_turntable_000", variant = "1080", width = 1920, height = 1080, s = 300, x = 50, y = 20, z = 300, yaw = 90, fov = 50, ambient = 12.5 };
                Assert.IsTrue(QaRigs.Register(set, () => new List<StillShot> { still },
                    (w, h) => new List<SequenceSpec> { new SequenceSpec { name = "seq_rig_orbit", mode = SeqMode.MovingPose, fps = 30, frames = 60, width = w, height = h } }));
                var plan = ShotPlan.Build("beats," + set, new CapturePlanOptions { seqWidth = 1280, seqHeight = 720 });
                Assert.AreEqual(0, plan.unknownSets.Count);
                CollectionAssert.Contains(plan.sets, set);
                StillShot got = FindStill(plan.stills, "rig_turntable_000");
                Assert.IsNotNull(got);
                Assert.AreEqual(set, got.set, "the rig's stills carry the set name");
                Assert.AreEqual(12.5, got.ambient, "the rig's ambient clock (animation phase) is kept");
                SequenceSpec sq = plan.sequences.Find(q => q.name == "seq_rig_orbit");
                Assert.IsNotNull(sq);
                Assert.AreEqual(1280, sq.width, "rig sequences take the plan's sequence resolution");
                Assert.AreEqual(set, sq.set);
                Assert.IsFalse(ShotPlan.Build("everything").stills.Exists(s => s.tag == "rig_turntable_000"), "never part of everything");
                var only = ShotPlan.Build(set, new CapturePlanOptions { only = new[] { "orbit" } });
                Assert.AreEqual(0, only.stills.Count, "-capture-only filters rig shots like every other set");
                Assert.AreEqual(1, only.sequences.Count);

                QaRigs.SetCurrent("rig_turntable_000", 0);
                Assert.AreEqual("rig_turntable_000", QaRigs.CurrentShot);
                QaRigs.SetCurrent(null, 0);
                Assert.AreEqual("", QaRigs.CurrentShot);
            }
            finally { QaRigs.Unregister(set); }
        }

        // ------------------------------------------------------------------ capture plan: sequences

        static SequenceSpec FindSeq(List<SequenceSpec> list, string name)
        {
            for (int i = 0; i < list.Count; i++) if (list[i].name == name) return list[i];
            return null;
        }

        [Test]
        public void Sequences_EveryBoundaryCoveredForAtLeast20s()
        {
            var seqs = ShotPlan.Build("transitions").sequences;
            var b = FrameworkConst.BeatBoundaries;
            for (int i = 1; i < FrameworkConst.BeatCount; i++)
            {
                bool covered = seqs.Exists(sq => sq.s0 <= b[i] - 20 && sq.s1 >= b[i] + 20 && sq.Seconds >= 20.0 - 1e-9);
                Assert.IsTrue(covered, "boundary s " + b[i]);
            }
            for (int k = 0; k < ShotPlan.ExtraWindows.GetLength(0); k++)
                Assert.IsTrue(seqs.Exists(sq => sq.s0 <= ShotPlan.ExtraWindows[k, 0] && sq.s1 >= ShotPlan.ExtraWindows[k, 1]), "window " + k);
        }

        [Test]
        public void Sequences_WalkPoseFollowsTheCinematicLane()
        {
            var sq = ShotPlan.WalkSeq("t", "transitions", "", 100, 200, 30, 1920, 1080);
            Assert.AreEqual(FrameworkConst.CinematicSpeed, sq.speed, 0.0, "sequences walk at the cinematic 2.5 m/s");
            for (int n = 0; n < sq.frames; n += 17)
            {
                sq.FramePose(n, out double s, out double x, out double y, out double z, out double yaw, out double pitch, out double fov);
                Assert.AreEqual(Math.Min(BwPath.Length, 100 + 2.5 * n / 30.0), s, 1e-9);
                SequenceSpec.EyeAt(s, out double ex, out double ey, out double ez);
                Assert.AreEqual(ex, x, 1e-9); Assert.AreEqual(ey, y, 1e-9); Assert.AreEqual(ez, z, 1e-9);
                Assert.AreEqual(BwPath.HeadingDeg(s), yaw, 1e-9);
                Assert.AreEqual(BulwarkSchedule.Vfov(s), fov, 1e-9);
            }
        }

        [Test]
        public void Sequences_StaticClipsPerBeat()
        {
            var seqs = ShotPlan.Build("statics").sequences;
            Assert.AreEqual(FrameworkConst.BeatCount, seqs.FindAll(sq => sq.name.StartsWith("seq_static_b", StringComparison.Ordinal)).Count, "one per beat");
            foreach (var sq in seqs)
            {
                Assert.AreEqual(SeqMode.Static, sq.mode, sq.name);
                if (sq.name == "seq_plumes_KS04") { Assert.AreEqual(ShotPlan.PlumeCountFps, sq.fps); Assert.AreEqual(60.0, (sq.frames - 1) / (double)sq.fps, 1e-9, "60 s plume count"); }
                else Assert.AreEqual(ShotPlan.StaticFrames, sq.frames, sq.name);
            }
            foreach (var id in new[] { "seq_static_KS04", "seq_static_KS08", "seq_static_KS20" })
            {
                var sq = FindSeq(seqs, id);
                Assert.IsNotNull(sq, id);
                var k = BulwarkConst.KeyShots[ShotPlan.KeyShotIndex(id.Substring(id.Length - 4))];
                Assert.AreEqual(k.s, sq.s0, 1e-9, id + " at its key shot");
                Assert.IsTrue(sq.advanceAmbient, id + ": the clock runs (flicker test)");
            }
        }

        [Test]
        public void Sequences_HalfPixelPansAndTheSlowRailPan()
        {
            var seqs = ShotPlan.Build("pans").sequences;
            foreach (var sq in seqs)
            {
                double px = sq.name == "seq_pan_rail_slow" ? ShotPlan.SlowPanPx : ShotPlan.PanPx;
                Assert.AreEqual(QaProjection.PanStepDeg(px, sq.fov, sq.height), sq.panStepDeg, 1e-12, sq.name);
            }
            Assert.IsNotNull(FindSeq(seqs, "seq_pan_rail_slow"));
            foreach (var id in new[] { "seq_pan_KS04_sea", "seq_pan_KS09_p58", "seq_pan_KS09_p90", "seq_pan_KS20", "seq_pan_s1470_city" })
            {
                var sq = FindSeq(seqs, id);
                Assert.IsNotNull(sq, id);
                Assert.IsFalse(sq.advanceAmbient, id + ": the ambient clock is frozen in a pan (14.10.3)");
            }
            Assert.AreEqual(-58.0, FindSeq(seqs, "seq_pan_KS09_p58").pitch, 1e-9);
            Assert.Less(FindSeq(seqs, "seq_pan_KS09_p90").pitch, -89.0);
            Assert.AreEqual(ShotPlan.CityLightS, FindSeq(seqs, "seq_pan_s1470_city").s0, 1e-9);
        }

        [Test]
        public void Sequences_StartFirstAndEndingLast()
        {
            var seqs = ShotPlan.Build("sequences").sequences;
            Assert.AreEqual("seq_start", seqs[0].name);
            var end = seqs[seqs.Count - 1];
            Assert.AreEqual("seq_end", end.name);
            Assert.GreaterOrEqual(end.Seconds, FrameworkConst.EndTotal + 2.0, "covers the 55.5 s ending");
        }

        [Test]
        public void Encode_TenSecondsAt60FpsAnd4kOnRequest()
        {
            var a = ShotPlan.Build("encode").sequences;
            var b = ShotPlan.Build("encode", new CapturePlanOptions { encode4k = true }).sequences;
            Assert.Greater(a.Count, 0);
            Assert.AreEqual(2 * a.Count, b.Count);
            foreach (var sq in b)
            {
                Assert.AreEqual(ShotPlan.EncodeFps, sq.fps);
                Assert.AreEqual(ShotPlan.EncodeSeconds, sq.Seconds, 1e-9);
                if (sq.name.EndsWith("_4k", StringComparison.Ordinal)) { Assert.AreEqual(3840, sq.width); Assert.AreEqual(2160, sq.height); }
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
            double expectLuma = (CaptureMetrics.WR * (255 + 255 + 0 + 128) + CaptureMetrics.WG * (0 + 255 + 0 + 128) + CaptureMetrics.WB * (255 + 255 + 0 + 128)) / 4.0;   // 0..255 (14.10.3)
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
        public void AutoTest_ExpectedTimesOf14_10_4()
        {
            // computed from the path and the speed (ADDENDUM A1 00 ending, fix wave R1): 1540 m of walk-equivalent s / v + 143.5 s
            Assert.AreEqual(1170.1667, AutoTestRunner.ExpectedTrigger((float)FrameworkConst.WalkSpeed), 0.01);
            Assert.AreEqual(4.0, AutoTestRunner.ToleranceSeconds((float)FrameworkConst.WalkSpeed));
            Assert.AreEqual(759.5, AutoTestRunner.ExpectedTrigger((float)FrameworkConst.CinematicSpeed), 0.01);
            Assert.AreEqual(3.0, AutoTestRunner.ToleranceSeconds((float)FrameworkConst.CinematicSpeed));
            Assert.AreEqual(205.10, AutoTestRunner.ExpectedTrigger(25f), 0.01, "the -speed 25 smoke walk");
            Assert.AreEqual(55.5, FrameworkConst.EndTotal, 1e-12, "ADDENDUM A1 3.10 ending, the look held to t_g 40 for the stinger");
            Assert.AreEqual(0.01, FrameworkConst.RollTolDeg, 0.0, "14.10.4 roll 0.00 +- 0.01 deg");
        }

        /// <summary>D-06: BwLog.Problem (editor) logs the [BW-FAIL] tag that unity-queue.ps1 counts, never "[BW] ERROR".</summary>
        [Test]
        public void BwLog_ProblemCarriesTheFailTag()
        {
            // the line goes to a capturing sink, never into the test log (a passing run logs 0 [BW-FAIL] lines)
            Assert.AreEqual("[BW-FAIL] ", Megastructure.EditorTools.BwLog.FailPrefix);
            Assert.AreEqual("[BW-FAIL] unit test problem", Megastructure.EditorTools.BwLog.FormatFail("unit test problem"));
            var problems = new List<string>();
            var lines = new List<string>();
            var sink = Megastructure.EditorTools.BwLog.ErrorSink;
            try
            {
                Megastructure.EditorTools.BwLog.ErrorSink = lines.Add;
                Megastructure.EditorTools.BwLog.Problem(problems, "unit test problem");
                Megastructure.EditorTools.BwLog.Fail("unit test fail");
            }
            finally { Megastructure.EditorTools.BwLog.ErrorSink = sink; }
            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual("unit test problem", problems[0]);
            CollectionAssert.AreEqual(new[] { "[BW-FAIL] unit test problem", "[BW-FAIL] unit test fail" }, lines);
        }

        [Test]
        public void QaMode_PrecedenceAndSpeed()
        {
            try
            {
                CliArgs.Override(new[] { "Bulwark.exe", "-autotest", "-wallbot" });
                Assert.AreEqual(QaRunMode.WallBot, QaMode.Resolve());
                CliArgs.Override(new[] { "Bulwark.exe", "-autotest", "-capture", "x" });
                Assert.AreEqual(QaRunMode.Capture, QaMode.Resolve());
                CliArgs.Override(new[] { "Bulwark.exe", "-soak", "120", "-autotest" });
                Assert.AreEqual(QaRunMode.Soak, QaMode.Resolve());
                CliArgs.Override(new[] { "Bulwark.exe", "-autotest", "-brisk" });
                Assert.AreEqual(QaRunMode.AutoTest, QaMode.Resolve());
                Assert.AreEqual(FrameworkConst.BriskSpeed, QaMode.DriveSpeed, 1e-6);
                Assert.AreEqual("brisk", QaMode.SpeedName(QaMode.DriveSpeed));
                CliArgs.Override(new[] { "Bulwark.exe", "-autotest", "-cinematic", "-speed", "1.5" });
                Assert.AreEqual(FrameworkConst.CinematicSpeed, QaMode.DriveSpeed, 1e-6, "-speed never changes the cinematic (D-02)");
                Assert.AreEqual("cinematic", QaMode.SpeedName(QaMode.DriveSpeed));
                CliArgs.Override(new[] { "Bulwark.exe" });
                Assert.AreEqual(QaRunMode.None, QaMode.Resolve());
                Assert.AreEqual("walk", QaMode.SpeedName(QaMode.DriveSpeed));
            }
            finally { CliArgs.Override(null); }
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
    }
}
