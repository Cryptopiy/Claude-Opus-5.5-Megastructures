# Frame sequences: kraken09

Sequences: 3. Checks: 9 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| kr_seq_storm800 | static | 600 | 0.087 | 0.381 | 0.549 | 73.5 | FAIL | flicker block std |
| kr_seq_uwclose | static | 300 | 0.019 | 0.063 | 0.128 | 141.2 | FAIL | flicker block std |
| kr_pan_storm800 | pan | 30 | 0.002 | 0.211 | 0.216 | 64.9 | PASS |  |

## kr_seq_storm800 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-09s\kr_seq_storm800`, 600 frames at 30 fps, kind static

- luma      `▄▅▅▅▅▅▅▄▄▃▃▂▂▂▂▂▁▁▁▁▁▂▄▆▇▇▇▇▇▆▅▅▄▄▅▅▅▆▆▅▄▃▃▃▃▃▃▁▁▂▂▂▃▄▅▇████`
- mean diff `▂▃▅▆▇█▇▆▄▃▂▂▃▃▄▄▄▄▄▃▁▁▃▄▄▅▆▆▆▄▂▂▂▃▄▄▅▅▄▃▁▂▃▄▄▄▄▂▂▂▁▂▂▃▄▅▅▅▅▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0874 | <= 1.2 levels | at frame 231 |
| EV slope | INFO | 0.0610 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 8.2128 | <= 3 levels | 600 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## kr_seq_uwclose (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-09s\kr_seq_uwclose`, 300 frames at 30 fps, kind static

- luma      `███▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▆▆▆▅▅▅▄▄▃▃▃▂▂▂▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂`
- mean diff `▃▃▃█▄▄▄▄▅▅▅▆▆▆▆▆▆▇▇▇▇▇▇▆▆▆▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁▁▁▂▂▂▃▃▃▃▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0193 | <= 1.2 levels | at frame 140 |
| EV slope | INFO | 0.0638 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 28.2442 | <= 3 levels | 300 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## kr_pan_storm800 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-09p\kr_pan_storm800`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▂▂▂▃▃▃▃▃▄▄▄▄▅▅▆▆▆▆▆▆▆▇▇▇███`
- mean diff `▆▅▃▄▅▃▅█▃▆▂▇▅▁▅▆▅▃▄▅█▄▄▆▆▅▇▃▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0023 | <= 1.2 levels | at frame 7 |
| EV slope | INFO | 0.0017 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.2320 | <= 2 levels | exact rotation homography; raw 1.232, control floor 0.747, low-pass (sigma 1.5) 0.373, block-mean 0.100; gate block [1600, 448] (raw); 0 of 420 blocks in the fine-pattern regime |
