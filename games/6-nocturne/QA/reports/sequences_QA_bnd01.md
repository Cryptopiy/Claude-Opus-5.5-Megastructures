# Frame sequences: QA_bnd01

Sequences: 1. Checks: 3 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| b23 | boundary | 601 | 0.0007 | 0.782 | 1.055 | 224.1 | 3078 / 1061 | FAIL | block pops (one-frame jumps), 16/32 px blocks, block blinks (out and back), 16/32 px blocks |

## b23 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd01\seq_b23`, 601 frames at 30 fps, kind boundary

- luma      `▇████████▇▇▇▆▆▆▆▅▅▄▄▄▃▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▂▁▂▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▃▃▃`
- mean diff `▆▆▅▆▆▇▇█▇▆▅▆▅▅▆▅▅▆▅▄▄▄▂▃▂▃▂▃▂▂▁▂▂▁▂▁▂▄▃▂▂▂▁▂▂▃▂▃▂▃▂▃▃▃▃▃▄▄▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1790 | <= 1.2 levels | at frame 215 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [400.0], worst at frame None |
| block pops (one-frame jumps), 16/32 px blocks | FAIL | 3078 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]; worst f419 s405.95 16px@[144, 112] 9.1/0.25; f18 s385.9 16px@[272, 80] 9.0/0.25; f11 s385.55 16px@[272, 80] 8.9/0.25; f152 s392.6 16px@[272, 80] 8.9/0.25; f85 s389.25 16px@[272, 80] 8.9/0.25 |
| block blinks (out and back), 16/32 px blocks | FAIL | 1061 | 0 (> 3x both neighbouring pairs and > 2 levels) | frames [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]; worst f433 s406.65 16px@[704, 0] 9.9/0.25; f207 s395.35 16px@[144, 256] 9.3/0.25; f281 s399.05 16px@[80, 0] 8.4/0.25; f332 s401.6 16px@[64, 160] 8.2/0.25; f568 s413.4 16px@[96, 256] 8.0/0.25 |


Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):

| Type | Frame | s | Block px | Size | Diff | Neighbours |
|---|---|---|---|---|---|---|
| blink | 433 | 406.65 | [704, 0, 720, 16] | 16 | 9.89 | 0.25 |
| blink | 207 | 395.35 | [144, 256, 160, 272] | 16 | 9.27 | 0.25 |
| pop | 419 | 405.95 | [144, 112, 160, 128] | 16 | 9.06 | 0.25 |
| pop | 18 | 385.90 | [272, 80, 288, 96] | 16 | 8.96 | 0.25 |
| pop | 11 | 385.55 | [272, 80, 288, 96] | 16 | 8.94 | 0.25 |
| pop | 152 | 392.60 | [272, 80, 288, 96] | 16 | 8.94 | 0.25 |
| pop | 85 | 389.25 | [272, 80, 288, 96] | 16 | 8.93 | 0.25 |
| pop | 11 | 385.55 | [208, 80, 224, 96] | 16 | 8.86 | 0.25 |
| pop | 85 | 389.25 | [224, 80, 240, 96] | 16 | 8.86 | 0.25 |
| pop | 85 | 389.25 | [208, 80, 224, 96] | 16 | 8.83 | 0.25 |
| pop | 218 | 395.90 | [272, 80, 288, 96] | 16 | 8.83 | 0.25 |
| pop | 78 | 388.90 | [208, 80, 224, 96] | 16 | 8.82 | 0.25 |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd01\seq_b23\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd01\seq_b23\montage_02.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd01\seq_b23\montage_03.png`

Pop crops (4 x): `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd01\seq_b23\pops_crops.png`

x264 clip: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_bnd01\seq_b23\clip.mp4`