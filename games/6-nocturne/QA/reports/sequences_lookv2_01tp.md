# Frame sequences: lookv2_01tp

Sequences: 7. Checks: 35 PASS, 6 FAIL, 8 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| texpan_ks00 | texpan | 30 | 0.0000 | 1.265 | 1.272 | 195.2 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max) |
| texpan_ks04 | texpan | 30 | 0.0000 | 1.175 | 1.181 | 204.1 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max) |
| texpan_ks05 | texpan | 30 | 0.0001 | 1.286 | 1.298 | 210.2 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max) |
| texpan_ks08 | texpan | 30 | 0.0000 | 0.926 | 0.929 | 209.1 | FAIL | stabilised residual above the reference (32 px block max) |
| texpan_ks09 | texpan | 30 | 0.0000 | 0.673 | 0.682 | 140.3 | WARN |  |
| texpan_ks15 | texpan | 30 | 0.0000 | 0.501 | 0.502 | 153.4 | WARN |  |
| texpan_ks17 | texpan | 30 | 0.0001 | 1.014 | 1.044 | 194.5 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max), stabilised residual above the reference (32 px block max) |

## texpan_ks00 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01tp\seq_texpan_ks00`, 30 frames at 30 fps, kind texpan

- luma      `▁▂▁▂▂▃▃▃▃▃▄▄▅▄▆▅▆▆▆▆▆▇▆█▇█▇███`
- mean diff `▁▃▂▃▂▄▂▄▄▃▃▃▃▄▅▃▅▅▇▄▆▅▆▅▇▄▇▄█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0040 | <= 1.2 levels | at frame 14 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.876 |  | mean 0.855 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.3110 | <= 0.5 px (max <= 1) | max 0.390 px over 32 textured 64 px blocks; pan 14.5 px; worst [960, 192] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 1.3600 | % of blocks | frame mean delta 0.158 levels (ref - normal); worst block 22.98 levels at (1088, 256) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 2.7670 | <= 2 levels | block at (1120, 128) px; p99 0.495, mean -0.132 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | PASS | 1.1460 | <= 2 levels | block at (960, 320) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks04 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01tp\seq_texpan_ks04`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▁▁▂▂▃▃▃▃▄▅▄▅▅▅▅▆▆▇▆▆▆▇█▇█▇██`
- mean diff `▃▂▁▄▄▄▆▂▄▂▅▅▆▆▄▆▅▆▆▇▅▅▆█▇▅▇▆▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0110 | <= 1.2 levels | at frame 19 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.678 |  | mean 0.922 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1410 | <= 0.5 px (max <= 1) | max 0.421 px over 179 textured 64 px blocks; pan 14.5 px; worst [704, 832] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 10.5100 | % of blocks | frame mean delta -0.356 levels (ref - normal); worst block 35.07 levels at (1280, 64) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 7.2120 | <= 2 levels | block at (256, 320) px; p99 2.579, mean -0.411 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | PASS | 1.2510 | <= 2 levels | block at (1408, 32) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks05 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01tp\seq_texpan_ks05`, 30 frames at 30 fps, kind texpan

- luma      `███▇▇▇▇▇▆▆▆▆▆▅▅▅▅▅▄▄▄▄▃▃▃▃▂▂▁▁`
- mean diff `██▇▆▆▇▆▇▆▆▅█▅▇▄▄▅▆▆▄▄▄▄▄▃▃▃▁▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0140 | <= 1.2 levels | at frame 16 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.547 |  | mean 0.949 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1770 | <= 0.5 px (max <= 1) | max 0.364 px over 142 textured 64 px blocks; pan 14.5 px; worst [128, 384] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 12.6800 | % of blocks | frame mean delta -0.408 levels (ref - normal); worst block 33.30 levels at (1312, 32) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 6.7070 | <= 2 levels | block at (0, 768) px; p99 3.659, mean -0.631 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | PASS | 1.2860 | <= 2 levels | block at (1472, 96) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks08 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01tp\seq_texpan_ks08`, 30 frames at 30 fps, kind texpan

- luma      `▄▃▃▃▂▁▁▁▁▁▂▂▂▃▄▅▅▇███▇▆▇▆▅▅▇▆▆`
- mean diff `▅▄▂▃▇▂▁▂▂▂▃▄▅▄▄▅█▆▅▇█▇▅▇▇▇█▇▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0110 | <= 1.2 levels | at frame 17 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 1.967 |  | mean 0.805 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1150 | <= 0.5 px (max <= 1) | max 0.289 px over 120 textured 64 px blocks; pan 14.5 px; worst [1536, 640] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 8.2300 | % of blocks | frame mean delta -0.338 levels (ref - normal); worst block 8.33 levels at (1184, 800) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | PASS | 1.4720 | <= 2 levels | block at (1536, 32) px; p99 0.921, mean -0.792 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | FAIL | 2.1690 | <= 2 levels | block at (1600, 768) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks09 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01tp\seq_texpan_ks09`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▂▂▂▃▃▃▃▄▄▄▄▅▅▅▅▆▆▆▆▇▇▇▇▇████`
- mean diff `▇▇▄█▅▄▆▄▃▆▄▄▅▃▃▅▄▅▂▅▄▄▁▅▁▄▃▃▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0120 | <= 1.2 levels | at frame 1 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.003 |  | mean 0.790 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | WARN | 0.1010 | <= 0.5 px (max <= 1) | max 2.673 px over 76 textured 64 px blocks; pan 14.5 px; worst [1088, 448] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 4.6000 | % of blocks | frame mean delta -0.150 levels (ref - normal); worst block 7.73 levels at (1312, 64) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | PASS | 1.3700 | <= 2 levels | block at (1504, 320) px; p99 0.677, mean -0.506 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | PASS | 1.9800 | <= 2 levels | block at (480, 512) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks15 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01tp\seq_texpan_ks15`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▁▁▁▃▃▃▃▃▃▃▆▃▃▃▆▃▃▆█▆█▆█▆▆██▆`
- mean diff `▅▃▇▂▃▆▃▅▅▁▆▃▄▂▅▃▆▁▆▃█▂▆▅▇▄▃▁▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0020 | <= 1.2 levels | at frame 29 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 1.533 |  | mean 0.745 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1190 | <= 0.5 px (max <= 1) | max 0.141 px over 41 textured 64 px blocks; pan 14.5 px; worst [1024, 896] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 0.2500 | % of blocks | frame mean delta 0.113 levels (ref - normal); worst block 3.06 levels at (320, 800) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | PASS | 0.6380 | <= 2 levels | block at (1024, 896) px; p99 0.240, mean -0.077 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | PASS | 1.1740 | <= 2 levels | block at (992, 64) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks17 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01tp\seq_texpan_ks17`, 30 frames at 30 fps, kind texpan

- luma      `▂▁▂▁▂▂▃▃▃▃▃▃▄▄▄▅▅▅▅▆▆▆▆▇▇▇▇▇██`
- mean diff `█▄▅▇▃▅▅▄▅▃▄▄▃▃▃▃▃▃▃▃▂▂▂▂▂▁▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0190 | <= 1.2 levels | at frame 2 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 6.125 |  | mean 1.045 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.3730 | <= 0.5 px (max <= 1) | max 0.821 px over 56 textured 64 px blocks; pan 14.5 px; worst [768, 576] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 3.5400 | % of blocks | frame mean delta -0.058 levels (ref - normal); worst block 6.53 levels at (1024, 768) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 2.4200 | <= 2 levels | block at (96, 544) px; p99 1.054, mean -0.549 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | FAIL | 2.8820 | <= 2 levels | block at (832, 224) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |
