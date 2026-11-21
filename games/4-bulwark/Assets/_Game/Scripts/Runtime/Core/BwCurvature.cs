// BwCurvature: stable curved-world maths (MASTER_PROMPT 14.7 H4, lesson 3.2 #12) and BwAtmosphereMath, the C# twin of
// Shaders/Include/BulwarkAtmosphere.hlsl (14.2.2). The curvature method is imported from The Ring's RingBend (stable
// 2R sin^2(t/2) instead of R(1 - cos t), a float32 replica next to a double reference, tests comparing them).
//   * drop of the sphere below the tangent plane at distance d: 2R sin^2(d / 2R)
//   * horizon dip for an eye at height h: sin^2(dip) = h (2R + h) / (R + h)^2  (T-NUM: crest 2.486 deg, lee ledge 2.467 deg)
//   * ray-sphere discriminant in the factored form (R + h)^2 (-dy - sin dip)(-dy + sin dip) (relative error <= 1e-5 vs double)
// BwAtmosphereMath evaluates the shader's sky model, haze and storm-slab optical depth, the 6 + 2 segment far ray with the
// earth shadow and the ray-sphere hits in double, from the same AtmoGlobals that AtmosphereDriver writes to the GPU, so
// tests (AtmosphereTests, FarFieldTests), LightCal and QA predictions use the exact shader numbers.
using System;

namespace Megastructure
{
    public static class BwCurvature
    {
        /// <summary>Mean Earth radius (m); the HLSL twin is BW_EARTH_R.</summary>
        public const double EarthR = 6371000.0;
        public const float EarthRf = 6371000f;

        // ------------------------------------------------------------------ double references
        public static double DropD(double d, double R = EarthR)
        {
            double s = Math.Sin(d / (2.0 * R));
            return 2.0 * R * s * s;
        }

        /// <summary>Horizon dip (radians) for an eye at height h above a sphere of radius R.</summary>
        public static double HorizonDipD(double h, double R = EarthR)
        {
            double s2 = Sin2Dip(h, R);
            return Math.Asin(Math.Sqrt(Math.Max(0.0, Math.Min(1.0, s2))));
        }

        /// <summary>sin^2(dip) = h (2R + h) / (R + h)^2 in double (the CPU side of the stable far-field intersection).</summary>
        public static double Sin2Dip(double h, double R = EarthR) => h * (2.0 * R + h) / ((R + h) * (R + h));

        /// <summary>
        /// Normalised discriminant of a ray from height h with direction elevation sin(el) = dy against the sphere of radius
        /// R: D/(R+h)^2 = dy^2 - sin^2(dip) = (-dy - sin dip)(-dy + sin dip). Positive: the ray hits the sphere (below the
        /// horizon).
        /// </summary>
        public static double DiscriminantD(double dy, double h, double R = EarthR)
        {
            double sd = Math.Sin(HorizonDipD(h, R));
            return (-dy - sd) * (-dy + sd);
        }

        // ------------------------------------------------------------------ float32 replicas (same operation order as HLSL)
        public static float Drop(float d, float R = EarthRf)
        {
            float s = (float)Math.Sin(d / (2f * R));
            return 2f * R * s * s;
        }

        public static float HorizonDip(float h, float R = EarthRf)
        {
            float s2 = h * (2f * R + h) / ((R + h) * (R + h));
            s2 = s2 < 0f ? 0f : (s2 > 1f ? 1f : s2);
            return (float)Math.Asin(Math.Sqrt(s2));
        }

        public static float Discriminant(float dy, float h, float R = EarthRf)
        {
            float sd = (float)Math.Sin(HorizonDip(h, R));
            return (-dy - sd) * (-dy + sd);
        }

        /// <summary>Float32 replica of BwRaySphereNear with a sin^2(dip) computed in double (the shader path).</summary>
        public static float RaySphereNearF(float rEye, float sin2Dip, float dy)
        {
            if (sin2Dip <= 0f) return -1f;
            float sd = (float)Math.Sqrt(sin2Dip);
            if (-dy <= sd) return -1f;
            float disc = (-dy - sd) * (-dy + sd);
            return rEye * sin2Dip / (-dy + (float)Math.Sqrt(disc));
        }

        /// <summary>Unstable textbook form, kept only so tests can show why it is not used: R (1 - cos(d / R)).</summary>
        public static float DropNaive(float d, float R = EarthRf) => R * (1f - (float)Math.Cos(d / R));
    }

