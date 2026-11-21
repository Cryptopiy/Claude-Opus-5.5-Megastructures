// GradeState: parameters of the BwGrade pass (exposure, white balance, saturation, contrast, bloom, vignette,
// fade, dither). One writer each: AtmosphereDriver writes the look values every frame (slew-limited, bypassed in
// capture mode); UiDirector writes FadeBlack and PauseDim; GameBootstrap writes Enabled (-grade-off).
// Read by BwGradeFeature/BwGradePass. Split out of Thread's ThreadClock.cs; Thread's Darkness gain is removed.
namespace Megastructure
{
    public enum DitherMode { Static = 0, Temporal = 1, Off = 2 }

    public static class GradeState
    {
        public static float ExposureEV;
        /// <summary>White balance temperature and tint in URP units (-100..100); positive temperature is warmer.</summary>
        public static float WhiteBalance;
        public static float Tint;
        /// <summary>-100..100 (URP convention): +4 = 4 percent.</summary>
        public static float Saturation;
        /// <summary>-100..100: k = Contrast/100 in the 0.18-pivot luminance power law (D-10).</summary>
        public static float Contrast;
        public static float BloomThreshold = 0.9f;
        public static float BloomIntensity = 0.2f;
        public static float BloomScatter = 0.7f;
        public static float Vignette = 0.12f;
        /// <summary>Peak of the TPDF dither in 8-bit codes (per beat, 14.12; 0 = off).</summary>
        public static float DitherPeak = (float)FrameworkConst.DitherPeakDark;
        /// <summary>0..1 black applied inside the grade (title / ending / restart fades). Owner: UiDirector.</summary>
        public static float FadeBlack;
        /// <summary>0..1 pause dim applied inside the grade. Owner: UiDirector.</summary>
        public static float PauseDim;
        public static bool Enabled = true;

        public static void ResetDefaults()
        {
            ExposureEV = 0f; WhiteBalance = 0f; Tint = 0f; Saturation = 0f; Contrast = 0f;
            BloomThreshold = 0.9f; BloomIntensity = 0.2f; BloomScatter = 0.7f; Vignette = 0.12f;
            DitherPeak = (float)FrameworkConst.DitherPeakDark; FadeBlack = 0f; PauseDim = 0f; Enabled = true;
        }
    }
}
