// NocMath: pure double-precision helpers (Thread's ThreadMath minus the helix, thread law, ascent and sun; plus The Ring's
// RingMath hashes, value noise, critically damped followers and slew, in double). No UnityEngine dependency: EditMode
// tests, the offline audio renderer and the QA projection call it directly. Path geometry lives in FilletPath / NocLanes,
// the per-s parameter schedules in NocSchedule.
using System;

namespace Megastructure
{
    public struct D3
    {
        public double x, y, z;
        public D3(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
        public static D3 operator +(D3 a, D3 b) => new D3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static D3 operator -(D3 a, D3 b) => new D3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static D3 operator *(D3 a, double k) => new D3(a.x * k, a.y * k, a.z * k);
        public double Length => Math.Sqrt(x * x + y * y + z * z);
        public D3 Normalized { get { double l = Length; return l > 1e-12 ? this * (1.0 / l) : new D3(0, 0, 0); } }
        public static double Dot(D3 a, D3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static D3 Cross(D3 a, D3 b) => new D3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        public override string ToString() => $"({x:F3}, {y:F3}, {z:F3})";
    }

    public static class NocMath
    {
        public const double Deg = Math.PI / 180.0;

        // ------------------------------------------------------------------ easing
        public static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
        public static double Clamp(double x, double a, double b) => x < a ? a : (x > b ? b : x);
        public static double Lerp(double a, double b, double t) => a + (b - a) * t;
        public static double InverseLerp(double a, double b, double x) => Math.Abs(b - a) < 1e-12 ? 0.0 : Clamp01((x - a) / (b - a));
        public static double Smoothstep(double e0, double e1, double x)
        {
            double t = Clamp01((x - e0) / (e1 - e0));
            return t * t * (3 - 2 * t);
        }
        public static double Smootherstep01(double x)
        {
            double t = Clamp01(x);
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        public static double Smootherstep(double e0, double e1, double x) => Smootherstep01((x - e0) / (e1 - e0));

        /// <summary>Wrap an angle in degrees to [-180, 180).</summary>
        public static double WrapDeg(double a)
        {
            a %= 360.0;
            if (a >= 180.0) a -= 360.0;
            else if (a < -180.0) a += 360.0;
            return a;
        }

        /// <summary>Shortest signed difference b - a in degrees, in [-180, 180).</summary>
        public static double DeltaAngle(double a, double b) => WrapDeg(b - a);

        // ------------------------------------------------------------------ beats
        public static int BeatIndex(double s)
        {
            var b = NocturneConstants.BeatBoundaries;
            for (int i = 1; i < b.Length - 1; i++) if (s < b[i]) return i - 1;
            return NocturneConstants.BeatCount - 1;
        }

        /// <summary>Per-beat table value with smootherstep blends of `window` metres centred on each boundary (14.6 Rule P).</summary>
        public static double BeatBlend(double[] perBeat, double s, double window = NocturneConstants.BlendWindow)
        {
            var b = NocturneConstants.BeatBoundaries;
            double v = perBeat[0];
            for (int i = 1; i < NocturneConstants.BeatCount; i++)
                v += (perBeat[i] - perBeat[i - 1]) * Smootherstep(b[i] - window * 0.5, b[i] + window * 0.5, s);
            return v;
        }

        /// <summary>Knot schedule (s_k, v_k): constant outside, smootherstep between consecutive knots (14.2.4 curves).</summary>
        public static double Knots(double[] ks, double[] kv, double s)
        {
            int n = ks.Length;
            if (s <= ks[0]) return kv[0];
            if (s >= ks[n - 1]) return kv[n - 1];
            int i = 0;
            while (i < n - 2 && s >= ks[i + 1]) i++;
            return Lerp(kv[i], kv[i + 1], Smootherstep(ks[i], ks[i + 1], s));
        }

        /// <summary>Largest |dv/ds| of a knot schedule (smootherstep peak slope 1.875 x step / span), for slew checks.</summary>
        public static double KnotsMaxSlope(double[] ks, double[] kv)
        {
            double m = 0;
            for (int i = 0; i + 1 < ks.Length; i++)
            {
                double span = ks[i + 1] - ks[i];
                if (span <= 0) continue;
                m = Math.Max(m, 1.875 * Math.Abs(kv[i + 1] - kv[i]) / span);
            }
            return m;
        }

        // ------------------------------------------------------------------ hashes (The Ring RingMath, PCG; twin of the HLSL hash)
        public static uint Pcg(uint v)
        {
            unchecked
            {
                uint state = v * 747796405u + 2891336453u;
                uint word = ((state >> (int)((state >> 28) + 4u)) ^ state) * 277803737u;
                return (word >> 22) ^ word;
            }
        }
        public static uint Pcg2(int x, int y) { unchecked { return Pcg((uint)x + Pcg((uint)y)); } }
        public static uint Pcg3(int x, int y, int z) { unchecked { return Pcg((uint)x + Pcg((uint)y + Pcg((uint)z))); } }

        /// <summary>Uniform [0, 1) from a hash (24 high-quality bits).</summary>
        public static double U01(uint h) => (h >> 8) * (1.0 / 16777216.0);

        /// <summary>2D value noise in [0, 1] with quintic interpolation.</summary>
        public static double ValueNoise(double x, double y)
        {
            double fx = Math.Floor(x), fy = Math.Floor(y);
            int ix = (int)fx, iy = (int)fy;
            double tx = x - fx, ty = y - fy;
            double ux = tx * tx * tx * (tx * (tx * 6 - 15) + 10);
            double uy = ty * ty * ty * (ty * (ty * 6 - 15) + 10);
            double a = U01(Pcg2(ix, iy)), b = U01(Pcg2(ix + 1, iy));
            double c = U01(Pcg2(ix, iy + 1)), d = U01(Pcg2(ix + 1, iy + 1));
            return Lerp(Lerp(a, b, ux), Lerp(c, d, ux), uy);
        }

        // ------------------------------------------------------------------ followers (The Ring, double)
        /// <summary>Exact critically damped step toward a constant target with natural frequency omega (rad/s).</summary>
        public static double CriticalDamp(double x, ref double v, double target, double omega, double dt)
        {
            double e = Math.Exp(-omega * dt);
            double d = x - target;
            double temp = (v + omega * d) * dt;
            v = (v - omega * temp) * e;
            return target + (d + temp) * e;
        }

        /// <summary>Angle variant of <see cref="CriticalDamp"/> that takes the shortest way round (degrees).</summary>
        public static double CriticalDampAngle(double x, ref double v, double target, double omega, double dt)
        {
            double t = x + DeltaAngle(x, target);
            return CriticalDamp(x, ref v, t, omega, dt);
        }

        public static double Slew(double current, double target, double maxRate, double dt)
        {
            double m = maxRate * dt;
            double d = target - current;
            if (d > m) d = m; else if (d < -m) d = -m;
            return current + d;
        }

        /// <summary>Thread AutoWalker.LimitRate (IMPORT_PLAN 4.4 b): limits the step x - x0 to maxRate dt and pins the
        /// follower velocity to the limit while it binds.</summary>
        public static void LimitRate(ref double x, ref double v, double x0, double maxRate, double dt)
        {
            double step = x - x0, lim = maxRate * dt;
            if (step > lim) { x = x0 + lim; v = maxRate; }
            else if (step < -lim) { x = x0 - lim; v = -maxRate; }
        }

        // ------------------------------------------------------------------ path facade (Thread's ThreadMath path API, for the imported QA)
        // The path is NocLanes.Path (the 14.4 filleted centre line, s = arc length); the walk plane is y 0 everywhere (14.4).
        /// <summary>Heading at s in RADIANS (Thread convention: callers divide by Deg).</summary>
        public static double Heading(double s) => NocLanes.Path.HeadingDeg(Clamp(s, 0, NocLanes.Path.Length)) * Deg;
        public static void Tangent(double s, out double tx, out double tz) => NocLanes.Path.TangentAt(s, out tx, out tz);
        public static void Right(double s, out double rx, out double rz) => NocLanes.Path.RightAt(s, out rx, out rz);
        /// <summary>Centre-line point at s (y = 0).</summary>
        public static D3 PathPoint(double s) { NocLanes.Path.PointAt(s, out double x, out double z); return new D3(x, 0.0, z); }
        /// <summary>World point of the corridor coordinate (s, lateral to the right, height above the walk plane).</summary>
        public static D3 CorridorToWorld(double s, double lateral, double height)
        {
            NocLanes.Path.CorridorPoint(s, lateral, out double x, out double z);
            return new D3(x, height, z);
        }
        /// <summary>Walkable half-width at s (NocLanes.LaneHalfWidth).</summary>
        public static double HalfWidth(double s) => NocLanes.LaneHalfWidth(s);
        /// <summary>Progress of a world position (nearest centre-line point near the hint).</summary>
        public static double ProjectToPath(D3 pos, double hint) => NocLanes.ProjectS(pos.x, pos.z, hint, out _);
        /// <summary>Walk-plane height and grade (flat world, 14.4).</summary>
        public static double PathY(double s) => 0.0;
        public static double PathGrade(double s) => 0.0;
        /// <summary>vFOV schedule (NocSchedule.Fov).</summary>
        public static double Fov(double s) => NocSchedule.Fov(s);

        // ------------------------------------------------------------------ colour
        /// <summary>Rec.709 luma of encoded (display) values, as 14.10 measures it.</summary>
        public static double Luma709(double r, double g, double b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }
}
