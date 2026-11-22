# Image metrics: SG_r06

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06`  
Scene constants: NocturneConstants.cs  
Images: 10, failing images: 6. Checks: 78 PASS, 7 FAIL, 0 WARN, 0 SKIP, 8 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS16_signs_wall | 6 | 1920x1080 | 0.195 | 0.051 | 0.0000 | 2.004 | FAIL | NaN-like pixels, banding[wall_panel] |
| KS17_heart_reveal | 7 | 1920x1080 | 0.148 | 0.057 | 0.0000 | 0.607 | FAIL | NaN-like pixels |
| beat06_signs_end | 6 | 1920x1080 | 0.118 | 0.064 | 0.0000 | - | FAIL | NaN-like pixels |
| beat06_signs_look_back | 6 | 1920x1080 | 0.103 | 0.052 | 0.0000 | - | PASS |  |
| beat06_signs_look_l | 6 | 1920x1080 | 0.174 | 0.054 | 0.0000 | - | PASS |  |
| beat06_signs_look_r | 6 | 1920x1080 | 0.130 | 0.064 | 0.0000 | - | PASS |  |
| beat06_signs_look_up60 | 6 | 1920x1080 | 0.208 | 0.063 | 0.0000 | - | PASS |  |
| beat06_signs_mid | 6 | 1920x1080 | 0.152 | 0.063 | 0.0000 | - | FAIL | isolated pixels > 8x median |
| beat06_signs_start | 6 | 1920x1080 | 0.091 | 0.053 | 0.0000 | - | FAIL | NaN-like pixels |
| beat07_heart_look_back | 7 | 1920x1080 | 0.159 | 0.062 | 0.0001 | - | FAIL | NaN-like pixels |

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
| KS11_bridge_train | - | INFO |
| KS12_bridge_void | - | INFO |
| KS13_bridge_glass | - | INFO |
| KS14_garden_gate | - | INFO |
| KS15_garden_canopy | - | INFO |
| KS16_signs_wall | 1920x1080 | INFO |
| KS17_heart_reveal | 1920x1080 | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### KS16_signs_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\KS16_signs_wall.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 3 (mode all), dark 3, first at [[303, 104], [1841, 1019], [1743, 1064], [1411, 1071], [1412, 1075]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.248 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0510 | >= 0.0353 | display |
| mean luma | PASS | 0.1947 | 0.1255..0.251 | display |
| banding[wall_panel] | FAIL | 2.0040 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 3.01 (run 7 px), - / -, 3 strips |

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\KS17_heart_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[1467, 643]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.308 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0572 | >= 0.0353 | display |
| mean luma | PASS | 0.1483 | 0.0706..0.2353 | display |
| banding[sky_koi_spire] | PASS | 0.6070 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.91 (run 9 px), - / -, 6 strips |
| banding[plaza] | PASS | 0.0000 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.02 (run 9 px) not gated (textured: gate steps), - / -, 22 strips |

### beat06_signs_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\beat06_signs_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[336, 447]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.741 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0639 | >= 0.0353 | display |
| mean luma | INFO | 0.1176 |  | display |

### beat06_signs_look_back (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\beat06_signs_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.181 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0521 | >= 0.0353 | display |
| mean luma | INFO | 0.1028 |  | display |

### beat06_signs_look_l (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\beat06_signs_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.134 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0538 | >= 0.0353 | display |
| mean luma | INFO | 0.1738 |  | display |

### beat06_signs_look_r (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\beat06_signs_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.057 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0636 | >= 0.0353 | display |
| mean luma | INFO | 0.1300 |  | display |

### beat06_signs_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\beat06_signs_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.093 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0628 | >= 0.0353 | display |
| mean luma | INFO | 0.2084 |  | display |

### beat06_signs_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\beat06_signs_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[379, 462]]; neon mask 0.256 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0628 | >= 0.0353 | display |
| mean luma | INFO | 0.1517 |  | display |

### beat06_signs_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\beat06_signs_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[615, 466], [461, 493], [424, 499], [281, 524]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.154 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0533 | >= 0.0353 | display |
| mean luma | INFO | 0.0907 |  | display |

### beat07_heart_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\SG_r06\beat07_heart_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[754, 445], [1363, 509], [1881, 509], [769, 545]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.180 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | INFO | 0.1588 |  | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
