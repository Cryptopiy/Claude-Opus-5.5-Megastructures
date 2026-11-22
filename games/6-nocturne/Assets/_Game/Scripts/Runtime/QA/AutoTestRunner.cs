// AutoTestRunner: autoplay completability (MASTER_PROMPT 11.8, 14.10 Autoplay), imported from Thread and re-targeted to the
// Nocturne walk (IMPORT_PLAN 4.4 d, 8.13). Active with -autotest or -autotest-until S (and not -capture / -soak / -wallbot).
// After GameBootstrap.Ready it runs in virtual time (Time.captureDeltaTime = 1/60) with the 60 fps cap of Part 2.5.3,
// places the player at s = 0 and drives the SAME AutoWalker the player uses (Part 9):
//   -autotest            QA walk at 1.5 m/s (gaze forward): trigger (s 1497) at 998.1 +- 3 s, stop at s 1500 (about 1002.1 s)
//   -autotest -brisk     QA walk at 2.5 m/s (0.2 s ramp): trigger at 1497 / 2.5 + 0.1 = 598.9 +- 2 s
//   -autotest -cinematic the cinematic walk itself (2.5 m/s, 0.5 s ramp, gaze schedule, sway; non-cancellable):
//                        trigger at 599.05 +- 2 s, stop at 601.45 +- 2 s
//   -autotest-until S    smoke gate: passes as soon as s >= S with every beat up to S entered once and in order, no errors
// Full runs then wait for the 17 s ending (IFadeUI.EndingDone) and assert: trigger and stop times, rest at s 1500 (+- 0.05 m),
// beats 1..7 entered once each, in order, at s = 0, 120, 400, 560, 880, 1080, 1260 (within one frame of travel), the goal once,
// the ending complete 17.0 +- 0.2 s after the trigger, zero error log messages, PathGuide KillCount 0, collider coverage
// 0 misses, no safety snaps, no camera-sphere contacts, camera roll <= 0.01 deg (QA walk) / <= 0.3 deg (cinematic; 14.10).
// Crossing times on the runner clock (audit S3 item 8): the exact time at which s crosses every beat boundary and the goal
// trigger, interpolated inside the frame on the runner clock (t = 0 when the walk starts), checked against
// B / v + ramp / 2 (+- the 14.10 tolerance); the BeatDirector's own event times (one frame late, game clock) are reported too.
// -beat N / -progress X (GB-19): the walk starts at CliArgs.StartS (beat N's start) instead of s 0; beats N.. are expected once
// and in order, and every crossing and the trigger / stop times are measured from that start ((B - s0) / v + ramp / 2).
// Writes <report dir>/autotest_<mode>.json, logs
// "[NOC] AUTOTEST PASS/FAIL ..." (failures also "[NOC-FAIL]") and exits 0 / 1. Zero allocations per frame while walking.
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

namespace Megastructure
{
    [DefaultExecutionOrder(-50)]
    public sealed class AutoTestRunner : QaRunnerBase
    {
        public static AutoTestRunner Instance { get; private set; }

        const double RestSpeed = 0.01;            // m/s: at rest
        const double GoalRestTolerance = 0.05;    // rests at s 1500 (14.11)
        const double BeatSTolerance = 0.1;        // a beat is entered within one frame of travel of its boundary
        const double EndingTolerance = 0.2;       // the ending completes 17.0 +- 0.2 s after the trigger
        const double StallSeconds = 15.0, StallDistance = 0.1;
        const double TimeoutMarginSeconds = 60.0;
        /// <summary>14.10: camera roll <= 0.01 deg in manual walking (the QA walk), <= 0.3 deg in the cinematic.</summary>
        public const double RollMaxManual = 0.01, RollMaxCinematic = 0.3;
        /// <summary>Part 8 "no per-frame allocations": after the load fade + NocAlloc.SettleSeconds our code allocates 0 B per
        /// frame; the one steady allocation left is the engine's boxed bool of NativeInputSystem.NotifyBeforeUpdate
        /// (NocAlloc.EngineBytesPerFrame: 17 B recorder / 32 B Mono-heap granule). The mean above that constant may be at most
        /// GcExcessBytesPerFrameMax per frame plus GcTimeBytesPerSecondMax per second of the window: allocations that scale with
        /// time, not frames (event log lines, the audio heartbeat, engine timers): measured 1.8 B/frame at 166 fps in the
        /// development recorder (about 300 B/s) and 3.2 B/frame at 58 fps in the release smoke (Mono heap, 4-8 KiB blocks).</summary>
        public const double GcExcessBytesPerFrameMax = 1.0, GcTimeBytesPerSecondMax = 400.0;

