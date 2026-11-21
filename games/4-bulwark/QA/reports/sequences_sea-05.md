# Frame sequences: sea-05

Sequences: 2. Checks: 7 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| static_b03_SpillwayBridge | static | 120 | 0.000 | 0.003 | 0.003 | 46.5 | PASS |  |
| static_b04_ArmourSlope | static | 120 | 0.010 | 0.095 | 0.122 | 155.8 | FAIL | flicker block std |

## static_b03_SpillwayBridge (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\sea-05\seq_static_b03_SpillwayBridge`, 120 frames at 30 fps, kind static

- luma      `▅▄▄▄▄▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▅▅▅▅▇▇▇▇▇███████`
- mean diff `▁▁▃▃▃▃▃▃▁▁▁▃▁▃▁▃▁▁▁▁▁▃▃▁▃▃▃▃▃▃▃▁▆▅▅█▆▃▃▃▃▃▅▅▆▅▅▅▃▃▁▁▁▁▁▁▃▃▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0005 | <= 2 levels | at frame 9, sun break |
| EV slope | INFO | 0.0005 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.4427 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b04_ArmourSlope (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\sea-05\seq_static_b04_ArmourSlope`, 120 frames at 30 fps, kind static

- luma      `▇███████████▇▇▇▆▆▆▅▅▄▄▃▃▃▃▃▃▃▃▃▂▂▂▂▂▁▁▁▁▁▁▂▂▂▂▂▂▃▃▃▃▄▄▄▅▅▅▆▆`
- mean diff `▇▅▇▇▇██▆▇▆▆▆▆▅▆▇▆▆▆▆▆▆▄▆▅▅▆▅▅▅▅▄▄▄▄▄▃▃▁▃▄▃▂▃▃▃▄▃▄▅▅▅▆▆▇▆▆▇▇▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0097 | <= 1.2 levels | at frame 39 |
| EV slope | INFO | 0.0073 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 21.2157 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
