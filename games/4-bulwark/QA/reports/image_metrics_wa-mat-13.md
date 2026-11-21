# Image metrics: wa-mat-13

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13`  
Scene constants: BulwarkConst.cs  
Images: 43, failing images: 6. Checks: 337 PASS, 7 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Input warnings

- regions file <WORKSPACE>\4 Megastructures\4 Megastructures\Tools\qa\regions_default.json missing or empty

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| mat_b01_gallery_close0 | 1 | 1920x1080 | 0.119 | 0.043 | 0.0000 | - | PASS |  |
| mat_b01_gallery_close1 | 1 | 1920x1080 | 0.107 | 0.056 | 0.0000 | - | FAIL | mean luma |
| mat_b01_gallery_far | 1 | 1920x1080 | 0.111 | 0.087 | 0.0004 | - | PASS |  |
| mat_b01_gallery_row | 1 | 1920x1080 | 0.143 | 0.079 | 0.0000 | - | PASS |  |
| mat_b02_crest_close0 | 2 | 1920x1080 | 0.339 | 0.206 | 0.0000 | - | PASS |  |
| mat_b02_crest_close1 | 2 | 1920x1080 | 0.301 | 0.176 | 0.0000 | - | FAIL | mean luma |
| mat_b02_crest_far | 2 | 1920x1080 | 0.417 | 0.230 | 0.0000 | - | PASS |  |
| mat_b02_crest_road_far | 2 | 1920x1080 | 0.429 | 0.229 | 0.0000 | - | PASS |  |
| mat_b02_crest_road_near | 2 | 1920x1080 | 0.402 | 0.214 | 0.0000 | - | PASS |  |
| mat_b02_crest_road_top | 2 | 1920x1080 | 0.251 | 0.199 | 0.0000 | - | FAIL | mean luma |
| mat_b02_crest_row | 2 | 1920x1080 | 0.379 | 0.211 | 0.0000 | - | PASS |  |
| mat_b03_spillway_close0 | 3 | 1920x1080 | 0.376 | 0.146 | 0.0000 | - | PASS |  |
| mat_b03_spillway_close1 | 3 | 1920x1080 | 0.435 | 0.139 | 0.0000 | - | PASS |  |
| mat_b03_spillway_far | 3 | 1920x1080 | 0.408 | 0.227 | 0.0000 | - | PASS |  |
| mat_b03_spillway_field_across | 3 | 1920x1080 | 0.461 | 0.248 | 0.0000 | - | PASS |  |
| mat_b03_spillway_field_along | 3 | 1920x1080 | 0.415 | 0.220 | 0.0000 | - | PASS |  |
| mat_b03_spillway_grating_down | 3 | 1920x1080 | 0.298 | 0.156 | 0.0000 | - | FAIL | mean luma |
| mat_b03_spillway_grating_grazing | 3 | 1920x1080 | 0.370 | 0.196 | 0.0000 | - | FAIL | mean luma |
| mat_b03_spillway_row | 3 | 1920x1080 | 0.374 | 0.212 | 0.0000 | - | PASS |  |
| mat_b04_reef_close0 | 4 | 1920x1080 | 0.453 | 0.142 | 0.0000 | - | PASS |  |
| mat_b04_reef_close1 | 4 | 1920x1080 | 0.450 | 0.122 | 0.0000 | - | PASS |  |
| mat_b04_reef_crust_far | 4 | 1920x1080 | 0.536 | 0.318 | 0.0000 | - | PASS |  |
| mat_b04_reef_crust_mid | 4 | 1920x1080 | 0.399 | 0.129 | 0.0000 | - | PASS |  |
| mat_b04_reef_crust_near | 4 | 1920x1080 | 0.354 | 0.132 | 0.0000 | - | FAIL | NaN-like pixels, mean luma |
| mat_b04_reef_far | 4 | 1920x1080 | 0.585 | 0.385 | 0.0000 | - | PASS |  |
| mat_b04_reef_row | 4 | 1920x1080 | 0.581 | 0.219 | 0.0000 | - | PASS |  |
| mat_b05_stairs_close0 | 5 | 1920x1080 | 0.195 | 0.107 | 0.0000 | - | PASS |  |
| mat_b05_stairs_close1 | 5 | 1920x1080 | 0.243 | 0.113 | 0.0000 | - | PASS |  |
| mat_b05_stairs_far | 5 | 1920x1080 | 0.131 | 0.106 | 0.0058 | - | PASS |  |
| mat_b05_stairs_row | 5 | 1920x1080 | 0.203 | 0.102 | 0.0000 | - | PASS |  |
| mat_b06_windows_close0 | 6 | 1920x1080 | 0.163 | 0.079 | 0.0000 | - | PASS |  |
| mat_b06_windows_close1 | 6 | 1920x1080 | 0.150 | 0.084 | 0.0000 | - | PASS |  |
| mat_b06_windows_far | 6 | 1920x1080 | 0.106 | 0.079 | 0.0005 | - | PASS |  |
| mat_b06_windows_row | 6 | 1920x1080 | 0.127 | 0.076 | 0.0000 | - | PASS |  |
| mat_b07_ledge_close0 | 7 | 1920x1080 | 0.242 | 0.090 | 0.0000 | - | PASS |  |
| mat_b07_ledge_close1 | 7 | 1920x1080 | 0.263 | 0.090 | 0.0000 | - | PASS |  |
| mat_b07_ledge_far | 7 | 1920x1080 | 0.314 | 0.100 | 0.0110 | - | PASS |  |
| mat_b07_ledge_row | 7 | 1920x1080 | 0.284 | 0.090 | 0.0000 | - | PASS |  |
| mat_b07_tunnel_close0 | 7 | 1920x1080 | 0.099 | 0.053 | 0.0000 | - | PASS |  |
| mat_b07_tunnel_close1 | 7 | 1920x1080 | 0.107 | 0.061 | 0.0000 | - | PASS |  |
| mat_b07_tunnel_far | 7 | 1920x1080 | 0.085 | 0.049 | 0.0073 | - | PASS |  |
| mat_b07_tunnel_grating_wall | 7 | 1920x1080 | 0.102 | 0.040 | 0.0000 | - | PASS |  |
| mat_b07_tunnel_row | 7 | 1920x1080 | 0.109 | 0.053 | 0.0000 | - | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | - | INFO |
| KS02_gal_slit | - | INFO |
| KS03_door_open | - | INFO |
| KS04_crest_sea | - | INFO |
| KS05_crest_drop | - | INFO |
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
| KS19_sluice | - | INFO |
| KS20_ledge_world | - | INFO |
| KS21_beacon | - | INFO |

## Details

### mat_b01_gallery_close0 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b01_gallery_close0.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1185 | 0.109804..0.27451 | display |

### mat_b01_gallery_close1 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b01_gallery_close1.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1068 | 0.109804..0.27451 | display |

### mat_b01_gallery_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b01_gallery_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0004 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1110 | 0.109804..0.27451 | display |

### mat_b01_gallery_row (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b01_gallery_row.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1430 | 0.109804..0.27451 | display |

### mat_b02_crest_close0 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b02_crest_close0.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3385 | 0.333333..0.588235 | display |

### mat_b02_crest_close1 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b02_crest_close1.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3013 | 0.333333..0.588235 | display |

### mat_b02_crest_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b02_crest_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4171 | 0.333333..0.588235 | display |

### mat_b02_crest_road_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b02_crest_road_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4287 | 0.333333..0.588235 | display |

### mat_b02_crest_road_near (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b02_crest_road_near.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4021 | 0.333333..0.588235 | display |

### mat_b02_crest_road_top (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b02_crest_road_top.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2508 | 0.333333..0.588235 | display |

### mat_b02_crest_row (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b02_crest_row.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3789 | 0.333333..0.588235 | display |

### mat_b03_spillway_close0 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b03_spillway_close0.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3761 | 0.372549..0.588235 | display |

### mat_b03_spillway_close1 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b03_spillway_close1.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4349 | 0.372549..0.588235 | display |

### mat_b03_spillway_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b03_spillway_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4083 | 0.372549..0.588235 | display |

### mat_b03_spillway_field_across (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b03_spillway_field_across.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4609 | 0.372549..0.588235 | display |

### mat_b03_spillway_field_along (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b03_spillway_field_along.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4152 | 0.372549..0.588235 | display |

### mat_b03_spillway_grating_down (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b03_spillway_grating_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2984 | 0.372549..0.588235 | display |

### mat_b03_spillway_grating_grazing (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b03_spillway_grating_grazing.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3702 | 0.372549..0.588235 | display |

### mat_b03_spillway_row (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b03_spillway_row.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3737 | 0.372549..0.588235 | display |

### mat_b04_reef_close0 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b04_reef_close0.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4527 | 0.392157..0.647059 | display |

### mat_b04_reef_close1 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b04_reef_close1.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4499 | 0.392157..0.647059 | display |

### mat_b04_reef_crust_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b04_reef_crust_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5360 | 0.392157..0.647059 | display |

### mat_b04_reef_crust_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b04_reef_crust_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3987 | 0.392157..0.647059 | display |

### mat_b04_reef_crust_near (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b04_reef_crust_near.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 2 (mode all), dark 1, first at [[47, 45], [203, 80], [144, 433]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3541 | 0.392157..0.647059 | display |

### mat_b04_reef_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b04_reef_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5848 | 0.392157..0.647059 | display |

### mat_b04_reef_row (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b04_reef_row.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5811 | 0.392157..0.647059 | display |

### mat_b05_stairs_close0 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b05_stairs_close0.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1950 | 0.109804..0.313725 | display |

### mat_b05_stairs_close1 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b05_stairs_close1.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2431 | 0.109804..0.313725 | display |

### mat_b05_stairs_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b05_stairs_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0058 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1307 | 0.109804..0.313725 | display |

### mat_b05_stairs_row (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b05_stairs_row.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2031 | 0.109804..0.313725 | display |

### mat_b06_windows_close0 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b06_windows_close0.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1629 | 0.054902..0.333333 | display |

### mat_b06_windows_close1 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b06_windows_close1.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1498 | 0.054902..0.333333 | display |

### mat_b06_windows_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b06_windows_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0005 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1063 | 0.054902..0.333333 | display |

### mat_b06_windows_row (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b06_windows_row.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1271 | 0.054902..0.333333 | display |

### mat_b07_ledge_close0 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b07_ledge_close0.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2425 | 0.0784314..0.352941 | display |

### mat_b07_ledge_close1 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b07_ledge_close1.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2632 | 0.0784314..0.352941 | display |

### mat_b07_ledge_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b07_ledge_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0110 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3143 | 0.0784314..0.352941 | display |

### mat_b07_ledge_row (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b07_ledge_row.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2843 | 0.0784314..0.352941 | display |

### mat_b07_tunnel_close0 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b07_tunnel_close0.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.0986 | 0.0784314..0.352941 | display |

### mat_b07_tunnel_close1 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b07_tunnel_close1.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1072 | 0.0784314..0.352941 | display |

### mat_b07_tunnel_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b07_tunnel_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0073 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.0848 | 0.0784314..0.352941 | display |

### mat_b07_tunnel_grating_wall (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b07_tunnel_grating_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1018 | 0.0784314..0.352941 | display |

### mat_b07_tunnel_row (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-mat-13\mat_b07_tunnel_row.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1088 | 0.0784314..0.352941 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
