# Audio analysis: audio_examples

Files: 10. Checks: 49 PASS, 0 FAIL, 7 SKIP, 20 INFO. Scene constants: BulwarkConst.cs.

**Result: PASS**

## RMS per beat (mix files, 14.3.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | WaveGallery | - | -34.0 | - | - | SKIP |
| 2 | Crest | - | -24.0 | - | - | SKIP |
| 3 | SpillwayBridge | - | -18.0 | - | - | SKIP |
| 4 | ArmourSlope | - | -23.0 | - | - | SKIP |
| 5 | GateTower | - | -32.0 | - | - | SKIP |
| 6 | WindowGallery | - | -38.0 | - | - | SKIP |
| 7 | SluiceLedge | - | -25.0 | - | - | SKIP |

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| kraken_call.wav | example | float32 48000 Hz x2 | 14.0 | -5.54 | 0.000022 | 0.0597 | -23.50 | -22.66 | PASS |
| kraken_call_underwater.wav | example | float32 48000 Hz x2 | 14.0 | -7.09 | 0.000001 | 0.0159 | -29.54 | -26.00 | PASS |
| kraken_surface_roar.wav | example | float32 48000 Hz x2 | 16.0 | -6.64 | 0.000073 | 0.0551 | -22.41 | -21.37 | PASS |
| kraken_slap.wav | example | float32 48000 Hz x2 | 10.0 | -4.93 | 0.000088 | 0.0787 | -22.98 | -22.20 | PASS |
| kraken_dam_shudder.wav | example | float32 48000 Hz x2 | 11.0 | -5.07 | 0.000041 | 0.0465 | -21.71 | -21.01 | PASS |
| kraken_glass_creak.wav | example | float32 48000 Hz x2 | 14.0 | -13.77 | 0.000001 | 0.0066 | -31.85 | -29.04 | PASS |
| kraken_sucker_pops.wav | example | float32 48000 Hz x2 | 16.0 | -6.73 | 0.000003 | 0.0081 | -33.41 | -33.22 | PASS |
| kraken_eye_swell.wav | example | float32 48000 Hz x2 | 18.0 | -7.75 | 0.000002 | 0.0117 | -30.09 | -26.51 | PASS |
| kraken_stinger.wav | example | float32 48000 Hz x2 | 14.0 | -19.54 | 0.000001 | 0.0316 | -38.13 | -37.32 | PASS |
| ride.wav | example | float32 48000 Hz x2 | 146.0 | -16.62 | 0.000000 | 0.0580 | -32.18 | -28.92 | PASS |

### kraken_call.wav (PASS)

RMS per 1 s: `▅▆▃▇████▅▁▅▆▅▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -5.5400 | <= -1 dBTP | 4x oversampled, at 7.005 s |
| sample peak | INFO | -5.5400 |  | at 7.005 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0597 | <= 0.25 FS | at 6.0803 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -22.66 |  | BS.1770-4, reported only |
| kraken Call at 2.0 s audible | PASS | 4.2300 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -31.6, during -27.4 dBFS |

### kraken_call_underwater.wav (PASS)

RMS per 1 s: `▁▁▃▇▇▇██▇▃▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -7.0800 | <= -1 dBTP | 4x oversampled, at 7.624 s |
| sample peak | INFO | -7.0900 |  | at 7.624 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0159 | <= 0.25 FS | at 7.0061 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -26.00 |  | BS.1770-4, reported only |
| kraken Call at 2.0 s audible | PASS | 21.8300 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -53.0, during -31.2 dBFS |

### kraken_surface_roar.wav (PASS)

