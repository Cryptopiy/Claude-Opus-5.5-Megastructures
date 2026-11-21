// BwGradeMath: the CPU mirror of the BwGrade shader maths (MASTER_PROMPT 8.2, 14.2.5, 14.7 H5; D-10) and the
// small per-frame parameter mappings the pass feeds to the shader. Pure double C#, no UnityEngine dependency, so
// EditMode tests and QA scripts can evaluate the exact display transform. Imported from Thread's ThreadGradeMath
// (Neutral, vignette, dither remap, bloom plan) and extended with the colour matrix and the 0.18-pivot contrast.
//
// Display transform of one pixel (BwGrade.shader, FragGrade, in this order):
//   1. c = max(M * (scene * 2^EV), 0)        M = S * W: white balance (LMS von Kries, luminance of white kept)
//                                            then saturation around Rec.709 luminance
//   2. contrast in linear HDR: Y' = 0.18 (Y / 0.18)^(1 + k), hue-preserving scale c *= Y'/Y, faded to the
//      identity below Y ~ 0.0125 (toe protection keeps the sRGB-6 floor, lesson 3.2 #6)
//   3. c += bloom * intensity                (bloom prefiltered from M * (scene * 2^EV), so its colour matches)
//   4. Khronos PBR Neutral highlight compression without the toe, identity below 0.76 (decision B03-1, kept)
//   5. vignette * (1 - FadeBlack) * (1 - 0.5 PauseDim)
//   6. encode to sRGB code space, add monochrome TPDF blue noise of peak A codes, decode (GPU re-encodes exactly)
//
// D-10 numbers: without the toe protection, Y 0.0018 at k 0.04 maps to 0.0015 (about sRGB 5); with it, the floor is
// the identity (sRGB 5.9). Contrast k is clamped to [-0.25, 0.25] (spec maximum +4 -> 0.04), inside the monotonic range.
// Bloom clamp 40 (14.2.5 "bloom 40"). The toe-less Neutral stays (B03-1): with the 0.04 toe the sRGB-6 floor
// (linear 0.0018) would map to about 2e-5.
using System;

namespace Megastructure.Rendering
{
    public static class BwGradeMath
    {
        // ------------------------------------------------------------ tone curve (Khronos PBR Neutral)
        public const double NeutralStart = 0.76;
        public const double NeutralDesaturation = 0.15;

        // ------------------------------------------------------------ luminance (linear Rec.709 / sRGB primaries)
        // Rec.709 / sRGB luminance row; B rounded to 0.0721749 so the weights sum to exactly 1 (luminance-preserving
        // saturation matrix; the XYZ row 0.0721750 summed to 1.0000001). BwGrade.shader kBwGradeLuma matches.
        public const double LumR = 0.2126729, LumG = 0.7151522, LumB = 0.0721749;

        // ------------------------------------------------------------ contrast (D-10)
        public const double ContrastPivot = 0.18;
        public const double ContrastToe0 = 0.0025, ContrastToe1 = 0.0125;
        public const double ContrastKMax = 0.25;

        // ------------------------------------------------------------ vignette
        public const double VignetteRefAspect = 16.0 / 9.0;
        public const double VignetteShape = 1.5;

        // ------------------------------------------------------------ bloom (14.2.5)
        public const int BloomLevels = 6;
        public const double BloomKneeFraction = 0.5;
        /// <summary>HDR input clamp of the prefilter (14.2.5: bloom 40; the sun disc 60 is clamped here).</summary>
        public const double BloomClamp = 40.0;
        public const double ScatterMin = 0.05, ScatterMax = 0.95;
        public const int BloomReferenceHeight = 1080;

        // ------------------------------------------------------------ white balance (URP Core Color.hlsl:254-272)
        public static readonly double[] Lin2Lms =
        {
            3.90405e-1, 5.49941e-1, 8.92632e-3,
            7.08416e-2, 9.63172e-1, 1.35775e-3,
            2.31082e-2, 1.28021e-1, 9.36245e-1,
        };
        public static readonly double[] Lms2Lin =
        {
            2.85847e+0, -1.62879e+0, -2.48910e-2,
            -2.10182e-1, 1.15820e+0, 3.24281e-4,
            -4.18120e-2, -1.18169e-1, 1.06867e+0,
        };

        // ============================================================ tone curve
        public static void Neutral(double r, double g, double b, out double or, out double og, out double ob)
        {
            r = Math.Max(r, 0.0); g = Math.Max(g, 0.0); b = Math.Max(b, 0.0);
            double peak = Math.Max(r, Math.Max(g, b));
            if (peak < NeutralStart) { or = r; og = g; ob = b; return; }
            double d = 1.0 - NeutralStart;
            double newPeak = 1.0 - d * d / (peak + d - NeutralStart);
            double k = newPeak / peak;
            r *= k; g *= k; b *= k;
            double w = 1.0 - 1.0 / (NeutralDesaturation * (peak - newPeak) + 1.0);
            or = r + (newPeak - r) * w;
            og = g + (newPeak - g) * w;
            ob = b + (newPeak - b) * w;
        }

