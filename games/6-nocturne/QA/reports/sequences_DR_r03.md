# Frame sequences: DR_r03

Sequences: 4. Checks: 8 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| dr_faceb | traffic | 30 | 0.0001 | 0.168 | 0.187 | 228.4 | PASS |  |
| dr_hull | traffic | 30 | 0.0001 | 0.141 | 0.160 | 219.5 | PASS |  |
| dr_panel | traffic | 30 | 0.0001 | 0.192 | 0.206 | 211.3 | PASS |  |
| dr_shadow | traffic | 30 | 0.0000 | 0.124 | 0.128 | 185.4 | PASS |  |

## dr_faceb (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r03\seq_dr_faceb`, 30 frames at 30 fps, kind traffic

- luma      `▂▁▁▂▁▁▁▁▂▃▂▄▅▆▆▆▇▆▆▅▆▇██▆▆▅▇▅▅`
- mean diff `▄▂▃▃▃▄▅▆▅▃▃▃▃▃▂▃▅▆▅▅▆▇█▆▃▁▃▆█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0210 | <= 1.2 levels | at frame 24 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## dr_hull (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r03\seq_dr_hull`, 30 frames at 30 fps, kind traffic

- luma      `▁▂▂▄▄▄▄▃▅▄▄▄▄▄▅▃▄▄▅▄▅▄▇▆▄▃▄▆▆█`
- mean diff `▂▄▅███▇▇▆▄▆▆▅▆▄▃▅▄▃▃▃▅▅▃▁▂▂▂▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0180 | <= 1.2 levels | at frame 22 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## dr_panel (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r03\seq_dr_panel`, 30 frames at 30 fps, kind traffic

- luma      `▃▃▄▆█▇▅▄▃▃▃▃▄▄▄▅▄▄▄▄▂▁▁▃▄▃▂▂▃▄`
- mean diff `▄▃▄▄▂▆▆▄▄▆█▆█▅▅▄▄█▅▆▆▄▂▃▃▂▂▁▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0190 | <= 1.2 levels | at frame 3 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## dr_shadow (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r03\seq_dr_shadow`, 30 frames at 30 fps, kind traffic

- luma      `█▇▇▆▆▆▆▆▅▆▅▅▅▅▅▅▅▄▃▄▃▄▃▃▃▃▂▂▁▁`
- mean diff `▄▂▁▂▃▁▂▃▆▅▄▄▄▆▆▇█▆▅▆▄▁▂▃▃▂▃▃▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0030 | <= 1.2 levels | at frame 10 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
