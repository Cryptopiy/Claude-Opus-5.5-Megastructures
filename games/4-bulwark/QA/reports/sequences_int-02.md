# Frame sequences: int-02

Sequences: 1. Checks: 2 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b04_b05 | transition | 601 | 4.324 | 1.468 | 6.822 | 227.7 | FAIL | luma step/frame |

## b04_b05 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-02\seq_b04_b05`, 601 frames at 30 fps, kind transition

- luma      `██████████████████▇▇▇▇▇▇▆▆▅▄▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▃▃▃▂▂▂`
- mean diff `▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▅▃▄▅▆█▄▃▂▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▁▁▁▂▂▂▄▄█▆▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 4.3237 | <= 1.2 levels | at frame 279 |
| activation-frame diff | PASS | 0.0081 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 7.2208 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
