# Frame sequences: QA_bndF_seq_b23

Sequences: 1. Checks: 3 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| b23 | boundary | 601 | 0.0004 | 0.924 | 1.187 | 222.8 | 27 / 461 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## b23 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bndF\seq_b23`, 601 frames at 30 fps, kind boundary

- luma      `▇████████▇▇▇▇▆▆▆▅▅▅▄▄▃▃▂▂▂▂▂▁▁▁▁▁▁▁▁▂▂▂▂▂▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▃▃▃▃`
- mean diff `▇▆▇▇▇▇███▇▇▇▇▇▇▆▆▅▅▄▄▃▃▂▂▂▁▂▂▁▁▁▁▁▁▁▂▂▂▂▁▂▁▁▁▂▁▁▁▂▂▂▂▂▂▃▂▃▃▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1070 | <= 1.2 levels | at frame 205 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [400.0], worst at frame None |
| start transient (frame 1, after the pose jump) | INFO | 2 |  | not gated (C20); worst blink 32px@[192, 224] 3.2/0.25; blink 32px@[192, 256] 2.3/0.51 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 27 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [2, 29, 44, 82, 89, 152, 153, 155, 175, 216, 269, 375]; worst f152 s392.6 32px@[992, 544] 13.5/0.25; f153 s392.65 32px@[896, 544] 13.3/0.25; f153 s392.65 64px@[896, 512] 4.3/0.25; f152 s392.6 64px@[960, 512] 4.3/0.25; f457 s407.85 32px@[128, 320] 3.2/0.26; 636 rain-streak and 2023 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 461 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [3, 4, 5, 6, 8, 9, 10, 12, 14, 19, 22, 24]; worst f237 s396.85 32px@[1280, 0] 8.9/0.25; f474 s408.7 32px@[544, 0] 8.3/0.25; f259 s397.95 32px@[160, 512] 8.2/0.25; f322 s401.1 32px@[0, 224] 7.8/0.25; f194 s394.7 32px@[96, 512] 7.6/0.25; 636 rain-streak and 2023 motion-explained events excluded |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| pop | 152 | 392.60 | [992, 544, 1024, 576] | 32 | 13.50 | 0.25 |
| pop | 153 | 392.65 | [896, 544, 928, 576] | 32 | 13.34 | 0.25 |
| blink | 237 | 396.85 | [1280, 0, 1312, 32] | 32 | 8.89 | 0.25 |
| blink | 474 | 408.70 | [544, 0, 576, 32] | 32 | 8.33 | 0.25 |
| blink | 259 | 397.95 | [160, 512, 192, 544] | 32 | 8.19 | 0.25 |
| blink | 322 | 401.10 | [0, 224, 32, 256] | 32 | 7.79 | 0.25 |
| blink | 194 | 394.70 | [96, 512, 128, 544] | 32 | 7.64 | 0.25 |
| blink | 232 | 396.60 | [0, 224, 32, 256] | 32 | 7.52 | 0.25 |
| blink | 10 | 385.50 | [128, 512, 160, 544] | 32 | 7.37 | 0.25 |
| blink | 372 | 403.60 | [128, 512, 160, 544] | 32 | 7.06 | 0.25 |
| blink | 101 | 390.05 | [192, 512, 224, 544] | 32 | 7.03 | 0.25 |
| blink | 363 | 403.15 | [448, 0, 480, 32] | 32 | 7.03 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bndF\seq_b23\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bndF\seq_b23\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bndF\seq_b23\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bndF\seq_b23\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bndF\seq_b23\clip.mp4`

Frames deleted after analysis: 601 (analysed.done in the folder)