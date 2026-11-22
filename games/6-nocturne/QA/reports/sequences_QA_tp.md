# Frame sequences: QA_tp

Sequences: 7. Checks: 34 PASS, 8 FAIL, 7 WARN, 0 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| texpan_ks00 | texpan | 30 | 0.0001 | 1.215 | 1.224 | 214.1 | - / - | WARN |  |
| texpan_ks04 | texpan | 30 | 0.0001 | 1.440 | 1.450 | 216.9 | - / - | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max) |
| texpan_ks05 | texpan | 30 | 0.0001 | 1.479 | 1.495 | 209.0 | - / - | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max) |
| texpan_ks08 | texpan | 30 | 0.0000 | 1.292 | 1.299 | 212.3 | - / - | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max), stabilised residual above the reference (32 px block max) |
| texpan_ks09 | texpan | 30 | 0.0001 | 0.940 | 0.945 | 222.4 | - / - | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max), stabilised residual above the reference (32 px block max) |
| texpan_ks15 | texpan | 30 | 0.0000 | 1.894 | 1.899 | 172.9 | - / - | WARN |  |
| texpan_ks17 | texpan | 30 | 0.0001 | 1.103 | 1.136 | 194.5 | - / - | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max), stabilised residual above the reference (32 px block max) |

## texpan_ks00 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks00`, 30 frames at 30 fps, kind texpan

- luma      `▁▃▃▂▃▃▃▃▃▃▃▃▄▅▅▄▄▃▄▅▆▆▇▆▆▇██▇█`
- mean diff `▃▁▄▃▂▃▄▄▅▄▄▄▄▂▅▅▆▆▆▄▅▅▇▆█▅▆▇▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0160 | <= 1.2 levels | at frame 20 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 2.789 |  | mean 0.844 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.2470 | <= 0.5 px (max <= 1) | max 0.353 px over 34 textured 64 px blocks; pan 14.5 px; worst [640, 384] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 0.7600 | % of blocks | frame mean delta 0.173 levels (ref - normal); worst block 5.01 levels at (832, 192) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | PASS | 1.9120 | <= 2 levels | block at (608, 384) px; p99 0.607, mean -0.100 (14.10, TEXTURE_PLAN 10 R1); 1.2 % line blocks gated on the stabilised residual instead (D-037 14) |
| stabilised residual above the reference (32 px block max) | PASS | 1.5350 | <= 2 levels | block at (608, 384) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks00\montage_01.png`

Frames deleted after analysis: 30 (analysed.done in the folder)
## texpan_ks04 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks04`, 30 frames at 30 fps, kind texpan

- luma      `▅▃▃▁▂▂▃▁▁▂▂▅▃▅▅▇▇▇▇▇█▇▇▆▇▆▆▆▇▇`
- mean diff `█▃▄▅▅▅▆▁▁▄▁▂▅▄▄▄▂▆▆▂▂▁▄▇▅▃▃▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0150 | <= 1.2 levels | at frame 11 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 4.182 |  | mean 1.098 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1760 | <= 0.5 px (max <= 1) | max 0.347 px over 220 textured 64 px blocks; pan 14.5 px; worst [832, 192] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 12.9300 | % of blocks | frame mean delta 0.008 levels (ref - normal); worst block 35.10 levels at (1280, 64) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 6.0630 | <= 2 levels | block at (352, 32) px; p99 2.313, mean -0.154 (14.10, TEXTURE_PLAN 10 R1); 19.5 % line blocks gated on the stabilised residual instead (D-037 14) |
| stabilised residual above the reference (32 px block max) | PASS | 1.9410 | <= 2 levels | block at (800, 416) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks04\montage_01.png`

Frames deleted after analysis: 30 (analysed.done in the folder)
## texpan_ks05 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks05`, 30 frames at 30 fps, kind texpan

- luma      `███▇▇▇▇▆▆▆▅▆▅▅▅▅▅▅▄▄▄▄▄▄▃▃▂▂▁▁`
- mean diff `▇▅█▅█▅▅▆▆▆▅▇▅▆▆▁▃▆▆▄▄▃▃▄▄▂▃▁▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0190 | <= 1.2 levels | at frame 24 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.690 |  | mean 1.117 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.2020 | <= 0.5 px (max <= 1) | max 0.371 px over 202 textured 64 px blocks; pan 14.5 px; worst [128, 384] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 13.0300 | % of blocks | frame mean delta -0.067 levels (ref - normal); worst block 33.29 levels at (1312, 32) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 5.2550 | <= 2 levels | block at (224, 448) px; p99 2.906, mean -0.255 (14.10, TEXTURE_PLAN 10 R1); 20.2 % line blocks gated on the stabilised residual instead (D-037 14) |
| stabilised residual above the reference (32 px block max) | PASS | 1.7100 | <= 2 levels | block at (608, 832) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks05\montage_01.png`

