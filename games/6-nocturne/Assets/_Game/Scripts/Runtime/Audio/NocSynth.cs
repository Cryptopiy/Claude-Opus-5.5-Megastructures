// NocSynth: the OnAudioFilterRead host of NocMixer (imported from Thread's ThreadSynth; IMPORT_PLAN 8.8, lesson 14).
// GENERATOR MODE ON A CLIP-LESS AudioSource is the primary path (Thread put the filter on the AudioListener first, which mixes
// whatever the listener hears and can stay silent): the component lives on the "Audio" object next to an AudioSource without a
// clip, which AudioDirector plays after both components exist. The main thread publishes NocAudioParams through a
// double-buffered mailbox with a volatile index (a torn read only mixes two consecutive frames' targets, which the per-sample
// smoothing hides); the audio thread copies the targets and calls NocMixer.Process. Audio-thread rules: no UnityEngine calls,
// no allocation, no locks, no logging; exceptions are stored and logged once by the main thread; a heartbeat counts callbacks;
// an optional preallocated capture records the real output for -audio-dump.
using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Single-producer single-consumer linear capture: the audio thread writes, the main thread reads once full.</summary>
    public sealed class AudioCapture
    {
        readonly float[] m_buf;
        readonly int m_frames;         // frames to record (the buffer holds up to maxChannels per frame)
        int m_count, m_limit;
        volatile int m_state;          // 0 idle, 1 armed, 2 recording, 3 full
        public int Channels { get; private set; }
        public readonly int SampleRate;

        public AudioCapture(double seconds, int sampleRate, int maxChannels)
        {
            SampleRate = sampleRate;
            m_frames = Math.Max(256, (int)(seconds * sampleRate));
            m_buf = new float[m_frames * Math.Max(1, maxChannels)];
        }

        public void Arm() { m_count = 0; m_state = 1; }
        public bool IsFull => m_state == 3;
        public int Count => m_count;
        public float[] Buffer => m_buf;

        /// <summary>Audio thread; allocation-free.</summary>
        public void Write(float[] data, int channels)
        {
            int st = m_state;
            if (st == 1)
            {
                // the buffer is sized for maxChannels; the dump is full after `seconds` of the device's real channel count
                Channels = channels;
                m_limit = Math.Min(m_buf.Length, m_frames * Math.Max(1, channels));
                st = 2;
            }
            if (st != 2) return;
            int n = Math.Min(data.Length, m_limit - m_count);
            n -= n % Math.Max(1, channels);
            if (n > 0) Array.Copy(data, 0, m_buf, m_count, n);
            m_count += n;
            m_state = (m_limit - m_count < channels || n <= 0) ? 3 : 2;
        }
    }

    [DisallowMultipleComponent]
    public sealed class NocSynth : MonoBehaviour
    {
        NocMixer m_mixer;
        NocAudioParams m_a, m_b;
        volatile int m_front;
        int m_callbacks;
        long m_lastTicks, m_maxTicks;
        Exception m_lastError;
        AudioCapture m_capture;

        public int SampleRate { get; private set; }
        public int Callbacks => Volatile.Read(ref m_callbacks);
        public double LastCallbackMs => Interlocked.Read(ref m_lastTicks) * 1000.0 / Stopwatch.Frequency;
        public double MaxCallbackMs => Interlocked.Read(ref m_maxTicks) * 1000.0 / Stopwatch.Frequency;
        public int NanResets { get { var m = Volatile.Read(ref m_mixer); return m != null ? m.NanResets : 0; } }

        public AudioCapture Capture
        {
            get => Volatile.Read(ref m_capture);
            set => Volatile.Write(ref m_capture, value);
        }

        /// <summary>Builds the mixer on the main thread (never on the audio thread).</summary>
        public void Init(int sampleRate, int seed)
        {
            SineTable.Warm();
            SampleRate = sampleRate;
            Volatile.Write(ref m_mixer, new NocMixer(sampleRate, seed));
        }

        /// <summary>Main thread: publish this frame's targets.</summary>
        public void Publish(in NocAudioParams p)
        {
            if (m_front == 0) { m_b = p; m_front = 1; }
            else { m_a = p; m_front = 0; }
        }

        /// <summary>Main thread: returns (and clears) an exception raised on the audio thread.</summary>
        public Exception TakeError() => Interlocked.Exchange(ref m_lastError, null);

        void OnAudioFilterRead(float[] data, int channels)
        {
            var mixer = Volatile.Read(ref m_mixer);
            if (mixer == null) return;
            try
            {
                long t0 = Stopwatch.GetTimestamp();
                NocAudioParams p = m_front == 0 ? m_a : m_b;
                mixer.Process(data, channels, in p);
                var cap = Volatile.Read(ref m_capture);
                if (cap != null) cap.Write(data, channels);
                Interlocked.Increment(ref m_callbacks);
                long dt = Stopwatch.GetTimestamp() - t0;
                Interlocked.Exchange(ref m_lastTicks, dt);
                if (dt > Interlocked.Read(ref m_maxTicks)) Interlocked.Exchange(ref m_maxTicks, dt);
            }
            catch (Exception e)
            {
                m_lastError = e;   // storing a reference does not allocate; the main thread logs it once
            }
        }
    }
}
