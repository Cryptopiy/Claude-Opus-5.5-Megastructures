# Frame sequences: sea-06

Sequences: 7. Checks: 19 PASS, 5 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_b02 | transition | 601 | 2.084 | 0.870 | 2.792 | 148.5 | FAIL | luma step/frame |
| b02_b03 | transition | 601 | 0.273 | 1.267 | 3.209 | 149.5 | PASS |  |
| pan_KS04_sea | pan | 30 | 0.001 | 0.192 | 0.194 | 34.4 | PASS |  |
| pan_b02_Crest | pan | 30 | 0.003 | 0.242 | 0.244 | 95.2 | PASS |  |
| plumes_KS04 | static | 241 | 2.799 | 3.298 | 5.436 | 111.7 | FAIL | luma step/frame, flicker block std |
| static_KS04 | static | 120 | 0.064 | 0.733 | 0.906 | 86.0 | FAIL | flicker block std |
| static_b02_Crest | static | 120 | 0.021 | 0.016 | 0.037 | 19.2 | FAIL | flicker block std |

## b01_b02 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-06\seq_b01_b02`, 601 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▁▁▁▂▂▂▂▂▃▃▄▆▇████████████████████████████████`
- mean diff `▁▁▁▁▂▃▂▁▂▂▂▂▂▂▁▁▁▁▁▁▁▂▂▃▄▅▇█▇▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▃▃▄▄▂▂▂▂▃▃▃▃▄▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 2.0840 | <= 1.2 levels | at frame 275 |
| activation-frame diff | PASS | 0.0000 | <= 0.5 levels over the local median | 2 boundary frames |
| EV slope | INFO | 1.9556 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b02_b03 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-06\seq_b02_b03`, 601 frames at 30 fps, kind transition

- luma      `██████▇▇▇▇▇▇▆▆▆▆▆▆▅▆▆▆▆▆▆▅▅▄▄▄▄▄▄▄▄▃▃▃▂▂▂▂▂▂▂▂▂▃▃▂▂▁▁▁▁▂▂▂▂▂`
- mean diff `▂▂▂▂▃▂▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▁▂▂▁▁▁▂▂▂▂▂▂▃▃▃▄▄▄▅▅▅▅▅▅▅▄▃▂▃▄▆█████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2733 | <= 1.2 levels | at frame 509 |
| activation-frame diff | PASS | 0.0000 | <= 0.5 levels over the local median | 2 boundary frames |
| EV slope | INFO | 0.1520 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-06\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▁▁▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▆▆▆▆██`
- mean diff `▇▅▇▅▄█▆▅▄▆▇▇▁█▄▆▅▅▄▅▅▆▄▆▄▆▂▆▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0007 | <= 1.2 levels | at frame 24 |
| EV slope | INFO | 0.0008 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 0.9120 | <= 2 levels | exact rotation homography; raw 0.912, control floor 0.720, low-pass (sigma 1.5) 0.272, block-mean 0.106; gate block [1664, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-06\seq_pan_b02_Crest`, 30 frames at 30 fps, kind pan

- luma      `▅▅▅▁▅▅▅▅▅▅▅▅▅▅█▅██▅█▅█▅██▅█▅█▅`
- mean diff `▄▃▅▂▄▅▆▁▃▄▅▅▅▂▇▂▇▄▅▅▃▅▇█▄▅▆▆▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0028 | <= 1.2 levels | at frame 14 |
| EV slope | INFO | 0.0030 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.2783 | <= 2 levels | exact rotation homography; raw 1.628, control floor 1.146, low-pass (sigma 1.5) 0.403, block-mean 0.135; gate block [832, 512] (raw); 1 of 420 blocks in the fine-pattern regime |

## plumes_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-06\seq_plumes_KS04`, 241 frames at 4 fps, kind static

- luma      `▁▆▆██▇▅▄▄▃▃▄▄▇▇█▆▃▂▄▅▅▇▇▆▄▄▅▆▄▄▄▆▇▇▅▄▂▃▆▇▇▅▆▆▄▄▃▂▃▆▇▅▄▅▆▆▅▂▂`
- mean diff `▁▃▃▃▃▃▂▂▂▂▂▂▂▃▂▂▂▂▁▃▃▂▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂█▇▃▃▂▂▂▃▂▂▂▂▂▃▃▂▂▂▂▃▃▁▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 2.7993 | <= 1.2 levels | at frame 152 |
| EV slope | INFO | 0.3766 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 11.5908 | <= 3 levels | 241 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-06\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▂▂▂▂▂▂▂▃▃▃▃▄▄▄▄▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇▇▇██████████`
- mean diff `▂▂▂▂▁▁▁▁▂▁▁▂▂▃▃▃▄▄▅▅▆▆▇▇▇█████████▇▇▇▇▇▇▇▇▇▇▇▆▆▆▆▅▅▅▅▅▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0644 | <= 1.2 levels | at frame 7 |
| EV slope | INFO | 0.0639 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 14.5262 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b02_Crest (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-06\seq_static_b02_Crest`, 120 frames at 30 fps, kind static

- luma      `▇▆▅▄▂▁▁▁▂▂▂▃▄▄▄▄▄▄▃▂▂▂▂▂▃▃▄▅▅▆▆▆▇▇▇▇████████████████████████`
- mean diff `▂▄▇▇██▇▆▄▄▄▄▃▃▂▄▃▄▅▅▅▄▄▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0210 | <= 1.2 levels | at frame 6 |
| EV slope | INFO | 0.0211 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 5.5866 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
