# INT - Look v2 integrator (third start of the build, run wf_2c6d782f-763)

Requests to INT: write "REQUEST INT: <file>: <change>" in your own log; answers appear here as "DONE INT <ID> <request>".
Every change INT makes in another owner's file is listed below with its reason.

## 2026-11-16 03:55 start
- Read: STATE.md (RESUMED / PAUSED AGAIN), DEVELOPMENT.md D-001..D-030, docs/BUILDER_BRIEF.md, TEXTURE_PLAN 7-10, LOOK_V2 8,
  the eight package reports and logs (TP, TQ, PA, KO, FA, SU, FO, GE: all carry their DONE line), the vault note.
- HEAD 8c44b63. Uncommitted at the start: RebuildAll output (28 materials, MaterialLibrary.asset, the new Leaves.mat /
  LeafCore.mat + metas, Main.unity, GraphicsSettings / ProjectSettings) and QA reports; no package source file is dirty.
- W: 20 GB free (the floor). QA/captures holds 7.6 GB (Phase 3 and package folders, not mine). Plan: every heavy INT set is
  captured, measured, montaged and its frames deleted before the next one.
- Machine: no Unity / Nocturne process; two python processes of another game (mc_r3*) are running, not mine.

## Requests
- DONE INT PA REQUEST Scripts/Tests/EditMode/Beats/Beat02BoulevardTests.cs:68: the v1 4-cycle assertion replaced by
  `Assert.That(BladeSign.Palette(i), Is.Not.EqualTo(0).And.Not.EqualTo(6), ...)` exactly as PA specified (no magenta or red
  blade; unowned file, G1's in Phase 3).
- Open, for the lead (listed, not carried): TQ's REQUEST LEAD Tools/run-player.ps1 (texpans / dolly in $heavySets, -capture-ss >= 2
  as 4K): it needs matching edits in TQ's ShotPlan.HeavySets / GuardedSequenceSets / GuardNote and two QaTests; the workaround
  (-sequence <set>, -capture-ss 2 with -capture-4k) works, INT uses it. GE's REQUEST lead (PlayerController / ProgressDirector
  -beat N start a few 1e-5 m short of the boundary, BeatDirector.OrderOk assumes a walk from beat 1): GE's AutoTestRunner
  tolerates both. GE's REQUEST PA KS13 luma window: a lead decision (see the critique).

## Changes in other owners' files (with reason)
- Scripts/Runtime/Rendering/NocDebugViewFeature.cs:83 and NocFacadePrepassFeature.cs:63 (TP): `FindObjectsByType<MeshRenderer>(
  FindObjectsInactive.Exclude, FindObjectsSortMode.None)` -> `FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude)`.
  Reason: the gate asks fastcheck with 0 warnings; these were the only 4 (CS0618, the sort-mode overload is deprecated in
  6000.6). Order does not matter (each loop tags renderers independently); the overload is the one CaptureHarness / NocAlloc use.
- Scripts/Tests/EditMode/Beats/Beat02BoulevardTests.cs:68 (unowned): PA's REQUEST INT above.
- Scripts/Editor/BatchTools.cs (TP) Validate R14 gate: Unity strips the fxc codes from its shader messages, so the gate's
  `Contains("X3570") || Contains("X4000")` never fired; INT-validate01 logged two X4000 that passed the gate ("use of potentially
  uninitialized variable (NocFaStreak)" NocFacade.shader:85, "(NocForwardHook)" NocFoliage.shader:362; PA had removed the same
  kind from NocNeon). The gate now also matches "potentially uninitialized", "gradient instruction used in a loop" and
  "gradient-based operations".
