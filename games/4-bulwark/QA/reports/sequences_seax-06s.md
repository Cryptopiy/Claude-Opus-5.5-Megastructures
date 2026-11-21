# Frame sequences: seax-06s

Sequences: 7. Checks: 20 PASS, 4 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_b02 | transition | 601 | 2.082 | 0.869 | 2.783 | 148.5 | FAIL | luma step/frame |
| b02_b03 | transition | 601 | 0.289 | 1.256 | 3.362 | 113.3 | PASS |  |
| pan_KS04_sea | pan | 30 | 0.002 | 0.200 | 0.202 | 34.4 | PASS |  |
| pan_b02_Crest | pan | 30 | 0.003 | 0.242 | 0.243 | 95.2 | PASS |  |
| plumes_KS04 | static | 241 | 1.739 | 3.311 | 4.057 | 111.7 | FAIL | luma step/frame, flicker block std |
| static_KS04 | static | 120 | 0.043 | 0.742 | 0.918 | 78.1 | FAIL | flicker block std |
| static_b02_Crest | static | 120 | 0.007 | 0.012 | 0.022 | 22.6 | PASS |  |

## b01_b02 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\seq_b01_b02`, 601 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▁▁▁▂▂▂▂▂▃▃▄▆▇████████████████████████████████`
- mean diff `▁▁▁▁▂▃▂▁▂▂▂▂▂▂▁▁▁▁▁▁▁▂▂▃▄▅▇█▇▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▃▃▄▄▂▂▂▂▃▃▃▃▄▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 2.0820 | <= 1.2 levels | at frame 275 |
| activation-frame diff | PASS | 0.0003 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.9556 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b02_b03 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\seq_b02_b03`, 601 frames at 30 fps, kind transition

- luma      `██████▇▇▇▇▇▇▆▆▆▆▆▅▅▆▆▆▆▆▆▅▅▄▄▄▄▄▄▄▄▄▃▃▃▂▂▂▂▂▂▂▃▃▃▃▂▁▁▁▂▂▂▂▂▂`
- mean diff `▂▂▂▂▃▂▁▁▁▁▁▂▁▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▂▂▂▂▂▂▃▃▃▄▄▄▅▅▅▅▅▅▅▄▃▂▃▄▆█████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2888 | <= 1.2 levels | at frame 509 |
| activation-frame diff | PASS | 0.0014 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 0.1648 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▂▂▂▃▃▃▃▃▃▃▄▄▄▄▅▅▅▆▆▆▆▆▇▇▇██`
- mean diff `▇▄▇▅▄█▆▆▄▆▇█▁█▄▆▆▅▅▅▆▆▄▆▄▇▃▇▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0020 | <= 1.2 levels | at frame 26 |
| EV slope | INFO | 0.0020 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 0.9212 | <= 2 levels | exact rotation homography; raw 0.921, control floor 0.726, low-pass (sigma 1.5) 0.277, block-mean 0.107; gate block [1664, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\seq_pan_b02_Crest`, 30 frames at 30 fps, kind pan

- luma      `▅▅▅▁▅▅▅▅▅▅▅▅▅▅█▅██▅█▅█▅████▅██`
- mean diff `▃▃▅▂▄▅▆▁▂▄▅▅▅▂▇▂▇▄▅▅▃▅▇█▄▅▆▆▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0025 | <= 1.2 levels | at frame 4 |
| EV slope | INFO | 0.0027 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.4067 | <= 2 levels | exact rotation homography; raw 1.407, control floor 0.993, low-pass (sigma 1.5) 0.346, block-mean 0.135; gate block [896, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## plumes_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\seq_plumes_KS04`, 241 frames at 4 fps, kind static

- luma      `▁▆▇██▇▄▃▃▂▂▃▄▇▇█▅▂▁▄▅▆▇█▆▄▃▅▆▃▄▄▅▇▆▄▃▁█▆▇▇▅▅▅▄▃▂▁▂▅▆▅▃▅▅▇▅▂▁`
- mean diff `▁▆▆▅▅▅▄▃▄▃▄▄▃▅▃▄▄▂▁▅▆▃▅▆▃▄▄▃▂▄▄▃▄▄▄▄▄██▄▅▃▃▄▅▃▄▄▃▃▅▅▃▃▂▄▅▅▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.7393 | <= 1.2 levels | at frame 153 |
| EV slope | INFO | 0.1901 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 11.4995 | <= 3 levels | 241 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▃▃▃▃▄▄▄▄▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇▇██████████`
- mean diff `▂▂▁▁▁▁▁▁▁▁▂▂▂▃▃▄▄▅▅▆▆▆▇▇▇██████████▇▇▇▇▇▇▇▇▇▇▆▆▆▆▆▅▅▅▅▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0432 | <= 1.2 levels | at frame 35 |
| EV slope | INFO | 0.0318 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 14.8459 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\seq_static_b02_Crest`, 120 frames at 30 fps, kind static

- luma      `▅▆███▇▆▅▄▃▃▂▁▁▁▁▂▂▃▄▄▃▃▃▃▃▄▄▄▅▅▆▆▇▇▇████████▇▇▇▇▇▇▇▇▇▇▆▆▆▆▆▅`
- mean diff `▅▆▆▆█▇▆▄▃▃▃▂▂▂▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0071 | <= 1.2 levels | at frame 2 |
| EV slope | INFO | 0.0068 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 2.0416 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
