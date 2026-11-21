# Frame sequences: mistx-10-seq

Sequences: 4. Checks: 14 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| mist_ks08_static | static | 120 | 0.001 | 0.007 | 0.008 | 84.7 | PASS |  |
| mist_ks09_pan58 | pan | 30 | 0.036 | 0.867 | 0.899 | 100.0 | PASS |  |
| mist_ks09_pan89 | pan | 30 | 0.017 | 1.017 | 1.024 | 71.8 | PASS |  |
| mist_ks09_static | static | 120 | 0.013 | 0.058 | 0.070 | 55.1 | PASS |  |

## mist_ks08_static (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-10-seq\seq_mist_ks08_static`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▂▂▂▂▂▃▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄▅▅▅▅▅▅▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇██████`
- mean diff `▄▄▄▄▄▄▄▃▃▃▂▃▂▄▃▃▂▂▂▁▂▃▃▂▃▄▃▃▅▄▄▅▅▆▇▇▆▇▆▆▆▇▆▇▇▇█▇▇▇█▇▇█▆▆▆▆▆▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0012 | <= 2 levels | at frame 2, sun break |
| EV slope | INFO | 0.0010 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.7805 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## mist_ks09_pan58 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-10-seq\seq_mist_ks09_pan58`, 30 frames at 30 fps, kind pan

- luma      `▅▆▆▇▇▇▇▆▆▇▆▇▇▇▆▁▆██▆▆▆▆▆▆▆▆▆▅▅`
- mean diff `▅▄▆▃▄▅▄▄▄▅▄▄▄▂▆█▁▅▅▄▆▂▆▃▅▆▃▄▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0362 | <= 2 levels | at frame 16, sun break |
| EV slope | INFO | 0.0352 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.3752 | <= 2 levels | exact rotation homography; raw 1.432, control floor 1.056, low-pass (sigma 1.5) 0.724, block-mean 0.285; gate block [704, 448] (raw); 1 of 420 blocks in the fine-pattern regime |

## mist_ks09_pan89 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-10-seq\seq_mist_ks09_pan89`, 30 frames at 30 fps, kind pan

- luma      `▆▆▇▆▇█████▇██▇▇▅█▇▇▇▆▇▆▆▅▅▄▂▂▁`
- mean diff `▄▃▄▃▃▄▄▂▆▅▁█▆▂▃▇▂▅█▄▄▆▆▄▂▂▂▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0166 | <= 2 levels | at frame 16, sun break |
| EV slope | INFO | 0.0095 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.1239 | <= 2 levels | exact rotation homography; raw 1.124, control floor 0.831, low-pass (sigma 1.5) 0.560, block-mean 0.240; gate block [384, 256] (raw); 0 of 420 blocks in the fine-pattern regime |

## mist_ks09_static (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-10-seq\seq_mist_ks09_static`, 120 frames at 30 fps, kind static

- luma      `▄▅▆▇▇▆▆▅▄▃▃▃▃▃▃▃▂▂▂▃▃▄▄▄▄▄▄▄▃▃▃▃▃▄▄▅▅▅▅▅▄▄▃▂▁▁▂▄▅▆▇███▇▇▆▆▆▅`
- mean diff `▅▅▅▆▆▄▄▄▄▄▄▄▄▄▄▃▂▃▃▂▁▁▂▂▂▂▂▂▂▃▄▂▂▂▂▂▃▂▃▃▂▂▃▂▁▁▂▃▄▅▄▄▄▅▄▅▅▇██`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0129 | <= 2 levels | at frame 85, sun break |
| EV slope | INFO | 0.0140 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 1.8545 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
