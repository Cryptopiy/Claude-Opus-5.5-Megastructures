// FrameworkConst: the constants the imported framework (systems, player, UI, QA, audio, grade) needs, under the names the
// framework uses. Every value that 14.12 defines is an ALIAS of BulwarkConst (the game table, one source of truth); the
// rest are framework rules cited per line (Part 2.5, 9, 14.10.4, 14.11). Nothing is copied from Thread, Abyss or The Ring.
// The path itself (length, lanes, floor, eye, beats) is BwPath.
using System;

namespace Megastructure
{
    public static class FrameworkConst
    {
        // ---------------------------------------------------------------- path and beats (14.12)
        public const int BeatCount = BulwarkConst.BeatCount;
        /// <summary>Beat starts and the path end (m of filleted plan distance). Mirrors BwPath.BeatAt.</summary>
        public static readonly double[] BeatBoundaries = BulwarkConst.BeatS;
        /// <summary>Short beat names (logs, QA tags "bNN_Name").</summary>
        public static readonly string[] BeatNames = { "WaveGallery", "Crest", "SpillwayBridge", "ArmourSlope", "GateTower", "WindowGallery", "SluiceLedge" };
        /// <summary>Hysteresis of the beat index (m), so a player stepping back over a boundary does not re-enter a beat.</summary>
        public const double BeatHysteresis = BulwarkConst.HysteresisBack;
        /// <summary>BOARD trigger (ADDENDUM A1 00): at s >= 1498.5 the walker ramps to rest at s 1500 in the elevator cabin and the
        /// ride starts (EndingPath). The walked path ends here: QA tools that stay on the walk (captures, soak) keep below it.</summary>
        public const double GoalTriggerS = BulwarkConst.GoalTriggerS;
        public const double GoalRestS = BulwarkConst.GoalRestS;
        /// <summary>Lantern (14.11), target of the ending dwell look.</summary>
        public const double LanternX = BulwarkConst.LanternX, LanternY = BulwarkConst.LanternY, LanternZ = BulwarkConst.LanternZ;
        /// <summary>The real goal: the Keeper's Lantern on the platform (EndingPath.SGoal 1896.0, rest at SRest 1897.5).</summary>
        public static double EndGoalS => EndingPath.SGoal;
        public static double EndRestS => EndingPath.SRest;
        /// <summary>Ride time (fixed at every walking speed): board 4 + descent 75 + transfer 6 + shuttle 55 + doors 3 = 143 s.</summary>
        public const double RideTotalTime = BulwarkConst.RideBoardTime + BulwarkConst.RideDescentTime + BulwarkConst.RideTransferTime
                                           + BulwarkConst.RideShuttleTime + BulwarkConst.RideDoorsTime;

        // ---------------------------------------------------------------- player (14.12, Part 9)
        public const double EyeHeight = BulwarkConst.Eye;
        public const double WalkSpeed = BulwarkConst.Walk;
        public const double BriskSpeed = BulwarkConst.Brisk;
        /// <summary>Cinematic speed = the Shift speed (Part 9, 3.1 a): one constant. BulwarkConst.Cine is the read-only alias of
        /// BulwarkConst.Brisk (not a literal of its own), so this const takes the source directly.</summary>
        public const double CinematicSpeed = BulwarkConst.Brisk;
        public const double AccelSmoothTime = BulwarkConst.Accel;
        /// <summary>Cinematic and test-walk start ramp (smootherstep, 3.1 a).</summary>
        public const double StartRamp = BulwarkConst.StartRamp;
        public const double PlayerRadius = BulwarkConst.PlayerR;
        public const double CcSlopeLimit = BulwarkConst.CcSlopeLimit;
        public const double CcStepOffset = BulwarkConst.CcStepOffset;
        public const double CameraProbeRadius = BulwarkConst.CameraProbe;
        /// <summary>Stride by speed (14.3): 0.75 m at 1.5 m/s, 0.85 m at 2.5 m/s, 0.90 m on stairs.</summary>
        public const double StrideWalk = BulwarkConst.StrideWalk, StrideBrisk = BulwarkConst.StrideBrisk, StrideStairs = BulwarkConst.StrideStairs;

