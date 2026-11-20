// VisibilityAudit: the 11.13 visibility and reachability audit of every key shot (MASTER_PROMPT 11.13, 14.5, 14.10). For each shot of
// VisibilitySubjects it
//   * takes the target pose (the 14.10 table: eye, yaw, pitch, vFOV), checks it against the gaze schedule and the vFOV schedule,
//     and the follower poses that the cinematic AutoWalker really has at that s (VisibilityFollower: the runtime WalkPlanner run 40 m
//     earlier at 2.5 m/s, and one whole cinematic walk from s 0); lag in degrees, flagged above 2 deg; the readable window
//     (8 deg yaw, 6 deg pitch, 14.5) measured on the walk against the spec's;
//   * projects every spec subject at 16:9 (1920 x 1080), 16:10 (1680 x 1050) and 21:9 (2560 x 1080) for the target and both
//     follower poses (QaProjection: Hor+, NDC frame +-1, the 3 % margin is |ndc| <= 0.94); NDC bounds next to the spec's numbers;
//     sizes in pixels at 1080p and 4K;
//   * ray-casts every sample from the eye against the spec occluder model (VisibilitySpecWorld) and, when given, the built meshes
//     (VisibilityRaycaster): the unoccluded fraction per subject; T-DROP (14.10: every shot with pitch below -5 deg, every named
//     subject >= 60 % unoccluded);
//   * the frame composition by a ray grid (KS09, KS10, KS12: sky / pier / drop against 26/39/36, 7/50/43, 22/30/48; KS13: the
//     strip; KS16: the wall share against 59 % / 46 %);
//   * the koi composition lint (KS17, KS18: the 183 KoiMath samples over the 20 wave phases at the three aspects; samples hidden by
//     an occluder are exempt, fin samples whose body root is hidden too (D-009); the head must not be occluded; minimum margins
//     against 0.085 / 0.178; KS19 (D-029 GB-01): the body as a partial subject, the head reported only);
//   * the pre-reveal koi check (D-029 GB-03, GE): no koi sample visible in KS04-KS16 and the beat 2-6 stills (ShotPlan.BeatStills),
//     20 wave phases at the koi phase of each pose's s, against the built meshes; each visible sample is classified by the opening
//     its sight line passes (the garden notch of face B, the promenade gap, over the arcade, other);
//   * reachability: the eye inside the walkable corridor (NocLanes) or on the cinematic path;
// and writes QA/reports/visibility.json and visibility.md (every failure with the fix it needs and its owner).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Megastructure
{
    public sealed class VisProj
    {
        public int n, inFront, inFrame, inMargin;
        public double x0 = double.MaxValue, x1 = double.MinValue, y0 = double.MaxValue, y1 = double.MinValue;     // in front
        public double fx0 = double.MaxValue, fx1 = double.MinValue, fy0 = double.MaxValue, fy1 = double.MinValue; // in frame
        public double minMargin = double.MaxValue;   // 1 - max(|x|, |y|) over in-front samples
        public bool pass;
        public bool HasFront => inFront > 0;
        public bool HasFrame => inFrame > 0;
    }

    public sealed class VisSubjectResult
    {
        public VisSubject subject;
        public VisProj[,] proj;       // [pose, aspect]
        public double visSpec = double.NaN, visBuilt = double.NaN;
        /// <summary>T-DROP (14.10): unoccluded fraction against the deck, rails and tongue edges only (spec walk model / built hits in
        /// the deck band y -1.5..1.25 within 30 m of the eye).</summary>
        public double visDeckSpec = double.NaN, visDeckBuilt = double.NaN;
        public int occSamples;        // samples used for the occlusion fraction
        public string occSpecBy = "", occBuiltBy = "";
        public double wPx1080, hPx1080, wPx4k, hPx4k;
        public double specErr = double.NaN;
        public bool pass = true;
        public string why = "";
    }

    public sealed class VisKoiAspect
    {
        public string aspect;
        public double x0 = double.MaxValue, x1 = double.MinValue, y0 = double.MaxValue, y1 = double.MinValue;   // visible samples, all phases
        public double minMargin = double.MaxValue, minMarginAll = double.MaxValue, minMarginCore = double.MaxValue;
        public double hx0 = double.MaxValue, hx1 = double.MinValue, hy0 = double.MaxValue, hy1 = double.MinValue, headMargin = double.MaxValue;
        public int outside;          // visible samples outside the 3 % margin (all phases)
        public int worstPhase = -1;
    }

    public sealed class VisKoiResult
    {
        public VisKoiAspect[,] aspects;   // [pose, aspect]
        public int maxSpire, maxArcade, maxOther, sumSpire, sumArcade, maxBuilt;   // core samples hidden per phase
        public int headOccluded, exemptFins;
        /// <summary>KS19 (D-029 GB-01): fewest core samples visible inside the margin over phases, poses and aspects (-1 = not a body shot).</summary>
        public int minBodyIn = -1;
        public string headOccluder = "";
        public double minThick4k = double.MaxValue, maxThick4k;
        public bool pass = true;
        public string why = "";
    }

    public sealed class VisShotResult
    {
        public VisShot shot;
        public double schedYaw, schedPitch, schedFov, yaw40, pitch40, yawWalk, pitchWalk;
        public double lag40, lagWalk;               // deg, max of |yaw| and |pitch| differences to the table pose
        public double readA = double.NaN, readB = double.NaN;
        public double reachS, reachLateral, reachOutside;
        public bool reachInLane, reachOnPath;
        public readonly List<VisSubjectResult> subjects = new List<VisSubjectResult>();
        public double[] sharesSpec, sharesBuilt;    // composition: (sky, deck, rail, structure, drop) / (deck, strip, other) / (wall 16:9, wall 21:9)
        public string compBuiltBy = "";            // top built renderers in the pier / near classes
        public double stripX0 = double.NaN, stripX1 = double.NaN, stripY0 = double.NaN, stripY1 = double.NaN;
        public VisKoiResult koi;
        public bool tdropPass = true;
    }

    /// <summary>One pose of the pre-reveal koi check (D-029 GB-03): a key shot KS04-KS16 (target pose, three aspects) or a beat 2-6
    /// still (16:9), the koi at the phase of the pose's s, 20 wave phases.</summary>
    public sealed class VisPreReveal
    {
        public string tag, kind;
        public double s, x, y, z, yaw, pitch, fov;
        /// <summary>Koi sample-phases inside the frame (any aspect) and those unoccluded by the built meshes.</summary>
        public int inFrame, visible;
        /// <summary>Visible sample-phases by the opening their sight line passes.</summary>
        public int viaNotch, viaPromenade, viaArcade, viaOther;
        public int spireInFrame, spireVisible, spireViaNotch;
        public double x0 = double.MaxValue, x1 = double.MinValue, y0 = double.MaxValue, y1 = double.MinValue;
        public string example = "";
    }

    public sealed class VisFailure
    {
        public string shot, subject, kind, detail, fix, owner;
    }

    public sealed class VisibilityAudit
    {
        public static readonly string[] AspectNames = { "16:9", "16:10", "21:9" };
        public static readonly int[] AspectW = { 1920, 1680, 2560 };
        public static readonly int[] AspectH = { 1080, 1050, 1080 };
        public static readonly string[] PoseNames = { "target", "follower40", "followerWalk" };
        public const double MarginNdc = 1.0 - 2.0 * QaProjection.FrameMargin;   // 0.94
        public const double LagFlag = 2.0;
        public const double TDropMin = 0.60;
        public const int GridX = 64, GridY = 36;
        /// <summary>Horizontal distance (m) within which a built hit belongs to the walk structure (deck, rails, gates, hull) in the
        /// composition shares; the Gulf faces are >= 120 m from every downward key-shot eye.</summary>
        public const double NearStructure = 60.0;

        public readonly IVisRayScene Spec;
        public readonly IVisRayScene SpecDeck = new VisibilitySpecWorld(true);
        public readonly IVisRayScene Built;
        public string BuiltInfo = "";
        public readonly List<VisShotResult> Results = new List<VisShotResult>();
        public readonly List<VisFailure> Failures = new List<VisFailure>();
        public readonly List<string> Notes = new List<string>();
        public readonly List<VisPreReveal> PreReveal = new List<VisPreReveal>();
        public double Seconds;
        /// <summary>The pre-reveal span (D-029 GB-03): beats 2-6 (s 120 .. 1 260; the reveal is KS17 in beat 7).</summary>
        public const double PreRevealS0 = 120.0, PreRevealS1 = 1260.0;

        public VisibilityAudit(IVisRayScene spec, IVisRayScene built) { Spec = spec; Built = built; }

        // ------------------------------------------------------------------ run
        public void Run()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var shots = VisibilitySubjects.All();
            // the whole cinematic walk from s 0 (sampled at every key-shot s), traced for the readable windows
            var sList = new List<double>();
            foreach (var sh in shots) sList.Add(sh.ks.s);
            var wy = new double[sList.Count]; var wp = new double[sList.Count];
            var trS = new List<double>(40000); var trY = new List<double>(40000); var trP = new List<double>(40000);
            VisibilityFollower.Run(0.0, sList, wy, wp, trS, trY, trP);
            for (int i = 0; i < shots.Count; i++)
            {
                var r = new VisShotResult { shot = shots[i] };
                RunShot(r, wy[i], wp[i], trS, trY, trP);
                Results.Add(r);
            }
            RunPreReveal();
            Seconds = sw.Elapsed.TotalSeconds;
        }

        void Fail(VisShotResult r, string subject, string kind, string detail, string fix, string owner)
            => Failures.Add(new VisFailure { shot = r.shot.id, subject = subject, kind = kind, detail = detail, fix = fix, owner = owner ?? r.shot.owner });

        static string F(double v, string f = "0.000") => double.IsNaN(v) || Math.Abs(v) > 1e300 ? "n/a" : v.ToString(f, CultureInfo.InvariantCulture);

        void RunShot(VisShotResult r, double yawWalk, double pitchWalk, List<double> trS, List<double> trY, List<double> trP)
        {
            KeyShot k = r.shot.ks;
            var eye = new D3(k.x, k.y, k.z);
            // schedule and follower
            GazeSchedule.Evaluate(k.s, out r.schedYaw, out r.schedPitch);
            r.schedFov = NocSchedule.Fov(k.s);
            VisibilityFollower.PoseAt(k.s - VisibilityFollower.LeadIn, k.s, out r.yaw40, out r.pitch40);
            r.yawWalk = yawWalk; r.pitchWalk = pitchWalk;
            r.lag40 = Math.Max(Math.Abs(NocMath.DeltaAngle(k.yaw, r.yaw40)), Math.Abs(k.pitch - r.pitch40));
            r.lagWalk = Math.Max(Math.Abs(NocMath.DeltaAngle(k.yaw, r.yawWalk)), Math.Abs(k.pitch - r.pitchWalk));
            double dYaw = NocMath.DeltaAngle(r.schedYaw, k.yaw), dPitch = k.pitch - r.schedPitch;
            if (Math.Abs(dYaw) > 0.05 || Math.Abs(dPitch) > 0.05)
                Fail(r, "pose", "schedule", FormattableString.Invariant($"table pose ({k.yaw:0.00}, {k.pitch:0.00}) differs from the gaze schedule at s {k.s} ({NocMath.WrapDeg(r.schedYaw):0.00}, {r.schedPitch:0.00})"),
                     "make the 14.10 pose a schedule point (gaze key) or correct the table", "lead");
            if (Math.Abs(r.schedFov - k.fov) > 0.05)
                Notes.Add(FormattableString.Invariant($"{r.shot.id}: table vFOV {k.fov} vs the 14.2.4 schedule {r.schedFov:0.00} at s {k.s} (captures use the table)"));
            if (r.lag40 > LagFlag || r.lagWalk > LagFlag)
            {
                string later = "";
                for (int i = 0; i < trS.Count; i++)
                {
                    if (trS[i] < k.s) continue;
                    if (trS[i] > k.s + 10.0) break;
                    double lag = Math.Max(Math.Abs(NocMath.DeltaAngle(k.yaw, trY[i])), Math.Abs(k.pitch - trP[i]));
                    if (lag < 1.0) { later = FormattableString.Invariant($"; the same pose captured at s {trS[i]:0.0} (+{trS[i] - k.s:0.0} m, eye x/z on the path) has a follower lag of {lag:0.00} deg"); break; }
                }
                Fail(r, "pose", "follower-lag", FormattableString.Invariant($"follower lags the key pose by {r.lag40:0.00} deg (40 m run) / {r.lagWalk:0.00} deg (whole walk): yaw {NocMath.WrapDeg(r.yawWalk):0.00}, pitch {r.pitchWalk:0.00}"),
                     FormattableString.Invariant($"on a ramp the omega 2.5 follower trails by 2/omega = 0.8 s = 2.0 m at 2.5 m/s, more where the 22 / 18 deg/s limits bind: move the preceding gaze key earlier, or move the key shot later inside its hold{later}"), "lead");
            }
            VisibilityFollower.ReadableWindow(trS, trY, trP, k.s, k.yaw, k.pitch, out r.readA, out r.readB);
            double readSec = (r.readB - r.readA) / NocturneConstants.CinematicSpeed;
            if (double.IsNaN(readSec) || readSec < 5.6 - 1e-6)
                Fail(r, "pose", "readable", FormattableString.Invariant($"readable s {F(r.readA, "0.0")}-{F(r.readB, "0.0")} ({F(readSec, "0.0")} s; spec s {r.shot.readA}-{r.shot.readB}) below 5.6 s"),
                     "widen the gaze hold around the key", "lead");

            // reachability
            r.reachS = NocLanes.ProjectS(k.x, k.z, k.s, out _);
            r.reachLateral = NocLanes.Path.Lateral(k.x, k.z, r.reachS);
            r.reachOutside = NocLanes.DistanceOutsideLanes(new UnityEngine.Vector3((float)k.x, 0f, (float)k.z), k.s);
            r.reachInLane = r.reachOutside <= 1e-6;
            r.reachOnPath = Math.Abs(r.reachLateral) < 0.05 && Math.Abs(r.reachS - k.s) < 0.5 && Math.Abs(k.y - NocturneConstants.EyeHeight) < 1e-6;
            if (!r.reachInLane && !r.reachOnPath)
                Fail(r, "eye", "reach", FormattableString.Invariant($"eye {k.x}, {k.z} lies {r.reachOutside:0.00} m outside the lane (lateral {r.reachLateral:0.00} at s {r.reachS:0.00})"),
                     "move the key-shot eye onto the path or widen the lane", "lead");

            var poses = new[] { (k.yaw, k.pitch), (r.yaw40, r.pitch40), (r.yawWalk, r.pitchWalk) };
            foreach (var sub in r.shot.subjects) r.subjects.Add(Subject(r, sub, eye, poses));
            if (r.shot.TDrop)
            {
                foreach (var sr in r.subjects)
                {
                    if (sr.subject.rule == VisRule.Info) continue;
                    double vs = sr.visDeckSpec, vb = sr.visDeckBuilt;
                    if (vs < TDropMin || (!double.IsNaN(vb) && vb < TDropMin))
                    {
                        r.tdropPass = false;
                        Fail(r, sr.subject.name, "T-DROP", FormattableString.Invariant($"unoccluded by deck, rails and edges {F(vs, "0.00")} (spec) / {F(vb, "0.00")} (built) < 0.60"),
                             vs >= TDropMin
                                ? "the built deck / rails deviate from the plan (" + sr.occBuiltBy + "): match 14.4 / 14.5 (for KS13: the glass strip x 521.3-541.3, |z - 195| <= 1.2 is a hole under NocGlass)"
                                : "narrow the deck / rail in front of the subject or move the eye or gaze toward the edge (14.5.3 pier plan)", null);
                    }
                }
            }
            if (r.shot.composition != VisComposition.None) Composition(r, eye);
            if (r.shot.koi) r.koi = Koi(r, eye, poses);
        }

        // ------------------------------------------------------------------ subjects
        static QaView View(KeyShot k, double yaw, double pitch, int a)
            => new QaView(k.x, k.y, k.z, yaw, pitch, k.fov, AspectW[a], AspectH[a]);

        VisSubjectResult Subject(VisShotResult r, VisSubject sub, D3 eye, (double yaw, double pitch)[] poses)
        {
            KeyShot k = r.shot.ks;
            var sr = new VisSubjectResult { subject = sub, proj = new VisProj[poses.Length, AspectNames.Length] };
            for (int p = 0; p < poses.Length; p++)
                for (int a = 0; a < AspectNames.Length; a++)
                {
                    var v = View(k, poses[p].yaw, poses[p].pitch, a);
                    var pr = new VisProj { n = sub.pts.Length };
                    foreach (var pt in sub.pts)
                    {
                        if (!QaProjection.ProjectPoint(v, pt, out double nx, out double ny)) continue;
                        pr.inFront++;
                        pr.x0 = Math.Min(pr.x0, nx); pr.x1 = Math.Max(pr.x1, nx); pr.y0 = Math.Min(pr.y0, ny); pr.y1 = Math.Max(pr.y1, ny);
                        pr.minMargin = Math.Min(pr.minMargin, 1.0 - Math.Max(Math.Abs(nx), Math.Abs(ny)));
                        if (Math.Abs(nx) <= 1.0 && Math.Abs(ny) <= 1.0)
                        {
                            pr.inFrame++;
                            pr.fx0 = Math.Min(pr.fx0, nx); pr.fx1 = Math.Max(pr.fx1, nx); pr.fy0 = Math.Min(pr.fy0, ny); pr.fy1 = Math.Max(pr.fy1, ny);
                        }
                        if (Math.Abs(nx) <= MarginNdc && Math.Abs(ny) <= MarginNdc) pr.inMargin++;
                    }
                    switch (sub.rule)
                    {
                        case VisRule.Whole: pr.pass = pr.inMargin == pr.n; break;
                        case VisRule.Part: pr.pass = pr.inMargin >= Math.Min(VisibilitySubjects.MinPartSamples, pr.n); break;
                        default: pr.pass = true; break;
                    }
                    sr.proj[p, a] = pr;
                }
            // pixel sizes (target pose, 16:9) at 1080p and 4K
            var t0 = sr.proj[0, 0];
            if (t0.HasFront)
            {
                bool whole = sub.rule != VisRule.Part;
                double w = whole ? t0.x1 - t0.x0 : Math.Max(0.0, Math.Min(1.0, t0.fx1) - Math.Max(-1.0, t0.fx0));
                double h = whole ? t0.y1 - t0.y0 : Math.Max(0.0, Math.Min(1.0, t0.fy1) - Math.Max(-1.0, t0.fy0));
                if (!whole && !t0.HasFrame) { w = h = 0.0; }
                sr.wPx1080 = 0.5 * w * 1920; sr.hPx1080 = 0.5 * h * 1080; sr.wPx4k = 2.0 * sr.wPx1080; sr.hPx4k = 2.0 * sr.hPx1080;
            }
            // spec comparison (16:9 target): bounds of the whole subject (Whole / Info) or of its in-frame part (Part)
            sr.specErr = SpecError(sub, t0);
            // occlusion: samples inside the 16:9 frame at the target pose (all in-front samples when none is)
            var v0 = View(k, poses[0].yaw, poses[0].pitch, 0);
            int used = 0, okSpec = 0, okBuilt = 0, okDeckSpec = 0, okDeckBuilt = 0;
            bool tdrop = r.shot.TDrop;
            var bySpec = new Dictionary<string, int>(); var byBuilt = new Dictionary<string, int>();
            bool anyFrame = t0.inFrame > 0;
            foreach (var pt in sub.pts)
            {
                if (!QaProjection.ProjectPoint(v0, pt, out double nx, out double ny)) continue;
                if (anyFrame && (Math.Abs(nx) > 1.0 || Math.Abs(ny) > 1.0)) continue;
                used++;
                if (Visible(Spec, eye, pt, sub.surface, out string ls)) okSpec++; else Count(bySpec, ls);
                if (Built != null) { if (VisibleIgnoring(Built, eye, pt, sub.surface, sub.self, out string lb)) okBuilt++; else Count(byBuilt, lb); }
                if (tdrop)
                {
                    if (Visible(SpecDeck, eye, pt, sub.surface, out _)) okDeckSpec++;
                    if (Built != null && !DeckBandHit(Built, eye, pt, sub.surface)) okDeckBuilt++;
                }
            }
            sr.occSamples = used;
            sr.visSpec = used > 0 ? (double)okSpec / used : double.NaN;
            sr.visBuilt = Built != null && used > 0 ? (double)okBuilt / used : double.NaN;
            sr.occSpecBy = Top(bySpec); sr.occBuiltBy = Top(byBuilt);
            if (tdrop && used > 0)
            {
                sr.visDeckSpec = (double)okDeckSpec / used;
                sr.visDeckBuilt = Built != null ? (double)okDeckBuilt / used : double.NaN;
            }

            // verdict
            var why = new StringBuilder();
            if (sub.rule != VisRule.Info)
            {
                for (int p = 0; p < poses.Length; p++)
                    for (int a = 0; a < AspectNames.Length; a++)
                        if (!sr.proj[p, a].pass)
                        {
                            var q = sr.proj[p, a];
                            why.Append(FormattableString.Invariant($"{PoseNames[p]} {AspectNames[a]}: {q.inMargin}/{q.n} samples inside the 3 % margin (x {F(q.x0, "0.00")}..{F(q.x1, "0.00")}, y {F(q.y0, "0.00")}..{F(q.y1, "0.00")}); "));
                        }
                if (sub.minWidthPx1080 > 0.0 && sr.wPx1080 < sub.minWidthPx1080)
                    why.Append(FormattableString.Invariant($"{sr.wPx1080:0.0} px wide at 1080p < {sub.minWidthPx1080:0.0}; "));
                if (used == 0) why.Append("no sample in front of the camera; ");
            }
            sr.why = why.ToString().TrimEnd(' ', ';');
            sr.pass = sr.why.Length == 0;
            if (!sr.pass)
                Fail(r, sub.name, "frame", sr.why,
                     "move the gaze key (pose) or the subject so that it lies inside the 3 % margin at every aspect and both poses; if the spec subject is unreachable, log a decision (Part 1.6)", null);
            double occ = Built != null ? Math.Min(sr.visSpec, sr.visBuilt) : sr.visSpec;
            bool hiddenWhole = sub.rule == VisRule.Whole && occ < 0.5;
            bool hiddenPart = sub.rule == VisRule.Part && occ <= 0.0;
            if (used > 0 && (hiddenWhole || hiddenPart))
            {
                bool builtOnly = sr.visSpec >= 0.9;
                Fail(r, sub.name, hiddenPart ? "hidden" : "occlusion", FormattableString.Invariant($"only {F(sr.visSpec, "0.00")} (spec) / {F(sr.visBuilt, "0.00")} (built) of the in-frame samples unoccluded; by {sr.occSpecBy} {sr.occBuiltBy}").Trim(),
                     builtOnly
                        ? "the built geometry deviates from the 14.4 / 14.5 plan, which shows the subject: " + sr.occBuiltBy + " must not stand in front of it (match the plan)"
                        : "move or lower the occluder, or move the subject / pose; if the spec subject cannot be seen from the walk, log a decision (Part 1.6, defect 39)" + ShiftHint(k, sub),
                     builtOnly ? OwnersOf(byBuilt) : "lead (spec / pose)");
            }
            return sr;
        }

        /// <summary>Tries the eye 1-6 m along the path (same table yaw / pitch) and names the first shift at which the subject is
        /// >= 90 % unoccluded in the spec model and still inside the 16:9 frame.</summary>
        string ShiftHint(KeyShot k, VisSubject sub)
        {
            foreach (double ds in new[] { 1.0, 2.0, 3.0, 4.0, 6.0, -1.0, -2.0, -3.0, -4.0, -6.0 })
            {
                double s = NocMath.Clamp(k.s + ds, 0.0, NocLanes.Path.Length);
                NocLanes.Path.PointAt(s, out double x, out double z);
                var e = new D3(x, k.y, z);
                var v = new QaView(x, k.y, z, k.yaw, k.pitch, k.fov, AspectW[0], AspectH[0]);
                int n = 0, ok = 0;
                foreach (var pt in sub.pts)
                {
                    if (!QaProjection.ProjectPoint(v, pt, out double nx, out double ny) || Math.Abs(nx) > 1.0 || Math.Abs(ny) > 1.0) continue;
                    n++;
                    if (Visible(Spec, e, pt, sub.surface, out _)) ok++;
                }
                if (n > 0 && ok >= 0.9 * n) return FormattableString.Invariant($"; with the eye at s {s:0.0} ({(ds > 0 ? "+" : "")}{ds:0} m) and the same yaw / pitch it is {(double)ok / n:0.00} unoccluded (spec)");
            }
            return "; no eye shift within +-6 m along the path clears it";
        }

        /// <summary>Owners (Phase-3 ownership table) of the built renderers in a label histogram: Beat01 / 02 G1, Beat03 G2,
        /// Beat04 / 05 G3, Beat06 / 07 G4, the city frozen (lead).</summary>
        static string OwnersOf(Dictionary<string, int> labels)
        {
            var set = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var kv in labels)
            {
                string l = kv.Key ?? "";
                if (l.IndexOf("Beat01", StringComparison.OrdinalIgnoreCase) >= 0 || l.IndexOf("Beat02", StringComparison.OrdinalIgnoreCase) >= 0) set.Add("G1");
                else if (l.IndexOf("Beat03", StringComparison.OrdinalIgnoreCase) >= 0) set.Add("G2");
                else if (l.IndexOf("Beat04", StringComparison.OrdinalIgnoreCase) >= 0 || l.IndexOf("Beat05", StringComparison.OrdinalIgnoreCase) >= 0) set.Add("G3");
                else if (l.IndexOf("Beat06", StringComparison.OrdinalIgnoreCase) >= 0 || l.IndexOf("Beat07", StringComparison.OrdinalIgnoreCase) >= 0) set.Add("G4");
                else if (l.IndexOf("CityField", StringComparison.OrdinalIgnoreCase) >= 0) set.Add("lead (city)");
            }
            return set.Count > 0 ? string.Join(" / ", set) : null;
        }

        static void Count(Dictionary<string, int> d, string label)
        {
            label = label ?? "?";
            d.TryGetValue(label, out int n);
            d[label] = n + 1;
        }

        static string Top(Dictionary<string, int> d)
        {
            if (d.Count == 0) return "";
            var l = new List<KeyValuePair<string, int>>(d);
            l.Sort((a, b) => b.Value.CompareTo(a.Value));
            var sb = new StringBuilder();
            for (int i = 0; i < Math.Min(3, l.Count); i++) sb.Append(i > 0 ? ", " : "").Append(l[i].Key).Append(" x").Append(l[i].Value);
            return sb.ToString();
        }

        /// <summary>Largest deviation of the computed 16:9 bounds from the spec's numbers (NaN when the spec gives none).</summary>
        static double SpecError(VisSubject s, VisProj p)
        {
            if (!p.HasFront) return double.NaN;
            bool part = s.rule == VisRule.Part;
            if (part && !p.HasFrame) return double.NaN;
            double x0 = part ? p.fx0 : p.x0, x1 = part ? p.fx1 : p.x1, y0 = part ? p.fy0 : p.y0, y1 = part ? p.fy1 : p.y1;
            double e = double.NaN;
            e = Err(e, s.sx0, s.sx1, x0, x1);
            e = Err(e, s.sy0, s.sy1, y0, y1);
            return e;
        }

        static double Err(double e, double a, double b, double lo, double hi)
        {
            if (double.IsNaN(a)) return e;
            double d;
            if (Math.Abs(a - b) < 1e-9) d = a < lo ? lo - a : (a > hi ? a - hi : 0.0);    // a single spec value: inside the range
            else d = Math.Max(Math.Abs(lo - a), Math.Abs(hi - b));
            return double.IsNaN(e) ? d : Math.Max(e, d);
        }

        /// <summary>True when nothing of the scene lies between the eye and the sample (self-hit tolerance for surface samples).</summary>
        public static bool Visible(IVisRayScene scene, D3 eye, D3 p, bool surface, out string label)
        {
            label = null;
            if (scene == null) return true;
            D3 d = p - eye;
            double dist = d.Length;
            if (dist < 1e-6) return true;
            double tol = surface ? Math.Max(1.0, 0.03 * dist) : Math.Max(0.25, 0.01 * dist);
            double tMax = dist - tol;
            if (tMax <= 0.0) return true;
            bool hit = scene.Raycast(eye, d * (1.0 / dist), tMax, out _, out _, out label);
            return !hit;
        }

        /// <summary>Visible() that steps over hits on the subject's own built geometry (renderer path containing a self tag).</summary>
        public static bool VisibleIgnoring(IVisRayScene scene, D3 eye, D3 p, bool surface, string[] self, out string label)
        {
            label = null;
            if (self == null || self.Length == 0) return Visible(scene, eye, p, surface, out label);
            D3 d = p - eye;
            double dist = d.Length;
            if (dist < 1e-6) return true;
            D3 u = d * (1.0 / dist);
            double tol = surface ? Math.Max(1.0, 0.03 * dist) : Math.Max(0.25, 0.01 * dist);
            double t0 = 0.0;
            for (int k = 0; k < 16; k++)
            {
                double tMax = dist - tol - t0;
                if (tMax <= 0.0) return true;
                if (!scene.Raycast(eye + u * t0, u, tMax, out double t, out _, out string l)) return true;
                if (!IsSelf(l, self)) { label = l; return false; }
                t0 += t + 0.05;
            }
            return true;
        }

        static bool IsSelf(string label, string[] self)
        {
            if (label == null) return false;
            for (int i = 0; i < self.Length; i++) if (label.IndexOf(self[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        /// <summary>True when a built hit in the deck band (y -1.5..1.25, within 30 m horizontally of the eye: deck slab, rails, tongue
        /// edges) lies between the eye and p; up to 8 nearer hits are stepped over.</summary>
        static bool DeckBandHit(IVisRayScene scene, D3 eye, D3 p, bool surface)
        {
            D3 d = p - eye;
            double dist = d.Length;
            if (dist < 1e-6) return false;
            D3 u = d * (1.0 / dist);
            double tol = surface ? Math.Max(1.0, 0.03 * dist) : Math.Max(0.25, 0.01 * dist);
            double t0 = 0.0;
            for (int k = 0; k < 8; k++)
            {
                D3 o = eye + u * t0;
                double tMax = dist - tol - t0;
                if (tMax <= 0.0) return false;
                if (!scene.Raycast(o, u, tMax, out double t, out _, out _)) return false;
                D3 h = o + u * t;
                double hd = Math.Sqrt((h.x - eye.x) * (h.x - eye.x) + (h.z - eye.z) * (h.z - eye.z));
                if (hd > 30.0) return false;
                if (h.y >= -1.5 && h.y <= 1.25) return true;
                t0 += t + 0.01;
            }
            return false;
        }

        static D3 RayDir(KeyShot k, double yaw, double pitch, double aspect, double nx, double ny)
        {
            QaProjection.Basis(yaw, pitch, out D3 f, out D3 r, out D3 u);
            double tv = Math.Tan(k.fov * 0.5 * NocMath.Deg);
            return (f + r * (nx * tv * aspect) + u * (ny * tv)).Normalized;
        }

        // ------------------------------------------------------------------ composition
        void Composition(VisShotResult r, D3 eye)
        {
            KeyShot k = r.shot.ks;
            switch (r.shot.composition)
            {
                case VisComposition.Drop:
                {
                    // shares: sky, deck, rail, near structure (gate blocks, frames, lamps), drop; the spec's "pier" (KS09), "pier and
                    // rail" (KS10) and "deck" (KS12) are compared with deck + rail, deck + rail and deck
                    r.sharesSpec = DropShares(k, eye, Spec);
                    if (Built != null) { r.sharesBuilt = DropShares(k, eye, Built, m_labels); r.compBuiltBy = Top(m_labels); }
                    var sp = r.shot.specShares;
                    bool deckOnly = r.shot.id == "KS12";
                    CheckDrop(r, r.sharesSpec, sp, deckOnly, "spec model", "composition", "lead");
                    if (r.sharesBuilt != null) CheckDrop(r, r.sharesBuilt, sp, deckOnly, "built", "composition-built", OwnersOf(m_labels));
                    break;
                }
                case VisComposition.Strip:
                {
                    r.sharesSpec = StripShares(k, eye, Spec, out r.stripX0, out r.stripX1, out r.stripY0, out r.stripY1);
                    if (Built != null) r.sharesBuilt = StripShares(k, eye, Built, out _, out _, out _, out _);
                    break;
                }
                case VisComposition.Wall:
                {
                    r.sharesSpec = new[] { WallShare(k, eye, Spec, 0), WallShare(k, eye, Spec, 2) };
                    if (Built != null) r.sharesBuilt = new[] { WallShare(k, eye, Built, 0), WallShare(k, eye, Built, 2) };
                    var sp = r.shot.specShares;
                    if (Math.Abs(r.sharesSpec[0] - sp[0]) > 0.05 || Math.Abs(r.sharesSpec[1] - sp[1]) > 0.05)
                        Fail(r, "wall_share", "composition", FormattableString.Invariant($"wall covers {r.sharesSpec[0]:0.00} (16:9) / {r.sharesSpec[1]:0.00} (21:9) of the frame vs spec {sp[0]:0.00} / {sp[1]:0.00}"),
                             "correct the 14.5.6 share numbers to the ray-cast values (Part 1.6)", "lead");
                    break;
                }
            }
        }

        void CheckDrop(VisShotResult r, double[] s, double[] sp, bool deckOnly, string what, string kind, string owner)
        {
            // KS12 states "deck 30 %": its thin rails (posts, rods) are counted with the drop seen between them
            double pier = deckOnly ? s[1] : s[1] + s[2];
            double drop = deckOnly ? s[4] + s[2] : s[4];
            double e = Math.Max(Math.Abs(pier - sp[1]), Math.Abs(drop - sp[2]));
            string txt = FormattableString.Invariant($"{what} shares sky {s[0]:0.000}, deck {s[1]:0.000}, rail {s[2]:0.000}, near structure {s[3]:0.000}, drop {s[4]:0.000} vs spec sky/{(deckOnly ? "deck" : "pier")}/drop {sp[0]:0.00}/{sp[1]:0.00}/{sp[2]:0.00}");
            if (e > 0.05 || Math.Abs(s[0] - sp[0]) > 0.02)
                Fail(r, "composition", kind, txt, s[3] > 0.02
                    ? "the near structure (gate block / frame) takes part of the drop: move the pose (yaw / s) or narrow the structure; else correct the 14.5 share numbers (Part 1.6)"
                    : "correct the 14.5 share numbers to the ray-cast values (Part 1.6) or change the deck plan / pose", owner);
            if (r.shot.id == "KS10" && s[4] < 0.40)
                Fail(r, "composition", kind, FormattableString.Invariant($"{what}: KS10 drop {s[4]:0.000} < 0.40 (14.5.3 acceptance 2); near structure {s[3]:0.000}"),
                     "keep gate block A out of the KS10 frame (yaw / s) or narrow the pier / pitch further down" + (owner == "lead" ? DropPoseHint(r) : ""), owner ?? "G2");
        }

        readonly Dictionary<string, int> m_labels = new Dictionary<string, int>();

        /// <summary>Searches yaw offsets -40..+20 deg and pitch offsets -8..+6 deg (spec model) for the pose nearest the table pose with
        /// a drop share >= 0.40 and every Whole subject inside the 3 % margin at the three aspects.</summary>
        string DropPoseHint(VisShotResult r)
        {
            KeyShot k0 = r.shot.ks;
            var eye = new D3(k0.x, k0.y, k0.z);
            double best = double.MaxValue, by = 0, bp = 0, bd = 0;
            for (double dy = -40.0; dy <= 20.0 + 1e-9; dy += 5.0)
                for (double dp = -8.0; dp <= 6.0 + 1e-9; dp += 2.0)
                {
                    double cost = Math.Abs(dy) + 2.0 * Math.Abs(dp);
                    if (cost >= best) continue;
                    KeyShot k = k0; k.yaw = k0.yaw + dy; k.pitch = k0.pitch + dp;
                    double[] sh = DropShares(k, eye, Spec);
                    if (sh[4] < 0.40) continue;
                    bool inside = true;
                    foreach (var sub in r.shot.subjects)
                    {
                        if (sub.rule != VisRule.Whole) continue;
                        for (int a = 0; a < AspectNames.Length && inside; a++)
                        {
                            var v = View(k, k.yaw, k.pitch, a);
                            foreach (var pt in sub.pts)
                                if (!QaProjection.ProjectPoint(v, pt, out double nx, out double ny) || Math.Abs(nx) > MarginNdc || Math.Abs(ny) > MarginNdc) { inside = false; break; }
                        }
                    }
                    if (!inside) continue;
                    best = cost; by = k.yaw; bp = k.pitch; bd = sh[4];
                }
            if (best == double.MaxValue) return "; no yaw -40..+20 / pitch -8..+6 change of the key reaches drop 0.40 with the subjects in frame";
            return FormattableString.Invariant($"; nearest key pose with drop >= 0.40 and every whole subject in frame (spec model): world yaw {NocMath.WrapDeg(by):0}, pitch {bp:0} (drop {bd:0.000}; gaze key rel yaw {NocMath.WrapDeg(by - NocMath.Heading(k0.s) / NocMath.Deg):0})");
        }

        /// <summary>Class of a built occluder by its renderer path (Phase-3 builder naming): deck / floor / street / plaza / hull / truss
        /// = deck, rail / balustrade / post / rod / barrier / planter = rail, anything else near the eye (gate blocks, frames, lamps) =
        /// structure.</summary>
        public static VisClass BuiltClass(string label)
        {
            if (string.IsNullOrEmpty(label)) return VisClass.Structure;
            string l = label.ToLowerInvariant();
            int cut = l.LastIndexOf('/');
            string leaf = cut >= 0 ? l.Substring(cut + 1) : l;
            if (leaf.Contains("rail") || leaf.Contains("balustrade") || leaf.Contains("post") || leaf.Contains("rod") || leaf.Contains("barrier") || leaf.Contains("planter")) return VisClass.Rail;
            if (leaf.Contains("deck") || leaf.Contains("floor") || leaf.Contains("street") || leaf.Contains("plaza") || leaf.Contains("hull") || leaf.Contains("truss") || leaf.Contains("skirt")) return VisClass.Deck;
            return VisClass.Structure;
        }

        /// <summary>Shares of the 16:9 frame: sky (elevation > 0), deck, rail, near structure, drop (the rest below the horizon). Spec
        /// scene: by the occluder class (every spec structure is a walk-side structure: gate blocks, frames). Built scene: a hit within
        /// NearStructure (60 m) horizontally of the eye counts as deck, rail or near structure by its renderer path (BuiltClass); farther
        /// hits (Gulf faces, cross bridges, layers) are the drop.</summary>
        double[] DropShares(KeyShot k, D3 eye, IVisRayScene scene, Dictionary<string, int> labels = null)
        {
            int sky = 0, deck = 0, rail = 0, near = 0, drop = 0;
            labels?.Clear();
            double asp = 16.0 / 9.0;
            for (int j = 0; j < GridY; j++)
                for (int i = 0; i < GridX; i++)
                {
                    double nx = -1.0 + (2.0 * i + 1.0) / GridX, ny = -1.0 + (2.0 * j + 1.0) / GridY;
                    D3 d = RayDir(k, k.yaw, k.pitch, asp, nx, ny);
                    if (d.y > 0.0) { sky++; continue; }
                    if (scene.Raycast(eye, d, 20000.0, out double t, out VisClass c, out string lab))
                    {
                        D3 h = eye + d * t;
                        double hd = Math.Sqrt((h.x - eye.x) * (h.x - eye.x) + (h.z - eye.z) * (h.z - eye.z));
                        if (labels != null && (c != VisClass.Built || hd < NearStructure)) Count(labels, lab);
                        if (c == VisClass.Deck) { deck++; continue; }
                        if (c == VisClass.Rail) { rail++; continue; }
                        if (c == VisClass.Structure) { near++; continue; }
                        if (c == VisClass.Built && hd < NearStructure)
                        {
                            switch (BuiltClass(lab)) { case VisClass.Deck: deck++; break; case VisClass.Rail: rail++; break; default: near++; break; }
                            continue;
                        }
                    }
                    drop++;
                }
            double n = GridX * GridY;
            return new[] { sky / n, deck / n, rail / n, near / n, drop / n };
        }

        /// <summary>KS13: shares of deck, strip (rays through the glass strip to the abyss) and other, plus the strip's NDC bounds.</summary>
        double[] StripShares(KeyShot k, D3 eye, IVisRayScene scene, out double x0, out double x1, out double y0, out double y1)
        {
            int deck = 0, strip = 0, other = 0;
            x0 = y0 = double.MaxValue; x1 = y1 = double.MinValue;
            double asp = 16.0 / 9.0;
            for (int j = 0; j < GridY; j++)
                for (int i = 0; i < GridX; i++)
                {
                    double nx = -1.0 + (2.0 * i + 1.0) / GridX, ny = -1.0 + (2.0 * j + 1.0) / GridY;
                    D3 d = RayDir(k, k.yaw, k.pitch, asp, nx, ny);
                    bool hit = scene.Raycast(eye, d, 20000.0, out double t, out VisClass c, out _);
                    double tp = d.y < 0 ? -eye.y / d.y : double.MaxValue;
                    // through the strip: the ray crosses y 0 inside the strip and nothing stops it within the 0.5 m deck slab
                    bool throughStrip = d.y < 0 && VisibilitySpecWorld.InStrip(eye.x + tp * d.x, eye.z + tp * d.z) && (!hit || t > tp + 0.6 / Math.Max(0.05, -d.y));
                    if (throughStrip)
                    {
                        strip++;
                        x0 = Math.Min(x0, nx); x1 = Math.Max(x1, nx); y0 = Math.Min(y0, ny); y1 = Math.Max(y1, ny);
                    }
                    else if (hit && (c == VisClass.Deck || c == VisClass.Rail || (c == VisClass.Built && t < 30.0))) deck++;
                    else other++;
                }
            double n = GridX * GridY;
            if (strip == 0) { x0 = x1 = y0 = y1 = double.NaN; }
            return new[] { deck / n, strip / n, other / n };
        }

        /// <summary>KS16: share of the frame whose first hit is the screen-wall face (x 874 +- 0.5, z 275..520).</summary>
        double WallShare(KeyShot k, D3 eye, IVisRayScene scene, int aspect)
        {
            int gx = aspect == 2 ? (int)Math.Round(GridY * 2560.0 / 1080.0) : GridX;
            double asp = AspectW[aspect] / (double)AspectH[aspect];
            int wall = 0;
            for (int j = 0; j < GridY; j++)
                for (int i = 0; i < gx; i++)
                {
                    double nx = -1.0 + (2.0 * i + 1.0) / gx, ny = -1.0 + (2.0 * j + 1.0) / GridY;
                    D3 d = RayDir(k, k.yaw, k.pitch, asp, nx, ny);
                    if (!scene.Raycast(eye, d, 20000.0, out double t, out _, out _)) continue;
                    D3 h = eye + d * t;
                    if (Math.Abs(h.x - 874.0) < 0.5 && h.z >= 275.0 && h.z <= 520.0) wall++;
                }
            return wall / (double)(gx * GridY);
        }

        // ------------------------------------------------------------------ koi
        VisKoiResult Koi(VisShotResult r, D3 eye, (double yaw, double pitch)[] poses)
        {
            KeyShot k = r.shot.ks;
            var res = new VisKoiResult { aspects = new VisKoiAspect[poses.Length, AspectNames.Length] };
            for (int p = 0; p < poses.Length; p++) for (int a = 0; a < AspectNames.Length; a++) res.aspects[p, a] = new VisKoiAspect { aspect = AspectNames[a] };
            double phiH = KoiMath.HeadAzimuth(KoiMath.PhaseU(k.s));
            var pts = new D3[KoiMath.SampleCount];
            var hidden = new bool[KoiMath.SampleCount];
            var views = new QaView[poses.Length, AspectNames.Length];
            for (int p = 0; p < poses.Length; p++) for (int a = 0; a < AspectNames.Length; a++) views[p, a] = View(k, poses[p].yaw, poses[p].pitch, a);
            double focal4k = QaProjection.FocalPx(k.fov, 2160);
            int minBodyIn = int.MaxValue;
            var bodyIn = new int[poses.Length, AspectNames.Length];
            for (int ph = 0; ph < KoiMath.WavePhases; ph++)
            {
                double tw = KoiMath.PhaseWaveTime(ph);
                KoiMath.Samples(phiH, tw, pts);
                Array.Clear(bodyIn, 0, bodyIn.Length);
                int spire = 0, arcade = 0, other = 0, built = 0;
                for (int i = 0; i < pts.Length; i++)
                {
                    bool vs = Visible(Spec, eye, pts[i], false, out string ls);
                    bool vb = Built == null || Visible(Built, eye, pts[i], false, out _);
                    hidden[i] = !vs || !vb;
                    if (KoiMath.IsCore(i))
                    {
                        if (!vs) { if (ls == "spire") spire++; else if (ls == "arcade") arcade++; else other++; }
                        if (!vb) built++;
                    }
                    if (KoiMath.SampleKind(i) == KoiPart.Head && hidden[i]) { res.headOccluded++; if (res.headOccluder.Length == 0) res.headOccluder = !vs ? (ls ?? "?") + " (spec)" : "built geometry"; }
                }
                res.maxSpire = Math.Max(res.maxSpire, spire); res.maxArcade = Math.Max(res.maxArcade, arcade);
                res.maxOther = Math.Max(res.maxOther, other); res.maxBuilt = Math.Max(res.maxBuilt, built);
                res.sumSpire += spire; res.sumArcade += arcade;
                bool rootPec = RootHidden(eye, KoiMath.PectoralA0 + 0.5 * KoiMath.PectoralChord, phiH, tw);
                bool rootDor = RootHidden(eye, KoiMath.DorsalA0 + 0.5 * KoiMath.DorsalLength, phiH, tw);
                bool rootVeil = RootHidden(eye, KoiMath.VeilA0, phiH, tw);
                // body thickness in pixels at 4K (stations: top-bottom diameter / distance)
                for (int st = 0; st < KoiMath.Stations; st++)
                {
                    D3 top = pts[st * 4 + 1], bot = pts[st * 4 + 3];
                    D3 c = (top + bot) * 0.5;
                    double dist = (c - eye).Length;
                    double px = focal4k * (top - bot).Length / Math.Max(1.0, dist);
                    res.minThick4k = Math.Min(res.minThick4k, px); res.maxThick4k = Math.Max(res.maxThick4k, px);
                }
                for (int p = 0; p < poses.Length; p++)
                    for (int a = 0; a < AspectNames.Length; a++)
                    {
                        var ka = res.aspects[p, a];
                        var v = views[p, a];
                        for (int i = 0; i < pts.Length; i++)
                        {
                            KoiPart kind = KoiMath.SampleKind(i);
                            if (!r.shot.koiWhole && !(r.shot.koiBody ? KoiMath.IsCore(i) || kind == KoiPart.Head : kind == KoiPart.Head)) continue;
                            if (r.shot.koiBody && kind != KoiPart.Head && QaProjection.ProjectPoint(v, pts[i], out double bx, out double by)
                                && !hidden[i] && Math.Abs(bx) <= MarginNdc && Math.Abs(by) <= MarginNdc) bodyIn[p, a]++;
                            if (r.shot.koiBody && kind != KoiPart.Head)
                            {
                                // D-029 (GB-01): the body is a partial subject: samples outside the frame or hidden are fine, the
                                // visible ones are measured; the head is reported (hx / hy) but never a failure
                                if (hidden[i] || !QaProjection.ProjectPoint(v, pts[i], out double cx, out double cy)) continue;
                                double cm = 1.0 - Math.Max(Math.Abs(cx), Math.Abs(cy));
                                ka.minMarginAll = Math.Min(ka.minMarginAll, cm);
                                ka.minMarginCore = Math.Min(ka.minMarginCore, cm);
                                if (cm < 0.0) continue;
                                ka.x0 = Math.Min(ka.x0, cx); ka.x1 = Math.Max(ka.x1, cx); ka.y0 = Math.Min(ka.y0, cy); ka.y1 = Math.Max(ka.y1, cy);
                                if (cm < ka.minMargin) { ka.minMargin = cm; ka.worstPhase = ph; }
                                continue;
                            }
                            if (!QaProjection.ProjectPoint(v, pts[i], out double nx, out double ny)) { if (!hidden[i] && !r.shot.koiBody) { ka.outside++; ka.minMargin = Math.Min(ka.minMargin, -1.0); } continue; }
                            double m = 1.0 - Math.Max(Math.Abs(nx), Math.Abs(ny));
                            ka.minMarginAll = Math.Min(ka.minMarginAll, m);
                            if (KoiMath.IsCore(i)) ka.minMarginCore = Math.Min(ka.minMarginCore, m);
                            if (kind == KoiPart.Head)
                            {
                                ka.hx0 = Math.Min(ka.hx0, nx); ka.hx1 = Math.Max(ka.hx1, nx); ka.hy0 = Math.Min(ka.hy0, ny); ka.hy1 = Math.Max(ka.hy1, ny);
                                ka.headMargin = Math.Min(ka.headMargin, m);
                            }
                            bool exempt = hidden[i];
                            if (!exempt && (kind == KoiPart.Pectoral || kind == KoiPart.Dorsal || kind == KoiPart.Veil))
                            {
                                // D-009: a fin sample whose body root (the spine point at its root arc length) is hidden is exempt
                                bool rootHidden = kind == KoiPart.Pectoral ? rootPec : (kind == KoiPart.Dorsal ? rootDor : rootVeil);
                                if (rootHidden) { exempt = true; if (p == 0 && a == 0) res.exemptFins++; }
                            }
                            if (exempt || r.shot.koiBody) continue;
                            ka.x0 = Math.Min(ka.x0, nx); ka.x1 = Math.Max(ka.x1, nx); ka.y0 = Math.Min(ka.y0, ny); ka.y1 = Math.Max(ka.y1, ny);
                            if (m < ka.minMargin) { ka.minMargin = m; ka.worstPhase = ph; }
                            if (m < 1.0 - MarginNdc) ka.outside++;
                        }
                    }
                if (r.shot.koiBody)
                    for (int p = 0; p < poses.Length; p++)
                        for (int a = 0; a < AspectNames.Length; a++) minBodyIn = Math.Min(minBodyIn, bodyIn[p, a]);
            }
            res.minBodyIn = r.shot.koiBody ? minBodyIn : -1;
            var why = new StringBuilder();
            for (int p = 0; p < poses.Length; p++)
                for (int a = 0; a < AspectNames.Length; a++)
                {
                    var ka = res.aspects[p, a];
                    if (ka.outside > 0)
                        why.Append(FormattableString.Invariant($"{PoseNames[p]} {ka.aspect}: {ka.outside} visible sample-phases outside the 3 % margin (min margin {F(ka.minMargin)} at phase {ka.worstPhase}); "));
                }
            if (res.headOccluded > 0 && !r.shot.koiBody) why.Append(FormattableString.Invariant($"head occluded in {res.headOccluded} of {KoiMath.HeadCount * KoiMath.WavePhases} sample-phases (by {res.headOccluder}){HeadGeometry(eye, phiH)}; "));
            if (r.shot.koiBody && res.minBodyIn < VisibilitySubjects.MinPartSamples)
                why.Append(FormattableString.Invariant($"koi body: only {res.minBodyIn} core samples visible inside the 3 % margin in the worst phase / pose / aspect (>= {VisibilitySubjects.MinPartSamples}); "));
            if (r.shot.koiBody && res.headOccluded > 0)
                Notes.Add(FormattableString.Invariant($"{r.shot.id}: the koi head is hidden in {res.headOccluded} of {KoiMath.HeadCount * KoiMath.WavePhases} sample-phases (by {res.headOccluder}){HeadGeometry(eye, phiH)}; not a subject (D-029 GB-01)"));
            res.why = why.ToString().TrimEnd(' ', ';');
            res.pass = res.why.Length == 0;
            if (!res.pass)
                Fail(r, r.shot.koiWhole ? "koi" : (r.shot.koiBody ? "koi_body" : "koi_head"), "koi-lint", res.why,
                     res.headOccluded > 0
                        ? "the occlusion does not depend on yaw or pitch (the eye is fixed): remove the koi head from the shot's subjects, or move the head (KoiMath phase table) above the occluder's top edge, or lower the occluder; a logged decision (Part 1.6, defect 39)"
                        : "move the gaze key (pitch / yaw) of the shot, or the koi phase table (KoiMath HeadDeg0 / span, a logged decision) so the whole koi stays inside the 3 % margin at all aspects",
                     r.shot.koiWhole ? "lead (gaze) / V (koi)" : "lead");
            return res;
        }

        /// <summary>Elevation of the koi head (spine at a = 0, phase 0) against the top edge of the spec occluder along its azimuth.</summary>
        string HeadGeometry(D3 eye, double phiH)
        {
            D3 h = KoiMath.SpinePoint(0.0, phiH, 0.0);
            D3 d = h - eye;
            double hd = Math.Sqrt(d.x * d.x + d.z * d.z);
            double eHead = Math.Atan2(d.y, hd) / NocMath.Deg, az = Math.Atan2(d.x, d.z);
            double eTop = double.NaN;
            for (double e = eHead; e < 89.0; e += 0.1)
            {
                double er = e * NocMath.Deg;
                var dir = new D3(Math.Cos(er) * Math.Sin(az), Math.Sin(er), Math.Cos(er) * Math.Cos(az));
                if (!Spec.Raycast(eye, dir, 2000.0, out double t, out _, out _) || t > 0.9 * d.Length) { eTop = e; break; }
            }
            return FormattableString.Invariant($": the head is at elevation {eHead:0.0} deg, {d.Length:0} m away; along its azimuth the occluder's top edge is at {F(eTop, "0.0")} deg");
        }

        bool RootHidden(D3 eye, double a, double phiH, double tw)
        {
            D3 c = KoiMath.SpinePoint(a, phiH, tw);
            return !Visible(Spec, eye, c, false, out _) || (Built != null && !Visible(Built, eye, c, false, out _));
        }

        // ------------------------------------------------------------------ pre-reveal koi (D-029 GB-03)
        /// <summary>
        /// The koi (and, for information, the spire) seen before the KS17 reveal: every key shot KS04-KS16 at its target pose (16:9,
        /// 16:10, 21:9) and every beat 2-6 still of ShotPlan.BeatStills (16:9), the 183 KoiMath samples over the 20 wave phases at
        /// the koi phase of the pose's s (u = (s - 1 260) / 240, the capture-mode phase; at any constant walking speed the play phase
        /// is the same), ray-cast against the built meshes (the spec model when no built world is given). A sample-phase counts when
        /// it projects inside the frame and nothing opaque stands between; its sight line is classified by the opening it passes.
        /// </summary>
        void RunPreReveal()
        {
            IVisRayScene scene = Built ?? Spec;
            var pts = new D3[KoiMath.SampleCount];
            var poses = new List<VisPreReveal>();
            var views = new List<QaView[]>();
            foreach (var k in NocturneConstants.KeyShots)
            {
                if (k.s < 250.0 - 1e-6 || k.s >= PreRevealS1) continue;      // KS04 .. KS16
                var pr = new VisPreReveal { tag = k.tag, kind = "key", s = k.s, x = k.x, y = k.y, z = k.z, yaw = k.yaw, pitch = k.pitch, fov = k.fov };
                var vv = new QaView[AspectNames.Length];
                for (int a = 0; a < vv.Length; a++) vv[a] = View(k, k.yaw, k.pitch, a);
                poses.Add(pr); views.Add(vv);
            }
            foreach (var st in ShotPlan.BeatStills())
            {
                if (st.s < PreRevealS0 || st.s >= PreRevealS1) continue;        // beats 2-6
                poses.Add(new VisPreReveal { tag = st.tag, kind = "still", s = st.s, x = st.x, y = st.y, z = st.z, yaw = st.yaw, pitch = st.pitch, fov = st.fov });
                views.Add(new[] { st.View });
            }
            for (int q = 0; q < poses.Count; q++)
            {
                var pr = poses[q];
                var eye = new D3(pr.x, pr.y, pr.z);
                double phiH = KoiMath.HeadAzimuth(KoiMath.PhaseU(pr.s));
                for (int ph = 0; ph < KoiMath.WavePhases; ph++)
                {
                    KoiMath.Samples(phiH, KoiMath.PhaseWaveTime(ph), pts);
                    for (int i = 0; i < pts.Length; i++)
                    {
                        bool inFrame = false; double fx = 0.0, fy = 0.0;
                        foreach (var v in views[q])
                            if (QaProjection.ProjectPoint(v, pts[i], out double nx, out double ny) && Math.Abs(nx) <= 1.0 && Math.Abs(ny) <= 1.0)
                            { inFrame = true; fx = nx; fy = ny; break; }
                        if (!inFrame) continue;
                        pr.inFrame++;
                        if (!Visible(scene, eye, pts[i], false, out _)) continue;
                        pr.visible++;
                        pr.x0 = Math.Min(pr.x0, fx); pr.x1 = Math.Max(pr.x1, fx); pr.y0 = Math.Min(pr.y0, fy); pr.y1 = Math.Max(pr.y1, fy);
                        switch (Opening(eye, pts[i]))
                        {
                            case 1: pr.viaNotch++; break;
                            case 2: pr.viaPromenade++; break;
                            case 3: pr.viaArcade++; break;
                            default: pr.viaOther++; break;
                        }
                        if (pr.example.Length == 0)
                            pr.example = FormattableString.Invariant($"sample {i} ({KoiMath.SampleKind(i)}) at ({pts[i].x:0}, {pts[i].y:0}, {pts[i].z:0}), phase {ph}, NDC ({fx:0.00}, {fy:0.00})");
                    }
                }
                // the spire (information): tangent-silhouette points of the shaft every 37 m from y 0 to 560
                foreach (var sp in SpirePoints(eye))
                {
                    bool inFrame = false;
                    foreach (var v in views[q])
                        if (QaProjection.ProjectPoint(v, sp, out double nx, out double ny) && Math.Abs(nx) <= 1.0 && Math.Abs(ny) <= 1.0) { inFrame = true; break; }
                    if (!inFrame) continue;
                    pr.spireInFrame++;
                    if (!VisibleIgnoring(scene, eye, sp, true, new[] { "spire" }, out _)) continue;
                    pr.spireVisible++;
                    if (Opening(eye, sp) == 1) pr.spireViaNotch++;
                }
                PreReveal.Add(pr);
                if (pr.visible > 0)
                    Failures.Add(new VisFailure
                    {
                        shot = pr.tag, subject = "koi_pre_reveal", kind = "pre-reveal", owner = "GE",
                        detail = FormattableString.Invariant($"s {pr.s:0.0}: {pr.visible} koi sample-phases visible ({pr.inFrame} in frame; via the garden notch {pr.viaNotch}, the promenade gap {pr.viaPromenade}, over the arcade {pr.viaArcade}, other {pr.viaOther}); first {pr.example}"),
                        fix = "close the sight line with structure (GE gate structures) or park the koi (KO, KoiMath) before the KS17 reveal (D-029 GB-03)",
                    });
            }
        }

        /// <summary>Which opening the sight line eye -> p passes: 1 the garden notch of face B (x 815.3, z 115..275), 2 the promenade
        /// gap (z 275, x 874..914), 3 over the arcade (x 914, z 236..360, y >= 38), 0 other.</summary>
        static int Opening(D3 eye, D3 p)
        {
            D3 d = p - eye;
            if (Math.Abs(d.x) > 1e-9)
            {
                double t = (EdgeDims.FaceXB - eye.x) / d.x;
                if (t > 0.0 && t < 1.0)
                {
                    double y = eye.y + t * d.y, z = eye.z + t * d.z;
                    if (z >= EdgeDims.NotchZ0 && z <= EdgeDims.NotchZ1 && y >= -1.0 && y <= EdgeDims.FaceTop) return 1;
                }
            }
            if (Math.Abs(d.z) > 1e-9)
            {
                double t = (EdgeDims.NotchZ1 - eye.z) / d.z;
                if (t > 0.0 && t < 1.0)
                {
                    double x = eye.x + t * d.x;
                    if (x >= SignsLayout.PromX0 && x <= SignsLayout.PromX1) return 2;
                }
            }
            if (Math.Abs(d.x) > 1e-9)
            {
                double t = (SignsLayout.ArcX0 - eye.x) / d.x;
                if (t > 0.0 && t < 1.0)
                {
                    double y = eye.y + t * d.y, z = eye.z + t * d.z;
                    if (z >= SignsLayout.ArcZ0 && z <= SignsLayout.ArcZ1 && y >= SignsLayout.ArcH) return 3;
                }
            }
            return 0;
        }

        static List<D3> SpirePoints(D3 eye)
        {
            var l = new List<D3>(48);
            double dx = NocturneConstants.SpireX - eye.x, dz = NocturneConstants.SpireZ - eye.z, dh = Math.Sqrt(dx * dx + dz * dz);
            double azc = Math.Atan2(dx, dz);
            for (int j = 0; j < 16; j++)
            {
                double y = 560.0 * j / 15.0, r = VisibilitySpecWorld.SpireRadius(y);
                double delta = Math.Acos(Math.Min(1.0, r / Math.Max(r + 1e-6, dh)));
                for (int side = -1; side <= 1; side++)
                {
                    double a = azc + Math.PI + side * delta * 0.98;
                    l.Add(new D3(NocturneConstants.SpireX + r * Math.Sin(a), y, NocturneConstants.SpireZ + r * Math.Cos(a)));
                }
            }
            return l;
        }

        // ------------------------------------------------------------------ reports
        public string Json()
        {
            var j = new QaJson();
            j.BeginObject();
            j.Prop("audit", "11.13 visibility and reachability (MASTER_PROMPT 11.13, 14.10)");
            j.Prop("generated", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
            j.Prop("seconds", Seconds);
            j.Prop("built", BuiltInfo);
            j.Prop("marginNdc", MarginNdc).Prop("lagFlagDeg", LagFlag).Prop("tdropMin", TDropMin);
            j.Prop("aspects", new List<string>(AspectNames)).Prop("poses", new List<string>(PoseNames));
            j.Prop("occluders", "spec = VisibilitySpecWorld (decks, rails, gates, alley, facades, spire, arcade, screen wall, Old Quarter); built = opaque MeshRenderers of every IBeatBuilder incl. CityField; instanced signs, streaks, traffic, beacons, glass, koi and the sky dome are not occluders");
            j.Key("notes").BeginArray();
            foreach (var n in Notes) j.Value(n);
            j.EndArray();
            j.Key("shots").BeginArray();
            foreach (var r in Results) ShotJson(j, r);
            j.EndArray();
            j.Key("preReveal").BeginArray();
            foreach (var p in PreReveal)
            {
                j.BeginObject(true).Prop("tag", p.tag).Prop("kind", p.kind).Prop("s", p.s).Prop("yaw", p.yaw).Prop("pitch", p.pitch)
                 .Prop("koiInFrame", p.inFrame).Prop("koiVisible", p.visible).Prop("viaNotch", p.viaNotch).Prop("viaPromenade", p.viaPromenade)
                 .Prop("viaArcade", p.viaArcade).Prop("viaOther", p.viaOther).Prop("spireInFrame", p.spireInFrame).Prop("spireVisible", p.spireVisible)
                 .Prop("spireViaNotch", p.spireViaNotch);
                if (p.visible > 0) j.Key("ndcVisible").BeginArray(true).Value(p.x0).Value(p.x1).Value(p.y0).Value(p.y1).EndArray();
                if (p.example.Length > 0) j.Prop("example", p.example);
                j.EndObject();
            }
            j.EndArray();
            j.Key("failures").BeginArray();
            foreach (var f in Failures)
                j.BeginObject().Prop("shot", f.shot).Prop("subject", f.subject).Prop("kind", f.kind).Prop("detail", f.detail).Prop("fix", f.fix).Prop("owner", f.owner).EndObject();
            j.EndArray();
            j.EndObject();
            return j.ToString();
        }

        static void ShotJson(QaJson j, VisShotResult r)
        {
            KeyShot k = r.shot.ks;
            j.BeginObject();
            j.Prop("id", r.shot.id).Prop("tag", k.tag).Prop("s", k.s).Prop("owner", r.shot.owner);
            j.Prop("eye", k.x, k.y, k.z);
            j.Key("target").BeginObject(true).Prop("yaw", k.yaw).Prop("pitch", k.pitch).Prop("vfov", k.fov).EndObject();
            j.Key("schedule").BeginObject(true).Prop("yaw", NocMath.WrapDeg(r.schedYaw)).Prop("pitch", r.schedPitch).Prop("vfov", r.schedFov).EndObject();
            j.Key("follower40").BeginObject(true).Prop("yaw", NocMath.WrapDeg(r.yaw40)).Prop("pitch", r.pitch40).Prop("lagDeg", r.lag40).EndObject();
            j.Key("followerWalk").BeginObject(true).Prop("yaw", NocMath.WrapDeg(r.yawWalk)).Prop("pitch", r.pitchWalk).Prop("lagDeg", r.lagWalk).EndObject();
            j.Prop("lagFlag", r.lag40 > LagFlag || r.lagWalk > LagFlag);
            j.Key("readable").BeginObject(true).Prop("s0", r.readA).Prop("s1", r.readB).Prop("seconds", (r.readB - r.readA) / 2.5).Prop("spec0", r.shot.readA).Prop("spec1", r.shot.readB).EndObject();
            j.Key("reach").BeginObject(true).Prop("s", r.reachS).Prop("lateral", r.reachLateral).Prop("outsideLane", r.reachOutside).Prop("inLane", r.reachInLane).Prop("onPath", r.reachOnPath).EndObject();
            if (r.shot.TDrop) j.Prop("tdrop", r.tdropPass);
            if (r.sharesSpec != null) j.Prop("sharesSpec", r.sharesSpec);
            if (r.sharesBuilt != null) j.Prop("sharesBuilt", r.sharesBuilt);
            if (r.compBuiltBy.Length > 0) j.Prop("compositionBuiltBy", r.compBuiltBy);
            if (r.shot.specShares != null) j.Prop("sharesSpecText", r.shot.specShares);
            if (!double.IsNaN(r.stripX0)) j.Key("stripNdc").BeginArray(true).Value(r.stripX0).Value(r.stripX1).Value(r.stripY0).Value(r.stripY1).EndArray();
            j.Key("subjects").BeginArray();
            foreach (var s in r.subjects)
            {
                var sub = s.subject;
                j.BeginObject();
                j.Prop("name", sub.name).Prop("rule", sub.rule.ToString()).Prop("samples", sub.pts.Length).Prop("pass", s.pass);
                if (s.why.Length > 0) j.Prop("why", s.why);
                j.Prop("spec", sub.spec);
                if (!string.IsNullOrEmpty(sub.note)) j.Prop("note", sub.note);
                j.Key("specNdc").BeginArray(true).Value(sub.sx0).Value(sub.sx1).Value(sub.sy0).Value(sub.sy1).EndArray();
                j.Prop("specErr", s.specErr);
                j.Prop("unoccludedSpec", s.visSpec).Prop("unoccludedBuilt", s.visBuilt).Prop("occlusionSamples", s.occSamples);
                if (s.occSpecBy.Length > 0) j.Prop("occludedBySpec", s.occSpecBy);
                if (s.occBuiltBy.Length > 0) j.Prop("occludedByBuilt", s.occBuiltBy);
                j.Key("px1080").BeginArray(true).Value(s.wPx1080).Value(s.hPx1080).EndArray();
                j.Key("px4k").BeginArray(true).Value(s.wPx4k).Value(s.hPx4k).EndArray();
                j.Key("projections").BeginArray();
                for (int p = 0; p < PoseNames.Length; p++)
                    for (int a = 0; a < AspectNames.Length; a++)
                    {
                        var q = s.proj[p, a];
                        j.BeginObject(true).Prop("pose", PoseNames[p]).Prop("aspect", AspectNames[a]).Prop("inFront", q.inFront).Prop("inFrame", q.inFrame).Prop("inMargin", q.inMargin).Prop("pass", q.pass);
                        if (q.HasFront) j.Key("ndc").BeginArray(true).Value(q.x0).Value(q.x1).Value(q.y0).Value(q.y1).EndArray();
                        if (q.HasFrame) j.Key("ndcInFrame").BeginArray(true).Value(q.fx0).Value(q.fx1).Value(q.fy0).Value(q.fy1).EndArray();
                        j.EndObject();
                    }
                j.EndArray();
                j.EndObject();
            }
            j.EndArray();
            if (r.koi != null)
            {
                var kr = r.koi;
                j.Key("koi").BeginObject();
                j.Prop("whole", r.shot.koiWhole).Prop("pass", kr.pass);
                if (kr.why.Length > 0) j.Prop("why", kr.why);
                j.Prop("specMinMargin", r.shot.koiSpecMargin);
                if (r.shot.koiSpecX != null) j.Prop("specX", r.shot.koiSpecX);
                j.Key("specY").BeginArray(true).Value(r.shot.koiSpecY0).Value(r.shot.koiSpecY1).EndArray();
                j.Key("specHead").BeginArray(true).Value(r.shot.headX0).Value(r.shot.headX1).Value(r.shot.headY0).Value(r.shot.headY1).EndArray();
                j.Prop("coreHiddenMaxSpire", kr.maxSpire).Prop("coreHiddenMaxArcade", kr.maxArcade).Prop("coreHiddenMaxOther", kr.maxOther).Prop("coreHiddenMaxBuilt", kr.maxBuilt);
                j.Prop("coreHiddenMeanSpire", kr.sumSpire / 20.0).Prop("coreHiddenMeanArcade", kr.sumArcade / 20.0);
                j.Prop("headOccluded", kr.headOccluded).Prop("exemptFinSamples", kr.exemptFins);
                j.Key("thickness4kPx").BeginArray(true).Value(kr.minThick4k).Value(kr.maxThick4k).EndArray();
                j.Key("aspects").BeginArray();
                for (int p = 0; p < PoseNames.Length; p++)
                    for (int a = 0; a < AspectNames.Length; a++)
                    {
                        var ka = kr.aspects[p, a];
                        j.BeginObject(true).Prop("pose", PoseNames[p]).Prop("aspect", ka.aspect)
                         .Prop("minMargin", ka.minMargin).Prop("minMarginAll", ka.minMarginAll).Prop("minMarginCore", ka.minMarginCore).Prop("outside", ka.outside)
                         .Prop("worstPhase", ka.worstPhase);
                        j.Key("ndcVisible").BeginArray(true).Value(ka.x0).Value(ka.x1).Value(ka.y0).Value(ka.y1).EndArray();
                        j.Key("head").BeginArray(true).Value(ka.hx0).Value(ka.hx1).Value(ka.hy0).Value(ka.hy1).Value(ka.headMargin).EndArray();
                        j.EndObject();
                    }
                j.EndArray();
                j.EndObject();
            }
            j.EndObject();
        }

        public string Markdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Visibility and reachability audit (MASTER_PROMPT 11.13)");
            sb.AppendLine();
            sb.AppendLine(FormattableString.Invariant($"Generated {DateTime.Now:yyyy-MM-dd HH:mm} by VisibilityAuditTests (V) in {Seconds:0.0} s. Built world: {BuiltInfo}."));
            sb.AppendLine("Subjects are spec-derived sample points (VisibilitySubjects); occlusion columns: **spec** = the analytic plan of 14.4/14.5 (VisibilitySpecWorld), **built** = the opaque meshes the builders produced (instanced signs, streaks, traffic, glass, koi and the sky dome are not occluders). Margin: |NDC| <= 0.94 (3 %) at 16:9, 16:10 and 21:9, at the table pose, the follower pose after a 40 m run (f40) and on the whole cinematic walk (fW).");
            sb.AppendLine();
            sb.AppendLine("| Shot | s | lag f40 / fW (deg) | readable s (spec) | reach | subjects ok | worst spec dev | composition (spec / built) | T-DROP |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
            foreach (var r in Results)
            {
                int ok = 0, n = 0;
                double worst = double.NaN;
                foreach (var s in r.subjects) { if (s.subject.rule == VisRule.Info) continue; n++; if (s.pass) ok++; if (!double.IsNaN(s.specErr)) worst = double.IsNaN(worst) ? s.specErr : Math.Max(worst, s.specErr); }
                string comp = "";
                if (r.sharesSpec != null) comp = Shares(r.sharesSpec) + (r.sharesBuilt != null ? " / " + Shares(r.sharesBuilt) : "");
                if (r.compBuiltBy.Length > 0) comp += " (built near: " + r.compBuiltBy + ")";
                if (r.koi != null) comp = FormattableString.Invariant($"koi min margin {F(MinKoi(r.koi, r.shot.koiWhole), "0.000")} visible / core {F(MinCore(r.koi), "0.000")} (spec {F(r.shot.koiSpecMargin, "0.000")}){(r.koi.headOccluded > 0 ? "; head hidden " + r.koi.headOccluded + "/160" : "")}");
                string reach = r.reachOnPath ? "path" : (r.reachInLane ? "lane" : "**OUT**");
                string lag = FormattableString.Invariant($"{r.lag40:0.00} / {r.lagWalk:0.00}") + (r.lag40 > LagFlag || r.lagWalk > LagFlag ? " **>2**" : "");
                sb.AppendLine(FormattableString.Invariant($"| {r.shot.id} | {r.shot.ks.s} | {lag} | {F(r.readA, "0.0")}-{F(r.readB, "0.0")} ({r.shot.readA}-{r.shot.readB}) | {reach} | {ok}/{n} | {F(worst, "0.00")} | {comp} | {(r.shot.TDrop ? (r.tdropPass ? "pass" : "**FAIL**") : "-")} |"));
            }
            sb.AppendLine();
            sb.AppendLine("## Subjects (16:9, target pose)");
            sb.AppendLine();
            sb.AppendLine("| Shot | subject | rule | NDC x | NDC y | spec | px 1080p (w x h) | px 4K | unoccluded spec / built | occluded by | ok |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|");
            foreach (var r in Results)
                foreach (var s in r.subjects)
                {
                    var q = s.proj[0, 0];
                    bool part = s.subject.rule == VisRule.Part;
                    string nx = q.HasFront ? (part && q.HasFrame ? $"{F(q.fx0, "0.00")}..{F(q.fx1, "0.00")}" : $"{F(q.x0, "0.00")}..{F(q.x1, "0.00")}") : "behind";
                    string ny = q.HasFront ? (part && q.HasFrame ? $"{F(q.fy0, "0.00")}..{F(q.fy1, "0.00")}" : $"{F(q.y0, "0.00")}..{F(q.y1, "0.00")}") : "";
                    string by = (s.occSpecBy + (s.occBuiltBy.Length > 0 ? " / " + s.occBuiltBy : "")).Trim();
                    sb.AppendLine(FormattableString.Invariant($"| {r.shot.id} | {s.subject.name} | {s.subject.rule} | {nx} | {ny} | {s.subject.spec.Replace("|", "/")} | {s.wPx1080:0} x {s.hPx1080:0} | {s.wPx4k:0} x {s.hPx4k:0} | {F(s.visSpec, "0.00")} / {F(s.visBuilt, "0.00")} | {by} | {(s.pass ? "ok" : "**no**")} |"));
                }
            sb.AppendLine();
            sb.AppendLine("## Koi lint (KoiMath sample set, 20 wave phases)");
            sb.AppendLine();
            sb.AppendLine("| Shot | pose | aspect | visible NDC x | visible NDC y | min margin (visible) | all samples | core 164 | head x / y | outside |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
            foreach (var r in Results)
            {
                if (r.koi == null) continue;
                for (int p = 0; p < PoseNames.Length; p++)
                    for (int a = 0; a < AspectNames.Length; a++)
                    {
                        var ka = r.koi.aspects[p, a];
                        sb.AppendLine(FormattableString.Invariant($"| {r.shot.id} | {PoseNames[p]} | {ka.aspect} | {F(ka.x0, "0.00")}..{F(ka.x1, "0.00")} | {F(ka.y0, "0.00")}..{F(ka.y1, "0.00")} | {F(ka.minMargin == double.MaxValue ? double.NaN : ka.minMargin)} | {F(ka.minMarginAll)} | {F(ka.minMarginCore == double.MaxValue ? double.NaN : ka.minMarginCore)} | {F(ka.hx0, "0.00")}..{F(ka.hx1, "0.00")} / {F(ka.hy0, "0.00")}..{F(ka.hy1, "0.00")} | {ka.outside} |"));
                    }
                sb.AppendLine(FormattableString.Invariant($"| {r.shot.id} | core samples hidden per phase: spire max {r.koi.maxSpire} (mean {r.koi.sumSpire / 20.0:0.0}), arcade max {r.koi.maxArcade} (mean {r.koi.sumArcade / 20.0:0.0}), other {r.koi.maxOther}, built {r.koi.maxBuilt}; head occluded {r.koi.headOccluded}; body {r.koi.minThick4k:0}-{r.koi.maxThick4k:0} px thick at 4K | | | | | | | | |"));
            }
            sb.AppendLine();
            sb.AppendLine("## Pre-reveal koi (D-029 GB-03: KS04-KS16 and the beat 2-6 stills, 20 wave phases, built meshes)");
            sb.AppendLine();
            sb.AppendLine("| Pose | s | yaw / pitch | koi in frame | koi visible | via notch / promenade / arcade / other | spire in frame / visible / via notch |");
            sb.AppendLine("|---|---|---|---|---|---|---|");
            foreach (var p in PreReveal)
                sb.AppendLine(FormattableString.Invariant($"| {p.tag} | {p.s:0.0} | {NocMath.WrapDeg(p.yaw):0} / {p.pitch:0} | {p.inFrame} | {(p.visible > 0 ? "**" + p.visible + "**" : "0")} | {p.viaNotch} / {p.viaPromenade} / {p.viaArcade} / {p.viaOther} | {p.spireInFrame} / {p.spireVisible} / {p.spireViaNotch} |"));
            sb.AppendLine();
            sb.AppendLine("## Failures (fix and owner)");
            sb.AppendLine();
            if (Failures.Count == 0) sb.AppendLine("None.");
            foreach (var f in Failures)
                sb.AppendLine($"- **{f.shot} {f.subject}** ({f.kind}, owner {f.owner}): {f.detail}. Fix: {f.fix}.");
            if (Notes.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("## Notes");
                sb.AppendLine();
                foreach (var n in Notes) sb.AppendLine("- " + n);
            }
            return sb.ToString();
        }

        static double MinCore(VisKoiResult k)
        {
            double m = double.MaxValue;
            foreach (var a in k.aspects) m = Math.Min(m, a.minMarginCore);
            return m == double.MaxValue ? double.NaN : m;
        }

        static string Shares(double[] s)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++) sb.Append(i > 0 ? "/" : "").Append((100.0 * s[i]).ToString("0", CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        public static double MinKoi(VisKoiResult k, bool whole = true)
        {
            double m = double.MaxValue;
            foreach (var a in k.aspects) m = Math.Min(m, whole ? a.minMargin : a.headMargin);
            return m == double.MaxValue ? double.NaN : m;
        }
    }
}
