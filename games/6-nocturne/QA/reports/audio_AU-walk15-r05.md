# Audio analysis: AU-walk15-r05

Files: 1. Checks: 12 PASS, 0 FAIL, 0 SKIP, 5 INFO. Scene constants: NocturneConstants.cs.

**Result: PASS**

## RMS per beat (mix files, 14.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | alley | -34.91 | -34.9 | -0.01 | 78.0 s | PASS |
| 2 | boulevard | -30.53 | -30.4 | -0.13 | 186.0 s | PASS |
| 3 | edge | -31.07 | -31.3 | +0.23 | 81.0 s | PASS |
| 4 | skybridge | -29.87 | -29.6 | -0.27 | 210.0 s | PASS |
| 5 | garden | -33.41 | -33.3 | -0.11 | 120.0 s | PASS |
| 6 | signs | -30.09 | -30.1 | +0.01 | 120.0 s | PASS |
| 7 | heart | -28.97 | -28.9 | -0.07 | 132.0 s | PASS |

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| walk15_r05.wav | mix | float32 48000 Hz x2 | 1015.0 | -5.52 | 0.000000 | 0.1379 | -29.84 | -27.27 | PASS |

### walk15_r05.wav (PASS)

RMS per 1 s: `▁▁▁▁▃▃▃▃▃▄▅▃▃▃▃▃▃▃▃▃▃▃▄█▄█▄█▄████▃▃▂▂▂▂▂▂▂▃▃▄▃▃▃▃▄▄▅▅▄▅▅▅▅▄▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| sample peak | PASS | -5.5200 | <= -1 dBFS | at 540.878 s (14.3) |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.1379 | <= 0.25 FS | at 539.7567 s, 0 over |
| not silent | PASS | -29.84 | > -80 dBFS RMS |  |
| S-window s 400-420 | INFO | -31.4500 |  | RMS dBFS inside the swell, peak -17.07 dBFS (14.3: logged separately) |
| S-window s 540-554 | INFO | -31.9000 |  | RMS dBFS inside the swell, peak -19.01 dBFS (14.3: logged separately) |
| S-window s 876-896 | INFO | -33.3500 |  | RMS dBFS inside the swell, peak -13.73 dBFS (14.3: logged separately) |
| S-window s 1340-1380 | INFO | -29.5200 |  | RMS dBFS inside the swell, peak -14.84 dBFS (14.3: logged separately) |
| integrated loudness | INFO | -27.27 |  | BS.1770-4 (reported, not gated) |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
