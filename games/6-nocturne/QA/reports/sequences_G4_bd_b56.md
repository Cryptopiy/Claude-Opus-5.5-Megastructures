# Frame sequences: G4_bd_b56

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b56 | boundary | 601 | 0.0009 | 2.149 | 2.727 | 216.0 | PASS |  |

## b56 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G4_bd_b56\seq_b56`, 601 frames at 30 fps, kind boundary

- luma      `█▇▆▅▄▃▃▂▂▂▂▂▂▂▃▃▄▄▄▄▄▄▄▅▅▆▆▆▆▆▅▅▅▄▄▃▃▂▂▁▁▁▁▂▂▂▃▃▄▄▄▅▅▆▇▇▆▅▅▄`
- mean diff `▆▆▆▆▇▆▆▆▆▅▅▅▅▅▅▅▅▅▆▆▇▇████▇▇▇████▇▇▇▇▇▆▆▆▅▅▅▄▄▄▄▃▃▂▂▂▂▁▁▁▁▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2350 | <= 1.2 levels | at frame 540 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0486 | <= 0.5 level | boundaries [1080.0], worst at frame 300 |
