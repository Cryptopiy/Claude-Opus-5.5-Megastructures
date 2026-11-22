# Image metrics: TR_r13

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 19. Checks: 154 PASS, 34 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS11_bridge_train | 4 | 1920x1080 | 0.119 | 0.067 | 0.0195 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train_1610 | 4 | 1680x1050 | 0.120 | 0.067 | 0.0220 | - | FAIL | NaN-like pixels |
| KS11_bridge_train_219 | 4 | 2560x1080 | 0.119 | 0.067 | 0.0142 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train_4k | 4 | 3840x2160 | 0.120 | 0.067 | 0.0196 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void | 4 | 1920x1080 | 0.121 | 0.063 | 0.0000 | 1.485 | FAIL | NaN-like pixels, banding[abyss] |
| KS12_bridge_void_1610 | 4 | 1680x1050 | 0.122 | 0.063 | 0.0000 | 1.675 | FAIL | NaN-like pixels, banding[abyss] |
| KS12_bridge_void_219 | 4 | 2560x1080 | 0.118 | 0.062 | 0.0000 | 2.586 | FAIL | NaN-like pixels, banding[abyss] |
| KS12_bridge_void_4k | 4 | 3840x2160 | 0.121 | 0.063 | 0.0000 | 2.000 | FAIL | banding[abyss] |
| KS13_bridge_glass | 4 | 1920x1080 | 0.134 | 0.068 | 0.0006 | 1.336 | FAIL | NaN-like pixels, banding[abyss] |
| KS13_bridge_glass_1610 | 4 | 1680x1050 | 0.139 | 0.070 | 0.0006 | 1.059 | FAIL | NaN-like pixels, banding[abyss] |
| KS13_bridge_glass_219 | 4 | 2560x1080 | 0.131 | 0.067 | 0.0005 | 1.280 | FAIL | NaN-like pixels, banding[abyss] |
| KS13_bridge_glass_4k | 4 | 3840x2160 | 0.134 | 0.068 | 0.0010 | 0.975 | PASS |  |
| KS14_garden_gate | 4 | 1920x1080 | 0.132 | 0.057 | 0.0037 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS14_garden_gate_1610 | 4 | 1680x1050 | 0.135 | 0.056 | 0.0042 | - | FAIL | NaN-like pixels |
| KS14_garden_gate_219 | 4 | 2560x1080 | 0.123 | 0.058 | 0.0029 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS14_garden_gate_4k | 4 | 3840x2160 | 0.133 | 0.057 | 0.0037 | - | FAIL | NaN-like pixels |
| KS15_garden_canopy | 5 | 1920x1080 | 0.127 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_1610 | 5 | 1680x1050 | 0.130 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_219 | 5 | 2560x1080 | 0.118 | 0.053 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_4k | 5 | 3840x2160 | 0.128 | 0.055 | 0.0001 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS00_alley_start | - | INFO |
| KS01_alley_noodle | - | INFO |
| KS02_alley_slit | - | INFO |
| KS03_boulevard_mouth | - | INFO |
| KS04_boulevard_axis | - | INFO |
| KS06_tram | - | INFO |
| KS05_boulevard_up | - | INFO |
| KS07_edge_window | - | INFO |
| KS08_edge_flank | - | INFO |
| KS09_edge_gulf | - | INFO |
| KS10_edge_plunge | - | INFO |
| KS11_bridge_train | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS12_bridge_void | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS13_bridge_glass | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS14_garden_gate | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS15_garden_canopy | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS16_signs_wall | - | INFO |
| KS17_heart_reveal | - | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1419, 605]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1086, 101], [1087, 101]]; neon mask 2.166 % of the frame |
| clipped highlights | PASS | 0.0195 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0670 | >= 0.0353 | display |
| mean luma | PASS | 0.1191 | 0.0941..0.1961 | display |

### KS11_bridge_train_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS11_bridge_train_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 2 (mode all), dark 1, first at [[957, 63], [1166, 386], [1024, 515]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.171 % of the frame |
| clipped highlights | PASS | 0.0220 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0672 | >= 0.0353 | display |
| mean luma | PASS | 0.1196 | 0.0941..0.1961 | display |

### KS11_bridge_train_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS11_bridge_train_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1739, 605]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1406, 101], [1407, 101]]; neon mask 2.085 % of the frame |
| clipped highlights | PASS | 0.0142 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1190 | 0.0941..0.1961 | display |

### KS11_bridge_train_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS11_bridge_train_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[2179, 1004]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[2765, 900]]; neon mask 1.186 % of the frame |
| clipped highlights | PASS | 0.0196 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0670 | >= 0.0353 | display |
| mean luma | PASS | 0.1200 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1330, 12], [1327, 16], [546, 612], [548, 622]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.627 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | PASS | 0.1211 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.4850 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.23 (run 6 px), - / -, 3 strips |

### KS12_bridge_void_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS12_bridge_void_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[745, 51], [437, 591]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.803 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | PASS | 0.1224 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.6750 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.51 (run 7 px), - / -, 3 strips |

