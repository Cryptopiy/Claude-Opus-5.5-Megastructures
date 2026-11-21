# Frame sequences: x2-seq

Sequences: 37. Checks: 112 PASS, 5 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_b02 | transition | 601 | 2.113 | 1.332 | 5.043 | 182.0 | PASS |  |
| b02_b03 | transition | 601 | 0.289 | 1.257 | 3.362 | 120.5 | PASS |  |
| b03_b04 | transition | 601 | 0.481 | 1.059 | 2.127 | 178.8 | PASS |  |
| b04_b05 | transition | 601 | 2.447 | 2.447 | 6.506 | 205.2 | PASS |  |
| b05_b06 | transition | 601 | 0.410 | 0.751 | 1.090 | 179.9 | PASS |  |
| b06_b07 | transition | 601 | 0.542 | 1.179 | 3.181 | 214.2 | PASS |  |
| end | transition | 217 | 4.508 | 1.459 | 12.399 | 181.3 | FAIL | luma step/frame |
| lamps_b01 | transition | 289 | 0.675 | 1.133 | 2.223 | 184.8 | PASS |  |
| pan_KS04_sea | pan | 30 | 0.002 | 0.209 | 0.211 | 49.6 | PASS |  |
| pan_KS09_p58 | pan | 30 | 0.036 | 0.867 | 0.899 | 100.0 | PASS |  |
| pan_KS09_p90 | pan | 30 | 0.013 | 1.011 | 1.038 | 70.0 | PASS |  |
| pan_KS20 | pan | 30 | 0.010 | 0.347 | 0.357 | 166.5 | PASS |  |
| pan_b01_WaveGallery | pan | 30 | 0.004 | 0.196 | 0.200 | 151.6 | PASS |  |
| pan_b02_Crest | pan | 30 | 0.002 | 0.256 | 0.258 | 103.4 | PASS |  |
| pan_b03_SpillwayBridge | pan | 30 | 0.009 | 0.654 | 0.660 | 109.0 | PASS |  |
| pan_b04_ArmourSlope | pan | 30 | 0.033 | 0.727 | 0.737 | 179.0 | PASS |  |
| pan_b05_GateTower | pan | 30 | 0.010 | 0.186 | 0.189 | 178.7 | PASS |  |
| pan_b06_WindowGallery | pan | 30 | 0.016 | 0.235 | 0.244 | 172.2 | PASS |  |
| pan_b07_SluiceLedge | pan | 30 | 0.018 | 0.351 | 0.362 | 157.9 | PASS |  |
| pan_rail_slow | pan | 30 | 0.002 | 0.049 | 0.072 | 118.2 | PASS |  |
| pan_s1470_city | pan | 30 | 0.011 | 0.346 | 0.360 | 164.8 | PASS |  |
| plumes_KS04 | static | 241 | 0.410 | 3.300 | 3.742 | 111.7 | FAIL | flicker block std |
| s0520_0640 | transition | 1441 | 0.544 | 1.818 | 6.121 | 168.9 | PASS |  |
| s0880_0960 | transition | 961 | 2.447 | 2.150 | 6.506 | 205.2 | PASS |  |
| s1060_1080 | transition | 241 | 0.410 | 0.719 | 1.049 | 179.9 | PASS |  |
| s1405_1450 | transition | 541 | 0.959 | 1.134 | 2.450 | 213.5 | PASS |  |
| start | transition | 60 | 0.369 | 0.055 | 0.369 | 31.3 | PASS |  |
| static_KS04 | static | 120 | 0.042 | 0.742 | 0.919 | 78.1 | FAIL | flicker block std |
| static_KS08 | static | 120 | 0.001 | 0.007 | 0.008 | 84.7 | PASS |  |
| static_KS20 | static | 120 | 0.001 | 0.002 | 0.003 | 92.9 | PASS |  |
| static_b01_WaveGallery | static | 120 | 0.004 | 0.000 | 0.012 | 129.9 | PASS |  |
| static_b02_Crest | static | 120 | 0.005 | 0.014 | 0.020 | 35.1 | FAIL | flicker block std |
| static_b03_SpillwayBridge | static | 120 | 0.000 | 0.003 | 0.003 | 46.5 | PASS |  |
| static_b04_ArmourSlope | static | 120 | 0.011 | 0.085 | 0.106 | 120.0 | FAIL | flicker block std |
| static_b05_GateTower | static | 120 | 0.000 | 0.000 | 0.000 | 1.9 | PASS |  |
| static_b06_WindowGallery | static | 120 | 0.002 | 0.011 | 0.012 | 5.7 | PASS |  |
| static_b07_SluiceLedge | static | 120 | 0.000 | 0.002 | 0.002 | 75.3 | PASS |  |

