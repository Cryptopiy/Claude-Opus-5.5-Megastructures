# Image metrics: atmos-04

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04`  
Scene constants: BulwarkConst.cs  
Images: 20, failing images: 4. Checks: 156 PASS, 4 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Input warnings

- regions file <WORKSPACE>\4 Megastructures\4 Megastructures\Tools\qa\regions_default.json missing or empty

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b02_Crest_KS04_crest_sea | 2 | 1920x1080 | 0.404 | 0.188 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea_1610 | 2 | 1680x1050 | 0.404 | 0.189 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea_219 | 2 | 2560x1080 | 0.402 | 0.188 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea_4k | 2 | 3840x2160 | 0.404 | 0.188 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop | 2 | 1920x1080 | 0.445 | 0.234 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_1610 | 2 | 1680x1050 | 0.445 | 0.234 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_219 | 2 | 2560x1080 | 0.450 | 0.231 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_4k | 2 | 3840x2160 | 0.445 | 0.233 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow | 3 | 1920x1080 | 0.535 | 0.229 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_1610 | 3 | 1680x1050 | 0.538 | 0.229 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_219 | 3 | 2560x1080 | 0.526 | 0.230 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_4k | 3 | 3840x2160 | 0.535 | 0.229 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world | 7 | 1920x1080 | 0.366 | 0.125 | 0.0000 | - | FAIL | mean luma |
| b07_SluiceLedge_KS20_ledge_world_1610 | 7 | 1680x1050 | 0.366 | 0.125 | 0.0000 | - | FAIL | mean luma |
| b07_SluiceLedge_KS20_ledge_world_219 | 7 | 2560x1080 | 0.364 | 0.125 | 0.0000 | - | FAIL | mean luma |
| b07_SluiceLedge_KS20_ledge_world_4k | 7 | 3840x2160 | 0.366 | 0.125 | 0.0000 | - | FAIL | mean luma |
| b07_SluiceLedge_KS21_beacon | 7 | 1920x1080 | 0.327 | 0.129 | 0.0145 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_1610 | 7 | 1680x1050 | 0.325 | 0.129 | 0.0166 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_219 | 7 | 2560x1080 | 0.334 | 0.129 | 0.0109 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_4k | 7 | 3840x2160 | 0.327 | 0.129 | 0.0148 | - | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | - | INFO |
| KS02_gal_slit | - | INFO |
| KS03_door_open | - | INFO |
| KS04_crest_sea | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS05_crest_drop | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS06_crest_axis | - | INFO |
| KS07_spill_gates | - | INFO |
| KS08_spill_bow | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
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
| KS20_ledge_world | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS21_beacon | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |

## Details

### b02_Crest_KS04_crest_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b02_Crest_KS04_crest_sea.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4036 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b02_Crest_KS04_crest_sea_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4043 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b02_Crest_KS04_crest_sea_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4021 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b02_Crest_KS04_crest_sea_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4036 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b02_Crest_KS05_crest_drop.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4453 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b02_Crest_KS05_crest_drop_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4454 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b02_Crest_KS05_crest_drop_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4498 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b02_Crest_KS05_crest_drop_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4452 | 0.333333..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b03_SpillwayBridge_KS08_spill_bow.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5346 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b03_SpillwayBridge_KS08_spill_bow_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5375 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b03_SpillwayBridge_KS08_spill_bow_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5260 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b03_SpillwayBridge_KS08_spill_bow_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5346 | 0.372549..0.588235 | display |

### b07_SluiceLedge_KS20_ledge_world (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b07_SluiceLedge_KS20_ledge_world.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3659 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b07_SluiceLedge_KS20_ledge_world_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3662 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b07_SluiceLedge_KS20_ledge_world_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3641 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b07_SluiceLedge_KS20_ledge_world_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3659 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS21_beacon (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b07_SluiceLedge_KS21_beacon.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0145 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3267 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b07_SluiceLedge_KS21_beacon_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0166 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3247 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b07_SluiceLedge_KS21_beacon_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0109 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3336 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\atmos-04\b07_SluiceLedge_KS21_beacon_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0148 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3267 | 0.078431..0.352941 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
