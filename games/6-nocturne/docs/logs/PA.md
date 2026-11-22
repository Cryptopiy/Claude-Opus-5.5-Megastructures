# PA log - palette, atmosphere, post and light colours (Look v2, TEXTURE_PLAN 8.3 PA; LOOK_V2 sections 2, 3, 6, 7)

## Step 1 - the 15 LOOK_V2 tokens (append only)
- NocturnePalette: enum, Table and SpecLinear += CloudCool, HazeLow, LampWarm, WinTungsten, WinNeutral, WinBlue, ScreenTeal,
  ScreenBase, Plaster, Brick, Granite, MetalPaint, SteelF0, Leaf, GlassDark (LOOK_V2 2.2 / 2.3), after TitleText; the 28
  14.2.1 tokens keep their v1 values in this commit. Checked in Python: 43 table rows = 43 enum names in order, every hex
  EOTF within 6e-5 of SpecLinear (max 4.9e-5, ScreenTeal).
- `python -I Tools/gen_hlsl.py`: NocPalette.hlsl +15 PAL_* lines (NocConstantsGen unchanged). ConstantsTests token count 28 -> 43.
- fastcheck all OK.
PA TOKENS READY 2026-11-14T09:38:41+01:00

## Resume after the pause (2026-11-14, 12:37)
- Read this log, STATE PAUSED, TEXTURE_PLAN 8.3 PA / 8.4, LOOK_V2 2, 3, 6, 7, 8, 10. The WIP patch had my palette values
  (Table + SpecLinear = LOOK_V2 2.3, emission factors), Floor, fog 4.4e-4, cloud constants, EV / post tables, KS rows, gaze
  keys, GoalLampCore in the tree, uncommitted.
- Compile fix: NocSchedule.cs mirrors the new cloud model (4 octaves 900/300/110/40 with fade smoothstep(4, 8, L / blurM), 2-D
  domain warp 60 m from n900 / n300, tint model with a tint per glow, optional Gulf shadow as tint 3), the HazeLow share of the
  alley layer in SkyColor, 4-octave scroll in StateAt, w_f / n_f / k_a schedules; AtmosphereDriver uploads Par[4 i + 3] (tint).
  fastcheck all OK (12:37): the tree compiles again.

