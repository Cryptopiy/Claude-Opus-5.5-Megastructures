# Frame sequences: BV_seq01

Sequences: 1. Checks: 2 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| traffic_ks06_tram | traffic | 480 | 0.0003 | 0.741 | 0.990 | 215.2 | PASS |  |

## traffic_ks06_tram (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_seq01\seq_traffic_ks06_tram`, 480 frames at 60 fps, kind traffic

- luma      `▄▄▄▄▃▃▃▃▃▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▃▃▄▄▄▄▄▅▅▆▇███████▇█`
- mean diff `▂▂▂▂▂▂▃▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▃▄▄▄▄▅▅▆▆▇▇▇▇▇▇▇▇▇▇█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0850 | <= 1.2 levels | at frame 408 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
