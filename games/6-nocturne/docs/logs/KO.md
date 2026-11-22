# KO log - the koi (Look v2, TEXTURE_PLAN 8.3 KO and section 6; LOOK_V2 2.1, 6 "Climaxes", 8.1, 8.2; CRIT0 #9; 14.7 H6)

Owner KO. Exclusive files: `Shaders/Beats/NocKoi.shader`; `Beats/Beat07_Heart/KoiBuilder.cs`, `KoiDriver.cs`,
`KoiShaderMath.cs`, `KoiMath.cs` (motion only on a GE request for GB-03; T-KOI stays green); `Scripts/Tests/EditMode/
KoiShaderTests.cs` (new), `KoiMathTests.cs`; this log; `QA/critique-KO.md`.
Requests to KO: write "REQUEST KO: <file>: <change>" in your own log; answers appear here as "DONE KO <ID> <request>".

## 2026-11-14

### 09:50 start
- Read: TEXTURE_PLAN 6, 7, 8.1-8.4, 9, 10, 11; LOOK_V2 1, 2 (tokens, 2.3 emission factors and displayed cores, 2.4 R2
  exception for the koi), 6 "Climaxes" (the koi ladder), 7 (beat 7 post), 8.1 (KS17-KS19), 8.2 (checks 8, 12), 10 (NocKoi
  lines); CRIT0 #9 and the KS17 / KS18 rows; critique-graybox01 (GB-01, GB-03); MASTER_PROMPT 14.5.7 "The koi", H6, 14.10
  beat 7 row; STATE Commands; BUILDER_BRIEF; DEVELOPMENT D-001..D-029 (D-009 koi, D-026 sequences).
- Code read: NocKoi.shader, KoiBuilder, KoiDriver, KoiShaderMath, KoiMath, KoiMathTests, Beat07HeartTests (SU's file:
  `KoiShader_MatchesKoiMath_AndStaysInBounds` compares every body ring vertex with KoiMath.RingPoint and every vertex with
  uv2.x > 0.5 with KoiMath.VeilPoint, all within 5 cm, and every vertex inside OrbitBounds; total koi <= 30 k triangles).
- Markers: PA TOKENS READY 09:38 (the 15 new tokens; KoiBody / KoiPatch / KoiGlow still carry v1 values until PA-1).
- Baseline looked at: graybox01 KS17, KS18 (full size): the koi is an opaque white-cyan banana with a flat white veil.

## Resume rule
Find the last step marked GREEN below and the last commit with "KO:" in the message; continue with the next open item.

### 12:40 resume after the pause (2026-11-14)
- Re-read this log and STATE "PAUSED". No KO file was in the WIP patch, so KO starts its first build step now. Markers: PA TOKENS
  READY 09:38 (PA's working tree already carries the v2 KoiBody #9EE0EE / KoiPatch #FF6A2A / KoiGlow #5CDCFF and
  EmissionKoiBody 0.55 / EmissionKoiRim 1.6; the generated NocPalette.hlsl still has v1 until PA regenerates). No REQUEST KO line
  in any log.
- Baseline measured on graybox01 (scratchpad ko_measure.py: koi mask in the KS box, body = luma >= 110 eroded 3 px):
  KS17 body median 214.6 (p10-p90 211-219), structure std (luma minus 15 px box) 5.6 codes, patch share 0, clipped 0.11 % of koi;
  KS18 body 215.8, structure 4.1, clipped 0.26 %; KS19 body 202.1, structure 3.9. The banana: flat, white, no parts.

### Step 1 - design (prototype in scratchpad ko/proto*.py, means.py)
- Scales: imbricate fish scales on an offset-row lattice in (theta, sv): 18 columns around, rows 0.6 column widths apart and
  following the radius (rows = integral da / (0.6 x 2 pi max(rho, 14) / 18): 151 rows head to tail, 10.05 m at the widest ring,
  2.9 m at the tail), disc radius 0.62 columns (the lattice covering radius is 0.508, so every point lies in a disc); the most
  anterior disc containing a point is on top; candidates are composited front to back with soft coverage over the pixel
  footprint, so the filtered shade keeps its mean 0.6245 at every footprint (0.6240-0.6248 measured for footprints 0.005-0.1
  columns); rim highlight on each scale's free edge, a shadow just behind it, sheen toward the posterior; faded to the mean below
  2.5-5 px of pitch. Scale arcs checked in the unwrapped preview: ")" convex toward the tail.
