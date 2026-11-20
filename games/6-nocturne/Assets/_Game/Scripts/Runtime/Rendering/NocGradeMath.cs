// NocGradeMath: the CPU mirror of the NocGrade shader maths (imported from Thread's ThreadGradeMath; MASTER_PROMPT 8.2,
// 14.2.4, 14.7 H1, D-007) and the small per-frame parameter mappings the pass feeds to the shader. Pure double C#, no
// UnityEngine dependency, so EditMode tests and QA scripts can evaluate the exact display transform.
//
// Display transform of one pixel (NocGrade.shader, FragGrade, in this order; F = the Floor, LOOK_V2 7):
//   1. c = F + (scene - F) * 2^EV + bloom * intensity   (bloom was thresholded on exposed values, prefilter clamp 30)
//   2. saturation in linear HDR around Rec.709 luminance, factor 1 + Saturation / 100 (URP convention)
//   3. Khronos PBR Neutral highlight compression, identity below 0.76 (decision B03-1 below)
//   4. contrast: odd S-curve in sRGB code space, fixed points 0, 0.5, 1, slope 1 + k at the pivot (0 in Nocturne)
//   5. c = (F + (c - F) * vignette) * fade (GradeState.FadeBlack = 1 - UiDirector.PictureMul): a Floor pixel displays as the
//      Floor at every EV and in every corner, the fade still reaches black
//   6. encode to sRGB code space; luma dither: one TPDF blue-noise value n (same on R, G, B) of peak
//      DitherAmplitude(peak, Y') = peak x lerp(1, 0.6, saturate((Y' - 0.35) / 0.4)) codes, Y' = Rec.709 luma of the encoded
//      values; chroma dither (D-007): +-1 code on (Cb, Cr) constant per 2 x 2 block, applied as an RGB offset with zero
//      luma; exact black and white stay exact; decode (the GPU re-encodes exactly).
//
// Decision B03-1 (tone curve): the Khronos PBR Neutral operator (Khronos 3D Commerce, 2024) is used
// WITHOUT its 0.04 "Fresnel offset" toe. That toe exists to cancel the F0 = 0.04 specular of a PBR
// material; Thread's flat shading has no specular term, and with the toe the palette's darks would be
// crushed (INK 0.0123 -> 0.0009, the night zenith 0.0012 -> 0.00001). Without it the curve is exactly
// the identity below 0.76 (every authored palette colour displays as authored), compresses only the
// highlights (same factor on all channels, then a mix toward white), is C1 at 0.76, monotonic and
// hue-preserving (HSV hue is invariant under a common scale and a mix with an achromatic colour).
//
// Decision B03-2 (contrast): URP's LogC contrast with +8 would push the night-sky zenith (code 4) below
// zero (crushed to black). Contrast is therefore an odd S-curve in display space,
//   f(s) = s + k (s - 0.5) 16 s^2 (1 - s)^2,
// which keeps 0, 0.5 and 1 fixed, has slope 1 + k at the pivot, slope 1 at both ends (blacks and whites
// stay exact) and is monotonic for k in [-1, 1.25] (min of the bracket's derivative is -0.8).
using System;

namespace Megastructure
{
    public static class NocGradeMath
    {
        // ------------------------------------------------------------ tone curve (Khronos PBR Neutral)
        /// <summary>Start of the highlight compression (Khronos: 0.8 - 0.04).</summary>
        public const double NeutralStart = 0.76;
        /// <summary>Desaturation speed of compressed highlights (Khronos value).</summary>
        public const double NeutralDesaturation = 0.15;

        // ------------------------------------------------------------ luminance (linear Rec.709 / sRGB primaries)
        public const double LumR = 0.2126729, LumG = 0.7151522, LumB = 0.0721750;

        // ------------------------------------------------------------ dither (14.7 H1, D-007)
        /// <summary>Rec.709 luma weights applied to the ENCODED values (Y', as 14.10 measures luma).</summary>
        public const double LumaR709 = 0.2126, LumaG709 = 0.7152, LumaB709 = 0.0722;

        // ------------------------------------------------------------ vignette (8.2 "very subtle", 14.2.5)
        /// <summary>Reference aspect at which GradeState.Vignette is the exact corner darkening.</summary>
        public const double VignetteRefAspect = 16.0 / 9.0;
        /// <summary>Saturation of the radial falloff (keeps 21:9 corners from darkening much more than 16:9 ones).</summary>
        public const double VignetteShape = 1.5;

