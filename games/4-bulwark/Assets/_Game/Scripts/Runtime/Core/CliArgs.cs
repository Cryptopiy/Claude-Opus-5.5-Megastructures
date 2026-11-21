// CliArgs: deterministic debug entry points (MASTER_PROMPT 11.1 plus the Part 14 flags). Imported from Thread.
//   -autotest              the AutoWalker from s 0 to the goal at 1.5 m/s (with -cinematic: 2.5 m/s), assertions, exit code
//   -beat N                start at the start of beat N (1..7)
//   -progress X            start at plan distance X (m)
//   -capture <dir>         run the capture harness and write PNGs + report.json to <dir>
//   -capture-set <name>    capture list (keyshots, beats, sequences, encode, stills, everything, ... ; ShotPlan)
//   -capture-only a,b      only these shots / sequences (comma list; pass as ONE argument, lesson 3.2 #1)
//   -capture-4k            also the 3840 x 2160 variant of every key shot
//   -seq-res WxH           resolution of sequence frames
//   -capture-warmup N, -capture-settle N, -capture-threads N, -capture-queue-mb N, -capture-flip   harness tuning
//   -report-dir <dir>      where QA runners write their JSON (default: next to Player.log)
//   -res WxH               window / capture resolution
//   -cinematic             start the cinematic autowalk (2.5 m/s)
//   -seed N                PRNG seed (audio, sway, placement)
//   -quit-after S          quit after S seconds of wall time
//   -perf-log <file>       write a frame-time CSV (at the 60 fps QA cap; add -uncapped to measure headroom, 11.7)
//   -perf-rt WxH           render offscreen at WxH for the perf log (4K budget on a smaller monitor)
//   -uncapped              with -perf-log only: measure uncapped (vSync 0, target -1, 11.7); every other run stays at the 60 fps QA cap (2.5.3)
//   -atmo-s S              QA: AtmosphereDriver evaluates the schedule at S whatever the camera s (logged as a warning)
//   -nofade                skip the title fade
//   -dither static|temporal|off   the one grade dither (D-05): static is the default; temporal only for high-bitrate
//                          cinematic captures; off == -dither-lsb 0 (undithered baseline of the encode test)
//   -dither-lsb X          override the per-beat dither peak (8-bit codes; 0 = off)
//   -freeze-dither         alias of -dither static (kept from Thread)
//   -freeze-ambient        frozen ambient clock (alias: -freeze-clock)
//   -clock T               start the ambient clock at T seconds
//   -grade-off             disable BwGrade (diagnostics only)
//   -uifont theme|legacy   UI Toolkit font (default theme; legacy = LegacyRuntime.ttf through FontDefinition.FromFont)
//   -audio-dump <file>     dump the real audio output to WAV (-audio-dump-seconds N, default 30)
//   -brisk                 test walk at 2.5 m/s (test walks only; the cinematic is always 2.5)
//   -speed X               test-walk speed override (m/s; test walks only)
//   -wallbot               wall-hugging bots (both lane edges, 0.40 m inside)
//   -soak S                soak test of S seconds
//   -tier-override c       force every DetailTier to coverage c (0 far .. 1 near; T7 crossover captures)
//   -graphics-api, -dev-build, -editor-timeout   editor batch tools (BatchTools / CaptureWatchdog), not the player
using System;
using System.Globalization;

namespace Megastructure
{
    public static class CliArgs
    {
        static string[] s_args;
        public static string[] Args
        {
            get
            {
                if (s_args == null)
                {
                    try { s_args = Environment.GetCommandLineArgs(); } catch { s_args = new string[0]; }
                }
                return s_args;
            }
        }

        /// <summary>Test hook: replace the argument list (null = back to the real command line).</summary>
        public static void Override(string[] args) { s_args = args; }

