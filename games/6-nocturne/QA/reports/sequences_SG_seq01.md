# Frame sequences: SG_seq01

Sequences: 1. Checks: 9 PASS, 3 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| signs_ks16 | signs | 1800 | 0.0002 | 0.130 | 0.195 | 174.6 | 1 / 1185 | FAIL | block pops (one-frame jumps), 11/21 px blocks, block blinks (out and back), 11/21 px blocks, G5 dominant block frequency |

## signs_ks16 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq01\seq_signs_ks16`, 1800 frames at 30 fps, kind signs

- luma      `▃▃▄▄▄▅▅▅▄▄▃▃▃▃▃▄▄▄▅▅▅▅▅▅▄▅▅▆▇▇███▇▇▆▆▆▆▆▇█████▇▆▆▅▅▄▄▄▄▄▃▃▂▁`
- mean diff `▄▅▅▃▃▄▄▄▄▅▄▃▄▅▆▄▅▄▃▅█▅▄▅▄▃▂▄▄▅▄▅▄▆▆▄▄▄▅▅▆▅▅▅▅▆▆▆▆▄▄▆▅▅▄▅▄▃▃▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0400 | <= 1.2 levels | at frame 1389 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 11/21 px blocks | FAIL | 1 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [928]; worst f928 s1150.0 11px@[88, 33] 2.1/0.59; 1490 rain-streak and 655 motion-explained events excluded |
| block blinks (out and back), 11/21 px blocks | FAIL | 1185 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [9, 25, 26, 27, 31, 32, 33, 34, 35, 37, 38, 39]; worst f1713 s1150.0 11px@[55, 341] 5.7/0.25; f1622 s1150.0 11px@[22, 341] 5.6/0.25; f1308 s1150.0 11px@[22, 330] 5.5/0.25; f1712 s1150.0 11px@[66, 308] 5.4/0.25; f1621 s1150.0 11px@[44, 286] 4.8/0.25; 1490 rain-streak and 655 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 0.1700 | < 25 % of the screen | worst frame 1626; 0 frames over; 2682 block transitions of >= 0.1 in all (R4 3.3, D-010; 5 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.1000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | FAIL | 2.9833 | <= 0.5 Hz (amplitude floor 0.002) | 5050 of 9216 blocks modulated, 16 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00342 (<= 0.002); df 0.0167 Hz |
| G6 mean-luma step | PASS | 0.0684 | <= 0.3 % / frame | at frame 1389 |
| G7 red share | PASS | 0.6398 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 5 px blocks |
| mean change per second (linear) | PASS | 3.0050 | <= 4 % / s | frame-mean linear Y, 1 s apart |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 1713 | 1150.00 | [55, 341, 66, 352] | 11 | 5.66 | 0.25 |
| blink | 1622 | 1150.00 | [22, 341, 33, 352] | 11 | 5.57 | 0.25 |
| blink | 1308 | 1150.00 | [22, 330, 33, 341] | 11 | 5.53 | 0.25 |
| blink | 1712 | 1150.00 | [66, 308, 77, 319] | 11 | 5.35 | 0.25 |
| blink | 1621 | 1150.00 | [44, 286, 55, 297] | 11 | 4.83 | 0.25 |
| blink | 1237 | 1150.00 | [121, 242, 132, 253] | 11 | 4.77 | 0.25 |
| blink | 1622 | 1150.00 | [22, 330, 33, 341] | 11 | 4.68 | 0.25 |
| blink | 663 | 1150.00 | [77, 253, 88, 264] | 11 | 4.55 | 0.25 |
| blink | 1044 | 1150.00 | [55, 253, 66, 264] | 11 | 4.50 | 0.25 |
| blink | 1046 | 1150.00 | [0, 341, 11, 352] | 11 | 4.41 | 0.25 |
| blink | 1620 | 1150.00 | [66, 242, 77, 253] | 11 | 4.27 | 0.25 |
| blink | 1050 | 1150.00 | [88, 231, 99, 242] | 11 | 4.20 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq01\seq_signs_ks16\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq01\seq_signs_ks16\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq01\seq_signs_ks16\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq01\seq_signs_ks16\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq01\seq_signs_ks16\pops_crops.png`

Frames deleted after analysis: 1800 (analysed.done in the folder)