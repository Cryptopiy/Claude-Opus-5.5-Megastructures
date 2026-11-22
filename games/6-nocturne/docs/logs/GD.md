# GD log - beat 5, the hanging garden (Phase 4 wave A fifth start + wave B, 2026-11-20)

Owner GD (new role this wave). Copy: .worktrees/waveb (branch n6-waveb), partners AL, TR, AU. Build folder Builds\GD.
Files: Beats/Beat05_Garden/** (and new files there), Shaders/Beats/NocFoliage.shader, the .Foliage partials
(Editor/MaterialFactory.Foliage.cs, Runtime/Core/MaterialLibrary.Foliage.cs, Tests/EditMode/MaterialTests.Foliage.cs),
Tests/EditMode/Beats/FoliageTests.cs, Tests/EditMode/Beats/Beat05GardenTests.cs (+ new GD test files), docs/logs/GD.md,
QA/critique-GD.md. Requests to GD are answered here as "DONE GD <request>".

## Start (20:34-20:55)
- Read: MASTER_PROMPT 14.5.5; LOOK_V2 2.4 (R1-R3), 6 (garden rig: 10 lantern clusters lerp(Lantern, WinNeutral, 0.4) r 14 i 3.5,
  8 leaf points), 8.1 (KS15 24-42, crowns 18-30, lantern-lit undersides 40-70); critique-lookv2-01 INT-04 / INT-03 / INT-28;
  FO log (v2 crowns, cards, core, perf A/B +0.75 ms), G3 log; code of every file of mine.
- Looked at the baseline (lead's lookv2_01 / lookv2_01b, main tree): KS15 near black (crowns 13-20 codes, no lanterns, the
  only highlights are the far facade windows); beat05_garden_mid: black lawn, path a dark ribbon, trunks as silhouettes;
  KS14 the crowns over the gate are dark textured masses.
- fastcheck all OK (runtime 199, editor 265). GD-b01 queued behind AL-b01 (first shader compile of this Library).
- No "REQUEST GD" line in any log of any copy (grep at 20:40).

## Plan (in order)
1. INT-04 + lanterns: GardenLanterns.cs (new): 520 lanterns per tree (8 840), 0.35 m icosahedra (20 tris) baked into one static
   mesh per tree, drawn by NocFoliage _LeafMode 3 (no new shader, no GraphicsSettings change): vertex hook keeps >= 2.2 px
   (radius expanded about the lantern centre, emission x (r / r')^2: energy preserved), emission Lantern x 2.0 x a 0.1 Hz
   +-6 % pulse on the scene clock. Placement: most in the lower canopy among the leaves, the rest hanging below it, plus
   10 low lantern chains with the LOOK_V2 cluster lights (r 14 i 3.5) for the pools on the path and moss.
   Under-canopy light: each card / core vertex gets the lantern field's irradiance (soft-kernel sum over the tree's lanterns)
   in vertex colour A; NocFoliage adds it as multiply-scattered leaf light (warm near a lantern, greener deeper).
2. Fireflies (_LeafMode 4, 600 motes, drift / orbit on the scene clock), leaf sway (vertex hook, cards only), drips.
3. Delete the foliage v1 path (shader mode 0, GardenTree v1 branches / crown, GardenMaterials fallback, v1 tests); REQUEST FX for
   the -foliage-v flag.
4. Terrace / path / moss / benches / planters: real matter, lit by the lantern pools.

## Step 1 - lanterns, field, posts, fireflies, sway; v1 deleted (20:55-21:35)
- GD-b01 20:57 crashed at start (Unity "Failed to initialise UDS client", Logs/GD-b01.log:97; infra). GD-b02 21:05-21:18 failed
  on AL's mid-edit Beat01_Alley files (AlleyMaterials.cs(86) 'NoodleBar.Lamp0', Logs/GD-b02.log:1038; not mine). fastcheck of my
  files OK throughout; GD-b03 queued 21:33 once AL's files compiled again.
- New `Beats/Beat05_Garden/GardenLanterns.cs`: tree lanterns (520 per tree, 8 840; 55 % among the leaves at 0.89-0.99 of the
  lobed radius, 30 % just under them 1.03-1.08, 15 % on 1-6 m cords; only under the crown's equator), 0.35 m icosahedra
  (12 vertices / 20 triangles, uv0 = (radius, hash), radial normals) in one mesh per tree; the lantern field (2D bucket grid,
  E = sum (0.25 + 0.75 max(0, n.l)) / (d^2 + 1.5^2) x (1 - (d / 8)^2)^2, A = E / 4 into colour A of every core and card
  vertex); 10 lantern posts on the planters (steel post 3.0 m, 0.6 m crook arm, 3 lanterns at 2.06-2.5 m, one light each
  lerp(Lantern, WinNeutral, 0.4) r 14 i 3.5 = LOOK_V2 6's 10 lantern clusters); 600 fireflies (r 1.2 cm, 5.5-26 m off the
  centre line, 0.4-3.5 m up, clear of the trunks).
- `NocFoliage.shader`: v1 mode deleted (mode 0 = core; v1-only properties and NocFoliageV1 gone); NOC_HAS_VERTEX_HOOK: leaf sway
  on the cards (0.25 m, 7 s + a 0.618x slower partial, both <= 0.2 Hz), lantern / firefly balls grown to >= 2.2 px about their
  centre with emission x (r / r')^2; mode 3 lanterns (Lantern x 2.0 x (1 +- 0.06 sin(2 pi (0.1 t + hash))) x energy), mode 4
  fireflies (lerp(LeafWarm, LeafGlow) x 8 x slow glow 0.12 Hz, near fade 1.5-4 m, wander 1.5 m / 0.05 m/s + 0.3 m orbit);
  the lantern field light in modes 1 / 2 (lum(albedo) x E x 0.6 x lerp(Lantern, WinNeutral, 0.4) x chroma^2.5). No keyword.
- Foliage v1 deleted in C#: GardenTree (BuildBranchesV1, WrapBarkLathe, Lump, Point, the v1 crown branch), GardenDims
  (CrownLump, FoliageVersionOverride, DefaultFoliageV, FoliageV2), GardenMaterials (ModeCrownV1, the v1 moss puddles);
  tests: FoliageTests.V1_EmitsOnlyTheGrayboxCrowns deleted, the bark test runs once, FoliageVisibilityTests now compares the
  garden without / with its lanterns (GardenLanterns.Enabled hook); Beat05GardenTests: the crown-bounds test runs on the lobed
  surface (bottom <= 0.62 Ry below the centre: the lobe floor is 0.66; top / radius 0.80-1.031), lights 8 + 10.
- New test file `Scripts/Tests/EditMode/Beats/GardenLanternTests.cs` (5 tests: 8 840 balls, under the crowns, field on the
  undersides, posts / fireflies, shader contract). New QA script `QA/GD_garden_check.py` (14.5.5 (2) numbers: mean, green share
  G > R + 15 and G > B, lantern blobs > 200 of 3-12 px). Baseline (main tree lookv2_01 KS15): mean 17.0, green 0.000, blobs 0;
  KS14 mean 25.5, blobs 3; beat05_garden_mid mean 19.6.
- REQUEST FX (in my report's shared_change_requests): -foliage-v flag parsing (NocLookGlobals.FoliageV / DefaultFoliageV,
  ShaderIds.FoliageV, the CliArgs doc line): v1 no longer exists, nothing in the garden reads the flag any more.

## Step 2 - r01 (GD-b03 21:37-21:49, 0 errors) and the r02 changes (21:50-21:55)
- GD_r01 (Builds\GD, `-capture-set keyshots1080,beats -capture-only KS14,KS15,beat05`, 9 stills, failures 0, alloc 0 B/frame).
  Looked at KS15 / KS14 / beat05_garden_mid full frame, KS15 crop (1100-1580 x 300-570 at 2x), a 4-up sheet of the other stills.
  `QA/GD_garden_check.py` (14.5.5 (2) numbers):
  | still | mean | green share | blobs | note |
  |---|---|---|---|---|
  | KS15 | 25.7 (17.0) | 0.002 | 0 | lantern peaks luma 195 (mean colour 235 / 168 / 103): maxima > 200 not met |
  | KS14 | 29.1 (25.5) | 0.001 | 9 | crowns over the gate full of lantern points: reads well |
  | beat05 mid / start / end | 20.9 / 21.7 / 22.1 | 0 | 10 / 21 / 3 | ground still dark; post pools miss the path |
  - INT-04 partly: KS15 mean 17.0 -> 25.7 (inside 24-42), the crowns read as lit green leaf masses (crown samples 21-23 / 30-34 /
    25-26 RGB, were 13-20), lanterns everywhere under both crowns.
  - GD-06 S2 (new): 520 independent lantern points per crown read as an even scatter of fairy lights; the field is too even to
    light the leaves locally (no warm pools around lanterns); lanterns flat orange discs, peak luma 195 (no hot core, no bloom).
  - GD-07 S2 (new): post lanterns at 20-25 m read as hexagons (icosahedron outline); the posts on the planters (6.9 m) leave the
    path dark (pool 6.5 m from the light).
  - Green share 0.002 (14.5.5 asks >= 55 % with G > R + 15): the canopy is a grey-green (G - R about 10-12 codes).
- r02 changes: tree lanterns in strands (104 strands a tree of 3-7, 0.6-0.9 m steps, the top among the leaves), field kernel
  soft 1.0 m (was 1.5), field gain 1.2 (was 0.6) + the jade under-glow of the down-facing leaves (LeafGlow x 0.02 x albedo / Leaf),
  hot lantern core (0.55 + 0.9 (n.v)^2, disc mean 1.0), posts on the moss verge at 4.6 m (light 4.5 m from the path centre)
  with smooth UV-sphere lanterns (8 x 12). GD-b04 queued 21:53 behind AU-t01; GD-t01 (my tests) queued after it.

## Step 3 - r02 (GD-b04 21:57-22:03, 0 errors) and the r03 changes (22:05-22:10)
- GD_r02 (same set, 9 stills, failures 0). QA/GD_garden_check.py:
  | still | mean | green share | blobs |
  |---|---|---|---|
  | KS15 | 26.4 | 0.015 | 27 (>= 25 met; peaks > 200 now) |
  | KS14 | 29.3 | 0.006 | 109 |
  | beat05 mid / start / look_up60 | 23.2 / 21.9 / 28.0 | 0.001-0.017 | 11 / 33 / 30 |
  Looked at KS15 full + crop (1100-1580 x 300-570, 2x) and beat05_garden_mid (lower 2/3): strands read as hanging lantern
  chains with warm-lit leaves around their tops (KS15 samples (48, 53, 34), (39, 45, 31)); seen up 50 deg the 3-7 strands'
  vertical dotted lines converge on the zenith like rain streaks in the far crowns (GD-08 S3). Post lanterns now round paper
  balls on a crook post with a warm pool on the moss (GD-07 fixed: beat05_garden_mid right side).
  - The jade under-glow did not show (crown samples (19, 29, 25) as in r01): the library LeafCore / Leaves .mat files predate
    the Phase 4 properties and the built materials did not take the shader defaults (GD-09). Fix: GardenMaterials derives
    runtime copies of lib.LeafCore / lib.Leaves and writes every Phase 4 property explicitly (SetPhase4; constants in
    GardenMaterials: field 1.2, scatter 2.5, under-glow 0.025, sway 0.25 / 7 s, lantern 2.0 / 0.06 / 0.1 Hz, min 2.2 px,
    fireflies 8 / 0.12 Hz). No RebuildAll needed; the lead's RebuildAll changes nothing of this.
  - Player alloc line at quit in r02: "mean 498073.6 B/frame ... frames allocating 1, worst 2490368 B" (r01: 0 B). One frame
    in the steady window; my code runs at load only. To re-check in r03 / smoke.
- r03: strands of 1-4 (mean 2.5, 0.5-0.8 m steps), under-glow 0.025, explicit material values. GD-b05 queued 22:09
  (behind TR-b02 and my GD-t01).
REQUEST FX: Assets/_Game/Scripts/Runtime/Rendering/NocLookGlobals.cs: the foliage v1 path is deleted (GD, Phase 4 wave B), so -foliage-v goes the way of -facade-v / -surf-v: (1) `public static readonly string[] RemovedFlags = { "-facade-v", "-surf-v", "-foliage-v" };` (2) replace `public static int FoliageV { get; private set; } = DefaultFoliageV;` by `/// <summary>Always 2 (GD deleted the v1 foliage in wave B).</summary> public static int FoliageV => DefaultFoliageV;` (3) delete the line `FoliageV = Flag("-foliage-v", DefaultFoliageV);` in Read(), and drop `FoliageV != DefaultFoliageV ||` from the log condition; (4) header line 5 becomes `//   -foliage-v          removed in wave B (GD): the v1 foliage is deleted; passing it logs a warning and changes nothing`. Keep `Shader.SetGlobalFloat(ShaderIds.FoliageV, FoliageV);` (writes 2; nothing reads _NocFoliageV). CliArgs.cs: drop -foliage-v from the flag list in its header if it is listed. Nothing in the garden reads the flag since GD's change.
- 22:23 GD-t01 (FoliageTests|FoliageVisibilityTests|Beat05GardenTests|GardenLanternTests|MaterialTests; the r03 code), QA/reports/
  GD-t01.xml: 36 / 38. Both failures mine and the same cause: the lane is 6.0 m half-width + 0.38 (not 3.0): r02's posts at
  4.6 m put the post lanterns at 4.06 m inside it (Beat05GardenTests.Furniture_AndTrees_OutsideTheLaneEnvelope "post_lanterns
  vertex (720.30, 2.11, 190.94) at s 905.0 inside the lane"; GardenLanternTests.Posts_... "outside the lane"). Fixed: posts back
  on the planter line (6.9 m, 2.6 m past the planter centre), 3.3 m tall, 0.5 m arm, lanterns at 2.47-2.87 m (bottoms 2.29 m:
  the lane check covers 0.02-2.2 m), light at 6.46 m; fireflies 8.3-26 m off the centre line (their 1.8 m reach stays outside
  6.38). Passed in GD-t01: FoliageVisibilityTests.Lanterns_AddNoVisibilityFinding, the 4 other GardenLanternTests, every
  FoliageTests / Beat05GardenTests / MaterialTests case (v1 deletion, lobed crown bounds, 8 + 10 lights).

## Step 4 - r03 (GD-b05 22:31-22:40, 0 errors), smoke on b05, the r04 tweak (22:40-22:50)
- GD_r03 (same set, 9 stills, failures 0, errors logged 0, alloc at quit 0 B / 0 frames: r02's one allocating frame did not
  recur). GD_garden_check: KS15 mean 28.4, green 0.069, blobs 19; KS14 30.4 / 0.034 / 77; look_up60 30.5 / 0.083 / 28;
  beat05 mid / start 21.0 / 22.4. KS15 pixels with G > R, G > B and luma > 18: 54 % (median G - R 12 codes, luma 34): the
  foliage coverage is there, the +15 margin is not. Looked at KS15 full and a 4-up sheet (KS14, mid, look_up60, start):
  lit green leaf masses with lantern clusters and warm pools (samples (29, 44, 28), (37, 46, 32)); the start still shows the
  glowing crowns over the dark path with the post lanterns along it. KS14's gate changed under TR's skybridge work (not mine).
- 22:47 smoke on b05 (`-autotest -brisk -autotest-until 1080 -beat 5`): AUTOTEST PASS, beats 2, errors 0, alloc mean 30.0 B/frame
  (inside the engine input box 32, excess 0), QA/reports/autotest_brisk_from880_until1080.json.
- r04 (GD-b06 queued 22:45): under-glow 0.045 (was 0.025: +5 G codes on the down-facing leaves, sat stays <= 0.40), strands of
  2-4 hung one step below anchors at 0.93-1.0 of the lobed radius (r03's 1-4 hid a third of the lanterns among the leaves:
  KS15 19 blobs < 25).

## Step 5 - r04 (GD-b06 23:01-23:02, 0 errors, 26 s build), smoke on b06, LOOK_V2 / image_metrics, the sparkle fix (23:02-23:10)
- 22:47 / 23:00: my queued GD-t02 (redundant with the full suite) was cancelled to free the queue (the mutex is not FIFO:
  partners' jobs ran first); AU-t02 10 min, TR-b03, TR-final ran meanwhile.
- GD_r04 (Builds\GD from b06, same set, 9 stills, failures 0, errors 0). GD_garden_check:
  | still | mean | green share | blobs |
  |---|---|---|---|
  | KS15 | 31.8 | 0.292 | 26 (>= 25 met) |
  | KS14 | 32.0 | 0.123 | 98 |
  | look_up60 / start / mid / look_back | 34.5 / 23.1 / 21.2 / 26.7 | 0.348 / 0.063 / 0.017 / 0.024 | 30 / 18 / 7 / 126 |
  Looked at KS15 full frame and the crop 200-680 x 450-720 (2x): lit leaf masses with leaf texture, lantern strands and warm
  pools; the canopy is "the one green-lit moment". Contact sheet QA/captures/GD_r04/_contact/contact_GD_r04_01.png.
- lookv2_check GD_r04 (Tools/qa/lookv2_check.py): KS15 passes mean (31.8 in 24-42), R1, R2, R3; FAILS p99 luma 97.8 (>= 120:
  INT-03, the lantern pixels are < 1 % of the frame) and dark cast BG -2.02 / RG -4.85 (the darks are green: the green garden
  14.5.5 asks for; conflicting rules, for the lead). KS14 fails p99 99.4 only. Baseline (lookv2_01): KS15 failed mean 17.0 and
  p99 44.4.
- image_metrics GD_r04 (QA/reports/image_metrics_GD_r04.md): NaN-like bright pixels 2-29 per still. Looked at 6 of them in KS15
  (32 px tiles at 6x): single orange pixels = lanterns among the leaves seen through 1-px gaps of the alpha-to-coverage cards
  (e.g. (1003, 553) = (208, 146, 76) among (27, 43, 34)); would sparkle as the leaves sway. GD-12 S2. Fix (code 23:08): every
  strand hangs from 1.04-1.08 of the lobed radius (under the outer card shell at 1.02), first lantern one step below. GD-b07
  queued 23:09. beat05_garden_look_back's row at y 373-375 (x 825-1086, every 12-25 px) is far lamps along the bridge view, not
  a garden mesh (to confirm in r05).
- 23:06 smoke on b06 (`-autotest -brisk -autotest-until 1080 -beat 5`): AUTOTEST PASS, beats 2, errors 0, alloc 31.8 B/frame
  (engine input box 32, excess 0). Capture runs r02 / r04 logged one allocating window at quit (477-498 KB/frame over 16
  frames, 1-5 frames); r01 / r03 0 B; the 4 504-frame smoke windows are clean, so it is the capture's quit path, not a frame of
  the walk (my code runs at load only).
- Beat 5 build numbers (player log): 8 840 tree lanterns in 17 meshes, 10 posts, 600 fireflies, beat-5 triangles 652 673
  (<= 750 k, 14.5.5 (4)); "built Beat05_Garden in 4.4-5.5 s" (the lantern field over ~480 k crown vertices costs about 3 s of
  load; GD-13 S3: evaluate it per card instead of per vertex).
- Disk: W: 13 GB free at 23:07 (below the 15 GB line; not my folders). Deleted GD_r01..r03 after logging; kept GD_r04
  (24 MB) and Builds\GD (464 MB).
- 23:13 TR-final (TR's full EditMode suite, run on this copy's code of 23:02 = my b06 state): 439 / 447 (+1 skipped). None of
  the 7 failures is a garden test or names a garden mesh: AbyssTests.KS10_DropShare, Beat07HeartTests.PlazaInlay,
  BoulevardStreetTests x 2, Q1IntegrationTests.A (Beat01_alley ventFrames / ventGratings), Q1IntegrationTests.B (Beat04
  trains T4 / T7), WetFloorTests.WallWindows. Every FoliageTests / FoliageVisibilityTests / Beat05GardenTests /
  GardenLanternTests / MaterialTests case passed there (QA/reports/TR-final.xml).
- 23:15 features stopped. In the queue: GD-b07 (the sparkle fix) and GD-final (my full suite), both behind AL-b05.

## Step 6 - final: GD-final, the field test, r05 (GD-b07), smoke (23:14-23:30)
- 23:21 GD-final (full EditMode suite on the code of 23:14 = the sparkle fix), QA/reports/GD-final.xml: 438 / 447, 1 skipped,
  8 failed. 7 are not mine and identical to TR-final: AbyssTests.KS10_DropShare_AtLeast40 (AB/AT, 0.298 < 0.40),
  Beat07HeartTests.PlazaInlay (HT, 962 <= 1000), BoulevardStreetTests.LightBudget (BV, 29 > 28) and .Shops_Varied (BV, 18 > 17),
  Q1IntegrationTests.A (Beat01_alley ventFrames / ventGratings: AL) and .B (Beat04 trains T4 / T7: TR), WetFloorTests.
  WallWindows (FX, -1.4e-11 < 0: a rounding sign). 1 is mine: GardenLanternTests.Field_LightsTheUndersides "down-facing mean
  7.2 > 12" on the CORE: the sparkle fix hangs every lantern under the outer leaf shell, so the deep core (0.86) gets only the
  light that passes the leaves. The check moved to the visible leaves (23:22): the card meshes' down-facing vertices must carry
  colour A > 12 and > 4 x the up-facing ones; the core keeps "> 2 and > 4 x up". GD-t03 (my 5 test classes) re-runs it.
- GD_r05 (GD-b07 23:22, 0 errors; 9 stills, failures 0, alloc 0 B). GD_garden_check:
  | still | mean | green share | blobs | p99 |
  |---|---|---|---|---|
  | KS15 | 32.1 | 0.267 | 67 | 138.9 |
  | KS14 | 31.9 | 0.111 | 199 | 108.8 |
  | look_up60 / start | 34.5 / 23.2 | 0.322 / 0.059 | 47 / 33 | 142.0 / 79.3 |
  lookv2_check: KS15 passes mean, R1, R2, R3 and now p99 (138.9 >= 120: INT-03 met for KS15); fails dark_cast_RG -4.82 only
  (the darks are green). KS14 fails p99 108.8 only. image_metrics KS15 NaN-like 18 (bright 13, dark 5; r04 24): looked at 6 at
  6x: the bright ones are gone from the leaf gaps (one ball-edge pixel by a lantern cluster); the rest are 1-px dark gaps between
  lit leaf cards (FO-09's kind, now on brighter leaves: GD-14 S3). Looked at KS15 full frame and the contact sheet
  QA/captures/GD_r05/_contact/contact_GD_r05_01.png: lit green canopy, lantern strands under the leaves with warm pools; the
  eye-level stills stay dark at the ground beyond the post pools (GD-10).
- 23:25 smoke on b07: AUTOTEST PASS (brisk 880-1080, beats 2, errors 0), alloc 21.8 B/frame (excess 0).
- Disk: deleted GD_r04 after logging; kept QA/captures/GD_r05 (24 MB) and Builds\GD (464 MB).
- 23:33 GD-t03 (GardenLanternTests|Beat05GardenTests|FoliageTests|FoliageVisibilityTests|MaterialTests on the final code),
  QA/reports/GD-t03.xml: 38 / 38. Field: cards down-facing mean colour A 15.2 (up 0.5), core 7.2 (up 0.8); visibility audit
  without / with lanterns 6 / 6 failures of the checked kinds (the lanterns add none). With GD-final (438 / 447, the 7 other
  failures not mine) every test of the suite that touches my files passes on the final code.
- fastcheck all OK (runtime 209, editor 278) at 23:34.

## Open at the end
- GD-10 S3: ground (moss, path) dark beyond the post pools; the path is SU's FloorGardenPath photo row (no 0.8 m / 0.15 m
  slab joints): needs a NocWet slab-joint mode (FX) or a garden path material of its own.
- GD-11 S3: KS15 green share (G > R + 15 and G > B) 0.267 (14.5.5 asks 0.55); with LOOK_V2 R2 (<= 0.45) / R3 (<= 0.35) a
  foliage pixel needs G >= 34 codes to carry G - R > 15; G > R and G > B with luma > 18: 0.55. Proposed to the lead (decision).
- GD-13 S3 (load ~3 s for the lantern field), GD-14 S3 (1-px dark leaf gaps), GD-15 S3 (dark cast rule vs the green garden),
  GD-05 S3 (drips not built), leaf sway and fireflies not looked at in motion (no sequence run; statics are -freeze-clock),
  KS14 p99 108.8 (< 120; INT-03, the gate frame is TR's), the -foliage-v flag (REQUEST FX above).
GD DONE 2026-11-20 23:34

# FIX WAVE (panel p4_01, master caddb90; 2026-11-21 01:57-)
Task: Logs/fix/GD.md (C32 lantern / firefly single-pixel maxima, C33 mint canopy + bark; lead ruling: beat-5 dark cast R - G in
[-8, +4] if the hue fix cannot reach |R - G| <= 4). Baseline = the lead's p4_01 captures of the same commit (INTEG\QA\captures\p4_01):
KS15 mean 32.1, green share (G > R, G > B, luma > 18) 0.540, green-pixel hue median 137.6, sat 0.36, NaN-like bright 13, warm
blobs > 200 68; KS14 hue 130.9, NaN-like bright 21, p99 108-116. No "REQUEST GD" line in any copy's logs (grep 02:30).

## Causes found (02:00-02:10)
- C32: a lantern drawn at the 2.2 px floor had the floor on the icosahedron's CIRCUMradius (silhouette 1.75-2.2 px) and the hot
  face-on profile 0.55 + 0.9 (n.v)^2. A sim of the MSAA 4x resolve (QA/GD_lantern_sim.py: flat / hot discs at 12 x 12 sub-pixel
  offsets, display = 255 x lin^(1/2.2)) gives the centre pixel's lead over its best 5 x 5 neighbour: hot 2.2 px 115 codes, hot
  1.75 px 160 (image_metrics counts > 77 as NaN-like); flat 2.2 px 55-69, flat >= 2.6 px <= 31; flat discs >= 2.2 px keep a
  constant peak at every offset (no twinkle). The critic's +100-137 maxima are exactly this.
- C33: the leaf atlas is hue-neutral (opaque mean 0.709 / 0.717 / 0.695), so the albedo is Leaf (hue 93); the mint came from the
  under-glow PAL_LeafGlow (0.21, 0.75, 0.43: B = 2 R) on every down-facing leaf and the field light's chroma^2.5 suppression.
  The limbs are lib.Bark (the CC0 trident maple photo is there) but nothing lights them under the crowns: black tubes.

## Changes and cycles (02:05-02:35)
- NocFoliage.shader (one write per change): C32: the 2.2 px floor on the inscribed sphere (inK 0.795), custom.z = drawn diameter
  px, the lantern profile blends from flat 1.0 (under 0.5 _FlatPx) to the hot core (from 1.5 _FlatPx), disc mean 1.0 both (energy
  unchanged); tree lanterns behind their own crown's leaf fringe fade out and collapse (uv1 = crown centre x, z, y, Rh; colour A =
  Ry / 2 Rh). C33: under-glow colour lerp(LeafGlow, LeafWarm, _UnderWarm 0.65) at LeafGlow's luminance; field chroma power from
  _ScatterPow 2.5 deep in the crown to 1 at E >= _NearWarm 1.6 (leaves beside a strand get the lantern's warmth); mode 5 BARK
  (lib.Bark's CC0 trident maple photo maps on the limbs' UV0 / 1.8 m, derivative tangent frame for the photo normal, template PBR,
  plus the lantern field as direct light). The clip hook now cuts only the cards (mode 2): it also cut modes 3-5 by the atlas alpha
  at their uv (depth passes only; latent). Vertex hook: sway only on mode 2, balls only on modes 3-4.
- GardenMaterials: Limbs (mode 5, derived from the cards material for the atlas sampler, lib.Bark _AlbedoTex2 / _NormalTex2 /
  _BaseColor2 / _NormalStrength2, smoothness 0.35, queue 2000); FieldLightGain 1.2 -> 1.6, UnderGlow 0.045 -> 0.035 (less even
  under-glow, more lantern pools), FlatPx 3, UnderWarm 0.65, NearWarm 1.6. GardenTree.Build(.., limbs = null): the limbs into
  their own builder; Beat05_Garden: limbs mesh with the field in colour A, lanterns with their crown (GardenLanterns.Balls overload).
- r06 (GD-b08 02:05, 0 errors; flat under 3-9 px, no fade): KS15 hue median 95.3 (was 137.6), mean 32.6, green share 0.534, limbs
  read as lantern-lit brown bark (looked at KS15, KS14, look_up60 full frame); but blobs > 200: 0 (the near lanterns lost the hot
  core) and NaN-like bright still 13: QA/GD crops at 8x (1082, 523), (1396, 468) ...: whole-radiance single orange pixels among the
  leaves = lanterns seen through 1-px gaps of fringe cards, not min-size discs.
- r07 (GD-b09, fringe fade (1.05, 1.12), _FlatPx 3): KS15 NaN-like bright 8, KS14 8 (r06 13 / 12); blobs 18 / 21 (> 200 at
  190-202: the 0.55 + 0.9 core at the threshold).
- r08 (GD-b10, core 0.35 + 1.3 (n.v)^2 (disc mean 1.0), fade (1.08, 1.18)): KS15 blobs 76 (GD_garden_check; >= 25), p99 136.8,
  mean 32.5, NaN-like bright 7; KS14 blobs 26, p99 141.4 (>= 120: TR's C31 highlight rule passes on KS14 with the lantern cores),
  NaN-like 8; lookv2_check: KS14 PASS (all checks), KS15 fails dark_cast_BG -3.21 only (R - G -2.40: |R - G| <= 4 met without the
  ruling; p4_01 B - G was -1.89 by my count, -2.23 by the critic's). The B - G is the leaf chroma itself: luma 24-32 holds 13.9 %
  of the frame at B - G -5.98 (76 % green-dominant), 32-40 15.8 % at -11.87 (92 %); for any hue < 120 B - G = -(G - min) = -chroma.
- GD_r06 / r07 deleted after logging (25 MB each).
- r09 (GD-b11 02:31-02:33, 0 errors; fade also when the ray grazes the fringe 1-2.4 m before the lantern): KS15 NaN-like bright 7
  at the SAME positions and values as r08 ((1082, 523) (240, 165, 88) +123, (1396, 468), (1359, 528), (752, 416), (1082, 530),
  (1575, 697), (633, 792)): not own-crown peeks (the fade changed none of them); 8x crops: single lantern pixels among the leaves of
  the far crowns seen through the near crowns' fringes (the fade knows only a lantern's own crown). KS14 7: 5 lantern pixels,
  (753, 15) a white window pixel (213, 222, 235) of the far facade, (1354, 205) a lit leaf pixel (175, 182, 105) (p4_01 had it green
  at the same place: a leaf, not a firefly). Blobs KS15 76, KS14 26; KS15 mean 32.4, p99 129; KS14 mean 33.5, p99 136.9.
- 02:37 GD-t04 (my 5 test classes, code of 02:29), QA/reports/GD-t04.xml: 39 / 40. The failure is
  Beat05GardenTests.Lights_WetSources_Foliage "one wet source per crown Expected 38 But was 43": beat 4 registers 5 more wet
  sources since TR's fix-wave gate light content (git diff: SkybridgeTubes.cs +54, SkybridgeMaterials.cs); the test now counts the
  garden's own sources (TotalSources after beat 5 minus before = 17) and that beat 4 registered at least its lamps.
- texpan KS15 on b11 (GD_tp09 + GD_tp09ss = -capture-ss 2, both -rain 0 -freeze-clock; sequences.py --reference): FAIL shimmer
  5.32 (block (320, 640)), stabilised residual 2.74 (p4_01 had no reference run; its residual p99 3.594, mine 3.584). My lantern
  mask (warm pixels of either run, dilated 2 px, 9.4 % of the frame): temporal std 29.17 vs the reference 27.10 (+2.07); the 8 worst
  blocks are 64-100 % lantern mask; crop of (304-368, 624-688) at 6x: 4-5 px discs whose MSAA rim changes shape frame to frame,
  the reference's rims are smooth. Fix (02:45): a soft rim 0.7 px wide on balls of 2.5-4 px and up, smoothstep(0, T, n.v),
  renormalised by the profile's disc mean (numeric check: disc mean 1.0000 at 2-10 px, flat / half / full hot): energy unchanged.
  Frames deleted after logging (GD_tp09 98 MB, GD_tp09ss).
- r10 (GD-b12 02:46-02:51, 0 errors; the soft rim): KS15 blobs 128, KS14 36, but KS15 p99 129 -> 108.2 (lookv2_check p99 >= 120
  FAILS: the rim moves the lantern energy into fewer, hotter pixels) and NaN-like bright KS15 7 -> 10, KS14 7 -> 9. texpan KS15
  (GD_tp10 / GD_tp10ss): shimmer 5.55 (block (320, 640); p99 2.28, was 2.59), stabilised residual 3.64 (was 2.74): worse where it
  counts. REVERTED (02:56) to the r09 lantern shading (inscribed floor, flat -> 0.35 + 1.3 (n.v)^2 over 1.5-4.5 px, crown-fringe
  fade); the shimmer of the dense near clusters stays open (C32-b below). GD_tp10 / GD_tp10ss / GD_r10 frames deleted after logging.
- Near-strand undersides (QA crop metric, scratch under.py: leaf pixels G > R, G > B, luma > 12 at 3-12 px from a lantern blob vs
  >= 30 px): r09 KS15 median 42.7 / p75 52.1 / p90 60.9, hue 90 (warm spill), far leaves median 35.4, hue 99 (p4_01: 43.1 / 50.7 /
  57.9, hue 123; far 37.1, hue 142): inside the spec's 40-70 beside the strands, warmer there than deeper in.
- Isolated bright pixels on the other beat-5 stills (r09 vs p4_01b): look_up60 11 (18), start 1 (9), end 2 (2), look_l 2 (2),
  look_r 5 (5), mid 1 (1), look_back 20 (21: the row at y 373-375 is grey far lamps of the bridge view, 2 lantern pixels at the top).
- 03:02 other-crown fringe fade: GardenLanterns.SetCrownGlobals writes the 17 crowns into the global array _NocGardenCrowns[34]
  (even = centre, odd = radii) + _NocGardenCrownCount once at build (no per-frame work, no allocation); the tree-lantern vertex hook
  runs the "eye ray grazes the fringe 1-2.4 m before the lantern" test against every crown with |l_j| >= 1.3 (not its own) and
  multiplies the fades. 17 x 12 vertices x 8 840 = 1.8 M short ray tests a frame on the vertex stage. Test:
  GardenLanternTests.Lanterns_CarryTheirCrown_ForTheFringeFade reads the globals back. GD-b13 (02:58-03:03, 0 errors) is the
  reverted r09 state; GD-b14 builds the crown globals.
- Disk: W: 11 GB free at 02:57 (under the 15 GB line; my share: Builds\GD 464 MB + one 25 MB capture folder; GD_r05 of the last
  wave deleted too).

## Requests (fix wave)
REQUEST FX (open since wave B, not carried at caddb90): Assets/_Game/Scripts/Runtime/Rendering/NocLookGlobals.cs: (1) line 38
`public static readonly string[] RemovedFlags = { "-facade-v", "-surf-v", "-foliage-v" };` (2) line 35 `public static int FoliageV
{ get; private set; } = DefaultFoliageV;` becomes `public static int FoliageV => DefaultFoliageV;   // always 2: GD deleted the v1
foliage in wave B` (3) delete `FoliageV = Flag("-foliage-v", DefaultFoliageV);` in Read() and drop `FoliageV != DefaultFoliageV ||`
and ` foliage-v {FoliageV}` from the log line (4) header line 5 becomes `//   -foliage-v          removed in wave B (GD): the v1
foliage is deleted; passing it logs a warning and changes nothing`. Keep `Shader.SetGlobalFloat(ShaderIds.FoliageV, FoliageV);`.
REQUEST QA (Tools/qa/lookv2_check.py, check 11, after the lead's ruling): beat-5 windows: `BG_MIN_BEAT5 = -5` (proposed, ruling
asked) and `RG_WIN_BEAT5 = (-8, 4)` (D-040 (5)); in the dark-cast block: `bg_lo = BG_MIN_BEAT5 if beat == 5 else -2`,
`C["dark_cast_BG"] = (round(bg, 2), bool(bg_lo <= bg <= 6))`, `ok_rg = (RG_WIN_BEAT5[0] <= rg <= RG_WIN_BEAT5[1]) if beat == 5 else
abs(rg) <= (RG_MAX_BEAT1 if beat == 1 else 4)`, `C["dark_cast_RG"] = (round(rg, 2), bool(ok_rg))`.
DONE GD (TR's C31 "GD lets the near lantern cores reach >= 200"): the near cores peak at 0.35 + 1.3 = 1.65 x the disc mean (was
1.45); KS14 p99 107.9 (p4_01) -> 136.9-141.4 in this copy (with TR's fix-wave gate light in the same builds; lookv2_check KS14 PASS
on r08 / r09). KS14 blobs > 200 (3-12 px) 199 -> 26-36: the 199 were mostly far 3-4 px lanterns whose hot profile peaked over 200,
i.e. the sparkle of C32; flat-shaded far lanterns keep their energy at a lower peak.
- C33 hue in the critic's KS15 crown rects (green-dominant pixels, hue median / sat): (150-600 x 150-500) 139 / 0.37 -> 97 / 0.34,
  (1250-1700 x 150-450) 140 / 0.37 -> 97 / 0.34, (700-1000 x 500-800) 150 / 0.38 -> 103 / 0.27 (p4_01 -> r09). Limbs: crop
  1480-1900 x 20-360 of KS15 at 2x, p4_01 vs r09 (QA/captures/GD_r09/_crops/bark_compare.png): black tubes -> brown bark with its
  grain, lit where the strands hang; the trunk columns (NocSurface, lib.Bark) stay dark under the crowns (no lantern there).
- 03:19 GD-final (full EditMode suite on the final code of 03:02: crown globals, reverted rim, the wet-source test fix),
  QA/reports/GD-final.xml: 476 tests, 475 passed, 0 failed, 1 skipped (exit 0, 819 s). In the log: the lantern field on the cards
  down-facing mean 15.2 / up 0.5; visibility audit without / with lanterns 6 / 6 failures of the checked kinds (the lanterns add none).
- Tools/qa/image_metrics.py on GD_r09 (the official tool): NaN-like bright KS14 7 (p4_01 21), KS15 7 (p4_01 13), look_up60 11, start 1,
  mid 1, end 2, look_l 2, look_r 5, look_back 20 (bridge-view lamps); isolated > 8x median KS14 3, KS15 2.
- r11 = FINAL (GD-b14 03:27-03:29, 0 errors; Builds\GD; 9 stills, failures 0, alloc 0 B): KS15 mean 32.4, green share (G > R, G > B,
  luma > 18) 0.534, green hue median 95.3, sat 0.34, p99 124.1, blobs > 200 (3-12 px) 76, lookv2_check: all pass but dark_cast_BG
  -3.2 (R - G passes); KS14 mean 33.7, p99 144.4, blobs 26, lookv2_check KS14 PASS. NaN-like bright KS15 7, KS14 7 (5 lantern +
  1 window + 1 leaf pixel): the other-crown fade changed none of the 7 KS15 peaks (identical positions and values from r08 on).
  Crops at 4x (scratch iso_r11): single orange pixels beside the limbs and trunk edges and between low cards: far lanterns at the
  2.2 px floor clipped to 1 px by an opaque edge in front (limb, trunk, the lowest cards), not crown-fringe peeks. The fades cannot
  see limbs / trunks; left open (C32-a). Looked at KS15 full frame: yellow-green lantern-lit canopy, strands with warm pools,
  brown bark limbs; KS14 (r09 / r11 same picture).

## Open at the end (fix wave)
- C32-a S3: 7 single-pixel lantern slivers in KS15 (5 in KS14): far lanterns at the 2.2 px floor clipped to 1 px by an opaque edge in
  front (limb, trunk, the lowest cards); the vertex fades model only the crowns. Next: lanterns as camera-facing billboards with
  analytic AA coverage (a NocLantern point sprite in Shaders/Beats, the critic's option; 14.5.5 says icosahedra, so it needs a ruling)
  or a depth-aware dilation of the lantern layer.
- C32-b S3: KS15 texpan shimmer at the dense near clusters (block +5.3 over the -capture-ss 2 reference; lantern-mask mean +2.07 on
  b11); a soft rim made it worse (r10, reverted). Same next step as C32-a.
- C33 B - G: KS15 dark cast B - G -3.2 (check 11 asks >= -2; p4_01 -1.9): ruling asked (proposed beat-5 window [-5, +6], arithmetic
  in decisions); R - G -2.40 meets |R - G| <= 4 without D-040 (5)'s window.
- Not re-checked this wave: 16:10 / 21:9 / 4K KS15 (GD_ks11, below), statics (no temporal change in my files except the fade,
  which is constant for a static camera), the -foliage-v flag (REQUEST FX), GD-10 ground / path slabs, GD-13 field load time, drips.

## Proposed decisions (for DEVELOPMENT.md)
1. C32 lanterns / fireflies: the 2.2 px floor holds for the inscribed sphere (k = max(1, 0.5 x 2.2 x fp / (0.795 r)), emission x 1 / k^2);
   the lantern profile is flat under 1.5 px drawn and 0.35 + 1.3 (n.v)^2 from 4.5 px (disc means 1 and 0.35 + 1.3 / 2 = 1: energy
   kept). MSAA 4x sim (QA/GD_lantern_sim.py): the old hot 0.55 + 0.9 on a 2.2 px disc leads its best neighbour by 115 codes (> 77 =
   NaN-like), 1.75 px by 160; flat 2.2 px 55-69, flat >= 2.6 px <= 31, peak std 0 (no twinkle).
2. C32 occlusion: a tree lantern whose eye ray passes its own crown deeper than the lantern, or grazes any crown's leaf fringe
   (q < 1.08-1.18 in that crown's ellipsoid units) 1-2.4 m before reaching it, fades out and collapses (uv1 crown + global
   _NocGardenCrowns[34] set at build; 17 x 12 x 8 840 = 1.8 M short tests a frame on the vertex stage).
3. C33 canopy light (amends D-039 (14)): under-glow lerp(LeafGlow, LeafWarm, 0.65) at LeafGlow's luminance x 0.035 (was LeafGlow x
   0.045), lantern field gain 1.6 (was 1.2), scattered-light chroma power from 2.5 deep in the crown to 1 at E >= 1.6 (next to a
   strand): KS15 green hue median 137.6 -> 95.3 (LOOK_V2 Leaf 93), R - G -4.76 -> -2.40, mean 32.1 -> 32.4, green share 0.540 -> 0.534,
   near-strand leaves median 42.7 / p90 61 (14.5.5's 40-70).
4. C33 limbs: NocFoliage mode 5, the CC0 trident maple bark of lib.Bark on the limbs' UV0 / 1.8 m with a derivative tangent frame,
   lit by the lantern field as direct light (albedo x E x 1.6 x lerp(Lantern, WinNeutral, 0.4)); the trunks stay lib.Bark (NocSurface).
5. Ruling asked: beat-5 dark-cast B - G window [-5, +6] on check 11 (with D-040 (5)'s R - G [-8, +4]). Arithmetic: check 11 averages
   luma 12-40, which in KS15 is mostly lit leaves (luma 24-40: 29.7 % of the frame, 76-92 % green-dominant); for any hue < 120,
   B - G = -(G - min) = -chroma, so a hue-93 leaf at sat 0.34 gives B - G -6 at luma 24-32 and -12 at 32-40; frame mean -3.2. The mint
   canopy of p4_01 measured -1.9 (critic -2.23) and failed too; |B - G| <= 2 would need sat <= ~0.2 on the lit leaves (a grey garden).
- 03:33:44 machine event: System log Resource-Exhaustion-Detector 2004 "low virtual memory" (commit limit 69.5 GB = 64 GB RAM + a
  ~4 GB page file; three Unity.exe at 2.8-2.9 GB each named), 03:33:47 Application Popup 26: AL's Nocturne.exe failed to start
  (0xc000012d, commit limit). From 03:33:49 every running player stalled (Logs/gpu_watch.csv: GPU util 0 %, 9-15 W; player logs of
  GD-smoke, QA-smoke, AU-smoke, AT-smoke all stop at 03:32-03:33; QA 2836 and AT 24976 "not responding", AL 1908 stuck at start).
  My GD-smoke (27788) hung at t 40 s, s 955.7 (no failure logged before the stall); I stopped my own chain and player at 03:39
  (others' processes untouched) and re-queued the smoke + KS15 keyshots (GD-smoke2, GD-ks11; run-player waits for a slot).
- 03:43 smoke on GD-b14 (GD-smoke2: -autotest -brisk -autotest-until 1080 -beat 5): AUTOTEST PASS, beats 2, errors 0, alloc 29.1 B/frame
  over 4 504 steady frames (engine input box 32, excess 0), QA/reports/autotest_brisk_from880_until1080.json.
- GD-final garden cases (all passed): Beat05GardenTests 9 (incl. Lights_WetSources_Foliage with the beat-4-independent count),
  FoliageTests 8, GardenLanternTests 7 (new: Lanterns_CarryTheirCrown_ForTheFringeFade, Limbs_AreBark_LitByTheField; updated
  Shader_LanternContract), FoliageVisibilityTests.Lanterns_AddNoVisibilityFinding, MaterialTests.Foliage 2.
- Note: Assets/_Game/Materials/LeafCore.mat / Leaves.mat show _LanternLightGain 0.6 -> 1.2 and _UnderGlow 0.02 -> 0.045 in git status:
  the shader defaults of caddb90 written by a RebuildAll / import, not an edit of mine; nothing reads them at run time
  (GardenMaterials derives runtime copies and writes every Phase 4 value, SetPhase4).
- 16:10 estimate while the 4K slot waits (centre crop 1728 x 1080 of r11 KS15 = the 16:10 frame at the same vertical FOV): B - G -3.64,
  R - G -2.68 (|R - G| <= 4 holds; p4_01 16:10 was -5.3 per the critic).
- 03:48 KS15 keyshots on GD-b14 (QA/captures/GD_ks11: 1080, 16:10, 21:9, 4K; my one 4K capture; waited ~10 min for a 4K slot):
  | variant | mean | green share | hue | B - G | R - G | p99 | blobs | NaN-like bright |
  |---|---|---|---|---|---|---|---|---|
  | 16:9 | 32.4 | 0.533 | 95.8 | -3.20 | -2.41 | 124.1 | 76 | 7 (p4_01 13) |
  | 16:10 | 33.0 | 0.575 | 96.1 | -3.65 | -2.70 | 121.2 | 71 | 9 (17) |
  | 21:9 | 30.1 | 0.448 | 95.5 | -2.20 | -2.00 | 110.7 | 71 | 7 (15) |
  | 4K | 32.6 | 0.518 | 94.7 | -3.12 | -2.01 | 115.0 | 323 | 10 (20) |
  |R - G| <= 4 at every aspect (p4_01: -4.76 / -5.3 / -3.87 / -4.14). REGRESSION found: KS15 p99 (LOOK_V2 check 10, >= 120) falls
  under 120 at 21:9 (110.7; p4_01 120.4) and 4K (115.0; p4_01 139.4); 16:9 124.1 and 16:10 121.2 still pass (p4_01 138.9 / 135.5).
  Cause: the sharper core 0.35 + 1.3 (n.v)^2 (same disc mean, less area above 0.6-0.8 of the mean: 81 % / 65 % of the disc against
  94 % / 72 % for 0.55 + 0.9), plus the faded hidden lanterns. Not changed this late (a 4K re-check needs a 4K slot): proposed fix
  0.45 + 1.1 (n.v)^2 (centre 1.55), to be verified on KS15 blobs (>= 25) and p99 at 4K / 21:9 (C32-c below).
  Looked at the 4K crop 2400-3400 x 200-1000 (QA/captures/GD_ks11/_crops/ks15_4k_crop.png): yellow-green leaves with their texture,
  bark grain on the limbs, a modest warm spill on the leaves beside the strands (warmer, a few codes brighter; not a strong halo); the
  near lanterns at 12-16 px show the icosahedron's polygon outline (C32-d).
- C32-c S3 (new, mine): KS15 p99 110.7 (21:9) / 115.0 (4K) < 120 (see above). C32-d S3: near lanterns at 4K read as 12-16 px polygons
  (14.5.5's icosahedra; the post lanterns are UV spheres since GD-07): a smooth sphere mesh or the billboard of C32-a fixes both.
- Disk: final folders QA/captures/GD_r11 (26 MB, 9 stills + contact sheet), QA/captures/GD_ks11 (22 MB), QA/captures/GD_r09/_crops
  (1.2 MB); Builds\GD (464 MB). All sequence frames deleted.
GD DONE 2026-11-21 03:53
