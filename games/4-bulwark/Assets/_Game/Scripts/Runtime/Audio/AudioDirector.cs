// AudioDirector: the main-thread side of the audio (MASTER_PROMPT 14.3, 14.11; lesson 3.2 #14; C-26). Thread's AudioDirector
// pattern rewritten with The Ring's CreateRig: a dedicated "AudioRig" GameObject (never on the camera) with a clip-less
// AudioSource (2D, loop, no clip) and BwSynth as its generator; AudioSettings.Reset to 48 kHz BEFORE the generator exists;
// Play() after AddComponent; runInBackground in test and dump modes. Thread's footstep AudioSource pool, its listener path
// and its metal re-render are not imported: footsteps go into the DSP (IAudioHooks.Footstep -> BwSynth.Step).
// Each frame (order 700, after ProgressDirector 500 and BeatDirector 600): BulwarkScore.Evaluate(s, ambient t, speed), master
// = UiDirector.FadeAudio (the bed: intro fade-in with the picture, ending fade-out, restart / quit fade), impactBus =
// UiDirector.FadeImpacts (the audio's own envelope for events: open from intro t 0), pause duck -12 dB over 0.4 s (unscaled
// time), published to the audio thread. 14.11: the first wave impact sounds at intro t 0.5 s, in the black (Impact on the
// post-fader impact bus; T9). Impact(strength, pan) is the hook for SeaState.Impacts (14.3 event coupling).
// -audio-dump <file> [-audio-dump-seconds N] records the real output to a WAV with a sidecar that logs (t, s) per frame, the
// dump time of LoadComplete (introStart) and of every impact, so Tools/qa/audio_analysis.py can replay the same (t, s) offline
// and compare within 1 dB, and the intro impact can be found in the black.
// The device may run at 44.1 kHz: coefficients follow AudioSettings.outputSampleRate, rebuilt on a configuration change.
// Wave X (ADDENDUM_A1 section 2, LEAD DECISIONS 1 and 3): the listener pose (camera position and yaw) goes to the kraken
// spatialiser; every new KrakenCall on the KrakenCalls bus becomes one KrakenEvent (delay = T0 - ambient now, plus d / 343 s
// for slaps and water roar outdoors); the EndingRide state drives the ride layers (acceleration by differentiation, 0.3 s
// smoothing) and its phase changes fire the ride cues (doors, coupling, motor). -audio-qa adds the QA writer (AudioQa).
// The dump sidecar also lists the kraken events and the ride cues (dump time, kind).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using Megastructure.UI;

namespace Megastructure.Audio
{
    [DefaultExecutionOrder(700)]
    [DisallowMultipleComponent]
    public sealed class AudioDirector : MonoBehaviour, IAudioHooks
    {
        public static AudioDirector Instance { get; private set; }

        BwSynth m_synth;
        AudioSource m_source;
        BwAudioParams m_last = BwAudioParams.Silent();
        float m_duck = 1f;
        bool m_paused, m_ending, m_warnedHeartbeat, m_introImpactDone;
        double m_startReal;
        int m_seed = 1;
        string m_dumpPath;
        readonly List<double> m_dumpT = new List<double>(4096), m_dumpS = new List<double>(4096);
        readonly List<double> m_dumpImpactT = new List<double>(16);
        readonly List<long> m_dumpImpactFrame = new List<long>(16);
        double m_dumpT0 = -1, m_dumpIntroStart = -1;
        long m_dumpIntroFrame = -1;
        readonly List<string> m_dumpEvents = new List<string>(64);
        int m_krakenSerial;
        int m_ridePhase;
        float m_rideSpeedPrev, m_rideAccel;

        public BwSynth Synth => m_synth;
        public bool Ending => m_ending;

