# Image metrics: p4_01_219

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 20. Checks: 158 PASS, 37 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start_219 | 1 | 2560x1080 | 0.115 | 0.066 | 0.0014 | 0.425 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS01_alley_noodle_219 | 1 | 2560x1080 | 0.160 | 0.066 | 0.0071 | - | FAIL | NaN-like pixels, mean luma |
| KS02_alley_slit_219 | 1 | 2560x1080 | 0.105 | 0.059 | 0.0000 | 0.629 | FAIL | NaN-like pixels |
| KS03_boulevard_mouth_219 | 1 | 2560x1080 | 0.090 | 0.056 | 0.0000 | 0.000 | FAIL | NaN-like pixels |
| KS04_boulevard_axis_219 | 2 | 2560x1080 | 0.131 | 0.058 | 0.0081 | 0.624 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up_219 | 2 | 2560x1080 | 0.153 | 0.059 | 0.0007 | 1.209 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[cloud_ceiling] |
| KS06_tram_219 | 2 | 2560x1080 | 0.133 | 0.061 | 0.0007 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window_219 | 3 | 2560x1080 | 0.128 | 0.074 | 0.0040 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank_219 | 3 | 2560x1080 | 0.143 | 0.062 | 0.0048 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS09_edge_gulf_219 | 3 | 2560x1080 | 0.129 | 0.061 | 0.0001 | 0.464 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS10_edge_plunge_219 | 3 | 2560x1080 | 0.125 | 0.058 | 0.0036 | 4.419 | FAIL | NaN-like pixels, banding[abyss] |
| KS11_bridge_train_219 | 4 | 2560x1080 | 0.119 | 0.067 | 0.0142 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void_219 | 4 | 2560x1080 | 0.122 | 0.062 | 0.0000 | 2.586 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS13_bridge_glass_219 | 4 | 2560x1080 | 0.131 | 0.067 | 0.0005 | 1.280 | FAIL | banding[abyss] |
| KS14_garden_gate_219 | 4 | 2560x1080 | 0.117 | 0.058 | 0.0028 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_219 | 5 | 2560x1080 | 0.116 | 0.053 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS16_signs_wall_219 | 6 | 2560x1080 | 0.181 | 0.058 | 0.0000 | 7.992 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[wall_panel] |
| KS17_heart_reveal_219 | 7 | 2560x1080 | 0.142 | 0.056 | 0.0001 | 0.764 | FAIL | NaN-like pixels |
| KS18_koi_pass_219 | 7 | 2560x1080 | 0.180 | 0.069 | 0.0000 | 0.614 | FAIL | isolated pixels > 8x median |
| KS19_goal_window_219 | 7 | 2560x1080 | 0.116 | 0.058 | 0.0021 | 0.662 | FAIL | NaN-like pixels |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS00_alley_start | 2560x1080 | INFO |
| KS01_alley_noodle | 2560x1080 | INFO |
| KS02_alley_slit | 2560x1080 | INFO |
| KS03_boulevard_mouth | 2560x1080 | INFO |
| KS04_boulevard_axis | 2560x1080 | INFO |
| KS06_tram | 2560x1080 | INFO |
| KS05_boulevard_up | 2560x1080 | INFO |
| KS07_edge_window | 2560x1080 | INFO |
| KS08_edge_flank | 2560x1080 | INFO |
| KS09_edge_gulf | 2560x1080 | INFO |
| KS10_edge_plunge | 2560x1080 | INFO |
| KS11_bridge_train | 2560x1080 | INFO |
| KS12_bridge_void | 2560x1080 | INFO |
| KS13_bridge_glass | 2560x1080 | INFO |
| KS14_garden_gate | 2560x1080 | INFO |
| KS15_garden_canopy | 2560x1080 | INFO |
| KS16_signs_wall | 2560x1080 | INFO |
| KS17_heart_reveal | 2560x1080 | INFO |
| KS18_koi_pass | 2560x1080 | INFO |
| KS19_goal_window | 2560x1080 | INFO |

