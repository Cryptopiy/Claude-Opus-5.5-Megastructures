# Image metrics: GD_r04

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04`  
Scene constants: NocturneConstants.cs  
Images: 9, failing images: 8. Checks: 64 PASS, 10 FAIL, 0 WARN, 0 SKIP, 7 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS14_garden_gate | 4 | 1920x1080 | 0.125 | 0.059 | 0.0036 | - | FAIL | NaN-like pixels |
| KS15_garden_canopy | 5 | 1920x1080 | 0.125 | 0.054 | 0.0000 | - | FAIL | NaN-like pixels |
| beat05_garden_end | 5 | 1920x1080 | 0.087 | 0.052 | 0.0000 | - | FAIL | NaN-like pixels |
| beat05_garden_look_back | 5 | 1920x1080 | 0.105 | 0.055 | 0.0027 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat05_garden_look_l | 5 | 1920x1080 | 0.086 | 0.054 | 0.0000 | - | PASS |  |
| beat05_garden_look_r | 5 | 1920x1080 | 0.085 | 0.052 | 0.0000 | - | FAIL | NaN-like pixels |
| beat05_garden_look_up60 | 5 | 1920x1080 | 0.135 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels |
| beat05_garden_mid | 5 | 1920x1080 | 0.083 | 0.054 | 0.0000 | - | FAIL | NaN-like pixels |
| beat05_garden_start | 5 | 1920x1080 | 0.091 | 0.055 | 0.0003 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |

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
| KS14_garden_gate | 1920x1080 | INFO |
| KS15_garden_canopy | 1920x1080 | INFO |
| KS16_signs_wall | - | INFO |
| KS17_heart_reveal | - | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### KS14_garden_gate (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 29 | 0 | bright 28 (mode all), dark 1, first at [[316, 0], [1863, 13], [753, 15], [1460, 75], [599, 81]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.449 % of the frame |
| clipped highlights | PASS | 0.0036 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0588 | >= 0.0353 | display |
| mean luma | PASS | 0.1253 | 0.0941..0.1961 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 24 | 0 | bright 19 (mode all), dark 5, first at [[1457, 115], [118, 310], [87, 332], [588, 359], [1272, 389]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.806 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0541 | >= 0.0353 | display |
| mean luma | PASS | 0.1247 | 0.0784..0.1725 | display |

### beat05_garden_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04\beat05_garden_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[447, 473], [299, 493]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.082 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0521 | >= 0.0353 | display |
| mean luma | INFO | 0.0868 |  | display |

### beat05_garden_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04\beat05_garden_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 19 | 0 | bright 19 (mode all), dark 0, first at [[1649, 12], [572, 67], [1309, 89], [1445, 92], [1021, 150]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1498, 512], [1499, 512], [525, 533]]; neon mask 2.559 % of the frame |
| clipped highlights | PASS | 0.0027 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0555 | >= 0.0353 | display |
| mean luma | INFO | 0.1046 |  | display |

### beat05_garden_look_l (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04\beat05_garden_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.838 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0544 | >= 0.0353 | display |
| mean luma | INFO | 0.0856 |  | display |

### beat05_garden_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04\beat05_garden_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1919, 116], [1080, 437]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.554 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0518 | >= 0.0353 | display |
| mean luma | INFO | 0.0855 |  | display |

### beat05_garden_look_up60 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04\beat05_garden_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 26 | 0 | bright 19 (mode all), dark 7, first at [[1503, 33], [73, 114], [33, 225], [3, 234], [806, 346]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 2.511 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0546 | >= 0.0353 | display |
| mean luma | INFO | 0.1352 |  | display |

### beat05_garden_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04\beat05_garden_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 6 (mode all), dark 0, first at [[1168, 7], [1465, 68], [1152, 94], [1270, 106], [1179, 108]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.667 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0535 | >= 0.0353 | display |
| mean luma | INFO | 0.0831 |  | display |

### beat05_garden_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_r04\beat05_garden_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 23 | 0 | bright 23 (mode all), dark 0, first at [[1474, 50], [450, 72], [1442, 92], [1073, 105], [1039, 116]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[1288, 273], [1288, 274], [1494, 508]]; neon mask 2.496 % of the frame |
| clipped highlights | PASS | 0.0003 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0549 | >= 0.0353 | display |
| mean luma | INFO | 0.0908 |  | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
