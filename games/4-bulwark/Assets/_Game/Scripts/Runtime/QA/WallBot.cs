// WallBot: the wall-hugging bots (MASTER_PROMPT 11.8, 14.4, 14.10.4; Part 10 item 9). Imported from Thread's WallBot,
// rewritten in lane coordinates (BwPath.Lane), with Abyss's per-point failure reporting (QaBots) merged in.
// Active with -wallbot. Virtual time (1/60 s frames), capped at 60 fps. Passes, each driven by PlayerController.SetDrive with
// the exact progress speed and an exact lateral target:
//   1. T1 left / 2. T1 right: exactly on the lane edges (gallery x -3.15 and 0.15) over s 0-120 and through the door
//   3. left  : 0.40 m inside the left lane edge, s 0 -> just before the goal trigger
//   4. platformLeft / 5. platformRight: 0.40 m inside the lantern-platform lane edges (EndingPath.Lane; A1 4.10), from just past
//      EndingPath.SFoot0 (the cabin's north door) to 0.5 m before EndingPath.SGoal; feet vs EndingPath.FloorFootY, eye vs
//      EndingPath.EyeY (+-0.3 m)
//   6. right : 0.40 m inside the right lane edge, s 0 -> the goal (the stop ramp runs; rest at s 1500); last: it boards the cabin
// Every frame (after settling) it asserts (14.10.4): the bot never leaves the lane (capsule-centre lateral within the lane
// + 1 mm), its feet never differ from the floor profile BwPath.FloorY by more than 0.3 m, the eye never differs from
// BwPath.EyeY by more than 0.3 m, it never loses more than 0.3 m of progress against v t, no CharacterController rejection,
// no safety snap. The first failures are listed with world position, s, lateral, expected floor and detail (Abyss QaBots).
// Ends with PathGuide KillCount 0, CoverageCheck misses 0, the 0.2 m camera probe never inside a collider (CameraRig.Violations,
// Part 10 #8), per-pass CharacterController rejections and safety snaps 0, zero error logs. Report: <report dir>/wallbot_<mode>.json.
// PathGuide obstacles (props the spec puts inside the lane, e.g. the cart): when the edge target would bring the capsule
// within 5 cm of an obstacle box over the next 2 m (or the last 0.6 m), the bot steers to the nearest clear lateral inside the
// lane and back afterwards; the event and any CharacterController contact near an obstacle are reported as kind "obstacle"
// (not a lane leak, not a rejection or progress failure; obstacles fail only on the cinematic path, AutoTestRunner).
// Not covered yet (needs ceiling colliders, which the corridor does not build): Abyss's upward soffit rays (0.62 m, 14.4).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Megastructure.Player;

namespace Megastructure.QA
{
    [DefaultExecutionOrder(-50)]
    public sealed class WallBot : QaRunnerBase
    {
        public const double LateralTolerance = 0.001;   // stays in the lane (+ 1 mm)
        public const double FloorTolerance = 0.3;       // 14.10.4: never differs from the floor profile by over 0.3 m
        public const double ProgressTolerance = 0.3;    // 14.10.4: never loses over 0.3 m of progress
        const double SettleSeconds = 0.5;
        const double T1EndS = 120.0;
        const double StallSeconds = 15.0, StallDistance = 0.1, TimeoutMarginSeconds = 60.0;
        const int MaxFailuresListed = 60;

        protected override QaRunMode Mode => QaRunMode.WallBot;
        protected override string ReportName => "wallbot_" + m_mode;
        protected override string ReportTag => "WALLBOT";

        sealed class PassStats
        {
            public string name;
            public int side;            // -1 left, +1 right
            public double inset;        // metres inside the lane edge
            public double sStart;       // pass starts at this s (0, or EndingPath.SFoot0 for the platform lanes)
            public double sEnd;         // pass ends at this s (or the goal rest)
            public bool completed;
            public double sReached = double.NaN, seconds;
            public long frames, checkedFrames;
            public int laneViolations, floorViolations, eyeViolations, progressViolations;
            public double maxExcess = double.NegativeInfinity, maxExcessS = double.NaN;
            public double maxFloorDev, maxFloorDevS = double.NaN, maxEyeDev, maxProgressLoss;
            public int ccRejections, safetySnaps, probeViolations;
            public int obstacleDetourFrames, obstacleContacts;
        }

