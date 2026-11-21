// AutoWalker: the cinematic autowalk and the deterministic test walk (MASTER_PROMPT Part 9, 3.1 a-f, 14.5, 14.10.4; D-02).
// Thread's AutoWalker shell (C toggle, -cinematic pending start after load, cancel on any input after a 0.3 s grace (also
// after unpause), test mode StartTest, ElapsedVirtual, PlayerController.SetDrive) around The Ring's trimmed WalkPlanner core.
// Thread's CinematicPlan (slow-downs, holds, budgets, per-cue speed) is NOT imported.
// Cinematic: constant 2.5 m/s (= the Shift speed) from the current s, 0.5 s start ramp from the current progress speed, the
// GazeSchedule look (lead, omega 2.5, 18 / 13 deg/s, sway <= 0.3 deg) and BwPath.CinematicLateral; brisk from frame 1.
// -speed and -brisk apply to test walks only; QA can never force 1.5 m/s on the cinematic (The Ring's QaHooks bug).
// Test walk (StartTest, QA): from s 0 at exactly the requested speed (1.5 for -autotest, 2.5 for -autotest -cinematic),
// SAME seed, gaze and lateral keys as the cinematic, not cancellable; ElapsedVirtual = virtual seconds to the rest at 1500.
// Update (-40) runs after CameraRig.Update (-50) and before PlayerController.Update (-30). Zero allocations per frame.
using System;
using UnityEngine;

namespace Megastructure.Player
{
    [DefaultExecutionOrder(-40)]
    public class AutoWalker : MonoBehaviour
    {
        public static AutoWalker Instance { get; private set; }

        PlayerController m_player;
        WalkPlanner m_planner;
        bool m_active, m_test, m_completed;
        double m_elapsed;           // virtual seconds since start (the player's integration step)
        double m_graceUntil;
        bool m_wasPaused;
        bool m_cinematicPending;
        bool m_captureMode;
        bool m_wasRiding;

        // ---------------------------------------------------------------- public state
        /// <summary>The walker is running (cinematic or test). Test mode turns it off once the player rests at s 1500.</summary>
        public bool Active => m_active;
        public bool TestMode => m_test;
        public bool Cinematic => m_active && !m_test;
        public bool Completed => m_completed;
        /// <summary>Virtual seconds walked since StartTest / StartCinematic (frozen at completion in test mode).</summary>
        public double ElapsedVirtual => m_elapsed;
        public double Cruise => m_planner != null ? m_planner.Cruise : 0.0;
        public WalkPlanner Planner => m_planner;

