// NocRainMath: the C# side of the rain pass (MASTER_PROMPT 14.7 H5, D-005, R2 1.3-1.7). Two parts:
//   * the per-frame state (NocRainState): rho (the 14.2.4 schedule, or the -rain X override), the drift X(t) = integral of
//     9 slope dt integrated in double with fixed 1/240 s steps (slope = lerp(0.10 sin(0.05 t), tan(0.28) sin(2 pi t / 17), gust(s)),
//     gust = the Rule-P weight of beat 4; a jump of the rain clock (captures, -clock) restarts from the closed form), and the
//     per-layer integer + fraction scroll phases handed to the shader;
//   * the double-precision mirror of NocRain.shader's layer maths (cells, stagger, slots, presence, drop centres, the projected
//     streak length and width) that RainTests uses for the H5 count / coverage / length checks and the seam and NaN checks.
// No allocation per frame.
using System;

namespace Megastructure
{
    public static class NocRainMath
    {
        public const double TwoPi = 2.0 * Math.PI;

        /// <summary>Cell width (arc, m) and height (m) of layer radius z.</summary>
        public static double CellW(double z) => z * TwoPi / NocturneConstants.RainCells;
        public static double CellH(double z) => z * NocturneConstants.RainCellH;

        /// <summary>Streak slope at rain time t for gust weight g (beat 4).</summary>
        public static double Slope(double t, double g) =>
            (1.0 - g) * NocturneConstants.RainTiltAmp * Math.Sin(NocturneConstants.RainTiltRate * t)
            + g * Math.Tan(NocturneConstants.RainGust) * Math.Sin(TwoPi * t / NocturneConstants.RainGustPeriod);

        /// <summary>Closed-form drift X(t) = integral_0^t 9 slope for a constant gust weight g (m).</summary>
        public static double DriftClosed(double t, double g)
        {
            double v = NocturneConstants.RainFallSpeed;
            double a = NocturneConstants.RainTiltAmp / NocturneConstants.RainTiltRate * (1.0 - Math.Cos(NocturneConstants.RainTiltRate * t));
            double b = Math.Tan(NocturneConstants.RainGust) * NocturneConstants.RainGustPeriod / TwoPi * (1.0 - Math.Cos(TwoPi * t / NocturneConstants.RainGustPeriod));
            return v * ((1.0 - g) * a + g * b);
        }

        /// <summary>Gust weight of beat 4 (Rule P, 24 m around s 560 and 880).</summary>
        public static double Gust(double s)
        {
            double w = 0.5 * NocturneConstants.BlendWindow;
            return NocMath.Smootherstep(560.0 - w, 560.0 + w, s) * (1.0 - NocMath.Smootherstep(880.0 - w, 880.0 + w, s));
        }

        /// <summary>Per-layer phases: drift in cells D = X / c_w (iBase = floor mod 21, uFrac), fall in cells 9 t / c_h (jBase mod 2^20, vFrac).</summary>
        public static void Phases(double z, double X, double t, out double iBase, out double uFrac, out double jBase, out double vFrac)
        {
            double D = X / CellW(z);
            double fD = Math.Floor(D);
            uFrac = D - fD;
            iBase = fD - NocturneConstants.RainCells * Math.Floor(fD / NocturneConstants.RainCells);
            double F = NocturneConstants.RainFallSpeed * t / CellH(z);
            double fF = Math.Floor(F);
            vFrac = F - fF;
            jBase = fF - 1048576.0 * Math.Floor(fF / 1048576.0);
        }

        // ---------------------------------------------------------------- shader mirror
        public static void Hash3(uint a, uint b, uint c, out double x, out double y, out double z)
        {
            NocHash.Pcg3d(ref a, ref b, ref c);
            x = a * (1.0 / 4294967296.0); y = b * (1.0 / 4294967296.0); z = c * (1.0 / 4294967296.0);
        }

        public static double Noise(double x, double y)
        {
            double fx = Math.Floor(x), fy = Math.Floor(y), tx = x - fx, ty = y - fy;
            tx = tx * tx * (3 - 2 * tx); ty = ty * ty * (3 - 2 * ty);
            int n = NocturneConstants.RainNoiseCells;
            double xm = fx - n * Math.Floor(fx / n);
            uint x0 = (uint)xm, x1 = (x0 + 1u) % (uint)n, y0 = (uint)Math.Max(fy, 0.0), y1 = y0 + 1u;
            Hash3(x0, y0, 977u, out double a, out _, out _); Hash3(x1, y0, 977u, out double b, out _, out _);
            Hash3(x0, y1, 977u, out double c, out _, out _); Hash3(x1, y1, 977u, out double d, out _, out _);
            double ab = a + (b - a) * tx, cd = c + (d - c) * tx;
            return ab + (cd - ab) * ty;
        }

