# Image metrics: wa-fixes

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-fixes`  
Scene constants: BulwarkConst.cs  
Images: 4, failing images: 0. Checks: 32 PASS, 0 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: PASS**

## Input warnings

- regions file <WORKSPACE>\4 Megastructures\4 Megastructures\Tools\qa\regions_default.json missing or empty

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_WaveGallery_KS01_gal_far | 1 | 1920x1080 | 0.187 | 0.117 | 0.0008 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_1610 | 1 | 1680x1050 | 0.192 | 0.117 | 0.0009 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_219 | 1 | 2560x1080 | 0.174 | 0.113 | 0.0006 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_4k | 1 | 3840x2160 | 0.187 | 0.116 | 0.0008 | - | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
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

### b01_WaveGallery_KS01_gal_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-fixes\b01_WaveGallery_KS01_gal_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1874 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-fixes\b01_WaveGallery_KS01_gal_far_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0009 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1916 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-fixes\b01_WaveGallery_KS01_gal_far_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1742 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\wa-fixes\b01_WaveGallery_KS01_gal_far_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1873 | 0.109804..0.27451 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
