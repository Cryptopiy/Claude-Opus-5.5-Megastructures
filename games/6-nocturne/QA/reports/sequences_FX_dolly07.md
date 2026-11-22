# Frame sequences: FX_dolly07

Sequences: 2. Checks: 6 PASS, 0 FAIL, 2 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| dolly_face30 | dolly | 300 | 0.0023 | 5.461 | 10.246 | 224.2 | WARN |  |
| dolly_graze | dolly | 251 | 0.0011 | 2.985 | 3.734 | 227.9 | WARN |  |

## dolly_face30 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_dolly07\seq_dolly_face30`, 300 frames at 30 fps, kind dolly

- luma      `████▇▇▇▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▅▅▆▇▇███▇███▇▇▇▇▇▇▆▆▅▅▅▅▅▅▅▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5790 | <= 1.2 levels | at frame 35 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| dolly run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the camera must be the only change |
| dolly local pops (64 px blocks) | WARN | 14 | 0 one-frame block jumps (> 3x both neighbours and > 2 levels) | worst [{'frame': 1, 'block': 30, 'diff': 48.38, 'median': 3.37, 'dist_m': 790.78}, {'frame': 1, 'block': 60, 'diff': 45.21, 'median': 12.28, 'dist_m': 790.78}, {'frame': 1, 'block': 91, 'diff': 45.13, 'median': 14.06, 'dist_m': 790.78}] |

## dolly_graze (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_dolly07\seq_dolly_graze`, 251 frames at 30 fps, kind dolly

- luma      `▅▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇█████████▇▇▇▇▇▆▆▆▆▆▆▅▅▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁`
- mean diff `▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▄▄▄▄▄▅▅▆▆▇▇███▇▇▇▇▇▆▇▇▆▆▆▆▆▇▇▇▅▅▅▄▄▄▃▃▃▃▂▂▂▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2860 | <= 1.2 levels | at frame 185 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| dolly run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the camera must be the only change |
| dolly local pops (64 px blocks) | WARN | 12 | 0 one-frame block jumps (> 3x both neighbours and > 2 levels) | worst [{'frame': 1, 'block': 475, 'diff': 26.74, 'median': 0.25, 'dist_m': 1240.57}, {'frame': 2, 'block': 476, 'diff': 27.03, 'median': 1.0, 'dist_m': 1237.65}, {'frame': 2, 'block': 446, 'diff': 26.6, 'median': 0.96, 'dist_m': 1237.65}] |
