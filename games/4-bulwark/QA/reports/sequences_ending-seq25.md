# Frame sequences: ending-seq25

Sequences: 1. Checks: 2 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| ride_descent | transition | 216 | 0.912 | 0.414 | 1.125 | 154.3 | PASS |  |

## ride_descent (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-seq25\seq_ride_descent`, 216 frames at 30 fps, kind transition

- luma      `██▆▇▇▅▆▅▇▅▆▅▄▅▅▅▄▄▄▄▄▅▂▄▃▄▃▃▃▂▃▃▃▃▂▂▂▃▂▂▁▂▂▂▁▂▁▁▂▂▂▂▁▁▁▂▃▂▂▂`
- mean diff `▄▅▃█▇▃▄▅▃▂▆▄▂▄▃▃▃▃▆█▄▄▂▅▃▃▄▂▃▂▂▃▄▄▂▃▃▄▇▄▄▂▃▂▂▃▃▃▃▃▂▂▂▃▂▂▅▇▃▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.9116 | <= 1.2 levels | at frame 13 |
| EV slope | INFO | 0.7630 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
