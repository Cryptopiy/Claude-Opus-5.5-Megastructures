# Image metrics: seax-06s

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s`  
Scene constants: BulwarkConst.cs  
Images: 32, failing images: 7. Checks: 255 PASS, 7 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_WaveGallery_KS03_door_open | 1 | 1920x1080 | 0.255 | 0.072 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_1610 | 1 | 1680x1050 | 0.269 | 0.072 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_219 | 1 | 2560x1080 | 0.221 | 0.072 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_4k | 1 | 3840x2160 | 0.255 | 0.072 | 0.0000 | - | FAIL | mean luma |
| b02_Crest_KS04_crest_sea | 2 | 1920x1080 | 0.418 | 0.246 | 0.0000 | 0.500 | PASS |  |
| b02_Crest_KS04_crest_sea_1610 | 2 | 1680x1050 | 0.418 | 0.246 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea_219 | 2 | 2560x1080 | 0.417 | 0.246 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea_4k | 2 | 3840x2160 | 0.418 | 0.246 | 0.0000 | 0.333 | PASS |  |
| b02_Crest_KS05_crest_drop | 2 | 1920x1080 | 0.339 | 0.199 | 0.0000 | 0.500 | PASS |  |
| b02_Crest_KS05_crest_drop_1610 | 2 | 1680x1050 | 0.340 | 0.199 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_219 | 2 | 2560x1080 | 0.338 | 0.199 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_4k | 2 | 3840x2160 | 0.339 | 0.198 | 0.0000 | 0.333 | PASS |  |
| b02_Crest_KS06_crest_axis | 2 | 1920x1080 | 0.428 | 0.242 | 0.0000 | - | PASS |  |
| b02_Crest_KS06_crest_axis_1610 | 2 | 1680x1050 | 0.428 | 0.242 | 0.0000 | - | PASS |  |
| b02_Crest_KS06_crest_axis_219 | 2 | 2560x1080 | 0.428 | 0.245 | 0.0000 | - | PASS |  |
| b02_Crest_KS06_crest_axis_4k | 2 | 3840x2160 | 0.428 | 0.242 | 0.0000 | - | PASS |  |
| b02_Crest_end | 2 | 1920x1080 | 0.385 | 0.194 | 0.0000 | - | PASS |  |
| b02_Crest_look_back | 2 | 1920x1080 | 0.425 | 0.241 | 0.0000 | - | PASS |  |
| b02_Crest_look_down | 2 | 1920x1080 | 0.230 | 0.202 | 0.0000 | - | FAIL | mean luma |
| b02_Crest_look_l | 2 | 1920x1080 | 0.430 | 0.242 | 0.0000 | - | PASS |  |
| b02_Crest_look_r | 2 | 1920x1080 | 0.426 | 0.238 | 0.0000 | - | PASS |  |
| b02_Crest_look_up | 2 | 1920x1080 | 0.471 | 0.446 | 0.0000 | - | PASS |  |
| b02_Crest_mid | 2 | 1920x1080 | 0.422 | 0.241 | 0.0000 | - | PASS |  |
| b02_Crest_start | 2 | 1920x1080 | 0.428 | 0.245 | 0.0000 | - | PASS |  |
| pose_cartnear | 2 | 1920x1080 | 0.407 | 0.226 | 0.0000 | - | PASS |  |
| pose_joint | 2 | 1920x1080 | 0.273 | 0.206 | 0.0000 | - | FAIL | mean luma |
| pose_ks04near | 2 | 1920x1080 | 0.371 | 0.175 | 0.0000 | - | PASS |  |
| pose_lampnear | 2 | 1920x1080 | 0.465 | 0.292 | 0.0000 | - | PASS |  |
| pose_seaback | 2 | 1920x1080 | 0.416 | 0.199 | 0.0000 | - | PASS |  |
| pose_seadown | 2 | 1920x1080 | 0.345 | 0.172 | 0.0000 | - | PASS |  |
| pose_seafar | 2 | 1920x1080 | 0.442 | 0.195 | 0.0000 | - | PASS |  |
| pose_wallfoot | 2 | 1920x1080 | 0.260 | 0.164 | 0.0000 | - | FAIL | mean luma |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | - | INFO |
| KS02_gal_slit | - | INFO |
| KS03_door_open | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS04_crest_sea | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS05_crest_drop | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS06_crest_axis | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS07_spill_gates | - | INFO |
| KS08_spill_bow | - | INFO |
| KS09_spill_grate | - | INFO |
| KS10_reef_wide | - | INFO |
| KS11_reef_spray | - | INFO |
| KS12_tower_up | - | INFO |
| KS13_atrium | - | INFO |
| KS14_stairs_down | - | INFO |
| KS15_win_first | - | INFO |
| KS16_win_up | - | INFO |
| KS17_win_whale | - | INFO |
| KS18_win_down | - | INFO |
| KS19_sluice | - | INFO |
| KS20_ledge_world | - | INFO |
| KS21_beacon | - | INFO |

## Details

### b01_WaveGallery_KS03_door_open (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b01_WaveGallery_KS03_door_open.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2551 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b01_WaveGallery_KS03_door_open_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2693 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b01_WaveGallery_KS03_door_open_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2210 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b01_WaveGallery_KS03_door_open_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2551 | 0.27451..0.470588 | display |

### b02_Crest_KS04_crest_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS04_crest_sea.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4176 | 0.333333..0.588235 | display |
| horizon[sea_horizon] step | PASS | 0.0670 | <= 2 dE | trend-removed at row ~420.0, 998 cols: above [140.7, 152.2, 160.7] vs below [140.5, 152.3, 160.8] |
| banding[storm_sky] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.03 px (limit 10.3), 15 strips |

### b02_Crest_KS04_crest_sea_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS04_crest_sea_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4181 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS04_crest_sea_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4171 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS04_crest_sea_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4185 | 0.333333..0.588235 | display |
| horizon[sea_horizon] step | PASS | 0.1280 | <= 2 dE | trend-removed at row ~840.0, 1996 cols: above [140.6, 152.2, 160.6] vs below [141.0, 152.4, 160.8] |
| banding[storm_sky] | PASS | 0.3330 | score <= 1 | longest run 4 px (max 12), mean run 1.02 px (limit 19.7), 30 strips |

### b02_Crest_KS05_crest_drop (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS05_crest_drop.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3390 | 0.333333..0.588235 | display |
| banding[haze] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.01 px (limit 14.8), 15 strips |

### b02_Crest_KS05_crest_drop_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS05_crest_drop_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3400 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS05_crest_drop_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3379 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS05_crest_drop_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3390 | 0.333333..0.588235 | display |
| banding[haze] | PASS | 0.3330 | score <= 1 | longest run 4 px (max 12), mean run 1.02 px (limit 18.2), 30 strips |

### b02_Crest_KS06_crest_axis (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS06_crest_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4278 | 0.333333..0.588235 | display |

### b02_Crest_KS06_crest_axis_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS06_crest_axis_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4280 | 0.333333..0.588235 | display |

### b02_Crest_KS06_crest_axis_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS06_crest_axis_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4277 | 0.333333..0.588235 | display |

### b02_Crest_KS06_crest_axis_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_KS06_crest_axis_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4278 | 0.333333..0.588235 | display |

### b02_Crest_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3850 | 0.333333..0.588235 | display |

### b02_Crest_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4253 | 0.333333..0.588235 | display |

### b02_Crest_look_down (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2305 | 0.333333..0.588235 | display |

### b02_Crest_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4295 | 0.333333..0.588235 | display |

### b02_Crest_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4258 | 0.333333..0.588235 | display |

### b02_Crest_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4712 | 0.333333..0.588235 | display |

### b02_Crest_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4222 | 0.333333..0.588235 | display |

### b02_Crest_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\b02_Crest_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4281 | 0.333333..0.588235 | display |

### pose_cartnear (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\pose_cartnear.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4068 | 0.333333..0.588235 | display |

### pose_joint (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\pose_joint.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2726 | 0.333333..0.588235 | display |

### pose_ks04near (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\pose_ks04near.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3706 | 0.333333..0.588235 | display |

### pose_lampnear (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\pose_lampnear.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4653 | 0.333333..0.588235 | display |

### pose_seaback (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\pose_seaback.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4164 | 0.333333..0.588235 | display |

### pose_seadown (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\pose_seadown.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3451 | 0.333333..0.588235 | display |

### pose_seafar (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\pose_seafar.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4419 | 0.333333..0.588235 | display |

### pose_wallfoot (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\seax-06s\pose_wallfoot.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2596 | 0.333333..0.588235 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
