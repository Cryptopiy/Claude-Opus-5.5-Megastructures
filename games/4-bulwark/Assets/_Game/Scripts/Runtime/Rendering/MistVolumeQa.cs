// MistVolumeQa: a QA-ONLY writer of the sea-disturbance bus (Core/SeaDisturbances), so the mist's kraken sprays (Addendum
// A1: Breach, TentacleRise, Slap) can be built, captured and measured before the kraken choreography exists. It writes ONLY
// when the player is started with `-mist-qa-sea <mode>` (never in normal play, never with the kraken's own writer: the bus
// has one writer, KrakenDirector; this flag is for the mist owner's proof renders while the kraken is not merged).
// Called from MistVolumeState.CommitSprays (once per tick and camera, idempotent at one clock value).
// Modes:
//   `static` / `cycle`: three stand-ins east of the KS04 framing (eye (4.2, 1.7, 160), yaw 80, pitch -8, vFOV 64):
//     Breach (417, 233) 45 x 30 m, heading 20 deg, up to 55 m; TentacleRise (149, 228) r 3.5 m, up to 38 m; Slap (243, 139)
//     30 x 6 m. `static`: all at strength 1 and full height; `cycle`: a 30 s loop on the ambient clock from `-mist-qa-t0`
//     (default 64.0 s): breach in over 0.5-2.5 s, height 0 -> 55 m over 0.5-3.5 s, out 8-10 s / sinking 8-11 s; tentacle
//     12-24 s (0 -> 38 m over 12-17 s, down 21-24 s); slaps of 0.6 s at 5, 15 and 25 s.
//   `k1`, `k2`, `k4`, `k5`, `k45` (K4 and K5 overlap at s 812-830), `kall` (Wave X): the Addendum A1 choreography table at its key-shot moments (1.6), so the
//     sprays are judged at KS04 (K1), KK1 (K2), KK2 (K4) and KS11 (K5) with the real geometry:
//     K1 Breach (1400, 600) 75 x 60 m, heading 250, +70 m; K2 TentacleRise (330, 690) r 7.25, heading 315, 180 m;
//     K4 Breach (500, 1120) 90 x 80 m, +178 m, TentacleRise (300, 880) and (225, 790) r 8, 80 m, (430, 1150) r 10, 120 m;
//     K5 Slap as the show publishes it: (213.7, 991.0) 25 x 25 m, heading 60, 50 m, strength 0.687 (KS11, s 824).
//   `kcycle`: a 12 s loop from `-mist-qa-t0` of the K5 moment (the whip TentacleRise (160, 960) r 1.3 rising 0 -> 60 m over
//     0-3 s, the slap at 4 s: strength attack 0.2 s, decay exp(-(t - 4) / 1.5)) for the frame sequence seq_mist_k5_cycle
//     (Beat03 MistQaSet), plus the K2 arm held at 180 m.
// `-mist-qa-cascade C` (0..1, default 0) forces the cascade activity (water sheeting off a moving body) in stills, whose
// frozen clock gives the followers no vertical speed.
using System;
using UnityEngine;

namespace Megastructure.Rendering
{
    public static class MistVolumeQa
    {
        public const double CyclePeriod = 30.0, DefaultT0 = 64.0;
        public static readonly Vector2 BreachXZ = new Vector2(417f, 233f), TentacleXZ = new Vector2(149f, 228f), SlapXZ = new Vector2(243f, 139f);
        public const float BreachHalfAlong = 45f, BreachHalfAcross = 30f, BreachHeight = 55f, BreachHeadingDeg = 20f;
        public const float TentacleRadius = 3.5f, TentacleHeight = 38f;
        public const float SlapHalfAlong = 30f, SlapHalfAcross = 6f;

