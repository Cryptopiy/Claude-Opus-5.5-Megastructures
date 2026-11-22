# Frame sequences: QA_dread_seq_dread_lightning_s0553

Sequences: 1. Checks: 7 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| dread_lightning_s0553 | dread | 180 | 0.0020 | 2.143 | 2.914 | 234.9 | 59 / 187 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## dread_lightning_s0553 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dread\seq_dread_lightning_s0553`, 180 frames at 30 fps, kind dread

- luma      `▆▆▆▆▆▇▇▇██████████████████▇▇▇▇▆▆▆▅▅▅▄▄▃▃▃▃▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▂▃▅▅▆▇▇▇███▇▇▇▇▇▇▇▇▇▇▇▇▇▆▆▆▅▅▅▅▄▄▃▂▂▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5130 | <= 1.2 levels | at frame 18 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| start transient (frame 1, after the pose jump) | INFO | 6 |  | not gated (C20); worst pop 32px@[160, 256] 2.4/0.42; pop 32px@[192, 320] 2.5/0.50; pop 32px@[608, 608] 3.6/0.86; pop 32px@[224, 384] 2.1/0.54 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 59 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [3, 7, 16, 18, 19, 22, 26, 29, 30, 39, 46, 50]; worst f100 s556.38 32px@[384, 608] 19.3/2.45; f90 s555.54 32px@[576, 640] 23.3/3.24; f90 s555.54 32px@[1408, 288] 38.7/5.76; f100 s556.38 32px@[1120, 288] 35.7/5.69; f142 s559.88 32px@[512, 416] 23.9/3.83; 30 rain-streak and 385 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 187 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15]; worst f53 s552.46 32px@[544, 416] 21.9/1.80; f40 s551.38 32px@[32, 256] 3.5/0.34; f43 s551.62 32px@[736, 288] 18.3/1.80; f14 s549.21 32px@[0, 608] 3.0/0.29; f51 s552.29 32px@[576, 384] 18.6/1.87; 30 rain-streak and 385 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 6.1900 | < 25 % of the screen | worst frame 47; 0 frames over; 3780 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.0000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 0.8333 | <= 0.5 Hz (amplitude floor 0.002) | 5504 of 8040 blocks modulated, 133 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.02874 (<= 0.002); df 0.1667 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 1.5200 | <= 0.3 % / frame | at frame 108; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6009 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 45.4600 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 53 | 552.46 | [544, 416, 576, 448] | 32 | 21.88 | 1.80 |
| blink | 40 | 551.38 | [32, 256, 64, 288] | 32 | 3.47 | 0.34 |
| blink | 43 | 551.62 | [736, 288, 768, 320] | 32 | 18.30 | 1.80 |
| blink | 14 | 549.21 | [0, 608, 32, 640] | 32 | 2.96 | 0.29 |
| blink | 51 | 552.29 | [576, 384, 608, 416] | 32 | 18.62 | 1.87 |
| blink | 65 | 553.46 | [1152, 672, 1184, 704] | 32 | 12.72 | 1.60 |
| pop | 100 | 556.38 | [384, 608, 416, 640] | 32 | 19.28 | 2.45 |
| blink | 7 | 548.62 | [672, 224, 704, 256] | 32 | 2.55 | 0.35 |
| pop | 90 | 555.54 | [576, 640, 608, 672] | 32 | 23.32 | 3.24 |
| blink | 66 | 553.54 | [1120, 640, 1152, 672] | 32 | 12.66 | 1.77 |
| blink | 26 | 550.21 | [1152, 672, 1184, 704] | 32 | 20.81 | 3.09 |
| pop | 90 | 555.54 | [1408, 288, 1440, 320] | 32 | 38.69 | 5.76 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dread\seq_dread_lightning_s0553\montage_01.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dread\seq_dread_lightning_s0553\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dread\seq_dread_lightning_s0553\clip.mp4`

Frames deleted after analysis: 180 (analysed.done in the folder)