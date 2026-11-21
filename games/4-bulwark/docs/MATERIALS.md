# MATERIALS — Bulwark material sheet (MASTER_PROMPT 8.11.2)

One row per material named in Part 14 (14.2.6, 14.5), including the families other owners build later. Real-world
reference values first, then what the game uses, the noise recipe (scale ladder of 8.11.3: micro mm-cm, meso cm-m,
macro m-tens of m) and the capture that shows it. Albedo is linear unless marked sRGB; "smoothness" is Unity's
(1 - perceptual roughness). Colours only from `BulwarkPalette` tokens (14.2.1); the texture sets are generated around a
reference token and recoloured in the shader by target / reference (`BwRecolour`), so the palette stays the only
colour source.

## 1. How the library works (one generator, one lit model)

- **Generator:** `Runtime/Rendering/BwTexGen.cs` (pure C#, deterministic, periodic noise toolbox: value, Perlin
  gradient, fBm with anisotropic periods, ridged, Worley/Voronoi, domain warp, curl, box blur) and
  `Editor/TextureFactory.cs` (writes the sets as level-0 PNGs under `Assets/_Game/Textures`, called by
  `MaterialFactory.Build`, so `RebuildAll` regenerates them; skipped when the manifest names the current
  `BwTexGen.Version`; about 8 s for all four sets). Per set: Albedo (sRGB, BC7), Normal (tangent RG from the height by
  tileable central differences, BC5), Mask (R metallic potential, G occlusion incl. a blurred-height cavity term,
  B height, A smoothness; BC7, the Crust mask RGBA32 because its R channel holds exact per-shell ranks). Mips are our own: 2 x 2 box, normals averaged without renormalisation and the
  smoothness of every mip Toksvig / vMF filtered (URP core `TextureNormalVariance` + `NormalFiltering` form, added
  roughness at most 0.5). Repeat wrap, trilinear, aniso 8 (QualitySettings forces aniso on). Wave B (integrate): the
  sets are PNG files (`Bw<Set>_<Albedo|Normal|Mask>.png`, level 0 only, 25.7 MB on disk instead of 56 MB of text hex
  rewritten per regeneration); `BwTexturePostprocessor` (TextureFactory.cs) sets the importer in code (sRGB for the
  albedo only, mipmaps, repeat, trilinear, aniso 8, not readable: no CPU copy in the player, Standalone BC7 / BC5 / the
  Crust mask RGBA32) and replaces the importer's mips by `BwTexGen.MipChainFromLevel0`, which rebuilds the same chain from
  the 8-bit level 0 (linear-light albedo, renormalised normals, per-level Toksvig smoothness from the Normal PNG; within
  2 codes of `BuildMips`, `MaterialTests.TexGen_MipChainFromLevel0MatchesBuildMips`). The PNGs change only when
  `BwTexGen.Version` (or the manifest format line) changes; the legacy `.asset` files are deleted by `RebuildAll`.
- **Sets and VRAM** (8.11.9): Concrete 2048 px per 2.0 m tile (about 1 mm per texel), Road 1024 per 1.0 m, Steel 1024
  per 1.0 m, Crust 1024 per 0.5 m: 32.0 MB of GPU memory in total (RebuildAll log line `TextureFactory: 4 sets
  written ... GPU size 32,0 MB`), far under the 2 GB budget.
