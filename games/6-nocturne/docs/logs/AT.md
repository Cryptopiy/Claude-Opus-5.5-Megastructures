# AT log - atmosphere, sky, post and the global light rig (Phase 4 wave A, D-033)

Owner files: Shaders/Include/NocAtmosphere.hlsl; Scripts/Runtime/Core/NocSchedule.cs; Shaders/NocSky.shader; Shaders/NocGrade.shader;
Scripts/Runtime/Rendering/NocGradeMath.cs, NocGradeFeature.cs, NocGradePass.cs, NocHalos.cs; Scripts/Runtime/Systems/AtmosphereDriver.cs;
Editor/MaterialFactory.Look.cs, Runtime/Core/MaterialLibrary.Look.cs, Tests/EditMode/MaterialTests.Look.cs; Tests/EditMode/AtmosphereTests.cs,
GradeTests.cs (+ new test files of mine); Tools/qa/lookv2_check.py; docs/logs/AT.md; QA/critique-AT.md.
Tasks (in order): 1 INT-01 haze and depths (H3) with AB; 2 INT-11 cloud deck form; 3 H1 banding INT-15; 4 exposure / highlights / lookv2_check
(D-032 4: beat-1 |R - G| <= 6; QA-1 KS07 separation box); 5 requests for NocHalos / AtmosphereDriver.

## Start (2026-11-16 08:15)
- First start of this wave (no earlier AT log). Read: DEVELOPMENT D-030..D-033, BUILDER_BRIEF, STATE, MASTER_PROMPT 14.0 / 14.2.2 / 14.5.3 /
  14.7 H1 / H3, LOOK_V2 3 / 6 / 7 / 8, critique-lookv2-01 (INT-01, -03, -11, -15, GB-07, QA-1), PA log (previous owner).
- Looked at lookv2_01 KS09 / KS10 at full size. Abyss luma profiles (median of 8-row strips): KS10 x 1180-1380 rows 0-500: 43.6 -> 39.7
  (row 140) -> 47.1, RGB (46, 43, 40) at mid; KS09 x 450-640: 39-45; KS12 x 600-800: 41-42; KS13 through the glass 63-66. LOOK_V2 8.1 asks
  drop 50-66 (KS09 / KS10 / KS12), 60-66 (KS13): the shallow abyss (e 0..-30, the only part these shots see) is 8-15 codes under, and the
  sheets' sky term (x (1 + 0.15 m), 5 deg Gaussians) gives ~3 codes of smooth variation: no layers, no texture.
- 09:35 new test file Tests/EditMode/AtmosphereAbyssTests.cs (+ .meta) (AB owns Beats/AbyssTests.cs: renamed mine to avoid the class clash).

## Step 1 - the abyss of H3 (INT-01) and the deck form (INT-11): design (08:30-09:40)
- Prototype: numpy twin of the sky function (scratch abyss_model.py, exact PCG / value-noise / rotation / seed twin of the HLSL),
  sky-only 1080p renders of the KS09 / KS10 / KS12 / KS13 poses. Looked at 6 iterations (v1-v5, m_pair): plane-hit billows alone
  read as murky blotches; art-directed bands at the H3 elevations e_k (-5.7 / -15.6 / -31, where a ray crosses each sheet 2.5 km
  out) with billow texture on the sheet plane read as layered strata receding along the rift (cloud sea seen from above); the
  nadir needs the deep glow of the lit city (plane y -2 000) under translucent veils.
- Model (NocAtmosphere.hlsl NocAbyss, twin NocFogMath.Abyss; constants "#define NA_*" = NocAbyss in NocSchedule.cs):
  base gradient below the horizon lerp(AbyssNear, AbyssGlow, 1 - (1 - |e|/90)^1.3) (monotone, flat at -90; v1 smoothstep);
  deep glow x (1 + 0.35 x 2 (n - 0.5)) on the plane y -2 000 (octaves 675 / 225 m) where e < -10 (smoothstep 10..70 deg);
  three sheets composited deep to near, a_k = coverage(n_k) x (0.75 band(e - e_k) + (1 - exp(-0.5 / sin|e|)) V_k ramp), band =
  exp(-(x/1.6)^2) above e_k / exp(-(x/5.5)^2) below, V_k = exp(-(sigma Down(y_k) t_k)^2), n_k = 4 octaves 1200 / 400 / 133.3 /
  44.4 m on the sheet plane (weights 0.30 / 0.30 / 0.25 / 0.15, faded below 6..12 px, rotations +17 / -53 / +38 / -71 / +64 / -29,
  lattice seeds per sheet and octave), coverage = smoothstep(0.5 - h, 0.5 + h, n), h = max(0.10, 10 g), g = blurM sum(w fade / L)
  (the threshold filtered by the noise change per pixel), colours (0.046, 0.039, 0.032) / (0.060, 0.048, 0.037) / (0.090, 0.066,
  0.044) (luma about 56 / 62 / 76: the deepest layer glows), shade 1 + 0.10 (2 nFine - 1) - 0.30 thick. Sheet billows scroll 1.2
  m/s, the deep glow 0.5 m/s (_NocAbyss, wrapped to 76 800 / 43 200 m: whole lattices of every octave). Geometry fog sheets s_w 0.5
  -> 1.5 (NocAbyss.SheetFogStrength via _NocFog.z). _NocSheetE.w is now the sheet scale (1 / 0 when cut), no longer the 0.15 amp.
- Deck (INT-11): f = x / sqrt(1 + x^2), x = 4.5 (n - 0.5) (odd, never flat: no H1 plateau); base x (1 + 0.5 f), tint x (1 + 0.5 f)
  (v1 1 + 1.2 (n - 0.5) / 1 + 1.0 (n - 0.5)); NocCloudMix weight smoothstep(380, 560, y + 70 f): towers dissolve into bellies.
- Numbers from the numpy twin (1080p, sky only, EV 0): KS10 drop e -5..-30 mean 51.0 (v1 capture 40-47); band rises over the gap
  above each band 7.9 / 6.4 / 4.2 codes (sheets -250 / -700 / -1 500); KS13 nadir centre mean 64.8, p5 60.0, p95 68.6 (8.5 codes of
  structure), hue 25.7, sat 0.36; H3 KS09 adjacent-row max 0.685 code at 1080p, 0.553 at 4K (limit < 1.0; without the filtered
  threshold 2.8 / 1.8, with only a wider threshold 1.48). Iterations that failed it: fade 4-8 px + coverage 0.45-0.56 (2.82),
  fade 8-16 + 0.40-0.60 (1.48), adaptive h with fade 8-16 (0.98, too close).
- Display-luma table (AtmosphereTests 3.2, noise at its mean) abyss columns e -6 / -30 / -60 / -90: beat 3 48.5 / 59.2 / 60.8 / 64.6
  (v1 40.9 / 50.1 / 60.1 / 65.9); TFog sheets-on path values at s_w 1.5 (independent erf evaluation in Python): y -250 0.975, -700
  0.615, -1 000 0.216, -1 500 0.013, -2 000 0.0005, B face strata 0.327 (v1 0.981 / 0.678 / 0.279 / 0.024 / 0.0014 / 0.420).
- Offline compile: fxc ps_5_0 of a scratch shader including NocAtmosphere.hlsl (sky + fog): 0 errors, 0 warnings, 2 719 slots
  (HEAD 1 919; [loop]s with static const tables; an unrolled first version was 4 894).
