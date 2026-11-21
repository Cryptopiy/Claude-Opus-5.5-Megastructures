// QualityBootstrap: frame pacing, window mode and the fullscreen toggle (MASTER_PROMPT 8.1, 8.10, Part 9, 11.7, 2.5.3).
// Imported from Thread. VSync on by default (the player runs at the monitor refresh). Every QA run (bots, soak, autotest,
// captures, -perf-log) is capped at 60 fps with vSyncCount 0 (Part 2.5.3: cap when not measuring; with VSync on Unity would
// ignore targetFrameRate and run at the monitor refresh); QaDrive and CaptureHarness re-apply the cap. Only a performance run
// that EXPLICITLY asks for it runs uncapped (vSyncCount 0, targetFrameRate -1, 11.7): -perf-log <file> together with
// -uncapped, never the soak (FramePolicy, UncappedPerf). -res WxH opens a window of that size. F11 toggles FullScreenWindow (at the display's native
// size, so there are never black bars) and Windowed. Alt+Enter is the native Windows switch (PlayerSettings
// allowFullscreenSwitch = true, R8 4); handling it in code as well would toggle twice, so code only acts on Alt+Enter
// when the native switch did not change the mode within 0.5 s (a fallback if the player setting is ever off).
using System;
using UnityEngine;

namespace Megastructure.Player
{
    [DefaultExecutionOrder(-900)]
    public class QualityBootstrap : MonoBehaviour
    {
        public const double AltEnterFallbackDelay = 0.5;
        /// <summary>The explicit request of a headroom measurement (11.7); only honoured together with -perf-log.</summary>
        public const string UncappedFlag = "-uncapped";

        int m_windowW, m_windowH;
        bool m_altEnterPending;
        double m_altEnterTime;
        FullScreenMode m_modeAtAltEnter;

        /// <summary>True only for a performance run that explicitly asks to be uncapped: -perf-log and -uncapped, not a soak.</summary>
        public static bool UncappedPerf => UncappedRequested(CliArgs.PerfLog != null, CliArgs.Has(UncappedFlag), CliArgs.SoakSeconds > 0);

        public static bool UncappedRequested(bool perfLog, bool uncappedFlag, bool soak) => perfLog && uncappedFlag && !soak;

        /// <summary>
        /// Frame pacing (pure, EditMode-tested): an explicit uncapped perf run measures headroom (vSync 0, target -1); every
        /// other QA run, -perf-log without -uncapped included, is capped at 60 (vSync 0 so the target applies); normal play
        /// keeps VSync on (the monitor refresh caps it).
        /// </summary>
        public static void FramePolicy(bool qa, bool perfLog, bool uncappedFlag, bool soak, out int vSyncCount, out int targetFrameRate)
        {
            if (UncappedRequested(perfLog, uncappedFlag, soak)) { vSyncCount = 0; targetFrameRate = -1; return; }
            if (qa || perfLog) { vSyncCount = 0; targetFrameRate = FrameworkConst.QaFrameCap; return; }
            vSyncCount = 1; targetFrameRate = -1;
        }

        void Awake()
        {
            bool perf = CliArgs.PerfLog != null;
            bool uncappedFlag = CliArgs.Has(UncappedFlag);
            bool soak = CliArgs.SoakSeconds > 0;
            bool qa = CliArgs.AutoTest || CliArgs.WallBot || soak || CliArgs.CaptureDir != null;
            FramePolicy(qa, perf, uncappedFlag, soak, out int vs, out int target);
            QualitySettings.vSyncCount = vs;
            Application.targetFrameRate = target;
            Application.runInBackground = true;   // autoplay, captures and the audio dump must not stall when unfocused
            if (uncappedFlag && !UncappedPerf) Debug.LogWarning("[BW] quality: " + UncappedFlag + " ignored (only a -perf-log run outside the soak runs uncapped; Part 2.5.3)");

            if (CliArgs.TryGetRes(out int w, out int h) && !Application.isEditor)
            {
                Screen.SetResolution(w, h, FullScreenMode.Windowed);
                m_windowW = w; m_windowH = h;
            }
            Debug.Log($"[BW] quality: vSync={QualitySettings.vSyncCount} targetFrameRate={Application.targetFrameRate} perf={perf} uncapped={UncappedPerf} qa={qa} " +
                      $"screen={Screen.width}x{Screen.height} mode={Screen.fullScreenMode} gfx={SystemInfo.graphicsDeviceType}");
        }

        void Start()
        {
            BwInput.DeviceSummary(out bool kb, out bool mouse, out bool gp);
            Debug.Log($"[BW] input devices: keyboard={kb} mouse={mouse} gamepad={gp}");
        }

        void Update()
        {
            if (BwInput.F11Pressed()) { m_altEnterPending = false; Toggle(); return; }

            double now = Time.unscaledTimeAsDouble;
            if (BwInput.AltEnterPressed())
            {
                m_altEnterPending = true;
                m_altEnterTime = now;
                m_modeAtAltEnter = Screen.fullScreenMode;
            }
            else if (m_altEnterPending && now - m_altEnterTime > AltEnterFallbackDelay)
            {
                m_altEnterPending = false;
                if (Screen.fullScreenMode == m_modeAtAltEnter) Toggle();   // the native switch did not act
            }
        }

        /// <summary>FullScreenWindow at the native display size, or back to the last windowed size.</summary>
        public void Toggle()
        {
            if (Application.isEditor) return;   // SetResolution only affects the Game view in the Editor
            var di = Screen.mainWindowDisplayInfo;
            if (Screen.fullScreenMode == FullScreenMode.Windowed)
            {
                m_windowW = Screen.width; m_windowH = Screen.height;
                int fw = di.width > 0 ? di.width : Screen.currentResolution.width;
                int fh = di.height > 0 ? di.height : Screen.currentResolution.height;
                Screen.SetResolution(fw, fh, FullScreenMode.FullScreenWindow);
                Debug.Log($"[BW] fullscreen on: {fw}x{fh}");
            }
            else
            {
                int w = m_windowW > 0 ? m_windowW : Mathf.RoundToInt(Math.Max(1, di.width) * 0.75f);
                int h = m_windowH > 0 ? m_windowH : Mathf.RoundToInt(Math.Max(1, di.height) * 0.75f);
                Screen.SetResolution(w, h, FullScreenMode.Windowed);
                Debug.Log($"[BW] fullscreen off: {w}x{h}");
            }
        }
    }
}