        // Addendum A1 1.6 presets (world x, z; half extents; heading deg; height above the sea)
        static readonly SeaDisturbance K1 = Make(SeaDisturbanceKind.Breach, 1400f, 600f, 75f, 60f, 250f, 70f);
        static readonly SeaDisturbance K2 = Make(SeaDisturbanceKind.TentacleRise, 330f, 690f, 7.25f, 7.25f, 315f, 180f);
        static readonly SeaDisturbance K4 = Make(SeaDisturbanceKind.Breach, 500f, 1120f, 90f, 80f, 236f, 178f);
        static readonly SeaDisturbance K4a = Make(SeaDisturbanceKind.TentacleRise, 300f, 880f, 8f, 8f, 0f, 80f);
        static readonly SeaDisturbance K4b = Make(SeaDisturbanceKind.TentacleRise, 225f, 790f, 8f, 8f, 0f, 80f);
        static readonly SeaDisturbance K4c = Make(SeaDisturbanceKind.TentacleRise, 430f, 1150f, 10f, 10f, 0f, 120f);
        /// <summary>K5 as the kraken show publishes it (Wave X, KrakenDirectorChoreo K5: the whip from (160, 960) topples toward 60
        /// deg; the Slap is centred 62 m along that line, 25 x 25 m, heading 60, HeightAboveSea 50, strength SlapEnvelope = 0.687
        /// at KS11, 0.8 s after the impact at s 822).</summary>
        static readonly SeaDisturbance K5 = MakeS(SeaDisturbanceKind.Slap, 160f + 62f * 0.8660254f, 960f + 62f * 0.5f, 25f, 25f, 60f, 50f, 0.687f);
        static readonly Vector2 K5Whip = new Vector2(160f, 960f);
        static SeaDisturbance MakeS(SeaDisturbanceKind k, float x, float z, float ha, float hc, float headingDeg, float h, float strength)
        { var d = Make(k, x, z, ha, hc, headingDeg, h); d.Strength = strength; return d; }

        static SeaDisturbance Make(SeaDisturbanceKind k, float x, float z, float ha, float hc, float headingDeg, float h)
            => new SeaDisturbance { Kind = k, X = x, Z = z, HalfAlong = ha, HalfAcross = hc, Heading = headingDeg * Mathf.Deg2Rad, Strength = 1f, HeightAboveSea = h };

        static bool s_read;
        static int s_mode;            // 0 off, 1 static, 2 cycle, 3 k1, 4 k2, 5 k4, 6 k5, 7 kall, 8 kcycle, 9 k45
        public const double KCyclePeriod = 12.0;
        static double s_t0 = DefaultT0;
        static double s_cascade;

        /// <summary>The writer is on (-mist-qa-sea static|cycle|k1|k2|k4|k5|kall).</summary>
        public static bool Active { get { Read(); return s_mode != 0; } }
        public static bool Cycle { get { Read(); return s_mode == 2; } }
        /// <summary>Forced cascade activity (-mist-qa-cascade, 0 without the writer).</summary>
        public static double ForcedCascade { get { Read(); return s_mode != 0 ? s_cascade : 0.0; } }

        static void Read()
        {
            if (s_read) return;
            s_read = true;
            string m = CliArgs.Has("-mist-qa-sea") ? CliArgs.Get("-mist-qa-sea", "static") : null;
            s_mode = 0;
            if (m != null)
            {
                switch (m.ToLowerInvariant())
                {
                    case "cycle": s_mode = 2; break;
                    case "k1": s_mode = 3; break;
                    case "k2": s_mode = 4; break;
                    case "k4": s_mode = 5; break;
                    case "k5": s_mode = 6; break;
                    case "kall": s_mode = 7; break;
                    case "k45": s_mode = 9; break;
                    case "kcycle": s_mode = 8; break;
                    default: s_mode = 1; break;
                }
            }
            s_t0 = CliArgs.GetDouble("-mist-qa-t0", DefaultT0);
            s_cascade = MistVolumeMath.Clamp01(CliArgs.GetDouble("-mist-qa-cascade", 0.0));
            if (s_mode != 0) Debug.Log($"[BW] MistVolumeQa: QA-only sea-disturbance writer on (mode {m}, t0 {s_t0:F1} s, cascade {s_cascade:F2})");
        }

        /// <summary>Tests: re-read the command line.</summary>
        public static void ResetCli() { s_read = false; }

        static double Smooth(double a, double b, double x) => MistVolumeMath.SmoothStep(a, b, x);

