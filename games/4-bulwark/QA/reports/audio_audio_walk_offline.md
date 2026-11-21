# Audio analysis: audio_walk_offline

Files: 2. Checks: 18 PASS, 7 FAIL, 0 SKIP, 4 INFO. Scene constants: BulwarkConst.cs.

**Result: FAIL**

## RMS per beat (mix files, 14.3.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | WaveGallery | -31.83 | -34.0 | +2.17 | 128.4 s | FAIL |
| 2 | Crest | -24.0 | -24.0 | -0.00 | 320.0 s | PASS |
| 3 | SpillwayBridge | -18.1 | -18.0 | -0.10 | 298.7 s | PASS |
| 4 | ArmourSlope | -23.1 | -23.0 | -0.10 | 234.7 s | PASS |
| 5 | GateTower | -32.15 | -32.0 | -0.15 | 160.0 s | PASS |
| 6 | WindowGallery | -38.12 | -38.0 | -0.12 | 320.0 s | PASS |
| 7 | SluiceLedge | -26.18 | -25.0 | -1.18 | 56.5 s | PASS |

Integrated loudness over all mix files: -21.38 LUFS (target about -30, INFO).

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| walk_1p5.wav | mix | float32 48000 Hz x2 | 1018.2 | -3.70 | 0.000000 | 0.0570 | -23.39 | -21.39 | PASS |
| walk_2p5.wav | mix | float32 48000 Hz x2 | 618.6 | -3.18 | 0.000000 | 0.0546 | -23.44 | -21.37 | PASS |

### walk_1p5.wav (PASS)

RMS per 1 s: `▃▅▆▅▆▆▆▆▆▆▆▆▆▆▆▆▇██████████▇▆▆▆▆▆▆▆▆▆▄▄▄▃▅▁▁▁▁▁▁▁▁▁▂▁▁▅▆▅▃▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -3.6700 | <= -1 dBTP | 4x oversampled, at 327.724 s |
| sample peak | INFO | -3.7000 |  | at 327.724 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0570 | <= 0.25 FS | at 554.4510 s, 0 over (footsteps excluded) |
| not silent | PASS | -23.39 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -21.39 |  | BS.1770-4, reported only |

### walk_2p5.wav (PASS)

RMS per 1 s: `█████████████████████████████████████████▇▇▇▇▇▇▇▇▇▇▇▇████▇▇▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -3.1700 | <= -1 dBTP | 4x oversampled, at 214.046 s |
| sample peak | INFO | -3.1800 |  | at 214.046 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0546 | <= 0.25 FS | at 287.4873 s, 0 over (footsteps excluded) |
| not silent | PASS | -23.44 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -21.37 |  | BS.1770-4, reported only |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