RMS per 1 s: `▅▆▄▅▇██▆██▃▁▃▅▅▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -6.6400 | <= -1 dBTP | 4x oversampled, at 6.382 s |
| sample peak | INFO | -6.6400 |  | at 6.382 s |
| DC offset | PASS | 0.0001 | < 0.001 |  |
| sample step | PASS | 0.0551 | <= 0.25 FS | at 1.8213 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -21.37 |  | BS.1770-4, reported only |
| kraken SurfaceRoar at 2.0 s audible | PASS | 3.8500 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -30.5, during -26.6 dBFS |

### kraken_slap.wav (PASS)

RMS per 1 s: `▄▅█▂▁▃▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -4.9300 | <= -1 dBTP | 4x oversampled, at 2.087 s |
| sample peak | INFO | -4.9300 |  | at 2.087 s |
| DC offset | PASS | 0.0001 | < 0.001 |  |
| sample step | PASS | 0.0787 | <= 0.25 FS | at 2.0291 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -22.20 |  | BS.1770-4, reported only |
| kraken Slap at 2.0 s audible | PASS | 5.5100 | >= +3 dB (80-800 Hz, 1 s after vs 3 s before) | before -30.5, during -25.0 dBFS |

### kraken_dam_shudder.wav (PASS)

RMS per 1 s: `▂▃▆▆██▄▁▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -5.0300 | <= -1 dBTP | 4x oversampled, at 2.043 s |
| sample peak | INFO | -5.0700 |  | at 2.043 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0465 | <= 0.25 FS | at 1.8213 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -21.01 |  | BS.1770-4, reported only |
| kraken DamShudder at 2.0 s audible | PASS | 5.2400 | >= +3 dB (80-800 Hz, 2 s after vs 3 s before) | before -31.4, during -26.2 dBFS |

### kraken_glass_creak.wav (PASS)

RMS per 1 s: `▁▁▂▅▇▇▇█▇▄▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -13.7700 | <= -1 dBTP | 4x oversampled, at 6.241 s |
| sample peak | INFO | -13.7700 |  | at 6.241 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0066 | <= 0.25 FS | at 7.1820 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -29.04 |  | BS.1770-4, reported only |
| kraken GlassCreak at 2.0 s audible | PASS | 21.0700 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -53.0, during -32.0 dBFS |

### kraken_sucker_pops.wav (PASS)

RMS per 1 s: `▁▁▂▅▅▆▇█▇██▅▃▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -6.7300 | <= -1 dBTP | 4x oversampled, at 7.502 s |
| sample peak | INFO | -6.7300 |  | at 7.502 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0081 | <= 0.25 FS | at 7.5008 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -33.22 |  | BS.1770-4, reported only |
| kraken SuckerPops at 2.0 s audible | PASS | 14.7600 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -53.0, during -38.3 dBFS |

### kraken_eye_swell.wav (PASS)

RMS per 1 s: `▃▃▂▁▁▅▇████▇▆▃▃▃▃▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -7.7500 | <= -1 dBTP | 4x oversampled, at 8.803 s |
| sample peak | INFO | -7.7500 |  | at 8.803 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0117 | <= 0.25 FS | at 8.8034 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -26.51 |  | BS.1770-4, reported only |
| kraken EyeSwell at 2.0 s audible | PASS | 22.0800 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -53.0, during -31.0 dBFS |

### kraken_stinger.wav (PASS)

RMS per 1 s: `▁▂▄█████▆▃▂▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -19.5400 | <= -1 dBTP | 4x oversampled, at 7.707 s |
| sample peak | INFO | -19.5400 |  | at 7.707 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0316 | <= 0.25 FS | at 12.2295 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -37.32 |  | BS.1770-4, reported only |
| kraken Stinger at 2.0 s audible | PASS | 10.9600 | >= +3 dB (80-800 Hz, 3 s after vs 3 s before) | before -50.5, during -39.5 dBFS |

### ride.wav (PASS)

RMS per 1 s: `▆▆▃▃▆▇▇▇▇▇▇▇▇▇▇▇▇▇▆▆▇▇▇▇▇▇▇▇▇▇▇▇▇▇▅▂▂▆▇▇▇████████▇▇▇▅▂▂▁▂▂▁▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| true peak | PASS | -16.5900 | <= -1 dBTP | 4x oversampled, at 110.765 s |
| sample peak | INFO | -16.6200 |  | at 110.765 s |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0580 | <= 0.25 FS | at 2.2017 s, 0 over (footsteps excluded) |
| integrated loudness | INFO | -28.92 |  | BS.1770-4, reported only |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