    /// <summary>The numbers the GPU sees (BulwarkAtmosphere.hlsl globals), in double. AtmosphereDriver fills one per frame
    /// from BwParams (From) and writes exactly these values; tests build their own.</summary>
    public struct AtmoGlobals
    {
        public D3 SunDir; public double SunRadius;
        public D3 SunColor; public double DiscClamp;
        public D3 SunDisc; public double DiscVis;
        public D3 Zenith, Upper, Mid, Horizon, Glow, Halo, FogMean;
        /// <summary>Slopes (colour per radian of elevation) of the monotone stop curve at 0, 12, 35 and 90 deg (_BwSkyTan*).</summary>
        public D3 TanH, TanM, TanU, TanZ;
        public double MidMix, GlowExp, HaloWidth, GlowElev;
        /// <summary>_BwFogLow: depression scale (rad), low-sun weight, glow kept, glow depression scale (rad).</summary>
        public double LowDep, LowW, LowGlow, LowGlowDep;
        public D3 AmbSky, AmbMid, AmbGround;
        public D3 AmbExtSky, AmbExtMid, AmbExtGround, AmbInt; public double IntEdge;
        public double S0, H, SigmaS, Desat, SlabLo, SlabHi, SlabEdge, AltOffset;
        public D3 FogShadow; public double DeckW;
        public D3 DustMin, DustMax, DustColor; public double DustSigma;
        public double BreakW, HoleX, HoleZ, HoleR;
        /// <summary>_BwFarCam: eye altitude above the lowland, sin^2 dips to the lowland, the sea and the crest (-1: below).</summary>
        public double EyeH, Sin2Lowland, Sin2Sea, Sin2Crest;
        public D3 Cam;

        public static D3 V(UnityEngine.Color c) => new D3(c.r, c.g, c.b);

        /// <summary>Globals of a schedule state (BwParams) for a camera at world position cam (FarCam in double).</summary>
        public static AtmoGlobals From(in BwParams p, D3 cam)
        {
            var g = new AtmoGlobals();
            g.SunDir = new D3(p.SunDir.x, p.SunDir.y, p.SunDir.z);
            g.SunRadius = BwAtmosphereMath.SunDiscRadiusRad;
            g.SunColor = V(p.SunColor) * p.SunIntensity;
            g.DiscClamp = BulwarkConst.ClampSunDisc;
            g.SunDisc = V(p.SunDisc); g.DiscVis = p.SunDiscVis;
            g.Zenith = V(p.SkyZenith); g.Upper = V(p.SkyUpper); g.Mid = V(p.SkyMid); g.Horizon = V(p.SkyHorizon);
            g.Glow = V(p.SkyGlow); g.Halo = V(p.SkyHalo);
            BwAtmosphereMath.StopSlopes(g.Horizon, g.Mid, g.Upper, g.Zenith, out g.TanH, out g.TanM, out g.TanU, out g.TanZ);
            g.MidMix = BulwarkConst.SkyMidLerp; g.GlowExp = p.GlowExp; g.HaloWidth = p.HaloWidthRad; g.GlowElev = p.GlowElevRad;
            g.LowDep = p.FogLowDepRad; g.LowW = p.FogLowWeight; g.LowGlow = p.FogLowGlow; g.LowGlowDep = p.FogLowGlowDepRad;
            g.AmbSky = V(p.AmbSky); g.AmbMid = V(p.AmbMid); g.AmbGround = V(p.AmbGround);
            g.AmbExtSky = V(p.AmbExtSky); g.AmbExtMid = V(p.AmbExtMid); g.AmbExtGround = V(p.AmbExtGround); g.AmbInt = V(p.AmbInt);
            g.IntEdge = BulwarkSchedule.InteriorEdge;
            g.S0 = p.FogSigma0; g.H = p.FogScaleHeight; g.SigmaS = p.StormSigma; g.Desat = BulwarkConst.FogDesaturate;
            g.SlabLo = BulwarkConst.StormSlabY0; g.SlabHi = BulwarkConst.StormSlabY1; g.SlabEdge = BulwarkConst.StormSlabEdge;
            g.AltOffset = BulwarkConst.AltitudeOffset;
            g.FogShadow = V(p.FogShadow); g.DeckW = p.FogDeckWeight;
            g.DustMin = new D3(BulwarkConst.ShellXMin, 0.0, BulwarkConst.ShellZMin);
            g.DustMax = new D3(BulwarkConst.ShellXMax, BulwarkConst.ShellYMax, BulwarkConst.DoorPlaneZ);
            g.DustSigma = p.DustSigma; g.DustColor = V(p.DustColor);
            g.BreakW = p.BreakWeight; g.HoleX = p.HoleX; g.HoleZ = p.HoleZ; g.HoleR = p.HoleRadius;
            g.SetCamera(cam);
            g.FogMean = BwAtmosphereMath.FogMeanColor(in g);
            return g;
        }

