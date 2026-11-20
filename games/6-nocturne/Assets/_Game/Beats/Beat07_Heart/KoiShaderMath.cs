// KoiShaderMath: the formulas of Shaders/Beats/NocKoi.shader in C# single precision (System.MathF, the GPU's float), so
// Beat07HeartTests and KoiShaderTests can prove that the shader's koi is KoiMath's koi (V's double-precision reference, the one the
// composition lint and the clearance tests sample) and measure its look (Look v2, TEXTURE_PLAN 6, LOOK_V2 6):
// - vertex (D-032 2, HT): the reference spine of KoiMath enters as per-vertex attributes of the station a (uv3 = (r_ref, alpha,
//   dr_ref/da, dalpha/da), uv4 = (Dy, dDy/da, 0, 0); ShapeUV3 / ShapeUV4), the spine phi = phi_h - alpha, r = r_ref + (R(phi) - 260)
//   m(a) + w, y = y(phi) + Dy with its analytic derivative instead of KoiMath's central difference, the ring frame
//   N = normalize(Y x F), B = F x N; every frame part (body, caps, fins, barbels) is a rigid point (forward, radial cos theta,
//   radial sin theta) in the frame of its station a (FrameOffset converts (F, N, B) offsets to that form); the veil spans
//   cos(roll) Y + sin(roll) N;
// - fragment: the integer hash and value noise, the Kohaku patch mask, the imbricate scale composite, the caustic network, the
//   eye and gill terms and the body structure factor, line for line (KoiShaderTests checks the means, the patch share, the head
//   mask, the eye ratio and that the shader's #defines are these constants);
// - the uniforms of the driver: phi_h wrapped to [0, 2 pi), frac(t_w / 20), the patch saturation fade of s and the scanline,
//   flow and caustic phases (computed in double, uploaded as floats).
// Inputs are exactly the vertex attributes KoiBuilder writes and the uniforms KoiDriver uploads. No allocation.
using System;
using UnityEngine;

namespace Megastructure
{
    public static class KoiShaderMath
    {
        const float SX = (float)KoiMath.CentreX, SZ = (float)KoiMath.CentreZ;
        const float R0 = (float)KoiMath.OrbitR0, RA = (float)KoiMath.OrbitRAmp, RP = (float)KoiMath.OrbitRPhase;
        const float Y0 = (float)KoiMath.OrbitY0, YA = (float)KoiMath.OrbitYAmp, YP = (float)KoiMath.OrbitYPhase;
        const float WL = (float)KoiMath.WaveLength, Tau = 6.28318530718f;

        // ------------------------------------------------------------------ look constants (NocKoi #defines)
        /// <summary>Scale columns around the body, row spacing / column width, scale disc radius (column widths; the offset-row
        /// lattice's covering radius is 0.508, so every point lies in a disc), smallest radius the scale size follows (m).</summary>
        public const float ScaleColumns = 18f, RowAspect = 0.6f, ScaleRadius = 0.62f, RhoMin = 14f;
        /// <summary>Patch head mask (0.12 L, LOOK_V2 6), gill cover edge a = GillA + GillB |cos theta| (m).</summary>
        public const float HeadA = 108f, GillA = 84f, GillB = 16f;
        /// <summary>Eye spots: station (m), ring angle above the side axis (rad), radius (m); pupil 0.62 of it at x0.30.</summary>
        public const float EyeA = 36f, EyeTheta = 0.40f, EyeR = 4.2f;
        /// <summary>Mean of the composited scale shade (footprints up to 0.08 columns) and of the caustic network (KoiShaderTests
        /// measures both).</summary>
        public const float ScaleMean = 0.6245f, CausticMean = 0.4392f;
        /// <summary>The soft composite darkens at wider footprints: mean(fw) = ScaleMean - A x^2 / (x + B), x = max(fw - F0, 0)
        /// (fit to 0.0005 over fw 0-0.45 columns, where the scales fade out; the vein subtracts it).</summary>
        public const float ScaleMeanF0 = 0.07f, ScaleMeanA = 0.0737f, ScaleMeanB = 0.08f;
        /// <summary>Gains relative to the body and max-channel budgets (LOOK_V2 6 ladder; TEXTURE_PLAN 6 body clamp 1.5-2.0).</summary>
        public const float FinGain = 0.25f, VeilGain = 0.20f, FrontBudget = 1.8f, BackBudget = 0.8f, BackWeight = 0.35f, FinBudget = 0.6f, VeilBudget = 0.35f;
        /// <summary>Patch saturation fade over s 1 440 - 1 490 down to x0.25 (LOOK_V2 6: no warm koi pixel next to the window).</summary>
        public const double PatchFadeS0 = 1440.0, PatchFadeS1 = 1490.0, PatchFadeFloor = 0.25;
        /// <summary>Scanlines: 24 m bands moving 6 m/s in world height (14.7 H6); flow period 8 s; caustic loop 240 s.</summary>
        public const double ScanPitch = 24.0, ScanSpeed = 6.0, FlowPeriod = 8.0, CausticPeriod = 240.0;
        /// <summary>Part ids in uv2.w (frame parts) and the veil's uv2.x = 1.</summary>
        public const float PartBody = 0f, PartVeil = 1f, PartPectoralOuter = 2f, PartPectoralInner = 3f, PartDorsal = 4f, PartBarbel = 5f,
                           PartPelvicOuter = 6f, PartPelvicInner = 7f, PartAnal = 8f;
        /// <summary>Body structure factor StructBase + StructSwing vein (mean 0.775 at a vein mean of 0.5, LOOK_V2 6); the vein
        /// carries VeinScale x (scale shade - its footprint mean) + CausticAmp x (caustic - its mean); Fresnel rim exponent.</summary>
        public const float StructBase = 0.40f, StructSwing = 0.75f, VeinScale = 1.6f, CausticAmp = 0.6f, RimPower = 3.0f;
        /// <summary>Tail: the body (x 1 - 0.9 f) and rim (x 1 - 0.85 f) fade over a 820 - 900 m, f = smoothstep, so the caudal
        /// peduncle dissolves into the veil instead of ending in a rounded tube.</summary>
        public const float TailFadeA0 = 820f, TailFadeA1 = 900f;
        /// <summary>Pelvic fins (KO's own frame parts, inside KoiMath's pectoral envelope): root from a = 0.47 L, chord 28 m, at
        /// 70 deg below the side axis, 60 m along normalize(-0.65 F +- 0.65 N - 0.40 B) (splayed, seen from below).</summary>
        public const double PelvicA0 = 423.0, PelvicChord = 28.0, PelvicLength = 60.0, PelvicRootTheta = 70.0;
        public const double PelvicDirF = -0.65, PelvicDirN = 0.65, PelvicDirB = -0.40;
        /// <summary>Anal fin on the belly from a = 0.70 L for 40 m: height 10 -> 24 (u 0.3) -> 8 m, swept back by 0.5 x its height.</summary>
        public const double AnalA0 = 630.0, AnalLength = 40.0, AnalH0 = 10.0, AnalHPeak = 24.0, AnalHEnd = 8.0, AnalPeakAt = 0.3, AnalSweep = 0.5;

        const float RMod = (float)KoiMath.RModFade;
        static readonly float s_rollC = (float)Math.Cos(KoiMath.VeilRollDeg * Math.PI / 180.0), s_rollS = (float)Math.Sin(KoiMath.VeilRollDeg * Math.PI / 180.0);

        /// <summary>The reference-spine attributes of station a (mesh uv3): r_ref, alpha, dr_ref / da, dalpha / da (KoiMath, double, then float).</summary>
        public static Vector4 ShapeUV3(double a)
        {
            double r = KoiMath.RefRadius(a, out double dr), al = KoiMath.RefAngle(a, out double dal);
            return new Vector4((float)r, (float)al, (float)dr, (float)dal);
        }

        /// <summary>The drop attributes of station a (mesh uv4): Dy, dDy / da, 0, 0.</summary>
        public static Vector4 ShapeUV4(double a)
        {
            double y = KoiMath.Drop(a, out double dy);
            return new Vector4((float)y, (float)dy, 0f, 0f);
        }

