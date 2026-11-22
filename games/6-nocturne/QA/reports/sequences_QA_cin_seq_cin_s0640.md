# Frame sequences: QA_cin_seq_cin_s0640

Sequences: 1. Checks: 6 PASS, 4 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s0640 | cinematic | 1080 | 0.0032 | 3.819 | 8.803 | 234.1 | 5939 / 5917 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks, G2 WCAG general (> 6 transitions / s in a 1/3 window), G6 mean-luma step |

## cin_s0640 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0640`, 1080 frames at 30 fps, kind cinematic

- luma      `▇▇▇▇▇▆▆▆▆▇▇▇▇▇▇▇▆▆▆▇███▇▇▇▇▇▇▇▇▆▆▅▄▅▆▆▆▇▇▇██▇▆▆▆▇▇▇██▇▇▆▅▄▁▂`
- mean diff `▂▂▂▂▂▂▂▁▂▄▄▄▄▄▄▄▃▃▃▄▄▄▅▅▄▄▃▂▃▄▅▅▆▆▆▅▅▅▄▄▃▄▆██▆▅▄▄▃▃▂▂▁▁▂▂▂▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.8210 | <= 1.2 levels | at frame 799 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 5939 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 3, 4, 5, 7, 12, 21, 23, 24, 25, 44, 66]; worst f718 s699.88 32px@[1760, 224] 39.4/0.36; f232 s659.38 32px@[1440, 384] 24.5/0.25; f715 s699.62 32px@[1440, 352] 21.8/0.25; f165 s653.79 32px@[736, 256] 23.8/0.27; f291 s664.29 32px@[1056, 320] 21.9/0.27; 987 rain-streak events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 5917 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]; worst f710 s699.21 32px@[1856, 128] 101.0/0.65; f680 s696.71 32px@[1760, 1024] 37.8/0.25; f414 s674.54 32px@[0, 960] 36.7/0.25; f413 s674.46 32px@[64, 928] 36.7/0.25; f405 s673.79 32px@[160, 992] 36.1/0.25; 987 rain-streak events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 16.1800 | < 25 % of the screen | worst frame 810; 0 frames over; 65557 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | FAIL | 25.3400 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 2.0833 | <= 0.5 Hz (amplitude floor 0.002) | 7957 of 8040 blocks modulated, 134 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00467 (<= 0.002); df 0.0278 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | FAIL | 3.8102 | <= 0.3 % / frame | at frame 1038 |
| G7 red share | PASS | 0.6214 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.112 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 59.5580 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 710 | 699.21 | [1856, 128, 1888, 160] | 32 | 100.96 | 0.65 |
| blink | 680 | 696.71 | [1760, 1024, 1792, 1056] | 32 | 37.85 | 0.25 |
| blink | 414 | 674.54 | [0, 960, 32, 992] | 32 | 36.74 | 0.25 |
| blink | 413 | 674.46 | [64, 928, 96, 960] | 32 | 36.69 | 0.25 |
| blink | 405 | 673.79 | [160, 992, 192, 1024] | 32 | 36.10 | 0.25 |
| blink | 735 | 701.29 | [1856, 992, 1888, 1024] | 32 | 35.51 | 0.25 |
| blink | 414 | 674.54 | [0, 928, 32, 960] | 32 | 34.70 | 0.25 |
| blink | 448 | 677.38 | [32, 960, 64, 992] | 32 | 33.32 | 0.25 |
| blink | 1020 | 725.04 | [704, 928, 736, 960] | 32 | 33.08 | 0.25 |
| blink | 447 | 677.29 | [96, 960, 128, 992] | 32 | 32.76 | 0.25 |
| blink | 734 | 701.21 | [1792, 1024, 1824, 1056] | 32 | 32.63 | 0.25 |
| blink | 705 | 698.79 | [1632, 960, 1664, 992] | 32 | 32.29 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0640\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0640\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0640\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0640\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0640\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0640\clip.mp4`

Frames deleted after analysis: 1080 (analysed.done in the folder)