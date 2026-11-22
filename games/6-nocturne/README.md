# Nocturne (Megastructures, game 6)

Nocturne is game 6 of the Megastructures series: a first-person walk of 1 500 m through a rain-soaked night megacity that
has no ground. The walk has seven beats (alley, boulevard canyon, the Edge, skybridge, hanging garden, wall of signs and
Heart Square). It lasts 600 s at the cinematic 2.5 m/s and about 1 000 s at the walking 1.5 m/s, and ends at one warm
window under a 900 m hologram koi. A horror and megalophobia layer (D-034, `docs/DREAD.md`) runs through the walk. Its
centre is the Ark (D-043): a 5 km ship, 438 m wide, that comes slowly out of the haze down the rift, passes over the
walker on the skybridge, and leaves through the haze again. A 900 m shadow passes under the bridge in the abyss haze, a
12 x 8 m sign panel tears off and falls without an impact sound, lift cabins ride the Gulf faces, a darkness wave runs
down a tower, an enormous eye opens on the wall of signs, and the koi's eye follows the camera. The
geometry, shaders, score and soundscape are procedural, built in Unity 6000.6.0f1 with URP 17.6 (Render Graph, Forward+,
MSAA 4x, HDR, D3D11). The only imported assets are CC0 textures (D-027, `docs/TEXTURES.md`). There are no people, no
real text, no shadow maps and no flashing. The game was made for a YouTube video.

**Licensing:** [MIT for code](../../LICENSE), [CC BY 4.0 for original assets](../../LICENSE-ASSETS.md), [third-party notices](../../THIRD_PARTY_NOTICES.md). The imported CC0 texture registry is in [ASSET_NOTICES.md](ASSET_NOTICES.md).

**Status (2026-11-21 23:35): finished, including the Ark wave (D-043); the known issues below are accepted for this
release (D-042).**
- How to get the game running, from a download or from these sources: **[HOW_TO_PLAY.md](HOW_TO_PLAY.md)**.
- The player `Builds/Nocturne/Nocturne.exe` was built from the Ark merge (948c767) with 0 errors and passed the Ark
  final gate (table below).
- The Ark wave (the user's order of 2026-11-21 evening, D-043) replaced the earlier 1 800 m hull with the Ark (5 000 m
  long, 438 m beam, keel at y 330, its back in the cloud deck): its lights appear far up the rift at the Edge, the bow
  passes over the walk at s 596, the walker crosses the skybridge under its belly, and the stern leaves at s 1 055.7.
  The same wave moved the abyss traffic at least 218 m away from the walk, so no vehicle passes close to the walker on
  the skybridge as a laser-like streak (longest vehicle streak 64 px at 60 fps over s 530-900).