- Kohaku: four patches (sandan + tail spot) at a 194 / 356 / 492 / 618 m, half-lengths 48 / 39 / 27 / 13, organic edges from two
  value-noise octaves (80, 34 m) on (a, (1 - sin theta) 70 m) (sin theta: symmetric about the dorsal line AND smooth on it, so no
  kink), wrap to 2.3 +- 0.3 rad from the top, head mask a < 108 m: area share 21.5 % (proto), wrap 73-81 %. Patch edge scalloped by
  the scale centres. Noise = integer hash value noise, identical in HLSL and C#.
- Structure factor 0.55 + 0.45 vein, vein = 0.5 + 1.15 (scales - 0.6245) + 0.55 (caustic - 0.4392): mean 0.774, std 0.111 (14 %).
- Parts: pectorals exactly on KoiMath.PectoralPoint (cut to the paddle v <= 1 - 0.45 u^1.6), dorsal on DorsalPoint, 4 barbels in
  the snout frame; all frame parts are rigid (F, N, B) offsets -> (theta, radial, forward) of the existing vertex path, so the
  shader's vertex code is unchanged and Beat07HeartTests' identity (body rings vs RingPoint, veil vs VeilPoint) still holds.

## 2026-11-15 (third start, run wf_2c6d782f-763)

### 21:30 resume
- Re-read this log, STATE "RESUMED, THIRD START" / "PAUSED AGAIN", the D-030 lines and `git diff` of the KO files: koi v2 of step 1
  (NocKoi, KoiBuilder, KoiDriver, KoiShaderMath; KoiShaderTests new) is in the tree, uncommitted. PA's palette v2 is in the
  generated NocPalette.hlsl (KoiBody 0.3419 0.7454 0.8550, KoiPatch 1.0 0.1441 0.0232, KoiGlow 0.1070 0.7157 1.0). No
  "REQUEST KO:" line in any log.
- KO-t01 (2026-11-14 13:25; filter KoiShaderTests|KoiMathTests|Beat07HeartTests, 54 cases in the xml): the one KO failure is
  ScaleAndCausticMeans at footprint 0.200 columns (mean 0.6191 vs 0.6245 +- 0.004; 0.005-0.100 gave 0.6240-0.6247).

### Step 2 - the KO-t01 failure: neither twin nor shader is wrong, the composite's mean depends on the footprint
- A numpy port of the composite (scratchpad ko/means.py) gives the twin's numbers (0.6189 at 0.2), so shader and twin agree.
  The soft coverage composite darkens as the footprint widens (front-to-back coverage, the shadow band 0.16 + fw wide, rims
  that fall outside the coverage): mean 0.6243-0.6248 for fw <= 0.08, 0.6221 at 0.15, 0.6189 at 0.2, 0.6115 at 0.3, 0.6048 at
  0.4 columns. The scales still show there (sFade = smoothstep(2.5, 5, 1 / max(fwX, fwY)) > 0 up to fw 0.4), so the body
  dimmed with distance by up to 1.15 x 0.006 x 0.45 = 0.3 % (worst at fw 0.2-0.25 where sFade is still ~0.65-1).
  Normalising the shadow band alone overshoots (+0.0065 at 0.1).
- Fix: the vein subtracts the footprint mean, veinS = 0.5 + 1.15 sFade (sc - mean(fw)), mean(fw) = 0.6245 - 0.0737 x^2 /
  (x + 0.08), x = max(fw - 0.07, 0) (least-squares over 19 footprints 0-0.45, max residual 0.0005). New #defines KOI_SMF0 /
  KOI_SMA / KOI_SMB (twin ScaleMeanF0 / A / B, ScaleMeanAt). The test now checks the fit at 0.005-0.4 (+-0.0015) and the defined
  mean for fw <= 0.08.
