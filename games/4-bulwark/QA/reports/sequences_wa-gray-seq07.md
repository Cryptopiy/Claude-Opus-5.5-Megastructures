# Frame sequences: wa-gray-seq07

Sequences: 3. Checks: 4 PASS, 4 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_b02 | transition | 601 | 1.546 | 0.832 | 2.358 | 132.3 | FAIL | luma step/frame |
| s0880_0960 | transition | 961 | 3.546 | 1.401 | 7.180 | 194.8 | FAIL | luma step/frame |
| s1405_1450 | transition | 541 | 46.287 | 0.874 | 46.499 | 210.0 | FAIL | luma step/frame, diff spikes |

## b01_b02 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-gray-seq07\seq_b01_b02`, 601 frames at 30 fps, kind transition

- luma      `▂▁▁▁▁▁▁▁▂▂▃▃▃▃▃▃▄▄▄▄▅▅▅▅▆▆▆▇████████████████████████████████`
- mean diff `▂▂▂▁▁▁▂▂▃▃▃▃▃▂▂▂▃▂▂▃▃▂▃▄▄▅▆█▇▁▁▁▁▁▁▂▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▄▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.5464 | <= 1.2 levels | at frame 285 |
| activation-frame diff | PASS | 0.0124 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 300 |
| EV slope | INFO | 1.0402 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## s0880_0960 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-gray-seq07\seq_s0880_0960`, 961 frames at 30 fps, kind transition

- luma      `████████████████████████▇▇▇▆▅▄▃▃▃▃▃▃▃▃▂▂▂▂▁▁▁▃▂▁▁▂▃▂▁▁▂▃▂▁▁▂`
- mean diff `▁▁▁▁▁▁▁▂▂▂▂▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▃▅▆▃▂▁▁▁▁▁▁▁▁▁▂▂▂▃▄█▃▁▁▃█▂▁▁▄█▂▂▂▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 3.5462 | <= 1.2 levels | at frame 459 |
| activation-frame diff | PASS | 0.0203 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 481 |
| EV slope | INFO | 4.8364 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## s1405_1450 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-gray-seq07\seq_s1405_1450`, 541 frames at 30 fps, kind transition

- luma      `▅▅▅▆▇▇▆▆▅▅▅▆▆▆▅▄▃▃▃▂▂▂▁████████████████▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁█▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 46.2871 | <= 1.2 levels | at frame 215 |
| EV slope | INFO | 87.3876 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | FAIL | 1 | 0 above 3x running median | frames [215] |