        // ------------------------------------------------------------ bloom (14.2.5)
        /// <summary>Bloom levels of the pyramid at the 1080p reference (URP's default maximum iteration count).</summary>
        public const int BloomLevels = 6;
        /// <summary>Soft-knee width as a fraction of the threshold (LOOK_V2 7: 0.25, so the knee starts at exposed 0.675 and the
        /// windows, at most 0.51 exposed, never enter the bloom; the URP default is 0.5).</summary>
        public const double BloomKneeFraction = 0.25;
        /// <summary>HDR input clamp of the prefilter (14.2.4 clamp column: 30; Thread used 64; the goal lamp is 3.5).</summary>
        public const double BloomClamp = NocturneConstants.BloomClamp;
        /// <summary>URP's scatter mapping: lerp(0.05, 0.95, scatter).</summary>
        public const double ScatterMin = 0.05, ScatterMax = 0.95;
        /// <summary>Reference height for the resolution-normalised pyramid (1080p).</summary>
        public const int BloomReferenceHeight = 1080;

        // ============================================================ tone curve
        /// <summary>Khronos PBR Neutral without the toe offset (decision B03-1). Identity below NeutralStart.</summary>
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

        /// <summary>Neutral curve of a grey value.</summary>
        public static double NeutralGrey(double x)
        {
            Neutral(x, x, x, out double r, out _, out _);
            return r;
        }

        // ============================================================ saturation / contrast
        /// <summary>Saturation factor from the URP-style -100..100 value (+4 = 4 percent around luminance).</summary>
        public static double SaturationFactor(double saturation) => Math.Max(0.0, 1.0 + saturation / 100.0);

        /// <summary>Contrast strength k from the URP-style -100..100 value, clamped to the monotonic range.</summary>
        public static double ContrastK(double contrast) => Math.Max(-1.0, Math.Min(1.0, contrast / 100.0));

        /// <summary>Linear saturation around Rec.709 luminance, negative results clamped to 0.</summary>
        public static void Saturate(double factor, ref double r, ref double g, ref double b)
        {
            double l = LumR * r + LumG * g + LumB * b;
            r = Math.Max(0.0, l + (r - l) * factor);
            g = Math.Max(0.0, l + (g - l) * factor);
            b = Math.Max(0.0, l + (b - l) * factor);
        }

        /// <summary>Display-space contrast S-curve (decision B03-2); s is an sRGB-encoded value in [0, 1].</summary>
        public static double ContrastCurve(double s, double k)
        {
            s = s < 0 ? 0 : (s > 1 ? 1 : s);
            double q = s * (1.0 - s);
            return s + k * (s - 0.5) * 16.0 * q * q;
        }

        // ============================================================ vignette
        /// <summary>
        /// Vignette factor at unit screen coordinates (u, v) in [0, 1] for a render of the given aspect (w / h).
        /// Circular in pixels, radius in units of the half height (Hor+: the image centre band is identical at
        /// every aspect, wider screens add naturally darker sides), exactly 1 - intensity at a 16:9 corner.
        /// </summary>
        public static double Vignette(double u, double v, double aspect, double intensity)
        {
            double px = (u - 0.5) * 2.0 * aspect, py = (v - 0.5) * 2.0;
            double r2 = (px * px + py * py) / (VignetteRefAspect * VignetteRefAspect + 1.0);
            double shape = (1.0 - Math.Exp(-VignetteShape * r2)) / (1.0 - Math.Exp(-VignetteShape));
            return 1.0 - Math.Max(0.0, intensity) * shape;
        }

        // ============================================================ dither
        /// <summary>
        /// Per-pixel luma dither amplitude (peak of the TPDF in 8-bit codes, 14.7 H1): peak x lerp(1, 0.6, saturate((Y' - 0.35)
        /// / 0.4)). 2.0 for Y' &lt;= 0.35, 1.6 at 0.55, 1.2 for Y' >= 0.75 (peak 2.0); monotone non-increasing in Y'.
        /// </summary>
        public static double DitherAmplitude(double peak, double lumaDisplay)
        {
            double t = NocMath.Clamp01((lumaDisplay - NocturneConstants.DitherY0) / NocturneConstants.DitherYRange);
            return peak * (1.0 + (NocturneConstants.DitherBrightScale - 1.0) * t);
        }

        /// <summary>Rec.709 luma of encoded (display) values.</summary>
        public static double LumaDisplay(double r, double g, double b) => LumaR709 * r + LumaG709 * g + LumaB709 * b;

