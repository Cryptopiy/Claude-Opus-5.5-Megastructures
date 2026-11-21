# Frame sequences: kraken33s

Sequences: 2. Checks: 5 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| kr_seq_dolly | transition | 600 | 0.294 | 3.155 | 4.287 | 132.9 | PASS |  |
| kr_pan_storm800 | pan | 30 | 0.002 | 0.185 | 0.190 | 54.2 | PASS |  |

## kr_seq_dolly (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-33s\kr_seq_dolly`, 600 frames at 30 fps, kind transition

- luma      `▁▂▄▄▅▅▆▆▆▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇█▇▇▇▇▇▇▇██▇▇██████████████████▇▇▇▇▇`
- mean diff `█▅▄▄▄▂▂▂▃▃▃▃▄▄▃▂▂▂▂▂▃▃▃▂▃▃▃▁▁▁▂▂▃▄▄▃▄▆▆▇▆▇█▇▇▇▇▆▇▇▇▇▆▆▅▄▄▄▄▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2941 | <= 1.2 levels | at frame 4 |
| EV slope | INFO | 0.2026 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## kr_pan_storm800 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\kraken-33s\kr_pan_storm800`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▂▂▂▂▃▃▃▃▃▃▄▄▅▅▅▆▆▆▆▆▆▆▇▇█▇█`
- mean diff `▅▅▃▄▅▂▅█▂▆▂▇▅▁▅▆▅▃▃▄█▄▃▅▇▅▆▃▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0019 | <= 1.2 levels | at frame 27 |
| EV slope | INFO | 0.0015 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 0.8958 | <= 2 levels | exact rotation homography; raw 0.896, control floor 0.648, low-pass (sigma 1.5) 0.258, block-mean 0.092; gate block [1408, 704] (raw); 0 of 420 blocks in the fine-pattern regime |
