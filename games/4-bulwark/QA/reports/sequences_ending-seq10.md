# Frame sequences: ending-seq10

Sequences: 1. Checks: 2 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| ride_descent | transition | 216 | 0.422 | 0.336 | 0.914 | 156.6 | PASS |  |

## ride_descent (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-seq10\seq_ride_descent`, 216 frames at 30 fps, kind transition

- luma      `▆▅▃▄▅▃▄▄▄▃▅▄▂▄▃▃▂▃▃▅▆▇▁▅▄▄▄▅▅▃▆▅▅▅▃▄▄▇▅▅▄▆▆▅▄▅▃▄▇▇▇▆▃▄▃▆█▆▇▅`
- mean diff `▄▂▂▄▃▂▂▄▂▁▃▃▂▃▂▂▂▂▆▆▃▃▁▄▂▂▄▂▃▂▂▃▄▄▂▃▃▅█▄▄▂▃▂▃▄▃▃▄▃▂▂▂▄▂▂▆█▃▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.4218 | <= 1.2 levels | at frame 116 |
| EV slope | INFO | 0.4446 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
