# Image metrics: graybox01_219

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 19. Checks: 169 PASS, 28 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start_219 | 1 | 2560x1080 | 0.067 | 0.040 | 0.0000 | 0.601 | FAIL | mean luma |
| KS01_alley_noodle_219 | 1 | 2560x1080 | 0.089 | 0.042 | 0.0072 | - | FAIL | NaN-like pixels, mean luma |
| KS02_alley_slit_219 | 1 | 2560x1080 | 0.094 | 0.044 | 0.0000 | 0.122 | FAIL | mean luma |
| KS03_boulevard_mouth_219 | 1 | 2560x1080 | 0.091 | 0.043 | 0.0000 | 0.000 | FAIL | isolated pixels > 8x median, mean luma |
| KS04_boulevard_axis_219 | 2 | 2560x1080 | 0.175 | 0.045 | 0.0166 | 0.316 | FAIL | isolated pixels > 8x median |
| KS05_boulevard_up_219 | 2 | 2560x1080 | 0.215 | 0.050 | 0.0001 | 0.793 | FAIL | NaN-like pixels |
| KS06_tram_219 | 2 | 2560x1080 | 0.172 | 0.047 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window_219 | 3 | 2560x1080 | 0.163 | 0.058 | 0.0041 | 0.334 | FAIL | NaN-like pixels |
| KS08_edge_flank_219 | 3 | 2560x1080 | 0.210 | 0.048 | 0.0005 | - | FAIL | NaN-like pixels |
| KS09_edge_gulf_219 | 3 | 2560x1080 | 0.139 | 0.044 | 0.0000 | 0.465 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS10_edge_plunge_219 | 3 | 2560x1080 | 0.117 | 0.045 | 0.0000 | 0.277 | FAIL | NaN-like pixels, isolated pixels > 8x median, mean luma |
| KS11_bridge_train_219 | 4 | 2560x1080 | 0.172 | 0.053 | 0.0009 | - | FAIL | NaN-like pixels |
| KS12_bridge_void_219 | 4 | 2560x1080 | 0.141 | 0.044 | 0.0000 | 0.355 | FAIL | NaN-like pixels |
| KS13_bridge_glass_219 | 4 | 2560x1080 | 0.076 | 0.050 | 0.0000 | 0.101 | FAIL | NaN-like pixels, mean luma |
| KS14_garden_gate_219 | 4 | 2560x1080 | 0.161 | 0.057 | 0.0020 | - | PASS |  |
| KS15_garden_canopy_219 | 5 | 2560x1080 | 0.186 | 0.036 | 0.0000 | 0.740 | FAIL | isolated pixels > 8x median |
| KS16_signs_wall_219 | 6 | 2560x1080 | 0.231 | 0.053 | 0.0000 | 0.169 | FAIL | NaN-like pixels |
| KS17_heart_reveal_219 | 7 | 2560x1080 | 0.205 | 0.057 | 0.0039 | 1.556 | FAIL | NaN-like pixels, banding[plaza] |
| KS18_koi_pass_219 | 7 | 2560x1080 | 0.288 | 0.079 | 0.0153 | 0.412 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS19_goal_window_219 | 7 | 2560x1080 | 0.102 | 0.052 | 0.0016 | 0.411 | FAIL | mean luma |

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

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS00_alley_start_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.676 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0404 | >= 0.0353 | display |
| mean luma | FAIL | 0.0670 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 34.0 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.6010 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.90 (run 6 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.00 (run 0 px), - / -, 8 strips |

### KS01_alley_noodle_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS01_alley_noodle_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1271, 0]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.453 % of the frame |
| clipped highlights | PASS | 0.0072 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0423 | >= 0.0353 | display |
| mean luma | FAIL | 0.0887 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 203.0 codes): objects on the horizon |

### KS02_alley_slit_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS02_alley_slit_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.212 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0437 | >= 0.0353 | display |
| mean luma | FAIL | 0.0944 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.1220 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.18 (run 8 px), - / -, 2 strips |

