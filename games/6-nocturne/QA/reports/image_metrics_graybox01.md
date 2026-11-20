# Image metrics: graybox01

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 17. Checks: 174 PASS, 23 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | 1 | 1920x1080 | 0.073 | 0.042 | 0.0000 | 0.556 | FAIL | mean luma |
| KS01_alley_noodle | 1 | 1920x1080 | 0.100 | 0.045 | 0.0093 | - | FAIL | NaN-like pixels, mean luma |
| KS02_alley_slit | 1 | 1920x1080 | 0.108 | 0.048 | 0.0000 | 0.113 | FAIL | mean luma |
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.104 | 0.046 | 0.0000 | 0.097 | FAIL | isolated pixels > 8x median, mean luma |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.190 | 0.048 | 0.0218 | 0.155 | FAIL | isolated pixels > 8x median |
| KS05_boulevard_up | 2 | 1920x1080 | 0.238 | 0.057 | 0.0000 | 0.233 | FAIL | NaN-like pixels |
| KS06_tram | 2 | 1920x1080 | 0.184 | 0.050 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window | 3 | 1920x1080 | 0.164 | 0.062 | 0.0064 | 0.303 | FAIL | NaN-like pixels |
| KS08_edge_flank | 3 | 1920x1080 | 0.217 | 0.052 | 0.0007 | - | FAIL | NaN-like pixels |
| KS09_edge_gulf | 3 | 1920x1080 | 0.148 | 0.050 | 0.0000 | 0.452 | FAIL | NaN-like pixels |
| KS10_edge_plunge | 3 | 1920x1080 | 0.120 | 0.048 | 0.0000 | 0.320 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train | 4 | 1920x1080 | 0.173 | 0.053 | 0.0011 | - | FAIL | NaN-like pixels |
| KS12_bridge_void | 4 | 1920x1080 | 0.147 | 0.049 | 0.0000 | 0.186 | FAIL | NaN-like pixels |
| KS13_bridge_glass | 4 | 1920x1080 | 0.066 | 0.050 | 0.0000 | 0.111 | FAIL | mean luma |
| KS14_garden_gate | 4 | 1920x1080 | 0.166 | 0.056 | 0.0024 | - | PASS |  |
| KS15_garden_canopy | 5 | 1920x1080 | 0.211 | 0.041 | 0.0000 | 0.583 | PASS |  |
| KS16_signs_wall | 6 | 1920x1080 | 0.255 | 0.068 | 0.0000 | 0.151 | PASS |  |
| KS17_heart_reveal | 7 | 1920x1080 | 0.220 | 0.061 | 0.0049 | 1.583 | FAIL | NaN-like pixels, banding[plaza] |
| KS18_koi_pass | 7 | 1920x1080 | 0.317 | 0.089 | 0.0196 | 0.119 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS19_goal_window | 7 | 1920x1080 | 0.106 | 0.053 | 0.0021 | 0.000 | FAIL | mean luma |

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

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.904 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0421 | >= 0.0353 | display |
| mean luma | FAIL | 0.0731 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 33.5 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.5560 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.83 (run 6 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.00 (run 0 px), - / -, 6 strips |

### KS01_alley_noodle (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[951, 0]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.604 % of the frame |
| clipped highlights | PASS | 0.0093 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0445 | >= 0.0353 | display |
| mean luma | FAIL | 0.0996 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 198.0 codes): objects on the horizon |

### KS02_alley_slit (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS02_alley_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.279 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0476 | >= 0.0353 | display |
| mean luma | FAIL | 0.1083 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.1130 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.17 (run 8 px), - / -, 1 strips |

