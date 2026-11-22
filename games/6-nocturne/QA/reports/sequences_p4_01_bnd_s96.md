# Frame sequences: p4_01_bnd_s96

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| s96 | boundary | 681 | 0.0025 | 1.223 | 3.766 | 209.0 | PASS |  |

## s96 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_bnd_s96\seq_s96`, 681 frames at 30 fps, kind boundary

- luma      `▇▄▃▃▃▂▂▃▃▃▃▄▄▅▅▆▇▇██▇▄▃▂▂▂▁▁▁▁▁▁▁▁▁▂▂▃▃▃▃▃▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄`
- mean diff `▆▆▄▃▃▃▃▃▃▃▃▃▄▄▅▆▇██▇▆▆▅▄▃▂▂▂▁▁▁▁▁▁▁▁▁▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6280 | <= 1.2 levels | at frame 234 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [120.0], worst at frame None |
