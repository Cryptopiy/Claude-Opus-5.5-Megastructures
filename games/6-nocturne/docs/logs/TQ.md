# TQ log - capture sets, supersampled reference, GPU timing, texture QA tools (D-030; TEXTURE_PLAN 7.5, 7.7, 8.3 TP-0 step 6)

Owner TQ (new at the third start, 2026-11-15). Exclusive files (moved from TP's TP-0 list by D-030):
- `Assets/_Game/Scripts/Runtime/QA/ShotPlan.cs` (sets `texpans`, `dolly`, `texclose`), `CaptureHarness.cs` (`-capture-ss 2`),
  `PerfMonitor.cs` (GPU timing fix);
- `Tools/qa/tiling.py` (new), `Tools/qa/debugview.py` (new), `Tools/qa/sequences.py` (kinds `texpan` and `dolly`),
  `Tools/qa/run_all.py` (registrations), `Tools/qa/selftest.py` (their self-tests);
- `Assets/_Game/Scripts/Tests/EditMode/QaTests.cs`; this log and `QA/critique-TQ.md`.
Requests to TQ: write "REQUEST TQ: <file>: <change>" in your own log; answers appear here as "DONE TQ <ID> <request>".

## 2026-11-15

### 22:07 start (third start of the build, D-030)
- Read: STATE (RESUMED THIRD START, PAUSED AGAIN), DEVELOPMENT D-001..D-030, BUILDER_BRIEF, TEXTURE_PLAN 7, 8.3 TP-0, 9, 10,
  LOOK_V2 8; ShotPlan, CaptureHarness, CaptureWriter, PerfMonitor, QaProjection, Tools/qa (qa_common, sequences, run_all,
  selftest), lookdev.ps1, run-player.ps1, gpu-guard.ps1; TP's NocDebugViewFeature / NocLookGlobals and FA's debug-view codes in
  NocFacade.shader (for debugview.py); the beat layouts for the texclose and dolly poses (AlleyLayout, NoodleBar,
  BoulevardLayout / StreetFacade, SkybridgeDims, GardenDims / GardenTree, HeartLayout / OldQuarterBuilder, EdgeDims).
- None of my files had uncommitted edits (git status): TQ starts from HEAD.
- A1's `gpuAvgMs` 571 319 705 ms (a1_perf_1080.summary.json): the CSV has 11 GPU samples of 1.8e12-1.7e13 ms among 256 100
  frames (median 0.273 ms, frame median 0.519 ms). 1.7e13 ms is an unsigned 64-bit tick difference that wrapped (2^64 ticks at
  ~1 GHz = 1.8e13 ms). The 10 s cap added in A1 hides that case but still accepts any value up to 10 000 ms, and
  GetLatestTimings(1) returns the same timing for several frames when the GPU queries lag (duplicates weight the average).

### 22:35 step 1 GREEN - PerfMonitor GPU fix, the three sets, -capture-ss 2, QaTests (TQ-t01 32/32)
- PerfMonitor: `PerfGpu` (pure): a GPU sample counts once (new `frameStartTimestamp`; GetLatestTimings(4), oldest first) and only
  when finite, > 0, <= 1000 ms and <= 4 x its own frame interval (`cpuFrameTime`, end to end of two frames) + 2 ms; else
  `gpuRejected`. ProfilerRecorder "GPU Frame Time" is the fallback with the same rule. Summary: gpuAvgMs / gpuMedianMs / gpuP99Ms /
  gpuMaxMs over the valid unique samples, gpuSamples, gpuRejected, gpuDuplicateReads, gpuSource, gpuConsistentWithFrameTime (mean
  GPU <= 1.05 x mean frame + 0.05 ms). A1's worst samples 1.7e13 / 1.8e12 ms are rejected (test).
- ShotPlan: `texpans` (7 x 30 frames, kind texpan, 0.5 px of the output height per frame, centred on the key-shot yaw, ambient held),
  `dolly` (seq_dolly_face30: 300 frames from 800 m to 25 m at 30 deg to face B's normal at its planar part (815.3, 150, -100),
  geometric steps 1.16 %/frame; seq_dolly_graze: 251 frames x 390-640 at 1 m, y 14 over the deck centre, yaw 160 = 70 deg off
  face B's normal, pitch 4, s = x + 184.70), `texclose` (8 stills 1920 x 1080 at vFOV 62, 1.5 m from the surface; floors at
  pitch -50 from 1.15 m). SeqMode.Poses (per-frame pose arrays, per-frame "dist" in sequence.json).
  Why face B and not a boulevard slab for dolly 1: the canyon is 150 m wide and the opposite slabs reach y 560, so no slab face
  can be seen from 800 m at 30 deg; face B at z -100 is planar (F = 0), the 800 m eye (122.5, 150, -500) is 27 m in front of face
  A (x_A(-500) = 95.3), no cross bridge between (z -640 is at y 90). Switch to FA's -facade-sheet when it exists.
  Why y 14 for the grazing pass: from the deck at eye height the glass tubes (z 183 / 207, r 4.5, tops y 6.5) cover 70 % of the
  frame height at yaw 160 (elevations -28..+21 deg); from y 14 their tops are at -32 deg, under the bottom row (-29 deg).
- Routing (Part 2.5): run-player.ps1 is not mine and does not list texpans / dolly. `ShotPlan.RunPlayerRoute` mirrors its rules
  (QaTests parses its trigger list) and `GuardNote` makes the harness refuse, in the player: texpans / dolly without `-sequence`
  (run-player routes every -sequence through gpu-guard) and `-capture-ss 2` without `-capture-4k` (the 4K turn, 6500 MiB).
REQUEST LEAD: Tools/run-player.ps1: add "texpans", "dolly" to $heavySets, and treat "-capture-ss" with a value >= 2 like -capture-4k ($heavy = $is4k = $true). After that TQ moves texpans / dolly from ShotPlan.GuardedSequenceSets to HeavySets (QaTests keeps the lists in sync) and the -sequence / -capture-4k workarounds become unnecessary.
- CaptureHarness `-capture-ss 2`: render at 2x per axis into the MSAA 4x RGBA16F target, Blit (MSAA resolve) into an RGBA16F
  bilinear texture of the render size, bilinear Blit into the output-size sRGB target: each output pixel centre samples the shared
  corner of its 2 x 2 texels (weights 0.25), an exact box in linear light; renders above 3840 x 2160 refused (the 4K key-shot
  variants fail with a note), parity skipped, report.json render.supersample / sequence.json supersample.
- Commands (until REQUEST LEAD is carried):
  - texclose: `powershell -NoProfile -File Tools\lookdev.ps1 -Slot <ID> -Run r01 -CaptureSet texclose -Extra "-freeze-clock,-rain,0"`
  - texpans:  `... -CaptureSet texpans -Extra "-sequence,texpans,-freeze-clock,-rain,0"` (+ `,-seq-res,3840x2160` for 4K)
  - dolly:    `... -CaptureSet dolly -Extra "-sequence,dolly,-freeze-clock,-rain,0"`
  - the R1 reference: `... -CaptureSet texpans -Extra "-sequence,texpans,-capture-ss,2,-capture-4k,-freeze-clock,-rain,0"`
  - then `python -I Tools/qa/sequences.py QA/captures/<ID>_r01 [--reference QA/captures/<ID>_r02]`, `python -I Tools/qa/tiling.py ...`
- fastcheck runtime 164 / editor 218 OK; Unity EditMode QaTests 32/32 (TQ-t01, every assembly compiled).

### 23:12 step 2 GREEN - Tools/qa: tiling.py, debugview.py, texpan / dolly kinds, run_all registrations (selftest 103/103)
- `tiling.py` (new): per region the window-unbiased autocorrelation of the high-passed display luma (zero lag 1, main lobe out,
  lags to half the region at >= 30 % window overlap); with a period range (px at 1080p) R3 gates the peak at <= 0.25, without one
  the five strongest peaks are listed (WARN > 0.25: brick courses, floor lines and window grids are periodic by design); LOOK_V2 8.2
  check 7 micro / meso / valid share as INFO (PA's lookv2_check.py is the gate). Default regions: LOOK_V2's KS00 / KS01 / KS02 /
  KS19 walls, the KS00 floor, the central 80 % of the texclose stills.
- `debugview.py` (new): decodes views 1-10 of TEXTURE_PLAN 7.1 (FA's NocFacade codes, TP's view 7 and 9 encodings); gates: view 4
  no pixel with both signs, view 9 overdraw <= 1.5 at KS04 / KS05 / KS09 / KS17 (else the prepass), view 7 unit normals WARN;
  `--maps` writes false-colour maps (2, 5, 9, 10) into <dir>/_debugview/.
- `sequences.py`: kind `texpan` (frames resampled into the middle frame with the exact rotation homography from sequence.json,
  Keys cubic; swimming = half-whitened cross-correlation shift of textured 64 px blocks first vs last, p95 <= 0.5 px; with
  `--reference <the -capture-ss 2 run>` the R1 gates: screen-space temporal std excess AND stabilised residual excess, worst 32 px
  block <= 2.0 levels. The screen-space one alone is blind: a 15 px pan gives textured pixels 10-15 levels of std from the motion,
  so a 6-level flicker adds only 1.7 (selftest); the stabilised one reads 5.2). Kind `dolly`: the generic luma-step / spike checks
  plus local pops per 64 px block (3 x the block's running median and > 2 levels) with the camera distance.
- `run_all.py`: steps tiling, debugview (when report.json carries -debug-view), lookv2_check.py (PA) and facade_sheet.py (FA) as
  processes, each skipped when missing; `--reference` passed to sequences.
- selftest: tiled 108 px texture R = 1.000 at (0, 108) FAIL, shuffled tiles 0.014 PASS; debugview 4 / 7 / 9; world-locked pan
  swim 0.03 px PASS, screen-locked layer FAIL, flicker over the reference FAIL (5.2), identical reference 0.0 PASS, dolly pop at
  frame 20 with its distance. 103 / 103 assertions (python -I Tools/qa/selftest.py --no-encode --no-4k).

### 23:25 step 3 GREEN - requests of FA and PA, texclose deck pose (TQ-t02 33/33)
- TQ_r01 (texclose, lookdev_TQ build 22:47-22:53, -freeze-clock -rain 0): 8 stills 1920 x 1080, image_metrics 0 FAIL. Looked at
  every one (contact sheet + full size for the deck, tenement, podium, noodle bar, glass tube): every pose frames its surface,
  nothing in the near plane, no empty sky, except tc_bridge_deck (TQ-01: the top-left corner saw past the north deck edge) -> pose
  moved to the centre line, pitch -65 from 1.36 m (corners 0.27 m inside the edges, QaTests checks them). Mean luma: alley wall 19.8,
  deck 31.6, glass tube 42.1 (the Gulf through invisible v1 glass, TQ-03 -> SU), noodle bar 116.6 (the WinWarm back wall),
  plaza 31.3, podium 13.3 (unlit, TQ-04 -> FA), tenement 23.9, trunk 16.0.
- tiling.py on TQ_r01: the 64 px dither tile showed as repetition on every flat v1 surface (TQ-02) -> lattice peaks reported apart
  (dither_peak 0.46-0.96), a repetition peak needs a trough before it (bark streaks no longer count). Repetition peaks now
  0.05-0.35 (tenement 0.35 at (0, 79): its panel joints). selftest 103/103 again.
DONE TQ FA REQUEST Scripts/Runtime/QA/ShotPlan.cs: (1) capture set "facadesheet" = every line of NocFacadeSheet.Poses() through ParsePose (found by reflection, tags pose_fs..., set "facadesheet"; run with -facade-sheet); (2) the optional ninth pose field s (`pose:NAME:x:y:z:yaw:pitch:fov:s`, 0..1500) replaces the path projection for the parameters, fog, vFOV <= 0 and the ambient clock. Also: with -facade-sheet the dolly's 30 deg line runs to the sheet (seq_dolly_sheet30: subject 0's frontal tower, front face z 5 585, from 800 m at (17, 120, 4 892) to 25 m, s 300).
DONE TQ PA REQUEST Scripts/Runtime/QA/ShotPlan.cs: EncodeClips: enc_beat07_goal (KS19 view) and enc_beat01_wall (KS00 view) added, enc_trans_mouth text "fog 5e-3 -> 4.4e-4".
- PerfGpu rule replayed on A1's CSVs (offline): a1_perf_1080 256 080 valid samples, 14 rejected (the 1.8e12-1.7e13 ms ones and
  3 above 4 x frame + 2 ms): gpuAvg 0.277 ms (median 0.273, p99 0.456) beside frame 0.568 ms -> consistent (A1 summary: 571 319 705);
  a1_perf_4k 63 086 valid, 0 rejected, gpuAvg 1.190 ms beside frame 1.354 -> consistent.

## 2026-11-16

### 00:15 step 4 - the sets in the player of slot TQ (lookdev_TQ build 23:31), analysis fixes
- TQ_r02 texclose (-freeze-clock -rain 0): 8 stills 1920 x 1080, image_metrics 0 FAIL; the deck now fills its frame (TQ-01 fixed).
- TQ_r03 texpans (-sequence texpans -freeze-clock -rain 0; run-player routed it through gpu-guard): 7 x 30 frames 1920 x 1080,
  13.1 s of capture, 0 failures; harness max luma step 0.003-0.016 levels/frame. Looked at every frame (montages
  QA/captures/TQ_r03/_montage, 30 frames each) and at frames 0 / 15 of each pose at full size.
- TQ_r04 texpans -capture-ss 2 -capture-4k: report.json render.supersample 2, every frame 1920 x 1080, 0 failures.
- TQ_r05 dolly: seq_dolly_face30 300 frames, seq_dolly_graze 251 frames, 1920 x 1080, 0 failures; harness max luma step 0.318 /
  0.227 levels (R2 limit 1.2), 0 spikes. Montages (every 5th frame) and the end frames looked at. Garden crowns enter the left
  edge of both dollies (TQ-05), never over the target; face A / gate A leave the frame in frames 1-3.
- Analysis fixes found on these real captures (selftest 103/103 after each):
  - swimming: the screen-locked dither in dark blocks with a few slits read as the pan (15 px) -> 5 x 5 box first; blocks of
    one-directional lines (the KS09 cross-bridge strip, KS17 slanted window rows) read their stair-step crawl as a shift
    along them -> 2-D texture only (structure-tensor eigenvalue ratio >= 0.15). Result p95 0.07-0.33 px, median 0.03-0.08 px:
    no swimming at any of the 7 poses; max WARN KS04 1.22, KS09 3.83, KS15 1.14 px (single blocks, TQ-06).
  - the -capture-ss 2 reference shows a different picture where a footprint regime switches with resolution: NocNeon's blade
    glyphs fade to a flat panel below 6 px of stroke (1x) but are resolved at 2x, and HDR-bright sub-pixel strokes are averaged
    after the tonemap -> blocks whose temporal mean differs by > 2 levels / 5 % are reported apart and left out of the shimmer
    gates. Share of such blocks: KS00 0.2 %, KS04 22.9 %, KS05 20.5 %, KS08 10.6 %, KS09 6.2 %, KS15 0.5 %, KS17 14.9 %.
  - dolly pops: a running-median test flagged 2 497 edges moving through blocks -> a pop is a one-frame jump (> 3 x both
    neighbours and > 2 levels): 20 (face30) and 13 (graze) events, all in frames 1-3 (near face A / gate A leaving the frame).
- R1 on TQ_r03 vs TQ_r04 (worst 32 px block, levels; mismatch blocks out): screen-space std excess KS00 2.30, KS04 3.38,
  KS05 3.34, KS08 2.05, KS09 1.17, KS15 0.75, KS17 1.86; stabilised residual excess 1.40 / 1.48 / 1.44 / 2.04 / 1.85 / 1.21 /
  1.90. v1 baseline for FA / SU / FO (TQ-07); frame mean ss2 - 1x: KS00 +0.08, KS15 +0.05 (no regime switch: the
  supersampling itself is neutral), KS04 -1.95, KS05 -1.80 (the blade signs, TQ-08 -> PA), KS08 -0.62, KS09 -0.34, KS17 -0.64.
TQ SETS READY 00:14 2026-11-16 (texpans TQ_r03, the -capture-ss 2 reference TQ_r04, dolly TQ_r05, texclose TQ_r02; commands in the 22:35 entry: texpans / dolly need -sequence <set>, -capture-ss 2 needs -capture-4k until REQUEST LEAD is carried; with -facade-sheet the dolly's 30 deg line runs to FA's sheet)
REQUEST PA: Shaders/NocNeon.shader: (finding TQ-08, your call) at 1080p the near boulevard blades (stroke 0.055 x 14 m = 0.77 m, 3-6 px at 100-200 m) sit in the 3..6 px flat-fade band and show as flat lit panels, 1.8-2.0 levels brighter than the same frame rendered at 2x (glyph strokes on a dark panel; QA/captures/TQ_r03/_evidence/ks04_side.png); LOOK_V2 8.1 KS04 asks "blade glyphs" to read. Options: keep strokes resolved down to the 1.75 px minimum drawn width before the flat fade (NC_NeonFlatPx lower), or darken the flat regime toward the post-tonemap mean of resolved strokes.

### 00:25 step 5 - keyshots1080 at 1x and -capture-ss 2, the perf run
- TQ_r06 keyshots1080 (-freeze-clock -rain 0) and TQ_r07 the same with -capture-ss 2 -capture-4k: 20 + 20 stills 1920 x 1080,
  0 failures, report render.supersample 1 / 2, parity skipped with a note in r07.
- Mean luma ss2 - 1x (levels): within 0.5 in 13 of 20 (KS00 +0.08, KS01 -0.02, KS02 -0.30, KS07 -0.21, KS09 -0.34, KS10 -0.02,
  KS11 +0.04, KS12 -0.18, KS13 -0.10, KS14 +0.08, KS15 +0.05, KS16 -0.17, KS19 +0.11); outside: KS03 -1.67, KS04 -1.95,
  KS05 -1.80, KS06 -0.98 (the boulevard blades' flat-fade regime, TQ-08), KS08 -0.62, KS17 -0.64, KS18 -0.82 (sub-pixel HDR strips
  / the koi strokes averaged after the tonemap). Where nothing switches regime the 2x render and its box are neutral (|delta|
  <= 0.11 in the alley, garden and Old Quarter).
- Aliasing on thin lines and edges (median level-line curvature |I_tt| / |grad I| over the pixels with |grad| > 6 levels / px of
  the 1x frame, after a [1 2 1] blur): lower with ss2 in 15 of 20 shots, median ratio 0.90 (KS13 0.70, KS19 0.71, KS15 0.72,
  KS01 0.78, KS04 0.80); higher only where the 2x frame resolves new strokes (KS03 1.36, KS17 1.28, KS18 1.45, KS07 1.00,
  KS12 1.01). (A plain second-difference score went up with ss2: it counts resolved detail as aliasing.)
- TQ_perf01: run-player -cinematic -perf-log -quit-after 90 through gpu-guard (exclusive turn), vSync 0: 46 797 frames,
  avg 1.808 ms (553 fps), p99 2.60 ms, max 7.0 ms; gpuAvgMs 1.790, median 1.695, p99 2.564, max 6.99; 46 458 unique GPU samples,
  0 rejected, 0 duplicate reads, source FrameTimingManager, gpuConsistentWithFrameTime true (GPU-bound: GPU 99 % of the frame).

### 00:45 step 6 - FA's sheet in the player, region requests of GE and PA
- TQ_r08 dolly with -facade-sheet -facade-v 2 (-capture-only sheet30): seq_dolly_sheet30 300 frames, 0 failures, harness max
  luma step 0.259 levels, 0 spikes; looked at all 300 (montage) and frames 0 / 60 / 150 / 200 / 260 / 299 larger: the sheet row
  at 800 m, the subject-0 tower face filling the frame at 25 m, no other tower in front. One local pop: frame 238 (50.7 m),
  6.5 levels against 2.1: lit windows of FA's v2 path change their interior pattern every frame (thin vertical bars / whole
  lit panes alternate, frames 236-239; QA/captures/TQ_r08/_montage/pop_f238.png) -> TQ-09, REQUEST FA below.
