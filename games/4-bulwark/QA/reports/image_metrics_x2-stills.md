# Image metrics: x2-stills

Capture folder: `<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills`  
Scene constants: BulwarkConst.cs  
Images: 142, failing images: 25. Checks: 1120 PASS, 25 FAIL, 0 WARN, 0 SKIP, 3 INFO.

**Result: FAIL**

## Input warnings

- parity_window.png is not listed in report.json

## Summary

| Image | Beat | Size | Mean luma | p1 | Clipped % | Band score | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b01_WaveGallery_KS01_gal_far | 1 | 1920x1080 | 0.171 | 0.084 | 0.0003 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_1610 | 1 | 1680x1050 | 0.174 | 0.086 | 0.0005 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_219 | 1 | 2560x1080 | 0.159 | 0.076 | 0.0003 | - | PASS |  |
| b01_WaveGallery_KS01_gal_far_4k | 1 | 3840x2160 | 0.171 | 0.083 | 0.0006 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit | 1 | 1920x1080 | 0.147 | 0.088 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_1610 | 1 | 1680x1050 | 0.152 | 0.089 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_219 | 1 | 2560x1080 | 0.135 | 0.076 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS02_gal_slit_4k | 1 | 3840x2160 | 0.147 | 0.088 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS03_door_open | 1 | 1920x1080 | 0.299 | 0.112 | 0.0000 | - | FAIL | NaN-like pixels |
| b01_WaveGallery_KS03_door_open_1610 | 1 | 1680x1050 | 0.308 | 0.113 | 0.0000 | - | PASS |  |
| b01_WaveGallery_KS03_door_open_219 | 1 | 2560x1080 | 0.276 | 0.109 | 0.0000 | - | FAIL | NaN-like pixels |
| b01_WaveGallery_KS03_door_open_4k | 1 | 3840x2160 | 0.299 | 0.112 | 0.0000 | - | PASS |  |
| b01_WaveGallery_end | 1 | 1920x1080 | 0.373 | 0.199 | 0.0000 | - | FAIL | mean luma |
| b01_WaveGallery_look_back | 1 | 1920x1080 | 0.120 | 0.076 | 0.0006 | - | PASS |  |
| b01_WaveGallery_look_down | 1 | 1920x1080 | 0.135 | 0.063 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_l | 1 | 1920x1080 | 0.157 | 0.076 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_r | 1 | 1920x1080 | 0.163 | 0.105 | 0.0000 | - | PASS |  |
| b01_WaveGallery_look_up | 1 | 1920x1080 | 0.218 | 0.088 | 0.0000 | - | PASS |  |
| b01_WaveGallery_mid | 1 | 1920x1080 | 0.120 | 0.076 | 0.0006 | - | PASS |  |
| b01_WaveGallery_start | 1 | 1920x1080 | 0.111 | 0.060 | 0.0004 | - | PASS |  |
| b02_Crest_KS04_crest_sea | 2 | 1920x1080 | 0.418 | 0.246 | 0.0000 | 0.500 | PASS |  |
| b02_Crest_KS04_crest_sea_1610 | 2 | 1680x1050 | 0.418 | 0.246 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea_219 | 2 | 2560x1080 | 0.417 | 0.246 | 0.0000 | - | PASS |  |
| b02_Crest_KS04_crest_sea_4k | 2 | 3840x2160 | 0.419 | 0.246 | 0.0000 | 0.333 | PASS |  |
| b02_Crest_KS05_crest_drop | 2 | 1920x1080 | 0.339 | 0.199 | 0.0000 | 0.500 | PASS |  |
| b02_Crest_KS05_crest_drop_1610 | 2 | 1680x1050 | 0.340 | 0.199 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_219 | 2 | 2560x1080 | 0.338 | 0.199 | 0.0000 | - | PASS |  |
| b02_Crest_KS05_crest_drop_4k | 2 | 3840x2160 | 0.339 | 0.198 | 0.0000 | 0.333 | PASS |  |
| b02_Crest_KS06_crest_axis | 2 | 1920x1080 | 0.427 | 0.242 | 0.0000 | - | PASS |  |
| b02_Crest_KS06_crest_axis_1610 | 2 | 1680x1050 | 0.427 | 0.242 | 0.0000 | - | PASS |  |
| b02_Crest_KS06_crest_axis_219 | 2 | 2560x1080 | 0.427 | 0.245 | 0.0000 | - | PASS |  |
| b02_Crest_KS06_crest_axis_4k | 2 | 3840x2160 | 0.427 | 0.242 | 0.0000 | - | PASS |  |
| b02_Crest_end | 2 | 1920x1080 | 0.384 | 0.194 | 0.0000 | - | PASS |  |
| b02_Crest_look_back | 2 | 1920x1080 | 0.425 | 0.241 | 0.0000 | - | PASS |  |
| b02_Crest_look_down | 2 | 1920x1080 | 0.230 | 0.202 | 0.0000 | - | FAIL | mean luma |
| b02_Crest_look_l | 2 | 1920x1080 | 0.430 | 0.242 | 0.0000 | - | PASS |  |
| b02_Crest_look_r | 2 | 1920x1080 | 0.425 | 0.238 | 0.0000 | - | PASS |  |
| b02_Crest_look_up | 2 | 1920x1080 | 0.471 | 0.446 | 0.0000 | - | PASS |  |
| b02_Crest_mid | 2 | 1920x1080 | 0.422 | 0.241 | 0.0000 | - | PASS |  |
| b02_Crest_start | 2 | 1920x1080 | 0.428 | 0.245 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS07_spill_gates | 3 | 1920x1080 | 0.353 | 0.173 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS07_spill_gates_1610 | 3 | 1680x1050 | 0.354 | 0.173 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS07_spill_gates_219 | 3 | 2560x1080 | 0.343 | 0.173 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS07_spill_gates_4k | 3 | 3840x2160 | 0.353 | 0.173 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS08_spill_bow | 3 | 1920x1080 | 0.474 | 0.229 | 0.0000 | 0.500 | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_1610 | 3 | 1680x1050 | 0.478 | 0.229 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_219 | 3 | 2560x1080 | 0.465 | 0.226 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS08_spill_bow_4k | 3 | 3840x2160 | 0.474 | 0.229 | 0.0000 | 0.333 | PASS |  |
| b03_SpillwayBridge_KS09_spill_grate | 3 | 1920x1080 | 0.379 | 0.146 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS09_spill_grate_1610 | 3 | 1680x1050 | 0.382 | 0.146 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_KS09_spill_grate_219 | 3 | 2560x1080 | 0.367 | 0.134 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_KS09_spill_grate_4k | 3 | 3840x2160 | 0.379 | 0.146 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_end | 3 | 1920x1080 | 0.426 | 0.248 | 0.0001 | - | FAIL | NaN-like pixels |
| b03_SpillwayBridge_look_back | 3 | 1920x1080 | 0.417 | 0.176 | 0.0000 | - | FAIL | NaN-like pixels |
| b03_SpillwayBridge_look_down | 3 | 1920x1080 | 0.297 | 0.228 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_look_down90 | 3 | 1920x1080 | 0.323 | 0.221 | 0.0000 | - | FAIL | mean luma |
| b03_SpillwayBridge_look_l | 3 | 1920x1080 | 0.469 | 0.338 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_look_r | 3 | 1920x1080 | 0.432 | 0.213 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_look_up | 3 | 1920x1080 | 0.445 | 0.414 | 0.0000 | - | PASS |  |
| b03_SpillwayBridge_mid | 3 | 1920x1080 | 0.405 | 0.180 | 0.0000 | - | FAIL | NaN-like pixels |
| b03_SpillwayBridge_start | 3 | 1920x1080 | 0.387 | 0.154 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide | 4 | 1920x1080 | 0.402 | 0.141 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide_1610 | 4 | 1680x1050 | 0.398 | 0.142 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide_219 | 4 | 2560x1080 | 0.413 | 0.144 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS10_reef_wide_4k | 4 | 3840x2160 | 0.403 | 0.139 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray | 4 | 1920x1080 | 0.505 | 0.159 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_1610 | 4 | 1680x1050 | 0.508 | 0.157 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS11_reef_spray_219 | 4 | 2560x1080 | 0.494 | 0.143 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_KS11_reef_spray_4k | 4 | 3840x2160 | 0.506 | 0.159 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up | 4 | 1920x1080 | 0.486 | 0.222 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_1610 | 4 | 1680x1050 | 0.486 | 0.226 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_219 | 4 | 2560x1080 | 0.487 | 0.219 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_KS12_tower_up_4k | 4 | 3840x2160 | 0.486 | 0.222 | 0.0000 | - | FAIL | NaN-like pixels |
| b04_ArmourSlope_end | 4 | 1920x1080 | 0.370 | 0.070 | 0.0000 | - | FAIL | mean luma |
| b04_ArmourSlope_look_back | 4 | 1920x1080 | 0.607 | 0.190 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_down | 4 | 1920x1080 | 0.617 | 0.186 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_l | 4 | 1920x1080 | 0.609 | 0.319 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_look_r | 4 | 1920x1080 | 0.391 | 0.132 | 0.0000 | - | FAIL | mean luma |
| b04_ArmourSlope_look_up | 4 | 1920x1080 | 0.411 | 0.353 | 0.0000 | - | PASS |  |
| b04_ArmourSlope_mid | 4 | 1920x1080 | 0.526 | 0.183 | 0.0003 | - | PASS |  |
| b04_ArmourSlope_start | 4 | 1920x1080 | 0.438 | 0.252 | 0.0001 | - | PASS |  |
| b05_GateTower_KS13_atrium | 5 | 1920x1080 | 0.154 | 0.066 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium_1610 | 5 | 1680x1050 | 0.151 | 0.066 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium_219 | 5 | 2560x1080 | 0.165 | 0.067 | 0.0000 | - | PASS |  |
| b05_GateTower_KS13_atrium_4k | 5 | 3840x2160 | 0.154 | 0.066 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down | 5 | 1920x1080 | 0.194 | 0.091 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_1610 | 5 | 1680x1050 | 0.197 | 0.091 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_219 | 5 | 2560x1080 | 0.186 | 0.091 | 0.0000 | - | PASS |  |
| b05_GateTower_KS14_stairs_down_4k | 5 | 3840x2160 | 0.194 | 0.091 | 0.0000 | - | PASS |  |
| b05_GateTower_end | 5 | 1920x1080 | 0.160 | 0.056 | 0.0000 | - | FAIL | NaN-like pixels |
| b05_GateTower_look_back | 5 | 1920x1080 | 0.124 | 0.053 | 0.0000 | - | PASS |  |
| b05_GateTower_look_down | 5 | 1920x1080 | 0.172 | 0.095 | 0.0000 | - | PASS |  |
| b05_GateTower_look_l | 5 | 1920x1080 | 0.143 | 0.064 | 0.0040 | - | PASS |  |
| b05_GateTower_look_r | 5 | 1920x1080 | 0.189 | 0.104 | 0.0000 | - | PASS |  |
| b05_GateTower_look_up | 5 | 1920x1080 | 0.125 | 0.083 | 0.0000 | - | PASS |  |
| b05_GateTower_mid | 5 | 1920x1080 | 0.127 | 0.083 | 0.0000 | - | PASS |  |
| b05_GateTower_start | 5 | 1920x1080 | 0.281 | 0.070 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first | 6 | 1920x1080 | 0.280 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_1610 | 6 | 1680x1050 | 0.290 | 0.099 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_219 | 6 | 2560x1080 | 0.225 | 0.030 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS15_win_first_4k | 6 | 3840x2160 | 0.280 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up | 6 | 1920x1080 | 0.219 | 0.034 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_1610 | 6 | 1680x1050 | 0.232 | 0.035 | 0.0000 | - | FAIL | fireflies |
| b06_WindowGallery_KS16_win_up_219 | 6 | 2560x1080 | 0.190 | 0.034 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS16_win_up_4k | 6 | 3840x2160 | 0.219 | 0.034 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_eye | 6 | 1920x1080 | 0.170 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_eye_1610 | 6 | 1680x1050 | 0.176 | 0.041 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_eye_219 | 6 | 2560x1080 | 0.159 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS17_win_eye_4k | 6 | 3840x2160 | 0.170 | 0.038 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down | 6 | 1920x1080 | 0.126 | 0.046 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_1610 | 6 | 1680x1050 | 0.126 | 0.046 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_219 | 6 | 2560x1080 | 0.126 | 0.042 | 0.0000 | - | PASS |  |
| b06_WindowGallery_KS18_win_down_4k | 6 | 3840x2160 | 0.126 | 0.046 | 0.0000 | - | PASS |  |
| b06_WindowGallery_end | 6 | 1920x1080 | 0.157 | 0.091 | 0.0028 | - | PASS |  |
| b06_WindowGallery_look_back | 6 | 1920x1080 | 0.146 | 0.042 | 0.0001 | - | PASS |  |
| b06_WindowGallery_look_down | 6 | 1920x1080 | 0.106 | 0.089 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_l | 6 | 1920x1080 | 0.155 | 0.097 | 0.0163 | - | PASS |  |
| b06_WindowGallery_look_r | 6 | 1920x1080 | 0.118 | 0.073 | 0.0000 | - | PASS |  |
| b06_WindowGallery_look_up | 6 | 1920x1080 | 0.147 | 0.043 | 0.0000 | - | PASS |  |
| b06_WindowGallery_mid | 6 | 1920x1080 | 0.157 | 0.042 | 0.0000 | - | PASS |  |
| b06_WindowGallery_start | 6 | 1920x1080 | 0.179 | 0.052 | 0.0001 | - | PASS |  |
| b07_SluiceLedge_KS19_sluice | 7 | 1920x1080 | 0.147 | 0.024 | 0.0007 | - | FAIL | fireflies |
| b07_SluiceLedge_KS19_sluice_1610 | 7 | 1680x1050 | 0.145 | 0.024 | 0.0006 | - | FAIL | fireflies |
| b07_SluiceLedge_KS19_sluice_219 | 7 | 2560x1080 | 0.171 | 0.025 | 0.0005 | - | FAIL | fireflies |
| b07_SluiceLedge_KS19_sluice_4k | 7 | 3840x2160 | 0.147 | 0.024 | 0.0008 | - | FAIL | fireflies |
| b07_SluiceLedge_KS20_ledge_world | 7 | 1920x1080 | 0.285 | 0.067 | 0.0000 | 0.333 | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_1610 | 7 | 1680x1050 | 0.287 | 0.067 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_219 | 7 | 2560x1080 | 0.282 | 0.069 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS20_ledge_world_4k | 7 | 3840x2160 | 0.285 | 0.067 | 0.0000 | 0.250 | PASS |  |
| b07_SluiceLedge_KS21_beacon | 7 | 1920x1080 | 0.252 | 0.031 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_1610 | 7 | 1680x1050 | 0.247 | 0.030 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_219 | 7 | 2560x1080 | 0.267 | 0.032 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_KS21_beacon_4k | 7 | 3840x2160 | 0.252 | 0.031 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_end | 7 | 1920x1080 | 0.259 | 0.046 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_look_back | 7 | 1920x1080 | 0.247 | 0.083 | 0.0001 | - | PASS |  |
| b07_SluiceLedge_look_down | 7 | 1920x1080 | 0.080 | 0.064 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_look_l | 7 | 1920x1080 | 0.344 | 0.069 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_look_r | 7 | 1920x1080 | 0.214 | 0.075 | 0.0000 | - | FAIL | fireflies |
| b07_SluiceLedge_look_up | 7 | 1920x1080 | 0.260 | 0.163 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_mid | 7 | 1920x1080 | 0.293 | 0.082 | 0.0000 | - | PASS |  |
| b07_SluiceLedge_start | 7 | 1920x1080 | 0.164 | 0.089 | 0.0008 | - | PASS |  |
| parity | 1 | 1920x1080 | 0.171 | 0.084 | 0.0003 | - | PASS |  |

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

