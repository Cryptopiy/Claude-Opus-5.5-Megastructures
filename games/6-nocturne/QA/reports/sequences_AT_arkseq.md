# Frame sequences: AT_arkseq

Sequences: 1. Checks: 8 PASS, 1 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| dread_hull_pass | dread | 289 | 0.0002 | 0.563 | 0.690 | 218.9 | 133 / 0 | FAIL | block pops (one-frame jumps), 16/32 px blocks |

## dread_hull_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_arkseq\seq_dread_hull_pass`, 289 frames at 60 fps, kind dread

- luma      `██████▇▇▇▇▇▇▇▇▇▆▆▆▆▆▆▆▆▆▅▅▅▅▅▅▅▅▅▄▄▄▄▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▁▁▁`
- mean diff `▁▁▁▁▁▂▂▂▂▂▂▂▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄▄▄▅▅▅▅▅▅▅▅▅▅▅▅▅▆▅▅▆▆▇▆▆▆▇▆▇▇▇▇▇██`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0610 | <= 1.2 levels | at frame 268 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| start transient (frame 1, after the pose jump) | INFO | 2 |  | not gated (C20); worst pop 16px@[64, 240] 4.3/0.25; pop 16px@[0, 352] 6.2/0.38 |
| block pops (one-frame jumps), 16/32 px blocks | FAIL | 133 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [4, 10, 15, 25, 26, 29, 30, 40, 44, 45, 46, 48]; worst f215 s552.96 16px@[64, 496] 6.2/0.25; f108 s548.5 16px@[80, 448] 5.7/0.25; f10 s544.42 16px@[32, 416] 6.8/0.34; f119 s548.96 16px@[16, 352] 4.4/0.25; f246 s554.25 16px@[160, 480] 4.3/0.26; 14 rain-streak and 400 motion-explained events excluded |
| block blinks (out and back), 16/32 px blocks | PASS | 0 | 0 (> 3x both neighbouring pairs and > 2 levels) | 289 frames; 14 rain-streak and 400 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 0.2000 | < 25 % of the screen | worst frame 24; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 20 frames (1/3 s, WCAG 2.x general flash, D-041 1); 1442 block transitions in all; 8 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 1.1200 | < 25 % of the screen (static camera) | worst frame 62; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.0000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 9.9654 | <= 0.5 Hz (amplitude floor 0.002) | 3521 of 8040 blocks modulated, 142 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.01659 (<= 0.002); df 0.2076 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.1937 | <= 0.3 % / frame | at frame 268; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6492 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.050 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 8 px blocks |
| mean change per second (linear) | INFO | 12.2550 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| pop | 215 | 552.96 | [64, 496, 80, 512] | 16 | 6.23 | 0.25 |
| pop | 108 | 548.50 | [80, 448, 96, 464] | 16 | 5.71 | 0.25 |
| pop | 10 | 544.42 | [32, 416, 48, 432] | 16 | 6.81 | 0.34 |
| pop | 119 | 548.96 | [16, 352, 32, 368] | 16 | 4.39 | 0.25 |
| pop | 246 | 554.25 | [160, 480, 176, 496] | 16 | 4.25 | 0.26 |
| pop | 265 | 555.04 | [144, 480, 160, 496] | 16 | 3.98 | 0.25 |
| pop | 255 | 554.62 | [16, 464, 32, 480] | 16 | 3.95 | 0.25 |
| pop | 95 | 547.96 | [48, 416, 64, 432] | 16 | 3.92 | 0.25 |
| pop | 49 | 546.04 | [32, 464, 48, 480] | 16 | 3.88 | 0.25 |
| pop | 58 | 546.42 | [32, 496, 48, 512] | 16 | 3.68 | 0.25 |
| pop | 262 | 554.92 | [16, 416, 32, 432] | 16 | 3.31 | 0.25 |
| pop | 165 | 550.88 | [48, 496, 64, 512] | 16 | 3.60 | 0.27 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_arkseq\seq_dread_hull_pass\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_arkseq\seq_dread_hull_pass\montage_02.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_arkseq\seq_dread_hull_pass\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_arkseq\seq_dread_hull_pass\clip.mp4`