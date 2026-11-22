# Frame sequences: INTB_dread2_seq_dread_lightning_s0553

Sequences: 1. Checks: 7 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| dread_lightning_s0553 | dread | 180 | 0.0015 | 3.570 | 5.211 | 230.6 | 32 / 105 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## dread_lightning_s0553 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_dread2\seq_dread_lightning_s0553`, 180 frames at 30 fps, kind dread

- luma      `███████████████████▇▇▇▇▇▇▇▇▇▆▆▆▆▆▅▅▅▄▄▃▂▂▁▁▁▁▁▁▁▂▂▂▂▂▃▃▃▃▄▄▄`
- mean diff `▁▁▁▁▁▁▂▂▂▂▃▃▃▄▄▄▄▅▅▅▆▆▆▇▇▇▇▇▇▇▇▇▇▇▇▆▆▆▆▅▄▄▄▃▄▄▄▅▅▅▆▆▆▆▆▇▇▇██`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.3940 | <= 1.2 levels | at frame 117 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| start transient (frame 1, after the pose jump) | INFO | 1 |  | not gated (C20); worst blink 32px@[1888, 736] 8.2/2.49 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 32 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [68, 69, 70, 83, 108, 109, 111, 112, 113, 114, 115, 116]; worst f179 s562.96 32px@[1888, 256] 37.8/2.38; f109 s557.12 32px@[576, 800] 25.9/3.24; f116 s557.71 32px@[416, 768] 21.9/3.10; f122 s558.21 32px@[128, 192] 16.4/2.41; f114 s557.54 32px@[352, 896] 21.5/3.88; 9 rain-streak and 359 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 105 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [2, 3, 4, 5, 6, 7, 8, 9, 10, 14, 15, 16]; worst f88 s555.38 32px@[32, 672] 24.5/2.75; f125 s558.46 32px@[0, 896] 26.2/3.02; f7 s548.62 32px@[672, 224] 2.5/0.35; f119 s557.96 32px@[32, 960] 22.8/3.46; f26 s550.21 32px@[1152, 672] 22.4/3.43; 9 rain-streak and 359 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 4.4900 | < 25 % of the screen | worst frame 108; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 10111 block transitions in all; 16 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 15.3400 | < 25 % of the screen (static camera) | worst frame 108; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 3.0700 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 4.8333 | <= 0.5 Hz (amplitude floor 0.002) | 7110 of 8040 blocks modulated, 853 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.02308 (<= 0.002); df 0.1667 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 1.4407 | <= 0.3 % / frame | at frame 117; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.5937 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 45.7990 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| pop | 179 | 562.96 | [1888, 256, 1920, 288] | 32 | 37.79 | 2.38 |
| blink | 88 | 555.38 | [32, 672, 64, 704] | 32 | 24.52 | 2.75 |
| blink | 125 | 558.46 | [0, 896, 32, 928] | 32 | 26.22 | 3.02 |
| pop | 109 | 557.12 | [576, 800, 608, 832] | 32 | 25.90 | 3.24 |
| blink | 7 | 548.62 | [672, 224, 704, 256] | 32 | 2.54 | 0.35 |
| pop | 116 | 557.71 | [416, 768, 448, 800] | 32 | 21.95 | 3.10 |
| pop | 122 | 558.21 | [128, 192, 160, 224] | 32 | 16.35 | 2.41 |
| blink | 119 | 557.96 | [32, 960, 64, 992] | 32 | 22.77 | 3.46 |
| blink | 26 | 550.21 | [1152, 672, 1184, 704] | 32 | 22.40 | 3.43 |
| blink | 116 | 557.71 | [96, 512, 128, 544] | 32 | 31.70 | 5.11 |
| blink | 27 | 550.29 | [1120, 640, 1152, 672] | 32 | 22.85 | 3.73 |
| blink | 30 | 550.54 | [1056, 320, 1088, 352] | 32 | 16.87 | 2.82 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_dread2\seq_dread_lightning_s0553\montage_01.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_dread2\seq_dread_lightning_s0553\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_dread2\seq_dread_lightning_s0553\clip.mp4`

Frames deleted after analysis: 180 (analysed.done in the folder)