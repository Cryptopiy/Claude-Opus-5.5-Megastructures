// NocRig: the trimmed bridge of The Ring's RingState / RingRig / RingEvents (IMPORT_PLAN 2.10) that lets the Ring player and
// UI compile almost verbatim next to Thread's services. Writers: GameBootstrap (WorldReady, LoadComplete), ProgressDirector
// (S, T, Dt, FrameIndex), BeatDirector (Beat, BeatEntered), PlayerController (Player, Controller, PlayerVelocity, GoalReached,
// Footstep, PlayerMoved, PauseChanged, CinematicChanged), CameraRig (Camera, Yaw, Pitch), UiDirector (EndingComplete).
using System;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Per-frame world state (single writer per field, see the header).</summary>
    public static class NocState
    {
        /// <summary>Progress s (m, filleted arc length).</summary>
        public static double S;
        /// <summary>Game time since the load finished (s); frozen / stepped in capture mode.</summary>
        public static double T;
        /// <summary>Delta of the current frame (0 when paused or frozen).</summary>
        public static float Dt;
        /// <summary>Capture mode: followers equal targets, slew limiters bypassed (stills).</summary>
        public static bool Capture;
        /// <summary>Frame sequences keep the slew limiters on even in capture mode.</summary>
        public static bool SequenceMode;
        public static bool Paused;
        public static bool Cinematic;
        public static bool AutoTest;
        public static bool GoalReached;
        /// <summary>Beat index 0..6.</summary>
        public static int Beat;
        public static int FrameIndex;

        public static void Reset()
        {
            S = 0; T = 0; Dt = 0f; Capture = false; SequenceMode = false; Paused = false; Cinematic = false; AutoTest = false;
            GoalReached = false; Beat = 0; FrameIndex = 0;
        }
    }

    /// <summary>Runtime registry of scene objects (no system references another system's class for these).</summary>
    public static class NocRig
    {
        public static Transform Player;              // feet transform of the CharacterController object
        public static CharacterController Controller;
        public static Camera Camera;                 // the gameplay camera
        public static bool WorldReady;               // load complete (world built, warmed up)
        /// <summary>Capture/test override: when not NaN the ProgressDirector uses this s instead of the player's.</summary>
        public static double ForcedS = double.NaN;
        /// <summary>Player ground velocity (m/s, xz).</summary>
        public static Vector3 PlayerVelocity;
        /// <summary>Camera yaw / pitch (deg; 0 = +Z, pitch up positive).</summary>
        public static float Yaw, Pitch;

        public static void Reset()
        {
            Player = null; Controller = null; Camera = null; WorldReady = false;
            ForcedS = double.NaN; PlayerVelocity = Vector3.zero; Yaw = 0f; Pitch = 0f;
        }
    }

    /// <summary>Game events (raised once, in order). Listeners: AudioDirector, UiDirector, AutoWalker, QA.</summary>
    public static class NocEvents
    {
        public static event Action<int> BeatEntered;           // beat index 0..6, fired once each, in order
        public static event Action GoalReached;                // s >= 1497 (the stop ramp starts)
        public static event Action EndingComplete;             // ending line faded out (17 s; the game stays on black)
        public static event Action<bool> PauseChanged;
        public static event Action<FootSurface, int, float> Footstep;   // surface, foot (0/1), speed m/s
        public static event Action LoadComplete;               // world built and warmed up; the fade-in may start
        public static event Action<bool> CinematicChanged;
        public static event Action PlayerMoved;                // first movement input (hides the controls hint)

        public static void RaiseBeatEntered(int b) => BeatEntered?.Invoke(b);
        public static void RaiseGoal() => GoalReached?.Invoke();
        public static void RaiseEndingComplete() => EndingComplete?.Invoke();
        public static void RaisePause(bool p) => PauseChanged?.Invoke(p);
        public static void RaiseFootstep(FootSurface s, int foot, float speed) => Footstep?.Invoke(s, foot, speed);
        public static void RaiseLoadComplete() => LoadComplete?.Invoke();
        public static void RaiseCinematic(bool on) => CinematicChanged?.Invoke(on);
        public static void RaisePlayerMoved() => PlayerMoved?.Invoke();

        public static void Reset()
        {
            BeatEntered = null; GoalReached = null; EndingComplete = null; PauseChanged = null;
            Footstep = null; LoadComplete = null; CinematicChanged = null; PlayerMoved = null;
        }

        /// <summary>Footstep surface for progress s (14.4 Surfaces; the glass strip s 706-726).</summary>
        public static FootSurface SurfaceAt(double s)
        {
            if (s < 120.0) return FootSurface.WetConcrete;
            if (s < 560.0) return FootSurface.WetStone;
            if (s < 880.0) return s >= 706.0 && s < 726.0 ? FootSurface.Glass : FootSurface.Steel;
            if (s < 1080.0) return FootSurface.Garden;
            if (s < 1260.0) return FootSurface.WetStone;
            return FootSurface.PlazaStone;
        }
    }
}
