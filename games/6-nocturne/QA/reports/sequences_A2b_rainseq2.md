# Frame sequences: A2b_rainseq2

Sequences: 1. Checks: 0 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| rain_beat04 | rain | 120 | 0.0054 | 0.098 | 1.431 | 215.4 | FAIL | luma step/frame, diff spikes |

## rain_beat04 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2b_rainseq2\seq_rain_beat04`, 120 frames at 60 fps, kind rain

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▄▅█▇▆▁▁▁▁▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▄▄▅▄▃█▁▁▁▂▁▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.3830 | <= 1.2 levels | at frame 60 (14.10) |
| diff spikes | FAIL | 8 | 0 above 3x running median | frames [51, 52, 53, 55, 56, 57, 58, 60] |
