# Image metrics: p4_01_4k

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k`  
Scene constants: NocturneConstants.cs  
Images: 80, failing images: 78. Checks: 648 PASS, 132 FAIL, 0 WARN, 28 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | 1 | 1920x1080 | 0.120 | 0.065 | 0.0018 | 0.480 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS00_alley_start_1610 | 1 | 1680x1050 | 0.121 | 0.064 | 0.0022 | 0.648 | FAIL | NaN-like pixels |
| KS00_alley_start_219 | 1 | 2560x1080 | 0.115 | 0.066 | 0.0014 | 0.425 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS00_alley_start_4k | 1 | 3840x2160 | 0.120 | 0.064 | 0.0016 | 0.689 | FAIL | NaN-like pixels |
| KS01_alley_noodle | 1 | 1920x1080 | 0.155 | 0.066 | 0.0102 | - | FAIL | NaN-like pixels |
| KS01_alley_noodle_1610 | 1 | 1680x1050 | 0.149 | 0.067 | 0.0111 | - | FAIL | NaN-like pixels |
| KS01_alley_noodle_219 | 1 | 2560x1080 | 0.160 | 0.066 | 0.0071 | - | FAIL | NaN-like pixels, mean luma |
| KS01_alley_noodle_4k | 1 | 3840x2160 | 0.155 | 0.066 | 0.0097 | - | FAIL | NaN-like pixels |
| KS02_alley_slit | 1 | 1920x1080 | 0.109 | 0.064 | 0.0000 | 0.336 | PASS |  |
| KS02_alley_slit_1610 | 1 | 1680x1050 | 0.111 | 0.064 | 0.0000 | 0.352 | FAIL | isolated pixels > 8x median |
| KS02_alley_slit_219 | 1 | 2560x1080 | 0.105 | 0.059 | 0.0000 | 0.629 | FAIL | NaN-like pixels |
| KS02_alley_slit_4k | 1 | 3840x2160 | 0.109 | 0.063 | 0.0001 | 0.399 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.099 | 0.059 | 0.0000 | 0.102 | FAIL | NaN-like pixels |
| KS03_boulevard_mouth_1610 | 1 | 1680x1050 | 0.102 | 0.059 | 0.0000 | 0.178 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS03_boulevard_mouth_219 | 1 | 2560x1080 | 0.090 | 0.056 | 0.0000 | 0.000 | FAIL | NaN-like pixels |
| KS03_boulevard_mouth_4k | 1 | 3840x2160 | 0.099 | 0.058 | 0.0000 | 0.075 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.140 | 0.060 | 0.0098 | 0.543 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS04_boulevard_axis_1610 | 2 | 1680x1050 | 0.145 | 0.062 | 0.0103 | 0.506 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS04_boulevard_axis_219 | 2 | 2560x1080 | 0.131 | 0.058 | 0.0081 | 0.624 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS04_boulevard_axis_4k | 2 | 3840x2160 | 0.139 | 0.060 | 0.0122 | 2.000 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[sky_above_facades] |
| KS05_boulevard_up | 2 | 1920x1080 | 0.170 | 0.067 | 0.0011 | 0.478 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up_1610 | 2 | 1680x1050 | 0.175 | 0.068 | 0.0006 | 0.416 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up_219 | 2 | 2560x1080 | 0.153 | 0.059 | 0.0007 | 1.209 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[cloud_ceiling] |
| KS05_boulevard_up_4k | 2 | 3840x2160 | 0.169 | 0.067 | 0.0014 | 2.000 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[cloud_ceiling] |
| KS06_tram | 2 | 1920x1080 | 0.135 | 0.062 | 0.0015 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram_1610 | 2 | 1680x1050 | 0.136 | 0.062 | 0.0015 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram_219 | 2 | 2560x1080 | 0.133 | 0.061 | 0.0007 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram_4k | 2 | 3840x2160 | 0.134 | 0.062 | 0.0016 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window | 3 | 1920x1080 | 0.127 | 0.074 | 0.0054 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window_1610 | 3 | 1680x1050 | 0.128 | 0.075 | 0.0060 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window_219 | 3 | 2560x1080 | 0.128 | 0.074 | 0.0040 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window_4k | 3 | 3840x2160 | 0.128 | 0.074 | 0.0055 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | 3 | 1920x1080 | 0.153 | 0.062 | 0.0065 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank_1610 | 3 | 1680x1050 | 0.157 | 0.066 | 0.0070 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank_219 | 3 | 2560x1080 | 0.143 | 0.062 | 0.0048 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank_4k | 3 | 3840x2160 | 0.152 | 0.062 | 0.0088 | - | FAIL | NaN-like pixels |
| KS09_edge_gulf | 3 | 1920x1080 | 0.134 | 0.063 | 0.0001 | 0.389 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS09_edge_gulf_1610 | 3 | 1680x1050 | 0.135 | 0.065 | 0.0002 | 0.421 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS09_edge_gulf_219 | 3 | 2560x1080 | 0.129 | 0.061 | 0.0001 | 0.464 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS09_edge_gulf_4k | 3 | 3840x2160 | 0.134 | 0.062 | 0.0015 | 0.278 | FAIL | NaN-like pixels |
| KS10_edge_plunge | 3 | 1920x1080 | 0.130 | 0.059 | 0.0048 | 0.638 | FAIL | NaN-like pixels |
| KS10_edge_plunge_1610 | 3 | 1680x1050 | 0.133 | 0.059 | 0.0057 | 0.576 | FAIL | NaN-like pixels |
| KS10_edge_plunge_219 | 3 | 2560x1080 | 0.125 | 0.058 | 0.0036 | 4.419 | FAIL | NaN-like pixels, banding[abyss] |
| KS10_edge_plunge_4k | 3 | 3840x2160 | 0.131 | 0.059 | 0.0062 | 1.437 | FAIL | NaN-like pixels, banding[abyss] |
| KS11_bridge_train | 4 | 1920x1080 | 0.119 | 0.067 | 0.0195 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train_1610 | 4 | 1680x1050 | 0.120 | 0.067 | 0.0220 | - | FAIL | NaN-like pixels |
| KS11_bridge_train_219 | 4 | 2560x1080 | 0.119 | 0.067 | 0.0142 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train_4k | 4 | 3840x2160 | 0.120 | 0.067 | 0.0196 | - | FAIL | NaN-like pixels |
| KS12_bridge_void | 4 | 1920x1080 | 0.125 | 0.063 | 0.0000 | 1.485 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS12_bridge_void_1610 | 4 | 1680x1050 | 0.126 | 0.063 | 0.0000 | 1.675 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS12_bridge_void_219 | 4 | 2560x1080 | 0.122 | 0.062 | 0.0000 | 2.586 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS12_bridge_void_4k | 4 | 3840x2160 | 0.125 | 0.063 | 0.0000 | 2.000 | FAIL | banding[abyss] |
| KS13_bridge_glass | 4 | 1920x1080 | 0.134 | 0.068 | 0.0006 | 1.337 | FAIL | NaN-like pixels, banding[abyss] |
| KS13_bridge_glass_1610 | 4 | 1680x1050 | 0.140 | 0.070 | 0.0006 | 1.059 | FAIL | banding[abyss] |
| KS13_bridge_glass_219 | 4 | 2560x1080 | 0.131 | 0.067 | 0.0005 | 1.280 | FAIL | banding[abyss] |
| KS13_bridge_glass_4k | 4 | 3840x2160 | 0.134 | 0.068 | 0.0011 | 0.974 | PASS |  |
| KS14_garden_gate | 4 | 1920x1080 | 0.123 | 0.056 | 0.0036 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS14_garden_gate_1610 | 4 | 1680x1050 | 0.125 | 0.055 | 0.0041 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS14_garden_gate_219 | 4 | 2560x1080 | 0.117 | 0.058 | 0.0028 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS14_garden_gate_4k | 4 | 3840x2160 | 0.124 | 0.056 | 0.0037 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy | 5 | 1920x1080 | 0.126 | 0.054 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_1610 | 5 | 1680x1050 | 0.129 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_219 | 5 | 2560x1080 | 0.116 | 0.053 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_4k | 5 | 3840x2160 | 0.127 | 0.054 | 0.0000 | - | FAIL | NaN-like pixels |
| KS16_signs_wall | 6 | 1920x1080 | 0.199 | 0.062 | 0.0000 | 2.033 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[wall_panel] |
| KS16_signs_wall_1610 | 6 | 1680x1050 | 0.205 | 0.062 | 0.0000 | 5.208 | FAIL | banding[wall_panel] |
| KS16_signs_wall_219 | 6 | 2560x1080 | 0.181 | 0.058 | 0.0000 | 7.992 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[wall_panel] |
| KS16_signs_wall_4k | 6 | 3840x2160 | 0.197 | 0.062 | 0.0000 | 3.211 | FAIL | NaN-like pixels, banding[wall_panel] |
| KS17_heart_reveal | 7 | 1920x1080 | 0.148 | 0.057 | 0.0000 | 0.776 | FAIL | NaN-like pixels |
| KS17_heart_reveal_1610 | 7 | 1680x1050 | 0.153 | 0.058 | 0.0001 | 0.566 | FAIL | NaN-like pixels |
| KS17_heart_reveal_219 | 7 | 2560x1080 | 0.142 | 0.056 | 0.0001 | 0.764 | FAIL | NaN-like pixels |
| KS17_heart_reveal_4k | 7 | 3840x2160 | 0.147 | 0.057 | 0.0001 | 0.573 | FAIL | NaN-like pixels |
| KS18_koi_pass | 7 | 1920x1080 | 0.197 | 0.070 | 0.0000 | 0.577 | FAIL | isolated pixels > 8x median |
| KS18_koi_pass_1610 | 7 | 1680x1050 | 0.203 | 0.070 | 0.0000 | 0.507 | FAIL | isolated pixels > 8x median |
| KS18_koi_pass_219 | 7 | 2560x1080 | 0.180 | 0.069 | 0.0000 | 0.614 | FAIL | isolated pixels > 8x median |
| KS18_koi_pass_4k | 7 | 3840x2160 | 0.195 | 0.071 | 0.0000 | 0.529 | FAIL | NaN-like pixels |
| KS19_goal_window | 7 | 1920x1080 | 0.121 | 0.058 | 0.0032 | 0.073 | FAIL | NaN-like pixels |
| KS19_goal_window_1610 | 7 | 1680x1050 | 0.120 | 0.058 | 0.0029 | 0.231 | FAIL | NaN-like pixels |
| KS19_goal_window_219 | 7 | 2560x1080 | 0.116 | 0.058 | 0.0021 | 0.662 | FAIL | NaN-like pixels |
| KS19_goal_window_4k | 7 | 3840x2160 | 0.121 | 0.058 | 0.0029 | 0.126 | FAIL | NaN-like pixels |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS00_alley_start | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS01_alley_noodle | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS02_alley_slit | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS03_boulevard_mouth | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS04_boulevard_axis | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS06_tram | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS05_boulevard_up | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS07_edge_window | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS08_edge_flank | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS09_edge_gulf | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS10_edge_plunge | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS11_bridge_train | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS12_bridge_void | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS13_bridge_glass | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS14_garden_gate | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS15_garden_canopy | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS16_signs_wall | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS17_heart_reveal | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS18_koi_pass | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS19_goal_window | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |

## Details

### KS00_alley_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[1073, 469], [977, 472]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1088, 137], [1087, 287]]; neon mask 0.097 % of the frame |
| clipped highlights | PASS | 0.0018 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0647 | >= 0.0353 | display |
| mean luma | PASS | 0.1196 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 52.0 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.1820 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.27 (run 5 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.4800 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.72 (run 6 px), - / -, 6 strips |

### KS00_alley_start_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS00_alley_start_1610.png`

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

