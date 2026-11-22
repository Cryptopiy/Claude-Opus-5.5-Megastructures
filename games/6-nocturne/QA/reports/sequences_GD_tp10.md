# Frame sequences: GD_tp10

Sequences: 1. Checks: 4 PASS, 2 FAIL, 1 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| texpan_ks15 | texpan | 30 | 0.0000 | 1.876 | 1.882 | 175.3 | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max), stabilised residual above the reference (32 px block max) |

## texpan_ks15 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_tp10\seq_texpan_ks15`, 30 frames at 30 fps, kind texpan

- luma      `████▇█▇▇▇▆▆▆▆▅▅▄▄▄▃▃▃▂▂▁▁▂▁▁▁▁`
- mean diff `▆▆▅▅▆▆▇▃█▄▆▆▄▇▆▁█▃█▁▇▂▆▂▆▂▆▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0050 | <= 1.2 levels | at frame 26 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 3.903 |  | mean 1.349 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1340 | <= 0.5 px (max <= 1) | max 0.262 px over 247 textured 64 px blocks; pan 14.5 px; worst [320, 512] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 2.7800 | % of blocks | frame mean delta 0.527 levels (ref - normal); worst block 4.58 levels at (1824, 512) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 5.5540 | <= 2 levels | block at (320, 640) px; p99 2.277, mean 0.086 (14.10, TEXTURE_PLAN 10 R1) |
| stabilised residual above the reference (32 px block max) | FAIL | 3.6370 | <= 2 levels | block at (320, 640) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |
