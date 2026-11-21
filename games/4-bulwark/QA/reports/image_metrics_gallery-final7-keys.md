# Image metrics: gallery-final7-keys

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys`  
Scene constants: BulwarkConst.cs  
Images: 16, failing images: 0. Checks: 128 PASS, 0 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: PASS**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b06_WindowGallery_KS15_win_first | 6 | 1920x1080 | 0.280 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_1610 | 6 | 1680x1050 | 0.290 | 0.099 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_219 | 6 | 2560x1080 | 0.225 | 0.030 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_4k | 6 | 3840x2160 | 0.280 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up | 6 | 1920x1080 | 0.185 | 0.034 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_1610 | 6 | 1680x1050 | 0.197 | 0.035 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_219 | 6 | 2560x1080 | 0.157 | 0.034 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_4k | 6 | 3840x2160 | 0.185 | 0.034 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale | 6 | 1920x1080 | 0.174 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale_1610 | 6 | 1680x1050 | 0.180 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale_219 | 6 | 2560x1080 | 0.161 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_whale_4k | 6 | 3840x2160 | 0.174 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down | 6 | 1920x1080 | 0.114 | 0.046 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_1610 | 6 | 1680x1050 | 0.113 | 0.046 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_219 | 6 | 2560x1080 | 0.116 | 0.042 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_4k | 6 | 3840x2160 | 0.114 | 0.046 | 0.0000 | - | PASS |  |

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
| KS15_win_first | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS16_win_up | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS17_win_whale | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS18_win_down | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS19_sluice | - | INFO |
| KS20_ledge_world | - | INFO |
| KS21_beacon | - | INFO |

## Details

### b06_WindowGallery_KS15_win_first (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS15_win_first.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2796 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS15_win_first_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2903 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS15_win_first_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2248 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS15_win_first_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2798 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS16_win_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1849 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS16_win_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1968 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS16_win_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1571 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS16_win_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1850 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_whale (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS17_win_whale.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1740 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_whale_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS17_win_whale_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1797 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_whale_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS17_win_whale_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1613 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_whale_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS17_win_whale_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1741 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS18_win_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1140 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS18_win_down_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1134 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS18_win_down_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1157 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\.worktrees\gallery\QA\captures\gallery-final7-keys\b06_WindowGallery_KS18_win_down_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1141 | 0.054902..0.333333 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
