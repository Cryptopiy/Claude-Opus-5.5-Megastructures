# Frame sequences: lookv2_01bd_seq_b34

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b34 | boundary | 601 | 0.0032 | 1.147 | 2.558 | 223.7 | PASS |  |

## b34 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01bd_seq_b34\seq_b34`, 601 frames at 30 fps, kind boundary

- luma      `▁▁▂▂▂▃▄▆▇██▅▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▃▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▃▃▃▃▄▄▅▆▇██▇▆▄▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▃▃▃▃▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.8130 | <= 1.2 levels | at frame 109 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0070 | <= 0.5 level | boundaries [560.0], worst at frame 300 |
