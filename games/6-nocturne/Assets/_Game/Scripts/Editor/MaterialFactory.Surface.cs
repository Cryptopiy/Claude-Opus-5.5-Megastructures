// MaterialFactory.Surface: the near-surface rows of the material contract (owner SU; TEXTURE_PLAN 8.3 / 8.4): the graybox set,
// the NocSurface PBR surfaces, the NocWet floors, the NocGlass faces and the NocLine row, plus their row helpers. Created by TP-0
// (TEXTURE_PLAN 8.3) with the rows moved VERBATIM from MaterialFactory.cs (no picture changes) and handed over to SU. MatSpec,
// Build and Check stay in MaterialFactory.cs (TP); Specs() concatenates SurfaceRows, FacadeRows, LookRows and FoliageRows.
// Processed Look v2 sets are wired with MatSpec.ExtMaps(key) (the tile comes from the texmeta).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Megastructure.EditorTools
{
    public static partial class MaterialFactory
    {
        /// <summary>Look v2 near-surface shaders of SU (TEXTURE_PLAN 8.3).</summary>
        public const string ShaderGrating = "Megastructure/NocGrating";
        public const string ShaderShopfront = "Megastructure/NocShopfront";

        const float G1MaterialsShopWarm = 0.9f;                                       // G1Materials.ShopWarmFactor (the default slot)

        // docs/MATERIALS.md: WetStone and MetalDark are ConcreteDark +-10 % (14.2.1); bare-metal F0 colours from tokens
        const float WetStone = 0.9f, MetalDark = 1.05f, SteelF0 = 0.55f, BrassF0 = 0.4f, BrassF0b = 0.45f;   // brass F0 (0.80, 0.61, 0.42)

        /// <summary>A NocSurface row (docs/MATERIALS.md column order).</summary>
        static MatSpec Surface(string field, NocPal token, float factor, NocTexFamily maps, double tile, double metal, double sMin, double sMax,
                               double normal, double macroScale, double macroAlbedo, double macroSmooth, double wet, double streaks, double wear = 0)
        {
            return Make(field, ShaderSurface).C("_BaseColor", token, factor).C("_EmissionColor", NocPal.Floor, 0f)
                .C("_WearColor", NocPal.SignWhite, SteelF0).Maps(maps, tile)
                .F("_Metallic", metal).F("_SmoothMin", sMin).F("_SmoothMax", sMax).F("_NormalStrength", normal)
                .F("_MacroScale", macroScale).F("_MacroAlbedo", macroAlbedo).F("_MacroSmooth", macroSmooth)
                .F("_AO", 1).F("_EdgeWear", wear).F("_Wetness", wet).F("_Streaks", streaks).F("_Cull", (float)CullMode.Back).Inst();
        }

        /// <summary>A NocWet row: albedo token x factor, maps, puddle fraction, GGX alpha dry (14.2.5: puddles 0.08, elsewhere 0.35).</summary>
        static MatSpec Wet(string field, NocPal token, float factor, NocTexFamily maps, double tile, double puddles, double alphaDry, double normal)
        {
            return Make(field, ShaderWet).C("_BaseColor", token, factor).C("_EmissionColor", NocPal.Floor, 0f).Maps(maps, tile)
                .F("_NormalStrength", normal).F("_PuddleFraction", puddles)
                .F("_AlphaPuddle", NocturneConstants.WetAlphaPuddle).F("_AlphaDry", alphaDry)
                .F("_ReflPuddle", NocturneConstants.WetReflPuddle).F("_ReflDry", NocturneConstants.WetReflDry)
                .F("_MacroScale", 11).F("_MacroAlbedo", 0.15).F("_AO", 0.5).F("_Cull", (float)CullMode.Back).Inst();
        }

        /// <summary>The Look v2 (-surf-v 2) NocSurface parameters of a row (TEXTURE_PLAN 3.2 / 3.3, LOOK_V2 5.2; the v1 path reads none):
        /// porosity, wet level (exposed 1, sheltered 0.15), leaks anchored to the face top (the vertex bake runs for _LeakAnchor 1)
        /// and their strength, hex rotation (0 = offsets only for directional sets), the anti-tiling seed and the mapping
        /// (0 world triplanar, 1 UV0 metres).</summary>
        static MatSpec SurfV2(MatSpec m, double porosity, double wetLevel, bool leaks, double leakGain, double hexRotate, int seed, int mapping = 0)
            => m.F("_Porosity", porosity).F("_WetLevel", wetLevel).F("_LeakAnchor", leaks ? 1 : 0).F("_LeakGain", leakGain)
                .F("_HexRotate", hexRotate).F("_TexSeed", seed).F("_Mapping", mapping);

        /// <summary>Applies f to the row (the row helpers chain without temporaries).</summary>
        static MatSpec With(this MatSpec m, System.Func<MatSpec, MatSpec> f) => f(m);

        /// <summary>TP's OverlayArr (leaks, glass grime, rivulets, rust bleed) and MicroArr (micro detail) on a row's v2 array
        /// properties, when their manifests exist.</summary>
        static MatSpec Arrays(MatSpec m, bool micro = true)
        {
            string o = NocTextureImport.ManifestOf("OverlayArr");
            if (o != null)
            {
                string pre = o.Substring(0, o.Length - TexMeta.Suffix.Length);
                m.Tx("_OverlayMask", pre + "_Mask.png").Tx("_OverlayNormal", pre + "_Normal.png");
            }
            string mi = micro ? NocTextureImport.ManifestOf("MicroArr") : null;
            if (mi != null)
            {
                string pre = mi.Substring(0, mi.Length - TexMeta.Suffix.Length);
                m.Tx("_MicroAlbedo", pre + "_Albedo.png").Tx("_MicroNormal", pre + "_Normal.png");
            }
            return m;
        }

        /// <summary>The Look v2 photo set of a row (TP-1, Textures/External/key_*.png + key.texmeta.json) on the v2-only properties
        /// (_AlbedoTex2 / _NormalTex2 / _MaskTex2, _Tile2 / _TileV2 from the texmeta, smoothness 0..1 for the absolute mask, normal
        /// strength 1: the class strength is baked), the LOOK_V2 5.1 token x factor in _BaseColor2, and the module shuffle of gridded
        /// sets (moduleU / V, running bond and bond axis from the texmeta; brick shuffles whole courses: _ModuleU 0 = the source course
        /// hashed per course only plus a hashed u offset per course, so no vertical cut runs through a brick). The v1
        /// path keeps the generated maps until SU-5. A key without a manifest leaves _Tex2 at 0 (Build reports nothing missing).</summary>
        static MatSpec Photo(MatSpec m, string key, NocPal token, float factor = 1f, bool wholeCourses = false)
        {
            if (m.Shader == ShaderSurface)
            {
                // the leak decal and the micro layer only with their arrays (no tap of an unbound array)
                Arrays(m);
                if (!m.Textures.ContainsKey("_OverlayMask")) m.F("_LeakDecal", 0);
                if (!m.Textures.ContainsKey("_MicroAlbedo")) m.F("_MicroGain", 0);
            }
            string meta = NocTextureImport.ManifestOf(key);
            if (meta == null) return m.F("_Tex2", 0);
            string prefix = meta.Substring(0, meta.Length - TexMeta.Suffix.Length);
            var t = TexMeta.Load(NocPaths.Abs(meta));
            if (t == null) return m.F("_Tex2", 0);
            m.Tx("_AlbedoTex2", prefix + "_Albedo.png").Tx("_NormalTex2", prefix + "_Normal.png").Tx("_MaskTex2", prefix + "_Mask.png")
             .C("_BaseColor2", token, factor).F("_Tex2", 1).F("_Tile2", t.tile > 0f ? t.tile : 1f).F("_TileV2", t.tileV > 0f && Mathf.Abs(t.tileV - t.tile) > 1e-4f ? t.tileV : 0f)
             .F("_SmoothMin2", 0).F("_SmoothMax2", 1).F("_NormalStrength2", 1);
            bool module = t.gridded && t.moduleU > 0f && t.moduleV > 0f;
            m.F("_AntiTiling", module ? 1 : 0).F("_ModuleU", module ? (wholeCourses ? 0f : t.moduleU) : 1).F("_ModuleV", module ? t.moduleV : 1)
             .F("_Bond", module && t.runningBond ? 1 : 0).F("_BondAxis", module && t.bondAxis == "v" ? 1 : 0);
            if (module) m.F("_HexRotate", 0);                                        // gridded sets never rotate
            return m;
        }

        /// <summary>Heights (mask B, 0..1) of a processed set at n hashed texels of its _Mask PNG (the shader's h on the photo path),
        /// optionally with the deck plates' sag; null without a manifest. MaterialFactory measures _PuddleTh2 from it, the tests too.</summary>
        public static List<double> PhotoHeights(string key, int n = 16384, bool plates = false, uint seed = 1u)
        {
            string meta = NocTextureImport.ManifestOf(key);
            if (meta == null) return null;
            string png = NocPaths.Abs(meta.Substring(0, meta.Length - TexMeta.Suffix.Length) + "_Mask.png");
            if (!System.IO.File.Exists(png)) return null;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            try
            {
                if (!tex.LoadImage(System.IO.File.ReadAllBytes(png), false)) return null;
                var px = tex.GetPixels32();
                int w = tex.width, hgt = tex.height;
                var l = new List<double>(n);
                for (int k = 0; k < n; k++)
                {
                    uint a = NocMath.Pcg((uint)k * 2654435761u + seed * 0x9E3779B9u + 977u), b = NocMath.Pcg(a + 0x68E31DA4u);
                    int x = (int)(NocMath.U01(a) * w), y = (int)(NocMath.U01(b) * hgt);
                    double h = px[Mathf.Clamp(y, 0, hgt - 1) * w + Mathf.Clamp(x, 0, w - 1)].b / 255.0;
                    if (plates)
                    {
                        uint c = NocMath.Pcg(b + 0xB5297A4Du), d = NocMath.Pcg(c + 0x1B873593u);
                        h = NocMath.Clamp01(h + NocSurfaceMath.PlateSag(System.Math.Max(NocMath.U01(c), NocMath.U01(d))));
                    }
                    l.Add(h);
                }
                return l;
            }
            finally { Object.DestroyImmediate(tex); }
        }

        /// <summary>FX (C04, fix wave p4_01; LOOK_V2 5.2 "up-facing 1.0"): the rain film's gloss lift of the open, up-facing wet floors
        /// (street, alley, pier, plazas): smoothness lerp(photo, SurfWetGloss 0.88, 0.8) = 0.70-0.78, perceptual roughness 0.22-0.30
        /// (0.4-0.55 left 0.45-0.55, which read as dry paving in p4_01); the steel deck 0.7. The garden path and moss stay (sheltered).</summary>
        const double WetUpGloss = 0.8, WetUpGlossDeck = 0.7;
        /// <summary>FX (C04): the alley film's reflectivity (WetReflDry 0.5 elsewhere): with the sharper film (SurfAlphaDryMin 0.08) the
        /// alley floor rose 32.2 -> 35.1 codes and KS01's mean 39.5 -> 40.6, over beat 1's 14-40 window (FX_r12); 0.38 gave 40.3
        /// (FX_r13, 0.023 codes per 0.01), 0.18 aims at 39.8; the puddles (0.95) keep the bar's streak.</summary>
        const double WetAlleyReflDry = 0.18;

        /// <summary>A NocWet row's photo set (Photo) plus the threshold of its own heights (_PuddleTh2, the row's puddle fraction) and the
        /// rain film's lift of its absolute dry smoothness (_WetGloss2).</summary>
        static MatSpec PhotoWet(MatSpec m, string key, NocPal token, float factor, double fraction, double wetGloss, bool plates = false, bool wholeCourses = false)
        {
            Photo(m, key, token, factor, wholeCourses);
            if (!m.Floats.TryGetValue("_Tex2", out float on) || on < 0.5f) return m;
            string k = "photo/" + key + "/" + fraction.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + (plates ? "/p" : "");
            if (!s_puddleTh.TryGetValue(k, out double th))
            {
                var hs = PhotoHeights(key, 16384, plates, 1u);
                th = hs != null ? NocSurfaceMath.PuddleThreshold(hs, fraction, 400000) : NocSurfaceConstants.SurfPuddleTh0;
                s_puddleTh[k] = th;
            }
            return m.F("_PuddleTh2", th).F("_WetGloss2", wetGloss);
        }

        /// <summary>NocGlass v2 overlays (TEXTURE_PLAN 3.2): grime x1 and rivulets x1 from OverlayArr slices 1 / 2 (off without it).</summary>
        static MatSpec GlassV2(MatSpec m)
        {
            Arrays(m, false);
            bool on = m.Textures.ContainsKey("_OverlayMask");
            return m.F("_GrimeGain", on ? 1 : 0).F("_RivuletGain", on ? 1 : 0);
        }

        static readonly Dictionary<string, double> s_puddleTh = new Dictionary<string, double>();

        /// <summary>The Look v2 (-surf-v 2) NocWet parameters of a row (TEXTURE_PLAN 3.3; the v1 path reads none of them): _PuddleTh =
        /// the (1 - fraction) quantile of noise + 0.35 (0.5 - h) over the family's own height histogram (NocSurfaceMath, 4 x 10^5
        /// samples, cached), so the puddle fraction stays 0.35 +- 0.02; the wet smoothness range of LOOK_V2 5.1 (rain everywhere: the
        /// floors are always wet); the analytic deck plates (FloorSteelDeck 2.4 x 1.2 m, long side along the bridge, x).</summary>
        static MatSpec WetV2(MatSpec m, NocTexFamily maps, double fraction, double sMin, double sMax, double plateU = 0, double plateV = 0)
        {
            string key = maps + "/" + fraction.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "/" + (plateU > 0 ? "p" : "");
            if (!s_puddleTh.TryGetValue(key, out double th))
            {
                th = NocSurfaceMath.PuddleThreshold(NocSurfaceMath.HeightHistogram(maps, 16384, plateU > 0, 1u), fraction, 400000);
                s_puddleTh[key] = th;
            }
            return m.F("_PuddleTh", th).F("_SmoothMin", sMin).F("_SmoothMax", sMax).F("_PlateU", plateU).F("_PlateV", plateV);
        }

        /// <summary>Graybox, NocSurface, NocWet, NocGlass and NocLine rows (SU).</summary>
        static List<MatSpec> SurfaceRows()
        {
            return new List<MatSpec>
            {
                // ---- graybox: the floor slabs are the wet stone floor (NocWet, the analytic mirror on every graybox beat)
                WetV2(Wet("GrayFloor", NocPal.ConcreteDark, WetStone, NocTexFamily.Paving, 2.4, NocturneConstants.WetPuddleFraction, NocturneConstants.WetAlphaDry, 0.6),
                      NocTexFamily.Paving, NocturneConstants.WetPuddleFraction, 0.45, 0.65)
                      .With(x => PhotoWet(x, "Pavers", NocPal.ConcreteDark, WetStone, NocturneConstants.WetPuddleFraction, WetUpGloss)),
                Make("GrayWall", NocPaths.ShaderGraybox).C("_BaseColor", NocPal.ConcreteDark).C("_EmissionColor", NocPal.Floor, 0f)
                    .F("_Smoothness", 0.25).F("_GridStrength", 0).F("_AO", 1).F("_Cull", (float)CullMode.Back).Inst(),
                Make("GrayMarker", NocPaths.ShaderGraybox).C("_BaseColor", NocPal.WetAsphalt).C("_EmissionColor", NocPal.NeonCyan, NocturnePalette.EmissionSign)
                    .F("_Smoothness", 0.3).F("_GridStrength", 0).F("_AO", 0).F("_Cull", (float)CullMode.Back).Inst(),
                Make("GrayMarkerWarm", NocPaths.ShaderGraybox).C("_BaseColor", NocPal.WetAsphalt).C("_EmissionColor", NocPal.SodiumAmber, NocturnePalette.EmissionSign)
                    .F("_Smoothness", 0.3).F("_GridStrength", 0).F("_AO", 0).F("_Cull", (float)CullMode.Back).Inst(),
                // ---- PBR surfaces (NocSurface; docs/MATERIALS.md: token x factor, maps, tile m, metallic, smoothness range, normal,
                //      macro scale m / albedo +- / smoothness +-, wetness, drip streaks, edge wear)
                SurfV2(Surface("Concrete", NocPal.ConcreteDark, 1f, NocTexFamily.Concrete, 3.0, 0, 0.15, 0.5, 1.0, 9, 0.18, 0.08, 0.5, 0.5),
                       NocSurfaceConstants.SurfPorConcrete, 1, true, 0.4, 1, 1)
                       .F("_LeakDecal", 1).F("_MicroGain", 1).F("_MicroSlice", 0).With(x => Photo(x, "Concrete", NocPal.ConcreteDark)),
                SurfV2(Surface("StoneWall", NocPal.ConcreteDark, WetStone, NocTexFamily.PlazaStone, 4.8, 0, 0.3, 0.6, 0.8, 13, 0.12, 0.07, 0.6, 0.35),
                       NocSurfaceConstants.SurfPorGranite, 1, true, 0.3, 0, 2)
                       .F("_LeakDecal", 1).F("_MicroGain", 1).F("_MicroSlice", 1).With(x => Photo(x, "Granite", NocPal.Granite)),
                SurfV2(Surface("SteelPainted", NocPal.ConcreteDark, MetalDark, NocTexFamily.Metal, 2.0, 0, 0.3, 0.6, 0.8, 7, 0.12, 0.07, 0.4, 0.4, 1.0),
                       0, 1, true, 0.3, 1, 3)
                       .F("_LeakDecal", 1).F("_MicroGain", 1).F("_MicroSlice", 2).With(x => Photo(x, "MetalPaint", NocPal.MetalPaint)),
                SurfV2(Surface("SteelBare", NocPal.SignWhite, SteelF0, NocTexFamily.Metal, 2.0, 1, 0.45, 0.75, 0.7, 7, 0.08, 0.06, 0.3, 0.3),
                       0, 1, false, 0, 0, 4)
                       .F("_MicroGain", 1).F("_MicroSlice", 2).With(x => Photo(x, "SteelPlates", NocPal.SteelF0)),
                SurfV2(Surface("Brass", NocPal.WinWarm, 1f, NocTexFamily.Metal, 2.0, 1, 0.5, 0.8, 0.6, 5, 0.1, 0.06, 0.25, 0.2)
                    .C2("_BaseColor", NocPal.WinWarm, BrassF0, NocPal.TitleText, BrassF0b), 0, 1, false, 0, 0, 5)
                    .With(x => Photo(x, "SteelPlates", NocPal.WinWarm)).C2("_BaseColor2", NocPal.WinWarm, BrassF0, NocPal.TitleText, BrassF0b),
                SurfV2(Surface("Moss", NocPal.Moss, 1f, NocTexFamily.Moss, 1.5, 0, 0.15, 0.45, 1.2, 6, 0.2, 0.07, 0.6, 0),
                       NocSurfaceConstants.SurfPorMoss, 1, false, 0, 1, 6).With(x => Photo(x, "Moss", NocPal.Moss)),
                SurfV2(Surface("Bark", NocPal.Bark, 1f, NocTexFamily.Bark, 1.0, 0, 0.1, 0.4, 1.4, 5, 0.15, 0.06, 0.5, 0.3),
                       NocSurfaceConstants.SurfPorBark, 1, false, 0, 0, 7).With(x => Photo(x, "Bark", NocPal.Bark)).F("_Mapping", 1),
                SurfV2(Surface("Asphalt", NocPal.WetAsphalt, 1f, NocTexFamily.Asphalt, 2.0, 0, 0.2, 0.5, 0.9, 10, 0.15, 0.08, 0.7, 0),
                       0, 1, false, 0, 1, 8)
                       .F("_MicroGain", 1).F("_MicroSlice", 3).With(x => Photo(x, "Asphalt", NocPal.WetAsphalt)),
                // ---- Look v2 rows (SU, TEXTURE_PLAN 1.3 / 3.2): used only on -surf-v 2 (the beats fall back to Concrete / StoneWall), so
                //      their v1 maps are the generated families of the nearest class
                SurfV2(Surface("ConcreteAlley", NocPal.ConcreteDark, 1f, NocTexFamily.Concrete, 3.0, 0, 0.15, 0.5, 1.0, 9, 0.18, 0.08, 0.5, 0.5),
                       NocSurfaceConstants.SurfPorConcrete, 1, true, 0.6, 0, 9)
                       .F("_LeakDecal", 1).F("_MicroGain", 1).F("_MicroSlice", 0).With(x => Photo(x, "ConcreteAlley", NocPal.ConcreteDark)),
                SurfV2(Surface("Brick", NocPal.Brick, 1f, NocTexFamily.Concrete, 3.0, 0, 0.1, 0.35, 1.0, 9, 0.15, 0.06, 0.5, 0.3),
                       NocSurfaceConstants.SurfPorBrick, 1, true, 0.5, 0, 10).F("_LeakDecal", 1).With(x => Photo(x, "Brick", NocPal.Brick, 1f, true)),
                SurfV2(Surface("Plaster", NocPal.Plaster, 1f, NocTexFamily.Concrete, 3.0, 0, 0.15, 0.4, 1.0, 9, 0.15, 0.06, 0.5, 0.3),
                       NocSurfaceConstants.SurfPorPlaster, 1, true, 0.5, 1, 11)
                       .F("_LeakDecal", 1).F("_MicroGain", 1).F("_MicroSlice", 0).With(x => Photo(x, "Plaster", NocPal.Plaster)),
                // ---- Look v2 small sets (TEXTURE_PLAN 1.3 / 3.5; sheltered: wet level 0.15): the noodle-bar counter (fine-grained wood,
                //      grain along the long axis, offsets only), its back wall (long white tiles, module shuffle) and fabric (noren, the goal
                //      curtain: emission x the weave)
                SurfV2(Surface("Wood", NocPal.Bark, 1.3f, NocTexFamily.Bark, 1.0, 0, 0.3, 0.5, 1.0, 5, 0.12, 0.05, 0.1, 0),
                       0, NocSurfaceConstants.SurfWetSheltered, false, 0, 0, 12).With(x => Photo(x, "Wood", NocPal.Bark, 1.3f)),
                SurfV2(Surface("TileWall", NocPal.Plaster, 1.6f, NocTexFamily.Concrete, 3.0, 0, 0.6, 0.85, 0.6, 9, 0.08, 0.04, 0.1, 0),
                       0, NocSurfaceConstants.SurfWetSheltered, false, 0, 0, 13).With(x => Photo(x, "TileWall", NocPal.Plaster, 1.6f)),
                SurfV2(Surface("Fabric", NocPal.Plaster, 1f, NocTexFamily.Concrete, 3.0, 0, 0.05, 0.25, 0.5, 9, 0.08, 0.04, 0.1, 0),
                       NocSurfaceConstants.SurfPorPlaster, NocSurfaceConstants.SurfWetSheltered, false, 0, 0, 14)
                       .F("_EmitByAlbedo", 1).With(x => Photo(x, "Fabric", NocPal.Plaster)),
                // ---- wet floors (NocWet, one per 14.4 surface)
                WetV2(Wet("FloorAlley", NocPal.WetAsphalt, 1f, NocTexFamily.Asphalt, 2.0, NocturneConstants.WetPuddleFraction, NocturneConstants.WetAlphaDry, 0.7),
                      NocTexFamily.Asphalt, NocturneConstants.WetPuddleFraction, 0.35, 0.55)
                      .With(x => PhotoWet(x, "AsphaltAlley", NocPal.WetAsphalt, 1f, NocturneConstants.WetPuddleFraction, WetUpGloss))
                      .F("_ReflDry", WetAlleyReflDry),
                WetV2(Wet("FloorStone", NocPal.ConcreteDark, WetStone, NocTexFamily.Paving, 2.4, NocturneConstants.WetPuddleFraction, NocturneConstants.WetAlphaDry, 0.6),
                      NocTexFamily.Paving, NocturneConstants.WetPuddleFraction, 0.45, 0.65)
                      .With(x => PhotoWet(x, "Pavers", NocPal.ConcreteDark, WetStone, NocturneConstants.WetPuddleFraction, WetUpGloss)),
                WetV2(Wet("FloorSteelDeck", NocPal.ConcreteDark, MetalDark, NocTexFamily.SteelDeck, 2.4, NocturneConstants.WetPuddleFraction, 0.3, 0.6),
                      NocTexFamily.SteelDeck, NocturneConstants.WetPuddleFraction, 0.55, 0.75, 2.4, 1.2)
                      .With(x => PhotoWet(x, "DeckPlate", NocPal.MetalPaint, 1f, NocturneConstants.WetPuddleFraction, WetUpGlossDeck, true)).F("_HexRotate", 0),
                WetV2(Wet("FloorGardenPath", NocPal.ConcreteDark, 0.8f, NocTexFamily.Paving, 1.8, 0.25, 0.4, 0.7),
                      NocTexFamily.Paving, 0.25, 0.4, 0.6)
                      .With(x => PhotoWet(x, "GardenTiles", NocPal.ConcreteDark, 0.8f, 0.25, 0.4)),
                WetV2(Wet("FloorPlaza", NocPal.ConcreteDark, 1f, NocTexFamily.PlazaStone, 4.8, NocturneConstants.WetPuddleFraction, NocturneConstants.WetAlphaDry, 0.5),
                      NocTexFamily.PlazaStone, NocturneConstants.WetPuddleFraction, 0.45, 0.65)
                      .With(x => PhotoWet(x, "Granite", NocPal.Granite, 1f, NocturneConstants.WetPuddleFraction, WetUpGloss)),
                // ---- Look v2 (SU, TEXTURE_PLAN 1.3 / 3.5): opaque dark panes (the Old Quarter windows; untextured, smoothness 0.5: at
                //      0.86-0.92 the panes beside the goal reflected the goal light as warm points, r01 KS17)
                Make("GlassDark", NocPaths.ShaderGraybox).C("_BaseColor", NocPal.GlassDark).C("_EmissionColor", NocPal.Floor, 0f)
                    .F("_Smoothness", 0.5).F("_GridStrength", 0).F("_AO", 0.5).F("_Cull", (float)CullMode.Back).Inst(),
                // ---- Look v2 (SU, TEXTURE_PLAN 3.5): open steel grating (NocGrating, analytic 62.5 mm mesh, A2C, two-sided)
                Make("Grating", ShaderGrating).C("_BaseColor", NocPal.MetalPaint).F("_Smoothness", 0.5).F("_MacroAlbedo", 0.15)
                    .F("_BarPitch", 0.0625).F("_BarW0", 0.010).F("_BarW1", 0.014).F("_BarDepth", 0.03).F("_AO", 0.5)
                    .F("_Cull", (float)CullMode.Off).Q(2450).Inst(),
                // ---- Look v2 (SU, TEXTURE_PLAN 3.4): the garden moss ground as a library row (FO's GardenMaterials picks it up on
                //      -foliage-v 2): Moss token on NocWet, no puddles (the moss soaks; CRIT0 #10 "no puddle blots on the lawn", FO's
                //      REQUEST SU carried by FX: fraction 0, so the thresholds sit at the top of the field), a rough dry lobe
                WetV2(Wet("FloorMoss", NocPal.Moss, 1f, NocTexFamily.Moss, 1.5, 0.0, 0.55, 1.0), NocTexFamily.Moss, 0.0, 0.25, 0.40)
                      .F("_ReflDry", 0.35).With(x => PhotoWet(x, "Moss", NocPal.Moss, 1f, 0.0, 0.25)),
                // ---- Look v2 (SU-4, TEXTURE_PLAN 3.5; CRIT0 #24): the lit-room shop glass; G1Materials derives one per emissive slot
                Make("Shopfront", ShaderShopfront).C("_BaseColor", NocPal.GlassDark).C("_EmissionColor", NocPal.WinWarm, G1MaterialsShopWarm)
                    .F("_RoomGain", 0.6).F("_RoomDepth", 4).F("_ShelfPitch", 0.45).F("_ItemWidth", 0.12).F("_Seed", 0).F("_Cull", (float)CullMode.Back).Inst(),
                // ---- glass (NocGlass: two materials so every back face draws before every front face, R3 5.4)
                Make("GlassBack", ShaderGlass).C("_BaseColor", NocPal.ConcreteDark).F("_Smoothness", 0.95).F("_AlphaBase", 0.12).F("_AlphaFresnel", 0.6)
                    .F("_Cull", (float)CullMode.Front).Q(3000).With(x => GlassV2(x)),
                Make("GlassFront", ShaderGlass).C("_BaseColor", NocPal.ConcreteDark).F("_Smoothness", 0.95).F("_AlphaBase", 0.12).F("_AlphaFresnel", 0.6)
                    .F("_Cull", (float)CullMode.Back).Q(3001).With(x => GlassV2(x)),
                // ---- lines (NocLine tube, A2C)
                Make("Line", NocPaths.ShaderLine).Kw("_LINE_TUBE").C("_BaseColor", NocPal.ConcreteDark).C("_EmissionColor", NocPal.Floor, 0f)
                    .F("_Smoothness", 0.4).F("_LineWidth", 0.03).F("_MinWidthPx", 0).F("_AlphaFloor", NocturneConstants.LineEnergyFloor)
                    .F("_SrcBlend", (float)BlendMode.One).F("_DstBlend", (float)BlendMode.Zero).F("_ZWrite", 1).F("_Cull", (float)CullMode.Off).F("_AlphaToMask", 1),
            };
        }
    }
}
