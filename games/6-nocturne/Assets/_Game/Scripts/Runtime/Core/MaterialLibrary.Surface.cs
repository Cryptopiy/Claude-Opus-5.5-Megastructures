// MaterialLibrary.Surface: the near-surface fields of the material library (owner SU; TEXTURE_PLAN 8.3): the graybox set, the
// NocSurface PBR surfaces, the NocWet floors, the NocGlass faces and the NocLine row. Created by TP-0 with the fields moved
// VERBATIM from MaterialLibrary.cs (serialised names unchanged, so MaterialLibrary.asset keeps every reference) and handed over
// to SU. FloorFor, All and FieldNames stay in MaterialLibrary.cs (TP).
using UnityEngine;

namespace Megastructure
{
    public partial class MaterialLibrary
    {
        [Header("Graybox (NocGraybox, lit template)")]
        public Material GrayFloor;        // graybox floor slabs: NocWet wet stone (the analytic mirror on every graybox beat)
        public Material GrayWall;         // ConcreteDark walls and blocks
        public Material GrayMarker;       // beat markers (NeonCyan emission)
        public Material GrayMarkerWarm;   // goal / warm markers (SodiumAmber emission)

        [Header("PBR surfaces (NocSurface, docs/MATERIALS.md)")]
        public Material Concrete;         // ConcreteDark cast concrete: walls, podia, plaza furniture
        public Material StoneWall;        // WetStone (ConcreteDark x0.9) granite cladding
        public Material SteelPainted;     // MetalDark painted steel (dielectric paint, edge wear to bare steel)
        public Material SteelBare;        // bare steel (metal, F0 SignWhite x0.55)
        public Material Brass;            // brass fittings (metal, F0 WinWarm x0.4 + TitleText x0.45 = (0.80, 0.61, 0.42))
        public Material Moss;             // Moss token, garden ground cover
        public Material Bark;             // Bark token, trunks
        public Material Asphalt;          // WetAsphalt kerbs and patches (non-floor)

        [Header("Wet floors (NocWet, 14.7 H4; one per 14.4 surface)")]
        public Material FloorAlley;       // s 0-120 wet concrete street (WetAsphalt token, asphalt maps)
        public Material FloorStone;       // s 120-560 and 1080-1260 wet stone pavers (WetStone)
        public Material FloorSteelDeck;   // s 560-880 steel deck (MetalDark)
        public Material FloorGardenPath;  // s 880-1080 stone path (fewer puddles)
        public Material FloorPlaza;       // s 1260-1500 plaza granite

        [Header("Look v2 near surfaces (SU, TEXTURE_PLAN 1.3)")]
        public Material GlassDark;        // opaque dark panes (NocGraybox, GlassDark token, smoothness 0.5): Old Quarter windows
        public Material Grating;          // open steel grating plates (NocGrating, MetalPaint, A2C, two-sided): catwalk, fire escapes
        public Material ConcreteAlley;    // alley walls (NocSurface v2: concrete_wall_006, anchored leaks x0.6, no rotation)
        public Material Brick;            // Old Quarter tenement (NocSurface v2: brick_wall_08, whole-course shuffle, porosity 0.5)
        public Material Plaster;          // Old Quarter neighbours (NocSurface v2: plastered_wall_04, porosity 0.3)
        public Material Shopfront;        // lit rooms behind shop glass (NocShopfront, SU-4; G1Materials derives one per slot)
        public Material Wood;             // noodle-bar counter, doors (NocSurface v2: fine_grained_wood, sheltered)
        public Material TileWall;         // noodle-bar back wall (NocSurface v2: long_white_tiles, module shuffle)
        public Material Fabric;           // noren and the goal curtain (NocSurface v2: poly_wool_herringbone, emission x the weave)
        public Material FloorMoss;        // garden moss ground (NocWet: Moss001, no puddles; FO's GardenMaterials on -foliage-v 2)

        [Header("Glass (NocGlass: back faces 3000, front faces 3001)")]
        public Material GlassBack;
        public Material GlassFront;

        [Header("Lines")]
        public Material Line;             // NocLine (cables, rails, rods)
    }
}
