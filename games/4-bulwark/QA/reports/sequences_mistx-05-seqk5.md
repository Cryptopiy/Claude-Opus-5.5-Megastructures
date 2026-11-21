# Frame sequences: mistx-05-seqk5

Sequences: 2. Checks: 3 PASS, 3 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| mist_k5_cycle | static | 360 | 7.536 | 0.499 | 7.538 | 146.2 | FAIL | luma step/frame, diff spikes, flicker block std |
| s0520_0640 | transition | 1441 | 0.643 | 1.332 | 3.386 | 144.8 | PASS |  |

## mist_k5_cycle (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-05-seqk5\seq_mist_k5_cycle`, 360 frames at 30 fps, kind static

- luma      `▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂██▇▇▆▃▂▁▁▁▁▁▁▁▁▁▁`
- mean diff `▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▃█▄▄▅▅▃▂▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 7.5362 | <= 1.2 levels | at frame 261 |
| EV slope | INFO | 4.8620 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | FAIL | 6 | 0 above 3x running median | frames [121, 122, 123, 124, 125, 261] |
| flicker block std | FAIL | 29.3387 | <= 3 levels | 360 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## s0520_0640 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-05-seqwalk\seq_s0520_0640`, 1441 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▂▂▂▂▁▂▂▃▃▃▄▅▅▅▄▄▄▅▆▆▆▆▆▆▆▅▄▆▆▆▆▇▇▇▇▆▄▅▆▆▆▇▇██▆▄▅▆▆▆▇▇██`
- mean diff `███▇▇▆▅▄▂▁▂▂▂▂▂▂▂▂▃▂▂▂▃▃▃▃▃▂▃▂▃▃▃▄▃▃▃▃▃▂▃▃▃▄▃▃▃▃▃▃▄▄▅▆▆▆▆▆▅▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6431 | <= 2 levels | at frame 784, sun break |
| EV slope | INFO | 0.4857 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
