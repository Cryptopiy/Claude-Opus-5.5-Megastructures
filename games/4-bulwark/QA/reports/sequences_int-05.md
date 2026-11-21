# Frame sequences: int-05

Sequences: 1. Checks: 2 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| lamps_b01 | transition | 289 | 0.516 | 0.665 | 1.098 | 142.7 | PASS |  |

## lamps_b01 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\seq_lamps_b01`, 289 frames at 30 fps, kind transition

- luma      `▅▆▆▇████▇▆▅▄▄▃▂▂▂▁▁▁▁▁▁▂▂▂▃▃▄▄▅▆▆▇████▇▆▅▄▄▃▂▂▁▁▁▁▁▁▂▂▂▂▂▃▄▄`
- mean diff `▅▆▇▇▆▆▅▅▆▅▅▃▃▃▃▂▂▂▁▁▁▂▂▂▃▃▂▃▄▄▅▅▆▆▆▆▅▆▆▄▄▃▃▃▃▂▂▂▂▂▃▄▅▇█▅▂▃▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5165 | <= 1.2 levels | at frame 184 |
| EV slope | INFO | 2.0363 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
