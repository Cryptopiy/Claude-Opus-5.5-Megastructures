# Image metrics: ending-08

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08`  
Scene constants: BulwarkConst.cs  
Images: 16, failing images: 4. Checks: 124 PASS, 4 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b08_Ending_KS21b_cabin_door | 7 | 1920x1080 | 0.219 | 0.026 | 0.0000 | - | FAIL | fireflies |
| b08_Ending_KS22_ride_drop | 7 | 1920x1080 | 0.175 | 0.041 | 0.0000 | - | PASS |  |
| b08_Ending_KS22b_glass_floor | 7 | 1920x1080 | 0.131 | 0.041 | 0.0000 | - | PASS |  |
| b08_Ending_KS23_ride_sunset | 7 | 1920x1080 | 0.267 | 0.041 | 0.0000 | - | PASS |  |
| b08_Ending_KS23b_face_rush | 7 | 1920x1080 | 0.144 | 0.036 | 0.0000 | - | PASS |  |
| b08_Ending_KS24_ride_lookup | 7 | 1920x1080 | 0.192 | 0.031 | 0.0000 | - | FAIL | fireflies |
| b08_Ending_KS24b_ride_mist | 7 | 1920x1080 | 0.240 | 0.051 | 0.0000 | - | PASS |  |
| b08_Ending_KS25_ride_lanterns | 7 | 1920x1080 | 0.202 | 0.063 | 0.0000 | - | PASS |  |
| b08_Ending_KS26_shuttle_back | 7 | 1920x1080 | 0.173 | 0.074 | 0.0000 | - | PASS |  |
| b08_Ending_KS26a_transfer | 7 | 1920x1080 | 0.153 | 0.031 | 0.0914 | - | FAIL | NaN-like pixels |
| b08_Ending_KS26b_platform_walk | 7 | 1920x1080 | 0.270 | 0.056 | 0.0029 | - | FAIL | NaN-like pixels |
| b08_Ending_KS27_keeper | 7 | 1920x1080 | 0.199 | 0.055 | 0.0504 | - | PASS |  |
| b08_Ending_KS28_dam_reveal | 7 | 1920x1080 | 0.182 | 0.056 | 0.0000 | - | PASS |  |
| b08_Ending_KS28_dam_reveal_219 | 7 | 2520x1080 | 0.178 | 0.056 | 0.0000 | - | PASS |  |
| b08_Ending_KS28_dam_reveal_4k | 7 | 3840x2160 | 0.181 | 0.056 | 0.0000 | - | PASS |  |
| b08_Ending_KS28b_crest_dark | 7 | 1920x1080 | 0.174 | 0.056 | 0.0000 | - | PASS |  |

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

### b08_Ending_KS21b_cabin_door (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS21b_cabin_door.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 2 | 0 px | first at [[859, 665], [859, 666]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2193 | 0.0784314..0.352941 | display |

### b08_Ending_KS22_ride_drop (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS22_ride_drop.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1748 | 0.0784314..0.352941 | display |

### b08_Ending_KS22b_glass_floor (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS22b_glass_floor.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1314 | 0.0784314..0.352941 | display |

### b08_Ending_KS23_ride_sunset (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS23_ride_sunset.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2666 | 0.0784314..0.352941 | display |

### b08_Ending_KS23b_face_rush (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS23b_face_rush.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1440 | 0.0784314..0.352941 | display |

### b08_Ending_KS24_ride_lookup (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS24_ride_lookup.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 1 | 0 px | first at [[1865, 643]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1925 | 0.0784314..0.352941 | display |

### b08_Ending_KS24b_ride_mist (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS24b_ride_mist.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2399 | 0.0784314..0.352941 | display |

### b08_Ending_KS25_ride_lanterns (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS25_ride_lanterns.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2017 | 0.0784314..0.352941 | display |

### b08_Ending_KS26_shuttle_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS26_shuttle_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1726 | 0.0784314..0.352941 | display |

### b08_Ending_KS26a_transfer (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS26a_transfer.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[919, 161]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0914 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1530 | 0.0784314..0.352941 | display |

### b08_Ending_KS26b_platform_walk (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS26b_platform_walk.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1911, 584]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0029 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2703 | 0.0784314..0.352941 | display |

### b08_Ending_KS27_keeper (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS27_keeper.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0504 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1989 | 0.0784314..0.352941 | display |

### b08_Ending_KS28_dam_reveal (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS28_dam_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1818 | 0.0784314..0.352941 | display |

### b08_Ending_KS28_dam_reveal_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS28_dam_reveal_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2520x1080 | 2520x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1776 | 0.0784314..0.352941 | display |

### b08_Ending_KS28_dam_reveal_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS28_dam_reveal_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1808 | 0.0784314..0.352941 | display |

### b08_Ending_KS28b_crest_dark (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-08\b08_Ending_KS28b_crest_dark.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1744 | 0.0784314..0.352941 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
