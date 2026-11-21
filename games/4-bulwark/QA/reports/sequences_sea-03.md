# Frame sequences: sea-03

Sequences: 5. Checks: 14 PASS, 4 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| pan_KS04_sea | pan | 30 | 0.002 | 0.195 | 0.196 | 36.4 | PASS |  |
| pan_b02_Crest | pan | 30 | 0.004 | 0.232 | 0.235 | 95.2 | PASS |  |
| plumes_KS04 | static | 241 | 1.423 | 3.211 | 3.850 | 115.1 | FAIL | luma step/frame, flicker block std |
| static_KS04 | static | 120 | 0.053 | 0.722 | 0.928 | 101.2 | FAIL | flicker block std |
| static_b02_Crest | static | 120 | 0.004 | 0.012 | 0.016 | 19.3 | FAIL | flicker block std |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-03\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▁▂▂▂▂▃▃▃▄▄▄▄▄▄▅▅▅▆▆▆▇▇▇▇▇██`
- mean diff `▇▂█▆▄▆▇▄▃▃█▇▁▆▁▅▇▄▂▃▅▆▃▂▄▄▁▅▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0017 | <= 1.2 levels | at frame 17 |
| EV slope | INFO | 0.0016 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.0164 | <= 2 levels | exact rotation homography; raw 1.016, control floor 0.753, low-pass (sigma 1.5) 0.321, block-mean 0.108; gate block [256, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-03\seq_pan_b02_Crest`, 30 frames at 30 fps, kind pan

- luma      `███▇▇▇▇▆▆▆▅▅▅▅▅▄▄▄▃▄▃▃▃▃▂▂▂▁▁▁`
- mean diff `▃▃▅▂▄▃▅▁▃▃▃▄▄▁▅▃▃▇▃▄▂▅▃█▃▆▄▅▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0036 | <= 1.2 levels | at frame 3 |
| EV slope | INFO | 0.0023 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.4160 | <= 2 levels | exact rotation homography; raw 1.416, control floor 0.986, low-pass (sigma 1.5) 0.340, block-mean 0.135; gate block [896, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## plumes_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-03\seq_plumes_KS04`, 241 frames at 4 fps, kind static

- luma      `▁▄▆██▇▄▄▄▃▃▄▄███▆▃▂▄▅▆██▇▄▄▆▆▄▅▄▅▇▇▅▄▂█▆██▆▆▆▅▄▃▂▃▆▇▅▄▅▆▇▅▃▂`
- mean diff `▁▆▆▅▅▆▄▃▄▃▅▅▃▅▄▅▄▂▁▅▆▃▅▆▄▄▄▃▃▄▄▄▅▄▅▄▄▂█▅▅▄▃▄▅▃▄▄▃▃▆▆▄▃▃▄▆▅▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.4229 | <= 1.2 levels | at frame 153 |
| EV slope | INFO | 0.1550 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 10.9980 | <= 3 levels | 241 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-03\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▃▃▃▃▃▄▄▄▄▄▄▅▅▅▅▅▅▅▅▅▅▅▅▅▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇███████`
- mean diff `▂▂▁▁▁▁▁▁▁▁▁▂▂▃▄▃▄▄▅▅▆▆▆▇▇▇▇▇▇█▇▇▇▇▇▇▇▆▆▇▇▇▇▆▆▆▆▆▅▅▅▅▅▅▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0526 | <= 1.2 levels | at frame 24 |
| EV slope | INFO | 0.0461 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 16.0948 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b02_Crest (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-03\seq_static_b02_Crest`, 120 frames at 30 fps, kind static

- luma      `▃▃▃▃▃▄▅▅▆▆▆▆▆▆▆▆▅▅▅▅▆▆▆▇▇███████████▇▇▇▆▆▆▅▅▅▄▄▃▃▃▃▂▂▂▂▂▂▁▁▁`
- mean diff `▂▂▃▄▅▆▆▆▇▇▇██▇▆▅▄▃▃▄▄▅▅▆▆▆▆▆▆▆▅▅▅▆▅▅▅▅▄▄▆▄▄▃▃▃▃▃▃▃▃▂▂▂▂▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0041 | <= 1.2 levels | at frame 12 |
| EV slope | INFO | 0.0042 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 3.5260 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
