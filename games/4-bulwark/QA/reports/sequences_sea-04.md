# Frame sequences: sea-04

Sequences: 7. Checks: 21 PASS, 3 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_b02 | transition | 601 | 2.189 | 0.878 | 2.849 | 148.5 | FAIL | luma step/frame |
| b02_b03 | transition | 601 | 0.188 | 1.284 | 3.257 | 149.5 | PASS |  |
| pan_KS04_sea | pan | 30 | 0.002 | 0.191 | 0.193 | 35.5 | PASS |  |
| pan_b02_Crest | pan | 30 | 0.004 | 0.244 | 0.246 | 95.2 | PASS |  |
| plumes_KS04 | static | 241 | 1.009 | 3.236 | 3.691 | 114.9 | FAIL | flicker block std |
| static_KS04 | static | 120 | 0.112 | 0.728 | 0.991 | 101.2 | FAIL | flicker block std |
| static_b02_Crest | static | 120 | 0.003 | 0.012 | 0.017 | 19.3 | PASS |  |

## b01_b02 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-04\seq_b01_b02`, 601 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▁▁▁▁▁▁▁▂▂▃▄▅▇████████████████████████████████`
- mean diff `▁▁▁▁▂▃▂▁▂▂▂▂▂▂▁▁▁▁▁▂▂▁▁▂▃▅▇█▇▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▃▃▃▃▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 2.1885 | <= 1.2 levels | at frame 275 |
| activation-frame diff | PASS | 0.0040 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.5448 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b02_b03 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-04\seq_b02_b03`, 601 frames at 30 fps, kind transition

- luma      `████▇▇▇▇▇▇▇▇▆▆▆▆▆▆▆▆▆▆▆▆▆▅▅▄▄▄▄▄▄▄▄▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▂▂▂▂▂`
- mean diff `▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▁▂▂▁▁▁▂▂▂▂▂▃▃▃▃▄▄▄▅▅▅▅▆▆▅▅▃▂▃▄▆█████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1876 | <= 1.2 levels | at frame 556 |
| activation-frame diff | PASS | 0.0005 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 300 |
| EV slope | INFO | 0.1105 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-04\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▂▂▂▂▂▃▃▃▃▃▄▄▄▄▅▅▅▅▆▆▆▆▆▇▇▇███`
- mean diff `▇▂▇▆▄▆▇▄▃▄█▇▁▆▁▅▇▃▃▃▅▆▃▂▄▄▁▅▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0016 | <= 1.2 levels | at frame 17 |
| EV slope | INFO | 0.0016 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.0207 | <= 2 levels | exact rotation homography; raw 1.021, control floor 0.753, low-pass (sigma 1.5) 0.321, block-mean 0.107; gate block [256, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-04\seq_pan_b02_Crest`, 30 frames at 30 fps, kind pan

- luma      `███▇▇▇▇▆▆▆▆▅▅▅▅▅▄▄▃▃▃▃▃▂▂▂▂▁▁▁`
- mean diff `▄▃▆▃▄▅▅▃▄▄▅▆▅▁▇▃▆▆▅▅▃▅▆█▅▇▄▆▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0039 | <= 1.2 levels | at frame 3 |
| EV slope | INFO | 0.0028 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.1978 | <= 2 levels | exact rotation homography; raw 1.625, control floor 1.162, low-pass (sigma 1.5) 0.387, block-mean 0.135; gate block [960, 512] (raw); 2 of 420 blocks in the fine-pattern regime |

## plumes_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-04\seq_plumes_KS04`, 241 frames at 4 fps, kind static

- luma      `▁▅▆██▇▅▄▄▃▃▄▄███▆▃▂▅▅▆▇█▇▅▄▆▇▄▅▄▅▇▇▅▄▂▆▆██▆▆▆▅▄▃▂▃▆▇▅▄▅▆▇▅▃▂`
- mean diff `▁▇█▆▆▇▄▃▄▄▅▆▄▆▅▅▅▃▁▆▇▃▇▇▄▅▄▄▃▅▅▄▆▅▅▅▅▂█▆▇▄▄▅▆▃▅▄▃▃▇▇▄▄▃▅█▆▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 1.0090 | <= 1.2 levels | at frame 153 |
| EV slope | INFO | 0.1092 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 10.9966 | <= 3 levels | 241 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-04\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▃▃▃▃▃▄▄▄▄▄▄▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▆▆▆▆▆▆▇▇▇▇▇▇▇▇███████`
- mean diff `▂▁▁▁▁▁▁▁▁▁▁▂▂▃▄▃▃▄▄▅▅▅▆▆▆▆▇▇▇█▇▇▆▆▆▆▆▆▆▆▆▆▆▆▆▆▅▅▅▅▅▅▄▄▄▄▄▃▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1123 | <= 1.2 levels | at frame 58 |
| EV slope | INFO | 0.1011 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 16.0947 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\sea-04\seq_static_b02_Crest`, 120 frames at 30 fps, kind static

- luma      `▃▃▂▃▃▃▄▅▅▅▆▆▅▅▅▅▅▅▅▅▅▆▆▇▇▇▇▇██████████▇▇▆▆▆▅▅▅▄▄▃▃▃▃▃▃▂▂▂▂▁▁`
- mean diff `▂▂▂▃▄▅▅▆▇▇███▇▆▅▄▃▃▄▄▅▅▆▆▆▆▆▆▆▅▅▅▅▅▅▅▄▄▄▇▃▃▃▃▃▃▃▃▃▂▂▂▂▂▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0032 | <= 1.2 levels | at frame 12 |
| EV slope | INFO | 0.0031 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 2.6248 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
