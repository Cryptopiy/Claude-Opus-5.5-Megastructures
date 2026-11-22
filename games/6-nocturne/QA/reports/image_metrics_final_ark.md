# Image metrics: final_ark

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark`  
Scene constants: NocturneConstants.cs  
Images: 13, failing images: 11. Checks: 48 PASS, 17 FAIL, 4 WARN, 13 SKIP, 26 INFO.

**Result: FAIL**

## Input warnings

- report.json missing or unreadable: sizes, poses and subjects unknown

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| ark_s0430_bow_far | - | 1920x1080 | 0.137 | 0.065 | 0.0231 | - | FAIL | NaN-like pixels |
| ark_s0494_bow_gaze | - | 1920x1080 | 0.152 | 0.061 | 0.0161 | - | FAIL | NaN-like pixels |
| ark_s0540_bow_near | - | 1920x1080 | 0.145 | 0.065 | 0.0042 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| ark_s0580_bow_gaze | - | 1920x1080 | 0.164 | 0.071 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| ark_s0590_bow_over | - | 1920x1080 | 0.162 | 0.073 | 0.0000 | - | FAIL | NaN-like pixels |
| ark_s0600_zenith | - | 1920x1080 | 0.203 | 0.087 | 0.0000 | - | PASS |  |
| ark_s0615_bow_south | - | 1920x1080 | 0.142 | 0.068 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| ark_s0660_belly_north | - | 1920x1080 | 0.146 | 0.062 | 0.0066 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| ark_s0700_belly_zenith | - | 1920x1080 | 0.157 | 0.110 | 0.0002 | - | WARN |  |
| ark_s0774_belly_north_gaze | - | 1920x1080 | 0.150 | 0.070 | 0.0012 | - | FAIL | NaN-like pixels |
| ark_s0810_belly_up_gaze | - | 1920x1080 | 0.153 | 0.072 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| ark_s0960_garden_stern | - | 1920x1080 | 0.113 | 0.055 | 0.0053 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| ark_s1020_garden_stern_late | - | 1920x1080 | 0.132 | 0.054 | 0.0001 | - | FAIL | NaN-like pixels |

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
| KS17_heart_reveal | - | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### ark_s0430_bow_far (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0430_bow_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1440, 246], [1419, 458]]; thin-line mask 1.214 %; owner ? (beat None) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 3.218 % of the frame, thin-line mask 1.214 % |
| clipped highlights | INFO | 0.0231 |  | beat unknown |
| mean luma | INFO | 0.1373 |  | display |

### ark_s0494_bow_gaze (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0494_bow_gaze.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 8 | 0 | bright 8 (mode all), dark 0, first at [[1851, 0], [642, 319], [1309, 378], [619, 503], [930, 506]]; thin-line mask 2.113 %; owner ? (beat None) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 4.381 % of the frame, thin-line mask 2.113 % |
| clipped highlights | INFO | 0.0161 |  | beat unknown |
| mean luma | INFO | 0.1522 |  | display |

### ark_s0540_bow_near (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0540_bow_near.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 34 | 0 | bright 34 (mode all), dark 0, first at [[557, 304], [556, 308], [554, 314], [535, 383], [1633, 404]]; thin-line mask 1.290 %; owner ? (beat None) |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon and thin-line masks | first at [[1629, 406], [1263, 920]]; neon mask 4.002 % of the frame, thin-line mask 1.290 %; owner ? (beat None): real 1-2 px emitters |
| clipped highlights | INFO | 0.0042 |  | beat unknown |
| mean luma | INFO | 0.1449 |  | display |

### ark_s0580_bow_gaze (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0580_bow_gaze.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 25 | 0 | bright 25 (mode all), dark 0, first at [[141, 45], [174, 388], [167, 395], [921, 587], [902, 605]]; thin-line mask 1.419 %; owner ? (beat None) |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[67, 76]]; neon mask 6.022 % of the frame, thin-line mask 1.419 %; owner ? (beat None): real 1-2 px emitters |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | INFO | 0.1643 |  | display |

### ark_s0590_bow_over (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0590_bow_over.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 11 | 0 | bright 11 (mode all), dark 0, first at [[45, 610], [24, 678], [458, 970], [389, 975], [382, 993]]; thin-line mask 1.476 %; owner ? (beat None) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 5.171 % of the frame, thin-line mask 1.476 % |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | INFO | 0.1623 |  | display |

### ark_s0600_zenith (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0600_zenith.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 1.052 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 2.858 % of the frame, thin-line mask 1.052 % |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | INFO | 0.2030 |  | display |

### ark_s0615_bow_south (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0615_bow_south.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 10 | 0 | bright 10 (mode all), dark 0, first at [[1785, 96], [1774, 370], [1581, 549], [1614, 554], [1634, 620]]; thin-line mask 1.061 %; owner ? (beat None) |
| isolated pixels > 8x median | FAIL | 5 | 0 outside neon and thin-line masks | first at [[1774, 370], [1771, 381], [1796, 399], [1850, 438], [1669, 662]]; neon mask 2.306 % of the frame, thin-line mask 1.061 %; owner ? (beat None): real 1-2 px emitters |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | INFO | 0.1423 |  | display |

### ark_s0660_belly_north (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0660_belly_north.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 50 | 0 | bright 50 (mode all), dark 0, first at [[748, 168], [763, 216], [1256, 227], [837, 308], [800, 334]]; thin-line mask 1.175 %; owner ? (beat None) |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon and thin-line masks | first at [[267, 196], [218, 333], [218, 334]]; neon mask 4.383 % of the frame, thin-line mask 1.175 %; owner ? (beat None): real 1-2 px emitters |
| clipped highlights | INFO | 0.0066 |  | beat unknown |
| mean luma | INFO | 0.1459 |  | display |

### ark_s0700_belly_zenith (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0700_belly_zenith.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[782, 1079]]; owner ? (beat None) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0; thin-line mask 1.654 % |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 6.651 % of the frame, thin-line mask 1.654 % |
| clipped highlights | INFO | 0.0002 |  | beat unknown |
| mean luma | INFO | 0.1571 |  | display |

### ark_s0774_belly_north_gaze (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0774_belly_north_gaze.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 40 | 0 | bright 40 (mode all), dark 0, first at [[1604, 120], [1654, 209], [219, 236], [720, 410], [744, 423]]; thin-line mask 1.793 %; owner ? (beat None) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 7.365 % of the frame, thin-line mask 1.793 % |
| clipped highlights | INFO | 0.0012 |  | beat unknown |
| mean luma | INFO | 0.1505 |  | display |

### ark_s0810_belly_up_gaze (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0810_belly_up_gaze.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 1 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[368, 547]]; owner ? (beat None) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[1220, 842], [338, 983], [336, 1027], [356, 1038], [1318, 1039]]; thin-line mask 1.687 %; owner ? (beat None) |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon and thin-line masks | first at [[1249, 915], [1271, 945]]; neon mask 5.190 % of the frame, thin-line mask 1.687 %; owner ? (beat None): real 1-2 px emitters |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | INFO | 0.1525 |  | display |

### ark_s0960_garden_stern (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s0960_garden_stern.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 3 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[501, 10], [1793, 10], [973, 116]]; owner ? (beat None) |
| NaN-like pixels | FAIL | 10 | 0 | bright 10 (mode all), dark 0, first at [[19, 106], [1743, 119], [52, 147], [166, 197], [247, 270]]; thin-line mask 1.313 %; owner ? (beat None) |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon and thin-line masks | first at [[1743, 119]]; neon mask 3.315 % of the frame, thin-line mask 1.313 %; owner ? (beat None): real 1-2 px emitters |
| clipped highlights | INFO | 0.0053 |  | beat unknown |
| mean luma | INFO | 0.1128 |  | display |

### ark_s1020_garden_stern_late (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ark\ark_s1020_garden_stern_late.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| dark single-pixel specks (not NaN) | WARN | 3 | inventory | darker than all 24 neighbours by > 0.10 but not black (max channel > 12 codes): texture specks; first at [[871, 50], [997, 95], [912, 126]]; owner ? (beat None) |
| NaN-like pixels | FAIL | 16 | 0 | bright 16 (mode all), dark 0, first at [[118, 48], [700, 262], [1250, 398], [1041, 517], [307, 518]]; thin-line mask 1.930 %; owner ? (beat None) |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon and thin-line masks | 5 x 5 median, display luma; neon mask 3.195 % of the frame, thin-line mask 1.930 % |
| clipped highlights | INFO | 0.0001 |  | beat unknown |
| mean luma | INFO | 0.1319 |  | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
