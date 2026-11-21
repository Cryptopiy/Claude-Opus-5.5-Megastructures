# CLAUDE_FABLE_5.2_FINAL_REPORT - Bulwark (game 4)

Final version, written on 2026-11-20 after the fix wave, the lead integration round and the final checks on master. It
replaces the draft of 17:15. The run finished under the user's speed order of 2026-11-20 with one six-lens critic panel,
one fix wave and one integration round. A second full critic round was **not** run (section 5.4).

Every number in this report comes from a project file, and the file is named next to it or in section 9. A few values
are computed from measured numbers and the constants in code; they are marked "computed".

---

## 0. Status at a glance

| Item | Final state | Source |
|---|---|---|
| Product | Bulwark. Unity 6000.6.0f1, URP 17.6, Direct3D 11. Player `Builds/Bulwark/Bulwark.exe` (231.1 MB) | autotest `env`; `Logs/lead_final.out` |
| Test machine | NVIDIA GeForce RTX 5070 (11944 MB), AMD Ryzen 7 5700X3D | autotest `env` |
| Final lead check | RebuildAll OK (0 problems), Validate 78 checks / 0 problems, EditMode 382/382, build OK (0 errors), smoke at 25 m/s PASS | `Logs/lead_final.out`, `QA/tests/editmode_final.xml` |
| Cinematic autowalk (2.5 m/s) | PASS: goal at 759.50 s (12:39.5), ending done at 815.00 s (13:35), 7 beats, roll 0, 0 errors | `QA/reports/autotest_cinematic.json`, `Logs/fin-auto25.out` |
| Walking (1.5 m/s) | PASS: goal at 1170.17 s, done at 1225.67 s, 0 errors | `QA/reports/autotest_walk.json` |
| Soak | 1800 s at 2.5 m/s through the whole ending PASS; 900 s at 1.5 m/s PASS | `.worktrees/integ/QA/reports/leadqa/soak/soak.json` (copy in `QA/reports/leadqa/soak/`), `QA/reports/soak.json` |
| Performance, measured alone | 4K uncapped 134-168 fps at the three heaviest poses, 0 frames over 33 ms; 1080p average 445 fps | `QA/perf/final/` |
| Critic panel and fixes | Panel before the fix wave: 0 S1, 24 S2, 36 S3/S4. One fix wave (9 builders) and the lead integration round fixed the main findings (section 4.10). No second panel | `Logs/panel_x2.json`, `Logs/fix_x2_reports.json` |
| Final 4K key shots | 32 stills in `QA/final/` | folder listing; DEVELOPMENT.md row "Final lead / Final 4K key shots" |
| Definition of Done (MASTER_PROMPT 15.1) | **Not fully met.** Item 2 (two consecutive clean critic rounds) was not run. Item 3: image metrics still flag 38 of 198 final stills and 11 of 48 ending stills, and the encode test was not run on the final build (section 5) | this report |

---

## 1. What was built

### 1.1 The game

Bulwark is a first-person, linear walk along the crest of a dam 6 km high that holds an ocean above a lowland world. The
sea is on the right (east) while walking and the lowland is on the left (west); the road is at y 0 and the sea at y -8.
The weather clears as you walk: storm, sun break, cold noon, tungsten light inside the dam, cyan-black window light, then
amber dusk (MASTER_PROMPT 14.0). There is no combat, no inventory and no dialogue. The product is meant to be recorded
for YouTube.

The user's redesign order of 2026-11-19 (section 2) added two things to the original specification: a colossal kraken
on the sea side, and a new ending in which you descend to the foot of the dam and look back at all of it. Both follow
`docs/ADDENDUM_A1.md`, whose section 00 (lead decisions of 2026-11-20) wins over the rest of the addendum and over
MASTER_PROMPT Part 14 where they conflict.

Every event is keyed to progress `s` along the path, not to clock time, so the game plays the same at any walking speed.
The walked path is s 0-1500 m (unchanged from the specification). From s 1500 the ending runs as a second part of the
same coordinate (`EndingPath` in `BwPath.cs`): the ride to s 1857.5, then a 38.5 m walk on the lantern platform to the
goal, the Keeper's Lantern, at s 1896.0, and the rest point at s 1897.5.

Beat timing on the final build. Times are on the autotest's fixed 60 fps clock and count from the start of the walk.

| # | Beat | s range (m) | Starts at 2.5 m/s (s) | Starts at 1.5 m/s (s) | Content (short) |
|---|---|---|---|---|---|
| 1 | Wave gallery | 0-120 | 0.00 | 0.00 | Service gallery inside the crest: ribs, slits with storm glow, caged sodium lamps that sway with the wave impacts, a timed door onto the crest |
| 2 | Crest | 120-420 | 48.25 | 80.25 | The crest road with the sea a few metres below; money shot KS03/KS04 (the door opens on the sea); kraken K1 and K2 |
| 3 | Spillway bridge | 420-700 | 168.25 | 280.25 | Bridge over the spillway, the fall, mist, sun break and rainbow (KS08); kraken K3 at the gates |
| 4 | Armour slope | 700-920 | 280.25 | 466.92 | 40 m armour units ("Colossopod"), six islets, tide gauges, the gate tower exterior; kraken K4 and K5 |
| 5 | Gate tower and descent | 920-1070 | 368.25 | 613.58 | Atrium with pendant lamps; 270 steps in 15 flights; three portholes |
| 6 | Window gallery | 1070-1370 | 428.25 | 713.58 | A 28 m high hall behind ten 20 x 36 m panes; the kraken encounter (replaces the whale of the original specification) |
| 7 | Sluice tunnel and lee ledge | 1370-1500 | 548.25 | 913.58 | Tunnel to a ledge on the lee face at dusk; the glass cabin at the end of the ledge |
| - | Ride: elevator and shuttle | 1500-1857.5 | 600.85 to 743.85 | 1001.25 to 1144.25 | 143.00 s in both runs (measured) |
| - | Lantern platform | 1857.5-1896.0 | goal at 759.50 | goal at 1170.17 | A 38.5 m walk to the Keeper's Lantern; the final look back at the dam |
| - | End sequence | after the goal | done at 815.00 | done at 1225.67 | 55.5 s: look back, fade, end line |

Sources: beat names and s ranges from MASTER_PROMPT 14.5 and 14.12; start, ride, goal and done times from the two final
autotest reports; the s values of the ending from the ending builder's fix-wave report (`Logs/fix_x2_reports.json`) and
the autotest checks `triggerAtGoal` and `restAtLantern`; beat content from the Wave X/X2 builder reports
(`Logs/wbx_reports.json`, `Logs/wbx2_reports.json`).

### 1.2 The kraken, built from the user's Tripo3D models

**Source models.** The user supplied two Tripo3D GLB files: `3D Models/SM_Creature_Octopus.glb` (58.7 MB) and
`3D Models/SM_Creature_MonsterTentacles.glb` (59.2 MB). They are a user-directed exception to the rule "everything
procedural, no imported assets". The exception is logged in `DEVELOPMENT.md` (row "2026-11-19 Wave A1 (user order)").
The files stay outside `Assets/` as read-only sources; Unity never imports them and nobody edits them. Everything else
stays procedural: the shaders are handwritten, the colours come from `BulwarkPalette` Kraken* tokens, and the sounds are
synthesised in code.

**Pipeline.** `Tools/kraken/build_kraken.py` regenerates every game asset from the two GLBs deterministically: cleanup,
four body LODs (330k / 80k / 20k / 4k triangles), the material maps, relief data, a manifest, and the tentacle profile
used for the procedural tentacles. The output goes to `Assets/_Game/Creature/Kraken/Resources/Kraken/`.

**Runtime.** `KrakenRig` skins the very long tentacles on the GPU along splines; they are lofted from the model's own
tentacle, so they can be any length and keep moving. `KrakenCreature` assembles the creature, and the handwritten
shaders `BwCreature`, `BwCreatureEye` and `BwCreatureVeil` draw it. The main steps after Wave X:

- **Wave X2 (kraken builder):** fixed a normal-map unpack bug (both Tripo normal maps are BC5; the blue channel was read
  as z, and every baked slope was tilted about 16x, so the hide read as rippled glass); made the hide read as wet,
  barnacled, rock-like skin; added the dam clip (`_BwDamClip`), so no part of the creature shows through the far-field
  lee face.
