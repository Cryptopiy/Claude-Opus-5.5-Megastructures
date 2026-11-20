# Frame sequences: A2a_statics

Sequences: 7. Checks: 15 PASS, 6 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| static_beat01_alley | static | 120 | 0.0000 | 0.000 | 0.000 | 8.7 | FAIL | static change |
| static_beat02_boulevard | static | 120 | 0.0000 | 0.000 | 0.000 | 9.1 | FAIL | static change |
| static_beat03_edge | static | 120 | 0.0000 | 0.000 | 0.000 | 6.1 | FAIL | static change |
| static_beat04_skybridge | static | 120 | 0.0000 | 0.000 | 0.000 | 4.6 | FAIL | static change |
| static_beat05_garden | static | 120 | 0.0000 | 0.000 | 0.000 | 1.0 | PASS |  |
| static_beat06_signs | static | 120 | 0.0000 | 0.000 | 0.000 | 4.4 | FAIL | static change |
| static_beat07_heart | static | 120 | 0.0000 | 0.000 | 0.000 | 3.5 | FAIL | static change |

## static_beat01_alley (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_statics\seq_static_beat01_alley`, 120 frames at 30 fps, kind static

- luma      `████████████▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 13 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 8.7040 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 0.93; freezeClock False, rain x0 (14.10: run with -freeze-clock -rain 0) |

## static_beat02_boulevard (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_statics\seq_static_beat02_boulevard`, 120 frames at 30 fps, kind static

- luma      `▁▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▃▃▃▃▅▅▅▅▅▅▅▅▅▅▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇██████████`
- mean diff `▁█████▁▁█████████▁▁██▁███▁███████▁█████▁█▁████▁▁▁▁▁█▁▁██▁███`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 10 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 9.1290 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 1.00; freezeClock False, rain x0 (14.10: run with -freeze-clock -rain 0) |

## static_beat03_edge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_statics\seq_static_beat03_edge`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁████████████████████████████████████████████`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 34 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 6.1340 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 1.00; freezeClock False, rain x0 (14.10: run with -freeze-clock -rain 0) |

## static_beat04_skybridge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_statics\seq_static_beat04_skybridge`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁█▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 5 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 4.5640 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 1.00; freezeClock False, rain x0 (14.10: run with -freeze-clock -rain 0) |

## static_beat05_garden (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_statics\seq_static_beat05_garden`, 120 frames at 30 fps, kind static

- luma      `██████████▆▆▆▆▆▆▆▆▆▆▆▆▆▄▄▄▄▄▄▄▄▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▃▃▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 25 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | PASS | 1.0000 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 1.00; freezeClock False, rain x0 (14.10: run with -freeze-clock -rain 0) |

## static_beat06_signs (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_statics\seq_static_beat06_signs`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 23 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 4.3520 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 1.00; freezeClock False, rain x0 (14.10: run with -freeze-clock -rain 0) |

## static_beat07_heart (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_statics\seq_static_beat07_heart`, 120 frames at 30 fps, kind static

- luma      `████████████▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▁▁▁▁▁▁▁▁▁`
- mean diff `███████████████████▁████████████████████████████████████████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 81 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | FAIL | 3.4920 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 1.00; freezeClock False, rain x0 (14.10: run with -freeze-clock -rain 0) |