- Look v2 (the user's directive D-027) is in: a dark neutral palette with no violet, and real textures on every building,
  window and surface. The D-034 horror and megalophobia layer is built, in picture and in sound.
- One six-lens critic panel (scale / wow, defects, matter / light, motion / pacing, audio, spec) plus a chief critic
  judged the merged game (run p4_01). It found 0 S1, 29 S2 and 24 S3 issues (C01-C53, `Logs/fix/_summary.md`). Its wow
  audit failed: KS04, KS05, KS06, KS10 and KS17 failed; KS09, KS18 and KS19 passed.
- One fix wave of twelve roles and one three-agent integration round fixed most of the findings. No second panel was
  run (the user's run shape has one), so no critic has judged the game after the fixes and the wow audit was not repeated.
  MASTER_PROMPT 15.1 item 2 (two clean critic rounds in a row and a passed wow audit) is therefore not met.
- `FEATURE_MATRIX.md` has the beat-by-beat status. `FINAL_REPORT.md` has the full report and the metrics.

### Ark final gate (2026-11-21 22:12-23:21, build of the Ark merge 948c767)
| Gate | Result |
|---|---|
| fastcheck (C#) | OK, runtime 221 / editor 297 files |
| Validate | SUCCESS, 0 problems |
| EditMode | 528 tests: 527 passed, 0 failed, 1 skipped (explicit search test) |
| Build | 0 errors (player folder 464.3 MB) |
| Smoke (brisk to s 400) | PASS |
| Autotest cinematic 2.5 m/s | PASS: goal trigger 599.050 s, stop 601.450 s (expected 599.05 / 601.45), ending 17.00 s, 7 beats, 0 errors |
| Autotest walk 1.5 m/s | PASS: trigger 998.100 s, stop 1 002.100 s (expected 998.10 / 1 002.10), 0 errors |
| Audio in the player | 0 audio overruns after the intro in all four player runs; callback max 5.9-8.2 ms |
| Perf 1080p, 4K | see "Performance" below |
| Final key shots | `QA/final`: 20 key shots at 1080p, 16:10, 21:9 and 4K (80 images, contact sheets in `QA/final/_contact`); lookv2_check 52 / 80 shots pass; image_metrics 697 of 780 checks pass |
| The Ark | 13 stills along the Ark's pass (s 430-1 020) in `QA/final/ark`, looked at by the lead. Four Ark frame sequences: 23 checks PASS, 8 FAIL; every failure is the block-pop / block-blink check (52-54 pops and 245-599 blinks of 32 / 64 px blocks per sequence); the flash and red checks G1-G4 and G7 pass |
| Wall bot 2.5 / 1.5 m/s (final gate before the Ark, b14abae) | PASS: max excess -292.8 / -293.1 mm and -295.7 / -295.7 mm, 0 errors; not repeated after the Ark wave |
| Soak (run p4_01, master caddb90) | 1 200 s PASS, 0 errors, managed growth -0.18 MB |
| Sequences (run p4_01) | PASS: signs, rain, koi, traffic, texpans (old harness), dolly, all nine boundary sequences, statics 7 / 7 with `-freeze-clock -rain 0`, pans 7 / 7 |
| Encode x264 CRF 23 1080p (run p4_01) | 10 of 11 clips PASS (enc_beat01_wall fails on BandScore only, 0 visible steps) |
| Encode H1 at 4K (QA, fix wave) | KS16 0.00 steps / 1 000; KS10 2.56 in R/G/B only (passes: the gate counts Y/Cb/Cr, D-041); KS04 Cr 2.31 and KS17 3.29 sky / 4.18 plaza, measured on the fix-wave build. The integration build no longer showed gated Y/Cb/Cr steps there (INT-B), but these were not re-measured at 4K after the integration round. |
| Audio offline renders (fix wave) | 62 / 62 audio gates at both speeds; beat medians within +-1.5 dB of the 14.3 targets; true peak -7.6 dBTP; in-game `-audio-dump` within 0.14 dB of the offline render; integrated loudness -27.3 LUFS (final f08 render; docs/AUDIO.md section 10) |

### Known issues (open; accepted for this release, D-042)
| Severity | Issue |
|---|---|
| S2 | Texture-pan shimmer (texpans) above the `-capture-ss 2` reference at KS04 (blade edge strips; the stabilised residual passes at 1.85), KS05, KS08, KS09 (2.55) and KS17 (2.72). |
| S2 | At s 552 the opaque ribs of the pier balustrade glass sweep across the distant Gulf galleries, so some warm light strips vanish for one frame (NocGlass rib pitch 6 m at 2.5-3 m from the eye). |
| S2 | KS11: face B below the deck stays bright (p95 88 against a target of 36). KS13: p99 71-78 against 90. |
| S2 | The wet-floor reflection streaks under the boulevard shop row switch on in one frame at s 132 (QA-F02). |
| S3 | KS16 photosensitivity check G5: 15 small blocks modulate above 0.5 Hz. The cause is real abyss traffic moving through gaps in the garden crowns. There is no flash; G1-G4, G6 and G7 pass. |
| S3 | Single bright pixels in some key shots (far lanterns through 1 px leaf gaps in KS14 / KS15). |
| S3 | The koi still reads fairly solid at 1080p (body 150 against a 206 rim), though tower lights now show through it and the air around it glows. |
| S3 | 4K performance under the Ark: in s 580-600, 171 frames take over 20 ms (mean 17.88 ms, max 22.84 ms; DR-09). 1080p is the perf gate (D-042). Record in real time at 2560x1440, or at 4K only with a frame-locked capture. |
| S3 | The H5 rain image gate is not valid yet: its component count is dominated by moving foliage. The difference method measures about 45 drops per frame. |
| Gate result | Final key shots: lookv2_check passes 52 of 80 shots; image_metrics passes 697 of 780 checks. |
| Gate result | The Ark sequences fail the block-pop / block-blink check (see the gate table); the failing frames were not reviewed one by one. Soak, the other sequences, the encode tests and the wall bots were last run before the Ark wave. |
| Gate result | Encode at 1080p: enc_beat01_wall fails on BandScore only (0 visible steps). 4K H1 at KS04 and KS17 was not re-measured after the integration round. |

## Run
- Step-by-step guide (download, build from source in Unity, controls, recording): [HOW_TO_PLAY.md](HOW_TO_PLAY.md).
- Player: `Builds/Nocturne/Nocturne.exe` (the folder is 464 MB; `Builds/` is not in the repository, build it as the
  guide shows). It needs Windows and a D3D11 GPU. VRAM use: not measured.
- The folder `Builds/Nocturne/Nocturne_BackUpThisFolder_ButDontShipItWithYourGame` is Unity's debug data. The game does
  not need it.
- The game opens on a black fade-in. Walk forward to reach the goal; the run ends with a fade.
- The player runs with vSync on, at the display's refresh rate. The cursor is locked and hidden during play and visible
  in the pause menu.

### Performance (Ark final gate, each run alone, vSync 0, whole cinematic walk)
| | 1080p | 4K (`-perf-rt 3840x2160`) | 1440p |
|---|---|---|---|
| Average | 288.3 fps | 83.8 fps | not measured |
| Frame p99 | 3.68 ms | 20.47 ms | not measured |
| Frame max | 16.55 ms | 25.61 ms | not measured |
| Frames over 33 ms | 0 | 0 | not measured |
| GPU average | 3.45 ms | 11.90 ms | not measured |
| GPU median | 3.51 ms | 11.42 ms | not measured |
| GPU p99 | 3.64 ms (budget 8.2) | 20.43 ms | not measured |
| Spec windows | the boundary windows PASS (0 frames over 20 ms); the s 580-600 and s 700-740 windows were not mapped for this run (SKIP), the whole-walk maximum is 16.55 ms | s 580-600: 171 frames over 20 ms, mean 17.88 ms, max 22.84 ms (INFO) | not measured |

The 20 ms frame-time rule is a 1080p gate and holds there (frame max 16.55 ms over the whole walk). At 4K it is INFO
and a known issue (D-042, DR-09).

## Controls
| Input | Action |
|---|---|
| W A S D / arrow keys, gamepad left stick | walk (1.5 m/s) |
| Shift (hold), gamepad shoulder buttons | brisk walk, 2.5 m/s (the cinematic speed) |
| Mouse, gamepad right stick | look |
| C | toggle the cinematic autowalk (constant 2.5 m/s with the authored gaze) |
| Esc, gamepad Start | pause menu |
| W / S, arrow keys, gamepad D-pad or left stick | menu up / down |
| Enter / Space, mouse click, gamepad A | menu select |
| gamepad B | menu back |
| F11, Alt+Enter | toggle full screen (borderless, at the display's native size) / windowed |

During the cinematic autowalk, any other input ends it and hands control back to the player: a movement key, a mouse
move or click, a stick or a gamepad button. C, F11, Esc, Alt, Enter, Print Screen and the Windows keys do not end it.

## Command-line flags (one value per flag)
| Flag | Effect |
|---|---|
| `-cinematic` | start in the cinematic autowalk: 2.5 m/s constant, authored gaze, no input needed (for recording) |
| `-res 2560x1440` | open a window of that size (F11 then switches to full screen at the display's native size) |
| `-rain 0..1` | rain density override (0 turns the rain off) |
| `-tier 0/1/2` | force a fallback tier: full / simplified / graybox (passed to the builders as WorldContext.Tier; the default is 0) |
| `-nofade` | skip the opening fade |
| `-quit-after <s>` | quit after that many seconds of wall time |
| `-beat <N>`, `-progress <X>` | start at beat N (1-7), or at the fraction X of the path (s = 1 500 X) (testing) |
| `-brisk`, `-speed <X>`, `-seed <N>` | QA: walk speed 2.5 m/s, or X m/s; PRNG seed |
| `-autotest`, `-autotest -cinematic`, `-autotest-until <s>`, `-wallbot`, `-soak <s>` | QA runs: completability at 1.5 / 2.5 m/s, the smoke gate up to s, the wall-hugging bot, the soak |
| `-capture <dir> -capture-set <set>`, `-capture-ss 2`, `-sequence <name>` | QA captures and frame sequences (sets in `Assets/_Game/Scripts/Runtime/QA/ShotPlan.cs`); `-capture-ss 2` is the supersampled reference |
| `-freeze-clock`, `-freeze-rain`, `-dither static/temporal/off` | QA: frozen scene clock, frozen rain clock, dither mode (default static) |
| `-perf-log <file>`, `-perf-rt <WxH>` | performance run: frame-time CSV with an uncapped frame rate; render at WxH (the 4K perf run uses 3840x2160) |
| `-render-audio <file.wav> [-render-s0 S] [-render-s1 S] [-speed 1.5 / -brisk / -cinematic]` | render the score offline to a 48 kHz 32-bit float WAV and quit (default s 0-1 500 at 2.5 m/s) |
| `-audio-dump <file.wav> [-audio-dump-seconds N]` | record the in-game audio mix (default 10 s, after the intro) and compare it with an offline render of the same window |
| `-facade-prepass 0/1`, `-relief-v 1`, `-tex 0`, `-debug-view <N>` | Look v2 debug switches (the defaults are the accepted paths). `-facade-v`, `-surf-v` and `-foliage-v` were removed: they log a warning and do nothing. |

## Recording the video
1. **Resolution.** Record in real time at 2560x1440 (D-042). 1440p performance was not measured. At 4K the frame p99 is
   20.47 ms, above the 16.7 ms of a 60 fps frame, so 4K needs a frame-locked capture; this README has no tested command
   for one.
2. **Start.** Start the recorder first, then run `Builds/Nocturne/Nocturne.exe -cinematic -res 2560x1440`. `-res` opens a
   2560x1440 window. On a 2560x1440 display, press F11 for full screen; on any other display, record the window (F11
   would switch to the display's native size).
3. **Capture.** Record with OBS (or similar) at 60 fps with a high bitrate or a near-lossless codec. Do not touch the
   mouse, keyboard or gamepad: any input except C, F11, Esc, Alt, Enter, Print Screen and the Windows keys ends the
   autowalk.
4. **Length.** The cinematic autotest measures the goal trigger at 599.050 s and the stop at 601.450 s. The ending
   lasts 17.00 s.
5. **Encode.** The image gates use x264 at CRF 23: at 1080p 10 of 11 clips pass (enc_beat01_wall fails on BandScore
   only, with 0 visible steps); the 4K results are in the gate table above. Encode the master at CRF 23 or better
   quality, and upload at 1440p or higher (MASTER_PROMPT 14.7 H1). Do not add denoise or debanding filters at the edit:
   the game's last pass adds a fixed dither against banding in the dark haze, and a denoiser removes it.
6. **Loudness.** Do the upload-loudness step below.

### Upload loudness (docs/AUDIO.md section 10, D-038, fix wave C50)
The game is mixed to the level table of MASTER_PROMPT 14.3, not to an upload target. The final facts give an integrated
loudness of -27.3 LUFS (final f08 render; docs/AUDIO.md section 10) and a true peak of -7.6 dBTP. AUDIO.md section 10 measures -27.3 LUFS integrated
(BS.1770-4) on its f08 render of the cinematic walk. YouTube turns loud uploads down to about -14 LUFS and never turns
quiet ones up, so an untreated capture plays far below a typical upload. Do this at the edit:
1. **The audio track.** Best: replace the screen capture's audio with the offline render of the same walk,
   `Nocturne.exe -render-audio walk.wav -cinematic` (48 kHz, 32-bit float, the same DSP as the game, a pure function of
   s at the cinematic pace). The footsteps are scheduled by distance, so they can sit a few ms off the picture's strides.
   Align the render on the 0.6 s fade-in: cross-correlate the first 10 s with the capture's own track and shift by the
   peak (a constant offset; there is no drift, both run at 48 kHz). Note: the player's `-render-audio` renders s 0-1 500
   at constant speed and does not include the stop ramp or the ending (the code leaves `OfflineRenderSettings.Ending`
   off). For the ending, keep the capture's own audio, or render with the audio harness's `walk --speed 2.5 --ending
   --wav <file>` (`docs/AUDIO.md` section 6). Otherwise capture the game at unity gain: the Windows output device and the
   OBS source at 100 % / 0 dB, 48 kHz, and no OBS filters (no noise suppression, gain, compressor or limiter on the
   source or the master).
2. **The gain.** Measure the integrated loudness of your track and add the difference to -14 LUFS into a true-peak
   limiter with its ceiling at -1.0 dBTP (oversampled detection, look-ahead >= 1 ms, release 50-100 ms). For the f08
   render (-27.3 LUFS) AUDIO.md gives +13.3 dB. In that render the limiter takes up to 6.7 dB off the loudest peaks, in
   2.7 % of the 10 ms blocks, mostly the train passes of beat 4 (s 600-830). Listen there, and through the KS09 and
   KS17 silence drops, for pumping or a lifted floor. If it pumps, use +11 dB (about -16.3 LUFS for the f08 render).
   Do not compress or normalise per scene: the dread layer's drops and swells are level contrasts by design (the
   silence drops before KS09 / KS17 are 11.0 / 14.8 dB).
3. **Check.** Measure the exported file (`ffmpeg -af ebur128=peak=true`, or the editor's loudness meter): integrated
   -14 +-1 LUFS, true peak <= -1 dBTP.

### Beats and moments for the edit
s is the distance along the path in metres. The times are s / 2.5, the cinematic pace; the autotest's goal trigger at
599.050 s shows they are close but not exact.

| Beat | s (m) | t at 2.5 m/s (s) | Moments (s in m unless marked t) |
|---|---|---|---|
| 1 Alley | 0-120 | 0-48 | empty lit rooms |
| 2 Boulevard canyon | 120-400 | 48-160 | wet-floor streaks switch on at s 132 (known issue); sheet-lightning glows at s 205 and 345; the empty tram waits with open doors and chimes at s 226.25; darkness wave down a tower at s 300; look-up at KS05 |
| 3 The Edge | 400-560 | 160-224 | the Ark's lights far up the rift (2.9 km at s 400, 0.9 km at KS09), the gaze turns to the coming bow at s 492-500; the sign panel tears off face B at s 470 and falls 662 m in 20 s with no impact sound; silence drop before KS09 and a look-up at KS09; vertigo at KS10 (vFOV 68 -> 74, 0.35 m lean); the rib sweep at s 552 (known issue); glow at s 553 outlines the Ark |
| 4 Skybridge | 560-880 | 224-352 | a 9 s silence, then the Ark's bow passes over the walk at s 596 (t 238.4; the gaze looks up at s 576-586); the walker crosses the bridge under its belly: lit decks, gondolas, engine pods, slow searchlights; the long look-up under the belly at s 748-822; 2-3 cm bridge sway; the 900 m shadow under the bridge, seen through the KS13 glass strip; glow at s 790 |
| 5 Hanging garden | 880-1 080 | 352-432 | the Ark's stern leaves the walk at s 1 055.7 (t 422.3) and goes north into the haze, glimpsed through the crown gaps |
| 6 Wall of signs | 1 080-1 260 | 432-504 | the enormous eye opens at t 454-478 s; glow at s 1 140 |
| 7 Heart Square | 1 260-1 500 | 504-600 | darkness wave at s 1 330; silence drop before KS17 and a look-up at KS17; the koi's eye follows the camera (at most 6 deg/s); glow at s 1 420; the warm window |

## For developers
- Spec: `MASTER_PROMPT.md` (Part 14 is the game; Part 15 is the definition of done and the metrics). Architecture,
  commands and every decision: `DEVELOPMENT.md`. Current state, the run history and resume notes: `STATE.md`.
- Topic documents: `docs/AUDIO.md` (score, soundscape, dread stem, gates), `docs/DREAD.md` (the D-034 layer),
  `docs/LOOK_V2.md` (Look v2), `docs/TEXTURES.md` (the CC0 textures), `docs/RESEARCH.md`. Builder decision logs:
  `docs/logs/<ROLE>.md`.
- Unity runs only through `Tools/unity-queue.ps1`; players only through `Tools/run-player.ps1` / `Tools/lookdev.ps1`.
  The command list is in `STATE.md` "Commands". Fast C# check: `python -I Tools/fastcheck.py all`.
- QA: `QA/`. Captures are in `QA/captures/` (git-ignored); reports in `QA/reports/`; critiques in `QA/critique-*.md`;
  the final key shots in `QA/final/` (git-ignored, on disk only). The panel's findings are in `Logs/fix/_summary.md` and
  the verified facts behind these documents in `Logs/final_facts.md` (`Logs/` is git-ignored, local only).
- Decisions that changed the spec (`DEVELOPMENT.md`): D-027 Look v2 (dark neutral palette, CC0 textures); D-029 key-shot
  pose amendments; D-032 lead arbitration (KS10 drop by composition, koi spine, wet-floor gain, p99 rule, encode test,
  floor-line gate, 33 ms gate); D-034 the horror and megalophobia layer; D-035 the fifth start's run shape (worktrees,
  one wave, deadlines); D-036..D-039 builder decisions; D-040 the panel rulings (hull over the walker, KS04 pitch 27,
  shadow via KS13 only, beat-5 cast window); D-041 the integration rulings (WCAG flash definition, H1 counts Y/Cb/Cr,
  KS01 window 20-36); D-042 the final rulings (perf gate at 1080p, 1440p real-time recording, known issues accepted for
  this release); D-043 the Ark and the traffic moved away from the walk.
- Process (Part 15): phases 0-3 (preflight, research, framework import, graybox) ran on 2026-11-13 / 14; the Look v2
  build on 2026-11-14 / 16 (accepted, D-031). Phase 4 wave A started on 2026-11-16 and was paused four times (the user,
  a weekly usage limit, a slow-.NET morning on 2026-11-20). The fifth start (2026-11-20 20:12 to 2026-11-21 07:10)
  followed the user's run shape: eleven builder roles (AB, AT, BV, FX, HT, SG + AL, TR, GD, AU, DR) in parallel in five
  git worktrees (abyss, blvd, heart, waveb, integ) with one Unity queue per folder and a cap of 3 players; a merge into
  an integration copy; a full gate; one six-lens panel plus a chief critic; one fix wave of twelve roles (plus a lead-QA
  builder); one three-agent integration round; the final gate.
- Machine notes: twice, capturing the whole "encode" / "boundaries" sets filled disk W: (13-17 GB of frames); run them
  clip by clip. At 03:33 on 2026-11-21 the Windows commit limit (64 GB RAM and a small pagefile) stopped several runs
  while twelve builders ran.
- The Ark wave (2026-11-21 20:10-23:35): five roles in two worktrees (DR the Ark, AB the traffic, AT the Ark in the
  haze and the searchlights, FX the gaze and the Ark captures, AU the Ark's sound), merged, then the Ark final gate.
- Metrics (Part 15.3: time, tokens, API-equivalent cost) for the whole project, Ark wave included:
  `python -I Tools/metrics.py --pricing Tools/pricing_fable52.json --only-sessions <the project's lead sessions> --md QA/reports/metrics_fable52.md --json QA/reports/metrics_fable52.json`.
  Result: wall clock 196:00:25, active time 45:09:37, 6,396,532,634 tokens, $4,270.64 API-equivalent (an estimate at
  the Claude Fable 5.2 list prices, the price basis the author chose; not a bill). The full table is in
  `QA/reports/metrics_fable52.md` and `FINAL_REPORT.md`.