## Details

### b01_WaveGallery_KS01_gal_far (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS01_gal_far.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1706 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS01_gal_far_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0005 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1742 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS01_gal_far_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1590 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS01_gal_far_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS01_gal_far_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1706 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS02_gal_slit.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1471 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS02_gal_slit_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1519 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS02_gal_slit_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1353 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS02_gal_slit_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS02_gal_slit_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1471 | 0.109804..0.27451 | display |

### b01_WaveGallery_KS03_door_open (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS03_door_open.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[959, 552]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2990 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS03_door_open_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3081 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS03_door_open_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[1279, 552]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2764 | 0.27451..0.470588 | display |

### b01_WaveGallery_KS03_door_open_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_KS03_door_open_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2990 | 0.27451..0.470588 | display |

### b01_WaveGallery_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3733 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1204 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1355 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1566 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1630 | 0.109804..0.27451 | display |

### b01_WaveGallery_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2177 | 0.109804..0.27451 | display |

### b01_WaveGallery_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0006 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1204 | 0.109804..0.27451 | display |

### b01_WaveGallery_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b01_WaveGallery_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0004 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1107 | 0.109804..0.27451 | display |

### b02_Crest_KS04_crest_sea (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS04_crest_sea.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4177 | 0.333333..0.588235 | display |
| horizon[sea_horizon] step | PASS | 0.0800 | <= 2 dE | trend-removed at row ~420.0, 211 cols: above [144.3, 154.8, 163.2] vs below [144.1, 154.6, 163.0] |
| banding[storm_sky] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.01 px (limit 11.2), 15 strips |

