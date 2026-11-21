# Image metrics: leadqa-ending

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending`  
Scene constants: BulwarkConst.cs  
Images: 13, failing images: 1. Checks: 107 PASS, 1 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b08_Ending_KS27_keeper | 9 | 1920x1080 | 0.201 | 0.092 | 0.0488 | - | PASS |  |
| b08_Ending_KS27_keeper_1610 | 9 | 1680x1050 | 0.205 | 0.093 | 0.0526 | - | FAIL | NaN-like pixels |
| b08_Ending_KS27_keeper_219 | 9 | 2560x1080 | 0.195 | 0.092 | 0.0365 | - | PASS |  |
| b08_Ending_KS28_dam_reveal | 9 | 1920x1080 | 0.182 | 0.092 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS28_dam_reveal_1610 | 9 | 1680x1050 | 0.183 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS28_dam_reveal_219 | 9 | 2560x1080 | 0.180 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS28_dam_reveal_4k | 9 | 3840x2160 | 0.181 | 0.092 | 0.0000 | 0.250 | PASS |  |
| b08_Ending_KS28b_crest_dark | 9 | 1920x1080 | 0.174 | 0.092 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS28b_crest_dark_1610 | 9 | 1680x1050 | 0.176 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS28b_crest_dark_219 | 9 | 2560x1080 | 0.173 | 0.092 | 0.0000 | - | PASS |  |
| b08_Ending_KS29_lift | 9 | 1920x1080 | 0.191 | 0.150 | 0.0000 | 0.333 | PASS |  |
| b08_Ending_KS29_lift_1610 | 9 | 1680x1050 | 0.191 | 0.150 | 0.0000 | - | PASS |  |
| b08_Ending_KS29_lift_219 | 9 | 2560x1080 | 0.191 | 0.150 | 0.0000 | - | PASS |  |

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

## Details

### b08_Ending_KS27_keeper (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS27_keeper.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0488 | <= 0.5 % | any channel >= 254; 1865 px excluded |
| mean luma | PASS | 0.2015 | 0.0784314..0.27451 | display |

### b08_Ending_KS27_keeper_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS27_keeper_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1402, 493]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0526 | <= 0.5 % | any channel >= 254; 1761 px excluded |
| mean luma | PASS | 0.2050 | 0.0784314..0.27451 | display |

### b08_Ending_KS27_keeper_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS27_keeper_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0365 | <= 0.5 % | any channel >= 254; 1865 px excluded |
| mean luma | PASS | 0.1954 | 0.0784314..0.27451 | display |

### b08_Ending_KS28_dam_reveal (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS28_dam_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1817 | 0.0784314..0.27451 | display |
| banding[alpenglow] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.00 px (limit 5.1), 11 strips |

### b08_Ending_KS28_dam_reveal_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS28_dam_reveal_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1830 | 0.0784314..0.27451 | display |

### b08_Ending_KS28_dam_reveal_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS28_dam_reveal_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1798 | 0.0784314..0.27451 | display |

### b08_Ending_KS28_dam_reveal_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS28_dam_reveal_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1805 | 0.0784314..0.27451 | display |
| banding[alpenglow] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.00 px (limit 7.4), 23 strips |

### b08_Ending_KS28b_crest_dark (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS28b_crest_dark.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1745 | 0.0784314..0.27451 | display |
| banding[alpenglow] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.00 px (limit 10.9), 11 strips |

### b08_Ending_KS28b_crest_dark_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS28b_crest_dark_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1757 | 0.0784314..0.27451 | display |

### b08_Ending_KS28b_crest_dark_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS28b_crest_dark_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1728 | 0.0784314..0.27451 | display |

### b08_Ending_KS29_lift (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS29_lift.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1908 | 0.0784314..0.27451 | display |
| banding[sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.08 px (limit 9.0), 11 strips |

### b08_Ending_KS29_lift_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS29_lift_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1914 | 0.0784314..0.27451 | display |

### b08_Ending_KS29_lift_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\integ\QA\captures\leadqa-ending\b08_Ending_KS29_lift_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1906 | 0.0784314..0.27451 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
