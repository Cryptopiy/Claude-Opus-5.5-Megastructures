# Frame sequences: final_ark

Sequences: 4. Checks: 23 PASS, 8 FAIL, 0 WARN, 4 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| ark_cin_belly | dread | 1272 | 0.0027 | 4.496 | 10.252 | 232.0 | 53 / 458 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |
| ark_cin_bow | dread | 432 | 0.0032 | 6.291 | 10.941 | 228.7 | 52 / 245 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |
| ark_cin_coming | dread | 720 | 0.0012 | 4.090 | 9.215 | 230.1 | 54 / 599 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |
| ark_emerge | boundary | 502 | 0.0004 | 1.504 | 1.870 | 229.0 | 3 / 355 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## ark_cin_belly (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_belly`, 1272 frames at 30 fps, kind dread

- luma      `▂▂▂▂▂▂▂▃▃▄▆▆▇▇▆▄▄▄▄▄▄▄▄▄▄▄▄▆███▆▄▄▄▄▄▄▄▄▄▄▄▄▆▇▇▇▄▄▄▄▃▄▃▁▁▁▂▂`
- mean diff `▅▆▅▅▃▂▃▄▅▅▆▆▆▆▄▂▁▁▁▁▁▁▁▁▁▂▃▅▆▆▆▅▄▄▃▂▂▁▁▁▁▂▁▁▂▂▂▃▄▅▆▇███▇▆▄▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6830 | <= 1.2 levels | at frame 590 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 16 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f13 s741.12 32px@[0, 864] 29.3 (nb8 median 16.1); f13 s741.12 32px@[0, 832] 25.3 (nb8 median 7.6); f37 s743.12 32px@[96, 992] 23.2 (nb8 median 11.8); f13 s741.12 32px@[32, 992] 20.6 (nb8 median 5.7) |
| start transient (frame 1, after the pose jump) | INFO | 3 |  | not gated (C20); worst blink 32px@[448, 640] 14.3/1.94; pop 32px@[448, 0] 14.7/2.94; pop 32px@[480, 640] 12.4/3.76 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 53 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [13, 264, 276, 284, 368, 373, 381, 383, 505, 525, 544, 606]; worst f1271 s845.96 32px@[448, 960] 3.1/0.55; f1000 s823.38 32px@[416, 800] 101.4/21.11; f1015 s824.62 32px@[0, 704] 131.2/27.33; f993 s822.79 32px@[416, 800] 100.8/21.12; f368 s770.71 32px@[1728, 800] 39.0/8.78; 121 rain-streak and 260 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 458 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]; worst f33 s742.79 32px@[288, 928] 26.9/0.40; f36 s743.04 32px@[160, 992] 31.6/0.48; f57 s744.79 32px@[352, 1024] 23.1/0.47; f36 s743.04 32px@[160, 960] 32.4/0.67; f12 s741.04 32px@[96, 992] 22.2/0.71; 121 rain-streak and 260 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 9.7300 | < 25 % of the screen | worst frame 1142; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 83431 block transitions in all; 16 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 29.8300 | < 25 % of the screen (static camera) | worst frame 1160; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 3.0700 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 1.8396 | <= 0.5 Hz (amplitude floor 0.002) | 8038 of 8040 blocks modulated, 119 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00551 (<= 0.002); df 0.0236 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 1.6583 | <= 0.3 % / frame | at frame 590; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.5878 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 38.0810 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 33 | 742.79 | [288, 928, 320, 960] | 32 | 26.89 | 0.40 |
| blink | 36 | 743.04 | [160, 992, 192, 1024] | 32 | 31.59 | 0.48 |
| blink | 57 | 744.79 | [352, 1024, 384, 1056] | 32 | 23.06 | 0.47 |
| blink | 36 | 743.04 | [160, 960, 192, 992] | 32 | 32.38 | 0.67 |
| blink | 12 | 741.04 | [96, 992, 128, 1024] | 32 | 22.21 | 0.71 |
| blink | 11 | 740.96 | [160, 992, 192, 1024] | 32 | 29.31 | 1.24 |
| blink | 446 | 777.21 | [0, 800, 32, 832] | 32 | 7.71 | 0.34 |
| blink | 11 | 740.96 | [160, 960, 192, 992] | 32 | 21.08 | 1.19 |
| blink | 34 | 742.88 | [256, 928, 288, 960] | 32 | 29.00 | 1.68 |
| blink | 368 | 770.71 | [1408, 416, 1440, 448] | 32 | 4.20 | 0.25 |
| blink | 39 | 743.29 | [0, 1024, 32, 1056] | 32 | 42.58 | 2.65 |
| blink | 342 | 768.54 | [1760, 448, 1792, 480] | 32 | 3.99 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_belly\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_belly\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_belly\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_belly\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_belly\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_belly\clip.mp4`
## ark_cin_bow (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_bow`, 432 frames at 30 fps, kind dread

