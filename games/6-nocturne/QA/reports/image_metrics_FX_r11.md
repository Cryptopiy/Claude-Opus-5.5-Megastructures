# Image metrics: FX_r11

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 19. Checks: 160 PASS, 35 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | 1 | 1920x1080 | 0.122 | 0.065 | 0.0018 | 0.480 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS01_alley_noodle | 1 | 1920x1080 | 0.159 | 0.067 | 0.0102 | - | FAIL | NaN-like pixels, mean luma |
| KS02_alley_slit | 1 | 1920x1080 | 0.109 | 0.064 | 0.0000 | 0.336 | PASS |  |
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.099 | 0.059 | 0.0000 | 0.102 | FAIL | NaN-like pixels |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.143 | 0.060 | 0.0122 | 0.609 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up | 2 | 1920x1080 | 0.169 | 0.067 | 0.0011 | 0.478 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram | 2 | 1920x1080 | 0.138 | 0.062 | 0.0015 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window | 3 | 1920x1080 | 0.131 | 0.074 | 0.0054 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | 3 | 1920x1080 | 0.154 | 0.062 | 0.0065 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS09_edge_gulf | 3 | 1920x1080 | 0.134 | 0.063 | 0.0001 | 0.389 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS10_edge_plunge | 3 | 1920x1080 | 0.131 | 0.059 | 0.0048 | 0.638 | FAIL | NaN-like pixels |
| KS11_bridge_train | 4 | 1920x1080 | 0.120 | 0.067 | 0.0195 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void | 4 | 1920x1080 | 0.125 | 0.063 | 0.0000 | 1.489 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS13_bridge_glass | 4 | 1920x1080 | 0.137 | 0.068 | 0.0004 | 1.155 | FAIL | NaN-like pixels, banding[abyss] |
| KS14_garden_gate | 4 | 1920x1080 | 0.124 | 0.056 | 0.0036 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy | 5 | 1920x1080 | 0.126 | 0.054 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS16_signs_wall | 6 | 1920x1080 | 0.199 | 0.062 | 0.0000 | 2.033 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[wall_panel] |
| KS17_heart_reveal | 7 | 1920x1080 | 0.151 | 0.057 | 0.0000 | 0.776 | FAIL | NaN-like pixels |
| KS18_koi_pass | 7 | 1920x1080 | 0.196 | 0.070 | 0.0000 | 0.577 | FAIL | isolated pixels > 8x median |
| KS19_goal_window | 7 | 1920x1080 | 0.121 | 0.058 | 0.0032 | 0.073 | FAIL | NaN-like pixels |

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

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[1073, 469], [977, 472]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1088, 137], [1087, 287]]; neon mask 0.097 % of the frame |
| clipped highlights | PASS | 0.0018 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0650 | >= 0.0353 | display |
| mean luma | PASS | 0.1216 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 52.0 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.1580 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.24 (run 6 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.4800 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.72 (run 6 px), - / -, 6 strips |

### KS01_alley_noodle (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 10 | 0 | bright 0 (mode all), dark 10, first at [[1919, 240], [1909, 274], [1914, 357], [1899, 362], [1214, 520]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.103 % of the frame |
| clipped highlights | PASS | 0.0102 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | FAIL | 0.1591 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 101.0 codes): objects on the horizon |

### KS02_alley_slit (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS02_alley_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.156 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0636 | >= 0.0353 | display |
| mean luma | PASS | 0.1091 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.3360 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.50 (run 7 px), - / -, 1 strips |

### KS03_boulevard_mouth (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1669, 755]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.889 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.0995 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 121.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.1020 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.15 (run 7 px), - / -, 2 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 32 | 0 | bright 6 (mode all), dark 26, first at [[904, 569], [882, 619], [572, 711], [556, 902], [1413, 904]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[513, 901], [513, 902], [1787, 945], [1057, 960]]; neon mask 2.759 % of the frame |
| clipped highlights | PASS | 0.0122 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0600 | >= 0.0353 | display |
| mean luma | PASS | 0.1435 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 963.7000 |  | sky band not uniform (spread 150.5 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.6090 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.91 (run 7 px), - / -, 3 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 5 (mode all), dark 1, first at [[1485, 158], [883, 815], [887, 857], [1431, 1008], [877, 1074]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[862, 977]]; neon mask 2.840 % of the frame |
| clipped highlights | PASS | 0.0011 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1692 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.4780 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.72 (run 9 px), - / -, 4 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 12 | 0 | bright 4 (mode all), dark 8, first at [[1830, 127], [1061, 536], [1409, 725], [541, 738], [630, 746]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[339, 680], [555, 689]]; neon mask 2.436 % of the frame |
| clipped highlights | PASS | 0.0015 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1382 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 117.0 codes): objects on the horizon |

