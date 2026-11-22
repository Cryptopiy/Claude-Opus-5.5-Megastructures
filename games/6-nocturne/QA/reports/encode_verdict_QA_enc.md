# Encode verdict: QA_enc

Gate (14.7 H1 (6)): x264 CRF 23 decode, visible steps <= 2 / 1000 and none > 1.5 codes (worst of Y, Cb, Cr, R, G, B); CRF 30 / VP9 = regression rows; BandScore reported only. The H1 (6) clips are judged at 3840 x 2160 (_4k).

**Result: FAIL**

| Clip | Size | Frames | Verdict |
|---|---|---|---|
| enc_beat01_slit | 1920x1080 | 240 | PASS |
| enc_beat01_wall | 1920x1080 | 240 | PASS |
| enc_beat02_axis | 1920x1080 | 240 | FAIL |
| enc_beat02_axis_4k | 3840x2160 | 120 | FAIL |
| enc_beat02_axis_4k_nodither | 3840x2160 | 120 | FAIL |
| enc_beat02_sky | 1920x1080 | 240 | PASS |
| enc_beat03_abyss | 1920x1080 | 240 | PASS |
| enc_beat03_abyss_4k | 3840x2160 | 120 | FAIL |
| enc_beat03_abyss_4k_nodither | 3840x2160 | 120 | FAIL |
| enc_beat04_void | 1920x1080 | 240 | PASS |
| enc_beat05_crown | 1920x1080 | 240 | PASS |
| enc_beat06_wall | 1920x1080 | 240 | PASS |
| enc_beat06_wall_4k | 3840x2160 | 120 | PASS |
| enc_beat06_wall_4k_nodither | 3840x2160 | 120 | PASS |
| enc_beat07_goal | 1920x1080 | 240 | PASS |
| enc_beat07_plaza | 1920x1080 | 240 | FAIL |
| enc_beat07_plaza_4k | 3840x2160 | 120 | FAIL |
| enc_beat07_plaza_4k_nodither | 3840x2160 | 120 | FAIL |
| enc_trans_mouth | 1920x1080 | 720 | PASS |