- **Fix wave (kraken builder):** the eye got a lens-shaped slit pupil that leaves iris at both ends, a dark limbal ring,
  a corneal dome with catchlights from the gallery lamps A and B, and soft rolled lids, which removed the "glowing lips"
  look; the eye glow in air dropped from 5.5 to 2.0. New `KrakenCascade` draws water falling off emerged parts (64
  emitters, 192 strands). Lee-side cameras more than 3 km away see a dark hide, so the stinger reads as a silhouette.
- **Fix wave (gallery builder):** the beat-6 encounter implements `IKrakenEncounter` again, which restores KK4 (the
  sucker arm on the glass, lit by lamp A); the main eyeballs are scaled 1.63x in their sockets for the approach and the
  hold.
- **Lead integration:** the stinger was posed but not drawn in the real end path, because `Beat08Ending.ApplySets` sets
  `forceRenderingOff` on every renderer outside its own roots; `KrakenCreature.Sync` now clears it on the creature's own
  renderers (DEVELOPMENT.md row "Stinger posed but not drawn").

**Choreography.** `KrakenDirector`, by the show builder, converts s into an "effective progress". This value is
latched, and a 1.5 m/s time branch lets it finish only the segment the player is in. The creature pose is therefore
identical at 1.5 and 2.5 m/s and reproducible in captures. The director publishes two shared contracts:

- `SeaDisturbances` (Breach, Shadow, Wake, Slap, TentacleRise), which the sea, mist and audio read;
- `KrakenCalls`, an ordered table of cues. Since the fix wave each cue also has an end s, and its sound lasts until the
  visible part is gone at either speed (test `Calls_SoundCoversEveryVisibleSpan`).

