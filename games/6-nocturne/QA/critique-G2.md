# Critique G2 - beat 3 The Edge (Phase 3 graybox)

Captures: `QA/captures/G2_<run>/` (lookdev slot G2, keyshots1080 + beats). Severity S1 blocker, S2 major, S3 minor.

| ID | Sev | Beat | Description | Evidence (file, region) | Cause | Fix | Status |
|---|---|---|---|---|---|---|---|
| C01 | S2 | 3 | The upper tube (y 118, z 195, G3) ends in the air above the tongue root and in the garden notch: a dark tube converging over the walk | G2_r03/KS07 centre column x 930-990, y 0-330; G2_r01/KS04 x 950-970 | agreement 2 (no face A above y 0 for z 120..270) and 5 (notch) leave no face for its portals | lead decision G2-D4 (lintel over the notch / portals at a canyon-end mass / move the tube) | open (lead) |
| C02 | S2 | 2-3 | The ten boulevard lanes cannot portal into face B: all lie inside the notch (z 143..247); their D-019 end ramps (x 815 -> ~1 008) fade in open air over the garden and the plaza edge; three lanes cross hero-tree crowns (758, 142) and (762, 250) | arithmetic in docs/logs/G2.md G2-D4 | 14.5.2 lanes vs agreement 5 | lead decision G2-D4 | open (lead) |
| C03 | S2 | 2-3 | The koi and the Heart sky show through the open notch at KS04, KS07, KS11 (before the KS17 reveal) | G2_r01/KS04 x 880-1030, y 640-740; G2_r03/KS07 x 860-1010, y 270-360 | notch open to the sky (agreement 5) | lead decision (a lintel at y >= 140 does not hide it at KS04: the koi is seen through the notch at ~120 m height) | open (lead) |
| C04 | S3 | 3 | NaN-like dark pixels: the 0.8 m gaps between lit section bays fall to ~1 px at 150-600 m and stay isolated dark pixels | G2_r01/KS08 (1527, 541) crop x4; KS09 (1412, 16) | NocFacade fades by the 9 x 4 m pitch, not by the gap width | shader owner: fade the window shape by its gap footprint as well | open (A2b / I0) |
| C05 | S3 | 3 | The section faces read as a random mosaic (cyan / magenta / amber bays) on every eastward view | G2_r03/KS07, G2_r01/KS11 | NocFacade section-mode hue mix, 40 % lit | art direction / Phase 4 NocSection with interior mapping | open |
| C06 | S2 | 3 | Balustrade pucks bloomed to ~100 px halos and their realtime light left a floating specular orb on the glass | G2_r01b/beat03_edge_look_l x 1500-1700, y 600-760 | puck x1.8 WinCold; light 0.35 m from the smooth glass at y 0.5 | puck x0.75, light at y 0.16, 0.2 m inside the edge | fixed (G2_r02s, G2_r03 KS09 / KS10) |
| C07 | S3 | 3 | KS09 banding[abyss] 5.1-6.2: the D-025 abyss rectangle (y 0.60-0.95) lies on the pier (39 % of KS09) | G2_r03/KS09 bottom band | QA region definition | region per shot (lead / A1) | open |
| C08 | S3 | - | `-autotest -beat 3` fails beatsInOrder: BeatDirector enters beat 3 at s 400, the walker starts at s 0 | Logs/player-G2_autotest_b3.log | framework start handling | I0 / A1 | open |
| C09 | S3 | all | autotest gcAllocPerFrame 88-90 B/frame (Beat03 registers no updater) | QA/reports/autotest_brisk_until560.json | pre-existing (A1 log: ~125 B/frame) | I0 | open |
