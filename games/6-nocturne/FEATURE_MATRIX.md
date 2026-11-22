# FEATURE MATRIX — Nocturne

Honest beat-by-beat status of the release build. Tiers per MASTER_PROMPT 14.5: Full, Simplified, Graybox. "Look v2" is the
user's directive D-027: dark neutral palette, no violet, real CC0 textures on every building, window and surface. "D-034" is
the horror and megalophobia layer of docs/DREAD.md.

**Last update:** 2026-11-21 23:35, after the Ark wave (D-043) and its final gate. Build: `Builds/Nocturne/Nocturne.exe`,
built from the Ark merge 948c767 with 0 errors. The Ark wave replaced the 1 800 m hull with the Ark (5 000 m x 438 m)
and moved the abyss traffic at least 218 m from the walk; the rows below say where.

**Sources.** Every number comes from the lead's verified-facts file `Logs/final_facts.md`. "not measured" means that file holds
no verified number for the item. Critic ids: C01-C53 are the six-lens panel of run p4_01 (`Logs/fix/_summary.md`); QA-Fxx are
the fix-wave QA findings (`Logs/fix_reports/QA.json`). Content marked with a decision number (D-0xx) is recorded as accepted
in DEVELOPMENT.md; the final gate did not check those items one by one.

## Read this first
- Every beat is at the Full tier. No fallback cut (Part 13 ladder, 14.8 cut order) is recorded in DEVELOPMENT.md. Full does
  not mean defect-free.
- One six-lens critic panel and a chief critic ran, on run p4_01 (master caddb90): 0 S1, 29 S2, 24 S3 (C01-C53). Its wow
  audit failed: KS04, KS05, KS06, KS10 and KS17 failed; KS09, KS18 and KS19 passed.
