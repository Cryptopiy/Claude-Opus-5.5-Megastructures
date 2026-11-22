# Frame sequences: HT_koi02

Sequences: 2. Checks: 4 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| koi_ks17 | koi | 720 | 0.0001 | 0.345 | 0.416 | 181.4 | PASS |  |
| koi_ks18 | koi | 720 | 0.0002 | 0.169 | 0.229 | 164.4 | PASS |  |

## koi_ks17 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_koi02\seq_koi_ks17`, 720 frames at 60 fps, kind koi

- luma      `▁▁▁▁▁▁▁▂▂▁▁▂▂▂▂▂▂▂▂▂▂▂▁▁▂▂▂▂▂▂▃▃▃▃▃▃▄▄▄▅▅▅▅▆▆▆▆▇▇▇▇▇▇▇▇▇████`
- mean diff `▅▅▄▄▄▅▆▅▅▄▄▄▄▅▆▅▅▅▃▃▃▃▃▂▂▂▂▂▂▁▁▁▁▁▂▂▂▃▃▄▃▄▄▄▅▅▆▅▆▅▅▅▅▅███▆▆▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0350 | <= 1.2 levels | at frame 71 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## koi_ks18 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_koi02\seq_koi_ks18`, 720 frames at 60 fps, kind koi

- luma      `▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▃▄▄▄▄▄▅▅▅▅▅▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇█████`
- mean diff `▄▁▂▂▁▂▁▁▂▂▂▂▂▂▂▂▂▃▄▃▃▃▃▄▄▄▃▄▄▄▅▅▅▆▆▆▇▇▇▇█▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0510 | <= 1.2 levels | at frame 1 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
