# Frame sequences: INTB_bnd2_seq_b12

Sequences: 1. Checks: 3 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| b12 | boundary | 601 | 0.0024 | 1.000 | 3.648 | 209.0 | 115 / 79 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## b12 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd2\seq_b12`, 601 frames at 30 fps, kind boundary

- luma      `▇▇███▆▄▃▂▂▂▂▁▁▁▁▁▁▁▁▁▂▂▃▃▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▅▅▅▅▅▅▅▅▅▅`
- mean diff `▇███▇▆▅▅▄▃▂▂▂▂▁▁▁▁▁▁▁▁▁▁▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6020 | <= 1.2 levels | at frame 54 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0022 | <= 0.5 level | boundaries [120.0], worst at frame 300 |
| start transient (frame 1, after the pose jump) | INFO | 1 |  | not gated (C20); worst pop 32px@[192, 288] 13.2/1.29 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 115 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [220, 226, 233, 241, 242, 243, 246, 248, 249, 251, 254, 540]; worst f540 s132.0 32px@[640, 576] 10.9/0.34; f540 s132.0 32px@[1184, 576] 11.0/0.38; f540 s132.0 32px@[960, 576] 7.0/0.25; f540 s132.0 32px@[896, 576] 7.5/0.28; f540 s132.0 32px@[928, 576] 6.5/0.25; 254 rain-streak and 140 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 79 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [15, 19, 47, 50, 183, 233, 234, 253, 254, 267, 268, 271]; worst f521 s131.05 32px@[1760, 32] 5.9/0.25; f402 s125.1 32px@[1056, 224] 5.4/0.38; f521 s131.05 32px@[1760, 64] 3.1/0.25; f414 s125.7 32px@[1696, 256] 3.0/0.26; f559 s132.95 32px@[1760, 256] 2.6/0.26; 254 rain-streak and 140 motion-explained events excluded |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| pop | 540 | 132.00 | [640, 576, 672, 608] | 32 | 10.86 | 0.34 |
| pop | 540 | 132.00 | [1184, 576, 1216, 608] | 32 | 10.99 | 0.38 |
| pop | 540 | 132.00 | [960, 576, 992, 608] | 32 | 7.03 | 0.25 |
| pop | 540 | 132.00 | [896, 576, 928, 608] | 32 | 7.53 | 0.28 |
| pop | 540 | 132.00 | [928, 576, 960, 608] | 32 | 6.45 | 0.25 |
| blink | 521 | 131.05 | [1760, 32, 1792, 64] | 32 | 5.90 | 0.25 |
| pop | 540 | 132.00 | [1152, 576, 1184, 608] | 32 | 9.82 | 0.43 |
| pop | 540 | 132.00 | [1152, 576, 1216, 640] | 64 | 9.47 | 0.42 |
| pop | 540 | 132.00 | [1184, 608, 1216, 640] | 32 | 10.58 | 0.47 |
| pop | 540 | 132.00 | [576, 640, 608, 672] | 32 | 5.61 | 0.27 |
| pop | 540 | 132.00 | [896, 576, 960, 640] | 64 | 6.39 | 0.31 |
| pop | 600 | 135.00 | [1056, 224, 1088, 256] | 32 | 6.16 | 0.32 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd2\seq_b12\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd2\seq_b12\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd2\seq_b12\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd2\seq_b12\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd2\seq_b12\clip.mp4`

Frames deleted after analysis: 601 (analysed.done in the folder)