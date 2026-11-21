// GazeSchedule: the cinematic gaze keys of MASTER_PROMPT 14.5.1-14.5.7 ("Gaze" lines) as pure data + pure functions.
// Structure imported from The Ring's GazeSchedule (s-keyed keys, shortest-angle yaw; fix wave: a monotone cubic through the keys);
// The Ring's holds, steps, glances, sway overlay, Rel/F/A flags, heading look-ahead and lateral table are NOT imported
// (D-02): Bulwark walks at a constant 2.5 m/s with no holds; a "hold" is two equal keys while the feet keep moving.
// Keys are (s, WORLD yaw, pitch): yaw 0 = +Z (north), 90 = +X (east, the sea); pitch up positive. The lateral offset of the
// cinematic is BwPath.CinematicLateral(s) (path builder, 14.5.2-14.5.4 keys).
// The AutoWalker's WalkPlanner evaluates the schedule at s + lead (lead = (2 / omega) v, 2.0 m at 2.5 m/s) so the critically
// damped follower (omega 2.5) reaches each key at its s ("lead the cues", 3.1 b), then rate-limits to 18 / 13 deg/s.
using System;
using System.Collections.Generic;

namespace Megastructure.Player
{
    public static class GazeSchedule
    {
        public struct Key
        {
            public double S, Yaw, Pitch;
            public Key(double s, double yaw, double pitch) { S = s; Yaw = yaw; Pitch = pitch; }
        }

        /// <summary>Gaze keys; each line quotes its 14.5.x "Gaze" line.</summary>
        public static readonly Key[] Keys =
        {
            // 14.5.1: (0,0,0) (44,0,1) (60,60,4) (72,60,4) (88,0,1) (104,0,2) (120,0,0)
            new Key(0, 0, 0), new Key(44, 0, 1), new Key(60, 60, 4), new Key(72, 60, 4), new Key(88, 0, 1), new Key(104, 0, 2),
            // 14.5.2: (120,0,0) (124,0,1) (150,80,-8) (166,80,-8) (226,-75,-30) (242,-75,-34) (282,0,1) (300,0,2) (330,-12,1) (360,0,3) (420,0,2)
            new Key(120, 0, 0), new Key(124, 0, 1), new Key(150, 80, -8), new Key(166, 80, -8), new Key(226, -75, -30), new Key(242, -75, -34),
            new Key(282, 0, 1), new Key(300, 0, 2), new Key(330, -12, 1), new Key(348, 0, 3), new Key(364, 22, 4), new Key(404, 24, 4),   // A1 1.7 K2 hold (show request, fix wave S3-5)
            // 14.5.3: (420,0,2) (440,0,4) (470,70,-6) (500,70,-6) (540,-75,-4) (550,-90,-30) (562,-90,-58) (574,-90,-58) (596,-75,-2)
            //         (612,-75,-2) (640,0,3) (668,25,2) (690,0,2) (700,0,2)
            new Key(420, 0, 2), new Key(440, 0, 4), new Key(470, 70, -6), new Key(500, 70, -6), new Key(540, -75, -4), new Key(550, -90, -30),
            new Key(562, -90, -58), new Key(574, -90, -58), new Key(596, -75, -2), new Key(612, -75, -2), new Key(640, 0, 3), new Key(668, 25, 2),
            new Key(690, 0, 2),
            // 14.5.4: (700,0,2) (724,40,2) (752,78,-3) (796,78,-3) (814,25,6) (832,25,6) (852,0,24) (884,0,24) (906,0,8) (920,0,8)
            new Key(700, 0, 2), new Key(724, 40, 2), new Key(752, 78, -3), new Key(806, 74, -3), new Key(822, 25, 6), new Key(836, 25, 6),   // K4 eye + drape (show request)
            new Key(852, 0, 24), new Key(884, 0, 24), new Key(906, 0, 8),
            // 14.5.5: (920,0,8) (926,0,14) (934,0,0) (942,3,-22) (1000,3,-24) (1034,3,-8) (1054,0,0) (1070,0,0)
            new Key(920, 0, 8), new Key(926, 0, 14), new Key(934, 0, 0), new Key(942, 3, -22), new Key(1000, 3, -24), new Key(1034, 3, -8),
            new Key(1054, 0, 0),
            // 14.5.6 with ADDENDUM A1 1.8 (the kraken encounter replaces the whale; fix wave R4): the sucker arm on the glass (KK4 at
            // 1232: 66 / +4), then the main eye (Beat06Encounter: abeam pane 6, 33.9 m east, y -75, hold s 1252-1280): the keys at
            // 1256-1280 follow its bearing (70.5, 90, 104.9 deg from the walker at s 1256, 1268, 1280), KS17 at 1268 (90 / +2)
            new Key(1070, 0, 0), new Key(1082, 40, 6), new Key(1100, 90, 10), new Key(1140, 90, 12), new Key(1166, 70, 40), new Key(1190, 50, 6),
            new Key(1214, 58, 4), new Key(1232, 66, 4), new Key(1246, 64, 2), new Key(1256, 70, 2), new Key(1268, 90, 2), new Key(1280, 104, 1),
            new Key(1292, 100, -4), new Key(1320, 60, -24), new Key(1348, 25, -4),
            // 14.5.7: (1370,0,0) (1382,-18,0) (1400,35,0) (1410,35,0) (1434,-18,2) (1448,-27.5,-7) (1470,-27.5,-7) (1492,0,-1) (1500,0,-1)
            new Key(1370, 0, 0), new Key(1382, -18, 0), new Key(1400, 35, 0), new Key(1410, 35, 0), new Key(1434, -18, 2), new Key(1448, -27.5, -7),
            new Key(1470, -27.5, -7), new Key(1492, 0, -1), new Key(1500, 0, -1),
        };

