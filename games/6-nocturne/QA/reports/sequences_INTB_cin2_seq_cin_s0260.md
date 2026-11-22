# Frame sequences: INTB_cin2_seq_cin_s0260

Sequences: 1. Checks: 7 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s0260 | cinematic | 960 | 0.0009 | 3.549 | 11.538 | 230.1 | 6 / 232 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## cin_s0260 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin2\seq_cin_s0260`, 960 frames at 30 fps, kind cinematic

- luma      `▁▁▁▁▁▁▁▂▂▂▂▂▃▄▄▅▆▇█████████████▇▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▅`
- mean diff `▂▁▁▁▁▁▁▁▂▃▅▆▇██▇▇▆▅▃▂▁▁▁▁▁▁▁▂▄▅▅▅▄▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▃▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2280 | <= 1.2 levels | at frame 219 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 1 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f859 s331.62 32px@[416, 1024] 3.2 (nb8 median 1.7) |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 6 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [120, 196, 214, 959]; worst f959 s339.96 32px@[1440, 992] 9.5/1.40; f959 s339.96 32px@[672, 160] 2.1/0.32; f120 s270.04 32px@[1376, 736] 3.5/0.56; f959 s339.96 32px@[480, 64] 6.7/1.19; f214 s277.88 32px@[96, 928] 99.9/28.74; 46 rain-streak and 629 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 232 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [16, 23, 28, 29, 31, 41, 42, 44, 56, 67, 68, 70]; worst f438 s296.54 32px@[0, 352] 8.1/0.25; f400 s293.38 32px@[32, 416] 5.1/0.25; f372 s291.04 32px@[32, 352] 5.0/0.25; f592 s309.38 32px@[512, 1024] 4.7/0.25; f121 s270.12 32px@[1888, 128] 6.8/0.37; 46 rain-streak and 629 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 13.2600 | < 25 % of the screen | worst frame 240; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 78963 block transitions in all; 16 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 40.6300 | < 25 % of the screen (static camera) | worst frame 246; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 2.7300 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 1.4375 | <= 0.5 Hz (amplitude floor 0.002) | 8017 of 8040 blocks modulated, 171 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00972 (<= 0.002); df 0.0312 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.5824 | <= 0.3 % / frame | at frame 219; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6777 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.050 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 21.9410 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 438 | 296.54 | [0, 352, 32, 384] | 32 | 8.13 | 0.25 |
| blink | 400 | 293.38 | [32, 416, 64, 448] | 32 | 5.10 | 0.25 |
| blink | 372 | 291.04 | [32, 352, 64, 384] | 32 | 4.97 | 0.25 |
| blink | 592 | 309.38 | [512, 1024, 544, 1056] | 32 | 4.74 | 0.25 |
| blink | 121 | 270.12 | [1888, 128, 1920, 160] | 32 | 6.76 | 0.37 |
| blink | 778 | 324.88 | [544, 1024, 576, 1056] | 32 | 4.60 | 0.25 |
| blink | 649 | 314.12 | [96, 416, 128, 448] | 32 | 4.39 | 0.25 |
| blink | 421 | 295.12 | [0, 288, 32, 320] | 32 | 4.34 | 0.25 |
| blink | 422 | 295.21 | [0, 320, 32, 352] | 32 | 4.05 | 0.25 |
| blink | 410 | 294.21 | [96, 288, 128, 320] | 32 | 4.01 | 0.25 |
| blink | 447 | 297.29 | [32, 352, 64, 384] | 32 | 5.35 | 0.34 |
| blink | 718 | 319.88 | [416, 992, 448, 1024] | 32 | 3.97 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin2\seq_cin_s0260\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin2\seq_cin_s0260\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin2\seq_cin_s0260\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin2\seq_cin_s0260\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin2\seq_cin_s0260\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin2\seq_cin_s0260\clip.mp4`

Frames deleted after analysis: 960 (analysed.done in the folder)