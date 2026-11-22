# Frame sequences: QA_bnd_seq_b12

Sequences: 1. Checks: 3 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| b12 | boundary | 601 | 0.0025 | 0.984 | 3.758 | 209.0 | 222 / 81 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## b12 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd\seq_b12`, 601 frames at 30 fps, kind boundary

- luma      `▇▇███▆▄▃▃▂▂▂▁▁▁▁▁▁▁▁▁▂▂▂▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄`
- mean diff `▇███▇▆▅▅▄▃▂▂▂▂▁▁▁▁▁▁▁▁▁▁▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6310 | <= 1.2 levels | at frame 54 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0006 | <= 0.5 level | boundaries [120.0], worst at frame 300 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 222 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 11, 29, 30, 79, 220, 226, 233, 241, 242, 243, 246]; worst f540 s132.0 32px@[1184, 576] 9.0/0.26; f540 s132.0 32px@[640, 576] 8.2/0.25; f540 s132.0 32px@[608, 640] 8.3/0.27; f540 s132.0 32px@[1152, 576] 8.3/0.28; f540 s132.0 32px@[896, 576] 6.4/0.25; 294 rain-streak events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 81 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [12, 19, 50, 183, 233, 234, 253, 254, 267, 268, 271, 273]; worst f521 s131.05 32px@[1760, 32] 5.9/0.25; f402 s125.1 32px@[1056, 224] 5.4/0.38; f521 s131.05 32px@[1760, 64] 3.1/0.25; f455 s127.75 32px@[1056, 96] 6.6/0.53; f414 s125.7 32px@[1696, 256] 3.0/0.26; 294 rain-streak events excluded |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| pop | 540 | 132.00 | [1184, 576, 1216, 608] | 32 | 8.97 | 0.26 |
| pop | 540 | 132.00 | [640, 576, 672, 608] | 32 | 8.17 | 0.25 |
| pop | 540 | 132.00 | [608, 640, 640, 672] | 32 | 8.29 | 0.27 |
| pop | 540 | 132.00 | [1152, 576, 1184, 608] | 32 | 8.33 | 0.28 |
| pop | 540 | 132.00 | [896, 576, 928, 608] | 32 | 6.39 | 0.25 |
| pop | 540 | 132.00 | [1152, 576, 1216, 640] | 64 | 7.66 | 0.31 |
| blink | 521 | 131.05 | [1760, 32, 1792, 64] | 32 | 5.90 | 0.25 |
| pop | 540 | 132.00 | [1184, 608, 1216, 640] | 32 | 8.19 | 0.35 |
| pop | 540 | 132.00 | [928, 576, 960, 608] | 32 | 5.50 | 0.25 |
| pop | 540 | 132.00 | [1088, 576, 1120, 608] | 32 | 5.47 | 0.25 |
| pop | 540 | 132.00 | [576, 640, 608, 672] | 32 | 5.17 | 0.25 |
| pop | 540 | 132.00 | [928, 608, 960, 640] | 32 | 5.70 | 0.28 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd\seq_b12\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd\seq_b12\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd\seq_b12\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd\seq_b12\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd\seq_b12\clip.mp4`

Frames deleted after analysis: 601 (analysed.done in the folder)