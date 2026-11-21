# Image metrics: dusk-05

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05`  
Scene constants: BulwarkConst.cs (ERRORS: KeyShot KS03_door_open: could not convert string to float: 'L(KS03LumaMin)')  
Images: 36, failing images: 2. Checks: 289 PASS, 2 FAIL, 0 WARN, 0 SKIP, 3 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b02_Crest_KS05_crest_drop | 2 | 1920x1080 | 0.355 | 0.199 | 0.0000 | 0.500 | PASS |  |
| b02_Crest_KS05_crest_drop_1610 | 2 | 1680x1050 | 0.356 | 0.199 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_219 | 2 | 2560x1080 | 0.352 | 0.199 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_4k | 2 | 3840x2160 | 0.355 | 0.198 | 0.0000 | 0.250 | PASS |  |
| b07_SluiceLedge_KS19_sluice | 7 | 1920x1080 | 0.190 | 0.069 | 0.0004 | - | PASS |  |
| b07_SluiceLedge_KS19_sluice_1610 | 7 | 1680x1050 | 0.190 | 0.069 | 0.0004 | - | PASS |  |
| b07_SluiceLedge_KS19_sluice_219 | 7 | 2560x1080 | 0.204 | 0.072 | 0.0003 | - | PASS |  |
| b07_SluiceLedge_KS19_sluice_4k | 7 | 3840x2160 | 0.190 | 0.069 | 0.0006 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world | 7 | 1920x1080 | 0.285 | 0.074 | 0.0000 | 0.333 | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_1610 | 7 | 1680x1050 | 0.285 | 0.074 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_219 | 7 | 2560x1080 | 0.282 | 0.074 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_4k | 7 | 3840x2160 | 0.284 | 0.074 | 0.0000 | 0.250 | PASS |  |
| b07_SluiceLedge_KS21_beacon | 7 | 1920x1080 | 0.281 | 0.075 | 0.0145 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_1610 | 7 | 1680x1050 | 0.279 | 0.078 | 0.0154 | - | FAIL | fireflies |
| b07_SluiceLedge_KS21_beacon_219 | 7 | 2560x1080 | 0.289 | 0.076 | 0.0109 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_4k | 7 | 3840x2160 | 0.281 | 0.074 | 0.0137 | - | PASS |  |
| b07_SluiceLedge_end | 7 | 1920x1080 | 0.305 | 0.091 | 0.1356 | - | PASS |  |
| b07_SluiceLedge_look_back | 7 | 1920x1080 | 0.243 | 0.083 | 0.0001 | - | PASS |  |
| b07_SluiceLedge_look_down | 7 | 1920x1080 | 0.083 | 0.070 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_look_l | 7 | 1920x1080 | 0.347 | 0.074 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_look_r | 7 | 1920x1080 | 0.213 | 0.070 | 0.0000 | - | FAIL | fireflies |
| b07_SluiceLedge_look_up | 7 | 1920x1080 | 0.260 | 0.163 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_mid | 7 | 1920x1080 | 0.290 | 0.083 | 0.0003 | - | PASS |  |
| b07_SluiceLedge_start | 7 | 1920x1080 | 0.166 | 0.091 | 0.0008 | - | PASS |  |
| pose_conddown | 7 | 1920x1080 | 0.151 | 0.060 | 0.0000 | - | PASS |  |
| pose_faceup | 7 | 1920x1080 | 0.312 | 0.206 | 0.0000 | - | PASS |  |
| pose_ks20s1470 | 7 | 1920x1080 | 0.276 | 0.069 | 0.0000 | - | PASS |  |
| pose_ledgedown | 7 | 1920x1080 | 0.103 | 0.052 | 0.0000 | - | PASS |  |
| pose_mouthback | 7 | 1920x1080 | 0.228 | 0.084 | 0.0000 | - | PASS |  |
| pose_nadir1470 | 7 | 1920x1080 | 0.080 | 0.052 | 0.0000 | - | PASS |  |
| pose_north1470 | 7 | 1920x1080 | 0.261 | 0.070 | 0.0021 | - | PASS |  |
| pose_northdown | 7 | 1920x1080 | 0.160 | 0.072 | 0.0000 | - | PASS |  |
| pose_shaft | 7 | 1920x1080 | 0.276 | 0.075 | 0.0000 | - | PASS |  |
| pose_south | 7 | 1920x1080 | 0.251 | 0.080 | 0.0000 | - | PASS |  |
| pose_tun1405 | 7 | 1920x1080 | 0.160 | 0.065 | 0.0041 | - | PASS |  |
| pose_tunback1395 | 7 | 1920x1080 | 0.153 | 0.072 | 0.0045 | - | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | - | INFO |
| KS02_gal_slit | - | INFO |
| KS04_crest_sea | - | INFO |
| KS05_crest_drop | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS06_crest_axis | - | INFO |
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
| KS19_sluice | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS20_ledge_world | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS21_beacon | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |

## Details

### b02_Crest_KS05_crest_drop (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b02_Crest_KS05_crest_drop.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3546 | 0.333333..0.588235 | display |
| banding[haze] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.01 px (limit 24.9), 15 strips |

### b02_Crest_KS05_crest_drop_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b02_Crest_KS05_crest_drop_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3558 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b02_Crest_KS05_crest_drop_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3525 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b02_Crest_KS05_crest_drop_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3546 | 0.333333..0.588235 | display |
| banding[haze] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.03 px (limit 26.4), 30 strips |

### b07_SluiceLedge_KS19_sluice (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS19_sluice.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0004 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1898 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS19_sluice_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0004 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1896 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS19_sluice_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2041 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS19_sluice_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1895 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS20_ledge_world.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 6014 px excluded |
| mean luma | PASS | 0.2845 | 0.117647..0.352941 | display |
| horizon[lowland_horizon] step | PASS | 0.2450 | <= 2 dE | trend-removed at row ~474.0, 1094 cols: above [207.0, 141.7, 110.6] vs below [207.8, 142.2, 111.1] |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.02 px (limit 7.1), 10 strips |

### b07_SluiceLedge_KS20_ledge_world_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS20_ledge_world_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 5688 px excluded |
| mean luma | PASS | 0.2854 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS20_ledge_world_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 6014 px excluded |
| mean luma | PASS | 0.2822 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS20_ledge_world_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 24075 px excluded |
| mean luma | PASS | 0.2843 | 0.117647..0.352941 | display |
| horizon[lowland_horizon] step | PASS | 0.2900 | <= 2 dE | trend-removed at row ~948.0, 2188 cols: above [207.0, 141.6, 110.8] vs below [207.9, 142.2, 111.2] |
| banding[dusk_sky] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.02 px (limit 6.9), 20 strips |

### b07_SluiceLedge_KS21_beacon (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS21_beacon.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0145 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2812 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS21_beacon_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 5 | 0 px | first at [[838, 626], [839, 626], [840, 626], [841, 626], [842, 626]] |
| clipped highlights | PASS | 0.0154 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2792 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS21_beacon_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0109 | <= 1 % | any channel >= 254; 20764 px excluded |
| mean luma | PASS | 0.2890 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_KS21_beacon_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0137 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2807 | 0.078431..0.352941 | display |

### b07_SluiceLedge_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.1356 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3049 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | INFO | 0.2434 | 0.0784314..0.352941 | sky 53.3 % of the frame (> 50 %): beat-7 band not applied (14.10.5 ruling) |

### b07_SluiceLedge_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.0830 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 8754 px excluded |
| mean luma | INFO | 0.3466 | 0.0784314..0.352941 | sky 53.3 % of the frame (> 50 %): beat-7 band not applied (14.10.5 ruling) |

### b07_SluiceLedge_look_r (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 5 | 0 px | first at [[1565, 747], [1565, 750], [1565, 753], [1565, 754], [1565, 755]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | INFO | 0.2133 | 0.0784314..0.352941 | sky 53.3 % of the frame (> 50 %): beat-7 band not applied (14.10.5 ruling) |

### b07_SluiceLedge_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2601 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2899 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\b07_SluiceLedge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1657 | 0.0784314..0.352941 | display |

### pose_conddown (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_conddown.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1509 | 0.0784314..0.352941 | display |

### pose_faceup (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_faceup.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3119 | 0.0784314..0.352941 | display |

### pose_ks20s1470 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_ks20s1470.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2755 | 0.0784314..0.352941 | display |

### pose_ledgedown (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_ledgedown.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1026 | 0.0784314..0.352941 | display |

### pose_mouthback (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_mouthback.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2279 | 0.0784314..0.352941 | display |

### pose_nadir1470 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_nadir1470.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.0803 | 0.0784314..0.352941 | display |

### pose_north1470 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_north1470.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0021 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2609 | 0.0784314..0.352941 | display |

### pose_northdown (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_northdown.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1605 | 0.0784314..0.352941 | display |

### pose_shaft (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_shaft.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2760 | 0.0784314..0.352941 | display |

### pose_south (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_south.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2512 | 0.0784314..0.352941 | display |

### pose_tun1405 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_tun1405.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0041 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1598 | 0.0784314..0.352941 | display |

### pose_tunback1395 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\dusk-05\pose_tunback1395.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0045 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1535 | 0.0784314..0.352941 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
