// GradeTests: EditMode tests of the NocGrade building blocks (MASTER_PROMPT 14.7 H1, 14.2.4, D-007):
// the void-and-cluster blue-noise tile (permutation, determinism, flat histogram, blue spectrum, < 1 s),
// the R2 tile offsets, and the CPU mirror of the display transform (Neutral curve: identity below 0.76,
// monotonic, hue-preserving, bright neon not clipped; contrast curve; dither amplitude; vignette;
// bloom pyramid plan). Look v2 (LOOK_V2 7): the Floor-relative exposure and vignette (Display(Floor) == Floor at every EV
// and in the corner), the bloom knee 0.25 (no window enters the bloom), the v2 darks below the Neutral start, and the GB-18
// goal lamp core hue (28-48 deg). Thresholds were checked against a numpy simulation of the same algorithms
// (void-and-cluster, seed 20261029: low-band power 0.06 vs white 341, 4x4 block-mean variance 0.00094 vs
// white 0.0053, neighbour correlation -0.29 / -0.26; R2: per-axis counts 62..66 of 64 over 4096 frames).
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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
            int[] a = BlueNoise.GenerateRanks(Tile, NocturneConstants.DitherSigma, BlueNoise.Seed);
            int[] b = BlueNoise.GenerateRanks(Tile, NocturneConstants.DitherSigma, BlueNoise.Seed);
            CollectionAssert.AreEqual(a, b);
            CollectionAssert.AreEqual(a, Ranks);
        }

        [Test]
        public void BlueNoise_GeneratesInUnderOneSecond()
        {
            var sw = Stopwatch.StartNew();
            BlueNoise.GenerateRanks(Tile, NocturneConstants.DitherSigma, BlueNoise.Seed);
            double ms = sw.Elapsed.TotalMilliseconds;
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] blue noise 64x64 void-and-cluster generated in {ms:F0} ms"));
            Assert.Less(ms, 1000.0, "generation must finish in under 1 s (14.7.1)");
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
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] blue-noise low-band power {blue:F3} vs white {white:F1} (expected {whiteExpected:F1})"));
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
            Assert.AreEqual(0.5, NocGradeMath.NeutralGrey(0.5), 0.0);
            for (double x = 0; x < NocGradeMath.NeutralStart; x += 0.001)
                Assert.AreEqual(x, NocGradeMath.NeutralGrey(x), 1e-15);
            // every palette token displays as authored (all are below 0.76 in their peak channel, except the
            // tokens whose peak is above it, which are only compressed, never shifted in hue)
            foreach (NocPal p in Enum.GetValues(typeof(NocPal)))
            {
                Color c = NocturnePalette.Linear(p);
                double peak = Math.Max(c.r, Math.Max(c.g, c.b));
                if (peak >= NocGradeMath.NeutralStart) continue;
                NocGradeMath.Neutral(c.r, c.g, c.b, out double r, out double g, out double b);
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
                double y = NocGradeMath.NeutralGrey(x);
                if (y < prev) Assert.Fail(FormattableString.Invariant($"grey ramp decreases at {x}"));
                if (!(y < 1.0)) Assert.Fail(FormattableString.Invariant($"grey ramp reaches 1 at {x}"));
                if (prev >= 0 && y - prev >= 0.0006) Assert.Fail(FormattableString.Invariant($"jump at {x}"));   // slope <= 1, C0 at 0.76
                prev = y;
            }
            // per channel, along an exposure ramp of every palette colour
            foreach (NocPal p in Enum.GetValues(typeof(NocPal)))
            {
                Color c = NocturnePalette.Linear(p);
                double pr = -1, pg = -1, pb = -1;
                for (double k = 0; k <= 60.0; k += 0.01)
                {
                    NocGradeMath.Neutral(c.r * k, c.g * k, c.b * k, out double r, out double g, out double b);
                    if (r < pr - 1e-12 || g < pg - 1e-12 || b < pb - 1e-12) Assert.Fail(FormattableString.Invariant($"{p} not monotonic at exposure {k}"));
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
                NocGradeMath.Neutral(r, g, b, out double or, out double og, out double ob);
                double cIn = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b));
                double cOut = Math.Max(or, Math.Max(og, ob)) - Math.Min(or, Math.Min(og, ob));
                if (cIn < 1e-6 || cOut < 1e-9) continue;   // achromatic: no hue
                double d = Math.Abs(Hue(r, g, b) - Hue(or, og, ob));
                d = Math.Min(d, 360.0 - d);
                if (!(d < 1e-6)) Assert.Fail(FormattableString.Invariant($"hue shift {d} deg at ({r}, {g}, {b})"));
                // channel order is kept as well (a common scale and a mix toward grey cannot reorder channels)
                if ((r - g) * (or - og) < 0 || (g - b) * (og - ob) < 0) Assert.Fail(FormattableString.Invariant($"channel order changed at ({r}, {g}, {b})"));
            }
        }

        // ============================================================================ contrast, saturation
        [Test]
        public void Contrast_IsMonotonicWithFixedEndsAndPivot()
        {
            double[] ks = { -1.0, -0.5, -0.04, 0.0, 0.04, 0.06, 0.08, 0.5, 1.0 };
            foreach (double k in ks)
            {
                Assert.AreEqual(0.0, NocGradeMath.ContrastCurve(0.0, k), 1e-15);
                Assert.AreEqual(0.5, NocGradeMath.ContrastCurve(0.5, k), 1e-15);
                Assert.AreEqual(1.0, NocGradeMath.ContrastCurve(1.0, k), 1e-15);
                double prev = -1;
                for (double s = 0; s <= 1.0; s += 0.0005)
                {
                    double y = NocGradeMath.ContrastCurve(s, k);
                    Assert.GreaterOrEqual(y, prev - 1e-12, FormattableString.Invariant($"k = {k}, s = {s}"));
                    prev = y;
                }
                const double h = 1e-5;
                double slope = (NocGradeMath.ContrastCurve(0.5 + h, k) - NocGradeMath.ContrastCurve(0.5 - h, k)) / (2 * h);
                Assert.AreEqual(1.0 + k, slope, 1e-6, FormattableString.Invariant($"pivot slope at k = {k}"));
            }
            // the night-sky zenith (code 4) and pure black survive the strongest specified contrast (+8)
            double kMax = NocGradeMath.ContrastK(8);
            double s4 = 4.0 / 255.0;
            Assert.Less(Math.Abs(NocGradeMath.ContrastCurve(s4, kMax) - s4) * 255.0, 0.1);
            Assert.AreEqual(1.0, NocGradeMath.SaturationFactor(0), 0.0);
            Assert.AreEqual(1.04, NocGradeMath.SaturationFactor(4), 1e-12);
        }

        // ============================================================================ dither

        [Test]
        public void Dither_TpdfRemapIsMonotonicSymmetricTriangular()
        {
            double prev = -2;
            for (double u = 0.0005; u < 1.0; u += 0.001)
            {
                double t = NocGradeMath.UniformToTpdf(u);
                Assert.That(t, Is.InRange(-1.0, 1.0));
                Assert.Greater(t, prev, "monotonic (keeps the blue-noise ordering)");
                Assert.AreEqual(-t, NocGradeMath.UniformToTpdf(1.0 - u), 1e-9, "symmetric");
                prev = t;
            }
            // variance of the remapped tile = 1/6 (triangular PDF on [-1, 1]); mean 0
            double sum = 0, sum2 = 0;
            foreach (int r in Ranks)
            {
                double t = NocGradeMath.UniformToTpdf(BlueNoise.RankToUniform(r, Count));
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
            double v = NocturneConstants.Vignette[0];
            double a169 = 16.0 / 9.0;
            Assert.AreEqual(1.0, NocGradeMath.Vignette(0.5, 0.5, a169, v), 1e-12);
            Assert.AreEqual(1.0 - v, NocGradeMath.Vignette(0.0, 0.0, a169, v), 1e-9, "16:9 corner = 1 - intensity");
            Assert.AreEqual(NocGradeMath.Vignette(0.1, 0.2, a169, v), NocGradeMath.Vignette(0.9, 0.8, a169, v), 1e-12);
            double prev = 2;
            for (double u = 0.5; u >= 0.0; u -= 0.01)
            {
                double f = NocGradeMath.Vignette(u, u, a169, v);
                Assert.LessOrEqual(f, prev + 1e-12, "darkens monotonically toward the corner");
                prev = f;
            }
            // Hor+: the same image point (same offset in half-height units) gets the same factor at 21:9 and 16:9
            double a219 = 2560.0 / 1080.0;
            double y = 0.3;
            double px = 0.6;   // half-height units right of centre
            Assert.AreEqual(NocGradeMath.Vignette(0.5 + px / (2 * a169), y, a169, v),
                            NocGradeMath.Vignette(0.5 + px / (2 * a219), y, a219, v), 1e-12);
            // and the farther 21:9 corners stay subtle
            Assert.Greater(NocGradeMath.Vignette(0.0, 0.0, a219, v), 1.0 - 1.25 * v);
        }

        [Test]
        public void Bloom_PyramidIsResolutionNormalised()
        {
            Assert.AreEqual(0, NocGradeMath.BloomSkipLevels(1080));
            Assert.AreEqual(1, NocGradeMath.BloomSkipLevels(2160));
            Assert.AreEqual(0, NocGradeMath.BloomSkipLevels(720));
            Assert.AreEqual(0, NocGradeMath.BloomSkipLevels(1440));
            Assert.AreEqual(NocGradeMath.BloomLevels, NocGradeMath.BloomTotalLevels(1920, 1080, 10));
            Assert.AreEqual(NocGradeMath.BloomLevels + 1, NocGradeMath.BloomTotalLevels(3840, 2160, 10));
            // the first bloom level has the same pixel size at 1080p (level 0) and at 2160p (level 1): 960 x 540
            Assert.AreEqual(1920 >> 1, 3840 >> 2);
            Assert.AreEqual(1080 >> 1, 2160 >> 2);
            // tiny targets still get at least one level, and no level below 2 px
            int t = NocGradeMath.BloomTotalLevels(16, 16, 10);
            Assert.GreaterOrEqual(t, 1);
            Assert.GreaterOrEqual(16 >> t, 2);
            Assert.AreEqual(0.05, NocGradeMath.ScatterLerp(0), 1e-12);
            Assert.AreEqual(0.59, NocGradeMath.ScatterLerp(0.6), 1e-12);
        }

        [Test]
        public void Shader_ConstantsMatchCpuMirror()
        {
            string path = Path.Combine(Application.dataPath, "_Game/Shaders/NocGrade.shader");
            Assert.IsTrue(File.Exists(path), path);
            string src = File.ReadAllText(path);
            StringAssert.Contains("Shader \"Megastructure/NocGrade\"", src);
            // the Neutral start / desaturation arrive as uniforms from NocGradeMath; the vignette shape is mirrored
            Assert.AreEqual(NocGradeMath.VignetteShape, ShaderConst(src, "kNocGradeVigShape"), 1e-9);
            StringAssert.Contains("NocGradeNeutral(c, _NocGradeP2.z, _NocGradeP2.w)", src);
            foreach (string pass in new[] { "NocBloomPrefilter", "NocBloomDown", "NocBloomUp", "NocGrade" })
                StringAssert.Contains("Name \"" + pass + "\"", src);
            StringAssert.DoesNotContain("multi_compile_fog", src);
            // 14.7 H1 per-pixel amplitude and the D-007 chroma component are in the shader with the constants of the CPU mirror
            StringAssert.Contains("lerp(1.0, NC_DitherBrightScale, saturate((lumaD - NC_DitherY0) / NC_DitherYRange))", src);
            StringAssert.Contains("float3(1.5748 * cr, -0.1873 * cb - 0.4681 * cr, 1.8556 * cb)", src);
            StringAssert.Contains("floor(input.positionCS.xy * 0.5)", src);
        }


        // ============================================================================ Nocturne deltas (14.7 H1, D-007, 14.2.4)
        [Test]
        public void Dither_PerPixelAmplitudeOf14_7_H1()
        {
            double peak = NocturneConstants.DitherPeak;
            Assert.AreEqual(2.0, NocGradeMath.DitherAmplitude(peak, 0.0), 1e-12, "dark: 2.0 codes");
            Assert.AreEqual(2.0, NocGradeMath.DitherAmplitude(peak, 0.35), 1e-12, "Y' 0.35: 2.0");
            Assert.AreEqual(1.6, NocGradeMath.DitherAmplitude(peak, 0.55), 1e-12, "Y' 0.55: 1.6");
            Assert.AreEqual(1.2, NocGradeMath.DitherAmplitude(peak, 0.75), 1e-12, "Y' 0.75: 1.2");
            Assert.AreEqual(1.2, NocGradeMath.DitherAmplitude(peak, 1.0), 1e-12, "bright: 1.2");
            Assert.AreEqual(2.5 * 0.6, NocGradeMath.DitherAmplitude(NocturneConstants.DitherLadder, 0.9), 1e-12, "ladder 2.5");
            Assert.AreEqual(0.0, NocGradeMath.DitherAmplitude(0.0, 0.2), 0.0, "-dither off");
            double prev = double.MaxValue;
            for (double y = 0; y <= 1.0; y += 0.001)
            {
                double a = NocGradeMath.DitherAmplitude(peak, y);
                Assert.LessOrEqual(a, prev + 1e-12, "monotone non-increasing at Y' " + y);
                prev = a;
            }
            // the floor colour (Y' about 10 codes) keeps every non-fade pixel 8 codes above black: a 2-code TPDF never clips at 0
            Assert.Greater(NocturneConstants.FloorLuma - 2.0, 7.9);
        }

        [Test]
        public void Dither_ChromaOffsetHasZeroLuma()
        {
            for (int i = -1; i <= 1; i += 2)
                for (int j = -1; j <= 1; j += 2)
                {
                    NocGradeMath.ChromaOffset(i, j, out double r, out double g, out double b);
                    Assert.AreEqual(0.0, NocGradeMath.LumaDisplay(r, g, b), 2e-4, "Rec.709 luma of the chroma dither");
                    double cb = (b - NocGradeMath.LumaDisplay(r, g, b)) / 1.8556, cr = (r - NocGradeMath.LumaDisplay(r, g, b)) / 1.5748;
                    Assert.AreEqual(i, cb, 1e-3, "Cb +-1 code"); Assert.AreEqual(j, cr, 1e-3, "Cr +-1 code");
                }
        }

        [Test]
        public void Dither_StaticByDefaultTemporalAndOff()
        {
            try
            {
                CliArgs.Override(new[] { "x" });
                Assert.AreEqual(DitherMode.Static, CliArgs.Dither);
                Assert.IsTrue(CliArgs.FreezeDither);
                CliArgs.Override(new[] { "x", "-dither", "temporal" });
                Assert.IsFalse(CliArgs.FreezeDither);
                // static: the R2 offset is constant over 1000 frames; temporal: it changes every frame
                R2Sequence.TileOffset(R2Sequence.DitherFrame(0, true), 64, out int x0, out int y0);
                for (long f = 1; f < 1000; f++)
                {
                    R2Sequence.TileOffset(R2Sequence.DitherFrame(f, true), 64, out int x, out int y);
                    Assert.AreEqual(x0, x); Assert.AreEqual(y0, y);
                }
                int changes = 0;
                for (long f = 1; f < 100; f++)
                {
                    R2Sequence.TileOffset(R2Sequence.DitherFrame(f - 1, false), 64, out int xa, out int ya);
                    R2Sequence.TileOffset(R2Sequence.DitherFrame(f, false), 64, out int xb, out int yb);
                    if (xa != xb || ya != yb) changes++;
                }
                Assert.AreEqual(99, changes, "temporal: the offset moves every frame");
            }
            finally { CliArgs.Override(null); }
        }

        [Test]
        public void Bloom_ClampIs30()
        {
            Assert.AreEqual(30.0, NocGradeMath.BloomClamp, 0.0);
            Assert.AreEqual(NocturneConstants.BloomClamp, NocGradeMath.BloomClamp, 0.0);
            // the goal lamp (x3.5) and the sign cores (<= 3.0) stay below the clamp after the strongest exposure (+0.3 EV)
            Assert.Less(3.5 * Math.Pow(2.0, 0.3), NocGradeMath.BloomClamp);
        }

        [Test]
        public void Neutral_PaletteDarksDisplayAsAuthored()
        {
            // every albedo / sky / floor token of LOOK_V2 2.1 / 2.2 is below the Neutral start (the largest is SteelF0 0.527):
            // identity through the tone curve at EV 0 (the Floor-relative exposure is the identity at EV 0)
            foreach (var t in new[] { NocPal.Floor, NocPal.NightZenith, NocPal.NightMid, NocPal.HazeHorizon, NocPal.CloudBase, NocPal.CloudHot,
                                      NocPal.AbyssNear, NocPal.AbyssGlow, NocPal.WetAsphalt, NocPal.ConcreteDark, NocPal.Moss, NocPal.Bark,
                                      NocPal.Plaster, NocPal.Brick, NocPal.Granite, NocPal.MetalPaint, NocPal.SteelF0, NocPal.Leaf, NocPal.GlassDark,
                                      NocPal.CloudCool, NocPal.HazeLow, NocPal.ScreenBase })
            {
                Color c = NocturnePalette.Linear(t);
                Assert.Less(Math.Max(c.r, Math.Max(c.g, c.b)), NocGradeMath.NeutralStart, t.ToString());
                NocGradeMath.Display(c.r, c.g, c.b, 0.0, 0.0, 0.0, 1.0, out double r, out double g, out double b);
                Assert.AreEqual(c.r, r, 1e-6, t.ToString()); Assert.AreEqual(c.g, g, 1e-6, t.ToString()); Assert.AreEqual(c.b, b, 1e-6, t.ToString());
            }
        }

        // ============================================================================ Look v2 grade (LOOK_V2 7)
        [Test]
        public void Display_FloorIsFixedAtEveryEvAndCorner()
        {
            // exposure and vignette act relative to the Floor: a Floor pixel displays as the Floor at EV -0.2 .. +0.3 and in the 16:9
            // corner at the strongest vignette (0.16), so the dithered 1st percentile of a Floor plateau stays >= 9 codes
            double fr = NocturneConstants.FloorLinR, fg = NocturneConstants.FloorLinG, fb = NocturneConstants.FloorLinB;
            double corner = NocGradeMath.Vignette(0.0, 0.0, 16.0 / 9.0, 0.16);
            Assert.AreEqual(0.84, corner, 1e-9);
            foreach (double ev in new[] { -0.2, -0.1, 0.0, 0.1, 0.3 })
                foreach (double vig in new[] { 1.0, corner })
                    foreach (double sat in new[] { 0.0, 4.0 })
                    {
                        NocGradeMath.Display(fr, fg, fb, ev, sat, 0.0, vig, out double r, out double g, out double b);
                        string at = FormattableString.Invariant($"EV {ev}, vignette {vig:F2}, saturation {sat}");
                        // exposure and vignette leave the Floor exactly; saturation +4 (step 2) moves its channels by <= 0.2 code
                        // and its luma by < 0.05 code (B - R of the blue-black grows by 4 %)
                        double tol = sat == 0.0 ? 1e-9 : 0.2;
                        Assert.AreEqual(255.0 * NocGradeMath.LinearToSrgb(fr), 255.0 * NocGradeMath.LinearToSrgb(r), tol, at + " R");
                        Assert.AreEqual(255.0 * NocGradeMath.LinearToSrgb(fg), 255.0 * NocGradeMath.LinearToSrgb(g), tol, at + " G");
                        Assert.AreEqual(255.0 * NocGradeMath.LinearToSrgb(fb), 255.0 * NocGradeMath.LinearToSrgb(b), tol, at + " B");
                        double y = 255.0 * NocGradeMath.LumaDisplay(NocGradeMath.LinearToSrgb(r), NocGradeMath.LinearToSrgb(g), NocGradeMath.LinearToSrgb(b));
                        Assert.AreEqual(NocturneConstants.FloorLuma, y, 0.1, at);
                    }
            // the picture fade still reaches black
            NocGradeMath.Display(fr, fg, fb, -0.2, 0.0, 0.0, corner, 0.0, out double r0, out double g0, out double b0);
            Assert.AreEqual(0.0, r0 + g0 + b0, 0.0, "fade 0 is black");
            // a darker-than-Floor value never goes negative at a positive EV (scene values sit at or above the Floor anyway)
            NocGradeMath.Display(0.0, 0.0, 0.0, 0.3, 0.0, 0.0, 1.0, out double rn, out double gn, out double bn);
            Assert.GreaterOrEqual(Math.Min(rn, Math.Min(gn, bn)), 0.0);
            // above the Floor, exposure still scales the difference by 2^EV
            NocGradeMath.Display(0.1, 0.1, 0.1, 1.0, 0.0, 0.0, 1.0, out double r1, out _, out _);
            Assert.AreEqual(fr + (0.1 - fr) * 2.0, r1, 1e-9);
        }

        [Test]
        public void Shader_FloorRelativeExposureAndVignette()
        {
            string src = File.ReadAllText(Path.Combine(Application.dataPath, "_Game/Shaders/NocGrade.shader"));
            StringAssert.Contains("static const float3 kNocGradeFloor = float3(NC_FloorLinR, NC_FloorLinG, NC_FloorLinB);", src);
            StringAssert.Contains("c = max(kNocGradeFloor + (c - kNocGradeFloor) * _NocGradeP0.x, 0.0) + max(bloom, 0.0) * _NocGradeP0.y;", src);
            StringAssert.Contains("c = max(kNocGradeFloor + (c - kNocGradeFloor) * NocGradeVignette(uvUnit, _NocGradeP1.y, _NocGradeP1.x), 0.0) * _NocGradeP1.z;", src);
            StringAssert.Contains("c = min(max(kNocGradeFloor + (c - kNocGradeFloor) * _NocGradeP0.w, 0.0), _NocGradeP0.z);", src);
        }

        [Test]
        public void Bloom_KneeKeepsEveryWindowOutOfTheBloom()
        {
            // LOOK_V2 7: knee fraction 0.25, so the soft knee starts at exposed 0.675 (0.6375 in beat 5); the brightest window
            // (WinTungsten at b 2.4, Y_med 0.12 luminance-normalised, x 2^0.1 at beat 2) peaks at 0.51 and never blooms
            Assert.AreEqual(0.25, NocGradeMath.BloomKneeFraction, 0.0);
            for (int b = 0; b < 7; b++)
            {
                double thr = NocturneConstants.BloomThreshold[b];
                double kneeStart = thr - thr * NocGradeMath.BloomKneeFraction;
                Assert.AreEqual(b == 4 ? 0.6375 : 0.675, kneeStart, 1e-9, "beat " + (b + 1));
            }
            Color w = NocturnePalette.Linear(NocPal.WinTungsten);
            double y = NocMath.Luma709(w.r, w.g, w.b);
            double peak = Math.Max(w.r, Math.Max(w.g, w.b)) * (0.12 / y) * 2.4 * Math.Pow(2.0, 0.1);
            Assert.AreEqual(0.51, peak, 0.01);
            Assert.Less(peak, 0.675);
            // the lamps and signs bloom: LampWarm x 1.6 and SignWhite x 1.6 reach the knee at EV 0
            Assert.Greater(1.6 * NocturnePalette.Linear(NocPal.LampWarm).r, 0.675);
            Assert.Greater(1.6 * NocturnePalette.Linear(NocPal.SignWhite).b, 0.675);
        }

        [Test]
        public void NeonFlatRegime_CarriesTheToneMappedMean()
        {
            // TQ-08: NocNeon's flat regime multiplies the mean coverage by the Neutral ratio at the stroke peak; its start is the grade's
            Assert.AreEqual(NocGradeMath.NeutralStart, NocturneConstants.NeonToneStart, 0.0);
            Assert.AreEqual(3.0, NocturneConstants.NeonFlatPx, 0.0, "strokes resolve down to 3 px (fade 1.5-3 px)");
            foreach (double pk in new[] { 0.9, 1.6, 2.0, 2.4 })
            {
                double d0 = 1.0 - NocturneConstants.NeonToneStart;
                double tr = (1.0 - d0 * d0 / (pk + d0 - NocturneConstants.NeonToneStart)) / pk;      // the shader's expression
                Assert.AreEqual(NocGradeMath.NeutralGrey(pk) / pk, tr, 1e-9, "peak " + pk);
            }
            string src = File.ReadAllText(Path.Combine(Application.dataPath, "_Game/Shaders/NocNeon.shader"));
            StringAssert.Contains("float tr = pk > NC_NeonToneStart ? (1.0 - d0 * d0 / (pk + d0 - NC_NeonToneStart)) / pk : 1.0;", src);
            StringAssert.Contains("float glyph = lerp(flatCov * tr, cov, w);", src);
        }

        [Test]
        public void GoalLampCore_HueInWindow()
        {
            // GB-18 (D-029): the goal lamp's 6 px core (GoalLamp x 3.5 with its blue x 0.3) displays at hue 28-48 deg at beat 7's
            // grade (EV 0, saturation +4, the Neutral curve's mix toward white); the plain GoalLamp x 3.5 sits at the edge (28.3)
            Color g = NocturnePalette.Linear(NocPal.GoalLamp);
            double e = NocturnePalette.EmissionGoalLamp;
            double ev = NocSchedule.ExposureEV(1500), sat = NocSchedule.Saturation(1500);
            NocGradeMath.Display(g.r * e, g.g * e, g.b * e * NocturneConstants.GoalLampCoreBlue, ev, sat, 0.0, 1.0, out double r, out double gg, out double b);
            double sr = NocGradeMath.LinearToSrgb(r), sg = NocGradeMath.LinearToSrgb(gg), sb = NocGradeMath.LinearToSrgb(b);
            double hue = Hue(sr, sg, sb), luma = 255.0 * NocGradeMath.LumaDisplay(sr, sg, sb);
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] goal lamp core: hue {hue:F1} deg, luma {luma:F0} (GB-18: 28-48)"));
            Assert.That(hue, Is.InRange(NocturneConstants.GoalLampCoreHueMin + 2.0, NocturneConstants.GoalLampCoreHueMax));
            Assert.That(luma, Is.InRange(200.0, 235.0), "LOOK_V2 6: the 6 px core displays 200-235");
            NocGradeMath.Display(g.r * e, g.g * e, g.b * e, ev, sat, 0.0, 1.0, out r, out gg, out b);
            Assert.Less(Hue(NocGradeMath.LinearToSrgb(r), NocGradeMath.LinearToSrgb(gg), NocGradeMath.LinearToSrgb(b)), NocturneConstants.GoalLampCoreHueMin + 1.0,
                        "without the blue cut the core sits at the window's edge (the graybox measured 27.6)");
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