- Found while checking the shader: the caustic network q = (a, 40 theta) did not close at theta = 2 pi (p.y advances by pi per
  ring, frequencies 1.7 / 1.4 / 2.2): a seam along the outer flank (theta 0), mean jump 0.36 of the caustic = 0.2 of the vein =
  about 12 % of the body brightness on a line. Fix: every p.y / r.y frequency 2 (the network closes; mean 0.43917, std 0.324
  vs 0.4395 / 0.326 before; looked at the unwrapped network, scratchpad ko/caust_cmp.png: same ridge character, no seam). New
  test BodyPattern_ClosesAtTheRingSeam (caustic, structure, patch, eye at theta 0 vs 2 pi).
- KO-t02 (21:46, same filter): 28 / 28 passed. [KOI-LOOK] scale mean vs fit at fw 0.005-0.4: 0.6244/0.6245, 0.6244/0.6245,
  0.6247/0.6245, 0.6240/0.6239, 0.6223/0.6216, 0.6190/0.6186, 0.6113/0.6119, 0.6048/0.6049; caustic mean 0.4387; ring seam jumps
  2.3e-6 / 1.9e-6; patch share 21.7 %, wrap 74 / 81 / 73 %, head 0; structure mean 0.776, std 0.110 (14.1 %); eye ratio 0.30;
  mesh 5 627 vertices, 10 600 triangles; fins vs KoiMath 0.03 / 0.02 cm; Beat07HeartTests koi identity body 0.03 cm, veil 0.01 cm.

### Step 3 - first look (KO_r01, build lookdev_KO 21:5x; fastcheck editor had a foreign error in Beat06SignsTests.cs:107, not KO)
- KS17 1080 (QA/captures/KO_r01/KS17_heart_reveal.png, full size + 3x crops of head and mid-body): the banana is gone - a cyan-rimmed
  translucent koi with scalloped vermilion Kohaku patches, scale discs, 4 barbels, eye spot, two pectoral paddles. Measured
  (scratchpad ko/ko_measure.py, koi box of LOOK_V2 8.2): body (luma >= 110, eroded 3) median 176.1, p10 144.4, p90 202.2; koi p99
  210.2, p99.9 215.2; structure std (luma - 15 px box, body eroded 7 px) 5.10 codes (graybox01: 3.71); clipped 0 %; patch pixels
  3 961 (median hue 9.4, sat 0.55, luma 113); frame mean 45.1, sky top 47.2.
- Defects seen: (1) structure 5.1 < 8 codes: where the body curves away (right half) the broad Fresnel rim ((1 - NdotV)^2.2 x 1.6)
  covers the whole surface - flat cyan sausage, scales invisible; (2) patch hue 9.4 / sat 0.55 (check 12 wants 10-22 / >= 0.60):
  the aqua back shell and the cyan rim pull the vermilion to red and desaturate it; (3) body median 176 at the top of 130-175, reads
  as pale grey plastic near the head (white gradient); (4) pectorals read as long glass capsules (rays invisible, parallel-edged
  paddle); (5) head: eye spot faint, gill arc not visible; (6) a large ring-shaped caustic ridge reads like a bubble in the mid body.
- image_metrics KO_r01: 3 dark NaN-like pixels at (255, 377), (263, 418), (41, 501) - lit facade tiles of the left towers, not the koi.
- GREEN, committed eaa10a9 (KO files + this log; tests KO-t02 28/28, Unity compile and the player build passed, koi renders with
  no NaN / black / magenta pixel).

### Step 4 - r02 batch (fixes KO-03..KO-07 of QA/critique-KO.md)
- Shader: Fresnel rim ^3 (was ^2.2) and x (0.8 + 0.5 (vein - 0.5)); structure factor 0.40 + 0.75 vein (mean still 0.775), vein
  = 0.5 + 1.3 (scales - mean(fw)) + 0.6 (caustic - 0.4392) (numpy: factor std 0.196 = 25 % vs 14 %); operculum shadow band behind
  the gill arc; pupil x0.15 + ring 0.6; head white lerp 0.30; 6 m scanlines +-7 %; tail: body x (1 - 0.6 f), rim x (1 - 0.4 f),
  f = smoothstep(830, 905, a) (the peduncle dissolves into the veil); veil gain 0.20 (was 0.12) with membrane 0.28 / rays 0.9 /
  edge 1.2 (membrane ~ +12 codes over the deck, rays ~ fins); fan fins (pectoral, pelvic): 11 jointed rays radiating from a
  short root, leading / trailing ray, rounded edge 1 - 0.3 s^2; dorsal and anal share the sloping-ray shading.
