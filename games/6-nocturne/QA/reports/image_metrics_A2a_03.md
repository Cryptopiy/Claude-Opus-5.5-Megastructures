# Image metrics: A2a_03

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03`  
Scene constants: NocturneConstants.cs  
Images: 18, failing images: 14. Checks: 160 PASS, 21 FAIL, 0 WARN, 2 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | 1 | 1920x1080 | 0.177 | 0.051 | 0.0000 | 0.219 | PASS |  |
| KS01_alley_noodle | 1 | 1920x1080 | 0.182 | 0.052 | 0.0000 | - | PASS |  |
| KS02_alley_slit | 1 | 1920x1080 | 0.262 | 0.206 | 0.0000 | 0.155 | FAIL | magenta |
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.232 | 0.079 | 0.0000 | 0.375 | PASS |  |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.259 | 0.103 | 0.0000 | 0.409 | FAIL | magenta, NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up | 2 | 1920x1080 | 0.301 | 0.227 | 0.0000 | 0.179 | FAIL | magenta |
| KS06_tram | 2 | 1920x1080 | 0.242 | 0.087 | 0.0000 | - | FAIL | magenta, isolated pixels > 8x median |
| KS08_edge_flank | 3 | 1920x1080 | 0.262 | 0.101 | 0.0000 | - | FAIL | magenta |
| KS09_edge_gulf | 3 | 1920x1080 | 0.178 | 0.054 | 0.0000 | 4.501 | FAIL | banding[abyss] |
| KS10_edge_plunge | 3 | 1920x1080 | 0.144 | 0.051 | 0.0000 | 0.182 | PASS |  |
| KS11_bridge_train | 4 | 1920x1080 | 0.210 | 0.063 | 0.0000 | - | FAIL | magenta, isolated pixels > 8x median |
| KS12_bridge_void | 4 | 1920x1080 | 0.167 | 0.050 | 0.0000 | 5.286 | FAIL | banding[abyss] |
| KS13_bridge_glass | 4 | 1920x1080 | 0.060 | 0.044 | 0.0000 | 0.146 | FAIL | mean luma |
| KS14_garden_gate | 4 | 1920x1080 | 0.250 | 0.096 | 0.0000 | - | FAIL | magenta, isolated pixels > 8x median |
| KS15_garden_canopy | 5 | 1920x1080 | 0.245 | 0.179 | 0.0000 | 0.000 | FAIL | magenta |
| KS16_signs_wall | 6 | 1920x1080 | 0.256 | 0.212 | 0.0000 | 0.558 | FAIL | magenta |
| KS17_heart_reveal | 7 | 1920x1080 | 0.230 | 0.087 | 0.0000 | 4.181 | FAIL | magenta, isolated pixels > 8x median, banding[plaza] |
| KS18_koi_pass | 7 | 1920x1080 | 0.278 | 0.204 | 0.0000 | 0.146 | FAIL | magenta |

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

### KS00_alley_start (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0513 | >= 0.0353 | display |
| mean luma | PASS | 0.1770 | 0.1098..0.2745 | display |
| horizon[default] | PASS | 0.0000 | <= 0.01 | row ~508.6, 576 cols: above [90.0, 45.0, 122.0] vs below [90.0, 45.0, 122.0] |
| banding[mouth_glow] | PASS | 0.2190 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.33 (run 8 px), - / -, 9 strips |

### KS01_alley_noodle (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0524 | >= 0.0353 | display |
| mean luma | PASS | 0.1825 | 0.1098..0.2745 | display |
| horizon[default] | PASS | 0.0000 | <= 0.01 | row ~602.8, 576 cols: above [90.0, 45.0, 122.0] vs below [90.0, 45.0, 122.0] |

### KS02_alley_slit (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS02_alley_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 223853 | 0 px | ratio 0.108 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.2056 | >= 0.0353 | display |
| mean luma | PASS | 0.2620 | 0.1098..0.2745 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.1550 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.23 (run 10 px), - / -, 12 strips |

### KS03_boulevard_mouth (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0787 | >= 0.0353 | display |
| mean luma | PASS | 0.2324 | 0.1098..0.2745 | display |
| horizon[default] | PASS | 0.0000 | <= 0.01 | row ~756.7, 576 cols: above [90.0, 45.0, 122.0] vs below [90.0, 45.0, 122.0] |
| banding[mouth_glow] | PASS | 0.3750 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.56 (run 9 px), - / -, 18 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 592393 | 0 px | ratio 0.286 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[950, 878]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[952, 880], [967, 880], [887, 886]] |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.1028 | >= 0.0353 | display |
| mean luma | PASS | 0.2591 | 0.1569..0.3529 | display |
| horizon[default] | PASS | 0.0000 | <= 0.01 | row ~876.0, 576 cols: above [102.0, 51.0, 138.0] vs below [102.0, 51.0, 138.0] |
| banding[sky_above_facades] | PASS | 0.4090 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.61 (run 12 px), - / -, 27 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 1242181 | 0 px | ratio 0.599 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.2269 | >= 0.0353 | display |
| mean luma | PASS | 0.3011 | 0.1569..0.3529 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.1790 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.27 (run 10 px), - / -, 24 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 534102 | 0 px | ratio 0.258 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1505, 752], [1506, 752]] |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0874 | >= 0.0353 | display |
| mean luma | PASS | 0.2417 | 0.1569..0.3529 | display |
| horizon[default] | PASS | 0.0000 | <= 0.01 | row ~747.3, 576 cols: above [103.0, 51.0, 139.0] vs below [103.0, 51.0, 139.0] |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 555842 | 0 px | ratio 0.268 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.1008 | >= 0.0353 | display |
| mean luma | PASS | 0.2617 | 0.1176..0.3137 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0541 | >= 0.0353 | display |
| mean luma | PASS | 0.1775 | 0.1176..0.3137 | display |
| banding[abyss] | FAIL | 4.5010 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 6.75 (run 9 px), - / -, 24 strips |

### KS10_edge_plunge (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0510 | >= 0.0353 | display |
| mean luma | PASS | 0.1443 | 0.1176..0.3137 | display |
| banding[abyss] | PASS | 0.1820 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.27 (run 12 px), - / -, 24 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 172336 | 0 px | ratio 0.0831 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[951, 565]] |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | PASS | 0.2100 | 0.1098..0.2941 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0499 | >= 0.0353 | display |
| mean luma | PASS | 0.1669 | 0.1098..0.2941 | display |
| banding[abyss] | FAIL | 5.2860 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 7.93 (run 9 px), - / -, 24 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0443 | >= 0.0353 | display |
| mean luma | FAIL | 0.0596 | 0.1098..0.2941 | display |
| banding[abyss] | PASS | 0.1460 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.22 (run 9 px), - / -, 24 strips |

### KS14_garden_gate (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 368640 | 0 px | ratio 0.178 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 6 | 0 outside neon masks | first at [[619, 802], [620, 802], [619, 803], [620, 803], [619, 804]] |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0963 | >= 0.0353 | display |
| mean luma | PASS | 0.2500 | 0.1098..0.2941 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 2954 | 0 px | ratio 0.00142 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.1786 | >= 0.0353 | display |
| mean luma | PASS | 0.2447 | 0.0941..0.251 | display |
| banding[crown_glow] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.00 (run 0 px), - / -, 21 strips |

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS16_signs_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 212552 | 0 px | ratio 0.103 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.2124 | >= 0.0353 | display |
| mean luma | PASS | 0.2562 | 0.1961..0.4314 | display |
| banding[wall_panel] | PASS | 0.5580 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.84 (run 10 px), - / -, 24 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS17_heart_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 281772 | 0 px | ratio 0.136 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[870, 792]] |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0873 | >= 0.0353 | display |
| mean luma | PASS | 0.2301 | 0.1333..0.3333 | display |
| banding[sky_koi_spire] | PASS | 0.6820 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.02 (run 9 px), - / -, 16 strips |
| banding[plaza] | FAIL | 4.1810 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 6.27 (run 8 px), - / -, 24 strips |

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\A2a_03\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | FAIL | 825925 | 0 px | ratio 0.398 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.2037 | >= 0.0353 | display |
| mean luma | PASS | 0.2775 | 0.1333..0.3333 | display |
| banding[sky_koi] | PASS | 0.1460 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.22 (run 10 px), - / -, 27 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
