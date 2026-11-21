// BulwarkScore: the procedural score of Bulwark as PURE functions of (s, t, speed) and the ending ride state (MASTER_PROMPT
// 14.3, 14.6, 14.11; ADDENDUM_A1 section 2 and LEAD DECISION 1; lessons 3.2 #14: the score assumes no walking speed,
// everything is keyed to s). Pattern of Thread's ThreadAudioParams (a parameter struct computed per frame on the main thread
// and per block offline).
//
// Layers (14.3 table, levels in dBFS RMS; BwBeds generators are unit-RMS so a level is exactly 20 log10(gain)):
//   W wind  B2 -30, B3 -29, B4 -27, B5 -50 (LP 400 Hz), B7 -36 (from the tunnel mouth); none in beats 1 and 6
//   S sea   B1 -50 (LP 250 Hz), B2 -26, B3 -31, B4 -25, B5 -39, B6 -47, B7 -40 tunnel / -46 ledge
//   B boom  the impact bus (SeaState impacts): trims per beat for B1 -35, B2 -33, B3 -40, B4 -34, B5 -36, B6 -45 (sub only),
//           B7 -50; S1: no booms in s 100-108 (the last at 100)
//   F falls B2 -42 rising toward the spillway (S3 s 380-520), B3 -19 (+-1.5 dB as the slots pass under the bridge), B4 -40,
//           B5 -48, B7 -36 tunnel / -41 ledge
//   T room  B1 -38 (brown LP 160 Hz; 14.3 -40 + 2 dB, see the boom note); B5 ballast sines 100 + 200 Hz -36
//   P       pressure B5 -46, B6 -39, B7 -45;  C conduit: tunnel -26 (bell over s 1380-1423);  city hum on the ledge -52
// Silences and swells (keyed to s, so identical at every speed): S1 s 96-108 no booms (impactTrim); S2 door trigger s 104:
// wind and sea rise from -60 dBFS to the beat 2 levels by s 111.5 (3.0 s at 2.5 m/s) and the sea opens from LP 250 to 700 Hz;
// S3 the fall roar swells over s 380-435; S4 portal s 920: all -20 dB within 1.5 m (0.6 s at 2.5 m/s); S5 hall entry s 1070:
// all but P and B below -50 dBFS for 20 m (8 s at 2.5 m/s); S7 mouth s 1423: C -5 and F -6 dB within 3.75 m (1.5 s), then C
// fades out by s 1468 so the master follows its 14.3 ramp (-25 at s 1423 to -35 by s 1470; the -14 dB step of S7 would put
// the ledge 6 dB under that ramp), wind and hum rise; S8 ending: the master fades with the picture, the lantern is silent.
// Ledge levels: W -38 (14.3 B7 -36 trimmed so the ledge sums to the -35 master target), F -43.
// Levels are linear interpolations in dB between keys (floor -180 dB, slopes <= 48 dB/m, so AudioTests.Params_AreContinuousInS
// holds); keys at boundary +-15 m are the 30 m Rule-P crossfades of 14.6. Per-beat trims (BeatTrimDb) are the T-AUDIO
// calibration of the whole mix (footsteps and reverb included), measured with OfflineAudioRenderer.
// The ending ride (LEAD DECISION 1; RideSound): Boarding muffles the ledge (-8 dB) as the doors close; the descent replaces
// the beds by the ride layers, closes the "ear" low-pass from open to 2.5 kHz as the cabin sinks and raises the outlet jets;
// the coupling at the foot pops the ear open; the shuttle runs with the rising whine while the jets recede; on arrival the
// ride layers fade over the first 15 % of the phase and the platform keeps only the foot ambience (-46 dBFS: the silence).
using System;

namespace Megastructure.Audio
{
    /// <summary>The ending ride as the score sees it (from EndingRide on the main thread, or a synthetic profile offline).</summary>
    public struct BwRideState
    {
        public int Phase;              // EndingRidePhase as int (0 = none)
        public float Progress;         // 0..1 within the phase
        public float Altitude;         // eye altitude above the lowland (m)
        public float Distance;         // horizontal distance from the lee face (m)
        public float Speed;            // m/s
        public float Accel;            // signed m/s^2 (AudioDirector differentiates the speed)
        public static BwRideState None => new BwRideState { Phase = 0, Altitude = 6000f };
    }

