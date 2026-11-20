# Frame sequences: lookv2_01rain

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| rain_beat04 | rain | 120 | 0.0000 | 0.012 | 0.019 | 210.2 | PASS |  |

## rain_beat04 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01rain\seq_rain_beat04`, 120 frames at 60 fps, kind rain

- luma      `▂▃▃▃▅▃█▆▃▆▇▅▅▅▅▆▅▆▆▆▅▅▃▃▃▃▅▅▆▇▅▅▃▃▃▃▁▂▃▁▁▅▃▁▁▁▅▃▁▁▃▂▃▂▂▂▁▅▃▅`
- mean diff `▃▃▄▃▄▅▆█▄▅▆▄▃▃▃▄▆▄▅▅▄▃▂▂▃▅▃▄▆▇▇▇▅▃▅▄▂▃▄▂▃▅▄▂▃▂▅▆▃▂▄▃▃▄▂▁▁▃▅▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0060 | <= 1.2 levels | at frame 12 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| rain outside-streak change | PASS | 2 | <= 6 levels | 14.7 H5: streak mask = luma > temporal min + 1.5 codes, dilated 2 px; p99.9 mean 1.00; mask coverage 0.260 % (mean); 119 pairs |
