# Audio analysis: AU-walk15-d05

Files: 1. Checks: 12 PASS, 0 FAIL, 0 SKIP, 5 INFO. Scene constants: NocturneConstants.cs.

**Result: PASS**

## RMS per beat (mix files, 14.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | alley | -34.91 | -34.9 | -0.01 | 78.0 s | PASS |
| 2 | boulevard | -30.46 | -30.4 | -0.06 | 186.0 s | PASS |
| 3 | edge | -30.77 | -31.3 | +0.53 | 81.0 s | PASS |
| 4 | skybridge | -29.77 | -29.6 | -0.17 | 210.0 s | PASS |
| 5 | garden | -33.4 | -33.3 | -0.10 | 120.0 s | PASS |
| 6 | signs | -30.07 | -30.1 | +0.03 | 120.0 s | PASS |
| 7 | heart | -28.96 | -28.9 | -0.06 | 132.0 s | PASS |

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| walk15_d05_mix.wav | mix | float32 48000 Hz x2 | 1015.0 | -5.53 | 0.000000 | 0.1379 | -29.74 | -27.22 | PASS |

### walk15_d05_mix.wav (PASS)

RMS per 1 s: `▁▁▁▁▃▃▃▃▃▄▅▃▃▃▃▃▃▃▃▃▄▃▄█▄█▄█▄████▃▃▂▂▂▂▂▂▂▃▃▄▃▃▃▃▄▄▅▅▄▅▅▅▅▄▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| sample peak | PASS | -5.5300 | <= -1 dBFS | at 540.878 s (14.3) |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.1379 | <= 0.25 FS | at 539.7567 s, 0 over |
| not silent | PASS | -29.74 | > -80 dBFS RMS |  |
| S-window s 400-420 | INFO | -31.4500 |  | RMS dBFS inside the swell, peak -17.08 dBFS (14.3: logged separately) |
| S-window s 540-554 | INFO | -30.2800 |  | RMS dBFS inside the swell, peak -17.54 dBFS (14.3: logged separately) |
| S-window s 876-896 | INFO | -33.1000 |  | RMS dBFS inside the swell, peak -13.73 dBFS (14.3: logged separately) |
| S-window s 1340-1380 | INFO | -29.5200 |  | RMS dBFS inside the swell, peak -14.85 dBFS (14.3: logged separately) |
| integrated loudness | INFO | -27.22 |  | BS.1770-4 (reported, not gated) |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
