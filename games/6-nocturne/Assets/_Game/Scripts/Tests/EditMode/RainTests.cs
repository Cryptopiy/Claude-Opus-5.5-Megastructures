// RainTests: EditMode tests of the rain pass maths (MASTER_PROMPT 14.7 H5, D-005, R2 1.10) on NocRainMath, the double mirror of
// NocRain.shader's layer geometry:
//   H5 count and coverage: static camera at pitch 0, vFOV 66, 1920 x 1080, rho 1 with the density noise forced to 1
//   (-rain-noise-off), 120 frames at 60 fps: 140-210 streaks in every frame (D-005: slot probability 0.9), energy coverage
//   0.6-1.0 %, no streak shorter than 10 px outside the zenith cone; the zenith fade removes each layer before its dashes are
//   shorter than 3 px; the cell index wraps continuously across theta = +-pi (21 whole cells: no seam at south); the drift
//   integrated in 1/240 s steps matches its closed form within 1 mm over 1 000 s; the noise is periodic around the circle; the
//   Look v2 tint (30 % of the local hue: no coloured sparks in front of saturated accents).
using System;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class RainTests
    {
        const int W = 1920, H = 1080;
        static readonly double F = (H / 2.0) / Math.Tan(33.0 * Math.PI / 180.0);

        /// <summary>Projects a direction (camera yaw 0, pitch 0) to pixels; false when outside the frame.</summary>
        static bool Project(double dx, double dy, double dz, out double px, out double py)
        {
            px = py = 0;
            if (dz <= 1e-6) return false;
            px = W / 2.0 + F * dx / dz; py = H / 2.0 - F * dy / dz;
            return px >= 0 && px < W && py >= 0 && py < H;
        }

        static void Dir(double theta, double tanEl, out double x, out double y, out double z)
        {
            double h = 1.0 / Math.Sqrt(1.0 + tanEl * tanEl);
            x = Math.Sin(theta) * h; y = tanEl * h; z = Math.Cos(theta) * h;
        }

        /// <summary>Streaks in frame at rain time t (rho 1, noise off), their energy coverage (fraction of the frame) and the
        /// shortest streak (px) among the drawn ones.</summary>
        static int Frame(double t, out double coverage, out double minLen)
        {
            int count = 0;
            double cov = 0;
            minLen = double.MaxValue;
            double X = NocRainMath.DriftClosed(t, 0), slope = NocRainMath.Slope(t, 0);
            for (int l = 0; l < 4; l++)
            {
                double z = NocturneConstants.RainLayerZ[l];
                NocRainMath.Phases(z, X, t, out double iBase, out double uFrac, out double jBase, out double vFrac);
                for (int iF = -6; iF <= 6; iF++)
                    for (int jF = -4; jF <= 4; jF++)
                        for (int k = 0; k < NocturneConstants.RainSlots; k++)
                        {
                            double ak = NocRainMath.Drop(l, z, iF, jF, k, NocturneConstants.RainSlotP, false, 0, 0, uFrac, vFrac, iBase, jBase,
                                                         out double th, out double te, out double bright);
                            if (ak < 0.5) continue;
                            Dir(th, te, out double dx, out double dy, out double dz);
                            if (!Project(dx, dy, dz, out double px, out double py)) continue;
                            // capsule ends 0.15 m along the fall direction (slope, -1) on the cylinder, projected
                            double D = z * Math.Sqrt(1.0 + te * te);
                            double el = Math.Sqrt(1.0 + slope * slope);
                            double ex = slope / el * 0.15, ey = -1.0 / el * 0.15;
                            // the end points: arc +-ex (along theta), height +-ey (along y at fixed horizontal radius)
                            double th1 = th + ex / z, te1 = te + ey / z, th0 = th - ex / z, te0 = te - ey / z;
                            Dir(th1, te1, out double ax, out double ay, out double az);
                            Dir(th0, te0, out double bx, out double by, out double bz);
                            double p1x = W / 2.0 + F * ax / az, p1y = H / 2.0 - F * ay / az, p0x = W / 2.0 + F * bx / bz, p0y = H / 2.0 - F * by / bz;
                            double len = Math.Sqrt((p1x - p0x) * (p1x - p0x) + (p1y - p0y) * (p1y - p0y));
                            double halfTrue = 0.5 * NocturneConstants.RainDropWidth * F / (D * dz);   // lateral size at depth D dz
                            double halfDraw = Math.Max(halfTrue, 0.5 * NocturneConstants.RainMinPx);
                            double kE = halfTrue / halfDraw;
                            double kLen = NocMath.Smoothstep(NocturneConstants.RainFadePx0, NocturneConstants.RainFadePx1, len);
                            if (kLen <= 0.5) continue;
                            count++;
                            minLen = Math.Min(minLen, len);
                            cov += (len + 2 * halfDraw) * 2 * halfDraw * kE * kLen * bright * ak;
                        }
            }
            coverage = cov / (W * (double)H);
            return count;
        }

        [Test]
        public void StaticCamera_CountCoverageLength_120Frames()
        {
            int min = int.MaxValue, max = 0; double covSum = 0, shortest = double.MaxValue; long sum = 0;
            for (int f = 0; f < 120; f++)
            {
                double t = 300.0 + f / 60.0;
                int n = Frame(t, out double cov, out double ml);
                min = Math.Min(min, n); max = Math.Max(max, n); sum += n; covSum += cov; shortest = Math.Min(shortest, ml);
            }
            double meanCov = covSum / 120;
            Debug.Log(FormattableString.Invariant($"[NOC] H5 rain at pitch 0, rho 1 (noise off): streaks {min}..{max} (mean {sum / 120.0:F1}), energy coverage {100 * meanCov:F3} %, shortest {shortest:F1} px"));
            Assert.GreaterOrEqual(min, 140, "H5: 140-210 streaks (D-005 slot probability 0.9)");
            Assert.LessOrEqual(max, 210);
            Assert.That(meanCov, Is.InRange(0.006, 0.010), "H5 coverage 0.6-1.0 %");
            Assert.GreaterOrEqual(shortest, 10.0, "no streak shorter than 10 px outside the zenith cone");
        }

        [Test]
        public void ZenithFade_RemovesShortDashes()
        {
            // at the screen centre with the camera pitched to el: streak px = f 0.30 cos^2(el) / z (R2 1.3)
            foreach (double z in NocturneConstants.RainLayerZ)
                for (double el = 0; el < 89.5; el += 0.5)
                {
                    double c = Math.Cos(el * Math.PI / 180);
                    double px = F * NocturneConstants.RainDropLen * c * c / z;
                    double k = NocMath.Smoothstep(NocturneConstants.RainFadePx0, NocturneConstants.RainFadePx1, px);
                    if (px < NocturneConstants.RainFadePx0) Assert.AreEqual(0.0, k, "layer " + z + " at el " + el);
                }
            Assert.AreEqual(83.2, F * 0.30 / 3.0, 0.2, "near layer streak 83 px at el 0 (H5)");
        }

        [Test]
        public void CellsWrapAroundTheCircle_NoSeamAtSouth()
        {
            int n = NocturneConstants.RainCells;
            foreach (double uFrac in new[] { 0.0, 0.37, 0.99 })
                foreach (double iBase in new[] { 0.0, 5.0, 20.0 })
                {
                    double eps = 1e-9;
                    double uA = Math.PI * n / (2 * Math.PI) - eps - uFrac, uB = -Math.PI * n / (2 * Math.PI) + eps - uFrac;   // theta = pi-, -pi+
                    double iA = Math.Floor(uA), iB = Math.Floor(uB);
                    int cA = (int)(iA - iBase - n * Math.Floor((iA - iBase) / n)), cB = (int)(iB - iBase - n * Math.Floor((iB - iBase) / n));
                    Assert.AreEqual(cA, cB, "same cell on both sides of theta = +-pi");
                    Assert.AreEqual(uA - iA, uB - iB, 1e-6, "continuous position inside the cell");
                }
            // the density noise is periodic in azimuth (11 lattice cells per turn)
            for (double y = 0; y < 5; y += 0.7)
                Assert.AreEqual(NocRainMath.Noise(0.3, y), NocRainMath.Noise(0.3 + NocturneConstants.RainNoiseCells, y), 1e-12);
        }

        [Test]
        public void Drift_IntegratesToTheClosedForm()
        {
            foreach (double g in new[] { 0.0, 1.0 })
            {
                double x = 0, t = 0;
                const double h = 1.0 / 240.0;
                while (t < 1000.0 - 1e-9) { x += NocturneConstants.RainFallSpeed * NocRainMath.Slope(t + 0.5 * h, g) * h; t += h; }
                Assert.AreEqual(NocRainMath.DriftClosed(1000.0, g), x, 1e-3, "drift X(1000 s), gust " + g);
            }
            // bounded drift (no sideways slide: the spec's arc += tilt q grew to 27 m/s)
            for (double t = 0; t < 3000; t += 3.7) Assert.LessOrEqual(Math.Abs(NocRainMath.DriftClosed(t, 0)), 36.01);
        }

        [Test]
        public void Rho_OverrideAndSchedule()
        {
            Assert.AreEqual(1.00, NocSchedule.RainRho(720), 1e-9, "beat 4 rho 1.00");
            Assert.AreEqual(0.55, NocSchedule.RainRho(40), 1e-9, "beat 1 rho 0.55");
            Assert.AreEqual(1.0, NocRainMath.Gust(720), 1e-9); Assert.AreEqual(0.0, NocRainMath.Gust(400), 1e-9);
        }

        /// <summary>sRGB HSV saturation of the NocRain tint min(lerp(1, env / Lloc, k), TintMax) for an environment colour env.</summary>
        static double TintSat(Color e, double l, double k)
        {
            double r = Math.Min(1.0 + (e.r / l - 1.0) * k, NocturneConstants.RainTintMax);
            double g = Math.Min(1.0 + (e.g / l - 1.0) * k, NocturneConstants.RainTintMax);
            double b = Math.Min(1.0 + (e.b / l - 1.0) * k, NocturneConstants.RainTintMax);
            double m = Math.Max(r, Math.Max(g, b));
            double sr = NocturnePalette.LinearToSrgb(r / m), sg = NocturnePalette.LinearToSrgb(g / m), sb = NocturnePalette.LinearToSrgb(b / m);
            return (Math.Max(sr, Math.Max(sg, sb)) - Math.Min(sr, Math.Min(sg, sb))) / Math.Max(sr, Math.Max(sg, sb));
        }

        [Test]
        public void Tint_LookV2_NoColouredSparks()
        {
            // Look v2 (CRIT0 #28): drops take the local light's hue at 30 % (NC_RainTint 0.3, v1 0.6), so a drop in front of the most
            // saturated accents (AviationRed, NeonMagenta, NeonCyan, SodiumAmber) stays a pale tint: sRGB HSV saturation <= 0.38,
            // at most 0.7 x the v1 tint (red 0.37 against 0.56 at 0.6)
            string src = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "_Game/Shaders/NocRain.shader"));
            StringAssert.Contains("float3 tint = min(lerp(float3(1.0, 1.0, 1.0), env / max(Lloc, 1e-4), NC_RainTint), (float3)NC_RainTintMax);", src);
            Assert.AreEqual(0.3, NocturneConstants.RainTint, 1e-12);
            foreach (var p in new[] { NocPal.AviationRed, NocPal.NeonMagenta, NocPal.NeonCyan, NocPal.SodiumAmber, NocPal.GoalLamp })
            {
                Color e = NocturnePalette.Linear(p);
                double l = Math.Max(NocMath.Luma709(e.r, e.g, e.b), 1e-4);
                double sat = TintSat(e, l, NocturneConstants.RainTint), v1 = TintSat(e, l, 0.6);
                Debug.Log(FormattableString.Invariant($"[NOC] rain tint in front of {p}: sRGB saturation {sat:F3} (v1 tint 0.6: {v1:F3})"));
                Assert.LessOrEqual(sat, 0.38, p.ToString());
                Assert.LessOrEqual(sat, 0.7 * v1 + 1e-9, p + " against the v1 tint");
            }
        }
    }
}
