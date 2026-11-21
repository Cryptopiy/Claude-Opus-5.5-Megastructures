# Audio analysis: x2_audio_dumps

Files: 4. Checks: 36 PASS, 9 FAIL, 2 SKIP, 32 INFO. Scene constants: BulwarkConst.cs.

**Result: FAIL**

## RMS per beat (mix files, 14.3.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | WaveGallery | - | -34.0 | - | - | SKIP |
| 2 | Crest | -21.49 | -24.0 | +2.51 | 61.6 s | FAIL |
| 3 | SpillwayBridge | -19.73 | -18.0 | -1.73 | 8.4 s | FAIL |
| 4 | ArmourSlope | -21.26 | -23.0 | +1.74 | 75.0 s | FAIL |
| 5 | GateTower | - | -32.0 | - | - | SKIP |
| 6 | WindowGallery | -29.42 | -38.0 | +8.58 | 42.4 s | FAIL |
| 7 | SluiceLedge | -40.56 | -25.0 | -15.56 | 2.6 s | FAIL |

Integrated loudness over all mix files: -20.36 LUFS (target about -30, INFO).

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| x2_k2.wav | mix | float32 48000 Hz x2 | 70.0 | -2.57 | 0.000005 | 0.1566 | -21.23 | -18.38 | PASS |
| x2_k4.wav | mix | float32 48000 Hz x2 | 75.0 | -2.48 | 0.000005 | 0.1171 | -21.26 | -18.32 | PASS |
| x2_strike.wav | mix | float32 48000 Hz x2 | 45.0 | -3.09 | 0.000009 | 0.2222 | -29.66 | -23.30 | PASS |
| x2_ending.wav | mix | float32 48000 Hz x2 | 200.0 | -13.74 | 0.000000 | 0.0649 | -34.52 | -30.76 | PASS |

### x2_k2.wav (PASS)

