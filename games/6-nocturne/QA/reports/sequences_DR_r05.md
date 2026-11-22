# Frame sequences: DR_r05

Sequences: 3. Checks: 4 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| dr_cabins | traffic | 0 | - | - | - | - | FAIL | frames |
| dr_hullpass | traffic | 30 | 0.0001 | 0.387 | 0.406 | 230.9 | PASS |  |
| dr_shadow | traffic | 30 | 0.0000 | 0.145 | 0.154 | 210.8 | PASS |  |

## dr_cabins (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r05\seq_dr_cabins`, 0 frames at 30 fps, kind traffic

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| frames | FAIL | 0 | >= 2 |  |

## dr_hullpass (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r05\seq_dr_hullpass`, 30 frames at 30 fps, kind traffic

- luma      `▁▂▂▄▆▇██▇▇█████▆▃▂▁▁▂▂▂▂▂▃▄▅▇▇`
- mean diff `▁▃▃▁▄▃▃▃▃▃▂▃▄▄▅▇▇▃▂▃▃▃▅█▆▅▅▅▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0250 | <= 1.2 levels | at frame 3 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## dr_shadow (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r05\seq_dr_shadow`, 30 frames at 30 fps, kind traffic

- luma      `▃▁▁▄▄▄▃▁▁▃▂▄▅▆▅▄▃▂▂▂▁▂▆█▇▅▄▄▄▅`
- mean diff `▁▂▂▂▃▂▂▄▅▆▄▄▅▅▅▇▆▃▂▃▄▅▅▆▇▇▇██`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0100 | <= 1.2 levels | at frame 22 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
