// AtmosphereTests: EditMode tests of the atmosphere maths (MASTER_PROMPT 14.2.2) and the schedule rows (14.2.3-14.2.5,
// 14.6 Rule P), on the C# twin BwAtmosphereMath (Runtime/Core/BwCurvature.cs) that mirrors BulwarkAtmosphere.hlsl.
//   * 14.2.2 checks: lowland transmittance from the crest eye at 10 / 20 / 50 / 100 km (s0 1.3e-4 and 4.5e-5), the storm
//     slab horizontally (0.52 at 1 km, 0.21 at 2.4 km, 0.0008 at 11 km); the closed form equals the spec text form and
//     its |sin g| < 1e-3 branch; the 8-segment far ray converges to the exact integral.
//   * 14.2.3: the sun path per beat (s 604: az 107.2 el 16.6, bow top 25.4), key colours x intensities, u_sun,
//     interior ambient = lamp colour x 0.06; 14.2.4: sigma0 / sigma_s / sky states; 14.2.5: vFOV, EV (the bright-to-
//     dark rule -0.81 / -0.52 / +0.05 / +0.64), bloom, saturation, contrast, WB, vignette, dither, the lamp boost, slews.
//   * 14.6: the continuity sweep s 0-1500 in 0.1 m steps with the per-step limits.
//   * The HLSL keeps its public API and its constants equal to BulwarkConst.
//   * Wave A review fixes: the stop curve meets DuskRose / DuskViolet at 12 / 35 deg, the low-sun in-scatter below the
//     visible horizon (dusk lowland 15-40), far-field motion offsets continuous across the 3600 s clock wrap, the
//     region-based ambient boxes, the key kept until the look-back to the crest is blocked (s 960).
using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class AtmosphereTests
    {
        const double CrestH = 6001.7, LedgeH = 5911.7;

        // ============================================================================ 14.2.2 optical depth
        [Test]
        public void Haze_LowlandTransmittanceFromTheCrestMatchesTheChecks()
        {
            double[] d = { 10e3, 20e3, 50e3 };
            double[] a = { 0.62, 0.42, 0.12 }, b = { 0.85, 0.74, 0.48 };
            for (int i = 0; i < d.Length; i++)
            {
                double ta = BwAtmosphereMath.LowlandTransmittance(1.3e-4, CrestH, d[i]);
                double tb = BwAtmosphereMath.LowlandTransmittance(4.5e-5, CrestH, d[i]);
                Assert.AreEqual(a[i], ta, 0.005, "s0 1.3e-4 at " + d[i] / 1000 + " km");
                Assert.AreEqual(b[i], tb, 0.005, "s0 4.5e-5 at " + d[i] / 1000 + " km");
            }
        }

        [Test]
        public void Haze_At100kmTheFarRayEqualsTheExactIntegral_TheSpecValuesAreTheEndAngleForm()
        {
            // 8 sphere-exact segments vs a 20000-step numeric integral of the same straight ray
            double fa = BwAtmosphereMath.LowlandTransmittance(1.3e-4, CrestH, 100e3), ea = BwAtmosphereMath.LowlandTransmittanceExact(1.3e-4, CrestH, 100e3);
            double fb = BwAtmosphereMath.LowlandTransmittance(4.5e-5, CrestH, 100e3), eb = BwAtmosphereMath.LowlandTransmittanceExact(4.5e-5, CrestH, 100e3);
            Assert.AreEqual(ea, fa, 0.03 * ea, "s0 1.3e-4 at 100 km: far ray vs exact");
            Assert.AreEqual(eb, fb, 0.01 * eb, "s0 4.5e-5 at 100 km: far ray vs exact");
            Assert.AreEqual(0.0126, ea, 0.0005);
            Assert.AreEqual(0.220, eb, 0.002);
            // the 14.2.2 numbers 0.009 / 0.19 at 100 km come from the closed form with the ray's angle to the LOCAL
            // horizontal at the ground end (DEVELOPMENT.md): reproduce them to show the arithmetic
            Assert.AreEqual(0.009, EndAngleForm(1.3e-4, 100e3), 0.0007);
            Assert.AreEqual(0.19, EndAngleForm(4.5e-5, 100e3), 0.005);
            Assert.AreEqual(0.12, EndAngleForm(1.3e-4, 50e3), 0.005);
            Assert.AreEqual(0.62, EndAngleForm(1.3e-4, 10e3), 0.005);
        }

        static double EndAngleForm(double s0, double D)
        {
            BwAtmosphereMath.GroundRay(CrestH, D, out D3 dir, out double t);
            double gEye = Math.Asin(-dir.y);
            double gEnd = gEye - D / BwCurvature.EarthR;           // the ray's depression below the local horizontal at the ground
            return Math.Exp(-s0 * BulwarkConst.HazeScaleHeight / Math.Sin(gEnd) * (1.0 - Math.Exp(-CrestH / BulwarkConst.HazeScaleHeight)));
        }

        [Test]
        public void Haze_SegmentsConvergeToTheExactIntegral()
        {
            foreach (double D in new[] { 10e3, 50e3, 150e3, 270e3 })
            {
                double ex = BwAtmosphereMath.LowlandTransmittanceExact(4.5e-5, LedgeH, D, 40000);
                double s6 = BwAtmosphereMath.LowlandTransmittance(4.5e-5, LedgeH, D);
                Assert.AreEqual(ex, s6, 0.03 * ex + 3e-4, "D " + D);
            }
        }

        [Test]
        public void Haze_ClosedFormEqualsTheSpecTextFormAndItsFlatBranch()
        {
            var g = new AtmoGlobals { S0 = 1.3e-4, H = 2000.0 };
            foreach (var c in new[] { (6001.7, 0.0, 11667.0), (6001.7, 5992.0, 11000.0), (6001.7, 5000.0, 3000.0), (5911.7, 2200.0, 40000.0) })
            {
                double a = BwAtmosphereMath.HazeTau(in g, c.Item1, c.Item2, c.Item3);
                double b = BwAtmosphereMath.HazeTauSpec(1.3e-4, 2000.0, c.Item1, c.Item2, c.Item3);
                Assert.AreEqual(b, a, 1e-9 * Math.Max(1.0, b) + 1e-6 * b, "closed form vs text form");
            }
            // |sin g| < 1e-3: s0 exp(-h/H) d, and continuous with the general form across the branch
            double flat = BwAtmosphereMath.HazeTau(in g, 6001.7, 6001.7, 5000.0);
            Assert.AreEqual(1.3e-4 * Math.Exp(-6001.7 / 2000.0) * 5000.0, flat, 1e-12);
            double lo = BwAtmosphereMath.HazeTau(in g, 6001.7, 6001.7 - 0.0019 * 1000.0, 1000.0);
            double hi = BwAtmosphereMath.HazeTau(in g, 6001.7, 6001.7 - 0.0021 * 1000.0, 1000.0);
            Assert.Less(Math.Abs(hi - lo), 1e-6, "no step at the series switch");
        }

        [Test]
        public void StormSlab_HorizontalTransmittanceMatchesTheChecks()
        {
            Assert.AreEqual(0.52, BwAtmosphereMath.StormHorizontalTransmittance(6.5e-4, CrestH, 1000.0), 0.005);
            Assert.AreEqual(0.21, BwAtmosphereMath.StormHorizontalTransmittance(6.5e-4, CrestH, 2400.0), 0.005);
            Assert.AreEqual(0.0008, BwAtmosphereMath.StormHorizontalTransmittance(6.5e-4, CrestH, 11000.0), 0.0001);
            // the slab edges are 150 m wide and continuous: density 0 below 5525, 1 inside 5675-6325, 0 above 6475
            Assert.AreEqual(0.0, BwAtmosphereMath.SlabDensity(5500, 5600, 6400, 150), 1e-12);
            Assert.AreEqual(0.5, BwAtmosphereMath.SlabDensity(5600, 5600, 6400, 150), 1e-12);
            Assert.AreEqual(1.0, BwAtmosphereMath.SlabDensity(6000, 5600, 6400, 150), 1e-12);
            Assert.AreEqual(0.5, BwAtmosphereMath.SlabDensity(6400, 5600, 6400, 150), 1e-12);
            // a vertical crossing of the whole slab integrates to sigma_s x 800 m
            var g = new AtmoGlobals { SigmaS = 6.5e-4, SlabLo = 5600, SlabHi = 6400, SlabEdge = 150 };
            Assert.AreEqual(6.5e-4 * 800.0, BwAtmosphereMath.StormTau(in g, 7000.0, 5000.0, 2000.0), 1e-9);
        }

        // ============================================================================ sky and fog colour
        [Test]
        public void Sky_FogAtTheHorizonEqualsTheVisibleSkyThere()
        {
            foreach (double s in new[] { 230.0, 604.0, 810.0, 1452.0 })
            {
                var g = Globals(s, s >= 1370 ? -88.3 : 1.7, s >= 1370 ? 1449.0 : s);
                double dip = BwCurvature.HorizonDipD(g.EyeH) / BwAtmosphereMath.Deg;
                for (int az = 0; az < 360; az += 30)
                {
                    D3 above = BwAtmosphereMath.Dir(az, -dip + 0.03), below = BwAtmosphereMath.Dir(az, -dip - 0.03);
                    D3 sky = BwAtmosphereMath.SkyVisible(in g, above);
                    double t = BwAtmosphereMath.RaySphereNear(BwCurvature.EarthR + g.EyeH, g.Sin2Lowland, below.y);
                    Assert.Greater(t, 0.0);
                    D3 ground = BwAtmosphereMath.FogApplyFar(in g, new D3(0.02, 0.02, 0.02), below, t);
                    double dE = FarFieldTests.DeltaE(sky, ground, BulwarkSchedule.Ev(s));
                    Assert.Less(dE, 2.0, "s " + s + " az " + az + ": horizon dE (Part 10 #13)");
                }
            }
        }

        [Test]
        public void Sky_StatesUseTheirPaletteStops()
        {
            var g = Globals(300.0, 1.7, 300.0);
            AssertColour(BulwarkPalette.Linear(Pal.StormZenith) * BulwarkSchedule.StormSkyGain, BwAtmosphereMath.SkyAt(in g, 200.0, 90.0), 0.01, "storm zenith (token x the storm sky gain)");
            var gn = Globals(810.0, 1.7, 810.0);
            AssertColour(BulwarkPalette.Linear(Pal.NoonZenith), BwAtmosphereMath.SkyAt(in gn, 330.0, 90.0), 0.04, "noon zenith (aureole of the 50 deg sun)");
            var gd = Globals(1452.0, -88.3, 1449.0);
            AssertColour(BulwarkPalette.Linear(Pal.DuskDeep) * BulwarkSchedule.DuskSkyGain, BwAtmosphereMath.SkyAt(in gd, 125.0, 90.0), 0.01, "dusk zenith (token x the dusk sky gain)");
            // dusk: amber toward the sun azimuth 305, rose and violet higher, the anti-sun horizon not amber
            D3 toward = BwAtmosphereMath.SkyAt(in gd, 305.0, 0.5), away = BwAtmosphereMath.SkyAt(in gd, 125.0, 0.5);
            Assert.Greater(toward.x / Math.Max(toward.z, 1e-6), 3.0, "amber toward the sun");
            Assert.Less(away.x / Math.Max(away.z, 1e-6), 3.0, "rose-violet away from the sun");
            D3 rose = BwAtmosphereMath.SkyAt(in gd, 125.0, 12.0);
            Assert.Greater(rose.x, rose.y, "12 deg is rose");
            // the stop curve meets every 14.2.1 stop exactly at its elevation (before the glow and the aureole)
            AssertColour(BulwarkPalette.Linear(Pal.DuskRose) * BulwarkSchedule.DuskSkyGain, BwAtmosphereMath.SkyStops(in gd, 12.0 * BwMath.Deg), 0.01, "dusk 12 deg = DuskRose (x gain)");
            AssertColour(BulwarkPalette.Linear(Pal.DuskViolet) * BulwarkSchedule.DuskSkyGain, BwAtmosphereMath.SkyStops(in gd, 35.0 * BwMath.Deg), 0.01, "dusk 35 deg = DuskViolet (x gain)");
            AssertColour(BulwarkPalette.Linear(Pal.DuskDeep) * BulwarkSchedule.DuskSkyGain, BwAtmosphereMath.SkyStops(in gd, 90.0 * BwMath.Deg), 0.01, "dusk zenith = DuskDeep (x gain)");
            Color zn = BulwarkPalette.Linear(Pal.NoonZenith), hn = BulwarkPalette.Linear(Pal.NoonHorizon);
            AssertColour(Color.Lerp(zn, hn, (float)BulwarkConst.SkyMidLerp), BwAtmosphereMath.SkyStops(in gn, 12.0 * BwMath.Deg), 0.01, "noon 12 deg = mid colour");
            // the whole sky at 35 deg away from the sun is violet (blue over red; KS20 / look_l "through rose to violet")
            D3 violet = BwAtmosphereMath.SkyAt(in gd, 125.0, 35.0);
            Assert.Greater(violet.z, violet.x, "35 deg away from the sun is violet");
            // monotone between the stops: no overshoot beyond the neighbouring stops in any channel
            D3 h = gd.Horizon, m = gd.Mid, u = gd.Upper, z = gd.Zenith;
            for (double e = 0.0; e <= 90.0; e += 0.25)
            {
                D3 c = BwAtmosphereMath.SkyStops(in gd, e * BwMath.Deg);
                D3 lo = e < 12 ? Min(h, m) : (e < 35 ? Min(m, u) : Min(u, z)), hi = e < 12 ? Max(h, m) : (e < 35 ? Max(m, u) : Max(u, z));
                Assert.GreaterOrEqual(c.x, lo.x - 1e-9, "monotone r at " + e); Assert.LessOrEqual(c.x, hi.x + 1e-9, "monotone r at " + e);
                Assert.GreaterOrEqual(c.z, lo.z - 1e-9, "monotone b at " + e); Assert.LessOrEqual(c.z, hi.z + 1e-9, "monotone b at " + e);
            }
            // storm, noon and dusk skies are never orange-brown away from the sun (KS20 check is the dusk table)
            D3 stormHor = BwAtmosphereMath.SkyAt(in g, 285.0, 1.0);
            Assert.Less(stormHor.x, stormHor.z, "storm horizon stays blue-grey");
        }

        static D3 Min(D3 a, D3 b) => new D3(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
        static D3 Max(D3 a, D3 b) => new D3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));

        [Test]
        public void Dusk_LowSunInScatterBelowTheVisibleHorizon()
        {
            // the low-sun fade is on at dusk only (sun below 1 deg; off above 6 deg)
            Assert.AreEqual(1.0, BulwarkSchedule.Evaluate(1452.0).FogLowWeight, 1e-6, "dusk");
            Assert.AreEqual(0.0, BulwarkSchedule.Evaluate(300.0).FogLowWeight, 1e-6, "storm, sun 6 deg");
            Assert.AreEqual(0.0, BulwarkSchedule.Evaluate(810.0).FogLowWeight, 1e-6, "noon");
            var g = Globals(1452.0, -88.3, 1449.0);
            double dip = BwAtmosphereMath.VisibleDip(in g) / BwMath.Deg;
            // continuous at the visible horizon, the shadowed-haze colour 5 deg below it
            D3 atH = BwAtmosphereMath.FogColorRaw(in g, BwAtmosphereMath.Dir(305.0, -dip + 1e-4));
            D3 justBelow = BwAtmosphereMath.FogColorRaw(in g, BwAtmosphereMath.Dir(305.0, -dip - 0.01));
            Assert.Less((atH - justBelow).Length, 0.02 * Math.Max(BwAtmosphereMath.Luma(atH), 0.05), "no step at the visible horizon");
            D3 deep = BwAtmosphereMath.FogColorRaw(in g, BwAtmosphereMath.Dir(305.0, -dip - 5.0));
            Assert.Less(BwAtmosphereMath.Luma(deep), BwAtmosphereMath.Luma(g.FogShadow) + 0.03, "5 deg below: shadowed haze");
            // the lowland 15 deg down toward the sun side: ground + haze darker than sRGB 45 after EV +0.3 (14.10.3 lowland 15-40)
            D3 dir = BwAtmosphereMath.Dir(300.0, -15.0);
            double t = BwAtmosphereMath.RaySphereNear(BwCurvature.EarthR + g.EyeH, g.Sin2Lowland, dir.y);
            D3 ground = g.AmbExtSky * 0.09;                       // mean field albedo (BwFarField FFFieldMean) x sky light
            D3 seen = BwAtmosphereMath.FogApplyFar(in g, ground, dir, t);
            double y = BwAtmosphereMath.Luma(seen) * Math.Pow(2.0, BulwarkSchedule.Ev(1452.0));
            Assert.Less(Megastructure.Rendering.BwGradeMath.LinearToSrgb(y) * 255.0, 45.0, "lowland at -15 deg (display)");
            Assert.Greater(Megastructure.Rendering.BwGradeMath.LinearToSrgb(y) * 255.0, 15.0, "lowland at -15 deg (display)");
        }

        [Test]
        public void Flow_OffsetsAreContinuousAcrossTheClockWrap()
        {
            // the far field moves with offsets from the unwrapped double clock: no jump where _BwTime wraps (3600 s)
            double dt = 1.0 / 60.0;
            foreach (double t in new[] { 3600.0, 7200.0, 36000.0 })
            {
                Vector4 a = BulwarkSchedule.FlowOffsets(t - dt), b = BulwarkSchedule.FlowOffsets(t);
                // tolerance: two float32 ulps of the offset (0.06 m of 3200 m noise cells after 10 h of storm wind)
                Assert.AreEqual(BulwarkSchedule.FlowStormWind * dt, b.x - a.x, Ulp2(b.x), "storm wind step at t " + t);
                Assert.AreEqual(BulwarkSchedule.FlowFairWind * dt, b.y - a.y, Ulp2(b.y), "fair wind step at t " + t);
                Assert.AreEqual(BulwarkSchedule.FlowMist * dt, b.z - a.z, Ulp2(b.z), "mist step at t " + t);
                Assert.AreEqual(BulwarkSchedule.FlowJet * dt, b.w - a.w, Ulp2(b.w), "jet step at t " + t);
            }
            // a pure function of the clock (deterministic captures), independent of the sky state
            Assert.AreEqual(BulwarkSchedule.FlowOffsets(1234.5).x, BulwarkSchedule.FlowOffsets(1234.5).x);
            string path = Path.Combine(Application.dataPath, "_Game/Shaders/BwFarField.shader");
            string src = Regex.Replace(File.ReadAllText(path), @"//[^\n]*", "");
            StringAssert.DoesNotContain("_BwTime", src, "the far field never reads the wrapped clock");
            StringAssert.Contains("_BwFlow", src);
        }

        static double Ulp2(float v) => Math.Max(1e-3, 2.0 * Math.Abs(v) * 1.1920929e-7 * 2.0);

        [Test]
        public void Ambient_RegionBoxesSeparateInteriorFromExterior()
        {
            var g = Globals(300.0, 1.7, 300.0);
            Assert.AreEqual(1.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(-1.5, 1.7, 60.0)), 1e-9, "beat 1 gallery");
            Assert.AreEqual(0.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(-1.5, 1.7, 122.0)), 1e-9, "crest beyond the door");
            Assert.AreEqual(1.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(-1.5, 1.7, 926.0)), 1e-9, "atrium");
            Assert.AreEqual(0.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(-1.5, 1.7, 918.0)), 1e-9, "crest before the portal");
            Assert.AreEqual(0.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(2.0, 0.0, 970.0)), 1e-9, "road beyond the north arch");
            Assert.AreEqual(1.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(-1.5, -12.0, 975.0)), 1e-9, "stairs");
            Assert.AreEqual(1.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(3.7, -52.3, 1050.0)), 1e-9, "exit hall");
            Assert.AreEqual(1.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(6.0, -70.0, 1200.0)), 1e-9, "window gallery");
            Assert.AreEqual(1.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(0.0, -88.3, 1400.0)), 1e-9, "sluice tunnel");
            Assert.AreEqual(0.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(-9.2, -88.3, 1452.0)), 1e-9, "lee ledge");
            Assert.AreEqual(0.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(-6.4, -90.0, 1421.0)), 1e-9, "ledge lane edge at the face");
            Assert.AreEqual(0.0, BwAtmosphereMath.InteriorWeightAt(in g, new D3(8.0, -8.0, 1000.0)), 1e-9, "sea surface at the face");
            // the region ambient: exterior trilight outside, lamp colour x 0.06 inside (14.2.3)
            var gi = Globals(1000.0, -20.0, 990.0);
            D3 inside = BwAtmosphereMath.AmbientAt(in gi, new D3(0, 1, 0), new D3(-1.5, -20.0, 990.0));
            AssertColour(BulwarkPalette.Linear(Pal.Tungsten) * 0.06f, inside, 1e-5, "interior ambient (tungsten x 0.06)");
            D3 outside = BwAtmosphereMath.AmbientAt(in gi, new D3(0, 1, 0), new D3(-1.5, 1.7, 900.0));
            Assert.AreEqual(gi.AmbExtSky.x, outside.x, 1e-9, "the crest keeps the exterior ambient while the player is inside");
        }

        [Test]
        public void Sky_IsContinuousInElevationAndAzimuth()
        {
            foreach (double s in new[] { 300.0, 604.0, 810.0, 1452.0 })
            {
                var g = Globals(s, 1.7, Math.Min(s, 1449.0));
                for (int az = 0; az < 360; az += 15)
                {
                    D3 prev = BwAtmosphereMath.SkyAt(in g, az, -5.0);
                    for (double el = -4.9; el <= 89.9; el += 0.1)
                    {
                        D3 c = BwAtmosphereMath.SkyAt(in g, az, el);
                        double step = Math.Abs(BwAtmosphereMath.Luma(c) - BwAtmosphereMath.Luma(prev));
                        Assert.Less(step, 0.035 * Math.Max(BwAtmosphereMath.Luma(c), 0.02), "s " + s + " az " + az + " el " + el);
                        prev = c;
                    }
                }
            }
        }

        // ============================================================================ 14.2.3 sun and key
        [Test]
        public void Sun_PathPerBeat()
        {
            AssertSun(300.0, 100.0, 6.0, 1e-9);
            AssertSun(604.0, 107.2, 16.6, 1e-9);
            AssertSun(640.0, 109.0, 17.5, 1e-9);
            AssertSun(760.0, 150.0, 48.0, 1e-9);
            AssertSun(920.0, 176.0, 56.0, 1e-9);
            AssertSun(1200.0, 215.0 + 0.255 * 130.0, 48.0 - 28.0 * 130.0 / 300.0, 1e-9);
            AssertSun(1420.0, 305.0, -0.9, 1e-9);
            AssertSun(1452.0, 305.0, -1.2809, 1e-3);
            AssertSun(1500.0, 305.0, -2.1, 1e-9);
            // beat 3 time-lapse to beat 4: 1.0 deg/s at 2.5 m/s (az 41, el 30.5 over 120 m)
            BulwarkSchedule.Sun(700.0, out double a0, out double e0);
            BulwarkSchedule.Sun(702.5, out double a1, out double e1);
            double rate = Math.Sqrt(Math.Pow((a1 - a0) * Math.Cos(e0 * BwMath.Deg), 2) + Math.Pow(e1 - e0, 2));
            Assert.AreEqual(1.0, rate, 0.15, "time-lapse deg/s at 2.5 m/s");
            // beat 7: 1.19 deg above the visible horizon at s 1452, descent 0.015 deg/m over s 1440-1480
            double dip = BwCurvature.HorizonDipD(BulwarkConst.AltitudeOffset - 88.3) / BwMath.Deg;
            BulwarkSchedule.Sun(1452.0, out _, out double el52);
            Assert.AreEqual(1.19, el52 + dip, 0.02, "sun above the visible horizon at s 1452");
            // descent: the smootherstep el(s) of 14.5.7 / 14.12 sinks 1.2 deg over s 1420-1500 = 0.015 deg/m on average;
            // over s 1440-1480 it is 0.0238 deg/m (peak 1.875 x 0.015 = 0.028 at s 1460): acceptance (3) of 14.5.7 is the
            // mean of the formula, corrected per 14.10.5 (DEVELOPMENT.md)
            BulwarkSchedule.Sun(1420.0, out _, out double e20);
            BulwarkSchedule.Sun(1500.0, out _, out double e00);
            Assert.AreEqual(0.015, (e20 - e00) / 80.0, 0.003, "sun descent deg/m, s 1420-1500");
            BulwarkSchedule.Sun(1440.0, out _, out double e40);
            BulwarkSchedule.Sun(1480.0, out _, out double e80);
            Assert.AreEqual(0.0238, (e40 - e80) / 40.0, 0.003, "sun descent deg/m, s 1440-1480 (smootherstep)");
            // the primary bow top is 42 - 16.6 = 25.4 deg at s 604 (14.5.3)
            BulwarkSchedule.Sun(604.0, out _, out double el604);
            Assert.AreEqual(BulwarkConst.RainbowTopDeg, BulwarkConst.RainbowPrimaryDeg - el604, 0.5);
        }

        static void AssertSun(double s, double az, double el, double tol)
        {
            BulwarkSchedule.Sun(s, out double a, out double e);
            Assert.AreEqual(az, a, tol, "sun az at s " + s);
            Assert.AreEqual(el, e, tol, "sun el at s " + s);
            var p = BulwarkSchedule.Evaluate(s);
            Vector3 d = BulwarkSchedule.Dir(az, el);
            Assert.Less((p.SunDir - d).magnitude, 1e-4f, "SunDir at s " + s);
        }

        [Test]
        public void Key_ColoursAndIntensitiesPerBeat()
        {
            AssertKey(300.0, Pal.StormKey, BulwarkConst.KeyB2);
            AssertKey(600.0, Pal.BreakGold, BulwarkConst.KeyB3Peak);
            Assert.AreEqual(BulwarkConst.KeyB3End, KeyB3Only(700.0), 1e-6, "beat 3 key ends at 2.6");
            AssertKey(810.0, Pal.NoonKey, BulwarkConst.KeyB4);
            // the key stays while a look-back can still see the crest (s < 960, 14.6) and is out by s 1004 (stairs)
            Assert.AreEqual(BulwarkConst.KeyB4, BulwarkSchedule.Evaluate(958.0).SunIntensity, 1e-6, "look-back at s 958: full key");
            Assert.AreEqual(0.0, BulwarkSchedule.Evaluate(1004.0).SunIntensity, 1e-9, "beat 5 stairs: no sun");
            Assert.AreEqual(0.0, BulwarkSchedule.Evaluate(1200.0).SunIntensity, 1e-9, "beat 6: the veil model, no key");
            AssertKey(1452.0, Pal.DuskKey, BulwarkConst.KeyB7);
            Assert.IsFalse(BulwarkSchedule.Evaluate(1452.0).SunShadows, "beat 7: no shadows");
            Assert.IsTrue(BulwarkSchedule.Evaluate(810.0).SunShadows);
            // the beat 4 key rises with smootherstep((s-690)/50)
            double k715 = BulwarkSchedule.Key(715.0);
            Assert.AreEqual(BwMath.Lerp(KeyB3Only(715.0), BulwarkConst.KeyB4, 0.5), k715, 1e-9);
            // u_sun
            Assert.AreEqual(0.0, BulwarkSchedule.USun(520.0), 1e-12);
            Assert.AreEqual(1.0, BulwarkSchedule.USun(600.0), 1e-12);
            Assert.AreEqual(0.65, BulwarkSchedule.USun(700.0), 1e-12);
        }

        static double KeyB3Only(double s) => (0.35 + 3.05 * BwMath.Smootherstep01((s - 520.0) / 55.0)) * (1.0 - BulwarkSchedule.KeyB3Dim * BwMath.Smootherstep01((s - 640.0) / 60.0));

        static void AssertKey(double s, Pal colour, double intensity)
        {
            var p = BulwarkSchedule.Evaluate(s);
            Assert.AreEqual(intensity * BulwarkSchedule.LightCalKey, p.SunIntensity, 1e-5, "key intensity at s " + s);
            AssertColour(BulwarkPalette.Linear(colour), new D3(p.SunColor.r, p.SunColor.g, p.SunColor.b), 1e-4, "key colour at s " + s);
        }

        [Test]
        public void Ambient_InteriorIsLampColourTimes006()
        {
            AssertAmb(50.0, BulwarkPalette.Linear(Pal.SodiumWarm), 0.06);
            AssertAmb(1000.0, BulwarkPalette.Linear(Pal.Tungsten), 0.06);
            AssertAmb(1400.0, BulwarkPalette.Linear(Pal.SodiumWarm), 0.06);
            Color teal = BulwarkPalette.Linear(Pal.SeaTeal);
            float m = Mathf.Max(teal.r, Mathf.Max(teal.g, teal.b));
            Color spill = new Color(teal.r / m, teal.g / m, teal.b / m);
            float y = 0.2126f * spill.r + 0.7152f * spill.g + 0.0722f * spill.b;
            Color hall = spill * 0.7f + new Color(y, y, y) * 0.3f;   // X2 S2-5: 0.7 cyan spill + 0.3 neutral at the same luminance
            Assert.GreaterOrEqual(hall.r, 0.09f * hall.g, "hall ambient R >= 0.09 G");
            AssertAmb(1200.0, hall, 0.08);
            Assert.AreEqual(0.0, BulwarkSchedule.Evaluate(300.0).Interior, 1e-9, "crest is exterior");
            Assert.AreEqual(0.0, BulwarkSchedule.Evaluate(1460.0).Interior, 1e-9, "ledge is exterior");
        }

        static void AssertAmb(double s, Color lamp, double k)
        {
            var p = BulwarkSchedule.Evaluate(s);
            Assert.AreEqual(1.0, p.Interior, 1e-9, "interior at s " + s);
            AssertColour(lamp * (float)k, new D3(p.AmbSky.r, p.AmbSky.g, p.AmbSky.b), 1e-5, "interior ambient at s " + s);
        }

        // ============================================================================ 14.2.4 fog and sky states
        [Test]
        public void Fog_SigmaRowsAndSkyStates()
        {
            AssertFog(300.0, 1.3e-4, 6.5e-4);
            AssertFog(560.0, 9e-5, 6.5e-4 + (2e-4 - 6.5e-4) * BwMath.Smootherstep(520, 640, 560));
            AssertFog(640.0, 9e-5, 2.0e-4);
            AssertFog(810.0, 6e-5, 1.0e-4);
            AssertFog(1452.0, 4.5e-5, 0.0);
            AssertSky(300.0, 1, 0, 0, 0);
            AssertSky(600.0, 0, 1, 0, 0);
            AssertSky(810.0, 0, 0, 1, 0);
            AssertSky(1452.0, 0, 0, 0, 1);
            Assert.AreEqual(BulwarkConst.HazeSigma[0], BulwarkSchedule.Evaluate(50.0).DustSigma, 1e-9, "beat 1 dust 6e-3");
            Assert.AreEqual(0.0, BulwarkSchedule.Evaluate(300.0).DustSigma, 1e-12);
            // sea state
            BulwarkSchedule.SeaState(300, out double h2, out double t2);
            BulwarkSchedule.SeaState(560, out double h3, out double t3);
            BulwarkSchedule.SeaState(810, out double h4, out double t4);
            Assert.AreEqual(2.8, h2, 1e-9); Assert.AreEqual(9.0, t2, 1e-9);
            Assert.AreEqual(2.0, h3, 1e-9); Assert.AreEqual(8.0, t3, 1e-9);
            Assert.AreEqual(3.2, h4, 1e-9); Assert.AreEqual(11.0, t4, 1e-9);
            // city lights N(s) = smoothstep((s - 1425) / 50): none at 1420, full by 1475
            Assert.AreEqual(0.0, BulwarkSchedule.CityN(1420.0), 1e-12);
            Assert.AreEqual(1.0, BulwarkSchedule.CityN(1475.0), 1e-12);
        }

        static void AssertFog(double s, double s0, double ss)
        {
            var p = BulwarkSchedule.Evaluate(s);
            Assert.AreEqual(s0, p.FogSigma0, 1e-9, "sigma0 at s " + s);
            Assert.AreEqual(ss, p.StormSigma, 1e-9, "sigma_s at s " + s);
            Assert.AreEqual(BulwarkConst.HazeScaleHeight, p.FogScaleHeight, 1e-6);
        }

        static void AssertSky(double s, double st, double go, double no, double du)
        {
            BulwarkSchedule.SkyWeights(s, out double a, out double b, out double c, out double d);
            Assert.AreEqual(st, a, 1e-9, "storm at s " + s); Assert.AreEqual(go, b, 1e-9, "gold at s " + s);
            Assert.AreEqual(no, c, 1e-9, "noon at s " + s); Assert.AreEqual(du, d, 1e-9, "dusk at s " + s);
        }

        // ============================================================================ 14.2.5 post, FOV, exposure
        [Test]
        public void Post_RowsPerBeat()
        {
            double[] mid = { 60, 270, 560, 810, 995, 1220, 1460 };
            for (int b = 0; b < 7; b++)
            {
                var p = BulwarkSchedule.Evaluate(mid[b]);
                string at = "beat " + (b + 1);
                Assert.AreEqual(BulwarkConst.VFov[b], p.Vfov, 1e-4, at + " vFOV");
                Assert.AreEqual(BulwarkConst.BloomThreshold[b], p.BloomThreshold, 1e-6, at + " bloom thr");
                Assert.AreEqual(BulwarkConst.BloomIntensity[b], p.BloomIntensity, 1e-6, at + " bloom int");
                Assert.AreEqual(BulwarkConst.BloomScatter[b], p.BloomScatter, 1e-6, at + " bloom scatter");
                Assert.AreEqual(BulwarkConst.Saturation[b], p.Saturation, 1e-6, at + " sat");
                Assert.AreEqual(BulwarkConst.Contrast[b], p.Contrast, 1e-6, at + " contrast");
                Assert.AreEqual(BulwarkConst.WhiteBalance[b], p.WbTemp, 1e-6, at + " WB");
                Assert.AreEqual(BulwarkConst.Vignette[b], p.Vignette, 1e-6, at + " vignette");
                Assert.AreEqual(BulwarkConst.DitherPeak[b], p.DitherPeak, 1e-6, at + " dither");
            }
            Assert.AreEqual(0.3, BulwarkSchedule.Ev(60), 1e-9);
            Assert.AreEqual(0.0, BulwarkSchedule.Ev(270), 1e-9);
            Assert.AreEqual(-0.3, BulwarkSchedule.Ev(560), 1e-9);
            Assert.AreEqual(-0.9, BulwarkSchedule.Ev(810), 1e-9);
            Assert.AreEqual(0.7, BulwarkSchedule.Ev(1000), 1e-9);
            Assert.AreEqual(0.4, BulwarkSchedule.Ev(1085), 1e-9);
            Assert.AreEqual(1.0, BulwarkSchedule.Ev(1355), 1e-9);
            Assert.AreEqual(0.8, BulwarkSchedule.Ev(1400), 1e-9);
            Assert.AreEqual(0.3, BulwarkSchedule.Ev(1452), 1e-9);
            // 30 m crossfades centred on the boundary: half-way at the boundary
            Assert.AreEqual(0.5 * (BulwarkConst.Vignette[1] + BulwarkConst.Vignette[2]), BulwarkSchedule.Evaluate(420.0).Vignette, 1e-6);
            Assert.AreEqual(BulwarkConst.Vignette[1], BulwarkSchedule.Evaluate(405.0).Vignette, 1e-6);
        }

        [Test]
        public void Exposure_BrightToDarkRuleAtThePortal()
        {
            Assert.AreEqual(-0.81, BulwarkSchedule.Ev(920.0), 0.01);
            Assert.AreEqual(-0.52, BulwarkSchedule.Ev(926.0), 0.01);
            Assert.AreEqual(0.05, BulwarkSchedule.Ev(934.0), 0.01);
            Assert.AreEqual(0.64, BulwarkSchedule.Ev(945.0), 0.01);
            Assert.AreEqual(-0.9, BulwarkSchedule.Ev(912.0), 1e-9, "exterior exposure kept until the window");
            // lamp boost x2.5 at the plane, x1 at s 960; lit interior net +0.49 EV at 920, +1.04 at 934, +0.70 at 960
            Assert.AreEqual(Math.Pow(2.0, 1.3), BulwarkSchedule.LampBoost(920.0), 1e-9);
            Assert.AreEqual(1.0, BulwarkSchedule.LampBoost(960.0), 1e-9);
            Assert.AreEqual(0.49, BulwarkSchedule.Ev(920.0) + Math.Log(BulwarkSchedule.LampBoost(920.0), 2.0), 0.01);
            Assert.AreEqual(1.04, BulwarkSchedule.Ev(934.0) + Math.Log(BulwarkSchedule.LampBoost(934.0), 2.0), 0.02);
            Assert.AreEqual(0.70, BulwarkSchedule.Ev(960.0) + Math.Log(BulwarkSchedule.LampBoost(960.0), 2.0), 0.01);
            // peak EV slopes at 2.5 m/s: door window 0.054, tower 0.19 (1.6 EV / 40 m), tunnel mouth 0.08 EV/s
            Assert.AreEqual(0.19, MaxSlope(BulwarkSchedule.Ev, 912, 952) * 2.5, 0.01);
            Assert.AreEqual(0.078, MaxSlope(BulwarkSchedule.Ev, 1415, 1445) * 2.5, 0.005);
            Assert.AreEqual(0.054, MaxSlope(BulwarkSchedule.Ev, 104, 130) * 2.5, 0.005);
            Assert.LessOrEqual(MaxSlope(BulwarkSchedule.Vfov, 0, 1500) * 2.5, 0.63, "vFOV peak deg/s (14.2.5: 0.62)");
        }

        static double MaxSlope(Func<double, double> f, double a, double b)
        {
            double m = 0;
            for (double s = a; s < b; s += 0.05) m = Math.Max(m, Math.Abs(f(s + 0.05) - f(s)) / 0.05);
            return m;
        }

        [Test]
        public void Slews_LimitsAndBehaviour()
        {
            Assert.AreEqual(0.30, FrameworkConst.EvSlew, 1e-12);
            Assert.AreEqual(6.0, FrameworkConst.WbSlew, 1e-12);
            Assert.AreEqual(0.1, FrameworkConst.BloomSlew, 1e-12);
            Assert.AreEqual(1.0, FrameworkConst.FovSlew, 1e-12);
            // a teleport-free walk at 2.5 m/s never hits the EV slew (the schedule's peak is 0.19 EV/s)
            float ev = (float)BulwarkSchedule.Ev(0), dt = 1f / 60f;
            float maxLag = 0f;
            for (double s = 0; s <= 1500; s += 2.5 * dt)
            {
                ev = BwMath.Slew(ev, (float)BulwarkSchedule.Ev(s), (float)FrameworkConst.EvSlew, dt);
                maxLag = Mathf.Max(maxLag, Mathf.Abs(ev - (float)BulwarkSchedule.Ev(s)));
            }
            Assert.Less(maxLag, 0.01f, "EV slew never engaged at 2.5 m/s");
            // a jump is limited to 0.30 EV/s
            Assert.AreEqual(0.005f, BwMath.Slew(0f, 1f, 0.30f, dt), 1e-6f);
        }

        // ============================================================================ 14.6 continuity
        [Test]
        public void Continuity_PerStepLimitsOverTheWholePath()
        {
            BwParams prev = BulwarkSchedule.Evaluate(0.0);
            double maxS0 = 0, maxSs = 0;
            foreach (double v in BulwarkConst.HazeSigma) if (v < 1e-3) maxS0 = Math.Max(maxS0, v);
            foreach (double v in BulwarkConst.StormSigma) maxSs = Math.Max(maxSs, v);
            for (int i = 1; i <= 15000; i++)
            {
                double s = i * 0.1;
                BwParams p = BulwarkSchedule.Evaluate(s);
                string at = " at s " + s.ToString("F1");
                Assert.LessOrEqual(Math.Abs(p.Ev - prev.Ev), 0.02, "EV" + at);
                Assert.LessOrEqual(Math.Abs(p.Vfov - prev.Vfov), 0.1, "vFOV" + at);
                Assert.LessOrEqual(Math.Abs(p.BloomThreshold - prev.BloomThreshold), 0.002, "bloom thr" + at);
                Assert.LessOrEqual(Math.Abs(p.BloomIntensity - prev.BloomIntensity), 0.002, "bloom int" + at);
                Assert.LessOrEqual(Math.Abs(p.BloomScatter - prev.BloomScatter), 0.002, "bloom scatter" + at);
                Assert.LessOrEqual(Math.Abs(p.Saturation - prev.Saturation), 0.15, "sat" + at);
                Assert.LessOrEqual(Math.Abs(p.Contrast - prev.Contrast), 0.15, "contrast" + at);
                Assert.LessOrEqual(Math.Abs(p.WbTemp - prev.WbTemp), 0.15, "WB" + at);
                Assert.LessOrEqual(Math.Abs(p.Vignette - prev.Vignette), 0.003, "vignette" + at);
                Assert.LessOrEqual(Math.Abs(p.SunIntensity - prev.SunIntensity), 0.02, "key" + at);
                Assert.LessOrEqual(Math.Abs(p.FogSigma0 - prev.FogSigma0), 0.005 * Math.Max(Math.Max(p.FogSigma0, prev.FogSigma0), 0.1 * maxS0), "sigma0" + at);
                Assert.LessOrEqual(Math.Abs(p.StormSigma - prev.StormSigma), 0.005 * Math.Max(Math.Max(p.StormSigma, prev.StormSigma), 0.1 * maxSs), "sigma_s" + at);
                Assert.LessOrEqual(Math.Abs(p.DustSigma - prev.DustSigma), 0.005 * BulwarkConst.HazeSigma[0], "dust" + at);
                Assert.LessOrEqual(MaxDiff(p.AmbSky, prev.AmbSky), 0.01, "ambient sky" + at);
                prev = p;
            }
        }

        static float MaxDiff(Color a, Color b) => Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Max(Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b)));

        // ============================================================================ the HLSL contract
        [Test]
        public void Hlsl_ApiAndConstantsMatchTheTwin()
        {
            string path = Path.Combine(Application.dataPath, "_Game/Shaders/Include/BulwarkAtmosphere.hlsl");
            Assert.IsTrue(File.Exists(path), path);
            string src = File.ReadAllText(path);
            foreach (string sig in new[] { "float3 BwSkyColor(float3 dir)", "float3 BwFogApply(float3 color, float3 posWS)", "float3 BwHorizon(float azDeg)",
                                           "float BwDrop(float d)", "float BwHorizonDip(float h, float R)", "float BwFogTransmittance(float3 posWS)",
                                           "float BwSunBreak(float3 posWS)", "float3 BwAmbientAt(float3 normalWS, float3 posWS)",
                                           "float BwInteriorWeight(float3 p)", "float3 BwSeaSurface(float3 dir, float3 n)" })
                StringAssert.Contains(sig, src, "public signature kept: " + sig);
            Assert.AreEqual(BwCurvature.EarthR, Define(src, "BW_EARTH_R"), 1e-6);
            Assert.AreEqual(BulwarkConst.SeaSphereOffset, Define(src, "BW_SEA_OFFSET"), 1e-6);
            Assert.AreEqual(BulwarkConst.AltitudeOffset, Define(src, "BW_CREST_OFFSET"), 1e-6);
            Assert.AreEqual(BwAtmosphereMath.SegNear, Define(src, "BW_ATMO_SEG_NEAR"), 1e-6);
            Assert.AreEqual(BwAtmosphereMath.SegFar, Define(src, "BW_ATMO_SEG_FAR"), 1e-6);
            Assert.AreEqual(BwAtmosphereMath.Terminator, Define(src, "BW_TERMINATOR"), 1e-6);
            StringAssert.DoesNotContain("multi_compile_fog", src);
        }

        static double Define(string src, string name)
        {
            Match m = Regex.Match(src, @"#define\s+" + name + @"\s+\(?([0-9.eE+-]+)");
            Assert.IsTrue(m.Success, "define " + name);
            return double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        }

        // ============================================================================ helpers
        internal static AtmoGlobals Globals(double s, double eyeY, double eyeZ)
        {
            var p = BulwarkSchedule.Evaluate(s);
            return AtmoGlobals.From(in p, new D3(-1.5, eyeY, eyeZ));
        }

        static void AssertColour(Color expect, D3 got, double tol, string what)
        {
            Assert.AreEqual(expect.r, got.x, tol, what + " r");
            Assert.AreEqual(expect.g, got.y, tol, what + " g");
            Assert.AreEqual(expect.b, got.z, tol, what + " b");
        }
    }
}
