// BeatDirector: current beat, first entry of each beat logged once and in order at the exact boundary (the autotest asserts
// beats 1..7 fire once, in order, at s = 0, 120, 400, 560, 880, 1080, 1260; 14.10), imported from Thread. Logs the goal
// trigger once (s 1497).
// 14.6 "the city is one persistent set" and "nothing activates inside the frustum": the beat content roots
// (WorldContext.BeatRoot) are NEVER switched off or on; they stay active for the whole walk (Part 10 item 36: content seen
// from behind never disappears). The 8 m lead / 12 m hysteresis window of 14.6 applies to EVENTS only (InEventWindow: a
// builder's updater may start a pass, a sound or a swap of flags and uniforms while s is inside its beat's window); a
// visible swap still needs fog >= 98 %, darkness or a sealed mesh (14.6 rule). The window is a pure function of s.
using System;
using UnityEngine;

namespace Megastructure
{
    [DefaultExecutionOrder(600)]
    public class BeatDirector : MonoBehaviour
    {
        public static BeatDirector Instance { get; private set; }
        public int Current { get; private set; } = -1;
        public int EnteredCount { get; private set; }
        /// <summary>The first beat entered (-1 before): beat 1 for a walk from s 0, beat N for a -beat N / -progress start.</summary>
        public int FirstEntered { get; private set; } = -1;
        public bool OrderOk { get; private set; } = true;
        public bool GoalFired { get; private set; }
        public int GoalCount { get; private set; }
        /// <summary>s at which each beat was first entered (NaN = not yet).</summary>
        public readonly double[] EnteredAtS = new double[NocturneConstants.BeatCount];
        public readonly double[] EnteredAtT = new double[NocturneConstants.BeatCount];
        public static event Action<int, double> BeatEntered;
        readonly bool[] m_entered = new bool[NocturneConstants.BeatCount];

        void Awake()
        {
            Instance = this;
            for (int i = 0; i < EnteredAtS.Length; i++) { EnteredAtS[i] = double.NaN; EnteredAtT[i] = double.NaN; }
        }
        void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>The beat roots stay active for the whole walk (14.6 persistent set): nothing is switched here.</summary>
        public void Init(WorldContext ctx)
        {
            int roots = 0;
            for (int b = 0; b < NocturneConstants.BeatCount; b++)
            {
                var root = ctx != null ? ctx.BeatRoots[b] : null;
                if (root == null) continue;
                roots++;
                if (!root.gameObject.activeSelf)
                {
                    root.gameObject.SetActive(true);
                    Debug.LogWarning("[NOC] beat root " + root.name + " was inactive at load; re-activated (14.6: beat roots are always active)");
                }
            }
            Debug.Log("[NOC] beat director: " + roots.ToString(System.Globalization.CultureInfo.InvariantCulture) + " beat roots, always active (14.6)");
        }

        /// <summary>True while s lies inside beat b's EVENT window (8 m before its start to 12 m after its end, 14.6). Events
        /// only: content roots are never switched by it.</summary>
        public static bool InEventWindow(int b, double s)
        {
            var bounds = NocturneConstants.BeatBoundaries;
            return s >= bounds[b] - NocturneConstants.ActivateLead && s <= bounds[b + 1] + NocturneConstants.Hysteresis;
        }

        /// <summary>Order rule (GE's request, carried by FX): the n-th beat entered (n = enteredCount, from 0) must be
        /// startBeat + n, where startBeat is the beat of the start s (0 for s 0; beat N - 1 for -beat N, whose start sits exactly
        /// on the boundary). A walk that starts mid-path is in order when it enters its own beat first, then each next one.</summary>
        public static bool OrderStepOk(int b, int enteredCount, int startBeat) => b == startBeat + enteredCount;

        void LateUpdate()
        {
            if (!GameBootstrap.Ready) return;
            double s = ProgressDirector.S;
            int b = NocMath.BeatIndex(s);   // exact boundary: beat b starts at BEAT_S[b]
            if (b != Current)
            {
                Current = b;
                if (!m_entered[b])
                {
                    m_entered[b] = true;
                    if (EnteredCount == 0) FirstEntered = b;
                    if (!OrderStepOk(b, EnteredCount, NocMath.BeatIndex(CliArgs.StartS))) OrderOk = false;
                    EnteredCount++;
                    EnteredAtS[b] = s; EnteredAtT[b] = NocState.T;
                    Debug.Log(FormattableString.Invariant($"[NOC] beat {b + 1} ({NocturneConstants.BeatNames[b]}) entered at s={s:F2} t={NocState.T:F2}"));
                    BeatEntered?.Invoke(b, s);
                    NocEvents.RaiseBeatEntered(b);
                }
            }
            var p = NocServices.Player;
            if (p != null && p.Finished && !GoalFired)
            {
                GoalFired = true;
                GoalCount++;
                Debug.Log(FormattableString.Invariant($"[NOC] goal fired at s={s:F2} t={NocState.T:F2}"));
            }
        }
    }
}
