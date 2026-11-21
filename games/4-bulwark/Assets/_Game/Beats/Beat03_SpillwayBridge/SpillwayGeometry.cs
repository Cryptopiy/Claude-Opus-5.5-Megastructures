// SpillwayGeometry: the numbers of beat 3 as pure functions (MASTER_PROMPT 14.5.3, 14.4, 14.12 "Spillway"), shared by
// SpillwayBuilder, SheetBuilder, the BwSheet material inputs and Beat03Tests. No UnityEngine state.
//   Piers k 0-14 at z 420 + 20 k .. 424 + 20 k (4 m) from the lip (x -6) to x 46, cap y +0.5 east of the sea face, rounded
//   cutwater nose (radius 2 m) at x 44..46; slots k 0-13 at z 424 + 20 k .. 440 + 20 k (16 m); sill y -14.
//   Draw-down: y -8.0 at x 46 to -11.5 at the lip, a smoothstep profile in x (zero slope at both ends: the sea meets it
//   flat at x 46, BwSeaCalm, and the sheet leaves the lip horizontally on the parabola); speed by Bernoulli
//   v = sqrt(0.8^2 + 2 g (-8 - y)) (8.33 m/s at the lip); travel time from x 46 by the trapezoid rule (texture advection).
//   Sheet: x = -6 - 8.3 t, y = -11.5 - 4.9 t^2, t 0..4.7 s; thickness by continuity h = q / v (q = 8.3 x 2.5 at the lip).
//   Radial gates: radius 14 m, trunnion (-2.0, -1.8) under the deck on the pier walls; raised 5 m, the lip at y -9.0 is at
//   x 10.0 and the plate top (y +0.1) at x 11.9: the plate spans x 10.0..12.0 (14.12 "x 9-12" with its ribs and arms).
using System;

namespace Megastructure.Beats
{
    public static class SpillwayGeometry
    {
        public const double Z0 = BulwarkConst.SpillwayZ0, Z1 = BulwarkConst.SpillwayZ1;    // 420, 704
        public const double LipX = BulwarkConst.LipX, LipY = BulwarkConst.LipY;            // -6, -11.5
        public const double MouthX = BulwarkConst.PierEastX, MouthY = BulwarkConst.DrawdownY0;   // 46, -8
        public const double SillY = BulwarkConst.SillY;                                    // -14
        public const double G = 9.81;
        public const double NoseRadius = 2.0;
        public const double DeckTop = 0.0, DeckUnder = -0.6;
        /// <summary>Solid deck edges of the grated spans (the edge girders); the grating spans x -5.6 .. 5.3, which covers the
        /// whole road lane (capsule centres x -5.4 .. 5.1) except the outer 0.15 / 0.15 m, standing on the girder tops.
        /// Girder depth 0.8 m (span / 20 for the 16 m bay): from the KS09 eye (x -4, y 1.7) a ray clears its inner face
        /// below at a depression above atan((1.7 + 0.8) / 1.6) = 57.4 deg (64.2 deg with a 1.6 m girder), which opens the
        /// look over the lip (the lip itself is at 81.4 deg) to most of the lower half of the KS09 frame.</summary>
        public const double GirderLeeX0 = -6.0, GirderLeeX1 = -5.6, GirderSeaX0 = 5.3, GirderSeaX1 = 6.0, GirderDepth = 0.8;
        public const double GratingX0 = GirderLeeX1, GratingX1 = GirderSeaX0;
        /// <summary>Transverse floor beams under the grating every 2 m (the 40 mm bearing bars span 2 m between them).</summary>
        public const double FloorBeamPitch = 2.0, FloorBeamDepth = 0.40, FloorBeamFlange = 0.20;
        /// <summary>Radial gate (14.12): radius 14, trunnion on the pier walls under the deck, raised 5 m above the sill.</summary>
        public const double GateRadius = BulwarkConst.RadialGateRadius, TrunnionX = -2.0, TrunnionY = -1.8;
        public const double GateLipY = BulwarkConst.SillY + BulwarkConst.RadialGateRaise;  // -9
        /// <summary>Angular span of the skin plate (deg): closed it reaches from the sill (-14) to y -7 (1 m above the sea).</summary>
        public static readonly double GateSpanDeg = (Math.Asin((-7.0 - TrunnionY) / GateRadius) - Math.Asin((SillY - TrunnionY) / GateRadius)) / BwMath.Deg;

        // ------------------------------------------------------------------ piers and slots
        public static double PierZ0(int k) => BulwarkConst.PierZ0 + BulwarkConst.PierPitch * k;
        public static double PierZ1(int k) => PierZ0(k) + BulwarkConst.PierLength;
        public static double SlotZ0(int k) => BulwarkConst.SlotZ0 + BulwarkConst.PierPitch * k;
        public static double SlotZ1(int k) => SlotZ0(k) + BulwarkConst.SlotLength;
        /// <summary>Slot index containing z, or -1 over a pier / outside.</summary>
        public static int SlotAt(double z)
        {
            for (int k = 0; k < BulwarkConst.SlotCount; k++) if (z >= SlotZ0(k) && z < SlotZ1(k)) return k;
            return -1;
        }

