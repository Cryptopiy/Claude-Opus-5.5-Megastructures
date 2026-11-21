# Image metrics: x2-gallery

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery`  
Scene constants: BulwarkConst.cs  
Images: 20, failing images: 1. Checks: 159 PASS, 1 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| pose_a1146 | 6 | 1920x1080 | 0.208 | 0.031 | 0.0000 | - | PASS |  |
| pose_a1180 | 6 | 1920x1080 | 0.208 | 0.035 | 0.0000 | - | PASS |  |
| pose_entry | 6 | 1920x1080 | 0.198 | 0.050 | 0.0000 | - | PASS |  |
| pose_kk4 | 6 | 1920x1080 | 0.180 | 0.038 | 0.0000 | - | PASS |  |
| pose_ks15 | 6 | 1920x1080 | 0.280 | 0.038 | 0.0000 | - | PASS |  |
| pose_ks16 | 6 | 1920x1080 | 0.171 | 0.038 | 0.0000 | - | PASS |  |
| pose_ks17a1 | 6 | 1920x1080 | 0.170 | 0.038 | 0.0000 | - | PASS |  |
| pose_ks18 | 6 | 1920x1080 | 0.126 | 0.046 | 0.0000 | - | PASS |  |
| pose_p1212 | 6 | 1920x1080 | 0.146 | 0.038 | 0.0000 | - | PASS |  |
| pose_p1225 | 6 | 1920x1080 | 0.168 | 0.035 | 0.0000 | - | PASS |  |
| pose_p1236 | 6 | 1920x1080 | 0.141 | 0.041 | 0.0000 | - | PASS |  |
| pose_p1236up | 6 | 1920x1080 | 0.179 | 0.038 | 0.0000 | - | PASS |  |
| pose_p1256 | 6 | 1920x1080 | 0.172 | 0.038 | 0.0000 | - | PASS |  |
| pose_p1262 | 6 | 1920x1080 | 0.176 | 0.038 | 0.0000 | - | PASS |  |
| pose_p1276 | 6 | 1920x1080 | 0.145 | 0.050 | 0.0000 | - | PASS |  |
| pose_p1290 | 6 | 1920x1080 | 0.133 | 0.046 | 0.0000 | - | PASS |  |
| pose_p1345 | 6 | 1920x1080 | 0.105 | 0.037 | 0.0000 | - | PASS |  |
| pose_w1085p40 | 6 | 1920x1080 | 0.365 | 0.038 | 0.0000 | - | FAIL | mean luma |
| pose_w1200p50 | 6 | 1920x1080 | 0.240 | 0.034 | 0.0000 | - | PASS |  |
| pose_w1355p40 | 6 | 1920x1080 | 0.219 | 0.042 | 0.0000 | - | PASS |  |

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

### pose_a1146 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_a1146.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2079 | 0.054902..0.333333 | display |

### pose_a1180 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_a1180.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2077 | 0.054902..0.333333 | display |

### pose_entry (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_entry.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1983 | 0.054902..0.333333 | display |

### pose_kk4 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_kk4.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1805 | 0.054902..0.333333 | display |

### pose_ks15 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_ks15.png`

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

### pose_ks16 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_ks16.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1711 | 0.054902..0.333333 | display |

### pose_ks17a1 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_ks17a1.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1696 | 0.054902..0.333333 | display |

### pose_ks18 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_ks18.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1263 | 0.054902..0.333333 | display |

### pose_p1212 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_p1212.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1457 | 0.054902..0.333333 | display |

### pose_p1225 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_p1225.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1682 | 0.054902..0.333333 | display |

### pose_p1236 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_p1236.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1410 | 0.054902..0.333333 | display |

### pose_p1236up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_p1236up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1790 | 0.054902..0.333333 | display |

### pose_p1256 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_p1256.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1715 | 0.054902..0.333333 | display |

### pose_p1262 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_p1262.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1765 | 0.054902..0.333333 | display |

### pose_p1276 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_p1276.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1446 | 0.054902..0.333333 | display |

### pose_p1290 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_p1290.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1325 | 0.054902..0.333333 | display |

### pose_p1345 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_p1345.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1054 | 0.054902..0.333333 | display |

### pose_w1085p40 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_w1085p40.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3648 | 0.054902..0.333333 | display |

### pose_w1200p50 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_w1200p50.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2401 | 0.054902..0.333333 | display |

### pose_w1355p40 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-gallery\pose_w1355p40.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2190 | 0.054902..0.333333 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
