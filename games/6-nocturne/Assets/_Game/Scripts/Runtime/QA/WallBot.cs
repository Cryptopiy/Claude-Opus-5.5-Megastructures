// WallBot: the wall-hugging bot (MASTER_PROMPT 11.8, 14.10 Autoplay; Part 10 item 9), imported from Thread.
// Active with -wallbot. Walks the whole path twice in virtual time (1/60 s frames, 60 fps cap) with the same AutoWalker
// (IPlayerState.SetDrive edge mode): first 0.40 m inside the left lane edge, then 0.40 m inside the right one (14.10;
// IMPORT_PLAN 4.6: Thread pushed 0.5 m beyond the edge), including every transition window of 14.6 and the funnels.
// Every frame (after settling) it asserts:
//   |Lateral| <= LaneHalfWidth(S) + 1 mm (never leaves the lane), progress never drops more than 0.3 m,
//   the bot really follows the edge (|Lateral| within 10 cm of the inset edge on at least 90 percent of frames),
//   the eye is within 2 cm (+ head bob) of PathY(S) + 1.7 where the floor grade has been constant for
//   3 m, either directly or after the steady camera-follow lag grade * speed * 0.12 s (14.12 smoothing),
//   and the player never falls (eye more than 0.5 m below the floor).
// It also reports the lateral offset measured independently from the eye position and the lowest feet height (14.10: never
// y < -0.5). After both passes a lane-reach probe (audit S3 item 15) pushes the CharacterController sideways into the
// invisible wall at 14 straight, constant-width spots on both sides and asserts that the reachable lateral equals the lane
// half-width of 14.4 within 1 cm (the lane is "collider gap minus 0.70 m": PathGuide places the wall face at
// half-width + radius + the controller's skin width, PlayerTuning.WallGap). Run at 1.5 m/s (default) and at 2.5 m/s (-brisk).
// Ends with PathGuide KillCount 0, CoverageCheck misses 0, zero error logs. Report: <report dir>/wallbot_<mode>.json.
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

namespace Megastructure
{
    [DefaultExecutionOrder(-50)]
    public sealed class WallBot : QaRunnerBase
    {
        public const double LateralTolerance = 0.001;   // |lateral| <= HalfWidth + 1 mm
        public const double EyeTolerance = 0.02;        // eye within 2 cm of PathY + 1.7
        public const double EdgeInset = 0.40;           // 14.10: 0.40 m inside both lane edges
        public const double MaxProgressDrop = 0.3;      // 14.10: progress never drops more than 0.3 m
        const double SettleSeconds = 1.0;
        const double HugSlack = 0.10;
        const double MinHugFraction = 0.9;
        const double IndependentTolerance = 0.02;
        const double FallDrop = 0.5;
        const double GradeLookBack = 3.0;
        const double PassEndS = NocturneConstants.GoalTriggerS - 0.5;   // pass 1 ends before the goal trigger (1497)
        const double RestSpeed = 0.01, RestHoldSeconds = 0.5;
        const double StallSeconds = 15.0, StallDistance = 0.1, TimeoutMarginSeconds = 60.0;

        // lane-reach probe: straight, constant-width spots (alley, crossing, median, tongue tip, bridge, garden, promenade, plaza)
        public static readonly double[] ProbeS = { 10, 100, 150, 300, 540, 600, 720, 840, 950, 1000, 1150, 1200, 1300, 1400 };
        public const int ProbeFrames = 30;              // 30 frames of a 0.05 m/frame sideways push (3 m/s) into the wall
        public const double ProbePush = 0.05, ProbeStartInset = 0.30;
        public const double ReachTolerance = 0.01;      // reachable lateral = lane half-width +- 1 cm

        protected override QaRunMode Mode => QaRunMode.WallBot;
        protected override string ReportName => "wallbot_" + m_mode;
        protected override string ReportTag => "WALLBOT";