**Motion limit.** Lead decision 2 and the user's order of 2026-11-20 say that every visible motion must be slow and heavy,
at 8 m/s or less at 2.5 m/s, with no exception for the K5 slap, which became a slow topple. The show builder's fix-wave
report measures the visible motion peak at 7.46 m/s at 2.5 m/s and 4.64 m/s at 1.5 m/s. The stinger peaks at 27.3 m/s
against its own limit of 28 m/s (ruling R3: 8 m/s in the animal's proportions, 8 x 3.5).

**Size.** The model numbers replace the A1 table (DEVELOPMENT.md, "Model numbers"): the head top is 168.8 m above the
crown, and the main eyes are 66 m above it. At K4 the head top rises to +179 m above the sea.

**Appearances (s windows as reported built):**

| ID | s window | What the viewer sees | Final status |
|---|---|---|---|
| K1 | 120-236 | "The islet that moves": a dome rising to +70 m, about 1.16 km out | Built |
| K2 | 320-450 | One tentacle rising 185 m out of the sea, leaning away, its top hooked, with red suckers | Built |
| K3 | 452-524 | A dark mass and a wake surge west towards the spillway gates; one arm tip curls 14 m out of the sea in the pier slot (s 483-503) | Built and made visible in the fix wave: the slot is 10.7 levels darker than the no-kraken twin (target 6 or more). The V-wake stays faint (show report) |
| K4 | 712-910 | The head rises to +179 m in the cove, the eye opens (s 792-800) and tracks the camera, and an arm arches 80 m over an islet | Built. Fix wave: dimmer eye at noon, wetter head, darker arms near the body; the arch flanks are still saturated red (kraken report) |
| K5 | 778-822 | A whip at the belt edge topples onto the sea (Slap at s 822) and throws a spray column | Built as a slow topple; the spray column measures 462 px at 4K in KS11 (A1 asks 445 +- 40; mist report) |
| K4a | - | An arm under the swell | Not built (first in the cut order) |
| K6 | - | An arm passing the stair porthole | Not built |
| K7 | 1066-1372 | Beat-6 encounter: suckers on pane 5 lit by lamp A (KK4); a dark arm above panes 1-3 (KS16); the eye in pane 6 lit by lamp B (KS17 at s 1268); a strike | Built; KK4 restored in the fix wave; the cinematic gaze now holds on the eye |
| Stinger | after the goal | Three hooked tentacle columns rise behind the crest, seen from the lantern platform 7.7 km away | Drawn on the final build at StingerScale 3.5 (ruling R3) as three dark hooked shapes about 75 px above the crest; modest in size, not frame-filling (section 5.1) |

Sources: show, gallery and kraken reports in `Logs/wbx_reports.json`, `Logs/wbx2_reports.json` and
`Logs/fix_x2_reports.json`; `README.md` (K5 window); `docs/ADDENDUM_A1.md` 1.6; DEVELOPMENT.md rows R3 and "Stinger posed but not drawn".

### 1.3 The ending: express elevator, shuttle and the look back

The ending follows lead decision 1 in `docs/ADDENDUM_A1.md` section 00. It replaced the inclined lift on a buttress fin
that the addendum's section 3 had chosen; the fin was never built. At the end of the lee ledge a glass cabin with glass
floor panels runs vertically down the outside of the lee face to the foot of the dam. The sunset happens during the
descent. At the foot the cabin couples to a horizontal maglev shuttle that runs 3.5 km west while the camera turns to
look back at the wall. The shuttle stops at the lantern platform, a small lit plaza at the edge of the lowland town
lights. The walker then walks 38.5 m to the Keeper's Lantern, and the final look back shows the whole dam face, from its
foot to the crest, in the dusk haze.

Ride phases. Durations are constants in `BulwarkConst.cs`; the ride runs at 2.5 m of s per ride second at every walking
speed, so the s ranges and the 2.5 m/s times are computed from the constants and the measured ride start (600.85 s).

| Phase | s (m) | Duration | At 2.5 m/s | Notes |
|---|---|---|---|---|
| Boarding | 1500-1510 | 4.0 s | 600.85-604.85 | Doors close over 0.5-3.0 s, seal over 3.0-4.0 s |
| Descent | 1510-1697.5 | 75.0 s | 604.85-679.85 | Vertical, peak 110 m/s (autotest `cabinSpeedPeak`). Its S-curve peaks at 8.12 m/s^2 (0.83 g); ruling R1 accepts this |
| Transfer | 1697.5-1712.5 | 6.0 s | 679.85-685.85 | The cabin settles and couples to the shuttle bogie |
| Shuttle | 1712.5-1850 | 55.0 s | 685.85-740.85 | 3500 m west: 20 s ramps, 15 s cruise at 100 m/s, 5.0 m/s^2 (0.51 g) (ruling R1; it was 35 s with a 152 m/s peak) |
| Doors | 1850-1857.5 | 3.0 s | 740.85-743.85 | The north door opens; measured ride end 743.85 s |
| Ride total | | 143.00 s | measured in both autotests (`rideTime`) | |
| Platform walk | 1857.5-1896.0 | 15.65 s at 2.5 m/s | goal at 759.50 | North, a right arc, then east to the Keeper's Lantern; rest at s 1897.5 |

End sequence after the goal (DEVELOPMENT.md rows "End look", "After-goal sun sink", R2; `FrameworkConst.cs`):

- The camera holds the KS28 look back at pitch 29 until t_g 18 (t_g = time since the goal trigger). At pitch 29 the
  foot and the crest are both in frame.
- It tilts up 7 deg over t_g 18-28 (peak 1.31 deg/s) and holds at pitch 36 until the fade at t_g 40, which leaves room
  for the stinger above the crest.
- The sun sinks over t_g 9-23, and the crest goes dark at t_g 19.0 (measured 19.00 in the real end path, section 4.5).
- Then 4.0 s fade, 1.5 s black, and the end line (fade in 2.0 s, hold 6.0 s, fade out 2.0 s). The end sequence lasts
  55.5 s (40 + 4 + 1.5 + 2 + 6 + 2); ruling R2 refused a shorter end.

Sky and far field built for the ending:

- the earth's shadow with the true penumbra, and the alpenglow band on the upper wall;
- afterglow light on the west-facing lee face, and tone variation on the face at far scale;
- the five outlet jets, continued as falling spray veils;
- three roads of street lamps with their light pools, a town glow round the platform, and trees east of the platform;
- beat-7 parts culled from far away, so they no longer show as a pale bar on the crest;
- fix wave (ending builder) for KS28: an ending-only valley haze, a twilight-arch gradient through the penumbra, doubled
  far amplitudes with 150 m monolith blocks and joints, lamp clusters, a guide light every 100 m on the elevator rail line,
  a bigger returning-cabin glow, jet arcs cut at the crest line and matt trees. At 4K the face now runs from 68.5
  levels at the crest band to 40.5 at mid-wall (a 28-level gradient, about 4 before). KS23 has its sun disc again (luma
  231), and the lowland mist rivers got a density ramp over 250 m and softer billows (ending report).

Not built: the outlet basin at the foot, broken tree silhouettes, the "Belt of Venus" sky gradient, and the floating
origin of A1 3.8 (section 5.1, T-PREC).

### 1.4 Sound

All sound is procedural, synthesised in code:

- **The score (`BulwarkScore`, `BwBeds`).** Bed layers are keyed to s with 30 m crossfades. Offline walks at 2.5 m/s in
  Wave X gave beat RMS levels of -24.0, -18.1, -23.1, -32.2, -38.1 and -26.2 dBFS for beats 2-7, against targets of
  -24, -18, -23, -32, -38 and -25. Beat 1 over s 2-97 was -34.8 against -34 (audio report, `Logs/wbx_reports.json`).
- **The kraken voice (`KrakenVoice`).** Growl, creaks, water roar, slap, dam shudder, sucker sounds, eye swell, stinger
  and, since the fix wave, the beat-6 strike with concrete modes, a pane boom and a glass crack. Each call is spatialised
  by distance and bearing, with low-passes behind glass and through the wall, and sent to its own reverb and limiter.
- **Small speakers (fix wave, audio builder).** Calls, roars and the stinger carry more energy above 250 Hz; each kind
  has its own small-speaker layer; outdoor levels are 2 dB higher and the kraken bus limiter ceiling is -3 dBFS (was
  -6). `audio_analysis.py` has a small-speaker gate (high-passes at 250 and 400 Hz; 80-800 Hz must clear the bed by 8
  dB). In the merged game every standalone call and roar passes with +8.9 to +13.5 dB (audio report).
- **The stinger sound.** On the final build the real end path logs stinger calls of 13.0-14.0 s at -31.6 to -30.7 dBFS,
  7771 m away (`Logs/fin-end.out`).
- **The ride (`RideSound`).** Motor whine, air rush, track joints, cabin hum, pressure rumble, door and coupling cues, an
  ear low-pass during the descent and, since the fix wave, the A1 "sunset hush". The ride sound reads the real
  `EndingRide` state, so the 55 s shuttle needed no audio change. Pause now fades and freezes the kraken and ride sounds.

Not verified: no human listening pass on laptop or phone speakers was done; the small-speaker gate is a measurement, not
a listening test. Full `-audio-dump` autotests at both speeds were not run (audio report, open items).

---

## 2. The user's orders over the run

| When (local) | Order or event | Effect |
|---|---|---|
| 2026-11-13 19:22 | Run start | Anchor `Run started at 2026-11-13T19:22:09.4636135+01:00`, line 1 of STATE.md |
| 2026-11-14 ~10:15 | First pause: stop and record the stop point | Wave B stopped; WIP committed (3171449) |
| 2026-11-14 12:25 | "продолжай" (continue) | Lead check; Wave B relaunched |
| 2026-11-14 | Second pause | Wave B stopped again; WIP 2d61d6a |
| 2026-11-15 ~21:10 | Continue in a new chat session | Lead check 03 |
| 2026-11-15 ~21:20 | Third pause ("stop for now") | Nothing launched |
| 2026-11-19 | **Redesign order** (below) | Overrides MASTER_PROMPT where they conflict; design addendum `docs/ADDENDUM_A1.md` |
| 2026-11-19 | The user sent the Tripo3D models and said "start". The user also asked for the GPU to be watched at all times | Wave A1 (design track plus kraken pipeline); watchdog `Logs/gpu_watch.ps1` |
| 2026-11-19 ~20:00 | Fourth pause ("останови пока работу") | WIP d71716c |
| 2026-11-20, before 11:00 | "продолжай" | Kraken pipeline re-run (build_kraken_08). Lead check 04 did not complete on an overloaded machine |
| 2026-11-20 ~11:00 | Fifth pause ("всю работу останови") | Everything stopped |
| 2026-11-20 ~11:24 | **Speed order** (below): the finished game within 10 hours (by about 21:30), the whole machine, no queues | Wave X: builders in six project copies (the main root and five git worktrees); rules 1-5 at the top of STATE.md |
| 2026-11-20 | The user ordered every kraken motion above 8 m/s slowed down | K5 slap becomes a slow topple (DEVELOPMENT.md row "Kraken speeds") |
| 2026-11-20 14:04 | Stop at the usage limit (weekly usage 99 %; STATE.md rule 4, not a user pause) | Every worktree committed on its branch |
| 2026-11-20 14:25-14:30 | Resume after the weekly usage reset | Wave X2: six builders |
| 2026-11-20 16:35 | Wave X2 merged into master 2689989 | Lead check, gate run, captures, six-lens critic panel |
| 2026-11-20 17:10 | Fix wave launched | Nine builders, report by 19:00 |
| 2026-11-20 ~19:00 | Fix wave reported (2 done, 7 partial); lead integration round | Commit 86e5d47: cross-role requests applied, stinger drawn in the real end path |
| 2026-11-20 19:00-19:45 | Final lead check, final captures, both autotests, performance alone | Commits 1a1407e, a29e2da, 145a648 |
| 2026-11-20 ~20:00 | Finished under the speed order: one panel, one fix wave, one integration round | STATE.md "FINISHED" note |

**The redesign order of 2026-11-19 (STATE.md, "USER REDESIGN ORDER")** said that the game did not yet produce the
megalophobia it must. It asked for two changes:

1. **The kraken.** A colossal, frightening kraken swims on the right (sea, east) side of the dam. It is seen partly from
   the crest and again from the window gallery during the descent. It is revealed gradually, so the viewer slowly grasps
   its size, and it is never static. The user's reference look is an octopus-like kraken: a bulbous, rock-like,
   barnacled mantle, glowing orange-red eyes in wrinkled sockets, and very long tentacles with red undersides and pale
   suckers. The model comes from Tripo3D (the external-asset exception above).
2. **The ending.** The player descends to the foot of the dam, for example in a high-speed elevator, and walks away to
   where the lanterns stand. Turning round must show the true size of the 6 km dam.

**The speed order of 2026-11-20 (STATE.md rules 1-5)** set the following:

- **Speed.** Launch the builders first, run checks in parallel, set hard report deadlines, and use one global six-lens
  panel plus one fix wave instead of per-builder critic rounds.
- **The whole machine.** No machine-wide GPU queue and no GPU hold (both removed from `Tools/run-player.ps1`), one Unity
  queue per project folder, parallel git worktrees, and up to 3 of our renderers at once (2 when one is 4K). The GPU
  watchdog only monitors.
- **Quality.** No visible defects, video first, and megalophobia everywhere.
- **Usage limit.** Stop everything at 99 % weekly usage.
- **Pauses.** "останови" means stop everything, commit, write a pause note and wait for "продолжай". Talk with the user
  in Russian.

---

## 3. Architecture in short

- **Framework.** The framework of the earlier "Thread" game was imported and adapted, with parts of "Abyss" and "The
  Ring" (`docs/IMPORT_MANIFEST.md`, `docs/FRAMEWORK.md`, ADRs D-01 to D-10 in DEVELOPMENT.md). The class prefix is `Bw`.
- **Path and player.**
  - `BwPath` / `FilletPath` hold the path in double precision with lane coordinates (s, lateral); `EndingPath` adds the
    ride and the platform walk.
  - One lane function `BwPath.Lane(s)` drives the corridor colliders (`PathGuide`) and the wall bot.
  - `PlayerController` and `CameraRig` handle the first-person view: eye height 1.7 m, roll held at 0.
  - `AutoWalker` with `WalkPlanner` / `GazeSchedule` runs the cinematic autowalk: constant 2.5 m/s, a 0.5 s start ramp,
    no holds, and gaze keys in world (s, yaw, pitch), interpolated since the fix wave by a monotone cubic. The same walker
    drives `-autotest` at 1.5 and 2.5 m/s.
- **Beats.**
  - `BeatDirector` runs one `IBeatBuilder` per beat. Each beat replaces its part of the graybox through `[ReplacesGraybox]`.
  - Constants live in `BulwarkConst` and colours in `BulwarkPalette`; `Tools/gen_hlsl.py` generates the matching HLSL.
- **Rendering.**
  - URP 17.6 with handwritten HLSL shaders (`Bw*`) and procedural textures generated at edit time (`BwTexGen`).
  - One writer of the atmosphere globals (`AtmosphereDriver`, `BulwarkAtmosphere.hlsl`).
  - One post pass, `BwGrade`, holds the grade, the fade and the single static blue-noise dither. The camera renders no
    URP post.
  - Special systems: `BwSea` (the ocean), `MistVolumeFeature` (fall, mist and rainbow as one analytic volume pass),
    `BwWaterPortal` (the sea behind the beat-6 glass), `BwFarField` (lowland, mist rivers, cities, jets, the far dam
    face), `LocalLightPool`, and the kraken (`KrakenRig`, `KrakenCreature`, `KrakenCascade`).
- **Shared contracts between builders.** `WaveImpacts`, `SeaDisturbances`, `KrakenCalls` and `EndingRide`; each has
  exactly one writer.
- **Audio.** Procedural DSP (`BulwarkScore`, `BwBeds`, `KrakenVoice`, `RideSound`) on a clip-less audio source, with an
  FDN reverb and a limiter.
- **QA in the built player.**
  - In-game runners: `CaptureHarness` / `ShotPlan` (key shots, stills, sequences, including the real end path and the
    `ride`, `kseq` and `tprec` sets added in the fix wave), `AutoTestRunner`, `SoakRunner`, `PerfPoseRunner`, the wall
    bot and `VisibilityAudit`.
  - Offline Python tools in `Tools/qa/`: image metrics, sequences, encode test, audio analysis, contact sheets.
  - `Tools/run-player.ps1` runs the player and scans its log; `Tools/unity-queue.ps1` runs the Unity batch jobs.
  - `Tools/metrics.py` measures time, tokens and cost.

**How the work was organised (STATE.md).** All work ran as Ultracode workflows:

- **Phases 0-2 (2026-11-13).** Preflight, research, then the framework with a graybox of the whole path, the autotest and
  the first build.
- **Wave A (2026-11-14).** Review fixes, atmosphere, materials, and the graybox gate.
- **Wave B (2026-11-14 to 11-15).** Sea, dusk, mist and integration, interrupted by pauses.
- **Wave A1 (2026-11-19).** The design addendum and the kraken pipeline.
- **Waves X and X2 (2026-11-20).** Builders in six project copies (the main root on master and five git worktrees on
  branches `a2-*`), plus the lead's integration worktree `.worktrees/integ`: kraken and show in the main root; gallery
  (beat 6); audio; ending (beat 7 and the ride); seamist (sea and mist); land (interiors and reef).
- **Then (2026-11-20):** lead merge, gate run, the six-lens critic panel, one fix wave of nine builders, the lead
  integration round, and the final checks.

---

## 4. QA evidence

### 4.1 Lead checks

Each lead check runs RebuildAll, Validate, the EditMode tests, a player build and a smoke run in the built player.

| When | Commit | Result | Source |
|---|---|---|---|
| 2026-11-13 Phase 2 | cb1972c | Validate 51/51, EditMode 120/120 (PlayMode 2 pass, 2 skipped by design), build OK (98 MB); autotest 1.5 m/s PASS (trigger 999.27 s), 2.5 m/s PASS (599.67 s) | STATE.md "Milestones" |
| 2026-11-14 Wave A graybox gate | 6b24b61 | EditMode 195/195, Validate 0 problems, build OK; autotests PASS at both speeds; soak 1800 s PASS (managed growth -0.43 MB) | STATE.md "Current phase" |
| 2026-11-14 resume | WIP 3171449 | RebuildAll OK, EditMode 245/245, build OK, Validate 1 problem (two writers of `_BwSurface`, fixed later) | STATE.md |
| 2026-11-15 lead check 03 | 2d61d6a | RebuildAll 0 problems (idempotent), Validate 66 checks PASS, EditMode 252/252, build OK, smoke at 25 m/s PASS | STATE.md |
| 2026-11-20 lead check 04 | WIP | Incomplete: machine overloaded and the licensing client kept disconnecting; EditMode 272/277 | STATE.md |
| 2026-11-20 Wave X2 merge | 2689989 | RebuildAll, Validate 77/0, EditMode 354/354, build OK, smoke PASS | STATE.md (16:35 note) |
| **2026-11-20 final** | master after the integration commit 86e5d47 (recorded in 1a1407e) | **RebuildAll SUCCESS (0 problems), Validate 78 checks / 0 problems, EditMode 382/382 (104.6 s), build OK (231.1 MB, 0 errors, 4 warnings), smoke `-autotest -speed 25` PASS (trigger 205.10 s, expected 205.10 +- 3; done 260.60 s)** | `Logs/lead_final.out`, `QA/tests/editmode_final.xml` |

### 4.2 Autotests on the final build (built player, 1920 x 1080)

| Check | 2.5 m/s cinematic (`-autotest -cinematic`) | 1.5 m/s walk (`-autotest`) |
|---|---|---|
| Result | PASS | PASS |
| Goal trigger | 759.50 s (expected 759.50 +- 3) at s 1896.021 (lantern 1896.000) | 1170.17 s (expected 1170.17 +- 4) at s 1896.013 |
| Ending done | 815.00 s (expected 815.00 +- 3; exit limit 823 s) | 1225.67 s (exit limit 1234 s) |
| Rest at the lantern | s 1897.500 (expected 1897.500) | same |
| Ride | 600.85-743.85 s, 143.00 s, phases in order, cabin peak 110.0 m/s | 1001.25-1144.25 s, 143.00 s, phases in order, cabin peak 110.0 m/s |
| Progress speed | 2.5000 constant | 1.5000 constant |
| Ground speed peak | 2.597 m/s | 1.558 m/s |
| Max roll | 6.4e-6 deg (limit 0.01) | 7.2e-6 deg |
| Beats | 7, in order, one event each; lantern goal once | same |
| Camera probe in colliders / safety snaps / collider coverage misses | 0 / 0 / 0 | 0 / 0 / 0 |
| Errors / warnings in the log | 0 / 0 | 0 / 0 |
| Load time | 6.22 s | 5.96 s |
| Frames (virtual 60 fps) / real time | 48 900 / 822.4 s | 73 540 / 1232.9 s |
| VRAM peak / max GPU temperature | 3944 MiB / 56 C | 2801 MiB / 44 C |
| Report / log | `QA/reports/autotest_cinematic.json`, `Logs/fin-auto25.out` | `QA/reports/autotest_walk.json`, `Logs/fin-auto15.out` |

VRAM and temperature come from `QA/vram/vram_summary.log` (runs fin-auto25 and fin-auto15). Against the X2 gate build
(goal 739.50 / 1150.17 s), every time from the shuttle on moved 20 s later, as ruling R1 predicted.

So a cinematic take runs **13:35 from the start of the walk to the end of the ending, and 12:39.5 to the lantern**.

### 4.3 Soak

| Run | Build | Result | Source |
|---|---|---|---|
| 1800 s at 2.5 m/s (`-soak 1800 -speed 2.5`, VSync on), 2026-11-20 17:25-17:55 local | The lead QA builder's fix-wave build in `.worktrees/integ` (`Builds/leadqa`) | PASS: 2 rides and the ending completed (last goal at 1478.6 s and last ending done at 1534.5 s on the soak clock; 1 loop counted, loop time 739.0 s, max s 1847.5); 296 281 frames; managed growth -5.17 MB, total 19.75 MB (limit 50 MB); 0 errors, 0 warnings | `.worktrees/integ/QA/reports/leadqa/soak/soak.json` (copy in `QA/reports/leadqa/soak/soak.json`) |
| 900 s at 1.5 m/s (`-soak 900`, VSync on), 2026-11-20 16:45-17:01 local | Master 2689989 (X2 gate) | PASS: 160 105 frames; managed growth -0.93 MB, total -15.81 MB; 0 errors. It ends at s 1349.7 (beats 1-6) | `QA/reports/soak.json` |

Honest notes:

- The 1800 s soak ran on the lead QA builder's build before ruling R1 reached it (its goal is at s 1846 and its loop is
  739.0 s), not on the final master build. No soak was run on the final build itself.