- luma      `▂▂▂▃▂▂▁▁▁▁▁▁▁▂▃▃▄▄▅▅▅▆▆▆▆▆▆▆▆▇▇▇█████▆▅▅▅▅▅▅▅▄▄▄▄▄▃▃▃▃▂▂▁▁▁▁`
- mean diff `▄▅▅▅▅▄▃▃▂▄▆▇███▇▇▇▇▇▆▅▅▄▃▂▁▂▂▃▃▃▃▃▃▃▃▃▂▃▅▅▆▆▇▇▇▇▇▇▆▆▆▆▅▃▃▂▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.8220 | <= 1.2 levels | at frame 267 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 4 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f265 s586.12 32px@[0, 448] 49.2 (nb8 median 16.1); f39 s567.29 32px@[32, 992] 14.6 (nb8 median 7.2); f39 s567.29 32px@[64, 992] 13.3 (nb8 median 7.2); f262 s585.88 32px@[0, 672] 7.5 (nb8 median 5.9) |
| start transient (frame 1, after the pose jump) | INFO | 1 |  | not gated (C20); worst pop 32px@[512, 448] 12.0/3.26 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 52 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [31, 32, 33, 35, 103, 129, 135, 157, 244, 246, 248, 249]; worst f157 s577.12 32px@[32, 1024] 2.5/0.32; f269 s586.46 32px@[160, 0] 129.9/17.16; f264 s586.04 32px@[352, 96] 107.3/15.53; f263 s585.96 32px@[384, 128] 105.5/16.89; f265 s586.12 32px@[320, 64] 110.4/17.86; 22 rain-streak and 202 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 245 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [3, 13, 27, 31, 32, 33, 34, 35, 36, 37, 38, 40]; worst f427 s599.62 32px@[128, 992] 24.3/0.25; f62 s569.21 32px@[0, 1024] 21.2/0.54; f422 s599.21 32px@[288, 1024] 19.2/0.56; f62 s569.21 32px@[0, 992] 12.2/0.38; f40 s567.38 32px@[0, 992] 27.0/0.95; 22 rain-streak and 202 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 12.9200 | < 25 % of the screen | worst frame 101; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 47552 block transitions in all; 16 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 32.1100 | < 25 % of the screen (static camera) | worst frame 115; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 4.3200 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 3.5417 | <= 0.5 Hz (amplitude floor 0.002) | 8035 of 8040 blocks modulated, 521 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.01595 (<= 0.002); df 0.0694 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 1.9593 | <= 0.3 % / frame | at frame 267; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.5831 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 33.5150 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 427 | 599.62 | [128, 992, 160, 1024] | 32 | 24.29 | 0.25 |
| blink | 62 | 569.21 | [0, 1024, 32, 1056] | 32 | 21.25 | 0.54 |
| blink | 422 | 599.21 | [288, 1024, 320, 1056] | 32 | 19.24 | 0.56 |
| blink | 62 | 569.21 | [0, 992, 32, 1024] | 32 | 12.16 | 0.38 |
| blink | 40 | 567.38 | [0, 992, 32, 1024] | 32 | 26.99 | 0.95 |
| blink | 38 | 567.21 | [96, 992, 128, 1024] | 32 | 23.87 | 1.13 |
| blink | 260 | 585.71 | [224, 480, 256, 512] | 32 | 62.56 | 3.03 |
| blink | 262 | 585.88 | [160, 448, 192, 480] | 32 | 62.08 | 3.13 |
| blink | 427 | 599.62 | [128, 960, 160, 992] | 32 | 15.98 | 0.88 |
| blink | 334 | 591.88 | [0, 960, 32, 992] | 32 | 10.71 | 0.60 |
| blink | 425 | 599.46 | [192, 1024, 224, 1056] | 32 | 8.92 | 0.51 |
| blink | 37 | 567.12 | [128, 992, 160, 1024] | 32 | 17.56 | 1.07 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_bow\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_bow\montage_02.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_bow\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_bow\clip.mp4`
## ark_cin_coming (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_coming`, 720 frames at 30 fps, kind dread