### KS12_bridge_void_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS12_bridge_void_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1650, 12], [1647, 16], [866, 612], [868, 622]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.470 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0625 | >= 0.0353 | display |
| mean luma | PASS | 0.1185 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 2.5860 | score <= 1 | visible steps/1000 4.63 (max 1.24 code), BandScore 3.88 (run 11 px), - / -, 5 strips |

### KS12_bridge_void_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS12_bridge_void_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.323 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | PASS | 0.1212 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 2.0000 | score <= 1 | visible steps/1000 2.31 (max 1.15 code), BandScore 2.33 (run 9 px), - / -, 7 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[887, 727]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.379 % of the frame |
| clipped highlights | PASS | 0.0006 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0683 | >= 0.0353 | display |
| mean luma | PASS | 0.1343 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.3360 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.00 (run 7 px), - / -, 1 strips |

### KS13_bridge_glass_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS13_bridge_glass_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[769, 707]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.509 % of the frame |
| clipped highlights | PASS | 0.0006 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0697 | >= 0.0353 | display |
| mean luma | PASS | 0.1394 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.0590 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.59 (run 6 px), - / -, 1 strips |

### KS13_bridge_glass_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS13_bridge_glass_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1207, 727]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.253 % of the frame |
| clipped highlights | PASS | 0.0005 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0670 | >= 0.0353 | display |
| mean luma | PASS | 0.1307 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.2800 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.92 (run 8 px), - / -, 2 strips |

### KS13_bridge_glass_4k (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS13_bridge_glass_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.398 % of the frame |
| clipped highlights | PASS | 0.0010 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0678 | >= 0.0353 | display |
| mean luma | PASS | 0.1341 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.9750 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.46 (run 9 px), - / -, 3 strips |

### KS14_garden_gate (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 7 | 0 | bright 7 (mode all), dark 0, first at [[753, 15], [1354, 205], [1252, 216], [1479, 230], [291, 290]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1580, 245], [1485, 492], [1492, 494]]; neon mask 2.095 % of the frame |
| clipped highlights | PASS | 0.0037 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0566 | >= 0.0353 | display |
| mean luma | PASS | 0.1320 | 0.0941..0.1961 | display |

### KS14_garden_gate_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS14_garden_gate_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 8 | 0 | bright 6 (mode all), dark 2, first at [[1315, 78], [1085, 178], [1124, 210], [173, 224], [1344, 224]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.454 % of the frame |
| clipped highlights | PASS | 0.0042 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0558 | >= 0.0353 | display |
| mean luma | PASS | 0.1346 | 0.0941..0.1961 | display |

### KS14_garden_gate_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS14_garden_gate_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 7 | 0 | bright 7 (mode all), dark 0, first at [[1073, 15], [1674, 205], [1572, 216], [1799, 230], [611, 290]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[1900, 245], [1805, 492], [1812, 493], [1812, 494]]; neon mask 1.694 % of the frame |
| clipped highlights | PASS | 0.0029 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0577 | >= 0.0353 | display |
| mean luma | PASS | 0.1230 | 0.0941..0.1961 | display |

### KS14_garden_gate_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS14_garden_gate_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 24 | 0 | bright 9 (mode all), dark 15, first at [[2434, 0], [885, 19], [887, 61], [2407, 172], [3217, 223]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.375 % of the frame |
| clipped highlights | PASS | 0.0037 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0566 | >= 0.0353 | display |
| mean luma | PASS | 0.1326 | 0.0941..0.1961 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 7 (mode all), dark 6, first at [[1465, 93], [1457, 115], [120, 158], [118, 310], [87, 332]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1756, 661], [360, 741]]; neon mask 2.644 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0546 | >= 0.0353 | display |
| mean luma | PASS | 0.1270 | 0.0784..0.1725 | display |

### KS15_garden_canopy_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS15_garden_canopy_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 15 | 0 | bright 9 (mode all), dark 6, first at [[588, 18], [1331, 93], [23, 154], [2, 298], [13, 321]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[501, 824]]; neon mask 2.982 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0549 | >= 0.0353 | display |
| mean luma | PASS | 0.1295 | 0.0784..0.1725 | display |

### KS15_garden_canopy_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS15_garden_canopy_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 12 | 0 | bright 7 (mode all), dark 5, first at [[1785, 93], [1777, 115], [440, 158], [407, 332], [1072, 416]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[2076, 661], [680, 741]]; neon mask 2.072 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0529 | >= 0.0353 | display |
| mean luma | PASS | 0.1179 | 0.0784..0.1725 | display |

### KS15_garden_canopy_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r13\KS15_garden_canopy_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 37 | 0 | bright 10 (mode all), dark 27, first at [[1402, 33], [2959, 166], [2355, 214], [2911, 217], [236, 321]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1402, 33]]; neon mask 0.368 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0546 | >= 0.0353 | display |
| mean luma | PASS | 0.1278 | 0.0784..0.1725 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
