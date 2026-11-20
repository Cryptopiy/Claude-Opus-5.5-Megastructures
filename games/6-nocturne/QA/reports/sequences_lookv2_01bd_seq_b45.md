# Frame sequences: lookv2_01bd_seq_b45

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b45 | boundary | 601 | 0.0001 | 0.544 | 0.978 | 148.2 | PASS |  |

## b45 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01bd_seq_b45\seq_b45`, 601 frames at 30 fps, kind boundary

- luma      `██▇▇▆▆▆▅▅▅▅▅▄▄▄▄▄▄▃▃▃▃▄▄▄▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `█▇▇▆▆▆▅▅▅▅▄▄▄▄▃▃▃▂▂▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0380 | <= 1.2 levels | at frame 5 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [880.0], worst at frame None |
