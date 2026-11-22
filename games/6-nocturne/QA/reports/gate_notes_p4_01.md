# Lead notes on the p4_01 gate (2026-11-21 01:25)

- Autotests exact: cinematic trigger 599.050 / stop 601.450 s, walk 998.100 / 1002.100 s (autotest_cinematic.json, autotest_walk.json). Soak 1 200 s PASS (0 errors, managed growth -0.18 MB). Wall bot 2.5 m/s PASS (wallbot_brisk.json); the 1.5 m/s wall bot is re-running (the first run hit a 30 min timeout: it needs about 35 min).
- EditMode 464 / 465 (1 explicit skip), Validate 127 / 127, build 0 errors, smoke PASS.
- Sequence sets PASS: signs (photosensitivity), rain, koi, traffic, texpans, dolly.
- Boundaries: b12, b23, b34, b45, b56 PASS. The FAILs of b67, s380 and s96 in sequences_p4_01_boundaries.md are "frame N readable" failures: the disk filled during that capture and the frames were cut. They are NOT picture defects. They are being re-captured one at a time (bnd_* reports, then statics, pans, edges).
- Encode (x264 CRF 23, 1080p): 10 of 11 clips PASS (abyss, sky, slit, axis, void, crown, wall, goal, plaza, mouth). enc_beat01_wall FAILs on BandScore 29.7 with 0.00 visible steps per 1 000 px (the same measurement-only case as SG-07: a flat region scored by run length, no visible step).
- Statics: sequences_p4_01_statics.md was captured WITHOUT -freeze-clock -rain 0 (14.10 asks for both), so its "static change" FAILs are rain and clock motion, not defects. The valid run is sequences_p4_01_staticsfz.md (landing about 01:35).
- Boundary re-runs: bnd_b67 PASS (the earlier b67 FAIL was the full disk); bnd_s96, bnd_s380, bnd_s1240 follow.
- 01:36 final sequence results: boundaries b12 b23 b34 b45 b56 PASS (sequences_p4_01_boundaries.md), b67 s96 s380 s1240 PASS (sequences_p4_01_bnd_*.md); statics with -freeze-clock -rain 0: 7 / 7 PASS, max change 0.00-0.72 levels (sequences_p4_01_staticsfz.md); pans 7 / 7 PASS (sequences_p4_01_pans.md); edges: start / end captured, no automatic checks (SKIP). sequences_p4_01_statics.md (no freeze) is invalid.
- 01:40 wall bot 1.5 m/s PASS (left / right max excess -295.7 mm, errors 0; wallbot_walk.json). Every gate run of p4_01 is in.
