# Image metrics: graybox01_1610

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 15. Checks: 174 PASS, 23 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start_1610 | 1 | 1680x1050 | 0.076 | 0.043 | 0.0000 | 0.627 | FAIL | isolated pixels > 8x median, mean luma |
| KS01_alley_noodle_1610 | 1 | 1680x1050 | 0.104 | 0.045 | 0.0104 | - | FAIL | NaN-like pixels, mean luma |
| KS02_alley_slit_1610 | 1 | 1680x1050 | 0.114 | 0.050 | 0.0001 | 0.117 | PASS |  |
| KS03_boulevard_mouth_1610 | 1 | 1680x1050 | 0.110 | 0.047 | 0.0000 | 0.086 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS04_boulevard_axis_1610 | 2 | 1680x1050 | 0.197 | 0.049 | 0.0241 | 0.171 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up_1610 | 2 | 1680x1050 | 0.247 | 0.061 | 0.0001 | 0.177 | FAIL | NaN-like pixels |
| KS06_tram_1610 | 2 | 1680x1050 | 0.188 | 0.051 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window_1610 | 3 | 1680x1050 | 0.166 | 0.063 | 0.0069 | 0.305 | FAIL | NaN-like pixels |
| KS08_edge_flank_1610 | 3 | 1680x1050 | 0.223 | 0.056 | 0.0009 | - | FAIL | NaN-like pixels |
| KS09_edge_gulf_1610 | 3 | 1680x1050 | 0.151 | 0.053 | 0.0000 | 0.350 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS10_edge_plunge_1610 | 3 | 1680x1050 | 0.123 | 0.049 | 0.0000 | 0.292 | PASS |  |
| KS11_bridge_train_1610 | 4 | 1680x1050 | 0.175 | 0.054 | 0.0009 | - | FAIL | NaN-like pixels |
| KS12_bridge_void_1610 | 4 | 1680x1050 | 0.149 | 0.051 | 0.0000 | 0.213 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS13_bridge_glass_1610 | 4 | 1680x1050 | 0.066 | 0.051 | 0.0000 | 0.160 | FAIL | mean luma |
| KS14_garden_gate_1610 | 4 | 1680x1050 | 0.166 | 0.056 | 0.0025 | - | PASS |  |
| KS15_garden_canopy_1610 | 5 | 1680x1050 | 0.222 | 0.045 | 0.0000 | 0.484 | PASS |  |
| KS16_signs_wall_1610 | 6 | 1680x1050 | 0.263 | 0.069 | 0.0000 | 0.120 | PASS |  |
| KS17_heart_reveal_1610 | 7 | 1680x1050 | 0.226 | 0.062 | 0.0060 | 1.483 | FAIL | banding[plaza] |
| KS18_koi_pass_1610 | 7 | 1680x1050 | 0.328 | 0.090 | 0.0232 | 0.099 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS19_goal_window_1610 | 7 | 1680x1050 | 0.106 | 0.054 | 0.0022 | 0.000 | FAIL | mean luma |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS00_alley_start | 1680x1050 | INFO |
| KS01_alley_noodle | 1680x1050 | INFO |
| KS02_alley_slit | 1680x1050 | INFO |
| KS03_boulevard_mouth | 1680x1050 | INFO |
| KS04_boulevard_axis | 1680x1050 | INFO |
| KS06_tram | 1680x1050 | INFO |
| KS05_boulevard_up | 1680x1050 | INFO |
| KS07_edge_window | 1680x1050 | INFO |
| KS08_edge_flank | 1680x1050 | INFO |
| KS09_edge_gulf | 1680x1050 | INFO |
| KS10_edge_plunge | 1680x1050 | INFO |
| KS11_bridge_train | 1680x1050 | INFO |
| KS12_bridge_void | 1680x1050 | INFO |
| KS13_bridge_glass | 1680x1050 | INFO |
| KS14_garden_gate | 1680x1050 | INFO |
| KS15_garden_canopy | 1680x1050 | INFO |
| KS16_signs_wall | 1680x1050 | INFO |
| KS17_heart_reveal | 1680x1050 | INFO |
| KS18_koi_pass | 1680x1050 | INFO |
| KS19_goal_window | 1680x1050 | INFO |