- Scripts/Runtime/Rendering/NocLookGlobals.cs (TP): `DefaultFacadeV = 1` -> `2` (FA-5 "after INT's gate", FA's proposed
  decision 6; D-027 asks for textured buildings and the v1 default is the confetti field CRIT0 #3 / #4 names). Evidence: one build,
  keyshots1080 with the v1 default (lookv2_01_ksv1) and with -facade-v 2: lookv2_check 6 -> 8 / 20, KS04 separation 1.4 -> pass,
  KS17 separation -4.7 -> pass, no new failure class; the v2 frames read as dark textured towers with small framed windows. The
  relief follows (-relief-v defaults to -facade-v: 83 111 triangles). EditMode with the new default 361/361 (INT-tall02).
- Tools/qa/regions_default.json (TQ's; no Look v2 owner) KS16 16:9 banding rect wall_panel [0.1, 0.62, 0.3, 0.95] -> [0.2, 0.56,
  0.32, 0.76]: the old rect now holds GE's sign-base arcade (lit panes, mullions: "banding" 46.4 with a 69.6-code step at its
  top edge); the new rect lies inside one panel, between the seams (drawn and checked on lookv2_01 KS16).
- Shaders/NocFacade.shader NocFaStreak (FA) and Shaders/Beats/NocFoliage.shader NocForwardHook (FO): the early `return` inside a
  `[branch]` rewritten as one return (`res` / `c` assigned in the branch; the NaN semantics of the condition kept with
  `!(...)`). Same arithmetic, no picture change; the fxc warning is gone (INT-validate02: 103 checks, 0 shader warnings).