### KS07_edge_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS07_edge_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[872, 222], [874, 290], [931, 584], [988, 584]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[872, 222], [881, 312]]; neon mask 4.091 % of the frame |
| clipped highlights | PASS | 0.0054 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0742 | >= 0.0353 | display |
| mean luma | PASS | 0.1305 | 0.0941..0.1961 | display |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[680, 648], [989, 790]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[1456, 584], [1456, 587], [1460, 587], [1461, 587]]; neon mask 4.099 % of the frame |
| clipped highlights | PASS | 0.0065 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1538 | 0.0941..0.1961 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[1126, 55], [1062, 112], [1181, 271], [1023, 478], [998, 480]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[865, 492]]; neon mask 2.965 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | PASS | 0.1342 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.3890 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.58 (run 6 px), - / -, 4 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 3 (mode all), dark 1, first at [[879, 290], [1026, 302], [1427, 341], [762, 422]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.668 % of the frame |
| clipped highlights | PASS | 0.0048 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0588 | >= 0.0353 | display |
| mean luma | PASS | 0.1307 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.6380 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.96 (run 10 px), - / -, 4 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1419, 605]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1086, 101], [1087, 101]]; neon mask 2.463 % of the frame |
| clipped highlights | PASS | 0.0195 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0670 | >= 0.0353 | display |
| mean luma | PASS | 0.1203 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1330, 12], [1327, 16], [546, 612], [548, 622]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[426, 118]]; neon mask 0.636 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | PASS | 0.1252 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.4890 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.23 (run 6 px), - / -, 3 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[887, 727]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.373 % of the frame |
| clipped highlights | PASS | 0.0004 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0683 | >= 0.0353 | display |
| mean luma | PASS | 0.1370 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.1550 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.73 (run 6 px), - / -, 1 strips |

### KS14_garden_gate (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 21 | 0 | bright 21 (mode all), dark 0, first at [[753, 15], [1820, 188], [218, 199], [1354, 205], [213, 209]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1580, 245], [1423, 494]]; neon mask 2.645 % of the frame |
| clipped highlights | PASS | 0.0036 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0563 | >= 0.0353 | display |
| mean luma | PASS | 0.1237 | 0.0941..0.1961 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 18 | 0 | bright 13 (mode all), dark 5, first at [[1919, 87], [1457, 115], [87, 332], [1599, 398], [752, 416]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1249, 1007]]; neon mask 3.121 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0544 | >= 0.0353 | display |
| mean luma | PASS | 0.1259 | 0.0784..0.1725 | display |

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS16_signs_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 7 | 0 | bright 4 (mode all), dark 3, first at [[279, 112], [22, 176], [1841, 1019], [1743, 1064], [1411, 1071]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[708, 980], [866, 1000]]; neon mask 0.607 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1994 | 0.1255..0.251 | display |
| banding[wall_panel] | FAIL | 2.0330 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 3.05 (run 8 px), - / -, 3 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS17_heart_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1467, 643]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.308 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0572 | >= 0.0353 | display |
| mean luma | PASS | 0.1505 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.7760 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.16 (run 11 px), - / -, 6 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.67 (run 9 px) not gated (textured: gate steps), - / -, 22 strips |

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 51 | 0 outside neon masks | first at [[1331, 941], [1332, 944], [1330, 945], [1333, 947], [1331, 948]]; neon mask 2.257 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0698 | >= 0.0353 | display |
| mean luma | PASS | 0.1964 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.5770 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.87 (run 9 px), - / -, 9 strips |

### KS19_goal_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r11\KS19_goal_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 0 (mode all), dark 11, first at [[775, 558], [783, 575], [800, 613], [778, 668], [864, 692]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.579 % of the frame |
| clipped highlights | PASS | 0.0032 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0580 | >= 0.0353 | display |
| mean luma | PASS | 0.1207 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.0730 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.11 (run 5 px), - / -, 2 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