        void Awake()
        {
            Instance = this;
            BwServices.Audio = this;
            m_seed = Math.Max(1, CliArgs.Seed);
            if (CliArgs.AutoTest || CliArgs.AudioDump != null || CliArgs.SoakSeconds > 0) Application.runInBackground = true;
            CreateRig();
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            m_krakenSerial = KrakenCalls.Serial;
            if (!string.IsNullOrEmpty(AudioQa.Mode)) gameObject.AddComponent<AudioQa>();
        }

        void OnDestroy()
        {
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
            if (ReferenceEquals(BwServices.Audio, this)) BwServices.Audio = null;
            if (Instance == this) Instance = null;
        }

        /// <summary>The Ring's CreateRig: 48 kHz reset, then a clip-less AudioSource with the generator, then Play().</summary>
        void CreateRig()
        {
            var cfg = AudioSettings.GetConfiguration();
            if (cfg.sampleRate != BwAudioSpec.SampleRate)
            {
                cfg.sampleRate = BwAudioSpec.SampleRate;
                if (!AudioSettings.Reset(cfg)) Debug.LogWarning("[BW] audio: AudioSettings.Reset to 48 kHz refused; running at " + AudioSettings.outputSampleRate + " Hz");
            }
            var go = new GameObject("AudioRig");
            go.transform.SetParent(transform, false);
            m_source = go.AddComponent<AudioSource>();
            m_source.clip = null;
            m_source.playOnAwake = false;
            m_source.loop = true;
            m_source.spatialBlend = 0f;
            m_source.volume = 1f;
            m_source.bypassListenerEffects = true;
            m_synth = go.AddComponent<BwSynth>();
            m_synth.Init(AudioSettings.outputSampleRate, m_seed);
            m_source.Play();   // after AddComponent: the generator callback starts now
            m_startReal = Time.realtimeSinceStartupAsDouble;
            Debug.Log("[BW] audio: generator on a clip-less AudioSource at " + AudioSettings.outputSampleRate + " Hz (" + AudioSettings.speakerMode + ")");
        }

        void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            if (m_synth == null) return;
            m_synth.Rebuild(AudioSettings.outputSampleRate, m_seed, in m_last);
            if (m_source != null && !m_source.isPlaying) m_source.Play();
            Debug.Log("[BW] audio: device changed (" + deviceWasChanged + "), DSP rebuilt at " + AudioSettings.outputSampleRate + " Hz");
        }

        void Start()
        {
            string dump = CliArgs.AudioDump;
            if (!string.IsNullOrEmpty(dump)) StartDump(dump, (float)CliArgs.AudioDumpSeconds);
        }

        void Update()
        {
            if (m_synth == null) return;
            var e = m_synth.TakeError();
            if (e != null) Debug.LogError("[BW-FAIL] audio thread exception: " + e);
            if (!m_warnedHeartbeat && Time.realtimeSinceStartupAsDouble - m_startReal > 3.0 && m_synth.Callbacks == 0)
            {
                m_warnedHeartbeat = true;
                Debug.LogError("[BW-FAIL] audio: no generator callback after 3 s (AudioSource not playing?)");
            }

            double s = GameState.S;
            var p = BwServices.Player;
            double speed = p != null ? Math.Abs(p.Speed) : 0.0;
            float rdt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            var ride = ReadRide(Mathf.Min(Time.deltaTime, 0.1f));
            var a = BulwarkScore.Evaluate(s, BwClock.Ambient, speed, in ride);
            Listener(out Vector3 eye, out float yaw);
            a.lx = eye.x; a.ly = eye.y; a.lz = eye.z; a.lyaw = yaw;
            a.indoors = ride.Phase == 0 && BulwarkScore.ListenerIndoors(s) ? 1f : 0f;
            float duckTarget = m_paused ? (float)BwAudioSpec.Db(BwAudioSpec.PauseDuckDb) : 1f;
            m_duck = Mathf.MoveTowards(m_duck, duckTarget, rdt * (1f - (float)BwAudioSpec.Db(BwAudioSpec.PauseDuckDb)) / (float)BwAudioSpec.PauseDuckSeconds);
            a.master = UiDirector.FadeAudio;
            a.impactBus = UiDirector.FadeImpacts;
            a.duck = m_duck;
            a.paused = m_paused ? 1f : 0f;
            m_last = a;
            m_synth.Publish(in a);
            PollKraken(eye);

            // 14.11: "Black 0-1.0 s with the first wave impact heard at 0.5 s" (once per load; the bus is already open)
            float intro = UiDirector.IntroTime;
            if (!m_introImpactDone && intro >= UiTimeline.IntroImpact)
            {
                m_introImpactDone = true;
                Impact(BulwarkScore.IntroImpactStrength, BulwarkScore.IntroImpactPan, BulwarkScore.ImpactSubOnly(s));
                Debug.Log("[BW] audio: intro wave impact at intro t=" + intro.ToString("0.000", CultureInfo.InvariantCulture)
                          + " s (picture " + UiDirector.FadePicture.ToString("0.00", CultureInfo.InvariantCulture)
                          + ", impact bus " + UiDirector.FadeImpacts.ToString("0.00", CultureInfo.InvariantCulture) + ")");
            }

            if (m_dumpPath != null)
            {
                var cap = m_synth.Capture;
                int frames = cap != null ? cap.Count / Math.Max(1, cap.Channels) : 0;
                if (cap != null && cap.IsRecording && frames > 0)
                {
                    // the dump clock is the SAMPLE clock: t 0 = the first recorded sample (the audio thread may start writing
                    // well after the capture was armed), so (t, s) and the impact times line up with the WAV
                    double now = Time.realtimeSinceStartupAsDouble;
                    if (m_dumpT0 < 0) m_dumpT0 = now - frames / (double)cap.SampleRate;
                    double dt = now - m_dumpT0;
                    m_dumpT.Add(dt);
                    m_dumpS.Add(s);
                    if (m_dumpIntroStart < 0 && intro >= 0f)
                    {
                        m_dumpIntroStart = dt - intro;                                       // dump time of LoadComplete
                        m_dumpIntroFrame = frames - (long)Math.Round(intro * cap.SampleRate);   // and its sample index
                    }
                }
                if (cap != null && cap.IsFull) FinishDump(cap);
            }
        }

        /// <summary>
        /// One wave impact (14.3 layer B boom) on the post-fader impact bus: heard whenever the impact bus is open, also while the
        /// picture is black (14.11). Hook for SeaState.Impacts (strength 0.3-1.0); pan -1..1 (east / sea = +).
        /// </summary>
        public void Impact(float strength, float pan, bool subOnly = false)
        {
            if (m_synth == null) return;
            m_synth.Impact(strength, pan, subOnly);
            var cap = m_dumpPath != null ? m_synth.Capture : null;
            if (cap != null && cap.IsRecording)
            {
                long f = cap.Count / Math.Max(1, cap.Channels);   // sample index reached when queued (it starts at the next callback)
                m_dumpImpactFrame.Add(f);
                m_dumpImpactT.Add(f / (double)cap.SampleRate);
            }
        }

        // ------------------------------------------------------------------ kraken and ride (Wave X)
        /// <summary>World eye position and view yaw (deg, 0 = +Z) of the camera that renders (capture poses included).</summary>
        static void Listener(out Vector3 eye, out float yaw)
        {
            var cc = BwServices.CameraControl;
            var cam = cc != null ? cc.Camera : Camera.main;
            if (cam != null) { var tr = cam.transform; eye = tr.position; yaw = tr.eulerAngles.y; return; }
            var pl = BwServices.Player;
            eye = pl != null ? pl.EyePosition : Vector3.zero;
            yaw = (float)BwPath.HeadingDeg(GameState.S);
        }