        void Awake()
        {
            Instance = this;
            m_player = GetComponent<PlayerController>();
            m_planner = new WalkPlanner(CliArgs.Seed);
            m_planner.ForwardGaze = string.Equals(CliArgs.Get("-gaze"), "forward", StringComparison.OrdinalIgnoreCase);
            m_captureMode = CliArgs.CaptureDir != null;
            m_cinematicPending = (CliArgs.Cinematic || GameBootstrap.CinematicOnLoad) && !CliArgs.AutoTest && !m_captureMode;
            GameBootstrap.CinematicOnLoad = false;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        PlayerController Player
        {
            get
            {
                if (m_player == null) m_player = PlayerController.Instance;
                return m_player;
            }
        }

        // ---------------------------------------------------------------- API
        /// <summary>Starts the cinematic autowalk from the current progress at the constant cinematic speed.</summary>
        public void StartCinematic()
        {
            var p = Player;
            if (p == null || p.Finished) return;
            m_active = true; m_test = false; m_completed = false;
            m_elapsed = 0;
            var cam = BwServices.CameraControl;
            double yaw = cam != null ? cam.Yaw : BwPath.HeadingDeg(p.S);
            double pitch = cam != null ? cam.Pitch : 0.0;
            m_planner.Begin(p.S, Math.Max(0.0, p.PathSpeed), p.Lateral, yaw, pitch, FrameworkConst.CinematicSpeed);
            m_graceUntil = Time.unscaledTimeAsDouble + PlayerTuning.CancelGrace;
            p.DriveNoSmoothing = true;
            p.DriveBrisk = true;
            p.SetDrive(true, (float)m_planner.SpeedCommand, (float)p.Lateral);
            GameState.Cinematic = true;
            BwEvents.RaiseCinematic(true);
            BwServices.Fade?.HideHint();
            Debug.Log($"[BW] cinematic autowalk on at s={p.S:F2} speed={FrameworkConst.CinematicSpeed:F2} m/s (v0 {m_planner.V0:F2})");
        }

        /// <summary>QA test walk: from s 0 at exactly <paramref name="speed"/> m/s, cinematic gaze and lateral, deterministic.</summary>
        public void StartTest(double speed)
        {
            var p = Player;
            if (p == null) { Debug.LogError("[BW-FAIL] AutoWalker.StartTest: no PlayerController"); return; }
            m_active = true; m_test = true; m_completed = false;
            m_elapsed = 0;
            p.Teleport(0.0, BwPath.CinematicLateral(0.0));
            m_planner.Begin(0.0, 0.0, p.Lateral, BwPath.HeadingDeg(0.0), 0.0, speed);
            p.DriveNoSmoothing = true;
            p.DriveBrisk = speed > FrameworkConst.WalkSpeed + 1e-6;
            p.SetDrive(true, (float)m_planner.SpeedCommand, (float)p.Lateral);
            BwServices.CameraControl?.SetLook((float)m_planner.OutYaw, (float)m_planner.OutPitch);
            BwServices.Fade?.HideHint();
            Debug.Log($"[BW] autowalk test start speed={speed:F2} m/s");
        }

        /// <summary>Stops the walker and hands control back to the player.</summary>
        public void Stop()
        {
            if (!m_active) return;
            bool wasCinematic = !m_test;
            m_active = false;
            var p = Player;
            if (p != null) p.SetDrive(false, 0f, 0f);
            Debug.Log($"[BW] autowalk off at s={(p != null ? p.S : 0):F2} after {m_elapsed:F2} s");
            m_test = false;
            if (wasCinematic) { GameState.Cinematic = false; BwEvents.RaiseCinematic(false); }
        }

        public void ToggleCinematic()
        {
            if (m_active) Stop(); else StartCinematic();
        }

        // ---------------------------------------------------------------- frame
        void Update()
        {
            var p = Player;
            if (p == null) return;
            bool paused = BwServices.Fade != null && BwServices.Fade.Paused;
            double now = Time.unscaledTimeAsDouble;
            if (paused) { m_wasPaused = true; return; }
            if (m_wasPaused) { m_wasPaused = false; m_graceUntil = now + PlayerTuning.CancelGrace; }

            bool ready = GameBootstrap.Instance == null || GameBootstrap.Ready;
            if (m_cinematicPending && ready) { m_cinematicPending = false; StartCinematic(); }
            if (!m_test && !m_captureMode && !p.Finished && BwInput.CinematicToggle()) { ToggleCinematic(); return; }
            if (!m_active) return;

            // any player input cancels the cinematic walk (never the test walk)
            if (!m_test && now >= m_graceUntil && BwInput.AnyCancelInput(PlayerTuning.LookThresholdCounts, PlayerTuning.StickThreshold))
            {
                Stop();
                return;
            }

            double dt = Math.Min(Time.deltaTime, PlayerTuning.MaxStepDt);   // the player integrates the same step
            if (dt <= 0 || !ready) return;

            if (m_test && p.Resting && !m_completed)
            {
                m_completed = true;
                Debug.Log($"[BW] autowalk test complete: s={p.S:F3} in {m_elapsed:F2} s virtual time");
                m_active = false;
                p.SetDrive(false, 0f, 0f);
                return;
            }

            m_elapsed += dt;   // this frame's step, integrated by PlayerController.Update right after
            // the ride (EndingPath) drives s by its own clock; the planner keeps looking along the ride keys. When the platform
            // walk begins the walker starts again from rest with the 0.5 s ramp (the look state continues).
            bool riding = p.Riding;
            if (m_wasRiding && !riding && !p.Finished) m_planner.RestartRamp(p.S, 0.0);
            m_wasRiding = riding;
            m_planner.Step(dt, p.S, BwClock.Ambient);
            if (!p.Finished) p.SetDrive(true, (float)m_planner.SpeedCommand, (float)m_planner.LateralTarget);
            if (!GameState.GoalReached) BwServices.CameraControl?.SetLook((float)m_planner.OutYaw, (float)m_planner.OutPitch);
        }
    }
}