    public static class BulwarkScore
    {
        /// <summary>14.11: the first wave impact of the intro (at FrameworkConst.IntroImpactTime = 0.5 s, in the black): full
        /// strength, from the sea side (east = right while walking north).</summary>
        public const float IntroImpactStrength = 1f, IntroImpactPan = 0.3f;

        public const double Off = -180.0;
        /// <summary>Top of the descent (eye altitude of the ledge above the lowland, m) for the ear and the jets.</summary>
        public const double RideTopAltitude = 5910.0;

        /// <summary>14.3 B: beat 6 hears only the 31 Hz sub of an impact.</summary>
        public static bool ImpactSubOnly(double s) => BwPath.BeatAt(s) == 6;

        /// <summary>True while the listener is inside the concrete (the gallery shed to the door plane at s 120, the tower and
        /// the stairs s 920-1070, the window gallery, the sluice tunnel to the mouth at s 1422.85): kraken calls from outside are
        /// heard through the wall (KrakenBus wall variant).</summary>
        public static bool ListenerIndoors(double s) => s < 120.0 || (s >= 920.0 && s < 1422.85);

        // ------------------------------------------------------------------------------------------------- layer tracks (s, dB)
        public const int LayerWind = 0, LayerSea = 1, LayerFalls = 2, LayerRoom = 3, LayerBallast = 4, LayerPressure = 5, LayerConduit = 6, LayerCity = 7, LayerCount = 8;
        public static readonly string[] LayerNames = { "W wind", "S sea", "F falls", "T room", "T ballast", "P pressure", "C conduit", "city hum" };

        static readonly double[] s_wind =
        {
            0, Off, 101.5, Off, 104, -60, 111.5, -30, 405, -30, 435, -29, 685, -29, 715, -27, 918.5, -27, 920, -47, 935, -50,
            1055, -50, 1070, -70, 1072.5, Off, 1365, Off, 1370, -120, 1385, -55, 1423, -55, 1427, -38, 1500, -38,
        };
        static readonly double[] s_sea =
        {
            0, -50, 104, -50, 111.5, -26, 405, -26, 435, -31, 685, -31, 715, -25, 918.5, -25, 920, -45, 926, -39, 1055, -39,
            1070, -56, 1090, -56, 1095, -47, 1355, -47, 1385, -40, 1423, -40, 1427, -46, 1500, -46,
        };
        static readonly double[] s_falls =
        {
            0, Off, 101, Off, 104, -90, 111.5, -44, 380, -36, 405, -33, 435, -19, 685, -19, 700, -26, 715, -33, 740, -40, 918.5, -40, 920, -60,
            935, -48, 1055, -48, 1070, -90, 1072, Off, 1368, Off, 1370, -90, 1380, -36, 1423, -36, 1427, -42, 1440, -43, 1500, -43,
        };
        static readonly double[] s_room = { 0, -38, 104, -38, 120, -58, 122.5, Off, 1500, Off };
        static readonly double[] s_ballast = { 0, Off, 915, Off, 920, -90, 926, -36, 1055, -36, 1070, -60, 1072, Off, 1500, Off };
        static readonly double[] s_pressure = { 0, Off, 915, Off, 920, -90, 926, -46, 1055, -46, 1085, -39, 1355, -39, 1385, -45, 1500, -45 };
        static readonly double[] s_conduit = { 0, Off, 1365, Off, 1370, -90, 1385, -26, 1423, -26, 1427, -30, 1450, -38, 1465, -48, 1468, Off, 1500, Off };
        static readonly double[] s_city = { 0, Off, 1418, Off, 1423, -90, 1427, -52, 1500, -52 };
        static readonly double[][] s_tracks = { s_wind, s_sea, s_falls, s_room, s_ballast, s_pressure, s_conduit, s_city };

