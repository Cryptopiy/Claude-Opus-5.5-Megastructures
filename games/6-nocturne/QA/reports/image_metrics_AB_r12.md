# Image metrics: AB_r12

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12`  
Scene constants: NocturneConstants.cs  
Images: 15, failing images: 15. Checks: 74 PASS, 30 FAIL, 0 WARN, 12 SKIP, 15 INFO.

**Result: FAIL**

## Input warnings

- KS04_boulevard_axis.png is not listed in report.json
- KS05_boulevard_up.png is not listed in report.json
- KS07_edge_window.png is not listed in report.json
- KS08_edge_flank.png is not listed in report.json
- KS09_edge_gulf.png is not listed in report.json
- KS10_edge_plunge.png is not listed in report.json
- KS11_bridge_train.png is not listed in report.json
- KS12_bridge_void.png is not listed in report.json
- KS13_bridge_glass.png is not listed in report.json
- KS14_garden_gate.png is not listed in report.json
- KS15_garden_canopy.png is not listed in report.json

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| KS04_boulevard_axis | - | 1920x1080 | 0.141 | 0.060 | 0.0091 | 0.543 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS05_boulevard_up | - | 1920x1080 | 0.170 | 0.067 | 0.0011 | 0.478 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS07_edge_window | - | 1920x1080 | 0.128 | 0.075 | 0.0056 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS08_edge_flank | - | 1920x1080 | 0.152 | 0.062 | 0.0042 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS09_edge_gulf | - | 1920x1080 | 0.131 | 0.063 | 0.0000 | 0.462 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS10_edge_plunge | - | 1920x1080 | 0.130 | 0.059 | 0.0052 | 0.580 | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS11_bridge_train | - | 1920x1080 | 0.120 | 0.067 | 0.0195 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS12_bridge_void | - | 1920x1080 | 0.126 | 0.063 | 0.0000 | 1.510 | FAIL | NaN-like pixels, isolated pixels > 8x median, banding[abyss] |
| KS13_bridge_glass | - | 1920x1080 | 0.131 | 0.068 | 0.0004 | 0.730 | FAIL | NaN-like pixels |
| KS14_garden_gate | - | 1920x1080 | 0.123 | 0.056 | 0.0036 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| KS15_garden_canopy | - | 1920x1080 | 0.126 | 0.054 | 0.0000 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat02_boulevard_end | 2 | 1920x1080 | 0.127 | 0.075 | 0.0061 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat03_edge_end | 3 | 1920x1080 | 0.110 | 0.067 | 0.0136 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat03_edge_start | 3 | 1920x1080 | 0.126 | 0.074 | 0.0049 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |
| beat04_skybridge_mid | 4 | 1920x1080 | 0.112 | 0.063 | 0.0015 | - | FAIL | NaN-like pixels, isolated pixels > 8x median |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS00_alley_start | - | INFO |
| KS01_alley_noodle | - | INFO |
| KS02_alley_slit | - | INFO |
| KS03_boulevard_mouth | - | INFO |
| KS04_boulevard_axis | 0x0 | INFO |
| KS06_tram | - | INFO |
| KS05_boulevard_up | 0x0 | INFO |
| KS07_edge_window | 0x0 | INFO |
| KS08_edge_flank | 0x0 | INFO |
| KS09_edge_gulf | 0x0 | INFO |
| KS10_edge_plunge | 0x0 | INFO |
| KS11_bridge_train | 0x0 | INFO |
| KS12_bridge_void | 0x0 | INFO |
| KS13_bridge_glass | 0x0 | INFO |
| KS14_garden_gate | 0x0 | INFO |
| KS15_garden_canopy | 0x0 | INFO |
| KS16_signs_wall | - | INFO |
| KS17_heart_reveal | - | INFO |
| KS18_koi_pass | - | INFO |
| KS19_goal_window | - | INFO |

## Details

### KS04_boulevard_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS04_boulevard_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 25 | 0 | bright 8 (mode all), dark 17, first at [[1215, 437], [870, 521], [881, 540], [883, 573], [131, 619]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1809, 811], [1810, 814]]; neon mask 2.681 % of the frame |
| clipped highlights | INFO | 0.0091 |  | beat unknown |
| mean luma | PASS | 0.1406 | 0.0941..0.2118 | display |
| banding[sky_above_facades] | PASS | 0.5430 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.81 (run 7 px), - / -, 3 strips |

### KS05_boulevard_up (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS05_boulevard_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 6 | 0 | bright 5 (mode all), dark 1, first at [[1485, 158], [883, 815], [887, 857], [1431, 1008], [877, 1074]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[862, 977]]; neon mask 2.867 % of the frame |
| clipped highlights | INFO | 0.0011 |  | beat unknown |
| mean luma | PASS | 0.1702 | 0.0941..0.2118 | display |
| banding[cloud_ceiling] | PASS | 0.4780 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.72 (run 9 px), - / -, 4 strips |

### KS07_edge_window (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS07_edge_window.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[872, 222], [874, 290], [967, 405], [952, 409]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[872, 222], [881, 312]]; neon mask 4.065 % of the frame |
| clipped highlights | INFO | 0.0056 |  | beat unknown |
| mean luma | PASS | 0.1282 | 0.0941..0.1961 | display |

### KS08_edge_flank (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS08_edge_flank.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[598, 21], [562, 146], [989, 790]] |
| isolated pixels > 8x median | FAIL | 6 | 0 outside neon masks | first at [[561, 156], [524, 292], [1456, 584], [1456, 587], [1460, 587]]; neon mask 4.144 % of the frame |
| clipped highlights | INFO | 0.0042 |  | beat unknown |
| mean luma | PASS | 0.1523 | 0.0941..0.1961 | display |

### KS09_edge_gulf (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS09_edge_gulf.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[1126, 55], [1062, 112], [1181, 271], [379, 517], [426, 717]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[143, 446], [143, 447], [379, 517]]; neon mask 2.948 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.1308 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.4620 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.69 (run 8 px), - / -, 4 strips |

### KS10_edge_plunge (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS10_edge_plunge.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1427, 341], [762, 422], [659, 660]] |
| isolated pixels > 8x median | FAIL | 6 | 0 outside neon masks | first at [[525, 145], [526, 145], [576, 308], [577, 308], [587, 384]]; neon mask 1.627 % of the frame |
| clipped highlights | INFO | 0.0052 |  | beat unknown |
| mean luma | PASS | 0.1298 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.5800 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.87 (run 7 px), - / -, 4 strips |

### KS11_bridge_train (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS11_bridge_train.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 14 | 0 | bright 13 (mode all), dark 1, first at [[738, 105], [1846, 118], [896, 289], [887, 290], [1868, 297]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[1086, 101], [1087, 101], [896, 243], [896, 289]]; neon mask 2.675 % of the frame |
| clipped highlights | INFO | 0.0195 |  | beat unknown |
| mean luma | PASS | 0.1196 | 0.0941..0.1961 | display |

### KS12_bridge_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS12_bridge_void.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1330, 12], [1327, 16]] |
| isolated pixels > 8x median | FAIL | 4 | 0 outside neon masks | first at [[426, 118], [152, 562], [153, 562], [187, 701]]; neon mask 0.549 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.1257 | 0.0941..0.1961 | display |
| banding[abyss] | FAIL | 1.5100 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.27 (run 6 px), - / -, 3 strips |

### KS13_bridge_glass (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS13_bridge_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1331, 78]] |
| isolated pixels > 8x median | PASS | 0 | 0 outside neon masks | 5 x 5 median, display luma; neon mask 0.848 % of the frame |
| clipped highlights | INFO | 0.0004 |  | beat unknown |
| mean luma | PASS | 0.1314 | 0.0941..0.1961 | display |
| banding[abyss] | PASS | 0.7300 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.10 (run 6 px), - / -, 1 strips |

### KS14_garden_gate (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS14_garden_gate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 21 | 0 | bright 21 (mode all), dark 0, first at [[753, 15], [1820, 188], [218, 199], [1354, 205], [213, 209]] |
| isolated pixels > 8x median | FAIL | 2 | 0 outside neon masks | first at [[1580, 245], [1423, 494]]; neon mask 2.688 % of the frame |
| clipped highlights | INFO | 0.0036 |  | beat unknown |
| mean luma | PASS | 0.1233 | 0.0941..0.1961 | display |

### KS15_garden_canopy (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\KS15_garden_canopy.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | SKIP | 1920x1080 |  | no requested size in report.json |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 18 | 0 | bright 13 (mode all), dark 5, first at [[1919, 87], [1457, 115], [87, 332], [1599, 398], [752, 416]] |
| isolated pixels > 8x median | FAIL | 1 | 0 outside neon masks | first at [[1249, 1007]]; neon mask 3.079 % of the frame |
| clipped highlights | INFO | 0.0000 |  | beat unknown |
| mean luma | PASS | 0.1259 | 0.0784..0.1725 | display |

### beat02_boulevard_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\beat02_boulevard_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 10 | 0 | bright 10 (mode all), dark 0, first at [[1809, 126], [651, 176], [1171, 176], [863, 186], [864, 228]] |
| isolated pixels > 8x median | FAIL | 7 | 0 outside neon masks | first at [[865, 193], [865, 194], [881, 229], [456, 246], [457, 246]]; neon mask 3.977 % of the frame |
| clipped highlights | PASS | 0.0061 | <= 1.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0745 | >= 0.0353 | display |
| mean luma | INFO | 0.1274 |  | display |
| horizon[default] | SKIP | 540.0000 |  | sky band not uniform (spread 72.0 codes): objects on the horizon |

### beat03_edge_end (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\beat03_edge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[891, 105], [871, 251], [511, 407]] |
| isolated pixels > 8x median | FAIL | 6 | 0 outside neon masks | first at [[881, 228], [881, 229], [865, 234], [866, 234], [871, 251]]; neon mask 0.794 % of the frame |
| clipped highlights | PASS | 0.0136 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0672 | >= 0.0353 | display |
| mean luma | INFO | 0.1097 |  | display |

### beat03_edge_start (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\beat03_edge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[873, 258], [967, 375], [952, 379], [970, 549]] |
| isolated pixels > 8x median | FAIL | 3 | 0 outside neon masks | first at [[875, 208], [876, 208], [882, 287]]; neon mask 3.919 % of the frame |
| clipped highlights | PASS | 0.0049 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0742 | >= 0.0353 | display |
| mean luma | INFO | 0.1262 |  | display |

### beat04_skybridge_mid (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_r12\beat04_skybridge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 13 | 0 | bright 11 (mode all), dark 2, first at [[563, 42], [1416, 42], [1745, 42], [1745, 78], [1745, 114]] |
| isolated pixels > 8x median | FAIL | 20 | 0 outside neon masks | first at [[1745, 6], [1745, 42], [548, 78], [549, 78], [1431, 78]]; neon mask 3.360 % of the frame |
| clipped highlights | PASS | 0.0015 | <= 0.8 % | any channel at 255; 0 px excluded |
| luma p1 | PASS | 0.0628 | >= 0.0353 | display |
| mean luma | INFO | 0.1116 |  | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10 gates (p1 >= 9 codes, mean-luma windows, region std) use display luma unless --luma-scale linear.
- Clipped: any channel >= 254, percentage of all pixels outside the exclusion discs (14.10 per-beat limits).
- NaN-like: display luma more than 0.30 above the max / 0.10 below the min of the 24 neighbours (isolated); isolated: display luma above 8 x the 5 x 5 median outside the neon masks (14.10). Neon mask: emitter pixels (display luma >= 0.40, or saturation >= 0.50 with luma >= 0.12, and >= 2 x the 5 x 5 median) with >= 3 emitter pixels in their 3 x 3, dilated 2 + 1 px (a lone pixel is never masked).
- Magenta (missing shader): R and B >= 170 and G <= 0.15 x min(R, B).
- Banding (14.7 H1 + 11.3): 64-px strips; Y, Cb, Cr profiles after a 9-px box; a confirmed visible step is >= 1.0 code between plateaus each >= 24 px (flat within 0.3 code) at most 24 px apart: 0 per 1000 px on 8-bit captures. BandScore = longest run of identical 8-bit luma x the local slope (= run / expected run) where the slope is >= 0.02 level/px, <= 1.5.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