        // ------------------------------------------------------------------ draw-down water
        /// <summary>Normalised distance from the mouth: 0 at x 46, 1 at the lip.</summary>
        public static double U(double x) => BwMath.Clamp01((MouthX - x) / (MouthX - LipX));
        /// <summary>Water surface height at x (smoothstep draw-down, -8.0 at x 46 to -11.5 at the lip).</summary>
        public static double WaterY(double x)
        {
            double u = U(x);
            return MouthY + (LipY - MouthY) * u * u * (3.0 - 2.0 * u);
        }
        /// <summary>Surface slope dy/dx.</summary>
        public static double WaterSlope(double x)
        {
            double u = U(x);
            return -(LipY - MouthY) * 6.0 * u * (1.0 - u) / (MouthX - LipX);
        }
        /// <summary>Surface speed by Bernoulli from 0.8 m/s at the mouth (8.33 m/s at the lip).</summary>
        public static double Speed(double x) => Math.Sqrt(BulwarkConst.FlowSpeed0 * BulwarkConst.FlowSpeed0 + 2.0 * G * Math.Max(0.0, MouthY - WaterY(x)));

        static double[] s_tau;
        const int TauSteps = 520;   // 0.1 m

        /// <summary>Travel time (s) of a surface parcel from the mouth (x 46) to x (trapezoid rule on 0.1 m steps).</summary>
        public static double TravelTime(double x)
        {
            if (s_tau == null)
            {
                var t = new double[TauSteps + 1];
                double dx = (MouthX - LipX) / TauSteps;
                for (int i = 1; i <= TauSteps; i++)
                {
                    double xa = MouthX - (i - 1) * dx, xb = MouthX - i * dx;
                    t[i] = t[i - 1] + 0.5 * dx * (1.0 / Speed(xa) + 1.0 / Speed(xb));
                }
                s_tau = t;
            }
            double f = BwMath.Clamp((MouthX - x) / (MouthX - LipX), 0.0, 1.0) * TauSteps;
            int j = Math.Min(TauSteps - 1, (int)Math.Floor(f));
            return s_tau[j] + (s_tau[j + 1] - s_tau[j]) * (f - j);
        }
        /// <summary>Travel time from the mouth to the lip (s).</summary>
        public static double LipTravelTime => TravelTime(LipX);

        // ------------------------------------------------------------------ the near sheets
        public const double SheetT1 = BulwarkConst.SheetT1;                // 4.7 s
        public const double LipThickness = BulwarkConst.SheetLipThickness; // 2.5 m
        public static double SheetX(double t) => LipX - BulwarkConst.LipSpeed * t;
        public static double SheetY(double t) => LipY - BulwarkConst.SheetGravityHalf * t * t;
        /// <summary>Drop below the lip (m) at flight time t.</summary>
        public static double SheetDrop(double t) => BulwarkConst.SheetGravityHalf * t * t;
        /// <summary>Speed along the parabola (m/s).</summary>
        public static double SheetSpeed(double t) => Math.Sqrt(BulwarkConst.LipSpeed * BulwarkConst.LipSpeed + (G * t) * (G * t));
        /// <summary>Sheet thickness by continuity (unit discharge q = 8.3 x 2.5 = 20.75 m^2/s): 2.5 m at the lip, 0.44 m at
        /// t 4.7 s (docs/RESEARCH.md B 3.1).</summary>
        public static double SheetThickness(double t) => BulwarkConst.LipSpeed * LipThickness / SheetSpeed(t);
        /// <summary>Unit tangent of the centre line (x, y).</summary>
        public static void SheetTangent(double t, out double tx, out double ty)
        {
            double vx = -BulwarkConst.LipSpeed, vy = -2.0 * BulwarkConst.SheetGravityHalf * t, l = Math.Sqrt(vx * vx + vy * vy);
            tx = vx / l; ty = vy / l;
        }
        /// <summary>Outer normal (up and away from the dam) of the centre line (x, y).</summary>
        public static void SheetNormal(double t, out double nx, out double ny)
        {
            SheetTangent(t, out double tx, out double ty);
            nx = ty; ny = -tx;          // rotate the tangent by -90 deg: (tx, ty) = (-1, 0) at the lip -> (0, 1)
            if (ny < 0) { nx = -nx; ny = -ny; }
        }
        /// <summary>Arc length of the centre line from the lip to t (numerical, 200 steps).</summary>
        public static double SheetArc(double t)
        {
            int n = 200; double s = 0.0, h = t / n;
            for (int i = 0; i < n; i++) s += h * 0.5 * (SheetSpeed(i * h) + SheetSpeed((i + 1) * h));
            return s;
        }
        /// <summary>Flight time at which the sheet has dropped D metres.</summary>
        public static double TimeAtDrop(double drop) => Math.Sqrt(Math.Max(drop, 0.0) / BulwarkConst.SheetGravityHalf);

        /// <summary>Alpha-to-coverage breakup window of the mesh sheet (flight time, s): holes open from 2.5 s (D 31 m) and
        /// the mesh is gone by 4.55 s (D 101 m); the analytic curtain fades in over D 70-95 m (MistVolumeMath).</summary>
        public const double BreakupT0 = 2.5, BreakupT1 = 4.55;
    }
}