- luma      `▁▁▁▁▂▂▂▂▃▃▃▄▄▄▄▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▄▅▅▅▅▅▆▆▆▅▅▆▇███████`
- mean diff `▂▁▂▂▂▂▂▂▂▃▄▅▆▇▇████▇▇▅▃▂▁▁▁▁▁▁▁▁▂▃▄▅▆▇▇▇▇▇▇▇▇▆▆▅▄▃▂▂▂▂▂▂▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.3100 | <= 1.2 levels | at frame 586 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 2 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f337 s498.12 32px@[320, 1024] 2.5 (nb8 median 1.1); f662 s525.21 32px@[1184, 128] 2.3 (nb8 median 1.4) |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 54 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [149, 619, 621, 622, 623, 625, 626, 627, 628, 629, 630, 631]; worst f719 s529.96 32px@[288, 736] 13.9/2.25; f627 s522.29 32px@[864, 864] 26.2/4.44; f630 s522.54 32px@[768, 832] 17.3/3.01; f626 s522.21 32px@[896, 864] 25.2/4.57; f657 s524.79 32px@[96, 896] 7.2/1.34; 219 rain-streak and 216 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 599 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [8, 15, 21, 32, 47, 59, 60, 72, 74, 76, 78, 79]; worst f336 s498.04 32px@[1760, 256] 6.1/0.25; f690 s527.54 32px@[352, 1024] 26.3/1.34; f636 s523.04 32px@[160, 64] 9.4/0.49; f642 s523.54 32px@[128, 32] 6.0/0.32; f315 s496.29 32px@[1536, 672] 4.5/0.25; 219 rain-streak and 216 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 7.4500 | < 25 % of the screen | worst frame 204; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 54903 block transitions in all; 16 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 28.3200 | < 25 % of the screen (static camera) | worst frame 220; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.3400 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 2.3333 | <= 0.5 Hz (amplitude floor 0.002) | 8039 of 8040 blocks modulated, 462 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00733 (<= 0.002); df 0.0417 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.7987 | <= 0.3 % / frame | at frame 586; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.5985 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 15.8360 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 336 | 498.04 | [1760, 256, 1792, 288] | 32 | 6.14 | 0.25 |
| blink | 690 | 527.54 | [352, 1024, 384, 1056] | 32 | 26.27 | 1.34 |
| blink | 636 | 523.04 | [160, 64, 192, 96] | 32 | 9.41 | 0.49 |
| blink | 642 | 523.54 | [128, 32, 160, 64] | 32 | 5.96 | 0.32 |
| blink | 315 | 496.29 | [1536, 672, 1568, 704] | 32 | 4.55 | 0.25 |
| blink | 696 | 528.04 | [576, 448, 608, 480] | 32 | 4.80 | 0.28 |
| blink | 338 | 498.21 | [1792, 448, 1824, 480] | 32 | 4.32 | 0.25 |
| blink | 346 | 498.88 | [1504, 672, 1536, 704] | 32 | 4.16 | 0.25 |
| blink | 681 | 526.79 | [672, 864, 704, 896] | 32 | 25.80 | 1.57 |
| blink | 634 | 522.88 | [192, 64, 224, 96] | 32 | 5.00 | 0.31 |
| blink | 288 | 494.04 | [160, 992, 192, 1024] | 32 | 3.91 | 0.25 |
| blink | 368 | 500.71 | [1824, 928, 1856, 960] | 32 | 4.01 | 0.28 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_coming\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_coming\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_coming\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_coming\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_cin_coming\clip.mp4`
## ark_emerge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_emerge`, 502 frames at 30 fps, kind boundary

