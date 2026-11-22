// AudioDirector: the main-thread side of the procedural audio (MASTER_PROMPT 14.3, Part 9 "Audio"; IMPORT_PLAN 8.9; docs/AUDIO.md),
// imported from Thread's AudioDirector: implements IAudioHooks (NocServices.Audio), lives on the "Audio" object with a clip-less
// AudioSource and NocSynth (generator mode, lesson 14), initialises the score DSP (NocMixer) at the device rate, PLAYS the
// source after both components exist (lesson 14), and publishes the snapshot NocAudioParams every frame: master =
// UiDirector.FadeAudio (intro fade, the 6 s ending fade, restart / quit fades) x the pause duck (smooth 0.6 s ramp), the rain
// density of s, s, the smoothed walk speed (the DSP extrapolates s between frames), the listener (camera) position and yaw
// for the spatial sources, and the scene time; footsteps arrive through IAudioHooks.Footstep (the PlayerController's stride)
// and are mixed inside the generator. Everything the score plays is keyed to s in NocturneScore. -audio-dump <file>
// [-audio-dump-seconds N] records N seconds (default 10) of the real output, writes a 16-bit WAV, renders the same window
// offline (OfflineAudioRenderer, same rate, seed, s and rain, 8 s pre-roll; standing still, so run it without movement, e.g.
// -nofade -quit-after 20) into <file>_offline.wav and compares the RMS: 14.7 H8 "-audio-dump within 1 dB of the offline render"
// ([NOC] audio dump vs offline ... PASS/FAIL, report <report dir>/audio_dump.json). Zero allocations per frame.
using System;
using System.IO;
using UnityEngine;

namespace Megastructure
{
    [DefaultExecutionOrder(700)]   // after ProgressDirector (500) and BeatDirector (600): s is final for the frame
    [DisallowMultipleComponent]
    public sealed class AudioDirector : MonoBehaviour, IAudioHooks
    {
        public static AudioDirector Instance { get; private set; }

        [Tooltip("Seconds of the pause duck ramp.")]
        public float pauseRampSeconds = 0.6f;
        const float PauseDuckGain = 0.25f;
        const double DumpAfterIntro = 4.0;

        NocSynth m_synth;
        AudioSource m_source;
        NocAudioParams m_params;
        bool m_paused;
        float m_duck = 1f;
        bool m_errorLogged, m_heartbeatLogged, m_introMarked, m_summaryLogged;
        double m_startTime, m_nextBeat;
        int m_introSlow, m_introOver, m_lastCallbacks;
        /// <summary>Heartbeat period (s) and the end of the intro (s after Start: the loading frames' long callbacks are not counted).</summary>
        const double HeartbeatPeriod = 30.0, IntroSeconds = 10.0;
        string m_dumpPath;
        float m_dumpSeconds = 10f;
        AudioCapture m_capture;
        bool m_dumpArmed, m_dumpDone;
        double m_lastS = double.NaN, m_speed;
        int m_seq;

        void Awake()
        {
            Instance = this;
            NocServices.Audio = this;
            m_source = GetComponent<AudioSource>();
            if (m_source == null) m_source = gameObject.AddComponent<AudioSource>();
            m_synth = GetComponent<NocSynth>();
            if (m_synth == null) m_synth = gameObject.AddComponent<NocSynth>();
            m_source.clip = null;
            m_source.playOnAwake = false;
            m_source.loop = false;
            m_source.spatialBlend = 0f;
            m_source.volume = 1f;
            m_dumpPath = CliArgs.AudioDump;
            m_dumpSeconds = (float)CliArgs.GetDouble("-audio-dump-seconds", 10.0);
        }

        void Start()
        {
            int rate = AudioSettings.outputSampleRate;
            m_synth.Init(rate > 0 ? rate : (int)NocturneConstants.AudioSampleRate, CliArgs.Seed);
            m_source.Play();   // generator mode: OnAudioFilterRead of NocSynth runs on this clip-less, playing source
            m_startTime = Time.realtimeSinceStartupAsDouble;
            Debug.Log(FormattableString.Invariant($"[NOC] audio: generator on a clip-less AudioSource, device rate {rate} Hz, speaker mode {AudioSettings.speakerMode}"));
        }