        protected override QaRunMode Mode => QaRunMode.AutoTest;
        protected override string ReportName => "autotest_" + m_mode;
        protected override string ReportTag => "AUTOTEST";

        double m_speed;
        bool m_cinematic;
        string m_mode = "walk";
        double m_until = double.NaN;
        bool m_walking;
        double m_t0, m_expTrigger, m_expStop, m_tol;
        double m_tTrigger = -1, m_sTrigger = double.NaN, m_tStop = -1, m_sStop = double.NaN, m_tEnding = -1;
        double m_lastS, m_lastMoveT, m_maxSpeed, m_minCruise = double.MaxValue, m_maxCruise;
        long m_frames;
        /// <summary>Start progress of the walk (CliArgs.StartS: -beat N or -progress X, else 0) and its beat index (GB-19).</summary>
        double m_s0;
        int m_b0;
        readonly double[] m_beatS = new double[NocturneConstants.BeatCount];
        readonly double[] m_beatT = new double[NocturneConstants.BeatCount];
        readonly int[] m_beatHits = new int[NocturneConstants.BeatCount];
        int m_beatOrderErrors, m_lastBeat = -1, m_startArtifacts;
        Action<int, double> m_onBeat;
        // runner-clock crossings (index b = boundary of beat b, index BeatCount = goal trigger)
        readonly double[] m_crossT = new double[NocturneConstants.BeatCount + 1];
        double m_prevS = double.NaN, m_prevT;
        // camera roll (14.10)
        double m_maxRoll, m_maxRollS;
        long m_rollFrames;

        /// <summary>Expected walk seconds (from rest at s 0) to the goal trigger for a mode (smootherstep ramp: T / 2 is lost).</summary>
        /// <summary>-autotest-until S: NaN (no until: the whole walk with its trigger / stop checks) when S is NaN or at least the path
        /// length (HT's request), else S.</summary>
        public static double EffectiveUntil(double until) => double.IsNaN(until) || until >= NocturneConstants.PathLength ? double.NaN : until;

        public static double ExpectedTrigger(double speed, bool cinematic) => ExpectedTrigger(speed, cinematic, 0.0);

        /// <summary>
        /// The expected runner time [lo, hi] of the -autotest-until stop: before the goal trigger, the cruise time to S; at or past
        /// the trigger (AT's request: -autotest-until 1497..1500 compared the stop 169.30 with the trigger 166.90) S is crossed in the
        /// 14.11 stop ramp, or the walker comes to rest short of it (m_tStop is then the stop time), so the window runs from the
        /// expected trigger to the expected stop.
        /// </summary>
        public static void ExpectedUntil(double until, double s0, double speed, bool cinematic, double expTrigger, double expStop, out double lo, out double hi)
        {
            if (until < NocturneConstants.GoalTriggerS)
            {
                lo = hi = (until - s0) / speed + 0.5 * (cinematic ? NocturneConstants.CineAccelTime : NocturneConstants.Accel);
                return;
            }
            lo = expTrigger; hi = Math.Max(expTrigger, expStop);
        }

        /// <summary>Expected walk seconds from rest at s0 to the goal trigger (GB-19: -beat N starts at beat N's boundary).</summary>
        public static double ExpectedTrigger(double speed, bool cinematic, double s0)
        {
            double ramp = cinematic ? NocturneConstants.CineAccelTime : NocturneConstants.Accel;
            return (NocturneConstants.GoalTriggerS - s0) / speed + 0.5 * ramp;
        }

