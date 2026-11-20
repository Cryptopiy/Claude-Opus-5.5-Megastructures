// MaterialTests.Foliage: the per-row expectations of the foliage rows (owner FO; TEXTURE_PLAN 8.3). Created empty by TP-0 and
// handed over to FO.
//   - LeafCore: NocFoliage _LeafMode 1, opaque (no alpha to coverage), queue 2000, no emission;
//   - Leaves: NocFoliage _LeafMode 2, AlphaToMask on, Cull Off, queue 2450, the LOOK_V2 10 gains, the three LeafCrown maps from
//     one prefix; when the atlas exists: a Texture2DArray of 4 square slices whose albedo import preserves alpha coverage at 0.5.
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    public partial class MaterialTests
    {
        static MatSpec FoliageRow(string field)
        {
            foreach (var s in MaterialFactory.Specs()) if (s.Field == field) return s;
            Assert.Fail("no material row " + field);
            return null;
        }

        [Test]
        public void Foliage_CoreAndCardRows()
        {
            var core = FoliageRow("LeafCore");
            var cards = FoliageRow("Leaves");
            Assert.AreEqual(GardenMaterials.FoliageShader, core.Shader);
            Assert.AreEqual(GardenMaterials.FoliageShader, cards.Shader);
            Assert.AreEqual(GardenMaterials.ModeCore, core.Floats["_LeafMode"]);
            Assert.AreEqual(GardenMaterials.ModeCards, cards.Floats["_LeafMode"]);
            Assert.AreEqual(0f, core.Floats["_AlphaToMask"], "the core is opaque");
            Assert.AreEqual(1f, cards.Floats["_AlphaToMask"], "alpha to coverage on the cards");
            Assert.AreEqual(0f, cards.Floats["_Cull"], "cards two-sided");
            Assert.AreEqual(GardenMaterials.CoreQueue, core.Queue);
            Assert.AreEqual(GardenMaterials.CardQueue, cards.Queue, "cards at AlphaTest 2450");
            Assert.AreEqual(0f, core.Floats["_GlowGain"] + core.Floats["_RimGain"] + core.Floats["_TipGain"], "no emission on the core");
            Assert.AreEqual(0.012f, cards.Floats["_GlowGain"], 1e-6f, "LOOK_V2 10 glow gain");
            Assert.AreEqual(0.015f, cards.Floats["_RimGain"], 1e-6f, "LOOK_V2 10 rim gain");
            Assert.AreEqual(0.03f, cards.Floats["_TipGain"], 1e-6f, "LOOK_V2 10 tip gain");
            Assert.IsFalse(core.Floats.ContainsKey("_Metallic") && core.Floats["_Metallic"] != 0f);
        }

        [Test]
        public void Foliage_LeafAtlas_IsACoveragePreservingArray()
        {
            string prefix = MaterialFactory.LeafAtlasPrefix();
            var cards = FoliageRow("Leaves");
            if (prefix == null)
            {
                Assert.IsFalse(cards.Textures.ContainsKey("_AlbedoTex"), "no atlas yet: the row binds none");
                Assert.Ignore("LeafCrown atlas not processed yet (TP-1)");
            }
            Assert.AreEqual(prefix + "_Albedo.png", cards.Textures["_AlbedoTex"]);
            Assert.AreEqual(prefix + "_Normal.png", cards.Textures["_NormalTex"]);
            Assert.AreEqual(prefix + "_Mask.png", cards.Textures["_MaskTex"]);
            foreach (var kv in cards.Textures)
            {
                var arr = AssetDatabase.LoadAssetAtPath<Texture2DArray>(kv.Value);
                Assert.IsNotNull(arr, kv.Value + " imports as a Texture2DArray");
                Assert.AreEqual(GardenLeafCards.Sprites, arr.depth, kv.Value + " holds the 4 cluster sprites");
                Assert.AreEqual(arr.width, arr.height, kv.Value + " square slices");
                Assert.Greater(arr.mipmapCount, 8, kv.Value + " mips");
            }
            var imp = AssetImporter.GetAtPath(prefix + "_Albedo.png") as TextureImporter;
            Assert.IsNotNull(imp);
            Assert.IsTrue(imp.mipMapsPreserveCoverage, "coverage-preserving mips on the leaf albedo (TEXTURE_PLAN 4)");
            Assert.AreEqual(0.5f, imp.alphaTestReferenceValue, 1e-6f);
            Assert.IsTrue(imp.sRGBTexture, "albedo sRGB (alpha stays linear)");
        }
    }
}
