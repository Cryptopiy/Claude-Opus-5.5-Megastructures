# Frame sequences: lookv2_01bd_seq_s1240

Sequences: 1. Checks: 2 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| s1240 | boundary | 1201 | 0.0003 | 2.412 | 4.234 | 205.1 | FAIL | activation-frame diff |

## s1240 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01bd_seq_s1240\seq_s1240`, 1201 frames at 30 fps, kind boundary

- luma      `▆▆▅▅▅▅▄▄▄▃▃▃▂▂▁▁▁▁▁▂▂▃▃▄▄▄▄▄▄▅▅▅▆▆▆▆▇▇▇▇▇▇▇█████████████████`
- mean diff `▄▄▄▄▄▄▄▄▄▄▄▄▄▄▅▅▅▆▆▇██████▇▇▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0850 | <= 1.2 levels | at frame 340 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | FAIL | 0.5382 | <= 0.5 level | boundaries [1260.0], worst at frame 400 |
