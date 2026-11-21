// AutoTestRunner: autoplay completability (MASTER_PROMPT 11.8, 14.10.4, Part 9). Imported from Thread's AutoTestRunner.
// Active with -autotest (and not -capture / -soak / -wallbot, which have their own runners). After GameBootstrap.Ready it
// sets Time.captureDeltaTime = 1/60 (virtual time, deterministic; the frame rate is capped at 60, Part 2.5.3) and starts
// the SAME AutoWalker the cinematic uses (D-02: same seed, gaze and lateral keys) from s 0: at 1.5 m/s for -autotest, at the
// cinematic 2.5 m/s for -autotest -cinematic (never forced to 1.5: The Ring's QaHooks bug). It waits for the ENDING to finish.
// Asserts (14.10.4 with ADDENDUM A1 00, the new ending): the walk boards the cabin at 1498.5, the 143 s ride (fixed at every
// speed), the platform walk; goal trigger at the Keeper's Lantern at FrameworkConst.TriggerTimeAt (1170.17 +- 4 s at 1.5 m/s,
// 759.50 +- 3 s at 2.5 m/s); the ride measured 143 +- 0.1 s with the EndingRide phases in order; ending done 55.5 s later;
// the run is over by done + 8 s; rest at EndingPath.SRest; progress speed on the walked path equals the commanded speed after
// the 0.5 s ramp; ground speed peak <= 2.60 m/s;
// max |roll| <= 0.01 deg in both walks (the camera never rolls; the 0.3 deg cinematic allowance is not needed); beats 1..7 entered once and in order;
// the lantern once; zero error logs; no safety-volume events; zero CharacterController rejections and camera-probe
// violations; full collider coverage. Writes <report dir>/autotest_<mode>.json, logs "[BW] AUTOTEST PASS/FAIL ..." and
// exits 0 / 1. Zero allocations per frame while walking.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Megastructure.Player;
using Megastructure.UI;

namespace Megastructure.QA
{
    [DefaultExecutionOrder(-50)]
    public sealed class AutoTestRunner : QaRunnerBase
    {
        public static AutoTestRunner Instance { get; private set; }

        const double GoalRestTolerance = 0.1;     // comes to rest at EndingPath.SRest (14.11, the platform)
        const double RideTolerance = 0.1;         // the ride lasts RideTotalTime at every speed
        const double StallSeconds = 15.0, StallDistance = 0.1;
        const double EndingTimeoutSeconds = 30.0;
        const double SpeedTolerance = 0.001;      // progress speed 2.500 +- 0.001 (B 5.7)

        protected override QaRunMode Mode => QaRunMode.AutoTest;
        protected override string ReportName => "autotest_" + m_mode;
        protected override string ReportTag => "AUTOTEST";

        float m_speed;
        string m_mode = "walk";
        bool m_walking;
        double m_t0, m_expTrigger, m_expDone, m_exitBy, m_tol;
        double m_tTrigger = -1, m_sTrigger = double.NaN, m_tDone = -1, m_sRest = double.NaN;
        double m_lastS, m_lastMoveT, m_maxGround, m_minPath = double.MaxValue, m_maxPath;
        double m_prevS = double.NaN;
        long m_frames;
        double m_tRide0 = -1, m_tRide1 = -1;
        int m_lastPhase, m_phaseOrderErrors, m_phasesSeen;
        double m_maxCabinSpeed;
        readonly double[] m_beatS = new double[FrameworkConst.BeatCount];
        readonly double[] m_beatT = new double[FrameworkConst.BeatCount];
        readonly int[] m_beatHits = new int[FrameworkConst.BeatCount];
        Action<int, double> m_onBeat;

        /// <summary>14.10.4 expected trigger time for a test speed, computed from the path and the speed (0.5 s smootherstep ramp
        /// from rest loses 0.25 s): 999.25 s at 1.5 m/s, 599.65 s at 2.5 m/s.</summary>
        public static double ExpectedTrigger(float speed) => FrameworkConst.TriggerTimeAt(speed);

        public static double ToleranceSeconds(float speed) => speed >= FrameworkConst.CinematicSpeed - 1e-4 ? FrameworkConst.CineTriggerTol : FrameworkConst.ManualTriggerTol;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsurePresent() { EnsureRunner<AutoTestRunner>(QaRunMode.AutoTest); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; }