## b01_b02 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_b01_b02`, 601 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▃▃▃▄▄▅▅▆▇████████████████████████████████`
- mean diff `▂▂▃▂▃▄▃▂▂▃▂▂▂▂▂▂▃▂▃▄▅▄▅▇▄▆█▇▄▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▁▁▁▁▂▂▂▂▂▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 2.1127 | <= 2.6 levels | at frame 270 |
| activation-frame diff | PASS | 0.0002 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.4150 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b02_b03 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_b02_b03`, 601 frames at 30 fps, kind transition

- luma      `██████▇▇▇▇▇▇▆▆▆▆▆▆▆▆▆▆▆▆▆▅▅▄▄▄▄▄▄▄▄▄▃▃▃▂▂▂▂▂▂▃▃▃▃▃▂▁▁▁▂▂▂▂▂▂`
- mean diff `▂▂▂▂▃▂▁▁▁▁▁▂▁▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▂▂▂▂▂▂▃▃▃▄▄▄▅▅▅▅▅▅▅▄▃▂▃▄▆█████`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.2890 | <= 1.2 levels | at frame 509 |
| activation-frame diff | PASS | 0.0014 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 0.1662 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b03_b04 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_b03_b04`, 601 frames at 30 fps, kind transition

- luma      `▄▄▅▄▄▄▃▃▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▂▂▂▃▃▃▃▃▄▄▄▄▅▅▅▅▆▆▆▆▇▇▇▇▇████████████`
- mean diff `▃▄▄▂▃▃▄▄▃▃▂▃▃▃▃▂▃▂▃▂▂▂▂▂▂▂▂▁▂▁▂▂▃▃▃▃▃▄▄▄▅▆▇█▅▄▅▅▅▅▅▅▅▅▅▅▅▅▅▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.4814 | <= 2 levels | at frame 76, sun break |
| activation-frame diff | PASS | 0.0080 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 0.3146 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b04_b05 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_b04_b05`, 601 frames at 30 fps, kind transition

- luma      `████████████████████████▇▇▆▆▅▅▄▄▄▃▃▃▂▂▂▂▃▃▃▃▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁`
- mean diff `▃▄▄▃▃▃▄▄▄▅▃▃▃▃▃▄▄▄▅▅▆▆▇▇▆▇██▅▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▂▂▃▄▅▅▂▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 2.4470 | <= 2.6 levels | at frame 278 |
| activation-frame diff | PASS | 0.1092 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 5.3677 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b05_b06 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_b05_b06`, 601 frames at 30 fps, kind transition

- luma      `▇▆▅▅▆▇███▇▆▆▅▅▆▇██▇▆▅▄▃▂▃▄▄▅▆▆▆▆▆▆▆▆▆▅▅▄▄▄▄▃▃▃▃▂▂▂▁▁▁▂▂▂▂▃▃▃`
- mean diff `▅▅▅▅▆▇██▇▆▆▅▅▆▇█▇█▆▅▅▅▄▄▆▇▇█▆▆▃▃▄▅▆▆▃▃▁▂▁▁▁▂▂▂▃▂▃▄▅▃▃▃▃▃▄▅▆▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.4103 | <= 1.2 levels | at frame 265 |
| activation-frame diff | PASS | 0.0335 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.2869 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## b06_b07 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_b06_b07`, 601 frames at 30 fps, kind transition

