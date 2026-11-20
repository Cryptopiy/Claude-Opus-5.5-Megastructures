# Frame sequences: A2b_rainseq3

Sequences: 1. Checks: 2 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| rain_beat04 | rain | 120 | 0.0000 | 0.048 | 0.064 | 135.9 | PASS |  |

## rain_beat04 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2b_rainseq3\seq_rain_beat04`, 120 frames at 60 fps, kind rain

- luma      `▅▅█▇▆▆▃▃▄▄▄▄▄▆▅▃▃▃▃▁▃▃▂▂▁▃▃▂▄▅▅▄▃▅▄▁▃▅▄▃▂▄▆▅▅▅▃▄▆▆▆▆▅▃▃▃▂▅▅▃`
- mean diff `▄▅▇█▇▆▄▃▄▄▄▄▄▆▆▄▃▄▃▂▃▄▄▂▁▂▃▃▄▅▅▅▄▄▄▂▃▄▄▃▂▄▅▅▄▄▃▂▄▆▆▅▄▂▁▂▁▂▄▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0090 | <= 1.2 levels | at frame 114 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
