# Audio analysis: audio_beds_offline

Files: 2. Checks: 21 PASS, 4 FAIL, 0 SKIP, 4 INFO. Scene constants: BulwarkConst.cs.

**Result: FAIL**

## RMS per beat (mix files, 14.3.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | WaveGallery | -34.02 | -34.0 | -0.02 | 128.4 s | PASS |
| 2 | Crest | -24.22 | -24.0 | -0.22 | 320.0 s | PASS |
| 3 | SpillwayBridge | -18.11 | -18.0 | -0.11 | 298.7 s | PASS |
| 4 | ArmourSlope | -23.23 | -23.0 | -0.23 | 234.7 s | PASS |
| 5 | GateTower | -32.91 | -32.0 | -0.91 | 160.0 s | PASS |
| 6 | WindowGallery | -38.31 | -38.0 | -0.31 | 320.0 s | PASS |
| 7 | SluiceLedge | -26.19 | -25.0 | -1.19 | 56.5 s | PASS |

Integrated loudness over all mix files: -21.41 LUFS (target about -30, INFO).

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| beds_1p5.wav | mix | float32 48000 Hz x2 | 1018.2 | -3.73 | 0.000000 | 0.0570 | -23.48 | -21.42 | PASS |
| beds_2p5.wav | mix | float32 48000 Hz x2 | 618.6 | -3.18 | 0.000000 | 0.0547 | -23.54 | -21.40 | PASS |

### beds_1p5.wav (PASS)

RMS per 1 s: `▁▁▁▁▆▆▆▆▆▆▆▆▆▆▆▆▇██████████▇▆▆▆▆▆▆▆▆▆▃▃▃▃▃▁▁▁▁▁▁▁▁▁▁▁▁▆▆▅▃▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -3.6900 | <= -1 dBTP | 4x oversampled, at 327.724 s |
| sample peak | INFO | -3.7300 |  | at 327.724 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0570 | <= 0.25 FS | at 554.4510 s, 0 over (footsteps excluded) |
| not silent | PASS | -23.48 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -21.42 |  | BS.1770-4, reported only |

### beds_2p5.wav (PASS)

RMS per 1 s: `▇▇▇▇█████████████████████████████████████▇▇▇▇▇▇▇▇▇▇▇▇████▇▇▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -3.1700 | <= -1 dBTP | 4x oversampled, at 214.046 s |
| sample peak | INFO | -3.1800 |  | at 214.046 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0547 | <= 0.25 FS | at 287.4873 s, 0 over (footsteps excluded) |
| not silent | PASS | -23.54 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -21.40 |  | BS.1770-4, reported only |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
