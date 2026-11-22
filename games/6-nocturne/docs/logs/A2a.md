# A2a log - look foundation 1 (atmosphere, lit template, PBR material library, wet floor)

Owner: A2a (wave A). Files: Shaders/**, Runtime/Rendering/*, Core NocturneConstants/NocturnePalette/NocSchedule/ShaderIds/MaterialLibrary,
Systems/AtmosphereDriver, Editor MaterialFactory/PaletteWriter/ShaderGen + new editor files, new tests (AtmosphereTests, MaterialTests,
WetFloorTests), docs/MATERIALS.md, Assets/_Game/Textures. Build slot: `Builds/A2a` (env NOC_BUILD_DIR).

## Plan
1. NocAtmosphere.hlsl single source (sky gradient + cloud deck + abyss sheets, path-integral fog with erf sheets and the capped alley haze,
   cloud mix, NocFloor) + NocFogMath double mirror + AtmosphereTests; AtmosphereDriver writes all globals once per frame.
2. NocSky dome (NocSkyColor, fog identity, no dither).
3. Lit template: BRDFData PBR, analytic environment from NocSkyColor, specular AA, trilight x LightCal, koi hook, vertex AO, fog + floor last.
4. Texture generator (NocTextureGen) + NocSurface triplanar PBR shader + MaterialFactory/MaterialLibrary rows + docs/MATERIALS.md.
5. NocWet (analytic mirror, sources registry, wall planes, puddles, ripples) + WetFloorTests.
6. Material sheet builder (-material-sheet) + captures (sheet, KS00, KS04, KS10).

## Steps
### Step 1 - atmosphere, lit template, PBR library, wet floor (code)
- NocAtmosphere.hlsl rewritten as the single source: NocSkyGradient (14.2.2 stops, D-006 smootherstep HazeHorizon -> A(e) over
  0..-6 deg, A(e) = lerp(AbyssNear, AbyssGlow, smoothstep(0, 90, -e)), H3 sheet modulation weighted by the same blend so it is 0
  at the horizon), cloud deck y 460 (glow Gaussians from `_NocGlowSeg/_NocGlowPar`, 2-octave periodic value noise 900/300 m,
  scroll 3 m/s wrapped per octave, octaves fade by footprint, fogged like geometry with min(sigma, 6e-4)), NocOpticalDepth
  (height term exact, erf sheets, capped alley haze), NocCloudMix, NocFog, NocFloor. C# double mirror NocFogMath + NocAtmoState
  + NocCloudGlow in NocSchedule.cs. AtmosphereDriver uploads every global from NocFogMath.StateAt(s, clock).
- Lit template: NocLighting (BRDFData/EnvironmentBRDF/LightingPhysicallyBased, analytic NocReflection with 3 elevation taps,
  NocDirect x LightCal incl. koi hook, NOC_SPECULAR_OFF), NocLitPass (spec AA via ProjectedSpaceGeometricNormalFiltering,
  smoothness <= 0.95, NOC_HAS_FORWARD_HOOK, NOC_TRANSPARENT premultiplied), new NocSurface (triplanar PBR), NocWet + NocMirror.hlsl
  (H4), NocGlass (back 3000 / front 3001). NocTexRecipes (pure C#) + editor NocTextureGen (24 maps), MaterialFactory rows (22
  materials), MaterialLibrary fields + FloorFor(s). NocWetRegistry (sources/walls), NocMirrorMath (C# mirror), NocMaterialSheet.
- Commands: `python -I Tools/gen_hlsl.py`; `Tools/unity-queue.ps1 -Name A2a-rebuild01/02 -Method ...RebuildAll` (SUCCESS, 24 maps
  in 8.8 s); `-Name A2a-validate01 -Method ...Validate` (PASS 67; found the HLSL reserved word `line` in NocMirror -> lineInt);
  tests `-Name A2a-tests02` -> 161/161 (horizon dE worst 0.170; H4 streak 26 x 128 px h/w 4.92; KS17 mirror point 0.2481).
- Build `-Name A2a-build01` (NOC_BUILD_DIR=Builds\A2a) -> 0 errors; 26 warnings = "integer modulus" in NocValueNoiseP (fixed: float
  wrap) and CS0618 FindObjectsByType(SortMode) in NocMaterialSheet (fixed).
- Captures QA/captures/A2a_01 (keyshots1080) and A2a_sheet01 (sheetL/R/W): looked at KS00, KS02, KS04, KS10, sheets. Fixes from
  the look: brass F0 too red (WinWarm x0.75) -> WinWarm x0.4 + TitleText x0.45 = (0.80, 0.61, 0.42); sheet key brighter/closer;
  cloud noise amplitude 0.6 -> 1.0 (KS02 cloud read flat).

### Decision (for DEVELOPMENT.md) - H4 KS17 streak position
The area-source mass scales with the solid angle (1/d_q^2); along the KS17 floor line the lobe term changes by d(delta)/dx =
5.15/160^2 + 1.7/52^2 = 8.3e-4 rad/m against d(ln Omega)/dx = 2/160 = 0.0125 /m, so the brightness peak moves toward the window by
dx = sigma2 x 0.0125 / (8.3e-4)^2 = sigma2 x 18 100 m: <= 21 m (10 %) needs sigma2 <= 1.2e-3, below the spec's own floor 0.004.
The mirror point (lobe axis on the window) is at 0.2481 (spec 0.246); the brightness peak at 0.735; on screen the streak spans
0.46 deg (window base) .. 1.85 deg (mirror point) below the horizon. T-H4-KS17 pins the mirror point and the span.

### Step 2 - look checks, fixes, static test
- Build `A2a-build02..04` (0 errors, 0 warnings). Captures: `QA/captures/A2a_02`, `A2a_03` (keyshots1080, -freeze-clock),
  `A2a_sheet02` (pose_sheetL/R/W, -material-sheet): looked at KS00, KS02, KS04, KS05, KS10, KS19 and the three sheet poses.
  Sheet log line: `_CLUSTER_LIGHT_LOOP=True lights=5 wet sources active=5 walls=1` (Part 8.6: sky fill + 4 point lights,
  Forward+, every lit Noc shader through NocDirect). Lamp reflections on the wet floor read as vertical streaks (sheetW).
- Cloud: octaves rotated +23 / -41 deg (value-noise lattice no longer aligned with the city grid; KS05 showed soft axis edges).
- Static sequences (`-capture-set statics -rain 0`, clock RUNNING, `python -I Tools/qa/sequences.py`): first run FAILED 3.5-9.1
  levels: a 1 px moving seam line in the cloud where the float wrap of NocValueNoiseP landed exactly on the period (hash of
  cell 64 instead of 0). Fixed (wrap the cell index after floor). Rerun `A2a_statics2`: PASS, all 7 beats, max change
  1.00-1.21 levels (<= 2), p99.9 <= 1.00. Frames deleted after the metrics (reports in QA/reports/sequences_A2a_statics2.*).
- image_metrics on A2a_03: "magenta" fails are the cloud deck (the detector's 'like' rule catches CloudHot itself), isolated
  pixels are the graybox emissive markers / goal window (looked at crops), banding regions of KS09/KS12/KS17 lie on graybox floor
  slabs (visible steps 0.00): QA-tool items for A1, not shader defects.
