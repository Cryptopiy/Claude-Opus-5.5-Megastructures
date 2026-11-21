# Image metrics: fin-ending

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending`  
Scene constants: BulwarkConst.cs  
Images: 48, failing images: 11. Checks: 394 PASS, 11 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b08_Ending_KS21b_cabin_door | 8 | 1920x1080 | 0.215 | 0.025 | 0.0000 | - | PASS |  |
| b08_Ending_KS21b_cabin_door_1610 | 8 | 1680x1050 | 0.214 | 0.025 | 0.0000 | - | FAIL | NaN-like pixels |
| b08_Ending_KS21b_cabin_door_219 | 8 | 2560x1080 | 0.229 | 0.028 | 0.0000 | - | PASS |  |
| b08_Ending_KS22_ride_drop | 8 | 1920x1080 | 0.180 | 0.041 | 0.0000 | 0.500 | PASS |  |
| b08_Ending_KS22_ride_drop_1610 | 8 | 1680x1050 | 0.179 | 0.041 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS22_ride_drop_219 | 8 | 2560x1080 | 0.183 | 0.039 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS22b_glass_floor | 8 | 1920x1080 | 0.131 | 0.040 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS22b_glass_floor_1610 | 8 | 1680x1050 | 0.130 | 0.040 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS22b_glass_floor_219 | 8 | 2560x1080 | 0.127 | 0.037 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS23_ride_sunset | 8 | 1920x1080 | 0.269 | 0.041 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS23_ride_sunset_1610 | 8 | 1680x1050 | 0.270 | 0.040 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS23_ride_sunset_219 | 8 | 2560x1080 | 0.265 | 0.041 | 0.0000 | 0.500 | PASS |  |
| b08_Ending_KS23b_face_rush | 8 | 1920x1080 | 0.145 | 0.036 | 0.0000 | - | PASS |  |
| b08_Ending_KS23b_face_rush_1610 | 8 | 1680x1050 | 0.145 | 0.036 | 0.0000 | - | PASS |  |
| b08_Ending_KS23b_face_rush_219 | 8 | 2560x1080 | 0.143 | 0.035 | 0.0000 | - | FAIL | fireflies |
| b08_Ending_KS24_ride_lookup | 8 | 1920x1080 | 0.195 | 0.031 | 0.0000 | 0.500 | PASS |  |
| b08_Ending_KS24_ride_lookup_1610 | 8 | 1680x1050 | 0.190 | 0.031 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS24_ride_lookup_219 | 8 | 2560x1080 | 0.207 | 0.031 | 0.0000 | 0.500 | FAIL | NaN-like pixels |
| b08_Ending_KS24b_ride_mist | 8 | 1920x1080 | 0.237 | 0.051 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS24b_ride_mist_1610 | 8 | 1680x1050 | 0.238 | 0.054 | 0.0000 | 0.500 | FAIL | fireflies |
| b08_Ending_KS24b_ride_mist_219 | 8 | 2560x1080 | 0.234 | 0.051 | 0.0000 | 0.500 | FAIL | fireflies |
| b08_Ending_KS25_ride_lanterns | 8 | 1920x1080 | 0.199 | 0.063 | 0.0005 | - | PASS |  |
| b08_Ending_KS25_ride_lanterns_1610 | 8 | 1680x1050 | 0.198 | 0.063 | 0.0005 | - | PASS |  |
| b08_Ending_KS25_ride_lanterns_219 | 8 | 2560x1080 | 0.198 | 0.060 | 0.0005 | - | PASS |  |
| b08_Ending_KS26_shuttle_back | 8 | 1920x1080 | 0.189 | 0.069 | 0.0000 | - | PASS |  |
| b08_Ending_KS26_shuttle_back_1610 | 8 | 1680x1050 | 0.191 | 0.073 | 0.0000 | - | PASS |  |
| b08_Ending_KS26_shuttle_back_219 | 8 | 2560x1080 | 0.186 | 0.068 | 0.0000 | - | FAIL | fireflies |
| b08_Ending_KS26a_transfer | 8 | 1920x1080 | 0.155 | 0.023 | 0.0911 | - | FAIL | NaN-like pixels |
| b08_Ending_KS26a_transfer_1610 | 8 | 1680x1050 | 0.162 | 0.023 | 0.1017 | - | FAIL | NaN-like pixels |
| b08_Ending_KS26a_transfer_219 | 8 | 2560x1080 | 0.149 | 0.026 | 0.0685 | - | FAIL | NaN-like pixels |
| b08_Ending_KS26b_platform_walk | 9 | 1920x1080 | 0.272 | 0.104 | 0.0029 | - | PASS |  |
| b08_Ending_KS26b_platform_walk_1610 | 9 | 1680x1050 | 0.276 | 0.104 | 0.0036 | - | FAIL | mean luma |
| b08_Ending_KS26b_platform_walk_219 | 9 | 2560x1080 | 0.260 | 0.101 | 0.0022 | - | PASS |  |
| b08_Ending_KS27_keeper | 9 | 1920x1080 | 0.197 | 0.092 | 0.0480 | - | PASS |  |
| b08_Ending_KS27_keeper_1610 | 9 | 1680x1050 | 0.201 | 0.095 | 0.0518 | - | FAIL | NaN-like pixels |
| b08_Ending_KS27_keeper_219 | 9 | 2560x1080 | 0.190 | 0.092 | 0.0359 | - | PASS |  |
| b08_Ending_KS28_dam_reveal | 9 | 1920x1080 | 0.177 | 0.092 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS28_dam_reveal_1610 | 9 | 1680x1050 | 0.179 | 0.093 | 0.0000 | - | PASS |  |
| b08_Ending_KS28_dam_reveal_219 | 9 | 2560x1080 | 0.174 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS28_dam_reveal_4k | 9 | 3840x2160 | 0.177 | 0.092 | 0.0000 | 0.250 | PASS |  |
| b08_Ending_KS28b_crest_dark | 9 | 1920x1080 | 0.168 | 0.092 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS28b_crest_dark_1610 | 9 | 1680x1050 | 0.169 | 0.093 | 0.0000 | - | PASS |  |
| b08_Ending_KS28b_crest_dark_219 | 9 | 2560x1080 | 0.165 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS28c_crest_zoom | 9 | 1920x1080 | 0.257 | 0.184 | 0.0000 | 0.500 | PASS |  |
| b08_Ending_KS28d_foot_zoom | 9 | 1920x1080 | 0.137 | 0.085 | 0.0031 | 0.500 | PASS |  |
| b08_Ending_KS29_lift | 9 | 1920x1080 | 0.184 | 0.142 | 0.0000 | 0.500 | PASS |  |
| b08_Ending_KS29_lift_1610 | 9 | 1680x1050 | 0.185 | 0.144 | 0.0000 | - | PASS |  |
| b08_Ending_KS29_lift_219 | 9 | 2560x1080 | 0.183 | 0.139 | 0.0000 | - | PASS |  |

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
| KS22_ride_drop | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KS23_ride_sunset | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KS24_ride_lookup | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KS25_ride_lanterns | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KS26a_transfer | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KS26_shuttle_back | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KS26b_platform_walk | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KS27_keeper | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KS28_dam_reveal | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS28b_crest_dark | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KS29_lift | 1680x1050, 1920x1080, 2560x1080 | INFO |
| KK1_arm_tower | - | INFO |
| KK2_head_eyes | - | INFO |
| KK4_arm_glass | - | INFO |

## Details

### b08_Ending_KS21b_cabin_door (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS21b_cabin_door.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2151 | 0.0784314..0.313725 | display |

### b08_Ending_KS21b_cabin_door_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS21b_cabin_door_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[432, 570], [432, 578]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2136 | 0.0784314..0.313725 | display |

### b08_Ending_KS21b_cabin_door_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS21b_cabin_door_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2285 | 0.0784314..0.313725 | display |

### b08_Ending_KS22_ride_drop (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS22_ride_drop.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 10796 px excluded |
| mean luma | PASS | 0.1798 | 0.0784314..0.313725 | display |
| banding[dusk_sky] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.04 px (limit 5.8), 15 strips |

### b08_Ending_KS22_ride_drop_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS22_ride_drop_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 10198 px excluded |
| mean luma | PASS | 0.1791 | 0.0784314..0.313725 | display |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.00 px (limit 5.3), 13 strips |

### b08_Ending_KS22_ride_drop_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS22_ride_drop_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 10796 px excluded |
| mean luma | PASS | 0.1832 | 0.0784314..0.313725 | display |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.05 px (limit 6.0), 20 strips |

### b08_Ending_KS22b_glass_floor (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS22b_glass_floor.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1308 | 0.0784314..0.313725 | display |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.00 px (limit 5.5), 15 strips |

### b08_Ending_KS22b_glass_floor_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS22b_glass_floor_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1302 | 0.0784314..0.313725 | display |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.00 px (limit 7.1), 13 strips |

### b08_Ending_KS22b_glass_floor_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS22b_glass_floor_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1271 | 0.0784314..0.313725 | display |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.07 px (limit 7.1), 20 strips |

### b08_Ending_KS23_ride_sunset (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS23_ride_sunset.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 4195 px excluded |
| mean luma | PASS | 0.2687 | 0.0784314..0.313725 | display |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.00 px (limit 5.4), 15 strips |

### b08_Ending_KS23_ride_sunset_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS23_ride_sunset_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 3964 px excluded |
| mean luma | PASS | 0.2702 | 0.0784314..0.313725 | display |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.03 px (limit 5.2), 13 strips |

### b08_Ending_KS23_ride_sunset_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS23_ride_sunset_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 4195 px excluded |
| mean luma | PASS | 0.2652 | 0.0784314..0.313725 | display |
| banding[dusk_sky] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.01 px (limit 6.0), 20 strips |

### b08_Ending_KS23b_face_rush (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS23b_face_rush.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1452 | 0.0784314..0.313725 | display |

### b08_Ending_KS23b_face_rush_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS23b_face_rush_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1451 | 0.0784314..0.313725 | display |

### b08_Ending_KS23b_face_rush_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS23b_face_rush_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 33 | 0 px | first at [[91, 0], [92, 2], [93, 4], [94, 6], [95, 8]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1427 | 0.0784314..0.313725 | display |

### b08_Ending_KS24_ride_lookup (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS24_ride_lookup.png`

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
| banding[face_gradient] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.03 px (limit 6.6), 5 strips |