        protected override void Awake()
        {
            base.Awake();
            if (!enabled) return;
            Instance = this;
            m_speed = QaMode.DriveSpeed;
            m_mode = QaMode.SpeedName(m_speed);
            m_expTrigger = ExpectedTrigger(m_speed);
            m_expDone = m_expTrigger + FrameworkConst.EndTotal;
            bool cine = Math.Abs(m_speed - FrameworkConst.CinematicSpeed) < 1e-4;
            m_exitBy = cine ? FrameworkConst.CineExitBy : (Math.Abs(m_speed - FrameworkConst.WalkSpeed) < 1e-4 ? FrameworkConst.ManualExitBy : m_expDone + 8.0);
            m_tol = ToleranceSeconds(m_speed);
            for (int i = 0; i < m_beatS.Length; i++) { m_beatS[i] = double.NaN; m_beatT[i] = double.NaN; }
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
            m_beatHits[beat]++;
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
            var walker = AutoWalker.Instance;
            var p = BwServices.Player;
            if (p == null || walker == null) { Checks.Add("playerService", false, "PlayerController / AutoWalker missing"); Finish("no player"); yield break; }
            p.Frozen = false;
            CameraRig.ResetStatics();
            walker.StartTest(m_speed);
            m_t0 = Time.timeAsDouble;
            m_lastS = p.S;
            m_lastMoveT = 0;
            m_walking = true;
            Debug.Log("[BW] AUTOTEST start mode=" + m_mode + " speed=" + m_speed.ToString("0.00", CultureInfo.InvariantCulture)
                      + " expected trigger " + m_expTrigger.ToString("0.00", CultureInfo.InvariantCulture) + " +- " + m_tol + " s virtual");
        }

        void Update()
        {
            if (!m_walking || Finished) return;
            var p = BwServices.Player;
            if (p == null) { Finish("player service lost"); return; }
            double t = Time.timeAsDouble - m_t0;
            double s = p.S;
            m_frames++;
            if (p.GroundSpeed > m_maxGround) m_maxGround = p.GroundSpeed;
            // progress speed after the start ramp and before the trigger (Part 9: constant, no slowing)
            var pc = PlayerController.Instance;
            bool riding = pc != null && pc.Riding;
            if (riding && pc.RideTime > 0 && m_tRide0 < 0) m_tRide0 = t - pc.RideTime;
            if (!riding && m_tRide0 >= 0 && m_tRide1 < 0 && s >= EndingPath.SFoot0 - 1e-6) m_tRide1 = t;
            int ph = (int)EndingRide.Phase;
            if (ph != m_lastPhase)
            {
                if (ph != 0 && ph < m_lastPhase) m_phaseOrderErrors++;
                if (ph != 0) m_phasesSeen |= 1 << ph;
                m_lastPhase = ph;
            }
            if (EndingRide.Speed > m_maxCabinSpeed) m_maxCabinSpeed = EndingRide.Speed;
            if (!p.Finished && s < FrameworkConst.GoalTriggerS && t > FrameworkConst.StartRamp + 0.05 && !double.IsNaN(m_prevS))
            {
                double v = p.Speed;   // progress speed ds/dt
                if (v < m_minPath) m_minPath = v;
                if (v > m_maxPath) m_maxPath = v;
            }
            m_prevS = s;

            if (p.Finished && m_tTrigger < 0)
            {
                m_tTrigger = t;
                m_sTrigger = s;
                Debug.Log("[BW] AUTOTEST goal trigger at s=" + s.ToString("0.000", CultureInfo.InvariantCulture) + " t=" + t.ToString("0.00", CultureInfo.InvariantCulture));
            }

            if (s > m_lastS + StallDistance) { m_lastS = s; m_lastMoveT = t; }
            if (!p.Finished && t - m_lastMoveT > StallSeconds) { Finish("stalled at s=" + s.ToString("0.00", CultureInfo.InvariantCulture)); return; }

            if (m_tTrigger >= 0 && UiDirector.EndingDone && m_tDone < 0)
            {
                m_tDone = t;
                m_sRest = s;
                Finish(null);
                return;
            }
            if (m_tTrigger >= 0 && t - m_tTrigger > FrameworkConst.EndTotal + EndingTimeoutSeconds) { Finish("the ending never completed"); return; }
            if (t > m_exitBy + 60.0) Finish("timeout at s=" + s.ToString("0.00", CultureInfo.InvariantCulture));
        }

