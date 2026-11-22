# Encode clips: QA_enc (2026-11-21 03:07)

| Clip | Size | Result | Report |
|---|---|---|---|
| enc_beat01_slit | 1920x1080 | FAIL | QA/reports/encode_test_QA_enc_enc_beat01_slit.md |
| enc_beat02_axis | 1920x1080 | FAIL | QA/reports/encode_test_QA_enc_enc_beat02_axis.md |
| enc_beat02_axis_4k | 3840x2160 | FAIL | QA/reports/encode_test_QA_enc_enc_beat02_axis_4k.md |
| enc_beat02_axis_4k_nodither | 3840x2160 | FAIL | QA/reports/encode_test_QA_enc_enc_beat02_axis_4k_nodither.md |

- enc_beat02_axis 4K CRF 23: dithered 2.3094688221709005 vs undithered 2.3094688221709005 visible steps / 1000: dithered better or equal

| enc_beat02_sky | 1920x1080 | FAIL | QA/reports/encode_test_QA_enc_enc_beat02_sky.md |
| enc_beat03_abyss | 1920x1080 | FAIL | QA/reports/encode_test_QA_enc_enc_beat03_abyss.md |
| enc_beat03_abyss_4k | 3840x2160 | FAIL | QA/reports/encode_test_QA_enc_enc_beat03_abyss_4k.md |
| enc_beat03_abyss_4k_nodither | 3840x2160 | FAIL | QA/reports/encode_test_QA_enc_enc_beat03_abyss_4k_nodither.md |

- enc_beat03_abyss 4K CRF 23: dithered 2.5641025641025643 vs undithered 2.5641025641025643 visible steps / 1000: dithered better or equal

| enc_beat04_void | 1920x1080 | FAIL | QA/reports/encode_test_QA_enc_enc_beat04_void.md |
| enc_beat05_crown | 1920x1080 | PASS | QA/reports/encode_test_QA_enc_enc_beat05_crown.md |
| enc_beat06_wall | 1920x1080 | PASS | QA/reports/encode_test_QA_enc_enc_beat06_wall.md |
| enc_beat06_wall_4k | 3840x2160 | PASS | QA/reports/encode_test_QA_enc_enc_beat06_wall_4k.md |
| enc_beat06_wall_4k_nodither | 3840x2160 | PASS | QA/reports/encode_test_QA_enc_enc_beat06_wall_4k_nodither.md |

- enc_beat06_wall 4K CRF 23: dithered 0.0 vs undithered 0.0 visible steps / 1000: dithered better or equal

| enc_beat07_plaza | 1920x1080 | FAIL | QA/reports/encode_test_QA_enc_enc_beat07_plaza.md |
| enc_beat07_plaza_4k | 3840x2160 | FAIL | QA/reports/encode_test_QA_enc_enc_beat07_plaza_4k.md |
| enc_beat07_plaza_4k_nodither | 3840x2160 | FAIL | QA/reports/encode_test_QA_enc_enc_beat07_plaza_4k_nodither.md |

- enc_beat07_plaza 4K CRF 23: dithered 4.184100418410042 vs undithered 4.184100418410042 visible steps / 1000: dithered better or equal

| enc_beat07_goal | 1920x1080 | PASS | QA/reports/encode_test_QA_enc_enc_beat07_goal.md |
| enc_beat01_wall | 1920x1080 | PASS | QA/reports/encode_test_QA_enc_enc_beat01_wall.md |
| enc_trans_mouth | 1920x1080 | PASS | QA/reports/encode_test_QA_enc_enc_trans_mouth.md |

lookv2_check --encoded (exit 0): QA/reports/lookv2_encoded_QA_enc.json
    KS00_alley_start.png       b1 FAIL texture_0_meso=0.55 texture_0_pool=0.04 texture_1_pool=0.38
    KS19_goal_window.png       b7 PASS 
    lookv2_check: 1 / 2 shots pass
