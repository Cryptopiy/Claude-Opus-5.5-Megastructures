# Audio analysis: AU-walk25-r05

Files: 1. Checks: 12 PASS, 0 FAIL, 0 SKIP, 5 INFO. Scene constants: NocturneConstants.cs.

**Result: PASS**

## RMS per beat (mix files, 14.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | alley | -34.91 | -34.9 | -0.01 | 48.0 s | PASS |
| 2 | boulevard | -30.27 | -30.4 | +0.13 | 111.0 s | PASS |
| 3 | edge | -31.53 | -31.3 | -0.23 | 48.0 s | PASS |
| 4 | skybridge | -29.33 | -29.6 | +0.27 | 126.0 s | PASS |
| 5 | garden | -33.17 | -33.3 | +0.13 | 72.0 s | PASS |
| 6 | signs | -30.1 | -30.1 | +0.00 | 72.0 s | PASS |
| 7 | heart | -28.91 | -28.9 | -0.01 | 78.0 s | PASS |

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| walk25_r05.wav | mix | float32 48000 Hz x2 | 615.8 | -4.93 | 0.000000 | 0.1470 | -29.37 | -26.79 | PASS |

### walk25_r05.wav (PASS)

RMS per 1 s: `▅▅▅▅▆▆▆▆▆▆▇▇▆▆▆▆▆▆▆▆▆▆▆█▆█▆█▆████▆▆▅▆▅▅▆▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▇▆▆▆▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| sample peak | PASS | -4.9300 | <= -1 dBFS | at 281.133 s (14.3) |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.1470 | <= 0.25 FS | at 303.6082 s, 0 over |
| not silent | PASS | -29.37 | > -80 dBFS RMS |  |
| S-window s 400-420 | INFO | -32.2700 |  | RMS dBFS inside the swell, peak -17.68 dBFS (14.3: logged separately) |
| S-window s 540-554 | INFO | -32.8800 |  | RMS dBFS inside the swell, peak -18.97 dBFS (14.3: logged separately) |
| S-window s 876-896 | INFO | -33.4500 |  | RMS dBFS inside the swell, peak -14.81 dBFS (14.3: logged separately) |
| S-window s 1340-1380 | INFO | -29.3900 |  | RMS dBFS inside the swell, peak -13.47 dBFS (14.3: logged separately) |
| integrated loudness | INFO | -26.79 |  | BS.1770-4 (reported, not gated) |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
