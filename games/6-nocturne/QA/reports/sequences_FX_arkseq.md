# Frame sequences: FX_arkseq

Sequences: 1. Checks: 6 PASS, 3 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| ark_cin_bow | dread | 432 | 0.0049 | 4.557 | 8.295 | 229.0 | 65 / 467 | FAIL | luma step/frame, block pops (one-frame jumps), 16/32 px blocks, block blinks (out and back), 16/32 px blocks |

## ark_cin_bow (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq\seq_ark_cin_bow`, 432 frames at 30 fps, kind dread

- luma      `▁▂▃▃▃▂▂▁▁▁▁▁▁▂▃▃▃▃▄▄▄▄▅▅▅▅▅▅▆▆▆▇████▇▆▄▄▄▄▄▃▃▃▃▃▂▂▃▃▃▃▂▁▁▁▁▁`
- mean diff `▄▅▅▅▄▄▃▂▂▃▅▆████▇▇▇▇▆▅▄▃▃▂▂▂▂▂▂▂▂▃▄▆▇▆▆▆▆▆▆▇▇▇▇▇▆▆▅▄▃▃▂▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.2620 | <= 1.2 levels | at frame 266 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 8 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f323 s590.96 16px@[0, 368] 11.6 (nb8 median 5.7); f39 s567.29 16px@[16, 496] 11.1 (nb8 median 5.8); f39 s567.29 16px@[16, 512] 10.1 (nb8 median 5.9); f320 s590.71 16px@[0, 496] 10.0 (nb8 median 4.1) |
| start transient (frame 1, after the pose jump) | INFO | 3 |  | not gated (C20); worst pop 16px@[768, 160] 32.1/8.40; pop 16px@[624, 144] 13.9/3.73; pop 16px@[320, 176] 24.5/7.55 |
| block pops (one-frame jumps), 16/32 px blocks | FAIL | 65 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [5, 10, 13, 31, 32, 33, 35, 39, 64, 98, 103, 105]; worst f431 s599.96 16px@[288, 240] 19.1/1.30; f269 s586.46 16px@[64, 0] 143.5/15.31; f431 s599.96 16px@[768, 416] 2.3/0.25; f35 s566.96 16px@[0, 464] 3.9/0.44; f262 s585.88 16px@[208, 80] 108.2/14.39; 81 rain-streak and 369 motion-explained events excluded |
| block blinks (out and back), 16/32 px blocks | FAIL | 467 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [8, 13, 27, 29, 32, 33, 34, 35, 36, 37, 38, 39]; worst f422 s599.21 16px@[144, 512] 11.3/0.25; f40 s567.38 16px@[0, 496] 21.5/0.50; f263 s585.96 16px@[32, 240] 64.1/1.69; f262 s585.88 16px@[48, 256] 51.8/1.39; f264 s586.04 16px@[0, 240] 55.4/1.59; 81 rain-streak and 369 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 13.7600 | < 25 % of the screen | worst frame 105; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 56869 block transitions in all; 8 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 37.1500 | < 25 % of the screen (static camera) | worst frame 121; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 1.5900 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 4.2361 | <= 0.5 Hz (amplitude floor 0.002) | 7998 of 8040 blocks modulated, 1033 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.01648 (<= 0.002); df 0.0694 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 3.0306 | <= 0.3 % / frame | at frame 267; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6764 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.137 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 8 px blocks |
| mean change per second (linear) | INFO | 38.1190 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 422 | 599.21 | [144, 512, 160, 528] | 16 | 11.29 | 0.25 |
| blink | 40 | 567.38 | [0, 496, 16, 512] | 16 | 21.48 | 0.50 |
| blink | 263 | 585.96 | [32, 240, 48, 256] | 16 | 64.05 | 1.69 |
| blink | 262 | 585.88 | [48, 256, 64, 272] | 16 | 51.78 | 1.39 |
| blink | 264 | 586.04 | [0, 240, 16, 256] | 16 | 55.37 | 1.59 |
| blink | 263 | 585.96 | [16, 256, 32, 272] | 16 | 50.97 | 1.52 |
| blink | 263 | 585.96 | [0, 272, 16, 288] | 16 | 41.68 | 1.27 |
| blink | 259 | 585.62 | [112, 272, 128, 288] | 16 | 57.14 | 1.76 |
| blink | 264 | 586.04 | [16, 224, 32, 240] | 16 | 69.57 | 2.22 |
| blink | 263 | 585.96 | [48, 224, 64, 240] | 16 | 58.81 | 2.14 |
| blink | 376 | 595.38 | [128, 480, 144, 496] | 16 | 14.32 | 0.60 |
| blink | 262 | 585.88 | [64, 240, 80, 256] | 16 | 56.92 | 2.54 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq\seq_ark_cin_bow\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq\seq_ark_cin_bow\montage_02.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq\seq_ark_cin_bow\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq\seq_ark_cin_bow\clip.mp4`