# Frame sequences: lookv2_01bd_seq_s380

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| s380 | boundary | 801 | 0.0004 | 0.641 | 0.765 | 220.2 | PASS |  |

## s380 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01bd_seq_s380\seq_s380`, 801 frames at 30 fps, kind boundary

- luma      `▁▂▂▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄▄▄▄▄▄▃▁▁▁▁▁▂▂▂▂▃▄▄▄▄▃▂▃▃▄▄▄▅▅▆▇████▅▃▃▃▃▂▃`
- mean diff `▇▇▆▆▄▄▂▂▁▂▂▂▃▄▃▃▃▄▃▄▄▄▄▃▃▂▃▃▄▃▄▄▄▄▆▅▆▆▅▅▄▅▅▅▅▅▆▇▇▇▇█▇▅▃▄▅▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0940 | <= 1.2 levels | at frame 305 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [400.0], worst at frame None |
