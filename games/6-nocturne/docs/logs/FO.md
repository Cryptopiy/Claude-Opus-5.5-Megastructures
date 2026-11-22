# FO log - foliage (Look v2, TEXTURE_PLAN 8.3 FO and section 4; CRIT0 #10, #25)

Owner FO. Authority: D-027 / D-028 / D-029 and the lead decisions of the Look v2 build workflow.
Requests to FO are answered here as "DONE FO <request>". Build slot `Builds/lookdev_FO` (`Tools/lookdev.ps1 -Slot FO`).
Critique: `QA/critique-FO.md`.

## Resume rule
Find the last step marked GREEN below and the last commit with "FO:" in the message; continue with the next open item.

## Ownership (exclusive; TEXTURE_PLAN 8.3 FO / 8.4)
- `Shaders/Beats/NocFoliage.shader`.
- `Beats/Beat05_Garden/GardenTree.cs` (core shell, card hand-off, bark UV0 wrap), `GardenLeafCards.cs` (new), `GardenMaterials.cs`
  (library foliage and `lib.FloorMoss`; no runtime `Shader.Find`), `GardenDims.cs`.
- `Scripts/Editor/MaterialFactory.Foliage.cs` (`Leaves`, `LeafCore`) and `Scripts/Runtime/Core/MaterialLibrary.Foliage.cs` (created by
  TP-0 and handed over).
- `Tools/texprep/foliage.json`.
- Tests: `Scripts/Tests/EditMode/Beats/Beat05GardenTests.cs`, `FoliageTests.cs` (new), `MaterialTests.Foliage.cs` (TP-0 split).
- `docs/logs/FO.md`, `QA/critique-FO.md`.
Not mine: `Beat05_Garden.cs` (SU; PA carries the light colour at 36), `GardenTerrace.cs`, `GardenFurniture.cs` (no package),
`BridgeGardenKit.cs` (GE), the Bark / Moss / FloorMoss rows and `surface.json` (SU), `VisibilityWorld` / `VisibilityAuditTests` (GE).