### b02_Crest_KS04_crest_sea_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS04_crest_sea_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4182 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS04_crest_sea_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4170 | 0.333333..0.588235 | display |

### b02_Crest_KS04_crest_sea_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS04_crest_sea_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4186 | 0.333333..0.588235 | display |
| horizon[sea_horizon] step | PASS | 0.1530 | <= 2 dE | trend-removed at row ~840.0, 422 cols: above [144.3, 154.9, 163.3] vs below [144.7, 155.3, 163.6] |
| banding[storm_sky] | PASS | 0.3330 | score <= 1 | longest run 4 px (max 12), mean run 1.03 px (limit 21.5), 30 strips |

### b02_Crest_KS05_crest_drop (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS05_crest_drop.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3390 | 0.333333..0.588235 | display |
| banding[haze] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.01 px (limit 14.8), 15 strips |

### b02_Crest_KS05_crest_drop_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS05_crest_drop_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3400 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS05_crest_drop_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3379 | 0.333333..0.588235 | display |

### b02_Crest_KS05_crest_drop_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS05_crest_drop_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3390 | 0.333333..0.588235 | display |
| banding[haze] | PASS | 0.3330 | score <= 1 | longest run 4 px (max 12), mean run 1.02 px (limit 18.2), 30 strips |

### b02_Crest_KS06_crest_axis (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS06_crest_axis.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4272 | 0.333333..0.588235 | display |