        sealed class PassStats
        {
            public string side;
            public bool completed;
            public double sStart = double.NaN, sEnd = double.NaN, seconds;
            public long frames, checkedFrames, hugFrames;
            public int lateralViolations, eyeViolations, falls, independentViolations, progressDrops;
            public double maxExcess = double.NegativeInfinity, maxExcessS = double.NaN;
            public double maxIndependentExcess = double.NegativeInfinity, maxIndependentS = double.NaN;
            public double maxEyeErr, maxEyeErrS = double.NaN, maxEyeDevRaw;
            public double minFeetY = double.PositiveInfinity, minFeetYS = double.NaN;
            public double HugFraction => checkedFrames > 0 ? (double)hugFrames / checkedFrames : 0;
        }

        // per transition window (14.6) plus the funnel s 350-400 (14.4.3)
        struct WindowStats { public double s0, s1, maxExcess, minGap, maxEyeErr; public long frames; }

        float m_speed;
        string m_mode = "walk";
        readonly PassStats[] m_passes = { new PassStats { side = "left" }, new PassStats { side = "right" } };
        WindowStats[] m_windows;
        int m_pass = -1;
        int m_frameDrive;
        double m_maxS;
        bool m_walking;
        double m_passT0, m_settleUntil, m_lastS, m_lastMoveT, m_restSince = -1, m_t;
        // lane-reach probe state
        int m_probe = -1, m_probeFrame, m_probeFails;
        double m_probeMaxErr, m_probeMaxErrS = double.NaN;
        readonly double[] m_reach = new double[2 * ProbeS.Length];

        protected override void Awake()
        {
            base.Awake();
            if (!enabled) return;
            m_speed = QaMode.DriveSpeed;
            m_mode = QaMode.SpeedName(m_speed);
            int n = ShotPlan.TransitionWindows.GetLength(0);
            m_windows = new WindowStats[n + 1];
            for (int i = 0; i < n; i++) m_windows[i] = NewWindow(ShotPlan.TransitionWindows[i, 0], ShotPlan.TransitionWindows[i, 1]);
            m_windows[n] = NewWindow(FunnelS0, FunnelS1);
        }

        // the tongue funnel of 14.4 (w(x) 75 -> 3.2 over x 225.3-305.3: s 410-490)
        const double FunnelS0 = 410.0, FunnelS1 = 490.0;

        static WindowStats NewWindow(double a, double b) => new WindowStats { s0 = a, s1 = b, maxExcess = double.NegativeInfinity, minGap = double.PositiveInfinity };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsurePresent() { EnsureRunner<WallBot>(QaRunMode.WallBot); }

        IEnumerator Start()
        {
            yield return WaitForWorld();
            if (!WorldOk) { Checks.Add("worldReady", false, "GameBootstrap.Ready never became true"); Done("world not ready"); yield break; }
            QaDrive.Prepare(true, false);
            yield return null;
            yield return null;
            RunStaticChecks();
            if (NocServices.Player == null) { Checks.Add("playerService", false, "NocServices.Player is null"); Done("no player"); yield break; }
            BeginPass(0);
            m_walking = true;
            Debug.Log("[NOC] WALLBOT start speed=" + m_speed.ToString("0.00", CultureInfo.InvariantCulture));
        }

        void BeginPass(int pass)
        {
            m_pass = pass;
            var p = NocServices.Player;
            double side = pass == 0 ? -1.0 : 1.0;
            p.Frozen = false;
            p.Teleport(0, side * Math.Max(0.0, NocMath.HalfWidth(0) - EdgeInset));
            m_passT0 = Time.timeAsDouble;
            m_settleUntil = SettleSeconds;
            m_lastS = 0;
            m_lastMoveT = 0;
            m_restSince = -1;
            m_passes[pass].sStart = 0;
            m_frameDrive = 0;
            m_maxS = 0;
            Debug.Log("[NOC] WALLBOT pass " + (pass + 1) + " (" + m_passes[pass].side + " edge)");
        }

