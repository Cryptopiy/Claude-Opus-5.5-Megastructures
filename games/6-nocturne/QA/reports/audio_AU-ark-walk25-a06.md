# Audio analysis: AU-ark-walk25-a06

Files: 1. Checks: 12 PASS, 0 FAIL, 0 SKIP, 5 INFO. Scene constants: NocturneConstants.cs.

**Result: PASS**

## RMS per beat (mix files, 14.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | alley | -34.93 | -34.9 | -0.03 | 48.0 s | PASS |
| 2 | boulevard | -30.22 | -30.4 | +0.18 | 111.0 s | PASS |
| 3 | edge | -31.31 | -31.3 | -0.01 | 48.0 s | PASS |
| 4 | skybridge | -29.72 | -29.6 | -0.12 | 126.0 s | PASS |
| 5 | garden | -32.63 | -33.3 | +0.67 | 72.0 s | PASS |
| 6 | signs | -30.12 | -30.1 | -0.02 | 72.0 s | PASS |
| 7 | heart | -29.02 | -28.9 | -0.12 | 78.0 s | PASS |

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| a06_25.wav | mix | float32 48000 Hz x2 | 615.8 | -11.51 | 0.000000 | 0.1422 | -30.50 | -27.88 | PASS |

### a06_25.wav (PASS)

RMS per 1 s: `▆▆▆▆▇▇▇▇▇▇██▇▇▇▇▇▇▇▇▇▇▇█▇█▇█▇████▇▇▇▇▇▇▆▇▆▇▇▇▇▇▇▇▇█▇▇▇██▇██▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| sample peak | PASS | -11.5100 | <= -1 dBFS | at 110.204 s (14.3) |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.1422 | <= 0.25 FS | at 315.6669 s, 0 over |
| not silent | PASS | -30.50 | > -80 dBFS RMS |  |
| S-window s 400-420 | INFO | -32.2800 |  | RMS dBFS inside the swell, peak -17.94 dBFS (14.3: logged separately) |
| S-window s 540-554 | INFO | -31.3100 |  | RMS dBFS inside the swell, peak -17.51 dBFS (14.3: logged separately) |
| S-window s 876-896 | INFO | -32.6100 |  | RMS dBFS inside the swell, peak -14.09 dBFS (14.3: logged separately) |
| S-window s 1340-1380 | INFO | -29.5400 |  | RMS dBFS inside the swell, peak -14.76 dBFS (14.3: logged separately) |
| integrated loudness | INFO | -27.88 |  | BS.1770-4 (reported, not gated) |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