- luma      `▄▅▅▅▅▅▅▆▆▆▆▆▆▅▄▄▄▅▅▅▅▅▆▆▆▆▆▆▆▆▆▆▇██▆▅▄▄▄▅▅▄▂▁▂▃▄▄▄▂▁▂▃▄▅▄▃▁▂`
- mean diff `▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▄▄▆▄▄▃▂▂▃▃▅██▃▃▃▃▄▄▄▃▂▂▃▄▄▄▄▂▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5415 | <= 1.2 levels | at frame 425 |
| activation-frame diff | PASS | 0.0291 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 301 |
| EV slope | INFO | 1.7977 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_end`, 217 frames at 30 fps, kind transition

- luma      `▅▅▂▁▄▇████▇▇▇▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆`
- mean diff `▃▅▅▇█▅▃▃▄▄▃▃▃▄▃▃▃▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | FAIL | 4.5084 | <= 1.2 levels | at frame 17 |
| EV slope | INFO | 4.2885 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## lamps_b01 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_lamps_b01`, 289 frames at 30 fps, kind transition

- luma      `▅▆▇▇████▇▆▅▅▄▃▂▂▂▁▁▁▂▂▂▃▃▃▃▃▃▄▅▆▇▇███▇▇▆▅▅▄▃▂▂▁▁▁▁▁▂▂▃▃▃▃▃▃▄`
- mean diff `▄▅▅▂▃▃▃▃▂▂▂▂▂▂▂▂▃▃▁▂▂▄▅▆▇▅▂▂▂▃▄▅▅▂▃▃▃▃▂▂▂▂▂▂▂▂▃▃▁▂▃▄▆▇█▆▃▂▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6751 | <= 1.2 levels | at frame 155 |
| EV slope | INFO | 1.5014 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## pan_KS04_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_KS04_sea`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▂▂▂▂▃▃▃▃▄▄▄▄▄▅▅▅▆▆▆▆▆▇▇▇███`
- mean diff `▆▄▇▅▃█▆▅▄▆▇█▁█▄▆▆▅▅▅▆▇▄▆▄▆▃▆▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0018 | <= 1.2 levels | at frame 24 |
| EV slope | INFO | 0.0018 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 0.9210 | <= 2 levels | exact rotation homography; raw 0.921, control floor 0.679, low-pass (sigma 1.5) 0.277, block-mean 0.106; gate block [1664, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_KS09_p58 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_KS09_p58`, 30 frames at 30 fps, kind pan

- luma      `▅▆▆▇▇▇▇▆▆▇▆▇▇▇▆▁▆██▆▆▆▆▆▆▆▆▆▅▅`
- mean diff `▅▄▆▃▄▅▄▄▄▅▄▄▄▂▆█▁▅▅▄▆▂▆▃▅▆▃▄▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0362 | <= 2 levels | at frame 16, sun break |
| EV slope | INFO | 0.0352 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.3752 | <= 2 levels | exact rotation homography; raw 1.432, control floor 1.056, low-pass (sigma 1.5) 0.724, block-mean 0.285; gate block [704, 448] (raw); 1 of 420 blocks in the fine-pattern regime |

## pan_KS09_p90 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_KS09_p90`, 30 frames at 30 fps, kind pan

- luma      `▂▃▃▄▄▅▅▆▆▇▇████▆█▇▇▇▆▆▅▄▄▃▃▂▂▁`
- mean diff `▃▃▃▃▃▄▃▃▃▄▃▃▅▁██▂▅▃▃▄▃▃▂▃▃▂▃▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0127 | <= 2 levels | at frame 15, sun break |
| EV slope | INFO | 0.0068 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.1584 | <= 2 levels | exact rotation homography; raw 1.158, control floor 0.825, low-pass (sigma 1.5) 0.585, block-mean 0.209; gate block [1472, 256] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_KS20 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_KS20`, 30 frames at 30 fps, kind pan

- luma      `██▇▇▇▇▇▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁▁`
- mean diff `▆▆▄▅▅▄▅▄▁▇▅▅▅▅▅▁█▄▆▃▆▁█▃▃▆▆▂▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0096 | <= 1.2 levels | at frame 10 |
| EV slope | INFO | 0.0130 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.0670 | <= 1.5 levels | exact rotation homography; raw 2.691, control floor 1.741, low-pass (sigma 1.5) 0.730, block-mean 0.253; gate block [1344, 320] (raw); 11 of 420 blocks in the fine-pattern regime; strict (beat 6 / ledge) |

## pan_b01_WaveGallery (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_b01_WaveGallery`, 30 frames at 30 fps, kind pan

- luma      `█▅█▁▅▅▅▅▅▅▅▅▅▅▅▅▅▁▅▅▅▅█▅█▅████`
- mean diff `▄▂▃▄▄▃▃▁█▂▃▃▆▂▄▄▃▄▅▂▃▅▄▂▃▅▄▃▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0043 | <= 1.2 levels | at frame 22 |
| EV slope | INFO | 0.0138 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 0.9699 | <= 2 levels | exact rotation homography; raw 0.970, control floor 0.583, low-pass (sigma 1.5) 0.217, block-mean 0.125; gate block [896, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_b02_Crest`, 30 frames at 30 fps, kind pan

- luma      `█▅█▅██▅█▅▅▅▅▅▅█▅▅█▅█▅▅▅█▅▅▅▅▅▁`
- mean diff `▃▃▄▂▄▅▆▁▃▄▄▅▅▂▇▂▇▄▅▅▃▅▇█▄▆▆▆▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0024 | <= 1.2 levels | at frame 3 |
| EV slope | INFO | 0.0025 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.3987 | <= 2 levels | exact rotation homography; raw 1.399, control floor 0.988, low-pass (sigma 1.5) 0.350, block-mean 0.135; gate block [896, 512] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_b03_SpillwayBridge (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_b03_SpillwayBridge`, 30 frames at 30 fps, kind pan

- luma      `████▇▇▇▇▆▆▆▆▅▅▅▅▄▄▄▃▃▃▃▃▂▂▂▂▁▁`
- mean diff `▅█▅█▂▇▃▇▅▆▅▆▄▆▃▅▄▃▂▇▆▆▇▄▁▄▁▅▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0085 | <= 2 levels | at frame 8, sun break |
| EV slope | INFO | 0.0062 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.5069 | <= 2 levels | exact rotation homography; raw 2.014, control floor 1.873, low-pass (sigma 1.5) 0.656, block-mean 0.329; gate block [1088, 512] (raw); 23 of 420 blocks in the fine-pattern regime |

## pan_b04_ArmourSlope (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_b04_ArmourSlope`, 30 frames at 30 fps, kind pan

- luma      `███▇▇▇▇▇▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁`
- mean diff `▃▁▃▃▃▁▅▂▆▅▅▄▇▃▆▁▇▅▂▄▆▄▅▃█▄▅▄▅`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0333 | <= 1.2 levels | at frame 14 |
| EV slope | INFO | 0.0175 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.9864 | <= 2 levels | exact rotation homography; raw 2.569, control floor 1.537, low-pass (sigma 1.5) 0.991, block-mean 0.343; gate block [1792, 512] (raw); 14 of 420 blocks in the fine-pattern regime |

## pan_b05_GateTower (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_b05_GateTower`, 30 frames at 30 fps, kind pan

- luma      `▁▁▂▂▂▂▂▃▃▃▃▃▄▄▄▅▅▅▅▆▆▆▆▆▇▇▇███`
- mean diff `▃▅▅▅▃▆▁█▁█▄▅▄▄▇▄█▃▆▂█▂█▆▄▆▇▇▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0098 | <= 1.2 levels | at frame 2 |
| EV slope | INFO | 0.0505 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.6933 | <= 2 levels | exact rotation homography; raw 1.693, control floor 0.834, low-pass (sigma 1.5) 0.639, block-mean 0.479; gate block [640, 960] (raw); 0 of 420 blocks in the fine-pattern regime |

## pan_b06_WindowGallery (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_b06_WindowGallery`, 30 frames at 30 fps, kind pan

- luma      `▁▁▂▂▂▂▃▃▃▃▃▄▄▄▄▅▅▅▅▆▆▆▆▇▇▇▇███`
- mean diff `▆▇▅▅▄▅▅▇▅▃▇▃█▄▄▆▄▆▆▃▆▄▅▄▄▆▂█▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0161 | <= 1.2 levels | at frame 26 |
| EV slope | INFO | 0.0387 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.1863 | <= 1.5 levels | exact rotation homography; raw 1.186, control floor 0.725, low-pass (sigma 1.5) 0.331, block-mean 0.085; gate block [1024, 640] (raw); 0 of 420 blocks in the fine-pattern regime; strict (beat 6 / ledge) |

## pan_b07_SluiceLedge (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_b07_SluiceLedge`, 30 frames at 30 fps, kind pan

- luma      `███▇▇▇▇▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▃▂▂▂▂▁▁`
- mean diff `▅▆▄▆▃▇▁█▅▆▆▅▆▄▅▄▆▅▆▄▄▆▄▄▇▃▇▃▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0176 | <= 1.2 levels | at frame 5 |
| EV slope | INFO | 0.0204 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.2757 | <= 1.5 levels | exact rotation homography; raw 2.262, control floor 1.579, low-pass (sigma 1.5) 0.757, block-mean 0.351; gate block [640, 640] (raw); 7 of 420 blocks in the fine-pattern regime; strict (beat 6 / ledge) |

## pan_rail_slow (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_rail_slow`, 30 frames at 30 fps, kind pan

- luma      `▁▁▁▁▁██▁██▁▁▁█▁▁▁█▁█▁█▁▁█▁███▁`
- mean diff `▃▅▃▃█▂▁▇▁▄▅▃▆▄▃▃▆▂▆▃▃▆▃▆▂▅▄▅▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0025 | <= 1.2 levels | at frame 25 |
| EV slope | INFO | 0.0021 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.1262 | <= 1.5 levels | exact rotation homography; raw 1.126, control floor 0.740, low-pass (sigma 1.5) 0.212, block-mean 0.063; gate block [1728, 512] (raw); 0 of 420 blocks in the fine-pattern regime; strict (beat 6 / ledge) |

## pan_s1470_city (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_pan_s1470_city`, 30 frames at 30 fps, kind pan

- luma      `██▇▇▇▇▇▆▆▆▆▅▅▅▅▄▄▄▄▃▃▃▃▂▂▂▂▁▁▁`
- mean diff `▄▅▅▆▅▃▅▅▃█▅▄▄▄▆▃▃▆▁█▁▆▂▆▂▄▄▃▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0113 | <= 1.2 levels | at frame 4 |
| EV slope | INFO | 0.0142 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| pan shimmer std | PASS | 1.0544 | <= 1.5 levels | exact rotation homography; raw 2.510, control floor 1.754, low-pass (sigma 1.5) 0.934, block-mean 0.158; gate block [1472, 320] (raw); 15 of 420 blocks in the fine-pattern regime; strict (beat 6 / ledge) |

## plumes_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_plumes_KS04`, 241 frames at 4 fps, kind static

- luma      `▁▆▇██▇▄▃▃▂▂▃▄▇▇█▅▂▁▄▅▆▇▇▆▄▃▆▆▃▄▄▅▇▆▄▃▁▂▆▇▇▅▅▅▄▃▂▁▂▅▆▅▃▅▅▇▅▂▁`
- mean diff `▁██▆▆▇▅▃▅▄▆▆▄▆▄▅▅▃▂▆▇▄▆█▄▆▅▄▃▅▆▄▆▆▅▆▅▂▃▆▆▄▄▆▇▄▅▅▄▄▇▇▄▃▃▅▇▆▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.4103 | <= 1.2 levels | at frame 110 |
| EV slope | INFO | 0.0458 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 11.4896 | <= 3 levels | 241 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## s0520_0640 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_s0520_0640`, 1441 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▂▂▂▂▁▂▂▃▃▃▄▅▅▅▄▄▄▅▆▆▆▆▆▆▆▅▄▅▆▆▆▆▇▇▇▆▄▅▆▆▆▇▇██▆▄▅▆▆▆▇▇██`
- mean diff `███▇▇▆▅▃▂▁▁▁▁▁▁▁▁▁▂▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▄▅▄▅▅▄▄▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5435 | <= 2 levels | at frame 784, sun break |
| EV slope | INFO | 0.3852 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## s0880_0960 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_s0880_0960`, 961 frames at 30 fps, kind transition

- luma      `██████████████████████████▇▇▆▅▄▄▃▃▂▂▃▃▃▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▃▃▃▃▃▃▃▃▃▃▃▃▄▃▃▄▅▅▃▃▃▄▅▅▆▇▇██▅▃▂▂▂▂▂▂▂▂▂▂▁▂▂▅▅▁▁▁▃▃▁▁▁▃▃▁▁▁▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 2.4470 | <= 2.6 levels | at frame 458 |
| activation-frame diff | PASS | 0.1092 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 481 |
| EV slope | INFO | 5.3677 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## s1060_1080 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_s1060_1080`, 241 frames at 30 fps, kind transition

- luma      `█▇▇▆▆▅▄▄▃▂▂▂▁▁▁▁▁▂▂▃▄▄▄▄▅▅▆▆▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▆▆▅▅▅▄▄▄▄▄▄▃▃▃`
- mean diff `▆▅▆▆▅▅▅▅▅▄▄▄▄▄▄▅▆▇▇▆▇█▇█▅▆▆▆▆▃▃▂▃▃▄▄▄▄▅▅▆▇▇▆▆▄▃▃▃▂▁▁▂▂▂▁▁▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.4103 | <= 1.2 levels | at frame 85 |
| activation-frame diff | PASS | 0.0335 | <= 0.5 levels over the local median | 2 boundary frames, worst frame 121 |
| EV slope | INFO | 1.2869 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## s1405_1450 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_s1405_1450`, 541 frames at 30 fps, kind transition

- luma      `▁▁▁▂▂▃▂▂▂▂▃▄▅▆▆▆▆▆▆▇▇▇███████████████▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇▇`
- mean diff `▂▃▃▄▆▆▅▄▃▄▅▆▇▇▇█▂▃▂▂▃▄▁▂▁▁▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▁▁▁▁▁▁▂▂▁▁▁▁▂▁▂▂▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.9587 | <= 1.2 levels | at frame 113 |
| EV slope | INFO | 1.6126 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_start`, 60 frames at 30 fps, kind transition

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▃▃▃▄▄▅▅▆▆▇█`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▃▃▃▃▃▄▄▄▄▅▅▆▆▇▇█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.3690 | <= 1.2 levels | at frame 59 |
| EV slope | INFO | 41.3201 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |

## static_KS04 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▂▁▁▁▁▁▁▁▁▁▂▂▂▂▃▃▃▃▄▄▄▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇▇▇█████████`
- mean diff `▂▂▁▁▁▁▁▁▁▁▂▂▂▃▃▄▄▅▅▆▆▆▇▇▇██████████▇▇▇▇▇▇▇▇▇▇▆▆▆▆▆▅▅▅▅▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0422 | <= 1.2 levels | at frame 35 |
| EV slope | INFO | 0.0362 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 14.8441 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_KS08 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_KS08`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▂▂▂▂▂▃▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄▅▅▅▅▅▅▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇██████`
- mean diff `▄▄▄▄▄▄▄▃▃▃▂▃▂▄▃▃▂▂▂▁▂▃▃▂▃▄▃▃▅▄▄▅▅▆▇▇▆▇▆▆▆▇▆▇▇▇█▇▇▇█▇▇█▆▆▆▆▆▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0012 | <= 2 levels | at frame 2, sun break |
| EV slope | INFO | 0.0010 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.7805 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_KS20 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_KS20`, 120 frames at 30 fps, kind static

- luma      `██████▇▇▇▇▇▇▇▇▇▆▆▆▆▆▅▅▅▅▅▅▅▅▅▅▅▅▅▅▄▄▄▄▄▄▄▄▃▃▃▃▂▂▂▂▂▁▁▁▂▂▂▂▂▃`
- mean diff `▂▂▂▁▁▁▂▂▂▂▂▂▃▃▃▃▃▃▃▄▄▃▃▃▃▃▄▃▃▄▄▅▅▆▆▆▆▆▇▆▇▇▇▇▇▇▅▅▅▆▆▆▆▆▆▆▇▇██`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0005 | <= 1.2 levels | at frame 91 |
| EV slope | INFO | 0.0006 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 1.4566 | <= 1.5 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b01_WaveGallery (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_b01_WaveGallery`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁█`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▆█`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0042 | <= 1.2 levels | at frame 119 |
| EV slope | INFO | 0.0090 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.0873 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b02_Crest (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_b02_Crest`, 120 frames at 30 fps, kind static

- luma      `▅▅▆▇███▇▆▅▄▃▂▂▁▁▂▃▄▅▅▅▅▅▅▅▅▅▅▅▅▆▆▇▇█████████▇▇▇▆▆▆▆▆▆▆▆▆▅▅▅▅`
- mean diff `▅▅▆▆▆▆▅▇▇▆▆▅▄▃▂▂▅█▅▄▄▃▅▅▅▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0050 | <= 1.2 levels | at frame 34 |
| EV slope | INFO | 0.0044 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 3.0342 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b03_SpillwayBridge (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_b03_SpillwayBridge`, 120 frames at 30 fps, kind static

- luma      `▄▄▄▄▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▄▄▂▂▄▄▄▄▄▄▄▄▄▄▅▅▅▅▇▇▇▇▇▇▇▇▇▇█`
- mean diff `▁▁▃▃▃▃▃▃▁▁▁▃▁▃▁▃▁▁▁▁▁▁▃▁▁▃▃▃▃▁▃▁▆▅▅█▆▃▃▁▃▃▅▅▆▅▅▅▃▃▁▁▁▁▁▁▃▃▃▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0005 | <= 2 levels | at frame 9, sun break |
| EV slope | INFO | 0.0005 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.4335 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b04_ArmourSlope (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_b04_ArmourSlope`, 120 frames at 30 fps, kind static

- luma      `▇███████▇▇▇▇▇▇▇▇▇▇▆▆▆▅▅▄▄▃▃▃▃▂▂▂▂▂▂▂▁▁▁▁▁▂▂▂▂▂▃▃▃▃▄▄▄▅▅▆▆▆▆▆`
- mean diff `▇▅██▇▇█▆▇▆▆▇▆▆▆▇▆▇▇▆▇▆▅▇▆▆▇▆▆▆▅▅▅▄▄▄▄▄▁▃▄▃▃▃▃▄▄▃▄▅▆▆▆▆▇▇▇███`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0106 | <= 1.2 levels | at frame 105 |
| EV slope | INFO | 0.0084 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | FAIL | 19.6560 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b05_GateTower (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_b05_GateTower`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0000 | <= 1.2 levels | at frame 30 |
| EV slope | INFO | 0.0000 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.0053 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b06_WindowGallery (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_b06_WindowGallery`, 120 frames at 30 fps, kind static

- luma      `██▇▇▇▆▆▆▅▅▅▅▅▄▄▄▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁`
- mean diff `▄▄▄▄▄▃▃▂▂▂▂▁▁▁▁▁▁▁▁▂▂▂▂▂▃▃▃▄▄▅▅▅▅▅▅▆▅▅▅▅▆▇▇▇████▇▇▆▆▅▆▆███▇▇`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0020 | <= 1.2 levels | at frame 1 |
| EV slope | INFO | 0.0088 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 2.1737 | <= 3 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b07_SluiceLedge (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_b07_SluiceLedge`, 120 frames at 30 fps, kind static

- luma      `▁▁▁▁▂▂▂▂▂▃▃▃▃▃▅▅▅▆▆▆▇▇▇▇▇▇▇▇▇▆▆▆▆▆▆▆▆▆▅▅▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇███`
- mean diff `▃▅▃▃▃▅▁▁▃▅▃▃▃▅▅▅▅▅▅▅▆▆▆▅▆▅▅▃▅▅▆▆█▆▅▅▆▆▅▃▃▅██▆█▆▆▅▅▅▅▆▆▆▅▁▃▅▆`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0004 | <= 1.2 levels | at frame 36 |
| EV slope | INFO | 0.0006 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.9933 | <= 1.5 levels | 120 frames, 64 px blocks |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