        // ---------------------------------------------------------------- cinematic gaze (Part 9, 3.1 b, D-02)
        public const double GazeOmega = 2.5;           // critically damped follower (rad/s), lag 0.8 s
        public const double GazeMaxYawRate = 18.0;     // deg/s after damping
        public const double GazeMaxPitchRate = 13.0;   // deg/s after damping
        public const double GazeSwayMaxDeg = 0.3;      // combined sway amplitude, no roll
        /// <summary>A player offset at cinematic start decays over distance (e-folding, m), identical at any speed.</summary>
        public const double LateralDecayMetres = 4.0;
        public const double LateralMargin = 0.15;

        // ---------------------------------------------------------------- camera (14.12)
        public const double CameraNear = BulwarkConst.CameraNear;
        public const double FarOutdoor = BulwarkConst.FarOutdoor;
        public const double FarBeat1 = BulwarkConst.FarBeat1, FarBeat5 = BulwarkConst.FarBeat5, FarBeat6 = BulwarkConst.FarBeat6;
        /// <summary>Far-plane switch points (s), each where the exterior cannot be seen (14.6).</summary>
        public const double FarSwitch1 = BulwarkConst.FarSwitch1, FarSwitch2 = BulwarkConst.FarSwitch2, FarSwitch3 = BulwarkConst.FarSwitch3, FarSwitch4 = BulwarkConst.FarSwitch4;
        public const int Msaa = BulwarkConst.Msaa;

        // ---------------------------------------------------------------- grade and dither (14.2.5, 14.12, D-05, D-10)
        public const int DitherTile = 64;
        public const double DitherSigma = 1.5;
        /// <summary>Static TPDF blue-noise dither peak (8-bit codes): 2.0 in beats 1, 6, 7; 1.5 in beats 2-5; 30 m crossfades.</summary>
        public const double DitherPeakDark = 2.0;
        public const double DitherPeakMid = 1.5;
        public const double ParamCrossfade = BulwarkConst.TransitionWindow;     // Rule P: parameters crossfade over 30 m at boundaries
        public const double BloomClamp = BulwarkConst.ClampBloom;
        public const double ContrastPivot = BulwarkConst.ContrastPivot;
        public const double FloorLinear = BulwarkConst.DarkFloorLinear;   // darkest allowed pixel (sRGB 6)
        public const double EvSlew = BulwarkConst.EvSlew;               // EV/s
        public const double WbSlew = BulwarkConst.WbSlew;               // WB units/s
        public const double BloomSlew = BulwarkConst.BloomSlew;         // per second
        public const double FovSlew = 1.0;             // deg/s
        public const double TeleportJump = 20.0;       // m: larger jumps bypass the slews

        // ---------------------------------------------------------------- UI timeline (14.11)
        public const double IntroBlack = 1.0;
        /// <summary>14.11 / 14.10.4 T9: the first wave impact is heard at 0.5 s of the intro, in the black (the impact bus is open
        /// from t 0; only the bed follows the picture fade).</summary>
        public const double IntroImpactTime = 0.5;
        public const double PictureIn0 = 1.0, PictureIn1 = 3.5;
        public const double TitleIn0 = 1.4, TitleIn1 = 2.6, TitleHold = 4.6, TitleOut = 6.0;
        public const double HintStart = 6.5, HintDuration = 6.0, HintFade = 0.6, HintKill = 0.35;
        // ADDENDUM A1 3.10: the end camera (ease 1.5 s, tilt 7 s), the look and the sun sinking; the look holds to t_g 40 (the
        // show's stinger rises behind the crest at t_g 5-29 and holds to 44: the slow tilt-up at 18-28), then the 4 s fade,
        // 1.5 s black and the line 2 + 6 + 2 s
        public const double EndDwell = 40.0, EndFade = 4.0, EndBlack = 1.5, EndLineIn = 2.0, EndLineHold = 6.0, EndLineOut = 2.0;
        public const double EndTotal = EndDwell + EndFade + EndBlack + EndLineIn + EndLineHold + EndLineOut;   // 55.5 s
        public const double PauseDuckDb = -12.0, PauseDuckTime = 0.4;
        public const int TitlePx = 56;
        public const double TitleLetterSpacingEm = 0.22;
        public const double TitleAlpha = 0.88;
        public const double TitleCentreFromTop = 0.38;
        public const int EndLinePx = 51;
        public const double HintAlpha = 0.7;
        public const double HintBottom = 0.06;
        public const int RefWidth = 1920, RefHeight = 1080;
        public const string TitleText = "BULWARK";
        public const string HintText = "WASD  move    Mouse  look    Shift  faster    C  cinematic";
        public const string EndText = "Someone held the sea back, so the land could stay dry.";
        /// <summary>Palette token of all UI text (14.2.1).</summary>
        public const string TitleTextToken = "TitleText";