Frames deleted after analysis: 30 (analysed.done in the folder)
## texpan_ks08 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks08`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▂▂▃▂▃▃▃▄▅▄▅▅▆▆▇████▇▇█▇▆▇███`
- mean diff `▁▂▁▂▄▂▂▃▃▄▃▄▄▅▅▆▅▅▇▆▇▇▆▆▇▇▇▇█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0120 | <= 1.2 levels | at frame 6 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.646 |  | mean 1.039 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1830 | <= 0.5 px (max <= 1) | max 0.529 px over 183 textured 64 px blocks; pan 14.5 px; worst [1024, 768] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 7.5800 | % of blocks | frame mean delta 0.039 levels (ref - normal); worst block 8.04 levels at (1184, 800) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 2.4310 | <= 2 levels | block at (1088, 224) px; p99 1.194, mean -0.300 (14.10, TEXTURE_PLAN 10 R1); 26.5 % line blocks gated on the stabilised residual instead (D-037 14) |
| stabilised residual above the reference (32 px block max) | FAIL | 2.3000 | <= 2 levels | block at (1056, 832) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks08\montage_01.png`

Frames deleted after analysis: 30 (analysed.done in the folder)
## texpan_ks09 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks09`, 30 frames at 30 fps, kind texpan

- luma      `▁▁▂▂▂▃▃▄▄▄▅▅▅▅▅▅▆▆▆▆▇▇▇▇██████`
- mean diff `▅▅▃█▄▅▇▅▄▆▆▅▅▆▃█▅▆▄▇▇▄▃█▁▇▅▅▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0150 | <= 1.2 levels | at frame 7 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 4.506 |  | mean 1.009 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1810 | <= 0.5 px (max <= 1) | max 0.373 px over 82 textured 64 px blocks; pan 14.5 px; worst [256, 64] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 5.7100 | % of blocks | frame mean delta 0.308 levels (ref - normal); worst block 8.23 levels at (1216, 512) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 2.0130 | <= 2 levels | block at (672, 640) px; p99 0.881, mean -0.138 (14.10, TEXTURE_PLAN 10 R1); 18.0 % line blocks gated on the stabilised residual instead (D-037 14) |
| stabilised residual above the reference (32 px block max) | FAIL | 2.5490 | <= 2 levels | block at (1088, 64) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks09\montage_01.png`

Frames deleted after analysis: 30 (analysed.done in the folder)
## texpan_ks15 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks15`, 30 frames at 30 fps, kind texpan

- luma      `▆▇▇▇▇████▇▇█▇▅▅▄▅▅▃▄▄▂▂▁▃▃▃▂▄▂`
- mean diff `▇▃█▃▆▆▆▄▇▄█▅▅▇▄▄▆▅▅▄▅▅▅▃▅▃█▁▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0050 | <= 1.2 levels | at frame 24 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.594 |  | mean 1.335 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1000 | <= 0.5 px (max <= 1) | max 0.253 px over 251 textured 64 px blocks; pan 14.5 px; worst [448, 64] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 2.8300 | % of blocks | frame mean delta 0.539 levels (ref - normal); worst block 4.78 levels at (1792, 544) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | PASS | 0.8650 | <= 2 levels | block at (1440, 448) px; p99 0.489, mean 0.037 (14.10, TEXTURE_PLAN 10 R1); 20.9 % line blocks gated on the stabilised residual instead (D-037 14) |
| stabilised residual above the reference (32 px block max) | PASS | 1.8640 | <= 2 levels | block at (1696, 544) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks15\montage_01.png`

Frames deleted after analysis: 30 (analysed.done in the folder)
## texpan_ks17 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks17`, 30 frames at 30 fps, kind texpan

- luma      `█▆▇▅▆▅▅▅▅▅▅▄▄▄▃▄▄▄▃▃▂▂▂▃▂▂▁▁▁▁`
- mean diff `█▃▅▆▄▅▄▄▄▄▃▄▃▃▃▃▃▂▂▂▂▃▂▂▂▂▁▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0140 | <= 1.2 levels | at frame 1 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 6.219 |  | mean 1.030 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1640 | <= 0.5 px (max <= 1) | max 0.455 px over 84 textured 64 px blocks; pan 14.5 px; worst [768, 448] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 3.6900 | % of blocks | frame mean delta -0.090 levels (ref - normal); worst block 6.64 levels at (1024, 736) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 2.4370 | <= 2 levels | block at (96, 544) px; p99 1.025, mean -0.603 (14.10, TEXTURE_PLAN 10 R1); 13.4 % line blocks gated on the stabilised residual instead (D-037 14) |
| stabilised residual above the reference (32 px block max) | FAIL | 2.7380 | <= 2 levels | block at (832, 224) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_tp\seq_texpan_ks17\montage_01.png`

Frames deleted after analysis: 30 (analysed.done in the folder)