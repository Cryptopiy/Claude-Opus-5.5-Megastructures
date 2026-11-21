// MistTests: the numbers of the mist volume (MASTER_PROMPT 14.7 H2, 14.5.3 acceptance, 14.10.1 T-NUM bow) on the C# twin
// MistVolumeMath, the noise tile MistVolumeNoise and the globals of MistVolumeState; plus the HLSL twin check (every
// MIST_* #define of Shaders/Include/BwMistVolume.hlsl equals its C# value).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Megastructure.Rendering;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class MistTests
    {
        static string Root => Path.GetDirectoryName(Application.dataPath);

        // ------------------------------------------------------------------ plume and veil
        [Test]
        public void Plume_PeakHorizontalOpticalDepthIsOne()
        {
            // a horizontal ray along x through the core (z 562, y below 0): tau = Sigma int rho dx = 1.0 (fbm term at 1)
            double tau = 0.0, dx = 0.05;
            for (double x = -6.0; x > -2500.0; x -= dx) tau += MistVolumeMath.Sigma * MistVolumeMath.PlumeDensity(x - 0.5 * dx, -10.0, 562.0) * dx;
            Assert.AreEqual(1.0, tau, 0.005, "peak horizontal optical depth (14.5.3)");
            // the formula of H2: Gaussian in z (sigma 150), exponential above y 0 (scale 130)
            // denser near the face (x profile e^(-d / 600 m)): across the plume (along z) near the face tau is 0.6 and more
            double across = 0.0;
            for (double z = 0.0; z < 1200.0; z += 0.5) across += MistVolumeMath.Sigma * MistVolumeMath.PlumeDensity(-60.0, -10.0, z + 0.25) * 0.5;
            Assert.Greater(across, 0.55, "across the plume at x -60 (KS06 must show it)");
            double r = MistVolumeMath.PlumeDensity(-600.0, 0.0, 712.0) / MistVolumeMath.PlumeDensity(-600.0, 0.0, 562.0);
            Assert.AreEqual(Math.Exp(-0.5), r, 1e-9, "sigma 150 m along z");
            double h = MistVolumeMath.PlumeDensity(-600.0, 130.0, 562.0) / MistVolumeMath.PlumeDensity(-600.0, 0.0, 562.0);
            Assert.AreEqual(Math.Exp(-1.0), h, 1e-9, "scale height 130 m above y 0");
            Assert.AreEqual(MistVolumeMath.PlumeDensity(-600.0, -3000.0, 562.0), MistVolumeMath.PlumeDensity(-600.0, 0.0, 562.0), 1e-12, "constant below y 0");
            Assert.AreEqual(0.0, MistVolumeMath.PlumeDensity(-5.9, -10.0, 562.0), 1e-12, "nothing east of the lee face");
            Assert.AreEqual(0.0, MistVolumeMath.PlumeDensity(-2500.1, -10.0, 562.0), 1e-12, "nothing beyond x -2500");
        }

        [Test]
        public void Veil_HasOpticalDepth0p02InsideItsWindow()
        {
            double w = MistVolumeMath.VeilWeight(600.0);
            Assert.AreEqual(1.0, w, 1e-12);
            double tau = 0.0, dx = 0.05;
            for (double x = -6.0; x > -1600.0; x -= dx) tau += MistVolumeMath.Sigma * MistVolumeMath.VeilDensity(x - 0.5 * dx, 100.0, 600.0, w) * dx;
            Assert.AreEqual(0.02, tau, 0.0005, "spray veil optical depth (H2)");
            Assert.AreEqual(0.0, MistVolumeMath.VeilDensity(-200.0, 450.0, 600.0, w), 1e-12, "veil ends at y 400");
            // no seam along the face plane (Wave X, back640): the veil starts from 0 at x -6
            Assert.Less(MistVolumeMath.VeilDensity(-6.5, 100.0, 600.0, w), 1e-3 * MistVolumeMath.VeilRho, "veil soft at the lee face");
            Assert.AreEqual(MistVolumeMath.VeilRho, MistVolumeMath.VeilDensity(-200.0, 100.0, 600.0, w), 1e-9 * MistVolumeMath.VeilRho, "full 150 m in");
            Assert.AreEqual(0.0, MistVolumeMath.VeilWeight(540.0), 1e-12, "off before s 545");
            Assert.AreEqual(0.0, MistVolumeMath.VeilWeight(660.0), 1e-12, "off after s 655");
            Assert.AreEqual(1.0, MistVolumeMath.VeilWeight(560.0), 1e-12);
            Assert.AreEqual(1.0, MistVolumeMath.VeilWeight(640.0), 1e-12);
            // continuous ramps: at most 0.1 per 0.1 m of s
            double prev = MistVolumeMath.VeilWeight(530.0);
            for (double s = 530.1; s < 670.0; s += 0.1)
            {
                double v = MistVolumeMath.VeilWeight(s);
                Assert.Less(Math.Abs(v - prev), 0.012, "veil step at s " + s.ToString("0.0", CultureInfo.InvariantCulture));
                prev = v;
            }
        }

        // ------------------------------------------------------------------ curtain
        [Test]
        public void Curtain_RadiusWaterFractionAndParabola()
        {
            Assert.AreEqual(3.0, MistVolumeMath.CurtainRadius(90.0), 1e-9, "Gaussian radius 3 m at 90 m");
            Assert.AreEqual(60.0, MistVolumeMath.CurtainRadius(1000.0), 1e-6, "Gaussian radius 60 m at 1 km");
            Assert.AreEqual(1.0, MistVolumeMath.WaterFraction(90.0), 1e-12);
            Assert.AreEqual(0.0, MistVolumeMath.WaterFraction(1000.0), 1e-12, "water fraction 0 at 1000 m");
            // the 14.5.3 parabola: x = -6 - 8.3 t, y = -11.5 - 4.9 t^2 (t 4.7 s: x -45.0, y -119.7)
            double t = 4.7;
            Assert.AreEqual(-6.0 - 8.3 * t, MistVolumeMath.CurtainX(MistVolumeMath.CurtainDropAt(t)), 1e-9);
            Assert.AreEqual(-45.0, MistVolumeMath.CurtainX(MistVolumeMath.CurtainDropAt(t)), 0.05);
            // a ray from the KS09 eye straight down the fall hits the curtain surface exactly
            var o = new D3(-4.0, 1.7, 572.0);
            double pitch = -80.0 * BwMath.Deg, yaw = -90.0 * BwMath.Deg;
            var d = new D3(Math.Cos(pitch) * Math.Sin(yaw), Math.Sin(pitch), Math.Cos(pitch) * Math.Cos(yaw));
            double s = MistVolumeMath.CurtainHit(o, d);
            Assert.Greater(s, 0.0, "the ray meets the curtain");
            var p = o + d * s;
            double ySurf = MistVolumeMath.LipY - MistVolumeMath.CurtainK * (p.x - MistVolumeMath.LipX) * (p.x - MistVolumeMath.LipX);
            Assert.AreEqual(ySurf, p.y, 1e-6, "hit on y = -11.5 - k (x + 6)^2");
            Assert.LessOrEqual(p.x, MistVolumeMath.LipX + 1e-9);
            // the across-slot profile fills the piers' 4 m gaps once the curtain has spread
            Assert.Less(MistVolumeMath.CurtainSlotProfile(562.0, 0.5), 0.05, "a pier gap (pier 7, z 560-564) is open at 0.5 m radius");
            Assert.Greater(MistVolumeMath.CurtainSlotProfile(562.0, 30.0), 0.7, "and filled at 30 m (16 / 20 of the full width)");
            Assert.Greater(MistVolumeMath.CurtainSlotProfile(432.0, 3.0), 0.99, "slot centre");
        }

        // ------------------------------------------------------------------ sprays
        [Test]
        public void Spray_EnvelopeTimingAndAtMost5PercentPerFrame()
        {
            Assert.AreEqual(0.0, MistVolumeMath.SprayEnvelope(0.0), 1e-12);
            Assert.AreEqual(1.0, MistVolumeMath.SprayEnvelope(0.6), 1e-12, "0.6 s in");
            Assert.AreEqual(0.0, MistVolumeMath.SprayEnvelope(3.0), 1e-12, "2.4 s out");
            double dt = 1.0 / 60.0, worst = 0.0;
            for (double a = 0.0; a < 3.2; a += dt) worst = Math.Max(worst, Math.Abs(MistVolumeMath.SprayEnvelope(a + dt) - MistVolumeMath.SprayEnvelope(a)));
            Assert.LessOrEqual(worst, 0.05, "density change per frame at 60 fps (H2: at most 5 %)");
            // peak optical depth through the centre along x: 0.5 at strength 1 (14.5.4 "at most 0.5")
            double rho = MistVolumeMath.SprayRho(1.0), A = 1.0 / (MistVolumeMath.SprayAx * MistVolumeMath.SprayAx), h = MistVolumeMath.SprayAx;
            Assert.AreEqual(0.5, MistVolumeMath.EllipsoidChordTau(rho, A, h, -h, h), 1e-9);
            // the closed form against a numerical integral of (1 - r^2)^2 along an off-centre chord
            double off = 0.6, q = 1.0 - off * off, hh = Math.Sqrt(q / A), num = 0.0, du = 1e-3;
            for (double u = -hh; u < hh; u += du) { double r2 = off * off + A * (u + 0.5 * du) * (u + 0.5 * du); num += MistVolumeMath.Sigma * rho * (1 - r2) * (1 - r2) * du; }
            Assert.AreEqual(num, MistVolumeMath.EllipsoidChordTau(rho, A, hh, -hh, hh), 1e-4, "closed-form chord optical depth");
        }

        [Test]
        public void Spray_YoungBurstIsDenseAndThinsToTheRoadMist()
        {
            // the white burst: optical depth 2.5 through the centre at the density peak (0.6 s), 0.5 once it is mist (>= 1.4 s)
            double rho = MistVolumeMath.SprayRho(1.0), A = 1.0 / (MistVolumeMath.SprayAx * MistVolumeMath.SprayAx), h = MistVolumeMath.SprayAx;
            double tau0 = MistVolumeMath.EllipsoidChordTau(rho, A, h, -h, h);
            Assert.AreEqual(MistVolumeMath.SprayCoreTau, tau0 * MistVolumeMath.SprayDensityFactor(0.6), 1e-9, "young burst at 0.6 s");
            Assert.AreEqual(MistVolumeMath.SprayPeakTau * MistVolumeMath.SprayEnvelope(1.5), tau0 * MistVolumeMath.SprayDensityFactor(1.5), 1e-9, "mist at 1.5 s");
            // the drift starts at 0.8 s; by 1.4 s the core is gone (14.5.4 'optical depth at most 0.5' over the road)
            Assert.LessOrEqual(tau0 * MistVolumeMath.SprayDensityFactor(MistVolumeMath.SprayCoreFade1), 0.5 + 1e-9);
            // change per 60 fps frame at most 5 % of the peak (H2 'density change at most 5 % per frame')
            double dt = 1.0 / 60.0, worst = 0.0, peak = 0.0;
            for (double a = 0.0; a < 3.2; a += dt) peak = Math.Max(peak, MistVolumeMath.SprayDensityFactor(a));
            for (double a = 0.0; a < 3.2; a += dt) worst = Math.Max(worst, Math.Abs(MistVolumeMath.SprayDensityFactor(a + dt) - MistVolumeMath.SprayDensityFactor(a)) / peak);
            Assert.LessOrEqual(worst, 0.05, "per-frame change relative to the peak");
        }

        // ------------------------------------------------------------------ the dam's shadow
        static D3 SunDir(double azDeg, double elDeg)
        {
            double a = azDeg * BwMath.Deg, e = elDeg * BwMath.Deg;
            return new D3(Math.Cos(e) * Math.Sin(a), Math.Sin(e), Math.Cos(e) * Math.Cos(a));
        }

        [Test]
        public void DamShadow_SegmentAverageIsExactAndTheKS08ArchIsLit()
        {
            BulwarkSchedule.Sun(604.0, out double saz, out double sel);
            D3 l = SunDir(saz, sel);
            var o = new D3(-4.0, 1.7, 604.0);
            // the segment average against a fine numerical average with the same (midpoint) edge width
            foreach (double el in new[] { -10.0, -16.0, -17.0, -25.0 })
            {
                D3 d = SunDir(285.0, el);
                foreach (var seg in new[] { (10.0, 60.0), (60.0, 400.0), (400.0, 2500.0) })
                {
                    double ta = seg.Item1, tb = seg.Item2, tm = 0.5 * (ta + tb);
                    double dist = Math.Max((MistVolumeMath.PlumeXFace - (o.x + d.x * tm)) / l.x, 0.0);
                    double w2 = 2.0 * (MistVolumeMath.DamSoft0 + MistVolumeMath.SunPenumbra * dist), sum = 0.0;
                    int n = 20000;
                    for (int i = 0; i < n; i++)
                    {
                        double t = ta + (tb - ta) * (i + 0.5) / n;
                        var p = new D3(o.x + d.x * t, o.y + d.y * t, o.z + d.z * t);
                        double yHit = p.y + l.y * (MistVolumeMath.PlumeXFace - p.x) / l.x - MistVolumeMath.DamCrestY;
                        sum += MistVolumeMath.Clamp01(yHit / w2 + 0.5);
                    }
                    Assert.AreEqual(sum / n, MistVolumeMath.DamLitSegment(o, d, l, ta, tb), 1e-3, "el " + el + " [" + ta + ", " + tb + "]");
                }
            }
            // the whole primary arch above the horizon lies in lit mist: its directions are above the shadow plane
            D3 anti = SunDir(saz + 180.0, -sel);
            for (int i = 0; i < 360; i++)
            {
                // points of the 42 deg circle with elevation above the visible horizon (-2.49 deg)
                double phi = 2.0 * Math.PI * i / 360.0;
                D3 u = new D3(0, 1, 0), a = anti;
                D3 e1 = Norm(Cross(a, u)), e2 = Cross(e1, a);
                double c = Math.Cos(42.0 * BwMath.Deg), s = Math.Sin(42.0 * BwMath.Deg);
                D3 d = new D3(a.x * c + (e1.x * Math.Cos(phi) + e2.x * Math.Sin(phi)) * s, a.y * c + (e1.y * Math.Cos(phi) + e2.y * Math.Sin(phi)) * s, a.z * c + (e1.z * Math.Cos(phi) + e2.z * Math.Sin(phi)) * s);
                if (Math.Asin(d.y) / BwMath.Deg < -2.49 || d.x > -0.05) continue;
                foreach (double t in new[] { 100.0, 600.0, 2000.0 })
                    Assert.Greater(MistVolumeMath.DamLitAt(new D3(o.x + d.x * t, o.y + d.y * t, o.z + d.z * t), l), 0.99, "arch point " + i + " at " + t + " m");
            }
            // below the antisolar elevation the plume is in the dam's shadow beyond a few metres
            D3 down = SunDir(287.2, -30.0);
            Assert.AreEqual(0.0, MistVolumeMath.DamLitAt(new D3(o.x + down.x * 100, o.y + down.y * 100, o.z + down.z * 100), l), 1e-9, "shadowed mist at -30 deg");
            // the storm's evening sun from the west never shadows the lee side
            Assert.AreEqual(1.0, MistVolumeMath.DamLitAt(new D3(-300, -500, 600), SunDir(305.0, -2.0)), 1e-12);
        }

        [Test]
        public void Plume_OnsetHugsTheFaceBelowTheCrestAndLeansOffAboveIt()
        {
            for (double n = 0.0; n <= 1.0; n += 0.1)
            {
                Assert.GreaterOrEqual(MistVolumeMath.OnsetOffset(-500.0, n), 0.0, "never east of the face");
                Assert.LessOrEqual(MistVolumeMath.OnsetOffset(-500.0, n), MistVolumeMath.OnsetBase + 1e-9, "below the crest within 12 m of the face");
                Assert.GreaterOrEqual(MistVolumeMath.OnsetOffset(260.0, n), 0.35 * (MistVolumeMath.OnsetBase + MistVolumeMath.OnsetTop) - 1e-9, "above y 220 at least 35 % of the reach");
            }
            // the horizontal optical depth along x below the crest loses at most rho0 x 12 m x Sigma
            Assert.LessOrEqual(MistVolumeMath.PlumeRho0 * MistVolumeMath.OnsetBase * MistVolumeMath.Sigma, 0.03);
        }

        static D3 Cross(D3 a, D3 b) => new D3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        static D3 Norm(D3 a) { double n = Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z); return new D3(a.x / n, a.y / n, a.z / n); }

        // ------------------------------------------------------------------ phase and bow
        [Test]
        public void Phase_HgIsNormalised()
        {
            foreach (double g in new[] { 0.0, 0.275, 0.55 })
            {
                double sum = 0.0; int n = 20000;
                for (int i = 0; i < n; i++)
                {
                    double mu = -1.0 + (i + 0.5) * 2.0 / n;
                    sum += MistVolumeMath.Hg(mu, g) * 2.0 * Math.PI * (2.0 / n);
                }
                Assert.AreEqual(1.0, sum, 1e-3, "HG g " + g);
            }
            // research B 3.5: p(138 deg) = 0.0180 for g 0.55
            Assert.AreEqual(0.0180, MistVolumeMath.Hg(Math.Cos(138.0 * BwMath.Deg), 0.55), 0.0005);
        }

        [Test]
        public void Bow_TopAt25p4RedOutermostDarkBandAndSecondary()
        {
            BulwarkSchedule.Sun(604.0, out double az, out double el);
            Assert.AreEqual(107.2, az, 0.05, "sun az at s 604");
            Assert.AreEqual(16.6, el, 0.05, "sun el at s 604");
            // the arch top read on the red outer edge (the R peak of the bow term): 42.0 - 16.6
            Assert.AreEqual(25.4, MistVolumeMath.BowTopElevation(el, 0), 0.5, "14.10.1 T-NUM bow top");
            Assert.AreEqual(34.4, MistVolumeMath.BowSecondary[0] - el, 0.5, "secondary top (red inner edge)");
            // red outermost on the primary, reversed on the secondary
            Assert.Greater(MistVolumeMath.BowPrimary[0], MistVolumeMath.BowPrimary[1]);
            Assert.Greater(MistVolumeMath.BowPrimary[1], MistVolumeMath.BowPrimary[2]);
            Assert.Less(MistVolumeMath.BowSecondary[0], MistVolumeMath.BowSecondary[1]);
            Assert.Less(MistVolumeMath.BowSecondary[1], MistVolumeMath.BowSecondary[2]);
            // per-channel peaks of the summed term sit at the H2 centres
            for (int c = 0; c < 3; c++)
            {
                double best = 0.0, at = 0.0;
                for (double th = 35.0; th < 46.0; th += 0.01) { double b = MistVolumeMath.Bow(c, th); if (b > best) { best = b; at = th; } }
                Assert.AreEqual(MistVolumeMath.BowPrimary[c], at, 0.02, "channel " + c + " peak");
            }
            Assert.AreEqual(0.6, MistVolumeMath.Band(46.0), 1e-9, "dark band 0.6 between 42.5 and 50");
            Assert.AreEqual(1.0, MistVolumeMath.Band(30.0), 1e-9);
            Assert.AreEqual(1.0, MistVolumeMath.Band(56.0), 1e-9);
            Assert.AreEqual(0.8, MistVolumeMath.Band(42.5), 1e-9, "edge midpoint");
        }

        // ------------------------------------------------------------------ noise and the frame-to-frame change
        [Test]
        public void Noise_IsPeriodicAndTheAdvectedDensityChangesAtMost5PercentPerFrame()
        {
            MistVolumeNoise.Generate(out double lo, out double hi);
            Assert.Less(lo, hi);
            var pts = new[] { new D3(0.13, 0.71, 0.29), new D3(0.5, 0.5, 0.5), new D3(0.91, 0.07, 0.66) };
            foreach (var p in pts)
            {
                double a = MistVolumeNoise.Sample(p.x, p.y, p.z);
                Assert.AreEqual(a, MistVolumeNoise.Sample(p.x + 1.0, p.y, p.z), 1e-9, "period x");
                Assert.AreEqual(a, MistVolumeNoise.Sample(p.x, p.y + 1.0, p.z), 1e-9, "period y");
                Assert.AreEqual(a, MistVolumeNoise.Sample(p.x, p.y, p.z - 1.0), 1e-9, "period z");
            }
            // modulation 0.65 + 0.35 fbm(p / 240 m tile + wind t / 240): change per frame at 60 fps
            double tile = MistVolumeMath.FbmScale * MistVolumeNoise.BaseCells, dt = 1.0 / 60.0, worst = 0.0;
            var rnd = new System.Random(7);
            for (int i = 0; i < 400; i++)
            {
                double x = rnd.NextDouble() * 2000.0, y = rnd.NextDouble() * 300.0, z = rnd.NextDouble() * 900.0, t = rnd.NextDouble() * 600.0;
                double m0 = MistVolumeMath.FbmBase + MistVolumeMath.FbmAmp * MistVolumeNoise.Sample((x + MistVolumeMath.WindX * t) / tile, (y + MistVolumeMath.WindY * t) / tile, (z + MistVolumeMath.WindZ * t) / tile);
                double t1 = t + dt;
                double m1 = MistVolumeMath.FbmBase + MistVolumeMath.FbmAmp * MistVolumeNoise.Sample((x + MistVolumeMath.WindX * t1) / tile, (y + MistVolumeMath.WindY * t1) / tile, (z + MistVolumeMath.WindZ * t1) / tile);
                worst = Math.Max(worst, Math.Abs(m1 - m0) / m0);
            }
            Assert.LessOrEqual(worst, 0.05, "density change per frame (H2)");
            Assert.Less(worst, 0.01, "in fact far below: the 5 m/s breathing crosses a 60 m cell in 12 s");
        }

        [Test]
        public void SampleGrid_IsFixedMonotonicAndCoversTheSpan()
        {
            Assert.AreEqual(24, MistVolumeMath.Samples);
            Assert.AreEqual(0.0, MistVolumeMath.SampleEdge(0), 1e-12);
            Assert.AreEqual(1.0, MistVolumeMath.SampleEdge(24), 1e-12);
            for (int i = 1; i <= 24; i++) Assert.Greater(MistVolumeMath.SampleEdge(i), MistVolumeMath.SampleEdge(i - 1));
        }

        // ------------------------------------------------------------------ globals and command-line switches
        [Test]
        public void State_BowAndSunSwitchesReachTheShader()
        {
            var keep = CliArgs.Args;
            try
            {
                CliArgs.Override(new[] { "Bulwark.exe" });
                MistVolumeState.ResetCli();
                MistVolumeState.Tick(604.0, 10.0);
                Vector4 m0 = Shader.GetGlobalVector("_BwMist0");
                Assert.AreEqual((float)MistVolumeMath.BowK, m0.z, 1e-6, "bow K on");
                Assert.AreEqual(1f, m0.w, 1e-6, "sun on");
                Assert.AreEqual((float)(MistVolumeMath.VeilRho), m0.y, 1e-9, "veil at full weight at s 604");
                CliArgs.Override(new[] { "Bulwark.exe", "-mist-bow", "off", "-mist-sun", "off" });
                MistVolumeState.ResetCli();
                MistVolumeState.Tick(604.0, 10.0);
                m0 = Shader.GetGlobalVector("_BwMist0");
                Assert.AreEqual(0f, m0.z, 1e-9, "-mist-bow off: K = 0");
                Assert.AreEqual(0f, m0.w, 1e-9, "-mist-sun off: no sun term, so no bow (H2 test 'bow absent with the sun off')");
            }
            finally
            {
                CliArgs.Override(keep);
                MistVolumeState.ResetCli();
            }
        }

        [Test]
        public void State_WallSpraysComeFromWaveImpactsAndTheConduitOnlyInBeat7()
        {
            WaveImpacts.Clear();
            try
            {
                WaveImpacts.Publish(new WaveImpact { Z = 175f, X = 6f, Strength = 1f, T0 = 9.4, Height = 14f });
                MistVolumeState.Tick(160.0, 10.0);
                Assert.AreEqual(1, MistVolumeState.ActiveSprays, "one live spray at age 0.6 s");
                Vector4[] c = Shader.GetGlobalVectorArray("_BwMistSprayC");
                Assert.Greater(c[0].w, 0f, "density x envelope");
                Assert.AreEqual(175f, c[0].z, 1e-4);
                MistVolumeState.Tick(160.0, 13.0);
                Assert.AreEqual(0, MistVolumeState.ActiveSprays, "expired after 3 s");
                MistVolumeState.Tick(1450.0, 10.0);
                Assert.AreEqual(0, MistVolumeState.ActiveSprays, "no wall sprays in beat 7");
                Assert.AreEqual(Megastructure.Beats.ConduitSpray.Active, MistVolumeState.ConduitActive, "the conduit box follows the beat 7 contract");
                MistVolumeState.Tick(1000.0, 10.0);
                Assert.AreEqual(0, MistVolumeState.ActiveSprays, "no sprays on the stairs");
                Assert.IsFalse(MistVolumeState.ConduitActive, "no conduit spray on the stairs");
            }
            finally { WaveImpacts.Clear(); }
        }

        // ------------------------------------------------------------------ sea-disturbance sprays (Addendum A1)
        static double TimeTo(double omega, double frac)
        {
            // critically damped from rest toward a unit step: 1 - (1 + w t) e^(-w t) = frac
            double x = 0.0, v = 0.0, t = 0.0, dt = 1.0 / 240.0;
            while (x < frac && t < 60.0) { MistVolumeMath.Follow(ref x, ref v, 1.0, omega, dt); t += dt; }
            return t;
        }

        static double MaxStepPerFrame(double from, double to, double rise, double fall)
        {
            double x = from, v = 0.0, worst = 0.0;
            for (int i = 0; i < 30 * 12; i++)
            {
                double p = x;
                MistVolumeMath.FollowAsym(ref x, ref v, to, rise, fall, 1.0 / 30.0);
                worst = Math.Max(worst, Math.Abs(x - p));
            }
            return worst;
        }

        [Test]
        public void Disturbance_FollowersRiseIn1To2sFallIn3To6sAtMost5PercentPerFrame()
        {
            double rise = TimeTo(MistVolumeMath.DistRiseOmega, 0.95);
            Assert.That(rise, Is.InRange(1.0, 2.0), "breach / tentacle spray rises in 1-2 s (95 %)");
            double fall = TimeTo(MistVolumeMath.DistFallOmega, 0.95);
            Assert.That(fall, Is.InRange(3.0, 6.0), "falls in 3-6 s (95 %)");
            Assert.That(TimeTo(MistVolumeMath.VeilFallOmega, 0.95), Is.InRange(3.0, 6.0), "the falling-water veil settles within 6 s");
            Assert.That(TimeTo(MistVolumeMath.SlapRiseOmega, 0.95), Is.LessThanOrEqualTo(2.0), "slap burst");
            Assert.That(TimeTo(MistVolumeMath.SlapFallOmega, 0.95), Is.InRange(3.0, 6.0), "slap spray falls in 3-6 s");
            Assert.That(TimeTo(MistVolumeMath.CascadeFallOmega, 0.95), Is.InRange(3.0, 6.0), "a cascade drains within 3-6 s of the body stopping");
            foreach (var w in new[] { (MistVolumeMath.DistRiseOmega, MistVolumeMath.DistFallOmega), (MistVolumeMath.SlapRiseOmega, MistVolumeMath.SlapFallOmega),
                                      (MistVolumeMath.VeilRiseOmega, MistVolumeMath.VeilFallOmega), (MistVolumeMath.CascadeRiseOmega, MistVolumeMath.CascadeFallOmega) })
            {
                Assert.LessOrEqual(MaxStepPerFrame(0.0, 1.0, w.Item1, w.Item2), 0.05, "rise: density change at most 5 % per 30 fps frame (H2)");
                Assert.LessOrEqual(MaxStepPerFrame(1.0, 0.0, w.Item1, w.Item2), 0.05, "fall: at most 5 % per frame");
            }
            // exact solution: one big step equals many small ones
            double xa = 0.0, va = 0.0, xb = 0.0, vb = 0.0;
            MistVolumeMath.Follow(ref xa, ref va, 1.0, 3.2, 0.5);
            for (int i = 0; i < 50; i++) MistVolumeMath.Follow(ref xb, ref vb, 1.0, 3.2, 0.01);
            Assert.AreEqual(xa, xb, 1e-9);
            MistVolumeMath.Follow(ref xa, ref va, 0.7, 3.2, double.PositiveInfinity);
            Assert.AreEqual(0.7, xa, 1e-12, "snap");
        }

        [Test]
        public void Disturbance_CutoffWeightIsContinuousAtTheBudgetEdge()
        {
            Assert.AreEqual(1.0, MistVolumeMath.CutoffWeight(0.3, 0.0), 1e-12, "nothing excluded: full weight");
            Assert.AreEqual(0.0, MistVolumeMath.CutoffWeight(1.0, 1.0), 1e-12, "equal scores: both at 0 (the swap is continuous)");
            Assert.AreEqual(1.0, MistVolumeMath.CutoffWeight(1.0 / (1.0 - MistVolumeMath.CutoffBand) + 1e-9, 1.0), 1e-9, "30 % above: full weight");
            double prev = 0.0;
            for (double sc = 1.0; sc < 2.0; sc += 0.01)
            {
                double w = MistVolumeMath.CutoffWeight(sc, 1.0);
                Assert.GreaterOrEqual(w + 1e-12, prev, "monotonic");
                Assert.LessOrEqual(w - prev, 0.08, "smooth");
                prev = w;
            }
            Assert.Greater(MistVolumeMath.SprayScore(2.5, 60.0, 400.0), MistVolumeMath.SprayScore(2.5, 60.0, 800.0), "nearer scores higher");
            Assert.Greater(MistVolumeMath.SprayScore(2.5, 60.0, 400.0), MistVolumeMath.SprayScore(1.0, 60.0, 400.0), "stronger scores higher");
        }

        static void PublishOne(SeaDisturbanceKind kind, float x, float z, float halfAlong, float halfAcross, float strength, float height)
        {
            SeaDisturbances.Begin();
            if (strength > 0f)
                SeaDisturbances.Set(new SeaDisturbance { Kind = kind, X = x, Z = z, HalfAlong = halfAlong, HalfAcross = halfAcross, Heading = 0.3f, Strength = strength, HeightAboveSea = height });
            SeaDisturbances.Publish();
        }

        [Test]
        public void Disturbance_BreachColumnRisesToItsHeightAndFallsAfterTheBodySinks()
        {
            WaveImpacts.Clear(); SeaDisturbances.Clear(); MistVolumeState.ResetTrackers();
            bool cap = GameState.CaptureMode;
            GameState.CaptureMode = false;
            try
            {
                var eye = new Vector3(4.2f, 1.7f, 160f);
                double t = 100.0;
                PublishOne(SeaDisturbanceKind.Breach, 417f, 233f, 45f, 30f, 0f, 0f);
                MistVolumeState.Tick(160.0, t, eye);                       // first commit: snap to nothing
                Assert.AreEqual(0, MistVolumeState.DisturbanceSprays);
                double topExpect = MistVolumeMath.BreachFootTop(55.0);
                Assert.AreEqual(1.4 * (0.3 * 55.0 + 10.0), topExpect, 1e-9, "foot 1.4 x min(0.3 h + 10, 30)");
                double maxStep = 0.0, prevTau = 0.0, riseAt = -1.0;
                int sprays = 0;
                // the body rises 0 -> 55 m over 2.5 s (22 m/s: full cascade), then holds
                for (int f = 1; f <= 30 * 4; f++)
                {
                    t += 1.0 / 30.0;
                    float h = (float)Math.Min(55.0, 22.0 * f / 30.0);
                    PublishOne(SeaDisturbanceKind.Breach, 417f, 233f, 45f, 30f, 1f, h);
                    MistVolumeState.Tick(160.0, t, eye);
                    if (f == 75) sprays = MistVolumeState.DisturbanceSprays;
                    Vector4[] c = Shader.GetGlobalVectorArray("_BwMistSprayC");
                    Vector4[] r = Shader.GetGlobalVectorArray("_BwMistSprayR");
                    // the foot is the candidate with the larger horizontal semi-axes
                    int col = -1;
                    for (int i = 0; i < MistVolumeState.ActiveSprays; i++) if (col < 0 || r[i].x > r[col].x) col = i;
                    if (col < 0) continue;
                    double tau = MistVolumeMath.TauForRho(c[col].w, Math.Min(r[col].x, r[col].z)) / MistVolumeMath.BreachColumnTau;
                    maxStep = Math.Max(maxStep, Math.Abs(tau - prevTau));
                    prevTau = tau;
                    if (riseAt < 0 && r[col].y >= 0.95 * topExpect) riseAt = f / 30.0;
                }
                Assert.AreEqual(2, sprays, "a rising breach is a foot and a cascade of water sheeting off the body");
                Assert.That(riseAt, Is.InRange(1.0, 4.0), "the foot reaches 95 % of its height while the body rises");
                Assert.LessOrEqual(maxStep, 0.05, "foot density changes at most 5 % per frame");
                Vector4[] cc = Shader.GetGlobalVectorArray("_BwMistSprayC");
                Assert.AreEqual((float)BulwarkConst.SeaY, cc[0].y, 1e-4, "domes stand on the mean sea surface");
                Vector4[] ss = Shader.GetGlobalVectorArray("_BwMistSprayS");
                Assert.Less(ss[0].x, 0f, "the foot leans downwind (west, toward the dam)");
                // held: the cascade drains (below 1 %) within 6 s of the body stopping
                for (int f = 1; f <= 30 * 6; f++)
                {
                    t += 1.0 / 30.0;
                    PublishOne(SeaDisturbanceKind.Breach, 417f, 233f, 45f, 30f, 1f, 55f);
                    MistVolumeState.Tick(160.0, t, eye);
                }
                Assert.AreEqual(1, MistVolumeState.DisturbanceSprays, "a body that holds still keeps only its foot");
                Vector4[] rh = Shader.GetGlobalVectorArray("_BwMistSprayR");
                Assert.Less(rh[0].y, 55.0f, "the foot stays below the body's top (the eyes of K4 at +76 m stay clear)");
                // the body sinks: the spray falls within 3-6 s and the tracker ends
                double goneAt = -1.0;
                for (int f = 1; f <= 30 * 10; f++)
                {
                    t += 1.0 / 30.0;
                    PublishOne(SeaDisturbanceKind.Breach, 417f, 233f, 45f, 30f, 0f, 0f);
                    MistVolumeState.Tick(160.0, t, eye);
                    if (goneAt < 0 && MistVolumeState.DisturbanceSprays == 0) goneAt = f / 30.0;
                }
                Assert.That(goneAt, Is.InRange(3.0, 9.0), "the foot (below 1 % of its density) is gone 3-9 s after the body left the bus");
                Assert.AreEqual(0, MistVolumeState.ActiveTrackers, "tracker released");
                // capture stills snap: a fresh breach is at full height in one commit (no vertical speed: no cascade)
                MistVolumeState.ResetTrackers();
                GameState.CaptureMode = true;
                PublishOne(SeaDisturbanceKind.Breach, 417f, 233f, 45f, 30f, 1f, 55f);
                MistVolumeState.Tick(160.0, t, eye);
                Vector4[] r2 = Shader.GetGlobalVectorArray("_BwMistSprayR");
                Assert.AreEqual(1, MistVolumeState.DisturbanceSprays, "still: the foot only");
                Assert.AreEqual(topExpect, r2[0].y, 1e-3, "still: the foot at its target height");
                Assert.AreEqual(MistVolumeMath.BreachColumnScale * 30.0 + MistVolumeMath.BreachColumnPad, r2[0].x, 1e-3, "around the body");
            }
            finally
            {
                GameState.CaptureMode = cap;
                SeaDisturbances.Clear(); MistVolumeState.ResetTrackers(); WaveImpacts.Clear();
            }
        }

        [Test]
        public void Spray_NearTheEyeRisesSlowlyWithoutTheCore()
        {
            // Wave X: an impact 6 m from the KS11 eye flashed half the frame white (6.1 levels per frame, mistx-07-seqk5)
            for (double a = 0.0; a <= 3.0; a += 0.01)
                Assert.AreEqual(MistVolumeMath.SprayDensityFactor(a), MistVolumeMath.SprayDensityFactorNear(a, 0.0), 1e-12, "far: unchanged");
            Assert.AreEqual(1.0, MistVolumeMath.SprayNearness(6.0), 1e-12);
            Assert.AreEqual(0.0, MistVolumeMath.SprayNearness(35.0), 1e-12);
            double prev = 0.0, peak = 0.0, maxStep = 0.0;
            for (int f = 1; f <= 30 * 3; f++)
            {
                double v = MistVolumeMath.SprayDensityFactorNear(f / 30.0, 1.0);
                maxStep = Math.Max(maxStep, Math.Abs(v - prev));
                peak = Math.Max(peak, v);
                prev = v;
            }
            Assert.LessOrEqual(peak, 1.0 + 1e-9, "no dense core near the eye");
            Assert.LessOrEqual(maxStep, 0.05, "at most 5 % of the peak per 30 fps frame");
            Assert.AreEqual(0.0, MistVolumeMath.SprayDensityFactorNear(3.0, 1.0), 1e-12, "gone at the 3 s lifetime");
        }

        [Test]
        public void Disturbance_KrakenSpraysWinTheirSlotsAndTheK5ColumnIs50m()
        {
            WaveImpacts.Clear(); SeaDisturbances.Clear(); MistVolumeState.ResetTrackers();
            bool cap = GameState.CaptureMode;
            GameState.CaptureMode = true;
            try
            {
                // K5 (Addendum A1 1.6) as the show publishes it: Slap half length 25 m at (213.7, 991.0), 270 m from KS11
                double top = MistVolumeMath.SlapTop(0.0, 25.0, 1.0);
                Assert.AreEqual(77.5, top, 1e-9, "vertical semi-axis 2.1 x 25 + 25 (fix wave X2 S3-4)");
                // the visible top is 0.71 of the upper semi-axis (measured in the merged game, mistx-21 KS11 4K: 370 px for the
                // 64.5 m column): projected from KS11 (pitch +6, vFOV 64, 4K) with the column 27 deg off the view axis
                double yTop = BulwarkConst.SeaY + MistVolumeMath.SlapCentreK * top + 0.71 * MistVolumeMath.SlapUpperK * top, dist = Math.Sqrt(212.8 * 212.8 + 167.0 * 167.0), f = 2160.0 / (2.0 * Math.Tan(32.0 * Math.PI / 180.0));
                double eTop = Math.Atan((yTop - 1.7) / dist) - 6.0 * Math.PI / 180.0, eBot = Math.Atan((BulwarkConst.SeaY - 1.7) / dist) - 6.0 * Math.PI / 180.0;
                double px = f * (Math.Tan(eTop) - Math.Tan(eBot)) / Math.Cos(Math.Atan2(212.8, 167.0) - 25.0 * Math.PI / 180.0);
                Assert.That(px, Is.InRange(405.0, 485.0), "the K5 column is 445 +- 40 px tall at 4K from KS11 (A1 4.9)");
                // six fresh wall sprays at the parapet in front of the eye and the K5 slap 209 m away: the slap keeps a slot
                var eye = new Vector3(0.9f, 1.7f, 824f);
                for (int k = 0; k < WaveImpacts.Max; k++)
                    WaveImpacts.Publish(new WaveImpact { X = 7f, Z = 810f + 5f * k, Strength = 1f, T0 = 63.4, Height = 14f });
                PublishOne(SeaDisturbanceKind.Slap, 160f, 960f, 25f, 12f, 1f, 0f);
                MistVolumeState.Tick(824.0, 64.0, eye);
                Assert.AreEqual(1, MistVolumeState.DisturbanceSprays, "kraken priority: the slap wins a slot over the wall sprays");
                // white-water share per slot: the kraken spray 0.85, the near wall sprays 0.3 (no strobe, mistx-05-seqk5)
                Vector4[] cw = Shader.GetGlobalVectorArray("_BwMistSprayC");
                Vector4[] ww = Shader.GetGlobalVectorArray("_BwMistSprayW");
                for (int i = 0; i < MistVolumeState.ActiveSprays; i++)
                {
                    bool kraken = Math.Abs(cw[i].x - 160f) < 1f;
                    // the wall sprays 4-24 m from the eye: between SprayWhiteWall (near) and SprayWhite (30 m and farther)
                    if (kraken) Assert.AreEqual(MistVolumeMath.SlapWhite, ww[i].x, 1e-4, "white share of the slap");
                    else Assert.That(ww[i].x, Is.InRange((float)MistVolumeMath.SprayWhiteWall - 1e-4f, (float)MistVolumeMath.SprayWhite + 1e-4f), "white share of slot " + i);
                }
                Assert.LessOrEqual(MistVolumeState.ActiveSprays, MistVolumeMath.SprayBudget);
            }
            finally
            {
                GameState.CaptureMode = cap;
                SeaDisturbances.Clear(); MistVolumeState.ResetTrackers(); WaveImpacts.Clear();
            }
        }

        [Test]
        public void Disturbance_TentacleAndSlapSpraysAndTheSixSlotBudget()
        {
            WaveImpacts.Clear(); SeaDisturbances.Clear(); MistVolumeState.ResetTrackers();
            bool cap = GameState.CaptureMode;
            GameState.CaptureMode = true;   // stills: followers snap
            try
            {
                var eye = new Vector3(4.2f, 1.7f, 160f);
                PublishOne(SeaDisturbanceKind.TentacleRise, 149f, 228f, 3.5f, 3.5f, 1f, 38f);
                MistVolumeState.Tick(160.0, 64.0, eye);
                Assert.AreEqual(1, MistVolumeState.DisturbanceSprays, "a still tentacle: its foot (no vertical speed: no cascade)");
                Vector4[] r = Shader.GetGlobalVectorArray("_BwMistSprayR");
                Assert.AreEqual(MistVolumeMath.TentacleFootTop(38.0, 3.5), r[0].y, 1e-3, "foot semi-axis 1.4 x min(0.25 h + 6, 4 r + 6, 30)");
                Assert.AreEqual(1.4 * 15.5, MistVolumeMath.TentacleFootTop(38.0, 3.5), 1e-9);
                Assert.AreEqual(MistVolumeMath.TentacleScale * 3.5 + MistVolumeMath.TentaclePad, r[0].x, 1e-3, "around its waterline radius");
                // Shadow and Wake throw no spray
                PublishOne(SeaDisturbanceKind.Shadow, 300f, 300f, 60f, 40f, 1f, 0f);
                MistVolumeState.ResetTrackers();
                MistVolumeState.Tick(160.0, 64.0, eye);
                Assert.AreEqual(0, MistVolumeState.DisturbanceSprays, "a submerged shadow has no spray");
                // eight slaps: the budget keeps 6 (H2: plume + curtain + 6 sprays = 8 emitters)
                MistVolumeState.ResetTrackers();
                SeaDisturbances.Begin();
                for (int k = 0; k < 8; k++)
                    SeaDisturbances.Set(new SeaDisturbance { Kind = SeaDisturbanceKind.Slap, X = 100f + 60f * k, Z = 200f + 90f * k, HalfAlong = 20f, HalfAcross = 5f, Strength = 1f, HeightAboveSea = 0f });
                SeaDisturbances.Publish();
                MistVolumeState.Tick(160.0, 64.0, eye);
                Assert.LessOrEqual(MistVolumeState.ActiveSprays, MistVolumeMath.SprayBudget, "at most 6 spray slots");
                Assert.AreEqual(2, MistVolumeState.ExcludedSprays, "the two farthest wait");
                Vector4[] c = Shader.GetGlobalVectorArray("_BwMistSprayC");
                Assert.Less(c[0].x, 200f, "the nearest slap takes the first slot");
                // no kraken sprays below the crest (the stairs, the window gallery: the eye is not outside)
                MistVolumeState.Tick(1200.0, 64.0, eye);
                Assert.AreEqual(0, MistVolumeState.ActiveSprays, "no sprays inside the dam");
            }
            finally
            {
                GameState.CaptureMode = cap;
                SeaDisturbances.Clear(); MistVolumeState.ResetTrackers(); WaveImpacts.Clear();
            }
        }

        [Test]
        public void Spray_WallPlumeRisesFromTheSeaOverTheParapet()
        {
            // Wave X (show report KS11: "a floating spray ball near the gate tower"): the forced KS11 plume (x 6, z 845, 14 m,
            // age 0.9 s) is anchored at the sea, so it is dense just over the parapet and not only 3.5-15 m up
            WaveImpacts.Clear(); SeaDisturbances.Clear(); MistVolumeState.ResetTrackers();
            bool cap = GameState.CaptureMode;
            GameState.CaptureMode = true;
            try
            {
                var eye = new Vector3(0.9f, 1.7f, 824f);
                WaveImpacts.Publish(new WaveImpact { X = 6f, Z = 845f, Strength = 1f, T0 = 64.0 - 0.9, Height = 14f });
                MistVolumeState.Tick(824.0, 64.0, eye);
                Assert.AreEqual(1, MistVolumeState.ActiveSprays);
                Vector4 C = Shader.GetGlobalVectorArray("_BwMistSprayC")[0], R = Shader.GetGlobalVectorArray("_BwMistSprayR")[0];
                Vector4 S = Shader.GetGlobalVectorArray("_BwMistSprayS")[0], W = Shader.GetGlobalVectorArray("_BwMistSprayW")[0];
                Assert.AreEqual(0f, W.y, 1e-6, "one ellipsoid (no split)");
                Assert.AreEqual((float)(BulwarkConst.SeaY - MistVolumeMath.SprayFootDepth), C.y - R.y, 1e-3, "the column reaches down into the water");
                MistVolumeMath.SprayPose(6.0 + MistVolumeMath.WallJetOffset, 845.0, 14.0, 0.9, MistVolumeMath.SprayDriftReef, out _, out double cy0, out _, out double g0);
                Assert.AreEqual(cy0 + MistVolumeMath.SprayAy * g0, C.y + R.y, 1e-3, "the top of the thrown water is unchanged (KS04PlumeBounds)");
                double TauTo(double y)
                {
                    var o = new D3(eye.x, eye.y, eye.z);
                    var p = new D3(C.x, y, C.z);
                    D3 d = (p - o).Normalized;
                    return MistVolumeMath.SprayChordTauBound(C, R, S, o, d, 0.0, 200.0);
                }
                double centre = TauTo(C.y), parapet = TauTo(BulwarkConst.ParapetTop + 0.4);
                Assert.Greater(centre, 0.3, "the plume is dense at its centre");
                Assert.GreaterOrEqual(parapet, 0.5 * centre, "just over the parapet the plume has at least half its centre density (rises over it)");
                Assert.Greater(TauTo(C.y + R.y * 0.6), 0.0, "upper half unchanged");
            }
            finally
            {
                GameState.CaptureMode = cap;
                SeaDisturbances.Clear(); MistVolumeState.ResetTrackers(); WaveImpacts.Clear();
            }
        }

        [Test]
        public void Disturbance_K4EyesStayClearOfTheBreachSprayAtKK2()
        {
            // show report KK2: "the Breach spray around the head veils the eye". K4 at strength 1 (worst case), the head rising
            // 150 -> 178 m at 5.6 m/s (full cascade), then the eye ray from KK2 (3, 1.7, 806) to the right eye: A1 1.6 puts the
            // eyes at 6.3 deg from (3, 1.7, 788) at 598 m (y 1.7 + 598 tan 6.3 = 67.7 = sea + 75.7), 20 m tall, on the
            // walker's side of the head (centre (500, 1120), half width 80 m: 48 m toward the camera)
            WaveImpacts.Clear(); SeaDisturbances.Clear(); MistVolumeState.ResetTrackers();
            bool cap = GameState.CaptureMode;
            GameState.CaptureMode = false;
            try
            {
                var eye = new Vector3(3f, 1.7f, 806f);
                double t = 300.0;
                for (int f = 0; f <= 30 * 5; f++)
                {
                    t += 1.0 / 30.0;
                    float h = (float)Math.Min(178.0, 150.0 + 5.6 * f / 30.0);
                    PublishOne(SeaDisturbanceKind.Breach, 500f, 1120f, 90f, 80f, 1f, h);
                    MistVolumeState.Tick(806.0, t, eye);
                }
                Assert.AreEqual(2, MistVolumeState.DisturbanceSprays, "foot and cascade");
                Vector4[] c = Shader.GetGlobalVectorArray("_BwMistSprayC"), r = Shader.GetGlobalVectorArray("_BwMistSprayR");
                Vector4[] ss = Shader.GetGlobalVectorArray("_BwMistSprayS"), w = Shader.GetGlobalVectorArray("_BwMistSprayW");
                var o = new D3(eye.x, eye.y, eye.z);
                double dx = eye.x - 500.0, dz = eye.z - 1120.0, l = Math.Sqrt(dx * dx + dz * dz);
                foreach (double above in new[] { 66.0, 75.7, 86.0 })
                {
                    var p = new D3(500.0 + 48.0 * dx / l, BulwarkConst.SeaY + above, 1120.0 + 48.0 * dz / l);
                    D3 d = (p - o).Normalized;
                    double dist = (p - o).Length, tau = 0.0;
                    for (int i = 0; i < MistVolumeState.ActiveSprays; i++) tau += MistVolumeMath.SprayChordTauBound(c[i], r[i], ss[i], o, d, 0.0, dist);
                    Assert.Less(tau, 0.1, "optical depth to the eye at sea + " + above + " m (bound without noise)");
                }
                // the foot and the cascade still stand at the waterline (white water under the head)
                var pw = new D3(500.0 + 48.0 * dx / l, BulwarkConst.SeaY + 10.0, 1120.0 + 48.0 * dz / l);
                double tauW = 0.0;
                for (int i = 0; i < MistVolumeState.ActiveSprays; i++)
                    tauW += MistVolumeMath.SprayChordTauBound(c[i], r[i], ss[i], o, (pw - o).Normalized, 0.0, (pw - o).Length);
                Assert.Greater(tauW, 0.25, "white water at the waterline (half chord up to the point: 0.40 measured)");
                Assert.AreEqual(MistVolumeMath.BreachCascadeEyeK * 178.0 + MistVolumeMath.BreachCascadeEyePad, MistVolumeMath.BreachCascadeTop(178.0), 1e-9);
                Assert.AreEqual(1.4 * 30.0 * 0.65, MistVolumeMath.BreachFootTop(178.0, 0.3), 1e-9, "foot capped at 30 m and scaled by strength");
            }
            finally
            {
                GameState.CaptureMode = cap;
                SeaDisturbances.Clear(); MistVolumeState.ResetTrackers(); WaveImpacts.Clear();
            }
        }

        [Test]
        public void Disturbance_TheSlapIsAFullWhiteColumnAt0p8s()
        {
            // show report KS11: "the K5 slap column is barely visible": the show's bus strength is 0.687 at KS11 (0.8 s after the
            // impact); the burst's density and height are latched, so the still shows the full white column
            WaveImpacts.Clear(); SeaDisturbances.Clear(); MistVolumeState.ResetTrackers();
            bool cap = GameState.CaptureMode;
            GameState.CaptureMode = true;
            try
            {
                var eye = new Vector3(0.9f, 1.7f, 824f);
                PublishOne(SeaDisturbanceKind.Slap, 213.7f, 991.0f, 25f, 25f, 0.687f, 50f);
                MistVolumeState.Tick(824.0, 64.0, eye);
                Assert.AreEqual(1, MistVolumeState.DisturbanceSprays);
                Vector4 C = Shader.GetGlobalVectorArray("_BwMistSprayC")[0], R = Shader.GetGlobalVectorArray("_BwMistSprayR")[0];
                Vector4 W = Shader.GetGlobalVectorArray("_BwMistSprayW")[0];
                double vFull = MistVolumeMath.SlapTop(50.0, 25.0, 1.0);
                Assert.AreEqual(MistVolumeMath.SlapUpperK * vFull, R.y, 1e-3, "full column height (upper half)");
                Assert.AreEqual(BulwarkConst.SeaY + MistVolumeMath.SlapCentreK * vFull, C.y, 1e-3, "a column: the core stands above the sea");
                Assert.Less(C.y - R.y, (float)BulwarkConst.SeaY, "the column reaches through the waterline");
                Assert.AreEqual(MistVolumeMath.SlapAcrossScale * 25.0 + MistVolumeMath.SlapPad, R.x, 1e-3, "narrower across (a column, not a dome)");
                Assert.AreEqual(MistVolumeMath.RhoForTau(MistVolumeMath.SlapTau, Math.Min(R.x, R.z)), C.w, 1e-6, "full density (latched)");
                Assert.AreEqual(MistVolumeMath.SlapWhite, W.x, 1e-6, "white");
                Assert.AreEqual(1.0, MistVolumeMath.SlapDensityTarget(0.3), 1e-12);
                Assert.AreEqual(0.0, MistVolumeMath.SlapDensityTarget(0.0), 1e-12);
                Assert.Less(R.y, 4.0 * R.x, "a burst, not a needle");
            }
            finally
            {
                GameState.CaptureMode = cap;
                SeaDisturbances.Clear(); MistVolumeState.ResetTrackers(); WaveImpacts.Clear();
            }
        }

        [Test]
        public void Disturbance_TheK5SlapKeepsItsFullSlotAmongTheK4ArmsAndWallSprays()
        {
            // fix wave X2 S3-4 (x2-show KS11: no slap column; s 828: 44 m at a partial weight): at KS11 the K4 head collar, six
            // arm crossings 120-200 m away and six fresh wall sprays at the parapet compete for the six slots; the slap is drawn
            // at its full latched density (no cut-off fade)
            WaveImpacts.Clear(); SeaDisturbances.Clear(); MistVolumeState.ResetTrackers();
            bool cap = GameState.CaptureMode;
            GameState.CaptureMode = true;
            try
            {
                var eye = new Vector3(0.9f, 1.7f, 824f);
                for (int k = 0; k < WaveImpacts.Max; k++)
                    WaveImpacts.Publish(new WaveImpact { X = 7f, Z = 812f + 4f * k, Strength = 1f, T0 = 63.6, Height = 14f });
                SeaDisturbances.Begin();
                SeaDisturbances.Set(new SeaDisturbance { Kind = SeaDisturbanceKind.Breach, X = 500f, Z = 1120f, HalfAlong = 60f, HalfAcross = 45f, Strength = 0.15f, HeightAboveSea = 150f });
                SeaDisturbances.Set(new SeaDisturbance { Kind = SeaDisturbanceKind.Slap, X = 213.7f, Z = 991.0f, HalfAlong = 25f, HalfAcross = 25f, Heading = 60f * Mathf.Deg2Rad, Strength = 0.687f, HeightAboveSea = 50f });
                for (int k = 0; k < 6; k++)
                    SeaDisturbances.Set(new SeaDisturbance { Kind = SeaDisturbanceKind.TentacleRise, X = 90f + 25f * k, Z = 900f + 20f * k, HalfAlong = 9f, HalfAcross = 9f, Strength = 1f, HeightAboveSea = 40f });
                SeaDisturbances.Publish();
                MistVolumeState.Tick(824.0, 64.0, eye);
                Vector4[] c = Shader.GetGlobalVectorArray("_BwMistSprayC"), r = Shader.GetGlobalVectorArray("_BwMistSprayR");
                int slot = -1;
                for (int i = 0; i < MistVolumeState.ActiveSprays; i++) if (Math.Abs(c[i].x - 213.7f) < 0.5f && Math.Abs(c[i].z - 991f) < 0.5f) slot = i;
                Assert.GreaterOrEqual(slot, 0, "the K5 slap column keeps a slot");
                Assert.AreEqual(MistVolumeMath.RhoForTau(MistVolumeMath.SlapTau, Math.Min(r[slot].x, r[slot].z)), c[slot].w, 1e-6, "at full density (no cut-off fade)");
                Assert.AreEqual(MistVolumeMath.SlapUpperK * MistVolumeMath.SlapTop(50.0, 25.0, 1.0), r[slot].y, 1e-3, "the full column (77.5 m semi-axis)");
                // the far Kf slap behind the walker keeps the plain kraken priority
                Assert.AreEqual(MistVolumeMath.KrakenPriority, MistVolumeMath.SlapPriorityFor(1200.0), 1e-12);
                Assert.AreEqual(MistVolumeMath.KrakenPriority * MistVolumeMath.SlapPriority, MistVolumeMath.SlapPriorityFor(270.0), 1e-12);
            }
            finally
            {
                GameState.CaptureMode = cap;
                SeaDisturbances.Clear(); MistVolumeState.ResetTrackers(); WaveImpacts.Clear();
            }
        }

        [Test]
        public void Disturbance_AStillReSnapsToTheBusPublishedAfterTheMistTick()
        {
            // fix wave X2 S3-4: the beat 3 driver ticks the mist before the kraken director publishes the bus (frame order);
            // the pass commits again per camera at the same clock and must then see the slap of THIS pose (x2-show KS11)
            WaveImpacts.Clear(); SeaDisturbances.Clear(); MistVolumeState.ResetTrackers();
            bool cap = GameState.CaptureMode;
            GameState.CaptureMode = true;
            try
            {
                var eye = new Vector3(0.9f, 1.7f, 824f);
                MistVolumeState.Tick(821.0, 328.4, eye);                       // the previous still: no slap
                Assert.AreEqual(0, MistVolumeState.DisturbanceSprays);
                PublishOne(SeaDisturbanceKind.Slap, 213.7f, 991.0f, 25f, 25f, 1f, 50f);
                MistVolumeState.Tick(824.0, 329.6, eye);                       // KS11: ticked with this pose's bus
                Assert.AreEqual(1, MistVolumeState.DisturbanceSprays);
                SeaDisturbances.Clear();                                       // the next still's first tick sees the stale bus...
                PublishOne(SeaDisturbanceKind.Slap, 213.7f, 991.0f, 25f, 25f, 1f, 50f);
                MistVolumeState.Tick(828.0, 331.2, eye);
                SeaDisturbances.Clear();                                       // ...and the director then clears it for this pose
                MistVolumeState.CommitSprays(eye);                             // the pass's commit at the same clock
                Assert.AreEqual(0, MistVolumeState.DisturbanceSprays, "the repeat commit of a still re-snaps to the current bus");
                // a capture sequence (running clock) keeps its followers on a repeat commit: the falling spray stays
                MistVolumeState.ResetTrackers();
                PublishOne(SeaDisturbanceKind.Slap, 213.7f, 991.0f, 25f, 25f, 1f, 50f);
                MistVolumeState.Tick(824.0, 329.6, eye);
                MistVolumeState.Tick(824.04, 329.6 + 1.0 / 30.0, eye);
                SeaDisturbances.Clear();
                MistVolumeState.CommitSprays(eye);
                Assert.AreEqual(1, MistVolumeState.DisturbanceSprays, "no snap on a running clock");
            }
            finally
            {
                GameState.CaptureMode = cap;
                SeaDisturbances.Clear(); MistVolumeState.ResetTrackers(); WaveImpacts.Clear();
            }
        }

        [Test]
        public void Disturbance_TheSlapColumnRisesIn1p5sAndDecaysOver4s()
        {
            // fix wave X2 S3-4 (critic: "rising to 50 m in about 1.5 s and decaying over 4 s"): the show's SlapEnvelope on the bus
            // from the impact (attack 0.2 s, exp(-(dt - 0.2) / 1.6), cut over 3.0-4.2 s; on the bus for 4.8 s), the game clock at
            // 30 fps; the visible top of the column (0.3 V + 0.71 x 0.7 V) and its density per frame
            WaveImpacts.Clear(); SeaDisturbances.Clear(); MistVolumeState.ResetTrackers();
            bool cap = GameState.CaptureMode;
            GameState.CaptureMode = false;
            try
            {
                var eye = new Vector3(0.9f, 1.7f, 824f);
                double v = MistVolumeMath.SlapTop(50.0, 25.0, 1.0), rhoFull = 0.0, t0 = 100.0, prevTop = 0.0, maxStep = 0.0;
                double top05 = 0.0, top15 = 0.0, dens20 = 0.0, dens48 = 0.0, dens60 = 1.0, top80 = -1.0;
                for (int i = -3; i <= 300; i++)
                {
                    double dt = i / 30.0, env = 0.0;
                    if (dt >= 0.0 && dt < 4.8)
                        env = dt < 0.2 ? dt / 0.2 : Math.Exp(-(dt - 0.2) / 1.6) * (1.0 - MistVolumeMath.Smooth((dt - 3.0) / 1.2));
                    PublishOne(SeaDisturbanceKind.Slap, 213.7f, 991.0f, 25f, 25f, (float)env, 50f);
                    MistVolumeState.Tick(822.0 + 2.5 * Math.Max(dt, 0.0), t0 + dt, eye);
                    Vector4[] c = Shader.GetGlobalVectorArray("_BwMistSprayC"), r = Shader.GetGlobalVectorArray("_BwMistSprayR");
                    double top = 0.0, rho = 0.0;
                    for (int k = 0; k < MistVolumeState.ActiveSprays; k++)
                        if (Math.Abs(c[k].x - 213.7f) < 0.5f) { top = (c[k].y - BulwarkConst.SeaY) + 0.71 * r[k].y; rho = c[k].w; rhoFull = MistVolumeMath.RhoForTau(MistVolumeMath.SlapTau, Math.Min(r[k].x, r[k].z)); }
                    double seen = rhoFull > 0.0 ? top * Math.Min(1.0, rho / rhoFull) : 0.0;   // the column x its density
                    maxStep = Math.Max(maxStep, Math.Abs(seen - prevTop));
                    prevTop = seen;
                    if (i == 15) top05 = top;
                    if (i == 45) top15 = top;
                    if (i == 60) dens20 = rhoFull > 0.0 ? rho / rhoFull : 0.0;
                    if (i == 144) dens48 = rhoFull > 0.0 ? rho / rhoFull : 0.0;
                    if (i == 180) dens60 = rhoFull > 0.0 ? rho / rhoFull : 0.0;
                    if (i == 240) top80 = top;
                }
                double vis = 0.3 * v + 0.71 * 0.7 * v;
                Assert.That(top05, Is.InRange(0.4 * vis, 0.8 * vis), "half way up 0.5 s after the impact");
                Assert.GreaterOrEqual(top15, 50.0, "50 m by 1.5 s");
                Assert.GreaterOrEqual(dens20, 0.9, "dense for 2 s (latched)");
                // the latch holds it to 2.1 s, then it thins with SlapFallOmega 1.4 (0.64 at 4 s, 0.33 at 4.8 s, 0.09 at 6 s)
                Assert.Less(dens48, 0.5, "thinning when the slap leaves the bus (4.8 s)");
                Assert.Less(dens60, 0.15, "nearly gone 6 s after the impact (a 4 s decay after the 2 s peak)");
                Assert.Less(top80, 0.2 * vis, "gone 8 s after the impact");
                Assert.Less(maxStep, 0.06 * vis, "no pop: the column x its density changes by at most 6 % per 30 fps frame");
            }
            finally
            {
                GameState.CaptureMode = cap;
                SeaDisturbances.Clear(); MistVolumeState.ResetTrackers(); WaveImpacts.Clear();
            }
        }

        [Test]
        public void Gold_TheSunBreakWindowTintsTheMist()
        {
            // fix wave X2 S3-1: the gold weight is 1 through the sun break (KS08 at s 604), 0 outside s 560-640, continuous
            Assert.AreEqual(1.0, MistVolumeMath.GoldWeight(604.0), 1e-12, "KS08");
            Assert.AreEqual(1.0, MistVolumeMath.GoldWeight(590.0), 1e-12);
            Assert.AreEqual(0.0, MistVolumeMath.GoldWeight(560.0), 1e-12, "before the window");
            Assert.AreEqual(0.0, MistVolumeMath.GoldWeight(492.0), 1e-12, "KS07 under the storm");
            Assert.AreEqual(0.0, MistVolumeMath.GoldWeight(640.0), 1e-12, "after the window");
            double prev = MistVolumeMath.GoldWeight(540.0), maxStep = 0.0;
            for (double s = 540.0; s <= 660.0; s += 0.1)
            {
                double w = MistVolumeMath.GoldWeight(s);
                maxStep = Math.Max(maxStep, Math.Abs(w - prev));
                prev = w;
            }
            Assert.Less(maxStep, 0.01, "no step (0.1 m of walk)");
            MistVolumeState.Tick(604.0, 64.0);
            Assert.AreEqual(1f, Shader.GetGlobalVector("_BwMistGold").x, 1e-6, "the global of the window");
            MistVolumeState.Tick(492.0, 64.0);
            Assert.AreEqual(0f, Shader.GetGlobalVector("_BwMistGold").x, 1e-6);
            // the forward lobe stays within its clamp: at most +0.75 (x pi phase) on top of the 1.5 clamp of the octaves
            Assert.LessOrEqual(Math.Min(MistVolumeMath.GoldLobe * Math.PI * MistVolumeMath.Hg(1.0, MistVolumeMath.GoldLobeG), MistVolumeMath.GoldLobeClamp), 0.75 + 1e-12);
        }

        [Test]
        public void Disturbance_QaWriterOnlyWithItsFlag()
        {
            var keep = CliArgs.Args;
            try
            {
                CliArgs.Override(new[] { "Bulwark.exe" });
                MistVolumeQa.ResetCli();
                Assert.IsFalse(MistVolumeQa.Active, "never in normal play");
                CliArgs.Override(new[] { "Bulwark.exe", "-mist-qa-sea", "static" });
                MistVolumeQa.ResetCli();
                MistVolumeQa.Write(64.0);
                Assert.AreEqual(3, SeaDisturbances.Count, "breach, tentacle, slap");
                Assert.AreEqual(SeaDisturbanceKind.Breach, SeaDisturbances.Get(0).Kind);
                CliArgs.Override(new[] { "Bulwark.exe", "-mist-qa-sea", "cycle" });
                MistVolumeQa.ResetCli();
                MistVolumeQa.Write(MistVolumeQa.DefaultT0 + 5.0);
                Assert.AreEqual(1f, SeaDisturbances.Get(0).Strength, 1e-4, "breach held at 5 s");
                Assert.AreEqual(1f, SeaDisturbances.Get(2).Strength, 1e-4, "slap at 5 s");
                MistVolumeQa.Write(MistVolumeQa.DefaultT0 + 18.0);
                Assert.AreEqual(1, SeaDisturbances.Count, "at 18 s only the tentacle (zero strengths are skipped)");
                Assert.AreEqual(SeaDisturbanceKind.TentacleRise, SeaDisturbances.Get(0).Kind);
                Assert.AreEqual(0.0, MistVolumeQa.ForcedCascade, 1e-12, "no forced cascade by default");
                CliArgs.Override(new[] { "Bulwark.exe", "-mist-qa-sea", "kall", "-mist-qa-cascade", "0.5" });
                MistVolumeQa.ResetCli();
                MistVolumeQa.Write(64.0);
                Assert.AreEqual(7, SeaDisturbances.Count, "K1, K2, K4 (breach + 3 arms), K5");
                Assert.AreEqual(0.5, MistVolumeQa.ForcedCascade, 1e-12);
                Assert.AreEqual(1400f, SeaDisturbances.Get(0).X, 1e-3, "K1 at (1400, 600)");
                Assert.AreEqual(180f, SeaDisturbances.Get(1).HeightAboveSea, 1e-3, "K2 arm 180 m");
                Assert.AreEqual(SeaDisturbanceKind.Slap, SeaDisturbances.Get(6).Kind, "K5 slap");
            }
            finally
            {
                CliArgs.Override(keep);
                MistVolumeQa.ResetCli();
                SeaDisturbances.Clear();
            }
        }

        // ------------------------------------------------------------------ HLSL twin
        [Test]
        public void Hlsl_ConstantsMatchTheTwin()
        {
            string path = Path.Combine(Root, "Assets/_Game/Shaders/Include/BwMistVolume.hlsl");
            Assert.IsTrue(File.Exists(path), path);
            var defs = new Dictionary<string, double>();
            var rx = new Regex(@"^\s*#define\s+(MIST_[A-Z0-9_]+)\s+\(?\s*(-?[0-9.]+)\s*\)?");
            foreach (var line in File.ReadAllLines(path))
            {
                var m = rx.Match(line);
                if (m.Success) defs[m.Groups[1].Value] = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            }
            var expect = new (string name, double value)[]
            {
                ("MIST_SIGMA", MistVolumeMath.Sigma), ("MIST_SAMPLES", MistVolumeMath.Samples), ("MIST_SPACING", MistVolumeMath.SpacingPower),
                ("MIST_FBM_BASE", MistVolumeMath.FbmBase), ("MIST_FBM_AMP", MistVolumeMath.FbmAmp),
                ("MIST_FBM_TILE", MistVolumeMath.FbmScale * MistVolumeNoise.BaseCells),
                ("MIST_PLUME_Z", MistVolumeMath.PlumeZ), ("MIST_PLUME_SIGMA_Z", MistVolumeMath.PlumeSigmaZ), ("MIST_PLUME_H", MistVolumeMath.PlumeScaleH),
                ("MIST_PLUME_X_FACE", MistVolumeMath.PlumeXFace), ("MIST_PLUME_X_FAR", MistVolumeMath.PlumeXFar),
                ("MIST_PLUME_FACE_FADE", MistVolumeMath.PlumeFaceFade), ("MIST_PLUME_FAR_FADE", MistVolumeMath.PlumeFarFade), ("MIST_PLUME_X_SCALE", MistVolumeMath.PlumeXScale),
                ("MIST_PLUME_Y_MIN", MistVolumeMath.PlumeYMin), ("MIST_PLUME_Y_MAX", MistVolumeMath.PlumeYMax), ("MIST_PLUME_Z_HALF", MistVolumeMath.PlumeZHalf),
                ("MIST_VEIL_WIDTH", MistVolumeMath.VeilWidth), ("MIST_VEIL_TOP", MistVolumeMath.VeilTop), ("MIST_VEIL_TOP_FADE", MistVolumeMath.VeilTopFade),
                ("MIST_VEIL_Z_HALF", MistVolumeMath.VeilZHalf), ("MIST_VEIL_Z_FADE", MistVolumeMath.VeilZFade), ("MIST_VEIL_X_FADE", MistVolumeMath.VeilXFade), ("MIST_VEIL_FACE_FADE", MistVolumeMath.VeilFaceFade),
                ("MIST_LIP_X", MistVolumeMath.LipX), ("MIST_LIP_Y", MistVolumeMath.LipY), ("MIST_LIP_SPEED", MistVolumeMath.LipSpeed),
                ("MIST_GRAVITY_HALF", BulwarkConst.SheetGravityHalf),
                ("MIST_CURTAIN_D0", MistVolumeMath.CurtainD0), ("MIST_CURTAIN_FADE_IN0", MistVolumeMath.CurtainFadeIn0), ("MIST_CURTAIN_FADE_IN1", MistVolumeMath.CurtainFadeIn1),
                ("MIST_CURTAIN_R0", MistVolumeMath.CurtainR0), ("MIST_CURTAIN_D1", MistVolumeMath.CurtainD1), ("MIST_CURTAIN_R_EXP", MistVolumeMath.CurtainRExp),
                ("MIST_CURTAIN_WATER_TAU", MistVolumeMath.CurtainWaterTau), ("MIST_CURTAIN_MIST_TAU", MistVolumeMath.CurtainMistTau),
                ("MIST_CURTAIN_END0", MistVolumeMath.CurtainEnd0), ("MIST_CURTAIN_END1", MistVolumeMath.CurtainEnd1),
                ("MIST_SLOT_Z0", BulwarkConst.SlotZ0), ("MIST_SLOT_PITCH", BulwarkConst.PierPitch), ("MIST_SLOT_LEN", BulwarkConst.SlotLength), ("MIST_SLOT_COUNT", BulwarkConst.SlotCount),
                ("MIST_HG_G", MistVolumeMath.HgG), ("MIST_PHASE_CLAMP", MistVolumeMath.PhaseClamp),
                ("MIST_GOLD_SUN", MistVolumeMath.GoldSun), ("MIST_GOLD_AMB", MistVolumeMath.GoldAmb), ("MIST_GOLD_LOBE", MistVolumeMath.GoldLobe),
                ("MIST_GOLD_LOBE_G", MistVolumeMath.GoldLobeG), ("MIST_GOLD_LOBE_CLAMP", MistVolumeMath.GoldLobeClamp),
                ("MIST_BOW_WIDTH", MistVolumeMath.BowWidth), ("MIST_BOW_SECONDARY", MistVolumeMath.BowSecondaryWeight), ("MIST_BOW_CLAMP", MistVolumeMath.BowClamp),
                ("MIST_BAND", MistVolumeMath.DarkBand), ("MIST_BAND_IN", MistVolumeMath.DarkBandIn), ("MIST_BAND_OUT", MistVolumeMath.DarkBandOut), ("MIST_BAND_EDGE", MistVolumeMath.DarkBandEdge),
                ("MIST_SPRAYS", MistVolumeState.SpraySlots), ("MIST_ALBEDO", MistVolumeMath.Albedo), ("MIST_SPRAY_BASE", MistVolumeMath.SprayBillowBase),
                ("MIST_SPRAY_ALBEDO", MistVolumeMath.SprayAlbedo), ("MIST_SPRAY_WHITE", MistVolumeMath.SprayWhite),
                ("MIST_SPRAY_WHITE_TAU", MistVolumeMath.SprayWhiteTau), ("MIST_SPRAY_NUP", MistVolumeMath.SprayNormalUp),
                ("MIST_SPRAY_ERODE", MistVolumeMath.SprayErode), ("MIST_SPRAY_ERODE_IN", MistVolumeMath.SprayErodeIn), ("MIST_SPRAY_ERODE_SOFT", MistVolumeMath.SprayErodeSoft), ("MIST_SPRAY_ERODE_TOP", MistVolumeMath.SprayErodeTop), ("MIST_SPRAY_NEAR0", MistVolumeMath.SprayNear0), ("MIST_SPRAY_NEAR1", MistVolumeMath.SprayNear1),
                ("MIST_DAM_CREST_Y", MistVolumeMath.DamCrestY), ("MIST_DAM_SOFT0", MistVolumeMath.DamSoft0), ("MIST_SUN_PENUMBRA", MistVolumeMath.SunPenumbra),
                ("MIST_ONSET_BASE", MistVolumeMath.OnsetBase), ("MIST_ONSET_TOP", MistVolumeMath.OnsetTop),
                ("MIST_ONSET_Y0", MistVolumeMath.OnsetY0), ("MIST_ONSET_Y1", MistVolumeMath.OnsetY1), ("MIST_STORM_KEY", MistVolumeMath.StormKey),
            };
            var bad = new List<string>();
            foreach (var e in expect)
            {
                if (!defs.TryGetValue(e.name, out double v)) { bad.Add(e.name + " missing"); continue; }
                double tol = e.name == "MIST_CURTAIN_R_EXP" ? 1e-5 : 1e-9;
                if (Math.Abs(v - e.value) > tol) bad.Add(e.name + " = " + v.ToString(CultureInfo.InvariantCulture) + ", C# " + e.value.ToString(CultureInfo.InvariantCulture));
            }
            // the bow centres are literal vectors in the shader: compare them as text
            string text = File.ReadAllText(path);
            if (text.IndexOf("float3(42.0, 41.0, 40.3)", StringComparison.Ordinal) < 0) bad.Add("primary bow centres (42.0, 41.0, 40.3) not found");
            if (text.IndexOf("float3(50.8, 52.0, 53.5)", StringComparison.Ordinal) < 0) bad.Add("secondary bow centres (50.8, 52.0, 53.5) not found");
            Assert.IsEmpty(bad, string.Join("\n", bad));
        }
    }
}
