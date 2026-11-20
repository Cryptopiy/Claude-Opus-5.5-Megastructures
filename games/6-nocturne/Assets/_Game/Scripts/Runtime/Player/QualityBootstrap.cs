// QualityBootstrap: frame pacing, window mode and the fullscreen toggle (MASTER_PROMPT 8.1, 8.10, Part 9, 11.7, 2.5.3),
// imported from Thread. VSync on by default (the player runs at the monitor refresh). QA runs (-autotest, -autotest-until,
// -soak, -wallbot, -sequence, captures, -render-audio) are capped at 60 fps with vSync off (Part 2.5.3, IMPORT_PLAN section 6 item 3:
// Thread rendered uncapped); only performance runs (-perf-log) are uncapped (vSyncCount 0, targetFrameRate -1).
// -res WxH opens a window of that size. F11 toggles FullScreenWindow (native size, never black bars) and Windowed; Alt+Enter
// is the native Windows switch (PlayerSettings allowFullscreenSwitch = true) and code only acts on Alt+Enter when the native
// switch did not change the mode within 0.5 s. This is the ONLY owner of the fullscreen keys (The Ring's PlayerController copy
// is not imported: a double toggle otherwise).
using System;
using UnityEngine;

namespace Megastructure
{
    [DefaultExecutionOrder(-900)]
    public class QualityBootstrap : MonoBehaviour
    {
        public const double AltEnterFallbackDelay = 0.5;
        /// <summary>Frame cap of every QA job (Part 2.5.3).</summary>
        public const int QaFrameCap = 60;

        int m_windowW, m_windowH;
        bool m_altEnterPending;
        double m_altEnterTime;
        FullScreenMode m_modeAtAltEnter;

        /// <summary>True when this process runs a QA job that must be frame capped.</summary>
        public static bool QaCapped =>
            CliArgs.AutoTest || CliArgs.SoakSeconds > 0 || CliArgs.WallBot || CliArgs.Sequence != null || CliArgs.Capturing || CliArgs.RenderAudio != null;

        void Awake()
        {
            bool perf = CliArgs.PerfLog != null;
            if (perf) { QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; }
            else if (QaCapped) { QualitySettings.vSyncCount = 0; Application.targetFrameRate = QaFrameCap; }
            else { QualitySettings.vSyncCount = 1; Application.targetFrameRate = -1; }
            Application.runInBackground = true;   // autoplay, captures and the audio dump must not stall when unfocused

            if (CliArgs.TryGetRes(out int w, out int h) && !Application.isEditor)
            {
                Screen.SetResolution(w, h, FullScreenMode.Windowed);
                m_windowW = w; m_windowH = h;
            }
            Debug.Log(FormattableString.Invariant($"[NOC] quality: vSync={QualitySettings.vSyncCount} targetFrameRate={Application.targetFrameRate} perf={perf} qaCap={QaCapped && !perf} ") +
                      FormattableString.Invariant($"screen={Screen.width}x{Screen.height} mode={Screen.fullScreenMode} gfx={SystemInfo.graphicsDeviceType}"));
        }

        void Start()
        {
            NocInput.DeviceSummary(out bool kb, out bool mouse, out bool gp);
            Debug.Log(FormattableString.Invariant($"[NOC] input devices: keyboard={kb} mouse={mouse} gamepad={gp}"));
        }

        void Update()
        {
            if (NocInput.F11Pressed()) { m_altEnterPending = false; Toggle(); return; }

            double now = Time.unscaledTimeAsDouble;
            if (NocInput.AltEnterPressed())
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
                Debug.Log(FormattableString.Invariant($"[NOC] fullscreen on: {fw}x{fh}"));
            }
            else
            {
                int w = m_windowW > 0 ? m_windowW : Mathf.RoundToInt(Math.Max(1, di.width) * 0.75f);
                int h = m_windowH > 0 ? m_windowH : Mathf.RoundToInt(Math.Max(1, di.height) * 0.75f);
                Screen.SetResolution(w, h, FullScreenMode.Windowed);
                Debug.Log(FormattableString.Invariant($"[NOC] fullscreen off: {w}x{h}"));
            }
        }
    }
}
