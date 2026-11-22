# QA summary: p4_01

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01`

**Overall: FAIL**

| Step | Status | Items | PASS | FAIL | Seconds | Report |
|---|---|---|---|---|---|---|
| image_metrics | FAIL | 20 | 161 | 34 | 5.5 | QA/reports\image_metrics_p4_01.md |
| contact_sheet | PASS | 20 | - | - | 1.6 | W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01\_contact\contact_p4_01_01.png |
| sequences | SKIP | - | - | - | - | no seq_* folder |
| audio_analysis | SKIP | - | - | - | - | no WAV given or found |
| tiling | PASS | 4 | 0 | 0 | 3.2 | QA/reports\tiling_p4_01.md |
| debugview | SKIP | - | - | - | - | no -debug-view in report.json and no --view |
| lookv2_check | FAIL | 20 | 14 | 6 | 10.5 | lookv2_check: 14 / 20 shots pass |
| facade_sheet | SKIP | - | - | - | - | no facade-sheet still (pose_fs*) |

## FAIL lines (40)

- **image_metrics** KS00_alley_start: NaN-like pixels (2)
- **image_metrics** KS00_alley_start: isolated pixels > 8x median (2)
- **image_metrics** KS01_alley_noodle: NaN-like pixels (10)
- **image_metrics** KS03_boulevard_mouth: NaN-like pixels (1)
- **image_metrics** KS04_boulevard_axis: NaN-like pixels (26)
- **image_metrics** KS04_boulevard_axis: isolated pixels > 8x median (2)
- **image_metrics** KS05_boulevard_up: NaN-like pixels (6)
- **image_metrics** KS05_boulevard_up: isolated pixels > 8x median (1)
- **image_metrics** KS06_tram: NaN-like pixels (11)
- **image_metrics** KS06_tram: isolated pixels > 8x median (2)
- **image_metrics** KS07_edge_window: NaN-like pixels (4)
- **image_metrics** KS07_edge_window: isolated pixels > 8x median (2)
- **image_metrics** KS08_edge_flank: NaN-like pixels (2)
- **image_metrics** KS08_edge_flank: isolated pixels > 8x median (4)
- **image_metrics** KS09_edge_gulf: NaN-like pixels (9)
- **image_metrics** KS09_edge_gulf: isolated pixels > 8x median (2)
- **image_metrics** KS10_edge_plunge: NaN-like pixels (4)
- **image_metrics** KS11_bridge_train: NaN-like pixels (1)
- **image_metrics** KS11_bridge_train: isolated pixels > 8x median (2)
- **image_metrics** KS12_bridge_void: NaN-like pixels (4)
- **image_metrics** KS12_bridge_void: isolated pixels > 8x median (1)
- **image_metrics** KS12_bridge_void: banding[abyss] (1.4850)
- **image_metrics** KS13_bridge_glass: NaN-like pixels (1)
- **image_metrics** KS13_bridge_glass: banding[abyss] (1.3370)
- **image_metrics** KS14_garden_gate: NaN-like pixels (21)
- **image_metrics** KS14_garden_gate: isolated pixels > 8x median (2)
- **image_metrics** KS15_garden_canopy: NaN-like pixels (18)
- **image_metrics** KS15_garden_canopy: isolated pixels > 8x median (1)
- **image_metrics** KS16_signs_wall: NaN-like pixels (7)
- **image_metrics** KS16_signs_wall: isolated pixels > 8x median (2)
- **image_metrics** KS16_signs_wall: banding[wall_panel] (2.0330)
- **image_metrics** KS17_heart_reveal: NaN-like pixels (1)
- **image_metrics** KS18_koi_pass: isolated pixels > 8x median (51)
- **image_metrics** KS19_goal_window: NaN-like pixels (11)
- **lookv2_check** KS00_alley_start.png       b1 FAIL mean_luma=30.5 texture_1_meso=0.93
- **lookv2_check** KS01_alley_noodle.png      b1 FAIL mean_luma=39.5
- **lookv2_check** KS02_alley_slit.png        b1 FAIL texture_0_micro=1.22 texture_0_meso=0.36 texture_0_pool=0.14 texture_1_micro=1.4 texture_1_meso=0.36 texture_1_pool=0.01
- **lookv2_check** KS13_bridge_glass.png      b4 FAIL p99_luma=83.9
- **lookv2_check** KS14_garden_gate.png       b4 FAIL p99_luma=107.9
- **lookv2_check** KS15_garden_canopy.png     b5 FAIL dark_cast_RG=-4.76

## Contact sheets (open them with the Read tool, Part 11.4)

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01\_contact\contact_p4_01_01.png`
