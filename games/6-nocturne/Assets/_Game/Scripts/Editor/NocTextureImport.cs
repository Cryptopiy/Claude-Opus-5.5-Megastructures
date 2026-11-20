// NocTextureImport: the import settings of Look v2's processed textures (docs/TEXTURE_PLAN.md 1.5; owner TP), an AssetPostprocessor
// scoped to Assets/_Game/Textures/External/. Tools/texprep.py writes, per set or array <Key>:
//   <Key>_Albedo.png  albedo variation (sRGB, per-channel mean 0.5 linear; leaves and grating: alpha = opacity, linear)
//   <Key>_Normal.png  tangent normal, GL convention
//   <Key>_Mask.png    R metallic, G AO, B height, A absolute perceptual smoothness (linear)
//   <Key>.texmeta.json  the manifest (TexMeta below): tile, module, anti-tiling, rotation, metalFromMap, the measured stats,
//                       the import hints (mip filter, aniso, alpha, flipbook layout) and the input hashes
// OnPreprocessTexture sets, by suffix: _Albedo Default type, sRGB, BC7 (quality 100), Kaiser mips for unstructured sets and Box
// for gridded sets and leaves; _Normal NormalMap type, BC5, Box mips; _Mask linear, alpha from input, BC7, Box mips. Every texture:
// mips on, trilinear, Repeat, aniso 16 (facade, paving, asphalt, deck and arrays) or 8 (ForceEnable runs 8 as 9), mip bias 0, no
// streaming, not readable, max size = the manifest resolution (8192 for flipbooks). Alpha sets add mipMapsPreserveCoverage with
// reference 0.5. Flipbook sources (texmeta cols x rows > 1) import as Texture2DArray.
// Toksvig (1.5) in OnPostprocessTexture / OnPostprocessTexture2DArray for _Mask files: the sibling _Normal PNG is read from disk
// (the mask depends on it, so a changed normal re-imports the mask); for mip L the mip-0 unit normals are averaged over 2^L
// blocks with wrap (|n_bar|), kappa = (3|n| - |n|^3) / (1 - |n|^2), variance = 1 / (2 kappa) (URP TextureNormalVariance), and
// alpha'^2 = alpha^2 + min(2 variance, 0.20^2) with alpha = (1 - s)^2, s' = 1 - sqrt(alpha'). Mip 0 is unchanged, smoothness
// never increases. GetVersion() changes with every change of these rules.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Megastructure.EditorTools
{
    /// <summary>One input file of a processed set (texmeta "inputs").</summary>
    [Serializable]
    public sealed class TexInput
    {
        public string path;
        public string sha256;
    }

    /// <summary>The measured statistics of a processed set or array slice (texmeta "stats"; texprep step 10).</summary>
    [Serializable]
    public sealed class TexStats
    {
        public float[] meanAbs;          // absolute processed linear mean RGB before the 0.5 normalisation (for PA and ART)
        public float[] mean;             // linear mean RGB of the written albedo variation (0.5 each)
        public float stdOverMean;        // luminance std / mean of the albedo variation (1.4 step 5)
        public float contrastTarget;     // the class midpoint it was normalised to
        public float satMax;             // the cap of the mean saturation (1.4 step 5: 0.12 neutral classes, 0.45 brick / bark / leaf / moss)
        public float saturation;         // mean HSV saturation of class token x 2 x variation (sRGB)
        public float violetShare;        // share of pixels with hue 260-330 deg and saturation > 0.15 after white balance
        public float bake;               // measured baked-light fit after processing (1.4 step 4a)
        public float bakeBefore;
        public float lf, lfBefore;       // std of the 1 m low-pass luminance after / before (content gate)
        public float seamScore;
        public float smoothP5, smoothP50, smoothP95;
        public float heightP1, heightP99;
        public float metalMean;          // mean of mask R
        public float coverage;           // mean alpha (alpha sets)
        public float signCorrX, signCorrY;   // step 2: corr(n.x, -dh/du), corr(n.y, -dh/dv) of the source (> 0)
        public float rampSlope;          // step 4b: low-pass plane slope per tile (a guard, never subtracted)
        public float blob, blobBefore;   // TEX-R blob share (local mean beyond 1.25 sd) after / before step 4
        public float normalStrength;     // the class normal strength baked into _Normal
        public float flatten;            // step 4c strength of this set / slice (the content gate is skipped when > 0)
    }

    /// <summary>The manifest of one processed set or array, written by Tools/texprep.py (JsonUtility-compatible).</summary>
    [Serializable]
    public sealed class TexMeta
    {
        public int version;
        public string key;               // processed key (the file prefix): a config key, e.g. "Concrete", "FacadeArr"
        public string owner;             // the package whose config produced it (TP, FA, SU, FO)
        public string source;            // the raw set folder (ExternalTextures/raw/<source>); arrays: "" (see slices)
        public string kind;              // "set" or "array"
        public string cls;               // contrast / chroma class (concrete, brick, leaves, ...)
        public int res;                  // slice or set resolution (px)
        public float tile, tileV;        // metres per tile (u, v; tileV 0 = tile)
        public float tileRef;            // the TEXTURES.md "Final sets" tile (m)
        public string registration;      // why the tile deviates from tileRef by more than 15 % ("" = it does not)
        public float moduleU, moduleV;   // gridded sets: module size (m), 0 = none
        public bool runningBond;
        public float jointWidth;         // gridded sets: measured joint width (m)
        public string antiTiling;        // hex, hexOffsets, heightHex, module, panel, none
        public bool rotate;
        public bool metalFromMap;
        public bool gridded;             // Box albedo mips
        public bool alpha;               // albedo alpha = opacity (leaves, grating): preserve coverage
        public float alphaRef;           // coverage reference (0.5)
        public int aniso;                // 16 or 8
        public int cols, rows;           // flipbook layout of arrays (1 x 1 for sets)
        public string[] slices;          // arrays: the source set of each slice, in slice order
        public string[] sliceNames;      // arrays: the config name of each slice ("precast", "leaks", ...)
        public float[] sliceTile, sliceTileV, sliceTileRef;   // arrays: metres per tile of each slice (tileV 0 = isotropic), TEXTURES.md tile
        public float[] sliceModuleU, sliceModuleV;            // arrays: module of each slice (0 = none)
        public string[] sliceRegistration;                    // arrays: why a slice tile deviates from its TEXTURES.md tile ("" = it does not)
        public string bondAxis;          // gridded sets: "u" (running bond, rows shifted along u), "v" (half bond along v) or ""
        public float flatten;            // step 4c strength (0 = off; the content gate is skipped when > 0)
        public bool placeholder;         // a 64 px neutral set written by texprep --placeholders (TP-1 replaces it)
        public string buildHash;         // texprep's staleness hash (inputs, config entry, texprep version)
        public int texprepVersion;
        public string[] maps;            // the written maps: Albedo, Normal, Mask (RoomArr: Albedo, Mask; OverlayArr: Mask, Normal; MicroArr: Albedo, Normal)
        public TexInput[] inputs;
        public TexInput[] outputs;       // the written files and their SHA-256 (texprep --check: edited after texprep = stale)
        public TexStats stats;           // sets: the set; arrays: slice 0 (sliceStats holds every slice)
        public TexStats[] sliceStats;

        public const string Suffix = ".texmeta.json";
        public bool IsArray => cols * rows > 1;
        public int Depth => Math.Max(1, cols * rows);
        public float TileU => tile;
        public float TileVOrU => tileV > 0f ? tileV : tile;
        /// <summary>True when texprep wrote this map for the set (maps lists them; an old manifest without the list has all three).</summary>
        public bool HasMap(ExtMap m) => maps == null || maps.Length == 0 || Array.IndexOf(maps, m.ToString()) >= 0;
        /// <summary>Metres per tile of slice k (u, v) of an array (the set's tile for a set).</summary>
        public float SliceTileU(int k) => sliceTile != null && k < sliceTile.Length && sliceTile[k] > 0f ? sliceTile[k] : tile;
        public float SliceTileV(int k) => sliceTileV != null && k < sliceTileV.Length && sliceTileV[k] > 0f ? sliceTileV[k] : SliceTileU(k);

        public static TexMeta Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var m = JsonUtility.FromJson<TexMeta>(File.ReadAllText(path));
                if (m != null && (m.cols <= 0 || m.rows <= 0)) { m.cols = Math.Max(1, m.cols); m.rows = Math.Max(1, m.rows); }
                return m;
            }
            catch (Exception e) { Debug.LogWarning("[NOC] TexMeta: cannot read " + path + ": " + e.Message); return null; }
        }
    }

    /// <summary>Map kinds of the processed files (file suffix).</summary>
    public enum ExtMap { None, Albedo, Normal, Mask }

    public sealed class NocTextureImport : AssetPostprocessor
    {
        /// <summary>Asset folder of the processed sets (texprep's output).</summary>
        public const string Root = "Assets/_Game/Textures/External/";
        /// <summary>Bumped on every change of the import rules or the Toksvig formula (re-imports every External texture).</summary>
        public const uint Version = 2;      // 2: the texmeta dependency is registered even while the texmeta is missing

        public override uint GetVersion() => Version;

        public static bool InScope(string assetPath) => assetPath != null && assetPath.Replace('\\', '/').StartsWith(Root, StringComparison.Ordinal);

        public static ExtMap MapOf(string assetPath)
        {
            string n = Path.GetFileNameWithoutExtension(assetPath);
            if (n.EndsWith("_Albedo", StringComparison.Ordinal)) return ExtMap.Albedo;
            if (n.EndsWith("_Normal", StringComparison.Ordinal)) return ExtMap.Normal;
            if (n.EndsWith("_Mask", StringComparison.Ordinal)) return ExtMap.Mask;
            return ExtMap.None;
        }

        /// <summary>"Dir/Key_Albedo.png" -> "Dir/Key".</summary>
        public static string KeyPathOf(string assetPath)
        {
            string p = assetPath.Replace('\\', '/');
            string n = Path.GetFileNameWithoutExtension(p);
            int us = n.LastIndexOf('_');
            string dir = p.Substring(0, p.LastIndexOf('/') + 1);
            return dir + (us > 0 ? n.Substring(0, us) : n);
        }

        public static string MetaPathOf(string assetPath) => KeyPathOf(assetPath) + TexMeta.Suffix;
        public static string SiblingPath(string assetPath, ExtMap map) => KeyPathOf(assetPath) + "_" + map + ".png";

        static string Abs(string assetPath) => Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath);

        /// <summary>The settings a texture of this map and manifest must have (shared by the importer and TextureTests).</summary>
        public struct Expected
        {
            public TextureImporterType type;
            public bool srgb;
            public TextureImporterFormat format;
            public TextureImporterMipFilter mipFilter;
            public int aniso, maxSize;
            public bool preserveCoverage;
            public TextureImporterAlphaSource alphaSource;
            public bool array;
        }

        public static Expected ExpectedFor(ExtMap map, TexMeta meta)
        {
            var e = new Expected
            {
                aniso = meta != null && meta.aniso > 0 ? meta.aniso : NocTexConstants.TexAnisoLow,
                array = meta != null && meta.IsArray,
            };
            int res = meta != null && meta.res > 0 ? meta.res : 2048;
            e.maxSize = e.array ? 8192 : NextPow2(res);
            bool box = meta != null && (meta.gridded || meta.alpha);
            switch (map)
            {
                case ExtMap.Albedo:
                    e.type = TextureImporterType.Default; e.srgb = true; e.format = TextureImporterFormat.BC7;
                    e.mipFilter = box ? TextureImporterMipFilter.BoxFilter : TextureImporterMipFilter.KaiserFilter;
                    e.alphaSource = meta != null && meta.alpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
                    e.preserveCoverage = meta != null && meta.alpha;
                    break;
                case ExtMap.Normal:
                    e.type = TextureImporterType.NormalMap; e.srgb = false; e.format = TextureImporterFormat.BC5;
                    e.mipFilter = TextureImporterMipFilter.BoxFilter; e.alphaSource = TextureImporterAlphaSource.None;
                    break;
                default:
                    e.type = TextureImporterType.Default; e.srgb = false; e.format = TextureImporterFormat.BC7;
                    e.mipFilter = TextureImporterMipFilter.BoxFilter; e.alphaSource = TextureImporterAlphaSource.FromInput;
                    break;
            }
            return e;
        }

        static int NextPow2(int v) { int p = 1; while (p < v && p < 16384) p <<= 1; return p; }

        void OnPreprocessTexture()
        {
            if (!InScope(assetPath)) return;
            ExtMap map = MapOf(assetPath);
            if (map == ExtMap.None) return;
            var imp = (TextureImporter)assetImporter;
            string metaPath = MetaPathOf(assetPath);
            TexMeta meta = TexMeta.Load(Abs(metaPath));
            if (meta == null) Debug.LogWarning("[NOC] NocTextureImport: no manifest " + metaPath + " for " + assetPath + " (defaults used)");
            // always, also while the manifest is missing: a PNG imported before its texmeta existed must re-import when the
            // texmeta appears (seen 2026-11-16: FacadeArr_Normal stayed a 2D texture with aniso 8 after a half-written first run)
            context.DependsOnSourceAsset(metaPath);
            Expected e = ExpectedFor(map, meta);

            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.textureType = e.type;
            s.sRGBTexture = e.srgb;
            s.mipmapEnabled = true;
            s.mipmapFilter = e.mipFilter;
            s.mipMapsPreserveCoverage = e.preserveCoverage;
            s.alphaTestReferenceValue = meta != null && meta.alphaRef > 0f ? meta.alphaRef : 0.5f;
            s.alphaSource = e.alphaSource;
            s.alphaIsTransparency = false;               // texprep dilates the colour itself
            s.filterMode = FilterMode.Trilinear;
            s.wrapMode = TextureWrapMode.Repeat;
            s.aniso = e.aniso;
            s.mipmapBias = 0f;
            s.streamingMipmaps = false;
            s.readable = false;
            s.npotScale = TextureImporterNPOTScale.None;
            if (e.array)
            {
                s.textureShape = TextureImporterShape.Texture2DArray;
                s.flipbookColumns = meta.cols;
                s.flipbookRows = meta.rows;
            }
            else s.textureShape = TextureImporterShape.Texture2D;
            imp.SetTextureSettings(s);
            imp.maxTextureSize = e.maxSize;
            imp.textureCompression = TextureImporterCompression.CompressedHQ;
            imp.compressionQuality = 100;

            var ps = imp.GetPlatformTextureSettings("Standalone");
            ps.overridden = true;
            ps.format = e.format;
            ps.maxTextureSize = e.maxSize;
            ps.compressionQuality = 100;
            ps.textureCompression = TextureImporterCompression.CompressedHQ;
            imp.SetPlatformTextureSettings(ps);

            if (map == ExtMap.Mask)
            {
                string normal = SiblingPath(assetPath, ExtMap.Normal);
                if (File.Exists(Abs(normal))) context.DependsOnSourceAsset(normal);   // a changed normal re-imports the mask
            }
        }

        void OnPostprocessTexture(Texture2D tex)
        {
            if (!InScope(assetPath) || MapOf(assetPath) != ExtMap.Mask) return;
            var nrm = LoadNormals(SiblingPath(assetPath, ExtMap.Normal), out int nw, out int nh);
            if (nrm == null) return;
            for (int mip = 1; mip < tex.mipmapCount; mip++)
            {
                Color[] c = tex.GetPixels(mip);
                int w = Math.Max(1, tex.width >> mip), h = Math.Max(1, tex.height >> mip);
                ApplyToksvig(c, w, h, nrm, nw, nh, 0, 0, nw, nh);
                tex.SetPixels(c, mip);
            }
        }

        void OnPostprocessTexture2DArray(Texture2DArray tex)
        {
            if (!InScope(assetPath) || MapOf(assetPath) != ExtMap.Mask) return;
            var meta = TexMeta.Load(Abs(MetaPathOf(assetPath)));
            if (meta == null || !meta.HasMap(ExtMap.Normal)) return;      // RoomArr: no normal array, no Toksvig term
            var nrm = LoadNormals(SiblingPath(assetPath, ExtMap.Normal), out int nw, out int nh);
            if (nrm == null) return;
            int sw = nw / Math.Max(1, meta.cols), sh = nh / Math.Max(1, meta.rows);
            for (int slice = 0; slice < tex.depth; slice++)
            {
                // flipbook slices run left to right, top to bottom; the decoded PNG rows run bottom-up
                int col = slice % meta.cols, row = slice / meta.cols;
                int x0 = col * sw, y0 = (meta.rows - 1 - row) * sh;
                for (int mip = 1; mip < tex.mipmapCount; mip++)
                {
                    Color[] c = tex.GetPixels(slice, mip);
                    int w = Math.Max(1, tex.width >> mip), h = Math.Max(1, tex.height >> mip);
                    ApplyToksvig(c, w, h, nrm, nw, nh, x0, y0, sw, sh);
                    tex.SetPixels(c, slice, mip);
                }
            }
        }

        /// <summary>Unit normals (x, y, z per texel, bottom-up rows) of a GL normal PNG on disk; null if missing.</summary>
        public static float[] LoadNormals(string assetPath, out int w, out int h)
        {
            w = h = 0;
            string abs = Abs(assetPath);
            if (!File.Exists(abs)) { Debug.LogWarning("[NOC] NocTextureImport: Toksvig needs " + assetPath); return null; }
            var t = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            try
            {
                if (!t.LoadImage(File.ReadAllBytes(abs), false)) return null;
                w = t.width; h = t.height;
                var px = t.GetPixels32();
                var n = new float[px.Length * 3];
                for (int i = 0; i < px.Length; i++)
                {
                    float x = px[i].r / 127.5f - 1f, y = px[i].g / 127.5f - 1f, z = px[i].b / 127.5f - 1f;
                    float l = Mathf.Sqrt(x * x + y * y + z * z);
                    if (l < 1e-6f) { x = 0; y = 0; z = 1; l = 1; }
                    n[3 * i] = x / l; n[3 * i + 1] = y / l; n[3 * i + 2] = z / l;
                }
                return n;
            }
            finally { UnityEngine.Object.DestroyImmediate(t); }
        }

        /// <summary>Toksvig on one mip (w x h, bottom-up) of the mask's alpha, from the mip-0 normals of the region (x0, y0, rw, rh)
        /// of the normal image (nw wide): each mip texel averages the (rw / w) x (rh / h) normals under it, with wrap.</summary>
        public static void ApplyToksvig(Color[] mask, int w, int h, float[] nrm, int nw, int nh, int x0, int y0, int rw, int rh)
        {
            int bx = Math.Max(1, rw / w), by = Math.Max(1, rh / h);
            for (int j = 0; j < h; j++)
                for (int i = 0; i < w; i++)
                {
                    double sx = 0, sy = 0, sz = 0;
                    for (int b = 0; b < by; b++)
                    {
                        int yy = y0 + ((j * by + b) % rh);
                        for (int a = 0; a < bx; a++)
                        {
                            int xx = x0 + ((i * bx + a) % rw);
                            int k = 3 * (yy * nw + xx);
                            sx += nrm[k]; sy += nrm[k + 1]; sz += nrm[k + 2];
                        }
                    }
                    double cnt = bx * by;
                    double len = Math.Sqrt(sx * sx + sy * sy + sz * sz) / cnt;
                    int idx = j * w + i;
                    mask[idx].a = (float)ToksvigSmoothness(mask[idx].a, len);
                }
        }

        /// <summary>s' for smoothness s under an averaged normal of length |n_bar| (1.5 step 4-5).</summary>
        public static double ToksvigSmoothness(double s, double nLen)
        {
            double n = Math.Min(Math.Max(nLen, 1e-4), 0.99999);
            double kappa = (3.0 * n - n * n * n) / (1.0 - n * n);
            double variance = 1.0 / (2.0 * kappa);
            double cap2 = NocTexConstants.TexToksvigCap * NocTexConstants.TexToksvigCap;
            double alpha = (1.0 - s) * (1.0 - s);
            double a2 = alpha * alpha + Math.Min(2.0 * variance, cap2);
            double sp = 1.0 - Math.Sqrt(Math.Sqrt(a2));
            return Math.Min(s, Math.Max(0.0, sp));
        }

        // ------------------------------------------------------------------------------------------------ manifests

        /// <summary>Every processed manifest under Root (asset paths of the .texmeta.json files).</summary>
        public static List<string> AllManifests()
        {
            var l = new List<string>();
            string abs = Abs(Root.TrimEnd('/'));
            if (!Directory.Exists(abs)) return l;
            string proj = Path.GetDirectoryName(Application.dataPath).Replace('\\', '/') + "/";
            foreach (var f in Directory.GetFiles(abs, "*" + TexMeta.Suffix, SearchOption.AllDirectories))
                l.Add(f.Replace('\\', '/').Replace(proj, ""));
            l.Sort(StringComparer.Ordinal);
            return l;
        }

        /// <summary>The manifest asset path of a processed key (searches every folder under Root; null if absent).</summary>
        public static string ManifestOf(string key)
        {
            foreach (var p in AllManifests())
                if (Path.GetFileName(p) == key + TexMeta.Suffix) return p;
            return null;
        }
    }
}
