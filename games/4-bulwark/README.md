# Bulwark

**Created entirely with Claude Fable 5.2.**

Game 4 of the Megastructures series: a first-person walk along a 6 km sea dam, with a colossal kraken.
Unity 6000.6.0f1, URP 17.6, Direct3D 11, Windows. Product name `Bulwark`, player `Builds/Bulwark/Bulwark.exe`.

## Status (2026-11-20, final)

The game is finished for this run. `Builds/Bulwark/Bulwark.exe` is the final build. It was built from master after the
fix wave: integration commit `86e5d47`, final lead check `1a1407e`. Use it for the video.

Verified on that build:

| Check | Result | Source |
|---|---|---|
| Final lead check | RebuildAll OK (0 problems); `Validate` 78 checks / 0 problems; EditMode 382/382; player build OK (231.1 MB, 0 errors); smoke at 25 m/s PASS (trigger 205.10 s) | `Logs/lead_final.out`, `QA/tests/editmode_final.xml` |
| Cinematic autotest, 2.5 m/s | PASS: goal trigger 759.50 s (expected 759.50 +- 3), ending done 815.00 s, 7 beats, roll 0, 0 errors, VRAM peak 3944 MiB | `QA/reports/autotest_cinematic.json`, `Logs/fin-auto25.out` |
| Walking autotest, 1.5 m/s | PASS: trigger 1170.17 s, done 1225.67 s, 0 errors | `QA/reports/autotest_walk.json` |
| Performance, measured alone, uncapped | 4K: KS17 7.37 ms (136 fps), ride 5.96 ms (168 fps), KS28 7.48 ms (134 fps); 141 fps average over the run, 1 % low 89.9 fps, 0 frames over 33 ms. 1080p: 445 fps average, 1 % low 190 fps, 0 frames over 33 ms | `QA/perf/final/4k|1080/perf_poses.json`, `QA/perf/final/perf4k.csv`, `perf1080.csv` |
| Ending in the real game path (`seq_end`) | PASS: goal at frame 42; the stinger is drawn from t_g 0; the crest goes dark at t_g 19.00 | `QA/captures/fin-end/report.json` |
| Final 4K key shots | 32 stills in `QA/final/`. KS28 comes from the ending capture set | `QA/final/` |

A cinematic take runs **13:35 from the first black frame to the end of the ending**. You reach the lantern at **12:39.5**.

