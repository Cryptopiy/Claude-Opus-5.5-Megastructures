# Frame sequences: seax-10s

Sequences: 5. Checks: 15 PASS, 3 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| pan_KS04_sea | pan | 30 | 0.002 | 0.199 | 0.201 | 34.4 | PASS |  |
| pan_b02_Crest | pan | 30 | 0.003 | 0.242 | 0.243 | 95.2 | PASS |  |
| plumes_KS04 | static | 241 | 1.313 | 3.306 | 3.736 | 111.7 | FAIL | luma step/frame, flicker block std |
| static_KS04 | static | 120 | 0.043 | 0.742 | 0.918 | 78.1 | FAIL | flicker block std |
| static_b02_Crest | static | 120 | 0.006 | 0.011 | 0.018 | 20.9 | PASS |  |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-10s\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▁▂▂▂▂▃▃▃▃▄▄▄▄▄▅▅▅▆▆▆▆▇▇████`
- mean diff `▇▄▇▅▄█▆▅▄▆▇█▁█▄▆▆▅▅▅▆▆▄▆▄▇▃▆▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0019 | <= 1.2 levels | at frame 26 |
| EV slope | INFO | 0.0019 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 0.9212 | <= 2 levels | exact rotation homography; raw 0.921, control floor 0.726, low-pass (sigma 1.5) 0.277, block-mean 0.107; gate block [1664, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-10s\seq_pan_b02_Crest`, 30 frames at 30 fps, kind pan

- luma      `▅▅▅▁▅▅▅▅▅▅▅▅▅▅█▅██▅█▅█▅████▅█▅`
- mean diff `▃▃▅▂▄▅▆▁▃▄▅▅▅▂▇▂▇▄▅▅▃▅▇█▄▅▆▆▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0025 | <= 1.2 levels | at frame 4 |
| EV slope | INFO | 0.0027 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.4067 | <= 2 levels | exact rotation homography; raw 1.407, control floor 0.993, low-pass (sigma 1.5) 0.346, block-mean 0.135; gate block [896, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## plumes_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-10s\seq_plumes_KS04`, 241 frames at 4 fps, kind static

- luma      `▁▆▇██▇▄▃▃▂▂▃▄▇▇█▅▂▁▄▅▅▇█▆▄▃▅▆▃▄▄▅▇▆▄▃▁▆▆▇▇▅▅▅▄▃▂▁▂▅▆▅▃▅▅▇▅▂▁`
- mean diff `▁██▆▆▇▅▃▅▄▆▆▄▆▄▅▅▃▂▆▇▄▆█▄▆▅▄▃▅▆▄▆▆▅▆▅▇█▆▆▄▄▆▇▄▅▅▄▄▇▇▄▃▃▅▇▆▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.3127 | <= 1.2 levels | at frame 153 |
| EV slope | INFO | 0.1325 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 11.4978 | <= 3 levels | 241 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-10s\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▃▃▃▃▄▄▄▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇▇▇██████████`
- mean diff `▂▂▁▁▁▁▁▁▁▁▂▂▂▃▃▄▄▅▅▆▆▇▇▇▇██████████▇▇▇▇▇▇▇▇▇▇▆▆▆▆▆▅▅▅▅▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0434 | <= 1.2 levels | at frame 35 |
| EV slope | INFO | 0.0320 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 14.8459 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-10s\seq_static_b02_Crest`, 120 frames at 30 fps, kind static

- luma      `▅▆▆▆▅▄▃▃▂▂▂▁▁▁▁▁▂▂▃▃▃▃▃▃▃▃▃▄▄▅▅▆▆▇▇▇████████▇▇▇▇▇▇▇▇▇▇▆▆▆▆▆▆`
- mean diff `▇█▇▆▇▆▅▃▂▂▂▂▂▂▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0058 | <= 1.2 levels | at frame 1 |
| EV slope | INFO | 0.0049 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 2.0416 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
