# Frame sequences: p4_01_signs

Sequences: 1. Checks: 2 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| signs_ks16 | signs | 1800 | 0.0001 | 0.118 | 0.167 | 193.2 | PASS |  |

## signs_ks16 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_signs\seq_signs_ks16`, 1800 frames at 30 fps, kind signs

- luma      `▄▄▅▅▆▆▆▆▆▅▅▄▄▅▅▆▆▇▇▇▇▆▅▅▄▅▅▆▇████▇▇▆▆▆▆▇▇█████▇▆▅▅▅▄▄▅▄▄▄▃▂▁`
- mean diff `▇▆▆▄▅▃▂▃▄▇▄▄▅▅▇▄▅▅▃▄▆▃▄▃▄▂▂▃▄▅▃▅▃▇▆▃▅▃▅▅█▇▅▄▆▇▅▆▆▄▅█▇▅▃▄▄▅▄▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0380 | <= 1.2 levels | at frame 1389 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
