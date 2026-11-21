# Frame sequences: dusk-final

Sequences: 5. Checks: 14 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b06_b07 | transition | 601 | 0.610 | 1.160 | 3.541 | 192.9 | PASS |  |
| pan_KS20 | pan | 30 | 0.012 | 0.301 | 0.313 | 149.9 | PASS |  |
| pan_s1470_city | pan | 30 | 0.013 | 0.309 | 0.322 | 155.2 | PASS |  |
| s1405_1450 | transition | 541 | 1.301 | 1.036 | 2.529 | 193.9 | FAIL | luma step/frame |
| static_KS20 | static | 120 | 0.001 | 0.002 | 0.003 | 50.3 | PASS |  |

## b06_b07 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-final\seq_b06_b07`, 601 frames at 30 fps, kind transition

- luma      `▆▆▆▆▆▆▆▇▇▇███▇▆▆▆▆▆▆▆▆▆▆▅▅▅▅▄▄▄▄▆▇▇▅▃▁▂▄▆▆▅▃▁▁▃▅▆▅▃▁▁▃▅▆▅▃▂▁`
- mean diff `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▂▃▃▄▄▅▃▄▃▂▃▃▃▆██▃▂▃▃▃▄▄▃▂▂▃▃▄▄▃▂▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6104 | <= 1.2 levels | at frame 403 |
| activation-frame diff | PASS | 0.0300 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.4855 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## pan_KS20 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-final\seq_pan_KS20`, 30 frames at 30 fps, kind pan

- luma      `███▇▇▇▇▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁▁`
- mean diff `▄▇▃▇▃▇▄▇▁█▄▆▃▇▃▄▅▇▄▆▅▄▆▅▃▇▅▄▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0122 | <= 1.2 levels | at frame 18 |
| EV slope | INFO | 0.0145 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 0.9953 | <= 1.5 levels | exact rotation homography; raw 2.646, control floor 1.571, low-pass (sigma 1.5) 0.586, block-mean 0.194; gate block [1408, 448] (raw); 8 of 420 blocks in the fine-pattern regime; strict (beat 6 / ledge) |

## pan_s1470_city (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-final\seq_pan_s1470_city`, 30 frames at 30 fps, kind pan

- luma      `███▇▇▇▇▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁▁`
- mean diff `▃▃▆▄▅▂▆▂▃▆▇▂▄▃▇▂▅▅▁█▂▆▄▄▃▄▄▅▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0132 | <= 1.2 levels | at frame 4 |
| EV slope | INFO | 0.0159 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.1377 | <= 1.5 levels | exact rotation homography; raw 2.221, control floor 1.260, low-pass (sigma 1.5) 0.590, block-mean 0.328; gate block [1152, 512] (raw); 8 of 420 blocks in the fine-pattern regime; strict (beat 6 / ledge) |

## s1405_1450 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-final\seq_s1405_1450`, 541 frames at 30 fps, kind transition

- luma      `▁▁▁▂▂▃▃▂▂▂▂▃▄▅▅▆▆▆▆▆▆▆▇▇▇████████████▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇`
- mean diff `▂▂▃▄▅▅▄▃▃▃▃▄▅▆▆█▁▂▂▁▂▂▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▁▁▁▁▁▂▂▂▂▁▁▂▁▂▂▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.3014 | <= 1.2 levels | at frame 138 |
| EV slope | INFO | 1.9982 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## static_KS20 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-final\seq_static_KS20`, 120 frames at 30 fps, kind static

- luma      `▆█▆▆▆▆▆▆▆▆▃▃▃▃▃▆▆▃▃▃▁▁▁▁▁▁▁▃▃▆▆▆███▆▆▆▃▃▃▃▃▃▁▃▃▃▆▆▆▃▃▃▃▆▆▃▃▃`
- mean diff `▁▂▄▆▇▇▅▄▅▆▆▅▄▄▃▁▄▄▄▄▃▄▄▄▃▄▆▅▅▄▄▄▆▄▄▄▆▇██▇▅▄▆▆▅▄▄▄▄▄▅████▇▆▇█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0007 | <= 1.2 levels | at frame 57 |
| EV slope | INFO | 0.0005 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.8289 | <= 1.5 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