        /// <summary>Waved spine point and its derivative along a from the station's shape attributes (NocKoi KoiSpine).</summary>
        public static void Spine(float a, Vector4 sh3, Vector4 sh4, float env, float envD, float phiH, float twFrac, out Vector3 c, out Vector3 dc)
        {
            float phi = phiH - sh3.y, dphi = -sh3.w;
            float sp = MathF.Sin(phi), cp = MathF.Cos(phi);
            float s2 = MathF.Sin(2f * phi + RP), c2 = MathF.Cos(2f * phi + RP);
            float t = Math.Max(0f, Math.Min(1f, a / RMod));
            float m = 1f - t * t * (3f - 2f * t), dm = -6f * t * (1f - t) / RMod;
            float ph = Tau * (a / WL - twFrac);
            float sw = MathF.Sin(ph), cw = MathF.Cos(ph);
            float w = env * sw;
            float dw = envD * sw + env * cw * (Tau / WL);
            float r = sh3.x + RA * s2 * m + w;
            float dr = sh3.z + 2f * RA * c2 * dphi * m + RA * s2 * dm + dw;
            float y = Y0 + YA * MathF.Sin(phi + YP) + sh4.x;
            float dy = YA * MathF.Cos(phi + YP) * dphi + sh4.y;
            c = new Vector3(SX + r * cp, y, SZ + r * sp);
            dc = new Vector3(dr * cp - r * sp * dphi, dy, dr * sp + r * cp * dphi);
        }

        /// <summary>World position and normal of one koi vertex; the shape attributes from its station uv0.x (as KoiBuilder writes them).</summary>
        public static void Vertex(Vector2 uv0, Vector4 uv1, Vector4 uv2, float phiH, float twFrac, out Vector3 p, out Vector3 n) =>
            Vertex(uv0, uv1, uv2, ShapeUV3(uv0.x), ShapeUV4(uv0.x), phiH, twFrac, out p, out n);

        /// <summary>World position and normal of one koi vertex (the shader's vertex function).</summary>
        public static void Vertex(Vector2 uv0, Vector4 uv1, Vector4 uv2, Vector4 uv3, Vector4 uv4, float phiH, float twFrac, out Vector3 p, out Vector3 n)
        {
            Spine(uv0.x, uv3, uv4, uv1.z, uv1.w, phiH, twFrac, out Vector3 c, out Vector3 dc);
            Vector3 f = -dc / MathF.Sqrt(MathF.Max(Vector3.Dot(dc, dc), 1e-12f));
            Vector3 nn = Vector3.Cross(Vector3.up, f);
            nn /= MathF.Sqrt(MathF.Max(Vector3.Dot(nn, nn), 1e-12f));
            Vector3 b = Vector3.Cross(f, nn);
            Vector3 ring = MathF.Cos(uv0.y) * nn + MathF.Sin(uv0.y) * b;
            if (uv2.x < 0.5f)
            {
                p = c + f * uv1.y + ring * uv1.x;
                n = (f * uv1.y + ring * uv1.x).normalized;
            }
            else
            {
                p = c + (s_rollC * Vector3.up + s_rollS * nn) * (uv2.y * uv2.z);
                n = (s_rollS * b - s_rollC * nn).normalized;
            }
        }

        /// <summary>A rigid point (fF, fN, fB) in the ring frame of a station as the vertex attributes of a frame part: theta in
        /// [0, 2 pi), radial >= 0, forward (the shader places it at C + F forward + (cos theta N + sin theta B) radial).</summary>
        public static void FrameOffset(double fF, double fN, double fB, out double theta, out double radial, out double forward)
        {
            radial = Math.Sqrt(fN * fN + fB * fB);
            theta = radial > 1e-12 ? Math.Atan2(fB, fN) : 0.0;
            if (theta < 0.0) theta += 2.0 * Math.PI;
            forward = fF;
        }

        /// <summary>The uniforms of the driver: phi_h wrapped to [0, 2 pi), frac(t_w / 20), the patch saturation at s and the
        /// scanline phase frac(6 t_w / 24) (computed in double).</summary>
        public static Vector4 Params(double phiHead, double tw, double s)
        {
            double tau = 2.0 * Math.PI;
            double pw = phiHead - tau * Math.Floor(phiHead / tau);
            return new Vector4((float)pw, (float)Frac(tw / KoiMath.WavePeriod), (float)PatchSaturation(s), (float)Frac(tw * ScanSpeed / ScanPitch));
        }

        /// <summary>The reference uniforms at a head azimuth and wave time with full patch saturation (builder, tests).</summary>
        public static Vector4 Params(double phiHead, double tw) => Params(phiHead, tw, KoiMath.BeatS);

        /// <summary>_KoiTime: flow phase frac(t_w / 8) and caustic phase frac(t_w / 240) (periodic, so float precision never runs out).</summary>
        public static Vector4 TimeParams(double tw) => new Vector4((float)Frac(tw / FlowPeriod), (float)Frac(tw / CausticPeriod), 0f, 0f);

        /// <summary>Patch saturation factor 1 - 0.75 smoothstep(1 440, 1 490, s) (LOOK_V2 6).</summary>
        public static double PatchSaturation(double s) => 1.0 - (1.0 - PatchFadeFloor) * NocMath.Smoothstep(PatchFadeS0, PatchFadeS1, s);

        static double Frac(double x) => x - Math.Floor(x);

        /// <summary>Derivative of the wave envelope 40 (0.15 + 0.85 (a / L)^2): 0 beyond L (KoiMath holds the tail value there) and
        /// at exactly L the mean of both sides, which is what KoiMath's central difference across the kink returns (the tail ring
        /// would otherwise tilt by 0.038 rad, 0.36 m at rho 9.6).</summary>
        public static double EnvelopeDerivative(double a)
        {
            if (a <= 0.0 || a > KoiMath.Length) return 0.0;
            double d = KoiMath.WaveAmp * KoiMath.WaveTail * 2.0 * a / (KoiMath.Length * KoiMath.Length);
            return a >= KoiMath.Length ? 0.5 * d : d;
        }

        // ------------------------------------------------------------------ layout of the body pattern
        /// <summary>Scale row spacing (m) at a: 0.6 x the column width 2 pi max(rho, 14) / 18 (10.05 m at the widest ring).</summary>
        public static double RowSpacing(double a) => RowAspect * 2.0 * Math.PI * Math.Max(KoiMath.Radius(a), RhoMin) / ScaleColumns;

        /// <summary>Scale row coordinate sv(a) = integral of 1 / RowSpacing from the head (rows grow smaller with the radius, so the
        /// scales stay about square); Simpson with steps of 0.5 m or less, extended linearly before 0 (the snout cap).</summary>
        public static double ScaleRow(double a)
        {
            if (a <= 0.0) return a / RowSpacing(0.0);
            int n = Math.Max(2, 2 * (int)Math.Ceiling(a));
            double h = a / n, sum = 1.0 / RowSpacing(0.0) + 1.0 / RowSpacing(a);
            for (int i = 1; i < n; i++) sum += (i % 2 == 1 ? 4.0 : 2.0) / RowSpacing(i * h);
            return sum * h / 3.0;
        }

        /// <summary>Fan fins (pectoral, pelvic) in KoiMath's (u along the root chord, v along the fin) square: the fin spans
        /// u in [0, FanUMax(v)] (0.35 of the chord at the root, the whole chord from v 0.75), so its rays s = u / FanUMax(v)
        /// radiate from a short root; the outer edge is v = FanVMax(s) = 1 - 0.3 s^2 (the leading ray longest, rounded tip).</summary>
        public static double FanUMax(double v) => 0.35 + 0.65 * NocMath.Smoothstep(0.0, 0.75, v);

        public static double FanVMax(double s) { double c = Math.Max(0.0, Math.Min(1.0, s)); return 1.0 - 0.3 * c * c; }

        /// <summary>The (u, v) of fan coordinates s (leading 0 .. trailing 1) and w (root 0 .. edge 1).</summary>
        public static void FanUV(double s, double w, out double u, out double v)
        {
            v = w * FanVMax(s);
            u = s * FanUMax(v);
        }

        /// <summary>Pelvic fin point in the ring frame of its station: (forward, side N, up B) offsets (m); side +1 outer.</summary>
        public static void PelvicOffset(int side, double u, double v, out double a, out double fF, out double fN, out double fB)
        {
            a = PelvicA0 + PelvicChord * Math.Max(0.0, Math.Min(1.0, u));
            double rho = KoiMath.Radius(a), th = PelvicRootTheta * (Math.PI / 180.0), sg = side >= 0 ? 1.0 : -1.0;
            double dl = Math.Sqrt(PelvicDirF * PelvicDirF + PelvicDirN * PelvicDirN + PelvicDirB * PelvicDirB), l = PelvicLength * v / dl;
            fF = PelvicDirF * l;
            fN = sg * (rho * Math.Cos(th) + PelvicDirN * l);
            fB = -rho * Math.Sin(th) + PelvicDirB * l;
        }

