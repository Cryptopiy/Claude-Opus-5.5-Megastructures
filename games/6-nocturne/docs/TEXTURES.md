# TEXTURES - CC0 photo texture sets for Look v2 (task TEX)

Date: 2026-11-14. Authority: user directive D-027 ("Look v2"). The directive replaces MASTER_PROMPT Part 6 item 1 and Part 5 rule 5 for textures only. This file is the provenance record for every downloaded image set.

**Status: reviewed (TEX-R).** The art-director review at the end of this file ("Review (TEX-R)") rejected 9 of TEX's 28 sets, demoted 3 to reference only and added 13. **Its "Final sets" table is the list to process.** The sections between here and the review are TEX's original record and stay as provenance for the kept sets.

Contact sheet: `docs/img/textures_v2_contact.jpg`. It has one tile per **final** set (29); the 13 sets added by the review have amber labels. Each tile shows the albedo on the left; decals are composited over dark grey. On the right is albedo x NormalGL Lambert x AO, lit from the upper left. For the two G overlays the left image is the mask instead, because their albedo is flat. The last row also shows the 9 rejected albedos, crossed out, with the reason.

## Licence and sources

| Source | Licence | Licence URL | Asset page | API used |
|---|---|---|---|---|
| ambientCG (aCG) | CC0 1.0 Universal (public domain, no attribution required) | https://docs.ambientcg.com/license/ ; https://creativecommons.org/publicdomain/zero/1.0/ | `https://ambientcg.com/a/<AssetId>` | `https://ambientcg.com/api/v2/full_json?q=...&include=downloadData,previewData,tagData,dimensionsData` |
| Poly Haven (PH) | CC0 1.0 Universal | https://polyhaven.com/license ; https://creativecommons.org/publicdomain/zero/1.0/ | `https://polyhaven.com/a/<id>` | `https://api.polyhaven.com/assets?t=textures`, `/files/<id>` (2k jpg per map, `nor_gl` normals), `/info/<id>` (dimensions in mm) |

CC0 requires no credit. As a courtesy, the Poly Haven authors are listed per set below.

No other library was needed: every class (A-I) was covered by these two.

Only image files were downloaded. Nothing was executed.

## Storage and conventions

- **Raw sets.** Raw sets live in `ExternalTextures/raw/<source>_<AssetId>/`, one new directory per set. This folder is git-ignored (`/ExternalTextures/` in `.gitignore`). Processed maps belong in `Assets/_Game/Textures/External` (D-027), which is the job of the TP texture pipeline, not of this task.
- **Download size.** 28 sets, 2K JPG, 406 MB on disk; the budget was 1.5 GB. After the review: 41 set directories, 592 MB (TEX's 28, including the rejected ones, which stay on disk unprocessed, plus the 13 added sets at 186 MB).
- **ambientCG extraction.** The ambientCG zips were extracted with Python `zipfile`:
  - a path-traversal guard was applied;
  - only `.jpg` and `.png` were kept (the `.mtlx` and `.usda` files were dropped);
  - the zip was deleted afterwards.
- **Image checks.** Every file was opened with PIL `verify()`. All 28 sets are real images.
- **ambientCG names and normals.** Files are named `<Id>_2K-JPG_<Map>.jpg`. Each set also has a 512 px `<Id>.png` preview. Both `NormalGL` and `NormalDX` are present; **use NormalGL** (Unity, Y+).
- **Poly Haven names and normals.** Files are named `<id>_<map>_2k.jpg`, with maps `diff`/`diffuse`, `nor_gl`, `rough`, `ao`, `disp`/`displacement` and `metal`. Normals are OpenGL. Poly Haven also offers `arm` (packed AO/rough/metal) and `nor_dx`; these are redundant and were not downloaded.
- **Colour spaces.** Albedo (Color/diff) is sRGB. Every other map is linear data. Roughness is roughness, not smoothness: smoothness = 1 - r.
- **Facade "Metalness" is a glass mask.** In the ambientCG facades, white = glass. ambientCG renders glass as a mirror. In Nocturne the map is the **glass mask**: glass is a dielectric (F0 0.04) with the analytic sky/facade reflection (Part 8.11 item 1). **Never use it as metallic = 1.**
- **Dominant colour.** This is the mean **linear** RGB of the full 2K albedo, sRGB-decoded in Python. Y = 0.2126 R + 0.7152 G + 0.0722 B. Sets with an opacity map are opacity-weighted. For the facades, wall and glass are split by the glass mask.
- **Brightness against the Look v2 target.** All sets are photographed in neutral daylight and are 2-10x brighter than the old dark albedo tokens (v1 ConcreteDark Y 0.023). To reach the darker Look v2 tone, apply an albedo gain or desaturation at import or in the material. Do not paint over the images.
- **Tiling.** All sets except the atlases and decals tile seamlessly. This was checked in Python: the edge-difference / interior-difference ratio is 0.4-1.8, and where it is above 1.3 the tile edge falls on a real joint. This was confirmed visually for concrete_panels, large_floor_tiles_02, wood_table_001 and Facade002.

## Chosen sets (summary)

TEX's original choice. Superseded by "Review (TEX-R)" → "Final sets" at the end of this file.

Size per tile is in metres; "est." means the source does not publish it.

| Class | Asset id | Src | Title | m / tile | Res | Maps | Mean linear RGB (albedo) | Y | MB |
|---|---|---|---|---|---|---|---|---|---|
| A | Facade018A | aCG | Facade 018 A | 13.0 x 13.0 | 2048 | Color, NormalGL/DX, Roughness, AO, Displacement, Metalness (glass mask), Emission (black) | (0.188, 0.159, 0.150); wall (0.291, 0.221, 0.186), glass (0.091, 0.100, 0.117) | 0.164 | 21.8 |
| A | Facade018B | aCG | Facade 018 B | 13.0 x 13.0 | 2048 | same as 018A, Emission = lit windows | (0.335, 0.304, 0.236); lit glass (0.375, 0.381, 0.282) | 0.306 | 22.5 |
| A | Facade019A | aCG | Facade 019 A | 13.0 x 13.0 | 2048 | Color, NormalGL/DX, Roughness, AO, Displacement, Metalness (glass mask), Emission (black) | (0.131, 0.134, 0.138); wall (0.132, 0.129, 0.122), glass (0.130, 0.139, 0.154) | 0.134 | 21.8 |
| A | Facade002 | aCG | Facade 002 | 36 x 36 est. | 2048 | Color, NormalGL/DX, Roughness, Displacement, Metalness (glass mask), Emission | (0.141, 0.135, 0.122); mullions (0.017, 0.016, 0.017), glass (0.156, 0.149, 0.134) | 0.136 | 4.9 |
| A | concrete_panels | PH | Concrete Panels | 8.0 x 8.0 | 2048 | diff, nor_gl, rough, ao, disp | (0.318, 0.236, 0.166) | 0.248 | 10.2 |
| A/F | blue_metal_plate | PH | Blue Metal Plate | 2.5 x 2.5 | 2048 | diff, nor_gl, rough, ao, disp | (0.024, 0.049, 0.074) | 0.046 | 4.3 |
| B | concrete_wall_006 | PH | Concrete Wall 006 | 2.0 x 2.0 | 2048 | diff, nor_gl, rough, ao, disp | (0.108, 0.084, 0.062) | 0.087 | 14.4 |
| B | concrete_layers | PH | Concrete Layers | 1.55 x 1.55 | 2048 | diff, nor_gl, rough, ao, disp | (0.153, 0.131, 0.114) | 0.135 | 10.9 |
| B | Leaking003 | aCG | Leaking 003 (decal) | 2.0 x 1.0 est. | 2048x1024 | Color, NormalGL/DX, Roughness, Displacement, Opacity | (0.041, 0.044, 0.024), opacity-weighted; cover 0.24 | 0.042 | 10.9 |
| C | brick_wall_001 | PH | Brick Wall 001 | 3.0 x 3.0 | 2048 | diffuse, nor_gl, rough, ao, displacement | (0.140, 0.078, 0.052) | 0.089 | 5.5 |
| C | damaged_plaster | PH | Damaged Plaster | 1.85 x 1.85 | 2048 | diff, nor_gl, rough, ao, disp | (0.302, 0.233, 0.177) | 0.244 | 11.1 |
| D | asphalt_02 | PH | Asphalt 02 | 3.0 x 3.0 | 2048 | diff, nor_gl, rough, ao, disp | (0.116, 0.114, 0.102) | 0.114 | 13.4 |
| D | Asphalt020L | aCG | Asphalt 020 L | 4.6 x 4.6 | 2048 | Color, NormalGL/DX, Roughness, AO, Displacement | (0.157, 0.160, 0.147) | 0.159 | 36.8 |
| E | granite_tile | PH | Granite Tile | 2.3 x 2.3 | 2048 | diff, nor_gl, rough, ao, disp | (0.077, 0.078, 0.078) | 0.078 | 11.2 |
| E | large_floor_tiles_02 | PH | Large Floor Tiles 02 | 3.0 x 3.0 | 2048 | diff, nor_gl, rough, ao, disp | (0.186, 0.189, 0.177) | 0.188 | 7.4 |
| F | metal_plate | PH | Metal Plate (diamond plate) | 0.5 x 0.5 | 2048 | diff, nor_gl, rough, ao, disp, metal | (0.050, 0.034, 0.013) | 0.036 | 15.1 |
| F | MetalWalkway006 | aCG | Metal Walkway 006 (grating) | 1.0 x 1.0 est. | 2048 | Color, NormalGL/DX, Roughness, Displacement, Metalness, Opacity | (0.233, 0.237, 0.281), opacity-weighted; cover 0.35 | 0.240 | 16.4 |
| F | MetalPlates013 | aCG | Metal Plates 013 (rust-streaked steel) | 1.6 x 1.6 | 2048 | Color, NormalGL/DX, Roughness, AO, Displacement, Metalness | (0.106, 0.090, 0.086) | 0.093 | 21.9 |
| F | corrugated_iron_02 | PH | Corrugated Iron 02 | 2.7 x 2.7 | 2048 | diff, nor_gl, rough, ao, disp, metal | (0.100, 0.098, 0.083) | 0.097 | 10.7 |
| G | SurfaceImperfections003 | aCG | Surface Imperfections 003 | 1.5 x 1.5 est. | 2048 | Color, NormalGL/DX, Opacity | (0.025, 0.025, 0.025), opacity-weighted; cover 0.14 | 0.025 | 17.8 |
| G | Leaking012A | aCG | Leaking 012 A (water rivulets) | 1.0 x 1.0 est. | 2048 | Color (white), NormalGL/DX, Roughness, Displacement, Metalness (white) | (0.990, 0.990, 0.990); not an albedo, use Roughness | 0.990 | 2.9 |
| H | bark_brown_02 | PH | Bark Brown 02 | 1.0 x 1.0 | 2048 | diff, nor_gl, rough, ao, disp | (0.130, 0.107, 0.065) | 0.109 | 9.8 |
| H | LeafSet024 | aCG | Leaf Set 024 (atlas 3 x 3) | 0.40 x 0.40 | 2048 | Color, NormalGL/DX, Roughness, Displacement, Opacity | (0.179, 0.307, 0.062), opacity-weighted; cover 0.42 | 0.262 | 14.1 |
| H | LeafSet029 | aCG | Leaf Set 029 (ivy atlas 3 x 3) | 0.25 x 0.25 | 2048 | Color, NormalGL/DX, Roughness, Displacement, Opacity, Scattering | (0.178, 0.318, 0.008), opacity-weighted; cover 0.30 | 0.266 | 9.1 |
| H | Moss001 | aCG | Moss 001 | 0.45 x 0.45 | 2048 | Color, NormalGL/DX, Roughness, AO, Displacement | (0.260, 0.255, 0.139) | 0.248 | 36.4 |
| I | wood_table_001 | PH | Wood Table 001 | 1.5 x 1.5 | 2048 | diff, nor_gl, rough, ao, disp | (0.052, 0.011, 0.004) | 0.019 | 4.3 |
| I | Fabric045 | aCG | Fabric 045 | 0.5 x 0.5 est. | 2048 | Color, NormalGL/DX, Roughness, AO, Displacement | (0.557, 0.552, 0.539) | 0.552 | 30.4 |
| I | long_white_tiles | PH | Long White Tiles | 1.27 x 1.27 | 2048 | diff, nor_gl, rough, ao, disp | (0.623, 0.581, 0.488) | 0.583 | 9.6 |

**Creation method.** The scanned sets (photogrammetry) are all 14 Poly Haven sets, Asphalt020L, LeafSet024, Moss001 and Fabric045. LeafSet029 is ambientCG "MultiAngle", a photographed set. The other ambientCG sets are "PBRProcedural": the facades, Leaking, MetalWalkway006, MetalPlates013 and SurfaceImperfections003. Where a scanned set existed for a class, it was preferred.

## Per-set record: URLs, measurements, suitability and intended surfaces

### Download URLs and source pages

- **ambientCG.** Download URLs are the ones the API reported: `https://ambientcg.com/get?file=<Id>_2K-JPG.zip`. Source page: `https://ambientcg.com/a/<Id>`.
- **Poly Haven.** Per-map files are at `https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/<id>/<id>_<map>_2k.jpg`, for each map listed above. Source page: `https://polyhaven.com/a/<id>`.

### A. Tower facades

The user complained most about this class: "the towers read as flat walls, not buildings".

- **Facade018A**
  - Source: https://ambientcg.com/a/Facade018A; download https://ambientcg.com/get?file=Facade018A_2K-JPG.zip.
  - Brick facade with punched windows: frames, sills, a recessed glazing plane, and glass carrying a soft interior gradient.
  - **Window grid, measured from the mask:**
    - 6 x 6 windows per 13 m tile, a pitch of 2.167 m in both axes;
    - openings 1.57 m wide x 1.55 m high;
    - glass fraction 0.518;
    - glass roughness 0.13, wall roughness 0.63.
    - A brick course of about 13 px (8.3 cm) confirms the 13 m tile.
  - **Mapping to NocFacade.** The analytic module is 2.4 x 3.6 m. Either map the tile to 14.4 m x 21.6 m (stretch 1.11 x 1.66; the bricks are invisible beyond about 60 m), or run the analytic lit grid at the texture's 2.167 m pitch.
  - Emission is black.
  - **Use:** residential stratum of near towers (T0/T1): alley mouth, the boulevard canyon walls, the Old Quarter around the plaza. The mask feeds the stateless per-window lit hash (H2), so the lit state is never baked.
- **Facade018B**
  - Source: https://ambientcg.com/a/Facade018B; download https://ambientcg.com/get?file=Facade018B_2K-JPG.zip.
  - The same layout as 018A. Its glass mask is pixel-identical to 018A and 019A (agreement 1.000).
  - **Use only its Emission map.** That map is the interior of a lit window: warm and cool rooms with blinds and curtains. Lit pixels are 52 % of the glass, with mean lit colour (0.83, 0.83, 0.69).
  - **Do not use its Color map,** because the lit windows are baked into it.
  - **Use:** the "lit room" appearance for 018A and 019A windows, chosen per window by the hash. Tint it with the beat's warm fraction and the WinCold / WinWarm tokens.
- **Facade019A**
  - Source: https://ambientcg.com/a/Facade019A; download https://ambientcg.com/get?file=Facade019A_2K-JPG.zip.
  - Office or commercial facade: dark stone piers, light concrete spandrel bands, ribbon windows with mullions and blinds.
  - The same 2.167 m grid and mask as 018A. It is almost neutral grey (wall 0.13).
  - **Use:** the commercial and office strata and the Gulf-side towers. Alternating 018A and 019A gives material variety with one window logic.
- **Facade002**
  - Source: https://ambientcg.com/a/Facade002; download https://ambientcg.com/get?file=Facade002_2K-JPG.zip.
  - Glass curtain wall with dark mullions: 16 x 10 panels per tile, glass fraction 0.895, glass roughness 0.03.
  - **Tile size.** It is not published. At a 3.6 m floor height the tile is about 36 m (panels 2.25 x 3.6 m); mapping it to the 2.4 x 3.6 module gives 38.4 x 36 m.
  - **Caveats:**
    - the glass albedo carries faint baked daylight tree reflections at the bottom of each pane;
    - the Emission map has a baked warm-lit pattern (14 % of panes).
  - **Use:** only the mullion mask, the normal and the roughness. Replace the glass colour with the analytic night reflection plus the NocFacade lit hash.
  - **Use:** glass towers, the spire shaft, skybridge-adjacent glass faces.
- **concrete_panels** (author: Amal Kumar)
  - Source: https://polyhaven.com/a/concrete_panels; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/concrete_panels/.
  - Precast ribbed concrete panels in a staggered bond. Panels are about 2 m wide, with vertical ribs at 8.7 cm (FFT of the height map), and the tile is 8 m. The panels carry real vertical rain streaks and stains.
  - It is beige (R/B 1.9): desaturate it toward neutral for Look v2.
  - **Use:** blank tower faces and mechanical bands (mechanical strata, 9 x 6 m slits overlaid), podia, NocSection Gulf faces between the bays.
- **blue_metal_plate** (author: Rob Tuytel)
  - Source: https://polyhaven.com/a/blue_metal_plate; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/blue_metal_plate/.
  - Painted steel sheet with vertical panel seams, scratches and chipped edges.
  - It has no metal map. Treat it as paint (dielectric) with the bare-metal scratches.
  - Its deep slate-blue albedo (Y 0.046) is close to the dark look already; hue-shift it if neutral is wanted.
  - **Use:** metal panel cladding on the spire base and podia, kiosk and vending bodies (class I), service doors.

### B. Stained, weathered concrete

- **concrete_wall_006** (authors: Dario Barresi, Charlotte Baglioni)
  - Source: https://polyhaven.com/a/concrete_wall_006; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/concrete_wall_006/.
  - Dark, scanned cast-in-place wall: rust-brown and water stains running vertically, form-tie holes.
  - It is the darkest wall set (Y 0.087).
  - **Use:** the alley walls (x +-2.2), podium bases, the undersides of bridges and tongues.
- **concrete_layers** (author: Amal Kumar)
  - Source: https://polyhaven.com/a/concrete_layers; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/concrete_layers/.
  - Horizontal pour layers with white efflorescence drips: vertical leak streaks below each layer.
  - **Use:** retaining walls and balustrade bases on the tongues, the lower strata of the Gulf faces, the terrace edges.
- **Leaking003** (decal)
  - Source: https://ambientcg.com/a/Leaking003; download https://ambientcg.com/get?file=Leaking003_2K-JPG.zip.
  - A 2:1 decal with opacity (cover 0.24): rain and moisture streaks hanging from a mossy top edge, with roughness and normal.
  - **Footprint.** It is not published; use about 2.0 x 1.0 m. Stretching it up to 2x vertically on tall walls is acceptable.
  - **Use:** an overlay under ledges, sills, sign frames and slab joints, in the alley and on the podia. Darken the albedo and lower the roughness where it is wet.

### C. Brick and plaster

- **brick_wall_001** (authors: Rob Tuytel, Dimitrios Savva)
  - Source: https://polyhaven.com/a/brick_wall_001; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/brick_wall_001/.
  - Dark, multi-tone burnt brick with deep mortar joints and soot. The 3 m tile limits repetition on the 14.4 m tenement.
  - **Use:** the Old Quarter tenement (the goal window wall, beat 7) and the alley side buildings.
- **damaged_plaster** (author: Amal Kumar)
  - Source: https://polyhaven.com/a/damaged_plaster; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/damaged_plaster/.
  - Plaster over brick, broken off in patches.
  - It is light (Y 0.244): apply a strong albedo gain below 1 and desaturate.
  - **Use:** alley wall patches, the noodle bar exterior, the lower tenement storey.

### D. Asphalt

The wet look itself comes from NocWet / H4. These sets supply the base albedo, the micro normal and the roughness variation.

- **asphalt_02** (author: Rob Tuytel)
  - Source: https://polyhaven.com/a/asphalt_02; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/asphalt_02/.
  - Scanned neutral-grey asphalt with fine aggregate and long sealed cracks.
  - The cracks give natural lines for water. Feed the crack/AO cavity into the puddle mask instead of the current blotch noise; that noise is an open defect.
  - **Use:** the alley floor (beat 1).
- **Asphalt020L**
  - Source: https://ambientcg.com/a/Asphalt020L; download https://ambientcg.com/get?file=Asphalt020L_2K-JPG.zip.
  - Scanned dark-grey asphalt. The tile is large (4.6 m), with low-frequency wear patches, and has AO.
  - **Use:** the boulevard lanes and any wide asphalt deck, where a large tile hides the repetition.

### E. Pavers and plaza

- **granite_tile** (author: Charlotte Baglioni)
  - Source: https://polyhaven.com/a/granite_tile; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/granite_tile/.
  - Dark flamed-granite slabs in offset courses, 0.77 m wide (3 per 2.3 m tile, measured).
  - Neutral grey, and dark already (Y 0.078).
  - **Use:** Heart Square plaza (beat 7), the promenade, the bridge landings.
- **large_floor_tiles_02** (author: Rob Tuytel)
  - Source: https://polyhaven.com/a/large_floor_tiles_02; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/large_floor_tiles_02/.
  - 1.0 m concrete or stone sidewalk slabs (3 x 3 per tile, measured), with chips, cracks and dirty joints. Cool grey.
  - **Use:** the boulevard sidewalks, the tongue deck, the garden terrace paths.

### F. Steel

- **metal_plate** (author: Rob Tuytel)
  - Source: https://polyhaven.com/a/metal_plate; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/metal_plate/.
  - Scanned diamond (checker) plate. It is painted olive-green with worn bare steel and rust; the metal map marks the bare metal.
  - Desaturate the paint for Look v2.
  - **Use:** stair treads, skybridge maintenance plates, alley steps and hatches.
- **MetalWalkway006**
  - Source: https://ambientcg.com/a/MetalWalkway006; download https://ambientcg.com/get?file=MetalWalkway006_2K-JPG.zip.
  - Square welded-mesh grating with opacity (bars cover 0.35), 16 x 16 cells per tile.
  - **Tile size.** It is not published. At 1.0 m per tile: 62 mm pitch, 52 mm openings, a standard 50 mm mesh.
  - Needs alpha-to-coverage (MSAA 4x) and a mip-aware fade to the mean coverage, to avoid moire.
  - **Use:** catwalks, fire escapes in the alley, the side screens of the bridges.
- **MetalPlates013**
  - Source: https://ambientcg.com/a/MetalPlates013; download https://ambientcg.com/get?file=MetalPlates013_2K-JPG.zip.
  - Dark steel plates with seams, rivets and vertical rust runs. The metalness map separates steel from rust.
  - **Use:** structural steel: bridge girders, sign frames and backs, tube and track steel, spire bracing.
- **corrugated_iron_02** (authors: Jenelle van Heerden, Sergej Majboroda)
  - Source: https://polyhaven.com/a/corrugated_iron_02; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/corrugated_iron_02/.
  - Scanned galvanized corrugated sheet. It has a metal map; the corrugation pitch is 9.6 cm (28 per tile, measured).
  - **Use:** alley shacks and lean-tos, rooftop sheds, the back of awnings, utility boxes.
  - Fade the corrugation normal by distance to avoid aliasing.

### G. Glass overlays

- **SurfaceImperfections003**
  - Source: https://ambientcg.com/a/SurfaceImperfections003; download https://ambientcg.com/get?file=SurfaceImperfections003_2K-JPG.zip.
  - Vertical grime and run streaks: Color, Opacity and NormalGL only.
  - **Use the Opacity map** (cover 0.14) as the dirt mask on window glass and on NocGlass tubes: raise the roughness, lower the reflectance, darken. The albedo is near black (Y 0.025).
  - Suggested tile: 1.5 m. Use it on the near-tier windows only.
- **Leaking012A**
  - Source: https://ambientcg.com/a/Leaking012A; download https://ambientcg.com/get?file=Leaking012A_2K-JPG.zip.
  - A water-film layer with thin vertical rivulets.
  - Its albedo and metalness are flat white. **Use only its Roughness map** (rivulet lines; dark = smooth) and its subtle NormalGL.
  - Suggested tile: 1.0 m.
  - **Use:** a wet-rain overlay on glass, painted metal and vending fronts in beats 1-4.

### H. Garden

- **bark_brown_02** (author: Rob Tuytel)
  - Source: https://polyhaven.com/a/bark_brown_02; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/bark_brown_02/.
  - Scanned furrowed broadleaf bark with moss patches and knots.
  - **Use:** the hanging-garden trunks and branches (beat 5).
- **LeafSet024**
  - Source: https://ambientcg.com/a/LeafSet024; download https://ambientcg.com/get?file=LeafSet024_2K-JPG.zip.
  - A scanned atlas of 3 x 3 beech-like leaves with opacity (cover 0.42) and normal; 40 cm across.
  - **Use:** leaf cards for the crowns. They replace the "flat green blobs" the user noted. The NocFoliage glow and rim stay in the shader; cards need alpha-to-coverage.
- **LeafSet029**
  - Source: https://ambientcg.com/a/LeafSet029; download https://ambientcg.com/get?file=LeafSet029_2K-JPG.zip.
  - A 3 x 3 ivy leaf atlas, 25 cm across, with Opacity and a **Scattering** (translucency) map for the thin-leaf wrap term (8.11 item 7).
  - Two of its nine leaves are autumn-spotted; mask them out or keep them as rare variants.
  - **Use:** ivy curtains over the garden edges and the tenement, the hanging vines.
- **Moss001**
  - Source: https://ambientcg.com/a/Moss001; download https://ambientcg.com/get?file=Moss001_2K-JPG.zip.
  - Scanned low leafy moss with a little litter, 45 cm, with AO.
  - The small tile needs macro variation or triplanar blending.
  - **Use:** garden beds and ground cover, moss at the bases of planters.

### I. Props

- **wood_table_001** (authors: Dimitrios Savva, Rico Cilliers)
  - Source: https://polyhaven.com/a/wood_table_001; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/wood_table_001/.
  - Dark varnished wenge-like wood with stains; very dark already (Y 0.019).
  - **Use:** the noodle bar counter and stools, the tenement door and window frame around the goal window.
- **Fabric045**
  - Source: https://ambientcg.com/a/Fabric045; download https://ambientcg.com/get?file=Fabric045_2K-JPG.zip.
  - Scanned, creased heavy cotton canvas: undyed off-white with dirt and folds.
  - The tile size is not published; use about 0.5 m.
  - **Use:** awnings and the noodle-bar curtain, tinted dark (amber, oxblood, slate) in the material. No text.
- **long_white_tiles** (authors: Jenelle van Heerden, Sergej Majboroda)
  - Source: https://polyhaven.com/a/long_white_tiles; files at https://dl.polyhaven.org/file/ph-assets/Textures/jpg/2k/long_white_tiles/.
  - Glossy glazed slim wall tiles, 3.8 x 12.7 cm (33 x 10 per tile, measured), weathered and chipped, with dirty grout.
  - **Use:** the noodle bar interior wall behind the counter (seen through the warm opening). Keep it light: it is the bounce surface of the WinWarm light.
- **Painted metal for vending.** This reuses blue_metal_plate (class A/F) plus the Leaking012A wet overlay.

## Rejected candidates

One line each. Previews were viewed as flat albedo and/or the source's sphere render; see `ExternalTextures/previews/`.

- aCG Facade001, Facade005: glass grid with a baked daylight sky and tree reflection over every pane; no emission. Wrong for night.
- aCG Facade006: ribbon windows with white spandrels; baked tree reflections, no emission or AO. Facade019A covers the band-window office with a clean mask.
- aCG Facade003, Facade004: mostly-lit glass grids. The lit state is baked, which contradicts the stateless per-window toggling of H2.
- aCG Facade007, 008, 009, 010, 011: night photos of lit offices showing human silhouettes (no-people rule), with the lit pattern baked.
- aCG Facade012, 013, 014, 015, 016: dense night-skyscraper grids with baked interiors and lit pattern.
  - The scale is unknown (about 40 windows per tile), so moire is a risk.
  - The analytic NocFacade already covers the far field.
- aCG Facade017: night office with photographic interiors (posters, possible text) and baked lit bands.
- aCG Facade018C, 019B, 019C, 020B, 020C: the same window mask as 018A/019A. Their lit variants are redundant with 018B's emission.
- aCG Facade020A: brick-pier variant of the same grid; too close to 018A.
- PH concrete_tile_facade: dark grooved cladding. Its 2 cm grooves alias at tower distances, and it is not metal.
- PH rectangular_facade_tiles and rectangular_facade_tiles_02: small tiles with coloured bands; too busy and saturated.
- PH container_side: bright green; reads as a shipping container.
- PH concrete_wall_008: beige board-formed concrete; too clean, few stains.
- PH concrete_wall_004: painted block wall, too clean.
- PH concrete (board-form 4 m): strong beige formwork ribs. concrete_panels covers precast; kept as a backup.
- PH concrete_layers_02: grey board-formed concrete without streaks. Backup for concrete_layers.
- PH concrete_wall_007: light beige with chipped patches.
- PH plastered_wall_03: warm, blotchy.
- PH painted_concrete_02: flat painted; low detail.
- PH dirty_concrete: a floor with mud clumps, not a wall.
- PH cracked_concrete_wall: bright peeling plaster, 1 m tile.
- aCG Leaking001, 002, 004, 005: the same procedural series as Leaking003, which has the cleanest streak spread.
- aCG Leaking006, 010A, 011A: a single stain under a visible pipe hole; yellow and olive tint.
- aCG Leaking008, 009: green gradient bands with little streak detail.
- aCG Concrete028: procedural bunker cast concrete, flat look.
- aCG Concrete044D: blotchy damage; reads as noise.
- PH red_brick_03: excellent relief but a 1 m tile (repeats on the tenement). Backup.
- PH brick_wall_006, red_bricks_04: bright orange.
- PH factory_brick: light beige-brown with pale mortar.
- PH brick_4: 0.5 m with green mortar.
- PH castle_brick_02_red: medieval and irregular.
- PH brick_wall_02: pale mortar, high contrast.
- PH painted_brick: bright cyan paint.
- PH plaster_brick_01: a trim sheet (brick over a mossy band); not tileable vertically.
- PH plastered_stone_wall: dark but featureless at distance.
- PH concrete_wall_003: cream with black mould blotches.
- PH red_brick_plaster_patch_02: too orange.
- PH rough_plaster_brick: busy, mossy.
- PH painted_plaster_wall: plain grey, low character.
- PH asphalt_01, asphalt_03, road_damaged_clean: brown colour cast.
- PH clean_asphalt: good, but a 2.1 m tile and flatter. Backup for Asphalt020L.
- PH asphalt_04, asphalt_06: light, concrete-like.
- PH aerial_asphalt_01: a 30 m aerial tile with tyre arcs and little micro detail.
- PH asphalt_track: near black, featureless.
- PH bitumen: roofing stripes.
- PH asphalt_floor: beige.
- aCG Asphalt025C ("rain wet"): its roughness is soft blobs, the same camouflage look the critics flagged on NocWet.
- aCG Asphalt024C: regular crocodile cracking.
- aCG Asphalt015: light and fine.
- aCG Asphalt030: light beige.
- aCG Asphalt033: olive cast.
- aCG Asphalt023L: backup.
- aCG Road001, Road008B: baked lane markings and patches that would have to match the lane layout.
- PH large_square_pattern_01, large_grey_tiles: moss in the joints and irregular flagstones (garden-like, not plaza).
- PH pavement_02: orange crazy paving.
- PH concrete_pavement, concrete_pavement_02: beige.
- PH floor_tiles_09: brown grid pattern.
- PH square_tiles_03: small mosaic.
- PH slab_tiles: orange grout.
- PH gravel_concrete_03: no joints.
- PH floor_pavement: small beige tiles.
- aCG DiamondPlate008C: procedural, neutral grey; the scanned PH metal_plate was preferred. Backup if neutral paint is wanted.
- aCG DiamondPlate009: worn brown, low relief.
- aCG MetalWalkway014: perforated rust-red.
- aCG MetalWalkway013: slot pattern, near black.
- aCG Metal027: flat black paint without seams (vending alternative).
- aCG PaintedMetal014: heavy peeling blue paint and rust.
- aCG CorrugatedSteel009: light horizontal corrugation.
- aCG MetalPlates014: light brushed steel.
- aCG MetalPlates017A: sci-fi octagons.
- aCG SheetMetal001: white speckle.
- PH green_metal_rust: green.
- PH rusty_metal_02: white with orange rust.
- PH rusty_metal_04: heavy orange rust.
- PH factory_wall: green corrugated.
- PH painted_metal_shutter: light, horizontal.
- PH metal_grate_rusty: orange bar plate, not see-through.
- PH rusty_metal_grid: teal paint.
- PH box_profile_metal_sheet: red.
- aCG SurfaceImperfections001: round water spots. Backup for dry glass.
- aCG SurfaceImperfections013: debris specks.
- aCG SurfaceImperfections008: cloudy smudge.
- aCG Smear005, 006, 007: horizontal wipe strokes; the wrong direction for rain grime.
- aCG Leaking013C: droplet speckle.
- PH bark_brown_01: yellow-brown.
- PH bark_willow_02: pale grey.
- PH pine_bark: orange.
- PH tree_bark_03: pale with lichen.
- aCG Bark014: orange pine.
- aCG Bark016: mossy, good. Backup.
- aCG Bark012: beige.
- aCG LeafSet022: spotted leaves on a white halo.
- aCG LeafSet023: pale heart leaves with a halo.
- aCG LeafSet025: round geranium leaves.
- aCG LeafSet014: pale serrated leaves.
- aCG LeafSet016: oak with a black background; lobed. Backup.
- aCG LeafSet017: pale-veined ivy with a dark blotch.
- aCG Moss002: procedural flat green.
- aCG Ground037: yellowish grass and dirt.
- PH forest_leaves_02: autumn leaf litter.
- PH mossy_rock: lichen rock.
- PH leafy_grass: dry brownish.
- PH dark_wood: too regular veneer.
- PH plank_flooring: parquet.
- PH wood_floor_deck: orange planks.
- PH interior_tiles: beige diagonal floor.
- PH tiled_floor_001: brown floor tiles.
- PH floor_tiles_08: quarry floor.
- aCG Fabric061, 062, 066: fine regular weave (moire risk).
- aCG Fabric030, 031: approximated woven grey.
- aCG PaintedMetal001: orange scratched paint.

## Reproduction

The working scripts are in `ExternalTextures/tmp/scripts/` (git-ignored scratch):

- `acg_summary.py`, `ph_summary.py`: query listings.
- `acg_previews.py`, `ph_previews.py`, `acg_maps_preview.py`: preview grids.
- `dl_sets.py`: download and extract.
- `window_pitch.py`: facade grid.
- `stats_sheet.py`: mean colours and the contact sheet.

The API responses are cached in `ExternalTextures/tmp/api/`, and the per-set download log is in `ExternalTextures/tmp/download_log.json`. All were run with `python -I`, using curl with a generic user agent.

## Review (TEX-R)

Date: 2026-11-14. Reviewer: TEX-R, as art director. Scope: TEX's 28 sets, judged against six rejection criteria: baked daylight shadows, visible repetition, wrong scale, single obvious features that show as a repeat, casts that read violet or saturated, and low resolution. The review also checked that every class A-I has a good set, and that class A can make towers read as buildings.

**Outcome.** 16 sets kept, 9 rejected, 3 demoted to reference only, 13 added. The final list is "Final sets" at the end of this section.

### Method

- **Viewing.** Every albedo was viewed three ways: tiled 3 x 3 (repetition), as a 1:1 centre crop (resolution, single features) and as a 1:1 crop across the wrap seam. The sheets are `ExternalTextures/tmp/texr/out/sheet_*.jpg` (git-ignored).
- **Measurements.** The script is `ExternalTextures/tmp/texr/review.py`, run with `python -I`; values are in `out/metrics.json`.
  - **lf:** the coefficient of variation of low-frequency luminance (periodic Gaussian, sigma N/8). It flags large stains and baked gradients.
  - **bake:** the absolute correlation between high-pass luminance and the high-passed normal map x/y. It flags baked directional light. A Lambert render from the measured direction, set beside the albedo, confirmed it on concrete_panels (`out/bake.jpg`).
  - **hf:** the share of luminance power above N/4 cycles, a measure of true resolution.
- **Violet after white balance (`tokencheck.py`).** The texprep colour rule (TEXTURE_PLAN 1.4, step 5) computes albedo x token / set mean, which is a per-channel white balance. The check runs that rule with the set's intended token family: neutral grey, Bark brown or Moss green. It reports the share of pixels that come out violet or magenta (hue 250-340 deg, sRGB chroma above 0.12).
- **Licences.** These were re-checked on both libraries' licence pages and on the page of every final set (see "Licence verification").

### Findings that change processing (for TP, FA, SU, PA and FO)

1. **The colour rule turns casts into their complementary hue.**
   - Dividing by a strongly cast mean makes the neutral parts of the image take the opposite hue. Under olive paint, bare steel turns violet; under red brick, grey grout turns cyan.
   - **Gate:** violet after white balance must be 0.5 % or less with the intended token.
   - **Failed:** metal_plate 6.4 % and wood_table_001 3.2 %. Every final set scores 0.0-0.2 %. LeafSet029 scores 0.0 % once its two autumn leaves are masked out, and 0.5 % with them.
   - **Rule:** choose sets whose own cast is neutral or close to the token's hue. Neutral sets (concrete_floor_worn_001, Metal046B, brick_wall_08, poly_wool_herringbone) take any token without artefacts.
2. **Some scans have directional light baked in.**
   - Bake values: concrete_panels 0.43 (rejected), concrete_wall_006 0.40, bark_brown_02 0.33 (rejected), asphalt_02 0.32.
   - **Proposal for texprep, step 4b:** fit the high-pass luminance (sigma 6 px) to the high-passed normal x/y by least squares, then subtract the fitted part. Gate: bake 0.10 or less afterwards. It is needed for concrete_wall_006 and asphalt_02.
   - Every other final set is at 0.25 or less (most below 0.10), so they need nothing. LeafSet024 measures 0.25, but it is a leaf card, not a surface.
3. **Facade skins must have no joints of their own.**
   - In option (a) (TEXTURE_PLAN 2.4), floor lines, panel joints and openings are analytic.
   - A skin with its own joint grid puts a second, unrelated grid across the windows. concrete_panels, with its 2 m staggered panels, does exactly that.
   - The skin slices 0, 2 and 8 therefore use joint-free sets.
4. **Scale corrections and measurements:**
   - **granite_tile.** The slabs are **0.46 m squares in stack bond**: 5 x 5 per 2.3 m tile, with the joint period measured as 409 px on the AO in both axes. TEX's figure of 0.77 m is wrong.
     - The plaza "1.2 m slabs" of TEXTURE_PLAN 3 would need a 2.6x stretch, which coarsens the flamed grain.
     - Keep 0.46 m (module 0.46 m, no row shift), or TECH changes the plaza spec.
   - **Moss001.** The published tile is 0.45 m. TEXTURE_PLAN 3 maps "Moss" at 1.5 m, which makes the fronds 3.3x too big. Map it at 0.45 m (0.9 m at most), with height-aware hex and macro variation.
   - **concrete_tiles_02.** Slabs are 0.305 x 0.61 m with the long side along v, laid in half bond. Measured: joints every 342 and 683 px of 2048.
   - **rectangular_facade_tiles.**
     - Courses are 0.154 m (157.5 px).
     - Tile widths are an irregular 5-10 cm, so use offset-only hex, not module shuffle, and no rotation.
     - Courses fall under 2 px beyond about 64 m at 1080p; the mips and Toksvig handle that.
   - **brick_wall_08.** The course is 0.093 m (about 100 px); module shuffle per brick.
   - **DiamondPlate008C.** No published size. The lug period is tile/20 (FFT of the height map), so a 0.6 m tile gives 30 mm lugs, the same as metal_plate's measured 29 mm.
   - **Metal046B.** No published size and no structure; use 2.0 m with hex.
5. **Maps to use and not use:**
   - **Leaking003:** use Opacity, Roughness and NormalGL only. Its Color has a saturated green moss band, and the overlay arrays carry no albedo anyway.
   - **OfficeCeiling001:** use Color, AO, NormalGL and Roughness. Ignore Emission and Metalness. Its seam score of 2.16 is the ceiling-grid joint on the tile edge, a real joint.
   - **LeafSet029:** mask out the two autumn leaves (TEX's note stands).
   - **poly_wool_herringbone:** it is 2048 x 2091 (0.270 x 0.276 m). Resample to square in texprep.
   - **fine_grained_wood:**
     - It has no displacement map, so texprep must derive the mask's B channel (height) from the normal map by integrating it, or use inverted AO as a proxy. Step 6 only covers the opposite case, AO from height.
     - Its seam ratio is 1.48, but no seam shows across the wrap even at 2.2x gain.
   - **plank_flooring_04 and concrete_floor_worn_001:** seam ratios are 1.44 and 1.34. The first falls on a plank joint; the second is within the interior column-difference p99. Both are fine.

### Class A: can the facades make towers read as buildings?

**Not on their own.**
- Facade018A, 018B, 019A and 002 are ambientCG "PBRProcedural" sets:
  - flat, single-colour glass;
  - the same 6 x 6 window grid in every tile;
  - clean CG brick;
  - hf 0.004-0.12. Facade002 is very soft (hf 0.004), and it also has baked lit panes and daylight tree reflections.
- Tiled on a tower, they would give the same "wallpaper" the user rejected. TEXTURE_PLAN 2.1 already declines them as the facade model.

In option (a), three things make a tower read as a building:
1. **Analytic structure:** slab lines, piers, recessed openings with reveals, mullions, sills and parapets.
2. **Lit rooms behind the glass:** interior mapping with the room atlas.
3. **Photographic skins** with real micro-relief and no joints of their own.

The review therefore spent class A on items 2 and 3. It chose joint-free, neutral, dark skins and added a **class R (room interiors)**, which no TEX set covered except the knitted rug Fabric045. Facade018A, 018B and 019A stay on disk as **reference only** (window pitch 2.167 m, openings 1.57 x 1.55 m). Do not process them. Facade002 is rejected.

**Skin slices (FacadeArr, TEXTURE_PLAN 2.3):**

| Slice | Plan named | Use |
|---|---|---|
| 0 precast | concrete_panels | **concrete_floor_worn_001** (3.0 m; neutral, Y 0.094, no joints, no repetition at 3 x 3) |
| 1 board/weathered | concrete_layers | concrete_layers (kept) |
| 2 metal panel | MetalPlates013, blue_metal_plate | **Metal046B** (neutral near-black, Y 0.051); MetalPlates013 as the weathered variation |
| 3 ribbed | corrugated_iron_02 | kept |
| 4 anodised aluminium | - | no photo set. Frames and mullion caps are under 4 px beyond about 15 m, so use a constant albedo and smoothness. |
| 5 spandrel glass | crops of the Facade sets | no photo set; those sets have no usable spandrel. Use dark glass plus slice 11 grime. |
| 6 ceramic tile | long_white_tiles | **rectangular_facade_tiles** (dark slim tiles, Y 0.093). long_white_tiles stays as a rare light variant. |
| 7 brick | brick_wall_001 | **brick_wall_08** |
| 8 plaster | damaged_plaster | **plastered_wall_04** |
| 9 stone | granite_tile | kept |
| 10 louvre or perforated | - | analytic blades on Metal046B or corrugated_iron_02 |
| 11 glass grime | SurfaceImperfections003 | kept |

**Room atlas (RoomArr, TEXTURE_PLAN 2.7):**
- Slices 0-2, walls: plastered_wall_04 (with three tints), long_white_tiles (kitchens) and concrete_floor_worn_001 (raw offices).
- 3, wood floor: plank_flooring_04.
- 4, carpet: Carpet016.
- 5, tile floor: large_floor_tiles_02.
- 6, ceiling: OfficeCeiling001.
- 7, curtain: poly_wool_herringbone.

### Verdicts on TEX's 28 sets

| Set | Verdict | Reason (measured) |
|---|---|---|
| Facade018A | reference only | procedural; flat glass; identical 6 x 6 grid per tile (hf 0.115); not used by option (a) |
| Facade018B | reference only | its Emission is a baked lit pattern; interior mapping replaces it |
| Facade019A | reference only | procedural (hf 0.050); bright white sill strip; not used by option (a) |
| Facade002 | **rejected** | baked lit panes in Color and Emission; daylight tree reflections; hf 0.004 (soft); the mullion grid is analytic anyway |
| concrete_panels | **rejected** | baked side light (bake 0.43, confirmed visually on the ribs); beige cast (sat 0.26); its own 2 m staggered joints clash with the analytic cells; one orange-stained panel per tile makes an 8 m repeat lattice |
| blue_metal_plate | **rejected** | saturated blue (sat 0.45, chroma variation 1.00); blotches repeat at 3 x 3 (blob 0.09); its rust specks turn saturated red under a neutral token |
| concrete_wall_006 | kept, with conditions | good dark scan; bake 0.40, so it needs texprep step 4b |
| concrete_layers | kept | neutral, bake 0.01, hf 0.19, no repeat |
| Leaking003 | kept, mask only | use Opacity, Roughness and Normal only; Color has a green moss band |
| brick_wall_001 | **rejected** | saturated orange (sat 0.39/0.61); bright white paint patches are single features that repeat in the 3 x 3 |
| damaged_plaster | **rejected** | its broken-plaster patches over brick form an obvious repeating pattern at 3 x 3; light, warm |
| asphalt_02 | kept, with conditions | excellent aggregate (hf 0.25); bake 0.32, so it needs step 4b; its 3 m crack must be broken up with hex and rotation, never plain-tiled |
| Asphalt020L | kept | neutral, lf 0.01, hf 0.26 |
| granite_tile | kept | neutral, dark, hf 0.30; slab size corrected to 0.46 m |
| large_floor_tiles_02 | kept | slab tone pattern repeats when plain-tiled, so module shuffle is required (as planned) |
| metal_plate | **rejected** | olive paint plus rust (sat 0.53/0.67); violet after white balance 6.4 % (bare steel turns violet under a neutral token) |
| MetalWalkway006 | kept | procedural but clean; the opacity edges are what count; alpha-to-coverage as planned |
| MetalPlates013 | kept | rust runs are acceptable for structural steel; violet after white balance 0 %; hex offsets only |
| corrugated_iron_02 | kept | neutral, no repeat |
| SurfaceImperfections003 | kept | mask |
| Leaking012A | kept | roughness only |
| bark_brown_02 | **rejected** | knots and a pale vertical band repeat every 1 m (lf 0.19); bake 0.33; moss patches (sat 0.29/0.57) |
| LeafSet024 | kept | scanned, sharp (hf 0.26) |
| LeafSet029 | kept | two autumn leaves masked out |
| Moss001 | kept, with conditions | the only scanned moss ground cover found on either library (the procedural Moss002-004 are flat green). Map at 0.45 m. Its orange needle litter goes under the green token without violet (0.0 %). Use chromaKeep of 0.5 or less. |
| wood_table_001 | **rejected** | extreme orange-red (sat 0.82/1.00); a large cathedral grain figure repeats every 1.5 m along the counter; violet after white balance 3.2 % |
| Fabric045 | **rejected** | it is a knitted or looped rug (1 cm stitch columns at 1:1, tagged "rug"), not canvas; wrong for awnings, noren and curtains |
| long_white_tiles | kept | noodle-bar interior and the light tile variant |

**A reversal.** TEX rejected rectangular_facade_tiles as "small tiles with coloured bands; too busy and saturated". The measurement does not support that: sat 0.10/0.14, neutral dark grey, violet 0 %, hf 0.19. It is now the slice 6 tile.

### Added sets (13)

Every set: CC0 1.0, 2K JPG, one new empty directory under `ExternalTextures/raw/`, checked as a real image with PIL `verify()`. ambientCG zips were extracted with `zipfile` and a path-traversal guard, keeping images only. API JSON was read in memory and never saved. Nothing was executed.

- **concrete_floor_worn_001** (Poly Haven; photography Dimitrios Savva, processing Rico Cilliers). https://polyhaven.com/a/concrete_floor_worn_001
  - 3.0 m; diff, nor_gl, rough, ao, disp; 3.4 MB.
  - Mean linear (0.093, 0.095, 0.089), Y 0.094.
  - Measures: lf 0.00, bake 0.07, hf 0.135, sat 0.03, seam 1.34 (inside p99).
  - Its fine floor scratches read as weathering on a vertical face beyond about 5 m.
  - **Use:** skin slice 0 (all precast and plain-concrete towers, roofs in mode 2), the `Concrete` library material (planters, columns, slabs), raw-office room walls. Not the near alley walls, which stay concrete_wall_006.
- **Metal046B** (ambientCG). https://ambientcg.com/a/Metal046B, download https://ambientcg.com/get?file=Metal046B_2K-JPG.zip
  - 2.0 m est.; Color, NormalGL/DX, Roughness, Displacement, Metalness; 21.1 MB.
  - Mean (0.051, 0.051, 0.051), Y 0.051; sat 0.00, bake 0.01, hf 0.086.
  - Procedural dark metal with smudges, which repeat when plain-tiled, so hex is required.
  - **Use:** skin slice 2 (metal-clad towers), `SteelPainted` (truss, gates, lamp housings, bollards), vending bodies and service doors.
- **rectangular_facade_tiles** (Poly Haven; Charlotte Baglioni). https://polyhaven.com/a/rectangular_facade_tiles
  - 2.0 m; diff, nor_gl, rough, ao, disp; 10.0 MB.
  - Mean (0.098, 0.093, 0.079), Y 0.093; sat 0.10, bake 0.04, hf 0.19, seam 0.75.
  - Courses 0.154 m; tiles 5-10 cm wide.
  - **Use:** skin slice 6 (tiled residential towers, S4).
- **brick_wall_08** (Poly Haven; Amal Kumar). https://polyhaven.com/a/brick_wall_08
  - 1.9 m; diff, nor_gl, rough, ao, disp; 12.5 MB.
  - Mean (0.151, 0.135, 0.127), Y 0.138; sat 0.08, bake 0.16, hf 0.10, seam 0.96.
  - Neutral grey brick with a 0.093 m course. The warm brick token gives the colour without complementary artefacts (0 %).
  - **Use:** skin slice 7, the Old Quarter tenement and its neighbours, alley side buildings.
- **plastered_wall_04** (Poly Haven; Rob Tuytel). https://polyhaven.com/a/plastered_wall_04
  - 3.2 m; diff, nor_gl, rough, ao, disp; 4.8 MB.
  - Mean (0.266, 0.255, 0.245), Y 0.257; sat 0.04, bake 0.03.
  - Plain-tiled, its soft horizontal cloud bands show (blob 0.19). They are low-frequency, so the texprep de-light at strength 1.0 removes them; the gate is a residual of 3 % or less.
  - **Use:** skin slice 8, the noodle bar exterior, alley wall patches, the lower tenement storey, room walls.
- **concrete_tiles_02** (Poly Haven; Charlotte Baglioni). https://polyhaven.com/a/concrete_tiles_02
  - 1.825 m; diff, nor_gl, rough, ao, disp; 9.5 MB.
  - Mean (0.139, 0.126, 0.110), Y 0.127; sat 0.11, bake 0.05, hf 0.15.
  - Slabs 0.305 x 0.61 m in half bond.
  - **Use:** the boulevard sidewalks, the promenade and the tongue terrace (`FloorStone`, `GrayFloor`). This is the "PavingStones set" that TEXTURE_PLAN 3 asked TEX for. Module shuffle.
- **DiamondPlate008C** (ambientCG). https://ambientcg.com/a/DiamondPlate008C, download https://ambientcg.com/get?file=DiamondPlate008C_2K-JPG.zip
  - 0.6 m est.; Color, NormalGL/DX, Roughness, AO, Displacement, Metalness; 23.2 MB.
  - Mean (0.149, 0.153, 0.167), Y 0.153; sat 0.05, violet 0 %, bake 0.02.
  - Neutral worn tread plate. It is procedural, so the scan of metal_plate is lost, but so is its olive and violet problem.
  - **Use:** stair treads, hatches, alley steps, the bridge deck plates.
- **trident_maple_bark** (Poly Haven; Charlotte Baglioni). https://polyhaven.com/a/trident_maple_bark
  - 1.8 m; diff, nor_gl, rough, ao, disp; 12.5 MB.
  - Mean (0.078, 0.048, 0.025), Y 0.053; lf 0.04, bake 0.03, hf 0.17.
  - Dark, even furrowed bark with no knots, no pale band and no repetition at 3 x 3.
  - **Use:** the hanging-garden trunks and branches, hex with offsets only.
- **fine_grained_wood** (Poly Haven; Rob Tuytel). https://polyhaven.com/a/fine_grained_wood
  - 0.6 m; col, nor_gl, rough, ao (no displacement); 5.4 MB.
  - Mean (0.055, 0.031, 0.016), Y 0.035; lf 0.02, bake 0.03, violet 0 %.
  - Straight dark grain with wear and no figure.
  - **Use:** the noodle bar counter and stools, the tenement door, the goal-window frame. Grain along the long axis, offsets only.
- **poly_wool_herringbone** (Poly Haven; photography colormass, processing Rico Cilliers). https://polyhaven.com/a/poly_wool_herringbone
  - 0.27 m; diff, nor_gl, rough, ao, disp, metal (ignore); 21.8 MB.
  - Mean (0.195, 0.182, 0.168), Y 0.184; sat 0.07, bake 0.04.
  - Neutral, fine woven fabric that takes any token (amber, oxblood, slate).
  - **Use:** awnings, noren and curtains (RoomArr slice 7), at 1K.
- **plank_flooring_04** (Poly Haven; Dimitrios Savva). https://polyhaven.com/a/plank_flooring_04
  - 2.0 m; diff, nor_gl, rough, ao, disp; 6.8 MB.
  - Mean (0.056, 0.019, 0.009), Y 0.026; violet 0.2 % with the warm token.
  - **Use:** RoomArr slice 3 (wood floor), at 512.
- **Carpet016** (ambientCG). https://ambientcg.com/a/Carpet016, download https://ambientcg.com/get?file=Carpet016_2K-JPG.zip
  - 1.7 m (published); Color, NormalGL/DX, Roughness, AO, Displacement; 30.1 MB.
  - Mean (0.502, 0.437, 0.295), Y 0.440. Scanned loop-pile carpet, no repetition.
  - **Use:** RoomArr slice 4.
- **OfficeCeiling001** (ambientCG). https://ambientcg.com/a/OfficeCeiling001, download https://ambientcg.com/get?file=OfficeCeiling001_2K-JPG.zip
  - 3.85 m (published; 6 x 6 tiles of 0.64 m); Color, NormalGL/DX, Roughness, AO, Displacement (Emission and Metalness ignored); 25.1 MB.
  - Y 0.909. Plain mineral-fibre grid; its variants 002-006 have baked light panels and were not used.
  - **Use:** RoomArr slice 6. Ceiling lights stay analytic.

The 13 sets add 186 MB.

### Candidates downloaded for measurement, then deleted

Each was downloaded as a full set, measured, and found worse than the chosen set:
- painted_concrete_02: horizontal banding repeats every 4 m tile; bake 0.35.
- brick_wall_10: plum-brown bricks; after a neutral white balance, 18.3 % of pixels are violet. This is exactly the violet the user rejected.
- brick_wall_11: ochre, with white paint patches; bake 0.31.
- plaster_grey_04: bake 0.61; dark blotches repeat at 3 x 3.
- rough_linen: blue cast (sat 0.29); poly_wool_herringbone is neutral.
- PavingStones109: good, but its basketweave of small pavers suits the boulevard less than concrete_tiles_02. **Backup.**
- PaintedPlaster017: bake 0.76; 2:1 aspect.

**Previewed only, rejected** (previews in `ExternalTextures/previews/texr/`):
- Concrete: concrete_slab_wall_02 (its own horizontal joints); damaged_concrete_floor and concrete_floor_damaged_01 (brown blotches); Concrete045 (beige board-form); Concrete046 (cream); Concrete036 (blotchy).
- Metal: Metal027 (blue-violet black); Metal032 and Metal009 (light, procedural).
- Brick: mossy_brick (green joints); dark_brick_wall (cream mortar, high contrast).
- Bark: metasequoia_bark and japanese_hackberry_bark (saturated); jolcham_oak_bark_01 (lichen patches).
- Fabric: hessian_230 (open burlap weave).
- Carpet: dirty_carpet (yellow stripes).
- Ceiling: OfficeCeiling002-006 (baked light panels).
- Garden path: slate_floor_02 (orange crazy paving), plus mossy setts.

### Substitutions for TEXTURE_PLAN rows that name a rejected set

| Plan row | Named | Use instead |
|---|---|---|
| FacadeArr 0; `Concrete` (planters, columns, tunnel, slabs, gate blocks); arcade columns | concrete_panels | concrete_floor_worn_001 |
| FacadeArr 2; `SteelPainted`; vending bodies | blue_metal_plate (+ MetalPlates013) | Metal046B (+ MetalPlates013) |
| `SteelBare`, `Brass` | metal_plate variation | MetalPlates013, whose metalness marks the bare steel. Diamond plate is the wrong pattern for bare steel. |
| Bridge deck (`FloorSteelDeck`) | MetalWalkway006, metal_plate | MetalWalkway006, DiamondPlate008C |
| FacadeArr 6 | long_white_tiles | rectangular_facade_tiles (long_white_tiles as a variant) |
| FacadeArr 7, Old Quarter `Brick` | brick_wall_001 | brick_wall_08 |
| FacadeArr 8, Old Quarter `Plaster` | damaged_plaster | plastered_wall_04 |
| FacadeArr 5 (spandrel glass) | Facade crops | none: dark glass plus slice 11 |
| Pavers (`FloorStone`, `GrayFloor`) | "TEX: a PavingStones set" | concrete_tiles_02 |
| Plaza (`FloorPlaza`) | large_floor_tiles_02, granite_tile "1.2 m" | granite_tile at its true 0.46 m slab (see finding 4) |
| Garden path (irregular flagstones) | - | no CC0 flagstone passes: candidates are orange crazy paving or mossy setts. Use large_floor_tiles_02 with height-aware hex and moss tint in the joints. |
| `Bark` | bark_brown_02 | trident_maple_bark |
| `Moss` | Moss001 at 1.5 m | Moss001 at 0.45 m |
| Noodle bar `Wood`, `Fabric` | wood_table_001, Fabric045 | fine_grained_wood, poly_wool_herringbone |
| RoomArr 0-7 | Fabric045 only | see "Room atlas" above |

### Coverage

Every class has at least one set that passes all six criteria:
- A: concrete_floor_worn_001, Metal046B, rectangular_facade_tiles, plus the shared sets.
- B: concrete_wall_006, concrete_layers, Leaking003.
- C: brick_wall_08, plastered_wall_04.
- D: Asphalt020L, asphalt_02.
- E: granite_tile, large_floor_tiles_02, concrete_tiles_02.
- F: DiamondPlate008C, MetalWalkway006, MetalPlates013, corrugated_iron_02, Metal046B.
- G: SurfaceImperfections003, Leaking012A.
- H: trident_maple_bark, LeafSet024, LeafSet029, Moss001.
- I: fine_grained_wood, poly_wool_herringbone, long_white_tiles.
- R: plank_flooring_04, Carpet016, OfficeCeiling001.

A hedge has no tileable CC0 set; build it from LeafSet024/029 cards. Every final set is neutral or sits on its token's hue, so none can bring back the violet cast.

### Licence verification (2026-11-14)

- https://polyhaven.com/license says the assets are all licensed as CC0: no attribution required, commercial use allowed. Logos, renders and site text are excluded, and none of them was downloaded.
- https://docs.ambientcg.com/license/ says all assets are under CC0 1.0 Universal.
- The page of every final set was fetched:
  - each Poly Haven page carries "CC0 1.0 Universal - public domain dedication" in its metadata and the CC0 licence block;
  - each ambientCG page says its assets are released under CC0. This includes the three reference facades.
- **Content rule (Part 6 item 5).** Every final albedo was viewed tiled and at 1:1: no text, logos, signage or people.

### Final sets

29 sets: 16 kept and 13 new. Sizes are metres per tile; "est." means not published.

| Class | Asset id (source) | Status | m / tile | Maps | Mean linear RGB | Y | Use |
|---|---|---|---|---|---|---|---|
| A | concrete_floor_worn_001 (PH) | NEW | 3.0 | diff, nor_gl, rough, ao, disp | 0.093, 0.095, 0.089 | 0.094 | skin slice 0; `Concrete`; raw-office walls |
| A/F | Metal046B (aCG) | NEW | 2.0 est. | Color, NormalGL/DX, Roughness, Displacement, Metalness | 0.051, 0.051, 0.051 | 0.051 | skin slice 2; `SteelPainted`; vending, doors |
| A | rectangular_facade_tiles (PH) | NEW | 2.0 | diff, nor_gl, rough, ao, disp | 0.098, 0.093, 0.079 | 0.093 | skin slice 6 (S4) |
| A/B | concrete_layers (PH) | kept | 1.55 | diff, nor_gl, rough, ao, disp | 0.153, 0.131, 0.114 | 0.135 | skin slice 1; retaining walls, terrace edges |
| B | concrete_wall_006 (PH) | kept, step 4b | 2.0 | diff, nor_gl, rough, ao, disp | 0.108, 0.084, 0.062 | 0.087 | alley walls, podium bases, bridge undersides |
| B | Leaking003 (aCG) | kept, mask only | 2.0 x 1.0 est. | Opacity, Roughness, NormalGL (Color unused) | - | - | leak overlay (OverlayArr 0) |
| C/A | brick_wall_08 (PH) | NEW | 1.9 | diff, nor_gl, rough, ao, disp | 0.151, 0.135, 0.127 | 0.138 | skin slice 7; Old Quarter brick |
| C/A/R | plastered_wall_04 (PH) | NEW | 3.2 | diff, nor_gl, rough, ao, disp | 0.266, 0.255, 0.245 | 0.257 | skin slice 8; plaster; room walls |
| D | asphalt_02 (PH) | kept, step 4b | 3.0 | diff, nor_gl, rough, ao, disp | 0.116, 0.114, 0.102 | 0.114 | alley floor (hex with rotation) |
| D | Asphalt020L (aCG) | kept | 4.6 | Color, NormalGL/DX, Roughness, AO, Displacement | 0.157, 0.160, 0.147 | 0.159 | boulevard lanes, wide decks |
| E/A | granite_tile (PH) | kept, 0.46 m slab | 2.3 | diff, nor_gl, rough, ao, disp | 0.077, 0.078, 0.078 | 0.078 | plaza; skin slice 9; podium cladding |
| E/R | large_floor_tiles_02 (PH) | kept | 3.0 | diff, nor_gl, rough, ao, disp | 0.186, 0.189, 0.177 | 0.188 | sidewalk slabs, garden path; RoomArr 5 |
| E | concrete_tiles_02 (PH) | NEW | 1.825 | diff, nor_gl, rough, ao, disp | 0.139, 0.126, 0.110 | 0.127 | boulevard, promenade and tongue pavers |
| F | DiamondPlate008C (aCG) | NEW | 0.6 est. | Color, NormalGL/DX, Roughness, AO, Displacement, Metalness | 0.149, 0.153, 0.167 | 0.153 | treads, hatches, deck plates |
| F | MetalWalkway006 (aCG) | kept | 1.0 est. | Color, NormalGL/DX, Roughness, Displacement, Metalness, Opacity | 0.233, 0.237, 0.281 | 0.240 | catwalks, fire escapes, bridge screens |
| F | MetalPlates013 (aCG) | kept | 1.6 | Color, NormalGL/DX, Roughness, AO, Displacement, Metalness | 0.106, 0.090, 0.086 | 0.093 | girders, sign frames, `SteelBare`; rust-bleed source |
| F/A | corrugated_iron_02 (PH) | kept | 2.7 | diff, nor_gl, rough, ao, disp, metal | 0.100, 0.098, 0.083 | 0.097 | skin slice 3; shacks, sheds |
| G | SurfaceImperfections003 (aCG) | kept | 1.5 est. | Color, NormalGL/DX, Opacity | - | - | glass grime (FacadeArr 11) |
| G | Leaking012A (aCG) | kept | 1.0 est. | Roughness, NormalGL | - | - | wet rivulets on glass and paint |
| H | trident_maple_bark (PH) | NEW | 1.8 | diff, nor_gl, rough, ao, disp | 0.078, 0.048, 0.025 | 0.053 | trunks, branches |
| H | LeafSet024 (aCG) | kept | 0.40 | Color, NormalGL/DX, Roughness, Displacement, Opacity | 0.179, 0.307, 0.062 | 0.262 | leaf cards |
| H | LeafSet029 (aCG) | kept | 0.25 | + Scattering | 0.178, 0.318, 0.008 | 0.266 | ivy and vine cards (2 leaves masked) |
| H | Moss001 (aCG) | kept, at 0.45 m | 0.45 | Color, NormalGL/DX, Roughness, AO, Displacement | 0.260, 0.255, 0.139 | 0.248 | garden beds, planter bases |
| I | fine_grained_wood (PH) | NEW | 0.6 | col, nor_gl, rough, ao | 0.055, 0.031, 0.016 | 0.035 | noodle bar counter and stools, door, goal-window frame |
| I/R | poly_wool_herringbone (PH) | NEW | 0.27 | diff, nor_gl, rough, ao, disp | 0.195, 0.182, 0.168 | 0.184 | awnings, noren; RoomArr 7 |
| I | long_white_tiles (PH) | kept | 1.27 | diff, nor_gl, rough, ao, disp | 0.623, 0.581, 0.488 | 0.583 | noodle bar interior; light tile variant |
| R | plank_flooring_04 (PH) | NEW | 2.0 | diff, nor_gl, rough, ao, disp | 0.056, 0.019, 0.009 | 0.026 | RoomArr 3 |
| R | Carpet016 (aCG) | NEW | 1.7 | Color, NormalGL/DX, Roughness, AO, Displacement | 0.502, 0.437, 0.295 | 0.440 | RoomArr 4 |
| R | OfficeCeiling001 (aCG) | NEW | 3.85 | Color, NormalGL/DX, Roughness, AO, Displacement | 0.909, 0.909, 0.909 | 0.909 | RoomArr 6 |

**Not to be processed:** Facade018A, Facade018B and Facade019A (reference only); Facade002, concrete_panels, blue_metal_plate, brick_wall_001, damaged_plaster, metal_plate, bark_brown_02, wood_table_001 and Fabric045 (rejected). Their raw directories stay on disk as provenance.

**Brightness.** As TEX noted, every set is brighter than the dark Look v2 albedos. The colour rule sets the mean from the token, so only the variation from these images reaches the screen.

**Review scripts** (`ExternalTextures/tmp/texr/`, git-ignored, all run with `python -I`):
- `review.py`: sheets and metrics.
- `tokencheck.py`: violet after white balance.
- `bakecheck.py`: visual bake check.
- `fetch_previews.py`: candidate previews.
- `dl2.py`: downloads; log in `download_log.json`.
- `contact_v2.py`: the contact sheet, adapted from TEX's `stats_sheet.py`.
