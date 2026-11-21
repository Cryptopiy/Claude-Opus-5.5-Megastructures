# Frame sequences: seax-09f

Sequences: 2. Checks: 6 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| pan_KS04_sea | pan | 30 | 0.002 | 0.200 | 0.202 | 34.4 | PASS |  |
| static_KS04 | static | 120 | 0.043 | 0.743 | 0.918 | 74.4 | FAIL | flicker block std |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-09f\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▂▂▂▃▃▃▃▃▄▄▄▄▅▅▅▅▅▅▆▆▆▇▇▇███`
- mean diff `▆▄▇▅▄█▆▅▄▆▇▇▁█▄▆▅▅▅▅▆▆▅▆▄▆▃▆▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0019 | <= 1.2 levels | at frame 24 |
| EV slope | INFO | 0.0018 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 0.9207 | <= 2 levels | exact rotation homography; raw 0.921, control floor 0.727, low-pass (sigma 1.5) 0.276, block-mean 0.106; gate block [1664, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-09f\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▃▃▃▃▄▄▄▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇▇██████████`
- mean diff `▂▂▁▁▁▁▁▁▁▁▁▂▂▃▃▄▄▅▅▆▆▆▇▇▇██████████▇▇▇▇▇▇▇▇▇▇▆▆▆▆▅▅▅▅▅▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0432 | <= 1.2 levels | at frame 35 |
| EV slope | INFO | 0.0319 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 14.8371 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