        /// <summary>Expected runner-clock time at which the walk crosses s = b (from rest at s 0; the ramp loses T / 2).</summary>
        public static double ExpectedCrossing(double b, double speed, bool cinematic) => ExpectedCrossing(b, speed, cinematic, 0.0);

        /// <summary>Expected runner-clock time at which a walk from rest at s0 crosses s = b (0 for the start boundary itself).</summary>
        public static double ExpectedCrossing(double b, double speed, bool cinematic, double s0)
        {
            if (b <= s0) return 0.0;
            double ramp = cinematic ? NocturneConstants.CineAccelTime : NocturneConstants.Accel;
            return (b - s0) / speed + 0.5 * ramp;
        }

        /// <summary>Camera roll in degrees: the elevation of the camera's right axis (0 for a level horizon at any yaw and pitch).</summary>
        public static double RollDeg(Vector3 right) => Math.Asin(Mathf.Clamp(right.y, -1f, 1f)) / NocMath.Deg;

        /// <summary>Stop ramp duration T = 6 / v0 (14.11).</summary>
        public static double StopRampSeconds(double speed) => 2.0 * (NocturneConstants.GoalS - NocturneConstants.GoalTriggerS) / speed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsurePresent() { EnsureRunner<AutoTestRunner>(QaRunMode.AutoTest); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; }

        protected override void Awake()
        {
            base.Awake();
            if (!enabled) return;
            Instance = this;
            m_cinematic = CliArgs.Cinematic;
            m_speed = m_cinematic ? NocturneConstants.CinematicSpeed : QaMode.DriveSpeed;
            m_until = CliArgs.AutoTestUntil;
            // HT's request (QA fix wave): an until at or past the path end is no until, so the end-of-walk trigger / stop checks
            // apply (-autotest-until 1500 -beat 7 failed untilTime: stop 97.30 s vs the trigger 94.90 +- 2.0)
            m_until = EffectiveUntil(m_until);
            m_s0 = Math.Max(0.0, Math.Min(CliArgs.StartS, NocturneConstants.GoalTriggerS - 1.0));
            m_b0 = NocMath.BeatIndex(m_s0);
            m_lastBeat = m_b0 - 1;
            m_mode = m_cinematic ? "cinematic" : QaMode.SpeedName((float)m_speed);
            if (m_s0 > 0.0) m_mode += "_from" + ((int)Math.Round(m_s0)).ToString(CultureInfo.InvariantCulture);
            if (!double.IsNaN(m_until)) m_mode += "_until" + ((int)Math.Round(m_until)).ToString(CultureInfo.InvariantCulture);
            m_expTrigger = ExpectedTrigger(m_speed, m_cinematic, m_s0);
            m_expStop = m_expTrigger + StopRampSeconds(m_speed);
            m_tol = m_speed >= NocturneConstants.BriskSpeed - 1e-6 ? NocturneConstants.CineTimeTol : NocturneConstants.WalkTimeTol;
            for (int i = 0; i < m_beatS.Length; i++) { m_beatS[i] = double.NaN; m_beatT[i] = double.NaN; }
            for (int i = 0; i < m_crossT.Length; i++) m_crossT[i] = double.NaN;
            m_onBeat = OnBeatEntered;
            BeatDirector.BeatEntered += m_onBeat;
        }

        void OnDestroy()
        {
            if (m_onBeat != null) BeatDirector.BeatEntered -= m_onBeat;
            if (Instance == this) Instance = null;
        }

        void OnBeatEntered(int beat, double s)
        {
            if (beat < 0 || beat >= m_beatS.Length) return;
            // GB-19: a -beat N walk starts exactly on beat N's boundary. Before PlayerMath.AnchoredS the player's s projected a few
            // 1e-5 m short of it and the BeatDirector entered beat N - 1 once (GE_autotest_b3: beat 2 at s 400.00, t 0.03, after
            // beat 3 at t 0.02): such a start-boundary artifact is counted apart (never a walk entry) and fails beatsInOrder
            if (!m_walking && beat < m_b0 && s >= m_s0 - 0.05) { m_startArtifacts++; return; }
            m_beatHits[beat]++;
            if (beat != m_lastBeat + 1) m_beatOrderErrors++;
            m_lastBeat = Math.Max(m_lastBeat, beat);
            if (double.IsNaN(m_beatS[beat])) { m_beatS[beat] = s; m_beatT[beat] = m_walking ? Time.timeAsDouble - m_t0 : 0; }
        }

        IEnumerator Start()
        {
            yield return WaitForWorld();
            if (!WorldOk) { Checks.Add("worldReady", false, "GameBootstrap.Ready never became true"); Finish("world not ready"); yield break; }
            QaDrive.Prepare(true, false);
            yield return null;
            yield return null;
            RunStaticChecks();
            var p = NocServices.Player;
            if (p == null) { Checks.Add("playerService", false, "NocServices.Player is null"); Finish("no player"); yield break; }
            p.Frozen = false;
            // GB-19: the player already stands at CliArgs.StartS, exactly (PlayerMath.AnchoredS holds the placement s until the
            // feet move, so a -beat N start no longer re-enters beat N - 1 a float step short of the boundary)
            if (m_s0 <= 0.0) p.Teleport(0, 0);
            else if (Math.Abs(p.S - m_s0) > 0.05) p.Teleport(m_s0, 0);
            yield return null;
            if (m_cinematic) AutoWalker.BeginCinematicQa();
            else p.SetDrive(true, (float)m_speed, float.NaN);
            m_t0 = Time.timeAsDouble;
            m_crossT[m_b0] = 0.0;   // the walk starts at rest at s0 (beat b0's boundary): crossed at t 0 on the runner clock
            m_lastS = p.S;
            m_lastMoveT = 0;
            m_walking = true;
            var inv = CultureInfo.InvariantCulture;
            Debug.Log("[NOC] AUTOTEST start mode=" + m_mode + " speed=" + m_speed.ToString("0.00", inv)
                      + " expected trigger=" + m_expTrigger.ToString("0.00", inv) + " stop=" + m_expStop.ToString("0.00", inv) + " +- " + m_tol.ToString("0.0", inv)
                      + " s virtual from s=" + m_s0.ToString("0.0", inv) + (double.IsNaN(m_until) ? "" : " until s=" + m_until.ToString("0.0", inv)));
        }