GLOBAL 09:40: sky below the horizon (abyss) and every fogged surface below the horizon (fog colour = NocSkyColor): brighter,
  layered, textured abyss (beats 2-7 wherever rays go down: KS09-KS13, beat-3 / 4 stills, the boulevard's portal views); geometry
  fog sheets x 3 (faces below y -250 / -700 lose contrast); cloud deck billows about x 2 contrast in every look-up (KS02, KS04, KS05,
  KS15, KS17, KS18) and tower tops dissolve 70 m higher or lower with the billows (every tower top near y 380-560).

## Restart (second start of wave A, 2026-11-16 10:05)
- Resumed from the tree (nothing committed by the first start). git diff of my files: NocAtmosphere.hlsl, NocSchedule.cs,
  AtmosphereDriver.cs, AtmosphereTests.cs, lookv2_check.py modified; AtmosphereAbyssTests.cs (+ .meta) new. The lead's 10:02
  CompileCheck (0 errors) covered them. fastcheck all OK (runtime 196 / editor 260). Offline fxc ps_5_0 of the scratch shader
  including NocAtmosphere.hlsl: 0 errors, 2 717 slots. Committing now (minimum gate), then Unity tests and the lookdev build.
- 10:18 AT-t01 (AtmosphereTests|AtmosphereAbyssTests|GradeTests|MaterialTests): 64 / 66. Fails: Abyss_ScrollWrapsExactly (the
  scroll was added before the octave rotation, so a wrap of 76 800 m moved each rotated lattice by a non-integer number of
  periods) and MaterialTests.LookShaders_V2ColourRules (SG moved NocScreen's ramp into Beats/NocScreenWave.hlsl as NocScrMix).
  Fix: the scroll is added in each octave's rotated frame (HLSL NocSheetNoise(p, s, ...) / deep glow, twin SheetNoise(x, z, sx,
  sz, ...)): exact wraps, each octave drifts along its own direction (the billows evolve); the colour-rule test reads NocScreen +
  NocScreenWave and accepts NocScrMix. KS10 / KS13 numbers from the C# twin: drop 50.97, sheet rises +7.87 / +6.43 / +4.20; nadir
  mean 64.8, p5 60.0, p95 68.5 (8.5 codes), hue 25.7, sat 0.357; deck std / mean 0.217 base, 0.212 lid (v1 0.13 / 0.11).
- 10:40 AT-t02: 66 / 66 green. fxc of the scratch shader still 0 errors.

## Third / fourth starts (2026-11-19 / 11-20 morning; not logged then, reconstructed from 2fe0da9)
- AtmosphereDriver: QA override -sheet-fog X (geometry strength of the haze sheets, _NocFog.z; read once in Init) so one build
  can A/B the layering of the abyss; FacadeShares lost its v1 branch (TP-2). No build or capture of AT's work was made.
DONE AT FX REQUEST AT: AtmosphereDriver.cs:142/199 (FacadeShares' dead v1 branch removed; in 2fe0da9).

## Fifth start (2026-11-20 20:24)
- Read the log, my commits (a821775, ea386fd, 2fe0da9), STATE, LOOK_V2 3 / 7 / 8, critique-lookv2-01 INT-01 / -11 / -15.
  No REQUEST AT open in any copy's logs (only FX's, done). Live note: Logs/AT_progress.md.
- 20:28 AT-b01: first build of AT's committed abyss / deck into Builds\AT (baseline: never captured before).
- 20:55 AT_r01 (AT-b01, keyshots1080, 3 min). Looked at KS05 / KS09 / KS10 / KS13 / KS18 full size and x6-contrast crops
  (scratchpad at/ks10_x6.png, ks09_x6.png, ks05_x6.png). Numbers (stats.py, 8-bit Rec.709 luma):
  - KS10 drop x 800-960 rows 0-700: row medians 42-55 (mean 50.7); KS09 x 420-640: 39-54; KS12 abyss rect 41-50; KS13 nadir
    through the glass 59-62, std 1.4-2.0. The billows and bands exist (visible at x6) but are +-2-3 codes at this exposure: the
    sheets were brighter veils (56 / 62 / 76) over a base that brightens with depth, so every hole showed an equally bright
    deeper layer and the shading (-0.30 thick) cancelled the coverage: INT-01 not met.
  - KS05 lid x 820-1100 rows 0-440: 69.5-73.0, std 5.0 (mostly the gradient), billows +-2-3 codes; KS04 sky rect 64.8 -> 55.7
    smooth; KS18 deck 38-54 (std 4.3): billows read there (cool deck); tower tops dissolve into the deck (KS18).
  - lookv2 r01: see below (after r02).
- 21:00-21:25 numpy model of the abyss (scratchpad at/abyss_np.py: same structure, own lattice hash; sky-only KS09 / KS10 / KS12 /
  KS13 at 960x540): variants v0 (HEAD) .. vG. Chosen (vG): DARK VEILS over GLOWING DEPTHS: c x (1 + 1.2 L(e)), L = smoothstep(3,
  30, -e) (1 - 0.8 smoothstep(30, 80, -e)); sheets c = lerp(c, c x veil_k x shade, a_k), veils 0.45 / 0.55 / 0.65 (near ones a
  little cooler), BandA 0.75 -> 0.85, detail 0.10 -> 0.20, thick 0.30 -> 0.35, DeepAmp 0.35 -> 0.50. Model: KS10 drop -5..-30
  51.8 -> 53.0, p5..p95 46-58 -> 36-71; KS13 centre 64.8 (p5 62.0, p95 69.2); KS09 column max adjacent step 0.66 (1080p) / 0.50
  (2160p) (< 1.0); the KS10 bands: sheet 0 a dark line -10.9 codes under the gap above, sheet 1 +5.6, sheet 2 2.6.
- 21:26 NocAtmosphere.hlsl (one write) + twin NocSchedule.cs (NocAbyss: Veil*, DeepLift, Lift*, OccBeam / OccEdge / HazeOccY;
  SheetCol* removed; NocFogMath.Abyss / AbyssLift / Occluder / CloudColor; NocAtmoState D-034 occluder fields): the veil model;
  deck tint amplitude 0.5 -> 0.8 (INT-11: billows under the glows); D-034 hooks _NocDeckOcc (hull in the deck: W x (1 - occ),
  base x (1 - 0.5 occ)) and _NocHazeOcc (shadow on the plane y -650: the depths under the sheets x (1 - occ)), identity at 0.
  Tests: AtmosphereAbyssTests (sheet contrast against the drop above or below, either sign; hot-lid spread 0.25-0.45; new
  Dread_OccludersAreIdentityAtZeroAndOnlyDarken), AtmosphereTests 3.2 table logs every value (the abyss columns change).
GLOBAL 21:26: NocAtmosphere.hlsl (every shader recompiles): the abyss below the horizon (sky and every fogged surface below the
  horizon, beats 2-7 where rays go down: KS09-KS13, beat-3 / 4 stills, portal views) has dark billowed veils over a brighter
  deep (drop mean about the same, +-8-15 codes of structure); the cloud deck under the city glows has +-34 % billows (KS02,
  KS04, KS05, KS15, KS17, KS18 lids). AB: a build started before 21:26 may hold a mix of old and new shaders.
