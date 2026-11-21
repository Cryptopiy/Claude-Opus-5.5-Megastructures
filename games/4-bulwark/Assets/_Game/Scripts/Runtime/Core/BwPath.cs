// BwPath: the walk path of Bulwark (MASTER_PROMPT 14.4), all in double. The single source of the path for the
// player controller, walker, PathGuide colliders, wall bot, captures and QA.
//  - s = horizontal distance along the filleted path, 0..1500 (corner arcs are part of the path).
//  - The plan curve is a FilletPath built from the 14.4 table: start (-1.5, 0) heading 0, L1+L2 934 m, A1 r 40
//    (0 -> 3.0), stairs 97.8 m, A2 r 40 (3.0 -> 0.86), exit hall + window gallery to s 1370, A3 r 30 (0.86 -> -18),
//    tunnel 50.1626 m, A4 r 20 (-18 -> 0), ledge to s 1500. End (-9.199, 1497.114).
//  - FloorY is the exact walking surface; EyeY = 1.7 + the triangular-kernel average (half-width 2.5 m) of FloorY,
//    evaluated in closed form (no time filter, identical at every speed).
//  - Lane gives capsule-centre lateral limits (lateral = signed distance to the right of the heading). Every lane
//    change is a smootherstep blend whose edge slope stays at most 0.6 m/m, so an analytic clamp never jumps,
//    forwards or backwards.
// Pure C# (the UnityEngine helpers at the end are compiled only inside Unity).
using System;

namespace Megastructure
{
    public static class BwPath
    {
        /// <summary>Path length (m of s), exact by construction.</summary>
        public const double Length = 1500.0;

        /// <summary>The filleted plan curve (s = its path distance).</summary>
        public static readonly FilletPath Fillet;

        // Piece boundaries of the built path (s). Table values (14.4) to 1e-3: 934, 936.094, 1033.894, 1035.388, 1370,
        // 1379.875, 1430.038, 1436.321, 1500.
        public static readonly double A1StartS, A1EndS, StairsStartS, StairsEndS, A2EndS, GalleryEndS, A3EndS, TunnelEndS, A4EndS;
        /// <summary>Plan x of the ledge leg (s >= A4EndS), -9.199.</summary>
        public static readonly double LedgeX;
        /// <summary>Waypoints (sharp corners) the FilletPath was built from: start, A1..A4 corners, end.</summary>
        public static readonly (double x, double z)[] Waypoints;
        public static readonly double[] WaypointRadius;

        /// <summary>Lane knots (s): every place where Lane(s) changes form (collider generators split boxes here).</summary>
        public static readonly double[] LaneKnots;

        static readonly double s_ledgeLatMin, s_ledgeLatMax;
        const double Deg2Rad = Math.PI / 180.0;

        static BwPath()
        {
            // ---- turtle walk of the 14.4 table: exact arcs, straight legs; corners = tangent-line intersections
            var w = new (double x, double z)[6];
            var r = new double[6];
            double s = 0.0, x = BulwarkConst.StartX, z = BulwarkConst.StartZ, h = 0.0;
            int wi = 0;
            w[wi++] = (x, z);

            void Straight(double len)
            {
                x += len * Math.Sin(h * Deg2Rad);
                z += len * Math.Cos(h * Deg2Rad);
                s += len;
            }

            void Arc(double radius, double h1)
            {
                double turn = Math.Abs(h1 - h) * Deg2Rad;
                double t = radius * Math.Tan(0.5 * turn);
                // corner = arc start + t along the incoming heading
                w[wi] = (x + t * Math.Sin(h * Deg2Rad), z + t * Math.Cos(h * Deg2Rad));
                r[wi] = radius;
                wi++;
                double sg = h1 > h ? 1.0 : -1.0;   // +1: right turn (heading increases)
                x += sg * radius * (Math.Cos(h * Deg2Rad) - Math.Cos(h1 * Deg2Rad));
                z += sg * radius * (Math.Sin(h1 * Deg2Rad) - Math.Sin(h * Deg2Rad));
                s += radius * turn;
                h = h1;
            }

            Straight(BulwarkConst.L2EndS);
            Arc(BulwarkConst.A1Radius, BulwarkConst.A1HeadingDeg);
            Straight(BulwarkConst.StairsLength);
            Arc(BulwarkConst.A2Radius, BulwarkConst.HallHeadingDeg);
            Straight(BulwarkConst.GalleryEndS - s);
            Arc(BulwarkConst.A3Radius, BulwarkConst.TunnelHeadingDeg);
            Straight(BulwarkConst.TunnelLength);
            Arc(BulwarkConst.A4Radius, BulwarkConst.LedgeHeadingDeg);
            Straight(Length - s);
            w[wi] = (x, z);

            var cum = new double[w.Length];
            for (int i = 1; i < w.Length; i++)
                cum[i] = cum[i - 1] + Math.Sqrt((w[i].x - w[i - 1].x) * (w[i].x - w[i - 1].x) + (w[i].z - w[i - 1].z) * (w[i].z - w[i - 1].z));

            Waypoints = w;
            WaypointRadius = r;
            Fillet = new FilletPath(w, cum, r);
            if (Fillet.PieceCount != 9) throw new InvalidOperationException("[BW-FAIL] BwPath: expected 9 pieces, got " + Fillet.PieceCount);

            A1StartS = Fillet.PieceStart(1);
            A1EndS = Fillet.PieceEnd(1);
            StairsStartS = Fillet.PieceStart(2);
            StairsEndS = Fillet.PieceEnd(2);
            A2EndS = Fillet.PieceEnd(3);
            GalleryEndS = Fillet.PieceEnd(4);
            A3EndS = Fillet.PieceEnd(5);
            TunnelEndS = Fillet.PieceEnd(6);
            A4EndS = Fillet.PieceEnd(7);
            LedgeX = w[w.Length - 1].x;
            s_ledgeLatMin = BulwarkConst.LaneLedgeXMin - LedgeX;
            s_ledgeLatMax = BulwarkConst.LaneLedgeXMax - LedgeX;

            LaneKnots = new[]
            {
                BulwarkConst.LaneDoorClearS, BulwarkConst.LaneDoorOpenS, BulwarkConst.LaneFunnelS0, BulwarkConst.LaneFunnelS1,
                BulwarkConst.LaneHallOpenS0, BulwarkConst.LaneHallOpenS1, BulwarkConst.LaneGalleryOpenS0, BulwarkConst.LaneGalleryOpenS1,
                BulwarkConst.LaneTunnelFunnelS0, BulwarkConst.LaneTunnelFunnelS1, BulwarkConst.LaneLedgeOpenS0, A4EndS,
                BulwarkConst.LaneCabinFunnelS0, BulwarkConst.LaneCabinFunnelS1,
            };
        }

        static double ClampS(double s) => s < 0.0 ? 0.0 : (s > Length ? Length : s);

        // ================================================================ plan curve
        /// <summary>Plan position (x, z) of the path centre line at s (clamped to 0..1500).</summary>
        public static void Point(double s, out double x, out double z) => Fillet.PointAt(ClampS(s), out x, out z);

        /// <summary>Yaw of the tangent in degrees (0 = +Z, 90 = +X).</summary>
        public static double HeadingDeg(double s) => Fillet.HeadingAt(ClampS(s));

        /// <summary>Unit plan tangent (x, z).</summary>
        public static void Tangent(double s, out double tx, out double tz) => Fillet.TangentAt(ClampS(s), out tx, out tz);

        /// <summary>Unit plan right vector (x, z): lateral axis (walking +Z, right = +X).</summary>
        public static void Right(double s, out double rx, out double rz) => Fillet.RightAt(ClampS(s), out rx, out rz);

        /// <summary>Signed curvature (1/m), positive in a right turn (A1, A4), negative in a left turn (A2, A3).</summary>
        public static double Curvature(double s) => Fillet.CurvatureAt(ClampS(s));

        /// <summary>Floor-level world point at (s, lateral).</summary>
        public static void World(double s, double lateral, out double x, out double y, out double z)
        {
            s = ClampS(s);
            Fillet.PointAt(s, out double px, out double pz);
            Fillet.RightAt(s, out double rx, out double rz);
            x = px + lateral * rx;
            z = pz + lateral * rz;
            y = FloorY(s);
        }

        /// <summary>Eye world point at (s, lateral): plan point plus EyeY(s).</summary>
        public static void EyeWorld(double s, double lateral, out double x, out double y, out double z)
        {
            World(s, lateral, out x, out _, out z);
            y = EyeY(s);
        }

        /// <summary>Nearest s (plan) to (x, z); pieces within 40 m of the hint are searched. Exact on every normal of the path.</summary>
        public static double Project(double x, double z, double hintS) => ClampS(Fillet.Project(x, z, ClampS(hintS), 40.0, out _));

        /// <summary>Nearest s and the plan distance to the path.</summary>
        public static double Project(double x, double z, double hintS, out double distance) => ClampS(Fillet.Project(x, z, ClampS(hintS), 40.0, out distance));

        /// <summary>Signed lateral of plan point (x, z) relative to the path at s (positive = right).</summary>
        public static double Lateral(double x, double z, double s) => Fillet.Lateral(x, z, ClampS(s));

        // ================================================================ beats
        /// <summary>Beat 1..7 from s (starts 0, 120, 420, 700, 920, 1070, 1370).</summary>
        public static int BeatAt(double s)
        {
            var b = BulwarkConst.BeatS;
            for (int i = BulwarkConst.BeatCount - 1; i > 0; i--) if (s >= b[i]) return i + 1;
            return 1;
        }

        /// <summary>Start s of beat 1..7 (clamped).</summary>
        public static double BeatStartS(int beat) => BulwarkConst.BeatS[Math.Max(1, Math.Min(BulwarkConst.BeatCount, beat)) - 1];

        // ================================================================ floor and eye
        /// <summary>s of the start of stair flight k (0..14).</summary>
        public static double StairFlightStartS(int k) => StairsStartS + BulwarkConst.StairFlightPitch * k;

        /// <summary>s of riser i (0..269): flight i / 18, step i % 18; the floor drops 0.2 m there.</summary>
        public static double StairRiserS(int i)
        {
            int k = i / BulwarkConst.StairStepsPerFlight, j = i % BulwarkConst.StairStepsPerFlight;
            return StairsStartS + BulwarkConst.StairFlightPitch * k + BulwarkConst.StairRiserPhase + BulwarkConst.StairTread * j;
        }

        /// <summary>Exact walking-surface y at s: road 0; stairs with 0.2 m risers, 0.3 m treads, 1.2 m landings
        /// (0 -> -54); -54 to s 1070; gallery ramp -0.12 per m to -90 at s 1370; -90 to the end.</summary>
        public static double FloorY(double s)
        {
            double y;
            if (s < StairsStartS) y = 0.0;
            else if (s < StairsEndS)
            {
                double u = s - StairsStartS;
                int k = (int)Math.Floor(u / BulwarkConst.StairFlightPitch);
                if (k > BulwarkConst.StairFlights - 1) k = BulwarkConst.StairFlights - 1;
                if (k < 0) k = 0;
                double lu = u - BulwarkConst.StairFlightPitch * k;
                int n;
                if (lu < BulwarkConst.StairRiserPhase) n = 0;
                else n = Math.Min(BulwarkConst.StairStepsPerFlight, (int)Math.Floor((lu - BulwarkConst.StairRiserPhase) / BulwarkConst.StairTread) + 1);
                y = -BulwarkConst.StairFlightDrop * k - BulwarkConst.StairRiser * n;
            }
            else y = -BulwarkConst.StairDrop;
            if (s > BulwarkConst.GalleryStartS)
                y -= BulwarkConst.GalleryRamp * (Math.Min(s, BulwarkConst.GalleryEndS) - BulwarkConst.GalleryStartS);
            return y;
        }

        /// <summary>Tread line: each flight as the straight pitch line through its step midpoints (start, top) to
        /// (start + 5.4, top - 3.6), landings flat, gallery ramp. Ramp colliders and the 0.28 m eye check use it.</summary>
        public static double TreadLineY(double s)
        {
            double y;
            if (s < StairsStartS) y = 0.0;
            else if (s < StairsEndS)
            {
                double u = s - StairsStartS;
                int k = (int)Math.Floor(u / BulwarkConst.StairFlightPitch);
                if (k > BulwarkConst.StairFlights - 1) k = BulwarkConst.StairFlights - 1;
                if (k < 0) k = 0;
                double lu = Math.Min(u - BulwarkConst.StairFlightPitch * k, BulwarkConst.StairFlightRun);
                y = -BulwarkConst.StairFlightDrop * (k + lu / BulwarkConst.StairFlightRun);
            }
            else y = -BulwarkConst.StairDrop;
            if (s > BulwarkConst.GalleryStartS)
                y -= BulwarkConst.GalleryRamp * (Math.Min(s, BulwarkConst.GalleryEndS) - BulwarkConst.GalleryStartS);
            return y;
        }

        /// <summary>Triangular-kernel average (half-width h) of FloorY at s, in closed form: each riser contributes
        /// -0.2 x the kernel CDF, each ramp kink the kernel's second integral.</summary>
        public static double SmoothedFloorY(double s)
        {
            double h = BulwarkConst.EyeKernelHalfWidth;
            double lo = s - h, hi = s + h;
            double y = 0.0;
            int steps = BulwarkConst.StairStepsPerFlight;
            for (int k = 0; k < BulwarkConst.StairFlights; k++)
            {
                double f0 = StairsStartS + BulwarkConst.StairFlightPitch * k + BulwarkConst.StairRiserPhase;
                double fl = f0 + BulwarkConst.StairTread * (steps - 1);
                if (fl <= lo) { y -= BulwarkConst.StairFlightDrop; continue; }
                if (f0 >= hi) break;
                for (int j = 0; j < steps; j++)
                    y -= BulwarkConst.StairRiser * KernelCdf(s - (f0 + BulwarkConst.StairTread * j), h);
            }
            y -= BulwarkConst.GalleryRamp * (KernelRamp(s - BulwarkConst.GalleryStartS, h) - KernelRamp(s - BulwarkConst.GalleryEndS, h));
            return y;
        }

