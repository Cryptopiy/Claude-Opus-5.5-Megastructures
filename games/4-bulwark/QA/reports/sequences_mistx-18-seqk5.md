# Frame sequences: mistx-18-seqk5

Sequences: 1. Checks: 3 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| mist_k5_cycle | static | 360 | 0.219 | 0.181 | 0.790 | 146.2 | FAIL | flicker block std |

## mist_k5_cycle (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-18-seqk5\seq_mist_k5_cycle`, 360 frames at 30 fps, kind static

- luma      `██▆▇▇▆▆▅▄▃▃▄▄▃▃▂▂▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▂▂▅▆▆▃▂▂▁▁▁▂▂▂▂▂▂▂`
- mean diff `▇▇▆█▆█▅▅▄▃▂▂▂▂▁▂▂▁▁▁▃▃▂▂▂▁▁▁▁▁▁▁▂▁▁▁▁▁▁▂▂▂▂▂▄▃▄▄▂▂▁▁▁▁▁▂▂▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2193 | <= 1.2 levels | at frame 283 |
| EV slope | INFO | 0.1729 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 34.9275 | <= 3 levels | 360 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
