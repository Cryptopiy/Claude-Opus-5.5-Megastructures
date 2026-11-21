// SeaDisturbanceQa: a QA-only writer of the SeaDisturbances bus (Addendum A1), sea owner. Off unless the player runs with
// "-sea-dist-qa <scenario>" (near | far | kraken); the real writer is the kraken choreography (next wave), which must never run together with
// this one (one writer per frame). It proves BwSea's reading of every kind at fixed world positions chosen for the capture
// poses below; strengths and the Slap envelope are functions of the ambient clock only, so frozen-clock stills reproduce.
// Scenarios (8 entries at most):
//   near  KS04 framing (eye (4.2, 1.7, 160), yaw 80): Shadow 84 x 40 m at 66 m, TentacleRise at 43 m, Slap at 58 m,
//         Breach at 180 m, Wake at 300 m; KS06 framing (eye (-1.5, 1.7, 300), yaw 0): Breach, TentacleRise, Wake beyond the
//         parapet's shadow line (x > 120).
//   far   from the KS04 eye, at the SIZES of Addendum A1 1.6 (Breach K1 75 x 60 m, TentacleRise K2 r 7.25 m, Shadow K3
//         160 x 70 m with its Wake 110 x 60 m at 0.6, Slap K5 25 m): Shadow + Wake, Breach, TentacleRise at 800 m (az 100, 84,
//         72) and Breach, TentacleRise, Slap, Shadow at 2 km (az 90, 78, 84, 96; beyond the 2048 m clipmap edge on the far
//         side, drawn by the disturbance ring of SeaClipmap).
//   kraken  the real A1 1.6 placements: K1 Breach (1400, 600) (KS04, 1464 m), K2 TentacleRise (330, 690) (s 378, 455 m),
//         K3 Shadow + Wake on the K3 path at the gate hold (231, 492) (KS07, about 230 m), K4 Breach (500, 1120) with its
//         arms at (300, 880) and (225, 790), K5 Slap (160, 960).
// The Slap repeats every 6 s: attack 0.12 s, decay 3.5 s (the envelope contract BwSeaWaves.hlsl reads); in a still at
// clock t it shows the ring of its envelope at t. Capture example (run-player.ps1 -ArgString):
//   "-capture QA\captures\sea-NN -sea-dist-qa near -capture-only KS04,KS06,pose:dnear:4.2:1.7:160:80:-14:0"
using System;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed class SeaDisturbanceQa : IWorldUpdater
    {
        public const string Flag = "-sea-dist-qa";
        /// <summary>True when the player was started with the QA flag.</summary>
        public static bool Requested => CliArgs.Has(Flag);

        readonly SeaDisturbance[] m_items = new SeaDisturbance[SeaDisturbances.Max];
        readonly bool[] m_slap = new bool[SeaDisturbances.Max];
        int m_count;
        public string Scenario { get; }
        /// <summary>Entries of the scenario and entry i as written at strength 1 (tests).</summary>
        public int Count => m_count;
        public SeaDisturbance Item(int i) => m_items[i];

        public SeaDisturbanceQa(string scenario)
        {
            Scenario = string.IsNullOrEmpty(scenario) || scenario.StartsWith("-", StringComparison.Ordinal) ? "near" : scenario.ToLowerInvariant();
            if (Scenario == "far") BuildFar();
            else if (Scenario == "kraken") BuildKraken();
            else BuildNear();
            Debug.Log($"[BW] sea: QA disturbance writer on (scenario '{Scenario}', {m_count} entries); the kraken writer must be off");
        }

        void Add(SeaDisturbanceKind kind, double x, double z, double along, double across, double headingDeg, double strength, double height = 0.0)
        {
            if (m_count >= m_items.Length) return;
            m_slap[m_count] = kind == SeaDisturbanceKind.Slap;
            m_items[m_count++] = new SeaDisturbance
            {
                Kind = kind, X = (float)x, Z = (float)z, HalfAlong = (float)along, HalfAcross = (float)across,
                Heading = (float)(headingDeg * Math.PI / 180.0), Strength = (float)strength, HeightAboveSea = (float)height,
            };
        }

        /// <summary>A point at horizontal distance d and azimuth az (deg, 0 = +z, clockwise) from (x0, z0).</summary>
        static void At(double x0, double z0, double d, double azDeg, out double x, out double z)
        {
            double a = azDeg * Math.PI / 180.0;
            x = x0 + d * Math.Sin(a); z = z0 + d * Math.Cos(a);
        }

        void BuildNear()
        {
            const double ex = 4.2, ez = 160.0;   // KS04 eye
            At(ex, ez, 66.0, 80.0, out double x, out double z);
            Add(SeaDisturbanceKind.Shadow, x, z, 42.0, 20.0, 15.0, 1.0);
            At(ex, ez, 43.0, 60.0, out x, out z);
            Add(SeaDisturbanceKind.TentacleRise, x, z, 2.2, 2.2, 0.0, 1.0, 14.0);
            At(ex, ez, 58.0, 105.0, out x, out z);
            Add(SeaDisturbanceKind.Slap, x, z, 3.5, 3.5, 0.0, 1.0, 3.0);
            At(ex, ez, 180.0, 95.0, out x, out z);
            Add(SeaDisturbanceKind.Breach, x, z, 28.0, 20.0, 20.0, 1.0, 18.0);
            At(ex, ez, 300.0, 70.0, out x, out z);
            Add(SeaDisturbanceKind.Wake, x, z, 25.0, 9.0, 200.0, 1.0);
            // KS06 framing: beyond the line the parapet hides (x > 120 at az 30-45 from (-1.5, 300))
            Add(SeaDisturbanceKind.Breach, 190.0, 560.0, 30.0, 22.0, 0.0, 1.0, 20.0);
            Add(SeaDisturbanceKind.TentacleRise, 150.0, 470.0, 3.0, 3.0, 0.0, 1.0, 18.0);
            Add(SeaDisturbanceKind.Wake, 260.0, 760.0, 30.0, 10.0, 0.0, 1.0);
        }

        // Addendum A1 1.6 sizes (m) of the appearances the sea must make readable.
        public const double K1Along = 75.0, K1Across = 60.0, K1Height = 70.0;            // Breach "the islet that moves"
        public const double K2Radius = 7.25, K2Height = 180.0;                           // TentacleRise "taller than the tower"
        public const double K3Along = 160.0, K3Across = 70.0;                            // Shadow "the shadow at the gates"
        public const double K3WakeAlong = 110.0, K3WakeAcross = 60.0, K3WakeStrength = 0.6;
        public const double K5Along = 25.0;                                              // Slap

        void BuildFar()
        {
            const double ex = 4.2, ez = 160.0;   // KS04 eye
            At(ex, ez, 800.0, 100.0, out double x, out double z);
            Add(SeaDisturbanceKind.Shadow, x, z, K3Along, K3Across, 355.0, 1.0);
            Add(SeaDisturbanceKind.Wake, x, z, K3WakeAlong, K3WakeAcross, 355.0, K3WakeStrength);
            At(ex, ez, 800.0, 84.0, out x, out z);
            Add(SeaDisturbanceKind.Breach, x, z, K1Along, K1Across, 250.0, 1.0, K1Height);
            At(ex, ez, 800.0, 72.0, out x, out z);
            Add(SeaDisturbanceKind.TentacleRise, x, z, K2Radius, K2Radius, 315.0, 1.0, K2Height);
            At(ex, ez, 2000.0, 90.0, out x, out z);
            Add(SeaDisturbanceKind.Breach, x, z, K1Along, K1Across, 250.0, 1.0, K1Height);
            At(ex, ez, 2000.0, 78.0, out x, out z);
            Add(SeaDisturbanceKind.TentacleRise, x, z, K2Radius, K2Radius, 315.0, 1.0, K2Height);
            At(ex, ez, 2000.0, 84.0, out x, out z);
            Add(SeaDisturbanceKind.Slap, x, z, K5Along, K5Along, 0.0, 1.0, 20.0);
            At(ex, ez, 2000.0, 96.0, out x, out z);
            Add(SeaDisturbanceKind.Shadow, x, z, K3Along, K3Across, 355.0, 1.0);
        }

        void BuildKraken()
        {
            Add(SeaDisturbanceKind.Breach, 1400.0, 600.0, K1Along, K1Across, 250.0, 1.0, K1Height);          // K1 (KS04)
            Add(SeaDisturbanceKind.TentacleRise, 330.0, 690.0, K2Radius, K2Radius, 315.0, 1.0, K2Height);    // K2 (s 378)
            Add(SeaDisturbanceKind.Shadow, 231.0, 492.0, K3Along, K3Across, 355.0, 1.0);                     // K3 (KS07)
            Add(SeaDisturbanceKind.Wake, 231.0, 492.0, K3WakeAlong, K3WakeAcross, 355.0, K3WakeStrength);
            Add(SeaDisturbanceKind.Breach, 500.0, 1120.0, 90.0, 80.0, 236.0, 1.0, 178.0);                     // K4 head
            Add(SeaDisturbanceKind.TentacleRise, 300.0, 880.0, 8.0, 8.0, 225.0, 1.0, 80.0);                   // K4 draped arm
            Add(SeaDisturbanceKind.TentacleRise, 225.0, 790.0, 8.0, 8.0, 225.0, 1.0, 80.0);
            Add(SeaDisturbanceKind.Slap, 160.0, 960.0, K5Along, K5Along, 0.0, 1.0, 60.0);                    // K5
        }

        /// <summary>Phase of the Slap cycle (s): at the KS04 still's clock (s 160 / 2.5 = 64 s) the cycle is 1.7 s in, the
        /// envelope 0.55 and the ring 2.8 A out (mid-run).</summary>
        public const double SlapPhase = 3.7;

        /// <summary>Slap envelope at clock t: every 6 s an attack of 0.12 s and a decay of 3.5 s (smooth, from 0 to 0).</summary>
        public static double SlapEnvelope(double t)
        {
            double p = t - 6.0 * Math.Floor(t / 6.0);
            if (p < 0.12) { double a = p / 0.12; return a * a * (3.0 - 2.0 * a); }
            double d = (p - 0.12) / 3.5;
            if (d >= 1.0) return 0.0;
            double k = 1.0 - d;
            return k * k * (3.0 - 2.0 * k);
        }

        public void OnFrame(in FrameState f)
        {
            SeaDisturbances.Begin();
            for (int i = 0; i < m_count; i++)
            {
                SeaDisturbance d = m_items[i];
                if (m_slap[i]) d.Strength = (float)SlapEnvelope(f.ambientTime + SlapPhase);
                SeaDisturbances.Set(d);
            }
            SeaDisturbances.Publish();
        }
    }
}