The soaks passed, but both ran before the final integration build: 1800 s at 2.5 m/s through the whole ending (on the
lead-QA builder's fix-wave build) and 900 s at 1.5 m/s (on the gate build `2689989`). See "Testing".

How the review went. A critic panel with six lenses and a chief reviewed the merged game before the fix wave
(`Logs/panel_x2.json`). It found 0 S1, 24 S2 and 36 S3 findings. Nine builders worked through them in the fix wave
(`Logs/fix_x2_reports.json`), and the lead fixed more in the integration round. Fixed:

- **Kraken eye (beat 6):** it now reads as an eye: a lens-shaped pupil, catchlights on the cornea, and no more "lips".
- **Window water:** it is a teal murk now, not aquarium blue.
- **KK4:** the sucker arm on the glass is back.
- **Stinger:** it is drawn, as a dark silhouette at `StingerScale` 3.5. Before the fix, `Beat08Ending.ApplySets`
  switched it off.
- **Falling water:** water runs off the kraken when it surfaces.
- **KS28, the final look back:** alpenglow at the top of the dam, trees at its foot, the elevator rail and the cabin
  light, and veils on the jets.
- **Shuttle:** it now runs 55 s (ruling R1).
- **K5:** the whip topples slowly.
- **Kraken speed:** every visible kraken motion is at most 8 m/s at 2.5 m/s.
- **Sound:** the kraken can be heard on small speakers.
- **Beat-6 gaze:** the camera holds on the eye.

What was not done:

- **No second critic round.** The user's speed order allowed one panel and one fix wave.
- **No perfection loop.** The loop needs two clean critic rounds, and it was not completed.

What is still open is listed under "Known limitations" below. This run's rulings are rows in `DEVELOPMENT.md`:

- R1-R6
- doorway ramps 2.6
- sea flicker high-pass
- KS07 gate house
- KS17 = `win_eye`
- `PendantY` 5.8
- the T-PREC limitation
- the ride door step

## What the game is

Bulwark is a linear first-person walk with no combat, puzzles or dialogue.

- **The setting.** A dam 6 km tall holds an ocean 8 m below its crest road. The sea is on the right (east). The dry
  lowland lies 6 km down on the left (west).
- **The light.** The weather clears as you walk: storm, sun break, cold noon, tungsten interiors, cyan-black windows
  under the sea, amber dusk.
- **The route.** You start in a wave gallery inside the dam. You walk the crest, cross a spillway bridge and pass the
  armour slope. You climb down a gate tower, walk a gallery of windows under the sea, and come out on a ledge on the dry
  side at dusk.
- **The kraken.** On the sea side a colossal kraken shows itself in stages, slowly, so its size sinks in.
- **The ending.** At the end of the ledge a glass express elevator runs down the dam face to the foot. A shuttle then
  carries you 3.5 km across the lowland to a lantern platform, and the final look back shows the whole dam.

Everything is procedural except the kraken. It is built from the user's two Tripo3D models
(`3D Models/SM_Creature_Octopus.glb`, `3D Models/SM_Creature_MonsterTentacles.glb`). That breaks the "no external assets"
rule on the user's order, and the exception is logged in `DEVELOPMENT.md` (row "2026-11-19 Wave A1 (user order)").

The game is made to be recorded: a cinematic autowalk at a constant 2.5 m/s walks the whole path by itself.

## How to run it

| Way | What to do |
|---|---|
| **Portable Windows build — one file** | Download [`Bulwark-Windows-x64.exe`](../../../../releases/download/bulwark-v1.0/Bulwark-Windows-x64.exe) and double-click it. No Unity, source download, installation or manual extraction is required. |
| Full launch guide | See [`HOW_TO_PLAY.md`](HOW_TO_PLAY.md) for requirements, controls, checksum verification and source setup in Russian and English. |
| Rebuilt player | After rebuilding from source, run `Builds\Bulwark\Bulwark.exe`, plus any flags below. `PLAY_WINDOWS.bat` is a convenience launcher for that folder build. |
| Through the QA wrapper | Run `Tools\run-player.ps1` (see "Testing"). It adds a window size, an absolute log file and a VRAM sampler. |

- The portable EXE automatically extracts the complete game to a temporary folder, starts it and removes the temporary files on exit.
- `Builds/` is in `.gitignore`; it is needed only when rebuilding from source, not when using the portable release.
- **Display and frame rate.** By default the player opens as a fullscreen window at the desktop resolution. Normal play
  runs with VSync on, at the monitor refresh. Every QA run is capped at 60 fps with VSync off (`QualityBootstrap.cs`).
- **Measured on the final build** (RTX 5070 12 GB, Ryzen 7 5700X3D, 1920x1080 window):
  - Load takes 6.22 s (5.96 s in the 1.5 m/s run).
  - VRAM peaks at 3944 MiB in the cinematic autotest and 2801 MiB in the 1.5 m/s autotest (`QA/vram/vram_summary.log`).
  - Rendered offscreen at 4K, uncapped, the three heaviest poses hold 134-168 fps.

## Controls (MASTER_PROMPT Part 9, checked against `Runtime/Player/BwInput.cs`)

| Action | Keyboard and mouse | Gamepad |
|---|---|---|
| Move | `W A S D` (or the arrow keys) | left stick |
| Look | mouse (raw input, no acceleration, Y not inverted) | right stick |
| Brisk walk (2.5 m/s instead of 1.5 m/s) | hold `Shift` | hold either shoulder button |
| Cinematic autowalk on / off | `C` | no binding |
| Pause overlay (`RESUME`, `RESTART`, `QUIT`) | `Esc` | `Start` |
| Menu navigation | `W`/`S` or Up/Down; `Enter`/`Space` or a mouse click | D-pad; the south button confirms; the east button resumes |
| Fullscreen window / window | `F11` or `Alt+Enter` | - |

- **Autowalk.** The cinematic autowalk starts from wherever you are. **Any other input cancels it and gives you
  control back:**
  - a key other than `C`, `F11`, `Esc`, `Alt`, `Enter`, `PrintScreen` or the Windows keys;
  - mouse movement above a small threshold, or a mouse button;
  - a stick or a gamepad button.
- **Opening.** The game starts in black. The first wave impact is heard at 0.5 s, and the picture fades in over
  1.0-3.5 s. The title "BULWARK" shows from 1.4 s to 6.0 s.
- **Controls hint.** A one-line hint appears at 6.5 s for 6 s and goes as soon as you move. It never appears in
  cinematic mode or in `-autotest` (`FrameworkConst.cs`).
- **Elevator and shuttle.** Movement input is ignored, and you can look around freely (`PlayerController.cs`).
- **After the goal.** The end camera takes over for the look back at the dam. Then the picture fades to black, one line
  of text appears ("Someone held the sea back, so the land could stay dry."), and the game stays on black. `Esc` still
  works.

## The walk: beats and timings

Times are from the start of the run. "s" is the distance along the path in metres; beyond s 1500 it continues as the
ending timeline (`EndingPath` in `Runtime/Core/BwPath.cs`).

- **The walked path (s 0-1500):** at 2.5 m/s the time is s / 2.5 + 0.25 s, because the 0.5 s start ramp loses 0.25 s.
- **The ride:** it runs on its own clock (2.5 m of s per ride second) at every walking speed, so it lasts 143 s at
  1.5 m/s and at 2.5 m/s.

All times marked "measured" come from the final autotests (`autotest_cinematic.json`, `autotest_walk.json`).

### Timeline at 2.5 m/s (the cinematic take)

| Starts at | What starts |
|---|---|
| 0:00 | Beat 1, wave gallery |
| 0:48.25 | Beat 2, crest |
| 2:48.25 | Beat 3, spillway bridge |
| 4:40.25 | Beat 4, armour slope |
| 6:08.25 | Beat 5, gate tower and descent |
| 7:08.25 | Beat 6, window gallery |
| 9:08.25 | Beat 7, sluice tunnel and lee ledge |
| 10:00.85 | Boarding the elevator (measured ride start) |
| 10:04.85 | Descent down the dam face |
| 11:19.85 | Transfer to the shuttle at the foot |
| 11:25.85 | Shuttle across the lowland |
| 12:20.85 | Arrival: the door opens |
| 12:23.85 | Platform walk (measured ride end) |
| **12:39.5** | **The lantern (goal trigger, measured); the look back begins** |
| 12:48.0 | The look-back frame (KS28) is reached |
| 13:19.5 | Fade to black |
| 13:25.0 | End line |
| **13:35.0** | **End of the ending (measured)** |

### Beats 1-7 (the walked path, s 0-1500)

| # | Beat | s (m) | Starts at 2.5 m/s (measured) | Starts at 1.5 m/s (measured) | What you see |
|---|---|---|---|---|---|
| 1 | Wave gallery | 0-120 | 0:00 | 0:00 | A sodium-lit gallery inside the dam. Storm waves boom against the wall and burst through the slits. A door opens onto the crest. Kraken K0 (heard only). |
| 2 | Crest (money shot 1) | 120-420 | 0:48.25 | 1:20.25 | The sea at road level on the right, the 6 km drop on the left. Kraken K1 and K2. |
| 3 | Spillway bridge (money shot 2) | 420-700 | 2:48.25 | 4:40.25 | Gates, the sheet of water, mist. The sun break and the rainbow over the mist river (KS08, s 604). Kraken K3. |
| 4 | Armour slope | 700-920 | 4:40.25 | 7:46.92 | 40 m armour units on six offshore islets; the 80 m gate tower. Kraken K4 and K5. |
| 5 | Gate tower and descent | 920-1070 | 6:08.25 | 10:13.58 | The atrium, then 270 steps (15 flights, 14 landings) down the tower past portholes. |
| 6 | Window gallery (money shot 3) | 1070-1370 | 7:08.25 | 11:53.58 | Glass panes 20 x 36 m with the sea behind them; the kraken encounter at the glass (K7). |
| 7 | Sluice tunnel and lee ledge | 1370-1500 | 9:08.25 | 15:13.58 | Thunder in the sluice tunnel, then the ledge on the dry side at dusk. The elevator cabin waits at the end. |

Sources:

- Beat entries: `QA/reports/autotest_cinematic.json`, `autotest_walk.json`.
- Beat names and s ranges: MASTER_PROMPT 14.1 and 14.5.
- The kraken: `docs/ADDENDUM_A1.md`.

### The kraken's appearances (cinematic 2.5 m/s)

Every kraken event is keyed to s, so it happens at the same place at any speed. If you stop, the current crest segment
finishes at the 1.5 m/s rate, and then the kraken waits deep until you reach the next one.

- **Sound comes first:** you hear each appearance 2-6 s before you see it (LEAD DECISION 3).
- **Speed limit:** every visible motion is at most 8 m/s at the cinematic speed (LEAD DECISION 2). `KrakenShowTests`
  measures a peak of 7.46 m/s.

Where the numbers come from:

- s values: `Runtime/Creature/KrakenDirectorChoreo.cs` (crest, reef, sound cues) and
  `Beats/Beat06_WindowGallery/Beat06Encounter.cs` (window gallery).
- Times: computed as s / 2.5 + 0.25.

| ID | s (m) | Time at 2.5 m/s | What happens |
|---|---|---|---|
| K0 | 58, 66 | 0:23, 0:27 | A call heard through the gallery wall. The lamps sway with a dam shudder. Sound only. |
| K1 "the islet that moves" | 120-214 | 0:48-1:26 | The voice comes at s 120. The top of the head breaks the surface about 1.16 km out at s 128.4 (0:52). It rises to 70 m above the sea by s 158 (1:03) and holds to s 164. Then it sinks and is under at s 196.4 (1:19). The eyes stay under water. |
| Kf | 236 | 1:35 | A far slap. Sound only. |
| K2 "taller than the tower" | 296-476 | 1:59-3:11 | The voice comes at s 301 (2:01). One arm is out of the sea from s 308 (2:03) and stands 178 m above it from s 374 (2:30), more than twice the height of the 80 m gate tower ahead. The key view KK1 is at s 378 (2:31). The arm holds to s 394 (2:38) and is under again at s 463.5 (3:06). |
| K3 "the shadow at the gates" | 452-516 | 3:01-3:27 | A dark mass and its wake surge towards the spillway gates in the slot between the piers over s 452-490 (3:01-3:16). One arm tip breaks the surface there, up to 14 m, over about s 483-503 (3:13-3:21). The shadow fades over s 502-516. |
| Kc | 655 | 4:22 | A far call. Sound only. |
| K4 "the ruler" | 712-905 | 4:45-6:02 | The voice comes at s 716 (4:47). The head rises behind the armour islets to 179 m above the sea at s 800 (5:20). The lids open over s 792-800 (5:17-5:20), and then one eye tracks the camera. An arm arches 80 m over an islet; it is above the water over s 783-866 (5:14-5:47). The head sinks from s 806 and is under at s 873 (5:49). |
| K5 "the slap" | 762-822 | 5:11-5:29 | A whip is out of the sea from s 778 (5:11) at the edge of the armour belt and rises to about 60 m. From s 786 it leans and slowly topples, like a colossal column, and slaps the sea at s 822 (5:29). The slap throws a white spray column. |
| K6 "the porthole" | 1008 | 6:43 | Sucker pops heard on the stairs. The visible porthole pass is not built. |
| K7 window gallery | 1104-1372 | 7:22-9:09 | See the next table. |
| Stinger | after the goal | see "The ending" | Seen from the lantern platform: three dark hooked tentacle columns rise slowly behind the crest of the dam (`StingerScale` 3.5, ruling R3). |

K7 in detail:

| s (m) | Time at 2.5 m/s | What happens |
|---|---|---|
| 1104 | 7:22 | A far voice through the glass. |
| 1122-1165 | 7:29-7:46 | "The light goes out." |
| 1150-1262 | 7:40-8:25 | A sucker arm is pressed on the glass. It comes in from the black over s 1150-1200, and lamp A lights it over s 1204-1245 (8:02-8:18). |
| 1228-1250 | 8:11-8:20 | The eyes approach in the dark. |
| 1252-1280 | 8:21-8:32 | The main eye holds abeam pane 6, 25.5 m off the glass, while the camera holds on it. The pupil narrows to a slit at s 1256 and blinks at s 1272 (8:29). |
| 1280-1372 | 8:32-9:09 | A slow withdrawal. The strike on the glass is heard at s 1300 (8:40). |

### The ending (LEAD DECISION 1, `docs/ADDENDUM_A1.md` section 00)

The walker is brought to rest in the elevator cabin at s 1500, and from there the ride runs on its own clock. ADDENDUM
A1 section 3 still describes an inclined lift on a buttress fin. Section 00 replaced it, and the fin is not built.

| Phase | s (m) | Duration | Starts at 2.5 m/s | Starts at 1.5 m/s |
|---|---|---|---|---|
| Board: the doors close and seal | 1500-1510 | 4 s | 10:00.85 (600.85 s, measured) | 16:41.25 (1001.25 s, measured) |
| Descent: the glass cabin runs vertically down the outside of the dry-side face, about 5.9 km (floor y -90 to -5999), peak 110 m/s; the sun sets during the descent | 1510-1697.5 | 75 s | 10:04.85 | 16:45.25 |
| Transfer: the cabin couples to the maglev shuttle at the foot | 1697.5-1712.5 | 6 s | 11:19.85 | 18:00.25 |
| Shuttle: 3500 m west across the lowland while the camera looks back at the receding wall (ruling R1: 20 s ramps, cruise 100 m/s, mean 5.0 m/s^2) | 1712.5-1850.0 | 55 s | 11:25.85 | 18:06.25 |
| Arrival: the door opens at the lantern platform | 1850.0-1857.5 | 3 s | 12:20.85 | 19:01.25 |
| Platform walk: 38.5 m to the Keeper's Lantern | 1857.5-1896.0 | 15.65 s at 2.5 m/s, 25.92 s at 1.5 m/s | 12:23.85 (ride end, measured) | 19:04.25 (ride end, measured) |
| **The lantern: goal trigger** | 1896.0 | - | **12:39.5 (759.50 s, measured)** | 19:30.17 (1170.17 s, measured) |
| Look back: the end camera (see below) | after the goal | 40 s | 12:39.5 | 19:30.17 |
| Fade 4 s, black 1.5 s, end line 2 + 6 + 2 s | - | 15.5 s | 13:19.5 | 20:10.17 |
| **End of the ending** | - | - | **13:35.0 (815.00 s, measured)** | 20:25.67 (1225.67 s, measured) |

The end camera, on the end clock t_g (seconds since the goal). Times are for 2.5 m/s:

| t_g (s) | Time | What happens |
|---|---|---|
| 0 | 12:39.5 | The walker stops 1.5 m before the rest point, 1.9 m short of the lantern. The stinger columns are already drawn behind the crest. |
| 1.5-8.5 | 12:41.0-12:48.0 | The camera tilts from level up to pitch 29 deg, facing the dam: the final look back (KS28), with the foot and the crest both in frame. |
| 3.5, 16, 28 | 12:43.0, 12:55.5, 13:07.5 | The stinger calls. |
| 6-30 | 12:45.5-13:09.5 | The stinger columns rise slowly, about 440 m each. |
| 9-23 | 12:48.5-13:02.5 | The sun sinks after the goal. The crest goes dark at t_g 19.0 (12:58.5). |
| 18-28 | 12:57.5-13:07.5 | A slow tilt up by 7 deg, to pitch 36. The stinger's hooked tops stand above the crest. |
| 28-40 | 13:07.5-13:19.5 | Hold. |
| 40-55.5 | 13:19.5-13:35.0 | Fade, black, the end line. |

Sources:

- `Runtime/Core/BwPath.cs` (`EndingPath`) and `BulwarkConst.cs` (ride times, `EndLookPitch`, `EndLiftStart`,
  `EndSunSink*`).
- `FrameworkConst.cs`: the end sequence is 40 + 4 + 1.5 + 2 + 6 + 2 = 55.5 s.
- `KrakenDirectorChoreo.cs`: `StingerCallTgs`, `StingerRise`.
- `DEVELOPMENT.md`: rows "Fix wave X2 lead" (R1, R2, R3) and "Wave X2 ending".

## How to record the video

1. **Use the final build.** That is `Builds\Bulwark\Bulwark.exe`. A fresh clone must build it first.
2. **Start from the command line.** Run `Builds\Bulwark\Bulwark.exe -cinematic` (or `PLAY_WINDOWS.bat -cinematic`). The
   autowalk then begins at s 0 and the controls hint never shows. Pressing `C` also works, but it starts from wherever
   you are.
3. **Resolution.** Record at 1440p or higher and upload at 1440p or higher (MASTER_PROMPT 3.2 #6). The look is judged as
   4K 60 fps footage (Part 1.4).
   - The game opens fullscreen at the desktop resolution, so a 1440p or 4K desktop gives a 1440p or 4K picture.
   - To set the size yourself, use the Unity flags `-screen-width W -screen-height H -screen-fullscreen 0|1`, or
     `-res WxH` for a window of that size.
   - The heaviest poses render at 134-168 fps at 4K uncapped on the RTX 5070, so 4K60 has headroom on that machine.
   - The game supports 16:9, 16:10 and 21:9 with Hor+ field of view (Part 8.10). 16:9 is the reference. The final
     still set includes 16:10 and 21:9 variants of every key shot. A few of them fail a metric (see "Known limitations").
4. **Frame rate.** Normal play runs with VSync on, at the monitor refresh. Record at 60 fps.
5. **Do not touch the mouse or keyboard during the take.** Any input cancels the autowalk. `Esc` does not cancel it, but
   it opens the pause overlay.
6. **Length.** A full take runs 815 s (13:35) from the first black frame, plus about 6 s of loading before it. You reach
   the lantern at 12:39.5. After the end line the game stays on black; press `Esc` and choose `QUIT`.
7. **Keep the default picture settings.** The single static blue-noise dither (`-dither static`, the default) stops
   banding after YouTube compression. Do not record with `-dither off`, `-dither-lsb 0`, `-grade-off` or `-nofade`.
   `-dither temporal` is only for high-bitrate cinematic captures (`CliArgs.cs`).
8. **Audio.** The game generates its audio live, so record the system output.
   - The fix wave gave the kraken a layer for small speakers, a louder outdoor mix, and a stinger sound of up to 30 s.
     Two of the stinger fixes are listed under "Known limitations".
   - The checks are measurements (a small-speaker band gate). The audio builder still asks for a human listening pass
     on laptop and phone speakers, and none is recorded.
9. **ffmpeg.** If you convert frames or RGB footage with ffmpeg, use the swscale flags `accurate_rnd+full_chroma_int`.
   The default RGB to YUV conversion darkens the picture by about one level (MASTER_PROMPT 3.2 #6).
10. **Thumbnails and stills.** Use the final 4K key shots in `QA/final/`.

## Command-line flags

All flags are read by `Runtime/Core/CliArgs.cs` or by the code named in the table. `-autotest`, `-wallbot`, `-soak`,
`-perf-poses` and `-capture` start a QA run: 60 fps cap (unless `-uncapped`), VSync off, report JSON.

### Unity player flags (used by `Tools/run-player.ps1`)

| Flag | Meaning |
|---|---|
| `-screen-width W` `-screen-height H` | Window or screen size. |
| `-screen-fullscreen 0\|1` | Windowed or fullscreen. |
| `-logFile <absolute path>` | Where Player.log goes. |
| (never) `-batchmode`, `-nographics` | Never use these for anything that renders (MASTER_PROMPT 3.2 #9). |

### Playing and recording

| Flag | Meaning |
|---|---|
| `-cinematic` | Start the cinematic autowalk (constant 2.5 m/s). |
| `-res WxH` | Open a window of that size. |
| `-beat N` | Start at the start of beat N (1-7). |
| `-progress X` | Start at s = X metres (up to 1500). |
| `-ride-s S` | QA: start inside the ending at s = S (above 1500; 1500.01 = the ride). |
| `-gaze forward` | The autowalker looks along the heading at pitch 0 instead of using the directed gaze. |
| `-seed N` | Random seed for audio, sway and placement (default 1). |
| `-nofade` | Skip the title fade (QA only). |
| `-uifont theme\|legacy` | UI font (default `theme`; `legacy` = LegacyRuntime.ttf). |
| `-dither static\|temporal\|off` | The single grade dither. `static` is the default; `temporal` is only for high-bitrate cinematic captures; `off` is the undithered baseline for the encode test. |
| `-dither-lsb X` | Override the per-beat dither peak in 8-bit codes (0 = off). |
| `-freeze-dither` | Alias of `-dither static`. |
| `-freeze-ambient` (alias `-freeze-clock`) | Freeze the ambient clock. |
| `-clock T` | Start the ambient clock at T seconds. |
| `-grade-off` | Disable the grade (diagnostics only). |
| `-quit-after S` | Quit after S seconds of real time (exit 1 if a QA runner had not finished). |

### Test walks and runners

| Flag | Meaning |
|---|---|
| `-autotest` | Run the autowalker from s 0 to the goal at 1.5 m/s (2.5 m/s with `-cinematic`), with assertions and an exit code. Report: `autotest_walk.json`, `autotest_cinematic.json` or `autotest_speed<X>.json`. |
| `-brisk` | Test walk at 2.5 m/s (test walks only; the cinematic is always 2.5). |
| `-speed X` | Test-walk speed in m/s (test walks only), e.g. 25 for a quick smoke. |
| `-wallbot` | Wall-hugging bots on both lane edges, 0.40 m inside. |
| `-soak S` | Soak test of S seconds. It walks at 1.5 m/s unless `-brisk` or `-speed` is given. |
| `-report-dir <dir>` | Where QA runners write their JSON (default: next to Player.log). |
| `-perf-log <file>` | Write a frame-time CSV (at the 60 fps QA cap). |
| `-perf-poses a,b,c` | Measure fixed poses (the final runs used `KS17,ride,KS28`): 2 s warm-up and 6 s of measurement each, written to `perf_poses.json`. |
| `-uncapped` | With `-perf-log` only: measure uncapped (VSync 0, no target). |
| `-perf-rt WxH` | Render offscreen at WxH for the perf log (a 4K budget on a smaller monitor). |
| `-perf-capacity N` | Row capacity of the perf log. |
| `-audio-dump <file>` | Dump the real audio output to WAV; `-audio-dump-seconds N` sets the length (default 30). |
| `-tier-override c` | Force every DetailTier to coverage c (0 far to 1 near). |
| `-atmo-s S` | QA: evaluate the atmosphere schedule at s = S whatever the camera s (logged as a warning). |

### Captures

| Flag | Meaning |
|---|---|
| `-capture <dir>` | Run the capture harness; PNGs and `report.json` go to `<dir>`. |
| `-capture-set <list>` | One or more sets, listed below. |
| `-capture-only a,b` | Only these shots or sequences. Pass the comma list as ONE argument (through `-ArgString` in run-player). |
| `-capture-4k` | Also capture the 3840 x 2160 variant of every key shot. |
| `-seq-res WxH` | Resolution of sequence frames. |
| `-capture-warmup N`, `-capture-settle N`, `-capture-threads N`, `-capture-queue-mb N`, `-capture-flip` | Harness tuning. |

The sets for `-capture-set`:

- **Stills:** `keyshots` (the default; always includes the 4K variant), `beats`, `parity`; `stills` = parity, keyshots
  and beats.
- **Sequences:** `sequences` = `transitions`, `statics`, `pans`, `edges`, `ride` and `kseq`. `ride` holds the real ride
  sequences, `kseq` holds the kraken sequences K1-K7, and `edges` includes `seq_end`.
- **T-PREC:** `tprec`, the precision clips.
- **Bundles:** `all` (stills and sequences); `everything` (all plus `encode`); `encode` on its own.
- **Materials:** `materials`, `matcontext`.
- **QA rig sets:** `show` (kraken choreography), `ending` (the ending stills at 16:9, 16:10 and 21:9), `kraken` (the
  look-dev rig), `mistqa`.
- **Free poses:** `pose:NAME:x:y:z:yaw:pitch:fov`.

### Kraken, sea, mist, audio and window-gallery QA (owners' tools)

| Flag | Meaning |
|---|---|
| `-show-off` | No kraken at all (A/B renders without the creature). |
| `-show-end-t T` (alias `-end-t T`) | Set the stinger's end clock in captures. |
| `-show-stinger` | Force the stinger on. |
| `-show-4k` | Add 4K variants of the `show` stills. |
| `-kraken-aspects`, `-kraken-4k`, `-kraken-lod N`, `-kraken-perf <station>`, `-kraken-off`, `-kraken-nodist` | Options for the kraken look-dev rig (`-capture-set kraken`), in order: extra aspects; 4K; a forced LOD; a held perf pose; the kraken built but not drawn (perf baseline); no sea or mist disturbances. |
| `-sea-dist-qa near\|far\|kraken` | Sea-disturbance QA writer (the kraken director then publishes nothing). |
| `-mist-qa-sea <mode>` (`static`, `cycle`, `k5`, `k45`, `kall`, `kcycle`), `-mist-qa-t0 T`, `-mist-qa-cascade c` | Mist spray QA writer. |
| `-audio-qa kraken\|ride\|all` | Publish test kraken calls and a synthetic ride for the audio. Never use it together with the real game writers. |
| `-b6-debug`, `-b6-debug-motion` | Log the window-gallery staging and call times. |
| `-b6-nokraken`, `-b6-noscatter` | Window gallery without the creature / without the water in-scatter pass. |
| `-b6-armh`, `-b6-plane`, `-b6-eyescale`, `-b6-lampgain`, `-b6-lampgaina`, `-b6-lampgainb`, `-b6-lampknee`, `-b6-lampslope`, `-b6-lampayaw`, `-b6-lampapitch` | Window-gallery look-dev overrides (no rebuild needed). |
| `-b1-breath-qa [X]` | Force the beat-1 slit breath for a capture. |

## Testing (PowerShell, from the project root)

The project path has spaces. Call the scripts from a PowerShell session with `powershell -NoProfile -ExecutionPolicy
Bypass -File ...`, not through `Start-Process -ArgumentList`, which splits quoted paths. In `run-player.ps1`,
`-PlayerArgs` takes one comma-joined string. A comma list such as `-capture-only KS04,KS08` or `-perf-poses
KS17,ride,KS28` must therefore go through `-ArgString` instead.

```powershell
Set-Location "<WORKSPACE>\4 Megastructures\4 Megastructures"
$rp = "Tools\run-player.ps1"

# autotests (14.10.4)
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name autotest25 -PlayerArgs "-autotest,-cinematic" -TimeoutMin 25
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name autotest15 -PlayerArgs "-autotest" -TimeoutMin 30
# quick smoke of the whole path at 25 m/s
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name smoke25 -PlayerArgs "-autotest,-speed,25" -TimeoutMin 8
# wall bot; soak through the whole ending at 2.5 m/s
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name wallbot -PlayerArgs "-wallbot"
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name soak -PlayerArgs "-soak,1800,-speed,2.5" -TimeoutMin 45
# key shots (4K variants included), selected shots, the kraken show set, the ending set, seq_end alone
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name keys -ArgString "-capture QA\captures\keys01 -capture-set keyshots"
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name ks2 -ArgString "-capture QA\captures\ks2 -capture-only KS04,KS08"
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name show -ArgString "-capture QA\captures\show01 -capture-set show"
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name ending -ArgString "-capture QA\captures\end01 -capture-set ending"
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name end -ArgString "-capture QA\captures\end02 -capture-set sequences -capture-only seq_end"
# performance at the three heaviest poses, 4K offscreen and 1080p, uncapped
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name perf4k -ArgString "-perf-poses KS17,ride,KS28 -perf-log QA\perf\perf4k.csv -perf-rt 3840x2160 -uncapped -nofade -report-dir QA\perf\4k"
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name perf1080 -ArgString "-perf-poses KS17,ride,KS28 -perf-log QA\perf\perf1080.csv -uncapped -nofade -report-dir QA\perf\1080"
# audio dump of a cinematic run
powershell -NoProfile -ExecutionPolicy Bypass -File $rp -Name dump -PlayerArgs "-cinematic,-audio-dump,QA\audio\dump.wav,-audio-dump-seconds,60"

# offline QA (python -I; numpy + Pillow; ffmpeg on PATH for the encode test)
python -I Tools/qa/run_all.py QA/captures/keys01
python -I Tools/qa/contact_sheet.py QA/captures/keys01
python -I Tools/qa/selftest.py
# project metrics (Part 15.3)
python -I Tools/metrics.py --md QA/metrics.md --json QA/metrics.json
```

About `run-player.ps1`:

- **Options.** The default window is 1920x1080 (`-Width` and `-Height` change it). `-BuildDir Builds\<owner>` or `-Exe`
  picks another build. `-DryRun` prints how the arguments were parsed and starts nothing.
- **Launching.** Since 2026-11-20 (the user's "no queues" order) it launches long jobs directly; the `gpu-guard` route
  is disabled. It still waits (then exits 95) while too many of our own renderers run: at most 3 at once, or 2 when one
  is 4K.
- **Results.** Exit code 98 means the player was not found. Otherwise the result is the player's exit code combined
  with the `pass` field of every QA JSON the run wrote; any `pass: false` gives exit 1. The log scan counts real errors
  only (`[BW-FAIL]`, exceptions, shader errors, device removal), never a summary line such as "errors=0".

### Final results

| Run | Result | Source |
|---|---|---|
| Lead check | RebuildAll OK, 0 problems. `Validate` 78 checks / 0 problems. EditMode 382/382. Build OK: 231.1 MB, 0 errors, 4 warnings. Smoke at 25 m/s PASS: trigger 205.10 s, done 260.60 s, 0 errors. | `Logs/lead_final.out`, `QA/tests/editmode_final.xml`, `QA/reports/autotest_speed25.00.json` |
| Autotest 2.5 m/s (cinematic) | PASS. Goal 759.50 s (expected 759.50 +- 3); done 815.00 s; 7 beats in order. Ride 600.85-743.85 s (143.00 s), cabin peak 110 m/s. Progress speed 2.5000 m/s; ground speed peak 2.597 m/s; roll 0. 0 errors, 0 warnings, 0 camera-probe violations, 0 coverage misses. Load 6.22 s; VRAM peak 3944 MiB. | `QA/reports/autotest_cinematic.json`, `Logs/fin-auto25.out` |
| Autotest 1.5 m/s | PASS. Goal 1170.17 s; done 1225.67 s; ride 1001.25-1144.25 s; 0 errors; VRAM peak 2801 MiB. | `QA/reports/autotest_walk.json` |
| Perf, 4K offscreen, uncapped, measured alone | PASS. KS17 7.37 ms (136 fps), ride 5.96 ms (168 fps), KS28 7.48 ms (134 fps). Over the run: 141 fps average, 1 % low 89.9 fps, 0 frames over 33 ms. | `QA/perf/final/4k/perf_poses.json`, `QA/perf/final/perf4k.csv` |
| Perf, 1080p, uncapped, measured alone | PASS. KS17 3.45 ms, ride 1.63 ms, KS28 2.24 ms. Over the run: 445 fps average, 1 % low 190 fps, 0 frames over 33 ms. | `QA/perf/final/1080/perf_poses.json`, `QA/perf/final/perf1080.csv` |
| Soak 1800 s at 2.5 m/s | PASS: managed memory -5.17 MB, total +19.75 MB, 0 errors, 2 rides through the whole ending. This ran on the lead-QA builder's fix-wave build (before R1 was merged: goal at s 1846, loop 739 s). The soak does not judge frame time: it logged 528 frames over 33 ms, which the perf runs measured alone do not show. | `.worktrees/integ/QA/reports/leadqa/soak/soak.json` (copy in `QA/reports/leadqa/soak/`) |
| Soak 900 s at 1.5 m/s | PASS: managed memory -0.93 MB, 0 errors. This ran on the gate build `2689989` and ends at s 1349.7, before the ending. | `QA/reports/soak.json` |
| Wall bot at 10 m/s | PASS: 0 failures, 0 errors, on the lead-QA fix-wave build. | `QA/reports/leadqa/wallbot/wallbot_speed10.00.json` |
| Frame sequences | 37 captured. Transitions and pans are clean. The static sea sequences pass under the lead ruling (block series high-passed above 1 Hz): 1.97 / 2.47 / 0.60 / 1.28 levels against a limit of 3, 0 fireflies. | `QA/reports/sequences_x2-seq-hp.md` |
| `seq_end` in the real end path, final build | PASS. Trigger at frame 42. The stinger is drawn from t_g 0; it is above the crest in all 372 KS29 frames, with 33.2 levels of contrast (limit 6). The crest goes dark at t_g 19.00 (19 +- 0.5). 0 failed frames; the harness logs 19 difference spikes. | `QA/captures/fin-end/report.json` |
| Image metrics, final build | `fin-stills`: 198 images, 38 failing (22 NaN-like pixels, 9 mean luma, 7 fireflies). `fin-ending`: 48 images, 11 failing (6 NaN-like pixels, 4 fireflies, 1 mean luma). | `QA/reports/image_metrics_fin-stills.md`, `image_metrics_fin-ending.md` |

## Rebuild from source

Requirements:

- Windows.
- Unity 6000.6.0f1 at `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe` (the path is fixed in
  `Tools/unity-queue.ps1`).
- URP 17.6.0 and Input System 1.20.0 come from `Packages/`.
- Python 3 for the tools: numpy and Pillow for `Tools/qa`, and ffmpeg on PATH for the encode test.
- The fast type check uses the dotnet SDK that ships inside the Unity install.

Every Unity batch call goes through `Tools/unity-queue.ps1`. It runs one Unity at a time per project folder. It refuses
to start while an interactive Editor has the project open. It writes `Logs/<Name>.log` with a digest of errors and
`[BW-FAIL]` lines.

```powershell
# Run from the repository root.
Set-Location "games\4-bulwark"
$uq = "Tools\unity-queue.ps1"

# 1. fast type check (about 15 s; needs one prior Unity compile; blind to asmdef references and shaders)
python -I Tools/fastcompile.py all
python -I Tools/gen_hlsl.py --check
# 2. regenerate everything: project settings, HLSL, materials, Main.unity (idempotent)
powershell -NoProfile -ExecutionPolicy Bypass -File $uq -Name rebuild -Method Megastructure.EditorTools.BatchTools.RebuildAll -TimeoutMin 30
# 3. validate (78 checks, 0 problems on the final lead check)
powershell -NoProfile -ExecutionPolicy Bypass -File $uq -Name validate -Method Megastructure.EditorTools.BatchTools.Validate
# 4. EditMode tests (absolute results path; 382/382 on the final lead check)
$res = (Resolve-Path QA\tests).Path + "\editmode.xml"
powershell -NoProfile -ExecutionPolicy Bypass -File $uq -Name tests -NoQuit -ExtraArgs "-runTests,-testPlatform,EditMode,-testResults,$res"
# 5. player build -> Builds\Bulwark\Bulwark.exe (add -BuildDir Builds\<owner> for another folder)
powershell -NoProfile -ExecutionPolicy Bypass -File $uq -Name build -Method Megastructure.EditorTools.BatchTools.BuildWindows
# 6. smoke the build (expect trigger 205.10 s)
powershell -NoProfile -ExecutionPolicy Bypass -File Tools\run-player.ps1 -Name smoke25 -PlayerArgs "-autotest,-speed,25" -TimeoutMin 8
```

- **Kraken assets.** The game assets generated from the two GLB files are committed
  (`Assets/_Game/Creature/Kraken/Resources/Kraken/`). Re-run the pipeline only if the sources in `3D Models/` change:
  `Tools\.venv\Scripts\python.exe -I -u Tools/kraken/build_kraken.py --redo` (caches in `Library/KrakenCache`).
- **More commands** (PlayMode tests, the look-dev loop, the encode test, per-owner builds): see `docs/FRAMEWORK.md`
  section 5 and the "Commands" section of `STATE.md`.

## Where QA outputs live

| Path | Contents | In git |
|---|---|---|
| `QA/final/` | **The 32 final 4K key shots** (KS01-KS26b, KK1, KK2, KK4 from the key-shot set; KS28 from the ending set). Use these for thumbnails. | yes |
| `QA/reports/` | Runner JSON (`autotest_cinematic.json`, `autotest_walk.json`, `autotest_speed25.00.json`, `soak.json`, `wallbot_*.json`) and offline reports (`image_metrics_<run>.md/.json`, `sequences_<run>.md/.json`, `audio_*.md/.json`) | yes |
| `QA/reports/leadqa/` | Lead-QA runs of the fix wave: the 1800 s soak, smoke, wall bot, the perf runs, T-PREC (`tprec_dither_*.json`) | yes |
| `QA/captures/<run>/` | PNG stills, `report.json`, sequences in `seq_<name>/` folders. Final-build sets: `fin-stills` (198 stills, including 4K, 16:10 and 21:9), `fin-ending` (48), `fin-show` (25 stills and 9 sequences), `fin-gallery` (20), `fin-end` (`seq_end`). Merged-game sequences: `x2-seq`. | no (large) |
| `QA/perf/final/` | Final frame-time CSVs, summaries and `perf_poses.json` (4K and 1080p) | yes |
| `QA/contact/` | Contact sheets (`Tools/qa/contact_sheet.py`) | yes |
| `QA/tests/` | EditMode and PlayMode result XML (`editmode_final.xml` = the final lead check) | yes |
| `QA/vram/` | `vram-<Name>.csv` per player run (nvidia-smi every 5 s) and `vram_summary.log` | yes |
| `QA/audio/` | Example WAVs (not in git) and their analysis JSON | JSON only |
| `QA/encode/` | Encode-test material | no |
| `QA/metrics.md`, `QA/metrics.json` | Time, tokens and API-equivalent cost (Part 15.3) | yes |
| `Logs/` | `player-<Name>.log` (the Player.log of every run-player run); Unity batch logs `<Name>.log`; `lead_final.out`; the critic panel `panel_x2.json`; fix-wave reports `fix_x2_reports.json` and tasks `fix_x2/<role>.md` | no |

## Known limitations (final build)

Honest list of what is below spec or not built:

- **Precision at the lantern platform (T-PREC).**
  - The cause: there is no floating origin. ADDENDUM A1 3.8 (`BwFrame`) was not built.
  - What was measured: a static camera shows 0 px of jitter. In a 0.1 px/frame pan the jitter reaches 0.52 px at most,
    against 0.2 px near the origin. That is at most a sub-pixel edge crawl, because the end camera is static or tilts
    slowly.
  - Where it is recorded: `DEVELOPMENT.md` row "Final lead", and `QA/reports/leadqa/tprec_dither_*.json`.
- **KS19 grating fireflies.** The metric flags 66-432 isolated pixels at the conduit behind the bars of the sluice
  grating.
- **Single NaN-like pixels.** These are distant lamp cores in a few stills: 22 of the 198 `fin-stills` and 6 of the 48
  `fin-ending` images.
- **Mean luma.** Nine `fin-stills` images fall outside their mean-luma band. Six are beat-sweep stills (`end`,
  `look_down`, `look_r`); the other three are the KS09 21:9, KK2 16:10 and KS26b 16:10 variants.
- **The stinger is small.**
  - It reads as three dark hooked tentacles about 75 px above the crest. It does not fill the frame.
  - During the KS28 hold (pitch 29, frame top 62 deg) the hooks sit at the top edge of the frame. They come fully into
    view with the tilt-up at t_g 18-28 (`KrakenDirectorChoreo.StingerH0`).
  - The stinger sound does not follow the picture fade.
- **Stills of KS27-KS29.** The key-shot set's stills lack the new end clock, so `QA/final` takes KS28 from the ending
  capture set.
- **Not built.**
  - Kraken K6: the visible porthole pass (its sound is in).
  - Kraken K4a.
  - The beat-4 horns: the armour reef uses real units clipped at y -13 instead of the specified horn pyramids (14.5.4).
  - The outlet basin at the dam foot.
- **Cyan suckers.** In the window gallery the suckers glow cyan under the lamps.
- **Review loop.** The perfection loop (two clean critic rounds) was not completed. Only one panel and one fix wave ran.
- **Open items from the fix-wave builders** (`Logs/fix_x2_reports.json`; not re-checked on the final build):
  - The K4 arch flanks are still reddish.
  - KK2 shows one main eye.
  - The K3 wake is faint (the pier slot is 10.7 levels darker with the kraken, but the V-wake has under 20 levels of
    contrast).
  - In KS28 a step of about 8.5 levels remains where the dam foot meets the ground.
  - The K5 slap clears the K4 sink roar by only +3.8 dB.
  - No human listening pass has been done.

## Project metrics (`Tools/metrics.py`, `QA/metrics.md`)

Prices are from https://platform.claude.com/docs/en/about-claude/pricing, accessed 2026-11-20: input $4, 5-minute cache
write $5, 1-hour cache write $8, cache read $0.20, output $20 per million tokens. There is no long-context tier.

| Metric | Value |
|---|---|
| Run anchor | 2026-11-13T19:22:09+01:00 |
| Transcripts | 2 main sessions (`<SESSION>`, `<SESSION>`), 2 subagent and 216 workflow transcripts |
| Assistant messages (deduplicated) | 10,882 |
| Tokens | uncached input 22,170; cache read 3,986,081,901; cache write 97,420,730 (5 min 91,447,214 / 1 h 5,973,516); output 14,476,893; total 4,098,001,694 |
| Time | wall-clock 168:23:10; 13 idle gaps over 10 min totalling 140:35:48; active 27:47:22 |
| API-equivalent cost | $1,591.87 (input $0.09, cache read $797.22, cache write 5 min $457.24, cache write 1 h $47.79, output $289.54) |

Not counted: the final documents and the closing message, written after the last measured record (2026-11-20 19:45
local).

## Documents

| File | What it holds |
|---|---|
| `STATE.md` | Run state, rules for the next agent, the user's redesign order, the digest, resume notes, working commands |
| `DEVELOPMENT.md` | Environment, architecture, ADRs, and every decision and ruling (the Tripo3D exception, rulings R1-R6, the T-PREC limitation, the final 4K shots), calibrations |
| `docs/ADDENDUM_A1.md` | The kraken and the new ending. Section 00 (lead decisions) wins over the rest of the addendum and over MASTER_PROMPT Part 14 |
| `docs/FRAMEWORK.md` | Where every framework piece came from, the frame order, how to add a beat, all commands |
| `MASTER_PROMPT.md` | The original specification (Part 9 controls, Part 14 the game, Part 15 definition of done) |
| `docs/RESEARCH.md`, `docs/MATERIALS.md`, `docs/IMPORT_MANIFEST.md` | Research log, material notes, framework import list |
| `FEATURE_MATRIX.md`, `CLAUDE_FABLE_5.2_FINAL_REPORT.md` | Beat-by-beat status, and the final report (Part 12) |