RMS per 1 s: `▁▁▁▁▁▁██████████████████████████████████████████████████████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -2.5700 | <= -1 dBTP | 4x oversampled, at 19.119 s |
| sample peak | INFO | -2.5700 |  | at 19.119 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.1566 | <= 0.25 FS | at 17.0461 s, 0 over (footsteps excluded) |
| not silent | PASS | -21.23 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -18.38 |  | BS.1770-4, reported only |
| kraken Call at 14.0 s 80-800 Hz | PASS | 10.7500 | >= +8 dB (loudest 1 s vs the 3 s before) | before -27.6, loudest -16.8 dBFS |
| kraken Call at 14.0 s HP 250 (laptop) | PASS | 12.5400 | >= +6 dB (loudest 1 s vs the 3 s before) | before -29.5, loudest -17.0 dBFS |
| kraken Call at 14.0 s HP 400 (phone) | PASS | 13.5400 | >= +6 dB (loudest 1 s vs the 3 s before) | before -32.1, loudest -18.6 dBFS |
| kraken SurfaceRoar at 17.5 s 80-800 Hz | INFO | 4.2200 | >= +8 dB (loudest 1 s vs the 3 s before) | before -21.0, loudest -16.8 dBFS; continuation of Call at 14.0 s |
| kraken SurfaceRoar at 17.5 s HP 250 (laptop) | INFO | 4.4300 | >= +6 dB (loudest 1 s vs the 3 s before) | before -21.4, loudest -17.0 dBFS; continuation of Call at 14.0 s |
| kraken SurfaceRoar at 17.5 s HP 400 (phone) | INFO | 4.3800 | >= +6 dB (loudest 1 s vs the 3 s before) | before -23.0, loudest -18.6 dBFS; continuation of Call at 14.0 s |
| kraken SurfaceRoar at 52.5 s 80-800 Hz | PASS | 10.4100 | >= +8 dB (loudest 1 s vs the 3 s before) | before -29.1, loudest -18.7 dBFS |
| kraken SurfaceRoar at 52.5 s HP 250 (laptop) | PASS | 11.7500 | >= +6 dB (loudest 1 s vs the 3 s before) | before -30.0, loudest -18.2 dBFS |
| kraken SurfaceRoar at 52.5 s HP 400 (phone) | PASS | 12.0200 | >= +6 dB (loudest 1 s vs the 3 s before) | before -31.6, loudest -19.6 dBFS |

### x2_k4.wav (PASS)

RMS per 1 s: `▁▁▁▁▁▁▁█▇███████████████████████████████████████████████████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -2.4700 | <= -1 dBTP | 4x oversampled, at 18.319 s |
| sample peak | INFO | -2.4800 |  | at 18.319 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.1171 | <= 0.25 FS | at 17.6417 s, 0 over (footsteps excluded) |
| not silent | PASS | -21.26 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -18.32 |  | BS.1770-4, reported only |
| kraken Call at 13.6 s 80-800 Hz | PASS | 9.7000 | >= +8 dB (loudest 1 s vs the 3 s before) | before -25.6, loudest -15.9 dBFS |
| kraken Call at 13.6 s HP 250 (laptop) | PASS | 11.1100 | >= +6 dB (loudest 1 s vs the 3 s before) | before -27.4, loudest -16.3 dBFS |
| kraken Call at 13.6 s HP 400 (phone) | PASS | 11.9000 | >= +6 dB (loudest 1 s vs the 3 s before) | before -30.1, loudest -18.2 dBFS |
| kraken SurfaceRoar at 18.6 s 80-800 Hz | INFO | 3.2300 | >= +8 dB (loudest 1 s vs the 3 s before) | before -19.1, loudest -15.9 dBFS; continuation of Call at 13.6 s |
| kraken SurfaceRoar at 18.6 s HP 250 (laptop) | INFO | 3.4300 | >= +6 dB (loudest 1 s vs the 3 s before) | before -19.7, loudest -16.3 dBFS; continuation of Call at 13.6 s |
| kraken SurfaceRoar at 18.6 s HP 400 (phone) | INFO | 3.5300 | >= +6 dB (loudest 1 s vs the 3 s before) | before -21.6, loudest -18.1 dBFS; continuation of Call at 13.6 s |
| kraken SurfaceRoar at 39.1 s 80-800 Hz | INFO | 9.2400 | >= +8 dB (loudest 1 s vs the 3 s before) | before -27.2, loudest -18.0 dBFS; continuation of SurfaceRoar at 18.6 s |
| kraken SurfaceRoar at 39.1 s HP 250 (laptop) | INFO | 9.0900 | >= +6 dB (loudest 1 s vs the 3 s before) | before -27.2, loudest -18.1 dBFS; continuation of SurfaceRoar at 18.6 s |
| kraken SurfaceRoar at 39.1 s HP 400 (phone) | INFO | 8.9000 | >= +6 dB (loudest 1 s vs the 3 s before) | before -28.5, loudest -19.6 dBFS; continuation of SurfaceRoar at 18.6 s |
| kraken Call at 40.8 s 80-800 Hz | INFO | 4.7800 | >= +8 dB (loudest 1 s vs the 3 s before) | before -22.7, loudest -17.9 dBFS; continuation of SurfaceRoar at 39.1 s |
| kraken Call at 40.8 s HP 250 (laptop) | INFO | 4.3400 | >= +6 dB (loudest 1 s vs the 3 s before) | before -22.4, loudest -18.0 dBFS; continuation of SurfaceRoar at 39.1 s |
| kraken Call at 40.8 s HP 400 (phone) | INFO | 4.1300 | >= +6 dB (loudest 1 s vs the 3 s before) | before -23.7, loudest -19.5 dBFS; continuation of SurfaceRoar at 39.1 s |
| kraken SurfaceRoar at 40.8 s 80-800 Hz | INFO | 4.6300 | >= +8 dB (loudest 1 s vs the 3 s before) | before -22.6, loudest -18.0 dBFS; continuation of Call at 40.8 s |
| kraken SurfaceRoar at 40.8 s HP 250 (laptop) | INFO | 4.1700 | >= +6 dB (loudest 1 s vs the 3 s before) | before -22.2, loudest -18.1 dBFS; continuation of Call at 40.8 s |
| kraken SurfaceRoar at 40.8 s HP 400 (phone) | INFO | 3.9400 | >= +6 dB (loudest 1 s vs the 3 s before) | before -23.5, loudest -19.6 dBFS; continuation of Call at 40.8 s |
| kraken SurfaceRoar at 52.1 s 80-800 Hz | INFO | 9.1400 | >= +8 dB (loudest 1 s vs the 3 s before) | before -25.8, loudest -16.7 dBFS; continuation of SurfaceRoar at 40.8 s |
| kraken SurfaceRoar at 52.1 s HP 250 (laptop) | INFO | 9.0900 | >= +6 dB (loudest 1 s vs the 3 s before) | before -25.7, loudest -16.7 dBFS; continuation of SurfaceRoar at 40.8 s |
| kraken SurfaceRoar at 52.1 s HP 400 (phone) | INFO | 9.1200 | >= +6 dB (loudest 1 s vs the 3 s before) | before -27.1, loudest -18.0 dBFS; continuation of SurfaceRoar at 40.8 s |
| kraken Slap at 56.8 s 80-800 Hz | INFO | 3.8200 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -18.3, loudest -14.5 dBFS; continuation of SurfaceRoar at 52.1 s |
| kraken Slap at 56.8 s HP 250 (laptop) | INFO | 1.9000 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -18.2, loudest -16.3 dBFS; continuation of SurfaceRoar at 52.1 s |
| kraken Slap at 56.8 s HP 400 (phone) | INFO | 1.3500 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -19.6, loudest -18.2 dBFS; continuation of SurfaceRoar at 52.1 s |

