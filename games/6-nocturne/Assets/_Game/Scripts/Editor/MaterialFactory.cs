// MaterialFactory: creates every MaterialLibrary field as Assets/_Game/Materials/<FieldName>.mat with the exact shader,
// keyword, palette colours, floats, render queue and instancing flag of the material contract, then
// Assets/_Game/Materials/MaterialLibrary.asset referencing them (imported from Thread; Specs() rewritten for Nocturne:
// the Phase-2 graybox set, then the shared PBR library of Part 8.11 / docs/MATERIALS.md: NocSurface surfaces, NocWet floors
// per 14.4 surface, NocGlass back and front faces). Build() first runs NocTextureGen (the one texture generator), so every
// texture a row names exists and is imported with its sRGB / mip / aniso settings before the material is written.
// Look v2 (TEXTURE_PLAN 8.3 TP-0): this file is the core (ColourSpec, MatSpec, Build, Check); the rows live in the partial row
// providers of their owners: MaterialFactory.Surface.cs (SU), .Facade.cs (FA), .Look.cs (PA), .Foliage.cs (FO). Textures are
// loaded as Texture (so Texture2DArray flipbooks work); MatSpec.ExtMaps(key) wires a processed set of Textures/External (TP-1)
// with the tile read from its texmeta.
//
// Colour space: the project is Linear. Unity stores a material Color property as an sRGB value and converts it to linear
// when the shader reads it, so the factory passes (NocturnePalette.Linear(token) x factor).gamma and every shader receives
// exactly the linear palette value (x the HDR emission factor of 14.2.1). Existing .mat files are rewritten in place
// (EditorUtility.CopySerialized from a fresh material), so GUIDs and scene references survive every rebuild and no stale
// property or keyword is carried over (idempotent, lesson 4).
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Megastructure.EditorTools
{
    /// <summary>One palette colour of a contract row: token x factor (HDR emission factors of 14.2.1), optionally plus a second
    /// token x factor (a mix of two tokens, e.g. a metal F0 between two appearances; still palette-only).</summary>
    public struct ColourSpec
    {
        public NocPal Token, Token2;
        public float Factor, Factor2;
        public ColourSpec(NocPal t, float f) { Token = t; Factor = f; Token2 = t; Factor2 = 0f; }
        public ColourSpec(NocPal t, float f, NocPal t2, float f2) { Token = t; Factor = f; Token2 = t2; Factor2 = f2; }
        public Color Linear()
        {
            var c = NocturnePalette.Linear(Token); var d = NocturnePalette.Linear(Token2);
            return new Color(c.r * Factor + d.r * Factor2, c.g * Factor + d.g * Factor2, c.b * Factor + d.b * Factor2, 1f);
        }
    }

    /// <summary>One row of the material contract.</summary>
    public sealed class MatSpec
    {
        public string Field;
        public string Shader;
        public string Keyword;                       // at most one shader_feature_local keyword
        public readonly Dictionary<string, ColourSpec> Colours = new Dictionary<string, ColourSpec>();
        public readonly Dictionary<string, float> Floats = new Dictionary<string, float>();
        public readonly Dictionary<string, string> Textures = new Dictionary<string, string>();   // property -> asset path
        public int Queue = -1;                       // -1 = the shader's queue
        public bool Instancing;

        public MatSpec C(string prop, NocPal p, float factor = 1f) { Colours[prop] = new ColourSpec(p, factor); return this; }
        public MatSpec C2(string prop, NocPal a, float fa, NocPal b, float fb) { Colours[prop] = new ColourSpec(a, fa, b, fb); return this; }
        public MatSpec F(string name, double v) { Floats[name] = (float)v; return this; }
        public MatSpec Kw(string k) { Keyword = k; return this; }
        public MatSpec Tx(string prop, string path) { Textures[prop] = path; return this; }
        /// <summary>The three generated maps of a texture family (_AlbedoTex, _NormalTex, _MaskTex) and its tile size.</summary>
        public MatSpec Maps(NocTexFamily f, double tile)
        {
            Textures["_AlbedoTex"] = NocTextureGen.AlbedoPath(f);
            Textures["_NormalTex"] = NocTextureGen.NormalPath(f);
            Textures["_MaskTex"] = NocTextureGen.MaskPath(f);
            Floats["_Tile"] = (float)tile;
            return this;
        }
        /// <summary>The three processed maps of a Look v2 set or array of Textures/External (TP-1: _AlbedoTex, _NormalTex,
        /// _MaskTex) and its tile from the texmeta: _Tile = tile u (m), plus _TileV when the tile is anisotropic. A key without a
        /// manifest keeps the paths of Root/key_*.png and a _Tile of 1, so Build reports the missing textures (run texprep).</summary>
        public MatSpec ExtMaps(string key)
        {
            string meta = NocTextureImport.ManifestOf(key);
            string prefix = meta != null ? meta.Substring(0, meta.Length - TexMeta.Suffix.Length) : NocTextureImport.Root + key;
            Textures["_AlbedoTex"] = prefix + "_Albedo.png";
            Textures["_NormalTex"] = prefix + "_Normal.png";
            Textures["_MaskTex"] = prefix + "_Mask.png";
            var m = meta != null ? TexMeta.Load(NocPaths.Abs(meta)) : null;
            Floats["_Tile"] = m != null && m.tile > 0f ? m.tile : 1f;
            if (m != null && m.tileV > 0f && Mathf.Abs(m.tileV - m.tile) > 1e-4f) Floats["_TileV"] = m.tileV;
            return this;
        }
        public MatSpec Q(int q) { Queue = q; return this; }
        public MatSpec Inst() { Instancing = true; return this; }
    }

    public static partial class MaterialFactory
    {
        public const string ErrorShaderName = "Hidden/InternalErrorShader";
        /// <summary>The PBR library shaders (A2a; NocPaths.ShaderNames discovers every Megastructure/* shader, these included).</summary>
        public const string ShaderSurface = "Megastructure/NocSurface";
        public const string ShaderWet = "Megastructure/NocWet";
        public const string ShaderGlass = "Megastructure/NocGlass";
        /// <summary>The city and light shaders (A2b: 14.7 H2, H5, H7).</summary>
        public const string ShaderFacade = "Megastructure/NocFacade";
        public const string ShaderNeon = "Megastructure/NocNeon";
        public const string ShaderStreak = "Megastructure/NocStreak";
        public const string ShaderRain = "Megastructure/NocRain";
        public static readonly string[] LibraryShaders = { ShaderSurface, ShaderWet, ShaderGlass, ShaderFacade, ShaderNeon, ShaderStreak, ShaderRain };

        static MatSpec Make(string field, string shader) => new MatSpec { Field = field, Shader = shader };

        /// <summary>The material contract, one row per MaterialLibrary field: the partial row providers of the owning packages
        /// (TEXTURE_PLAN 8.3), concatenated.</summary>
        public static List<MatSpec> Specs()
        {
            var l = new List<MatSpec>();
            l.AddRange(SurfaceRows());      // MaterialFactory.Surface.cs (SU)
            l.AddRange(FacadeRows());       // MaterialFactory.Facade.cs (FA)
            l.AddRange(LookRows());         // MaterialFactory.Look.cs (PA)
            l.AddRange(FoliageRows());      // MaterialFactory.Foliage.cs (FO)
            l.AddRange(BoulevardRows());    // MaterialFactory.Boulevard.cs (BV, Phase 4)
            l.AddRange(HeartRows());        // MaterialFactory.Heart.cs (HT, Phase 4)
            l.AddRange(SignsRows());        // MaterialFactory.Signs.cs (SG, Phase 4)
            return l;
        }

        /// <summary>
        /// Adds the PBR library shaders to GraphicsSettings.m_AlwaysIncludedShaders (Part 3.2 item 7), additively and idempotently,
        /// like ProjectConfigurator does for NocPaths.ShaderNames (which lists the framework shaders).
        /// </summary>
        public static void EnsureLibraryShadersIncluded(List<string> problems)
        {
            var gs = EditorUtil.LoadProjectSettingsObject("ProjectSettings/GraphicsSettings.asset");
            if (gs == null) { NocLog.Problem(problems, "MaterialFactory: cannot load GraphicsSettings"); return; }
            var so = new SerializedObject(gs);
            var arr = so.FindProperty("m_AlwaysIncludedShaders");
            if (arr == null) { NocLog.Problem(problems, "MaterialFactory: m_AlwaysIncludedShaders not found"); return; }
            int added = 0;
            foreach (var name in LibraryShaders)
            {
                var sh = FindShader(name);
                if (sh == null) { NocLog.Problem(problems, $"MaterialFactory: shader {name} not found (Always Included)"); continue; }
                bool present = false;
                for (int i = 0; i < arr.arraySize && !present; i++) present = arr.GetArrayElementAtIndex(i).objectReferenceValue == sh;
                if (present) continue;
                arr.InsertArrayElementAtIndex(arr.arraySize);
                arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
                added++;
            }
            if (added > 0) so.ApplyModifiedPropertiesWithoutUndo();
            NocLog.Info($"MaterialFactory: Always Included Shaders: {added} library shader(s) added");
        }

        public static Shader FindShader(string shaderName)
        {
            var sh = UnityEngine.Shader.Find(shaderName);
            if (sh != null) return sh;
            string path = ShaderCatalog.PathOf(shaderName);                 // any file under Shaders/ (Shaders/Beats included)
            if (path != null && (sh = AssetDatabase.LoadAssetAtPath<Shader>(path)) != null) return sh;
            int slash = shaderName.LastIndexOf('/');
            string leaf = slash >= 0 ? shaderName.Substring(slash + 1) : shaderName;
            return AssetDatabase.LoadAssetAtPath<Shader>(NocPaths.ShadersDir + "/" + leaf + ".shader");
        }

        public static string MaterialPath(string field) => NocPaths.MaterialsDir + "/" + field + ".mat";

        public static List<FieldInfo> LibraryFields()
        {
            var l = new List<FieldInfo>();
            foreach (var f in typeof(MaterialLibrary).GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (f.FieldType == typeof(Material)) l.Add(f);
            return l;
        }

        /// <summary>Creates / rewrites every material and the library asset.</summary>
        public static MaterialLibrary Build(List<string> problems)
        {
            EditorUtil.EnsureFolder(NocPaths.MaterialsDir);
            NocTextureGen.GenerateAll(problems);
            var specs = Specs();
            var fields = LibraryFields();

            var byName = new Dictionary<string, MatSpec>();
            foreach (var s in specs) byName[s.Field] = s;
            foreach (var f in fields)
                if (!byName.ContainsKey(f.Name)) NocLog.Problem(problems, $"MaterialFactory: MaterialLibrary.{f.Name} has no row in the material contract");

            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
            bool createdLib = lib == null;
            if (createdLib)
            {
                lib = ScriptableObject.CreateInstance<MaterialLibrary>();
                lib.name = "MaterialLibrary";
                AssetDatabase.CreateAsset(lib, NocPaths.LibraryAsset);
            }

            int made = 0;
            foreach (var s in specs)
            {
                FieldInfo field = null;
                foreach (var f in fields) if (f.Name == s.Field) { field = f; break; }
                if (field == null) { NocLog.Problem(problems, $"MaterialFactory: contract row '{s.Field}' has no MaterialLibrary field"); continue; }
                var mat = CreateOrUpdate(s, problems);
                if (mat == null) continue;
                field.SetValue(lib, mat);
                made++;
            }
            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            EnsureLibraryShadersIncluded(problems);
            NocLog.Info($"MaterialFactory: {made}/{specs.Count} materials written, library {(createdLib ? "created" : "updated")} at {NocPaths.LibraryAsset}");
            return lib;
        }

        static Material CreateOrUpdate(MatSpec s, List<string> problems)
        {
            var sh = FindShader(s.Shader);
            if (sh == null)
            {
                NocLog.Problem(problems, $"MaterialFactory: shader '{s.Shader}' not found for material {s.Field}");
                return AssetDatabase.LoadAssetAtPath<Material>(MaterialPath(s.Field));
            }
            var fresh = new Material(sh) { name = s.Field };
            Configure(fresh, s, problems);

            string path = MaterialPath(s.Field);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(fresh, path);
                return fresh;
            }
            EditorUtility.CopySerialized(fresh, existing);
            existing.name = s.Field;
            Object.DestroyImmediate(fresh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        /// <summary>The value to pass to Material.SetColor so the shader receives the linear colour (see header).</summary>
        public static Color StoredColour(ColourSpec c) => c.Linear().gamma;

        static void Configure(Material m, MatSpec s, List<string> problems)
        {
            Shader sh = m.shader;
            foreach (var kv in s.Colours)
            {
                if (!m.HasProperty(kv.Key)) { NocLog.Problem(problems, $"material {s.Field}: shader {sh.name} has no colour property {kv.Key}"); continue; }
                m.SetColor(kv.Key, StoredColour(kv.Value));
            }
            foreach (var kv in s.Floats)
            {
                if (!m.HasProperty(kv.Key)) { NocLog.Problem(problems, $"material {s.Field}: shader {sh.name} has no float property {kv.Key}"); continue; }
                m.SetFloat(kv.Key, kv.Value);
            }
            foreach (var kv in s.Textures)
            {
                if (!m.HasProperty(kv.Key)) { NocLog.Problem(problems, $"material {s.Field}: shader {sh.name} has no texture property {kv.Key}"); continue; }
                var tex = AssetDatabase.LoadAssetAtPath<Texture>(kv.Value);       // Texture: Texture2DArray flipbooks too
                if (tex == null) NocLog.Problem(problems, $"material {s.Field}: texture {kv.Value} missing");
                m.SetTexture(kv.Key, tex);
            }
            if (!string.IsNullOrEmpty(s.Keyword))
            {
                var lk = sh.keywordSpace.FindKeyword(s.Keyword);
                if (lk.isValid) m.EnableKeyword(lk);
                else NocLog.Problem(problems, $"material {s.Field}: shader {sh.name} does not declare keyword {s.Keyword}");
            }
            m.renderQueue = s.Queue;
            m.enableInstancing = s.Instancing;
            // no shadow maps in Nocturne (14.2.3): the caster pass is never needed
            m.SetShaderPassEnabled("ShadowCaster", false);
        }

        /// <summary>Contract check used by Validate and the tests: one line per problem (every material, every property).</summary>
        public static void Check(MaterialLibrary lib, List<string> problems)
        {
            if (lib == null) { problems.Add("MaterialLibrary asset missing: run RebuildAll"); return; }
            foreach (var f in LibraryFields())
                if (f.GetValue(lib) as Material == null) problems.Add($"MaterialLibrary.{f.Name} is null");
            foreach (var s in Specs())
            {
                FieldInfo field = typeof(MaterialLibrary).GetField(s.Field, BindingFlags.Public | BindingFlags.Instance);
                var m = field != null ? field.GetValue(lib) as Material : null;
                if (m == null) { problems.Add($"material {s.Field}: MaterialLibrary field is null"); continue; }
                if (m.shader == null || m.shader.name != s.Shader) { problems.Add($"material {s.Field}: shader is '{(m.shader != null ? m.shader.name : "null")}', expected '{s.Shader}'"); continue; }
                if (!m.shader.isSupported) problems.Add($"material {s.Field}: shader {s.Shader} is not supported");
                foreach (var kv in s.Colours)
                {
                    if (!m.HasProperty(kv.Key)) { problems.Add($"material {s.Field}: shader lacks colour property {kv.Key}"); continue; }
                    Color want = StoredColour(kv.Value), got = m.GetColor(kv.Key);
                    if (Mathf.Abs(want.r - got.r) > 1e-4f || Mathf.Abs(want.g - got.g) > 1e-4f || Mathf.Abs(want.b - got.b) > 1e-4f)
                        problems.Add($"material {s.Field}: {kv.Key} = {got}, contract {want}");
                }
                foreach (var kv in s.Floats)
                {
                    if (!m.HasProperty(kv.Key)) { problems.Add($"material {s.Field}: shader lacks float property {kv.Key}"); continue; }
                    float v = m.GetFloat(kv.Key);
                    if (Mathf.Abs(v - kv.Value) > 1e-5f) problems.Add($"material {s.Field}: {kv.Key} = {v}, contract {kv.Value}");
                }
                foreach (var kv in s.Textures)
                {
                    if (!m.HasProperty(kv.Key)) { problems.Add($"material {s.Field}: shader lacks texture property {kv.Key}"); continue; }
                    var t = m.GetTexture(kv.Key);
                    string got = t != null ? AssetDatabase.GetAssetPath(t) : "null";
                    if (got != kv.Value) problems.Add($"material {s.Field}: {kv.Key} = {got}, contract {kv.Value}");
                }
                if (!string.IsNullOrEmpty(s.Keyword) && !m.IsKeywordEnabled(s.Keyword)) problems.Add($"material {s.Field}: keyword {s.Keyword} not enabled");
                if (m.enableInstancing != s.Instancing) problems.Add($"material {s.Field}: enableInstancing = {m.enableInstancing}, contract {s.Instancing}");
                int wantQueue = s.Queue >= 0 ? s.Queue : m.shader.renderQueue;
                if (m.renderQueue != wantQueue) problems.Add($"material {s.Field}: renderQueue = {m.renderQueue}, contract {wantQueue}");
            }
        }
    }
}
