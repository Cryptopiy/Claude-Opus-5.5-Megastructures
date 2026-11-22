# Image metrics: AB_r07

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r07`  
Scene constants: NocturneConstants.cs  
Images: 7, failing images: 7. Checks: 56 PASS, 11 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS07_edge_window | 3 | 1920x1080 | 0.127 | 0.074 | 0.0054 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | 3 | 1920x1080 | 0.153 | 0.062 | 0.0065 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS09_edge_gulf | 3 | 1920x1080 | 0.135 | 0.063 | 0.0001 | 0.355 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS10_edge_plunge | 3 | 1920x1080 | 0.131 | 0.059 | 0.0048 | 0.806 | FAIL | NaN-like pixels |
| KS11_bridge_train | 4 | 1920x1080 | 0.119 | 0.067 | 0.0148 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void | 4 | 1920x1080 | 0.128 | 0.070 | 0.0000 | 0.684 | FAIL | NaN-like pixels |
| KS13_bridge_glass | 4 | 1920x1080 | 0.141 | 0.068 | 0.0009 | 1.117 | FAIL | banding[abyss] |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS00_alley_start | - | INFO |
| KS01_alley_noodle | - | INFO |
| KS02_alley_slit | - | INFO |
| KS03_boulevard_mouth | - | INFO |
| KS04_boulevard_axis | - | INFO |
| KS06_tram | - | INFO |
| KS05_boulevard_up | - | INFO |
| KS07_edge_window | 1920x1080 | INFO |
| KS08_edge_flank | 1920x1080 | INFO |
| KS09_edge_gulf | 1920x1080 | INFO |
| KS10_edge_plunge | 1920x1080 | INFO |
| KS11_bridge_train | 1920x1080 | INFO |
| KS12_bridge_void | 1920x1080 | INFO |
| KS13_bridge_glass | 1920x1080 | INFO |
| KS14_garden_gate | - | INFO |
| KS15_garden_canopy | - | INFO |
| KS16_signs_wall | - | INFO |
| KS17_heart_reveal | - | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### KS07_edge_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r07\KS07_edge_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[872, 222], [874, 290], [931, 584], [988, 584]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[872, 222], [874, 290]]; neon mask 4.289 % of the frame |
| clipped highlights | PASS | 0.0054 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0742 | >= 0.0353 | display |
| mean luma | PASS | 0.1266 | 0.0941..0.1961 | display |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r07\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 7 | 0 | bright 2 (mode all), dark 5, first at [[242, 231], [194, 303], [198, 304], [206, 306], [210, 307]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[1456, 584], [1456, 587], [1460, 587], [1461, 587]]; neon mask 4.095 % of the frame |
| clipped highlights | PASS | 0.0065 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0622 | >= 0.0353 | display |
| mean luma | PASS | 0.1534 | 0.0941..0.1961 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r07\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 9 | 0 | bright 9 (mode all), dark 0, first at [[1126, 55], [1062, 112], [1181, 271], [1023, 478], [998, 480]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[865, 492]]; neon mask 2.976 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0628 | >= 0.0353 | display |
| mean luma | PASS | 0.1354 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.3550 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.53 (run 6 px), - / -, 4 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r07\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 4 (mode all), dark 1, first at [[879, 290], [1187, 297], [1026, 302], [1427, 341], [762, 422]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.662 % of the frame |
| clipped highlights | PASS | 0.0048 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0586 | >= 0.0353 | display |
| mean luma | PASS | 0.1312 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.8060 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.21 (run 9 px), - / -, 4 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r07\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 1 (mode all), dark 1, first at [[1022, 508], [1419, 605]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1086, 101], [1087, 101]]; neon mask 2.389 % of the frame |
| clipped highlights | PASS | 0.0148 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0667 | >= 0.0353 | display |
| mean luma | PASS | 0.1194 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r07\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 12 | 0 | bright 12 (mode all), dark 0, first at [[1330, 12], [1327, 16], [557, 469], [588, 469], [619, 469]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.830 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0698 | >= 0.0353 | display |
| mean luma | PASS | 0.1276 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.6840 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.03 (run 7 px), - / -, 3 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r07\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.355 % of the frame |
| clipped highlights | PASS | 0.0009 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0683 | >= 0.0353 | display |
| mean luma | PASS | 0.1407 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.1170 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.68 (run 6 px), - / -, 1 strips |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
