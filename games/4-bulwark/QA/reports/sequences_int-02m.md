# Frame sequences: int-02m

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| mat_pan_field_across | pan | 30 | 0.065 | 2.735 | 2.775 | 67.5 | PASS |  |

## mat_pan_field_across (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-02m\seq_mat_pan_field_across`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▁▁▁▁▂▂▂▂▃▃▄▄▄▄▄▅▅▅▆▆▆▇▇▇▇██`
- mean diff `▇▇▇▇▆█▅▇▅▅▃▃▅▆▄▆▅▆▃▇▃▅▄▇▄▄▇▆▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0649 | <= 2 levels | at frame 21, sun break |
| EV slope | INFO | 0.0341 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.1590 | <= 2 levels | exact rotation homography; raw 3.019, control floor 3.877, low-pass (sigma 1.5) 0.549, block-mean 0.186; gate block [384, 384] (raw); 92 of 420 blocks in the fine-pattern regime |