### KS00_alley_start_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS00_alley_start_219.png`

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

### KS00_alley_start_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS00_alley_start_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 0 (mode all), dark 6, first at [[1177, 716], [1167, 766], [1187, 767], [1184, 770], [1951, 936]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.103 % of the frame |
| clipped highlights | PASS | 0.0016 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0641 | >= 0.0353 | display |
| mean luma | PASS | 0.1198 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 1017.2000 |  | sky band not uniform (spread 52.0 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.3060 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.46 (run 7 px), - / -, 2 strips |
| banding[alley_wall] | PASS | 0.6890 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.03 (run 8 px), - / -, 13 strips |

### KS01_alley_noodle (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 10 | 0 | bright 0 (mode all), dark 10, first at [[1919, 240], [1909, 274], [1914, 357], [1899, 362], [1214, 520]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.103 % of the frame |
| clipped highlights | PASS | 0.0102 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0664 | >= 0.0353 | display |
| mean luma | PASS | 0.1548 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 101.0 codes): objects on the horizon |

### KS01_alley_noodle_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS01_alley_noodle_1610.png`

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

### KS01_alley_noodle_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS01_alley_noodle_219.png`

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

### KS01_alley_noodle_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS01_alley_noodle_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 35 | 0 | bright 2 (mode all), dark 33, first at [[3824, 321], [3749, 579], [3821, 580], [3799, 606], [3685, 622]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.037 % of the frame |
| clipped highlights | PASS | 0.0097 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0661 | >= 0.0353 | display |
| mean luma | PASS | 0.1546 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 1205.7000 |  | sky band not uniform (spread 103.4 codes): objects on the horizon |