        public static double NeutralGrey(double x)
        {
            Neutral(x, x, x, out double r, out _, out _);
            return r;
        }

        // ============================================================ colour matrix (14.2.5, D-10)
        /// <summary>URP ColorUtils.ColorBalanceToLMSCoeffs in double (temperature, tint in -100..100).</summary>
        public static void ColorBalanceToLmsCoeffs(double temperature, double tint, out double kL, out double kM, out double kS)
        {
            double t1 = temperature / 65.0, t2 = tint / 65.0;
            double x = 0.31271 - t1 * (t1 < 0.0 ? 0.1 : 0.05);
            double y = (2.87 * x - 3.0 * x * x - 0.27509507) + t2 * 0.05;
            double X = x / y, Y = 1.0, Z = (1.0 - x - y) / y;
            double L = 0.7328 * X + 0.4296 * Y - 0.1624 * Z;
            double M = -0.7036 * X + 1.6975 * Y + 0.0061 * Z;
            double S = 0.0030 * X + 0.0136 * Y + 0.9834 * Z;
            kL = 0.949237 / L; kM = 1.03542 / M; kS = 1.08728 / S;
        }

        static void Mul3(double[] a, double[] b, double[] o)
        {
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    o[r * 3 + c] = a[r * 3] * b[c] + a[r * 3 + 1] * b[3 + c] + a[r * 3 + 2] * b[6 + c];
        }

        /// <summary>White-balance matrix W = LMS2LIN diag(k) LIN2LMS, normalised so the luminance of white is unchanged.</summary>
        public static double[] WhiteBalanceMatrix(double temperature, double tint)
        {
            var w = new double[9];
            WhiteBalanceMatrix(temperature, tint, w, new double[9]);
            return w;
        }

        /// <summary>Non-allocating form: writes W into w (9), uses scratch (9).</summary>
        public static void WhiteBalanceMatrix(double temperature, double tint, double[] w, double[] scratch)
        {
            ColorBalanceToLmsCoeffs(temperature, tint, out double kL, out double kM, out double kS);
            var d = scratch;
            for (int i = 0; i < 3; i++) { d[i] = Lin2Lms[i] * kL; d[3 + i] = Lin2Lms[3 + i] * kM; d[6 + i] = Lin2Lms[6 + i] * kS; }
            Mul3(Lms2Lin, d, w);
            double y = 0;
            for (int c = 0; c < 3; c++) y += LumR * w[c] + LumG * w[3 + c] + LumB * w[6 + c];
            if (y > 1e-12) for (int i = 0; i < 9; i++) w[i] /= y;
        }

        /// <summary>Saturation matrix S = (1 - f) [lum; lum; lum] + f I, f = 1 + Sat/100 (clamped at 0).</summary>
        public static double[] SaturationMatrix(double saturation)
        {
            var s = new double[9];
            SaturationMatrix(saturation, s);
            return s;
        }

        public static void SaturationMatrix(double saturation, double[] s)
        {
            double f = SaturationFactor(saturation);
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    s[r * 3 + c] = (1.0 - f) * (c == 0 ? LumR : (c == 1 ? LumG : LumB)) + (r == c ? f : 0.0);
        }

        /// <summary>Combined grade matrix M = S W (row-major 3x3). Identity at WB 0, tint 0, Sat 0 (up to 1e-6).</summary>
        public static double[] GradeMatrix(double temperature, double tint, double saturation)
        {
            var m = new double[9];
            GradeMatrix(temperature, tint, saturation, m);
            return m;
        }

        static readonly double[] s_w = new double[9], s_s = new double[9], s_d = new double[9];

        /// <summary>Non-allocating form for the render pass (main thread only: shared scratch).</summary>
        public static void GradeMatrix(double temperature, double tint, double saturation, double[] m)
        {
            WhiteBalanceMatrix(temperature, tint, s_w, s_d);
            SaturationMatrix(saturation, s_s);
            Mul3(s_s, s_w, m);
        }

        public static void Apply(double[] m, ref double r, ref double g, ref double b)
        {
            double nr = m[0] * r + m[1] * g + m[2] * b;
            double ng = m[3] * r + m[4] * g + m[5] * b;
            double nb = m[6] * r + m[7] * g + m[8] * b;
            r = nr; g = ng; b = nb;
        }

        public static double SaturationFactor(double saturation) => Math.Max(0.0, 1.0 + saturation / 100.0);

        // ============================================================ contrast (D-10)
        public static double ContrastK(double contrast) => Math.Max(-ContrastKMax, Math.Min(ContrastKMax, contrast / 100.0));

        static double Smoothstep(double e0, double e1, double x)
        {
            double t = (x - e0) / (e1 - e0);
            t = t < 0 ? 0 : (t > 1 ? 1 : t);
            return t * t * (3 - 2 * t);
        }