        /// <summary>Per-camera far-field values (14.7 H4: sin^2(dip) in double on the CPU).</summary>
        public void SetCamera(D3 cam)
        {
            Cam = cam;
            double h = AltOffset + cam.y;
            EyeH = h;
            double R = BwCurvature.EarthR;
            Sin2Lowland = h > 0.0 ? BwCurvature.Sin2Dip(h, R) : -1.0;
            Sin2Sea = SphereSin2(h, BulwarkConst.SeaSphereOffset);
            Sin2Crest = SphereSin2(h, BulwarkConst.AltitudeOffset);
        }

        /// <summary>sin^2 of the dip to the sphere of altitude a for an eye at altitude h: ((R+h)^2 - (R+a)^2) / (R+h)^2.</summary>
        public static double SphereSin2(double h, double a)
        {
            double R = BwCurvature.EarthR;
            double rh = R + h;
            return h > a ? (h - a) * (2.0 * R + h + a) / (rh * rh) : -1.0;
        }
    }

    public static class BwAtmosphereMath
    {
        public const double Deg = Math.PI / 180.0;
        public const double SunDiscRadiusRad = 0.00465;   // 0.266 deg (KS20: 16 px across at 4K)
        public const int SegNear = 6, SegFar = 2;         // BW_ATMO_SEG_NEAR / BW_ATMO_SEG_FAR
        public const double Terminator = 300.0;           // BW_TERMINATOR
        public const double SeaOffset = BulwarkConst.SeaSphereOffset, CrestOffset = BulwarkConst.AltitudeOffset;

