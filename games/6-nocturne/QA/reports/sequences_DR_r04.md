# Frame sequences: DR_r04

Sequences: 3. Checks: 6 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| dr_cabins | traffic | 30 | 0.0001 | 0.154 | 0.173 | 210.0 | PASS |  |
| dr_hullpass | traffic | 30 | 0.0001 | 0.387 | 0.406 | 231.1 | PASS |  |
| dr_shadow | traffic | 30 | 0.0000 | 0.145 | 0.154 | 210.8 | PASS |  |

## dr_cabins (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r04\seq_dr_cabins`, 30 frames at 30 fps, kind traffic

- luma      `▆▆▅▆▇██▆▆▅▆▆▃▃▂▁▁▁▁▃▂▃▃▁▁▁▁▁▂▃`
- mean diff `▆▅▅▅▇█▆▄▃▃▃▃▄▄▃▂▁▂▄▅▅▆▅▃▂▁▁▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0180 | <= 1.2 levels | at frame 12 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## dr_hullpass (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r04\seq_dr_hullpass`, 30 frames at 30 fps, kind traffic

- luma      `▁▂▂▄▆▇██▇▇█████▆▃▂▁▁▂▂▂▂▂▃▄▅▇▇`
- mean diff `▁▃▃▁▄▃▃▃▃▃▂▃▄▄▅▇▇▃▃▃▃▃▅█▆▅▅▅▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0250 | <= 1.2 levels | at frame 3 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## dr_shadow (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\DR_r04\seq_dr_shadow`, 30 frames at 30 fps, kind traffic

- luma      `▃▁▁▄▄▄▃▁▁▃▂▄▅▆▅▄▃▂▂▂▁▂▆█▇▅▄▄▄▅`
- mean diff `▁▂▂▂▃▂▂▄▅▆▄▄▅▅▅▇▆▃▂▃▄▅▅▆▇▇▇██`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0100 | <= 1.2 levels | at frame 22 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