- Back shell -> front shell: the back fragment intersects its view ray with the local cylinder (radius rad around the spine
  tangent, new varying tangentWS) and evaluates the front point's patch (KoiFrontPatch, twin FrontHit / FrontPatch): a front
  patch tints the back shell vermilion (light through pigment), a back-only patch shows at 0.3 x 0.35; patch rims glow
  vermilion at (1 - 0.5 pk) instead of cyan (no pink mixes).
- Builder: pectorals are a fan inside KoiMath's (u, v) square (FanUV: u <= 0.35 + 0.65 smoothstep(0, 0.75, v), v <= 1 - 0.3 s^2;
  KoiMath's composition samples still bound them); new KO frame parts: pelvic fans (a 423-451 m, 70 deg below the side axis, 60 m
  along normalize(-0.65 F +- 0.65 N - 0.40 B)) and the anal fin (a 630-670 m, 10 -> 24 -> 8 m, swept back 0.5 h). Mesh 5 789
  vertices, 10 840 triangles.
- KO-t03 (22:44): 29 / 29. New: BackShell_FindsTheFrontShellOnItsViewRay (1 912 rays: radius error 3.6e-7, off-ray 1.3e-7, all
  facing the camera; below -> belly, side -> outer flank), pelvic + anal vs their frame offsets 0.03 cm, pectoral (u, v) inside
  the square; structure mean 0.776, std 0.196 (25.3 %); eye ratio 0.15.

### Step 5 - r02 look (QA/captures/KO_r02, build 22:56; KS17, KS18, KS19 at full size + 3x crops of head, mid body, fins, tail)
- KS17: body median 161.7 (p10 136.2, p90 194.7), koi p99 205.9 / p99.9 210.3, structure std 7.77 (r01 5.10), clipped 0, patch
  px 10 349 (r01 3 961) hue 14.3 / sat 0.72 / luma 108 (check 12: 10-22 / >= 0.60 PASS); check 12 warm stray outside the goal
  point 0 px; goal point 63 px, core 246.
- KS18: body 167.3 (141.9-199.8), p99 209.3, structure 7.39 (r01 3.84), clipped 0, patch px 11 327 hue 13.7 / sat 0.71 (PASS),
  warm stray 0. KS19: koi body 155.4, no warm koi pixel in the head box (check 8 koi part PASS).
- Seen: vermilion Kohaku patches with scalloped scale edges, clear imbricate scales, gill arc, eyes (KS17), fanned pectorals with
  visible rays (KS18), pelvic fans, a forked veil with rays. Still wrong: (a) the peduncle ends in a rounded bright tube cap that
  sticks into the veil (KS18 1300-1500, 690-780); (b) a back-only patch at 0.3 weight reads as a grey-mauve stain (KS17
  1220-1250, 360-440); (c) scanlines not visible; (d) structure 7.4-7.8 < 8; (e) head from below has no mouth.
- image_metrics KO_r02: NaN-like dark pixels (KS17 1 at (255, 377); KS18 15 at (1237, 803) .. (1739, 892)) are dark mullions
  between lit facade tiles of the towers / spire, not koi pixels; banding[plaza] 2.0 in KS17 is the plaza floor (not KO).

