# TP log - Look v2 infrastructure (TEXTURE_PLAN 8.3 TP-0, TP-1)

Owner TP. Exclusive files: TEXTURE_PLAN 8.3 TP-0 / TP-1 as amended by the workflow prompt (lead decisions 1-7).
Requests to TP: write "REQUEST TP: <file>: <change>" in your own log; answers appear here as "DONE TP <ID> <request>".

## 2026-11-14

### Step 1 - variant baseline (committed first)
- `BatchTools.WriteVariantBaseline` (new, -executeMethod) parses every Megastructure/* shader: per pass the keyword pragmas of the
  pass, of the HLSLINCLUDE blocks and of the local includes (recursive), the variant count they give, and Unity's own pass keyword
  list (`ShaderUtil.GetPassKeywords`). Colour passes = every LightMode except ShadowCaster / DepthOnly / DepthNormals / Meta /
  MotionVectors (Unity reports the ShadowCaster tag as "SHADOWCASTER": compared case-insensitively).
- Run TP-baseline02 on the shaders of HEAD 59621c6 (working tree: only the generated palette differs, no pragma): 14 shaders,
  19 colour passes -> `QA/baselines/variants_tp0.json`. Lit template NocSurface ForwardLit: 8 variants
  (instancing x _ADDITIONAL_LIGHTS x _CLUSTER_LIGHT_LOOP).
- Validate gate `forward variants (TP-0 baseline)`: every colour pass equals its baseline (variants and keywords); a shader not in
  the baseline (NocGrating, NocShopfront of SU) passes when its forward keywords are a subset of the template's and its variant
  count is not larger. Non-colour pass changes are warnings.
- `ShaderUtil.GetVariantCount` is not reachable by reflection in 6000.6 (signature changed): not recorded.

## Resume rule
Find the last step marked GREEN below and the last commit with "TP:" in the message; continue with the next open item. Uncommitted
TP work lives in the files of the TP-0 list (TEXTURE_PLAN 8.3) only.

### Step 3 (in progress at the pause, re-applied from docs/wip/lookv2-build-wip-2026-11-14.patch at 12:22)
- Lit template (NocLitPass / NocLitCore): `float4 uv1` (xy as before), `NOC_PASS_UV1ZW` (uv1.zw -> custom.zw before the vertex
  hook), `NOC_ALPHA_OUT` (forward only, straight alpha for AlphaToMask), `NOC_FRONT_FACE` (`f.faceSign`), `f.specOcc` (default 1),
  `f.debug` (hook-written debug codes), `NocFaceFrame(n, u, P)` with the measured sigma and the projected-up B (1.6).
  Every one is off unless a shader defines it, so no picture changes.
- NocLighting: `NocShadeOcc(inputData, s, specOcc)` = diffuse env x occlusion + specular env x occlusion x specOcc
  (EnvironmentBRDF split into its two terms); `NocShade` = NocShadeOcc(.., 1): identical arithmetic for every v1 shader.
- NocCommon: globals `_NocTexDetail`, `_NocDebugView`, `_NocFacadeV`, `_NocSurfV`, `_NocFoliageV`, `_NocParity` (0 = default
  everywhere, so editor previews need no setup) and their helpers; `NocSrgbToLinear` (exact) for the debug codes.
- Forward pass: debug view return (raw codes before fog / floor; view 7 = world normal by the template) and the parity branch
  (skips NocFog and NocFloor), both uniform branches after every derivative.
- ShaderIds: TexDetail, DebugView, FacadeV, SurfV, FoliageV, Parity. NocLookGlobals (new): `-tex`, `-debug-view`, `-facade-v`,
  `-surf-v`, `-foliage-v` (v1 default), `-debug-shaders`. NocDebugViewFeature (new) + `Megastructure/NocDebugView` (new):
  views 1-8, 10 copied before the rain and written back after the grade; view 9 = overdraw (shaded count R, depth complexity G).

### 12:44 generators (FA request) and handed-over constants files
- `Tools/gen_hlsl.py` and `ShaderGen.cs`: one header per constants class (CLASSES / ShaderGen.ConstantClasses): NocturneConstants ->
  NocConstantsGen.hlsl (bytes unchanged: checked against the HEAD generator's output), NocFacadeConstants -> NocFacadeConstantsGen.hlsl
  (guard NOC_FACADE_CONSTANTS_INCLUDED, 182 lines from FA's file), NocSurfaceConstants -> NocSurfaceConstantsGen.hlsl, NocTexConstants
  -> NocTexConstantsGen.hlsl. Comments are stripped before parsing, hex int literals (0x78) parse, a class file that does not exist yet
  gets no header, a NC_ name exported twice aborts both generators (ShaderGen.DuplicateConstantNames). ProjectTests compares every header
  (disk = ShaderGen = gen_hlsl.py).
DONE TP FA REQUEST Scripts/Runtime/Core/NocFacadeConstants.cs: not recreated; exported to Shaders/Generated/NocFacadeConstantsGen.hlsl
  (guard NOC_FACADE_CONSTANTS_INCLUDED, `#define NC_<Name>`). The generated header and its .meta are FA's to commit.
- `Scripts/Runtime/Core/NocTexConstants.cs` (new, TP): hex, module, panel, triplanar, fade, micro, Toksvig, aniso and VRAM constants.
- `Scripts/Runtime/Core/NocSurfaceConstants.cs`: created empty and handed over to SU (TEXTURE_PLAN 8.3).
REQUEST SU: Scripts/Runtime/Core/NocSurfaceConstants.cs: TP wrote the empty handed-over class at 12:42 and the write reported an existing file. If you had already created it with content (between 12:31 and 12:42), it was replaced: please re-apply your constants. It is yours from now on; the generators export it to Shaders/Generated/NocSurfaceConstantsGen.hlsl (yours to commit).

### 13:05 step 5 - the split (handed over now; every file keeps exactly its current rows, so no picture changes)
- `Scripts/Editor/MaterialFactory.cs` (TP core): ColourSpec, MatSpec (+ `ExtMaps(key)`: the three maps of a processed set or array
  under Textures/External and `_Tile` (+ `_TileV` when anisotropic) from its texmeta), Make, Specs() = SurfaceRows + FacadeRows +
  LookRows + FoliageRows, Build, Check. Textures load as `Texture` (Texture2DArray flipbooks work).
- HANDOVER SU: `Scripts/Editor/MaterialFactory.Surface.cs` (Gray*, NocSurface rows, NocWet floors, GlassBack/Front, Line + the
  Surface()/Wet() helpers and WetStone/MetalDark/SteelF0/Brass constants), `Scripts/Runtime/Core/MaterialLibrary.Surface.cs`,
  `Scripts/Tests/EditMode/MaterialTests.Surface.cs` (EveryLargeSurface_VariesSmoothnessByAtLeast0_1, FloorForFollows14_4Surfaces,
  FamilyOf), `Scripts/Runtime/Core/NocSurfaceConstants.cs`.
- HANDOVER FA: `MaterialFactory.Facade.cs` (Facade row), `MaterialLibrary.Facade.cs`, `MaterialTests.Facade.cs` (empty).
- HANDOVER PA: `MaterialFactory.Look.cs` (Neon, StreakTail, StreakHead, Beacon, Rain, Sky, Grade), `MaterialLibrary.Look.cs`,
  `MaterialTests.Look.cs` (empty).
- HANDOVER FO: `MaterialFactory.Foliage.cs`, `MaterialLibrary.Foliage.cs`, `MaterialTests.Foliage.cs` (all empty: add Leaves and
  LeafCore as a row + a field).
- A new row needs its MaterialLibrary field in your own MaterialLibrary.<part>.cs (serialised name = the row's Field).
- Generated .meta files of these new files: TP commits them with the split (they are created by the next Unity import).
- fastcheck 13:05: runtime 162 / editor 211 OK.

## 2026-11-15 (third start, run wf_2c6d782f-763; D-030: step 6 moved to TQ)
### 21:41 resume
- Read STATE "PAUSED AGAIN", this log, `git diff` of the TP files. Re-applied tree as at 13:30 on 2026-11-14; the split partials
  (MaterialFactory/MaterialLibrary/MaterialTests .Surface/.Facade/.Look/.Foliage) are still byte-for-byte the verbatim split (no owner
  edited them yet), so TP commits them with the core as one consistent split.
- BatchTools.cs and Tools/gen_hlsl.py were CRLF at HEAD and LF in the re-applied tree (1684-line whitespace diff): restored CRLF, the
  diff is now the 105 / 84 real lines.
- fastcheck 21:43: runtime 162 / editor 215 OK.
### 21:47 TP-t01 (NocTexturingTests|ShaderParityTests|ProjectTests|MaterialTests): 29 / 31
- NocTexturingTests: hex max |sum - 1| 2.2e-16, max jump 2.4e-6 (after the S-curve 4.4e-6) < 1e-4; triplanar and UV0 surface-gradient
  normals 0.000 deg; float32 uv error at 1.5-2 km 0.062 texel; sigma, panel seam, hex rotation PASS.
- FAIL ModuleShuffle concrete_tiles_02 "tu in [0, 1)": 1.164. Not a library error: in a running-bond row the last brick straddles the
  tile's wrap edge, so tu reaches 1 + 0.5 module / tile (Repeat wraps it onto the same brick, gradients continuous). Test bound fixed
  (tuMax = 1 + 0.5 mu / tile for bond lattices).
- ShaderParityTests: debug view 7 on NocGraybox at 5 poses, 2304 px each, max diff 0.00 codes; parity global (emissive quad 0.251,
  0.501, 0.751, deterministic, global restored) PASS.
- ProjectTests: GeneratedHlslIsByteIdentical PASS (every header: disk = ShaderGen = gen_hlsl.py). MaterialContractHolds FAIL: the
  materials on disk were built with palette v1 (GrayFloor 0.155, 0.155, 0.208) and PA's palette v2 contract is 0.433, 0.429, 0.414:
  stale assets, cured by RebuildAll.
### 21:53 TP-rebuild01: RebuildAll SUCCESS, 0 problems; 28/28 materials written; "added NocDebugViewFeature on PC_Renderer";
  GraphicsSettings gains NocDebugView in Always Included (guid a28079aa).
### 22:03 TP-tall01 (full EditMode, after TP-rebuild01): 306 / 316
- TP: NocTexturingTests.ModuleShuffle failed once more on long_white_tiles (tu 1.0000156: an unregistered lattice is whole within
  1e-3 only): bound = (round(cu) mu + bond shift) / tile. Every other TP test PASS.
- Not TP (listed for D-030 (c); owners carry them): AtmosphereTests.AlleyHaze_KS00MouthAndKS02Slit (bf38311 test, PA's atmosphere
  v2 on purpose), AtmosphereTests.Deck_TintModel (PA, new), Beat02BoulevardTests.Blades_Are14x200 (bf38311 test; PA retired the violet
  blade on purpose: "magenta, cyan, amber, violet" no longer holds), Beat04SkybridgeTests.Lamps21 (bf38311 test; PA's lamp 4.0 -> 3.5
  on purpose), MaterialTests.Foliage_LeafAtlas (FO, new, mid-edit), RainTests.Tint_LookV2 (PA, new), VisibilityAuditTests
  .PreRevealKoiIsHidden (GE, new; GE now marks it ignored until the gate structures land), ProjectTests.GeneratedHlslIsByteIdentical
  (NocSurfaceConstantsGen.hlsl on disk older than SU's constants: regenerated by the next RebuildAll), ProjectTests
  .MaterialContractHolds (FO's new LeafCore / Leaves rows added after TP-rebuild01: the next RebuildAll writes them).
### 22:14-22:40 TP-rebuild02 / validate02 / t02 and the first rebuild03 attempt failed to compile on other packages' mid-edit files
  (KO KoiBuilder PectoralVMax, TQ ShotPlan TexPanSequences): not TP; retried by a wrapper that waits for a clean fastcheck.
### 22:40 TP-rebuild03: RebuildAll SUCCESS (0 problems). TP-validate03: Validate PASS, 96 checks, 0 problems (every TP-0 gate:
  forward variants against variants_tp0.json, X3570 / X4000, External textures import, debug-view feature after the grade).
### 23:02 TP-t03 (NocTexturingTests|ShaderParityTests|ProjectTests|MaterialTests): 31 / 34
- TP: NocTexturingTests.ModuleShuffle on the new Pavers.texmeta.json (texprep, half bond along v): the texmeta lattice is now read
  with its axes swapped when bondAxis is "v" (as the shaders sample it). Fixed in the test.
- Not TP: GeneratedHlslIsByteIdentical (NocFacadeConstantsGen.hlsl older than FA's NocFacadeConstants.cs), MaterialContractHolds
  (SU's new Grating row after rebuild03): both written by the next RebuildAll.
### 23:27 commit 489beeb (TP-0 steps 3-5; the split partials as split, FO's later MaterialFactory.Foliage edits stay FO's)
- Built in a temporary index (HEAD + TP paths; the three .Foliage partials from the split-time blobs because FO edited them
  since; GraphicsSettings = HEAD + the NocDebugView line only), HEAD moved with a compare-and-swap, the real index refreshed for
  these paths only. HEAD before it did not compile without TP's files (PA's committed AtmosphereDriver reads NocLookGlobals.FacadeV).
### 23:15 TP-0 acceptance run (lookdev slot TP, every flag at 1): QA/captures/TP_r01 (keyshots1080) and TP_r01dv9 (-debug-view 9)
- (b) BuildWindows of Builds/lookdev_TP: OK (the same tree as rebuild03 / validate03).
- (d) TP_r01, 20 / 20 frames: no black (Y < 1: 0.000 everywhere), no white (Y > 254: 0.0000), no magenta, no NaN (the image_metrics
  "NaN-like" isolated pixels, 1-34 per frame, are the v1 facade window-grid gaps PA traced (KS08 1556,543); graybox01 has them in
  11 frames, TP_r01 in 12). Mean luma 27-58, p1 13-30.
- Side by side with QA/captures/graybox01 (all 20 shots, 640 x 360 each, looked at): geometry, window cells and their colours,
  facade structure, signs, wet floor reflections, rain streaks, trees and the goal tenement are the same in structure. Differences,
  none from TP: PA's palette v2 and atmosphere (violet sky -> dark warm grey deck; far faces now read through the fog: the
  horizontal band of face B above the boulevard end in KS04 / KS05 / KS06; KS16 wall amber instead of teal), KO's koi v2 (Kohaku
  patches and scales in KS17 / KS18 / KS19), GE's pre-reveal koi gating (the cyan koi arc of graybox01 KS04 / KS06 / KS07 / KS11 /
  KS15 is gone), the D-029 KS13 pose (s 718: the bridge glass, graybox01 showed the dark void), GE's KS19 lamp.
- Spike, NocFacade overdraw (view 9, decoded with TQ's Tools/qa/debugview.py, QA/reports/debugview_TP_r01dv9.md): shaded facade
  fragments per covered pixel without a prepass KS04 1.79, KS05 1.39, KS09 3.30, KS17 3.83 (depth complexity 9.7 / - / 16.4 /
  12.0). Above 1.5 at three of the four shots -> TEXTURE_PLAN 2.12 / 8.3: the facade depth prepass (NocFacadePrepassFeature) is
  added. Caveat: the count draws the facade renderers against their own depth only, so shafts under the plaza and deck floors
  count too (KS17's lower half); the follow-up run primes the depth with the other opaques first. Even halved, KS09 / KS17 stay
  above 1.5: the decision does not depend on it.
- (a) ShaderParityTests 2 / 2, NocTexturingTests 7 / 7 on the plan's lattices (the texmeta-driven Pavers case fixed in the test),
  generators: every Shaders/Generated header on disk = gen_hlsl.py output (checked 23:27 into a scratch folder; ProjectTests
  compares the same with ShaderGen).
- (c) TP-tall01: every bf38311 test passes except those listed at 22:03 with their owners (PA x3 on purpose, mid-edit generated /
  material assets of SU / FO / FA cured by RebuildAll); every TP test passes after the two test-bound fixes.
TP-0 READY 2026-11-15 23:30
- For FA / SU / FO / PA-2: the lit template hooks (NOC_PASS_UV1ZW, NOC_ALPHA_OUT, NOC_FRONT_FACE, f.specOcc, f.debug, NocFaceFrame),
  NocTexturing.hlsl (hex, module, panel, triplanar, UV0 frame, fade to the last mip), the globals and flags (-facade-v / -surf-v /
  -foliage-v, -debug-view N, -tex 0) and MaterialFactory.ExtMaps(key) are committed (489beeb). Processed sets: TP-1 (texprep.py
  running; the outputs in Textures/External are drafts until "TP-1 READY").

## TP-1 (texture sets) - in progress
### Tools/texprep.py (new, TP) and Tools/texprep/tp_common.json (new, TP: OverlayArr, MicroArr)
- Schema = the drafts of facade.json, surface.json and foliage.json as their owners wrote them (no renames needed): "sets" {Key:
  unit} and "arrays" {Key: {res, flipbook, maps, aniso, kind, slices}}; the full field list is the texprep.py docstring. Extra
  optional fields: "tileReason" (a deviation > 15 % from the TEXTURES.md tile that is not a registration), "token" (palette token
  for the chroma judgement; default by class), "normalStrength" (default by class, LOOK_V2 5.1).
- Steps 1-10 of TEXTURE_PLAN 1.4 as written, with these measured findings:
  - Step 2 sign check on every final set (corr(n.x, -dh/du), corr(n.y, -dh/dv), 1.5 px): all positive (0.35-1.00) except
    polyhaven_concrete_floor_worn_001: x -0.689, y +0.813 -> its nor_gl red channel is inverted. texprep flips n.x for that
    source (SOURCE_FIX, recorded as stats.sourceFix; re-checked positive after the fix). It feeds Concrete, FacadeArr slice 0,
    RoomArr slice 2 and MicroArr slice 0.
  - Step 3 lattice registration: counts from the config module (FFT-checked), then every joint is WARPED onto the grid
    (piecewise linear per axis, Catmull-Rom resampling; straight joints globally, bonded joints per course / column band, a
    joint off the grid by more than p/5 is dropped, more than 10 % local stretch falls back to a pure shift). A roll alone left
    joints up to 13 px off (concrete_tiles_02: real slabs vary +-1.2 cm). Results: granite 5 x 5 (0.46 m, max shift 1.9 px),
    concrete_tiles_02 6 x 3 (0.304 x 0.608 m, half bond along v), large_floor_tiles_02 3 x 3 (1.0 m), long_white_tiles 33 x 10
    (0.0385 x 0.127 m), brick_wall_08 8 x 20 (0.2375 x 0.095 m, running bond along u, local stretch <= 6.7 %).
  - Seam scores above 1.25 accepted only where TEX / TEX-R looked (concrete_floor_worn_001, OfficeCeiling001, plank_flooring_04,
    fine_grained_wood, large_floor_tiles_02) and Asphalt020L 1.30 (looked at 1.6x gain 23:20: no seam; 1-px aggregate noise).
  - Step 4a bake fit: concrete_wall_006 0.389 -> 0.021, asphalt_02 0.322 -> 0.005 (gate 0.10). Step 4c plastered_wall_04 blob
    0.19 -> 0.12 (strength 0.6). Step 4b ramp guard (never subtracted from a periodic tile) warns on concrete_wall_006 32 %,
    large_floor_tiles_02 18 % (slab tones), MetalPlates013 11 %, plank_flooring_04 9 %, trident_maple_bark 5.5 %.
  - Step 5 violet: the desaturation uses both LOOK_V2 5.4's HSV rule and TEX-R's gate measure (plank_flooring_04 under the Bark
    token measured 2.7 % violet with the HSV rule alone).
  - Outputs are written to <file>.tmp and os.replace()d (texmeta first; unchanged bytes are not rewritten). At 23:04 PA's lookdev
    build imported a half-written FacadeArr_Mask.png from my first texprep run and drew the facades magenta (PA log): my fault,
    fixed by the atomic replace.
DONE TP FO REQUEST Tools/texprep.py step 7: foliage.json sets (a "compose" block) are decoded to float32 linear and passed to
  foliage_compose.compose(cfg_set, maps) (imported by path, maps albedo / normal / opacity / rough / height / scattering); the
  returned 2 x 2 flipbook goes through steps 5 (contrast and chroma over alpha > 0.5), 6 (mask; B = scatter with
  compose.scatteringToMaskB), 9, 10 (texmeta kind array, alpha true, alphaRef 0.5, cols 2, rows 2, cls leaves, registration
  "leaf-cluster sprites ..."). `python -I Tools/texprep.py --only LeafCrown` works now (LeafCrown and LeafIvy were written 23:40).
REQUEST FA: Tools/texprep/facade.json: RoomArr slice 4 (Carpet016, tile 1.0; TEXTURES.md 1.7) and slice 6 (OfficeCeiling001,
  tile 1.2; TEXTURES.md 3.85) deviate by more than 15 % without a registration, so TextureTests.Sets_TileMatchesTexturesMdOrIs
  Registered fails on them: either use the TEXTURES.md tile or add "tileReason": "<why>" to the slice (texprep records it as the
  slice registration). Schema notes for FA: "count" as a string = the nearest whole number (done: concrete_layers 6 lifts per
  1.55 m -> 14 per 3.6 m floor, tile 1.543 m; corrugated_iron_02 28 ribs -> 12 per 1.2 m, tile 2.80 m), "offsetQuantum" is not
  read by texprep: the per-slice pitches are in the texmeta (sliceTile, sliceTileV, sliceRegistration = JSON with count, pitch,
  perUnit, scale), brick 39 courses per 3.6 m (tile 1.846 m, brick 0.2308 m), facade tile 24 courses (tile 1.95 m), stone
  2.40 x 2.25 m (5 x 8 slabs per bay x floor).
NOTE SU / FA (no change needed in your files, for the shaders): Pavers.texmeta.json has bondAxis "v" (concrete_tiles_02: 6
  columns of 0.304 m along u, 3 slabs of 0.608 m along v, odd columns shifted by half a slab along v): sample the module shuffle
  with the axes swapped (NocTexturingTests reads it that way). Brick (and FacadeArr slice 5): the courses are exact (0.095 m set,
  0.0923 m slice), but the bricks are irregular (0.15-0.30 m), so a per-brick shuffle at moduleU would cut bricks: shuffle whole
  courses (module = tile x course, a hashed u offset per course) instead.
### 23:50 off-hue rule (texprep VERSION 3): plank grain (RoomArr 3) came out as blue / yellow stripes and the ivy twigs blue
  (TEX-R finding 1: the per-channel white balance turns a cast into its complement). Product pixels more than 90 deg of hue from
  the token with saturation > 0.15 are desaturated; looked at again: planks brown / mauve, twigs dark green-grey. QA/critique-TP.md.
### 00:15 TP-rebuild04 RebuildAll SUCCESS (the facade prepass feature registered on PC_Renderer). 00:18 TP-t04
  (TextureTests|NocTexturingTests|ShaderParityTests|ProjectTests|MaterialTests): 41 / 46; ProjectTests (generators byte-identical,
  material contract) and every NocTexturing / ShaderParity / Material test PASS. TextureTests, first run on the processed sets:
- Spike (TEXTURE_PLAN 1.5, step 2): a NormalMap-type flipbook DOES import as a Texture2DArray in 6000.6 (OverlayArr_Normal and
  LeafCrown_Normal: textureType NormalMap, textureShape Texture2DArray, flipbook 2 x 2, BC5). No fallback needed.
- FAIL FacadeArr_Normal "anisoLevel 8": its import was stale (2D, aniso 8, flipbook 1 x 1): imported while FacadeArr.texmeta.json
  did not exist yet (the half-written first run), and the importer registered the texmeta dependency only when it existed, so the
  texmeta that appeared later never re-imported it (the PNG bytes did not change). Fixed: the dependency is always registered;
  NocTextureImport.Version 1 -> 2 re-imports every External texture once.
- FAIL Grating_Albedo last-mip mean 0.27: the grating's holes kept the source's dark colour. Fixed: texprep push-pull dilates
  the covered colour into alpha <= 0.5 texels for every opacity set (as foliage_compose does for the leaves).
- FAIL SteelPlates_Mask smoothness +0.0053 at the 4 x 4 mip: BC7 re-encodes each mip (a 4 x 4 mip is one block); the means fall
  0.557 -> 0.540 over mips 0-8 as Toksvig must. Tolerance 1 / 255 -> 2 / 255.
- FAIL content gate on FacadeArr slice 6 (plaster): flattening is configured there, but only the top-level flatten was recorded.
  texprep now records stats.flatten per slice (TexStats.flatten).
- FAIL RoomArr slice 4 (Carpet016 1.0 m vs 1.7 m): REQUEST FA above (tileReason or the TEXTURES.md tile).
- PASS: manifests, VRAM (from formats), leaves coverage, metallic allow-list, contrast / chroma / violet on every set and slice.
### Facade prepass: FA plans ZWrite Off / ZTest Equal on NocFacade's forward pass once the prepass lands (FA.md 00:00), so the
  prepass writes bit-identical depth (no bias): NocFacadePrepass computes the clip position as NocVert does (object -> world ->
  clip). Default off (-facade-prepass 1) until the pixel comparison r02 / r02p passes.
### 00:38 lookdev TP r02 / r02p / r02dv9 (build 00:37, texprep v3 sets in the tree)
- Prepass pixel test: QA/captures/TP_r02 (-facade-prepass 0) vs TP_r02p (-facade-prepass 1, "1945 facade renderers in the depth
  prepass"): 20 / 20 shots identical to 1 code (max 1 at isolated pixels in 11 shots, mean 0.000, no pixel off by more than
  2 codes). The bit-identical depth holds: no holes, no z-fight.
- r02dv9: every frame black (written share 0): my bug - the new Prime pass was inserted before "Overdraw" in NocDebugView.shader,
  so pass index 3 drew Prime as the full-screen output. Prime moved to index 4 (after Overdraw); r03dv9 re-runs it.
### 00:30 texprep v4: 24 / 24 current (--check exit 0); Grating's holes now carry the dilated grating colour.
### 00:56 DONE FA's tiles (RoomArr 4 Carpet016 1.7 m, 6 OfficeCeiling001 3.85 m): RoomArr re-processed, --check 24 / 24.
### 01:00 TP-t05: 45 / 46 (every Import / Toksvig / tile / metallic / contrast / content / VRAM test PASS after the re-import of
  NocTextureImport v2: FacadeArr_Normal is now a Texture2DArray 8 x 1024, BC5, aniso 16). Only FAIL: LeafCrown whole-slice last
  mip B 0.645 (the dilated texels kept the pre-grading colour) -> texprep re-dilates after grading.
### 01:05 TP-validate04: Validate PASS, 101 checks, 0 problems (new: "facade prepass feature", shader NocFacadePrepass).
### 01:20 TP-t06: 46 / 47; FAIL LeafIvy slice 2 last mip (0.45, 0.46, 0.435): at 6 % coverage the dilated texels decide the
  whole-slice mean. texprep balance_uncovered(): the uncovered texels of each flipbook cell are shifted by a constant so the cell
  averages 0.5 (invisible at mip 0, they only enter the coarse mips); every leaf cell and Grating now 0.50 +- 0.001 (linear, PNG).
  VRAM from formats: 374.7 MiB (cap 900). 69 External textures, 23 masks checked.
### 01:30 r03dv9 (primed overdraw): 1.000 shaded per visible facade pixel everywhere = my second bug: the facades were only
  ADDED to bit 30, so the Prime pass (every renderer with another bit) drew them too and they primed themselves. The re-tag now
  sets bit 30 only (plus the prepass bit 29) in debug view 9, and Prime excludes both bits. The unprimed r01dv9 numbers (1.79 /
  1.39 / 3.30 / 3.83) remain the upper bound until r04dv9.
### 01:40 perf TP_perf_v2_off (-facade-v 2, -facade-prepass 0, 90 s cinematic, gpu-guard): 32 278 frames, 394.4 fps, 1 % low
  78.7, max 72.3 ms, 12 frames over 33 ms. The prepass run follows.
### 01:55 perf, prepass off / on (-facade-v 2: FA's textured path; 90 s cinematic each, gpu-guard turn, vSync 0; two pairs):
  off 394.4 / 416.8 fps, GPU avg 2.46 / 2.41 ms, p99 5.11 / 4.31 ms; on 453.5 / 463.8 fps, GPU avg 2.20 / 2.14 ms, p99 5.18 /
  3.22 ms: -11 % GPU time on average (medians equal at 1.85 ms: the gain is in the stacked-tower views). QA/reports/
  TP_perf_v2_{off,on,off2,on2}.summary.json. -> DefaultFacadePrepass = true (no picture change: r02 / r02p within 1 code).
NOTE FA: the prepass is on by default and its depth is bit-identical to NocFacade's forward depth, so the Facade row may set
  _FacadeZWrite 0 / _FacadeZTest Equal. With Equal, -facade-prepass 0 drops the facades (nothing to equal): keep it a debug switch,
  or keep LEqual (the same early-Z rejection; only the redundant depth write remains).
### 01:58 TP-t07 (TextureTests|NocTexturingTests|ShaderParityTests|ProjectTests|MaterialTests): 47 / 47 PASS.
- TextureTests on the 24 processed keys (69 textures: 17 sets x 3, FacadeArr 3 x 8 slices, RoomArr 2 x 8, OverlayArr 2 x 4,
  MicroArr 2 x 4, LeafCrown / LeafIvy 3 x 4): import settings and formats (BC7 sRGB / BC5 / BC7 linear, aniso 8 / 16 -> effective
  >= 9, mips, no streaming, bias 0, not readable, size = manifest, arrays = Texture2DArray incl. the normal arrays), last-mip mean
  0.5 +- 0.01 (alpha sets +- 0.05), Toksvig (mip 0 = PNG, smoothness non-increasing), tiles (TEXTURES.md +- 15 % or registered),
  metallic only in the allow-list, contrast at the class midpoint, saturation under its cap, violet <= 0.5 %, content gate,
  coverage preservation, VRAM 374.7 MiB (cap 900).
- texprep --check: 24 / 24 current. Contact sheet ExternalTextures/previews/contact.jpg looked at (00:05 and 01:00 versions):
  no text, logos or people; no baked sun (ConcreteAlley / AsphaltAlley after the fit); repeats visible only in plain 3 x 3
  tilings (plaster blobs, asphalt_02's crack, the bark lichen line), which the shaders' hex / module / panel shuffles break.
TP-1 READY 2026-11-16 02:00
- For FA / SU / FO: every key of your configs is processed in Assets/_Game/Textures/External (committed now); MaterialFactory
  .ExtMaps(key) wires a set, arrays by path; per-slice tiles, modules and registrations are in the texmeta (sliceTile,
  sliceTileV, sliceModuleU / V, sliceRegistration). Recalibration: send REQUEST TP lines (config changes stay yours).
### 02:05 r04 / r04dv9 (build with the prepass on by default, every other flag at 1)
- QA/captures/TP_r04 keyshots1080: 20 / 20 frames sane (no black / white / magenta pixel, mean luma 27-58, p1 13-30); contact
  sheet looked at: the same picture as r02 apart from the other packages' commits since 00:37; "1945 facade renderers in the
  depth prepass".
- Overdraw, primed correctly now (QA/captures/TP_r04dv9; shaded NocFacade fragments per visible facade pixel WITHOUT the
  prepass, i.e. what the prepass removes): KS04 1.78 (p95 4), KS05 1.41 (p95 2), KS09 3.53 (p95 8), KS17 3.25 (p95 7); also
  KS07 2.72, KS10 3.22, KS11 3.79, KS13 3.12. With the prepass each visible pixel is shaded once. TP-0 step 2 closed: above 1.5,
  prepass added (TEXTURE_PLAN 2.12), default on.
DONE TP FO REQUEST LeafCrown outputs: not part of a commit until "FO ATLAS FINAL" in FO.md; then TP commits LeafCrown (PNGs +
  texmeta) as a calibration milestone (lead decision 4: at most two more texture commits after d2c7d35). d2c7d35 holds the 02:00
  LeafCrown; FO's 02:20 regeneration stays uncommitted until then.
### 02:20 TP-tall02 (full EditMode suite): 357 / 359. Not TP: Beat02BoulevardTests.Blades_Are14x200 (bf38311 test; PA's blade
  colours on purpose, PA's REQUEST INT for the unowned test file), FoliageTests.Materials_V2_AlphaToCoverage_AndQueues (FO, new,
  mid-calibration: "no puddle blots on the lawn" 0.1). Every TP test passes (TextureTests 10, NocTexturingTests 7, ShaderParityTests
  2, ProjectTests, MaterialTests).
### 02:20- infrastructure duty: logs checked for "REQUEST TP:" every 10 min until FA / SU / FO write their DONE (PA, KO, GE done).
DONE TP FO REQUEST NocLookGlobals.cs: DefaultFoliageV = 2 (FO accepted, FO.md 03:05; GardenDims.DefaultFoliageV is 2; nothing in a
  shader reads _NocFoliageV, so no picture changes); fastcheck OK, TP-compile01 Unity CompileCheck SUCCESS.
DONE TP FO REQUEST LeafCrown ("FO ATLAS FINAL 03:05"): LeafCrown_{Albedo,Normal,Mask}.png + LeafCrown.texmeta.json committed as
  texture calibration commit 1 of the 2 allowed after d2c7d35 (lead decision 4); texprep --check current for it against FO's
  foliage.json as of 02:20 (FO commits foliage.json: until then HEAD's --check on LeafCrown is stale by design).
DONE TP SU REQUEST NocLookGlobals.cs: DefaultSurfV = 2 (SU accepted, SU.md 03:25): SU's v2 near surfaces, wet floors, glass,
  gratings, lines and shopfronts are now the default picture; -surf-v 1 keeps v1 for A/B until TP-2. fastcheck OK; TP-tall03 (full
  EditMode with the new default): 360 / 361, the only failure PA's Blades_Are14x200 (blade colour order, unowned test, REQUEST INT).
  (FO's flag note: FO's request line now says "accepted 02:45"; the clock note in FO.md explains "03:05".)
### 03:52 infrastructure duty ends: PA, KO, FA, SU, FO and GE all carry their DONE line; no open "REQUEST TP:" (FA 13, FO 80 /
  234 / 264, SU 261 answered above). Builds/lookdev_TP deleted (W: 20 GB free); kept capture folders: QA/captures/TP_r01
  (TP-0 acceptance), TP_r04 (prepass on by default), TP_r04dv9 (primed overdraw). Texture commits: d2c7d35 (TP-1), bba89b2
  (calibration 1, LeafCrown); one calibration commit remains under lead decision 4.
TP DONE 2026-11-16 03:53
