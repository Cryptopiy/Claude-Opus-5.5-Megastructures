# Audio analysis: wa-gray

Files: 1. Checks: 5 PASS, 2 FAIL, 6 SKIP, 2 INFO. Scene constants: BulwarkConst.cs.

**Result: FAIL**

## RMS per beat (mix files, 14.3.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | WaveGallery | -39.84 | -34.0 | -5.84 | 30.0 s | FAIL |
| 2 | Crest | - | -24.0 | - | - | SKIP |
| 3 | SpillwayBridge | - | -18.0 | - | - | SKIP |
| 4 | ArmourSlope | - | -23.0 | - | - | SKIP |
| 5 | GateTower | - | -32.0 | - | - | SKIP |
| 6 | WindowGallery | - | -38.0 | - | - | SKIP |
| 7 | SluiceLedge | - | -25.0 | - | - | SKIP |

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| wa-gray-dump.wav | mix | float32 48000 Hz x2 | 30.0 | -13.56 | 0.000003 | 0.0062 | -39.84 | -41.38 | PASS |

### wa-gray-dump.wav (PASS)

RMS per 1 s: `▁██▇██████████████████████████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -13.5600 | <= -1 dBTP | 4x oversampled, at 1.599 s |
| sample peak | INFO | -13.5600 |  | at 1.599 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0062 | <= 0.25 FS | at 17.4667 s, 0 over (footsteps excluded) |
| not silent | PASS | -39.84 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -41.38 |  | BS.1770-4, reported only |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