## Details

### KS00_alley_start_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS00_alley_start_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[1393, 469], [1297, 472]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1408, 137], [1407, 287]]; neon mask 0.076 % of the frame |
| clipped highlights | PASS | 0.0014 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0661 | >= 0.0353 | display |
| mean luma | PASS | 0.1147 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 51.3 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.1130 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.17 (run 4 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.4250 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.64 (run 6 px), - / -, 8 strips |

### KS01_alley_noodle_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS01_alley_noodle_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 61 | 0 | bright 0 (mode all), dark 61, first at [[2374, 114], [2320, 153], [2310, 180], [2456, 231], [2450, 234]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.089 % of the frame |
| clipped highlights | PASS | 0.0071 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0664 | >= 0.0353 | display |
| mean luma | FAIL | 0.1600 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 102.0 codes): objects on the horizon |

### KS02_alley_slit_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS02_alley_slit_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 0 (mode all), dark 6, first at [[2351, 14], [2322, 39], [2412, 40], [2250, 47], [2340, 48]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.120 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0588 | >= 0.0353 | display |
| mean luma | PASS | 0.1053 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.6290 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.94 (run 8 px), - / -, 2 strips |

### KS03_boulevard_mouth_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS03_boulevard_mouth_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1989, 755]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.670 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0558 | >= 0.0353 | display |
| mean luma | PASS | 0.0900 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 118.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.00 (run 0 px), - / -, 3 strips |

### KS04_boulevard_axis_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS04_boulevard_axis_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 27 | 0 | bright 9 (mode all), dark 18, first at [[1535, 437], [1190, 521], [1201, 540], [1203, 573], [451, 619]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[2129, 811], [2130, 814], [78, 849], [110, 850]]; neon mask 2.037 % of the frame |
| clipped highlights | PASS | 0.0081 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0580 | >= 0.0353 | display |
| mean luma | PASS | 0.1310 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 876.0000 |  | sky band not uniform (spread 113.0 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.6240 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.94 (run 7 px), - / -, 5 strips |

### KS05_boulevard_up_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS05_boulevard_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 5 (mode all), dark 1, first at [[1805, 158], [1203, 815], [1207, 857], [1751, 1008], [1197, 1074]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1182, 977]]; neon mask 2.187 % of the frame |
| clipped highlights | PASS | 0.0007 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0594 | >= 0.0353 | display |
| mean luma | PASS | 0.1529 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | FAIL | 1.2090 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.81 (run 9 px), - / -, 6 strips |

### KS06_tram_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS06_tram_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 3 (mode all), dark 10, first at [[2273, 92], [2270, 93], [2150, 127], [1381, 536], [1729, 725]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[659, 680], [875, 689], [13, 715]]; neon mask 2.140 % of the frame |
| clipped highlights | PASS | 0.0007 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0605 | >= 0.0353 | display |
| mean luma | PASS | 0.1325 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 116.0 codes): objects on the horizon |

### KS07_edge_window_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS07_edge_window_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1192, 222], [1194, 290], [1251, 584], [1308, 584]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1192, 222]]; neon mask 4.340 % of the frame |
| clipped highlights | PASS | 0.0040 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0737 | >= 0.0353 | display |
| mean luma | PASS | 0.1281 | 0.0941..0.1961 | display |

### KS08_edge_flank_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS08_edge_flank_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 3 (mode all), dark 1, first at [[2429, 479], [1000, 648], [1309, 790], [1274, 860]] |
| isolated pixels > 8x median | FAIL | 6 | 0 outside neon masks | first at [[2256, 245], [2257, 245], [1776, 584], [1776, 587], [1780, 587]]; neon mask 3.346 % of the frame |
| clipped highlights | PASS | 0.0048 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | PASS | 0.1432 | 0.0941..0.1961 | display |

