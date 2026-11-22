# HT log - beat 7 Heart Square, money shot 3 (Phase 4 wave A, D-033; MASTER_PROMPT 14.5.7)

Owner HT. Files: `Beats/Beat07_Heart/**` (KoiMath.cs included, D-032 2; new files there); `Shaders/Beats/NocKoi.shader`;
`Beats/Relief/NotchEastBlock.cs`, `Beats/Relief/PromenadeLintel.cs`; `Scripts/Runtime/QA/NocVisibility.cs` and every
`Scripts/Runtime/QA/Visibility*.cs`; `Editor/MaterialFactory.Heart.cs`; new `Runtime/Core/MaterialLibrary.Heart.cs`; new
`Tests/EditMode/MaterialTests.Heart.cs`; `Tests/EditMode/KoiMathTests.cs`, `KoiShaderTests.cs`, `VisibilityAuditTests.cs`,
`VisibilityWorld.cs`, `Tests/EditMode/Beats/Beat07HeartTests.cs` and new test files; this log; `QA/critique-HT.md`.
Requests to HT: write "REQUEST HT: <file>: <change>" in your own log; answers appear here as "DONE HT <ID> <request>".

## Resume rule
Read this log, then `git diff -- <HT files>`; continue from the last step marked GREEN / committed. Never redo committed work.

## 2026-11-16

