# Frame sequences: mistx-06-seqk5

Sequences: 1. Checks: 1 PASS, 3 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| mist_k5_cycle | static | 360 | 6.758 | 0.448 | 6.760 | 146.2 | FAIL | luma step/frame, diff spikes, flicker block std |

## mist_k5_cycle (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-06-seqk5\seq_mist_k5_cycle`, 360 frames at 30 fps, kind static

- luma      `▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂██▇▇▆▃▂▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▃█▄▄▅▅▃▂▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 6.7581 | <= 1.2 levels | at frame 261 |
| EV slope | INFO | 4.3395 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | FAIL | 5 | 0 above 3x running median | frames [121, 122, 123, 124, 125] |
| flicker block std | FAIL | 26.3386 | <= 3 levels | 360 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