### KS02_alley_slit (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS02_alley_slit.png`

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

### KS02_alley_slit_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS02_alley_slit_1610.png`

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

### KS02_alley_slit_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS02_alley_slit_219.png`

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

### KS02_alley_slit_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS02_alley_slit_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 0 (mode all), dark 5, first at [[2237, 845], [2236, 856], [2236, 860], [2235, 872], [2235, 879]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[2415, 2049]]; neon mask 0.066 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | PASS | 0.1090 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.3990 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.60 (run 8 px), - / -, 3 strips |

### KS03_boulevard_mouth (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS03_boulevard_mouth.png`

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
| mean luma | PASS | 0.0986 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 121.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.1020 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.15 (run 7 px), - / -, 2 strips |

### KS03_boulevard_mouth_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS03_boulevard_mouth_1610.png`

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

### KS03_boulevard_mouth_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS03_boulevard_mouth_219.png`

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

### KS03_boulevard_mouth_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS03_boulevard_mouth_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 0 (mode all), dark 3, first at [[2186, 1502], [2209, 1502], [2814, 1502]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[1341, 1433], [797, 1503], [821, 1503], [1121, 1503]]; neon mask 0.185 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.0986 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 1512.6000 |  | sky band not uniform (spread 122.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.0750 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.11 (run 5 px), - / -, 5 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 26 | 0 | bright 9 (mode all), dark 17, first at [[1215, 437], [870, 521], [881, 540], [883, 573], [131, 619]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1809, 811], [1810, 814]]; neon mask 2.674 % of the frame |
| clipped highlights | PASS | 0.0098 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0600 | >= 0.0353 | display |
| mean luma | PASS | 0.1404 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 876.0000 |  | sky band not uniform (spread 113.0 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.5430 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.81 (run 7 px), - / -, 3 strips |

### KS04_boulevard_axis_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS04_boulevard_axis_1610.png`

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

### KS04_boulevard_axis_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS04_boulevard_axis_219.png`

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

### KS04_boulevard_axis_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS04_boulevard_axis_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 24 | 0 | bright 2 (mode all), dark 22, first at [[2809, 420], [2810, 424], [2811, 428], [873, 458], [870, 468]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1785, 1220], [1816, 1388]]; neon mask 1.054 % of the frame |
| clipped highlights | PASS | 0.0122 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0597 | >= 0.0353 | display |
| mean luma | PASS | 0.1394 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 1751.9000 |  | sky band not uniform (spread 114.0 codes): objects on the horizon |
| banding[sky_above_facades] | FAIL | 2.0000 | score <= 1 | visible steps/1000 2.31 (max 1.39 code), BandScore 0.85 (run 10 px), - / -, 7 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 5 (mode all), dark 1, first at [[1485, 158], [883, 815], [887, 857], [1431, 1008], [877, 1074]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[862, 977]]; neon mask 2.867 % of the frame |
| clipped highlights | PASS | 0.0011 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1701 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.4780 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.72 (run 9 px), - / -, 4 strips |

### KS05_boulevard_up_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS05_boulevard_up_1610.png`

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

### KS05_boulevard_up_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS05_boulevard_up_219.png`

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

### KS05_boulevard_up_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS05_boulevard_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 1 (mode all), dark 2, first at [[3184, 671], [2797, 1879], [458, 1891]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[2373, 1212], [2541, 1264]]; neon mask 0.908 % of the frame |
| clipped highlights | PASS | 0.0014 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0670 | >= 0.0353 | display |
| mean luma | PASS | 0.1685 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | FAIL | 2.0000 | score <= 1 | visible steps/1000 1.49 (max 1.17 code), BandScore 0.73 (run 9 px), - / -, 9 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 3 (mode all), dark 8, first at [[1830, 127], [1061, 536], [1409, 725], [541, 738], [630, 746]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[339, 680], [555, 689]]; neon mask 2.444 % of the frame |
| clipped highlights | PASS | 0.0015 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1352 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 116.5 codes): objects on the horizon |

