// GardenMaterials: the materials of beat 5. Walk surfaces: the flagstone path is lib.FloorFor(s) of 14.4 (FloorGardenPath, NocWet),
// the promenade the beat-6 surface (FloorStone), the moss ground a NocWet variant of the garden floor with the Moss token, the Moss
// maps and fewer, rougher puddles (14.5.5 "NocWet moss variant"); concrete for the slab and keel, wet stone for the balustrade,
// planters and benches, a Moss NocSurface for the hedges, Bark for trunks and branches, and NocFoliage (Shaders/Beats, this
// folder's shader) for the crowns, falling back to the Moss surface when the shader is missing. Derived once at build.
// Look v2 (TEXTURE_PLAN 4, FO; v1 deleted by GD in Phase 4): the crowns are two NocFoliage materials, LeafCore (_LeafMode 1: the opaque core,
// shaded with the leaf atlas mean, queue 2000) and Leaves (_LeafMode 2: the leaf-cluster cards, AlphaToMask, queue 2450, the
// LeafCrown atlas); the moss ground has no puddle mask (CRIT0 #10: no puddle blots on the lawn; standing water stays on the path).
// Both come from the library (lib.LeafCore, lib.Leaves: MaterialFactory.Foliage rows with the LeafCrown atlas; no runtime
// Shader.Find in v2); a library without them (tests on an old asset) falls back to materials derived from the graybox one.
using UnityEngine;

namespace Megastructure
{
    public sealed class GardenMaterials
    {
        public const string FoliageShader = "Megastructure/NocFoliage";
        /// <summary>NocFoliage _LeafMode values: the core, the cards (lanterns 3 and fireflies 4 below).</summary>
        public const float ModeCore = 1f, ModeCards = 2f;
        public const int CoreQueue = 2000, CardQueue = 2450;
        /// <summary>LOOK_V2 10 emission gains of the cards: LeafGlow specks, LeafGlow rim, LeafWarm tips.</summary>
        public const float CardGlowGain = 0.012f, CardRimGain = 0.015f, CardTipGain = 0.03f;

        public readonly Material Path, Promenade, Moss, Concrete, Stone, Hedge, Bark, Foliage;
        /// <summary>GD: NocFoliage _LeafMode 3 (lanterns) and 4 (fireflies), opaque, queue 2000; the posts' painted steel.</summary>
        public readonly Material Lanterns, Fireflies, PostSteel, Wood;
        public const float ModeLanterns = 3f, ModeFireflies = 4f;
        /// <summary>The opaque core and the leaf-cluster cards.</summary>
        public readonly Material LeafCore, Leaves;
        /// <summary>GD fix wave (C33): the limbs, NocFoliage _LeafMode 5 (lib.Bark's CC0 photo maps + the lantern field); null without
        /// the library's Bark photo maps (the limbs then stay in the trunk mesh with lib.Bark).</summary>
        public readonly Material Limbs;
        public const float ModeBark = 5f;

        public GardenMaterials(MaterialLibrary lib)
        {
            if (lib != null && lib.LeafCore != null && lib.Leaves != null)
            {
                // GD: runtime copies with the Phase 4 properties written explicitly (the library .mat files predate them; GD r02
                // showed a property missing from a built material does not reliably take the shader default)
                LeafCore = BridgeGardenKit.Derive(lib.LeafCore, "Garden_LeafCore");
                Leaves = BridgeGardenKit.Derive(lib.Leaves, "Garden_Leaves");
            }
            Foliage = BridgeGardenKit.FromShader(FoliageShader, "Garden_Foliage");
            if (Leaves == null && Foliage != null)
            {
                LeafCore = BridgeGardenKit.Derive(Foliage, "Garden_LeafCore");
                LeafCore.SetFloat("_LeafMode", ModeCore);
                LeafCore.SetFloat("_AlphaToMask", 0f);
                LeafCore.SetFloat("_AlbedoGain", 1f);            // x Leaf (the atlas mean)
                LeafCore.SetFloat("_Smoothness", 0.35f);
                LeafCore.renderQueue = CoreQueue;
                Leaves = BridgeGardenKit.Derive(Foliage, "Garden_Leaves");
                Leaves.SetFloat("_LeafMode", ModeCards);
                Leaves.SetFloat("_AlphaToMask", 1f);
                Leaves.SetFloat("_Cull", 0f);
                Leaves.SetFloat("_AlbedoGain", 1f);
                Leaves.SetFloat("_GlowGain", CardGlowGain);
                Leaves.SetFloat("_RimGain", CardRimGain);
                Leaves.SetFloat("_TipGain", CardTipGain);
                Leaves.renderQueue = CardQueue;
            }
            Lanterns = Emissive(Foliage, "Garden_Lanterns", ModeLanterns);
            Fireflies = Emissive(Foliage, "Garden_Fireflies", ModeFireflies);
            foreach (var mat in new[] { LeafCore, Leaves, Lanterns, Fireflies }) SetPhase4(mat);
            if (lib == null) return;
            Limbs = BarkLimbs(lib.Bark, Leaves != null ? Leaves : Foliage);
            PostSteel = lib.SteelPainted != null ? lib.SteelPainted : lib.Concrete;
            Wood = lib.Wood;
            Path = lib.FloorFor(0.5 * (NocturneConstants.BeatBoundaries[4] + NocturneConstants.BeatBoundaries[5]));
            Promenade = lib.FloorFor(0.5 * (NocturneConstants.BeatBoundaries[5] + NocturneConstants.BeatBoundaries[6]));
            // SU's library row FloorMoss (TEXTURE_PLAN 3.4: Moss001, NocWet, every new property)
            var floorMoss = lib.FloorMoss;
            if (floorMoss != null && floorMoss.HasProperty("_PuddleFraction") && floorMoss.GetFloat("_PuddleFraction") > 0f)
            {
                // CRIT0 #10: no puddle blots on the lawn (standing water stays on the path): a copy of SU's row with every one of its
                // properties, puddles off
                floorMoss = BridgeGardenKit.Derive(floorMoss, "Garden_MossGround");
                floorMoss.SetFloat("_PuddleFraction", 0f);
            }
            Moss = floorMoss != null ? floorMoss : BridgeGardenKit.Derive(lib.FloorGardenPath, "Garden_MossGround");
            if (Moss != null && floorMoss == null)
            {
                BridgeGardenKit.SetLinear(Moss, "_BaseColor", BridgeGardenKit.Lin(NocPal.Moss));
                if (lib.Moss != null)
                {
                    foreach (var tex in new[] { "_AlbedoTex", "_NormalTex", "_MaskTex" })
                        if (lib.Moss.HasProperty(tex) && Moss.HasProperty(tex)) Moss.SetTexture(tex, lib.Moss.GetTexture(tex));
                    if (lib.Moss.HasProperty("_Tile")) Moss.SetFloat("_Tile", lib.Moss.GetFloat("_Tile"));
                }
                Moss.SetFloat("_PuddleFraction", 0f);
                Moss.SetFloat("_AlphaDry", 0.55f);
                Moss.SetFloat("_ReflDry", 0.35f);
                Moss.SetFloat("_NormalStrength", 1.0f);
            }
            Concrete = lib.Concrete;
            Stone = lib.StoneWall;
            Hedge = lib.Moss;
            Bark = lib.Bark;
            if (Foliage == null) Foliage = lib.Moss;
        }

