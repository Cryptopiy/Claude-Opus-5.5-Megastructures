# Frame sequences: FX_arkseq3

Sequences: 1. Checks: 6 PASS, 3 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| ark_cin_belly | dread | 1272 | 0.0053 | 2.995 | 7.447 | 228.9 | 84 / 518 | FAIL | luma step/frame, block pops (one-frame jumps), 11/21 px blocks, block blinks (out and back), 11/21 px blocks |

## ark_cin_belly (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq3\seq_ark_cin_belly`, 1272 frames at 30 fps, kind dread

- luma      `▁▂▂▂▂▂▂▂▃▃▅▅▆▅▅▄▄▄▄▄▃▃▃▃▃▄▄▆▇▇▇▇▅▆▆▆▆▆▆▆▆▆▆▆▇███▅▄▄▃▃▃▂▂▂▂▂▂`
- mean diff `▄▅▅▄▃▂▂▄▅▅▆▆▆▆▄▂▂▂▂▂▂▂▂▂▂▂▃▆▆▆▆▆▄▃▂▂▁▁▁▁▁▁▁▁▃▄▆▆▇████▇▅▄▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 1.3510 | <= 1.2 levels | at frame 1006 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 11 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f993 s822.79 11px@[22, 341] 46.7 (nb8 median 22.9); f990 s822.54 11px@[77, 341] 39.7 (nb8 median 17.4); f992 s822.71 11px@[44, 341] 33.8 (nb8 median 15.4); f37 s743.12 11px@[33, 330] 18.1 (nb8 median 5.8) |
| start transient (frame 1, after the pose jump) | INFO | 2 |  | not gated (C20); worst pop 11px@[66, 143] 2.7/0.56; pop 11px@[88, 0] 11.3/3.53 |
| block pops (one-frame jumps), 11/21 px blocks | FAIL | 84 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [89, 93, 263, 265, 276, 280, 283, 300, 343, 369, 375, 381]; worst f772 s804.38 11px@[451, 330] 5.0/0.25; f777 s804.79 11px@[55, 198] 6.4/0.40; f1057 s828.12 11px@[0, 341] 17.3/1.42; f404 s773.71 11px@[77, 55] 3.9/0.34; f464 s778.71 11px@[495, 165] 2.2/0.25; 73 rain-streak and 2104 motion-explained events excluded |
| block blinks (out and back), 11/21 px blocks | FAIL | 518 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [4, 6, 7, 8, 9, 10, 12, 13, 14, 27, 28, 29]; worst f994 s822.88 11px@[11, 330] 62.4/0.72; f991 s822.62 11px@[66, 330] 53.1/1.13; f995 s822.96 11px@[0, 319] 61.3/1.42; f991 s822.62 11px@[55, 341] 55.0/1.28; f989 s822.46 11px@[99, 330] 51.8/1.39; 73 rain-streak and 2104 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 8.6700 | < 25 % of the screen | worst frame 1061; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 112166 block transitions in all; 5 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 27.3800 | < 25 % of the screen (static camera) | worst frame 645; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 2.0800 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.5000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 2.1934 | <= 0.5 Hz (amplitude floor 0.002) | 9216 of 9216 blocks modulated, 345 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00742 (<= 0.002); df 0.0236 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 2.9568 | <= 0.3 % / frame | at frame 1006; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.7438 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.694 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 5 px blocks |
| mean change per second (linear) | INFO | 50.0550 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 994 | 822.88 | [11, 330, 22, 341] | 11 | 62.41 | 0.72 |
| blink | 991 | 822.62 | [66, 330, 77, 341] | 11 | 53.06 | 1.13 |
| blink | 995 | 822.96 | [0, 319, 11, 330] | 11 | 61.35 | 1.42 |
| blink | 991 | 822.62 | [55, 341, 66, 352] | 11 | 55.00 | 1.28 |
| blink | 989 | 822.46 | [99, 330, 110, 341] | 11 | 51.80 | 1.39 |
| blink | 997 | 823.12 | [0, 286, 11, 297] | 11 | 55.08 | 1.78 |
| blink | 79 | 746.62 | [154, 341, 165, 352] | 11 | 10.32 | 0.39 |
| blink | 12 | 741.04 | [33, 341, 44, 352] | 11 | 15.68 | 0.61 |
| blink | 994 | 822.88 | [33, 308, 44, 319] | 11 | 53.76 | 2.10 |
| blink | 78 | 746.54 | [165, 341, 176, 352] | 11 | 10.75 | 0.43 |
| blink | 39 | 743.29 | [0, 330, 11, 341] | 11 | 21.43 | 0.90 |
| blink | 1000 | 823.38 | [0, 242, 11, 253] | 11 | 49.82 | 2.15 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq3\seq_ark_cin_belly\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq3\seq_ark_cin_belly\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq3\seq_ark_cin_belly\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq3\seq_ark_cin_belly\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq3\seq_ark_cin_belly\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\FX_arkseq3\seq_ark_cin_belly\clip.mp4`