        void Update()
        {
            if (!m_walking || Finished || m_pass < 0) return;
            var p = NocServices.Player;
            if (p == null) { Done("player service lost"); return; }
            if (m_probe >= 0) { ProbeStep(p); return; }
            PassStats ps = m_passes[m_pass];
            m_t = Time.timeAsDouble - m_passT0;
            double s = p.S;
            ps.frames++;

            if (m_t >= m_settleUntil) Check(p, ps, s);

            // drive: the AutoWalker follows this pass's lane edge 0.40 m inside (edge mode of IPlayerState.SetDrive)
            double side = m_pass == 0 ? -1.0 : 1.0;
            if (!p.Finished && (m_frameDrive++ == 0)) p.SetDrive(true, m_speed, (float)(side * 1000.0));
            if (m_t >= m_settleUntil && s < m_maxS - MaxProgressDrop) ps.progressDrops++;
            if (s > m_maxS) m_maxS = s;
            QaDrive.LookForward(s, (float)NocturneConstants.GazeDefaultPitch);

            if (s > m_lastS + StallDistance) { m_lastS = s; m_lastMoveT = m_t; }
            if (!p.Finished && m_t - m_lastMoveT > StallSeconds) { Done("stalled at s=" + s.ToString("0.00", CultureInfo.InvariantCulture) + " (pass " + (m_pass + 1) + ")"); return; }
            if (m_t > NocturneConstants.PathLength / m_speed + TimeoutMarginSeconds) { Done("timeout in pass " + (m_pass + 1)); return; }

            if (m_pass == 0 && s >= PassEndS)
            {
                ps.completed = true; ps.sEnd = s; ps.seconds = m_t;
                BeginPass(1);
                return;
            }
            if (m_pass == 1 && p.Finished)
            {
                if (Math.Abs(p.Speed) < RestSpeed)
                {
                    if (m_restSince < 0) m_restSince = m_t;
                    if (m_t - m_restSince >= RestHoldSeconds) { ps.completed = true; ps.sEnd = s; ps.seconds = m_t; m_probe = 0; m_probeFrame = 0; }
                }
                else m_restSince = -1;
            }
        }

        /// <summary>One frame of the lane-reach probe: teleport 0.30 m inside the edge, push sideways for 30 frames, read the lateral.</summary>
        void ProbeStep(IPlayerState p)
        {
            int n = 2 * ProbeS.Length;
            if (m_probe >= n) { Done(null); return; }
            double s = ProbeS[m_probe / 2], side = (m_probe & 1) == 0 ? -1.0 : 1.0;
            double h = NocMath.HalfWidth(s);
            if (m_probeFrame == 0)
            {
                p.SetDrive(false, 0f, 0f);
                p.Teleport(s, side * (h - ProbeStartInset));
            }
            else if (m_probeFrame <= ProbeFrames)
            {
                NocMath.Right(s, out double rx, out double rz);
                var pc = PlayerController.Instance;
                if (pc != null) pc.DriveExternal(new Vector3((float)(rx * side * ProbePush), 0f, (float)(rz * side * ProbePush)));
            }
            else if (m_probeFrame >= ProbeFrames + 2)
            {
                double reach = Math.Abs(p.Lateral);
                m_reach[m_probe] = reach;
                double err = reach - h;
                if (Math.Abs(err) > Math.Abs(m_probeMaxErr)) { m_probeMaxErr = err; m_probeMaxErrS = s * side; }
                if (Math.Abs(err) > ReachTolerance) m_probeFails++;
                m_probe++;
                m_probeFrame = 0;
                return;
            }
            m_probeFrame++;
        }

