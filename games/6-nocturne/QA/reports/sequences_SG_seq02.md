# Frame sequences: SG_seq02

Sequences: 1. Checks: 10 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| signs_ks16 | signs | 1800 | 0.0001 | 0.044 | 0.080 | 174.6 | 1 / 0 | FAIL | block pops (one-frame jumps), 11/21 px blocks, G5 dominant block frequency |

## signs_ks16 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq02\seq_signs_ks16`, 1800 frames at 30 fps, kind signs

- luma      `▃▃▄▄▄▅▅▅▄▄▃▃▃▃▃▄▄▄▅▅▅▅▅▅▄▅▅▆▇▇████▇▆▆▆▆▆▇█████▇▆▆▅▅▄▄▄▄▄▃▃▂▁`
- mean diff `▄▄▄▃▂▂▁▂▂▃▃▂▃▄▄▄▄▃▅▇▇█▇▄▂▃▃▄▅▄▄▃▃▃▃▃▁▂▂▄▄▃▂▁▂▂▅▄▄▄▄▄▅▅▅▅▅▅▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0280 | <= 1.2 levels | at frame 1704 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 11/21 px blocks | FAIL | 1 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [928]; worst f928 s1150.0 11px@[88, 33] 2.1/0.59; 1 rain-streak and 15 motion-explained events excluded |
| block blinks (out and back), 11/21 px blocks | PASS | 0 | 0 (> 3x both neighbouring pairs and > 2 levels) | 1800 frames; 1 rain-streak and 15 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 0.1700 | < 25 % of the screen | worst frame 1626; 0 frames over; 2471 block transitions of >= 0.1 in all (R4 3.3, D-010; 5 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.1000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | FAIL | 1.2000 | <= 0.5 Hz (amplitude floor 0.002) | 5049 of 9216 blocks modulated, 16 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00342 (<= 0.002); df 0.0167 Hz |
| G6 mean-luma step | PASS | 0.0522 | <= 0.3 % / frame | at frame 1735 |
| G7 red share | PASS | 0.6399 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.000 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 5 px blocks |
| mean change per second (linear) | PASS | 3.0080 | <= 4 % / s | frame-mean linear Y, 1 s apart |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| pop | 928 | 1150.00 | [88, 33, 99, 44] | 11 | 2.07 | 0.59 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq02\seq_signs_ks16\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq02\seq_signs_ks16\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq02\seq_signs_ks16\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq02\seq_signs_ks16\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_seq02\seq_signs_ks16\pops_crops.png`

Frames deleted after analysis: 1800 (analysed.done in the folder)