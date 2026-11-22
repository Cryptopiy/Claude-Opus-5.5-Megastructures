# Frame sequences: p4_01_statics

Sequences: 7. Checks: 14 PASS, 7 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| static_beat01_alley | static | 120 | 0.0001 | 0.024 | 0.058 | 86.5 | FAIL | static change |
| static_beat02_boulevard | static | 120 | 0.0001 | 0.247 | 0.271 | 217.5 | FAIL | static change |
| static_beat03_edge | static | 120 | 0.0001 | 0.216 | 0.247 | 220.2 | FAIL | static change |
| static_beat04_skybridge | static | 120 | 0.0001 | 0.047 | 0.061 | 220.2 | FAIL | static change |
| static_beat05_garden | static | 120 | 0.0000 | 0.019 | 0.026 | 196.6 | FAIL | static change |
| static_beat06_signs | static | 120 | 0.0000 | 0.044 | 0.065 | 86.8 | FAIL | static change |
| static_beat07_heart | static | 120 | 0.0000 | 0.022 | 0.043 | 83.1 | FAIL | static change |

## static_beat01_alley (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_statics\seq_static_beat01_alley`, 120 frames at 30 fps, kind static

- luma      `▂▃▃▃▃▁▄▇▅▂▄▄▂▃▄▇▇█▆▃▄▄▄▄▄▂▅▄▄▄▃▄▄▃▂▂▁▂▂▂▂▂▂▇▇▂▃▄▄▅▇▄▄▆▆▄▆▇▂▃`
- mean diff `▂▃▃▂▃▂▃▆▅▃▄▄▃▃▅▆▆█▇▄▄▄▄▅▅▃▅▄▄▄▃▄▄▃▂▂▁▂▂▂▂▂▂▅▇▃▃▄▄▅▆▅▄▅▆▄▅▆▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0150 | <= 1.2 levels | at frame 89 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 86.5160 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 41.62; freezeClock False, rain override None (14.10: run with -freeze-clock -rain 0) |

## static_beat02_boulevard (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_statics\seq_static_beat02_boulevard`, 120 frames at 30 fps, kind static

- luma      `▅▇█▇▆▇▇▇▆▅▆▄▄▅▅▆▇▇▇▆▆▆▅▅▃▃▅▆▄▅▄▄▄▃▃▄▄▄▅▅▆▅▅▃▃▂▁▂▂▃▂▁▃▂▁▁▃▄▄▄`
- mean diff `▁▅▃▅▆▇▆▆▇▅▄▂▂▂▃▆▅▅▅▅▄▅▃▅▆▄▄▄▄▄▅▄▄▃▃▃▂▂▄▃▄▆▆▄▄▃▂▂▂▄▄▃█▄▃▂▂▄▂▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0250 | <= 1.2 levels | at frame 85 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 220.8600 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 198.30; freezeClock False, rain override None (14.10: run with -freeze-clock -rain 0) |

## static_beat03_edge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_statics\seq_static_beat03_edge`, 120 frames at 30 fps, kind static

- luma      `▁▃▃▅▆▄▃▄▃▃▃▃▃▄▅▆▅▆▅▆▇▇▇▆▆▆▆▆▆█▇▇▆▆▅▆▆▅▆▅▆▆▆▆▇██▇█████▇▆▆▆▇▆█`
- mean diff `▂▃▄▅▇▄▂▃▅▃▃▂▁▄▆▆▆▄▄▅▅▄▄▃▃▃▄▄▆▇▇▇▅▅▃▆▄▃▃▂▂▂▃▅▄▇█▆▇▆▅▅▆▅▃▂▂▅▅▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0230 | <= 1.2 levels | at frame 7 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 229.2900 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 215.51; freezeClock False, rain override None (14.10: run with -freeze-clock -rain 0) |

## static_beat04_skybridge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_statics\seq_static_beat04_skybridge`, 120 frames at 30 fps, kind static

- luma      `████▇▇▇█▇▇▆▆▅▆▆▅▅▄▃▄▃▄▃▄▃▃▃▃▃▄▃▄▄▃▃▃▂▃▃▄▃▂▁▃▄▄▂▄▃▃▄▃▃▅▄▄▃▂▃▃`
- mean diff `▄▅▇▇▆▆▄▇▅▄▃▃▂▄▅▄▄▄▁▅▄▅▅▆▄▄▃▂▃▇▄▆█▅▃▄▃▄▃▄█▅▁▃▇█▅▇▅▃▄▅▃▇▆▅▄▂▄▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0130 | <= 1.2 levels | at frame 101 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 222.9320 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 83.68; freezeClock False, rain override None (14.10: run with -freeze-clock -rain 0) |

## static_beat05_garden (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_statics\seq_static_beat05_garden`, 120 frames at 30 fps, kind static

- luma      `▇▇█▆▆▆▆▅▄▃▃▃▃▂▁▁▂▂▃▄▄▄▅▅▅▅▆▅▅▆▆▄▄▅▄▃▃▃▃▃▃▃▃▃▃▃▂▃▃▃▃▃▃▂▃▂▂▂▁▂`
- mean diff `▃▅█▅▆▆▃▂▂▂▂▃▅▃▂▂▃▅▃▄▄▃▄▆▄▆▄▄▄▆▅▄▂▅▆▂▄▃▁▁▂▂▂▂▂▂▂▄▂▅▄▄▄▂▄▄▃▃▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0060 | <= 1.2 levels | at frame 5 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 214.4380 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 137.98; freezeClock False, rain override None (14.10: run with -freeze-clock -rain 0) |

## static_beat06_signs (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_statics\seq_static_beat06_signs`, 120 frames at 30 fps, kind static

- luma      `▇▆▆▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁▂▁▁▁▁▂▁▁▁▁▁▁▂▂▂▂▂▂▂▃▃▃▃▃▄▅▄▄▅▅▅▅▆▆▆▇▇███`
- mean diff `▆▅▂▁▁▅▄▅▆▆▆▆▇▅▅▃▄▄▄▂▅▄▄▄▆█▇▅▃▄▂▂▆▇▆▄▄▃▃▃▄▃▂▂▄▇▇▄▃▄▃▂▃▃▃▆▅▇█▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0110 | <= 1.2 levels | at frame 3 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 89.8600 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 42.79; freezeClock False, rain override None (14.10: run with -freeze-clock -rain 0) |

## static_beat07_heart (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_statics\seq_static_beat07_heart`, 120 frames at 30 fps, kind static

- luma      `▆█▇▇▇▆▅▅▅▅▅▅▆▆▆▇▆▇▇▆▅▆▇▆▅▆▆▅▅▅▃▄▅▅▄▄▄▄▄▄▃▄▅▆▄▂▂▂▂▃▃▂▃▃▃▂▁▂▁▂`
- mean diff `▅▆▇▆▆▄▂▂▂▂▁▁▃▃▃▅▅▅▅▄▃▄▅▄▄▃▅▄▃▃▂▂▃▄▃▃▄▄▄▄▃▅▇█▆▃▂▂▂▃▃▃▅▆▄▄▃▃▃▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0100 | <= 1.2 levels | at frame 3 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 105.7480 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 39.80; freezeClock False, rain override None (14.10: run with -freeze-clock -rain 0) |
