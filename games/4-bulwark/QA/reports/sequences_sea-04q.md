# Frame sequences: sea-04q

Sequences: 2. Checks: 6 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| pan_KS04_sea | pan | 30 | 0.002 | 0.285 | 0.288 | 95.9 | PASS |  |
| static_KS04 | static | 120 | 0.039 | 1.029 | 1.296 | 103.7 | FAIL | flicker block std |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\sea-04q\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▁▂▂▂▂▃▃▃▃▄▄▄▄▅▅▅▅▅▅▅▆▆▆▇▇▇███`
- mean diff `▇▄▆▅▃█▅▆▄▆▇▇▁▇▄▆▅▅▅▄▅▆▄▅▄▇▃▆▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0022 | <= 1.2 levels | at frame 10 |
| EV slope | INFO | 0.0023 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.1754 | <= 2 levels | exact rotation homography; raw 1.358, control floor 1.098, low-pass (sigma 1.5) 0.401, block-mean 0.133; gate block [768, 640] (raw); 7 of 420 blocks in the fine-pattern regime |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\sea-04q\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▂▁▁▁▁▁▁▁▁▁▂▂▂▂▃▃▃▄▄▄▄▄▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇██████`
- mean diff `▁▁▁▁▁▁▁▁▁▂▂▂▂▃▃▄▄▅▅▆▆▆▇▇██████████▇▇▇▇▇▇▇▇▇▆▆▆▆▆▅▅▅▅▅▄▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0388 | <= 1.2 levels | at frame 29 |
| EV slope | INFO | 0.0284 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 16.1939 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
