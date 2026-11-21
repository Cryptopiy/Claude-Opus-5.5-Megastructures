# Image metrics: int-05

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05`  
Scene constants: BulwarkConst.cs  
Images: 63, failing images: 17. Checks: 487 PASS, 17 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_WaveGallery_KS01_gal_far | 1 | 1920x1080 | 0.161 | 0.084 | 0.0008 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_1610 | 1 | 1680x1050 | 0.164 | 0.084 | 0.0009 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_219 | 1 | 2560x1080 | 0.151 | 0.084 | 0.0006 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_4k | 1 | 3840x2160 | 0.161 | 0.084 | 0.0008 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit | 1 | 1920x1080 | 0.100 | 0.064 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS02_gal_slit_1610 | 1 | 1680x1050 | 0.100 | 0.064 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS02_gal_slit_219 | 1 | 2560x1080 | 0.100 | 0.068 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS02_gal_slit_4k | 1 | 3840x2160 | 0.100 | 0.064 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open | 1 | 1920x1080 | 0.254 | 0.072 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_1610 | 1 | 1680x1050 | 0.268 | 0.072 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_219 | 1 | 2560x1080 | 0.220 | 0.072 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_4k | 1 | 3840x2160 | 0.253 | 0.072 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_end | 1 | 1920x1080 | 0.370 | 0.178 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_look_back | 1 | 1920x1080 | 0.110 | 0.072 | 0.0008 | - | PASS |  |
| b01_WaveGallery_look_down | 1 | 1920x1080 | 0.084 | 0.059 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_look_l | 1 | 1920x1080 | 0.152 | 0.097 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_r | 1 | 1920x1080 | 0.151 | 0.097 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_up | 1 | 1920x1080 | 0.223 | 0.089 | 0.0000 | - | PASS |  |
| b01_WaveGallery_mid | 1 | 1920x1080 | 0.109 | 0.072 | 0.0008 | - | FAIL | mean luma |
| b01_WaveGallery_start | 1 | 1920x1080 | 0.097 | 0.060 | 0.0008 | - | FAIL | mean luma |
| b05_GateTower_KS13_atrium | 5 | 1920x1080 | 0.105 | 0.063 | 0.0000 | - | FAIL | mean luma |
| b05_GateTower_KS13_atrium_1610 | 5 | 1680x1050 | 0.105 | 0.063 | 0.0000 | - | FAIL | mean luma |
| b05_GateTower_KS13_atrium_219 | 5 | 2560x1080 | 0.107 | 0.063 | 0.0000 | - | FAIL | mean luma |
| b05_GateTower_KS13_atrium_4k | 5 | 3840x2160 | 0.105 | 0.062 | 0.0000 | - | FAIL | mean luma |
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
| b05_GateTower_start | 5 | 1920x1080 | 0.131 | 0.063 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first | 6 | 1920x1080 | 0.275 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_1610 | 6 | 1680x1050 | 0.278 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_219 | 6 | 2560x1080 | 0.265 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_4k | 6 | 3840x2160 | 0.275 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up | 6 | 1920x1080 | 0.276 | 0.087 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_1610 | 6 | 1680x1050 | 0.289 | 0.091 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_219 | 6 | 2560x1080 | 0.242 | 0.087 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_4k | 6 | 3840x2160 | 0.276 | 0.087 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale | 6 | 1920x1080 | 0.187 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale_1610 | 6 | 1680x1050 | 0.191 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale_219 | 6 | 2560x1080 | 0.179 | 0.101 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale_4k | 6 | 3840x2160 | 0.187 | 0.100 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down | 6 | 1920x1080 | 0.147 | 0.090 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_1610 | 6 | 1680x1050 | 0.147 | 0.090 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_219 | 6 | 2560x1080 | 0.147 | 0.090 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_4k | 6 | 3840x2160 | 0.147 | 0.090 | 0.0000 | - | PASS |  |
| b06_WindowGallery_end | 6 | 1920x1080 | 0.159 | 0.091 | 0.0028 | - | PASS |  |
| b06_WindowGallery_look_back | 6 | 1920x1080 | 0.170 | 0.110 | 0.0001 | - | PASS |  |
| b06_WindowGallery_look_down | 6 | 1920x1080 | 0.140 | 0.106 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_l | 6 | 1920x1080 | 0.173 | 0.122 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_r | 6 | 1920x1080 | 0.148 | 0.103 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_up | 6 | 1920x1080 | 0.181 | 0.092 | 0.0000 | - | PASS |  |
| b06_WindowGallery_mid | 6 | 1920x1080 | 0.168 | 0.109 | 0.0000 | - | PASS |  |
| b06_WindowGallery_start | 6 | 1920x1080 | 0.215 | 0.116 | 0.0000 | - | PASS |  |
| pose_atrium_back_s940 | 5 | 1920x1080 | 0.200 | 0.092 | 0.0011 | - | PASS |  |
| pose_door_in_s108 | 1 | 1920x1080 | 0.123 | 0.076 | 0.0000 | - | PASS |  |
| pose_door_wall_s114 | 1 | 1920x1080 | 0.147 | 0.063 | 0.0000 | - | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS02_gal_slit | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS03_door_open | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS04_crest_sea | - | INFO |
| KS05_crest_drop | - | INFO |
| KS06_crest_axis | - | INFO |
| KS07_spill_gates | - | INFO |
| KS08_spill_bow | - | INFO |
| KS09_spill_grate | - | INFO |
| KS10_reef_wide | - | INFO |
| KS11_reef_spray | - | INFO |
| KS12_tower_up | - | INFO |
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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS01_gal_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1610 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS01_gal_far_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0009 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1640 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS01_gal_far_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1508 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS01_gal_far_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1610 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS02_gal_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.0998 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS02_gal_slit_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1005 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS02_gal_slit_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1002 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS02_gal_slit_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.0998 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS03_door_open (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS03_door_open.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2535 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS03_door_open_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2675 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS03_door_open_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2197 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_KS03_door_open_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2535 | 0.27451..0.470588 | display |

### b01_WaveGallery_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3698 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1104 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_down (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.0839 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1519 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1514 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2226 | 0.109804..0.27451 | display |

### b01_WaveGallery_mid (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1092 | 0.109804..0.27451 | display |

### b01_WaveGallery_start (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b01_WaveGallery_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.0966 | 0.109804..0.27451 | display |

### b05_GateTower_KS13_atrium (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_KS13_atrium.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1049 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_KS13_atrium_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1049 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_KS13_atrium_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1066 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_KS13_atrium_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1049 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_KS14_stairs_down.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_KS14_stairs_down_1610.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_KS14_stairs_down_219.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_KS14_stairs_down_4k.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1976 | 0.109804..0.313725 | display |

### b05_GateTower_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_look_back.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_look_down.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_look_l.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_look_r.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_look_up.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_mid.png`

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

