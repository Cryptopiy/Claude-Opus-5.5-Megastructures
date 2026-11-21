// MaterialFactory: creates every MaterialLibrary field as Assets/_Game/Materials/<FieldName>.mat with the exact shader,
// keyword, palette colours, floats, render state, render queue and instancing flag of the material contract (MASTER_PROMPT
// 14.2.6), then Assets/_Game/Materials/MaterialLibrary.asset referencing them. Imported from Thread (the machinery: in-place
// rewrite with EditorUtility.CopySerialized, so GUIDs and scene references survive every rebuild; the Check() contract;
// explicit keywords; linear colours via .gamma). Thread's rows, tints, sway, fades and keyword sets are NOT imported.
//
// Colour space: the project is Linear. Unity stores a material Color property as an sRGB value and converts it to linear
// when the shader reads it (a property flagged [HDR] is converted at SetColor time instead), so the factory passes
// BulwarkPalette.Linear(token).gamma and every shader receives exactly the linear palette value.
//
// Bulwark families (14.2.6): BwConcrete, BwSea, BwSheet, BwArmour, BwGrating, BwSteel, BwRoad, BwFarField, BwWaterPortal,
// BwMist, BwSnow, BwBeam, BwCreature, BwEmissive, BwLantern, BwLine, BwGrade. Wave A (materials owner) ships Concrete,
// Road, Steel, Grating, Armour, Emissive, Lantern next to the imported Line, Beam, Grade; each new family adds a
// ShaderFamily value, its shader name in ShaderName(), a MaterialLibrary field and a row in Specs() (BwPaths.ShaderNames
// discovers the shader for Always Included and Validate by itself).
// Textures (8.11.3): Build() first runs TextureFactory.EnsureAll (the one generator, BwTexGen; regenerated only when its
// version changes), then assigns each family's packed set (ContractTextures); Check() verifies the assignment.
// Colour contract: family defaults in ContractColours, per-row overrides with MatSpec.Col(property, token).
using System;
using System.Collections.Generic;
using System.Reflection;
using Megastructure.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Megastructure.EditorTools
{
    public enum ShaderFamily { Concrete, Line, Beam, Grade, Road, Steel, Grating, Armour, Emissive, Lantern }

    /// <summary>One row of the material contract.</summary>
    public sealed class MatSpec
    {
        public string Field;
        public ShaderFamily Family;
        public string Keyword;                       // at most one shader_feature_local keyword
        public Pal Base = Pal.ConcreteMid;
        public Pal Second = Pal.ConcreteWet;
        public readonly Dictionary<string, float> Floats = new Dictionary<string, float>();
        public readonly Dictionary<string, Pal> Colours = new Dictionary<string, Pal>();   // per-row colour overrides
        public BlendMode Src = BlendMode.One, Dst = BlendMode.Zero;
        public bool ZWrite = true;
        public CullMode Cull = CullMode.Back;
        public int Queue = -1;                       // -1 = the shader's queue
        public bool Instancing;
        public bool CastShadows = true;              // false: ShadowCaster pass disabled on the material
        public bool DepthPasses = true;              // false: DepthOnly / DepthNormals passes disabled

        public MatSpec Kw(string k) { Keyword = k; return this; }
        public MatSpec Sec(Pal p) { Second = p; return this; }
        public MatSpec F(string name, double v) { Floats[name] = (float)v; return this; }
        public MatSpec Col(string property, Pal p) { Colours[property] = p; return this; }
        public MatSpec Blend(BlendMode s, BlendMode d) { Src = s; Dst = d; return this; }
        public MatSpec NoZWrite() { ZWrite = false; return this; }
        public MatSpec CullOff() { Cull = CullMode.Off; return this; }
        public MatSpec Q(int q) { Queue = q; return this; }
        public MatSpec Inst() { Instancing = true; return this; }
        public MatSpec NoShadows() { CastShadows = false; return this; }
        public MatSpec NoDepth() { DepthPasses = false; return this; }
    }

    public static class MaterialFactory
    {
        public const string ErrorShaderName = "Hidden/InternalErrorShader";

        static readonly string[] s_LineKeywords = { "_LINE_TUBE", "_LINE_RIBBON" };
        static readonly string[] s_None = new string[0];

        // shader names of the Wave A families (BwPaths keeps the imported four)
        public const string ShaderRoad = "Megastructure/BwRoad";
        public const string ShaderSteel = "Megastructure/BwSteel";
        public const string ShaderGrating = "Megastructure/BwGrating";
        public const string ShaderArmour = "Megastructure/BwArmour";
        public const string ShaderEmissive = "Megastructure/BwEmissive";
        public const string ShaderLantern = "Megastructure/BwLantern";

        public static string ShaderName(ShaderFamily f)
        {
            switch (f)
            {
                case ShaderFamily.Concrete: return BwPaths.ShaderConcrete;
                case ShaderFamily.Line: return BwPaths.ShaderLine;
                case ShaderFamily.Beam: return BwPaths.ShaderBeam;
                case ShaderFamily.Road: return ShaderRoad;
                case ShaderFamily.Steel: return ShaderSteel;
                case ShaderFamily.Grating: return ShaderGrating;
                case ShaderFamily.Armour: return ShaderArmour;
                case ShaderFamily.Emissive: return ShaderEmissive;
                case ShaderFamily.Lantern: return ShaderLantern;
                default: return BwPaths.ShaderGrade;
            }
        }

        public static string[] FamilyKeywords(ShaderFamily f) => f == ShaderFamily.Line ? s_LineKeywords : s_None;

        static MatSpec Make(string field, ShaderFamily fam, Pal baseColour) => new MatSpec { Field = field, Family = fam, Base = baseColour };

        /// <summary>The material contract, one row per MaterialLibrary field.</summary>
        public static List<MatSpec> Specs()
        {
            int transparent = (int)RenderQueue.Transparent;
            return new List<MatSpec>
            {
                // ---- structure (BwConcrete, 14.2.6): dry / wet / basalt; instanced for InstancedBatch. Dry concrete follows
                //      the rain wetness of the atmosphere (_BwSurface.x), the wet row is wet everywhere.
                Make("Concrete", ShaderFamily.Concrete, Pal.ConcreteMid).Sec(Pal.ConcreteWet).F("_Wet", 0).F("_WetFollow", 1).Inst(),
                Make("ConcreteWet", ShaderFamily.Concrete, Pal.ConcreteMid).Sec(Pal.ConcreteWet).F("_Wet", 1).F("_WetFollow", 0).Inst(),
                Make("ConcreteDark", ShaderFamily.Concrete, Pal.ConcreteDark).Sec(Pal.ConcreteWet).F("_Wet", 0).F("_WetFollow", 1).Inst(),
                // ---- road deck (BwRoad): broomed pavement, LaneWhite lines and chainage numerals
                Make("Road", ShaderFamily.Road, Pal.ConcreteMid).Sec(Pal.ConcreteWet),
                // ---- steel (BwSteel): SteelPaint with ochre bands; SafetyOchre paint with steel-grey bands
                Make("Steel", ShaderFamily.Steel, Pal.SteelPaint).Sec(Pal.SafetyOchre).Inst(),
                Make("SteelOchre", ShaderFamily.Steel, Pal.SafetyOchre).Sec(Pal.SteelPaint).F("_Smoothness", 0.48).Inst(),
                // ---- grating (BwGrating): A2C, double sided, no shadows
                Make("Grating", ShaderFamily.Grating, Pal.SteelPaint).CullOff().NoShadows(),
                // ---- armour units (BwArmour): instanced islets
                Make("Armour", ShaderFamily.Armour, Pal.ConcreteDark).Sec(Pal.ConcreteWet).Inst(),
                // ---- thin lines (BwLine, D-07): round tubes with A2C under MSAA; rails 0.10 m (14.5.2/6/7), handrails 0.05 m (14.5.5)
                Make("Rail", ShaderFamily.Line, Pal.SteelPaint).Kw("_LINE_TUBE").F("_LineWidth", BulwarkConst.LeeRailDiameter).F("_Metallic", 0.0).F("_Smoothness", 0.45).CullOff().NoShadows(),
                Make("Handrail", ShaderFamily.Line, Pal.SafetyOchre).Kw("_LINE_TUBE").F("_LineWidth", 0.05).F("_Smoothness", 0.4).CullOff().NoShadows(),
                // ---- light shafts (BwBeam): additive, no depth write, transparent queue
                Make("Beam", ShaderFamily.Beam, Pal.FoamWhite).Blend(BlendMode.One, BlendMode.One).NoZWrite().CullOff().Q(transparent - 50).NoShadows().NoDepth(),
                // ---- lamps (BwEmissive, clamp 3) and the lantern (BwLantern, x6, clamp 6): no shadow casting
                Make("LampSodium", ShaderFamily.Emissive, Pal.SodiumWarm).F("_Intensity", BulwarkConst.KeyB1Sodium).NoShadows().Inst(),
                Make("LampTungsten", ShaderFamily.Emissive, Pal.Tungsten).F("_Intensity", BulwarkConst.KeyB5Tungsten).F("_GainFollow", 1).NoShadows().Inst(),
                Make("Lantern", ShaderFamily.Lantern, Pal.Lantern).NoShadows(),
                // ---- post (BwGradeFeature draws its passes by index; no properties)
                Make("Grade", ShaderFamily.Grade, Pal.ConcreteMid),
            };
        }

        /// <summary>Colour properties of the contract per family, with the token each one receives.</summary>
        public static List<KeyValuePair<string, Pal>> ContractColours(MatSpec s)
        {
            var l = new List<KeyValuePair<string, Pal>>();
            void Add(string prop, Pal p) => l.Add(new KeyValuePair<string, Pal>(prop, s.Colours.TryGetValue(prop, out Pal o) ? o : p));
            switch (s.Family)
            {
                case ShaderFamily.Concrete:
                    Add("_BaseColor", s.Base);
                    Add("_WetColor", s.Second);
                    break;
                case ShaderFamily.Road:
                    Add("_BaseColor", s.Base);
                    Add("_WetColor", s.Second);
                    Add("_LineColor", Pal.LaneWhite);
                    break;
                case ShaderFamily.Steel:
                    Add("_BaseColor", s.Base);
                    Add("_BandColor", s.Second);
                    break;
                case ShaderFamily.Grating:
                case ShaderFamily.Line:
                    Add("_BaseColor", s.Base);
                    break;
                case ShaderFamily.Armour:
                    Add("_BaseColor", s.Base);
                    Add("_WetColor", s.Second);
                    Add("_FoamColor", Pal.FoamWhite);
                    Add("_BandColor", Pal.SafetyOchre);
                    break;
                case ShaderFamily.Beam:
                case ShaderFamily.Emissive:
                case ShaderFamily.Lantern:
                    Add("_EmissionColor", s.Base);
                    break;
            }
            return l;
        }

        /// <summary>Texture properties of the contract: (property, BwTexGen set, map).</summary>
        public static List<(string prop, string set, TextureFactory.Map map)> ContractTextures(MatSpec s)
        {
            var l = new List<(string, string, TextureFactory.Map)>();
            void Set3(string set, string a, string n, string m)
            {
                l.Add((a, set, TextureFactory.Map.Albedo));
                l.Add((n, set, TextureFactory.Map.Normal));
                l.Add((m, set, TextureFactory.Map.Mask));
            }
            switch (s.Family)
            {
                case ShaderFamily.Concrete: Set3("Concrete", "_AlbedoMap", "_NormalMap", "_MaskMap"); break;
                case ShaderFamily.Road: Set3("Road", "_AlbedoMap", "_NormalMap", "_MaskMap"); break;
                case ShaderFamily.Steel:
                case ShaderFamily.Grating: Set3("Steel", "_AlbedoMap", "_NormalMap", "_MaskMap"); break;
                case ShaderFamily.Armour:
                    Set3("Concrete", "_AlbedoMap", "_NormalMap", "_MaskMap");
                    Set3("Crust", "_CrustAlbedo", "_CrustNormal", "_CrustMask");
                    break;
            }
            return l;
        }

        /// <summary>Every float property of the contract (family defaults overridden by the row), incl. render state.</summary>
        public static Dictionary<string, float> ContractFloats(MatSpec s)
        {
            var d = new Dictionary<string, float>();
            switch (s.Family)
            {
                case ShaderFamily.Concrete:
                    d["_Wet"] = 0f; d["_WetFollow"] = 0f; d["_Smoothness"] = 0.30f; d["_WetSmoothness"] = 0.72f;
                    d["_PanelGap"] = 0.012f; d["_TieSpacing"] = 0.6f; d["_TieRadius"] = 0.012f; d["_MacroTone"] = 0.06f;
                    d["_AO"] = 1f; d["_EdgeWear"] = 0.5f; d["_TexScale"] = BwTexGen.ConcreteTile; d["_NormalStrength"] = 1f;
                    d["_DetailMix"] = 0.6f; d["_DripStrength"] = 0.6f; d["_SunTint"] = 1.9f;
                    d["_Cull"] = (float)s.Cull;
                    break;
                case ShaderFamily.Road:
                    d["_Wet"] = 0f; d["_WetFollow"] = 1f; d["_Smoothness"] = 0.30f; d["_WetSmoothness"] = 0.72f; d["_WetAlbedo"] = 0.8f; d["_PaintSmoothness"] = 0.45f;
                    d["_MacroTone"] = 0.06f; d["_KerbGrime"] = 0.12f; d["_AO"] = 1f; d["_EdgeWear"] = 0.5f;
                    d["_TexScale"] = BwTexGen.RoadTile; d["_NormalStrength"] = 1f; d["_DetailMix"] = 0.6f;
                    d["_LineWidth"] = 0.15f; d["_EdgeLineX"] = 5.0f; d["_CenterDash"] = 3f; d["_CenterGap"] = 6f; d["_DigitsOn"] = 1f;
                    d["_Cull"] = (float)s.Cull;
                    break;
                case ShaderFamily.Steel:
                    d["_Smoothness"] = 0.52f; d["_BareSmoothness"] = 0.5f; d["_EdgeWear"] = 0.8f; d["_ChipAmount"] = 0.12f;
                    d["_RustAmount"] = 0.6f; d["_BandMode"] = 0f; d["_BandPeriod"] = 0.4f; d["_BandWidth"] = 0.2f; d["_AO"] = 1f;
                    d["_TexScale"] = BwTexGen.SteelTile; d["_NormalStrength"] = 1f; d["_DetailMix"] = 0.6f;
                    d["_Cull"] = (float)s.Cull;
                    break;
                case ShaderFamily.Grating:
                    d["_BarWidth"] = (float)BulwarkConst.GratingBar; d["_BarPitch"] = (float)BulwarkConst.GratingPitch;
                    d["_BarDepth"] = 0.03f; d["_BarAxis"] = 0f; d["_FadePx"] = 3f; d["_Smoothness"] = 0.42f; d["_TopWear"] = 0.35f;
                    d["_TexScale"] = BwTexGen.SteelTile;
                    d["_Cull"] = (float)s.Cull;
                    break;
                case ShaderFamily.Armour:
                    d["_SeaLevel"] = (float)BulwarkConst.SeaY; d["_WetHeight"] = 5f; d["_SwashHeight"] = 3f; d["_FoamWidth"] = 0.35f;
                    d["_Smoothness"] = 0.26f; d["_WetSmoothness"] = 0.45f; d["_MacroTone"] = 0.06f; d["_AO"] = 1f; d["_EdgeWear"] = 0.4f;
                    d["_TexScale"] = BwTexGen.ConcreteTile; d["_CrustScale"] = BwTexGen.CrustTile; d["_NormalStrength"] = 1f;
                    d["_CrustStrength"] = 1f; d["_GaugeBands"] = 0f; d["_GaugePitch"] = 0.5f;
                    d["_Cull"] = (float)s.Cull;
                    break;
                case ShaderFamily.Emissive:
                    d["_Intensity"] = 2.5f; d["_Clamp"] = (float)BulwarkConst.ClampLamp; d["_GlassFalloff"] = 0.35f; d["_Smoothness"] = 0.8f; d["_GainFollow"] = 0f;
                    d["_Cull"] = (float)s.Cull;
                    break;
                case ShaderFamily.Lantern:
                    d["_Intensity"] = (float)BulwarkConst.LanternEmissive; d["_Clamp"] = (float)BulwarkConst.ClampLantern;
                    d["_Flicker"] = (float)BulwarkConst.LanternFlicker; d["_GlassFalloff"] = 0.45f; d["_Smoothness"] = 0.85f;
                    d["_Cull"] = (float)s.Cull;
                    break;
                case ShaderFamily.Line:
                    d["_LineWidth"] = 0.05f;
                    d["_MinWidthPx"] = 0f;                                   // 0 = the global _BwScreen.z rule
                    d["_AlphaFloor"] = 0.35f;                                // 14.7 H7 energy alpha floor
                    d["_FadeNear"] = 0f;
                    d["_FadeFar"] = 0f;
                    d["_SrcBlend"] = (float)s.Src;
                    d["_DstBlend"] = (float)s.Dst;
                    d["_ZWrite"] = s.ZWrite ? 1f : 0f;
                    d["_Cull"] = (float)s.Cull;
                    break;
                case ShaderFamily.Beam:
                    d["_Cull"] = (float)s.Cull;
                    d["_BwFade"] = 1f;
                    break;
                default:
                    return d;   // grade: no material properties
            }
            foreach (var kv in s.Floats) d[kv.Key] = kv.Value;
            return d;
        }

        /// <summary>An [HDR] colour property or one named like an emission: it gets black unless the contract names it.</summary>
        public static bool IsEmissive(Shader sh, int index)
        {
            if ((sh.GetPropertyFlags(index) & ShaderPropertyFlags.HDR) != 0) return true;
            return sh.GetPropertyName(index).IndexOf("Emission", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Alpha-to-coverage (with MSAA) for the opaque thin-line tubes; blended / additive materials never.</summary>
        public static bool UsesAlphaToMask(MatSpec s) => s.Keyword == "_LINE_TUBE" || s.Family == ShaderFamily.Grating;

        public static Shader FindShader(string shaderName)
        {
            var sh = Shader.Find(shaderName);
            if (sh != null) return sh;
            int slash = shaderName.LastIndexOf('/');
            string leaf = slash >= 0 ? shaderName.Substring(slash + 1) : shaderName;
            return AssetDatabase.LoadAssetAtPath<Shader>(BwPaths.ShadersDir + "/" + leaf + ".shader");
        }

        /// <summary>The value to pass to Material.SetColor so the shader receives the linear token (see header).</summary>
        public static Color StoredColour(Pal p) => BulwarkPalette.Linear(p).gamma;

        public static string MaterialPath(string field) => BwPaths.MaterialsDir + "/" + field + ".mat";

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
            EditorUtil.EnsureFolder(BwPaths.MaterialsDir);
            try { TextureFactory.EnsureAll(problems); }
            catch (Exception e) { BwLog.Problem(problems, "MaterialFactory: texture generation failed: " + e); }
            var specs = Specs();
            var fields = LibraryFields();

            var byName = new Dictionary<string, MatSpec>();
            foreach (var s in specs) byName[s.Field] = s;
            foreach (var f in fields)
                if (!byName.ContainsKey(f.Name)) BwLog.Problem(problems, $"MaterialFactory: MaterialLibrary.{f.Name} has no row in the material contract");

            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(BwPaths.LibraryAsset);
            bool createdLib = lib == null;
            if (createdLib)
            {
                lib = ScriptableObject.CreateInstance<MaterialLibrary>();
                lib.name = "MaterialLibrary";
                AssetDatabase.CreateAsset(lib, BwPaths.LibraryAsset);
            }

            int made = 0;
            foreach (var s in specs)
            {
                FieldInfo field = null;
                foreach (var f in fields) if (f.Name == s.Field) { field = f; break; }
                if (field == null) { BwLog.Problem(problems, $"MaterialFactory: contract row '{s.Field}' has no MaterialLibrary field"); continue; }
                var mat = CreateOrUpdate(s, problems);
                if (mat == null) continue;
                field.SetValue(lib, mat);
                made++;
            }
            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            BwLog.Info($"MaterialFactory: {made}/{specs.Count} materials written, library {(createdLib ? "created" : "updated")} at {BwPaths.LibraryAsset}");
            return lib;
        }

        static Material CreateOrUpdate(MatSpec s, List<string> problems)
        {
            string shaderName = ShaderName(s.Family);
            var sh = FindShader(shaderName);
            if (sh == null)
            {
                BwLog.Problem(problems, $"MaterialFactory: shader '{shaderName}' not found for material {s.Field} (using {ErrorShaderName} until it exists)");
                sh = Shader.Find(ErrorShaderName);
                if (sh == null) return AssetDatabase.LoadAssetAtPath<Material>(MaterialPath(s.Field));
            }

            var fresh = new Material(sh) { name = s.Field };
            Configure(fresh, s, sh.name == shaderName);

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

        /// <summary>Applies one contract row to a fresh material (no previous keywords or properties).</summary>
        static void Configure(Material m, MatSpec s, bool realShader)
        {
            Shader sh = m.shader;

            // Colours: contract tokens first. Every other colour property of the shader: emission / HDR colours = black
            // (a non-contract emission would make every surface glow: Phase 2 found _Emission = ConcreteMid flattening
            // all concrete to one grey), any other colour = ConcreteMid (a visible neutral).
            var colours = ContractColours(s);
            var assigned = new HashSet<string>();
            foreach (var kv in colours)
            {
                if (realShader && !m.HasProperty(kv.Key)) BwLog.Warn($"material {s.Field}: shader {sh.name} has no colour property {kv.Key}");
                m.SetColor(kv.Key, StoredColour(kv.Value));
                assigned.Add(kv.Key);
            }
            int n = sh.GetPropertyCount();
            for (int i = 0; i < n; i++)
            {
                if (sh.GetPropertyType(i) != ShaderPropertyType.Color) continue;
                string pn = sh.GetPropertyName(i);
                if (assigned.Contains(pn)) continue;
                if (IsEmissive(sh, i))
                {
                    m.SetColor(pn, Color.black);
                    if (realShader) BwLog.Info($"material {s.Field}: extra emission colour {pn} set to black");
                    continue;
                }
                m.SetColor(pn, StoredColour(Pal.ConcreteMid));
                if (realShader) BwLog.Info($"material {s.Field}: extra colour property {pn} set to ConcreteMid");
            }

            // Floats (incl. render state).
            foreach (var kv in ContractFloats(s))
            {
                if (realShader && !m.HasProperty(kv.Key)) BwLog.Warn($"material {s.Field}: shader {sh.name} has no float property {kv.Key}");
                m.SetFloat(kv.Key, kv.Value);
            }
            // Optional alpha-to-coverage toggle (thin-line tubes use A2C with MSAA; additive/blended never).
            if (m.HasProperty("_AlphaToMask")) m.SetFloat("_AlphaToMask", UsesAlphaToMask(s) ? 1f : 0f);

            // Textures: the family's BwTexGen sets (TextureFactory assets).
            foreach (var t in ContractTextures(s))
            {
                var tex = TextureFactory.Load(t.set, t.map);
                if (tex == null) { BwLog.Warn($"material {s.Field}: texture {TextureFactory.AssetPath(t.set, t.map)} missing"); continue; }
                if (realShader && !m.HasProperty(t.prop)) BwLog.Warn($"material {s.Field}: shader {sh.name} has no texture property {t.prop}");
                m.SetTexture(t.prop, tex);
            }

            // Keyword: exactly the contract one (fresh material has none enabled).
            if (!string.IsNullOrEmpty(s.Keyword))
            {
                var lk = sh.keywordSpace.FindKeyword(s.Keyword);
                if (lk.isValid) m.EnableKeyword(lk);
                else
                {
                    m.EnableKeyword(s.Keyword);   // kept as an invalid keyword until the shader declares it
                    if (realShader) BwLog.Warn($"material {s.Field}: shader {sh.name} does not declare keyword {s.Keyword}");
                }
            }

            m.renderQueue = s.Queue;
            m.enableInstancing = s.Instancing;
            m.SetShaderPassEnabled("ShadowCaster", s.CastShadows);
            m.SetShaderPassEnabled("DepthOnly", s.DepthPasses);
            m.SetShaderPassEnabled("DepthNormals", s.DepthPasses);
            if (s.Queue > (int)RenderQueue.GeometryLast) m.SetOverrideTag("RenderType", "Transparent");
        }

        /// <summary>Contract check used by Validate and the tests: returns one line per problem.</summary>
        public static void Check(MaterialLibrary lib, List<string> problems)
        {
            if (lib == null) { problems.Add("MaterialLibrary asset missing: run RebuildAll"); return; }
            foreach (var s in Specs())
            {
                FieldInfo field = typeof(MaterialLibrary).GetField(s.Field, BindingFlags.Public | BindingFlags.Instance);
                var m = field != null ? field.GetValue(lib) as Material : null;
                if (m == null) { problems.Add($"material {s.Field}: MaterialLibrary field is null"); continue; }
                string want = ShaderName(s.Family);
                if (m.shader == null || m.shader.name != want) { problems.Add($"material {s.Field}: shader is '{(m.shader != null ? m.shader.name : "null")}', expected '{want}'"); continue; }
                var contractColours = new HashSet<string>();
                foreach (var kv in ContractColours(s))
                {
                    contractColours.Add(kv.Key);
                    if (!m.HasProperty(kv.Key)) problems.Add($"material {s.Field}: shader {want} lacks colour property {kv.Key}");
                }
                for (int i = 0; i < m.shader.GetPropertyCount(); i++)
                {
                    if (m.shader.GetPropertyType(i) != ShaderPropertyType.Color || !IsEmissive(m.shader, i)) continue;
                    string pn = m.shader.GetPropertyName(i);
                    if (contractColours.Contains(pn)) continue;
                    Color e = m.GetColor(pn);
                    if (Mathf.Max(e.r, Mathf.Max(e.g, e.b)) > 1e-6f) problems.Add($"material {s.Field}: non-contract emission {pn} = {e}, must be black");
                }
                foreach (var kv in ContractFloats(s))
                {
                    if (!m.HasProperty(kv.Key)) { problems.Add($"material {s.Field}: shader {want} lacks float property {kv.Key}"); continue; }
                    float v = m.GetFloat(kv.Key);
                    if (Mathf.Abs(v - kv.Value) > 1e-5f) problems.Add($"material {s.Field}: {kv.Key} = {v}, contract {kv.Value}");
                }
                foreach (var t in ContractTextures(s))
                {
                    if (!m.HasProperty(t.prop)) { problems.Add($"material {s.Field}: shader {want} lacks texture property {t.prop}"); continue; }
                    var tex = m.GetTexture(t.prop);
                    string want2 = TextureFactory.AssetPath(t.set, t.map);
                    if (tex == null) problems.Add($"material {s.Field}: {t.prop} is null (expected {want2}; run RebuildAll)");
                    else if (AssetDatabase.GetAssetPath(tex) != want2) problems.Add($"material {s.Field}: {t.prop} = {AssetDatabase.GetAssetPath(tex)}, contract {want2}");
                }
                int enabled = 0;
                foreach (var k in FamilyKeywords(s.Family)) if (m.IsKeywordEnabled(k)) enabled++;
                if (enabled > 1) problems.Add($"material {s.Field}: {enabled} pattern/mode keywords enabled (max 1)");
                if (!string.IsNullOrEmpty(s.Keyword))
                {
                    if (!m.shader.keywordSpace.FindKeyword(s.Keyword).isValid) problems.Add($"material {s.Field}: shader {want} does not declare keyword {s.Keyword}");
                    else if (!m.IsKeywordEnabled(s.Keyword)) problems.Add($"material {s.Field}: keyword {s.Keyword} not enabled");
                }
                if (m.enableInstancing != s.Instancing) problems.Add($"material {s.Field}: enableInstancing = {m.enableInstancing}, contract {s.Instancing}");
                int wantQueue = s.Queue >= 0 ? s.Queue : m.shader.renderQueue;
                if (m.renderQueue != wantQueue) problems.Add($"material {s.Field}: renderQueue = {m.renderQueue}, contract {wantQueue}");
            }
        }
    }
}
