# Image metrics: mistx-22

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22`  
Scene constants: BulwarkConst.cs  
Images: 24, failing images: 10. Checks: 184 PASS, 10 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b02_Crest_end | 2 | 1920x1080 | 0.384 | 0.194 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS07_spill_gates | 3 | 1920x1080 | 0.353 | 0.173 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS07_spill_gates_1610 | 3 | 1680x1050 | 0.354 | 0.173 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS07_spill_gates_219 | 3 | 2560x1080 | 0.343 | 0.173 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS07_spill_gates_4k | 3 | 3840x2160 | 0.353 | 0.173 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS08_spill_bow | 3 | 1920x1080 | 0.476 | 0.229 | 0.0000 | 0.500 | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_1610 | 3 | 1680x1050 | 0.480 | 0.229 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_219 | 3 | 2560x1080 | 0.467 | 0.226 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_4k | 3 | 3840x2160 | 0.476 | 0.229 | 0.0000 | 0.250 | PASS |  |
| b03_SpillwayBridge_KS09_spill_grate | 3 | 1920x1080 | 0.380 | 0.146 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS09_spill_grate_1610 | 3 | 1680x1050 | 0.382 | 0.146 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS09_spill_grate_219 | 3 | 2560x1080 | 0.368 | 0.134 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS09_spill_grate_4k | 3 | 3840x2160 | 0.379 | 0.146 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_end | 3 | 1920x1080 | 0.426 | 0.248 | 0.0001 | - | FAIL | NaN-like pixels |
| b03_SpillwayBridge_look_back | 3 | 1920x1080 | 0.417 | 0.181 | 0.0000 | - | FAIL | NaN-like pixels |
| b03_SpillwayBridge_mid | 3 | 1920x1080 | 0.405 | 0.195 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS11_reef_spray | 4 | 1920x1080 | 0.510 | 0.170 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_1610 | 4 | 1680x1050 | 0.513 | 0.166 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_219 | 4 | 2560x1080 | 0.497 | 0.146 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS11_reef_spray_4k | 4 | 3840x2160 | 0.510 | 0.168 | 0.0000 | - | PASS |  |
| show_k3_ks07 | 3 | 1920x1080 | 0.353 | 0.173 | 0.0000 | - | FAIL | mean luma |
| show_k5_ks11 | 4 | 1920x1080 | 0.510 | 0.170 | 0.0000 | - | PASS |  |
| show_k5_s821 | 4 | 1920x1080 | 0.489 | 0.153 | 0.0000 | - | PASS |  |
| show_k5_s828 | 4 | 1920x1080 | 0.498 | 0.150 | 0.0000 | - | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | - | INFO |
| KS02_gal_slit | - | INFO |
| KS03_door_open | - | INFO |
| KS04_crest_sea | - | INFO |
| KS05_crest_drop | - | INFO |
| KS06_crest_axis | - | INFO |
| KS07_spill_gates | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS08_spill_bow | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS09_spill_grate | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS10_reef_wide | - | INFO |
| KS11_reef_spray | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS12_tower_up | - | INFO |
| KS13_atrium | - | INFO |
| KS14_stairs_down | - | INFO |
| KS15_win_first | - | INFO |
| KS16_win_up | - | INFO |
| KS18_win_down | - | INFO |
| KS19_sluice | - | INFO |
| KS20_ledge_world | - | INFO |
| KS21_beacon | - | INFO |

## Details

### b02_Crest_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b02_Crest_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3845 | 0.333333..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS07_spill_gates.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3531 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS07_spill_gates_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3539 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS07_spill_gates_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3426 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS07_spill_gates_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3532 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS08_spill_bow.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4756 | 0.372549..0.588235 | display |
| banding[sky] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.02 px (limit 11.1), 15 strips |

### b03_SpillwayBridge_KS08_spill_bow_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS08_spill_bow_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4798 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS08_spill_bow_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4670 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS08_spill_bow_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4756 | 0.372549..0.588235 | display |
| banding[sky] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.04 px (limit 20.5), 30 strips |

### b03_SpillwayBridge_KS09_spill_grate (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS09_spill_grate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3796 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS09_spill_grate_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS09_spill_grate_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3823 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS09_spill_grate_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS09_spill_grate_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3675 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS09_spill_grate_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_KS09_spill_grate_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3792 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[975, 525]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4262 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_back (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[651, 592]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4167 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_mid (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b03_SpillwayBridge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[959, 519]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4053 | 0.372549..0.588235 | display |

### b04_ArmourSlope_KS11_reef_spray (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b04_ArmourSlope_KS11_reef_spray.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5097 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b04_ArmourSlope_KS11_reef_spray_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5129 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b04_ArmourSlope_KS11_reef_spray_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[2289, 465]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4975 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\b04_ArmourSlope_KS11_reef_spray_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5101 | 0.392157..0.647059 | display |

### show_k3_ks07 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\show_k3_ks07.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3531 | 0.372549..0.588235 | display |

### show_k5_ks11 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\show_k5_ks11.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5097 | 0.392157..0.647059 | display |

### show_k5_s821 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\show_k5_s821.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4887 | 0.392157..0.647059 | display |

### show_k5_s828 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\seamist\QA\captures\mistx-22\show_k5_s828.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4976 | 0.392157..0.647059 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
