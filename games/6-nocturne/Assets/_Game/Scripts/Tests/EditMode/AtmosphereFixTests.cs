// AtmosphereFixTests (AT, fix wave after panel p4_01, 2026-11-21): EditMode tests of the four atmosphere fixes on the double mirror
// NocFogMath (NocAtmosphere.hlsl's twins) and the AtmosphereDriver's flash schedule:
//   C11 the under-city glow pools give the KS10 drop lateral structure (row std over x 800-940) that the pool-free drop lacks, and
//       keep the mean of the depths (multiplier mean within 10 %); the sink veil leaves geometry above y -40 untouched, halves T at
//       y -400 or deeper, stays monotone along downward rays and is cut with the sheets;
//   C02-air the koi's in-scatter equals the numeric line integral of its 12 Gaussian glows, is zero outside beat 7, and lifts the
//       air 50-80 m beside the body by more than the air 400 m away;
//   C26 the sheet lightning: envelope (rise 0.4 s, decay 1.5 s, peak 1), at most +15 % of the deck, the times come from
//       DreadScore.LightningS (none in beats 1 and 5, >= 62.5 m apart), each centre within 1 km of the walker, the hull's ellipse
//       keeps <= 0.1 of the flash (it outlines the hull), and a zero amplitude is the identity;
//   C44 the alley lift is 1 at (0, 80), < 0.2 % at the KS04 / KS05 lid centres and < 2.5 % at the boulevard's hot core.
using System;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class AtmosphereFixTests
    {
        static double Luma(C3 lin, double ev)
        {
            var c = NocFogMath.Floor(lin);
            NocGradeMath.Display(c.r, c.g, c.b, ev, 0.0, 0.0, 1.0, out double r, out double g, out double b);
            return 255.0 * NocMath.Luma709(NocturnePalette.LinearToSrgb(r), NocturnePalette.LinearToSrgb(g), NocturnePalette.LinearToSrgb(b));
        }

        static void Ray(int px, int py, int W, int H, double yawDeg, double pitchDeg, double fovDeg, out double dx, out double dy, out double dz)
        {
            double yaw = yawDeg * Math.PI / 180, pitch = pitchDeg * Math.PI / 180;
            double f = 0.5 * H / Math.Tan(fovDeg * Math.PI / 360);
            double sx = (px + 0.5 - W * 0.5) / f, sy = (H * 0.5 - py - 0.5) / f;
            double fx = Math.Sin(yaw) * Math.Cos(pitch), fy = Math.Sin(pitch), fz = Math.Cos(yaw) * Math.Cos(pitch);
            double rx = Math.Cos(yaw), rz = -Math.Sin(yaw);
            double ux = fy * rz, uy = fz * rx - fx * rz, uz = -fy * rx;
            dx = fx + sx * rx + sy * ux; dy = fy + sy * uy; dz = fz + sx * rz + sy * uz;
            double l = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            dx /= l; dy /= l; dz /= l;
        }

        static NocAtmoState ShotState(string tag, out KeyShot k)
        {
            KeyShot kk = Array.Find(NocturneConstants.KeyShots, q => q.tag == tag);
            k = kk;
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, kk.s, kk.s / NocturneConstants.CinematicSpeed);
            st.CamX = kk.x; st.CamY = kk.y; st.CamZ = kk.z;
            st.PixelAngle = 2.0 * Math.Tan(kk.fov * Math.PI / 360) / 1080.0;
            return st;
        }

        static double RowStdMedian(NocAtmoState st, KeyShot k, double eMax, out int rows)
        {
            var stds = new System.Collections.Generic.List<double>();
            for (int py = 0; py < 1080; py += 6)
            {
                Ray(870, py, 1920, 1080, k.yaw, k.pitch, k.fov, out _, out double cy, out _);
                if (Math.Asin(cy) * 180 / Math.PI > eMax) continue;
                double s = 0, s2 = 0; int n = 0;
                for (int px = 800; px < 940; px += 2)
                {
                    Ray(px, py, 1920, 1080, k.yaw, k.pitch, k.fov, out double dx, out double dy, out double dz);
                    double y = Luma(NocFogMath.SkyColor(st, dx, dy, dz), 0.0);
                    s += y; s2 += y * y; n++;
                }
                double m = s / n;
                stds.Add(Math.Sqrt(Math.Max(s2 / n - m * m, 0.0)));
            }
            stds.Sort();
            rows = stds.Count;
            return stds.Count > 0 ? stds[stds.Count / 2] : 0.0;
        }

        // ---------------------------------------------------------------- C11 pools
        [Test]
        public void Pools_GiveTheKS10DropLateralStructure()
        {
            var st = ShotState("KS10_edge_plunge", out KeyShot k);
            double withP = RowStdMedian(st, k, -20.0, out int rows);
            st.PoolAmp = 0.0;
            double without = RowStdMedian(st, k, -20.0, out _);
            string msg = FormattableString.Invariant($"KS10 drop x 800-940, e <= -20 ({rows} rows): median row std {withP:F2} codes with pools, {without:F2} without");
            UnityEngine.Debug.Log("[NOC] " + msg);
            Assert.Greater(rows, 20, msg);
            Assert.GreaterOrEqual(withP, without + 1.5, msg);
        }

        [Test]
        public void Pools_KeepTheMeanOfTheDepths()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 548, 219.2);
            st.CamX = 363.3; st.CamZ = 195;
            var rnd = new Random(5);
            double sr = 0, sg = 0, sb = 0; int n = 0;
            for (int i = 0; i < 6000; i++)
            {
                double e = -(25 + 50 * rnd.NextDouble()), a = 2 * Math.PI * rnd.NextDouble();
                double dx = Math.Cos(e * Math.PI / 180) * Math.Sin(a), dy = Math.Sin(e * Math.PI / 180), dz = Math.Cos(e * Math.PI / 180) * Math.Cos(a);
                st.PoolAmp = 0.0;
                var c0 = NocFogMath.Abyss(st, new C3(1, 1, 1), dx, dy, dz, e, 1e-4);
                st.PoolAmp = NocAir.PoolAmp;
                var c1 = NocFogMath.Abyss(st, new C3(1, 1, 1), dx, dy, dz, e, 1e-4);
                sr += c1.r / c0.r; sg += c1.g / c0.g; sb += c1.b / c0.b; n++;
            }
            string msg = FormattableString.Invariant($"pool multiplier mean r {sr / n:F3} g {sg / n:F3} b {sb / n:F3}");
            UnityEngine.Debug.Log("[NOC] " + msg);
            Assert.That(sg / n, Is.InRange(0.9, 1.1), msg);
            Assert.That(sr / n, Is.InRange(0.9, 1.15), msg);
            Assert.That(sb / n, Is.InRange(0.85, 1.1), msg);
        }

        // ---------------------------------------------------------------- C11 sink veil
        [Test]
        public void Sink_VeilOnlyBelowTheWalk_MonotoneAndHalfAtDepth()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 600, 240);
            st.CamX = 415.3; st.CamZ = 195;
            Assert.AreEqual(0.0, NocFogMath.SinkVeil(st, -40.0), 1e-12, "nothing above y -40");
            Assert.AreEqual(0.0, NocFogMath.SinkVeil(st, 1.7), 1e-12);
            Assert.AreEqual(NocAir.SinkV, NocFogMath.SinkVeil(st, -400.0), 1e-12, "full veil below y -400");
            Assert.AreEqual(NocFogMath.FogT(st, 815.3, -30, 300), NocFogMath.FogTV(st, 815.3, -30, 300), 1e-12, "geometry above y -40 is untouched");
            double dh = Math.Sqrt(500.0 * 500.0 - 401.7 * 401.7);
            double off = NocFogMath.FogT(st, st.CamX + dh, -400, 195), on = NocFogMath.FogTV(st, st.CamX + dh, -400, 195);
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] face y -400 at 500 m: T {off:F3} -> {on:F3}"));
            Assert.LessOrEqual(on, 0.55, "the strata sink into the haze (about half by y -400)");
            // monotone along downward rays (T falls, the veil rises), the sheets drifting
            for (int k = 0; k < 90; k++)
            {
                double el = -1 - 88.0 * k / 89.0, e = el * Math.PI / 180, prev = 1.0;
                for (double d = 5; d <= 6000; d += 5)
                {
                    double t = NocFogMath.FogTV(st, st.CamX + d * Math.Cos(e), 1.7 + d * Math.Sin(e), 195);
                    Assert.LessOrEqual(t, prev + 1e-12, FormattableString.Invariant($"T' rises along the {el:F1} deg ray at {d} m"));
                    prev = t;
                }
            }
            NocFogMath.StateAt(st, 600, 240, 1.0, false);
            Assert.AreEqual(0.0, NocFogMath.SinkVeil(st, -1000.0), 1e-12, "cut with the sheets (14.8 item 9)");
        }

        // ---------------------------------------------------------------- C02-air
        [Test]
        public void KoiAir_ClosedFormEqualsQuadrature_AndLiftsTheAirBesideTheBody()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 1300, 520);
            st.CamX = 944.18; st.CamZ = 387.88;
            st.KoiAir = 0.0;
            Assert.AreEqual(0.0, NocFogMath.KoiAir(st, 1, 0, 0, 3000).g, 0.0, "zero outside beat 7");
            // a spine of 12 points 75 m apart, 900 m long, at y 300, 700 m east of the eye
            for (int i = 0; i < 12; i++) { st.KoiPts[3 * i] = 1650; st.KoiPts[3 * i + 1] = 300; st.KoiPts[3 * i + 2] = 387.88 - 412.5 + 75 * i; }
            st.KoiAir = NocAir.KoiAirGain;
            var rnd = new Random(9);
            double R = NocAbyss.KoiAirR, kg = NocFogMath.KoiAirColour().g;
            for (int t = 0; t < 40; t++)
            {
                double dx = 1, dy = 0.2 + 0.4 * (rnd.NextDouble() - 0.5), dz = 0.6 * (rnd.NextDouble() - 0.5);
                double l = Math.Sqrt(dx * dx + dy * dy + dz * dz); dx /= l; dy /= l; dz /= l;
                double len = 400 + 1200 * rnd.NextDouble();
                int n = 6000; double sum = 0;
                var own = new double[12];
                for (int i = 0; i < 12; i++)
                {
                    double qx = st.KoiPts[3 * i] - st.CamX, qy = st.KoiPts[3 * i + 1] - st.CamY, qz = st.KoiPts[3 * i + 2] - st.CamZ;
                    double q0 = qx * dx + qy * dy + qz * dz, qh = Math.Sqrt(Math.Max(qx * qx + qy * qy + qz * qz - q0 * q0, 0.0));
                    own[i] = NocFogMath.KoiAirOwn(qx, qy, qz, dx * len, dy * len, dz * len) * NocMath.Smoothstep(NocAbyss.KoiAirH0, NocAbyss.KoiAirH1, qh);
                }
                for (int j = 0; j <= n; j++)
                {
                    double tt = len * j / n, w = (j == 0 || j == n) ? 0.5 : 1.0, px = st.CamX + dx * tt, py = st.CamY + dy * tt, pz = st.CamZ + dz * tt;
                    for (int i = 0; i < 12; i++)
                    {
                        double ax = px - st.KoiPts[3 * i], ay = py - st.KoiPts[3 * i + 1], az = pz - st.KoiPts[3 * i + 2];
                        sum += w * own[i] * Math.Exp(-(ax * ax + ay * ay + az * az) / (2 * R * R));
                    }
                }
                double quad = sum * len / n / (R * Math.Sqrt(Math.PI / 2.0)) * 0.5 * st.KoiAir * kg;
                Assert.AreEqual(quad, NocFogMath.KoiAir(st, dx, dy, dz, len).g, Math.Max(2e-3 * quad, 1e-7), "ray " + t);
            }
            // the air 65 m beside the body against 400 m away (a ray to the deck past the side of the spine)
            Func<double, double> beside = off =>
            {
                double tx = 1650 - st.CamX, ty = 300 - st.CamY, tz = off;
                double l = Math.Sqrt(tx * tx + ty * ty + tz * tz);
                return NocFogMath.KoiAir(st, tx / l, ty / l, tz / l, 3000).g;
            };
            double near = beside(412.5 + 48 + 65), far = beside(412.5 + 400);
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] koi air g: 65 m beside the tail end {near:E3}, 400 m beyond {far:E3}"));
            Assert.Greater(near, 5.0 * far, "the glow hugs the body");
            // the body's own fragments (on the spine's surface, 48 m from a point) take no glow from it; the air colour is less
            // saturated than KoiGlow (rain haze)
            double bx = 1650 - st.CamX - 48, by = 300 - st.CamY, bz = 387.88 - 412.5 + 75 * 5 - st.CamZ;
            Assert.AreEqual(0.0, NocFogMath.KoiAirOwn(bx + 48, by, bz, bx, by, bz), 1e-12, "a body fragment 48 m from its point");
            var ac = NocFogMath.KoiAirColour(); var kc = NocFogMath.Pal(NocPal.KoiGlow);
            Assert.Greater(ac.r / ac.b, kc.r / kc.b, "the air is whiter than the emitter");
        }

        // ---------------------------------------------------------------- C26 sheet lightning
        [Test]
        public void Flash_EnvelopeScheduleAmplitudeAndHullOutline()
        {
            Assert.AreEqual(0.0, NocFogMath.FlashEnvelope(0.0), 1e-12);
            Assert.AreEqual(1.0, NocFogMath.FlashEnvelope(NocAir.FlashRise), 1e-9);
            Assert.AreEqual(0.0, NocFogMath.FlashEnvelope(NocAir.FlashRise + NocAir.FlashDecay), 1e-12);
            double prev = 0;
            for (int i = 1; i < 190; i++)
            {
                double t = i / 100.0, v = NocFogMath.FlashEnvelope(t);
                if (t <= NocAir.FlashRise + 1e-9) Assert.GreaterOrEqual(v, prev - 1e-12, "rises over 0.4 s"); else Assert.LessOrEqual(v, prev + 1e-12, "decays over 1.5 s");
                prev = v;
            }
            double[] ls = DreadScore.LightningS;
            for (int i = 0; i < ls.Length; i++)
            {
                int b = NocMath.BeatIndex(ls[i]);
                Assert.IsTrue(b != 0 && b != 4, "no sheet lightning in beats 1 and 5: s " + ls[i]);
                if (i > 0) Assert.GreaterOrEqual(ls[i] - ls[i - 1], 62.5, "at most one per 25 s at 2.5 m/s");
                bool hull = AtmosphereDriver.FlashCentre(ls[i], out double cx, out double cz);
                D3 p = NocMath.PathPoint(ls[i]);
                double d = Math.Sqrt((cx - p.x) * (cx - p.x) + (cz - p.z) * (cz - p.z));
                UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] flash s {ls[i]}: centre ({cx:F0}, {cz:F0}), {d:F0} m from the walker, hull {hull}"));
                Assert.LessOrEqual(d, 1000.0, "the glow is overhead or ahead, s " + ls[i]);
            }
            // amplitude: at most +15 % of the deck anywhere; zero amplitude = identity; the hull keeps <= 0.1 of it
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 553, 221.2);
            st.FlashX = 515.3; st.FlashZ = 195; st.FlashSigma = NocAir.FlashSigma;
            var rnd = new Random(13);
            for (int i = 0; i < 400; i++)
            {
                double x = 515.3 + 900 * (rnd.NextDouble() - 0.5), z = 195 + 900 * (rnd.NextDouble() - 0.5);
                st.FlashAmp = 0; var c0 = NocFogMath.CloudColor(st, x, z, 1.0);
                st.FlashAmp = NocAir.FlashAmp; var c1 = NocFogMath.CloudColor(st, x, z, 1.0);
                Assert.LessOrEqual(c1.Luma / c0.Luma, 1.0 + NocAir.FlashAmp + 1e-9, "at most +15 %");
                Assert.GreaterOrEqual(c1.Luma, c0.Luma - 1e-12, "a flash only brightens");
            }
            st.DeckOccX = 515.3; st.DeckOccZ = 195; st.DeckOccHalf = 950; st.DeckOccW = 0.6;
            st.FlashAmp = 0; var h0 = NocFogMath.CloudColor(st, 515.3, 195, 1.0);
            st.FlashAmp = NocAir.FlashAmp; var h1 = NocFogMath.CloudColor(st, 515.3, 195, 1.0);
            var e0 = NocFogMath.CloudColor(st, 515.3 + 300, 195, 1.0); st.FlashAmp = 0; var e00 = NocFogMath.CloudColor(st, 515.3 + 300, 195, 1.0);
            double inHull = h1.Luma / h0.Luma - 1.0, outside = e0.Luma / e00.Luma - 1.0;
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] flash gain over the hull {inHull:P2}, 300 m beside it {outside:P2}"));
            Assert.LessOrEqual(inHull, 0.1 * NocAir.FlashAmp + 1e-9, "the hull stays dark: the flash outlines it");
            Assert.Greater(outside, 3.0 * inHull, "the deck beside the hull flashes");
        }

        // ---------------------------------------------------------------- C44 alley lift
        [Test]
        public void SlitLift_OnlyOverTheAlley()
        {
            Assert.AreEqual(1.0, NocFogMath.SlitShape(NocAbyss.SlitX, NocAbyss.SlitZ), 1e-12);
            foreach (string tag in new[] { "KS04_boulevard_axis", "KS05_boulevard_up" })
            {
                KeyShot k = Array.Find(NocturneConstants.KeyShots, q => q.tag == tag);
                double t = (NocturneConstants.CloudBaseY - k.y) / Math.Tan(k.pitch * Math.PI / 180);
                double x = k.x + t * Math.Sin(k.yaw * Math.PI / 180), z = k.z + t * Math.Cos(k.yaw * Math.PI / 180);
                Assert.Less(NocAir.SlitLift * NocFogMath.SlitShape(x, z), 2e-3, tag + " lid centre");
            }
            Assert.Less(NocAir.SlitLift * NocFogMath.SlitShape(115, 195), 0.025, "the boulevard's hot core (3.2 table: +0.5 code)");
            Assert.Greater(NocAir.SlitLift * NocFogMath.SlitShape(0, 88), 0.2, "the KS02 zenith");
        }
    }
}