### KS06_tram_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS06_tram_1610.png`

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

### KS06_tram_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS06_tram_219.png`

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

### KS06_tram_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS06_tram_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 7 | 0 | bright 6 (mode all), dark 1, first at [[2039, 538], [2801, 812], [2557, 1343], [1256, 1498], [3008, 1506]] |
| isolated pixels > 8x median | FAIL | 7 | 0 outside neon masks | first at [[2809, 175], [2817, 222], [2512, 286], [2800, 1002], [2823, 1028]]; neon mask 0.945 % of the frame |
| clipped highlights | PASS | 0.0016 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1343 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 1494.6000 |  | sky band not uniform (spread 116.5 codes): objects on the horizon |

### KS07_edge_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS07_edge_window.png`

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
| mean luma | PASS | 0.1272 | 0.0941..0.1961 | display |

### KS07_edge_window_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS07_edge_window_1610.png`

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

### KS07_edge_window_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS07_edge_window_219.png`

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

### KS07_edge_window_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS07_edge_window_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[3747, 527], [3774, 728], [2231, 1153], [1891, 1155], [1948, 1155]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1795, 731], [1795, 732]]; neon mask 2.047 % of the frame |
| clipped highlights | PASS | 0.0055 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0740 | >= 0.0353 | display |
| mean luma | PASS | 0.1281 | 0.0941..0.1961 | display |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS08_edge_flank.png`

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
| mean luma | PASS | 0.1534 | 0.0941..0.1961 | display |

### KS08_edge_flank_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS08_edge_flank_1610.png`

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