### KS03_boulevard_mouth_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS03_boulevard_mouth_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1509, 764]]; neon mask 0.886 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0432 | >= 0.0353 | display |
| mean luma | FAIL | 0.0912 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 756.7000 |  | sky band not uniform (spread 211.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.00 (run 0 px), - / -, 6 strips |

### KS04_boulevard_axis_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS04_boulevard_axis_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[553, 510], [164, 662], [165, 662]]; neon mask 0.689 % of the frame |
| clipped highlights | PASS | 0.0166 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0446 | >= 0.0353 | display |
| mean luma | PASS | 0.1754 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | 876.0000 |  | sky band not uniform (spread 58.3 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.3160 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.47 (run 10 px), - / -, 5 strips |

### KS05_boulevard_up_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS05_boulevard_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 1 (mode all), dark 12, first at [[455, 122], [2149, 144], [2219, 145], [2222, 152], [411, 161]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.549 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0502 | >= 0.0353 | display |
| mean luma | PASS | 0.2152 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.7930 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.19 (run 7 px), - / -, 6 strips |

### KS06_tram_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS06_tram_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 24 | 0 | bright 2 (mode all), dark 22, first at [[2355, 88], [2332, 94], [2328, 95], [2258, 114], [2257, 120]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[578, 558], [1112, 620]]; neon mask 0.740 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0465 | >= 0.0353 | display |
| mean luma | PASS | 0.1721 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 207.0 codes): objects on the horizon |

### KS07_edge_window_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS07_edge_window_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 2 (mode all), dark 2, first at [[1936, 199], [1941, 200], [1167, 320], [1251, 584]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.959 % of the frame |
| clipped highlights | PASS | 0.0041 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.1628 | 0.1176..0.3137 | display |
| banding[sky_above_gulf] | PASS | 0.3340 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.50 (run 6 px), - / -, 3 strips |

### KS08_edge_flank_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS08_edge_flank_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 59 | 0 | bright 0 (mode all), dark 59, first at [[776, 232], [749, 351], [738, 385], [737, 388], [1847, 541]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.556 % of the frame |
| clipped highlights | PASS | 0.0005 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0485 | >= 0.0353 | display |
| mean luma | PASS | 0.2104 | 0.1176..0.3137 | display |

### KS09_edge_gulf_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS09_edge_gulf_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 32 | 0 | bright 0 (mode all), dark 32, first at [[1729, 17], [1706, 25], [1677, 35], [1637, 43], [1595, 69]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[260, 358], [316, 402]]; neon mask 0.827 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0438 | >= 0.0353 | display |
| mean luma | PASS | 0.1389 | 0.1176..0.3137 | display |
| banding[abyss] | PASS | 0.4650 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.70 (run 7 px), - / -, 6 strips |

### KS10_edge_plunge_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS10_edge_plunge_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[646, 517]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[157, 16], [301, 205], [329, 214]]; neon mask 0.648 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0452 | >= 0.0353 | display |
| mean luma | FAIL | 0.1168 | 0.1176..0.3137 | display |
| banding[abyss] | PASS | 0.2770 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.42 (run 9 px), - / -, 6 strips |

### KS11_bridge_train_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS11_bridge_train_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 0 (mode all), dark 3, first at [[335, 50], [315, 60], [301, 69]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.309 % of the frame |
| clipped highlights | PASS | 0.0009 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0530 | >= 0.0353 | display |
| mean luma | PASS | 0.1722 | 0.1098..0.2941 | display |

### KS12_bridge_void_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS12_bridge_void_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 12 | 0 | bright 0 (mode all), dark 12, first at [[1760, 89], [1937, 229], [1903, 556], [1785, 575], [1925, 576]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.819 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0440 | >= 0.0353 | display |
| mean luma | PASS | 0.1414 | 0.1098..0.2941 | display |
| banding[abyss] | PASS | 0.3550 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.53 (run 7 px), - / -, 5 strips |

### KS13_bridge_glass_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS13_bridge_glass_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[82, 10], [175, 26]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.036 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0499 | >= 0.0353 | display |
| mean luma | FAIL | 0.0762 | 0.1098..0.2941 | display |
| banding[abyss] | PASS | 0.1010 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.15 (run 7 px), - / -, 32 strips |

### KS14_garden_gate_219 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS14_garden_gate_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.287 % of the frame |
| clipped highlights | PASS | 0.0020 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0566 | >= 0.0353 | display |
| mean luma | PASS | 0.1612 | 0.1098..0.2941 | display |

### KS15_garden_canopy_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS15_garden_canopy_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[2397, 1048]]; neon mask 0.522 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0359 | >= 0.0353 | display |
| mean luma | PASS | 0.1856 | 0.0941..0.251 | display |
| banding[crown_gap_sky] | PASS | 0.7400 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.11 (run 6 px), - / -, 2 strips |

### KS16_signs_wall_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS16_signs_wall_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 0 (mode all), dark 5, first at [[190, 816], [172, 841], [76, 974], [63, 992], [50, 1010]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.105 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0529 | >= 0.0353 | display |
| mean luma | PASS | 0.2313 | 0.1961..0.4314 | display |
| banding[wall_panel] | PASS | 0.1690 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.25 (run 10 px), - / -, 8 strips |

### KS17_heart_reveal_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS17_heart_reveal_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[575, 377]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.708 % of the frame |
| clipped highlights | PASS | 0.0039 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0574 | >= 0.0353 | display |
| mean luma | PASS | 0.2054 | 0.1333..0.3333 | display |
| banding[sky_koi_spire] | PASS | 0.4600 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.69 (run 10 px), - / -, 8 strips |
| banding[plaza] | FAIL | 1.5560 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.33 (run 10 px), - / -, 32 strips |

### KS18_koi_pass_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS18_koi_pass_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 15 | 0 | bright 0 (mode all), dark 15, first at [[1557, 803], [1574, 868], [2057, 889], [2059, 892], [1619, 896]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1661, 972], [1671, 1003]]; neon mask 1.585 % of the frame |
| clipped highlights | PASS | 0.0153 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0792 | >= 0.0353 | display |
| mean luma | PASS | 0.2877 | 0.1333..0.3333 | display |
| banding[sky_koi] | PASS | 0.4120 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.62 (run 8 px), - / -, 12 strips |

### KS19_goal_window_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_219\KS19_goal_window_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.093 % of the frame |
| clipped highlights | PASS | 0.0016 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0521 | >= 0.0353 | display |
| mean luma | FAIL | 0.1025 | 0.1333..0.3333 | display |
| banding[tenement_wall] | PASS | 0.4110 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.62 (run 10 px), - / -, 2 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