### b02_Crest_KS06_crest_axis_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS06_crest_axis_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4273 | 0.333333..0.588235 | display |

### b02_Crest_KS06_crest_axis_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS06_crest_axis_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4269 | 0.333333..0.588235 | display |

### b02_Crest_KS06_crest_axis_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_KS06_crest_axis_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4271 | 0.333333..0.588235 | display |

### b02_Crest_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3845 | 0.333333..0.588235 | display |

### b02_Crest_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4254 | 0.333333..0.588235 | display |

### b02_Crest_look_down (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2305 | 0.333333..0.588235 | display |

### b02_Crest_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4295 | 0.333333..0.588235 | display |

### b02_Crest_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4254 | 0.333333..0.588235 | display |

### b02_Crest_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4712 | 0.333333..0.588235 | display |

### b02_Crest_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4215 | 0.333333..0.588235 | display |

### b02_Crest_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b02_Crest_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.4 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4276 | 0.333333..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS07_spill_gates.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3534 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS07_spill_gates_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3542 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS07_spill_gates_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3429 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS07_spill_gates_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS07_spill_gates_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3534 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS08_spill_bow.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4735 | 0.372549..0.588235 | display |
| banding[sky] | PASS | 0.5000 | score <= 1 | longest run 3 px (max 6), mean run 1.01 px (limit 10.4), 15 strips |