        /// <summary>
        /// Converts a KrakenCall into the audio thread's event: delay = T0 - now (sound scheduled ahead is honoured), plus the
        /// air delay d / 343 s for slaps and water roar outdoors (ADDENDUM_A1 2.1 rule 2: T0 is the picture's time). A call that
        /// arrives late starts now with the remaining duration; false when less than 1 s of it is left.
        /// </summary>
        public static bool ToEvent(in KrakenCall c, Vector3 listener, double now, out KrakenEvent e, double listenerS = -1.0)
            => KrakenCallEvents.ToEvent(in c, listener, now, out e, listenerS);

        /// <summary>True where a DamShudder is the strike on the glass: the listener in beat 6 (the window gallery).</summary>
        public static bool IsStrike(double listenerS) => KrakenCallEvents.IsStrike(listenerS);

        void PollKraken(Vector3 eye)
        {
            int serial = KrakenCalls.Serial;
            if (serial == m_krakenSerial) return;
            int fresh = Math.Min(KrakenCalls.Count, Math.Max(0, serial - m_krakenSerial));
            m_krakenSerial = serial;
            double now = BwClock.Ambient;
            var inv = CultureInfo.InvariantCulture;
            for (int i = KrakenCalls.Count - fresh; i < KrakenCalls.Count; i++)
            {
                var c = KrakenCalls.Get(i);
                if (!ToEvent(in c, eye, now, out KrakenEvent e, GameState.S)) { Debug.LogWarning("[BW] audio: kraken " + c.Kind + " arrived too late, skipped"); continue; }
                m_synth.Kraken(in e);
                float d = Vector3.Distance(c.Position, eye);
                double lvl = KrakenRecipe.LevelDb(e.kind, d, e.strength, e.underwater);
                Debug.Log("[BW] audio: kraken " + (e.kind == KrakenRecipe.Strike ? "Strike" : c.Kind.ToString()) + " d " + d.ToString("0", inv) + " m, level " + lvl.ToString("0.0", inv)
                          + " dBFS, delay " + e.delay.ToString("0.00", inv) + " s, " + e.duration.ToString("0.0", inv) + " s"
                          + (e.underwater ? ", under water" : ""));
                DumpEvent("kraken", e.kind, e.delay, e.duration);
            }
        }

        /// <summary>The EndingRide contract as the score's ride state; fires the ride cues on a phase change.</summary>
        BwRideState ReadRide(float dt)
        {
            var r = new BwRideState
            {
                Phase = (int)EndingRide.Phase, Progress = EndingRide.PhaseProgress, Altitude = EndingRide.Altitude,
                Distance = EndingRide.DistanceFromFace, Speed = EndingRide.Speed,
            };
            if (r.Phase == 0) { m_rideSpeedPrev = 0f; m_rideAccel = 0f; r.Altitude = (float)BulwarkScore.RideTopAltitude; }
            else
            {
                float acc = dt > 1e-4f ? (r.Speed - m_rideSpeedPrev) / dt : 0f;
                m_rideAccel += (acc - m_rideAccel) * Mathf.Clamp01(dt / 0.3f);
                m_rideSpeedPrev = r.Speed;
                r.Accel = m_rideAccel;
            }
            int cue = RideCues.ForTransition(m_ridePhase, r.Phase);
            if (cue >= 0 && m_synth != null)
            {
                m_synth.Ride((RideCue)cue);
                Debug.Log("[BW] audio: ride " + (EndingRidePhase)r.Phase + " (cue " + (RideCue)cue + ")");
                DumpEvent("ride", cue, 0.0);
            }
            m_ridePhase = r.Phase;
            return r;
        }

        void DumpEvent(string what, int kind, double delay, double duration = 0.0)
        {
            var cap = m_dumpPath != null && m_synth != null ? m_synth.Capture : null;
            if (cap == null || !cap.IsRecording) return;
            double t = cap.Count / Math.Max(1, cap.Channels) / (double)cap.SampleRate + delay;
            m_dumpEvents.Add("{\"what\": \"" + what + "\", \"kind\": " + kind + ", \"t\": " + t.ToString("0.000", CultureInfo.InvariantCulture)
                             + (duration > 0.0 ? ", \"duration\": " + duration.ToString("0.00", CultureInfo.InvariantCulture) : "") + "}");
        }

