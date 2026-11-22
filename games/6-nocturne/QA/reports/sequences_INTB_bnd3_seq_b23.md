# Frame sequences: INTB_bnd3_seq_b23

Sequences: 1. Checks: 3 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| b23 | boundary | 601 | 0.0003 | 0.960 | 1.183 | 222.0 | 9 / 461 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## b23 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd3\seq_b23`, 601 frames at 30 fps, kind boundary

- luma      `██████████▇▇▇▇▆▆▆▅▅▅▄▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▃▃▃▃`
- mean diff `▇▇▇▇▇▇█▇█▇▇▇▇▇█▇▇▆▆▅▄▄▃▃▃▃▂▂▂▂▂▂▂▂▂▂▃▃▃▃▂▃▁▂▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0810 | <= 1.2 levels | at frame 203 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [400.0], worst at frame None |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 2 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f259 s397.95 32px@[128, 480] 3.3 (nb8 median 1.6); f259 s397.95 32px@[160, 480] 2.9 (nb8 median 2.6) |
| start transient (frame 1, after the pose jump) | INFO | 2 |  | not gated (C20); worst blink 32px@[192, 224] 3.2/0.25; blink 32px@[192, 256] 2.3/0.51 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 9 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [29, 44, 82, 89, 269, 450, 457, 587, 600]; worst f457 s407.85 32px@[128, 320] 3.2/0.25; f29 s386.45 32px@[608, 320] 2.3/0.25; f450 s407.5 32px@[288, 224] 2.3/0.25; f587 s414.35 32px@[576, 320] 2.2/0.25; f44 s387.2 32px@[608, 320] 2.5/0.51; 644 rain-streak and 1684 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 461 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [2, 3, 4, 5, 6, 8, 9, 10, 12, 14, 19, 22]; worst f237 s396.85 32px@[1280, 0] 8.9/0.25; f474 s408.7 32px@[544, 0] 8.3/0.25; f259 s397.95 32px@[160, 512] 8.2/0.25; f322 s401.1 32px@[0, 224] 7.8/0.25; f194 s394.7 32px@[96, 512] 7.6/0.25; 644 rain-streak and 1684 motion-explained events excluded |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 237 | 396.85 | [1280, 0, 1312, 32] | 32 | 8.89 | 0.25 |
| blink | 474 | 408.70 | [544, 0, 576, 32] | 32 | 8.33 | 0.25 |
| blink | 259 | 397.95 | [160, 512, 192, 544] | 32 | 8.20 | 0.25 |
| blink | 322 | 401.10 | [0, 224, 32, 256] | 32 | 7.79 | 0.25 |
| blink | 194 | 394.70 | [96, 512, 128, 544] | 32 | 7.61 | 0.25 |
| blink | 203 | 395.15 | [96, 512, 128, 544] | 32 | 7.57 | 0.25 |
| blink | 232 | 396.60 | [0, 224, 32, 256] | 32 | 7.52 | 0.25 |
| blink | 10 | 385.50 | [128, 512, 160, 544] | 32 | 7.33 | 0.25 |
| blink | 358 | 402.90 | [544, 320, 576, 352] | 32 | 7.25 | 0.25 |
| blink | 372 | 403.60 | [128, 512, 160, 544] | 32 | 7.06 | 0.25 |
| blink | 363 | 403.15 | [448, 0, 480, 32] | 32 | 7.03 | 0.25 |
| blink | 101 | 390.05 | [192, 512, 224, 544] | 32 | 7.02 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd3\seq_b23\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd3\seq_b23\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd3\seq_b23\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd3\seq_b23\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_bnd3\seq_b23\clip.mp4`

Frames deleted after analysis: 601 (analysed.done in the folder)