# Frame sequences: gallery-seq35

Sequences: 2. Checks: 7 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| pan_b06_WindowGallery | pan | 30 | 0.015 | 0.238 | 0.245 | 172.2 | PASS |  |
| static_b06_WindowGallery | static | 120 | 0.002 | 0.011 | 0.014 | 10.1 | PASS |  |

## pan_b06_WindowGallery (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-seq35\seq_pan_b06_WindowGallery`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▂▂▂▂▃▃▃▃▄▄▄▄▅▅▅▅▆▆▆▆▇▇▇▇███`
- mean diff `▆▆▆▄▅▅▆▇▄▃▇▃█▃▄▆▅▅▆▃▆▄▅▄▄▅▃█▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0149 | <= 1.2 levels | at frame 26 |
| EV slope | INFO | 0.0354 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.1886 | <= 1.5 levels | exact rotation homography; raw 1.189, control floor 0.727, low-pass (sigma 1.5) 0.332, block-mean 0.085; gate block [1024, 640] (raw); 0 of 420 blocks in the fine-pattern regime; strict (beat 6 / ledge) |

## static_b06_WindowGallery (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-seq35\seq_static_b06_WindowGallery`, 120 frames at 30 fps, kind static

- luma      `██▇▇▇▇▇▇▆▆▆▆▆▅▅▅▅▅▅▄▄▄▄▄▄▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁`
- mean diff `▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▄▄▄▄▄▄▄▅▅▅▅▅▆▆▆▇▇▇▇██`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0021 | <= 1.2 levels | at frame 10 |
| EV slope | INFO | 0.0093 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 2.5959 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
