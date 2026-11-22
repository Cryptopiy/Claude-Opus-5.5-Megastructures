# Frame sequences: QA_cin_seq_cin_s0860

Sequences: 1. Checks: 7 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s0860 | cinematic | 480 | 0.0017 | 1.342 | 2.106 | 218.7 | 7 / 20 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## cin_s0860 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0860`, 480 frames at 30 fps, kind cinematic

- luma      `▅▆▇██▆▄▂▂▂▁▁▂▂▂▂▃▃▃▃▄▄▄▄▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▃▃▃▃`
- mean diff `▅▆▇███▃▂▂▁▁▁▁▁▂▂▃▃▃▄▄▄▃▄▄▄▄▄▄▄▄▄▄▄▄▄▄▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▃▄▄▅▅▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.4230 | <= 1.2 levels | at frame 47 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| start transient (frame 1, after the pose jump) | INFO | 2 |  | not gated (C20); worst pop 32px@[1632, 960] 8.2/2.28; pop 32px@[256, 960] 4.2/1.29 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 7 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [131, 137, 138, 159, 162, 166]; worst f162 s873.54 32px@[192, 192] 6.1/1.24; f159 s873.29 32px@[192, 480] 7.4/1.62; f166 s873.88 32px@[1792, 448] 14.2/3.65; f131 s870.96 32px@[1056, 64] 5.6/1.64; f137 s871.46 32px@[1088, 32] 12.4/3.80; 14 rain-streak and 37 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 20 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [10, 33, 34, 40, 41, 42, 43, 45, 46, 56, 57, 58]; worst f58 s864.88 32px@[1888, 928] 6.6/0.76; f58 s864.88 32px@[0, 960] 4.0/0.51; f57 s864.79 32px@[32, 960] 3.7/0.51; f56 s864.71 32px@[1824, 928] 7.8/1.10; f56 s864.71 32px@[64, 960] 4.3/0.69; 14 rain-streak and 37 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 5.1600 | < 25 % of the screen | worst frame 47; 0 frames over; 2809 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.0000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 0.8750 | <= 0.5 Hz (amplitude floor 0.002) | 3741 of 8040 blocks modulated, 15 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00666 (<= 0.002); df 0.0625 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 1.5751 | <= 0.3 % / frame | at frame 47; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6068 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 38.1030 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 58 | 864.88 | [1888, 928, 1920, 960] | 32 | 6.60 | 0.76 |
| blink | 58 | 864.88 | [0, 960, 32, 992] | 32 | 4.04 | 0.51 |
| blink | 57 | 864.79 | [32, 960, 64, 992] | 32 | 3.72 | 0.51 |
| blink | 56 | 864.71 | [1824, 928, 1856, 960] | 32 | 7.82 | 1.10 |
| blink | 56 | 864.71 | [64, 960, 96, 992] | 32 | 4.31 | 0.69 |
| blink | 58 | 864.88 | [1888, 960, 1920, 992] | 32 | 5.60 | 1.00 |
| blink | 46 | 863.88 | [1888, 128, 1920, 160] | 32 | 48.33 | 9.15 |
| blink | 46 | 863.88 | [1888, 160, 1920, 192] | 32 | 42.25 | 8.45 |
| pop | 162 | 873.54 | [192, 192, 224, 224] | 32 | 6.11 | 1.24 |
| pop | 159 | 873.29 | [192, 480, 224, 512] | 32 | 7.42 | 1.62 |
| blink | 34 | 862.88 | [0, 896, 32, 928] | 32 | 9.42 | 2.29 |
| blink | 45 | 863.79 | [1856, 128, 1888, 160] | 32 | 49.27 | 12.17 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0860\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0860\montage_02.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0860\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0860\clip.mp4`

Frames deleted after analysis: 480 (analysed.done in the folder)