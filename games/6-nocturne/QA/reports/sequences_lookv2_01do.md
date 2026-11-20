# Frame sequences: lookv2_01do

Sequences: 2. Checks: 6 PASS, 0 FAIL, 2 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| dolly_face30 | dolly | 300 | 0.0015 | 4.506 | 7.603 | 210.9 | WARN |  |
| dolly_graze | dolly | 251 | 0.0005 | 1.871 | 2.076 | 182.7 | WARN |  |

## dolly_face30 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01do\seq_dolly_face30`, 300 frames at 30 fps, kind dolly

- luma      `███▇▇▇▆▆▆▅▅▅▅▄▄▄▄▃▃▃▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂`
- mean diff `▅▅▅▆▆▇▇███████▇▇▇▇▆▆▅▅▅▅▄▄▄▄▄▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.3900 | <= 1.2 levels | at frame 2 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| dolly run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the camera must be the only change |
| dolly local pops (64 px blocks) | WARN | 17 | 0 one-frame block jumps (> 3x both neighbours and > 2 levels) | worst [{'frame': 299, 'block': 479, 'diff': 26.8, 'median': 3.66, 'dist_m': 25}, {'frame': 299, 'block': 449, 'diff': 26.36, 'median': 3.75, 'dist_m': 25}, {'frame': 1, 'block': 3, 'diff': 16.78, 'median': 0.25, 'dist_m': 790.78}] |

## dolly_graze (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01do\seq_dolly_graze`, 251 frames at 30 fps, kind dolly

- luma      `▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇████████████████▇▇▇▇▇▇▆▆▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▂▂▂▁▁`
- mean diff `▅▃▃▃▃▂▂▂▃▃▃▃▃▃▄▄▄▄▄▄▅▅▅▆▆▆▇▇▇▇▇▇██████████▇▇▇▇▇▆▆▆▆▅▅▅▄▄▃▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1360 | <= 1.2 levels | at frame 2 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| dolly run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the camera must be the only change |
| dolly local pops (64 px blocks) | WARN | 10 | 0 one-frame block jumps (> 3x both neighbours and > 2 levels) | worst [{'frame': 2, 'block': 476, 'diff': 19.31, 'median': 0.75, 'dist_m': 1237.65}, {'frame': 2, 'block': 446, 'diff': 19.02, 'median': 0.64, 'dist_m': 1237.65}, {'frame': 1, 'block': 475, 'diff': 18.25, 'median': 0.25, 'dist_m': 1240.57}] |
