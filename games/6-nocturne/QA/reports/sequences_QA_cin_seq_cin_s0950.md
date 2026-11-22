# Frame sequences: QA_cin_seq_cin_s0950

Sequences: 1. Checks: 9 PASS, 0 FAIL, 0 WARN, 1 SKIP.

**Result: PASS**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| cin_s0950 | cinematic | 720 | 0.0003 | 3.476 | 4.473 | 215.9 | 0 / 0 | PASS |  |

## cin_s0950 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0950`, 720 frames at 30 fps, kind cinematic

- luma      `▂▂▁▁▂▃▃▄▅▅▆▆▇▇▇▇▇▇███████▇██████████████▇▇▇▇▆▆▆▆▅▅▄▄▃▃▃▃▂▂▁▁`
- mean diff `▅▅▄▃▃▃▃▂▂▁▁▁▁▁▂▂▃▃▄▅▆▆▆▆▆▆▆▆▆▆▆▆▅▅▆▆▆▆▇▆▅▅▅▅▄▄▄▄▄▄▄▄▃▄▅▅▆▇▇█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0880 | <= 1.2 levels | at frame 20 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| block pops (one-frame jumps), 32/64 px blocks | PASS | 0 | 0 (> 3x both neighbouring pairs and > 2 levels) | 720 frames; 0 rain-streak and 3 motion-explained events excluded |
| block blinks (out and back), 32/64 px blocks | PASS | 0 | 0 (> 3x both neighbouring pairs and > 2 levels) | 720 frames; 0 rain-streak and 3 motion-explained events excluded |
| G1 flashes (area with a rise and a fall in 1 s) | PASS | 3.8700 | < 25 % of the screen | worst frame 280; 0 frames over; 17970 block transitions of >= 0.1 in all (R4 3.3, D-010; 16 px linear blocks) |
| G2 WCAG general (> 6 transitions / s in a 1/3 window) | PASS | 0.0000 | < 25 % of the window |  |
| G3 WCAG red (saturated-red transitions) | PASS | 0.0000 | < 25 % of the window | saturated red: R / (R + G + B) >= 0.8 linear |
| G4 saturated-red area | PASS | 0.0000 | <= 10 % of any window |  |
| G5 dominant block frequency | INFO | 1.4167 | <= 0.5 Hz (amplitude floor 0.002) | 7272 of 8040 blocks modulated, 119 over 0.5 Hz; per-bin RMS above 0.5 Hz max 0.00402 (<= 0.002); df 0.0417 Hz; the camera moves: content motion, not modulation (INFO) |
| G6 mean-luma step | INFO | 0.3048 | <= 0.3 % / frame | at frame 20; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 luma-step check above gates this sequence (INFO) |
| G7 red share | PASS | 0.5878 | p99.9 <= 0.7 (static); blocks over 0.7 <= 2 % / frame | worst frame 0.012 % of the blocks with Y > 0.02 over 0.7; share = R / (R + G + B) linear, 16 px blocks |
| mean change per second (linear) | INFO | 7.3390 | <= 4 % / s | frame-mean linear Y, 1 s apart; the camera moves (INFO) |
| G8 pattern contrast | SKIP | - |  | a still test: the 4K KS16 still (image_metrics), not a sequence |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0950\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0950\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0950\montage_03.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_cin\seq_cin_s0950\clip.mp4`

Frames deleted after analysis: 720 (analysed.done in the folder)