        void Update()
        {
            if (!m_walking || Finished) return;
            var p = NocServices.Player;
            if (p == null) { Finish("player service lost"); return; }
            double t = Time.timeAsDouble - m_t0;
            double s = p.S;
            m_frames++;
            if (p.Speed > m_maxSpeed) m_maxSpeed = p.Speed;
            if (t > 2.0 && !p.Finished) { m_minCruise = Math.Min(m_minCruise, p.Speed); m_maxCruise = Math.Max(m_maxCruise, p.Speed); }
            if (!m_cinematic) QaDrive.LookForward(s, (float)NocturneConstants.GazeDefaultPitch);
            RecordCrossings(s, t);
            RecordRoll(s);

            if (s > m_lastS + StallDistance) { m_lastS = s; m_lastMoveT = t; }
            if (!p.Finished && t - m_lastMoveT > StallSeconds) { Finish("stalled at s=" + s.ToString("0.00", CultureInfo.InvariantCulture)); return; }

            // -autotest-until: record the crossing, finish one frame later (the BeatDirector logs a boundary beat in LateUpdate)
            if (!double.IsNaN(m_until) && s >= m_until)
            {
                if (m_tStop < 0) { m_tStop = t; m_sStop = s; return; }
                Finish(null); return;
            }

            if (p.Finished && m_tTrigger < 0)
            {
                // the exact crossing time from the walker (resolved inside the frame), else this frame
                var w = AutoWalker.Instance != null ? AutoWalker.Instance.Planner : null;
                m_tTrigger = w != null && !double.IsNaN(w.TriggerTime) ? w.TriggerTime : t;
                m_sTrigger = s;
                Debug.Log("[NOC] AUTOTEST goal trigger at s=" + s.ToString("0.000", CultureInfo.InvariantCulture) + " t=" + m_tTrigger.ToString("0.000", CultureInfo.InvariantCulture));
            }
            var wp = AutoWalker.Instance != null ? AutoWalker.Instance.Planner : null;
            // at rest: the walker's ramp has ended (its exact stop time), not merely a speed below 1 cm/s (the ramp tail
            // drops below that about 0.1 s before T)
            if (m_tTrigger >= 0 && m_tStop < 0 && Math.Abs(p.Speed) < RestSpeed && s >= NocturneConstants.GoalS - 0.5 && (wp == null || wp.Finished))
            {
                var w = wp;
                m_tStop = w != null && !double.IsNaN(w.StopTime) ? w.StopTime : t;
                m_sStop = s;
                Debug.Log("[NOC] AUTOTEST stopped at s=" + s.ToString("0.000", CultureInfo.InvariantCulture) + " t=" + m_tStop.ToString("0.000", CultureInfo.InvariantCulture));
            }
            if (m_tStop >= 0 && m_tEnding < 0)
            {
                var f = NocServices.Fade;
                if (f == null) { Checks.Add("fadeService", false, "NocServices.Fade is null: the 17 s ending cannot be measured"); Finish(null); return; }
                if (f.EndingDone)
                {
                    m_tEnding = t;
                    Finish(null);
                    return;
                }
            }
            double limit = (double.IsNaN(m_until) ? m_expStop + NocturneConstants.EndingSeconds : (m_until - m_s0) / m_speed) + m_tol + TimeoutMarginSeconds;
            if (t > limit) Finish("timeout at s=" + s.ToString("0.00", CultureInfo.InvariantCulture));
        }

        /// <summary>Interpolates the runner-clock time of every boundary (and the goal trigger) crossed since the last frame.</summary>
        void RecordCrossings(double s, double t)
        {
            if (double.IsNaN(m_prevS))
            {
                m_prevS = s; m_prevT = t;
                if (s <= m_s0 + 1e-9) m_crossT[m_b0] = 0.0;   // the start beat begins at rest at s0
                return;
            }
            if (s > m_prevS)
            {
                var b = NocturneConstants.BeatBoundaries;
                for (int i = 0; i <= NocturneConstants.BeatCount; i++)
                {
                    double x = i < NocturneConstants.BeatCount ? b[i] : NocturneConstants.GoalTriggerS;
                    if (!double.IsNaN(m_crossT[i]) || x <= m_s0) continue;
                    if (m_prevS < x && s >= x) m_crossT[i] = m_prevT + (x - m_prevS) / (s - m_prevS) * (t - m_prevT);
                }
            }
            m_prevS = s; m_prevT = t;
        }

        /// <summary>Largest |roll| of the camera over the walk (14.10; Part 10 item 18).</summary>
        void RecordRoll(double s)
        {
            var cc = NocServices.CameraControl;
            Camera cam = cc != null ? cc.Camera : Camera.main;
            if (cam == null) return;
            double r = Math.Abs(RollDeg(cam.transform.right));
            m_rollFrames++;
            if (r > m_maxRoll) { m_maxRoll = r; m_maxRollS = s; }
        }

