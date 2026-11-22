# AL - beat 1, the alley and the mouth (Phase 4 wave B, fifth start of wave A + wave B, 2026-11-20)

Owner of Beats/Beat01_Alley/**, Shaders/Beats/NocAlley*.shader and NocSteam*.shader, Tests/EditMode/Beats/Beat01AlleyTests.cs,
docs/logs/AL.md, QA/critique-AL.md. Copy: .worktrees/waveb (branch n6-waveb), partners TR, GD, AU. Never commits (the lead does).

## Start (20:34)
- Read: Beat01_Alley code (G1 graybox-plus, the Look v2 rows of SU / PA / FA), Beat01AlleyTests, MASTER_PROMPT 14.5.1, LOOK_V2 6 / 8,
  DREAD 6, BUILDER_BRIEF, critique INT-06 / 07 / 20 / 21 / 27 / 39, VisibilitySubjects (KS00-KS03), NocShopfront (template).
- Tooling: the copy had no Assembly-CSharp*.csproj (fastcheck refused); copied the generated, git-ignored csproj files from the
  blvd copy (same package hashes) -> fastcheck OK (199 / 265 files). fxc harness: scratchpad al/shcheck.py (FX's harness, root waveb).
- 20:43-20:55 AL-b01 (HEAD state of beat 1) SUCCESS, 685 s. AL-r00 capture KS00-KS03 (keyshots1080):
  KS00 mean luma 26.98, mouth peak 230; lookv2_check: KS00 FAIL (texture_0 meso 0.91, pool 0.33 / texture_1 micro 1.34 meso 0.48
  pool 0.37), KS01 FAIL (micro 1.30, meso 0.39), KS02 FAIL (micro 1.19 / 1.39, meso 0.33 / 0.36, pool 0.00 / 0.01), KS03 PASS.
  Looked at KS00 (+ x4 crop of the mouth), KS01, KS02, KS03: the INT-06 / 07 / 39 picture as in lookv2_01; the mouth at 120 m is
  NOT a glowing slit: the north podium of the boulevard shows dark through it (title frame weak).

## Plan (in order)
1. INT-06 windows: every grid opening a framed, recessed window (NocAlleyWindow: reveal, frame, room with lamp, furniture,
   curtains / blinds; sill and drip head geometry). Blade signs: steel housings with inset lit faces and arms. Bulkheads: fixtures.
2. INT-07 noodle bar: modelled room (NocAlleyRoom: tiles, kitchen line, pots and steam, menu light-boxes, stools, pendants, noren,
   red lanterns), its own analytic lamps.
3. Kit of 14.5.1: 140 cables, 40 AC units with detail (INT-39), 22 pipes, 24 awnings, 6 bins, 4 steam vents (NocSteam).
4. INT-21 pools: NocAlleyPool (Blend DstColor One) for every lamp on its own and the facing wall; the dead-end door lamp (20th light).
5. DREAD 6: two empty street-level rooms (table, chair, cup, lamp on, nobody), AlleyRooms.
6. KS00 title frame: the mouth haze card (NocAlleyGlow, additive, dissolves with the walker's distance).

## Cycle 1 (21:36-21:50)
- 21:36 fastcheck OK (209 / 278). AL-b02 SUCCESS in 73 s (26 Megastructure/* shaders checked, my 5 added to Always Included:
  NocAlleyWindow, NocAlleyRoom, NocSteam, NocAlleyPool, NocAlleyGlow; fxc offline 0 failures for all five).
- AL-r01 (13 stills: KS00-03, beat01 stills, tc_noodle_bar, tc_alley_wall). Player log: "beat 1 alley: 52 parts, 123952 triangles
  (budget 480 000), 45 signs, 20 lights, 33 wet sources + 2 walls, widths 4.40/6.00/4.60/5.00 m, 289 windows (119 lit), 0 invalid
  meshes". (The one logged error is Beat04 train, TR's.)
- Looked: KS00 (+ crops mouth x4, lamp), KS01 (+ bar crop x3), KS02, tc_noodle_bar, sheet of the beat01 stills.
  KS00 mean 29.21 (r00 26.98), slit mean 47.9 (45.7), slit max 229, p99 98.4. lookv2_check: KS00 dark_cast_RG 6.01 (D-032 4
  allows 6), meso 0.89 / 0.59 (pools now pass); KS01 micro 1.37 meso 0.44; KS02 p99 83.3 (< 90), pool 0.04 / 0.01, micro / meso low.
  Seen: windows are framed recessed windows with rooms (KS02 top, KS01 right); bulkheads are fixtures with cages and hoods; the bar
  shows lanterns, noren, menus (blown white), tiles (too bright, blocky variation); the kitchen (pots, steam) is invisible from
  KS01 (it stood on the back wall); transoms over the doors still flat cream cards; the mouth haze too weak (+2 codes);
  the grey / teal blade faces still read as flat slabs at distance; vending fronts plain.
- Fixes for cycle 2: kitchen line moved to the north wall (pots, steam, menus above it, in KS01's view line); menu glyph cells
  0.20 x 0.27 with a uv-derivative footprint; tiles darker (0.44), grout stronger, tile variation +-4 %; Lamp0 i 2.6, Lamp1 over the
  kitchen line; steam opacity 0.38 (bar) / 0.30, glow 0.30; transoms -> NocAlleyWindow fanlights; cyan / white / neutral blades ->
  light-boxes with dark glyphs (NocAlleyRoom kind 4, new slot AlleySign), magenta / amber keep NocNeon; window spill pools on the
  facing wall for every lit window (INT-21, E ~ L A / d^2); pools lerp to white 0.6 (dark cast); mouth haze x4; window rooms x 1.36;
  vending header light-box, dispenser flap and coin panel (flat, 1 cm proud).

## Cycle 2 (21:52-22:18)
- AL-b03 SUCCESS (132 s), AL-r02 (13 stills). KS00 mean 30.62, slit mean 56.0 (r01 47.9), p99 125.3; lookv2_check: KS00 mean 30.6
  (> 30) meso 0.89 / 0.89, dark cast now passes; KS01 mean 35.7 (> 34) meso 0.50; KS02 p99 passes (83 -> >= 90), pool 0.08 / 0.01,
  micro 1.21 / 1.40, meso 0.35 / 0.36; KS03 PASS.
- Looked: KS00, KS02, KS01 + bar crop x4, KS00 crops (meter cabinet, a light-box blade). The bar now reads as a noodle bar
  (lanterns, noren, tiled kitchen wall, three stock pots, steam, menus); menus blown white (no glyphs), noren mauve under the warm
  light, light-box blades too bright and blank at distance (and they lifted the means); the slit glows (56).
- Fixes for cycle 3: light-box gain 3 -> 1.4, glyph fade 2-4 px -> 1.2-2.5 px (strokes are box filtered), menus albedo 0.34; noren
  dark teal-navy; bar steam opacity 0.5, plumes 0.75 x 1.75 m; window spill 1.1 / 0.85, facing-wall bulkhead pools 0.6; the west
  meter cabinet moved into KS00's region 0 (z 3.0); empty room 1 moved to the west wall z 37.6-39.0 (inside KS01's frame, left of
  the bar, seen at about 50 deg: the traces of emptiness in a key shot).

## Cycle 3 (22:18-22:50)
- 22:20 AL-t01 (Beat01AlleyTests | VisibilityAuditTests): 17 / 17 passed (QA/reports/AL-t01.xml). Logged: widths 4.4000 / 6.0000 /
  4.6000 / 5.0000 at every station; KS00 slit 35.09 px at 1080p (33 +-3); KS02 half-angles 6.185 / 4.294; KS01 window x -0.153..-0.020
  y -0.273..0.016, sign x -0.199..0.004 y 0.058..0.555, counter y -0.500..-0.219; catwalk y -0.929..-0.700; clearance camera 0.400 m,
  capsule 0.349 m (>= 0.348); 126 674 triangles (budget 480 000), 20 lights, 289 windows (119 lit, 41.2 %), 0 invalid meshes.
- The Unity queue is not FIFO: AL-b04 waited 22:16-22:43 behind TR / GD / AU calls.
- AL-b04 SUCCESS (76 s), AL-r03: KS00 mean 30.74, slit 55.0, p99 116; KS01 mean 39.15; KS02 mean 28.26, p99 93.3. lookv2_check:
  KS00 mean 30.7 (> 30), meso region 1 0.94 (region 0 now passes); KS01 mean 39.2 (> 34) but texture passes; KS02 micro 1.22 /
  1.40, meso 0.36 / 0.36, pool 0.16 / 0.01; KS03 PASS.
- Looked: KS01 (the empty room 1 big at the left, pale and milky: the NocGlass pane over a too-bright room; table only at the
  frame edge), KS00 (cabinet, light-box blades with glyph blocks, fanlight transoms, slit haze).
- Fixes for the final build: no glass pane on the empty rooms; wallpaper 0.17 / 0.15, ceiling 0.24, fill 0.004, lamp i 1.6; the
  table, chair, cup and lamp moved 0.7 m north (into the part of the room the walk sees from the south); window spill 0.95 / 0.75;
  dead-end pools 0.75 (means toward 30 / 34); an unused local removed (fastcheck --warnings: none in beat 1 files).
- 22:50 final chain started: AL-b05 build -> AL_final captures -> AL-smoke; AL-tfinal (full EditMode) queued behind the build.

## Final (22:50-23:20)
- 23:02 (the queue held AL-b05 until 23:13) smoke on a copy of the b04 build (Builds/ALsmoke, deleted after): AUTOTEST PASS
  brisk_until125, beats 2, errors 0, corridor coverage 48 204 rays 0 misses, alloc 31.8 B/frame (excess 0).
- 23:14 AL-b05 SUCCESS (0 problems; 26 Megastructure/* shaders checked, no shader error). Player log: "beat 1 alley: 52 parts,
  126 666 triangles (budget 480 000), 13 signs, 20 lights, 33 wet sources + 2 walls, widths 4.40/6.00/4.60/5.00 m, 289 windows
  (119 lit), 0 invalid meshes".
- AL_final (13 stills, failures 0, errors logged 0) and its contact sheet QA/captures/AL_final/_contact/contact_AL_final_01.png.
  Numbers: KS00 mean 30.56, p99 116.3, slit mean 55.0 (r00 45.7), slit peak 228.9, slit 35.09 px (test); KS01 mean 39.52, p99 163.7;
  KS02 mean 27.94, p99 93.2; KS03 mean 25.09. lookv2_check: KS03 PASS; KS00 FAIL only mean 30.6 (LOOK_V2 <= 30; 14.5.1 asks
  28-70) and meso region 1 0.92; KS01 FAIL only mean 39.5 (LOOK_V2 <= 34; beat window 14-40 holds); KS02 FAIL texture (micro
  1.22 / 1.40, meso 0.36, pool 0.15 / 0.01). Dark cast, violet, R2, R3, crush, p1, p99 pass in all four.
- Looked at full size: KS00, KS01 (+ the empty room crop: the table, the cup, the lamp on, nobody), KS02, the contact sheet.
- 23:15 smoke on AL-b05: AUTOTEST PASS brisk_until125, beats 2, errors 0, corridor 48 204 rays 0 misses, alloc 25.8 B/frame
  (excess 0) (QA/reports/autotest_brisk_until125.json).
- Files (new): Beats/Beat01_Alley/AlleyMaterials.cs, AlleyRooms.cs; Shaders/Beats/NocAlleyWindow.shader, NocAlleyRoom.shader,
  NocSteam.shader, NocAlleyPool.shader, NocAlleyGlow.shader (+ .meta). Edited: AlleyBlocks, AlleyDoors, AlleyFixtures, AlleyLayout,
  AlleyWindows, Beat01_Alley, G1Kit (new G1Mat slots at the end of the enum; AL transparent slots skipped when their shader is
  missing), G1Materials (AL slots), NoodleBar, VendingMachines, Tests/EditMode/Beats/Beat01AlleyTests.cs.
- GLOBAL: none (no sky, fog, grade, facade, wet floor, glass or rain change). G1Kit / G1Materials are shared with BV's beat 2 only
  through new enum values and cases (no existing slot changed).
- 23:30 AL-tfinal, the full EditMode suite (QA/reports/AL-final.xml): 447 total, 441 passed, 5 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch). Failures:
  * Q1IntegrationTests.A_EveryLaneMetreIsFlooredAtYZero: MINE - 45 rays HIGH at s 24 / 58 / 86 / 104 on Beat01_alley/ventFrames
    (y 0.012) and ventGratings (y 0.013), over Q1's 1 cm walkable-top tolerance. 23:31 fixed in AlleyFixtures.SteamVents: frames 8 mm,
    grating 9 mm (pit 4 mm unchanged; a 4 mm change, invisible, not rebuilt); fastcheck OK; AL-t02 (Q1IntegrationTests |
    Beat01AlleyTests) queued to confirm.
  * BoulevardStreetTests.LightBudget_AtMost28_SmoothFades (29 > 28: "beat 2 lights: 64 managed + 3 tram", beat 2 only) and
    BoulevardStreetTests.Shops_Varied_NeverBrighterThanTheLitRoom (18 > 17 lit shops): BV's beat-2 files (not changed here).
  * Beat07HeartTests.PlazaInlay_SameTopFan_RingsSeamsFade (962 < 1000): HT's beat 7 (not changed here).
  * WetFloorTests.WallWindows_MeanOne_Bounded_StreakVertically (-1.4e-11 >= 0, a rounding sign): FX's shared wet / surface math (not
    changed here).
- 23:34 AL-t02 (Q1IntegrationTests | Beat01AlleyTests, on the vent fix): 12 / 12 passed (QA/reports/AL-t02.xml): Q1 failures per
  beat 0/0/0/0/0/0/0, alley capsule min 0.349 m. The remaining 4 suite failures are other owners' (above).
- Open: KS00 mean 30.6 (LOOK_V2 <= 30 vs 14.5.1 >= 28) and meso region 1 0.92; KS01 mean 39.5 (LOOK_V2 <= 34; the lit empty room
  adds about 6 codes: wallpaper 0.08 or a dimmer lamp would bring it to ~34); KS02 check 7 (walls beside the eye at 4-10 m have no
  source); vending screens still the shopfront product rows in cyan.
AL DONE 23:35

# FIX WAVE (panel p4_01, 2026-11-21; task Logs/fix/AL.md + lead rulings D-040; copy fast-forwarded to caddb90)
- 01:57 start. Read Logs/fix/AL.md (C14 S2, C37 S3, C38 S3, C46 partner S3, D-039 (6) KS02 work-lamp pool), the critic crops
  (b12_win_zoom / b12_blink1: vertical 1-4 px stripes crawling in the lit transom at seq_b12 frames 1-3, 17-19) and p4_01 KS00 /
  KS01 / KS02 (KS01 mean luma 39.47, room 1 region x 70-400 y 450-860 mean 77.0 on 6.5 % of the frame). No "REQUEST AL" lines in
  any copy's logs.
- Diagnosis C14: the transom is a NocAlleyWindow panel 0.9 x 0.25 m 2 cm proud above each door, seen at a grazing angle beside the
  walker; every pattern of the shader used the footprint estimate fp0 = footprint / max(cos, 0.2), which undershoots the true
  footprint on the glass by up to 5x at grazing views, and inside the room the hit point moves with d(tan)/d(angle) = 1 / cos^2 per
  pixel: wallpaper stripe, folds, furniture edges aliased (vertical 1-4 px stripes).
- Diagnosis C37: room 1 was lit by the template's sky ambient (x albedo 0.15-0.17, the brightest albedo on the wall line) and by
  the alley's realtime lights through the alley wall (no shadow maps): flat, even, grey.
- Diagnosis C38: NocSteam was lit by the template's N.L with the billboard's normal turned to the camera: the wall lamps beside or
  behind a plume gave it nothing, so a plume was darker than the lit walls behind it at 6 % mean alpha (0.30 x dens x env).
- Fixes (02:00-02:20):
  * C14 NocAlleyWindow: every pattern takes the screen derivative (fwidth, before any branch) of its own coordinate (glass xg2 / yg2,
    the wallpaper coordinate, the furniture plane, the pendant plane, the ceiling panel); thin bars, furniture and shade by an exact
    box coverage (AlBox); the reveal edge as a coverage blend, not a per-pixel switch; the blind slats fade to their true mean 0.5
    (was 0.8), folds / slats fade below 3-6 px (was 2-4). fxc 0 failures.
  * C37 NocAlleyRoom: _Interior (enclosed room): own lamp only + 10 % of the sky ambient + the lamp's GGX highlight (forward hook,
    NOC_HAS_FORWARD_HOOK as NocWet); CC0 plaster (MaterialLibrary.Plaster _AlbedoTex2, texprep Plaster / polyhaven plastered_wall_04,
    x 2 variation, tile 3.2 m) with damp blotches, tide marks under the ceiling and floor grime in world space; wallpaper 0.135 /
    0.115 ochre and 0.115 / 0.125 dusty green; shade self-lit x 2.6 (was 4, clipped white). The bar and the signs unchanged (_Interior 0).
  * C38 NocSteam: own forward hook, in-scatter of the main light and every additional light through a Henyey-Greenstein phase
    (g 0.35, relative to isotropic), x _Scatter 1.5, + ambient + _Glow; vent opacity 0.30 -> 0.45.
  * D-039 (6): two work floodlights under the catwalk's south stringer (x -+1.8, y 8.85, z 95.86; housing, bezel, lens (EmitLamp),
    guard bars, yoke, fins, hanger), cross-aimed at the opposite wall at (y 3.0, z 89.6) = the KS02 texture regions; their wash
    baked by NocAlleyPool's new spot mode (G1Mat.AlleyWorkPool, one panel per wall z 84-97): E = I N.L cone / (d^2 + 0.25),
    I 350, cone 12 / 28 deg; x ~2.9 at the regions' centre, nothing beside the catwalk. No realtime light (20 stays).
  * Tests: Beat01AlleyTests.WorkLamps_WashTheKS02Walls_NotTheCatwalk, FixWave_EnclosedRooms_Plaster_SteamOpacity; workLampPools is
    non-solid in the clearance test.
- 02:20 AL-b06 failed on compile: my new test used NocPaths without its namespace (fixed 02:21) and AU's AudioDreadTests
  Assert.Warn (AU fixed it 02:20). AL-b07 queued.
- 02:23 AL-b07 SUCCESS (0 problems, 30 Megastructure/* shaders checked). AL-t03 (Beat01AlleyTests | VisibilityAuditTests |
  Q1IntegrationTests): 22 / 22 passed (QA/reports/AL-t03.xml).
- Cycle 1, AL_f01 (KS00-KS02, all four variants) and seq_b12 (frames analysed and deleted as written: blockpop.py in my scratchpad):
  * KS01 38.67 (p4_01 39.47), room region 65.8 (77.0): the room got the plaster but was still bright (lamp i 1.6 is strong at the
    alley's exposure: emission 0.07 linear = code ~75).
  * KS02 pools 1.00 / 1.00 (0.14 / 0.01), region means 40.9 / 35.9 (22.5 / 19.3), KS02 mean 39.17: a flat grey wash: too strong.
  * KS00 4K plume column: no lift (rows 880-1100: 60.1 / 37.6 / 50.7 / 34.9 vs p4_01 60.0 / 36.2 / 53.0 / 35.0). Cause: the
    realtime lights' r 7 window (the s 24 plume gets 0.001 of the z 17.5 bulkhead's light, 0.28 of the z 29 one).
  * seq_b12: the stripes were STILL there, and identical with -rain 0 (so not the rain): zoom of frames 2-5 showed 1 px dark lines
    crossing the room AND the frame bar. Root cause found: the per-panel seed (uint)(rnd x 65535) with rnd = byte / 255 is byte x
    257 exactly; the interpolated colour wanders by an ulp per pixel and the truncation flipped the seed (another room: depth,
    lamp, wallpaper, mullion, frame colour) in scattered pixel columns. Fixed by rounding the byte.
- Cycle 2 (AL-b08, 02:42): seed fix; steam lamps' irradiance baked per plume in vertex G (AlleyFixtures.SteamLampByte, no range
  window); work lamp I 350 -> 100; room lamp 1.6 -> 0.8, wallpaper 0.09-0.11, shade 2.2; own-wall bulkhead pools 0.55 -> 0.30 (they
  multiplied the realtime hot spot), bar pool 0.5 -> 0.4; look-dev knob -al-pool (scales every baked pool, default 1).
  * AL_f02: KS00 30.47, slit mean 56.8; KS01 37.25 (16:10 35.98, 21:9 38.82, 4K 37.18), room 49.4; KS02 31.85, pools 0.96 / 0.51,
    catwalk band 24.7; plume lift vs AL_f01 at x 1995: rows 880 / 950 / 1020 / 1100 +8 / +36 / +40 / +32 % (too strong for the
    10-20 % target). AL_f02p6 (-al-pool 0.6): KS00 29.47, KS01 35.29 (16:10 34.12, 21:9 36.62, 4K 35.24); KS00 16:9 pools pass.
  * KS01 by 240 px column band (f02): the excess over 34 is the bar band (x 720-960: +1.59 codes of the frame mean) and the right
    edge (x 1680-1920: +2.03; the z 40.5 east bulkhead's realtime hot spot, rows 270-540 at 91, almost unchanged at pool 0.6: 88).
    The room's share is now small. Looked at KS01 (the room reads as a dim abandoned room: plaster stains, the lamp, the cup, the
    chair edge), KS00 (soft plumes at the vanishing point), KS02 (a subtle wash on the lower walls, the two lamps under the catwalk).
  * seq_b12 (b08), pop metric refined to true up-and-back pops (a monotonic change as an edge sweeps a block is not a pop): 64 px max
    7.7; the 32 px transom block at frame 10 (24.5) is the dark mullion crossing the block (crops QA/captures/AL_C14/frame_00009-11:
    146.7 / 122.3 / 157.7, a clean bar moving 16 px per frame). Frames 1-3 / 9-11 are clean (no stripes).
- Cycle 3 (AL-b09): every baked pool x 0.7 (PoolGain), vent scatter 2.0 -> 1.4, work lamp I 130 (+ halos on the lenses), look-dev knob
  -al-bulk (the bulkheads' realtime intensity; default 1) to measure the right-edge share of KS01.
- 03:05 the empty rooms' wallpaper warmed (room 1's dusty green read as a cold bunker in the AL_f02 4K crop: plaster stains, holes and
  cracks visible, the lamp, the cup and the table readable) to ochre / rose-beige 0.09-0.11, the shade x 1.5 (was 2.2: clipped white).
- 03:18 static check on b08: seq_static_beat01_alley (-freeze-clock -rain 0, 120 frames) PASS, max |frame - frame 0| 0.0 levels
  (QA/reports/sequences_AL_st.md); frames deleted.
- 03:19-03:30 AL-b09 SUCCESS (180 s; the queue held it 03:01-03:19 behind TR / GD tests). 03:31 AL-f03 (KS00-KS02, 4 variants)
  and AL-f03b (KS01 at -al-bulk 0.6) started: f03b's player stalled at start-up (no log, killed after 6 min by run-player), f03
  waited for a 4K slot (three other copies' players running: integ QA, abyss AT, heart SG).
- REQUEST QA (Assets/_Game/Scripts/Runtime/QA/ShotPlan.cs), also in shared_change_requests:
  (1) a 60 fps transom sequence (C14 acceptance), in TransitionSequences after the loop:
      list.Add(WalkSeq("seq_al_transom", "boundaries", "boundary", "s 100-110 at 1.5 m/s and 60 fps: the lit transoms beside the walker (C14)", 100.0, 110.0, MotionFps, w, h));
  (2) a 120-frame steam clip with the clock running (C38 acceptance), in the rain set (line 270):
      if (rain) { plan.sequences.Add(RainSequence(w, h)); plan.sequences.Add(AlleySteamSequence(w, h)); }
      public static SequenceSpec AlleySteamSequence(int w, int h) => StaticSeq("seq_al_steam_s20", "rain", "static", "120 frames at 60 fps, s 20 looking up the alley, clock running: the s 24 vent plume rises 0.3-0.6 m/s (C38)", 20.0, 0.0, NocturneConstants.EyeHeight, 20.0, 0.0, -2.0, NocMath.Fov(20.0), StaticFrames, MotionFps, w, h);
  (3) the C21 block-pop test: count a pop only where frame i departs from BOTH neighbours in the same direction (a local
      extremum): blink = (m_i - m_{i-1}) (m_i - m_{i+1}) > 0 ? min(|m_i - m_{i-1}|, |m_i - m_{i+1}|) : 0. With min(|d1|, |d2|) alone
      a crisp bar sweeping through a block as the walker moves (16 px per frame at s 105) scores like a blink (AL seq_b12: 64 px
      max 34.2 naive vs 7.7 with the sign rule; the 32 px transom block's 24.5 at frame 10 is the window's mullion crossing it).
- 03:45 smoke on AL-b09: AUTOTEST PASS brisk_until125, beats 2, errors 0, coverage misses 0, alloc 27.3 B/frame (excess 0)
  (QA/reports/autotest_brisk_until125.json); player log: "beat 1 alley: 55 parts, 127 902 triangles (budget 480 000), 13 signs,
  20 lights, ... widths 4.40/6.00/4.60/5.00 m, 289 windows (119 lit), 0 invalid meshes".
- 03:50 AL_f03 (b09, 12 stills): KS00 29.73, slit mean 54.3, peak 198.5; KS01 35.65 / 16:10 34.45 / 21:9 37.09 / 4K 35.60 (p4_01
  39.47 / 38.0 / 40.8 / 39.4), room region 44.2 (77.0), bar-window p99 229.9; KS02 31.40, pools 0.89 / 0.41 (21:9 east 0.31), catwalk
  band 43.0 (!); plume lift at KS00 4K x 1995 vs AL_f01 (no visible steam): rows 880 / 950 / 1020 / 1100 +5 / +24 / +28 / +23 %.
  lookv2_check: KS00 only meso region 1 0.87 (p4_01 0.93: the pools x 0.7) and the 16:10 mean 30.1 (this copy's table is (16, 30),
  D-039 (5) says 31); KS01 mean only; KS02 micro / meso (the wall material, p4_01 the same) and the 21:9 p99 / east pool.
  Looked: KS01 full frame and the 4K room crop (warm rose-beige plaster with stains, holes and cracks, the lamp, the cup, the table,
  a dark room: the abandoned room reads), KS02 (the wash a soft gradient on the lower walls; the two halos I had added to the work
  lamps were big glow balls over the catwalk: catwalk band 24.7 -> 43.0, over the 14.5.1 limit 40).
- Final changes (03:51, AL-b10): the work lamps' halos removed; vent scatter 1.4 -> 1.2 (the lift 23-28 % at 1.4, the target 10-20 %).
- KS01 residual (C37 acceptance <= 34 at every aspect NOT met): the room is fixed (region 77 -> 44, now dimmer than the walls around
  the bar), the remaining excess over 34 is, by 240 px column band of the 16:9 frame (AL_f02), the bar band x 720-960 (+1.6 codes of
  the frame mean: KS01's key subject) and the right edge x 1680-1920 (+2.0: the z 40.5 east bulkhead in frame, LOOK_V2 6's i 1.6
  0.25 m off the wall, its realtime hot spot rows 270-540 at ~90 codes, unchanged by the pools: 91.3 at pool 1.0, 88.0 at 0.6).
  Lowering every pool further costs KS00's check-7 pool shares (21:9 already 0.33). Proposal for the lead (decisions_to_log): KS01's
  window (20, 37) at all aspects, as D-039 (5) did for KS00, or the z 40.5 bulkhead at i 1.0 (an -al-bulk 0.6 test run stalled at
  player start-up and could not be repeated before the deadline).
- 04:05 AL-final (full EditMode, final code incl. the 03:51 changes): 476 total, 475 passed, 0 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch) (QA/reports/AL-final.xml). Logged: "work wash side -1 / 1: x1.48 at the KS02 region, x1.00 at
  the catwalk, x1.00 at z 80"; "beat 1 windows: 289 framed, 119 lit (41.2 %); parts 55".
- 04:14 AL-b10 SUCCESS (0 problems; the final build, Builds/AL).
- 04:16 AL_fin (KS00-KS02, 4 variants; failures 0, errors 0, framing 0) and AL_finb (the seven beat01 stills; failures 0):
  KS00 29.73, slit mean 53.4, peak 198.4; KS01 35.64 / 16:10 34.43 / 21:9 37.07 / 4K 35.58, room region 44.2, bar-window p99 229.9,
  dark cast R - G 5.26 (<= 6); KS02 30.56, pools 0.89 / 0.41 (21:9 east 0.31), catwalk band 24.3 (< 40); KS00 4K plume column
  x 1995 rows 880 / 950 / 1020 / 1100: 62.7 / 45.3 / 62.5 / 41.6 vs AL_f01 (no visible steam) 60.1 / 37.6 / 50.7 / 34.9 = +4 / +20 /
  +23 / +19 %. Contact sheets QA/captures/AL_fin/_contact/contact_AL_fin_01.png, QA/captures/AL_finb/_contact/contact_AL_finb_01.png.
  Looked: KS02 (no halo balls; the lenses small; a soft wash on the lower walls), the beat01 stills (look_l / look_r: the lit
  transoms clean, no stripes; mid: vending; start / end / look_back / up60 unchanged in character).
- 04:16 smoke on AL-b10: AUTOTEST PASS brisk_until125, beats 2, errors 0, coverage misses 0, alloc 28.8 B/frame (excess 0).
- 04:17 comment-only fix in AlleyFixtures (the work-lamp doc: x1.48 with PoolGain 0.7); fastcheck OK. Intermediate captures deleted
  (AL_f01 / f02 / f02p6 / f03 / f03b / b12 / b12r0, the previous wave's AL_final); kept AL_fin, AL_finb, AL_C14 (the C14 crops),
  AL_st (static report only). Disk W: 7.6 GB free (other copies; was 14 GB at 02:16).
- Files changed in this wave: Beats/Beat01_Alley/AlleyFixtures.cs, AlleyMaterials.cs, AlleyRooms.cs, AlleyWindows.cs, G1Kit.cs
  (G1Mat.AlleyWorkPool), G1Materials.cs; Shaders/Beats/NocAlleyWindow.shader, NocAlleyRoom.shader, NocSteam.shader,
  NocAlleyPool.shader; Scripts/Tests/EditMode/Beats/Beat01AlleyTests.cs; docs/logs/AL.md; QA/critique-AL.md; Logs/AL_progress.md.
  No new shader file (no RebuildAll needed). GLOBAL: none outside beat 1 (the pools x 0.7 and the room / steam / wash changes show in
  beat 1 only; the boulevard's mouth frames KS03 see the alley's far end only).
- Open: C37's KS01 <= 34 (35.6 / 34.4 / 37.1 / 35.6; proposal above); KS02 21:9 east pool 0.31; KS00 meso region 1 0.87 (p4_01 0.93)
  and C46 (encoded meso; waits on AT's dither ladder and QA's encode); C14's 60 fps and C38's 120-frame clip need QA's sequences
  (requested); look-dev knobs -al-pool / -al-bulk (default 1, read through CliArgs.GetDouble) stay in AlleyFixtures.
AL DONE 04:19
