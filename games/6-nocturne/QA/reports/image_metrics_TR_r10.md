# Image metrics: TR_r10

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r10`  
Scene constants: NocturneConstants.cs  
Images: 5, failing images: 5. Checks: 36 PASS, 11 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS11_bridge_train | 4 | 1920x1080 | 0.119 | 0.067 | 0.0195 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void | 4 | 1920x1080 | 0.125 | 0.063 | 0.0000 | 1.485 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS13_bridge_glass | 4 | 1920x1080 | 0.134 | 0.068 | 0.0006 | 1.337 | FAIL | NaN-like pixels, banding[abyss] |
| KS14_garden_gate | 4 | 1920x1080 | 0.123 | 0.056 | 0.0036 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy | 5 | 1920x1080 | 0.126 | 0.054 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |

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
| KS07_edge_window | - | INFO |
| KS08_edge_flank | - | INFO |
| KS09_edge_gulf | - | INFO |
| KS10_edge_plunge | - | INFO |
| KS11_bridge_train | 1920x1080 | INFO |
| KS12_bridge_void | 1920x1080 | INFO |
| KS13_bridge_glass | 1920x1080 | INFO |
| KS14_garden_gate | 1920x1080 | INFO |
| KS15_garden_canopy | 1920x1080 | INFO |
| KS16_signs_wall | - | INFO |
| KS17_heart_reveal | - | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r10\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1419, 605]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1086, 101], [1087, 101]]; neon mask 2.463 % of the frame |
| clipped highlights | PASS | 0.0195 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0670 | >= 0.0353 | display |
| mean luma | PASS | 0.1192 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r10\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1330, 12], [1327, 16], [546, 612], [548, 622]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[426, 118]]; neon mask 0.636 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | PASS | 0.1250 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.4850 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.23 (run 6 px), - / -, 3 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r10\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[887, 727]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.401 % of the frame |
| clipped highlights | PASS | 0.0006 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0683 | >= 0.0353 | display |
| mean luma | PASS | 0.1344 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.3370 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.00 (run 7 px), - / -, 1 strips |

### KS14_garden_gate (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r10\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 21 | 0 | bright 21 (mode all), dark 0, first at [[753, 15], [1820, 188], [218, 199], [1354, 205], [213, 209]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1580, 245], [1423, 494]]; neon mask 2.645 % of the frame |
| clipped highlights | PASS | 0.0036 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0558 | >= 0.0353 | display |
| mean luma | PASS | 0.1234 | 0.0941..0.1961 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\TR_r10\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 18 | 0 | bright 13 (mode all), dark 5, first at [[1919, 87], [1457, 115], [87, 332], [1599, 398], [752, 416]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1249, 1007]]; neon mask 3.121 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0544 | >= 0.0353 | display |
| mean luma | PASS | 0.1259 | 0.0784..0.1725 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
