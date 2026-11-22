# Frame sequences: QA_cin_seq_cin_s0096

Sequences: 1. Checks: 6 PASS, 4 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s0096 | cinematic | 408 | 0.0059 | 3.474 | 7.229 | 228.4 | 418 / 199 | FAIL | luma step/frame, block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks, G6 mean-luma step |

## cin_s0096 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0096`, 408 frames at 30 fps, kind cinematic

- luma      `▄▄▃▃▃▃▃▃▃▃▃▃▄▅▆▇▇████▆▃▃▂▂▂▂▂▁▁▁▁▁▁▂▂▃▃▃▃▃▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄▄▄▄`
- mean diff `▄▄▄▄▄▄▄▄▄▄▅▅▅▆▆▇██▇▇▇▆▅▄▃▃▃▃▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▃▄▅▆▇▇████▇▇▆▅▄▃▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.5060 | <= 1.2 levels | at frame 143 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 418 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 2, 3, 4, 7, 8, 10, 14, 15, 16, 21, 33]; worst f265 s118.12 32px@[0, 32] 13.3/0.91; f260 s117.71 32px@[32, 608] 27.5/2.10; f264 s118.04 32px@[32, 64] 12.3/0.95; f1 s96.12 32px@[1632, 96] 4.3/0.41; f261 s117.79 32px@[1824, 320] 20.7/2.25; 131 rain-streak events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 199 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [9, 39, 40, 42, 43, 44, 45, 46, 47, 48, 49, 50]; worst f81 s102.79 32px@[1568, 160] 106.6/5.16; f83 s102.96 32px@[1600, 96] 88.9/4.76; f85 s103.12 32px@[1600, 0] 88.1/5.16; f80 s102.71 32px@[1568, 192] 76.9/5.31; f82 s102.88 32px@[1600, 128] 67.4/4.69; 131 rain-streak events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 22.3000 | < 25 % of the screen | worst frame 143; 0 frames over; 27867 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 1.9300 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 1.8382 | <= 0.5 Hz (amplitude floor 0.002) | 8032 of 8040 blocks modulated, 296 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.01044 (<= 0.002); df 0.0735 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | FAIL | 4.1807 | <= 0.3 % / frame | at frame 144 |
| G7 red share | PASS | 0.6919 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.025 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 115.1470 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 81 | 102.79 | [1568, 160, 1600, 192] | 32 | 106.56 | 5.16 |
| blink | 83 | 102.96 | [1600, 96, 1632, 128] | 32 | 88.92 | 4.76 |
| blink | 85 | 103.12 | [1600, 0, 1632, 32] | 32 | 88.09 | 5.16 |
| pop | 265 | 118.12 | [0, 32, 32, 64] | 32 | 13.32 | 0.91 |
| blink | 80 | 102.71 | [1568, 192, 1600, 224] | 32 | 76.87 | 5.31 |
| blink | 82 | 102.88 | [1600, 128, 1632, 160] | 32 | 67.38 | 4.69 |
| blink | 284 | 119.71 | [1632, 256, 1664, 288] | 32 | 4.27 | 0.33 |
| pop | 260 | 117.71 | [32, 608, 64, 640] | 32 | 27.47 | 2.10 |
| pop | 264 | 118.04 | [32, 64, 64, 96] | 32 | 12.31 | 0.95 |
| blink | 54 | 100.54 | [192, 128, 224, 160] | 32 | 3.13 | 0.25 |
| blink | 54 | 100.54 | [224, 128, 256, 160] | 32 | 3.14 | 0.25 |
| blink | 84 | 103.04 | [1632, 64, 1664, 96] | 32 | 59.18 | 4.82 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0096\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0096\montage_02.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0096\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0096\clip.mp4`

Frames deleted after analysis: 408 (analysed.done in the folder)