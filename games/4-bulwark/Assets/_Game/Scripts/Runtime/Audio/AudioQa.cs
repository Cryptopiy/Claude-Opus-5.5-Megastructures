// AudioQa: a QA-only writer of the audio contracts, so the kraken voice and the ride sound can be verified in the built
// player before (and independently of) the kraken show and the ending ride controller. Added by AudioDirector only when the
// command line has -audio-qa:
//   -audio-qa kraken   publishes one KrakenCall every 16 s on the KrakenCalls bus, cycling through every KrakenCallKind, at
//                      the BwAudioExamples geometry relative to the live listener (distance, bearing right of the view,
//                      height, under water for the beat 6 kinds), strength 1, scheduled 0.5 s ahead (sound before sight)
//   -audio-qa ride     drives EndingRide with BulwarkScore.SyntheticRide from 3 s after the start (boarding, 75 s descent,
//                      transfer, 35 s shuttle, arrival) and a Stinger KrakenCall 8 s after the arrival (3.5 km east, 2 km up)
//   -audio-qa all      both (the kraken from 3 s, the ride from 3 + 8 x 16 s)
// Never use it together with the real writers (KrakenDirector / the ride controller): it logs a warning when the bus has
// other events. Each publish is logged ("[BW] audio-qa: ...") so the -audio-dump sidecar times can be matched.
using System;
using System.Globalization;
using UnityEngine;

namespace Megastructure.Audio
{
    [DefaultExecutionOrder(650)]
    [DisallowMultipleComponent]
    public sealed class AudioQa : MonoBehaviour
    {
        public const double KrakenPeriod = 16.0, StartDelay = 3.0;
        bool m_kraken, m_ride, m_stinger;
        double m_t0 = -1.0, m_rideT0;
        int m_next;
        int m_ownSerial = -1;

        public static string Mode => CliArgs.Get("-audio-qa");

        void Start()
        {
            string mode = (Mode ?? "").ToLowerInvariant();
            m_kraken = mode == "kraken" || mode == "all";
            m_ride = mode == "ride" || mode == "all";
            m_rideT0 = StartDelay + (mode == "all" ? KrakenRecipe.KindCount * KrakenPeriod : 0.0);
            Debug.LogWarning("[BW] audio-qa: QA writer active (" + mode + "): publishes test KrakenCalls / EndingRide states");
        }

        void Update()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (m_t0 < 0.0) m_t0 = now;
            double t = now - m_t0;
            var inv = CultureInfo.InvariantCulture;

            if (m_kraken && m_next < KrakenRecipe.KindCount && t >= StartDelay + m_next * KrakenPeriod)
            {
                if (m_ownSerial >= 0 && KrakenCalls.Serial != m_ownSerial) Debug.LogWarning("[BW] audio-qa: another writer is publishing KrakenCalls");
                int kind = m_next++;
                BwAudioExamples.ExampleFor(kind, out _, out double dist, out double bearing, out double height, out bool underwater);
                Listener(out Vector3 eye, out float yaw);
                double a = (yaw + bearing) * Math.PI / 180.0;
                double horiz = Math.Sqrt(Math.Max(0.0, dist * dist - height * height));
                var c = new KrakenCall
                {
                    Kind = (KrakenCallKind)kind,
                    Position = new Vector3((float)(eye.x + horiz * Math.Sin(a)), (float)(eye.y + height), (float)(eye.z + horiz * Math.Cos(a))),
                    Strength = 1f,
                    T0 = BwClock.Ambient + 0.5,
                    Duration = (float)KrakenRecipe.DefaultDuration[kind],
                    Underwater = underwater,
                };
                KrakenCalls.Publish(in c);
                m_ownSerial = KrakenCalls.Serial;
                Debug.Log("[BW] audio-qa: kraken " + c.Kind + " at t=" + t.ToString("0.00", inv) + " s (ambient " + c.T0.ToString("0.00", inv)
                          + "), d " + dist.ToString("0", inv) + " m, bearing " + bearing.ToString("0", inv) + ", underwater " + underwater);
            }

            if (m_ride)
            {
                var r = BulwarkScore.SyntheticRide(t - m_rideT0);
                var prev = EndingRide.Phase;
                EndingRide.Set((EndingRidePhase)r.Phase, r.Progress, r.Altitude, r.Distance, r.Speed);
                if ((int)prev != r.Phase)
                    Debug.Log("[BW] audio-qa: ride phase " + (EndingRidePhase)r.Phase + " at t=" + t.ToString("0.00", inv) + " s");
                // the stinger on the platform, 8 s after the arrival: far behind the dam face (3.5 km east, 2 km up)
                if (!m_stinger && t - m_rideT0 >= BulwarkScore.SyntheticRideArrival + 8.0)
                {
                    m_stinger = true;
                    Listener(out Vector3 eye, out _);
                    var c = new KrakenCall
                    {
                        Kind = KrakenCallKind.Stinger, Position = new Vector3(eye.x + 3500f, eye.y + 2000f, eye.z), Strength = 1f,
                        T0 = BwClock.Ambient + 0.5, Duration = (float)KrakenRecipe.DefaultDuration[KrakenRecipe.Stinger], Underwater = false,
                    };
                    KrakenCalls.Publish(in c);
                    Debug.Log("[BW] audio-qa: kraken Stinger on the platform at t=" + t.ToString("0.00", inv) + " s");
                }
            }
        }

        static void Listener(out Vector3 eye, out float yaw)
        {
            var cc = BwServices.CameraControl;
            var cam = cc != null ? cc.Camera : Camera.main;
            if (cam != null) { eye = cam.transform.position; yaw = cam.transform.eulerAngles.y; return; }
            var p = BwServices.Player;
            eye = p != null ? p.EyePosition : Vector3.zero;
            yaw = (float)BwPath.HeadingDeg(GameState.S);
        }

        void OnDestroy()
        {
            if (m_ride) EndingRide.Reset();
        }
    }
}
