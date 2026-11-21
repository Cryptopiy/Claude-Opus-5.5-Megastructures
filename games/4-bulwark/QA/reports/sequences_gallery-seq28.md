# Frame sequences: gallery-seq28

Sequences: 1. Checks: 4 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| static_b06_WindowGallery | static | 120 | 0.002 | 0.011 | 0.012 | 5.5 | PASS |  |

## static_b06_WindowGallery (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-seq28\seq_static_b06_WindowGallery`, 120 frames at 30 fps, kind static

- luma      `█▇▇▇▆▆▅▅▅▄▄▄▄▃▃▃▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▃▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄`
- mean diff `█████▇▆▆▅▅▅▄▄▃▄▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▂▃▂▂▂▂▃▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0019 | <= 1.2 levels | at frame 5 |
| EV slope | INFO | 0.0084 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 1.8926 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
