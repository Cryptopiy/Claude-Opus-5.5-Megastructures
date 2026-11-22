# Frame sequences: QA_dolly_seq_dolly_graze

Sequences: 1. Checks: 3 PASS, 0 FAIL, 1 WARN, 0 SKIP.

**Result: PASS**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| dolly_graze | dolly | 251 | 0.0011 | 3.692 | 4.747 | 226.4 | - / - | WARN |  |

## dolly_graze (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dolly\seq_dolly_graze`, 251 frames at 30 fps, kind dolly

- luma      `▄▄▄▄▄▄▅▅▅▅▅▅▅▅▆▆▆▆▆▆▇▇▇▇████████▇▇▇▇▇▇▆▆▆▆▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁▁`
- mean diff `▂▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▃▃▃▄▄▄▅▆▅▅▆▇▇▇▇▇▇▇▇▇▇██▇▆▇▆▆▅▅▅▅▅▄▄▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2740 | <= 1.2 levels | at frame 188 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| dolly run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the camera must be the only change |
| dolly local pops (64 px blocks) | WARN | 17 | 0 one-frame block jumps (> 3x both neighbours and > 2 levels) | worst [{'frame': 137, 'block': 450, 'diff': 32.55, 'median': 5.5, 'dist_m': 842.93}, {'frame': 135, 'block': 450, 'diff': 31.61, 'median': 5.63, 'dist_m': 848.78}, {'frame': 139, 'block': 450, 'diff': 32.3, 'median': 7.11, 'dist_m': 837.09}] |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dolly\seq_dolly_graze\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dolly\seq_dolly_graze\montage_02.png`

Frames deleted after analysis: 251 (analysed.done in the folder)