# TR log - beat 4 (Skybridge): the trains T1-T7 and the crossing (Phase 4 wave B)

Owner: TR (new in wave B, started 2026-11-20 20:34). Copy: `.worktrees/waveb` (branch n6-waveb), partners AL, GD, AU.
Files: `Assets/_Game/Beats/Beat04_Skybridge/**` except `SkybridgeLamps.cs` (AB), new `Shaders/Beats/NocTrain.shader`,
`Scripts/Tests/EditMode/Beats/Beat04SkybridgeTests.cs`, new `TrainTests.cs`, this log, `QA/critique-TR.md`. Build `Builds/TR`.

## Start (20:34-21:20)
- Read: MASTER_PROMPT 14.5.4 / 14.7 H7 / 14.10, LOOK_V2 (beat 4, KS11-KS14 rows, 6 row 4 "2 train lights SignWhite r 25 i 3"),
  D-031..D-034, DREAD 4.3, critique-lookv2-01 (INT-16, INT-19, INT-40), G3 / GE logs for beat 4, BV's tram (TramSchedule,
  TramDriver, NocTram, read-only), GE's / AB's face-tube portals (CanyonBridge, NotchGate keystone, GulfPortal), the visibility
  audit's KS11-KS14 subjects (VisibilitySubjects: the train body is a 3.4 m box about the tube axis), AU's vehicle table
  (NocturneScore.Vehicles).
- Requests for TR in every copy's logs (grep "REQUEST TR"): none.
- TR-b01 (baseline build, HEAD + nothing of mine): OK, 0 errors (Logs/TR-b01.log). TR_r00 KS11-KS14 1080p looked at:
  - KS11: the N tube is an empty transparent tube (no train: INT-12), the S tube on the right too; rings of FX's glass frames.
  - KS12: the N tube crosses the upper frame as big elliptical frame rings; drop and layers below.
  - KS13: through the strip the truss members and the abyss with traffic rows; the lower tube (x ~870 px) shows as a vertical
    chain of frame ellipses left of the centre member: T6 will be there.
  - KS14: INT-40 confirmed: both tubes read as grey glass capsules whose ends at gate B look like rounded pills (Fresnel at
    grazing, no frame where the tube meets the block, the blind 6 m tunnel lit grey).

## Plan (in order)
1. Trains: TrainSchedule (pure, T-TRAIN), TrainBuilder (mesh, guideway, linings, lights), TrainDriver (per frame, capture mode),
   NocTrain.shader (NocTram-derived; darkness behind the mouth planes + clip planes at the tunnel ends), tests (TrainTests).
2. Tubes / gates: gate tunnels 6 -> 11.9 m drawn black (NocTrain lining), collars at the gate mouths and steel rib bands over the
   saddles (INT-40 / INT-16 geometry side), G3's NocLine rails replaced by the guideway beam.
3. Build, capture KS11-KS14 + look-back / side poses, look, fix (<= 4 cycles per item).
4. Deck / rails / truss moire and scale check; exposure (wind, rain) ideas within my files.
5. Shared requests (FX: bridge sway D-034 4.3 numbers, glass interior), final build, smoke, EditMode suite, report.

## Design notes
- Timing (14.5.4): x(tau) = x0 -+ 38 tau from the s-keyed launch; capture tau = max(0, (s - sL) / 2.5). Meeting s (2.5 / 1.5 m/s):
  T1 601.98 / 595.25, T3 653.59 / 648.17, T4 699.29 / 695.71, T2 759.14 / 756.39, T7 816.29 / 809.51 -> differences 6.73,
  5.42, 3.58, 2.75, 6.78 m (<= 7). T6 is not a meeting: nose under the strip centre x 531.3 at s 715.79 (2.5) / 707.47 (1.5),
  both while the walker is on the strip (s 706-726), so the 8.3 m difference does not move it off the strip.
