# Frame sequences: p4_01_dolly

Sequences: 2. Checks: 4 PASS, 0 FAIL, 4 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| dolly_face30 | dolly | 300 | 0.0026 | 5.846 | 11.510 | 219.8 | WARN |  |
| dolly_graze | dolly | 251 | 0.0011 | 3.693 | 4.755 | 226.4 | WARN |  |

## dolly_face30 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_dolly\seq_dolly_face30`, 300 frames at 30 fps, kind dolly

- luma      `██████▇▆▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▆▆▆▇▇██▇██▇▇▇▇▇▇▆▆▆▆▅▅▅▄▄▄▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6690 | <= 1.2 levels | at frame 30 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| dolly run conditions | WARN | rain None, freezeClock False | -rain 0 -freeze-clock | the camera must be the only change |
| dolly local pops (64 px blocks) | WARN | 12 | 0 one-frame block jumps (> 3x both neighbours and > 2 levels) | worst [{'frame': 1, 'block': 30, 'diff': 52.55, 'median': 2.98, 'dist_m': 790.78}, {'frame': 1, 'block': 91, 'diff': 51.65, 'median': 15.77, 'dist_m': 790.78}, {'frame': 1, 'block': 122, 'diff': 27.79, 'median': 6.2, 'dist_m': 790.78}] |

## dolly_graze (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_dolly\seq_dolly_graze`, 251 frames at 30 fps, kind dolly

- luma      `▄▄▄▄▄▄▅▅▅▅▅▅▅▅▆▆▆▆▆▆▇▇▇█████████▇▇▇▇▇▇▆▆▆▆▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁▁`
- mean diff `▂▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▃▃▃▄▄▄▅▆▅▅▆▇▇▇▇▇▇▇▇▇▇██▇▆▇▆▆▅▅▅▅▅▄▄▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2750 | <= 1.2 levels | at frame 188 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| dolly run conditions | WARN | rain None, freezeClock False | -rain 0 -freeze-clock | the camera must be the only change |
| dolly local pops (64 px blocks) | WARN | 17 | 0 one-frame block jumps (> 3x both neighbours and > 2 levels) | worst [{'frame': 137, 'block': 450, 'diff': 32.47, 'median': 5.53, 'dist_m': 842.93}, {'frame': 135, 'block': 450, 'diff': 32.07, 'median': 5.68, 'dist_m': 848.78}, {'frame': 139, 'block': 450, 'diff': 32.89, 'median': 7.12, 'dist_m': 837.09}] |
