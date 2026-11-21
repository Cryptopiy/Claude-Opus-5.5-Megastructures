# Frame sequences: kraken15

Sequences: 2. Checks: 5 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| kr_seq_dolly | transition | 600 | 0.341 | 2.651 | 3.620 | 136.0 | PASS |  |
| kr_pan_storm800 | pan | 30 | 0.002 | 0.204 | 0.208 | 69.8 | PASS |  |

## kr_seq_dolly (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-15s\kr_seq_dolly`, 600 frames at 30 fps, kind transition

- luma      `▁▃▄▅▅▅▆▆▆▇▇▇▇▇▇▇▇▇▇▇███▇███▇▇▇▇████▇███████████████████▇▇▇▇▇`
- mean diff `█▅▅▄▄▂▂▂▃▃▃▃▄▄▃▂▂▂▃▃▃▃▃▂▃▃▃▁▁▁▂▂▃▄▄▃▄▆▇▇▆▇█▇▇█▇▆▇▇▆▆▆▆▅▄▃▄▃▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.3412 | <= 1.2 levels | at frame 4 |
| EV slope | INFO | 0.2352 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## kr_pan_storm800 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-15p\kr_pan_storm800`, 30 frames at 30 fps, kind pan

- luma      `▂▁▂▂▂▂▂▃▃▃▃▄▄▅▅▅▅▅▆▆▆▆▇▇▇▇▇▇▇█`
- mean diff `▆▅▃▄▅▂▅█▃▆▂▇▅▁▅▆▅▃▄▄█▄▄▆▆▅▇▃▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0023 | <= 1.2 levels | at frame 7 |
| EV slope | INFO | 0.0017 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.0191 | <= 2 levels | exact rotation homography; raw 1.019, control floor 0.648, low-pass (sigma 1.5) 0.312, block-mean 0.100; gate block [1728, 320] (raw); 0 of 420 blocks in the fine-pattern regime |
