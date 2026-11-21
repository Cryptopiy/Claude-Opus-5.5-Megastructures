# Frame sequences: mistx-09-seqwalk

Sequences: 1. Checks: 1 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| s0520_0640 | transition | 49 | 7.160 | 6.650 | 11.921 | 161.5 | FAIL | luma step/frame |

## s0520_0640 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-09-seqwalk\seq_s0520_0640`, 49 frames at 30 fps, kind transition

- luma      `▁▂▂▂▂▃▃▂▂▂▃▃▄▅▅▅▄▄▅▆▆▆▆▆▅▄▅▆▆▇▇▇▆▄▅▆▆▇▇█▆▄▅▆▆▇▇█▆`
- mean diff `▃▂▄▂▂▃▃▁▃▂▂▂▃▂▃▃▄▄▅▂▃▂▃▄▅▄▆▃▃▃▄▄▇▄▆▃▃▃▅▅█▅▇▇▆▅▇▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 7.1597 | <= 2 levels | at frame 41, sun break |
| EV slope | INFO | 4.7174 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
