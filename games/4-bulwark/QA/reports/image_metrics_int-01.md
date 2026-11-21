# Image metrics: int-01

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01`  
Scene constants: BulwarkConst.cs (ERRORS: KeyShot KS03_door_open: could not convert string to float: 'L(KS03LumaMin)')  
Images: 80, failing images: 12. Checks: 623 PASS, 17 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_WaveGallery_KS01_gal_far | 1 | 1920x1080 | 0.200 | 0.130 | 0.0008 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_1610 | 1 | 1680x1050 | 0.203 | 0.130 | 0.0009 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_219 | 1 | 2560x1080 | 0.187 | 0.127 | 0.0006 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_4k | 1 | 3840x2160 | 0.200 | 0.130 | 0.0008 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit | 1 | 1920x1080 | 0.140 | 0.090 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_1610 | 1 | 1680x1050 | 0.142 | 0.090 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_219 | 1 | 2560x1080 | 0.139 | 0.095 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_4k | 1 | 3840x2160 | 0.140 | 0.090 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS03_door_open | 1 | 1920x1080 | 0.253 | 0.100 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_1610 | 1 | 1680x1050 | 0.269 | 0.109 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_219 | 1 | 2560x1080 | 0.216 | 0.096 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_4k | 1 | 3840x2160 | 0.252 | 0.100 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_end | 1 | 1920x1080 | 0.377 | 0.178 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_look_back | 1 | 1920x1080 | 0.156 | 0.104 | 0.0008 | - | PASS |  |
| b01_WaveGallery_look_down | 1 | 1920x1080 | 0.134 | 0.108 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_l | 1 | 1920x1080 | 0.181 | 0.127 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_r | 1 | 1920x1080 | 0.180 | 0.127 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_up | 1 | 1920x1080 | 0.235 | 0.107 | 0.0000 | - | PASS |  |
| b01_WaveGallery_mid | 1 | 1920x1080 | 0.154 | 0.104 | 0.0008 | - | PASS |  |
| b01_WaveGallery_start | 1 | 1920x1080 | 0.146 | 0.096 | 0.0008 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide | 4 | 1920x1080 | 0.411 | 0.130 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide_1610 | 4 | 1680x1050 | 0.410 | 0.130 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide_219 | 4 | 2560x1080 | 0.416 | 0.127 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide_4k | 4 | 3840x2160 | 0.411 | 0.130 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray | 4 | 1920x1080 | 0.512 | 0.134 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_1610 | 4 | 1680x1050 | 0.515 | 0.134 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_219 | 4 | 2560x1080 | 0.506 | 0.134 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_4k | 4 | 3840x2160 | 0.512 | 0.134 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up | 4 | 1920x1080 | 0.484 | 0.142 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_1610 | 4 | 1680x1050 | 0.485 | 0.145 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_219 | 4 | 2560x1080 | 0.486 | 0.141 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_4k | 4 | 3840x2160 | 0.484 | 0.142 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_end | 4 | 1920x1080 | 0.225 | 0.058 | 0.0000 | - | FAIL | mean luma |
| b04_ArmourSlope_look_back | 4 | 1920x1080 | 0.589 | 0.138 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_down | 4 | 1920x1080 | 0.642 | 0.481 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_l | 4 | 1920x1080 | 0.590 | 0.326 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_r | 4 | 1920x1080 | 0.401 | 0.131 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_up | 4 | 1920x1080 | 0.412 | 0.353 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_mid | 4 | 1920x1080 | 0.527 | 0.137 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_start | 4 | 1920x1080 | 0.435 | 0.180 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium | 5 | 1920x1080 | 0.096 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, fireflies, mean luma |
| b05_GateTower_KS13_atrium_1610 | 5 | 1680x1050 | 0.098 | 0.057 | 0.0000 | - | FAIL | mean luma |
| b05_GateTower_KS13_atrium_219 | 5 | 2560x1080 | 0.092 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, fireflies, mean luma |
| b05_GateTower_KS13_atrium_4k | 5 | 3840x2160 | 0.096 | 0.055 | 0.0000 | - | FAIL | fireflies, mean luma |
| b05_GateTower_KS14_stairs_down | 5 | 1920x1080 | 0.216 | 0.107 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_1610 | 5 | 1680x1050 | 0.222 | 0.108 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_219 | 5 | 2560x1080 | 0.200 | 0.107 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_4k | 5 | 3840x2160 | 0.216 | 0.107 | 0.0000 | - | PASS |  |
| b05_GateTower_end | 5 | 1920x1080 | 0.198 | 0.103 | 0.0000 | - | PASS |  |
| b05_GateTower_look_back | 5 | 1920x1080 | 0.191 | 0.124 | 0.0000 | - | PASS |  |
| b05_GateTower_look_down | 5 | 1920x1080 | 0.273 | 0.136 | 0.0000 | - | PASS |  |
| b05_GateTower_look_l | 5 | 1920x1080 | 0.288 | 0.152 | 0.0000 | - | PASS |  |
| b05_GateTower_look_r | 5 | 1920x1080 | 0.209 | 0.127 | 0.0000 | - | PASS |  |
| b05_GateTower_look_up | 5 | 1920x1080 | 0.317 | 0.148 | 0.0000 | - | FAIL | mean luma |
| b05_GateTower_mid | 5 | 1920x1080 | 0.183 | 0.120 | 0.0000 | - | PASS |  |
| b05_GateTower_start | 5 | 1920x1080 | 0.099 | 0.053 | 0.0000 | - | FAIL | mean luma |
| b06_WindowGallery_KS15_win_first | 6 | 1920x1080 | 0.275 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_1610 | 6 | 1680x1050 | 0.278 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_219 | 6 | 2560x1080 | 0.265 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_4k | 6 | 3840x2160 | 0.275 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up | 6 | 1920x1080 | 0.276 | 0.091 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_1610 | 6 | 1680x1050 | 0.290 | 0.091 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_219 | 6 | 2560x1080 | 0.242 | 0.088 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_4k | 6 | 3840x2160 | 0.276 | 0.091 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale | 6 | 1920x1080 | 0.187 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale_1610 | 6 | 1680x1050 | 0.191 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale_219 | 6 | 2560x1080 | 0.179 | 0.101 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale_4k | 6 | 3840x2160 | 0.187 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down | 6 | 1920x1080 | 0.153 | 0.097 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_1610 | 6 | 1680x1050 | 0.153 | 0.097 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_219 | 6 | 2560x1080 | 0.154 | 0.096 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_4k | 6 | 3840x2160 | 0.153 | 0.097 | 0.0000 | - | PASS |  |
| b06_WindowGallery_end | 6 | 1920x1080 | 0.162 | 0.110 | 0.0028 | - | PASS |  |
| b06_WindowGallery_look_back | 6 | 1920x1080 | 0.170 | 0.111 | 0.0001 | - | PASS |  |
| b06_WindowGallery_look_down | 6 | 1920x1080 | 0.140 | 0.106 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_l | 6 | 1920x1080 | 0.173 | 0.122 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_r | 6 | 1920x1080 | 0.148 | 0.103 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_up | 6 | 1920x1080 | 0.179 | 0.069 | 0.0000 | - | PASS |  |
| b06_WindowGallery_mid | 6 | 1920x1080 | 0.168 | 0.109 | 0.0000 | - | PASS |  |
| b06_WindowGallery_start | 6 | 1920x1080 | 0.215 | 0.116 | 0.0000 | - | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS02_gal_slit | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS04_crest_sea | - | INFO |
| KS05_crest_drop | - | INFO |
| KS06_crest_axis | - | INFO |
| KS07_spill_gates | - | INFO |
| KS08_spill_bow | - | INFO |
| KS09_spill_grate | - | INFO |
| KS10_reef_wide | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS11_reef_spray | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS12_tower_up | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS13_atrium | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS14_stairs_down | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS15_win_first | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS16_win_up | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS17_win_whale | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS18_win_down | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS19_sluice | - | INFO |
| KS20_ledge_world | - | INFO |
| KS21_beacon | - | INFO |

## Details

### b01_WaveGallery_KS01_gal_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS01_gal_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1995 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS01_gal_far_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0009 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2034 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS01_gal_far_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1873 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS01_gal_far_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1995 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS02_gal_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1404 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS02_gal_slit_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1415 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS02_gal_slit_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1391 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS02_gal_slit_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1404 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS03_door_open (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS03_door_open.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2525 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS03_door_open_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2686 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS03_door_open_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2156 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_KS03_door_open_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2525 | 0.27451..0.470588 | display |

### b01_WaveGallery_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3765 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1557 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1341 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1806 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1800 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2354 | 0.109804..0.27451 | display |

### b01_WaveGallery_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1541 | 0.109804..0.27451 | display |

### b01_WaveGallery_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b01_WaveGallery_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1457 | 0.109804..0.27451 | display |

### b04_ArmourSlope_KS10_reef_wide (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS10_reef_wide.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4111 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS10_reef_wide_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4096 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS10_reef_wide_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4164 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS10_reef_wide_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4105 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS11_reef_spray.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5121 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS11_reef_spray_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5150 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS11_reef_spray_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5056 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS11_reef_spray_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5119 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS12_tower_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4843 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS12_tower_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4848 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS12_tower_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4865 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_KS12_tower_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4843 | 0.392157..0.647059 | display |

### b04_ArmourSlope_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2253 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5892 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.6420 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5896 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4007 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4119 | 0.392157..0.647059 | display |

### b04_ArmourSlope_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5271 | 0.392157..0.647059 | display |

### b04_ArmourSlope_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b04_ArmourSlope_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4353 | 0.392157..0.647059 | display |

### b05_GateTower_KS13_atrium (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_KS13_atrium.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[318, 1054]] |
| fireflies | FAIL | 20 | 0 px | first at [[1057, 927], [1058, 927], [1059, 927], [1060, 927], [1061, 927]] |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.0960 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_KS13_atrium_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.0980 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_KS13_atrium_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[638, 1054]] |
| fireflies | FAIL | 22 | 0 px | first at [[1377, 927], [1378, 927], [1379, 927], [1380, 927], [1381, 927]] |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.0919 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_KS13_atrium_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 57 | 0 px | first at [[2098, 1855], [2099, 1855], [2100, 1855], [2101, 1855], [2102, 1855]] |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.0960 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_KS14_stairs_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2163 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_KS14_stairs_down_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2217 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_KS14_stairs_down_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2004 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_KS14_stairs_down_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2162 | 0.109804..0.313725 | display |

### b05_GateTower_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1980 | 0.109804..0.313725 | display |

### b05_GateTower_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1907 | 0.109804..0.313725 | display |

### b05_GateTower_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2728 | 0.109804..0.313725 | display |

### b05_GateTower_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2875 | 0.109804..0.313725 | display |

### b05_GateTower_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2086 | 0.109804..0.313725 | display |

### b05_GateTower_look_up (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3173 | 0.109804..0.313725 | display |

### b05_GateTower_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1826 | 0.109804..0.313725 | display |

### b05_GateTower_start (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b05_GateTower_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.0994 | 0.109804..0.313725 | display |

### b06_WindowGallery_KS15_win_first (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS15_win_first.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2752 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS15_win_first_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2781 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS15_win_first_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2647 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS15_win_first_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2752 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS16_win_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2760 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS16_win_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2896 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS16_win_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2421 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS16_win_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2760 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_whale (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS17_win_whale.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1870 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_whale_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS17_win_whale_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1905 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_whale_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS17_win_whale_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1787 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_whale_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS17_win_whale_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1870 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS18_win_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1532 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS18_win_down_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1529 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS18_win_down_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1535 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_KS18_win_down_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1531 | 0.054902..0.333333 | display |

### b06_WindowGallery_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0028 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1623 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1699 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1399 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1729 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1476 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1788 | 0.054902..0.333333 | display |

### b06_WindowGallery_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1681 | 0.054902..0.333333 | display |

### b06_WindowGallery_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-01\b06_WindowGallery_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2149 | 0.054902..0.333333 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
