# Audio analysis: audio_dumps05

Files: 2. Checks: 14 PASS, 7 FAIL, 6 SKIP, 4 INFO. Scene constants: BulwarkConst.cs.

**Result: FAIL**

## RMS per beat (mix files, 14.3.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | WaveGallery | - | -34.0 | - | - | SKIP |
| 2 | Crest | - | -24.0 | - | - | SKIP |
| 3 | SpillwayBridge | - | -18.0 | - | - | SKIP |
| 4 | ArmourSlope | -23.83 | -23.0 | -0.83 | 140.0 s | PASS |
| 5 | GateTower | - | -32.0 | - | - | SKIP |
| 6 | WindowGallery | - | -38.0 | - | - | SKIP |
| 7 | SluiceLedge | - | -25.0 | - | - | SKIP |

Integrated loudness over all mix files: -24.43 LUFS (target about -30, INFO).

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| dump_kraken.wav | mix | float32 48000 Hz x2 | 140.0 | -4.68 | 0.000002 | 0.0788 | -23.83 | -22.75 | FAIL |
| dump_ride.wav | mix | float32 48000 Hz x2 | 150.0 | -16.73 | 0.000000 | 0.0528 | -32.34 | -28.81 | PASS |

### dump_kraken.wav (FAIL)

RMS per 1 s: `▁███████████████████████████████████████████████████████████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -4.6800 | <= -1 dBTP | 4x oversampled, at 34.796 s |
| sample peak | INFO | -4.6800 |  | at 34.796 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0788 | <= 0.25 FS | at 110.1227 s, 0 over (footsteps excluded) |
| not silent | PASS | -23.83 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -22.75 |  | BS.1770-4, reported only |
| kraken Call at 2.5 s audible | PASS | 208.0700 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -240.0, during -31.9 dBFS |
| kraken SurfaceRoar at 20.0 s audible | PASS | 3.5500 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -29.9, during -26.3 dBFS |
| kraken Slap at 34.8 s audible | PASS | 9.0400 | >= +3 dB (80-800 Hz, 1 s after vs 3 s before) | before -34.7, during -25.6 dBFS |
| kraken DamShudder at 50.2 s audible | FAIL | 2.3200 | >= +3 dB (80-800 Hz, 2 s after vs 3 s before) | before -29.0, during -26.7 dBFS |
| kraken GlassCreak at 66.2 s audible | FAIL | 1.0600 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -28.9, during -27.8 dBFS |
| kraken SuckerPops at 82.2 s audible | FAIL | -2.2300 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -28.8, during -31.0 dBFS |
| kraken EyeSwell at 98.2 s audible | FAIL | -1.0500 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -29.5, during -30.6 dBFS |
| kraken Stinger at 113.8 s audible | FAIL | 1.9500 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -30.3, during -28.3 dBFS |

### dump_ride.wav (PASS)

RMS per 1 s: `▁█████████████████████████████████▇▇███████████████▇▇▇▇▇▇▇▇▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -16.7200 | <= -1 dBTP | 4x oversampled, at 106.842 s |
| sample peak | INFO | -16.7300 |  | at 106.842 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0528 | <= 0.25 FS | at 126.4910 s, 0 over (footsteps excluded) |
| not silent | PASS | -32.34 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -28.81 |  | BS.1770-4, reported only |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