        // ------------------------------------------------------------------ IAudioHooks
        public void Footstep(FootSurface surface, float pan, float gain)
        {
            if (m_synth != null) m_synth.Step(surface, pan, gain);
        }

        public void SetPauseDuck(bool paused) { m_paused = paused; }

        /// <summary>The ending's audio envelope comes from UiDirector.FadeAudio (the 14.11 timeline); this only latches the state.</summary>
        public void BeginEnding()
        {
            if (m_ending) return;
            m_ending = true;
            Debug.Log("[BW] audio: ending");
        }

        public void StartDump(string path, float seconds)
        {
            if (m_synth == null) return;
            var cap = new AudioCapture(Math.Max(1.0, seconds), AudioSettings.outputSampleRate, 2);
            m_synth.Capture = cap;
            cap.Arm();
            m_dumpPath = path;
            m_dumpT.Clear(); m_dumpS.Clear(); m_dumpT0 = -1; m_dumpIntroStart = -1; m_dumpIntroFrame = -1;
            m_dumpImpactT.Clear(); m_dumpImpactFrame.Clear(); m_dumpEvents.Clear();
            Debug.Log("[BW] audio dump armed: " + seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s -> " + path);
        }

        void FinishDump(AudioCapture cap)
        {
            string path = m_dumpPath;
            m_dumpPath = null;
            m_synth.Capture = null;
            try
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(path));
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                int ch = Math.Max(1, cap.Channels);
                WavWriter.Write(path, cap.Buffer, cap.Count, ch, cap.SampleRate, WavFormat.Float32, m_seed);
                var inv = CultureInfo.InvariantCulture;
                var sb = new StringBuilder();
                sb.Append("{\n  \"wav\": \"").Append(Path.GetFileName(path)).Append("\",\n  \"sampleRate\": ").Append(cap.SampleRate)
                  .Append(",\n  \"channels\": ").Append(ch).Append(",\n  \"seed\": ").Append(m_seed);
                // dump-relative seconds of LoadComplete (intro t 0) and of every impact (main-thread time and the sample index
                // reached when it was queued; the audio thread starts it at its next callback)
                sb.Append(",\n  \"introStart\": ").Append(m_dumpIntroStart.ToString("0.0000", inv));
                sb.Append(",\n  \"introStartFrame\": ").Append(m_dumpIntroFrame);
                sb.Append(",\n  \"introImpactTime\": ").Append(UiTimeline.IntroImpact.ToString("0.000", inv));
                sb.Append(",\n  \"impacts\": [");
                for (int i = 0; i < m_dumpImpactT.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append("{\"t\": ").Append(m_dumpImpactT[i].ToString("0.0000", inv)).Append(", \"frame\": ").Append(m_dumpImpactFrame[i]).Append('}');
                }
                sb.Append("]");
                sb.Append(",\n  \"events\": [").Append(string.Join(", ", m_dumpEvents)).Append("]");
                sb.Append(",\n  \"frames\": [");
                for (int i = 0; i < m_dumpT.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append('[').Append(m_dumpT[i].ToString("0.0000", inv)).Append(", ").Append(m_dumpS[i].ToString("0.0000", inv)).Append(']');
                }
                sb.Append("]\n}\n");
                File.WriteAllText(Path.ChangeExtension(path, ".json"), sb.ToString(), new UTF8Encoding(false));
                Debug.Log("[BW] audio dump written: " + path + " (" + (cap.Count / ch) + " frames, peak via audio_analysis.py); audio callback max "
                          + m_synth.MaxCallbackMs.ToString("0.00", inv) + " ms, last " + m_synth.LastCallbackMs.ToString("0.00", inv) + " ms, NaN resets " + m_synth.NanResets);
            }
            catch (Exception e) { Debug.LogError("[BW-FAIL] audio dump failed: " + e.Message); }
        }
    }
}
