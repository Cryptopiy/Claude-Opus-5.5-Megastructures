# QA log (lead-QA builder, Nocturne fix wave, copy .worktrees/integ, branch n6-integ)

Role: the gates themselves (Logs/fix/QA.md, panel p4_01 C19-C24, C47 + the requests addressed to QA). Files owned this wave: Tools/qa/**
except lookv2_check.py (AT; QA carries the KS00 line and the 21:9 ruling), Scripts/Runtime/QA/** except NocVisibility.cs /
Visibility*.cs (HT), Tools/run-player.ps1, Tools/lookdev.ps1, Tests/EditMode/QaTests.cs and PathScheduleTests.cs, this log,
QA/critique-QA.md. No commits (the lead commits).

## FIX WAVE (2026-11-21, from master caddb90)

- 01:57 start. W: 17 GB free at start, 6.7-14 GB during the wave (other copies write too): every capture runs one sequence at a
  time and deletes its frames after analysis (Tools/qa/run_seq_clips.sh).
- 01:58 QA-b01 build into Builds/QA on the untouched merge: Succeeded, 0 errors (Logs/QA-b01.log).
- 02:06-02:08 QA-t01 / QA-t02 (QaTests|PathScheduleTests): 52 / 52 PASS after the new tests (QA/reports/QA-t02.xml).

### Harness (C#)
- ShotPlan.cs: SeqMode.Cinematic (C23): CinematicTrace(fps) runs the RUNTIME WalkPlanner in Cinematic mode from rest at s 0
  (GazeSchedule, follower omega 2.5 with the 22 / 18 deg/s limits, the seeded sway with seed 1 = CliArgs.Seed's default) at
  1 / fps steps to 1 s after the stop, cached per fps; CinematicSeq cuts a window into fixed per-frame poses (s, corridor point at
  eye height, yaw / pitch with sway, vFOV(s)), the ambient clock = the walk time, boundaryS = the 14.6 boundaries crossed. Sets
  "cinematic" (30 fps) and "cinematic60" (60 fps): windows s 96-130, 260-340, 380-440, 530-590, 640-730, 860-900, 950-1010,
  1050-1110, 1240-1300, 1490-ending (seq_cin_sNNNN / seq_cin60_sNNNN, kind "cinematic").
- ShotPlan.cs: DR's "dread" set (C22, DR's request with kind "dread" instead of "traffic" so the photosensitivity gate runs):
  seq_dread_ks08_hull / ks09_hull / ks13_shadow / panel as DR wrote them, plus cinematic windows over every sheet lightning
  (s 205, 345, 553, 790, 1140, 1420: s - 5 .. s + 10) and both darkness waves (s 300, 1330: 75 m = 30 s).
- ShotPlan.cs: AL's two texclose poses tc_empty_room_e / tc_empty_room_w (exact values of AL's request).
- ShotPlan.cs: RunPlayerRoute classifies a comma list of sets per set (mirror of run-player, HT's request); HeavySets += dread,
  cinematic, cinematic60; CapturePlanOptions.encodeSeconds (-encode-seconds S, C19: 4K clips of 2 s = 120 frames on a full disk).
- CaptureHarness.cs (C20): texpans / dolly without -freeze-clock -rain 0 are REFUSED (were a Note); the facadesheet set without
  -facade-sheet is refused; sequence.json gains rainNoiseOff and facadeSheet; -encode-seconds parsed.
- AutoTestRunner.cs (HT's request): -autotest-until >= PathLength is no until (EffectiveUntil).
- QaTests.cs: texclose 10 poses (the empty rooms: in the lane, facing the window wall, dist = horizontal distance to that wall);
  Cinematic_WindowsFollowTheRuntimePlanner, Dread_SetHasTheWalksAndTheLightEvents, AutoTestUntil_PastThePathEndIsNoUntil,
  RunPlayer_SplitsSetListsAndGuardsTheDisk.

### Tools
- run-player.ps1: HT's comma split of -capture-set lists; disk guard (C24): refuses to start under -MinFreeGB 5 (exit 94), kills a
  running player under -AbortFreeGB 1.5 (exit 94); adds -facade-sheet to a facadesheet set (C20); heavySets += dread, cinematic,
  cinematic60. The cap of 3 players (2 with 4K) and -BuildDir are unchanged.
- sequences.py + new seq_extra.py (C20-C24): run-condition refusal (texpan / dolly / static / pan need -rain 0 -freeze-clock);
  "pan" gets the texpan shimmer computation; the screen-space shimmer gate leaves out minimum-width line blocks (ridges of the ss2
  mean, D-037 14), which stay gated on the stabilised residual; required gates (texpan / pan shimmer, signs / dread G1, rain H5)
  that did not measure make the sequence SKIP and the report's Result SKIP, never PASS; block pops and blinks at 32 / 64 px
  (> 3 x both neighbouring pairs and > 2 levels) with rain-streak and motion-explained events excluded (motion search +-64 px);
  photosensitivity G1-G7 (R4 3.3) on signs / dread / cinematic (G5, G6, G7 p99.9 and the 4 %/s wall rule gated on a static camera,
  INFO when the camera moves); H5 rain streak count / coverage / minimum length (needs -rain 1 -rain-noise-off); a montage per
  sequence (every 10th frame, 4 x 6 pages), the worst pop blocks at 4 x, an x264 clip for boundary / cinematic / dread, then
  --delete-frames deletes the frames and writes analysed.done.
- run_seq_clips.sh (new): capture -> analyse -> montage -> delete, one sequence at a time.
- encode_test.py (C19): the gate is x264 CRF 23 (CRF 30 / VP9 = INFO regression rows); per-region Y / Cb / Cr steps / 1000 and max
  code; an H1 (6) clip without a region FAILs, any other SKIPs, Result PASS only when every clip measured; the last decoded CRF-23
  frame of enc_beat01_wall / enc_beat07_goal goes to QA/encode/lookv2_encoded and lookv2_check --encoded runs on it.
- qa_common.py: strip_metrics / band_score report per-channel steps (Y, Cb, Cr, R, G, B); "units": "frac169" (a 16:9 rect projected
  to the image's aspect at the same vFOV).
- regions_default.json (C19, C47, AT's request): every enc_* clip carries its key shot's rects (match lists); KS10 abyss [0.40, 0.04,
  0.51, 0.22]; KS12 abyss on the visible drop [0.41, 0.53, 0.51, 0.68]; KS16 wall_panel on one open panel [0.14, 0.53, 0.20, 0.62] as
  frac169 (one rect for 16:9 / 16:10 / 21:9); enc_beat05_crown crown_gap, enc_trans_mouth mouth_dark_wall.
- image_metrics.py (C47, HT's request): thin-line masks (components of the loose outlier masks against the 9 x 9 median, length
  >= 3 px, 7 x 7 density <= 55 %) for the NaN-like and isolated tests; dark single-pixel specks that are not black (max channel
  > 12) are a WARN inventory, not NaN; owners by beat on the remaining FAIL lines.
- run_encode_clips.sh rewritten for Nocturne's 11 clips, clip by clip, the H1 four also at 4K (-capture-4k -encode-seconds 2),
  NODITHER=1 for the undithered comparison, lookv2 --encoded at the end.
- lookv2_check.py (AT's file, QA carries the ruling): KS00 window (16, 31) (D-039 5); a frame wider than 16:9 is measured for the
  mean-luma window on its central 16:9 part (the 16:9 key shot itself).

### Runs on this build (Builds/QA b03 / Builds/QA2 b04: same C# as the final b05; reports in QA/reports, montages inside each
### QA/captures/<run>/<seq>/; frames deleted after analysis, analysed.done markers)
- 02:10-02:31 C21 proof, b23 (s 385-415, Walk 1.5 m/s, clock + rain as p4_01): 540p first (QA_bnd01: 3 078 pops, sub-pixel lights
  and rain at 540p; a pop gate needs 1080p), then 1080p (QA_bnd02): the worst event is the critic's C13 exactly: pop f152 s 392.6,
  32 px block (992, 544), 13.5 levels vs 0.25, and f153 (896, 544) 13.3 vs 0.25: AB's balustrade step lights switch on in one frame
  (QA/captures/QA_bnd02/seq_b23/pops_crops.png, looked at). Sampled mid-level events (scratch crops, looked at): street-lamp heads
  whose glare switches on / off between frames (f37, f578, blocks x 1152-1216 y 480-512), rain streaks, 1 px row steps of thin strips.
  Montages QA_bnd02/seq_b23/montage_01..03.png, clip.mp4.
- b12 (QA_bnd/seq_b12, 1080p, s 105-135): frame 1 flagged (C14's transom frames start there), worst pops f540 s 132.0 (several 32 px
  blocks at y 576-640, 6-9 levels vs 0.25: a row of lights switching on). 222 pops / 81 blinks after 294 rain events excluded.
- Exclusions added after looking at the crops (each verified): rain streaks (steep elongated changed components; b23 626 excluded),
  motion (+-2 px min / max envelope: a 1-2 px strip stepping a pixel row under the sway; +-64 px block search: a near sign sweeping
  through a block). Synthetic check: a strip stepping one row is excluded, a lamp switching on stays (scratch test, 03:01).
  Windows analysed before the envelope test (b23 QA_bnd02, b12, cin s96 / s380 / s640) still count those sub-pixel steps;
  b23 / b12 re-run with the final code: QA_bndF (see below).
- Frame 1 after the pose jump is a harness start transient (C20; dolly_face30 frame 1 block 30, 52.6 vs 3.0 under -rain 0
  -freeze-clock): listed as INFO "start transient", not gated (sequences.py dolly and seq_extra pops).
- 02:38-03:00 texpans at -rain 0 -freeze-clock + the -capture-ss 2 reference (QA_tp, QA_tpss2; sequences_QA_tp.md). Screen-space
  excess over ss2 (32 px block max, line blocks left out) / stabilised excess, limit 2.0: KS00 1.91 / 1.54 PASS, KS04 6.06 / 1.94
  FAIL (13 % blocks picture-mismatched vs ss2), KS05 5.26 / 1.71 FAIL (13 %), KS08 2.43 / 2.30 FAIL, KS09 2.01 / 2.55 FAIL,
  KS15 0.87 / 1.86 PASS, KS17 2.44 / 2.74 FAIL; swimming p95 0.10-0.25 px (<= 0.5) everywhere.
- 03:01 facade sheet with -facade-sheet (added by run-player) -freeze-clock -rain 0 (QA_fs): "built ZZ_FacadeSheet in 5 ms, 264
  triangles" (Logs/player-QA-fs.log line 77; p4_01fs had 0); floor-line gate 2 FAIL / 331 faces (fs300b O0 45 face 1 ac 0.162,
  fs300c SEC 45 face 0 ac 0.118 at 17 px; p4_01fs: 92 FAIL on fog). Contact QA/captures/QA_fs/_contact/contact_QA_fs_01.png looked
  at: the sheet towers stand in every pose; three 30 m subjects (O0, S1, S2) show unlit faces (FA).
- 03:03-03:07 dolly at -rain 0 -freeze-clock (QA_dolly): face30 and graze PASS every gate; local pops WARN 12 / 17 (face30's are the
  frame-1 transient; graze's are block 450 at frames 128-139, 842-869 m: one recurring spot).
- 03:03 / 03:07 rain H5 (-rain 1 -rain-noise-off, QA_rain2): flicker outside streaks 4 levels PASS (<= 6); streak gate FAIL: 2 592
  drop components / frame (140-210) at > 4 codes, coverage 1.50 % (0.6-1.0), 89 % shorter than 10 px (p5 2.8 px); at the 1.5-code
  flicker threshold 2 758 / 3.43 %. NocRain draws many more and shorter drops than 14.7 H5 states: FX.
- 02:35-03:30 cinematic windows at 2.5 m/s, 30 fps, 1080p (QA_cin, looked at: s96 / s380 pop crops, s260 montage_02): s950 PASS;
  s96 luma step 1.51 levels at frame 143 (s 107.9: the gaze pitches down off the bright mouth sky, mean 43 -> 30 in 14 frames);
  s260 G1 28.5 % at frame 259 (s 281.6, the look-up over the lit street and tram; limit < 25 %); s640 G2 25.3 % (the swing + dive,
  limit < 25 %); pops / blinks after exclusions: s260 9 / 224, s530 171 / 870, s860 7 / 20, s1050 0 / 1, s1240 0 / 17, s1490 5 / 40.
- image_metrics on p4_01 with the line mask (image_metrics_QA_im_p4_01.md): NaN-like 139 -> 70 (all bright single pixels: KS14 21,
  KS15 18, KS04 17, KS01 10, KS19 8, KS09 8, KS16 7, KS05 6, KS06 4, KS10 4, KS12 4 ...), 45 dark non-black specks now a WARN
  inventory (KS19 mortar 22 among 70-90), isolated 74 -> 6 (KS18's 51 spire-rib pixels and KS04's pendant gone; left: KS07 1,
  KS09 1, KS11 2, KS14 1, KS15 1). 24 FAIL lines (was 74).
- lookv2 on p4_01 (lookv2_QA_p4_01.json): KS00 30.5 now PASS in (16, 31); 14 / 20 pass (KS00 texture meso 0.93, KS01 mean 39.5,
  KS02 texture / pools, KS13 p99 83.9, KS14 p99 107.9, KS15 dark cast R-G -4.76: picture owners AL, AB, GD).
- selftest.py: 115 assertions, 1 failed: "KS04 undithered banding FAIL" scores 0.204 with the original tools too (pre-existing,
  scratch comparison 03:23); the new run-condition and pan-SKIP assertions pass.
- 03:31-03:37 C21 proof with the FINAL analysis (QA_bndF, b23 1080p; sequences_QA_bndF_seq_b23.md): pops 1 874 -> 27 after the
  exclusions (636 rain, 2 023 motion), the worst two are C13: f152 s 392.6 (992, 544) 13.5 vs 0.25 and f153 s 392.65 (896, 544)
  13.3 vs 0.25; blinks 461 (the top ones are near rain streaks crossing bright strips that the rain test still misses: f237, f474,
  f259, crops looked at in QA_bnd02/seq_b23/pops_crops.png); start transient f1 listed apart. Montage + crops + clip kept in
  QA/captures/QA_bndF/seq_b23. b12's re-run was killed by the commit limit (below): its frame-1 flag is from QA_bnd (02:38).
- 03:33 MACHINE: Windows commit charge exhausted (free virtual 3.1 GB of 69.5 GB while 25 GB physical were free: about ten Unity
  editors and the players of every copy). numpy "Unable to allocate 1.98 MiB", bash fork failures: the signs analysis, the dread
  chain and the b12 re-run died; I stopped my chains (no parallel analyses any more) and ran what was left one at a time.
- Disk / cleanup: superseded folders deleted (QA_bnd01 540p, QA_rain, QA/tmp/selftest 99 MB); Builds/QA2 (the parallel build used
  by the chains while Builds/QA was busy) deleted at the end.
- 03:40 run-player disk guard checked: -MinFreeGB 9999 -> "[NOC-FAIL] refused: 9,2 GB free on W:\ (< 9999 GB) ... (run-player exit 94)",
  exit 94, no player started; PowerShell parser: 0 errors in run-player.ps1. The facadesheet run's log shows the added
  -facade-sheet (Logs/player-QA-fs.log line 30).
- Encode clip by clip (QA_enc, Builds/QA2; 1080p clips at -encode-seconds 4 = 240 frames, the H1 four also at 4K at 2 s = 120
  frames, NODITHER on the 4K ones; encode_summary_QA_enc.md, encode_verdict_QA_enc.md): every clip now has a region; steps-only
  decode verdicts (14.7 H1 (6)): slit 0.00 PASS, axis 1080p 9.22 / 1000 Cr FAIL, axis 4K 2.31 Cr (max 1.08) FAIL, axis 4K undithered
  2.31 Cb FAIL (dithered not better on this measure), sky 0.00 PASS, abyss 1080p 0.00 PASS, abyss 4K 2.56 (R/G/B only, max 1.01)
  FAIL, undithered 2.56 (Y) FAIL, void 0.00 PASS, crown 0.00 PASS (BandScore 2.8-17 reported, not gated on a decode). The first
  five reports were written before the steps-only decode gate (their per-check score is the old combined one);
  encode_verdict.py recomputes all from the stored numbers.
- 03:42 FULL EditMode suite on the final tree (QA/reports/QA-final.xml): 469 tests, 468 passed, 0 failed, 1 skipped.
- 03:32 FINAL BUILD QA-b05 into Builds/QA: result=Succeeded errors=0 warnings=4 (Logs/QA-b05.log), no shader error.
- 03:33-03:47 first smoke (-autotest -brisk -autotest-until 1500) hung after "AutoWalker AutoTest from s 0.00" (log last written
  03:33:25, the commit-limit minute) and was killed at the 14 min timeout: not a verdict; re-run as QA-smoke2 (below).
- 03:43 AL's empty rooms (QA_tc, texclose -capture-only tc_empty_room_e,tc_empty_room_w -freeze-clock; looked at): both rooms read
  head-on through their windows (table, cup, chair, picture, curtain; the east one beside its door under the wall lamp). DONE QA
  AL tc_empty_room_e / tc_empty_room_w.
- 03:44-03:50 signs, KS16 static, 60 s at 30 fps from t 445 (SG's request), 960 x 540 (8 px photosensitivity blocks = 16 px at
  1080p; disk) (QA_signs; sequences_QA_signs_seq_signs_ks16.md; montage_01..04 looked at: the wall's slow colour waves, no flash):
  G1 0.14 % PASS (< 25), G2 0 / G3 0 / G4 0 PASS, G6 0.056 % / frame PASS (<= 0.3), G7 p99.9 red share 0.617 PASS (<= 0.70),
  wall change 2.61 % / s PASS (<= 4), G5 FAIL: 15 of 3 658 modulated blocks peak above 0.5 Hz (max 2.55 Hz) and the per-bin RMS
  above 0.5 Hz reaches 0.00243 (<= 0.002): SG (or rain on bright blocks: the R4 estimate was 2e-4 per bin).
- DONE QA requests: AL (texclose rooms), AT (KS10 rect + enc_beat03_abyss region), DR (dread set, fix-wave version, kind dread),
  HT (run-player comma split; -autotest-until >= PathLength = no until; image_metrics thin-line mask), SG (signs from t 445),
  AB (_NocPxScale = the -capture-ss factor, set by CaptureHarness), lead (KS00 (16, 31); 21:9 mean on the central 16:9 part).
- 03:51-03:56 dread (QA_dread; DR's fix-wave sequences, kind dread, 1080p): seq_dread_hull_pass (289 frames at 60 fps): G1 1.22 %,
  G2-G4 0, G7 0.621: photosensitivity PASS; 19 pops (worst f4 s 544.2 7.3 vs 1.34 at the lower left). seq_dread_lightning_s0553
  (cinematic, 30 fps): G1 6.19 % PASS, G2-G4 0, G7 0.601; luma step 0.51 PASS; 59 pops / 187 blinks: the crops (looked at) show
  warm light strips vanishing for ONE frame and returning (f51, f53, s 552.3-552.5, 18-22 levels) and gallery lights blinking at
  f43: a real strobing defect for DR / AB / FA (critique QA-F14). The other dread windows (sheet lightning 205 / 345 / 790 / 1140 /
  1420, darkness waves 300 / 1330, hull sink, shadow, panel, cabins) did not run: the 03:33 commit limit killed that chain; the
  set and the job script are ready (Tools/qa/run_seq_clips.sh dread <run> <names>).
- 03:51-04:02 SMOKE on Builds/QA (QA-smoke2: -autotest -brisk -autotest-until 1500, i.e. HT's rule: no until, the end checks run):
  "AUTOTEST PASS mode=brisk trigger=598.900s stop=601.300s (expected 598.90 / 601.30) ending=17.00s beats=7 errors=0"
  (QA/reports/autotest_brisk.json, Logs/player-QA-smoke2.log); corridor coverage 48 204 rays, 0 misses; steady allocation 33.5 B /
  frame (engine input box 32). The one "error-like" line of run-player is that PASS line ("errors=0").
- Encode, the rest (encode_verdict_QA_enc.md): wall 1080p / 4K / 4K undithered 0.00 PASS (the open-panel rect); plaza 1080p plaza
  8.33 FAIL, 4K sky_koi_spire 3.29 (Y, max 1.24) FAIL, 4K plaza 4.18 coherent (the KS17 plaza is gated on coherent steps; the Y / Cb /
  Cr columns of a "steps" region show the raw per-channel counts) FAIL; undithered equal counts. So the H1 (6) four at 4K CRF 23:
  KS04 FAIL (2.31 Cr), KS10 FAIL (2.56 in R/G/B only; Y / Cb / Cr 0), KS16 PASS, KS17 FAIL: AT / HT, with the numbers.
- 04:06 encode tail: goal 0.00, beat01_wall 0.00, trans_mouth 0.00 PASS (x264 CRF 23; trans_mouth's VP9 regression row 1.85 Cb at
  4.32 codes, INFO). lookv2_check --encoded (QA/reports/lookv2_encoded_QA_enc.json): KS19 PASS, KS00 FAIL (meso 0.55 < 0.75, pools
  0.04 / 0.38; the source KS00 meso is 0.93): AL / AT.
- 04:06 21:9 check (QA_ks219, keyshots -capture-only KS03 / KS10 variants, lookv2_QA_ks219.json): 8 / 8 PASS; KS03_219 25.1 and
  KS10_219 33.2 = their 16:9 shots (25.1 / 33.2): the windows were right, the measurement took the 21:9 side strips. Not a picture
  problem, no owner.
- Kept (867 MB, no frame files): QA/captures/QA_bnd, QA_bndF, QA_bnd02, QA_cin, QA_dolly, QA_dread, QA_enc_*, QA_fs, QA_rain2,
  QA_signs, QA_tc, QA_tp, QA_tpss2, QA_ks219 (montages, crops, clips, sequence.json, stills); QA/encode/QA_enc_* (decode crops),
  QA/encode/lookv2_encoded. Builds/QA2 deleted. W: 9.1 GB free at the end (others write too).
- 04:07 fastcheck OK (217 / 287).
- Not done (open, see critique-QA.md): perf at 1080p / 4K (the lead runs it; perf_windows.py reads its CSV), the 4K texpans and
  dolly (the lead's 4K budget), most of the dread windows, cinematic60, a per-pixel temporal rain mask for the block gate (QA-F12),
  b12 with the final analysis.
QA DONE 04:08