## Details

### KS00_alley_start_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS00_alley_start_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[896, 25]]; neon mask 0.970 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0429 | >= 0.0353 | display |
| mean luma | FAIL | 0.0755 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 494.5000 |  | sky band not uniform (spread 34.0 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.6270 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.94 (run 5 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.0590 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.09 (run 4 px), - / -, 5 strips |

### KS01_alley_noodle_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS01_alley_noodle_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[832, 0], [1089, 327]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.724 % of the frame |
| clipped highlights | PASS | 0.0104 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0449 | >= 0.0353 | display |
| mean luma | FAIL | 0.1037 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 586.1000 |  | sky band not uniform (spread 198.0 codes): objects on the horizon |

### KS02_alley_slit_1610 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS02_alley_slit_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.334 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0496 | >= 0.0353 | display |
| mean luma | PASS | 0.1141 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.1170 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.18 (run 8 px), - / -, 1 strips |

### KS03_boulevard_mouth_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS03_boulevard_mouth_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[372, 259]] |
| isolated pixels > 8x median | FAIL | 5 | 0 outside neon masks | first at [[920, 627], [557, 641], [690, 641], [819, 641], [822, 641]]; neon mask 1.383 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0474 | >= 0.0353 | display |
| mean luma | PASS | 0.1098 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | 735.7000 |  | sky band not uniform (spread 210.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.0860 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.13 (run 6 px), - / -, 3 strips |

### KS04_boulevard_axis_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS04_boulevard_axis_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[441, 359]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1599, 832]]; neon mask 0.943 % of the frame |
| clipped highlights | PASS | 0.0241 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0491 | >= 0.0353 | display |
| mean luma | PASS | 0.1972 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | 851.6000 |  | sky band not uniform (spread 43.4 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.1710 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.26 (run 10 px), - / -, 3 strips |

### KS05_boulevard_up_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS05_boulevard_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 0 (mode all), dark 3, first at [[511, 162], [512, 921], [550, 936]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.717 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0605 | >= 0.0353 | display |
| mean luma | PASS | 0.2465 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.1770 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.26 (run 7 px), - / -, 4 strips |

### KS06_tram_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS06_tram_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 1 (mode all), dark 3, first at [[1635, 258], [1533, 281], [1594, 336], [1293, 712]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[412, 556], [428, 583], [372, 615]]; neon mask 1.008 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0507 | >= 0.0353 | display |
| mean luma | PASS | 0.1884 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | 726.6000 |  | sky band not uniform (spread 208.0 codes): objects on the horizon |

### KS07_edge_window_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS07_edge_window_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[825, 561], [854, 561], [815, 566], [864, 566]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.269 % of the frame |
| clipped highlights | PASS | 0.0069 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0628 | >= 0.0353 | display |
| mean luma | PASS | 0.1659 | 0.1176..0.3137 | display |
| banding[sky_above_gulf] | PASS | 0.3050 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.46 (run 6 px), - / -, 2 strips |

### KS08_edge_flank_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS08_edge_flank_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 59 | 0 | bright 0 (mode all), dark 59, first at [[1506, 519], [1420, 522], [1417, 523], [1391, 526], [1423, 527]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.765 % of the frame |
| clipped highlights | PASS | 0.0009 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0563 | >= 0.0353 | display |
| mean luma | PASS | 0.2227 | 0.1176..0.3137 | display |

### KS09_edge_gulf_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS09_edge_gulf_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 41 | 0 | bright 0 (mode all), dark 41, first at [[1321, 8], [1278, 16], [1275, 17], [1255, 24], [1226, 28]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[100, 443]]; neon mask 1.199 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0527 | >= 0.0353 | display |
| mean luma | PASS | 0.1506 | 0.1176..0.3137 | display |
| banding[abyss] | PASS | 0.3500 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.53 (run 6 px), - / -, 3 strips |

### KS10_edge_plunge_1610 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS10_edge_plunge_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.949 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0491 | >= 0.0353 | display |
| mean luma | PASS | 0.1233 | 0.1176..0.3137 | display |
| banding[abyss] | PASS | 0.2920 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.44 (run 8 px), - / -, 4 strips |

### KS11_bridge_train_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS11_bridge_train_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 1 (mode all), dark 1, first at [[85, 30], [963, 98]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.217 % of the frame |
| clipped highlights | PASS | 0.0009 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0535 | >= 0.0353 | display |
| mean luma | PASS | 0.1749 | 0.1098..0.2941 | display |

### KS12_bridge_void_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS12_bridge_void_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 7 | 0 | bright 0 (mode all), dark 7, first at [[1331, 559], [1348, 564], [1361, 579], [1364, 581], [1360, 585]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[68, 642]]; neon mask 1.157 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0507 | >= 0.0353 | display |
| mean luma | PASS | 0.1487 | 0.1098..0.2941 | display |
| banding[abyss] | PASS | 0.2130 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.32 (run 7 px), - / -, 3 strips |

### KS13_bridge_glass_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS13_bridge_glass_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.000 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0513 | >= 0.0353 | display |
| mean luma | FAIL | 0.0664 | 0.1098..0.2941 | display |
| banding[abyss] | PASS | 0.1600 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.24 (run 12 px), - / -, 21 strips |

### KS14_garden_gate_1610 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS14_garden_gate_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.424 % of the frame |
| clipped highlights | PASS | 0.0025 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0561 | >= 0.0353 | display |
| mean luma | PASS | 0.1659 | 0.1098..0.2941 | display |

### KS15_garden_canopy_1610 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS15_garden_canopy_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.791 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0448 | >= 0.0353 | display |
| mean luma | PASS | 0.2220 | 0.0941..0.251 | display |
| banding[crown_gap_sky] | PASS | 0.4840 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.73 (run 5 px), - / -, 1 strips |

### KS16_signs_wall_1610 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS16_signs_wall_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.110 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0689 | >= 0.0353 | display |
| mean luma | PASS | 0.2633 | 0.1961..0.4314 | display |
| banding[wall_panel] | PASS | 0.1200 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.18 (run 7 px), - / -, 5 strips |

### KS17_heart_reveal_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS17_heart_reveal_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.342 % of the frame |
| clipped highlights | PASS | 0.0060 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | PASS | 0.2264 | 0.1333..0.3333 | display |
| banding[sky_koi_spire] | PASS | 0.2880 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.43 (run 10 px), - / -, 5 strips |
| banding[plaza] | FAIL | 1.4830 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.22 (run 9 px), - / -, 21 strips |

### KS18_koi_pass_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS18_koi_pass_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 0 (mode all), dark 5, first at [[1126, 845], [1623, 873], [1148, 929], [1150, 937], [1167, 1002]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1215, 959]]; neon mask 2.304 % of the frame |
| clipped highlights | PASS | 0.0232 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0905 | >= 0.0353 | display |
| mean luma | PASS | 0.3278 | 0.1333..0.3333 | display |
| banding[sky_koi] | PASS | 0.0990 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.15 (run 6 px), - / -, 7 strips |

### KS19_goal_window_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\graybox01_1610\KS19_goal_window_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.137 % of the frame |
| clipped highlights | PASS | 0.0022 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0535 | >= 0.0353 | display |
| mean luma | FAIL | 0.1057 | 0.1333..0.3333 | display |
| banding[tenement_wall] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.00 (run 0 px), - / -, 1 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