        struct Failure { public string pass, kind, detail; public Vector3 pos; public double s, lateral, floorY; }

        float m_speed;
        string m_mode = "walk";
        PassStats[] m_passes;
        readonly List<Failure> m_failures = new List<Failure>(MaxFailuresListed);
        int m_failureCount;
        int m_pass = -1;
        bool m_walking;
        double m_passT0, m_s0, m_lastS, m_lastMoveT, m_t;
        int m_rej0, m_snap0, m_probe0, m_rejSeen, m_rejObstacle;
        int m_obstacleEvents, m_obstacleNotes;
        string m_lastObstacle;
        const double DetourAhead = 2.0, DetourBehind = 0.6, DetourStep = 0.05, ObstacleMargin = 0.05, ObstacleNearMargin = 0.3;

        protected override void Awake()
        {
            base.Awake();
            if (!enabled) return;
            m_speed = QaMode.DriveSpeed;
            m_mode = QaMode.SpeedName(m_speed);
            double inset = FrameworkConst.WallBotInset;
            m_passes = new[]
            {
                new PassStats { name = "t1Left", side = -1, inset = 0.0, sEnd = T1EndS },
                new PassStats { name = "t1Right", side = +1, inset = 0.0, sEnd = T1EndS },
                new PassStats { name = "left", side = -1, inset = inset, sEnd = FrameworkConst.GoalTriggerS - 1.0 },
                // the lantern platform (A1 4.10: SFoot0 .. SGoal), before "right", which must stay last because it boards the cabin
                new PassStats { name = "platformLeft", side = -1, inset = inset, sStart = EndingPath.SFoot0, sEnd = EndingPath.SGoal - 0.5 },
                new PassStats { name = "platformRight", side = +1, inset = inset, sStart = EndingPath.SFoot0, sEnd = EndingPath.SGoal - 0.5 },
                new PassStats { name = "right", side = +1, inset = inset, sEnd = BwPath.Length },   // last: runs the board trigger and the ramp into the cabin
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsurePresent() { EnsureRunner<WallBot>(QaRunMode.WallBot); }

        IEnumerator Start()
        {
            yield return WaitForWorld();
            if (!WorldOk) { Checks.Add("worldReady", false, "GameBootstrap.Ready never became true"); Done("world not ready"); yield break; }
            QaDrive.Prepare(true, false);
            yield return null;
            yield return null;
            yield return null;
            RunStaticChecks();
            if (BwServices.Player == null) { Checks.Add("playerService", false, "BwServices.Player is null"); Done("no player"); yield break; }
            BeginPass(0);
            m_walking = true;
            Debug.Log("[BW] WALLBOT start speed=" + m_speed.ToString("0.00", CultureInfo.InvariantCulture));
        }

        static double TargetLateral(double s, int side, double inset)
        {
            if (s > BwPath.Length) return PlatformLateral(s, side, inset);
            BwPath.Lane(s, out double lo, out double hi);
            double half = 0.5 * (hi - lo);
            double ins = Math.Min(inset, half);   // a lane narrower than 2 x inset is driven on its centre
            return side < 0 ? lo + ins : hi - ins;
        }

        /// <summary>The lantern-platform lane target (EndingPath.Lane): inset inside the edge, the centre where the lane is narrower.</summary>
        static double PlatformLateral(double s, int side, double inset)
        {
            EndingPath.Lane(s, out double lo, out double hi);
            double ins = Math.Min(inset, 0.5 * (hi - lo));
            return side < 0 ? lo + ins : hi - ins;
        }

        static bool OnPlatform(PassStats ps) => ps.sStart >= EndingPath.SFoot0;
        static double FloorAt(double s) => s > BwPath.Length ? EndingPath.FloorFootY : BwPath.FloorY(s);
        static double EyeAt(double s) => s > BwPath.Length ? EndingPath.EyeY(s) : BwPath.EyeY(s);

        void BeginPass(int pass)
        {
            m_pass = pass;
            var p = BwServices.Player;
            var ps = m_passes[pass];
            p.Frozen = false;
            double s0 = OnPlatform(ps) ? EndingPath.SFoot0 + 0.01 : 0.0;
            p.Teleport(s0, s0 > BwPath.Length ? PlatformLateral(s0, ps.side, ps.inset) : TargetLateral(0, ps.side, ps.inset));
            var pc = PlayerController.Instance;
            if (pc != null) { pc.DriveNoSmoothing = true; pc.DriveBrisk = m_speed > FrameworkConst.WalkSpeed + 1e-6; m_rej0 = pc.Rejections; m_snap0 = pc.SafetySnaps; }
            m_rejSeen = pc != null ? pc.Rejections : 0;
            m_rejObstacle = 0;
            m_lastObstacle = null;
            m_probe0 = CameraRig.Violations;
            m_passT0 = Time.timeAsDouble;
            m_s0 = s0;
            m_lastS = s0;
            m_lastMoveT = 0;
            Debug.Log("[BW] WALLBOT pass " + (pass + 1) + " (" + ps.name + ", inset " + ps.inset.ToString("0.00", CultureInfo.InvariantCulture) + " m)");
        }

        void Update()
        {
            if (!m_walking || Finished || m_pass < 0) return;
            var p = BwServices.Player;
            if (p == null) { Done("player service lost"); return; }
            PassStats ps = m_passes[m_pass];
            m_t = Time.timeAsDouble - m_passT0;
            double s = p.S;
            ps.frames++;

            // CharacterController contacts since the last frame next to an obstacle are obstacle contacts, not leaks
            var pcNow = PlayerController.Instance;
            if (pcNow != null && pcNow.Rejections != m_rejSeen)
            {
                int d = pcNow.Rejections - m_rejSeen;
                m_rejSeen = pcNow.Rejections;
                if (NearObstacle(s, p.Lateral, ObstacleNearMargin, out string oname)) { m_rejObstacle += d; ps.obstacleContacts += d; Fail(ps, p, s, "obstacle", d + " CharacterController contact(s) with obstacle " + oname); }
            }

            if (m_t >= SettleSeconds) Check(p, ps, s);

            double target = TargetLateral(s, ps.side, ps.inset);
            string obstacle = null;
            double drive = s > BwPath.Length ? target : Detour(s, target, ps.side, out obstacle);   // PathGuide obstacles live on s 0-1500
            if (obstacle != null)
            {
                ps.obstacleDetourFrames++;
                if (obstacle != m_lastObstacle) { m_obstacleEvents++; Fail(ps, p, s, "obstacle", "detour round " + obstacle + ": lateral " + target.ToString("0.000", CultureInfo.InvariantCulture) + " -> " + drive.ToString("0.000", CultureInfo.InvariantCulture)); }
            }
            m_lastObstacle = obstacle;
            if (!p.Finished) p.SetDrive(true, m_speed, (float)drive);
            QaDrive.LookForward(s, 0f);

            if (s > m_lastS + StallDistance) { m_lastS = s; m_lastMoveT = m_t; }
            if (!p.Finished && m_t - m_lastMoveT > StallSeconds) { Fail(ps, p, s, "stall", "no progress for " + StallSeconds + " s"); Done("stalled at s=" + s.ToString("0.00", CultureInfo.InvariantCulture) + " (pass " + ps.name + ")"); return; }
            if (m_t > BwPath.Length / m_speed + TimeoutMarginSeconds) { Done("timeout in pass " + ps.name); return; }

            // the last pass runs the board trigger and its stop ramp: it ends at rest in the cabin when the ride starts (the ride and
            // the platform walk are covered by the autotest; ADDENDUM A1 00)
            var pcEnd = PlayerController.Instance;
            bool inCabin = pcEnd != null && ((pcEnd.Riding && !pcEnd.Boarding) || pcEnd.Finished);
            bool end = !OnPlatform(ps) && ps.sEnd >= BwPath.Length ? inCabin : s >= ps.sEnd;
            if (end)
            {
                ps.completed = true; ps.sReached = s; ps.seconds = m_t;
                var pc = PlayerController.Instance;
                ps.probeViolations = CameraRig.Violations - m_probe0;
                if (pc != null)
                {
                    ps.ccRejections = pc.Rejections - m_rej0 - m_rejObstacle;
                    ps.safetySnaps = pc.SafetySnaps - m_snap0;
                    if (ps.ccRejections != 0) Fail(ps, p, s, "ccRejection", ps.ccRejections + " CharacterController rejections in this pass (plus " + m_rejObstacle + " obstacle contacts)");
                    if (pc.SafetySnaps != m_snap0) Fail(ps, p, s, "safetySnap", (pc.SafetySnaps - m_snap0) + " safety snaps in this pass");
                }
                if (m_pass + 1 < m_passes.Length) { BeginPass(m_pass + 1); return; }
                Done(null);
            }
        }

        /// <summary>True when the capsule at (s', lateral) touches an obstacle for some s' in [s - DetourBehind, s + DetourAhead].</summary>
        static bool Blocked(double s, double lateral, double margin, out string name)
        {
            name = null;
            if (PathGuide.Obstacles.Count == 0) return false;
            for (double k = -DetourBehind; k <= DetourAhead + 1e-9; k += 0.2)
            {
                double sk = Math.Max(0.0, Math.Min(BwPath.Length, s + k));
                if (PathGuide.ObstacleAt(sk, lateral, (float)margin, out PathGuide.Obstacle o)) { name = o.Name; return true; }
            }
            return false;
        }

        /// <summary>True when the capsule at (s', lateral) lies within margin of an obstacle for some s' in [s - 1, s + 1].</summary>
        static bool NearObstacle(double s, double lateral, double margin, out string name)
        {
            name = null;
            if (PathGuide.Obstacles.Count == 0 || s > BwPath.Length + 1.0) return false;   // none on the lantern platform
            for (double k = -1.0; k <= 1.0 + 1e-9; k += 0.25)
                if (PathGuide.ObstacleAt(Math.Max(0.0, Math.Min(BwPath.Length, s + k)), lateral, (float)margin, out PathGuide.Obstacle o)) { name = o.Name; return true; }
            return false;
        }

        /// <summary>The drive lateral: the edge target, or the nearest clear lateral inside the lane while an obstacle blocks it
        /// (searched outward from the target, toward the lane centre first). obstacle = the blocking obstacle's name or null.</summary>
        static double Detour(double s, double target, int side, out string obstacle)
        {
            if (!Blocked(s, target, ObstacleMargin, out obstacle)) return target;
            BwPath.Lane(s, out double lo, out double hi);
            for (double d = DetourStep; d <= hi - lo + 1e-9; d += DetourStep)
            {
                for (int k = 0; k < 2; k++)
                {
                    double dir = k == 0 ? -side : side;
                    double c = target + dir * d;
                    if (c < lo - 1e-9 || c > hi + 1e-9) continue;
                    if (!Blocked(s, c, ObstacleMargin, out _)) return c;
                }
            }
            return target;   // no clear lateral: drive on (the contact is reported as an obstacle contact)
        }

        void Check(IPlayerState p, PassStats ps, double s)
        {
            ps.checkedFrames++;
            double lo, hi;
            if (s > BwPath.Length) EndingPath.Lane(s, out lo, out hi); else BwPath.Lane(s, out lo, out hi);
            double lat = p.Lateral;
            double excess = Math.Max(lo - lat, lat - hi);
            if (excess > ps.maxExcess) { ps.maxExcess = excess; ps.maxExcessS = s; }
            bool nearObs = NearObstacle(s, lat, ObstacleNearMargin + 0.5, out string obsName);
            if (excess > LateralTolerance && nearObs) Fail(ps, p, s, "obstacle", "lateral " + lat.ToString("0.000", CultureInfo.InvariantCulture) + " pushed by " + obsName);
            else if (excess > LateralTolerance) { ps.laneViolations++; Fail(ps, p, s, "lane", "lateral " + lat.ToString("0.000", CultureInfo.InvariantCulture) + " outside [" + lo.ToString("0.000", CultureInfo.InvariantCulture) + ", " + hi.ToString("0.000", CultureInfo.InvariantCulture) + "]"); }

            var pc = PlayerController.Instance;
            Vector3 feet = pc != null ? pc.FeetPosition : p.EyePosition - Vector3.up * (float)FrameworkConst.EyeHeight;
            double floor = FloorAt(s);
            double fdev = Math.Abs(feet.y - floor);
            if (fdev > ps.maxFloorDev) { ps.maxFloorDev = fdev; ps.maxFloorDevS = s; }
            if (fdev > FloorTolerance) { ps.floorViolations++; Fail(ps, p, s, "floor", "feet y " + feet.y.ToString("0.000", CultureInfo.InvariantCulture) + " vs floor " + floor.ToString("0.000", CultureInfo.InvariantCulture)); }
            double eyeY = EyeAt(s);
            double edev = Math.Abs(p.EyePosition.y - eyeY);
            if (edev > ps.maxEyeDev) ps.maxEyeDev = edev;
            if (edev > FloorTolerance) { ps.eyeViolations++; Fail(ps, p, s, "eye", "eye y " + p.EyePosition.y.ToString("0.000", CultureInfo.InvariantCulture) + " vs " + eyeY.ToString("0.000", CultureInfo.InvariantCulture)); }

            if (!p.Finished)
            {
                double expected = Math.Min(OnPlatform(ps) ? EndingPath.SRest : BwPath.Length, m_s0 + m_speed * (m_t - Time.deltaTime));   // this frame's step is integrated after this check
                double loss = expected - s;
                if (loss > ps.maxProgressLoss) ps.maxProgressLoss = loss;
                if (loss > ProgressTolerance && nearObs) Fail(ps, p, s, "obstacle", "progress lost " + loss.ToString("0.000", CultureInfo.InvariantCulture) + " m at " + obsName);
                else if (loss > ProgressTolerance) { ps.progressViolations++; Fail(ps, p, s, "progress", "lost " + loss.ToString("0.000", CultureInfo.InvariantCulture) + " m against v t"); }
            }
        }

        void Fail(PassStats ps, IPlayerState p, double s, string kind, string detail)
        {
            m_failureCount++;
            if (kind == "obstacle") m_obstacleNotes++;
            if (m_failures.Count >= MaxFailuresListed) return;
            m_failures.Add(new Failure { pass = ps.name, kind = kind, detail = detail, pos = p.EyePosition, s = s, lateral = p.Lateral, floorY = FloorAt(s) });
            if (m_failures.Count <= 10) Debug.LogWarning("[BW] WALLBOT " + ps.name + " " + kind + " at s=" + s.ToString("0.00", CultureInfo.InvariantCulture) + ": " + detail);
        }

        void Done(string abort)
        {
            m_walking = false;
            var pc = PlayerController.Instance;
            if (pc != null) { pc.DriveNoSmoothing = false; pc.SetDrive(false, 0f, 0f); }
            var inv = CultureInfo.InvariantCulture;
            bool all = abort == null;
            foreach (var ps in m_passes) all &= ps.completed;
            Checks.Add("completedAllPasses", all, abort ?? "all passes completed");
            foreach (var ps in m_passes)
            {
                string n = ps.name;
                Checks.Add(n + "StaysInLane", ps.laneViolations == 0 && ps.checkedFrames > 0,
                           ps.laneViolations + " frames outside the lane, max excess " + Mm(ps.maxExcess) + " mm at s=" + ps.maxExcessS.ToString("0.0", inv));
                Checks.Add(n + "FloorProfile", ps.floorViolations == 0, ps.floorViolations + " frames, max feet deviation " + Mm(ps.maxFloorDev) + " mm at s=" + ps.maxFloorDevS.ToString("0.0", inv));
                Checks.Add(n + "EyeProfile", ps.eyeViolations == 0, ps.eyeViolations + " frames, max eye deviation " + Mm(ps.maxEyeDev) + " mm");
                Checks.Add(n + "Progress", ps.progressViolations == 0, "max progress loss " + Mm(ps.maxProgressLoss) + " mm");
            }
            // Part 10 #8 / 11.8: the 0.2 m camera probe never enters a collider while the bots hug the edges
            Checks.Add("cameraProbe", CameraRig.Violations == 0, CameraRig.Violations + " frames with the 0.2 m camera probe inside a collider, max penetration " + Mm(CameraRig.MaxPenetration) + " mm");
            Checks.Add("noFailuresListed", m_failureCount - m_obstacleNotes == 0, (m_failureCount - m_obstacleNotes) + " failure points (" + m_failures.Count + " entries in the report; " + m_obstacleNotes + " obstacle notes are not failures)");
            int detours = 0, contacts = 0;
            foreach (var ps in m_passes) { detours += ps.obstacleDetourFrames; contacts += ps.obstacleContacts; }
            Checks.Add("obstacles", true, PathGuide.Obstacles.Count + " obstacles registered; " + m_obstacleEvents + " detours (" + detours + " frames), " + contacts + " contacts (kind obstacle, not a leak; obstacles fail only on the cinematic path)", false);
            AddCommonChecks();
            string summary = "mode=" + m_mode + " failures=" + m_failureCount + " errors=" + QaLog.ErrorCount;
            Complete(summary, WriteBody);
        }

        static string Mm(double m) => double.IsInfinity(m) || double.IsNaN(m) ? "n/a" : (m * 1000.0).ToString("0.0", CultureInfo.InvariantCulture);

        void WriteBody(QaJson j)
        {
            j.Prop("mode", m_mode).Prop("speed", m_speed).Prop("inset", FrameworkConst.WallBotInset).Prop("lateralTolerance", LateralTolerance)
             .Prop("floorTolerance", FloorTolerance).Prop("progressTolerance", ProgressTolerance);
            j.Key("passes").BeginArray();
            foreach (var ps in m_passes)
            {
                j.BeginObject();
                j.Prop("name", ps.name).Prop("side", ps.side).Prop("inset", ps.inset).Prop("sStart", ps.sStart).Prop("completed", ps.completed).Prop("sReached", ps.sReached).Prop("seconds", ps.seconds);
                j.Prop("frames", ps.frames).Prop("checkedFrames", ps.checkedFrames);
                j.Prop("laneViolations", ps.laneViolations).Prop("maxExcessM", ps.maxExcess).Prop("maxExcessS", ps.maxExcessS);
                j.Prop("floorViolations", ps.floorViolations).Prop("maxFloorDevM", ps.maxFloorDev).Prop("maxFloorDevS", ps.maxFloorDevS);
                j.Prop("eyeViolations", ps.eyeViolations).Prop("maxEyeDevM", ps.maxEyeDev);
                j.Prop("progressViolations", ps.progressViolations).Prop("maxProgressLossM", ps.maxProgressLoss);
                j.Prop("ccRejections", ps.ccRejections).Prop("safetySnaps", ps.safetySnaps).Prop("cameraProbeViolations", ps.probeViolations);
                j.Prop("obstacleDetourFrames", ps.obstacleDetourFrames).Prop("obstacleContacts", ps.obstacleContacts);
                j.EndObject();
            }
            j.EndArray();
            j.Key("failures").BeginArray();
            foreach (var f in m_failures)
            {
                j.BeginObject(true).Prop("pass", f.pass).Prop("kind", f.kind).Prop("s", f.s).Prop("lateral", f.lateral)
                 .Prop("x", f.pos.x).Prop("y", f.pos.y).Prop("z", f.pos.z).Prop("floorY", f.floorY).Prop("detail", f.detail).EndObject();
            }
            j.EndArray();
            j.Key("obstacles").BeginArray();
            foreach (var o in PathGuide.Obstacles)
                j.BeginObject(true).Prop("name", o.Name).Prop("cx", o.Box.center.x).Prop("cy", o.Box.center.y).Prop("cz", o.Box.center.z)
                 .Prop("sx", o.Box.size.x).Prop("sy", o.Box.size.y).Prop("sz", o.Box.size.z).EndObject();
            j.EndArray();
            j.Prop("coverage", CoverageNote).Prop("coverageMisses", CoverageMisses);
            j.Prop("cameraProbeViolations", CameraRig.Violations).Prop("cameraMaxPenetrationM", CameraRig.MaxPenetration);
        }
    }
}
