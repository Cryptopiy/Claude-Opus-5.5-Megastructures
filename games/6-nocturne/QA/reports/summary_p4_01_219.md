# QA summary: p4_01_219

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219`

**Overall: FAIL**

| Step | Status | Items | PASS | FAIL | Seconds | Report |
|---|---|---|---|---|---|---|
| image_metrics | FAIL | 20 | 158 | 37 | 6.9 | QA/reports\image_metrics_p4_01_219.md |
| contact_sheet | PASS | 20 | - | - | 1.7 | W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\_contact\contact_p4_01_219_01.png |
| sequences | SKIP | - | - | - | - | no seq_* folder |
| audio_analysis | SKIP | - | - | - | - | no WAV given or found |
| tiling | PASS | 4 | 0 | 0 | 3.9 | QA/reports\tiling_p4_01_219.md |
| debugview | SKIP | - | - | - | - | no -debug-view in report.json and no --view |
| lookv2_check | FAIL | 20 | 12 | 8 | 14.2 | lookv2_check: 12 / 20 shots pass |
| facade_sheet | SKIP | - | - | - | - | no facade-sheet still (pose_fs*) |

## FAIL lines (45)

- **image_metrics** KS00_alley_start_219: NaN-like pixels (2)
- **image_metrics** KS00_alley_start_219: isolated pixels > 8x median (2)
- **image_metrics** KS01_alley_noodle_219: NaN-like pixels (61)
- **image_metrics** KS01_alley_noodle_219: mean luma (0.1600)
- **image_metrics** KS02_alley_slit_219: NaN-like pixels (6)
- **image_metrics** KS03_boulevard_mouth_219: NaN-like pixels (1)
- **image_metrics** KS04_boulevard_axis_219: NaN-like pixels (27)
- **image_metrics** KS04_boulevard_axis_219: isolated pixels > 8x median (4)
- **image_metrics** KS05_boulevard_up_219: NaN-like pixels (6)
- **image_metrics** KS05_boulevard_up_219: isolated pixels > 8x median (1)
- **image_metrics** KS05_boulevard_up_219: banding[cloud_ceiling] (1.2090)
- **image_metrics** KS06_tram_219: NaN-like pixels (13)
- **image_metrics** KS06_tram_219: isolated pixels > 8x median (3)
- **image_metrics** KS07_edge_window_219: NaN-like pixels (4)
- **image_metrics** KS07_edge_window_219: isolated pixels > 8x median (1)
- **image_metrics** KS08_edge_flank_219: NaN-like pixels (4)
- **image_metrics** KS08_edge_flank_219: isolated pixels > 8x median (6)
- **image_metrics** KS09_edge_gulf_219: NaN-like pixels (8)
- **image_metrics** KS09_edge_gulf_219: isolated pixels > 8x median (2)
- **image_metrics** KS10_edge_plunge_219: NaN-like pixels (5)
- **image_metrics** KS10_edge_plunge_219: banding[abyss] (4.4190)
- **image_metrics** KS11_bridge_train_219: NaN-like pixels (1)
- **image_metrics** KS11_bridge_train_219: isolated pixels > 8x median (2)
- **image_metrics** KS12_bridge_void_219: NaN-like pixels (4)
- **image_metrics** KS12_bridge_void_219: isolated pixels > 8x median (1)
- **image_metrics** KS12_bridge_void_219: banding[abyss] (2.5860)
- **image_metrics** KS13_bridge_glass_219: banding[abyss] (1.2800)
- **image_metrics** KS14_garden_gate_219: NaN-like pixels (22)
- **image_metrics** KS14_garden_gate_219: isolated pixels > 8x median (2)
- **image_metrics** KS15_garden_canopy_219: NaN-like pixels (20)
- **image_metrics** KS15_garden_canopy_219: isolated pixels > 8x median (2)
- **image_metrics** KS16_signs_wall_219: NaN-like pixels (9)
- **image_metrics** KS16_signs_wall_219: isolated pixels > 8x median (2)
- **image_metrics** KS16_signs_wall_219: banding[wall_panel] (7.9920)
- **image_metrics** KS17_heart_reveal_219: NaN-like pixels (1)
- **image_metrics** KS18_koi_pass_219: isolated pixels > 8x median (51)
- **image_metrics** KS19_goal_window_219: NaN-like pixels (12)
- **lookv2_check** KS00_alley_start_219.png   b1 FAIL texture_0_meso=0.87 texture_0_pool=0.36 texture_1_meso=0.98
- **lookv2_check** KS01_alley_noodle_219.png  b1 FAIL mean_luma=40.8 texture_0_micro=1.3
- **lookv2_check** KS02_alley_slit_219.png    b1 FAIL p99_luma=74.4 texture_0_micro=1.29 texture_0_meso=0.44 texture_0_pool=0.29 texture_1_meso=0.6 texture_1_pool=0.05
- **lookv2_check** KS03_boulevard_mouth_219.png b1 FAIL mean_luma=22.9
- **lookv2_check** KS10_edge_plunge_219.png   b3 FAIL mean_luma=31.8
- **lookv2_check** KS13_bridge_glass_219.png  b4 FAIL p99_luma=79.4
- **lookv2_check** KS14_garden_gate_219.png   b4 FAIL p99_luma=104.9
- **lookv2_check** KS19_goal_window_219.png   b7 FAIL texture_1_meso=0.95

## Contact sheets (open them with the Read tool, Part 11.4)

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_219\_contact\contact_p4_01_219_01.png`
