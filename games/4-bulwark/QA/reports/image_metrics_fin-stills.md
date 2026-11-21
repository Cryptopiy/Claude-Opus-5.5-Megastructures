# Image metrics: fin-stills

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills`  
Scene constants: BulwarkConst.cs  
Images: 198, failing images: 38. Checks: 1555 PASS, 38 FAIL, 0 WARN, 0 SKIP, 3 INFO.

**Result: FAIL**

## Input warnings

- parity_window.png is not listed in report.json

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_WaveGallery_KS01_gal_far | 1 | 1920x1080 | 0.172 | 0.086 | 0.0003 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_1610 | 1 | 1680x1050 | 0.176 | 0.088 | 0.0005 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_219 | 1 | 2560x1080 | 0.160 | 0.079 | 0.0003 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_4k | 1 | 3840x2160 | 0.172 | 0.085 | 0.0006 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit | 1 | 1920x1080 | 0.179 | 0.093 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_1610 | 1 | 1680x1050 | 0.187 | 0.097 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_219 | 1 | 2560x1080 | 0.160 | 0.080 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_4k | 1 | 3840x2160 | 0.179 | 0.093 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS03_door_open | 1 | 1920x1080 | 0.346 | 0.117 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS03_door_open_1610 | 1 | 1680x1050 | 0.359 | 0.117 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS03_door_open_219 | 1 | 2560x1080 | 0.314 | 0.113 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS03_door_open_4k | 1 | 3840x2160 | 0.346 | 0.117 | 0.0000 | - | PASS |  |
| b01_WaveGallery_end | 1 | 1920x1080 | 0.450 | 0.261 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_look_back | 1 | 1920x1080 | 0.122 | 0.078 | 0.0006 | - | PASS |  |
| b01_WaveGallery_look_down | 1 | 1920x1080 | 0.136 | 0.063 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_l | 1 | 1920x1080 | 0.157 | 0.078 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_r | 1 | 1920x1080 | 0.164 | 0.105 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_up | 1 | 1920x1080 | 0.218 | 0.089 | 0.0000 | - | PASS |  |
| b01_WaveGallery_mid | 1 | 1920x1080 | 0.122 | 0.079 | 0.0006 | - | PASS |  |
| b01_WaveGallery_start | 1 | 1920x1080 | 0.114 | 0.063 | 0.0004 | - | PASS |  |
| b02_Crest_KK1_arm_tower | 2 | 1920x1080 | 0.509 | 0.330 | 0.0000 | - | PASS |  |
| b02_Crest_KK1_arm_tower_1610 | 2 | 1680x1050 | 0.509 | 0.330 | 0.0000 | - | PASS |  |
| b02_Crest_KK1_arm_tower_219 | 2 | 2560x1080 | 0.507 | 0.330 | 0.0000 | - | PASS |  |
| b02_Crest_KK1_arm_tower_4k | 2 | 3840x2160 | 0.509 | 0.330 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea | 2 | 1920x1080 | 0.492 | 0.297 | 0.0000 | 0.500 | PASS |  |
| b02_Crest_KS04_crest_sea_1610 | 2 | 1680x1050 | 0.493 | 0.297 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea_219 | 2 | 2560x1080 | 0.491 | 0.297 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea_4k | 2 | 3840x2160 | 0.493 | 0.297 | 0.0000 | 0.333 | FAIL | NaN-like pixels |
| b02_Crest_KS05_crest_drop | 2 | 1920x1080 | 0.398 | 0.268 | 0.0000 | 0.500 | PASS |  |
| b02_Crest_KS05_crest_drop_1610 | 2 | 1680x1050 | 0.399 | 0.268 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_219 | 2 | 2560x1080 | 0.396 | 0.266 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_4k | 2 | 3840x2160 | 0.398 | 0.268 | 0.0000 | 0.250 | PASS |  |
| b02_Crest_KS06_crest_axis | 2 | 1920x1080 | 0.502 | 0.322 | 0.0000 | - | PASS |  |
| b02_Crest_KS06_crest_axis_1610 | 2 | 1680x1050 | 0.502 | 0.321 | 0.0000 | - | PASS |  |
| b02_Crest_KS06_crest_axis_219 | 2 | 2560x1080 | 0.500 | 0.322 | 0.0000 | - | PASS |  |
| b02_Crest_KS06_crest_axis_4k | 2 | 3840x2160 | 0.502 | 0.321 | 0.0000 | - | PASS |  |
| b02_Crest_end | 2 | 1920x1080 | 0.457 | 0.256 | 0.0000 | - | PASS |  |
| b02_Crest_look_back | 2 | 1920x1080 | 0.501 | 0.312 | 0.0000 | - | PASS |  |
| b02_Crest_look_down | 2 | 1920x1080 | 0.313 | 0.279 | 0.0000 | - | FAIL | mean luma |
| b02_Crest_look_l | 2 | 1920x1080 | 0.495 | 0.310 | 0.0000 | - | PASS |  |
| b02_Crest_look_r | 2 | 1920x1080 | 0.504 | 0.318 | 0.0000 | - | PASS |  |
| b02_Crest_look_up | 2 | 1920x1080 | 0.535 | 0.505 | 0.0000 | - | PASS |  |
| b02_Crest_mid | 2 | 1920x1080 | 0.498 | 0.320 | 0.0000 | - | PASS |  |
| b02_Crest_start | 2 | 1920x1080 | 0.504 | 0.326 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS07_spill_gates | 3 | 1920x1080 | 0.476 | 0.264 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS07_spill_gates_1610 | 3 | 1680x1050 | 0.477 | 0.263 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS07_spill_gates_219 | 3 | 2560x1080 | 0.466 | 0.264 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS07_spill_gates_4k | 3 | 3840x2160 | 0.476 | 0.260 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow | 3 | 1920x1080 | 0.474 | 0.229 | 0.0000 | 0.500 | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_1610 | 3 | 1680x1050 | 0.478 | 0.229 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_219 | 3 | 2560x1080 | 0.467 | 0.226 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_4k | 3 | 3840x2160 | 0.474 | 0.229 | 0.0000 | 0.250 | PASS |  |
| b03_SpillwayBridge_KS09_spill_grate | 3 | 1920x1080 | 0.379 | 0.146 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS09_spill_grate_1610 | 3 | 1680x1050 | 0.381 | 0.146 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS09_spill_grate_219 | 3 | 2560x1080 | 0.367 | 0.134 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS09_spill_grate_4k | 3 | 3840x2160 | 0.378 | 0.146 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_end | 3 | 1920x1080 | 0.427 | 0.248 | 0.0001 | - | FAIL | NaN-like pixels |
| b03_SpillwayBridge_look_back | 3 | 1920x1080 | 0.426 | 0.189 | 0.0000 | - | FAIL | NaN-like pixels |
| b03_SpillwayBridge_look_down | 3 | 1920x1080 | 0.305 | 0.238 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_look_down90 | 3 | 1920x1080 | 0.332 | 0.228 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_look_l | 3 | 1920x1080 | 0.476 | 0.342 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_look_r | 3 | 1920x1080 | 0.441 | 0.233 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_look_up | 3 | 1920x1080 | 0.453 | 0.421 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_mid | 3 | 1920x1080 | 0.415 | 0.207 | 0.0000 | - | FAIL | NaN-like pixels |
| b03_SpillwayBridge_start | 3 | 1920x1080 | 0.467 | 0.211 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KK2_head_eyes | 4 | 1920x1080 | 0.393 | 0.126 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KK2_head_eyes_1610 | 4 | 1680x1050 | 0.389 | 0.128 | 0.0000 | - | FAIL | mean luma |
| b04_ArmourSlope_KK2_head_eyes_219 | 4 | 2560x1080 | 0.407 | 0.127 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KK2_head_eyes_4k | 4 | 3840x2160 | 0.395 | 0.126 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS10_reef_wide | 4 | 1920x1080 | 0.398 | 0.140 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide_1610 | 4 | 1680x1050 | 0.395 | 0.142 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide_219 | 4 | 2560x1080 | 0.409 | 0.145 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide_4k | 4 | 3840x2160 | 0.400 | 0.139 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS11_reef_spray | 4 | 1920x1080 | 0.512 | 0.180 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_1610 | 4 | 1680x1050 | 0.515 | 0.177 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_219 | 4 | 2560x1080 | 0.499 | 0.149 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS11_reef_spray_4k | 4 | 3840x2160 | 0.512 | 0.179 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up | 4 | 1920x1080 | 0.486 | 0.222 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_1610 | 4 | 1680x1050 | 0.486 | 0.226 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_219 | 4 | 2560x1080 | 0.487 | 0.222 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_4k | 4 | 3840x2160 | 0.486 | 0.222 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_end | 4 | 1920x1080 | 0.370 | 0.070 | 0.0000 | - | FAIL | mean luma |
| b04_ArmourSlope_look_back | 4 | 1920x1080 | 0.607 | 0.190 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_down | 4 | 1920x1080 | 0.617 | 0.186 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_l | 4 | 1920x1080 | 0.610 | 0.319 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_r | 4 | 1920x1080 | 0.387 | 0.132 | 0.0000 | - | FAIL | mean luma |
| b04_ArmourSlope_look_up | 4 | 1920x1080 | 0.411 | 0.353 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_mid | 4 | 1920x1080 | 0.524 | 0.184 | 0.0003 | - | PASS |  |
| b04_ArmourSlope_start | 4 | 1920x1080 | 0.438 | 0.253 | 0.0001 | - | PASS |  |
| b05_GateTower_KS13_atrium | 5 | 1920x1080 | 0.154 | 0.066 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium_1610 | 5 | 1680x1050 | 0.151 | 0.066 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium_219 | 5 | 2560x1080 | 0.165 | 0.067 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium_4k | 5 | 3840x2160 | 0.154 | 0.066 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down | 5 | 1920x1080 | 0.194 | 0.091 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_1610 | 5 | 1680x1050 | 0.197 | 0.091 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_219 | 5 | 2560x1080 | 0.186 | 0.091 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_4k | 5 | 3840x2160 | 0.194 | 0.091 | 0.0000 | - | PASS |  |
| b05_GateTower_end | 5 | 1920x1080 | 0.163 | 0.058 | 0.0000 | - | FAIL | NaN-like pixels |
| b05_GateTower_look_back | 5 | 1920x1080 | 0.124 | 0.053 | 0.0000 | - | PASS |  |
| b05_GateTower_look_down | 5 | 1920x1080 | 0.172 | 0.095 | 0.0000 | - | PASS |  |
| b05_GateTower_look_l | 5 | 1920x1080 | 0.143 | 0.064 | 0.0040 | - | PASS |  |
| b05_GateTower_look_r | 5 | 1920x1080 | 0.189 | 0.104 | 0.0000 | - | PASS |  |
| b05_GateTower_look_up | 5 | 1920x1080 | 0.125 | 0.083 | 0.0000 | - | PASS |  |
| b05_GateTower_mid | 5 | 1920x1080 | 0.127 | 0.083 | 0.0000 | - | PASS |  |
| b05_GateTower_start | 5 | 1920x1080 | 0.281 | 0.070 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KK4_arm_glass | 6 | 1920x1080 | 0.202 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KK4_arm_glass_1610 | 6 | 1680x1050 | 0.202 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KK4_arm_glass_219 | 6 | 2560x1080 | 0.198 | 0.035 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KK4_arm_glass_4k | 6 | 3840x2160 | 0.202 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first | 6 | 1920x1080 | 0.293 | 0.035 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_1610 | 6 | 1680x1050 | 0.304 | 0.103 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_219 | 6 | 2560x1080 | 0.235 | 0.030 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_4k | 6 | 3840x2160 | 0.293 | 0.035 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up | 6 | 1920x1080 | 0.192 | 0.035 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_1610 | 6 | 1680x1050 | 0.205 | 0.035 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_219 | 6 | 2560x1080 | 0.164 | 0.034 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_4k | 6 | 3840x2160 | 0.193 | 0.035 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_eye | 6 | 1920x1080 | 0.187 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_eye_1610 | 6 | 1680x1050 | 0.195 | 0.039 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_eye_219 | 6 | 2560x1080 | 0.174 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_eye_4k | 6 | 3840x2160 | 0.187 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down | 6 | 1920x1080 | 0.128 | 0.046 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_1610 | 6 | 1680x1050 | 0.127 | 0.046 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_219 | 6 | 2560x1080 | 0.128 | 0.043 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_4k | 6 | 3840x2160 | 0.128 | 0.046 | 0.0000 | - | PASS |  |
| b06_WindowGallery_end | 6 | 1920x1080 | 0.157 | 0.093 | 0.0028 | - | PASS |  |
| b06_WindowGallery_look_back | 6 | 1920x1080 | 0.153 | 0.043 | 0.0001 | - | PASS |  |
| b06_WindowGallery_look_down | 6 | 1920x1080 | 0.111 | 0.092 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_l | 6 | 1920x1080 | 0.159 | 0.100 | 0.0163 | - | PASS |  |
| b06_WindowGallery_look_r | 6 | 1920x1080 | 0.124 | 0.074 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_up | 6 | 1920x1080 | 0.149 | 0.043 | 0.0000 | - | PASS |  |
| b06_WindowGallery_mid | 6 | 1920x1080 | 0.165 | 0.043 | 0.0000 | - | PASS |  |
| b06_WindowGallery_start | 6 | 1920x1080 | 0.184 | 0.050 | 0.0001 | - | PASS |  |
| b07_SluiceLedge_KS19_sluice | 7 | 1920x1080 | 0.144 | 0.024 | 0.0007 | - | FAIL | fireflies |
| b07_SluiceLedge_KS19_sluice_1610 | 7 | 1680x1050 | 0.141 | 0.024 | 0.0006 | - | FAIL | fireflies |
| b07_SluiceLedge_KS19_sluice_219 | 7 | 2560x1080 | 0.168 | 0.025 | 0.0005 | - | FAIL | fireflies |
| b07_SluiceLedge_KS19_sluice_4k | 7 | 3840x2160 | 0.143 | 0.024 | 0.0007 | - | FAIL | fireflies |
| b07_SluiceLedge_KS20_ledge_world | 7 | 1920x1080 | 0.289 | 0.067 | 0.0000 | 0.333 | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_1610 | 7 | 1680x1050 | 0.290 | 0.066 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_219 | 7 | 2560x1080 | 0.286 | 0.068 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_4k | 7 | 3840x2160 | 0.288 | 0.067 | 0.0000 | 0.250 | PASS |  |
| b07_SluiceLedge_KS21_beacon | 7 | 1920x1080 | 0.255 | 0.031 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_1610 | 7 | 1680x1050 | 0.249 | 0.030 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_219 | 7 | 2560x1080 | 0.271 | 0.032 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_4k | 7 | 3840x2160 | 0.255 | 0.031 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS22_ride_drop | 7 | 1920x1080 | 0.180 | 0.041 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS22_ride_drop_1610 | 7 | 1680x1050 | 0.179 | 0.041 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS22_ride_drop_219 | 7 | 2560x1080 | 0.183 | 0.039 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS22_ride_drop_4k | 7 | 3840x2160 | 0.179 | 0.041 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS23_ride_sunset | 7 | 1920x1080 | 0.269 | 0.041 | 0.0009 | - | PASS |  |
| b07_SluiceLedge_KS23_ride_sunset_1610 | 7 | 1680x1050 | 0.270 | 0.040 | 0.0012 | - | PASS |  |
| b07_SluiceLedge_KS23_ride_sunset_219 | 7 | 2560x1080 | 0.265 | 0.041 | 0.0007 | - | PASS |  |
| b07_SluiceLedge_KS23_ride_sunset_4k | 7 | 3840x2160 | 0.268 | 0.041 | 0.0010 | - | PASS |  |
| b07_SluiceLedge_KS24_ride_lookup | 7 | 1920x1080 | 0.195 | 0.031 | 0.0000 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS24_ride_lookup_1610 | 7 | 1680x1050 | 0.190 | 0.031 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS24_ride_lookup_219 | 7 | 2560x1080 | 0.207 | 0.031 | 0.0000 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS24_ride_lookup_4k | 7 | 3840x2160 | 0.195 | 0.031 | 0.0000 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS25_ride_lanterns | 7 | 1920x1080 | 0.199 | 0.063 | 0.0005 | - | PASS |  |
| b07_SluiceLedge_KS25_ride_lanterns_1610 | 7 | 1680x1050 | 0.198 | 0.063 | 0.0006 | - | PASS |  |
| b07_SluiceLedge_KS25_ride_lanterns_219 | 7 | 2560x1080 | 0.198 | 0.060 | 0.0004 | - | PASS |  |
| b07_SluiceLedge_KS25_ride_lanterns_4k | 7 | 3840x2160 | 0.198 | 0.063 | 0.0012 | - | FAIL | fireflies |
| b07_SluiceLedge_KS26_shuttle_back | 7 | 1920x1080 | 0.189 | 0.069 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS26_shuttle_back_1610 | 7 | 1680x1050 | 0.191 | 0.073 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS26_shuttle_back_219 | 7 | 2560x1080 | 0.185 | 0.068 | 0.0000 | - | FAIL | fireflies |
| b07_SluiceLedge_KS26_shuttle_back_4k | 7 | 3840x2160 | 0.189 | 0.072 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS26a_transfer | 7 | 1920x1080 | 0.155 | 0.023 | 0.0910 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS26a_transfer_1610 | 7 | 1680x1050 | 0.162 | 0.023 | 0.1017 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS26a_transfer_219 | 7 | 2560x1080 | 0.149 | 0.026 | 0.0685 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS26a_transfer_4k | 7 | 3840x2160 | 0.155 | 0.023 | 0.0920 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS26b_platform_walk | 7 | 1920x1080 | 0.272 | 0.104 | 0.0029 | - | PASS |  |
| b07_SluiceLedge_KS26b_platform_walk_1610 | 7 | 1680x1050 | 0.276 | 0.104 | 0.0036 | - | FAIL | mean luma |
| b07_SluiceLedge_KS26b_platform_walk_219 | 7 | 2560x1080 | 0.260 | 0.101 | 0.0022 | - | PASS |  |
| b07_SluiceLedge_KS26b_platform_walk_4k | 7 | 3840x2160 | 0.272 | 0.104 | 0.0031 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS27_keeper | 7 | 1920x1080 | 0.197 | 0.092 | 0.0494 | - | PASS |  |
| b07_SluiceLedge_KS27_keeper_1610 | 7 | 1680x1050 | 0.201 | 0.095 | 0.0545 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS27_keeper_219 | 7 | 2560x1080 | 0.190 | 0.092 | 0.0372 | - | PASS |  |
| b07_SluiceLedge_KS27_keeper_4k | 7 | 3840x2160 | 0.197 | 0.092 | 0.0492 | - | PASS |  |
| b07_SluiceLedge_KS28_dam_reveal | 7 | 1920x1080 | 0.177 | 0.092 | 0.0000 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS28_dam_reveal_1610 | 7 | 1680x1050 | 0.179 | 0.093 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS28_dam_reveal_219 | 7 | 2560x1080 | 0.174 | 0.092 | 0.0000 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS28_dam_reveal_4k | 7 | 3840x2160 | 0.177 | 0.092 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS28b_crest_dark | 7 | 1920x1080 | 0.177 | 0.092 | 0.0000 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS28b_crest_dark_1610 | 7 | 1680x1050 | 0.179 | 0.093 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS28b_crest_dark_219 | 7 | 2560x1080 | 0.174 | 0.092 | 0.0000 | - | FAIL | NaN-like pixels |
| b07_SluiceLedge_KS28b_crest_dark_4k | 7 | 3840x2160 | 0.177 | 0.092 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS29_lift | 7 | 1920x1080 | 0.195 | 0.143 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS29_lift_1610 | 7 | 1680x1050 | 0.196 | 0.145 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS29_lift_219 | 7 | 2560x1080 | 0.193 | 0.139 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS29_lift_4k | 7 | 3840x2160 | 0.194 | 0.142 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_end | 7 | 1920x1080 | 0.262 | 0.046 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_look_back | 7 | 1920x1080 | 0.247 | 0.083 | 0.0001 | - | PASS |  |
| b07_SluiceLedge_look_down | 7 | 1920x1080 | 0.080 | 0.064 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_look_l | 7 | 1920x1080 | 0.349 | 0.069 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_look_r | 7 | 1920x1080 | 0.214 | 0.075 | 0.0000 | - | FAIL | fireflies |
| b07_SluiceLedge_look_up | 7 | 1920x1080 | 0.260 | 0.163 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_mid | 7 | 1920x1080 | 0.295 | 0.082 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_start | 7 | 1920x1080 | 0.164 | 0.089 | 0.0008 | - | PASS |  |
| parity | 1 | 1920x1080 | 0.172 | 0.086 | 0.0003 | - | PASS |  |

## Key-shot coverage (14.10.2: every key shot at 1920x1080 and 3840x2160)

| Key shot | Requested sizes found | Status |
|---|---|---|
| KS01_gal_far | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS02_gal_slit | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS03_door_open | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS04_crest_sea | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS05_crest_drop | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS06_crest_axis | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS07_spill_gates | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS08_spill_bow | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS09_spill_grate | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS10_reef_wide | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS11_reef_spray | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS12_tower_up | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS13_atrium | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS14_stairs_down | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS15_win_first | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS16_win_up | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS18_win_down | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS19_sluice | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS20_ledge_world | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS21_beacon | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS22_ride_drop | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS23_ride_sunset | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS24_ride_lookup | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS25_ride_lanterns | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS26a_transfer | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS26_shuttle_back | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS26b_platform_walk | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS27_keeper | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS28_dam_reveal | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS28b_crest_dark | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KS29_lift | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KK1_arm_tower | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KK2_head_eyes | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |
| KK4_arm_glass | 1680x1050, 1920x1080, 2560x1080, 3840x2160 | PASS |

## Details

### b01_WaveGallery_KS01_gal_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS01_gal_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1721 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS01_gal_far_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0005 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1757 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS01_gal_far_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1603 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS01_gal_far_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1721 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS02_gal_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1790 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS02_gal_slit_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1869 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS02_gal_slit_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1601 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS02_gal_slit_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1790 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS03_door_open (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS03_door_open.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3457 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS03_door_open_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3587 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS03_door_open_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3138 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_KS03_door_open_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3457 | 0.27451..0.470588 | display |

### b01_WaveGallery_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.4501 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1223 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1363 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1574 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1637 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2184 | 0.109804..0.27451 | display |

### b01_WaveGallery_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1223 | 0.109804..0.27451 | display |

### b01_WaveGallery_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b01_WaveGallery_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0004 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1142 | 0.109804..0.27451 | display |

### b02_Crest_KK1_arm_tower (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KK1_arm_tower.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5088 | 0.333333..0.588235 | display |

### b02_Crest_KK1_arm_tower_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KK1_arm_tower_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5093 | 0.333333..0.588235 | display |

### b02_Crest_KK1_arm_tower_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KK1_arm_tower_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5067 | 0.333333..0.588235 | display |

### b02_Crest_KK1_arm_tower_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KK1_arm_tower_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5088 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS04_crest_sea.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4922 | 0.333333..0.588235 | display |
| horizon[sea_horizon] step | PASS | 0.1120 | <= 2 dE | trend-removed at row ~420.0, 211 cols: above [163.2, 175.2, 184.6] vs below [163.3, 175.2, 184.4] |
| banding[storm_sky] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.03 px (limit 10.0), 15 strips |

### b02_Crest_KS04_crest_sea_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS04_crest_sea_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4932 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS04_crest_sea_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4910 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS04_crest_sea_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1685, 1344]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4931 | 0.333333..0.588235 | display |
| horizon[sea_horizon] step | PASS | 0.3590 | <= 2 dE | trend-removed at row ~840.0, 422 cols: above [163.3, 175.3, 184.6] vs below [164.5, 176.1, 185.3] |
| banding[storm_sky] | PASS | 0.3330 | score <= 1 | longest run 4 px (max 12), mean run 1.03 px (limit 18.8), 30 strips |

### b02_Crest_KS05_crest_drop (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS05_crest_drop.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3976 | 0.333333..0.588235 | display |
| banding[haze] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.04 px (limit 10.2), 15 strips |

### b02_Crest_KS05_crest_drop_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS05_crest_drop_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3986 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS05_crest_drop_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3957 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS05_crest_drop_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3976 | 0.333333..0.588235 | display |
| banding[haze] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.02 px (limit 11.3), 30 strips |

### b02_Crest_KS06_crest_axis (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS06_crest_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5018 | 0.333333..0.588235 | display |

### b02_Crest_KS06_crest_axis_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS06_crest_axis_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5024 | 0.333333..0.588235 | display |

### b02_Crest_KS06_crest_axis_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS06_crest_axis_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5003 | 0.333333..0.588235 | display |

### b02_Crest_KS06_crest_axis_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_KS06_crest_axis_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5018 | 0.333333..0.588235 | display |

### b02_Crest_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4567 | 0.333333..0.588235 | display |

### b02_Crest_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5009 | 0.333333..0.588235 | display |

### b02_Crest_look_down (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3132 | 0.333333..0.588235 | display |

### b02_Crest_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4952 | 0.333333..0.588235 | display |

### b02_Crest_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5041 | 0.333333..0.588235 | display |

### b02_Crest_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5346 | 0.333333..0.588235 | display |

### b02_Crest_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4977 | 0.333333..0.588235 | display |

### b02_Crest_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b02_Crest_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5040 | 0.333333..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS07_spill_gates.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4762 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS07_spill_gates_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4770 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS07_spill_gates_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4660 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS07_spill_gates_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4763 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS08_spill_bow.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4742 | 0.372549..0.588235 | display |
| banding[sky] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.02 px (limit 11.1), 15 strips |

### b03_SpillwayBridge_KS08_spill_bow_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS08_spill_bow_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4781 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS08_spill_bow_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4665 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS08_spill_bow_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4742 | 0.372549..0.588235 | display |
| banding[sky] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.04 px (limit 20.5), 30 strips |

### b03_SpillwayBridge_KS09_spill_grate (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS09_spill_grate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3787 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS09_spill_grate_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS09_spill_grate_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3814 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS09_spill_grate_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS09_spill_grate_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3668 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS09_spill_grate_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_KS09_spill_grate_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3783 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[975, 525]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4270 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_back (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[651, 592]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4258 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_down (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3045 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_down90 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_look_down90.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3319 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4760 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4409 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4529 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_mid (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[959, 519]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4148 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b03_SpillwayBridge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4671 | 0.372549..0.588235 | display |

### b04_ArmourSlope_KK2_head_eyes (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KK2_head_eyes.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3932 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KK2_head_eyes_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KK2_head_eyes_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3890 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KK2_head_eyes_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KK2_head_eyes_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4072 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KK2_head_eyes_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KK2_head_eyes_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 5 | 0 | bright 5 (mode all), dark 0, first at [[1745, 949], [1124, 971], [1135, 974], [1134, 1023], [1301, 1044]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3953 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS10_reef_wide.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3978 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS10_reef_wide_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3945 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS10_reef_wide_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4092 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS10_reef_wide_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 4 | 0 | bright 4 (mode all), dark 0, first at [[1224, 659], [1360, 853], [1180, 887], [3385, 1316]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3999 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS11_reef_spray.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5118 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS11_reef_spray_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5152 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS11_reef_spray_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[2289, 465]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4992 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS11_reef_spray_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5122 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS12_tower_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4856 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS12_tower_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4859 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS12_tower_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4866 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_KS12_tower_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[2534, 1866]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4856 | 0.392157..0.647059 | display |

### b04_ArmourSlope_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3700 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.6073 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.6171 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.6096 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_r (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3873 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4114 | 0.392157..0.647059 | display |

### b04_ArmourSlope_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5240 | 0.392157..0.647059 | display |

### b04_ArmourSlope_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b04_ArmourSlope_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4383 | 0.392157..0.647059 | display |

### b05_GateTower_KS13_atrium (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_KS13_atrium.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1536 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_KS13_atrium_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1506 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_KS13_atrium_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1652 | 0.109804..0.313725 | display |

### b05_GateTower_KS13_atrium_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_KS13_atrium_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1537 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_KS14_stairs_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1939 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_KS14_stairs_down_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1969 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_KS14_stairs_down_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1857 | 0.109804..0.313725 | display |

### b05_GateTower_KS14_stairs_down_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_KS14_stairs_down_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1939 | 0.109804..0.313725 | display |

### b05_GateTower_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[927, 652]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1626 | 0.109804..0.313725 | display |

### b05_GateTower_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1243 | 0.109804..0.313725 | display |

### b05_GateTower_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1725 | 0.109804..0.313725 | display |

### b05_GateTower_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0040 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1429 | 0.109804..0.313725 | display |

### b05_GateTower_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1892 | 0.109804..0.313725 | display |

### b05_GateTower_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1254 | 0.109804..0.313725 | display |

### b05_GateTower_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1273 | 0.109804..0.313725 | display |

### b05_GateTower_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b05_GateTower_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2814 | 0.109804..0.313725 | display |

### b06_WindowGallery_KK4_arm_glass (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KK4_arm_glass.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2021 | 0.054902..0.333333 | display |

### b06_WindowGallery_KK4_arm_glass_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KK4_arm_glass_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2022 | 0.054902..0.333333 | display |

### b06_WindowGallery_KK4_arm_glass_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KK4_arm_glass_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1978 | 0.054902..0.333333 | display |

### b06_WindowGallery_KK4_arm_glass_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KK4_arm_glass_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2021 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS15_win_first.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2931 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS15_win_first_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3043 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS15_win_first_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2352 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS15_win_first_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2933 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS16_win_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1924 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS16_win_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2050 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS16_win_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1641 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS16_win_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1925 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_eye (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS17_win_eye.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1868 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_eye_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS17_win_eye_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1949 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_eye_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS17_win_eye_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1739 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_eye_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS17_win_eye_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1869 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS18_win_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1276 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS18_win_down_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1271 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS18_win_down_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1282 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_KS18_win_down_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1277 | 0.054902..0.333333 | display |

### b06_WindowGallery_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0028 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1573 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1529 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1107 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0163 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1593 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1239 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1495 | 0.054902..0.333333 | display |

### b06_WindowGallery_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1654 | 0.054902..0.333333 | display |

### b06_WindowGallery_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b06_WindowGallery_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1841 | 0.054902..0.333333 | display |

### b07_SluiceLedge_KS19_sluice (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS19_sluice.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 66 | 0 px | first at [[857, 646], [857, 647], [857, 648], [835, 664], [857, 682]] |
| clipped highlights | PASS | 0.0007 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1439 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS19_sluice_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 64 | 0 px | first at [[740, 628], [740, 629], [740, 630], [740, 631], [718, 647]] |
| clipped highlights | PASS | 0.0006 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1415 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS19_sluice_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 432 | 0 px | first at [[1200, 645], [1201, 645], [1177, 646], [1200, 646], [1201, 646]] |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1677 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS19_sluice_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 237 | 0 px | first at [[1670, 1297], [1670, 1298], [1670, 1322], [1670, 1323], [1671, 1323]] |
| clipped highlights | PASS | 0.0007 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1435 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS20_ledge_world.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 6014 px excluded |
| mean luma | PASS | 0.2886 | 0.117647..0.352941 | display |
| horizon[lowland_horizon] step | PASS | 0.1250 | <= 2 dE | trend-removed at row ~474.0, 1094 cols: above [207.0, 141.7, 110.6] vs below [207.4, 141.9, 110.9] |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.02 px (limit 7.1), 10 strips |

### b07_SluiceLedge_KS20_ledge_world_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS20_ledge_world_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 5688 px excluded |
| mean luma | PASS | 0.2901 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS20_ledge_world_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 6014 px excluded |
| mean luma | PASS | 0.2856 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS20_ledge_world_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 24075 px excluded |
| mean luma | PASS | 0.2883 | 0.117647..0.352941 | display |
| horizon[lowland_horizon] step | PASS | 0.1490 | <= 2 dE | trend-removed at row ~948.0, 2188 cols: above [207.0, 141.7, 110.8] vs below [207.5, 141.9, 111.0] |
| banding[dusk_sky] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.02 px (limit 6.9), 20 strips |

### b07_SluiceLedge_KS21_beacon (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS21_beacon.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2549 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS21_beacon_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2493 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS21_beacon_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 20764 px excluded |
| mean luma | PASS | 0.2713 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS21_beacon_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2546 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS22_ride_drop (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS22_ride_drop.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1798 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS22_ride_drop_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS22_ride_drop_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1791 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS22_ride_drop_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS22_ride_drop_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1833 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS22_ride_drop_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS22_ride_drop_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1792 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS23_ride_sunset (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS23_ride_sunset.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0009 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2687 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS23_ride_sunset_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS23_ride_sunset_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0012 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2702 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS23_ride_sunset_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS23_ride_sunset_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0007 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2652 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS23_ride_sunset_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS23_ride_sunset_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0010 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2681 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS24_ride_lookup (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS24_ride_lookup.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[721, 542], [655, 720], [603, 862]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1945 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS24_ride_lookup_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS24_ride_lookup_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1898 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS24_ride_lookup_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS24_ride_lookup_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1194, 125]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2067 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS24_ride_lookup_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS24_ride_lookup_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 3 | 0 | bright 3 (mode all), dark 0, first at [[1525, 858], [1409, 1174], [1205, 1728]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1945 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS25_ride_lanterns (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS25_ride_lanterns.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1985 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS25_ride_lanterns_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS25_ride_lanterns_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1981 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS25_ride_lanterns_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS25_ride_lanterns_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0004 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1977 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS25_ride_lanterns_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS25_ride_lanterns_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 13 | 0 px | first at [[638, 451], [639, 457], [639, 458], [640, 463], [640, 464]] |
| clipped highlights | PASS | 0.0012 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1984 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS26_shuttle_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26_shuttle_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1892 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS26_shuttle_back_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26_shuttle_back_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1910 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS26_shuttle_back_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26_shuttle_back_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 54 | 0 px | first at [[2326, 809], [2328, 812], [2333, 820], [2337, 826], [2341, 832]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1855 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS26_shuttle_back_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26_shuttle_back_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1889 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS26a_transfer (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26a_transfer.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[919, 161]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0910 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1554 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS26a_transfer_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26a_transfer_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1104, 97]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.1017 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1616 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS26a_transfer_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26a_transfer_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1041, 199]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0685 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1488 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS26a_transfer_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26a_transfer_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[2138, 263]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0920 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1554 | 0.078431..0.313725 | display |

### b07_SluiceLedge_KS26b_platform_walk (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26b_platform_walk.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0029 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2715 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS26b_platform_walk_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26b_platform_walk_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0036 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2763 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS26b_platform_walk_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26b_platform_walk_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0022 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2604 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS26b_platform_walk_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS26b_platform_walk_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1697, 1132]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0031 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2715 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS27_keeper (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS27_keeper.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0494 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1970 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS27_keeper_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS27_keeper_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 2 | 0 | bright 2 (mode all), dark 0, first at [[1402, 493], [943, 715]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0545 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2006 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS27_keeper_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS27_keeper_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0372 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1903 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS27_keeper_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS27_keeper_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0492 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1968 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS28_dam_reveal (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS28_dam_reveal.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1606, 966]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1773 | 0.098039..0.27451 | display |

### b07_SluiceLedge_KS28_dam_reveal_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS28_dam_reveal_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1788 | 0.098039..0.27451 | display |

### b07_SluiceLedge_KS28_dam_reveal_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS28_dam_reveal_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1926, 966]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1740 | 0.098039..0.27451 | display |

### b07_SluiceLedge_KS28_dam_reveal_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS28_dam_reveal_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1769 | 0.098039..0.27451 | display |

### b07_SluiceLedge_KS28b_crest_dark (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS28b_crest_dark.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1606, 966]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1773 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS28b_crest_dark_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS28b_crest_dark_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1788 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS28b_crest_dark_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS28b_crest_dark_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1926, 966]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1740 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS28b_crest_dark_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS28b_crest_dark_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1769 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS29_lift (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS29_lift.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1946 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS29_lift_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS29_lift_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1956 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS29_lift_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS29_lift_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1929 | 0.078431..0.27451 | display |

### b07_SluiceLedge_KS29_lift_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_KS29_lift_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1944 | 0.078431..0.27451 | display |

### b07_SluiceLedge_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2619 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | INFO | 0.2473 | 0.0784314..0.352941 | sky 53.3 % of the frame (> 50 %): beat-7 band not applied (14.10.5 ruling) |

### b07_SluiceLedge_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.0797 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 8754 px excluded |
| mean luma | INFO | 0.3486 | 0.0784314..0.352941 | sky 53.3 % of the frame (> 50 %): beat-7 band not applied (14.10.5 ruling) |

### b07_SluiceLedge_look_r (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 5 | 0 px | first at [[1565, 747], [1565, 750], [1565, 753], [1565, 754], [1565, 755]] |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | INFO | 0.2137 | 0.0784314..0.352941 | sky 53.3 % of the frame (> 50 %): beat-7 band not applied (14.10.5 ruling) |

### b07_SluiceLedge_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2601 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2949 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\b07_SluiceLedge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1641 | 0.0784314..0.352941 | display |

### parity (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\fin-stills\parity_rt.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1721 | 0.109804..0.27451 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
