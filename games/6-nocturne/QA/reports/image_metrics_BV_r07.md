# Image metrics: BV_r07

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07`  
Scene constants: NocturneConstants.cs  
Images: 11, failing images: 10. Checks: 79 PASS, 16 FAIL, 0 WARN, 11 SKIP, 7 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS03_boulevard_mouth | 1 | 1920x1080 | 0.099 | 0.059 | 0.0000 | 0.102 | FAIL | NaN-like pixels |
| KS04_boulevard_axis | 2 | 1920x1080 | 0.143 | 0.060 | 0.0122 | 0.609 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up | 2 | 1920x1080 | 0.169 | 0.067 | 0.0011 | 0.478 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS06_tram | 2 | 1920x1080 | 0.138 | 0.062 | 0.0015 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_end | 2 | 1920x1080 | 0.129 | 0.075 | 0.0067 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_look_back | 2 | 1920x1080 | 0.132 | 0.061 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_look_l | 2 | 1920x1080 | 0.130 | 0.060 | 0.0000 | - | FAIL | NaN-like pixels |
| beat02_boulevard_look_r | 2 | 1920x1080 | 0.123 | 0.056 | 0.0000 | - | FAIL | NaN-like pixels |
| beat02_boulevard_look_up60 | 2 | 1920x1080 | 0.179 | 0.066 | 0.0000 | - | PASS |  |
| beat02_boulevard_mid | 2 | 1920x1080 | 0.138 | 0.061 | 0.0047 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_start | 2 | 1920x1080 | 0.109 | 0.059 | 0.0000 | - | FAIL | isolated pixels > 8x median |

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

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\KS03_boulevard_mouth.png`

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
| mean luma | PASS | 0.0995 | 0.0549..0.1569 | display |
| horizon[default] | SKIP | 756.3000 |  | sky band not uniform (spread 121.0 codes): objects on the horizon |
| banding[alley_wall_dark] | PASS | 0.1020 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.15 (run 7 px), - / -, 2 strips |

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 32 | 0 | bright 6 (mode all), dark 26, first at [[904, 569], [882, 619], [572, 711], [556, 902], [1413, 904]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[513, 901], [513, 902], [1787, 945], [1057, 960]]; neon mask 2.761 % of the frame |
| clipped highlights | PASS | 0.0122 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0600 | >= 0.0353 | display |
| mean luma | PASS | 0.1435 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 963.7000 |  | sky band not uniform (spread 150.5 codes): objects on the horizon |
| banding[sky_above_facades] | PASS | 0.6090 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.91 (run 7 px), - / -, 3 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 5 (mode all), dark 1, first at [[1485, 158], [883, 815], [887, 857], [1431, 1008], [877, 1074]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[862, 977]]; neon mask 2.840 % of the frame |
| clipped highlights | PASS | 0.0011 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1692 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |
| banding[cloud_ceiling] | PASS | 0.4780 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.72 (run 9 px), - / -, 4 strips |

### KS06_tram (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\KS06_tram.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 12 | 0 | bright 4 (mode all), dark 8, first at [[1830, 127], [1061, 536], [1409, 725], [541, 738], [630, 746]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[339, 680], [555, 689]]; neon mask 2.443 % of the frame |
| clipped highlights | PASS | 0.0015 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0616 | >= 0.0353 | display |
| mean luma | PASS | 0.1382 | 0.0941..0.2118 | display |
| horizon[default] | SKIP | 747.3000 |  | sky band not uniform (spread 117.0 codes): objects on the horizon |

### beat02_boulevard_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\beat02_boulevard_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[1809, 126], [863, 186], [864, 228], [881, 229], [1881, 232]] |
| isolated pixels > 8x median | FAIL | 7 | 0 outside neon masks | first at [[865, 193], [865, 194], [881, 229], [456, 246], [457, 246]]; neon mask 3.994 % of the frame |
| clipped highlights | PASS | 0.0067 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0745 | >= 0.0353 | display |
| mean luma | INFO | 0.1290 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 72.0 codes): objects on the horizon |

### beat02_boulevard_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\beat02_boulevard_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 4 (mode all), dark 1, first at [[1027, 112], [1049, 211], [1058, 219], [1461, 520], [362, 533]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[238, 549]]; neon mask 0.581 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0611 | >= 0.0353 | display |
| mean luma | INFO | 0.1319 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 32.0 codes): objects on the horizon |

### beat02_boulevard_look_l (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\beat02_boulevard_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 0 (mode all), dark 2, first at [[1756, 385], [1478, 530]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.307 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0597 | >= 0.0353 | display |
| mean luma | INFO | 0.1295 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 127.0 codes): objects on the horizon |

### beat02_boulevard_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\beat02_boulevard_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 0 (mode all), dark 4, first at [[163, 412], [600, 530], [820, 530], [946, 530]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.321 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0560 | >= 0.0353 | display |
| mean luma | INFO | 0.1228 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 152.5 codes): objects on the horizon |

### beat02_boulevard_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\beat02_boulevard_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.006 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0658 | >= 0.0353 | display |
| mean luma | INFO | 0.1787 |  | display |
| horizon[default] | SKIP | - |  | line not in frame or no pitch/fov |

### beat02_boulevard_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\beat02_boulevard_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 3 (mode all), dark 1, first at [[893, 117], [893, 276], [1560, 533], [955, 534]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[866, 197], [444, 523], [1781, 523], [543, 526]]; neon mask 2.474 % of the frame |
| clipped highlights | PASS | 0.0047 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0614 | >= 0.0353 | display |
| mean luma | INFO | 0.1376 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 168.5 codes): objects on the horizon |

### beat02_boulevard_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\blvd\QA\captures\BV_r07\beat02_boulevard_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[391, 535], [378, 547]]; neon mask 1.043 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | INFO | 0.1086 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 125.0 codes): objects on the horizon |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
