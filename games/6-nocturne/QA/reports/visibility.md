# Visibility and reachability audit (MASTER_PROMPT 11.13)

Generated 2026-11-16 04:13 by VisibilityAuditTests (V) in 4.5 s. Built world: 14 builders (Beat01_Alley, Beat02_Boulevard, Beat03_Edge, Beat04_Skybridge, Beat05_Garden, Beat06_Signs, Beat07_Heart, City00_Field, Relief, ZZZ_Halos, ZZ_FacadeSheet, ZZ_MaterialSheet, ZZ_RainEdgeProbe, ZZ_SignSheet); 2176 of 2185 mesh renderers as occluders, 815348 triangles (excluded: 8 transparent, 0 backdrop, 1 koi; 0 unreadable; 1828 read back; 0 city cells regenerated); world 1.6 s, BVH 3.2 s.
Subjects are spec-derived sample points (VisibilitySubjects); occlusion columns: **spec** = the analytic plan of 14.4/14.5 (VisibilitySpecWorld), **built** = the opaque meshes the builders produced (instanced signs, streaks, traffic, glass, koi and the sky dome are not occluders). Margin: |NDC| <= 0.94 (3 %) at 16:9, 16:10 and 21:9, at the table pose, the follower pose after a 40 m run (f40) and on the whole cinematic walk (fW).

| Shot | s | lag f40 / fW (deg) | readable s (spec) | reach | subjects ok | worst spec dev | composition (spec / built) | T-DROP |
|---|---|---|---|---|---|---|---|---|
| KS00 | 0 | 0.00 / 0.00 | 0.0-26.8 (0-25) | path | 1/1 | 0.00 |  | - |
| KS01 | 36 | 0.08 / 0.08 | 22.5-47.3 (20-45.5) | path | 2/2 | 0.04 |  | - |
| KS02 | 88 | 1.78 / 1.78 | 86.0-99.8 (84-98) | path | 5/5 | 0.12 |  | - |
| KS03 | 117 | 0.00 / 0.00 | 108.1-125.5 (106-123.5) | path | 3/3 | 0.01 |  | - |
| KS04 | 250 | 0.01 / 0.01 | 207.2-254.5 (205-253) | path | 5/5 | 0.14 |  | - |
| KS06 | 264 | 0.04 / 0.04 | 257.4-277.5 (255-275.5) | path | 2/2 | 0.61 |  | - |
| KS05 | 310 | 0.27 / 0.27 | 303.1-339.3 (301-337) | path | 5/5 | 0.24 |  | - |
| KS07 | 402 | 1.49 / 1.49 | 392.0-410.9 (390-409) | path | 3/3 | 0.02 |  | - |
| KS08 | 428 | 0.62 / 0.62 | 423.1-442.2 (421-440) | path | 1/1 | 0.55 |  | - |
| KS09 | 520 | 0.03 / 0.03 | 499.7-534.6 (498-534) | path | 4/4 | 1.03 | 25/39/2/4/30 / 25/38/4/2/31 (built near: City/Beat03_edge/Tongue_deck x875, City/Beat03_edge/Tongue_railShoe x59, City/Beat04_skybridge/gateA/block x40) | pass |
| KS10 | 548 | 0.00 / 0.00 | 537.3-564.5 (534-563.5) | path | 5/5 | 0.77 | 3/51/2/4/40 / 3/50/5/10/32 (built near: City/Beat03_edge/Tongue_deck x1159, City/Beat04_skybridge/gateA/block x227, City/Beat03_edge/Tongue_railShoe x68) | pass |
| KS11 | 600 | 0.01 / 0.01 | 590.5-607.8 (589-606) | path | 2/2 | 0.16 |  | - |
| KS12 | 660 | 0.04 / 0.04 | 642.0-667.7 (640-666) | path | 3/3 | 0.90 | 22/31/4/1/41 / 22/32/4/0/42 (built near: City/Beat04_skybridge/deckTop x704, City/Beat04_skybridge/rails/rods x64, City/Beat04_skybridge/truss x30) | pass |
| KS13 | 718 | 0.45 / 0.45 | 714.4-731.3 (712-730) | path | 3/3 | 0.44 | 41/59/0 / 54/46/0 | pass |
| KS14 | 846 | 0.02 / 0.02 | 822.2-854.9 (820-853) | path | 2/2 | 0.00 |  | - |
| KS15 | 970 | 0.09 / 0.09 | 959.0-1013.5 (957-1011.5) | path | 2/2 | n/a |  | - |
| KS16 | 1150 | 0.09 / 0.09 | 1135.9-1198.6 (1134-1196.5) | path | 3/3 | 0.00 | 59/44 / 51/39 | - |
| KS17 | 1296 | 0.27 / 0.27 | 1278.7-1325.3 (1277-1323) | path | 1/1 | 0.07 | koi min margin 0.237 visible / core 0.082 (spec 0.085) | - |
| KS18 | 1350 | 0.09 / 0.09 | 1338.6-1378.4 (1337-1376.5) | path | 0/0 | n/a | koi min margin 0.113 visible / core 0.180 (spec 0.178) | - |
| KS19 | 1500 | 0.00 / 0.00 | 1462.3-1500.0 (1460-1500) | path | 2/2 | 0.02 | koi min margin 0.106 visible / core -1.902 (spec 0.110); head hidden 160/160 | - |

