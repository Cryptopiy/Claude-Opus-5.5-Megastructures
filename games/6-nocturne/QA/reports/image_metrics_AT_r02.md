# Image metrics: AT_r02

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 16. Checks: 169 PASS, 26 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | 1 | 1920x1080 | 0.106 | 0.062 | 0.0000 | 0.201 | PASS |  |
| KS01_alley_noodle | 1 | 1920x1080 | 0.126 | 0.064 | 0.0087 | - | PASS |  |
| KS02_alley_slit | 1 | 1920x1080 | 0.105 | 0.063 | 0.0000 | 0.556 | PASS |  |
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.098 | 0.059 | 0.0000 | 0.102 | FAIL | NaN-like pixels |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.139 | 0.060 | 0.0098 | 0.459 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up | 2 | 1920x1080 | 0.170 | 0.067 | 0.0011 | 0.383 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram | 2 | 1920x1080 | 0.133 | 0.062 | 0.0014 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window | 3 | 1920x1080 | 0.123 | 0.074 | 0.0054 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | 3 | 1920x1080 | 0.151 | 0.062 | 0.0065 | - | FAIL | NaN-like pixels |
| KS09_edge_gulf | 3 | 1920x1080 | 0.133 | 0.063 | 0.0000 | 0.373 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS10_edge_plunge | 3 | 1920x1080 | 0.132 | 0.059 | 0.0048 | 0.787 | FAIL | NaN-like pixels |
| KS11_bridge_train | 4 | 1920x1080 | 0.116 | 0.066 | 0.0148 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void | 4 | 1920x1080 | 0.125 | 0.069 | 0.0000 | 1.031 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS13_bridge_glass | 4 | 1920x1080 | 0.142 | 0.068 | 0.0009 | 2.635 | FAIL | banding[abyss] |
| KS14_garden_gate | 4 | 1920x1080 | 0.103 | 0.051 | 0.0036 | - | PASS |  |
| KS15_garden_canopy | 5 | 1920x1080 | 0.068 | 0.047 | 0.0000 | - | FAIL | mean luma |
| KS16_signs_wall | 6 | 1920x1080 | 0.171 | 0.052 | 0.0000 | 0.108 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS17_heart_reveal | 7 | 1920x1080 | 0.151 | 0.057 | 0.0000 | 0.956 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS18_koi_pass | 7 | 1920x1080 | 0.197 | 0.070 | 0.0000 | 0.596 | FAIL | isolated pixels > 8x median |
| KS19_goal_window | 7 | 1920x1080 | 0.121 | 0.058 | 0.0032 | 0.072 | FAIL | NaN-like pixels |

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

### KS00_alley_start (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.508 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1058 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 43.5 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.1760 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.26 (run 5 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.2010 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.30 (run 6 px), - / -, 6 strips |

### KS01_alley_noodle (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.410 % of the frame |
| clipped highlights | PASS | 0.0087 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0641 | >= 0.0353 | display |
| mean luma | PASS | 0.1256 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 167.5 codes): objects on the horizon |

### KS02_alley_slit (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS02_alley_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.362 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | PASS | 0.1047 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.5560 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.83 (run 10 px), - / -, 1 strips |

### KS03_boulevard_mouth (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1669, 755]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.985 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.0984 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 140.5 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.1020 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.15 (run 7 px), - / -, 2 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 25 | 0 | bright 9 (mode all), dark 16, first at [[870, 521], [881, 540], [883, 573], [889, 628], [135, 749]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[632, 636], [632, 637], [1809, 811], [1810, 814]]; neon mask 2.339 % of the frame |
| clipped highlights | PASS | 0.0098 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0600 | >= 0.0353 | display |
| mean luma | PASS | 0.1386 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 876.0000 |  | sky band not uniform (spread 113.0 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.4590 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.69 (run 8 px), - / -, 3 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 5 (mode all), dark 1, first at [[1485, 158], [883, 815], [887, 857], [892, 910], [877, 1074]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[862, 977]]; neon mask 2.573 % of the frame |
| clipped highlights | PASS | 0.0011 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1700 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.3830 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.57 (run 7 px), - / -, 4 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 10 | 0 | bright 3 (mode all), dark 7, first at [[1061, 536], [1409, 725], [541, 738], [630, 746], [540, 747]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1108, 529], [339, 680], [555, 689]]; neon mask 2.114 % of the frame |
| clipped highlights | PASS | 0.0014 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1330 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 117.0 codes): objects on the horizon |

