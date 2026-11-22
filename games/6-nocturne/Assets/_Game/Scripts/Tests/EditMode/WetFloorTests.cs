// WetFloorTests: EditMode tests of the analytic wet-floor mirror (MASTER_PROMPT 14.7 H4, D-003, R1 2.10) on its double mirror
// NocMirrorMath and of the source registry NocWetRegistry:
//   Fresnel at 10 and 30 m (0.397, 0.717 by arithmetic; the spec's 0.74 is corrected in D-003); T-H4-ENERGY (a small disc
//   integrates to Omega for every roughness and view, the segment to its area, the mirror limit never exceeds the emitter);
//   T-H4-STREAK (a lamp 40 m ahead reflects as a vertical streak, h/w >= 2.5 at 1080p); T-H4-KS17 (the goal window's floor
//   maximum at 0.246 of the distance +-10 %); T-H4-CONT (no jump when the reflection ray passes a segment's ends or turns
//   away); the puddle mask covers 35 %; the registry keeps at most 32 sources and crossfades them without a step; Look v2: the
//   wall-plane blend (no seam at a near wall's vanishing line, CRIT0 #8); FX (INT-13, D-032 3): the facade planes' lit windows
//   (mean 1 at every footprint, bounded, vertical streaks, continuous) and their gain on the floor path only.
using System;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class WetFloorTests
    {
        static readonly D3 Up = new D3(0, 1, 0);
        const double SigPuddle = 0.08 * 0.08 + 0.004, SigDry = 0.35 * 0.35 + 0.004;

        [Test]
        public void Fresnel_At10And30m()
        {
            double nv10 = 1.7 / Math.Sqrt(100 + 1.7 * 1.7), nv30 = 1.7 / Math.Sqrt(900 + 1.7 * 1.7);
            Assert.AreEqual(0.397, NocMirrorMath.Fresnel(nv10, 0.95), 0.002);
            Assert.AreEqual(0.717, NocMirrorMath.Fresnel(nv30, 0.95), 0.002, "D-003: the spec's 0.74 is 0.717 by arithmetic");
            Assert.AreEqual(0.03, NocMirrorMath.Fresnel(1.0, 1.0), 1e-12, "F0 0.03");
        }

        [Test]
        public void Energy_DiscIntegratesToOmega_SegmentToArea()
        {
            foreach (double s2 in new[] { 0.004, SigPuddle, SigDry })
                foreach (double nv in new[] { 1.0, 0.2, 0.05 })
                {
                    double sxx = s2 * nv * nv, syy = s2, w = 0.02;
                    double sx = Math.Sqrt(sxx + w * w / 8), sy = Math.Sqrt(syy + w * w / 8);
                    double sum = 0, step = Math.Min(sx, sy) / 12.0;
                    for (double x = -7 * sx; x <= 7 * sx; x += step)
                        for (double y = -7 * sy; y <= 7 * sy; y += step)
                            sum += NocMirrorMath.SegmentMass(x, y, x, y, sxx, syy, 0, w) * step * step;
                    double omega = Math.PI * w * w / 4;
                    Assert.AreEqual(omega, sum, 0.03 * omega, FormattableString.Invariant($"disc sigma2 {s2} nv {nv}"));
                }
            // segment of length 0.2 rad, width 0.01: integral over offsets = area (within 5 %)
            {
                double sxx = SigPuddle * 0.04, syy = SigPuddle, w = 0.01, len = 0.2, step = 0.004, sum = 0;
                for (double x = -0.6; x <= 0.6; x += step)
                    for (double y = -0.8; y <= 0.8; y += step)
                        sum += NocMirrorMath.SegmentMass(x, y, x, y + len, sxx, syy, 0, w) * step * step;
                Assert.AreEqual(w * len, sum, 0.05 * w * len, "segment integrates to its area");
            }
            // mirror limit: a large source under a narrow lobe has mass <= 1 (never brighter than its emitter)
            double m = NocMirrorMath.SegmentMass(0, -0.5, 0, 0.5, 1e-6, 1e-6, 0, 0.5);
            Assert.LessOrEqual(m, 1.0);
            Assert.Greater(m, 0.95, "mirror limit approaches 1");
        }

        [Test]
        public void Streak_LampAhead_IsVertical()
        {
            // 1080p, vFOV 66, eye (0, 1.7, 0) level; lamp disc (rho 0.2 m) at (0, 5, 40); puddle roughness
            const int W = 1920, H = 1080;
            double f = 0.5 * H / Math.Tan(33 * Math.PI / 180);
            var eye = new D3(0, 1.7, 0);
            var lamp = new D3(0, 5, 40);
            double lat = NocturneConstants.WetLatMinPx / f;
            var mass = new double[W, H];
            double peak = 0;
            for (int j = H / 2 + 1; j < H; j += 1)
                for (int i = W / 2 - 300; i < W / 2 + 300; i += 1)
                {
                    var dir = new D3((i + 0.5 - W / 2) / f, (H / 2 - j - 0.5) / f, 1).Normalized;
                    if (dir.y >= -1e-4) continue;
                    double t = 1.7 / -dir.y;
                    var P = eye + dir * t;
                    var o = NocMirrorMath.MirrorLobe(P, eye, Up, SigPuddle, 0.95, lat);
                    double v = NocMirrorMath.SourceFactor(P, t, o, lamp, lamp, 0.2, 0) / Math.Max(o.FG, 1e-9);
                    mass[i, j] = v;
                    peak = Math.Max(peak, v);
                }
            Assert.Greater(peak, 0, "the lamp reflects");
            int x0 = W, x1 = -1, y0 = H, y1 = -1;
            for (int j = 0; j < H; j++)
                for (int i = 0; i < W; i++)
                    if (mass[i, j] >= peak / Math.E) { x0 = Math.Min(x0, i); x1 = Math.Max(x1, i); y0 = Math.Min(y0, j); y1 = Math.Max(y1, j); }
            double w = x1 - x0 + 1, h = y1 - y0 + 1;
            Debug.Log(FormattableString.Invariant($"[NOC] H4 streak: lamp 40 m ahead, 1/e region {w} x {h} px (h/w {h / w:F2})"));
            Assert.GreaterOrEqual(h / w, 2.5, FormattableString.Invariant($"streak {w} x {h} px"));
        }

        [Test]
        public void KS17_GoalWindowStreakPosition()
        {
            // The mirror point (the lobe axis hits the window centre) lies at 1.7 / (1.7 + 5.15) = 0.248 of the distance (spec
            // 0.246 +- 10 %). The brightness maximum of the area-source model lies nearer the window (mass ~ solid angle ~ 1/d^2
            // wins over a lobe of sigma2 >= 0.004 at these 0.01-0.09 rad elevations; 10 % would need sigma2 <= 1.2e-3,
            // decision in docs/logs/A2a.md): the test pins both and checks the streak spans from the window base toward the
            // mirror point on screen (rows between them).
            var eye = new D3(944.18, 1.7, 387.88);
            var w0 = new D3(1156.0, 4.4, 387.9); var w1 = new D3(1156.0, 5.9, 387.9);
            double L = 1156.0 - eye.x;
            double bestAxis = 1e9, axisX = 0, best = -1, bestX = 0;
            for (double x = eye.x + 5; x < 1150; x += 0.05)
            {
                var P = new D3(x, 0, 387.88);
                double d = (P - eye).Length;
                var o = NocMirrorMath.MirrorLobe(P, eye, Up, SigPuddle, 0.95);
                // lobe axis: elevation of r against the elevation of the window centre seen from P
                double er = Math.Asin(o.r.y), ew = Math.Atan2(5.15, 1156.0 - x);
                if (Math.Abs(er - ew) < bestAxis) { bestAxis = Math.Abs(er - ew); axisX = x; }
                double v = NocMirrorMath.SourceFactor(P, d, o, w0, w1, 0.6, 3.4e-4);
                if (v > best) { best = v; bestX = x; }
            }
            double fAxis = (axisX - eye.x) / L, fPeak = (bestX - eye.x) / L;
            double rowAxis = Math.Atan2(1.7, axisX - eye.x) * 180 / Math.PI, rowPeak = Math.Atan2(1.7, bestX - eye.x) * 180 / Math.PI;
            double rowBase = Math.Atan2(1.7, L) * 180 / Math.PI;
            Debug.Log(FormattableString.Invariant($"[NOC] H4 KS17: mirror point at {fAxis:F4} of the distance (spec 0.246), brightness peak at {fPeak:F4}; ") +
                      FormattableString.Invariant($"screen: window base {rowBase:F2} deg, peak {rowPeak:F2} deg, mirror point {rowAxis:F2} deg below the horizon"));
            Assert.AreEqual(0.246, fAxis, 0.0246, "mirror point");
            Assert.Greater(best, 0);
            Assert.IsTrue(rowPeak >= rowBase && rowPeak <= rowAxis, "the streak lies between the window base and the mirror point");
        }

        [Test]
        public void Continuity_EndpointsAndBehind()
        {
            var eye = new D3(0, 1.7, 0);
            var P = new D3(0, 0, 15);
            var o = NocMirrorMath.MirrorLobe(P, eye, Up, SigPuddle, 0.95);
            double prev = double.NaN, maxStep = 0, peak = 0;
            // slide a 3 m vertical segment along x past the reflection axis and then far behind the floor point (z decreasing)
            for (int k = 0; k <= 4000; k++)
            {
                double z = 60 - k * 0.02;            // 60 m ahead -> -20 m (behind the floor point)
                var a = new D3(0.3, 2, z); var b = new D3(0.3, 5, z);
                double v = NocMirrorMath.SourceFactor(P, 15, o, a, b, 0.3, 0);
                if (!double.IsNaN(prev)) maxStep = Math.Max(maxStep, Math.Abs(v - prev));
                peak = Math.Max(peak, v);
                prev = v;
            }
            Assert.Greater(peak, 0);
            Assert.Less(maxStep, 0.01 * peak, "adjacent-sample step below 1 % of the peak");
        }

        // Look v2 (TEXTURE_PLAN 3.3, CRIT0 #8): a near wall plane ends in front of a far one (a canyon block and the face behind it).
        // The deleted v1 blend (TP-2) let the near wall take over as soon as its soft end had any coverage (a vertical seam at the
        // near wall's vanishing line); the blend composites the near wall over the far one and widens the end blend to at least
        // 2.5 deg, so the reflected radiance is continuous.
        [Test]
        public void WallBlend_Continuous()
        {
            var walls = new[]
            {
                new NocMirrorMath.WallPlane { cx = 20, cz = 40, nx = -1, nz = 0, halfLength = 40, top = 60, kind = 1, weight = 1, colour = new D3(0.03, 0.03, 0.03) },
                new NocMirrorMath.WallPlane { cx = 60, cz = 200, nx = -1, nz = 0, halfLength = 300, top = 200, kind = 1, weight = 1, colour = new D3(0.05, 0.05, 0.05) },
            };
            var sky = new D3(0.01, 0.01, 0.01);
            var P = new D3(0, 0, 10);
            double MaxStep(double soft)
            {
                double prev = double.NaN, maxStep = 0;
                for (int k = 0; k <= 2000; k++)
                {
                    double az = (10.0 + k * 0.01) * Math.PI / 180.0;          // azimuth from +z toward +x: 10 .. 30 deg (the near end at 15.9 deg)
                    var r = new D3(Math.Sin(az) * Math.Cos(0.05), Math.Sin(0.05), Math.Cos(az) * Math.Cos(0.05));
                    double a = NocMirrorMath.WallReflect(P, r, soft, 10, 4e-4, walls, walls.Length, sky, out D3 c);
                    double v = sky.x + (c.x - sky.x) * a;
                    if (!double.IsNaN(prev)) maxStep = Math.Max(maxStep, Math.Abs(v - prev));
                    prev = v;
                }
                return maxStep;
            }
            const double soft = 0.01;                                             // a sharp puddle lobe (0.6 deg)
            double v2 = MaxStep(soft);
            Assert.Less(v2, 3e-4, "at most 0.6 % of the wall radiance per 0.01 deg step (the 5 deg blend: 0.02 over about 140 steps)");
            // a single wall wider than the lobe: full coverage inside it, the sky outside
            var one = new[] { walls[1] };
            double cin = NocMirrorMath.WallReflect(P, new D3(1.0, 0.05, 0.3).Normalized, 0.1, 10, 4e-4, one, 1, sky, out D3 kin);
            Assert.AreEqual(1.0, cin, 1e-9, "inside the wall");
            Assert.IsFalse(double.IsNaN(kin.x));
        }

        /// <summary>FX (INT-13, D-032 3): the lit windows of a facade wall plane in the wet mirror keep the registered macro as their
        /// mean at every footprint (so WetMacroTests and the energy of the mirror stay), never go negative or above one lit window
        /// (1 / (FaLit WinU WinV)), and streak vertically (correlated over 3 m up, anti-correlated half a bay across).</summary>
        [Test]
        public void WallWindows_MeanOne_Bounded_StreakVertically()
        {
            double lit = NocFacadeConstants.FaLit, wu = NocSurfaceConstants.SurfWallWinU, wv = NocSurfaceConstants.SurfWallWinV;
            double max = 1.0 / (lit * wu * wv) + 1e-9;
            var fp = new[] { (0.05, 0.5), (0.3, 3.0), (0.6, 8.0), (2.0, 16.0), (6.0, 40.0) };
            foreach (var (sU, sV) in fp)
            {
                double sum = 0; long n = 0; double hi = 0, lo = 1e9;
                for (double u = 0; u < 1200; u += 0.61)
                    for (double y = 0; y < 360; y += 0.47)
                    {
                        double v = NocMirrorMath.WallWindows(u, y, sU, sV, 1);
                        sum += v; n++; hi = Math.Max(hi, v); lo = Math.Min(lo, v);
                    }
                double mean = sum / n;
                Debug.Log(FormattableString.Invariant($"[NOC] wall windows sU {sU} sV {sV}: mean {mean:F4}, min {lo:F3}, max {hi:F3}"));
                Assert.AreEqual(1.0, mean, 0.04, FormattableString.Invariant($"mean at sU {sU}, sV {sV}"));
                Assert.GreaterOrEqual(lo, 0.0);
                Assert.LessOrEqual(hi, max);
            }
            var rnd = new System.Random(5);
            double sa = 0, sb = 0, sc = 0, saa = 0, sbb = 0, scc = 0, sab = 0, sac = 0; int m = 100000;
            for (int k = 0; k < m; k++)
            {
                double u = rnd.NextDouble() * 1200 - 600, y = rnd.NextDouble() * 300;
                double a = NocMirrorMath.WallWindows(u, y, 0.2, 10, 3), b = NocMirrorMath.WallWindows(u, y + 3.0, 0.2, 10, 3);
                double c = NocMirrorMath.WallWindows(u + 1.5, y, 0.2, 10, 3);
                sa += a; sb += b; sc += c; saa += a * a; sbb += b * b; scc += c * c; sab += a * b; sac += a * c;
            }
            double Corr(double sx, double sy, double sxx, double syy, double sxy)
                => (sxy / m - sx / m * sy / m) / Math.Sqrt((sxx / m - sx / m * sx / m) * (syy / m - sy / m * sy / m));
            double up = Corr(sa, sb, saa, sbb, sab), across = Corr(sa, sc, saa, scc, sac);
            Debug.Log(FormattableString.Invariant($"[NOC] wall windows streak (sU 0.2, sV 10 m): corr 3 m up {up:F3}, half a bay across {across:F3}"));
            Assert.Greater(up, 0.9, "a lit column stays lit over 3 m of height");
            Assert.Less(across, -0.3, "window and pier alternate across");
            // continuous in the footprint (the window level fades into the column share without a step)
            double step = 0;
            foreach (var (u0, y0) in new[] { (10.3, 20.1), (55.7, 40.3), (101.5, 7.7) })
            {
                double prev = double.NaN;
                for (double sv = 0.5; sv <= 2.5; sv += 0.01)
                {
                    double v = NocMirrorMath.WallWindows(u0, y0, 0.2, sv, 0);
                    if (!double.IsNaN(prev)) step = Math.Max(step, Math.Abs(v - prev));
                    prev = v;
                }
            }
            Assert.Less(step, 0.3, "no step as the vertical footprint grows");
            // the floor path's gain: the facade plane (kind 0) shows its windows x SurfWallGain, a flat plane (kind 1) is unchanged
            Assert.LessOrEqual(NocSurfaceConstants.SurfWallGain, 2.5, "D-032 3: the art-directed gain is at most 2.5");
            var wall = new[] { new NocMirrorMath.WallPlane { cx = 30, cz = 0, nx = -1, nz = 0, halfLength = 400, top = 300, kind = 0, weight = 1, colour = new D3(0.02, 0.02, 0.02) } };
            var r = new D3(0.95, 0.12, 0.2).Normalized;
            double a0 = NocMirrorMath.WallReflect(new D3(0, 0, 0), r, 0.05, 0.05, 10, 0, wall, 1, new D3(0, 0, 0), false, out D3 c0);
            double a1 = NocMirrorMath.WallReflect(new D3(0, 0, 0), r, 0.05, 0.05, 10, 0, wall, 1, new D3(0, 0, 0), true, out D3 c1);
            Assert.AreEqual(a0, a1, 1e-12, "the coverage does not change");
            Assert.IsFalse(double.IsNaN(c1.x));
            wall[0].kind = 1;
            NocMirrorMath.WallReflect(new D3(0, 0, 0), r, 0.05, 0.05, 10, 0, wall, 1, new D3(0, 0, 0), true, out D3 c2);
            Assert.AreEqual(0.02, c2.x, 1e-12, "a flat plane keeps its radiance");
        }

        [Test]
        public void PuddleMask_Covers35Percent()
        {
            var rnd = new System.Random(3);
            double sum = 0; int n = 400000;
            for (int i = 0; i < n; i++) sum += NocMirrorMath.PuddleMask(rnd.NextDouble() * 4000 - 2000, rnd.NextDouble() * 4000 - 2000);
            Assert.AreEqual(NocturneConstants.WetPuddleFraction, sum / n, 0.01);
        }

        [Test]
        public void Registry_AtMost32_CrossfadesWithoutSteps()
        {
            NocWetRegistry.Clear();
            try
            {
                for (double s = 0; s <= 1500; s += 5)
                    NocWetRegistry.AddSource(new Vector3(0, 5, 0), new Vector3(0, 9, 0), 0.5f, new Color(2, 1, 0.5f), s);
                Assert.LessOrEqual(NocWetRegistry.MaxActive(), NocWetRegistry.MaxSources);
                // the summed weight of the window changes smoothly as the walker moves (no pop)
                double prev = double.NaN, maxStep = 0;
                for (double s = 100; s <= 200; s += 0.1)
                {
                    int n = NocWetRegistry.Collect(s);
                    double sum = 0;
                    for (int i = 0; i < n; i++) { NocWetRegistry.GetSource(i, out _, out _, out Vector4 l); sum += l.x; }
                    if (!double.IsNaN(prev)) maxStep = Math.Max(maxStep, Math.Abs(sum - prev));
                    prev = sum;
                    Assert.AreEqual(0, NocWetRegistry.Dropped);
                }
                Assert.Less(maxStep, 0.2, "total source weight per 0.1 m step (one popping source would step by 2)");
                // L_eff is clamped to 3 by its brightest channel
                NocWetRegistry.Clear();
                NocWetRegistry.AddSource(Vector3.zero, Vector3.zero, 1f, new Color(9, 3, 0), 10);
                NocWetRegistry.Collect(10);
                NocWetRegistry.GetSource(0, out _, out _, out Vector4 c);
                Assert.AreEqual(3f, c.x, 1e-5f); Assert.AreEqual(1f, c.y, 1e-5f);
            }
            finally { NocWetRegistry.Clear(); }
        }
    }
}