## Subjects (16:9, target pose)

| Shot | subject | rule | NDC x | NDC y | spec | px 1080p (w x h) | px 4K | unoccluded spec / built | occluded by | ok |
|---|---|---|---|---|---|---|---|---|---|---|
| KS00 | mouth_slit | Whole | -0.02..0.02 | 0.03..0.20 | 4.4 m wide at 120 m: 33 px at 1080p, slit y +0.03..+0.20 | 33 x 89 | 66 x 179 | 1.00 / 0.88 | / City/Beat01_alley/fireEscape x2, City/Beat01_alley/acUnits x1 | ok |
| KS01 | bar_window | Whole | -0.15..-0.02 | -0.27..0.02 | bar window x -0.15..-0.02, y -0.27..+0.02 (about 250 px wide at 4K) | 127 x 156 | 254 x 313 | 1.00 / 1.00 |  | ok |
| KS01 | bar_sign | Whole | -0.21..0.00 | 0.06..0.55 | sign above x -0.21..+0.04, y +0.06..+0.57 | 197 x 268 | 395 x 536 | 1.00 / 1.00 |  | ok |
| KS01 | counter | Info | -0.15..0.05 | -0.51..-0.22 | counter y -0.51..-0.22 | 199 x 158 | 398 x 316 | 1.00 / 1.00 |  | ok |
| KS02 | zenith_cloud | Part | -0.09..0.07 | 0.70..0.78 | zenith cloud at y +0.74 | 151 x 47 | 303 x 94 | 0.88 / 0.88 | alley west tower x3 / City/Beat01_alley/towerW x3 | ok |
| KS02 | catwalk | Whole | -0.21..0.21 | -0.93..-0.70 | catwalk x +-0.21, y -0.93..-0.70 | 405 x 124 | 810 x 248 | 1.00 / 1.00 |  | ok |
| KS02 | roof_edge_west | Part | -0.11..-0.07 | -0.92..0.55 | west roof edge x -0.10..-0.07 (half-angle 6.19 deg) | 35 x 796 | 70 x 1593 | 1.00 / 0.83 | / City/Beat01_alley/catwalk x1 | ok |
| KS02 | roof_edge_east | Part | 0.05..0.08 | -0.72..0.95 | east roof edge x +0.06..+0.07 (half-angle 4.29 deg) | 28 x 905 | 55 x 1810 | 1.00 / 0.88 | / City/Beat01_alley/catwalk x1 | ok |
| KS02 | tower_flank | Part | -0.21..-0.12 | 0.46..0.72 | tower flank in the slit x -0.20..-0.10, y +0.58..+0.66 | 83 x 141 | 167 x 282 | 0.00 / 0.00 | alley west podium x15 / City/Beat01_alley/alleyWallW x15 | ok |
| KS03 | north_facade_base | Whole | -0.18..0.18 | -0.42..0.00 | north facade base x +-0.18, y -0.42..0 | 352 x 227 | 703 x 455 | 1.00 / 0.67 | / City/Beat02_boulevard/medianBarriers x5, City/Beat02_boulevard/planters x1, City/Beat02_boulevard/bollards x1 | ok |
| KS03 | blade_n54 | Whole | 0.28..0.41 | 0.00..0.67 | blade x 54 (lower 70 m) x +0.28..+0.41, y 0..+0.66 | 123 x 360 | 245 x 719 | 1.00 / 1.00 |  | ok |
| KS03 | blade_n90 | Whole | 0.47..0.62 | 0.00..0.67 | blade x 90 (lower 70 m) x +0.46..+0.62, y 0..+0.66 | 151 x 360 | 302 x 719 | 1.00 / 1.00 |  | ok |
| KS04 | blade_n198 | Whole | -0.47..-0.31 | -0.20..0.69 | north blade x 198 (y 40-140) x -0.47..-0.31, y -0.20..+0.69 | 153 x 481 | 306 x 962 | 1.00 / 1.00 |  | ok |
| KS04 | blade_s186 | Whole | 0.32..0.49 | 0.04..0.91 | south blade x 186 (y 60-160) x +0.32..+0.49, y +0.04..+0.91 | 159 x 473 | 319 x 946 | 1.00 / 1.00 |  | ok |
| KS04 | canyon_end | Part | -0.44..0.44 | -0.64..-0.64 | canyon end at y -0.64 | 841 x 0 | 1682 x 0 | 1.00 / 0.40 | / City/Beat02_boulevard/terminus x5, City/Beat02_boulevard/medianBarriers x2, City/Beat02_boulevard/lampPosts x1 | ok |
| KS04 | face_B | Part | -0.37..0.37 | -0.63..-0.19 | far Gulf face (x 815.3, 750 m away) at x 0, y -0.63..-0.19 | 718 x 233 | 1436 x 467 | 0.98 / 0.72 | gate A block x2 / City/Beat02_boulevard/terminus x15, City/Beat05_garden/balustrade x3, City/Beat05_garden/trees/crown_cards_B1 x2 | ok |
| KS04 | lane_n185 | Part | -0.22..-0.06 | -0.22..0.95 | north lane (y 112) x -0.36..-0.10 | 159 x 630 | 319 x 1260 | 1.00 / 0.84 | / City/Beat03_edge/CanyonBridge_facade x2, City/Beat03_edge/CanyonBridge_concrete x2 | ok |
| KS06 | tram | Whole | -0.24..0.23 | -0.44..-0.30 | tram (x 109.8-169.8) x -0.24..+0.23, y -0.43..-0.29 | 451 x 75 | 903 x 150 | 1.00 / 1.00 |  | ok |
| KS06 | north_facade | Part | -0.68..0.16 | -0.07..0.92 | north facade (y 40-110) above at y +0.54..+0.66 | 804 x 538 | 1607 x 1077 | 1.00 / 1.00 |  | ok |
| KS05 | cloud_lines | Part | -0.16..0.16 | -0.49..0.84 | facade-cloud lines at x +-0.12, y +0.13 (rising apart toward the top) | 306 x 723 | 613 x 1446 | 1.00 / 0.84 | / City/Beat03_edge/CanyonBridge_concrete x8 | ok |
| KS05 | blade_n198_top | Whole | -0.32..-0.24 | 0.39..0.59 | north blade x 198 top x -0.25, y +0.50..+0.59 | 76 x 107 | 152 x 213 | 1.00 / 1.00 |  | ok |
| KS05 | blade_s186_top | Whole | 0.24..0.31 | 0.53..0.71 | south blade x 186 top x +0.24, y +0.62..+0.71 | 71 x 96 | 142 x 191 | 1.00 / 1.00 |  | ok |
| KS05 | blade_n162_mid | Whole | -0.65..-0.34 | 0.29..0.76 | blade x 162 mid x -0.42..-0.40, y +0.53..+0.69 | 294 x 255 | 588 x 510 | 1.00 / 1.00 |  | ok |
| KS05 | lane_skyway_320 | Part | 0.00..0.00 | -0.78..0.99 | skyway lane (y 320) y -0.08 | 0 x 953 | 0 x 1906 | 1.00 / 0.85 | / City/Beat03_edge/CanyonBridge_facade x2, City/Beat03_edge/CanyonBridge_concrete x2 | ok |
| KS07 | garden_crowns | Whole | -0.09..0.09 | 0.04..0.34 | garden crowns (box x 720-850, y 40-130) x +-0.08, y +0.04..+0.34 (362 px tall at 4K) | 182 x 160 | 363 x 321 | 1.00 / 0.96 | / City/Beat05_garden/NotchGate_concrete x1 | ok |
| KS07 | far_deck_gate_opening | Part | -0.01..0.01 | -0.06..-0.03 | far tongue tip and deck line at y -0.06..-0.03 through the 12 m gate opening (0.067 wide) | 21 x 14 | 43 x 29 | 0.80 / 0.75 | bridge post x4 / City/Beat04_skybridge/rails/posts x4, City/Beat04_skybridge/lamps/poles x1 | ok |
| KS07 | face_B_above | Part | -0.43..0.43 | 0.68..0.95 | face B at y 293 projects to y +0.68 (y 400 to +0.95) | 821 x 143 | 1642 x 286 | 1.00 / 1.00 |  | ok |
| KS08 | north_flank_face | Part | -1.00..-0.39 | -0.72..0.84 | north flank's east face near x -0.62..-0.48, y -0.17..+0.57 (far x -0.58..-0.50, y -0.22..+0.31) | 580 x 847 | 1161 x 1694 | 0.81 / 0.40 | boulevard north facade x13 / City/Beat02_boulevard/podiumNE x24, City/Beat03_edge/GulfRibA_facade x11, City/Beat03_edge/GulfRibA_steel x2 | ok |
| KS09 | face_A_strata | Part | -0.98..-0.49 | -0.93..0.99 | face A strata x -0.78..-0.66, y -0.03..+0.48 | 473 x 1038 | 946 x 2076 | 0.86 / 0.62 | tongue A deck x11, tongue balustrade rail x3 / City/Beat03_edge/GulfRibA_facade x17, City/Beat03_edge/Tongue_deck x10, City/Beat03_edge/GulfRibA_steel x5 | ok |
| KS09 | face_B | Part | 0.09..0.93 | -0.87..0.98 | face B x +0.10..+0.38, y +0.16..+0.48 | 814 x 996 | 1627 x 1992 | 0.83 / 0.70 | tongue A deck x16, tongue balustrade rail x3, gate A block x1 / City/Beat03_edge/Tongue_deck x15, City/Beat03_edge/GulfRibB_facade x12, City/Beat03_edge/Tongue_railTop x3 | ok |
| KS09 | cross_bridge_850 | Whole | -0.63..0.24 | -0.05..0.13 | cross bridge (z 850, y -180) x -0.63..+0.24, y -0.01..+0.13 | 839 x 96 | 1678 x 193 | 1.00 / 0.94 | / City/Beat03_edge/GulfRibA_facade x2, City/Beat03_edge/FaceA_c1 x1, City/Beat03_edge/TrafficLayer_350_-120 x1 | ok |
| KS09 | layer_520 | Whole | -0.21..-0.10 | -0.24..0.13 | traffic layer (y -520, z 1 200-2 500) x -0.21..-0.10, y -0.24..+0.13 | 108 x 196 | 215 x 393 | 1.00 / 0.85 | / City/Beat03_edge/CrossBridge_850_-180 x2, City/Beat03_edge/Tongue_railShoe x1, City/Beat03_edge/CrossBridge_850_-180_lights x1 | ok |
| KS10 | cross_bridge_850 | Whole | -0.24..0.69 | 0.38..0.45 | cross bridge (z 850, y -180) x -0.69..+0.22, y +0.29..+0.45 | 894 x 42 | 1788 x 83 | 1.00 / 0.73 | / City/Beat04_skybridge/gateA/block x14, City/Beat03_edge/FaceA_c1 x2, City/Beat03_edge/GulfRibA_steel x1 | ok |
| KS10 | layer_520 | Whole | 0.17..0.21 | 0.11..0.36 | layer y -520 (x 560, z 1 200-1 800) x -0.19..-0.12, y +0.07..+0.31 | 38 x 139 | 76 x 278 | 1.00 / 1.00 |  | ok |
| KS10 | layer_120 | Part | -0.02..0.08 | -0.45..0.84 | layer y -120 (x 350) x -0.34..-0.33, y +0.32..+0.70 | 97 x 698 | 195 x 1396 | 0.96 / 0.93 | tongue A deck x1 / City/Beat03_edge/Tongue_deck x1, City/Beat03_edge/Tongue_railShoe x1 | ok |
| KS10 | face_B | Part | 0.42..0.97 | -0.75..0.92 | faces at x +0.09 and -0.81 (y +0.46, +0.32) | 531 x 904 | 1062 x 1808 | 0.56 / 0.14 | gate A block x22, tongue A deck x10, tongue balustrade rail x3 / City/Beat04_skybridge/gateA/block x50, City/Beat03_edge/Tongue_deck x10, City/Beat03_edge/Tongue_railTop x3 | ok |
| KS10 | face_A | Part | -0.58..-0.15 | -0.86..0.92 | faces at x +0.09 and -0.81 (y +0.46, +0.32) | 416 x 966 | 831 x 1932 | 0.78 / 0.48 | tongue A deck x16, tongue balustrade rail x3 / City/Beat03_edge/GulfRibA_facade x23, City/Beat03_edge/Tongue_deck x15, City/Beat03_edge/GulfRibA_steel x3 | ok |
| KS11 | T1_nose | Whole | -0.14..-0.02 | -0.12..0.04 | T1's nose 34 m ahead, 12 m left: x -0.14..-0.06, y -0.12..+0.04 (165 px wide at 4K) | 117 x 84 | 234 x 168 | 1.00 / 1.00 |  | ok |
| KS11 | T1_cars | Part | -0.09..0.17 | -0.11..0.02 | its cars converge to the right at x +0.07..+0.15, y -0.09..-0.04 | 248 x 72 | 495 x 143 | 1.00 / 0.98 | / City/Beat04_skybridge/lamps/poles x2 | ok |
| KS12 | face_A_far | Part | -0.86..-0.29 | -0.99..0.98 | face A far x -0.54..-0.44, y -0.09..+0.55 | 545 x 1064 | 1091 x 2129 | 0.87 / 0.68 | bridge deck x8, bridge top rail x6, bridge rod x3 / City/Beat03_edge/GulfRibA_facade x21, City/Beat04_skybridge/deckTop x8, City/Beat04_skybridge/truss x4 | ok |
| KS12 | face_B_far | Part | 0.28..0.83 | -0.99..0.98 | face B far x +0.38..+0.62 | 525 x 1064 | 1049 x 2129 | 0.83 / 0.69 | bridge deck x7, bridge top rail x6, bridge post x6 / City/Beat03_edge/GulfRibB_facade x19, City/Beat04_skybridge/rails/posts x8, City/Beat04_skybridge/deckTop x7 | ok |
| KS12 | layer_520 | Whole | 0.04..0.07 | -0.27..0.05 | traffic layer y -520 (z 1 100-1 800) x +0.01..+0.02, y -0.27..+0.05 | 27 x 175 | 53 x 350 | 0.80 / 0.93 | bridge rod x3 / City/Beat04_skybridge/rails/rods x1 | ok |
| KS12 | north_tube_T3 | Info | -10.01..6.75 | 0.33..0.92 | the north tube and T3 (abeam at s 653.6) cross the upper-left foreground | 16085 x 319 | 32171 x 637 | 1.00 / 1.00 |  | ok |
| KS13 | glass_strip | Part | -0.63..0.63 | -0.41..0.43 | the glass strip as a trapezoid x +-0.60, y -0.14..+0.83 | 1215 x 453 | 2431 x 906 | 1.00 / 1.00 |  | ok |
| KS13 | layer_520_below | Part | -0.18..0.16 | -0.33..-0.33 | the layer at y -520 (x 0, y -0.41) | 326 x 0 | 651 x 0 | 1.00 / 0.67 | / City/Beat04_skybridge/truss x3 | ok |
| KS13 | T6_lower_tube | Part | -0.11..-0.08 | -0.99..-0.40 | T6 in the lower tube at x -0.09, y -0.55..-0.26 (visible under the strip) | 30 x 321 | 61 x 643 | 1.00 / 0.93 | / City/Beat04_skybridge/truss x4 | ok |
| KS14 | gate_B_frame | Whole | -0.26..0.26 | -0.64..0.42 | gate B frame opening x -0.26..+0.26, y -0.64..+0.42 | 496 x 573 | 992 x 1146 | 1.00 / 0.94 | / City/Beat04_skybridge/rails/posts x2 | ok |
| KS14 | trees_above | Part | -0.98..0.98 | -0.24..0.77 | the glowing trees above | 1880 x 548 | 3761 x 1096 | 0.35 / 0.45 | gate B block x4, gate B block lintel x4, gate B frame lintel x3 / City/Beat04_skybridge/gateB/block x8, City/Beat04_skybridge/gateB/frame x3 | ok |
| KS15 | crown_T3 | Part | -0.96..-0.16 | -0.16..0.99 | crown T3 at (806, 232), bottom 33 m up, 37-42 m away | 768 x 621 | 1536 x 1243 | 1.00 / 1.00 |  | ok |
| KS15 | crown_T7 | Part | 0.10..0.98 | -0.01..0.97 | crown T7 at (800, 160) | 844 x 526 | 1688 x 1053 | 1.00 / 1.00 |  | ok |
| KS16 | wall_p10_300 | Whole | -0.15..-0.15 | -0.62..-0.62 | wall point (y 10, z 300) x -0.15, y -0.62 | 0 x 0 | 0 x 0 | 1.00 / 1.00 |  | ok |
| KS16 | wall_p150_350 | Whole | 0.13..0.13 | 0.80..0.80 | wall point (y 150, z 350) x +0.13, y +0.80 | 0 x 0 | 0 x 0 | 1.00 / 1.00 |  | ok |
| KS16 | wall_p300_420 | Whole | 0.19..0.19 | 0.93..0.93 | wall point (y 300, z 420) x +0.19, y +0.93 | 0 x 0 | 0 x 0 | 1.00 / 1.00 |  | ok |
| KS17 | spire | Part | -0.17..-0.02 | -0.51..0.98 | spire x -0.10..-0.08 (276 px at 4K) | 147 x 801 | 295 x 1602 | 0.92 / 0.92 | old quarter neighbour S x1, tenement x1, old quarter neighbour N x1 / City/Beat07_heart/neighbours x2, City/Beat07_heart/tenement x1 | ok |
| KS17 | goal_window | Info | -0.10..-0.09 | -0.48..-0.47 | the goal window is 5.1 x 6.4 px at 1080p (10.2 x 12.7 at 4K) at 212 m | 5 x 6 | 10 x 13 | 1.00 / 1.00 |  | ok |
| KS18 | spire | Info | 0.13..0.60 | -1.61..0.45 | 20 of 164 sample points pass behind the spire | 449 x 1115 | 898 x 2230 | 1.00 / 1.00 |  | ok |
| KS19 | goal_window | Whole | -0.17..-0.03 | -0.26..0.02 | window x -0.15..-0.03, y -0.26..+0.02 (138 x 172 px at 1080p, 276 x 345 at 4K, 8 m away) | 135 x 151 | 270 x 302 | 1.00 / 0.83 | / City/Beat07_heart/railRods x2 | ok |
| KS19 | goal_lamp | Whole | -0.11..-0.08 | -0.25..-0.20 | the table lamp (0.2 m sphere at (1 157.2, 5.0, 387.9)) seen through the window | 20 x 25 | 41 x 51 | 1.00 / 1.00 |  | ok |
| KS19 | spire | Info | -0.31..0.08 | -0.91..1.72 | spire shaft behind the tenement (intended: hidden) | 370 x 1419 | 740 x 2838 | 0.00 / 0.00 | tenement x22, goal balcony slab x1 / City/Beat07_heart/tenement x19, City/Beat07_heart/darkWindows x2, City/Beat07_heart/balcony x1 | ok |