        /// <summary>
        /// D-007 chroma dither: an RGB offset (in codes) with zero Rec.709 luma that moves Cb by cb and Cr by cr codes
        /// (BT.709: R = Y + 1.5748 Cr, G = Y - 0.1873 Cb - 0.4681 Cr, B = Y + 1.8556 Cb; full-range code units).
        /// The shader takes cb, cr = +-amp from two blue-noise samples constant per 2 x 2 block.
        /// </summary>
        public static void ChromaOffset(double cb, double cr, out double dr, out double dg, out double db)
        {
            dr = 1.5748 * cr;
            dg = -0.1873 * cb - 0.4681 * cr;
            db = 1.8556 * cb;
        }

        /// <summary>Uniform (0,1) -> symmetric triangular PDF on [-1, 1], monotonic (URP Common.hlsl ApplyDithering remap).</summary>
        public static double UniformToTpdf(double u)
        {
            double t = 2.0 * u - 1.0;
            double m = 1.0 - Math.Sqrt(Math.Max(0.0, 1.0 - Math.Abs(t)));
            return t < 0 ? -m : m;
        }

        // ============================================================ bloom pyramid plan
        /// <summary>
        /// Number of pyramid levels below half resolution that are used only as intermediates, so the bloom
        /// kernel has the same screen-relative size at 1080p and 4K: 0 up to ~1527 px height, 1 at 2160p.
        /// </summary>
        public static int BloomSkipLevels(int height)
        {
            if (height <= 0) return 0;
            int skip = (int)Math.Round(Math.Log(height / (double)BloomReferenceHeight, 2.0));
            return skip < 0 ? 0 : (skip > 2 ? 2 : skip);
        }

        /// <summary>Total pyramid levels (skip + bloom levels), limited so the coarsest level is at least 2 px.</summary>
        public static int BloomTotalLevels(int width, int height, int maxLevels)
        {
            int total = BloomSkipLevels(height) + BloomLevels;
            if (total > maxLevels) total = maxLevels;
            int minDim = Math.Min(width, height);
            int fit = 0;
            while (fit < total && (minDim >> (fit + 1)) >= 2) fit++;
            return Math.Max(1, fit);
        }

        /// <summary>URP scatter mapping lerp(0.05, 0.95, scatter).</summary>
        public static double ScatterLerp(double scatter)
        {
            double s = scatter < 0 ? 0 : (scatter > 1 ? 1 : scatter);
            return ScatterMin + (ScatterMax - ScatterMin) * s;
        }

        // ============================================================ full display transform (no bloom, no dither)
        /// <summary>
        /// CPU mirror of FragGrade steps 1-5 without bloom and dither: linear scene colour -> linear display colour
        /// (what the 8-bit sRGB code encodes before the dither). Exposure and the vignette factor act relative to the Floor
        /// (LOOK_V2 7), the fade (1 - FadeBlack) multiplies last. Used by tests and by QA predictions (clipped highlights,
        /// palette identity, Display(Floor) == Floor).
        /// </summary>
        public static void Display(double r, double g, double b, double exposureEV, double saturation, double contrast,
                                   double vignette, double fade, out double or, out double og, out double ob)
        {
            double e = Math.Pow(2.0, exposureEV);
            double fr = NocturneConstants.FloorLinR, fg = NocturneConstants.FloorLinG, fb = NocturneConstants.FloorLinB;
            r = Math.Max(0.0, fr + (r - fr) * e); g = Math.Max(0.0, fg + (g - fg) * e); b = Math.Max(0.0, fb + (b - fb) * e);
            Saturate(SaturationFactor(saturation), ref r, ref g, ref b);
            Neutral(r, g, b, out r, out g, out b);
            double k = ContrastK(contrast);
            r = SrgbToLinear(ContrastCurve(LinearToSrgb(Clamp01(r)), k));
            g = SrgbToLinear(ContrastCurve(LinearToSrgb(Clamp01(g)), k));
            b = SrgbToLinear(ContrastCurve(LinearToSrgb(Clamp01(b)), k));
            double v = Math.Max(0.0, vignette), m = Math.Max(0.0, fade);
            or = Clamp01(Math.Max(0.0, fr + (r - fr) * v) * m);
            og = Clamp01(Math.Max(0.0, fg + (g - fg) * v) * m);
            ob = Clamp01(Math.Max(0.0, fb + (b - fb) * v) * m);
        }
        /// <summary>Display without a picture fade (fade 1).</summary>
        public static void Display(double r, double g, double b, double exposureEV, double saturation, double contrast,
                                   double vignette, out double or, out double og, out double ob)
            => Display(r, g, b, exposureEV, saturation, contrast, vignette, 1.0, out or, out og, out ob);

        static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);

        // ============================================================ sRGB (exact piecewise, as the shader)
        public static double LinearToSrgb(double c) => c <= 0.0031308 ? c * 12.92 : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;
        public static double SrgbToLinear(double s) => s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
    }
}
