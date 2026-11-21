// BeatDirector: the current beat (BwPath.BeatAt boundaries 0, 120, 420, 700, 920, 1070, 1370 with 30 m backward
// hysteresis, 14.6) and the goal trigger at the Keeper's Lantern, s >= EndingPath.SGoal 1896.0 after the ride (ADDENDUM A1 00). Imported from Thread. Logs each first entry in order (the
// autotest asserts beats 1..7 fire exactly once and in order, and the lantern once) and raises BwEvents.
// Skeleton: no beat content; set visibility belongs to the beat builders (they read Current / BeatChanged).
using System;
using UnityEngine;

namespace Megastructure
{
    [DefaultExecutionOrder(600)]
    public class BeatDirector : MonoBehaviour
    {
        public static BeatDirector Instance { get; private set; }
        /// <summary>Current beat 0..6 (-1 before the first frame).</summary>
        public int Current { get; private set; } = -1;
        public int EnteredCount { get; private set; }
        public bool OrderOk { get; private set; } = true;
        public bool GoalFired { get; private set; }
        public int GoalCount { get; private set; }
        /// <summary>First entry of a beat: (beat 0..6, s).</summary>
        public static event Action<int, double> BeatEntered;
        /// <summary>Every change of the current beat (0..6), also when stepping back over a boundary.</summary>
        public static event Action<int> BeatChanged;
        readonly bool[] m_entered = new bool[FrameworkConst.BeatCount];

        /// <summary>Backward hysteresis of the beat index (14.6: 30 m back).</summary>
        public const double HysteresisBack = 30.0;

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        void LateUpdate()
        {
            if (!GameBootstrap.Ready) return;
            Step(ProgressDirector.S);
        }

        /// <summary>Advance the beat state to progress s (public for EditMode tests).</summary>
        public void Step(double s)
        {
            int b = Current < 0 ? BwPath.BeatAt(s) - 1 : Current;
            var bounds = FrameworkConst.BeatBoundaries;
            while (b < FrameworkConst.BeatCount - 1 && s >= bounds[b + 1]) b++;
            while (b > 0 && s < bounds[b] - HysteresisBack) b--;
            if (b != Current)
            {
                Current = b;
                GameState.Beat = b + 1;
                if (!m_entered[b])
                {
                    m_entered[b] = true;
                    if (b != EnteredCount && !(EnteredCount == 0 && CliArgs.StartS > 0)) OrderOk = false;
                    EnteredCount++;
                    Debug.Log($"[BW] beat {b + 1} ({FrameworkConst.BeatNames[b]}) entered at s={s:F2} t={Time.timeSinceLevelLoadAsDouble:F2}");
                    try { BeatEntered?.Invoke(b, s); BwEvents.RaiseBeatEntered(b + 1); }
                    catch (Exception e) { Debug.LogError($"[BW-FAIL] BeatEntered handler threw: {e}"); }
                }
                try { BeatChanged?.Invoke(b); }
                catch (Exception e) { Debug.LogError($"[BW-FAIL] BeatChanged handler threw: {e}"); }
            }
            var p = BwServices.Player;
            // the goal is the Keeper's Lantern on the platform after the ride (ADDENDUM A1 00; EndingPath.SGoal)
            bool goal = s >= FrameworkConst.EndGoalS || (p != null && p.Finished);
            if (goal && !GoalFired)
            {
                GoalFired = true;
                GoalCount++;
                GameState.GoalReached = true;
                GameState.GoalRealTime = BwClock.RealTime;
                Debug.Log($"[BW] goal trigger (lantern) at s={s:F2} t={Time.timeSinceLevelLoadAsDouble:F2}");
                try { BwEvents.RaiseGoal(); }
                catch (Exception e) { Debug.LogError($"[BW-FAIL] GoalReached handler threw: {e}"); }
            }
        }
    }
}
