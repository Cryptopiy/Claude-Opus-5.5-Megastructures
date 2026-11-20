using System;
using UnityEngine;

namespace Megastructure
{
    /// <summary>
    /// The walkable lanes of 14.4 as one pure function of the progress s (IMPORT_PLAN 3.2): the half-width that the player's
    /// centre may reach ("collider gap minus 0.70 m", so the CharacterController (r 0.35) touches the invisible wall at the lane
    /// edge + 0.35). Exposes The Ring's PathGuide API (LaneHalfWidth, DistanceOutsideLanes, LaneSamplePoints, CentreAt) for
    /// the player, the safety volume, the wall bot and Thread's generic corridor builder (PathGuide), so colliders are derived
    /// from this walkable-width function (lesson 11), never from a hand-listed span table.
    ///
    /// Lane list of 14.4 (half-widths, m) and the transitions chosen here (smoothstep between knots; each window is short and
    /// lies where the visible barrier changes):
    ///  - alley: clear width 4.4 (s 0-26) -> 6.0 at the bar (s 36-56) -> 4.6 (s 66-70) -> 4.4 (s 76-112) -> 5.0 at the mouth
    ///    (s 120); half-width = (clear width - 0.70) / 2: 1.85, 2.65, 1.95, 1.85, 2.15 (14.5.1);
    ///  - the mouth opens to the crossing of the street over s 120-124: 10.0 (bollard line) to the corner C1 (s 171);
    ///  - through C1 (s 171-208.7) 10.0 -> median 11.0, held to s 396;
    ///  - tongue (beat 3, x = s - 184.70): min(12.0, max(2.15, w(x) - 2.0)) with w(x) of 14.4; blended from the median over
    ///    s 396-404 (11 -> 12);
    ///  - bridge 2.15 (s 560-867.7, both gates included: gate A sits at the tongue tip, where the lane is already 2.15);
    ///  - gate B funnel (x 683-695, s 867.7-879.7) 2.15 -> 3.5, then the garden 6.0 from s 886;
    ///  - garden 6.0 -> promenade 10.0 over s 1074-1086 (inside C2, centred on the beat boundary 1080);
    ///  - promenade 10.0 -> plaza 12.0 over s 1254-1266 (inside C3, centred on 1260), 12.0 to s 1440, funnel to 3.0 at the
    ///    tenement (s 1500).
    /// Every lane half-width stays below the fillet radius at the corners (11 &lt; 24, 10 and 12 &lt; 30), so the projection onto
    /// the centre line is unique everywhere in the lanes. Pure maths except the Vector3 conveniences; allocation-free.
    /// </summary>
    public static class NocLanes
    {
        /// <summary>The walk path (filleted centre line of 14.4); s is its arc length.</summary>
        public static readonly FilletPath Path = FilletPath.CreateNocturne();

        /// <summary>Knots (s, half-width) of the piecewise lanes; the tongue (s 404-560) is the analytic w(x) rule instead.</summary>
        static readonly double[] KnotS =
        {
            0, 26, 36, 56, 66, 70, 76, 112, 120, 124, 171, 208.7, 396,
        };
        static readonly double[] KnotV =
        {
            1.85, 1.85, 2.65, 2.65, 1.95, 1.95, 1.85, 1.85, 2.15, 10.0, 10.0, 11.0, 11.0,
        };
        static readonly double[] KnotS2 =
        {
            560, 867.7, 879.7, 886, 1074, 1086, 1254, 1266, 1440, 1500,
        };
        static readonly double[] KnotV2 =
        {
            2.15, 2.15, 3.5, 6.0, 6.0, 10.0, 10.0, 12.0, 12.0, 3.0,
        };

        /// <summary>Every s where the lane function changes form (knots, the tongue blend and its w(x) breaks): the corridor
        /// builder puts a segment boundary on each, so a straight box never spans a kink.</summary>
        public static double[] Breakpoints()
        {
            var list = new System.Collections.Generic.List<double>();
            list.AddRange(KnotS);
            list.Add(404.0);
            foreach (double x in new[] { 225.3, 305.3, 350.0 }) list.Add(x + NocturneConstants.EastLegOffset);
            list.AddRange(KnotS2);
            list.Sort();
            return list.ToArray();
        }

