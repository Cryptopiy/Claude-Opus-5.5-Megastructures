# Audio analysis: audio_dumps06

Files: 3. Checks: 28 PASS, 9 FAIL, 5 SKIP, 6 INFO. Scene constants: BulwarkConst.cs.

**Result: FAIL**

## RMS per beat (mix files, 14.3.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | WaveGallery | - | -34.0 | - | - | SKIP |
| 2 | Crest | - | -24.0 | - | - | SKIP |
| 3 | SpillwayBridge | - | -18.0 | - | - | SKIP |
| 4 | ArmourSlope | -23.41 | -23.0 | -0.41 | 140.0 s | PASS |
| 5 | GateTower | - | -32.0 | - | - | SKIP |
| 6 | WindowGallery | -30.17 | -38.0 | +7.83 | 140.0 s | FAIL |
| 7 | SluiceLedge | - | -25.0 | - | - | SKIP |

Integrated loudness over all mix files: -24.34 LUFS (target about -30, INFO).

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| dump_kraken_s760.wav | mix | float32 48000 Hz x2 | 140.0 | -4.29 | 0.000005 | 0.0660 | -23.41 | -22.35 | FAIL |
| dump_kraken_s1230.wav | mix | float32 48000 Hz x2 | 140.0 | -5.94 | 0.000002 | 0.0146 | -30.17 | -27.28 | PASS |
| dump_ride.wav | mix | float32 48000 Hz x2 | 150.0 | -16.48 | 0.000000 | 0.0698 | -32.26 | -28.99 | PASS |

### dump_kraken_s760.wav (FAIL)

RMS per 1 s: `▁▄▆▇▆▆▆▆▆▇▇▇▇▆▇█▅▆▆▆▇▆▇█▆▆▆▆▆▅▆▆▆▇▆▆▅▆▆▆▆▆▅▂▆▆▆▆▆▆▆▆▆▆▇▆▆▆▆▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -4.2900 | <= -1 dBTP | 4x oversampled, at 91.744 s |
| sample peak | INFO | -4.2900 |  | at 91.744 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0660 | <= 0.25 FS | at 6.9216 s, 0 over (footsteps excluded) |
| not silent | PASS | -23.41 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -22.35 |  | BS.1770-4, reported only |
| kraken Call at 2.8 s audible | PASS | 16.4300 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -44.0, during -27.6 dBFS |
| kraken SurfaceRoar at 20.6 s audible | PASS | 3.6800 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -30.0, during -26.3 dBFS |
| kraken Slap at 35.4 s audible | PASS | 4.2500 | >= +3 dB (80-800 Hz, 1 s after vs 3 s before) | before -30.0, during -25.8 dBFS |
| kraken DamShudder at 50.8 s audible | FAIL | 2.2100 | >= +3 dB (80-800 Hz, 2 s after vs 3 s before) | before -29.0, during -26.7 dBFS |
| kraken GlassCreak at 66.8 s audible | FAIL | 1.3300 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -29.1, during -27.8 dBFS |
| kraken SuckerPops at 82.8 s audible | FAIL | -2.2000 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -28.8, during -31.0 dBFS |
| kraken EyeSwell at 98.8 s audible | FAIL | -0.8500 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -29.2, during -30.1 dBFS |
| kraken Stinger at 114.9 s audible | FAIL | 2.3400 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -30.2, during -27.9 dBFS |

### dump_kraken_s1230.wav (PASS)

RMS per 1 s: `▁▇███▇▇▇▇████▇▇█▇▇▇▇▇███▇▇▇▇▇███▇▇▇▇▇███▇▇▇▇████▇▇▇▇▇▇▇▇▇▇▇▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -5.9400 | <= -1 dBTP | 4x oversampled, at 35.565 s |
| sample peak | INFO | -5.9400 |  | at 35.565 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0146 | <= 0.25 FS | at 105.3609 s, 0 over (footsteps excluded) |
| not silent | PASS | -30.17 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -27.28 |  | BS.1770-4, reported only |
| kraken Call at 3.3 s audible | PASS | 207.0200 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -240.0, during -33.0 dBFS |
| kraken SurfaceRoar at 20.7 s audible | PASS | 23.1900 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -53.0, during -29.8 dBFS |
| kraken Slap at 35.5 s audible | PASS | 29.8600 | >= +3 dB (80-800 Hz, 1 s after vs 3 s before) | before -53.6, during -23.8 dBFS |
| kraken DamShudder at 51.0 s audible | PASS | 25.5600 | >= +3 dB (80-800 Hz, 2 s after vs 3 s before) | before -52.8, during -27.2 dBFS |
| kraken GlassCreak at 67.0 s audible | PASS | 21.5300 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -53.9, during -32.4 dBFS |
| kraken SuckerPops at 83.0 s audible | PASS | 12.2700 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -52.1, during -39.8 dBFS |
| kraken EyeSwell at 99.0 s audible | PASS | 23.4200 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -53.8, during -30.4 dBFS |
| kraken Stinger at 115.0 s audible | PASS | 7.5100 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -49.8, during -42.3 dBFS |

### dump_ride.wav (PASS)

RMS per 1 s: `▁█████████████████████████████████▇▇████████████████▇▇███▇▇▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -16.4300 | <= -1 dBTP | 4x oversampled, at 112.959 s |
| sample peak | INFO | -16.4800 |  | at 112.959 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0698 | <= 0.25 FS | at 126.7880 s, 0 over (footsteps excluded) |
| not silent | PASS | -32.26 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -28.99 |  | BS.1770-4, reported only |
| kraken Stinger at 135.1 s audible | PASS | 9.0000 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -51.8, during -42.8 dBFS |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
