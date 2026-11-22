# Frame sequences: p4_01_texpans

Sequences: 7. Checks: 21 PASS, 0 FAIL, 7 WARN, 7 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| texpan_ks00 | texpan | 30 | 0.0001 | 1.210 | 1.219 | 214.1 | WARN |  |
| texpan_ks04 | texpan | 30 | 0.0001 | 1.444 | 1.453 | 216.9 | WARN |  |
| texpan_ks05 | texpan | 30 | 0.0001 | 1.489 | 1.505 | 209.0 | WARN |  |
| texpan_ks08 | texpan | 30 | 0.0000 | 1.302 | 1.309 | 212.3 | WARN |  |
| texpan_ks09 | texpan | 30 | 0.0001 | 0.945 | 0.950 | 222.4 | WARN |  |
| texpan_ks15 | texpan | 30 | 0.0000 | 1.895 | 1.900 | 172.9 | WARN |  |
| texpan_ks17 | texpan | 30 | 0.0001 | 1.106 | 1.140 | 194.5 | WARN |  |

## texpan_ks00 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_texpans\seq_texpan_ks00`, 30 frames at 30 fps, kind texpan

- luma      `▁▂▄▂▄▃▃▃▃▃▃▃▄▄▅▄▄▄▄▄▆▅▇▅▆▇██▇█`
- mean diff `▃▁▄▃▂▃▄▄▅▄▄▄▄▂▅▅▆▆▆▄▆▅▇▆█▅▆▇▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0160 | <= 1.2 levels | at frame 20 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | WARN | rain None, freezeClock False | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.768 |  | mean 0.841 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.2520 | <= 0.5 px (max <= 1) | max 0.361 px over 33 textured 64 px blocks; pan 14.5 px; worst [640, 384] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks04 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_texpans\seq_texpan_ks04`, 30 frames at 30 fps, kind texpan

- luma      `▄▄▃▂▃▂▃▂▁▂▂▅▃▅▅▇▆▇▇▇█▆▇▆▇▆▆▆▆▇`
- mean diff `█▃▄▆▆▆▆▁▁▄▁▂▅▄▄▄▂▅▆▂▂▁▃▇▅▃▃▄▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0140 | <= 1.2 levels | at frame 11 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | WARN | rain None, freezeClock False | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 4.164 |  | mean 1.098 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1760 | <= 0.5 px (max <= 1) | max 0.347 px over 220 textured 64 px blocks; pan 14.5 px; worst [832, 192] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks05 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_texpans\seq_texpan_ks05`, 30 frames at 30 fps, kind texpan

- luma      `███▇▇▇▇▆▆▆▅▆▅▅▅▆▅▅▄▄▄▄▄▄▃▃▂▂▁▁`
- mean diff `▇▅█▅█▅▅▆▆▆▅▇▅▆▆▁▃▆▆▄▄▃▃▄▄▂▃▁▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0180 | <= 1.2 levels | at frame 18 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | WARN | rain None, freezeClock False | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.691 |  | mean 1.119 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.2020 | <= 0.5 px (max <= 1) | max 0.363 px over 203 textured 64 px blocks; pan 14.5 px; worst [128, 384] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks08 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_texpans\seq_texpan_ks08`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▂▂▃▂▃▃▃▄▄▄▄▅▆▆▇████▇▇█▇▇▇███`
- mean diff `▁▂▁▂▄▂▂▃▃▄▃▄▄▅▅▆▄▅▆▅▇▇▆▆▇▇▇▇█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0120 | <= 1.2 levels | at frame 6 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | WARN | rain None, freezeClock False | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.647 |  | mean 1.040 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1840 | <= 0.5 px (max <= 1) | max 0.528 px over 183 textured 64 px blocks; pan 14.5 px; worst [1024, 768] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks09 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_texpans\seq_texpan_ks09`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▂▂▂▃▃▄▄▄▅▅▅▅▅▅▆▆▆▆▇▇▇▇██████`
- mean diff `▅▄▃█▄▅▇▅▄▆▆▅▅▆▃█▅▆▄▇▇▄▃█▁▇▅▆▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0150 | <= 1.2 levels | at frame 7 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | WARN | rain None, freezeClock False | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 4.506 |  | mean 1.010 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1810 | <= 0.5 px (max <= 1) | max 0.373 px over 82 textured 64 px blocks; pan 14.5 px; worst [256, 64] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks15 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_texpans\seq_texpan_ks15`, 30 frames at 30 fps, kind texpan

- luma      `▅▇▆▇▇▇█▇▇▇▇▇▇▅▄▄▅▄▂▄▄▂▂▁▃▂▂▂▃▂`
- mean diff `▇▃█▄▆▆▅▄▇▄█▅▅▇▄▄▆▅▅▄▅▅▅▃▅▃█▁▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0050 | <= 1.2 levels | at frame 18 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | WARN | rain None, freezeClock False | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.594 |  | mean 1.335 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1000 | <= 0.5 px (max <= 1) | max 0.253 px over 251 textured 64 px blocks; pan 14.5 px; worst [448, 64] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks17 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_texpans\seq_texpan_ks17`, 30 frames at 30 fps, kind texpan

- luma      `█▆▇▅▆▅▅▅▅▄▄▄▄▄▃▄▃▄▃▃▂▂▂▃▁▂▁▁▁▁`
- mean diff `█▃▅▆▄▅▄▄▄▄▃▄▃▃▃▃▃▂▂▂▂▃▂▂▂▂▁▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0140 | <= 1.2 levels | at frame 1 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | WARN | rain None, freezeClock False | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 6.219 |  | mean 1.030 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1640 | <= 0.5 px (max <= 1) | max 0.455 px over 84 textured 64 px blocks; pan 14.5 px; worst [768, 448] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |
