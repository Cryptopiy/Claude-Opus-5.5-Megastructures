# Frame sequences: p4_01_traffic

Sequences: 2. Checks: 4 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| traffic_ks06_tram | traffic | 480 | 0.0002 | 0.905 | 1.126 | 225.4 | PASS |  |
| traffic_ks11_train | traffic | 480 | 0.0015 | 1.216 | 1.931 | 223.8 | PASS |  |

## traffic_ks06_tram (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_traffic\seq_traffic_ks06_tram`, 480 frames at 60 fps, kind traffic

- luma      `▄▄▄▃▃▃▃▃▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▃▃▄▄▄▄▅▅▅▆▇███████▇▇`
- mean diff `▃▂▂▂▃▂▂▂▂▂▂▁▂▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▄▄▄▄▅▅▅▆▇▇▇█▇▇▇█▇████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0460 | <= 1.2 levels | at frame 413 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## traffic_ks11_train (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_traffic\seq_traffic_ks11_train`, 480 frames at 60 fps, kind traffic

- luma      `▂▂▂▂▂▂▃▃▃▄▄▅▅▅▅▆▆▆▅▃▂▂▁▁▁▁▁▁▁▁▁▁▂▂▂▁▂▂▂▂▂▂▃▃▃▄▄▄▄▅▅▅▅▆▆▇▇▇██`
- mean diff `▁▁▁▁▁▁▁▂▂▂▂▂▃▃▃▄▅▄▂▂▂▂▂▁▁▁▁▂▂▁▂▂▃▄▇▅▆▅▅▆▇▅▅▆▇▆▆▆▇▇▆▆▇█▇▆▇██▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.3790 | <= 1.2 levels | at frame 274 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
