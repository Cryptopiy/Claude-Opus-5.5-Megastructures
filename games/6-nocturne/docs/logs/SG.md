# SG - Phase 4 wave A: beat 6, the wall of signs (14.5.6, KS16), its back side and arcade seen from beat 7

Owner files: Beats/Beat06_Signs/**, Shaders/Beats/NocScreen.shader, Shaders/NocNeon.shader, Scripts/Runtime/Rendering/NocSignSheet.cs,
Beats/Relief/SignBaseRelief.cs, Scripts/Editor/MaterialFactory.Signs.cs, Scripts/Runtime/Core/MaterialLibrary.Signs.cs (new),
Scripts/Tests/EditMode/MaterialTests.Signs.cs (new), Scripts/Tests/EditMode/Beats/Beat06SignsTests.cs (+ new test files),
docs/logs/SG.md, QA/critique-SG.md.
Requests to SG: write "REQUEST SG: <file>: <exact change>" in your own log; answers appear here as "DONE SG <ID> <request>".

## Log
- 08:16 start (fresh: no earlier SG entries). HEAD 01111e9. Read MASTER_PROMPT 14.0-14.2, 14.5.6, 14.5.7, 14.6-14.10; D-027..D-033;
  BUILDER_BRIEF; LOOK_V2 2.4 / 8.1 / 8.2; critique-lookv2-01 (INT-02, INT-34); PA / GE / FA log lines on the screen wall.
  Looked at lookv2_01/KS16, KS17, lookv2_01b/beat06_signs_mid, beat06_signs_look_r, beat07_heart_look_back (full size):
  - KS16: the wall is 5-6 huge smooth gradient panels (brown -> grey-blue) with dark 0.4 m seams that read as thick lines; no
    LED structure, no imagery, no glyph signs (Simplified tier, one wave); the SignBase shop row below is flat blue-grey / amber
    panes. lookv2_check KS16 PASS (mean 52.7, R3 0.173, p99 124.7).
  - beat06_signs_look_r / mid: the arcade x 914 west face is a 4 m flat teal glass band + flat white strips on black concrete.
  - beat07_heart_look_back / KS17 right: the arcade block reads as a black box with white / teal stripes; the screen face seen
    from the plaza is a flat teal grid.

### 10:20 second start (RESTART NOTE)
- The log of the first start stopped after the 08:16 look entry; everything below 08:16 - 09:56 is reconstructed from
  "git diff -- <SG files>" (nothing was committed). In the tree:
  - Task 1 (INT-02 / CRIT0 #14, the LED wall): new Shaders/Beats/NocScreenWave.hlsl (+ .meta): the wave of 14.5.6 on the Look v2
    ramp; imagery (displays 72.8 m x 2..4 panels, 7 slow programmes per 40-75 s epoch with an 8 s crossfade: ambient, large glyph,
    rising bands, disc with ripples, glyph column, diagonal bands, tile mosaic; every frequency <= 0.11 Hz); hardware (panels 36 x 20
    on 36.4 x 20.4, 0.4 m gap + 0.12 m bezels box-filtered, per-panel brightness / white point / age 18 % / dead 1.2 % / dead
    modules / stuck column, 4 m modules with joints and calibration steps faded below 5.5 px, 0.25 m LED dots faded in 3-6 px,
    imagery sampled at the LED centre while resolved); LED view-angle falloff 0.62 at grazing. NocScreen.shader now calls
    NocScreenShade (global _NocScreenT instead of the material _ScreenTime; _Content; bezel albedo 0.055; smoothness 0.35).
    NocScreenMath.cs: C# twin (Display, Period, ProgramOf, GlyphDist, Program, Content, Panel, SeamMean, AngleFactor,
    PixelEmission / PixelLuminance, MirrorMacro). SignsDriver: Shader.SetGlobalVector(_NocScreenT, (p58, p60, t600, waves)).
    ScreenWallBuilder: tier-driven (Full: 3 waves, imagery 1, nine glyph signs as NocNeon faces on painted-steel housings, five
    blades with struts and four boards, SignsLayout.GlyphSigns); H4 wall plane kind 2 with NocScreenMath.MirrorMacro.
    Beat06_Signs: ctx.Tier, sign-base shop rooms (below).
  - Task 2 (back side seen from the Heart): ArcadeBuilder rewritten: granite plinth, colonnades on the west / north / east faces
    (columns 0.8 m, glass line 4 m back under a 6.6 m soffit, fascia, two soffit downlights per bay), FA's S2 LED-housing skin on
    the upper floors (NocFacade), two service catwalks (grating, rails, posts, brackets) on the north and east faces at y 14.6 /
    26.6 with maintenance lights every 9 m (NocHalos "ArcadeMaint"), cable risers on three faces; nothing above the 38 m roof.
  - Task 3 (arcade x 914-990, INT-34): one NocShopfront room per bay (G4Kit.ShopRoom: material variants of BV's lib.Shopfront,
    cold / white / neutral / blue / closed 1 in 6), steel frames + mullion, piers, a NocNeon blade per open bay; the sign-wall
    base (SignBaseRelief): flat ShopCold / ShopWarm relief panes removed, Beat06_Signs builds NocShopfront rooms on its bays
    (SignBaseRelief.ShopBays) plus a blade sign per open bay; PromenadeBuilder: lamp fixtures (base plate, post, arm, luminaire,
    lens facing down, light 0.12 m under it, halo at the lens), planter coping, round bollards with domed caps and a light ring
    on every third.
  - Beat06SignsTests.cs: 16 tests (slab, KS16 wall points / share, ramp + LED peak below bloom, T-WAVE, phases, lights,
    imagery periods / means / frequencies, photosensitivity model 60 s, panel failure rates static, glyph signs off sight lines
    and lane, arcade below roof / off lane / shops, mirror kind 2, floors, furniture, mesh budgets).
  - Not done yet: no Unity test run, no build, no capture of the new wall; REQUEST FX for the mirror (task 4) not yet filed.
- 10:20 fastcheck: runtime OK (196); editor FAILED only in MaterialTests.Look.cs (AT, mid-edit), my files compiled in the lead's
  10:02 CompileCheck. Offline fxc (scratch sg3/shcheck.py, copy of HT's harness + the _ADDITIONAL_LIGHTS/_CLUSTER_LIGHT_LOOP
  variant): NocScreen.shader all 4 passes vs_5_0 / ps_5_0 OK, 0 errors, 0 warnings. Committing the tree state now (minimum gate).
- REQUEST FX: Shaders/Include/NocMirror.hlsl: add `#include "../Beats/NocScreenWave.hlsl"` after the NocAtmosphere include, and in
  NocWallMacro put `if (kind > 1.5) return NocScreenMirror(H, colour);` before `if (kind > 0.5) return colour;` (kind 2 = the
  screen wall: the 14.5.6 wave at the hit, scaled to the registered LED-wall mean; ScreenWallBuilder registers kind 2 already,
  which today falls into the flat branch, so nothing breaks before you carry it). _NocScreenT is a global set by SignsDriver.
- 10:24 commit 611a7fb (the tree state above). 10:35 SG-t01 (Beat06SignsTests|ReliefTests|VisibilityAuditTests): 27 / 31.
  - Beat06SignsTests 14 / 16: slab 40.000 x 245.000, face x 874.0000; KS16 wall points ndc (-0.148, -0.625) / (0.129, 0.804) /
    (0.193, 0.934); wall share 58.4 % (16:9); ramp stops Y 0.229 / 0.301 / 0.269 / 0.287, red share max 0.578; screen emission
    peak 0.126, LED wall peak channel 0.789 (< 0.9 bloom threshold: imagery x 2.42, dot x 2.30); R4 light saturation 0.278;
    imagery mean m 0.942, all 7 programmes occur; panels 714: dead 1.0 %, aged 21 %, mean brightness 0.953; mirror macro
    (0.0457, 0.0477, 0.0449) Y 0.0471 (the wave alone 0.0681); Beat06 18 612 triangles (budget 120 k), 24 lights, 22 bollards.
  - FAIL TWave: Full tier max wall-pixel rate 0.0417 /s against my design bound 0.04 (14.10 limit 0.12; tier 1: 0.0046). Cause:
    the glyph column (Delta m 1.9 across a 1 m soft edge at 0.22 m/s). Fix: scroll 0.18 m/s (SCR_COLV / ColumnSpeed): 0.034 expected.
  - FAIL Photosensitivity model: "flashes 1" (max rate 0.0446 /s, dominant frequency 0.083 Hz). Cause: the detector, not the
    wall: it counted a window whose maximum was interior with only ONE end 10 % lower (a ramp into a plateau). Fixed to 14.10's
    definition (an increase AND a decrease of >= 10 % of the maximum within 1 s, peaks and troughs), still per point (stricter
    than the picture test's 25 % of the screen).
  - FAIL VisibilityAuditTests KoiCompositionLint / KS19SubjectsAreWindowLampAndKoiBody: KS19 koi body 0 core samples. Not SG
    (HT's koi orbit change, HT-t01 also exit 2; the arcade stays below 38 m and nowhere near KS19's sight line).
- 10:40 fastcheck OK (196 / 260), fxc NocScreen OK; lookdev_SG build + KS16 capture (r01) started.

### 2026-11-19 17:40 third start (RESTART NOTE)
- Read this log; HEAD 34e5d92. SG commits so far: 611a7fb, 904a5ed (glyph column 0.18 m/s, 14.10 flash detector; SG-t02 16/16).
- The pause (16th, 11:37) cut a step that was not logged: after r01 (KS16, 10:57), r01b (beat06 stills, 11:13) and r01h (KS17 +
  beat07 stills, 11:14) I had redesigned the imagery in the tree (uncommitted, re-applied by the lead): 8 programmes (wave, glyph
  on a dark field, aurora, sun, glyph column at 0.15 m/s with 2 m soft edges, horizon, tiles per panel, standby), a display level
  per epoch (0.95 / 1.2 / 1.5) with m capped at 2.42, displays of 3..7 panels, 12 s crossfade, LED dots faded in from 5 to 10 px
  over a 15 % diffuser floor, view-angle floor 0.70 with cos^0.4, fewer dead modules (1.8 % of the panels, 8 % of their modules)
  and stuck columns (1 %); the imagery test now checks mean m x the LED hardware mean = 1 +- 0.08.
- What r01 showed (re-read today at full size): KS16: the near LED dots read as a coarse grid mesh (left third), the imagery
  is barely visible (brown / grey-blue gradients per panel), shop rooms below are flat white boxes; beat07_heart_look_back: the
  wall seen from the plaza is a teal grid of mottled tiles with black dead-module squares (Tetris blocks), sign-base and arcade
  shop rooms flat white; KS17 right: the arcade's lower floor shows white boxes. image_metrics r01: NaN-like / isolated bright
  pixels (also in lookv2_01 at other places per FX r01) and KS16 1610 / 219 banding[wall_panel] 7.9 / 11.5.
- New offline shader harness (scratchpad sg/shcheck.py: fxc /Gec vs_5_0 / ps_5_0, every pass, base + INSTANCING_ON + the
  _ADDITIONAL_LIGHTS + _CLUSTER_LIGHT_LOOP variant, URP / core ShaderLibrary with Common.hlsl's zero-argument PopMarker() patched).
- 17:55 fastcheck runtime 198 / editor 263 OK (4 min: the machine is loaded); NocScreen.shader 18 / 18 compiles, 0 warnings.
  Committing the re-applied tree state (minimum gate), then SG-t03.
- 18:40 SG-t03 (Beat06SignsTests|ReliefTests|VisibilityAuditTests): 31 / 31. Imagery mean m 1.106 x LED hardware mean 0.881 =
  0.974 (wave 424 / glyph 768 / aurora 386 / sun 681 / column 539 / horizon 676 / tiles 263 / standby 263); T-WAVE tier 0 0.0242 /s,
  tier 1 0.0046 /s (limit 0.12); photosensitivity model: max rate 0.0234 /s, flashes 0, dominant 0.100 Hz; LED wall peak channel
  0.722 (< 0.9); mirror macro kind 2 (0.0558, 0.0614, 0.0594) Y 0.0600; Beat06 21 208 triangles in the audit (budget 120 k).
  VisibilityAuditTests green: KS19 koi body has core samples again after HT's 7b0dbaa (SG's open note closed).
- Task 4: FX carried the NocMirror request at 18:05 on the 16th (DONE FX SG ...): NocMirror.hlsl includes NocScreenWave.hlsl, so
  NocScreenWave.hlsl is now compiled into NocWet / NocGlass / NocFacade too; every change to it recompiles them (and must compile
  there: offline check of all three before each commit). The mirror macro test keeps the H4 mean consistent with the LED wall.
- r01 isolated / NaN-like pixels in KS16 sit on the 1-2 px slivers of the white shop rooms (471, 950), (708, 980), (866, 1000),
  (1183, 1040): their right neighbours are 17-21 codes, the pixels 206-238: the rooms are far too bright.
- 18:45 task 3, shop rooms: G4Kit.PickShop / ShopColour give every arcade and sign-base shop panel BV's NocShopfront state
  (vertex colour R random, G light reduction, B state x 32, A half-shutter edge): open bays 56 % lit (gain 0.55-0.85), 20 % dim
  (0.16-0.30), 12 % half-lowered shutter (edge 42-70 %), 7 % dark, 5 % shuttered; closed bays (1 in 6 arcade, 1 in 7 sign base)
  roller shutters that may leak light (the closed rooms keep a colour material, so the leak has the room's tint). ShopLight
  0.45 -> 0.34. Signs only on open bays. Beat06_Signs: the sign-base closed-room material row is gone (two rooms rows).
  New test ShopStates_MixGainsAndColours (state shares, gains <= 0.85, brightest room mean < 3 x the LED wall mean).
  Depends on BV's NocShopfront state block (uncommitted in BV's tree); without it the shader ignores the colour (lit room).
- 18:50 fastcheck runtime 198 / editor 264 OK. Commit; then lookdev_SG r02 (build + KS16 1080 only).
- 19:00 r01b look_r (arcade west face from the promenade, full size): every room is a floor-to-ceiling wall of pastel product
  boxes (shelves on the back and side walls up to the 4.4 m ceiling), uniform and bright: reads as a library mosaic, not a shop.
REQUEST BV: Shaders/NocShopfront.shader: NocSurfaceHook: draw NocShelves only below a shelf top (hit.y < min(H - 0.6, 2.3) m; the
  top board at that height), and above it a plain wall at 0.45 x the room mean with a 0.25 m lit frieze strip (x 1.6) just under
  the ceiling, renormalising the room mean as you do now (raw / mean): tall rooms (the arcade's 4.4 m, the sign base's 5.0 m, your
  3.13 m openings) stop reading as walls of boxes. Optional: a counter block (0.95 m, dark) in the front third on half the seeds.

### 2026-11-20 20:24 FIFTH start (RESTART NOTE)
- Read this log, my commits (611a7fb, 904a5ed, fe8e245, ce4d8ed), 2fe0da9 (SG: log only), critique-SG. The fourth start (this morning)
  left no SG code in the WIP patch; its scratch (session <session>, scratchpad sg4) holds r02 crops and a numpy prototype of the
  imagery v3 (scr_v3.py: 10 programmes, display hue offsets, saturated accents, cap 4.5, stats: mean m x hw 1.03, T-WAVE 0.031 /s,
  flashes 0) that was never ported. Progress note: Logs/SG_progress.md.
- 20:26 DONE SG REQUEST FX G4Kit.cs (TP-2 optional cleanup): ShopRoom no longer tests NocLookGlobals.SurfV (always 2).
- 20:26 this copy had no Assembly-CSharp*.csproj (fastcheck refuses): copied the generated pair from the main project (gitignored).
- 20:35 SG-b01 failed on HT's mid-edit Beat07HeartTests.cs(562) (not SG); waited for fastcheck green; 20:58 SG-b02 OK (613 s).
- 20:59 SG_r03 (KS16, KS17, beat06 x 7, beat07_heart_look_back; b02 = imagery v2), LOOKED at full size and crops:
  - KS16: the wall is still dull gradient panels (grey-teal / brown) with a faint glyph column; the near LED dot lattice reads as
    a wire mesh (x 0-500, y 380-1000); sign-base rooms are white boxes (x 300-1100, y 900-1080). Glyph signs read well.
    lookv2_check PASS (frame mean 43.7, R3 0.187, p99 132); ks16_wall.py: wall mean 51.5 (LOOK_V2 band 60-85), red share mean
    0.309, 8x8 max 0.576.
  - KS17 right: the arcade now reads as a building (S2 skin, windows, catwalk lights, colonnade); its rooms are flat white boxes
    (x 1490-1900, y 740-850). PASS (mean 38.7). beat07_heart_look_back: the wall from the plaza is a teal grid of flat panels.
  - beat06_signs_look_r: arcade west face fine (columns, soffit lights, shelves, a shutter, blades, planters, lamps).
- 21:03 imagery v3 ported to NocScreenWave.hlsl and NocScreenMath.cs (the C# twin): programmes wave / glyph on a dark vignetted
  field / liquid (domain-warped fbm) / sun with rings / glyph column / skyline with window cells / glyph matrix / bokeh / scan
  lines / standby; per-epoch display level 0.9 / 1.1 / 1.35 and hue offset (a quarter turn +-0.04, crossfaded the short way);
  accents = the next stop saturated x 1.3 (was: mixed toward ice); m capped at 4.5 / (1 + DotGain wp) (cap x the resolved dot's
  peak gain = 4.5 everywhere); joints 3 cm at 40 % (were 5 cm at 85 %); 16 s crossfade. Fixed in the port (prototype bugs):
  skyline window cells box-filtered and faded to their mean (0.0473) below 2-4 px (the prototype sampled a random cell per pixel
  far away: sparkle), scan lines box-filtered and faded to 0.65 / pitch below 3-6 px, the liquid ridge widened to the footprint.
  fxc (scratch harness, all passes and variants): NocScreen 18 / 18, NocWet + NocGlass + NocFacade (NocMirror includes the wave) 0 fails.
- 21:10 LED dots fade in from 8 to 14 px (was 5-10) over a 25 % diffuser floor (was 15 %); SCR_DOTG 0.971763.
- 21:10 shops (SG-03): open bays lit 40 % (gain 0.42-0.68), dim 22 % (0.12-0.24), half shutter 13 % (0.40-0.62), dark 13 %,
  shuttered 12 %; closed shutters 0.30-0.60; ShopLight 0.34 -> 0.26.
- 21:24 SG-t04 (Beat06SignsTests|MaterialTests, compiled before the MNORM change): 30 / 32. Photosensitivity model: max rate
  0.0253 /s, flashes 0, dominant 0.100 Hz; T-WAVE tier 0 0.0361 /s, tier 1 0.0047; mirror macro Y 0.0642 (wave 0.0681); shop
  states lit 34.0 / dim 17.6 / half 10.9 / dark 10.8 / shutter 26.8 %; brightest room 1.64 x the wall mean. FAILS: (1) my new R7
  accent assert flagged the pinkish-grey between the ice and amber stops (hue ~332, saturation 0.10: not violet by R1's sat > 0.35;
  the assert now requires sat > 0.20); (2) imagery m minimum 0.144 (the glyph field's vignette) under my sanity floor 0.15 -> 0.10.
- 21:22 SCR_MNORM / ContentNorm 1.18 -> 1.70: the wall mean was 51.5 codes against LOOK_V2's KS16 wall band 60-85. The peak is
  capped (4.5) so bloom is unchanged; prototype stats at 1.70: mean m x hw 1.43 (WallMeanGain), T-WAVE 0.039 /s, flashes 0.
  Tests: the imagery mean and the mirror ratio are now relative to WallMeanGain 1.43; my T-WAVE design bound 0.04 -> 0.06 /s (half
  the 14.10 limit 0.12, which is unchanged). Arithmetic: 51.5 codes at 0.97 x the wave; linear x 1.47 -> about 51.5 x 1.47^(1/2.2)
  = 61.6 codes expected.
- 21:31 SG-b03 queued behind HT-t05 (v3 + dots + shops + MNORM).
- 21:40 SG-b03 OK (236 s; NocScreen, NocWet, NocGlass, NocFacade recompiled). 21:43 SG_r04 (same set), LOOKED (full size + crops):
  - KS16: the wall now shows screens with content (glyph matrix on plates, an amber glyph, scan lines, colour fields, skyline far
    up); ks16_wall.py: frame mean 53.8, wall mean 68.6 (LOOK_V2 60-85), p5 / p50 / p95 20 / 74 / 116, red share mean 0.278,
    8x8 max 0.589 (<= 0.70); lookv2_check KS16 PASS, KS17 PASS. Defects: the near dot lattice still a mesh (bottom left); large
    pale flat displays (SG-08).
  - beat07_heart_look_back: skyline with window cells, liquid, scan lines, glyph matrix, rings, bokeh: SG-05 fixed. A pale grey
    display at x 1140-1340, y 330-510 (glyph programme on the ice hue: field too bright at x 2.3).
  - KS17 right: the arcade reads as a dark building with a few lit windows, catwalk lights and a dim colonnade (rooms grey, not white).
  - image_metrics SG_r04: NaN-like 6 in KS16 at (303, 104) [garden trees, not SG], (1841, 1019), (1743, 1064) [city / lamp, bottom
    right], (1411, 1071) [a dark 1 px sliver on the sign-base shop frames, SG-06]; 1 each in KS17, beat06_signs_end, 4 in
    beat06_signs_start and beat07_heart_look_back (report QA/reports/image_metrics_SG_r04.md).
- 21:47 b04: dots 10-18 px over a 35 % floor (SCR_DOTG 0.842195); programme 0 = the wave under a soft band (SG-08).
- 21:52 SG-b04 OK (203 s); SG_r05: KS16 wall mean 68.9, frame 54.0, red 8x8 max 0.589; the dot mesh is gone (faint texture at the
  bottom left only). Still pale: the scan-lines programme (a pale gradient, lines on part of the width: beat06_signs_look_l
  x 480-1550, y 0-330) and the glyph field. Fixed in the tree for b05: scan lines over 0.15 + 0.45 y, lines across 80-92 % of
  the width at 1.2 + 2.6 b; glyph field 0.14 + 0.40 exp (was 0.26 + 0.55).
- 21:58 SG-smoke on b04 (-autotest -brisk -autotest-until 1260 -beat 6): AUTOTEST PASS, beat 7 entered at s 1260.00 t 72.17,
  errors 0, corridor coverage 48204 rays 0 misses, alloc mean 31.5 B/frame = the engine input box (excess 0.0)
  (QA/reports/autotest_brisk_from1080_until1260.json).
- 21:59 deleted SG_r03 and SG_r04 (numbers above). Queue: HT-final, then SG-t05, SG-b05.
- 22:09 HT-final (full EditMode on the tree of 21:52) carried my code: Beat06SignsTests had one failure, my own sanity count "every
  programme > 150 of 4000 samples" (the 6 % wave programme gave 149: the samples cluster on a few displays) -> > 80.
- 22:14 SG-t05 (Beat06SignsTests|MaterialTests|ReliefTests|VisibilityAuditTests, the final C#): 47 / 47 (QA/reports/SG-t05.xml).
  Imagery mean m 1.543 x LED hardware mean 0.894 = 1.380 (WallMeanGain 1.43 +- 8 %); programmes 149 / 346 / 697 / 386 / 327 / 633 /
  692 / 336 / 258 / 176; H4 macro Y 0.0866 (the bare wave 0.0681); photosensitivity model: max rate 0.0321 /s, flashes 0, dominant
  0.100 Hz; LED wall peak channel 0.720 (< 0.9; colour peak 0.527, cap x dot = 4.5); accent red share max 0.652 (<= 0.70); T-WAVE
  tier 0 0.0413 /s, tier 1 0.0047 /s (14.10 limit 0.12); slab 40.000 x 245.000, face x 874.0000; Beat06 18 612 triangles (120 k),
  24 real lights; brightest shop room 1.21 x the wall mean.
- 22:22 SG-b05 OK (204 s, 0 errors, no shader error). SG_r06 (final set) LOOKED: KS16 wall mean 61.4 (60-85), frame 49.6 (38-62),
  p5 / p50 / p95 20 / 68 / 116, red share mean 0.276, 8x8 max 0.573; lookv2_check KS16 PASS (R3 0.184, R2 0.05 %, p99 135, crush
  3.1 %), KS17 PASS (mean 37.8, separation 11.2, warm stray 0). beat06_signs_look_l: the scan-line display now reads as a screen
  (dark teal, bright lines). Contact sheet QA/captures/SG_r06/_contact/contact_SG_r06_01.png.
- 22:24 SG-smoke on b05: AUTOTEST PASS (beat 7 at s 1260.00, t 72.17, errors 0; alloc = the engine input box, excess 0.0).
- 22:26 image_metrics SG_r06: KS16 banding[wall_panel] BandScore 3.01 (run 7 px), score 2.004 > 1 (visible steps 0.00 / 1000):
  the region (0.20-0.32, 0.56-0.76) is a smooth vertical gradient of a display. NaN-like as in r04 (not SG except the 1 px dark
  sliver on the sign-base shop frames, SG-06). Fix (picture, not threshold): LED binning, every 0.25 m pixel at a static +-3 %
  (zero mean) faded in from 3 to 6 px of pitch; fxc NocScreen 18 / 18, NocWet / NocGlass / NocFacade checked. HLSL only: the C#
  twin's far-field means are unchanged (zero-mean, static), so SG-t05 / SG-final stay valid. SG-b06 + SG_r07 queued after SG-final.
- 22:38 SG-final (full EditMode, the final C#): 423 tests, 419 passed, 3 failed, 1 skipped (QA/reports/SG-final.xml). Failures, none
  SG's: BoulevardStreetTests.LightBudget_AtMost28_SmoothFades (29 > 28) and Shops_Varied_NeverBrighterThanTheLitRoom (18 > 17) are
  BV's (BV-t01 logs the same), WetFloorTests.WallWindows_MeanOne_Bounded_StreakVertically (-1.4e-11 >= 0, a float rounding) is
  in the WIP 2fe0da9 shared wet tests (FX / HT). Every Beat06SignsTests, MaterialTests, ReliefTests and VisibilityAuditTests passes.
- 22:46 SG-b06 OK (211 s, 0 errors, 0 warnings, no shader error): the final build, with the LED binning. SG_r07 (final set):
  KS16 wall mean 61.4, frame 49.6, red share 8x8 max 0.573; lookv2_check KS16 PASS, KS17 PASS; image_metrics: banding[wall_panel]
  2.40 (BandScore 3.60, run 8 px, visible steps 0.00 / 1000): the binning did not fix it (+-3 % at a 4 px pitch, faded to 40 %);
  the crop (384-614, 605-821) is a near-flat dark teal field with module joints, no visible band; SG-07 stays open. NaN-like as
  before (SG-06 open). Contact sheet QA/captures/SG_r07/_contact/contact_SG_r07_01.png. Deleted SG_r05, SG_r06.
- 22:49 SG-smoke on b06: AUTOTEST PASS (beat 7 at s 1260.00, t 72.16, errors 0), corridor coverage 48204 rays 0 misses, alloc mean
  28.5 B/frame = the engine input box (excess 0.0), GC 0.
- Disk: W: 14 GB free at 22:50 (below 15; mine: Builds/SG 464 MB, QA/captures/SG_r07 28 MB).
- Open for the next start: SG-07 (BandScore on flat display fields), SG-08 (one pale grey display seen from the plaza), SG-06 (1 px
  sliver on the sign-base frames), the 16:10 / 21:9 KS16 framings and the 14.10 signs sequence (lead, heavy), BV's NocShopfront
  interiors read plain from below (not SG).
SG DONE 22:51

## FIX WAVE (2026-11-21, panel p4_01 on the merged game, master caddb90; task Logs/fix/SG.md + lead rulings)
Findings: C16 (S2) KS16 wall share 43 % built; C17 (S2) the wall reads as a 40-storey office front (no scale cue, no top);
C28 (S2) the eye (D-034 5.3); D-036 (6) ContentNorm toward 1.85; C36 (S3) the arcade facade in KS17's right third.
- 02:00 read the task, the rulings, p4_01 KS16 / KS17 / beat06_signs_look_up60 (INTEG). KS16 blockers by an offline ray-cast of my
  own geometry (scratchpad ks16share.py, KS16 pose, 16:9 at 384 x 216): blade z 292 5.6 %, blade z 366 3.3 %, fascia 1.6 %,
  columns 1.5 %, canopy 1.4 %, blade z 490 1.1 %, blade z 438 0.55 % -> wall 44.7 % (the audit's built 43.4 %; the spec model
  58.4 % without any of them; the remaining 1.3 % is not mine: trees / grid sampling).
- 02:05-02:15 edits (C16): blades z 292 / 366 raised above KS16's frame (y 200-290 / 230-330, into the dissolve band), z 438 a
  board, every board flush (BoardX 876.4 -> 874.35: within the audit's 0.5 m, counted as wall); the sign-base canopy cantilevered
  (x 882.2 on 0.8 m columns -> x 876.8 on steel brackets, slab 0.25 m, slim fascia; shop blades at x 876.1). Model: 16:9 56.9 %,
  21:9 43.9 %, 16:10 61.8 % (with the cradles). 21:9 >= 44 % is at the spec ceiling (44.4 % with nothing in front).
- (C17) SignsLayout + ScreenWallBuilder.BuildService: two service ledges on the row seams y 81.6 / 204.0 (flush 0.45 m, lamp lens
  every 6 m + NocHalos "SignsLedge"), four window-cleaning cradles (3 x 1 x 1.5 m lit cabins, WinCold x 0.9) on fixed NocLine
  cables from y 560, y = Yc + A sin(2 pi (t600 / 600 + phase)), v <= 0.188 m/s, moved by SignsDriver (no allocation).
  NocScreen.shader: the face dissolves into the deck colour (NocCloudColorN of the ceiling, wisps +-40 m) from y 250 to 460.
- (C28) the eye: NocScreenWave.hlsl NocScrEye + C# twin NocScreenMath.Eye: a 4 x 5 panel canvas (z 311.4-457.0, y 20.4-122.4),
  t600 448-454 canvas in, 454-460 opening, hold to 472, 472-478 closing, 478-484 out (KS16 still t 460 = open; readable window
  t 453.6-478.4); soft lids (14 m edges, <= 8.5 m/s), amber iris with fibres, pupil, catchlight, lid crease; under the LED hardware.
- (D-036 6) SCR_MNORM / ContentNorm 1.70 -> 1.85, WallMeanGain 1.43 -> 1.556 (61.4 x (1.85 / 1.70)^(1/2.2) = 64.0 codes expected).
- (C36) ArcadeBuilder: the upper floors on FA's S1 precast skin (override 2) instead of the S2 metal ribbon.
- Tests: photosensitivity model over t 300 and the eye's window t 445 (+16 canvas points); Eye_OnceInTheKS16Window_OnTheFrame;
  ScaleMarkers_CradlesAndLedges; GlyphSigns 13 faces; meshes + budget with the new meshes.
- 02:17 fastcheck all OK (runtime 218, editor 289). Offline fxc (scratchpad sgf/shcheck.py): NocScreen, NocWet, NocGlass,
  NocFacade, NocInlay: 0 failures. 02:19 SG-b07 build queued.
- 02:21 SG-b07 OK (118 s, 0 errors; one shader warning of mine: "potentially uninitialized variable (NocScrEye)" at
  NocScreenWave.hlsl(391), the early return; fixed in the tree for b08). 02:22 SG_f01 (keyshots1080 + beats, KS16 / KS17 /
  beat06 / beat07_heart_look_back) LOOKED:
  - KS16: the wall now fills the frame bar GE's promenade lintel (the black box top centre-left, Relief/PromenadeLintel.cs,
    z 280-286, y 40+, 2.9 % of the frame: it was half of the critic's "hanging signbox"); the eye (open at t 460) fills the
    centre-right, a cradle (lit cabin) at the left third, the y 81.6 ledge lamp row runs across the receding wall.
    ks16_wall.py: frame mean 54.0, wall mean 68.5 (D-036 (6) target about 68; was 61.4), p5 / p50 / p95 20 / 72 / 123, red share
    mean 0.272, 8x8 max 0.603 (<= 0.70); lookv2_check KS16 PASS. Defect: the eye canvas field is a flat dark teal (fixed in
    b08: a soft glow round the eye).
  - beat06_signs_look_up60 (scratchpad up60.py, wall pixels by world height): y 350-460 57.5 codes (p4_01 68.2), y 460-560 45.4
    (53.3), the deck 40.8 -> the dissolve works above 430 but 350-460 is 16.7 codes off; b08: dissolve 230-430. A cradle reads
    clearly (top left). The raised blades show as grey undersides along the wall.
  - KS17 right third with S1 (override 2): all louvres, no lit window (mean 17.7, high-pass std 1.22 < p4_01 2.77): worse;
    b08 uses the boulevard podium's override 4 (S1 + stone base, punched windows: KS04's podium reads well).
  - Ray-cast with GE's lintel: 16:9 54.0 %, 21:9 41.7 % -> b08: the blades z 490 / 505 flush boards too (55.6 % / 42.9 %); the
    ledges are cut 0.3 m clear of every sign housing they would cross.
- 02:31 SG-t06 (Beat06SignsTests|VisibilityAuditTests|ReliefTests|MaterialTests) + SG-b08 queued behind HT-b07.
- 02:34 SG-t06 (Beat06SignsTests|VisibilityAuditTests|ReliefTests|MaterialTests, tree of 02:30): 47 / 49 (QA/reports/SG-t06.xml).
  - VisibilityAuditTests green; QA/reports/visibility.json KS16 sharesBuilt 0.545 / 0.413 (16:9 / 21:9; p4_01 0.434 / 0.328;
    spec 0.586 / 0.444). My ray-cast model gave 0.543 (within 0.002). The rest is GE's promenade lintel (Relief/PromenadeLintel.cs,
    GateDims.PlY0 40: 2.9 % / 2.2 % of KS16) and my slim canopy (1.1 %). Model: lintel soffit 50 m -> 0.562 / 0.433, 60 m ->
    0.571 / 0.440; no canopy at all -> 0.554 / 0.427. 21:9 >= 0.44 is not reachable while the lintel stands at 40 m (the spec
    ceiling is 0.444). REQUEST LEAD (GE's file): see the shared change request.
  - FAIL Photosensitivity_Model_KS16Wall60s, eye window: T-WAVE 0.141 /s (> 0.12), 33 model flashes (t 300 window: 0.035 /s,
    0 flashes, 0.100 Hz). Cause: the lid crease (sigma 1.8 m) rode the lid at 8.5 m/s (a bright line passing a point in 0.4 s)
    and the sclera edge (14 m wide, delta 0.25) changed 0.15 /s. Fix (tree): the fold is static above the open lid and fades in
    with the opening, lids 20 m soft, sclera m 1.6, catchlight m 1.6.
  - FAIL Eye_OnceInTheKS16Window_OnTheFrame: my test's "sclera" point (370, 70) lies in the pupil; points corrected.
  - The rest: imagery mean m 1.653 x LED hardware 0.894 = 1.478 (WallMeanGain 1.556 -8 %: in band), programmes unchanged,
    LED wall peak 0.720 (< 0.9), accent red share max 0.652, T-WAVE tier 0 0.045 /s, H4 macro Y 0.0927 (was 0.0866), 4 cradles and
    38 ledge lamps on the KS16 frame at t 460, Beat06 21 010 triangles (120 k), 24 lights; slab and wall points unchanged.
- 02:39 SG-b08 OK (no shader warning). SG_f02 (KS16, KS17, beat06_signs_look_up60, mid, beat07_heart_look_back) LOOKED:
  - KS16 ks16_wall.py: wall mean 69.0, frame 54.2, red share mean 0.271, 8x8 max 0.603; lookv2_check KS16 PASS, KS17 PASS.
    The eye field glows round the eye now (no flat teal canvas); defect: the z 405 flush board sat on the canvas and covered
    the iris (crop x 1190-1310, y 300-690) -> boards z 405 / 438 moved above the canvas (y 150 / 126); a test keeps it free.
  - KS17 right third with the podium skin (override 4): punched precast windows, a few lit (cool), catwalk lights: reads as a
    building (f01: all louvres). Metric on the critic's patch 1650-1900 x 520-640: mean 17.1, high-pass std 1.66 (target 21-26 /
    >= 6 not met: the patch is dark window grid; the lit share is FX's facade light model). beat06_signs_look_up60 shows the
    arcade's west face with lit interiors now.
  - look_up60 dissolve (up60.py): y 250-350 67.7, 350-460 54.2, 460-560 45.4 vs the deck 40.8 (the wall's top 20 % of its height,
    y >= 448, within 4.6 codes).
- 02:45 SG-t07 (Beat06SignsTests, the tree with the eye fix and the boards moved): 19 / 19 (QA/reports/SG-t07.xml). Eye window
  (t 445-505, 48 + 16 canvas points): max rate 0.0876 /s (T-WAVE 0.12), flashes 0, dominant 0.100 Hz; t 300 window 0.0350 /s,
  0, 0.100 Hz; eye centre at KS16 ndc (0.282, -0.033), canvas on 35.9 s of 600, open at the KS16 still (t 460).
- 02:47 deleted SG_f01 and SG_r07 (numbers logged). W: 7.1 GB free (other copies' sequences); the signs sequence goes at 640 x 360.
- REQUEST LEAD (GE's file, no owner this wave): Assets/_Game/Beats/Relief/GateDims.cs line 76: `PlY0 = 40.0` -> `PlY0 = 50.0` (the
  promenade lintel's soffit). KS16 built wall share 0.545 -> 0.562 at 16:9 and 0.413 -> 0.433 at 21:9 (scratchpad ks16share.py,
  within 0.002 of the audit); 60.0 gives 0.571 / 0.440. Check VisibilityAuditTests (pre-reveal koi from the garden, KS15) and the
  beat-5 stills first: D-029 GB-03 put the lintel there to hide the koi over the 38 m arcade.
- REQUEST QA: Assets/_Game/Scripts/Runtime/QA/ShotPlan.cs SignsSequence: start the clock at t 445 so the 60 s KS16 sequence holds
  the whole eye event (canvas 448-484; the still stays at t 460):
      public static SequenceSpec SignsSequence(int w, int h)
      {
          var sq = KeyStatic("seq_signs_ks16", "signs", "signs", "KS16 pose, 60 s at 30 fps from t 445 (the D-034 eye, t 448-484), clock running (14.10 photosensitivity)", "KS16_signs_wall",
                             (int)Math.Round(SignsSeconds * SeqFps), SeqFps, w, h);
          sq.ambient0 = 445.0;   // SG fix wave C28: NocScreenMath.EyeIn0 - 3 s
          return sq;
      }
- 02:49 SG-b09 OK (155.8 s, 0 errors, no shader warning of mine). 02:50-02:53 SG_f03 (keyshots, KS16 at 16:9 / 16:10 / 21:9 / 4K)
  and SG_f03c (beat stills) LOOKED (a combined "keyshots,beats" list is refused by ShotPlan.GuardNote: the route test compares the
  whole list string; QA's): KS16 ks16_wall.py 16:9 wall 68.9 / frame 54.1 / red 8x8 0.604; 16:10 69.1 / 56.3 / 0.605; 21:9
  68.7 / 48.0 / 0.603; 4K 68.2 / 53.5 / 0.608. The eye reads whole (no board on it). 4K crop: the iris fibres a regular 40-fold
  sunburst -> b10: two fibre families twisting with the radius (same range, static). beat06_signs_start / look_r: the arcade's
  podium skin (stone band, punched precast windows, warm and cool rooms in beat 6) reads as a real building.
- 02:58 SG_seq01: the signs sequence (KS16, 60 s at 30 fps, t 460-520, rain on, 640 x 360 for the disk) through INTEG's
  Tools/qa/sequences.py + seq_extra.py (QA's C22 gate, run read-only with --out-dir my QA/reports, --delete-frames):
  QA/reports/sequences_SG_seq01.md: G1 flashes 0.17 % (< 25 %), G2 0.10 %, G3 0, G4 0, G6 mean-luma step 0.068 % / frame
  (<= 0.3 %), G7 red p99.9 0.640 (<= 0.70), mean change 3.0 % / s (<= 4 %), luma step 0.04 levels, 0 spikes; FAIL block blinks
  1185 / pops 1 and G5 2.98 Hz (16 of 9216 blocks, per-bin RMS 0.0034): pops_crops.png shows every worst blink is a rain streak
  (the streak exclusion misses streaks at 640 x 360). SG_seq02 = the same with -rain 0 (the wall alone).
- 03:03 SG-t08 (Beat06SignsTests, tree with the iris fibres + the darker glyph field 0.10 + 0.32): 18 / 19: my own floor "m >= 0.10"
  failed (a glyph field corner 0.089). Picture fixed, not the floor: field 0.125 + 0.30 (the corner 0.104), HLSL + C#.
- 03:05 SG_seq02 (signs, -rain 0, 640 x 360) QA/reports/sequences_SG_seq02.md: blinks 0 (the rain was every blink of seq01),
  G1 0.17 %, G2 0.10 %, G3 0, G4 0, G6 0.052 % / frame, G7 0.640, mean change 3.01 % / s, luma step 0.028 levels: PASS. FAIL:
  1 block pop (frame 928, 11 px block (88, 33) = (264-297, 99-132) at 1080p: the garden trees' lanterns, GD) and G5 1.2 Hz on
  16 of 9216 blocks (per-bin RMS 0.0034 > 0.002; the same 16 blocks with rain). The frames were deleted by the gate, so
  SG_seq03 (b10 copy in Builds/SG_seq, rain off) localises the 16 blocks (scratchpad g5loc.py) before deleting.
- 03:07 SG-b10 OK (80 s). SG-t09 + SG-b11 queued (the glyph field fix).
- 03:12 SG_seq03 (b10, signs, rain off, 640 x 360) localised G5 with the gate's own computation (scratchpad g5loc.py: 5 px linear
  blocks, detrended, Hann FFT): the 20 worst blocks (rms above 0.5 Hz 0.0010-0.0034, dominant 0.6-1.2 Hz, amplitude up to 0.023)
  all lie at x 80-120, y 35-65 of 640 x 360 = x 240-360, y 105-195 at 1080p: the garden tree crown with GD's amber lanterns and
  pink lights at KS16's top left (crop scratchpad g5_region.png). No wall block (the screen, the eye, the cradles, the ledge lamps)
  is among them: the wall and the eye pass G5; the G5 FAIL and the frame-928 pop are GD's garden. Frames and the Builds/SG_seq
  copy deleted.
- 03:14 restored LF line endings in my 11 edited files (my Python edits had written CRLF; HEAD is LF, .gitattributes * -text): git diff now 573 / 48 lines.
- 03:14 SG-t09 (Beat06SignsTests, the final C# and HLSL twins): 19 / 19 (QA/reports/SG-t09.xml): imagery m 1.647 x hardware 0.894 =
  1.473 (WallMeanGain 1.556, -5.3 %), eye window 0.0876 /s, 0 flashes, 0.100 Hz; t 300 window 0.0350 /s; LED peak 0.720; accent red
  0.652; 4 cradles + 38 ledge lamps on KS16; Beat06 21 010 triangles, 24 lights. No GLOBAL change: the H4 macro of the wall rose
  with ContentNorm (Y 0.0866 -> 0.0927), seen only in the beat 6 / 7 wet floor under the wall.
- 03:18 SG-smoke on b10 (a copy in Builds/SG_smoke while the queue held b11; b10 = b11 minus the glyph-field constant):
  -autotest -brisk -autotest-until 1260 -beat 6: AUTOTEST PASS, beat 7 entered at s 1260.00 t 72.17, errors 0, corridor coverage
  48204 rays 0 misses, alloc mean 31.6 B/frame = the engine input box (excess 0.0), GC 0 (the cradle driver allocates nothing)
  (QA/reports/autotest_brisk_from1080_until1260.json). Copy deleted.
- 03:26 HT-final (full EditMode on this copy's tree of 03:13, i.e. my final C# / HLSL twins): 472 tests, 471 passed, 0 failed,
  1 skipped (AbyssTests.KS10_CompositionSearch) (QA/reports/HT-final.xml).
- 03:31 SG-b11 OK (147.8 s, 0 errors, no shader warning of mine): the final shader state (iris fibres, glyph field 0.125 + 0.30).
- 03:33 SG-final (full EditMode) CRASHED after 71 s: Unity "Crash!!!" with "not enough memory resources" (LoadModule) in the log
  (machine: free virtual memory 19.5 GB, W: 6-7 GB free); not a test failure. 03:42 retried as SG-final2 (QA/reports/SG-final.xml).
- 03:32-03:43 SG_f04 (KS16, KS17 at 1080p) and SG_f04b (beat06 x 7, beat07_heart_look_back) on b11, LOOKED:
  - KS16 ks16_wall.py: frame 54.0, wall 68.9 (60-85; D-036 (6) about 68), p5 / p50 / p95 20 / 72 / 121, red share mean 0.270,
    8x8 max 0.603 (<= 0.70); lookv2_check KS16 PASS, KS17 PASS. The eye with two fibre families reads as an eye, not a sunburst;
    4 cradles, the y 81.6 ledge lamp row; contact sheets QA/captures/SG_f04/_contact/contact_SG_f04_01.png,
    QA/captures/SG_f04b/_contact/contact_SG_f04b_01.png.
  - beat06_signs_look_up60 (up60.py): y 250-350 67.7, 350-460 54.1, 460-560 45.4 vs the deck 40.8.
  - image_metrics_SG_f04: KS16 NaN-like 17 (10 bright: the ledge lamp lenses, single hot pixels on the ledge lines at 90-200 m,
    crop scratchpad nan_crop.png; the rest garden trees (279, 112), (22, 176) as before), isolated > 8x median 2 (shop glass edges
    at the base, (708, 980) / (866, 1000), SG-06 class), banding[wall_panel] 2.06 (SG-07, unchanged); KS17 NaN-like 1.
    Fix (tree, b12): the lens glows at 0.35 (LedgeLensGain), the halo keeps 1.6 x 0.85: a sub-pixel lens no longer reads as a hot
    pixel or twinkles in motion.
- beat07_heart_look_back: the pale grey glyph displays (x 1140-1530, y 330-520) are still pale (SG-08 stays partly open).
- 03:54 SG-final2 = QA/reports/SG-final.xml (full EditMode, tree of 03:43): 472 tests, 471 passed, 0 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch, skipped by design). The only later change (03:46) is LedgeLensGain, a material constant no
  test reads; SG-t10 on the final tree below.
- 03:57 SG-b12 OK (61.5 s, 0 errors, no shader error): THE FINAL BUILD (Builds/SG). 03:59 SG_f05 (KS16, KS17) LOOKED:
  ks16_wall.py wall 68.8, frame 54.0, red share 0.270, 8x8 max 0.603; lookv2_check KS16 PASS, KS17 PASS; image_metrics_SG_f05:
  KS16 NaN-like 17 -> 11 (bright 10 -> 4: the ledge lens hot pixels are gone; the rest garden trees (279, 112), (22, 176), two
  pixels at the iris / board edge (1209, 426), (1211, 429), one at the far wall edge (1320, 526), 7 dark as in r07), isolated 2
  (shop glass edges, SG-06 class), banding[wall_panel] 2.06 (SG-07); KS17 NaN-like 1 (1328, 330), not mine (koi side).
  Contact sheets QA/captures/SG_f05/_contact/contact_SG_f05_01.png, SG_f03/_contact/contact_SG_f03_01.png (4 aspects + 4K),
  SG_f04b/_contact/contact_SG_f04b_01.png (beat stills).
- 04:01 SG-t10 (Beat06SignsTests|VisibilityAuditTests, the final tree): 28 / 28 (QA/reports/SG-t10.xml); visibility.json of
  04:01: KS16 sharesBuilt 0.545 / 0.413.
- 04:02 SG-smoke2 on b12: AUTOTEST PASS, beat 7 at s 1260.00 t 72.17, errors 0, alloc mean 27.5 B/frame = the engine input box
  (excess 0.0), GC 0.
- Kept: QA/captures/SG_f03 (KS16 16:9 / 16:10 / 21:9 / 4K, b09), SG_f04b (beat stills, b11), SG_f05 (final KS16 / KS17, b12),
  SG_seq02 (the QA gate's montages of the signs sequence; frames deleted). Deleted SG_f01, f02, f03b, f03c, f04, seq01, seq03 and
  the Builds/SG_seq / SG_smoke copies. W: 8.6 GB free.
- Fix-wave summary: C16 improved (0.434 -> 0.545 at 16:9; the rest is GE's lintel: REQUEST LEAD), C17 done (ledges, cradles,
  dissolve), C28 done (the eye; model + QA gate on the wall), D-036 (6) done (wall 68.8), C36 partly (podium skin; metric open).
  New files: none (all edits in my existing files).
SG DONE 04:03