### x2_strike.wav (PASS)

RMS per 1 s: `▁▁▁▁▁▁▁▁▇▇▇▇▇▇█████▇███████▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -3.0900 | <= -1 dBTP | 4x oversampled, at 14.821 s |
| sample peak | INFO | -3.0900 |  | at 14.821 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.2222 | <= 0.25 FS | at 14.8210 s, 0 over (footsteps excluded) |
| not silent | PASS | -29.66 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -23.30 |  | BS.1770-4, reported only |
| kraken Strike at 14.4 s 80-800 Hz | PASS | 30.8600 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -48.1, loudest -17.3 dBFS |
| kraken Strike at 14.4 s HP 250 (laptop) | PASS | 38.0000 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -56.8, loudest -18.8 dBFS |
| kraken Strike at 14.4 s HP 400 (phone) | PASS | 39.9800 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -59.7, loudest -19.8 dBFS |
| kraken Call at 19.2 s 80-800 Hz | INFO | -3.8300 | >= +8 dB (loudest 1 s vs the 3 s before) | before -23.8, loudest -27.7 dBFS; continuation of Strike at 14.4 s |
| kraken Call at 19.2 s HP 250 (laptop) | INFO | -2.8400 | >= +6 dB (loudest 1 s vs the 3 s before) | before -28.0, loudest -30.8 dBFS; continuation of Strike at 14.4 s |
| kraken Call at 19.2 s HP 400 (phone) | INFO | -3.6200 | >= +6 dB (loudest 1 s vs the 3 s before) | before -35.1, loudest -38.7 dBFS; continuation of Strike at 14.4 s |

### x2_ending.wav (PASS)

RMS per 1 s: `▁▁████████████████████████████████████████▇▇▇▇▇████▇▇▇▇▇▇▇▇▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -13.7300 | <= -1 dBTP | 4x oversampled, at 159.954 s |
| sample peak | INFO | -13.7400 |  | at 159.954 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0649 | <= 0.25 FS | at 12.1446 s, 0 over (footsteps excluded) |
| not silent | PASS | -34.52 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -30.76 |  | BS.1770-4, reported only |
| kraken Stinger at 155.3 s 80-800 Hz | PASS | 15.3500 | >= +8 dB (loudest 1 s vs the 3 s before) | before -49.1, loudest -33.8 dBFS |
| kraken Stinger at 155.3 s HP 250 (laptop) | PASS | 11.3000 | >= +6 dB (loudest 1 s vs the 3 s before) | before -48.5, loudest -37.2 dBFS |
| kraken Stinger at 155.3 s HP 400 (phone) | PASS | 8.5500 | >= +6 dB (loudest 1 s vs the 3 s before) | before -49.8, loudest -41.2 dBFS |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
