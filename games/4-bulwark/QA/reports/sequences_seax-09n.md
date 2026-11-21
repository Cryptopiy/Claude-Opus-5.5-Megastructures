# Frame sequences: seax-09n

Sequences: 2. Checks: 6 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| pan_KS04_sea | pan | 30 | 0.002 | 0.230 | 0.232 | 39.6 | PASS |  |
| static_KS04 | static | 120 | 0.029 | 0.717 | 0.868 | 91.7 | FAIL | flicker block std |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-09n\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▁▂▂▃▃▃▃▃▃▃▃▄▄▄▄▄▅▅▅▅▆▆▆▆▆▆▇██`
- mean diff `▇▅▇▅▄█▆▅▅▆▇▇▁▇▄▆▅▅▅▄▆▆▅▆▄▆▃▆▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0016 | <= 1.2 levels | at frame 27 |
| EV slope | INFO | 0.0014 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.0233 | <= 2 levels | exact rotation homography; raw 1.023, control floor 0.821, low-pass (sigma 1.5) 0.295, block-mean 0.106; gate block [1664, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-09n\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▂▂▃▃▄▄▄▄▅▅▄▄▄▄▄▄▄▄▄▄▄▅▅▅▅▅▅▄▄▄▄▄▄▄▄▅▅▅▅▆▆▆▆▇▇▇▇▇▇▇▇▇████`
- mean diff `▂▂▂▁▁▁▁▁▁▁▁▁▂▃▃▃▄▅▅▆▆▇▇▇█████████▇▇▇▇▇▇▇▇▇▇▆▆▆▆▅▆▅▅▄▄▄▃▃▃▃▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0289 | <= 1.2 levels | at frame 13 |
| EV slope | INFO | 0.0177 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 20.3120 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
