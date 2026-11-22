# Image metrics: HT_r02

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02`  
Scene constants: NocturneConstants.cs  
Images: 10, failing images: 6. Checks: 81 PASS, 6 FAIL, 0 WARN, 0 SKIP, 7 INFO.

**Result: FAIL**

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS17_heart_reveal | 7 | 1920x1080 | 0.148 | 0.057 | 0.0000 | 0.607 | FAIL | NaN-like pixels |
| KS18_koi_pass | 7 | 1920x1080 | 0.196 | 0.070 | 0.0000 | 0.541 | FAIL | isolated pixels > 8x median |
| KS19_goal_window | 7 | 1920x1080 | 0.121 | 0.058 | 0.0032 | 0.073 | FAIL | NaN-like pixels |
| beat07_heart_end | 7 | 1920x1080 | 0.107 | 0.059 | 0.0000 | - | FAIL | NaN-like pixels |
| beat07_heart_look_back | 7 | 1920x1080 | 0.160 | 0.062 | 0.0001 | - | FAIL | NaN-like pixels |
| beat07_heart_look_l | 7 | 1920x1080 | 0.128 | 0.066 | 0.0009 | - | PASS |  |
| beat07_heart_look_r | 7 | 1920x1080 | 0.126 | 0.063 | 0.0010 | - | FAIL | NaN-like pixels |
| beat07_heart_look_up60 | 7 | 1920x1080 | 0.262 | 0.081 | 0.0000 | - | PASS |  |
| beat07_heart_mid | 7 | 1920x1080 | 0.134 | 0.071 | 0.0000 | - | PASS |  |
| beat07_heart_start | 7 | 1920x1080 | 0.127 | 0.066 | 0.0001 | - | PASS |  |

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
| KS16_signs_wall | - | INFO |
| KS17_heart_reveal | 1920x1080 | INFO |
| KS18_koi_pass | 1920x1080 | INFO |
| KS19_goal_window | 1920x1080 | INFO |

## Details

### KS17_heart_reveal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\KS17_heart_reveal.png`

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

### KS18_koi_pass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\KS18_koi_pass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | FAIL | 51 | 0 outside neon masks | first at [[1331, 941], [1332, 944], [1330, 945], [1333, 947], [1331, 948]]; neon mask 2.258 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0698 | >= 0.0353 | display |
| mean luma | PASS | 0.1958 | 0.0706..0.2353 | display |
| banding[sky_koi] | PASS | 0.5410 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.81 (run 9 px), - / -, 9 strips |

### KS19_goal_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\KS19_goal_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 0 (mode all), dark 11, first at [[775, 558], [783, 575], [800, 613], [778, 668], [864, 692]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.579 % of the frame |
| clipped highlights | PASS | 0.0032 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0583 | >= 0.0353 | display |
| mean luma | PASS | 0.1209 | 0.0706..0.2353 | display |
| banding[tenement_wall] | PASS | 0.0730 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.11 (run 5 px), - / -, 2 strips |

### beat07_heart_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\beat07_heart_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[610, 438]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.303 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0588 | >= 0.0353 | display |
| mean luma | INFO | 0.1072 |  | display |

### beat07_heart_look_back (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\beat07_heart_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[754, 445], [1363, 509], [1881, 509], [769, 545]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.154 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0619 | >= 0.0353 | display |
| mean luma | INFO | 0.1596 |  | display |

### beat07_heart_look_l (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\beat07_heart_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.204 % of the frame |
| clipped highlights | PASS | 0.0009 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0661 | >= 0.0353 | display |
| mean luma | INFO | 0.1280 |  | display |

### beat07_heart_look_r (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\beat07_heart_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1219, 508]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.250 % of the frame |
| clipped highlights | PASS | 0.0010 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0633 | >= 0.0353 | display |
| mean luma | INFO | 0.1262 |  | display |

### beat07_heart_look_up60 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\beat07_heart_look_up60.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.383 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0813 | >= 0.0353 | display |
| mean luma | INFO | 0.2619 |  | display |

### beat07_heart_mid (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\beat07_heart_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.267 % of the frame |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0712 | >= 0.0353 | display |
| mean luma | INFO | 0.1335 |  | display |

### beat07_heart_start (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\heart\QA\captures\HT_r02\beat07_heart_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 1.189 % of the frame |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0656 | >= 0.0353 | display |
| mean luma | INFO | 0.1268 |  | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
