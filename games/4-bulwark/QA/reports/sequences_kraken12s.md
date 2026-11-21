# Frame sequences: kraken12s

Sequences: 1. Checks: 2 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| kr_seq_dolly | transition | 600 | 0.341 | 2.652 | 3.616 | 135.0 | PASS |  |

## kr_seq_dolly (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-12s\kr_seq_dolly`, 600 frames at 30 fps, kind transition

- luma      `▁▃▄▅▅▅▆▆▆▇▇▇▇▇▇▇▇▇▇▇███▇███▇▇▇▇████▇███████████████████▇▇▇▇▇`
- mean diff `█▅▅▄▄▂▂▂▃▃▃▃▄▄▃▂▂▂▃▃▃▃▃▂▃▃▃▁▁▁▂▂▃▄▄▃▄▆▇▇▆▇█▇▇█▇▆▇▇▆▆▆▆▅▄▃▄▃▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.3414 | <= 1.2 levels | at frame 4 |
| EV slope | INFO | 0.2354 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
