# Image metrics: SU_r09v1

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 16. Checks: 172 PASS, 23 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | 1 | 1920x1080 | 0.110 | 0.064 | 0.0001 | 1.263 | FAIL | banding[mouth_glow] |
| KS01_alley_noodle | 1 | 1920x1080 | 0.134 | 0.068 | 0.0089 | - | FAIL | NaN-like pixels |
| KS02_alley_slit | 1 | 1920x1080 | 0.115 | 0.068 | 0.0000 | 0.158 | PASS |  |
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.122 | 0.058 | 0.0000 | 1.170 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[alley_wall_dark] |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.164 | 0.051 | 0.0000 | 0.472 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up | 2 | 1920x1080 | 0.191 | 0.064 | 0.0000 | 0.288 | FAIL | NaN-like pixels |
| KS06_tram | 2 | 1920x1080 | 0.157 | 0.062 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window | 3 | 1920x1080 | 0.152 | 0.073 | 0.0009 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | 3 | 1920x1080 | 0.177 | 0.057 | 0.0016 | - | FAIL | NaN-like pixels |
| KS09_edge_gulf | 3 | 1920x1080 | 0.152 | 0.054 | 0.0000 | 0.422 | FAIL | NaN-like pixels |
| KS10_edge_plunge | 3 | 1920x1080 | 0.134 | 0.050 | 0.0000 | 0.482 | PASS |  |
| KS11_bridge_train | 4 | 1920x1080 | 0.162 | 0.062 | 0.0024 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void | 4 | 1920x1080 | 0.164 | 0.057 | 0.0000 | 0.170 | FAIL | NaN-like pixels |
| KS13_bridge_glass | 4 | 1920x1080 | 0.208 | 0.116 | 0.0006 | 0.092 | FAIL | mean luma |
| KS14_garden_gate | 4 | 1920x1080 | 0.122 | 0.050 | 0.0094 | - | FAIL | NaN-like pixels |
| KS15_garden_canopy | 5 | 1920x1080 | 0.079 | 0.046 | 0.0000 | - | PASS |  |
| KS16_signs_wall | 6 | 1920x1080 | 0.218 | 0.051 | 0.0000 | 0.108 | FAIL | NaN-like pixels |
| KS17_heart_reveal | 7 | 1920x1080 | 0.183 | 0.070 | 0.0000 | 2.000 | FAIL | NaN-like pixels, banding[plaza] |
| KS18_koi_pass | 7 | 1920x1080 | 0.215 | 0.082 | 0.0000 | 0.221 | FAIL | NaN-like pixels |
| KS19_goal_window | 7 | 1920x1080 | 0.135 | 0.051 | 0.0020 | 0.091 | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS00_alley_start | 1920x1080 | INFO |
| KS01_alley_noodle | 1920x1080 | INFO |
| KS02_alley_slit | 1920x1080 | INFO |
| KS03_boulevard_mouth | 1920x1080 | INFO |
| KS04_boulevard_axis | 1920x1080 | INFO |
| KS06_tram | 1920x1080 | INFO |
| KS05_boulevard_up | 1920x1080 | INFO |
| KS07_edge_window | 1920x1080 | INFO |
| KS08_edge_flank | 1920x1080 | INFO |
| KS09_edge_gulf | 1920x1080 | INFO |
| KS10_edge_plunge | 1920x1080 | INFO |
| KS11_bridge_train | 1920x1080 | INFO |
| KS12_bridge_void | 1920x1080 | INFO |
| KS13_bridge_glass | 1920x1080 | INFO |
| KS14_garden_gate | 1920x1080 | INFO |
| KS15_garden_canopy | 1920x1080 | INFO |
| KS16_signs_wall | 1920x1080 | INFO |
| KS17_heart_reveal | 1920x1080 | INFO |
| KS18_koi_pass | 1920x1080 | INFO |
| KS19_goal_window | 1920x1080 | INFO |

## Details

### KS00_alley_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.703 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0644 | >= 0.0353 | display |
| mean luma | PASS | 0.1104 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 40.5 codes): objects on the horizon |
| banding[mouth_glow] | FAIL | 1.2630 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.89 (run 4 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.3230 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.48 (run 7 px), - / -, 6 strips |

### KS01_alley_noodle (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[951, 0], [1144, 392]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.600 % of the frame |
| clipped highlights | PASS | 0.0089 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0681 | >= 0.0353 | display |
| mean luma | PASS | 0.1335 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 205.0 codes): objects on the horizon |

### KS02_alley_slit (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS02_alley_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.404 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0675 | >= 0.0353 | display |
| mean luma | PASS | 0.1151 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.1580 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.24 (run 7 px), - / -, 1 strips |