### KS09_edge_gulf_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS09_edge_gulf_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 8 | 0 | bright 8 (mode all), dark 0, first at [[1446, 55], [1382, 112], [1501, 271], [1318, 480], [1240, 487]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1185, 492], [699, 517]]; neon mask 2.308 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0611 | >= 0.0353 | display |
| mean luma | PASS | 0.1286 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.4640 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.70 (run 8 px), - / -, 6 strips |

### KS10_edge_plunge_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS10_edge_plunge_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 4 (mode all), dark 1, first at [[1199, 290], [1507, 297], [1346, 302], [1747, 341], [1082, 422]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.268 % of the frame |
| clipped highlights | PASS | 0.0036 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0580 | >= 0.0353 | display |
| mean luma | PASS | 0.1245 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 4.4190 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 6.63 (run 6 px), - / -, 6 strips |

### KS11_bridge_train_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS11_bridge_train_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1739, 605]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1406, 101], [1407, 101]]; neon mask 2.306 % of the frame |
| clipped highlights | PASS | 0.0142 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1191 | 0.0941..0.1961 | display |

### KS12_bridge_void_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS12_bridge_void_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1650, 12], [1647, 16], [866, 612], [868, 622]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[746, 118]]; neon mask 0.477 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0625 | >= 0.0353 | display |
| mean luma | PASS | 0.1221 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 2.5860 | score <= 1 | visible steps/1000 4.63 (max 1.24 code), BandScore 3.88 (run 11 px), - / -, 5 strips |

### KS13_bridge_glass_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS13_bridge_glass_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.269 % of the frame |
| clipped highlights | PASS | 0.0005 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0670 | >= 0.0353 | display |
| mean luma | PASS | 0.1308 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.2800 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.92 (run 8 px), - / -, 2 strips |

### KS14_garden_gate_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS14_garden_gate_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 22 | 0 | bright 22 (mode all), dark 0, first at [[2240, 12], [1073, 15], [2140, 188], [538, 199], [1674, 205]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1900, 245], [1743, 494]]; neon mask 2.123 % of the frame |
| clipped highlights | PASS | 0.0028 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0577 | >= 0.0353 | display |
| mean luma | PASS | 0.1166 | 0.0941..0.1961 | display |

### KS15_garden_canopy_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS15_garden_canopy_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 20 | 0 | bright 15 (mode all), dark 5, first at [[1777, 115], [440, 158], [2398, 158], [407, 332], [219, 345]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[680, 741], [1569, 1007]]; neon mask 2.508 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0527 | >= 0.0353 | display |
| mean luma | PASS | 0.1164 | 0.0784..0.1725 | display |

### KS16_signs_wall_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS16_signs_wall_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 6 (mode all), dark 3, first at [[599, 112], [342, 176], [58, 820], [56, 823], [2161, 1019]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1028, 980], [1186, 1000]]; neon mask 1.041 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.1814 | 0.1255..0.251 | display |
| banding[wall_panel] | FAIL | 7.9920 | score <= 1 | visible steps/1000 2.80 (max 2.44 code), BandScore 11.99 (run 9 px), - / -, 8 strips |

### KS17_heart_reveal_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS17_heart_reveal_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1787, 643]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.741 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0558 | >= 0.0353 | display |
| mean luma | PASS | 0.1419 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.7640 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.15 (run 8 px), - / -, 8 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.18 (run 12 px) not gated (textured: gate steps), - / -, 30 strips |

### KS18_koi_pass_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS18_koi_pass_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 51 | 0 outside neon masks | first at [[1651, 941], [1652, 944], [1650, 945], [1653, 947], [1651, 948]]; neon mask 1.696 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0692 | >= 0.0353 | display |
| mean luma | PASS | 0.1799 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.6140 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.92 (run 8 px), - / -, 12 strips |

### KS19_goal_window_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\KS19_goal_window_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 12 | 0 | bright 0 (mode all), dark 12, first at [[1095, 558], [1103, 575], [1120, 613], [1098, 668], [1184, 692]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.442 % of the frame |
| clipped highlights | PASS | 0.0021 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.1155 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.6620 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.99 (run 8 px), - / -, 2 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