        public static bool Has(string flag)
        {
            foreach (var a in Args) if (string.Equals(a, flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static string Get(string flag, string fallback = null)
        {
            var a = Args;
            for (int i = 0; i < a.Length - 1; i++)
                if (string.Equals(a[i], flag, StringComparison.OrdinalIgnoreCase)) return a[i + 1];
            return fallback;
        }

        public static double GetDouble(string flag, double fallback)
        {
            var v = Get(flag);
            return v != null && double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? d : fallback;
        }

        public static int GetInt(string flag, int fallback)
        {
            var v = Get(flag);
            return v != null && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int d) ? d : fallback;
        }

        public static bool TryGetRes(out int w, out int h) => TryGetRes("-res", out w, out h);

        public static bool TryGetRes(string flag, out int w, out int h)
        {
            w = h = 0;
            var v = Get(flag);
            if (v == null) return false;
            var p = v.ToLowerInvariant().Split('x');
            return p.Length == 2 && int.TryParse(p[0], out w) && int.TryParse(p[1], out h) && w > 0 && h > 0;
        }

        /// <summary>Comma-separated list value (empty array when absent).</summary>
        public static string[] GetList(string flag)
        {
            var v = Get(flag);
            if (string.IsNullOrEmpty(v)) return new string[0];
            var parts = v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++) parts[i] = parts[i].Trim();
            return parts;
        }

        public static bool AutoTest => Has("-autotest");
        public static bool Cinematic => Has("-cinematic");
        public static bool NoFade => Has("-nofade");
        public static bool FreezeAmbient => Has("-freeze-ambient") || Has("-freeze-clock");
        public static bool Brisk => Has("-brisk");
        public static bool WallBot => Has("-wallbot");
        public static bool GradeOff => Has("-grade-off");
        public static string CaptureDir => Get("-capture");
        public static string CaptureSet => Get("-capture-set", "keyshots");
        public static string[] CaptureOnly => GetList("-capture-only");
        public static bool Capture4K => Has("-capture-4k");
        public static string ReportDir => Get("-report-dir");
        public static string PerfLog => Get("-perf-log");
        public static string AudioDump => Get("-audio-dump");
        public static double AudioDumpSeconds => GetDouble("-audio-dump-seconds", 30.0);
        public static int Seed => GetInt("-seed", 1);
        public static double QuitAfter => GetDouble("-quit-after", -1);
        public static double SoakSeconds => GetDouble("-soak", -1);
        public static double TierOverride => GetDouble("-tier-override", -1);
        /// <summary>UI font mode: true = LegacyRuntime.ttf, false = the theme default font (14.11).</summary>
        public static bool LegacyFont => string.Equals(Get("-uifont", "theme"), "legacy", StringComparison.OrdinalIgnoreCase);

        /// <summary>Dither mode (D-05). -freeze-dither is an alias of static; -dither-lsb 0 behaves as off.</summary>
        public static DitherMode DitherMode
        {
            get
            {
                string v = Get("-dither");
                if (v != null)
                {
                    if (v.Equals("temporal", StringComparison.OrdinalIgnoreCase)) return DitherMode.Temporal;
                    if (v.Equals("off", StringComparison.OrdinalIgnoreCase)) return DitherMode.Off;
                }
                if (Has("-dither-lsb") && DitherLsbOverride <= 0.0) return DitherMode.Off;
                return DitherMode.Static;
            }
        }

        /// <summary>-dither-lsb X: explicit dither peak in 8-bit codes, or -1 when absent (the per-beat schedule is used).</summary>
        public static double DitherLsbOverride => GetDouble("-dither-lsb", -1.0);

        /// <summary>Test-walk speed: -speed X, else -brisk (2.5), else walk (1.5). Never applies to the cinematic.</summary>
        public static double TestWalkSpeed
        {
            get
            {
                double v = GetDouble("-speed", -1);
                if (v > 0) return v;
                return Brisk ? FrameworkConst.BriskSpeed : FrameworkConst.WalkSpeed;
            }
        }

        /// <summary>Start progress from -progress X or -beat N (1-based), else 0.</summary>
        public static double StartS
        {
            get
            {
                double p = GetDouble("-progress", -1);
                if (p >= 0) return Math.Min(p, BwPath.Length);
                int b = GetInt("-beat", 0);
                if (b >= 1 && b <= FrameworkConst.BeatCount) return FrameworkConst.BeatBoundaries[b - 1];
                return 0;
            }
        }
    }
}