## Koi lint (KoiMath sample set, 20 wave phases)

| Shot | pose | aspect | visible NDC x | visible NDC y | min margin (visible) | all samples | core 164 | head x / y | outside |
|---|---|---|---|---|---|---|---|---|---|
| KS17 | target | 16:9 | -0.50..0.44 | 0.02..0.76 | 0.243 | -0.701 | 0.087 | -0.50..-0.42 / 0.49..0.61 | 0 |
| KS17 | target | 16:10 | -0.55..0.49 | 0.02..0.76 | 0.243 | -0.701 | 0.087 | -0.55..-0.46 / 0.49..0.61 | 0 |
| KS17 | target | 21:9 | -0.37..0.33 | 0.02..0.76 | 0.243 | -0.701 | 0.087 | -0.37..-0.31 / 0.49..0.61 | 0 |
| KS17 | follower40 | 16:9 | -0.50..0.44 | 0.03..0.76 | 0.237 | -0.710 | 0.082 | -0.50..-0.42 / 0.50..0.62 | 0 |
| KS17 | follower40 | 16:10 | -0.56..0.48 | 0.03..0.76 | 0.237 | -0.710 | 0.082 | -0.56..-0.47 / 0.50..0.62 | 0 |
| KS17 | follower40 | 21:9 | -0.38..0.33 | 0.03..0.76 | 0.237 | -0.710 | 0.082 | -0.38..-0.32 / 0.50..0.62 | 0 |
| KS17 | followerWalk | 16:9 | -0.50..0.44 | 0.03..0.76 | 0.237 | -0.710 | 0.082 | -0.50..-0.42 / 0.50..0.62 | 0 |
| KS17 | followerWalk | 16:10 | -0.56..0.48 | 0.03..0.76 | 0.237 | -0.710 | 0.082 | -0.56..-0.47 / 0.50..0.62 | 0 |
| KS17 | followerWalk | 21:9 | -0.38..0.33 | 0.03..0.76 | 0.237 | -0.710 | 0.082 | -0.38..-0.32 / 0.50..0.62 | 0 |
| KS17 | core samples hidden per phase: spire max 17 (mean 17.0), arcade max 40 (mean 34.3), other 0, built 58; head occluded 0; body 54-219 px thick at 4K | | | | | | | | |
| KS18 | target | 16:9 | -0.46..0.80 | -0.53..0.82 | 0.183 | 0.183 | 0.183 | -0.22..-0.08 / 0.57..0.82 | 0 |
| KS18 | target | 16:10 | -0.51..0.89 | -0.53..0.82 | 0.113 | 0.113 | 0.183 | -0.25..-0.09 / 0.57..0.82 | 0 |
| KS18 | target | 21:9 | -0.34..0.60 | -0.53..0.82 | 0.183 | 0.183 | 0.183 | -0.17..-0.06 / 0.57..0.82 | 0 |
| KS18 | follower40 | 16:9 | -0.46..0.80 | -0.53..0.82 | 0.180 | 0.180 | 0.180 | -0.22..-0.08 / 0.57..0.82 | 0 |
| KS18 | follower40 | 16:10 | -0.51..0.88 | -0.53..0.82 | 0.115 | 0.115 | 0.180 | -0.25..-0.09 / 0.57..0.82 | 0 |
| KS18 | follower40 | 21:9 | -0.34..0.60 | -0.53..0.82 | 0.180 | 0.180 | 0.180 | -0.17..-0.06 / 0.57..0.82 | 0 |
| KS18 | followerWalk | 16:9 | -0.46..0.80 | -0.53..0.82 | 0.180 | 0.180 | 0.180 | -0.22..-0.08 / 0.57..0.82 | 0 |
| KS18 | followerWalk | 16:10 | -0.51..0.88 | -0.53..0.82 | 0.115 | 0.115 | 0.180 | -0.25..-0.09 / 0.57..0.82 | 0 |
| KS18 | followerWalk | 21:9 | -0.34..0.60 | -0.53..0.82 | 0.180 | 0.180 | 0.180 | -0.17..-0.06 / 0.57..0.82 | 0 |
| KS18 | core samples hidden per phase: spire max 19 (mean 18.0), arcade max 0 (mean 0.0), other 0, built 22; head occluded 0; body 48-351 px thick at 4K | | | | | | | | |
| KS19 | target | 16:9 | 0.61..0.99 | 0.43..1.00 | 0.003 | -1.902 | -1.902 | -0.69..-0.57 / 0.71..0.89 | 0 |
| KS19 | target | 16:10 | 0.67..1.00 | 0.43..1.00 | 0.001 | -1.902 | -1.902 | -0.77..-0.63 / 0.71..0.89 | 0 |
| KS19 | target | 21:9 | 0.45..0.82 | 0.43..1.00 | 0.003 | -1.902 | -1.902 | -0.52..-0.42 / 0.71..0.89 | 0 |
| KS19 | follower40 | 16:9 | 0.61..0.99 | 0.43..1.00 | 0.003 | -1.902 | -1.902 | -0.69..-0.57 / 0.71..0.89 | 0 |
| KS19 | follower40 | 16:10 | 0.67..1.00 | 0.43..1.00 | 0.001 | -1.902 | -1.902 | -0.77..-0.63 / 0.71..0.89 | 0 |
| KS19 | follower40 | 21:9 | 0.45..0.82 | 0.43..1.00 | 0.003 | -1.902 | -1.902 | -0.52..-0.42 / 0.71..0.89 | 0 |
| KS19 | followerWalk | 16:9 | 0.61..0.99 | 0.43..1.00 | 0.003 | -1.902 | -1.902 | -0.69..-0.57 / 0.71..0.89 | 0 |
| KS19 | followerWalk | 16:10 | 0.67..1.00 | 0.43..1.00 | 0.001 | -1.902 | -1.902 | -0.77..-0.63 / 0.71..0.89 | 0 |
| KS19 | followerWalk | 21:9 | 0.45..0.82 | 0.43..1.00 | 0.003 | -1.902 | -1.902 | -0.52..-0.42 / 0.71..0.89 | 0 |
| KS19 | core samples hidden per phase: spire max 0 (mean 0.0), arcade max 0 (mean 0.0), other 128, built 128; head occluded 160; body 73-311 px thick at 4K | | | | | | | | |

