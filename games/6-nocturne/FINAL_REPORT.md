# FINAL REPORT — Nocturne (game 6)

> Final report (MASTER_PROMPT Part 15), 2026-11-21, updated after the Ark wave (D-043, 23:35). Player:
> `Builds/Nocturne/Nocturne.exe`, built from the Ark merge 948c767 with 0 errors. The numbers of the first release come
> from the lead's verified-facts file `Logs/final_facts.md`; the Ark wave's numbers come from the Ark final gate's logs
> and reports (`QA/reports/*final*`). `FEATURE_MATRIX.md` holds the status of every beat. `README.md` covers running,
> controls and flags; `HOW_TO_PLAY.md` is the step-by-step guide.

## What was built

### The game
- A first-person walk of 1 500 m through a rain-night megacity with no ground. It ends at one warm window under a 900 m
  hologram koi.
- The walk takes 600 s at the cinematic 2.5 m/s and about 1 000 s at the walking 1.5 m/s.
- No people, no real text, no shadow maps, no flashing.
- Game 6 of the Megastructures series, built by Claude in Claude Code (one lead session and its builder, critic and
  writer agents) from the prompt in `MASTER_PROMPT.md`.

| Beat | Path (s, m) |
|---|---|
| 1 Alley | 0-120 |
| 2 Boulevard canyon | 120-400 |
| 3 The Edge | 400-560 |
| 4 Skybridge | 560-880 |
| 5 Hanging garden | 880-1 080 |
| 6 Wall of signs | 1 080-1 260 |
| 7 Heart Square | 1 260-1 500 |

### Engine and pipeline
- Unity 6000.6.0f1, URP 17.6 Render Graph, Forward+, MSAA 4x, HDR, D3D11.
- Geometry, shaders and audio are procedural. The only imported assets are CC0 textures (D-027; provenance in
  `docs/TEXTURES.md`).
- The framework was imported from games 1-3 (Part 3.1). Every Unity call runs through a queue; every player run goes
  through `Tools/run-player.ps1`.
- The player folder `Builds/Nocturne/` is 464.3 MB.

### Look v2 (the user's directive D-027, design in `docs/LOOK_V2.md`)
The directive: a dark neutral palette with no violet, and real textures on every building, window and surface. It was
built as:
- the dark neutral palette;
- analytic window facades with photographic CC0 skins, recesses, interiors and section districts;
- textured near surfaces with an analytic wet-floor mirror;
- lobed leaf-card trees;
- a scaled Kohaku koi hologram;
- relief and gate structures;
- a tinted atmosphere with fog, haze and cloud;
- a Floor-relative grade with a static TPDF dither.

### The Ark (D-043, the user's order of 2026-11-21 evening)
The user asked for the flying ship to become colossal, to come smoothly out of the haze, and to be the most mesmerizing
scene of the game. The Ark replaces the earlier 1 800 m hull:
- **Size.** 5 000 m long, 438 m beam (80 m clear of both rift faces at the 600 m neck), keel plate at y 330, its back
  at y 590 inside the cloud deck: a stepped inverted ziggurat whose upper steps melt into the deck. 16 engine pods, 59
  hanging gondolas, hangar bays, 4 m-pitch lit decks drawn by the filtered NocArk shader. About 39 k triangles and no
  realtime light.
- **Course.** It comes down the rift from the north, because every gaze of beat 3 faces north. Hidden until s 380; the
  bow is 2.9 km out at s 400 and 0.9 km out at KS09; it passes over the walk at s 596 (s 509.6 at 1.5 m/s); the stern
  leaves at s 1 055.7. It appears and leaves only through fog or the cloud deck.
- **Light.** Six slow searchlight cones (28-40 s sweeps) that never contain the walker, an Ark glow in the haze, a
  deck occluder round the hull; the sheet lightning at s 553 and s 790 outlines it.
- **Camera.** The gaze turns to the coming bow (s 492-500), looks up as the bow passes overhead (s 576-586), and holds
  a long look-up under the belly to near the zenith (s 748-822). Every key shot stays readable; gaze rates stay at or
  under 22 / 18 deg/s.
- **Sound.** Horn calls from the bow, a rising pressure, a 9 s silence before the bow (rest -21 dB), a long Doppler,
  the engine pods passing one by one, deep groans.