### b05_GateTower_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b05_GateTower_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1309 | 0.109804..0.313725 | display |

### b06_WindowGallery_KS15_win_first (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS15_win_first.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS15_win_first_1610.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS15_win_first_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2646 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS15_win_first_4k.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS16_win_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2757 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS16_win_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2894 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS16_win_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2418 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS16_win_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2757 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_whale (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS17_win_whale.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS17_win_whale_1610.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS17_win_whale_219.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS17_win_whale_4k.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS18_win_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1467 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS18_win_down_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1468 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS18_win_down_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1472 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_KS18_win_down_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1467 | 0.054902..0.333333 | display |

### b06_WindowGallery_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0028 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1594 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1697 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_look_down.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_look_l.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_look_r.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1811 | 0.054902..0.333333 | display |

### b06_WindowGallery_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1678 | 0.054902..0.333333 | display |

### b06_WindowGallery_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\b06_WindowGallery_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2145 | 0.054902..0.333333 | display |

### pose_atrium_back_s940 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\pose_atrium_back_s940.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0011 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2004 | 0.109804..0.313725 | display |

### pose_door_in_s108 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\pose_door_in_s108.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1225 | 0.109804..0.27451 | display |

### pose_door_wall_s114 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-05\pose_door_wall_s114.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1467 | 0.109804..0.27451 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