- luma      `█▇▇▅▄▄▂▂▂▂▂▃▃▂▂▂▂▂▂▁▁▁▂▂▃▃▃▂▂▂▂▂▂▂▂▂▃▃▃▄▄▄▃▃▄▃▃▄▃▄▄▄▄▅▅▅▄▄▄▄`
- mean diff `█▇▇▇▇▅▂▃▃▃▂▂▂▂▁▁▂▂▂▁▁▂▃▃▃▂▂▁▁▁▁▁▁▂▁▁▁▂▂▂▂▁▁▁▂▂▂▂▂▂▃▃▃▃▃▃▂▂▁▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0950 | <= 1.2 levels | at frame 32 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | SKIP | - |  | no boundary crossed or no per-frame s |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 5 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f380 sNone 32px@[1536, 608] 2.7 (nb8 median 1.3); f78 sNone 32px@[1568, 512] 2.6 (nb8 median 1.8); f473 sNone 32px@[1440, 512] 2.6 (nb8 median 1.4); f247 sNone 32px@[1408, 576] 2.5 (nb8 median 1.2) |
| start transient (frame 1, after the pose jump) | INFO | 1 |  | not gated (C20); worst blink 32px@[1536, 448] 3.7/0.41 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 3 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [260, 501]; worst f501 sNone 32px@[1792, 160] 2.7/0.32; f260 sNone 32px@[1856, 512] 2.8/0.65; f501 sNone 32px@[1696, 64] 5.1/1.64; 213 rain-streak and 83 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 355 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [3, 5, 6, 10, 11, 13, 14, 17, 19, 21, 22, 23]; worst f14 sNone 32px@[1568, 640] 6.1/0.27; f327 sNone 32px@[1600, 640] 6.2/0.30; f25 sNone 32px@[1696, 576] 6.1/0.31; f23 sNone 32px@[1536, 640] 5.9/0.33; f23 sNone 32px@[1568, 640] 6.6/0.37; 213 rain-streak and 83 motion-explained events excluded |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 14 | - | [1568, 640, 1600, 672] | 32 | 6.06 | 0.27 |
| blink | 327 | - | [1600, 640, 1632, 672] | 32 | 6.15 | 0.30 |
| blink | 25 | - | [1696, 576, 1728, 608] | 32 | 6.06 | 0.31 |
| blink | 23 | - | [1536, 640, 1568, 672] | 32 | 5.85 | 0.33 |
| blink | 23 | - | [1568, 640, 1600, 672] | 32 | 6.55 | 0.37 |
| blink | 105 | - | [1664, 640, 1696, 672] | 32 | 7.69 | 0.45 |
| blink | 124 | - | [1632, 640, 1664, 672] | 32 | 6.71 | 0.40 |
| blink | 34 | - | [1664, 640, 1696, 672] | 32 | 7.50 | 0.47 |
| blink | 62 | - | [1600, 640, 1632, 672] | 32 | 6.82 | 0.43 |
| blink | 72 | - | [1728, 640, 1760, 672] | 32 | 9.05 | 0.58 |
| blink | 11 | - | [1728, 640, 1760, 672] | 32 | 4.98 | 0.33 |
| blink | 26 | - | [1440, 576, 1472, 608] | 32 | 3.70 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_emerge\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_emerge\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_emerge\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_emerge\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\seq_ark_emerge\clip.mp4`