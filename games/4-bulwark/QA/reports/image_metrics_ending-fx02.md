# Image metrics: ending-fx02

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02`  
Scene constants: BulwarkConst.cs  
Images: 19, failing images: 1. Checks: 151 PASS, 1 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b08_Ending_KS21b_cabin_door | 7 | 1920x1080 | 0.215 | 0.025 | 0.0000 | - | PASS |  |
| b08_Ending_KS22_ride_drop | 7 | 1920x1080 | 0.180 | 0.041 | 0.0000 | - | PASS |  |
| b08_Ending_KS22b_glass_floor | 7 | 1920x1080 | 0.131 | 0.040 | 0.0000 | - | PASS |  |
| b08_Ending_KS23_ride_sunset | 7 | 1920x1080 | 0.269 | 0.041 | 0.0009 | - | PASS |  |
| b08_Ending_KS23b_face_rush | 7 | 1920x1080 | 0.145 | 0.036 | 0.0000 | - | PASS |  |
| b08_Ending_KS24_ride_lookup | 7 | 1920x1080 | 0.195 | 0.031 | 0.0000 | - | PASS |  |
| b08_Ending_KS24b_ride_mist | 7 | 1920x1080 | 0.237 | 0.051 | 0.0000 | - | PASS |  |
| b08_Ending_KS25_ride_lanterns | 7 | 1920x1080 | 0.198 | 0.063 | 0.0005 | - | PASS |  |
| b08_Ending_KS26_shuttle_back | 7 | 1920x1080 | 0.190 | 0.069 | 0.0000 | - | PASS |  |
| b08_Ending_KS26a_transfer | 7 | 1920x1080 | 0.155 | 0.023 | 0.0913 | - | FAIL | NaN-like pixels |
| b08_Ending_KS26b_platform_walk | 7 | 1920x1080 | 0.272 | 0.103 | 0.0029 | - | PASS |  |
| b08_Ending_KS27_keeper | 7 | 1920x1080 | 0.198 | 0.092 | 0.0496 | - | PASS |  |
| b08_Ending_KS28_dam_reveal | 7 | 1920x1080 | 0.179 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS28_dam_reveal_219 | 7 | 2520x1080 | 0.175 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS28_dam_reveal_4k | 7 | 3840x2160 | 0.178 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS28b_crest_dark | 7 | 1920x1080 | 0.169 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS28c_crest_zoom | 7 | 1920x1080 | 0.260 | 0.185 | 0.0000 | - | PASS |  |
| b08_Ending_KS28d_foot_zoom | 7 | 1920x1080 | 0.138 | 0.084 | 0.0028 | - | PASS |  |
| b08_Ending_KS29_lift | 7 | 1920x1080 | 0.185 | 0.143 | 0.0000 | - | PASS |  |

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
| KS18_win_down | - | INFO |
| KS19_sluice | - | INFO |
| KS20_ledge_world | - | INFO |
| KS21_beacon | - | INFO |
| KS22_ride_drop | 1920x1080 | INFO |
| KS23_ride_sunset | 1920x1080 | INFO |
| KS24_ride_lookup | 1920x1080 | INFO |
| KS25_ride_lanterns | 1920x1080 | INFO |
| KS26a_transfer | 1920x1080 | INFO |
| KS26_shuttle_back | 1920x1080 | INFO |
| KS26b_platform_walk | 1920x1080 | INFO |
| KS27_keeper | 1920x1080 | INFO |
| KS28_dam_reveal | 1920x1080, 2520x1080, 3840x2160 | PASS |
| KS28b_crest_dark | 1920x1080 | INFO |
| KS29_lift | 1920x1080 | INFO |
| KK1_arm_tower | - | INFO |
| KK2_head_eyes | - | INFO |
| KK4_arm_glass | - | INFO |

## Details

### b08_Ending_KS21b_cabin_door (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS21b_cabin_door.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2152 | 0.0784314..0.352941 | display |

### b08_Ending_KS22_ride_drop (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS22_ride_drop.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1798 | 0.0784314..0.313725 | display |

### b08_Ending_KS22b_glass_floor (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS22b_glass_floor.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1308 | 0.0784314..0.352941 | display |

### b08_Ending_KS23_ride_sunset (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS23_ride_sunset.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0009 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2687 | 0.0784314..0.313725 | display |

### b08_Ending_KS23b_face_rush (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS23b_face_rush.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1452 | 0.0784314..0.352941 | display |

### b08_Ending_KS24_ride_lookup (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS24_ride_lookup.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1945 | 0.0784314..0.313725 | display |

### b08_Ending_KS24b_ride_mist (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS24b_ride_mist.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2371 | 0.0784314..0.352941 | display |

### b08_Ending_KS25_ride_lanterns (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS25_ride_lanterns.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1983 | 0.0784314..0.313725 | display |

### b08_Ending_KS26_shuttle_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS26_shuttle_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1902 | 0.0784314..0.313725 | display |

### b08_Ending_KS26a_transfer (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS26a_transfer.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[919, 161]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0913 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1554 | 0.0784314..0.313725 | display |

### b08_Ending_KS26b_platform_walk (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS26b_platform_walk.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0029 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2718 | 0.0784314..0.27451 | display |

### b08_Ending_KS27_keeper (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS27_keeper.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0496 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1976 | 0.0784314..0.27451 | display |

### b08_Ending_KS28_dam_reveal (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS28_dam_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1788 | 0.0980392..0.27451 | display |

### b08_Ending_KS28_dam_reveal_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS28_dam_reveal_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2520x1080 | 2520x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1755 | 0.0980392..0.27451 | display |

### b08_Ending_KS28_dam_reveal_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS28_dam_reveal_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1784 | 0.0980392..0.27451 | display |

### b08_Ending_KS28b_crest_dark (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS28b_crest_dark.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1687 | 0.0784314..0.27451 | display |

### b08_Ending_KS28c_crest_zoom (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS28c_crest_zoom.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2598 | 0.0784314..0.352941 | display |

### b08_Ending_KS28d_foot_zoom (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS28d_foot_zoom.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0028 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1380 | 0.0784314..0.352941 | display |

### b08_Ending_KS29_lift (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-fx02\b08_Ending_KS29_lift.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1855 | 0.0784314..0.27451 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
