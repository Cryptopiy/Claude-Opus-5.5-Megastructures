# Frame sequences: lookv2_01bd_seq_b12

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b12 | boundary | 601 | 0.0023 | 0.944 | 3.581 | 209.1 | PASS |  |

## b12 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01bd_seq_b12\seq_b12`, 601 frames at 30 fps, kind boundary

- luma      `▇▇███▆▄▃▂▂▂▂▁▁▁▁▁▁▁▁▁▁▂▂▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄`
- mean diff `▇███▆▆▅▅▄▃▃▂▂▂▁▁▁▁▁▁▁▁▁▁▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5950 | <= 1.2 levels | at frame 50 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0134 | <= 0.5 level | boundaries [120.0], worst at frame 300 |
