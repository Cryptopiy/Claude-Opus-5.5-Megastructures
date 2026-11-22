# Frame sequences: QA_signs_seq_signs_ks16

Sequences: 1. Checks: 9 PASS, 3 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| signs_ks16 | signs | 1800 | 0.0001 | 0.116 | 0.163 | 181.0 | 1 / 966 | FAIL | block pops (one-frame jumps), 16/32 px blocks, block blinks (out and back), 16/32 px blocks, G5 dominant block frequency |

## signs_ks16 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_signs\seq_signs_ks16`, 1800 frames at 30 fps, kind signs

- luma      `▂▂▁▁▁▂▂▂▃▃▃▃▃▂▂▂▂▃▄▄▅▅▅▄▄▃▂▂▃▄▅▆▆▆▆▆▅▄▃▃▃▄▅▇████▇▆▅▅▄▅▆▇▇███`
- mean diff `▂▇▃▄▃▁▃▅▅▃▄▅▆▄▄▄▅▅▃▅▂▂▃▃▆▄▄▅▄█▃▅▄▃▄▆▃▃▂▂▁▁▂▃▄▂▄▂▇▆▃▄▂▄▅▇▆▃▃▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0300 | <= 1.2 levels | at frame 1266 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| start transient (frame 1, after the pose jump) | INFO | 1 |  | not gated (C20); worst blink 16px@[112, 272] 3.1/0.25 |
| block pops (one-frame jumps), 16/32 px blocks | FAIL | 1 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1456]; worst f1456 s1150.0 16px@[128, 48] 2.8/0.87; 2005 rain-streak and 460 motion-explained events excluded |
| block blinks (out and back), 16/32 px blocks | FAIL | 966 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [2, 3, 4, 26, 32, 36, 38, 39, 44, 45, 47, 52]; worst f1758 s1150.0 16px@[32, 496] 5.3/0.25; f1439 s1150.0 16px@[864, 0] 4.8/0.25; f844 s1150.0 16px@[160, 480] 4.7/0.25; f363 s1150.0 16px@[256, 160] 4.3/0.25; f849 s1150.0 16px@[864, 0] 4.2/0.25; 2005 rain-streak and 460 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 0.1400 | < 25 % of the screen | worst frame 115; 0 frames over; 1021 block transitions of >= 0.1 in all (R4 3.3, D-010; 8 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.0000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | FAIL | 2.5500 | <= 0.5 Hz (amplitude floor 0.002) | 3658 of 8040 blocks modulated, 15 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00243 (<= 0.002); df 0.0167 Hz |
| G6 mean-luma step | PASS | 0.0562 | <= 0.3 % / frame | at frame 1266 |
| G7 red share | PASS | 0.6169 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 8 px blocks |
| mean change per second (linear) | PASS | 2.6100 | <= 4 % / s | frame-mean linear Y, 1 s apart |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 1758 | 1150.00 | [32, 496, 48, 512] | 16 | 5.26 | 0.25 |
| blink | 1439 | 1150.00 | [864, 0, 880, 16] | 16 | 4.78 | 0.25 |
| blink | 844 | 1150.00 | [160, 480, 176, 496] | 16 | 4.66 | 0.25 |
| blink | 363 | 1150.00 | [256, 160, 272, 176] | 16 | 4.33 | 0.25 |
| blink | 849 | 1150.00 | [864, 0, 880, 16] | 16 | 4.25 | 0.25 |
| blink | 1106 | 1150.00 | [256, 144, 272, 160] | 16 | 4.94 | 0.29 |
| blink | 722 | 1150.00 | [192, 208, 208, 224] | 16 | 4.16 | 0.25 |
| blink | 721 | 1150.00 | [208, 176, 224, 192] | 16 | 4.11 | 0.25 |
| blink | 1494 | 1150.00 | [80, 384, 96, 400] | 16 | 4.11 | 0.25 |
| blink | 203 | 1150.00 | [240, 192, 256, 208] | 16 | 4.09 | 0.25 |
| blink | 1495 | 1150.00 | [48, 448, 64, 464] | 16 | 3.95 | 0.25 |
| blink | 1496 | 1150.00 | [0, 512, 16, 528] | 16 | 3.92 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_signs\seq_signs_ks16\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_signs\seq_signs_ks16\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_signs\seq_signs_ks16\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_signs\seq_signs_ks16\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_signs\seq_signs_ks16\pops_crops.png`

Frames deleted after analysis: 1800 (analysed.done in the folder)