        void Finish(string abort)
        {
            m_walking = false;
            var inv = CultureInfo.InvariantCulture;
            bool done = abort == null && m_tDone >= 0;
            Checks.Add("endingDone", done, abort ?? ("ending done at t=" + m_tDone.ToString("0.00", inv) + " s"));
            Checks.Add("triggerTime", m_tTrigger >= 0 && Math.Abs(m_tTrigger - m_expTrigger) <= m_tol,
                       m_tTrigger.ToString("0.00", inv) + " s vs " + m_expTrigger.ToString("0.00", inv) + " +- " + m_tol + " s");
            Checks.Add("endingTime", done && Math.Abs(m_tDone - m_expDone) <= m_tol,
                       m_tDone.ToString("0.00", inv) + " s vs " + m_expDone.ToString("0.00", inv) + " +- " + m_tol + " s");
            Checks.Add("exitInTime", done && m_tDone <= m_exitBy, "done " + m_tDone.ToString("0.00", inv) + " s, limit " + m_exitBy.ToString("0", inv) + " s");
            Checks.Add("triggerAtGoal", m_tTrigger >= 0 && m_sTrigger >= FrameworkConst.EndGoalS - 1e-6, "trigger s=" + m_sTrigger.ToString("0.000", inv) + " (lantern " + FrameworkConst.EndGoalS.ToString("0.000", inv) + ")");
            Checks.Add("restAtLantern", done && Math.Abs(m_sRest - FrameworkConst.EndRestS) <= GoalRestTolerance, "rest s=" + m_sRest.ToString("0.000", inv) + " (expected " + FrameworkConst.EndRestS.ToString("0.000", inv) + ")");
            double ride = m_tRide1 >= 0 && m_tRide0 >= 0 ? m_tRide1 - m_tRide0 : -1;
            Checks.Add("rideTime", Math.Abs(ride - FrameworkConst.RideTotalTime) <= RideTolerance, "ride " + ride.ToString("0.00", inv) + " s vs " + FrameworkConst.RideTotalTime.ToString("0.0", inv) + " s");
            const int allPhases = (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5);
            Checks.Add("ridePhasesInOrder", m_phaseOrderErrors == 0 && m_phasesSeen == allPhases,
                       "phases seen mask " + m_phasesSeen + " (expected " + allPhases + "), order errors " + m_phaseOrderErrors + ", peak cabin speed " + m_maxCabinSpeed.ToString("0.0", inv) + " m/s");
            Checks.Add("progressSpeedConstant", m_minPath <= m_maxPath && Math.Abs(m_minPath - m_speed) <= SpeedTolerance && Math.Abs(m_maxPath - m_speed) <= SpeedTolerance,
                       "ds/dt " + m_minPath.ToString("0.0000", inv) + ".." + m_maxPath.ToString("0.0000", inv) + " vs " + m_speed.ToString("0.000", inv));
            Checks.Add("groundSpeedPeak", m_maxGround <= FrameworkConst.GroundSpeedPeakMax * (m_speed / FrameworkConst.CinematicSpeed) + 1e-3,
                       "peak " + m_maxGround.ToString("0.000", inv) + " m/s");
            // 14.10.4 "roll 0.00 +- 0.01 deg manual, at most 0.3 cinematic": the camera never rolls (CameraRig builds the rotation
            // from yaw and pitch only and the walker's sway is yaw / pitch), so both walks assert the strict bound.
            Checks.Add("roll", CameraRig.MaxRollDeg <= FrameworkConst.RollTolDeg,
                       "max |roll| " + CameraRig.MaxRollDeg.ToString("0.0000", inv) + " deg (limit " + FrameworkConst.RollTolDeg.ToString("0.00", inv) + ")");
            Checks.Add("cameraProbe", CameraRig.Violations == 0, CameraRig.Violations + " frames with the 0.2 m probe inside a collider");
            // PathGuide obstacles (props inside the lane) are failures only on the cinematic path (FRAMEWORK.md 4.0)
            var onPath = new List<string>();
            int blocked = PathGuide.ObstaclesOnCinematicPath(0.25, onPath);
            Checks.Add("cinematicPathClearOfObstacles", blocked == 0,
                       PathGuide.Obstacles.Count + " obstacles registered, " + blocked + " on the cinematic capsule path" + (blocked > 0 ? ": " + string.Join("; ", onPath) : ""));
            var pcf = PlayerController.Instance;
            if (pcf != null)
            {
                var pc = pcf;
                Checks.Add("noCcRejections", pc.Rejections == 0, pc.Rejections + " CharacterController rejections");
                Checks.Add("noSafetySnaps", pc.SafetySnaps == 0, pc.SafetySnaps + " safety snaps");
            }
            var bd = BeatDirector.Instance;
            if (bd != null)
            {
                Checks.Add("beatsInOrder", bd.EnteredCount == FrameworkConst.BeatCount && bd.OrderOk, "entered " + bd.EnteredCount + ", orderOk " + bd.OrderOk);
                Checks.Add("lanternOnce", bd.GoalCount == 1, "goal count " + bd.GoalCount);
            }
            else Checks.Add("beatDirector", false, "BeatDirector.Instance is null");
            bool once = true;
            for (int i = 0; i < m_beatHits.Length; i++) if (m_beatHits[i] != 1) once = false;
            Checks.Add("beatEventsOnce", once, "BeatEntered events per beat: " + string.Join(",", Array.ConvertAll(m_beatHits, h => h.ToString(inv))));
            AddCommonChecks();

            string summary = "mode=" + m_mode + " trigger=" + m_tTrigger.ToString("0.00", inv) + "s (expected " + m_expTrigger.ToString("0.00", inv)
                             + " +- " + m_tol + ") done=" + m_tDone.ToString("0.00", inv) + "s beats=" + (bd != null ? bd.EnteredCount : -1)
                             + " roll=" + CameraRig.MaxRollDeg.ToString("0.0000", inv) + "deg errors=" + QaLog.ErrorCount;
            Complete(summary, WriteBody);
        }

