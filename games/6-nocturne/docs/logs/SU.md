# SU log - near surfaces, wet floors, glass and lines (Look v2, TEXTURE_PLAN 8.3 SU, section 3)

Owner SU. Authority: D-027 / D-028 / D-029 and the lead decisions of the build workflow. Package steps SU-1 .. SU-5.
Exclusive files: TEXTURE_PLAN 8.3 (SU) and 8.4 (SU rows), as amended by the lead.

## 2026-11-14 09:30 start
- Read: TEXTURE_PLAN 0, 1 (1.1-1.7), 2.2-2.11 (contract, glass, wall macro), 3 (all), 7, 8 (8.1-8.4), 9, 10; LOOK_V2 1-8 and 10;
  TEXTURES.md TEX-R review and final sets; CRIT0 (#5, #8, #17, #24, #26, #29); critique-graybox01 (GB-10, GB-12, GB-18);
  BUILDER_BRIEF; STATE Commands; DEVELOPMENT D-001..D-029.
- Current code read: NocSurface, NocWet, NocGlass, NocGraybox, NocLine, NocMirror.hlsl, NocLitPass/Core/Common/Lighting,
  MaterialFactory, MaterialLibrary, NocWetRegistry, NocMirrorMath, NocMaterialSheet, VertexBake, WorldContext, the beat-1 files,
  OldQuarterBuilder, Beat02/03/05 builders.
- Markers: no TP.md / PA.md / FA.md yet. Work that does not depend on TP-0: the two carried Q1 gate lines, VertexBake.TopDistance
  and the AddMesh leak-anchor bake (inactive until a material declares _LeakAnchor), the surface.json draft.

## 09:50 requests
REQUEST PA: Assets/_Game/Scripts/Runtime/Systems/AtmosphereDriver.cs: add `public static void AmbientAt(double s, out Color sky, out Color equator, out Color ground)` that returns the LOOK_V2 6 trilight exactly as Apply uploads it to _NocAmbSky / _NocAmbEquator / _NocAmbGround (per-beat AmbientMul and the garden set blended, before LightCal), and make Apply call it; plus `public static double SkyFillAt(double s)` and `public static Color SkyFillColourAt(double s)` (the main-light intensity and normalised colour of LOOK_V2 6). SU's NocSurfaceMath.WallRadiance (TEXTURE_PLAN 2.11, the H4 wall macros) and WetMacroTests read them, so the wet-floor wall planes follow PA's rig without a second copy of its numbers.

## 09:50 Q1 gate lines (carried, no TP-0 dependency)
- AlleyFixtures.cs: `CatwalkZ0 = 96.0, CatwalkZ1 = 97.5` (D-029, GB-12); the catwalk deck uses them (AlleyLayout keeps the spec's 97.6
  for the AlleyWindows keep-out band: 0.1 m more margin, harmless). Beat01AlleyTests.KS02: the box uses the new constants and asserts
  the bottom edge > NDC -0.94.
- OldQuarterBuilder.cs: `BalconyTop = 3.6, BalconyT = 0.2` (slab y 3.4-3.6, D-029, GB-18), railing and pot follow it; `LampCentreY`
  back to 14.5.7's 5.0 (HeartLayout.LampY; the Phase 3 raise to 5.45 only existed to clear the y 4.2 slab). KS19 arithmetic: eye
  (1148.18, 1.7), slab front x 1154.6: window bottom ray at y 1.7 + 2.7 x 6.42 / 7.82 = 3.917 (0.32 above the slab); lamp's lowest
  point (y 4.9) ray at 1.7 + 3.2 x 6.42 / 9.02 = 3.978. Beat07HeartTests.KS19 asserts both > slab top + 0.05.
- Note for the lead: V's QA/VisibilitySpecWorld.cs:106 still models the spec slab at y 4.0-4.2 (V's file, not in this workflow).
- VertexBake.TopDistance (new) and WorldContext.AddMesh: vertex colour B = 255 saturate(d_top / 25.5 m) for materials with
  _LeakAnchor = 1 (union-find over shared indices: a face's top is its highest vertex). Inactive until SU-1 adds the property.
- Tools/texprep/surface.json drafted (18 sets, rows mapped; schema to be aligned with TP's texprep.py).

## Resume rule
Find the last step marked GREEN below and the last commit with "SU:" in the message (23646db: Q1 lines, leak bake, surface.json);
continue with the next open item. WIP of SU in the tree: NocMirror.hlsl (v2 wall blend NocWallReflect, NocPuddleNoise2,
NocRippleGrad2 behind -surf-v 2).

## 2026-11-14 12:30 resumed (after the 10:10 pause)
- Read own log, STATE PAUSED/RESUMED. WIP patch re-applied by the lead: NocMirror.hlsl diff present (NocWallReflect with the
  2.5 deg half-width blend and the second wall behind, guarded NC_Surf* defaults, NocPuddleNoise2, NocRippleGrad2).
- fastcheck 12:31: runtime/editor FAILED only in NocSchedule.cs (PA mid-edit, CloudNoiseAmp / CloudGlowGain): waiting for PA.
- Markers: PA TOKENS READY 09:38 only. No "REQUEST SU:" in any log.
- Plan while TP-0 is open: NocSurfaceConstants.cs (SU's file; TP adds it to the generators on request), NocSurfaceMath.cs
  (puddle / ripple / leak / line-paint twins), NocMirrorMath wall-blend twin, WetFloorTests / SurfaceTests, NocLine paint
  variation (no texture dependency), NocGrating draft; the rows and the textured NocSurface / NocWet paths after TP-0 lands.
- 12:44 NocSurfaceConstants.cs: TP-0 created it empty at 12:42 (handed over); SU filled it (52 `Surf*` literals: puddles 2.0 / 0.6 m +
  height bias 0.35, ripples cell 0.3 m / R <= 0.155 m / lambda 0.02 / slope 0.07, lobe, wall blend 2.5 deg, wetness, leaks, line
  paint, glass) and wrote only its own generated header NocSurfaceConstantsGen.hlsl with gen_hlsl.py's functions (no other
  header touched); both .meta files written with fixed GUIDs.
- 13:09 SU-t02 (SurfaceTests|WetFloorTests) did not run: the EditMode test assembly fails in Unity on FO's FoliageTests.cs.
REQUEST FO: Assets/_Game/Scripts/Tests/EditMode/Beats/FoliageTests.cs:33: `NocPaths` is not found when Unity compiles Megastructure.Tests.EditMode (Logs/SU-t02.log:318, CS0103; it lives in Megastructure.EditorTools, so the file needs `using Megastructure.EditorTools;`); fastcheck builds one assembly and does not see it. Every EditMode run fails until it is fixed.
DONE SU TP REQUEST NocSurfaceConstants.cs: re-applied at 12:44 after TP's empty hand-over (52 Surf* constants); SU commits it and
  NocSurfaceConstantsGen.hlsl (+ .meta).
- 13:20 received TP's step-5 hand-over: MaterialFactory.Surface.cs, MaterialLibrary.Surface.cs, MaterialTests.Surface.cs.

## 2026-11-15 21:30 resumed (third start, run wf_2c6d782f-763; D-030)
- Read own log, STATE "RESUMED, THIRD START" / "PAUSED AGAIN", TEXTURE_PLAN 3 and 8.3/8.4 SU, TP.md, FO.md. Tree: the b-patch
  re-applied (SU files: NocSurfaceConstants + Gen header, NocSurfaceMath, NocMirror v2 wall blend + NocPuddleNoise2 + NocRippleGrad2,
  NocMirrorMath.WallReflect twin, WetFloorTests WallBlend test, SurfaceTests, NocLine paint variation (NocLinePaint behind NocSurfV2),
  the handed-over MaterialFactory.Surface / MaterialLibrary.Surface / MaterialTests.Surface).
- Markers 21:30: PA TOKENS READY only (no TP-0 READY, TP-1 READY, PA-1 READY, FA-1 READY, TQ SETS READY).
- fastcheck 21:34 runtime 162 / editor 215 OK.
DONE FO REQUEST Beat05_Garden.cs: BuildTrees now calls `GardenTree.EmitCrowns(ctx, g, crowns, m);` (21:36; with -foliage-v 1 it is
  exactly the old `BridgeGardenKit.Emit(ctx, g, "crowns", crowns, m.Foliage)`).
- Commit plan (D-030 item 2, HEAD must stay consistent): the files that compile on HEAD alone (NocSurfaceConstants, its Gen header,
  NocSurfaceMath, NocMirrorMath, SurfaceTests, WetFloorTests, Beat05_Garden) go in as soon as SU-t04 passes; NocMirror.hlsl and
  NocLine.shader call TP's NocSurfV2() (NocCommon, uncommitted) and MaterialFactory/Library/Tests.Surface need TP's split of
  MaterialFactory.cs: those wait for TP's TP-0 commit.
- 21:38 SU-t04 (SurfaceTests|WetFloorTests|MaterialTests): 22/22 passed (QA/reports/SU-t04.xml; Logs/SU-t04.log). KS17 mirror point
  0.2481 of the distance (spec 0.246); streak h/w 4.92. Generated NocSurfaceConstantsGen.hlsl = gen_hlsl.py output (byte compare).
- GREEN 21:40: committed NocSurfaceConstants (+ Gen header), NocSurfaceMath, NocMirrorMath.WallReflect twin, SurfaceTests,
  WetFloorTests (they compile on HEAD alone). Held back: Beat05_Garden.cs (calls FO's uncommitted GardenTree.EmitCrowns), NocMirror.hlsl
  and NocLine.shader (TP's NocSurfV2), MaterialFactory/Library/Tests.Surface (TP's split) - committed after their owners' commits.
- 21:50-22:20 SU-2 (TEXTURE_PLAN 2.11), all behind -surf-v 2 (C#: NocLookGlobals.SurfV == 2; v1 macros unchanged):
  - NocSurfaceMath.WallRadiance(token, s, nx, nz, factor, porosity, wet) = token x factor x 0.96 x WetDarkening(porosity, wet 0.6)
    x (PA's trilight equator(s) + sky fill(s) x max(0, N.L)); FillDirection = -forward of Euler(60, 35): (-0.287, 0.866, -0.410)
    (N.L 0.41 on a wall facing -z, 0 on +x); FacadeMacro(mode, region, s) = FA's NocFacadeMath.MeanRadiance with ambY = luma of PA's
    equator(s); AlleyWallMacro(s, nx) = WallRadiance(ConcreteDark) + the lit windows (0.45 x 1.0 x 1.4 / (4.5 x 3.2) = 4.4 % of
    0.6 WinWarm x 0.9 + 0.4 WinCold x 0.6).
  - Call sites: Beat01_Alley (AlleyWallMacro per side at s 60), Beat02_Boulevard (FacadeMacro(0, D, 260)), Beat03_Edge
    (FacadeMacro(1, D, 640)), Beat05_Garden (FacadeMacro(1, D, mid)), OldQuarterBuilder (WallRadiance(Brick, 1447.5, -x, porosity
    0.5)), NocMaterialSheet (WallRadiance(ConcreteDark, 0, +z)). NocWetRegistry: RegisteredWalls / GetWall (tests).
  - Constants: SurfPor* (0.4 / 0.3 / 0.5 / 0.35 / 0.45 / 0.4), SurfWetVertical 0.6, SurfWetSheltered 0.15, SurfLambert 0.96,
    SurfMacroTol 0.05, SurfRippleVar 0.00126 (mean |g|^2 per unit rho, measured in Python over 2e5 points: 0.0012635 at rho 1,
    0.000642 at rho 0.5).
  - WetMacroTests (new): v2 every registered macro within 5 % of its mean at mid-s; v1 macros unchanged; WallRadiance follows
    the rig. SU-t06: boulevard (0.0126, 0.0111, 0.0106), Gulf (0.0208, 0.0190, 0.0190), garden notch (0.0187, 0.0190, 0.0214).
- 22:15 NocWet v2 (SU-1, behind NocSurfV2()): NocPuddleNoise2 + height bias 0.35 (0.5 - h) thresholded at the row's _PuddleTh,
  edge 0.035 + fwidth(noise) + 0.35 fwidth(h), fade to _PuddleFraction below 3-6 px of 0.6 m; NocRippleGrad2 x 0.07 inside
  puddles only (faded 3-6 px of lambda 0.02 m), the removed variance 0.07^2 x 0.00126 x rho x (1 - fade^2) x m into the lobe;
  alpha_dry = max((1 - lerp(_SmoothMin, _SmoothMax, mask.a))^2, 0.18), puddles 0.08, refl 0.55 / 0.95; flat puddle normals;
  FloorSteelDeck analytic plates 2.4 x 1.2 m (zero-mean sag 0.25 (r^2 - 0.5), 6 mm welds by NocFilteredGrid1: no water, x0.6
  albedo); debug view 8 (0.25 m checker x mip colour). All derivatives before the v2 branch; v1 arithmetic unchanged.
  Factory rows: _PuddleTh measured per row (NocSurfaceMath.HeightHistogram 16 384 hashed texels of the generated family, 4e5
  samples), wet smoothness ranges asphalt 0.35-0.55, stone/gray 0.45-0.65, deck 0.55-0.75, garden 0.4-0.6, plaza 0.45-0.65.
  SU-t06: the first histogram (regular 128 lattice) aliased with the asphalt recipe (FloorAlley 0.409 on a 96 lattice): hashed
  points now.
- 22:20 NocGlass v2 (SU-3 part, CRIT0 #17): alpha = Fresnel only, tint GlassDark, env = sky over the registered wall planes
  (NocWallReflect), MetalPaint frames 0.08 m every 6 m along the pane's dominant horizontal axis (NocFilteredGrid1, mean below
  2 px); v1 path unchanged (custom.w = 0).
- 22:25 GlassDark row (NocGraybox, GlassDark token, smoothness 0.5, AO 0.5) + MaterialLibrary.GlassDark; OldQuarterBuilder panes
  use it with -surf-v 2 (v1: the tinted Concrete, now with the GlassDark token per PA).
DONE PA REQUEST G1Kit.cs: NocNeon batch loop pal -1..7; G1Materials.Neon 9 slots, index Clamp(pal + 1, 0, 8). PA may switch
  BladeSign.ExtendedPalettes to true.
DONE PA REQUEST AlleyFixtures.cs: sign lights r 6 i 1.5 lerp(c, WinNeutral, 0.4) on 4 signs (i % 6 == 0: 0, 6, 12, 18); mouth light
  lerp(HazeLow, CloudHot, 0.5) r 25 i 2.0; blade palettes: magenta on sign 0 only, the other palette-0 signs SignWhite (slot 4), slot 3
  (violet) -> WinNeutral (slot 5), BladeColour follows; mouth wet strip CloudHot x 0.55; ten door bulkheads LampWarm r 7 i 1.6 at
  y 2.6, 0.25 m off the wall, z 6 + 11.5 k alternating west / east, each with a 0.24 x 0.12 m LampWarm x 1.6 lens (G1Mat.EmitLamp).
  Beat 1 lights: bar 1 + bulkheads 10 + signs 4 + vending 3 + mouth 1 = 19 (cap 20).
DONE PA REQUEST G1Materials.cs: G1_Rail SteelF0 x 1.0 (was SignWhite x 0.25); bar / shop factors kept.
DONE PA REQUEST NoodleBar.cs: bar light WinWarm r 9 i 6 (BarLightIntensityV2; AlleyLayout keeps 7.0); the sign is amber (no violet).
DONE PA REQUEST VendingMachines.cs: light and wet-source colour lerp(NeonCyan, SignWhite, 0.4), r 4, i 1.2.
DONE PA REQUEST Beat03_Edge.cs: lamp heads LampWarm x 1.6, balustrade lights SignWhite, portal ring NeonCyan, layer lines SignWhite;
  SectionMacro (v1 wall macro) hue = FA's MeanHueV2(w_f, n_f at s 640) scaled to the v1 mix's luminance.
DONE PA REQUEST Beat05_Garden.cs: the 8 leaf point lights lerp(LeafGlow, WinNeutral, 0.5) r 30 i 1.0 (no lantern clusters exist yet:
  Phase 4).
DONE PA REQUEST OldQuarterBuilder.cs: goalLamp core GoalLamp with blue x 0.3 (same gain), room / curtain GoalLamp; rail metal
  MetalPaint; dark glass GlassDark token; entrance light lerp(WinCold, WinNeutral, 0.5) r 8 i 1.5 at (TenX0 - 0.4, 2.8,
  GoalZC + 4.0) (4.66 m from the window centre).
DONE PA REQUEST Beat07HeartTests.cs: NoAmber_ButTheWindow emitters SignWhite (for WinCold) + KoiPatch.
DONE FA REQUEST AlleyBlocks.cs: WestSeed = 0xFFFFFFF2u (SkinOverride(OverrideAlleyWest): S1 precast), EastSeed = 0xFFFFFFF3u
  (SkinOverride(OverrideAlleyEast): S4 tiled with loggias) (23:50).
- 23:00-23:30 SU-3 part 2: NocGrating.shader (new): analytic bar mesh 62.5 mm both ways, bars 10 / 14 mm (mean coverage 0.348),
  widened by depth 30 mm x tan(theta) across each family (solid at grazing view), box filtered, mean below 3 px of the pitch;
  AlphaToMask On, Cull Off, ZWrite On, queue 2450, NOC_ALPHA_OUT + NOC_FRONT_FACE; depth-type passes clip at coverage 0.5;
  MetalPaint x macro +-15 % x rust-dark speckle (0.4 m, faded below 2-4 px). C# twin NocSurfaceMath.GratingCover + FilteredGrid1.
  Row Grating (MetalPaint, smoothness 0.5) + MaterialLibrary.Grating + G1Mat.Grating (fallback SteelPainted). AlleyFixtures with
  -surf-v 2: the catwalk deck is a grating plate at y 9.2 on two 0.15 x 0.05 m stringers and cross beams every 1.1 m (5 mm
  under the plate); each fire-escape landing a grating plate in four 0.06 x 0.04 m edge angles. v1 keeps the boxes.
- NocMaterialSheet (-surf-v 2 only): GlassDark slab at x -16 and a horizontal Grating plate at x 15-17, y 1.2.
- NocSurfaceMath split: FacadeMacro moved to NocSurfaceMath.Facade.cs (it reads FA's NocFacadeMath).
- 23:24 SU-rebuild01: RebuildAll SUCCESS, 32/32 materials (GlassDark, Grating new). SU-t08 71/72 (the grating near-mean sampled a
  non-integer number of pitches: 0.360 vs 0.348; now 8 x 8 whole pitches). ProjectTests, ShaderParityTests, MaterialTests,
  WetMacroTests, Beat01/03/07 tests green. SU-t07 failures not mine: Beat02BoulevardTests.Blades (PA, REQUEST INT),
  Beat05GardenTests/Beat04 FloorSeam_DeckEnd (deck vertex x 695, GE/FO geometry).
- Note: HEAD does not compile on its own at 23:20 (PA's committed AtmosphereDriver uses TP's uncommitted NocLookGlobals); SU's
  v2 files depend on NocLookGlobals / NocSurfV2 / the MaterialFactory split too, so SU commits its batch as the other owners do
  (whole-tree green) and HEAD becomes consistent with TP's TP-0 commit.

## 2026-11-15 23:45 - 2026-11-16 01:05
- Markers: TP-0 READY 23:30 (TP committed the split 489beeb with SU's edited .Surface partials, d27c6a9), FA-1 READY 22:58,
  PA-1 READY 23:53, TQ SETS READY 00:14. TP-1 not READY (Textures/External holds drafts; not wired into SU rows yet).
- 23:47 lookdev_SU build SUCCESS (0 problems). Captures SU_r01v2 (keyshots1080, -surf-v 2, -freeze-clock) and SU_r01v1 (-surf-v 1,
  same build). Looked at KS00, KS02 (+ x2.5 / x6 crops of the catwalk), KS10, KS13, KS06 / KS04 / KS17 floor crops side by side.
  v1 -> v2 mean |diff| per still: KS00 0.43, KS01 0.52, KS02 0.14, KS10 2.14, KS12 2.43, KS13 7.34, KS14 1.50, KS17 0.43, KS19 0.34.
  - KS00 floor (x 600-1400, y 520-900): v1 large smooth blobs; v2 smaller ragged puddles with height-biased edges (mean luma 27.4 ->
    27.7, std 5.8 -> 6.1) but they still read as dark stains: nothing bright to mirror (SU-01) -> the west door bulkheads are now
    wet sources (5 discs LampWarm x 1.6; beats 1-2 at most 29 active, cap 32; with all 10 it was 34).
  - KS02: the catwalk is an open grating from below (the tower face shows through); low contrast, the 6 px mesh barely reads (SU-03).
  - KS13 / KS10: the glass is darker and neutral (KS13 mean luma 52.9 -> 45.3), frames every 6 m (a near frame as a flat band,
    SU-04); the face tubes far below show their frames as 1 px rings. KS10 balustrade without the grey film.
  - KS06 floor: v1's blue blotches gone; v2 darker (facade mean D 0.0126 instead of WinCold x 0.035) (SU-05).
  - image_metrics flags NaN-like / isolated pixels in KS11, KS12, KS16, KS17, KS18 in BOTH v1 and v2 (rain / sparks, not SU).
- 00:20 NocSurface v2 applied after TP-0 READY: TP's NocTriSample (hex triplanar, rotation per row) or NocHexSetup on UV0 metres
  (_Mapping 1), surface-gradient normals, Lagarde wetness with _Porosity / _WetLevel, edge wear, leaks anchored to the face top
  (2 m columns, hashed stretch 1-2 and offset, the nearest neighbour column blended over 0.15 m at the border: no vertical seam),
  the OverlayArr slice 0 decal path (_LeakDecal 1; far: 2 x SurfLeakMean (0.242 = texmeta 0.121 x 2) x (1 - d / len)) or the analytic
  stand-in, MicroArr micro detail on the dominant plane (w_det = smoothstep(4, 8, 0.5 / fp) x _MicroGain), debug view 8. v1 taps stay
  outside the uniform branch, every v2 derivative before it. 5 samplers. Rows: SurfV2(porosity, wet level, leak anchor, gain,
  rotation, seed): Concrete 0.4 / leaks 0.4, StoneWall 0.35 / 0.3 (rotation 0), SteelPainted 0 / 0.3, SteelBare and Brass offsets
  only, Moss 0.4, Bark 0.45 (offsets only; _Mapping stays 0 until FO's bark UV0 is the default), Asphalt (token). The leak bake in
  WorldContext.AddMesh runs only with -surf-v 2.
- 00:25 SU-rebuild02 SUCCESS (32 materials). SU-t11: 71/72 (MaterialContractHolds: stale .mat before the rebuild); SU-t12 83/84 after
  it (the failure is TP's TextureTests.Import_AlbedoLastMipMeanIsHalfLinear on the TP-1 drafts). Wet sources beats 1-2: 70 total,
  29 active max.
- GREEN 01:05: commit of the SU batch (beat files, shaders, constants, math, tests, the two new materials, critique-SU.md).
- 01:17-01:27 SU BROKE NocWet.shader for 10 minutes (the v2 photo lookup used NocModule without including NocTexturing.hlsl;
  and the photo rows set _SmoothMin2 / _SmoothMax2 that NocWet did not declare). Affected: lookdev_TP 01:20 (BuildWindows FAILED:
  NocWet has compile errors) and FO-rebuild04 01:24 (RebuildAll FAILED: NocWet has no _SmoothMin2 / _SmoothMax2). Fixed 01:27
  (include + properties); TP and FO: please re-run those. Sorry.
- 01:10-01:25 photo sets wired on the v2-only properties (no v1 change): NocSurface / NocWet get _Tex2, _BaseColor2, _AlbedoTex2 /
  _NormalTex2 / _MaskTex2 (sampled with the v1 maps' samplers: NocSurface 5 samplers, NocWet 3), _Tile2 / _TileV2 from the texmeta,
  _SmoothMin2 / _SmoothMax2 0..1 (absolute mask), _NormalStrength2 1, the module shuffle (_AntiTiling, _ModuleU / V, _Bond,
  _BondAxis from the texmeta; Pavers half bond along v = swapped axes; Brick whole courses: module u = the tile). NocWet: hex or
  module on planar xz, _PuddleTh2 measured from the photo mask's own heights (MaterialFactory.PhotoHeights, 16 384 hashed texels),
  _WetGloss2 lifts the dry absolute smoothness toward 0.88 (asphalt 0.4, pavers 0.5, deck 0.5, garden 0.4, plaza 0.55).
  Rows: Concrete, StoneWall (Granite), SteelPainted (MetalPaint), SteelBare / Brass (SteelPlates), Moss, Bark, Asphalt; floors
  GrayFloor / FloorStone (Pavers), FloorAlley (AsphaltAlley), FloorSteelDeck (DeckPlate, offsets only), FloorGardenPath
  (GardenTiles), FloorPlaza (Granite). New rows ConcreteAlley (alley walls and dead end, G1Mat.ConcreteAlley, v1 falls back to
  Concrete), Brick (tenement) and Plaster (neighbours) on -surf-v 2 only. Leak decal (OverlayArr 0) and micro (MicroArr) on.
  The look is not judged until "TP-1 READY" (drafts).
- 01:40 offline shader check (to never break the others' builds again): a scratch harness compiles each pass of an SU shader with
  the Windows SDK fxc (defines SHADER_API_D3D11 / target 4.5, the URP and core ShaderLibrary copied with one empty-macro patch,
  /Gec): NocSurface, NocWet, NocGlass, NocGrating, NocLine all passes OK; a planted identifier error is caught. Unity stays the gate.
- 01:40 SU-rebuild04 SUCCESS (35 materials: ConcreteAlley, Brick, Plaster new); SU-t14 63/63 (SurfaceTests, Beat01AlleyTests,
  WetMacroTests, WetFloorTests, MaterialTests incl. SuShaders_Compile_AndV2RowsExist, Beat07HeartTests, ProjectTests). Photo puddle
  thresholds (fraction): Pavers 0.4233 (0.351), AsphaltAlley 0.5540 (0.348), DeckPlate + sag 0.6748 (0.352), GardenTiles 0.5696
  (0.252), Granite 0.5465 (0.351). GREEN: commit.
- 02:00 r03 (build 01:46, photo drafts of texprep v4, functional check only, not calibration): SU_r03v2 (keyshots1080), SU_r03tc
  (texclose, -rain 0), SU_r03sheetL / W (-material-sheet). Looked at the sheet, texclose contact and full-size tc_tenement_brick,
  tc_alley_wall (contrast-stretched). texclose luma mean / std: alley wall 22.5 / 4.75, bridge deck 19.3 / 1.71, plaza 35.2 / 3.84,
  podium panel 13.5 / 1.70, tenement brick 27.4 / 5.46, trunk 16.2 / 2.07 (glass 43.3, noodle bar 105).
  - The brick reads as real brick (running bond, mortar lighter, warm from the window); the alley wall reads as cast concrete with
    pores and tie holes; the plaza and the sheet floor show real granite / pavers with the half bond.
  - Defects found and fixed in the tree: (a) the leak decal and micro layer sampled UNBOUND arrays in r03 (the rows had gains on but
    the OverlayArr / MicroArr were never wired): Arrays() now wires them and turns the gains off without them; the sheet's spheres
    showed dark cube-corner regions from it; (b) the micro layer switched hard at the dominant triplanar plane (a seam on chamfers
    and spheres): now every active plane with its weight; (c) brick whole courses hashed per (u cell, course): a vertical cut through
    the bricks every 1.9 m (seen at x 745 in tc_tenement_brick): now _ModuleU 0 = per-course source only + a hashed u offset per
    course. (d) NocGlass v2 got its grime (OverlayArr 1, 1.5 m: roughness 0.05 -> 0.20, alpha + 0.25 grime, albedo + 0.03 grime) and
    rain rivulets (OverlayArr 2, 1.0 m, within 12 m, faded from 7.2 m) on the pane's (run, height) coordinates (fxc-checked).
- Markers 02:05: TP-1 READY 02:00 (d2c7d35). FA committed FA-1..3 (2f3da2b, NocInterior box rooms).
DONE FA REQUEST NocMirror.hlsl: NocWallReflect is committed (08a8827, 01:05).
DONE PA REQUEST AlleyFixtures.cs halos: NocHalos.Queue("AlleyBulkheads", lens centre, LampWarm x EmissionSign, 7.0) per door
  bulkhead (02:15). No halo for the noodle-bar sign (no lamp head).
DONE FO REQUEST MaterialFactory.Surface.cs: Bark's photo set samples UV0 metres (_Mapping 1, trident_maple_bark, offsets only);
  FloorMoss row + MaterialLibrary.FloorMoss (NocWet, Moss token, puddles 0.10, _AlphaDry 0.55, _ReflDry 0.35, v2 wet smoothness
  0.25-0.40, photo Moss001 with wet gloss 0.25) (02:15). NocWet v2 now uses the row's _ReflDry (0.5; moss 0.35) instead of 0.55.
- 02:10 r04 (build 02:08: arrays wired, micro blended, whole courses, glass grime / rivulets): SU_r04v2 keyshots1080. Looked at the
  contact sheet, KS00, KS01, KS16 full size. KS00 / KS01: the alley walls are cast concrete with pores and tie holes, the bulkhead pools
  graze them (relief visible), the asphalt shows cracks and a puddle sheen mirroring the bar light; KS19 a brick tenement lit warm
  around the window. tiling.py SU_r04v2: 0 FAIL, 1 WARN (KS19 wall_w 0.266 at lag (-23, 12): the brick bond, periodic by design);
  detail micro / meso: KS00 wall_w 3.30 / 0.77, wall_e 1.33 / 0.48, floor 1.23 / 0.19; KS01 wall_w 1.30 / 0.40; KS02 1.19 / 0.33 and
  1.41 / 0.36; KS19 wall_w 3.31 / 2.69, wall_e 1.57 / 0.62 (LOOK_V2 check 7 thresholds 1.4 / 1.0: the ambient-only walls fall under
  it; the pools carry the detail, as LOOK_V2 5.3 plans). tiling.py SU_r03tc: 0 FAIL.
  - Found: the anchored rows had lost the drip streaks (streak = leak only): the tall alley walls had no streak at eye level; now
    max(leak, drip) on every vertical face.
- 02:25 SU-rebuild06 SUCCESS, SU-t16 74/74 (SurfaceTests incl. the puddle-normal / wet-pool twins, MaterialTests incl.
  PhotoRows_WholeModules_NoRotationForDirectionalSets, Beat05GardenTests, ProjectTests). GREEN: commit.
- 02:35 disk W: 18 GB free (under the 20 GB floor; not SU: SU's captures are ~0.2 GB after deleting SU_r01v2 / r02* / r03* (numbers
  above), Builds/lookdev_SU 0.4 GB). NOTE LEAD: someone else's output is filling W:.
- 02:30-02:45 r05 (build 02:29): SU_r05v2 (keyshots1080), SU_r05tc (texclose -rain 0), SU_r05sheetL. The sheet's spheres are solid now
  (the r03 cube-corner artefact was the unbound micro array); SteelPlates / MetalPaint / Granite / Concrete slabs read as photo
  materials, the pavers' half bond on the floor. texclose luma mean / std: alley wall 18.7 / 2.45, bridge deck 19.3 / 1.70, plaza
  35.2 / 3.84, tenement brick 20.3 / 5.23, trunk 16.2 / 2.12. KS19 r04 = r05 (brick far from lights 18.9 = LOOK_V2 5.3's ambient-only
  brick 18.4; within 3 m of the window 30.8); r03's 29.8 used texprep-v3 drafts (TP-1 final sets since 02:00). KS19 stretched crop
  around the window: brick relief under the warm spill, no leak band beside the opening.
- 02:40 SU-4 (FA-3's NocInterior committed 2f3da2b): G1Mesh.Quad writes panel-local metric UV0 (u along the bottom edge to the
  viewer's right, v up; level quads world x / z) and UV1 = (W, H); NocShopfront.shader (new; fxc-checked): FA's box room behind the
  glass (NocRoomRawV2 / exact NocRoomMeanV2: raw / mean keeps the mean), shelves and product rows on the back and side walls (hashed
  items 0.55-1.45, 12 % gaps, 3 cm boards, faded to their mean below 2-4 px of the item), a ceiling light strip near the glass, light
  = slot emission x 0.6 (CRIT0 #24; the bloom carries the brightness), dark glass on top. Row Shopfront + MaterialLibrary.Shopfront;
  G1Materials on -surf-v 2 derives one per emissive slot: bar window WinWarm x 1.2 (3 m), alley windows / shops / transoms (4 m),
  vending fronts NeonCyan x 0.35 (0.35 m deep, 0.22 m shelves, 0.07 m items). Test G1Quad_MetricUV0_PanelSizeInUV1_NoFacadePart
  (tracks every Quad builder over beats 1-2: no NocFacade part).
- 02:43 SU-rebuild07 SUCCESS (37 materials); SU-t17 73/75: mine all green; failures Beat02BoulevardTests.Blades (PA, REQUEST INT) and
  ProjectTests.MaterialContractHolds on FA's Facade row (_FaStreakOn / _OverlayMask: FA edited its row after my rebuild). GREEN: commit.
- 02:48 r06 (build 02:46, shopfronts): SU_r06v2 keyshots1080. KS01: the bar's back wall is a lit shelf room with product rows
  behind the counter (parallax, 3 rows); it read as a near-white "barcode" wall -> products get muted packaging tints (half of the
  items, luma ~1) so the mean holds (03:02, fxc-checked). Alley windows at grazing view show the side wall's gradient (pale
  WinCold rooms), the boulevard shop row (KS03) is x0.6 and no longer clipped (region y 400-1080 mean luma 28.9 -> 28.7).
  r05dv8 (debug view 8): the 0.25 m checker on every SU surface, near walls green (1 texel / px at 2-3 m), floors yellow-red with
  distance and grazing (minified, as expected); NocGlass / NocLine / NocNeon black (no view 8).
  SU-11 fixed in the tree: G1Materials.ShopSourceGain (0.6 on -surf-v 2) scales the bar window and vending wet sources.
- 03:04 SU-t18 52/52. GREEN: commit (shop source gain, product tints, critique).
- 03:10 r07 (build 03:09): SU_r07v2 keyshots1080, SU_r07tp (TQ's texpans, -sequence texpans -freeze-clock -rain 0, -surf-v 2),
  SU_r07st (statics, -sequence statics -freeze-clock -rain 0, -surf-v 2). sequences.py:
  - texpans 0 FAIL: KS00 PASS (swimming p95 0.296 / 0.117 / 0.124 / 0.085 px per 64 px block group, max 0.613 px, pan 14.5 px), KS17
    PASS; WARN only KS09 (one block max 3.8 px at [1024, 448]: the Gulf face, not SU) and KS15 (garden canopy, FO).
  - statics (the TEXTURE_PLAN SU check "static -rain 0 floor change <= 2 levels"): all 7 beats PASS, max |frame - frame 0| 0.715
    levels (beats 2, 3, 4, 7), 0.0 in 1, 5, 6; p99.9 0.00.
- 03:12 Wood (noodle-bar counter, Bark token x 1.3, offsets only, sheltered 0.15), TileWall (bar back wall, Plaster x 1.6, module
  shuffle 33 x 10 per tile), Fabric (Plaster, porosity 0.3, emission x the weave: _EmitByAlbedo) rows + fields + G1Mat.Wood /
  TileWall (v1 fallback SteelPainted / Concrete); the goal curtain is emissive Fabric on -surf-v 2 (TEXTURE_PLAN 3.5).
  SU-t19: Fabric's smoothness range failed the 8.11 variation test (0.073): range 0.05-0.25, macro 0.04. SU-rebuild09 (40 materials),
  SU-t20 66/66. GREEN: commit.
- 03:25 SU acceptance holds (TEXTURE_PLAN 8.3 SU tests all green in SurfaceTests / WetFloorTests / WetMacroTests / MaterialTests:
  whole modules, no rotation for directional sets, puddle normal up at m = 1, puddle fraction 0.35 +- 0.02 per wet row from the real
  height histograms (generated and photo), wetness pools low, leak anchoring on test boxes, overlay / micro means, KS17 mirror point
  0.2481 (D-012), continuous wall blend, WetMacro within 5 %, G1 quad UV0 metric with no facade part; statics <= 2 levels (max 0.715);
  texpans KS00 / KS17 PASS; tiling.py 0 FAIL; debug view 8 as designed). SU-5 step 1:
REQUEST TP: Scripts/Runtime/Rendering/NocLookGlobals.cs: `DefaultSurfV = 1` -> `DefaultSurfV = 2` (SU accepted, 03:25; the v2 near
  surfaces, wet floors, glass, gratings, lines and shopfronts become the default picture; -surf-v 1 still selects v1 for the lead's
  A/B until TP-2 deletes the v1 paths, TEXTURE_PLAN 7.6).
- 03:30 NocSurface: the v1 triplanar taps moved inside the v1 branch with explicit gradients (dP/dx, dP/dy of NocTriSetup, taken
  before the branch): v2 (the coming default) no longer pays for nine unused taps; fxc-checked.
- 03:28 r08 (build 03:24: Wood / TileWall / Fabric, tints, shop source gain): SU_r08v2, SU_r08tc, SU_r08v1. KS01: the bar shows a
  wood counter and a back wall of tinted product rows (no more white barcode); KS19: the curtain's weave (0.27 m tile) is sub-pixel
  at 12 m (faded to its mean, as designed) and the goal room read flat -> the room surfaces glow through plaster (_EmitByAlbedo,
  mean kept) on -surf-v 2 (03:31). texclose r08 luma mean / std: alley wall 18.7 / 2.45, noodle bar 90.1 / 65.7 (was 105 / 87),
  tenement brick 20.3 / 5.23, plaza 35.2 / 3.84, bridge deck 19.3 / 1.70.
- 03:34 FULL EditMode suite SU-tall: 360 / 361; the one failure is Beat02BoulevardTests.Blades_Are14x200_AtTheSpecPositions (PA's
  6-cycle blade palette; REQUEST INT in PA.md). GREEN: commit.
- 03:45 r09 (build 03:37: v1 taps in the v1 branch, goal-room plaster glow): SU_r09v2, SU_r09v1, SU_r09sheetL. v1 parity of the
  gradient change: r08v1 vs r09v1 at KS00 / KS01 / KS10 / KS14 / KS17 / KS19 mean |d| <= 0.001, max 1-2 codes (identical);
  KS19 v2: the goal room's walls show plaster variation under the warm glow (region 780-980 x 500-720 std 50.4 -> 51.5).
  tiling.py SU_r09v2: 0 FAIL (KS19 wall_w WARN 0.267 = the brick bond). image_metrics SU_r09v2 vs SU_r09v1: the NaN-like / isolated
  / banding fails are the same set in both (rain, facades, koi, plaza / mouth banding), except v2 KS13 banding[abyss] 1.956 (the
  clear glass shows the abyss haze: PA's H1) and dark single-pixel brick pits in KS19 (values 23-82, frame min 7: texture, not NaN).
- Folders kept: QA/captures/SU_r09v2 (final key shots, -surf-v 2), SU_r09v1 (same build, -surf-v 1), SU_r09sheetL (material sheet),
  SU_r08tc (texclose), SU_r07tp (texpans), SU_r01v1 (the first v1 baseline). Deleted after logging: r01v2, r02*, r03*, r04v2,
  r05*, r06*, r07v2, r07st (2.2 GB; sequences_SU_r07st.md holds the numbers), r08v1, r08v2. W: 20 GB free at 03:45.
- surface.json: its comment no longer calls it a draft (TP's schema; texprep --check Concrete / Brick current).
- SU-5 state: calibration to PA's tokens holds (LOOK_V2 5.3: alley walls outside pools 18.7-24.6 vs 19.6, brick ambient-only 18.9
  vs 18.4, the tenement within 3 m of the window 30.8 in 26-40, sheet / texclose plausible); the default switch is REQUEST TP
  (NocLookGlobals.DefaultSurfV is TP's file; open at 03:45). The v1 paths stay for the lead's A/B and INT's bisect by flags (as FO
  kept -foliage-v 1); they are dormant in v2 (no v1 tap runs on -surf-v 2). Deleting them is the next SU / TP-2 step after INT.
- 03:47 TP carried the default switch (TP.md 274: NocLookGlobals.DefaultSurfV = 2; TP-tall03 360 / 361). SU-tall2 (full EditMode
  suite with -surf-v 2 as the default, 03:50): 360 / 361, the only failure Beat02BoulevardTests.Blades_Are14x200_AtTheSpecPositions
  (PA's blade order; REQUEST INT in PA.md). Open requests from SU: none. Open requests to SU: none (FO, FA, PA, TP all answered).
SU DONE 2026-11-16 03:50
