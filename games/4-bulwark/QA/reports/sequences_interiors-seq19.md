# Frame sequences: interiors-seq19

Sequences: 3. Checks: 6 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_b02 | transition | 601 | 2.160 | 1.309 | 4.914 | 182.0 | FAIL | luma step/frame |
| b04_b05 | transition | 601 | 2.551 | 2.428 | 6.526 | 205.2 | FAIL | luma step/frame |
| lamps_b01 | transition | 289 | 0.675 | 1.133 | 2.223 | 184.8 | PASS |  |

## b01_b02 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-seq19\seq_b01_b02`, 601 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▁▂▂▂▂▃▃▄▄▄▅▆▇████████████████████████████████`
- mean diff `▂▂▃▂▃▄▃▂▃▃▂▂▂▂▂▂▃▂▃▄▅▄▅▆▄▆█▇▄▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▁▁▁▁▂▂▂▂▂▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 2.1598 | <= 1.2 levels | at frame 270 |
| activation-frame diff | PASS | 0.0003 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.4327 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b04_b05 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-seq19\seq_b04_b05`, 601 frames at 30 fps, kind transition

- luma      `████████████████████████▇▇▆▆▅▄▄▄▄▃▃▃▂▂▂▂▃▃▃▃▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁`
- mean diff `▃▄▄▃▃▃▄▄▄▅▃▃▃▄▃▄▄▄▅▅▆▆▆▇▆▇██▅▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▂▂▃▄▅▅▂▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 2.5515 | <= 1.2 levels | at frame 278 |
| activation-frame diff | PASS | 0.0713 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 5.5712 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## lamps_b01 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-seq19\seq_lamps_b01`, 289 frames at 30 fps, kind transition

- luma      `▅▆▇▇████▇▆▅▅▄▃▂▂▂▁▁▁▂▂▂▃▃▃▃▃▃▄▅▆▇▇███▇▇▆▅▅▄▃▂▂▁▁▁▁▁▂▂▃▃▃▃▃▃▄`
- mean diff `▄▅▅▂▃▃▃▃▂▂▂▂▂▂▂▂▃▃▁▂▂▄▅▆▇▅▂▂▂▃▄▅▅▂▃▃▃▃▂▂▂▂▂▂▂▂▃▃▁▂▃▄▆▇█▆▃▂▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6751 | <= 1.2 levels | at frame 155 |
| EV slope | INFO | 1.5014 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