        void Finish(string abort)
        {
            m_walking = false;
            var inv = CultureInfo.InvariantCulture;
            bool until = !double.IsNaN(m_until);
            var bd = BeatDirector.Instance;
            // beats b0 .. beatsExpected - 1 are expected (GB-19: -beat N starts at beat N)
            int beatsExpected = until ? NocMath.BeatIndex(Math.Min(m_until, NocturneConstants.GoalS - 1e-6)) + 1 : NocturneConstants.BeatCount;
            int beatsCount = Math.Max(0, beatsExpected - m_b0);

            if (until)
            {
                Checks.Add("reachedUntil", abort == null && m_tStop >= 0, abort ?? ("s=" + m_sStop.ToString("0.000", inv) + " at t=" + m_tStop.ToString("0.00", inv) + " s"));
                ExpectedUntil(m_until, m_s0, m_speed, m_cinematic, m_expTrigger, m_expStop, out double expLo, out double expHi);
                Checks.Add("untilTime", m_tStop >= 0 && m_tStop >= expLo - m_tol && m_tStop <= expHi + m_tol,
                           m_tStop.ToString("0.00", inv) + " s vs " + expLo.ToString("0.00", inv) + (expHi > expLo ? ".." + expHi.ToString("0.00", inv) + " (trigger..stop)" : "") + " +- " + m_tol.ToString("0.0", inv));
            }
            else
            {
                Checks.Add("reachedGoal", abort == null && m_tStop >= 0, abort ?? ("stopped at t=" + m_tStop.ToString("0.000", inv) + " s, s=" + m_sStop.ToString("0.000", inv)));
                Checks.Add("triggerTime", m_tTrigger >= 0 && Math.Abs(m_tTrigger - m_expTrigger) <= m_tol,
                           m_tTrigger.ToString("0.000", inv) + " s vs " + m_expTrigger.ToString("0.000", inv) + " +- " + m_tol.ToString("0.0", inv));
                Checks.Add("triggerAt1497", m_tTrigger >= 0 && m_sTrigger >= NocturneConstants.GoalTriggerS - 1e-6, "trigger s=" + m_sTrigger.ToString("0.000", inv));
                Checks.Add("stopTime", m_tStop >= 0 && Math.Abs(m_tStop - m_expStop) <= m_tol,
                           m_tStop.ToString("0.000", inv) + " s vs " + m_expStop.ToString("0.000", inv) + " +- " + m_tol.ToString("0.0", inv));
                Checks.Add("restAt1500", m_tStop >= 0 && Math.Abs(m_sStop - NocturneConstants.GoalS) <= GoalRestTolerance, "rest s=" + m_sStop.ToString("0.000", inv));
                double ending = m_tEnding >= 0 && m_tTrigger >= 0 ? m_tEnding - m_tTrigger : double.NaN;
                Checks.Add("ending17s", !double.IsNaN(ending) && Math.Abs(ending - NocturneConstants.EndingSeconds) <= EndingTolerance,
                           "ending complete " + ending.ToString("0.00", inv) + " s after the trigger (17.0 +- " + EndingTolerance.ToString("0.0", inv) + ")");
                if (bd != null) Checks.Add("goalOnce", bd.GoalCount == 1, "goal count " + bd.GoalCount);
                Checks.Add("constantCruise", m_maxCruise - m_minCruise < 0.05 || m_minCruise == double.MaxValue,
                           "plan speed during the walk " + (m_minCruise == double.MaxValue ? "n/a" : m_minCruise.ToString("0.000", inv) + " .. " + m_maxCruise.ToString("0.000", inv)) + " m/s", false);
            }
            if (bd != null)
                // BeatDirector.OrderOk accepts a walk that starts mid-path (its own beat first, BeatDirector.OrderStepOk), and the
                // start sits exactly on the boundary: a start-boundary artifact (beat N - 1 entered at the start) fails the check
                Checks.Add("beatsInOrder", bd.EnteredCount - m_startArtifacts == beatsCount && bd.OrderOk && m_startArtifacts == 0 && m_beatOrderErrors == 0,
                           "entered " + bd.EnteredCount + " of " + beatsCount + " (beats " + (m_b0 + 1) + ".." + beatsExpected + "; start-boundary artifacts " + m_startArtifacts
                           + "), orderOk " + bd.OrderOk + ", order errors " + m_beatOrderErrors);
            else Checks.Add("beatDirector", false, "BeatDirector.Instance is null");
            bool once = true, atBoundary = true;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < m_b0; i++) if (m_beatHits[i] != 0) once = false;   // beats before the start are never entered
            for (int i = m_b0; i < beatsExpected; i++)
            {
                if (m_beatHits[i] != 1) once = false;
                double b = NocturneConstants.BeatBoundaries[i];
                if (double.IsNaN(m_beatS[i]) || m_beatS[i] < b - 1e-6 || m_beatS[i] > b + BeatSTolerance) atBoundary = false;
                sb.Append(i == m_b0 ? "" : ", ").Append(m_beatS[i].ToString("0.000", inv));
            }
            // runner-clock crossing times of the boundaries (and the goal trigger) against B / v + ramp / 2
            bool timesOk = true;
            var ct = new System.Text.StringBuilder();
            int crossN = until ? beatsExpected : NocturneConstants.BeatCount + 1;
            for (int i = m_b0; i < crossN; i++)
            {
                double x = i < NocturneConstants.BeatCount ? NocturneConstants.BeatBoundaries[i] : NocturneConstants.GoalTriggerS;
                double exp = ExpectedCrossing(x, m_speed, m_cinematic, m_s0);
                if (double.IsNaN(m_crossT[i]) || Math.Abs(m_crossT[i] - exp) > m_tol) timesOk = false;
                ct.Append(i == m_b0 ? "" : ", ").Append(m_crossT[i].ToString("0.000", inv)).Append('/').Append(exp.ToString("0.00", inv));
            }
            Checks.Add("crossingTimes", timesOk, "runner clock t at s = boundaries" + (until ? "" : " + trigger") + " (measured/expected, +- " + m_tol.ToString("0.0", inv) + " s): " + ct);
            double rollMax = m_cinematic ? RollMaxCinematic : RollMaxManual;
            Checks.Add("cameraRoll", m_rollFrames > 0 && m_maxRoll <= rollMax,
                       "max |roll| " + m_maxRoll.ToString("0.0000", inv) + " deg at s=" + m_maxRollS.ToString("0.0", inv) + " over " + m_rollFrames + " frames (<= " + rollMax.ToString("0.00", inv) + ")");
            Checks.Add("beatEventsOnce", once, "BeatEntered events per beat: " + string.Join(",", Array.ConvertAll(m_beatHits, h => h.ToString(inv))));
            Checks.Add("beatsAtBoundaries", atBoundary, "entered at s = " + sb);
            Checks.Add("noSafetySnaps", PlayerController.SafetySnaps == 0, PlayerController.SafetySnaps + " safety snaps");
            Checks.Add("noCameraSphereContacts", CameraRig.SphereContacts == 0, CameraRig.SphereContacts + " frames");
            double gcAllowed = GcExcessBytesPerFrameMax + (NocAlloc.SteadyFrames > 0 ? GcTimeBytesPerSecondMax * NocAlloc.SteadySeconds / NocAlloc.SteadyFrames : 0.0);
            Checks.Add("gcAllocPerFrame", NocAlloc.SteadyFrames > 0 && NocAlloc.SteadyExcessBytesPerFrame <= gcAllowed,
                       NocAlloc.Describe() + " over " + NocAlloc.SteadySeconds.ToString("0.0", inv) + " s (excess over the engine constant <= " +
                       GcExcessBytesPerFrameMax.ToString("0.0", inv) + " B/frame + " + GcTimeBytesPerSecondMax.ToString("0", inv) + " B/s = " +
                       gcAllowed.ToString("0.00", inv) + " B/frame)");
            AddCommonChecks();

