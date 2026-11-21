# Image metrics: int-02

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-02`  
Scene constants: BulwarkConst.cs (ERRORS: KeyShot KS03_door_open: could not convert string to float: 'L(KS03LumaMin)')  
Images: 4, failing images: 4. Checks: 23 PASS, 9 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b05_GateTower_KS13_atrium | 5 | 1920x1080 | 0.102 | 0.059 | 0.0000 | - | FAIL | NaN-like pixels, fireflies, mean luma |
| b05_GateTower_KS13_atrium_1610 | 5 | 1680x1050 | 0.103 | 0.059 | 0.0000 | - | FAIL | mean luma |
| b05_GateTower_KS13_atrium_219 | 5 | 2560x1080 | 0.101 | 0.058 | 0.0000 | - | FAIL | NaN-like pixels, fireflies, mean luma |
| b05_GateTower_KS13_atrium_4k | 5 | 3840x2160 | 0.102 | 0.058 | 0.0000 | - | FAIL | fireflies, mean luma |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | - | INFO |
| KS02_gal_slit | - | INFO |
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
| KS14_stairs_down | - | INFO |
| KS15_win_first | - | INFO |
| KS16_win_up | - | INFO |
| KS17_win_whale | - | INFO |
| KS18_win_down | - | INFO |
| KS19_sluice | - | INFO |
| KS20_ledge_world | - | INFO |
| KS21_beacon | - | INFO |

## Details

### b05_GateTower_KS13_atrium (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-02\b05_GateTower_KS13_atrium.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[318, 1054]] |
| fireflies | FAIL | 20 | 0 px | first at [[1057, 927], [1058, 927], [1059, 927], [1060, 927], [1061, 927]] |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1017 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-02\b05_GateTower_KS13_atrium_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1026 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-02\b05_GateTower_KS13_atrium_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[638, 1054]] |
| fireflies | FAIL | 22 | 0 px | first at [[1377, 927], [1378, 927], [1379, 927], [1380, 927], [1381, 927]] |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1009 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\int-02\b05_GateTower_KS13_atrium_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 57 | 0 px | first at [[2098, 1855], [2099, 1855], [2100, 1855], [2101, 1855], [2102, 1855]] |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.1017 | 0.109804..0.313725 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
