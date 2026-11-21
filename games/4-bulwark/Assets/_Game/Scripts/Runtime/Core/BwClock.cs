// BwClock: the ambient clock (waves, flicker, sway, mist drift) and the grade frame counter.
// Base: Thread's ThreadClock. Merged from Abyss SceneClock/ClockDriver: RealTime (never freezes; UI fades),
// Wrapped(period) in double, freeze while paused, the _BwTime global (mod 3600 in double), -clock T and
// -freeze-clock (alias of -freeze-ambient). Play: Time.timeAsDouble. Capture: stepped/set by the harness.
// Ambient motion never feeds a beat parameter (14.6); progress-keyed events use s (H6).
using UnityEngine;

namespace Megastructure
{
    public static class BwClock
    {
        /// <summary>-freeze-ambient / -freeze-clock or static tests: the ambient clock does not advance.</summary>
        public static bool Frozen;
        /// <summary>The capture harness drives the clock explicitly (Set/Advance).</summary>
        public static bool Stepped;
        /// <summary>Freeze the ambient clock while the game is paused (set by the pause owner through GameState.Paused).</summary>
        public static bool FreezeOnPause = true;
        /// <summary>Grade frame index (drives the R2 dither offset when the dither is temporal).</summary>
        public static long Frame;
        /// <summary>Static dither (default, 14.7 H5). False only for "-dither temporal".</summary>
        public static bool FreezeDither = true;

        static double s_time;
        static double s_real;
        static double s_lastUnity;
        static bool s_init;

        /// <summary>Ambient time (s).</summary>
        public static double Ambient => s_time;
        /// <summary>Unfreezable real time since start (s): UI fades, title timeline.</summary>
        public static double RealTime => s_real;

        /// <summary>Wrapped ambient clock for a loop of the given period, computed in double (no drift after hours).</summary>
        public static double Wrapped(double period) => s_time - period * System.Math.Floor(s_time / period);

        /// <summary>Applies -clock T, -freeze-ambient / -freeze-clock and the dither mode. Called by GameBootstrap.</summary>
        public static void ApplyCli()
        {
            double t0 = CliArgs.GetDouble("-clock", double.NaN);
            if (!double.IsNaN(t0)) s_time = t0;
            if (CliArgs.FreezeAmbient) Frozen = true;
            FreezeDither = CliArgs.DitherMode != DitherMode.Temporal;
        }

        /// <summary>Called once per frame by ProgressDirector.LateUpdate with Time.timeAsDouble.</summary>
        public static void Tick(double unityTime)
        {
            if (!s_init) { s_lastUnity = unityTime; s_init = true; }
            double dt = unityTime - s_lastUnity;
            s_lastUnity = unityTime;
            if (dt > 0 && dt < 1.0)
            {
                float cap = Time.captureDeltaTime;
                s_real += cap > 0f ? cap : Time.unscaledDeltaTime;
                bool paused = FreezeOnPause && GameState.Paused;
                if (!Frozen && !Stepped && !paused) s_time += dt;
            }
            if (!FreezeDither) Frame++;
            Publish();
        }

        /// <summary>Capture harness: set the ambient time explicitly (deterministic sequences, H6 capture rule).</summary>
        public static void Set(double t) { s_time = t; Publish(); }
        public static void Advance(double dt) { if (!Frozen) s_time += dt; Publish(); }
        public static void Reset() { s_time = 0; s_real = 0; s_init = false; Frame = 0; }

        /// <summary>
        /// H6 capture rule hook: for a set piece started by a progress trigger at s0, the event clock of a capture taken at s
        /// equals the progress branch (s - s0) / v. Set pieces call this so stills, sequences and play agree at any speed.
        /// </summary>
        public static double ProgressBranch(double s, double s0, double speed) => speed > 1e-6 ? (s - s0) / speed : 0.0;

        /// <summary>Writes _BwTime = (ambient mod 3600, real mod 3600, 0, 0) in double before the cast.</summary>
        public static void Publish()
        {
            double t = s_time, r = s_real;
            Shader.SetGlobalVector(ShaderIds.BwTime, new Vector4(
                (float)(t - 3600.0 * System.Math.Floor(t / 3600.0)),
                (float)(r - 3600.0 * System.Math.Floor(r / 3600.0)), 0f, 0f));
        }
    }
}
