# Image metrics: BV_r04

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04`  
Scene constants: NocturneConstants.cs  
Images: 11, failing images: 10. Checks: 79 PASS, 16 FAIL, 0 WARN, 11 SKIP, 7 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.099 | 0.059 | 0.0000 | 0.105 | FAIL | NaN-like pixels |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.139 | 0.060 | 0.0015 | 0.459 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up | 2 | 1920x1080 | 0.167 | 0.066 | 0.0001 | 0.364 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram | 2 | 1920x1080 | 0.133 | 0.062 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_end | 2 | 1920x1080 | 0.123 | 0.074 | 0.0018 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_look_back | 2 | 1920x1080 | 0.131 | 0.061 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_look_l | 2 | 1920x1080 | 0.128 | 0.060 | 0.0000 | - | FAIL | NaN-like pixels |
| beat02_boulevard_look_r | 2 | 1920x1080 | 0.123 | 0.056 | 0.0000 | - | FAIL | NaN-like pixels |
| beat02_boulevard_look_up60 | 2 | 1920x1080 | 0.177 | 0.066 | 0.0000 | - | PASS |  |
| beat02_boulevard_mid | 2 | 1920x1080 | 0.133 | 0.061 | 0.0010 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_start | 2 | 1920x1080 | 0.108 | 0.059 | 0.0000 | - | FAIL | isolated pixels > 8x median |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS00_alley_start | - | INFO |
| KS01_alley_noodle | - | INFO |
| KS02_alley_slit | - | INFO |
| KS03_boulevard_mouth | 1920x1080 | INFO |
| KS04_boulevard_axis | 1920x1080 | INFO |
| KS06_tram | 1920x1080 | INFO |
| KS05_boulevard_up | 1920x1080 | INFO |
| KS07_edge_window | - | INFO |
| KS08_edge_flank | - | INFO |
| KS09_edge_gulf | - | INFO |
| KS10_edge_plunge | - | INFO |
| KS11_bridge_train | - | INFO |
| KS12_bridge_void | - | INFO |
| KS13_bridge_glass | - | INFO |
| KS14_garden_gate | - | INFO |
| KS15_garden_canopy | - | INFO |
| KS16_signs_wall | - | INFO |
| KS17_heart_reveal | - | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### KS03_boulevard_mouth (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\KS03_boulevard_mouth.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1669, 755]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.889 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.0987 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 121.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.1050 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.16 (run 7 px), - / -, 2 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 25 | 0 | bright 7 (mode all), dark 18, first at [[870, 521], [881, 540], [883, 573], [131, 619], [889, 628]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1809, 811], [1810, 814]]; neon mask 2.466 % of the frame |
| clipped highlights | PASS | 0.0015 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0597 | >= 0.0353 | display |
| mean luma | PASS | 0.1388 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 876.0000 |  | sky band not uniform (spread 113.0 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.4590 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.69 (run 7 px), - / -, 3 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 5 (mode all), dark 1, first at [[1485, 158], [883, 815], [887, 857], [1431, 1008], [877, 1074]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[862, 977]]; neon mask 2.466 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0664 | >= 0.0353 | display |
| mean luma | PASS | 0.1673 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.3640 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.55 (run 8 px), - / -, 4 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 10 | 0 | bright 3 (mode all), dark 7, first at [[1409, 725], [541, 738], [630, 746], [540, 747], [404, 751]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[339, 680], [555, 689]]; neon mask 2.165 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1330 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 116.5 codes): objects on the horizon |

### beat02_boulevard_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\beat02_boulevard_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 8 | 0 | bright 8 (mode all), dark 0, first at [[863, 186], [864, 228], [881, 229], [1881, 232], [793, 547]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[865, 193], [865, 194], [881, 229]]; neon mask 3.179 % of the frame |
| clipped highlights | PASS | 0.0018 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0737 | >= 0.0353 | display |
| mean luma | INFO | 0.1235 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 72.0 codes): objects on the horizon |

### beat02_boulevard_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\beat02_boulevard_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 4 (mode all), dark 1, first at [[1027, 112], [1049, 211], [1058, 219], [1461, 520], [362, 533]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[238, 549]]; neon mask 0.585 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0614 | >= 0.0353 | display |
| mean luma | INFO | 0.1312 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 32.0 codes): objects on the horizon |

### beat02_boulevard_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\beat02_boulevard_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[1756, 385], [1478, 530]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.314 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0597 | >= 0.0353 | display |
| mean luma | INFO | 0.1278 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 126.0 codes): objects on the horizon |

### beat02_boulevard_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\beat02_boulevard_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 0 (mode all), dark 4, first at [[163, 412], [600, 530], [820, 530], [946, 530]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.319 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0560 | >= 0.0353 | display |
| mean luma | INFO | 0.1231 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 152.0 codes): objects on the horizon |

### beat02_boulevard_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\beat02_boulevard_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.840 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0658 | >= 0.0353 | display |
| mean luma | INFO | 0.1767 |  | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |

### beat02_boulevard_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\beat02_boulevard_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 3 (mode all), dark 1, first at [[893, 117], [893, 276], [1560, 533], [955, 534]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[866, 197], [1781, 523]]; neon mask 2.221 % of the frame |
| clipped highlights | PASS | 0.0010 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0614 | >= 0.0353 | display |
| mean luma | INFO | 0.1333 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 44.0 codes): objects on the horizon |

### beat02_boulevard_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r04\beat02_boulevard_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[391, 535], [378, 547]]; neon mask 1.038 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0588 | >= 0.0353 | display |
| mean luma | INFO | 0.1078 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 125.0 codes): objects on the horizon |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