- One fix wave of twelve roles and one three-agent integration round followed. No second panel and no second wow audit ran
  (the user's run shape has one). The fixes below are checked by gates, not by critics.
- Part 15.1 item 2 (two clean critic rounds and a passing wow audit) is therefore not met. Item 3 is not fully met either: on
  the final key shots, lookv2_check passes 52 of 80 shots and image_metrics passes 706 of 780 checks.
- The known issues in this file are accepted for this release (D-042 (3)).

## Beat matrix

| Beat | Tier now | Done | Missing or weak (critique id) | Phase 4 owner |
|---|---|---|---|---|
| 1 Alley (s 0-120) | Full | Look v2 walls and surfaces with CC0 textures. Baked light pools, lit window interiors without realtime lights, and a glow card at the alley mouth for the title frame (D-039). Empty lit rooms beside the path (D-034). KS01 mean-luma window set to 20-36 (D-041). | No known issue of the final facts is specific to this beat. No verified final status: a lit transom near the mouth crawls (C14); the KS01 empty room is flat and over-lit (C37; window re-set by D-041); the steam plumes are invisible (C38); KS02 slit mean low (C44); KS02 readable time short (C45); plaster macroblocks after x264 CRF 23 at 1080p (C46; enc_beat01_wall fails on BandScore only, 0 visible steps, run p4_01). | AL |
| 2 Boulevard (s 120-400), money shot 1 | Full | Look v2 facades, blade signs and traffic lanes. KS04 pose at pitch 27 (D-040). Street furniture and arcade signs near the walker (fix wave, C15). Wet floors fixed (fix wave, C04). Portal rings fixed (fix wave, C03). D-034: the empty tram waits with open doors and chimes at s 226.25, and a lift lobby (C29); a darkness wave down a tower at s 300 (C25); a long look-up at KS05; sheet-lightning glows at s 205 and s 345. | S2 texpans shimmer above the -capture-ss 2 reference at KS04 (blade edge strips; the stabilised residual passes at 1.85) and at KS05 (QA-F06). S2 wet-floor reflection streaks under the shop row switch on in one frame at s 132 (QA-F02). 4K H1: KS04 Cr 2.31 on the fix-wave build, not re-measured at 4K after the integration round (QA-F10). Wow audit (p4_01): KS04, KS05 and KS06 FAIL; not re-audited. No verified final status: no stone texture on the podium band (C39). | BV, FX |
| 3 The Edge (s 400-560), money shot 2 | Full | Pier and balustrade glass over the rift; Gulf faces with lit galleries. Abyss depth pools and a sink veil (fix wave, C11). Frameless glass edge at KS10 with a drop of 43.8 % (fix wave, C05) and horizontal glass fixed (fix wave, C06). Balustrade step lights fixed (fix wave, C13). D-043: the Ark's lights appear far up the rift (hidden until s 380; 2.9 km out at s 400, 0.9 km at KS09) and the dark mass reads behind them; the gaze turns to the coming bow at s 492-500; the s 553 lightning outlines it. D-034: a 12 x 8 m panel tears off face B at s 470 and falls 662 m in 20 s with no impact sound; 32 lift cabins on 8 shafts and 2 gantries on the Gulf faces; a long look-up at KS09; vertigo at KS10 (vFOV 68 -> 74 with a 0.35 m lean); a sheet-lightning glow at s 553. | S2 at s 552 the opaque ribs of the pier balustrade glass sweep across the distant Gulf galleries, so some warm light strips vanish for one frame (NocGlass rib pitch 6 m at 2.5-3 m from the eye; QA-F14). S2 texpans shimmer at KS08 and KS09 (2.55) (QA-F06). KS10 4K H1 2.56 in R/G/B only: passes, because the gate counts Y/Cb/Cr (D-041; QA-F11). Wow audit (p4_01): KS10 FAIL, KS09 PASS; not re-audited. No verified final status: after KS10 the gaze walks into gate A's tunnel (C12); KS07 spends much of its frame on a dead plaza (C43). | AB, AT, DR |
| 4 Skybridge (s 560-880) | Full | Deck and the KS13 glass strip; trains on the tubes (D-039). D-043: a 9 s silence, then the Ark's bow passes over the walk at s 596 (the gaze looks up at s 576-586); the walker crosses the whole bridge under its belly (4 m-pitch lit decks, 59 hanging gondolas, 16 engine pods, hangar bays, six slow searchlight cones that never contain the walker; about 39 k triangles, no realtime light); a long look-up under the belly to near the zenith at s 748-822. The abyss traffic lies at least 218 m from the walk and below the walker; the longest vehicle streak at 60 fps over s 530-900 is 64 px (the "laser" fix). D-034: a 900 m shadow passes under the bridge in the abyss haze, seen through the KS13 glass strip (D-040: KS13 only); bridge sway of 2-3 cm; a sheet-lightning glow at s 790. The 1080p perf windows of this beat pass. | S2 KS11: face B below the deck stays bright (p95 88 against a target of 36; rest of C11). S2 KS13 p99 71-78 against 90 (C30). S3 4K perf under the Ark: 171 frames over 20 ms in s 580-600 (mean 17.88 ms, max 22.84 ms; DR-09); 1080p is the perf gate and 4K is INFO (D-042). The Ark sequences fail the block-pop / blink check (Ark final gate); the failing frames were not reviewed one by one. No verified final status: the KS12 train window band is a flat grid (C40); the upper tube's portal floats (C41); the shadow reads as cloud with star-like back lights (C42); the KS14 garden gate frame is a flat dark box (C31). | TR, AB (lamps), DR (shadow) |
| 5 Hanging garden (s 880-1 080) | Full | Trees with crowns; lanterns and fireflies in the crowns, and light under them (D-039). Beat-5 dark-cast window ruled (D-040). D-043: the Ark's stern leaves the walk at s 1 055.7 and slides north into the haze, glimpsed through the crown gaps. No sheet-lightning glow falls in this beat. | S3 single bright pixels: far lanterns through 1 px leaf gaps in KS14 / KS15 (QA-F08, C32). No verified final status: the canopy hue (C33; the D-040 cast window applies if the hue fix fell short; final value not measured). | GD |
| 6 Wall of signs (s 1 080-1 260) | Full | Wall of signs with the LED screen wall (dot structure and imagery programmes, D-036), promenade and arcade. D-034: one enormous eye opens on the wall (t 454-478; C28); a sheet-lightning glow at s 1 140. KS16 at 4K CRF 23: 0.00 H1 steps / 1 000. | S3 KS16 photosensitivity G5: 15 small blocks modulate above 0.5 Hz, caused by real abyss traffic moving through gaps in the garden crowns (no flash; G1-G4, G6, G7 pass; QA-F13). No verified final status: KS16 wall share (C16; D-041 (5) raised the promenade lintel for it; final share not measured); the wall reads as office glazing without scale markers (C17); the arcade block in KS17's right third reads as untextured (C36); the visibility audit does not report KS16's wall share (C48). | SG |
| 7 Heart Square (s 1 260-1 500), money shot 3 | Full | Plaza, spire, the Old Quarter tenement with empty lit rooms, and the warm goal window. The 900 m hologram koi with air and translucency (fix wave, C02): tower lights now show through it and the air around it glows. D-034: the koi's eye follows the camera (at most 6 deg/s; C27); a darkness wave at s 1 330 (C25); a long look-up at KS17; a sheet-lightning glow at s 1 420. The walk ends at the one warm window under the koi. | S2 texpans shimmer at KS17 (2.72) (QA-F06). S3 the koi still reads fairly solid at 1080p (body 150 against a 206 rim; rest of C02). 4K H1: KS17 3.29 sky / 4.18 plaza on the fix-wave build, not re-measured at 4K after the integration round (QA-F15). Wow audit (p4_01): KS17 FAIL, KS18 and KS19 PASS; not re-audited. No verified final status: the goal window aperture and the grey balcony underside (C18); teal plaza granite (C34); KS19 railing aliasing at 1080p (C35). | HT |
| Audio | Full | Procedural score (pad voices, chords keyed to s) and a soundscape per beat. D-034 stem E: echoes off the Gulf faces at 2 d / 343 s, sub-bass swells under KS04 / KS09 / KS10 / KS13 / KS17 with a laptop-band layer (C09), structural groans, silence drops of 11.0 / 14.8 dB before KS09 / KS17 (C07), the Ark's sound (D-043: horn calls, rising pressure, a 9 s silence before the bow, a long Doppler, engine pods, groans), the panel's creak. Upload-loudness step in docs/AUDIO.md section 10 (C50). | Integrated loudness is -27.3 LUFS (final f08 render; docs/AUDIO.md section 10): a recording needs the docs/AUDIO.md section 10 step before upload. A human listen of the dread layer on a laptop and on headphones (AU-17) is still listed as open in docs/AUDIO.md section 10. C08, C49, C51 and C53 are checked only by the audio gate list (docs/AUDIO.md section 11); no critic listened again. | AU |
| D-034 dread layer (all beats) | Full (every item built) | Every item of docs/DREAD.md exists, with the hull replaced by the Ark (D-043): Ark, abyss shadow, falling panel, lift cabins and gantries, darkness wave, koi eye, wall eye, six sheet-lightning glows, look-ups, vertigo, bridge sway, empty tram, lift lobby, empty lit rooms, and the sound of section 1. Per beat in the table below. | The camera layer, darkness wave, koi eye, wall eye, sheet lightning, tram dwell and lobby (C25-C29) were built after the only panel; no critic has seen them. The panel had no "dread" lens, which the docs/DREAD.md acceptance asks for. Per-item sequence and photosensitivity results for the D-034 light events: not measured (the final facts hold only the KS16 G5 result and the D-041 flash definition). | DR, FX, HT, SG, AT, BV, AU |
| Transitions, title, ending | Working | All nine boundary sequences PASS (run p4_01). The 1080p perf boundary windows PASS. Cinematic autotest: goal trigger 599.050 s, stop 601.450 s, ending 17.00 s, 7 beats, 0 errors. Walk autotest: trigger 998.100 s, stop 1 002.100 s, 0 errors. | The boundary sequences ran on run p4_01, before the fix wave, and the final gate did not re-run them. The panel found they test only whole-frame differences, with no montage (C21; the QA gates were reworked afterwards). Title card (14.11): not measured. | FX, QA |
| QA | Working, with the gaps listed | The final gate below. 20 final key shots at 1080p, 16:10, 21:9 and 4K (80 images) in `QA/final`, contact sheets in `QA/final/_contact`. QA gates reworked after the panel so they measure what they claim (C19-C23). | lookv2_check passes 52 / 80 final shots and image_metrics 706 of 780 checks; the failing items are not listed in the verified facts. S3 the H5 rain image gate is not valid yet: its component count is dominated by moving foliage; the difference method measures about 45 drops per frame (QA-F07, C24). Soak, sequences and the 1080p encode are from run p4_01 (master caddb90), not from the final build; the texpans sequence there used the old harness, and the panel found that encode gate vacuous (C19). | QA (fix wave), INT-B, lead |

## D-034 dread layer by beat (picture and camera)

| Beat | What happens |
|---|---|
| 1 Alley | Empty lit rooms beside the path. No sheet-lightning glow. |
| 2 Boulevard | The empty tram waits with open doors and chimes at s 226.25. A lift lobby (docs/DREAD.md 6 places it at the boulevard terminus). A darkness wave runs down one tower at s 300. A long look-up at KS05. Sheet-lightning glows at s 205 and s 345. |
| 3 The Edge | The Ark's lights come out of the haze far up the rift (2.9 km at s 400, 0.9 km at KS09); the gaze turns to the coming bow at s 492-500. The 12 x 8 m panel tears off face B at s 470 and falls 662 m in 20 s. 32 lift cabins on 8 shafts and 2 gantries on the Gulf faces. A long look-up at KS09. Vertigo at KS10: vFOV 68 -> 74 with a 0.35 m lean. A sheet-lightning glow at s 553. |
| 4 Skybridge | The Ark's bow passes over the walk at s 596 (look-up at s 576-586); the walker crosses the bridge under its belly; a long look-up under the belly at s 748-822; six slow searchlight cones (28-40 s sweeps). A 900 m shadow passes under the bridge in the abyss haze, seen through the KS13 glass strip. Bridge sway of 2-3 cm. A sheet-lightning glow at s 790. |
| 5 Hanging garden | The Ark's stern leaves the walk at s 1 055.7 and slides north into the haze. No sheet-lightning glow. |
| 6 Wall of signs | One enormous eye opens on the wall (t 454-478). A sheet-lightning glow at s 1 140. |
| 7 Heart Square | A darkness wave at s 1 330. The koi's eye follows the camera, at most 6 deg/s. A long look-up at KS17. Empty lit rooms in the Old Quarter. A sheet-lightning glow at s 1 420. |

## Sound by beat

The soundscape column is the design as written in docs/AUDIO.md section 1. The levels were verified for the whole walk, not per
cue: in the fix-wave offline renders every beat median is within +-1.5 dB of its 14.3 target at both speeds.

| Beat | Soundscape | D-034 cues (stem E) |
|---|---|---|
| 1 Alley | Rain and the city hum, then the pad. Rain on concrete and awnings, drips, AC units, steam vents, vending machines, a neon buzz, footsteps on wet concrete; the reverb opens toward the mouth. | No beat-specific cue in the verified facts. |
| 2 Boulevard | The canyon's breath, the traffic wash overhead with pass-bys, ballast buzz, the tram wire, the tram's bell, motor and rails; stone footsteps. | Sub-bass swell under KS04. Tram door chime at s 226.25. |
| 3 The Edge | The pad drops and the wind rises; the abyss's deep hum, an updraft whistle, band-passed wind. | Echoes off the Gulf faces (2 d / 343 s). Silence drop of 11.0 dB before KS09. Swells under KS09 and KS10. The Ark: distant horn calls from the bow and a rising pressure. The panel's creak; no impact is heard. |
| 4 Skybridge | Steel-deck footsteps, glass on the strip, rain on steel and glass, wind gusts locked to the rain slant, the tube drone and rail whine, the train passes with Doppler. | The Ark: a 9 s silence before the bow (rest -21 dB), then a long Doppler, the engine pods passing one by one, deep hull groans. Swell under KS13. Echoes off the Gulf faces. |
| 5 Hanging garden | Wind and traffic fall away; dense soft rain on the crowns, rustle, leaf drips, soft footsteps. The quietest and widest beat. | No beat-specific cue in the verified facts. |
| 6 Wall of signs | The hum layer at its loudest, the screens' hum, arcade drips; the koi tone enters faintly. | No beat-specific cue in the verified facts. |
| 7 Heart Square | The koi tone follows the koi's distance; the pad thins near the end; after the goal trigger everything releases to rain, hum and the goal lamp's buzz. | Silence drop of 14.8 dB before KS17. Swell under KS17. |

Structural groans recur from beat 2 on (docs/DREAD.md 1.3); their times are not measured in the verified facts.

## Verification by beat

| Beat | Key shots (14.10) | Wow audit (p4_01, before the fixes) | Verified numbers | Not measured |
|---|---|---|---|---|
| 1 Alley | KS00-KS03 | not a money shot | KS01 window 20-36 (D-041). enc_beat01_wall: fails on BandScore only, 0 visible steps (1080p CRF 23, run p4_01). Smoke (brisk to s 400) PASS. | final KS00-KS03 values; title card |
| 2 Boulevard | KS04, KS05, KS06 | KS04 FAIL, KS05 FAIL, KS06 FAIL | KS04 texpans: stabilised residual passes at 1.85. KS04 4K Cr 2.31 (fix-wave build). Smoke PASS. | KS05 texpans value; KS04 at 4K after the integration round |
| 3 The Edge | KS07-KS10 | KS09 PASS, KS10 FAIL | KS10 drop 43.8 %. KS10 4K H1 2.56 in R/G/B only (passes, D-041). KS09 texpans 2.55. Silence drop 11.0 dB before KS09. | KS08 texpans value |
| 4 Skybridge | KS11-KS14 | not a money shot | 1080p: s 580-600 max 7.27 ms, s 700-740 max 5.09 ms (PASS). 4K: s 580-600 mean 18.46 ms with one frame of 20.05 ms. KS11 face B p95 88 (target 36). KS13 p99 71-78 (target 90). | - |
| 5 Hanging garden | KS15 | not a money shot | - | count of single bright pixels in KS14 / KS15 |
| 6 Wall of signs | KS16 | not a money shot | KS16 4K H1 0.00 steps / 1 000. Photosensitivity: G5 fails on 15 blocks above 0.5 Hz; G1-G4, G6, G7 pass. | KS16 wall share |
| 7 Heart Square | KS17, KS18, KS19 | KS17 FAIL, KS18 PASS, KS19 PASS | KS17 texpans 2.72. KS17 4K 3.29 sky / 4.18 plaza (fix-wave build). Koi body 150 against a 206 rim (1080p). Silence drop 14.8 dB before KS17. Goal trigger 599.050 s, stop 601.450 s, ending 17.00 s. | KS17 at 4K after the integration round |

## Verification at the Ark final gate (2026-11-21 22:12-23:21, build of the Ark merge 948c767)

| Gate | Result |
|---|---|
| fastcheck (C#) | OK, runtime 221 / editor 297 files |
| Validate | SUCCESS, 0 problems |
| EditMode | 528 tests: 527 passed, 0 failed, 1 skipped (explicit search test) |
| PlayMode | not measured |
| Build | 0 errors (player folder 464.3 MB) |
| Smoke (brisk to s 400) | PASS |
| Autotest cinematic 2.5 m/s | PASS, goal trigger 599.050 s, stop 601.450 s (expected 599.05 / 601.45), ending 17.00 s, 7 beats, 0 errors |
| Autotest walk 1.5 m/s | PASS, trigger 998.100 s, stop 1 002.100 s (expected 998.10 / 1 002.10), 0 errors |
| Audio in the player | 0 audio overruns after the intro in all four player runs, callback max 5.9-8.2 ms |
| Perf 1080p (whole cinematic walk, vSync 0, alone) | 288.3 fps average, frame p99 3.68 ms, max 16.55 ms, 0 frames over 33 ms; GPU average 3.45 ms, median 3.51, p99 3.64 ms (budget 8.2); boundary windows PASS; the s 580-600 / 700-740 windows were not mapped for this run (SKIP) |
| Perf 4K (-perf-rt 3840x2160, alone) | 83.8 fps average, frame p99 20.47 ms, max 25.61 ms, 0 frames over 33 ms; GPU average 11.90 ms, median 11.42, p99 20.43 ms; s 580-600: 171 frames over 20 ms, mean 17.88 ms (INFO at 4K, D-042; DR-09) |
| Final key shots | `QA/final`: 20 key shots at 1080p, 16:10, 21:9 and 4K (80 images, contact sheets in `QA/final/_contact`); lookv2_check 52 / 80 shots pass; image_metrics 697 of 780 checks pass |
| The Ark | 13 stills along its pass (s 430-1 020) in `QA/final/ark`, looked at by the lead; four Ark sequences: 23 checks PASS, 8 FAIL, all of them the block-pop / block-blink check; G1-G4 and G7 (flash, red) pass |
| Traffic (AB, Ark wave) | every abyss layer at least 218 m from the walk and below the walker; boulevard vehicles hidden within 300 m of the camera over the rift; longest vehicle streak at 60 fps over s 530-900 is 64 px (limit 80) |

Earlier runs that the Ark final gate did not repeat:

| Gate | Run | Result |
|---|---|---|
| Wall bots 2.5 / 1.5 m/s | final gate before the Ark, b14abae | PASS, max excess -292.8 / -293.1 mm and -295.7 / -295.7 mm, 0 errors |
| Soak | p4_01, master caddb90 | 1 200 s PASS, 0 errors, managed growth -0.18 MB |
| Sequences | p4_01 | PASS: signs, rain, koi, traffic, texpans (old harness), dolly, all nine boundary sequences, statics 7 / 7 with -freeze-clock -rain 0, pans 7 / 7 |
| Encode x264 CRF 23, 1080p | p4_01 | 10 of 11 clips PASS (enc_beat01_wall fails on BandScore only, 0 visible steps) |
| Encode H1 at 4K | fix wave (QA) | KS16 0.00 steps / 1 000; KS10 2.56 in R/G/B only (passes: the gate counts Y/Cb/Cr, D-041); KS04 Cr 2.31 and KS17 3.29 sky / 4.18 plaza on the fix-wave build. The integration build no longer showed gated Y/Cb/Cr steps there (INT-B), but they were not re-measured at 4K after the integration round. |
| Audio offline renders | fix wave | 62 / 62 audio gates at both speeds; beat medians within +-1.5 dB of the 14.3 targets; true peak -7.6 dBTP; in-game -audio-dump within 0.14 dB of the offline render; integrated loudness -27.3 LUFS (final f08 render; docs/AUDIO.md section 10) |

## Known issues accepted for this release (D-042 (3))

| Sev | Issue | Beat | Id |
|---|---|---|---|
| S2 | texpans shimmer above the -capture-ss 2 reference at KS04 (blade edge strips; stabilised residual passes 1.85), KS05, KS08, KS09 (2.55), KS17 (2.72) | 2, 3, 7 | QA-F06 |
| S2 | At s 552 the opaque ribs of the pier balustrade glass sweep across the distant Gulf galleries; some warm light strips vanish for one frame (NocGlass rib pitch 6 m at 2.5-3 m from the eye) | 3 | QA-F14 |
| S2 | KS11: face B below the deck stays bright (p95 88 against a target of 36); KS13 p99 71-78 against 90 | 4 | C11 (rest), C30 |
| S2 | Wet-floor reflection streaks under the boulevard shop row switch on in one frame at s 132 | 2 | QA-F02 |
| S3 | KS16 photosensitivity G5: 15 small blocks modulate above 0.5 Hz, caused by real abyss traffic moving through gaps in the garden crowns (no flash; G1-G4, G6, G7 pass) | 6 | QA-F13 |
| S3 | Single bright pixels in some key shots (far lanterns through 1 px leaf gaps in KS14 / KS15) | 4, 5 | QA-F08, C32 |
| S3 | The koi still reads fairly solid at 1080p (body 150 against a 206 rim), though tower lights now show through and the air around it glows | 7 | C02 (rest) |
| S3 | 4K perf under the Ark: 171 frames over 20 ms in s 580-600 (mean 17.88 ms, max 22.84 ms). Record in real time at 2560x1440, or at 4K only with a frame-locked capture | 4 | D-042 (1), (2), DR-09 |
| gate | The four Ark sequences fail the block-pop / block-blink check (52-54 pops, 245-599 blinks of 32 / 64 px blocks); the failing frames were not reviewed one by one | 3, 4 | Ark final gate |
| S3 | The H5 rain image gate is not valid yet (its component count is dominated by moving foliage); the difference method measures about 45 drops per frame | all | QA-F07, C24 |
| note | Twice the capture of the whole "encode" / "boundaries" sets filled disk W: (13-17 GB of frames). At 03:33 the Windows commit limit (64 GB RAM + a small pagefile) stopped several runs while twelve builders ran. | - | machine |

## Panel items C01-C53: where each one stands

The mapping from the fix list of the final facts to the panel ids is by topic. "Fixed" means a fix landed and the gates pass;
no critic has looked again.

| Status | Ids |
|---|---|
| Fixed or built in the fix wave or the integration round | C01 hull course; C03 portal rings; C04 wet floors (rest: QA-F02); C05 KS10 frame (drop 43.8 %); C06 horizontal glass; C07 silence drops (11.0 / 14.8 dB); C09 laptop-band layer; C13 step lights; C15 street furniture and arcade signs; C19-C23 QA gates that measure what they claim; C25 camera layer and darkness wave; C26 sheet lightning; C27 koi eye; C28 wall eye; C29 tram dwell and lobby; C50 upload-loudness step; C52 audio callbacks measured in the player |
| Partly fixed; the rest is open and accepted | C02 koi (reads fairly solid at 1080p); C11 abyss depth fixed, KS11 face B still bright; C24 perf run done in the final gate, the H5 rain image gate not valid, the disk guard has no verified status |
| Settled by a ruling | C10 KS04 pitch 27 and the KS05 look-up (D-040); C33 beat-5 cast window (D-040; final value not measured); C37 KS01 window 20-36 (D-041; final value not measured); C42 shadow required through KS13 only (D-040; the "reads as cloud" part has no verified status) |
| Open, accepted for this release | C30 KS13 p99; C32 single bright pixels |
| Checked only by the audio gates (62 / 62 pass; gate list in docs/AUDIO.md section 11) | C08 dread cues buried by train meets; C49 city hum spectrum; C51 beat-5 pad stereo; C53 tram chime masked |
| No verified final status (not in the final facts; no second panel) | C12, C14, C16, C17, C18, C31, C34, C35, C36, C38, C39, C40, C41, C43, C44, C45, C46, C47, C48 |

## Decisions that changed the spec (DEVELOPMENT.md)
D-027 Look v2 (dark neutral palette, CC0 textures); D-029 key-shot pose amendments; D-032 lead arbitration (KS10 drop by
composition, koi spine, wet-floor gain, p99 rule, encode test, floor-line gate, 33 ms gate); D-034 the horror and megalophobia
layer; D-035 the fifth start's run shape; D-036..D-039 builder decisions; D-040 the panel rulings (hull over the walker, KS04
pitch 27, shadow via KS13 only, beat-5 cast window); D-041 the integration rulings (WCAG flash definition, H1 counts Y/Cb/Cr,
KS01 window 20-36); D-042 the final rulings (perf gate at 1080p, 1440p real-time recording, known issues accepted for this
release); D-043 the Ark (5 000 m, down the rift from the north, over the walk at s 596) and the traffic moved at least
218 m from the walk.

## How this status was reached (Part 15)
- Phases 0-3 (preflight, research, framework import, graybox) on 2026-11-13 / 14. Look v2 built 2026-11-14 / 16 (accepted
  D-031). Phase 4 wave A started 2026-11-16 and was paused four times.
- Fifth start, 2026-11-20 20:12 to 2026-11-21 07:10: eleven builder roles (AB, AT, BV, FX, HT, SG + AL, TR, GD, AU, DR) in
  parallel in five git worktrees; merged into an integration copy; full gate; one six-lens panel + chief; one fix wave of
  twelve roles (+ a lead-QA builder); one three-agent integration round; final gate.
- The Ark wave, 2026-11-21 20:10 to 23:35 (the user's order, D-043): five roles (DR, AB, AT, FX, AU) in two worktrees,
  merged, then the Ark final gate above.
