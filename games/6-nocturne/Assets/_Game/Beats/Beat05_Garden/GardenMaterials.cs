// GardenMaterials: the materials of beat 5. Walk surfaces: the flagstone path is lib.FloorFor(s) of 14.4 (FloorGardenPath, NocWet),
// the promenade the beat-6 surface (FloorStone), the moss ground a NocWet variant of the garden floor with the Moss token, the Moss
// maps and fewer, rougher puddles (14.5.5 "NocWet moss variant"); concrete for the slab and keel, wet stone for the balustrade,
// planters and benches, a Moss NocSurface for the hedges, Bark for trunks and branches, and NocFoliage (Shaders/Beats, this
// folder's shader) for the crowns, falling back to the Moss surface when the shader is missing. Derived once at build.
// Look v2 (-foliage-v 2, TEXTURE_PLAN 4, FO): the crowns become two NocFoliage materials, LeafCore (_LeafMode 1: the opaque core,
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
        /// <summary>NocFoliage _LeafMode values: the v1 glowing crown, the v2 core, the v2 cards.</summary>
        public const float ModeCrownV1 = 0f, ModeCore = 1f, ModeCards = 2f;
        public const int CoreQueue = 2000, CardQueue = 2450;
        /// <summary>LOOK_V2 10 emission gains of the cards (v1: 0.042, 0.06, 0.08): LeafGlow specks, LeafGlow rim, LeafWarm tips.</summary>
        public const float CardGlowGain = 0.012f, CardRimGain = 0.015f, CardTipGain = 0.03f;

        public readonly Material Path, Promenade, Moss, Concrete, Stone, Hedge, Bark, Foliage;
        /// <summary>v2 only (null in v1): the opaque core and the leaf-cluster cards.</summary>
        public readonly Material LeafCore, Leaves;

        public GardenMaterials(MaterialLibrary lib)
        {
            bool v2 = GardenDims.FoliageV2;
            if (v2 && lib != null && lib.LeafCore != null && lib.Leaves != null)
            {
                LeafCore = lib.LeafCore;
                Leaves = lib.Leaves;
            }
            Foliage = BridgeGardenKit.FromShader(FoliageShader, "Garden_Foliage");
            if (v2 && Leaves == null && Foliage != null)
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
            if (lib == null) return;
            Path = lib.FloorFor(0.5 * (NocturneConstants.BeatBoundaries[4] + NocturneConstants.BeatBoundaries[5]));
            Promenade = lib.FloorFor(0.5 * (NocturneConstants.BeatBoundaries[5] + NocturneConstants.BeatBoundaries[6]));
            // v2: SU's library row FloorMoss (TEXTURE_PLAN 3.4: Moss001, NocWet, every new property) once it exists; looked up by
            // name so this file does not depend on SU's field before it lands
            var floorMoss = v2 ? typeof(MaterialLibrary).GetField("FloorMoss")?.GetValue(lib) as Material : null;
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
                Moss.SetFloat("_PuddleFraction", v2 ? 0f : 0.10f);
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
    }
}