### KS03_boulevard_mouth (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[427, 515]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[880, 745], [1259, 745], [1414, 745]]; neon mask 1.456 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0580 | >= 0.0353 | display |
| mean luma | PASS | 0.1216 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 215.0 codes): objects on the horizon |
| banding[alley_wall_dark] | FAIL | 1.1700 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.76 (run 5 px), - / -, 4 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[870, 521], [883, 573], [1029, 861]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1503, 885]]; neon mask 1.191 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0513 | >= 0.0353 | display |
| mean luma | PASS | 0.1637 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 876.0000 |  | sky band not uniform (spread 24.0 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.4720 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.71 (run 7 px), - / -, 3 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 8 | 0 | bright 4 (mode all), dark 4, first at [[1899, 145], [1485, 158], [883, 815], [887, 857], [892, 910]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.045 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0636 | >= 0.0353 | display |
| mean luma | PASS | 0.1909 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.2880 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.43 (run 7 px), - / -, 4 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 8 | 0 | bright 4 (mode all), dark 4, first at [[1839, 163], [1824, 167], [1860, 174], [1673, 289], [1409, 725]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[875, 734]]; neon mask 1.057 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1565 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 211.0 codes): objects on the horizon |

### KS07_edge_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS07_edge_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 3 (mode all), dark 2, first at [[874, 290], [1112, 391], [1117, 391], [931, 584], [988, 584]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[874, 290]]; neon mask 1.749 % of the frame |
| clipped highlights | PASS | 0.0009 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0728 | >= 0.0353 | display |
| mean luma | PASS | 0.1520 | 0.0941..0.1961 | display |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 32 | 0 | bright 0 (mode all), dark 32, first at [[456, 232], [429, 351], [418, 385], [417, 388], [1560, 542]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.471 % of the frame |
| clipped highlights | PASS | 0.0016 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0569 | >= 0.0353 | display |
| mean luma | PASS | 0.1767 | 0.0941..0.1961 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 0 (mode all), dark 13, first at [[1317, 43], [1275, 69], [1330, 104], [1278, 112], [1276, 118]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.963 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0541 | >= 0.0353 | display |
| mean luma | PASS | 0.1520 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.4220 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.63 (run 6 px), - / -, 4 strips |

### KS10_edge_plunge (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.421 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0502 | >= 0.0353 | display |
| mean luma | PASS | 0.1338 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.4820 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.72 (run 7 px), - / -, 4 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[15, 50]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1086, 101], [1087, 101]]; neon mask 1.041 % of the frame |
| clipped highlights | PASS | 0.0024 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1624 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 0 (mode all), dark 4, first at [[1465, 575], [1498, 597], [1493, 607], [1492, 613]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.790 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0574 | >= 0.0353 | display |
| mean luma | PASS | 0.1643 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.1700 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.25 (run 7 px), - / -, 3 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.065 % of the frame |
| clipped highlights | PASS | 0.0006 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.1165 | >= 0.0353 | display |
| mean luma | FAIL | 0.2083 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.0920 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.14 (run 6 px), - / -, 2 strips |

### KS14_garden_gate (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[72, 68]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.396 % of the frame |
| clipped highlights | PASS | 0.0094 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0499 | >= 0.0353 | display |
| mean luma | PASS | 0.1219 | 0.0941..0.1961 | display |

### KS15_garden_canopy (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.509 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0462 | >= 0.0353 | display |
| mean luma | PASS | 0.0794 | 0.0784..0.1725 | display |

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS16_signs_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 0 (mode all), dark 3, first at [[1499, 983], [1520, 1009], [1492, 1020]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.131 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0507 | >= 0.0353 | display |
| mean luma | PASS | 0.2184 | 0.1255..0.251 | display |
| banding[wall_panel] | PASS | 0.1080 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.16 (run 7 px), - / -, 6 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS17_heart_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[255, 377]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.082 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0703 | >= 0.0353 | display |
| mean luma | PASS | 0.1833 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.3420 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.51 (run 7 px), - / -, 6 strips |
| banding[plaza] | FAIL | 2.0000 | score <= 1 | visible steps/1000 8.33 (max 1.18 code), BandScore 0.76 (run 9 px), - / -, 22 strips |

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 15 | 0 | bright 0 (mode all), dark 15, first at [[1237, 803], [1228, 813], [1254, 868], [1737, 889], [1739, 892]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.053 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0821 | >= 0.0353 | display |
| mean luma | PASS | 0.2154 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.2210 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.33 (run 8 px), - / -, 9 strips |

### KS19_goal_window (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\SU_r09v1\KS19_goal_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.358 % of the frame |
| clipped highlights | PASS | 0.0020 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0513 | >= 0.0353 | display |
| mean luma | PASS | 0.1352 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.0910 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.14 (run 6 px), - / -, 2 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