### 08:17 start
- Read: BUILDER_BRIEF; DEVELOPMENT D-001..D-033 (D-009 koi, D-029 KS19 subjects, D-032 2 koi outline approved); MASTER_PROMPT 14.0,
  14.2, 14.4, 14.5.7, 14.6, 14.7 H1-H8, 14.8, 14.10, 14.11, 14.12; QA/critique-lookv2-01 (INT-08, INT-10, INT-12, INT-31, CRIT0 #9,
  #21); KO log + critique-KO (KO-12 / KO-13 open: crescent outline, pectorals in a near-vertical plane).
- Looked at (full size): lookv2_01 KS17, KS18, KS19; lookv2_01b beat07_heart_mid, beat07_heart_look_up60.
  - KS17: the koi is a cyan banana arcing behind the spire; the plaza (lower 40 %) is a featureless dark slab with lamp posts as
    sticks; the spire is a dark cylinder with 12 cyan lines and 4 ring lines (a lined box, CRIT0 #21); the right third is the
    striped back of the screen wall / arcade (SG).
  - KS18 / look_up60: from below the koi is a crescent / "C" wrapped 200 deg around the spire; pectorals invisible, the veil
    edge-on; the body a uniform tube.
  - KS19: the goal window is a pale flat square, the curtain a stone slab, no frame or sash, no sill; the other tenement windows
    are flat black rectangles; the balcony slab and railing are fine; a bright white lamp pool on the brick at the lower left.

### 08:30-09:56 INT-10 the koi's outline (first start of wave A; reconstructed after the usage-limit stop)
- Offline prototype (scratch ht/koi_proto.py, render.py: the koi's sample set rendered as KS17 / KS18 / look_up60 silhouettes;
  opt.py / opt2.py / refine.py: a penalty search over the shape parameters with every clearance, the 356 m envelope, the KS17 /
  KS18 framing margins of 20 wave phases and the body-low rule as constraints). Result (r22.log, pen 0): turning over the head and
  trunk 64.2 deg (orbit 88.1), the bend in the rear half, the tail flicking out; clearances spire 110.1, wall 109.8, arcade 105.4 m,
  rmax 347.6 m (<= 356), top 399.9, body low 164.8 m; KS17 margin 0.389, KS18 0.121.
- KoiMath.cs (D-032 2): the body follows a 900.0 m REFERENCE SPINE (plane curve in the head's co-rotating frame, RK4 at load,
  tables every 0.5 m, cubic Hermite with exact derivatives), curvature KFront 0.00185 -> KRear 0.00420 (smoothstep a 200.7-276.1)
  minus a tail flick 0.00167 from a 853.4; crab 19.27 deg inward at the head; the orbit's ellipticity fades over 110 m behind the
  head; a PCHIP drop 0 / -11.88 / -40.25 / -20.07 m at a 0 / 300 / 600 / 900. The head stays on the orbit and keeps the azimuth
  table. Veil 140 x 140 m rolled 30 deg toward N (reads from below), notch at 2/3. Pectorals splayed: root 50 deg below the side
  axis, direction (F, N, B) = (-0.55, 0.50, -0.60).
- KoiShaderMath.cs / NocKoi.shader: the spine from per-vertex attributes uv3 = (r_ref, alpha, dr_ref/da, dalpha/da), uv4 = (Dy,
  dDy/da) instead of the Newton orbit inversion; veil axis rolled. KoiBuilder.cs writes uv3 / uv4 after ToMesh.
- KoiMathTests.cs: L = length of the reference spine (polyline over 90 000 pieces), head on the orbit, new
  OutlineReadsAsAFish_StraightTrunkBendBehind, veil rolled, body lowest y >= 160 (was 192, the orbit's; arcade 38 + 100 = 138).
- Started task 2 (scratch NocGoalRoom.shader prototype). Nothing was committed before the stop.

### 10:10 second start
- fastcheck OK (runtime 196 / editor 260). Offline fxc (scratch ht/fxc/shcheck.py, vs_5_0 + ps_5_0, /Gec): NocKoi.shader OK,
  0 errors, 0 warnings. Committing the task-1 state now (RESTART NOTE), Unity tests next.

### 10:15-10:50 INT-10 Unity tests, KS19 body, shader frame at the head
- HT-t01 (KoiMathTests | KoiShaderTests | VisibilityAuditTests | Beat07HeartTests): 35/38. Failures:
  1. KS19 koi body: 0 core samples visible inside the 3 % margin (D-029 needs >= 3; the orbit koi had 8 in my offline model).
     Cause: the drop of -40 m at a 600 sank the stations a 675-832 (the only ones KS19 can see, above the tenement's south
     corner) behind the tenement. Offline (scratch ht/ks19.py with the tenement, neighbours and back-row boxes; it reproduces 0
     for the committed shape and 8 for the orbit): a grid over the drop knots, clamped end slopes.
  2. KoiShader_MatchesKoiMath body 6.97 cm (gate 5 cm; was 0.03): the drop's PCHIP end slope at a = 0 (-0.012) against its held
     value for a < 0: KoiMath's central difference saw half the slope, the shader the full slope (7 cm on the head ring).
- Fix (KoiMath.cs): DropY = 0 / -8 / +4 / -20 m at a 0 / 300 / 600 / 900 with zero end slopes (PchipClamped): level at the head
  and at the veil, a gentle vertical S; KoiMathTests asserts the C1 ends. Offline: KS19 body 6, KS17 margin 0.389, KS18 0.117,
  clearances spire 110.1 / wall 109.7 / arcade 106.1, rmax 347.7, top 401.1, body low 184.2.
- HT-t02: 38/38. KS19 body 5 samples (>= 3); KS17 min margin 0.378 (16:9 follower), KS18 0.132 (16:10); shader vs KoiMath body
  0.15 cm, veil 0.02 cm; clearances body spire 155.8, wall 115.9, towers R456 115.9, arcade 182.8, top 401.5, low 184.1;
  per-frame spine 0.410 m (gate 0.45); outline turning a 0-400 64.2 deg (orbit 88.1), a 400-900 118.1 deg.

### 10:50-11:25 INT-08 the goal window (first pass, code)
- Looked at: lookv2_01 KS19 (full) and lookv2_01_4k KS19 crop (1540-2140, 1000-1560): the "room" is the back wall's pale
  marbled plaster glow seen flat through a frameless hole; the curtain (Fabric row with NocPal.Plaster albedo, glow 0.42, and
  the outdoor goal light i 5 only 0.85 m in front of it) is a blown cream slab; the other windows are flat black panes 3 cm proud.
- KS19 geometry through the window (eye (1148.18, 1.7, 387.88), 24 deg up): the back wall is seen only at y 5.64-7.0, z 387.0-388.8;
  the ceiling from x 1158.05 to the back wall; the side walls not at all. The ray to the lamp crosses the frame plane at
  y 4.49-4.70 (lamp bottom / top).
- OldQuarterBuilder.cs (rewritten; HeartLayout.cs new constants):
  - goal window: box-frame lining 0.06 m, upper sash with meeting rail (top y 5.17) and one glazing bar, the lower sash raised
    0.34 m (its bottom rail 4.775-4.82 m, above the lamp ray 4.70), outer stone sill (0.06 x 0.12 proud), stone lintel; all named
    goalWindow* (VisibilitySubjects self tags of the window and the lamp);
  - curtain: a pleated sheet (pitch 0.11 m, pleat amplitude 0.028 at the leading edge -> 0.05 at the gathered side, +30 % to the
    hem), deep rust albedo (0.16, 0.075, 0.035) on both v2 tokens, glow 0.30 (was 0.42), on a rod with finials at y 6.0;
  - room: ceiling split from the side walls (part 3), base glow lowered (back 0.30, side 0.18, floor 0.08, ceiling 0.10) and the
    lamp's own point light "goalRoomLight" (GoalLamp, r 5.5, i 4, at the globe + 0.15 m) for the falloff; back-wall props in
    KS19's view: a framed picture (dim canvas), a shelf on brackets with 10 books (one leaning) and a box, a cornice;
  - the tenement's west face is cut for 20 openings (18 dark windows, the goal, the door): brick reveals 0.18 m, dark glass at
    the back, frame lining, mullion, transom, stone sills and lintels; two windows glow faintly cold behind drawn curtains
    (WinCold x 0.05); the neighbours' proud panes get frames, mullions and sills;
  - entrance: a recessed door (1.1 x 2.3 m, the ground-floor window of column +1 gives way) with a stone step, and a wall lantern
    (arm, plate, cap, base, frosted glass WinCold/WinNeutral x 1.4) at the entrance light, which so far lit a bare wall;
  - FX's optional REQUEST HT (SurfV == 2 always true): the SurfV tests in OldQuarterBuilder are gone.
- Beat07HeartTests.cs: GoalWindow_JoineryKeepsTheLampInView_CurtainIsPleated (KS19 rays to the lamp's disc vs joinery + sill,
  >= 60 % of the opening open, pleat depth and normal spread), Tenement_OpeningsCutTheFace (18 + 1 + 1 openings, the west face's
  area = facade - openings, reveal quad count), the furniture / lane test covers every new mesh, lights 20 + 3.
- Lights in beat 7: 20 plaza + goal + room + entrance = 23 (<= 32).
DONE HT FX REQUEST (optional SurfV cleanup in OldQuarterBuilder.cs).

## 2026-11-19 third start (run wf_8e439303-949)

### 17:50 resume
- Read this log, the RESTART NOTE (GPU care: CompileCheck / EditMode / offline first, -capture-only at 1080, one player) and
  docs/DREAD.md (HT items for wave B: 5.2 the koi's eye pupil follows the camera <= 6 deg/s; 6 rooms near the path with a chair,
  a table, a cup and a lamp, nobody; the goal window stays the only warm window). Nothing built now blocks them.
- git diff: HeartLayout.cs, OldQuarterBuilder.cs, Beat07HeartTests.cs, this log (INT-08 first pass, 10:50-11:25) back in the tree.
- gpu-guard 17:56: 42 C, util 3 %, VRAM free 9 908 MiB.
- fastcheck OK (runtime 198 / editor 263; 297 s wall, the machine is busy). Committing the INT-08 first pass (code only,
  no shader change); Unity tests next.

### 18:15-20:05 (third start; reconstructed on the 20th from the tree and scratchpad ht/edit_*.py: the pause cut the log)
- HT-t03 (18:14; KoiMathTests | KoiShaderTests | VisibilityAuditTests | Beat07HeartTests): 39/40. Failure:
  Furniture_StaysOutOfTheLane_AndMeshesValidate, "tenement: 20 degenerate triangles": the goal window's constants (387.3, 4.4)
  and the column / storey arithmetic (387.9 - 0.6, 3.6 + 0.8) differ in the last bits, so FaceWithHoles' grid had sub-mm
  slivers. Fix in the tree: grid lines closer than 1 mm merge (OldQuarterBuilder.Merged). t03 numbers: lamp rays blocked 0/29,
  opening 61.1 % open from the KS19 eye (280/720 rays on the joinery), curtain depth 0.128 m (2 244 triangles), KS19 window
  135.2 x 151.2 px at 1080p, goal lamp hue 32.2, west face 240.47 m2 = 277.20 - 36.73 (18 windows, goal, door), joinery 10 824
  triangles, Beat07 built 52 504 triangles (budget 560 k), koi 11 000 triangles, shader vs KoiMath body 0.15 cm, veil 0.02 cm.
- INT-12 plaza (code, Unity tests pending):
  - Shaders/Beats/NocInlay.shader (new, + .meta): the brass inlay as a transparent lit overlay (queue 2501, ZWrite Off, ZTest
    LEqual, Blend One OneMinusSrcAlpha, the lit template's NOC_TRANSPARENT premultiplied output) drawn on the slab's own top fan:
    17 ring lines 0.12 m every 20 m (R 60 - 380), 48 radial seams 0.06 m every 7.5 deg beyond R 58 (one runs 0.12 m beside the
    east leg: the walk's leading line to the spire), exact box coverage along each line's normal (sub-pixel lines keep their
    energy), each family faded below 6 px pitch (full above 10 px); aged brass F0 (0.50, 0.42, 0.30), metallic, smoothness 0.62.
    No keyword, no sampler; the template's own pragmas only.
  - HeartFloorBuilder.cs: TopFan (shared by the slab and the overlay: identical vertices and triangles), InlayFan,
    InlayCoverage (C# twin), the heartFloorInlay draw.
  - PlazaFurnitureBuilder.cs: the 20 lamps are fixtures (lathe: cast plinth, tapered pole, collar; a dark disc housing R 0.42
    over a SignWhite x 2.0 lens R 0.315 facing down; the light 0.35 m under the lens; halo and H4 source at the lens); ten
    granite benches with slatted wooden seats between the lamp columns, 14.8 m off the east leg (2.5 m outside the lane).
- Spire (CRIT0 #21, code, Unity tests pending): SpireBuilder.cs: GE's five 92 m belts become 11 ring girders (hoops) every 40 m
  (y 40 - 440, R(y) - 0.3 .. R(y) + 3.6) carrying the 14.5.7 ring lights (one NocLine material per ring); the 12 fins flare into
  buttresses below 96 m (outer edge R(y) + 13.5 at y 6, landing on the R 48 drum, smoothstep to R(y) + 4 at 96 m), strips follow
  the edge down to the drum; a podium of three granite steps R 56 / 53 / 50.5 x 0.16 m. SpireDriver.cs (new, + .meta): the ring
  lights breathe 0.55 .. 1.0 at 0.1 Hz, ring k lagging 0.5 k rad (a slow pulse rising up the spire; the 11 rings' sum stays
  within +-4.1 % of its mean; steepest change 0.141 of the peak per second; scene clock, so captures are deterministic; no
  allocation). Everything above 96 m stays inside the 14.5.7 envelope R(y) + 4 (koi clearances and the KS17 / KS18 lint
  unchanged).
- DREAD 5.2 hook (wave B): NocKoi _KoiPupil (outer / inner eye pupil offsets, 0 = centred: no change in wave A).
- Beat07HeartTests.cs: PlazaInlay_SameTopFan_RingsSeamsFade, Spire_HoopsRingLightsFlarePodium; the furniture test covers the
  fixtures, lenses and benches and checks the 6 m lamp height; the triangle sum counts the inlay fan, podium, hoops, ring lights
  and the flared fins / strips.
GLOBAL 19:35 (third start, logged on the 20th): the spire (visible from beats 5-7: KS14-KS19 and the beat-6 / beat-7 stills)
  has 11 breathing ring lights every 40 m instead of 5 steady ones at 92 m, flared fin buttresses at its base and a podium.

## 2026-11-20 fourth start (run wf_ab1bc94d-acd)

### 10:10 resume
- Read this log, the RESTART NOTE (fourth start: Unity cannot start reliably today, ILPP stall guard exit 96; work Unity-free
  first, batch Unity runs, retry at most every 20 min) and my commits since 34e5d92 (f4a016c). git diff: the third start's
  uncommitted work above is back in the tree (patch 19c). HeartLayout.cs had lost its CRLF endings in the patch round trip:
  restored (the diff is now the 44 added lines only).
- The lookdev_SG / _FX / _AB builds of the 19th (19:18 - 19:55, exit 0) compiled this tree's NocInlay and SpireDriver: FX_r03
  (19:47) shows them. Looked at FX_r03 KS17, KS18, KS19 (full size) and crops:
  - KS17: the koi now reads as a fish (head, taper, splayed pectorals, the tail running behind the arcade); the spire shows the
    11 ring lights and the fin flare at its foot, but with the cyan fin strips it reads as a cyan wire cage; the plaza (lower
    quarter) is still a near-uniform dark floor: the brass lines show only in a contrast stretch (the leading seam, two rings
    near the camera), because metallic brass mirrors the same dark sky as the wet stone; the distant lamp lenses (flat discs
    seen edge-on) are 1-2 px slivers: image_metrics NaN-like 7 (653,756), (756,780), (771,784), (952,788) are lamp lenses.
  - KS18: the S-bend, head and pectorals read; the veil fans at the right; no crescent any more. Body median luma 173.6
    (KO 130-175), clipped 0 %.
  - KS19: frames, sashes, sills and lintels everywhere, warm room with the lamp, the plant and railing read; the door is a
    flat grey leaf under the lantern, the lantern a white box, the right half of the wall is dim and plain.
- lookv2_check on FX_r03: KS17 PASS (mean 38.5, sky top 47.4, separation 11.4), KS18 PASS (mean 50.0), KS19 FAIL only
  texture_1_meso 0.95 (>= 1.0; right wall region; lookv2_01 had 0.62 and a p999 fail), warm inside 14.2 %, outside 0.029 %,
  koi warm px 0, p999 211.8. image_metrics: KS19 NaN-like 14 = dark brick pits around the goal window (INT-31, FX's NocSurface).
- Offline fxc (scratchpad ht/shcheck.py): NocInlay 6/6, NocKoi 4/4 OK, 0 warnings.

## 2026-11-20 fifth start (evening)

### 20:24 resume
- Read this log, my commits (8f9e813, 7b0dbaa, f4a016c), git show --stat 2fe0da9 and QA/critique-HT.md. No open REQUEST HT in any
  copy's logs (FX's optional SurfV cleanup was DONE on the 16th).
- Found: the fourth start's round 2 (HT-03 downpipe, HT-04 inlay mirror, HT-05 lamp bowl, HT-08 lantern cage), marked "in tree"
  in critique-HT, is NOT in patch d / 2fe0da9 (only the critique row is): its edit script (4th-start scratchpad ht/edit_r2.py)
  had targeted the main project. Re-applied it to this copy (all replacements matched once).
- The heart copy had no Assembly-CSharp(-Editor).csproj (fastcheck exit 2): copied the generated, git-ignored pair from blvd.
- fastcheck: Beat07HeartTests.cs(562) Assert.GreaterOrEqual(x, 0.0, 1e-3) has no such overload: fixed. NOTE: SG-b01 (20:30-20:35)
  compiled the tree in the 2 minutes between the round-2 write and this fix and failed on that line; SG-b02 (20:45) has the fix.
- fastcheck runtime 199 / editor 265 OK; offline fxc (ps/vs 5.0, base / instancing / additional lights): NocInlay 6/6, NocKoi 4/4.
- HT-06 (spire wire cage), approach 1: ring lights NeonCyan x 0.6 (14.5.7 colour; was the strips' cyan-white x 1.5): the 11 rings
  become the subordinate accent, the 12 verticals carry the spire (HeartLayout.RingLightGain, SpireBuilder ringBase).
GLOBAL 20:55: the spire's 11 ring lights are NeonCyan x 0.6 (were cyan-white x 1.5); shows wherever the spire is seen (beats 5-7,
  KS14-KS19 and the beat-6 / beat-7 stills).

### 20:58-21:05 HT-t04 (KoiMathTests | KoiShaderTests | VisibilityAuditTests | Beat07HeartTests): 41/43 - the third / fourth
  start's Heart work through Unity for the first time
- Failures (both fixed in the tree, fastcheck OK):
  1. PlazaInlay_SameTopFan_RingsSeamsFade: fan.VertexCount > 1000 was 962 (the outline's 961 points + the centre; a count, not a
     picture threshold): > 900.
  2. Tenement_DownpipeClearOfOpeningsAndBelowTheRoof: the hopper's top at 14.15 m stood 0.32 deg above the roof edge from the
     KS19 eye (58.70 vs 58.38 deg: it is 0.25 m proud of the face, so nearer the eye): DownpipeTop 13.85 (58.07 deg).
- Evidence (QA/reports/HT-t04.xml): koi KS17 min margin 0.378 (16:9 / 16:10 / 21:9 follower; spec 0.085), KS18 0.132 (16:10),
  head occluded 0, body 54-228 px at 4K; outline turning a 0-400 64.2 deg, 400-900 118.1 deg; shader vs KoiMath body 0.15 cm,
  veil 0.02 cm; koi 11 000 triangles; Beat07 built 77 181 triangles (budget 560 k), 23 real lights (<= 32); KS19 window 135.2 x
  151.2 px at 1080p, lamp rays blocked 0/29, opening 61.1 % open, goal lamp hue 32.2 deg; ring lights sum +-4.07 %, steepest
  0.141 /s; downpipe clear of every opening.
- New (21:00, after t04 compiled): SpireBuilder.AddStripSources - the wet mirror of the spire (INT-12): the strips of fins 4-8
  (120-240 deg, facing the walk) as H4 segment sources, one chord per fin from the drum to y 200, three copies at s 1 300 /
  1 392 / 1 484 whose window crossfades sum to exactly 1 (steady from s 1 240 to 1 504, at most 10 sources). Arithmetic: KS17's
  floor mirrors elevations 0-15 deg (pitch 18 - vFOV/2 33 = -15 deg at the frame bottom, 6.3 m out), i.e. the spire up to 100 m at
  380 m; the koi (y >= 184 m) is 25-66 deg up from KS17's eye and from KS18's floor-less frame, so its mirror is never in a money
  shot (no koi H4 request to FX needed). Test SpireStrips_MirrorSources_SteadyAcrossThePlaza.

### 21:14-21:30 HT-b01 (0 errors, 13.3 min, Builds\HT) + HT_r01 capture (KS17, KS18, KS19, 7 beat-7 stills; 0 failures)
- Looked at (full size + crops in my scratchpad ht/r01_*.png):
  - KS17: the ring lights now sit under the strips (deeper cyan, dimmer): the verticals lead, the cage reads less; the plaza floor
    now carries the spire strips' reflection streaks (five cyan streaks from the spire base toward the camera), the lamp streaks and
    the leading seam; the lamp bowls read as lit fixtures (no edge-on slivers). The brass rings stay invisible at eye height
    (physics: at 3.7 deg depression water's Fresnel 0.73 equals the brass's; a 0.12 m ring at 26 m covers 0.29 px).
  - KS18: koi fish outline, splayed pectorals and veil read; the spire hierarchy as in KS17.
  - KS19: the downpipe (pier between the south columns) breaks the plain right half; the lantern has its cage; the door was still a
    flat grey slab (lib.Wood under the cold lantern) -> HT-10 below; the room's upper back wall a flat pale panel -> room glow.
- lookv2_check (HT_r01): KS17 PASS (mean 38.8, separation 11.2, goal point 38 px, warm stray 0.0 %, koi patch hue 14.8), KS18 PASS
  (mean 49.9, separation 15.4), KS19 PASS (mean 30.9, texture region 0 meso 5.03, region 1 meso 1.20 (was 0.95 in FX_r03: HT-03
  fixed), koi warm px 0). image_metrics (QA/reports/image_metrics_HT_r01.md): KS17 NaN-like 1 (was 7: the four lamp-lens slivers
  are gone, HT-05 fixed; the one left (1467, 643) is SG's arcade), isolated 2 (SG's arcade lights, as in FX_r03); KS18 isolated 51
  on the spire strips' 1.75 px NocLine edges (FX_r03: 61; the neon mask misses thin cyan lines; NocLine / image_metrics are FX's);
  KS19 NaN-like 11 dark brick pits (FX's INT-31, was 14).
- HT-10 (21:27): the door is a dark painted leaf (SteelPainted) with a bottom rail, a centre muntin, four raised fielded panels, a
  lever handle and back plate, a transom bar (OldQuarterBuilder.TenementTrim's new optional `door` builder; the test adds it).
- 21:29: goal room base glow back 0.30 -> 0.20, ceiling 0.10 -> 0.07 (the room light carries the gradient).

### 21:24-21:40 HT-t05, koi sequences
- HT-t05 (KoiMathTests | KoiShaderTests | VisibilityAuditTests | Beat07HeartTests | WetFloorTests; compiled before the door edit):
  52/53. All HT tests pass (incl. SpireStrips_MirrorSources_SteadyAcrossThePlaza: weight 1.000000..1.000000 over s 1 240-1 500,
  at most 10 sources; downpipe hopper 58.08 deg < roof 58.38 deg). The one failure is FX's: WetFloorTests.
  WallWindows_MeanOne_Bounded_StreakVertically, "expected >= 0, was -1.435e-11" (NocMirrorMath floating round-off below zero; FX's
  file, WIP 2fe0da9). Not an HT file.
- Koi sequences (HT_koi01, Builds\HT of HT-b01, -capture-set koi -seq-res 960x540: 2 x 720 frames at 60 fps, 12 s at 2.5 m/s
  through KS17 and KS18): sequences.py PASS (QA/reports/sequences_HT_koi01.md): koi_ks17 luma step 0.041 / frame (gate 1.2), 0
  spikes; koi_ks18 0.051, 0 spikes. Looked at frame 600 of ks17. Frames deleted (report kept).
- HT-b02 queued (door, room glow, downpipe 13.85).
- 14.5.7 acceptance (4), beat 7 sequences (HT_seq01, HT-b01 build, -capture-set sequences -capture-only beat07_heart,seq_b67
  -seq-res 960x540 -freeze-clock -rain 0): sequences.py PASS (QA/reports/sequences_HT_seq01.md): static_beat07_heart 120 frames
  change 0.00 levels (gate 2); seq_b67 (s 1 260 boundary, 601 frames at 30 fps) luma step 0.111 / frame (gate 1.2), activation
  diff 0.015 (gate 0.5), 0 spikes; pan_beat07_heart step 0.012. KS17 clipped 0.0000 % of the frame (image_metrics_HT_r01; koi
  clip gate 0.6 %). Frames deleted.
  Note: -capture-set "statics,boundaries" is refused by the player (ShotPlan.GuardNote splits the list, run-player's heavy check
  does not); "sequences" with -capture-only works.

### 21:40-21:58 HT-b02 (final build), HT_r02, smoke
- HT-b02: Builds\HT, exit 0 after 401 s, 0 compile errors, no shader errors (Logs/HT-b02.log). Holds every HT edit of this start
  (the last code edit was 21:29; no code change after it).
- HT_r02 (KS17, KS18, KS19 + 7 beat-7 stills, 0 capture failures): looked at all ten (contact sheet QA/captures/HT_r02/_contact/
  contact_HT_r02_01.png) and KS17 / KS19 at full size, KS19 window and door crops: the door reads as a dark panelled door under the
  lantern (HT-10 fixed); the room keeps its warm read; look_up60 shows the koi from below as a fish (head, barbels, pectorals).
- lookv2_check HT_r02: 3/3 PASS. KS17 mean 37.8, sky top 47.4 / sat 0.21, separation 11.2, p999 208.6, goal point 38 px (>= 200
  core), warm stray 0.0 %, koi patch hue 14.8 / sat 0.74; KS18 mean 49.9, separation 15.4, p999 211.1, koi patch hue 13.2 / sat
  0.68; KS19 mean 30.8, texture 0 micro 4.36 / meso 5.01, texture 1 micro 1.63 / meso 1.20, warm outside the goal 0.029 %, inside
  13.94 %, koi warm px 0, p999 209.1.
- image_metrics HT_r02 (QA/captures/HT_r02/image_metrics_HT_r02.md): 6 FAIL checks, none from an HT surface: KS17 NaN-like 1
  (1467, 643) SG's arcade; KS18 isolated 51 on the spire strips' NocLine edges (FX's mask; pre-existing, FX_r03 61); KS19 NaN-like
  11 and beat07_heart_end 1 (610, 438) = FX's brick pits (INT-31); look_back 4 and look_r 1 = single-pixel wall lights of SG's
  arcade / screen wall. Clipped: KS17 0.0000 %, KS19 0.0032 %.
- Smoke (Builds\HT): "-autotest -brisk -beat 7": AUTOTEST PASS, trigger 94.900 s / stop 97.300 s (expected 94.90 / 97.30), ending
  17.02 s, corridor coverage 48 204 rays 0 misses, errors 0, GC collections 0, alloc excess 0.0 B/frame
  (QA/reports/autotest_brisk_from1260.json). With "-autotest-until 1500" (the path end) the harness fails its own untilTime check
  (97.30 s vs the trigger 94.90 +- 2.0: the walk stops 2.4 s after the goal trigger at 1 497.04), so beat 7's smoke runs without
  -until (QA harness note for FX / TQ).
- HT_r01 deleted (numbers above and at 21:14); kept: HT_r02 (28 MB), HT_koi01 / HT_seq01 (reports only).

### 21:52-22:09 HT-final (full EditMode suite, QA/reports/HT-final.xml): 423 tests, 418 passed, 4 failed, 1 skipped
- Every HT test passes (KoiMathTests, KoiShaderTests, VisibilityAuditTests, Beat07HeartTests, MaterialTests.Heart).
- The four failures are in other owners' files, none touched by HT:
  1. Beat06SignsTests.Imagery_PeriodsMeansAndFrequencies (149, gate > 150): SG's NocScreenMath / NocScreenWave work in flight in
     this copy (SG's uncommitted diff).
  2. BoulevardStreetTests.LightBudget_AtMost28_SmoothFades (29 lights on at once in beat 2) and 3. BoulevardStreetTests.
     Shops_Varied_NeverBrighterThanTheLitRoom (18 lit shops, gate 17): BV's beat 2 (the WIP test of 2fe0da9 against BV's committed
     builders; BV's uncommitted work is in the blvd copy). No HT file builds beat 2.
  4. WetFloorTests.WallWindows_MeanOne_Bounded_StreakVertically (min -1.435e-11 < 0): FX's NocMirrorMath round-off (REQUEST below).
  Skipped: AbyssTests.KS10_CompositionSearch (AB's, ignored by design).

REQUEST FX: Scripts/Runtime/Rendering/NocMirrorMath.cs: WallWindows, last line: `return covU / winU * V;` ->
  `return System.Math.Max(covU, 0.0) / winU * V;` (the WallPulseInt difference goes to -1.4e-11 by round-off; WetFloorTests.
  WallWindows_MeanOne_Bounded_StreakVertically fails on it). Optional: the same max(covU, 0.0) in NocMirror.hlsl's twin.
REQUEST FX: Tools/run-player.ps1, the heavy-set loop: `if ($heavySets -contains $val) { $heavy = $true }` ->
  `if (@($val.Split(',') | Where-Object { $heavySets -contains $_ }).Count -gt 0) { $heavy = $true }` (and the same split for
  $fourKSets): a comma list "-capture-set statics,boundaries" is refused by the player (ShotPlan.GuardNote splits the list,
  run-player did not), exit 1 "needs gpu-guard".
REQUEST FX (optional, QA tools): Tools/qa/image_metrics.py neon mask: the spire strips' 1.75 px cyan NocLine edges are flagged as
  isolated pixels (HT_r02 KS18: 51 px at (1331, 941)..(1333, 948)); pre-existing (FX_r03: 61).
NOTE TQ / FX: "-autotest -brisk -autotest-until 1500 -beat 7" fails the harness's own untilTime check (stop 97.30 s vs the goal
  trigger 94.90 +- 2.0) because 1 500 is the path end; until >= PathLength could mean "no until".

### Open at the end of this start (critique-HT.md)
- HT-06 S3: the spire's shaft (FA's hero skin, warm 0) is near black between the strips; at KS18 the spire still reads as a lined
  cylinder, less than before. HT-07 (FX brick pits), HT-09 (SG shop rooms), HT-11 (FX mask) are other owners'.
- HT-12 S2: plaza-facing stills have a large dark near-uniform floor (Look v2 lamps, brass mirrors dark towers): candidates logged.

HT DONE 22:12

## 2026-11-21 FIX WAVE (panel p4_01 on the merged game, master caddb90; task Logs/fix/HT.md with the lead's rulings)

### 01:57 start
- Read Logs/fix/HT.md (C02, C18, C27 S2; C34, C35, C48 S3; lead rulings D-034 5.2 / 6, HT-12, the Mesh.SetColors crash, C48 /
  C10 panel decisions, FX's kind-0 wall request). No open "REQUEST HT" line in any copy's logs (FX's SurfV cleanup was DONE).
- fastcheck OK (runtime 217 / editor 287). HT-b03 (01:58, Builds\HT, 0 errors, 139 s): the merged game built before any change.
- HT_r03 (baseline, KS17 / KS18 / KS19 + 7 beat-7 stills): identical to p4_01. Measured (scratchpad ht6/stats.py): KS19 aperture
  (822-930 x 548-600) mean 117.2, hue 29.8, sat 0.64; balcony underside (690-1040 x 730-780) 27.1, sat 0.09; KS17 koi body
  (1130-1200 x 400-480) 165.7; KS18 body (660-780 x 400-700) 167.0.

### C02 (S2) the koi: a hologram, not an inflatable (NocKoi.shader, KoiShaderMath, KoiBuilder)
- (1) transmission: a first pass "Ghost" (LightMode SRPDefaultUnlit, which URP's DrawObjectsPass draws before UniversalForward for
  the same renderer: its tag list is SRPDefaultUnlit, UniversalForward, UniversalForwardOnly; Blend DstColor One = dst x (1 + k))
  lifts what lies behind the front body shell by k = GhostGain n.v^2 T (1 - cloudMix) (0 on fins, veil, barbels and at the rim);
  the front shell's body term thins toward the silhouette centre, x (1 - CoreClear n.v^2 (1 - 0.7 patch)) (the Kohaku pigment
  stays denser: lookv2 patch sat >= 0.60). Order verified in HT_r04: the koi did not brighten x (1 + k) (it would if the Ghost
  pass ran after the additive one); tower edge-lights read through the body.
- (2) structure: interference gaps across the body every 4.0 m along the spine (half-width 0.25 of the pitch, depth 0.16, the
  line's mean subtracted so the body keeps its energy; faded below 6 px of pitch: they read in KS18 and at 4K, fade in KS17 at
  1080p) and a half-cell feather on the patch edges (patchM = lerp(smooth, scalloped, 0.5 sFade); twin updated).
- Cycles (HT_r04..r08): CoreClear 0.22 / GhostGain 2.0 -> no visible change (KS17 164.4); 0.38 / 3.0 -> 162.5 (the sky x 4 behind
  the centre gave back what the thinning removed: a flat veil); 0.75 / 4.0 -> 155.6, lookv2 patch sat 0.50 FAIL; 0.8 / 1.5 ->
  149.8, patch sat 0.59 FAIL; + pigment denser (1 - 0.7 patch) -> lookv2 3 / 3 PASS (HT_r08, again HT_r10).
- Result (HT_r10): KS17 body region 149.9 (was 165.7), KS18 159.6 (was 167.0): inside LOOK_V2's 130-175 ladder but 16 / 7 codes
  under the old means (the critic's "within 5 codes" is not met in KS17: a darker centre IS the transmission; logged open). KS17
  profile at x 1 165 (y 380-520): rim 206 -> centre 145 (was 157). Clipped: KS17 0.0001 %, KS18 0.0000 % (image_metrics_HT_r10).
  The body still reads mostly pale and solid at 1080p: C02 stays open at S2 with this best result (4 cycles + 2 approaches used).
- (3) cyan light landing on the spire strips / ring-tower faces: NocLighting / NocFacade are FX's (and the strips are emissive
  NocLine): REQUEST FX below. Not raised in KoiDriver (KoiK lights the plaza too, which C34 asks to cool down).

### C18 (S2) the goal window and the balcony (HeartLayout, OldQuarterBuilder)
- From KS19 the upper sash looks UP at the room's ceiling (the critic's "grey veil" was the ceiling plaster at 0.07 - 0.18):
  ceiling 0.07 -> 0.55, back 0.20 -> 0.70, sides 0.18 -> 0.60, floor 0.08 -> 0.18, curtain 0.30 -> 0.90, room light i 4 -> 10.
  Cycles: x 1.7 -> 134.1 (HT_r04); x 2.75 -> 148.2 (r05); ceiling 0.55 -> 167.4 (r06).
- The balcony slab: CC0 concrete skin (lib.Concrete) glowing with the window's warm bounce (GoalLamp x 0.06, emission follows the
  albedo): underside 27.1 -> 57.0, hue 29.5, sat 0.54 (HT_r10).
- Result (HT_r10/KS19): aperture mean 167.4, hue 30.6, sat 0.61 (LOOK_V2 165-195 / 30-36 / 0.50-0.62: PASS); lamp core
  (850-885 x 645-675) peak 253.3, hue 32.0; lookv2 KS19 PASS.

### C35 (S3) railing side returns
- Rods of the two side returns at 0.333 m (4 intervals, RodStepSide 0.33), the front keeps 0.109 m. Test Railing_SideReturns_
  ThreePixelPitchFromKs19 (QA/reports/HT-t07.xml): 32 rods, smallest projected pitch from the KS19 eye 12.7 px at 1080p. 6x crop
  (scratchpad ht6/ks19_lret_r05.png): no hatch, 3-4 clean rods. The NaN-like dark pixels (775, 558), (783, 575), (800, 613) are
  brick pits in the wall beside the window (8x crop ht6/nan_r08.png), not rods: FX's brick (INT-31), unchanged.

### C27 (S2) the koi's eye follows the walker (KoiEye.cs new, KoiDriver)
- Each eye's gaze is a unit vector in the eye's ring frame (KoiMath.Frame at a = 36 m, the shader's frame); its target is the
  direction to the camera; it turns by at most 6 deg/s (great-circle slew); the pupil sits at 0.3 R x (g.a, g.arc) (_KoiPupil).
  Capture stills (a jump > 0.5 m in s) snap, sequences and play slew.
- Beat07FixWaveTests (HT-t07): over a 12 s, 60 fps walk through KS18 starting 40 deg off: largest turn 6.000 deg/s, pupil
  <= 1.260 m (0.3 R), error 40.0 -> 0.00 deg; a still snaps, play never snaps after the first frame.
- HT_koi02 (koi set, 960x540, 2 x 720 frames): sequences.py PASS (koi_ks17 max luma step 0.0001, koi_ks18 0.0002, no spikes:
  no flash, no pop). The pupil itself is 1-2 px at 960x540 (the eye is 4.2 m at 400-500 m; from KS18's eye below the belly the
  flank eyes sit on the silhouette): not resolvable in a head crop at that size (montage ht6/pupil_ks17.png); the KS17 still at 5x
  (ht6/head17_r08.png) shows the pupils off-centre toward the walker. Frames deleted.

### D-034 6 (lead ruling) the empty lit room
- Ground-floor window column -1 (z 384.3 - 385.5) has no dark pane: behind it a room (x 1 156.18 - 1 160.2, y 0 - 3.2, z 382.6 -
  387.2, under the goal room, clear of every other opening: test EmptyRoom_InsideTheTenement_...): a table, a chair pushed back and
  turned 25 deg, a cup, a desk lamp ON (WinCold bulb x 2.2 and a WinCold point light r 4 i 1.8: hue 215, the goal window stays the
  only warm window), plaster walls WinCold x 0.012. Nobody. Real lights in beat 7: 24 (<= 32). HT_r06 beat07_heart_end crop
  (ht6/end_r06.png): the room reads as a cold lit room with the lamp behind the mullion.

### HT-12 (lead ruling) and FX's request: the plaza floor
- FX's REQUEST (kind-0 wall planes): OldQuarterBuilder.RegisterPlazaWalls: the ring towers' inner faces east of the spire (x
  1 800, z 388 +- 330, 450 m, s 1 250 - 1 515) and SG's arcade north face (z 360, x 914 - 990, 38 m, s 1 250 - 1 350), macro
  NocSurfaceMath.FacadeMacro(0, RegionD, s); with the tenement's kind-1 plane at most 3 walls at any s. HT_r10 KS17: the arcade's
  lit ground floor now streaks in the wet floor at the right (x 1 450 - 1 900, y 850 - 950). WetFloorTests in HT-t06 57 / 57.
  DONE HT FX's request (Logs/fix/HT.md: the Heart plaza's facades as kind-0 wall planes).
- Plaza lamps i 2.5 -> 7.0 (r 12 kept): pools at the lamp feet show (r08 -> r10 diff up to 36 codes in the far band y 370-890);
  the near floor 2-20 m ahead is 16 m from the nearest lamp row and stays dark (mid / look_l / look_r unchanged in y 750-1060).

### C34 (S3) the plaza cast: open, REQUEST FX
- dark_cast_RG (lookv2 check 11 on the beat stills): mid -5.30, look_l -5.48, look_r -5.78, look_back -6.09, start -3.23 (HT_r10;
  HT_r03 -5.25 / -5.47 / -5.62 / -5.78 / -3.08). start (s 1 260, KoiK 0.5) is 2 codes less cast: the koi light is part of it.
  The ground term lives in FX's NocLighting.hlsl: REQUEST FX below.

### C48 (S3) the visibility audit (VisibilityAudit, VisibilitySubjects, VisibilityAuditTests)
- KS16: a built wall share more than 2 points under the ray-cast spec model's share is a reported failure (owner SG): visibility.md
  lists "KS16 wall_share (composition-built, owner SG): built 0.545 / 0.413 vs the spec model's 0.586 / 0.444 (14.5.6 0.59 / 0.46)".
- KS12: the composition-built failure carries "RULED D-039 (12)" with the Part 1.6 shares (sky 0.222 / deck 0.313 / rail 0.036 /
  near structure 0.144 / drop 0.284), also in visibility.json ("ruling").
- KS02's tower_flank dropped (lead decision). C10: VisibilityAudit.Run(shots, preReveal) audits a candidate pose; test
  KS04_SubjectsHoldAtPitch27: at FX's pitch 27 every KS04 subject passes (blade_n198 / blade_s186 1.00 / 1.00, canyon_end 1.00 /
  0.40, face_B 0.98 / 0.74, lane_n185 1.00 / 0.85, spec / built), 0 frame / hidden / occlusion failures (HT-t07 13 / 13).

### Tests and runs
- HT-t06 (KoiMathTests | KoiShaderTests | VisibilityAuditTests | Beat07HeartTests | Beat07FixWaveTests | WetFloorTests): 57 / 57.
- HT-t07 (VisibilityAuditTests | Beat07FixWaveTests): 13 / 13.
- Smoke (Builds\HT of HT-b10, "-autotest -brisk -beat 7"): AUTOTEST PASS, trigger 94.900 s / stop 97.300 s (expected 94.90 /
  97.30), ending 17.02 s, coverage misses 0, errors 0, alloc excess 0.0 B/frame, GC 0 (QA/reports/autotest_brisk_from1260.json).
- Builds HT-b04 .. HT-b10: all 0 errors (Logs/HT-b0N.log, Logs/HT-b10.log).
- DISK: W: had 7.2 GB free at 02:50 (other copies / projects); HT's captures are under 0.2 GB (HT_r04-r06, HT_koi02 frames deleted).

### Requests (also in the report's shared_change_requests)
REQUEST FX: Assets/_Game/Shaders/Include/NocLighting.hlsl, NocKoiLight, last line `return PAL_KoiGlow * (NC_KoiLightGain * _NocKoiK * k);`
  -> `float3 tint = lerp(lerp(PAL_KoiGlow, PAL_WinNeutral, 0.6), PAL_KoiGlow, smoothstep(2.0, 30.0, positionWS.y));`
     `return tint * (NC_KoiLightGain * _NocKoiK * k);`
  (C34: the plaza granite reads teal, dark_cast_RG -5.3 .. -6.1 in the beat-7 stills; the koi light's R-G at the ground drops from
  -0.61 to -0.20 of its strength; the koi's own accents above 30 m keep KoiGlow. Shared include: batch it with FX's other edits.)
REQUEST FX (FA): the facade shader (NocFacade lit path): add the analytic koi light to the facade's direct light like NocLighting's
  NocDirect does, `[branch] if (_NocKoiK > 0.0) direct += <facade diffuse albedo> * NocKoiLight(positionWS, normalWS);`, so the
  ring-tower faces and the spire shaft facing the koi take its cyan (C02 (3): +10 codes on the faces next to the body vs the far
  side). The spire strips are emissive NocLine: their read next to the body comes from that shaft light.
NOTE TQ / QA: Tools/qa sequences of the koi eye: a 4K head crop (seq_koi_ks17 at -seq-res 3840x2160, frames 0 / 360 / 719 around
  the head) would show the 6 deg/s pupil turn; at 960x540 the pupil is 1-2 px.

### 03:24-03:56 final cycle
- C02 (cycle 5): the interference bands at 7 m pitch (half-width 0.15, depth 0.22, mean kept) so they survive 1080p in KS17
  (13 px of pitch): HT-b11. HT_r11 KS17 body 149.9, KS18 159.6 (means kept by the bands, as designed).
- Mesh.SetColors crash (lead ruling): 10 player loads of HT-b10 (Logs/player-HT-load1..10.log, KS19 capture each, the whole world
  incl. OldQuarterBuilder.Build): 10 / 10 clean, 0 "Crash!!!"; plus 14 more clean loads this wave (HT-r03..r11, r11k, HT-koi02,
  two smokes). Analysis: MeshBuilder adds a colour with every vertex (Vertex() and Append() are the only writers of Positions and
  Colors), so OldQuarterBuilder's lists always match; BV's stack (blvd Logs/player-BV-r03.log 301-311) dies inside Unity's native
  Mesh.SetArrayForChannelImpl reading a valid List<Color32>: a native-heap fault, not an HT data error. Not reproduced; if it
  returns, FX could build the meshes with Mesh.SetVertexBufferParams / SetVertexBufferData (one interleaved NativeArray).
- HT-final (full EditMode suite on the HT-b10 tree, QA/reports/HT-final.xml): 472 tests, 471 passed, 0 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch, ignored by design). HT-t08 on the final tree (KoiMathTests | KoiShaderTests |
  VisibilityAuditTests | Beat07HeartTests | Beat07FixWaveTests | MaterialTests): 64 / 64.
- Smoke on HT-b11 (final build): AUTOTEST PASS, trigger 94.900 s / stop 97.300 s, errors 0, alloc excess 0.0, GC 0
  (Logs/player-HT-smoke2.log, QA/reports/autotest_brisk_from1260.json).
- Final captures, looked at: HT_r11 (KS17 / KS18 / KS19 + 7 beat-7 stills, contact QA/captures/HT_r11/_contact/contact_HT_r11_01.png)
  and HT_r11_4k (KS17 / KS18 / KS19 at 16:9, 16:10, 21:9 and 4K). lookv2_check HT_r11 3 / 3 PASS (QA/reports/lookv2_HT_r11.md).
  image_metrics HT_r11: 6 FAIL checks, none on an HT surface (KS17 (1328, 330) SG's arcade mullion; KS18 51 isolated px on the
  spire's NocLine edges, FX's mask, as in FX_r03 / HT_r02; KS19 8 dark brick pits, FX's INT-31; heart_end 1 brick pit;
  look_back 8 = SG's screen-wall seams; look_r 1 SG's arcade light). Clipped: KS17 0.0001 %, KS19 0.0037 %.
- C18 at every aspect (HT_r11_4k, warm-blob-centred aperture box): 16:9 166.9 / hue 30.5 / sat 0.61; 16:10 166.4 / 30.4 / 0.61;
  21:9 166.9 / 30.5 / 0.61; 4K 167.2 / 30.5 / 0.61; 4K balcony underside (1380-2080 x 1460-1560) 56.9, hue 29.5. Note: the slab
  reads slightly self-lit (front edge and soffit equally warm); a baked gradient toward the wall would be better (S3, open).
- C02 at 4K (HT_r11_4k KS17 crop 900-2700 x 350-1150): body region 149.6, the tower edge-light verticals read through the body,
  the patch edges soft; the towers' windows behind the body stay faint: C02 open (S2) with this best result.
- Deleted: HT_r04 .. HT_r10 (numbers above), HT_koi02 frames. Kept: HT_r03 (baseline), HT_r11, HT_r11_4k (0.11 GB in all).
- Files (this wave): NocKoi.shader; Beat07_Heart/KoiShaderMath.cs, KoiBuilder.cs, KoiDriver.cs, KoiEye.cs (new), HeartLayout.cs,
  OldQuarterBuilder.cs, PlazaFurnitureBuilder.cs; Scripts/Runtime/QA/VisibilityAudit.cs, VisibilitySubjects.cs; Tests/EditMode/
  Beats/Beat07HeartTests.cs, Beats/Beat07FixWaveTests.cs (new), VisibilityAuditTests.cs; docs/logs/HT.md, Logs/HT_progress.md,
  QA/critique-HT.md.
GLOBAL 03:10: the wet-floor wall planes (ring towers east, SG's arcade face) are active over s 1 238 - 1 527 with their 12 m
  crossfades: they show on the plaza floor of beat 7 (KS17 right) and the last 22 m of beat 6's promenade floor.
HT DONE 03:56