        /// <summary>Anal fin height profile (m) at u in [0, 1]: 10 -> 24 (u 0.3) -> 8.</summary>
        public static double AnalHeight(double u)
        {
            u = Math.Max(0.0, Math.Min(1.0, u));
            return u <= AnalPeakAt ? AnalH0 + (AnalHPeak - AnalH0) * u / AnalPeakAt
                                   : AnalHPeak + (AnalHEnd - AnalHPeak) * (u - AnalPeakAt) / (1.0 - AnalPeakAt);
        }

        /// <summary>Anal fin point in the ring frame of its station: straight down from the belly, swept back by 0.5 x the height.</summary>
        public static void AnalOffset(double u, double v, out double a, out double fF, out double fN, out double fB)
        {
            a = AnalA0 + AnalLength * Math.Max(0.0, Math.Min(1.0, u));
            double h = AnalHeight(u) * Math.Max(0.0, Math.Min(1.0, v));
            fF = -AnalSweep * h;
            fN = 0.0;
            fB = -(KoiMath.Radius(a) + h);
        }

        /// <summary>Reference world point of a frame-part offset at station a (KoiMath's double-precision frame; tests).</summary>
        public static D3 FramePoint(double a, double fF, double fN, double fB, double phiHead, double tw)
        {
            KoiMath.Frame(a, phiHead, tw, out D3 c, out D3 f, out D3 n, out D3 b);
            return c + f * fF + n * fN + b * fB;
        }

        // ------------------------------------------------------------------ fragment twin
        static float Smoothstep(float e0, float e1, float x)
        {
            float t = Math.Max(0f, Math.Min(1f, (x - e0) / (e1 - e0)));
            return t * t * (3f - 2f * t);
        }

        static float Floor(float x) => MathF.Floor(x);
        static float Lerp(float a, float b, float t) => a + (b - a) * t;
        static float Sat(float x) => Math.Max(0f, Math.Min(1f, x));

        public static uint Hash(int x, int y)
        {
            unchecked
            {
                uint h = ((uint)x * 0x8DA6B343u) ^ ((uint)y * 0xD8163841u);
                h ^= h >> 15; h *= 0x2C1B3C6Du; h ^= h >> 12; h *= 0x297A2D39u; h ^= h >> 15;
                return h;
            }
        }

        static float Hash01(int x, int y) => (Hash(x, y) & 0xFFFFFFu) * (1f / 16777216f);

        /// <summary>Value noise on the integer lattice (quintic fade), [0, 1].</summary>
        public static float Noise(float px, float py)
        {
            float ix = Floor(px), iy = Floor(py), fx = px - ix, fy = py - iy;
            float ux = fx * fx * fx * (fx * (fx * 6f - 15f) + 10f), uy = fy * fy * fy * (fy * (fy * 6f - 15f) + 10f);
            int cx = (int)ix, cy = (int)iy;
            float n00 = Hash01(cx, cy), n10 = Hash01(cx + 1, cy), n01 = Hash01(cx, cy + 1), n11 = Hash01(cx + 1, cy + 1);
            return n00 + (n10 - n00) * ux + (n01 - n00) * uy + (n00 - n10 - n01 + n11) * ux * uy;
        }

        static readonly float[] s_patchC = { 194f, 356f, 492f, 618f };
        static readonly float[] s_patchH = { 48f, 39f, 27f, 13f };

        /// <summary>Kohaku patch mask at arc length a and st = sin theta (symmetric about the dorsal line).</summary>
        public static float Patch(float a, float st)
        {
            float q = (1f - st) * 70f;
            float n = 0.6f * Noise(a * (1f / 80f) + 3.1f, q * (1f / 80f) + 7.3f) + 0.4f * Noise(a * (1f / 34f) + 9.4f, q * (1f / 34f) + 1.7f);
            float env = 0f, gate = 0f;
            for (int k = 0; k < 4; k++)
            {
                float x = (a - s_patchC[k]) / s_patchH[k];
                env = Math.Max(env, Sat(1f - x * x));
                gate = Math.Max(gate, Smoothstep(1.6f * s_patchH[k], 1.3f * s_patchH[k], Math.Abs(a - s_patchC[k])));
            }
            float band = Smoothstep(0.37f, 0.43f, env + 1.8f * (n - 0.5f)) * gate;
            float psi = MathF.Acos(Math.Max(-1f, Math.Min(1f, st)));
            float pmax = 2.30f + 0.6f * (Noise(a * (1f / 60f) + 5.3f, 2.9f) - 0.5f);
            return band * Smoothstep(pmax + 0.10f, pmax - 0.10f, psi) * Smoothstep(HeadA - 6f, HeadA + 6f, a);
        }

