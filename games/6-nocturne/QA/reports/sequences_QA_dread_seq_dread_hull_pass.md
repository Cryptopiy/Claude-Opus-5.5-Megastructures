# Frame sequences: QA_dread_seq_dread_hull_pass

Sequences: 1. Checks: 8 PASS, 1 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| dread_hull_pass | dread | 289 | 0.0002 | 0.706 | 0.957 | 229.8 | 19 / 0 | FAIL | block pops (one-frame jumps), 32/64 px blocks |

## dread_hull_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dread\seq_dread_hull_pass`, 289 frames at 60 fps, kind dread

- luma      `████▇▇▇▇▇▇▇▇▇▆▆▆▆▆▆▆▆▆▅▅▅▅▅▅▅▅▄▄▄▄▄▄▄▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▁▁▁▁`
- mean diff `▁▁▁▁▁▁▂▁▁▁▁▂▂▂▂▂▂▂▃▃▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▅▅▅▅▅▅▆▆▆▆▆▇▇▇▇▇███`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0520 | <= 1.2 levels | at frame 31 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| start transient (frame 1, after the pose jump) | INFO | 2 |  | not gated (C20); worst pop 32px@[288, 384] 2.4/0.52; pop 32px@[256, 384] 4.0/1.26 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 19 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [4, 28, 79, 119, 120, 123, 156, 168, 189, 193, 194, 229]; worst f4 s544.17 32px@[32, 672] 2.3/0.39; f4 s544.17 32px@[0, 672] 7.3/1.34; f251 s554.46 32px@[1696, 928] 6.1/1.15; f193 s552.04 32px@[0, 736] 7.1/1.46; f79 s547.29 32px@[0, 672] 8.9/1.85; 0 rain-streak and 44 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | PASS | 0 | 0 (> 3x both neighbouring pairs and > 2 levels) | 289 frames; 0 rain-streak and 44 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 1.2200 | < 25 % of the screen | worst frame 248; 0 frames over; 1443 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.0000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 16.4014 | <= 0.5 Hz (amplitude floor 0.002) | 3485 of 8040 blocks modulated, 272 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.01258 (<= 0.002); df 0.2076 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.1694 | <= 0.3 % / frame | at frame 284; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6212 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 8.9790 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| pop | 4 | 544.17 | [32, 672, 64, 704] | 32 | 2.26 | 0.39 |
| pop | 4 | 544.17 | [0, 672, 32, 704] | 32 | 7.27 | 1.34 |
| pop | 251 | 554.46 | [1696, 928, 1728, 960] | 32 | 6.13 | 1.15 |
| pop | 193 | 552.04 | [0, 736, 32, 768] | 32 | 7.09 | 1.46 |
| pop | 79 | 547.29 | [0, 672, 32, 704] | 32 | 8.89 | 1.85 |
| pop | 4 | 544.17 | [0, 640, 64, 704] | 64 | 2.54 | 0.55 |
| pop | 229 | 553.54 | [0, 704, 32, 736] | 32 | 5.08 | 1.17 |
| pop | 257 | 554.71 | [224, 384, 256, 416] | 32 | 2.78 | 0.66 |
| pop | 272 | 555.33 | [224, 352, 256, 384] | 32 | 2.59 | 0.63 |
| pop | 79 | 547.29 | [0, 640, 64, 704] | 64 | 2.35 | 0.58 |
| pop | 189 | 551.88 | [256, 256, 288, 288] | 32 | 3.69 | 0.92 |
| pop | 120 | 549.00 | [0, 736, 32, 768] | 32 | 6.23 | 1.55 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dread\seq_dread_hull_pass\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dread\seq_dread_hull_pass\montage_02.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dread\seq_dread_hull_pass\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dread\seq_dread_hull_pass\clip.mp4`

Frames deleted after analysis: 289 (analysed.done in the folder)