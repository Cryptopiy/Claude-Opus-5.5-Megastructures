# Audio analysis: AU-walk25-f07

Files: 1. Checks: 12 PASS, 0 FAIL, 0 SKIP, 5 INFO. Scene constants: NocturneConstants.cs.

**Result: PASS**

## RMS per beat (mix files, 14.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | alley | -34.93 | -34.9 | -0.03 | 48.0 s | PASS |
| 2 | boulevard | -30.22 | -30.4 | +0.18 | 111.0 s | PASS |
| 3 | edge | -31.12 | -31.3 | +0.18 | 48.0 s | PASS |
| 4 | skybridge | -29.09 | -29.6 | +0.51 | 126.0 s | PASS |
| 5 | garden | -33.07 | -33.3 | +0.23 | 72.0 s | PASS |
| 6 | signs | -30.12 | -30.1 | -0.02 | 72.0 s | PASS |
| 7 | heart | -29.04 | -28.9 | -0.14 | 78.0 s | PASS |

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| mix.wav | mix | float32 48000 Hz x2 | 615.8 | -4.94 | 0.000000 | 0.1468 | -29.29 | -26.70 | PASS |

### mix.wav (PASS)

RMS per 1 s: `▅▅▅▅▆▆▆▆▆▆▇▇▆▆▆▆▆▆▆▆▆▆▆█▆█▆█▆████▆▆▅▅▅▆▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▇▇▆▇▆▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| sample peak | PASS | -4.9400 | <= -1 dBFS | at 281.133 s (14.3) |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.1468 | <= 0.25 FS | at 303.6082 s, 0 over |
| not silent | PASS | -29.29 | > -80 dBFS RMS |  |
| S-window s 400-420 | INFO | -32.2900 |  | RMS dBFS inside the swell, peak -18.01 dBFS (14.3: logged separately) |
| S-window s 540-554 | INFO | -31.1100 |  | RMS dBFS inside the swell, peak -16.78 dBFS (14.3: logged separately) |
| S-window s 876-896 | INFO | -32.7100 |  | RMS dBFS inside the swell, peak -14.43 dBFS (14.3: logged separately) |
| S-window s 1340-1380 | INFO | -29.5400 |  | RMS dBFS inside the swell, peak -14.72 dBFS (14.3: logged separately) |
| integrated loudness | INFO | -26.70 |  | BS.1770-4 (reported, not gated) |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