- **Traffic.** The user also asked to remove the vehicles that passed close to the walker on the skybridge and read as
  laser beams. Every abyss traffic layer now lies at least 218 m from the walk and below the walker, boulevard vehicles
  are hidden within 300 m of the camera over the rift, and three distant air crossings keep the highlight rule. A 60 fps
  proof over s 530-900 has no vehicle streak of 80 px or more (the longest is 64 px).

### The horror and megalophobia layer (D-034, `docs/DREAD.md`)
The tone is dread, never jump scares. Built:
- **Something huge moves.** The Ark (above). 32 lift cabins run on 8 shafts, and 2 gantries move on the Gulf faces.
- **Presence in the abyss.** A 900 m shadow passes under the bridge in the abyss haze; it is seen through the KS13 glass
  strip. A 12 x 8 m sign panel tears off face B at s 470 and falls 662 m in 20 s with no impact sound.
- **The city notices you.** A darkness wave runs down a tower (s 300 and s 1 330). The koi's eye follows the camera (at
  most 6 deg/s). One enormous eye opens on the wall of signs (t 454-478). Six dim sheet-lightning glows light the deck
  (s 205, 345, 553, 790, 1 140, 1 420).
- **Camera.** Long look-ups at KS05 / KS09 / KS17 and under the Ark. A vertigo vFOV 68 -> 74 at KS10 with a 0.35 m
  lean. 2-3 cm bridge sway.
- **Traces of emptiness.** An empty tram waits with open doors and chimes at s 226.25. A lift lobby. Empty lit rooms in
  the alley and the Old Quarter.

### Sound (`docs/AUDIO.md`)
- A procedural score (pad voices, chords keyed to s) and a soundscape per beat.
- The D-034 dread layer (stem E): echoes off the Gulf faces at 2 d / 343 s; sub-bass swells under KS04 / KS09 / KS10 /
  KS13 / KS17 with a laptop-band layer; structural groans; silence drops of 11.0 / 14.8 dB before KS09 / KS17; the Ark's
  sound (above); the panel's creak.

## How it was built

### Timeline
| Period | Work |
|---|---|
| 2026-11-13 / 14 | Phases 0-3: preflight, research, framework import, graybox. The run anchor is 2026-11-13 19:22:09 (+01:00). |
| 2026-11-14 / 16 | Look v2 build after the user's directive D-027; accepted in D-031. |
| 2026-11-16 to 2026-11-20 | Phase 4 wave A. Paused four times: by the user, by a weekly usage limit, and by a slow-.NET morning on 2026-11-20. |
| 2026-11-20 20:12 to 2026-11-21 07:10 | The fifth start, in the user's run shape (D-035), to the first release. |
| 2026-11-21 20:10 to 23:35 | The Ark wave (D-043) and the Ark final gate. |

### The fifth start (D-035)
The user set the run shape. It ran in this order:
1. Eleven builder roles in parallel in five git worktrees, with one Unity queue per folder and a cap of 3 players.
2. Merge into an integration copy.
3. Full gate (run p4_01, master caddb90).
4. ONE six-lens critic panel plus a chief critic.
5. ONE fix wave of twelve roles (plus a lead-QA builder).
6. ONE three-agent integration round.
7. Final gate (2026-11-21 05:25-06:56, build of master b14abae, quiet machine).

After the gate, three writers wrote the documents from `Logs/final_facts.md`, and the lead measured the Part 15.3
metrics.

| Role | Area | Worktree |
|---|---|---|
| AL | beat 1, the alley | waveb |
| BV | beat 2, the boulevard canyon, the tram and street level | blvd |
| FX | cross-beat look and every shared file | blvd |
| AB | beat 3, the Edge and the abyss content | abyss |
| AT | atmosphere, sky, post and light rig | abyss |
| DR | the D-034 objects: hull (later the Ark), abyss shadow, lift cabins and gantries, falling panel | abyss |
| TR | beat 4, the trains | waveb |
| GD | beat 5, the hanging garden | waveb |
| SG | beat 6, the wall of signs and the arcade | heart |
| HT | beat 7, Heart Square, the koi, the goal window and the spire | heart |
| AU | the score, the soundscape and the dread stem | waveb |
| Lead | merges, gates, rulings, metrics | integ (integration copy) |

