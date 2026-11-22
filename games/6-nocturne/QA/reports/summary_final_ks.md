# QA summary: final_ks

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks`

**Overall: FAIL**

| Step | Status | Items | PASS | FAIL | Seconds | Report |
|---|---|---|---|---|---|---|
| image_metrics | FAIL | 80 | 697 | 83 | 69.8 | QA/reports\image_metrics_final_ks.md |
| contact_sheet | PASS | 80 | - | - | 8.2 | W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\_contact\contact_final_ks_01.png, W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\_contact\contact_final_ks_02.png, W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\_contact\contact_final_ks_03.png ... |
| sequences | SKIP | - | - | - | - | no seq_* folder |
| audio_analysis | SKIP | - | - | - | - | no WAV given or found |
| tiling | PASS | 16 | 0 | 0 | 20.7 | QA/reports\tiling_final_ks.md |
| debugview | SKIP | - | - | - | - | no -debug-view in report.json and no --view |
| lookv2_check | FAIL | 80 | 52 | 28 | 65.2 | lookv2_check: 52 / 80 shots pass |
| facade_sheet | SKIP | - | - | - | - | no facade-sheet still (pose_fs*) |

## FAIL lines (111)

- **image_metrics** KS00_alley_start: NaN-like pixels (1)
- **image_metrics** KS00_alley_start: banding[alley_wall] (2.0000)
- **image_metrics** KS00_alley_start_1610: NaN-like pixels (1)
- **image_metrics** KS00_alley_start_219: NaN-like pixels (1)
- **image_metrics** KS01_alley_noodle_4k: NaN-like pixels (2)
- **image_metrics** KS02_alley_slit_1610: isolated pixels > 8x median (1)
- **image_metrics** KS04_boulevard_axis: NaN-like pixels (2)
- **image_metrics** KS04_boulevard_axis_1610: NaN-like pixels (3)
- **image_metrics** KS04_boulevard_axis_219: NaN-like pixels (3)
- **image_metrics** KS04_boulevard_axis_4k: NaN-like pixels (11)
- **image_metrics** KS04_boulevard_axis_4k: isolated pixels > 8x median (3)
- **image_metrics** KS05_boulevard_up: NaN-like pixels (3)
- **image_metrics** KS05_boulevard_up_1610: NaN-like pixels (1)
- **image_metrics** KS05_boulevard_up_219: NaN-like pixels (3)
- **image_metrics** KS05_boulevard_up_219: banding[cloud_ceiling] (1.2090)
- **image_metrics** KS05_boulevard_up_4k: NaN-like pixels (11)
- **image_metrics** KS05_boulevard_up_4k: isolated pixels > 8x median (1)
- **image_metrics** KS05_boulevard_up_4k: banding[cloud_ceiling] (2.0000)
- **image_metrics** KS06_tram: NaN-like pixels (5)
- **image_metrics** KS06_tram_1610: NaN-like pixels (4)
- **image_metrics** KS06_tram_1610: isolated pixels > 8x median (3)
- **image_metrics** KS06_tram_219: NaN-like pixels (4)
- **image_metrics** KS06_tram_4k: NaN-like pixels (15)
- **image_metrics** KS06_tram_4k: isolated pixels > 8x median (6)
- **image_metrics** KS07_edge_window: NaN-like pixels (1)
- **image_metrics** KS07_edge_window: isolated pixels > 8x median (1)
- **image_metrics** KS07_edge_window_1610: NaN-like pixels (1)
- **image_metrics** KS07_edge_window_219: NaN-like pixels (1)
- **image_metrics** KS07_edge_window_219: isolated pixels > 8x median (1)
- **image_metrics** KS07_edge_window_4k: NaN-like pixels (17)
- **image_metrics** KS07_edge_window_4k: isolated pixels > 8x median (4)
- **image_metrics** KS08_edge_flank: NaN-like pixels (5)
- **image_metrics** KS08_edge_flank_1610: NaN-like pixels (9)
- **image_metrics** KS08_edge_flank_219: NaN-like pixels (5)
- **image_metrics** KS08_edge_flank_4k: NaN-like pixels (9)
- **image_metrics** KS08_edge_flank_4k: isolated pixels > 8x median (4)
- **image_metrics** KS09_edge_gulf: NaN-like pixels (6)
- **image_metrics** KS09_edge_gulf_1610: NaN-like pixels (11)
- **image_metrics** KS09_edge_gulf_219: NaN-like pixels (6)
- **image_metrics** KS09_edge_gulf_4k: NaN-like pixels (17)
- **image_metrics** KS09_edge_gulf_4k: banding[abyss] (1.1900)
- **image_metrics** KS10_edge_plunge: NaN-like pixels (3)
- **image_metrics** KS10_edge_plunge_1610: NaN-like pixels (1)
- **image_metrics** KS10_edge_plunge_219: NaN-like pixels (3)
- **image_metrics** KS10_edge_plunge_4k: NaN-like pixels (2)
- **image_metrics** KS11_bridge_train: NaN-like pixels (6)
- **image_metrics** KS11_bridge_train: isolated pixels > 8x median (5)
- **image_metrics** KS11_bridge_train_1610: NaN-like pixels (4)
- **image_metrics** KS11_bridge_train_1610: isolated pixels > 8x median (1)
- **image_metrics** KS11_bridge_train_219: NaN-like pixels (6)
- **image_metrics** KS11_bridge_train_219: isolated pixels > 8x median (5)
- **image_metrics** KS11_bridge_train_4k: NaN-like pixels (2)
- **image_metrics** KS11_bridge_train_4k: isolated pixels > 8x median (2)
- **image_metrics** KS12_bridge_void: NaN-like pixels (5)
- **image_metrics** KS12_bridge_void: banding[abyss] (2.1390)
- **image_metrics** KS12_bridge_void_1610: NaN-like pixels (2)
- **image_metrics** KS12_bridge_void_1610: isolated pixels > 8x median (2)
- **image_metrics** KS12_bridge_void_1610: banding[abyss] (1.8580)
- **image_metrics** KS12_bridge_void_219: NaN-like pixels (6)
- **image_metrics** KS12_bridge_void_219: banding[abyss] (1.9430)
- **image_metrics** KS12_bridge_void_4k: NaN-like pixels (6)
- **image_metrics** KS12_bridge_void_4k: banding[abyss] (4.2480)
- **image_metrics** KS13_bridge_glass: NaN-like pixels (1)
- **image_metrics** KS13_bridge_glass_1610: NaN-like pixels (4)
- **image_metrics** KS13_bridge_glass_219: NaN-like pixels (1)
- **image_metrics** KS13_bridge_glass_219: banding[abyss] (1.6410)
- **image_metrics** KS13_bridge_glass_4k: NaN-like pixels (5)
- **image_metrics** KS13_bridge_glass_4k: banding[abyss] (1.5160)
- **image_metrics** KS14_garden_gate: NaN-like pixels (4)
- **image_metrics** KS14_garden_gate_1610: NaN-like pixels (3)
- **image_metrics** KS14_garden_gate_219: NaN-like pixels (3)
- **image_metrics** KS14_garden_gate_4k: NaN-like pixels (8)
- **image_metrics** KS15_garden_canopy: NaN-like pixels (8)
- **image_metrics** KS15_garden_canopy: isolated pixels > 8x median (1)
- **image_metrics** KS15_garden_canopy_1610: NaN-like pixels (5)
- **image_metrics** KS15_garden_canopy_1610: isolated pixels > 8x median (2)
- **image_metrics** KS15_garden_canopy_219: NaN-like pixels (8)
- **image_metrics** KS15_garden_canopy_219: isolated pixels > 8x median (1)
- **image_metrics** KS15_garden_canopy_4k: NaN-like pixels (18)
- **image_metrics** KS15_garden_canopy_4k: isolated pixels > 8x median (1)
- **image_metrics** KS16_signs_wall: NaN-like pixels (2)
- **image_metrics** KS16_signs_wall_219: NaN-like pixels (4)
- **image_metrics** KS16_signs_wall_4k: NaN-like pixels (2)
- **lookv2_check** KS00_alley_start.png       b1 FAIL texture_1_meso=0.87
- **lookv2_check** KS00_alley_start_1610.png  b1 FAIL texture_1_meso=0.89
- **lookv2_check** KS00_alley_start_219.png   b1 FAIL texture_0_meso=0.87 texture_0_pool=0.33 texture_1_meso=0.92
- **lookv2_check** KS00_alley_start_4k.png    b1 FAIL texture_1_meso=0.93
- **lookv2_check** KS01_alley_noodle_219.png  b1 FAIL texture_0_micro=1.27
- **lookv2_check** KS02_alley_slit.png        b1 FAIL texture_0_micro=1.33 texture_0_meso=0.41 texture_1_meso=0.42
- **lookv2_check** KS02_alley_slit_1610.png   b1 FAIL texture_0_micro=1.29 texture_0_meso=0.38 texture_1_micro=1.39 texture_1_meso=0.36
- **lookv2_check** KS02_alley_slit_219.png    b1 FAIL p99_luma=74.6 texture_0_micro=1.38 texture_0_meso=0.49 texture_1_meso=0.73 texture_1_pool=0.31
- **lookv2_check** KS02_alley_slit_4k.png     b1 FAIL texture_0_meso=0.52 texture_1_meso=0.64
- **lookv2_check** KS11_bridge_train_1610.png b4 FAIL p99_luma=116.7
- **lookv2_check** KS13_bridge_glass.png      b4 FAIL p99_luma=73.1
- **lookv2_check** KS13_bridge_glass_1610.png b4 FAIL p99_luma=74.0
- **lookv2_check** KS13_bridge_glass_219.png  b4 FAIL p99_luma=77.5
- **lookv2_check** KS13_bridge_glass_4k.png   b4 FAIL p99_luma=73.9
- **lookv2_check** KS15_garden_canopy_219.png b5 FAIL p99_luma=111.3
- **lookv2_check** KS15_garden_canopy_4k.png  b5 FAIL p99_luma=115.7
- **lookv2_check** KS17_heart_reveal.png      b7 FAIL koi_patch_hue=9.5 koi_patch_sat=0.52
- **lookv2_check** KS17_heart_reveal_1610.png b7 FAIL koi_patch_hue=9.6 koi_patch_sat=0.52
- **lookv2_check** KS17_heart_reveal_219.png  b7 FAIL koi_patch_hue=9.5 koi_patch_sat=0.52
- **lookv2_check** KS17_heart_reveal_4k.png   b7 FAIL koi_patch_hue=9.5 koi_patch_sat=0.52
- **lookv2_check** KS18_koi_pass.png          b7 FAIL koi_patch_hue=9.1 koi_patch_sat=0.52
- **lookv2_check** KS18_koi_pass_1610.png     b7 FAIL koi_patch_hue=9.1 koi_patch_sat=0.52
- **lookv2_check** KS18_koi_pass_219.png      b7 FAIL koi_patch_hue=9.1 koi_patch_sat=0.52
- **lookv2_check** KS18_koi_pass_4k.png       b7 FAIL koi_patch_hue=9.0 koi_patch_sat=0.52
- **lookv2_check** KS19_goal_window.png       b7 FAIL mean_luma=32.2
- **lookv2_check** KS19_goal_window_1610.png  b7 FAIL mean_luma=32.1
- **lookv2_check** KS19_goal_window_219.png   b7 FAIL mean_luma=32.2
- **lookv2_check** KS19_goal_window_4k.png    b7 FAIL mean_luma=32.3

## Contact sheets (open them with the Read tool, Part 11.4)

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\_contact\contact_final_ks_01.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\_contact\contact_final_ks_02.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\_contact\contact_final_ks_03.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\final_ks\_contact\contact_final_ks_04.png`