        void LogBeat(string what)
        {
            int cb = m_synth.Callbacks;
            string line = FormattableString.Invariant($"[NOC] audio {what} t {Time.realtimeSinceStartupAsDouble - m_startTime:F0} s, s {ProgressDirector.S:F1}: {cb - m_lastCallbacks} callbacks, window max {m_synth.TakeWindowMaxMs():F2} ms, since the intro: slow (> 50 % of the buffer) {m_synth.SlowCallbacks - m_introSlow}, overruns {m_synth.Overruns - m_introOver} (audio overruns after the intro = {m_synth.Overruns - m_introOver})");
            m_lastCallbacks = cb;
            if (m_synth.Overruns - m_introOver > 0) Debug.LogWarning(line); else Debug.Log(line);
        }

        void OnApplicationQuit()
        {
            if (m_introMarked && !m_summaryLogged) { m_summaryLogged = true; LogBeat("summary"); }
        }

        void OnDestroy()
        {
            if (ReferenceEquals(NocServices.Audio, this)) NocServices.Audio = null;
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            float rdt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            m_duck = Mathf.MoveTowards(m_duck, m_paused ? PauseDuckGain : 1f, rdt / Mathf.Max(0.01f, pauseRampSeconds));
            double s = ProgressDirector.S;
            float dt = Mathf.Max(1e-4f, Time.unscaledDeltaTime);
            if (!double.IsNaN(m_lastS))
            {
                double v = (s - m_lastS) / dt;
                if (Math.Abs(v) > 20.0) v = 0.0;                                  // a jump (-progress, restart): no extrapolation
                m_speed += (1.0 - Math.Exp(-dt / 0.15)) * (v - m_speed);        // 150 ms smoothing of ds/dt
            }
            m_lastS = s;
            m_params.master = UiDirector.FadeAudio * m_duck;
            m_params.rain = (float)NocRainState.RhoAt(s);   // D-020: -rain X overrides rho (the picture uses the same rule)
            m_params.s = s;
            m_params.t = NocClock.Time;
            m_params.speed = m_paused ? 0.0 : Math.Max(0.0, m_speed);
            var cam = NocRig.Camera;
            if (cam != null) { Vector3 p = cam.transform.position; m_params.x = p.x; m_params.z = p.z; }
            else { NocLanes.Path.PointAt(NocMath.Clamp(s, 0.0, NocturneConstants.GoalS), out m_params.x, out m_params.z); }
            m_params.yaw = NocRig.Yaw;
            m_params.seq = ++m_seq;
            m_synth.Publish(in m_params);

            var err = m_synth.TakeError();
            if (err != null && !m_errorLogged) { m_errorLogged = true; Debug.LogError("[NOC-FAIL] audio thread exception: " + err); }
            double age = Time.realtimeSinceStartupAsDouble - m_startTime;
            if (!m_heartbeatLogged && age > 3.0)
            {
                m_heartbeatLogged = true;
                if (m_synth.Callbacks > 0) Debug.Log(FormattableString.Invariant($"[NOC] audio heartbeat: {m_synth.Callbacks} callbacks, last {m_synth.LastCallbackMs:F3} ms, max {m_synth.MaxCallbackMs:F3} ms"));
                else Debug.LogWarning("[NOC] audio heartbeat: no OnAudioFilterRead callback after 3 s (no audio device?)");
            }
            // C52: the intro's long callbacks (loading) are the baseline; then a line every 30 s with the window's longest callback and
            // the callbacks over half / all of their buffer's period since the intro (a 2160p recording must keep "overruns 0")
            if (!m_introMarked && age > IntroSeconds)
            {
                m_introMarked = true; m_introSlow = m_synth.SlowCallbacks; m_introOver = m_synth.Overruns; m_lastCallbacks = m_synth.Callbacks;
                m_synth.TakeWindowMaxMs(); m_nextBeat = age + HeartbeatPeriod;
            }
            if (m_introMarked && age >= m_nextBeat)
            {
                m_nextBeat += HeartbeatPeriod;
                LogBeat("heartbeat");
            }
            Dump();
        }

        // ---------------------------------------------------------------- IAudioHooks
        public void Footstep(FootSurface surface, float pan, float gain)
        {
            m_params.footsteps++;
            m_params.footPan = Mathf.Clamp(pan, -1f, 1f);
            m_params.footGain = Mathf.Clamp(gain, 0f, 1.5f);
            m_params.footSurface = (int)surface;
        }

        public void SetPauseDuck(bool paused) { m_paused = paused; }

        /// <summary>The 6 s ending fade is part of UiDirector.FadeAudio (UiTimeline.EndAudio); nothing else to start here.</summary>
        public void BeginEnding() { Debug.Log("[NOC] audio: ending fade (6 s) follows UiDirector.FadeAudio"); }