### KS03_boulevard_mouth (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1008, 488]]; neon mask 1.175 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0463 | >= 0.0353 | display |
| mean luma | FAIL | 0.1041 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 756.7000 |  | sky band not uniform (spread 210.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.0970 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.14 (run 6 px), - / -, 4 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[233, 510]]; neon mask 0.827 % of the frame |
| clipped highlights | PASS | 0.0218 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0482 | >= 0.0353 | display |
| mean luma | PASS | 0.1904 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | 876.0000 |  | sky band not uniform (spread 40.0 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.1550 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.23 (run 6 px), - / -, 3 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 1 (mode all), dark 10, first at [[135, 122], [1829, 144], [1899, 145], [1902, 152], [91, 161]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.604 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0566 | >= 0.0353 | display |
| mean luma | PASS | 0.2381 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.2330 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.35 (run 7 px), - / -, 4 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 2 (mode all), dark 11, first at [[1421, 142], [1839, 163], [1824, 167], [1860, 174], [1897, 187]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[258, 558], [290, 563]]; neon mask 0.902 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0496 | >= 0.0353 | display |
| mean luma | PASS | 0.1843 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 208.0 codes): objects on the horizon |

### KS07_edge_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS07_edge_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 2 (mode all), dark 2, first at [[1616, 199], [1621, 200], [847, 320], [931, 584]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.196 % of the frame |
| clipped highlights | PASS | 0.0064 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | PASS | 0.1644 | 0.1176..0.3137 | display |
| banding[sky_above_gulf] | PASS | 0.3030 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.46 (run 7 px), - / -, 2 strips |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS08_edge_flank.png`

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

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 34 | 0 | bright 0 (mode all), dark 34, first at [[1412, 16], [1409, 17], [1386, 25], [1357, 35], [1317, 43]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.095 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0502 | >= 0.0353 | display |
| mean luma | PASS | 0.1479 | 0.1176..0.3137 | display |
| banding[abyss] | PASS | 0.4520 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.68 (run 8 px), - / -, 4 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[326, 517]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[9, 214]]; neon mask 0.842 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0480 | >= 0.0353 | display |
| mean luma | PASS | 0.1205 | 0.1176..0.3137 | display |
| banding[abyss] | PASS | 0.3200 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.48 (run 9 px), - / -, 4 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[15, 50], [31, 52]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.328 % of the frame |
| clipped highlights | PASS | 0.0011 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0533 | >= 0.0353 | display |
| mean luma | PASS | 0.1730 | 0.1098..0.2941 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 0 (mode all), dark 11, first at [[1440, 89], [1617, 229], [1394, 386], [1583, 556], [1465, 575]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.900 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0490 | >= 0.0353 | display |
| mean luma | PASS | 0.1467 | 0.1098..0.2941 | display |
| banding[abyss] | PASS | 0.1860 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.28 (run 6 px), - / -, 3 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS13_bridge_glass.png`

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

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.325 % of the frame |
| clipped highlights | PASS | 0.0024 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0563 | >= 0.0353 | display |
| mean luma | PASS | 0.1662 | 0.1098..0.2941 | display |

### KS15_garden_canopy (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.691 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0412 | >= 0.0353 | display |
| mean luma | PASS | 0.2110 | 0.0941..0.251 | display |
| banding[crown_gap_sky] | PASS | 0.5830 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.88 (run 7 px), - / -, 2 strips |

### KS16_signs_wall (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS16_signs_wall.png`

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
| banding[wall_panel] | PASS | 0.1510 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.23 (run 9 px), - / -, 6 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS17_heart_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[255, 377]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.096 % of the frame |
| clipped highlights | PASS | 0.0049 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0608 | >= 0.0353 | display |
| mean luma | PASS | 0.2198 | 0.1333..0.3333 | display |
| banding[sky_koi_spire] | PASS | 0.3480 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.52 (run 10 px), - / -, 6 strips |
| banding[plaza] | FAIL | 1.5830 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.37 (run 10 px), - / -, 24 strips |

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 15 | 0 | bright 0 (mode all), dark 15, first at [[1237, 803], [1254, 868], [1737, 889], [1739, 892], [1299, 896]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1341, 972], [1342, 975], [1343, 978]]; neon mask 1.967 % of the frame |
| clipped highlights | PASS | 0.0196 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0894 | >= 0.0353 | display |
| mean luma | PASS | 0.3165 | 0.1333..0.3333 | display |
| banding[sky_koi] | PASS | 0.1190 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.18 (run 7 px), - / -, 9 strips |

### KS19_goal_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01\KS19_goal_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.124 % of the frame |
| clipped highlights | PASS | 0.0021 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0530 | >= 0.0353 | display |
| mean luma | FAIL | 0.1062 | 0.1333..0.3333 | display |
| banding[tenement_wall] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.00 (run 0 px), - / -, 2 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