        /// <summary>Lane half-width at the alley mouth / bridge / gate minimum (14.4).</summary>
        public const double BridgeHalfWidth = 2.15;
        public const double TongueMax = 12.0, TongueMargin = 2.0;

        /// <summary>Tongue A half-width w(x) of 14.4: 75 to x 225.3, smoothstep to 3.2 over 225.3-305.3, 3.2 to x 350, smoothstep
        /// to 2.5 over x 350-375.3.</summary>
        public static double TongueHalfWidth(double x)
        {
            double w = 75.0 + (3.2 - 75.0) * NocMath.Smoothstep(225.3, 305.3, x);
            return w + (2.5 - 3.2) * NocMath.Smoothstep(350.0, 375.3, x);
        }

        /// <summary>Tongue lane: min(12.0, max(2.15, w(x) - 2.0)).</summary>
        public static double TongueLane(double x) => Math.Min(TongueMax, Math.Max(BridgeHalfWidth, TongueHalfWidth(x) - TongueMargin));

        /// <summary>Walkable half-width of the lane at progress s (m), for the player's centre.</summary>
        public static double LaneHalfWidth(double s)
        {
            if (s <= 396.0) return NocMath.Knots(KnotS, KnotV, s);
            if (s < 560.0)
            {
                double tongue = TongueLane(s - NocturneConstants.EastLegOffset);
                if (s < 404.0) return NocMath.Lerp(11.0, tongue, NocMath.Smoothstep(396.0, 404.0, s));
                return tongue;
            }
            return NocMath.Knots(KnotS2, KnotV2, s);
        }

        /// <summary>Centre-line point at s (y = 0, the walk plane of 14.4).</summary>
        public static Vector3 CentreAt(double s)
        {
            Path.PointAt(s, out double x, out double z);
            return new Vector3((float)x, 0f, (float)z);
        }

        /// <summary>World point of the corridor coordinate (s, lateral to the right), y = height.</summary>
        public static Vector3 CorridorToWorld(double s, double lateral, double height)
        {
            Path.CorridorPoint(s, lateral, out double x, out double z);
            return new Vector3((float)x, (float)height, (float)z);
        }

        /// <summary>Progress s of a world point (nearest centre-line point, searched around hintS, window 25 m; whole path when far).</summary>
        public static double ProjectS(double x, double z, double hintS, out double distance)
        {
            double s = Path.Project(x, z, hintS, 25.0, out distance);
            if (distance > 15.0) s = Path.Project(x, z, hintS, -1.0, out distance);
            return s;
        }

        /// <summary>How far (m) a world point lies outside the lane union (0 inside). Uses the projection on the centre line.</summary>
        public static double DistanceOutsideLanes(Vector3 p, double hintS)
        {
            double s = ProjectS(p.x, p.z, hintS, out _);
            double lat = Path.Lateral(p.x, p.z, s);
            double h = LaneHalfWidth(s);
            double outside = Math.Abs(lat) - h;
            // before the start and beyond the goal (the end caps)
            if (s <= 0.0) { Path.TangentAt(0.0, out double tx, out double tz); outside = Math.Max(outside, -(p.x * tx + p.z * tz)); }
            else if (s >= Path.Length)
            {
                Path.PointAt(Path.Length, out double ex, out double ez);
                Path.TangentAt(Path.Length, out double tx, out double tz);
                outside = Math.Max(outside, (p.x - ex) * tx + (p.z - ez) * tz);
            }
            return Math.Max(0.0, outside);
        }

        public static double DistanceOutsideLanes(Vector3 p) => DistanceOutsideLanes(p, 0.0);

        /// <summary>Sample points along both lane edges and the centre every `step` metres (QA, wall bot planning), inset metres inside.</summary>
        public static int LaneSamplePoints(double step, double inset, Vector3[] buffer)
        {
            int n = 0;
            int count = (int)Math.Floor(Path.Length / step) + 1;
            for (int i = 0; i < count && n + 3 <= buffer.Length; i++)
            {
                double s = Math.Min(Path.Length, i * step);
                double h = Math.Max(0.0, LaneHalfWidth(s) - inset);
                buffer[n++] = CorridorToWorld(s, -h, 0.0);
                buffer[n++] = CorridorToWorld(s, 0.0, 0.0);
                buffer[n++] = CorridorToWorld(s, h, 0.0);
            }
            return n;
        }
    }
}
