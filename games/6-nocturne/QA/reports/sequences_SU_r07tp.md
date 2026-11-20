# Frame sequences: SU_r07tp

Sequences: 7. Checks: 26 PASS, 0 FAIL, 2 WARN, 7 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| texpan_ks00 | texpan | 30 | 0.0000 | 1.279 | 1.285 | 195.2 | PASS |  |
| texpan_ks04 | texpan | 30 | 0.0000 | 1.847 | 1.853 | 195.7 | PASS |  |
| texpan_ks05 | texpan | 30 | 0.0000 | 1.790 | 1.796 | 199.9 | PASS |  |
| texpan_ks08 | texpan | 30 | 0.0001 | 1.552 | 1.557 | 208.1 | PASS |  |
| texpan_ks09 | texpan | 30 | 0.0000 | 1.316 | 1.324 | 115.1 | WARN |  |
| texpan_ks15 | texpan | 30 | 0.0000 | 0.642 | 0.644 | 153.4 | WARN |  |
| texpan_ks17 | texpan | 30 | 0.0001 | 1.311 | 1.337 | 195.8 | PASS |  |

## texpan_ks00 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r07tp\seq_texpan_ks00`, 30 frames at 30 fps, kind texpan

- luma      `▁▂▂▃▂▄▃▄▄▄▄▄▄▅▅▆▆▆▆▇▇▇▇█▇█▇█▇█`
- mean diff `▁▃▂▄▂▄▂▅▄▄▃▃▃▄▅▄▅▆▇▅▆▆▆▆▇▅▇▅█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0050 | <= 1.2 levels | at frame 23 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.877 |  | mean 0.858 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.2960 | <= 0.5 px (max <= 1) | max 0.613 px over 33 textured 64 px blocks; pan 14.5 px; worst [960, 192] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks04 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r07tp\seq_texpan_ks04`, 30 frames at 30 fps, kind texpan

- luma      `█▆▆▆▇▅▆▄▅▅▄▅▃▄▂▂▁▃▂▄▂▃▃▃▅▄▅▅▆▆`
- mean diff `▃▃▁▄▅▄▆▄▄▄▆▇▇▇▆▆▅▇▅█▄▆▄▇▆▆▆▅▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0110 | <= 1.2 levels | at frame 24 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.839 |  | mean 1.201 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1170 | <= 0.5 px (max <= 1) | max 0.421 px over 320 textured 64 px blocks; pan 14.5 px; worst [704, 832] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks05 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r07tp\seq_texpan_ks05`, 30 frames at 30 fps, kind texpan

- luma      `███▇▇▇▆▆▆▅▆▆▅▆▆▇▆▅▅▅▅▅▄▄▃▃▃▂▁▁`
- mean diff `▆▇▆▇▆▇▃█▄▄▅▅▅▆▄▃▆▃▅▆▂▄▂▁▄▃▂▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0110 | <= 1.2 levels | at frame 17 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.110 |  | mean 1.210 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1240 | <= 0.5 px (max <= 1) | max 0.352 px over 250 textured 64 px blocks; pan 14.5 px; worst [128, 384] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks08 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r07tp\seq_texpan_ks08`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▁▁▂▂▂▂▂▃▃▃▄▄▅▅▆▆▇▇▇▇▇▇▇▇▇███`
- mean diff `▂▄▂▄▄▃▁▂▃▄▁▄▄▄▄▅▆▄▄▇▇▅▆▅▇▆▆▇█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0140 | <= 1.2 levels | at frame 17 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.742 |  | mean 1.107 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.0850 | <= 0.5 px (max <= 1) | max 0.319 px over 196 textured 64 px blocks; pan 14.5 px; worst [384, 832] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks09 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r07tp\seq_texpan_ks09`, 30 frames at 30 fps, kind texpan

- luma      `▁▂▂▂▃▃▃▃▄▄▄▄▄▅▅▅▅▅▆▆▆▇▇▇▇█████`
- mean diff `▇▇▅█▆▅▇▅▄▇▅▅▇▃▄▆▄▆▂▆▄▄▁▅▁▃▃▃▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0060 | <= 1.2 levels | at frame 1 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.074 |  | mean 1.031 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | WARN | 0.0750 | <= 0.5 px (max <= 1) | max 3.806 px over 146 textured 64 px blocks; pan 14.5 px; worst [1024, 448] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks15 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r07tp\seq_texpan_ks15`, 30 frames at 30 fps, kind texpan

- luma      `▁▂▂▂▂▃▃▄▄▄▄▅▅▅▅▅▅▆▆▆▇▇▇▇▇▇▇███`
- mean diff `▁▂▃▃▂▅▂▅▁▃▄▄▁▆▆▃▃█▅▅▅▅▅▆▃▇▃█▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0050 | <= 1.2 levels | at frame 5 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.493 |  | mean 0.840 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | WARN | 0.1250 | <= 0.5 px (max <= 1) | max 1.137 px over 95 textured 64 px blocks; pan 14.5 px; worst [192, 960] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |

## texpan_ks17 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r07tp\seq_texpan_ks17`, 30 frames at 30 fps, kind texpan

- luma      `▂▁▃▁▃▂▃▃▃▄▄▄▄▄▄▅▅▅▅▅▅▆▆▇▇▇▇▇██`
- mean diff `█▄▄▇▃▅▄▄▅▄▄▄▃▃▃▃▃▃▂▃▂▂▂▂▂▁▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0170 | <= 1.2 levels | at frame 2 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 6.126 |  | mean 1.157 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1200 | <= 0.5 px (max <= 1) | max 0.446 px over 114 textured 64 px blocks; pan 14.5 px; worst [1280, 320] |
| shimmer vs -capture-ss 2 reference | SKIP | - |  | no --reference given |
