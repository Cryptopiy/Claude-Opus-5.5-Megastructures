# Frame sequences: QA_cin_seq_cin_s1490

Sequences: 1. Checks: 7 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s1490 | cinematic | 187 | 0.0005 | 1.694 | 2.957 | 212.8 | 5 / 40 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## cin_s1490 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1490`, 187 frames at 30 fps, kind cinematic

- luma      `█████▇▇▇▇▇▆▆▆▆▆▅▅▅▅▅▄▄▄▄▄▃▃▃▃▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▄▄▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▅▆▆▆▆▆▆▇▇▇▇▇▇██████████▇▇▆▆▅▄▃▂▂▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1200 | <= 1.2 levels | at frame 55 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 5 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 21, 78, 186]; worst f186 s1500.0 32px@[1888, 128] 4.9/0.25; f1 s1490.12 32px@[1472, 224] 3.1/0.37; f186 s1500.0 32px@[1888, 96] 2.4/0.58; f78 s1496.54 32px@[768, 128] 2.0/0.55; f21 s1491.79 32px@[576, 1024] 7.4/2.28; 65 rain-streak and 1 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 40 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [2, 6, 7, 10, 13, 14, 18, 19, 52, 53, 60, 64]; worst f18 s1491.54 32px@[768, 320] 5.7/0.25; f165 s1500.0 32px@[1856, 96] 5.5/0.25; f185 s1500.0 32px@[1856, 64] 5.2/0.25; f185 s1500.0 32px@[1824, 192] 4.2/0.25; f165 s1500.0 32px@[1888, 128] 5.0/0.30; 65 rain-streak and 1 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 0.8100 | < 25 % of the screen | worst frame 109; 0 frames over; 2066 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.0000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 1.2834 | <= 0.5 Hz (amplitude floor 0.002) | 4769 of 8040 blocks modulated, 167 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.02059 (<= 0.002); df 0.1604 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.3302 | <= 0.3 % / frame | at frame 55; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.7532 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 1.617 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 16.7380 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 18 | 1491.54 | [768, 320, 800, 352] | 32 | 5.65 | 0.25 |
| blink | 165 | 1500.00 | [1856, 96, 1888, 128] | 32 | 5.45 | 0.25 |
| blink | 185 | 1500.00 | [1856, 64, 1888, 96] | 32 | 5.22 | 0.25 |
| pop | 186 | 1500.00 | [1888, 128, 1920, 160] | 32 | 4.89 | 0.25 |
| blink | 185 | 1500.00 | [1824, 192, 1856, 224] | 32 | 4.21 | 0.25 |
| blink | 165 | 1500.00 | [1888, 128, 1920, 160] | 32 | 4.98 | 0.30 |
| blink | 152 | 1500.00 | [1856, 128, 1888, 160] | 32 | 4.05 | 0.25 |
| blink | 184 | 1500.00 | [1824, 0, 1856, 32] | 32 | 4.16 | 0.28 |
| blink | 164 | 1500.00 | [1824, 32, 1856, 64] | 32 | 4.50 | 0.32 |
| blink | 162 | 1500.00 | [1824, 96, 1856, 128] | 32 | 3.53 | 0.25 |
| blink | 152 | 1500.00 | [1824, 96, 1856, 128] | 32 | 3.25 | 0.25 |
| blink | 151 | 1500.00 | [1792, 32, 1824, 64] | 32 | 3.24 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1490\montage_01.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1490\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s1490\clip.mp4`

Frames deleted after analysis: 187 (analysed.done in the folder)