- **Sampling:** world-space triplanar (`BwLitCore.hlsl`: `BwTriSetup`, `BwTriSample`, `BwTriNormal` with Golus'
  whiteout blend). The plane coordinates stay UNWRAPPED in the frame and are wrapped by 64 tiles only at the fetch
  (`BwWrap64`, exact because the sets tile per unit), with `SAMPLE_TEXTURE2D_GRAD` derivatives from the unwrapped values.
  Large surfaces take a second, rotated (about 37 deg) and rescaled copy of the same set (`BwTriDetune`, applied to the
  unwrapped coordinates) mixed 33-60 % by a world mask, never one pure tile (Part 10 #16). Correction of the Wave A
  first round: the copy used to be rotated AFTER the wrap, so it jumped by k R (64, 0) tiles (19.08, 14.31 at
  k 0.3727) on every wrap line (x = 0 along the whole crest, y = 0, z = 128 k for the 2 m sets, z = 64 k for the road):
  a straight seam (Part 10 #33). The C# twin `BwTriTwin` and `MaterialTests.Triplanar_DetunedCopyIsContinuousAcrossEveryWrap`
  now prove continuity across x = 0, y = 0, z = 64 tiles and 128 tiles for every k in use (the same test shows the old
  form's jump). Meshes need no UVs or tangents. Vertex colour: R = baked AO, G = edge wear (MeshKit chamfers), B =
  family mask (bands, drip source).
- **World-space macro fields** (`BwCommon.hlsl`): 3D lattices only. `BwFbmFiltered3` (value noise) for unthresholded
  tone and weathering-smoothness fields, `BwFbmGrad3` (gradient noise, 12 cube-edge gradients) for thresholded masks
  (crust colonies, foam, zone wobble). Both fade every octave whose cell is under 2-4 footprints to its mean. A 2D domain
  such as (p.xz + p.y) has a null direction: on walls containing it the noise turns into 4:1 streaks (seen on the beat-4
  crust wall at yaw 300 in round 9); thresholded value noise reads as square blocks (crust colonies, foam in round 10).
  `BwMacroSigned3` stretches the fBm to about +-1 at its p5 / p95 (3D fBm p5..p95 = 0.5 +- 0.22).
- **Lit model** (`BwLighting.hlsl`, verified against URP 17.6 `Lighting.hlsl` 48-96 and 302-400, `BRDF.hlsl` 50-172,
  `GlobalIllumination.hlsl` 423-566): URP `BRDFData` (`InitializeBRDFData`, metallic workflow, F0 0.04 for
  dielectrics) and `DirectBRDFSpecular` (GGX), `EnvironmentBRDF` / `EnvironmentBRDFSpecular` with URP's Fresnel
  `Pow4(1 - NoV)`; reflection source = analytic environment `BwSkyColor(dominant direction)` sinking into the ground
  ambient below the horizon and blending to the trilight ambient as roughness rises and indoors (`_BwSurface.y`),
  the same function as the sky and the fog, so reflections never lag the sky (Part 10 #38). Specular AA (8.11.6):
  smoothness capped at 0.95, URP core `ProjectedSpaceGeometricNormalFiltering` (variance 0.15, threshold 0.2) on the
  shading normal, specular term clamped at 100 and the specular radiance per light at 16 (linear HDR). Diffuse AO,
  Lagarde specular occlusion, horizon occlusion on the geometric normal. `BwFogApply` last in every colour pass.
- **Pattern filtering:** every procedural line or dot is filtered by the footprint ACROSS it, from `abs(ddx) + abs(ddy)`
  of the position on its own axis (never the isotropic distance footprint, which under-filters walls seen at grazing
  angles by the inverse cosine), and faded once its spacing drops under a few pixels along that axis.

## 2. Rows

Columns: reference (with source keys of section 3) | game values | recipe | capture (`QA/captures/wa-mat-NN/`, the
"materials" capture set: `-capture-set materials`).

| Material (field, shader) | Real-world reference | Game: albedo / metallic / smoothness / normal | Noise recipe (micro / meso / macro) | Capture |
|---|---|---|---|---|
| **Concrete** (`Concrete`, BwConcrete) | Gray-cement concrete: new pavement albedo 0.35-0.40, aged 0.20-0.30 [LBNL, XU]; physicallybased.info concrete base colour 0.51 [PBI]; new concrete sRGB 168-186 (0.40-0.55), aged sRGB 122-147 (0.20-0.30) [BAR]; weathering -0.06, soiling -0.05 albedo [LBNL]; dielectric, F0 0.04 [UE, PBR]; rough: smoothness 0.2-0.4 typical for formed, weathered concrete [BAR, UNI] | ConcreteMid 0.212 / 0.223 / 0.220 (aged end of the range), texture R 0.135-0.238 mean 0.212; metallic 0; smoothness 0.11-0.41 in the set (p5-p95 spread 0.104; 0.147 at mip 5 with the macro term), +-0.08 macro at 23 m, wet 0.72; normal from 4 mm of relief over 2 m (bug holes 1.2-3.8 mm deep pits) | micro: paste speckle fBm (384 per tile, 5 mm), bug holes (Worley pits in 31 mm cells, only inside low-frequency clusters), faint 18 mm stones (Worley F1, no cell edges); meso: plywood grain (anisotropic fBm 6 x 96), 14 cm paste mottling, 50 cm pour undulation, formwork panels 3.0 x 1.5 m with 12 mm gaps and tie holes on 0.6 m (cast faces only, off on curved surfaces): each joint train box-filtered with the per-axis footprint across it and faded between 4 and 8 px of its own spacing; tie holes = disc with a directional edge width up close, equal-area square as the product of two 1D box filters once the footprint passes 1.5 r, faded below 4 px spacing; drip stains below each joint (faded by the footprint across the streaks), per-panel pour tone +-2.5 %; macro: tone +-6 % at 40 m and smoothness +-0.08 at 23 m (3D footprint-filtered fBm), second rotated 5.37 m sample mixed 33-60 %; sun tint x1.9 = the main light's chroma x1.9 around its luminance, capped at a channel ratio of 0.6 (noon key: k 1.86; the already saturated gold, storm and dusk keys: k 1) | `mat_b02_crest_close1`, `mat_b01_gallery_*`, `mat_b03_spillway_close1`, KS04 / KS05 / KS06 (`wa-mat-keys15`), `seq_matctx_pan_ks05_leeface`, `seq_matctx_pan_ks06_parapet`, `seq_matctx_dolly_ks06_parapet`, `seq_matctx_pan_gallery`, `seq_matctx_dolly_gallery` |
| **ConcreteWet** (`ConcreteWet`, BwConcrete `_Wet` 1) | Wetting lowers concrete albedo by 0.23 on average [LBNL]; wet rough dielectrics darken by total internal reflection in the water film [ANG]; water F0 0.02 (n 1.333) [PBI] | ConcreteWet 0.037 / 0.045 / 0.053 recoloured from the set; smoothness 0.72 x (0.85 + 0.15 (1 - height)) (lows wetter); normal flattened 60 % by the film | as Concrete, the film fills the micro relief | `mat_b01_gallery_close1`, `mat_b03_spillway_row`, `mat_b07_tunnel_*` |
| **ConcreteDark** (`ConcreteDark`, BwConcrete) | Dark basalt-aggregate concrete / basalt: albedo 0.08-0.15 [BAR]; Unity's floor for non-metals about 0.04 linear (sRGB 50) [UNI] | ConcreteDark 0.070 / 0.078 / 0.084 (palette, darker than Unity's tolerant floor by intent: "dark basalt", 14.2); smoothness as Concrete | as Concrete | `mat_b02_crest_row`, `mat_b07_ledge_*` |
| **Road** (`Road`, BwRoad) | Concrete pavement 0.35-0.40 new, 0.20-0.30 aged [XU]; fresh asphalt 0.043 [PBI] (not used: the spec's road is ConcreteMid); broom finish: transverse grooves about 3 mm deep at 10-20 mm spacing (pavement practice); white markings: luminance factor of EN 1436 class B2-B5 [EN1436] | ConcreteMid, set R 0.160-0.250 mean 0.206; smoothness 0.21-0.44 (broom ridges polished by wear; p5-p95 0.124) +-0.08 macro at 23 m, wet 0.72; wet albedo goes `_WetAlbedo` 0.8 of the way to ConcreteWet (a broomed deck drains; wet slab luma 89 in the beat-2 storm sheet, LightCal 85-115); paint LaneWhite 0.687 / 0.665 / 0.597, smoothness 0.45; markings: edge lines 0.15 m at x +-5.0, dashed centre 3 m / 6 m, chainage numerals 0.7 x 1.2 m stroke 0.15 (01..09 at z 100..900 lee lane x -3.0, 01..08 at z 150..850 sea lane x +3.0; the lee "01" at z 100 lies under the beat-1 gallery, whose floor is not BwRoad, so the walker sees 02..09 and 01..08 on the deck: logged decision) | micro: broom grooves (ridged anisotropic fBm 2 x 128 per metre, domain warped), aggregate exposure (Worley 12 mm) where worn; meso: wear polish (3 per metre), stains; macro: tone +-6 % at 40 m, kerb grime within 1.6 m of x +-5.6; markings exact box-filtered rectangles (dash train triangle-filtered), numerals faded below 3 px | `mat_b02_crest_road_near`, `_road_top`, `_road_far`, `seq_mat_pan_road`, `seq_mat_pan_floor_far` |
| **Steel** (`Steel`, BwSteel) | Painted steel is a dielectric (paint sits on the metal) [UE, UNI]; bare iron base colour 0.530 / 0.513 / 0.494, metallic 1 [PBI]; industrial semi-gloss enamel, weathered: smoothness 0.4-0.6; rust 0.181 / 0.057 / 0.017, dielectric, rough [PBI, UNI] | SteelPaint 0.112 / 0.144 / 0.171 (set mean R 0.114); metallic 0 on paint, 1 only inside chips (bare iron = FoamWhite x 0.62 = 0.51-0.56), rust halo lerp(SafetyOchre, ConcreteDark, 0.55) smoothness 0.22; paint smoothness 0.39-0.61 (p5-p95 0.103), bare 0.50; normal from 0.6 mm of orange peel | micro: orange peel (fBm 160 per metre), chip potential (domain-warped fBm 10 per metre, thresholded by vertex wear G x 0.8 + 0.12 with an fwidth-wide step); meso: mottle (6 per metre), chalking streaks running down on a curl field, grime with AO; a second rotated 2.83 m sample mixed 33-60 % (chips and streaks never repeat on a 4-16 m door); macro: tone +-6 % at 9 m, smoothness +-0.08 at 6 m (3D fBm); ochre bands by vertex B or world-y stripes | `mat_b05_stairs_close0`, `mat_b02_crest_row`, `mat_b07_ledge_row` |
| **SteelOchre** (`SteelOchre`, BwSteel) | Safety-yellow / ochre enamel on steel [UNI] | SafetyOchre 0.571 / 0.323 / 0.033, paint smoothness 0.48; same wear layers | as Steel | `mat_b01_gallery_row`, `mat_b04_reef_row` |
| **Grating** (`Grating`, BwGrating) | Bar grating bearing bars at 30-80 mm pitch [GRT]; the spec fixes 40 mm bars at 80 mm (50 % open); painted or galvanised steel (dielectric paint here) | SteelPaint; coverage analytic: triangle-filtered pulse train across the bars, bar depth 30 mm widens the apparent bar by depth x tan(view) (closes up toward grazing), fade to the mean coverage below 3 px per period; alpha-to-coverage under MSAA 4x; walked tops worn lighter and smoother (paint, metallic 0: no compromise metal, 8.11.1) | micro: Steel set at 1 m; meso: the bars; macro: smoothness +-0.08 at 6 m (3D fBm); the pattern fades to its mean coverage | `mat_b03_spillway_grating_down` (KS09-like pitch -58), `_grating_grazing`, `mat_b07_tunnel_grating_wall` (KS19-like), `_field_along`, `_field_across` (12 x 50 m field: periods 6 .. 1.5 px in view), `seq_mat_pan_grating`, `seq_mat_step_grating`, `seq_mat_pan_field_along`, `seq_mat_pan_field_across`, `seq_mat_step_field_along` |
| **Armour** (`Armour`, BwArmour) | Armour units are plain concrete, dark when wet [LBNL, ANG]; barnacles: calcite shells [BAR2], grey-white when dry, in the intertidal / splash zone; foam albedo about 0.8-0.9 (FoamWhite) | dry ConcreteDark, wet ConcreteWet below 5 m above the sea, crust from 1 m below to 3 m above the sea, broken foam line +-0.35 m at the waterline, ochre gauge bands every 0.5 m; crust shells lerp(ConcreteMid, FoamWhite, 0.12), film lerp(ConcreteWet, SeaTeal, 0.12), crust smoothness at most 0.35 dry / 0.5 wet (chalky shells, the film no longer mirrors the sky) | micro: Crust set at 0.5 m, UNIFORM (two Worley generations of volcano shells 36 / 14 mm with soft orifices and 6 plate ridges on a slimy film; Mask R = random rank of the nearest shell, large 0.02-0.7, small 0.4-1); two samples (0.5 m and a rotated 0.65 m copy, shells 47 / 18 mm) merged by height (the taller shell wins: no ghosted blend), triplanar sharpness 12; meso: colony density = 3D gradient fBm at 0.5 / 0.25 / 0.12 m, a shell shows where the density exceeds its rank (ranks point-sampled at mip 0 at 4 sub-pixel positions: whole shells vanish, no isolated pixels), crust patches (1.7 m) and zone edges broken by gradient-noise wobble, lacy foam streaks (stretched 4:1 along the waterline); below 4 px per 27 mm shell the detail fades to the tile's 1 x 1 mip thinned toward the film by the density (gone at 2 px: no 0.5 m lattice at the 45-140 m of the belt and the islets); macro: tone +-6 % at 40 m, smoothness +-0.08 at 23 m | `mat_b04_reef_close0` (zones compressed onto one stand), `_close1`, `_row`, `_crust_near` (3 m), `_crust_mid` (20 m), `_crust_far` (80 m, KS10-like), `seq_mat_pan_crust_far` |
| **LampSodium / LampTungsten** (BwEmissive) | High/low-pressure sodium and tungsten lamp colours (palette SodiumWarm, Tungsten); the spec clamps lamp radiance at 3 (14.2.5) | token x 2.5 (sodium) / x 3.0 (tungsten), clamped at 3 per channel, x lamp gain `_BwSurface.z` after the clamp (beat-5 portal boost); dark glass body (ConcreteWet x 0.5) smoothness 0.8; constant in time | none (a smooth emitter; 35 % grazing falloff) | `mat_b01_gallery_row`, `mat_b05_stairs_row`, booth bulbs |
| **Lantern** (BwLantern) | Oil / candle lantern glass, warm (palette Lantern) | Lantern x6, clamped at 6 / 1.03 per channel BEFORE the flicker gain so the flicker's peak is exactly 6 (14.2.5) and stays a pure sum of sines (clipping after the gain would square off the R channel, which sits at the clamp, and add harmonics above 1 Hz); flicker 1 +- 0.03 from sines 0.31 / 0.77 / 0.97 Hz weights 1 : 0.6 : 0.3 on the ambient clock (C# twin `LanternFlicker.Gain`) | none | `mat_b07_ledge_row` |
| **Rail / Handrail** (BwLine, D-07) | Painted steel tubes (dielectric paint) | SteelPaint / SafetyOchre, metallic 0 (the imported 0.6 removed: 8.11.1); Rail 0.10 m (`_LineWidth` = `BulwarkConst.LeeRailDiameter`, 14.5.2 / 14.5.6 / 14.5.7 round rails; Wave B), Handrail 0.05 m (14.5.5); tubes built at ring radius `_LineWidth` / 2 and widened to 1.5 px with energy alpha | none (thin-line shader) | KS04 / KS06 rails (graybox) |
| **Beam** (BwBeam) | Light shafts: additive scattering, no surface | FoamWhite emission, analytic wedge | none | beat 6 (later) |
| **BwSea** (later, other owner) | Sea water F0 0.02 (n 1.333) [PBI]; body colours SeaStorm / SeaTeal; Cox-Munk slope variance [RESEARCH A] | 32-wave sum, footprint-filtered, unresolved variance in roughness, glint clamp 8 (14.7 H1) | wave spectrum (no textures) | KS04 (later) |
| **BwSheet / BwConduit** (later) | Aerated falling water: white-green, rough | opaque glassy sheet, flow streaks, alpha-to-coverage ligaments | advected flow noise | KS09 / KS19 (later) |
| **BwFarField** (atmosphere owner) | Lowland fields, rivers, haze (14.7 H4) | analytic sphere shading | fields by jittered hash, river SDFs | KS05, KS20 |
| **BwWaterPortal** (later) | Underwater radiance, Jerlov Kd / c (14.7 H3) | analytic veil, shafts, caustics | caustic sines | KS15-KS18 (later) |
| **BwMist** (later) | Mie single scattering, HG g 0.55 (14.7 H2) | analytic volume | fBm density | KS08 (later) |
| **BwSnow** (later) | Marine snow, 3 cm flakes | additive soft quads, min 2 px | none | KS15 (later) |
| **BwCreature** (later) | Whale skin: dark grey, slightly glossy when wet; albedo about 0.05-0.1 | lit only by lamps and shafts | scar / barnacle patches could reuse the Crust set | KS17 (later) |

## 3. Sources (accessed 2026-11-14 unless marked)

| Key | Source | Used for |
|---|---|---|
| PBI | physicallybased.info, "Physically Based - the PBR values database", https://physicallybased.info/ | concrete 0.51, asphalt 0.043, iron 0.530 / 0.513 / 0.494, rust 0.181 / 0.057 / 0.017, water and ice IOR, snow 0.85 |
| LBNL | Levinson R., Akbari H., "Effects of composition and exposure on the solar reflectance of Portland cement concrete", LBNL-48334 / Cement and Concrete Research 2002 (via the EPA chapter https://19january2021snapshot.epa.gov/sites/static/files/2014-08/documents/reflectance_chap12.pdf and https://digital.library.unt.edu/ark:/67531/metadc737346/m2/1/high_res_d/820773.pdf) | weathering -0.06, soiling -0.05, abrasion -0.19, wetting -0.23 mean albedo change; white vs grey cement |
| XU | Xu, Gregory, Kirchain (review of pavement albedo, cited by the LBNL search results) | new grey-cement pavement 0.35-0.40, aged 0.20-0.30 |
| BAR | Grzegorz Baran, "PBR Color Reference List for Materials" (https://pixelsham.com/2022/09/23/pbr-color-reference-list-for-materials-by-grzegorz-baran) | new concrete sRGB 168-186, aged 122-147, metals 0.5-0.98 |
| UNI | Unity Manual, "Making believable visuals: Material and shader" (https://docs.unity3d.com/2020.2/Documentation/Manual/BestPracticeMakingBelievableVisuals5.html) | painted metal is non-metal, rust is not metal, albedo 0.2-0.8 sRGB guidance |
| UE | Epic, "Physically Based Materials in Unreal Engine" (https://dev.epicgames.com/documentation/unreal-engine/physically-based-materials-in-unreal-engine) | metallic 0/1, paint over metal, F0 4 % for non-metals |
| ANG | Lekner J., Dorf M. C., "Why some things are darker when wet" (Applied Optics 1988; https://www.wgtn.ac.nz/scps/staff/pdf/darkerwhenwet.pdf), after Angstrom | wet darkening by total internal reflection in the film |
| EN1436 | EN 1436:2018 Road marking materials - performance for road users (https://shop.standards.ie/en-ie/standards/en-1436-2018-345420_saig_cen_cen_790153/) | luminance factor classes of white markings (values behind the paywall; LaneWhite is the palette's) |
| GRT | grating manufacturers' specifications (e.g. https://www.shanghaimetal.com/heavy_duty_steel_grating-435.htm) | standard bearing-bar pitches 12.5-80 mm |
| BAR2 | barnacle shell mineralogy, Frontiers in Marine Science 2018 (https://www.frontiersin.org/journals/marine-science/articles/10.3389/fmars.2018.00369/pdf) | shells are calcite (grey-white) |
| URP | URP 17.6.0 package source, `Library/PackageCache/com.unity.render-pipelines.universal@8457e85b8184/ShaderLibrary/{Lighting,BRDF,GlobalIllumination}.hlsl`, core `CommonMaterial.hlsl` 182-265 (local) | the lit model (section 1) |
| TOK | Toksvig 2005 and Tokuyoshi & Kaplanyan 2021 (as cited in the URP core source and docs/RESEARCH.md A 3.3) | mip and geometric specular AA |
| LAG | Lagarde & de Rousiers, "Moving Frostbite to PBR", SIGGRAPH 2014 course (not re-fetched) | dominant direction, specular occlusion |
| GOL | Golus B., "Normal Mapping for a Triplanar Shader", 2017 (not re-fetched) | whiteout triplanar normal blend |
| IQ | Quilez, "filterable procedurals" / "more checker filtering" (docs/RESEARCH.md B 6) | box and triangle filtered patterns |

## 4. Measured in captures

Final state (Wave A review round): build `Builds/wa-mat` round 15 (`Logs/mat-build15.out`: 0 errors, 0 shader errors),
BwTexGen version 15, captures `QA/captures/wa-mat-15` (materials set: 43 stills, 8 sequences), `QA/captures/wa-mat-ctx15`
(matcontext: 5 in-context sequences, the real game state without the rig) and `QA/captures/wa-mat-keys15` (KS04, KS05,
KS06 at 1080p, 4K, 16:10, 21:9).

**Generator (RebuildAll log, `Logs/mat-rebuild15.log`):** GPU size 32.0 MB (the Crust mask is RGBA32 since the review round).

| Set | Albedo R min..max, mean (linear) | Smoothness min..max, mean | p5-p95 smoothness spread, 256 px tile (EditMode) | Same at mip 5 + the shader's macro term (EditMode) | Mean normal length mip 4 |
|---|---|---|---|---|---|
| Concrete | 0.135..0.238, 0.212 | 0.11..0.41, 0.30 | 0.104 | 0.147 (3D fBm, 23 m) | 0.999 |
| Road | 0.160..0.250, 0.206 | 0.21..0.44, 0.30 | 0.124 | 0.125 (2D fBm on the deck, 23 m) | 0.997 |
| Steel | 0.108..0.121, 0.112 | 0.39..0.61, 0.52 | 0.103 | 0.137 (3D fBm, 6 m) | 0.999 |
| Crust | 0.033..0.292, 0.098 | 0.13..0.39, 0.31 (mip 4: 0.268) | 0.175 | n/a (capped 0.35 dry / 0.5 wet in the shader) | 0.885 |

**Captures**: mean sRGB of a 24 x 24 px box on the upper wall panel and on the front of the slab top of each stand in the
row stills of `wa-mat-15`, after the grade (layout projected through the shot pose). Beat 2 is the storm with the road
wet (the QA stand-in `_BwSurface.x` = 1), beat 3 the sun break (wetness 0.21 at s 600), beat 4 cold noon (dry):

| sheet | material | panel sRGB (R, G, B) | panel luma | slab top sRGB | slab luma |
|---|---|---|---|---|---|
| b01_gallery | Concrete | 58, 39, 15 | 41 | 56, 37, 13 | 39 |
| b01_gallery | ConcreteWet | 43, 29, 12 | 31 | 48, 32, 12 | 34 |
| b01_gallery | Steel | 45, 32, 13 | 33 | 52, 36, 14 | 38 |
| b01_gallery | SteelOchre | 106, 56, 10 | 63 | 110, 62, 15 | 69 |
| b01_gallery | LampSodium | 45, 32, 13 | 33 | 59, 40, 15 | 43 |
| b02_crest | Road | 99, 99, 96 | 99 | 82, 89, 95 | 88 |
| b02_crest | Concrete | 94, 94, 93 | 94 | 77, 84, 90 | 83 |
| b02_crest | ConcreteDark | 66, 67, 69 | 67 | 65, 73, 79 | 72 |
| b02_crest | Steel | 78, 86, 91 | 85 | 84, 97, 109 | 95 |
| b02_crest | LampSodium | 76, 84, 89 | 82 | 87, 98, 108 | 97 |
| b03_spillway | ConcreteWet | 152, 121, 81 | 125 | 86, 81, 72 | 82 |
| b03_spillway | Concrete | 191, 155, 106 | 159 | 151, 130, 103 | 133 |
| b03_spillway | Steel | 151, 133, 100 | 134 | 121, 115, 101 | 115 |
| b03_spillway | Road | 182, 147, 100 | 151 | 142, 123, 98 | 125 |
| b04_reef | Armour@sea | 94, 98, 103 | 97 | 95, 108, 118 | 106 |
| b04_reef | Armour | 95, 99, 104 | 98 | 100, 108, 117 | 107 |
| b04_reef | Armour@gauge | 93, 97, 102 | 97 | 227, 180, 87 | 183 |
| b04_reef | Concrete | 149, 148, 141 | 148 | 156, 157, 156 | 157 |
| b04_reef | SteelOchre | 219, 169, 64 | 172 | 209, 171, 98 | 174 |
| b05_stairs | Steel | 61, 49, 28 | 50 | 66, 51, 28 | 53 |
| b05_stairs | SteelOchre | 156, 91, 13 | 99 | 156, 97, 22 | 104 |
| b05_stairs | Concrete | 81, 60, 31 | 62 | 78, 57, 29 | 60 |
| b05_stairs | ConcreteWet | 64, 47, 25 | 49 | 64, 48, 25 | 50 |
| b05_stairs | LampTungsten | 65, 51, 29 | 53 | 91, 69, 38 | 71 |
| b06_windows | Concrete | 47, 58, 56 | 55 | 45, 62, 62 | 59 |
| b06_windows | ConcreteDark | 50, 49, 43 | 49 | 54, 59, 53 | 57 |
| b06_windows | ConcreteWet | 13, 28, 31 | 25 | 11, 42, 48 | 36 |
| b06_windows | Steel | 8, 38, 46 | 32 | 7, 50, 59 | 41 |
| b07_tunnel | ConcreteDark | 50, 32, 4 | 34 | 48, 30, 3 | 32 |
| b07_tunnel | ConcreteWet | 38, 23, 3 | 25 | 46, 29, 3 | 31 |
| b07_tunnel | Steel | 60, 41, 7 | 43 | 69, 47, 8 | 49 |
| b07_tunnel | LampSodium | 70, 48, 9 | 50 | 100, 67, 14 | 70 |
| b07_ledge | ConcreteDark | 108, 62, 39 | 70 | 64, 40, 47 | 45 |
| b07_ledge | Concrete | 160, 93, 59 | 105 | 90, 57, 62 | 65 |
| b07_ledge | Steel | 205, 123, 63 | 136 | 97, 61, 74 | 69 |
| b07_ledge | Lantern | 143, 89, 56 | 98 | 128, 84, 78 | 93 |

Reading: inside one sheet the values keep the palette's order (beat 2: Road panel 99 > Concrete 94 > Steel 85 >
ConcreteDark 67; beat 4 sunlit Concrete slab 157, inside LightCal's 150-175 for the beat-4 sunlit road). The wet beat-2
road slab is 88 (was 84 before `_WetAlbedo` 0.8), inside LightCal's 85-115. Lamps and the lantern are measured on their
steel stand (the emitter is the sphere).

**Image metrics** (`python -I Tools/qa/image_metrics.py`): `wa-mat-15` 43 stills: fireflies 0, isolated extreme
("NaN-like") pixels 0, magenta 0, clipped at most 0.011 %; the 6 FAIL rows are the 14.10.3 per-beat mean-luma bands
applied to off-path sheet stills. `wa-mat-keys15` 12 stills: PASS, 0 FAIL checks.

**Shimmer** (scratchpad tool `shimmer.py`, every frame warped exactly into frame 0: rotation homography for pans,
known-plane homography for translations; per-pixel temporal std of display luma, max of the 64 x 64 block means; the
"low-pass" column repeats it after a Gaussian sigma 1.5 px, which removes the band where the bilinear resampling of the
compensation itself is unreliable (its own floor reaches 3-4 levels on 2-4 px periods) and keeps the low-frequency beats
of a moire or a flicker; limit 2.0 levels):

| Sequence | Subject | Raw max block | Low-pass max | Block-mean luma std |
|---|---|---|---|---|
| `seq_mat_pan_road` | road patch, numerals | 0.68 | 0.13 | 0.10 |
| `seq_mat_pan_floor_far` | far stage floor | 0.84 | 0.21 | 0.10 |
| `seq_mat_pan_grating` | grating at pitch -58 (24 px per period) | 1.03 | 0.25 | 0.10 |
| `seq_mat_pan_field_along` (grating blocks only) | field, periods 6 .. 1.5 px across the view | 1.42 | 0.33 | 0.17 |
| `seq_mat_pan_field_across` (grating blocks only) | field, foreshortened periods 6 .. 1.5 px, pitch pan | 3.40 (interpolation floor 3.7) | 0.48 | 0.18 |
| `seq_mat_step_field_along` (3 mm/frame across the bars, plane-compensated) | field | 1.43 | 0.34 | 0.77 |
| `seq_mat_pan_crust_far` | crust wall at 80 m | 0.74 | 0.30 | 0.21 |
| `seq_matctx_pan_ks05_leeface` | lee face formwork at grazing (KS05) | 0.63 | 0.15 | 0.08 |
| `seq_matctx_pan_ks06_parapet` | parapet formwork receding (KS06) | 1.63 | 0.52 | 0.16 |
| `seq_matctx_pan_gallery` | gallery walls receding (beat 1) | 0.93 | 0.20 | 0.15 |

The whole-frame maxima of the two field pans (2.16 / 3.40) sit on the hard-edged sea-foam patches of another owner's
graybox sea in the background, not on the grating. The 1.5 m forward dollies at walking pace (known planes: road,
parapet face and top, sea for KS06; walls, floor, roof for the gallery) change their low-frequency shading with the
viewpoint (raw / low-pass up to 3.0 / 2.8 on the KS06 deck, view-dependent specular), so they are judged on the
high-pass residual (sigma 2 px) against a double-interpolation control: KS06 dolly 0.87 (control 0.68, excess 0.26);
gallery dolly p99 excess 0.33, max 1.89 on the roof block that holds a lamp box (an occluder outside the plane model).

**Formwork joints** (scratchpad tool `joints.py`: the symmetric dip of every vertical joint, z = 3 k, in display levels,
mean and temporal std over the 30 frames). KS06 parapet (x 5.5), pan / dolly: 11.5 m -8.1 / -9.4 (std 0.14 / 0.84),
13.9 m -5.2 / -5.9, 16.6 m -3.6 / -3.8, 19.3 m -2.2 / -2.3, 22.2 m -2.4 / -2.6, 25.0 m -1.7 / -1.7, 27.9 m -1.1 / -1.2,
30.8 m -2.8 / -2.7, 33.8 m -0.2 / -0.3, 36.7 m -3.2 / -3.1; every temporal std 0.10-0.19 in the pan and 0.12-0.25 beyond
15 m in the dolly: the joints no longer blink. (The first round's isotropic footprint gave -0.7 at 18 m and -6.6 at 27 m
in a single KS06 still.) Gallery west wall (x -3.5), beyond 11 m: std 0.12-0.17 (pan), 0.10-0.38 (dolly).
