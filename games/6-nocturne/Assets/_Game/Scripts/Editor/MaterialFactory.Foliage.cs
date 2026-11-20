// MaterialFactory.Foliage: the foliage rows of the material contract (owner FO; TEXTURE_PLAN 8.3: Leaves, LeafCore). Created
// empty by TP-0 and handed over to FO; every row needs its MaterialLibrary field (MaterialLibrary.Foliage.cs).
// Look v2 crowns (TEXTURE_PLAN 4, LOOK_V2 5.1 and 10), used by GardenMaterials with -foliage-v 2:
//   LeafCore  NocFoliage _LeafMode 1: the opaque core (Leaf token x the atlas mean, x _CoreAo), queue 2000.
//   Leaves    NocFoliage _LeafMode 2: the leaf-cluster cards, AlphaToMask on, Cull Off, queue 2450 (AlphaTest), the LOOK_V2 10
//             emission gains (glow 0.012, rim 0.015, tips 0.03), the LeafCrown atlas (Texture2DArray of 4 cluster sprites,
//             texprep step 7 with Tools/texprep/foliage_compose.py and foliage.json).
// The atlas is wired from TP-1's processed set (Textures/External, found by its texmeta); without it the row binds no texture
// (it builds without a missing-texture problem; the v2 cards are then not usable, and v1 never uses these rows).
using System.Collections.Generic;

namespace Megastructure.EditorTools
{
    public static partial class MaterialFactory
    {
        /// <summary>texprep key of the crown atlas (foliage.json).</summary>
        public const string LeafAtlasKey = "LeafCrown";

        /// <summary>Occlusion of the crown interior seen between the cards (FO lookdev r02: at 0.55 with the value-noise core
        /// the core matched the card leaves, 13-16 codes both, so the leaves on a lobe facing the camera vanished into a smooth
        /// dome; 0.40 with the atlas core and its darker gaps (x 0.45) puts the deep interior at about 0.26 x Leaf, about a third
        /// of the outer leaves, as in a real crown).</summary>
        public const float CoreAo = 0.40f, CoreGap = 0.25f;

        /// <summary>Foliage rows (FO).</summary>
        static List<MatSpec> FoliageRows()
        {
            var core = Make("LeafCore", GardenMaterials.FoliageShader)
                .F("_LeafMode", GardenMaterials.ModeCore).F("_AlphaToMask", 0).F("_Cull", 2)
                .F("_AlbedoGain", 1.0).F("_Smoothness", 0.35).F("_CoreAo", CoreAo).F("_CoreGap", CoreGap)
                .F("_GlowGain", 0).F("_RimGain", 0).F("_TipGain", 0)
                .Q(GardenMaterials.CoreQueue);
            var cards = Make("Leaves", GardenMaterials.FoliageShader)
                .F("_LeafMode", GardenMaterials.ModeCards).F("_AlphaToMask", 1).F("_Cull", 0)
                .F("_AlbedoGain", 1.0)
                .F("_GlowGain", GardenMaterials.CardGlowGain).F("_RimGain", GardenMaterials.CardRimGain).F("_TipGain", GardenMaterials.CardTipGain)
                .Q(GardenMaterials.CardQueue);
            string prefix = LeafAtlasPrefix();
            if (prefix != null)
            {
                cards.Tx("_AlbedoTex", prefix + "_Albedo.png").Tx("_NormalTex", prefix + "_Normal.png").Tx("_MaskTex", prefix + "_Mask.png");
                core.Tx("_AlbedoTex", prefix + "_Albedo.png");             // the core's two planar leaf layers
            }
            return new List<MatSpec> { core, cards };
        }

        /// <summary>Asset path prefix of the crown atlas maps ("dir/LeafCrown", TP-1's processed set), null before texprep ran.</summary>
        public static string LeafAtlasPrefix()
        {
            string meta = NocTextureImport.ManifestOf(LeafAtlasKey);
            return meta != null ? meta.Substring(0, meta.Length - TexMeta.Suffix.Length) : null;
        }
    }
}