### b03_SpillwayBridge_KS08_spill_bow_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS08_spill_bow_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4778 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS08_spill_bow_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4650 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS08_spill_bow_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS08_spill_bow_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4735 | 0.372549..0.588235 | display |
| banding[sky] | PASS | 0.3330 | score <= 1 | longest run 4 px (max 12), mean run 1.02 px (limit 23.6), 30 strips |

### b03_SpillwayBridge_KS09_spill_grate (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS09_spill_grate.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3793 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS09_spill_grate_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS09_spill_grate_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3822 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS09_spill_grate_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS09_spill_grate_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3672 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_KS09_spill_grate_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_KS09_spill_grate_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3791 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[975, 525]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4262 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_back (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[651, 592]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4166 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_down (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.2972 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_down90 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_look_down90.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3233 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4689 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4321 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4453 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_mid (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[959, 519]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4053 | 0.372549..0.588235 | display |

### b03_SpillwayBridge_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b03_SpillwayBridge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.8 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3869 | 0.372549..0.588235 | display |

### b04_ArmourSlope_KS10_reef_wide (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS10_reef_wide.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4016 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS10_reef_wide_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.3981 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS10_reef_wide_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4134 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS10_reef_wide_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS10_reef_wide_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4028 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS11_reef_spray.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5053 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS11_reef_spray_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5077 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS11_reef_spray_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 0 (mode all), dark 1, first at [[2289, 465]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4944 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS11_reef_spray_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS11_reef_spray_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5055 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS12_tower_up.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS12_tower_up_1610.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS12_tower_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4874 | 0.392157..0.647059 | display |

### b04_ArmourSlope_KS12_tower_up_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_KS12_tower_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[2534, 1866]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4855 | 0.392157..0.647059 | display |

### b04_ArmourSlope_end (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_end.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.6072 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_look_down.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.6094 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_r (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | FAIL | 0.3908 | 0.392157..0.647059 | display |

### b04_ArmourSlope_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_look_up.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.5256 | 0.392157..0.647059 | display |

### b04_ArmourSlope_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b04_ArmourSlope_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.6 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.4376 | 0.392157..0.647059 | display |

### b05_GateTower_KS13_atrium (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_KS13_atrium.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_KS13_atrium_1610.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_KS13_atrium_219.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_KS13_atrium_4k.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_KS14_stairs_down.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_KS14_stairs_down_1610.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_KS14_stairs_down_219.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_KS14_stairs_down_4k.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | FAIL | 1 | 0 | bright 1 (mode all), dark 0, first at [[927, 652]] |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1596 | 0.109804..0.313725 | display |

### b05_GateTower_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_look_back.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.3 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1724 | 0.109804..0.313725 | display |

### b05_GateTower_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_look_l.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_look_r.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_look_up.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_mid.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b05_GateTower_start.png`

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

### b06_WindowGallery_KS15_win_first (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS15_win_first.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2796 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS15_win_first_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2903 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS15_win_first_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2248 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS15_win_first_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS15_win_first_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2798 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS16_win_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2193 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS16_win_up_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | FAIL | 1 | 0 px | first at [[1033, 21]] |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2317 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS16_win_up_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1901 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS16_win_up_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS16_win_up_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2194 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_eye (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS17_win_eye.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1696 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_eye_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS17_win_eye_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1756 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_eye_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS17_win_eye_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1593 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS17_win_eye_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS17_win_eye_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1696 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS18_win_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1263 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS18_win_down_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1261 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS18_win_down_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1264 | 0.054902..0.333333 | display |

### b06_WindowGallery_KS18_win_down_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_KS18_win_down_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1264 | 0.054902..0.333333 | display |

### b06_WindowGallery_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0028 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1568 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1462 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1061 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0163 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1552 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_r (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_look_r.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1177 | 0.054902..0.333333 | display |

### b06_WindowGallery_look_up (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_look_up.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1468 | 0.054902..0.333333 | display |

### b06_WindowGallery_mid (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1567 | 0.054902..0.333333 | display |

### b06_WindowGallery_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b06_WindowGallery_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode clipped), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 0.1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1794 | 0.054902..0.333333 | display |