- Body centred on the tube axis (3.4 x 3.4 m, y axis +- 1.7): the spec's KS11 nose box (x -0.14..-0.06, y -0.12..+0.04) is
  computed for that body (eye 1.7, pitch +2, 32 m: y -0.117..+0.037), and VisibilitySubjects uses the same box. So the train
  rides a guideway beam (0.9 m wide, top 2.55 m under the axis) with magnet bogies that wrap its top: a maglev in a glass tube,
  never floating.
- Never a pop: NocTrain multiplies the shaded colour by 1 - smoothstep(0, 2.5 m, depth behind the nearer mouth plane) and clips
  at the tunnel ends (3 cm behind the end discs); the gate tunnels and the face-tube linings use the same material, so a mouth is
  a black hole, the waiting train (nose on the portal plane) is invisible and the done train is clipped. Lights fade in over
  30 m after the nose has left the mouth and out before the destination mouth.

## New files
- Beats/Beat04_Skybridge/TrainSchedule.cs, TrainBuilder.cs, TrainDriver.cs; Shaders/Beats/NocTrain.shader;
  Scripts/Tests/EditMode/Beats/TrainTests.cs; QA/critique-TR.md.

## Progress
- 21:05 NocTrain.shader offline fxc (scratch tr/shcheck.py, AL's harness: every pass, base / INSTANCING_ON / additional lights):
  all OK, FAILED 0.
- 21:20 fastcheck: my files compile; the copy fails on AL's in-progress Beat01_Alley files (AlleyLayout.AcZ, NoodleBar.Lamp0,
  AlleyRooms ...), so Unity calls wait for AL.
