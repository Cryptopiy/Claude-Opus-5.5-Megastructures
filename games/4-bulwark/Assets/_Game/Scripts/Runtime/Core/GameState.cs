// GameState: plain per-frame flags shared by UI, audio and QA (slimmed from Abyss's GameState, D-09 / C-06).
// BwServices (interfaces) stay primary; these fields are the cheap read-only mirror. Each field has exactly one
// writer, named in its comment. Zero allocation; readable from any main-thread system.
using UnityEngine;

namespace Megastructure
{
    public static class GameState
    {
        /// <summary>Progress s (m of filleted plan distance). Owner: ProgressDirector.</summary>
        public static double S;

        /// <summary>Current beat 1..7 (with hysteresis). Owner: BeatDirector.</summary>
        public static int Beat = 1;

        /// <summary>Main camera pose this frame (after CameraRig.LateUpdate). Owner: CameraRig.</summary>
        public static Vector3 CameraPos;
        public static Quaternion CameraRot = Quaternion.identity;
        public static Camera MainCamera;

        /// <summary>Player feet position (world). Owner: PlayerController.</summary>
        public static Vector3 PlayerPos;

        /// <summary>Cinematic autowalk active. Owner: AutoWalker.</summary>
        public static bool Cinematic;
        /// <summary>Capture harness active (-capture). Owner: GameBootstrap.</summary>
        public static bool CaptureMode;
        /// <summary>-autotest / -wallbot / -soak run. Owner: GameBootstrap.</summary>
        public static bool AutoTest;
        /// <summary>Pause overlay open. Owner: UiDirector (the single pause owner, D-04).</summary>
        public static bool Paused;
        /// <summary>Ending finished (game on black). Owner: UiDirector.</summary>
        public static bool Ended;

        /// <summary>Goal trigger fired (s >= 1498.5). Owner: BeatDirector.</summary>
        public static bool GoalReached;
        /// <summary>BwClock.RealTime at which GoalReached became true (-1 before). Owner: BeatDirector.</summary>
        public static double GoalRealTime = -1;

        /// <summary>Picture/audio master fade 0..1 of the UI timeline (0 = black/silent). Owner: UiDirector.</summary>
        public static float PictureFade;

        /// <summary>Seed of every deterministic random choice (-seed N). Owner: GameBootstrap.</summary>
        public static uint Seed = 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            S = 0; Beat = 1; CameraPos = Vector3.zero; CameraRot = Quaternion.identity; MainCamera = null; PlayerPos = Vector3.zero;
            Cinematic = false; CaptureMode = false; AutoTest = false; Paused = false; Ended = false;
            GoalReached = false; GoalRealTime = -1; PictureFade = 0f; Seed = 1;
        }
    }
}
