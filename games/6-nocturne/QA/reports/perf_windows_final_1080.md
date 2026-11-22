# Perf windows: final_1080

CSV `QA/reports/perf_final_1080.csv` (173041 frames; offscreen RT none: window size)

**Result: SKIP**

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| 14.5.4 (4) s 580-600 | SKIP | 0 | frames | the run never reached this window |
| 14.5.4 (4) s 700-740 | SKIP | 0 | frames | the run never reached this window |
| boundary s 120 +-10 s: frames over 20 ms | PASS | 0 | 0 | max 16.55 ms, p99 4.08, mean 3.04 over 6575 frames (s [95.0, 145.0]) |
| boundary s 120 +-10 s: GPU p99 | PASS | 4.051 | <= 8.2 ms (1080p) | mean 3.022 ms |
| boundary s 400 +-10 s | SKIP | 0 | frames | the run did not cross s 400 |
| boundary s 560 +-10 s | SKIP | 0 | frames | the run did not cross s 560 |
| boundary s 880 +-10 s | SKIP | 0 | frames | the run did not cross s 880 |
| boundary s 1080 +-10 s | SKIP | 0 | frames | the run did not cross s 1080 |
| boundary s 1260 +-10 s | SKIP | 0 | frames | the run did not cross s 1260 |


## boundary s 120 +-10 s

| t | s | frames | max ms | mean ms | GPU max ms |
|---|---|---|---|---|---|
| 35.2 | 95.0 | 425 | 3.54 | 2.36 | 3.64 |
| 36.2 | 97.5 | 434 | 5.16 | 2.3 | 4.46 |
| 37.2 | 100.0 | 432 | 2.46 | 2.31 | 2.45 |
| 38.2 | 102.5 | 399 | 5.91 | 2.51 | 4.30 |
| 39.2 | 105.0 | 376 | 2.87 | 2.66 | 2.86 |
| 40.2 | 107.5 | 368 | 3.01 | 2.72 | 2.93 |
| 41.2 | 110.0 | 361 | 2.92 | 2.77 | 2.90 |
| 42.2 | 112.5 | 369 | 3.01 | 2.71 | 2.85 |
| 43.2 | 115.0 | 337 | 3.72 | 2.96 | 3.72 |
| 44.2 | 117.5 | 292 | 4.08 | 3.42 | 4.06 |
| 45.2 | 120.0 | 293 | 4.09 | 3.41 | 4.05 |
| 46.2 | 122.5 | 287 | 4.28 | 3.48 | 4.27 |
| 47.2 | 125.0 | 283 | 4.27 | 3.54 | 4.29 |
| 48.2 | 127.5 | 281 | 4.23 | 3.55 | 4.20 |
| 49.2 | 130.0 | 283 | 3.7 | 3.53 | 3.69 |
| 50.2 | 132.5 | 284 | 3.72 | 3.53 | 3.66 |
| 51.2 | 135.0 | 278 | 3.97 | 3.59 | 3.95 |
| 52.2 | 137.5 | 273 | 4.18 | 3.67 | 4.14 |
| 53.2 | 140.0 | 263 | 4.78 | 3.8 | 4.78 |
| 54.2 | 142.5 | 257 | 16.55 | 3.87 | 11.09 |

