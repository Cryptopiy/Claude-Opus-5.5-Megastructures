# Frame sequences: G4_bd_s1240

Sequences: 1. Checks: 3 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| s1240 | boundary | 1201 | 0.0005 | 2.586 | 4.784 | 218.4 | PASS |  |

## s1240 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G4_bd_s1240\seq_s1240`, 1201 frames at 30 fps, kind boundary

- luma      `▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▃▃▄▄▅▅▆▆▆▆▆▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇████████`
- mean diff `▅▅▅▅▅▅▅▅▅▅▆▆▆▆▇▇▇▇▇█████▇▇▇▇▇▆▆▆▆▆▆▆▆▆▅▅▅▅▅▅▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1240 | <= 1.2 levels | at frame 340 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.2876 | <= 0.5 level | boundaries [1260.0], worst at frame 400 |
