# Frame sequences: lookv2_01bd_seq_s96

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| s96 | boundary | 681 | 0.0023 | 1.190 | 3.587 | 209.1 | PASS |  |

## s96 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01bd_seq_s96\seq_s96`, 681 frames at 30 fps, kind boundary

- luma      `▇▄▃▃▃▃▃▃▃▃▃▄▅▅▅▆▇███▇▄▃▂▂▂▂▁▁▁▁▁▁▁▁▂▂▃▃▃▃▃▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄`
- mean diff `▆▆▄▄▃▃▃▃▃▃▃▃▄▅▆▆▇██▇▆▆▅▄▃▃▂▂▁▁▁▁▁▁▁▁▁▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5920 | <= 1.2 levels | at frame 230 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0110 | <= 0.5 level | boundaries [120.0], worst at frame 480 |
