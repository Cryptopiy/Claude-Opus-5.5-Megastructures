# BV - Phase 4 wave A: beat 2 Boulevard canyon (14.5.2), money shot 1 (KS03-KS06), the tram T_A

Owner files: Beats/Beat02_Boulevard/** (and new files there); Beats/Relief/BoulevardRelief.cs, Beats/Relief/CanyonBridge.cs;
Shaders/NocShopfront.shader; Editor/MaterialFactory.Boulevard.cs; new Runtime/Core/MaterialLibrary.Boulevard.cs; new
Tests/EditMode/MaterialTests.Boulevard.cs; Tests/EditMode/Beats/Beat02BoulevardTests.cs and new test files; docs/logs/BV.md;
QA/critique-BV.md (all Assets paths under Assets/_Game).
Requests to BV: write "REQUEST BV: <file>: <exact change>" in your own log; answers appear here as "DONE BV <ID> <request>".

Tasks (in order): 1 tram T_A; 2 street level (INT-12, INT-22, CRIT0 #15 / #16 / #18, KS04 lower third); 3 podium shopfronts
(INT-22) and the terminus hall (INT-35); 4 the rest of KS03-KS06 and the beat-2 stills.

## Resume rule
Read this log, then `git diff -- <BV files>`; continue from the last step marked GREEN / committed. Never redo committed work.

## 2026-11-16

### 08:20 start
- First start of this wave (no earlier BV log). HEAD 01111e9. W: 32 GB free.
- Read: BUILDER_BRIEF; DEVELOPMENT D-027..D-033; MASTER_PROMPT 14.0, 14.4, 14.5.2, 14.7 (H4, H7), 14.8, 14.10; critique-lookv2-01
  (INT-12, INT-13, INT-22, INT-23, INT-35, CRIT0 #15 / #16 / #18); the G1 beat-2 code, GE's relief / terminus edits.
- Looked at (full size): lookv2_01 KS03, KS04, KS05, KS06; lookv2_01b beat02_boulevard_look_l, beat02_boulevard_mid.
  - KS04: the lower third (y 880-1080) is a featureless near-black plane; the podium shopfronts are rows of identical flat
    white squares (x 30-480 and 1460-1920, y 850-890), the brightest thing near the street; lamp heads are flat white slabs;
    no tram, no kiosk, nothing on the median; face B a star field (FA / GE).
  - KS06: no tram (the subject); the same white squares; the lower 40 % is empty dark paving.
  - KS05: the near lamp head (1480-1560, 740-770) is a flat white hexagonal slab.
  - KS03: the street is a dark plane with a row of black bollards; shopfronts white squares (700-1700, 740-770).
  - beat02 look_l: rows of white shop squares with bright vertical bars; the street floor empty paving.

### 09:30 task 1, the tram T_A: design and code (not built yet)
- Route problem: the spec starts the 60 m tram with its head at x 205, but the terminus is only 10.3 m deep (x 205-215.3); 50 m
  would stand on the open tongue root. Solution: Megastructure/NocTram (new, Shaders/Beats/NocTram.shader) discards fragments
  beyond two world planes, both inside sealed meshes: x > 214.47 (2 cm behind the terminus hall's new east interior wall) and
  z > 278 (8 m inside a new depot tunnel). West end: the north track leaves the straight at x -16 on a right-hand r 35 curve
  into a depot portal in the north podium face (x -53.6..-48.4, y 0..6.4, z 270, west of the arcade), tunnel z 270..300.
  Seen from s >= 218 the walker sees at most 5.5 m into the tunnel (portal faces south, sight lines >= 48 deg): clip at 8 m.
- Timing (TramSchedule): x(tau) = 205 - 7 tau on the straight; capture tau = max(0, (s - 230) / 2.5); abeam s 272.03 (2.5 m/s)
  / 258.18 (1.5 m/s) analytic; head x 109.8 at s 264; hidden (tail past z 278 + 0.3 m) at p 352.28 = tau 50.33 s.
- Reset rule (proposed BV-D1): armed when s < 218, applied only while the tram is invisible (at the portal, or hidden in the
  depot); never a jump on the street. The next crossing of s 230 launches it again.
- Vehicle (TramBuilder): 5 modules 11.68 m + 4 gangways 0.4 m = 60.0 m, 3.4 m wide, body y 0.3..3.9; cab ends with a 0.35 m
  rake, side cab windows, destination band, headlights / tail lights; zones by vertex colour R; inset window band with gaskets;
  single-arm pantograph on module 2 touching the contact wire (5.575 vs 5.6); roof gear, bogies, bellows.
- Overhead line: contact wire y 5.6 from inside the terminus into the depot, messenger y 6.7 sagging 0.45 m, poles 7.4 m every
  30 m (11 m on the curve) on the route's right side (z 230.4: outside the sight sector), cantilever arms, droppers.
- Terminus (INT-35): portal 5.2 x 6.4 (was 4.4 x 4.8 with a lit pane behind) open into a real hall: concrete interior walls
  (with holes at the facade's openings), steel trusses every bay, two rows of linear lamps (one in five off), three dim hall
  lights (r 9, i 1.4 / 0.8 / 1.1, cool neutral), a steel door leaf in the east service door. EmitShopCold panes removed.
- Lights: headlight SignWhite r 22 i 3 (off 2 m before the depot portal), two saloon lights WinNeutral r 11 i 1.4 riding in
  modules 1 and 3 (off inside the terminus and the depot): the tram lights the wet street as it passes.
- Offline shader check: scratch fxc harness (scratchpad fxc/shcheck.py: every pass vs_5_0 / ps_5_0, URP / core ShaderLibrary
  copied, relative includes resolved, _ADDITIONAL_LIGHTS + _CLUSTER_LIGHT_LOOP variant too): NocShopfront OK (control), NocTram
  OK, a planted error caught. NocTram stayed outside Assets until it passed.
- New files: Beats/Beat02_Boulevard/TramSchedule.cs, TramBuilder.cs, TramDriver.cs, TramLook.cs; Shaders/Beats/NocTram.shader;
  Scripts/Runtime/Core/MaterialLibrary.Boulevard.cs (field Tram); Tests/EditMode/Beats/TramTests.cs; Tests/EditMode/
  MaterialTests.Boulevard.cs. Edited: MaterialFactory.Boulevard.cs (row Tram), Beat02_Boulevard.cs, BoulevardLayout.cs (portal
  5.2 x 6.4), StreetFacade.cs (depot opening), TramTerminus.cs.
- fastcheck: runtime OK (196 files); editor fails only in AbyssTests.cs (AB, mid-edit): waiting before the Unity calls.

### 10:10 second start of wave A (restart after the usage-limit stop)
- Read this log, `git status`, `git diff -- <BV files>`: the 09:30 state is in the tree (nothing built, nothing committed).
- Three files had lost their CRLF endings in my edits (Beat02_Boulevard.cs, BoulevardLayout.cs, StreetFacade.cs): restored, the
  diff is now the real 154 / 37 lines.
- fastcheck all: runtime OK (196), editor OK (260). NocTram offline fxc (scratch harness): 3 passes x vs/ps OK, also with
  _ADDITIONAL_LIGHTS + _CLUSTER_LIGHT_LOOP. Committing the tram code now (RESTART NOTE: commit early), before the Unity calls.

### 10:45 BV-t01 / t02 (TramTests | Beat02BoulevardTests | Boulevard_TramRow)
- t01: blocked by a compile error in MaterialTests.Look.cs (AT, mid-edit); waited, fastcheck OK, re-run.
- t02 (11:01): 15 / 16. Tram: saloon band 0.280 linear; abeam s 272.026 (2.5) / 258.182 (1.5) analytic, 272.083 / 258.200 in a
  stepped 60 fps walk; body 60.000 x 3.400 x 3.600 (y 0.300..3.900), pantograph top 5.580 under the wire 5.6; 3 252 triangles;
  KS06 built modules x -0.239..0.232, y -0.439..-0.300 (spec -0.24..0.23, -0.43..-0.29); depot: deepest sight line 5.54 m vs the
  clip plane 8.0 m in. NocTram compiles in Unity with no error or warning (Boulevard_TramRow_AndShader).
- The one failure: Meshes_Valid_ChunkOrigins_UnderBudget, 32 static lights > 28 (my 3 terminus hall lights + the depot light on
  top of 16 lamps + 12 blades). Plan: a light-budget driver (LOOK_V2 6 reads the 14.2.3 caps as "lights enabled at once,
  nearest first"): s-windowed lights with smooth fades, never more than 28 non-zero at any s (test over s).
- 11:05 queued RebuildAll (BV-rebuild01, the Tram row) and the first build + capture (lookdev BV r01) to see the tram.

### 11:05-11:37 (second start, written up at the third start; the pause stopped the run before this was logged)
- lookdev_BV build (11:18-11:29, exit 0) ran; its capture did not run before the pause (11:37). Uncommitted at the pause (saved
  in p4a-wip-2026-11-16b.patch, re-applied by the lead):
  - BoulevardLights.cs (new): every realtime light of beat 2 under one budget (14.2.3 row 2, 28; LOOK_V2 6 "lights enabled at
    once, nearest first"): the 40 median lamps as DOWNWARD SPOTS under their lenses (LampWarm, r 26, i 18, 150 / 80 deg: the old
    r 14 i 3.5 point 1.2 m under a 12 m head put 1.2 % of its 1 m irradiance on the street, so the pools never showed: INT-12,
    KS04's black lower third), the 12 blade lights (moved from BladeSign), 8 arcade soffit lights (WinNeutral r 11 i 2.5, INT-23),
    the 3 terminus hall lights (TramTerminus.HallLights) and the depot light (TramBuilder.DepotLight); each light has a camera-x
    window with 16 m smootherstep ramps and is disabled at weight 0. No allocation per frame.
  - ShopFronts.cs (new, INT-22): one shop per bay with a per-bay state (lit 30 %, dim 18 %, closed shutter 32 %, half shutter 11 %,
    dark 9 %), frames, door, transom, mullions, shutter coil boxes, fascia band with glyph-cell signs (3-6 cells; palettes white /
    neutral / tungsten / amber / a little cyan and red, never magenta or violet), hanging blade signs under the arcade ceiling,
    warm pendants; projecting podium-face signs (y 9-30) on brackets at the column lines. StreetFacade's flat panels removed.
  - NocShopfront.shader: the per-panel state from the vertex colour (B = state x 32, G = light reduction, R = random, A = half
    shutter edge); default (1, 0, 0, 1) = the old lit room, so the alley and other users are unchanged; corrugated shutters with
    filtered slats, grime, leak lines and perforated glow.
  - MedianPromenade: real luminaires (dark painted-steel housing, roof plate, tail over the arm; the lens 0.80 x 0.42 x 0.22 under
    it) instead of the flat white slabs (KS05, CRIT0 #18).

## 2026-11-19 (third start of wave A)

### 17:50 resume
- Read this log, git status, git diff of the BV files: the 11:37 state is in the tree (uncommitted: the four items above).
- GPU (gpu-guard -Status 18:00): 45 C, util 1 %, free 10.4 GB. The machine is heavily loaded (Blender, ffmpeg, three Unity
  instances, many python jobs): fastcheck took 363 s (nominal 9 s), git and bash calls time out at 120 s.
- fastcheck all: runtime OK (198), editor OK (263, then 264 with the new test file).
- Offline shader check rebuilt (scratch bv/shcheck.py: fxc vs_5_0 / ps_5_0 /Gec, every pass, every multi_compile variant incl.
  INSTANCING_ON, URP / core ShaderLibrary copies): HEAD NocShopfront 16 / 16 OK (control), working-tree NocShopfront 16 / 16 OK,
  NocTram 16 / 16 OK, a planted error fails 16 / 16.
- ShopFronts: the podium sign's east glyph face now faces east (LookRotation(left)); NocNeon is Cull Off, so this only fixes the
  quad's orientation.
- New test file Tests/EditMode/Beats/BoulevardStreetTests.cs: LightBudget_AtMost28_SmoothFades (also the worst case: the tram's
  three lights counted wherever the walker could stand while it runs), LampHeads_HaveHousingsOverTheirLenses,
  Shops_Varied_NeverBrighterThanTheLitRoom, ShopPieces_StayInTheArcadeOrOnThePodiumFace.
- REQUEST FX: Materials/Tram.mat (+ .meta) and Materials/MaterialLibrary.asset (the Tram row): the output of BV-rebuild01
  (RebuildAll of 2026-11-16 11:10 with BV's Tram row); please commit them with your next RebuildAll output (Materials are yours).
- Committing (fastcheck green, shaders compile offline), before the Unity calls.

### 19:20 commit 00a15aa; BV-t03 + lookdev_BV r01 pipeline queued (fastcheck, tests, build, capture KS03-KS06 + beat02 stills)
- Beat02_Boulevard.cs: FX's optional TP-2 cleanup (the wet wall macro is the v2 FacadeMacro only). DONE FX REQUEST BV Beat02_Boulevard.cs:36.
- NocShopfront.shader, SG's request (SG log 19:00): NocShopWall() wraps NocShelves for the back and side walls: in rooms taller than
  2.4-2.8 m (smoothstep blend on H) the shelves stop at a 4 cm top board at hs = min(H - 0.6, 2.3) m, above it a plain wall at
  0.45 x and a 0.25 m lit frieze (x 1.6) under the ceiling; the profile is divided by its analytic mean over H, so the room mean is
  unchanged. Steps box filtered over one pixel. fxc: 16 / 16 OK. No counter block (optional, not done).
  DONE SG REQUEST BV NocShopfront.shader shelves below a shelf top, wall and frieze above.
GLOBAL 19:25: NocShopfront rooms 2.4 m and taller (SG's arcade and sign-base shops in beat 6, BV's shops in beat 2, any alley shop
  panel that tall) show shelves only up to 2.3 m with a plain wall and a lit frieze above; the panel's mean brightness is unchanged.

### 19:40 small signs (14.5.2 "6 000 shopfront signs (1-4 m)"), in the tree before BV-t03 / lookdev_BV r01 start
- ShopFronts.ShaftSigns / SmallSigns: projecting vertical signs (0.7-1.3 x 1.6-4.0 m, glyph faces both sides, dark case, two
  brackets) and flat light boxes (1.4-2.7 x 0.8-1.8 m, glyph face on a 6 cm dark case) on the shafts' street faces (z 116 / 274)
  over the terrace, on a 3.0 m x 4.2 m grid, y 43.5..165 with p = 0.30 / 0.22 x exp(-(y - 43.5) / 34); none behind a blade
  (its x span +-7.5 m above its bottom - 1 m: covered from the street, and the blade brackets cross that gap); flat light boxes
  (1.6-3.6 x 0.8-1.6 m, p 0.16) on the podium faces y 10..37 between the column lines. New part shaftSignCasesS/N (SteelPainted).
- Test SmallSigns_1to4m_OnTheFaces_ClearOfTheBlades (sizes <= 4 m, palettes, within 1.7 m of the face, not behind a blade,
  densest low). fastcheck runtime 199 / editor 265 OK (19:44).

## 2026-11-20 (fifth start of wave A)

### 20:25 resume
- Read this log, `git log` of the BV files, `git show --stat 2fe0da9`. The fourth start's BV work (re-applied in WIP 2fe0da9, not
  logged here before the pause): CanyonBridge soffit luminaires (three rows of housing + LipLight lens between the ribs, no realtime
  light) and the test CanyonBridge_SoffitLamps_BetweenTheRibs. No open "REQUEST BV" (FX TP-2 and SG shelves were done 2026-11-19).
- No BV capture had ever run (the lookdev BV r01 of 11-19 was stopped by the pause): this start's first job is pictures.
- fastcheck needs Assembly-CSharp*.csproj in the copy root (the worktree had none): copied the two Unity-generated, git-ignored
  files from the main project (Unity install hint paths only). fastcheck all OK (runtime 199, editor 265).
- Live note: Logs/BV_progress.md. New: QA/critique-BV.md (BV-01..06).

### 20:37-20:49 BV-b01 build (0 errors in my files; exit 0, 543.6 s), BV_r01 capture (11 stills, 0 failures), BV-t01 34/36
- BV_r01 is NOT a valid look at the city: Logs/BV-b01.log line 2370 "Shader error in 'Megastructure/NocFacade': undeclared
  identifier 'NC_FaSecRunFill' at NocFacadeSkin.hlsl(412)" - FX's in-flight INT-14 edit (the define was added to
  NocFacadeConstantsGen.hlsl after my build compiled the shader). Every NocFacade tower is missing in the player: the KS03-KS06
  frames show fog with floating signs, and the koi of beat 7 is seen through the missing face B (a consequence, not a koi bug).
- What r01 does show at street level (looked at full size + crops QA/captures/BV_r01/KS04 lower third, KS06 tram x 680-1240):
  - KS06 tram: at the spec position (x 735-1175 px, rows 700-775), head lamps, amber destination band, window band p50 137 luma
    (LOOK_V2 135-155: OK), seat backs, pillars; but the graphite body vanishes into the dark podium: it reads as a floating
    window strip (BV-07).
  - KS04: tram leaving the terminus (left), shops in varied states (lit / dim / shutters, pendants, fascia signs), barriers, posts;
    but the median paving is an even dark plane ~30-37 luma with no lamp pools (BV-02 still open: spots i 18, 150 / 80 deg).
- BV-t01 (QA/reports/BV-t01.xml) 34 / 36: LightBudget 29 > 28 at camera x 69.8 (the third start's soffit / hall windows), and
  Shops lit 18 > 17 on the south side. Tram tests all pass (abeam 272.026 / 258.182, KS06 x -0.239..0.232, y -0.439..-0.300).

### 21:00-21:22 fixes (in the tree, fastcheck OK 21:22; NocTram + NocShopfront fxc 0 failures)
- Light budget (modelled in scratch budget.py, which reproduces the test's 29 at x 69.75): arcade soffit lights' look-ahead 110 ->
  80 m, terminus hall window x 40 -> 50 (faded in 155 m away), depot light window x 30 -> 10 (behind the walker): model max 27,
  worst case 27.
- Lamp pools: median spots i 18 -> 40, cone 150 / 80 -> 120 / 60 deg (full light within 6.9 m of the foot, gone by 20.8 m: pools
  with darker paving between them at the 14 m pitch).
- Shops: lit share 0.30 -> 0.26 (south 16 lit / 39, north 6 lit + 12 dim / 42; scratch shops.py reproduces the test's 18).
- Tram (NocTram.shader): two-tone livery: a light warm-grey shoulder v 2.86..3.45 and an 18 cm stripe over the skirt v 0.78..0.96,
  _StripeColor x 2.1 (albedo ~0.33), box-filtered over one pixel.
- Pole luminaires (TramBuilder.PoleLamps): a mast to y 8.95 and a 1.5 m arm north on every straight overhead-line pole east of x 0
  (7 lamps, z 231.9: outside the sector), the median lamps' luminaire (MedianPromenade.Luminaire, factored out unchanged), halos,
  wet sources at x 60..170. Test PoleLamps_OutsideTheSector_AboveTheTram (incl. beats 1-2 wet max <= 31, no drop).
- 21:23 BV-b02 build + BV_r02 capture queued; BV-t02 queued after it.

### 21:24-21:36 BV-b02 (exit 0, no shader error: FX's NC_FaSecRunFill is in the gen file now), BV_r02, BV-t02 45/45
- BV-t02 (QA/reports/BV-t02.xml) 45 / 45: beat 2 lights at most 27 non-zero (x 140.3), worst case 27, largest weight step 0.00488
  per frame; shops south lit 16 / 39, north 6 / 42; pole lamps 7, lenses y 8.430..8.670, z 231.5..232.3; beats 1-2 wet sources 74,
  at most 29 active (cap 32), no drop; tram tests unchanged.
- lookv2_check QA/captures/BV_r02: KS03 (b1), KS04, KS05, KS06 (b2) PASS. Means KS03 25.3, KS04 35.7, KS05 42.9, KS06 34.1;
  p99 KS03 108, KS04 167, KS05 191, KS06 169 (KS03 is beat 1 by s 117: LOOK_V2 8.2 rule 10 asks >= 90 there; D-032 5 keeps
  >= 120 for beats 2-7).
- Looked at full size + crops (r02 KS04 right tower 1380-1920 x 380-900, lower thirds, terminus 480-1100 x 770-900, KS06 tram):
  - towers back; KS04 lower third now a warm lit wet paving (rows 960-1080: 34-49 luma, the warm pools under the south-row
    lamps at x 1280-1600; base 23-33), the barrier tops lit;
  - the terminus west face has rhythm (pilasters, clerestory with mullions) but the windows read dark with the hall's lamps and
    the Gulf traffic seen through the hall as dots -> hall lights x 1.9;
  - tram livery: the bands at 33 luma on a 20 luma body (column x 880-920 in KS06): visible but weak -> stripe colour x 2.8;
  - NEW DEFECT BV-08: my small signs (third start) read as confetti: flat colour rectangles without readable glyphs over the window
    grid of the shafts (KS04 right tower, KS06 left tower, beat02 look_l / look_r: the podium faces covered with cyan / red / grey
    boxes) -> shaft flat boxes removed, projecting signs only y 43.5..100 (p 0.5, e-folding 30 m), podium flat boxes p 0.08.
    The SmallSigns test's count floor is now > 250 (it was > 400, a content count I set myself; the change removes clutter for
    the picture, see decisions).
- 21:49 BV-b03 build + BV_r03 capture, BV-t03 (+ Q1IntegrationTests, VisibilityAuditTests) queued. BV_r01 deleted (invalid,
  numbers above).

### 21:49-22:12 BV-b03 (exit 0), BV-t03 56/56, BV_r03 (first player run crashed, retry clean)
- BV-t03 (QA/reports/BV-t03.xml) 56 / 56 with Q1IntegrationTests and VisibilityAuditTests (path, floor probe, koi hiding, KS
  subjects) on the new pole lamps, terminus west face and street content.
- First BV_r03 player run: native crash at load in HT's beat 7 (Logs/player-BV-r03.log line 59: Mesh.SetColors <- MeshBuilder.ToMesh
  <- G4Kit.Add <- OldQuarterBuilder.Build <- Beat07_Heart.Build), after beat 2 was built. The same build ran clean on the retry
  (Logs/player-BV-r03b.log, 11 stills, 0 failures): transient, reported to the lead (not a BV file).
- lookv2_check BV_r03: KS03-KS06 PASS; means KS03 25.1, KS04 35.3, KS05 42.8, KS06 33.7; p99 108 / 167 / 191 / 170.
- Tram (KS06 column x 880-920): shoulder band 35-38 luma over a 22 luma roof line and 17-19 skirt; the vehicle now reads as a
  body with a pale band above its windows (crop r03 KS06 680-1240 x 600-800). The pole luminaires light the tram line's rhythm.
- Terminus west face: pilasters + clerestory read as a structured face; the windows still read dark (the view runs through the
  hall and out of its east windows): partly open (BV-05).
- Signs: r02 vs r03 vs base crop of KS04's right podium (1380-1920 x 380-900): most of the remaining rectangles were the podium
  flat boxes and the shaft projecting signs above y 80 -> podium flat boxes removed, shaft signs y 43.5..80, p 0.4 (model:
  174 signs, all low). SmallSigns count floor > 150.
- 22:15 BV-b04 + BV_r04 queued (fastcheck OK 22:15).

### 22:15-22:28 BV-b04 (exit 0, 31.6 s), BV_r04 (final captures), smoke PASS, tram sequence PASS
- BV_r04 (QA/captures/BV_r04, contact QA/captures/BV_r04/_contact/contact_BV_r04_01.png; looked at: KS04 and KS06 full size, KS04
  right podium crop r03 vs r04, contact sheet): the horizontal colour boxes are gone; the podium faces keep their projecting signs
  at the column lines (vertical bars). 745 sign quads in beat 2 (r02: 1 040), 103 810 triangles (budget 520 000).
- lookv2_check BV_r04: KS03 (b1), KS04, KS05, KS06 PASS. image_metrics_BV_r04: KS03-KS06 FAIL only on NaN-like / isolated pixels:
  KS04 25 NaN-like at (870, 521).. = the traffic heads of INT-33 (QA); isolated pixels = shop pendants (BV-10); KS03 1 dark pixel
  = a sub-pixel mullion (BV-11). Banding: KS03 alley_wall_dark 0.105, KS05 cloud_ceiling 0.364 PASS.
- Smoke: run-player BV-smoke "-autotest -brisk -autotest-until 400 -beat 2": AUTOTEST PASS (QA/reports/
  autotest_brisk_from120_until400.json: errors 0, coverage misses 0, safety snaps 0, stop s 400.000; gcAlloc 35.1 B/frame of which
  32 the engine input box, 53 of 6 426 frames allocating, 0 collections).
- Tram no-pop at 2.5 m/s: capture set traffic, seq_traffic_ks06_tram at 960 x 540 (disk W: 14 GB): sequences.py PASS, luma step
  0.085 levels / frame (<= 1.2), 0 difference spikes (QA/reports/sequences_BV_seq01.md); three frames looked at (approach,
  passing, past). Frames deleted. The 1.5 m/s case is covered by TramTests (abeam 258.18, the schedule is a function of tau).
- Deleted: BV_r02, BV_r03, BV_seq01 (numbers above). Kept: BV_r04 (31 MB).
- 22:28 code freeze. BV-final (full EditMode) queued behind FX-final.

### 22:36-22:58 BV-final (full EditMode) 421 / 423
- QA/reports/BV-final.xml: 423 tests, 421 passed, 1 failed, 1 skipped (978.9 s). The failure is HT's
  Beat07HeartTests.PlazaInlay_SameTopFan_RingsSeamsFade ("Expected: greater than 1000, But was: 962"; HT's beat-7 WIP of 2fe0da9;
  FX-final in the same copy fails identically); the skip is AB's AbyssTests.KS10_CompositionSearch. Every BV test passes: beat 2
  lights at most 27 (x 140.3), pole lamps 7, beats 1-2 wet sources at most 29 (cap 32), small signs 174, 721 sign quads.
- fastcheck all OK (23:01). Final build Builds/BV (BV-b04, 0 errors, no shader error), final captures QA/captures/BV_r04.
- New files this wave (all in the tree): none this start; earlier: TramSchedule.cs, TramBuilder.cs, TramDriver.cs, TramLook.cs,
  BoulevardLights.cs, ShopFronts.cs, Shaders/Beats/NocTram.shader, MaterialLibrary.Boulevard.cs, MaterialTests.Boulevard.cs,
  TramTests.cs, BoulevardStreetTests.cs, QA/critique-BV.md (this start).
- Uncommitted BV files for the lead to commit: Beats/Beat02_Boulevard/{Beat02_Boulevard, BoulevardLights, MedianPromenade,
  ShopFronts, TramBuilder, TramTerminus}.cs, Shaders/Beats/NocTram.shader, Tests/EditMode/Beats/BoulevardStreetTests.cs,
  docs/logs/BV.md, QA/critique-BV.md.
BV DONE 23:01

## FIX WAVE (2026-11-21, panel p4_01 on the merged game caddb90; task Logs/fix/BV.md, lead rulings D-040)
Assigned: C15 [S2] human-scale furniture near the walker + D-037 (10) podium-level shop signs; C29 [S2] DREAD 6 empty tram dwell
and terminus lobby; partner S3s C36 (SG arcade room variant), C03 (CanyonBridge ring after AB's Relief change).
Requests to BV checked 02:00 (all copies' logs): only AU's (TramSchedule / TramDriver: doors close at s >= DreadScore.ChimeS),
carried below. Partner FX in this copy changed KS04's pitch 22 -> 27 (D-040 C10): the paving enters KS04 at 16 m, so the KS04
foreground pieces are placed by projection at the new pitch.

### 02:00-02:25 baseline, C15 code, BV-b05 / BV_r05
- Baseline (INTEG p4_01, read-only): KS06 bottom quarter (y 810-1080) mean 31.6 std 5.1; KS04 bottom 16.7 % (y 900-1080) mean
  35.5 std 8.3; beat02_mid lower half an empty paving.
- MedianPromenade.PromenadeFurniture (new): 12 backless benches (3 wood slats on 2 steel frames, seat 0.45, an LED channel under
  the seat that pools on the paving: NocAlleyPool quads 1 cm over the floor), 8 planters 1.0 m, 2 kiosks 3 x 3 x 2.6 m (lit hatch,
  counter, awning, small sign, hatch pool), x 74-194, |dz| 4.75..9.75; planters / kiosks only in the south half inside the tram
  sight sector; Default-layer BoxColliders (MedianPromenade.AddColliders, 1.2 m tall: no step-up) so PathGuide's layer-30 probes
  (coverage wall rays at 1 m, kill volumes) never see them.
- ShopFronts.ArcadeSignList / ArcadeSigns (new, D-037 (10) yes): a projecting light box on 80 % of the column lines (x 35-207
  south, -36-199 north), 0.6-0.9 m out, 1.4-3.5 m tall, y 3.6..7.6 under the arcade head, a 16 cm dark case 6 cm larger than
  the faces, a cap and two brackets to the column.
- Tests (BoulevardStreetTests): Promenade_HumanScaleFurniture_NearTheWalker_ClearOfTheLanes, ArcadeSigns_PodiumLevel_OnTheColumns_Housed;
  ShopPieces test accepts the kiosks' fascia signs.
- BV-b05 02:18-02:23 exit 0 (Builds/BV, 268 s, 0 errors). BV_r05 (11 stills, 0 failures; lookv2_check KS03-KS06 4 / 4 PASS).
  Looked at KS04, KS06, KS05, beat02_mid, look_l (full size) and crops (KS06 benches x3, KS04 kiosk, KS04 street walls):
  - KS06 bottom quarter std 5.1 -> 6.1 (two benches 8.5 / 12.8 m, their pools only a faint haze): not yet >= 9;
  - KS04 bottom band std 8.3 -> 22.4 (planter tops at 14 m in both corners, a kiosk at 40 m, benches, planter at 24 m);
  - the kiosk hatch faced the axis: KS04 saw a blank brown box; StoneWall planters read as crates / a sarcophagus (look_l);
  - arcade signs: small vertical light boxes over the shopfronts at the column lines on both walls (KS04 crops 1400-1920 x
    850-1000 r05 vs p4_01), cased, not confetti; edge-on (invisible) from the median looking north / south, by design.
- Fixes (02:27): the bench pools 3 feet d 0.45 c 4.0 (was 2 feet d 0.36 c 2.6); a bench at x 83 (6.3 m from the KS06 eye, the
  x 142 bench moved); kiosk hatch on the west face (toward the walker), hatch pool x 2.4; planters Concrete with a 4 cm coping.

### 02:20-02:30 C29 code (DREAD 6)
- TramSchedule: DoorOpen(s) (open from s 192.25, fully open s 196.25 = 12 s at 2.5 m/s before the chime, closing from
  DreadScore.ChimeS 226.25 (AU's request: doorsClosing = s >= ChimeS, DoorsClosing), shut at 229.25 < launch 230),
  StepDoors (play: at most 1 / 0.8 s per second, only while Waiting). Launch, abeam (272.03 / 258.18) and KS06 unchanged.
- TramBuilder: two plug-sliding doors per module and side (car-local z -2.9 / +2.9, 1.32 x 2.57 m): north closed leaves in the
  module mesh, south lit doorways (ZWindow) + two moving leaf meshes per module (tramDoorsA / B, children, NocTram: clipped).
  TramDriver slides them 0.64 m apart (capture: DoorOpen(s); play: StepDoors).
- LobbyDoors (new file): the terminus lobby in the west face's southern bay (opening z 214.1-216.9, y 0-3.1, stone reveals,
  a lit tiled room behind it, a ceiling lens, wall / floor pools, two brushed-steel leaves into pockets, a small white sign, a
  street spill pool whose gain follows the opening through a MaterialPropertyBlock). Opens when s first crosses 350 (the lobby
  34-43 deg left of the walk at s 350-366, in frame), 1.5 s / 8 s open / 1.5 s; capture tau = (s - 350) / 2.5.
- Visibility note (geometry, C29): the tram's stop is inside the hall; from the median (s 190-230, 150-200 m away, 10 deg off
  the axis) its south flank is at a grazing 9-10 deg behind the west wall: the open doors cannot be seen from the walk; the dwell
  shows as the tram standing in the lit portal. Recorded as an open item for the lead (C29 acceptance asks for a sequence that
  shows the doors).
- Tests (TramTests): Dwell_DoorsOpen12s_CloseWithTheChime_ShutBeforeTheLaunch, Lobby_DoorsOpen8s_SmoothNoFlash_ReArm.
- REQUEST AB: Tests/EditMode/Beats/Q1IntegrationTests.cs: the promenade furniture stands in the median lane by design (C15,
  D-040); its tops are not walkable floor (Default-layer colliders block the capsule). Change line 103 to
  `static readonly string[] Furniture = { "/promenade" };` and its summary to "Floor furniture the walk crosses or walks around
  by design: the promenade benches, planters and kiosks of beat 2 (BV, C15: colliders, never walkable floor)".

### 02:31-02:55 BV-t05, BV-b06 / BV_r06, debug look BV_dbg01, fixes
- BV-t05 (QA/reports/BV-t05.xml; BoulevardStreetTests|TramTests|Beat02BoulevardTests|Q1IntegrationTests|VisibilityAuditTests|
  ReliefTests): 44 tests, 40 passed. The new tests (promenade, arcade signs, dwell, lobby) pass. Failures:
  - Beat02BoulevardTests.KeyShots_Subjects and VisibilityAuditTests.SpecSubjectDerivations: KS04 projections after FX's D-040
    pitch 22 -> 27 (FX-t10 fails identically). Mine updated: KS04 north blade -0.48..-0.30 / -0.34..0.54, south 0.31..0.49 /
    -0.10..0.74, canyon end -0.81 (projections at pitch 27). VisibilityAuditTests is not mine (FX / HT).
  - Beat02BoulevardTests.Walkability: the kiosk's hatch pool (z +-3.2) came within 0.34 m of the capsule at lat 11 (s 283):
    the pool panel now z +-2.5 (0.5 m inside the lane edge).
  - Q1IntegrationTests.A: 44 walkable tops off y 0 in the lane = the promenade furniture (by design): REQUEST AB above. A BV
    test now proves that every top off y 0 in the east-leg lane (s 190-395) is a "promenade" part.
- BV-b06 02:34-02:35 exit 0 (70 s). BV_r06 11 stills, lookv2_check KS03-KS06 4 / 4 PASS. Measured (luma, Rec 709):
  KS06 bottom quarter (y 810-1080) mean 34.1 std 12.7 (p4_01 31.6 / 5.1; acceptance std >= 9: PASS); KS04 bottom band
  (y 900-1080) mean 34.8 std 24.4 (p4_01 35.5 / 8.3); KS04 p99 180, KS06 p99 174 (D-032 5 >= 120).
  Looked at KS04, KS06 full size, crops (KS06 benches x2, KS04 lower band x2, terminus x4, tram flank x3) and the contact
  sheet: the benches' warm pools read as a lit, used, empty promenade; the kiosk shows its lit hatch and awning to KS04; the
  closed lobby leaves and the white sign show on the terminus west face; the tram's closed doors show as thin frames in the
  window band. Defect: the planters read as black crates in beat02 look_l / look_r (5-7 m away) -> recessed base with an LED
  pool on all four sides (PlanterBaseH 0.10, d 0.25, c 3.2).
- BV_dbg01 (a DEBUG build, reverted at once: LobbyDoors.TriggerS 240 and the tram doors forced open in capture mode, only to
  see the open states; Builds/BV overwritten by the next build): KS06 crop x3 shows the open doorways as lit full-height bays,
  two per module (correct geometry); beat02_mid crop x6 at s 260 (131 m) shows the open lobby as a dim grey hole: the modelled
  tiled room lit only by pools does not read as lit -> the lobby's interior is now a NocShopfront room (G1 EmitCold, gain 0.9)
  on a panel 6 cm behind the leaves (the game's interior shader); the modelled room, its lens and pools are removed.
- DONE BV AU REQUEST (waveb docs/logs/AU.md line 174, TramSchedule / TramDriver, the DREAD 6 dwell): the doors close at
  s >= DreadScore.ChimeS (TramSchedule.DoorsCloseS = DreadScore.ChimeS = 226.25; DoorsClosing(s) = s >= ChimeS); evidence
  TramTests.Dwell_DoorsOpen12s_CloseWithTheChime_ShutBeforeTheLaunch (BV-t05 pass).
- REQUEST QA: Scripts/Runtime/QA/ShotPlan.cs BeatStills(): two DREAD 6 stills for beat 2 (the open states are not in any set):
  `if (i == 1) { list.Add(PathStill(pre + "_dread_dwell", 212.0, -10.0, 2.0)); list.Add(PathStill(pre + "_dread_lobby", 362.0, -36.5, 3.0)); }`
  (s 212: the tram at the stop, doors open 196.25-226.25; s 362: the lobby open 353.75-373.75, bearing 53.5 deg = heading 90 - 36.5).

### 02:53-03:26 BV_dbg02, BV-t06 (killed), BV-b07 / BV_r07, BV-t07, final fixes
- BV_dbg02 (DEBUG build, reverted at once; Logs/BV-bdbg2.log; lobby trigger 240, doors forced open; QA/captures/BV_dbg02):
  beat02_mid crop x6 at s 260 (131 m): the open lobby now reads as a lit, cold, empty doorway (the NocShopfront room), 18 x 18
  px; KS06 shows the open doorways (DEBUG only). The real build never has these values (LobbyDoors.TriggerS 350 checked after
  the revert, TramDriver back to TramSchedule.DoorOpen).
- BV-t06 killed after 12 min (my new lane floor probe: 75 000 brute-force rays over every beat-2 triangle); the test now loads
  only the parts whose bounds reach into the median above y 0.02 and steps 0.5 m.
- BV-b07 03:15-03:18 exit 0; BV_r07: lookv2_check KS03-KS06 4 / 4 PASS; KS06 bottom quarter mean 34.4 std 13.1; KS04 bottom
  band mean 35.2 std 24.5; KS04 / KS05 / KS06 p99 180 / 196 / 174, KS03 p99 108 (beat 1). Looked at the contact sheet and
  beat02 look_r full size: the planter reads as a stone planter (coping, dark soil bed, a faint warm line at its base) next to
  the lit bench; base LED d 0.25 -> 0.35, c 3.2 -> 4.0.
- image_metrics_BV_r07: the known FAILs only (NaN-like / isolated pixels: pendant lamps BV-10, the traffic heads INT-33, and
  the new small arcade signs at the far end, 4-6 px red / amber faces next to the pendants: KS04 (556, 902), (1413, 904),
  looked at x8). Not loosened; listed in critique BV-10.
- BV-t07 (QA/reports/BV-t07.xml): 51 tests, 50 passed. The one failure, Q1IntegrationTests.A (24 rays): q1_floor_probe.txt
  shows them all on Beat02_boulevard/promenadeKiosks at lat 6.51, y 0.023: the probe starts 5 cm over the floor, so the only
  thing it meets inside a kiosk is the body's 3 cm bottom bevel. Fixed in geometry: the kiosk stands on a 0.12 m plinth with a
  6 mm bevel (planter base bevel 6 mm too). The REQUEST AB above is WITHDRAWN (not needed: Q1 stays unchanged).
- 03:26 code freeze; BV-b08 / BV_r08, smoke and BV-final (full EditMode) running.

### 03:26-03:58 final: BV-b08 / BV_r08, smoke, BV-final
- BV-b08 03:30-03:33 exit 0 (Builds/BV, 0 errors, no shader error). BV_r08 (QA/captures/BV_r08, contact
  QA/captures/BV_r08/_contact/contact_BV_r08_01.png): 11 stills, 0 failures; lookv2_check KS03 (b1), KS04, KS05, KS06 4 / 4 PASS.
  KS06 bottom quarter mean 34.4 std 13.1 (p4_01 31.6 / 5.1); KS04 bottom band mean 35.2 std 24.5 (35.5 / 8.3); p99 KS04 180,
  KS05 196, KS06 174 (>= 120), KS03 108 (beat 1). Looked at KS04 and beat02 look_l full size and the contact sheet: the planter
  shows its coping, soil bed and the warm line of its base LED; KS04 unchanged from r07 (planter tops at both bottom corners,
  benches with pools, the lit kiosk hatch facing the camera, the second kiosk, the closed lobby leaves on the terminus).
- Smoke (Logs/player-BV-smoke.log; QA/reports/autotest_brisk_from120_until400.json): AUTOTEST PASS -beat 2 -brisk until s 400:
  errors 0, coverage misses 0, safety snaps 0, kill volume events 0, gcAlloc 35.1 B/frame (32 the engine input box, excess 3.1),
  0 GC collections. The promenade colliders and the lobby / door drivers allocate nothing per frame.
- BV-final (QA/reports/BV-final.xml, full EditMode): 473 tests, 471 passed, 1 failed, 1 skipped. The failure is
  VisibilityAuditTests.SpecSubjectDerivationsReproduceTheSpecProjections (KS04 blade_n198 / blade_s186 / canyon_end): the 14.5
  subject projections in VisibilitySubjects.cs (HT's file) still assume KS04 pitch 22; FX's D-040 pitch 27 moves them (FX-t10
  fails identically). REQUEST HT (with the values; also in the report). The skip is AB's AbyssTests.KS10_CompositionSearch
  (Explicit). Every BV test passes, Q1IntegrationTests passes (the kiosk plinth), TramTests 8 / 8 incl. dwell and lobby, BoulevardStreetTests 9 / 9, Beat02BoulevardTests 9 / 9, Q1IntegrationTests 3 / 3.
- REQUEST HT: Scripts/Runtime/QA/VisibilitySubjects.cs lines 253-255 (KS04 after D-040 pitch 27; projections computed by FX's
  script and asserted by Beat02BoulevardTests.KeyShots_Subjects, green in BV-final):
  `S(k, "blade_n198", VisRule.Whole, Box(198.0, 212.0, 40.0, 140.0, 263.0, 270.0), -0.48, -0.30, -0.34, 0.54, "north blade x 198 (y 40-140) x -0.48..-0.30, y -0.34..+0.54 (D-040 pitch 27)");`
  `S(k, "blade_s186", VisRule.Whole, Box(186.0, 200.0, 60.0, 160.0, 120.0, 127.0), 0.31, 0.49, -0.10, 0.74, "south blade x 186 (y 60-160) x +0.31..+0.49, y -0.10..+0.74 (D-040 pitch 27)");`
  `S(k, "canyon_end", VisRule.Part, Line(new D3(215.3, 0.0, 125.0), new D3(215.3, 0.0, 265.0), 15), N, N, -0.81, -0.81, "canyon end at y -0.81 (D-040 pitch 27)");`
- Kept: QA/captures/BV_r08 (final), QA/captures/BV_dbg02 (DEBUG open states, 3 stills; may be deleted after the review).
  Deleted after logging: BV_r04, BV_r05, BV_r06, BV_dbg01. Disk W: 9.5 GB free at 03:15 (below the 15 GB rule; not BV's
  folders: Builds/BV 464 MB + captures ~40 MB are all of BV's).
- New file this wave: Beats/Beat02_Boulevard/LobbyDoors.cs (+ .meta). Changed: Beat02_Boulevard.cs, MedianPromenade.cs,
  ShopFronts.cs, TramBuilder.cs, TramDriver.cs, TramSchedule.cs, TramTerminus.cs, Tests Beat02BoulevardTests.cs,
  BoulevardStreetTests.cs, TramTests.cs, docs/logs/BV.md, QA/critique-BV.md. Nothing committed (the lead commits).
- Open: C29's tram doors are not visible from the walk (the stop is inside the hall; sight line to the south flank 9-10 deg
  through the portal edge): the dwell reads as the tram standing in the lit portal and AU's chime; lead to rule. C36 (SG room
  variant) not started. C03 after AB's Relief change. BV-10 small bright pixels (pendants, far arcade signs) in image_metrics.
BV DONE 03:58