        void WriteBody(QaJson j)
        {
            j.Prop("mode", m_mode).Prop("speed", m_speed).Prop("virtualFps", QaDrive.VirtualFps);
            j.Prop("expectedTriggerSeconds", m_expTrigger).Prop("expectedDoneSeconds", m_expDone).Prop("toleranceSeconds", m_tol).Prop("exitBySeconds", m_exitBy);
            j.Prop("triggerSeconds", m_tTrigger).Prop("triggerS", m_sTrigger);
            j.Prop("doneSeconds", m_tDone).Prop("restS", m_sRest);
            j.Prop("rideStartSeconds", m_tRide0).Prop("rideEndSeconds", m_tRide1).Prop("ridePhasesMask", m_phasesSeen).Prop("cabinSpeedPeak", m_maxCabinSpeed);
            j.Prop("frames", m_frames).Prop("groundSpeedPeak", m_maxGround).Prop("progressSpeedMin", m_minPath).Prop("progressSpeedMax", m_maxPath);
            j.Prop("maxRollDeg", CameraRig.MaxRollDeg).Prop("rollLimitDeg", FrameworkConst.RollTolDeg).Prop("cameraProbeViolations", CameraRig.Violations);
            var gb = GameBootstrap.Instance;
            if (gb != null) j.Prop("loadSeconds", gb.LoadSeconds);
            j.Key("beats").BeginArray();
            for (int i = 0; i < m_beatS.Length; i++)
            {
                j.BeginObject(true).Prop("beat", i + 1).Prop("name", FrameworkConst.BeatNames[i]).Prop("boundary", FrameworkConst.BeatBoundaries[i])
                 .Prop("enteredS", m_beatS[i]).Prop("enteredT", m_beatT[i]).Prop("events", m_beatHits[i]).EndObject();
            }
            j.EndArray();
            j.Prop("coverage", CoverageNote).Prop("coverageMisses", CoverageMisses);
            j.Prop("devices", Devices);
        }
    }
}
