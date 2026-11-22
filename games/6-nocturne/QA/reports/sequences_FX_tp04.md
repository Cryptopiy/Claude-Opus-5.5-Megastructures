# Frame sequences: FX_tp04

Sequences: 5. Checks: 23 PASS, 7 FAIL, 5 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| texpan_ks00 | texpan | 30 | 0.0000 | 1.266 | 1.273 | 195.2 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max) |
| texpan_ks04 | texpan | 30 | 0.0000 | 1.429 | 1.437 | 216.3 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max) |
| texpan_ks05 | texpan | 30 | 0.0001 | 1.463 | 1.477 | 209.2 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max) |
| texpan_ks08 | texpan | 30 | 0.0001 | 1.211 | 1.219 | 209.2 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max), stabilised residual above the reference (32 px block max) |
| texpan_ks17 | texpan | 30 | 0.0000 | 1.139 | 1.170 | 194.5 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max), stabilised residual above the reference (32 px block max) |

## texpan_ks00 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_tp04\seq_texpan_ks00`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▁▂▂▃▃▃▃▃▄▃▄▄▅▅▆▆▆▆▆▆▆▇▇█▇▇▇█`
- mean diff `▁▃▂▄▂▄▂▄▄▃▃▄▄▄▅▄▅▅▇▅▆▆▆▅▇▄▇▄█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0040 | <= 1.2 levels | at frame 14 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.868 |  | mean 0.854 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.3000 | <= 0.5 px (max <= 1) | max 0.390 px over 32 textured 64 px blocks; pan 14.5 px; worst [960, 192] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 1.4600 | % of blocks | frame mean delta 0.160 levels (ref - normal); worst block 22.98 levels at (1088, 256) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 2.7600 | <= 2 levels | block at (1120, 128) px; p99 0.492, mean -0.133 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | PASS | 1.1460 | <= 2 levels | block at (960, 320) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks04 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_tp04\seq_texpan_ks04`, 30 frames at 30 fps, kind texpan

- luma      `▄▃▂▁▄▂▄▃▂▂▂▅▂▄▄▆▅▇▆█▇▅▆▅▅▃▃▃▄▄`
- mean diff `█▅▄█▆▆▅▃▃▄▃▃▅▆▂▄▅▄▇▃▃▁▄▆▅▃▃▃▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0130 | <= 1.2 levels | at frame 12 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.594 |  | mean 1.075 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1480 | <= 0.5 px (max <= 1) | max 0.585 px over 232 textured 64 px blocks; pan 14.5 px; worst [768, 64] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 11.4100 | % of blocks | frame mean delta -0.288 levels (ref - normal); worst block 35.09 levels at (1280, 64) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 6.9540 | <= 2 levels | block at (1216, 416) px; p99 3.022, mean -0.151 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | PASS | 1.7850 | <= 2 levels | block at (1824, 864) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks05 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_tp04\seq_texpan_ks05`, 30 frames at 30 fps, kind texpan

- luma      `███▇▇▇▇▆▆▆▅▆▅▆▅▅▅▄▄▄▄▄▄▃▃▃▂▂▂▁`
- mean diff `█▆▇▇▇▇▅▆▆▇▆▇▆▅█▁▅▇▆▄▄▄▄▅▅▃▄▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0130 | <= 1.2 levels | at frame 16 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.291 |  | mean 1.084 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.2060 | <= 0.5 px (max <= 1) | max 0.585 px over 212 textured 64 px blocks; pan 14.5 px; worst [768, 256] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 12.5300 | % of blocks | frame mean delta -0.369 levels (ref - normal); worst block 33.29 levels at (1312, 32) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 6.5250 | <= 2 levels | block at (160, 512) px; p99 3.731, mean -0.253 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | PASS | 1.2860 | <= 2 levels | block at (1472, 96) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks08 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_tp04\seq_texpan_ks08`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▁▂▂▁▂▂▂▂▃▃▃▄▅▅▆▇▇▇█▇▇▇▇▇▇███`
- mean diff `▂▁▁▁▃▂▁▃▂▄▂▄▄▅▅▄▇▆▆▇▆▇▆▇▇██▇█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0130 | <= 1.2 levels | at frame 17 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.225 |  | mean 0.970 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1620 | <= 0.5 px (max <= 1) | max 0.631 px over 160 textured 64 px blocks; pan 14.5 px; worst [1024, 768] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 6.7200 | % of blocks | frame mean delta -0.222 levels (ref - normal); worst block 10.82 levels at (1184, 736) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 6.9140 | <= 2 levels | block at (1376, 800) px; p99 2.899, mean -0.345 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | FAIL | 2.1260 | <= 2 levels | block at (1600, 768) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |

## texpan_ks17 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_tp04\seq_texpan_ks17`, 30 frames at 30 fps, kind texpan

- luma      `█▇█▇▇▇▇▆▆▆▆▅▅▅▅▅▅▅▄▄▃▃▃▃▂▂▂▁▁▁`
- mean diff `█▃▅▆▄▅▄▄▄▄▃▄▃▃▃▃▃▂▃▂▂▃▁▂▂▁▁▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0110 | <= 1.2 levels | at frame 1 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 6.477 |  | mean 1.057 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.3430 | <= 0.5 px (max <= 1) | max 0.634 px over 100 textured 64 px blocks; pan 14.5 px; worst [768, 576] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 3.8900 | % of blocks | frame mean delta -0.106 levels (ref - normal); worst block 7.08 levels at (1024, 736) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 2.4360 | <= 2 levels | block at (96, 544) px; p99 1.144, mean -0.620 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | FAIL | 3.0620 | <= 2 levels | block at (832, 224) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |
