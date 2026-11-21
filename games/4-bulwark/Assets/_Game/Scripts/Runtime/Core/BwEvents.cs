// BwEvents: game events raised once, in order (built from The Ring's RingEvents, D-09 / C-06).
// Listeners: AudioDirector, UiDirector, QA runners. Raise only from the owner named per event.
using System;

namespace Megastructure
{
    public static class BwEvents
    {
        /// <summary>Beat entered, index 1..7, once each, in order. Raised by BeatDirector.</summary>
        public static event Action<int> BeatEntered;
        /// <summary>Goal trigger s >= 1498.5 (once). Raised by BeatDirector.</summary>
        public static event Action GoalReached;
        /// <summary>Ending line faded out; the game stays on black. Raised by UiDirector.</summary>
        public static event Action EndingComplete;
        /// <summary>Pause overlay opened/closed. Raised by UiDirector.</summary>
        public static event Action<bool> PauseChanged;
        /// <summary>World built and pre-warmed; the fade-in may start. Raised by GameBootstrap.</summary>
        public static event Action LoadComplete;
        /// <summary>Cinematic autowalk started/stopped. Raised by AutoWalker.</summary>
        public static event Action<bool> CinematicChanged;
        /// <summary>First movement input (hides the controls hint). Raised by PlayerController.</summary>
        public static event Action PlayerMoved;

        public static void RaiseBeatEntered(int beat) => BeatEntered?.Invoke(beat);
        public static void RaiseGoal() => GoalReached?.Invoke();
        public static void RaiseEndingComplete() => EndingComplete?.Invoke();
        public static void RaisePause(bool paused) => PauseChanged?.Invoke(paused);
        public static void RaiseLoadComplete() => LoadComplete?.Invoke();
        public static void RaiseCinematic(bool on) => CinematicChanged?.Invoke(on);
        public static void RaisePlayerMoved() => PlayerMoved?.Invoke();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            BeatEntered = null; GoalReached = null; EndingComplete = null; PauseChanged = null;
            LoadComplete = null; CinematicChanged = null; PlayerMoved = null;
        }
    }
}
