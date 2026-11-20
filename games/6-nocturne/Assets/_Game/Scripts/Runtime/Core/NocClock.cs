// NocClock: the scene clock (sign breathing, cloud scroll, traffic, koi wave, sway) and the grade frame counter, from Thread's
// ThreadClock plus Abyss's SceneClock.Wrapped and a separate rain clock t_r (14.7 H5). Play: Time.timeAsDouble.
// Capture: stepped by the harness. Static tests: frozen (-freeze-clock). -clock T starts the scene clock at T.
// The dither offset is frozen by default (14.7 H1, IMPORT_PLAN section 6 item 4): -dither temporal advances it per frame.
// Clock-driven motion never feeds a beat parameter (14.6: parameters are functions of s).
namespace Megastructure
{
    public enum DitherMode { Static = 0, Temporal = 1, Off = 2 }

    public static class NocClock
    {
        /// <summary>-freeze-clock or static tests: the scene clock does not advance.</summary>
        public static bool Frozen;
        /// <summary>-freeze-rain: the rain clock t_r does not advance (rain stills).</summary>
        public static bool FreezeRain;
        /// <summary>Capture harness drives the clocks explicitly (Set / Advance).</summary>
        public static bool Stepped;
        static double s_time, s_rain;
        static double s_lastUnity;
        static bool s_init;
        /// <summary>Grade frame index (drives the R2 dither offset in DitherMode.Temporal).</summary>
        public static long Frame;
        /// <summary>Dither mode (default Static: the R2 offset is frozen).</summary>
        public static DitherMode Dither = DitherMode.Static;

        /// <summary>True when the dither offset does not move (Static or Off).</summary>
        public static bool FreezeDither => Dither != DitherMode.Temporal;

        /// <summary>Scene time (s, double).</summary>
        public static double Time => s_time;
        /// <summary>Thread name of the scene clock (the imported QA reads it).</summary>
        public static double Ambient => s_time;
        /// <summary>Rain time t_r (s, double).</summary>
        public static double Rain => s_rain;

        /// <summary>Scene time wrapped to a loop of the given period, in double (Abyss SceneClock.Wrapped; no drift after hours).</summary>
        public static double Wrapped(double period) => s_time - period * System.Math.Floor(s_time / period);

        /// <summary>Called once per frame by ProgressDirector with Time.timeAsDouble.</summary>
        public static void Tick(double unityTime)
        {
            if (!s_init) { s_lastUnity = unityTime; s_init = true; }
            double dt = unityTime - s_lastUnity;
            s_lastUnity = unityTime;
            if (!Stepped && dt > 0 && dt < 1.0)
            {
                if (!Frozen) s_time += dt;
                if (!FreezeRain && !Frozen) s_rain += dt;
            }
            if (Dither == DitherMode.Temporal) Frame++;
        }

        /// <summary>Capture harness: set the clocks explicitly (deterministic sequences).</summary>
        public static void Set(double t) { s_time = t; s_rain = t; }
        public static void SetRain(double t) { s_rain = t; }
        public static void Advance(double dt)
        {
            if (!Frozen) s_time += dt;
            if (!Frozen && !FreezeRain) s_rain += dt;
            if (Dither == DitherMode.Temporal) Frame++;
        }
        public static void Reset(double t0 = 0.0) { s_time = t0; s_rain = t0; s_init = false; Frame = 0; }

        /// <summary>Parses -dither static|temporal|off (alias -freeze-dither = static); default static.</summary>
        public static DitherMode ParseDither(string v)
        {
            if (v == null) return DitherMode.Static;
            switch (v.Trim().ToLowerInvariant())
            {
                case "temporal": return DitherMode.Temporal;
                case "off": return DitherMode.Off;
                default: return DitherMode.Static;
            }
        }
    }

    /// <summary>
    /// GradeState: parameters of the NocGrade pass (exposure, bloom, vignette, saturation, contrast, dither, picture fade).
    /// Written each frame by the AtmosphereDriver (through ProgressDirector) and the UiDirector (FadeBlack), read by
    /// NocGradeFeature. Thread's Darkness gain is gone (14.7 H1: the amplitude is a per-pixel function of luma).
    /// </summary>
    public static class GradeState
    {
        public static float ExposureEV;
        public static float BloomThreshold = 0.9f;
        public static float BloomIntensity = 0.45f;
        public static float BloomScatter = 0.70f;
        public static float Vignette = 0.20f;
        public static float Saturation;      // -100..100 (URP convention), +8 = 8 percent
        public static float Contrast;        // -100..100 (0 in Nocturne)
        /// <summary>Peak of the TPDF in 8-bit codes for Y' &lt;= 0.35 (2.0; ladder 2.5 via -dither-amp; 0 with -dither off).</summary>
        public static float DitherPeak = (float)NocturneConstants.DitherPeak;
        /// <summary>D-007 chroma dither: +-codes per 2 x 2 block (1.0; 0 with -dither off).</summary>
        public static float DitherChroma = (float)NocturneConstants.DitherChroma;
        /// <summary>0..1 black applied inside the grade before the dither (UiDirector.ShaderFade: 1 - PictureMul).</summary>
        public static float FadeBlack = 1f;
        public static bool Enabled = true;
    }
}