        static readonly double[] s_seaLp = { 0, 250, 104, 250, 111.5, 700, 905, 700, 935, 400, 1055, 400, 1085, 300, 1355, 300, 1385, 500, 1423, 500, 1427, 700, 1500, 700 };
        static readonly double[] s_seaPan = { 0, 0.3, 905, 0.4, 935, 0.15, 1385, 0.15, 1427, 0.0, 1500, 0.0 };
        static readonly double[] s_windPan = { 0, 0.0, 120, 0.0, 135, 0.35, 905, 0.35, 935, 0.0, 1427, -0.15, 1500, -0.15 };
        static readonly double[] s_windLp = { 0, 20000, 905, 20000, 935, 400, 1055, 400, 1085, 20000, 1500, 20000 };
        /// <summary>Impact bus trim (dB) relative to the BwAudioSpec boom (estimated -38 dBFS RMS at a 9 s interval and mean
        /// strength 0.65; -36.2 at the 6 s interval of beat 4): B2 -33 (+5), B3 -40 (-2), B4 -34 (+2), B5 -36 (+2), B6 -45 (the sub
        /// only, -7), B7 -50 (-12); S1 no booms in s 100-108. Beat 1 +6: the offline render of the gallery (SeaState lattice,
        /// s 2-97) measured the booms 4-5 dB under the estimate (the lattice's mean strength is 0.58), so the gallery reaches its
        /// -34 target with booms about -37.7 and the room tone at -38.</summary>
        static readonly double[] s_impact = { 0, 6, 99, 6, 100, -60, 108, -60, 110, 5, 405, 5, 435, -2, 685, -2, 715, 2, 935, 2, 1055, 2, 1085, -7, 1355, -7, 1385, -12, 1500, -12 };

        /// <summary>T-AUDIO calibration per beat (dB on every bed layer; OfflineAudioRenderer at 2.5 m/s, footsteps and reverb
        /// included), crossfaded over the 30 m Rule-P windows.</summary>
        public static readonly double[] BeatTrimDb = { 0.0, 1.6, 1.1, 1.0, 1.4, 0.8, 0.0 };

        /// <summary>Linear interpolation of flat (s, value) keys; clamped at both ends.</summary>
        public static double Track(double[] k, double s)
        {
            int n = k.Length / 2;
            if (s <= k[0]) return k[1];
            for (int i = 1; i < n; i++)
            {
                double s1 = k[2 * i];
                if (s <= s1)
                {
                    double s0 = k[2 * i - 2];
                    double u = s1 > s0 ? (s - s0) / (s1 - s0) : 1.0;
                    return k[2 * i - 1] + (k[2 * i + 1] - k[2 * i - 1]) * u;
                }
            }
            return k[2 * n - 1];
        }

        /// <summary>Level of layer (dBFS RMS, before the beat trim) at s.</summary>
        public static double LayerDb(int layer, double s)
        {
            double db = Track(s_tracks[layer], s);
            // the spillway slots pass under the bridge every 20 m (+-1.5 dB, faded in and out over the Rule-P windows)
            if (layer == LayerFalls && s > 420.0 && s < 700.0)
                db += 1.5 * Math.Cos(2.0 * Math.PI * (s - 432.0) / 20.0) * BwMath.Smoothstep(420.0, 435.0, s) * (1.0 - BwMath.Smoothstep(685.0, 700.0, s));
            return db;
        }

        /// <summary>dB to linear; the -180 dB floor stays 1e-9 (no jump to an exact 0: AudioTests.Params_AreContinuousInS; BwBeds
        /// skips any layer under 1e-6).</summary>
        static float Gain(double db) => (float)Math.Pow(10.0, Math.Max(Off, db) / 20.0);

        /// <summary>Score parameters at progress s, ambient time t (s) and walking speed (m/s). master / impactBus / duck and the
        /// listener pose are filled by the caller (1 here: bus open).</summary>
        public static BwAudioParams Evaluate(double s, double t, double speed) => Evaluate(s, t, speed, BwRideState.None);

