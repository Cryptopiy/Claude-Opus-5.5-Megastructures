# Image metrics: FX_r06

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 15. Checks: 171 PASS, 24 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | 1 | 1920x1080 | 0.106 | 0.062 | 0.0000 | 0.200 | PASS |  |
| KS01_alley_noodle | 1 | 1920x1080 | 0.125 | 0.064 | 0.0087 | - | PASS |  |
| KS02_alley_slit | 1 | 1920x1080 | 0.105 | 0.063 | 0.0000 | 0.240 | PASS |  |
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.098 | 0.059 | 0.0000 | 0.105 | FAIL | NaN-like pixels |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.138 | 0.060 | 0.0015 | 0.459 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up | 2 | 1920x1080 | 0.168 | 0.067 | 0.0001 | 0.364 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram | 2 | 1920x1080 | 0.132 | 0.062 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window | 3 | 1920x1080 | 0.122 | 0.073 | 0.0011 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | 3 | 1920x1080 | 0.150 | 0.062 | 0.0031 | - | PASS |  |
| KS09_edge_gulf | 3 | 1920x1080 | 0.135 | 0.063 | 0.0000 | 0.692 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS10_edge_plunge | 3 | 1920x1080 | 0.132 | 0.059 | 0.0030 | 0.511 | FAIL | NaN-like pixels |
| KS11_bridge_train | 4 | 1920x1080 | 0.115 | 0.067 | 0.0087 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void | 4 | 1920x1080 | 0.123 | 0.070 | 0.0000 | 0.584 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS13_bridge_glass | 4 | 1920x1080 | 0.145 | 0.068 | 0.0006 | 1.037 | FAIL | banding[abyss] |
| KS14_garden_gate | 4 | 1920x1080 | 0.100 | 0.050 | 0.0036 | - | PASS |  |
| KS15_garden_canopy | 5 | 1920x1080 | 0.068 | 0.047 | 0.0000 | - | FAIL | mean luma |
| KS16_signs_wall | 6 | 1920x1080 | 0.171 | 0.051 | 0.0000 | 0.109 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS17_heart_reveal | 7 | 1920x1080 | 0.149 | 0.057 | 0.0000 | 0.607 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS18_koi_pass | 7 | 1920x1080 | 0.196 | 0.070 | 0.0000 | 0.541 | FAIL | isolated pixels > 8x median |
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

### KS00_alley_start (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.507 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | PASS | 0.1057 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 43.5 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.1220 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.18 (run 5 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.2000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.30 (run 5 px), - / -, 6 strips |

### KS01_alley_noodle (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.407 % of the frame |
| clipped highlights | PASS | 0.0087 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0639 | >= 0.0353 | display |
| mean luma | PASS | 0.1253 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 167.5 codes): objects on the horizon |

### KS02_alley_slit (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS02_alley_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.362 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0628 | >= 0.0353 | display |
| mean luma | PASS | 0.1054 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.2400 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.36 (run 8 px), - / -, 1 strips |

### KS03_boulevard_mouth (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1669, 755]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.920 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.0984 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 121.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.1050 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.16 (run 7 px), - / -, 2 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 24 | 0 | bright 7 (mode all), dark 17, first at [[870, 521], [881, 540], [883, 573], [889, 628], [135, 749]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1809, 811], [1810, 814]]; neon mask 2.495 % of the frame |
| clipped highlights | PASS | 0.0015 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0600 | >= 0.0353 | display |
| mean luma | PASS | 0.1385 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 876.0000 |  | sky band not uniform (spread 113.0 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.4590 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.69 (run 7 px), - / -, 3 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 5 (mode all), dark 1, first at [[1485, 158], [883, 815], [887, 857], [1431, 1008], [877, 1074]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[862, 977]]; neon mask 2.468 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1677 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.3640 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.55 (run 8 px), - / -, 4 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 10 | 0 | bright 3 (mode all), dark 7, first at [[1409, 725], [541, 738], [630, 746], [540, 747], [404, 751]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[339, 680], [555, 689]]; neon mask 2.165 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1322 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 116.5 codes): objects on the horizon |

### KS07_edge_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS07_edge_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[872, 222], [874, 290], [931, 584], [988, 584]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[872, 222], [881, 312]]; neon mask 3.230 % of the frame |
| clipped highlights | PASS | 0.0011 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0734 | >= 0.0353 | display |
| mean luma | PASS | 0.1217 | 0.0941..0.1961 | display |

### KS08_edge_flank (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 3.170 % of the frame |
| clipped highlights | PASS | 0.0031 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0625 | >= 0.0353 | display |
| mean luma | PASS | 0.1503 | 0.0941..0.1961 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1181, 271], [920, 487], [865, 492], [426, 717]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[865, 492]]; neon mask 2.060 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | PASS | 0.1350 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.6920 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.04 (run 5 px), - / -, 4 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 3 (mode all), dark 1, first at [[879, 290], [953, 296], [1026, 302], [762, 422]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.282 % of the frame |
| clipped highlights | PASS | 0.0030 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0588 | >= 0.0353 | display |
| mean luma | PASS | 0.1317 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.5110 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.77 (run 7 px), - / -, 4 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 1 (mode all), dark 1, first at [[1022, 508], [1419, 605]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1086, 101], [1087, 101], [566, 922]]; neon mask 2.397 % of the frame |
| clipped highlights | PASS | 0.0087 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1148 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 10 | 0 | bright 10 (mode all), dark 0, first at [[1033, 24], [588, 469], [619, 469], [650, 469], [1085, 469]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[390, 347], [1557, 603]]; neon mask 2.021 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0698 | >= 0.0353 | display |
| mean luma | PASS | 0.1233 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.5840 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.88 (run 9 px), - / -, 3 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.162 % of the frame |
| clipped highlights | PASS | 0.0006 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0683 | >= 0.0353 | display |
| mean luma | PASS | 0.1452 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.0370 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.55 (run 6 px), - / -, 1 strips |

### KS14_garden_gate (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.237 % of the frame |
| clipped highlights | PASS | 0.0036 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0504 | >= 0.0353 | display |
| mean luma | PASS | 0.0999 | 0.0941..0.1961 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.136 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0465 | >= 0.0353 | display |
| mean luma | FAIL | 0.0676 | 0.0784..0.1725 | display |

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS16_signs_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 3 (mode all), dark 2, first at [[303, 104], [1841, 1019], [1743, 1064], [1412, 1075], [1413, 1079]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[708, 980], [866, 1000]]; neon mask 0.238 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0510 | >= 0.0353 | display |
| mean luma | PASS | 0.1712 | 0.1255..0.251 | display |
| banding[wall_panel] | PASS | 0.1090 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.16 (run 5 px), - / -, 3 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS17_heart_reveal.png`

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
| mean luma | PASS | 0.1488 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.6070 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.91 (run 9 px), - / -, 6 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.77 (run 9 px) not gated (textured: gate steps), - / -, 22 strips |

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 64 | 0 outside neon masks | first at [[1231, 790], [1206, 796], [1203, 797], [1201, 798], [1180, 809]]; neon mask 2.234 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0698 | >= 0.0353 | display |
| mean luma | PASS | 0.1960 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.5410 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.81 (run 9 px), - / -, 9 strips |

### KS19_goal_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\FX_r06\KS19_goal_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 14 | 0 | bright 0 (mode all), dark 14, first at [[775, 558], [783, 575], [800, 613], [778, 668], [864, 692]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.565 % of the frame |
| clipped highlights | PASS | 0.0032 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.1213 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.0730 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.11 (run 5 px), - / -, 2 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
