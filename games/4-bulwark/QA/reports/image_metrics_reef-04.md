# Image metrics: reef-04

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04`  
Scene constants: BulwarkConst.cs  
Images: 23, failing images: 12. Checks: 172 PASS, 12 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b04_ArmourSlope_KS10_reef_wide | 4 | 1920x1080 | 0.408 | 0.138 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS10_reef_wide_1610 | 4 | 1680x1050 | 0.405 | 0.137 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS10_reef_wide_219 | 4 | 2560x1080 | 0.418 | 0.142 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS10_reef_wide_4k | 4 | 3840x2160 | 0.408 | 0.137 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS11_reef_spray | 4 | 1920x1080 | 0.517 | 0.199 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS11_reef_spray_1610 | 4 | 1680x1050 | 0.521 | 0.198 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_219 | 4 | 2560x1080 | 0.509 | 0.200 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS11_reef_spray_4k | 4 | 3840x2160 | 0.517 | 0.199 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS12_tower_up | 4 | 1920x1080 | 0.486 | 0.222 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_1610 | 4 | 1680x1050 | 0.486 | 0.226 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_219 | 4 | 2560x1080 | 0.488 | 0.222 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_4k | 4 | 3840x2160 | 0.486 | 0.222 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_end | 4 | 1920x1080 | 0.301 | 0.067 | 0.0000 | - | FAIL | mean luma |
| b04_ArmourSlope_look_back | 4 | 1920x1080 | 0.624 | 0.187 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_down | 4 | 1920x1080 | 0.617 | 0.186 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_l | 4 | 1920x1080 | 0.609 | 0.319 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_r | 4 | 1920x1080 | 0.409 | 0.132 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_look_up | 4 | 1920x1080 | 0.411 | 0.353 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_mid | 4 | 1920x1080 | 0.529 | 0.195 | 0.0003 | - | PASS |  |
| b04_ArmourSlope_start | 4 | 1920x1080 | 0.438 | 0.252 | 0.0001 | - | PASS |  |
| pose_b04p_belt | 4 | 1920x1080 | 0.444 | 0.121 | 0.0000 | - | FAIL | NaN-like pixels |
| pose_b04p_far | 4 | 1920x1080 | 0.503 | 0.158 | 0.0000 | - | FAIL | NaN-like pixels |
| pose_b04p_isl | 4 | 1920x1080 | 0.482 | 0.120 | 0.0000 | - | FAIL | NaN-like pixels |

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
| KS10_reef_wide | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS11_reef_spray | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS12_tower_up | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
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

### b04_ArmourSlope_KS10_reef_wide (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS10_reef_wide.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1273, 536], [1172, 538]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4081 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS10_reef_wide_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[835, 501], [420, 511], [1325, 544]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4047 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS10_reef_wide_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1593, 536], [1492, 538]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4177 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS10_reef_wide_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 7 | 0 | bright 7 (mode all), dark 0, first at [[1628, 1016], [1154, 1053], [1464, 1062], [2352, 1077], [2370, 1078]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4084 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS11_reef_spray.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1756, 673], [1788, 682]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5168 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS11_reef_spray_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5206 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS11_reef_spray_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[2076, 673], [2108, 682]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5087 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS11_reef_spray_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 13 (mode all), dark 0, first at [[3326, 1073], [3471, 1244], [2822, 1334], [3416, 1345], [3424, 1345]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5168 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS12_tower_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4859 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS12_tower_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4861 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS12_tower_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4882 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_KS12_tower_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4858 | 0.392157..0.647059 | display |

### b04_ArmourSlope_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3012 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.6244 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.6171 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.6094 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_r (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[1345, 397], [1342, 417], [1271, 528], [581, 557], [742, 557]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4091 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4114 | 0.392157..0.647059 | display |

### b04_ArmourSlope_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5285 | 0.392157..0.647059 | display |

### b04_ArmourSlope_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\b04_ArmourSlope_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4377 | 0.392157..0.647059 | display |

### pose_b04p_belt (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\pose_b04p_belt.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[594, 297]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4444 | 0.392157..0.647059 | display |

### pose_b04p_far (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\pose_b04p_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1364, 634]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5025 | 0.392157..0.647059 | display |

### pose_b04p_isl (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\land\QA\captures\reef-04\pose_b04p_isl.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[64, 744], [708, 771], [1133, 799], [1158, 800], [1883, 824]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4816 | 0.392157..0.647059 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