### KS07_edge_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS07_edge_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[872, 222], [874, 290], [944, 577], [975, 577], [988, 584]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[872, 222], [874, 290], [881, 312]]; neon mask 3.417 % of the frame |
| clipped highlights | PASS | 0.0054 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0737 | >= 0.0353 | display |
| mean luma | PASS | 0.1226 | 0.0941..0.1961 | display |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 8 | 0 | bright 4 (mode all), dark 4, first at [[194, 303], [198, 304], [206, 306], [210, 307], [704, 614]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 3.386 % of the frame |
| clipped highlights | PASS | 0.0065 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1507 | 0.0941..0.1961 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 8 | 0 | bright 8 (mode all), dark 0, first at [[1078, 479], [998, 480], [1041, 480], [920, 487], [989, 488]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[865, 492]]; neon mask 2.218 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0628 | >= 0.0353 | display |
| mean luma | PASS | 0.1330 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.3730 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.56 (run 7 px), - / -, 4 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 3 (mode all), dark 1, first at [[879, 290], [953, 296], [867, 300], [1026, 302]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.296 % of the frame |
| clipped highlights | PASS | 0.0048 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.1321 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.7870 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.18 (run 7 px), - / -, 4 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 1 (mode all), dark 1, first at [[1022, 508], [1419, 605]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[1086, 101], [1087, 101], [547, 260], [547, 262]]; neon mask 1.807 % of the frame |
| clipped highlights | PASS | 0.0148 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0664 | >= 0.0353 | display |
| mean luma | PASS | 0.1159 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 12 | 0 | bright 12 (mode all), dark 0, first at [[1330, 12], [1327, 16], [557, 469], [588, 469], [619, 469]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1616, 227], [1567, 231]]; neon mask 1.949 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0686 | >= 0.0353 | display |
| mean luma | PASS | 0.1253 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.0310 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.55 (run 7 px), - / -, 3 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.188 % of the frame |
| clipped highlights | PASS | 0.0009 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0683 | >= 0.0353 | display |
| mean luma | PASS | 0.1417 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 2.6350 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 3.95 (run 6 px), - / -, 1 strips |

### KS14_garden_gate (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.226 % of the frame |
| clipped highlights | PASS | 0.0036 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0510 | >= 0.0353 | display |
| mean luma | PASS | 0.1025 | 0.0941..0.1961 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.136 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0468 | >= 0.0353 | display |
| mean luma | FAIL | 0.0681 | 0.0784..0.1725 | display |

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS16_signs_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 2 (mode all), dark 2, first at [[1841, 1019], [1743, 1064], [1412, 1075], [1413, 1079]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[471, 950], [708, 980], [785, 990], [866, 1000]]; neon mask 0.240 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0515 | >= 0.0353 | display |
| mean luma | PASS | 0.1713 | 0.1255..0.251 | display |
| banding[wall_panel] | PASS | 0.1080 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.16 (run 5 px), - / -, 3 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS17_heart_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 7 | 0 | bright 6 (mode all), dark 1, first at [[1467, 643], [653, 756], [756, 780], [771, 784], [952, 788]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[909, 534], [1704, 736], [1616, 744]]; neon mask 2.304 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0572 | >= 0.0353 | display |
| mean luma | PASS | 0.1514 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.9560 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.43 (run 11 px), - / -, 6 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.01 (run 9 px) not gated (textured: gate steps), - / -, 22 strips |

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 64 | 0 outside neon masks | first at [[1231, 790], [1206, 796], [1203, 797], [1201, 798], [1180, 809]]; neon mask 2.231 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0698 | >= 0.0353 | display |
| mean luma | PASS | 0.1971 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.5960 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.89 (run 9 px), - / -, 9 strips |

### KS19_goal_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_r02\KS19_goal_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 14 | 0 | bright 0 (mode all), dark 14, first at [[775, 558], [783, 575], [800, 613], [778, 668], [864, 692]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.568 % of the frame |
| clipped highlights | PASS | 0.0032 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.1212 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.0720 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.11 (run 5 px), - / -, 2 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