### Step 6 - r03 batch
- Tail fade a 820-900, body x (1 - 0.9 f), rim x (1 - 0.85 f); veil root mask smoothstep(0, 0.15, v) (was 0.08-0.30).
- Back shell: colour lerp(aqua, vermilion, max(frontP, 0.25 x own patch)) at the full 0.35 weight (no dimming, no stain).
- Scanlines: thin 6 m lines (KoiLine half-width 0.06 period, +31 %, mean 0.12 subtracted), faded below 6 px; 24 m bands kept.
- Mouth: dark lip line on the lower front of the snout cap (a = -7 + 3.5 (1 - cos dV), |dV| < 1.1 rad from the ventral line).
- KOI_VEINS 1.45 (numpy: structure std 0.203 = 26 %).
- r03 (QA/captures/KO_r03, build 23:17): KS17 body 164.0, structure 8.27, patch px 10 270 hue 14.2 / sat 0.72, warm stray 0;
  KS18 body 171.3, structure 7.96, patch 11 161 hue 13.7 / sat 0.71; clipped 0 in both. Looked at: the grey stain is gone (a faint
  warm hint stays where only the back is patched); the tail end fades (tube centre at 1 450 px: 111 -> 79 codes) and the veil rays
  converge into it; but the thin world-height scanlines of BOTH shells cross into a lattice of rings on the head from below (KS18
  head crop) - noisy.
- KO-t04 (23:29): 29 / 29 (VeinScale 1.6 already in: structure mean 0.774, std 0.210 = 27.1 %).
- r04 batch: scanlines on front faces only at +26 % (0.3 x (line - 0.12)); KOI_VEINS 1.6.
- r04 (QA/captures/KO_r04 + KO_r04b beat-7 stills, build 23:37): KS17 body 164.1 (142.2-195.3), p99 206.2, structure 8.52, patch
  px 10 268 hue 14.2 / sat 0.72, warm stray 0, clipped 0; KS18 body 171.3 (144.8-202.1), p99 210.0, structure 8.21, patch px
  11 125 hue 13.7 / sat 0.70, warm stray 0, clipped 0. Beat-7 stills (contact sheet + look_up60 at full size): look_up60 shows
  the whole koi from below - scales, patches, eye, barbels, mouth line, gill arc; image_metrics KO_r04b PASS 7 / 7. Remaining:
  the world-height lines draw contour loops where the skin is near horizontal (KS18 head from below).
- GREEN: KO-t04 29 / 29 on this code; committing the r04 state.
- r05 (QA/captures/KO_r05, build 23:59; scanlines faded where |n.y| 0.55-0.85): KS17 1080 body 164.0 (142.4-195.3), p99 206.2,
  structure 8.34, patch px 10 265 hue 14.2 / sat 0.72, warm stray 0, clipped 0, goal point 63 px core 246; KS17 4K body 163.5,
  structure 8.85, patch 40 971 px hue 14.3 / sat 0.72, warm stray 0, clipped 0. KS18 1080 body 171.5 (145.2-201.9), p99 210.0,
  structure 7.99, patch 11 149 px hue 13.7 / sat 0.70, warm stray 0, clipped 0; KS18 4K body 171.9, structure 7.47 (15 px of 4K
  = half the angle), patch 44 537 px hue 13.6 / sat 0.70, clipped 0. Looked at 4K crops (KS17 head, KS18 head / mid / tail): the
  contour loops on the belly are gone; two eyes (far one through the head), gill arc, mouth line, barbels, scalloped patches,
  fins translucent with towers through them. Left: barbels read as flat 4-sided planks at 4K; the peduncle still ends in a
  rounded (dim) dome inside the veil; the scales are perfectly uniform.
- r06 batch: per-scale brightness +-12 % (hash of row and half-column, 36 around: closes at the seam; twin identical);
  BarbelSides 6; tail cap a half-ellipsoid 3.5 rho long (pointed, inside the veil; the head cap stays a hemisphere).
- KO-t05 (00:11): 28 / 29 - ScaleAndCausticMeans at fw 0.005-0.05: 0.6229 vs 0.6245 +- 0.0015. Cause (numpy, scratchpad
  ko/jitmean.py): over the test's 100 x 100 cells the per-scale jitter's own draw moves the mean by -0.0015 (over 2 000 x 2 000:
  +0.00003); the koi's real 2 736 scales have a jitter mean of -0.0026 = -0.06 % body brightness. Test now samples 2 000 x 2 000.