### b07_SluiceLedge_KS19_sluice (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS19_sluice.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 747 | 0 px | first at [[880, 645], [881, 645], [857, 646], [880, 646], [881, 646]] |
| clipped highlights | PASS | 0.0007 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1474 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_1610 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS19_sluice_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 872 | 0 px | first at [[786, 626], [786, 627], [787, 627], [740, 628], [786, 628]] |
| clipped highlights | PASS | 0.0006 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1447 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_219 (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS19_sluice_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 742 | 0 px | first at [[1200, 645], [1201, 645], [1177, 646], [1200, 646], [1201, 646]] |
| clipped highlights | PASS | 0.0005 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1706 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS19_sluice_4k (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS19_sluice_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | FAIL | 222 | 0 px | first at [[1670, 1297], [1670, 1298], [1670, 1325], [1670, 1326], [1670, 1327]] |
| clipped highlights | PASS | 0.0008 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1471 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS20_ledge_world.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 6014 px excluded |
| mean luma | PASS | 0.2849 | 0.117647..0.352941 | display |
| horizon[lowland_horizon] step | PASS | 0.2610 | <= 2 dE | trend-removed at row ~474.0, 1094 cols: above [207.0, 141.7, 110.6] vs below [207.8, 142.2, 111.0] |
| banding[dusk_sky] | PASS | 0.3330 | score <= 1 | longest run 2 px (max 6), mean run 1.02 px (limit 7.1), 10 strips |

### b07_SluiceLedge_KS20_ledge_world_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS20_ledge_world_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 5688 px excluded |
| mean luma | PASS | 0.2866 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS20_ledge_world_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 6014 px excluded |
| mean luma | PASS | 0.2820 | 0.117647..0.352941 | display |

### b07_SluiceLedge_KS20_ledge_world_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS20_ledge_world_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 24075 px excluded |
| mean luma | PASS | 0.2847 | 0.117647..0.352941 | display |
| horizon[lowland_horizon] step | PASS | 0.2990 | <= 2 dE | trend-removed at row ~948.0, 2188 cols: above [207.0, 141.6, 110.8] vs below [207.9, 142.2, 111.1] |
| banding[dusk_sky] | PASS | 0.2500 | score <= 1 | longest run 3 px (max 12), mean run 1.02 px (limit 6.9), 20 strips |

### b07_SluiceLedge_KS21_beacon (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS21_beacon.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2518 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_1610 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS21_beacon_1610.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1680x1050 | 1680x1050 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2466 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_219 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS21_beacon_219.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 2560x1080 | 2560x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 20764 px excluded |
| mean luma | PASS | 0.2668 | 0.078431..0.352941 | display |

### b07_SluiceLedge_KS21_beacon_4k (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_KS21_beacon_4k.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 3840x2160 | 3840x2160 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2515 | 0.078431..0.352941 | display |

### b07_SluiceLedge_end (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_end.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2586 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_look_back (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_look_back.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0001 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | INFO | 0.2466 | 0.0784314..0.352941 | sky 53.3 % of the frame (> 50 %): beat-7 band not applied (14.10.5 ruling) |

### b07_SluiceLedge_look_down (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_look_down.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.0796 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_look_l (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_look_l.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 8754 px excluded |
| mean luma | INFO | 0.3441 | 0.0784314..0.352941 | sky 53.3 % of the frame (> 50 %): beat-7 band not applied (14.10.5 ruling) |

### b07_SluiceLedge_look_r (FAIL)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_look_r.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_look_up.png`

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

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_mid.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0000 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.2925 | 0.0784314..0.352941 | display |

### b07_SluiceLedge_start (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\b07_SluiceLedge_start.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0008 | <= 1 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1645 | 0.0784314..0.352941 | display |

### parity (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-stills\parity_rt.png`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| size | PASS | 1920x1080 | 1920x1080 |  |
| black frame | PASS | 0.0000 | < 0.98 of pixels <= code 3 |  |
| white frame | PASS | 0.0000 | < 0.98 of pixels >= code 252 |  |
| magenta | PASS | 0 | 0 px | ratio 0 (exact 255,0,255: 0 px) |
| NaN-like pixels | PASS | 0 | 0 | bright 0 (mode all), dark 0 |
| fireflies | PASS | 0 | 0 px | 8 x 5x5 median and +16 levels |
| clipped highlights | PASS | 0.0003 | <= 1.5 % | any channel >= 254; 0 px excluded |
| mean luma | PASS | 0.1706 | 0.109804..0.27451 | display |

## Method notes

- Luma: Rec.709 weights; 'display' on the 8-bit encoded values / 255, 'linear' on the exact sRGB-decoded values. The 14.10.3 gates (p1, expected mean ranges, region std) use display luma unless --luma-scale linear.
- Clipped: any channel at code 255 (>= 0.999), percentage of all pixels outside the exclusion discs.
- Horizon 'trend' items: a line per channel fitted to rows h-7..h-2 and h+2..h+7 (1080p px) per column, both extrapolated to h, Lab dE of the column medians (limit 2.0): a real step, not the gradient on either side.
- Off-path material sheets (set 'materials', tags 'mat_*') carry no 14.10.3 beat luma band.
- NaN-like: display luma more than 0.30 above the max / 0.30 below the min of the 24 neighbours (isolated); beat 6 counts bright outliers only when they also clip (snow).
- Banding (14.10.3 + R6 6.2): 64-px strips, 9-px box, steps > 0.6 code per 1000 px <= 2; longest run (< 0.05 code) <= 6 px only where |slope| >= 0.06 code/px; residual vs a 129-px quadratic trend RMS <= 0.10 and p-p <= 0.60 code.  Score = worst ratio to its limit (<= 1 passes).
- Thin line: crop 64 x 256 on linear luminance, flank-fitted background, FWHM per row (reported at 1080p scale), monotone 3-level edge on >= 95 % of rows, centroid residual <= 0.25 px, energy variation <= 10 %.