        public static float ScaleShade(float e, float t, float w)
        {
            float rim = (1f - Smoothstep(0f, 0.11f + w, e)) * (0.11f / (0.11f + w));
            return 0.42f + 0.26f * Smoothstep(-0.7f, 1f, t) + 0.42f * rim;
        }

        /// <summary>The imbricate scale composite (shade) and the scalloped patch mask of the scale centres.</summary>
        public static float Scales(float X, float Y, float fw, float a, float rowM, float patchP, out float patchQ)
        {
            float j0 = Floor(Y - ScaleRadius / RowAspect) + 1f;
            float acc = 0f, wsum = 0f, cover = 1f, shadow = 1f;
            patchQ = 0f;
            for (int k = 0; k < 3; k++)
            {
                float j = j0 + k;
                float odd = 0.5f * (j - 2f * Floor(j * 0.5f));
                float xc = Floor(X - odd + 0.5f) + odd;
                float dx = X - xc, dy = (Y - j) * RowAspect;
                float e = ScaleRadius - MathF.Sqrt(dx * dx + dy * dy);
                float cov = Smoothstep(-fw, fw, e);
                float wk = cov * cover;
                float hc = 2f * xc - 36f * Floor(2f * xc / 36f);
                float jit = Hash01((int)j, (int)(hc + 0.5f)) - 0.5f;
                float s = ScaleShade(e, dy / ScaleRadius, fw) * shadow * (1f + 0.24f * jit);
                float ac = a - (Y - j) * rowM;
                patchQ += wk * Patch(ac, MathF.Sin(xc * (Tau / ScaleColumns)));
                acc += wk * s;
                wsum += wk;
                cover *= 1f - cov;
                shadow *= 1f - 0.45f * (1f - Smoothstep(0f, 0.16f + fw, -e)) * (1f - cov);
            }
            float rest = Math.Max(0f, 1f - wsum);
            patchQ += rest * patchP;
            return acc + rest * ScaleMean;
        }

        /// <summary>Mean of the composited scale shade at footprint fw (column widths).</summary>
        public static float ScaleMeanAt(float fw)
        {
            float x = Math.Max(fw - ScaleMeanF0, 0f);
            return ScaleMean - ScaleMeanA * x * x / (x + ScaleMeanB);
        }

        /// <summary>The caustic ridge network at surface metres (qx, qy = 40 theta) and loop phase ph, [0, 1]; every qy frequency
        /// is 2 / 80 per metre, so the network closes at theta = 2 pi.</summary>
        public static float Caustic(float qx, float qy, float ph)
        {
            float s = MathF.Sin(Tau * ph), c = MathF.Cos(Tau * ph);
            float px = qx * (1f / 80f) + 0.8f * c, py = qy * (1f / 80f) + 0.8f * s;
            float rx = px + 0.55f * MathF.Sin(py * 2f + 1.3f), ry = py + 0.55f * MathF.Sin(px * 1.3f - 0.4f);
            float v = 1f - 0.5f * Math.Abs(MathF.Sin(rx * 1.9f + MathF.Sin(ry * 2f)) + MathF.Sin(ry * 2f + MathF.Sin(rx * 1.1f + 0.5f)));
            return v * v;
        }

        /// <summary>The back shell's view ray through the tube to the front shell (NocKoi KoiFrontPatch): for a back-shell fragment
        /// at arc length a with radial offset rad, outward normal nOut, spine tangent f (toward the head) and direction to the camera
        /// vdir, the front point's offset df from the axis, its station aF and sin theta (the local cylinder of radius rad).</summary>
        public static void FrontHit(float a, float rad, Vector3 nOut, Vector3 f, Vector3 vdir, out float aF, out float stF, out Vector3 df)
        {
            Vector3 v = -vdir;
            float vF = Vector3.Dot(v, f);
            Vector3 vp = v - vF * f;
            float vp2 = MathF.Max(Vector3.Dot(vp, vp), 1e-4f);
            Vector3 d = rad * nOut;
            float t = 2f * MathF.Max(Vector3.Dot(d, vp), 0f) / vp2;
            df = d - t * vp;
            Vector3 n = Vector3.Cross(Vector3.up, f);
            n /= MathF.Sqrt(MathF.Max(Vector3.Dot(n, n), 1e-12f));
            stF = Vector3.Dot(df, Vector3.Cross(f, n)) / MathF.Max(rad, 1e-3f);
            aF = a + Math.Max(-80f, Math.Min(80f, t * vF));
        }

