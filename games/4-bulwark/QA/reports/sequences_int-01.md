# Frame sequences: int-01

Sequences: 5. Checks: 12 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_b02 | transition | 601 | 2.188 | 0.878 | 2.849 | 148.5 | FAIL | luma step/frame |
| b04_b05 | transition | 601 | 4.689 | 1.441 | 6.822 | 227.7 | FAIL | luma step/frame |
| b05_b06 | transition | 601 | 0.507 | 0.716 | 1.076 | 164.1 | PASS |  |
| b06_b07 | transition | 601 | 0.582 | 1.155 | 3.448 | 192.9 | PASS |  |
| lamps_b01 | transition | 289 | 0.491 | 0.755 | 1.282 | 139.9 | PASS |  |

## b01_b02 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\seq_b01_b02`, 601 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▁▁▁▁▁▁▁▂▂▃▄▅▇████████████████████████████████`
- mean diff `▁▁▁▁▂▃▂▁▂▂▂▂▂▂▁▁▁▁▁▂▂▁▁▂▃▅▇█▇▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▃▃▃▃▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 2.1883 | <= 1.2 levels | at frame 275 |
| activation-frame diff | PASS | 0.0041 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.5398 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b04_b05 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\seq_b04_b05`, 601 frames at 30 fps, kind transition

- luma      `██████████████████▇▇▇▇▇▇▆▆▅▄▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▃▃▃▂▂▂`
- mean diff `▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▄▃▄▅▆█▄▃▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▄▄█▆▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 4.6888 | <= 1.2 levels | at frame 279 |
| activation-frame diff | PASS | 0.0066 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 8.3414 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b05_b06 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\seq_b05_b06`, 601 frames at 30 fps, kind transition

- luma      `▆▅▄▄▅▇███▇▆▅▄▄▆▇██▇▆▄▃▂▁▂▃▄▅▆▇▇▇▇▇▇▇▇▆▅▅▅▄▄▄▄▄▃▃▃▃▂▂▂▁▁▁▂▂▃▃`
- mean diff `▅▅▅▅▆▇██▇▅▆▅▅▆▇▇▇█▆▅▅▅▄▅▇█▆▅▆▅▃▃▃▄▅▅▄▂▁▁▁▁▁▁▁▁▁▁▂▂▃▃▄▅▃▄▄▅▆▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5071 | <= 1.2 levels | at frame 253 |
| activation-frame diff | PASS | 0.0004 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 300 |
| EV slope | INFO | 1.1068 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b06_b07 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\seq_b06_b07`, 601 frames at 30 fps, kind transition

- luma      `▆▆▆▆▆▆▆▇▇▇███▇▆▆▆▇▇▆▆▆▆▆▆▅▅▅▄▄▄▄▆▇▆▅▃▁▂▄▆▆▅▃▁▁▃▅▅▅▃▁▁▃▅▆▅▃▂▁`
- mean diff `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▄▄▅▄▄▃▃▃▃▄▆██▃▂▃▃▃▄▄▃▂▂▃▃▄▄▃▂▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5820 | <= 1.2 levels | at frame 403 |
| activation-frame diff | PASS | 0.0302 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.4912 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## lamps_b01 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\seq_lamps_b01`, 289 frames at 30 fps, kind transition

- luma      `▅▆▆▇███▇▇▆▅▄▃▃▂▂▁▁▁▁▁▁▁▂▂▂▃▃▃▄▅▆▆▇███▇▆▆▅▄▃▃▂▂▁▁▁▁▁▁▂▂▃▂▂▃▃▄`
- mean diff `▄▅▆▅▅▅▄▅▅▄▄▃▂▂▂▂▂▂▁▁▁▂▂▃▅▄▂▂▃▃▄▄▅▅▅▅▄▅▅▄▄▃▃▂▂▂▂▂▁▂▃▄▅█▇▄▃▂▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.4912 | <= 1.2 levels | at frame 184 |
| EV slope | INFO | 1.4849 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
