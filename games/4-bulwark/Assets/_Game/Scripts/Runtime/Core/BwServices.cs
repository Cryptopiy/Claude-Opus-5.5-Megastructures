// BwServices: the small interfaces through which separately-owned systems talk to each other
// (player, camera, audio, UI). Base: Thread's ThreadServices. Each system registers itself in Awake;
// consumers must tolerate a null service (EditMode tests, capture mode without UI).
using UnityEngine;

namespace Megastructure
{
    /// <summary>Footstep surfaces of 14.3 (recipes in FootstepBank).</summary>
    public enum FootSurface { Concrete = 0, WetConcrete = 1, Road = 2, Grating = 3, SteelStair = 4, Ledge = 5 }

    public interface IPlayerState
    {
        /// <summary>Progress = filleted plan distance along the path (m), BwPath s.</summary>
        double S { get; }
        /// <summary>Lateral offset to the right of the heading (m), lane coordinates.</summary>
        double Lateral { get; }
        /// <summary>Current progress speed ds/dt (m/s).</summary>
        float Speed { get; }
        /// <summary>Current horizontal ground speed (m/s), including the lateral component.</summary>
        float GroundSpeed { get; }
        bool Brisk { get; }
        /// <summary>True once the goal trigger (s >= 1498.5) fired; input is replaced by the stop ramp.</summary>
        bool Finished { get; }
        /// <summary>World position of the eye (camera) this frame.</summary>
        Vector3 EyePosition { get; }
        /// <summary>Teleport to a lane coordinate (tests, captures, -progress).</summary>
        void Teleport(double s, double lateral);
        /// <summary>External drive (AutoWalker): progress speed (m/s) and lateral target; active false = player input.</summary>
        void SetDrive(bool active, float speed, float lateralTarget);
        /// <summary>Freeze all movement (pause, ending dwell, captures).</summary>
        bool Frozen { get; set; }
    }

    public interface ICameraControl
    {
        Camera Camera { get; }
        /// <summary>World yaw (deg, 0 = +Z, 90 = +X) and pitch (deg, up positive) of the view.</summary>
        float Yaw { get; }
        float Pitch { get; }
        /// <summary>Look drive for the AutoWalker / ending dwell (world yaw and pitch, deg).</summary>
        void SetLook(float worldYawDeg, float pitchDeg);
        /// <summary>Full pose override for captures; fovDeg &lt;= 0 keeps the schedule vFOV(s).</summary>
        void SetPoseOverride(Vector3 position, float worldYawDeg, float pitchDeg, float fovDeg);
        void ClearPoseOverride();
        bool PoseOverridden { get; }
        /// <summary>True while mouse/gamepad look input arrived this frame (cancels the autowalk).</summary>
        bool LookInputThisFrame { get; }
    }

    public interface IAudioHooks
    {
        /// <summary>One footstep (mixed inside BwDsp, through the reverb and the limiter).</summary>
        void Footstep(FootSurface surface, float pan, float gain);
        /// <summary>Pause duck (-12 dB over 0.4 s, 14.11).</summary>
        void SetPauseDuck(bool paused);
        /// <summary>Called once at the goal trigger: the audio follows the 14.11 ending (dwell 3.0 s, fade 4.0 s smootherstep).</summary>
        void BeginEnding();
        /// <summary>Starts the -audio-dump capture (seconds) and writes a WAV when done.</summary>
        void StartDump(string path, float seconds);
        /// <summary>One wave impact (14.3 boom: strength 0..1, pan -1..1; subOnly = beat 6 31 Hz sub only). Driven by SeaState
        /// impacts (WaveImpacts) and the 14.11 intro; implemented by AudioDirector.Impact.</summary>
        void Impact(float strength, float pan, bool subOnly);
    }

    public interface IFadeUI
    {
        /// <summary>0 = picture visible, 1 = black.</summary>
        float Black { get; }
        void BeginIntro(bool showHint);
        void BeginEnding();
        void HideHint();
        /// <summary>The single pause owner (D-04).</summary>
        bool Paused { get; }
        void SetBlackImmediate(float black);
        /// <summary>Hide every UI element without disabling the UIDocument (D-04 / C-29: captures).</summary>
        void SetCaptureHidden(bool hidden);
    }

    public static class BwServices
    {
        public static IPlayerState Player;
        public static ICameraControl CameraControl;
        public static IAudioHooks Audio;
        public static IFadeUI Fade;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear() { Player = null; CameraControl = null; Audio = null; Fade = null; }
    }
}
