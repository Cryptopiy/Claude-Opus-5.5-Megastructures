# QA summary: p4_01_1610

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610`

**Overall: FAIL**

| Step | Status | Items | PASS | FAIL | Seconds | Report |
|---|---|---|---|---|---|---|
| image_metrics | FAIL | 20 | 164 | 31 | 5.4 | QA/reports\image_metrics_p4_01_1610.md |
| contact_sheet | PASS | 20 | - | - | 1.3 | W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\_contact\contact_p4_01_1610_01.png |
| sequences | SKIP | - | - | - | - | no seq_* folder |
| audio_analysis | SKIP | - | - | - | - | no WAV given or found |
| tiling | PASS | 4 | 0 | 0 | 2.8 | QA/reports\tiling_p4_01_1610.md |
| debugview | SKIP | - | - | - | - | no -debug-view in report.json and no --view |
| lookv2_check | FAIL | 20 | 14 | 6 | 8.9 | lookv2_check: 14 / 20 shots pass |
| facade_sheet | SKIP | - | - | - | - | no facade-sheet still (pose_fs*) |

## FAIL lines (37)

- **image_metrics** KS00_alley_start_1610: NaN-like pixels (3)
- **image_metrics** KS01_alley_noodle_1610: NaN-like pixels (4)
- **image_metrics** KS02_alley_slit_1610: isolated pixels > 8x median (1)
- **image_metrics** KS03_boulevard_mouth_1610: NaN-like pixels (2)
- **image_metrics** KS03_boulevard_mouth_1610: isolated pixels > 8x median (5)
- **image_metrics** KS04_boulevard_axis_1610: NaN-like pixels (37)
- **image_metrics** KS04_boulevard_axis_1610: isolated pixels > 8x median (7)
- **image_metrics** KS05_boulevard_up_1610: NaN-like pixels (2)
- **image_metrics** KS05_boulevard_up_1610: isolated pixels > 8x median (2)
- **image_metrics** KS06_tram_1610: NaN-like pixels (8)
- **image_metrics** KS06_tram_1610: isolated pixels > 8x median (3)
- **image_metrics** KS07_edge_window_1610: NaN-like pixels (5)
- **image_metrics** KS07_edge_window_1610: isolated pixels > 8x median (2)
- **image_metrics** KS08_edge_flank_1610: NaN-like pixels (4)
- **image_metrics** KS08_edge_flank_1610: isolated pixels > 8x median (1)
- **image_metrics** KS09_edge_gulf_1610: NaN-like pixels (11)
- **image_metrics** KS09_edge_gulf_1610: isolated pixels > 8x median (1)
- **image_metrics** KS10_edge_plunge_1610: NaN-like pixels (3)
- **image_metrics** KS11_bridge_train_1610: NaN-like pixels (1)
- **image_metrics** KS12_bridge_void_1610: NaN-like pixels (2)
- **image_metrics** KS12_bridge_void_1610: isolated pixels > 8x median (1)
- **image_metrics** KS12_bridge_void_1610: banding[abyss] (1.6750)
- **image_metrics** KS13_bridge_glass_1610: banding[abyss] (1.0590)
- **image_metrics** KS14_garden_gate_1610: NaN-like pixels (21)
- **image_metrics** KS14_garden_gate_1610: isolated pixels > 8x median (2)
- **image_metrics** KS15_garden_canopy_1610: NaN-like pixels (22)
- **image_metrics** KS15_garden_canopy_1610: isolated pixels > 8x median (3)
- **image_metrics** KS16_signs_wall_1610: banding[wall_panel] (5.2080)
- **image_metrics** KS17_heart_reveal_1610: NaN-like pixels (3)
- **image_metrics** KS18_koi_pass_1610: isolated pixels > 8x median (19)
- **image_metrics** KS19_goal_window_1610: NaN-like pixels (14)
- **lookv2_check** KS00_alley_start_1610.png  b1 FAIL mean_luma=30.9 texture_1_meso=0.96
- **lookv2_check** KS01_alley_noodle_1610.png b1 FAIL mean_luma=38.0
- **lookv2_check** KS02_alley_slit_1610.png   b1 FAIL texture_0_micro=1.19 texture_0_meso=0.33 texture_0_pool=0.16 texture_1_micro=1.28 texture_1_meso=0.3 texture_1_pool=0.0
- **lookv2_check** KS13_bridge_glass_1610.png b4 FAIL p99_luma=86.6
- **lookv2_check** KS14_garden_gate_1610.png  b4 FAIL p99_luma=107.1
- **lookv2_check** KS15_garden_canopy_1610.png b5 FAIL dark_cast_BG=-2.23 dark_cast_RG=-5.3

## Contact sheets (open them with the Read tool, Part 11.4)

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_1610\_contact\contact_p4_01_1610_01.png`