## Pre-reveal koi (D-029 GB-03: KS04-KS16 and the beat 2-6 stills, 20 wave phases, built meshes)

| Pose | s | yaw / pitch | koi in frame | koi visible | via notch / promenade / arcade / other | spire in frame / visible / via notch |
|---|---|---|---|---|---|---|
| KS04_boulevard_axis | 250.0 | 90 / 22 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| KS06_tram | 264.0 | 57 / 14 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| KS05_boulevard_up | 310.0 | 90 / 52 | 20 | 0 | 0 / 0 / 0 / 0 | 13 / 0 / 0 |
| KS07_edge_window | 402.0 | 90 / 2 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| KS08_edge_flank | 428.0 | 28 / 24 | 1790 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| KS09_edge_gulf | 520.0 | 20 / -18 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| KS10_edge_plunge | 548.0 | -5 / -32 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| KS11_bridge_train | 600.0 | 76 / 2 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| KS12_bridge_void | 660.0 | 0 / -20 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| KS13_bridge_glass | 718.0 | 90 / -75 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| KS14_garden_gate | 846.0 | 90 / 18 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| KS15_garden_canopy | 970.0 | 90 / 50 | 3640 | 0 | 0 / 0 / 0 / 0 | 32 / 0 / 0 |
| KS16_signs_wall | 1150.0 | -35 / 34 | 57 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat02_boulevard_start | 122.0 | 0 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat02_boulevard_mid | 260.0 | 90 / 0 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| beat02_boulevard_end | 398.0 | 90 / 0 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| beat02_boulevard_look_r | 260.0 | -180 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat02_boulevard_look_l | 260.0 | 0 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat02_boulevard_look_back | 260.0 | -90 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat02_boulevard_look_up60 | 260.0 | 90 / 60 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat03_edge_start | 402.0 | 90 / 0 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| beat03_edge_mid | 480.0 | 90 / 0 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| beat03_edge_end | 558.0 | 90 / 0 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| beat03_edge_look_r | 480.0 | -180 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat03_edge_look_l | 480.0 | 0 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat03_edge_look_back | 480.0 | -90 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat03_edge_look_up60 | 480.0 | 90 / 60 | 0 | 0 | 0 / 0 / 0 / 0 | 6 / 0 / 0 |
| beat03_edge_look_down60 | 480.0 | 90 / -60 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat04_skybridge_start | 562.0 | 90 / 0 | 3660 | 0 | 0 / 0 / 0 / 0 | 48 / 0 / 0 |
| beat04_skybridge_mid | 720.0 | 90 / 0 | 3660 | 0 | 0 / 0 / 0 / 0 | 42 / 0 / 0 |
| beat04_skybridge_end | 878.0 | 90 / 0 | 3601 | 0 | 0 / 0 / 0 / 0 | 34 / 0 / 0 |
| beat04_skybridge_look_r | 720.0 | -180 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat04_skybridge_look_l | 720.0 | 0 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat04_skybridge_look_back | 720.0 | -90 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat04_skybridge_look_up60 | 720.0 | 90 / 60 | 285 | 0 | 0 / 0 / 0 / 0 | 15 / 0 / 0 |
| beat04_skybridge_look_down60 | 720.0 | 90 / -60 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat05_garden_start | 882.0 | 90 / 0 | 3617 | 0 | 0 / 0 / 0 / 0 | 34 / 0 / 0 |
| beat05_garden_mid | 980.0 | 90 / 0 | 869 | 0 | 0 / 0 / 0 / 0 | 27 / 0 / 0 |
| beat05_garden_end | 1078.0 | 49 / 0 | 1795 | 0 | 0 / 0 / 0 / 0 | 24 / 0 / 0 |
| beat05_garden_look_r | 980.0 | -180 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat05_garden_look_l | 980.0 | 0 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat05_garden_look_back | 980.0 | -90 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat05_garden_look_up60 | 980.0 | 90 / 60 | 2994 | 0 | 0 / 0 / 0 / 0 | 23 / 0 / 0 |
| beat06_signs_start | 1082.0 | 41 / 0 | 1542 | 0 | 0 / 0 / 0 / 0 | 22 / 0 / 0 |
| beat06_signs_mid | 1170.0 | 0 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat06_signs_end | 1258.0 | 41 / 0 | 4 | 0 | 0 / 0 / 0 / 0 | 11 / 9 / 0 |
| beat06_signs_look_r | 1170.0 | 90 / 0 | 1057 | 0 | 0 / 0 / 0 / 0 | 24 / 0 / 0 |
| beat06_signs_look_l | 1170.0 | -90 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat06_signs_look_back | 1170.0 | -180 / 0 | 0 | 0 | 0 / 0 / 0 / 0 | 0 / 0 / 0 |
| beat06_signs_look_up60 | 1170.0 | 0 / 60 | 2450 | 0 | 0 / 0 / 0 / 0 | 17 / 0 / 0 |

