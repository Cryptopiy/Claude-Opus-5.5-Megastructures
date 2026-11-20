# Frame sequences: G4_bd_b67

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b67 | boundary | 601 | 0.0004 | 3.712 | 4.789 | 218.4 | PASS |  |

## b67 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G4_bd_b67\seq_b67`, 601 frames at 30 fps, kind boundary

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▃▃▃▃▄▄▄▄▅▅▅▆▆▆▇▇▇▇▇▇▇▇█████████`
- mean diff `▁▁▁▁▁▁▂▂▂▂▂▃▃▃▃▃▃▃▃▅▅▆▅▆▆▆▇▇▇███▇▇▇▇▇▇▇▇▆▆▅▅▅▅▅▅▅▅▄▄▄▄▄▄▃▃▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1130 | <= 1.2 levels | at frame 240 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.2825 | <= 0.5 level | boundaries [1260.0], worst at frame 300 |
