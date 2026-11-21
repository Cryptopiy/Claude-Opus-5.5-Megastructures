// BwMath: generic maths shared by the framework.
//   * D3 and the double-precision easing helpers are extracted from Thread's ThreadMath (its path, helix,
//     ascent, fog and FOV functions are NOT imported: the path API is BwPath, the schedule BulwarkSchedule).
//   * The float helpers (Slew, CriticalDamp, CriticalDampAngle, Pcg, U01, ValueNoise, Smax) come from The
//     Ring's RingMath; Shaders/Include/BwCommon.hlsl holds the HLSL twins of Pcg/ValueNoise.
// No game content. Pure functions; no allocation.
using System;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Double-precision 3-vector (QA projection, shot plans, bots).</summary>
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
        public Vector3 ToVector3() => new Vector3((float)x, (float)y, (float)z);
        public override string ToString() => $"({x:F3}, {y:F3}, {z:F3})";
    }

    public static class BwMath
    {
        public const double Deg = Math.PI / 180.0;

        // ------------------------------------------------------------------ double easing (Thread)
        public static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
        public static double Clamp(double x, double a, double b) => x < a ? a : (x > b ? b : x);
        public static double Lerp(double a, double b, double t) => a + (b - a) * t;
        public static double Smoothstep(double e0, double e1, double x)
        {
            double d = e1 - e0;
            double t = Clamp01(Math.Abs(d) < 1e-300 ? (x >= e1 ? 1 : 0) : (x - e0) / d);
            return t * t * (3 - 2 * t);
        }
        public static double Smootherstep01(double x)
        {
            double t = Clamp01(x);
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        public static double Smootherstep(double e0, double e1, double x)
        {
            double d = e1 - e0;
            return Smootherstep01(Math.Abs(d) < 1e-300 ? (x >= e1 ? 1 : 0) : (x - e0) / d);
        }
        /// <summary>Derivative of Smootherstep01 with respect to its argument (for feed-forward rates).</summary>
        public static double Smootherstep01Slope(double x)
        {
            if (x <= 0 || x >= 1) return 0;
            return 30 * x * x * (x - 1) * (x - 1);
        }
        /// <summary>Wraps an angle to (-180, 180].</summary>
        public static double WrapDeg(double a)
        {
            a %= 360.0;
            if (a > 180.0) a -= 360.0;
            else if (a <= -180.0) a += 360.0;
            return a;
        }
        /// <summary>Shortest signed difference b - a in degrees.</summary>
        public static double DeltaDeg(double a, double b) => WrapDeg(b - a);

        // ------------------------------------------------------------------ float helpers (The Ring)
        public static float Saturate(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);

        /// <summary>HLSL smoothstep semantics (e0 may be greater than e1).</summary>
        public static float Smoothstep(float e0, float e1, float x)
        {
            float d = e1 - e0;
            float t = Saturate(Mathf.Abs(d) < 1e-12f ? (x >= e1 ? 1f : 0f) : (x - e0) / d);
            return t * t * (3f - 2f * t);
        }

        public static float Smootherstep01(float t)
        {
            t = Saturate(t);
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        public static float Smootherstep(float e0, float e1, float x)
        {
            float d = e1 - e0;
            return Smootherstep01(Mathf.Abs(d) < 1e-12f ? (x >= e1 ? 1f : 0f) : (x - e0) / d);
        }

        /// <summary>Smooth maximum: (a + b + sqrt((a - b)^2 + k^2)) / 2.</summary>
        public static float Smax(float a, float b, float k = 0.03f)
        {
            if (float.IsNegativeInfinity(a)) return b;
            if (float.IsNegativeInfinity(b)) return a;
            float d = a - b;
            return 0.5f * (a + b + Mathf.Sqrt(d * d + k * k));
        }

        public static float Lerp(float a, float b, float t) => a + (b - a) * t;

        // ---------------- hashes (PCG, Jarzynski & Olano 2020)
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

        /// <summary>Uniform [0, 1) from a hash (24 high bits).</summary>
        public static float U01(uint h) => (h >> 8) * (1f / 16777216f);

        /// <summary>2D value noise in [0, 1] with quintic interpolation (twin of BwValueNoise in HLSL).</summary>
        public static float ValueNoise(float x, float y)
        {
            float fx = Mathf.Floor(x), fy = Mathf.Floor(y);
            int ix = (int)fx, iy = (int)fy;
            float tx = x - fx, ty = y - fy;
            float ux = tx * tx * tx * (tx * (tx * 6f - 15f) + 10f);
            float uy = ty * ty * ty * (ty * (ty * 6f - 15f) + 10f);
            float a = U01(Pcg2(ix, iy)), b = U01(Pcg2(ix + 1, iy));
            float c = U01(Pcg2(ix, iy + 1)), d = U01(Pcg2(ix + 1, iy + 1));
            return Lerp(Lerp(a, b, ux), Lerp(c, d, ux), uy);
        }

        // ---------------- followers
        /// <summary>Critically damped spring step (exact for a constant target). Returns the new value, updates velocity.</summary>
        public static float CriticalDamp(float x, ref float v, float target, float omega, float dt)
        {
            float o = omega * dt;
            float e = Mathf.Exp(-o);
            float d = x - target;
            float temp = (v + omega * d) * dt;
            v = (v - omega * temp) * e;
            return target + (d + temp) * e;
        }

        /// <summary>Double variant of <see cref="CriticalDamp(float, ref float, float, float, float)"/>.</summary>
        public static double CriticalDamp(double x, ref double v, double target, double omega, double dt)
        {
            double e = Math.Exp(-omega * dt);
            double d = x - target;
            double temp = (v + omega * d) * dt;
            v = (v - omega * temp) * e;
            return target + (d + temp) * e;
        }

        /// <summary>Angle variant that takes the shortest way round (degrees).</summary>
        public static float CriticalDampAngle(float x, ref float v, float target, float omega, float dt)
        {
            float t = x + Mathf.DeltaAngle(x, target);
            return CriticalDamp(x, ref v, t, omega, dt);
        }

        public static double CriticalDampAngle(double x, ref double v, double target, double omega, double dt)
        {
            double t = x + DeltaDeg(x, target);
            return CriticalDamp(x, ref v, t, omega, dt);
        }

        public static float Slew(float current, float target, float maxRate, float dt)
        {
            float m = maxRate * dt;
            float d = target - current;
            if (d > m) d = m; else if (d < -m) d = -m;
            return current + d;
        }

        public static double Slew(double current, double target, double maxRate, double dt)
        {
            double m = maxRate * dt;
            double d = target - current;
            if (d > m) d = m; else if (d < -m) d = -m;
            return current + d;
        }
    }
}