- TQ_r09 facadesheet (-facade-sheet -facade-v 2): the 27 poses of NocFacadeSheet.Poses() as pose_fs* stills, 1920 x 1080, 0
  failures (image_metrics' 4 FAIL checks are the sheet's mean-luma windows, not a capture defect).
- Disk: TQ_r05's frames deleted (montages, sequence.json and report.json kept); W: 25 GB free.
- Region requests (Tools/qa/regions_default.json: the lead's prompt sends "new poses, regions, metrics" to TQ; no package owns
  the file in TEXTURE_PLAN 8.3, so TQ carries them): checked on QA/captures/PA_r03 with the rects drawn.
DONE TQ GE REQUEST Tools/qa/regions_default.json: KS15 crown_gap_sky dropped (the east block's lit facade fills the crown gap; no sky or soffit gradient is left in KS15); KS13 abyss = [0.465, 0.34, 0.545, 0.64], inside the glass strip between its inner members, above the light line (y 0.665) and the truss beam (y 0.84).
DONE TQ PA REQUEST Tools/qa/regions_default.json: (1) KS05 cloud_ceiling y 0.05-0.36; KS04 sky_above_facades y 0.02-0.22 (the canyon skybridge starts at 0.26); KS07 sky_above_gulf dropped (face B now fills the top of KS07); KS13 abyss and KS15 as for GE; KS17 plaza [0.10, 0.86, 0.85, 0.97] (clear of the bollards, sky_koi_spire unchanged: still sky). (2) "seq_enc_beat07_goal" with KS19's tenement_wall rect, and "seq_enc_beat01_wall" with KS00's alley_wall rect; enc_beat07_goal is now a static 10 s clip at the KS19 pose (clock running), because on a 15 m walk at KS19's gaze the wall grows 3 x (23.8 m -> 7.8 m) and no fixed rect stays on it (enc_beat01_wall at KS00, s 0, was static already).
REQUEST FA: (finding TQ-09, your call) seq_dolly_sheet30 with -facade-v 2 (TQ_r08, your 23:31 tree): at 50 m the lit windows' interior pattern changes every frame (frames 236-239: thin vertical bars, then a whole lit pane, then bars again; one-frame block jump 6.5 levels at frame 238, QA/captures/TQ_r08/_montage/pop_f238.png). Re-run `lookdev.ps1 -Slot FA -CaptureSet dolly -Extra "-sequence,dolly,-capture-only,sheet30,-facade-sheet,-facade-v,2,-freeze-clock,-rain,0"` and `sequences.py` on it (kind dolly: local pops).
- 00:50 TQ-t03 full EditMode suite: 351 / 355; QaTests all green. The 4 failures are in other owners' files, mid-edit:
  Beat02BoulevardTests.Blades_Are14x200_AtTheSpecPositions ("magenta, cyan, amber, violet in x order": the v1 colour order against
  PA's v2 blade colours -> PA), FoliageTests.Crowns_AreLobed_AndLimbEndsStayInsideTheCore (B3 circle 1 lobes 0 -> FO),
  TextureTests.Import_AlbedoLastMipMeanIsHalfLinear (LeafCrown_Albedo last mip B 0.645 -> TP) and
  TextureTests.Sets_TileMatchesTexturesMdOrIsRegistered (RoomArr slice 4 Carpet016 tile 1 m vs 1.7 m -> TP).

### 01:10 step 7 - debugview.py on real debug-view captures; selftest with its own KS04 rect
- TQ_r10 (-debug-view 9, KS00-KS09) and TQ_r11 (-debug-view 4 -facade-v 2, KS00-KS09), decoded with
  `debugview.py --maps` (QA/reports/debugview_TQ_r10_r11.md, maps in QA/captures/TQ_r10/_debugview): NocFacade overdraw without
  the prepass KS04 1.795 (complexity 9.7), KS05 1.398, KS09 3.314 (16.4), KS07 3.50, KS08 2.23 -> the KS04 / KS09 gates FAIL,
  the same numbers as TP's own r01dv9 (1.79 / 1.39 / 3.30): TP adds NocFacadePrepassFeature. View 4: no pixel with both signs
  above 0.05 % (worst KS09 0.050 %, MSAA edges), sigma -1 share 25 % in KS08 and 39 % in KS09 (the faces on the other side).
- The KS04 sky rect moved to y 0.22 broke the selftest's undithered-banding catch (its synthetic gradient spans the top 30 %):
  the selftest now writes its own copy of the regions with the old KS04 rect. selftest 103/103 (--no-encode --no-4k) and the
  full selftest with the x264 / VP9 encode and 4K images 111/111.
- 01:15 TQ_r12: texpans at 4K (-sequence texpans -capture-only ks04 -seq-res 3840x2160, routed as a 4K job by -seq-res): 30 frames 3840 x 2160, pan step 0.0172 deg (half the 1080p step), 0 failures; frame 15 mean luma 40.91 levels: near the ss2 frame (41.30), 2.35 below the 1080p frame (43.26), the blade regime of TQ-08. Folder deleted after the check.
- 01:25 TQ-06 identified: the residual 3.8 px shift at KS09 (1024-1088, 448) is moire arcs of the v1 section-mode window grid on the Gulf face moving against the face during the pan (QA/captures/TQ_r03/_evidence/texpan_ks09_swim.png) -> FA verifies texpans KS09 with -facade-v 2.
- 01:42 FA answered TQ-09 (FA.md: FA_d03 sheet dolly -facade-v 2, 0 local pops, luma step max 0.218): critique TQ-09 closed. No open REQUEST TQ; DONE so far: PA, KO.
- 02:21 note: TP.md 22:14-22:40 records one failed TP compile on ShotPlan.TexPanSequences while TQ's step-1 edits were half applied (the Build switch called the new generator before its body was written): TQ's later multi-part edits were applied in one write (patch scripts) after a fastcheck.
- 03:16 end of the stay-on window (3 h after TQ SETS READY). DONE lines seen: PA, KO, GE, FO (FA and SU still working). Every
  REQUEST TQ answered (FA facadesheet + pose s; PA encode clips; GE / PA regions). TQ's own requests: TQ-08 carried by PA (NocNeon
  flat regime, PA_r04), TQ-09 carried by FA (FA_d03); REQUEST LEAD for Tools/run-player.ps1 still open (texpans / dolly need
  -sequence, -capture-ss 2 needs -capture-4k until then).
- Kept capture folders: TQ_r02 (texclose), TQ_r03 + TQ_r04 (texpans 1x and the ss2 reference, _montage, _evidence), TQ_r05
  (dolly face30 / graze: montages and reports, frames deleted), TQ_r06 + TQ_r07 (keyshots1080 1x / ss2), TQ_r08 (sheet dolly),
  TQ_r09 (facadesheet), TQ_r10 + TQ_r11 (debug views 9 and 4). Perf: QA/reports/TQ_perf01.csv + .summary.json.
TQ DONE 03:16 2026-11-16
