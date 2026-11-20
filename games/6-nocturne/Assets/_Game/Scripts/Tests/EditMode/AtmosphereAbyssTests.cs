// AtmosphereAbyssTests (AT, Phase 4 wave A): EditMode tests of the abyss of H3 and the deck form of INT-11 on the double mirror NocFogMath
// (NocAtmosphere.hlsl's NocAbyss / NocSheetNoise / NocDeckForm / NocCloudMix are line-by-line twins):
//   the "#define NA_" constants of the shader equal NocAbyss's (one source of truth in two languages); the three haze sheets layer
//   the KS10 drop (each sheet's band rises >= 3 codes over the gap above it, the drop e -5..-30 reads 48-60: LOOK_V2 8.1 drop 50-66);
//   the nadir seen from KS13 is warm and structured (p95 - p5 >= 5 codes, mean 58-68, hue 18-35, saturation <= 0.45); no surface
//   below y -2 500 can show in KS10 or KS13 (T <= 1e-3 there, H3); the abyss scroll wraps exactly; the deck form is odd, bounded
//   and never flat, gives +-17 % or more of luminance std where v1 gave 13 % / 11 %, and its cloud-mix wisps reach 70 m.
// Reference numbers (KS10 rises 7.9 / 6.4 / 4.2 codes, drop 51.0; KS13 range 8.5, mean 64.8, hue 25.7, sat 0.36) come from a numpy
// twin of the same model (AT log, 2026-11-16); the thresholds keep a margin for the float / double difference.
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class AtmosphereAbyssTests
    {
        static double Luma(C3 lin, double ev)
        {
            var c = NocFogMath.Floor(lin);
            NocGradeMath.Display(c.r, c.g, c.b, ev, 0.0, 0.0, 1.0, out double r, out double g, out double b);
            return 255.0 * NocMath.Luma709(NocturnePalette.LinearToSrgb(r), NocturnePalette.LinearToSrgb(g), NocturnePalette.LinearToSrgb(b));
        }

        static void Srgb(C3 lin, double ev, out double r, out double g, out double b)
        {
            var c = NocFogMath.Floor(lin);
            NocGradeMath.Display(c.r, c.g, c.b, ev, 0.0, 0.0, 1.0, out r, out g, out b);
            r = NocturnePalette.LinearToSrgb(r); g = NocturnePalette.LinearToSrgb(g); b = NocturnePalette.LinearToSrgb(b);
        }

        /// <summary>Unit ray of pixel (px, py) of a W x H frame at yaw / pitch / vFOV (deg; yaw 0 = +Z, 90 = +X).</summary>
        static void Ray(int px, int py, int W, int H, double yawDeg, double pitchDeg, double fovDeg, out double dx, out double dy, out double dz)
        {
            double yaw = yawDeg * Math.PI / 180, pitch = pitchDeg * Math.PI / 180;
            double f = 0.5 * H / Math.Tan(fovDeg * Math.PI / 360);
            double sx = (px + 0.5 - W * 0.5) / f, sy = (H * 0.5 - py - 0.5) / f;
            double fx = Math.Sin(yaw) * Math.Cos(pitch), fy = Math.Sin(pitch), fz = Math.Cos(yaw) * Math.Cos(pitch);
            double rx = Math.Cos(yaw), rz = -Math.Sin(yaw);                                 // right (y 0)
            double ux = fy * rz, uy = fz * rx - fx * rz, uz = -fy * rx;                     // up = forward x right
            dx = fx + sx * rx + sy * ux; dy = fy + sy * uy; dz = fz + sx * rz + sy * uz;
            double l = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            dx /= l; dy /= l; dz /= l;
        }

        static NocAtmoState ShotState(string tag, double t = 0.0)
        {
            KeyShot k = Array.Find(NocturneConstants.KeyShots, q => q.tag == tag);
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, k.s, t);
            st.CamX = k.x; st.CamY = k.y; st.CamZ = k.z;
            st.PixelAngle = 2.0 * Math.Tan(k.fov * Math.PI / 360) / 1080.0;
            return st;
        }

        // ---------------------------------------------------------------- one source of truth in two languages
        [Test]
        public void AbyssConstants_HlslMatchesCSharp()
        {
            string src = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "_Game/Shaders/Include/NocAtmosphere.hlsl"));
            var defs = Regex.Matches(src, @"^#define NA_(\w+) ([-+0-9.eE]+)\s*$", RegexOptions.Multiline);
            Assert.Greater(defs.Count, 40, "the NA_ block of NocAtmosphere.hlsl");
            foreach (Match m in defs)
            {
                FieldInfo f = typeof(NocAbyss).GetField(m.Groups[1].Value, BindingFlags.Public | BindingFlags.Static);
                Assert.IsNotNull(f, "NocAbyss has no twin of NA_" + m.Groups[1].Value);
                double hl = double.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
                Assert.AreEqual(Convert.ToDouble(f.GetValue(null)), hl, 1e-9 * Math.Max(1.0, Math.Abs(hl)), "NA_" + m.Groups[1].Value);
            }
            foreach (FieldInfo f in typeof(NocAbyss).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (f.IsLiteral && f.Name != "SheetWind" && f.Name != "DeepWind")
                    StringAssert.Contains("#define NA_" + f.Name + " ", src, "the shader lacks NocAbyss." + f.Name);
            // the shader uses its sheet, abyss and deck functions where the twins say it does
            StringAssert.Contains("[branch] if (e < 0.0) c = NocAbyss(c, d, e, blurRad);", src);
            StringAssert.Contains("float hw = max(NA_CovHalf, NA_CovAA * g);", src);
            StringAssert.Contains("smoothstep(NC_CloudMixY0, NC_CloudMixY1, posWS.y + NA_CloudWisp * f)", src);
            StringAssert.Contains("1.0 - pow(x, NA_AbyssExp)", src);
        }

        // ---------------------------------------------------------------- H3: the sheets layer the drop (INT-01)
        [Test]
        public void Abyss_ThreeSheetsLayerTheKS10Drop()
        {
            var st = ShotState("KS10_edge_plunge");
            KeyShot k = Array.Find(NocturneConstants.KeyShots, q => q.tag == "KS10_edge_plunge");
            const int W = 1920, H = 1080, C0 = 928, C1 = 992;
            var prof = new double[H]; var el = new double[H];
            for (int py = 0; py < H; py++)
            {
                double sum = 0, es = 0;
                for (int px = C0; px < C1; px++)
                {
                    Ray(px, py, W, H, k.yaw, k.pitch, k.fov, out double dx, out double dy, out double dz);
                    sum += Luma(NocFogMath.SkyColor(st, dx, dy, dz), 0.0);
                    es += Math.Asin(dy) * 180 / Math.PI;
                }
                prof[py] = sum / (C1 - C0); el[py] = es / (C1 - C0);
            }
            double dropSum = 0; int dropN = 0;
            for (int i = 0; i < H; i++) if (el[i] <= -5 && el[i] >= -30) { dropSum += prof[i]; dropN++; }
            double drop = dropSum / dropN;
            var msg = FormattableString.Invariant($"KS10 drop e -5..-30 mean {drop:F2}");
            for (int s = 0; s < 3; s++)
            {
                double ek = st.SheetE[s], bandMax = double.MinValue, gapMin = double.MaxValue;
                for (int i = 0; i < H; i++)
                {
                    if (el[i] <= ek && el[i] >= ek - 3) bandMax = Math.Max(bandMax, prof[i]);
                    if (el[i] >= ek + 1 && el[i] <= ek + 4) gapMin = Math.Min(gapMin, prof[i]);
                }
                msg += FormattableString.Invariant($"; sheet {s} (e {ek:F1}) band {bandMax:F1} over gap {gapMin:F1}: +{bandMax - gapMin:F2}");
                Assert.GreaterOrEqual(bandMax - gapMin, 3.0, "sheet " + s + " does not layer the drop: " + msg);
            }
            UnityEngine.Debug.Log("[NOC] " + msg);
            Assert.That(drop, Is.InRange(48.0, 60.0), msg);
        }

        // ---------------------------------------------------------------- H3: the nadir glows warm with structure (INT-01)
        [Test]
        public void Abyss_NadirWarmAndStructured_KS13()
        {
            var st = ShotState("KS13_bridge_glass");
            KeyShot k = Array.Find(NocturneConstants.KeyShots, q => q.tag == "KS13_bridge_glass");
            const int W = 1920, H = 1080;
            var ys = new System.Collections.Generic.List<double>();
            double sr = 0, sg = 0, sb = 0, ssat = 0;
            for (int py = 412; py < 668; py += 2)
                for (int px = 832; px < 1088; px += 2)
                {
                    Ray(px, py, W, H, k.yaw, k.pitch, k.fov, out double dx, out double dy, out double dz);
                    Srgb(NocFogMath.SkyColor(st, dx, dy, dz), 0.0, out double r, out double g, out double b);
                    ys.Add(255.0 * NocMath.Luma709(r, g, b));
                    sr += r; sg += g; sb += b;
                    double mx = Math.Max(r, Math.Max(g, b)), mn = Math.Min(r, Math.Min(g, b));
                    ssat += mx > 0 ? (mx - mn) / mx : 0;
                }
            ys.Sort();
            int n = ys.Count;
            double p5 = ys[(int)(0.05 * n)], p95 = ys[(int)(0.95 * n)], mean = 0;
            foreach (double v in ys) mean += v;
            mean /= n;
            double mr = sr / n, mg = sg / n, mb = sb / n;
            double hue = (Math.Atan2(Math.Sqrt(3) * (mg - mb), 2 * mr - mg - mb) * 180 / Math.PI + 360) % 360;
            string msg = FormattableString.Invariant($"KS13 nadir: mean {mean:F1}, p5 {p5:F1}, p95 {p95:F1}, hue {hue:F1}, sat {ssat / n:F3}");
            UnityEngine.Debug.Log("[NOC] " + msg);
            Assert.GreaterOrEqual(p95 - p5, 5.0, "at least 5 codes of structure: " + msg);
            Assert.That(mean, Is.InRange(58.0, 68.0), "LOOK_V2 8.1 KS13 glowing abyss 60-66: " + msg);
            Assert.That(hue, Is.InRange(18.0, 35.0), "warm glow of the lit city: " + msg);
            Assert.LessOrEqual(ssat / n, 0.45, "umber, not orange: " + msg);
        }

        // ---------------------------------------------------------------- H3: no surface below y -2 500 in KS10 / KS13
        [Test]
        public void Abyss_NoSurfaceBelow2500_KS10_KS13()
        {
            foreach (string tag in new[] { "KS10_edge_plunge", "KS13_bridge_glass" })
            {
                var st = ShotState(tag);
                KeyShot k = Array.Find(NocturneConstants.KeyShots, q => q.tag == tag);
                double worst = 0;
                for (int py = 0; py < 1080; py += 8)
                    for (int px = 0; px < 1920; px += 8)
                    {
                        Ray(px, py, 1920, 1080, k.yaw, k.pitch, k.fov, out double dx, out double dy, out double dz);
                        if (dy >= -1e-3) continue;
                        double d = (st.CamY + 2500.0) / -dy;
                        worst = Math.Max(worst, NocFogMath.FogT(st, st.CamX + dx * d, -2500.0, st.CamZ + dz * d));
                    }
                Assert.LessOrEqual(worst, 1e-3, tag + FormattableString.Invariant($": a surface at y -2 500 keeps T {worst:E2}"));
            }
        }

        // ---------------------------------------------------------------- scroll and periodicity
        [Test]
        public void Abyss_ScrollWrapsExactly()
        {
            var rnd = new Random(11);
            for (int i = 0; i < 200; i++)
            {
                double x = -9000 + 18000 * rnd.NextDouble(), z = -9000 + 18000 * rnd.NextDouble(), blur = 0.2 + 3.0 * rnd.NextDouble();
                int k = i % 3;
                double sx = NocAbyss.SheetScrollPeriod * rnd.NextDouble(), sz = NocAbyss.SheetScrollPeriod * rnd.NextDouble();
                double a = NocFogMath.SheetNoise(x, z, sx, sz, blur, k, out _, out _);
                double b = NocFogMath.SheetNoise(x, z, sx + NocAbyss.SheetScrollPeriod, sz, blur, k, out _, out _);
                double c = NocFogMath.SheetNoise(x, z, sx, sz - NocAbyss.SheetScrollPeriod, blur, k, out _, out _);
                Assert.AreEqual(a, b, 1e-5, "sheet billows repeat when the scroll wraps (x)");
                Assert.AreEqual(a, c, 1e-5, "sheet billows repeat when the scroll wraps (z)");
            }
            var s0 = new NocAtmoState(); var s1 = new NocAtmoState();
            NocFogMath.StateAt(s0, 520, 3000.0); NocFogMath.StateAt(s1, 520, 3000.0 + 1.0 / 60.0);
            double step = Math.Sqrt(Math.Pow(s1.SheetScrollX - s0.SheetScrollX, 2) + Math.Pow(s1.SheetScrollZ - s0.SheetScrollZ, 2));
            Assert.AreEqual(NocAbyss.SheetWind / 60.0, step, 1e-6, "1.2 m/s of wind: 0.02 m per frame at 60 fps (no pop)");
            Assert.That(s0.SheetScrollX, Is.InRange(0.0, NocAbyss.SheetScrollPeriod));
            Assert.That(s0.DeepScrollZ, Is.InRange(0.0, NocAbyss.DeepScrollPeriod));
            // the periods hold a whole number of 64-cell lattices of every octave
            foreach (double L in new[] { NocAbyss.SheetL0, NocAbyss.SheetL1, NocAbyss.SheetL2, NocAbyss.SheetL3 })
                Assert.AreEqual(0.0, Frac(NocAbyss.SheetScrollPeriod / (64 * L)), 1e-5, "sheet octave " + L);
            foreach (double L in new[] { NocAbyss.DeepL0, NocAbyss.DeepL1 })
                Assert.AreEqual(0.0, Frac(NocAbyss.DeepScrollPeriod / (64 * L)), 1e-9, "deep octave " + L);
        }

        static double Frac(double v) { double f = v - Math.Round(v); return Math.Abs(f); }

        // ---------------------------------------------------------------- INT-11: the deck form
        [Test]
        public void Deck_FormIsOddBoundedNeverFlat_AndStrongerBillows()
        {
            Assert.AreEqual(0.0, NocFogMath.DeckForm(0.5), 1e-12);
            double prev = -2;
            for (double n = 0; n <= 1.0; n += 0.001)
            {
                double f = NocFogMath.DeckForm(n);
                Assert.AreEqual(-f, NocFogMath.DeckForm(1.0 - n), 1e-12, "odd about n 0.5");
                Assert.Less(Math.Abs(f), 1.0);
                Assert.Greater(f, prev, "monotone, never a plateau (H1)");
                prev = f;
            }
            // luminance std / mean of the base deck (far from every glow) and of the hot lid (one point under the east leg, the noise
            // sampled by 20 000 random scroll offsets) (v1: 13 % and 11 %)
            Assert.GreaterOrEqual(DeckSpread(575.0, -2500.0, 20000.0, 1), 0.17, "base deck");
            Assert.LessOrEqual(DeckSpread(575.0, -2500.0, 20000.0, 1), 0.30, "base deck: billows, not blotches");
            Assert.GreaterOrEqual(DeckSpread(115.0, 195.0, 0.0, 2), 0.17, "hot lid over the east leg");
            Assert.LessOrEqual(DeckSpread(115.0, 195.0, 0.0, 2), 0.30, "hot lid: billows, not blotches");
        }

        /// <summary>Luminance std / mean of CloudColor over 20 000 samples: points spread over +-span/2 around (x0, z0), or (span 0)
        /// one point with random scroll offsets (the octaves' own periods).</summary>
        static double DeckSpread(double x0, double z0, double span, int seed)
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 300, 17.0);
            var rnd = new Random(seed);
            double s = 0, s2 = 0; const int n = 20000;
            int np = NocturneConstants.CloudNoisePeriod;
            for (int i = 0; i < n; i++)
            {
                double x = x0 + (rnd.NextDouble() - 0.5) * span, z = z0 + (rnd.NextDouble() - 0.5) * span;
                if (span <= 0)
                {
                    st.Cloud1X = rnd.NextDouble() * np * NocturneConstants.CloudNoise1; st.Cloud1Z = rnd.NextDouble() * np * NocturneConstants.CloudNoise1;
                    st.Cloud2X = rnd.NextDouble() * np * NocturneConstants.CloudNoise2; st.Cloud2Z = rnd.NextDouble() * np * NocturneConstants.CloudNoise2;
                    st.Cloud3X = rnd.NextDouble() * np * NocturneConstants.CloudNoise3; st.Cloud3Z = rnd.NextDouble() * np * NocturneConstants.CloudNoise3;
                    st.Cloud4X = rnd.NextDouble() * np * NocturneConstants.CloudNoise4; st.Cloud4Z = rnd.NextDouble() * np * NocturneConstants.CloudNoise4;
                }
                double y = NocFogMath.CloudColor(st, x, z, 0.5).Luma;
                s += y; s2 += y * y;
            }
            double mean = s / n, sd = Math.Sqrt(Math.Max(0, s2 / n - mean * mean));
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] deck form at ({x0}, {z0}): luminance std / mean {sd / mean:F3} (v1 0.13 base / 0.11 lid)"));
            return sd / mean;
        }

        [Test]
        public void Deck_CloudMixWispsReach70m()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 300, 0.0);
            st.CamX = 115; st.CamZ = 195;
            var col = new C3(0, 0, 0);
            int mixedLow = 0, clearLow = 0, n = 2000;
            var rnd = new Random(9);
            for (int i = 0; i < n; i++)
            {
                double x = -3000 + 6000 * rnd.NextDouble(), z = -3000 + 6000 * rnd.NextDouble();
                double dx = x - st.CamX, dz = z - st.CamZ, d = Math.Sqrt(dx * dx + dz * dz + 348.3 * 348.3);
                NocFogMath.CloudColor(st, x, z, st.PixelAngle * d, out double f);
                var m = NocFogMath.CloudMix(st, col, x, 350.0, z);                 // 30 m under the v1 mix start
                if (m.g > 0) mixedLow++; else clearLow++;
                if (f < 0.4) Assert.AreEqual(0.0, m.g, 1e-15, "a gap in the deck keeps the tower clear at y 350");
                var top = NocFogMath.CloudMix(st, col, x, 640.0, z);              // above 560 + 70: always fully cloud
                var cc = NocFogMath.CloudColor(st, x, z, st.PixelAngle * Math.Sqrt(dx * dx + dz * dz + 638.3 * 638.3));
                Assert.AreEqual(cc.g, top.g, 1e-12);
            }
            Assert.Greater(mixedLow, n / 20, "bellies hang below y 380 somewhere");
            Assert.Greater(clearLow, n / 4, "and gaps stay clear");
            Assert.AreEqual(0.0, NocFogMath.CloudMix(st, col, 0, 309.0, 0).g, 1e-15, "nothing mixes below 380 - 70 m");
        }
    }
}
