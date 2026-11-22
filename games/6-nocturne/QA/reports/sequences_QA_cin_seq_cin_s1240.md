# Frame sequences: QA_cin_seq_cin_s1240

Sequences: 1. Checks: 8 PASS, 1 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s1240 | cinematic | 720 | 0.0009 | 3.356 | 5.516 | 211.7 | 0 / 17 | FAIL | block blinks (out and back), 32/64 px blocks |

## cin_s1240 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1240`, 720 frames at 30 fps, kind cinematic

- luma      `█▇▇▆▆▅▄▂▁▂▂▂▃▃▃▃▃▃▃▃▄▄▄▄▅▅▅▅▅▅▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇███`
- mean diff `▆▇▇▇██████████▇▇▆▆▆▅▅▅▅▅▄▄▄▄▄▄▄▄▄▄▅▅▅▅▅▆▆▆▆▅▅▃▂▂▃▃▃▃▃▃▃▂▂▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2400 | <= 1.2 levels | at frame 80 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 32/64 px blocks | PASS | 0 | 0 (> 3x both neighbouring pairs and > 2 levels) | 720 frames; 23 rain-streak and 75 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 17 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [75, 288, 446, 459, 467, 475, 496, 514, 524, 533, 538, 539]; worst f475 s1279.62 32px@[1856, 736] 4.3/0.25; f459 s1278.29 32px@[1760, 736] 3.4/0.37; f538 s1284.88 32px@[640, 256] 3.0/0.41; f533 s1284.46 32px@[1536, 576] 3.2/0.51; f524 s1283.71 32px@[672, 288] 3.0/0.50; 23 rain-streak and 75 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 12.8000 | < 25 % of the screen | worst frame 99; 0 frames over; 23246 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 6.1400 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 4.2000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 2.2083 | <= 0.5 Hz (amplitude floor 0.002) | 6604 of 8040 blocks modulated, 122 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00517 (<= 0.002); df 0.0417 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.7277 | <= 0.3 % / frame | at frame 80; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.8207 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.609 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 22.1440 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 475 | 1279.62 | [1856, 736, 1888, 768] | 32 | 4.34 | 0.25 |
| blink | 459 | 1278.29 | [1760, 736, 1792, 768] | 32 | 3.42 | 0.37 |
| blink | 538 | 1284.88 | [640, 256, 672, 288] | 32 | 3.01 | 0.41 |
| blink | 533 | 1284.46 | [1536, 576, 1568, 608] | 32 | 3.18 | 0.51 |
| blink | 524 | 1283.71 | [672, 288, 704, 320] | 32 | 3.01 | 0.50 |
| blink | 467 | 1278.96 | [1856, 704, 1888, 736] | 32 | 4.25 | 0.86 |
| blink | 514 | 1282.88 | [704, 288, 736, 320] | 32 | 2.78 | 0.57 |
| blink | 288 | 1264.04 | [1504, 512, 1536, 544] | 32 | 2.56 | 0.52 |
| blink | 446 | 1277.21 | [1568, 576, 1600, 608] | 32 | 2.56 | 0.59 |
| blink | 566 | 1287.21 | [576, 224, 608, 256] | 32 | 3.46 | 0.83 |
| blink | 496 | 1281.38 | [768, 288, 800, 320] | 32 | 2.52 | 0.64 |
| blink | 75 | 1246.29 | [32, 512, 64, 544] | 32 | 5.33 | 1.38 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1240\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1240\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1240\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1240\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1240\clip.mp4`

Frames deleted after analysis: 720 (analysed.done in the folder)