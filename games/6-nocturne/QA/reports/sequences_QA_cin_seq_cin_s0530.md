# Frame sequences: QA_cin_seq_cin_s0530

Sequences: 1. Checks: 7 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s0530 | cinematic | 720 | 0.0034 | 3.530 | 6.526 | 234.9 | 171 / 870 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## cin_s0530 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0530`, 720 frames at 30 fps, kind cinematic

- luma      `█▇▆▆▆▇▇▇▇▇▇▇▇▇▆▆▅▅▅▆▆▇▆▆▆▆▅▃▂▂▁▁▁▁▁▂▂▃▃▄▄▅▅▅▅▅▅▄▅▅▅▅▅▄▄▄▄▄▄▄`
- mean diff `▃▄▆▇█████▆▄▃▂▂▂▂▁▁▁▂▃▃▃▃▃▂▂▂▁▁▁▁▁▁▂▂▃▄▄▅▅▆▆▆▆▇▇████▇▇▅▄▂▁▃▄▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.8730 | <= 1.2 levels | at frame 11 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 171 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 11, 132, 137, 139, 146, 150, 153, 159, 162, 167, 168]; worst f1 s530.12 32px@[1088, 736] 10.1/0.66; f1 s530.12 32px@[320, 896] 69.8/4.59; f185 s545.46 32px@[640, 576] 17.0/1.38; f185 s545.46 32px@[672, 544] 21.8/1.84; f185 s545.46 32px@[608, 640] 15.9/1.50; 108 rain-streak and 991 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 870 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]; worst f521 s573.46 32px@[32, 992] 38.6/0.25; f519 s573.29 32px@[160, 928] 36.1/0.25; f556 s576.38 32px@[192, 992] 27.9/0.26; f555 s576.29 32px@[224, 832] 26.3/0.25; f554 s576.21 32px@[288, 960] 25.4/0.25; 108 rain-streak and 991 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 21.1300 | < 25 % of the screen | worst frame 92; 0 frames over; 38041 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.6800 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 0.8333 | <= 0.5 Hz (amplitude floor 0.002) | 7561 of 8040 blocks modulated, 67 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00713 (<= 0.002); df 0.0417 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 2.2224 | <= 0.3 % / frame | at frame 11; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6002 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.012 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 45.4600 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 521 | 573.46 | [32, 992, 64, 1024] | 32 | 38.57 | 0.25 |
| blink | 519 | 573.29 | [160, 928, 192, 960] | 32 | 36.09 | 0.25 |
| blink | 556 | 576.38 | [192, 992, 224, 1024] | 32 | 27.89 | 0.26 |
| blink | 555 | 576.29 | [224, 832, 256, 864] | 32 | 26.27 | 0.25 |
| blink | 554 | 576.21 | [288, 960, 320, 992] | 32 | 25.43 | 0.25 |
| blink | 535 | 574.62 | [288, 800, 320, 832] | 32 | 24.95 | 0.25 |
| blink | 553 | 576.12 | [320, 832, 352, 864] | 32 | 24.75 | 0.25 |
| blink | 553 | 576.12 | [320, 800, 352, 832] | 32 | 23.34 | 0.25 |
| blink | 558 | 576.54 | [64, 960, 96, 992] | 32 | 26.37 | 0.28 |
| blink | 559 | 576.62 | [0, 992, 32, 1024] | 32 | 21.78 | 0.25 |
| blink | 533 | 574.46 | [384, 768, 416, 800] | 32 | 22.30 | 0.28 |
| blink | 519 | 573.29 | [128, 800, 160, 832] | 32 | 35.18 | 0.47 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0530\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0530\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0530\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0530\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0530\clip.mp4`

Frames deleted after analysis: 720 (analysed.done in the folder)