        public static BwAudioParams Evaluate(double s, double t, double speed, in BwRideState ride)
        {
            var p = BwAudioParams.Silent();
            p.s = s; p.speed = speed;
            p.master = 1f; p.duck = 1f; p.impactBus = 1f;
            int beat = Math.Max(1, Math.Min(FrameworkConst.BeatCount, BwPath.BeatAt(s))) - 1;
            double trim = Blend(s, b => BeatTrimDb[Math.Min(b, BeatTrimDb.Length - 1)]);   // lambdas, not method groups: C# 9 caches only the lambdas; clamped if beats are added

            // ---- the ending ride: how much of the walking world is left (doors closed, the beds replaced by the ride layers)
            double world = 1.0, rideMix = 0.0, ear = 20000.0, foot = 0.0, jetsDb = Off;
            float rp = (float)BwMath.Clamp01(ride.Progress);
            double altN = BwMath.Clamp01(ride.Altitude / RideTopAltitude);
            switch (ride.Phase)
            {
                case 1:   // boarding: the doors close, the ledge is muffled
                    world = BwAudioSpec.Db(-8.0 * BwMath.Smoothstep(0.0, 0.4, rp));
                    rideMix = 0.4 * BwMath.Smoothstep(0.0, 0.5, rp);
                    break;
                case 2:   // descent: the world through the glass (-20 dB), the ear closes as the air thickens, the jets grow
                    world = BwAudioSpec.Db(-8.0 - 12.0 * BwMath.Smoothstep(0.0, 0.1, rp));
                    rideMix = 0.4 + 0.6 * BwMath.Smoothstep(0.0, 0.08, rp);
                    ear = 2500.0 + (20000.0 - 2500.0) * Math.Pow(altN, 1.5);
                    jetsDb = -66.0 + 24.0 * (1.0 - altN);
                    break;
                case 3:   // transfer at the foot: the coupling, the ear pops open (BwDsp smooths it over 0.4 s)
                    world = BwAudioSpec.Db(-20.0);
                    rideMix = 0.55;
                    jetsDb = -44.0;
                    foot = 0.3;
                    break;
                case 4:   // the shuttle west: the rising whine, the jets recede
                    world = BwAudioSpec.Db(-20.0);
                    rideMix = 1.0;
                    jetsDb = -44.0 - 8.0 * BwMath.Clamp01(ride.Distance / BulwarkConst.ShuttleDistance);
                    foot = 0.3;
                    break;
                case 5:   // arrived: doors open, the ride falls silent, the platform keeps its quiet
                    world = BwAudioSpec.Db(-20.0) * (1.0 - BwMath.Smoothstep(0.0, 0.15, rp));
                    rideMix = 0.55 * (1.0 - BwMath.Smoothstep(0.0, 0.15, rp));
                    jetsDb = -52.0;
                    foot = 0.3 + 0.7 * BwMath.Smoothstep(0.0, 0.1, rp);
                    break;
            }
            float wg = (float)(world * BwAudioSpec.Db(trim));
            p.w = Gain(LayerDb(LayerWind, s)) * wg;
            p.sea = Gain(LayerDb(LayerSea, s)) * wg;
            p.falls = Gain(LayerDb(LayerFalls, s)) * wg;
            p.bed = Gain(LayerDb(LayerRoom, s)) * wg;
            p.tonal = Gain(LayerDb(LayerBallast, s)) * wg;
            p.pad = Gain(LayerDb(LayerPressure, s)) * wg;
            p.conduit = Gain(LayerDb(LayerConduit, s)) * wg;
            p.voice = Gain(LayerDb(LayerCity, s)) * wg;
            p.windHz = 380f;
            p.gust = 0.5f + 0.5f * (float)Math.Sin(2.0 * Math.PI * (t / 13.0));
            p.windLp = (float)Track(s_windLp, s);
            p.windPan = (float)Track(s_windPan, s);
            p.seaLp = (float)Track(s_seaLp, s);
            p.seaPan = (float)Track(s_seaPan, s);
            p.impactTrim = (float)(BwAudioSpec.Db(Track(s_impact, s)) * world);

            p.ridePhase = ride.Phase;
            p.rideMix = (float)rideMix;
            p.rideSpeed = ride.Speed;
            p.rideAccel = ride.Accel;
            p.rideAlt = ride.Altitude;
            p.rideDist = ride.Distance;
            p.earHz = (float)ear;
            p.foot = (float)foot;
            p.jets = jetsDb <= Off ? 0f : Gain(jetsDb);
            // the bed level the kraken must clear (KrakenBus audibility floor): power sum of the bed layers and the ride
            double pw = (double)p.w * p.w + (double)p.sea * p.sea + (double)p.falls * p.falls + (double)p.bed * p.bed + (double)p.tonal * p.tonal
                      + (double)p.pad * p.pad + (double)p.conduit * p.conduit + (double)p.voice * p.voice;
            pw += rideMix * rideMix * BwAudioSpec.Db(2.0 * -33.5) + foot * foot * BwAudioSpec.Db(2.0 * -46.0);
            p.bedDb = (float)(10.0 * Math.Log10(Math.Max(1e-12, pw)));

            // reverb rows (BulwarkConst.ReverbDecay, 14.3): T60 by beat, crossfaded by s; the cabin of the ride 0.4 s (2.8 RC)
            p.rt60 = (float)Blend(s, b => BulwarkConst.ReverbDecay[b]);
            p.preDelayMs = (float)Blend(s, b => Outdoors(b) ? 4.0 : 18.0);
            p.wetDb = (float)Blend(s, b => Outdoors(b) ? -24.0 : -12.0);
            p.dampHz = (float)Blend(s, b => Outdoors(b) ? 9000.0 : 5000.0);
            p.blendSeconds = (float)(beat == 4 ? BwAudioSpec.ReverbBlendHardEdge : BwAudioSpec.ReverbBlendSeconds);
            if (ride.Phase >= 1 && ride.Phase <= 4) { p.rt60 = 0.4f; p.preDelayMs = 3f; p.wetDb = -16f; p.dampHz = 6000f; p.blendSeconds = 1.5f; }
            else if (ride.Phase == 5) { p.rt60 = 0.5f; p.preDelayMs = 6f; p.wetDb = -24f; p.dampHz = 7000f; p.blendSeconds = 2f; }
            return p;
        }

