# QA summary: p4_01_4k

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k`

**Overall: FAIL**

| Step | Status | Items | PASS | FAIL | Seconds | Report |
|---|---|---|---|---|---|---|
| image_metrics | FAIL | 80 | 648 | 132 | 38.9 | QA/reports\image_metrics_p4_01_4k.md |
| contact_sheet | PASS | 80 | - | - | 12.3 | W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\_contact\contact_p4_01_4k_01.png, W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\_contact\contact_p4_01_4k_02.png, W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\_contact\contact_p4_01_4k_03.png ... |
| sequences | SKIP | - | - | - | - | no seq_* folder |
| audio_analysis | SKIP | - | - | - | - | no WAV given or found |
| tiling | PASS | 16 | 0 | 0 | 22.5 | QA/reports\tiling_p4_01_4k.md |
| debugview | SKIP | - | - | - | - | no -debug-view in report.json and no --view |
| lookv2_check | FAIL | 80 | 54 | 26 | 81.1 | lookv2_check: 54 / 80 shots pass |
| facade_sheet | SKIP | - | - | - | - | no facade-sheet still (pose_fs*) |

## FAIL lines (158)

- **image_metrics** KS00_alley_start: NaN-like pixels (2)
- **image_metrics** KS00_alley_start: isolated pixels > 8x median (2)
- **image_metrics** KS00_alley_start_1610: NaN-like pixels (3)
- **image_metrics** KS00_alley_start_219: NaN-like pixels (2)
- **image_metrics** KS00_alley_start_219: isolated pixels > 8x median (2)
- **image_metrics** KS00_alley_start_4k: NaN-like pixels (6)
- **image_metrics** KS01_alley_noodle: NaN-like pixels (10)
- **image_metrics** KS01_alley_noodle_1610: NaN-like pixels (4)
- **image_metrics** KS01_alley_noodle_219: NaN-like pixels (61)
- **image_metrics** KS01_alley_noodle_219: mean luma (0.1600)
- **image_metrics** KS01_alley_noodle_4k: NaN-like pixels (35)
- **image_metrics** KS02_alley_slit_1610: isolated pixels > 8x median (1)
- **image_metrics** KS02_alley_slit_219: NaN-like pixels (6)
- **image_metrics** KS02_alley_slit_4k: NaN-like pixels (5)
- **image_metrics** KS02_alley_slit_4k: isolated pixels > 8x median (1)
- **image_metrics** KS03_boulevard_mouth: NaN-like pixels (1)
- **image_metrics** KS03_boulevard_mouth_1610: NaN-like pixels (2)
- **image_metrics** KS03_boulevard_mouth_1610: isolated pixels > 8x median (5)
- **image_metrics** KS03_boulevard_mouth_219: NaN-like pixels (1)
- **image_metrics** KS03_boulevard_mouth_4k: NaN-like pixels (3)
- **image_metrics** KS03_boulevard_mouth_4k: isolated pixels > 8x median (4)
- **image_metrics** KS04_boulevard_axis: NaN-like pixels (26)
- **image_metrics** KS04_boulevard_axis: isolated pixels > 8x median (2)
- **image_metrics** KS04_boulevard_axis_1610: NaN-like pixels (37)
- **image_metrics** KS04_boulevard_axis_1610: isolated pixels > 8x median (7)
- **image_metrics** KS04_boulevard_axis_219: NaN-like pixels (27)
- **image_metrics** KS04_boulevard_axis_219: isolated pixels > 8x median (4)
- **image_metrics** KS04_boulevard_axis_4k: NaN-like pixels (24)
- **image_metrics** KS04_boulevard_axis_4k: isolated pixels > 8x median (2)
- **image_metrics** KS04_boulevard_axis_4k: banding[sky_above_facades] (2.0000)
- **image_metrics** KS05_boulevard_up: NaN-like pixels (6)
- **image_metrics** KS05_boulevard_up: isolated pixels > 8x median (1)
- **image_metrics** KS05_boulevard_up_1610: NaN-like pixels (2)
- **image_metrics** KS05_boulevard_up_1610: isolated pixels > 8x median (2)
- **image_metrics** KS05_boulevard_up_219: NaN-like pixels (6)
- **image_metrics** KS05_boulevard_up_219: isolated pixels > 8x median (1)
- **image_metrics** KS05_boulevard_up_219: banding[cloud_ceiling] (1.2090)
- **image_metrics** KS05_boulevard_up_4k: NaN-like pixels (3)
- **image_metrics** KS05_boulevard_up_4k: isolated pixels > 8x median (2)
- **image_metrics** KS05_boulevard_up_4k: banding[cloud_ceiling] (2.0000)
- **image_metrics** KS06_tram: NaN-like pixels (11)
- **image_metrics** KS06_tram: isolated pixels > 8x median (2)
- **image_metrics** KS06_tram_1610: NaN-like pixels (8)
- **image_metrics** KS06_tram_1610: isolated pixels > 8x median (3)
- **image_metrics** KS06_tram_219: NaN-like pixels (13)
- **image_metrics** KS06_tram_219: isolated pixels > 8x median (3)
- **image_metrics** KS06_tram_4k: NaN-like pixels (7)
- **image_metrics** KS06_tram_4k: isolated pixels > 8x median (7)
- **image_metrics** KS07_edge_window: NaN-like pixels (4)
- **image_metrics** KS07_edge_window: isolated pixels > 8x median (2)
- **image_metrics** KS07_edge_window_1610: NaN-like pixels (5)
- **image_metrics** KS07_edge_window_1610: isolated pixels > 8x median (2)
- **image_metrics** KS07_edge_window_219: NaN-like pixels (4)
- **image_metrics** KS07_edge_window_219: isolated pixels > 8x median (1)
- **image_metrics** KS07_edge_window_4k: NaN-like pixels (6)
- **image_metrics** KS07_edge_window_4k: isolated pixels > 8x median (2)
- **image_metrics** KS08_edge_flank: NaN-like pixels (2)
- **image_metrics** KS08_edge_flank: isolated pixels > 8x median (4)
- **image_metrics** KS08_edge_flank_1610: NaN-like pixels (4)
- **image_metrics** KS08_edge_flank_1610: isolated pixels > 8x median (1)
- **image_metrics** KS08_edge_flank_219: NaN-like pixels (4)
- **image_metrics** KS08_edge_flank_219: isolated pixels > 8x median (6)
- **image_metrics** KS08_edge_flank_4k: NaN-like pixels (10)
- **image_metrics** KS09_edge_gulf: NaN-like pixels (9)
- **image_metrics** KS09_edge_gulf: isolated pixels > 8x median (2)
- **image_metrics** KS09_edge_gulf_1610: NaN-like pixels (11)
- **image_metrics** KS09_edge_gulf_1610: isolated pixels > 8x median (1)
- **image_metrics** KS09_edge_gulf_219: NaN-like pixels (8)
- **image_metrics** KS09_edge_gulf_219: isolated pixels > 8x median (2)
- **image_metrics** KS09_edge_gulf_4k: NaN-like pixels (4)
- **image_metrics** KS10_edge_plunge: NaN-like pixels (4)
- **image_metrics** KS10_edge_plunge_1610: NaN-like pixels (3)
- **image_metrics** KS10_edge_plunge_219: NaN-like pixels (5)
- **image_metrics** KS10_edge_plunge_219: banding[abyss] (4.4190)
- **image_metrics** KS10_edge_plunge_4k: NaN-like pixels (2)
- **image_metrics** KS10_edge_plunge_4k: banding[abyss] (1.4370)
- **image_metrics** KS11_bridge_train: NaN-like pixels (1)
- **image_metrics** KS11_bridge_train: isolated pixels > 8x median (2)
- **image_metrics** KS11_bridge_train_1610: NaN-like pixels (1)
- **image_metrics** KS11_bridge_train_219: NaN-like pixels (1)
- **image_metrics** KS11_bridge_train_219: isolated pixels > 8x median (2)
- **image_metrics** KS11_bridge_train_4k: NaN-like pixels (14)
- **image_metrics** KS12_bridge_void: NaN-like pixels (4)
- **image_metrics** KS12_bridge_void: isolated pixels > 8x median (1)
- **image_metrics** KS12_bridge_void: banding[abyss] (1.4850)
- **image_metrics** KS12_bridge_void_1610: NaN-like pixels (2)
- **image_metrics** KS12_bridge_void_1610: isolated pixels > 8x median (1)
- **image_metrics** KS12_bridge_void_1610: banding[abyss] (1.6750)
- **image_metrics** KS12_bridge_void_219: NaN-like pixels (4)
- **image_metrics** KS12_bridge_void_219: isolated pixels > 8x median (1)
- **image_metrics** KS12_bridge_void_219: banding[abyss] (2.5860)
- **image_metrics** KS12_bridge_void_4k: banding[abyss] (2.0000)
- **image_metrics** KS13_bridge_glass: NaN-like pixels (1)
- **image_metrics** KS13_bridge_glass: banding[abyss] (1.3370)
- **image_metrics** KS13_bridge_glass_1610: banding[abyss] (1.0590)
- **image_metrics** KS13_bridge_glass_219: banding[abyss] (1.2800)
- **image_metrics** KS14_garden_gate: NaN-like pixels (21)
- **image_metrics** KS14_garden_gate: isolated pixels > 8x median (2)
- **image_metrics** KS14_garden_gate_1610: NaN-like pixels (21)
- **image_metrics** KS14_garden_gate_1610: isolated pixels > 8x median (2)
- **image_metrics** KS14_garden_gate_219: NaN-like pixels (22)
- **image_metrics** KS14_garden_gate_219: isolated pixels > 8x median (2)
- **image_metrics** KS14_garden_gate_4k: NaN-like pixels (29)
- **image_metrics** KS14_garden_gate_4k: isolated pixels > 8x median (1)
- **image_metrics** KS15_garden_canopy: NaN-like pixels (18)
- **image_metrics** KS15_garden_canopy: isolated pixels > 8x median (1)
- **image_metrics** KS15_garden_canopy_1610: NaN-like pixels (22)
- **image_metrics** KS15_garden_canopy_1610: isolated pixels > 8x median (3)
- **image_metrics** KS15_garden_canopy_219: NaN-like pixels (20)
- **image_metrics** KS15_garden_canopy_219: isolated pixels > 8x median (2)
- **image_metrics** KS15_garden_canopy_4k: NaN-like pixels (36)
- **image_metrics** KS16_signs_wall: NaN-like pixels (7)
- **image_metrics** KS16_signs_wall: isolated pixels > 8x median (2)
- **image_metrics** KS16_signs_wall: banding[wall_panel] (2.0330)
- **image_metrics** KS16_signs_wall_1610: banding[wall_panel] (5.2080)
- **image_metrics** KS16_signs_wall_219: NaN-like pixels (9)
- **image_metrics** KS16_signs_wall_219: isolated pixels > 8x median (2)
- **image_metrics** KS16_signs_wall_219: banding[wall_panel] (7.9920)
- **image_metrics** KS16_signs_wall_4k: NaN-like pixels (11)
- **image_metrics** KS16_signs_wall_4k: banding[wall_panel] (3.2110)
- **image_metrics** KS17_heart_reveal: NaN-like pixels (1)
- **image_metrics** KS17_heart_reveal_1610: NaN-like pixels (3)
- **image_metrics** KS17_heart_reveal_219: NaN-like pixels (1)
- **image_metrics** KS17_heart_reveal_4k: NaN-like pixels (3)
- **image_metrics** KS18_koi_pass: isolated pixels > 8x median (51)
- **image_metrics** KS18_koi_pass_1610: isolated pixels > 8x median (19)
- **image_metrics** KS18_koi_pass_219: isolated pixels > 8x median (51)
- **image_metrics** KS18_koi_pass_4k: NaN-like pixels (3)
- **image_metrics** KS19_goal_window: NaN-like pixels (11)
- **image_metrics** KS19_goal_window_1610: NaN-like pixels (14)
- **image_metrics** KS19_goal_window_219: NaN-like pixels (12)
- **image_metrics** KS19_goal_window_4k: NaN-like pixels (72)
- **lookv2_check** KS00_alley_start.png       b1 FAIL mean_luma=30.5 texture_1_meso=0.93
- **lookv2_check** KS00_alley_start_1610.png  b1 FAIL mean_luma=30.9 texture_1_meso=0.96
- **lookv2_check** KS00_alley_start_219.png   b1 FAIL texture_0_meso=0.87 texture_0_pool=0.36 texture_1_meso=0.98
- **lookv2_check** KS00_alley_start_4k.png    b1 FAIL mean_luma=30.5 texture_1_meso=0.98
- **lookv2_check** KS01_alley_noodle.png      b1 FAIL mean_luma=39.5
- **lookv2_check** KS01_alley_noodle_1610.png b1 FAIL mean_luma=38.0
- **lookv2_check** KS01_alley_noodle_219.png  b1 FAIL mean_luma=40.8 texture_0_micro=1.3
- **lookv2_check** KS01_alley_noodle_4k.png   b1 FAIL mean_luma=39.4
- **lookv2_check** KS02_alley_slit.png        b1 FAIL texture_0_micro=1.22 texture_0_meso=0.36 texture_0_pool=0.14 texture_1_micro=1.4 texture_1_meso=0.36 texture_1_pool=0.01
- **lookv2_check** KS02_alley_slit_1610.png   b1 FAIL texture_0_micro=1.19 texture_0_meso=0.33 texture_0_pool=0.16 texture_1_micro=1.28 texture_1_meso=0.3 texture_1_pool=0.0
- **lookv2_check** KS02_alley_slit_219.png    b1 FAIL p99_luma=74.4 texture_0_micro=1.29 texture_0_meso=0.44 texture_0_pool=0.29 texture_1_meso=0.6 texture_1_pool=0.05
- **lookv2_check** KS02_alley_slit_4k.png     b1 FAIL texture_0_meso=0.45 texture_0_pool=0.17 texture_1_meso=0.55 texture_1_pool=0.02
- **lookv2_check** KS03_boulevard_mouth_219.png b1 FAIL mean_luma=22.9
- **lookv2_check** KS10_edge_plunge_219.png   b3 FAIL mean_luma=31.8
- **lookv2_check** KS13_bridge_glass.png      b4 FAIL p99_luma=83.9
- **lookv2_check** KS13_bridge_glass_1610.png b4 FAIL p99_luma=86.6
- **lookv2_check** KS13_bridge_glass_219.png  b4 FAIL p99_luma=79.4
- **lookv2_check** KS13_bridge_glass_4k.png   b4 FAIL p99_luma=86.1
- **lookv2_check** KS14_garden_gate.png       b4 FAIL p99_luma=107.9
- **lookv2_check** KS14_garden_gate_1610.png  b4 FAIL p99_luma=107.1
- **lookv2_check** KS14_garden_gate_219.png   b4 FAIL p99_luma=104.9
- **lookv2_check** KS14_garden_gate_4k.png    b4 FAIL p99_luma=103.4
- **lookv2_check** KS15_garden_canopy.png     b5 FAIL dark_cast_RG=-4.76
- **lookv2_check** KS15_garden_canopy_1610.png b5 FAIL dark_cast_BG=-2.23 dark_cast_RG=-5.3
- **lookv2_check** KS15_garden_canopy_4k.png  b5 FAIL dark_cast_RG=-4.14
- **lookv2_check** KS19_goal_window_219.png   b7 FAIL texture_1_meso=0.95

## Contact sheets (open them with the Read tool, Part 11.4)

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\_contact\contact_p4_01_4k_01.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\_contact\contact_p4_01_4k_02.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\_contact\contact_p4_01_4k_03.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_4k\_contact\contact_p4_01_4k_04.png`