### KS08_edge_flank_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS08_edge_flank_219.png`

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

### KS08_edge_flank_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS08_edge_flank_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 10 | 0 | bright 10 (mode all), dark 0, first at [[952, 1087], [950, 1095], [1262, 1414], [1317, 1417], [1285, 1453]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.704 % of the frame |
| clipped highlights | PASS | 0.0088 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1524 | 0.0941..0.1961 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[1126, 55], [1062, 112], [1181, 271], [1023, 478], [998, 480]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[865, 492], [379, 517]]; neon mask 2.965 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | PASS | 0.1336 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.3890 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.58 (run 6 px), - / -, 4 strips |

### KS09_edge_gulf_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS09_edge_gulf_1610.png`

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

### KS09_edge_gulf_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS09_edge_gulf_219.png`

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

### KS09_edge_gulf_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS09_edge_gulf_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[888, 446], [1653, 510], [756, 1024], [851, 1429]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.103 % of the frame |
| clipped highlights | PASS | 0.0015 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1339 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.2780 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.42 (run 9 px), - / -, 9 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS10_edge_plunge.png`

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
| mean luma | PASS | 0.1303 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.6380 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.96 (run 10 px), - / -, 4 strips |

### KS10_edge_plunge_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS10_edge_plunge_1610.png`

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

### KS10_edge_plunge_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS10_edge_plunge_219.png`

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

### KS10_edge_plunge_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS10_edge_plunge_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[2499, 0], [1525, 844]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.585 % of the frame |
| clipped highlights | PASS | 0.0062 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.1315 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.4370 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.16 (run 9 px), - / -, 9 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS11_bridge_train.png`

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
| mean luma | PASS | 0.1192 | 0.0941..0.1961 | display |

### KS11_bridge_train_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS11_bridge_train_1610.png`

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

### KS11_bridge_train_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS11_bridge_train_219.png`

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

### KS11_bridge_train_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS11_bridge_train_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 14 | 0 | bright 13 (mode all), dark 1, first at [[2759, 656], [2437, 751], [2800, 810], [2265, 844], [2468, 861]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.319 % of the frame |
| clipped highlights | PASS | 0.0196 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0670 | >= 0.0353 | display |
| mean luma | PASS | 0.1201 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS12_bridge_void.png`

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
| mean luma | PASS | 0.1250 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.4850 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.23 (run 6 px), - / -, 3 strips |

### KS12_bridge_void_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS12_bridge_void_1610.png`

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

### KS12_bridge_void_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS12_bridge_void_219.png`

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

### KS12_bridge_void_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS12_bridge_void_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.328 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | PASS | 0.1252 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 2.0000 | score <= 1 | visible steps/1000 2.31 (max 1.15 code), BandScore 2.33 (run 9 px), - / -, 7 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[887, 727]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.401 % of the frame |
| clipped highlights | PASS | 0.0006 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0683 | >= 0.0353 | display |
| mean luma | PASS | 0.1344 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.3370 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.00 (run 7 px), - / -, 1 strips |

### KS13_bridge_glass_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS13_bridge_glass_1610.png`

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

