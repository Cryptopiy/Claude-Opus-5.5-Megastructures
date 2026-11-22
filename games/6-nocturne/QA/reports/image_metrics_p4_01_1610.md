# Image metrics: p4_01_1610

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610`  
Scene constants: NocturneConstants.cs  
Images: 20, failing images: 20. Checks: 164 PASS, 31 FAIL, 0 WARN, 7 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start_1610 | 1 | 1680x1050 | 0.121 | 0.064 | 0.0022 | 0.648 | FAIL | NaN-like pixels |
| KS01_alley_noodle_1610 | 1 | 1680x1050 | 0.149 | 0.067 | 0.0111 | - | FAIL | NaN-like pixels |
| KS02_alley_slit_1610 | 1 | 1680x1050 | 0.111 | 0.064 | 0.0000 | 0.352 | FAIL | isolated pixels > 8x median |
| KS03_boulevard_mouth_1610 | 1 | 1680x1050 | 0.102 | 0.059 | 0.0000 | 0.178 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS04_boulevard_axis_1610 | 2 | 1680x1050 | 0.145 | 0.062 | 0.0103 | 0.506 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up_1610 | 2 | 1680x1050 | 0.175 | 0.068 | 0.0006 | 0.416 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram_1610 | 2 | 1680x1050 | 0.136 | 0.062 | 0.0015 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window_1610 | 3 | 1680x1050 | 0.128 | 0.075 | 0.0060 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank_1610 | 3 | 1680x1050 | 0.157 | 0.066 | 0.0070 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS09_edge_gulf_1610 | 3 | 1680x1050 | 0.135 | 0.065 | 0.0002 | 0.421 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS10_edge_plunge_1610 | 3 | 1680x1050 | 0.133 | 0.059 | 0.0057 | 0.576 | FAIL | NaN-like pixels |
| KS11_bridge_train_1610 | 4 | 1680x1050 | 0.120 | 0.067 | 0.0220 | - | FAIL | NaN-like pixels |
| KS12_bridge_void_1610 | 4 | 1680x1050 | 0.126 | 0.063 | 0.0000 | 1.675 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS13_bridge_glass_1610 | 4 | 1680x1050 | 0.140 | 0.070 | 0.0006 | 1.059 | FAIL | banding[abyss] |
| KS14_garden_gate_1610 | 4 | 1680x1050 | 0.125 | 0.055 | 0.0041 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_1610 | 5 | 1680x1050 | 0.129 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS16_signs_wall_1610 | 6 | 1680x1050 | 0.205 | 0.062 | 0.0000 | 5.208 | FAIL | banding[wall_panel] |
| KS17_heart_reveal_1610 | 7 | 1680x1050 | 0.153 | 0.058 | 0.0001 | 0.566 | FAIL | NaN-like pixels |
| KS18_koi_pass_1610 | 7 | 1680x1050 | 0.203 | 0.070 | 0.0000 | 0.507 | FAIL | isolated pixels > 8x median |
| KS19_goal_window_1610 | 7 | 1680x1050 | 0.120 | 0.058 | 0.0029 | 0.231 | FAIL | NaN-like pixels |

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

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS00_alley_start_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 2 (mode all), dark 1, first at [[755, 284], [856, 463], [820, 486]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.101 % of the frame |
| clipped highlights | PASS | 0.0022 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0644 | >= 0.0353 | display |
| mean luma | PASS | 0.1212 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 494.5000 |  | sky band not uniform (spread 53.0 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.1640 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.25 (run 9 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.6480 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.97 (run 9 px), - / -, 5 strips |

### KS01_alley_noodle_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS01_alley_noodle_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 0 (mode all), dark 4, first at [[1087, 513], [1087, 516], [1080, 548], [1139, 551]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.111 % of the frame |
| clipped highlights | PASS | 0.0111 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0669 | >= 0.0353 | display |
| mean luma | PASS | 0.1489 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 586.1000 |  | sky band not uniform (spread 103.0 codes): objects on the horizon |

### KS02_alley_slit_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS02_alley_slit_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1080, 996]]; neon mask 0.195 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0641 | >= 0.0353 | display |
| mean luma | PASS | 0.1109 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.3520 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.53 (run 7 px), - / -, 1 strips |

### KS03_boulevard_mouth_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS03_boulevard_mouth_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[770, 730], [1530, 737]] |
| isolated pixels > 8x median | FAIL | 5 | 0 outside neon masks | first at [[761, 726], [786, 726], [1258, 726], [1523, 726], [304, 742]]; neon mask 1.101 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.1020 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 735.3000 |  | sky band not uniform (spread 122.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.1780 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.27 (run 5 px), - / -, 2 strips |

### KS04_boulevard_axis_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS04_boulevard_axis_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 37 | 0 | bright 7 (mode all), dark 30, first at [[1088, 425], [1158, 427], [744, 473], [758, 504], [763, 525]] |
| isolated pixels > 8x median | FAIL | 7 | 0 outside neon masks | first at [[522, 612], [1671, 788], [1672, 792], [420, 795], [420, 796]]; neon mask 3.032 % of the frame |
| clipped highlights | PASS | 0.0103 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | PASS | 0.1445 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 851.6000 |  | sky band not uniform (spread 95.7 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.5060 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.76 (run 8 px), - / -, 3 strips |

### KS05_boulevard_up_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS05_boulevard_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 1 (mode all), dark 1, first at [[1351, 151], [1295, 982]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[741, 973], [737, 1017]]; neon mask 3.299 % of the frame |
| clipped highlights | PASS | 0.0006 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0683 | >= 0.0353 | display |
| mean luma | PASS | 0.1750 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.4160 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.62 (run 6 px), - / -, 4 strips |

### KS06_tram_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS06_tram_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 8 | 0 | bright 4 (mode all), dark 4, first at [[1194, 362], [433, 717], [301, 720], [432, 723], [1539, 730]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1073, 515], [232, 657], [237, 657]]; neon mask 2.671 % of the frame |
| clipped highlights | PASS | 0.0015 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1364 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 726.6000 |  | sky band not uniform (spread 115.0 codes): objects on the horizon |

### KS07_edge_window_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS07_edge_window_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[778, 163], [825, 561], [854, 561], [822, 562], [864, 566]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[731, 154], [420, 277]]; neon mask 3.970 % of the frame |
| clipped highlights | PASS | 0.0060 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0745 | >= 0.0353 | display |
| mean luma | PASS | 0.1278 | 0.0941..0.1961 | display |

### KS08_edge_flank_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS08_edge_flank_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[373, 501], [982, 519], [967, 693], [1016, 869]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1329, 566]]; neon mask 4.457 % of the frame |
| clipped highlights | PASS | 0.0070 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0656 | >= 0.0353 | display |
| mean luma | PASS | 0.1572 | 0.0941..0.1961 | display |

### KS09_edge_gulf_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS09_edge_gulf_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 10 (mode all), dark 1, first at [[1002, 52], [939, 105], [1019, 138], [313, 221], [890, 231]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[275, 502]]; neon mask 3.333 % of the frame |
| clipped highlights | PASS | 0.0002 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0647 | >= 0.0353 | display |
| mean luma | PASS | 0.1350 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.4210 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.63 (run 10 px), - / -, 3 strips |

### KS10_edge_plunge_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS10_edge_plunge_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[939, 286], [1154, 291], [648, 410]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.876 % of the frame |
| clipped highlights | PASS | 0.0057 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0588 | >= 0.0353 | display |
| mean luma | PASS | 0.1333 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.5760 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.86 (run 7 px), - / -, 4 strips |

### KS11_bridge_train_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS11_bridge_train_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[957, 63]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.485 % of the frame |
| clipped highlights | PASS | 0.0220 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0672 | >= 0.0353 | display |
| mean luma | PASS | 0.1198 | 0.0941..0.1961 | display |

### KS12_bridge_void_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS12_bridge_void_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[745, 51], [437, 591]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[718, 115]]; neon mask 0.803 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | PASS | 0.1264 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.6750 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.51 (run 7 px), - / -, 3 strips |

### KS13_bridge_glass_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS13_bridge_glass_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.528 % of the frame |
| clipped highlights | PASS | 0.0006 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0697 | >= 0.0353 | display |
| mean luma | PASS | 0.1396 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.0590 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.59 (run 6 px), - / -, 1 strips |

### KS14_garden_gate_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS14_garden_gate_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 21 | 0 | bright 20 (mode all), dark 1, first at [[1085, 178], [119, 194], [1663, 195], [1038, 205], [1040, 210]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1348, 236], [1290, 480]]; neon mask 2.995 % of the frame |
| clipped highlights | PASS | 0.0041 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0555 | >= 0.0353 | display |
| mean luma | PASS | 0.1247 | 0.0941..0.1961 | display |

### KS15_garden_canopy_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS15_garden_canopy_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 22 | 0 | bright 17 (mode all), dark 5, first at [[588, 18], [1331, 93], [23, 154], [13, 321], [1502, 412]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[501, 824], [1269, 980], [1263, 1017]]; neon mask 3.426 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0546 | >= 0.0353 | display |
| mean luma | PASS | 0.1289 | 0.0784..0.1725 | display |

### KS16_signs_wall_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS16_signs_wall_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.459 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0625 | >= 0.0353 | display |
| mean luma | PASS | 0.2054 | 0.1255..0.251 | display |
| banding[wall_panel] | FAIL | 5.2080 | score <= 1 | visible steps/1000 2.88 (max 2.45 code), BandScore 7.81 (run 7 px), - / -, 5 strips |

### KS17_heart_reveal_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS17_heart_reveal_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 0 (mode all), dark 3, first at [[1338, 466], [1331, 615], [1335, 636]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.626 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0577 | >= 0.0353 | display |
| mean luma | PASS | 0.1529 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.5660 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.85 (run 8 px), - / -, 5 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.91 (run 8 px) not gated (textured: gate steps), - / -, 19 strips |

### KS18_koi_pass_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS18_koi_pass_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 19 | 0 outside neon masks | first at [[1195, 897], [1194, 901], [1205, 928], [1203, 929], [1204, 932]]; neon mask 2.657 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0698 | >= 0.0353 | display |
| mean luma | PASS | 0.2035 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.5070 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.76 (run 7 px), - / -, 7 strips |

### KS19_goal_window_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\KS19_goal_window_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 14 | 0 | bright 0 (mode all), dark 14, first at [[774, 481], [827, 518], [643, 567], [858, 573], [822, 626]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.717 % of the frame |
| clipped highlights | PASS | 0.0029 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0580 | >= 0.0353 | display |
| mean luma | PASS | 0.1199 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.2310 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.35 (run 7 px), - / -, 1 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
