# FA log - facades, glass, interiors (Look v2, TEXTURE_PLAN 8.3 FA, section 2)

Owner FA. Exclusive files: TEXTURE_PLAN 8.3 FA list + CityMesher.cs AddFace (lead decision 1).
Requests to FA are answered here as "DONE FA <request>".

## Steps
- 09:35 start. Read TEXTURE_PLAN 0, 1.2-1.7, 2.x, 7, 8, 9, 10; LOOK_V2 2, 4, 5, 6, 8; STATE, BUILDER_BRIEF, DEVELOPMENT D-001..D-029;
  CRIT0, critique-graybox01.
- Note: TEXTURE_PLAN 2.4 still carries the draft window light (lit 0.26, Y_med 0.15, stand-by 0.0025). LOOK_V2 section 4 (final,
  ART-F, D-028) owns light levels: lit 0.18, Y_med 0.12, stand-by x0.0005, city mean 0.0114 linear. FA implements LOOK_V2's values.
- 09:55 FA created `Assets/_Game/Scripts/Runtime/Core/NocFacadeConstants.cs` itself (it is on FA's exclusive list; FA-1 and GE
  need it before TP-0 lands). Every exported name starts with `Fa` (no clash with NocturneConstants' `Facade*`).
REQUEST TP: Scripts/Runtime/Core/NocFacadeConstants.cs: the file already exists with FA's content - do not recreate or empty it; add it to Tools/gen_hlsl.py and Scripts/Editor/ShaderGen.cs as Shaders/Generated/NocFacadeConstantsGen.hlsl (guard NOC_FACADE_CONSTANTS_INCLUDED, `#define NC_<Name>` like NocConstantsGen). FA includes the header from Shaders/Include/NocFacadeSkin.hlsl itself.

## Resume after the pause (2026-11-14 12:25)
- Read this log, STATE PAUSED, TEXTURE_PLAN 2 / 8.3 FA / 8.4 / 9 / 10, LOOK_V2 4 / 5 / 8, CRIT0. The WIP patch had
  NocFacadeConstants.cs (new) and the v2 twin in NocFacadeMath.cs. At 12:25 fastcheck failed in PA's NocSchedule.cs (not mine);
  PA fixed it at 12:37 (fastcheck all OK), so I continued.
- 12:40 NocFacadeConstants: hex literals made decimal (FaOqBase 120, FaOverrideBase 240); TP's gen_hlsl.py now writes
  Shaders/Generated/NocFacadeConstantsGen.hlsl (my generated file; I regenerate only that file with TP's generator functions).
  FaLedgeGain 0.6 (x Y_med, WinNeutral).
- 12:45 FA-1 writers (2.2), every one keeps the v1 picture (the v1 shader never reads colour G or uv1.zw):
  - CityMesher.AddFace (lead decision 1): G = SkinByte(t.seed, Region(t.cx, t.cz)), uv1 = (uMin, uMax, f.y0, f.y1).
  - FacadeKit.Face (+ optional region: default the face centre) / Box (region of the box centre): G = SkinByte, uv1.zw = (y0, y1).
  - G4Kit.FacadeQuad (+ optional region: default the quad centre) / FacadeBox (box centre): G = SkinByte, both CurrentUV1 carry
    (min(a0.y, b0.y), max(a1.y, b1.y)).
  - StreetFacade: podium street, step and end faces use SkinOverride(4) (precast + stone base); the shop hashes keep `seed`.
- 12:50-13:05 FA-1 shader: new Shaders/Include/NocFacadeSkin.hlsl (skins, regions, cells, whole bays, rows, lit groups, zones,
  hue / brightness, windows, recess rectangles, spill and halo, neighbour-band means, per-panel tone, ledge line, LOOK_V2 edge
  colours) and the v2 path in NocFacade.shader behind NocFacadeV2() (uniform branch after every derivative; v1 code unchanged,
  its two derivatives now taken before the branch); NOC_PASS_UV1ZW; forward hook = NocShadeOcc(.., f.specOcc) + the glass
  reflection of the glass share (v1: specOcc 1 = NocShade). City term (D-017) on f.specOcc x the opaque share.
- Twin (NocFacadeMath): halo without albedo (the plan's E x 0.02 exp(-d / 0.4)), KnownY (y1 - y0 > 2 m), 8 groups max (HLSL
  loop), neighbour-band means (AddBandMeanPart = NocBandMeanPartV2), w_f / n_f from PA's NocSchedule.WarmShare / NeutralShare.
- Debug view 1 encoding: B = 16 (k + 1) for override k, 0 for regional skins (so override 0 is distinguishable).

## Third start (2026-11-15 21:28, after the second pause; D-030)
- 21:28 read this log, STATE (RESUMED THIRD START, PAUSED AGAIN), TEXTURE_PLAN 2 / 7 / 8.3 FA / 8.4 / 9, LOOK_V2 4 / 5, BUILDER_BRIEF,
  `git diff` of my files. No "REQUEST FA:" in any log. Markers: PA TOKENS READY only (no TP-0 READY yet). fastcheck OK (162 / 215).
- 21:35 FA-t01 (FacadeSkinTests|FacadeTests|CityTests): 14/16. Failures:
  1. Contract: |grad u| out of [0.9, 1.1] on 4 362 triangles. Cause: the test also measured roof triangles (mode 2, slice 0 only: u is
     meaningless there) and the tongue hull (TonguePier.Hull, a ruled skirt written with u = x, not FA's writer: |grad u| 0.61-0.90).
  2. V2 w forced 0 / 1: 5.34 % on byte 17 (S4). Cause: sampling noise, not bias. One tower patch holds ~4 800 windows, ~900 lit: the
     Bernoulli lit state alone gives sqrt(0.81 / 912) = 3.0 % and b (CV 0.47) 1.6 %, so a 3 % bound on one realisation is a coin toss.
- Fixes (FacadeSkinTests, NocFacadeMath):
  - contract: roof triangles skipped for the metric and sigma checks; the tongue hull tracked as open defect FA-D01 (QA/critique-FA.md),
    the other writers stay strict; separate bad lists for grad and sigma; CityMesher towers (their cell meshes are unreadable in the
    built world) now checked through MeshBuilder in SkinByte_*: 200 towers, 2 672 triangles, |grad u| 1.0000, sigma +1 everywhere.
  - twin: `FaceIn.expectState` (tests only) gives every resolved cell its expected state (lit = p, hue x b and stand-by at their
    means); the C# twin now carries the HLSL interior factor (ceiling falloff, curtains with tint, blinds via the new
    `NocFacadeMath.FilteredGrid1` twin of NocFilteredGrid1, I / E[I] faded in over 4-8 px of window height) - it was missing.
  - regime test: expected states at 4 px columns (tight) + an interior case at 0.15 m px (wI = 1); the sampled states over 6 towers
    x 4 faces logged as information; E[b] checked (1.0941 vs 1.0954).
- 21:58 FA-t02 (FacadeSkinTests|FacadeTests): 10/10. Expected regimes worst 0.41 % (4 px columns), interior 1.62 % (the curtain
  closure is measured over the opening, the glass is inside the frame: a small negative bias, inside 3 %); sampled states 0.19 %;
  MeanRadiance D 0.01139 (LOOK_V2 0.0114), OQ 0.01021 = 1.9 sRGB codes darker (lead decision 6 asks ~4: FA-4 calibration item).
- 22:05 facade sheet (TEXTURE_PLAN 7.2): new `Scripts/Runtime/Rendering/NocFacadeSheet.cs` (IBeatBuilder, -facade-sheet): S0..S4
  (region D), overrides 0xF0..0xF4 and the section mode as 30 x 30 x 200 m towers (y 20..220) in one row at z 5 600 across the north
  Gulf (faces A / B at full flare, no bridge north of z 2 300), each turned 0 / 45 / 75 deg, pitch 54 m; `Poses()` = 27 stills
  (fs3000, fs1000, fs300a-c, fs100_<subject>, fs30_<subject>), looking north from y 120. New `Tools/qa/facade_sheet.py`: exact
  pinhole crops of every visible face, floor-line autocorrelation at the floor pitch (gate > 0.3 where >= 4 px), window / pier
  contrast, micro / meso texture (lookv2_check.texture) before and after an x264 CRF 23 4:2:0 round trip (gate micro >= 1.5 at
  75-300 m), mean luma per skin at 1 / 3 km, the reveal sign from debug view 6 (R - 128 against d . T from the geometry).
REQUEST TQ: Scripts/Runtime/QA/ShotPlan.cs: (1) a capture set "facadesheet" = every line of `NocFacadeSheet.Poses()` through
  `ParsePose` (one player run instead of 27; run with -facade-sheet); (2) an optional 9th pose field s (`pose:NAME:x:y:z:yaw:pitch:fov:s`)
  that replaces `NocMath.ProjectToPath` for parameters, fog, vFOV, w_f / n_f and `AmbientForS`, so the sheet can be shot under beat 2
  (s 300) far from the boulevard; `NocFacadeSheet.Poses()` will append ":300" once (2) exists.
- 22:21 lookdev_FA build (BuildWindows SUCCESS, 0 problems). Captures (1080p, -freeze-clock):
  - FA_01v2 (keyshots1080, -facade-v 2) and FA_01v1 (-facade-v 1). Looked at the contact sheets and at KS04, KS05, KS09, KS17 full size
    plus 2x crops (KS04 left / right, KS05 far face, KS09 face B at grazing). v2: towers read as buildings - punched windows in
    recesses with the visible jamb on the far side, centre mullions, curtains, lit floor groups, dark unlit glass, no confetti grid;
    v1 is the old cyan / magenta confetti (unchanged path: the v1 shader reads neither colour G nor uv1.zw, derivatives were only
    moved before the uniform branch). image_metrics: NaN-like / isolated pixels in both (v1 more: 13 stills, v2 7) - rain and
    sparks, not facade pixels; mean-luma FAILs KS15 / KS16 in both (not FA).
  - Open from the stills (QA/critique-FA.md): flat walls (no photo skin until TP-1), section-mode faces without atria / mechanical
    bands (FA-3), Heart ring towers teal from the koi light on real albedo walls (PA / KO), bright shopfront panels at the
    boulevard podium base (G1Materials EmitShop*, SU / PA lines), grazing rows of face B smear to grey lit-floor bands.
  - Facade sheet: FA_s01 (fs100_S1), FA_s02 (fs30_S1), FA_s01d6 (fs100_S1 at -debug-view 6), 26 s per run. facade_sheet.py
    (FA_s01 report): reveal sign image = geometry on all 10 faces of the 100 m pose (sigma right on +1 and -1 faces); floor-line
    autocorrelation per band 0.68-0.97 on residential / mechanical bands (2 information fails on bands with < 6 pitches in view);
    micro texture after x264 0.39-1.30 (< 1.5: no photo skin yet, TP-1).
- FA-1 complete: writers + skin byte, regions and skins, LOOK_V2 window light keyed per tower, whole bays, macro layer (tower
  multiplier, lit floor groups, core strip, mechanical stripes, setback ledge line), recess rectangles with spill and wet sills,
  city term on f.specOcc, MeanRadiance (D 0.01139), the facade sheet. Cell API for GE: NocFacadeMath.CellOf / Bays / Rows /
  StratumV2 / SkinOf / SkinByte / SkinOverride / Region (FacadeSkinTests green, FA-t02).
FA-1 READY 2026-11-15T22:58+01:00
- 23:13 FA-t03 (FacadeSkinTests|FacadeTests|CityTests): 16/17. New failure from GE's new relief meshes: CanyonBridge_facade face
  0x6101 (the east face) has mixed sigma (16 + / 36 -): `ReliefKit.PanelWithHole` writes u = uc + dz while the face's u is 600 - z.
  Exempted in the test as open FA-D13 (listed in OpenSigmaFaces), strict for every other writer.
REQUEST GE: Beats/Relief/ReliefKit.cs PanelWithHole: add a parameter `float uSign` and write u as `uc + uSign * EdgeDims.PortalOuter * Math.Cos(a)` (ring) and `uc + uSign * h * cz[c, 0]` (corners); CanyonBridge.cs passes uSign -1 for the east panel (its face uses u = 600 - z), so sigma is +1 over all of face 0x6101 (FacadeSkinTests contract; the reveals around the tube portal otherwise light from the wrong side).
- 23:00-23:20 FA-2 / FA-3 work (in the tree, behind -facade-v 2; verified with the next lookdev build):
  - FA-2 glass: `NocSkylineV2` (NocFacadeSkin.hlsl) - the reflected city: 4 deg azimuth bins cut into 1-3 tower slots, a 34-64 m
    tower filling its slot (d = W / slot angle), body = the unresolved facade mean (lit glass 0.332 of the area at the LOOK_V2 hue
    mix) with window speckle on the city's own 2.4 x 3.6 m grid (hashed phase per tower, H2 fade in blur footprints), fogged by d
    (height factor averaged along the ray), faded into the deck over NC_CloudMixY0..1; plus SU's `NocWallReflect` (NocMirror.hlsl,
    read-only) for the registered canyon / Gulf walls in front of it. The skyline replaces the D-017 city term on the glass only.
    New constants FaSky* (bin 4 deg, W 34-64 m, glass 0.332, window rectangle 0.5-1.9 x 1.0-3.05 m).
  - Curtain closure measured over the glass inside the frame (FA-D09; HLSL + twin).
  - FA-3 section macro (mode 1, CRIT0 #6): districts 252 x 300 m (28 bays x 75 floors) with a log-normal lit-density multiplier
    (sigma 0.6, box-filtered for the band means, exact per cell), a 12 m mechanical band at each district's bottom (louvre wall,
    3 slits lit on 60 % of floors, amber x k_a or SignWhite at 0.6 Y_med, drawn >= 1 px with energy kept), one glazed atrium
    27 x 48 m per district (lit on 85 %, 1.4 Y_med, warm to neutral with w_f, mullions every 4.5 m and every floor). Twin
    `NocFacadeMath.SecMacro` / `SecDistMul`; `MeanRadiance(1, ...)` includes districts, bands and atria (SU's wet macro reads it).
  - Louvre walls (residential mechanical stripes and section bands): MetalPaint x 2.0 under filtered blades (0.15 m pitch, mean
    factor 0.75), AO 0.85 (FA-D06: were a black band).
  - FA-3 box rooms: new `Shaders/Include/NocInterior.hlsl` + twin `NocInteriorMath.cs`: one room per window cell (x 0..P.x,
    y 0..P.y - 0.3, depth 6-10 m residential, 10-14 commercial / lobby, 8-12 section), exit through back / side / ceiling / floor
    (van Dongen slab form); each surface linear in its exit depth (back 1.0 (0.85 + 0.3 v), side 0.6 (1.3 - 0.6 w), ceiling
    0.9 (1.25 - 0.5 w), floor 0.4 (1.2 - 0.4 w)), so the shares and mean exit depths over the glass are closed-form integrals:
    `NocRoomMeanV2` is exact (prototype: worst 0.99 % against a 48 x 48 integral, 0-60 deg, every azimuth, 5 room types). The room
    replaces the ceiling falloff by raw / mean x E[falloff], faded in by smoothstep(5, 9) of window height (not behind louvres).
    New `InteriorTests` (closed form vs a 64 x 64 integral within 3 % up to 60 deg; surfaces read as a room; no NaN at grazing).
- Hero overrides (TEXTURE_PLAN 2.2 / 8.4: the beat-file owners carry these lines for FA; the shader and twin already decode 0xF0 + k):
REQUEST GE: Beats/Beat07_Heart/SpireBuilder.cs:35: the seed argument `0xA7u` of G4Kit.FacadeQuad -> `NocFacadeMath.SkinOverride(NocFacadeMath.OverrideSpire)` (S0 dark glass with fins; one byte on every spire face).
REQUEST PA: Beats/Beat06_Signs/ScreenWallBuilder.cs:38 (MassFaces): the seed argument of G4Kit.FacadeBox -> `NocFacadeMath.SkinOverride(NocFacadeMath.OverrideScreenWall)` (S2 metal panels with LED-strip housings, CRIT0 #22).
REQUEST SU: Beats/Beat01_Alley/AlleyBlocks.cs:19: `WestSeed = 0x41A1u, EastSeed = 0x41A2u` -> `WestSeed = 0xFFFFFFF2u /* NocFacadeMath.SkinOverride(OverrideAlleyWest): S1 precast */, EastSeed = 0xFFFFFFF3u /* SkinOverride(OverrideAlleyEast): S4 tiled with loggias */` (const uint; the window hashes are keyed by face id, so the window field moves but stays deterministic).
- 23:30-23:50 (FA_02v2 keyshots looked at: contact + KS03, KS07 full size, KS07 atria 3x crop, KS05 top-left crop):
  - Section faces: atria read as flat beige posters, slits as continuous laser lines, 9 m bays as 8 m light blocks. Changes:
    section lit units of 4.5 m (half bays, one centre mullion; FaSecP 4.5, district 252 = 56 units), atrium halls with a lit
    balcony edge every floor and brighter toward the hall floor (gain 1.0 Y_med, lit 70 %, mean profile 0.61), slits cut into
    9 m bay segments each lit by its own hash (1 m gaps at the columns).
  - S2 ribbed panels (zero-mean albedo ribs 0.06 / 0.3 m), S3 stone sills and lintels (Granite, 0.12 m, 0.1 m wider than the
    window, blended to their mean area below the window fade).
  - S4 loggias (TEXTURE_PLAN 2.3): 40 % of an S4 tower's residential bands: opening 2.2 x (3.6 - slab) recessed 1.4 m, sliding
    doors 2.0 x 2.2 with a centre mullion at the back (the glass rectangle is now separate from the opening: Cell gx0..gy1),
    top rail 0.05 m at 1.05 m and balusters 0.03 / 0.12 m in front (surface hook, exact coverage, mean below the fade); corner
    bays and the floor under the parapet stay solid in those bands; BandMeanRadiance counts the doors.
  - NocFacade.shader: forward ZWrite [_FacadeZWrite] / ZTest [_FacadeZTest] (defaults On / LEqual = today) for TP's facade prepass
    (TEXTURE_PLAN 2.12: the Facade row sets Off / Equal once NocFacadePrepassFeature lands).
  - NocFacadeSheet.Poses() now carries s 300 (TQ's ninth pose field, beat 2); the whole sheet is one run: -capture-set facadesheet.
- 23:55 FA_03v2 (keyshots1080) and FA_s03 (the whole sheet in one run: -capture-set facadesheet, 27 stills, s 300) looked at
  (contacts, KS07 atria crop, fs30_S2, fs100_SEC full size). Atria now read as stacked lit halls, slits as bay segments. Sheet
  (facade_sheet.py FA_s03): skin means at 1 000 m 29-37 display luma for the regional skins (LOOK_V2 1 km 36.3), section 47.5 (too
  bright: its glass covers 0.62 of the face, 1.9 x residential) -> FaSecLit 0.21 -> 0.13 and district sigma 0.6 -> 0.8 (dark voids,
  lit cores); lit glass median display luma 94-111 at 30 m (LOOK_V2: 100), wall 21-25 (LOOK_V2 shade side 22.3).
  - image_metrics "NaN-like" flags in KS04-KS07 traced (crops): edge-light / blade-sign line edges (not FA) and 1-2 px lit
    window slivers behind the recess at oblique views (FA) -> visible-glass sliver fade (FaSliverPx 1-2 px, the axis falls to its
    view-dependent mean, E4 intact).
  - Mechanical strata: louvre blades now fill the slits (FA-D07).
- 00:00 FA-t04 (FacadeSkinTests|FacadeTests|InteriorTests|CityTests): 19/19. Expected regimes worst 0.49 % (4 px columns),
  interior incl. box rooms and loggias 0.98 %; room mean vs 64 x 64 integral worst 0.93 % (0-60 deg); MeanRadiance D 0.01165
  (+2.2 % with the loggia doors), OQ 0.01042, section 0.01290.
- PA-1 READY (36d4b03) seen 00:03. FA-4 calibration: region OQ lit-share factor FaOqLit 0.78 (lead decision 6: about 4 codes darker;
  the skin shares alone gave 1.9 sRGB codes; the factor scales the lit probability, never the window radiance).
- Override lines carried by their owners: GE SpireBuilder (S0 fins), PA ScreenWallBuilder (S2 LED), SU AlleyBlocks (S1 / S4).
  DONE seen in PA.md:132, SU.md:120, SpireBuilder.cs:42. Shader details for them: spire fins (0.08 m anodised blades 0.5 m deep at
  every mullion line, widened by their side at oblique views), screen-wall LED strips (one 6 cm strip per floor in a 0.3 m housing,
  70 % lit, SignWhite / WinNeutral / WinCold at 0.5 Y_med, drawn >= 1 px with energy kept).
REQUEST SU: Shaders/Include/NocMirror.hlsl: please commit NocWallReflect with your next green batch: FA's NocFacade.shader includes NocMirror.hlsl read-only and calls NocWallReflect(P, r, softRad, dEye, sky) for the canyon / Gulf walls in the glass (TEXTURE_PLAN 2.7); FA commits its shader with this dependency.
- 00:12 lookdev_FA build SUCCESS (shader check of all 15 Megastructure shaders). Captures: FA_04v2 (keyshots1080, -facade-v 2:
  section faces now dark with sparse lit units, atria halls, spire fins) and FA_04def (default = v1: no black / white / magenta
  frame; NaN-like flags 78, v2 24 - rain, sparks, line edges).
- 00:30 FA-t05 (FacadeSkinTests|FacadeTests|InteriorTests|CityTests|MaterialTests): 31/31. MeanRadiance D 0.01165, OQ 0.00869
  (4.8 sRGB codes darker: lead decision 6), section 0.01290.
- 00:33 texpans with -facade-v 2 (FA_p01, 7 x 30 frames, -freeze-clock -rain 0): sequences.py PASS, 0 FAIL; swimming p95 0.13-0.32 px
  (WARN KS04 max 1.22 px at block (1472, 192), WARN KS09); stabilised residual p99 2.4-2.8 levels (INFO: needs the -capture-ss 2
  reference for the shimmer verdict).
- 00:40 debug views 1, 3, 4, 5, 6, 10 on keyshots1080 (FA_dv*; debugview.py): PASS except view 4 KS10 (0.1 % of pixels carry both
  signs): located (crop of FA_dv4 KS10 x 739-1344, y 83-260): GE's relief pieces on faces A / B carry the opposite u direction of
  the face behind them (each piece consistent, FacadeSkinTests contract), so MSAA edges between them blend red and green - a
  decoder edge case, not a frame-sign error. View 1 KS05: S1 43 %, S2 14 %, S4 11 %, section 32 %; view 10 mean p 0.20 (KS05),
  0.10 (KS07 section faces), 0.18 (KS17); view 5 KS05 cell period median 9.9 px, KS07 5.4 px.
- TQ-09 (TQ's REQUEST FA, dolly seq_dolly_sheet30): reproduced (FA_d01, sequences.py: one local pop, frame 238, 6.5 levels at
  50.7 m). Crops of frames 236-239: whole pixel columns of lit S0 windows go dark in alternate frames. Cause: S0's vision glass
  reaches 3.54 m of the 3.6 m floor but the box room's ceiling was P.y - 0.3 = 3.3 m, so glass points above the ceiling gave
  negative exit depths; the sign of ky flips frame to frame at the eye height. Fix: the room ceiling is max(P.y - 0.3, glass head
  + 0.02) and the glass-plane point is clamped into the aperture (the off-centre cell of a 2 x 2 footprint) - HLSL and twin.
DONE FA TQ REQUEST seq_dolly_sheet30 pop (TQ-09): S0 box room ceiling below the vision glass; fixed (verification in FA_d02).
DONE FA TP REQUEST Tools/texprep/facade.json: RoomArr slice 4 tile 1.7 and slice 6 tile 3.85 (the TEXTURES.md tiles; textured rooms
  are cut-ladder item 1 and not sampled yet); the file's comment now says it follows texprep VERSION 4.
- 01:10 TQ-09 not fixed by the room change (FA_d02: the same pop, frame 238). Shot frame 239's pose as stills (FA_x0 beauty, FA_x2 /
  x3 / x6 debug views 2 / 3 / 6): along row 570 the dark columns of lit windows coincide pixel for pixel with debug view 3's
  whole-bay mask (B) flipping 255 / 0 inside one cell, while the cell coordinate (R) runs smoothly. Real cause: the sheet towers
  are exactly 30 m wide, the core strip needs uMax - uMin > 30 (FaCoreMinFace), and the per-pixel interpolation noise of the
  per-face constant uMin / uMax (about 1 ulp) flips that test (and could flip any whole-bay / row ceil / floor near an integer).
  Fix: the v2 shader snaps uMin, uMax, y0, y1 to 1/16 m (FaExtentQ); the writers keep u extents off the snap's half-steps
  (NocFacadeMath.SafeExtent: a <= 1 mm nudge on about 3 % of faces, the v1 reading of uv1.xy moves by <= 1 mm there - far below a
  pixel) and write y0 / y1 on the grid, rounded outward (v2 only); the twin snaps the same way (EvalV2 -> EvalV2Snapped).
  The room ceiling / aperture clamp of 00:45 stays (a real defect for S0, found on the way).
REQUEST GE: Beats/Beat03_Edge/EdgeMesh.cs FacadeWriter.Quad and Beats/Relief/ReliefKit.cs (HoledFaceX, PanelWithHole): write uMin / uMax through `NocFacadeMath.SafeExtent(...)` (a <= 1 mm nudge off the 1/16 m snap half-steps), so the v2 whole-bay / district decisions on faces A / B and the relief never flicker with interpolation noise (TQ-09; FA's writers do it since 01:10).
- 01:36 lookdev_FA build SUCCESS; FA-t06 (FacadeSkinTests|FacadeTests|InteriorTests|CityTests|MaterialTests) 33/33, new
  V2_BoxRooms_StableAcrossEyeLevel (S0 / S4 / S1 / S3: ky -1e-3 / 0 / +1e-3 means equal to 4 digits).
- 01:50 FA_d03 dolly on the sheet (-facade-v 2): sequences.py PASS, local pops 0 (was 1, 6.5 levels at frame 238), luma step max
  0.218 levels (was 0.305). Looked at frames 60 / 180 / 260 / 299: rooms behind the lit windows at 25-50 m, face A's atrium hall.
DONE FA TQ REQUEST TQ-09 (dolly pop): fixed and verified (FA_d03: 0 local pops).
- 02:00 texpans against the -capture-ss 2 reference (FA_p02 vs FA_p03, both -facade-v 2, build 01:36): shimmer FAIL at KS00
  (1120, 128) 2.78 (a lit sign panel: G1 / PA), KS04 (384, 0) 5.53 and KS05 (288, 480) 7.70 (the slabs' cyan / white edge lights:
  FA), KS17 residual (832, 224) 2.88 (the spire's cyan belt and strips: GE / PA). Fix for FA's part: v2 edge lights drawn >= 3 px
  (was 2) with a 1.5 px box (FaEdgeMinPx, FaEdgeBlurPx; energy kept). Atrium halls toned down (gain 0.8, base 0.25, balcony edge 1.6).
- TP-1 READY (02:00) seen 02:05; TP's NOTE: the facade prepass is on by default with bit-identical depth -> the Facade row keeps
  ZWrite On / LEqual (early-Z does the work, and -facade-prepass 0 keeps working).
- 02:10 FA-4 photo skins: NocFacade samples FacadeArr (albedo / normal / mask arrays, one sampler): the tower's slice (S1 precast 0
  or weathered 1 on 15 %, S2 metal 2 / ribbed 3, S3 brick 5, S4 tile 4, S0 caps 2, section 1, roofs 0), cell-aligned panel shuffle
  with offsets in whole registered courses / lifts / ribs / modules (seams between whole courses), SampleGrad with the true
  gradients of (u, v) / tile, every map faded to its last mip (NocTexFade). Albedo x 2 x variation on wall, slab and reveals;
  smoothness = slice dry + 0.15 wet lift; AO x lerp(1, AO, 0.5); the normal as a surface gradient along the facade frame on the
  wall share. MaterialFactory.Facade wires the arrays and the slice tiles from FacadeArr.texmeta (_FaTexOn 1 only when they exist).
- 02:25 FA-rebuild01 (RebuildAll SUCCESS: Facade.mat carries FacadeArr, _FaTexOn 1, the slice tiles 3 / 1.543 / 2 / 2.8 / 1.95 /
  1.846 / 3.2 / 2.4) and lookdev_FA build SUCCESS (17 shaders checked). Captures FA_05v2 (keyshots1080) and FA_s05 (facadesheet).
  Looked at: contact, KS04 left 2x crop, fs30_O2 / S3 / S4 / O4 wall-range stretched (luma 10-45 -> 0-255): precast panels with
  grain and per-panel tone, brick courses with stone sills and lintels, tile, podium precast; at display level the skins are
  +-2-3 codes on a 25-code wall, as LOOK_V2 5.3 asks for ambient-only walls. facade_sheet.py FA_s05: texture micro at 75-300 m
  median 2.27 codes, after x264 CRF 23 median 1.54 (gate 1.5; FA_s01 untextured 0.39-1.30), meso 6.4; skin means at 1 km 28-38,
  section 42.8 (was 47.5). Floor autocorrelation at 1 km fails where floors are 4-5 px: there the H2 fade (2.5-5.5 px per cell)
  is already blending the rows to the band mean - the gate's 4 px and the fade disagree (open, FA-D18).
- 02:45 FA-t07 (FacadeSkinTests|FacadeTests|InteriorTests|CityTests|MaterialTests|TextureTests): 44/44.
- 03:05 sill streaks (TEXTURE_PLAN 2.5, LOOK_V2 4 -20 % albedo): TP's OverlayArr slice 0 leak decal under every opening of a
  resolved residential / commercial cell (1.1-1.76 m from the sill down, column offset by the cell hash, across the window width),
  albedo x (1 - 0.35 coverage), smoothness + 0.25 coverage, faded to its mean profile below 2-4 px of the 2 m tile, off below the
  window fade (mean darkening at distance < 0.5 %). MaterialFactory.Facade binds OverlayArr_Mask (_FaStreakOn). Samplers: 2.
  FA-rebuild02 + lookdev_FA build SUCCESS; FA_s06 (facadesheet) looked at (fs30_O2 / S3, fs100_S1 wall-range stretched): faint
  streaks under the sills, precast / brick skins intact. The texpans run of 02:50 (FA_p04) was dropped: its -capture-ss 2
  reference would have come from the new build; texpans re-run as FA_p06 / FA_p07 on one build.
- ~03:10 FA-tall01 (full EditMode suite, tree of 03:15): 360/361. The one failure is not FA's: Beat02BoulevardTests.Blades
  ("magenta, cyan, amber, violet in x order" - PA's LOOK_V2 blade palette; PA's REQUEST INT at 22:03).
- ~03:15 texpans FA_p06 vs the -capture-ss 2 reference FA_p07 (one build): KS04 5.76 at (224, 416) and KS05 7.91 at (0, 736) - the
  bright edge band sits on the tower corner, a geometric silhouette resolved by MSAA's 4 samples; KS17 2.42 at (96, 544): lit
  ring-tower windows at 2.5-5.5 px cells (the H2 fade band); KS00 (G1 sign) and KS08 residual (lamp pole) not FA's. Fix: the v2
  edge band set in 1.25 px from the corner (FaEdgeInsetPx), so the silhouette pixel keeps the wall. Rebuilt 03:55; texpans re-run
  as FA_p08 / FA_p09.
- ~03:30 texpans FA_p08 vs FA_p09 (edge inset build): world-locked residual PASS at KS04 / KS05 / KS09 / KS15 (1.48 / 1.29 / 1.85 /
  1.16 <= 2); screen-space temporal std over the reference FAIL at KS04 (256, 320) 7.21 and KS05 (0, 768) 6.71 - debug view 1 of
  KS05 (FA_dvk) shows those bright pixels are facade (S1 code 40): the edge band's own motion against the reference, whose 3 px
  minimum renders as 1.5 px at 1080 (FA-D19, partly); KS17 2.42 (ring windows in the H2 fade band, FA-D20), KS00 / KS08 / KS17
  residual not FA's. Swimming p95 0.08-0.37 px (PASS).
- ~03:35 keyshots (1080 + 4K + 16:10 + 21:9) FA_06k looked at (KS04 4K left crop: reveals, mullions, rooms, same as 1080); NaN-like
  flags as before (rain, sparks, line edges); deleted after the look (disk W: 15-17 GB free, other agents' folders included).
- ~03:38 GE carried PanelWithHole uSign (GE.md 23:35) and SafeExtent in EdgeMesh / GulfFace / ReliefKit (GE.md 01:30); SU committed
  NocWallReflect (SU.md 202). OpenSigmaFaces emptied: FA-t08 (FacadeSkinTests|InteriorTests) 9/9, contract strict.
- FA-5 not done: -facade-v stays 1 by default. The default lives in TP's NocLookGlobals (DefaultFacadeV) and the switch changes
  every package's picture; open acceptance items: FA-D18 (floor lines at 1 km vs the H2 fade), FA-D19 / FA-D20 (pan shimmer
  measure), the twin crawl sweeps (TEXTURE_PLAN 2.9), statics. Proposed for INT / the lead in the final report.
- Final folders: QA/captures/FA_05v2 (keyshots1080, -facade-v 2, textured, build 02:25), QA/captures/FA_s06 (facadesheet, build
  03:10 with streaks); reports QA/reports/facade_sheet_FA_s05.md, sequences_FA_p08.md, FA-tall01.xml, FA-t08.xml.
FA DONE 2026-11-16T03:41:41+01:00
