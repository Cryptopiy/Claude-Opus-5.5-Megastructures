# Audio analysis: x2_audio_examples

Files: 11. Checks: 68 PASS, 4 FAIL, 6 SKIP, 42 INFO. Scene constants: BulwarkConst.cs.

**Result: FAIL**

## RMS per beat (mix files, 14.3.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | WaveGallery | - | -34.0 | - | - | SKIP |
| 2 | Crest | - | -24.0 | - | - | SKIP |
| 3 | SpillwayBridge | - | -18.0 | - | - | SKIP |
| 4 | ArmourSlope | -20.42 | -23.0 | +2.58 | 26.7 s | FAIL |
| 5 | GateTower | - | -32.0 | - | - | SKIP |
| 6 | WindowGallery | - | -38.0 | - | - | SKIP |
| 7 | SluiceLedge | - | -25.0 | - | - | SKIP |

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| kraken_call.wav | example | float32 48000 Hz x2 | 15.5 | -2.52 | 0.000027 | 0.1213 | -21.19 | -19.52 | FAIL |
| kraken_surface_roar.wav | example | float32 48000 Hz x2 | 17.5 | -3.00 | 0.000013 | 0.0768 | -22.21 | -20.31 | FAIL |
| kraken_slap.wav | example | float32 48000 Hz x2 | 11.5 | -2.45 | 0.000081 | 0.1354 | -21.76 | -20.06 | PASS |
| kraken_dam_shudder.wav | example | float32 48000 Hz x2 | 12.5 | -2.65 | 0.000017 | 0.2142 | -20.89 | -19.75 | PASS |
| kraken_stinger.wav | example | float32 48000 Hz x2 | 37.5 | -11.42 | 0.000000 | 0.0312 | -33.04 | -30.99 | PASS |
| kraken_strike.wav | example | float32 48000 Hz x2 | 13.5 | -3.16 | 0.000012 | 0.2242 | -25.67 | -21.61 | PASS |
| kraken_glass_creak.wav | example | float32 48000 Hz x2 | 15.5 | -14.16 | 0.000003 | 0.0066 | -32.19 | -29.43 | PASS |
| kraken_sucker_pops.wav | example | float32 48000 Hz x2 | 17.5 | -6.43 | 0.000002 | 0.0086 | -33.67 | -33.52 | PASS |
| kraken_eye_swell.wav | example | float32 48000 Hz x2 | 19.5 | -7.64 | 0.000005 | 0.0118 | -30.36 | -26.69 | PASS |
| kraken_call_underwater.wav | example | float32 48000 Hz x2 | 15.5 | -7.11 | 0.000003 | 0.0155 | -30.26 | -26.48 | PASS |
| kfeed_770_835_2p5.wav | mix | float32 48000 Hz x2 | 26.8 | -2.59 | 0.000028 | 0.1203 | -20.42 | -18.17 | PASS |

### kraken_call.wav (FAIL)

RMS per 1 s: `▁▂▂▁▅▆█▇██▆▄▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -2.5200 | <= -1 dBTP | 4x oversampled, at 6.146 s |
| sample peak | INFO | -2.5200 |  | at 6.146 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.1213 | <= 0.25 FS | at 8.2605 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -19.52 |  | BS.1770-4, reported only |
| kraken Call at 3.5 s 80-800 Hz | FAIL | 7.9100 | >= +8 dB (loudest 1 s vs the 3 s before) | before -27.8, loudest -19.9 dBFS |
| kraken Call at 3.5 s HP 250 (laptop) | PASS | 7.0800 | >= +6 dB (loudest 1 s vs the 3 s before) | before -27.9, loudest -20.8 dBFS |
| kraken Call at 3.5 s HP 400 (phone) | PASS | 6.4500 | >= +6 dB (loudest 1 s vs the 3 s before) | before -29.1, loudest -22.7 dBFS |

### kraken_surface_roar.wav (FAIL)

RMS per 1 s: `▄▅▅▄▄██▇▅▃▄▂▁▃▅▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -2.9900 | <= -1 dBTP | 4x oversampled, at 6.149 s |
| sample peak | INFO | -3.0000 |  | at 6.149 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0768 | <= 0.25 FS | at 5.5242 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -20.31 |  | BS.1770-4, reported only |
| kraken SurfaceRoar at 3.5 s 80-800 Hz | FAIL | 7.9400 | >= +8 dB (loudest 1 s vs the 3 s before) | before -26.5, loudest -18.5 dBFS |
| kraken SurfaceRoar at 3.5 s HP 250 (laptop) | PASS | 7.2800 | >= +6 dB (loudest 1 s vs the 3 s before) | before -25.9, loudest -18.6 dBFS |
| kraken SurfaceRoar at 3.5 s HP 400 (phone) | PASS | 7.0200 | >= +6 dB (loudest 1 s vs the 3 s before) | before -27.0, loudest -20.0 dBFS |