## Gate
- 03:58 fastcheck: runtime 177 / editor 234 OK, 0 warnings (after the TP fix above; 4 CS0618 before).
- 03:59 INT-rebuild01 RebuildAll SUCCESS, 0 problems, 8.0 s: 40/40 materials written (the dirty .mat / MaterialLibrary / Leaves /
  LeafCore and the NocGrating / NocShopfront Always-Included lines are this output, committed by INT), Main.unity structurally
  identical (previous file kept), 18 Megastructure/* shaders, ShaderGen 0 files changed.
- 04:00 INT-validate01 PASS 103/103 (but two X4000 passed the gate, see above); 04:02 INT-validate02 PASS 103/103, 0 shader
  warnings, R14 gate "none". Warnings only: SRP Batcher "Not initialized" for 18 shaders (batch mode, as in every earlier
  Validate), 8 of 69 External textures unreferenced (a gate after TP-2).
- 04:07 INT-tall01 full EditMode 361/361 (203 s; PA's blade test fixed). 04:08 INT-build01 BuildWindows SUCCESS 0 errors /
  0 warnings, 462.5 MB.
- 04:09 one build, both facade paths: lookv2_01_ksv1 (default v1) and -facade-v 2 keyshots1080. Looked at both contact sheets and
  KS04 v1 / v2 full size, then every v2 key shot full size (KS00-KS05, KS07-KS13, KS15-KS19) and crops (KS00 lamp wall and slot,
  KS13 lamp through the glass and deck plate, KS18 isolated pixels, KS16 regions). Decision: DefaultFacadeV = 2 (above).
- 04:13 INT-tall02 EditMode with DefaultFacadeV 2: 361/361 (203 s); INT-build02 SUCCESS 0 / 0, 462.5 MB.
- 04:15 lookv2_01 keyshots1080 on the final build: pixel-identical to the -facade-v 2 run (mean |d| 0.0, 0 px > 8 codes on all 20).
  run_all: lookv2_check 8 / 20; image_metrics 19 FAIL checks (NaN-like / isolated = traffic heads, spire strips, rain, brick pits:
  looked at in crops; banding KS00 mouth_glow and KS03 alley_wall_dark: rects full of signs / edges, 0 visible steps; KS17 plaza 2.0
  (granite macro patches under x3 gain, known GB-10); KS13 abyss: rect held the strip's beam and the tube rings -> moved);
  tiling 0 FAIL (KS19 WARN, the brick bond).
- regions_default.json KS13 abyss [0.465, 0.34, 0.545, 0.64] -> [0.49, 0.44, 0.545, 0.64] (the old rect crossed the glass strip's
  cross beam at y 0.39-0.42 and the tube rings at x < 0.485; drawn on KS13): banding 1.93 FAIL -> PASS.
- 04:20 INT_smoke01 (-autotest -brisk -autotest-until 400): PASS, beats 1-3 at t 0.02 / 48.17 / 160.17, 0 errors, coverage 48 204
  rays 0 misses, GC 33.5 B/frame (32 box + 1.5).
- 04:23 INT-play01 PlayMode: 4 tests, 2 passed, 2 skipped (no keyboard / mouse in batch mode, as in Phase 2 / Q1), 0 failed.

## Captures (main build Builds/Nocturne, build02, default flags facade-v 2 / surf-v 2 / foliage-v 2)
- 04:21 lookv2_01_4k (keyshots -capture-only _4k: 20 x 3840 x 2160, 204 MB): KS04 crops at 1:1 (left slab: punched grid, two-pane
  windows, warm / neutral / cool panes, faint wall skin; face B: section grid, atria halls, mechanical slits, relief risers; the
  upper tube end-on reads as a translucent "searchlight" wedge from the canyon bridge to the face-B portal ring, in v1 too).
- 04:21 lookv2_01b (beats, 51 stills): all three contact sheets; full size beat03_edge_look_l (balustrade lamp: white blob with a
  diagonal flare and white layer lines like laser beams over a flat brown abyss).
- 04:22 lookv2_01tc (texclose -rain 0, 8): contact + tc_noodle_bar full size (the bar room behind the counter reads as a pastel
  bookshop: SU's NocShopfront product rows, flat bright); alley wall, bridge deck, podium panel and trunk are near black at 1.5 m.
- 04:22 statics (-sequence statics -freeze-clock -rain 0, 7 x 120): sequences.py PASS, max luma step 0.0000, max pixel change
  0.8 level (beat 7), 0 spikes. Frames deleted (2.2 GB); QA/reports/sequences_lookv2_01st.md.
- 04:23 texpans 1x + -capture-ss 2 -capture-4k reference (7 x 30): swimming p95 0.10-0.31 px (max 0.42; KS09 max 2.67 WARN,
  TQ-06 moire on the Gulf face); R1 shimmer over the reference FAIL KS04 7.21 (256, 320), KS05 6.71 (0, 768) - tower-corner
  edge lights (white + cyan, 3 px) stair-step and crawl (crop montage QA/reports/INT_texpan_ks04_block.png), KS00 2.77 (G1 sign),
  KS17 2.42 + stabilised 2.88 (ring-tower windows, FA-D20), KS08 stabilised 2.17 (lamp pole); KS09 1.47 / KS15 1.37 PASS. Same
  numbers as FA's p08 / p09 (FA-D19 / FA-D20 open). Frames deleted (1.1 GB); QA/reports/sequences_lookv2_01tp.md.
- 04:25 dolly (face30 300 + graze 251): luma step max 0.390 / 0.136 levels (<= 1.2) PASS, 0 spikes; local pops WARN 17 / 10, all
  at the first frames (1-2) or the last (299 at 25 m: window-room parallax of a whole lit pane, looked at in crops) - motion of near
  geometry, no regime pop. Montages QA/captures/lookv2_01seq/dolly_*_montage.png; frames deleted (1.4 GB).
- 04:34 koi (seq_koi_ks17 / ks18, 2 x 720 frames at 60 fps, 2.5 m/s): sequences.py PASS, luma step max 0.018 / 0.026 levels,
  0 spikes; montages lookv2_01seq/koi_ks1{7,8}_montage.png (looked at KS18: head, scales, patches, veil hold over the walk; the
  crescent outline stays, KO-12). Frames deleted (3.7 GB).
- 04:35 rain (seq_rain_beat04, 120 frames 60 fps): PASS, luma step 0.006, outside-streak change 2 levels (<= 6), streak mask
  0.26 %. Frame 60 kept (lookv2_01seq/rain_beat04_f60.png).
- 04:36 material sheet (pose sheetL, sheetW with -material-sheet): looked at sheetL (metals, painted steel, concrete, plaster on
  pavers under coloured lights: no missing map, no magenta). facade sheet (27 poses, -facade-sheet): contact sheet 1 looked at;
  facade_sheet.py: 331 faces, 89 floor-line failures (54 at 1 km, FA-D18; FA_s05 had 140), wall micro at 75-300 m median 2.27
  codes, skin means at 1 km 28.3-42.8, at 3 km 43.6-46.2.
- tiling.py: key shots 0 FAIL (KS19 brick bond WARN 0.267), texclose 0 FAIL (deck plate 0.304 and tube grid 0.507 WARN: periodic
  by design).
- 4K money shots looked at in crops: KS04 (slab, face B), KS10 abyss x2 gain (empty umber haze, two white beam-like layer lines),
  KS17 koi head (scales, eyes, barbels, fin rays), KS19 window (no frame, curtain reads as stone). The 13 other 4K frames deleted.
- boundaries, one window per run (frames deleted after each): see below.
- 04:40-04:58 boundaries (-sequence boundaries -capture-only <window>, 1080p 30 fps, 1.5-3.0 GB each, deleted after the
  analysis): luma step per frame max b12 0.595, b23 0.098, b34 0.813, b45 0.038, b56 0.067, b67 0.082, s96 0.592, s380 0.094,
  s1240 0.085 (<= 1.2); 0 spikes in all 9; activation-frame excess 0.013 / 0.000 / 0.007 / 0.000 / 0.000 / **0.543** / 0.011 /
  0.000 / **0.538** (s1240 holds the same s 1260 crossing).
- 05:00-05:05 b67 investigated: re-captured with the default (v2) and with -facade-v 1. v2 deterministic (0.5426 again), v1 PASS.
  Frame-to-frame mean |diff| around the crossing (frame 300 = s 1260): 286 3.50 ... 298 3.95, 299 4.00, **300 3.99**, 301 4.05,
  302 4.00, ... 315 3.95 - a smooth rise while the camera turns through C3 (centre s 1260), no jump at the crossing; the largest
  64 px blocks stay at the right edge (content entering the turn). sequences.py measures the excess against the median of the
  previous frames, so a rising diff curve reads as an excess; the harness's own excess (sequence.json maxActivationExcess) is
  0.206 (v2) / 0.117 (v1). Verdict: no activation pop; the v2 textures raise the per-frame diff of the turn. QA-7 in the critique
  (measure the excess against a centred median). Frames deleted.
- Process slip (mine): chain2 passed the capture folder to encode_test, which only scans seq_* subfolders, so the KS04-axis
  clip was skipped and deleted unencoded; TaskStop on the wrapper left the bash loop alive (the KS10-abyss clip was deleted too).
  I stopped my two chain2 bash processes (pids 21924 / 29576; the running player of my own was left to finish) and restarted
  the chain with the clip folder passed directly (chain2b).
- 05:06-05:13 encode test, 1080p clips (600 frames 60 fps) through x264 CRF 23 (encode_test.py, --region from regions_default
  where the clip has no seq_ entry; frames and work folders deleted after each). 14.7 H1 (6) step gate (visible steps <= 2 / 1000
  px, none > 1.5 codes) and the 11.3 BandScore:
  - enc_beat01_slit (KS02, 912,0-1027,227): source 0.233 PASS; decode 0 steps, BandScore 2.17 (run 52 px) -> score 1.449 FAIL
    on BandScore only (PA-09 measured 1.82).
  - enc_beat02_axis (KS04 sky, 806,22-1056,238): decode score 2.994 FAIL (report lost: my commit step removed the stale SKIP
    report at the moment the new one was written; re-run queued after the perf runs).
  - enc_beat03_abyss (KS10, 691,43-998,238): source 1.311 (0 steps); decode **5.13 visible steps / 1000 (max 1.04 code)**,
    BandScore 9.06, run 50 px -> FAIL on the step gate: the empty abyss haze bands after x264 at 1080p (INT-15).
  - enc_beat07_plaza (KS17 plaza, 192,929-1632,1048): source already 8.40 steps / 1000 (score 2.0: granite texture edges), decode
    8.40 / BandScore 3.32 -> the rect measures texture, not banding (QA-6).
  - enc_beat01_wall (KS00 alley wall): 0 steps on source and decode; BandScore 14.4 / 56.7 on a textured wall with a lamp (QA).
  - enc_beat07_goal (KS19 tenement wall): PASS, decode 0 steps, BandScore 0.12.
  - enc_beat06_wall (KS16): no gradient region for the clip (WARN only).
  The 4K CRF-23 test of 11.12 / LOOK_V2 7 was not run (a 600-frame 4K clip is 4-6 GB; W: 19-25 GB free).
- 05:23 INT_autotest_cinematic (-autotest -cinematic): PASS 21 / 21, trigger 599.050 s (599.05 +- 2), stop 601.450 s, ending
  17.00 s, beats 7 / 7 in order, crossings 48.250 / 160.250 / 224.250 / 352.250 / 432.250 / 504.250 (runner within 4e-5 s),
  roll max 5.1e-6 deg, 0 safety snaps, 0 camera-sphere contacts, coverage 48 204 rays 0 misses, 0 errors, GC 32.2 B/frame
  (excess 0.18), load 1.78 s, real 619 s. Copy QA/reports/INT_autotest_cinematic_r01.json.
- 05:40 INT_autotest_walk (-autotest, 1.5 m/s): PASS 21 / 21, trigger 998.100 s, stop 1002.100 s, ending 17.00 s, crossings
  0 / 80.100 / 266.767 / 373.433 / 586.767 / 720.100 / 840.100 / 998.100 (all exact), roll 0, 0 snaps, 0 contacts, coverage 0
  misses, 0 errors, GC 31.7 B/frame (excess 0.0), load 1.78 s, real 1017.6 s. Copy QA/reports/INT_autotest_walk_r01.json.
- 06:15 INT_wallbot_walk (-wallbot, 1.5 m/s): PASS 20 / 20 - both passes 0.40 m inside the left / right lane edge on 99.8 % of
  frames, 0 frames beyond the half-width + 1 mm (max excess -295.7 mm at s 121.6), progress monotone, eye-height error 0.0 mm,
  lowest feet y 0.000, lane reach 28 probes at 0.0 mm, PathGuide kills 0, coverage 0 misses, 0 errors, load 1.81 s. Copy
  QA/reports/INT_wallbot_walk_r01.json.
- 06:38 INT_wallbot_brisk (-wallbot -brisk, 2.5 m/s): PASS 20 / 20, max excess -292.8 / -293.1 mm, edge followed on 99.8 %,
  lowest feet y 0.000, lane reach 28 / 28 at 0.0 mm, coverage 0 misses, 0 errors. Copy QA/reports/INT_wallbot_brisk_r01.json.
  Ray-grid coverage: 48 204 rays, 0 misses in every run (smoke, both autotests, both wall bots).
- 06:39-07:23 performance through gpu-guard (exclusive GPU turn, vSync 0, -cinematic -perf-log -quit-after 610, the whole walk):
  | run | avg fps | p99 ms | 1 % low | max ms | frames > 33 ms | GPU avg / median / p99 ms |
  | 1080 r1 | 216.9 | 49.0 | 12.2 | 228.6 | 2 140 | 2.94 / 2.64 / 5.59 |
  | 1080 r2 | 274.6 | 24.5 | 20.3 | 1 048.9 | 1 100 | 2.85 / 2.63 / 5.27 |
  | 1080 r3 | 275.2 | 24.9 | 20.3 | 150.6 | 1 178 | 2.88 / 2.62 / 6.54 |
  | median 1080 | 274.6 | 24.9 | - | - | 1 178 | 2.88 / 2.63 / 5.59 |
  | 4K (-perf-rt 3840x2160) | 105.5 | 28.7 | 19.1 | 523.5 | 490 | 9.00 / 8.32 / 16.56 |
  GPU per beat (r1 median / p99 ms): b1 2.06 / 4.31, b2 3.46 / 5.69, b3 2.56 / 4.89, b4 3.43 / 6.17, b5 3.33 / 5.42, b6 2.13 / 3.42,
  b7 2.45 / 3.71. Budgets (TEXTURE_PLAN 9): 1080 average >= 90 fps PASS, GPU total 8.2 ms PASS (p99 5.6); 4K average >= 60 fps
  PASS, GPU 16.5 ms PASS on average / median (p99 16.56 at the limit). "No frame above 33 ms" FAILS on this run, but not by the
  game: the slow frames are spread evenly (about 2-4 per second over the whole walk, all beats); in them the main thread takes
  1.0-1.5 ms and the GPU 2-4.5 ms (r2's worst, 1 048.9 ms at s 1255.5: CPU main 1.07, GPU 2.28). During the runs three ffmpeg
  processes of other sessions held about 65 % of the 16 threads and a dotnet compiler 15 % (sampled 06:55; gpu-guard guards the
  GPU only). A1's quiet-machine run had 1 frame above 33 ms. To be re-measured on a quiet machine (lead).
  VRAM (Tools/vram-sample.ps1): 1080 player dedicated peak 663 MiB, total +886 MiB over the 2 710 MiB baseline (<= 4 GB);
  4K 1 622 MiB, +2 337 MiB (<= 6 GB). Texture VRAM 374.7 MiB (TextureTests, TP). Load 1.78-1.81 s (<= 15 s).
- 07:25 enc_beat02_axis re-run: source 0.572 PASS; x264 CRF 23 0 visible steps (step gate PASS), BandScore 4.49 (run 49 px) ->
  score 2.994 FAIL on BandScore only. Crops at 100 / 200 % and x8 looked at: no band visible; the static dither turns into
  macroblock noise, rain streaks vanish, red aerial tail lights smear into dark-red blobs (4:2:0).

## Critique
- QA/critique-lookv2-01.md: S1 1, S2 15, S3 26 (+ 7 QA items, 2 fixed by INT). CRIT0's 30: fixed 10, partly 16, open 4.
  GB-01..GB-20: fixed 14, partly 3, open 3. Top items: the empty abyss (S1), the flat wall of signs, no highlights in beats 3-5,
  the dark garden, empty street level, the abyss banding after x264, the formless cloud deck, the alley window cards.

## Open requests in other owners' files (not carried by INT)
- TQ's REQUEST LEAD Tools/run-player.ps1 (texpans / dolly routing, -capture-ss as 4K) with the matching ShotPlan / QaTests edits.
- GE's REQUEST lead PlayerController / ProgressDirector (-beat N start 1e-5 m short; OrderOk from beat 1).
- FO's REQUEST SU MaterialFactory.Surface.cs FloorMoss puddle fraction 0 (FO's puddle-free copy works; no picture change).
- Lead decisions asked by packages: GE KS13 luma window (obsolete: KS13 36.4 is inside LOOK_V2's 28-48), PA beat-1 |R - G| <= 6,
  D-KO-5 koi spine, SU-05 wall-plane gain, FA-D18 floor-line gate.

## Folders kept
QA/captures/lookv2_01 (keyshots1080, final), lookv2_01_ksv1 (-facade-v 1 A/B), lookv2_01_4k (7 money shots), lookv2_01b (beats),
lookv2_01tc (texclose), lookv2_01ms (material sheet), lookv2_01fs (facade sheet), lookv2_01seq (dolly / koi montages, rain frame).
Every sequence folder was deleted after its numbers were logged. W: 25 GB free at 07:30.

INT DONE 2026-11-16T07:32:32+01:00
