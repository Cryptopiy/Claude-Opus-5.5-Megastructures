# Frame sequences: FX_arkseq2

Sequences: 1. Checks: 7 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| ark_cin_bow | dread | 432 | 0.0047 | 4.435 | 8.295 | 229.0 | 50 / 457 | FAIL | block pops (one-frame jumps), 16/32 px blocks, block blinks (out and back), 16/32 px blocks |

## ark_cin_bow (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq2\seq_ark_cin_bow`, 432 frames at 30 fps, kind dread

- luma      `▁▂▃▃▃▂▂▁▁▁▁▁▁▂▃▃▃▃▄▄▄▄▅▅▅▅▅▅▅▆▆▇█████▇▄▄▄▄▄▄▄▄▄▄▄▄▃▃▃▃▂▁▁▁▁▁`
- mean diff `▄▄▄▄▄▄▃▂▂▃▅▆███▇▇▇▇▇▆▅▄▃▂▂▁▁▁▂▂▂▂▂▂▃▃▃▁▂▃▄▅▆▇▇▇▇▇▇▇▇▆▅▄▃▂▂▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 1.1860 | <= 1.2 levels | at frame 271 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 8 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f333 s591.79 16px@[0, 512] 12.0 (nb8 median 4.1); f263 s585.96 16px@[0, 336] 11.6 (nb8 median 3.6); f39 s567.29 16px@[16, 496] 11.1 (nb8 median 5.8); f39 s567.29 16px@[16, 512] 10.1 (nb8 median 5.9) |
| start transient (frame 1, after the pose jump) | INFO | 3 |  | not gated (C20); worst pop 16px@[768, 160] 32.1/8.40; pop 16px@[624, 144] 13.9/3.73; pop 16px@[320, 176] 24.5/7.55 |
| block pops (one-frame jumps), 16/32 px blocks | FAIL | 50 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [5, 10, 13, 31, 32, 33, 35, 39, 64, 98, 103, 105]; worst f431 s599.96 16px@[288, 240] 17.1/1.12; f431 s599.96 16px@[48, 368] 3.1/0.35; f35 s566.96 16px@[0, 464] 3.9/0.44; f274 s586.88 16px@[0, 0] 142.0/18.01; f35 s566.96 16px@[0, 400] 3.1/0.43; 53 rain-streak and 375 motion-explained events excluded |
| block blinks (out and back), 16/32 px blocks | FAIL | 457 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [8, 13, 27, 29, 32, 33, 34, 35, 36, 37, 38, 39]; worst f427 s599.62 16px@[64, 496] 17.8/0.25; f425 s599.46 16px@[96, 496] 14.4/0.25; f40 s567.38 16px@[0, 496] 21.5/0.50; f422 s599.21 16px@[144, 512] 10.6/0.44; f406 s597.88 16px@[0, 512] 11.6/0.50; 53 rain-streak and 375 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 13.7600 | < 25 % of the screen | worst frame 105; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 56250 block transitions in all; 8 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 37.1500 | < 25 % of the screen (static camera) | worst frame 121; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 2.0500 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 5.1389 | <= 0.5 Hz (amplitude floor 0.002) | 8000 of 8040 blocks modulated, 727 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.01615 (<= 0.002); df 0.0694 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 2.8635 | <= 0.3 % / frame | at frame 271; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6809 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.137 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 8 px blocks |
| mean change per second (linear) | INFO | 37.4420 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 427 | 599.62 | [64, 496, 80, 512] | 16 | 17.76 | 0.25 |
| blink | 425 | 599.46 | [96, 496, 112, 512] | 16 | 14.43 | 0.25 |
| blink | 40 | 567.38 | [0, 496, 16, 512] | 16 | 21.48 | 0.50 |
| blink | 422 | 599.21 | [144, 512, 160, 528] | 16 | 10.56 | 0.44 |
| blink | 406 | 597.88 | [0, 512, 16, 528] | 16 | 11.64 | 0.50 |
| blink | 265 | 586.12 | [0, 272, 16, 288] | 16 | 49.24 | 2.58 |
| blink | 265 | 586.12 | [16, 256, 32, 272] | 16 | 63.45 | 3.39 |
| blink | 37 | 567.12 | [64, 496, 80, 512] | 16 | 14.49 | 0.78 |
| blink | 261 | 585.79 | [96, 272, 112, 288] | 16 | 62.21 | 3.38 |
| blink | 263 | 585.96 | [64, 256, 80, 272] | 16 | 58.34 | 3.60 |
| blink | 265 | 586.12 | [32, 240, 48, 256] | 16 | 57.74 | 3.67 |
| blink | 8 | 564.71 | [256, 32, 272, 48] | 16 | 5.34 | 0.34 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq2\seq_ark_cin_bow\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq2\seq_ark_cin_bow\montage_02.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq2\seq_ark_cin_bow\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq2\seq_ark_cin_bow\clip.mp4`