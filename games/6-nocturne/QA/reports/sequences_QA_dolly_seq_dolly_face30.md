# Frame sequences: QA_dolly_seq_dolly_face30

Sequences: 1. Checks: 3 PASS, 0 FAIL, 1 WARN, 0 SKIP.

**Result: PASS**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| dolly_face30 | dolly | 300 | 0.0026 | 5.845 | 11.502 | 219.8 | - / - | WARN |  |

## dolly_face30 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dolly\seq_dolly_face30`, 300 frames at 30 fps, kind dolly

- luma      `██████▇▆▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▆▆▆▇▇██▇███▇▇▇▇▇▆▆▆▆▅▅▅▄▄▄▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6670 | <= 1.2 levels | at frame 30 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| dolly run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the camera must be the only change |
| dolly local pops (64 px blocks) | WARN | 12 | 0 one-frame block jumps (> 3x both neighbours and > 2 levels) | worst [{'frame': 1, 'block': 30, 'diff': 52.55, 'median': 2.98, 'dist_m': 790.78}, {'frame': 1, 'block': 91, 'diff': 51.65, 'median': 15.77, 'dist_m': 790.78}, {'frame': 1, 'block': 122, 'diff': 27.79, 'median': 6.2, 'dist_m': 790.78}] |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dolly\seq_dolly_face30\montage_01.png`, `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_dolly\seq_dolly_face30\montage_02.png`

Frames deleted after analysis: 300 (analysed.done in the folder)