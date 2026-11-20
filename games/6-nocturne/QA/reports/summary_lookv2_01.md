# QA summary: lookv2_01

Capture folder: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01`

**Overall: FAIL**

| Step | Status | Items | PASS | FAIL | Seconds | Report |
|---|---|---|---|---|---|---|
| image_metrics | FAIL | 20 | 176 | 19 | 17.9 | W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\reports\image_metrics_lookv2_01.md |
| contact_sheet | PASS | 20 | - | - | 1.4 | W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01\_contact\contact_lookv2_01_01.png |
| sequences | SKIP | - | - | - | - | no seq_* folder |
| audio_analysis | SKIP | - | - | - | - | no WAV given or found |
| tiling | PASS | 4 | 0 | 0 | 3.0 | W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\reports\tiling_lookv2_01.md |
| debugview | SKIP | - | - | - | - | no -debug-view in report.json and no --view |
| lookv2_check | FAIL | 20 | 8 | 12 | 9.8 | lookv2_check: 8 / 20 shots pass |
| facade_sheet | SKIP | - | - | - | - | no facade-sheet still (pose_fs*) |

## FAIL lines (31)

- **image_metrics** KS00_alley_start: banding[mouth_glow] (1.2850)
- **image_metrics** KS03_boulevard_mouth: isolated pixels > 8x median (1)
- **image_metrics** KS03_boulevard_mouth: banding[alley_wall_dark] (1.5120)
- **image_metrics** KS04_boulevard_axis: NaN-like pixels (5)
- **image_metrics** KS04_boulevard_axis: isolated pixels > 8x median (1)
- **image_metrics** KS05_boulevard_up: NaN-like pixels (6)
- **image_metrics** KS05_boulevard_up: isolated pixels > 8x median (1)
- **image_metrics** KS06_tram: NaN-like pixels (1)
- **image_metrics** KS06_tram: isolated pixels > 8x median (1)
- **image_metrics** KS07_edge_window: NaN-like pixels (4)
- **image_metrics** KS07_edge_window: isolated pixels > 8x median (3)
- **image_metrics** KS11_bridge_train: NaN-like pixels (1)
- **image_metrics** KS11_bridge_train: isolated pixels > 8x median (2)
- **image_metrics** KS13_bridge_glass: banding[abyss] (1.9320)
- **image_metrics** KS15_garden_canopy: mean luma (0.0669)
- **image_metrics** KS16_signs_wall: NaN-like pixels (3)
- **image_metrics** KS17_heart_reveal: banding[plaza] (2.0000)
- **image_metrics** KS18_koi_pass: isolated pixels > 8x median (73)
- **image_metrics** KS19_goal_window: NaN-like pixels (8)
- **lookv2_check** KS00_alley_start.png       b1 FAIL dark_cast_RG=4.67 texture_0_meso=0.91 texture_0_pool=0.33 texture_1_micro=1.35 texture_1_meso=0.48 texture_1_pool=0.36
- **lookv2_check** KS01_alley_noodle.png      b1 FAIL dark_cast_RG=4.94 texture_0_micro=1.3 texture_0_meso=0.39
- **lookv2_check** KS02_alley_slit.png        b1 FAIL texture_0_micro=1.19 texture_0_meso=0.33 texture_0_pool=0.0 texture_1_micro=1.39 texture_1_meso=0.36 texture_1_pool=0.01
- **lookv2_check** KS07_edge_window.png       b3 FAIL p99_luma=83.9 separation=1.1
- **lookv2_check** KS08_edge_flank.png        b3 FAIL p99_luma=92.1
- **lookv2_check** KS09_edge_gulf.png         b3 FAIL p99_luma=79.4 p999_luma=178.7
- **lookv2_check** KS10_edge_plunge.png       b3 FAIL mean_luma=28.6 p99_luma=49.4 p999_luma=133.0
- **lookv2_check** KS11_bridge_train.png      b4 FAIL p99_luma=92.9
- **lookv2_check** KS12_bridge_void.png       b4 FAIL mean_luma=29.5 p99_luma=78.1
- **lookv2_check** KS14_garden_gate.png       b4 FAIL p99_luma=77.2
- **lookv2_check** KS15_garden_canopy.png     b5 FAIL mean_luma=17.0 p99_luma=44.4
- **lookv2_check** KS19_goal_window.png       b7 FAIL p999_luma=198.6 texture_1_meso=0.62

## Contact sheets (open them with the Read tool, Part 11.4)

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01\_contact\contact_lookv2_01_01.png`
