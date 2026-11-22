# Frame sequences: QA_bnd02

Sequences: 1. Checks: 3 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| b23 | boundary | 601 | 0.0004 | 0.924 | 1.187 | 222.8 | 1874 / 649 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## b23 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd02\seq_b23`, 601 frames at 30 fps, kind boundary

- luma      `▇████████▇▇▇▇▆▆▆▅▅▅▄▄▃▃▂▂▂▂▂▁▁▁▁▁▁▁▁▂▂▂▂▂▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▃▃▃▃`
- mean diff `▇▆▇▇▇▇███▇▇▇▇▇▇▆▆▅▅▄▄▃▃▂▂▂▁▂▂▁▁▁▁▁▁▁▂▂▂▂▁▂▁▁▁▂▁▁▁▂▂▂▂▂▂▃▂▃▃▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1070 | <= 1.2 levels | at frame 205 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [400.0], worst at frame None |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 1874 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15]; worst f152 s392.6 32px@[992, 544] 13.5/0.25; f153 s392.65 32px@[896, 544] 13.3/0.25; f600 s415.0 32px@[736, 480] 8.7/0.34; f600 s415.0 32px@[1152, 480] 9.0/0.36; f22 s386.1 32px@[512, 256] 5.4/0.25; 626 rain-streak events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 649 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 3, 4, 5, 6, 8, 9, 10, 11, 12, 14, 19]; worst f262 s398.1 32px@[544, 160] 9.7/0.25; f237 s396.85 32px@[1280, 0] 8.9/0.25; f474 s408.7 32px@[544, 0] 8.3/0.25; f259 s397.95 32px@[160, 512] 8.2/0.25; f322 s401.1 32px@[0, 224] 7.8/0.25; 626 rain-streak events excluded |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| pop | 152 | 392.60 | [992, 544, 1024, 576] | 32 | 13.50 | 0.25 |
| pop | 153 | 392.65 | [896, 544, 928, 576] | 32 | 13.34 | 0.25 |
| blink | 262 | 398.10 | [544, 160, 576, 192] | 32 | 9.67 | 0.25 |
| blink | 237 | 396.85 | [1280, 0, 1312, 32] | 32 | 8.89 | 0.25 |
| blink | 474 | 408.70 | [544, 0, 576, 32] | 32 | 8.33 | 0.25 |
| blink | 259 | 397.95 | [160, 512, 192, 544] | 32 | 8.19 | 0.25 |
| blink | 322 | 401.10 | [0, 224, 32, 256] | 32 | 7.79 | 0.25 |
| blink | 194 | 394.70 | [96, 512, 128, 544] | 32 | 7.64 | 0.25 |
| blink | 232 | 396.60 | [0, 224, 32, 256] | 32 | 7.52 | 0.25 |
| blink | 10 | 385.50 | [128, 512, 160, 544] | 32 | 7.37 | 0.25 |
| blink | 372 | 403.60 | [128, 512, 160, 544] | 32 | 7.06 | 0.25 |
| blink | 101 | 390.05 | [192, 512, 224, 544] | 32 | 7.03 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd02\seq_b23\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd02\seq_b23\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd02\seq_b23\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd02\seq_b23\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd02\seq_b23\clip.mp4`

Frames deleted after analysis: 601 (analysed.done in the folder)