        void Check(IPlayerState p, PassStats ps, double s)
        {
            ps.checkedFrames++;
            double hw = NocMath.HalfWidth(s);
            double lat = p.Lateral;
            double excess = Math.Abs(lat) - hw;
            if (excess > ps.maxExcess) { ps.maxExcess = excess; ps.maxExcessS = s; }
            if (excess > LateralTolerance) ps.lateralViolations++;
            if (Math.Abs(excess + EdgeInset) <= HugSlack) ps.hugFrames++;

            // lateral measured independently from the eye position (informational)
            Vector3 eye = p.EyePosition;
            var e = new D3(eye.x, eye.y - NocturneConstants.EyeHeight, eye.z);
            double sp = NocMath.ProjectToPath(e, s);
            D3 pp = NocMath.PathPoint(sp);
            NocMath.Right(sp, out double rx, out double rz);
            double latI = (e.x - pp.x) * rx + (e.z - pp.z) * rz;
            double exI = Math.Abs(latI) - NocMath.HalfWidth(sp);
            if (exI > ps.maxIndependentExcess) { ps.maxIndependentExcess = exI; ps.maxIndependentS = s; }
            if (exI > IndependentTolerance) ps.independentViolations++;

            // eye height
            double dev = eye.y - (NocMath.PathY(s) + NocturneConstants.EyeHeight);
            if (Math.Abs(dev) > Math.Abs(ps.maxEyeDevRaw)) ps.maxEyeDevRaw = dev;
            QaDrive.GradeRangeBehind(s, GradeLookBack, out double gMin, out double gMax);
            double v = Math.Abs(p.Speed), tau = 0.12;
            double err, allowed;
            if (gMax - gMin < 1e-6)
            {
                double lag = NocMath.PathGrade(s) * v * tau;   // steady lag of a critically damped follow
                err = Math.Min(Math.Abs(dev), Math.Abs(dev + lag));
                allowed = EyeTolerance + NocturneConstants.HeadBobMax;
            }
            else
            {
                err = Math.Abs(dev);
                allowed = EyeTolerance + NocturneConstants.HeadBobMax + 2.0 * gMax * v * tau;
            }
            if (err > ps.maxEyeErr) { ps.maxEyeErr = err; ps.maxEyeErrS = s; }
            if (err > allowed) ps.eyeViolations++;
            if (dev < -FallDrop) ps.falls++;
            double feetY = eye.y - NocturneConstants.EyeHeight;
            if (feetY < ps.minFeetY) { ps.minFeetY = feetY; ps.minFeetYS = s; }

            for (int i = 0; i < m_windows.Length; i++)
            {
                if (s < m_windows[i].s0 || s > m_windows[i].s1) continue;
                m_windows[i].frames++;
                if (excess > m_windows[i].maxExcess) m_windows[i].maxExcess = excess;
                if (-excess < m_windows[i].minGap) m_windows[i].minGap = -excess;
                if (err > m_windows[i].maxEyeErr) m_windows[i].maxEyeErr = err;
            }
        }

        void Done(string abort)
        {
            m_walking = false;
            var inv = CultureInfo.InvariantCulture;
            Checks.Add("completedBothPasses", abort == null && m_passes[0].completed && m_passes[1].completed, abort ?? "both passes completed");
            for (int i = 0; i < m_passes.Length; i++)
            {
                PassStats ps = m_passes[i];
                string n = ps.side;
                Checks.Add(n + "ClampHolds", ps.lateralViolations == 0 && ps.checkedFrames > 0,
                           ps.lateralViolations + " frames beyond HalfWidth + 1 mm, max excess " + Mm(ps.maxExcess) + " mm at s=" + ps.maxExcessS.ToString("0.0", inv));
                Checks.Add(n + "FollowsEdge", ps.HugFraction >= MinHugFraction, "0.40 m inside the edge (+-10 cm) on " + (100.0 * ps.HugFraction).ToString("0.0", inv) + " % of checked frames");
                Checks.Add(n + "ProgressMonotone", ps.progressDrops == 0, ps.progressDrops + " frames with progress more than 0.3 m below its maximum");
                Checks.Add(n + "EyeHeight", ps.eyeViolations == 0, ps.eyeViolations + " frames off, max error " + Mm(ps.maxEyeErr) + " mm at s=" + ps.maxEyeErrS.ToString("0.0", inv));
                Checks.Add(n + "NeverFalls", ps.falls == 0 && !(ps.minFeetY < -FallDrop), ps.falls + " frames with the eye more than 0.5 m below the floor, lowest feet y " + ps.minFeetY.ToString("0.000", inv) + " m at s=" + ps.minFeetYS.ToString("0.0", inv) + " (>= -0.5)");
                Checks.Add(n + "IndependentLateral", ps.independentViolations == 0, ps.independentViolations + " frames, max excess " + Mm(ps.maxIndependentExcess) + " mm (eye-position measurement)", false);
            }
            int probed = Math.Min(m_probe < 0 ? 0 : m_probe, 2 * ProbeS.Length);
            Checks.Add("laneReach", probed == 2 * ProbeS.Length && m_probeFails == 0,
                       probed + " probes, " + m_probeFails + " beyond +-1 cm, max (reach - half-width) " + Mm(m_probeMaxErr) + " mm at signed s " + m_probeMaxErrS.ToString("0.0", inv));
            var bd = BeatDirector.Instance;
            if (bd != null) Checks.Add("goalOnce", bd.GoalCount == 1, "goal count " + bd.GoalCount, false);
            AddCommonChecks();
            string summary = "mode=" + m_mode + " left max excess " + Mm(m_passes[0].maxExcess) + " mm, right max excess " + Mm(m_passes[1].maxExcess) + " mm, errors " + QaLog.ErrorCount;
            Complete(summary, WriteBody);
        }