### b08_Ending_KS24_ride_lookup_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS24_ride_lookup_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1898 | 0.0784314..0.313725 | display |
| banding[face_gradient] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.01 px (limit 10.5), 4 strips |

### b08_Ending_KS24_ride_lookup_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS24_ride_lookup_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1101, 381], [1061, 487], [1039, 549]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2067 | 0.0784314..0.313725 | display |
| banding[face_gradient] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.02 px (limit 7.4), 7 strips |

### b08_Ending_KS24b_ride_mist (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS24b_ride_mist.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 7727 px excluded |
| mean luma | PASS | 0.2371 | 0.0784314..0.313725 | display |
| banding[face_gradient] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.01 px (limit 9.6), 5 strips |

### b08_Ending_KS24b_ride_mist_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS24b_ride_mist_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 1 | 0 px | first at [[14, 203]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 7300 px excluded |
| mean luma | PASS | 0.2383 | 0.0784314..0.313725 | display |
| banding[face_gradient] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.01 px (limit 9.3), 4 strips |

### b08_Ending_KS24b_ride_mist_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS24b_ride_mist_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 1 | 0 px | first at [[2303, 232]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 7727 px excluded |
| mean luma | PASS | 0.2341 | 0.0784314..0.313725 | display |
| banding[face_gradient] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.01 px (limit 9.3), 7 strips |

