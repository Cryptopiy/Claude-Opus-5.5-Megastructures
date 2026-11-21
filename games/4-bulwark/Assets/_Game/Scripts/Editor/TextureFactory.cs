// TextureFactory: writes the BwTexGen sets (Runtime/Rendering/BwTexGen.cs) as PNG textures under Assets/_Game/Textures
// (MASTER_PROMPT 8.8, 8.11.3, 8.11.9). Called by MaterialFactory.Build (so RebuildAll and RebuildMaterials regenerate it).
// Per set three PNGs, level 0 only (Wave B: PNG instead of text-serialised .asset hex, which rewrote about 56 MB of hex per
// regeneration in a '* -text' repository):
//   Bw<Set>_Albedo.png  sRGB, BC7
//   Bw<Set>_Normal.png  linear, BC5 (RG = tangent xy; z rebuilt in BwLitCore.BwUnpackRG)
//   Bw<Set>_Mask.png    linear, BC7 (R metallic potential, G occlusion, B height, A smoothness); Crust: RGBA32, R = exact
//                       per-shell rank (point-sampled by BwArmour)
// The importer settings are code, not .meta state: BwTexturePostprocessor.OnPreprocessTexture sets them for every
// Bw<Set>_<Map>.png of this folder (type Default, sRGB for albedo only, mipmaps, repeat, trilinear, aniso 8, not readable,
// no NPOT scaling, max 2048, the Standalone format above), and OnPostprocessTexture replaces the importer's mips by our own
// chain (BwTexGen.MipChainFromLevel0: linear-light albedo, renormalised normals, Toksvig smoothness per level from the
// Normal PNG's level 0), so the result equals the old BuildMips assets. Generation is skipped when the manifest
// (BwTextures.manifest.txt) names the current BwTexGen.Version, the sizes and the png format and every PNG exists: the
// PNGs change only when BwTexGen.Version changes. VRAM as before: about 32 MB (budget 8.11.9: under 2 GB).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Megastructure.Rendering;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Megastructure.EditorTools
{
    public static class TextureFactory
    {
        public const string TexturesDir = "Assets/_Game/Textures";
        public const string ManifestPath = TexturesDir + "/BwTextures.manifest.txt";
        public const int MaxSize = 2048;

        public enum Map { Albedo, Normal, Mask }

        public static string AssetPath(string set, Map map) => TexturesDir + "/Bw" + set + "_" + map + ".png";
        /// <summary>The pre-Wave-B text asset of a map (deleted when the PNGs are written).</summary>
        public static string LegacyAssetPath(string set, Map map) => TexturesDir + "/Bw" + set + "_" + map + ".asset";

        public static int SizeOf(string set)
        {
            switch (set)
            {
                case "Concrete": return BwTexGen.ConcreteSize;
                case "Road": return BwTexGen.RoadSize;
                case "Steel": return BwTexGen.SteelSize;
                default: return BwTexGen.CrustSize;
            }
        }

        /// <summary>The Standalone format of a map (TextureImporterFormat) and its runtime TextureFormat.</summary>
        public static TextureImporterFormat ImporterFormat(string set, Map map)
            => map == Map.Normal ? TextureImporterFormat.BC5 : (map == Map.Mask && set == "Crust" ? TextureImporterFormat.RGBA32 : TextureImporterFormat.BC7);

        static string ManifestText()
        {
            var sb = new StringBuilder();
            sb.Append("BwTexGen version ").Append(BwTexGen.Version).Append('\n');
            sb.Append("format png level0 + MipChainFromLevel0\n");
            foreach (var s in BwTexGen.SetNames) sb.Append(s).Append(' ').Append(SizeOf(s)).Append('\n');
            return sb.ToString();
        }

        /// <summary>Generates every set unless the manifest is current and all PNGs exist. Returns true when it wrote.</summary>
        public static bool EnsureAll(List<string> problems, bool force = false)
        {
            EditorUtil.EnsureFolder(TexturesDir);
            string abs = BwPaths.Abs(ManifestPath);
            bool current = !force && File.Exists(abs) && File.ReadAllText(abs) == ManifestText();
            if (current)
            {
                bool all = true;
                foreach (var s in BwTexGen.SetNames)
                    foreach (Map m in Enum.GetValues(typeof(Map)))
                        if (AssetDatabase.LoadAssetAtPath<Texture2D>(AssetPath(s, m)) == null) all = false;
                if (all)
                {
                    int legacy = DeleteLegacyAssets();
                    BwLog.Info("TextureFactory: textures current (BwTexGen version " + BwTexGen.Version + ", png), not regenerated" + (legacy > 0 ? $"; {legacy} legacy .asset deleted" : ""));
                    return false;
                }
            }
            var total = Stopwatch.StartNew();
            var written = new List<string>();
            foreach (var name in BwTexGen.SetNames)
            {
                try { written.AddRange(WriteSetPngs(name)); }
                catch (Exception e) { BwLog.Problem(problems, "TextureFactory: set " + name + " failed: " + e); }
            }
            // import: the Normal PNG of a set must be on disk before its Mask imports (the postprocessor reads it)
            foreach (var p in written) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            int del = DeleteLegacyAssets();
            File.WriteAllText(abs, ManifestText());
            AssetDatabase.ImportAsset(ManifestPath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.SaveAssets();
            long bytes = 0, disk = 0;
            foreach (var s in BwTexGen.SetNames)
                foreach (Map m in Enum.GetValues(typeof(Map)))
                {
                    int w = SizeOf(s);
                    bytes += (long)w * w * (ImporterFormat(s, m) == TextureImporterFormat.RGBA32 ? 4 : 1) * 4 / 3;
                    var fi = new FileInfo(BwPaths.Abs(AssetPath(s, m)));
                    if (fi.Exists) disk += fi.Length;
                }
            BwLog.Info($"TextureFactory: {BwTexGen.SetNames.Length} sets written as PNG in {total.Elapsed.TotalSeconds:F1} s, GPU size {bytes / (1024.0 * 1024.0):F1} MB, " +
                       $"PNG on disk {disk / (1024.0 * 1024.0):F1} MB, {del} legacy .asset deleted");
            return true;
        }

        /// <summary>Generates one set and writes its three level-0 PNGs; returns their asset paths (Normal first).</summary>
        static List<string> WriteSetPngs(string name)
        {
            var sw = Stopwatch.StartNew();
            TexSet set = BwTexGen.Generate(name);
            double genS = sw.Elapsed.TotalSeconds;
            TexMip[] mips = BwTexGen.BuildMips(set);
            var paths = new List<string>(3);
            foreach (Map map in new[] { Map.Normal, Map.Albedo, Map.Mask })
            {
                byte[] data = map == Map.Albedo ? mips[0].Albedo : map == Map.Normal ? mips[0].Normal : mips[0].Mask;
                var tex = new Texture2D(set.Size, set.Size, TextureFormat.RGBA32, false, true);
                try
                {
                    tex.SetPixelData(data, 0);
                    tex.Apply(false, false);
                    byte[] png = tex.EncodeToPNG();
                    string path = AssetPath(set.Name, map);
                    File.WriteAllBytes(BwPaths.Abs(path), png);
                    paths.Add(path);
                }
                finally { Object.DestroyImmediate(tex); }
            }
            BwTexGen.Stats(set.R, out float rMean, out float rMin, out float rMax);
            BwTexGen.Stats(set.Smooth, out float sMean, out float sMin, out float sMax);
            BwLog.Info($"TextureFactory: {name} {set.Size}^2 per {set.TileMetres} m (generate {genS:F1} s, total {sw.Elapsed.TotalSeconds:F1} s); " +
                       $"albedo R {rMin:F3}..{rMax:F3} mean {rMean:F3}; smoothness {sMin:F2}..{sMax:F2} mean {sMean:F2}; " +
                       $"Toksvig mean |N| mip0 {mips[0].MeanNormalLength:F3} mip4 {mips[Math.Min(4, mips.Length - 1)].MeanNormalLength:F3}, " +
                       $"smoothness mip0 {mips[0].MeanSmoothness:F3} mip4 {mips[Math.Min(4, mips.Length - 1)].MeanSmoothness:F3}");
            return paths;
        }

        /// <summary>Deletes the pre-Wave-B text assets (Bw*_*.asset) of the sets. Returns the number deleted.</summary>
        static int DeleteLegacyAssets()
        {
            int n = 0;
            foreach (var s in BwTexGen.SetNames)
                foreach (Map m in Enum.GetValues(typeof(Map)))
                {
                    string p = LegacyAssetPath(s, m);
                    if (File.Exists(BwPaths.Abs(p)) && AssetDatabase.DeleteAsset(p)) n++;
                }
            return n;
        }

        public static Texture2D Load(string set, Map map) => AssetDatabase.LoadAssetAtPath<Texture2D>(AssetPath(set, map));

        /// <summary>Set and map of a generated texture path (Assets/_Game/Textures/Bw&lt;Set&gt;_&lt;Map&gt;.png), else false.</summary>
        public static bool ParsePath(string assetPath, out string set, out Map map)
        {
            set = null; map = Map.Albedo;
            if (string.IsNullOrEmpty(assetPath)) return false;
            string p = assetPath.Replace('\\', '/');
            if (!p.StartsWith(TexturesDir + "/Bw", StringComparison.Ordinal) || !p.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return false;
            string f = Path.GetFileNameWithoutExtension(p).Substring(2);   // <Set>_<Map>
            int u = f.LastIndexOf('_');
            if (u <= 0) return false;
            string s = f.Substring(0, u), m = f.Substring(u + 1);
            if (Array.IndexOf(BwTexGen.SetNames, s) < 0) return false;
            if (!Enum.TryParse(m, false, out map)) return false;
            set = s;
            return true;
        }

        [MenuItem("Tools/Megastructure/Regenerate Textures", priority = 23)]
        public static void RegenerateMenu()
        {
            var p = new List<string>();
            EnsureAll(p, true);
            foreach (var s in p) UnityEngine.Debug.LogError(s);
        }
    }

    /// <summary>Import settings and the custom mip chain of the generated PNGs (TextureFactory header).</summary>
    public sealed class BwTexturePostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!TextureFactory.ParsePath(assetPath, out string set, out TextureFactory.Map map)) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Default;
            ti.textureShape = TextureImporterShape.Texture2D;
            ti.sRGBTexture = map == TextureFactory.Map.Albedo;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency = false;
            ti.mipmapEnabled = true;
            ti.mipmapFilter = TextureImporterMipFilter.BoxFilter;
            ti.mipMapsPreserveCoverage = false;
            ti.borderMipmap = false;
            ti.fadeout = false;
            ti.isReadable = false;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.wrapMode = TextureWrapMode.Repeat;
            ti.filterMode = FilterMode.Trilinear;
            ti.anisoLevel = 8;
            ti.streamingMipmaps = false;
            ti.maxTextureSize = TextureFactory.MaxSize;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            var fmt = TextureFactory.ImporterFormat(set, map);
            foreach (string platform in new[] { "Standalone", "Default" })
            {
                var ps = platform == "Default" ? ti.GetDefaultPlatformTextureSettings() : ti.GetPlatformTextureSettings(platform);
                ps.maxTextureSize = TextureFactory.MaxSize;
                ps.textureCompression = TextureImporterCompression.CompressedHQ;
                ps.compressionQuality = 100;
                if (platform == "Default") ps.format = fmt == TextureImporterFormat.RGBA32 ? TextureImporterFormat.RGBA32 : TextureImporterFormat.Automatic;
                else { ps.overridden = true; ps.format = fmt; }
                ti.SetPlatformTextureSettings(ps);
            }
        }

        void OnPostprocessTexture(Texture2D texture)
        {
            if (!TextureFactory.ParsePath(assetPath, out string set, out TextureFactory.Map map)) return;
            int size = texture.width;
            if (texture.height != size || texture.mipmapCount <= 1) return;
            byte[] level0 = ToBytes(texture.GetPixels32(0));
            byte[] normal0 = null;
            if (map == TextureFactory.Map.Mask)
            {
                string np = BwPaths.Abs(TextureFactory.AssetPath(set, TextureFactory.Map.Normal));
                if (!File.Exists(np)) { UnityEngine.Debug.LogWarning("[BW] TextureFactory: " + assetPath + " imported before its Normal PNG: importer mips kept"); return; }
                var tmp = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                try
                {
                    if (!tmp.LoadImage(File.ReadAllBytes(np), false) || tmp.width != size) { UnityEngine.Debug.LogWarning("[BW] TextureFactory: Normal PNG of " + set + " unreadable or of another size"); return; }
                    normal0 = ToBytes(tmp.GetPixels32(0));
                }
                finally { Object.DestroyImmediate(tmp); }
            }
            var mm = map == TextureFactory.Map.Albedo ? BwTexGen.MipMap.Albedo : map == TextureFactory.Map.Normal ? BwTexGen.MipMap.Normal : BwTexGen.MipMap.Mask;
            byte[][] chain = BwTexGen.MipChainFromLevel0(size, mm, level0, normal0);
            int levels = Math.Min(chain.Length, texture.mipmapCount);
            for (int l = 1; l < levels; l++) texture.SetPixels32(ToColors(chain[l]), l);
            texture.Apply(false, false);
        }

        static byte[] ToBytes(Color32[] c)
        {
            var b = new byte[c.Length * 4];
            for (int i = 0; i < c.Length; i++) { b[i * 4] = c[i].r; b[i * 4 + 1] = c[i].g; b[i * 4 + 2] = c[i].b; b[i * 4 + 3] = c[i].a; }
            return b;
        }

        static Color32[] ToColors(byte[] b)
        {
            var c = new Color32[b.Length / 4];
            for (int i = 0; i < c.Length; i++) c[i] = new Color32(b[i * 4], b[i * 4 + 1], b[i * 4 + 2], b[i * 4 + 3]);
            return c;
        }
    }
}