        static string Mm(double m) => double.IsInfinity(m) || double.IsNaN(m) ? "n/a" : (m * 1000.0).ToString("0.0", CultureInfo.InvariantCulture);

        void WriteBody(QaJson j)
        {
            j.Prop("mode", m_mode).Prop("speed", m_speed).Prop("edgeInset", EdgeInset).Prop("lateralTolerance", LateralTolerance).Prop("eyeTolerance", EyeTolerance);
            j.Key("passes").BeginArray();
            for (int i = 0; i < m_passes.Length; i++)
            {
                PassStats ps = m_passes[i];
                j.BeginObject();
                j.Prop("side", ps.side).Prop("completed", ps.completed).Prop("sStart", ps.sStart).Prop("sEnd", ps.sEnd).Prop("seconds", ps.seconds);
                j.Prop("frames", ps.frames).Prop("checkedFrames", ps.checkedFrames).Prop("hugFraction", ps.HugFraction);
                j.Prop("lateralViolations", ps.lateralViolations).Prop("maxExcessM", ps.maxExcess).Prop("maxExcessS", ps.maxExcessS);
                j.Prop("independentViolations", ps.independentViolations).Prop("maxIndependentExcessM", ps.maxIndependentExcess).Prop("maxIndependentS", ps.maxIndependentS);
                j.Prop("eyeViolations", ps.eyeViolations).Prop("maxEyeErrM", ps.maxEyeErr).Prop("maxEyeErrS", ps.maxEyeErrS).Prop("maxEyeDevRawM", ps.maxEyeDevRaw);
                j.Prop("falls", ps.falls).Prop("minFeetY", ps.minFeetY).Prop("minFeetYS", ps.minFeetYS);
                j.EndObject();
            }
            j.EndArray();
            j.Key("windows").BeginArray();
            for (int i = 0; i < m_windows.Length; i++)
            {
                WindowStats w = m_windows[i];
                j.BeginObject(true).Prop("s0", w.s0).Prop("s1", w.s1).Prop("frames", w.frames).Prop("maxExcessM", w.maxExcess).Prop("minGapM", w.minGap).Prop("maxEyeErrM", w.maxEyeErr).EndObject();
            }
            j.EndArray();
            j.Prop("coverage", CoverageNote).Prop("coverageMisses", CoverageMisses);
            j.Prop("controllerSkin", PlayerTuning.ControllerSkin).Prop("wallGap", PlayerTuning.WallGap);
            j.Key("laneReach").BeginArray();
            for (int i = 0; i < m_reach.Length; i++)
            {
                double s = ProbeS[i / 2];
                j.BeginObject(true).Prop("s", s).Prop("side", (i & 1) == 0 ? "left" : "right").Prop("halfWidth", NocMath.HalfWidth(s))
                 .Prop("reach", m_reach[i]).Prop("errorM", m_reach[i] - NocMath.HalfWidth(s)).EndObject();
            }
            j.EndArray();
        }
    }
}
