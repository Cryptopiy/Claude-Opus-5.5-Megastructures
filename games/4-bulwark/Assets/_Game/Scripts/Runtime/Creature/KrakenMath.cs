// KrakenMath: pure maths of the kraken rig (Addendum A1, kraken owner), shared by the rig, the tube generator and the tests.
// Twins of Assets/_Game/Shaders/Include/BwCreatureSkin.hlsl: ControlIndex (KrControlIndex), CatmullRom (KrCatmull), TileV
// (KrTileV). No allocation.
using UnityEngine;

namespace Megastructure.Creature
{
    public static class KrakenMath
    {
        /// <summary>Fractional control-point index of arc u on a limb of length L with K points: denser towards the tip,
        /// index = (K - 1) (a u / L + (1 - a) (exp(u / lambda) - 1) / (exp(L / lambda) - 1)).</summary>
        public static double ControlIndex(double u, double L, double lambda, double a, int K)
        {
            L = System.Math.Max(L, 1.0); lambda = System.Math.Max(lambda, 1.0);
            double x = Clamp01(u / L);
            double e = (System.Math.Exp(System.Math.Min(u, L) / lambda) - 1.0) / System.Math.Max(System.Math.Exp(L / lambda) - 1.0, 1e-6);
            return (K - 1) * (a * x + (1.0 - a) * Clamp01(e));
        }

        /// <summary>Inverse of <see cref="ControlIndex"/> by bisection (monotonic): the arc u of control point i.</summary>
        public static double ArcOfIndex(double index, double L, double lambda, double a, int K)
        {
            double lo = 0.0, hi = L;
            for (int it = 0; it < 60; it++)
            {
                double mid = 0.5 * (lo + hi);
                if (ControlIndex(mid, L, lambda, a, K) < index) lo = mid; else hi = mid;
            }
            return 0.5 * (lo + hi);
        }

        public static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);

        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        /// <summary>Normalised linear quaternion blend with the shorter-arc sign (the shader's nlerp).</summary>
        public static Quaternion Nlerp(Quaternion a, Quaternion b, float t)
        {
            float d = a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
            if (d < 0f) { b.x = -b.x; b.y = -b.y; b.z = -b.z; b.w = -b.w; }
            var q = new Quaternion(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t, a.w + (b.w - a.w) * t);
            float n = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            if (n < 1e-12f) return Quaternion.identity;
            float k = 1f / n;
            return new Quaternion(q.x * k, q.y * k, q.z * k, q.w * k);
        }

        /// <summary>Taper exponent of the procedural tentacles (A1 1.2: d(u) = 21 (1 - (u - 264) / 1036)^1.43).</summary>
        public const double TaperPower = 1.43;
        /// <summary>Largest taper fraction used by the analytic tile coordinate (the radius floor takes over near the tip).</summary>
        public const double TaperXMax = 0.985;

        /// <summary>Radius of the power-law taper r0 (1 - (u - u0) / span)^1.43 (no floor).</summary>
        public static double TaperRadius(double u, double u0, double r0, double span)
        {
            double x = System.Math.Min((u - u0) / System.Math.Max(span, 1.0), TaperXMax);
            return r0 * System.Math.Pow(1.0 - x, TaperPower);
        }

        /// <summary>Tentacle tile v at arc u: the integral of du / (tile r(u)) for the power-law taper,
        /// span / (r0 tile) ((1 - x)^(1 - p) - 1) / (p - 1), x = (u - u0) / span (clamped at TaperXMax).</summary>
        public static double TileV(double u, double u0, double r0, double span, double tileRadii)
        {
            double sp = System.Math.Max(span, 1.0);
            double x = System.Math.Min((u - u0) / sp, TaperXMax);
            double p = TaperPower;
            return sp / (System.Math.Max(r0, 0.05) * System.Math.Max(tileRadii, 1.0)) * (System.Math.Pow(1.0 - x, 1.0 - p) - 1.0) / (p - 1.0);
        }

        /// <summary>smootherstep on [a, b].</summary>
        public static float Smoother(float a, float b, float x)
        {
            float t = Mathf.Clamp01((x - a) / (b - a));
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        public static float Smooth(float a, float b, float x)
        {
            float t = Mathf.Clamp01((x - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        /// <summary>Phase in [0, 1) of an unwrapped clock t (double) for a period (s): no jump when the float clock wraps.</summary>
        public static float Phase(double t, double period)
        {
            double p = t / period;
            return (float)(p - System.Math.Floor(p));
        }

        /// <summary>sin(2 pi (x - t / T) + phi) with the time part reduced in double first (unwrapped ambient clock).</summary>
        public static float Wave(float x, double t, double period, float phi)
        {
            return Mathf.Sin(2f * Mathf.PI * (x - Phase(t, period)) + phi);
        }

        /// <summary>Deterministic hash in [0, 1) of an integer (PCG, the BwCommon twin).</summary>
        public static float Hash01(uint v)
        {
            uint state = v * 747796405u + 2891336453u;
            uint word = ((state >> (int)((state >> 28) + 4u)) ^ state) * 277803737u;
            uint h = (word >> 22) ^ word;
            return (h >> 8) * (1f / 16777216f);
        }
    }
}