- It ran during the fix wave on a shared machine. It logged a longest frame of 554.2 ms and 528 frames above 33 ms;
  these were not analysed. The performance run on the final build, measured alone, has 0 frames above 33 ms (4.4).

### 4.4 Performance, measured alone on the final build

`-perf-poses KS17,ride,KS28 -uncapped -nofade`, 2 s warm-up and 6 s measured per pose; at 4K the camera renders off
screen at 3840 x 2160 (`QA/perf/final/4k|1080/perf_poses.json`, `perf4k.csv`, `perf1080.csv` and their summaries).

| Pose | 4K avg (ms / fps) | 4K p99 (ms) | 1080p avg (ms / fps) |
|---|---|---|---|
| KS17, the eye in the window gallery (s 1268) | 7.37 / 136 | 8.32 | 3.45 / 290 |
| Ride, descent (s 1600) | 5.96 / 168 | 6.70 | 1.63 / 614 |
| KS28, the final look back (s 1897.5, t_g 8.5) | 7.48 / 134 | 8.70 | 2.24 / 447 |

| Whole perf run (three poses, 24 s) | 4K | 1080p |
|---|---|---|
| Average | 7.09 ms, 141 fps | 2.25 ms, 445 fps |
| 1 % low / 0.1 % low | 89.9 / 61.9 fps | 190 / 98.1 fps |
| Longest frame / frames over 33 ms | 30.2 ms / 0 | 14.4 ms / 0 |
| Targets in the summary | avg 60 fps, no frame over 33 ms: met | avg 90 fps, 1 % low 60 fps, no frame over 33 ms: met |
| VRAM peak / max GPU temperature | 3295 MiB / 67 C | 2547 MiB / 64 C |

