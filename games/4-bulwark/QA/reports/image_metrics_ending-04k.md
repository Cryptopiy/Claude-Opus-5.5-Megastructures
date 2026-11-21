# Image metrics: ending-04k

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k`  
Scene constants: BulwarkConst.cs  
Images: 12, failing images: 9. Checks: 91 PASS, 9 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b07_SluiceLedge_KS19_sluice | 7 | 1920x1080 | 0.147 | 0.024 | 0.0007 | - | FAIL | fireflies |
| b07_SluiceLedge_KS19_sluice_1610 | 7 | 1680x1050 | 0.145 | 0.024 | 0.0006 | - | FAIL | fireflies |
| b07_SluiceLedge_KS19_sluice_219 | 7 | 2560x1080 | 0.171 | 0.025 | 0.0005 | - | FAIL | fireflies |
| b07_SluiceLedge_KS19_sluice_4k | 7 | 3840x2160 | 0.147 | 0.024 | 0.0008 | - | FAIL | fireflies |
| b07_SluiceLedge_KS20_ledge_world | 7 | 1920x1080 | 0.285 | 0.069 | 0.0000 | 0.333 | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_1610 | 7 | 1680x1050 | 0.287 | 0.069 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_219 | 7 | 2560x1080 | 0.282 | 0.070 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_4k | 7 | 3840x2160 | 0.285 | 0.069 | 0.0000 | 0.250 | FAIL | fireflies |
| b07_SluiceLedge_KS21_beacon | 7 | 1920x1080 | 0.253 | 0.027 | 0.0005 | - | FAIL | fireflies |
| b07_SluiceLedge_KS21_beacon_1610 | 7 | 1680x1050 | 0.248 | 0.027 | 0.0008 | - | FAIL | fireflies |
| b07_SluiceLedge_KS21_beacon_219 | 7 | 2560x1080 | 0.268 | 0.030 | 0.0004 | - | FAIL | fireflies |
| b07_SluiceLedge_KS21_beacon_4k | 7 | 3840x2160 | 0.253 | 0.027 | 0.0005 | - | FAIL | fireflies |

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
| KS19_sluice | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS20_ledge_world | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS21_beacon | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |

## Details

### b07_SluiceLedge_KS19_sluice (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS19_sluice.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 747 | 0 px | first at [[880, 645], [881, 645], [857, 646], [880, 646], [881, 646]] |
| clipped highlights | PASS | 0.0007 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1474 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS19_sluice_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 872 | 0 px | first at [[786, 626], [786, 627], [787, 627], [740, 628], [786, 628]] |
| clipped highlights | PASS | 0.0006 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1447 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS19_sluice_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 742 | 0 px | first at [[1200, 645], [1201, 645], [1177, 646], [1200, 646], [1201, 646]] |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1706 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS19_sluice_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 222 | 0 px | first at [[1670, 1297], [1670, 1298], [1670, 1325], [1670, 1326], [1670, 1327]] |
| clipped highlights | PASS | 0.0008 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1471 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS20_ledge_world.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 6014 px excluded |
| mean luma | PASS | 0.2850 | 0.117647..0.352941 | display |
| horizon[lowland_horizon] step | PASS | 0.2610 | <= 2 dE | trend-removed at row ~474.0, 1094 cols: above [207.0, 141.7, 110.6] vs below [207.8, 142.2, 111.0] |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.02 px (limit 7.1), 10 strips |

### b07_SluiceLedge_KS20_ledge_world_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS20_ledge_world_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 5688 px excluded |
| mean luma | PASS | 0.2867 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS20_ledge_world_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 6014 px excluded |
| mean luma | PASS | 0.2820 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS20_ledge_world_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 6 | 0 px | first at [[2753, 902], [2753, 903], [2753, 906], [2752, 919], [2752, 920]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 24075 px excluded |
| mean luma | PASS | 0.2848 | 0.117647..0.352941 | display |
| horizon[lowland_horizon] step | PASS | 0.2970 | <= 2 dE | trend-removed at row ~948.0, 2188 cols: above [207.0, 141.6, 110.8] vs below [207.9, 142.2, 111.1] |
| banding[dusk_sky] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.02 px (limit 6.9), 20 strips |

### b07_SluiceLedge_KS21_beacon (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS21_beacon.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 2 | 0 px | first at [[618, 612], [618, 613]] |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2533 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS21_beacon_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 3 | 0 px | first at [[508, 595], [508, 661], [508, 662]] |
| clipped highlights | PASS | 0.0008 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2483 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS21_beacon_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 2 | 0 px | first at [[938, 612], [938, 613]] |
| clipped highlights | PASS | 0.0004 | <= 1 % | any channel >= 254; 20764 px excluded |
| mean luma | PASS | 0.2679 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\ending\QA\captures\ending-04k\b07_SluiceLedge_KS21_beacon_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 2 | 0 px | first at [[1807, 1256], [1711, 1433]] |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2529 | 0.078431..0.352941 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
