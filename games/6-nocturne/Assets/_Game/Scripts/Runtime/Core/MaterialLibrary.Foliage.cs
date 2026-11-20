// MaterialLibrary.Foliage: the foliage fields of the material library (owner FO; TEXTURE_PLAN 8.3: Leaves, LeafCore). Created
// empty by TP-0 and handed over to FO; every field needs its row in MaterialFactory.Foliage.cs.
// Both are NocFoliage materials of Look v2 (TEXTURE_PLAN 4); GardenMaterials uses them with -foliage-v 2 (v1 keeps its graybox
// crown material, so the default picture does not depend on these fields).
using UnityEngine;

namespace Megastructure
{
    public partial class MaterialLibrary
    {
        [Header("Foliage (NocFoliage, Look v2; FO)")]
        public Material LeafCore;         // _LeafMode 1: the opaque crown core at 0.90 of the radii (Leaf token x interior AO), queue 2000
        public Material Leaves;           // _LeafMode 2: leaf-cluster cards, LeafCrown atlas (Texture2DArray), AlphaToMask, Cull Off, queue 2450
    }
}