- NOTE for AB: AB-b02 (21:17-21:31, exit 1) failed with "script class layout is incompatible between the editor and the player"
  (NocAtmoState has an extra field DeckOccX...): my NocSchedule.cs edit at about 21:27 landed while that build was compiling
  the player scripts. It is not a code error (fastcheck all OK at 21:31; the editor compile of the next build picks it up): a
  rebuild passes. My fault; I edit C# only between builds from now on.
- 21:45-21:50 AT-b02 (exit 0, 0 shader errors, 59.6 s: shader cache warm from DR-b01) + AT_r02 keyshots1080. Looked at KS10 / KS09 /
  KS05 (x1, x2.5 crops, r01 | r02 side by side: scratchpad at/KS10_r02_x25.png, ks05_r01_r02.png):
  - KS10 abyss x 820-940 rows 330-480: mean 51.7 -> 64.4, p5..p95 48.8-55.1 -> 53.4-72.5, std 3.9 -> 8.3: billowed veils over a
    glowing deep now read at x1; the band under the horizon is a dark veil (row 145: 36.8). KS09 profile 44 -> 36.5 (e ~ -9) ->
    52 -> 68 (bottom): the near band is too dark (LOOK_V2 3.2 -6..-15: 41-44); KS12 rect 45.1 -> 42.2 (38.7-48.1).
  - KS13 [560,880,760,1060] through the glass 59.9 -> 60.6 (flat, std 1.9); [560,470,740,640] 31 = DR's shadow body under the
    strip (DR log 21:20), not the sky.
  - KS05 lid 71.7 -> 74.7, p5..p95 67.2-76.6 -> 68.7-81.5 (soft billows visible at x1); KS04 sky rect 61.2 -> 63.7 (56.6-69.1);
    KS02 slit 54.0 -> 50.4 (sky_top 53.5 -> 50.3, window 50.4-70.4: at its floor).
  - lookv2_check r02 9 / 20 (r01 10): R1a / R1b / R2 unchanged in 20 / 20, R3 within +-0.03 (no failure; scratchpad
    at/cmp_lookv2.py). New failure KS09 p99 153.4 -> 113.1 (share >= 120: 1.24 -> 0.93 %: the bloom of AB's handrail over a darker
    band; AB-01 says the rail bar was most of KS09's >= 120 pixels).
- 21:52 AT-t03 (Atmosphere|AtmosphereAbyss|Grade|Material): 64 / 67. Exact C# twin numbers: KS10 drop 52.95, sheet contrasts
  9.19 / 14.97 / 2.71 (sheet 2 < 3); KS13 nadir mean 72.5 (> 68), p5 64.3, p95 78.6; 3.2 table beat 2: e -6 38.2, -30 66.8, -60
  70.8, -90 69.2; deck spread base 0.218, lid 0.339; horizon dE 0.098.
- 21:59-22:00 encode gate (AT-b02 build), enc_beat03_abyss (KS10 view, 600 frames 60 fps, 42 s), x264 CRF 23, region
  691,43-998,238: visible steps 0.00 / 1000 px (max 0.00 code) (lookv2_01: 5.13): the 14.7 H1 (6) step gate PASSES. The check
  is still FAIL on the 11.3 BandScore (27.2, source 5.1): the rect now holds billows, traffic lights and the face edge (texture,
  as INT found for KS17 / KS00). x8 crop: QA/captures/AT_e02_keep/enc_beat03_abyss_x8.png (blocks, no contours).
- Disk: W: 13 GB free at 22:00 (others' builds and captures; mine: 0.6 GB).
- 22:00-22:07 encode gate on AT-b02: enc_beat02_axis (KS04 sky, 806,22-1056,238): 13.89 visible steps / 1000 px = 3 steps of
  1.37 code over the 216 px profile (source 0): FAIL; enc_beat01_slit (KS02, 912,0-1027,227): 0.00 steps (step gate PASS;
  BandScore 3.16 FAIL as before, PA-09 / INT).
- 22:03 batch 2 (one write per file, after DR-b03 exited): veils 0.66 / 0.55 / 0.55 (the near one lighter: it lies along the
  horizon band), band opacities 0.75 / 0.85 / 0.95, lift 1.2 -> 0.7 with no lift at the nadir (LiftNadir 1.0), deck tint amp
  0.8 -> 0.95. NOTE for AB: AB-b05 started at 22:03:18, 15 s before this edit (constants only, no layout change): it may hold a
  mix of r02 / r03 atmosphere values.
- 22:15-22:21 AT-b03 (exit 0, 0 shader errors). AT-t04 (22:26): 66 / 67: KS10 drop 50.62, sheet contrasts 6.51 / 10.74 / 3.78
  (all >= 3), KS13 nadir mean 65.5 (p5 57.8, p95 71.0: 13 codes of structure, hue 25.7, sat 0.367), lid spread 0.402; the one
  failure is the 3.2 table (abyss values changed by design: beat 2 e -6 / -30 / -60 / -90 = 40.2 / 60.0 / 64.8 / 63.8).
- 22:22 AT_r03 (keyshots1080). KS10 x 820-940 rows 330-480: mean 58.6, p5..p95 49.9-65.0, std 7.5 (r01 3.9); KS10 column
  profile 43.7 (horizon) -> 38.9 (dark near veil) -> 65 (lit depths); KS09 38.8 -> 61; KS12 rect 42.3 (39.4-47.1); KS13 through
  the glass 57.2 / 61.0; KS05 lid 76.1 (69.0-83.9); KS04 64.9 (57.1-70.7); KS02 slit 48.4. lookv2_check r03: 14 / 20 (r01 10);
  R1a / R1b / R2 / R3 no new failure in 20 / 20 (KS18 R2 0 -> 0.05 %, limit 5 %); new failure KS02 sky_top 48.5 < 50.4: the slit
  lies on a dark billow at its pose (f ~ -0.5), so a stronger tint amplitude darkens it (r01 53.5 at 0.5, r02 50.3 at 0.8).
  Side by side: scratchpad at/ks10_r01_r03.png (darker near band, glowing deep, soft billows at x1).
- H1 ladder on KS04 (same clip, CLI on AT-b02 and a dome-grain build AT-b04): dither peak 2.5 -> 13.89 (max 1.36); -dither
  temporal -> 13.89 (max 1.41); a static direction-locked grain on the dome (NocSky, value noise 0.0036 rad cells, +-7 %
  linear) -> 13.89 (max 1.28). The count never moves (exactly 3 steps, one per strip of the 3-strip profile, different frames):
  not removable by dither or grain at these amplitudes. Reverted the grain (NocSky.shader back to HEAD: LOOK_V2 7 says no grain,
  and it bought nothing). INT-15 KS04 stays open (next: look at which rows the 3 steps sit on; wider gradients of the lid).
- 22:32 final tuning (one write per file): deck tint amp 0.95 -> 0.7 (KS02 slit back toward 51-52; lid spread about 0.30).
  AtmosphereTests 3.2 logs every row before it asserts. 22:33 AT-b05 (final build) + AT-t05 queued.
- 22:35-22:57 AB-final (AB's full EditMode suite, the same tree as my final code: started after my 22:32 write): 429 / 435,
  5 failed: AtmosphereTests.LookV2_DisplayLumaTable_3_2 (mine: the abyss columns changed by design, fixed below);
  Beat07HeartTests.PlazaInlay_SameTopFan_RingsSeamsFade (HT: 962 <= 1000), BoulevardStreetTests.LightBudget_AtMost28 (BV: 29 >
  28) and Shops_Varied (BV: 18 > 17), WetFloorTests.WallWindows_MeanOne (FX: -1.4e-11 >= 0, a float rounding): not AT files.
  3.2 rows from its [NOC] lines (exact twin): s 300: ... 40.2 / 60.0 / 64.8 / 63.8; s 480: 39.0 / 58.1 / 62.8 / 61.8; s 980:
  36.5 / 55.3 / 59.5 / 58.5; s 1400: 39.0 / 57.2 / 62.2 / 61.3 (e -6 / -30 / -60 / -90): written into the test's "want" (22:59).
- 23:00 feature freeze. AT-b05 (final build: deck tint 0.7, grain reverted) started 22:57:46 behind AB-final; AT-t05 follows.
- 22:57-23:06 AT-b05 FINAL build (Builds\AT, exit 0, 0 errors, 0 shader errors, 332 s: full shader recompile after the 22:32
  NocAtmosphere write). AT-t05 (my test groups with the new 3.2 values) could not run: DR-final (DR's full suite, 23:07) holds
  this copy's Unity queue past 23:30. The 3.2 abyss values are the exact twin's own printed numbers (AB-final [NOC] lines), so
  those columns pass by construction; the hot-core column at deck tint 0.7 is NOT re-verified: an exact Python port of
  NocFogMath.CloudNoise gives f = -0.337 at (115, 195), t 0 (tint factor 0.831 at 0.5 -> 0.764 at 0.7: about -2 codes). If
  it leaves 68.0 +- 3, evaluate the hot / cool cores at the noise mean (the table's definition) rather than at that point.
- 23:07 AT_r04 (final, keyshots1080; looked at KS10 full size): lookv2_check 14 / 20 (r01 10 / 20), R1a / R1b / R2 / R3 no new
  failure in 20 / 20; fails left: texture KS00 / KS01 / KS02 / KS19 (walls, not AT), KS14 p99 76.8 (garden gate light content),
  KS15 mean 17.2 (AT-03, open). KS02 sky_top passes again (slit rect 51.6). Numbers: KS10 drop rect mean 58.6, p5..p95 49.8-64.9,
  std 7.48 (r01 51.7 / 48.8-55.1 / 3.89); KS09 rect 49.8 (37.8-61.6); KS05 lid 73.7 (68.2-80.0, r01 71.7 / 67.2-76.6); KS13
  through the glass 57.3; KS12 rect 42.3 (AT-01 open).
- 23:08-23:10 smoke AT-smoke (AT-b05, -autotest -brisk -autotest-until 880 -beat 3): AUTOTEST PASS, beats 3 / 4 / 5 entered at
  s 400.00 / 560.00 / 880.00 (t 0.02 / 64.17 / 192.17), 0 errors (QA/reports/autotest_brisk_from400_until880.json).
- Intermediate captures deleted (AT_r02, AT_r03, every encode clip and work folder). Kept: QA/captures/AT_r01 (baseline),
  AT_r04 (final), AT_e02_keep (encode crops and frames). fastcheck all OK (23:00 tree: runtime 207 / editor 274).
- Files changed this start: Shaders/Include/NocAtmosphere.hlsl, Scripts/Runtime/Core/NocSchedule.cs, Tests/EditMode/
  AtmosphereTests.cs, Tests/EditMode/AtmosphereAbyssTests.cs, docs/logs/AT.md, QA/critique-AT.md (new), Logs/AT_progress.md
  (new); NocSky.shader was changed (dome grain) and restored to HEAD; QA/reports lookv2_AT_r01..r04.json, encode_test_AT_*.
AT DONE 23:12

## FIX WAVE (panel p4_01, 2026-11-21 01:57; task Logs/fix/AT.md with the lead's rulings)
- Read the task file and rulings (C11 S2 abyss depth + sink, C02-air S2 koi in-scatter, C26 S2 sheet lightning, C44 S3 KS02 slit,
  C46 S3 alley wall encode (waits on QA's 4K encode), the KS04 sky encode steps). Baseline: the lead's p4_01 captures of master
  caddb90 (INTEG QA/captures/p4_01; the same tree as this copy): looked at KS09 / KS10 / KS11 / KS17 full size (the drop is a
  smooth umber gradient; face B's windows below the walk are crisp in KS11; no air glow beside the koi in KS17).
- Open requests: AU's REQUEST AT (sheet lightning at DreadScore.LightningS) is this wave's C26; FX's old request done (2fe0da9).
- 02:13 one write per file (scratch-edited, then copied together; fastcheck all OK, fxc ps_5_0 of a scratch shader including
  the new NocAtmosphere.hlsl: 0 errors, about 3 760 slots):
  - NocAtmosphere.hlsl: C11 under-city glow pools in NocAbyss (plane y -2 100, value noise 675 / 225 m, pool = smoothstep(0.55,
    0.80, n), amber / neutral by a third lattice, c x (1 + A f0 (pool tint - 0.20)), weight smoothstep(10, 20, -e)); C11 sink
    (extra extinction sigma K_s S01((-60 - y) / 340) in NocOpticalDepthS's sheet branch, exact path mean NocSinkMean); C02-air
    NocKoiAir (closed-form Gaussian in-scatter of the 12 spine points, R 70 m) added after the fog in NocFog; C26 flash and C44
    alley lift in NocCloudColorN. New globals _NocAir (pool A, sink K_s, koi gain x KoiK, slit lift), _NocFlash, _NocKoiAir[12].
  - NocSky.shader: the dome adds NocKoiAir to the deck plane (beat 7 only, uniform branch).
  - NocSchedule.cs: NocAbyss constants (twins of every new NA_ define), NocAtmoState fields, twins (Density / OpticalDepth sink,
    SinkMean, CloudColor lift + flash, FlashEnvelope, SlitShape, KoiAir, Abyss pools, Fog koi air), StateAt defaults.
  - AtmosphereDriver.cs: uploads _NocAir / _NocFlash / _NocKoiAir; UpdateFlash (DreadScore.LightningS, DreadClock launch rule,
    stills at the cinematic pace), FlashCentre (550 m ahead, or DR's hull within 900 m, live _NocDeckOcc); QA overrides
    -abyss-pool, -abyss-sink, -koi-air, -slit-lift, -flash (read once in Init).
  - New test file Tests/EditMode/AtmosphereFixTests.cs.
GLOBAL 02:13: NocAtmosphere.hlsl (every shader recompiles): (1) the abyss sky below e -10 (and every fogged surface whose ray goes
  down there: KS09-KS13, beat-3 / 4 stills, portal views) gains soft amber / neutral glow pools; (2) every surface below y -60 is
  fogged harder (sink): face strata, AB's lower traffic layers and DR's shadow body below the walk fade sooner (beats 2-4 wherever
  rays go down); (3) beat 7 only: the air near the koi glows KoiGlow on every fogged surface and on the dome; (4) the deck over the
  alley (x 0, z 80, sigma 70 m) is 25 % brighter (KS02's slit; nothing else sees it); (5) six slow deck glows at
  DreadScore.LightningS (s 205 / 345 / 553 / 790 / 1 140 / 1 420, about 2 s each): no key shot's s falls inside one.
- 02:14-02:19 AT-b01 (exit 0, 0 errors, 0 shader errors, 277 s). 02:31 AT_r01f (keyshots1080 KS02/04/05/09-13/17/18 + flash poses
  s 204/206, 552/554, 791), looked at KS10 / KS13 / KS17 / KS18 / pose_fl205b / pose_fl553b and the x8 flash diffs (scratchpad
  at/pose_fl205a_diff.png, pose_fl553a_diff.png). Numbers (scratchpad at/stats.py, koiprof.py; p4_01 -> r01f):
  - C11 pools (675 / 225 m, smoothstep 0.55-0.80): KS10 drop x 800-940 lateral row std 0.9-4.0 (was 0.3-2.4, rows 300 / 600 are
    content); visible as soft blotches right of the column (x 1100-1500), hard-edged: too large and too sparse for the column.
  - C11 density sink (K_s 2.5): no visible change on the near strata (KS11 face B below p95 90.0 -> 90.0), while the exact twin
    (scratchpad at/tfog.py) takes AB's y -520 river 800 m out from T 0.54 to 0.15 and y -880 from 0.30 to 0.03: a bad trade.
  - C02-air (gain 0.03, R 70): KS17 sky 50-80 px above the body +14..+18 codes over the far sky at the head and mid-body, +26..+29
    over the back (col 1180); far sky +1.4; KS18 inside the arc 53.1 -> 68.6 (far 45.7 unchanged); clipped 0.000 %. But
    lookv2_check: KS17 / KS18 koi_patch hue 14.9 -> 8.0, sat 0.74 -> 0.51 (the body's own fragments took the glow) and KS18 dark
    cast R - G -2.83 -> -4.36, sky_top sat 0.20 -> 0.30 (teal cast): FAIL.
  - C44: KS02 slit rect 51.7 -> 55.9 (lookv2 sky_top 51.4 -> 55.4); KS04 lid 62.9 -> 62.6, KS05 73.7 -> 73.6.
  - C26: s 206 vs 204: frame mean +0.99 %, the canyon's deck slot +1-4 codes (soft, billow-shaped); s 554 vs 552: the hull covers
    the overhead (DR's re-park: abeam at s 553), the deck strip beside it +1-3 codes; frame mean +1.08 %.
- 02:36 batch 2 (one write per file): pools 337.5 / 112.5 m, smoothstep(0.45, 0.85), mean 0.24, weight smoothstep(12, 22, -e);
  the density sink replaced by a depth veil in NocFog only (T' = T (1 - 0.5 S01((-40 - y) / 360)); NocStreak's own NocFogT and
  the T-FOG path values unchanged); the C#-only values moved to a new class NocAir (PoolAmp 1.0, SinkV 0.5, KoiAirGain 0.03,
  SlitLift 0.25, Flash*) with no NA_ define, so they tune without a shader recompile; flash in capture mode = DreadDriver's rule
  (cinematic pace in stills and sequences). Tests updated (Sink_VeilOnlyBelowTheWalk_MonotoneAndHalfAtDepth).
- 02:41 batch 3 (NocAtmosphere.hlsl, NocSchedule.cs, AtmosphereFixTests.cs): the koi air skips fragments within 60..110 m of a
  spine point (the body keeps HT's colour) and its colour is KoiGlow half-desaturated (lerp(0.607, KoiGlow, 0.55)).
  NOTE for DR: DR-b08 (started 02:37:39) compiles shaders while batch 2 / 3 landed (02:36:46 / 02:41:13): it may hold a mix of
  the two koi-air versions (beat 7 only) - nothing in beats 3-4 differs between them.
GLOBAL 02:41: (replaces the 02:13 sink line) surfaces below y -40 drawn through NocFog veil toward the abyss sky by up to 50 % (half
  at y -400 and deeper): face strata, terraces, DR's shadow body; AB's NocStreak rivers keep their own transmittance. Beat 7: the
  koi air is a whiter cyan and leaves the koi body untouched.
REQUEST FX: Assets/_Game/Shaders/Include/NocFacadeSkin.hlsl:646 and :737 (C11, KS11 acceptance "face B below the deck p95 <= above
  p95 + 10"): the bright p95 below the walk is face B's lit windows at y 0..-220, about 450 m out, where no air term can veil them
  without fogging the walk itself (AT's sink veil is 0 above y -40 and about 0.2 at y -200). Thin the lit share with depth below the
  walk: multiply the lit probability p (line 646) and aT (line 737) by
  `(1.0 - 0.6 * NocS01((-40.0 - yWS) / 400.0))` (NocS01 = saturate smoothstep of NocAtmosphere.hlsl; yWS = the world height of the
  window row / fragment), so the strata below the rails go dark with depth and AT's veil lifts the dark wall into the umber haze.
REQUEST FX: the rain pass (C02-air "the rain streaks near the koi take KoiGlow"): in beat 7 (_NocAir.z > 0) add to a streak's
  radiance its koi light, e.g. `streak += NocKoiAir(rd, dist) * 4.0;` (NocKoiAir(rd, len) of NocAtmosphere.hlsl, rd the unit view
  ray, dist the drop distance) or NocKoiLight(posWS, -rd) of NocLighting.hlsl; AT did not touch the rain files.
- 02:49-02:59 AT-b02 (batch 2 + 3; exit 0, 0 errors, 520 s: full shader recompile). 03:01 AT_r02f (same set), looked at KS10 /
  KS09 full size and a p4_01 | r02f side-by-side crop of the KS10 drop (scratchpad at/ks10_p401_r02f.png): the drop now reads as a
  cloud sea lit from below (soft amber / neutral lobes between the traffic rivers, under the veils), the rivers unchanged.
  - KS10 drop x 800-940 lateral row std (rows 350 / 400 / 450 / 550 / 650): 5.1 / 2.8 / 2.1 / 6.4 / 2.2 (p4_01 0.9 / 1.4 / 1.0 /
    0.9 / 2.6); abyss rect 820-940 x 330-480 mean 58.5, p5..p95 46.6-73.9 (p4_01 58.0, 49.3-64.2). KS09 rows 600 / 650: 7.7 / 2.2.
    Exact twin (AT-t01 [NOC]): KS10 drop e <= -20 median row std 4.77 codes with pools, 1.19 without.
  - KS11 face B below p95 88.8 (p4_01 90.0), above 26.2: the veil starts at y -40, the windows there are at y 0..-220 (see
    REQUEST FX above). Twin: a face at y -400, 500 m out, T 0.869 -> 0.435.
  - KS17 sky 50-80 px above the body +14..+18 codes (head, mid), +28 (back) over the far sky; KS18 arc 69.1 vs 45.7; clipped 0 %;
    lookv2: KS18 dark cast -3.6 (passes), sky_top sat passes; koi_patch hue 9.9 / 9.0, sat 0.55 / 0.52 still FAIL (the koi is
    additive: the glowing air BEHIND its body shows through the patches).
  - lookv2_check r02f vs p4_01 (scratchpad at/cmplv.py): KS02 sky_top 51.4 -> 55.4; KS04 / 05 / 09 / 10 / 11 / 12 unchanged
    pass; KS13 fails p99 as in p4_01 (83.9 -> 81.0, content); KS17 / KS18 the patch checks above.
- 03:00 AT-t01 (Atmosphere|AtmosphereAbyss|AtmosphereFix|Grade|Material|Abyss|Dread|Constants): 115 / 118. Fails: KS13 nadir twin
  mean 69.4 (> 68: the pools stacked on the deep glow), the flash envelope test's float loop (t 0.4 + 1e-17 counted as decay),
  the slit lift at the hot core 1.7 % (my comment said < 0.2 %: wrong; the 3.2 table still passes, +0.4 code).
- 03:05 batch 4 (NocAtmosphere.hlsl, NocSchedule.cs, AtmosphereFixTests.cs): koi air weighted smoothstep(10, 50, h) (rays through
  the body's core see the hologram, not the glowing air behind it), gain 0.03 -> 0.025 (C#); pools fade by 0.8 over -e 45..80
  (the nadir keeps the deep glow); test fixes. 03:05 AT-b13 and AT-final (full EditMode suite) queued behind AB-final.
- Encode (on AT-b02, x264 CRF 23, 1080p, encode_test.py): enc_beat03_abyss (KS10, 691,43-998,238) visible steps 0.00 / 1 000
  (PASS, as before), BandScore 27.2 -> 12.9 (QA/reports/encode_test_AT_e13_enc_beat03_abyss.md). enc_beat02_axis (KS04,
  806,22-1056,238) 13.89 / 1 000 as in the fifth start: located (frames 513 / 257 kept in QA/captures/AT_e13_keep): all steps are
  in Cb (channel 4; one in Cr), 0 in Y and RGB: rows about 82-90, 129-150 and 184-190 of the frame, steps 1.1-1.4 Cb codes on a
  Cb slope of +0.021 code / px (4.5 codes over the 216 px profile: the warm lid fading into the cooler haze toward the horizon).
  x264's 4:2:0 chroma quantiser at CRF 23 turns that slow chroma ramp into steps; NocGrade's chroma dither (+-1 code per 2 x 2,
  D-007) does not survive it. A luma-gradient widening cannot remove them; removing them needs the lid's chroma slope cut about
  3.5x (a warmer haze under the beat-2 glows, a global look change) - not done this wave (decision proposed).
- 03:16 AtmosphereDriver.FlashCentre: DR's HullCourse.CentreZ now takes tau (D-040 course API): the static check uses
  HullCourse.CentreZAtS(s) / SternZAtS(s) (DR's REQUEST AT in docs/logs/DR.md: the s 553 glow on the hull at its new place; in play
  and capture the driver takes the live _NocDeckOcc (515.3, CentreZ(tau)) that DreadDriver writes). DONE AT DR's request (s 553
  glow centred on the hull: AT-final [NOC] "flash s 553: centre (515, 136), 159 m from the walker, hull True").
- 03:25-03:28 AT-b13 (batch 4; exit 0, 0 errors, 0 shader errors). 03:29 AT_r03f (all 20 key shots at 1080 + flash poses s
  204/206, 552/554 (yaw 60 pitch 55, under the hull), 789/791); looked at KS13, KS18, pose_fl553b and the x8 diff
  (scratchpad at/r03_pose_fl553a_diff.png: the deck beside the hull +1-3 codes, the hull dark: the outline).
  - Flash pairs (frame mean, linear-display luma): s 206 vs 204 +0.99 %, s 554 vs 552 +0.82 %, s 791 vs 789 +0.70 %; deck p90
    +2-3 codes; D-010: the frame mean moves < 1 % over the 0.4 s rise (< 0.04 level / frame at 60 fps; the sequences gate is
    1.2 levels / frame), one rise and one fall per glow, glows >= 62.5 m (25 s) apart.
  - KS17 sky 50-80 px above the body (gain 0.025): col 640 +13..+15, col 1020 +12..+15, col 1180 +25..+27 over the far sky (p4_01
    +5, -2, +3); KS18 arc 66.7 vs 45.7 (p4_01 53.1); clipped 0.000 %. lookv2: KS17 passes (patch hue 11.2, sat 0.60), KS18 patch
    hue 9.9 / sat 0.56 (window 10-22 / 0.60) still fails -> 03:31 batch 5 (NocAtmosphere.hlsl + NocSchedule.cs): the hollow
    weight smoothstep(10, 50, h) -> smoothstep(30, 70, h). AT-b14 queued (behind DR-b10), AT-t14 (my groups) queued.
  - lookv2_check r03f: 13 / 20 (p4_01 14 / 20: the one change is KS18's patch, fixed in batch 5 pending its capture); R1a / R1b /
    R2 / R3 within +-0.1 % / +-0.07 in 20 / 20 (QA/reports/lookv2_AT_r03f.md; scratchpad at/cmplv.py). Other fails as in p4_01:
    KS00 / KS01 mean luma and texture (alley content), KS02 texture, KS13 / KS14 p99 (content), KS15 dark cast -4.77 (garden).
- 03:28-03:40 AT-final (the full EditMode suite on batch 4 + the FlashCentre fix): 476 / 477 passed, 0 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch, Explicit) (QA/reports/AT-final.xml). [NOC]: KS13 nadir twin mean 66.4 (p5 56.7, p95 73.2,
  hue 25.7, sat 0.379); KS10 drop row std 4.32 codes with pools / 1.19 without; pool multiplier mean r 1.033 g 1.010 b 0.980;
  a face at y -400, 500 m out: T 0.869 -> 0.435; flash over the hull +1.12 %, 300 m beside it +9.74 %.
- Smokes: AT-smoke (AT-b13, -beat 3 until 1500) stopped logging at s 560 (t 64, 03:32) and was killed (exit 255) at its timeout;
  not reproduced: AT-smoke2 (same build copy, -beat 4 until 880) AUTOTEST PASS, beat 5 at t 128.17, 0 errors, 32.7 B/frame (the
  engine input box); AT-smoke3 (-beat 6 until 1500): goal trigger s 1497 at t 166.900, stop s 1500 at t 169.300 = exactly the
  expected 166.90 / 169.30, reported FAIL only on "untilTime (169.30 vs 166.90 +- 2.0)" (the harness compares the -autotest-until
  1500 stop with the trigger time: a QA tool quirk when until = the path end; 0 errors, 27 B/frame); AT-smoke4 (AT-b14, -beat 3
  until 880) AUTOTEST PASS, beats 3 / 4 / 5 at t 0.02 / 64.17 / 192.17, 0 errors, 30.7 B/frame (QA/reports/
  autotest_brisk_from400_until880.json).
- 03:56-03:58 AT-b14 (batch 5: koi hollow 30..70 m; exit 0, 0 errors). AT-t14 (my groups): 73 / 73. 04:01 AT_r04f (keyshots set:
  KS02 / KS10 / KS17 / KS18 at 1080, 4K, 16:10, 21:9; the one 4K job): clipped 0.000 % in all 8 koi frames; KS17 passes lookv2 at
  every aspect, KS18 patch sat 0.59 (hue now passes); KS02 slit 55.9 (1080 / 16:10 / 4K), 52.5 at 21:9: the 21:9 rect (6 % of a
  wider frame) takes in both slit walls (columns 29-44), the sky inside the slit reads 54-57 as at 16:9.
- 04:04 final C# tuning (NocSchedule.cs NocAir, AtmosphereFixTests.cs): KoiAirGain 0.025 -> 0.022 (KS18 patch sat), SlitLift 0.25 ->
  0.30 (21:9 margin); the NocAtmosphere.hlsl C44 comment still says 0.25 (the value lives in C#, NocAir.SlitLift; fix at the next
  shader edit). 04:07 AT-b15 (FINAL build, Builds\AT, exit 0, 0 errors, 0 shader errors, 24 s: shaders unchanged since AT-b14).
  AT-t15 (Atmosphere|AtmosphereAbyss|AtmosphereFix|Grade|Material on the final tree): 73 / 73 (QA/reports/AT-t15.xml; horizon
  agreement worst dE 0.098; KS13 nadir twin 66.4; 3.2 table unchanged).
- 04:07 AT_r05f (AT-b15, KS02 / KS04 / KS05 / KS17 / KS18 at 1080; looked at KS17 full size: a soft teal halo in the rain air
  around a clean koi): lookv2 4 / 5 (KS17 and KS18 PASS; KS02 fails texture only, alley walls as in p4_01). KS17 sky 50-80 px
  above the body: col 640 +12.6..+13.7, col 1020 +11.0..+13.5, col 1180 +22.5..+23.6 over the far sky (p4_01 +5 / -2 / +3);
  KS18 arc 65.3 vs 45.7 (+19.6; p4_01 +7.5); KS02 slit 56.7; KS04 / KS05 lids 62.9 / 73.7 (p4_01 62.9 / 73.7). Contact sheets:
  QA/captures/AT_r03f/_contact, AT_r04f/_contact, AT_r05f/_contact.
- Full suite: AT-final (03:28-03:40, batch 4 tree) 476 / 477, 0 failed, 1 skipped (Explicit); the two later changes (batch 5 HLSL
  + C# constants, the final C# constants) are covered by AT-t14 / AT-t15 (my five groups, 73 / 73) and AB-final / DR-final ran
  the other groups on the same shared tree.
- Deleted my intermediate folders (AT_r01f, AT_r02f, AT_e13 work, the old AT_r01 / AT_r04, Builds/AT_smoke). Kept: AT_r03f (20
  key shots + flash poses, AT-b13), AT_r04f (4K / aspects, AT-b14), AT_r05f (final, AT-b15), AT_e13_keep (encode frames / crops).
  W: had 7.1-9.4 GB free during this wave (below the 15 GB floor: other copies' and projects' files; mine total about 175 MB).
- fastcheck all OK on the final tree (runtime 217 / editor 288 files).
AT DONE 04:10

## ARK (D-043, the user's order of 2026-11-21 evening; Logs/ark/ARK.md; copy .worktrees\abyss with DR / AB; from 20:16)
- Read ARK.md (AT: the Ark in the haze - occlusion of the city glow below it and of the cloud deck, its lights' in-scatter,
  searchlight cones, lightning retimed to outline it), my log, the code I own; DR's new HullCourse (20:32: 5 000 x 440 m, belly
  y 330, top 590, axis x 515.3, launched at s 380 from 3.4 km +z of the walk line, moving -z at ~28 m/s, bow over z 195 at s 596,
  hidden 6.5 km past it; HullCourse.Searchlight(i, tau, t): DR's six keel emitters, tilt 25 deg, half-angle 3.5 deg, outward sweep
  +-50 deg with periods 28-40 s, DR's own fade 220..420 m from the walk line).
- Design (AT, no realtime light; every term analytic, all globals zero = identity, so beats 1-2 and NOC_DREAD_OFF are untouched):
  (1) deck occluder NocArkShape: a soft stadium (half-beam 220 narrowing to 0.35 over 700 m soft ends, penumbra 160 m) under the
  cloud base: the glow weight W x (1 - occ), the base x (1 - 0.6 occ), strength 0.85 x S01(tau / 20) x DR's Emerge; it replaces
  DR's _NocDeckOcc ellipse while _NocArk.w > 0. (2) The glow of its lights: a Gaussian tube (R 180 m) about the axis 60 m over the
  belly, closed-form line integral (NocTubeIn: I exp(-h^2/r^2) (erf + erf) / 2 sn, fogged at the closest approach), cold window
  light, 0.010 across. (3) Searchlights: DR's emitters drawn as Gaussian cones (r = 6 m + u tan 3.5 deg, I = 0.22 (r0 / r)
  exp(-u / 900 m)), ignited one by one from the bow aft at tau 36..66 s (s 470-545) over 9 s each, x a walker clearance (3 r + 150 m)
  and a bridge clearance (axis at y 60 140..300 m off the walk line over x 200-720); their lit spots on any surface they meet
  (NocFog, before the fog: SignWhite x 0.25 x min(1, (40 / r)^2) exp(-u / L) exp(-d^2 / r^2)) and their pools on the haze sheets
  (NocAbyss: x the sheet's opacity, fog visibility and fine billows). (4) The s 553 / 790 sheet lightning (AU's DreadScore times,
  unchanged) centres on the live Ark's axis at the walker's z clamped to the hull, stretched x 2.2 along it, the hull's stadium
  keeping 0.1 of it: the deck flashes round the silhouette (s 553: 0.75 km ahead behind the bow; s 790: overhead).
- 20:39 batch 1 (one copy per file from scratch; fastcheck OK, fxc ps_5_0 0 errors, AT-x01 CompileCheck 0 errors): NocAtmosphere.hlsl
  (globals _NocArk / B / G / L / S, _NocArkBeamP / D[6]; NocArkShape, NocLineApproach, NocTubeIn, NocArkAir, NocArkAirTo for DR's
  own-fog shader, NocArkSpot, NocArkPool; hooks in NocCloudColorN (occluder, flash stretch and outline), NocAbyss (pools), NocFog
  (spot before the fog, air after it)), NocSky.shader (dome adds NocArkAir), NocSchedule.cs (NocArkAir constants, Update / SetBeam /
  ClearOfWalker / ClearOfBridge / FlashCentre; NocAtmoState Ark fields; twins ArkShape / ArkAir / ArkSpot / LineApproach / TubeIn;
  CloudColor), AtmosphereDriver.cs (UpdateArk from HullCourse incl. HullCourse.Searchlight, UploadArk, flash on the live Ark,
  QA -ark-air X / -ark-occ X), new Tests/EditMode/AtmosphereArkTests.cs (+ .meta: 5 tests).
GLOBAL 20:39: NocAtmosphere.hlsl (every shader recompiles): identity while the Ark is parked; from s 380 the deck darkens round the
  Ark, the air round it glows, from s 470 its six searchlight cones light one by one, with spots on surfaces and pools on the haze
  sheets; the s 553 / 790 sheet lightning centres on the Ark.
- 20:41-20:45 AT-b01 (exit 0, 190 s). AT_ark01 (13 poses s 450-820 + flash pairs) and AT_ark01off (-ark-air 0 -ark-occ 0) looked
  at: the cones read as soft volumes (not lasers: 6 m apex, 3.5 deg, Gaussian), faint and short at gain 0.22 / fall 900 m; A/B diff
  p99 +4.8 / +8.7 / +10.7 codes (ark520 / 560 / 700n), the occluder and the 0.010 glow near invisible at 0.9 km. Flash pairs (frame
  mean, linear): s 554 / 552 +1.6 %, s 791 / 789 +1.2 % (both include 0.8 s of the Ark's motion; D-010 < 10 %).
- 20:47 batch 2 (constants): BeamGain 0.40, BeamFall 1 600 m, PoolGain 4, SpotGain 0.6, GlowGain 0.025. AT-b02 (25 s).
- 20:50 AT_ark02 (9 poses s 480-860) looked at full size: ark700n (QA/captures/AT_ark02/pose_ark700n.png) - four cones under the
  5 km belly falling into the abyss along the rift, ark640n, ark480 (the first lamp wakes under the bow at s 480), ark760nd / sd.
- 20:51 AT-ARKt02 (Atmosphere|AtmosphereAbyss|AtmosphereFix|AtmosphereArk|Grade|Material): 78 / 78 ([NOC] ark flash gain over the
  Ark 1.21 %, beside it 5.55 %; lit beams sampled over s 400-1 100 x 16 times: 22 342, closest 151 m beyond 3 r of the eye).
- 20:52 perf A/B 1080p (-autotest -cinematic -beat 4 -autotest-until 760; off = -ark-air 0 -ark-occ 0): GPU mean s 580-600 8.46 ->
  9.35 ms, 600-700 6.56 -> 6.93, 700-740 4.40 -> 4.54; the on run had a 20-73 ms spike train at s 741-749 while FX's integ players
  and DR's build ran (contaminated). 20:55 batch 3 (perf): NocTubeIn window 1 - smoothstep(4, 6, h^2 / r^2) (skirt ends at 2.45 r;
  the cut test 9 -> 6 r^2) and the erf pair skipped when the closest approach lies 2.5 widths inside the segment (twin + test).
- 21:00 AT-ARKb03 (batch 3, 189 s). 21:05 KS08-KS14 on / off (QA/captures/AT_arkks_on / _off, -ark-air 0 -ark-occ 0): pixels > 3 codes
  KS09 3.9 % / KS10 3.1 % / KS12 2.7 % / KS11 2.0 % / KS13 0.7 % / KS08 0.2 % / KS14 0.1 % (the cones; KS10's drop untouched, the
  cones are in its top rows; KS09: two cones crossing under the bow). No pool and no spot showed in any capture.
- 21:07 batch 4 (perf): pools and spots off (PoolGain / SpotGain 0; NocArkSpot gated on _NocArkS.x). The pools ran inside NocSkyColor
  = the fog colour of every fogged fragment below the horizon (18 beam-sheet pairs each): that was the cost.
- 21:13 AT-ARKb04 (186 s). Perf A/B interleaved on / off / on / off (1080p, -autotest -cinematic -beat 4 -autotest-until 640;
  QA/reports/AT_ark_perf4_*.csv): GPU mean s 580-600 on 7.68 / 7.31 vs off 7.22 / 7.67 ms; s 600-640 on 6.25 / 5.96 vs off 5.89 /
  6.61; s 560-580 on 7.42 / 6.89 vs off 7.18 / 7.51: AT's terms are inside the run-to-run noise (<= 0.5 ms). GPU p99 s 580-600 is
  about 10 ms with and without them (DR's Ark itself; lead / DR). The 4K window was not re-measured (the 1080p cost is noise-level).
- 21:16 seq_dread_hull_pass (960x540, 289 frames, s 544-556 with five cones): luma step 0.061 / frame, diff spikes 0, flashes G1-G4
  PASS, block blinks 0; block pops FAIL 133 = one-frame rain streaks (QA/captures/AT_arkseq/seq_dread_hull_pass/pops_crops.png;
  FX's rain pass, not AT). Frames deleted.
- 21:18 flash A/B at the same s (-flash 0): s 554 frame mean x 1.018, deck round the bow +2-4 codes with the bow dark (outlined);
  s 791 x 1.0065. DREAD 5.4's +15 % cap kept.
- 21:28 beat 2 identity (KS04 / KS05 / KS07, default vs -ark-air 0 -ark-occ 0; QA/captures/AT_arkb2_*): max diff 0 codes in all three
  (every Ark global is 0 before DR's s 380 launch).
- 21:30 glow A/B (ark480 / 520 / 640n, -ark-air 0 -ark-occ 0): the glow is a soft haze round the hull's lit lower steps (x 2.5 crop
  scratchpad ark/g/ark480_bow_x25.png, no disc edge); under the belly (640n) it lifts the haze above the bridge by about 1-3 codes
  (the lit air under the ship); the deck occluder reads only beside the hull (from the Edge the deck over the Ark is behind it).
- 21:38 AT-ARKfull (full EditMode suite on batch 4, the final AT code; QA/reports/AT-ARKfull.xml): 518 / 521, 2 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch, Explicit). Both failures are AU's (AudioDreadTests, against DR's new course): 
  Cues_AreKeyedToTheirSAndTimes ("a glow outlines the hull (abeam at s 596)": DreadScore.LightningS still holds 553, the old
  hull's pass) and Hull_CrossesAlongZAtEighteenMetresPerSecondWithASlowDoppler (the old 18 m/s hull). AtmosphereArkTests 5 / 5.
REQUEST AU: Assets/_Game/Scripts/Runtime/Audio/DreadScore.cs:332 - the lightning retimed to the Ark (ARK.md, AT's part): LightningS
  = { 205.0, 345.0, 592.0, 790.0, 1140.0, 1420.0 } (553 -> 592: its glow peaks at s 593 with the bow 85 m north of the walk line,
  overhead, AT's centre 250 m aft of the bow; within 4 s of DR's bow pass s 596 (your test), outside KS11's s 600, >= 62.5 m from its
  neighbours, beat 4). AtmosphereDriver / AtmosphereFixTests take any times (the centre at s 592: 318 m from the walker).
REQUEST FX: Assets/_Game/Scripts/Runtime/QA/ShotPlan.cs:614 - SheetLightningS = { 205, 345, 592, 790, 1140, 1420 } (mirror of AU's).
- 21:39 AT-ARKb05 FINAL build (Builds\AT, exit 0, 0 errors). 21:42 smoke AT-ARKsmoke (-autotest -brisk -autotest-until 880 -beat 3):
  AUTOTEST PASS, beats 3 / 4 / 5 entered at s 400.00 / 560.00 / 880.00 (t 0.17 / 64.32 / 192.32), errors 0, 32.1 B/frame (the engine
  input box; QA/reports/autotest_brisk_from400_until880.json).
- 21:43 AT_arkfinal (final build, 10 poses s 480-860 + the bow pass s 593 looking up) looked at full size: ark700n (four cones under
  the belly into the abyss beyond AB's new air crossings), fl593 (the bow overhead, the lit deck beside it, the occluded deck past
  its far edge), ark560 / 480 (the first cones waking under the bow). Kept: QA/captures/AT_arkfinal, AT_arkks_on / _off (KS08-KS14
  A/B), AT_arkb2_on / _off (beat 2 identity), AT_arkseq (sequence report crops only); about 86 MB. Deleted AT_ark01 / 02 / 01off /
  arkfl / arkg and the sequence frames. fastcheck all OK (runtime 221 / editor 297).
- Files (AT): Shaders/Include/NocAtmosphere.hlsl, Shaders/NocSky.shader, Scripts/Runtime/Core/NocSchedule.cs, Scripts/Runtime/Systems/
  AtmosphereDriver.cs, Scripts/Tests/EditMode/AtmosphereArkTests.cs (+ .meta, new), docs/logs/AT.md, Logs/AT_progress.md.
- Open: AU's two tests and the s 592 retime (REQUEST AU / FX above); 1080p GPU p99 at s 580-600 about 10 ms with or without AT's
  terms (DR's Ark; ARK.md budget 8.2: lead / DR); the 4K s 580-600 window not re-measured by AT (AT's 1080p delta is inside the
  noise); the hull-pass sequence's "block pops" are one-frame rain streaks (FX's rain pass).
AT ARK DONE 21:44