- r06 (QA/captures/KO_r06, build 00:17): KS17 body 164.1 (141.5-195.1), p99 206.2, structure 8.42, patch 10 275 px hue 14.2 / sat
  0.72, warm stray 0, clipped 0; KS18 body 171.2 (144.8-202.0), p99 210.1, structure 8.03, patch 11 196 px hue 13.7 / sat 0.70,
  warm stray 0, clipped 0. Looked at KS18 tail crop: the peduncle now narrows to a point inside the veil (no dome); KS17 head
  crop: per-scale variation subtle, eyes / barbels / gill arc intact.
- KO-t06 (00:2x): 29 / 29 (mesh 5 877 vertices, 11 000 triangles; scale means within 0.0007 of the fit at every footprint;
  structure mean 0.772, std 0.214 = 27.7 %). GREEN -> commit.

### Step 7 - temporal check (koi sequence, KS18 walk: -capture-set koi -capture-only seq_koi_ks18, 720 frames 1080p 60 fps)
- Tools/qa/sequences.py: PASS (luma step max 0.024 levels / frame at frame 10, <= 1.2; 0 difference spikes; QA/reports/
  sequences_KO_seq1.md). KO's own measure (scratchpad ko/ko_seq.py: fine-detail energy E = mean |luma - 5x5 box| inside the koi
  mask, luma >= 100 eroded 4 px; jitter = |E - 7-frame running median| / median): E 3.31, jitter p95 0.44 %, max 1.39 % (frame 43);
  koi mask area step max 0.54 % / frame; warm (patch) pixels step max 65 of ~8 724. No flicker or crawl of the scale lines,
  scanlines or fin rays. Looked at frames 0 / 240 / 480 / 719 (the body bends through its wave phases). Frames deleted after
  the analysis (2.0 GB).
- Beat-7 stills on the r06 build (QA/captures/KO_r06b, image_metrics PASS 7 / 7; look_up60 at full size: the whole koi from
  below, 0 clipped pixels; start / mid show only the koi's edge at the frame top) and KS19 (KO_r06/KS19_goal_window.png): koi
  body median 156.3, structure 7.95, 0 warm koi pixels in the head box (check 8 koi part PASS), clipped 0.
- Kept capture folders: KO_r05 (KS17 / KS18 at 1080, 4K, 16:10, 21:9 - the 4K pass), KO_r06 (KS17, KS18, KS19 final 1080),
  KO_r06b (beat-7 stills final). KO_r01-r04b deleted after their numbers were logged here.

### Step 8 - final checks
- Full EditMode suite KO-t07 (00:53): 351 / 355; KoiShaderTests 11 / 11, KoiMathTests 9 / 9, Beat07HeartTests 9 / 9. The 4
  failures are outside KO's files: Beat02BoulevardTests.Blades_Are14x200 (blade colour order after PA's palette), FoliageTests.
  Crowns_AreLobed (FO), TextureTests.Import_AlbedoLastMipMeanIsHalfLinear (LeafCrown_Albedo slice 0 mean 0.645, TP / FO) and
  TextureTests.Sets_TileMatchesTexturesMd (RoomArr Carpet016 tile, TP / FA).
- PA's Tools/qa/lookv2_check.py on QA/captures/KO_r06: every koi item passes - KS17 koi_patch_hue 14.2 / sat 0.72, goal point
  63 px, warm stray 0 %, R2 0.1 %, p99.9 209.3; KS18 koi_patch_hue 13.6 / sat 0.70, warm stray 0 %, R2 0.1 %, p99.9 212.6,
  separation 6.9; KS19 koi_warm_px 0. Not KO: KS17 separation -4.8 (left tower box vs sky), KS18 dark_cast_RG -4.08 (limit 4;
  cool cast of the dark towers / deck, PA's rig incl. the koi light KoiGlow x 0.7), KS19 mean luma 34.5 and wall texture (FA/SU).
- Ladder (KS18 r06): veil membrane boxes p50 40-43 (deck 44: the deck shows through), rays to 85-94; pectoral p50 64, p90 78,
  max 101 (fins 70-110); body core median 164-171 (130-175); rim / koi p99 206-210 (210-235 at the very edge, p99.9 210-214).

KO DONE 2026-11-16 00:58
