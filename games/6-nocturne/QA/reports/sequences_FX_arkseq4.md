# Frame sequences: FX_arkseq4

Sequences: 2. Checks: 13 PASS, 5 FAIL, 0 WARN, 2 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| ark_cin_belly | dread | 1272 | 0.0041 | 2.931 | 8.467 | 228.9 | 60 / 468 | FAIL | block pops (one-frame jumps), 11/21 px blocks, block blinks (out and back), 11/21 px blocks |
| ark_cin_bow | dread | 432 | 0.0067 | 4.324 | 7.946 | 230.1 | 49 / 631 | FAIL | luma step/frame, block pops (one-frame jumps), 11/21 px blocks, block blinks (out and back), 11/21 px blocks |

## ark_cin_belly (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_belly`, 1272 frames at 30 fps, kind dread

- luma      `▁▂▂▂▂▂▂▂▂▃▄▅▅▅▅▄▄▃▃▃▃▃▃▃▃▄▃▅▇▇▇▆▅▅▅▆▆▆▆▆▆▆▆▆▇███▆▆▅▅▄▄▃▂▂▂▂▂`
- mean diff `▄▄▄▄▃▂▂▃▄▅▅▆▆▅▄▂▁▂▂▂▂▂▂▂▂▂▂▄▅▅▅▅▄▄▃▂▂▁▁▁▁▁▁▁▂▂▂▂▂▄▅▆▇██▇▆▄▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 1.0550 | <= 1.2 levels | at frame 590 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 5 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f37 s743.12 11px@[33, 330] 18.1 (nb8 median 5.8); f37 s743.12 11px@[44, 341] 9.1 (nb8 median 7.9); f34 s742.88 11px@[77, 297] 8.6 (nb8 median 2.9); f71 s745.96 11px@[231, 341] 5.7 (nb8 median 2.1) |
| start transient (frame 1, after the pose jump) | INFO | 2 |  | not gated (C20); worst pop 11px@[66, 143] 2.7/0.56; pop 11px@[88, 0] 11.3/3.53 |
| block pops (one-frame jumps), 11/21 px blocks | FAIL | 60 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [89, 93, 263, 265, 276, 280, 283, 300, 343, 369, 375, 381]; worst f404 s773.71 11px@[77, 55] 3.9/0.34; f464 s778.71 11px@[495, 165] 2.2/0.25; f386 s772.21 11px@[121, 88] 2.1/0.25; f389 s772.46 11px@[440, 187] 2.4/0.29; f422 s775.21 11px@[22, 0] 3.3/0.43; 70 rain-streak and 2129 motion-explained events excluded |
| block blinks (out and back), 11/21 px blocks | FAIL | 468 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [4, 6, 7, 8, 9, 10, 12, 13, 14, 27, 28, 29]; worst f79 s746.62 11px@[154, 341] 10.3/0.39; f12 s741.04 11px@[33, 341] 15.7/0.61; f78 s746.54 11px@[165, 341] 10.7/0.43; f39 s743.29 11px@[0, 330] 21.4/0.90; f12 s741.04 11px@[44, 341] 11.7/0.52; 70 rain-streak and 2129 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 10.1900 | < 25 % of the screen | worst frame 1161; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 111961 block transitions in all; 5 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 31.6000 | < 25 % of the screen (static camera) | worst frame 1141; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 1.4900 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.5000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 1.6509 | <= 0.5 Hz (amplitude floor 0.002) | 9201 of 9216 blocks modulated, 307 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00766 (<= 0.002); df 0.0236 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 2.5659 | <= 0.3 % / frame | at frame 590; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.7439 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.694 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 5 px blocks |
| mean change per second (linear) | INFO | 51.6080 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 79 | 746.62 | [154, 341, 165, 352] | 11 | 10.32 | 0.39 |
| blink | 12 | 741.04 | [33, 341, 44, 352] | 11 | 15.68 | 0.61 |
| blink | 78 | 746.54 | [165, 341, 176, 352] | 11 | 10.75 | 0.43 |
| blink | 39 | 743.29 | [0, 330, 11, 341] | 11 | 21.43 | 0.90 |
| blink | 12 | 741.04 | [44, 341, 55, 352] | 11 | 11.65 | 0.52 |
| blink | 317 | 766.46 | [110, 209, 121, 220] | 11 | 6.39 | 0.31 |
| blink | 388 | 772.38 | [99, 220, 110, 231] | 11 | 4.99 | 0.25 |
| blink | 434 | 776.21 | [484, 308, 495, 319] | 11 | 4.83 | 0.25 |
| blink | 558 | 786.54 | [33, 286, 44, 297] | 11 | 4.48 | 0.25 |
| blink | 59 | 744.96 | [88, 341, 99, 352] | 11 | 14.87 | 0.85 |
| blink | 528 | 784.04 | [33, 264, 44, 275] | 11 | 4.36 | 0.25 |
| blink | 1075 | 829.62 | [77, 22, 88, 33] | 11 | 4.27 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_belly\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_belly\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_belly\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_belly\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_belly\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_belly\clip.mp4`
## ark_cin_bow (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_bow`, 432 frames at 30 fps, kind dread