        /// <summary>The front shell's patch mask seen by a back-shell fragment (NocKoi KoiFrontPatch).</summary>
        public static float FrontPatch(float a, float rad, Vector3 nOut, Vector3 f, Vector3 vdir)
        {
            FrontHit(a, rad, nOut, f, vdir, out float aF, out float stF, out _);
            return Patch(aF, Math.Max(-1f, Math.Min(1f, stF)));
        }

        /// <summary>A filtered line of half-width w at signed distance d with footprint fw (energy kept as it widens).</summary>
        public static float Line(float d, float w, float fw) => (1f - Smoothstep(w, w + fw, Math.Abs(d))) * (w / (w + 0.5f * fw));

        /// <summary>The body structure factor (StructBase + StructSwing vein) and patch mask of a body fragment with its footprints: fwX and
        /// fwY of the scale lattice, fwA (m / px along the body), fwQ of the caustic coordinates; caust = KoiCaustic or its mean.</summary>
        public static float BodyStructure(float a, float theta, float sv, float rad, float fwX, float fwY, float fwA, float caust, out float patchM, out float eye)
        {
            float st = MathF.Sin(theta), ct = MathF.Cos(theta);
            float X = theta * (ScaleColumns / Tau);
            float rhoC = Math.Max(rad, RhoMin);
            float rowM = RowAspect * Tau * rhoC / ScaleColumns;
            float fwE = Math.Max(fwX, fwY * RowAspect);
            float pitchPx = 1f / Math.Max(Math.Max(fwX, fwY), 1e-5f);
            float sFade = Smoothstep(2.5f, 5f, pitchPx);
            float patchP = Patch(a, st);
            float sc = Scales(X, sv, fwE, a, rowM, patchP, out float patchQ);
            patchM = Lerp(patchP, patchQ, sFade);
            float gill = GillA + GillB * Math.Abs(ct);
            float onBody = Smoothstep(gill - 3f, gill + 5f, a);
            float sideW = Smoothstep(0.15f, 0.45f, Math.Abs(ct));
            float veinS = 0.5f + VeinScale * sFade * (sc - ScaleMeanAt(fwE));
            float vein = Lerp(0.48f, veinS, onBody) + CausticAmp * (caust - CausticMean);
            vein += 0.45f * Line(a - gill, 1.2f, fwA) * sideW;
            vein -= 0.30f * Smoothstep(gill + 1f, gill + 3f + fwA, a) * (1f - Smoothstep(gill + 4f, gill + 12f + fwA, a)) * sideW;
            float dV = theta - 4.71238898f;
            dV -= Tau * Floor(dV / Tau + 0.5f);
            vein -= 0.6f * Line(a - (-7f + 3.5f * (1f - MathF.Cos(dV))), 0.7f, fwA) * (1f - Smoothstep(0.9f, 1.3f, Math.Abs(dV)));
            vein = Sat(vein);
            float dthO = theta - EyeTheta, dthI = theta - (3.14159265f - EyeTheta);
            dthO -= Tau * Floor(dthO / Tau + 0.5f);
            dthI -= Tau * Floor(dthI / Tau + 0.5f);
            float rr = Math.Max(rad, 1f);
            float dE = Math.Min(MathF.Sqrt((a - EyeA) * (a - EyeA) + dthO * rr * dthO * rr), MathF.Sqrt((a - EyeA) * (a - EyeA) + dthI * rr * dthI * rr));
            float fwD = fwA + 1e-3f;
            eye = Lerp(1f, 0.15f, 1f - Smoothstep(EyeR * 0.62f - fwD, EyeR * 0.62f + fwD, dE));
            eye += 0.6f * Line(dE - EyeR, 0.45f, fwD);
            eye = Lerp(eye, 1f, Smoothstep(EyeR * 0.8f, EyeR * 2.5f, fwD));
            return (StructBase + StructSwing * vein) * (0.88f + 0.12f * st);
        }
    }
}
