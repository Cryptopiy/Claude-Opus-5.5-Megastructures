# Frame sequences: QA_cin_seq_cin_s0260

Sequences: 1. Checks: 6 PASS, 3 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s0260 | cinematic | 960 | 0.0006 | 3.207 | 9.088 | 231.6 | 9 / 224 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks, G1 flashes (area with a rise and a fall in 1 s) |

## cin_s0260 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0260`, 960 frames at 30 fps, kind cinematic

- luma      `▁▁▁▁▁▁▁▂▂▂▂▂▂▃▃▃▃▃▃▃▃▃▃▃▃▃▄▄▄▅▆▆▆▇▇▇▇████████████████████▇▇▇`
- mean diff `▂▁▁▁▁▁▁▁▂▃▄▆▇▇███▇▆▄▂▁▁▂▂▂▃▃▄▄▄▄▄▃▃▂▁▁▁▁▂▂▁▁▁▁▁▁▁▁▁▁▁▁▂▃▄▅▅▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1420 | <= 1.2 levels | at frame 222 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 9 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [120, 167, 196, 197, 354, 594, 959]; worst f959 s339.96 32px@[1440, 992] 9.5/1.40; f120 s270.04 32px@[1376, 736] 3.7/0.56; f959 s339.96 32px@[672, 160] 2.1/0.32; f959 s339.96 32px@[480, 64] 6.7/1.19; f197 s276.46 32px@[0, 768] 85.3/18.62; 66 rain-streak and 711 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 224 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 16, 23, 28, 29, 31, 41, 44, 56, 67, 68, 70]; worst f536 s304.71 32px@[32, 480] 7.1/0.25; f557 s306.46 32px@[384, 608] 4.9/0.25; f778 s324.88 32px@[544, 1024] 4.6/0.25; f649 s314.12 32px@[96, 416] 4.4/0.25; f121 s270.12 32px@[1888, 128] 6.7/0.40; 66 rain-streak and 711 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | FAIL | 28.4600 | < 25 % of the screen | worst frame 259; 45 frames over; 61140 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 1.3600 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 1.2188 | <= 0.5 Hz (amplitude floor 0.002) | 8027 of 8040 blocks modulated, 156 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00938 (<= 0.002); df 0.0312 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.3868 | <= 0.3 % / frame | at frame 222; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6710 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.037 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 8.6580 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 536 | 304.71 | [32, 480, 64, 512] | 32 | 7.12 | 0.25 |
| blink | 557 | 306.46 | [384, 608, 416, 640] | 32 | 4.94 | 0.25 |
| blink | 778 | 324.88 | [544, 1024, 576, 1056] | 32 | 4.60 | 0.25 |
| blink | 649 | 314.12 | [96, 416, 128, 448] | 32 | 4.39 | 0.25 |
| blink | 121 | 270.12 | [1888, 128, 1920, 160] | 32 | 6.72 | 0.40 |
| blink | 718 | 319.88 | [416, 992, 448, 1024] | 32 | 3.97 | 0.25 |
| blink | 675 | 316.29 | [256, 896, 288, 928] | 32 | 3.82 | 0.25 |
| blink | 794 | 326.21 | [448, 1024, 480, 1056] | 32 | 3.72 | 0.25 |
| blink | 799 | 326.62 | [1856, 64, 1888, 96] | 32 | 3.64 | 0.25 |
| blink | 96 | 268.04 | [0, 0, 32, 32] | 32 | 3.84 | 0.27 |
| blink | 642 | 313.54 | [480, 1024, 512, 1056] | 32 | 3.56 | 0.25 |
| blink | 23 | 261.96 | [160, 32, 192, 64] | 32 | 3.54 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0260\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0260\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0260\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0260\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0260\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0260\clip.mp4`

Frames deleted after analysis: 960 (analysed.done in the folder)