### kraken_slap.wav (PASS)

RMS per 1 s: `▃▄▄▇█▂▁▃▃▃▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -2.4100 | <= -1 dBTP | 4x oversampled, at 3.546 s |
| sample peak | INFO | -2.4500 |  | at 3.546 s |
| DC offset | PASS | 0.0001 | < 0.001 |  |
| sample step | PASS | 0.1354 | <= 0.25 FS | at 3.5793 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -20.06 |  | BS.1770-4, reported only |
| kraken Slap at 3.5 s 80-800 Hz | PASS | 10.6100 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -26.5, loudest -15.9 dBFS |
| kraken Slap at 3.5 s HP 250 (laptop) | PASS | 9.2000 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -25.9, loudest -16.7 dBFS |
| kraken Slap at 3.5 s HP 400 (phone) | PASS | 8.8400 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -27.0, loudest -18.2 dBFS |

### kraken_dam_shudder.wav (PASS)

RMS per 1 s: `▁▂▂▃▅▆██▂▁▁▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -2.6500 | <= -1 dBTP | 4x oversampled, at 5.322 s |
| sample peak | INFO | -2.6500 |  | at 5.322 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.2142 | <= 0.25 FS | at 3.5449 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -19.75 |  | BS.1770-4, reported only |
| kraken DamShudder at 3.5 s 80-800 Hz | PASS | 11.3100 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -27.7, loudest -16.4 dBFS |
| kraken DamShudder at 3.5 s HP 250 (laptop) | PASS | 9.1000 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -27.8, loudest -18.7 dBFS |
| kraken DamShudder at 3.5 s HP 400 (phone) | PASS | 9.2400 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -29.0, loudest -19.8 dBFS |

### kraken_stinger.wav (PASS)

RMS per 1 s: `▁▂▂▁▅▇▇▇▇▇██▇▆▇██▇▇▇██▇▆▇█▇▇▇▇▇▆▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -11.4100 | <= -1 dBTP | 4x oversampled, at 15.184 s |
| sample peak | INFO | -11.4200 |  | at 15.184 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0312 | <= 0.25 FS | at 36.3026 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -30.99 |  | BS.1770-4, reported only |
| kraken Stinger at 3.5 s 80-800 Hz | PASS | 15.0700 | >= +8 dB (loudest 1 s vs the 3 s before) | before -45.8, loudest -30.7 dBFS |
| kraken Stinger at 3.5 s HP 250 (laptop) | PASS | 10.9700 | >= +6 dB (loudest 1 s vs the 3 s before) | before -45.5, loudest -34.6 dBFS |
| kraken Stinger at 3.5 s HP 400 (phone) | PASS | 7.7200 | >= +6 dB (loudest 1 s vs the 3 s before) | before -47.1, loudest -39.4 dBFS |

### kraken_strike.wav (PASS)

RMS per 1 s: `▁▁▁█▇▇███▅▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -3.1600 | <= -1 dBTP | 4x oversampled, at 3.531 s |
| sample peak | INFO | -3.1600 |  | at 3.531 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.2242 | <= 0.25 FS | at 3.9304 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -21.61 |  | BS.1770-4, reported only |
| kraken Strike at 3.5 s 80-800 Hz | PASS | 33.2400 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -50.3, loudest -17.1 dBFS |
| kraken Strike at 3.5 s HP 250 (laptop) | PASS | 43.7300 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -61.5, loudest -17.7 dBFS |
| kraken Strike at 3.5 s HP 400 (phone) | PASS | 45.3500 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -64.0, loudest -18.7 dBFS |