| Clip | Size | Stream | Region | Steps/1000 | Max code | Y / Cb / Cr (steps/1000 / max) | BandScore | Result |
|---|---|---|---|---|---|---|---|---|
| enc_beat01_slit | 1920x1080 | x264 23 | slit_cloud | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 2.82 | PASS |
| enc_beat01_slit | 1920x1080 | x264 30 (regr.) | slit_cloud | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 3.23 | INFO |
| enc_beat01_slit | 1920x1080 | vp9 31 (regr.) | slit_cloud | 4.41 | 1.01 | Y 4.41/1.01, Cb 0.00/0.00, Cr 0.00/0.00 | 2.53 | INFO |
| enc_beat01_wall | 1920x1080 | x264 23 | alley_wall | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 15.11 | PASS |
| enc_beat01_wall | 1920x1080 | x264 30 (regr.) | alley_wall | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 14.84 | INFO |
| enc_beat01_wall | 1920x1080 | vp9 31 (regr.) | alley_wall | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 19.20 | INFO |
| enc_beat02_axis | 1920x1080 | x264 23 | sky_above_facades | 9.22 | 1.18 | Y 0.00/0.00, Cb 0.00/0.00, Cr 9.22/1.18 | 3.61 | FAIL |
| enc_beat02_axis | 1920x1080 | x264 30 (regr.) | sky_above_facades | 9.22 | 1.37 | Y 0.00/0.00, Cb 9.22/1.37, Cr 0.00/0.00 | 3.95 | INFO |
| enc_beat02_axis | 1920x1080 | vp9 31 (regr.) | sky_above_facades | 13.82 | 1.20 | Y 0.00/0.00, Cb 13.82/1.12, Cr 4.61/1.20 | 3.75 | INFO |
| enc_beat02_axis_4k | 3840x2160 | x264 23 | sky_above_facades | 2.31 | 1.08 | Y 0.00/0.00, Cb 0.00/0.00, Cr 2.31/1.08 | 5.26 | FAIL |
| enc_beat02_axis_4k | 3840x2160 | x264 30 (regr.) | sky_above_facades | 2.31 | 1.87 | Y 2.31/1.01, Cb 2.31/1.36, Cr 0.00/0.00 | 6.68 | INFO |
| enc_beat02_axis_4k | 3840x2160 | vp9 31 (regr.) | sky_above_facades | 2.31 | 2.27 | Y 0.00/0.00, Cb 2.31/1.24, Cr 0.00/0.00 | 6.39 | INFO |
| enc_beat02_axis_4k_nodither | 3840x2160 | x264 23 | sky_above_facades | 2.31 | 1.17 | Y 0.00/0.00, Cb 2.31/1.17, Cr 0.00/0.00 | 7.17 | FAIL |
| enc_beat02_sky | 1920x1080 | x264 23 | cloud_ceiling | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 5.17 | PASS |
| enc_beat02_sky | 1920x1080 | x264 30 (regr.) | cloud_ceiling | 2.99 | 1.93 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 7.63 | INFO |
| enc_beat02_sky | 1920x1080 | vp9 31 (regr.) | cloud_ceiling | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 11.36 | INFO |
| enc_beat03_abyss | 1920x1080 | x264 23 | abyss | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 3.74 | PASS |
| enc_beat03_abyss | 1920x1080 | x264 30 (regr.) | abyss | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 3.70 | INFO |
| enc_beat03_abyss | 1920x1080 | vp9 31 (regr.) | abyss | 5.13 | 1.06 | Y 0.00/0.00, Cb 5.13/1.06, Cr 0.00/0.00 | 1.78 | INFO |
| enc_beat03_abyss_4k | 3840x2160 | x264 23 | abyss | 2.56 | 1.01 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 3.01 | FAIL |
| enc_beat03_abyss_4k | 3840x2160 | x264 30 (regr.) | abyss | 2.56 | 2.00 | Y 2.56/1.14, Cb 0.00/0.00, Cr 2.56/1.53 | 3.34 | INFO |
| enc_beat03_abyss_4k | 3840x2160 | vp9 31 (regr.) | abyss | 2.56 | 2.00 | Y 2.56/1.57, Cb 2.56/1.41, Cr 2.56/1.55 | 2.95 | INFO |
| enc_beat03_abyss_4k_nodither | 3840x2160 | x264 23 | abyss | 2.56 | 1.02 | Y 2.56/1.02, Cb 0.00/0.00, Cr 0.00/0.00 | 3.10 | FAIL |
| enc_beat04_void | 1920x1080 | x264 23 | abyss | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 17.26 | PASS |
| enc_beat04_void | 1920x1080 | x264 30 (regr.) | abyss | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 19.57 | INFO |
| enc_beat04_void | 1920x1080 | vp9 31 (regr.) | abyss | 6.13 | 1.22 | Y 0.00/0.00, Cb 0.00/0.00, Cr 6.13/1.22 | 20.46 | INFO |
| enc_beat05_crown | 1920x1080 | x264 23 | crown_gap | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 7.56 | PASS |
| enc_beat05_crown | 1920x1080 | x264 30 (regr.) | crown_gap | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 6.86 | INFO |
| enc_beat05_crown | 1920x1080 | vp9 31 (regr.) | crown_gap | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 7.45 | INFO |
| enc_beat06_wall | 1920x1080 | x264 23 | wall_panel | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 7.62 | PASS |
| enc_beat06_wall | 1920x1080 | x264 30 (regr.) | wall_panel | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 10.91 | INFO |
| enc_beat06_wall | 1920x1080 | vp9 31 (regr.) | wall_panel | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 14.21 | INFO |
| enc_beat06_wall_4k | 3840x2160 | x264 23 | wall_panel | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 7.25 | PASS |
| enc_beat06_wall_4k | 3840x2160 | x264 30 (regr.) | wall_panel | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 9.75 | INFO |
| enc_beat06_wall_4k | 3840x2160 | vp9 31 (regr.) | wall_panel | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 8.33 | INFO |
| enc_beat06_wall_4k_nodither | 3840x2160 | x264 23 | wall_panel | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 8.59 | PASS |
| enc_beat07_goal | 1920x1080 | x264 23 | tenement_wall | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 0.22 | PASS |
| enc_beat07_goal | 1920x1080 | x264 30 (regr.) | tenement_wall | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 0.60 | INFO |
| enc_beat07_goal | 1920x1080 | vp9 31 (regr.) | tenement_wall | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 0.38 | INFO |
| enc_beat07_plaza | 1920x1080 | x264 23 | sky_koi_spire | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 5.67 | PASS |
| enc_beat07_plaza | 1920x1080 | x264 23 | plaza | 8.33 | 2.11 | Y 8.33/1.55, Cb 8.33/1.26, Cr 8.33/1.06 | 6.50 | FAIL |
| enc_beat07_plaza | 1920x1080 | x264 30 (regr.) | sky_koi_spire | 6.58 | 1.58 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 4.51 | INFO |
| enc_beat07_plaza | 1920x1080 | x264 30 (regr.) | plaza | 8.33 | 1.99 | Y 8.33/1.98, Cb 8.33/1.32, Cr 0.00/0.00 | 6.88 | INFO |
| enc_beat07_plaza | 1920x1080 | vp9 31 (regr.) | sky_koi_spire | 6.58 | 1.03 | Y 0.00/0.00, Cb 0.00/0.00, Cr 6.58/1.03 | 5.23 | INFO |
| enc_beat07_plaza | 1920x1080 | vp9 31 (regr.) | plaza | 8.33 | 1.22 | Y 0.00/0.00, Cb 8.33/1.28, Cr 8.33/1.23 | 5.52 | INFO |
| enc_beat07_plaza_4k | 3840x2160 | x264 23 | sky_koi_spire | 3.29 | 1.99 | Y 3.29/1.24, Cb 0.00/0.00, Cr 0.00/0.00 | 3.77 | FAIL |
| enc_beat07_plaza_4k | 3840x2160 | x264 23 | plaza | 4.18 | 1.05 | Y 0.00/0.00, Cb 8.37/1.34, Cr 4.18/1.28 | 6.04 | FAIL |
| enc_beat07_plaza_4k | 3840x2160 | x264 30 (regr.) | sky_koi_spire | 3.29 | 1.20 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 4.29 | INFO |
| enc_beat07_plaza_4k | 3840x2160 | x264 30 (regr.) | plaza | 8.37 | 3.52 | Y 4.18/1.35, Cb 8.37/1.42, Cr 4.18/1.23 | 8.92 | INFO |
| enc_beat07_plaza_4k | 3840x2160 | vp9 31 (regr.) | sky_koi_spire | 6.58 | 2.00 | Y 3.29/1.21, Cb 0.00/0.00, Cr 6.58/1.02 | 3.99 | INFO |
| enc_beat07_plaza_4k | 3840x2160 | vp9 31 (regr.) | plaza | 8.37 | 1.36 | Y 0.00/0.00, Cb 8.37/1.36, Cr 8.37/1.28 | 3.15 | INFO |
| enc_beat07_plaza_4k_nodither | 3840x2160 | x264 23 | sky_koi_spire | 3.29 | 1.34 | Y 0.00/0.00, Cb 3.29/1.34, Cr 0.00/0.00 | 4.65 | FAIL |
| enc_beat07_plaza_4k_nodither | 3840x2160 | x264 23 | plaza | 4.18 | 1.33 | Y 0.00/0.00, Cb 8.37/1.33, Cr 4.18/1.11 | 5.59 | FAIL |
| enc_trans_mouth | 1920x1080 | x264 23 | mouth_dark_wall | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 45.49 | PASS |
| enc_trans_mouth | 1920x1080 | x264 30 (regr.) | mouth_dark_wall | 0.00 | 0.00 | Y 0.00/0.00, Cb 0.00/0.00, Cr 0.00/0.00 | 46.08 | INFO |
| enc_trans_mouth | 1920x1080 | vp9 31 (regr.) | mouth_dark_wall | 1.85 | 4.32 | Y 0.00/0.00, Cb 1.85/4.32, Cr 0.00/0.00 | 87.41 | INFO |
