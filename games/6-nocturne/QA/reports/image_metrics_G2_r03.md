# Image metrics: G2_r03

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03`  
Scene constants: NocturneConstants.cs  
Images: 69, failing images: 43. Checks: 468 PASS, 63 FAIL, 0 WARN, 31 SKIP, 69 INFO.

**Result: FAIL**

## Input warnings

- KS00_alley_start.png is not listed in report.json
- KS01_alley_noodle.png is not listed in report.json
- KS02_alley_slit.png is not listed in report.json
- KS03_boulevard_mouth.png is not listed in report.json
- KS04_boulevard_axis.png is not listed in report.json
- KS05_boulevard_up.png is not listed in report.json
- KS06_tram.png is not listed in report.json
- KS07_edge_window.png is not listed in report.json
- KS08_edge_flank.png is not listed in report.json
- KS09_edge_gulf.png is not listed in report.json
- KS10_edge_plunge.png is not listed in report.json
- KS11_bridge_train.png is not listed in report.json
- KS12_bridge_void.png is not listed in report.json
- KS13_bridge_glass.png is not listed in report.json
- KS14_garden_gate.png is not listed in report.json
- KS15_garden_canopy.png is not listed in report.json
- KS16_signs_wall.png is not listed in report.json
- KS17_heart_reveal.png is not listed in report.json
- KS18_koi_pass.png is not listed in report.json
- KS19_goal_window.png is not listed in report.json

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | - | 1920x1080 | 0.072 | 0.042 | 0.0000 | 3.703 | FAIL | mean luma, banding[mouth_glow] |
| KS01_alley_noodle | - | 1920x1080 | 0.099 | 0.045 | 0.0093 | - | FAIL | NaN-like pixels, mean luma |
| KS02_alley_slit | - | 1920x1080 | 0.108 | 0.048 | 0.0000 | 14.560 | FAIL | mean luma, banding[slit_cloud] |
| KS03_boulevard_mouth | - | 1920x1080 | 0.104 | 0.046 | 0.0000 | 5.571 | FAIL | isolated pixels > 8x median, mean luma, banding[mouth_glow] |
| KS04_boulevard_axis | - | 1920x1080 | 0.190 | 0.048 | 0.0218 | 6.126 | FAIL | isolated pixels > 8x median, banding[sky_above_facades] |
| KS05_boulevard_up | - | 1920x1080 | 0.238 | 0.057 | 0.0000 | 4.567 | FAIL | NaN-like pixels, banding[cloud_ceiling] |
| KS06_tram | - | 1920x1080 | 0.184 | 0.050 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | - | 1920x1080 | 0.217 | 0.052 | 0.0007 | - | FAIL | NaN-like pixels |
| KS09_edge_gulf | - | 1920x1080 | 0.148 | 0.050 | 0.0000 | 6.212 | FAIL | NaN-like pixels, banding[abyss] |
| KS10_edge_plunge | - | 1920x1080 | 0.120 | 0.048 | 0.0000 | 0.288 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train | - | 1920x1080 | 0.171 | 0.053 | 0.0011 | - | FAIL | NaN-like pixels |
| KS12_bridge_void | - | 1920x1080 | 0.141 | 0.049 | 0.0000 | 40.618 | FAIL | NaN-like pixels, banding[abyss] |
| KS13_bridge_glass | - | 1920x1080 | 0.066 | 0.050 | 0.0000 | 0.111 | FAIL | mean luma |
| KS14_garden_gate | - | 1920x1080 | 0.166 | 0.056 | 0.0024 | - | PASS |  |
| KS15_garden_canopy | - | 1920x1080 | 0.212 | 0.041 | 0.0000 | 1.326 | FAIL | banding[crown_glow] |
| KS16_signs_wall | - | 1920x1080 | 0.255 | 0.068 | 0.0000 | 2.000 | FAIL | banding[wall_panel] |
| KS17_heart_reveal | - | 1920x1080 | 0.220 | 0.061 | 0.0049 | 9.463 | FAIL | NaN-like pixels, banding[sky_koi_spire], banding[plaza] |
| KS18_koi_pass | - | 1920x1080 | 0.317 | 0.089 | 0.0196 | 10.935 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[sky_koi] |
| beat01_alley_end | 1 | 1920x1080 | 0.117 | 0.044 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat01_alley_look_back | 1 | 1920x1080 | 0.087 | 0.044 | 0.0019 | - | PASS |  |
| beat01_alley_look_l | 1 | 1920x1080 | 0.054 | 0.042 | 0.0000 | - | PASS |  |
| beat01_alley_look_r | 1 | 1920x1080 | 0.086 | 0.044 | 0.0000 | - | PASS |  |
| beat01_alley_look_up60 | 1 | 1920x1080 | 0.110 | 0.048 | 0.0000 | - | PASS |  |
| beat01_alley_mid | 1 | 1920x1080 | 0.098 | 0.045 | 0.0000 | - | PASS |  |
| beat01_alley_start | 1 | 1920x1080 | 0.073 | 0.044 | 0.0000 | - | FAIL | NaN-like pixels |
| beat02_boulevard_end | 2 | 1920x1080 | 0.160 | 0.062 | 0.0029 | - | FAIL | NaN-like pixels |
| beat02_boulevard_look_back | 2 | 1920x1080 | 0.136 | 0.048 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat02_boulevard_look_l | 2 | 1920x1080 | 0.117 | 0.048 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat02_boulevard_look_r | 2 | 1920x1080 | 0.109 | 0.048 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat02_boulevard_look_up60 | 2 | 1920x1080 | 0.233 | 0.065 | 0.0000 | - | FAIL | NaN-like pixels |
| beat02_boulevard_mid | 2 | 1920x1080 | 0.150 | 0.048 | 0.0079 | - | FAIL | NaN-like pixels |
| beat02_boulevard_start | 2 | 1920x1080 | 0.116 | 0.042 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat03_edge_end | 3 | 1920x1080 | 0.118 | 0.057 | 0.0013 | - | FAIL | NaN-like pixels |
| beat03_edge_look_back | 3 | 1920x1080 | 0.134 | 0.046 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat03_edge_look_down60 | 3 | 1920x1080 | 0.063 | 0.049 | 0.0000 | - | PASS |  |
| beat03_edge_look_l | 3 | 1920x1080 | 0.168 | 0.048 | 0.0052 | - | FAIL | NaN-like pixels |
| beat03_edge_look_r | 3 | 1920x1080 | 0.168 | 0.048 | 0.0041 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat03_edge_look_up60 | 3 | 1920x1080 | 0.282 | 0.088 | 0.0000 | - | FAIL | NaN-like pixels |
| beat03_edge_mid | 3 | 1920x1080 | 0.165 | 0.062 | 0.0039 | - | FAIL | NaN-like pixels |
| beat03_edge_start | 3 | 1920x1080 | 0.159 | 0.061 | 0.0028 | - | FAIL | NaN-like pixels |
| beat04_skybridge_end | 4 | 1920x1080 | 0.119 | 0.040 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat04_skybridge_look_back | 4 | 1920x1080 | 0.153 | 0.050 | 0.0007 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat04_skybridge_look_down60 | 4 | 1920x1080 | 0.064 | 0.050 | 0.0000 | - | PASS |  |
| beat04_skybridge_look_l | 4 | 1920x1080 | 0.188 | 0.050 | 0.0001 | - | FAIL | NaN-like pixels |
| beat04_skybridge_look_r | 4 | 1920x1080 | 0.188 | 0.050 | 0.0029 | - | FAIL | NaN-like pixels |
| beat04_skybridge_look_up60 | 4 | 1920x1080 | 0.257 | 0.066 | 0.0001 | - | PASS |  |
| beat04_skybridge_mid | 4 | 1920x1080 | 0.158 | 0.048 | 0.0008 | - | FAIL | isolated pixels > 8x median |
| beat04_skybridge_start | 4 | 1920x1080 | 0.149 | 0.058 | 0.0005 | - | PASS |  |
| beat05_garden_end | 5 | 1920x1080 | 0.138 | 0.048 | 0.0000 | - | PASS |  |
| beat05_garden_look_back | 5 | 1920x1080 | 0.118 | 0.045 | 0.0000 | - | PASS |  |
| beat05_garden_look_l | 5 | 1920x1080 | 0.088 | 0.033 | 0.0000 | - | FAIL | NaN-like pixels, luma p1 |
| beat05_garden_look_r | 5 | 1920x1080 | 0.087 | 0.036 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat05_garden_look_up60 | 5 | 1920x1080 | 0.238 | 0.045 | 0.0000 | - | PASS |  |
| beat05_garden_mid | 5 | 1920x1080 | 0.105 | 0.038 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat05_garden_start | 5 | 1920x1080 | 0.114 | 0.039 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat06_signs_end | 6 | 1920x1080 | 0.148 | 0.064 | 0.0000 | - | PASS |  |
| beat06_signs_look_back | 6 | 1920x1080 | 0.135 | 0.048 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat06_signs_look_l | 6 | 1920x1080 | 0.166 | 0.053 | 0.0000 | - | PASS |  |
| beat06_signs_look_r | 6 | 1920x1080 | 0.179 | 0.053 | 0.0000 | - | PASS |  |
| beat06_signs_look_up60 | 6 | 1920x1080 | 0.224 | 0.058 | 0.0000 | - | PASS |  |
| beat06_signs_mid | 6 | 1920x1080 | 0.170 | 0.056 | 0.0000 | - | PASS |  |
| beat06_signs_start | 6 | 1920x1080 | 0.137 | 0.048 | 0.0000 | - | PASS |  |
| beat07_heart_end | 7 | 1920x1080 | 0.080 | 0.052 | 0.0000 | - | PASS |  |
| beat07_heart_look_back | 7 | 1920x1080 | 0.176 | 0.082 | 0.0000 | - | PASS |  |
| beat07_heart_look_l | 7 | 1920x1080 | 0.172 | 0.077 | 0.0002 | - | PASS |  |
| beat07_heart_look_r | 7 | 1920x1080 | 0.173 | 0.077 | 0.0002 | - | PASS |  |
| beat07_heart_look_up60 | 7 | 1920x1080 | 0.404 | 0.120 | 0.0010 | - | PASS |  |
| beat07_heart_mid | 7 | 1920x1080 | 0.178 | 0.081 | 0.0000 | - | PASS |  |
| beat07_heart_start | 7 | 1920x1080 | 0.159 | 0.064 | 0.0000 | - | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS00_alley_start | 0x0 | INFO |
| KS01_alley_noodle | 0x0 | INFO |
| KS02_alley_slit | 0x0 | INFO |
| KS03_boulevard_mouth | 0x0 | INFO |
| KS04_boulevard_axis | 0x0 | INFO |
| KS06_tram | 0x0 | INFO |
| KS05_boulevard_up | 0x0 | INFO |
| KS07_edge_window | - | INFO |
| KS08_edge_flank | 0x0 | INFO |
| KS09_edge_gulf | 0x0 | INFO |
| KS10_edge_plunge | 0x0 | INFO |
| KS11_bridge_train | 0x0 | INFO |
| KS12_bridge_void | 0x0 | INFO |
| KS13_bridge_glass | 0x0 | INFO |
| KS14_garden_gate | 0x0 | INFO |
| KS15_garden_canopy | 0x0 | INFO |
| KS16_signs_wall | 0x0 | INFO |
| KS17_heart_reveal | 0x0 | INFO |
| KS18_koi_pass | 0x0 | INFO |
| KS19_goal_window | - | INFO |

## Details

### KS00_alley_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.860 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | FAIL | 0.0721 | 0.1098..0.2745 | display |
| banding[mouth_glow] | FAIL | 3.7030 | score <= 1 | visible steps/1000 2.65 (max 5.55 code), BandScore 4.40 (run 10 px), - / -, 9 strips |

### KS01_alley_noodle (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[951, 0]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.552 % of the frame |
| clipped highlights | INFO | 0.0093 |  | beat unknown |
| mean luma | FAIL | 0.0993 | 0.1098..0.2745 | display |

### KS02_alley_slit (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS02_alley_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.279 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | FAIL | 0.1083 | 0.1098..0.2745 | display |
| banding[slit_cloud] | FAIL | 14.5600 | score <= 1 | visible steps/1000 3.37 (max 8.25 code), BandScore 21.84 (run 14 px), - / -, 12 strips |

### KS03_boulevard_mouth (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1008, 488]]; neon mask 1.175 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | FAIL | 0.1041 | 0.1098..0.2745 | display |
| banding[mouth_glow] | FAIL | 5.5710 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 8.36 (run 13 px), - / -, 18 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[233, 510]]; neon mask 0.827 % of the frame |
| clipped highlights | INFO | 0.0218 |  | beat unknown |
| mean luma | PASS | 0.1904 | 0.1569..0.3529 | display |
| banding[sky_above_facades] | FAIL | 6.1260 | score <= 1 | visible steps/1000 2.50 (max 1.27 code), BandScore 9.19 (run 10 px), - / -, 27 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 1 (mode all), dark 10, first at [[135, 122], [1829, 144], [1899, 145], [1902, 152], [91, 161]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.604 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.2381 | 0.1569..0.3529 | display |
| banding[cloud_ceiling] | FAIL | 4.5670 | score <= 1 | visible steps/1000 2.06 (max 5.73 code), BandScore 6.85 (run 12 px), - / -, 24 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 2 (mode all), dark 11, first at [[1421, 142], [1839, 163], [1824, 167], [1860, 174], [1897, 187]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[258, 558], [290, 563]]; neon mask 0.902 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.1841 | 0.1569..0.3529 | display |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 58 | 0 | bright 0 (mode all), dark 58, first at [[456, 232], [429, 351], [418, 385], [417, 388], [1527, 541]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.683 % of the frame |
| clipped highlights | INFO | 0.0007 |  | beat unknown |
| mean luma | PASS | 0.2166 | 0.1176..0.3137 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 34 | 0 | bright 0 (mode all), dark 34, first at [[1412, 16], [1409, 17], [1386, 25], [1357, 35], [1317, 43]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.095 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.1479 | 0.1176..0.3137 | display |
| banding[abyss] | FAIL | 6.2120 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 9.32 (run 13 px), - / -, 24 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[326, 517]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[9, 214]]; neon mask 0.842 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.1205 | 0.1176..0.3137 | display |
| banding[abyss] | PASS | 0.2880 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.43 (run 8 px), - / -, 24 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[15, 50], [31, 52]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.226 % of the frame |
| clipped highlights | INFO | 0.0011 |  | beat unknown |
| mean luma | PASS | 0.1715 | 0.1098..0.2941 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 0 (mode all), dark 11, first at [[1440, 89], [1617, 229], [1394, 386], [1583, 556], [1465, 575]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.570 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.1411 | 0.1098..0.2941 | display |
| banding[abyss] | FAIL | 40.6180 | score <= 1 | visible steps/1000 2.65 (max 60.93 code), BandScore 6.42 (run 10 px), - / -, 24 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.000 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | FAIL | 0.0659 | 0.1098..0.2941 | display |
| banding[abyss] | PASS | 0.1110 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.17 (run 7 px), - / -, 24 strips |

### KS14_garden_gate (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.325 % of the frame |
| clipped highlights | INFO | 0.0024 |  | beat unknown |
| mean luma | PASS | 0.1662 | 0.1098..0.2941 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.701 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.2115 | 0.0941..0.251 | display |
| banding[crown_glow] | FAIL | 1.3260 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.99 (run 11 px), - / -, 21 strips |

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS16_signs_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.103 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.2546 | 0.1961..0.4314 | display |
| banding[wall_panel] | FAIL | 2.0000 | score <= 1 | visible steps/1000 1.85 (max 1.50 code), BandScore 2.89 (run 10 px), - / -, 24 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS17_heart_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[255, 377]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.093 % of the frame |
| clipped highlights | INFO | 0.0049 |  | beat unknown |
| mean luma | PASS | 0.2196 | 0.1333..0.3333 | display |
| banding[sky_koi_spire] | FAIL | 9.4630 | score <= 1 | visible steps/1000 2.89 (max 1.50 code), BandScore 14.19 (run 12 px), - / -, 16 strips |
| banding[plaza] | FAIL | 1.5830 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.37 (run 10 px), - / -, 24 strips |

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 15 | 0 | bright 0 (mode all), dark 15, first at [[1237, 803], [1254, 868], [1737, 889], [1739, 892], [1299, 896]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1341, 972], [1342, 975], [1343, 978]]; neon mask 1.967 % of the frame |
| clipped highlights | INFO | 0.0196 |  | beat unknown |
| mean luma | PASS | 0.3174 | 0.1333..0.3333 | display |
| banding[sky_koi] | FAIL | 10.9350 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 16.40 (run 11 px), - / -, 27 strips |

### beat01_alley_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat01_alley_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[108, 440]] |
| isolated pixels > 8x median | FAIL | 14 | 0 outside neon masks | first at [[1779, 327], [1783, 327], [1660, 340], [1779, 340], [1670, 360]]; neon mask 2.278 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0440 | >= 0.0353 | display |
| mean luma | INFO | 0.1174 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 215.0 codes): objects on the horizon |

### beat01_alley_look_back (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat01_alley_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.432 % of the frame |
| clipped highlights | PASS | 0.0019 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0440 | >= 0.0353 | display |
| mean luma | INFO | 0.0867 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 78.0 codes): objects on the horizon |

### beat01_alley_look_l (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat01_alley_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.000 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0420 | >= 0.0353 | display |
| mean luma | INFO | 0.0539 |  | display |
| horizon[default] | PASS | 0.0039 | <= 0.01 | row ~540.0, 576 cols: above [21.0, 10.0, 42.0] vs below [21.0, 10.0, 41.0] |

### beat01_alley_look_r (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat01_alley_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.003 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0440 | >= 0.0353 | display |
| mean luma | INFO | 0.0861 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 5.0 codes): objects on the horizon |

### beat01_alley_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat01_alley_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.414 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0482 | >= 0.0353 | display |
| mean luma | INFO | 0.1102 |  | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |

### beat01_alley_mid (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat01_alley_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.437 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0451 | >= 0.0353 | display |
| mean luma | INFO | 0.0984 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 47.0 codes): objects on the horizon |

### beat01_alley_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat01_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1020, 24]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.772 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0440 | >= 0.0353 | display |
| mean luma | INFO | 0.0728 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 34.5 codes): objects on the horizon |

### beat02_boulevard_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat02_boulevard_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 3 (mode all), dark 3, first at [[1633, 9], [1677, 110], [1627, 168], [942, 549], [931, 553]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.982 % of the frame |
| clipped highlights | PASS | 0.0029 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0617 | >= 0.0353 | display |
| mean luma | INFO | 0.1599 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 83.0 codes): objects on the horizon |

### beat02_boulevard_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat02_boulevard_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 7 | 0 outside neon masks | first at [[72, 312], [1557, 347], [1464, 425], [7, 432], [421, 479]]; neon mask 1.071 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0479 | >= 0.0353 | display |
| mean luma | INFO | 0.1358 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 78.5 codes): objects on the horizon |

### beat02_boulevard_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat02_boulevard_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[122, 391], [1644, 391], [1910, 431], [1825, 446]]; neon mask 1.013 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0479 | >= 0.0353 | display |
| mean luma | INFO | 0.1169 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 207.0 codes): objects on the horizon |

### beat02_boulevard_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat02_boulevard_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 5 | 0 outside neon masks | first at [[115, 326], [913, 326], [1738, 326], [94, 351], [1020, 391]]; neon mask 1.068 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0477 | >= 0.0353 | display |
| mean luma | INFO | 0.1093 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 208.0 codes): objects on the horizon |

### beat02_boulevard_look_up60 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat02_boulevard_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[628, 590]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.885 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0647 | >= 0.0353 | display |
| mean luma | INFO | 0.2326 |  | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |

### beat02_boulevard_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat02_boulevard_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 3 (mode all), dark 1, first at [[840, 276], [963, 354], [901, 525], [948, 545]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.024 % of the frame |
| clipped highlights | PASS | 0.0079 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0482 | >= 0.0353 | display |
| mean luma | INFO | 0.1500 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 41.5 codes): objects on the horizon |

### beat02_boulevard_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat02_boulevard_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 55 | 0 outside neon masks | first at [[989, 275], [1012, 275], [684, 315], [1727, 315], [473, 328]]; neon mask 2.611 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0423 | >= 0.0353 | display |
| mean luma | INFO | 0.1161 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 217.0 codes): objects on the horizon |

### beat03_edge_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat03_edge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[891, 105]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.683 % of the frame |
| clipped highlights | PASS | 0.0013 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0566 | >= 0.0353 | display |
| mean luma | INFO | 0.1178 |  | display |

### beat03_edge_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat03_edge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 6 | 0 outside neon masks | first at [[286, 335], [300, 339], [300, 358], [1529, 410], [54, 432]]; neon mask 1.466 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0463 | >= 0.0353 | display |
| mean luma | INFO | 0.1336 |  | display |

### beat03_edge_look_down60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat03_edge_look_down60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.000 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0493 | >= 0.0353 | display |
| mean luma | INFO | 0.0632 |  | display |

### beat03_edge_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat03_edge_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 42 | 0 | bright 0 (mode all), dark 42, first at [[747, 236], [1776, 247], [752, 254], [1737, 255], [1823, 270]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.013 % of the frame |
| clipped highlights | PASS | 0.0052 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0477 | >= 0.0353 | display |
| mean luma | INFO | 0.1679 |  | display |

### beat03_edge_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat03_edge_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 23 | 0 | bright 0 (mode all), dark 23, first at [[29, 64], [32, 66], [35, 68], [66, 70], [59, 78]] |
| isolated pixels > 8x median | FAIL | 7 | 0 outside neon masks | first at [[1911, 296], [1555, 437], [1555, 459], [1550, 489], [1270, 533]]; neon mask 0.735 % of the frame |
| clipped highlights | PASS | 0.0041 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0479 | >= 0.0353 | display |
| mean luma | INFO | 0.1682 |  | display |

### beat03_edge_look_up60 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat03_edge_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 0 (mode all), dark 4, first at [[1275, 1034], [339, 1040], [642, 1040], [296, 1061]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.373 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0884 | >= 0.0353 | display |
| mean luma | INFO | 0.2816 |  | display |

### beat03_edge_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat03_edge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 12 | 0 | bright 0 (mode all), dark 12, first at [[1703, 17], [1695, 20], [1719, 96], [1720, 102], [1687, 127]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.298 % of the frame |
| clipped highlights | PASS | 0.0039 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0617 | >= 0.0353 | display |
| mean luma | INFO | 0.1651 |  | display |

### beat03_edge_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat03_edge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 0 (mode all), dark 5, first at [[1654, 37], [1645, 117], [1649, 118], [1623, 164], [1710, 182]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.965 % of the frame |
| clipped highlights | PASS | 0.0028 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0611 | >= 0.0353 | display |
| mean luma | INFO | 0.1593 |  | display |

### beat04_skybridge_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat04_skybridge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 18 | 0 outside neon masks | first at [[37, 280], [37, 281], [37, 335], [37, 355], [37, 356]]; neon mask 0.380 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0401 | >= 0.0353 | display |
| mean luma | INFO | 0.1185 |  | display |

### beat04_skybridge_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat04_skybridge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[1082, 8], [1091, 30]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[1289, 688], [537, 729], [1476, 878], [1659, 982]]; neon mask 1.868 % of the frame |
| clipped highlights | PASS | 0.0007 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0499 | >= 0.0353 | display |
| mean luma | INFO | 0.1528 |  | display |

### beat04_skybridge_look_down60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat04_skybridge_look_down60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.207 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0502 | >= 0.0353 | display |
| mean luma | INFO | 0.0645 |  | display |

### beat04_skybridge_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat04_skybridge_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1436, 168]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.861 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0505 | >= 0.0353 | display |
| mean luma | INFO | 0.1884 |  | display |

### beat04_skybridge_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat04_skybridge_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1685, 1026]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.764 % of the frame |
| clipped highlights | PASS | 0.0029 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0505 | >= 0.0353 | display |
| mean luma | INFO | 0.1880 |  | display |

### beat04_skybridge_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat04_skybridge_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.530 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0661 | >= 0.0353 | display |
| mean luma | INFO | 0.2572 |  | display |

### beat04_skybridge_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat04_skybridge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 11 | 0 outside neon masks | first at [[1332, 756], [614, 760], [316, 828], [454, 828], [476, 828]]; neon mask 1.303 % of the frame |
| clipped highlights | PASS | 0.0008 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0479 | >= 0.0353 | display |
| mean luma | INFO | 0.1578 |  | display |

### beat04_skybridge_start (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat04_skybridge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.243 % of the frame |
| clipped highlights | PASS | 0.0005 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | INFO | 0.1487 |  | display |

### beat05_garden_end (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat05_garden_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.716 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0485 | >= 0.0353 | display |
| mean luma | INFO | 0.1383 |  | display |

### beat05_garden_look_back (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat05_garden_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.576 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0449 | >= 0.0353 | display |
| mean luma | INFO | 0.1185 |  | display |

### beat05_garden_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat05_garden_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1058, 213]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.066 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | FAIL | 0.0334 | >= 0.0353 | display |
| mean luma | INFO | 0.0881 |  | display |

### beat05_garden_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat05_garden_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 15 | 0 outside neon masks | first at [[283, 204], [284, 204], [284, 205], [384, 249], [385, 249]]; neon mask 0.153 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0356 | >= 0.0353 | display |
| mean luma | INFO | 0.0872 |  | display |

### beat05_garden_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat05_garden_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.428 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0451 | >= 0.0353 | display |
| mean luma | INFO | 0.2383 |  | display |

### beat05_garden_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat05_garden_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1793, 312], [1793, 344]]; neon mask 0.812 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0376 | >= 0.0353 | display |
| mean luma | INFO | 0.1051 |  | display |

### beat05_garden_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat05_garden_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1462, 150]] |
| isolated pixels > 8x median | FAIL | 21 | 0 outside neon masks | first at [[1789, 149], [1789, 271], [59, 292], [1530, 292], [1532, 292]]; neon mask 0.358 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0390 | >= 0.0353 | display |
| mean luma | INFO | 0.1142 |  | display |

### beat06_signs_end (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat06_signs_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.786 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0639 | >= 0.0353 | display |
| mean luma | INFO | 0.1483 |  | display |

### beat06_signs_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat06_signs_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[788, 475], [1007, 512], [970, 526], [965, 531]]; neon mask 0.187 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0479 | >= 0.0353 | display |
| mean luma | INFO | 0.1350 |  | display |

### beat06_signs_look_l (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat06_signs_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.003 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0533 | >= 0.0353 | display |
| mean luma | INFO | 0.1659 |  | display |

### beat06_signs_look_r (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat06_signs_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.039 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0532 | >= 0.0353 | display |
| mean luma | INFO | 0.1789 |  | display |

### beat06_signs_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat06_signs_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.002 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0580 | >= 0.0353 | display |
| mean luma | INFO | 0.2237 |  | display |

### beat06_signs_mid (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat06_signs_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.069 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0560 | >= 0.0353 | display |
| mean luma | INFO | 0.1700 |  | display |

### beat06_signs_start (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat06_signs_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.493 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0482 | >= 0.0353 | display |
| mean luma | INFO | 0.1368 |  | display |

### beat07_heart_end (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat07_heart_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.088 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0521 | >= 0.0353 | display |
| mean luma | INFO | 0.0797 |  | display |

### beat07_heart_look_back (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat07_heart_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.434 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0824 | >= 0.0353 | display |
| mean luma | INFO | 0.1762 |  | display |

### beat07_heart_look_l (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat07_heart_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.712 % of the frame |
| clipped highlights | PASS | 0.0002 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0768 | >= 0.0353 | display |
| mean luma | INFO | 0.1716 |  | display |

### beat07_heart_look_r (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat07_heart_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.389 % of the frame |
| clipped highlights | PASS | 0.0002 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0771 | >= 0.0353 | display |
| mean luma | INFO | 0.1727 |  | display |

### beat07_heart_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat07_heart_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.181 % of the frame |
| clipped highlights | PASS | 0.0010 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.1205 | >= 0.0353 | display |
| mean luma | INFO | 0.4039 |  | display |

### beat07_heart_mid (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat07_heart_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.761 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0810 | >= 0.0353 | display |
| mean luma | INFO | 0.1783 |  | display |

### beat07_heart_start (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\G2_r03\beat07_heart_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.277 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0639 | >= 0.0353 | display |
| mean luma | INFO | 0.1593 |  | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