        /// <summary>One drop of the shader's layer l: screen-space cell (iF, jF), slot k. Returns the presence a_k and the drop
        /// centre as (theta rad, tan el); the brightness factor 0.7 + 0.3 frac(7.31 r).</summary>
        public static double Drop(int l, double z, double iF, double jF, int k, double pScale, bool noise, double noiseX, double noiseY,
                                  double uFrac, double vFrac, double iBase, double jBase, out double theta, out double tanEl, out double bright)
        {
            int n = NocturneConstants.RainCells;
            double cw = CellW(z), ch = CellH(z);
            double im = iF - iBase;
            uint ic = (uint)(im - n * Math.Floor(im / n));
            Hash3(ic, 41u, (uint)l, out double stagger, out _, out _);
            uint jc = unchecked((uint)(int)(jF + jBase)) & 0xFFFFFu;
            double nz = Noise((iF + 0.5 + uFrac) * ((double)NocturneConstants.RainNoiseCells / n) + noiseX, noiseY);
            double pCell = pScale * (noise ? NocturneConstants.RainNoiseBase + NocturneConstants.RainNoiseAmp * nz : 1.0);
            Hash3(ic, jc, 8u * (uint)l + (uint)k, out double r0, out double r1, out double r2);
            double ak = NocMath.Clamp01((pCell - r0) / NocturneConstants.RainPresenceBand);
            double cx = NocturneConstants.RainMarginH + (cw - 2 * NocturneConstants.RainMarginH) * r1;
            double cy = NocturneConstants.RainMarginV + (ch - 2 * NocturneConstants.RainMarginV) * r2;
            // screen U = iF + uFrac + cx / cw (U = theta 21 / 2pi - uFrac); V = jF + cy / ch = tanEl / 0.8 + vFrac + stagger
            theta = (iF + cx / cw + uFrac) * TwoPi / n;
            tanEl = (jF + cy / ch - vFrac - stagger) * NocturneConstants.RainCellH;
            double fr = r0 * 7.31; fr -= Math.Floor(fr);
            bright = 0.7 + 0.3 * fr;
            return ak;
        }
    }

    /// <summary>Per-frame rain state (written by the AtmosphereDriver, read by NocRainPass).</summary>
    public static class NocRainState
    {
        /// <summary>Rain density rho of this frame: the -rain X override when the flag is given, else the 14.2.4 schedule.</summary>
        public static double Rho;
        /// <summary>Density noise on (off with -rain-noise-off: the H5 count test forces vnoise = 1).</summary>
        public static bool Noise = true;
        public static double Time, Slope, Drift, Gust;
        static double s_t = double.NaN;

        /// <summary>The -rain X value (0..1) when the flag is given, else NaN (D-020: -rain overrides rho, it does not multiply it).</summary>
        public static double Override => CliArgs.Has("-rain") ? CliArgs.Rain : double.NaN;

        /// <summary>Rain density at s: the -rain override, else the 14.2.4 schedule (picture, audio and QA use this one rule).</summary>
        public static double RhoAt(double s) => RhoAt(s, Override);

        /// <summary>Rain density at s for an explicit override (NaN: the schedule); the offline audio render uses it.</summary>
        public static double RhoAt(double s, double rainOverride) => double.IsNaN(rainOverride) ? NocSchedule.RainRho(s) : rainOverride;

        public static void Reset() { s_t = double.NaN; Drift = 0; }

        /// <summary>Advance to rain time t at progress s (fixed 1/240 s steps; a jump or a restart uses the closed form).</summary>
        public static void Update(double t, double s)
        {
            Rho = RhoAt(s);
            Noise = !CliArgs.Has("-rain-noise-off");
            Gust = NocRainMath.Gust(s);
            if (double.IsNaN(s_t) || t < s_t || t - s_t > 5.0)
                Drift = NocRainMath.DriftClosed(t, Gust);
            else
            {
                const double h = 1.0 / 240.0;
                double tau = s_t;
                while (tau < t)
                {
                    double dt = Math.Min(h, t - tau);
                    Drift += NocturneConstants.RainFallSpeed * NocRainMath.Slope(tau + 0.5 * dt, Gust) * dt;
                    tau += dt;
                }
            }
            s_t = t;
            Time = t;
            Slope = NocRainMath.Slope(t, Gust);
        }
    }
}
