# Frame sequences: QA_cin_seq_cin_s0380

Sequences: 1. Checks: 7 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s0380 | cinematic | 720 | 0.0007 | 3.251 | 5.857 | 229.0 | 574 / 189 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## cin_s0380 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0380`, 720 frames at 30 fps, kind cinematic

- luma      `████████▇▇▇▆▆▅▅▄▃▃▂▂▂▂▁▁▁▁▁▁▂▂▃▃▃▄▅▅▅▆▆▆▇▇▇▇▇▇▇▇▇▇██▇▇▇▇▇▇▇▇`
- mean diff `▂▂▁▂▂▃▃▄▄▄▅▅▅▅▄▄▃▃▂▂▂▄▄▃▂▂▄▅▆▇███████▇▇▇▇▇▇▇▇▅▄▂▁▁▁▁▁▁▁▁▁▂▄▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1840 | <= 1.2 levels | at frame 185 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 574 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 14]; worst f7 s380.62 32px@[608, 448] 5.6/0.25; f38 s383.21 32px@[608, 448] 5.6/0.26; f38 s383.21 32px@[576, 448] 5.6/0.27; f7 s380.62 32px@[608, 192] 5.1/0.25; f15 s381.29 32px@[576, 224] 7.7/0.42; 28 rain-streak and 32 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 189 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 3, 5, 6, 7, 9, 11, 13, 14, 15, 16, 17]; worst f113 s389.46 32px@[992, 704] 5.5/0.25; f230 s399.21 32px@[992, 576] 4.4/0.25; f6 s380.54 32px@[544, 448] 6.1/0.36; f113 s389.46 32px@[896, 704] 3.2/0.25; f59 s384.96 32px@[896, 736] 3.1/0.25; 28 rain-streak and 32 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 11.4300 | < 25 % of the screen | worst frame 424; 0 frames over; 26996 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.3400 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 1.5417 | <= 0.5 Hz (amplitude floor 0.002) | 8038 of 8040 blocks modulated, 196 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00625 (<= 0.002); df 0.0417 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.5180 | <= 0.3 % / frame | at frame 185; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6262 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.012 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 9.7770 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| pop | 7 | 380.62 | [608, 448, 640, 480] | 32 | 5.57 | 0.25 |
| blink | 113 | 389.46 | [992, 704, 1024, 736] | 32 | 5.48 | 0.25 |
| pop | 38 | 383.21 | [608, 448, 640, 480] | 32 | 5.60 | 0.26 |
| pop | 38 | 383.21 | [576, 448, 608, 480] | 32 | 5.57 | 0.27 |
| pop | 7 | 380.62 | [608, 192, 640, 224] | 32 | 5.11 | 0.25 |
| pop | 15 | 381.29 | [576, 224, 608, 256] | 32 | 7.74 | 0.42 |
| blink | 230 | 399.21 | [992, 576, 1024, 608] | 32 | 4.44 | 0.25 |
| pop | 7 | 380.62 | [448, 224, 480, 256] | 32 | 5.19 | 0.29 |
| pop | 7 | 380.62 | [576, 448, 608, 480] | 32 | 5.41 | 0.31 |
| blink | 6 | 380.54 | [544, 448, 576, 480] | 32 | 6.09 | 0.36 |
| pop | 298 | 404.88 | [288, 544, 320, 576] | 32 | 5.05 | 0.34 |
| pop | 37 | 383.12 | [544, 192, 576, 224] | 32 | 5.53 | 0.38 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0380\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0380\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0380\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0380\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0380\clip.mp4`

Frames deleted after analysis: 720 (analysed.done in the folder)