        /// <summary>Eye height y at s: 1.7 + the triangular-kernel average of FloorY (14.4). Second difference at 30 fps
        /// on the stairs: 1.95 mm at 2.5 m/s, 0.72 mm at 1.5 m/s; at most 0.28 m above the tread line.</summary>
        public static double EyeY(double s) => BulwarkConst.Eye + SmoothedFloorY(s);

        /// <summary>CDF of the symmetric triangular kernel of half-width h (the smoothed unit step).</summary>
        public static double KernelCdf(double x, double h)
        {
            if (x <= -h) return 0.0;
            if (x >= h) return 1.0;
            double h2 = 2.0 * h * h;
            if (x <= 0.0) { double a = x + h; return a * a / h2; }
            double b = h - x;
            return 1.0 - b * b / h2;
        }

        /// <summary>Integral of KernelCdf (the smoothed unit ramp max(x, 0)).</summary>
        public static double KernelRamp(double x, double h)
        {
            if (x <= -h) return 0.0;
            if (x >= h) return x;
            double h6 = 6.0 * h * h;
            if (x <= 0.0) { double a = x + h; return a * a * a / h6; }
            double b = h - x;
            return x + b * b * b / h6;
        }

        // ================================================================ lanes
        /// <summary>Capsule-centre lateral limits at s (lateral = signed distance to the right of the heading).
        /// Corridor x -3.15..0.15 through the door (until the 0.7 m door wall is cleared, s 121.05), widening to the road
        /// x -5.4..+5.1 by s 137.05; funnel s 898-919.65 to the portal (3.6 m portal x -3.3..0.3 -> lane 2.9, reached one
        /// radius before the plane z 920); atrium and stairs 2.9;
        /// exit hall 3.7 (from s 1035.74); gallery axis -5.0..+2.65 (from the doorway, s 1070.95..1082.95); funnel
        /// s 1355-1372.65 to the tunnel door; tunnel 2.5; widening s 1428..A4 end to the ledge x -11.8..-6.4.</summary>
        public static void Lane(double s, out double latMin, out double latMax)
        {
            s = ClampS(s);
            double corrMin = BulwarkConst.LaneCorridorXMin - BulwarkConst.StartX, corrMax = BulwarkConst.LaneCorridorXMax - BulwarkConst.StartX;
            double roadMin = BulwarkConst.LaneRoadXMin - BulwarkConst.StartX, roadMax = BulwarkConst.LaneRoadXMax - BulwarkConst.StartX;
            double portal = 0.5 * BulwarkConst.LanePortalWidth, stairs = 0.5 * BulwarkConst.LaneStairsWidth;
            double hall = 0.5 * BulwarkConst.LaneExitHallWidth, tunnel = 0.5 * BulwarkConst.LaneTunnelWidth;
            double galMin = BulwarkConst.LaneGalleryLeft, galMax = BulwarkConst.LaneGalleryRight;

            if (s <= BulwarkConst.LaneDoorClearS) { latMin = corrMin; latMax = corrMax; return; }
            if (s < BulwarkConst.LaneDoorOpenS) { Blend(s, BulwarkConst.LaneDoorClearS, BulwarkConst.LaneDoorOpenS, corrMin, corrMax, roadMin, roadMax, out latMin, out latMax); return; }
            if (s <= BulwarkConst.LaneFunnelS0) { latMin = roadMin; latMax = roadMax; return; }
            if (s < BulwarkConst.LaneFunnelS1) { Blend(s, BulwarkConst.LaneFunnelS0, BulwarkConst.LaneFunnelS1, roadMin, roadMax, -portal, portal, out latMin, out latMax); return; }
            if (s <= BulwarkConst.LaneHallOpenS0)
            {
                // portal, atrium (pedestrian way), stair mouth, stairs and A2 share the 2.9 m lane
                double half = Math.Min(portal, stairs);
                latMin = -half; latMax = half; return;
            }
            if (s < BulwarkConst.LaneHallOpenS1) { Blend(s, BulwarkConst.LaneHallOpenS0, BulwarkConst.LaneHallOpenS1, -stairs, stairs, -hall, hall, out latMin, out latMax); return; }
            if (s <= BulwarkConst.LaneGalleryOpenS0) { latMin = -hall; latMax = hall; return; }
            if (s < BulwarkConst.LaneGalleryOpenS1) { Blend(s, BulwarkConst.LaneGalleryOpenS0, BulwarkConst.LaneGalleryOpenS1, -hall, hall, galMin, galMax, out latMin, out latMax); return; }
            if (s <= BulwarkConst.LaneTunnelFunnelS0) { latMin = galMin; latMax = galMax; return; }
            if (s < BulwarkConst.LaneTunnelFunnelS1) { Blend(s, BulwarkConst.LaneTunnelFunnelS0, BulwarkConst.LaneTunnelFunnelS1, galMin, galMax, -tunnel, tunnel, out latMin, out latMax); return; }
            if (s <= BulwarkConst.LaneLedgeOpenS0) { latMin = -tunnel; latMax = tunnel; return; }
            if (s < A4EndS) { Blend(s, BulwarkConst.LaneLedgeOpenS0, A4EndS, -tunnel, tunnel, s_ledgeLatMin, s_ledgeLatMax, out latMin, out latMax); return; }
            if (s <= BulwarkConst.LaneCabinFunnelS0) { latMin = s_ledgeLatMin; latMax = s_ledgeLatMax; return; }
            // the ending (ADDENDUM A1 00): funnel into the cabin's 1.6 m south door (capsule lane +-0.45), then the cabin
            double door = BulwarkConst.FootDoorHalfLane;
            if (s < BulwarkConst.LaneCabinFunnelS1) { Blend(s, BulwarkConst.LaneCabinFunnelS0, BulwarkConst.LaneCabinFunnelS1, s_ledgeLatMin, s_ledgeLatMax, -door, door, out latMin, out latMax); return; }
            latMin = -door; latMax = door;
        }

