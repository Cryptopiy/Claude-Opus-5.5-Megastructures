// NocTextureGen: the one texture generator of Nocturne (MASTER_PROMPT 8.8, 8.11 items 2-3 and 9; docs/MATERIALS.md). For
// every NocTexFamily it bakes the NocTexRecipes functions into three tileable maps under Assets/_Game/Textures:
//   <Family>_Albedo.png  albedo VARIATION, each channel renormalised to a mean of exactly 0.5 linear, stored sRGB (sRGB on)
//   <Family>_Normal.png  tangent-space normal from the height (central differences, wrapped; physical depth DepthMetres over
//                        TileMetres), imported as a Normal Map (linear, mipmapped)
//   <Family>_Mask.png    R metallic, G occlusion, B height, A smoothness (linear, sRGB off)
// Import: mipmaps on (Kaiser), trilinear, anisotropic 8 (16 for the floor families), repeat, high-quality compression, max
// size 1024 / 512. The bake is deterministic and parallel; files are rewritten only when their bytes change (an unchanged
// RebuildAll triggers no reimport). Called from MaterialFactory.Build before the materials (RebuildAll).
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Megastructure.EditorTools
{
    public static class NocTextureGen
    {
        public const string Dir = "Assets/_Game/Textures";

        public static string AlbedoPath(NocTexFamily f) => Dir + "/" + f + "_Albedo.png";
        public static string NormalPath(NocTexFamily f) => Dir + "/" + f + "_Normal.png";
        public static string MaskPath(NocTexFamily f) => Dir + "/" + f + "_Mask.png";

        static bool IsFloor(NocTexFamily f) => f == NocTexFamily.Paving || f == NocTexFamily.Asphalt || f == NocTexFamily.SteelDeck || f == NocTexFamily.PlazaStone;

        /// <summary>Bakes and imports every family; returns the number of files that changed.</summary>
        public static int GenerateAll(List<string> problems)
        {
            EditorUtil.EnsureFolder(Dir);
            int changed = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            foreach (var f in NocTexRecipes.All)
            {
                try
                {
                    Bake(f, out byte[] albedo, out byte[] normal, out byte[] mask, out int n);
                    changed += Write(AlbedoPath(f), albedo, n);
                    changed += Write(NormalPath(f), normal, n);
                    changed += Write(MaskPath(f), mask, n);
                }
                catch (Exception e) { NocLog.Problem(problems, $"NocTextureGen: {f} failed: {e.Message}"); }
            }
            foreach (var f in NocTexRecipes.All)
            {
                ConfigureImporter(AlbedoPath(f), f, kind: 0, problems);
                ConfigureImporter(NormalPath(f), f, kind: 1, problems);
                ConfigureImporter(MaskPath(f), f, kind: 2, problems);
            }
            NocLog.Info($"NocTextureGen: {NocTexRecipes.All.Length} families x 3 maps, {changed} file(s) changed, {sw.Elapsed.TotalSeconds:F1} s");
            return changed;
        }

        /// <summary>Bakes one family into RGBA32 pixel buffers (row 0 = v 0).</summary>
        public static void Bake(NocTexFamily f, out byte[] albedo, out byte[] normal, out byte[] mask, out int n)
        {
            int N = NocTexRecipes.Size(f);
            n = N;
            var tex = new NocTexel[N * N];
            Parallel.For(0, N, j =>
            {
                double v = (j + 0.5) / N;
                for (int i = 0; i < N; i++) tex[j * N + i] = NocTexRecipes.Sample(f, (i + 0.5) / N, v);
            });
            // renormalise the albedo variation to a mean of exactly 0.5 per channel (the token in the material is the mean)
            double mr = 0, mg = 0, mb = 0;
            for (int k = 0; k < tex.Length; k++) { mr += tex[k].r; mg += tex[k].g; mb += tex[k].b; }
            double inv = 1.0 / tex.Length;
            double kr = 0.5 / Math.Max(mr * inv, 1e-6), kg = 0.5 / Math.Max(mg * inv, 1e-6), kb = 0.5 / Math.Max(mb * inv, 1e-6);
            // normal: slope of the physical height per metre of tile
            double texel = NocTexRecipes.TileMetres(f) / N, depth = NocTexRecipes.DepthMetres(f);
            var a = new byte[N * N * 4];
            var nm = new byte[N * N * 4];
            var mk = new byte[N * N * 4];
            Parallel.For(0, N, j =>
            {
                for (int i = 0; i < N; i++)
                {
                    int k = j * N + i;
                    var t = tex[k];
                    int o = k * 4;
                    a[o] = Srgb8(t.r * kr); a[o + 1] = Srgb8(t.g * kg); a[o + 2] = Srgb8(t.b * kb); a[o + 3] = 255;
                    double hl = tex[j * N + ((i - 1 + N) % N)].h, hr = tex[j * N + ((i + 1) % N)].h;
                    double hd = tex[((j - 1 + N) % N) * N + i].h, hu = tex[((j + 1) % N) * N + i].h;
                    double dx = (hr - hl) * depth / (2.0 * texel), dy = (hu - hd) * depth / (2.0 * texel);
                    double len = Math.Sqrt(dx * dx + dy * dy + 1.0);
                    nm[o] = Lin8(0.5 - 0.5 * dx / len); nm[o + 1] = Lin8(0.5 - 0.5 * dy / len); nm[o + 2] = Lin8(0.5 + 0.5 / len); nm[o + 3] = 255;
                    mk[o] = Lin8(t.metal); mk[o + 1] = Lin8(t.ao); mk[o + 2] = Lin8(t.h); mk[o + 3] = Lin8(t.smooth);
                }
            });
            albedo = a; normal = nm; mask = mk;
        }

        static byte Lin8(double x) => (byte)Math.Round(Math.Max(0.0, Math.Min(1.0, x)) * 255.0);
        static byte Srgb8(double lin) => Lin8(NocturnePalette.LinearToSrgb(Math.Max(0.0, Math.Min(1.0, lin))));

        static int Write(string assetPath, byte[] rgba, int n)
        {
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false, true);
            t.SetPixelData(rgba, 0);
            t.Apply(false, false);
            byte[] png = t.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(t);
            string abs = NocPaths.Abs(assetPath);
            if (File.Exists(abs) && ShaderGen.BytesEqual(File.ReadAllBytes(abs), png)) return 0;
            File.WriteAllBytes(abs, png);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            return 1;
        }

        /// <summary>kind 0 albedo (sRGB), 1 normal map, 2 mask (linear).</summary>
        static void ConfigureImporter(string assetPath, NocTexFamily f, int kind, List<string> problems)
        {
            var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (imp == null) { NocLog.Problem(problems, "NocTextureGen: no TextureImporter for " + assetPath); return; }
            bool dirty = false;
            void Set<T>(T cur, T want, Action<T> apply) { if (!EqualityComparer<T>.Default.Equals(cur, want)) { apply(want); dirty = true; } }
            Set(imp.textureType, kind == 1 ? TextureImporterType.NormalMap : TextureImporterType.Default, v => imp.textureType = v);
            Set(imp.sRGBTexture, kind == 0, v => imp.sRGBTexture = v);
            Set(imp.mipmapEnabled, true, v => imp.mipmapEnabled = v);
            Set(imp.mipmapFilter, TextureImporterMipFilter.KaiserFilter, v => imp.mipmapFilter = v);
            Set(imp.streamingMipmaps, false, v => imp.streamingMipmaps = v);
            Set(imp.wrapMode, TextureWrapMode.Repeat, v => imp.wrapMode = v);
            Set(imp.filterMode, FilterMode.Trilinear, v => imp.filterMode = v);
            Set(imp.anisoLevel, IsFloor(f) ? 16 : 8, v => imp.anisoLevel = v);
            Set(imp.maxTextureSize, NocTexRecipes.Size(f), v => imp.maxTextureSize = v);
            Set(imp.textureCompression, TextureImporterCompression.CompressedHQ, v => imp.textureCompression = v);
            Set(imp.alphaSource, kind == 2 ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None, v => imp.alphaSource = v);
            Set(imp.alphaIsTransparency, false, v => imp.alphaIsTransparency = v);
            Set(imp.isReadable, false, v => imp.isReadable = v);
            if (dirty) imp.SaveAndReimport();
        }

        /// <summary>Import-settings check for Validate / tests: one line per problem.</summary>
        public static void Check(List<string> problems)
        {
            foreach (var f in NocTexRecipes.All)
                for (int kind = 0; kind < 3; kind++)
                {
                    string p = kind == 0 ? AlbedoPath(f) : kind == 1 ? NormalPath(f) : MaskPath(f);
                    var imp = AssetImporter.GetAtPath(p) as TextureImporter;
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                    if (imp == null || tex == null) { problems.Add("texture missing: " + p); continue; }
                    if (imp.sRGBTexture != (kind == 0)) problems.Add(p + ": sRGB flag " + imp.sRGBTexture);
                    if (kind == 1 && imp.textureType != TextureImporterType.NormalMap) problems.Add(p + ": not a normal map");
                    if (!imp.mipmapEnabled || tex.mipmapCount < 2) problems.Add(p + ": no mipmaps");
                    if (imp.anisoLevel < 8) problems.Add(p + ": aniso " + imp.anisoLevel);
                    if (tex.width != NocTexRecipes.Size(f)) problems.Add(p + ": size " + tex.width);
                }
        }
    }
}