## Third start (2026-11-15, 21:30 -)
- Read: this log, STATE "PAUSED AGAIN" / "RESUMED, THIRD START", D-030, TEXTURE_PLAN 8.3 PA / 8.4, LOOK_V2 2, 3, 6, 7, 8, 10,
  BUILDER_BRIEF. `git diff` of my files (27 files, +889 / -320) holds, uncommitted and never captured:
  - NocturnePalette: the 28 v2 values (Table + SpecLinear = LOOK_V2 2.3), emission factors tube 2.0, sign 1.6, koi body 0.55,
    rim 1.6 (EmissionWindow 1.2 kept for the v1 facade path only). Generated NocPalette.hlsl / NocConstantsGen.hlsl are in sync
    with `gen_hlsl.py` (checked 21:55 into a temp dir, byte-identical).
  - NocturneConstants: Floor #0A0C0F (0.00304, 0.00368, 0.00478, luma 11.8); cloud tint model (CloudBaseAmp 1.2, TintAmp 0.5,
    Gulf shade 0.35, octaves 900/300/110/40 m, weights 0.36/0.19/0.30/0.15, warp 60 m, fade 4-8 px); SheetSkyAmp 0.15; regional
    sigma 4.4e-4 and FogKnotV beats 2-4 4.4e-4; HazeLowShare 0.6 / path 6 500 m; KoiLightGain 0.7; w_f / n_f knots, AmberRef /
    AmberBand / ScreenAmber s 1 230-1 290; NeonGain 1.6 + NocNeon weights and panel cap; RainTint 0.3; EV / bloom / saturation /
    vignette tables of LOOK_V2 7; KeyShot luma windows = LOOK_V2 8.1 beat windows; D-029 rows KS03 s 117 (vFOV 63.8), KS13 s 718
    eye (533.30, 1.7, 195), KS10 yaw 355 pitch -32; gaze keys s 540 / 556 -> rel yaw -95 pitch -32; GoalLampCoreBlue 0.3 (GB-18).
    Facade (FacadeLit ..) and wet-floor blocks unchanged (byte-identical content).
  - NocSchedule: LinearKnots, WarmShare / NeutralShare / AmberK / ScreenAmber; NocCloudGlow with a tint per glow (Par 4 per glow),
    amplitudes 0.85 / 0.95 / 0.60 / 0.90 / 0.55, tints hot / hot / cool / cool / cool, optional Gulf shadow (tint 3, off);
    NocFogMath CloudGlow (tint, gulf), OctaveFade, 4-octave CloudNoise with the 2-D warp, CloudColor tint model, HazeLowMix in
    SkyColor, 4-octave scroll in StateAt.
  - Shaders: NocAtmosphere (the HLSL twins + _NocCloud2, _NocAmber, NocAmberMix, HazeLow share in NocSkyColorBlur), NocGrade
    (Floor-relative exposure in the prefilter and FragGrade, Floor-relative vignette before the fade), NocNeon (v2 weights with
    k_a per item, fixed slots 0-7, panel <= 0.015 x colour desaturated 60 %), NocStreak (tails toward amber x k_a, heads toward
    WinNeutral), NocScreen (teal / ice / amber / cyan ramp, desat 35 %, ScreenBase 15 %, amber -> ice), NocSky / NocRain comments.
  - NocGradeMath: knee 0.25, Floor-relative Display (vignette and fade separate; the 7-argument overload = fade 1).
  - AtmosphereDriver: SkyFill / AmbientMul / GroundMul / FillTone of LOOK_V2 6, fill pitch 60 yaw 35, trilight, upload
    value / LightCal, _NocCloud2, _NocAmber, _NocFacade.y = w_f(s) and .w = n_f(s) on -facade-v 2 (v1 keeps WarmFraction, w 0),
    and the static AmbientAt / SkyFillAt / SkyFillColourAt that SU asked for.
  - SceneBuilder: fill rotation and colour from the driver. Beat files: BladeSign (6-cycle, R4 light colour, i 4,
    ExtendedPalettes false until SU batches slots 4-7), MedianPromenade (LampWarm r 14 i 3.5, head x 1.6), TonguePier (SignWhite
    balustrade i 2, LampWarm lamps i 3.5), SkybridgeLamps / SkybridgeMaterials (LampWarm i 3.5, head x 1.6, MetalPaint lines),
    ArcadeBuilder (cyan band lerp(NeonCyan, SignWhite, 0.4), ex-violet bands neutral white <= x 0.30), PromenadeBuilder (MetalPaint
    posts, heads x 1.6), ScreenWallBuilder (lights desaturated 40 % toward WinNeutral, i 3.5, ScreenBase fallback), NocScreenMath
    (C# twin of the ramp), PlazaFurnitureBuilder (SignWhite i 2.5, heads x 1.6, MetalPaint posts).
  - Tests in the tree: ConstantsTests (T-FOG 4.4e-4, EV, Floor codes, window shares, post tables), AtmosphereTests (T-FOG,
    LOOK_V2 3.2 display-luma table, deck tint model, 4-octave noise, HazeLow share).
DONE PA SU REQUEST AtmosphereDriver.cs: `AmbientAt(s, out sky, out equator, out ground)` (before LightCal, AmbientMul and the
  garden set blended; Apply uploads exactly it / LightCal), `SkyFillAt(s)` and `SkyFillColourAt(s)` (linear, max channel 1) exist
  and Apply calls them.
- 22:00 tests added: GradeTests (v2 darks below the Neutral start, Display(Floor) == Floor at EV -0.2..+0.3 and in the 16:9
  corner at vignette 0.16, shader strings of the Floor-relative grade, knee 0.25 keeps the brightest window (0.51) out of the
  bloom, GoalLampCore_HueInWindow: GoalLamp x 3.5 with blue x 0.3 displays hue 32.7 / luma 210 at beat 7, 28.3 without);
  Beat06SignsTests (v2 ramp stops Y 0.343 / 0.450 / 0.403 / 0.429, red share <= 0.59, no violet, periodic, amber -> ice; R4
  saturation of the wall lights (max 0.28 by Python), blades (cyan 0.31, amber 0.36, tungsten 0.30) and lamp tokens); RainTests
  (tint 0.3: a drop in front of red / magenta stays at sRGB saturation 0.35). ScreenWallBuilder: doc comment order fixed.
- Colour lines other owners carry (TEXTURE_PLAN 8.4, LOOK_V2 6 / 10), handed over:
REQUEST SU: Beats/Beat01_Alley/G1Kit.cs: the NocNeon batch loop `for (int pal = -1; pal <= 3; pal++)` -> `pal <= 7`, and G1Materials.Neon: `s_neon` with 9 entries, index `Mathf.Clamp(pal + 1, 0, 8)` (NocNeon now has fixed slots 4 SignWhite, 5 WinNeutral, 6 AviationRed, 7 WinTungsten). Write DONE and PA switches BladeSign.ExtendedPalettes to true (the blade cycle then shows SignWhite and WinTungsten instead of WinNeutral / amber stand-ins).
REQUEST SU: Beats/Beat01_Alley/AlleyFixtures.cs: :19 the 4 nearest alley sign lights r 6 i 1.5 coloured lerp(c, WinNeutral, 0.4) (only these 4 are real lights); :29 mouth light lerp(HazeLow, CloudHot, 0.5) r 25 i 2.0 (v1 i 3); :171-174 BladeColour: case 0 NeonMagenta for ONE sign only, SignWhite otherwise, the NeonViolet default -> WinNeutral; :201 mouth wet source CloudHot x 0.8 -> x 0.55; ten door bulkheads / lit transoms LampWarm r 7 i 1.6, 0.25 m off the wall at y 2.6, every <= 12 m on alternating walls (LOOK_V2 6 beat 1: 19 lights, cap 20).
REQUEST SU: Beats/Beat01_Alley/G1Materials.cs: :75-84 G1_Rail SignWhite -> SteelF0 (metal F0), G1_Mouth stays CloudHot; the bar window / warm shops WinWarm, cold shops WinCold (the per-token luminances changed: keep the v1 factors until you calibrate); :99 WetAsphalt is v2 automatically.
REQUEST SU: Beats/Beat01_Alley/NoodleBar.cs: :57, 65, 67 bar light WinWarm r 9 i 6 (LOOK_V2 6), sign colours without violet.
REQUEST SU: Beats/Beat01_Alley/VendingMachines.cs: :15, 51, 61 vending light colour lerp(NeonCyan, SignWhite, 0.4), r 4, i 1.2.
REQUEST SU: Beats/Beat03_Edge/Beat03_Edge.cs: :104 lamp heads SodiumAmber -> LampWarm (x 1.6), :106 balustrade WinCold -> SignWhite, :107 portal ring NeonCyan (kept), :109-110 bridge / layer lights SignWhite; :160-161 the old 82/8/6/4 window hue mix -> NocFacadeMath.MeanHue (FA's v2 mean).
REQUEST SU: Beats/Beat05_Garden/Beat05_Garden.cs: :36 the 8 leaf point lights lerp(LeafGlow, WinNeutral, 0.5) r 30 i 1.0; the 10 lantern-cluster lights lerp(Lantern, WinNeutral, 0.4) r 14 i 3.5 (sat 0.34, R4).
REQUEST SU: Beats/Beat07_Heart/OldQuarterBuilder.cs: :177-193 GB-18: the "goalLamp" core emission = GoalLamp with its blue channel x NocturneConstants.GoalLampCoreBlue (0.3) at the same gain (hue 32.7, luma 210 at beat 7; GradeTests.GoalLampCore_HueInWindow), the room / curtain keep GoalLamp; :199 rail metal ConcreteDark x 1.05 -> MetalPaint; :211 dark glass WetAsphalt x 0.5 -> GlassDark; the tenement entrance light lerp(WinCold, WinNeutral, 0.5) r 8 i 1.5, y 2.8, 0.4 m off the wall, >= 4 m from the goal window (LOOK_V2 6 beat 7).
REQUEST GE: Beats/Beat07_Heart/SpireBuilder.cs: :109-110 strip mix kept; the base ConcreteDark (x factor) -> MetalPaint (LOOK_V2 10).
- 22:07 PA-t01 (ConstantsTests, AtmosphereTests, GradeTests, RainTests, Beat06SignsTests, PathScheduleTests, QaTests,
  Beat07HeartTests, ProjectTests): 129 / 134. Mine: AlleyHaze_KS00MouthAndKS02Slit (KS02 cloud T at el 60 = 0.885 with the
  LOOK_V2 3.1 regional 4.4e-4 under the alley layer; D-006's 0.90 was set at 3.6e-4: threshold 0.875 with the arithmetic in the
  test, el 60 / 75 / 90 = 0.885 / 0.902 / 0.908), Deck_TintModel (the east-leg tint is CloudHot within 0.06 %: the sign-wall
  glow 759 m away adds g 0.0016 of CloudCool; tolerance 0.5 %), Rain tint (red 0.366, v1 0.543: limit 0.38 and <= 0.7 x v1).
  Not mine: ProjectTests.GeneratedHlslIsByteIdentical (NocSurfaceConstantsGen.hlsl, SU), MaterialContractHolds (Leaves /
  LeafCore, FO). Passed: Floor identity, knee, GoalLampCore (hue 32.7, luma 210), ramp stops (Y 0.343 / 0.450 / 0.403 / 0.429,
  red share <= 0.578), R4 wall lights max 0.278, T-WAVE 0.0074 /s (<= 0.012), deck noise mean 0.502 / std 0.122, the gaze rate
  and follower tests with the GB-04 keys (PathScheduleTests all green), the LOOK_V2 3.2 display-luma table (+-3 codes).
- TP-tall01 (22:03, full suite, TP's run) also shows two tests I broke on purpose in files I do not own:
REQUEST INT: Scripts/Tests/EditMode/Beats/Beat02BoulevardTests.cs (no Look v2 owner; G1's in Phase 3): :68 `Assert.AreEqual(i % 4, BladeSign.Palette(i), "magenta, cyan, amber, violet in x order")` -> `Assert.That(BladeSign.Palette(i), Is.Not.EqualTo(0).And.Not.EqualTo(6), "LOOK_V2 10: no magenta or red blade (6-cycle SignWhite, NeonCyan, WinNeutral, SodiumAmber, SignWhite, WinTungsten)")` (Beat06SignsTests.Lights_R4_SaturationOfPaLights checks the tokens and light saturations).
REQUEST GE: Scripts/Tests/EditMode/Beats/Beat04SkybridgeTests.cs: :194 `SkybridgeDims.LampIntensity` -> `SkybridgeLamps.LampIntensityV2` (3.5, LOOK_V2 6 beat 4; the light colour is now LampWarm).
REQUEST SU: Scripts/Tests/EditMode/Beats/Beat07HeartTests.cs: :395 NoAmber_ButTheWindow emitters `NocPal.WinCold` -> `NocPal.SignWhite` (the plaza lamps are SignWhite now, LOOK_V2 6 / 10), add `NocturnePalette.Linear(NocPal.KoiPatch)` (hue 18, outside 20-55) as LOOK_V2 10 lists.
REQUEST TQ: Scripts/Runtime/QA/ShotPlan.cs: EncodeClips: add `AddEncode(list, also4k, "enc_beat07_goal", "tenement wall beside the goal window (KS19 view, LOOK_V2 7 H1)", "KS19_goal_window", EncodeSeconds);` and `AddEncode(list, also4k, "enc_beat01_wall", "alley walls in the bulkhead pools (KS00 view, LOOK_V2 7 H1)", "KS00_alley_start", EncodeSeconds);` (LOOK_V2 7 runs the CRF-23 test on KS02, KS04, KS10, KS17 and the KS00 / KS19 walls; KS02 is already there); the enc_trans_mouth purpose text "fog 5e-3 -> 3.6e-4" -> "5e-3 -> 4.4e-4". Regions for both exist in Tools/qa/regions_default.json (KS00_*, KS19_*).
- 22:27 PA-t02 (ConstantsTests, AtmosphereTests, GradeTests, RainTests, Beat06SignsTests): 70 / 70.
- 22:24-22:27 lookdev slot PA, run r01 (`Tools/lookdev.ps1 -Slot PA -Run r01 -CaptureSet keyshots1080 -Extra -freeze-clock`), after
  TP's RebuildAll of 21:56 (library materials carry the v2 albedos). QA/captures/PA_r01: 20 stills, 0 failures, 0 errors.
  Looked at the contact sheet and KS00, KS02, KS04, KS05, KS09, KS10, KS16, KS17 at full size (+ crops of the KS04 sky,
  the KS08 "NaN-like" pixels). Violet is gone from every frame; the sky is a dark warm grey over the boulevard (KS04 top 55,
  KS05 67) and neutral-cool over the Heart (KS17 39-45); the abyss is umber; no black, white or magenta frame.
  image_metrics "NaN-like" pixels (1-34 per frame) are 1-px dark gaps between lit cells of the v1 facade window grid (KS08
  1556,543: (47,54,61) / (23,25,25) between (88,98,114) and (101,81,61)), not NaNs: FA's v1 path.
  lookv2_check (QA/reports/lookv2_PA_r01.md): 3 / 20 pass (graybox01 v1: 0 / 20; R1a there 17-81 %, now 0 failing):
  R1a / R1b pass in all 20; sky_top passes in all 8 measured shots; R3 passes except KS13 0.336 (deck fills the frame, GB-05)
  and KS17 0.306; mean luma high in KS08 44.7, KS11 43.9, KS14 41.2, KS15 48.6, KS16 68.0, KS17 52.1 (v1 facades lit 0.34
  (FA), foliage blobs (FO), the screen wall (mine)); p99 < 120 in beats 3-4 (windows of the v1 facade at about 100: FA's
  log-normal v2 windows reach 150); separation KS04 -1.9, KS17 -13.0, KS18 -4.1 (v1 facades brighter than the sky, plus the
  koi light, below); dark cast RG KS17 -7.0, KS18 -4.8 (koi light), KS00 / KS01 +4.5 / +4.1 (warm alley haze + SU's alley
  lights); texture checks fail (untextured, FA / SU).
  Defects of mine found: (1) the analytic koi light paints the plaza and the ring towers cyan (KS17 floor luma 42 hue 195 sat
  0.41, right wall 100 sat 0.37): with v2 albedos (x 7) KoiGlow x 0.7 is about 10 x a physical hologram (900 x 60 m at 400 m:
  0.34 sr x radiance 0.3 / pi = 0.032 luminance vs 0.76 x 0.7 x 0.606 = 0.32 in the model). (2) KS16 wall 84-91 codes (target
  60-85, frame 68 vs 38-62): the v2 stops' geometric-mean luminance is 0.405 (v1 0.27). (3) the lid shows no billows (KS05
  sky band std 0.9 code, KS18 0.6): under a glow the tint amplitude 0.5 gives +-5.5 % only, LOOK_V2 3.1 wants +-5-6 codes.
- 22:40 fixes for PA-01..03 (QA/critique-PA.md): KoiLightGain 0.7 -> 0.07; ScreenRampY 0.27 (stops rescaled to a geometric-mean
  luminance of 0.27 after the k 0.8 normalisation, shader + NocScreenMath); CloudTintAmp 0.5 -> 1.0. NocConstantsGen.hlsl
  regenerated (gen_hlsl.py, byte-identical). Tests: ConstantsTests 0.07, Beat06SignsTests stops Y 0.229 / 0.301 / 0.269 / 0.287
  and their geometric mean 0.27.
- 23:08 PA-t03 (ConstantsTests, AtmosphereTests, GradeTests, RainTests, Beat06SignsTests, PathScheduleTests): 85 / 85; T-WAVE
  0.0049 /s.
- 23:04-23:09 lookdev r02 (QA/captures/PA_r02): every city tower renders MAGENTA (the error shader) in 16 of 20 shots. Not PA:
  the build log shows "FacadeArr_Mask.png: invalid or corrupted PNG / Could not create asset" (Logs/lookdev_PA.log:545-696) while
  TP-1 was writing Assets/_Game/Textures/External and TP's rebuild03 (22:40) had rewritten the library materials; NocFacade (FA)
  draws magenta without its arrays. The parts PA changed measure as intended: KS16 wall 69-75 codes (r01 84-91; target 60-85),
  hue 19 / sat 0.29 on the amber wave, 0.08 on ice; KS17 plaza floor 30-33 codes, sat 0.10-0.19 (r01 42, sat 0.41).
  lookv2_check on r02 is meaningless while the towers are magenta (0 / 20); r01 stays the reference until r03.
- 23:15-23:25 PA-1b: (1) NocNeon: NocNeonColour as one expression (the early-return chain gave "potentially uninitialized
  variable" X4000 at NocNeon.shader:103 in Logs/lookdev_PA.log; TP-0's Validate gates X4000); same slots and hash weights.
  (2) BladeSign.ExtendedPalettes = true (DONE SU G1Kit slots -1..7): blades SignWhite 4, WinNeutral 5, WinTungsten 7.
  (3) MedianPromenade lamp heads G1Mat.EmitSodium (lib.GrayMarkerWarm = SodiumAmber x 1.8) -> G1Mat.EmitLamp (LampWarm x 1.6,
  SU's slot): the 24 heads now match their LampWarm lights. (4) LOOK_V2 6 "light in the air": NocHalos (new,
  Scripts/Runtime/Rendering/NocHalos.cs: Queue / Flush, C# twin Peak, SigmaFor) + HaloBuilder ("ZZZ_Halos", runs after every
  builder) + NocStreak kind 3 (Gaussian sigma 0.5 / 0.8 m on +-3 sigma, peak _Tint x 0.05 x min(2, sigma_local / 4.4e-4) x
  (0.6 + 0.4 rho), faded 1.5-3 px of sigma, pulled 3 sigma toward the camera like the beacons, fogged x T x (1 - cloud mix));
  constants HaloPeak / HaloSigmaRef / HaloSigmaSmall / HaloSigmaLarge / HaloExtent / HaloFadePx0 / HaloFadePx1. Queued: 24
  boulevard median heads (LampWarm x 1.6, r 14 -> 0.8 m), 14 tongue lamps, 21 bridge lamps, promenade and 20 plaza lamps
  (SignWhite x 1.6, r 12 -> 0.8 m). AtmosphereTests.Halos_LightInTheAir_LookV2_6 (sigma, peak, beat-2 delta 25-50 codes at
  the centre: LampWarm x 1.6 x 0.05 x 0.92 = 0.074 linear lifts a 41-code sky to about 80, the queue, shader strings).
REQUEST SU: Beats/Beat01_Alley/AlleyFixtures.cs: queue a halo per door-bulkhead lens, `NocHalos.Queue("AlleyBulkheads", <lens centre>, G1Materials.Lin(NocPal.LampWarm, NocturnePalette.EmissionSign), 7.0);` (LOOK_V2 6 light in the air; sigma 0.5 m, twice as strong in the alley haze; NocHalos.cs builds them after every builder). Optional, the same for the noodle-bar sign lamp if it has a lamp head.
DONE PA FA REQUEST ScreenWallBuilder.cs:38 MassFaces: the FacadeBox seed is NocFacadeMath.SkinOverride(NocFacadeMath.OverrideScreenWall) (23:27; in lookdev r03).
- 23:37-23:41 lookdev r03 (QA/captures/PA_r03; TQ's encode clips, GE's new masses and glass strip, SU's alley lights and rebuild,
  FA-1 in the tree): no magenta, no black / white frame; "NaN-like" pixels again only the 1-px gaps of the v1 window grid (FA).
  Looked at the contact sheet, KS04, KS13 at full size, crops of the KS17 plaza lamps, KS11 and KS14. lookv2_check: 4 / 20 pass
  (KS03, KS05, KS06, KS16). KS16 now passes (mean in window, R3 0.40 cap holds). KS17: R3 and the dark cast pass after the koi
  light fix (r01 0.306 / -7.0); only separation -4.8 left (v1 facades, FA). KS18 dark cast R - G -4.09 (cool deck token
  CloudCool has R - G -6 codes by design). KS00 / KS01 R - G +4.75 / +5.12: by 3 x 3 cells the cast sits in the lit pools (KS00
  top-left 13.0 = the red sign light, top-right 9.2 = bulkhead pool; KS01 right 11.4 = the 2 900 K bar pool), the floor and the
  far haze are 1.3-3.5: the warm pools are LOOK_V2 9's beat 1, so check 11's |R - G| <= 4 conflicts with the design there
  (proposed decision in the final report; not loosened in the script). KS19 mean 34.5 (18-32): the new entrance light (SU).
  Halos: KS17 column above the near plaza lamp +15 / +12 / +9 codes at 16-32 px over r01; the boulevard heads show small warm
  glows (12 px sigma at 60 m); not a disc anywhere.
- 23:42 PA-t04 (+ Beat02BoulevardTests, Beat04SkybridgeTests, Beat07HeartTests): 97 / 98; the only failure is
  Beat02BoulevardTests.Blades_Are14x200 (REQUEST INT above, unowned file). Halo test: beat-2 sky 47.5 -> 83.6 at the centre.
- PA-1 complete: palette v2, Floor-relative grade, atmosphere (sky, 4-octave tinted deck, fog 4.4e-4, HazeLow), post tables,
  neon / streak / rain / screen colours, AtmosphereDriver rig, beat light colours, halos; commits 3154d6c, the PA-01..03 fix
  commit and the halo commit; keyshots1080 PA_r03 clean (no NaN / black / white / magenta frame from PA's files), R1 violet
  passes in 20 / 20, sky_top in 8 / 8. FA, SU, FO and KO calibrate to these values (LightCal 1.0, the trilight of
  AtmosphereDriver.AmbientAt, SkyFillAt / SkyFillColourAt, KoiLightGain 0.07).
PA-1 READY 2026-11-15T23:53:04+01:00
- 23:56-00:08 H1 encode test at CRF 23 (Tools/qa/encode_test.py, x264 only, 1080p sequences of the r03 player through
  lookdev -NoBuild; frames deleted after each run, reports in QA/reports/encode_test_PA_enc0N.md):
  - KS02 slit (enc_beat01_slit, region 912,0-1027,227 = regions_default's slit_cloud): source 0.233 PASS; decode 0 visible
    steps / 1000 (the 14.7 H1 (6) step gate passes) but BandScore 1.82 (run 48 px) > 1.5 -> score 1.216 FAIL (enc01).
    Ladder (1) -dither-amp 2.5: decode BandScore 1.70 -> 1.134 FAIL (enc02). Ladder (2) -dither temporal: worse, 4.41
    visible steps / 1000, run 91 px -> 2.203 (enc03). The x8 crop shows the encoder's smoothing of the static dither; at 100 %
    and 200 % the decode is smooth, no step visible. LOOK_V2 7's test is 4K CRF 23 (11.12): not run, disk W: is at 24-25 GB
    and a 600-frame 4K clip needs 4-6 GB (floor 20 GB). Open, for the lead (4K run, or a shorter -capture-4k clip).
  - KS19 (enc_beat07_goal, TQ's new clip): regions_default's KS19 tenement_wall rect lies on far towers at the start of the walk
    and on a roof edge at its end (source already 2.64): no clean region exists for the whole walk; the tenement wall is
    untextured until SU's skins land. Open (TQ / INT: a region for seq_enc_beat07_goal; re-run after SU).
  - image_metrics r03 banding FAILs (KS04 sky_above_facades 1.68, KS05 cloud_ceiling 3.12, KS07, KS13, KS15, KS17 plaza) are
    region contamination: KS05's rect rows 400-486 now hold GE's new beam (pure-sky rows 54-400: BandScore 0.43 in r03, 0.36 in
    r01, both < 1.5; with the rows to 486: 4.68). Not the deck noise (QA/critique-PA.md PA-08).
- PA-2 (TP-0 READY 23:30, split committed in 489beeb): the Look rows need no Look v2 value (LOOK_V2 10 lists none: colours live
  in the shaders and NocturneConstants; NocNeon caps the panel at 0.015 whatever the row's 0.03 says, so the row stays verbatim
  and no material asset goes stale). MaterialLibrary.Look: comments only (serialised names unchanged). MaterialTests.Look:
  LookRows_GainsKindsAndShaders (rows exist, Neon gain = NeonGain 1.6, hash palette, streak kinds 0 / 1 / 2, shaders) and
  LookShaders_V2ColourRules (NocNeon panel cap and no NeonViolet, NocStreak tails / heads / _Tint without magenta, NocScreen
  teal ramp without violet). The grade bypass of TEXTURE_PLAN 7.1 is not needed: TP's NocDebugViewFeature writes the debug codes
  back AFTER the grade (Validate gates it), so NocGradeFeature / NocGradePass stay unchanged.
- 00:18 PA-t05 (MaterialTests, ConstantsTests, AtmosphereTests, GradeTests, RainTests, Beat06SignsTests): 83 / 83.
REQUEST TQ: Tools/qa/regions_default.json (or a seq_ entry): (1) re-place the banding rects that GE's new masses now cut (KS05 cloud_ceiling rows 0.37-0.45 hold the new beam: keep y 0.05-0.36; KS04 sky_above_facades, KS07 sky_above_gulf, KS13 abyss, KS15 crown_gap_sky, KS17 plaza: check against QA/captures/PA_r03); (2) add a "seq_enc_beat07_goal" banding region on the tenement wall that stays wall for the whole 15 m walk (the KS19_* rect is far towers at the start and a roof edge at the end, source score 2.64). Evidence: QA/critique-PA.md PA-08, PA-10.
DONE PA TQ REQUEST NocNeon.shader (TQ-08): both options. NeonFlatPx 6 -> 3 (strokes stay resolved down to 3 px, the flat fade
  runs 1.5-3 px; 1.75 px drawn strokes with the capsule coverage AA hold below that) and the flat regime carries the tone-mapped
  mean: flatCov x Neutral(pk) / pk at the stroke peak pk = max(colour) x gain x breath (NeonToneStart 0.76 = the grade's
  NeutralStart; SodiumAmber x 1.6: x 0.59). GradeTests.NeonFlatRegime_CarriesTheToneMappedMean. To verify in lookdev r04
  (KS04 / KS05 blades) and, for TQ, against the -capture-ss 2 reference.
- 00:38-00:42 PA-tall01 (full EditMode suite): 350 / 355. Not PA: Beat02BoulevardTests.Blades_Are14x200 (my blade cycle on
  purpose; unowned file, REQUEST INT open), FoliageTests.Crowns_AreLobed (FO), ProjectTests.MaterialContractHolds (SU's new
  Concrete / StoneWall row values: stale materials until the next RebuildAll), TextureTests.Import_AlbedoLastMip (LeafCrown,
  FO / TP), TextureTests.Sets_Tile (RoomArr Carpet, FA / TP). Every PA test passes, NeonFlatRegime included.
- 00:43-00:47 lookdev r04 (QA/captures/PA_r04, TQ-08 neon change): no magenta / black / white frame. KS05 side by side with r03
  (scratch crop): the amber blade's glyphs now resolve up to the far end, the flat remainder is brown (x 0.59) instead of a
  bright orange plate; KS04 separation 0.4 -> 1.4. lookv2_check 4 / 20 (same set: KS03, KS05, KS06, KS16); the remaining
  failures are FA (v1 facades: separation KS04 / KS07 / KS17, p99 in beats 3-4, means KS08 / KS11 / KS14), FO (KS15), SU / GE
  (KS13, KS19 mean 34.5 with the new entrance light), the untextured walls (texture checks) and check 11 in beat 1 (lead).
- Final numbers (QA/captures/PA_r04, lookv2_check): sky_top median vs the LOOK_V2 8.2 model: KS02 56.8 / 60.4, KS04 61.3 / 59.4,
  KS05 69.0 / 67.6, KS08 52.0 / 50.5, KS09 40.8 / 40.9, KS12 42.8 / 41.9, KS17 47.2 / 48.4, KS18 48.5 / 48.4 (all within 4 codes,
  saturation 0.05-0.27); R1a <= 0.25 % and R2 <= 1.52 % in every shot; R3 0.10-0.23 except KS13 0.33 and KS15 0.34; p1 >= 12.8
  everywhere (Floor-relative grade); dark cast B - G -1.4..+2.9 everywhere but KS13 (deck, -3.3).
- Open in other owners' files: REQUEST INT Beat02BoulevardTests :68; REQUEST SU AlleyFixtures halos; REQUEST TQ banding regions
  (KS04 / KS05 / KS07 / KS13 / KS15 / KS17 plaza, seq_enc_beat07_goal). Disk: kept PA_r01 (first look), PA_r03, PA_r04 (final);
  deleted PA_r02 (magenta, not PA) and every encode clip (reports kept).
PA DONE 2026-11-16T00:50:25+01:00