- 21:30 fastcheck all OK (runtime 209, editor 278) after AL's fix. New BridgeSway.cs (D-034 4.3 as a pure function; FX adds one
  line to CameraRig). Queued TR-r01 RebuildAll (NocTrain into GraphicsSettings' always-included list) then TR-b02.

## Requests (for owners in other copies; also in the final report's shared_change_requests)
REQUEST FX: Assets/_Game/Scripts/Runtime/Player/CameraRig.cs, ApplyPose(), D-034 4.3 bridge sway (2-3 cm lateral at 0.12 Hz,
  gust-scaled, no roll; numbers and phase in Beats/Beat04_Skybridge/BridgeSway.cs, tested in TrainTests.BridgeSway_*):
  replace
      Vector3 pos = player != null ? player.EyePosition : transform.position;
  by
      Vector3 pos = player != null ? player.EyePosition : transform.position;
      // D-034 4.3 bridge sway (TR, BridgeSway): lateral eye translation on the skybridge (z is lateral on the east leg), no roll
      if (player != null) pos.z += (float)BridgeSway.Offset(player.S, NocClock.Time);
  (the override branch for stills is untouched, so key shots stay exact; peak 3.0 cm, lateral speed <= 2.3 cm/s; the creaks of
  AU's DreadVoices.BridgeGust fall on the sway's extremes because both use sin(2 pi 0.12 NocClock.Time)).
- 21:50 TR-r01 RebuildAll SUCCESS (Logs/TR-r01.log; NocTrain guid 06b48f7a... is in GraphicsSettings' always-included list).
- 21:45 train shell made T-junction-free (window sill / head are profile points; walls and chamfer split at the window ends;
  car 0 closes the joint with the nose); Simplified tier keeps T1 / T4 (TrainBuilder.Kept), Graybox none.
- 22:08 TR-b02 started (the queue served AU-t01, GD-b04, AL-b03 first).
- 22:15 TR-b02 OK (0 errors, NocTrain compiled; Logs/TR-b02.log). Player log: [NOC-FAIL] Beat04_train: 2 winding mismatches
  (0.03 %): the nose tip's roof vertices took their outward sense from the tube axis, but the tip's roof lies below the axis.
  Reproduced offline (scratch tr/nose.py: "bad i 3 j 8/9"), fixed (outward from each station's own centre), offline 0 bad.
- 22:17 TR_r01 (TR-b02; KS11-KS14 + 6 poses) looked at, full size and crops:
  - KS11: T1 reads as a train in the tube: nose at x -0.125..-0.06, y +0.02..-0.11 (spec -0.14..-0.06, -0.12..+0.04), two
    headlights, the window strips converging toward gate B, the guideway under it. Problems: the headlights are small white
    dots, not "a bright cool headlight"; a white blob left of the nose (650, 520) = the headlight point light mirrored in the far
    tube wall at eye level, reads as a floating light.
  - KS12: T3 (spec timing: abeam at 653.6, its body over x 371..545 at s 660) crosses the whole upper third 12 m away; the window
    band is a flat grey-white panel (median luma 127, top 149) with grey seat blocks: crude.
  - KS13: T6 visible under the strip as a thin dark line with a nose light (it shows its dark roof from 143 m above); centre
    median 57.5 (30 x 30 %) / 53.6 (30 %-area box), in 40-75.
  - KS14: the tubes now enter framed black mouths (collars), no pill ends; mean luma 30.4.
  - poses: TRback610 T1 slides into gate A's mouth and darkens, no cut visible; TRt4side699 T4's nose overtaking on the right;
    TRt2back762 / TRt7gate819 trains in the tubes with the gate mouths; TRt5up610: T5 is directly over the walk axis (z 195) and
    shows only its dark underside and (head-on) its headlights; TRemerge588: T1 at 226 m is ~2 px headlights.
- 22:25 fixes for TR-b03: the saloon redesigned (ceiling light line, lit upper wall and rack, the far side's dark windows
  between lit pillars, seat backs with lit gaps, per-window variation, mean ~0.62 instead of ~1); roof glazing between the
  roof gear (zone 9) so a train seen from above (T6 at KS13, the abyss views) shows a lit line; headlight lens gain 5 -> 10
  (bloom core visible from 275 m); the headlight point light 4 m ahead and 0.45 m over the beam (its mirror image falls near the
  track). fxc 0 failed, fastcheck OK. TR-t01 (TrainTests + Beat04SkybridgeTests) running, TR-b03 queued.
- 22:31 TR-t01 (TrainTests + Beat04SkybridgeTests): 16 / 18 (QA/reports/TR-t01.xml). Logged numbers: T1 head-on 6.99 s from
  283.2 m; meetings T1 601.981 / 595.254, T3 653.586 / 648.166, T4 699.292 / 695.714, T2 759.142 / 756.394, T7 816.289 /
  809.505 (2.5 / 1.5 m/s); T6 under the strip centre 715.79 / 707.47; bridge sway peak 2.98 cm, mid-span 3.00 cm; beat 4
  88 418 triangles (budget 520 k). Failures, both fixed for TR-b03: the train's x extent -174.11 (the tail coupler cover 11 cm
  proud -> 5.5 cm) and NeverOutsideItsTube timed out (7 x 401 instants x 14 k vertices with an Assert per vertex -> 81 instants,
  counters, one Assert each).
- 22:47 SkybridgeTubes.cs back to CRLF (the diff is the real change only). TR-final (full EditMode) queued.
- 22:54 TR-b03 started (the queue served GD-b05, AL-b04, AU-t02 first).

## Proposed decisions (DEVELOPMENT.md lines)
- D-TR-1 Trains ride a guideway beam, body centred on the tube axis: 3.4 x 3.4 m about the axis (y 0.3..3.7 in the bridge tubes),
  the beam 0.9 m wide, top 2.55 m under the axis, magnet bogies wrapping it (0.21 m gap), G3's two NocLine rails removed. Reason:
  14.5.4's KS11 nose box is computed for that body: at 32.1 m, eye 1.7, pitch +2: atan(-1.4 / 32.1) - 2 = -4.50 deg ->
  tan / tan(33.05) = -0.121; atan(2.0 / 32.1) - 2 = +1.57 deg -> +0.042 (spec -0.12..+0.04); VisibilitySubjects.Train uses it.
- D-TR-2 Mouth darkness instead of a popping clip: NocTrain x (1 - smoothstep(0, 2.5 m, depth behind the mouth plane)), clip
  planes 3 cm behind the tunnel end discs; the bridge-tube tunnels 11.9 m deep (through the 12 m gate blocks) and drawn with the
  same material; face-tube mouths get a r 5.25 lining. A train is enabled only for 0 < tau < HiddenTau (T1: (683 - (372.07 -
  174.5)) / 38 = 12.77 s); at tau 0 every drawn vertex lies behind the origin mouth (TrainTests).
- D-TR-3 "Passes differ by 7 m or less" covers the five meetings (T1 6.73, T3 5.42, T4 3.58, T2 2.75, T7 6.78 m); T6 is a pass
  under the strip, 715.79 (2.5 m/s) vs 707.47 (1.5 m/s) = 8.32 m, both while the walker is on the strip (s 706-726).
- D-TR-4 Train lights: one SignWhite headlight (r 25, i 3, 4 m ahead, 0.45 m over the beam) and WinNeutral wash lights (r 14,
  i 1.0; two per bridge-tube train, one per face-tube train), each 0 at and behind a mouth, smootherstep to full over 30 m
  (headlight, from 4.1 m) / 24 m (wash); at most 7 x 3 = 21 moving lights, all disabled while a train waits or is done.
- D-TR-5 Bridge sway (D-034 4.3): dz = w4(s) sin(pi u) (0.020 + 0.010 (0.5 - 0.5 cos(2 pi t / 17))) sin(2 pi 0.12 t), u = (x -
  375.3) / 300 on the span; peak 3.0 cm, lateral speed <= 2.3 cm/s, no rotation (BridgeSway.cs; FX applies it in CameraRig).
- 23:01 TR-b03 OK (0 errors; Logs/TR-b03.log). This is my final build (23:15 feature stop; the shared queue gave no slot for a
  further cycle).
- 23:02 TR_r02 (TR-b03; KS11-KS14 + 6 poses; QA/captures/TR_r02, sheet TR_r02_sheet.png) looked at, full size and crops:
  player log errors 0 (the train mesh's winding NOC-FAIL is gone). KS11: T1's nose with two white headlights (brighter), the
  strips converging to gate B; a small reflection of the headlight light remains in the near glass (595, 488), reads as a
  reflection now. KS12: the band shows a ceiling line, lit pillars and seat shapes (structure instead of a flat panel), still
  blocky at 12 m (TR-06 partly). KS13: T6 under the strip as a dotted lit line (roof glazing) with its nose light; centre median
  57.7 (30 x 30 %) / 53.7 (30 %-area box), in 40-75. KS14: framed black mouths, mean luma 32.0 (24-40). Means: KS11 29.4,
  KS12 32.1, KS13 37.0, KS14 32.0 (LOOK_V2 windows 24-40 / 30-46 / 28-48 / 24-40: all inside).
- 23:04 smoke on TR-b03 (-autotest -brisk -beat 4 -autotest-until 880): AUTOTEST PASS, errors 0, corridor coverage 48 204 rays
  0 misses, beat 5 entered at s 880.00 t 128.17 (QA/reports/autotest_brisk_from560_until880.json, Logs/player-TR-smoke.log);
  alloc mean 32.7 B/frame = the engine's 32 B input box + 0.7 (4 KB heap pages, as in AL's / GD's smokes), GC 0.
- 23:05 deleted QA/captures/TR_r01 (numbers logged); kept TR_r00 (baseline) and TR_r02 (final). W: has 14 GB free (builds of
  every role; mine Builds/TR 463 MB).
- 23:13 TR-final (full EditMode, QA/reports/TR-final.xml): 447 tests, 439 passed, 7 failed, 1 skipped. Every TrainTests and
  Beat04SkybridgeTests case passed. The 7 failures:
  - MINE (fixed 23:15, see below): Q1IntegrationTests.B (848 capsule hits, every one on Beat04_skybridge/trains/train_T7 / T4 at
    s 394-399, lateral +-11) and AbyssTests.KS10_DropShare (0.298; "near ... trains/train_T7 x1402"): the waiting trains' renderers
    are disabled, but their transforms sat at the origin mouth with the 174 m bodies over the tongue, and the ray probes ignore
    `enabled`; probably also Q1IntegrationTests.A (45 walkable tops off y 0: the train roofs over the tongue).
  - not mine (base state of this copy, the owners' files): Beat07HeartTests.PlazaInlay (HT), BoulevardStreetTests.LightBudget
    (29 > 28) and Shops_Varied (18 > 17) (BV), WetFloorTests.WallWindows_MeanOne (min -1.4e-11 < 0, FX: a rounding sign).
- 23:15 fix: a train is parked at y -4 500 (TrainSchedule.ParkPos, far under anything built) while it waits or is done
  (TrainBuilder's initial transform, TrainDriver on every on -> off switch); its renderer stays disabled. TrainTests asserts the
  park height. fastcheck OK. Not in TR-b03 (the player is unaffected: the trains were disabled there; the next build has it).
  TR-t02 (Q1IntegrationTests | AbyssTests | TrainTests | Beat04SkybridgeTests) queued to verify.
- 23:27 Q1IntegrationTests.A's 45 "tops off y 0" are AL's alley vents (Beat01_alley/ventFrames / ventGratings at s 24 and 58,
  y 0.012-0.013; GD-final's output), not trains. GD-final (compiled 23:14, before my park fix) still shows the train hits in B
  and KS10; AL-tfinal (started 23:22, after the fix) and my TR-t02 verify it.
- 23:30 the park fix verified by AL's full EditMode run compiled after it (QA/reports/AL-final.xml, 447 / 441 passed, 5 failed):
  AbyssTests.KS10_DropShare PASS, Q1IntegrationTests.B PASS, all 9 TrainTests PASS. The 5 failures are not TR's (HT PlazaInlay,
  BV LightBudget 29 > 28 and Shops 18 > 17, AL's alley vents in Q1.A, FX WallWindows -1.4e-11). TR-t02 running.

## Final state (TR, wave B)
- Build TR-b03 (0 errors) with captures QA/captures/TR_r02 (+ TR_r02_sheet.png) and the smoke PASS; the 23:15 park fix is
  after TR-b03 (EditMode-verified; it changes only where disabled trains sit).
- Open: TR-06 the saloon still reads blocky at 12 m (KS12); TR-08 KS12 composition with T3 (spec timing, lead); TR-02 the glass
  look itself (FX); a small reflection of the headlight light in the near tube glass in KS11 (595, 488); frame time at s 580-600
  and 700-740 not measured (no perf runs in this wave; 21 extra moving lights at most, all Forward+ point lights, no shadows).
- 23:32 TR-t02 (Q1IntegrationTests | AbyssTests | TrainTests | Beat04SkybridgeTests, after the park fix): 36 / 35 passed
  (QA/reports/TR-t02.xml); the one failure is Q1.A, AL's alley vents (45 tops at y 0.012-0.013). KS10 drop share and Q1.B pass.
  fastcheck all OK.
TR DONE 23:33

## FIX WAVE (2026-11-21, panel p4_01; task Logs/fix/TR.md: C31, C40, C41 + lead rulings D-040)
- 01:57 start. Copy at master caddb90 (pre-warmed). Requests for TR in every copy's logs (grep "REQUEST TR"): none new. FX's
  log (blvd, 02:25) carried my two old requests: BridgeSway in CameraRig (D-034 4.3) and NocGlass `_FresnelMax` ("TR sets about 0.5
  on the tube variants in SkybridgeMaterials").
- 02:05 TR-b10 baseline build OK (0 errors; Logs/TR-b10.log; note: `-BuildDir Builds\TR` from bash lost its separator and wrote
  BuildsTR, moved to Builds/TR; later calls quote 'Builds\TR'). TR_r10 (KS11-KS15 1080) looked at; KS14 p99 107.9, 0.82 % >= 120.
- 02:10 C40 row stripes diagnosed: the 1-px binary rows (x 0.92) in the KS12 window band are in TR_r10 and TR_r10b (-rain 0
  -freeze-clock) alike, in TR_r11g1 / g2 (tube glass with grime / rivulets off, -tr-glassdiag, since removed) alike, and absent in a
  pose inside the tube (TR_r10d pose_TRin660, no glass between). Cause: MY shader. NocTrain hashed the per-car seed as
  (uint)(seed x 255): the interpolated vertex colour b x 255 lands a hair either side of the integer per pixel along the 22 m x 1 m
  window quad, so the truncation flipped the per-window variation hash row by row (8 %); with the new saloon it flipped the
  far-blind hash too (TR_r11 KS12 / pose_TRside660: hard black-white rows and speckle). Fix: round (seed x 255 + 0.5). TR_r12:
  rows gone (KS12 band vertical 2nd difference 7.63 -> 5.74, now the interior's own edges; pose_TRside660 no speckle).
- C40 saloon (NocTrain.shader, zone 1): interior mapping of a lit box 3.2 m deep (floor v 0.85, ceiling v 3.0): ceiling luminaire
  (2.6) with falloff to the walls, rack undersides on both walls, the far wall with its own windows (night 0.08, a quarter with
  lit blinds) between lit pillars and a lit upper wall, transverse seat rows (0.95 m, 2 + 2, aisle 1.18-2.02 m, seats face +u:
  white headrest covers on the fronts, dark backs, none at the doors), 2.4 m ceiling-panel light pools; four rotated-grid taps along
  the uv screen derivatives (taken before every branch) and a fade to the mean (0.5) between 3.5 and 10 cm of glass per pixel;
  transmitted x (1 - Fresnel). Python twin (scratch tr/saloon.py + ks12sim.py, the KS12 camera) used to design it before Unity.
  fxc offline (scratch tr/shcheck.py): FAILED 0 (02:22, 03:00).
- C31 gate B (SkybridgeGate.cs): frames of both gates in the blocks' CC0 concrete (lib.Concrete) instead of painted steel; gate B
  light content (GateLights): a 1.0 x 10.8 m LampWarm luminaire 4 cm under the lintel between the pylons, a wall lantern on the bridge
  face of each pylon (lens 0.36 x 0.70 at y 5.2), two 10 m luminaire lines in the passage ceiling, four shadowless point lights
  (soffit r 14 i 4.0, lanterns r 10 i 2.0, passage r 12 i 2.5) and five wet sources (lintel line, lanterns, passage lines), halos
  at the lanterns. Gate A keeps no lights (KS10, AB's C05). TR_r12 KS14 (first sizes): p99 125.2, 1.07 % >= 120, mean 33.3 (24-40);
  enlarged (1.0 m lens, 0.36 x 0.70 lanterns, 0.24 m passage lines, soffit i 4.0) for the 21:9 / 4K margin.
- C41 face tubes (SkybridgeTubes.cs): rib bands r 5.02-5.22, 0.45 m, on the bridge's 24 m grid inside both open sections (upper 24,
  lower 25); on every upper rib a faint LED arc (NocLine 0.14 m on the rib's outer face, +-75 deg around the bottom, eight chords,
  lit on the lower facets, SignWhite x 0.22). TR_r11 first tried two continuous LED channels along the flanks: in KS11 / KS14 they
  read as two strings hanging the portal ring from the sky (rejected). TR_r12 arcs: KS11 (crop 1080-1260 x 0-340) and KS14 read as a
  ribbed glass tube in perspective whose last ring is AB's portal; the glass body outline shows between the arcs; KS04 shows the
  tube as a ladder of arcs down to the far portal (gain 0.3 -> 0.22 after that look).
- FX's `_FresnelMax` 0.5 on beat-4 copies of the tube glass for the bridge tubes N / S only (SkybridgeMaterials.TubeGlass*); the
  face tubes keep the library glass (the end-on upper tube must not vanish, C41). In this copy NocGlass has no `_FresnelMax` yet, so
  it is a no-op here: to be looked at after the merge (KS11, KS14).
- 03:05 TR-t10 (TrainTests | Beat04SkybridgeTests): 20 / 20 passed (QA/reports/TR-t10.xml), with the two new tests
  GateB_LightContent_ConcreteFrame and FaceTubes_Ribs24_UpperLedChannels; beat 4 117 214 triangles (budget 520 k).
- 03:31 TR-b13 OK (0 errors, 0 shader errors; Logs/TR-b13.log): saloon top band toned (upper far wall 0.55-0.9, luminaire 2.6),
  arcs x 0.22, gate lights enlarged, FX's cap on the bridge-tube glass copies. This is the final build.
- 03:40 TR_r13p (TR-b13, 1080: KS04, KS10, poses TRin660, TRside660, TRback610) looked at: KS04 the upper tube is a ladder of faint
  arcs from the canyon bridge down to the far portal ring (structure, not a floating ring); KS10 unchanged except gate A's frame
  material (not in view: the block at the right edge stays as it was, AB's C05 / D-037 (6)); TRback610 T1's windows at grazing
  show the seat rows in perspective, the canyon-bridge portal with an arc; pose_TRside660 / TRin660: no row stripes, no speckle,
  the saloon reads as a lit car (ceiling, racks, far windows, seats), max 221 -> 202 at the ceiling band.
- 03:42 TR-final (full EditMode on the final tree, QA/reports/TR-final.xml): 476 tests, 475 passed, 0 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch, skipped by design).
- KS13 note (not a change of mine): central median 42.1 (30 x 30 %) / 39.1 (30 %-area box) in both TR_r10 (baseline) and TR_r12,
  frame mean 34.3: the drop from wave B's 57.7 came with the merged abyss / glass work; T6 under the strip is visible (roof glazing,
  the lower tube's new ribs). KS13 p99 83.9 is AB's C30.
- 03:48 TR_r13 (TR-b13, keyshots incl. 4K / 16:10 / 21:9) looked at, and the lookv2 check: KS14 PASS at all four (p99 144.4 /
  153.2 / 125.3 / 132.7, >= 120 share 1.26 / 1.33 / 1.08 / 1.16 %); KS11 16:10 / 21:9 FAIL p99 119.7 / 118.5 (integ 123.4 /
  121.3): my far windows had lost the bright top line (T1 band >= 120 px: 552 baseline -> 286). Fix: the saloon fades at distance
  to a vertical far profile (0.33 seat zone, +0.29 middle, +1.30 ceiling line in the top 19 %, x 1.3) instead of a flat mean.
  TR-b14 / TR-b15 (shader only, NocTrain; 12 s builds), fxc FAILED 0 both.
- 04:11 TR_r15 (TR-b15, keyshots incl. 4K; QA/captures/TR_r15, sheet _contact/contact_TR_r15_01.png) looked at, full size and
  4K crops: KS11 p99 121.4 / 120.4 / 118.9 / 130.2 (16:9 / 16:10 / 21:9 / 4K); the T1 band is back to 525 px >= 120 (552
  baseline); the remaining 21:9 shortfall is in the garden block (900-1440 x 300-480: 1 112 fewer >= 120 px than TR_r10 at 1080p),
  the under-crown glow behind gate B (GD's beat 5, in progress in this copy), not the train. KS12 p99 127.6 (all four pass), band
  mean 84.1, no row stripes (4K crop 1600-3200 x 150-500: seat rows with white headrests in perspective, the rack line, far windows
  between lit pillars, a deck lamp's GGX highlight on the glass). KS14 as TR_r13. KS15 4K crop (1600-2240 x 0-320): the upper tube
  is a glass cylinder with dark ribs and a lit rib arc, the guideway inside, ending at the cyan portal ring (C41).
- 04:13 smoke on TR-b15 (-autotest -brisk -beat 4 -autotest-until 880): AUTOTEST PASS, errors 0, corridor 48 204 rays 0 misses,
  beat 5 at s 880 (QA/reports/autotest_brisk_from560_until880.json, Logs/player-TR-smoke.log); alloc mean 32.2 B/frame (engine
  input box 32, excess 0.2), GC 0.
- 04:14 TR_r15p (TR-b15, 1080: KS04, KS10, TRside660, TRin660, TRback610, TRemerge588; sheet _contact/contact_TR_r15p_01.png).
- TR-final (03:31, full EditMode) ran on the final C#; the later builds changed only NocTrain.shader (the far profile).
- Image metrics (QA/reports/image_metrics_TR_r13.md vs image_metrics_TR_r10.md): the NaN-like / isolated-pixel hits in KS11 /
  KS14 / KS15 are the baseline's (same coordinates, e.g. KS11 (1419, 605), (1086, 101)); KS12's isolated pixel (426, 118) in the
  old window band is gone; banding[abyss] KS12 / KS13 has 0.00 visible steps (D-040 (6)).
- Disk: W: 9.7 GB free at 04:15 (below 15; not my folders: mine are TR_r02 31 MB, TR_r10 14 MB, TR_r15 99 MB, TR_r15p 17 MB,
  Builds/TR 464 MB). Deleted TR_r00, TR_r11, TR_r12, TR_r13, TR_r13p, TR_r14, TR_r14p after logging their numbers.

## Fix-wave results against Logs/fix/TR.md
- C31: KS14 p99 >= 120 at 16:9 / 16:10 / 21:9 / 4K (144.4 / 153.2 / 125.3 / 132.7) with light content on the gate (lintel
  luminaire, lanterns, passage lines; no window change); the frames are concrete with texture visible in the 4K crop. DONE.
- C40: the KS12 band shows seat rows and light falloff in the 4K crop; no stripe aliasing (root cause my seed truncation); TrainTests
  9 / 9 (TR-t10, TR-final). DONE.
- C41: KS14 / KS15 the upper tube reads as a ribbed tube ending at the portal (arcs in the 4K crops), with AB's collar when it
  lands. DONE (FX's _FresnelMax leaves the face tubes uncapped).
- T-TRAIN timings unchanged (TrainSchedule untouched; TrainTests TTrain_MeetingPoints PASS). Frame time at s 580-600 / 700-740:
  not measured (no perf runs in this wave); added cost: 4 static point lights at gate B (s 860-880), the saloon's 4 taps only on
  window pixels at < 10 cm per pixel, 2.4 k triangles of arcs, 17 k of ribs.

## Proposed decisions (fix wave)
- D-TR-6 (C40) Train saloon by interior mapping: box 3.2 m deep, floor v 0.85, ceiling v 3.0, seat rows 0.95 m (2 + 2, aisle depth
  1.18-2.02, headrest band 0.24 m under v 2.10), far windows at the near band's height (25 % lit blinds), 4 rotated-grid taps on the
  uv footprint; detail kept below 3.5 cm of glass per pixel (KS12: 10.3 m x 2 tan 33 / 1080 = 1.24 cm), faded to the far profile
  by 10 cm (T1 at 32 m in KS11: 3.85 cm x grazing). Per-car hashes take round(b x 255), never the truncation (the KS12 row stripes).
- D-TR-7 (C31) Gate B carries the beat's light content for KS14: lintel luminaire 1.0 x 10.8 m, two lanterns 0.36 x 0.70 at y 5.2,
  two passage lines 10 x 0.24 m, 4 point lights, 5 wet sources (beat 4 now 21 + 5 = 26 sources registered; MaxActive <= 32 checked
  by Lamps21_LightsAndWetSources); frames in cast concrete. KS14 >= 120 share 0.82 % -> 1.08-1.33 %.
- D-TR-8 (C41) Face-tube ribs at x = 375.3 + 24 k inside each open section (upper 24, lower 25), r 5.02-5.22, 0.45 m; upper-tube
  LED arcs on the ribs: axis radius 5.22 + 0.07 + 0.005 = 5.295, +-75 deg, 8 chords (sagitta 5.295 (1 - cos 9.375 deg) = 0.071 m,
  so the chord midpoints sit on the rib face), SignWhite x 0.22, NocLine lower facets.
TR DONE 04:15
