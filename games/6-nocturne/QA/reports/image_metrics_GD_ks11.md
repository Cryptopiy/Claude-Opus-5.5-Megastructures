# Image metrics: GD_ks11

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_ks11`  
Scene constants: NocturneConstants.cs  
Images: 4, failing images: 4. Checks: 28 PASS, 8 FAIL, 0 WARN, 0 SKIP, 0 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS15_garden_canopy | 5 | 1920x1080 | 0.127 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_1610 | 5 | 1680x1050 | 0.130 | 0.055 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_219 | 5 | 2560x1080 | 0.118 | 0.053 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy_4k | 5 | 3840x2160 | 0.128 | 0.055 | 0.0001 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |

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
| KS15_garden_canopy | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS16_signs_wall | - | INFO |
| KS17_heart_reveal | - | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_ks11\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 7 (mode all), dark 6, first at [[1465, 93], [1457, 115], [120, 158], [118, 310], [87, 332]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1756, 661], [360, 741]]; neon mask 2.644 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0546 | >= 0.0353 | display |
| mean luma | PASS | 0.1270 | 0.0784..0.1725 | display |

### KS15_garden_canopy_1610 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_ks11\KS15_garden_canopy_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 15 | 0 | bright 9 (mode all), dark 6, first at [[588, 18], [1331, 93], [23, 154], [2, 298], [13, 321]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[501, 824]]; neon mask 2.982 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0549 | >= 0.0353 | display |
| mean luma | PASS | 0.1295 | 0.0784..0.1725 | display |

### KS15_garden_canopy_219 (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_ks11\KS15_garden_canopy_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 12 | 0 | bright 7 (mode all), dark 5, first at [[1785, 93], [1777, 115], [440, 158], [407, 332], [1072, 416]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[2076, 661], [680, 741]]; neon mask 2.072 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0529 | >= 0.0353 | display |
| mean luma | PASS | 0.1179 | 0.0784..0.1725 | display |

### KS15_garden_canopy_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\waveb\QA\captures\GD_ks11\KS15_garden_canopy_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 37 | 0 | bright 10 (mode all), dark 27, first at [[1402, 33], [2959, 166], [2355, 214], [2911, 217], [236, 321]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1402, 33]]; neon mask 0.368 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0546 | >= 0.0353 | display |
| mean luma | PASS | 0.1278 | 0.0784..0.1725 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
