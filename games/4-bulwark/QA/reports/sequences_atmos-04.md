# Frame sequences: atmos-04

Sequences: 4. Checks: 10 PASS, 3 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| pan_b02_Crest | pan | 30 | 0.006 | 0.229 | 0.235 | 97.7 | FAIL | pan shimmer std |
| pan_b07_SluiceLedge | pan | 30 | 0.036 | 0.322 | 0.338 | 105.7 | FAIL | pan shimmer std |
| pan_rail_slow | pan | 30 | 0.002 | 0.034 | 0.048 | 53.2 | FAIL | pan shimmer std |
| static_b07_SluiceLedge | static | 120 | 0.000 | 0.000 | 0.000 | 0.8 | PASS |  |

## pan_b02_Crest (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\seq_pan_b02_Crest`, 30 frames at 30 fps, kind pan

- luma      `██▇▇▇▆▆▆▆▆▅▅▅▅▅▅▄▄▄▄▄▃▃▃▂▂▂▂▁▁`
- mean diff `▃▃▂▆▁▅▄▂▅▁▄▁▄▆▁▃▃▄▁▃█▂▃▃▂▅▄▂▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0063 | <= 1.2 levels | at frame 5 |
| EV slope | INFO | 0.0033 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | FAIL | 7.7426 | <= 2 levels | offsets from yaw and vFOV, total -14.50 px |

## pan_b07_SluiceLedge (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\seq_pan_b07_SluiceLedge`, 30 frames at 30 fps, kind pan

- luma      `███▇▇▇▇▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▂▁▁`
- mean diff `▅▁▄▂▅▄▄▄▄▅▅▄▅▄▄▄▅▃▆▁▇▂▄▄▂▅▄▄█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0357 | <= 1.2 levels | at frame 28 |
| EV slope | INFO | 0.0293 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | FAIL | 6.5365 | <= 1.5 levels | offsets from yaw and vFOV, total -14.50 px; strict (beat 6 / ledge) |

## pan_rail_slow (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\seq_pan_rail_slow`, 30 frames at 30 fps, kind pan

- luma      `███▆▆▆█▆▆▆▃▃▆▆▃▃▃▃▁▃▃▁▁▁▃▁▁▁▁▁`
- mean diff `▃▅▃▄█▃▁▆▂▄▅▃▇▃▃▃▆▂▅▃▃▅▃▆▂▅▄▄▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0024 | <= 1.2 levels | at frame 25 |
| EV slope | INFO | 0.0017 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | FAIL | 3.2792 | <= 1.5 levels | offsets from yaw and vFOV, total -2.90 px; strict (beat 6 / ledge) |

## static_b07_SluiceLedge (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\seq_static_b07_SluiceLedge`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 52 |
| EV slope | INFO | 0.0000 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.0069 | <= 1.5 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