### b08_Ending_KS25_ride_lanterns (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS25_ride_lanterns.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 11001 px excluded |
| mean luma | PASS | 0.1985 | 0.0784314..0.313725 | display |

### b08_Ending_KS25_ride_lanterns_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS25_ride_lanterns_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 10398 px excluded |
| mean luma | PASS | 0.1981 | 0.0784314..0.313725 | display |

### b08_Ending_KS25_ride_lanterns_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS25_ride_lanterns_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 11001 px excluded |
| mean luma | PASS | 0.1977 | 0.0784314..0.313725 | display |

### b08_Ending_KS26_shuttle_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS26_shuttle_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1892 | 0.0784314..0.313725 | display |

### b08_Ending_KS26_shuttle_back_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS26_shuttle_back_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1910 | 0.0784314..0.313725 | display |

### b08_Ending_KS26_shuttle_back_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS26_shuttle_back_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 55 | 0 px | first at [[2294, 760], [2328, 812], [2337, 826], [2341, 832], [2343, 835]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1855 | 0.0784314..0.313725 | display |

### b08_Ending_KS26a_transfer (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS26a_transfer.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[919, 161]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0911 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1554 | 0.0784314..0.313725 | display |

### b08_Ending_KS26a_transfer_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS26a_transfer_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1104, 97]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.1017 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1616 | 0.0784314..0.313725 | display |

