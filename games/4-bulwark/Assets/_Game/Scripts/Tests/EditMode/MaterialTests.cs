// MaterialTests: the material library and the texture generator (MASTER_PROMPT 8.8, 8.11, 14.2.6, 14.10.1 T-PAL; materials
// owner, Wave A). Covers: BwTexGen sets tile seamlessly, are deterministic, vary smoothness by at least 0.1 (8.11.4), keep
// the palette mean (recolour reference), and their mips are Toksvig-filtered (smoothness and mean normal length never rise
// with the mip level, 8.11.6); the generated texture assets have the declared formats, sRGB flags, mips, sizes <= 2048 and
// fit the VRAM budget (8.11.9); the material contract holds (MaterialFactory.Check); the grating, lane-digit and lantern
// numbers of 14.2.6 / 14.12 and their HLSL twins agree; the "materials" capture set covers beats 1-7. Wave A review fixes:
// the triplanar detuned copy is continuous across every 64-tile wrap (Part 10 #33), smoothness still varies by 0.1 at a
// coarse mip with the macro term (8.11.4), the crust tile is uniform (no baked colonies), the textures are non-readable,
// the lantern peak stays at its clamp.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Megastructure.EditorTools;
using Megastructure.QA;
using Megastructure.Rendering;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Megastructure.Tests
{
    public class MaterialTests
    {
        const int TestSize = 256;   // recipes evaluated at a reduced size (same periods: the tile and the mips stay exact)

        static string Root => Path.GetDirectoryName(Application.dataPath);
        static string ReadAsset(string assetPath) => File.ReadAllText(Path.Combine(Root, assetPath.Replace('/', Path.DirectorySeparatorChar)));

        static readonly Dictionary<string, TexSet> s_sets = new Dictionary<string, TexSet>();
        static TexSet Set(string name)
        {
            if (!s_sets.TryGetValue(name, out var s)) { s = BwTexGen.Generate(name, TestSize); s_sets[name] = s; }
            return s;
        }

        // ------------------------------------------------------------------ generator

        [Test]
        public void TexGen_NoiseIsPeriodicOnTheTile()
        {
            for (int k = 0; k < 50; k++)
            {
                float u = k * 0.0173f, v = k * 0.0291f;
                Assert.AreEqual(BwTexGen.Fbm(u, v, 4, 4, 0.5f, 7u), BwTexGen.Fbm(u + 1f, v, 4, 4, 0.5f, 7u), 1e-4, "fbm u period");
                Assert.AreEqual(BwTexGen.Fbm(u, v, 3, 40, 3, 0.5f, 7u), BwTexGen.Fbm(u, v + 1f, 3, 40, 3, 0.5f, 7u), 1e-4, "anisotropic fbm v period");
                BwTexGen.Worley(u, v, 22, 0.8f, 9u, out float a1, out float a2, out int id0, out _, out _);
                BwTexGen.Worley(u + 1f, v - 1f, 22, 0.8f, 9u, out float b1, out float b2, out int id1, out _, out _);
                Assert.AreEqual(a1, b1, 1e-4, "worley F1 period");
                Assert.AreEqual(a2, b2, 1e-4, "worley F2 period");
                Assert.AreEqual(id0, id1, "worley cell id period");
            }
        }

        [Test]
        public void TexGen_CurlIsPeriodicAndNonTrivial()
        {
            float sum = 0f;
            for (int k = 0; k < 20; k++)
            {
                float u = 0.05f + k * 0.043f, v = 0.11f + k * 0.031f;
                BwTexGen.Curl(u, v, 4, 2, 3u, out float cu, out float cv);
                BwTexGen.Curl(u + 1f, v + 1f, 4, 2, 3u, out float du, out float dv);
                Assert.AreEqual(cu, du, 1e-3f, "curl u period");
                Assert.AreEqual(cv, dv, 1e-3f, "curl v period");
                sum += Mathf.Abs(cu) + Mathf.Abs(cv);
            }
            Assert.Greater(sum, 0.1f, "curl field is not zero");
        }

        [Test]
        public void TexGen_SetsTileSeamlessly([Values("Concrete", "Road", "Steel", "Crust")] string name)
        {
            var s = Set(name);
            Assert.LessOrEqual(BwTexGen.SeamRatio(s.Height, s.Size), 1.0f, name + " height seam");
            Assert.LessOrEqual(BwTexGen.SeamRatio(s.R, s.Size), 1.0f, name + " albedo seam");
            Assert.LessOrEqual(BwTexGen.SeamRatio(s.Smooth, s.Size), 1.0f, name + " smoothness seam");
            Assert.LessOrEqual(BwTexGen.SeamRatio(s.Nx, s.Size), 1.0f, name + " normal seam");
        }

        [Test]
        public void TexGen_IsDeterministic()
        {
            var a = BwTexGen.Generate("Steel", 64);
            var b = BwTexGen.Generate("Steel", 64);
            for (int i = 0; i < a.Height.Length; i++)
            {
                Assert.AreEqual(a.Height[i], b.Height[i], "height " + i);
                Assert.AreEqual(a.Metal[i], b.Metal[i], "chip potential " + i);
            }
        }

        static float Percentile(float[] a, double p)
        {
            var c = (float[])a.Clone();
            Array.Sort(c);
            return c[(int)Math.Round(p * (c.Length - 1))];
        }

        [Test]
        public void TexGen_SmoothnessVariesByAtLeastPoint1([Values("Concrete", "Road", "Steel", "Crust")] string name)
        {
            // 8.11.4: smoothness varies across a large surface by at least 0.1 with spatial noise (5-95 % spread of one tile;
            // the shaders add +-0.06 at 40 m on top)
            var s = Set(name);
            float spread = Percentile(s.Smooth, 0.95) - Percentile(s.Smooth, 0.05);
            Debug.Log($"[BW] {name} smoothness p5..p95 spread {spread:F3}");
            Assert.GreaterOrEqual(spread, 0.1f, name);
            Assert.LessOrEqual(Percentile(s.Smooth, 1.0), 0.95f, name + " smoothness cap 0.95 (8.11.6)");
        }

        [Test]
        public void TexGen_AlbedoMeanStaysOnThePaletteReference([Values("Concrete", "Road", "Steel")] string name)
        {
            // the shaders recolour by target / reference token: the set's mean must sit on its reference (within 6 %)
            var s = Set(name);
            Color refC = BulwarkPalette.Linear(s.AlbedoRef);
            BwTexGen.Stats(s.R, out float r, out _, out _);
            BwTexGen.Stats(s.G, out float g, out _, out _);
            BwTexGen.Stats(s.B, out float b, out _, out _);
            Assert.AreEqual(1.0, r / refC.r, 0.06, name + " R mean / reference");
            Assert.AreEqual(1.0, g / refC.g, 0.06, name + " G mean / reference");
            Assert.AreEqual(1.0, b / refC.b, 0.06, name + " B mean / reference");
        }

        [Test]
        public void TexGen_MipsAreToksvigFiltered([Values("Concrete", "Crust")] string name)
        {
            var s = Set(name);
            var mips = BwTexGen.BuildMips(s);
            Assert.AreEqual(9, mips.Length, "256 -> 1: 9 levels");
            for (int l = 1; l < mips.Length; l++)
            {
                Assert.LessOrEqual(mips[l].MeanNormalLength, mips[l - 1].MeanNormalLength + 1e-4f, $"{name} mean |N| mip {l}");
                Assert.LessOrEqual(mips[l].MeanSmoothness, mips[l - 1].MeanSmoothness + 2f / 255f, $"{name} smoothness mip {l}");
            }
            // the vMF variance of 14.2 rises as the mean normal shortens; a flat field adds nothing
            Assert.AreEqual(0f, BwTexGen.NormalVariance(1f), 1e-6f);
            Assert.Greater(BwTexGen.NormalVariance(0.9f), BwTexGen.NormalVariance(0.99f));
            Assert.Less(BwTexGen.FilterSmoothness(BwTexGen.SmoothToRoughness2(0.6f), BwTexGen.NormalVariance(0.9f)), 0.6f);
        }

        [Test]
        public void TexGen_MipChainFromLevel0MatchesBuildMips([Values("Concrete", "Crust")] string name)
        {
            // the PNG importer keeps level 0 and rebuilds the chain (TextureFactory, BwTexturePostprocessor): level 0 round-trips
            // bit-exactly and every coarser level stays within 2 codes of BuildMips (only the 8-bit quantisation of level 0
            // differs), the Mask's Toksvig smoothness included
            var s = Set(name);
            var mips = BwTexGen.BuildMips(s);
            int size = s.Size;
            var maps = new[] { BwTexGen.MipMap.Albedo, BwTexGen.MipMap.Normal, BwTexGen.MipMap.Mask };
            foreach (var map in maps)
            {
                byte[] l0 = map == BwTexGen.MipMap.Albedo ? mips[0].Albedo : map == BwTexGen.MipMap.Normal ? mips[0].Normal : mips[0].Mask;
                byte[][] chain = BwTexGen.MipChainFromLevel0(size, map, l0, mips[0].Normal);
                Assert.AreEqual(mips.Length, chain.Length, map + " levels");
                CollectionAssert.AreEqual(l0, chain[0], map + " level 0 round trip");
                for (int l = 1; l < mips.Length; l++)
                {
                    byte[] want = map == BwTexGen.MipMap.Albedo ? mips[l].Albedo : map == BwTexGen.MipMap.Normal ? mips[l].Normal : mips[l].Mask;
                    int worst = 0;
                    for (int i = 0; i < want.Length; i++) worst = Math.Max(worst, Math.Abs(want[i] - chain[l][i]));
                    Assert.LessOrEqual(worst, 2, $"{name} {map} mip {l}: max code difference");
                }
            }
            Assert.Throws<ArgumentException>(() => BwTexGen.MipChainFromLevel0(size, BwTexGen.MipMap.Mask, mips[0].Mask, null), "the Mask needs the normals");
        }

        [Test]
        public void TexGen_SmoothnessVariesAtACoarseMipWithTheMacroTerm([Values("Concrete", "Road", "Steel")] string name)
        {
            // 8.11.4 at viewing distance: the tile detail is mip-averaged, so the variation must come from the coarse mip plus
            // the shaders' weathering term: 0.08 BwMacroSigned3(BwFbmFiltered3(p / L)) on walls (L 23 m concrete, 6 m steel
            // and grating), 0.08 BwMacroSigned(BwFbmFiltered(p.xz / 23)) on the road deck. A 40 x 40 m surface seen from
            // 40 m at 1080p (footprint 46 mm) on a 0.5 m grid.
            var s = Set(name);
            var mips = BwTexGen.BuildMips(s);
            int level = Math.Min(mips.Length - 1, 5);                    // 8 x 8 at 256: 1/8 tile per texel
            TexMip m = mips[level];
            float L = name == "Steel" ? 6f : 23f, fp = 0.046f;
            var vals = new List<float>();
            for (int j = 0; j < 80; j++)
                for (int i = 0; i < 80; i++)
                {
                    float x = 0.5f * i + 0.13f, y = 0.5f * j + 0.29f;
                    double u = BwTriTwin.Wrap64(x / s.TileMetres), v = BwTriTwin.Wrap64(y / s.TileMetres);
                    int tx = (int)Math.Floor((u - Math.Floor(u)) * m.Size) % m.Size, ty = (int)Math.Floor((v - Math.Floor(v)) * m.Size) % m.Size;
                    float tex = m.Mask[(ty * m.Size + tx) * 4 + 3] / 255f;
                    float macro = name == "Road"
                        ? BwTexGen.MacroSigned(BwTexGen.ShaderFbm(x / L + 7.7f, y / L + 7.7f, fp / L, 3))
                        : BwTexGen.MacroSigned3(BwTexGen.ShaderFbm3(x / L + 4.1f, y / L + 4.1f, 0.37f + 4.1f, fp / L, 3));
                    vals.Add(Mathf.Clamp01(tex + 0.08f * macro));
                }
            var a = vals.ToArray();
            float spread = Percentile(a, 0.95) - Percentile(a, 0.05);
            Debug.Log($"[BW] {name} smoothness at mip {level} + macro: p5..p95 spread {spread:F3}");
            Assert.GreaterOrEqual(spread, 0.1f, name + ": 8.11.4 at viewing distance");
        }

        [Test]
        public void TexGen_MacroSignedTwinSpansPlusMinusOne()
        {
            // BwMacroSigned stretches the 3-octave fBm (p5..p95 about 0.5 +- 0.24) to about +-1
            var v = new float[20000];
            for (int i = 0; i < v.Length; i++) v[i] = BwTexGen.MacroSigned(BwTexGen.ShaderFbm(i * 0.731f, i * 0.377f + 0.5f * (i % 7), 0f, 3));
            Assert.Less(Percentile(v, 0.05), -0.8f, "p5");
            Assert.Greater(Percentile(v, 0.95), 0.8f, "p95");
            Assert.AreEqual(0f, BwTexGen.MacroSigned(BwTexGen.ShaderFbm(3.3f, 4.4f, 10f, 3)), 1e-6f, "filtered flat far away: no term");
            // the 3D form (walls of any orientation): BwMacroSigned3 of BwFbmFiltered3, p5..p95 about 0.5 +- 0.22
            var w = new float[20000];
            for (int i = 0; i < w.Length; i++) w[i] = BwTexGen.MacroSigned3(BwTexGen.ShaderFbm3(i * 0.731f, i * 0.377f + 0.5f * (i % 7), i * 0.193f + 0.3f * (i % 5), 0f, 3));
            Assert.Less(Percentile(w, 0.05), -0.8f, "3D p5");
            Assert.Greater(Percentile(w, 0.95), 0.8f, "3D p95");
            Assert.AreEqual(0f, BwTexGen.MacroSigned3(BwTexGen.ShaderFbm3(3.3f, 4.4f, 5.5f, 10f, 3)), 1e-6f, "3D: filtered flat far away");
            // the gradient fBm of the thresholded armour masks has the same spread around 0.5 (its thresholds keep their meaning)
            var gq = new float[20000];
            for (int i = 0; i < gq.Length; i++) gq[i] = BwTexGen.ShaderFbmGrad3(i * 0.731f, i * 0.377f + 0.5f * (i % 7), i * 0.193f + 0.3f * (i % 5), 0f, 3);
            Assert.AreEqual(0.5f, Percentile(gq, 0.5), 0.03f, "gradient fBm median");
            Assert.That(Percentile(gq, 0.95) - Percentile(gq, 0.05), Is.InRange(0.34f, 0.54f), "gradient fBm p5..p95 about 0.44");
            Assert.AreEqual(0.5f, BwTexGen.ShaderFbmGrad3(3.3f, 4.4f, 5.5f, 10f, 3), 1e-6f, "gradient fBm filtered flat far away");
        }

        [Test]
        public void TexGen_CrustTileIsUniformAndRanksItsShells()
        {
            // no baked colonies (their 0.5 m period read as a lattice at 45-140 m): shell coverage (height above the film) is
            // even over the quadrants, and every texel carries the rank in (0, 1) of its nearest shell for the shader's
            // world-space colony thinning
            var s = Set("Crust");
            int n = s.Size, h = n / 2;
            var q = new double[4];
            int shells = 0;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float r = s.Metal[y * n + x];
                    Assert.Greater(r, 0f);
                    Assert.Less(r, 1f);
                    if (s.Height[y * n + x] > 0.3f) { q[(y / h) * 2 + x / h] += 1; shells++; }
                }
            double mean = shells / 4.0;
            for (int k = 0; k < 4; k++) Assert.AreEqual(1.0, q[k] / mean, 0.15, "shell coverage of quadrant " + k);
            double cover = shells / (double)(n * n);
            Debug.Log($"[BW] crust shell coverage {cover:F3}");
            Assert.That(cover, Is.InRange(0.3, 0.95), "shells over the film");
        }

        // ------------------------------------------------------------------ triplanar wrap (Part 10 #33)

        static double Frac(double a) => a - Math.Floor(a);
        static double CircDist(double a, double b) { double d = Math.Abs(Frac(a) - Frac(b)); return Math.Min(d, 1.0 - d); }

        [Test]
        public void Triplanar_DetunedCopyIsContinuousAcrossEveryWrap()
        {
            // (k, tile) of every BwTriDetune call: concrete / armour 0.3727 at 2 m, road 0.2941 at 1 m, steel 0.3533 at 1 m,
            // crust 0.77 at 0.5 m. Wrap lines: plane coordinate = 64 k tiles, i.e. x = 0, y = 0 and z = 64 tiles.
            var cases = new[] { (0.3727, 2.0), (0.2941, 1.0), (0.3533, 1.0), (0.77, 0.5) };
            double eps = 1e-5, worstOld = 0.0;
            foreach (var (k, tile) in cases)
            {
                double zWrap = 64.0 * tile;
                var pts = new[] { (0.0, 1.3, 700.0), (0.37, 0.0, 300.0), (2.1, 1.1, zWrap), (-3.0, 0.4, 2.0 * zWrap) };
                for (int axis = 0; axis < 3; axis++)
                    foreach (var (px, py, pz) in pts)
                        for (int dir = 0; dir < 3; dir++)
                        {
                            double ax = dir == 0 ? eps : 0, ay = dir == 1 ? eps : 0, az = dir == 2 ? eps : 0;
                            BwTriTwin.Plane(px - ax, py - ay, pz - az, tile, axis, out double u0, out double v0);
                            BwTriTwin.Plane(px + ax, py + ay, pz + az, tile, axis, out double u1, out double v1);
                            BwTriTwin.Detune(u0, v0, k, 0.31, 0.57, out double a0, out double b0);
                            BwTriTwin.Detune(u1, v1, k, 0.31, 0.57, out double a1, out double b1);
                            double ju = CircDist(BwTriTwin.Wrap64(a0), BwTriTwin.Wrap64(a1)), jv = CircDist(BwTriTwin.Wrap64(b0), BwTriTwin.Wrap64(b1));
                            Assert.Less(Math.Max(ju, jv), 1e-3, $"k {k} tile {tile} axis {axis} at ({px}, {py}, {pz}) dir {dir}: texel jump");
                            BwTriTwin.DetuneWrappedFirst(u0, v0, k, 0.31, 0.57, out double c0, out double d0);
                            BwTriTwin.DetuneWrappedFirst(u1, v1, k, 0.31, 0.57, out double c1, out double d1);
                            worstOld = Math.Max(worstOld, Math.Max(CircDist(c0, c1), CircDist(d0, d1)));
                        }
            }
            Assert.Greater(worstOld, 0.05, "the test sees the pre-fix seam (wrap, then rotate)");
            string core = ReadAsset("Assets/_Game/Shaders/Include/BwLitCore.hlsl");
            var setup = Regex.Match(core, @"BwTri BwTriSetup\([^)]*\)\s*\{(?<b>[^}]*)\}");
            Assert.IsTrue(setup.Success, "BwTriSetup in BwLitCore.hlsl");
            StringAssert.DoesNotContain("floor(", setup.Groups["b"].Value, "BwTriSetup keeps the plane coordinates unwrapped");
            Assert.AreEqual(6, Regex.Matches(core, @"SAMPLE_TEXTURE2D_GRAD\(tex, smp, BwWrap64\(").Count, "every triplanar fetch wraps at the sample");
        }

        [Test]
        public void TexGen_NormalsAreUnit()
        {
            var s = Set("Crust");
            for (int i = 0; i < s.Nx.Length; i += 97)
            {
                float l = Mathf.Sqrt(s.Nx[i] * s.Nx[i] + s.Ny[i] * s.Ny[i] + s.Nz[i] * s.Nz[i]);
                Assert.AreEqual(1f, l, 1e-4f, "normal length " + i);
                Assert.Greater(s.Nz[i], 0f, "normal faces out " + i);
            }
        }

        // ------------------------------------------------------------------ texture assets (8.8, 8.11.9)

        [Test]
        public void TextureAssets_FormatsMipsAndBudget()
        {
            long bytes = 0;
            foreach (var set in BwTexGen.SetNames)
                foreach (TextureFactory.Map map in Enum.GetValues(typeof(TextureFactory.Map)))
                {
                    var tex = TextureFactory.Load(set, map);
                    Assert.IsNotNull(tex, TextureFactory.AssetPath(set, map) + " missing: run RebuildAll");
                    Assert.LessOrEqual(tex.width, 2048, "8.11.9: at most 2k per tile");
                    Assert.AreEqual(TextureFactory.SizeOf(set), tex.width, set + " size");
                    Assert.Greater(tex.mipmapCount, 1, "mipmaps (8.8)");
                    Assert.AreEqual(TextureWrapMode.Repeat, tex.wrapMode, "tileable");
                    bool srgb = GraphicsFormatUtility.IsSRGBFormat(tex.graphicsFormat);
                    Assert.AreEqual(map == TextureFactory.Map.Albedo, srgb, $"{set} {map}: sRGB only for albedo");
                    // the Crust mask is uncompressed: its R channel holds exact per-shell ranks (point-sampled by BwArmour)
                    TextureFormat want = map == TextureFactory.Map.Normal ? TextureFormat.BC5
                                       : (map == TextureFactory.Map.Mask && set == "Crust" ? TextureFormat.RGBA32 : TextureFormat.BC7);
                    Assert.AreEqual(want, tex.format, $"{set} {map} format");
                    Assert.IsFalse(tex.isReadable, $"{set} {map}: non-readable (no CPU copy in the player)");
                    bytes += (long)(tex.width * (long)tex.height * (want == TextureFormat.RGBA32 ? 4 : 1) * 4 / 3);   // BC5 / BC7: 1 byte per texel, RGBA32 4; mips +1/3
                }
            Debug.Log($"[BW] texture sets: {bytes / (1024.0 * 1024.0):F1} MB of VRAM");
            Assert.Less(bytes, 2L * 1024 * 1024 * 1024 / 20, "well under the 2 GB budget (8.11.9): under 100 MB");
        }

        // ------------------------------------------------------------------ material contract (14.2.6, T-PAL)

        [Test]
        public void Materials_ContractHolds()
        {
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(BwPaths.LibraryAsset);
            Assert.IsNotNull(lib, "MaterialLibrary asset: run RebuildAll");
            var problems = new List<string>();
            MaterialFactory.Check(lib, problems);
            Assert.IsEmpty(problems, string.Join("\n", problems));
            // every field has a spec and every spec a field
            var specs = new HashSet<string>();
            foreach (var s in MaterialFactory.Specs()) specs.Add(s.Field);
            foreach (var f in MaterialFactory.LibraryFields()) Assert.IsTrue(specs.Contains(f.Name), "no spec for " + f.Name);
            foreach (var api in new[] { "Concrete", "ConcreteWet", "ConcreteDark", "Rail", "Handrail", "Beam", "Grade" })
                Assert.IsTrue(specs.Contains(api), "framework field renamed: " + api);
            foreach (var wave in new[] { "Road", "Steel", "SteelOchre", "Grating", "Armour", "LampSodium", "LampTungsten", "Lantern" })
                Assert.IsTrue(specs.Contains(wave), "baseline material missing: " + wave);
        }

        [Test]
        public void Materials_LitShadersKeepTheFourPassesAndOneCBuffer()
        {
            foreach (var name in new[] { "BwConcrete", "BwRoad", "BwSteel", "BwGrating", "BwArmour", "BwEmissive", "BwLantern" })
            {
                string src = ReadAsset("Assets/_Game/Shaders/" + name + ".shader");
                Assert.AreEqual(1, Regex.Matches(src, @"CBUFFER_START\(UnityPerMaterial\)").Count, name + ": one UnityPerMaterial cbuffer (SRP Batcher)");
                foreach (var pass in new[] { "\"ForwardLit\"", "\"ShadowCaster\"", "\"DepthOnly\"", "\"DepthNormals\"" })
                    StringAssert.Contains("Name " + pass, src, name + " pass " + pass);
                Assert.AreEqual(4, Regex.Matches(src, @"#pragma\s+multi_compile_instancing").Count, name + ": instancing in every pass");
                StringAssert.Contains("Include/BwLitPass.hlsl", src, name + ": shared lit pass (BwFogApply, BwShade)");
            }
            string lighting = ReadAsset("Assets/_Game/Shaders/Include/BwLighting.hlsl");
            StringAssert.Contains("InitializeBRDFData", lighting, "URP BRDFData");
            StringAssert.Contains("DirectBRDFSpecular", lighting, "URP GGX specular");
            StringAssert.Contains("EnvironmentBRDFSpecular", lighting, "URP environment BRDF with Fresnel");
            StringAssert.Contains("ProjectedSpaceGeometricNormalFiltering", lighting, "geometric specular AA");
            StringAssert.Contains("BwSkyColor", lighting, "analytic environment from the sky");
            StringAssert.Contains("BW_SMOOTHNESS_MAX 0.95", lighting, "smoothness cap");
        }

        [Test]
        public void Materials_GratingAndLampNumbersMatchTheSpec()
        {
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(BwPaths.LibraryAsset);
            Assert.IsNotNull(lib);
            Assert.AreEqual(0.04f, lib.Grating.GetFloat("_BarWidth"), 1e-6f, "bars 40 mm");
            Assert.AreEqual(0.08f, lib.Grating.GetFloat("_BarPitch"), 1e-6f, "pitch 80 mm");
            Assert.AreEqual(3f, lib.Grating.GetFloat("_FadePx"), 1e-6f, "fade below 3 px");
            Assert.AreEqual(1f, lib.Grating.GetFloat("_AlphaToMask"), 1e-6f, "alpha-to-coverage");
            Assert.AreEqual(3f, lib.LampSodium.GetFloat("_Clamp"), 1e-6f, "lamps HDR 3");
            Assert.AreEqual(2.5f, lib.LampSodium.GetFloat("_Intensity"), 1e-6f, "sodium x2.5");
            Assert.AreEqual(3f, lib.LampTungsten.GetFloat("_Intensity"), 1e-6f, "tungsten x3");
            Assert.AreEqual(6f, lib.Lantern.GetFloat("_Intensity"), 1e-6f, "lantern x6");
            Assert.AreEqual(6f, lib.Lantern.GetFloat("_Clamp"), 1e-6f, "lantern clamp 6");
            Assert.AreEqual(0.03f, lib.Lantern.GetFloat("_Flicker"), 1e-6f, "flicker 3 %");
            // the clamp holds at the flicker's peak (14.2.5 lantern 6): base clamp 6 / 1.03, then x gain <= 1.03
            string lantern = ReadAsset("Assets/_Game/Shaders/BwLantern.shader");
            StringAssert.Contains("min(_EmissionColor.rgb * _Intensity, _Clamp / (1.0 + _Flicker)) * BwLanternGain", lantern, "clamp before the gain, scaled by the flicker");
            double peak = 0;
            for (double t = 0; t < 3600.0; t += 0.05) peak = Math.Max(peak, Math.Min(1.0 * 6.0, 6.0 / 1.03) * LanternFlicker.Gain(t));
            Assert.LessOrEqual(peak, 6.0 + 1e-9, "lantern radiance peak");
            StringAssert.Contains("s.metallic = 0.0;", ReadAsset("Assets/_Game/Shaders/BwGrating.shader"), "grating: no compromise metallic (8.11.1)");
        }

        // ------------------------------------------------------------------ road numerals and lantern twins

        [Test]
        public void Road_SevenSegmentTableMatchesTheShader()
        {
            int[] expected = { 6, 2, 5, 5, 4, 5, 6, 3, 7, 6 };
            for (int d = 0; d < 10; d++) Assert.AreEqual(expected[d], RoadMarkings.SegmentCount(d), "segments of " + d);
            string src = ReadAsset("Assets/_Game/Shaders/BwRoad.shader");
            var m = Regex.Match(src, @"BwSevenSeg\[10\]\s*=\s*\{([^}]*)\}");
            Assert.IsTrue(m.Success, "BwSevenSeg table in BwRoad.shader");
            string[] parts = m.Groups[1].Value.Split(',');
            Assert.AreEqual(10, parts.Length);
            for (int d = 0; d < 10; d++) Assert.AreEqual(RoadMarkings.Segments[d], int.Parse(parts[d].Trim().TrimEnd('u')), "digit " + d);
        }

        [Test]
        public void Road_ChainageNumbersEvery100m()
        {
            for (int k = 1; k <= RoadMarkings.LeeCount; k++)
            {
                RoadMarkings.Number(false, k, out double x, out double z, out string text);
                Assert.AreEqual(-3.0, x, 1e-9);
                Assert.AreEqual(100.0 * k, z, 1e-9);
                Assert.AreEqual("0" + k, text, "lee lane shows floor(z / 100)");
                Assert.AreEqual(k, (int)Math.Floor(z / 100.0));
            }
            for (int k = 1; k <= RoadMarkings.SeaCount; k++)
            {
                RoadMarkings.Number(true, k, out double x, out double z, out string text);
                Assert.AreEqual(3.0, x, 1e-9);
                Assert.AreEqual(100.0 * k + 50.0, z, 1e-9);
                Assert.AreEqual(k, (int)Math.Floor(z / 100.0), "sea lane shows floor(z / 100)");
                Assert.Less(z, BulwarkConst.TowerZ0, "sea-lane numerals stay on the road (south of the tower)");
            }
            Assert.AreEqual(0.7, BulwarkConst.LaneDigitW, 1e-9);
            Assert.AreEqual(1.2, BulwarkConst.LaneDigitH, 1e-9);
            Assert.AreEqual(0.15, BulwarkConst.LaneDigitStroke, 1e-9);
        }

        [Test]
        public void Lantern_FlickerIsThreePercentBelowOneHertzAndMatchesTheShader()
        {
            double max = 0;
            for (double t = 0; t < 3600.0; t += 0.05) max = Math.Max(max, Math.Abs(LanternFlicker.Gain(t) - 1.0));
            Assert.LessOrEqual(max, 0.03 + 1e-9, "+-3 %");
            Assert.Greater(max, 0.024, "the flicker reaches close to its 3 % bound");
            foreach (double hz in LanternFlicker.Hz) Assert.LessOrEqual(hz, 1.0, "no modulation above 1 Hz (14.2.5)");
            Assert.AreEqual(LanternFlicker.Gain(0.0), LanternFlicker.Gain(3600.0), 1e-9, "_BwTime wraps at 3600 s without a step");
            string src = ReadAsset("Assets/_Game/Shaders/BwLantern.shader");
            for (int i = 0; i < 3; i++)
            {
                var m = Regex.Match(src, @"#define BW_LANTERN_PHASE" + (i + 1) + @"\s+([0-9.]+)");
                Assert.IsTrue(m.Success, "phase " + (i + 1) + " in BwLantern.shader");
                Assert.AreEqual(LanternFlicker.Phase[i], double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), 1e-9);
            }
        }

        // ------------------------------------------------------------------ material sheets (8.11.8)

        [Test]
        public void MaterialSheets_CoverEveryBeatOffThePath()
        {
            var plan = ShotPlan.Build("materials");
            Assert.IsEmpty(plan.unknownSets);
            var beats = new HashSet<int>();
            foreach (var d in MaterialSheet.Sheets)
            {
                beats.Add(d.Beat);
                Assert.AreEqual(d.Beat, BwPath.BeatAt(d.S), d.Name + ": its s lies in its beat (the beat's key light and sky)");
                Assert.Less(d.X, BulwarkConst.LeeFaceX - 40.0, d.Name + ": off the path (west of the lee face)");
            }
            for (int b = 1; b <= 7; b++) Assert.IsTrue(beats.Contains(b), "sheet for beat " + b);
            int stills = 0;
            foreach (var st in plan.stills) { if (st.set == "materials") stills++; Assert.LessOrEqual(st.s, ShotPlan.MaxPoseS); }
            Assert.GreaterOrEqual(stills, 4 * MaterialSheet.Sheets.Length);
            Assert.AreEqual(8, plan.sequences.Count, "pans and steps over the road, the gratings and the crust wall");
            foreach (var sq in plan.sequences) Assert.AreEqual("materials", sq.set, sq.name);
            var ctx = ShotPlan.Build("matcontext");
            Assert.IsEmpty(ctx.unknownSets);
            Assert.AreEqual(0, ctx.stills.Count);
            Assert.AreEqual(5, ctx.sequences.Count, "KS05 / KS06 pans, KS06 dolly, gallery pan and dolly");
            foreach (var sq in ctx.sequences)
            {
                Assert.AreEqual("matcontext", sq.set, sq.name);
                Assert.IsFalse(sq.advanceAmbient, sq.name + ": ambient frozen");
                sq.FramePose(0, out _, out double x0, out _, out double z0, out _, out _, out _);
                sq.FramePose(sq.frames - 1, out _, out double x1, out _, out double z1, out _, out _, out _);
                double moved = Math.Sqrt((x1 - x0) * (x1 - x0) + (z1 - z0) * (z1 - z0));
                Assert.AreEqual(sq.mode == SeqMode.MovingPose ? MaterialSheet.DollyMetres : 0.0, moved, 1e-6, sq.name + ": dolly length");
            }
            Assert.IsFalse(MaterialSheet.Requested("QA/x", "matcontext"), "matcontext never builds the rig");
            Assert.AreEqual(0, ShotPlan.Build("everything").sequences.FindAll(q => q.set == "matcontext" || q.set == "materials").Count);
            Assert.IsTrue(MaterialSheet.Requested("QA/x", "keyshots,materials"));
            Assert.IsFalse(MaterialSheet.Requested("QA/x", "keyshots,all"));
            Assert.IsFalse(MaterialSheet.Requested(null, "materials"));
            // "all" / "everything" never build the rig
            Assert.AreEqual(0, ShotPlan.Build("everything").stills.FindAll(s => s.set == "materials").Count);
        }

        [Test]
        public void MaterialSheets_LeaveTheSurfaceStateToTheDriverAndPoolTheBoothLamps()
        {
            // Validate row "one writer of _BwSurface" (Wave B): only AtmosphereDriver publishes the surface state
            var writers = BatchTools.GlobalWriters("_BwSurface");
            Assert.AreEqual(1, writers.Count, "writers of _BwSurface: " + string.Join(", ", writers));
            StringAssert.EndsWith("/Runtime/Systems/AtmosphereDriver.cs", writers[0]);
            // 14.2.3: the booth lamps are LocalLightPool anchors, never Light components of the sheet
            string src = ReadAsset("Assets/_Game/Beats/QA_MaterialSheet/QaMaterialSheet.cs");
            StringAssert.DoesNotContain("AddComponent<Light>", src);
            StringAssert.Contains("LocalLightPool.Ensure(ctx)", src);
            // the booth interior boxes (for the region ambient): inner faces round the stands and the lamps, off the path,
            // apart from the real interiors; inactive outside a "materials" capture
            var real = BulwarkSchedule.InteriorBoxes;
            int booths = 0;
            foreach (var d in MaterialSheet.Sheets)
            {
                bool has = MaterialSheet.BoothBox(d, out D3 lo, out D3 hi);
                Assert.AreEqual(d.Booth, has, d.Name);
                if (!has) continue;
                booths++;
                Assert.AreEqual(MaterialSheet.BoothHeight, hi.y - lo.y, 1e-9, d.Name + ": floor to ceiling");
                Assert.Less(hi.x, BulwarkConst.LeeFaceX - 10.0, d.Name + ": west of the lee face (off the path)");
                MaterialSheet.Frame(d.Yaw, out double fx, out double fz, out double rx, out double rz);
                for (int i = 0; i < d.Items.Length; i++)
                {
                    MaterialSheet.ItemCentre(d, i, out double x, out double y, out double z);
                    Assert.IsTrue(x > lo.x && x < hi.x && z > lo.z && z < hi.z && y >= lo.y && y < hi.y, d.Name + ": stand " + i + " inside the booth");
                }
                for (int k = 0; k < d.LampCount; k++)
                {
                    double lx = d.LampCount == 1 ? -1.6 : (k == 0 ? -3.2 : 3.2), lz = -2.2;
                    double x = d.X + rx * lx + fx * lz, y = d.Y + d.LampHeight, z = d.Z + rz * lx + fz * lz;
                    Assert.IsTrue(x > lo.x && x < hi.x && z > lo.z && z < hi.z && y > lo.y && y < hi.y, d.Name + ": lamp " + k + " inside the booth");
                }
                for (int b = 0; b + 5 < real.Length; b += 6)
                {
                    bool overlap = lo.x < real[b + 3] && hi.x > real[b] && lo.y < real[b + 4] && hi.y > real[b + 1] && lo.z < real[b + 5] && hi.z > real[b + 2];
                    Assert.IsFalse(overlap, d.Name + ": booth box apart from interior box " + (b / 6));
                }
                Assert.IsFalse(MaterialSheet.BoothBoxAt(d.S, out _, out _), d.Name + ": no booth box outside a 'materials' capture");
            }
            Assert.AreEqual(4, booths, "booths of beats 1, 5, 6 and the tunnel");
        }

        // ------------------------------------------------------------------ region ambient probe (BwLighting.hlsl, Wave B)

        /// <summary>C# twin of BwLighting.hlsl BwInteriorProbe (the probe refinement inside an interior box).</summary>
        static D3 InteriorProbeTwin(D3 q, double e)
        {
            double[] b = BulwarkSchedule.InteriorBoxes;
            double best = 0.0;
            D3 lo = q, hi = q;
            for (int k = 0; k + 5 < b.Length; k += 6)
            {
                double ix = Math.Min(Sat((q.x - b[k]) / (2 * e) + 0.5), Sat((b[k + 3] - q.x) / (2 * e) + 0.5));
                double iy = Math.Min(Sat((q.y - b[k + 1]) / (2 * e) + 0.5), Sat((b[k + 4] - q.y) / (2 * e) + 0.5));
                double iz = Math.Min(Sat((q.z - b[k + 2]) / (2 * e) + 0.5), Sat((b[k + 5] - q.z) / (2 * e) + 0.5));
                double wk = ix * iy * iz;
                if (wk > best)
                {
                    best = wk;
                    lo = new D3(b[k] + e, b[k + 1] + e, b[k + 2] + e);
                    hi = new D3(Math.Max(lo.x, b[k + 3] - e), Math.Max(lo.y, b[k + 4] - e), Math.Max(lo.z, b[k + 5] - e));
                }
            }
            double u = Sat((best - 0.5) / 0.25), t = u * u * (3.0 - 2.0 * u);
            var c = new D3(Math.Min(Math.Max(q.x, lo.x), hi.x), Math.Min(Math.Max(q.y, lo.y), hi.y), Math.Min(Math.Max(q.z, lo.z), hi.z));
            return q + (c - q) * t;
        }

        static double Sat(double x) => x < 0.0 ? 0.0 : (x > 1.0 ? 1.0 : x);

        [Test]
        public void RegionAmbient_ProbeKeepsInteriorJunctionsInteriorAndTheExteriorUntouched()
        {
            string src = ReadAsset("Assets/_Game/Shaders/Include/BwLighting.hlsl");
            StringAssert.Contains("float3 BwInteriorProbe(float3 q)", src);
            StringAssert.Contains("return BwInteriorProbe(positionWS + normalGeoWS * max(_BwAmbInt.w, 0.0));", src);
            StringAssert.Contains("smoothstep(0.5, 0.75, best)", src);
            var g = default(AtmoGlobals);
            g.IntEdge = BulwarkSchedule.InteriorEdge;
            double e = g.IntEdge;
            D3 Probe(D3 p, D3 n) => InteriorProbeTwin(p + n * e, e);
            double Raw(D3 p, D3 n) => BwAtmosphereMath.InteriorWeightAt(in g, p + n * e);
            double New(D3 p, D3 n) => BwAtmosphereMath.InteriorWeightAt(in g, Probe(p, n));
            var up = new D3(0, 1, 0); var east = new D3(1, 0, 0); var west = new D3(-1, 0, 0);
            double xIn0 = BulwarkConst.ShellXMin + 0.6, xIn1 = BulwarkConst.ShellXMax - 0.6;   // inner wall faces -3.5 / 0.5
            // the gallery junctions (the int-03 KS01 band): raw weight below 1, refined weight 1
            var wallFoot = new D3(xIn0, 0.02, 60.0);
            var floorEdge = new D3(xIn0 + 0.05, 0.0, 60.0);
            Assert.Less(Raw(wallFoot, east), 0.9, "the raw probe of the wall foot leaks");
            Assert.Less(Raw(floorEdge, up), 0.95, "the raw probe of the floor edge leaks");
            Assert.AreEqual(1.0, New(wallFoot, east), 1e-9, "wall foot: interior");
            Assert.AreEqual(1.0, New(floorEdge, up), 1e-9, "floor edge: interior");
            Assert.AreEqual(1.0, New(new D3(xIn1, 3.58, 60.0), west), 1e-9, "east wall top: interior");
            // the exterior stays the atmosphere owner's: the deck beside the gallery, its outer wall, its roof, the road
            // beyond the door, the ledge beside the tunnel mouth
            var ext = new[] { (new D3(1.3, 0.0, 60.0), up), (new D3(BulwarkConst.ShellXMax, 2.0, 60.0), east),
                              (new D3(-1.5, BulwarkConst.ShellYMax, 60.0), up), (new D3(-1.5, 0.0, 120.8), up),
                              (new D3(-6.3, -90.0, 1430.0), up) };
            foreach (var (p, n) in ext)
            {
                double r = Raw(p, n);
                Assert.LessOrEqual(r, 0.5, "exterior point " + p);
                Assert.AreEqual(r, New(p, n), 1e-12, "exterior point " + p + " unchanged");
            }
            // continuity through the door plane (floor, z 118..123) and across the gallery's west shell (y 2, x -6..-2.5):
            // the refined weight never steps more than 0.01 per mm, and equals 0.5 on the box face
            double worst = 0.0, prev = New(new D3(-1.5, 0.0, 118.0), up);
            for (int i = 1; i <= 5000; i++)
            {
                double w = New(new D3(-1.5, 0.0, 118.0 + i * 0.001), up);
                worst = Math.Max(worst, Math.Abs(w - prev)); prev = w;
            }
            Assert.Less(worst, 0.01, "door plane: refined weight continuous");
            Assert.AreEqual(0.5, New(new D3(-1.5, 0.0, BulwarkConst.DoorPlaneZ), up), 1e-9, "0.5 on the door plane");
            worst = 0.0; prev = New(new D3(-6.0, 2.0, 60.0), new D3(0, 0, 0));
            for (int i = 1; i <= 3500; i++)
            {
                double w = New(new D3(-6.0 + i * 0.001, 2.0, 60.0), new D3(0, 0, 0));
                worst = Math.Max(worst, Math.Abs(w - prev)); prev = w;
            }
            Assert.Less(worst, 0.01, "shell face: refined weight continuous");
        }

        /// <summary>Exact form factor from a point on the axis of a parallel a x b rectangle at distance d (facing it).</summary>
        static double AxisRectFormFactor(double a, double b, double d)
        {
            double x = a / (2.0 * d), y = b / (2.0 * d), sx = Math.Sqrt(1.0 + x * x), sy = Math.Sqrt(1.0 + y * y);
            return 2.0 / Math.PI * (x / sx * Math.Atan(y / sx) + y / sy * Math.Atan(x / sy));
        }

        [Test]
        public void OpeningLight_IsTheFormFactorOfTheOpeningAndContinuous()
        {
            string src = ReadAsset("Assets/_Game/Shaders/Include/BwLighting.hlsl");
            StringAssert.Contains("float4 _BwOpen[8];", src);
            StringAssert.Contains("BwInteriorWeight(q) * BwOpeningLight(normalWS, positionWS)", src);
            StringAssert.Contains("saturate((c.x - p.z) * c.y / 0.25) * saturate(1.0 + p.y - r.z)", src);
            StringAssert.Contains("BwRegionAmbient(inputData.normalWS, f.positionWS, f.normalGeoWS)", ReadAsset("Assets/_Game/Shaders/Include/BwLitPass.hlsl"));
            // the beat-1 door fully open: 4.0 x 3.4 m in the plane z 120, exterior to the north, reach 40 m
            var rect = new Vector4(-3.5f, 0.5f, 0f, 3.4f);
            var plane = new Vector4(120f, 1f, 40f, 1f);
            var north = new D3(0, 0, 1);
            // on the axis, facing it: Lambert's polygon formula equals the closed form of a parallel rectangle
            foreach (double d in new[] { 0.5, 2.0, 8.0, 20.0 })
            {
                double F = LocalLightPool.OpeningFactor(rect, plane, north, new D3(-1.5, 1.7, 120.0 - d), out D3 phi);
                Assert.AreEqual(AxisRectFormFactor(4.0, 3.4, d), F, 1e-6, "on-axis form factor at " + d + " m");
                Assert.Greater(phi.z, 0.0, "the vector irradiance points out through the door");
            }
            Assert.AreEqual(0.0, LocalLightPool.OpeningFactor(rect, plane, north, new D3(-1.5, 1.7, 121.0), out _), "exterior side: none");
            Assert.AreEqual(0.0, LocalLightPool.OpeningFactor(rect, plane, north, new D3(-1.5, 1.7, 60.0), out _), "beyond the reach: none");
            Assert.AreEqual(0.0, LocalLightPool.OpeningFactor(rect, plane, north, new D3(-1.5, -1.5, 110.0), out _), "1 m under the sill: none");
            Assert.AreEqual(0.0, LocalLightPool.OpeningFactor(rect, new Vector4(120f, 1f, 40f, 0f), north, new D3(-1.5, 1.7, 110.0), out _), "door shut: none");
            Assert.AreEqual(0.0, LocalLightPool.OpeningFactor(rect, plane, new D3(0, 0, -1), new D3(-1.5, 1.7, 110.0), out _), "facing away: none");
            // a wall beside the door (west wall x -3.5 facing east) and the floor in front of it get light; the tower portal
            // (exterior to the south) lights a point north of its plane
            Assert.Greater(LocalLightPool.OpeningFactor(rect, plane, new D3(1, 0, 0), new D3(-3.5, 1.5, 118.0), out _), 0.15, "west wall 2 m from the door (F 0.172)");
            Assert.Greater(LocalLightPool.OpeningFactor(rect, plane, new D3(0, 1, 0), new D3(-1.5, 0.0, 117.0), out _), 0.08, "floor 3 m from the door (F 0.0997)");
            var portal = new Vector4(-3.3f, 0.3f, 0f, 3.4f);
            Assert.Greater(LocalLightPool.OpeningFactor(portal, new Vector4(920f, -1f, 40f, 1f), new D3(0, 0, -1), new D3(-1.5, 1.7, 926.0), out D3 pp), 0.09, "KS13 eye facing the portal (F 0.097)");
            Assert.Less(pp.z, 0.0, "the portal's light comes from the south");
            // continuity along the floor through the door plane and along the gallery to the reach (1 mm / 1 cm steps)
            double worst = 0.0, prev = LocalLightPool.OpeningFactor(rect, plane, new D3(0, 1, 0), new D3(-1.5, 0.0, 118.0), out _);
            for (int i = 1; i <= 4000; i++)
            {
                double f = LocalLightPool.OpeningFactor(rect, plane, new D3(0, 1, 0), new D3(-1.5, 0.0, 118.0 + i * 0.001), out _);
                worst = Math.Max(worst, Math.Abs(f - prev)); prev = f;
            }
            Assert.Less(worst, 0.01, "through the door plane");
            worst = 0.0; prev = LocalLightPool.OpeningFactor(rect, plane, new D3(1, 0, 0), new D3(-3.5, 1.5, 119.0), out _);
            for (int i = 1; i <= 4500; i++)
            {
                double f = LocalLightPool.OpeningFactor(rect, plane, new D3(1, 0, 0), new D3(-3.5, 1.5, 119.0 - i * 0.01), out _);
                worst = Math.Max(worst, Math.Abs(f - prev)); prev = f;
            }
            Assert.Less(worst, 0.01, "along the west wall to the reach");
        }
    }
}
