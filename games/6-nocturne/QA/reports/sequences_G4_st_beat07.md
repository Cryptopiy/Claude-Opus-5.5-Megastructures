# Frame sequences: G4_st_beat07

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| static_beat07_heart | static | 120 | 0.0000 | 0.000 | 0.000 | 0.0 | PASS |  |

## static_beat07_heart (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G4_st_beat07\seq_static_beat07_heart`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 1 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| static change | PASS | 0.0000 | <= 2 levels | max \|frame - frame 0\| per pixel; p99.9 0.00; freezeClock True, rain override 0 (14.10: run with -freeze-clock -rain 0) |