### KS13_bridge_glass_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS13_bridge_glass_219.png`

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

### KS13_bridge_glass_4k (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS13_bridge_glass_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.421 % of the frame |
| clipped highlights | PASS | 0.0011 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0678 | >= 0.0353 | display |
| mean luma | PASS | 0.1342 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.9740 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.46 (run 9 px), - / -, 3 strips |

### KS14_garden_gate (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 21 | 0 | bright 21 (mode all), dark 0, first at [[753, 15], [1820, 188], [218, 199], [1354, 205], [213, 209]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1580, 245], [1423, 494]]; neon mask 2.645 % of the frame |
| clipped highlights | PASS | 0.0036 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0558 | >= 0.0353 | display |
| mean luma | PASS | 0.1234 | 0.0941..0.1961 | display |

### KS14_garden_gate_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS14_garden_gate_1610.png`

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

### KS14_garden_gate_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS14_garden_gate_219.png`

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

### KS14_garden_gate_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS14_garden_gate_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 29 | 0 | bright 16 (mode all), dark 13, first at [[2434, 0], [885, 19], [887, 61], [2407, 172], [3757, 204]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[2966, 486]]; neon mask 0.531 % of the frame |
| clipped highlights | PASS | 0.0037 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0560 | >= 0.0353 | display |
| mean luma | PASS | 0.1242 | 0.0941..0.1961 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS15_garden_canopy.png`

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

### KS15_garden_canopy_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS15_garden_canopy_1610.png`

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

### KS15_garden_canopy_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS15_garden_canopy_219.png`

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

### KS15_garden_canopy_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS15_garden_canopy_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 36 | 0 | bright 20 (mode all), dark 16, first at [[1402, 33], [2355, 214], [236, 321], [3525, 326], [184, 456]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.495 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0544 | >= 0.0353 | display |
| mean luma | PASS | 0.1270 | 0.0784..0.1725 | display |

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS16_signs_wall.png`

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

### KS16_signs_wall_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS16_signs_wall_1610.png`

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

### KS16_signs_wall_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS16_signs_wall_219.png`

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

### KS16_signs_wall_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS16_signs_wall_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 6 (mode all), dark 5, first at [[451, 239], [645, 242], [648, 244], [47, 357], [41, 387]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.125 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | PASS | 0.1972 | 0.1255..0.251 | display |
| banding[wall_panel] | FAIL | 3.2110 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 4.82 (run 7 px), - / -, 7 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS17_heart_reveal.png`

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
| mean luma | PASS | 0.1483 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.7760 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.16 (run 11 px), - / -, 6 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.02 (run 9 px) not gated (textured: gate steps), - / -, 22 strips |

### KS17_heart_reveal_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS17_heart_reveal_1610.png`

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

### KS17_heart_reveal_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS17_heart_reveal_219.png`

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

### KS17_heart_reveal_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS17_heart_reveal_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 0 (mode all), dark 3, first at [[1866, 76], [1653, 653], [1881, 801]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.221 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0569 | >= 0.0353 | display |
| mean luma | PASS | 0.1470 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.5730 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.86 (run 11 px), - / -, 12 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.93 (run 9 px) not gated (textured: gate steps), - / -, 45 strips |

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS18_koi_pass.png`

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
| mean luma | PASS | 0.1965 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.5770 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.87 (run 9 px), - / -, 9 strips |

### KS18_koi_pass_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS18_koi_pass_1610.png`

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

### KS18_koi_pass_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS18_koi_pass_219.png`

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

### KS18_koi_pass_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS18_koi_pass_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 0 (mode all), dark 3, first at [[1318, 1488], [2654, 1619], [2733, 1831]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.028 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0706 | >= 0.0353 | display |
| mean luma | PASS | 0.1948 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.5290 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.79 (run 9 px), - / -, 18 strips |

### KS19_goal_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS19_goal_window.png`

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

### KS19_goal_window_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS19_goal_window_1610.png`

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

### KS19_goal_window_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS19_goal_window_219.png`

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

### KS19_goal_window_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\KS19_goal_window_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 72 | 0 | bright 0 (mode all), dark 72, first at [[1666, 1004], [1644, 1014], [1693, 1033], [1612, 1036], [1839, 1041]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.655 % of the frame |
| clipped highlights | PASS | 0.0029 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0577 | >= 0.0353 | display |
| mean luma | PASS | 0.1211 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.1260 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.19 (run 7 px), - / -, 4 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