- luma      `▁▂▃▃▃▂▂▁▁▁▁▁▂▂▃▃▃▄▄▄▄▅▅▅▅▅▅▅▆▆▆▇█████▇▆▄▄▄▄▄▄▄▄▄▄▄▄▃▃▃▂▁▁▁▁▁`
- mean diff `▄▄▄▄▄▃▂▂▁▂▅▆▇███▇▇▇▇▇▆▅▄▃▂▂▁▁▁▂▂▂▂▂▃▃▃▃▂▄▅▆▇▇████▇▇▇▆▅▄▂▂▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.7000 | <= 1.2 levels | at frame 275 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 5 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f93 s571.79 11px@[627, 319] 19.0 (nb8 median 6.7); f39 s567.29 11px@[11, 308] 4.9 (nb8 median 2.7); f13 s565.12 11px@[99, 154] 4.2 (nb8 median 2.0); f337 s592.12 11px@[33, 44] 4.1 (nb8 median 1.4) |
| start transient (frame 1, after the pose jump) | INFO | 2 |  | not gated (C20); worst pop 11px@[154, 165] 2.4/0.42; pop 11px@[132, 187] 2.1/0.37 |
| block pops (one-frame jumps), 11/21 px blocks | FAIL | 49 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [11, 27, 31, 33, 41, 98, 129, 191, 224, 255, 257, 258]; worst f431 s599.96 11px@[187, 297] 3.4/0.25; f431 s599.96 11px@[33, 242] 3.3/0.25; f431 s599.96 11px@[44, 198] 3.6/0.33; f431 s599.96 11px@[187, 154] 2.7/0.34; f277 s587.12 11px@[0, 22] 132.4/17.12; 65 rain-streak and 725 motion-explained events excluded |
| block blinks (out and back), 11/21 px blocks | FAIL | 631 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [6, 8, 9, 28, 30, 32, 33, 34, 35, 36, 37, 38]; worst f40 s567.38 11px@[0, 330] 16.0/0.25; f37 s567.12 11px@[44, 319] 12.9/0.34; f403 s597.62 11px@[44, 330] 11.5/0.55; f37 s567.12 11px@[44, 330] 11.6/0.56; f140 s575.71 11px@[242, 110] 4.7/0.25; 65 rain-streak and 725 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 16.4300 | < 25 % of the screen | worst frame 110; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 78333 block transitions in all; 5 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 42.1100 | < 25 % of the screen (static camera) | worst frame 134; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 3.2700 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.6000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 4.6528 | <= 0.5 Hz (amplitude floor 0.002) | 9175 of 9216 blocks modulated, 1080 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.01641 (<= 0.002); df 0.0694 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 4.0388 | <= 0.3 % / frame | at frame 275; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.7563 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.825 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 5 px blocks |
| mean change per second (linear) | INFO | 35.8190 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 40 | 567.38 | [0, 330, 11, 341] | 11 | 15.98 | 0.25 |
| blink | 37 | 567.12 | [44, 319, 55, 330] | 11 | 12.89 | 0.34 |
| blink | 403 | 597.62 | [44, 330, 55, 341] | 11 | 11.54 | 0.55 |
| blink | 37 | 567.12 | [44, 330, 55, 341] | 11 | 11.61 | 0.56 |
| blink | 140 | 575.71 | [242, 110, 253, 121] | 11 | 4.71 | 0.25 |
| blink | 38 | 567.21 | [550, 143, 561, 154] | 11 | 4.63 | 0.25 |
| blink | 338 | 592.21 | [88, 88, 99, 99] | 11 | 4.40 | 0.25 |
| blink | 327 | 591.29 | [220, 0, 231, 11] | 11 | 4.83 | 0.28 |
| blink | 336 | 592.04 | [99, 99, 110, 110] | 11 | 4.23 | 0.25 |
| blink | 324 | 591.04 | [198, 66, 209, 77] | 11 | 4.16 | 0.25 |
| blink | 145 | 576.12 | [297, 77, 308, 88] | 11 | 4.11 | 0.25 |
| blink | 322 | 590.88 | [231, 33, 242, 44] | 11 | 4.09 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_bow\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_bow\montage_02.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_bow\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq4\seq_ark_cin_bow\clip.mp4`