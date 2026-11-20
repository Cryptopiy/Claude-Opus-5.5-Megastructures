# TEXTURE_PLAN - textured materials for Look v2 (TECH, final, 2026-11-14)

**Status.** Final. TECH wrote the draft (commit ffd8bda). TECH-C reviewed it against the code. TECH-F applied that review and reconciled the plan with TEX-R's review of the texture sets (commit 72ade9d). Appendix A logs every review amendment: applied, corrected or rejected, each with its reason. Appendix B lists the changes that TEX-R forced.

**Authority.** User directive D-027 (2026-11-14):
- the palette must be darker, with no violet;
- every building, window and surface needs real texture;
- CC0 image downloads are allowed, textures only;
- the visuals must be perfect.

**Scope.**
- This plan covers the technical side: formats, pipeline, shader design, work packages, budgets, risks and tests.
- Colour values belong to `docs/LOOK_V2.md` (ART): tokens, window temperatures, haze, luma targets. Where this plan needs a colour or a light level, it cites the LOOK_V2 section.
- The choice of texture sets, their provenance and their measured sizes belong to `docs/TEXTURES.md`. Its "Final sets" table (TEX-R) is the authority on which sets are used and how many metres one tile covers.

**Inputs read.**
- `STATE.md`; `DEVELOPMENT.md` (D-003, D-004, D-005, D-008, D-012, D-017, D-018, D-027).
- MASTER_PROMPT 8.6, 8.8, 8.11, 14.2, 14.7 (H1, H2, H4) and 14.10.
- `docs/MATERIALS.md`, `docs/LOOK_V2.md` and `docs/TEXTURES.md`, including TEX-R's review and its "Final sets" table.
- `QA/critique-lookv2-baseline.md` (CRIT0, ranked issues #1-#30).
- Every Noc shader and include.
- The editor scripts, and the runtime city, geometry, rendering and QA code.
- Every NocFacade vertex writer (`CityMesher`, `FacadeKit`, `G4Kit`, `EdgeMesh.FacadeWriter`, `GulfFace`, `TonguePier`).
- Every `lib.` and `new Material` use in `Assets/_Game/Beats`.
- The tests.
- The raw sets under `ExternalTextures/raw/`.

**Conventions.**
- Pixel numbers assume 1080p, vFOV 66 deg and frontal incidence.
- The footprint is k = 2 tan(33 deg) / H, which is 1.2026e-3 m/px per metre of distance at 1080p and 6.013e-4 at 4K. So one metre covers 27.7 px at 30 m, 2.77 px at 300 m and 0.277 px at 3 km (double at 4K).
- [computed] marks numbers I derived.
- Code paths are relative to `Assets/_Game/` unless they start with `Tools/`, `docs/`, `QA/` or `ProjectSettings/`.

---

## 0. Decisions in one page

1. **Texture set format.** Each material has three maps.
   - **Albedo** is a variation map: sRGB, with a per-channel mean of exactly 0.5 linear. The LOOK_V2 token stays the mean colour (the colour rule of `docs/MATERIALS.md`).
   - **Normal** uses the GL convention.
   - **Mask** is RGBA: R metallic (from the map only for four allow-listed bare-metal sets, otherwise 0), G AO, B height, A absolute perceptual smoothness. Toksvig is baked into every mip of the mask.
   - Compression is BC7 for albedo and mask and BC5 for normals. Near sets are 2K. Facade skin slices are 1K, which is 320-680 px/m against the 32-64 px/m that towers need.
2. **Pipeline.**
   - `Tools/texprep.py` (`python -I`) turns the TEX-R final sets into processed maps plus a `.texmeta.json` per set.
   - De-lighting is off by default. texprep removes baked light only where it is measured: the bake fit against the normal map that TEX-R proposed, plus an opt-in repeat flattening.
   - texprep normalises contrast to LOOK_V2 5.1 and chroma to LOOK_V2 5.4.3.
3. **One sampling library, `NocTexturing.hlsl`.** It provides:
   - hex tiling for unstructured sets;
   - module shuffle for gridded sets;
   - a **cell-aligned panel shuffle** for facade skins (1 tap, seam inside the analytic joint);
   - height-aware hex;
   - triplanar or UV0-metre mapping, all normals as surface gradients;
   - a per-pixel facade frame with a measured orientation sign sigma;
   - a fade to the texture's last mip.
4. **Facades: option (a).** The window field stays procedural and analytic (H2, D-004, D-017). On top of it:
   - five skins chosen per tower and per region;
   - LOOK_V2's window light (1.6 x 2.2 m windows, lit fraction 0.26, log-normal brightness, curtains and blinds);
   - floors and lit-floor zones keyed per tower, so floor lines continue around corners;
   - only whole bays are drawn;
   - a macro layer that still reads at 3 km;
   - exact recess rectangles with room-light spill and wet sills;
   - glass reflecting an irregular skyline, or the registered wall planes inside the canyon and the Gulf;
   - box-room interiors as the main lit-window look; textured rooms only under about 50 m, and first in the cut ladder.
5. **Near surfaces.**
   - World triplanar, planar xz on floors, and UV0 metres for bark.
   - A hex, module or height-hex anti-tiling mode per class, macro noise, leaks anchored to the top edge of each face, and micro detail.
   - Wetness: puddles fill from the height map and puddle normals stay flat (H4).
   - New rows: `ConcreteAlley`, `Brick`, `Plaster`, `Wood`, `TileWall`, `Fabric`, `Grating`, `GlassDark`, `FloorMoss`. A shopfront shader puts lit rooms behind shop glass.
6. **Foliage.** Leaf-cluster cards at true leaf size on two shells around a shrunk core, with alpha-to-coverage under MSAA 4x. At most 135 k cards and 270 k triangles in 17 draws.
7. **Geometry.** GE is a scheduled and owned package: T0 relief within 150 m, crowns below the cloud base, Gulf ribs, the spire. The koi has its own owner (KO).
8. **Packages.**
   - PA (palette and atmosphere, the user's first complaint) and KO start now. TP-0 runs in parallel with them.
   - After TP-0: TP-1, FA, SU, FO and GE run in parallel.
   - TP-2 is the cleanup.
   - Every new path sits behind a uniform feature flag (`-facade-v`, `-surf-v`, `-foliage-v`), so a regression can be traced to its package.
   - Packages iterate with `Tools/lookdev.ps1 -Slot`, debug views, a facade lookdev sheet and GPU parity tests.
9. **Budgets.**
   - Texture VRAM: about 405 MiB.
   - RTX 5070 at 1080p: 90 fps average, no frame above 33 ms, per-pass sum 8.2 ms or less.
   - 4K: 60 fps soft, per-pass sum 16.5 ms or less.
   - Measure overdraw before the facade work. If it exceeds 1.5 on the measured key shots, add a facade depth prepass.

---

## 1. Texture sets, pipeline and sampling

### 1.1 Maps per material

| Map | Content | Colour space | Format | Notes |
|---|---|---|---|---|
| `_Albedo` | Albedo **variation**: each channel normalised to a mean of exactly 0.5 linear; luminance contrast (std / mean) set to the LOOK_V2 5.1 class value (1.4 step 5). | sRGB | BC7 sRGB | `_BaseColor` (a LOOK_V2 token) is the mean, so albedo = token x 2 x var. A texture can never shift a beat's mean luma. Leaves: alpha is the opacity (linear; BC7 alpha is never sRGB-decoded). |
| `_Normal` | Tangent-space normal, GL convention (+Y = +v). The class normal strength is baked in. | linear | BC5 (`UnpackNormalScale` rebuilds z) | `_NormalStrength` stays 1 for photo rows, so the baked Toksvig term stays consistent. |
| `_Mask` | R metallic (1.4 step 6 allow-list, else 0); G AO; B height (p1..p99 to 0..1); A **absolute** perceptual smoothness (class range from texprep, Toksvig per mip on import). | linear | BC7 | `_SmoothMin` and `_SmoothMax` are 0 and 1 for photo rows. Wetness offsets smoothness in the shader. |

### 1.2 Arrays, resolution and VRAM

Arrays hold layer families that one material switches between per pixel. Every slice of an array is square, the same size and the same format (a Unity constraint, asserted by TextureTests).

| Array | Slices | Size | Flipbook (cols x rows) | Used by |
|---|---|---|---|---|
| `FacadeArr_{Albedo,Normal,Mask}` | 8 skin slices (1.3) | 1024 | 4 x 2 (4096 x 2048) | `NocFacade` |
| `RoomArr_{Albedo,Mask}` | 8 room surfaces (1.3) | 512 | 4 x 2 | `NocInterior` (near band only) |
| `OverlayArr_{Mask,Normal}` | 4: leaks, glass grime, rivulets, rust bleed | 2048 | 2 x 2 | `NocSurface`, `NocFacade`, `NocGlass` |
| `MicroArr_{Albedo,Normal}` | 4: concrete pores, stone grain, metal scratches, asphalt grit (high-pass crops; albedo modifier mean exactly 1) | 1024 | 2 x 2 | `NocSurface`, `NocWet` |

A BC7 or BC5 map with mips takes 5.33 MiB at 2K, 1.33 MiB at 1K and 0.33 MiB at 512 [computed].

| Class | Res | Count | VRAM (MiB) |
|---|---|---|---|
| Facade skin array | 1024 | 8 slices x 3 maps | 32 |
| Room array | 512 | 8 x 2 | 5 |
| Overlay array | 2048 | 4 x 2 | 43 |
| Micro array | 1024 | 4 x 2 | 11 |
| Near sets (1.3, 15 sets) | 2048 | 15 x 3 | 240 |
| Small sets (wood, wall tiles, fabric) | 1024 | 3 x 3 | 12 |
| Leaf cluster atlases (LeafSet024 crowns, LeafSet029 ivy) | 2048 | 2 x 3 | 32 |
| NocTexRecipes families still referenced until TP-2 | 512-1024 | up to 8 x 3 | 30 |
| **Total** | | | **about 405** (plan cap 900, hard cap 2 048) |

**How far a 2K tile stays sharp** [computed]:
- d(1:1) is the distance at which one texel covers one pixel. Nearer than that, the texture is magnified.
- d(mean) is the distance at which the whole tile covers 4 px and the shader fades to the tile mean (1.6).

| Tile N (m) | Texel (mm) | d(1:1) 1080p | d(1:1) 4K | d(mean) 1080p | d(mean) 4K |
|---|---|---|---|---|---|
| 0.45 (moss) | 0.22 | 0.18 m | 0.37 m | 94 m | 187 m |
| 0.6 (tread plate) | 0.29 | 0.24 m | 0.49 m | 125 m | 250 m |
| 1.825 (pavers) | 0.89 | 0.74 m | 1.48 m | 379 m | 759 m |
| 2.0 | 0.98 | 0.81 m | 1.62 m | 416 m | 832 m |
| 3.0 | 1.47 | 1.22 m | 2.44 m | 624 m | 1 247 m |
| 4.6 (Asphalt020L) | 2.25 | 1.87 m | 3.74 m | 956 m | 1 913 m |

Near sets stay at 2K plus the micro layer. 4K versions of the 15 near sets would add about 720 MiB [computed], and TEX downloaded only 2K. The micro layer already gives that detail within 1-2 m.

**Facade texel density.** A 1K slice over a 1.55-3.2 m tile gives 320-660 px/m. LOOK_V2 5.4.6 asks for 32-64 px/m on towers, and the nearest tower face seen from the walk (more than 30 m away) needs at most 55 px/m at 4K [computed].

### 1.3 Sets, tiles and anti-tiling per row

The source of truth is the "Final sets" table in `docs/TEXTURES.md` (TEX-R).
- **Tile rule:** a row's tile equals the TEXTURES.md value within +-15 %. The only larger deviations allowed are the facade lattice registrations listed below. TextureTests asserts the rule from the texmeta.
- **Not processed:** Facade018A, Facade018B and Facade019A (reference only), and the nine sets TEX-R rejected.

**Library rows** (NocSurface unless marked NocWet or another shader):

| Row | Set | Tile (m) | Module (m) | Anti-tiling | Rotation | Metallic | Res |
|---|---|---|---|---|---|---|---|
| `Concrete` (planters, columns, tunnel, slabs, gate blocks) | concrete_floor_worn_001 | 3.0 | - | hex r 0.70 | yes | 0 | 2K |
| `ConcreteAlley` **new** (alley walls) | concrete_wall_006 (bake fit) | 2.0 | - | hex r 0.70 | **no** (vertical stains) | 0 | 2K |
| `StoneWall` (podia, arcade walls, kerbs, benches) | granite_tile | 2.3 | 0.46 x 0.46 stack bond | module, tone +-6 % | - | 0 | 2K |
| `SteelPainted` (truss, gates, lamp housings, bollards, vending bodies) | Metal046B | 2.0 | - | hex r 0.70 | yes | **0** (paint; the 86 % metal map is ignored) | 2K |
| `SteelBare`, `Brass` | MetalPlates013 | 1.6 | - | hex, offsets only | no | map | 2K |
| `Moss` (hedges, ground cover) | Moss001 | 0.45 | - | height hex r 0.75 | yes | 0 | 2K |
| `Bark` (trunks, branches) | trident_maple_bark | 1.8 | - | UV0 metres (`_Mapping` 1), hex offsets only | no | 0 | 2K |
| `Asphalt` (lanes, kerbs, patches) | Asphalt020L | 4.6 | - | hex r 0.70 | yes | 0 | 2K |
| `Brick` **new** (Old Quarter tenement) | brick_wall_08 | 1.9 | course 0.093, running bond (texprep measures the brick length) | module | - | 0 | 2K |
| `Plaster` **new** (Old Quarter neighbours, noodle bar exterior) | plastered_wall_04 (repeat flattening) | 3.2 | - | hex r 0.70 | yes | 0 | 2K |
| `Grating` **new** (catwalk, fire escapes; `NocGrating`) | MetalWalkway006 | 1.0 | 0.0625 mesh | none (regular; fades to the mean coverage 0.35) | - | map | 2K |
| `Wood` **new** (noodle-bar counter and stools, tenement door, goal-window frame) | fine_grained_wood | 0.6 | - | hex, offsets only, grain along the long axis | no | 0 | 1K |
| `TileWall` **new** (noodle-bar back wall) | long_white_tiles | 1.27 | 0.0385 x 0.127 | module | - | 0 | 1K |
| `Fabric` **new** (noren, awnings, goal curtain) | poly_wool_herringbone | 0.27 | - | hex, offsets only | no | 0 | 1K |
| `GlassDark` **new** (opaque dark panes, Old Quarter) | none (untextured) | - | - | - | - | 0 | - |
| `GlassBack`, `GlassFront` (NocGlass) | OverlayArr 1 grime, 2 rivulets | 1.5 / 1.0 | - | hex, offsets only | no | 0 | - |
| `FloorAlley` (NocWet) | asphalt_02 (bake fit) | 3.0 | - | hex r 0.70 (its 3 m crack must never plain-tile) | yes | 0 | 2K |
| `FloorStone`, `GrayFloor` (NocWet; boulevard, promenade, tongue) | concrete_tiles_02 | 1.825 | 0.305 x 0.61, half bond, long side along v | module | - | 0 | 2K |
| `FloorSteelDeck` (NocWet; skybridge) | DiamondPlate008C | 0.6 | analytic plates 1.2 x 2.4 with weld seams | hex, offsets only, inside each plate | no | map | 2K |
| `FloorGardenPath` (NocWet) | large_floor_tiles_02 | 3.0 | 1.0 x 1.0 | module, moss tint in the joints | - | 0 | 2K |
| `FloorPlaza` (NocWet) | granite_tile | 2.3 | 0.46 x 0.46 stack bond | module; tone +-8 %, polish +-0.05 | - | 0 | 2K |
| `FloorMoss` **new** (NocWet; garden ground) | Moss001 | 0.45 | - | height hex | yes | 0 | 2K |
| `Line` (NocLine) | none: an analytic 1D paint variation along the axis (3.5) | - | - | - | - | - | - |

**Facade skin slices** (`FacadeArr`, 1024). Every slice is joint-free, or its joints are registered to the analytic grid (2.5).

| Slice | Set | Source tile (m) | Registered to | Notes |
|---|---|---|---|---|
| 0 precast | concrete_floor_worn_001 | 3.0 | - (joint-free) | S1 walls, spandrels, roofs in mode 2 |
| 1 weathered concrete | concrete_layers | 1.55 | a whole number of pour lifts per 3.6 m floor | S1 variation; section-mode cut floors |
| 2 metal panel | Metal046B | 2.0 | - | S2 panels, frames (metallic 0) |
| 3 ribbed metal | corrugated_iron_02 | 2.7 | rib count per 1.2 m mullion module | S2 variation, offsets only (ribs vertical) |
| 4 facade tile | rectangular_facade_tiles | 2.0 | 24 courses per 3.6 m floor (0.150 m; source 0.154) | S4; offset-only hex inside panels (tile widths are irregular) |
| 5 brick | brick_wall_08 | 1.9 | 39 courses per 3.6 m floor (0.0923 m; source 0.093) | S3 |
| 6 plaster | plastered_wall_04 | 3.2 | - | S3 and S4 bands |
| 7 stone | granite_tile | 2.3 | 5 x 8 slabs per 2.4 x 3.6 m bay (0.48 x 0.45 m; source 0.46) | base bands, lobbies, S3 sills and lintels |

There are no photo slices for anodised aluminium, spandrel glass or louvres (TEX-R):
- **Anodised frames and mullion caps** are a constant: albedo from LOOK_V2's MetalPaint token, smoothness 0.55. They fall under 4 px beyond about 15 m.
- **Spandrel glass** is dark glass plus OverlayArr 1 grime.
- **Louvres** are analytic blades over slice 2.

**Overlay, micro and room arrays.**

| Array slice | Set and maps used |
|---|---|
| OverlayArr 0 leaks | Leaking003: Opacity, Roughness, NormalGL (its Color is unused). The 2048 x 1024 decal is **padded** to 2048 x 2048 with opacity 0 in the pad, never stretched. |
| OverlayArr 1 glass grime | SurfaceImperfections003: Opacity, NormalGL |
| OverlayArr 2 rivulets | Leaking012A: Roughness, NormalGL only |
| OverlayArr 3 rust bleed | MetalPlates013: coverage = 1 - Metalness (the rust runs), NormalGL |
| MicroArr 0-3 | high-pass crops of concrete_floor_worn_001, granite_tile, Metal046B, asphalt_02 (period about 0.5 m) |
| RoomArr 0-2 walls | plastered_wall_04, long_white_tiles, concrete_floor_worn_001 |
| RoomArr 3-5 floors | plank_flooring_04, Carpet016, large_floor_tiles_02 |
| RoomArr 6 ceiling | OfficeCeiling001: Color, AO, NormalGL, Roughness (Emission and Metalness ignored) |
| RoomArr 7 curtain | poly_wool_herringbone |

### 1.4 `Tools/texprep.py` (run with `python -I`; TP owns the script)

Command line: `python -I Tools/texprep.py [--only <Key>] [--check] [--previews] [--placeholders]`. There is one config per owning package:
- `Tools/texprep/tp_common.json` (TP: overlay and micro arrays);
- `facade.json` (FA: FacadeArr, RoomArr);
- `surface.json` (SU);
- `foliage.json` (FO).

No config file is shared.

1. **Inputs and safety.**
   - texprep opens only files under `ExternalTextures/raw/` whose real path stays inside it, whose extension is `.png`, `.jpg` or `.jpeg`, and which PIL reports as PNG or JPEG.
   - It sets `Image.MAX_IMAGE_PIXELS = 8192*8192`.
   - It never executes or imports anything from raw.
   - Every config key must name a set in the TEX-R final-set list; any other set aborts the run.
   - The SHA-256 of every input goes into the texmeta.
2. **Decode to float32 linear.**
   - Albedo goes from sRGB to linear. Normals go from [0, 1] to [-1, 1]; GL sources only.
   - **Sign check:** where a displacement map exists, the correlation of n.x with -dh/du must be positive, or the run aborts.
   - Roughness, AO, height and metal are linear; 16-bit PNG is supported.
   - fine_grained_wood has no displacement map. Its height comes from integrating the normal map (Poisson, periodic).
3. **Crop, register, resize and make seamless.**
   - The default is no crop: the final sets tile at their full extent.
   - **Lattice registration** applies to gridded near sets (granite_tile, concrete_tiles_02, large_floor_tiles_02, long_white_tiles, brick_wall_08). texprep measures the joint period by FFT of the AO or height, then resamples so the texture holds a whole number of modules on both axes, with module boundaries in the middle of the joints.
   - **Facade registration** (1.3 slice table) resamples courses, lifts and slabs to whole numbers per 3.6 m floor and per 2.4 m bay. The scale change is 7 % or less.
   - Resizing uses Lanczos in linear light per channel. Normals are resized as vectors and renormalised.
   - **Squaring:** Leaking003 is padded to square. poly_wool_herringbone (2048 x 2091) is resampled to square, a 2 % change.
   - `seamless: true` is only for sources that fail the seam check: a half-offset with a 12 % feathered cross-fade.
   - `seamScore` must be 1.25 or less, except where TEX-R measured the seam as a real joint (OfficeCeiling001, plank_flooring_04, concrete_floor_worn_001).
4. **Removing baked light.** The default strength is 0. Stains and streaks at 1-10 m are the only texture that survives at distance (LOOK_V2 5.3), so they are kept.
   - **4a. Bake fit** (TEX-R finding 2). Fit the high-pass luminance (sigma 6 px) to the high-passed normal x and y by least squares, then subtract the fitted part.
     - It applies only to sets whose measured bake is above 0.10: concrete_wall_006 (0.40) and asphalt_02 (0.32).
     - Gate: bake 0.10 or less afterwards.
   - **4b. Ramp.** Fit a plane to the sigma = N/8 periodic low-pass luminance. Remove it only if its slope exceeds 3 % per tile. No final set needs this today; the step is a guard.
   - **4c. Repeat flattening** is opt-in per set in the config, at a strength of 0.6 or less. It is allowed only where TEX-R measured a repeating blob (blob 0.15 or more at 3 x 3): today only plastered_wall_04 (0.19).
   - **AO.** If an AO map exists, divide the albedo by lerp(1, AO, 0.5). The shader re-applies AO from the mask, so cavities are not darkened twice.
   - **Content gate.** The std of the 1 m low-pass luminance after processing is at least 0.6 x its value before, unless 4c is configured.
5. **Colour and contrast (LOOK_V2 5.1 and 5.4).**
   - Record the absolute processed mean in the texmeta, for PA and ART.
   - Normalise each channel to a mean of 0.5 (the colour rule; this removes casts).
   - **Contrast.** Scale deviations around the mean so the luminance std / mean equals the LOOK_V2 5.1 class midpoint: concrete 0.32, plaster 0.27, brick 0.45, asphalt 0.37, granite 0.27, painted steel 0.30, bark 0.50, leaves 0.40, moss 0.40. Clamp to [0.01, 0.99] and renormalise the mean.
   - **Chroma.** Measured on the map multiplied by its class token in sRGB:
     - mean HSV saturation 0.12 or less for concrete, render, granite, asphalt and metal, and 0.45 or less for brick, bark, leaf and moss;
     - every pixel with hue 260-330 deg and saturation above 0.15 is desaturated to 0;
     - TEX-R's violet-after-white-balance share must be 0.5 % or less.
6. **Pack the mask.**
   - **R metallic** is taken from the map only for MetalPlates013, MetalWalkway006, corrugated_iron_02 and DiamondPlate008C. Every other row is 0, including Metal046B (paint and cladding, although its map is 86 % white) and any `Metalness` that is really a glass mask.
   - **G AO** comes from the AO map, or else from the height cavity: AO = saturate(1 - k(blur(h) - h)).
   - **B height** maps p1 to 0 and p99 to 1.
   - **A smoothness** = 1 - roughness, with p5..p95 mapped to the class's dry range from LOOK_V2 5.1.
7. **Leaves (FO config).**
   - Compose four cluster sprites per atlas from the atlas leaves at true leaf size: LeafSet024's 9 leaves; LeafSet029's 7, with the two autumn leaves masked.
   - Opacity goes to alpha. Colour bleeds into transparent texels by push-pull dilation. Alpha stays straight.
8. **Micro detail.** A high-pass of a crop of the parent set (period about 0.5 m), normalised so the albedo modifier has a mean of exactly 1 and the normal is flat on average.
9. **Arrays.** Slices are processed exactly like sets, then laid into the flipbook of 1.2.
10. **Write.**
    - Deterministic PNGs (`optimize=True`).
    - Previews go to `ExternalTextures/previews/` (git-ignored): a 3 x 3 tiling, a 1:1 crop and the seam crop.
    - The texmeta records tile, module, anti-tiling, rotation, `metalFromMap`, the measured stats (mean, lf, bake, std / mean, saturation, violet share, smoothness p5/p50/p95, height p1/p99, seamScore) and the import hints (mip filter, aniso).
    - `--check` exits non-zero if an output is stale against its input hashes.
    - `--placeholders` writes 64 px neutral sets, so materials build before TP-1.

**Content rule (MASTER_PROMPT Part 6 item 5).** No legible text, logos, signage, graffiti letters, faces or people. TEX-R recorded the review; the previews keep it checkable.

### 1.5 Import settings (`Scripts/Editor/NocTextureImport.cs`, an AssetPostprocessor scoped to `Assets/_Game/Textures/External/`)

**`OnPreprocessTexture`**, by file suffix:

| Suffix | Settings |
|---|---|
| `_Albedo` | Default type, sRGB on, Standalone BC7, `compressionQuality` 100. Mip filter: Kaiser for unstructured sets, **Box** for gridded sets and leaves. |
| `_Normal` | NormalMap type, Standalone BC5, Box mips. |
| `_Mask` | sRGB off, alpha from input, BC7, Box mips. |

Every texture also gets:
- `mipmapEnabled` true, trilinear filtering, Repeat wrap;
- `anisoLevel` 16 for the facade, paving, asphalt and deck classes and the arrays, 8 for the rest;
- `mipMapBias` 0, `streamingMipmaps` false, `isReadable` false;
- `maxTextureSize` = the manifest resolution, or 8192 for flipbook sources.

Leaves add `mipMapsPreserveCoverage = true` and `alphaTestReferenceValue = 0.5`. Flipbook sources get `textureShape = Texture2DArray` and the flipbook rows and columns of 1.2.

**Anisotropy.**
- `ProjectSettings/QualitySettings.asset:24` sets `anisotropicTextures: 2` (ForceEnable); no configurator script writes it.
- Under ForceEnable, Unity's forced minimum is 9, so an `anisoLevel` of 8 runs as 9 and 16 stays 16.
- TextureTests expects the effective level max(9, anisoLevel). This plan does not change the quality asset.

**Normal-map arrays (spike in TP-0).** First check whether a NormalMap-type flipbook imports as a `Texture2DArray` in 6000.6. If it does not, the fallback is Default type, linear, BC5, with the shader decoding `UnpackNormalRG`.

**Toksvig, in `OnPostprocessTexture` and `OnPostprocessTexture2DArray` for `_Mask` files.**
1. In `OnPreprocessTexture`, call `context.DependsOnSourceAsset(<sibling _Normal path>)`, so a changed normal re-imports the mask and no smoothness mip goes stale.
2. Read the sibling `_Normal` PNG from disk; import order does not matter.
3. For mip L, average the mip-0 unit normals over 2^L blocks with wrap, giving |n_bar|.
4. Compute kappa = (3|n| - |n|^3) / (1 - |n|^2) and variance = 1 / (2 kappa), as URP's `TextureNormalVariance` does.
5. Set alpha'^2 = alpha^2 + min(2 variance, 0.20^2), with alpha = (1 - s)^2, and write s' = 1 - sqrt(alpha').
6. Bump `GetVersion()` on every change to the formula.

Mip 0 is unchanged, and smoothness never increases with the mip level.

**TextureTests read the imported asset, not the PNG.** The last mip's linear mean is read through a blit of that mip into a small RenderTexture, then `ReadPixels`.

### 1.6 Sampling library `Shaders/Include/NocTexturing.hlsl` (TP)

The include holds functions only and declares no CBUFFER, so each shader keeps its own SRP Batcher layout.

**Hex tiling** (Mikkelsen 2022):
- `NocHexSetup(uv, dUVdx, dUVdy, rate, rotate, seed)`, then `NocHexColour`, `NocHexDeriv` and `NocHexMask`, plus `_ARRAY` variants.
- All taps use `SampleGrad` with the true gradients.
- gamma 7 and beta 0.6; S-curve r 0.70 for colour and smoothness, 0.5 (off) for normals.
- Rate 1.25 hexes per tile. A tap whose weight is below 0.02 is skipped.
- Rotation only where the texmeta sets `rotate`.

**Height-aware hex.** w_i' = w_i (h_i + 0.05)^8, normalised and shared by all maps. Used for moss and ground.

**Module shuffle** (gridded near sets):
- Module id: (i, j) = floor(uvMetres / m), with the row shift by j parity.
- Source module: hash(i, j, seed) mod (Mx, My), keeping parity for running bond.
- `uvTex = (uvMetres - (i, j) m + (si, sj) m) / tile`, sampled with `SampleGrad` using the continuous gradients of uvMetres / tile.
- Seams lie in the middle of the joints.
- Per-module tone +-6 % and smoothness +-0.04, faded to their means by smoothstep(2, 4, module_px).

**Cell-aligned panel shuffle** (facade skins, 2.5):
- The panel is the analytic panel of the skin: one bay x one floor, or the spandrel and pier zones of that bay.
- `uvTex = (cellLocalMetres + offset(hash(panel))) / tile`: one tap per map, `SampleGrad` with the continuous gradients of (u, v) / tile.
- The seam between two panels lies inside the analytic panel joint, so no photographed joint ever meets an analytic one.
- The same hash gives each panel a tone of +-12 % (2.5). It fades to its mean by smoothstep(2, 4, panel_px).

**Mapping modes** (`_Mapping` on NocSurface):
- **0, world triplanar with surface gradients.**
  - Weights are |n|^4, normalised; planes with a weight of 0.01 or less are skipped with `[branch]`.
  - Each plane's normal sample becomes a height derivative, then a volume gradient by swizzle.
  - The sum is projected to the surface gradient: n' = normalize(n - sg).
- **1, UV0 in metres.**
  - The tangent frame comes from screen derivatives of UV0 and P (Mikkelsen 2020, `GenBasisTB` form), so the mesh needs no tangents.
  - Used for bark, where Loft and TaperedCylinder write metric UV0 (`MeshKit.Loft.cs:42`) and the grain must follow the branch.

**Facade frame `NocFaceFrame(n, u, P)`.** The draft's fixed frame is wrong for faces written by `EdgeMesh.FacadeWriter`:
- On face A, u = `EdgeDims.ArcLength` grows with z (`EdgeDims.cs:188`), but the normal points to +x (`GulfFace.cs:77-82`, RecedeSign -1). So u runs along -T0.
- On the z 275 notch wall, n = -z and u = x (`GulfFace.cs:230-231`).
- On the -z side of the tongue hull, both sides use u = x (`TonguePier.cs:93-102`).
- The tongue hull and the spire have tilted normals.

The frame is therefore built per pixel:
- T0 = (n.z, 0, -n.x); Tp = normalize(T0 - n (n . T0)).
- sigma = sign(ddx(u) dot(ddx(P), Tp) + ddy(u) dot(ddy(P), Tp)), with +1 when the sum is under 1e-12 (an edge-on face is sub-pixel anyway).
- T = sigma Tp: the direction in which u grows.
- **B = normalize(Y - n (n . Y))**: the direction in which v = world y grows.

The review proposed B = cross(n, T). That vector points down on every sigma = -1 face, so this plan uses the projected up direction. The frame need not be right-handed: tangent-space normals and the recess shift are expressed along dP/du and dP/dv, which is correct for either handedness.

**Fade to mean.**
- w_tex = smoothstep(2, 4, tile / fp_box), where fp_box is the bounding-box footprint |ddx| + |ddy| (CRIT0 #19).
- Below the fade, each map returns its last mip through `SampleLevel` (which includes Toksvig), and the normal is flat.
- When w_tex = 0, all taps are skipped. Derivatives are taken before every branch.

**Precision.** Textured world coordinates are used only within 2 km of the origin, where float32 loses under 0.25 texel at 2K [computed]. Facade arrays are sampled only while w_tex > 0.

**Globals** (uniform branches, never keywords; set by TP-0's `NocLookGlobals`):
- `_NocTexDetail` (`-tex 0|1`);
- `_NocDebugView` (`-debug-view N`, section 7);
- `_NocFacadeV`, `_NocSurfV`, `_NocFoliageV` (`-facade-v`, `-surf-v`, `-foliage-v`: 1 = the v1 path, 2 = the new one; the default switches to 2 when each package is accepted).

### 1.7 Anti-tiling: options weighed

| Method | Taps per map | Pre-processing | Look | Verdict |
|---|---|---|---|---|
| Hex tiling (Mikkelsen 2022) | 3 `SampleGrad` (often 1-2) + about 40 ALU | none | slight contrast loss at hex borders (r 0.65-0.75) | **Default for unstructured near sets.** |
| Histogram-preserving (Heitz and Neyret 2018; Deliot and Heitz 2019) | 3 + 3 LUT | Gaussianised texture and LUT per map | best for colour; no normal rotation | Rejected: twice the taps for a gain invisible on dark, low-chroma albedos. |
| Cheap 2-sample (Quilez style) | 2 + noise | none | cross-fade smears | Cut-ladder fallback for overlays only. |
| Module shuffle | 1 | lattice registration | exact (joints stay joints) | **Default for gridded near sets.** |
| Cell-aligned panel shuffle | 1 | facade registration | seams inside analytic joints; per-panel tone | **Facade skins.** |
| None | 1 | - | - | Props that never show two tiles. |

---

## 2. Facades (the main subject)

### 2.1 Options weighed

| Option | For | Against | Verdict |
|---|---|---|---|
| (a) Procedural window field + photo skin layers + analytic structure + interiors | Keeps H2: stateless epochs, zones, the exact 2 x 2 box filter, tier identity and the energy tests (D-004, D-017, D-018). Every new element is an interval in cell coordinates, so it filters exactly. | More shader work (2.12). | **Chosen.** |
| (b) Photographic facade tiles (ambientCG `Facade0xx`) scaled to the cell pitch | Quick realism at 20-80 m. | The lit pattern is baked and repeats over millions of windows. The mask filters only by mips, so the 2.5-5.5 px band shimmers. TEX-R measured the sets as procedural, with an identical 6 x 6 grid per tile and flat glass. | Rejected. TEX-R made 018A, 018B and 019A reference only and rejected 002. |
| (c) Photo tiles near, analytic far | Photo detail near. | Needs (b)'s grid alignment, plus a pop between two window fields at the switch (H2). | Rejected. |

### 2.2 Vertex contract, writers and the skin byte

**Contract** (every NocFacade writer):
- `uv0 = (u metres along the face, 16-bit face id)`;
- `uv1 = (uMin, uMax, y0, y1)`: **new**, y0 and y1 are the face's bottom and top in world y, and (0, 0) means unknown;
- colour R = AO, G = **skin byte**, B = mode (0 facade, 128 section, 255 roof), A = edge kind x 63.

`NocLitPass` reads `float4 uv1`. With `NOC_PASS_UV1ZW` defined, it copies `uv1.zw` to `custom.zw` (TP-0).

| Writer | File (package) | uv1.zw | Skin byte |
|---|---|---|---|
| `CityMesher.AddFace` | `Scripts/Runtime/City/CityMesher.cs` (FA; frozen, needs the lead's approval) | (f.y0, f.y1) | `SkinByte(t.seed, region of the tower centre)` |
| `FacadeKit.Face`, `FacadeKit.Box` | `Beats/Beat02_Boulevard/FacadeKit.cs` (FA) | (y0, y1) arguments | `SkinByte(seed, region of the box centre, or of the face centre for a lone face)` |
| `G4Kit.FacadeQuad`, `G4Kit.FacadeBox` | `Beats/Beat06_Signs/G4Kit.cs` (FA) | both `CurrentUV1` assignments (`G4Kit.cs:127` for the bottom vertices, `:130` for the top) carry (min(a0.y, b0.y), max(a1.y, b1.y)) | as FacadeKit |
| `EdgeMesh.FacadeWriter` (Gulf faces, tongue hull) | `Beats/Beat03_Edge/EdgeMesh.cs:43-60` (not edited) | (0, 0) | G = 0 (`EdgeMesh.cs:48`) |
| `GulfFace.PortalPanel` | `Beats/Beat03_Edge/GulfFace.cs:250` (not edited) | (0, 0) | G = 0 |

**The Gulf writers stay unedited.** EdgeMesh and GulfFace write section mode, or closures hidden by the floors. With y0 = y1 = 0, these features are off on those faces:
- the parapet and setback bands;
- the base band;
- the vertical limit of the whole-bay rule;
- the setback ledge line.

Section-mode slab lines, columns, atria and mechanical bands are periodic in v and need no y0 or y1. The orientation sign sigma (1.6) fixes u's direction on these faces.

**Skin byte.** Every writer calls `NocFacadeMath.SkinByte(uint seed, int region)`:
- region D (default): `seed % 120`, bytes 0x00-0x77;
- region OQ (2.3): `0x78 + seed % 120`, bytes 0x78-0xEF;
- **overrides:** a builder passes `seed = NocFacadeConstants.SkinOverride(k) = 0xFFFFFFF0 + k`, which writes byte 0xF0 + k. A fixed table maps k to a skin and variant.

The shader reads the byte as follows:
- A byte of 0xF0 or more selects the override table.
- Otherwise it selects the region's share table, using U = pcg(byte) / 2^32. PCG runs before the thresholding, so consecutive bytes (StreetFacade's `seed + (k + 1)`, `StreetFacade.cs:137`, gives 0xA2-0xAA) get independent skins. The draft's optional per-slab seed edit is dropped; a test asserts that the boulevard slabs get more than one skin.
- Today's shader never reads colour G, so the encoding changes no current picture.

**G = 0 from EdgeMesh.**
- Modes 1 and 2 ignore the byte: the section skin is fixed (2.10), and the roof uses slice 0.
- The mode-0 closure at the z 275 jog is hidden by the floors. It gets the region-D skin of pcg(0).

**Overrides for hero faces.**

| k | Byte | Skin | Builder (file, package that edits it) |
|---|---|---|---|
| 0 | 0xF0 | S0, dark glass with vertical fins (spire) | `Beats/Beat07_Heart/SpireBuilder.cs` (GE); today the constant 0xA7 (`SpireBuilder.cs:35`) |
| 1 | 0xF1 | S2, photo metal panels with LED-strip housings (screen-wall mass, CRIT0 #22) | `Beats/Beat06_Signs/ScreenWallBuilder.cs` (PA carries the line) |
| 2 | 0xF2 | S1 precast (alley west tower) | `Beats/Beat01_Alley/AlleyBlocks.cs` (SU carries the line) |
| 3 | 0xF3 | S4 tiled with loggias (alley east tower) | `Beats/Beat01_Alley/AlleyBlocks.cs` (SU carries the line) |
| 4 | 0xF4 | S1 precast with a stone base band (boulevard podium faces only; the `seed` used for shop hashes is unchanged) | `Beats/Beat02_Boulevard/StreetFacade.cs` (FA) |
| 5-15 | - | reserved | - |

**Tower key K = pcg(skin byte).**
- K keys the stratum of each band, the zone rows, the lit floor groups, the tower multiplier, the core strip and the mechanical stripes (2.4, 2.5).
- Every face of a tower carries the same byte: CityMesher writes `t.seed` on all faces of a tower, and FacadeKit.Box and G4Kit.FacadeBox write one seed per box. So floor lines, strata and lit floors continue around corners.
- Two towers in one region that share a byte (1 chance in 120 per pair) also share these patterns. Their window states still differ, because those are keyed by face id.
- Tier identity holds (D-018): T2 rebuilds the ring-1 towers with seeds from (ci, cj) (`CityField.cs:124`) through the same CityMesher.

**Contract test** (`FacadeSkinTests`, FA). It runs over every NocFacade vertex of the built world, built as `Q1IntegrationTests` builds it:
- **Per triangle:**
  - u is affine with |grad u| in [0.9, 1.1] (metric; this includes the tilted tongue hull);
  - the C# twin of `NocFaceFrame` returns sigma = +-1, constant within each (mesh, face id).
- **Known orientations:** face A has sigma = -1, the south notch wall -1, the -z side of the hull -1, and city faces +1.
- **Ranges:**
  - uMin - 1e-3 <= u <= uMax + 1e-3;
  - when y1 > y0, also y0 - 1e-3 <= y <= y1 + 1e-3.
  - These checks catch any later merge through `MeshBuilder.Append`, which renormalises UV1.xyz as a tube axis (`MeshBuilder.cs:119-135`; MeshKit tubes need that, `MeshKit.cs:258`). TP-0 adds a warning comment there.
- **Skin byte:** the same on every face of a tower or box.

### 2.3 Skins, regions and the budget

A skin is chosen per **tower** (the skin byte), never per face. A tower with four skins reads wrong.

**Regions** are world discs. The writer picks the region from the tower or box centre, so no tower straddles two regions and nothing pops.
- **OQ (Old Quarter and Heart ring):** centre (1 250, 388), radius 600 m. It covers the tenement at x 1 156, the plaza (R 400 about the spire S (1 330, 388)) and the ring towers.
- **D:** everywhere else.
- Mode-1 Gulf faces have no skin.

| Skin | Share D | Share OQ | Read | Slices | Residential cell 2.4 x 3.6 m | Clear-glass coverage c_k |
|---|---|---|---|---|---|---|
| S0 glass curtain wall | 25 % | 10 % | dark glass grid with spandrel glass and fins | constant anodised caps, overlay 1 grime | slab cap 0.12 m; opaque spandrel glass 1.5 m; vision glass 1.8 m; mullions 0.12 m every 1.2 m; recess 0.05 m | 0.47 |
| S1 precast concrete | 30 % | 25 % | punched windows, panel joints, deep reveals | 0, with 1 on 15 % of panels | slab band 0.35 m; LOOK_V2 window **1.6 x 2.2 m clear glass** at sill 0.9 m; 0.12 m frame outside the glass; a 0.12 m centre mullion on 50 % of windows (opening widened, so the clear area stays 3.52 m^2); recess 0.22 m; 15 mm joints at cell edges | 0.407 |
| S2 metal-clad | 15 % | 5 % | ribbon windows, ribbed panels | 2, 3 | ribbon 1.8 m tall at sill 0.9 m, full bay width with 0.12 m mullions every 1.2 m; recess 0.08 m; panel joints every 1.2 m | 0.45 |
| S3 brick | 5 % | 40 % | masonry, small windows | 5, 6; 7 for sills and lintels | window 1.2 x 1.6 m at sill 0.85 m; recess 0.18 m; 0.12 m stone sill and lintel bands | 0.22 |
| S4 tiled residential with loggias | 25 % | 20 % | balconies on 40 % of bands | 4, 6, 7 | loggia floors: the whole bay recessed 1.4 m, slab edge 0.25 m, rail bars 0.04 m at 1.05 m (glass balustrade on 50 %), sliding doors 2.0 x 2.2 m at the back. Other floors: 1.4 x 1.6 m windows. **No loggias in corner bays** (the first and last whole bay of a face) **or in the bay under the parapet**: a flat quad cannot show a 1.4 m loggia against the sky. | 0.36 |
| **Sum** | 100 % | 100 % | | | | |

**Budget** (frontal, unresolved, residential stratum) [computed]:
- **D:** sum of share x c = 0.408, LOOK_V2's 0.407. Region D therefore reproduces LOOK_V2's unresolved facade mean of 0.022 linear (luma 44-46 before fog).
- **OQ:** sum = 0.331, which is 0.81 x region D's window term. Its mean is about 0.018 linear, about 4 luma codes darker. That fits LOOK_V2 beat 7 (the coolest, darkest city, where the warm window stays unique). ART confirms this (open question 6).
- The commercial, mechanical and lobby strata keep LOOK_V2's geometry in every skin, so they do not move the budget.
- **Window radiance is never scaled per skin.** The draft's f_skin is dropped. The budget is reached through glass coverage only, so LOOK_V2's per-window brightness normalisation holds.
- **Test:** FacadeTests asserts, from the C# twin, that each region's share-weighted mean radiance (lit glass + unlit glass + wall x ambient) equals its target within 2 %.

**Other strata.**
- **Commercial (4.8 x 4.32):** curtain or ribbon glazing on S0, S2 and S4; large punched windows on S1 and S3.
- **Mechanical (9 x 6.17):** a louvre band of analytic blades at 0.15 m pitch over slice 2.
- **Sky lobby (18 x 14.4):** full-height glazing in stone portal frames (slice 7).
- **Edge lights:** kept, with LOOK_V2's colours (2.4).

**Per-face variation within a tower:** only the warm band and +-4 % albedo, both by face id.

### 2.4 Window light (LOOK_V2 section 4, adopted as written)

The constants get new names in `NocFacadeConstants.cs` (FA). The old `NC_Facade*` lines of `NocturneConstants.cs` (126-160) stay byte-identical:
- `CityField` (frozen) reads `FacadeEdgeFraction`;
- the v1 path (`-facade-v 1`) reads the rest until TP-2.

| Item | Value (LOOK_V2 section 4) |
|---|---|
| Residential window | 1.6 x 2.2 m clear glass (S1); the other skins reach the budget through c_k (2.3) |
| Lit fraction | 0.26, with strata factors 1.08 / 0.69 / 0.23 / 2.31 (share x factor sums to 1.00) |
| Zone spread | 0.40 + 1.2 U (mean 1) |
| Hue classes | WinTungsten w_f/3, WinWarm 2 w_f/3, WinNeutral n_f, WinCold 1 - w_f - n_f - 0.07, WinBlue 0.03 (x0.6), coloured 0.04 (desaturated 40 %, x0.8); class bands soft by 0.03 |
| Schedules | w_f(s): 0.32 for s 0-400, 0.20 at s 880, 0 from s 1 250; n_f(s): 0.30 up to s 880, 0.15 from s 1 250 |
| Brightness | E = token x (Y_med / Y(token)) x b x interior, with Y_med 0.15 and b = exp(0.45 N(0, 1)) clamped to [0.40, 2.40] |
| Frame and mullion | 0.12 m, MetalPaint |
| Curtains / blinds | 35 % (x0.45, tint lerp(hue, WinWarm, 0.25)) / 15 % (8 cm slats, faded below a 3 px slat pitch) |
| Ceiling falloff | 1.0 at the head to 0.7 at the sill |
| Unlit glass | GlassDark x ambient_eq + WinNeutral x 0.0025 x (0.5..1.5 by hash), at every distance, included in every mean |
| Glass | F0 0.04, perceptual roughness 0.05, pane tilt +-1.5 deg |
| Edge lights | white 42.5 %, cyan 25 %, amber 25 %, magenta 7.5 % (`NocFacadeEdgeColour` only) |

**Schedules.**
- AtmosphereDriver (PA) writes `_NocFacade.y = w_f(s)` (as today) and **`_NocFacade.w = n_f(s)`**. Component w is free; z is the scene time (`NocFacade.hlsl:19`).
- The knots are appended to `NocturneConstants.cs` (PA), so AtmosphereDriver and `NocFacadeMath` (FA) read the same values.
- Both PA and FA list this as a dependency.

**Lit probability per window:**

p = clamp(lit x f_stratum x m_T x g_G x z_Z, 0, 1) x c_norm

- **m_T** is the tower multiplier: log-normal with sigma_ln 0.35 and mean 1, keyed by K.
- **g_G** is the lit-floor-group factor: log-normal with sigma_ln 0.45 and mean 1, keyed by (K, group). Groups of 4-12 floors are laid out by (K, band).
- **z_Z** = 0.40 + 1.2 x 0.5 (U_row(K, band, zone row) + U_cell(face, zone row, zone column)).
  - Zone rows (2 floors) are keyed per tower, so lit and dark floors continue around corners.
  - Zone columns (24) stay per face.
- **Stratum type** is keyed by (K, band). Today it is (face, band) (`NocFacade.hlsl:42`), which lets two faces of one tower show 3.6 m and 4.32 m floors in the same band.
- **c_norm** is a constant that restores the mean after the clamp. The C# twin computes it, and FacadeTests asserts the mean within 1 %.

**Each level is filtered to its parent's mean:**
- cells by the exact 2 x 2 box filter (H2);
- zones by today's 2 x 2 zone filter;
- groups: g fades to 1 by smoothstep(3, 6, group_px);
- m_T never fades: a tower is 9 px or wider at 3 km.

### 2.5 Structure: whole bays, elements and the 3 km macro layer

**Cell coordinates.** x = u - i P.x and y = v - j P.y. Every element is an interval or a set of filtered bars per axis. Its coverage comes from `NocCover` over the 2 x 2 cells under the footprint, so it is exact.

**Whole bays only.**
- A cell is drawn as a window only if its opening lies inside [uMin + 0.6, uMax - 0.6] x [y0 + base, y1 - parapet]. Base is 6 m on podium faces whose y0 is 0 or less, and 0 elsewhere; the parapet is 1.6 m. Everywhere else is solid wall, so a corner, ledge or parapet never cuts a window in half (a half window with a reveal would read as a hole).
- Without y0 and y1 (Gulf writers), only the u limit applies.
- u stays world-absolute, and the limits come from the shared face extents. Coplanar neighbours (a slab and its closure, or the two city tiers) therefore stay continuous.
- **Band means include the suppressed cells:** mean x (whole cells x P.x) / (uMax - uMin) x (rows inside) / (rows in the band), analytic per pixel.
- The rule also produces the **corner piers**: 0.6 m plus the remainder to the first whole bay, 0.6-3.0 m in total.

| Element | Interval or rule | Material and shading |
|---|---|---|
| Slab edge (floor line) | y in [0, h_slab) | skin slice, AO 0.85; continuous around corners, because strata are per tower (2.4) |
| Spandrel | y in [h_slab, y_sill) | skin slice (S0: spandrel glass) |
| Pier | x outside the opening | skin slice; on S1 and S4 a deeper column line every 3 bays |
| Panel joints | lines at the panel edges, 15 mm | darker and rougher; `NocFilteredGrid1` (min 1 px, energy w/drawn; mean w/P below 4 px). The photo panels' seams lie inside them (1.6). |
| Opening | [x0, x1] x [y_sill, y_head] | recess rectangle (2.6) |
| Frame and mullions | 0.12 m bars at the glass plane | constant anodised (MetalPaint) albedo; filtered bars; they occlude glass emission |
| Sill and lintel | 0.12 m bands (S1, S3) | stone or concrete slice; sills are wet (2.6) |
| Loggia band (S4) | whole bay, 40 % of bands, not in corner bays or under the parapet | recess 1.4 m; slab edge in front; filtered rail bars |
| Parapet and setback band | v in [y1 - 1.6, y1] | no windows; coping line; metal flashing |
| Base band | v in [y0, y0 + 6] on podium faces | lobby glazing or stone (slice 7) |
| Corner trim | u within 0.25 m of uMin or uMax | metal flashing; shares the edge-light distance |
| Sill streaks | below each opening, OverlayArr 0 in cell-local coordinates, offset by the window hash | darker and smoother; faded with w_tex |

**Macro layer: structure that survives to 3 km.** A feature is 3 px or wider at 3 km only if it is about 11 m or larger. The layer has these parts:
- the **tower multiplier** m_T (2.4);
- **lit floor groups** of 4-12 floors (2.4);
- a **core strip**: on 40 % of faces wider than 30 m, a blind strip of whole bays 3-6 m wide (lift and stair core), full height, at a bay position keyed by K, so it lines up on opposite faces (slice 0 or 2);
- **mechanical stripes**: 2 blind floors (7.2 m) with louvres every 20-30 floors, keyed by K, on all four faces;
- a **setback ledge line**: on 30 % of towers (by K), a lit line at the top of each box (y1), LOOK_V2's SignWhite or WinNeutral. It is drawn at least 1 px wide with energy w/drawn, so at 3 km it is a 1 px line of the right energy. It needs y1.
- the **stratum bands** (86.4 m) and mechanical strata, as today.

**Pixel size of every feature** [computed; px = size / (k x distance); 4K doubles every value]:

| Feature | Size (m) | 30 m | 300 m | 3 km | 3 km, 4K |
|---|---|---|---|---|---|
| Panel joint | 0.015 | 0.42 | 0.04 | 0.004 | 0.008 |
| Blind slat pitch | 0.08 | 2.2 | 0.22 | 0.02 | 0.04 |
| Frame or mullion | 0.12 | 3.3 | 0.33 | 0.03 | 0.07 |
| Window width (S1) | 1.6 | 44 | 4.4 | 0.44 | 0.89 |
| Column pitch | 2.4 | 67 | 6.7 | 0.67 | 1.3 |
| Floor | 3.6 | 100 | 10 | 1.0 | 2.0 |
| Corner pier | 0.6-3.0 | 17-83 | 1.7-8.3 | 0.17-0.83 | 0.33-1.7 |
| Core strip | 3-6 | 83-166 | 8.3-17 | 0.83-1.7 | 1.7-3.3 |
| Mechanical stripe | 7.2 | 200 | 20 | 2.0 | 4.0 |
| Lit floor group | 14.4-43.2 | 399-1 198 | 40-120 | 4.0-12 | 8.0-24 |
| Tower width (CityHalfMin/Max 17-32) | 34-64 | 943-1 774 | 94-177 | 9.4-18 | 19-35 |
| Stratum band | 86.4 | 2 395 | 240 | 24 | 48 |
| Setback ledge line | 0.3 (drawn at 1 px or more) | 8.3 | 0.83 | 0.08 | 0.17 |

At 3 km a tower therefore reads through its overall brightness, its lit floor groups (4-12 px tall), mechanical stripes (2 px), setback lines (1 px), stratum bands and edge lights. Every smaller feature contributes only its mean.

### 2.6 Recess rectangles, spill and wet sills

**Setup.**
- Express the eye-to-surface direction in the facade frame (T, B, n) of 1.6 as (dt, db, dn), with dn > 0 pointing into the wall. Let D be the recess depth.
- A ray entering the opening reaches the glass plane shifted by s = D (dt/dn, db/dn) along T and B.
- T is the direction of growing u, so the cell-coordinate shift is (du, dv) = (s_t, s_b B.y). B.y is 1 on vertical faces and below 1 on the tilted hull.

**The method.**
- The visible glass is the opening intersected with its image shifted by -(du, dv). Per axis that is [max(x0, x0 - du), min(x1, x1 - du)], and the same in v.
- glass coverage = cov(glass rectangle); reveal coverage = cov(opening) - cov(glass). The existing exact 2 x 2 box filter runs unchanged on these per-pixel bounds.
- **Reveal normals** are expressed in the corrected frame: the visible jamb uses +-T, facing the opening centre; the sill +B; the head -B. Reveals use the skin slice at AO 0.6.
- On sigma = -1 faces the draft's fixed frame inverted all of this (glass looked proud of the wall, and reveals were lit from the wrong side). FA's facade sheet checks the left and right reveal sign (section 7).
- Frames and mullions lie at the glass plane, so they shift with it.
- At grazing views (|du| at or above the opening width), no glass is left.
- dn is clamped to 1e-3 or more.

**Room-light spill** (new). It is the one cue that survives on towers that have no local lights (LOOK_V2 5.3).
- A lit window lights its own reveal and frame: L_reveal += E_window x albedo_reveal x F, with F = 0.25 on jambs, 0.35 on the sill and 0.15 on the head.
- The frame and mullion inner edges get F = 0.20.
- A short halo sits below the opening: spandrel L += E_window x 0.02 x exp(-d / 0.4 m) for d in [0, 1.2] m (rain scatter).
- All of it is analytic per cell (x4 in the box filter) and included in the band means through the C# twin.

**Wet sills and ledges.**
- The sill top inside the reveal (normal +B) and the parapet coping have smoothness 0.85 and wetness 1, and reflect the sky and the skyline (2.7). Views from the bridges and the skybridge see them.
- The 5 cm sill drip edge reflects the horizon haze. It is a bar under the energy rule.

**Per-panel tone.** Wall panels vary by +-12 % by panel hash (1.6), faded below 2-4 px panel size.

**Energy.** The far regime uses the same view-dependent expected glass fraction, m_g(V) = max(0, W_x - |du|) max(0, W_y - |dv|) / (P_x P_y), so every regime agrees.

**Cost:** about 25 ALU for the rectangle and 15 for the spill.

### 2.7 Glass

**Material.** Dielectric, F0 0.04, perceptual roughness 0.05 (LOOK_V2).

**Reflection.** The draft's city-glint lattice is replaced. On planar glass the reflected direction changes by about 0.061 deg per pixel, so 0.6 x 0.4 deg lattice cells would show as a fixed grid of about 10 x 7 px at every distance [computed].
- **Open city: a reflected skyline (`NocSkyline`, in `NocFacadeSkin.hlsl`).**
  - Azimuth is split in two levels: 4 deg coarse bins, each cut at hashed points into 1-3 buildings 0.6-4 deg wide.
  - Each building has a hashed top elevation (horizon + 0.5-12 deg, log-normal) and a hashed distance.
  - Its body radiance is the facade mean radiance (2.11) through the fog of that distance.
  - Window speckle uses a per-building (azimuth, elevation) grid with a building-specific pitch (0.08-0.3 deg) and a hashed phase, so there is no global lattice. Lit cells follow LOOK_V2's hue mix and lit fraction.
  - The speckle falls to its building mean when a cell is under 2 x (GGX lobe + pixel angle).
  - Above the skyline: `NocSkyColorBlur`. Below the horizon: the abyss haze.
  - Cost about 60 ALU.
- **Canyon and Gulf.**
  - Where the reflected ray hits a wall plane registered in `NocWetRegistry` (beats 1-3 and 5), the glass reflects that plane's macro radiance through SU's wall evaluation in `NocMirror.hlsl` (FA includes it read-only).
  - The opposite wall fills the reflection up to about 75 deg of elevation.
  - The same 5 deg blend around the vanishing line as the wet floor (3.3) avoids a switch.

**Pane tilt.**
- A hashed normal tilt of +-1.5 deg per pane breaks the reflection pane by pane.
- When panes are under 2 px, the tilt variance ((1.5 deg)^2 / 3, about 2.3e-4 rad^2 [computed]) is added to alpha^2 instead: same mean, no sparkle.

**Unlit glass:** GlassDark x ambient_eq + WinNeutral x 0.0025 x (0.5..1.5 by hash), at every distance (2.4).

**Grime and rivulets.**
- Grime (OverlayArr 1) modulates roughness from 0.05 up to 0.20 and adds up to 3 % albedo.
- Rivulets (OverlayArr 2) apply in the near band only and are cut early (9).

### 2.8 Interiors

**Box rooms are the main lit-window look.** The facades are seen from 50-1 000 m (2.9), where a window is 5-40 px. The cues that read at that size are:
- room depth parallax: a box room with analytic shading, back wall brighter than the side walls;
- the ceiling-light gradient (1.0 to 0.7);
- curtain and blind silhouettes (LOOK_V2: 35 % and 15 %);
- the room's own spill on the reveal (2.6).

**Box geometry.**
- Room box per window cell: width P.x, height P.y, depth 6-10 m (residential) or 10-14 m (commercial) by hash.
- Ray-box intersection in van Dongen's branch-free reciprocal form, starting at the glass-plane point.
- One ceiling light per room at (0.5 W, 0.9 H, 0.35 D), with LOOK_V2's temperature and brightness; the planes are shaded analytically from it.
- Unlit rooms show only the stand-by term (2.4).

**Textured rooms** (RoomArr slices of 1.3, `SampleLevel` with the analytic LOD log2(fp (viewDist + t_hit) / viewDist / texel)) appear only where w_room = smoothstep(24, 40, px2) is above 0, which is under about 50 m at 1080p. The walk almost never sees NocFacade that close:
- the alley towers start above 22 and 31 m podia, at a 30-34 m setback;
- the boulevard podia are 75 m from the median.

So textured rooms and the dark furniture plane (four procedural templates at 0.45 D) are the first two items of the cut ladder.

**Normalisation.**
- The C# twin `NocInteriorMath` integrates each template's visible radiance through the aperture.
- E_room_mean(V) is approximated separably.
- Textured room, box room and E_room_mean(V) are each the expectation of the previous one (tests: 1 % between regimes, 3 % against numeric integration up to 60 deg).
- Curtains and blinds enter as their mean factors below their fades.

**Not used: Facade018B's Emission as a window atlas.** The review suggested this. I measured the map:
- its 36 windows are flat gradient rectangles (per-window mean 0.08-0.32), with no rooms, curtains or blinds (crop in `ExternalTextures/tmp/techf/f018b_em_crop.png`, git-ignored);
- TEX-R made the set reference only.

The analytic ceiling gradient gives the same signal with no texture.

### 2.9 Distance ladder, energy, shimmer and crawl

**Measures.**
- fp_u and fp_v are the per-axis bounding-box footprints, |ddx(u)| + |ddy(u)| and the same for v.
- px2(a, b) = min(a / fp_u, b / fp_v).
- The thresholds are in pixels, so every distance doubles at 4K.

| Band (1080p) | What is rendered | Weight |
|---|---|---|
| **Near**, under about 50 m (rare on the walk) | everything of Mid, plus textured rooms, the furniture plane and glass rivulets (cut ladder 1-3). Facades get no micro detail: they are never within 30 m. | w_room = smoothstep(24, 40, px2) |
| **Mid**, about 50-1 000 m (the effort goes here) | box rooms with parallax (w_box = smoothstep(5, 9, px2), to about 220 m); ceiling gradient; curtains and blinds (windows 6 px or taller, to about 250 m); recess rectangles with spill and wet sills; frames and mullions at their thresholds; panel joints; per-panel tone; panel-shuffle skins (aniso 16, Toksvig mips); skyline reflection; pane tilt | per element, from the crawl tests (below) |
| **Far**, beyond about 1 000 m | window means (H2: w = smoothstep(2.5, 5.5, P / fp) per axis); band means with whole-bay suppression; the macro layer (2.5); wall = the skin's last-mip mean; x m_g(V) x E_room_mean(V) | H2 |

**Energy rules.** Every fade blends a term with its own expectation.
- **E1. Textures.** A texture blends with its last mip.
- **E2. Per-cell hashes.** Below their fade, per-cell and per-tower hashes are replaced by their analytic means.
- **E3. Rooms.** Textured room, box room and E_room_mean(V) form a chain of expectations.
- **E4. Recess.** The rectangle method is exact in every regime.
- **E5. Bars and joints.** `NocFilteredGrid1` semantics.
- **E6. Bands.** Bands, whole-bay limits and macro features use exact `NocCover` blends.
- **E7. Budget.** Each region's mean is reached through c_k (2.3).

**No-shimmer rules.**
- **S1.** Every periodic element reaches its mean before its period drops under its crawl threshold.
- **S2.** There is no per-pixel noise.
- **S3.** `SampleGrad` with true gradients, Toksvig mips and aniso 16.
- **S4.** Derivatives are taken only of continuous quantities, before any branch.
- **S5.** Weights are per-pixel functions of pixel measures; there is never a per-object switch.
- **S6.** Tilt, glint and speckle variance fold into roughness or into their means below 2 px.
- **S7.** The same thresholds hold at 4K.

**Crawl tests** (S1 thresholds come from these, not from a guess).
- A box integral over one shading sample still crawls in pans when bars are 2-4 px. FA's C# twins therefore sweep every periodic element:
  - window cells, mullions, frames, joints, blinds, louvres, ribs;
  - courses of the registered slices;
  - groups and stripes.
- Each sweep covers periods of 2-8 px in 0.25 px shifts (32 shifts) and measures the temporal std of the 8-bit output, after the twin of the grade's tone curve at beat-2 EV.
- An element must stay at a std of 1 code or less. Its fade threshold is the smallest period that passes.
- `Tools/qa/tiling.py` checks the hex lattice period (tile / 1.25) as well as the tile period.

### 2.10 Section mode, roofs and the Gulf (CRIT0 #6)

**Mode 1 (Gulf cut faces, the tongue hull, the portal panels).**
- Bays of 9 x 4 m: slab lines every 4 m, columns every 9 m.
- Cut floors in weathered concrete (slice 1), and glass.
- Box-room interiors 8-12 m deep.
- Atria every 250 m and mechanical bands every 300 m are dominant macro features.
- The skin byte is ignored; the frame comes from sigma (1.6); y0 and y1 are unknown (2.2).

**Mode 2 (roofs).** Slice 0 with the parapet coping.

### 2.11 Macro radiance of the wet wall planes (H4)

The wet floor reflects each registered wall plane as one macro radiance (`NocWetRegistry.AddWall`; `NocMirror.hlsl:137-144` adds the 86.4 m band ripple). Today these values are hard-coded, and nothing calls `NocFacadeMacro` or `NocFacadeMath.Macro`. The only runtime use of `NocFacadeMath` is `FaceId`. This plan adds two helpers:
- `NocFacadeMath.MeanRadiance(int mode, int region, double s)` (FA): the facade's mean radiance at frontal incidence (lit glass + unlit glass + wall x ambient). It is the same expectation as the far regime. `NocFacadeMacro` stays as its HLSL twin for the tests.
- `NocSurfaceMath.WallRadiance(NocPal token, double s)` (SU): the albedo token x the ambient at s (AtmosphereDriver's trilight), for flat walls (kind 1).

| Call site | Kind | Today | New |
|---|---|---|---|
| `Beat01_Alley.cs:26` | 1 | literal (0.031, 0.021, 0.021) | WallRadiance(ConcreteAlley token) |
| `Beat02_Boulevard.cs:32` | 0 | WinCold x 0.035 | MeanRadiance(0, D, s) |
| `Beat03_Edge.cs:158-168` | 0 | v1 hue mix 82/8/6/4 with NeonMagenta, FacadeSecLit 0.40 | MeanRadiance(1, -, s) |
| `Beat05_Garden.cs:64` | 0 | WinCold x 0.03 | MeanRadiance(1, -, s) (the notch walls are section mode, `GulfFace.cs:225-231`) |
| `OldQuarterBuilder.cs:225` | 1 | ConcreteDark x 0.08 | WallRadiance(Brick token) |
| `NocMaterialSheet.cs:93` | 1 | ConcreteDark x 0.25 | WallRadiance(ConcreteDark) |
| `ScreenWallBuilder.cs:71` | 1 | `NocScreenMath.MeanEmission` | unchanged: it is already the screen wall's own twin (PA updates the ramp) |

SU edits the first six call sites. `WetMacroTests` (SU) asserts that every registered macro is within 5 % (luminance and each channel) of its C# mean at the midpoint of the wall's s range.

### 2.12 Cost, overdraw and anisotropy

**Per-cell and per-pixel work.**
- **Per 2 x 2 cell** (x4 in the box filter): window hashes (pcg3d/pcg4d), lit state over two epochs, hue class, brightness b, curtain or blind state, pane tilt, spill weights, room depth.
- **Per pixel:** the separable interval coverages (decomposition, bands, whole-bay limits, macro layer), the recess rectangle, the panel-shuffle taps, the grime tap, the skyline, the visible-room shading and lighting.

| | Today | Near | Mid | Far |
|---|---|---|---|---|
| ALU per pixel (approx.) | about 1.0 k | about 1.7 k | about 1.5 k | about 1.15 k |
| Texture taps | 0 | 6-8 (skin 3, grime 1, rooms 2-4) | 4-5 | 1-3 (last mips) |

**Overdraw first.** Depth priming is disabled (`PipelineConfigurator.cs:97`), there is no depth prepass, and the depth texture is off (D-008). Every overlapping tower fragment therefore pays the full facade cost.
- TP-0 measures NocFacade overdraw with debug view 9 (section 7) at KS04, KS05, KS09 and KS17.
- If any of them exceeds 1.5, TP-0 adds `NocFacadePrepassFeature`: a depth-only pass for NocFacade (and the foliage cores) before the opaques. The facade forward pass then uses `ZTest [_FacadeZTest]` = Equal and ZWrite Off, set on the Facade row by FA.
- The prepass shares the MSAA target, so the equal test runs per sample.

**Anisotropy.**
- At grazing incidence (the Gulf faces, the canyon), one aniso-16 trilinear tap can cost up to 32 bilinear fetches.
- At about 482 GTexel/s (RTX 5070), facade texturing alone is about 2-4 ms at 4K [estimate]. It sits inside the 4K facade budget (section 9).
- Cut ladder item 7 lowers facade aniso to 8 (9 under ForceEnable) if needed.

---

## 3. Near surfaces

### 3.1 Mapping rules

- **`NocSurface`** has `_Mapping`: 0 is world triplanar (the default: beat meshes have no consistent UV0 scale), 1 is UV0 in metres (bark).
- **`NocWet`** uses planar xz.
- **Facades** use face (u, v).
- **`NocGrating`** uses planar xz in world space.
- **`NocShopfront`** uses panel-local UV0 (3.5).
- **`NocLine`** has no textures; a 1D paint variation runs along the axis (3.5).
- Every row follows the colour rule (token = mean) and the absolute-smoothness mask. Sets and tiles are in 1.3.

### 3.2 Macro variation, overlays and wetness per row

Porosity values (LOOK_V2 5.2): concrete 0.4, render 0.3, brick 0.5, granite 0.35, bark 0.45, moss 0.4, paint 0; asphalt's wetness is already in its token.

| Row | Macro variation | Overlay | Wetness |
|---|---|---|---|
| `Concrete` | 9 m, +-15 % | leaks x0.4 | porosity 0.4 |
| `ConcreteAlley` | 9 m, +-18 %, plus OverlayArr grime at 8 m x0.5 | leaks x0.6, anchored (3.3) | porosity 0.4 |
| `StoneWall` | 13 m, +-12 % | leaks x0.3 | porosity 0.35; smoothness up |
| `SteelPainted` | 7 m, +-12 % | rust bleed (OverlayArr 3) x0.3 | water film: smoothness up only |
| `SteelBare`, `Brass` | 7 m, +-8 % | - | film |
| `Moss`, `FloorMoss` | 6 m | - | porosity 0.4 |
| `Bark` | 5 m | - | porosity 0.45 |
| `Asphalt` | 11 m, +-15 % | - | token |
| `Brick` | 9 m, +-15 %, plus grime | leaks below the windows and the balcony x0.5 | porosity 0.5 |
| `Plaster` | 9 m, +-15 % | leaks x0.5 | porosity 0.3 |
| `Wood`, `TileWall` | - | grease grime x0.3 | sheltered: wet 0.1 |
| `Fabric` | - | - | sheltered: 0.1 (noren, curtain) |
| `Grating` | - | rust bleed x0.2 | film |
| `GlassBack`, `GlassFront` | - | grime (OverlayArr 1); rivulets near only | - |
| `GlassDark` | - | - | film |
| `FloorAlley` | 11 m, +-15 % | - | puddles 0.35 with a height bias |
| `FloorStone`, `GrayFloor` | 11 m, +-15 % | - | puddles fill the joints first |
| `FloorSteelDeck` | 11 m, +-10 %, plus grime | - | puddles in plate low points; analytic weld seams on the 1.2 x 2.4 m plate lines |
| `FloorGardenPath` | 11 m | - | moss tint in the joints (height < 0.3, x Moss token); puddles 0.25 |
| `FloorPlaza` | 11 m | - | puddles 0.35 |

### 3.3 Micro detail, anchored leaks and wetness

**Micro detail.**
- `MicroArr` at 0.5 m, with w_det = smoothstep(4, 8, 0.5 / fp), on NocSurface and NocWet rows.
- It removes the magnification blur within 1-2 m at 4K: the alley walls come within 0.4 m, and the deck at KS13 is at 1.8 m.
- It is cut-ladder item 4.

**Leaks anchored to top edges.** The draft placed leaks by hex, so they started at random heights.
- **The bake.** For materials with `_LeakAnchor` = 1 (`ConcreteAlley`, `Concrete`, `StoneWall`, `Brick`, `Plaster`, `SteelPainted`), `WorldContext.AddMesh` (SU) bakes once at load:
  - vertex colour B = the distance below the top of the vertex's flat face, in 0.1 m steps (B = 255 saturate(d / 25.5 m));
  - the top is the highest vertex over the triangles that share the vertex, propagated over shared vertices.
  - It uses `VertexBake.TopDistance` (SU). NocSurface reads only R (AO) and G (edge wear) today, so B is free.
- **The decal.** The leak overlay (OverlayArr 0) is sampled as a decal hanging from that edge:
  - v_leak = d_top / (1.0 m x stretch), with stretch 1-2 by hash;
  - horizontal tiles of 2 m with hashed offsets;
  - the padded part of the slice has opacity 0, so leaks end below the decal.
- Leaks therefore start at ledges, sills, parapets and sign frames.
- **Effect:** albedo x0.7 at full strength, smoothness +0.15, a little less AO. Below its fade the overlay uses its slice mean.

**Wetness (H4, LOOK_V2 5.2).**
- **NocSurface.** wet = saturate(_Wetness (0.55 + 0.45 up) + streak 0.5 + up x 0.35 x (0.5 - height)).
  - Porous rows darken by lerp(1, 0.2, porosity) x wet, and gain gloss per Lagarde.
  - Metals, glass and tiles gain smoothness only.
- **NocWet puddles.**
  - Mask: m = smoothstep(th - e, th + e, pn + 0.35 (0.5 - h)), so joints and low spots fill first.
  - The noise scales become 2.0 and 0.6 m (were 4.0 and 1.3). These are new constants in `NocSurfaceConstants`; the `NocturneConstants` puddle, ripple and wet lines (103-110) stay frozen for the v1 path.
  - The threshold is recalibrated per material from the real height histograms, so the puddle fraction stays 0.35 +- 0.02 (10^6 samples, C# mirror).
- **Puddle normals stay flat:** up plus ripples, with the textured gradient weighted by (1 - m).
- **Mirror lobe.** sigma2 = lerp(alpha_dry^2, alpha_puddle^2, m) + floor + ripple, with alpha_dry from the absolute mask smoothness. The D-003 footprint AA and the L_eff <= 3 cap stay.
- **Ripples** become many faint rings of 0.1-0.4 m inside puddles only, faded by distance (CRIT0 #29).
- **Wall planes** in `NocMirror` blend over about 5 deg around the vanishing line instead of switching (CRIT0 #8). `NocMirror.hlsl` exports `NocWallReflect(P, r, soft)` for FA's glass (2.7).

### 3.4 Property contract

Names and types stay as they are, because other owners set or read them:

| Property | Type | Set or read by |
|---|---|---|
| `_AlbedoTex`, `_NormalTex`, `_MaskTex` | Texture | `GardenMaterials.cs` (copies), `G4Kit`, `G1Materials` |
| `_Tile` | **Float** (read with `GetFloat`) | `GardenMaterials.cs`. An anisotropic tile gets a separate new Float, `_TileV`. |
| `_BaseColor`, `_EmissionColor` | Color | beats throughout |
| `_Wetness`, `_Streaks`, `_Cull` | Float | `SkybridgeMaterials.cs:21-23` |
| `_SmoothMin`, `_SmoothMax`, `_MacroAlbedo`, `_NormalStrength` | Float | `OldQuarterBuilder.cs:213-216` (until SU moves the panes to `GlassDark`), `GardenMaterials.cs` |
| `_PuddleFraction`, `_AlphaDry`, `_ReflDry` | Float | `GardenMaterials.cs` (until FO uses `lib.FloorMoss`) |
| `_LineWidth`, `_Palette` | Float | `G1Materials.cs:27-28`, `Beat03_Edge.cs:138`, `SkybridgeMaterials.cs:39`, `G4Kit.cs:16` |
| `_WindowGain` | Float | `Beat03_Edge.cs` (`GetFloat`; still read until SU switches `SectionMacro` to `MeanRadiance`) |

**New properties:** `_TileV`, `_Mapping`, `_LeakAnchor`, `_Porosity`, and per-row overlay, micro and macro gains. All are Floats with no keywords.

**Moss ground.** `GardenMaterials` derives its moss ground from `FloorGardenPath`, then copies only three textures and `_Tile` from `lib.Moss`. New anti-tiling, micro and overlay properties would never reach it. SU therefore provides a library row `FloorMoss`, and FO's `GardenMaterials` uses it directly.

### 3.5 Special surfaces

- **Grating** (`NocGrating.shader`, new; CRIT0 and KS02 name the catwalk).
  - MetalWalkway006's opacity drives alpha-to-coverage: `AlphaToMask On`, Cull Off, ZWrite On, AlphaTest queue.
  - Alpha sharpening works as for foliage. Coverage fades to its mean of 0.35 when the 62.5 mm mesh pitch falls under 3 px (beyond about 17 m at 1080p).
  - The catwalk deck and the fire-escape landings (`AlleyFixtures.cs:35, :59`, today SteelPainted boxes) become thin two-sided plates, so sky and walls show through. Rails stay NocLine.
- **`GlassDark`** (opaque dark panes).
  - Today the Old Quarter panes are `G4Kit.Tinted(lib.Concrete)` (`OldQuarterBuilder.cs:211`), which would turn into textured concrete in KS19. They move to `GlassDark`.
  - Smoothness stays 0.45-0.55: the r01 KS17 lesson was that at 0.86-0.92 the panes beside the goal reflected the goal light as warm points.
- **Goal curtain** (`OldQuarterBuilder.cs:188`): emissive `Fabric` instead of emissive Concrete (KS19).
- **Shopfronts, alley windows, transoms and vending screens** (CRIT0 #24).
  - These are built with `G1Mesh.Quad`, which writes UV0 = 0 (`G1Kit.cs:132-136` calls `MeshBuilder.FlatQuad` without UVs, `MeshBuilder.cs:92-93`).
  - `G1Mesh.Quad` will write **panel-local metric UV0** for every quad (u along the bottom edge, v up; horizontal quads use world x and z). AlleyWindows, AlleyDoors, NoodleBar, VendingMachines and the StreetFacade shops then get surface coordinates with no call-site edits.
  - `NocShopfront.shader` (new, SU) includes FA's `NocInterior.hlsl` read-only and shows a lit shallow room (3-6 m deep) behind the glass. It replaces the flat emissive shop, transom and screen materials through the `G1Materials` slot mapping.
  - Test: no NocFacade mesh is built through `G1Mesh.Quad`, so face ids survive.
- **NocLine paint variation.**
  - Posts of 0.18-0.20 m are seen from 1-3 m: G1 `LinePost` is 0.20 (`G1Materials.cs:19`); the plaza lamp posts are 0.18 (`PlazaFurnitureBuilder.cs:47`).
  - Albedo x (1 + 0.12 n1(a / 0.35 m)) plus sparse chips, and a darker 0.3 m splash band at the foot, where a is the axial coordinate from UV1's tube axis (`MeshKit.cs:258`).
  - It is filtered to its mean below 2 px.
  - SU owns `NocLine.shader` (moved from PA; the Line colour is a row value).
- **Noodle bar.** `Wood` counter (grain along the long axis), `TileWall` back wall, `Fabric` noren.

---

## 4. Foliage (CRIT0 #10, #25)

**Geometry** (`GardenLeafCards.cs`, called from `GardenTree`).
- **Core.** The crown ellipsoid shrinks to 0.90 of its radii. It is opaque and shaded with the atlas's leaf mean, so no sky shows through holes.
- **Cards** sit on two shells at 0.94 and 1.02 of the radii, Poisson-disc placed at 1.5x area coverage, inside 1.10x the ellipsoid. The notch-wall clearance of 1 m is kept.
- **Leaf size.**
  - Cards are 1.6-2.6 m. Each carries a cluster sprite that texprep composes at true leaf size.
  - LeafSet024's 0.40 m atlas holds 3 x 3 leaves of about 0.13 m. They are drawn at 1.0-1.3x, so leaves are 0.13-0.17 m and a 2 m card shows 40-80 leaves.
  - Sprites are 1 024 px over 2 m (512 px/m) in a 2 x 2 atlas of 2048.
  - The draft magnified the atlas 4-6x; this layout magnifies at most 1.3x.
- **Count** [computed; per shell per tree N = 1.5 A / 4.4 m^2, the mean card area (2.1 m)^2]:
  - The 9 hero crowns (r, 0.28 h, r) from `GardenDims.HeroTable` have surfaces of 7 900-18 900 m^2, 124 000 m^2 in all. That gives about 4 700 cards per shell per tree, or 85 k on two shells.
  - The 8 back trees (r = 0.316 h, 50 % density) have about 147 000 m^2 unclipped, so 50 k cards or fewer.
  - **Total:** 135 k cards or fewer, 270 k triangles, 540 k vertices, one mesh per tree, 17 draws.

**Shading** (`NocFoliage.shader`, queue AlphaTest 2450).
- **Pass state.** `AlphaToMask On`, Cull Off, ZWrite On; no blending, no sorting. TP-0's `NOC_ALPHA_OUT` applies to the forward pass only: the DepthOnly and DepthNormals passes never run, because depth priming is disabled (`PipelineConfigurator.cs:97`) and the depth texture is off.
- **Alpha sharpening** (Golus 2017): a' = (a - 0.5) / max(fwidth(a), 1e-4) + 0.5, faded out as the mip rises: lerp(a', a, saturate((mip - 1) / 2)).
- **Coverage-preserving mips** are set on import.
- **Normals.** normalize(lerp(card normal, ellipsoid radial normal, 0.7)) plus the atlas normal as a surface gradient. Back faces flip the normal through TP-0's `NOC_FRONT_FACE`.
- **Lighting.** A wrap and thin-translucency term (8.11 item 7); LeafSet029's Scattering map feeds it on ivy.
- **Colour.** Per-card hue jitter of +-8 %. LeafGlow is sparse (LOOK_V2).

**Early-Z.**
- Alpha-to-coverage decides coverage in the shader and writes depth late.
- Early-Z therefore rejects cards hidden behind opaques (the core, slabs) but not cards hidden behind other cards.
- Cost at KS15, looking up into the canopy: about 3 card layers over 60 % of the frame, about 3.7 M fragments at about 250 ALU and 3 taps [computed].
- Budget: 1.0 ms at 1080p, 2.5 ms at 4K.

**Visibility (V).**
- The cards change the crown silhouettes: the envelope grows to 1.10 and the core shrinks to 0.90.
- V owns `VisibilityAudit`, `VisibilityWorld` and `VisibilitySpecWorld` and decides whether the koi and goal sightlines use the core or the envelope.
- FO provides `GardenLeafCards.Envelope(tree)` and the card density.
- FO's switch to `-foliage-v 2` lands only after V's audit passes.

**Bark.**
- `lib.Bark` uses `_Mapping` 1. Loft and TaperedCylinder write metric UV0 (`MeshKit.Loft.cs:42`).
- After the loft, FO rewrites UV0.x of the trunk and branch rings in `GardenTree` as (angle / 2 pi) x N_t x 1.8 m, with N_t = round(2 pi r_base / 1.8) per branch. A whole number of tiles then wraps the perimeter (no seam), and the grain follows the branch. World triplanar would show top-down bark on leaning branches.

**Rain and depth.** Alpha-to-coverage writes per-sample depth, so the rain's per-sample depth test (D-005) clips correctly behind leaves. `NocRainEdgeProbe` verifies this on a crown edge.

**Moire controls and fallback ladder.**
- Controls: coverage-preserving mips; sharpening only at mips 0-1; bent normals; no wind.
- Fallback, in order:
  1. +0.25 then +0.5 mip bias on the alpha;
  2. 30 % fewer outer-shell cards;
  3. the core alone where a leaf is under 1 px.

---

## 5. Geometry relief (GE; CRIT0 #4, #6, #11, #21, #22)

**Why.** Half of the user's "walls, not buildings" complaint is geometry. Flat quads cannot show a 1.4 m loggia at a corner or against the sky, a crown, a setback silhouette or a fin. GE is a scheduled package with its own owner.

**Scope.**
1. **T0 relief** on beat towers within 150 m of the path: balconies (S4 loggia slab edges and rails as real geometry on near faces), AC units, vertical fins on S0.
   - They are aligned to the analytic grid through FA's `NocFacadeMath` cell API: bay and floor boundaries use the same constants and the face's u, so geometry and shader agree within 1 cm.
   - Relief goes on whole bays only, and never inside the walk clearance.
2. **Crowns and setback silhouettes**, only for towers whose top is below the cloud base (y < 440) and seen in a key shot.
   - City shafts rise to y 560, into the deck, so most tops are hidden. GE lists the visible tops from the 14.10 poses (CityField, read-only).
   - Each visible top gets a mechanical penthouse, parapet, mast or needle, with aviation beacons through the existing `Beacon` material.
3. **Gulf ribs** (CRIT0 #6): cantilevers, pipe bundles and stair towers along faces A and B at the section bays (EdgeDims, read-only), plus the edges of the 300 m terraces. The meshes are separate; no G2 file is edited.
4. **Spire** (CRIT0 #21): a tapering shaft with setbacks, ribs and ring lines, a podium, and a crown or needle with beacons. Skin override 0xF0; PA's strip colours.
5. **Sign-wall base** (CRIT0 #22): an arcade and shopfront module in front of the screen-wall mass, using `NocShopfront`. `ScreenWallBuilder.cs` is not edited.

**Files.**
- `Beats/Relief/ReliefBuilder.cs`, `ReliefKit.cs`, `CrownBuilder.cs`, `GulfRibBuilder.cs`, `SignBaseRelief.cs` (all new). IBeatBuilders are discovered by reflection (`GameBootstrap.cs:139-144`), so no wiring file changes.
- `Beats/Beat07_Heart/SpireBuilder.cs`.
- Tests: `Scripts/Tests/EditMode/ReliefTests.cs` (new).
- Docs: `docs/logs/GE.md`.

**Materials:** library rows only.

**Tests.**
- Relief edges lie on cell boundaries within 1 cm (C# twin).
- No relief inside the PathGuide corridor.
- 400 k added triangles or fewer, 40 draws or fewer (repeats are instanced through `InstancedBatch`).
- V's VisibilityAudit passes.
- CityTests tier identity is unchanged.

**Depends on:** FA-1 (the cell API and skin overrides); the lead's approval for the Gulf ribs.

---

## 6. Koi (KO; CRIT0 #9, H6)

**Why.** The user called the koi a white crescent. LOOK_V2 sets its look (tokens KoiBody, KoiPatch and KoiGlow; sections 7 and 8.1, KS17 and KS18). That work goes beyond colour terms, so it gets its own owner instead of being folded into PA.

**Scope.**
- **Look:**
  - additive and translucent, with the Fresnel rim brighter than the core;
  - body gain 0.55, rim 1.6, body clamp 1.5-2.0;
  - clipped koi pixels 0.6 % or fewer.
- **Kohaku patches:** vermilion KoiPatch cells by filtered cell noise, covering 15-25 % of the body.
- **Structure:**
  - filtered hex scale lines and veins (std 8 codes or more inside the body);
  - slow scanlines;
  - a cyan-to-white gradient along the body;
  - an internal caustic-like variation.
- **Geometry:** pectoral and dorsal fins, a head with eye spots and barbel silhouettes, a body taper.

**Files.**
- `Shaders/Beats/NocKoi.shader`.
- `Beats/Beat07_Heart/KoiBuilder.cs`, `KoiDriver.cs`, `KoiShaderMath.cs` (the C# twin).
- Tests: `Scripts/Tests/EditMode/KoiShaderTests.cs` (new).
- Docs: `docs/logs/KO.md`.
- `KoiMath.cs` (motion) stays V's and is used read-only.

**Depends on:** PA-1's appended tokens.

**Verification:**
- KS17 and KS18 at 1080p and 4K;
- the `koi` sequence;
- LOOK_V2 8.2 (clipped share, structure std, patch share).

---

## 7. Verification infrastructure

**7.1 Debug views** (`-debug-view N`). A uniform global, `_NocDebugView`, set by TP-0's `NocLookGlobals`; never a keyword.
- Views write raw values and bypass fog, grade, bloom and dither: `NocLitPass` returns the debug colour, and `NocGradeFeature` skips the grade while the global is non-zero (PA-2 carries that line).
- `Tools/qa/debugview.py` decodes the PNGs.

| N | View | Encoding |
|---|---|---|
| 1 | skin id, region, override | R = skin x 40, G = region x 128, B = k x 16 |
| 2 | regime weights | R = window w (w_u w_v), G = w_tex, B = w_room |
| 3 | cell coordinates | R = frac(x / P.x), G = frac(y / P.y), B = whole-bay mask |
| 4 | frame sign sigma | R = 255 where sigma = -1, G = 255 where +1 |
| 5 | footprint | R and G = 32 (log2(P / fp) + 2) for u and v |
| 6 | recess shift | R and G = 128 + 64 du / W and 128 + 64 dv / H |
| 7 | world normal | 128 + 127 n |
| 8 | texel density | 0.25 m checker in each row's mapping x a mip-level colour |
| 9 | overdraw | `NocDebugViewFeature` redraws the chosen renderers additively, 1/32 per fragment |
| 10 | lit probability, tower key | R = p, G = K & 0xFF |

**7.2 Facade lookdev sheet** (`-facade-sheet`, FA).
- `NocFacadeSheet.cs` is an IBeatBuilder, as `NocSignSheet` is.
- It builds every skin, every override and the section mode as isolated towers at 30, 100, 300, 1 000 and 3 000 m, each at 0, 45 and 75 deg incidence, under beat 2's lights and atmosphere.
- `Tools/qa/facade_sheet.py` (FA) measures:
  - floor-line autocorrelation at the floor pitch (above 0.3 wherever floors are 4 px or more);
  - window and pier contrast;
  - **texture_std after `Tools/qa/encode_test.py`** (x264 CRF 23, 4:2:0) on wall crops at 75-300 m: 1.5 codes or more;
  - the left and right reveal sign (a wrong sigma fails it);
  - the mean luma per skin against the region budget.

**7.3 Iteration loop.** Packages iterate without waiting for the lead:
- `Tools/lookdev.ps1 -Slot <ID>`: its own build folder, serialised by unity-queue's mutex and gpu-guard;
- `BatchTools.CaptureInEditor` for 3-5 poses;
- `CompileCheck` for shaders;
- `python -I Tools/fastcheck.py all` for C#.

The lead's integration loop (RebuildAll, Validate, EditMode, build, captures, CRIT) runs once per landed package. Never two players at once; long GPU jobs go through gpu-guard.

**7.4 GPU parity tests** (TP-0 harness `ShaderParityTests`).
- Render a quad with the real shader (Facade, Surface, Wet, Foliage, Koi) into a 64 x 64 RenderTexture at fixed inputs, with fog off through a parity global.
- Compare with the C# twin: the mean within 1 %, every pixel within 3 codes.

**7.5 Capture sets** (`ShotPlan.cs` and `CaptureHarness.cs`, TP-0).
- **`texpans`:** pans of 0.5 px per frame at KS00, KS04, KS05, KS08, KS09, KS15 and KS17.
- **`dolly`:** fixed poses with no tower in the way.
  - On the facade sheet: a line from 800 m to 25 m at 30 deg to the face normal. The draft's "along its normal at 30 deg incidence" contradicted itself.
  - A grazing pass along face B from the bridge deck (s 560-880, yaw 70 deg off the face normal).
- **`texclose`:** 8 near poses at 1.5 m (alley wall, podium panel, plaza, bridge deck, tenement brick, noodle bar, trunk, glass tube).
- **`-capture-ss 2`:** renders at twice the resolution per axis and box-downsamples. This is R1's 4x supersampled reference: 4 samples per pixel on top of MSAA 4x. It runs through gpu-guard as a 4K set.

**7.6 Feature flags.**
- `-facade-v`, `-surf-v` and `-foliage-v` are uniform globals (TP-0).
- Each default switches to 2 when its package is accepted. TP-2 deletes the v1 paths.
- A regression is bisected by flipping flags in one build.

**7.7 Performance runs.**
- Exclusive gpu-guard, vsync off, median of 3 runs.
- PerfMonitor's `gpuAvgMs` was invalid in A1 (an overflow value). TP-0 fixes the GPU timing source in `PerfMonitor.cs`; until then the runs use frame time.

---

## 8. Work packages

### 8.1 Order and dependencies

```
now, in parallel:  PA-1 (palette, atmosphere, post, light colours; needs nothing from TP-0)
                   KO   (after PA-1's first commit, which appends the tokens)
                   TP-0 (infrastructure; changes no picture)
after TP-0:        TP-1 (process sets)   FA (FA-1..FA-5)   SU (SU-1..SU-5)   FO   PA-2
                   GE   (after FA-1: cell API and skin overrides)
per landed package: the lead's integration (RebuildAll, Validate, EditMode, build, captures, CRIT)
last:              TP-2 (cleanup)
```

**External dependencies.**
- TEX and TEX-R are done: the final sets are on disk.
- ART's LOOK_V2 is done. PA-1 implements it, and the final FA, SU, FO and KO calibration follows PA's tokens.
- **Lead:**
  - unfreeze `CityMesher.cs` for FA (a few lines in `AddFace`);
  - approve the beat-file ownership in 8.4;
  - approve GE's Gulf ribs.
- **V:** visibility audits for FO and GE.

### 8.2 Rules for every package

- **Exclusive files.** Paths are relative to `Assets/_Game/` unless stated. A file appears in exactly one package, and generated `.meta` files belong to the asset's owner.
  - TP-0 creates the shared partial files once, then hands each to its owner.
  - When another package needs a line in a file, the file's owner carries that line (8.4).
- **Commits.** Each package commits only its own files, as `<ID>: <summary>`.
- **Unity.** Packages may run Unity through `Tools/lookdev.ps1 -Slot`, `CaptureInEditor` and `CompileCheck` (7.3), always through the queue mutex and gpu-guard, never two players at once. C# is checked with `python -I Tools/fastcheck.py all`.
- **Processed textures** (`Assets/_Game/Textures/External/**`):
  - committed once at TP-1 acceptance after calibration, then only at calibration milestones (at most 3 in Look v2), never per texprep iteration;
  - `texprep --check` guards staleness between commits.
- **v1 paths.** Every new path sits behind its flag (7.6). Each package deletes its own v1 path after acceptance.

### 8.3 Packages and exclusive file lists

#### TP-0: infrastructure (owner TP; starts now; changes no picture)

**Files.**
- `Scripts/Editor/MaterialFactory.cs`: core only.
  - `MatSpec`, Build and Check.
  - `Specs()` concatenates the partial row providers.
  - Textures are loaded as `Texture`, so arrays work. A new helper, `ExtMaps(key)`, reads the tile from the texmeta.
  - The existing rows move **verbatim** into `MaterialFactory.Facade.cs` (FA), `.Surface.cs` (SU), `.Look.cs` (PA) and `.Foliage.cs` (FO). TP-0 creates these files and hands them over.
- `Scripts/Runtime/Core/MaterialLibrary.cs`:
  - made `partial`; it keeps `FloorFor`, `All` and `FieldNames`;
  - the fields move verbatim into `MaterialLibrary.{Facade,Surface,Look,Foliage}.cs`, which are created and handed over;
  - serialised field names are unchanged.
- `Scripts/Runtime/Core/NocFacadeConstants.cs` and `NocSurfaceConstants.cs`: created empty, handed to FA and SU.
- `Tools/gen_hlsl.py` and `Scripts/Editor/ShaderGen.cs`: one generated header per constants class, byte-identical between the two generators; a duplicate `NC_` name is an error.
- `Scripts/Runtime/Core/NocTexConstants.cs` (new) and `Shaders/Generated/NocTexConstantsGen.hlsl`.
- `Scripts/Runtime/Core/ShaderIds.cs`: the new globals. Validate reflects over this class (`BatchTools.cs:355`).
- `Shaders/Include/NocLitPass.hlsl`, `NocLitCore.hlsl`, `NocCommon.hlsl`:
  - `float4 uv1` and `NOC_PASS_UV1ZW`;
  - `NOC_ALPHA_OUT`, forward pass only;
  - `NOC_FRONT_FACE` (`SV_IsFrontFace` into `NocFragData`);
  - `NocFaceFrame` with sigma;
  - `f.specOcc`;
  - the debug-view return.
- `Shaders/Include/NocLighting.hlsl` (moved from PA; PA needs only constants there):
  - `NocShade` multiplies the diffuse environment by `s.occlusion` and the specular by `s.occlusion x f.specOcc`.
  - Today both are multiplied by `s.occlusion` (`NocLighting.hlsl:127`). NocFacade puts the city term there (`NocFacade.shader:63-64`), so far walls get x0.12 ambient at eye-level elevations.
  - The default specOcc of 1 changes no picture. FA moves the term.
- `Shaders/Include/NocTexturing.hlsl` (new).
- `Scripts/Runtime/Rendering/NocTexturingMath.cs` (new): the C# twin.
- `Scripts/Runtime/Rendering/NocLookGlobals.cs` (new): reads `-tex`, `-debug-view`, `-facade-v`, `-surf-v` and `-foliage-v`, with v1 as the default.
- `Scripts/Runtime/Rendering/NocDebugViewFeature.cs` (new): overdraw and debug views.
- `Scripts/Runtime/Rendering/NocFacadePrepassFeature.cs` (new, only if overdraw exceeds 1.5).
- `Scripts/Editor/NocTextureImport.cs` (new).
- `Scripts/Editor/PipelineConfigurator.cs`: registers the new renderer features.
- `Scripts/Editor/BatchTools.cs`:
  - **gates:** forward variants against the committed baseline; texture import and format checks; compile warnings X3570 and X4000;
  - **warnings, not gates:** SRP Batcher compatibility via the internal API; unreferenced External textures (until TP-2).
- `QA/baselines/variants_tp0.json` (new): committed first, before any shader change.
- `Scripts/Runtime/Geometry/MeshBuilder.cs`: a warning comment on `Append` only.
- `Scripts/Runtime/QA/ShotPlan.cs`: `texpans`, `dolly` and `texclose` (7.5).
- `Scripts/Runtime/QA/CaptureHarness.cs`: `-capture-ss 2`.
- `Scripts/Runtime/QA/PerfMonitor.cs`: the GPU timing fix.
- `Tools/texprep.py` (new) and `Tools/texprep/tp_common.json` (new).
- `Tools/qa/tiling.py` (new), `Tools/qa/debugview.py` (new), `Tools/qa/sequences.py` (the dolly kind) and `Tools/qa/run_all.py`. run_all registers tiling, debugview, `lookv2_check.py` (PA) and `facade_sheet.py` (FA), each skipped if missing.
- Tests:
  - `TextureTests.cs` (new), `NocTexturingTests.cs` (new), `ShaderParityTests.cs` (new);
  - `ProjectTests.cs`;
  - `MaterialTests.cs` (core). TP-0 splits the per-row expectations into `MaterialTests.{Facade,Surface,Look,Foliage}.cs` and hands them over.
- `docs/logs/TP.md`.

**Steps.**
1. Commit the variant baseline.
2. Run the spikes: does a NormalMap flipbook import as an array (1.5); NocFacade overdraw at KS04, KS05, KS09 and KS17. If it exceeds 1.5, add the prepass.
3. Build the lit template, the globals, the debug views and the parity harness.
4. Build the texturing library, the importer, texprep and the generators.
5. Split the factory, library and tests.
6. Add the capture sets and the PerfMonitor fix.

**Tests.**
- `NocTexturingTests`:
  - hex weights sum to 1 and are continuous (max jump 1e-4 over 10^5 pairs);
  - module-shuffle and panel-shuffle seams fall inside the joint band for every lattice in the manifests;
  - triplanar and UV0 surface-gradient normals match the analytic gradient within 1 deg;
  - sigma is correct on synthetic faces of both orientations and on tilted normals;
  - the float32 UV error at 2 km is under 0.25 texel.
- `ShaderParityTests` passes on the unchanged shaders.
- `ProjectTests`: the generators are byte-identical.

**Acceptance.**
- RebuildAll, Validate, the full EditMode suite and the build pass.
- `keyshots1080` matches the previous run within the dither (mean-luma delta 0.2 levels or less), with every flag at 1.

#### TP-1: texture sets (owner TP; after TP-0)

**Files.** `Assets/_Game/Textures/External/**` (processed PNGs, texmeta and `.meta`) and `docs/logs/TP.md`. The per-package texprep configs are inputs owned by FA, SU and FO.

**Tests** (`TextureTests`, data-driven over every texmeta):
- **Import:**
  - formats (BC7 sRGB, BC5, BC7 linear) and sRGB flags;
  - mips, aniso (effective max(9, level)), streaming off, mip bias 0;
  - size equals the manifest; array depth and slice size;
  - the imported last mip's linear mean is 0.5 +- 0.01 per channel;
  - Toksvig: mip 0 unchanged, A never increasing.
- **Set rules:**
  - each tile equals the TEXTURES.md value within +-15 %, or carries a listed registration;
  - metallic is nonzero only in the four allow-listed sets;
  - contrast and chroma match 1.4 step 5; the violet share is 0.5 % or less;
  - the content gate of 1.4 step 4 holds;
  - leaves have coverage preservation on.
- **Budget:** the VRAM sum from formats is 900 MiB or less.

**Verification.**
- `python -I Tools/texprep.py --check` exits 0.
- The previews' contact sheet is reviewed for tiling, baked light and text.
- The lead runs `-material-sheet` once SU has wired the rows.

#### TP-2: cleanup (owner TP; after FA, SU and FO are accepted)

**Files.**
- `Scripts/Editor/NocTextureGen.cs`, `Scripts/Runtime/Rendering/NocTexRecipes.cs` and the generated `Textures/*.png`: retire the superseded families; keep the rivulets and puddle noise if still referenced.
- `Scripts/Editor/BatchTools.cs`: the unreferenced-texture check becomes a gate.

**Tests.** Validate passes with the gate on.

#### PA: palette, atmosphere, post and light colours (owner PA; implements LOOK_V2 sections 2, 3, 6 and 7)

**Files.**
- `Scripts/Runtime/Core/NocturnePalette.cs`.
- `Scripts/Runtime/Core/NocturneConstants.cs`:
  - the facade and wet lines (103-110 and 126-160) stay byte-identical (frozen: CityField and the v1 paths read them);
  - the w_f and n_f knots are appended.
- `Scripts/Runtime/Core/NocSchedule.cs`.
- `Shaders/Generated/NocPalette.hlsl` and `NocConstantsGen.hlsl` (generated).
- `Shaders/Include/NocAtmosphere.hlsl`.
- `Shaders/NocSky.shader`, `NocGrade.shader`, `NocNeon.shader`, `NocStreak.shader`, `NocRain.shader`.
- `Shaders/Beats/NocScreen.shader` and `Beats/Beat06_Signs/NocScreenMath.cs`.
- `Scripts/Runtime/Systems/AtmosphereDriver.cs`: trilight, AmbientMul, and `_NocFacade.y = w_f(s)`, `_NocFacade.w = n_f(s)` (a dependency shared with FA).
- `Scripts/Runtime/Rendering/NocGradeMath.cs`, `NocGradePass.cs`, `NocGradeFeature.cs` (PA-2: the debug-view bypass).
- `Scripts/Editor/PaletteWriter.cs` and `SceneBuilder.cs`.
- PA-2: `Scripts/Editor/MaterialFactory.Look.cs` and `Scripts/Runtime/Core/MaterialLibrary.Look.cs`.
- Beat files, colour lines only (8.4):
  - `Beats/Beat02_Boulevard/BladeSign.cs`, `MedianPromenade.cs`;
  - `Beats/Beat03_Edge/TonguePier.cs`;
  - `Beats/Beat04_Skybridge/SkybridgeLamps.cs`, `SkybridgeMaterials.cs`;
  - `Beats/Beat06_Signs/ArcadeBuilder.cs`, `PromenadeBuilder.cs`, `ScreenWallBuilder.cs`;
  - `Beats/Beat07_Heart/PlazaFurnitureBuilder.cs`.
- `Tools/qa/lookv2_check.py` (new).
- Tests: `AtmosphereTests.cs`, `GradeTests.cs`, `ConstantsTests.cs`, `RainTests.cs`, `MaterialTests.Look.cs`, `Beats/Beat06SignsTests.cs`.
- `docs/logs/PA.md`.

**Steps.**
1. **PA-1 (now).**
   - The first commit appends the 15 LOOK_V2 tokens only. No `PAL_*` token that another package's shader references is removed until FA, SU, FO and KO have switched.
   - Then the palette values, atmosphere, sky, fog, grade, neon, streaks, rain tint, screen ramp and AtmosphereDriver.
   - Then the beat light colours in PA's files.
   - PA hands the colour lines that other owners carry (8.4) to them as a list.
2. **PA-2 (after TP-0).** The Look rows and the grade bypass.

**Tests.**
- T-FOG follows the new sigma.
- Horizon agreement: fog, sky and dome agree at 12 elevations within dE 2.0.
- The H1 gates, including the encode test at CRF 23.
- The LOOK_V2 8.2 checks: violet share under the cap; magenta and violet only as small emissive accents.

**Verification by the lead.**
- All key shots at 1080p, the encode set and the boundaries.
- Image metrics against the LOOK_V2 luma windows, with 14.10 arbitration where arithmetic requires it.

#### KO: the koi (owner KO; after PA-1's first commit)

**Files.**
- `Shaders/Beats/NocKoi.shader`.
- `Beats/Beat07_Heart/KoiBuilder.cs`, `KoiDriver.cs`, `KoiShaderMath.cs`.
- `Scripts/Tests/EditMode/KoiShaderTests.cs` (new).
- `docs/logs/KO.md`.

**Scope and verification:** section 6.

#### FA: facades, glass, interiors (owner FA; after TP-0; final calibration after PA-1)

**Files.**
- `Shaders/NocFacade.shader`.
- `Shaders/Include/NocFacade.hlsl`, `NocFacadeSkin.hlsl` (new: skins, regions, decomposition, bands, macro layer, recess, spill, skyline), `NocInterior.hlsl` (new).
- `Scripts/Runtime/Core/NocFacadeConstants.cs` and the generated `NocFacadeConstantsGen.hlsl`.
- `Scripts/Runtime/Rendering/NocFacadeMath.cs`: SkinByte, MeanRadiance, the cell API for GE, and the twins.
- `Scripts/Runtime/Rendering/NocInteriorMath.cs` (new).
- `Scripts/Runtime/Rendering/NocFacadeSheet.cs` (new; `-facade-sheet`).
- `Scripts/Editor/MaterialFactory.Facade.cs` and `Scripts/Runtime/Core/MaterialLibrary.Facade.cs`.
- `Tools/texprep/facade.json` (FacadeArr, RoomArr) and `Tools/qa/facade_sheet.py` (new).
- `Scripts/Runtime/City/CityMesher.cs`: uv1.zw and the skin byte; lead approval.
- `Beats/Beat02_Boulevard/FacadeKit.cs`, `Beats/Beat02_Boulevard/StreetFacade.cs` (podium override) and `Beats/Beat06_Signs/G4Kit.cs`.
- Tests: `FacadeTests.cs`, `FacadeSkinTests.cs` (new), `InteriorTests.cs` (new), `CityTests.cs`, `MaterialTests.Facade.cs`.
- `docs/FACADE_V2.md` (new) and `docs/logs/FA.md`.

**Steps.**
1. **FA-1.**
   - The writers and the skin byte (2.2); regions and skins (2.3).
   - LOOK_V2 window light, and strata and zones keyed per tower (2.4).
   - Whole bays, elements and the macro layer (2.5).
   - Recess with spill and wet sills (2.6).
   - The city term moves to `f.specOcc`.
   - MeanRadiance; the facade sheet.
2. **FA-2.** Glass (2.7).
3. **FA-3.** Interiors (2.8) and mode 1 (2.10).
4. **FA-4.** Calibration: region budgets; crawl thresholds (2.9); the texture_std gate; LOOK_V2 KS03-KS12 and KS16-KS19.
5. **FA-5.** `-facade-v` defaults to 2; delete the v1 path.

**Dependencies:** TP-0, TP-1 (FacadeArr, RoomArr, OverlayArr), PA-1 (tokens and the `_NocFacade.w` schedule), SU-2's `NocWallReflect` (for FA-2), the lead's unfreeze.

**Tests.**
- The H2 tests re-based on v2: w forced to 0 and 1 within 3 %; lit fraction, hue split and toggle rate 0.3 %/s or less; w_f = 0 means no warm window; face id exact.
- FacadeSkinTests (2.2).
- Region budgets within 2 % (2.3); the lit-probability mean within 1 % (2.4).
- Regime energy within 1 %, with the recess checked against a 64 x 64 supersampled ray cast.
- E_room_mean within 3 % of integration up to 60 deg.
- Crawl sweeps at 1 code or less (2.9).
- T1 and T2 mean luma at 1.8 km within 3 % (D-018).
- No NaN at grazing.
- At least two skins on the boulevard slabs.
- MeanRadiance equals the band-mean twin.

**Verification by the lead.**
- 1080p and 4K stills of KS03-KS09, KS11 and KS16-KS19.
- `texpans` and `dolly` at both resolutions; `statics`.
- The facade sheet with `facade_sheet.py`, including the reveal-sign check.
- Debug views 1-6 and 10.
- The `-facade-v` A/B.
- CRIT against CRIT0 #3, #4, #6, #19, #20, #21 and #22.

#### SU: near surfaces, wet floors, glass, lines (owner SU; after TP-0)

**Files.**
- `Shaders/NocSurface.shader`, `NocWet.shader`, `NocGlass.shader`, `NocGraybox.shader`, `NocLine.shader` (moved from PA), `NocGrating.shader` (new), `NocShopfront.shader` (new).
- `Shaders/Include/NocMirror.hlsl`.
- `Scripts/Runtime/Core/NocSurfaceConstants.cs` and the generated `NocSurfaceConstantsGen.hlsl`.
- `Scripts/Runtime/Rendering/NocSurfaceMath.cs` (new: WallRadiance, puddle, leak and line-paint twins), `NocMirrorMath.cs`, `NocWetRegistry.cs`, `NocMaterialSheet.cs`.
- `Scripts/Runtime/Geometry/VertexBake.cs` (`TopDistance`) and `Scripts/Runtime/Core/WorldContext.cs` (the `AddMesh` leak-anchor bake).
- `Scripts/Editor/MaterialFactory.Surface.cs` and `Scripts/Runtime/Core/MaterialLibrary.Surface.cs`. These hold all NocSurface, NocWet, NocGlass, graybox and Line rows, including the new rows of 1.3.
- `Tools/texprep/surface.json`.
- Beat files (8.4):
  - `Beats/Beat01_Alley/G1Kit.cs`, `G1Materials.cs`, `AlleyBlocks.cs`, `AlleyFixtures.cs`, `NoodleBar.cs`, `VendingMachines.cs`, `Beat01_Alley.cs`;
  - `Beats/Beat02_Boulevard/Beat02_Boulevard.cs`;
  - `Beats/Beat03_Edge/Beat03_Edge.cs`;
  - `Beats/Beat05_Garden/Beat05_Garden.cs`;
  - `Beats/Beat07_Heart/OldQuarterBuilder.cs`.
- Tests: `WetFloorTests.cs`, `SurfaceTests.cs` (new), `WetMacroTests.cs` (new), `MaterialTests.Surface.cs`, `Beats/Beat01AlleyTests.cs`, `Beats/Beat07HeartTests.cs`.
- `docs/MATERIALS.md` and `docs/logs/SU.md`.

**Steps.**
1. **SU-1.**
   - NocSurface: mappings, anti-tiling, micro, anchored leaks, wetness.
   - NocWet: puddles and ripples.
   - All rows, including `FloorMoss`, `GlassDark` and the `Fabric` curtain.
   - The beat slot edits and the property contract.
2. **SU-2.** `NocMirror` wall blend and the `NocWallReflect` export; `WallRadiance` and the six call sites (2.11).
3. **SU-3.** Grating, NocLine paint, glass grime and rivulets.
4. **SU-4** (after FA-3's `NocInterior` is stable). `G1Mesh.Quad` UV0 and `NocShopfront`.
5. **SU-5.** Calibration to PA's tokens; `-surf-v` defaults to 2; delete the v1 path.

**Tests.**
- Every module set's tile holds a whole number of modules.
- No rotation for directional classes.
- The puddle normal equals up at m = 1.
- The puddle fraction is 0.35 +- 0.02 per wet row, from the real height histograms.
- Wetness pools in low height.
- Leak anchoring: B equals the distance below the face top on test boxes.
- The overlay mean is preserved under its fade; the micro-detail mean is 1.
- The KS17 mirror point is unchanged (D-012); the wall-plane blend is continuous.
- WetMacroTests: within 5 % (2.11).
- `G1Mesh.Quad` UV0 is metric, and no facade mesh uses it.
- The static `-rain 0` floor change is 2 levels or less.

**Verification by the lead.**
- `-material-sheet` and `texclose`.
- KS00-KS02, KS10, KS12-KS14, KS17 and KS19.
- `texpans` at KS00 and KS17; `tiling.py` on floor and wall masks.
- Debug view 8.
- CRIT against CRIT0 #5, #8, #17, #24, #26 and #29.

#### FO: foliage (owner FO; after TP-0 and TP-1's leaf atlases)

**Files.**
- `Shaders/Beats/NocFoliage.shader`.
- `Beats/Beat05_Garden/GardenTree.cs` (core shell, card hand-off, bark UV0 wrap), `GardenLeafCards.cs` (new), `GardenMaterials.cs` (library foliage and `lib.FloorMoss`; no runtime `Shader.Find`), `GardenDims.cs`.
- `Scripts/Editor/MaterialFactory.Foliage.cs` (`Leaves`, `LeafCore`) and `Scripts/Runtime/Core/MaterialLibrary.Foliage.cs`.
- `Tools/texprep/foliage.json`.
- Tests: `Beats/Beat05GardenTests.cs`, `FoliageTests.cs` (new), `MaterialTests.Foliage.cs`.
- `docs/logs/FO.md`.

**Tests.**
- 135 k cards and 270 k triangles or fewer.
- Cards inside 1.10x the ellipsoid; notch clearance 1 m.
- `AlphaToMask On` in the forward pass; queue 2450; the atlas coverage flag on.
- The core covers the crown minus the card fringe (ray samples).
- Bark UV0 wraps a whole number of tiles.

**Verification by the lead.**
- KS14 and KS15 at 1080p and 4K; `texpans` at KS15.
- The rain edge probe on a crown.
- Beat 5 performance at 1.0 ms or less (the `-foliage-v` A/B).
- V's audit.
- CRIT against CRIT0 #10 and #25.

#### GE: geometry relief (owner GE; after FA-1)

**Files, scope and tests:** section 5.

### 8.4 Beat files: one owner each, and the lines they carry for others

| Beat file | Owner | Owner's change | Lines carried for others |
|---|---|---|---|
| `Beat01_Alley/G1Kit.cs` | SU | `G1Mat` += ConcreteAlley, Wood, TileWall, Fabric, Grating, Shopfront; `G1Mesh.Quad` writes UV0 | - |
| `Beat01_Alley/G1Materials.cs` | SU | slot mapping | PA: tokens at 75-84 and 99 (LOOK_V2 section 10) |
| `Beat01_Alley/AlleyBlocks.cs` | SU | walls to `ConcreteAlley` | FA: tower seeds `SkinOverride(2)` and `(3)` |
| `Beat01_Alley/AlleyFixtures.cs` | SU | catwalk and fire escapes as `Grating` plates | PA: light and blade colours at 19, 29, 171-174, 201 |
| `Beat01_Alley/NoodleBar.cs` | SU | `Wood`, `TileWall`, `Fabric` | PA: colours at 57, 65, 67 |
| `Beat01_Alley/VendingMachines.cs` | SU | screens to `Shopfront` (via slot) | PA: light colours at 15, 51, 61 |
| `Beat01_Alley/Beat01_Alley.cs` | SU | wall macro (2.11) | - |
| `Beat02_Boulevard/FacadeKit.cs` | FA | uv1.zw, skin byte | - |
| `Beat02_Boulevard/StreetFacade.cs` | FA | podium faces `SkinOverride(4)` | - |
| `Beat02_Boulevard/Beat02_Boulevard.cs` | SU | wall macro | - |
| `Beat02_Boulevard/BladeSign.cs`, `MedianPromenade.cs` | PA | light colours | - |
| `Beat03_Edge/Beat03_Edge.cs` | SU | `SectionMacro` to `MeanRadiance` | PA: light colours at 104-110 |
| `Beat03_Edge/TonguePier.cs` | PA | light colours at 189, 251 | - |
| `Beat03_Edge/EdgeMesh.cs`, `GulfFace.cs` | not edited | - | - |
| `Beat04_Skybridge/SkybridgeLamps.cs`, `SkybridgeMaterials.cs` | PA | LampWarm; MetalPaint rails | - |
| `Beat05_Garden/Beat05_Garden.cs` | SU | wall macro | PA: LeafGlow light at 36 |
| `Beat05_Garden/GardenTree.cs`, `GardenLeafCards.cs` (new), `GardenMaterials.cs`, `GardenDims.cs` | FO | crowns, bark UV0, library materials | - |
| `Beat06_Signs/G4Kit.cs` | FA | FacadeQuad and FacadeBox uv1.zw, skin byte | - |
| `Beat06_Signs/ArcadeBuilder.cs`, `PromenadeBuilder.cs`, `NocScreenMath.cs` | PA | colours, screen ramp | - |
| `Beat06_Signs/ScreenWallBuilder.cs` | PA | fallback and mean colour | FA: mass `SkinOverride(1)` |
| `Beat07_Heart/OldQuarterBuilder.cs` | SU | `Brick`, `Plaster`, `GlassDark`, `Fabric` curtain, wall macro | PA: metal colour at 195 |
| `Beat07_Heart/PlazaFurnitureBuilder.cs` | PA | lamp and metal colours | - |
| `Beat07_Heart/SpireBuilder.cs` | GE | spire geometry | FA: `SkinOverride(0)`; PA: strip and base colours at 109-110 |
| `Beat07_Heart/KoiBuilder.cs`, `KoiDriver.cs`, `KoiShaderMath.cs` | KO | koi | - |
| `Beat07_Heart/KoiMath.cs` | V (unchanged) | - | - |
| `Beat07_Heart/RingTowersBuilder.cs` | not edited | random seeds are remapped inside `G4Kit` | - |

**Everything else is textured through the library.** Beat 2 street level, Beat 3, Beat 4, the Beat 6 promenade and arcade, and the Beat 7 plaza and ring towers become textured through the library materials they already use. The emissive variants that beats derive with `new Material(lib.Concrete)` or `new Material(lib.SteelPainted)` (lamp heads, the tunnel) inherit the textures under their emission. That is acceptable where emission dominates. The two cases where it is not acceptable (the Old Quarter panes and the goal curtain) are fixed in SU (3.5).

---

## 9. Budgets and the cut ladder

| Item | Budget | How measured |
|---|---|---|
| Texture VRAM | about 405 MiB planned, plan cap 900, hard cap 2 048 | TextureTests (sum from formats); `Tools/vram-sample.ps1` in the player |
| Player memory | 4 GB or less at 1080p, 6 GB or less at 4K | `vram-sample.ps1` over the cinematic autotest |
| Load time | 15 s or less | the autotest report's load timer |
| Frame time, RTX 5070 | 1080p: 90 fps average or more, no frame above 33 ms. 4K: 60 fps average (soft), 33 ms or less | PerfMonitor over the 600 s cinematic (7.7) |
| GPU per pass, 1080p | facades 2.0; near surfaces 1.2; wet floors 1.0; foliage 1.0 (beat 5); sky, neon, streaks, rain and grade 3.0; **total 8.2 ms** or less | the flag A/Bs and per-beat segments |
| GPU per pass, 4K | facades 5.0 (texturing 2-4 of it); near 3.0; wet 2.5; foliage 2.5; the rest 3.5; **total 16.5 ms** or less | as above |
| Overdraw | NocFacade 1.5 or less at KS04, KS05, KS09 and KS17, else the prepass | debug view 9 |
| Shader ALU (design limits) | NocFacade 1.8 k near, 1.2 k far; NocSurface 0.45 k + 20 taps; NocWet 2.4 k + 14 taps; NocFoliage 0.3 k + 4 taps | estimates; the performance gates decide |
| Shader variants | no new `multi_compile` or `shader_feature`; forward variants equal TP-0's committed baseline | Validate |
| Samplers per stage | 8 or fewer (D3D11 limit 16) | review and the Validate compile |
| Triangles added | GE 400 k or fewer, FO 270 k or fewer | tests and the build log |
| Repository | processed PNGs about 0.5 GB, committed at most 3 times | `git count-objects -vH` |
| Import | first import of all sets about 3-6 min | lead log |

**Cut ladder.** Use it when performance or shimmer fails, in this order:
1. textured rooms down to box rooms;
2. the furniture plane;
3. glass rivulets;
4. micro detail on near surfaces;
5. overlay hex down to plain tiling;
6. skyline speckle down to building means;
7. facade aniso 16 down to 8;
8. near-surface hex down to plain tiling with macro variation;
9. `_NocTexDetail` 0 beyond 150 m.

**Never cut:**
- the photo base sets on near surfaces;
- the analytic facade structure, the whole-bay rule and the macro layer;
- recess rectangles and spill;
- Toksvig mips;
- puddles from height;
- the leaf-card crowns.

---

## 10. Risks and their tests

| # | Risk | Prevention | Test (gate) |
|---|---|---|---|
| R1 | Facade moire and crawl at 2-40 px periods, worst at grazing (Gulf, KS05) | S1-S7, crawl-derived thresholds, bounding-box footprints, aniso 16, Toksvig | crawl sweeps at 1 code or less (2.9); `texpans` at 0.5 px per frame: temporal std at most 2.0 levels above the `-capture-ss 2` reference |
| R2 | Popping at regime boundaries | E1-E7; smoothstep on pixel measures | `dolly`: difference spike at most 3x the running median, mean-luma step at most 1.2 levels per frame; regime energy within 1 % |
| R3 | Visible tiling | hex, module and panel shuffle, height hex; macro noise; overlays; per-module and per-panel tone | `tiling.py`: autocorrelation peak at the tile period and at the hex period at most 0.25 of zero lag |
| R4 | Seams | `SampleGrad` with true gradients; seams inside joints; corner trim; whole bays; padded decals; integer bark wrap | NocTexturingTests continuity; seamScore; `texclose`; CRIT |
| R5 | Wrong facade frame (inverted reveals and parallax) | per-pixel sigma, projected B | FacadeSkinTests; debug view 4; the facade-sheet reveal sign |
| R6 | Floor lines break at corners; half windows | strata and zones per tower; whole bays | facade-sheet autocorrelation; CRIT |
| R7 | Texture invisible in the video | spill, wet sills, per-panel tone, contrast normalisation; ambient no longer crushed (specOcc) | texture_std after encode, 1.5 codes or more at 75-300 m; LOOK_V2 8.2 check 7 |
| R8 | Flat walls at 1-3 km | macro layer (2.5) | facade sheet at 1 000 and 3 000 m: group and stripe contrast present; CRIT0 #20 |
| R9 | Specular aliasing on wet, normal-mapped floors and glass | Toksvig, projected-space filtering, footprint AA, L_eff cap, flat puddles, tilt variance into roughness | static `-rain 0` change of 2 levels or less; isolated-pixel test |
| R10 | Baked light, casts or violet in photos | TEX-R's choice of sets; bake fit; the chroma and violet gates | texprep gates; previews; CRIT |
| R11 | Stains removed by processing | de-light off by default; content gate | texprep content gate |
| R12 | Metallic misuse | allow-list | TextureTests |
| R13 | SRP Batcher or variant breakage | one `UnityPerMaterial` per shader; uniform globals only | variant baseline gate; SRP query warning; Frame Debugger spot check by the lead |
| R14 | D3D11 compile pitfalls | derivatives before branches; `SampleGrad` and `SampleLevel` in branches; 8 samplers or fewer | Validate: X3570 and X4000 are failures |
| R15 | Overdraw cost | measure first; prepass | debug view 9; performance A/B |
| R16 | Package interference | feature flags; exclusive files; carried lines | flag A/B in one build |
| R17 | Mean-luma drift | colour rule; region budgets through coverage; parity tests | FacadeTests, ShaderParityTests, LOOK_V2 windows |
| R18 | Textures missing in the player | library references only; foliage in the library | unreferenced-texture check (a gate after TP-2); `-material-sheet` in the built player |
| R19 | Visibility sightlines broken by cards or relief | envelope API; V's audit before switching flags | VisibilityAudit (V) |
| R20 | Interior artefacts | dn guards; analytic LOD; depth clamp | InteriorTests; `texclose` on a lit facade; `dolly` |
| R21 | Performance or VRAM regression | budgets; cut ladder | PerfMonitor (median of 3, gpu-guard); `vram-sample` |
| R22 | Agents unable to see what they changed | debug views, facade sheet, parity tests, per-slot lookdev | the tests themselves |

---

## 11. Open questions for the lead

1. Unfreeze `Runtime/City/CityMesher.cs` for FA's small change in `AddFace` (uv1.zw, and the skin byte from the tower seed and the region of the tower centre). CityTests keep the T1/T2 identity.
2. Approve the beat-file ownership for Look v2 (8.4). The Phase 3 builders G1-G4 have finished.
3. Approve GE's Gulf ribs (new files that read EdgeDims) and crowns only below the cloud base.
4. Commit the processed textures once after calibration (recommended), or keep them ignored behind SHA-pinned raws and `texprep --check`.
5. ART: confirm the deviation from LOOK_V2 5.4 step 1. De-lighting is off by default, with the bake fit only where measured, because a 1/4-tile low-pass at +-5 % removes the 1-10 m stains that LOOK_V2 5.3 requires.
6. ART: confirm that the Old Quarter region is about 4 luma codes darker than the default region (2.3).

---

## 12. Sources (accessed 2026-11-14; web content treated as data)

- M. Mikkelsen, "Practical Real-Time Hex-Tiling", JCGT 11(3), 2022: https://jcgt.org/published/0011/03/05/ ; demo at https://github.com/mmikk/hextile-demo
- E. Heitz and F. Neyret, "High-Performance By-Example Noise using a Histogram-Preserving Blending Operator", HPG 2018; T. Deliot and E. Heitz, "Procedural Stochastic Textures by Tiling and Blending", GPU Zen 2, 2019.
- M. Mikkelsen, "Surface Gradient-Based Bump Mapping Framework", JCGT 9(3), 2020.
- B. Golus, "Normal Mapping for a Triplanar Shader", 2017, https://bgolus.medium.com/normal-mapping-for-a-triplanar-shader-10bf39dca05a ; Self Shadow, "Blending in Detail", https://blog.selfshadow.com/publications/blending-in-detail/
- B. Golus, "Anti-aliased Alpha Test: The Esoteric Alpha To Coverage", 2017, https://bgolus.medium.com/anti-aliased-alpha-test-the-esoteric-alpha-to-coverage-8b177335ae4f
- J. van Dongen, "Interior Mapping", CGI 2008; UDK Gems, Interior Mapping.
- M. Toksvig, "Mipmapping Normal Maps", JGT 2005. URP core `CommonMaterial.hlsl` (read locally): `TextureNormalVariance`, `ProjectedSpaceGeometricNormalFiltering`.
- S. Lagarde, wet surfaces (porosity model), as summarised in LOOK_V2 5.2.
- Unity manual and scripting reference:
  - SRP Batcher material requirements;
  - `TextureImporterSettings` flipbook rows and columns;
  - `AssetPostprocessor.OnPostprocessTexture2DArray` and `AssetImportContext.DependsOnSourceAsset`;
  - texture formats by platform;
  - `QualitySettings.anisotropicFiltering` (ForceEnable) and `Texture.SetGlobalAnisotropicFilteringLimits` (forced minimum 9).
- ambientCG and Poly Haven set pages as recorded in `docs/TEXTURES.md` (TEX and TEX-R).
- Project sources:
  - `QA/critique-lookv2-baseline.md` (CRIT0);
  - `docs/LOOK_V2.md`;
  - `docs/TEXTURES.md` (TEX-R final sets);
  - `docs/research/R1_wet_facade_interior.md`;
  - DEVELOPMENT.md D-003, D-004, D-005, D-008, D-012, D-017, D-018, D-027.

---

## Appendix A. Review log (TECH-C review of the draft; each amendment checked against the code)

**Verdicts:**
- **Applied:** the amendment is in the plan as proposed.
- **Applied, corrected:** it is in the plan, but the code showed part of it to be inaccurate, and that part was fixed.
- **Partly applied:** part of it was rejected, with the reason given.

| # | Kind | Verdict | Where | Reason, and what the code showed |
|---|---|---|---|---|
| 1 | MUST | **Applied, corrected** | 1.6, 2.2 | Confirmed:<br>- face A n = (-s, 0, s dx) with RecedeSign -1 (`GulfFace.cs:77-82`, `EdgeDims.cs:135`) while u = ArcLength grows with z (`EdgeDims.cs:188`);<br>- notch wall n = -z with u = x (`GulfFace.cs:230-231`);<br>- both hull sides use u = x (`TonguePier.cs:93-102`), with normals from a cross product (`HullNormal`).<br>sigma adopted. **Corrected:** B is the projected world up, not cross(n, T), which points down on sigma = -1 faces. The vertex test is FacadeSkinTests. |
| 2 | MUST | Applied | 2.2 | `EdgeMesh.cs:43-60` and `GulfFace.cs:250` write (uMin, uMax, 0, 0) with G = 0. They stay unedited (G2's files), so y0/y1 = (0, 0); the bands that are off and the skins for G = 0 and mode 1 are stated. G4Kit sets `CurrentUV1` twice because the bottom and top vertices can differ; both carry y0/y1. |
| 3 | MUST | **Applied, corrected** | 2.11 | Confirmed that nothing calls `NocFacadeMacro` or `NocFacadeMath.Macro`, and that `NocMirror` uses `NocWallMacro` (`NocMirror.hlsl:137-144`). Six of the seven sites are hard-coded wall macros; SU owns them, and there is a 5 % test. **Corrected:** `ScreenWallBuilder.cs:71` registers `NocScreenMath.MeanEmission`, the screen wall's own twin, not a facade mean. It needs only the test. Kind 1 sites use a separate `WallRadiance` helper. |
| 4 | MUST | **Applied, corrected** | 2.4, 3.3, 8.3 | New constant names in `NocFacadeConstants` and `NocSurfaceConstants`. The `NocturneConstants` lines stay frozen (CityField reads FacadeEdgeFraction). **Corrected:** `_NocFacade.z` is the scene time (`NocFacade.hlsl:19`), so n_f goes to `.w` and w_f stays in `.y`. The knots are in PA's file, and the dependency is listed in both PA and FA. |
| 5 | MUST | Applied | 2.2 | `StreetFacade.cs:137` already seeds `seed + (k + 1)`. The per-slab edit is dropped; PCG runs before thresholding; a test asserts more than one slab skin. |
| 6 | MUST | Applied | 8.3 TP-0, FA-1 | Confirmed `NocFacade.shader:63-64` and `NocLighting.hlsl:127`. The occlusion split is in TP-0, with no picture change; FA moves the city term. `NocLighting.hlsl` moves from PA to TP-0 (PA needs only constants there). |
| 7 | MUST | Applied | 3.4 | All the listed properties were confirmed by grep. `_Tile` stays a Float and `_TileV` is added. SU provides `FloorMoss`, and GardenMaterials uses it. |
| 8 | MUST | Applied | 3.5, 2.2, 8.4 | (a) `GlassDark`; (b) `Fabric` curtain; (c) `Grating` row and shader; (d) `G1Mesh.Quad` writes panel-local UV0 itself (it calls `FlatQuad` without UVs, `MeshBuilder.cs:92-93`), so no call-site edits are needed, plus `NocShopfront`; (e) NocLine axial paint (posts 0.20, `G1Materials.cs:19`, and 0.18, `PlazaFurnitureBuilder.cs:47`); (f) overrides 0xF0-0xFF with the builder files listed (spire 0xA7, `SpireBuilder.cs:35`). |
| 9 | SHOULD | Applied | 1.5, 8.3 | `BatchTools.cs:290` checks only useSRPBatcher. The variant baseline is committed first, and the SRP query is a warning. The aniso source is `QualitySettings.asset:24` (no script writes it); the forced minimum of 9 is in TextureTests. |
| 10 | SHOULD | Applied as a test | 2.2 | `MeshBuilder.Append` renormalises UV1.xyz (`MeshBuilder.cs:119-135`), and MeshKit tubes rely on that (`MeshKit.cs:258`), so it cannot be removed. FacadeSkinTests' range checks catch any damage, and TP-0 adds a warning comment. |
| 11 | MUST | Applied | 8.3 TP-0, 4 | `NOC_FRONT_FACE` added. `NOC_ALPHA_OUT` applies to the forward pass only: depth priming is disabled (`PipelineConfigurator.cs:97`) and the depth texture is off. |
| 12 | MUST | Applied | 2.4 | Confirmed the per-(face, band) stratum (`NocFacade.hlsl:42`) and the per-face zone hashes (57-61). Strata and zone rows are now keyed by tower key and band; zone columns stay per face. |
| 13 | MUST | Applied | 2.5 | Whole bays inside [uMin + 0.6, uMax - 0.6] x [y0 + base, y1 - parapet]; the suppressed cells are counted in the band means. |
| 14 | MUST | Applied | 2.5 | Macro layer and pixel table at 30, 300 and 3 000 m, at 1080p and 4K. |
| 15 | MUST | Applied | 2.6, 7.2 | Spill, wet sills and per-panel tone +-12 %; texture_std gate after `encode_test.py`. |
| 16 | SHOULD | **Partly applied** | 2.8, 2.9, 9 | Applied: rooms, furniture, rivulets and micro detail moved to the top of the cut ladder; the 5-40 px cues built; effort aimed at 50-1 000 m (no facade micro detail at all). **Rejected:** Facade018B's Emission as a window atlas. Measured, its 36 windows are flat gradient rectangles (per-window mean 0.08-0.32, no rooms, curtains or blinds), and TEX-R demoted the set to reference only. |
| 17 | SHOULD | Applied | 2.3, 5 | No S4 loggias in corner bays or under the parapet. GE is a scheduled, owned package. |
| 18 | MUST | Applied | 1.3, 1.6, 2.5 | Facade slices are joint-free (TEX-R) or registered to the grid (courses, lifts, slabs per floor and bay). The cell-aligned panel shuffle puts seams inside analytic joints. Facades no longer use hex. |
| 19 | MUST | Applied | 2.7 | 0.061 deg/px confirmed [computed]. Reflected skyline with per-building speckle pitch; registered wall planes in the canyon and the Gulf. |
| 20 | SHOULD | Applied | 2.9, 10 | Crawl sweeps of 2-8 px at 0.25 px, at 1 code or less, set every threshold. tiling.py checks the hex period. |
| 21 | SHOULD | Applied | 3.3 | NocSurface uses only colour R and G (`NocSurface.shader:103, 109`), so B is free. The bake runs in `WorldContext.AddMesh` for materials with `_LeakAnchor`, using `VertexBake.TopDistance`. This avoids editing every kit. |
| 22 | MUST | **Applied, corrected** | 1.3 | Tile = TEXTURES.md value within +-15 %, but from TEX-R's final table, which supersedes the values the review cited:<br>- granite slabs are 0.46 m, not 0.77 m;<br>- brick_wall_001, metal_plate, bark_brown_02 and wood_table_001 are replaced by brick_wall_08 (1.9), DiamondPlate008C (0.6), trident_maple_bark (1.8) and fine_grained_wood (0.6).<br>Diamond plate never goes on `SteelBare` or `Brass`. It stays on the skybridge deck plates at its true 0.6 m tile, as TEX-R assigns, because the review's objection was the 2.4 m scale. |
| 23 | MUST | **Applied, corrected** | 1.4 step 6 | The allow-list follows TEX-R's sets: MetalPlates013, MetalWalkway006, corrugated_iron_02 and DiamondPlate008C (metal_plate is rejected). Measured: Metal046B's metalness is 86 % white, but it is used as paint and cladding, so metallic is 0; Leaking012A is 100 % white and only its roughness is used. |
| 24 | MUST | Applied | 1.4 step 4 | Default strength 0. TEX-R's bake fit is the measured-illumination step, for sets with bake above 0.10; there is a ramp guard and an opt-in repeat flattening (plastered_wall_04). Contrast follows LOOK_V2 5.1 and chroma 5.4.3; the 0.06-0.94 clamp and chromaKeep are removed. The deviation from LOOK_V2 5.4 step 1 goes to ART (question 5). |
| 25 | MUST | Applied | 1.3 | Resolved by TEX-R's sets and substitutions:<br>- anodised: constant;<br>- spandrel: dark glass plus grime;<br>- louvre: analytic blades;<br>- rust bleed: MetalPlates013;<br>- carpet: Carpet016; ceiling: OfficeCeiling001;<br>- pavers: concrete_tiles_02;<br>- flagstones: large_floor_tiles_02 with module shuffle and moss joints.<br>Leaking003 (2048 x 1024, measured) is padded, not stretched. |
| 26 | SHOULD | Applied | 1.2, 8.2 | FacadeArr is 1024 (32 MiB). Processed textures are committed once after calibration, at most 3 times. |
| 27 | SHOULD | Applied | 1.5 | `DependsOnSourceAsset` on the sibling normal; the normal-array spike with the BC5 fallback; TextureTests read the imported last mip. |
| 28 | MUST | Applied | 2.4, 2.7 | LOOK_V2's window, lit fraction, factors, zones, frames, tilt, curtains and blinds, stand-by glass and roughness are adopted. f_skin is dropped; the budget is reached through coverage. |
| 29 | MUST | Applied | 2.3 | The shares sum to 100 % in each region. Regions are discs chosen per tower by the writer, and the budget is recomputed per region. |
| 30 | MUST | Applied | 2.12, 8.3 | Overdraw is measured first at KS04, KS05, KS09 and KS17; above 1.5, a ZTest Equal prepass is added in TP-0. |
| 31 | SHOULD | Applied | 2.12, 9 | Per-cell and per-pixel work is stated; aniso cost modelled; 4K per-pass budgets; median-of-3 perf under gpu-guard; PerfMonitor fixed. |
| 32 | SHOULD | Applied | 4 | Recomputed from `GardenDims.HeroTable`: 85 k hero cards plus 50 k or fewer back cards (135 k, 270 k triangles); leaves at 1.0-1.3x true size; early-Z limits stated; coordinated with V. |
| 33 | MUST | Applied | 7.1 | Ten debug views through a uniform global, decoded in Python. |
| 34 | MUST | Applied | 7.2 | `-facade-sheet` (FA) and `facade_sheet.py` with all four measures. |
| 35 | MUST | Applied | 7.3, 7.4 | lookdev per slot, CaptureInEditor, CompileCheck; GPU parity harness. |
| 36 | MUST | Applied | 1.6, 7.6 | `-facade-v`, `-surf-v`, `-foliage-v`. |
| 37 | SHOULD | Applied | 8.1, 8.3 | PA starts now (its generated headers already exist). NocTexRecipes retirement is in TP-2, and the unreferenced check is a warning until then. |
| 38 | SHOULD | Applied | 7.5, 8.3 | `-capture-ss 2` in CaptureHarness; fixed dolly poses; ShaderIds in TP-0; MaterialTests split per partial. |
| 39 | SHOULD | Applied | 6, 8.3 | The koi gets its own package (KO); `KoiMath.cs` stays V's. |
| 40 | SHOULD | Applied | 1.6, 4 | `_Mapping` 1 (UV0 metres, derivative tangent frame). Loft UV0 is metric (`MeshKit.Loft.cs:42`). Added: FO rewrites the ring u so a whole number of tiles wraps the perimeter, with no seam. |

**Summary:** 40 amendments (26 MUST, 14 SHOULD). 39 are applied: 5 with factual corrections (1, 3, 4, 22, 23) and 1 as a test rather than a code guard (10). 1 is partly applied (16: the window atlas is rejected).

---

## Appendix B. Changes forced by TEX-R (commit 72ade9d)

The draft predates TEX-R's review of the sets. These changes follow it:

- **Facade skins.**
  - The draft's precast, metal-panel, ceramic-tile, brick and plaster slices now use concrete_floor_worn_001, Metal046B, rectangular_facade_tiles, brick_wall_08 and plastered_wall_04 (new slices 0, 2, 4, 5 and 6).
  - concrete_panels, blue_metal_plate, brick_wall_001 and damaged_plaster were rejected. Reasons: baked side light, a beige cast, their own 2 m joints, saturated blue or orange, and repeating patches.
  - The draft's slices for anodised aluminium, spandrel glass and louvres have no photo set and become analytic.
  - FacadeArr shrinks from 12 to 8 slices, and glass grime moves to OverlayArr.
- **Reference only.** The Facade0xx sets are not processed.
- **Near rows.**
  - `Concrete`: concrete_floor_worn_001.
  - `SteelPainted`: Metal046B.
  - `SteelBare`, `Brass`: MetalPlates013.
  - Deck: DiamondPlate008C.
  - Pavers: concrete_tiles_02.
  - Plaza: granite_tile at 0.46 m slabs.
  - Garden path: large_floor_tiles_02.
  - `Bark`: trident_maple_bark.
  - `Wood`: fine_grained_wood.
  - `Fabric`: poly_wool_herringbone.
  - `Brick`: brick_wall_08; `Plaster`: plastered_wall_04.
  - `Moss`: Moss001 at 0.45 m.
- **Room atlas.** Defined from TEX-R's class R.
- **texprep.** The bake fit (TEX-R step 4b), the violet-after-white-balance gate, height from normal integration for fine_grained_wood, and squaring of poly_wool_herringbone.