## Steps
- 09:36 start. Read TEXTURE_PLAN 0, 1.1-1.7, 3.1-3.4, 4, 7, 8.1-8.4, 9, 10, 11; LOOK_V2 1, 2, 3, 5, 6, 8, 9 (beat 5), 10 (foliage);
  TEXTURES.md (H sets, TEX-R final table); CRIT0 (#10, #25, KS14 / KS15 rows); critique-graybox01 (GB-02, GB-03, GB-06);
  BUILDER_BRIEF; STATE Commands; DEVELOPMENT D-001..D-029; G3 log. Code: GardenDims, GardenTree, GardenMaterials, Beat05_Garden,
  GardenFurniture, BridgeGardenKit, NocFoliage, NocLitPass / NocLitCore / NocCommon / NocLighting / NocAtmosphere, MaterialFactory,
  MeshBuilder, MeshKit.Loft, VisibilityWorld / VisibilityRaycaster, Beat05GardenTests. Raw LeafSet024 colour and opacity viewed
  (beech, 3 x 3 leaves of about 0.13 m on a 0.40 m atlas; three leaves of the bottom row are chewed, good for variety).
- Markers seen: PA TOKENS READY 09:38 (PAL_Leaf available). Waiting for TP-0 READY and TP-1 READY.

## Resume after the pause (2026-11-14 12:30)
- Read FO log, STATE PAUSED, TEXTURE_PLAN 0, 1.1-1.7, 4, 7, 8.1-8.4; LOOK_V2 2.4, 5.1-5.4, 9 (beat 5), 10; CRIT0 #10 / #25; GB-02/03/06.
- WIP re-applied by the lead (uncommitted): GardenDims (v2 constants, FoliageV2 flag), GardenTree (v2 curved limbs and sub-limbs,
  bark UV0 wrap for trunk / limbs / v1 branches, ring start away from the walk axis, core at 0.90), Tools/texprep/foliage.json draft.
- 12:33 fastcheck failed only in PA's NocSchedule (PA mid-edit). Markers: PA TOKENS READY only; TP is in TP-0 step 3.

## Step 1 - v2 geometry and shader drafted (12:35-13:00; behind -foliage-v 2, v1 unchanged)
- `GardenLeafCards.cs` (new): cards on two shells (outer 1.02 first, inner 0.94; x the core lumps, +-1.5 % radial jitter),
  Fibonacci lattice jittered 0.45 spacing, thinned by the ellipsoid area element and a clump noise (density x (1 +- 0.45));
  n0 = target / (E_area x E_clump), target = 1.5 x Thomsen area / 4.4 m^2 x (1 hero, 0.5 back). Card 1.6-2.6 m, tilt <= 50 deg,
  random in-plane rotation; corners inside 1.10 x ellipsoid and 1 m inside the notch walls (x > 814.3), else shrunk to 1.2 m,
  else dropped. Vertex contract: normal = ellipsoid normal at the corner (bent target; within 50 deg of the card normal, so
  MeshValidation's winding check holds), tangent = card u, uv0 sprite, uv1 = (slice, hash, oct card normal), colour R AO
  (inner 0.72, underside 0.85), G glow card (2.5 %), B sky exposure. API: Envelope(tree), CoreRadii, NormRadius, TargetCount.
  (Design note: the card normal is constant per card, so it goes in uv1.zw as octahedral; the radial normal varies per corner
  and would cross the octahedral fold near -z, so it goes in NORMAL.)
- `GardenTree.cs`: v2 builds the core into the crown builder and the cards into one pending MeshBuilder per tree;
  `EmitCrowns(ctx, group, crowns, m)` emits v1 exactly as before, or the cores ("crowns", LeafCore) + "crown_cards_<T>"
  (Leaves, tangents set, 32-bit indices) and logs the counts.
- `GardenMaterials.cs`: v2 LeafCore (_LeafMode 1, queue 2000) and Leaves (_LeafMode 2, AlphaToMask, Cull Off, queue 2450,
  LOOK_V2 10 gains 0.012 / 0.015 / 0.03), derived from the foliage material until the library rows exist (after TP-0's split);
  v2 moss ground _PuddleFraction 0 (no puddle blots on the lawn).
- `NocFoliage.shader`: _LeafMode 0 = the v1 code verbatim; 1 = core (Leaf x _CoreAo x low-contrast leaf noise); 2 = cards
  (atlas Texture2DArray albedo / normal / mask with one sampler, Golus alpha sharpening faded by mip, bent normal 0.7 + atlas
  surface gradient x faceSign, hue +-8 % / value +-6 %, wet sheen, transmission in one light loop with sky exposure, specks /
  rim / tips). Derivatives and the albedo tap before every branch; AlphaToMask [_AlphaToMask]; NOC_ALPHA_OUT, NOC_FRONT_FACE,
  NOC_PASS_UV1ZW, NOC_HAS_FORWARD_HOOK (v1 / core return NocShadeOcc exactly); clip hook in the depth-type passes only.
  No keyword added. fastcheck runtime OK (editor: TP's NocTexturingTests mid-edit).

REQUEST SU: Assets/_Game/Beats/Beat05_Garden/Beat05_Garden.cs: in BuildTrees replace the line `BridgeGardenKit.Emit(ctx, g, "crowns", crowns, m.Foliage);` with `GardenTree.EmitCrowns(ctx, g, crowns, m);` (FO's new entry point in GardenTree.cs; with -foliage-v 1 it makes exactly the old call, so no picture changes; with -foliage-v 2 it emits the cores and the per-tree leaf-card meshes).

## Resume, third start (2026-11-15 21:26; run wf_2c6d782f-763, D-030)
- Read FO log, STATE (RESUMED THIRD START, PAUSED AGAIN), TEXTURE_PLAN 1.1-1.5, 4, 8.3 TP-0 / TP-1 / FO, 8.4; LOOK_V2 5.1, 8.1 (KS14 /
  KS15), 10; GB-02 / GB-03 / GB-06; TP log (TP-0 steps 1-5 in the tree, texprep.py not written yet, TP-1 not started); GE log (no
  REQUEST FO yet); SU log. `git diff` of my files: the step-1 work above is in the tree as I left it.
- DONE FO SU REQUEST FoliageTests.cs:33 `NocPaths`: the file has `using Megastructure.EditorTools;` (line 17); FO-t01 and FO-t02 compile
  the EditMode assembly in Unity.
- My REQUEST SU (Beat05_Garden.cs BuildTrees -> `GardenTree.EmitCrowns(ctx, g, crowns, m)`) is carried in SU's working tree (line 53).
- 21:37 fastcheck runtime 162 / editor 215 OK. 21:40 FO-t02 (FoliageTests|Beat05GardenTests, real Unity compile of every assembly):
  17 / 17 passed. v2 log line: 17 card meshes, 115 632 cards (42 shrunk, 33 dropped), 231 264 card triangles (budget 135 k / 270 k).
- GREEN step 1. Commit plan (D-030 item 2): the runtime C# (GardenDims, GardenTree, GardenLeafCards + meta, GardenMaterials) and
  foliage.json now: they compile against HEAD and change no default picture (EmitCrowns is v1-exact; the bark UV0 wrap is ignored by
  v1's triplanar NocSurface). NocFoliage.shader and FoliageTests wait for TP's lit-template commit (they use NOC_PASS_UV1ZW,
  NOC_ALPHA_OUT, NOC_FRONT_FACE, f.specOcc, NocShadeOcc, which HEAD does not have), so HEAD never holds a shader that does not compile.
- 21:45 Commit 1 (FO: geometry, GardenMaterials, foliage.json). Waiting for TP-0 READY / TP-1 READY (texprep.py does not exist yet).
  To keep the leaf atlases off TP's critical path, FO writes the leaf-cluster composition of texprep step 7 itself as a pure
  library module `Tools/texprep/foliage_compose.py` (FO, new; numpy + PIL only, deterministic, no file IO: it takes the decoded
  float32 linear source maps and returns the flipbook maps, the alpha coverage and the stats). Ratification of the new file is
  proposed in the final report.
REQUEST TP: Tools/texprep.py step 7 (leaves): for a foliage.json set, after decoding the source maps to float32 linear (albedo linear RGB, normal GL [-1, 1], roughness, height, opacity, scattering when present), call `foliage_compose.compose(cfg_set, maps)` from Tools/texprep/foliage_compose.py (import it by path; it never touches raw files or the disk) and continue with the returned 2 x 2 flipbook (albedo RGBA with straight alpha, normal GL, AO, height, smoothness, scattering) through your steps 5, 6, 9 and 10 (contrast / chroma on alpha > 0.5 texels, mask pack, array flipbook, texmeta with alpha true, alphaRef 0.5, cols 2, rows 2, cls "leaves"); FO keeps the compose rules and the config. Also: could texprep run `--only LeafCrown` as soon as it can write a set (before the other sets), so FO can calibrate the crowns while TP-1 continues? Answer here or in TP.md.

## Step 2 - leaf-cluster composition, library rows, lookdev atlas (21:50-22:25)
- `Tools/texprep/foliage_compose.py` (FO, new): compose(cfg_set, maps) -> 2 x 2 flipbook of cluster sprites. Each sprite = 4 beech
  sprays (curved main twig 1.1-1.6 m, 4-6 alternate side twigs 0.3-0.6 m, leaves alternating at 4.5-7 cm internodes, 40-75 deg
  off the twig) plus fill leaves inside a lobed outline (radius 0.42 x (1 +- 0.08..0.14), 3-5 lobes, soft edge 0.10), so the card
  corners stay empty. Leaves at true size 0.123-0.167 m (source 0.127 m x 1.0-1.3), tilted out of the card plane up to 60 deg
  (foreshortened, normal tilted), 20 % undersides (paler, less saturated), per-leaf hue +-4 % / value +-10 %. Front-to-back
  premultiplied compositing at 2x supersampling (2048 px per sprite), AO = 1 - aoDepth x blurred leaf cover in front (3.5 cm
  penumbra, aoMin 0.55), height = layer position, 2x box downsample, push-pull dilation of colour / normal / mask. 14 s per atlas.
- foliage.json: leavesPerSprite 48-80 gave 8-12 % sprite coverage (sparse single sprays, looked thin); now 170 / 190 / 210 / 230
  leaves, 4 sprays: coverage 0.197 / 0.200 / 0.218 / 0.228 (about 40 % of the lobed cluster area; 3 card layers per crown point
  -> about half the rays reach the core or the sky); coverTarget 0.36-0.46 of the outline recorded. Looked at the atlas
  (scratch LeafCrown_prev_shaded.png full and a 512 px 1:1 crop, plus the normal crop): real beech sprays, venation reads at
  1:1, twigs 3-6 px, no fringes, chewed leaves give variety.
- Emulated texprep 5 / 6 (lookdev only): per-channel mean 0.5 over alpha > 0.5 (B 0.480 after the clamp), std / mean 0.394
  (class 0.40), saturation of Leaf x 2 x var 0.434 (<= 0.45), smoothness p5 / p50 / p95 0.30 / 0.40 / 0.48 (class 0.30-0.48).
- Lookdev atlas `Assets/_Game/Beats/Beat05_Garden/DevLeaf/LeafCrown_{Albedo,Normal,Mask}.png` + hand-written importer metas
  (Texture2DArray 2 x 2 flipbook, Box mips, albedo sRGB BC7 with mipMapsPreserveCoverage / ref 0.5, normal Default linear BC5,
  mask linear BC7). NEVER committed; outside TP's External scope so TP's TextureTests never see it; deleted when TP-1's set lands
  (MaterialFactory.LeafAtlasPrefix prefers the texmeta of Textures/External).
- `MaterialLibrary.Foliage.cs`: fields LeafCore, Leaves. `MaterialFactory.Foliage.cs`: rows LeafCore (_LeafMode 1, A2C 0, Cull 2,
  queue 2000, no emission) and Leaves (_LeafMode 2, A2C 1, Cull 0, queue 2450, gains 0.012 / 0.015 / 0.03, LeafCrown maps).
  `GardenMaterials`: v2 takes lib.LeafCore / lib.Leaves (no runtime Shader.Find), derived fallback only for an old library.
  `MaterialTests.Foliage.cs`: Foliage_CoreAndCardRows, Foliage_LeafAtlas_IsACoveragePreservingArray (ignored until an atlas exists).
- 22:10 FO-rebuild01 RebuildAll: SUCCESS, 0 problems, 30 / 30 materials; Leaves.mat binds the three arrays (fileID 18700000).
- 22:15 FO-t03 (FoliageTests|Beat05GardenTests|MaterialTests): 26 / 27; mine all pass (the atlas test sees a 4-slice coverage-
  preserving array); the one failure is SU's FloorAlley puddle fraction (0.409 vs 0.35, SU mid-calibration), not FO.
  (First attempt 22:12 did not compile: KO's KoiBuilder.cs:181 PectoralVMax mid-edit; retried after 4 min.)
- 22:30 NOTE SU: GardenTree.EmitCrowns is committed (721f0e1), so Beat05_Garden.cs (your carried call) compiles on HEAD now.
REQUEST GE: Assets/_Game/Beats/Beat04_Skybridge/SkybridgeDeck.cs: the glass-strip z lines (zs0 193.8 / zs1 196.2) now run the whole deck and reach its east end x 695, where the garden terrace has no twin vertices: Beat05GardenTests.FloorSeam_DeckEnd_SharesEveryVertex fails (FO-t04: "deck vertex (695.00, 0.00, 193.80) has no terrace twin (T-junction)"). Please keep the deck top's vertex set on the line x 695 unchanged (e.g. emit the strip lines only between the strip's neighbouring profile samples, or merge the band outside x 521.3-541.3), so the seam stays crack-free without touching GardenTerrace (no package).

## Step 3 - first lookdev (r01) and the lobed crowns (22:25-23:30)
- 22:40 lookdev build FO (Builds/lookdev_FO), captures at 1080p, -freeze-clock: FO_r01v2 KS15 (-foliage-v 2), FO_r01v2b KS14 (v2),
  FO_r01v1 KS15 (v1). Looked at all three full frame plus crops (fringe 880-1000 x 330-430 at 8x, NaN-like neighbourhoods).
  - v1 KS15: mean luma 0.191 (fails the KS15 window: the glowing green sponges). v2 KS15: mean 0.087, p1 0.050; KS14 v2 0.115.
  - FO-01 S1: v2 crowns read as giant smooth dark domes with an ivy fringe (core + shells on the +-6 % lumpy ellipsoid).
  - FO-02 S2: the fringe against the sky is a grey-green veil (30, 38, 36) lighter than the leaves (16-20) with the sky at
    (38, 41, 45): sprites sit at mip 4.7 at KS15 (2 m card = 40 px) where a 0.15 m leaf is still 3 px, but the sharpening was
    faded out by mip 3, so A2C dithered the soft alpha.
  - FO-03 S3: 4 bright NaN-like pixels where the koi shows through the leaves (GB-03, external); KS14 3 dark ones at the frame
    edge (42, 150), (1889, 179), (99, 207) on window cells, not foliage.
  - FO-04 S2 external: undersides 13-20 codes; no light reaches the crown bottoms (PA rig, Phase 4 lanterns).
  - Good: the sub-limbs read as real branches entering the canopy (KS14 1500-1700 x 200-450); the leaf sprays at the silhouette
    read as beech sprays.
- Fix FO-01: `GardenLeafCards.CrownShape` (lobes, GardenDims constants): L(d) = 0.66 + 0.34 S(d), S = p-norm (p 4) union of 12
  (hero) / 10 (back) smoothstep domes on a jittered Fibonacci set (angular radius 0.62-0.82 rad, height 0.82-1.0), x (1 +- 0.03)
  fine lump. Offline simulation (scratch lobesim.py, 120 outlines): every outline has min L 0.66-0.77 and max 0.94-1.0, 2-6 lobe
  tops; sphere mean L 0.84-0.86, 15-28 % of directions in valleys (L < 0.75). Cards: acceptance x L^2 x valley thinning
  (x 0.55 on the floors), n0 from the mean acceptance of a 4 096-point probe (the count stays on target); card normal = the lobed
  surface normal (central differences) tilted <= 50 deg; corner normals = that normal + the ellipsoid's corner turn, clamped to
  54 deg of the card normal; AO x lerp(0.55, 1, valley), exposure x lerp(0.40, 1, valley). Core (`GardenTree.CoreV2`): the lobed
  surface at 0.90, vertex colour R = lerp(0.55, 1, valley); shader core occlusion = _CoreAo x R (v1 mode untouched). Limb ends
  pulled to <= 0.85 x the core radius along their direction (`GardenTree.InsideCore`), so no open limb end shows in a valley.
- Fix FO-02: `_SharpMip` 5 (a 75-texel leaf is >= 2 px up to mip 5.2): sharpening kept to mip 5, faded over 1.5 mips. Moire /
  shimmer to be checked with texpans (TQ) before acceptance.
- Tests: FoliageTests.Crowns_AreLobed_AndLimbEndsStayInsideTheCore (new: both great circles reach <= 0.82 and >= 0.93, >= 2 lobe
  tops each; 64 random limb ends per tree inside 0.85 x the core); Core_CoversTheCrownMinusTheFringe now samples 0.80 x the lobed
  surface and bounds the core by 0.90 x 0.66 x 0.97.
- 23:16 FO-t04: 25 / 28. Card counts 116 290 / 115 886 (8 shrunk, 40 dropped), 232 580 triangles. Failures: corner normal 55.09
  deg (fixed: the 54 deg clamp); T1 had 4 lobe tops on the two outlines vs my 6 (dropped the total, >= 2 per outline kept);
  Beat05GardenTests.FloorSeam: GE's glass-strip z lines reach the deck end x 695 (REQUEST GE above; not an FO change).
REQUEST SU: Scripts/Editor/MaterialFactory.Surface.cs: (TEXTURE_PLAN 3.1 / 4, your rows) when Bark moves to the trident_maple_bark set, give it `_Mapping` 1 (UV0 metres): GardenTree writes the whole-tile wrapped bark UV0 on trunks, limbs and v1 branches since 721f0e1 (u = arc fraction x N x 1.8 m, v = metres along the limb). And the FloorMoss row: GardenMaterials picks `lib.FloorMoss` up by name for the v2 moss ground as soon as the field exists (no puddles on the lawn: please keep its _PuddleFraction 0).
- Markers: TP-0 READY 23:30 (TP.md:136); TP's template (NOC_PASS_UV1ZW, NOC_ALPHA_OUT, NOC_FRONT_FACE, f.specOcc, NocShadeOcc) is
  committed (489beeb), so NocFoliage.shader can be committed. DONE TP FO REQUEST (TP.md): texprep.py step 7 calls
  foliage_compose.compose; LeafCrown / LeafIvy were written to Textures/External at 23:40-23:48 (drafts until TP-1 READY).
  LeafCrown texmeta: array 2 x 2 of 1024, tile 2.0, alpha, ref 0.5, coverage 0.2107, std / mean 0.3999, saturation 0.404,
  violet 0, smoothness p5 / p50 / p95 0.30 / 0.38 / 0.48, mean 0.5 per channel (TP's step 5; my emulation had B 0.480).
- 23:55 the lookdev atlas is retired: DevLeaf/ deleted, MaterialFactory.LeafAtlasPrefix takes only the texmeta of Textures/External.
  FO-rebuild02 (queued) rebinds Leaves.mat to TP's set.
- 23:50 r02 build attempt failed: GE's ReliefTests.cs(136) mid-edit (GulfRibBuilder.Slabs / Stairs); rebuild.sh retries.
- GardenMaterials v2 moss ground: SU's lib.FloorMoss looked up by name (reflection, load time) when the field exists, else the derived
  moss with _PuddleFraction 0. REQUEST SU above (Bark _Mapping 1, FloorMoss).
- Lobes revised before r02 (scratch lobesim2.py, the outline seen from straight below = per azimuth the widest extent over the
  elevations): 12 lobes of 0.62-0.82 rad left a single bump on 10 % of the from-below outlines (FO-t05: T3 had 1); 15 (hero) /
  12 (back) lobes of 0.55-0.75 rad give 2-7 tops per outline (mostly 4-6), outline minimum <= 0.83, maximum >= 0.95.

## Step 4 - r02 (lobed crowns, TP's LeafCrown set) and the r03 changes (00:10-00:40)
- FO-rebuild02 00:02 SUCCESS (Leaves.mat now binds TP's Textures/External LeafCrown arrays). FO-t06: 27 / 28 (B1, a back tree,
  showed 1 lobe top on its from-below outline; the test now asks >= 2 for hero trees, >= 1 for back trees). Cards 116 425
  (9 shrunk, 24 dropped), 232 850 triangles.
- Captures FO_r02 (KS15) and FO_r02b (KS14), 1080p v2. Both first refused with exit 97 (another agent's player held the turn;
  cap.sh now retries every 150 s). Looked at both full frame and a 2x crop of KS15 (650-1050 x 150-450).
  - FO-01 improved: the outline breaks into lobes, the sky / far facade shows through the valleys at the silhouette, leaf sprays
    trace every outline. KS14: crowns read as several leaf masses; sprays against the lit facade behind are convincing.
  - FO-05 S2 (new): every lobe is outlined by a lighter jade ring (21-24 codes; inside the lobes 13-16): the LeafGlow rim of
    every card at grazing view angles, now also at the inner lobe boundaries. Fix: the rim only on the 2.5 % bioluminescent cards
    (LOOK_V2: LeafGlow as sparse cells and rims).
  - FO-06 S2 (new): a lobe facing the camera is a smooth black dome: card leaves (13-16 codes) and the value-noise core (13-16)
    have the same luminance, so the leaves vanish. Fix: the core becomes the deep leaves (two planar layers of the LeafCrown
    atlas on the axis plane facing the normal, 2 m sprites, hashed slice per cell, gradients from ddx / ddy of the position;
    gaps x 0.45) with _CoreAo 0.40 (row) -> about 0.26 x Leaf on average, a third of the outer leaves.
  - Other packages' changes in these frames: a lintel / tower facade now fills the notch above the garden (GE GB-02), a cyan
    ring lamp at its soffit; image_metrics KS15 "banding[crown_gap_sky] 1.48" is that facade in the gap region, not foliage.
- 01:05 FO-rebuild03 SUCCESS; FO-t07 30 / 31 (B3's vertical great circle runs along one lobe ridge: 0 tops; the lobe-top count
  now applies to the hero crowns' outline from below only, every outline still needs a valley <= 0.82 / 0.86 and a top >= 0.93).
- r03 captures (FO_r03 KS15, FO_r03b KS14; rim on glow cards only, atlas core, _CoreAo 0.40, gap 0.45). Looked at both full frame,
  KS15 crops 650-1050 x 150-450 and 200-600 x 200-500 at 3x gain, KS14 0-800 x 0-450 at 2.5x gain.
  - FO-05 fixed: no jade rings; lobe outlines are dark leaf sprays now.
  - FO-06 partly: a crown underside facing the camera reads as dense dark foliage with branches entering (3x gain), KS14's crowns
    show leaf texture over the whole mass; one lobe in KS15 (800-900, 230-330) is still a smooth dark dome: the clump noise
    (x 2.5 stretch) saturates at -1 over whole lobes, so the card density there falls to about 0.3 of the mean and the core shows,
    and the core's leaf layers at gap 0.45 are too faint. r04: clump stretch 1.8, _CoreGap 0.25.
  - Luminance (Rec.709 codes): KS15 frame mean 19.9 (LOOK_V2 window 24-42), crown undersides 14-18, fringe against the far facade
    17-21; KS14 crowns 10-15. FO-04 (external, PA): the garden rig gives a face-down leaf about 0.06 x (Moss x 0.12) and a
    face-up one 0.06 x (NightMid x 1.6 = 0.019); the leaves are at the LOOK_V2 albedo (Leaf 0.062), so the crowns stay
    silhouettes until light reaches the crown bottoms (8 leaf points of i 1.0 at half the crown-bottom height reach them at
    about 20 m; lanterns are Phase 4). r04 adds only what is physical: an outer-shell card sees about a quarter of the sky past
    the crown's side (exposure floor 0.25 for the transmitted lid light).
- Rain edge probe 1 (FO_r03rain1 / FO_r03rain0, pose KS15 785.3, 1.7, 195, yaw 90, pitch 50, -freeze-clock -freeze-rain):
  degenerate - looking up 50 deg the streaks fade by their projected length (NocRain zenith fade), rain adds 0.005 codes on
  average; 878 px differ by > 3 codes (streak pixels), none of them a leaf-edge halo. Repeat at the KS14 pose (pitch 18: crown
  edges against the lit facade).
- Perf A/B r03 (Builds/lookdev_FO, 1080p windowed, vSync 0, uncapped; `-autotest -cinematic -beat 5 -perf-log -quit-after 95`,
  order v1, v2, v2, v1; GPU frame time over the frames with s 880-1080, 4 634 frames each; FrameTimingManager samples validated by
  PerfGpu):
  | run | GPU mean | p50 | p95 | triangles |
  |---|---|---|---|---|
  | v1 a | 1.654 | 1.698 | 1.783 | 257 845 |
  | v2 a | 2.544 | 2.671 | 3.459 | 444 550 |
  | v2 b | 2.506 | 2.590 | 3.469 | 444 550 |
  | v1 b | 1.920 | 1.719 | 4.108 (another agent's GPU load: 317 rejected samples, max 238 ms) | 257 847 |
  Delta v2 - v1: mean +0.87 ms (clean v1 a), p50 +0.90 ms, p95 +1.7 ms (looking up into the canopy). Inside the 1.0 ms budget on
  mean and median, not on the canopy views. r04: inner shell x 0.7 density (InnerShellDensity) -> about 15 % fewer cards.
  (The first attempt wrote all four CSVs to QA/reports$N.csv by a quoting slip in my script; deleted, rerun.)
- 01:20 FO-rebuild04 failed on SU's mid-edit NocWet rows (_SmoothMin2 / _SmoothMax2 not in the shader yet); rebuild.sh retries.
- 01:45 FO-rebuild04 SUCCESS (retry after SU's NocWet rows compiled). FO-t08 (FoliageTests|Beat05GardenTests|MaterialTests):
  31 / 31. Cards with the inner shell at 0.7: see the log line of the next build.
- GREEN step 2 (D-030): commit 2 = the shader (TP's template is committed since 489beeb), the library rows / fields / tests, the
  lobed crowns, the atlas core, foliage_compose.py, foliage.json, this log, the critique. Not committed: the generated
  Leaves.mat / LeafCore.mat / MaterialLibrary.asset (RebuildAll output, like every other package's materials: INT) and TP's
  LeafCrown textures (TP-1 commits them).

## Step 5 - r04 measurements and the r05 changes (01:50-02:30)
- r04 (inner shell 0.7, clump stretch 1.8, outer exposure floor 0.25, _CoreGap 0.25): 99 985 cards (9 shrunk, 24 dropped),
  199 970 card triangles. FO_r04 KS15: image_metrics PASS (mean 20.0 codes; crowns 13-23); FO_r04b KS14: 3 NaN-like px at the
  frame edge on window cells (not foliage). Looked at KS15 full + crop 650-1050 x 150-450 at 3x gain: one lobe facing the camera is
  still a smooth dark dome (860, 300: 12.9 codes) with a crisp core outline. Cause found by geometry: the inner shell (0.94) lies
  only 1.2-1.6 m above the 0.90 core, so its 1.6-2.6 m cards (tilted up to 50 deg) sink half into it; with 21 % sprite coverage
  a lobe facing the camera shows the core through about one visible layer.
- Perf A/B r04 (v1, v2, v2, v1): GPU mean 1.709 / 2.474 / 2.400 / 1.580, p50 1.678 / 2.517 / 2.502 / 1.586, p95 2.031 / 3.420 /
  3.134 / 1.797 -> delta mean +0.79 ms, p50 +0.88, p95 +1.36 (r03: +0.87 / +0.90 / +1.7). Triangles v1 286 k, v2 456 k.
- Rain edge probe 2 (FO_r04rain1 / rain0, pose 661.3, 1.7, 195, yaw 90, pitch 18, vFOV 66 - crown edges against the lit facade,
  -freeze-clock -freeze-rain): rain gain mean 0.74 codes over the frame (p99 10.3, streaks); in the crown band (rows 0-300) 0.09
  over leaves / core and 0.05 over the windows; the rain-on minus rain-off image (x10, looked at) is flat over the crowns - no
  leaf-shaped structure, no halo along the A2C edges. The rain layers (3-20 m) are always nearer than the crowns (>= 33 m up),
  so the per-sample depth test never clips at a leaf; PASS.
- r05: sprites denser (foliage.json leavesPerSprite 290 / 310 / 330 / 350, 5 sprays; offline 0.264 / 0.256 / 0.283 / 0.311
  coverage, looked at the shaded atlas: dense beech clusters with ragged edges and stray leaves), core 0.86 (frees the inner
  shell), cards emitted bottom-first per shell (early-Z from below). 02:20 I re-ran `python -I Tools/texprep.py --only LeafCrown`
  (TP's tool, my config; 55 s; std / mean 0.3998, sat 0.401, violet 0) - NOTE TP: LeafCrown outputs regenerated by FO from the
  new foliage.json (no other key touched).
- Markers seen: TQ SETS READY 00:14 (TQ.md:120), TP-1 READY 02:00 (TP.md:249, LeafCrown committed in d2c7d35), FA-1 READY 22:58,
  PA-1 READY 23:53.
REQUEST TP: Assets/_Game/Textures/External/LeafCrown_{Albedo,Normal,Mask}.png + LeafCrown.texmeta.json: FO changed foliage.json (LeafCrown compose: leavesPerSprite 290 / 310 / 330 / 350, sprays 5, coverTarget 0.25-0.32; lookdev r04 showed 21 % sprites too sparse over the core) and re-ran `python -I Tools/texprep.py --only LeafCrown` at 02:20 (texprep --check is current for it; no other key touched). Please do not commit these yet: FO announces "FO ATLAS FINAL" in this log when the crown calibration is done, then please commit LeafCrown in your next calibration commit (lead decision 4 counts it), or re-run texprep yourself if you prefer.
- FO-t-r05 (02:16): 31 / 32 - "no puddle blots on the lawn": SU's new FloorMoss row carries _PuddleFraction 0.10 and GardenMaterials
  took it as is. Fixed in GardenMaterials (v2: a derived copy of lib.FloorMoss with every SU property and _PuddleFraction 0 when the
  row has puddles).
REQUEST SU: Scripts/Editor/MaterialFactory.Surface.cs: FloorMoss: please set the puddle fraction to 0 (CRIT0 #10 "no puddle mask on the lawn", TEXTURE_PLAN FO goal "no puddle blots on the lawn"); GardenMaterials derives a puddle-free copy until then, and uses your row directly once it is 0.
- r05 (denser sprites, core 0.86, bottom-first order): FO_r05 KS15 image_metrics PASS (frame mean 20.2 codes; crown undersides
  14.5, fringe against the far facade 21.8-26.7), FO_r05b KS14 1 NaN-like px (window cell at the frame edge, not foliage).
  Looked at both full frame and KS15 crops at 3x gain (650-1050 x 150-450, 1100-1920 x 100-640): FO-06 fixed - every lobe now
  reads as dense leaves, the former dome (860, 300) is covered by sprays, the underside shows leaf clusters, limbs entering the
  canopy, lobe outlines as sprays and sparse jade specks; KS14's crowns are leaf masses with sprays against the lit facade.
- Perf A/B r05 (v1, v2, v2, v1): GPU mean 1.759 / 2.437 / 2.499 / 1.679, p50 1.581 / 2.512 / 2.439 / 1.614, p95 3.003 (noisy:
  501 rejected samples) / 3.272 / 3.154 / 2.057 -> delta mean +0.75 ms, p50 +0.88 ms, p95 about +1.15 ms (v1 b as the clean
  reference). Mean and median inside the 1.0 ms budget; the canopy views stay about 0.15 ms above it (open, S3).
- texpans at KS15 (TQ's set; FO_r05tp: `-sequence texpans -capture-only seq_texpan_ks15 -foliage-v 2 -freeze-clock -rain 0`;
  reference FO_r05tpref with `-capture-ss 2 -capture-4k`; `python -I Tools/qa/sequences.py QA/captures/FO_r05tp --reference
  QA/captures/FO_r05tpref`, QA/reports/sequences_FO_r05tp.md): PASS. Shimmer above the supersampled reference: block max 1.187
  levels (gate 2; p99 0.754, mean -0.018), stabilised residual above the reference 1.212 (gate 2), luma step 0.005 / frame, no
  diff spikes, swimming p95 0.125 px (gate 0.5; max 1.137 px WARN at (192, 960) = the window facade bottom left, not foliage),
  0.45 % of blocks differ from the reference by > 2 levels (worst (1088, 256), the lit facade in the crown gap). So keeping the
  Golus sharpening to mip 5 (FO-02) does not shimmer.
- New FO test FoliageVisibilityTests.V2_AddsNoVisibilityFinding: runs V's audit (VisibilityWorld + VisibilityAudit, GE's code,
  unchanged, no report written) on the v1 and on the v2 world; v2 may add no occlusion / hidden / composition / frame / readable
  finding (the cards count as opaque triangles there, queue 2450 < 2500: conservative).
- 02:38 FO-t09 (FoliageTests|FoliageVisibilityTests|Beat05GardenTests|MaterialTests): 33 / 33. V's audit: v1 6 failures (5 of the
  checked kinds), v2 the same 6 - v2 adds none (world 2 157 of 2 166 renderers used with v2).
- Acceptance (TEXTURE_PLAN 8.3 FO "verification"): KS14 / KS15 1080p (FO_r05, FO_r05b) looked at; texpans KS15 PASS; rain edge
  probe PASS; beat-5 GPU A/B +0.75 ms mean / +0.88 p50 (<= 1.0), canopy p95 about +1.15 (S3 open); V's audit unchanged; CRIT0
  #10 / #25 met (FO-01, FO-02, FO-05, FO-06 fixed). 4K KS14 / KS15 queued (other players hold the turn).
- 02:45 -foliage-v default -> 2 (`GardenDims.DefaultFoliageV`; -foliage-v 1 keeps the graybox crowns for A/B). Player load with
  v2: 1.78 s (v1 run r01: 0.73 s; the card build adds about 1 s).
REQUEST TP: Scripts/Runtime/Rendering/NocLookGlobals.cs: `DefaultFoliageV = 2` (FO accepted 02:45; GardenDims.DefaultFoliageV is 2), so _NocFoliageV and the NocLookGlobals log line match the geometry; nothing reads _NocFoliageV in a shader today.
NOTE PA: v2 crowns are at the LOOK_V2 albedo (Leaf 0.062, atlas mean 0.5) and read as leaves; their brightness is now the beat-5 rig's: KS15 crown undersides 14-15 codes (LOOK_V2 KS15: crowns 18-30, lantern-lit undersides 40-70), fringe against the lit facade 22-27, frame mean 20.2 (window 24-42). The light reaching the crown bottoms is the 8 leaf points (i 1.0, r 30, at half the crown-bottom height, about 20 m below) and the trilight ground term (Moss x 0.12); the lantern clusters are Phase 4. FO adds no emission (LeafGlow only on 2.5 % of the cards).
FO ATLAS FINAL 03:05 (written 02:46: "03:05" was my mis-estimate of the clock, kept because TP.md quotes it; LeafCrown from foliage.json as of 02:20; TP: please commit it in your next calibration commit, see REQUEST TP above)
- 02:55 FO-tall01 (full EditMode suite, -foliage-v default 2, run 02:49): 360 / 361. The one failure is Beat02BoulevardTests.
  Blades_Are14x200_AtTheSpecPositions ("magenta, cyan, amber, violet in x order": PA's blade palette), not FO. Every foliage,
  garden, visibility, Q1 and material test passes with v2 as the default.
- GREEN step 3: commit 3 = the default switch, the puddle-free moss copy, the visibility comparison test, foliage.json (denser
  sprites), this log, the critique.
- DONE seen: TP committed LeafCrown (texture calibration commit 1 of 2, TP.md:271) and set NocLookGlobals.DefaultFoliageV = 2
  (TP.md:269). foliage.json as of 02:20 is in 79ae604, so HEAD's texprep --check is current for LeafCrown.
- 4K once (keyshots set, -capture-only KS15 / KS14, -foliage-v 2): FO_r05k (KS15 1080 / 16:10 / 21:9 / 4K) and FO_r05kb (KS14 the
  same). Looked at the 4K centre crop of KS15 (1500-2400 x 400-1000) and the KS14 4K top-left (0-1600 x 0-900): leaf masses with
  sprays against the lit facade, limbs under the canopy, lobed outlines, no card squares, no seams, no magenta / NaN. image_metrics
  flags isolated pixels at 4K (KS15 12 dark; KS14 17 dark + 3 bright; 16:10 / 21:9 0-3): 6x crops show leaf tips and petioles of
  2-3 px leaves over bright window cells (FO-09 S3, accepted). Mean luma KS15 0.079 (all variants), KS14 0.122 / 0.131.
- Not re-verified in a picture: the puddle-free FloorMoss copy (built after r05; FoliageTests.Materials_V2 checks
  _PuddleFraction 0 on the v2 moss ground; KS14 / KS15 do not show the lawn).
- Disk: intermediate folders deleted (FO_r01*, r02*, r03*, r04, r04b, r05tp / r05tpref after their numbers were logged; r03 / r04
  perf CSVs). Kept: FO_r05 (KS15), FO_r05b (KS14), FO_r05k / FO_r05kb (4K sets), FO_r04rain1 / rain0 (rain probe),
  QA/reports/FO_perf_r05_*.csv, QA/reports/sequences_FO_r05tp.md. W: 20 GB free (other packages).
- Open at the end: FO-04 (external, PA / Phase 4 light), FO-07 S3 (canopy p95 GPU about +1.15 ms), FO-08 REQUEST SU (FloorMoss
  puddles 0 in the row; worked around), FO-09 accepted.
FO DONE 2026-11-16 03:01