During the fix wave the lead QA builder had measured KS17 at 4K with a 19.43 ms average while other players shared the
GPU (fix report); the final run alone replaces that number. Only these three poses were measured; the other key shots
have no final perf numbers.

### 4.5 Sequences and the real end path

**37 sequences of the merged build** (`QA/captures/x2-seq`, `QA/reports/sequences_x2-seq.md`): 112 checks PASS and 5 FAIL
in the raw report.

- The beat transitions and the 13 pans pass. The doorway luma ramps (seq_b01_b02 2.11, seq_b04_b05 2.45 levels per frame)
  are under the 2.6 gate of the lead ruling on content ramps.
- Four of the five FAILs were the static-flicker check on the moving sea. Under the lead ruling (each 64 px block series
  high-passed above 1 Hz, limit 3 levels unchanged) they pass at 1.97 / 2.47 / 0.60 / 1.28 levels with 0 fireflies
  (`QA/reports/sequences_x2-seq-hp.md`, commit 49d1068).
- The fifth was `seq_end`, still keyed to the old goal; it is replaced by the real-path run below.

**seq_end on the final build** (`QA/captures/fin-end/report.json`, `Logs/fin-end.out`), mode RealEnd from s 1893 to the
rest point, 1723 of 1776 planned frames written:

- the goal triggers at frame 42 (goal s 1896);
- the stinger is drawn from t_g 0.03; in all 372 KS29 frames it stands above the crest, with a contrast of 33.2 levels
  against the sky (minimum 6);
- the crest goes dark at t_g 19.00 (expected 19 +- 0.5); the sun is at -2.35 deg at the goal;
- max luma step 0.93 levels per frame, 0 magenta frames, 0 failed frames; the harness records 0 failures.
- The harness verdict line also counts 19 difference spikes in this sequence; no source read for this report analyses
  them.

**Other sequences of the final build:**

- `seq_ride_descent` (fin-ending): ok, 216 frames, max luma step 0.37, 0 spikes.
- Kraken show sequences (fin-show, 9): 5 ok (k2_30, k4_30, k5, k5_30, stinger). The harness notes on the other four are
  a luma step of 1.24 levels per frame in seq_show_k1 and 2.00 in seq_show_k4 (limit 1.2), and 4 and 5 difference spikes
  in seq_show_k2 and seq_show_k3. They were not triaged.
- The ride door step in seq_turnback (2.23 levels per frame as a door post leaves the frame) is accepted by a lead ruling
  at 2.6 or less (DEVELOPMENT.md).

### 4.6 Final captures and image metrics

| Capture folder | Content | Image metrics (`Tools/qa/image_metrics.py`) | Harness |
|---|---|---|---|
| `QA/captures/fin-stills` | 198 stills: key shots and beat stills at 1080p, 4K, 16:10 and 21:9 | 38 failing images; 1555 PASS / 38 FAIL / 3 INFO (`QA/reports/image_metrics_fin-stills.md`) | 0 failures; 4 framing failures (KS27 in the keyshots set, all four variants: no end clock, section 5.1); 3 luma out of range |
| `QA/captures/fin-ending` | 48 ending stills plus `seq_ride_descent` | 11 failing images; 394 PASS / 11 FAIL (`QA/reports/image_metrics_fin-ending.md`) | 0 failures |
| `QA/captures/fin-show` | 25 kraken show stills plus 9 sequences | no image-metrics report written | 0 failures (sequence notes in 4.5) |
| `QA/captures/fin-gallery` | 20 gallery poses | no image-metrics report written | 0 failures |
| `QA/captures/fin-end` | `seq_end` in the real path | - | section 4.5 |

What the failing images are:

| Check | fin-stills | fin-ending | Detail |
|---|---|---|---|
| NaN-like pixels | 22 | 6 | 1-5 isolated bright pixels per image (one dark one), for example distant lamp cores; spread over beat 2-5 stills and the ride and platform shots; 13 of the 22 stills are 4K, 16:10 or 21:9 variants |
| Fireflies | 7 | 4 | KS19 at all four variants (64, 66, 237 and 432 px: the conduit seen through the grating), KS26 21:9 (54 and 55 px), KS23b 21:9 (33), KS25 4K (13), b07 look_r (5), KS24b 16:10 and 21:9 (1 each) |
| Mean luma outside the beat band | 9 | 1 | Mostly free-look and end poses (b01 end, b02 look_down, b03 look_down and look_down90, b04 end and look_r), plus KK2 16:10, KS09 21:9 and KS26b 16:10 |

### 4.7 Final 4K key shots