        /// <summary>Writes the bus for ambient time t (Begin, Set..., Publish).</summary>
        public static void Write(double t)
        {
            if (!Active) return;
            if (s_mode == 8)
            {
                double ph = t - s_t0;
                ph -= KCyclePeriod * Math.Floor(ph / KCyclePeriod);
                var whip = K5; whip.Kind = SeaDisturbanceKind.TentacleRise; whip.HalfAlong = whip.HalfAcross = 1.3f; whip.Heading = 0f;
                whip.X = K5Whip.x; whip.Z = K5Whip.y;
                whip.HeightAboveSea = (float)(60.0 * Smooth(0.0, 3.0, ph));
                whip.Strength = (float)(Smooth(0.0, 0.5, ph) * (1.0 - Smooth(3.8, 4.0, ph)));
                var slap = K5;
                // the show's SlapEnvelope: attack 0.2 s, then exp(-(dt - 0.2) / 1.6), gone over 3.0-4.2 s
                double dts = ph - 4.0;
                slap.Strength = dts < 0.0 ? 0f : (float)(dts < 0.2 ? dts / 0.2 : Math.Exp(-(dts - 0.2) / 1.6) * (1.0 - Smooth(3.0, 4.2, dts)));
                SeaDisturbances.Begin();
                SeaDisturbances.Set(K2);
                SeaDisturbances.Set(whip);
                SeaDisturbances.Set(slap);
                SeaDisturbances.Publish();
                return;
            }
            if (s_mode >= 3)
            {
                SeaDisturbances.Begin();
                if (s_mode == 3 || s_mode == 7) SeaDisturbances.Set(K1);
                if (s_mode == 4 || s_mode == 7) SeaDisturbances.Set(K2);
                if (s_mode == 5 || s_mode == 7 || s_mode == 9) { SeaDisturbances.Set(K4); SeaDisturbances.Set(K4a); SeaDisturbances.Set(K4b); SeaDisturbances.Set(K4c); }
                if (s_mode == 6 || s_mode == 7 || s_mode == 9) SeaDisturbances.Set(K5);
                SeaDisturbances.Publish();
                return;
            }
            float bStr = 1f, bH = BreachHeight, tStr = 1f, tH = TentacleHeight, sStr = 1f;
            if (s_mode == 2)
            {
                double ph = t - s_t0;
                ph -= CyclePeriod * Math.Floor(ph / CyclePeriod);
                bStr = (float)(Smooth(0.5, 2.5, ph) * (1.0 - Smooth(8.0, 10.0, ph)));
                bH = (float)(BreachHeight * Smooth(0.5, 3.5, ph) * (1.0 - Smooth(8.0, 11.0, ph)));
                tStr = (float)(Smooth(12.0, 13.0, ph) * (1.0 - Smooth(23.0, 24.0, ph)));
                tH = (float)(TentacleHeight * Smooth(12.0, 17.0, ph) * (1.0 - Smooth(21.0, 24.0, ph)));
                sStr = 0f;
                for (int k = 0; k < 3; k++)
                {
                    double c = 5.0 + 10.0 * k;
                    sStr = Mathf.Max(sStr, (float)(Smooth(c - 0.3, c, ph) * (1.0 - Smooth(c, c + 0.3, ph))));
                }
            }
            SeaDisturbances.Begin();
            SeaDisturbances.Set(new SeaDisturbance
            {
                Kind = SeaDisturbanceKind.Breach, X = BreachXZ.x, Z = BreachXZ.y, HalfAlong = BreachHalfAlong, HalfAcross = BreachHalfAcross,
                Heading = BreachHeadingDeg * Mathf.Deg2Rad, Strength = bStr, HeightAboveSea = bH,
            });
            SeaDisturbances.Set(new SeaDisturbance
            {
                Kind = SeaDisturbanceKind.TentacleRise, X = TentacleXZ.x, Z = TentacleXZ.y, HalfAlong = TentacleRadius, HalfAcross = TentacleRadius,
                Heading = 0f, Strength = tStr, HeightAboveSea = tH,
            });
            SeaDisturbances.Set(new SeaDisturbance
            {
                Kind = SeaDisturbanceKind.Slap, X = SlapXZ.x, Z = SlapXZ.y, HalfAlong = SlapHalfAlong, HalfAcross = SlapHalfAcross,
                Heading = 0f, Strength = sStr, HeightAboveSea = 0f,
            });
            SeaDisturbances.Publish();
        }
    }
}
