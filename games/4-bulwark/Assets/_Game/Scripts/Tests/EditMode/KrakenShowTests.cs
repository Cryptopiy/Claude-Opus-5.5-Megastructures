// KrakenShowTests: the kraken's choreography over the walk (Addendum A1 1.4-1.6, 4.8 T-KRAKEN; LEAD DECISIONS 2 and 3; show
// owner). Pure choreography (KrakenDirectorChoreo) driven through the director's effective-progress model
// (KrakenDirector.StepEffective) at 1.5 and 2.5 m/s, plus the real rig with the hero arms (KrakenDirectorArms) for the
// geometry numbers; writes QA/reports/kraken_show.json (measured numbers for the report).
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Megastructure.Creature;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class KrakenShowTests
    {
        const double Dt = 1.0 / 60.0;

        /// <summary>Runs the director's sEff model at a walking speed and records the root at every 0.5 m of s.</summary>
        static Dictionary<int, KrakenShowPose> Run(double speed, double s0, double s1)
        {
            var map = new Dictionary<int, KrakenShowPose>();
            double sEff = double.NaN, s = s0, t = 1000.0;
            int nextKey = (int)System.Math.Ceiling(s0 * 2.0);
            while (s <= s1)
            {
                sEff = KrakenDirector.StepEffective(sEff, s, Dt);
                if (s * 2.0 >= nextKey)
                {
                    var p = new KrakenShowPose();
                    KrakenDirectorChoreo.Evaluate(sEff, t, -1.0, p);
                    map[nextKey] = p;
                    nextKey++;
                }
                s += speed * Dt; t += Dt;
            }
            return map;
        }

        [Test]
        public void Root_IsTheSameAt15And25MetresPerSecond()
        {
            var a = Run(1.5, 0, 1060);
            var b = Run(2.5, 0, 1060);
            int n = 0;
            foreach (var kv in b)
            {
                if (!a.TryGetValue(kv.Key, out var pa)) continue;
                var pb = kv.Value;
                Assert.Less(Vector3.Distance(pa.Root, pb.Root), 1.0f, $"root at s {kv.Key / 2.0}");
                Assert.Less(Mathf.Abs(Mathf.DeltaAngle(pa.Heading, pb.Heading)), 1.0f, $"heading at s {kv.Key / 2.0}");
                n++;
            }
            Assert.Greater(n, 1500);
        }

        [Test]
        public void Root_NeverStepsMoreThanHalfAMetrePerFrame()
        {
            foreach (double v in new[] { 1.5, 2.5 })
            {
                var p = new KrakenShowPose();
                Vector3 prev = Vector3.zero; bool has = false;
                double maxStep = 0, at = 0;
                for (double s = 0; s < 1060; s += v * Dt)
                {
                    KrakenDirectorChoreo.Evaluate(s, s / v, -1.0, p);
                    if (has) { double d = Vector3.Distance(prev, p.Root); if (d > maxStep) { maxStep = d; at = s; } }
                    prev = p.Root; has = true;
                }
                Assert.LessOrEqual(maxStep, 0.5, $"max root step {maxStep:F3} m per 1/60 s at s {at:F1} ({v} m/s)");
            }
        }

        [Test]
        public void Lingering_FinishesTheAppearanceThenWaits()
        {
            // walk at 2.5 m/s to s 160 (K1 hold), then stop for 120 s: K1 completes at 1.5 m/s and the kraken waits at the
            // segment end (214: the head 72 m deep), never starting K2 before the player arrives
            double sEff = double.NaN, s = 0;
            while (s < 160) { sEff = KrakenDirector.StepEffective(sEff, s, Dt); s += 2.5 * Dt; }
            for (int i = 0; i < 120 * 60; i++) sEff = KrakenDirector.StepEffective(sEff, s, Dt);
            Assert.AreEqual(KrakenDirectorChoreo.SegmentEnds[1], sEff, 1e-6);
            var pw = new KrakenShowPose();
            KrakenDirectorChoreo.Evaluate(sEff, 0, -1, pw);
            Assert.LessOrEqual(pw.HeadTopAboveSea, -40f, "waiting deep");
            // a player who stops while the K2 arm sinks sees it go under, then the kraken waits (no frozen arm above the sea)
            KrakenDirectorChoreo.Evaluate(KrakenDirectorChoreo.SegmentEnds[3], 0, -1, pw);
            for (int i = 0; i < pw.ArmCount; i++)
                Assert.Less(pw.Arms[i].P[pw.Arms[i].AnchorIndex].y, KrakenDirectorChoreo.SeaY, "K2 arm under at the segment end");
            // in the interiors: progress only
            double sIn = KrakenDirector.StepEffective(1000.0, 1000.0, 60.0);
            Assert.AreEqual(1000.0, sIn, 1e-9);
            // never backwards (latch)
            Assert.AreEqual(300.0, KrakenDirector.StepEffective(300.0, 250.0, Dt), 0.03);
        }

        [Test]
        public void Body_KeepsTheCorridorClearance()
        {
            // A1 1.4: in beats 2-4 the body (head extents of the model, any heading) stays >= 140 m east of x 6.0;
            // the hero-arm waypoints above the sea too
            var p = new KrakenShowPose();
            for (double s = 100; s <= 920; s += 0.5)
            {
                KrakenDirectorChoreo.Evaluate(s, s / 2.5, -1.0, p);
                if (!p.Visible) continue;
                var rot = Quaternion.Euler(0f, p.Heading, 0f);
                float minX = float.MaxValue;
                foreach (var c in new[] { new Vector3(-97f, 0f, -220f), new Vector3(97f, 0f, -220f), new Vector3(-97f, 0f, 107f), new Vector3(97f, 0f, 107f) })
                    minX = Mathf.Min(minX, (p.Root + rot * c).x);
                Assert.GreaterOrEqual(minX, KrakenDirectorChoreo.CorridorX, $"head at s {s}");
                for (int i = 0; i < p.ArmCount; i++)
                    for (int j = 0; j < p.Arms[i].Count; j++)
                        if (p.Arms[i].P[j].y > KrakenDirectorChoreo.SeaY - 1f)
                            Assert.GreaterOrEqual(p.Arms[i].P[j].x, KrakenDirectorChoreo.CorridorX, $"arm {i} waypoint {j} at s {s}");
            }
        }

        [Test]
        public void Body_StaysOutOfTheReefExceptTheChannel()
        {
            var p = new KrakenShowPose();
            for (double s = 452; s <= 920; s += 0.5)
            {
                KrakenDirectorChoreo.Evaluate(s, s / 2.5, -1.0, p);
                float x = p.Root.x, z = p.Root.z;
                bool reef = z >= BulwarkConst.ReefZ0 && z <= BulwarkConst.ReefZ1 && x < BulwarkConst.ReefX1;
                bool channel = x >= 900f && x <= 1100f;
                Assert.IsFalse(reef && !channel, $"crown in the reef at s {s}: ({x:F0}, {z:F0})");
            }
        }

        [Test]
        public void Bus_HasAtMostEightValidEntries()
        {
            var p = new KrakenShowPose();
            for (double s = 0; s <= 1100; s += 0.25)
            {
                KrakenDirectorChoreo.Evaluate(s, s / 2.5, -1.0, p);
                Assert.LessOrEqual(p.DistCount, SeaDisturbances.Max);
                for (int i = 0; i < p.DistCount; i++)
                {
                    var d = p.Dist[i];
                    Assert.IsTrue(d.Strength > 0f && d.Strength <= 1f, $"strength at s {s}");
                    Assert.IsTrue(d.HalfAlong > 0f && d.HalfAcross > 0f, $"extent at s {s}");
                    Assert.IsTrue(float.IsFinite(d.X) && float.IsFinite(d.Z) && float.IsFinite(d.Heading), $"finite at s {s}");
                }
            }
            // the published windows: K1 breach, K3 shadow + wake, K4 breach, K5 slap
            KrakenDirectorChoreo.Evaluate(160, 64, -1, p); Assert.IsTrue(Has(p, SeaDisturbanceKind.Breach), "K1 breach");
            KrakenDirectorChoreo.Evaluate(492, 197, -1, p); Assert.IsTrue(Has(p, SeaDisturbanceKind.Shadow) && Has(p, SeaDisturbanceKind.Wake), "K3 shadow + wake");
            KrakenDirectorChoreo.Evaluate(800, 320, -1, p); Assert.IsTrue(Has(p, SeaDisturbanceKind.Breach), "K4 breach");
            KrakenDirectorChoreo.Evaluate(823, 329, -1, p); Assert.IsTrue(Has(p, SeaDisturbanceKind.Slap), "K5 slap");
            KrakenDirectorChoreo.Evaluate(237, 95, -1, p); Assert.IsTrue(Has(p, SeaDisturbanceKind.Slap), "Kf slap");
        }

        static bool Has(KrakenShowPose p, SeaDisturbanceKind k) { for (int i = 0; i < p.DistCount; i++) if (p.Dist[i].Kind == k) return true; return false; }

        /// <summary>First s >= s0 where something breaks the surface (head top or an arm anchor above the sea).</summary>
        static double FirstSight(double s0, double s1)
        {
            var p = new KrakenShowPose();
            for (double s = s0; s < s1; s += 0.1)
            {
                KrakenDirectorChoreo.Evaluate(s, s / 2.5, -1.0, p);
                if (p.HeadTopAboveSea > 0.5f) return s;
                for (int i = 0; i < p.ArmCount; i++)
                    if (p.Arms[i].Count > 0 && p.Arms[i].P[p.Arms[i].AnchorIndex].y > KrakenDirectorChoreo.SeaY + 0.5f) return s;
            }
            return double.NaN;
        }

        [Test]
        public void Calls_PrecedeTheSightByTwoToSixSeconds()
        {
            // LEAD DECISION 3: sound may precede sight by 2-6 s; checked at both speeds (s rate 2.5 and 1.5)
            foreach (var id in new[] { "K1_voice", "K2_voice", "K4_voice" })
            {
                KrakenCallCue cue = default; bool found = false;
                foreach (var c in KrakenDirectorChoreo.Calls) if (c.Id == id) { cue = c; found = true; }
                Assert.IsTrue(found, id);
                double sight = FirstSight(cue.S, cue.S + 60);
                Assert.IsFalse(double.IsNaN(sight), id + " sight");
                foreach (double v in new[] { 1.5, 2.5 })
                {
                    double lead = (sight - cue.S) / v;
                    Assert.That(lead, Is.InRange(2.0, 6.0), $"{id}: sound {cue.S}, sight {sight:F1}, lead {lead:F2} s at {v} m/s");
                }
            }
            // ordered by s (the director fires them in order)
            for (int i = 1; i < KrakenDirectorChoreo.Calls.Length; i++)
                Assert.LessOrEqual(KrakenDirectorChoreo.Calls[i - 1].S, KrakenDirectorChoreo.Calls[i].S, "ordered: " + KrakenDirectorChoreo.Calls[i].Id);
        }

        /// <summary>Something of the kraken stands above the sea at sEff s (a head top, or any hero-arm waypoint).</summary>
        static bool AboveSea(KrakenShowPose p)
        {
            if (p.HeadTopAboveSea > 0f) return true;
            for (int i = 0; i < p.ArmCount; i++)
                if (p.Arms[i].Weight > 0.5f)
                    for (int j = 0; j < p.Arms[i].Count; j++) if (p.Arms[i].P[j].y > KrakenDirectorChoreo.SeaY + 0.5f) return true;
            return false;
        }

        [Test]
        public void Calls_SoundCoversEveryVisibleSpan()
        {
            // fix wave S2-2: at 1.5 and 2.5 m/s (sEff advances at the walking speed; the director publishes CueDuration at the
            // measured s rate) some kraken sound plays at every s where a part of the kraken stands above the sea
            var p = new KrakenShowPose();
            var fails = new List<string>();
            var sb = new StringBuilder("{\n");
            foreach (double v in new[] { 1.5, 2.5 })
            {
                double gapS0 = double.NaN; int visible = 0;
                for (double s = 100; s < KrakenDirectorChoreo.InteriorS; s += 0.5)
                {
                    KrakenDirectorChoreo.Evaluate(s, s / v, -1.0, p);
                    if (!AboveSea(p)) continue;
                    visible++;
                    bool covered = false;
                    foreach (var c in KrakenDirectorChoreo.Calls)
                        if (c.S <= s && s <= c.S + KrakenDirectorChoreo.CueDuration(in c, v) * v) { covered = true; break; }
                    if (!covered) { if (double.IsNaN(gapS0)) gapS0 = s; }
                    else if (!double.IsNaN(gapS0)) { fails.Add($"{v} m/s: silent while visible over s {gapS0:F1}-{s:F1} ({p.Appearance})"); gapS0 = double.NaN; }
                }
                if (!double.IsNaN(gapS0)) fails.Add($"{v} m/s: silent while visible from s {gapS0:F1}");
                sb.Append("  \"visibleSamples_" + F(v) + "\": " + visible + ",\n");
            }
            foreach (var c in KrakenDirectorChoreo.Calls)
                sb.Append("  \"" + c.Id + "\": { \"s\": " + F(c.S) + ", \"sEnd\": " + F(c.SEnd) + ", \"dur25\": " + F(KrakenDirectorChoreo.CueDuration(in c, 2.5))
                          + ", \"dur15\": " + F(KrakenDirectorChoreo.CueDuration(in c, 1.5)) + " },\n");
            sb.Append("  \"ok\": " + (fails.Count == 0 ? "true" : "false") + "\n}\n");
            Directory.CreateDirectory("QA/reports");
            File.WriteAllText("QA/reports/kraken_calls.json", sb.ToString());
            Debug.Log("[BW] kraken calls coverage: " + sb);
            // every stretched cue ends at or after its SEnd at both rates
            foreach (var c in KrakenDirectorChoreo.Calls)
                foreach (double v in new[] { 1.5, 2.5 })
                    if (c.SEnd > c.S && c.S + KrakenDirectorChoreo.CueDuration(in c, v) * v < c.SEnd) fails.Add($"{c.Id} ends before its SEnd at {v} m/s");
            Assert.IsEmpty(fails, string.Join("; ", fails));
        }

        [Test]
        public void Choreography_IsPureAndDeterministic()
        {
            var a = new KrakenShowPose(); var b = new KrakenShowPose();
            for (double s = 0; s < 1100; s += 3.7)
            {
                KrakenDirectorChoreo.Evaluate(s, 1234.5 + s, -1, a);
                KrakenDirectorChoreo.Evaluate(s, 1234.5 + s, -1, b);
                Assert.AreEqual(a.Root, b.Root); Assert.AreEqual(a.ArmCount, b.ArmCount); Assert.AreEqual(a.DistCount, b.DistCount);
            }
            KrakenDirectorChoreo.Evaluate(0, 0, 20.0, a);
            Assert.AreEqual("stinger", a.Appearance);
            Assert.AreEqual(3, a.ArmCount);
        }

        // ------------------------------------------------------------------ the real rig: hero arm geometry and speeds
        static KrakenAssets Assets()
        {
            var a = KrakenAssets.Load();
            Assert.IsTrue(a.Ok, "kraken assets: " + a.Error);
            return a;
        }

        sealed class Probe
        {
            public KrakenRig Rig;
            public KrakenDirectorArms Arms = new KrakenDirectorArms();
            public KrakenShowPose Pose = new KrakenShowPose();
            public void At(double s, double tg = -1.0, double speed = 2.5)
            {
                KrakenDirectorChoreo.Evaluate(s, s / speed, tg, Pose);
                Rig.ResetInputs();
                Rig.SetPresetWeight(KrakenPreset.Lurk, Pose.Lurk);
                Rig.SetPresetWeight(KrakenPreset.Swim, Pose.Swim);
                Rig.Scale = Pose.Scale;
                Rig.SetRoot(Pose.Root, Pose.Heading, Pose.Pitch);
                Rig.Evaluate(s / speed);
                for (int i = 0; i < Pose.ArmCount; i++) Arms.Apply(Rig, Pose.Arms[i]);
                KrakenDirector.ClampCorridor(Rig, tg < 0 && s < 920);
            }
            public Vector3 W(int limb, int i) => Rig.LocalToWorld.MultiplyPoint3x4(Rig.Limbs[limb].CurP[i]);
            /// <summary>Emerged run of a limb: waterline crossing (first rising), its radius and the top above the sea.</summary>
            public bool Emerged(int limb, out Vector3 at, out float radius, out float top, out float topX)
            {
                var L = Rig.Limbs[limb]; at = Vector3.zero; radius = 0; top = 0; topX = 0;
                float sea = KrakenDirectorChoreo.SeaY;
                bool found = false;
                for (int i = 0; i < L.K - 1; i++)
                {
                    Vector3 a = W(limb, i), b = W(limb, i + 1);
                    if (!found && a.y < sea && b.y >= sea) { float f = (sea - a.y) / (b.y - a.y); at = Vector3.Lerp(a, b, f); radius = Mathf.Lerp(L.Radius[i], L.Radius[i + 1], f); found = true; }
                    if (b.y - sea > top) { top = b.y - sea; topX = b.x; }
                }
                return found;
            }
        }

        static string F(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        [Test]
        public void HeroArms_MatchTheChoreographyAndMoveSlowly()
        {
            var probe = new Probe { Rig = new KrakenRig(Assets().Manifest) };
            var fails = new List<string>();
            var sb = new StringBuilder();
            sb.Append("{\n");
            // K2 at full height (s 386): the arm leaves the sea near (330, 690), apex ~180 m
            probe.At(386);
            bool em2 = probe.Emerged(KrakenDirectorChoreo.K2Limb, out Vector3 e2, out float r2, out float top2, out _);
            sb.Append("  \"K2\": { \"s\": 386, \"exit\": [" + F(e2.x) + ", " + F(e2.z) + "], \"waterlineDiameter\": " + F(2 * r2) + ", \"top\": " + F(top2)
                      + ", \"anchorError\": " + F(probe.Arms.LastAnchorError) + ", \"stretch\": " + F(probe.Arms.LastStretch) + ", \"dip\": " + F(probe.Arms.LastDip) + " },\n");
            if (!em2) fails.Add("K2 arm does not emerge");
            if (Vector2.Distance(new Vector2(e2.x, e2.z), new Vector2(330f, 690f)) > 20f) fails.Add($"K2 exit ({e2.x:F0}, {e2.z:F0}) not near (330, 690)");
            if (top2 < 160f || top2 > 200f) fails.Add($"K2 height {top2:F1}");
            // K4 drape over islet (260, 830) at s 812
            probe.At(812);
            bool em4 = probe.Emerged(KrakenDirectorChoreo.K4Limb, out Vector3 e4, out float r4, out float top4, out _);
            sb.Append("  \"K4drape\": { \"s\": 812, \"exit\": [" + F(e4.x) + ", " + F(e4.z) + "], \"waterlineDiameter\": " + F(2 * r4) + ", \"top\": " + F(top4)
                      + ", \"anchorError\": " + F(probe.Arms.LastAnchorError) + ", \"stretch\": " + F(probe.Arms.LastStretch) + " },\n");
            if (!em4) fails.Add("K4 drape does not emerge");
            if (top4 < 60f || top4 > 100f) fails.Add($"K4 drape apex {top4:F1}");
            // stinger columns (tg 30)
            probe.At(0, 30.0);
            probe.Emerged(2, out _, out float rs, out float tops, out float topsX);
            sb.Append("  \"stinger\": { \"tg\": 30, \"mainTop\": " + F(tops) + ", \"mainTopX\": " + F(topsX) + ", \"waterlineDiameter\": " + F(2 * rs)
                      + ", \"stretch\": " + F(probe.Arms.LastStretch) + " },\n");

            // speeds of every emerged control point per appearance window at the cinematic 2.5 m/s and at 1.5 m/s walking (the pose
            // is keyed to s, the slow wobble to the ambient clock t = s / v); K4 excludes the K5 whip (limb 3), the K5 windows
            // are that limb alone. LEAD DECISION 2 with the lead's correction (show follow-up): EVERY visible motion <= 8 m/s at
            // 2.5 m/s, the K5 topple and slap included; the shadow and wake on the sea (the K3 glide, the transits) too.
            var windows = new (string id, double s0, double s1, int only, int skip, double limit)[]
            {
                ("K1", 116, 240, -1, -1, 8.0), ("K2", 290, 480, -1, -1, 8.0), ("K3", 440, 540, -1, -1, 8.0),
                ("K4", 704, 912, -1, KrakenDirectorChoreo.K5Limb, 8.0),
                ("K5rise_topple", 756, KrakenDirectorChoreo.K5ImpactS, KrakenDirectorChoreo.K5Limb, -1, 8.0),
                ("K5after", KrakenDirectorChoreo.K5ImpactS, 850, KrakenDirectorChoreo.K5Limb, -1, 8.0),
            };
            double peak25 = 0, peak15 = 0;
            foreach (double walk in new[] { 2.5, 1.5 })
            {
                sb.Append("  \"speeds_" + F(walk) + "\": {\n");
                for (int w = 0; w < windows.Length; w++)
                {
                    var (id, s0, s1, only, skip, limit) = windows[w];
                    double ds = 0.25, maxV = 0, at = 0; int atLimb = -1;
                    var prev = new Vector3[KrakenRig.ArmCount * KrakenRig.ControlPoints];
                    var prevHead = Vector3.zero; bool has = false; double maxHead = 0, atHead = 0;
                    double maxBus = 0, atBus = 0;
                    // shadow / wake entries matched frame to frame by kind and nearest centre (within 20 m): a dissolve from
                    // one standing entry into another (K2 -> K3 at s 446-458) is no motion; a moving entry is
                    var prevBus = new List<(SeaDisturbanceKind kind, Vector2 c)>(); var curBus = new List<(SeaDisturbanceKind kind, Vector2 c)>();
                    for (double s = s0; s <= s1 + 1e-9; s += ds)
                    {
                        probe.At(s, -1.0, walk);
                        // the dark mass on the sea: the Shadow and Wake centres (strength > 0.05) move like the body (the Breach
                        // ring follows the waterline section of a rising head: its shift is the head's shape, not a motion)
                        curBus.Clear();
                        for (int d = 0; d < probe.Pose.DistCount; d++)
                        {
                            var e = probe.Pose.Dist[d];
                            if ((e.Kind != SeaDisturbanceKind.Shadow && e.Kind != SeaDisturbanceKind.Wake) || e.Strength <= 0.05f) continue;
                            var c = new Vector2(e.X, e.Z);
                            curBus.Add((e.Kind, c));
                            double best = double.MaxValue;
                            foreach (var q in prevBus) if (q.kind == e.Kind) best = System.Math.Min(best, Vector2.Distance(c, q.c));
                            if (best < 20.0)
                            {
                                double vb = best / (ds / walk);
                                if (vb > maxBus) { maxBus = vb; atBus = s; }
                            }
                        }
                        prevBus.Clear(); prevBus.AddRange(curBus);
                        var hp = probe.Pose.Root + Vector3.up * KrakenDirectorChoreo.HeadTop;
                        if (has && probe.Pose.HeadTopAboveSea > 0f)
                        {
                            double vh = Vector3.Distance(hp, prevHead) / (ds / walk);
                            if (vh > maxHead) { maxHead = vh; atHead = s; }
                        }
                        prevHead = hp;
                        for (int k = 0; k < KrakenRig.ArmCount; k++)
                        {
                            if ((only >= 0 && k != only) || k == skip) continue;
                            for (int i = 0; i < KrakenRig.ControlPoints; i++)
                            {
                                var q = probe.W(k, i);
                                int idx = k * KrakenRig.ControlPoints + i;
                                if (has && q.y > KrakenDirectorChoreo.SeaY + 1f && prev[idx].y > KrakenDirectorChoreo.SeaY + 1f)
                                {
                                    double v = Vector3.Distance(q, prev[idx]) / (ds / walk);
                                    if (v > maxV) { maxV = v; at = s; atLimb = k; }
                                }
                                prev[idx] = q;
                            }
                        }
                        has = true;
                    }
                    sb.Append("    \"" + id + "\": { \"maxEmergedPointSpeed\": " + F(maxV) + ", \"atS\": " + F(at) + ", \"limb\": " + atLimb
                              + ", \"maxHeadTopSpeed\": " + F(maxHead) + ", \"headAtS\": " + F(atHead)
                              + ", \"maxShadowWakeSpeed\": " + F(maxBus) + ", \"busAtS\": " + F(atBus) + " }" + (w < windows.Length - 1 ? "," : "") + "\n");
                    Debug.Log($"[BW] kraken speeds at {walk} m/s walking: {id} emerged arm points {maxV:F2} m/s at s {at:F2} (limb {atLimb}), head top {maxHead:F2} m/s at s {atHead:F2}, shadow/wake {maxBus:F2} m/s at s {atBus:F2}");
                    double peak = System.Math.Max(maxV, System.Math.Max(maxHead, maxBus));
                    if (walk > 2.0) peak25 = System.Math.Max(peak25, peak); else peak15 = System.Math.Max(peak15, peak);
                    if (maxBus > 8.0) fails.Add($"{id} at {walk} m/s: shadow/wake speed {maxBus:F1} m/s at s {atBus:F1} > 8");
                    if (maxV > limit) fails.Add($"{id} at {walk} m/s: emerged arm point speed {maxV:F1} m/s at s {at:F1} (limb {atLimb}) > {limit}");
                    if (maxHead > 8.0) fails.Add($"{id} at {walk} m/s: head top speed {maxHead:F1} m/s at s {atHead:F1} > 8");
                }
                sb.Append("  },\n");
            }
            sb.Append("  \"peak_2.5\": " + F(peak25) + ", \"peak_1.5\": " + F(peak15) + "\n");
            sb.Append("}\n");
            Debug.Log($"[BW] kraken peak visible speed: {peak25:F2} m/s at 2.5 m/s walking, {peak15:F2} m/s at 1.5 m/s (limit {KrakenDirectorChoreo.MaxVisibleSpeed} at 2.5)");
            Directory.CreateDirectory("QA/reports");
            File.WriteAllText("QA/reports/kraken_show.json", sb.ToString());
            Debug.Log("[BW] kraken_show.json\n" + sb);
            Assert.IsEmpty(fails, string.Join("; ", fails));
        }

        // ------------------------------------------------------------------ the stinger seen from the lantern platform
        /// <summary>KrDamClipKeep (BwCreatureSkin.hlsl) on the CPU: > 0 = visible over the dam from a lee-side eye.</summary>
        static float DamLine(Vector3 eye, Vector3 q)
        {
            float dx = q.x - eye.x;
            if (q.x <= (float)BulwarkConst.SeaFaceX || dx <= 1e-3f) return 1f;
            float yLee = eye.y + (q.y - eye.y) * Mathf.Clamp01(((float)BulwarkConst.LeeFaceX - eye.x) / dx);
            float ySea = eye.y + (q.y - eye.y) * Mathf.Clamp01(((float)BulwarkConst.SeaFaceX - eye.x) / dx);
            return Mathf.Min(yLee, ySea) - (float)BulwarkConst.ParapetTop;
        }

        static float Elev(Vector3 eye, Vector3 q) { var d = q - eye; return Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg; }

        /// <summary>Elevation of the crest (the parapet top over the lee face) seen from the eye along the azimuth of q.</summary>
        static float CrestElev(Vector3 eye, Vector3 q)
        {
            var d = q - eye;
            float hd = new Vector2(d.x, d.z).magnitude * ((float)BulwarkConst.LeeFaceX - eye.x) / Mathf.Max(d.x, 1f);
            return Mathf.Atan2((float)BulwarkConst.ParapetTop - eye.y, hd) * Mathf.Rad2Deg;
        }

        [Test]
        public void Stinger_ReadsInTheLookBackAndRisesSlowly()
        {
            // the end camera at the lantern-platform rest point (the ending owner's EndingPath / CameraRig, mirrored in
            // KrakenDirectorChoreo): KS28 (pitch 29, frame top 62 deg) holds to t_g 18, the slow tilt-up reaches pitch 36 (frame
            // top 69 deg) at 28, the fade starts at 40. Visible = above the dam line (the creature's dam clip).
            var rig = new KrakenRig(Assets().Manifest);
            var arms = new KrakenDirectorArms();
            var pose = new KrakenShowPose();
            var eye = new Vector3((float)KrakenDirectorChoreo.EndEyeX, (float)KrakenDirectorChoreo.EndEyeY, (float)KrakenDirectorChoreo.EndEyeZ);
            int[] limbs = { 2, 3, 1 };    // main, north flank, south flank (KrakenDirectorChoreo.Stinger)
            string[] names = { "main", "north", "south" };
            var fails = new List<string>();
            int K = KrakenRig.ControlPoints;
            var prev = new Vector3[KrakenRig.ArmCount * K];
            var prevVis = new bool[KrakenRig.ArmCount * K];
            double dt = 0.25, maxV = 0, atV = 0, maxW = 0;
            int atLimb = -1;
            var sb = new StringBuilder("{\n  \"frames\": [\n");
            bool first = true;
            for (double tg = 0; tg <= 44.0 + 1e-9; tg += dt)
            {
                double t = 1000.0 + tg;
                KrakenDirectorChoreo.Evaluate(1500, t, tg, pose);
                rig.ResetInputs();
                rig.SetPresetWeight(KrakenPreset.Lurk, pose.Lurk);
                rig.Scale = pose.Scale;
                rig.SetRoot(pose.Root, pose.Heading, pose.Pitch);
                rig.Evaluate(t);
                for (int i = 0; i < pose.ArmCount; i++) arms.Apply(rig, pose.Arms[i]);
                KrakenDirector.ClampCorridor(rig, false);
                double pitch = KrakenDirectorChoreo.EndLookPitch(tg), frameTop = pitch + 0.5 * KrakenDirectorChoreo.EndLookFovV;
                var colTop = new float[] { -90f, -90f, -90f };
                var colCrest = new float[3];
                var colAz = new float[3];
                for (int k = 0; k < KrakenRig.ArmCount; k++)
                    for (int i = 0; i < K; i++)
                    {
                        var q = rig.LocalToWorld.MultiplyPoint3x4(rig.Limbs[k].CurP[i]);
                        int idx = k * K + i;
                        bool vis = DamLine(eye, q) > 0f;
                        if (vis && prevVis[idx] && tg > 0.0)
                        {
                            double v = Vector3.Distance(q, prev[idx]) / dt;
                            if (v > maxV) { maxV = v; atV = tg; atLimb = k; }
                            double w = Vector3.Angle(q - eye, prev[idx] - eye) / dt;
                            if (w > maxW) maxW = w;
                        }
                        prev[idx] = q; prevVis[idx] = vis;
                        int col = System.Array.IndexOf(limbs, k);
                        if (vis && col >= 0)
                        {
                            float el = Elev(eye, q);
                            if (el > colTop[col]) { colTop[col] = el; colCrest[col] = CrestElev(eye, q); colAz[col] = Mathf.Atan2(q.z - eye.z, q.x - eye.x) * Mathf.Rad2Deg; }
                        }
                    }
                bool ks28 = tg >= 8.5 && tg <= 18.0, final = tg >= 28.0 && tg <= 40.0;
                for (int c = 0; c < 3; c++)
                {
                    float above = colTop[c] - colCrest[c];
                    // KS28: the shafts stand at least 1 deg above the crest (they rise out of the frame's top edge)
                    if (ks28 && above < 1f) fails.Add($"KS28 t_g {tg:F1}: {names[c]} column only {above:F2} deg above the crest");
                    // the final look-back frame: the hooked tops inside the frame (>= 2 deg under the top edge: the 3 % margin)
                    // and 3 deg or more above the crest; all three columns within +-40 deg of the view axis (hFOV +-49)
                    if (final && colTop[c] > frameTop - 2.0) fails.Add($"t_g {tg:F1}: {names[c]} top {colTop[c]:F2} deg above the frame margin ({frameTop - 2.0:F2})");
                    if (final && above < 3f) fails.Add($"t_g {tg:F1}: {names[c]} top only {above:F2} deg above the crest");
                    if ((ks28 || final) && Mathf.Abs(colAz[c]) > 40f) fails.Add($"t_g {tg:F1}: {names[c]} at azimuth {colAz[c]:F1} off the view axis");
                }
                if (System.Math.Abs(tg - System.Math.Round(tg / 2.0) * 2.0) < 1e-6)
                {
                    sb.Append((first ? "" : ",\n") + "    { \"tg\": " + F(tg) + ", \"pitch\": " + F(pitch) + ", \"frameTop\": " + F(frameTop)
                              + ", \"topDeg\": [" + F(colTop[0]) + ", " + F(colTop[1]) + ", " + F(colTop[2]) + "], \"crestDeg\": [" + F(colCrest[0]) + ", "
                              + F(colCrest[1]) + ", " + F(colCrest[2]) + "], \"heights\": [" + F(KrakenDirectorChoreo.StingerHeight(0, tg)) + ", "
                              + F(KrakenDirectorChoreo.StingerHeight(1, tg)) + ", " + F(KrakenDirectorChoreo.StingerHeight(2, tg)) + "] }");
                    first = false;
                }
            }
            // LEAD DECISION 2 for a 2.5x animal 7.4 km away: 8 m/s x StingerScale in its own proportions
            double limit = KrakenDirectorChoreo.MaxVisibleSpeed * KrakenDirectorChoreo.StingerScale;
            sb.Append("\n  ],\n  \"maxVisibleSpeed\": " + F(maxV) + ", \"atTg\": " + F(atV) + ", \"limb\": " + atLimb + ", \"maxAngularDegPerS\": " + F(maxW)
                      + ", \"limit\": " + F(limit) + "\n}\n");
            Directory.CreateDirectory("QA/reports");
            File.WriteAllText("QA/reports/kraken_stinger.json", sb.ToString());
            Debug.Log($"[BW] kraken stinger: max visible speed {maxV:F2} m/s at t_g {atV:F2} (limb {atLimb}), {maxW:F3} deg/s; limit {limit:F1}\n" + sb);
            if (maxV > limit) fails.Add($"stinger visible speed {maxV:F1} m/s at t_g {atV:F2} (limb {atLimb}) > {limit:F1}");
            Assert.IsEmpty(fails, string.Join("; ", fails.GetRange(0, System.Math.Min(fails.Count, 12))));
        }

        [Test]
        public void Stinger_AppearsOnlyWhileTheCameraLooksBelowTheCrest()
        {
            var eye = new Vector3((float)KrakenDirectorChoreo.EndEyeX, (float)KrakenDirectorChoreo.EndEyeY, (float)KrakenDirectorChoreo.EndEyeZ);
            Vector3 Fwd(float yaw, float pitch)
            {
                float y = yaw * Mathf.Deg2Rad, p = pitch * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(y) * Mathf.Cos(p), Mathf.Sin(p), Mathf.Cos(y) * Mathf.Cos(p));
            }
            Assert.IsTrue(KrakenDirector.StingerMayAppear(eye, Fwd(90f, 0f), 66f), "KS27 (pitch 0): frame top 33 deg, crest 59.9");
            Assert.IsTrue(KrakenDirector.StingerMayAppear(eye, Fwd(0f, 10f), 66f), "looking north along the platform");
            Assert.IsTrue(KrakenDirector.StingerMayAppear(eye, Fwd(270f, 40f), 66f), "looking west");
            Assert.IsFalse(KrakenDirector.StingerMayAppear(eye, Fwd(90f, 29f), 66f), "KS28: the crest is in the frame");
            Assert.IsFalse(KrakenDirector.StingerMayAppear(eye, Fwd(70f, 40f), 66f), "a player looking up at the crest");
        }
    }
}
