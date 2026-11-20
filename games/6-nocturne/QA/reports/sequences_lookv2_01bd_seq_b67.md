# Frame sequences: lookv2_01bd_seq_b67

Sequences: 1. Checks: 2 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b67 | boundary | 601 | 0.0003 | 3.100 | 4.240 | 205.0 | FAIL | activation-frame diff |

## b67 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01bd_seq_b67\seq_b67`, 601 frames at 30 fps, kind boundary

- luma      `▆▆▅▅▅▅▅▅▄▄▄▄▃▃▃▂▂▂▂▁▁▁▁▁▁▁▂▂▂▂▂▃▃▄▄▄▄▅▅▅▅▅▅▅▆▆▆▆▆▆▆▇▇▇▇▇████`
- mean diff `▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▃▃▄▄▄▄▄▅▅▆▇▇▇▇▇██████▇▇▆▆▆▆▅▅▅▅▅▅▅▅▅▅▅▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0820 | <= 1.2 levels | at frame 240 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | FAIL | 0.5426 | <= 0.5 level | boundaries [1260.0], worst at frame 300 |