        static bool Outdoors(int beat) => beat >= 1 && beat <= 3 || beat == 6;

        /// <summary>Per-beat value v(beat) crossfaded with smootherstep over BulwarkConst.TransitionWindow (30 m) centred
        /// on every beat boundary. Pure; no allocation (the delegates are cached static lambdas).</summary>
        static double Blend(double s, Func<int, double> v)
        {
            double half = 0.5 * BulwarkConst.TransitionWindow;
            double value = v(0);
            for (int k = 1; k < FrameworkConst.BeatCount; k++)
            {
                double b = BulwarkConst.BeatS[k];
                double w = BwMath.Smootherstep01((s - (b - half)) / (2.0 * half));
                if (w <= 0.0) break;
                value += (v(k) - v(k - 1)) * w;
            }
            return value;
        }

        /// <summary>
        /// The ride state the player's EndingRide writer publishes (PlayerController: EndingPath.PhaseAt, Cabin, Altitude,
        /// DistanceFromFace), as a pure function of the ride clock tr (seconds since the walker came to rest in the cabin, s 1500):
        /// the REAL profile (fix wave S3-3 / R1: the descent and the shuttle follow the ride constants of BulwarkConst, so the
        /// 55 s shuttle of R1 is heard as soon as the ending builder sets them), for QA, -audio-qa ride and the offline ride
        /// render. Accel is the closed-form profile acceleration (AudioDirector differentiates EndingRide.Speed in the player).
        /// </summary>
        public static BwRideState SyntheticRide(double tr)
        {
            var r = new BwRideState { Altitude = (float)RideTopAltitude };
            if (tr <= 0.0) { r.Phase = 0; return r; }
            double s = EndingPath.SOfTau(tr);
            var ph = EndingPath.PhaseAt(s, out double prog);
            r.Phase = (int)ph; r.Progress = (float)prog;
            EndingPath.Cabin(s, out double x, out double y, out _, out double vx, out double vy);
            r.Altitude = (float)EndingPath.Altitude(y + BulwarkConst.Eye);
            r.Distance = (float)EndingPath.DistanceFromFace(x);
            r.Speed = (float)Math.Sqrt(vx * vx + vy * vy);
            double t = EndingPath.TauL(s);
            if (ph == EndingRidePhase.Descent)
                r.Accel = (float)EndingPath.ProfileAccel(t - EndingPath.TauDescent0, EndingPath.DescentLength, BulwarkConst.RideDescentTime, EndingPath.DescentRamp);
            else if (ph == EndingRidePhase.Shuttle)
                r.Accel = (float)EndingPath.ProfileAccel(t - EndingPath.TauTransfer1, BulwarkConst.ShuttleDistance, BulwarkConst.RideShuttleTime, BulwarkConst.ShuttleRampTime);
            return r;
        }

        /// <summary>Ride seconds up to the arrival (the doors at the lantern platform): EndingPath, so it follows R1.</summary>
        public static double SyntheticRideArrival => EndingPath.TauShuttle1;
    }
}
