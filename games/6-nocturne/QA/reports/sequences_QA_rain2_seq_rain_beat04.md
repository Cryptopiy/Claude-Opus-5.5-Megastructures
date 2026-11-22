# Frame sequences: QA_rain2_seq_rain_beat04

Sequences: 1. Checks: 3 PASS, 3 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| rain_beat04 | rain | 120 | 0.0000 | 0.027 | 0.037 | 210.2 | - / - | FAIL | rain H5 streak count per frame (median), rain H5 coverage, rain H5 minimum streak length |

## rain_beat04 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_rain2\seq_rain_beat04`, 120 frames at 60 fps, kind rain

- luma      `████▇▆██▆▇▇▆▆▇█▆▆▅▆▆▅▅▅▅▆▅▅▆▅▅▄▅▄▄▃▂▁▂▃▂▁▂▂▃▁▂▃▃▂▂▂▂▂▂▁▂▁▃▃▂`
- mean diff `▃▄▅▆▆▄▆▆▄▄▆▆▄▅▆▆▄▃▃▃▂▂▃▄▇▅▃▆▆▄▄▄▃▂▃▃▁▂▅▅▂▃▃▅▅▃▅▇▃▃▅▃▃▄▄▃▂▅█▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0110 | <= 1.2 levels | at frame 13 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| rain H5 streak count per frame (median) | FAIL | 2592.0000 | 140-210 | min 2520, max 2770 over 120 frames; drop streaks = components of the undilated mask (luma > temporal min + 4 codes: drops, not the far veil; 8-connected, >= 3 px) |
| rain H5 coverage | FAIL | 1.5030 | 0.6-1 % | undilated streak mask, mean over the frames |
| rain mask at the flicker threshold (drops + far veil) | INFO | 2758.0000 |  | components at > 1.5 codes, coverage 3.428 % (first 12 frames): the far veil's streak noise included |
| rain H5 minimum streak length | FAIL | 2.8000 | >= 10.0 px (10 px at 1080p) | 279507 of 313144 inner streaks shorter (p5 2.8 px); border streaks excluded |
| rain outside-streak change | PASS | 4 | <= 6 levels | 14.7 H5: streak mask = luma > temporal min + 1.5 codes, dilated 2 px; p99.9 mean 1.00; mask coverage 3.570 % (mean); 119 pairs |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_rain2\seq_rain_beat04\montage_01.png`

Frames deleted after analysis: 120 (analysed.done in the folder)