        /// <summary>Luminance contrast around the 0.18 pivot with toe protection: returns Y'.</summary>
        public static double ContrastLuma(double y, double k)
        {
            if (y <= 0) return 0;
            double p = ContrastPivot * Math.Pow(y / ContrastPivot, 1.0 + k);
            double w = Smoothstep(ContrastToe0, ContrastToe1, y);
            return y + (p - y) * w;
        }

        /// <summary>Hue-preserving contrast of a linear colour (scales by Y'/Y).</summary>
        public static void Contrast(double k, ref double r, ref double g, ref double b)
        {
            double y = LumR * r + LumG * g + LumB * b;
            if (y <= 1e-12) return;
            double s = ContrastLuma(y, k) / y;
            r *= s; g *= s; b *= s;
        }

        // ============================================================ vignette
        public static double Vignette(double u, double v, double aspect, double intensity)
        {
            double px = (u - 0.5) * 2.0 * aspect, py = (v - 0.5) * 2.0;
            double r2 = (px * px + py * py) / (VignetteRefAspect * VignetteRefAspect + 1.0);
            double shape = (1.0 - Math.Exp(-VignetteShape * r2)) / (1.0 - Math.Exp(-VignetteShape));
            return 1.0 - Math.Max(0.0, intensity) * shape;
        }

        // ============================================================ dither (14.7 H5, D-05)
        /// <summary>
        /// Effective dither peak (8-bit codes): mode Off -> 0; an explicit -dither-lsb override (>= 0) wins; otherwise the
        /// per-beat schedule peak. The result is clamped to [0, 4].
        /// </summary>
        public static double DitherPeak(DitherMode mode, double overridePeak, double schedulePeak)
        {
            if (mode == DitherMode.Off) return 0.0;
            double a = overridePeak >= 0.0 ? overridePeak : schedulePeak;
            return a < 0 ? 0 : (a > 4 ? 4 : a);
        }

        /// <summary>Uniform (0,1) -> symmetric triangular PDF on [-1, 1], monotonic (URP ApplyDithering remap).</summary>
        public static double UniformToTpdf(double u)
        {
            double t = 2.0 * u - 1.0;
            double m = 1.0 - Math.Sqrt(Math.Max(0.0, 1.0 - Math.Abs(t)));
            return t < 0 ? -m : m;
        }

        // ============================================================ bloom pyramid plan
        public static int BloomSkipLevels(int height)
        {
            if (height <= 0) return 0;
            int skip = (int)Math.Round(Math.Log(height / (double)BloomReferenceHeight, 2.0));
            return skip < 0 ? 0 : (skip > 2 ? 2 : skip);
        }

        public static int BloomTotalLevels(int width, int height, int maxLevels)
        {
            int total = BloomSkipLevels(height) + BloomLevels;
            if (total > maxLevels) total = maxLevels;
            int minDim = Math.Min(width, height);
            int fit = 0;
            while (fit < total && (minDim >> (fit + 1)) >= 2) fit++;
            return Math.Max(1, fit);
        }

        public static double ScatterLerp(double scatter)
        {
            double s = scatter < 0 ? 0 : (scatter > 1 ? 1 : scatter);
            return ScatterMin + (ScatterMax - ScatterMin) * s;
        }

        // ============================================================ full display transform (no bloom, no dither)
        /// <summary>
        /// CPU mirror of FragGrade without bloom and dither, in the shader order:
        /// Exposure -> Matrix -> Contrast -> Neutral -> Vignette*Fade -> clamp. vignetteTimesFade is the product of the
        /// vignette factor, (1 - FadeBlack) and the pause dim. Used by tests and QA predictions.
        /// </summary>
        public static void Display(double r, double g, double b, double exposureEV, double temperature, double tint,
                                   double saturation, double contrast, double vignetteTimesFade,
                                   out double or, out double og, out double ob)
        {
            double e = Math.Pow(2.0, exposureEV);
            r *= e; g *= e; b *= e;
            Apply(GradeMatrix(temperature, tint, saturation), ref r, ref g, ref b);
            r = Math.Max(0.0, r); g = Math.Max(0.0, g); b = Math.Max(0.0, b);
            Contrast(ContrastK(contrast), ref r, ref g, ref b);
            Neutral(r, g, b, out r, out g, out b);
            double m = Math.Max(0.0, vignetteTimesFade);
            or = Clamp01(r * m); og = Clamp01(g * m); ob = Clamp01(b * m);
        }

        /// <summary>The shader's FragGrade call order (Shader_ConstantsMatchCpuMirror asserts these appear in this order).</summary>
        public static readonly string[] ShaderOrder = { "BwGradeMatrix(", "BwGradeContrast(", "BwGradeNeutral(", "BwGradeVignette(", "BwGradeTpdf(" };

        static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);

        // ============================================================ sRGB (exact piecewise, as the shader)
        public static double LinearToSrgb(double c) => c <= 0.0031308 ? c * 12.92 : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;
        public static double SrgbToLinear(double s) => s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
    }
}
