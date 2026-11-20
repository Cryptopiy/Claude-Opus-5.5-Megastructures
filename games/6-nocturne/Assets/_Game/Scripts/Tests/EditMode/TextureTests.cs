// TextureTests: the processed Look v2 texture sets of TP-1 (docs/TEXTURE_PLAN.md 8.3 TP-1 tests), data-driven over every
// <Key>.texmeta.json under Assets/_Game/Textures/External (written by Tools/texprep.py):
//   import: formats (BC7 sRGB albedo, BC5 normal, BC7 linear mask) and sRGB flags, mips with the manifest's mip filter, aniso
//           (effective max(9, level) under QualitySettings ForceEnable), streaming off, mip bias 0, not readable, size = the
//           manifest, arrays = Texture2DArray of cols x rows slices (the normal-map flipbook spike of TEXTURE_PLAN 1.5 included),
//           the imported last mip's linear mean 0.5 +- 0.01 per channel (read at the 4 x 4 mip, one BC block) (alpha sets: over the whole slice, +- 0.05, since the
//           dilated texels count there), Toksvig: mip 0 smoothness = the PNG's, the mean smoothness never increases with the mip;
//   set rules: tile = the TEXTURES.md tile +- 15 % or a registration, metallic only in the four allow-listed sets, contrast at the
//           class midpoint (+- 0.02), mean saturation under its cap, violet share <= 0.5 %, the content gate, coverage
//           preservation on alpha sets;
//   budget: texture VRAM from the formats <= 900 MiB (TEXTURE_PLAN 9).
// The imported pixels are read on the GPU: Graphics.CopyTexture of one mip / slice into a Texture2D of the same format, then a
// blit into a float RenderTexture (the importer keeps every External texture non-readable).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    public class TextureTests
    {
        static readonly string[] MetalAllow = { "ambientcg_MetalPlates013", "ambientcg_MetalWalkway006", "polyhaven_corrugated_iron_02", "ambientcg_DiamondPlate008C" };

        struct Entry { public string metaPath; public TexMeta meta; public string prefix; }

        static List<Entry> Entries()
        {
            var l = new List<Entry>();
            foreach (var mp in NocTextureImport.AllManifests())
            {
                var m = TexMeta.Load(NocPaths.Abs(mp));
                l.Add(new Entry { metaPath = mp, meta = m, prefix = mp.Substring(0, mp.Length - TexMeta.Suffix.Length) });
            }
            return l;
        }

        static IEnumerable<(Entry e, ExtMap map, string path)> Maps()
        {
            foreach (var e in Entries())
            {
                if (e.meta == null) continue;
                foreach (ExtMap map in new[] { ExtMap.Albedo, ExtMap.Normal, ExtMap.Mask })
                    if (e.meta.HasMap(map)) yield return (e, map, e.prefix + "_" + map + ".png");
            }
        }

        [Test]
        public void Manifests_AreReadableAndComplete()
        {
            var es = Entries();
            TestContext.WriteLine(es.Count + " processed set(s) / array(s)");
            foreach (var e in es)
            {
                Assert.IsNotNull(e.meta, e.metaPath + " readable");
                Assert.AreEqual(Path.GetFileName(e.prefix), e.meta.key, e.metaPath + " key = file name");
                Assert.Greater(e.meta.res, 0, e.metaPath + " res");
                Assert.Greater(e.meta.tile, 0f, e.metaPath + " tile");
                Assert.IsNotNull(e.meta.maps, e.metaPath + " maps");
                foreach (ExtMap map in new[] { ExtMap.Albedo, ExtMap.Normal, ExtMap.Mask })
                    if (e.meta.HasMap(map)) Assert.IsTrue(File.Exists(NocPaths.Abs(e.prefix + "_" + map + ".png")), e.prefix + "_" + map + ".png exists");
                if (e.meta.IsArray && e.meta.slices != null && e.meta.slices.Length > 0)
                    Assert.LessOrEqual(e.meta.slices.Length, e.meta.Depth, e.metaPath + " slices fit the flipbook");
                if (e.meta.placeholder) TestContext.WriteLine("placeholder (texprep --placeholders): " + e.meta.key);
            }
        }

        [Test]
        public void Import_FormatsSizesAndSettings()
        {
            bool force = QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable;
            int n = 0;
            foreach (var (e, map, path) in Maps())
            {
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.IsNotNull(imp, path + " imported");
                var x = NocTextureImport.ExpectedFor(map, e.meta);
                var ps = imp.GetPlatformTextureSettings("Standalone");
                var st = new TextureImporterSettings();
                imp.ReadTextureSettings(st);
                Assert.AreEqual(x.type, imp.textureType, path + " type");
                Assert.AreEqual(x.srgb, imp.sRGBTexture, path + " sRGB");
                Assert.IsTrue(ps.overridden, path + " Standalone override");
                Assert.AreEqual(x.format, ps.format, path + " Standalone format");
                Assert.IsTrue(imp.mipmapEnabled, path + " mips");
                Assert.AreEqual(x.mipFilter, imp.mipmapFilter, path + " mip filter");
                Assert.AreEqual(x.aniso, imp.anisoLevel, path + " anisoLevel");
                int effective = force ? Math.Max(NocTexConstants.TexAnisoForcedMin, imp.anisoLevel) : imp.anisoLevel;
                Assert.GreaterOrEqual(effective, NocTexConstants.TexAnisoForcedMin, path + " effective aniso");
                Assert.AreEqual(0f, imp.mipMapBias, 1e-6f, path + " mip bias");
                Assert.IsFalse(imp.streamingMipmaps, path + " streaming");
                Assert.IsFalse(imp.isReadable, path + " readable");
                Assert.AreEqual(x.preserveCoverage, imp.mipMapsPreserveCoverage, path + " preserve coverage");
                Assert.AreEqual(TextureWrapMode.Repeat, imp.wrapMode, path + " wrap");
                Assert.AreEqual(FilterMode.Trilinear, imp.filterMode, path + " trilinear");
                var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                Assert.IsNotNull(tex, path + " loads");
                Assert.AreEqual(e.meta.res, tex.width, path + " width = manifest res");
                Assert.AreEqual(e.meta.res, tex.height, path + " height = manifest res (square slices)");
                Assert.AreEqual(Mathf.RoundToInt(Mathf.Log(e.meta.res, 2)) + 1, tex.mipmapCount, path + " full mip chain");
                if (e.meta.IsArray)
                {
                    var arr = tex as Texture2DArray;
                    Assert.IsNotNull(arr, path + " imports as a Texture2DArray (flipbook " + e.meta.cols + " x " + e.meta.rows + ")");
                    Assert.AreEqual(e.meta.Depth, arr.depth, path + " array depth");
                }
                else Assert.IsInstanceOf<Texture2D>(tex, path + " is a Texture2D");
                n++;
            }
            TestContext.WriteLine(n + " External textures checked");
        }

        // ---------------------------------------------------------------------------------------------------------- GPU reads

        /// <summary>Linear RGBA pixels of mip `mip` of slice `slice` (0 for a Texture2D) of an imported texture.</summary>
        static Color[] ReadMip(Texture tex, int slice, int mip, out int w, out int h)
        {
            w = Math.Max(1, tex.width >> mip);
            h = Math.Max(1, tex.height >> mip);
            var tmp = new Texture2D(w, h, tex.graphicsFormat, TextureCreationFlags.None) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var rt = RenderTexture.GetTemporary(new RenderTextureDescriptor(w, h, RenderTextureFormat.ARGBFloat, 0) { sRGB = false, useMipMap = false, msaaSamples = 1 });
            var cpu = new Texture2D(w, h, TextureFormat.RGBAFloat, false, true);
            var prev = RenderTexture.active;
            try
            {
                Graphics.CopyTexture(tex, slice, mip, tmp, 0, 0);
                Graphics.Blit(tmp, rt);
                RenderTexture.active = rt;
                cpu.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
                cpu.Apply(false);
                return cpu.GetPixels();
            }
            finally
            {
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(tmp);
                UnityEngine.Object.DestroyImmediate(cpu);
            }
        }

        static Vector4 Mean(Color[] px)
        {
            double r = 0, g = 0, b = 0, a = 0;
            foreach (var c in px) { r += c.r; g += c.g; b += c.b; a += c.a; }
            double n = Math.Max(1, px.Length);
            return new Vector4((float)(r / n), (float)(g / n), (float)(b / n), (float)(a / n));
        }

        [Test]
        public void Import_AlbedoLastMipMeanIsHalfLinear()
        {
            int n = 0;
            foreach (var (e, map, path) in Maps())
            {
                if (map != ExtMap.Albedo || e.meta.placeholder) continue;
                var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                Assert.IsNotNull(tex, path);
                // the 4 x 4 mip (one BC block; its mean is the last mip's under the box and Kaiser chains to < 1e-3): a 1 x 1 or
                // 2 x 2 BC texture cannot be created as the CopyTexture target
                int last = Math.Max(0, tex.mipmapCount - 3);
                int depth = e.meta.IsArray ? e.meta.Depth : 1;
                int slices = e.meta.slices != null && e.meta.slices.Length > 0 ? Math.Min(depth, e.meta.slices.Length) : depth;
                double tol = e.meta.alpha ? 0.05 : NocTexConstants.TexMeanTol;
                for (int s = 0; s < slices; s++)
                {
                    var m = Mean(ReadMip(tex, s, last, out _, out _));
                    string what = FormattableString.Invariant($"{path} slice {s} last mip mean ({m.x:0.0000}, {m.y:0.0000}, {m.z:0.0000})");
                    TestContext.WriteLine(what);
                    Assert.AreEqual(0.5, m.x, tol, what);
                    Assert.AreEqual(0.5, m.y, tol, what);
                    Assert.AreEqual(0.5, m.z, tol, what);
                    n++;
                }
            }
            TestContext.WriteLine(n + " albedo slices checked");
        }

        [Test]
        public void Import_ToksvigKeepsMip0_AndSmoothnessNeverIncreases()
        {
            int n = 0;
            foreach (var (e, map, path) in Maps())
            {
                if (map != ExtMap.Mask || e.meta.placeholder) continue;
                var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                Assert.IsNotNull(tex, path);
                // the PNG on disk (mip 0 as texprep wrote it), alpha mean over the whole flipbook / set
                var png = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                double pngA;
                try
                {
                    Assert.IsTrue(png.LoadImage(File.ReadAllBytes(NocPaths.Abs(path)), false), path + " decodes");
                    double sum = 0;
                    var p32 = png.GetPixels32();
                    foreach (var c in p32) sum += c.a;
                    pngA = sum / Math.Max(1, p32.Length) / 255.0;
                }
                finally { UnityEngine.Object.DestroyImmediate(png); }
                int depth = e.meta.IsArray ? e.meta.Depth : 1;
                double mip0 = 0;
                var means = new double[Math.Max(1, tex.mipmapCount - 2)];              // down to the 4 x 4 mip (one BC block)
                for (int s = 0; s < depth; s++)
                    for (int mip = 0; mip < means.Length; mip++)
                        means[mip] += Mean(ReadMip(tex, s, mip, out _, out _)).w / depth;
                mip0 = means[0];
                string rep = path + " smoothness mean per mip: " + string.Join(" ", Array.ConvertAll(means, v => v.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)));
                TestContext.WriteLine(rep + FormattableString.Invariant($" (PNG {pngA:0.0000})"));
                Assert.AreEqual(pngA, mip0, 2.0 / 255.0, path + " mip 0 smoothness = the PNG's (Toksvig leaves mip 0 alone)");
                // tolerance 2 / 255: BC7 re-encodes every mip (a 4 x 4 mip is one block; SteelPlates' measured +0.0053 at mip 9)
                for (int mip = 1; mip < means.Length; mip++)
                    Assert.LessOrEqual(means[mip], means[mip - 1] + 2.0 / 255.0, rep + ": smoothness never increases (mip " + mip + ")");
                n++;
            }
            TestContext.WriteLine(n + " masks checked");
        }

        // ---------------------------------------------------------------------------------------------------------- set rules

        static IEnumerable<(Entry e, int slice, string source, float tile, float tileRef, string reg, TexStats st)> Units()
        {
            foreach (var e in Entries())
            {
                if (e.meta == null || e.meta.placeholder) continue;
                if (e.meta.slices != null && e.meta.slices.Length > 0 && e.meta.sliceStats != null && e.meta.sliceStats.Length == e.meta.slices.Length)
                {
                    for (int k = 0; k < e.meta.slices.Length; k++)
                        yield return (e, k, e.meta.slices[k], e.meta.SliceTileU(k),
                                      e.meta.sliceTileRef != null && k < e.meta.sliceTileRef.Length ? e.meta.sliceTileRef[k] : e.meta.tileRef,
                                      e.meta.sliceRegistration != null && k < e.meta.sliceRegistration.Length ? e.meta.sliceRegistration[k] : "", e.meta.sliceStats[k]);
                }
                else yield return (e, -1, e.meta.source, e.meta.tile, e.meta.tileRef, e.meta.registration, e.meta.stats);
            }
        }

        static string Name((Entry e, int slice, string source, float tile, float tileRef, string reg, TexStats st) u)
            => u.e.meta.key + (u.slice >= 0 ? " slice " + u.slice : "") + " (" + u.source + ")";

        [Test]
        public void Sets_TileMatchesTexturesMdOrIsRegistered()
        {
            foreach (var u in Units())
            {
                if (u.tileRef <= 0f) continue;
                float dev = Math.Abs(u.tile / u.tileRef - 1f);
                if (dev > 0.15f + 1e-4f)
                    Assert.IsFalse(string.IsNullOrEmpty(u.reg), FormattableString.Invariant($"{Name(u)}: tile {u.tile:0.###} m is {100 * dev:0.0} % off TEXTURES.md {u.tileRef:0.###} m without a registration"));
            }
        }

        [Test]
        public void Sets_MetallicOnlyInTheAllowList()
        {
            foreach (var u in Units())
            {
                if (u.st == null) continue;
                if (u.e.meta.kind == "array" && u.e.meta.cls == "overlay") continue;   // overlay masks carry coverage in R
                if (Array.IndexOf(MetalAllow, u.source) < 0)
                    Assert.AreEqual(0f, u.st.metalMean, 1e-6f, Name(u) + ": metallic must be 0 outside the allow-list (TEXTURE_PLAN 1.4 step 6)");
            }
        }

        [Test]
        public void Sets_ContrastChromaAndViolet()
        {
            foreach (var u in Units())
            {
                if (u.st == null || u.st.contrastTarget <= 0f || u.st.mean == null || u.st.mean.Length < 3) continue;
                if (u.e.meta.cls == "micro") continue;                                   // the micro modifier is graded by its own contrast
                string n = Name(u);
                Assert.AreEqual(u.st.contrastTarget, u.st.stdOverMean, 0.02, n + " std / mean at the class midpoint");
                for (int c = 0; c < 3; c++) Assert.AreEqual(0.5, u.st.mean[c], 0.01, n + " variation mean 0.5 (channel " + c + ")");
                Assert.LessOrEqual(u.st.violetShare, 0.005, n + " violet share");
                Assert.LessOrEqual(u.st.saturation, 0.45 + 1e-4, n + " mean saturation (the warm-class cap)");
                if (u.st.satMax > 0f)
                    Assert.LessOrEqual(u.st.saturation, u.st.satMax + 1e-3, n + " mean saturation under its configured cap (0.12 neutral classes)");
            }
        }

        [Test]
        public void Sets_ContentGateHolds()
        {
            foreach (var u in Units())
            {
                if (u.st == null || u.e.meta.flatten > 0f || u.st.flatten > 0f || u.st.lfBefore < 0.01f) continue;
                Assert.GreaterOrEqual(u.st.lf, 0.6f * u.st.lfBefore - 1e-6f, Name(u) + " keeps 60 % of its 1 m low-pass content (1.4 step 4)");
            }
        }

        [Test]
        public void Leaves_PreserveCoverage()
        {
            foreach (var (e, map, path) in Maps())
            {
                if (!e.meta.alpha || map != ExtMap.Albedo) continue;
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.IsNotNull(imp, path);
                Assert.IsTrue(imp.mipMapsPreserveCoverage, path + " mipMapsPreserveCoverage");
                Assert.AreEqual(TextureImporterAlphaSource.FromInput, imp.alphaSource, path + " alpha from input");
            }
        }

        [Test]
        public void Budget_TextureVramUnder900MiB()
        {
            double bytes = 0;
            foreach (var (e, map, path) in Maps())
            {
                int depth = e.meta.IsArray ? e.meta.Depth : 1;
                bytes += (double)e.meta.res * e.meta.res * depth * 1.0 * 4.0 / 3.0;   // BC7 / BC5: 1 byte per texel, mips 4 / 3
            }
            double mib = bytes / (1024.0 * 1024.0);
            TestContext.WriteLine(FormattableString.Invariant($"External texture VRAM from formats: {mib:0.0} MiB (plan cap {NocTexConstants.TexVramCapMiB:0}, hard cap {NocTexConstants.TexVramHardMiB:0})"));
            Assert.LessOrEqual(mib, NocTexConstants.TexVramCapMiB);
        }
    }
}
