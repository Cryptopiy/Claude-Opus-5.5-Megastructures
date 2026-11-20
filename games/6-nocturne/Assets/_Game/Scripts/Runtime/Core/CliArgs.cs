// CliArgs: deterministic debug entry points (MASTER_PROMPT 11.1, 14.12 Flags, IMPORT_PLAN 4.5), from Thread's CliArgs.
//   -autotest              AutoWalker start to goal (1.5 m/s; -brisk or -cinematic 2.5 m/s), assertions, exit code
//   -autotest-until S      smoke gate: the autotest passes once s >= S (beats up to S in order, no errors) and quits
//   -beat N                start at the start of beat N (1..7)
//   -progress X            start at the FRACTION X of the path (s = 1500 X, 14.12; Thread used metres)
//   -capture <dir>         run the capture harness and write PNGs + report.json to <dir>
//   -capture-set <name>    which capture list (keyshots, beats, statics, ...); -capture-only <id> (one value per call)
//   -res WxH               window / capture resolution
//   -cinematic             start the cinematic autowalk (2.5 m/s constant)
//   -seed N                PRNG seed (sway, QA)
//   -quit-after S          quit after S seconds of wall time
//   -perf-log <file>       write frame-time CSV (uncapped frame rate)
//   -nofade                skip the title fade
//   -rain X                rain density 0..1 (0 = off): overrides the 14.2.4 schedule rho (D-020; NocRainState.RhoAt)
//   -freeze-rain           frozen rain clock
//   -dither static|temporal|off   the one dither of NocGrade (default static; -freeze-dither = static)
//   -dither-amp X          peak amplitude in codes (encode ladder 2.0 / 2.5)
//   -sequence NAME         frame sequences (boundaries, statics, signs, rain, koi, traffic, encode = the ShotPlan sets of the
//                          same name); runs the capture harness, into -capture <dir> or QA/captures/seq_<NAME>
//   -tier N                force a fallback tier (0 = full, 1 = simplified, 2 = graybox; WorldContext.Tier)
//   -freeze-clock          frozen scene clock;  -clock T   start the scene clock at T
//   -render-audio <file>   offline WAV render of the score (AudioRenderRunner; -render-s0/-render-s1 m, speed from -speed /
//                          -brisk / -cinematic, default 2.5 m/s), then quit
//   -audio-dump <file>     dump the real OnAudioFilterRead output (-audio-dump-seconds N) and compare it with an offline render
//   -alloc-probe           per-PlayerLoop-subsystem attribution of the managed allocation per frame (NocAlloc), logged every 10 s
//   QA extras (Thread): -brisk, -speed X, -wallbot, -soak S, -capture-4k, -report-dir <dir>, -grade-off; -uifont theme|legacy
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

        /// <summary>Test hook: replace the argument list (null restores the process arguments).</summary>
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

        public static bool TryGetRes(out int w, out int h)
        {
            w = h = 0;
            var v = Get("-res");
            if (v == null) return false;
            var p = v.ToLowerInvariant().Split('x');
            return p.Length == 2 && int.TryParse(p[0], out w) && int.TryParse(p[1], out h) && w > 0 && h > 0;
        }

        public static bool AutoTest => Has("-autotest") || Has("-autotest-until");
        /// <summary>-autotest-until S (smoke gate): NaN when absent.</summary>
        public static double AutoTestUntil => GetDouble("-autotest-until", double.NaN);
        public static bool Cinematic => Has("-cinematic");
        public static bool NoFade => Has("-nofade");
        public static bool FreezeClock => Has("-freeze-clock") || Has("-freeze-ambient");
        public static bool FreezeRain => Has("-freeze-rain");
        /// <summary>Thread QA aliases: the dither offset is frozen unless -dither temporal; -freeze-ambient = -freeze-clock.</summary>
        public static bool FreezeDither => Dither != DitherMode.Temporal;
        public static bool FreezeAmbient => FreezeClock;
        public static double ClockStart => GetDouble("-clock", 0.0);
        /// <summary>-dither static|temporal|off; -freeze-dither is an alias of static (the default).</summary>
        public static DitherMode Dither => Has("-freeze-dither") ? DitherMode.Static : NocClock.ParseDither(Get("-dither"));
        public static double DitherAmp => GetDouble("-dither-amp", NocturneConstants.DitherPeak);
        public static double Rain => Math.Max(0.0, Math.Min(1.0, GetDouble("-rain", 1.0)));
        public static bool Brisk => Has("-brisk");
        public static bool WallBot => Has("-wallbot");
        /// <summary>-capture DIR; with -sequence NAME and no -capture: QA/captures/seq_NAME (resolved by the harness).</summary>
        public static string CaptureDir => Get("-capture") ?? (Sequence != null ? "QA/captures/seq_" + Sequence : null);
        public static bool Capturing => CaptureDir != null;
        /// <summary>-capture-set LIST; else the -sequence NAME (a ShotPlan set); else the key shots.</summary>
        public static string CaptureSet => Get("-capture-set") ?? Sequence ?? "keyshots";
        /// <summary>-sequence NAME (14.12): boundaries, statics, signs, rain, koi, traffic, encode (lower case).</summary>
        public static string Sequence
        {
            get
            {
                var v = Get("-sequence");
                return string.IsNullOrEmpty(v) || v.StartsWith("-", StringComparison.Ordinal) ? null : v.Trim().ToLowerInvariant();
            }
        }
        public static string PerfLog => Get("-perf-log");
        public static string AudioDump => Get("-audio-dump");
        public static string RenderAudio => Get("-render-audio");
        public static int Seed => GetInt("-seed", 1);
        public static double QuitAfter => GetDouble("-quit-after", -1);
        public static double SoakSeconds => GetDouble("-soak", -1);
        public static int Tier => GetInt("-tier", 0);

        /// <summary>Start progress (m): -progress X is the fraction X of the path (s = 1500 X), else -beat N (1-based), else 0.</summary>
        public static double StartS
        {
            get
            {
                double p = GetDouble("-progress", -1);
                if (p >= 0) return Math.Min(p, 1.0) * NocturneConstants.PathLength;
                int b = GetInt("-beat", 0);
                if (b >= 1 && b <= NocturneConstants.BeatCount) return NocturneConstants.BeatBoundaries[b - 1];
                return 0;
            }
        }
    }
}