Working rules (D-035):
- Builders did not commit. The lead committed each copy on its branch and merged it.
- Roles with disjoint files shared a copy and its Unity queue. Free disk space set the number of copies.
- Each role had a deadline.
- Cross-copy requests travelled as "REQUEST" lines and in the structured reports.

The fix wave gave each role its findings and the lead's rulings in `Logs/fix/<ROLE>.md` (D-040). The integration round
ran three agents on disjoint files: INT-A (rendering and shared files), INT-B (QA and visibility) and INT-C (beat
content, dread and audio), under the rulings of D-041.

### The Ark wave (D-043)
Five roles in two worktrees, one run each, with a deadline: DR (the Ark: course, builder, NocArk shader), AB (the
traffic) and AT (the Ark in the haze, searchlights, lightning) in the abyss copy; FX (gaze, Ark capture poses and the
"ark" capture set) and AU (the Ark's sound) in the integration copy. The lead merged both copies, ran the Ark final gate,
looked at the Ark stills and copied the build to `Builds/Nocturne`.

### Critic panel and fixes
- ONE six-lens panel (scale / wow, defects, matter / light, motion / pacing, audio, spec) plus a chief critic reviewed the
  merged game (run p4_01). Result: 0 S1, 29 S2, 24 S3 (C01-C53, `Logs/fix/_summary.md`).
- The wow audit FAILED before the fix wave: KS04, KS05, KS06, KS10 and KS17 failed; KS09, KS18 and KS19 passed.
- ONE fix wave of twelve roles and ONE three-agent integration round fixed most of the findings: the hull course; the
  koi's air and translucency; abyss depth pools and a sink veil; street furniture and arcade signs; a frameless glass edge
  at KS10 with a drop of 43.8 %; portal rings; step lights; wet floors; horizontal glass; the D-034 items; and QA gates
  that measure what they claim.
- No second panel was run, and no critic has judged the Ark. The user's run shape has one panel.

## QA evidence

### Ark final gate (2026-11-21 22:12-23:21, build of the Ark merge 948c767)
| Gate | Build or run | Result |
|---|---|---|
| fastcheck (C#) | 948c767 | OK, runtime 221 / editor 297 files |
| Validate | 948c767 | SUCCESS, 0 problems |
| EditMode | 948c767 | 528 tests: 527 passed, 0 failed, 1 skipped (explicit search test) |
| Build | 948c767 | 0 errors, 464.3 MB |
| Smoke (brisk to s 400) | 948c767 | PASS |
| Autotest cinematic 2.5 m/s | 948c767 | PASS, goal trigger 599.050 s, stop 601.450 s (expected 599.05 / 601.45), ending 17.00 s, 7 beats, 0 errors |
| Autotest walk 1.5 m/s | 948c767 | PASS, trigger 998.100 s, stop 1 002.100 s (expected 998.10 / 1 002.10), ending 17.00 s, 7 beats, 0 errors |
| Audio in the player | 948c767 | 0 audio overruns after the intro in all four player runs, callback max 5.9-8.2 ms |
| Perf 1080p / 4K | 948c767 | see the performance table below |
| Final key shots | 948c767 | `QA/final`: 20 key shots at 1080p, 16:10, 21:9 and 4K (80 images, contact sheets in `QA/final/_contact`); lookv2_check 52 / 80 shots pass; image_metrics 697 of 780 checks pass |
| The Ark | 948c767 | 13 stills along its pass (s 430-1 020) in `QA/final/ark`, looked at by the lead; four Ark sequences (coming, bow, belly, emerge): 23 checks PASS, 8 FAIL, all of them the block-pop / block-blink check (52-54 pops and 245-599 blinks of 32 / 64 px blocks per sequence); G1-G4 and G7 (flash, red) PASS |
| Traffic | AB, Ark wave | every abyss layer at least 218 m from the walk; longest vehicle streak at 60 fps over s 530-900 is 64 px (limit 80) |
| Wall bot 2.5 m/s | b14abae (before the Ark) | PASS, max excess -292.8 / -293.1 mm (left / right), 0 errors |
| Wall bot 1.5 m/s | b14abae (before the Ark) | PASS, max excess -295.7 / -295.7 mm, 0 errors |
| Soak | p4_01, master caddb90 | 1 200 s PASS, 0 errors, managed growth -0.18 MB |
| Sequences | p4_01, master caddb90 | PASS: signs, rain, koi, traffic, texpans (old harness), dolly, all nine boundary sequences, statics 7 / 7 with -freeze-clock -rain 0, pans 7 / 7 |
| Encode x264 CRF 23 1080p | p4_01, master caddb90 | 10 of 11 clips PASS (enc_beat01_wall fails on BandScore only, 0 visible steps) |
| Encode H1 at 4K | fix wave (QA) | KS16 0.00 steps / 1 000; KS10 2.56 in R/G/B only (passes: the gate counts Y/Cb/Cr, D-041); KS04 Cr 2.31 and KS17 3.29 sky / 4.18 plaza measured on the fix-wave build (not re-measured at 4K since) |
| Audio offline renders | fix wave | 62 / 62 audio gates at both speeds; beat medians within +-1.5 dB of the 14.3 targets; true peak -7.6 dBTP; in-game -audio-dump within 0.14 dB of the offline render; integrated loudness -27.3 LUFS (final f08 render; docs/AUDIO.md section 10) |

The wall bot rows come from the first release's final gate (b14abae); the soak, sequence and 1080p encode rows come from
gate p4_01 on master caddb90; the 4K encode and the offline audio rows come from the fix wave. They are the latest
measurements of those gates; the Ark final gate did not repeat them.

### Performance (built player, alone, Ark final gate)
| Metric | 1080p (whole cinematic walk, vSync 0) | 4K (`-perf-rt 3840x2160`) |
|---|---|---|
| Average fps | 288.3 | 83.8 |
| Frame time p99 | 3.68 ms | 20.47 ms |
| Frame time max | 16.55 ms | 25.61 ms |
| Frames over 33 ms | 0 | 0 |
| GPU average | 3.45 ms | 11.90 ms |
| GPU median | 3.51 ms | 11.42 ms |
| GPU p99 | 3.64 ms (budget 8.2) | 20.43 ms |
| Spec windows | boundary windows PASS; s 580-600 and s 700-740 not mapped for this run (SKIP); whole-walk max 16.55 ms | s 580-600: 171 frames over 20 ms, mean 17.88 ms, max 22.84 ms |

The 20 ms frame-time rule is the 1080p gate (D-042) and holds there. At 4K it is INFO and a known issue (DR-09, the Ark
over the bridge in s 580-600). The machine is described in `DEVELOPMENT.md` "Environment".

Not measured at the final gates: PlayMode tests, ray-grid coverage, VRAM, and performance at 2560x1440 (the recommended
recording size).

### Definition of Done (Part 15.1)
| Item | Status |
|---|---|
| 1. The player starts to a black fade-in, plays from start to goal and ends cleanly | Both autotests PASS on the Ark build (7 beats, 0 errors). Per-beat tiers are in `FEATURE_MATRIX.md`. |
| 2. Perfection criterion: two consecutive clean critic rounds and a passed wow audit | **Not met.** One panel ran (the user's run shape, D-035). It found 0 S1, 29 S2, 24 S3. The wow audit failed before the fix wave, and no wow audit ran after it or after the Ark wave. D-042 accepts the open issues below for this release. |
| 3. All automated checks pass in the built player | **Partly.** Exceptions: encode 10 of 11 clips at 1080p; lookv2_check 52 / 80 shots; image_metrics 697 of 780 checks; the Ark sequences' block-pop / blink check; the H5 rain image gate is not valid yet; the 20 ms rule at 4K is INFO. Soak, sequences and the 1080p encode were last run at p4_01; the wall bots before the Ark wave. |
| 5. Documents and final key shots | This report, `README.md`, `FEATURE_MATRIX.md` and `HOW_TO_PLAY.md`. The final key shots are in `QA/final` and the Ark stills in `QA/final/ark` (git-ignored; they stay on disk). |
| 8. The cinematic walks at a constant 2.5 m/s and its autotest passes | PASS: goal trigger 599.050 s and stop 601.450 s, exactly as expected. |

Items 4, 6 and 7 are not assessed in this report.

## Known issues

These are open. D-042 accepts them for this release. They stay visible here and in `FEATURE_MATRIX.md`.

| Severity | Issue |
|---|---|
| S2 | texpans shimmer above the `-capture-ss 2` reference at KS04 (blade edge strips; the stabilised residual passes at 1.85), KS05, KS08, KS09 (2.55) and KS17 (2.72). |
| S2 | At s 552 the opaque ribs of the pier balustrade glass sweep across the distant Gulf galleries, so some warm light strips vanish for one frame (NocGlass rib pitch 6 m at 2.5-3 m from the eye). |
| S2 | KS11: face B below the deck stays bright (p95 88 against a target of 36). KS13 p99 is 71-78 against 90. |
| S2 | Wet-floor reflection streaks under the boulevard shop row switch on in one frame at s 132 (QA-F02). |
| S3 | KS16 photosensitivity G5: 15 small blocks modulate above 0.5 Hz. The cause is real abyss traffic moving through gaps in the garden crowns. There is no flash; G1-G4, G6 and G7 pass. |
| S3 | Single bright pixels in some key shots (far lanterns through 1 px leaf gaps in KS14 / KS15). |
| S3 | The koi still reads fairly solid at 1080p (body 150 against a 206 rim), though tower lights now show through it and the air around it glows. |
| S3 | 4K perf under the Ark: 171 frames over 20 ms in s 580-600 (mean 17.88 ms, max 22.84 ms; DR-09). Record in real time at 2560x1440, or at 4K only with a frame-locked capture. |
| S3 | The H5 rain image gate is not valid yet: its component count is dominated by moving foliage. The difference method measures about 45 drops per frame. |

Open verification gaps (no severity assigned):
- The wow audit failed before the fix wave (KS04, KS05, KS06, KS10, KS17) and was not repeated after it; no critic has
  judged the Ark.
- The four Ark sequences fail the block-pop / block-blink check; the failing frames were not reviewed one by one.
- lookv2_check passes 52 of the 80 final shots, and image_metrics passes 697 of 780 checks. The failing items are not
  broken down in this report.
- Encode at 1080p: enc_beat01_wall fails on BandScore only (0 visible steps).
- Encode H1 at 4K: KS04 Cr 2.31 and KS17 3.29 sky / 4.18 plaza were measured on the fix-wave build and not re-measured
  at 4K since.
- Soak, sequences and the 1080p encode were not repeated on a final build; the wall bots were not repeated after the Ark
  wave.
- The sequences row ran texpans in the old harness.
- The 1080p perf run's s 580-600 and s 700-740 windows were not mapped (SKIP); the whole-walk frame max of 16.55 ms is
  under the 20 ms rule.

Machine notes:
- Twice, capturing the whole "encode" / "boundaries" sets filled disk W: (13-17 GB of frames); in the Ark gate the disk
  guard stopped the Ark set after its stills were written.
- At 03:33 on 2026-11-21 the Windows commit limit (64 GB RAM + a small pagefile) stopped several runs while twelve
  builders ran.

## Decisions that changed the spec
The full text of every decision is in `DEVELOPMENT.md`.

| Decision | What changed |
|---|---|
| D-027 Look v2 (user directive) | Dark neutral palette with no violet; real textures on every building, window and surface. CC0 textures became the only imported assets (`docs/TEXTURES.md`); models, code, audio and fonts may still not be imported. |
| D-029 Key-shot poses | Lead arbitration of the Phase 3 gate: key-shot pose and subject amendments (KS19's subjects, the gaze keys at the Edge, the capture points of KS13 and KS03). |
| D-032 Lead arbitration | KS10 drop reached by composition; the koi spine may leave its orbit arc so the koi reads as a fish from below; an art-directed wet-floor gain; the p99 rule stays and is met with light content, not brighter windows; the encode test; the floor-line gate; the 33 ms gate re-measured on a quiet machine. |
| D-034 Horror and megalophobia layer | The tone moves to dread, never jump scares; no people, no real text, no flashing. Adds the hull (now the Ark), the abyss shadow, the falling panel, the lift cabins and gantries, the darkness wave, the koi's eye, the wall eye, the sheet lightning, the look-ups, the vertigo at KS10 (an exception to the vFOV schedule), the bridge sway, the traces of emptiness and the dread stem. No AI-generated or imported 3-D models. |
| D-035 Fifth start's run shape | Worktrees, one wave, deadlines (see "How it was built"). |
| D-036..D-039 Builder decisions | The rulings on the builders' reports of the fifth start. |
| D-040 Panel rulings | The hull flies over the walker; KS04 pitch 27 (a key-shot table amendment like D-029); the abyss shadow is required through KS13 only; a beat-5 dark-cast window for the garden's green darks. Measurement corrections: banding is judged by visible steps after encoding; statics run with -freeze-clock -rain 0. |
| D-041 Integration rulings | Photosensitivity follows the WCAG flash definition; the 4K H1 gate counts Y/Cb/Cr; KS01's mean-luma window is 20-36. |
| D-042 Final rulings | The perf gate (the 20 ms frame rule) is the 1080p gate; 4K is INFO. Real-time recording at 1440p. The known issues above are accepted for this release. |
| D-043 The Ark | The 1 800 m hull is replaced by the 5 000 m Ark coming down the rift from the north, over the walk at s 596; the abyss traffic moves at least 218 m from the walk (no laser-like streaks near the walker). |

## How to record the video
1. Start the player in the cinematic autowalk at 1440p:
   `Builds/Nocturne/Nocturne.exe -cinematic -res 2560x1440`.
   The cinematic walks at a constant 2.5 m/s with the authored gaze and needs no input.
2. Record in real time (D-042), at 60 fps with a high bitrate. Performance at 2560x1440 was not measured.
3. Record 4K only with a frame-locked capture. At 4K the frame p99 is 20.47 ms, longer than one 60 fps frame, and s
   580-600 (the Ark's bow over the bridge) has 171 frames over 20 ms.
4. Keep recording through the ending. The goal trigger comes at 599.050 s; the ending runs 17.00 s.
5. Sound: the in-game mix matches the offline render within 0.14 dB. The integrated loudness is -27.3 LUFS (final f08
   render; docs/AUDIO.md section 10). The loudness step for the upload is in `docs/AUDIO.md` section 10. The silence
   drops before KS09, KS17 and the Ark's bow are part of the design; do not normalise per scene.
6. Upload at 1440p, or at 2160p from a frame-locked 4K capture. More detail is in `README.md`.

## Project metrics
Measured by `python -I Tools/metrics.py --pricing Tools/pricing_fable52.json --only-sessions <the project's lead
sessions>` (output in `QA/reports/metrics_fable52.md`), from the anchor line `Run started at
2026-11-13T19:22:09.6952535+01:00` in `STATE.md`, over the whole project including the Ark wave.

| Metric | Value | How obtained |
|---|---|---|
| Wall-clock time | 196:00:25 | measured from the anchor 2026-11-13 19:22:09 (+01:00) to the last record 2026-11-21 23:22 (+01:00) |
| Active time | 45:09:37 | measured: wall clock minus 25 gaps longer than 10 min without transcript activity |
| Uncached input tokens | 37,226 | measured |
| Cache read tokens | 6,160,230,131 | measured |
| Cache write tokens (5 min) | 208,827,774 | measured |
| Cache write tokens (1 h) | 6,145,255 | measured |
| Output tokens (incl. thinking) | 21,292,248 | measured |
| Total tokens | 6,396,532,634 | measured: 18,429 assistant messages in 2 lead sessions and 225 subagent / workflow transcripts |
| API-equivalent cost | $4,270.64 | estimated at the Claude Fable 5.2 list prices of platform.claude.com/docs/en/about-claude/pricing (the price basis the author chose), fetched 2026-11-21: input $8, cache read $0.20, cache write 5 min $10, 1 h $16, output $40 per MTok |

Two other sessions that merely read this project's STATE.md (another game's lead session and an unrelated folder)
contain the anchor line and were excluded. Not counted: the final measurement turn and the final chat message.

Cost formula (the token counts and prices of the table; arithmetic by script):

| Category | Tokens x price | Cost |
|---|---|---|
| Uncached input | 37,226 x $8 / MTok | $0.30 |
| Cache read | 6,160,230,131 x $0.20 / MTok | $1,232.05 |
| Cache write (5 min) | 208,827,774 x $10 / MTok | $2,088.28 |
| Cache write (1 h) | 6,145,255 x $16 / MTok | $98.32 |
| Output | 21,292,248 x $40 / MTok | $851.69 |
| Sum (of the unrounded parts) | | $4,270.64 |

- The 25 gaps longer than 10 minutes are listed with their times in `QA/reports/metrics_fable52.md`.
- The cost is an API-equivalent estimate for comparison. It is not the user's subscription bill.
