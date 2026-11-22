# Frame sequences: GD_tp09

Sequences: 1. Checks: 4 PASS, 2 FAIL, 1 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| texpan_ks15 | texpan | 30 | 0.0000 | 1.887 | 1.892 | 166.0 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max), stabilised residual above the reference (32 px block max) |

## texpan_ks15 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_tp09\seq_texpan_ks15`, 30 frames at 30 fps, kind texpan

- luma      `███████▇▇▇▇▇▆▅▅▄▅▄▃▄▃▂▂▁▂▂▂▁▂▁`
- mean diff `▇▄█▅▆▇▆▅▇▅█▅▅▇▅▃▆▄▆▃▅▄▄▄▅▃▆▁▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0060 | <= 1.2 levels | at frame 18 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.584 |  | mean 1.325 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1200 | <= 0.5 px (max <= 1) | max 0.215 px over 247 textured 64 px blocks; pan 14.5 px; worst [448, 64] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 0.9100 | % of blocks | frame mean delta 0.444 levels (ref - normal); worst block 3.49 levels at (640, 544) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 5.3200 | <= 2 levels | block at (320, 640) px; p99 2.589, mean 0.115 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | FAIL | 2.7360 | <= 2 levels | block at (320, 640) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |
