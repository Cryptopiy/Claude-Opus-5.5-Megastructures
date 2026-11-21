# Frame sequences: seax-03s

Sequences: 7. Checks: 20 PASS, 4 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_b02 | transition | 601 | 2.082 | 0.870 | 2.783 | 148.5 | FAIL | luma step/frame |
| b02_b03 | transition | 601 | 0.273 | 1.206 | 3.124 | 113.8 | PASS |  |
| pan_KS04_sea | pan | 30 | 0.001 | 0.191 | 0.194 | 34.4 | PASS |  |
| pan_b02_Crest | pan | 30 | 0.002 | 0.241 | 0.243 | 95.2 | PASS |  |
| plumes_KS04 | static | 241 | 2.054 | 3.283 | 4.342 | 111.7 | FAIL | luma step/frame, flicker block std |
| static_KS04 | static | 120 | 0.047 | 0.733 | 0.908 | 86.0 | FAIL | flicker block std |
| static_b02_Crest | static | 120 | 0.008 | 0.012 | 0.025 | 23.4 | PASS |  |

## b01_b02 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-03s\seq_b01_b02`, 601 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▁▁▁▂▂▂▂▂▃▄▄▆▇████████████████████████████████`
- mean diff `▁▁▁▁▂▃▂▁▂▂▂▂▂▂▁▁▁▁▁▁▁▂▂▃▄▅▇█▇▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▃▃▄▄▂▂▂▂▃▃▃▃▄▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 2.0819 | <= 1.2 levels | at frame 275 |
| activation-frame diff | PASS | 0.0003 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.9556 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b02_b03 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-03s\seq_b02_b03`, 601 frames at 30 fps, kind transition

- luma      `██████▇▇▇▇▇▇▆▆▆▆▆▆▅▆▆▆▆▆▆▅▅▄▄▄▄▄▄▄▄▃▃▃▂▂▂▂▂▂▂▂▂▃▃▂▂▁▁▁▁▂▂▂▂▂`
- mean diff `▂▃▂▂▃▃▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▂▂▂▂▂▂▃▃▃▄▄▄▅▅▅▅▅▅▅▄▃▂▃▄▆█████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2728 | <= 1.2 levels | at frame 509 |
| activation-frame diff | PASS | 0.0000 | <= 0.5 levels over the local median | 2 boundary frames |
| EV slope | INFO | 0.1523 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-03s\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▂▂▂▂▂▃▃▃▃▄▄▄▄▄▄▄▅▅▅▅▆▆▇▇▇██`
- mean diff `▇▄▇▆▄█▆▅▄▆▇█▁█▄▆▅▅▄▅▅▆▄▆▄▆▃▆▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0012 | <= 1.2 levels | at frame 26 |
| EV slope | INFO | 0.0014 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 0.9199 | <= 2 levels | exact rotation homography; raw 0.920, control floor 0.719, low-pass (sigma 1.5) 0.272, block-mean 0.106; gate block [1664, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-03s\seq_pan_b02_Crest`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▁█▁▁█▁▁▁█▁▁██████▁█████████`
- mean diff `▃▃▅▂▄▅▆▁▃▄▅▅▅▂▇▂▇▄▅▅▃▅▇█▄▅▆▆▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0024 | <= 1.2 levels | at frame 14 |
| EV slope | INFO | 0.0027 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.4066 | <= 2 levels | exact rotation homography; raw 1.407, control floor 0.992, low-pass (sigma 1.5) 0.343, block-mean 0.135; gate block [896, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## plumes_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-03s\seq_plumes_KS04`, 241 frames at 4 fps, kind static

- luma      `▁▅▆▇▇▆▄▄▃▂▂▃▄▇▇▇▅▂▁▄▅▅▆▇▆▄▃▅▆▃▄▄▅▆▆▄▃▃█▅▇▇▅▅▅▄▃▃▁▂▅▆▅▃▄▅▇▄▂▁`
- mean diff `▁▅▅▄▄▄▃▂▃▃▄▄▃▄▃▄▃▂▂▄▅▃▄▅▃▄▃▃▂▃▄▃▄▄▄▄▃▆█▄▄▃▃▄▄▃▃▃▃▂▅▅▃▂▂▃▄▄▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 2.0537 | <= 1.2 levels | at frame 153 |
| EV slope | INFO | 0.2361 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 11.6368 | <= 3 levels | 241 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-03s\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▁▂▂▂▂▃▃▃▃▄▄▄▄▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇▇██████████`
- mean diff `▂▂▁▁▁▁▁▁▁▁▁▂▂▃▃▄▄▅▅▆▆▆▇▇▇██████████▇▇▇▇▇▇▇▇▇▇▇▆▆▆▅▅▅▅▅▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0469 | <= 1.2 levels | at frame 24 |
| EV slope | INFO | 0.0332 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 14.5262 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-03s\seq_static_b02_Crest`, 120 frames at 30 fps, kind static

- luma      `▅▆███▇▆▅▄▄▃▂▂▁▁▁▂▂▃▄▄▄▄▄▄▄▄▄▄▅▅▆▆▇▇▇████████▇▇▇▇▇▆▇▆▆▆▆▆▆▆▆▅`
- mean diff `▄▆▅▆██▇▄▃▃▃▂▂▂▁▂▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0078 | <= 1.2 levels | at frame 3 |
| EV slope | INFO | 0.0072 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 2.0363 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
