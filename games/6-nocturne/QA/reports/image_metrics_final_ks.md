# Image metrics: final_ks

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks`  
Scene constants: NocturneConstants.cs  
Images: 80, failing images: 56. Checks: 697 PASS, 83 FAIL, 45 WARN, 28 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS00_alley_start | 1 | 1920x1080 | 0.117 | 0.060 | 0.0012 | 2.000 | FAIL | NaN-like pixels, banding[alley_wall] |
| KS00_alley_start_1610 | 1 | 1680x1050 | 0.119 | 0.060 | 0.0014 | 0.793 | FAIL | NaN-like pixels |
| KS00_alley_start_219 | 1 | 2560x1080 | 0.112 | 0.062 | 0.0010 | 0.423 | FAIL | NaN-like pixels |
| KS00_alley_start_4k | 1 | 3840x2160 | 0.117 | 0.060 | 0.0010 | 0.689 | WARN |  |
| KS01_alley_noodle | 1 | 1920x1080 | 0.141 | 0.064 | 0.0098 | - | WARN |  |
| KS01_alley_noodle_1610 | 1 | 1680x1050 | 0.136 | 0.064 | 0.0108 | - | WARN |  |
| KS01_alley_noodle_219 | 1 | 2560x1080 | 0.146 | 0.064 | 0.0070 | - | WARN |  |
| KS01_alley_noodle_4k | 1 | 3840x2160 | 0.141 | 0.064 | 0.0094 | - | FAIL | NaN-like pixels |
| KS02_alley_slit | 1 | 1920x1080 | 0.121 | 0.070 | 0.0000 | 0.548 | PASS |  |
| KS02_alley_slit_1610 | 1 | 1680x1050 | 0.122 | 0.069 | 0.0000 | 0.447 | FAIL | isolated pixels > 8x median |
| KS02_alley_slit_219 | 1 | 2560x1080 | 0.117 | 0.059 | 0.0000 | 0.762 | PASS |  |
| KS02_alley_slit_4k | 1 | 3840x2160 | 0.121 | 0.068 | 0.0001 | 0.480 | PASS |  |
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.100 | 0.059 | 0.0000 | 0.102 | WARN |  |
| KS03_boulevard_mouth_1610 | 1 | 1680x1050 | 0.103 | 0.059 | 0.0000 | 0.178 | WARN |  |
| KS03_boulevard_mouth_219 | 1 | 2560x1080 | 0.091 | 0.056 | 0.0000 | 0.000 | WARN |  |
| KS03_boulevard_mouth_4k | 1 | 3840x2160 | 0.100 | 0.058 | 0.0000 | 0.075 | WARN |  |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.139 | 0.060 | 0.0000 | 0.466 | FAIL | NaN-like pixels |
| KS04_boulevard_axis_1610 | 2 | 1680x1050 | 0.143 | 0.062 | 0.0000 | 0.502 | FAIL | NaN-like pixels |
| KS04_boulevard_axis_219 | 2 | 2560x1080 | 0.131 | 0.058 | 0.0000 | 0.750 | FAIL | NaN-like pixels |
| KS04_boulevard_axis_4k | 2 | 3840x2160 | 0.137 | 0.059 | 0.0001 | 0.389 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up | 2 | 1920x1080 | 0.164 | 0.067 | 0.0000 | 0.546 | FAIL | NaN-like pixels |
| KS05_boulevard_up_1610 | 2 | 1680x1050 | 0.169 | 0.068 | 0.0000 | 0.416 | FAIL | NaN-like pixels |
| KS05_boulevard_up_219 | 2 | 2560x1080 | 0.148 | 0.059 | 0.0000 | 1.209 | FAIL | NaN-like pixels, banding[cloud_ceiling] |
| KS05_boulevard_up_4k | 2 | 3840x2160 | 0.162 | 0.067 | 0.0001 | 2.000 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[cloud_ceiling] |
| KS06_tram | 2 | 1920x1080 | 0.132 | 0.062 | 0.0000 | - | FAIL | NaN-like pixels |
| KS06_tram_1610 | 2 | 1680x1050 | 0.134 | 0.062 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram_219 | 2 | 2560x1080 | 0.131 | 0.060 | 0.0000 | - | FAIL | NaN-like pixels |
| KS06_tram_4k | 2 | 3840x2160 | 0.131 | 0.062 | 0.0002 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window | 3 | 1920x1080 | 0.130 | 0.074 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window_1610 | 3 | 1680x1050 | 0.131 | 0.074 | 0.0000 | - | FAIL | NaN-like pixels |
| KS07_edge_window_219 | 3 | 2560x1080 | 0.131 | 0.073 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window_4k | 3 | 3840x2160 | 0.131 | 0.073 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | 3 | 1920x1080 | 0.150 | 0.062 | 0.0037 | - | FAIL | NaN-like pixels |
| KS08_edge_flank_1610 | 3 | 1680x1050 | 0.154 | 0.066 | 0.0043 | - | FAIL | NaN-like pixels |
| KS08_edge_flank_219 | 3 | 2560x1080 | 0.140 | 0.062 | 0.0031 | - | FAIL | NaN-like pixels |
| KS08_edge_flank_4k | 3 | 3840x2160 | 0.150 | 0.062 | 0.0054 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS09_edge_gulf | 3 | 1920x1080 | 0.151 | 0.066 | 0.0005 | 0.455 | FAIL | NaN-like pixels |
| KS09_edge_gulf_1610 | 3 | 1680x1050 | 0.154 | 0.068 | 0.0003 | 0.885 | FAIL | NaN-like pixels |
| KS09_edge_gulf_219 | 3 | 2560x1080 | 0.142 | 0.062 | 0.0002 | 0.621 | FAIL | NaN-like pixels |
| KS09_edge_gulf_4k | 3 | 3840x2160 | 0.152 | 0.066 | 0.0013 | 1.190 | FAIL | NaN-like pixels, banding[abyss] |
| KS10_edge_plunge | 3 | 1920x1080 | 0.144 | 0.059 | 0.0018 | 0.608 | FAIL | NaN-like pixels |
| KS10_edge_plunge_1610 | 3 | 1680x1050 | 0.147 | 0.060 | 0.0024 | 0.544 | FAIL | NaN-like pixels |
| KS10_edge_plunge_219 | 3 | 2560x1080 | 0.137 | 0.059 | 0.0012 | 0.578 | FAIL | NaN-like pixels |
| KS10_edge_plunge_4k | 3 | 3840x2160 | 0.145 | 0.059 | 0.0041 | 0.641 | FAIL | NaN-like pixels |
| KS11_bridge_train | 4 | 1920x1080 | 0.119 | 0.066 | 0.0056 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train_1610 | 4 | 1680x1050 | 0.118 | 0.066 | 0.0065 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train_219 | 4 | 2560x1080 | 0.121 | 0.066 | 0.0044 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train_4k | 4 | 3840x2160 | 0.119 | 0.066 | 0.0059 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void | 4 | 1920x1080 | 0.127 | 0.063 | 0.0000 | 2.139 | FAIL | NaN-like pixels, banding[abyss] |
| KS12_bridge_void_1610 | 4 | 1680x1050 | 0.129 | 0.063 | 0.0000 | 1.858 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS12_bridge_void_219 | 4 | 2560x1080 | 0.123 | 0.063 | 0.0000 | 1.943 | FAIL | NaN-like pixels, banding[abyss] |
| KS12_bridge_void_4k | 4 | 3840x2160 | 0.128 | 0.063 | 0.0001 | 4.248 | FAIL | NaN-like pixels, banding[abyss] |
| KS13_bridge_glass | 4 | 1920x1080 | 0.135 | 0.067 | 0.0023 | 0.882 | FAIL | NaN-like pixels |
| KS13_bridge_glass_1610 | 4 | 1680x1050 | 0.141 | 0.068 | 0.0027 | 0.872 | FAIL | NaN-like pixels |
| KS13_bridge_glass_219 | 4 | 2560x1080 | 0.135 | 0.065 | 0.0012 | 1.641 | FAIL | NaN-like pixels, banding[abyss] |
| KS13_bridge_glass_4k | 4 | 3840x2160 | 0.135 | 0.067 | 0.0045 | 1.516 | FAIL | NaN-like pixels, banding[abyss] |
| KS14_garden_gate | 4 | 1920x1080 | 0.132 | 0.056 | 0.0037 | - | FAIL | NaN-like pixels |
| KS14_garden_gate_1610 | 4 | 1680x1050 | 0.135 | 0.056 | 0.0042 | - | FAIL | NaN-like pixels |
| KS14_garden_gate_219 | 4 | 2560x1080 | 0.124 | 0.058 | 0.0029 | - | FAIL | NaN-like pixels |
| KS14_garden_gate_4k | 4 | 3840x2160 | 0.133 | 0.057 | 0.0037 | - | FAIL | NaN-like pixels |
| KS15_garden_canopy | 5 | 1920x1080 | 0.127 | 0.054 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_1610 | 5 | 1680x1050 | 0.129 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_219 | 5 | 2560x1080 | 0.118 | 0.052 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_4k | 5 | 3840x2160 | 0.128 | 0.054 | 0.0001 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS16_signs_wall | 6 | 1920x1080 | 0.216 | 0.066 | 0.0000 | 0.095 | FAIL | NaN-like pixels |
| KS16_signs_wall_1610 | 6 | 1680x1050 | 0.225 | 0.066 | 0.0000 | 0.114 | WARN |  |
| KS16_signs_wall_219 | 6 | 2560x1080 | 0.192 | 0.059 | 0.0000 | 0.095 | FAIL | NaN-like pixels |
| KS16_signs_wall_4k | 6 | 3840x2160 | 0.213 | 0.064 | 0.0000 | 0.454 | FAIL | NaN-like pixels |
| KS17_heart_reveal | 7 | 1920x1080 | 0.158 | 0.058 | 0.0001 | 0.577 | PASS |  |
| KS17_heart_reveal_1610 | 7 | 1680x1050 | 0.164 | 0.058 | 0.0002 | 0.547 | PASS |  |
| KS17_heart_reveal_219 | 7 | 2560x1080 | 0.149 | 0.057 | 0.0001 | 0.669 | PASS |  |
| KS17_heart_reveal_4k | 7 | 3840x2160 | 0.157 | 0.057 | 0.0003 | 0.512 | WARN |  |
| KS18_koi_pass | 7 | 1920x1080 | 0.209 | 0.071 | 0.0000 | 0.485 | PASS |  |
| KS18_koi_pass_1610 | 7 | 1680x1050 | 0.217 | 0.070 | 0.0000 | 0.506 | PASS |  |
| KS18_koi_pass_219 | 7 | 2560x1080 | 0.189 | 0.069 | 0.0000 | 0.627 | PASS |  |
| KS18_koi_pass_4k | 7 | 3840x2160 | 0.207 | 0.072 | 0.0000 | 0.457 | WARN |  |
| KS19_goal_window | 7 | 1920x1080 | 0.126 | 0.058 | 0.0037 | 0.073 | WARN |  |
| KS19_goal_window_1610 | 7 | 1680x1050 | 0.126 | 0.058 | 0.0040 | 0.232 | WARN |  |
| KS19_goal_window_219 | 7 | 2560x1080 | 0.122 | 0.058 | 0.0025 | 0.501 | WARN |  |
| KS19_goal_window_4k | 7 | 3840x2160 | 0.127 | 0.058 | 0.0036 | 0.127 | WARN |  |

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

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS00_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 2 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1073, 469], [977, 472]]; owner AL (beat 1) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[976, 441]]; thin-line mask 0.174 %; owner AL (beat 1) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.093 % of the frame, thin-line mask 0.174 % |
| clipped highlights | PASS | 0.0012 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0602 | >= 0.0353 | display |
| mean luma | PASS | 0.1170 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 53.0 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.3400 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.51 (run 7 px), - / -, 1 strips |
| banding[alley_wall] | FAIL | 2.0000 | score <= 1 | visible steps/1000 1.85 (max 1.01 code), BandScore 0.72 (run 6 px), - / -, 6 strips |

### KS00_alley_start_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS00_alley_start_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[856, 463]]; owner AL (beat 1) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[755, 284]]; thin-line mask 0.194 %; owner AL (beat 1) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.108 % of the frame, thin-line mask 0.194 % |
| clipped highlights | PASS | 0.0014 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0602 | >= 0.0353 | display |
| mean luma | PASS | 0.1186 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 494.5000 |  | sky band not uniform (spread 53.0 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.2810 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.42 (run 6 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.7930 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.19 (run 9 px), - / -, 5 strips |

### KS00_alley_start_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS00_alley_start_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 2 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1393, 469], [1297, 472]]; owner AL (beat 1) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1296, 441]]; thin-line mask 0.130 %; owner AL (beat 1) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.072 % of the frame, thin-line mask 0.130 % |
| clipped highlights | PASS | 0.0010 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1119 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 508.6000 |  | sky band not uniform (spread 51.0 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.2630 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.39 (run 6 px), - / -, 1 strips |
| banding[alley_wall] | PASS | 0.4230 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.63 (run 9 px), - / -, 8 strips |

### KS00_alley_start_4k (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS00_alley_start_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 4 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1167, 766], [1187, 767], [1951, 936], [1951, 940]]; owner AL (beat 1) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.120 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.105 % of the frame, thin-line mask 0.120 % |
| clipped highlights | PASS | 0.0010 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0599 | >= 0.0353 | display |
| mean luma | PASS | 0.1171 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 1017.2000 |  | sky band not uniform (spread 52.5 codes): objects on the horizon |
| banding[mouth_glow] | PASS | 0.6200 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.93 (run 8 px), - / -, 2 strips |
| banding[alley_wall] | PASS | 0.6890 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.03 (run 7 px), - / -, 13 strips |

### KS01_alley_noodle (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS01_alley_noodle.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 10 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1907, 296], [1914, 357], [1911, 358], [1899, 362], [1214, 520]]; owner AL (beat 1) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.162 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.102 % of the frame, thin-line mask 0.162 % |
| clipped highlights | PASS | 0.0098 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0639 | >= 0.0353 | display |
| mean luma | PASS | 0.1408 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 97.0 codes): objects on the horizon |

### KS01_alley_noodle_1610 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS01_alley_noodle_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 4 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1087, 513], [1087, 516], [1080, 548], [1139, 551]]; owner AL (beat 1) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.182 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.128 % of the frame, thin-line mask 0.182 % |
| clipped highlights | PASS | 0.0108 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0641 | >= 0.0353 | display |
| mean luma | PASS | 0.1362 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 586.1000 |  | sky band not uniform (spread 100.0 codes): objects on the horizon |

### KS01_alley_noodle_219 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS01_alley_noodle_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 45 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[2374, 114], [2320, 153], [2310, 180], [2456, 231], [2450, 234]]; owner AL (beat 1) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.244 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.092 % of the frame, thin-line mask 0.244 % |
| clipped highlights | PASS | 0.0070 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0641 | >= 0.0353 | display |
| mean luma | PASS | 0.1462 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 602.8000 |  | sky band not uniform (spread 99.0 codes): objects on the horizon |

### KS01_alley_noodle_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS01_alley_noodle_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 12 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[3703, 932], [3805, 953], [3794, 965], [3758, 982], [3790, 992]]; owner AL (beat 1) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[2546, 628], [2532, 1122]]; thin-line mask 0.116 %; owner AL (beat 1) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.040 % of the frame, thin-line mask 0.116 % |
| clipped highlights | PASS | 0.0094 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0636 | >= 0.0353 | display |
| mean luma | PASS | 0.1406 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 1205.7000 |  | sky band not uniform (spread 100.0 codes): objects on the horizon |

### KS02_alley_slit (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS02_alley_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.217 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.172 % of the frame, thin-line mask 0.217 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0698 | >= 0.0353 | display |
| mean luma | PASS | 0.1206 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.5480 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.82 (run 9 px), - / -, 1 strips |

### KS02_alley_slit_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS02_alley_slit_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.266 % |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[991, 912]]; neon mask 0.198 % of the frame, thin-line mask 0.266 %; owner AL (beat 1): real 1-2 px emitters |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0692 | >= 0.0353 | display |
| mean luma | PASS | 0.1220 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.4470 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.67 (run 7 px), - / -, 1 strips |

### KS02_alley_slit_219 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS02_alley_slit_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.168 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.130 % of the frame, thin-line mask 0.168 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0594 | >= 0.0353 | display |
| mean luma | PASS | 0.1169 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.7620 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.14 (run 8 px), - / -, 2 strips |

### KS02_alley_slit_4k (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS02_alley_slit_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.102 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.065 % of the frame, thin-line mask 0.102 % |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0675 | >= 0.0353 | display |
| mean luma | PASS | 0.1207 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[slit_cloud] | PASS | 0.4800 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.72 (run 8 px), - / -, 3 strips |

### KS03_boulevard_mouth (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1669, 755]]; owner AL (beat 1) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 1.003 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.889 % of the frame, thin-line mask 1.003 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.0996 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 121.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.1020 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.15 (run 7 px), - / -, 2 strips |

### KS03_boulevard_mouth_1610 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS03_boulevard_mouth_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 2 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[770, 730], [1530, 737]]; owner AL (beat 1) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 1.161 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.101 % of the frame, thin-line mask 1.161 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.1031 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 735.3000 |  | sky band not uniform (spread 122.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.1780 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.27 (run 5 px), - / -, 2 strips |

### KS03_boulevard_mouth_219 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS03_boulevard_mouth_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1989, 755]]; owner AL (beat 1) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.752 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.670 % of the frame, thin-line mask 0.752 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0558 | >= 0.0353 | display |
| mean luma | PASS | 0.0907 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 118.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.00 (run 0 px), - / -, 3 strips |

### KS03_boulevard_mouth_4k (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS03_boulevard_mouth_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[2186, 1502]]; owner AL (beat 1) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.471 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.186 % of the frame, thin-line mask 0.471 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.0996 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 1512.6000 |  | sky band not uniform (spread 122.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.0750 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.11 (run 5 px), - / -, 5 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 23 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1574, 953], [1532, 955], [1542, 956], [1575, 956], [1603, 957]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[904, 569], [963, 959]]; thin-line mask 1.586 %; owner BV (beat 2) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.177 % of the frame, thin-line mask 1.586 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0597 | >= 0.0353 | display |
| mean luma | PASS | 0.1394 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 963.7000 |  | sky band not uniform (spread 152.5 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.4660 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.70 (run 7 px), - / -, 3 strips |

### KS04_boulevard_axis_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS04_boulevard_axis_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 22 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[141, 928], [1538, 929], [1429, 933], [1439, 933], [1466, 933]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[752, 577], [763, 596], [834, 942]]; thin-line mask 1.714 %; owner BV (beat 2) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.453 % of the frame, thin-line mask 1.714 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1435 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 936.9000 |  | sky band not uniform (spread 159.4 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.5020 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.75 (run 9 px), - / -, 3 strips |

### KS04_boulevard_axis_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS04_boulevard_axis_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 23 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1894, 953], [1852, 955], [1862, 956], [1895, 956], [1923, 957]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1224, 569], [2245, 892], [1283, 959]]; thin-line mask 1.300 %; owner BV (beat 2) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.643 % of the frame, thin-line mask 1.300 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0577 | >= 0.0353 | display |
| mean luma | PASS | 0.1314 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 963.7000 |  | sky band not uniform (spread 118.0 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.7500 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.12 (run 12 px), - / -, 5 strips |

### KS04_boulevard_axis_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS04_boulevard_axis_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 12 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[2708, 344], [1838, 1832], [3773, 1865], [1266, 1905], [1307, 1907]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 11 | 0 | bright 11 (mode all), dark 0, first at [[1656, 975], [2214, 995], [2238, 1256], [2248, 1256], [1831, 1296]]; thin-line mask 0.786 %; owner BV (beat 2) |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon and thin-line masks | first at [[1627, 1455], [1406, 1536], [3737, 1885]]; neon mask 0.990 % of the frame, thin-line mask 0.786 %; owner BV (beat 2): real 1-2 px emitters |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0594 | >= 0.0353 | display |
| mean luma | PASS | 0.1370 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 1927.4000 |  | sky band not uniform (spread 135.9 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.3890 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.58 (run 9 px), - / -, 7 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1485, 158]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1431, 1008], [877, 1074], [878, 1079]]; thin-line mask 1.501 %; owner BV (beat 2) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.070 % of the frame, thin-line mask 1.501 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1644 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.5460 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.82 (run 7 px), - / -, 4 strips |

### KS05_boulevard_up_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS05_boulevard_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1295, 982]]; thin-line mask 1.581 %; owner BV (beat 2) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.378 % of the frame, thin-line mask 1.581 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0683 | >= 0.0353 | display |
| mean luma | PASS | 0.1687 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.4160 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.62 (run 6 px), - / -, 4 strips |

### KS05_boulevard_up_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS05_boulevard_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1805, 158]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1751, 1008], [1197, 1074], [1198, 1079]]; thin-line mask 1.250 %; owner BV (beat 2) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.573 % of the frame, thin-line mask 1.250 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0594 | >= 0.0353 | display |
| mean luma | PASS | 0.1483 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | FAIL | 1.2090 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.81 (run 7 px), - / -, 6 strips |

### KS05_boulevard_up_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS05_boulevard_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[458, 1891]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 11 | 0 | bright 11 (mode all), dark 0, first at [[1620, 1684], [1616, 1708], [1391, 1810], [1600, 1810], [2541, 1810]]; thin-line mask 1.031 %; owner BV (beat 2) |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[2317, 1985]]; neon mask 0.717 % of the frame, thin-line mask 1.031 %; owner BV (beat 2): real 1-2 px emitters |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0670 | >= 0.0353 | display |
| mean luma | PASS | 0.1622 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | FAIL | 2.0000 | score <= 1 | visible steps/1000 1.49 (max 1.17 code), BandScore 0.85 (run 9 px), - / -, 9 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[541, 738]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[1499, 584], [1409, 725], [1510, 736], [1718, 751], [915, 898]]; thin-line mask 1.418 %; owner BV (beat 2) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.958 % of the frame, thin-line mask 1.418 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1324 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 117.0 codes): objects on the horizon |

### KS06_tram_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS06_tram_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1367, 706], [1375, 716], [1374, 732], [768, 870]]; thin-line mask 1.492 %; owner BV (beat 2) |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon and thin-line masks | first at [[1073, 515], [770, 709], [771, 709]]; neon mask 2.148 % of the frame, thin-line mask 1.492 %; owner BV (beat 2): real 1-2 px emitters |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | PASS | 0.1342 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 726.6000 |  | sky band not uniform (spread 116.0 codes): objects on the horizon |

### KS06_tram_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS06_tram_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 2 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[861, 738], [1420, 744]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1729, 725], [1830, 736], [2038, 751], [1235, 898]]; thin-line mask 1.335 %; owner BV (beat 2) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.616 % of the frame, thin-line mask 1.335 % |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0605 | >= 0.0353 | display |
| mean luma | PASS | 0.1310 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 117.0 codes): objects on the horizon |

### KS06_tram_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS06_tram_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1282, 2013]]; owner BV (beat 2) |
| NaN-like pixels | FAIL | 15 | 0 | bright 15 (mode all), dark 0, first at [[3781, 274], [3831, 432], [3406, 531], [3389, 535], [2039, 538]]; thin-line mask 0.798 %; owner BV (beat 2) |
| isolated pixels > 8x median | FAIL | 6 | 0 outside neon and thin-line masks | first at [[3428, 622], [2828, 1094], [2828, 1095], [1766, 1457], [2136, 1468]]; neon mask 0.889 % of the frame, thin-line mask 0.798 %; owner BV (beat 2): real 1-2 px emitters |
| clipped highlights | PASS | 0.0002 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1305 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 1494.6000 |  | sky band not uniform (spread 117.0 codes): objects on the horizon |

### KS07_edge_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS07_edge_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1762, 291]]; thin-line mask 2.543 %; owner AB (beat 3) |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[881, 312]]; neon mask 4.322 % of the frame, thin-line mask 2.543 %; owner AB (beat 3): real 1-2 px emitters |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0737 | >= 0.0353 | display |
| mean luma | PASS | 0.1303 | 0.0941..0.1961 | display |

### KS07_edge_window_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS07_edge_window_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[660, 490]]; thin-line mask 2.479 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 4.091 % of the frame, thin-line mask 2.479 % |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0740 | >= 0.0353 | display |
| mean luma | PASS | 0.1307 | 0.0941..0.1961 | display |

### KS07_edge_window_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS07_edge_window_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[2082, 291]]; thin-line mask 2.578 %; owner AB (beat 3) |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[1201, 312]]; neon mask 4.790 % of the frame, thin-line mask 2.578 %; owner AB (beat 3): real 1-2 px emitters |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0734 | >= 0.0353 | display |
| mean luma | PASS | 0.1311 | 0.0941..0.1961 | display |

### KS07_edge_window_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS07_edge_window_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1746, 1014]]; owner AB (beat 3) |
| NaN-like pixels | FAIL | 17 | 0 | bright 17 (mode all), dark 0, first at [[1558, 29], [1299, 61], [2336, 61], [2636, 61], [1544, 125]]; thin-line mask 1.277 %; owner AB (beat 3) |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon and thin-line masks | first at [[1290, 743], [1289, 809], [2652, 1075], [2666, 1109]]; neon mask 2.184 % of the frame, thin-line mask 1.277 %; owner AB (beat 3): real 1-2 px emitters |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0734 | >= 0.0353 | display |
| mean luma | PASS | 0.1313 | 0.0941..0.1961 | display |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[598, 21], [549, 255], [515, 382], [515, 385], [481, 512]]; thin-line mask 1.588 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 3.890 % of the frame, thin-line mask 1.588 % |
| clipped highlights | PASS | 0.0037 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1501 | 0.0941..0.1961 | display |

### KS08_edge_flank_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS08_edge_flank_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[484, 30], [465, 105], [440, 251], [373, 501], [982, 519]]; thin-line mask 1.629 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 4.270 % of the frame, thin-line mask 1.629 % |
| clipped highlights | PASS | 0.0043 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0656 | >= 0.0353 | display |
| mean luma | PASS | 0.1539 | 0.0941..0.1961 | display |

### KS08_edge_flank_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS08_edge_flank_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[918, 21], [869, 255], [835, 382], [835, 385], [801, 512]]; thin-line mask 1.466 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 3.235 % of the frame, thin-line mask 1.466 % |
| clipped highlights | PASS | 0.0031 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | PASS | 0.1400 | 0.0941..0.1961 | display |

### KS08_edge_flank_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS08_edge_flank_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[3302, 372], [3035, 600], [3040, 662], [3148, 923], [952, 1087]]; thin-line mask 1.051 %; owner AB (beat 3) |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon and thin-line masks | first at [[3344, 1485], [3324, 1488], [3325, 1488], [3375, 1574]]; neon mask 1.372 % of the frame, thin-line mask 1.051 %; owner AB (beat 3): real 1-2 px emitters |
| clipped highlights | PASS | 0.0054 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1496 | 0.0941..0.1961 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[1126, 55], [743, 67], [1062, 112], [1181, 271], [379, 517]]; thin-line mask 1.313 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 3.557 % of the frame, thin-line mask 1.313 % |
| clipped highlights | PASS | 0.0005 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0664 | >= 0.0353 | display |
| mean luma | PASS | 0.1511 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.4550 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.68 (run 6 px), - / -, 4 strips |

### KS09_edge_gulf_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS09_edge_gulf_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[0, 491]]; owner AB (beat 3) |
| NaN-like pixels | FAIL | 11 | 0 | bright 11 (mode all), dark 0, first at [[942, 29], [941, 32], [1002, 52], [629, 65], [939, 105]]; thin-line mask 1.367 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 3.937 % of the frame, thin-line mask 1.367 % |
| clipped highlights | PASS | 0.0003 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0675 | >= 0.0353 | display |
| mean luma | PASS | 0.1543 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.8850 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.33 (run 6 px), - / -, 3 strips |

### KS09_edge_gulf_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS09_edge_gulf_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 10 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[2559, 551], [2552, 553], [2548, 554], [2533, 558], [2529, 559]]; owner AB (beat 3) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[1446, 55], [1063, 67], [1382, 112], [1501, 271], [699, 517]]; thin-line mask 1.133 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.718 % of the frame, thin-line mask 1.133 % |
| clipped highlights | PASS | 0.0002 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | PASS | 0.1424 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.6210 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.93 (run 7 px), - / -, 6 strips |

### KS09_edge_gulf_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS09_edge_gulf_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 17 | 0 | bright 17 (mode all), dark 0, first at [[1399, 13], [1665, 20], [1657, 45], [1646, 61], [1636, 64]]; thin-line mask 0.630 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.394 % of the frame, thin-line mask 0.630 % |
| clipped highlights | PASS | 0.0013 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0658 | >= 0.0353 | display |
| mean luma | PASS | 0.1521 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.1900 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.78 (run 7 px), - / -, 9 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1427, 341], [762, 422], [659, 660]]; thin-line mask 0.954 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.262 % of the frame, thin-line mask 0.954 % |
| clipped highlights | PASS | 0.0018 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0594 | >= 0.0353 | display |
| mean luma | PASS | 0.1436 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.6080 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.91 (run 7 px), - / -, 3 strips |

### KS10_edge_plunge_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS10_edge_plunge_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[0, 627]]; owner AB (beat 3) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[648, 410]]; thin-line mask 1.005 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.526 % of the frame, thin-line mask 1.005 % |
| clipped highlights | PASS | 0.0024 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0600 | >= 0.0353 | display |
| mean luma | PASS | 0.1468 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.5440 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.82 (run 7 px), - / -, 3 strips |

### KS10_edge_plunge_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS10_edge_plunge_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1747, 341], [1082, 422], [979, 660]]; thin-line mask 0.827 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.720 % of the frame, thin-line mask 0.827 % |
| clipped highlights | PASS | 0.0012 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.1371 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.5780 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.87 (run 6 px), - / -, 3 strips |

### KS10_edge_plunge_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS10_edge_plunge_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1914, 614], [1525, 844]]; thin-line mask 0.362 %; owner AB (beat 3) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.885 % of the frame, thin-line mask 0.362 % |
| clipped highlights | PASS | 0.0041 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0591 | >= 0.0353 | display |
| mean luma | PASS | 0.1445 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.6410 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.96 (run 10 px), - / -, 6 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1419, 605]]; owner AB / TR (beat 4) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[1868, 297], [1176, 317], [1550, 430], [726, 504], [1553, 544]]; thin-line mask 2.119 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | FAIL | 5 | 0 outside neon and thin-line masks | first at [[1086, 101], [1087, 101], [896, 243], [887, 336], [1535, 346]]; neon mask 2.668 % of the frame, thin-line mask 2.119 %; owner AB / TR (beat 4): real 1-2 px emitters |
| clipped highlights | PASS | 0.0056 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0661 | >= 0.0353 | display |
| mean luma | PASS | 0.1186 | 0.0941..0.1961 | display |

### KS11_bridge_train_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS11_bridge_train_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[617, 40], [1409, 227], [1024, 515], [1410, 781]]; thin-line mask 2.099 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[778, 236]]; neon mask 2.578 % of the frame, thin-line mask 2.099 %; owner AB / TR (beat 4): real 1-2 px emitters |
| clipped highlights | PASS | 0.0065 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0661 | >= 0.0353 | display |
| mean luma | PASS | 0.1181 | 0.0941..0.1961 | display |

### KS11_bridge_train_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS11_bridge_train_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1739, 605]]; owner AB / TR (beat 4) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[2188, 297], [1496, 317], [1870, 430], [1046, 504], [1873, 544]]; thin-line mask 1.904 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | FAIL | 5 | 0 outside neon and thin-line masks | first at [[1406, 101], [1407, 101], [1216, 243], [1207, 336], [1855, 346]]; neon mask 2.591 % of the frame, thin-line mask 1.904 %; owner AB / TR (beat 4): real 1-2 px emitters |
| clipped highlights | PASS | 0.0044 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0658 | >= 0.0353 | display |
| mean luma | PASS | 0.1214 | 0.0941..0.1961 | display |

### KS11_bridge_train_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS11_bridge_train_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 2 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1658, 858], [2179, 1004]]; owner AB / TR (beat 4) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[2469, 846], [2039, 982]]; thin-line mask 1.096 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon and thin-line masks | first at [[1478, 37], [3750, 961]]; neon mask 1.395 % of the frame, thin-line mask 1.096 %; owner AB / TR (beat 4): real 1-2 px emitters |
| clipped highlights | PASS | 0.0059 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0661 | >= 0.0353 | display |
| mean luma | PASS | 0.1190 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[1330, 12], [993, 14], [1327, 16], [978, 19], [976, 44]]; thin-line mask 0.429 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.156 % of the frame, thin-line mask 0.429 % |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | PASS | 0.1270 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 2.1390 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 3.21 (run 6 px), - / -, 3 strips |

### KS12_bridge_void_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS12_bridge_void_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1203, 0], [870, 27]]; thin-line mask 0.471 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon and thin-line masks | first at [[55, 546], [55, 547]]; neon mask 1.340 % of the frame, thin-line mask 0.471 %; owner AB / TR (beat 4): real 1-2 px emitters |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | PASS | 0.1289 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.8580 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.79 (run 5 px), - / -, 2 strips |

### KS12_bridge_void_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS12_bridge_void_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[1650, 12], [1313, 14], [1647, 16], [1298, 19], [1296, 44]]; thin-line mask 0.409 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.891 % of the frame, thin-line mask 0.409 % |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | PASS | 0.1231 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.9430 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.91 (run 7 px), - / -, 3 strips |

### KS12_bridge_void_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS12_bridge_void_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[1864, 2], [2077, 2], [2073, 14], [2068, 29], [1870, 53]]; thin-line mask 0.126 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.437 % of the frame, thin-line mask 0.126 % |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | PASS | 0.1275 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 4.2480 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 6.37 (run 6 px), - / -, 6 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[887, 727]]; thin-line mask 0.270 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.433 % of the frame, thin-line mask 0.270 % |
| clipped highlights | PASS | 0.0023 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0672 | >= 0.0353 | display |
| mean luma | PASS | 0.1355 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.8820 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.32 (run 6 px), - / -, 1 strips |

### KS13_bridge_glass_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS13_bridge_glass_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[502, 3], [502, 29], [1168, 117], [769, 707]]; thin-line mask 0.304 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.604 % of the frame, thin-line mask 0.304 % |
| clipped highlights | PASS | 0.0027 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0681 | >= 0.0353 | display |
| mean luma | PASS | 0.1409 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.8720 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.31 (run 5 px), - / -, 1 strips |

### KS13_bridge_glass_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS13_bridge_glass_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1207, 727]]; thin-line mask 0.246 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.291 % of the frame, thin-line mask 0.246 % |
| clipped highlights | PASS | 0.0012 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0647 | >= 0.0353 | display |
| mean luma | PASS | 0.1353 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.6410 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.46 (run 9 px), - / -, 2 strips |

### KS13_bridge_glass_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS13_bridge_glass_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[1761, 458], [2436, 544], [1259, 683], [1273, 683], [1582, 813]]; thin-line mask 0.169 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.717 % of the frame, thin-line mask 0.169 % |
| clipped highlights | PASS | 0.0045 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0669 | >= 0.0353 | display |
| mean luma | PASS | 0.1353 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.5160 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.27 (run 7 px), - / -, 3 strips |

### KS14_garden_gate (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1745, 177], [1354, 205], [1252, 216], [291, 290]]; thin-line mask 1.077 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.180 % of the frame, thin-line mask 1.077 % |
| clipped highlights | PASS | 0.0037 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0561 | >= 0.0353 | display |
| mean luma | PASS | 0.1322 | 0.0941..0.1961 | display |

### KS14_garden_gate_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS14_garden_gate_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 4 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1315, 78], [43, 180], [173, 224], [1354, 479]]; owner AB / TR (beat 4) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1124, 210], [190, 282], [428, 341]]; thin-line mask 1.087 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.501 % of the frame, thin-line mask 1.087 % |
| clipped highlights | PASS | 0.0042 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0558 | >= 0.0353 | display |
| mean luma | PASS | 0.1348 | 0.0941..0.1961 | display |

### KS14_garden_gate_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS14_garden_gate_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1674, 205], [1572, 216], [611, 290]]; thin-line mask 0.951 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.824 % of the frame, thin-line mask 0.951 % |
| clipped highlights | PASS | 0.0029 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0577 | >= 0.0353 | display |
| mean luma | PASS | 0.1238 | 0.0941..0.1961 | display |

### KS14_garden_gate_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS14_garden_gate_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 14 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[885, 19], [887, 61], [2407, 172], [3217, 223], [1437, 328]]; owner AB / TR (beat 4) |
| NaN-like pixels | FAIL | 8 | 0 | bright 8 (mode all), dark 0, first at [[340, 254], [333, 279], [2425, 366], [3611, 384], [598, 662]]; thin-line mask 0.488 %; owner AB / TR (beat 4) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.400 % of the frame, thin-line mask 0.488 % |
| clipped highlights | PASS | 0.0037 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0566 | >= 0.0353 | display |
| mean luma | PASS | 0.1328 | 0.0941..0.1961 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 6 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1465, 93], [1457, 115], [120, 158], [118, 310], [87, 332]]; owner GD (beat 5) |
| NaN-like pixels | FAIL | 8 | 0 | bright 8 (mode all), dark 0, first at [[460, 87], [468, 88], [1191, 431], [1082, 523], [1082, 530]]; thin-line mask 1.767 %; owner GD (beat 5) |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[1394, 1008]]; neon mask 2.573 % of the frame, thin-line mask 1.767 %; owner GD (beat 5): real 1-2 px emitters |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0544 | >= 0.0353 | display |
| mean luma | PASS | 0.1268 | 0.0784..0.1725 | display |

### KS15_garden_canopy_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS15_garden_canopy_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 5 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1331, 93], [23, 154], [2, 298], [13, 321], [1574, 420]]; owner GD (beat 5) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[588, 18], [515, 493], [342, 579], [522, 770], [1245, 970]]; thin-line mask 1.874 %; owner GD (beat 5) |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon and thin-line masks | first at [[1509, 732], [1262, 980]]; neon mask 2.938 % of the frame, thin-line mask 1.874 %; owner GD (beat 5): real 1-2 px emitters |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0546 | >= 0.0353 | display |
| mean luma | PASS | 0.1293 | 0.0784..0.1725 | display |

### KS15_garden_canopy_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS15_garden_canopy_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 6 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1785, 93], [1777, 115], [440, 158], [438, 310], [407, 332]]; owner GD (beat 5) |
| NaN-like pixels | FAIL | 8 | 0 | bright 8 (mode all), dark 0, first at [[780, 87], [788, 88], [1511, 431], [1402, 523], [1402, 530]]; thin-line mask 1.482 %; owner GD (beat 5) |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[1714, 1008]]; neon mask 2.024 % of the frame, thin-line mask 1.482 %; owner GD (beat 5): real 1-2 px emitters |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0521 | >= 0.0353 | display |
| mean luma | PASS | 0.1175 | 0.0784..0.1725 | display |

### KS15_garden_canopy_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS15_garden_canopy_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 26 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[2959, 166], [2911, 217], [236, 321], [3525, 326], [184, 456]]; owner GD (beat 5) |
| NaN-like pixels | FAIL | 18 | 0 | bright 18 (mode all), dark 0, first at [[1402, 33], [943, 172], [2382, 862], [1300, 906], [3024, 987]]; thin-line mask 1.040 %; owner GD (beat 5) |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[1402, 33]]; neon mask 0.332 % of the frame, thin-line mask 1.040 %; owner GD (beat 5): real 1-2 px emitters |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0544 | >= 0.0353 | display |
| mean luma | PASS | 0.1276 | 0.0784..0.1725 | display |

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS16_signs_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 7 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1209, 426], [1211, 429], [1320, 526], [1382, 1070], [1411, 1071]]; owner SG (beat 6) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1841, 1019], [1743, 1064]]; thin-line mask 1.359 %; owner SG (beat 6) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.418 % of the frame, thin-line mask 1.359 % |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0656 | >= 0.0353 | display |
| mean luma | PASS | 0.2158 | 0.1255..0.251 | display |
| banding[wall_panel] | PASS | 0.0950 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.14 (run 6 px), - / -, 1 strips |

### KS16_signs_wall_1610 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS16_signs_wall_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1250, 1039]]; owner SG (beat 6) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 1.360 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.301 % of the frame, thin-line mask 1.360 % |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0658 | >= 0.0353 | display |
| mean luma | PASS | 0.2248 | 0.1255..0.251 | display |
| banding[wall_panel] | PASS | 0.1140 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.17 (run 6 px), - / -, 1 strips |

### KS16_signs_wall_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS16_signs_wall_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 8 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[45, 139], [1529, 426], [1531, 429], [1640, 526], [1702, 1070]]; owner SG (beat 6) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[58, 820], [56, 823], [2161, 1019], [2063, 1064]]; thin-line mask 1.436 %; owner SG (beat 6) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.683 % of the frame, thin-line mask 1.436 % |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0591 | >= 0.0353 | display |
| mean luma | PASS | 0.1917 | 0.1255..0.251 | display |
| banding[wall_panel] | PASS | 0.0950 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.14 (run 5 px), - / -, 1 strips |

### KS16_signs_wall_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS16_signs_wall_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 5 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[81, 442], [2237, 449], [2631, 1020], [2644, 1064], [2797, 1675]]; owner SG (beat 6) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[343, 317], [154, 366]]; thin-line mask 0.748 %; owner SG (beat 6) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.098 % of the frame, thin-line mask 0.748 % |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0642 | >= 0.0353 | display |
| mean luma | PASS | 0.2133 | 0.1255..0.251 | display |
| banding[wall_panel] | PASS | 0.4540 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.68 (run 7 px), - / -, 3 strips |

### KS17_heart_reveal (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS17_heart_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.985 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.297 % of the frame, thin-line mask 0.985 % |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0577 | >= 0.0353 | display |
| mean luma | PASS | 0.1581 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.5770 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.87 (run 8 px), - / -, 6 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.75 (run 9 px) not gated (textured: gate steps), - / -, 22 strips |

### KS17_heart_reveal_1610 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS17_heart_reveal_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 1.085 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.615 % of the frame, thin-line mask 1.085 % |
| clipped highlights | PASS | 0.0002 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0580 | >= 0.0353 | display |
| mean luma | PASS | 0.1638 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.5470 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.82 (run 7 px), - / -, 5 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.34 (run 7 px) not gated (textured: gate steps), - / -, 19 strips |

### KS17_heart_reveal_219 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS17_heart_reveal_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 1.122 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.729 % of the frame, thin-line mask 1.122 % |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0569 | >= 0.0353 | display |
| mean luma | PASS | 0.1489 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.6690 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.00 (run 9 px), - / -, 8 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.66 (run 8 px) not gated (textured: gate steps), - / -, 30 strips |

### KS17_heart_reveal_4k (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS17_heart_reveal_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 3 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1866, 76], [1653, 653], [1881, 801]]; owner HT (beat 7) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.891 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.216 % of the frame, thin-line mask 0.891 % |
| clipped highlights | PASS | 0.0003 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0575 | >= 0.0353 | display |
| mean luma | PASS | 0.1571 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.5120 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.77 (run 10 px), - / -, 12 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.46 (run 9 px) not gated (textured: gate steps), - / -, 45 strips |

### KS18_koi_pass (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.998 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.242 % of the frame, thin-line mask 0.998 % |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0706 | >= 0.0353 | display |
| mean luma | PASS | 0.2088 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.4850 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.73 (run 8 px), - / -, 9 strips |

### KS18_koi_pass_1610 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS18_koi_pass_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 1.055 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.648 % of the frame, thin-line mask 1.055 % |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0701 | >= 0.0353 | display |
| mean luma | PASS | 0.2170 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.5060 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.76 (run 9 px), - / -, 7 strips |

### KS18_koi_pass_219 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS18_koi_pass_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 1.068 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 1.682 % of the frame, thin-line mask 1.068 % |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0692 | >= 0.0353 | display |
| mean luma | PASS | 0.1892 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.6270 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.94 (run 8 px), - / -, 12 strips |

### KS18_koi_pass_4k (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS18_koi_pass_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 2 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[2654, 1619], [2733, 1831]]; owner HT (beat 7) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.730 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.026 % of the frame, thin-line mask 0.730 % |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0723 | >= 0.0353 | display |
| mean luma | PASS | 0.2073 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.4570 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.69 (run 9 px), - / -, 18 strips |

### KS19_goal_window (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS19_goal_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 5 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[775, 558], [783, 575], [800, 613], [368, 863], [373, 898]]; owner HT (beat 7) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.597 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.557 % of the frame, thin-line mask 0.597 % |
| clipped highlights | PASS | 0.0037 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.1262 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.0730 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.11 (run 5 px), - / -, 2 strips |

### KS19_goal_window_1610 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS19_goal_window_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 9 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[774, 481], [643, 567], [858, 573], [822, 626], [668, 630]]; owner HT (beat 7) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.655 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.684 % of the frame, thin-line mask 0.655 % |
| clipped highlights | PASS | 0.0040 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.1258 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.2320 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.35 (run 7 px), - / -, 1 strips |

### KS19_goal_window_219 (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS19_goal_window_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 7 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1095, 558], [1103, 575], [1120, 613], [717, 815], [568, 823]]; owner HT (beat 7) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.464 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.418 % of the frame, thin-line mask 0.464 % |
| clipped highlights | PASS | 0.0025 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.1217 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.5010 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.75 (run 8 px), - / -, 2 strips |

### KS19_goal_window_4k (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\KS19_goal_window_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 60 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[1666, 1004], [1693, 1033], [1839, 1041], [1659, 1065], [1807, 1072]]; owner HT (beat 7) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 0.560 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 0.577 % of the frame, thin-line mask 0.560 % |
| clipped highlights | PASS | 0.0036 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0577 | >= 0.0353 | display |
| mean luma | PASS | 0.1267 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.1270 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.19 (run 7 px), - / -, 4 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