### kraken_glass_creak.wav (PASS)

RMS per 1 s: `▁▁▁▂▄▅▇▇██▆▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -14.1600 | <= -1 dBTP | 4x oversampled, at 7.745 s |
| sample peak | INFO | -14.1600 |  | at 7.745 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0066 | <= 0.25 FS | at 8.6901 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -29.43 |  | BS.1770-4, reported only |
| kraken GlassCreak at 3.5 s 80-800 Hz | PASS | 23.3200 | >= +8 dB (loudest 1 s vs the 3 s before) | before -50.3, loudest -27.0 dBFS |
| kraken GlassCreak at 3.5 s HP 250 (laptop) | INFO | 29.7800 | >= +6 dB (loudest 1 s vs the 3 s before) | before -61.5, loudest -31.7 dBFS |
| kraken GlassCreak at 3.5 s HP 400 (phone) | INFO | 23.1600 | >= +6 dB (loudest 1 s vs the 3 s before) | before -64.0, loudest -40.9 dBFS |

### kraken_sucker_pops.wav (PASS)

RMS per 1 s: `▂▂▁▃▁▅▆▇▆█▇▇▇▃▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -6.4300 | <= -1 dBTP | 4x oversampled, at 9.006 s |
| sample peak | INFO | -6.4300 |  | at 9.006 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0086 | <= 0.25 FS | at 9.0048 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -33.52 |  | BS.1770-4, reported only |
| kraken SuckerPops at 3.5 s 80-800 Hz | PASS | 18.9400 | >= +8 dB (loudest 1 s vs the 3 s before) | before -50.3, loudest -31.4 dBFS |
| kraken SuckerPops at 3.5 s HP 250 (laptop) | INFO | 19.0700 | >= +6 dB (loudest 1 s vs the 3 s before) | before -61.5, loudest -42.4 dBFS |
| kraken SuckerPops at 3.5 s HP 400 (phone) | INFO | 13.7200 | >= +6 dB (loudest 1 s vs the 3 s before) | before -64.0, loudest -50.3 dBFS |

### kraken_eye_swell.wav (PASS)

RMS per 1 s: `▃▃▃▄▂▁▃▆█████▇▄▃▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -7.6400 | <= -1 dBTP | 4x oversampled, at 10.307 s |
| sample peak | INFO | -7.6400 |  | at 10.307 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0118 | <= 0.25 FS | at 10.3074 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -26.69 |  | BS.1770-4, reported only |
| kraken EyeSwell at 3.5 s 80-800 Hz | PASS | 23.2800 | >= +8 dB (loudest 1 s vs the 3 s before) | before -50.3, loudest -27.1 dBFS |
| kraken EyeSwell at 3.5 s HP 250 (laptop) | INFO | 30.7500 | >= +6 dB (loudest 1 s vs the 3 s before) | before -61.5, loudest -30.7 dBFS |
| kraken EyeSwell at 3.5 s HP 400 (phone) | INFO | 23.2600 | >= +6 dB (loudest 1 s vs the 3 s before) | before -64.0, loudest -40.8 dBFS |

### kraken_call_underwater.wav (PASS)

RMS per 1 s: `▁▁▁▂▆▇████▅▂▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -7.1100 | <= -1 dBTP | 4x oversampled, at 9.128 s |
| sample peak | INFO | -7.1100 |  | at 9.128 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0155 | <= 0.25 FS | at 8.5101 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -26.48 |  | BS.1770-4, reported only |
| kraken Call at 3.5 s 80-800 Hz | PASS | 24.0500 | >= +8 dB (loudest 1 s vs the 3 s before) | before -50.3, loudest -26.3 dBFS |
| kraken Call at 3.5 s HP 250 (laptop) | INFO | 31.8200 | >= +6 dB (loudest 1 s vs the 3 s before) | before -61.5, loudest -29.6 dBFS |
| kraken Call at 3.5 s HP 400 (phone) | INFO | 26.0800 | >= +6 dB (loudest 1 s vs the 3 s before) | before -64.0, loudest -38.0 dBFS |