        static double Sat(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
        static double Smooth(double a, double b, double x) { double t = Sat((x - a) / (b - a)); return t * t * (3 - 2 * t); }
        static D3 Lerp(D3 a, D3 b, double t) => a + (b - a) * t;
        static D3 Max0(D3 a) => new D3(Math.Max(a.x, 0), Math.Max(a.y, 0), Math.Max(a.z, 0));
        public static double Luma(D3 c) => 0.2126 * c.x + 0.7152 * c.y + 0.0722 * c.z;

        // ------------------------------------------------------------------ sky (BwSkyColor, BwFogColorRaw)
        public static double SunAzLobe(in AtmoGlobals g, D3 d)
        {
            double la = Math.Sqrt(d.x * d.x + d.z * d.z), lb = Math.Sqrt(g.SunDir.x * g.SunDir.x + g.SunDir.z * g.SunDir.z);
            double c = (la > 1e-5 && lb > 1e-5) ? (d.x * g.SunDir.x + d.z * g.SunDir.z) / (la * lb) : 0.0;
            return Math.Pow(Sat(0.5 + 0.5 * c), Math.Max(g.GlowExp, 0.0));
        }

        /// <summary>Elevations (rad) of the 14.2.1 sky stops: horizon, mid (12 deg), upper (35 deg), zenith (BW_STOP_*).</summary>
        public static readonly double[] StopEl = { 0.0, 12.0 * Deg, 35.0 * Deg, 90.0 * Deg };

        /// <summary>Fritsch-Carlson slopes of the monotone C1 cubic through the four stops, per channel (colour per radian):
        /// interior stops the weighted harmonic mean of the neighbouring secants (0 at a local extremum), the horizon end the
        /// shape-preserving three-point formula, the zenith 0 (smooth through the zenith).</summary>
        public static void StopSlopes(D3 h, D3 m, D3 u, D3 z, out D3 tH, out D3 tM, out D3 tU, out D3 tZ)
        {
            tH = new D3(Slope0(h.x, m.x, u.x), Slope0(h.y, m.y, u.y), Slope0(h.z, m.z, u.z));
            tM = new D3(SlopeIn(h.x, m.x, u.x, 0), SlopeIn(h.y, m.y, u.y, 0), SlopeIn(h.z, m.z, u.z, 0));
            tU = new D3(SlopeIn(m.x, u.x, z.x, 1), SlopeIn(m.y, u.y, z.y, 1), SlopeIn(m.z, u.z, z.z, 1));
            tZ = new D3(0.0, 0.0, 0.0);
        }

        static double SlopeIn(double y0, double y1, double y2, int k)
        {
            double h0 = StopEl[k + 1] - StopEl[k], h1 = StopEl[k + 2] - StopEl[k + 1];
            double d0 = (y1 - y0) / h0, d1 = (y2 - y1) / h1;
            if (d0 * d1 <= 0.0) return 0.0;
            double w1 = 2.0 * h1 + h0, w2 = h1 + 2.0 * h0;
            return (w1 + w2) / (w1 / d0 + w2 / d1);
        }

        static double Slope0(double y0, double y1, double y2)
        {
            double h0 = StopEl[1] - StopEl[0], h1 = StopEl[2] - StopEl[1];
            double d0 = (y1 - y0) / h0, d1 = (y2 - y1) / h1;
            double m = ((2.0 * h0 + h1) * d0 - h0 * d1) / (h0 + h1);
            if (m * d0 <= 0.0) return 0.0;
            if (d0 * d1 < 0.0 && Math.Abs(m) > 3.0 * Math.Abs(d0)) return 3.0 * d0;
            return m;
        }

        static D3 Hermite(D3 y0, D3 y1, D3 m0, D3 m1, double x0, double x1, double x)
        {
            double h = x1 - x0, t = Sat((x - x0) / h), t2 = t * t, t3 = t2 * t;
            return y0 * (2.0 * t3 - 3.0 * t2 + 1.0) + m0 * ((t3 - 2.0 * t2 + t) * h) + y1 * (3.0 * t2 - 2.0 * t3) + m1 * ((t3 - t2) * h);
        }

        /// <summary>BwSkyStops: the stop curve at elevation e >= 0 (rad).</summary>
        public static D3 SkyStops(in AtmoGlobals g, double e)
        {
            if (e < StopEl[1]) return Hermite(g.Horizon, g.Mid, g.TanH, g.TanM, StopEl[0], StopEl[1], e);
            if (e < StopEl[2]) return Hermite(g.Mid, g.Upper, g.TanM, g.TanU, StopEl[1], StopEl[2], e);
            return Hermite(g.Upper, g.Zenith, g.TanU, g.TanZ, StopEl[2], StopEl[3], e);
        }

        public static D3 SkyBase(in AtmoGlobals g, D3 d, double glowScale)
        {
            double el = Math.Asin(Math.Max(-1.0, Math.Min(1.0, d.y)));
            double e = Math.Max(el, 0.0);
            D3 c = SkyStops(in g, e);
            double w = glowScale * SunAzLobe(in g, d) * Math.Exp(-e / Math.Max(g.GlowElev, 1e-3));
            return Lerp(c, g.Glow, w);
        }

        public static D3 HaloTerm(in AtmoGlobals g, D3 d) => g.Halo * Math.Exp(-(d - g.SunDir).Length / Math.Max(g.HaloWidth, 1e-4));

        public static D3 SkyColor(in AtmoGlobals g, D3 dir)
        {
            D3 d = dir.Normalized;
            return Max0(SkyBase(in g, d, 1.0) + HaloTerm(in g, d));
        }

        /// <summary>Sky colour at azimuth az (deg) and elevation el (deg).</summary>
        public static D3 SkyAt(in AtmoGlobals g, double azDeg, double elDeg) => SkyColor(in g, Dir(azDeg, elDeg));

        public static D3 Dir(double azDeg, double elDeg)
        {
            double a = azDeg * Deg, e = elDeg * Deg;
            return new D3(Math.Cos(e) * Math.Sin(a), Math.Sin(e), Math.Cos(e) * Math.Cos(a));
        }

        public static D3 FogColorRaw(in AtmoGlobals g, D3 dir)
        {
            D3 d = dir.Normalized;
            double hz = Math.Sqrt(d.x * d.x + d.z * d.z);
            double elTrue = Math.Atan2(d.y, Math.Max(hz, 1e-6));
            double el = Math.Max(BulwarkConst.FogElevMin * Deg, Math.Min(BulwarkConst.FogElevMax * Deg, elTrue));
            double hx = hz > 1e-6 ? d.x / hz : 0.0, hzz = hz > 1e-6 ? d.z / hz : 1.0;
            D3 fd = new D3(hx * Math.Cos(el), Math.Sin(el), hzz * Math.Cos(el));
            double glow = Math.Exp(-Math.Max(-elTrue, 0.0) / (6.0 * Deg));
            double w = hz * hz;
            D3 c = Lerp(g.FogMean, SkyBase(in g, fd, glow), w * w) + HaloTerm(in g, d);
            if (g.LowW > 0.0)
            {
                double dep = Math.Max(-elTrue - VisibleDip(in g), 0.0);
                double x = dep / Math.Max(g.LowDep, 1e-4);
                double wLow = (1.0 - Math.Exp(-x * x)) * g.LowW;
                D3 jLow = Lerp(g.FogShadow, g.Glow, g.LowGlow * SunAzLobe(in g, d) * Math.Exp(-dep / Math.Max(g.LowGlowDep, 1e-4)));
                c = Lerp(c, jLow, wLow);
            }
            return Max0(c);
        }

        /// <summary>BwVisibleDip: the dip of the visible lowland horizon (rad), 0 when the eye is below it.</summary>
        public static double VisibleDip(in AtmoGlobals g) => g.Sin2Lowland > 0.0 ? Math.Asin(Math.Sqrt(Sat(g.Sin2Lowland))) : 0.0;

        /// <summary>BwDiscTransmittance: per-channel disc transmittance for a grey T (extinction ratios 0.8 / 1.0 / 1.25).</summary>
        public static D3 DiscTransmittance(double T)
        {
            double tau = -Math.Log(Math.Max(T, 1e-6));
            return new D3(Math.Exp(-0.8 * tau), Math.Exp(-tau), Math.Exp(-1.25 * tau));
        }

        // ------------------------------------------------------------------ region ambient (BwInteriorWeight, BwAmbientAt)
        /// <summary>BwInteriorWeight: max over the interior boxes of the product of soft (+-edge) inside-ness per axis.</summary>
        public static double InteriorWeightAt(in AtmoGlobals g, D3 p)
        {
            double e = Math.Max(g.IntEdge, 1e-3), w = 0.0;
            double[] b = BulwarkSchedule.InteriorBoxes;
            for (int k = 0; k + 5 < b.Length; k += 6)
            {
                double ix = Math.Min(Sat((p.x - b[k]) / (2 * e) + 0.5), Sat((b[k + 3] - p.x) / (2 * e) + 0.5));
                double iy = Math.Min(Sat((p.y - b[k + 1]) / (2 * e) + 0.5), Sat((b[k + 4] - p.y) / (2 * e) + 0.5));
                double iz = Math.Min(Sat((p.z - b[k + 2]) / (2 * e) + 0.5), Sat((b[k + 5] - p.z) / (2 * e) + 0.5));
                w = Math.Max(w, ix * iy * iz);
            }
            return w;
        }

        /// <summary>BwAmbientAt: exterior trilight or interior ambient by region.</summary>
        public static D3 AmbientAt(in AtmoGlobals g, D3 n, D3 p)
        {
            double sky = Sat(n.y), ground = Sat(-n.y), mid = 1.0 - sky - ground;
            D3 ext = g.AmbExtSky * sky + g.AmbExtMid * mid + g.AmbExtGround * ground;
            D3 inn = g.AmbInt * (sky + mid + 0.8 * ground);
            return Lerp(ext, inn, InteriorWeightAt(in g, p));
        }

        /// <summary>Azimuth mean of the sky at 2.5 deg elevation (16 azimuths): the fog colour of steep rays (_BwSkyShape.yzw).</summary>
        public static D3 FogMeanColor(in AtmoGlobals g)
        {
            D3 sum = new D3(0, 0, 0);
            for (int i = 0; i < 16; i++) sum = sum + SkyColor(in g, Dir(i * 22.5, 2.5));
            return sum * (1.0 / 16.0);
        }

        public static D3 FogDesat(in AtmoGlobals g, D3 c, double T) { double l = Luma(c); return Lerp(c, new D3(l, l, l), g.Desat * (1.0 - T)); }

        // ------------------------------------------------------------------ optical depth (BwHazeTau, BwStormTau)
        /// <summary>Haze along a linear-altitude segment: sigma0 e^(-h0/H) d (e^x - 1)/x, x = (h0 - h1)/H (14.2.2 closed form).</summary>
        public static double HazeTau(in AtmoGlobals g, double h0, double h1, double d)
        {
            double H = Math.Max(g.H, 1.0);
            double x = (h0 - h1) / H;
            double f = Math.Abs(x) < 1e-3 ? 1.0 + 0.5 * x : (Math.Exp(x) - 1.0) / x;
            return g.S0 * Math.Exp(-h0 / H) * d * f;
        }

        /// <summary>The 14.2.2 text form: tau = s0 H / sin(g) (exp(-h_end/H) - exp(-h_eye/H)); |sin g| below 1e-3: s0 exp(-h/H) d.</summary>
        public static double HazeTauSpec(double s0, double H, double hEye, double hEnd, double d)
        {
            double sg = (hEye - hEnd) / d;
            if (Math.Abs(sg) < 1e-3) return s0 * Math.Exp(-0.5 * (hEye + hEnd) / H) * d;
            return s0 * H / sg * (Math.Exp(-hEnd / H) - Math.Exp(-hEye / H));
        }

        static double RampR(double h, double a, double w)
        {
            double t = h - a;
            return t <= 0.0 ? 0.0 : (t < w ? t * t / (2.0 * w) : t - 0.5 * w);
        }

        public static double SlabDensity(double h, double lo, double hi, double w) => Sat((h - (lo - 0.5 * w)) / w) - Sat((h - (hi - 0.5 * w)) / w);

        public static double StormTau(in AtmoGlobals g, double h0, double h1, double d)
        {
            if (g.SigmaS <= 0.0) return 0.0;
            double lo = g.SlabLo, hi = g.SlabHi, w = Math.Max(g.SlabEdge, 1.0);
            double dh = h1 - h0;
            double frac;
            if (Math.Abs(dh) < 10.0)
                frac = (SlabDensity(h0, lo, hi, w) + 4.0 * SlabDensity(0.5 * (h0 + h1), lo, hi, w) + SlabDensity(h1, lo, hi, w)) / 6.0;
            else
            {
                double a1 = lo - 0.5 * w, a2 = hi - 0.5 * w;
                double i1 = RampR(h1, a1, w) - RampR(h1, a2, w);
                double i0 = RampR(h0, a1, w) - RampR(h0, a2, w);
                frac = Sat((i1 - i0) / dh);
            }
            return g.SigmaS * frac * d;
        }

        /// <summary>Near fog (BwFogApply without the dust): transmittance from the camera to world point p (flat altitudes).</summary>
        public static double NearTransmittance(in AtmoGlobals g, D3 p)
        {
            double d = (p - g.Cam).Length;
            double h0 = g.AltOffset + g.Cam.y, h1 = g.AltOffset + p.y;
            return Math.Exp(-(HazeTau(in g, h0, h1, d) + StormTau(in g, h0, h1, d)));
        }

        public static D3 FogApply(in AtmoGlobals g, D3 color, D3 p)
        {
            D3 v = p - g.Cam;
            double d = Math.Max(v.Length, 1e-4);
            double T = NearTransmittance(in g, p);
            D3 fogc = FogDesat(in g, FogColorRaw(in g, v * (1.0 / d)), T);
            return color * T + fogc * (1.0 - T);
        }

        // ------------------------------------------------------------------ far rays (BwAltAt, BwRaySphereNear/Exit, BwAtmoRay)
        public static double AltAt(double h0, double dy, double t)
        {
            double R = BwCurvature.EarthR, r0 = R + h0;
            double n = h0 * (2.0 * R + h0) + t * (2.0 * r0 * dy + t);
            double rr = r0 * r0 + t * (2.0 * r0 * dy + t);
            return n / (Math.Sqrt(Math.Max(rr, 0.0)) + R);
        }

        /// <summary>Near hit with a sphere for an eye outside it (sin2Dip from SphereSin2); -1 above the horizon.</summary>
        public static double RaySphereNear(double rEye, double sin2Dip, double dy)
        {
            if (sin2Dip <= 0.0) return -1.0;
            double sd = Math.Sqrt(sin2Dip);
            if (-dy <= sd) return -1.0;
            double disc = (-dy - sd) * (-dy + sd);
            return rEye * sin2Dip / (-dy + Math.Sqrt(disc));
        }

        public static double RaySphereExit(double h0, double hTop, double dy)
        {
            double R = BwCurvature.EarthR, r0 = R + h0;
            double q = (hTop - h0) * (2.0 * R + hTop + h0);
            double b = r0 * dy;
            double s = Math.Sqrt(Math.Max(b * b + q, 0.0));
            return b > 0.0 ? q / (b + s) : s - b;
        }

        static double HazeTauAbove(in AtmoGlobals g, double hA, double hB, double d, double hx)
        {
            if (hA <= hx && hB <= hx) return 0.0;
            if (hA >= hx && hB >= hx) return HazeTau(in g, hA, hB, d);
            double f = (hx - hA) / (hB - hA);
            return hA > hx ? HazeTau(in g, hA, hx, d * f) : HazeTau(in g, hx, hB, d * (1.0 - f));
        }

        /// <summary>Signed earth-shadow altitude at a point with local up n: R (1/cos el - 1) when the sun is below its horizon,
        /// negative when it is above (BwShadowAltitude).</summary>
        public static double ShadowAltitude(in AtmoGlobals g, D3 n)
        {
            double se = D3.Dot(g.SunDir, n);
            double ce = Math.Sqrt(Sat(1.0 - se * se));
            double hs = BwCurvature.EarthR * se * se / Math.Max(ce * (1.0 + ce), 1e-6);
            return se >= 0.0 ? -hs : hs;
        }

        static void Segment(in AtmoGlobals g, D3 dir, D3 C, double tA, double hA, double tB, double hB, D3 jLit, ref double T, ref D3 L)
        {
            double d = tB - tA;
            double tauH = HazeTau(in g, hA, hB, d);
            double tau = tauH + StormTau(in g, hA, hB, d);
            D3 J = jLit;
            if (tauH > 1e-7)
            {
                D3 pm = dir * (0.5 * (tA + tB));
                D3 n = (pm - C).Normalized;
                double hs = ShadowAltitude(in g, n);
                double wE = 0.5 * (HazeTauAbove(in g, hA, hB, d, hs - Terminator) + HazeTauAbove(in g, hA, hB, d, hs + Terminator)) / tauH;
                double wD = 1.0;
                if (g.DeckW > 0.0)
                {
                    double deck = g.SlabLo - 0.5 * g.SlabEdge;
                    double under = 1.0 - HazeTauAbove(in g, hA, hB, d, deck) / tauH;
                    double hole = SunHoleAt(in g, g.Cam.x + pm.x, g.Cam.z + pm.z, 0.5 * (hA + hB));
                    wD = 1.0 - under * g.DeckW * (1.0 - hole);
                }
                // the slab below the eye, for rays below the visible horizon (KS05; BwAtmoSegment)
                double depS = Math.Max(-Math.Asin(Math.Max(-1.0, Math.Min(1.0, dir.y))) - VisibleDip(in g), 0.0) / (1.5 * Deg);
                double sLit = 1.0 - g.DeckW * Sat((g.EyeH - 0.5 * (hA + hB)) / 400.0) * (1.0 - Math.Exp(-depS * depS));
                double w = Sat((Math.Min(wE, wD) * tauH + sLit * (tau - tauH)) / Math.Max(tau, 1e-9));
                J = Lerp(g.FogShadow, jLit, w);
            }
            double e = Math.Exp(-tau);
            L = L + J * (T * (1.0 - e));
            T *= e;
        }

        /// <summary>BwSunBreakAt: 1 - 0.85 w (1 - hole) for the sun shadow of (x, z, h) on the ground in the lit disc.</summary>
        public static double SunBreakAt(in AtmoGlobals g, double x, double z, double h)
        {
            double w = g.BreakW;
            if (w <= 0.0) return 1.0;
            double k = Math.Max(h, 0.0) / Math.Max(g.SunDir.y, 0.02);
            double px = x - g.SunDir.x * k, pz = z - g.SunDir.z * k;
            double dist = Math.Sqrt((px - g.HoleX) * (px - g.HoleX) + (pz - g.HoleZ) * (pz - g.HoleZ));
            double hole = Sat((g.HoleR - dist) / 3000.0 + 0.5) * Sat(g.HoleR / 3000.0);
            return 1.0 - 0.85 * w * (1.0 - hole);
        }

        public static double SunHoleAt(in AtmoGlobals g, double x, double z, double h)
        {
            if (g.BreakW <= 0.0) return 0.0;
            return Sat((SunBreakAt(in g, x, z, h) - (1.0 - 0.85 * g.BreakW)) / Math.Max(0.85 * g.BreakW, 1e-4));
        }

        /// <summary>Distance to the horizon of the lowland, (R + h) sin(dip): the far ray's node split.</summary>
        public static double HorizonDistance(in AtmoGlobals g) => g.Sin2Lowland > 0.0 ? (BwCurvature.EarthR + g.EyeH) * Math.Sqrt(g.Sin2Lowland) : double.PositiveInfinity;

        /// <summary>BwAtmoRay: transmittance T and in-scatter L (not desaturated) along a far ray of length tEnd: segNear
        /// uniform segments up to min(tEnd, tH), segFar beyond (tH = the horizon distance).</summary>
        public static void AtmoRay(in AtmoGlobals g, D3 dir, double tEnd, D3 jLit, out double T, out D3 L, int segNear = SegNear, int segFar = SegFar)
        {
            double h0 = g.EyeH, dy = dir.y;
            D3 C = new D3(0.0, -(BwCurvature.EarthR + h0), 0.0);
            double tN = Math.Min(tEnd, HorizonDistance(in g));
            T = 1.0; L = new D3(0, 0, 0);
            double tA = 0.0, hA = h0;
            for (int i = 1; i <= segNear; i++)
            {
                double tB = tN * i / segNear, hB = AltAt(h0, dy, tB);
                Segment(in g, dir, C, tA, hA, tB, hB, jLit, ref T, ref L);
                tA = tB; hA = hB;
            }
            if (tEnd > tN)
                for (int k = 1; k <= segFar; k++)
                {
                    double tB = tN + (tEnd - tN) * k / segFar, hB = AltAt(h0, dy, tB);
                    Segment(in g, dir, C, tA, hA, tB, hB, jLit, ref T, ref L);
                    tA = tB; hA = hB;
                }
        }

        /// <summary>Transmittance only along a far ray (no colour).</summary>
        public static double FarTransmittance(in AtmoGlobals g, D3 dir, double tEnd, int segNear = SegNear)
        {
            AtmoRay(in g, dir, tEnd, new D3(0, 0, 0), out double T, out _, segNear, SegFar);
            return T;
        }

        public static double AtmoExit(in AtmoGlobals g, D3 dir)
        {
            double hTop = Math.Max(g.SlabHi + g.SlabEdge, g.EyeH) + 3.0 * Math.Max(g.H, 1.0);
            return RaySphereExit(g.EyeH, hTop, dir.y);
        }

        public static D3 FogApplyFar(in AtmoGlobals g, D3 color, D3 dir, double t)
        {
            AtmoRay(in g, dir, t, FogColorRaw(in g, dir), out double T, out D3 L);
            return color * T + FogDesat(in g, L, T);
        }

        /// <summary>BwSkyVisible: the raw sky seen through the atmosphere (ground rays: up to the lowland hit).</summary>
        public static D3 SkyVisible(in AtmoGlobals g, D3 dir)
        {
            D3 d = dir.Normalized;
            double tl = RaySphereNear(BwCurvature.EarthR + g.EyeH, g.Sin2Lowland, d.y);
            double t = tl > 0.0 ? tl : AtmoExit(in g, d);
            return FogApplyFar(in g, SkyColor(in g, d), d, t);
        }

        // ------------------------------------------------------------------ geometry helpers (FarFieldTests)
        /// <summary>Ground point on the lowland sphere at arc distance D below an eye at altitude h: direction and distance.</summary>
        public static void GroundRay(double h, double D, out D3 dir, out double t)
        {
            double R = BwCurvature.EarthR, phi = D / R;
            double gx = R * Math.Sin(phi), gy = R * Math.Cos(phi) - (R + h);
            t = Math.Sqrt(gx * gx + gy * gy);
            dir = new D3(gx / t, gy / t, 0.0);
        }

        /// <summary>Transmittance of the haze alone from an eye at altitude h to the lowland at arc distance D (14.2.2 check).</summary>
        public static double LowlandTransmittance(double s0, double h, double D, int segNear = SegNear)
        {
            var g = new AtmoGlobals { S0 = s0, H = BulwarkConst.HazeScaleHeight, SigmaS = 0.0, AltOffset = BulwarkConst.AltitudeOffset, EyeH = h, Sin2Lowland = BwCurvature.Sin2Dip(h) };
            GroundRay(h, D, out D3 dir, out double t);
            return FarTransmittance(in g, dir, t, segNear);
        }

        /// <summary>Reference: exact numeric integral of the haze along the same straight ray (midpoint rule, n steps).</summary>
        public static double LowlandTransmittanceExact(double s0, double h, double D, int n = 20000)
        {
            GroundRay(h, D, out D3 dir, out double t);
            double H = BulwarkConst.HazeScaleHeight, sum = 0.0;
            for (int i = 0; i < n; i++) sum += Math.Exp(-AltAt(h, dir.y, (i + 0.5) * t / n) / H);
            return Math.Exp(-s0 * sum * t / n);
        }

        /// <summary>Storm slab alone along a horizontal ray of length d from altitude h (14.2.2: 0.52 at 1 km, 0.21 at 2.4 km).</summary>
        public static double StormHorizontalTransmittance(double sigmaS, double h, double d)
        {
            var g = new AtmoGlobals
            {
                S0 = 0.0, H = BulwarkConst.HazeScaleHeight, SigmaS = sigmaS, SlabLo = BulwarkConst.StormSlabY0, SlabHi = BulwarkConst.StormSlabY1,
                SlabEdge = BulwarkConst.StormSlabEdge, AltOffset = BulwarkConst.AltitudeOffset, EyeH = h,
            };
            return FarTransmittance(in g, new D3(1, 0, 0), d);
        }
    }
}
