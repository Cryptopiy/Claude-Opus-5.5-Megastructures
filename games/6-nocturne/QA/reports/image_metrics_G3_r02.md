# Image metrics: G3_r02

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02`  
Scene constants: NocturneConstants.cs  
Images: 18, failing images: 17. Checks: 142 PASS, 34 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | 1 | 1920x1080 | 0.068 | 0.034 | 0.0000 | 3.703 | FAIL | luma p1, mean luma, banding[mouth_glow] |
| KS01_alley_noodle | 1 | 1920x1080 | 0.093 | 0.037 | 0.0093 | - | FAIL | NaN-like pixels, mean luma |
| KS02_alley_slit | 1 | 1920x1080 | 0.107 | 0.048 | 0.0000 | 14.560 | FAIL | mean luma, banding[slit_cloud] |
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.104 | 0.046 | 0.0000 | 5.571 | FAIL | NaN-like pixels, isolated pixels > 8x median, mean luma, banding[mouth_glow] |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.190 | 0.048 | 0.0218 | 6.126 | FAIL | isolated pixels > 8x median, banding[sky_above_facades] |
| KS05_boulevard_up | 2 | 1920x1080 | 0.238 | 0.057 | 0.0000 | 4.567 | FAIL | NaN-like pixels, banding[cloud_ceiling] |
| KS06_tram | 2 | 1920x1080 | 0.184 | 0.050 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | 3 | 1920x1080 | 0.217 | 0.052 | 0.0007 | - | FAIL | NaN-like pixels |
| KS09_edge_gulf | 3 | 1920x1080 | 0.150 | 0.050 | 0.0000 | 5.111 | FAIL | NaN-like pixels, banding[abyss] |
| KS10_edge_plunge | 3 | 1920x1080 | 0.126 | 0.049 | 0.0000 | 0.296 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train | 4 | 1920x1080 | 0.171 | 0.053 | 0.0011 | - | FAIL | NaN-like pixels |
| KS12_bridge_void | 4 | 1920x1080 | 0.141 | 0.049 | 0.0000 | 40.618 | FAIL | NaN-like pixels, banding[abyss] |
| KS13_bridge_glass | 4 | 1920x1080 | 0.066 | 0.050 | 0.0000 | 0.111 | FAIL | mean luma |
| KS14_garden_gate | 4 | 1920x1080 | 0.166 | 0.056 | 0.0025 | - | PASS |  |
| KS15_garden_canopy | 5 | 1920x1080 | 0.212 | 0.041 | 0.0000 | 1.510 | FAIL | banding[crown_glow] |
| KS16_signs_wall | 6 | 1920x1080 | 0.255 | 0.068 | 0.0000 | 2.000 | FAIL | banding[wall_panel] |
| KS17_heart_reveal | 7 | 1920x1080 | 0.220 | 0.061 | 0.0049 | 9.463 | FAIL | NaN-like pixels, banding[sky_koi_spire], banding[plaza] |
| KS18_koi_pass | 7 | 1920x1080 | 0.317 | 0.089 | 0.0196 | 10.934 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[sky_koi] |

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
| KS07_edge_window | - | INFO |
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
| KS19_goal_window | - | INFO |

## Details

### KS00_alley_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.674 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | FAIL | 0.0339 | >= 0.0353 | display |
| mean luma | FAIL | 0.0683 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 33.0 codes): objects on the horizon |
| banding[mouth_glow] | FAIL | 3.7030 | score <= 1 | visible steps/1000 2.65 (max 5.55 code), BandScore 4.40 (run 10 px), - / -, 9 strips |

### KS01_alley_noodle (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[951, 0]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.378 % of the frame |
| clipped highlights | PASS | 0.0093 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0367 | >= 0.0353 | display |
| mean luma | FAIL | 0.0934 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 197.0 codes): objects on the horizon |

### KS02_alley_slit (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS02_alley_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.243 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0476 | >= 0.0353 | display |
| mean luma | FAIL | 0.1075 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | FAIL | 14.5600 | score <= 1 | visible steps/1000 3.37 (max 8.25 code), BandScore 21.84 (run 14 px), - / -, 12 strips |

### KS03_boulevard_mouth (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[940, 655], [1485, 656]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1008, 488], [1189, 764]]; neon mask 1.188 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0463 | >= 0.0353 | display |
| mean luma | FAIL | 0.1041 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 756.7000 |  | sky band not uniform (spread 210.0 codes): objects on the horizon |
| banding[mouth_glow] | FAIL | 5.5710 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 8.36 (run 13 px), - / -, 18 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[233, 510]]; neon mask 0.834 % of the frame |
| clipped highlights | PASS | 0.0218 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0482 | >= 0.0353 | display |
| mean luma | PASS | 0.1903 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | 876.0000 |  | sky band not uniform (spread 40.0 codes): objects on the horizon |
| banding[sky_above_facades] | FAIL | 6.1260 | score <= 1 | visible steps/1000 2.50 (max 1.27 code), BandScore 9.19 (run 10 px), - / -, 27 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 1 (mode all), dark 10, first at [[135, 122], [1829, 144], [1899, 145], [1902, 152], [91, 161]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.584 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0566 | >= 0.0353 | display |
| mean luma | PASS | 0.2380 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | FAIL | 4.5670 | score <= 1 | visible steps/1000 2.06 (max 5.73 code), BandScore 6.85 (run 12 px), - / -, 24 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 2 (mode all), dark 11, first at [[1421, 142], [1839, 163], [1824, 167], [1860, 174], [1897, 187]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[258, 558], [290, 563]]; neon mask 0.927 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0496 | >= 0.0353 | display |
| mean luma | PASS | 0.1840 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 208.0 codes): objects on the horizon |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 58 | 0 | bright 0 (mode all), dark 58, first at [[456, 232], [429, 351], [418, 385], [417, 388], [1527, 541]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.683 % of the frame |
| clipped highlights | PASS | 0.0007 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0516 | >= 0.0353 | display |
| mean luma | PASS | 0.2166 | 0.1176..0.3137 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 34 | 0 | bright 0 (mode all), dark 34, first at [[1412, 16], [1409, 17], [1386, 25], [1357, 35], [1317, 43]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.089 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0502 | >= 0.0353 | display |
| mean luma | PASS | 0.1504 | 0.1176..0.3137 | display |
| banding[abyss] | FAIL | 5.1110 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 7.67 (run 10 px), - / -, 24 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[326, 517]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[9, 214]]; neon mask 0.839 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0488 | >= 0.0353 | display |
| mean luma | PASS | 0.1255 | 0.1176..0.3137 | display |
| banding[abyss] | PASS | 0.2960 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.44 (run 8 px), - / -, 24 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[15, 50], [31, 52]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.226 % of the frame |
| clipped highlights | PASS | 0.0011 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0533 | >= 0.0353 | display |
| mean luma | PASS | 0.1715 | 0.1098..0.2941 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 0 (mode all), dark 11, first at [[1440, 89], [1617, 229], [1394, 386], [1583, 556], [1465, 575]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.570 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0490 | >= 0.0353 | display |
| mean luma | PASS | 0.1411 | 0.1098..0.2941 | display |
| banding[abyss] | FAIL | 40.6180 | score <= 1 | visible steps/1000 2.65 (max 60.93 code), BandScore 6.42 (run 10 px), - / -, 24 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.000 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0504 | >= 0.0353 | display |
| mean luma | FAIL | 0.0659 | 0.1098..0.2941 | display |
| banding[abyss] | PASS | 0.1110 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.17 (run 7 px), - / -, 24 strips |

### KS14_garden_gate (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.305 % of the frame |
| clipped highlights | PASS | 0.0025 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0563 | >= 0.0353 | display |
| mean luma | PASS | 0.1662 | 0.1098..0.2941 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.700 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0412 | >= 0.0353 | display |
| mean luma | PASS | 0.2118 | 0.0941..0.251 | display |
| banding[crown_glow] | FAIL | 1.5100 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.26 (run 10 px), - / -, 21 strips |

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS16_signs_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.103 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0678 | >= 0.0353 | display |
| mean luma | PASS | 0.2546 | 0.1961..0.4314 | display |
| banding[wall_panel] | FAIL | 2.0000 | score <= 1 | visible steps/1000 1.85 (max 1.50 code), BandScore 2.89 (run 10 px), - / -, 24 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS17_heart_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[255, 377]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.093 % of the frame |
| clipped highlights | PASS | 0.0049 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0608 | >= 0.0353 | display |
| mean luma | PASS | 0.2196 | 0.1333..0.3333 | display |
| banding[sky_koi_spire] | FAIL | 9.4630 | score <= 1 | visible steps/1000 2.89 (max 1.50 code), BandScore 14.19 (run 12 px), - / -, 16 strips |
| banding[plaza] | FAIL | 1.5830 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.37 (run 10 px), - / -, 24 strips |

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G3_r02\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 15 | 0 | bright 0 (mode all), dark 15, first at [[1237, 803], [1254, 868], [1737, 889], [1739, 892], [1299, 896]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1341, 972], [1342, 975], [1343, 978]]; neon mask 1.967 % of the frame |
| clipped highlights | PASS | 0.0196 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0893 | >= 0.0353 | display |
| mean luma | PASS | 0.3174 | 0.1333..0.3333 | display |
| banding[sky_koi] | FAIL | 10.9340 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 16.40 (run 11 px), - / -, 27 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