        public void StartDump(string path, float seconds)
        {
            m_dumpPath = path;
            m_dumpSeconds = seconds;
            m_dumpArmed = false;
            m_dumpDone = false;
        }

        void Dump()
        {
            if (string.IsNullOrEmpty(m_dumpPath) || m_dumpDone) return;
            if (!m_dumpArmed)
            {
                if (UiDirector.IntroTime < DumpAfterIntro) return;
                m_capture = new AudioCapture(m_dumpSeconds, m_synth.SampleRate, 8);
                m_capture.Arm();
                m_synth.Capture = m_capture;
                m_dumpArmed = true;
                Debug.Log(FormattableString.Invariant($"[NOC] audio dump: recording {m_dumpSeconds:F1} s"));
                return;
            }
            if (!m_capture.IsFull) return;
            m_synth.Capture = null;
            m_dumpDone = true;
            try
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(m_dumpPath));
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                int ch = Math.Max(1, m_capture.Channels);
                WavWriter.WritePcm16(m_dumpPath, m_capture.Buffer, m_capture.Count, ch, m_capture.SampleRate);
                Debug.Log(FormattableString.Invariant($"[NOC] audio dump written: {m_dumpPath} ({m_capture.Count / ch} frames, {ch} channels, {m_capture.SampleRate} Hz)"));
                CompareWithOffline(ch);
            }
            catch (Exception e) { Debug.LogError("[NOC-FAIL] audio dump failed: " + e.Message); }
        }

        /// <summary>Max |dB| difference between the dump and the offline render (14.7 H8).</summary>
        public const double DumpMatchDb = 1.0;
        /// <summary>Offline pre-roll of the dump comparison (R5 7 item 7: the reverbs fill before the compared window).</summary>
        public const double DumpPrerollSeconds = 8.0;

        /// <summary>RMS of the dumped samples (the first two channels) against an offline render of the same window.</summary>
        void CompareWithOffline(int ch)
        {
            int frames = m_capture.Count / Math.Max(1, ch);
            double ss = 0; long n = 0;
            for (int i = 0; i < frames; i++)
                for (int c = 0; c < Math.Min(2, ch); c++) { double v = m_capture.Buffer[i * ch + c]; ss += v * v; n++; }
            double dumpDb = RmsSink.Db(n > 0 ? Math.Sqrt(ss / n) : 0.0);
            var st = new OfflineRenderSettings
            {
                SampleRate = m_capture.SampleRate, Seed = CliArgs.Seed, S0 = ProgressDirector.S, Speed = 0.0,
                Seconds = frames / (double)Math.Max(1, m_capture.SampleRate), Footsteps = false, RainOverride = NocRainState.Override,
                PrerollSeconds = DumpPrerollSeconds,
            };
            var rms = new RmsSink();
            string off = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(m_dumpPath)) ?? ".", Path.GetFileNameWithoutExtension(m_dumpPath) + "_offline.wav");
            using (var wav = new WavStreamWriter(off, OfflineAudioRenderer.Channels, st.SampleRate, WavFormat.Pcm16))
                OfflineAudioRenderer.Render(st, wav, rms);
            double offDb = RmsSink.Db(rms.Rms), diff = dumpDb - offDb;
            bool ok = Math.Abs(diff) <= DumpMatchDb && m_synth.Callbacks > 0;
            string line = FormattableString.Invariant($"[NOC] audio dump vs offline: dump {dumpDb:F2} dBFS, offline {offDb:F2} dBFS, difference {diff:F2} dB (<= {DumpMatchDb:F1}), {m_synth.Callbacks} callbacks, s {st.S0:F1}, {st.Seconds:F1} s -> {(ok ? "PASS" : "FAIL")}");
            if (ok) Debug.Log(line); else Debug.LogError(line.Replace("[NOC]", "[NOC-FAIL]"));
            var j = new QaJson();
            j.BeginObject().Prop("tool", "AudioDirector.-audio-dump").Prop("pass", ok).Prop("dump", m_dumpPath).Prop("offline", off);
            j.Prop("dumpRmsDbfs", dumpDb).Prop("offlineRmsDbfs", offDb).Prop("differenceDb", diff).Prop("limitDb", DumpMatchDb);
            j.Prop("callbacks", m_synth.Callbacks).Prop("sampleRate", m_capture.SampleRate).Prop("channels", ch).Prop("seconds", st.Seconds).Prop("s", st.S0);
            j.EndObject();
            QaFiles.WriteText(QaPaths.ReportPath("audio_dump.json"), j.ToString());
        }
    }
}
