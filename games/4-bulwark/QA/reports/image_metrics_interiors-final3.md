# Image metrics: interiors-final3

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3`  
Scene constants: BulwarkConst.cs  
Images: 43, failing images: 7. Checks: 335 PASS, 9 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_WaveGallery_KS01_gal_far | 1 | 1920x1080 | 0.171 | 0.084 | 0.0003 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_1610 | 1 | 1680x1050 | 0.174 | 0.086 | 0.0005 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_219 | 1 | 2560x1080 | 0.159 | 0.076 | 0.0003 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_4k | 1 | 3840x2160 | 0.171 | 0.083 | 0.0006 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit | 1 | 1920x1080 | 0.147 | 0.088 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_1610 | 1 | 1680x1050 | 0.152 | 0.089 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_219 | 1 | 2560x1080 | 0.135 | 0.076 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_4k | 1 | 3840x2160 | 0.147 | 0.088 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS03_door_open | 1 | 1920x1080 | 0.256 | 0.077 | 0.0000 | - | FAIL | NaN-like pixels, mean luma |
| b01_WaveGallery_KS03_door_open_1610 | 1 | 1680x1050 | 0.269 | 0.077 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_KS03_door_open_219 | 1 | 2560x1080 | 0.223 | 0.076 | 0.0000 | - | FAIL | NaN-like pixels, mean luma |
| b01_WaveGallery_KS03_door_open_4k | 1 | 3840x2160 | 0.256 | 0.077 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_end | 1 | 1920x1080 | 0.360 | 0.118 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_look_back | 1 | 1920x1080 | 0.120 | 0.076 | 0.0006 | - | PASS |  |
| b01_WaveGallery_look_down | 1 | 1920x1080 | 0.135 | 0.063 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_l | 1 | 1920x1080 | 0.157 | 0.076 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_r | 1 | 1920x1080 | 0.163 | 0.105 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_up | 1 | 1920x1080 | 0.218 | 0.088 | 0.0000 | - | PASS |  |
| b01_WaveGallery_mid | 1 | 1920x1080 | 0.120 | 0.076 | 0.0006 | - | PASS |  |
| b01_WaveGallery_start | 1 | 1920x1080 | 0.111 | 0.060 | 0.0004 | - | PASS |  |
| b05_GateTower_KS13_atrium | 5 | 1920x1080 | 0.126 | 0.062 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium_1610 | 5 | 1680x1050 | 0.125 | 0.062 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium_219 | 5 | 2560x1080 | 0.131 | 0.063 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium_4k | 5 | 3840x2160 | 0.126 | 0.062 | 0.0000 | - | FAIL | NaN-like pixels |
| b05_GateTower_KS14_stairs_down | 5 | 1920x1080 | 0.229 | 0.100 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_1610 | 5 | 1680x1050 | 0.232 | 0.102 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_219 | 5 | 2560x1080 | 0.219 | 0.099 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_4k | 5 | 3840x2160 | 0.229 | 0.099 | 0.0000 | - | PASS |  |
| b05_GateTower_end | 5 | 1920x1080 | 0.198 | 0.103 | 0.0000 | - | PASS |  |
| b05_GateTower_look_back | 5 | 1920x1080 | 0.145 | 0.062 | 0.0000 | - | PASS |  |
| b05_GateTower_look_down | 5 | 1920x1080 | 0.180 | 0.108 | 0.0000 | - | PASS |  |
| b05_GateTower_look_l | 5 | 1920x1080 | 0.294 | 0.132 | 0.0049 | - | PASS |  |
| b05_GateTower_look_r | 5 | 1920x1080 | 0.198 | 0.112 | 0.0000 | - | PASS |  |
| b05_GateTower_look_up | 5 | 1920x1080 | 0.279 | 0.131 | 0.0000 | - | PASS |  |
| b05_GateTower_mid | 5 | 1920x1080 | 0.169 | 0.108 | 0.0000 | - | PASS |  |
| b05_GateTower_start | 5 | 1920x1080 | 0.199 | 0.066 | 0.0000 | - | PASS |  |
| pose_cone | 1 | 1920x1080 | 0.174 | 0.072 | 0.0001 | - | PASS |  |
| pose_gwest | 1 | 1920x1080 | 0.101 | 0.060 | 0.0000 | - | FAIL | mean luma |
| pose_hall | 5 | 1920x1080 | 0.190 | 0.100 | 0.0001 | - | PASS |  |
| pose_land6 | 5 | 1920x1080 | 0.221 | 0.111 | 0.0000 | - | PASS |  |
| pose_p11 | 5 | 1920x1080 | 0.203 | 0.119 | 0.0000 | - | PASS |  |
| pose_p5 | 5 | 1920x1080 | 0.276 | 0.123 | 0.0000 | - | PASS |  |
| pose_p8 | 5 | 1920x1080 | 0.271 | 0.118 | 0.0000 | - | PASS |  |

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
| KS15_win_first | - | INFO |
| KS16_win_up | - | INFO |
| KS17_win_whale | - | INFO |
| KS18_win_down | - | INFO |
| KS19_sluice | - | INFO |
| KS20_ledge_world | - | INFO |
| KS21_beacon | - | INFO |

## Details

### b01_WaveGallery_KS01_gal_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS01_gal_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1706 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS01_gal_far_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0005 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1742 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS01_gal_far_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1590 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS01_gal_far_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1706 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS02_gal_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1471 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS02_gal_slit_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1519 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS02_gal_slit_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1353 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS02_gal_slit_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1471 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS03_door_open (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS03_door_open.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[959, 552]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2557 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS03_door_open_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2689 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS03_door_open_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1279, 552]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2226 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_KS03_door_open_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2556 | 0.27451..0.470588 | display |

### b01_WaveGallery_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3597 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1204 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1355 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1566 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1630 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2177 | 0.109804..0.27451 | display |

### b01_WaveGallery_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1204 | 0.109804..0.27451 | display |

### b01_WaveGallery_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b01_WaveGallery_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0004 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1107 | 0.109804..0.27451 | display |

### b05_GateTower_KS13_atrium (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_KS13_atrium.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1258 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_KS13_atrium_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1249 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_KS13_atrium_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1311 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_KS13_atrium_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[3021, 1822]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1259 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_KS14_stairs_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2292 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_KS14_stairs_down_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2324 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_KS14_stairs_down_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2192 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_KS14_stairs_down_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2292 | 0.109804..0.313725 | display |

### b05_GateTower_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_end.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1447 | 0.109804..0.313725 | display |

### b05_GateTower_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1802 | 0.109804..0.313725 | display |

### b05_GateTower_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0049 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2936 | 0.109804..0.313725 | display |

### b05_GateTower_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1979 | 0.109804..0.313725 | display |

### b05_GateTower_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2788 | 0.109804..0.313725 | display |

### b05_GateTower_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1695 | 0.109804..0.313725 | display |

### b05_GateTower_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\b05_GateTower_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1992 | 0.109804..0.313725 | display |

### pose_cone (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\pose_cone.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1745 | 0.109804..0.27451 | display |

### pose_gwest (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\pose_gwest.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1008 | 0.109804..0.27451 | display |

### pose_hall (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\pose_hall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1904 | 0.109804..0.313725 | display |

### pose_land6 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\pose_land6.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2214 | 0.109804..0.313725 | display |

### pose_p11 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\pose_p11.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2032 | 0.109804..0.313725 | display |

### pose_p5 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\pose_p5.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2763 | 0.109804..0.313725 | display |

### pose_p8 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\interiors-final3\pose_p8.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2713 | 0.109804..0.313725 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