        /// <summary>GD (Phase 4) NocFoliage values, written on every garden foliage material (see NocFoliage.shader's header).</summary>
        public const float SwayAmp = 0.25f, SwayPeriod = 7f, LanternGain = 2.0f, LanternPulse = 0.06f, LanternHz = 0.1f, MinPx = 2.2f;
        public const float FieldLightGain = 1.6f, ScatterPow = 2.5f, UnderGlow = 0.035f, FireflyGain = 8f, FireflyHz = 0.12f;
        /// <summary>GD fix wave: lanterns drawn under FlatPx px are shaded flat (C32); the under-glow hue lerp(LeafGlow, LeafWarm, UnderWarm)
        /// and the field E at which the scattered light keeps the lantern's warmth (C33: canopy hue toward Leaf 93, warm beside strands).</summary>
        public const float FlatPx = 3f, UnderWarm = 0.65f, NearWarm = 1.6f, BarkSmoothness = 0.35f;

        public static void SetPhase4(Material m)
        {
            if (m == null) return;
            m.SetFloat("_SwayAmp", SwayAmp);
            m.SetFloat("_SwayPeriod", SwayPeriod);
            m.SetFloat("_LanternGain", LanternGain);
            m.SetFloat("_LanternPulse", LanternPulse);
            m.SetFloat("_LanternHz", LanternHz);
            m.SetFloat("_MinPx", MinPx);
            m.SetFloat("_LanternLightGain", FieldLightGain);
            m.SetFloat("_ScatterPow", ScatterPow);
            m.SetFloat("_FieldScale", GardenLanterns.FieldScale);
            m.SetFloat("_UnderGlow", UnderGlow);
            m.SetFloat("_FireflyGain", FireflyGain);
            m.SetFloat("_FireflyHz", FireflyHz);
            m.SetFloat("_FlatPx", FlatPx);
            m.SetFloat("_UnderWarm", UnderWarm);
            m.SetFloat("_NearWarm", NearWarm);
        }

        /// <summary>The limbs' material (mode 5): a NocFoliage copy (of the cards: the atlas sampler, Repeat) with lib.Bark's photo maps,
        /// its token and the 1.8 m bark tile of GardenTree's UV0; null when the bark row has no photo maps.</summary>
        static Material BarkLimbs(Material bark, Material foliage)
        {
            if (bark == null || foliage == null || !bark.HasProperty("_AlbedoTex2") || !bark.HasProperty("_NormalTex2")) return null;
            Texture alb = bark.GetTexture("_AlbedoTex2"), nrm = bark.GetTexture("_NormalTex2");
            if (alb == null || nrm == null || !foliage.HasProperty("_BarkTex")) return null;
            var m = BridgeGardenKit.Derive(foliage, "Garden_Limbs");
            m.SetFloat("_LeafMode", ModeBark);
            m.SetFloat("_AlphaToMask", 0f);
            m.SetFloat("_Cull", 2f);
            m.SetFloat("_Smoothness", BarkSmoothness);
            m.SetTexture("_BarkTex", alb);
            m.SetTexture("_BarkNormalTex", nrm);
            if (bark.HasProperty("_BaseColor2")) m.SetColor("_BarkColor", bark.GetColor("_BaseColor2"));
            else BridgeGardenKit.SetLinear(m, "_BarkColor", BridgeGardenKit.Lin(NocPal.Bark));
            m.SetFloat("_BarkTile", GardenDims.BarkTile);
            m.SetFloat("_BarkNormalStrength", bark.HasProperty("_NormalStrength2") ? bark.GetFloat("_NormalStrength2") : 1f);
            SetPhase4(m);
            m.renderQueue = CoreQueue;
            return m;
        }

        static Material Emissive(Material foliage, string name, float mode)
        {
            var m = BridgeGardenKit.Derive(foliage, name);
            if (m == null) return null;
            m.SetFloat("_LeafMode", mode);
            m.SetFloat("_AlphaToMask", 0f);
            m.SetFloat("_Cull", 2f);
            m.renderQueue = CoreQueue;
            return m;
        }
    }
}