        // ---------------------------------------------------------------- QA (14.10.4, Part 2.5)
        public const int QaFrameCap = 60;
        public const double LoadBudgetSeconds = 15.0;
        public const double WallBotInset = 0.40;
        /// <summary>Expected time of the goal trigger at the Keeper's Lantern for a test walk from rest at speed v (ADDENDUM A1 00):
        /// the walk to the board trigger 1498.5 / v + 0.25 s (the 0.5 s start ramp), the board ramp 2 x 1.5 / v (rest at s 1500),
        /// the 143 s ride, the platform walk 38.5 / v + 0.25 s (a new start ramp from rest):
        /// 1540 / v + 143.5 = 759.50 s at 2.5 m/s, 1170.17 s at 1.5 m/s, 205.10 s at 25 m/s (fix wave R1).</summary>
        public static double TriggerTimeAt(double speed)
        {
            double v = Math.Max(0.05, speed);
            double board = BulwarkConst.GoalRestS - GoalTriggerS;
            return GoalTriggerS / v + 0.5 * StartRamp + 2.0 * board / v + RideTotalTime + (EndingPath.SGoal - EndingPath.SFoot0) / v + 0.5 * StartRamp;
        }
        public static double ManualTriggerTime => TriggerTimeAt(WalkSpeed);       // 1170.17 s
        public static double CineTriggerTime => TriggerTimeAt(CinematicSpeed);    // 759.50 s
        /// <summary>Ending done = trigger + the 31.5 s end sequence (1181.67 s / 771.00 s).</summary>
        public static double ManualDoneTime => ManualTriggerTime + EndTotal;
        public static double CineDoneTime => CineTriggerTime + EndTotal;
        /// <summary>14.10.4 tolerances and the exit deadlines (done + 8 s, as the spec's 1026 / 626 were done + 8.25 / 7.85).</summary>
        public const double ManualTriggerTol = 4.0, CineTriggerTol = 3.0;
        public static double ManualExitBy => ManualDoneTime + 8.0;
        public static double CineExitBy => CineDoneTime + 8.0;
        public const double GroundSpeedPeakMax = 2.60;
        /// <summary>14.10.4 allowance for the cinematic look (at most 0.3 deg).</summary>
        public const double CineRollMaxDeg = 0.3;
        /// <summary>14.10.4 "roll 0.00 +- 0.01 deg": the camera never rolls (CameraRig, the sway is yaw / pitch only), so every
        /// autotest asserts max |roll| at most 0.01 deg, also at the cinematic speed.</summary>
        public const double RollTolDeg = 0.01;

        // ---------------------------------------------------------------- audio (14.3, 14.12)
        public const int AudioSampleRate = (int)BulwarkConst.AudioSampleRate;
        public const double LimiterCeilingDb = BulwarkConst.LimiterAsymptoteDb;  // asymptote; true peak <= -1 dBFS
        public const double FootPeakDb = -30.0;

        // ---------------------------------------------------------------- helpers
        /// <summary>Beat index 0..6 of s with the boundaries above (no hysteresis).</summary>
        public static int BeatIndex(double s)
        {
            for (int i = BeatCount - 1; i > 0; i--) if (s >= BeatBoundaries[i]) return i;
            return 0;
        }

        /// <summary>Start s of beat n (1-based); clamps.</summary>
        public static double BeatStart(int beat1) => BeatBoundaries[Math.Max(0, Math.Min(BeatCount - 1, beat1 - 1))];
    }
}
