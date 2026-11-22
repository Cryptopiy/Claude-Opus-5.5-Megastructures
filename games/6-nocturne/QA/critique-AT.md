# Critique AT - atmosphere, sky, post, light rig (Phase 4 wave A)

Columns: ID, severity (S1 = money shot / seen by every viewer, S2 visible, S3 minor), beat, description, evidence, cause, fix, status.
Captures: QA/captures/AT_rNN (this copy, keyshots1080). Shots looked at full size, as crops and as x2.5 / x6 contrast crops
(scratchpad at/*.png); numbers from scratchpad at/stats.py (8-bit Rec.709 luma) and Tools/qa/lookv2_check.py.

| ID | Sev | Beat | Description | Evidence | Cause | Fix | Status |
|---|---|---|---|---|---|---|---|
| INT-01 (AT part) | S1 | 3-4 | Abyss haze without visible layers at this exposure: KS10 drop x 820-940 rows 330-480 std 3.9, p5..p95 48.8-55.1 (r01); KS13 nadir flat (std 1.4-2.0) | AT_r01 KS09 / KS10 / KS13, x6 crops | sheets were brighter veils over a base that brightens with depth: holes showed equally bright deeper layers, thick shading cancelled coverage | dark veils over glowing depths (veils 0.66 / 0.55 / 0.55, band A 0.75 / 0.85 / 0.95, lift 0.7 L(e), DeepAmp 0.5); twin + tests | improved: r03 std 7.5, p5..p95 49.9-65.0; darker near band 38.9 -> lit deep 65; KS13 twin nadir p5..p95 57.8-71.0. Billows still soft at x1 (S2 open) |
| AT-01 | S2 | 3-4 | KS12 abyss rect (0.53-0.66 x 0.2-0.4) reads 39-47 (LOOK_V2 8.1 drop 50-66); it lies on the near (sheet 0) band | AT_r03 KS12 [1018,216,1267,432] mean 42.3 | the e -10..-18 rows sit under the near veil and the shallow lift | lighter near veil (0.45 -> 0.66) moved it 42.2 -> 42.3 only; next: lift from e -2 (LiftE0) or a narrower BandDn for sheet 0 | open |
| INT-11 | S2 | 2, 4, 7 | Deck lids without form | AT_r01 KS05 lid std 5.0 (gradient), billows +-2-3 codes | tint amplitude 0.5 | tint amp 0.7 (r02 0.8: KS05 p5..p95 68.7-81.5, soft billows at x1; KS18 deck billows and tower tops dissolving into the deck read in r01 already) | improved; final value 0.7 (see AT-02) |
| AT-02 | S3 | 1 | KS02 slit sky_top falls under its 50.4-70.4 window with strong deck billows (0.8: 50.3, 0.95: 48.5) | lookv2_AT_r03.json KS02 sky_top_luma 48.5 | the slit lies on a dark billow (f about -0.5) at its pose | tint amp 0.7 (expected about 51-52) | check in AT_r04 |
| INT-15 KS10 | S2 | 3 | abyss bands after x264 CRF 23 (lookv2_01: 5.13 steps / 1000) | QA/reports/encode_test_AT_e02_enc_beat03_abyss.md | smooth dark haze | veils / billows give the encoder texture | step gate PASS: 0.00 steps / 1000 (BandScore 27 on the rect, which now holds billows, lights and a face edge) |
| INT-15 KS02 | S3 | 1 | slit after x264 | encode_test_AT_e02_enc_beat01_slit.md | - | - | step gate PASS: 0.00 (BandScore 3.16 as before) |
| INT-15 KS04 | S2 | 2 | KS04 sky after x264 CRF 23: 3 steps of 1.3-1.4 codes over the 216 px profile (13.89 / 1000) | encode_test_AT_e02 / AT_e03_axis_d25 / AT_e03_axis_temporal / AT_e04_axis | not dither: peak 2.5, temporal dither and a +-7 % dome grain all give exactly 3 steps | grain reverted (LOOK_V2 7: no grain); next: locate the 3 rows (structure or chroma), wider lid gradient | open |
| AT-03 | S2 | 5 | KS15 garden canopy mean 17.2 (beat-5 window 24-42, KS15 24-42), crowns near the Floor | AT_r01 / r03 KS15 | no light on the crown undersides (garden content); the garden ambient set (sky NightMid x 1.6, ground Moss x 0.12) is small | needs lantern light on the undersides (content) or a lifted garden ground term (AtmosphereDriver GardenGroundMoss); not done: no garden owner in wave A, a uniform ambient would flatten the crowns | open |
| AT-04 | S3 | 3 | KS09 p99 depends on AB's rail bar: with the darker near band it fell to 113 in r02 (r03 168.7 with AB / DR content) | lookv2_AT_r02 / r03 | p99 >= 120 comes from light content | none in the sky | watch |
| D-034 | - | 3-4 | hooks for the hull (deck) and the abyss shadow | AtmosphereAbyssTests.Dread_OccludersAreIdentityAtZeroAndOnlyDarken | - | _NocDeckOcc / _NocHazeOcc, identity at 0 | done (DR uses its own veil meshes today) |

## Fix wave (panel p4_01, 2026-11-21; task Logs/fix/AT.md). Baseline INTEG QA/captures/p4_01 (master caddb90); captures QA/captures/AT_rNNf.

| ID | Sev | Beat | Description | Evidence | Cause | Fix | Status |
|---|---|---|---|---|---|---|---|
| C11 pools | S2 | 3-4 | The drop is a smooth umber gradient, no lateral structure, no city glow below | p4_01 KS10 drop x 800-940 row std 0.3-2.4; KS09 0.3-3.1 | the deep glow only acts below e -10 x smoothstep(10, 70) and the sheets are bands | under-city glow pools on y -2 100 (r01f 675 m: too big / sparse; r02f 337.5 / 112.5 m, smoothstep 0.45-0.85) | see AT_r02f |
| C11 sink | S2 | 3-4 | Gulf faces below the walk stay crisp | p4_01 KS11 face B below p95 90.0 vs above 26.1 | fog T near 1 for y > -250 within 500 m | r01f density sink (no visible change, kills AB's deep rivers: T 0.54 -> 0.15) -> depth veil in NocFog V 0.5 S01((-40 - y) / 360) | see AT_r02f; KS11 p95 <= 36 not reachable by air (windows at y 0..-220, 450 m: lit share is facade content) |
| C02-air | S2 | 7 | No light in the air around the koi | p4_01 KS17 beside 53.6 vs far 49.4 | no in-scatter term | NocKoiAir: closed-form Gaussian in-scatter of the 12 spine points (R 70 m, gain 0.03) in NocFog and the dome; r01f broke patch hue / dark cast -> no glow within 60-110 m of a point, whiter colour | r01f KS17 +14..+18 codes 50-80 px off the body, KS18 arc +23; re-check r02f |
| C26 | S2 | 2-7 | No sheet lightning while AU's thunder plays | DreadScore.LightningS, no shader term | not built | _NocFlash at DreadScore.LightningS (rise 0.4 / decay 1.5 s, +15 % deck, 450 m, ahead or on DR's hull) | r01f poses s 204/206 +0.99 % frame mean, deck +1-4 codes; s 553 on the hull |
| C44 | S3 | 1 | KS02 slit mean 51.7 < 55 | p4_01 KS02 slit rect | the slit lies on a dark billow | alley lift x (1 + 0.25 G(70 m)) at (0, 80) | r01f 55.9; KS04 / KS05 lids -0.3 / -0.1 |
| C46 | S3 | 1 | Alley wall texture lost in the encode | QA C19 4K encode pending | - | waits on QA's 4K CRF 23 encode / lookv2 --encoded | open (not run this wave) |
| KS04 enc | S2 | 2 | KS04 sky 3 encode steps (13.89 / 1000) | encode_test_AT_e02_enc_beat02_axis | - | widen the lid gradient (ruling) | open |

### Fix-wave status (03:56; final captures QA/captures/AT_r03f, 1080p, 20 key shots + flash poses)

| ID | Status | Evidence |
|---|---|---|
| C11 pools | done (partial on the column metric) | KS10 drop reads as a lit cloud sea under the veils (scratchpad at/ks10_p401_r02f.png); KS10 drop lateral row std rows 350 / 550: 5.1 / 6.4 (p4_01 0.9 / 0.9), rows 400 / 450 / 650: 2.8 / 2.1 / 2.2; twin median 4.32 vs 1.19; KS09 rows 600: 7.7; KS13 twin nadir mean 66.4, p5..p95 56.7-73.2; KS10 abyss encode 0.00 steps / 1000 (BandScore 27.2 -> 12.9) |
| C11 sink | done (veil); KS11 acceptance not reachable by air | depth veil T x (1 - 0.5 S01((-40 - y) / 360)): a face at y -400, 500 m out T 0.869 -> 0.435 (AT-final); KS11 face B below p95 88.8 (p4_01 90.0): windows at y 0..-220 -> REQUEST FX (lit share with depth) |
| C02-air | done (KS17 / KS18 1080p); KS18 patch fixed in batch 5 pending its capture | KS17 +12..+15 codes 50-80 px off the body at head / mid (+25 at the back), KS18 arc 66.7 vs 45.7; clipped 0 %; lookv2 KS17 pass; KS18 patch hue 9.9 / sat 0.56 at r03f -> hollow weight 30..70 m (AT-b14) |
| C26 | done | flash at DreadScore.LightningS, rise 0.4 / decay 1.5 s, +15 % deck max; poses s 206 / 554 / 791: frame mean +0.99 / +0.82 / +0.70 %; s 553 centred on DR's hull (515, 136), hull keeps 1.1 % vs 9.7 % beside it (AT-final) |
| C44 | done | KS02 slit 51.7 -> 55.9 (lookv2 sky_top 51.4 -> 55.4); KS04 / KS05 lid 62.9 / 73.7 unchanged; other aspects not captured |
| C46 | open (not run) | waits on QA's 4K CRF 23 encode / lookv2 --encoded |
| KS04 enc | open (located) | 3 steps are Cb-only (rows ~82-90 / 129-150 / 184-190; Cb slope +0.021 code/px, 4.5 codes over 216 px; Y 0 steps): a chroma ramp of the warm lid into the cooler haze, quantised by x264 4:2:0 at CRF 23; no luma fix applies |
