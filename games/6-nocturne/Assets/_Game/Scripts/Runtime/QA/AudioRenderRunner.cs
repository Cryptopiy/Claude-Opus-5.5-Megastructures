// AudioRenderRunner: -render-audio <file.wav> (MASTER_PROMPT 11.9, 14.12 Flags, 14.7 H8 "NocturneScore.Render(s0, s1, speed)
// renders offline to WAV"; audit S3 item 18). Renders the score skeleton (NocMixer, the same generator chain that
// OnAudioFilterRead runs) without an audio device through OfflineAudioRenderer, from -render-s0 to -render-s1 (default 0 to
// 1500 m) at the walk speed (-speed X, -brisk or -cinematic = 2.5 m/s, default 2.5), with footsteps at the controller's
// stride, into a 32-bit float WAV plus a sidecar <file>.json ({"s": [s every second], "dt": 1, "speed": v, "stem": "mix"}) for
// Tools/qa/audio_analysis.py, and a report <report dir>/render_audio.json (peak, RMS per beat, DC). Then quits with 0 (1 on an
// exception or a non-finite sample). Runs before the world matters: no GPU work, no QA frame loop.
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Megastructure
{
    [DefaultExecutionOrder(-60)]
    public sealed class AudioRenderRunner : QaRunnerBase
    {
        protected override QaRunMode Mode => QaRunMode.RenderAudio;
        protected override string ReportName => "render_audio";
        protected override string ReportTag => "RENDER-AUDIO";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsurePresent() { EnsureRunner<AudioRenderRunner>(QaRunMode.RenderAudio); }

        /// <summary>Speed of the render: -speed X, else 2.5 with -brisk or -cinematic, else the cinematic 2.5 m/s.</summary>
        public static double RenderSpeed()
        {
            double v = CliArgs.GetDouble("-speed", CliArgs.Brisk || CliArgs.Cinematic ? NocturneConstants.BriskSpeed : NocturneConstants.CinematicSpeed);
            return v > 0.05 ? v : NocturneConstants.CinematicSpeed;
        }

        void Start()
        {
            if (!enabled) return;
            var inv = CultureInfo.InvariantCulture;
            string path = QaPaths.Resolve(CliArgs.RenderAudio);
            var st = new OfflineRenderSettings
            {
                Seed = CliArgs.Seed,
                S0 = Math.Max(0.0, CliArgs.GetDouble("-render-s0", 0.0)),
                S1 = Math.Min(NocturneConstants.GoalS, CliArgs.GetDouble("-render-s1", NocturneConstants.GoalS)),
                Speed = RenderSpeed(),
                RainOverride = NocRainState.Override,   // D-020: -rain X overrides rho
            };
            var rms = new RmsSink();
            var meter = new MeterSink();
            double seconds = OfflineAudioRenderer.Duration(st);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string error = null;
            try
            {
                using (var wav = new WavStreamWriter(path, OfflineAudioRenderer.Channels, st.SampleRate, WavFormat.Float32))
                    OfflineAudioRenderer.Render(st, wav, rms, meter);
                WriteSidecar(path, st, seconds);
            }
            catch (Exception e) { error = e.GetType().Name + ": " + e.Message; Debug.LogError("[NOC-FAIL] render-audio: " + e); }
            double peakDb = RmsSink.Db(meter.Peak);
            bool finite = !(double.IsNaN(meter.SumSq) || double.IsInfinity(meter.SumSq));
            Checks.Add("rendered", error == null, error ?? (seconds.ToString("0.0", inv) + " s of audio in " + sw.Elapsed.TotalSeconds.ToString("0.0", inv) + " s -> " + path));
            Checks.Add("finite", finite, "sum of squares " + meter.SumSq.ToString("R", inv));
            Checks.Add("peakBelowMinus1dBFS", meter.Peak <= Math.Pow(10.0, -1.0 / 20.0), "sample peak " + peakDb.ToString("0.00", inv) + " dBFS (14.3: <= -1)");
            Checks.Add("dcBelow0.001", Math.Abs(meter.Dc) < 0.001, "DC " + meter.Dc.ToString("0.000000", inv));
            Checks.Add("noErrorLogs", QaLog.ErrorCount == 0, QaLog.ErrorCount + " error/exception/assert messages");
            string summary = "s " + st.S0.ToString("0", inv) + "-" + st.S1.ToString("0", inv) + " at " + st.Speed.ToString("0.00", inv) + " m/s, "
                             + seconds.ToString("0.0", inv) + " s, peak " + peakDb.ToString("0.00", inv) + " dBFS, RMS " + RmsSink.Db(rms.Rms).ToString("0.00", inv) + " dBFS";
            Debug.Log("[NOC] render-audio " + summary);
            for (int b = 0; b < NocturneConstants.BeatCount; b++)
                if (rms.BeatSamples[b] > 0) Debug.Log("[NOC] render-audio beat " + (b + 1) + " RMS " + rms.BeatDb(b).ToString("0.00", inv) + " dBFS (14.3 target " + NocturneConstants.TargetRmsDb[b].ToString("0.0", inv) + ", score skeleton)");
            Complete(summary, j =>
            {
                j.Prop("wav", path).Prop("s0", st.S0).Prop("s1", st.S1).Prop("speed", st.Speed).Prop("seconds", seconds).Prop("sampleRate", st.SampleRate);
                j.Prop("peakDbfs", peakDb).Prop("rmsDbfs", RmsSink.Db(rms.Rms)).Prop("dc", meter.Dc).Prop("renderSeconds", sw.Elapsed.TotalSeconds);
                j.Key("beats").BeginArray();
                for (int b = 0; b < NocturneConstants.BeatCount; b++)
                    j.BeginObject(true).Prop("beat", b + 1).Prop("rmsDbfs", rms.BeatDb(b)).Prop("targetDbfs", NocturneConstants.TargetRmsDb[b]).EndObject();
                j.EndArray();
            });
        }

        /// <summary>Sidecar for Tools/qa/audio_analysis.py: s at t = 0, 1, 2 ... s.</summary>
        static void WriteSidecar(string wavPath, OfflineRenderSettings st, double seconds)
        {
            var j = new QaJson();
            j.BeginObject().Prop("stem", "mix").Prop("dt", 1.0).Prop("t0", 0.0).Prop("speed", st.Speed).Prop("seed", st.Seed);
            j.Key("s").BeginArray(true);
            int n = (int)Math.Floor(seconds) + 1;
            for (int i = 0; i < n; i++) j.Value(Math.Min(st.S1, st.S0 + st.Speed * i));
            j.EndArray().EndObject();
            QaFiles.WriteText(Path.ChangeExtension(wavPath, ".json"), j.ToString());
        }
    }
}