        /// <summary>Lane centre and half-width at s (convenience for collider generators).</summary>
        public static void LaneCentre(double s, out double centre, out double halfWidth)
        {
            Lane(s, out double a, out double b);
            centre = 0.5 * (a + b);
            halfWidth = 0.5 * (b - a);
        }

        /// <summary>Clamp a lateral into the lane at s.</summary>
        public static double ClampLateral(double s, double lateral)
        {
            Lane(s, out double a, out double b);
            return lateral < a ? a : (lateral > b ? b : lateral);
        }

        static void Blend(double s, double s0, double s1, double a0, double b0, double a1, double b1, out double a, out double b)
        {
            double w = Smootherstep01((s - s0) / (s1 - s0));
            a = a0 + (a1 - a0) * w;
            b = b0 + (b1 - b0) * w;
        }

        // ================================================================ cinematic lateral keys
        /// <summary>Lateral offset of the cinematic walker (14.12 keys of beats 2-4, smootherstep), 0 elsewhere; always
        /// inside Lane(s) with at least 0.15 m margin.</summary>
        public static double CinematicLateral(double s)
        {
            var ks = BulwarkConst.LateralKeyS;
            var kx = BulwarkConst.LateralKeyX;
            if (s <= ks[0] || s >= ks[ks.Length - 1]) return 0.0;
            int i = KeyIndex(ks, s);
            double l0 = kx[i] - BulwarkConst.StartX, l1 = kx[i + 1] - BulwarkConst.StartX;
            return l0 + (l1 - l0) * Smootherstep01((s - ks[i]) / (ks[i + 1] - ks[i]));
        }

        /// <summary>d CinematicLateral / ds (lateral velocity = this x progress speed; feed-forward for the walker).</summary>
        public static double CinematicLateralRate(double s)
        {
            var ks = BulwarkConst.LateralKeyS;
            var kx = BulwarkConst.LateralKeyX;
            if (s <= ks[0] || s >= ks[ks.Length - 1]) return 0.0;
            int i = KeyIndex(ks, s);
            double ds = ks[i + 1] - ks[i];
            double u = (s - ks[i]) / ds;
            return (kx[i + 1] - kx[i]) / ds * 30.0 * u * u * (1.0 - u) * (1.0 - u);
        }

        static int KeyIndex(double[] ks, double s)
        {
            for (int i = 0; i < ks.Length - 1; i++) if (s < ks[i + 1]) return i;
            return ks.Length - 2;
        }

        /// <summary>Ground speed / progress speed for a walker at (s, lateral) moving its lateral at lateralRate per m of s:
        /// sqrt((1 - curvature lateral)^2 + lateralRate^2). Peak with the cinematic keys: 1.03878 at s 141 (2.597 m/s).</summary>
        public static double GroundSpeedFactor(double s, double lateral, double lateralRate)
        {
            double a = 1.0 - Curvature(s) * lateral;
            return Math.Sqrt(a * a + lateralRate * lateralRate);
        }

        static double Smootherstep01(double u)
        {
            if (u <= 0.0) return 0.0;
            if (u >= 1.0) return 1.0;
            return u * u * u * (u * (u * 6.0 - 15.0) + 10.0);
        }

#if UNITY_5_3_OR_NEWER
        // ================================================================ Unity conveniences (float, for meshes and transforms)
        /// <summary>Floor-level world point as a Vector3.</summary>
        public static UnityEngine.Vector3 WorldPoint(double s, double lateral)
        {
            World(s, lateral, out double x, out double y, out double z);
            return new UnityEngine.Vector3((float)x, (float)y, (float)z);
        }

