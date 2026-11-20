// AtmosphereTests: EditMode tests of the atmosphere single source of truth (MASTER_PROMPT 14.2.2, 14.7 H1 / H3, D-006, R2
// section 2) on its double-precision mirror NocFogMath (the HLSL in NocAtmosphere.hlsl is a line-by-line twin):
//   T-FOG (the 14.2.2 checked values, sheets off, within 0.002) and the R2 sheet-on path values; the path forms against numeric
//   quadrature of the physical density; T monotone along 180 downward rays (sheets on, drifting); KS02 slit and KS00 mouth
//   (D-006 alley haze); horizon agreement of fog, sky and dome at 12 elevations (CIE76 dE <= 2.0) for every beat; the abyss
//   AbyssNear -> AbyssGlow monotone per channel; no step >= 2 codes in the luma profile through the horizon (KS09, 1080p
//   and 4K); the cloud scroll wraps exactly; the sky has no seam at the cloud fade-in. Look v2 (LOOK_V2 3): sigma 4.4e-4 in
//   beats 2-4, the cloud tint model (hot lid over the boulevard, cool deck over the Heart, never warm over the Old Quarter), the
//   4-octave noise (mean 0.5, std about 0.111, octave fade below 8 px), the HazeLow share of the alley layer (0.55 at eye level,
//   0.20 at the zenith) and the LOOK_V2 3.2 display-luma table (+-3 codes, noise at its mean, through NocGradeMath.Display; the
//   abyss columns are AT's Phase 4 model: base rise 1 - (1 - |e| / 90)^1.3 and the three sheets at their mean coverage). The
//   abyss itself (sheet bands, nadir structure, no surface below y -2 500, the HLSL constants) is tested in AtmosphereAbyssTests.
using System;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class AtmosphereTests
    {
        const double Sig = 4.4e-4;

        static NocAtmoState Clean(double sigma = Sig, bool sheets = false)
        {
            var st = new NocAtmoState { SigmaR = sigma, AlleyExtra = 0, SheetStrength = sheets ? NocAbyss.SheetFogStrength : 0, Rho = 0 };
            st.SheetAmp = sheets ? 1.0 : 0.0;
            NocFogMath.UpdateSheetE(st);
            return st;
        }

        static double T(NocAtmoState st, double x, double y, double z) => NocFogMath.FogT(st, x, y, z);

        // ---------------------------------------------------------------- T-FOG
        [Test]
        public void TFog_SpecValuesSheetsOff()
        {
            var st = Clean();
            // horizontal at the eye: d 200 m .. 6.5 km (fragment y 1.7, height factor 1)
            double[] d = { 200, 1000, 2000, 3000, 5000, 6500 };
            double[] want = { 0.992, 0.824, 0.461, 0.175, 0.008, 0.0003 };
            for (int i = 0; i < d.Length; i++) Assert.AreEqual(want[i], T(st, d[i], 1.7, 0), 0.002, "horizontal " + d[i]);
            // straight down from the eye
            double[] y = { -1000, -1500, -2000, -2500, -3300 };
            double[] wd = { 0.314, 0.033, 0.002, 0.0001, 0.0 };
            for (int i = 0; i < y.Length; i++) Assert.AreEqual(wd[i], T(st, 0, y[i], 0), 0.002, "down " + y[i]);
            // B face lower strata (y -800 at 943 m)
            double dz = Math.Sqrt(943.0 * 943.0 - (801.7 * 801.7));
            Assert.AreEqual(0.471, T(st, 0, -800, dz), 0.005, "B face strata");
        }

        [Test]
        public void TFog_SheetsOnPathValues()
        {
            var st = Clean(Sig, true);
            double[] y = { -250, -700, -1000, -1500, -2000, -2500 };
            // R2 2.2 (path model) at 4.4e-4 with the AT sheet strength 1.5 (v1 0.5: 0.981, 0.678, 0.279, 0.024, 0.0014; B face 0.420);
            // values from an independent erf / quadrature evaluation of the same density (Python, AT log)
            double[] want = { 0.975, 0.615, 0.216, 0.013, 0.0005, 0.0 };
            for (int i = 0; i < y.Length; i++) Assert.AreEqual(want[i], T(st, 0, y[i], 0), 0.002, "down " + y[i]);
            Assert.AreEqual(0.327, T(st, 0, -800, Math.Sqrt(943.0 * 943.0 - 801.7 * 801.7)), 0.005, "B face strata, sheets on");
            Assert.Less(T(st, 0, -3300, 0), 1e-4, "geometry bottoms invisible (H3)");
        }

        [Test]
        public void OpticalDepthEqualsQuadratureOfTheDensity()
        {
            var st = Clean(Sig, true);
            st.AlleyExtra = 4.64e-3;
            var rnd = new Random(7);
            for (int k = 0; k < 60; k++)
            {
                double el = -89 + 178 * rnd.NextDouble(), az = 360 * rnd.NextDouble(), d = 5 + 4000 * rnd.NextDouble();
                double e = el * Math.PI / 180, a = az * Math.PI / 180;
                double x = d * Math.Cos(e) * Math.Sin(a), y = 1.7 + d * Math.Sin(e), z = d * Math.Cos(e) * Math.Cos(a);
                double path = NocFogMath.OpticalDepth(st, x, y, z);
                // trapezoid quadrature of the density along the ray (the height term is exact only as an endpoint form of
                // the smoothstep antiderivative, which holds for rays from y 0: tolerance covers the 1.7 m eye offset)
                int n = 20000; double sum = 0;
                for (int i = 0; i <= n; i++)
                {
                    double t = (double)i / n, yy = 1.7 + (y - 1.7) * t;
                    double w = (i == 0 || i == n) ? 0.5 : 1.0;
                    sum += w * NocFogMath.Density(st, yy, Sig);
                }
                double quad = sum / n * d;
                Assert.AreEqual(quad, path, Math.Max(2e-3 * quad, 2e-4), FormattableString.Invariant($"el {el:F1} d {d:F0}"));
            }
        }

        [Test]
        public void TransmittanceIsMonotoneAlongDownwardRays()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 520, 37.0);                     // beat 3, drifted sheets
            for (int k = 0; k < 180; k++)
            {
                double el = -1 - 88.0 * k / 179.0, e = el * Math.PI / 180;
                double prev = 1.0;
                for (double d = 5; d <= 9000; d += 5)
                {
                    double t = T(st, d * Math.Cos(e), 1.7 + d * Math.Sin(e), 0);
                    Assert.LessOrEqual(t, prev + 1e-12, FormattableString.Invariant($"T rises along the {el:F1} deg ray at {d} m"));
                    prev = t;
                }
            }
        }

        // ---------------------------------------------------------------- D-006 alley haze
        [Test]
        public void AlleyHaze_KS00MouthAndKS02Slit()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 0, 0);
            Assert.AreEqual(4.56e-3, st.AlleyExtra, 1e-9);
            Assert.AreEqual(4.4e-4, st.SigmaR, 1e-12);
            Assert.AreEqual(0.698, T(st, 0, 1.7, 120), 0.01, "KS00: the mouth at 120 m keeps the spec alley fog");
            NocFogMath.StateAt(st, 88, 0);
            st.CamZ = 88;
            double sigSky = Math.Min(st.SigmaR, NocturneConstants.CloudSkySigmaMax);
            foreach (double el in new[] { 60.0, 75.0, 90.0 })
            {
                double e = el * Math.PI / 180, t = (460 - 1.7) / Math.Sin(e);
                double tau = NocFogMath.OpticalDepth(st, 0, 460, 88 + t * Math.Cos(e), sigSky);
                // D-006 kept the slit's cloud readable (>= 0.90 at the regional 3.6e-4); LOOK_V2 3.1 raises the regional sigma under
                // the alley layer to 4.4e-4 (the alley total stays 5.0e-3): T 0.885 / 0.902 / 0.908 at el 60 / 75 / 90. The fog
                // colour is the sky function itself, so the cloud's structure loses at most 12 % of its contrast
                Assert.GreaterOrEqual(Math.Exp(-tau * tau), 0.875, "KS02 cloud at el " + el);
            }
            double ef = 86.5 * Math.PI / 180;
            Assert.GreaterOrEqual(T(st, 0, 1.7 + 491 * Math.Sin(ef), 88 + 491 * Math.Cos(ef)), 0.9, "KS02 tower flank 491 m");
            NocFogMath.StateAt(st, 124, 0);
            Assert.AreEqual(0.0, st.AlleyExtra, 1e-12, "the alley haze ends with the 14.6 window");
        }

        // ---------------------------------------------------------------- sky
        [Test]
        public void Abyss_AbyssNearToAbyssGlowMonotone()
        {
            C3 prev = NocFogMath.SkyGradientC(-6, null);
            var an = NocFogMath.Pal(NocPal.AbyssNear); var ag = NocFogMath.Pal(NocPal.AbyssGlow);
            for (double e = -6.1; e >= -90; e -= 0.1)
            {
                var c = NocFogMath.SkyGradientC(e, null);
                Assert.IsTrue(Monotone(prev.r, c.r, an.r, ag.r) && Monotone(prev.g, c.g, an.g, ag.g) && Monotone(prev.b, c.b, an.b, ag.b), "dip at " + e);
                prev = c;
            }
            var end = NocFogMath.SkyGradientC(-90, null);
            Assert.AreEqual(ag.r, end.r, 1e-9); Assert.AreEqual(ag.b, end.b, 1e-9);
        }

        static bool Monotone(double prev, double cur, double from, double to) => to >= from ? cur >= prev - 1e-12 : cur <= prev + 1e-12;

        [Test]
        public void HorizonAgreement_FogSkyDome_12Elevations()
        {
            double[] els = { -30, -20, -12, -8, -5, -3, -2, -1, -0.5, 0, 1, 3 };
            double[] beats = { 0, 250, 520, 720, 950, 1150, 1350 };
            var facade = new C3(0.02, 0.02, 0.035);               // a dark lit facade (radiance)
            double worst = 0;
            foreach (double s in beats)
            {
                var st = new NocAtmoState();
                NocFogMath.StateAt(st, s, 100.0);
                st.CamX = 300; st.CamZ = 195;
                foreach (double el in els)
                {
                    double e = el * Math.PI / 180, az = 0.7;
                    double dx = Math.Cos(e) * Math.Sin(az), dy = Math.Sin(e), dz = Math.Cos(e) * Math.Cos(az);
                    double d = 6500;                                // the city's edge (H3)
                    if (dy < 0) d = Math.Min(d, (1.7 + 3300) / -dy); // geometry bottoms
                    var sky = NocFogMath.SkyColor(st, dx, dy, dz);
                    var dome = NocFogMath.Floor(sky);
                    var fogged = NocFogMath.Floor(NocFogMath.Fog(st, facade, st.CamX + dx * d, 1.7 + dy * d, st.CamZ + dz * d));
                    double de = DeltaE76(dome, fogged);
                    worst = Math.Max(worst, de);
                    Assert.LessOrEqual(de, 2.0, FormattableString.Invariant($"s {s} el {el}: dome {dome} fog {fogged}"));
                    var far = NocFogMath.Floor(NocFogMath.Fog(st, facade, st.CamX + dx * 20000, 1.7 + dy * 20000, st.CamZ + dz * 20000));
                    Assert.LessOrEqual(DeltaE76(dome, far), 0.05, "fully fogged geometry = the dome colour");
                }
            }
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] horizon agreement: worst CIE76 dE {worst:F3} over 12 elevations x 7 beats"));
        }

        [Test]
        public void HorizonLumaProfile_NoStepOf2Codes_KS09()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 520, 100.0);
            st.CamX = 335.3; st.CamZ = 195;
            foreach (int H in new[] { 1080, 2160 })
            {
                double f = 0.5 * H / Math.Tan(34.0 * Math.PI / 180.0);   // vFOV 68 (KS09)
                double maxStep = 0, prev = double.NaN;
                for (int row = 0; row < H; row++)
                {
                    double el = -18 + Math.Atan((H * 0.5 - row - 0.5) / f) * 180 / Math.PI;   // KS09 pitch -18
                    if (el < -12 || el > 12) continue;
                    double e = el * Math.PI / 180, az = 20 * Math.PI / 180;
                    var c = NocFogMath.Floor(NocFogMath.SkyColor(st, Math.Cos(e) * Math.Sin(az), Math.Sin(e), Math.Cos(e) * Math.Cos(az), 2 * Math.Tan(34 * Math.PI / 180) / H));
                    double y = 255.0 * NocMath.Luma709(NocturnePalette.LinearToSrgb(c.r), NocturnePalette.LinearToSrgb(c.g), NocturnePalette.LinearToSrgb(c.b));
                    if (!double.IsNaN(prev)) maxStep = Math.Max(maxStep, Math.Abs(y - prev));
                    prev = y;
                }
                Assert.Less(maxStep, 1.0, FormattableString.Invariant($"{H}p: adjacent rows differ by {maxStep:F3} codes (quantised steps would reach 2)"));
            }
        }

        [Test]
        public void Sky_CloudFadeHasNoSeam()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 300, 50.0);
            st.CamX = 115; st.CamZ = 195;
            double prev = double.NaN, maxStep = 0;
            for (double el = 1.0; el <= 12.0; el += 0.01)
            {
                double e = el * Math.PI / 180;
                var c = NocFogMath.Floor(NocFogMath.SkyColor(st, Math.Cos(e), Math.Sin(e), 0));
                double y = 255.0 * NocMath.Luma709(NocturnePalette.LinearToSrgb(c.r), NocturnePalette.LinearToSrgb(c.g), NocturnePalette.LinearToSrgb(c.b));
                if (!double.IsNaN(prev)) maxStep = Math.Max(maxStep, Math.Abs(y - prev));
                prev = y;
            }
            Assert.Less(maxStep, 0.5, "0.01 deg steps through the cloud fade-in (3..5 deg)");
        }

        [Test]
        public void CloudScrollWrapsExactly()
        {
            int p = NocturneConstants.CloudNoisePeriod;
            for (int i = 0; i < 50; i++)
            {
                double x = i * 3.17 - 40, y = i * 1.31 + 7;
                Assert.AreEqual(NocFogMath.ValueNoiseP(x, y, p), NocFogMath.ValueNoiseP(x + p, y, p), 1e-12);
                Assert.AreEqual(NocFogMath.ValueNoiseP(x, y, p), NocFogMath.ValueNoiseP(x, y - p, p), 1e-12);
            }
            var a = new NocAtmoState(); var b = new NocAtmoState();
            NocFogMath.StateAt(a, 300, 1000.0); NocFogMath.StateAt(b, 300, 1000.0 + 1e-3);
            Assert.Less(Math.Abs(a.Cloud1X - b.Cloud1X), 0.01, "scroll continuous");
            Assert.AreEqual(a.SheetY[0], b.SheetY[0], 1e-3, "sheets drift slowly");
        }

        // ---------------------------------------------------------------- Look v2 deck, HazeLow and the display-luma table
        /// <summary>Displayed luma (codes) of the sky in direction (e, az) for state st at the beat's EV and saturation, the cloud
        /// noise at its mean (a huge blur fades every octave), through NocFloor and NocGradeMath.Display.</summary>
        static double SkyLuma(NocAtmoState st, double eDeg, double azDeg, double ev, double sat)
        {
            double e = eDeg * Math.PI / 180, a = azDeg * Math.PI / 180;
            var c = NocFogMath.Floor(NocFogMath.SkyColor(st, Math.Cos(e) * Math.Sin(a), Math.Sin(e), Math.Cos(e) * Math.Cos(a), 10.0));
            NocGradeMath.Display(c.r, c.g, c.b, ev, sat, 0.0, 1.0, out double r, out double g, out double b);
            return 255.0 * NocMath.Luma709(NocturnePalette.LinearToSrgb(r), NocturnePalette.LinearToSrgb(g), NocturnePalette.LinearToSrgb(b));
        }

        [Test]
        public void LookV2_DisplayLumaTable_3_2()
        {
            // LOOK_V2 3.2 (beats 2, 3, 5, 7; EV / saturation of section 7), +-3 codes; base deck far from every glow. Abyss columns
            // (e -6 / -30 / -60 / -90): AT's model at the noise mean (v1 3.2: 40.9 / 50.1 / 60.1 / 65.9 at beat 3): the band of the
            // upper sheet at -5.7 lifts e -6 to 48.5, the faster base rise lifts the shallow drop (LOOK_V2 8.1 drop 50-66)
            double[] els = { 90, 60, 35, 10, 5, 0, -6, -30, -60, -90 };
            var rows = new[]
            {
                (s: 300.0, ev: 0.1, sat: 4.0, want: new[] { 40.9, 40.9, 40.6, 43.8, 46.1, 46.7, 50.1, 61.2, 62.8, 66.8 }, hot: 68.0, cool: 51.1),
                (s: 480.0, ev: 0.0, sat: 2.0, want: new[] { 39.6, 39.6, 39.3, 42.4, 44.7, 45.2, 48.5, 59.2, 60.8, 64.6 }, hot: 65.8, cool: 49.4),
                (s: 980.0, ev: -0.2, sat: 0.0, want: new[] { 36.8, 36.8, 36.4, 40.3, 41.9, 42.4, 45.5, 55.3, 56.7, 60.6 }, hot: 60.8, cool: 45.7),
                (s: 1400.0, ev: 0.0, sat: 4.0, want: new[] { 39.8, 39.8, 39.6, 41.8, 44.5, 45.2, 48.5, 59.5, 61.0, 64.8 }, hot: 66.2, cool: 49.7),
            };
            foreach (var row in rows)
            {
                var st = new NocAtmoState();
                NocFogMath.StateAt(st, row.s, 0.0);
                st.CamX = 575; st.CamZ = -1500;
                for (int i = 0; i < els.Length; i++)
                {
                    double y = SkyLuma(st, els[i], 0, row.ev, row.sat);
                    Assert.AreEqual(row.want[i], y, 3.0, FormattableString.Invariant($"s {row.s} e {els[i]}"));
                }
                st.CamX = 115; st.CamZ = 195;                              // under the boulevard axis (east leg): the hot lid
                Assert.AreEqual(row.hot, SkyLuma(st, 90, 0, row.ev, row.sat), 3.0, "hot core s " + row.s);
                st.CamX = NocturneConstants.SpireX; st.CamZ = NocturneConstants.SpireZ;   // over the spire: the cool deck
                Assert.AreEqual(row.cool, SkyLuma(st, 90, 0, row.ev, row.sat), 3.0, "cool core s " + row.s);
            }
        }

        [Test]
        public void Deck_TintModel_HotLidCoolHeartNeutralBase()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 300, 0.0);
            // hot lid over the east leg (both legs: W 0.987) toward CloudHot
            double W = NocFogMath.CloudGlow(115, 195, out C3 tint, out double gulf);
            Assert.Greater(W, 0.95); Assert.AreEqual(0.0, gulf, 1e-12);
            // the tint is CloudHot within 0.5 % (the sign-wall glow 759 m away adds a g of 0.0016 of CloudCool)
            var hot = NocFogMath.Pal(NocPal.CloudHot);
            Assert.AreEqual(hot.r, tint.r, 0.005 * hot.r); Assert.AreEqual(hot.g, tint.g, 0.005 * hot.g); Assert.AreEqual(hot.b, tint.b, 0.005 * hot.b);
            // the Old Quarter (x 1 100-1 200, z 340-440) is cool: no warm glow there (beat 7's ceiling stays cool)
            for (double x = 1100; x <= 1200; x += 25)
                for (double z = 340; z <= 440; z += 25)
                {
                    NocFogMath.CloudGlow(x, z, out C3 t, out _);
                    Assert.Less(t.r - t.b, 0.0, FormattableString.Invariant($"warm deck tint over the Old Quarter at ({x}, {z})"));
                }
            // base deck far from every glow = CloudBase at the noise mean
            var c = NocFogMath.CloudColor(st, 575, -2500, 1e6);
            var cb = NocFogMath.Pal(NocPal.CloudBase);
            Assert.AreEqual(cb.g, c.g, 2e-4, "base deck");
            // the optional Gulf shadow multiplies the base by 1 - 0.35 g (never a negative glow)
            try
            {
                NocCloudGlow.SetGulfShadow(5);
                var cs = NocFogMath.CloudColor(st, 515, -600, 1e6);
                NocFogMath.CloudGlow(515, -600, out _, out double g2);
                Assert.Greater(g2, 0.99);
                Assert.AreEqual(cb.g * (1 - NocturneConstants.CloudGulfShade * g2), cs.g, 3e-4);
            }
            finally { NocCloudGlow.Defaults(); }
        }

        [Test]
        public void Deck_FourOctaveNoise_MeanStdAndFade()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 300, 17.0);
            var rnd = new Random(3);
            double sum = 0, sum2 = 0; int n = 20000;
            for (int i = 0; i < n; i++)
            {
                double v = NocFogMath.CloudNoise(st, -20000 + 40000 * rnd.NextDouble(), -20000 + 40000 * rnd.NextDouble(), 0.5);
                Assert.That(v, Is.InRange(0.0, 1.0));
                sum += v; sum2 += v * v;
            }
            double mean = sum / n, sd = Math.Sqrt(sum2 / n - mean * mean);
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] deck noise: mean {mean:F4}, std {sd:F4} (LOOK_V2 3.1: 0.5, 0.111)"));
            Assert.AreEqual(0.5, mean, 0.01);
            Assert.AreEqual(0.111, sd, 0.02);
            // fade: an octave keeps its amplitude above 8 px of wavelength and is gone below 4 px
            Assert.AreEqual(1.0, NocFogMath.OctaveFade(40, 5.0), 1e-12);
            Assert.AreEqual(0.0, NocFogMath.OctaveFade(40, 10.0), 1e-12);
            Assert.AreEqual(0.5, NocFogMath.CloudNoise(st, 123, 456, 1e6), 1e-12, "every octave faded: the mean");
        }

        [Test]
        public void HazeLowShare_AlleyEyeAndZenith()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 50, 0.0);
            var hl = NocFogMath.Pal(NocPal.HazeLow);
            // share = 0.6 sa m / (sr + sa m): 0.547 at eye level (m 1), 0.20 toward the zenith (LOOK_V2 3.1)
            var g0 = NocFogMath.SkyGradientC(0, st);
            var c0 = NocFogMath.HazeLowMix(st, g0, 0.0);
            Assert.AreEqual(0.547, (c0.g - g0.g) / (hl.g - g0.g), 0.005, "eye level");
            var g9 = NocFogMath.SkyGradientC(90, st);
            var c9 = NocFogMath.HazeLowMix(st, g9, 1.0);
            Assert.AreEqual(0.201, (c9.g - g9.g) / (hl.g - g9.g), 0.005, "zenith");
            NocFogMath.StateAt(st, 124, 0.0);
            var c124 = NocFogMath.HazeLowMix(st, g0, 0.0);
            Assert.AreEqual(g0.g, c124.g, 1e-12, "gone with the layer at s 124");
        }

        [Test]
        public void Halos_LightInTheAir_LookV2_6()
        {
            // sigma 0.5 m for lights of range <= 10 m, 0.8 m from 12 m
            Assert.AreEqual(0.5, NocHalos.SigmaFor(7.0), 1e-12); Assert.AreEqual(0.5, NocHalos.SigmaFor(10.0), 1e-12);
            Assert.AreEqual(0.8, NocHalos.SigmaFor(12.0), 1e-12); Assert.AreEqual(0.8, NocHalos.SigmaFor(14.0), 1e-12);
            // peak = 5 % of the head radiance x min(2, sigma / 4.4e-4) x (0.6 + 0.4 rho): beat 2 (rho 0.8) 0.92, the alley haze x 2
            Assert.AreEqual(0.05 * 1.6 * 0.92, NocHalos.Peak(1.6, 4.4e-4, 0.8), 1e-12);
            Assert.AreEqual(0.05 * 1.6 * 2.0 * 0.82, NocHalos.Peak(1.6, 5.0e-3, 0.55), 1e-12, "alley: capped at 2");
            // a LampWarm x 1.6 halo (peak 0.074 linear) over the beat-2 sky at e 20 (41 codes) adds about 39 codes at its centre and
            // fades to nothing at 3 sigma: a glow of lit rain, not a disc (the head itself clips into the bloom)
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 250, 0.0);
            st.CamX = 65; st.CamZ = 195;
            double sky = SkyLuma(st, 20, 90, 0.1, 4.0);
            UnityEngine.Color lw = NocturnePalette.Linear(NocPal.LampWarm);
            double k = NocHalos.Peak(1.6, NocSchedule.FogSigma(250), NocSchedule.RainRho(250));
            double e = 20 * Math.PI / 180, a = 90 * Math.PI / 180;
            var c = NocFogMath.Floor(NocFogMath.SkyColor(st, Math.Cos(e) * Math.Sin(a), Math.Sin(e), Math.Cos(e) * Math.Cos(a), 10.0));
            NocGradeMath.Display(c.r + k * lw.r, c.g + k * lw.g, c.b + k * lw.b, 0.1, 4.0, 0.0, 1.0, out double r, out double g, out double b);
            double lit = 255.0 * NocMath.Luma709(NocturnePalette.LinearToSrgb(r), NocturnePalette.LinearToSrgb(g), NocturnePalette.LinearToSrgb(b));
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] halo over the beat-2 sky: {sky:F1} -> {lit:F1} codes at its centre"));
            Assert.That(lit - sky, Is.InRange(25.0, 50.0));
            // the queue: builders queue heads without a library; Flush without one drops them (EditMode) and builds nothing
            NocHalos.Clear();
            NocHalos.Queue("test", new[] { new UnityEngine.Vector3(0, 5, 0), new UnityEngine.Vector3(10, 5, 0) }, lw, 14.0);
            NocHalos.Queue("test", new UnityEngine.Vector3(20, 5, 0), lw, 14.0);
            Assert.AreEqual(3, NocHalos.PendingHeads);
            Assert.AreEqual(0, NocHalos.Flush(null));
            Assert.AreEqual(0, NocHalos.PendingHeads);
            // the shader carries kind 3 with the constants of the C# twin
            string src = System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath, "_Game/Shaders/NocStreak.shader"));
            StringAssert.Contains("float rhoEff = min(2.0, (_NocFog.x + _NocFog.y) / NC_HaloSigmaRef) * (0.6 + 0.4 * _NocFog.w);", src);
            StringAssert.Contains("alpha = NC_HaloPeak * rhoEff;", src);
            StringAssert.Contains("smoothstep(NC_HaloFadePx0, NC_HaloFadePx1,", src);
        }

        [Test]
        public void SheetElevationsMatchH3()
        {
            var st = Clean(Sig, true);
            Assert.AreEqual(-5.7, st.SheetE[0], 0.1); Assert.AreEqual(-15.6, st.SheetE[1], 0.15); Assert.AreEqual(-31.0, st.SheetE[2], 0.1);
        }

        // ---------------------------------------------------------------- colour science helpers
        static double DeltaE76(C3 a, C3 b)
        {
            Lab(a, out double l1, out double a1, out double b1);
            Lab(b, out double l2, out double a2, out double b2);
            return Math.Sqrt((l1 - l2) * (l1 - l2) + (a1 - a2) * (a1 - a2) + (b1 - b2) * (b1 - b2));
        }

        static void Lab(C3 c, out double L, out double A, out double B)
        {
            double x = 0.4124 * c.r + 0.3576 * c.g + 0.1805 * c.b, y = 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b, z = 0.0193 * c.r + 0.1192 * c.g + 0.9505 * c.b;
            double fx = Fl(x / 0.95047), fy = Fl(y), fz = Fl(z / 1.08883);
            L = 116 * fy - 16; A = 500 * (fx - fy); B = 200 * (fy - fz);
        }

        static double Fl(double t) => t > 216.0 / 24389.0 ? Math.Pow(t, 1.0 / 3.0) : (24389.0 / 27.0 * t + 16) / 116.0;
    }
}
