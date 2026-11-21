// GradeTests: EditMode tests of the BwGrade building blocks (MASTER_PROMPT 14.2.5, 14.7 H5; imported from Thread):
// the void-and-cluster blue-noise tile (permutation, determinism, flat histogram, blue spectrum, < 1 s),
// the R2 tile offsets, and the CPU mirror of the display transform (Neutral curve: identity below 0.76,
// monotonic, hue-preserving, clamps; the D-10 colour matrix and 0.18-pivot contrast; the per-beat static dither;
// vignette; bloom pyramid plan; the shader mirrors the CPU order). Thresholds were checked against a numpy simulation of the same algorithms
// (void-and-cluster, seed 20261029: low-band power 0.06 vs white 341, 4x4 block-mean variance 0.00094 vs
// white 0.0053, neighbour correlation -0.29 / -0.26; R2: per-axis counts 62..66 of 64 over 4096 frames).
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Megastructure.Rendering;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class GradeTests
    {
        const int Tile = BlueNoise.Size;
        const int Count = Tile * Tile;

        static int[] Ranks => BlueNoise.Ranks;

        // ============================================================================ blue noise
        [Test]
        public void BlueNoise_RankMapIsPermutationOf0To4095()
        {
            int[] r = Ranks;
            Assert.AreEqual(Count, r.Length);
            var seen = new bool[Count];
            for (int i = 0; i < r.Length; i++)
            {
                Assert.That(r[i], Is.InRange(0, Count - 1), $"rank out of range at {i}");
                Assert.IsFalse(seen[r[i]], $"rank {r[i]} appears twice");
                seen[r[i]] = true;
            }
        }

        [Test]
        public void BlueNoise_IsDeterministic()
        {
            int[] a = BlueNoise.GenerateRanks(Tile, FrameworkConst.DitherSigma, BlueNoise.Seed);
            int[] b = BlueNoise.GenerateRanks(Tile, FrameworkConst.DitherSigma, BlueNoise.Seed);
            CollectionAssert.AreEqual(a, b);
            CollectionAssert.AreEqual(a, Ranks);
        }

        [Test]
        public void BlueNoise_GeneratesInUnderOneSecond()
        {
            var sw = Stopwatch.StartNew();
            BlueNoise.GenerateRanks(Tile, FrameworkConst.DitherSigma, BlueNoise.Seed);
            double ms = sw.Elapsed.TotalMilliseconds;
            UnityEngine.Debug.Log($"[BW] blue noise 64x64 void-and-cluster generated in {ms:F0} ms");
            Assert.Less(ms, 1000.0, "generation must finish in under 1 s");
        }

        [Test]
        public void BlueNoise_HistogramIsUniform()
        {
            // 16 bins of the stored uniform value (rank + 0.5) / 4096: exactly 256 each, mean exactly 0.5.
            var bins = new int[16];
            double sum = 0;
            foreach (int r in Ranks)
            {
                double u = BlueNoise.RankToUniform(r, Count);
                Assert.That(u, Is.GreaterThan(0.0).And.LessThan(1.0));
                bins[Math.Min(15, (int)(u * 16))]++;
                sum += u;
            }
            foreach (int c in bins) Assert.AreEqual(Count / 16, c);
            Assert.AreEqual(0.5, sum / Count, 1e-6);
        }

        [Test]
        public void BlueNoise_LowFrequencyEnergyFarBelowWhiteNoise()
        {
            double blue = LowBandPower(Ranks);
            double white = LowBandPower(WhitePermutation(12345UL));
            double whiteExpected = Count * (1.0 / 12.0);   // E|X(f)|^2 of i.i.d. uniform noise
            UnityEngine.Debug.Log($"[BW] blue-noise low-band power {blue:F3} vs white {white:F1} (expected {whiteExpected:F1})");
            Assert.Less(blue, 0.02 * whiteExpected, "blue noise must have almost no energy below 0.1 cycles/px");
            Assert.Less(blue, 0.02 * white);
            Assert.Greater(white, 0.25 * whiteExpected, "sanity: the white reference is white");
        }

        [Test]
        public void BlueNoise_SpatialStatisticsAreBlue()
        {
            double[] u = Uniforms(Ranks);
            double blockVar = BlockMeanVariance(u, 4);
            double whiteBlockVar = BlockMeanVariance(Uniforms(WhitePermutation(777UL)), 4);
            double right = NeighbourCorrelation(u, 1, 0), down = NeighbourCorrelation(u, 0, 1);
            Assert.Less(blockVar, 0.0015, "4x4 block means of blue noise are nearly constant");
            Assert.Greater(whiteBlockVar, 0.003, "sanity: white noise blocks vary");
            Assert.Less(right, -0.15, "neighbouring texels anti-correlate (horizontal)");
            Assert.Less(down, -0.15, "neighbouring texels anti-correlate (vertical)");
        }

        [Test]
        public void BlueNoise_TextureMatchesRanks()
        {
            Texture2D tex = BlueNoise.Get();
            Assert.IsNotNull(tex);
            Assert.AreEqual(Tile, tex.width);
            Assert.AreEqual(Tile, tex.height);
            Assert.AreEqual(FilterMode.Point, tex.filterMode);
            Assert.AreEqual(TextureWrapMode.Repeat, tex.wrapModeU);
            Assert.AreEqual(TextureWrapMode.Repeat, tex.wrapModeV);
            Assert.AreEqual(1, tex.mipmapCount);
            Assert.That(tex.format == TextureFormat.RFloat || tex.format == TextureFormat.R16, $"format {tex.format}");
            Assert.AreSame(tex, BlueNoise.Get(), "the texture is created once");
            int[] r = Ranks;
            if (tex.format == TextureFormat.RFloat)
            {
                var data = tex.GetPixelData<float>(0);
                Assert.AreEqual(Count, data.Length);
                for (int i = 0; i < Count; i++) Assert.AreEqual(BlueNoise.RankToUniform(r[i], Count), data[i], 1e-7f);
            }
            else
            {
                var data = tex.GetPixelData<ushort>(0);
                Assert.AreEqual(Count, data.Length);
                for (int i = 0; i < Count; i++) Assert.AreEqual(BlueNoise.RankToUniform(r[i], Count), data[i] / 65535.0, 1e-4);
            }
        }

        // ============================================================================ R2 offsets
        [Test]
        public void R2_FirstOffsetsMatchReference()
        {
            // research R6 3.4 (numpy): (32,32), (16,4), (0,40), (48,13), (33,49), (17,22)
            int[] ex = { 32, 32, 16, 4, 0, 40, 48, 13, 33, 49, 17, 22 };
            for (int n = 0; n < 6; n++)
            {
                R2Sequence.TileOffset(n, Tile, out int ox, out int oy);
                Assert.AreEqual(ex[2 * n], ox, $"x of frame {n}");
                Assert.AreEqual(ex[2 * n + 1], oy, $"y of frame {n}");
            }
        }

        [Test]
        public void R2_OffsetsAreWellDistributed()
        {
            var hx = new int[Tile];
            var hy = new int[Tile];
            const int frames = 4096;
            for (int n = 0; n < frames; n++)
            {
                R2Sequence.TileOffset(n, Tile, out int ox, out int oy);
                Assert.That(ox, Is.InRange(0, Tile - 1));
                Assert.That(oy, Is.InRange(0, Tile - 1));
                hx[ox]++; hy[oy]++;
            }
            for (int i = 0; i < Tile; i++)
            {
                Assert.That(hx[i], Is.InRange(60, 68), $"x offset {i} used {hx[i]} times (uniform = 64)");
                Assert.That(hy[i], Is.InRange(60, 68), $"y offset {i} used {hy[i]} times (uniform = 64)");
            }
            // low discrepancy in 2D: the first 16 / 64 offsets keep a minimum toroidal distance (white noise would not)
            Assert.GreaterOrEqual(MinToroidalDistance(16), 8.0);
            Assert.GreaterOrEqual(MinToroidalDistance(64), 4.0);
            // no crawling: consecutive offsets jump far on the torus in both axes
            for (int n = 0; n < frames; n++)
            {
                R2Sequence.TileOffset(n, Tile, out int ax, out int ay);
                R2Sequence.TileOffset(n + 1, Tile, out int bx, out int by);
                Assert.GreaterOrEqual(TorusDelta(ax, bx), 12, $"x step at frame {n}");
                Assert.GreaterOrEqual(TorusDelta(ay, by), 20, $"y step at frame {n}");
            }
        }

        [Test]
        public void R2_FrozenDitherIsStaticAndLargeFramesStayPrecise()
        {
            Assert.AreEqual(0L, R2Sequence.DitherFrame(123456, true));
            Assert.AreEqual(123456L, R2Sequence.DitherFrame(123456, false));
            R2Sequence.TileOffset(R2Sequence.DitherFrame(99999, true), Tile, out int ox, out int oy);
            Assert.AreEqual(32, ox);
            Assert.AreEqual(32, oy);
            // double evaluation keeps the fraction exact far into a session (10^8 frames = 19 days at 60 fps)
            R2Sequence.Point(100000000L, out double x, out double y);
            double ex = Frac(0.5 + 100000000.0 / R2Sequence.G);
            Assert.AreEqual(ex, x, 1e-6);
            Assert.That(y, Is.InRange(0.0, 1.0));
        }

        // ============================================================================ Neutral curve
        [Test]
        public void Neutral_IdentityBelowStartAndHalfMapsToHalf()
        {
            Assert.AreEqual(0.5, BwGradeMath.NeutralGrey(0.5), 0.0);
            for (double x = 0; x < BwGradeMath.NeutralStart; x += 0.001)
                Assert.AreEqual(x, BwGradeMath.NeutralGrey(x), 1e-15);
            // every palette token displays as authored (all are below 0.76 in their peak channel, except the
            // tokens whose peak is above it, which are only compressed, never shifted in hue)
            foreach (Pal p in Enum.GetValues(typeof(Pal)))
            {
                Color c = BulwarkPalette.Linear(p);
                double peak = Math.Max(c.r, Math.Max(c.g, c.b));
                if (peak >= BwGradeMath.NeutralStart) continue;
                BwGradeMath.Neutral(c.r, c.g, c.b, out double r, out double g, out double b);
                Assert.AreEqual(c.r, r, 1e-7, p.ToString());
                Assert.AreEqual(c.g, g, 1e-7, p.ToString());
                Assert.AreEqual(c.b, b, 1e-7, p.ToString());
            }
        }

        [Test]
        public void Neutral_IsMonotonicAndContinuous()
        {
            // (plain checks inside the loops; one Assert.Fail with the first violation keeps the test fast)
            double prev = -1;
            for (double x = 0; x <= 100.0; x += 0.0005)
            {
                double y = BwGradeMath.NeutralGrey(x);
                if (y < prev) Assert.Fail($"grey ramp decreases at {x}");
                if (!(y < 1.0)) Assert.Fail($"grey ramp reaches 1 at {x}");
                if (prev >= 0 && y - prev >= 0.0006) Assert.Fail($"jump at {x}");   // slope <= 1, C0 at 0.76
                prev = y;
            }
            // per channel, along an exposure ramp of every palette colour
            foreach (Pal p in Enum.GetValues(typeof(Pal)))
            {
                Color c = BulwarkPalette.Linear(p);
                double pr = -1, pg = -1, pb = -1;
                for (double k = 0; k <= 60.0; k += 0.01)
                {
                    BwGradeMath.Neutral(c.r * k, c.g * k, c.b * k, out double r, out double g, out double b);
                    if (r < pr - 1e-12 || g < pg - 1e-12 || b < pb - 1e-12) Assert.Fail($"{p} not monotonic at exposure {k}");
                    pr = r; pg = g; pb = b;
                }
            }
        }

        [Test]
        public void Neutral_PreservesHue()
        {
            var rng = new BlueNoise.SplitMix64(4242UL);
            for (int i = 0; i < 20000; i++)
            {
                double r = 8.0 * Rand01(ref rng), g = 8.0 * Rand01(ref rng), b = 8.0 * Rand01(ref rng);
                BwGradeMath.Neutral(r, g, b, out double or, out double og, out double ob);
                double cIn = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b));
                double cOut = Math.Max(or, Math.Max(og, ob)) - Math.Min(or, Math.Min(og, ob));
                if (cIn < 1e-6 || cOut < 1e-9) continue;   // achromatic: no hue
                double d = Math.Abs(Hue(r, g, b) - Hue(or, og, ob));
                d = Math.Min(d, 360.0 - d);
                if (!(d < 1e-6)) Assert.Fail($"hue shift {d} deg at ({r}, {g}, {b})");
                // channel order is kept as well (a common scale and a mix toward grey cannot reorder channels)
                if ((r - g) * (or - og) < 0 || (g - b) * (og - ob) < 0) Assert.Fail($"channel order changed at ({r}, {g}, {b})");
            }
        }

        [Test]
        public void Neutral_SunDiscStaysBelowClipAtClamp()
        {
            // 14.2.5 clamps: sun disc radiance 60 is clamped by the bloom prefilter (40); the toe-less Khronos Neutral maps
            // any finite HDR value below 1 (no overflow, no hue flip). A disc of radiance 60 lands at 0.99903 display, i.e.
            // code 255: the disc core is allowed to clip, because 14.10.3 excludes the sun disc and the lamp cores from the
            // clipped-pixel metric (Phase 2 decision, DEVELOPMENT.md). Grey stays grey and the curve is monotonic.
            BwGradeMath.Neutral(60.0, 60.0, 60.0, out double r, out double g, out double b);
            Assert.Less(Math.Max(r, Math.Max(g, b)), 1.0);
            Assert.AreEqual(r, g, 1e-12); Assert.AreEqual(g, b, 1e-12);
            Assert.Greater(r, BwGradeMath.NeutralGrey(40.0));
            Assert.AreEqual(40.0, BwGradeMath.BloomClamp, 0.0, "14.2.5: bloom clamp 40");
        }

        [Test]
        public void Neutral_LanternStaysBelowClip()
        {
            // the lantern glass is Lantern x 6 (14.11); through the beat-7 grade (EV +0.3) it must stay below code 254
            UnityEngine.Color c = PaletteLookup.Linear("Lantern");
            double k = 6.0;
            BwGradeMath.Display(c.r * k, c.g * k, c.b * k, 0.3, 4.0, 0.0, 8.0, 0.0, 1.0, out double r, out double g, out double b);
            Assert.Less(Math.Max(r, Math.Max(g, b)), 0.9961);
        }

        // ============================================================================ colour matrix (D-10, A 4.3)
        [Test]
        public void Matrix_IdentityAtNeutralSettings()
        {
            double[] m = BwGradeMath.GradeMatrix(0, 0, 0);
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Assert.AreEqual(r == c ? 1.0 : 0.0, m[r * 3 + c], 2e-4, $"M[{r},{c}]");
        }

        [Test]
        public void Matrix_WhiteLuminancePreserved([Values(-10.0, -6.0, 4.0, 10.0)] double wb)
        {
            double[] m = BwGradeMath.GradeMatrix(wb, 0, 0);
            double r = 1, g = 1, b = 1;
            BwGradeMath.Apply(m, ref r, ref g, ref b);
            double y = BwGradeMath.LumR * r + BwGradeMath.LumG * g + BwGradeMath.LumB * b;
            Assert.AreEqual(1.0, y, 1e-9, "exposure stays the only brightness control");
        }

        [Test]
        public void Matrix_SaturationKeepsGreyAndLuminance([Values(-8.0, 4.0, 8.0)] double sat)
        {
            double[] m = BwGradeMath.GradeMatrix(0, 0, sat);
            double r = 0.3, g = 0.3, b = 0.3;
            BwGradeMath.Apply(m, ref r, ref g, ref b);
            Assert.AreEqual(0.3, r, 1e-4); Assert.AreEqual(0.3, g, 1e-4); Assert.AreEqual(0.3, b, 1e-4);
            double[] s = BwGradeMath.SaturationMatrix(sat);
            double cr = 0.6, cg = 0.2, cb = 0.1, y0 = BwGradeMath.LumR * cr + BwGradeMath.LumG * cg + BwGradeMath.LumB * cb;
            BwGradeMath.Apply(s, ref cr, ref cg, ref cb);
            Assert.AreEqual(y0, BwGradeMath.LumR * cr + BwGradeMath.LumG * cg + BwGradeMath.LumB * cb, 1e-12);
        }

        [Test]
        public void Matrix_WarmerWbRaisesRedOverBlue()
        {
            double[] m = BwGradeMath.GradeMatrix(10, 0, 0);
            double r = 0.5, g = 0.5, b = 0.5;
            BwGradeMath.Apply(m, ref r, ref g, ref b);
            Assert.Greater(r, b, "URP sign: temperature > 0 is warmer (R up, B down)");
        }

        // ============================================================================ contrast (D-10)
        [Test]
        public void Contrast_PivotFixedMonotonicAndFloorKept()
        {
            double[] ks = { -0.25, -0.08, -0.04, 0.0, 0.04, 0.08, 0.25 };
            foreach (double k in ks)
            {
                Assert.AreEqual(BwGradeMath.ContrastPivot, BwGradeMath.ContrastLuma(BwGradeMath.ContrastPivot, k), 1e-12, $"pivot fixed at k = {k}");
                double prev = -1;
                for (double y = 0; y <= 4.0; y += 0.0005)
                {
                    double v = BwGradeMath.ContrastLuma(y, k);
                    Assert.GreaterOrEqual(v, prev - 1e-12, $"monotonic at k = {k}, Y = {y}");
                    prev = v;
                }
            }
            // the sRGB-6 floor (linear 0.0018) maps to at least sRGB 5.5 at the strongest specified contrast (+4)
            double kSpec = BwGradeMath.ContrastK(4);
            double fl = BwGradeMath.ContrastLuma(FrameworkConst.FloorLinear, kSpec);
            Assert.GreaterOrEqual(BwGradeMath.LinearToSrgb(fl) * 255.0, 5.5);
            Assert.AreEqual(1.0, BwGradeMath.SaturationFactor(0), 0.0);
            Assert.AreEqual(1.04, BwGradeMath.SaturationFactor(4), 1e-12);
        }

        // ============================================================================ dither
        [Test]
        public void Dither_PeakPerBeatCrossfadesOver30mAndIsStaticByDefault()
        {
            Assert.AreEqual(2.0, BulwarkSchedule.DitherPeak(50.0), 1e-12, "beat 1");
            Assert.AreEqual(1.5, BulwarkSchedule.DitherPeak(300.0), 1e-12, "beat 2");
            Assert.AreEqual(2.0, BulwarkSchedule.DitherPeak(1200.0), 1e-12, "beat 6");
            Assert.AreEqual(1.75, BulwarkSchedule.DitherPeak(120.0), 1e-9, "centre of the 30 m window");
            Assert.AreEqual(1.5, BulwarkSchedule.DitherPeak(135.0), 1e-9, "window ends 15 m after the boundary");
            double prev = BulwarkSchedule.DitherPeak(0);
            for (double s = 0.1; s <= 1500; s += 0.1)
            {
                double a = BulwarkSchedule.DitherPeak(s);
                Assert.Less(Math.Abs(a - prev), 0.01, $"dither peak pops at s = {s}");
                prev = a;
            }
            Assert.AreEqual(0L, R2Sequence.DitherFrame(12345, true), "static dither: tile offset 0");
            Assert.AreEqual(0.0, BwGradeMath.DitherPeak(DitherMode.Off, -1, 2.0), 0.0, "-dither off");
            Assert.AreEqual(0.0, BwGradeMath.DitherPeak(DitherMode.Static, 0.0, 2.0), 0.0, "-dither-lsb 0 == off");
            Assert.AreEqual(1.25, BwGradeMath.DitherPeak(DitherMode.Static, 1.25, 2.0), 0.0, "-dither-lsb override");
        }

        [Test]
        public void Dither_TpdfRemapIsMonotonicSymmetricTriangular()
        {
            double prev = -2;
            for (double u = 0.0005; u < 1.0; u += 0.001)
            {
                double t = BwGradeMath.UniformToTpdf(u);
                Assert.That(t, Is.InRange(-1.0, 1.0));
                Assert.Greater(t, prev, "monotonic (keeps the blue-noise ordering)");
                Assert.AreEqual(-t, BwGradeMath.UniformToTpdf(1.0 - u), 1e-9, "symmetric");
                prev = t;
            }
            // variance of the remapped tile = 1/6 (triangular PDF on [-1, 1]); mean 0
            double sum = 0, sum2 = 0;
            foreach (int r in Ranks)
            {
                double t = BwGradeMath.UniformToTpdf(BlueNoise.RankToUniform(r, Count));
                sum += t; sum2 += t * t;
            }
            double mean = sum / Count;
            Assert.AreEqual(0.0, mean, 1e-6);
            Assert.AreEqual(1.0 / 6.0, sum2 / Count - mean * mean, 0.005);
        }

        // ============================================================================ vignette, bloom plan
        [Test]
        public void Vignette_IsSubtleSymmetricAndHorPlus()
        {
            double v = BulwarkConst.Vignette[0];
            double a169 = 16.0 / 9.0;
            Assert.AreEqual(1.0, BwGradeMath.Vignette(0.5, 0.5, a169, v), 1e-12);
            Assert.AreEqual(1.0 - v, BwGradeMath.Vignette(0.0, 0.0, a169, v), 1e-9, "16:9 corner = 1 - intensity");
            Assert.AreEqual(BwGradeMath.Vignette(0.1, 0.2, a169, v), BwGradeMath.Vignette(0.9, 0.8, a169, v), 1e-12);
            double prev = 2;
            for (double u = 0.5; u >= 0.0; u -= 0.01)
            {
                double f = BwGradeMath.Vignette(u, u, a169, v);
                Assert.LessOrEqual(f, prev + 1e-12, "darkens monotonically toward the corner");
                prev = f;
            }
            // Hor+: the same image point (same offset in half-height units) gets the same factor at 21:9 and 16:9
            double a219 = 2560.0 / 1080.0;
            double y = 0.3;
            double px = 0.6;   // half-height units right of centre
            Assert.AreEqual(BwGradeMath.Vignette(0.5 + px / (2 * a169), y, a169, v),
                            BwGradeMath.Vignette(0.5 + px / (2 * a219), y, a219, v), 1e-12);
            // and the farther 21:9 corners stay subtle
            Assert.Greater(BwGradeMath.Vignette(0.0, 0.0, a219, v), 1.0 - 1.25 * v);
        }

        [Test]
        public void Bloom_PyramidIsResolutionNormalised()
        {
            Assert.AreEqual(0, BwGradeMath.BloomSkipLevels(1080));
            Assert.AreEqual(1, BwGradeMath.BloomSkipLevels(2160));
            Assert.AreEqual(0, BwGradeMath.BloomSkipLevels(720));
            Assert.AreEqual(0, BwGradeMath.BloomSkipLevels(1440));
            Assert.AreEqual(BwGradeMath.BloomLevels, BwGradeMath.BloomTotalLevels(1920, 1080, 10));
            Assert.AreEqual(BwGradeMath.BloomLevels + 1, BwGradeMath.BloomTotalLevels(3840, 2160, 10));
            // the first bloom level has the same pixel size at 1080p (level 0) and at 2160p (level 1): 960 x 540
            Assert.AreEqual(1920 >> 1, 3840 >> 2);
            Assert.AreEqual(1080 >> 1, 2160 >> 2);
            // tiny targets still get at least one level, and no level below 2 px
            int t = BwGradeMath.BloomTotalLevels(16, 16, 10);
            Assert.GreaterOrEqual(t, 1);
            Assert.GreaterOrEqual(16 >> t, 2);
            Assert.AreEqual(0.05, BwGradeMath.ScatterLerp(0), 1e-12);
            Assert.AreEqual(0.59, BwGradeMath.ScatterLerp(0.6), 1e-12);
        }

        [Test]
        public void Shader_ConstantsMatchCpuMirror()
        {
            string path = Path.Combine(Application.dataPath, "_Game/Shaders/BwGrade.shader");
            Assert.IsTrue(File.Exists(path), path);
            string src = File.ReadAllText(path);
            StringAssert.Contains("Shader \"Megastructure/BwGrade\"", src);
            // the fragment calls appear in the CPU mirror's order: matrix, contrast, Neutral, vignette, dither
            int frag = src.IndexOf("float4 FragGrade", StringComparison.Ordinal);
            Assert.Greater(frag, 0);
            int last = frag;
            foreach (string call in BwGradeMath.ShaderOrder)
            {
                int at = src.IndexOf(call, last, StringComparison.Ordinal);
                Assert.Greater(at, last, "shader order: " + call);
                last = at;
            }
            Assert.AreEqual(FrameworkConst.DitherTile, int.Parse(Regex.Match(src, @"#define\s+BW_DITHER_TILE\s+(\d+)").Groups[1].Value));
            Assert.AreEqual(BwGradeMath.ContrastToe0, ShaderConst(src, "kBwGradeToe0"), 1e-9);
            Assert.AreEqual(BwGradeMath.ContrastToe1, ShaderConst(src, "kBwGradeToe1"), 1e-9);
            // the Neutral start / desaturation arrive as uniforms from BwGradeMath; the vignette shape is mirrored
            Assert.AreEqual(BwGradeMath.VignetteShape, ShaderConst(src, "kBwGradeVigShape"), 1e-9);
            StringAssert.Contains("BwGradeNeutral(c, _BwGradeP2.z, _BwGradeP2.w)", src);
            foreach (string pass in new[] { "BwBloomPrefilter", "BwBloomDown", "BwBloomUp", "BwGrade" })
                StringAssert.Contains("Name \"" + pass + "\"", src);
            StringAssert.DoesNotContain("multi_compile_fog", src);
        }

        // ============================================================================ helpers
        static double ShaderConst(string src, string name)
        {
            Match m = Regex.Match(src, @"static\s+const\s+float\s+" + name + @"\s*=\s*([0-9.eE+-]+)\s*;");
            Assert.IsTrue(m.Success, "shader constant " + name + " not found");
            return double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        }

        static double[] Uniforms(int[] ranks)
        {
            var u = new double[ranks.Length];
            for (int i = 0; i < ranks.Length; i++) u[i] = BlueNoise.RankToUniform(ranks[i], ranks.Length);
            return u;
        }

        static int[] WhitePermutation(ulong seed)
        {
            var p = new int[Count];
            for (int i = 0; i < Count; i++) p[i] = i;
            var rng = new BlueNoise.SplitMix64(seed);
            for (int i = Count - 1; i > 0; i--)
            {
                int j = (int)(rng.Next() % (ulong)(i + 1));
                int t = p[i]; p[i] = p[j]; p[j] = t;
            }
            return p;
        }

        /// <summary>Mean |DFT|^2 of (u - 0.5) over 0 < |f| < 0.1 cycles/px (separable 64 x 64 DFT).</summary>
        static double LowBandPower(int[] ranks)
        {
            int n = Tile;
            double[] u = Uniforms(ranks);
            var cs = new double[n];
            var sn = new double[n];
            for (int k = 0; k < n; k++) { cs[k] = Math.Cos(2 * Math.PI * k / n); sn[k] = -Math.Sin(2 * Math.PI * k / n); }
            // rows: R[y, kx]
            var rr = new double[n * n];
            var ri = new double[n * n];
            for (int y = 0; y < n; y++)
                for (int kx = 0; kx < n; kx++)
                {
                    double re = 0, im = 0;
                    for (int x = 0; x < n; x++)
                    {
                        double v = u[y * n + x] - 0.5;
                        int k = (kx * x) & (n - 1);
                        re += v * cs[k]; im += v * sn[k];
                    }
                    rr[y * n + kx] = re; ri[y * n + kx] = im;
                }
            double sum = 0;
            int cnt = 0;
            for (int ky = 0; ky < n; ky++)
                for (int kx = 0; kx < n; kx++)
                {
                    double fx = (kx <= n / 2 ? kx : kx - n) / (double)n;
                    double fy = (ky <= n / 2 ? ky : ky - n) / (double)n;
                    double f = Math.Sqrt(fx * fx + fy * fy);
                    if (f <= 0 || f >= 0.1) continue;
                    double re = 0, im = 0;
                    for (int y = 0; y < n; y++)
                    {
                        int k = (ky * y) & (n - 1);
                        double a = rr[y * n + kx], b = ri[y * n + kx];
                        re += a * cs[k] - b * sn[k];
                        im += a * sn[k] + b * cs[k];
                    }
                    sum += re * re + im * im;
                    cnt++;
                }
            Assert.Greater(cnt, 0);
            return sum / cnt;
        }

        static double BlockMeanVariance(double[] u, int block)
        {
            int n = Tile, m = n / block;
            double s = 0, s2 = 0;
            for (int by = 0; by < m; by++)
                for (int bx = 0; bx < m; bx++)
                {
                    double acc = 0;
                    for (int y = 0; y < block; y++)
                        for (int x = 0; x < block; x++)
                            acc += u[(by * block + y) * n + bx * block + x];
                    acc /= block * block;
                    s += acc; s2 += acc * acc;
                }
            int c = m * m;
            double mean = s / c;
            return s2 / c - mean * mean;
        }

        static double NeighbourCorrelation(double[] u, int dx, int dy)
        {
            int n = Tile;
            double ma = 0;
            for (int i = 0; i < u.Length; i++) ma += u[i];
            ma /= u.Length;
            double cov = 0, var = 0;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    double a = u[y * n + x] - ma;
                    double b = u[((y + dy) & (n - 1)) * n + ((x + dx) & (n - 1))] - ma;
                    cov += a * b; var += a * a;
                }
            return cov / var;
        }

        static int TorusDelta(int a, int b)
        {
            int d = Math.Abs(a - b);
            return Math.Min(d, Tile - d);
        }

        static double MinToroidalDistance(int count)
        {
            double best = double.MaxValue;
            for (int i = 0; i < count; i++)
            {
                R2Sequence.TileOffset(i, Tile, out int ax, out int ay);
                for (int j = i + 1; j < count; j++)
                {
                    R2Sequence.TileOffset(j, Tile, out int bx, out int by);
                    int dx = TorusDelta(ax, bx), dy = TorusDelta(ay, by);
                    best = Math.Min(best, Math.Sqrt(dx * dx + dy * dy));
                }
            }
            return best;
        }

        static double Frac(double v) => v - Math.Floor(v);

        static double Rand01(ref BlueNoise.SplitMix64 rng) => (rng.Next() >> 11) * (1.0 / 9007199254740992.0);

        /// <summary>HSV hue in degrees.</summary>
        static double Hue(double r, double g, double b)
        {
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            double c = max - min;
            double h;
            if (max == r) h = ((g - b) / c) % 6.0;
            else if (max == g) h = (b - r) / c + 2.0;
            else h = (r - g) / c + 4.0;
            h *= 60.0;
            return h < 0 ? h + 360.0 : h;
        }
    }
}
