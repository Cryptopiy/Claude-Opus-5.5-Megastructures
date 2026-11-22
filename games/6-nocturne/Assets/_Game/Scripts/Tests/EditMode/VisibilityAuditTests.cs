using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    /// <summary>
    /// The 11.13 visibility and reachability audit as an EditMode test (V). The world is built once per run (VisibilityWorld: every
    /// IBeatBuilder as GameBootstrap discovers them, CityField included) and the audit (VisibilityAudit) writes
    /// QA/reports/visibility.json and QA/reports/visibility.md. Hard requirements assert (reachability, T-DROP of 14.10, the koi
    /// composition lint of 14.5.7 acceptance 2, the follower is the runtime planner); findings that need a lead decision (follower
    /// lag over 2 deg, subjects outside the 3 % margin, spec numbers that the ray cast does not reproduce) are listed in the report
    /// with their fix and owner and logged as a warning. Subjects are spec-derived, so missing beats never break the audit.
    /// </summary>
    public sealed class VisibilityAuditTests
    {
        static VisibilityAudit s_audit;
        static VisibilityRaycaster s_built;
        static string s_buildInfo;
        static Exception s_error;
        CultureInfo m_culture;

        static string Root => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        static void Report(string line) { TestContext.Out.WriteLine("[VIS] " + line); Debug.Log("[VIS] " + line); }

        [OneTimeSetUp]
        public void Setup()
        {
            m_culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [OneTimeTearDown]
        public void TearDown() { if (m_culture != null) CultureInfo.CurrentCulture = m_culture; }

        static VisibilityAudit Audit()
        {
            if (s_audit != null || s_error != null) { if (s_error != null) throw new Exception("audit failed earlier", s_error); return s_audit; }
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                VisibilityRaycaster built = VisibilityWorld.Build(out VisibilityWorld.Info info);
                s_built = built;
                s_buildInfo = info.ToString();
                var audit = new VisibilityAudit(new VisibilitySpecWorld(), built.TriangleCount > 0 ? built : null) { BuiltInfo = s_buildInfo };
                audit.Run();
                string dir = Path.Combine(Root, "QA", "reports");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "visibility.json"), audit.Json());
                File.WriteAllText(Path.Combine(dir, "visibility.md"), audit.Markdown());
                Report(FormattableString.Invariant($"world: {s_buildInfo}"));
                Report(FormattableString.Invariant($"audit {audit.Seconds:0.0} s, total {sw.Elapsed.TotalSeconds:0.0} s, {audit.Failures.Count} failures; reports in QA/reports/visibility.json / .md"));
                s_audit = audit;
            }
            catch (Exception e) { s_error = e; throw; }
            return s_audit;
        }

        static List<VisFailure> Kind(VisibilityAudit a, params string[] kinds)
        {
            var l = new List<VisFailure>();
            foreach (var f in a.Failures) if (Array.IndexOf(kinds, f.kind) >= 0) l.Add(f);
            return l;
        }

        static string Lines(List<VisFailure> l)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var f in l) sb.Append("\n  ").Append(f.shot).Append(' ').Append(f.subject).Append(": ").Append(f.detail);
            return sb.ToString();
        }

        // ------------------------------------------------------------------ the audit
        [Test]
        public void A_AuditBuildsTheWorldAndWritesTheReports()
        {
            var a = Audit();
            Assert.AreEqual(20, a.Results.Count, "every 14.10 key shot");
            foreach (var r in a.Results) Assert.Greater(r.subjects.Count + (r.koi != null ? 1 : 0), 0, r.shot.id + " has subjects");
            Assert.IsTrue(File.Exists(Path.Combine(Root, "QA", "reports", "visibility.json")));
            Assert.IsTrue(File.Exists(Path.Combine(Root, "QA", "reports", "visibility.md")));
            Assert.Less(a.Seconds, 180.0, "audit runtime (task budget about 3 minutes)");
            foreach (var r in a.Results)
                Report(FormattableString.Invariant($"{r.shot.id} s {r.shot.ks.s}: lag f40 {r.lag40:0.00} fW {r.lagWalk:0.00} deg, readable {r.readA:0.0}-{r.readB:0.0} (spec {r.shot.readA}-{r.shot.readB}), reach {(r.reachOnPath ? "path" : r.reachInLane ? "lane" : "OUT")}"));
            foreach (var f in a.Failures) Report($"FAILURE {f.shot} {f.subject} [{f.kind}, {f.owner}]: {f.detail}");
            var findings = Kind(a, "follower-lag", "frame", "occlusion", "hidden", "composition", "composition-built", "readable", "schedule");
            if (findings.Count > 0) Debug.LogWarning("[VIS] " + findings.Count + " findings for the lead / owners (see QA/reports/visibility.md):" + Lines(findings));
        }

        /// <summary>Fix wave C10 (lead): KS04's subjects at FX's new pitch +27 (D-040; blvd copy's NocturneConstants): every subject
        /// framed and visible at the target pose for the three aspects, spec model and built meshes.</summary>
        [Test]
        public void KS04_SubjectsHoldAtPitch27()
        {
            Audit();
            var shots = VisibilitySubjects.All().FindAll(v => v.id == "KS04");
            Assert.AreEqual(1, shots.Count);
            KeyShot k = shots[0].ks;
            double pitch0 = k.pitch;
            k.pitch = 27.0;
            shots[0].ks = k;
            var a = new VisibilityAudit(new VisibilitySpecWorld(), s_built != null && s_built.TriangleCount > 0 ? s_built : null);
            a.Run(shots, false);
            Assert.AreEqual(1, a.Results.Count);
            foreach (var sr in a.Results[0].subjects)
                Report(FormattableString.Invariant($"KS04 pitch {pitch0:0} -> 27: {sr.subject.name} {(sr.pass ? "pass" : "FAIL")} visible spec {sr.visSpec:0.00} built {sr.visBuilt:0.00} {sr.why}"));
            var f = a.Failures.FindAll(x => x.kind == "frame" || x.kind == "hidden" || x.kind == "occlusion");
            foreach (var x in f) Report($"KS04@27 FAILURE {x.subject} [{x.kind}]: {x.detail}");
            Assert.IsEmpty(f, "KS04's subjects at pitch 27:" + Lines(f));
        }

        [Test]
        public void ReachabilityOfEveryKeyShotEye()
        {
            var a = Audit();
            var f = Kind(a, "reach");
            Assert.IsEmpty(f, "key-shot eyes outside the walkable corridor and off the cinematic path:" + Lines(f));
            foreach (var r in a.Results) Assert.IsTrue(r.reachOnPath || r.reachInLane, r.shot.id);
        }

        [Test]
        public void TDropEveryDownwardShotKeepsItsSubjectsUnoccluded()
        {
            var a = Audit();
            int shots = 0;
            foreach (var r in a.Results)
            {
                if (!r.shot.TDrop) continue;
                shots++;
                foreach (var s in r.subjects)
                    if (s.subject.rule != VisRule.Info)
                        Report(FormattableString.Invariant($"T-DROP {r.shot.id} {s.subject.name}: unoccluded spec {s.visSpec:0.00} built {s.visBuilt:0.00} over {s.occSamples} samples {s.occSpecBy} {s.occBuiltBy}"));
            }
            Assert.AreEqual(4, shots, "KS09, KS10, KS12, KS13 have pitch below -5 deg");
            // the plan (spec deck, rails and tongue edges) must pass; failures on the built meshes are listed in visibility.md for the
            // owner (Phase 3: the KS13 glass strip is deferred to Phase 4 by G3, so the built deck is continuous there)
            var plan = new List<VisFailure>();
            foreach (var r in a.Results)
                if (r.shot.TDrop)
                    foreach (var s in r.subjects)
                        if (s.subject.rule != VisRule.Info && s.visDeckSpec < VisibilityAudit.TDropMin)
                            plan.Add(new VisFailure { shot = r.shot.id, subject = s.subject.name, detail = FormattableString.Invariant($"{s.visDeckSpec:0.00}") });
            var built = Kind(a, "T-DROP");
            if (built.Count > 0) Debug.LogWarning("[VIS] T-DROP on the built geometry:" + Lines(built));
            Assert.IsEmpty(plan, "T-DROP on the 14.4 / 14.5 plan (>= 60 % unoccluded by deck, rails and edges):" + Lines(plan));
        }

        [Test]
        public void KoiCompositionLint()
        {
            var a = Audit();
            int n = 0;
            foreach (var r in a.Results)
            {
                if (r.koi == null) continue;
                n++;
                for (int p = 0; p < VisibilityAudit.PoseNames.Length; p++)
                    for (int q = 0; q < VisibilityAudit.AspectNames.Length; q++)
                    {
                        var ka = r.koi.aspects[p, q];
                        Report(FormattableString.Invariant($"koi {r.shot.id} {VisibilityAudit.PoseNames[p]} {ka.aspect}: visible x {ka.x0:0.00}..{ka.x1:0.00} y {ka.y0:0.00}..{ka.y1:0.00}, min margin {ka.minMargin:0.000} (all {ka.minMarginAll:0.000}), head x {ka.hx0:0.00}..{ka.hx1:0.00} y {ka.hy0:0.00}..{ka.hy1:0.00}, outside {ka.outside}"));
                    }
                Report(FormattableString.Invariant($"koi {r.shot.id}: core hidden per phase spire max {r.koi.maxSpire} arcade max {r.koi.maxArcade} other {r.koi.maxOther} built {r.koi.maxBuilt}; head occluded {r.koi.headOccluded}; body {r.koi.minThick4k:0}-{r.koi.maxThick4k:0} px at 4K; spec margin {r.shot.koiSpecMargin}"));
            }
            Assert.AreEqual(3, n, "KS17, KS18, KS19");
            // 14.5.7 acceptance (2) names KS17 and KS18 (whole koi); D-029 (GB-01): KS19's subjects are the window, the lamp and the
            // koi body (the head stands behind the tenement's roof edge and is reported only), so KS19 asserts the body
            var f = Kind(a, "koi-lint");
            foreach (var r in a.Results)
                if (r.koi != null && r.shot.koiBody) Report(FormattableString.Invariant($"koi {r.shot.id}: body samples visible inside the margin, worst phase / pose / aspect {r.koi.minBodyIn}"));
            Assert.IsEmpty(f, "koi composition lint (14.5.7 acceptance 2: KS17, KS18, 3 % margin, three aspects, 20 wave phases; KS19 body, D-029):" + Lines(f));
        }

        [Test]
        public void KS19SubjectsAreWindowLampAndKoiBody()
        {
            // D-029 (GB-01): the window and the lamp are whole subjects, unoccluded; the koi body is the shot's partial koi subject
            var a = Audit();
            var r = a.Results.Find(x => x.shot.id == "KS19");
            Assert.IsNotNull(r);
            Assert.IsTrue(r.shot.koiBody && !r.shot.koiWhole, "KS19 koi subject is the body");
            foreach (string name in new[] { "goal_window", "goal_lamp" })
            {
                var s = r.subjects.Find(x => x.subject.name == name);
                Assert.IsNotNull(s, name);
                Report(FormattableString.Invariant($"KS19 {name}: pass {s.pass}, unoccluded spec {s.visSpec:0.00} built {s.visBuilt:0.00}, {s.wPx1080:0} x {s.hPx1080:0} px"));
                Assert.IsTrue(s.pass, "KS19 " + name + ": " + s.why);
            }
            Assert.GreaterOrEqual(r.koi.minBodyIn, VisibilitySubjects.MinPartSamples, "KS19 koi body visible in every wave phase");
        }

        [Test]
        public void PreRevealKoiIsHidden()
        {
            // D-029 (GB-03): before the KS17 reveal no koi sample shows in KS04-KS16 or in the beat 2-6 stills (built meshes)
            var a = Audit();
            Assert.Greater(a.PreReveal.Count, 40, "13 key shots + 35 beat stills");
            int total = 0, spire = 0;
            foreach (var p in a.PreReveal)
            {
                total += p.visible; spire += p.spireViaNotch;
                if (p.visible > 0 || p.spireVisible > 0)
                    Report(FormattableString.Invariant($"pre-reveal {p.tag} s {p.s:0.0}: koi {p.visible} / {p.inFrame} in frame (notch {p.viaNotch}, promenade {p.viaPromenade}, arcade {p.viaArcade}, other {p.viaOther}); spire {p.spireVisible} / {p.spireInFrame} (notch {p.spireViaNotch}) {p.example}"));
            }
            Report(FormattableString.Invariant($"pre-reveal: {a.PreReveal.Count} poses, koi sample-phases visible {total}, spire points through the notch {spire}"));
            var f = Kind(a, "pre-reveal");
            Assert.IsEmpty(f, "koi visible before the reveal (D-029 GB-03):" + Lines(f));
            Assert.AreEqual(0, spire, "no spire point shows through the garden notch before the reveal");
        }

        // ------------------------------------------------------------------ the follower is the runtime code
        [Test]
        public void FollowerIsTheRuntimePlanner()
        {
            // AutoWalker drives exactly one WalkPlanner; VisibilityFollower runs the same class (no mirror to drift)
            var f = typeof(AutoWalker).GetField("_plan", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(f, "AutoWalker._plan");
            Assert.AreEqual(typeof(WalkPlanner), f.FieldType);
            Assert.AreEqual(2.5, WalkPlanner.LookOmega, 1e-12, "omega 2.5 rad/s (14.5)");
            Assert.AreEqual(22.0, WalkPlanner.YawRateMax, 1e-12);
            Assert.AreEqual(18.0, WalkPlanner.PitchRateMax, 1e-12);
            // a traced walk respects the rate limits and settles on a held key (KS17: keys (1296, 6, 18) and (1310, 6, 18))
            var s = new List<double>(); var y = new List<double>(); var p = new List<double>();
            var yo = new double[1]; var po = new double[1];
            VisibilityFollower.Run(1200.0, new[] { 1310.0 }, yo, po, s, y, p);
            double maxYaw = 0.0, maxPitch = 0.0;
            for (int i = 1; i < s.Count; i++)
            {
                maxYaw = Math.Max(maxYaw, Math.Abs(NocMath.DeltaAngle(y[i - 1], y[i])) * VisibilityFollower.Fps);
                maxPitch = Math.Max(maxPitch, Math.Abs(p[i] - p[i - 1]) * VisibilityFollower.Fps);
            }
            Assert.LessOrEqual(maxYaw, 22.0 + 1e-6, "yaw rate");
            Assert.LessOrEqual(maxPitch, 18.0 + 1e-6, "pitch rate");
            Assert.AreEqual(96.0, NocMath.WrapDeg(yo[0]) < 0 ? NocMath.WrapDeg(yo[0]) + 360.0 : NocMath.WrapDeg(yo[0]), 0.5, "settled on the KS17 hold yaw");
            Assert.AreEqual(18.0, po[0], 0.5, "settled on the KS17 hold pitch");
            Report(FormattableString.Invariant($"follower 1200-1310: max rates {maxYaw:0.00} / {maxPitch:0.00} deg/s; at s 1310 yaw {NocMath.WrapDeg(yo[0]):0.00} pitch {po[0]:0.00}"));
        }

        // ------------------------------------------------------------------ spec subjects reproduce the spec's own numbers
        [Test]
        public void SpecSubjectDerivationsReproduceTheSpecProjections()
        {
            // subjects whose placement V derived from the spec text (docs/logs/V.md): their 16:9 target projection must match 14.5
            var exact = new Dictionary<string, double>
            {
                { "KS01/bar_window", 0.03 }, { "KS01/bar_sign", 0.05 }, { "KS01/counter", 0.03 }, { "KS02/catwalk", 0.04 },
                { "KS03/blade_n54", 0.03 }, { "KS03/blade_n90", 0.04 }, { "KS03/north_facade_base", 0.03 },
                { "KS04/blade_n198", 0.04 }, { "KS04/blade_s186", 0.03 }, { "KS04/canyon_end", 0.02 },
                { "KS06/tram", 0.03 }, { "KS07/garden_crowns", 0.03 }, { "KS14/gate_B_frame", 0.04 },
                { "KS16/wall_p10_300", 0.02 }, { "KS16/wall_p150_350", 0.02 }, { "KS16/wall_p300_420", 0.02 }, { "KS19/goal_window", 0.03 },
            };
            var a = Audit();
            var bad = new List<string>();
            foreach (var r in a.Results)
                foreach (var s in r.subjects)
                {
                    string key = r.shot.id + "/" + s.subject.name;
                    if (!double.IsNaN(s.specErr)) Report(FormattableString.Invariant($"spec {key}: deviation {s.specErr:0.000}"));
                    if (!exact.TryGetValue(key, out double tol)) continue;
                    if (double.IsNaN(s.specErr) || s.specErr > tol) bad.Add(FormattableString.Invariant($"{key} {s.specErr:0.000} > {tol}"));
                }
            Assert.IsEmpty(bad, "derived subjects off the 14.5 projections: " + string.Join("; ", bad));
        }
    }
}