        /// <summary>Ride and platform gaze keys (ADDENDUM A1 00 LEAD DECISION 1; s of the ending timeline, EndingPath: the ride
        /// advances s at 2.5 per second at every speed). Descent: down-out through the west glass and the glass floor (KS22), the
        /// sunset made by the descent (lower limb s 1545, centre s 1571 = KS23, gone s 1593), the look up the face to the lit band
        /// (KS24), the mist rivers at eye level, the platform lights below (KS25); transfer; the shuttle: the turn back to the dam
        /// through north while it recedes (KS26); the platform walk: north-west to the afterglow and the town, the right arc
        /// swings the dam into view, east to the Keeper's Lantern with the dam beyond it (the goal; CameraRig's end camera).</summary>
        public static readonly Key[] RideKeys =
        {
            // the descent (s 1510-1697.5): down-out (KS22), the sunset (KS23), the look up the face (KS24), the mist, the lights
            new Key(1500, 0, -1), new Key(1506, 0, -1), new Key(1534, -90, -30), new Key(1542, -90, -32),
            new Key(1556, -62, -3), new Key(1580, -60, -3), new Key(1594, -60, -4), new Key(1610, -15, 34), new Key(1620, -15, 36),
            new Key(1642, -90, -10), new Key(1652, -90, -10), new Key(1666, -105, -45), new Key(1684, -100, -40),
            // the foot: the turn back to the dam through north while the cabin settles and the shuttle starts gently (fix wave R1:
            // 20 s ramps, 0.51 g mean; launch s 1712.5), peak 16.7 deg/s; then the long look back while the wall recedes (cruise
            // s 1762.5-1800, 100 m/s), the pitch easing up as the wall recedes so the crest enters at the end (KS26 at 1822: 3.26
            // km out, crest 61.5 deg, frame top 65 deg; fix wave S3-8)
            new Key(1700, -90, -6), new Key(1718, 0, 4), new Key(1736, 90, 10), new Key(1760, 90, 16), new Key(1800, 90, 24), new Key(1822, 90, 32),
            // the braking (s 1800-1850) turns the look north-west to the afterglow and the town (KS26b, held through the doors and
            // the first 8.5 m of the walk: A1 3.9 "compression before the release"), then east to the Keeper's Lantern and the dam
            new Key(1850, -30, 3), new Key(1866, -30, 3), new Key(1893.5, 90, 0), new Key(1897.5, 90, 0),
        };

        /// <summary>Key-schedule gaze at progress s (world yaw, pitch; deg). Monotone cubic (PCHIP, Fritsch-Carlson with the
        /// weighted harmonic-mean tangents) through the keys on the unwrapped yaw and on the pitch: the tangent is zero only at
        /// holds and reversals, so a run of keys turning one way is one continuous turn (no stop at every key) and no segment
        /// overshoots or dips beyond its two keys (lead fix task S3-4).</summary>
        public static void Evaluate(double s, out double yaw, out double pitch)
        {
            bool ride = s > BwPath.Length;
            var k = ride ? RideKeys : Keys;
            var yu = ride ? s_rideYawU : s_walkYawU;
            var my = ride ? s_rideYawM : s_walkYawM;
            var mp = ride ? s_ridePitchM : s_walkPitchM;
            if (s <= k[0].S) { yaw = k[0].Yaw; pitch = k[0].Pitch; return; }
            int last = k.Length - 1;
            if (s >= k[last].S) { yaw = k[last].Yaw; pitch = k[last].Pitch; return; }
            int lo = 0, hi = last;
            while (hi - lo > 1)   // binary search: k[lo].S <= s < k[hi].S
            {
                int mid = (lo + hi) >> 1;
                if (k[mid].S <= s) lo = mid; else hi = mid;
            }
            double h = k[hi].S - k[lo].S;
            yaw = BwMath.WrapDeg(Hermite(yu[lo], yu[hi], my[lo], my[hi], h, (s - k[lo].S) / h));
            pitch = Hermite(k[lo].Pitch, k[hi].Pitch, mp[lo], mp[hi], h, (s - k[lo].S) / h);
        }

        static double Hermite(double v0, double v1, double m0, double m1, double h, double t)
        {
            double t2 = t * t, t3 = t2 * t;
            return (2.0 * t3 - 3.0 * t2 + 1.0) * v0 + (t3 - 2.0 * t2 + t) * h * m0 + (-2.0 * t3 + 3.0 * t2) * v1 + (t3 - t2) * h * m1;
        }

        static readonly double[] s_walkYawU, s_walkYawM, s_walkPitchM, s_rideYawU, s_rideYawM, s_ridePitchM;

        static GazeSchedule()
        {
            Prepare(Keys, out s_walkYawU, out s_walkYawM, out s_walkPitchM);
            Prepare(RideKeys, out s_rideYawU, out s_rideYawM, out s_ridePitchM);
        }

        static void Prepare(Key[] k, out double[] yawU, out double[] yawM, out double[] pitchM)
        {
            int n = k.Length;
            yawU = new double[n];
            var pitch = new double[n];
            for (int i = 0; i < n; i++)
            {
                yawU[i] = i == 0 ? k[0].Yaw : yawU[i - 1] + BwMath.DeltaDeg(k[i - 1].Yaw, k[i].Yaw);   // shortest-angle steps, unwrapped
                pitch[i] = k[i].Pitch;
            }
            yawM = Tangents(k, yawU);
            pitchM = Tangents(k, pitch);
        }

        /// <summary>PCHIP tangents (deg per m of s): 0 at the ends, at holds and at reversals; else the weighted harmonic mean of
        /// the two secants (w1 = 2 h1 + h0, w2 = h1 + 2 h0), which keeps every segment monotone.</summary>
        static double[] Tangents(Key[] k, double[] v)
        {
            int n = v.Length;
            var m = new double[n];
            for (int i = 1; i < n - 1; i++)
            {
                double h0 = k[i].S - k[i - 1].S, h1 = k[i + 1].S - k[i].S;
                if (h0 <= 0.0 || h1 <= 0.0) continue;
                double d0 = (v[i] - v[i - 1]) / h0, d1 = (v[i + 1] - v[i]) / h1;
                if (d0 * d1 <= 0.0) continue;
                double w1 = 2.0 * h1 + h0, w2 = h1 + 2.0 * h0;
                m[i] = (w1 + w2) / (w1 / d0 + w2 / d1);
            }
            return m;
        }

        /// <summary>
        /// Validator (3.1 b): keys strictly increasing in s, inside the path, and no key pair closer than 1 m with a changed
        /// pose (it would be a whip pan; such a change needs a ramp of at least 4 m). Returns the problems (empty = valid).
        /// </summary>
        public static List<string> Validate()
        {
            var problems = new List<string>();
            var k = Keys;
            for (int i = 0; i < k.Length; i++)
            {
                if (k[i].S < 0 || k[i].S > BwPath.Length) problems.Add($"key {i} s={k[i].S} outside the path");
                if (Math.Abs(k[i].Pitch) > 88.0) problems.Add($"key {i} pitch {k[i].Pitch} beyond +-88");
                if (i == 0) continue;
                double ds = k[i].S - k[i - 1].S;
                if (ds <= 0) problems.Add($"keys {i - 1}/{i} not increasing in s");
                bool changed = Math.Abs(BwMath.DeltaDeg(k[i - 1].Yaw, k[i].Yaw)) > 1e-6 || Math.Abs(k[i].Pitch - k[i - 1].Pitch) > 1e-6;
                if (changed && ds < 4.0) problems.Add($"keys {i - 1}/{i} change the pose within {ds:F2} m (< 4 m ramp)");
            }
            // the ride and platform keys: inside the ending timeline, increasing, no whip pans (s of the ride runs at 2.5 / s)
            var r = RideKeys;
            for (int i = 0; i < r.Length; i++)
            {
                if (r[i].S < BwPath.Length || r[i].S > EndingPath.EndS) problems.Add($"ride key {i} s={r[i].S} outside the ending");
                if (Math.Abs(r[i].Pitch) > 88.0) problems.Add($"ride key {i} pitch {r[i].Pitch} beyond +-88");
                if (i == 0) continue;
                double ds = r[i].S - r[i - 1].S;
                if (ds <= 0) problems.Add($"ride keys {i - 1}/{i} not increasing in s");
                bool changed = Math.Abs(BwMath.DeltaDeg(r[i - 1].Yaw, r[i].Yaw)) > 1e-6 || Math.Abs(r[i].Pitch - r[i - 1].Pitch) > 1e-6;
                if (changed && ds < 4.0) problems.Add($"ride keys {i - 1}/{i} change the pose within {ds:F2} m (< 4 m ramp)");
            }
            if (r.Length > 0 && (Math.Abs(r[0].S - Keys[Keys.Length - 1].S) > 1e-9 || Math.Abs(r[0].Yaw - Keys[Keys.Length - 1].Yaw) > 1e-9))
                problems.Add("the ride keys do not continue the last walk key");
            return problems;
        }
    }
}