            string summary = "mode=" + m_mode + " trigger=" + m_tTrigger.ToString("0.000", inv) + "s stop=" + m_tStop.ToString("0.000", inv) + "s"
                             + " (expected " + m_expTrigger.ToString("0.00", inv) + " / " + m_expStop.ToString("0.00", inv) + ")"
                             + " ending=" + (m_tEnding >= 0 && m_tTrigger >= 0 ? (m_tEnding - m_tTrigger).ToString("0.00", inv) : "n/a") + "s"
                             + " beats=" + (bd != null ? bd.EnteredCount : -1) + " errors=" + QaLog.ErrorCount;
            Complete(summary, WriteBody);
        }

        void WriteBody(QaJson j)
        {
            j.Prop("mode", m_mode).Prop("speed", m_speed).Prop("cinematic", m_cinematic).Prop("virtualFps", QaDrive.VirtualFps);
            j.Prop("until", double.IsNaN(m_until) ? -1.0 : m_until).Prop("startS", m_s0).Prop("startBeat", m_b0 + 1);
            j.Prop("expectedTriggerSeconds", m_expTrigger).Prop("expectedStopSeconds", m_expStop).Prop("toleranceSeconds", m_tol);
            j.Prop("triggerSeconds", m_tTrigger).Prop("triggerS", m_sTrigger);
            j.Prop("stopSeconds", m_tStop).Prop("stopS", m_sStop);
            j.Prop("endingSeconds", m_tEnding >= 0 && m_tTrigger >= 0 ? m_tEnding - m_tTrigger : -1.0);
            j.Prop("frames", m_frames).Prop("maxSpeed", m_maxSpeed);
            j.Prop("safetySnaps", PlayerController.SafetySnaps).Prop("cameraSphereContacts", CameraRig.SphereContacts);
            j.Prop("maxRollDeg", m_maxRoll).Prop("maxRollS", m_maxRollS).Prop("rollLimitDeg", m_cinematic ? RollMaxCinematic : RollMaxManual).Prop("rollFrames", m_rollFrames);
            j.Key("crossings").BeginArray();
            for (int i = 0; i < m_crossT.Length; i++)
            {
                double x = i < NocturneConstants.BeatCount ? NocturneConstants.BeatBoundaries[i] : NocturneConstants.GoalTriggerS;
                j.BeginObject(true).Prop("s", x).Prop("runnerT", m_crossT[i]).Prop("expectedT", ExpectedCrossing(x, m_speed, m_cinematic, m_s0)).EndObject();
            }
            j.EndArray();
            var gb = GameBootstrap.Instance;
            if (gb != null) j.Prop("loadSeconds", gb.LoadSeconds);
            var bd2 = BeatDirector.Instance;
            j.Key("beats").BeginArray();
            for (int i = 0; i < m_beatS.Length; i++)
            {
                j.BeginObject(true).Prop("beat", i + 1).Prop("name", NocturneConstants.BeatNames[i]).Prop("boundary", NocturneConstants.BeatBoundaries[i])
                 .Prop("enteredS", m_beatS[i]).Prop("enteredT", m_beatT[i]).Prop("crossingT", m_crossT[i])
                 .Prop("directorT", bd2 != null ? bd2.EnteredAtT[i] : double.NaN).Prop("events", m_beatHits[i]).EndObject();
            }
            j.EndArray();
            j.Prop("coverage", CoverageNote).Prop("coverageMisses", CoverageMisses);
            j.Key("gcAlloc").BeginObject(true).Prop("mode", NocAlloc.Mode).Prop("steadyFrames", NocAlloc.SteadyFrames)
             .Prop("bytesPerFrame", NocAlloc.SteadyBytesPerFrame).Prop("engineBytesPerFrame", NocAlloc.EngineBytesPerFrame)
             .Prop("excessBytesPerFrame", NocAlloc.SteadyExcessBytesPerFrame).Prop("steadySeconds", NocAlloc.SteadySeconds).Prop("worstFrameBytes", NocAlloc.SteadyMaxFrameBytes)
             .Prop("framesAllocating", NocAlloc.SteadyAllocFrames).Prop("totalBytes", NocAlloc.SteadyBytes)
             .Prop("collections", NocAlloc.SteadyCollections).EndObject();
            j.Prop("devices", Devices);
        }
    }
}
