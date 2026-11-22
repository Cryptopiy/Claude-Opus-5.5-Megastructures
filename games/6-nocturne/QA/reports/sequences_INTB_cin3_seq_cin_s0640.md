# Frame sequences: INTB_cin3_seq_cin_s0640

Sequences: 1. Checks: 7 PASS, 2 FAIL, 0 WARN, 1 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s0640 | cinematic | 1080 | 0.0028 | 3.749 | 7.514 | 233.9 | 3610 / 3943 | FAIL | block pops (one-frame jumps), 32/64 px blocks, block blinks (out and back), 32/64 px blocks |

## cin_s0640 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin3\seq_cin_s0640`, 1080 frames at 30 fps, kind cinematic

- luma      `▇▇▇▇▇▆▆▆▆▆▇▇▇▇▇▆▆▆▆▇████▇▇▇▇▇▇▇▇▆▅▅▅▆▆▆▇▆▇███▇▇▇▇▇███▇▇▆▅▃▁▂`
- mean diff `▂▃▂▂▂▂▂▁▃▄▄▄▅▅▄▄▄▃▄▅▅▅▆▅▅▄▄▂▃▅▆▆▇▇▆▆▆▅▄▄▃▄▇██▆▅▅▄▃▃▂▂▁▁▂▂▂▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.7150 | <= 1.2 levels | at frame 778 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| wide blinks (neighbour blocks blink too, AL's sign rule) | INFO | 460 |  | not gated as block blinks (area changes are the photosensitivity gates' case); worst f185 s655.46 32px@[896, 128] 108.0 (nb8 median 36.1); f255 s661.29 32px@[1024, 128] 102.6 (nb8 median 40.6); f254 s661.21 32px@[1120, 128] 100.5 (nb8 median 31.3); f190 s655.88 32px@[288, 128] 99.7 (nb8 median 35.6) |
| start transient (frame 1, after the pose jump) | INFO | 23 |  | not gated (C20); worst blink 32px@[672, 768] 19.5/0.32; blink 32px@[672, 800] 14.9/0.45; pop 32px@[704, 800] 15.6/0.53; pop 32px@[1312, 672] 8.7/0.57 |
| block pops (one-frame jumps), 32/64 px blocks | FAIL | 3610 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [3, 4, 7, 12, 17, 19, 80, 85, 89, 100, 135, 141]; worst f715 s699.62 32px@[1440, 352] 21.8/0.25; f232 s659.38 32px@[1440, 384] 23.9/0.30; f292 s664.38 32px@[896, 320] 20.8/0.27; f166 s653.88 32px@[608, 256] 18.9/0.28; f291 s664.29 64px@[1024, 256] 22.7/0.35; 735 rain-streak and 3917 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | FAIL | 3943 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]; worst f710 s699.21 32px@[1856, 128] 101.2/0.66; f731 s700.96 32px@[1664, 896] 36.0/0.25; f414 s674.54 32px@[0, 960] 29.8/0.25; f735 s701.29 32px@[1856, 992] 28.9/0.25; f414 s674.54 32px@[0, 928] 27.9/0.25; 735 rain-streak and 3917 motion-explained events excluded |
| G1 flashes (area with a pair of opposing changes in 1/3 s) | PASS | 6.5700 | < 25 % of the screen | worst frame 260; 0 frames over; a pair = two consecutive opposing transitions of >= 0.1 (darker < 0.8) inside 10 frames (1/3 s, WCAG 2.x general flash, D-041 1); 57930 block transitions in all; 16 px linear blocks |
| G1 slow pairs (area with a rise and a fall in 1 s, R4) | INFO | 14.4000 | < 25 % of the screen (static camera) | worst frame 807; the camera moves: content sweeping through a block over up to 1 s is camera motion, not a flash (D-041 1); the 1/3 s G1 above gates |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 18.6400 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 2.0833 | <= 0.5 Hz (amplitude floor 0.002) | 7954 of 8040 blocks modulated, 125 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00464 (<= 0.002); df 0.0278 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 2.9838 | <= 0.3 % / frame | at frame 1038; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.6213 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.112 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 56.2060 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 710 | 699.21 | [1856, 128, 1888, 160] | 32 | 101.16 | 0.66 |
| blink | 731 | 700.96 | [1664, 896, 1696, 928] | 32 | 36.03 | 0.25 |
| blink | 414 | 674.54 | [0, 960, 32, 992] | 32 | 29.77 | 0.25 |
| blink | 735 | 701.29 | [1856, 992, 1888, 1024] | 32 | 28.87 | 0.25 |
| blink | 414 | 674.54 | [0, 928, 32, 960] | 32 | 27.95 | 0.25 |
| blink | 734 | 701.21 | [1824, 864, 1856, 896] | 32 | 27.67 | 0.25 |
| blink | 734 | 701.21 | [1792, 992, 1824, 1024] | 32 | 25.70 | 0.25 |
| blink | 712 | 699.38 | [1664, 160, 1696, 192] | 32 | 61.83 | 0.63 |
| blink | 162 | 653.54 | [800, 96, 832, 128] | 32 | 54.05 | 0.55 |
| blink | 165 | 653.79 | [640, 96, 672, 128] | 32 | 55.18 | 0.59 |
| blink | 758 | 703.21 | [1792, 992, 1824, 1024] | 32 | 24.03 | 0.26 |
| blink | 250 | 660.88 | [800, 352, 832, 384] | 32 | 22.81 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin3\seq_cin_s0640\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin3\seq_cin_s0640\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin3\seq_cin_s0640\montage_03.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin3\seq_cin_s0640\montage_04.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin3\seq_cin_s0640\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_cin3\seq_cin_s0640\clip.mp4`

Frames deleted after analysis: 1080 (analysed.done in the folder)