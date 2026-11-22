# Frame sequences: HT_seq01

Sequences: 3. Checks: 8 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b67 | boundary | 601 | 0.0004 | 2.341 | 3.177 | 221.3 | PASS |  |
| pan_beat07_heart | pan | 30 | 0.0000 | 1.598 | 1.652 | 176.9 | PASS |  |
| static_beat07_heart | static | 120 | 0.0000 | 0.000 | 0.000 | 0.0 | PASS |  |

## b67 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_seq01\seq_b67`, 601 frames at 30 fps, kind boundary

- luma      `▅▅▅▅▅▅▅▅▅▄▄▄▄▃▃▃▂▂▂▁▁▁▁▁▁▁▂▂▂▃▃▄▄▄▅▅▆▆▆▆▇▇▇▇▇▇▇▇▇▇▇▇▇▇██████`
- mean diff `▁▁▁▂▁▁▁▁▂▂▂▂▂▁▁▁▁▁▁▃▂▃▃▃▄▄▄▅▅▇▇▇████████▇▇▆▅▅▅▅▅▄▄▄▄▅▅▄▅▅▄▅▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1110 | <= 1.2 levels | at frame 240 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0151 | <= 0.5 level | boundaries [1260.0], worst at frame 300 |

## pan_beat07_heart (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_seq01\seq_pan_beat07_heart`, 30 frames at 30 fps, kind pan

- luma      `▆▆▇▆▇▇█▇▇█▇████▇▇▆▅▄▃▂▂▂▁▁▁▁▂▁`
- mean diff `▅█▆█▆█▅█▅█▄█▄█▄█▃▇▃▆▁▆▂▅▂▆▂▆▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0120 | <= 1.2 levels | at frame 17 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## static_beat07_heart (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_seq01\seq_static_beat07_heart`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 1 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | PASS | 0.0000 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 0.00; freezeClock True, rain override 0 (14.10: run with -freeze-clock -rain 0) |
