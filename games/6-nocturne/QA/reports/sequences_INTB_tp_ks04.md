# Frame sequences: INTB_tp_ks04

Sequences: 1. Checks: 5 PASS, 1 FAIL, 1 WARN, 0 SKIP.

**Result: FAIL**

(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Pops / blinks | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|---|
| texpan_ks04 | texpan | 30 | 0.0001 | 1.622 | 1.632 | 214.5 | - / - | FAIL | shimmer: temporal std above the -capture-ss 2 reference (32 px block max) |

## texpan_ks04 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_tp_ss1\seq_texpan_ks04`, 30 frames at 30 fps, kind texpan

- luma      `▁▃▃▄▄▅▅▄▄▄▄▃▄▅▅▆▇█▆▅▅▄▄▃▄▃▄▂▂▁`
- mean diff `▃▇▄▆▁▆▂▂▄▄▁▃▂▃▄█▂▃▃▂▄▁▄▄▂▃▁▁▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0140 | <= 1.2 levels | at frame 18 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| texpan run conditions | PASS | rain 0, freezeClock True | -rain 0 -freeze-clock | the pan must be the only change |
| stabilised residual std (32 px block p99) | INFO | 4.047 |  | mean 1.140 levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run) |
| swimming (block shift first vs last, p95) | PASS | 0.1780 | <= 0.5 px (max <= 1) | max 0.327 px over 260 textured 64 px blocks; pan 14.5 px; worst [576, 384] |
| picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %) | WARN | 14.2900 | % of blocks | frame mean delta -0.037 levels (ref - normal); worst block 23.62 levels at (1280, 256) px: a footprint regime that switches with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates |
| shimmer: temporal std above the -capture-ss 2 reference (32 px block max) | FAIL | 6.7590 | <= 2 levels | block at (224, 480) px; p99 2.340, mean -0.246 (14.10, TEXTURE_PLAN 10 R1); 22.3 % line blocks gated on the stabilised residual instead (D-037 14) |
| stabilised residual above the reference (32 px block max) | PASS | 1.8500 | <= 2 levels | block at (608, 480) px: the shimmer of world-locked content (the screen-space std above also carries the pan's own motion, which masks it) |


Montage: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_tp_ss1\seq_texpan_ks04\montage_01.png`