        /// <summary>Eye world point as a Vector3.</summary>
        public static UnityEngine.Vector3 EyePoint(double s, double lateral)
        {
            EyeWorld(s, lateral, out double x, out double y, out double z);
            return new UnityEngine.Vector3((float)x, (float)y, (float)z);
        }

        /// <summary>Unit tangent as a Vector3 (y = 0).</summary>
        public static UnityEngine.Vector3 TangentV(double s)
        {
            Tangent(s, out double tx, out double tz);
            return new UnityEngine.Vector3((float)tx, 0f, (float)tz);
        }

        /// <summary>Unit right vector as a Vector3 (y = 0).</summary>
        public static UnityEngine.Vector3 RightV(double s)
        {
            Right(s, out double rx, out double rz);
            return new UnityEngine.Vector3((float)rx, 0f, (float)rz);
        }
#endif
    }

    /// <summary>
    /// The ending timeline beyond the walked path (ADDENDUM A1 00 LEAD DECISION 1): one coordinate s continues past 1500.
    /// Ride legs (s advances RideSRate = 2.5 per ride second at every walking speed; tauL = (s - 1500) / 2.5):
    ///   Boarding  s 1500.0-1510.0  (tauL 0-4)      doors close 0.5-3.0, seal 3.0-4.0 at the top station on the ledge
    ///   Descent   s 1510.0-1697.5  (tauL 4-79)     75 s vertical descent on the outside of the lee face, floor y -90 -> -5999
    ///   Transfer  s 1697.5-1712.5  (tauL 79-85)    the cabin settles and couples to the maglev bogie at the foot
    ///   Shuttle   s 1712.5-1850.0  (tauL 85-140)   55 s run 3500 m west across the lowland (fix wave R1)
    ///   Arrived   s 1850.0-1857.5  (tauL 140-143)  the north door opens at the lantern platform
    /// then the walk (1 m of s per m): s 1857.5-1897.5 on the platform (north 12, right arc r 8, east 15.434) to the rest
    /// 1.9 m short of the Keeper's Lantern; goal trigger s 1896.0. Both motion profiles are cosine S-curves (v = vmax (1 -
    /// cos(pi t / Ta)) / 2 on each ramp, cruise between): descent L 5909 m, T 75, vmax 110 -> Ta = 75 - 5909 / 110 = 21.282 s,
    /// peak acceleration pi/2 x 110 / 21.282 = 8.12 m/s^2; shuttle L 3500, T 55, Ta 20 -> vmax 100 m/s, mean 5.0 m/s^2 (0.51 g), peak 7.85 m/s^2.
    /// The cabin never rolls or pitches. Pure C#, double; the walker's foot point in the cabin is the path end (-9.199, 1497.114).
    /// </summary>
    public static class EndingPath
    {
        public const double S0 = 1500.0;
        public static readonly double SDescent0, SDescent1, STransfer1, SShuttle1, SArrive1, SFoot0, SRest, SGoal, EndS;
        public static readonly double TauDescent0, TauDescent1, TauTransfer1, TauShuttle1, TauArrive1;
        /// <summary>The walker's foot point in the cabin (plan) = the end of the walked path.</summary>
        public static readonly double SpotX, SpotZ;
        public static readonly double DescentLength, DescentRamp, ShuttleVMax;
        /// <summary>The platform walk (plan, starts at the cabin spot at the platform, heading 0).</summary>
        public static readonly FilletPath Foot;
        public static readonly double FootX0, FootZ0, RestX, RestZ, KeeperX, KeeperZ;
        public const double FloorTopY = BulwarkConst.FloorYTunnel;
        public const double FloorFootY = BulwarkConst.CabinFloorFootY;

        static EndingPath()
        {
            BwPath.Point(BwPath.Length, out SpotX, out SpotZ);
            double r = BulwarkConst.RideSRate;
            TauDescent0 = BulwarkConst.RideBoardTime;
            TauDescent1 = TauDescent0 + BulwarkConst.RideDescentTime;
            TauTransfer1 = TauDescent1 + BulwarkConst.RideTransferTime;
            TauShuttle1 = TauTransfer1 + BulwarkConst.RideShuttleTime;
            TauArrive1 = TauShuttle1 + BulwarkConst.RideDoorsTime;
            SDescent0 = S0 + r * TauDescent0; SDescent1 = S0 + r * TauDescent1; STransfer1 = S0 + r * TauTransfer1;
            SShuttle1 = S0 + r * TauShuttle1; SArrive1 = S0 + r * TauArrive1;
            SFoot0 = SArrive1;
            DescentLength = BulwarkConst.FloorYTunnel - BulwarkConst.CabinFloorFootY;                  // 5909
            DescentRamp = BulwarkConst.RideDescentTime - DescentLength / BulwarkConst.DescentVMax;      // 21.282
            ShuttleVMax = BulwarkConst.ShuttleDistance / (BulwarkConst.RideShuttleTime - BulwarkConst.ShuttleRampTime);

            FootX0 = SpotX - BulwarkConst.ShuttleDistance; FootZ0 = SpotZ;
            double cz = FootZ0 + BulwarkConst.FootNorth + BulwarkConst.FootArcR;
            var w = new (double x, double z)[] { (FootX0, FootZ0), (FootX0, cz), (FootX0 + BulwarkConst.FootArcR + BulwarkConst.FootEast, cz) };
            var cum = new double[] { 0.0, cz - FootZ0, cz - FootZ0 + BulwarkConst.FootArcR + BulwarkConst.FootEast };
            Foot = new FilletPath(w, cum, new double[] { 0.0, BulwarkConst.FootArcR, 0.0 });
            SRest = SFoot0 + Foot.Length;
            SGoal = SRest - BulwarkConst.EndGoalBeforeRest;
            EndS = SRest;
            RestX = w[2].x; RestZ = w[2].z;
            KeeperX = RestX + BulwarkConst.KeeperAhead; KeeperZ = RestZ - BulwarkConst.KeeperRight;
        }

        /// <summary>Ride clock (s since the walker came to rest in the cabin) of s.</summary>
        public static double TauL(double s) => Math.Max(0.0, Math.Min(s, SFoot0) - S0) / BulwarkConst.RideSRate;
        public static double SOfTau(double tauL) => S0 + BulwarkConst.RideSRate * tauL;
        public static bool InRide(double s) => s > S0 + 1e-9 && s < SFoot0;
        public static bool OnFoot(double s) => s >= SFoot0;

        public static EndingRidePhase PhaseAt(double s, out double progress)
        {
            double t = TauL(s);
            if (s <= S0) { progress = 0; return EndingRidePhase.None; }
            if (t < TauDescent0) { progress = t / TauDescent0; return EndingRidePhase.Boarding; }
            if (t < TauDescent1) { progress = (t - TauDescent0) / (TauDescent1 - TauDescent0); return EndingRidePhase.Descent; }
            if (t < TauTransfer1) { progress = (t - TauDescent1) / (TauTransfer1 - TauDescent1); return EndingRidePhase.Transfer; }
            if (t < TauShuttle1) { progress = (t - TauTransfer1) / (TauShuttle1 - TauTransfer1); return EndingRidePhase.Shuttle; }
            if (s < SFoot0) { progress = (t - TauShuttle1) / (TauArrive1 - TauShuttle1); return EndingRidePhase.Arrived; }
            progress = 1.0;
            return EndingRidePhase.Arrived;
        }

        // ---------------------------------------------------------------- motion profiles (cosine S-curve, closed form)
        /// <summary>Distance at time t of a profile with length L, duration T, ramp time Ta (0..T/2); v = speed.</summary>
        public static double Profile(double t, double L, double T, double Ta, out double v)
        {
            double vmax = L / Math.Max(1e-9, T - Ta);
            if (t <= 0) { v = 0; return 0; }
            if (t >= T) { v = 0; return L; }
            if (t < Ta) { v = vmax * 0.5 * (1.0 - Math.Cos(Math.PI * t / Ta)); return vmax * (0.5 * t - Ta / (2.0 * Math.PI) * Math.Sin(Math.PI * t / Ta)); }
            if (t > T - Ta)
            {
                double u = T - t;
                v = vmax * 0.5 * (1.0 - Math.Cos(Math.PI * u / Ta));
                return L - vmax * (0.5 * u - Ta / (2.0 * Math.PI) * Math.Sin(Math.PI * u / Ta));
            }
            v = vmax;
            return vmax * (0.5 * Ta + (t - Ta));
        }

        /// <summary>Signed acceleration (m/s^2) of the profile at t (positive = speeding up).</summary>
        public static double ProfileAccel(double t, double L, double T, double Ta)
        {
            double vmax = L / Math.Max(1e-9, T - Ta);
            if (t <= 0 || t >= T) return 0;
            if (t < Ta) return vmax * 0.5 * Math.PI / Ta * Math.Sin(Math.PI * t / Ta);
            if (t > T - Ta) return -vmax * 0.5 * Math.PI / Ta * Math.Sin(Math.PI * (T - t) / Ta);
            return 0;
        }

        /// <summary>Descended distance (m) and speed at descent time t (0..75).</summary>
        public static double Descent(double t, out double v) => Profile(t, DescentLength, BulwarkConst.RideDescentTime, DescentRamp, out v);
        /// <summary>Shuttle distance west (m) and speed at shuttle time t (0..55).</summary>
        public static double Shuttle(double t, out double v) => Profile(t, BulwarkConst.ShuttleDistance, BulwarkConst.RideShuttleTime, BulwarkConst.ShuttleRampTime, out v);

        /// <summary>The cabin's walker spot (floor point under the eye) at ride s: world x, floor y, z; velocity (m/s).</summary>
        public static void Cabin(double s, out double x, out double y, out double z, out double vx, out double vy)
        {
            double t = TauL(s);
            x = SpotX; z = SpotZ; y = FloorTopY; vx = 0; vy = 0;
            if (t <= TauDescent0) return;
            double dd = Descent(Math.Min(t, TauDescent1) - TauDescent0, out double v);
            y = FloorTopY - dd;
            if (t < TauDescent1) { vy = -v; return; }
            y = FloorFootY;
            if (t <= TauTransfer1) return;
            double dx = Shuttle(Math.Min(t, TauShuttle1) - TauTransfer1, out double vs);
            x = SpotX - dx;
            if (t < TauShuttle1) vx = -vs;
        }

        /// <summary>Offset of the cabin (m along x, plus y) from its position at the top: the whole cabin set moves by it.</summary>
        public static void CabinOffset(double s, out double dx, out double dy)
        {
            Cabin(s, out double x, out double y, out _, out _, out _);
            dx = x - SpotX; dy = y - FloorTopY;
        }

        /// <summary>Floor point of the walker at any ending s (ride: the cabin spot + lateral along +x; foot: the platform path).</summary>
        public static void World(double s, double lateral, out double x, out double y, out double z)
        {
            if (s < SFoot0) { Cabin(s, out x, out y, out z, out _, out _); x += lateral; return; }
            double d = Math.Max(0.0, Math.Min(Foot.Length, s - SFoot0));
            Foot.PointAt(d, out double px, out double pz);
            Foot.RightAt(d, out double rx, out double rz);
            x = px + lateral * rx; z = pz + lateral * rz; y = FloorFootY;
        }

        public static double CabinFloorY(double s) { Cabin(s, out _, out double y, out _, out _, out _); return y; }
        public static double EyeY(double s) => (s < SFoot0 ? CabinFloorY(s) : FloorFootY) + BulwarkConst.Eye;

        /// <summary>Heading of the walk: 0 in the cabin (the walker entered facing north), the platform path after.</summary>
        public static double HeadingDeg(double s) => s < SFoot0 ? 0.0 : Foot.HeadingAt(Math.Max(0.0, Math.Min(Foot.Length, s - SFoot0)));

        /// <summary>Capsule-centre lane of the platform walk: the door lane (+-0.45) to 1 m past the north wall, smootherstep to
        /// the plaza lane (+-3.0) over 8 m (edge slope 1.875 x 2.55 / 8 = 0.60 m/m), the plaza after. In the cabin: 0.</summary>
        public static void Lane(double s, out double lo, out double hi)
        {
            if (s < SFoot0) { lo = 0; hi = 0; return; }
            double d = s - SFoot0;
            double d0 = BulwarkConst.CabinZ1 - SpotZ + 1.0, d1 = d0 + 8.0;
            double a = BulwarkConst.FootDoorHalfLane, b = BulwarkConst.FootPlazaHalfLane;
            double u = d <= d0 ? 0.0 : (d >= d1 ? 1.0 : Sst((d - d0) / (d1 - d0)));
            hi = a + (b - a) * u; lo = -hi;
        }

        public static double ClampLateral(double s, double lateral)
        {
            Lane(s, out double lo, out double hi);
            return lateral < lo ? lo : (lateral > hi ? hi : lateral);
        }

        /// <summary>Eye altitude above the lowland (m) and horizontal distance from the lee face (m) of a world point.</summary>
        public static double Altitude(double y) => y - BulwarkConst.LowlandY;
        public static double DistanceFromFace(double x) => Math.Max(0.0, BulwarkConst.LeeFaceX - x);

        /// <summary>Descended fraction F (0 at the top, 1 at the foot) of the ride at s (the sun and haze key, 3.7).</summary>
        public static double DescentFraction(double s)
        {
            if (s <= SDescent0) return 0.0;
            if (s >= SDescent1) return 1.0;
            return (FloorTopY - CabinFloorY(s)) / DescentLength;
        }

        /// <summary>Platform-walk fraction (0 at the door, 1 at the goal trigger).</summary>
        public static double FootFraction(double s) => Math.Max(0.0, Math.Min(1.0, (s - SFoot0) / Math.Max(1e-9, SGoal - SFoot0)));

        static double Sst(double u) => u <= 0 ? 0 : (u >= 1 ? 1 : u * u * u * (u * (u * 6.0 - 15.0) + 10.0));
    }
}