## Failures (fix and owner)

- **KS02 pose** (readable, owner lead): readable s 86.0-99.8 (5.5 s; spec s 84-98) below 5.6 s. Fix: widen the gaze hold around the key.
- **KS02 tower_flank** (hidden, owner lead (spec / pose)): only 0.00 (spec) / 0.00 (built) of the in-frame samples unoccluded; by alley west podium x15 City/Beat01_alley/alleyWallW x15. Fix: move or lower the occluder, or move the subject / pose; if the spec subject cannot be seen from the walk, log a decision (Part 1.6, defect 39); no eye shift within +-6 m along the path clears it.
- **KS09 composition** (composition, owner lead): spec model shares sky 0.250, deck 0.389, rail 0.016, near structure 0.043, drop 0.302 vs spec sky/pier/drop 0.26/0.39/0.36. Fix: the near structure (gate block / frame) takes part of the drop: move the pose (yaw / s) or narrow the structure; else correct the 14.5 share numbers (Part 1.6).
- **KS10 composition** (composition, owner lead): spec model shares sky 0.028, deck 0.515, rail 0.020, near structure 0.036, drop 0.401 vs spec sky/pier/drop 0.07/0.50/0.43. Fix: the near structure (gate block / frame) takes part of the drop: move the pose (yaw / s) or narrow the structure; else correct the 14.5 share numbers (Part 1.6).
- **KS10 composition** (composition-built, owner G2 / G3): built shares sky 0.028, deck 0.503, rail 0.046, near structure 0.100, drop 0.324 vs spec sky/pier/drop 0.07/0.50/0.43. Fix: the near structure (gate block / frame) takes part of the drop: move the pose (yaw / s) or narrow the structure; else correct the 14.5 share numbers (Part 1.6).
- **KS10 composition** (composition-built, owner G2 / G3): built: KS10 drop 0.324 < 0.40 (14.5.3 acceptance 2); near structure 0.100. Fix: keep gate block A out of the KS10 frame (yaw / s) or narrow the pier / pitch further down.

## Notes

- KS19: the koi head is hidden in 160 of 160 sample-phases (by tenement (spec)): the head is at elevation 44.3 deg, 513 m away; along its azimuth the occluder's top edge is at 50.6 deg; not a subject (D-029 GB-01)
