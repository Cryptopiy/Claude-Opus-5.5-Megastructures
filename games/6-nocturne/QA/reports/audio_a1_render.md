# Audio analysis: a1_render

Files: 1. Checks: 5 PASS, 7 FAIL, 0 SKIP, 5 INFO. Scene constants: NocturneConstants.cs.

**Result: FAIL**

## RMS per beat (mix files, 14.3 totals +-3 dB)

| Beat | Name | RMS dBFS | Target | Delta dB | Measured | Result |
|---|---|---|---|---|---|---|
| 1 | alley | -52.37 | -34.9 | -17.47 | 48.0 s | FAIL |
| 2 | boulevard | -51.78 | -30.4 | -21.38 | 111.0 s | FAIL |
| 3 | edge | -51.8 | -31.3 | -20.50 | 48.0 s | FAIL |
| 4 | skybridge | -51.26 | -29.6 | -21.66 | 126.0 s | FAIL |
| 5 | garden | -52.72 | -33.3 | -19.42 | 72.0 s | FAIL |
| 6 | signs | -52.08 | -30.1 | -21.98 | 72.0 s | FAIL |
| 7 | heart | -52.27 | -28.9 | -23.37 | 78.0 s | FAIL |

## Files

| File | Stem | Format | Seconds | Peak dBFS | \|DC\| | Max step | RMS dBFS | LUFS | Result |
|---|---|---|---|---|---|---|---|---|---|
| a1_render_cinematic.wav | mix | float32 48000 Hz x2 | 600.0 | -26.50 | 0.000000 | 0.0375 | -51.94 | -48.17 | PASS |

### a1_render_cinematic.wav (PASS)

RMS per 1 s: `▃▃▃▄▅▆▆▆▆▆▆▅▆▅▆▆▅▆▅▇▆▇▇███▇▇███▇██▇▆▃▂▂▃▁▂▂▄▅▅▅▄▅▄▅▃▄▃▃▃▃▃▄▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| NaN/Inf samples | PASS | 0 | 0 |  |
| sample peak | PASS | -26.5000 | <= -1 dBFS | at 253.983 s (14.3) |
| DC offset | PASS | 0.0000 | < 0.001 |  |
| sample step | PASS | 0.0375 | <= 0.25 FS | at 250.2450 s, 0 over |
| not silent | PASS | -51.94 | > -80 dBFS RMS |  |
| S-window s 400-420 | INFO | -51.8400 |  | RMS dBFS inside the swell, peak -29.46 dBFS (14.3: logged separately) |
| S-window s 540-554 | INFO | -51.9500 |  | RMS dBFS inside the swell, peak -28.74 dBFS (14.3: logged separately) |
| S-window s 876-896 | INFO | -52.2100 |  | RMS dBFS inside the swell, peak -28.33 dBFS (14.3: logged separately) |
| S-window s 1340-1380 | INFO | -52.3000 |  | RMS dBFS inside the swell, peak -26.71 dBFS (14.3: logged separately) |
| integrated loudness | INFO | -48.17 |  | BS.1770-4 (reported, not gated) |

## Method notes

- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).
- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.
