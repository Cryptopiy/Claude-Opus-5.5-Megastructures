# Frame sequences: A2b_rainseq

Sequences: 1. Checks: 1 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| rain_beat04 | rain | 120 | 0.0045 | 0.102 | 1.260 | 217.5 | FAIL | diff spikes |

## rain_beat04 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2b_rainseq\seq_rain_beat04`, 120 frames at 60 fps, kind rain

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▃▆█▆▇▁▁▁▁▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▆█▅█▁▁▁▁▂▁▁▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 1.1450 | <= 1.2 levels | at frame 57 (14.10) |
| diff spikes | FAIL | 8 | 0 above 3x running median | frames [50, 51, 52, 53, 54, 55, 56, 57] |
