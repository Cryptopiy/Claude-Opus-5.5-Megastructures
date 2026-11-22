# Image metrics: p4_01b

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b`  
Scene constants: NocturneConstants.cs  
Images: 51, failing images: 37. Checks: 356 PASS, 52 FAIL, 0 WARN, 14 SKIP, 51 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| beat01_alley_end | 1 | 1920x1080 | 0.105 | 0.059 | 0.0000 | - | PASS |  |
| beat01_alley_look_back | 1 | 1920x1080 | 0.107 | 0.059 | 0.0035 | - | FAIL | NaN-like pixels |
| beat01_alley_look_l | 1 | 1920x1080 | 0.078 | 0.058 | 0.0000 | - | PASS |  |
| beat01_alley_look_r | 1 | 1920x1080 | 0.130 | 0.059 | 0.0000 | - | PASS |  |
| beat01_alley_look_up60 | 1 | 1920x1080 | 0.107 | 0.066 | 0.0000 | - | FAIL | NaN-like pixels |
| beat01_alley_mid | 1 | 1920x1080 | 0.155 | 0.066 | 0.0017 | - | FAIL | NaN-like pixels |
| beat01_alley_start | 1 | 1920x1080 | 0.140 | 0.067 | 0.0026 | - | FAIL | NaN-like pixels |
| beat02_boulevard_end | 2 | 1920x1080 | 0.127 | 0.074 | 0.0067 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_look_back | 2 | 1920x1080 | 0.131 | 0.061 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_look_l | 2 | 1920x1080 | 0.128 | 0.060 | 0.0000 | - | FAIL | NaN-like pixels |
| beat02_boulevard_look_r | 2 | 1920x1080 | 0.123 | 0.056 | 0.0000 | - | FAIL | NaN-like pixels |
| beat02_boulevard_look_up60 | 2 | 1920x1080 | 0.179 | 0.066 | 0.0000 | - | PASS |  |
| beat02_boulevard_mid | 2 | 1920x1080 | 0.135 | 0.061 | 0.0047 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_start | 2 | 1920x1080 | 0.108 | 0.059 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat03_edge_end | 3 | 1920x1080 | 0.110 | 0.067 | 0.0135 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat03_edge_look_back | 3 | 1920x1080 | 0.125 | 0.060 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat03_edge_look_down60 | 3 | 1920x1080 | 0.111 | 0.055 | 0.0000 | - | PASS |  |
| beat03_edge_look_l | 3 | 1920x1080 | 0.141 | 0.061 | 0.0019 | - | FAIL | NaN-like pixels |
| beat03_edge_look_r | 3 | 1920x1080 | 0.136 | 0.060 | 0.0021 | - | FAIL | NaN-like pixels |
| beat03_edge_look_up60 | 3 | 1920x1080 | 0.193 | 0.082 | 0.0124 | - | FAIL | NaN-like pixels |
| beat03_edge_mid | 3 | 1920x1080 | 0.124 | 0.070 | 0.0063 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat03_edge_start | 3 | 1920x1080 | 0.125 | 0.074 | 0.0052 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat04_skybridge_end | 4 | 1920x1080 | 0.095 | 0.057 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat04_skybridge_look_back | 4 | 1920x1080 | 0.112 | 0.061 | 0.0009 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat04_skybridge_look_down60 | 4 | 1920x1080 | 0.122 | 0.067 | 0.0002 | - | PASS |  |
| beat04_skybridge_look_l | 4 | 1920x1080 | 0.137 | 0.068 | 0.0040 | - | FAIL | NaN-like pixels |
| beat04_skybridge_look_r | 4 | 1920x1080 | 0.139 | 0.066 | 0.0048 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat04_skybridge_look_up60 | 4 | 1920x1080 | 0.162 | 0.067 | 0.0119 | - | PASS |  |
| beat04_skybridge_mid | 4 | 1920x1080 | 0.112 | 0.063 | 0.0015 | - | FAIL | NaN-like pixels |
| beat04_skybridge_start | 4 | 1920x1080 | 0.120 | 0.069 | 0.0150 | - | PASS |  |
| beat05_garden_end | 5 | 1920x1080 | 0.086 | 0.052 | 0.0000 | - | FAIL | NaN-like pixels |
| beat05_garden_look_back | 5 | 1920x1080 | 0.107 | 0.056 | 0.0049 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat05_garden_look_l | 5 | 1920x1080 | 0.086 | 0.054 | 0.0000 | - | FAIL | NaN-like pixels |
| beat05_garden_look_r | 5 | 1920x1080 | 0.087 | 0.052 | 0.0000 | - | FAIL | NaN-like pixels |
| beat05_garden_look_up60 | 5 | 1920x1080 | 0.135 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat05_garden_mid | 5 | 1920x1080 | 0.083 | 0.053 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat05_garden_start | 5 | 1920x1080 | 0.092 | 0.055 | 0.0003 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat06_signs_end | 6 | 1920x1080 | 0.117 | 0.064 | 0.0000 | - | FAIL | NaN-like pixels |
| beat06_signs_look_back | 6 | 1920x1080 | 0.112 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat06_signs_look_l | 6 | 1920x1080 | 0.179 | 0.057 | 0.0000 | - | FAIL | NaN-like pixels |
| beat06_signs_look_r | 6 | 1920x1080 | 0.130 | 0.063 | 0.0000 | - | PASS |  |
| beat06_signs_look_up60 | 6 | 1920x1080 | 0.208 | 0.063 | 0.0000 | - | PASS |  |
| beat06_signs_mid | 6 | 1920x1080 | 0.151 | 0.062 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat06_signs_start | 6 | 1920x1080 | 0.091 | 0.053 | 0.0000 | - | FAIL | NaN-like pixels |
| beat07_heart_end | 7 | 1920x1080 | 0.107 | 0.059 | 0.0000 | - | FAIL | NaN-like pixels |
| beat07_heart_look_back | 7 | 1920x1080 | 0.159 | 0.062 | 0.0001 | - | FAIL | NaN-like pixels |
| beat07_heart_look_l | 7 | 1920x1080 | 0.128 | 0.066 | 0.0009 | - | PASS |  |
| beat07_heart_look_r | 7 | 1920x1080 | 0.126 | 0.063 | 0.0010 | - | FAIL | NaN-like pixels |
| beat07_heart_look_up60 | 7 | 1920x1080 | 0.262 | 0.081 | 0.0000 | - | PASS |  |
| beat07_heart_mid | 7 | 1920x1080 | 0.133 | 0.071 | 0.0000 | - | PASS |  |
| beat07_heart_start | 7 | 1920x1080 | 0.127 | 0.066 | 0.0001 | - | PASS |  |

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
| KS11_bridge_train | - | INFO |
| KS12_bridge_void | - | INFO |
| KS13_bridge_glass | - | INFO |
| KS14_garden_gate | - | INFO |
| KS15_garden_canopy | - | INFO |
| KS16_signs_wall | - | INFO |
| KS17_heart_reveal | - | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### beat01_alley_end (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat01_alley_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.867 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0588 | >= 0.0353 | display |
| mean luma | INFO | 0.1049 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 122.0 codes): objects on the horizon |

### beat01_alley_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat01_alley_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 0 (mode all), dark 6, first at [[924, 453], [929, 456], [924, 458], [929, 459], [929, 469]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.083 % of the frame |
| clipped highlights | PASS | 0.0035 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | INFO | 0.1072 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 79.0 codes): objects on the horizon |

### beat01_alley_look_l (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat01_alley_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.033 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0577 | >= 0.0353 | display |
| mean luma | INFO | 0.0778 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 5.0 codes): objects on the horizon |

### beat01_alley_look_r (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat01_alley_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.118 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | INFO | 0.1299 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 18.0 codes): objects on the horizon |

### beat01_alley_look_up60 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat01_alley_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[1454, 64], [1163, 504]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.147 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0664 | >= 0.0353 | display |
| mean luma | INFO | 0.1070 |  | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |

### beat01_alley_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat01_alley_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 0 (mode all), dark 6, first at [[1690, 225], [1547, 278], [1518, 294], [1663, 316], [1556, 350]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.120 % of the frame |
| clipped highlights | PASS | 0.0017 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0656 | >= 0.0353 | display |
| mean luma | INFO | 0.1553 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 65.5 codes): objects on the horizon |

### beat01_alley_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat01_alley_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 0 (mode all), dark 4, first at [[374, 338], [383, 343], [319, 376], [936, 497]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.188 % of the frame |
| clipped highlights | PASS | 0.0026 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0669 | >= 0.0353 | display |
| mean luma | INFO | 0.1395 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 57.5 codes): objects on the horizon |

### beat02_boulevard_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat02_boulevard_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[1809, 126], [863, 186], [864, 228], [881, 229], [1881, 232]] |
| isolated pixels > 8x median | FAIL | 7 | 0 outside neon masks | first at [[865, 193], [865, 194], [881, 229], [456, 246], [457, 246]]; neon mask 3.994 % of the frame |
| clipped highlights | PASS | 0.0067 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0742 | >= 0.0353 | display |
| mean luma | INFO | 0.1265 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 72.0 codes): objects on the horizon |

### beat02_boulevard_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat02_boulevard_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 4 (mode all), dark 1, first at [[1027, 112], [1049, 211], [1058, 219], [1461, 520], [362, 533]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[238, 549]]; neon mask 0.586 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0611 | >= 0.0353 | display |
| mean luma | INFO | 0.1312 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 32.0 codes): objects on the horizon |

### beat02_boulevard_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat02_boulevard_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[1756, 385], [1478, 530]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.311 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0597 | >= 0.0353 | display |
| mean luma | INFO | 0.1277 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 127.0 codes): objects on the horizon |

### beat02_boulevard_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat02_boulevard_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 0 (mode all), dark 4, first at [[163, 412], [600, 530], [820, 530], [946, 530]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.320 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0560 | >= 0.0353 | display |
| mean luma | INFO | 0.1231 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 152.5 codes): objects on the horizon |

### beat02_boulevard_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat02_boulevard_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.006 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0658 | >= 0.0353 | display |
| mean luma | INFO | 0.1787 |  | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |

### beat02_boulevard_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat02_boulevard_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 3 (mode all), dark 1, first at [[893, 117], [893, 276], [1560, 533], [955, 534]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[866, 197], [1781, 523]]; neon mask 2.487 % of the frame |
| clipped highlights | PASS | 0.0047 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0614 | >= 0.0353 | display |
| mean luma | INFO | 0.1351 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 45.0 codes): objects on the horizon |

### beat02_boulevard_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat02_boulevard_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[391, 535], [378, 547]]; neon mask 1.040 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | INFO | 0.1078 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 125.0 codes): objects on the horizon |

### beat03_edge_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat03_edge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[891, 105], [871, 251], [511, 407]] |
| isolated pixels > 8x median | FAIL | 5 | 0 outside neon masks | first at [[881, 228], [881, 229], [865, 234], [866, 234], [871, 251]]; neon mask 0.717 % of the frame |
| clipped highlights | PASS | 0.0135 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0672 | >= 0.0353 | display |
| mean luma | INFO | 0.1096 |  | display |

### beat03_edge_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat03_edge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 2 (mode all), dark 1, first at [[1045, 226], [763, 526], [351, 533]] |
| isolated pixels > 8x median | FAIL | 7 | 0 outside neon masks | first at [[581, 297], [606, 314], [607, 314], [630, 517], [668, 519]]; neon mask 1.162 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0600 | >= 0.0353 | display |
| mean luma | INFO | 0.1252 |  | display |

### beat03_edge_look_down60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat03_edge_look_down60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.000 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0555 | >= 0.0353 | display |
| mean luma | INFO | 0.1110 |  | display |

### beat03_edge_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat03_edge_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 5 (mode all), dark 1, first at [[1133, 752], [1254, 752], [1345, 752], [1375, 752], [1466, 752]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 3.403 % of the frame |
| clipped highlights | PASS | 0.0019 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0614 | >= 0.0353 | display |
| mean luma | INFO | 0.1410 |  | display |

### beat03_edge_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat03_edge_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[305, 260], [303, 286]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 3.512 % of the frame |
| clipped highlights | PASS | 0.0021 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0600 | >= 0.0353 | display |
| mean luma | INFO | 0.1362 |  | display |

### beat03_edge_look_up60 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat03_edge_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1350, 938]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.989 % of the frame |
| clipped highlights | PASS | 0.0124 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0815 | >= 0.0353 | display |
| mean luma | INFO | 0.1928 |  | display |

### beat03_edge_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat03_edge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[854, 196], [921, 562]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[395, 221], [396, 221], [873, 224], [880, 226]]; neon mask 3.683 % of the frame |
| clipped highlights | PASS | 0.0063 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0703 | >= 0.0353 | display |
| mean luma | INFO | 0.1239 |  | display |

### beat03_edge_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat03_edge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[873, 258], [970, 549], [980, 550], [937, 551], [931, 554]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[875, 208], [876, 208], [875, 266], [882, 287]]; neon mask 3.947 % of the frame |
| clipped highlights | PASS | 0.0052 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0742 | >= 0.0353 | display |
| mean luma | INFO | 0.1253 |  | display |

### beat04_skybridge_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat04_skybridge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 16 | 0 | bright 16 (mode all), dark 0, first at [[623, 69], [1475, 170], [1417, 171], [1404, 173], [1074, 181]] |
| isolated pixels > 8x median | FAIL | 10 | 0 outside neon masks | first at [[408, 70], [408, 71], [465, 176], [1223, 247], [1291, 267]]; neon mask 3.308 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0566 | >= 0.0353 | display |
| mean luma | INFO | 0.0954 |  | display |

### beat04_skybridge_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat04_skybridge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[1058, 178], [750, 258], [771, 258], [856, 258], [1132, 258]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[771, 258], [856, 258], [837, 263], [1796, 662]]; neon mask 1.910 % of the frame |
| clipped highlights | PASS | 0.0009 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0614 | >= 0.0353 | display |
| mean luma | INFO | 0.1121 |  | display |

### beat04_skybridge_look_down60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat04_skybridge_look_down60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.168 % of the frame |
| clipped highlights | PASS | 0.0002 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | INFO | 0.1216 |  | display |

### beat04_skybridge_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat04_skybridge_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[1230, 296], [1229, 299], [1229, 337], [1229, 352], [1234, 444]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 3.550 % of the frame |
| clipped highlights | PASS | 0.0040 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0675 | >= 0.0353 | display |
| mean luma | INFO | 0.1367 |  | display |

### beat04_skybridge_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat04_skybridge_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[919, 388], [611, 450], [737, 450], [482, 531]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1761, 617]]; neon mask 3.101 % of the frame |
| clipped highlights | PASS | 0.0048 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0656 | >= 0.0353 | display |
| mean luma | INFO | 0.1394 |  | display |

### beat04_skybridge_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat04_skybridge_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.257 % of the frame |
| clipped highlights | PASS | 0.0119 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | INFO | 0.1616 |  | display |

### beat04_skybridge_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat04_skybridge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 4 (mode all), dark 2, first at [[646, 294], [585, 315], [1115, 376], [1116, 379], [746, 388]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 3.164 % of the frame |
| clipped highlights | PASS | 0.0015 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0628 | >= 0.0353 | display |
| mean luma | INFO | 0.1116 |  | display |

### beat04_skybridge_start (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat04_skybridge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.604 % of the frame |
| clipped highlights | PASS | 0.0150 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0692 | >= 0.0353 | display |
| mean luma | INFO | 0.1202 |  | display |

### beat05_garden_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat05_garden_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[447, 473], [299, 493]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.073 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0521 | >= 0.0353 | display |
| mean luma | INFO | 0.0864 |  | display |

### beat05_garden_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat05_garden_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 21 | 0 | bright 21 (mode all), dark 0, first at [[379, 10], [1665, 23], [368, 87], [1588, 108], [378, 143]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1498, 512], [1499, 512], [525, 533]]; neon mask 3.171 % of the frame |
| clipped highlights | PASS | 0.0049 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0558 | >= 0.0353 | display |
| mean luma | INFO | 0.1073 |  | display |

### beat05_garden_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat05_garden_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[47, 285], [627, 400]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.130 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0541 | >= 0.0353 | display |
| mean luma | INFO | 0.0860 |  | display |

### beat05_garden_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat05_garden_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[1048, 12], [1053, 437], [1080, 437], [1297, 437], [1324, 437]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.938 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0518 | >= 0.0353 | display |
| mean luma | INFO | 0.0866 |  | display |

### beat05_garden_look_up60 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat05_garden_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 22 | 0 | bright 18 (mode all), dark 4, first at [[1503, 33], [443, 48], [435, 220], [3, 234], [1833, 332]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[291, 836]]; neon mask 2.480 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0549 | >= 0.0353 | display |
| mean luma | INFO | 0.1354 |  | display |

### beat05_garden_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat05_garden_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1411, 164]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1251, 105], [639, 546]]; neon mask 0.778 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0530 | >= 0.0353 | display |
| mean luma | INFO | 0.0832 |  | display |

### beat05_garden_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat05_garden_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[1064, 168], [505, 179], [1211, 246], [1305, 260], [1138, 268]] |
| isolated pixels > 8x median | FAIL | 6 | 0 outside neon masks | first at [[186, 54], [1304, 266], [611, 287], [727, 315], [727, 316]]; neon mask 3.132 % of the frame |
| clipped highlights | PASS | 0.0003 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0549 | >= 0.0353 | display |
| mean luma | INFO | 0.0921 |  | display |

### beat06_signs_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat06_signs_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[336, 447]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.738 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0639 | >= 0.0353 | display |
| mean luma | INFO | 0.1175 |  | display |

### beat06_signs_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat06_signs_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[931, 235], [1226, 246], [1318, 269], [1011, 274], [1021, 290]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1011, 274]]; neon mask 2.258 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0549 | >= 0.0353 | display |
| mean luma | INFO | 0.1120 |  | display |

### beat06_signs_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat06_signs_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[334, 222]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.096 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0572 | >= 0.0353 | display |
| mean luma | INFO | 0.1789 |  | display |

### beat06_signs_look_r (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat06_signs_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.057 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | INFO | 0.1297 |  | display |

### beat06_signs_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat06_signs_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.093 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0630 | >= 0.0353 | display |
| mean luma | INFO | 0.2083 |  | display |

### beat06_signs_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat06_signs_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[379, 462]]; neon mask 0.256 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0625 | >= 0.0353 | display |
| mean luma | INFO | 0.1515 |  | display |

### beat06_signs_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat06_signs_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[615, 466], [461, 493], [424, 499], [281, 524]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.154 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0533 | >= 0.0353 | display |
| mean luma | INFO | 0.0906 |  | display |

### beat07_heart_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat07_heart_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[610, 438]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.303 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | INFO | 0.1068 |  | display |

### beat07_heart_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat07_heart_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[754, 445], [1363, 509], [1881, 509], [769, 545]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.180 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | INFO | 0.1587 |  | display |

### beat07_heart_look_l (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat07_heart_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.204 % of the frame |
| clipped highlights | PASS | 0.0009 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0661 | >= 0.0353 | display |
| mean luma | INFO | 0.1280 |  | display |

### beat07_heart_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat07_heart_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1219, 508]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.250 % of the frame |
| clipped highlights | PASS | 0.0010 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | INFO | 0.1261 |  | display |

### beat07_heart_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat07_heart_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.382 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0813 | >= 0.0353 | display |
| mean luma | INFO | 0.2623 |  | display |

### beat07_heart_mid (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat07_heart_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.267 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0709 | >= 0.0353 | display |
| mean luma | INFO | 0.1335 |  | display |

### beat07_heart_start (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01b\beat07_heart_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.189 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0656 | >= 0.0353 | display |
| mean luma | INFO | 0.1267 |  | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