### b08_Ending_KS26a_transfer_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS26a_transfer_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1413, 127], [1256, 157]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0685 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1488 | 0.0784314..0.313725 | display |

### b08_Ending_KS26b_platform_walk (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS26b_platform_walk.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0029 | <= 0.5 % | any channel >= 254; 5636 px excluded |
| mean luma | PASS | 0.2715 | 0.0784314..0.27451 | display |

### b08_Ending_KS26b_platform_walk_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS26b_platform_walk_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0036 | <= 0.5 % | any channel >= 254; 5328 px excluded |
| mean luma | FAIL | 0.2763 | 0.0784314..0.27451 | display |

### b08_Ending_KS26b_platform_walk_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS26b_platform_walk_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0022 | <= 0.5 % | any channel >= 254; 5636 px excluded |
| mean luma | PASS | 0.2603 | 0.0784314..0.27451 | display |

### b08_Ending_KS27_keeper (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS27_keeper.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0480 | <= 0.5 % | any channel >= 254; 1865 px excluded |
| mean luma | PASS | 0.1970 | 0.0784314..0.27451 | display |

### b08_Ending_KS27_keeper_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS27_keeper_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1402, 493]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0518 | <= 0.5 % | any channel >= 254; 1761 px excluded |
| mean luma | PASS | 0.2006 | 0.0784314..0.27451 | display |

### b08_Ending_KS27_keeper_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS27_keeper_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0359 | <= 0.5 % | any channel >= 254; 1865 px excluded |
| mean luma | PASS | 0.1903 | 0.0784314..0.27451 | display |

### b08_Ending_KS28_dam_reveal (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS28_dam_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1773 | 0.0980392..0.27451 | display |
| banding[alpenglow] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.12 px (limit 5.5), 11 strips |

### b08_Ending_KS28_dam_reveal_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS28_dam_reveal_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1788 | 0.0980392..0.27451 | display |

### b08_Ending_KS28_dam_reveal_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS28_dam_reveal_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1739 | 0.0980392..0.27451 | display |

### b08_Ending_KS28_dam_reveal_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS28_dam_reveal_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1769 | 0.0980392..0.27451 | display |
| banding[alpenglow] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.01 px (limit 6.5), 23 strips |

### b08_Ending_KS28b_crest_dark (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS28b_crest_dark.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1677 | 0.0784314..0.27451 | display |
| banding[alpenglow] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.01 px (limit 10.0), 11 strips |

### b08_Ending_KS28b_crest_dark_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS28b_crest_dark_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1692 | 0.0784314..0.27451 | display |

### b08_Ending_KS28b_crest_dark_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS28b_crest_dark_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1648 | 0.0784314..0.27451 | display |

### b08_Ending_KS28c_crest_zoom (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS28c_crest_zoom.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2569 | 0.0784314..0.27451 | display |
| banding[alpenglow] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.07 px (limit 44.7), 11 strips |

### b08_Ending_KS28d_foot_zoom (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS28d_foot_zoom.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0031 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1374 | 0.0784314..0.27451 | display |
| banding[alpenglow] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.00 px (limit 29.5), 11 strips |

### b08_Ending_KS29_lift (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS29_lift.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1845 | 0.0784314..0.27451 | display |
| banding[sky] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.00 px (limit 6.2), 11 strips |

### b08_Ending_KS29_lift_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS29_lift_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1853 | 0.0784314..0.27451 | display |

### b08_Ending_KS29_lift_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-ending\b08_Ending_KS29_lift_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1833 | 0.0784314..0.27451 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
