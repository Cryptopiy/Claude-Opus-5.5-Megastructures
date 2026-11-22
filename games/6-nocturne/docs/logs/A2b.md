# A2b log - look foundation 2 (facade shader, city field, signs/streaks basics, rain pass, pipeline settings)

Owner: A2b (wave A), after A2a. Files: everything A2a owned (Shaders/**, Runtime/Rendering/*, Core NocturneConstants/NocturnePalette/
NocSchedule/ShaderIds/MaterialLibrary, Systems/AtmosphereDriver, Editor MaterialFactory/PaletteWriter/ShaderGen/NocTextureGen, A2a tests)
plus Editor/PipelineConfigurator.cs, Runtime/City/*, new tests CityTests/FacadeTests/RainTests, this log. Build slot: `Builds/A2b`
(env NOC_BUILD_DIR). Research read: R1 section 3 (facade), R2 section 1 (rain), R3 sections 1, 2, 5.

## Resume rule
Find the last step marked GREEN below and the last commit with "A2b" in the message; continue with the next open item.

## Plan
1. PipelineConfigurator: D-008 complete (postProcessData null: audit item 12), NocRainFeature before NocGradeFeature; [NocProbe]
   line in the player with the live pipeline facts.
2. NocFacade (H2, D-004): NocFacade.hlsl + shader + C# mirror NocFacadeMath; FacadeTests (w forced 0/1 on linear light, tiers).
3. CityField (Runtime/City): generator, exclusions of every 14.4 hero volume, chunked meshes with chunk-local origins, beacons;
   CityTests.
4. NocNeon + NocStreak + NocTraffic lanes + per-camera InstancedBatch submission (NocCameraBatches); boulevard demo lanes.
5. NocRain (H5, D-005): Render Graph feature (1/8 Lloc, blur, 4 layers with SV_Depth per-sample test), C# mirror NocRainMath,
   RainTests (count / coverage / length / seam / NaN), edge probe in the player.
6. Captures KS00, KS02, KS04, KS05, KS09, KS10, KS17 (keyshots1080, -freeze-clock), look, fix.

## Steps
### Step 1 - code for all five items (GREEN: commit 7023815)
- Constants (facade strata, city, signs/streaks, traffic, rain) in NocturneConstants; `python -I Tools/gen_hlsl.py` (312 NC_ defines).
- Shaders: `Include/NocFacade.hlsl` + `NocFacade.shader` (lit template hook), `NocNeon.shader`, `NocStreak.shader`, `NocRain.shader`;
  NocCommon gained NocPcg3d / NocPcg4d / NocCover. C# mirrors: `Rendering/NocFacadeMath.cs` (+ NocHash), `Rendering/NocRainMath.cs`
  (+ NocRainState). City: `Runtime/City/CityField.cs`, `CityMesher.cs`, `CityBuilder.cs` (IBeatBuilder "City00_Field"),
  `NocTraffic.cs`. Per-camera instanced submission: `Rendering/NocCameraBatches.cs` (beginCameraRendering, camera-specific
  RenderMeshInstanced: capture cameras included). Rain feature: `Rendering/NocRainFeature.cs`, `NocRainPass.cs` ([NocProbe] line).
- PipelineConfigurator: postProcessData -> null (audit item 12), NocRainFeature added with Rain.mat; MaterialFactory rows Facade,
  Neon, StreakTail, StreakHead, Beacon, Rain (+ 4 shaders in Always Included); AtmosphereDriver writes _NocFacade, _NocTraffic and
  NocRainState (rho = the -rain X override or the schedule; it was a multiplier before).
- Commands: `unity-queue -Name A2b-rebuild01 -Method ...RebuildAll` SUCCESS (28 materials, features=2, postData=null);
  `A2b-tests01` 173/177 -> fixes (NocRain redefined sampler_LinearClamp: URP's GlobalSamplers has it; setbacks capped at 22 % of
  the half size; independent rectangle-area reference for T-WIN-BOX; speed tolerance + wrap quantum) -> `A2b-tests02` 176/177
  (vehicle count 1 790 vs 1 720 +-3 %: lanes run on through the portal ramp; tolerance 5 %, D-019).
- Numbers (A2b-tests02): T-WIN-MEAN linear w1 0.044323 / w0 0.044397 (0.16 %; gamma codes -34.7 %, information); T-WIN-BOX 0.0000 %
  vs the area reference (0.15-0.94 % vs 32 x 32 supersampling); lit 0.3275 (window-count weighted), hues 82.0/8.1/6.0/3.9 %,
  toggles 0.269 %/s; rings 708 T1 cells / 2 125 T2 cells, 263 T1 + 6 409 T2 towers after exclusions, 24.9 % rotated, max vertex
  160.5 m from its chunk origin; faces kept 41 458 (82 916 triangles), 38 606 back faces for every camera skipped; T1/T2 crossover
  0.000 % (276 T2 towers in view); rain at pitch 0, rho 1, noise off: 155-183 streaks (mean 165.8), coverage 0.865 %, shortest 12.5 px.
- Build `A2b-build01` / `A2b-build02` (NOC_BUILD_DIR=Builds\A2b): 0 errors, 0 warnings.

### Step 2 - first look, fixes (GREEN: commit e5bb2e6)
- Captures `QA/captures/A2b_01` (keyshots1080, -freeze-clock; build01): looked at KS00, KS02, KS04, KS05, KS09, KS10, KS17.
  [NocProbe] in the player: api Direct3D11, reversedZ True, colour R16G16B16A16_SFloat x4, depth D32_SFloat_S8_UInt x4, bindMS
  True, HDR _64Bits, MSAA 4, depth/opaque textures off, shadows off, additional lights PerPixel, LOD cross-fade off, SRP batcher,
  clear SolidColor, no skybox, near 0.05 far 12000. No "stripped" line in the player log any more (audit item 12).
- Seen and fixed: (1) facades grey everywhere: the analytic sky reflection at grazing incidence returned the bright horizon haze
  (the env has no city in it) -> city specular occlusion by the view elevation (vertical faces reflect at that elevation); (2) a
  square mosaic of zones (8 x 4 windows at +-60 %) -> 24 x 2 floors at +-45 % (lit floors as bands); (3) mean too bright ->
  window gain 0.30 -> 0.20 (facade mean ~ the 14.10 budget, KS17 tower median luma 47-55); (4) face ids 24-bit could round to a
  neighbour after perspective interpolation -> 16-bit (FacadeTests checks the interpolation).
- T-RAIN-EDGE in the player (`-rain-edge-probe`, pose:rainedge, -freeze-clock -freeze-rain, -rain 1 vs -rain 0; scratch script
  rain_edge.py): rail interior 5 750 px, max |on - off| 0 codes; edge pixels with rain beside them: added 0.353 vs
  (1 - coverage) x neighbour 0.427 vs neighbour 1.144 (halo would be ~1.0x the neighbour): ratio 0.83, 0 of 44 edge pixels
  brighter than their neighbours' rain. No halo.
- Sign sheet (`-sign-sheet`, pose:signs): near 2 x 1.2 m and the 100 m blade show AA capsule strokes; 200-800 m blades flat colour
  per glyph cell; the 1 m signs at 1 km draw as faint 2.2 px dots. EditMode `A2b-tests03` 177/177.
- Rain sequence `A2b_rainseq` (-capture-set rain, s 720, rho 1, clock running): outside-streak change max 3 levels (H5 <= 6),
  streak mask 1.36 %; sequences.py FAIL "diff spikes" frames 50-57: a traffic streak passing above the camera plane projected as a
  screen-wide cyan line (its end point crossed w = 0). Fix: exact projection of both ends with the half-length clamped so both ends
  keep w >= 0.25 w_centre, lights with w < 0.1 culled (NocStreak; the same clamp in NocNeon's size inflation). Build05.
- The symmetric screen-space extension still ran a streak of a light just in front of the camera plane through the vanishing point
  (A2b_rainseq2, frames 48-61: luma step 1.38 levels, diff spikes). Fix: the quad now spans exactly the projected (clamped)
  segment between its two ends and only grows symmetrically to the 2.2 px minimum (NocStreak). Build06.
- Rain sequence `A2b_rainseq3` (build06): outside-streak change max 3 levels (H5 <= 6), p99.9 1.00, streak mask 0.89 %;
  `python -I Tools/qa/sequences.py QA/captures/A2b_rainseq3` PASS (luma step <= 1.2, no diff spikes). [NocProbe] renderingMode
  ForwardPlus (read from the renderer; the global keyword is not set at record time).

### Step 3 (GREEN: see the commit "A2b step 3")
Open for the lead / later builders: rain shelters (gate tunnels, tubes: no tunnel exists yet); lanes fade out east of x_B in the
air until the Gulf face builder adds the portals; city towers behind the Gulf faces show until GulfFace exists; NocNeon glyphs are
hashed scribbles; NocRain / NocFacade cost not measured (perf log), no 4K captures yet; the facades read desaturated grey-violet
at mid distance (energy-correct average of the cold window hues inside magenta fog: an art-direction call for the critic panel).
