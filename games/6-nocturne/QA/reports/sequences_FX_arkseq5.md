# Frame sequences: FX_arkseq5

Sequences: 2. Checks: 14 PASS, 4 FAIL, 0 WARN, 2 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| ark_cin_bow | dread | 432 | 0.0047 | 4.266 | 7.855 | 229.9 | 41 / 576 | FAIL | block pops (one-frame jumps), 11/21 px blocks, block blinks (out and back), 11/21 px blocks |
| ark_cin_coming | dread | 720 | 0.0015 | 2.991 | 7.480 | 222.2 | 95 / 549 | FAIL | block pops (one-frame jumps), 11/21 px blocks, block blinks (out and back), 11/21 px blocks |

## ark_cin_bow (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_bow`, 432 frames at 30 fps, kind dread

- luma      `▁▂▃▃▃▂▂▁▁▁▁▁▂▂▃▃▃▃▄▄▄▅▅▅▅▅▅▅▆▆▆▇█████▆▄▄▄▄▄▄▄▄▄▄▄▄▃▃▃▃▂▂▁▁▁▁`
- mean diff `▄▄▄▄▄▃▂▂▁▂▅▆▇█████▇▇▇▆▄▃▂▁▁▁▂▂▂▂▃▃▃▃▃▃▁▂▃▅▆▇▇█▇▇▇▇▇▆▆▄▃▂▂▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 1.1920 | <= 1.2 levels | at frame 266 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 9 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f263 s585.96 11px@[11, 187] 19.3 (nb8 median 5.7); f262 s585.88 11px@[0, 220] 13.4 (nb8 median 6.3); f336 s592.04 11px@[0, 242] 13.1 (nb8 median 4.6); f331 s591.62 11px@[0, 0] 10.5 (nb8 median 5.1) |
| start transient (frame 1, after the pose jump) | INFO | 2 |  | not gated (C20); worst pop 11px@[154, 165] 2.4/0.42; pop 11px@[132, 187] 2.1/0.37 |
| block pops (one-frame jumps), 11/21 px blocks | FAIL | 41 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [11, 27, 31, 33, 41, 98, 105, 243, 244, 248, 252, 254]; worst f431 s599.96 11px@[187, 154] 2.9/0.30; f431 s599.96 11px@[275, 187] 19.1/2.07; f431 s599.96 11px@[187, 308] 17.6/2.16; f269 s586.46 11px@[55, 0] 131.4/20.17; f431 s599.96 11px@[517, 275] 2.1/0.39; 57 rain-streak and 724 motion-explained events excluded |
| block blinks (out and back), 11/21 px blocks | FAIL | 576 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [6, 8, 9, 28, 30, 32, 33, 34, 35, 36, 37, 38]; worst f40 s567.38 11px@[0, 330] 16.0/0.25; f37 s567.12 11px@[44, 319] 12.9/0.34; f334 s591.88 11px@[0, 308] 11.6/0.41; f333 s591.79 11px@[0, 341] 9.4/0.34; f305 s589.46 11px@[110, 55] 8.2/0.30; 57 rain-streak and 724 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 16.3600 | < 25 % of the screen | worst frame 109; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 74950 block transitions in all; 5 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 42.2300 | < 25 % of the screen (static camera) | worst frame 132; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 2.0800 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.6000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 5.0694 | <= 0.5 Hz (amplitude floor 0.002) | 9114 of 9216 blocks modulated, 992 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.01608 (<= 0.002); df 0.0694 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 2.7402 | <= 0.3 % / frame | at frame 266; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.7562 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.814 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 5 px blocks |
| mean change per second (linear) | INFO | 36.5960 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 40 | 567.38 | [0, 330, 11, 341] | 11 | 15.98 | 0.25 |
| blink | 37 | 567.12 | [44, 319, 55, 330] | 11 | 12.89 | 0.34 |
| blink | 334 | 591.88 | [0, 308, 11, 319] | 11 | 11.57 | 0.41 |
| blink | 333 | 591.79 | [0, 341, 11, 352] | 11 | 9.40 | 0.34 |
| blink | 305 | 589.46 | [110, 55, 121, 66] | 11 | 8.16 | 0.30 |
| blink | 328 | 591.38 | [121, 77, 132, 88] | 11 | 5.82 | 0.25 |
| blink | 318 | 590.54 | [77, 0, 88, 11] | 11 | 14.41 | 0.69 |
| blink | 37 | 567.12 | [44, 330, 55, 341] | 11 | 11.61 | 0.56 |
| blink | 325 | 591.12 | [132, 88, 143, 99] | 11 | 5.11 | 0.25 |
| blink | 378 | 595.54 | [77, 341, 88, 352] | 11 | 13.96 | 0.70 |
| blink | 345 | 592.79 | [99, 33, 110, 44] | 11 | 5.68 | 0.29 |
| blink | 263 | 585.96 | [506, 275, 517, 286] | 11 | 4.73 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_bow\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_bow\montage_02.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_bow\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_bow\clip.mp4`
## ark_cin_coming (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_coming`, 720 frames at 30 fps, kind dread