### kfeed_770_835_2p5.wav (PASS)

RMS per 1 s: `▄▅▄▄▁▄▇▇▇▇▇▇▇▆▄▃▁▃▆▆▆▆█▅▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -2.5700 | <= -1 dBTP | 4x oversampled, at 10.480 s |
| sample peak | INFO | -2.5900 |  | at 10.480 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.1203 | <= 0.25 FS | at 10.9825 s, 0 over (footsteps excluded) |
| not silent | PASS | -20.42 | > -80 dBFS RMS |  |
| integrated loudness | INFO | -18.17 |  | BS.1770-4, reported only |
| kraken SurfaceRoar at 4.1 s 80-800 Hz | PASS | 8.0200 | >= +8 dB (loudest 1 s vs the 3 s before) | before -27.0, loudest -19.0 dBFS |
| kraken SurfaceRoar at 4.1 s HP 250 (laptop) | PASS | 7.1300 | >= +6 dB (loudest 1 s vs the 3 s before) | before -26.3, loudest -19.2 dBFS |
| kraken SurfaceRoar at 4.1 s HP 400 (phone) | PASS | 6.6000 | >= +6 dB (loudest 1 s vs the 3 s before) | before -27.4, loudest -20.8 dBFS |
| kraken Call at 5.8 s 80-800 Hz | INFO | 9.3800 | >= +8 dB (loudest 1 s vs the 3 s before) | before -28.2, loudest -18.9 dBFS; continuation of SurfaceRoar at 4.1 s |
| kraken Call at 5.8 s HP 250 (laptop) | INFO | 8.6300 | >= +6 dB (loudest 1 s vs the 3 s before) | before -27.8, loudest -19.1 dBFS; continuation of SurfaceRoar at 4.1 s |
| kraken Call at 5.8 s HP 400 (phone) | INFO | 8.0100 | >= +6 dB (loudest 1 s vs the 3 s before) | before -28.8, loudest -20.8 dBFS; continuation of SurfaceRoar at 4.1 s |
| kraken SurfaceRoar at 5.9 s 80-800 Hz | INFO | 9.1400 | >= +8 dB (loudest 1 s vs the 3 s before) | before -28.1, loudest -18.9 dBFS; continuation of Call at 5.8 s |
| kraken SurfaceRoar at 5.9 s HP 250 (laptop) | INFO | 8.5200 | >= +6 dB (loudest 1 s vs the 3 s before) | before -27.6, loudest -19.1 dBFS; continuation of Call at 5.8 s |
| kraken SurfaceRoar at 5.9 s HP 400 (phone) | INFO | 7.8400 | >= +6 dB (loudest 1 s vs the 3 s before) | before -28.6, loudest -20.8 dBFS; continuation of Call at 5.8 s |
| kraken SurfaceRoar at 17.2 s 80-800 Hz | INFO | 9.3200 | >= +8 dB (loudest 1 s vs the 3 s before) | before -27.9, loudest -18.6 dBFS; continuation of SurfaceRoar at 5.9 s |
| kraken SurfaceRoar at 17.2 s HP 250 (laptop) | INFO | 8.9800 | >= +6 dB (loudest 1 s vs the 3 s before) | before -28.3, loudest -19.3 dBFS; continuation of SurfaceRoar at 5.9 s |
| kraken SurfaceRoar at 17.2 s HP 400 (phone) | INFO | 9.2600 | >= +6 dB (loudest 1 s vs the 3 s before) | before -29.8, loudest -20.5 dBFS; continuation of SurfaceRoar at 5.9 s |
| kraken Slap at 21.8 s 80-800 Hz | INFO | 5.7000 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -21.0, loudest -15.3 dBFS; continuation of SurfaceRoar at 17.2 s |
| kraken Slap at 21.8 s HP 250 (laptop) | INFO | 3.7200 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -20.9, loudest -17.2 dBFS; continuation of SurfaceRoar at 17.2 s |
| kraken Slap at 21.8 s HP 400 (phone) | INFO | 3.4800 | >= +8 dB (loudest 0.05 s vs the 3 s before) | before -22.2, loudest -18.8 dBFS; continuation of SurfaceRoar at 17.2 s |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
