# Frame sequences: kraken-show27

Sequences: 9. Checks: 16 PASS, 6 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| show_k1 | transition | 140 | 9.670 | 2.321 | 10.906 | 96.8 | FAIL | luma step/frame, diff spikes |
| show_k2 | transition | 288 | 0.761 | 0.148 | 1.398 | 64.5 | FAIL | diff spikes |
| show_k2_30 | transition | 72 | 0.082 | 0.035 | 0.113 | 31.3 | PASS |  |
| show_k3 | transition | 128 | 0.179 | 0.121 | 0.273 | 46.8 | PASS |  |
| show_k4 | transition | 216 | 25.280 | 2.328 | 26.854 | 187.8 | FAIL | luma step/frame, diff spikes |
| show_k4_30 | transition | 72 | 0.256 | 0.337 | 0.564 | 157.7 | PASS |  |
| show_k5 | transition | 72 | 1.034 | 1.062 | 2.745 | 200.4 | PASS |  |
| show_k5_30 | transition | 120 | 0.258 | 0.151 | 0.446 | 190.0 | FAIL | diff spikes |
| show_stinger | static | 72 | 0.000 | 0.013 | 0.014 | 17.5 | PASS |  |

## show_k1 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-show27\seq_show_k1`, 140 frames at 5 fps, kind transition

- luma      `▆▆▅▅▄▄▃▃▃▂▂▂▂▂▂▂▂▂▂▂█▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂`
- mean diff `▁▁▁▁▁▁▁▁▁▂▁▂▂▁▁▁▁▁▂▁█▇▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 9.6703 | <= 1.2 levels | at frame 48 |
| activation-frame diff | PASS | 0.0000 | <= 0.5 levels over the local median | 2 boundary frames |
| EV slope | INFO | 0.9429 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | FAIL | 2 | 0 above 3x running median | frames [48, 49] |

## show_k2 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-show27\seq_show_k2`, 288 frames at 4 fps, kind transition

- luma      `████████████████████████████████████▇▇▆▅▄▄▄▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▁▁▂▃▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▃▃█▅▅▂▂▁▁▁▁▂▁▁▁▁▁▁▁▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.7605 | <= 1.2 levels | at frame 203 |
| activation-frame diff | PASS | 0.0000 | <= 0.5 levels over the local median | 2 boundary frames |
| EV slope | INFO | 0.0688 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | FAIL | 2 | 0 above 3x running median | frames [91, 195] |

## show_k2_30 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-show27\seq_show_k2_30`, 72 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▃▆▇▇▇██▇▆▆▆▅▅▆▆▆▆`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▃▇█▇▆▅▆▇▆▅▄▃▃▃▇▃▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0825 | <= 1.2 levels | at frame 53 |
| EV slope | INFO | 0.0510 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## show_k3 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-show27\seq_show_k3`, 128 frames at 4 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▂▂▁▁▁▂▂▂▂▂▂▃▂▂▂▂▂▂▂▂▂▃▃▃▃▄▃▃▃▃▃▃▃▄▄▃▄▄▄▄▄▄▄▄▅▅▅▅▅▅▆█`
- mean diff `▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂██▃▂▂▃▂▂▂▂▂▂▂▂▂▅▃▂▂▂▁▁▂▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▄▄▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1791 | <= 2 levels | at frame 41, sun break |
| EV slope | INFO | 0.0175 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## show_k4 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-show27\seq_show_k4`, 216 frames at 3 fps, kind transition

- luma      `▁▂▂▂▃▂▂▂▂▂▁▁▂▃▃▃▃▂▁▂▃▄▅▃▂▂▂▂▂▂▂▂▂▂▂▁▁▁▂▂▂▂▂▂▁▁▂█▄▂▂▂▂▂▂▂▂▂▂▂`
- mean diff `▁▁▁▁▃▃▁▁▁▁▁▁▁▁▁▁▂▁▁▁▂▂▃▁▁▁▂▂▁▂▂▁▁▁▁▂▂▁▁▁▁▁▂▂▂▁▂█▄▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 25.2802 | <= 1.2 levels | at frame 170 |
| EV slope | INFO | 1.4568 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | FAIL | 8 | 0 above 3x running median | frames [17, 18, 79, 80, 170, 171, 172, 173] |

## show_k4_30 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-show27\seq_show_k4_30`, 72 frames at 30 fps, kind transition

- luma      `▆▆▆▆▅▅▅▅▅▄▄▃▃▂▂▂▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▃▃▃▄▄▄▄▅▅▅▆▆▇▇▇▇████████`
- mean diff `▅▄▃█▃▄▄▄▄▄▅▇▆▆▅▄▄▅▃▃▃▃▃▃▃▃▃▄▃▂▂▂▃▃▃▄▃▄▄▃▃▃▃▃▃▃▃▅▃▃▂▂▂▂▂▁▁▃▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2563 | <= 1.2 levels | at frame 4 |
| EV slope | INFO | 0.1699 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## show_k5 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-show27\seq_show_k5`, 72 frames at 3 fps, kind transition

- luma      `▇▇▇▇▇▇▇██▇▇▅▄▄▄▃▃▃▃▃▃▂▃▃▃▃▃▂▂▂▁▂▄▅▄▂▂▂▁▁▁▁▁▁▁▁▂▂▃▄▅▅▅▆▆▆▆▆█▇`
- mean diff `▁▁▁▂▁▁▃▃▃▃▄▃▃▃▃▃▃▃▃▃▃▄▄▃▃▂▂▃▃▂▂▂▆█▄▃▃▃▄▄▄▄▃▃▄▃▃▃▃▃▃▃▂▃▂▂▂▃▅▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 1.0344 | <= 1.2 levels | at frame 38 |
| EV slope | INFO | 0.0688 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## show_k5_30 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-show27\seq_show_k5_30`, 120 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▃▃▃▃▃▃▄▄▄▅▅▅▅▅▅▆▆▆▇▇▇▇███████`
- mean diff `▁▁▁▁▁▁▁█▁▁▁▁█▁▁▁▁▃▁▁▁▁▁▁▁▁▁▂▁▁▂▂▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2577 | <= 1.2 levels | at frame 24 |
| EV slope | INFO | 0.2348 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | FAIL | 2 | 0 above 3x running median | frames [14, 24] |

## show_stinger (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-show27\seq_show_stinger`, 72 frames at 2 fps, kind static

- luma      `███████████████████████████████████████████████████▁▁▁▁▁▁▁▁▁`
- mean diff `▅▅▃▄▄▅▃▅▄▅▄▄▃▅▄▄▅▅▅▅▅▅▆▆▅▆▆▆▇▆▇▇▆▆▇▆▆███████▆▇▇▆▃▄▃▂▂▂▁▂▃▃▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0004 | <= 1.2 levels | at frame 41 |
| EV slope | INFO | 0.0001 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.3998 | <= 1.5 levels | 72 frames, 64 px blocks (fewer than 120 frames) |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