- luma      `▁▁▁▂▂▂▂▃▃▄▄▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▅▅▅▅▆▅▆▆▆▆▅▅▅▇███████`
- mean diff `▁▁▁▂▂▂▂▂▂▃▄▅▅▆▇████▇▆▄▃▂▁▁▁▁▁▁▁▁▂▂▃▄▅▆▇▇▇▇▇▇▆▆▅▄▄▃▂▂▂▂▂▂▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.3740 | <= 1.2 levels | at frame 586 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 7 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f389 s502.46 11px@[418, 319] 4.5 (nb8 median 1.6); f431 s505.96 11px@[297, 308] 3.8 (nb8 median 1.8); f411 s504.29 11px@[363, 319] 3.1 (nb8 median 1.3); f264 s492.04 11px@[363, 319] 3.0 (nb8 median 1.8) |
| start transient (frame 1, after the pose jump) | INFO | 1 |  | not gated (C20); worst pop 11px@[473, 165] 2.1/0.52 |
| block pops (one-frame jumps), 11/21 px blocks | FAIL | 95 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [7, 14, 15, 22, 23, 74, 76, 79, 105, 109, 182, 267]; worst f637 s523.12 11px@[187, 319] 22.9/1.74; f719 s529.96 11px@[99, 253] 18.7/1.61; f719 s529.96 11px@[99, 330] 10.8/1.09; f716 s529.71 11px@[99, 121] 2.5/0.25; f649 s524.12 11px@[110, 286] 29.4/3.22; 411 rain-streak and 1646 motion-explained events excluded |
| block blinks (out and back), 11/21 px blocks | FAIL | 549 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [2, 6, 7, 9, 12, 15, 28, 39, 41, 42, 47, 48]; worst f282 s493.54 11px@[44, 330] 9.2/0.25; f653 s524.46 11px@[22, 154] 9.0/0.25; f688 s527.38 11px@[11, 154] 8.1/0.25; f305 s495.46 11px@[594, 198] 7.9/0.25; f310 s495.88 11px@[605, 198] 7.3/0.25; 411 rain-streak and 1646 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 9.5900 | < 25 % of the screen | worst frame 211; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 70290 block transitions in all; 5 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 33.3600 | < 25 % of the screen (static camera) | worst frame 214; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.6000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.3000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 2.4583 | <= 0.5 Hz (amplitude floor 0.002) | 9215 of 9216 blocks modulated, 833 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00765 (<= 0.002); df 0.0417 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.9805 | <= 0.3 % / frame | at frame 586; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.7380 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.434 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 5 px blocks |
| mean change per second (linear) | INFO | 15.7120 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 282 | 493.54 | [44, 330, 55, 341] | 11 | 9.22 | 0.25 |
| blink | 653 | 524.46 | [22, 154, 33, 165] | 11 | 8.96 | 0.25 |
| blink | 688 | 527.38 | [11, 154, 22, 165] | 11 | 8.12 | 0.25 |
| blink | 305 | 495.46 | [594, 198, 605, 209] | 11 | 7.86 | 0.25 |
| blink | 310 | 495.88 | [605, 198, 616, 209] | 11 | 7.31 | 0.25 |
| blink | 337 | 498.12 | [11, 330, 22, 341] | 11 | 6.87 | 0.27 |
| blink | 666 | 525.54 | [66, 165, 77, 176] | 11 | 6.30 | 0.25 |
| blink | 500 | 511.71 | [550, 143, 561, 154] | 11 | 6.11 | 0.25 |
| blink | 282 | 493.54 | [473, 297, 484, 308] | 11 | 5.74 | 0.25 |
| blink | 287 | 493.96 | [473, 297, 484, 308] | 11 | 5.66 | 0.25 |
| blink | 239 | 489.96 | [242, 308, 253, 319] | 11 | 6.42 | 0.30 |
| blink | 404 | 503.71 | [594, 308, 605, 319] | 11 | 4.94 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_coming\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_coming\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_coming\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_coming\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq5\seq_ark_cin_coming\clip.mp4`