`QA/final/` holds 32 4K stills of the final build: KS01-KS26b and KK1, KK2, KK4 from the keyshots set, and KS28
(`b08_Ending_KS28_dam_reveal_4k.png`) from the ending set. KS27 and KS29 from the keyshots set were left out because
those stills lack the end clock; the ending capture set is the reference for KS27-KS29 (DEVELOPMENT.md, "Final 4K key
shots").

### 4.8 Precision at the lantern platform (T-PREC, ruling R6)

There is no floating origin (A1 3.8 `BwFrame` was not built). At the platform eye, about 6 km from the origin, the float
spacing is 0.49 mm. The lead QA measurement (`QA/reports/leadqa/tprec_dither_*.json`): static camera over 120 frames
0.000 px; a 0.1 px/frame pan max 0.517 px, rms 0.165 px, against 0.19-0.20 px near the origin with the same method. This
is logged as a known limitation (section 5.1). The cabin clip was inconclusive (too few camera-fixed textured tiles).

### 4.9 Wall bot

The last wall-bot run is the lead QA builder's, on 2026-11-20 at 18:00 local in `.worktrees/integ` (`-wallbot -speed
10`): PASS, 0 failures, 0 errors, four passes along the walked path (two to s 120, two to s 1497.5-1500), 0 camera probe
violations (`QA/reports/leadqa/wallbot/wallbot_speed10.00.json`). Platform passes were added to the wall bot in the
integration round (commit 86e5d47); no wall-bot run on the final build is recorded in the files read for this report.

### 4.10 Critic panel and the fix wave

**The six-lens panel** (workflow `bulwark-panel`, run `<WORKFLOW_ID>`, read-only, on the captures of master 2689989).
Six critics with distinct lenses wrote their own findings; a chief critic removed duplicates, checked the most important
claims by eye, dropped stale or refuted items and gave each finding to a role.

| Lens | Raw findings (S2 / S3 / S4) |
|---|---|
| Scale, megalophobia and wow | 8 / 7 / 0 |
| Visual defects (Part 10 catalogue) | 7 / 8 / 3 |
| Materials, light and colour | 8 / 6 / 1 |
| Motion, pacing and camera | 5 / 5 / 0 |
| Audio | 4 / 4 / 5 |
| Spec conformance and robustness | 8 / 9 / 1 |

Chief critic's verdict: "not ready for sign-off, but nothing is broken at S1 level", with 0 S1, 24 S2 and 36 S3/S4
findings after the merge, spread over kraken, gallery, show, audio, ending, lead, sea, mist and interiors
(`Logs/panel_x2.json`).

**The fix wave** (workflow `bulwark-fix`, run `<WORKFLOW_ID>`, nine builders, reports in `Logs/fix_x2_reports.json`):

| Builder | Copy | Status | Main results (own report) |
|---|---|---|---|
| kraken | main root | partial | The eye (lens pupil, cornea catchlights, no "lips"); falling water off emerged parts; a dark stinger hide from far away. K4 arch flanks still red |
| show | main root | partial | K3 visible in the KS07 slot; kraken sound durations follow the walking speed; stinger at 3.5x as hooked crooks; motion peak 7.46 m/s at 2.5 m/s |
| gallery | .worktrees/gallery | partial | KK4 restored; the eye scaled 1.63x for the approach and the hold; the beat-6 call table scales with speed; the teal water needed shared constants (applied by the lead) |
| ending | .worktrees/ending | partial | R1 (ride 143 s), R4 (monotone cubic gaze, beat-6 keys on the eye), R5 (key-shot rows KS22-KS29, KK1, KK2, KK4); KS28 far-field rework; KS23 sun disc; softer mist rivers. KS19 not fixed |
| audio | .worktrees/audio | done | Small-speaker layers and gate; stinger as phrases; the beat-6 strike with a glass crack; pause freezes; sunset hush |
| sea | .worktrees/seamist | partial | Mid-sea streaks removed by a per-wave band limit; streaked near foam. Horizon crest silhouettes only partly done |
| mist | .worktrees/seamist | partial | K5 spray column fixed (462 px at 4K); KS08 sun-break gold. KS07 brightness needed the exposure owner |
| interiors | .worktrees/land | done | KS14 stain replaced by a soft wet decal; wave impacts burst through the KS02 slits |
| leadqa | .worktrees/integ | partial | `seq_end` re-keyed to the real goal; new sequence sets; VisibilityAudit without the whale; `PerfPoseRunner`; T-PREC measured; found that the stinger did not render |

**The lead integration round** (commit 86e5d47) applied the builders' cross-role requests: the K2 gaze row, the water
veil constants, the hall ambient, the storm LightCal, the KS07 EV hold, KS28 at 21:9, the wall-bot platform passes, the
`BwWArmColour` clamp, the audit ids, the still end clock (it reaches the ending set; the keyshots set still lacks it), the
crest joints via `BwRoad`, the stinger drawn in the real end path, and the row order of the harness stinger-contrast
probe.

**Fixed after the fix wave and integration** (verified by the final checks above where a number is given):

- the kraken eye: a lens-shaped pupil, cornea catchlights, no "lips";
- the window water: teal murk instead of aquarium blue;
- KK4, the arm on the glass, restored;
- the stinger drawn and dark at StingerScale 3.5 (it had been switched off by `Beat08Ending.ApplySets`); contrast 33.2
  levels in `seq_end`;
- water falling off the kraken;
- the KS28 look back: alpenglow at the top, trees at the foot, the elevator rail and the cabin light, the jet veils;
- the shuttle at 55 s (R1), measured as a 143.00 s ride;
- the K5 slow topple, and all kraken motion at 8 m/s or less at 2.5 m/s;
- the kraken sound on small speakers (measured gate);
- the beat-6 gaze on the eye.

A second full critic round was **not** run (user's speed order: one panel plus one fix wave).

---

## 5. Known issues

### 5.1 Known limitations of the final build

| Item | What it is | Source |
|---|---|---|
| T-PREC at the lantern platform | No floating origin (A1 3.8 not built). A 0.1 px/frame pan jitters up to 0.52 px against 0.2 px near the origin; a static camera shows 0 px. The end camera is static or tilts slowly, so this is a sub-pixel edge crawl at most | DEVELOPMENT.md "Final lead / T-PREC" |
| KS19 grating fireflies | 64-432 isolated bright pixels at the conduit seen behind the bars (metric; all four variants) | `image_metrics_fin-stills.md`; ending report |
| Single NaN-like pixels | 1-5 isolated pixels in 22 final stills and 6 ending stills, for example distant lamp cores | 4.6 |
| The stinger is modest | It reads as three dark hooked tentacles about 75 px above the crest, not frame-filling | DEVELOPMENT.md "Stinger posed but not drawn"; show report |
| KS27-KS29 in the keyshots set | Those stills lack the end clock (4 framing failures for KS27); use the ending capture set (`-capture-set ending`) | DEVELOPMENT.md "Final 4K key shots" |
| Kraken K6 (porthole) and K4a | Not built | show report |
| Beat-4 horns | Not built as real units | the lead's final list of known limitations |
| Window-gallery suckers | Glow cyan under the lamps (water absorbs red over the lamp path) | gallery reports; panel S3 |
| Perfection loop | Two consecutive clean critic rounds (MASTER_PROMPT 2.4, 15.1 item 2) not completed | section 4.10 |

### 5.2 Open items the fix-wave builders reported

These come from the builders' own reports (`Logs/fix_x2_reports.json`). They are not in the integration commit's list,
and no second critic round checked them:

- **kraken:** the K4 arch flanks are still saturated red at noon; the KS17 pupil measures luma 18 against the target of
  10 or less; the small 8 m eye was dimmed but not moved; the falling water reads as soft streaks rather than torn
  ribbons; the S3 findings (decimation at gallery range, cyan sucker rings) were not done.
- **show:** the K3 V-wake stays faint (p95-p5 68 with the kraken against 64 without); KK2 still shows one main eye.
- **gallery:** the sucker and skin texture shows a grid of vertical stripes under the grazing lamp (KK4, right half;
  the Tripo tile magnified); the eyes at depth are a dim teal ring, not green points.
- **ending:** at KS28 the step from the dam foot to the ground is still about 8.5 levels over 3 rows (target under 4); a
  peach trapezoid at the crest in KS28 has no identified owner; the outlet basin, broken tree silhouettes and the sky
  gradient above the crest are not built; KS19 is unchanged.
- **sea:** no distinct crest silhouettes near the horizon (beyond about 600 m the swell is under 3 px per wavelength and
  the no-sparkle filter removes it).
- **mist:** a new 12 x 1 px gold dash at the KS08 horizon was reported for the lead to bisect; no result is recorded in
  the files read for this report.
- **audio:** the K5 slap clears the K4 sink roar by only +3.82 dB (80-800 Hz); the full audio-dump autotests and a human
  listening pass were not done.
- **interiors:** the KS02 slit burst was checked with stills only, and its frame-time cost was not measured.

### 5.3 What the automated checks still flag

- Image metrics: 38 of 198 final stills and 11 of 48 ending stills fail at least one check (4.6). None of these is an S1
  in the panel's sense, but they are open by the letter of MASTER_PROMPT 15.1 item 3.
- Sequences: the harness notes 19 difference spikes in `seq_end` and four kraken show sequences with notes (4.5).
- The soak through the ending ran on a pre-R1 build of the lead QA copy, not on the final build (4.3).

### 5.4 Not verified

- A second full critic round on the final build (not run, by the speed order).
- The encode test (banding after a YouTube-like re-encode) on the final build: `QA/encode/` holds only the tool self-test
  of 2026-11-13.
- A human listening pass of the sound (1.4).
- A wall-bot run and a soak on the final build itself (4.3, 4.9).
- Performance at poses other than KS17, the ride and KS28 (4.4).

### 5.5 Rulings and deliberate deviations

Rulings this run are rows in DEVELOPMENT.md (two of them are listed in the STATE.md note of 16:35):

| Ruling or deviation | Content | Authority |
|---|---|---|
| External assets: the two Tripo3D GLB files | The only exception; everything is regenerated from them by code | User order of 2026-11-19 |
| An express elevator and a maglev shuttle, which are machinery ("no machinery, no sci-fi", 14.1) | The new ending | User order of 2026-11-19; A1 0.2(b); lead decision 1 |
| Run time longer than the 600 s cinematic of the specification | 759.50 s to the goal, 815.00 s to the end | A1 0.2(c); R1 |
| R1 ride profile | Descent 75 s, peak 110 m/s, 0.83 g S-curve peak accepted; shuttle 55 s with 20 s ramps (0.51 g) | DEVELOPMENT.md |
| R2 end camera | Retime refused; EndTotal stays 55.5 s | DEVELOPMENT.md |
| R3 visual stinger (A1 1.9 said sound only) | StingerScale 3.5, a dark silhouette; speed limit 8 m/s in the animal's proportions | DEVELOPMENT.md |
| R4 gaze keys | One writer (the ending owner) | DEVELOPMENT.md |
| R5 key-shot rows | KS22-KS29, KK1, KK2, KK4 by the ending owner | DEVELOPMENT.md |
| R6 no floating origin | Measure T-PREC; logged as a known limitation with the measurement | DEVELOPMENT.md "Final lead" |
| Kraken speeds | Every sustained motion 8 m/s or less at 2.5 m/s, K5 included | Lead decision 2; user order of 2026-11-20 |
| Doorway luma ramps | Gated at 2.6 levels per frame instead of 1.2 (content ramps, not exposure steps) | Lead ruling 14.10.5 |
| Ride door step | seq_turnback 2.23 levels per frame accepted at 2.6 or less, 0 spikes required | Lead ruling 14.10.5 |
| Sea static flicker | Block series high-passed above 1 Hz, limit 3 levels unchanged | Lead ruling 14.10.5 (commit 49d1068) |
| KS07 gate house | In frame, cut by the left edge | Lead ruling 14.10.5 |
| KS17 = win_eye at s 1268; PendantY 5.8 | KS17 is the win_eye pose at s 1268; PendantY = 5.8 | Lead rulings (STATE.md, 16:35 note) |

---

## 6. How to record the video

**Which build.** `Builds/Bulwark/Bulwark.exe`, the final build checked in section 4. `PLAY_WINDOWS.bat` in the project
root starts it and passes any arguments through.

**Controls** (MASTER_PROMPT Part 9):

| Input | Action |
|---|---|
| W A S D / left stick | Move |
| Mouse / right stick | Look (raw input, Y not inverted) |
| Shift / a shoulder button | Brisk walk (2.5 m/s) |
| C | Toggle the cinematic autowalk (constant 2.5 m/s from the current progress) |
| Esc | Pause overlay (Resume, Restart, Quit); after the end the game stays on black with Esc available |
| Alt+Enter or F11 | Toggle fullscreen |

**Recording procedure.**

1. Start the screen and audio recorder first. The game starts in black: the first wave impact is heard in the black, and
   the picture fades in with the title "BULWARK" (14.11).
2. Start the cinematic autowalk with `Bulwark.exe -cinematic` (or `PLAY_WINDOWS.bat -cinematic`), which starts it from
   s 0. Pressing C also works, but it starts from the current progress. The controls hint never appears in cinematic
   mode.
3. **Do not touch the mouse, keyboard or gamepad during the run.** Any input cancels the cinematic and returns control
   (Part 9). If that happens, press C to resume from the current progress.
4. Length: **815.00 s (13:35)** from the start of the walk to the end of the end line: 759.50 s (12:39.5) to the goal,
   then the 55.5 s end sequence. Add the load time (5.96-6.22 s in the final autotests). After the end line the game
   stays on black; stop the recorder there, then press Esc and Quit.
5. Resolution and aspect. Record at 1440p or higher. Measured alone on the RTX 5070, the final build runs at 134-168 fps
   at 4K uncapped at its three heaviest poses (4.4), without a recorder running. The final key shots in `QA/final/` are
   16:9; 16:10 and 21:9 are captured too, and several of those variants carry isolated-pixel or firefly flags (4.6).
6. Keep the default picture settings: the one static blue-noise dither (`-dither static`, the default) is there to stop
   banding after YouTube compression. Do not record with `-dither off` or `-nofade`. The encode test was not run on the
   final build (5.4).
7. The audio is procedural; record the system output. The mix targets a true peak of -1 dBTP or less (docs/FRAMEWORK.md
   section 6). The small-speaker fixes are measured, not listened to (1.4).
8. A 1.5 m/s walking run lasts 1225.67 s (goal at 1170.17 s). Recording it needs manual play, because `-speed` and
   `-brisk` apply only to the test walks.

**Timeline of a cinematic take** (2.5 m/s; m:ss from the start of the walk; from the final autotest and the constants):

| Time | Moment |
|---|---|
| 0:00 | Wave gallery (beat 1) |
| 0:48.25 | The door opens onto the crest (beat 2; KS03/KS04) |
| 2:48.25 | Spillway bridge (beat 3) |
| 4:40.25 | Armour slope (beat 4); the K4 head rises in the cove |
| 6:08.25 | Gate tower and the stair descent (beat 5) |
| 7:08.25 | Window gallery (beat 6); the kraken encounter, the eye at s 1268 |
| 9:08.25 | Sluice tunnel and the lee ledge at dusk (beat 7) |
| 10:00.85 | The cabin doors close; the descent runs 10:04.85-11:19.85 |
| 11:25.85 | The shuttle runs west for 55 s |
| 12:23.85 | The ride ends; the door is open at the lantern platform |
| 12:39.5 | The Keeper's Lantern: the goal and the final look back |
| 12:57.5-13:07.5 | The camera tilts up 7 deg; the crest goes dark at about 12:58.5 |
| 13:19.5 | Fade, black, the end line; done at 13:35 |

**Useful command-line flags** (`Assets/_Game/Scripts/Runtime/Core/CliArgs.cs`; Unity's standard screen flags are the
ones `Tools/run-player.ps1` passes):

| Flag | Use |
|---|---|
| `-cinematic` | Start the cinematic autowalk at 2.5 m/s |
| `-screen-fullscreen 0/1 -screen-width W -screen-height H` | Unity standard window and resolution flags. run-player uses `-screen-fullscreen 0 -screen-width 1920 -screen-height 1080` |
| `-progress X` | Start at plan distance X (m). It is clamped to the walked path (0-1500), so the ride cannot be entered directly |
| `-beat N` | Start at the start of beat N (1-7) |
| `-seed N` | Random seed for audio, sway and placement (default 1) |
| `-dither static\|temporal\|off` | `static` is the default, the one blue-noise dither the encode test is built around; `temporal` only for high-bitrate captures; `off` only for the encode-test baseline |
| `-nofade` | Skip the title fade (tests only, not for the video) |
| `-audio-dump <file>` `-audio-dump-seconds N` | Write the real audio output to a WAV (QA) |
| `-autotest`, `-soak S`, `-wallbot`, `-capture <dir>` with `-capture-set` / `-capture-only`, `-perf-log <file>`, `-perf-poses <list>`, `-perf-rt WxH`, `-uncapped` | QA runs (docs/FRAMEWORK.md section 5); run them through `Tools/run-player.ps1` |

---

## 7. Command reference (for re-checking)

From the project root in PowerShell; the path has spaces (STATE.md "Commands", docs/FRAMEWORK.md section 5):

- Build: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools\unity-queue.ps1 -Name build -Method Megastructure.EditorTools.BatchTools.BuildWindows`
- Autotest 2.5 m/s: `powershell -NoProfile -File Tools\run-player.ps1 -Name autotest25 -PlayerArgs "-autotest,-cinematic"`
  writes `QA\reports\autotest_cinematic.json`
- Autotest 1.5 m/s: `powershell -NoProfile -File Tools\run-player.ps1 -Name autotest15 -PlayerArgs "-autotest"` writes
  `QA\reports\autotest_walk.json`
- Smoke at 25 m/s: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools\run-player.ps1 -Name smoke25 -PlayerArgs "-autotest,-speed,25" -TimeoutMin 8`
- Soak through the ending: `powershell -NoProfile -File Tools\run-player.ps1 -Name soak -PlayerArgs "-soak,1800,-speed,2.5"`
- Performance at 4K: player arguments `-perf-poses KS17,ride,KS28 -perf-log QA\perf\final\perf4k.csv -perf-rt 3840x2160 -uncapped -nofade`
- Ending captures: `-capture QA\captures\<run> -capture-set ending`; the real end path: `-capture-set sequences -capture-only seq_end`
- Offline QA on a capture folder: `python -I Tools/qa/run_all.py QA/captures/<run>`
- Metrics: `python -I Tools/metrics.py --md QA/metrics.md --json QA/metrics.json`

---

## 8. Project metrics

Measured by `Tools/metrics.py` (MASTER_PROMPT 15.3), output in `QA/metrics.md` and `QA/metrics.json`. Prices from
https://platform.claude.com/docs/en/about-claude/pricing, accessed 2026-11-20 (`Tools/pricing.json`), for
`claude-fable-5-2`: input $4, 5-minute cache write $5, 1-hour cache write $8, cache read $0.20, output $20 per million
tokens; no long-context tier applies to this model.

| Metric | Value | How obtained |
|---|---|---|
| Run anchor | 2026-11-13T19:22:09+01:00 (`Run started at 2026-11-13T19:22:09.4636135+01:00`) | measured: line 1 of STATE.md; only records at or after it count |
| Sessions | 2 main sessions (<SESSION>, which wrote the anchor; <SESSION>, with the project root as cwd after the anchor) | measured |
| Transcript files (main / subagent / workflow) | 2 / 2 / 216 | measured: the `.jsonl` files of the accepted sessions |
| Assistant messages | 10,882 (deduplicated; 20,358 streamed records before deduplication, 10,891 API-request records, 25 synthetic records ignored) | measured: unique `message.id` with usage at or after the anchor |
| Models | claude-fable-5-2 (10,882 messages) | measured: `message.model` |
| Uncached input tokens | 22,170 | measured: sum of `usage.input_tokens` |
| Cache read tokens | 3,986,081,901 | measured: sum of `usage.cache_read_input_tokens` |
| Cache write tokens | 97,420,730 (5 min 91,447,214 / 1 h 5,973,516) | measured: `usage.cache_creation_input_tokens` and its ephemeral split |
| Output tokens (thinking included) | 14,476,893 | measured: sum of `usage.output_tokens` |
| Total tokens | 4,098,001,694 | measured: sum of the four categories |
| Wall-clock time | 168:23:10 | measured: from the anchor to the last included record, 2026-11-20T18:45:19 UTC (19:45 local) |
| Idle gaps over 10 minutes | 13 gaps, 140:35:48 in total | measured: consecutive timestamps (anchor included) more than 600 s apart |
| Active time | 27:47:22 | derived: wall-clock time minus all gaps over 10 minutes |
| Cost: uncached input | $0.09 | estimated: 22,170 x $4 / 1,000,000 |
| Cost: cache read | $797.22 | estimated: 3,986,081,901 x $0.20 / 1,000,000 |
| Cost: cache write, 5 min | $457.24 | estimated: 91,447,214 x $5 / 1,000,000 |
| Cost: cache write, 1 h | $47.79 | estimated: 5,973,516 x $8 / 1,000,000 |
| Cost: output | $289.54 | estimated: 14,476,893 x $20 / 1,000,000 |
| **Total API-equivalent cost** | **$1,591.87** | estimated: sum of the categories (not the user's subscription bill) |

**Cost formula.** cost (USD) = (input x 4 + cache_write_5m x 5 + cache_write_1h x 8 + cache_read x 0.20 + output x 20)
/ 1,000,000, with each token count in tokens and each price in USD per million tokens. The unrounded sum is $1,591.867;
the rounded category values add up to $1,591.88 because each is rounded to the cent. All messages, including the two
claude-fable-5-2 ones, are priced at the Claude Fable 5.2 rates of `Tools/pricing.json`.

**Idle-gap rule.** The script sorts the timestamps of all included records, with the anchor as the first one. Every
interval between consecutive timestamps longer than 10 minutes (600 s) counts as an idle gap. Active time is wall-clock
time minus the sum of those gaps. The 13 gaps (UTC, from `QA/metrics.md`):

| From (UTC) | To (UTC) | Length |
|---|---|---|
| 2026-11-13 19:17:33 | 2026-11-13 19:28:04 | 0:10:32 |
| 2026-11-13 23:04:30 | 2026-11-13 23:14:38 | 0:10:08 |
| 2026-11-14 09:12:03 | 2026-11-14 11:19:20 | 2:07:17 |
| 2026-11-14 12:29:51 | 2026-11-15 20:09:58 | 31:40:07 |
| 2026-11-15 20:19:21 | 2026-11-19 15:08:16 | 90:48:55 |
| 2026-11-19 15:14:54 | 2026-11-19 15:31:36 | 0:16:42 |
| 2026-11-19 19:03:34 | 2026-11-20 07:01:53 | 11:58:19 |
| 2026-11-20 07:15:34 | 2026-11-20 07:44:55 | 0:29:21 |
| 2026-11-20 07:45:50 | 2026-11-20 09:27:58 | 1:42:08 |
| 2026-11-20 09:30:00 | 2026-11-20 09:43:42 | 0:13:42 |
| 2026-11-20 09:47:08 | 2026-11-20 10:10:37 | 0:23:30 |
| 2026-11-20 13:09:23 | 2026-11-20 13:21:37 | 0:12:14 |
| 2026-11-20 18:20:34 | 2026-11-20 18:43:27 | 0:22:54 |

**What was not counted.**

- Everything after the last measured record (2026-11-20 19:45 local): writing the final documents, including this
  report, the final commit and the final message to the user.
- Records of the accepted sessions older than the anchor (the earlier game of a continued chat): 39,049 records, 8,266
  messages, 2,628,644,747 tokens.
- One session that only quotes the anchor line (it read STATE.md) and has no anchor write and no project cwd.
- The cost is an API-equivalent estimate for comparison, not what the user's subscription actually charged.

---

### 8.1 Final re-run (after the final documents, 2026-11-20 19:57 local)

`python -I Tools/metrics.py --md QA/metrics.md --json QA/metrics.json` was re-run after the documents were written; it replaces the numbers above (same method, same prices):

| Metric | Value | How obtained |
|---|---|---|
| Assistant messages (deduplicated) | 11,006 | measured |
| Uncached input tokens | 22,420 | measured |
| Cache read tokens | 4,006,188,179 | measured |
| Cache write tokens (5 min / 1 h) | 98,055,658 (92,072,731 / 5,982,927) | measured |
| Output tokens (incl. thinking) | 14,652,085 | measured |
| Total tokens | 4,118,918,342 | measured |
| Wall-clock time | 168:35:39 | measured (anchor 2026-11-13 19:22:09 local to the last record 2026-11-20 19:57:48 local) |
| Idle gaps > 10 min | 13 gaps, 140:35:48 | measured |
| Active time | 27:59:51 | derived: wall-clock minus the gaps |
| Cost | $1,602.60 (input $0.09 + cache read $801.24 + cache write 5 min $460.36 + 1 h $47.86 + output $293.04) | estimated at the official prices accessed 2026-11-20 |

Not counted: the final commit and the final chat message after the last record.

## 9. Sources

All paths are relative to the project root `<WORKSPACE>\4 Megastructures\4 Megastructures`:

- **Run history and the user's orders:** `STATE.md` (rules, the "FINISHED" note, running notes, the redesign order, the
  Digest, milestones).
- **Specification:** `MASTER_PROMPT.md` Parts 9, 12, 14.0, 14.1, 14.5, 14.10, 14.11, 15.
- **Kraken and ending design:** `docs/ADDENDUM_A1.md` sections 00, 1 and 3.
- **Decisions, rulings R1-R6, the X2 gate and the final lead rows:** `DEVELOPMENT.md`.
- **Commands and flags:** `docs/FRAMEWORK.md` section 5; `Tools/run-player.ps1`;
  `Assets/_Game/Scripts/Runtime/Core/CliArgs.cs`; `Assets/_Game/Scripts/Runtime/Core/BulwarkConst.cs` (ride constants);
  `Assets/_Game/Scripts/Runtime/Core/BwPath.cs` (`EndingPath`); `FrameworkConst.cs` (end sequence).
- **Builder reports:** `Logs/wbx_reports.json` (Wave X), `Logs/wbx2_reports.json` (Wave X2), `Logs/fix_x2_reports.json`
  (fix wave).
- **Critic panel:** `Logs/panel_x2.json` (six lenses plus the chief critic).
- **Final checks:** `Logs/lead_final.out`, `QA/tests/editmode_final.xml`; `QA/reports/autotest_cinematic.json`,
  `QA/reports/autotest_walk.json`, `Logs/fin-auto25.out`, `Logs/fin-auto15.out`; `QA/reports/soak.json`,
  `.worktrees/integ/QA/reports/leadqa/soak/soak.json`; `QA/perf/final/`; `QA/reports/leadqa/tprec_dither_*.json`;
  `QA/reports/leadqa/wallbot/wallbot_speed10.00.json`.
- **Captures and metrics:** `QA/captures/fin-stills`, `fin-ending`, `fin-show`, `fin-gallery`, `fin-end` (each with
  `report.json`); `QA/reports/image_metrics_fin-stills.md`, `image_metrics_fin-ending.md`; `QA/reports/sequences_x2-seq.md`,
  `sequences_x2-seq-hp.md`; `Logs/fin-end.out`; `QA/final/`; `QA/vram/vram_summary.log`.
- **Project metrics:** `Tools/metrics.py`, `Tools/pricing.json`, `QA/metrics.md`, `QA/metrics.json`.
- **Commits named:** 2689989, 49d1068, 86e5d47, 1a1407e, a29e2da, 145a648 (the recent-commit list of the repository).
