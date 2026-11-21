# Bulwark framework

What the import step (MASTER_PROMPT Part 13 phase 2, Part 3.1) put into this project, where every piece came from,
what was renamed, how the frame runs, how to add a beat, and every command. Sources (read-only; never modified, never
run): **T** = Thread `<WORKSPACE>\3rd Megastructures\3rd Megastructures`, **A** = Abyss
`<WORKSPACE>\1st Megastructures\1st Megastructures`, **R** = The Ring
`<WORKSPACE>\2nd Megastructures\2nd Megastructures`. The copy list is `docs/IMPORT_MANIFEST.md` (S1-S13)
with the decisions D-01..D-10 of `DEVELOPMENT.md`. No game content was copied (no beats, constants, palettes, key-shot
tables, audio mixes or game shaders), no `.meta` files, nothing from Library, Builds, QA, Logs or .git.

Rules that hold everywhere: namespace `Megastructure` (sub-namespaces by folder: `Megastructure.Rendering`,
`.Player`, `.UI`, `.QA`, `.Audio`, `.EditorTools`); prefix `Bw` (`Thread*`/`Th*`/`Ab*`/`Abyss*`/`Ring*` -> `Bw*`,
shader globals `_Bw*`, shaders `Megastructure/Bw*`); log tags `[BW]` and `[BW-FAIL]` (D-06, the log-scan key);
product "Bulwark", company "Megastructures"; Direct3D 11 only.

**Wave A1 (user redesign 2026-11-19).** The kraken and the new ending follow `docs/ADDENDUM_A1.md` (it wins over MASTER_PROMPT
where they conflict). The ONLY external assets are the user's two Tripo3D sources in `3D Models/` (`SM_Creature_Octopus.glb`,
`SM_Creature_MonsterTentacles.glb`): read-only, outside `Assets/` (Unity never imports them); `Tools/kraken/**` regenerates
the game assets from them into `Assets/_Game/Creature/**` (kraken owner; DEVELOPMENT.md row 2026-11-19 Wave A1). Everything
else stays procedural.

**Owned by the path builder, not by the import** (the framework only calls them): `Runtime/Core/BulwarkConst.cs`
(14.12 constants, the key-shot table), `Runtime/Core/BulwarkPalette.cs` (14.2.1), `Runtime/Core/BwPath.cs` +
`FilletPath.cs` (the 1500 m filleted path), `Shaders/Include/BulwarkPalette.hlsl`, `Tests/EditMode/PathTests.cs`,
`PaletteTests.cs`, `Tools/path_check.py`.

## 1. What came from where

Paths are under `Assets/_Game/Scripts/` unless they start with `Assets/`, `Tools/` or `docs/`.

### S1 assemblies
| File | Source | Notes |
|---|---|---|
| `Runtime/Megastructure.Runtime.asmdef`, `Editor/Megastructure.Editor.asmdef`, `Tests/EditMode/Megastructure.Tests.EditMode.asmdef`, `Tests/PlayMode/Megastructure.Tests.PlayMode.asmdef` | T | unchanged |
| `Assets/_Game/Beats/Megastructure.Beats.asmref` | T | compiles every beat into Runtime (required by `GameBootstrap.DiscoverBuilders`) |

### S2 core (`Runtime/Core/`)
| File | Source | Notes |
|---|---|---|
| `FrameworkConst.cs` | new (T `ThreadConstants` framework part) | framework numbers; every 14.12 value is an **alias** of `BulwarkConst` (`EyeHeight = BulwarkConst.Eye`, `BeatBoundaries = BulwarkConst.BeatS`, ...); own: `BeatNames`, gaze limits, dither tile/sigma, UI timings 14.11, QA times, `QaFrameCap` 60, `WallBotInset` 0.40 |
| `BwMath.cs` | T `ThreadMath` (D3, easing) + R float helpers | `D3`, smootherstep, `WrapDeg`/`DeltaDeg`, PCG hash, value noise, `CriticalDamp`, `Slew` |
| `BwClock.cs` | T `ThreadClock` + A clock rules | ambient clock: `Frozen`, `Stepped`, `FreezeOnPause`, `FreezeDither` (static by default, D-05), `-clock`, publishes `_BwTime` |
| `GameState.cs`, `BwEvents.cs`, `BwServices.cs` | T + R (`IFadeUI`, audio hooks) | `FootSurface` {Concrete, WetConcrete, Road, Grating, SteelStair, Ledge}; `IPlayerState`, `ICameraControl`, `IAudioHooks`, `IFadeUI` |
| `ShaderIds.cs`, `MaterialLibrary.cs` | T | `_Bw*` globals; library fields Concrete, ConcreteWet, ConcreteDark, Road, Steel, SteelOchre, Grating, Armour, Rail (0.10 m), Handrail (0.05 m), Beam, LampSodium, LampTungsten, Lantern, Grade (Wave A materials; rows in `docs/MATERIALS.md`) |
| `WaveImpacts.cs` | new (lead contract) | the allocation-free impact bus: one writer (`SeaState`, sea owner: Publish / Expire / Clear), readers the mist feature (wall sprays 8 x 12 x 8 m, 0.6 s in, 2.4 s out), beats (lamp sway, dust); times on the ambient clock; the sea owner also calls `BwServices.Audio.Impact(strength, pan, subOnly)` |
| `SeaDisturbances.cs` | new (lead contract, Wave A1) | the kraken -> sea / mist / audio bus: one writer per frame (the kraken choreography: `Begin`, `Set` per disturbance, `Publish`; up to 8 of kinds Shadow, Breach, TentacleRise, Slap, Wake, world x/z, half extents, heading, strength, height above the sea); globals `_BwSeaDistCount`, `_BwSeaDist0[8]`, `_BwSeaDist1[8]`; readers BwSea, the mist feature, audio. QA writers before the kraken exists: `-sea-dist-qa near\|far` (sea owner, `Beat02_Crest/SeaDisturbanceQa`) and `-mist-qa-sea static\|cycle` (mist owner, `MistVolumeQa`): never two of them in one run |
| `WorldContext.cs` | T | `FrameState`, `IWorldUpdater`, `IBeatBuilder`, `KeyShotSubject`, `IKeyShotSubjects`, `PrewarmSet`, `AddMesh` with `MeshValidation` |
| `CliArgs.cs` | T + Bulwark flags | `-dither static\|temporal\|off`, `-dither-lsb`, `-clock`, `-freeze-clock`, `-uifont`, `-capture-only`, `-capture-4k`, `-report-dir`, `-audio-dump-seconds`, `-tier-override`, `-uncapped` (with `-perf-log` only: measure uncapped; every other run stays at the 60 fps cap), `-atmo-s S` (QA: the schedule at S whatever the camera s) (full list in the file header) |
| `BwCurvature.cs` | A (curvature drop) | `DropD`, `HorizonDipD`, Earth R 6371000 (H4) |
| `PaletteLookup.cs` | new | palette token by name (case/underscore-insensitive) over `BulwarkPalette.Table` |

### S3 mesh (`Runtime/Mesh/`)
`MeshBuilder`, `MeshKit`, `MeshKit.Loft`, `MeshKit.Revolve`, `MeshKit.Structures` (helical annulus, Warren truss; the
hex generators were Abyss content and were dropped), `MeshValidation`, `VertexBake` from **A**; `ShaderMeshes.cs`
(`BeamWedgeMesh`) from **A** `BeamWedge`.

### S4 rendering and shaders
| File | Source | Notes |
|---|---|---|
| `Runtime/Rendering/BwGradeFeature.cs`, `BwGradePass.cs`, `BwGradeMath.cs`, `GradeState.cs` | T `ThreadGrade*` | D-10 matrix `M = S·W` (von Kries LMS white balance with white luminance kept, saturation), 0.18-pivot contrast with toe protection, toe-less Khronos Neutral, RGBA16F bloom pyramid (clamp 40), vignette x fade, static blue-noise TPDF dither (D-05); fade and pause dim live in the grade |
| `BlueNoise.cs`, `R2Sequence.cs` | T | 64 x 64 void-and-cluster tile `BwBlueNoise64`, offset 0 when static |
| `BackdropRig.cs`, `InstancedBatch.cs`, `InstancedRegistry.cs`, `InstancedRegistryDriver.cs`, `DetailTier.cs` | A | `FollowAll`, chunks of 511 with explicit bounds, `SubmitAllFor(cam)` |
| `InstancedField.cs`, `InstancedFieldDriver.cs`, `InstancedFieldHandle.cs`, `TerrainIndirectField.cs` | R | GPU placement (`RenderMeshIndirect`), explicit world bounds, `SubmitAllFor(cam)` |
| `Assets/_Game/Shaders/BwGrade.shader` | T `ThreadGrade.shader` | matrix -> contrast -> bloom -> Neutral -> vignette x fade -> dither; mirrored by `BwGradeMath.ShaderOrder` |
| `BwLine.shader` | T | thin lines: min width max(1.5, 1.75 H/1080) px, alpha floor 0.35, alpha-to-coverage, tier fades (D-08) |
| `BwConcrete.shader` | R `RingLit` template | bend removed, instancing added; the `BwLitPass` hooks |
| `BwBeam.shader` | A `BeamWedge.shader` | analytic shafts (recipes 0 and 2) |
| `Shaders/Include/BwCommon.hlsl`, `BwLitCore.hlsl`, `BwLitPass.hlsl`, `BwLighting.hlsl` | R + T | hashes, filtered grids, `_BwBlueNoise`, `BwTierClip`; the lit pass (details below) |
| `Shaders/Include/BulwarkAtmosphere.hlsl` | T fog + A curvature (dusk owner) | public, signature-stable: `BwSkyColor`, `BwFogApply` (exact sphere haze + storm slab), `BwHorizon`, `BwDrop`, `BwHorizonDip`, `BwSunBreak`, `BwAmbientAt` (region ambient: exterior trilight / interior lamp colour x 0.06 by the interior boxes `_BwIntBox`, `BwInteriorWeight`), `BwSunDisc`, `BwSkyVisible` (sea reflections), `BwSeaSurface`; globals `_BwSea0` (far-sea body, slope variance), `_BwSeaState` (Hs, Tp), `_BwFlow` (wind / flow offsets from the unwrapped clock), `_BwLampBoost` (beat 5 portal boost) |
| `Shaders/BwSteel`, `BwRoad`, `BwGrating`, `BwArmour`, `BwEmissive`, `BwLantern`, `BwFarField` | new (Wave A) | the PBR library (`docs/MATERIALS.md`) and the far field (H4); `Beats/World_Sky/WorldSky.cs` builds the camera-following far-field dome (the graybox sky is skipped while it exists) |
| `Runtime/Rendering/BwTexGen.cs` | new (Wave A) | the one texture generator (fBm, Worley, curl, domain warp; Concrete 2048 / 2 m, Road, Steel, Crust 1024), Toksvig mips (`BuildMips`, `MipChainFromLevel0`), shader twins for the tests |
| `Runtime/Rendering/LocalLightPool.cs` | new (Wave B integrate) | the at most 4 real local lights (14.2.3): beats register lamp anchors, the pool picks the nearest in view and fades by distance, rank and time (section 4.0c); it also owns the up to 4 sky OPENINGS (doors, portals, arches; the only writer of `_BwOpen`) that `BwOpeningLight` turns into exterior light on the interior side |
| `Shaders/Include/BwLighting.hlsl` region ambient (Wave B) | integrate | `BwRegionAmbient(n, p, nGeo)` = `BwAmbientAt` at `BwAmbientProbePos` (one soft edge along the geometric normal, refined into the shrunk interior box by `BwInteriorProbe`, so floors, walls and their junctions take the full interior ambient) + interior weight x `BwOpeningLight(n, p)` (Lambert's polygon formula per opening, twin `LocalLightPool.OpeningFactor`); `BwEnvironment(R, N, roughness, p)` uses the same region terms for the reflection; used by `BwLitPass` (every lit Bw* shader) and `BwLine` |
| `Shaders/Generated/BulwarkConstGen.hlsl` | generated | `TC_*` macros from `BulwarkConst` (`ShaderGen` / `Tools/gen_hlsl.py`) |

### S5 systems (`Runtime/Systems/`)
| File | Source | Notes |
|---|---|---|
| `GameBootstrap.cs` | T + A `WarmupLoader` + R `CinematicOnLoad` | skeleton with an empty beat list; reflection discovery sorted by `Name`; per-builder try/catch; 14.6 set pre-warm; `LoadSeconds` < 15 s |
| `ProgressDirector.cs` | T | skeleton: s -> `AtmosphereDriver.Tick` -> `FrameState` -> updaters; per-camera `_BwScreen` |
| `BeatDirector.cs` | T | beat entry with 30 m back-hysteresis, goal at s >= 1498.5 once |
| `AtmosphereDriver.cs`, `BulwarkSchedule.cs` | new (A/T pattern) | the single writer of light, fog, sky, grade and vFOV globals (14.2.2); `BulwarkSchedule` is a pure function of s; slews EV 0.30/s, WB 6/s, bloom 0.1/s, vFOV 1 deg/s; bypassed in capture stills and teleports > 20 m |
| `WarmupLoader.cs` | A | every material drawn behind black in three lighting states |

### S6 player (`Runtime/Player/`)
| File | Source | Notes |
|---|---|---|
| `BwInput.cs` | T `ThreadInput` | |
| `PlayerController.cs`, `PlayerTuning.cs`, `QualityBootstrap.cs` | T | on `BwPath` lane coordinates, analytic floor, `EyeY`; CharacterController check (rejections > 1 cm projected back); safety snap (layer 31); `StopRamp` to s 1500; 60 fps cap for QA runs |
| `CameraRig.cs` | T + R probe | camera probe (violations, penetration, roll); `SetPoseOverride` for captures |
| `AutoWalker.cs` | T shell (D-02) | `StartCinematic` (2.5 m/s), `StartTest(speed)` |
| `WalkPlanner.cs`, `GazeSchedule.cs` | R (trimmed, D-02) | 0.5 s smootherstep ramp (loses 0.25 s), critically damped gaze with lead, rate limits 18/13 deg/s, sway <= 0.3 deg; Bulwark gaze keys of 14.5 |
| `PathGuide.cs`, `CorridorKit.cs` | T pattern + lesson 3.2 #11 | colliders generated from `BwPath.Lane` (4 m segments, 1 m overlap, walls 1 x 6 m, slabs under the analytic floor, safety triggers layer 31), coverage check; obstacles for props inside the lane (`AddObstacle(Bounds, name)`, `ObstacleAt`, `ObstaclesOnCinematicPath`, section 4.0b) |

### S7 UI (`Runtime/UI/`)
`UiDirector.cs`, `UiTimeline.cs` from **R** (UI Toolkit; the only fade and pause owner; 14.11 typography and timings;
`SetCaptureHidden`).

### S8 QA (`Runtime/QA/`)
| File | Source | Notes |
|---|---|---|
| `QaJson`, `QaReport`, `QaCommon`, `QaRunnerBase`, `CaptureWriter`, `PerfMonitor` | T | report env: frame cap, dither mode and peak; PerfMonitor uncaps only with `-perf-log -uncapped` (`QualityBootstrap.UncappedPerf`) |
| `AutoTestRunner`, `WallBot`, `SoakRunner` | T (rewritten for 14.10.4) | trigger times, progress speed, roll, CC rejections, safety snaps; both lane edges 0.40 m inside; obstacles: the autotest fails when one touches the cinematic path (`cinematicPathClearOfObstacles`), the wall bot steers round them and reports kind `obstacle` |
| `CaptureHarness`, `ShotPlan`, `QaProjection` | T (adapted) | capture rule C-12 (`SubmitAllFor` before every `SubmitRenderRequest`), `BackdropRig.FollowAll`, sets keyshots/beats/parity/sequences/encode/materials/matcontext, `pose:` tokens (s from the eye position over the whole path, `ShotPlan.PoseS`); report.json: `lumaRange` (fraction) and `lumaRangeLevels`, `lumaOk` compared in levels, `sun` [az, el] of the schedule at the shot |
| `QaRigs` | new (Wave B integrate, for Wave A1) | capture sets of off-path QA rigs (e.g. `kraken` from `Beats/QA_KrakenRig`): `Requested(set)`, `Register(set, stills, (w, h) => sequences)`, expanded by `ShotPlan.Build`, never part of stills / all / everything; `CurrentShot` / `CurrentFrame` published by the harness per still and sequence frame (section 4.0d) |
| `CompositionLint` | A | framing of `IKeyShotSubjects` subjects (11.13); occlusion is the EditMode audit's (no depth readback in the harness) |
| `ShotPlan` / `CaptureHarness` ending gate | fix wave X2 (lead QA) | `seq_end` starts at `EndingPath.SGoal - 3` and keeps the whole `EndTotal` (RealEnd: the real walker driven by a `WalkPlanner`, the after-goal sun sink fed `UiDirector.EndingTime`); per frame `tg`, `stinger`, `sunEl`, the stinger waypoints projected (`stingerInFrame`, `stingerAboveCrest`, `stingerTop`, `stingerContrast`); errors: goal never fired, stinger columns posed late (after the crest enters the frame), crest dark not at t_g 19.0 +- 0.5, stinger above the crest in the KS29 frames but < 6 levels of contrast at its top. Sets `ride` (RealWalk: `seq_ride_depart`, `seq_ride_arrive`, `seq_shuttle` 60 fps, `seq_turnback`; keyed to `EndingPath`), `kseq` (`seq_k1`..`seq_k7`, WalkGaze), `tprec` (T-PREC: static, 0.1 px/frame pan, cabin); transitions walk with `SeqMode.WalkGaze` (the AutoWalker look). One goal per run: posed frames feed progress <= `SGoal - 0.05` (`CaptureHarness.PoseProgressS`), and with `edges` the turn-back stops before the goal. Ending rig stills get 16:10 / 21:9 variants (`ShotPlan.EndingVariants`). |
| `PerfPoseRunner` | new (fix wave X2) | `-perf-poses KS17,ride,KS28` (+ `-perf-log <csv> -uncapped`, `-perf-rt 3840x2160`): 2 s warm + 6 s per pose, frame / CPU / GPU ms -> `perf_poses.json` |
| `VisibilityAudit` (+ `Tests/EditMode/VisibilityAuditTests`) | new (Wave A graybox) | the 11.13 audit: every key shot and named subject at 16:9 / 16:10 / 21:9 with the 3 % margin, px at 1080p / 4K, occlusion by ray casts against the built world plus the analytic floor rule, the AutoWalker follower pose, the reachability sweep; writes `QA/reports/visibility.json`; expected px numbers from `BulwarkConst` (the KS*4K constants with the 14.10.5 rulings) |

### S9 audio (`Runtime/Audio/`)
`BwDsp.cs` (T blocks + R `DspLimiter` -1.42 dBFS, FDN with explicit delays and blend time, brown noise, step-event ring),
`BwSynth.cs` (generator on a clip-less AudioSource), `AudioDirector.cs` (48 kHz rig, pause duck, `-audio-dump` WAV +
(t, s) sidecar), `FootstepBank.cs` (6 surfaces x 6 variants, modal), `OfflineAudioRenderer.cs` (T-AUDIO renders at
1.5 and 2.5 m/s, sidecar with `sPerSecond`), `WavWriter.cs`, `BwAudioSpec.cs`, `BulwarkScore.cs` (skeleton: the score
of 14.3 is game content to be written) from **T** with **R** pieces.

### S10 editor tools (`Editor/`)
`EditorCommon.cs` (`BwPaths`, `BwLog` with an injectable `ErrorSink`, `EditorUtil.ResetToDefaults`), `BatchTools.cs`,
`MaterialFactory.cs`, `TextureFactory.cs` (Wave A/B, below), `ProjectConfigurator.cs`, `PipelineConfigurator.cs`,
`SceneBuilder.cs`, `PaletteWriter.cs`, `ShaderGen.cs`, `CaptureWatchdog.cs` (`BULWARK_EXIT_CODE`), `TemplateCleanup.cs` from
**T**. `BatchTools.PlayerExePath()` honours `BW_BUILD_DIR` (lesson 3.2 #10); `Validate` checks D3D11 only, shadows 150 m,
D32_S8 depth, no skybox, no SSAO, no Volume overrides, the PanelSettings asset, Always Included `Megastructure/Bw*` shaders,
every material (lesson 3.2 #4) and exactly one writer of `_BwSurface` (AtmosphereDriver; `BatchTools.GlobalWriters`; the Wave A
QA stand-in writer of `QA_MaterialSheet` is gone, `MaterialTests.MaterialSheets_LeaveTheSurfaceStateToTheDriverAndPoolTheBoothLamps`
runs the same scan in EditMode).
SceneBuilder updates Main.unity in place and resets every reused MonoBehaviour to its script defaults before it sets the
contract references, so an in-place RebuildAll equals a from-scratch one (stable hash). TextureFactory writes the
BwTexGen sets as level-0 PNGs (`Assets/_Game/Textures/Bw<Set>_<Albedo|Normal|Mask>.png`); `BwTexturePostprocessor` sets
the importer in code (sRGB albedo only, mipmaps, repeat, trilinear, aniso 8, not readable, Standalone BC7 / BC5, the Crust
mask RGBA32) and replaces the importer's mips by `BwTexGen.MipChainFromLevel0` (Toksvig smoothness); regenerated only when
`BwTexGen.Version` or the manifest changes.

### S11 tests (`Tests/`)
EditMode: `GradeTests`, `ProjectTests`, `QaTests`, `AudioTests` (T, adapted), `WalkerTests` (new, D-02 / 14.10.4; PathGuide
obstacles), `MaterialTests` (library, generator, twins), `AtmosphereTests`, `FarFieldTests` (dusk owner),
`VisibilityAuditTests` (11.13 audit, graybox capsule clearance, ReplacesGraybox keys, LocalLightPool continuity).
PlayMode: `InputTests` (T). `PathTests` and `PaletteTests` belong to the path builder.

### S12 tools and root files
| File | Source | Notes |
|---|---|---|
| `Tools/unity-queue.ps1` | T | mutex `Global\BulwarkUnityQueue_4Megastructures`; exact `-projectPath` match; `[BW]`/`[BW-FAIL]` greps; stall 15 min (0 for `-runTests`); `-BuildDir` -> `BW_BUILD_DIR` |
| `Tools/run-player.ps1` | T | `Builds\Bulwark\Bulwark.exe` or `-Exe`/`-BuildDir`/`BW_BUILD_DIR`; `-PlayerArgs` (comma split) and `-ArgString` (one string); long and 4K jobs through `gpu-guard.ps1`; waits for other renderers of ours; reads the QA JSON `pass`; nvidia-smi VRAM log in `QA/vram/` |
| `Tools/lookdev.ps1` | A | build into `Builds/lookdev_<slot>`, capture through `run-player.ps1`, metrics + contact sheet |
| `Tools/fastcompile.py` | T + R `fastcheck` + A per-run folders | see section 5 |
| `Tools/gen_hlsl.py` | T | palette and constants HLSL, byte-identical to `ShaderGen`; `--check` |
| `Tools/metrics.py`, `Tools/pricing.json` | T | `--anchor` (STATE.md line 1), session search in all project folders, exact exclusion of `W--YouTube-Megastructures`; pricing is a template (re-fetch at the end, 15.3.4) |
| `Tools/qa/*.py`, `regions_default.json`, `run_encode_clips.sh` | T (+ A encode merges) | 14.10.3 thresholds; see sections 5 and 6. Wave B: `sequences.py` compensates pans by the exact rotation homography and gates 2-4 px patterns on the low-pass / block-mean std; `image_metrics.py` trend-removed horizon steps, no beat band for `mat_` sheets, the beat-7 level free-look rule; `regions_default.json` holds the KS04 / KS05 / KS08 / KS20 banding regions, horizon pairs and sun discs; `qa_common.Spec` reads the key-shot table from `BulwarkConst.cs` including named levels (`L(KS03LumaMin)`) |
| `Tools/gpu-guard.ps1` | machine copy | present before the import |
| `Tools/kraken/**` | new (kraken owner, Wave A1) | the kraken asset pipeline from the read-only `3D Models/*.glb` sources into `Assets/_Game/Creature/**` |
| `PLAY_WINDOWS.bat`, `.gitignore` | T | `.gitignore` kept, plus `/Assets/Resources.meta` and `/Assets/Resources/` |

### S13 docs
`docs/BUILDER_BRIEF.md`, `docs/CRITIC_BRIEF.md` (templates from **R**), this file.

## 2. Renamed classes and identifiers
| Source name | Bulwark name |
|---|---|
| `ThreadConstants` (framework part) | `FrameworkConst` (aliases `BulwarkConst`) |
| `ThreadMath`, `RingMath` helpers | `BwMath` |
| `ThreadClock` / Abyss `SceneClock`, `ClockDriver` | `BwClock` |
| `ThreadEvents` / `RingEvents` | `BwEvents` |
| `ThreadInput` | `BwInput` |
| `ThreadGrade`, `ThreadGradeFeature`, `ThreadGradePass`, `ThreadGradeMath` | `BwGrade` (shader), `BwGradeFeature`, `BwGradePass`, `BwGradeMath` |
| `ThreadBlueNoise` texture | `BwBlueNoise64` |
| `ThreadLine`, `ThreadFlat`/`RingLit` | `BwLine`, `BwConcrete` (+ `BwLitCore`/`BwLitPass`) |
| Abyss `BeamWedge` | `BwBeam` + `BeamWedgeMesh` |
| `ThreadFog` / `RingAtmosphere` | `BulwarkAtmosphere.hlsl` (`BwFogApply`) |
| `ThreadCommon.hlsl` / `RingCommon.hlsl` | `BwCommon.hlsl` |
| `ThreadPalette` | `BulwarkPalette` (path builder) + `PaletteLookup` |
| `ThreadPaths`, `ThreadLog` | `BwPaths`, `BwLog` |
| `ThreadDsp`, `RingSynth` | `BwDsp`, `BwSynth` |
| `ThreadScore` | `BulwarkScore` |
| `_Th*`, `_Ab*`, `_Ring*` shader globals | `_Bw*` (`ShaderIds`) |
| `[THREAD]`, `[THREAD-FAIL]`, `[ABYSS]`, `[ABYSS-ERROR]`, `[Ring]` | `[BW]`, `[BW-FAIL]` |
| `THREAD_EXIT_CODE` | `BULWARK_EXIT_CODE` |
| `ThreadConstantsGen.hlsl` | `Generated/BulwarkConstGen.hlsl` (`TC_*` macros, guard `BULWARK_CONSTANTS_INCLUDED`) |

## 3. Execution order (one frame)
`[DefaultExecutionOrder]` values, lowest first:

| Order | Component | Job |
|---|---|---|
| -1000 | `GameBootstrap` | load (coroutine): discover and run builders, pre-warm, warm-up, `Ready` |
| -900 | `QualityBootstrap` | frame cap 60 (vSync 0) for QA runs and `-perf-log`; uncapped only for `-perf-log -uncapped`, never the soak |
| -800 | `UiDirector` | title, hint, pause, ending; the fade (`GradeState.FadeBlack`) and pause dim |
| -50 | `CameraRig`, `AutoTestRunner`, `WallBot`, `SoakRunner` | camera probe and pose override; QA runners drive input |
| -40 | `AutoWalker` | cinematic / test walk: speed, lateral, gaze commands |
| -30 | `PlayerController` | move in lane coordinates on `BwPath`, CharacterController check, eye height |
| -20 | `PathGuide` | safety snaps, coverage |
| 150 | `BackdropRig` | far rigs follow the camera |
| 200 | `InstancedRegistryDriver` | submits `InstancedBatch` draws (not in capture mode) |
| 500 | `ProgressDirector` (+ `AtmosphereDriver.Tick`) | s -> schedule -> globals -> `FrameState` -> every `IWorldUpdater.OnFrame` (registration order; the `LocalLightPool` is one of them) |
| 600 | `BeatDirector` | beat entry events, goal trigger |
| 700 | `AudioDirector` | publishes `BwAudioParams` to the audio thread |
| 32000 | `CaptureHarness`, `InstancedFieldDriver` | captures after everything moved; GPU fields |
| 32100 | `PerfMonitor` | frame times |

Per camera: `ProgressDirector.OnBeginCamera` sets `_BwScreen` and the blue-noise tile; `AtmosphereDriver.SetBwCam`
sets the per-camera atmosphere globals. The grade (`BwGradeFeature`) runs after rendering: bloom, matrix, contrast,
Neutral, vignette x fade, dither. In capture mode the harness calls `InstancedRegistry.SubmitAllFor(cam)` and
`InstancedField.SubmitAllFor(cam)` right before each `SubmitRenderRequest` (C-12).

## 4. How to add a beat
0. **Graybox replacement (`[ReplacesGraybox]`).** The graybox `Assets/_Game/Beats/Graybox/GrayboxPath.cs` (builder
   `Graybox_Path`, integrate owner) holds clean volumes for the whole path. A beat builder does NOT edit it: once its own
   replacement renders it puts `[ReplacesGraybox("beatN")]` (one attribute per key, `AllowMultiple`) on its builder class,
   and the graybox then builds none of that key's pieces (no surface drawn twice, no z-fighting). Keys and the pieces they
   remove (fixed pieces of s < 934 lie on the straight crest, where z = s):

   | Key | Graybox pieces it removes |
   |---|---|
   | `beat1` | the gallery: shell with slits, baffles, niches, valve wheels, the 9 lamps and their anchors, the door leaves, the deck under the shell (x -4.1..1.1, z -0.6..120.7, the 14.4 "road deck cut") |
   | `beat2` | the 14.5.2 road mesh: deck, sea face, parapet, lee face, lee rail, lamp posts, bollards for z < 420 (beside the shell too) and z >= 956 (north of the tower); the cart (and its PathGuide obstacle); the far gate towers |
   | `beat3` | the spillway bridge z 420-704: its deck, piers, slots, sill, gates, draw-down water, the lee and sea-side rails over it, the plume cone |
   | `beat4` | the crest z 704-920 (deck, faces, parapet, lee rail, lamp posts, bollards), the lee face below it, the surf strip and the six islets, the tower shell (14.5.4 TowerBuilder) |
   | `beat5` | the tower interior z 920-956 (atrium floor and ceiling, well wall, pendants and their anchors, guard rail) and every lane-driven piece of s 934-1070 (stairs, handrails, landing lamps, exit hall and its lamps) |
   | `beat6` | lane-driven s 1070-1370: gallery floor, walls, ceiling, trench, rail, panes, bollard lamps, the doorway wall at s 1070 |
   | `beat7` | lane-driven s 1370-1500: tunnel, conduit quad, tunnel lamps, the hall end wall at s 1373; the ledge slab, its rails, the lantern and its light, the lee face north of z 920 with the mouth hole |
   | `sea` | the sea plane (x 6..4500); the spillway water stays with `beat3` |
   | `sky` | the stand-in sky sphere (never built while `Beats/World_Sky/WorldSky` exists) |
   | `lights` | every graybox lamp anchor (the beats then register their own lamps in the `LocalLightPool`) |

   The graybox groups each key's meshes under `Graybox_Path/gb_part_<key>`; the build log prints the replaced keys
   (`graybox: replaced parts [...]`). The corridor colliders are not touched (PathGuide builds them from `BwPath.Lane`).
   Proof: `VisibilityAuditTests.Graybox_HonoursTheReplacesGrayboxKeys` (each key removes exactly its pieces, every other
   piece stays bit-identical, ray probes through the beat find no graybox surface). Add the attribute only when your
   replacement covers the whole key; until then you see your geometry overlap the graybox.
0b. **Obstacles (props inside the walkable lane).** `PathGuide.AddObstacle(Bounds worldBox, string name)` in your `Build`
   makes a corridor-layer `BoxCollider` (axis-aligned; re-adding a name replaces it, `RemoveObstacle(name)` drops it; works
   before and after the corridor exists). The CharacterController then stops a manual player at it (one logged warning,
   no snap). Rule: an obstacle must not touch the cinematic capsule path (`BwPath.CinematicLateral`): the autotest fails
   on it (`cinematicPathClearOfObstacles`, and the walk's CharacterController rejections stay 0). Elsewhere contacts are
   allowed: the wall bot steers round obstacles inside the lane and reports kind `obstacle` (never a lane leak or a
   rejection failure). `PathGuide.ObstacleAt(s, lateral, margin, out obstacle)` is the analytic capsule test. Use it only for
   props the spec puts inside the lane (the cart); keep everything else outside lane edge + 0.35 m.
0c. **Real local lights (`LocalLightPool`, 14.2.3: at most 4).** Never create `Light` components in a beat. Register lamp
   anchors once in `Build`: `var pool = LocalLightPool.Ensure(ctx); int id = pool.Add(pos, BulwarkPalette.Linear(Pal.SodiumWarm),
   range, intensity, "beat1");` (`AddSpot(pos, rotation, coneDeg, colour, range, intensity)` for a spot; optional `rank` < 1
   = picked first; `lampBoost: true` = the light follows the 14.2.5 portal lamp boost 2^(1.3 (1 - smootherstep((s - 920) / 40)))
   like the atrium lamps' emission). The pool owns the 4 lights and gives them to the nearest anchors in view (priority d / (range + 12) x
   a view penalty up to 1.6 outside the 21:9 cone), with a distance fade at the reach, a rank fade where the 5th anchor
   overtakes the 4th and a 0.5 s time fade per slot, so no pool ever pops (Part 10 #6); capture stills use the steady state
   (a pure function of the pose). Per frame, allocation-free: `pool.SetPosition(id, p)` (lamp sway), `pool.SetGain(id, g)`
   (relays; 0 = off). Proof: `VisibilityAuditTests.LightPool_IsContinuousAlongTheWalkAndUnderAWhipPan` and the player sequence
   `seq_lamps_b01` (luma step <= 1.2 levels per frame). Users: the graybox lamps (key `lights`), beat 7 (tunnel cages, lantern),
   the QA material-sheet booths (rank 0.5).
   Sky OPENINGS (Wave B): a beat whose interior opens onto the exterior through a rectangle in a z plane registers it once:
   `int o = pool.AddOpening("beat1_door", xMin, xMax, yMin, yMax, zPlane, exteriorAtPlusZ, reach)` and, per frame,
   `pool.SetOpeningX(o, xMin, xMax)` (a sliding door) and `pool.SetOpeningOn(o, gap > 0)`; the lit pass then adds the
   exterior radiance through it (form factor F, 0 on the exterior side and from 1 m below the sill, fade over the last 30 %
   of the reach) to the interior ambient of every lit surface within reach. The graybox registers the door (z 120), the
   tower portal and both arches (z 920 / 956, reach 40 m). An opening outside a z plane (the beat 7 mouth in the x plane of
   the lee face) uses its owner's term (`BwMouthLight`). Interior surfaces must lie INSIDE an interior box
   (`BulwarkSchedule.InteriorBoxes`, soft edge 0.75 m); a surface within 0.75 m of a box face whose normal does not point
   into the box takes part of the exterior ambient (Part 10 #36).
0d. **QA rigs (`QaRigs`, Wave A1).** An off-path look-dev rig (e.g. `Beats/QA_KrakenRig`) is an ordinary `IBeatBuilder` whose
   `Build` returns at once unless `QaRigs.Requested("kraken")` (a capture run whose `-capture-set` names the set); it then
   builds its rig away from the path and registers its shots:
   `QaRigs.Register("kraken", () => stills, (w, h) => sequences)` (`StillShot` / `SequenceSpec` of `ShotPlan`; the still's
   `s` drives the schedule through the ProgressDirector override, its `ambient` the ambient clock = the animation phase;
   sequences use `SeqMode` poses and `advanceAmbient`). `-capture-set kraken` then captures exactly those (with
   `-capture-only` filters; never part of stills / all / everything, because a rig changes the world). Per shot,
   `QaRigs.CurrentShot` (tag or sequence name) and `QaRigs.CurrentFrame` let the rig's `OnFrame` switch LOD, debug view or
   turntable angle. Rig-only flags are read with `CliArgs.Get("-kraken-...")` / `GetDouble` / `Has` (no CliArgs edit);
   document them in the rig's header and here. `run-player.ps1` treats a set name it does not know as a heavy 4K job.
1. Create `Assets/_Game/Beats/BeatNN_<Name>/` (the asmref compiles it into `Megastructure.Runtime`). `<Name>` is
   `FrameworkConst.BeatNames[NN-1]`; the folder is the builder's.
2. Add a class with a public parameterless constructor implementing `IBeatBuilder`:
   ```csharp
   namespace Megastructure.Beats
   {
       public sealed class Beat02Crest : IBeatBuilder, IWorldUpdater, IKeyShotSubjects
       {
           public string Name => "Beat02_Crest";               // builders run sorted by Name
           public void Build(WorldContext ctx)
           {
               var root = new GameObject(Name).transform; root.SetParent(ctx.Root, false);
               // geometry from BwPath (World, Lane, FloorY) and BulwarkConst; meshes via MeshBuilder/MeshKit
               ctx.AddMesh(root, "road", mesh, ctx.Materials.Concrete);   // validated, triangles counted
               ctx.Updaters.Add(this);                          // per-frame updates keyed to s
               ctx.SubjectProviders.Add(this);                  // key-shot subjects for CompositionLint (11.13)
               // ctx.PrewarmSets.Add(...) for sets that switch on later (14.6 pre-warm)
           }
           public void OnFrame(in FrameState f) { /* animate from f.s, f.ambientTime; never from Time.time */ }
           public void CollectSubjects(string shotId, List<KeyShotSubject> into) { /* KSnn subjects */ }
       }
   }
   ```
3. Everything is keyed to s (`BwPath`), positions from `BwPath.World`, eye and floor from `BwPath.EyeY`/`FloorY`, beat
   ranges from `BulwarkConst.BeatS`. Light, fog, sky, exposure and FOV are NOT set by a beat: they come from
   `BulwarkSchedule` through `AtmosphereDriver` (one writer, 14.2.2). Code-drawn instances go through
   `InstancedBatch`/`InstancedRegistry` or `InstancedField` (so captures re-submit them). Walkable surfaces need no
   colliders of their own: `PathGuide` builds the corridor from `BwPath.Lane`.
4. Materials: use the `MaterialLibrary` fields (section 1 S2; recipes in `docs/MATERIALS.md`). A new material of this wave
   is created at runtime by its owner (`Shader.Find("Megastructure/Bw...")` + properties) unless the integrate agent adds
   it to `MaterialFactory.Specs`; every new `Megastructure/Bw*` shader needs one `RebuildAll` (queue) to land in Always
   Included Shaders. Colours only from `BulwarkPalette` tokens. Lit surfaces use the lit template (`BwLitCore` + `BwLitPass`):
   implement `BwSurfaceHook(inout BwFragData f, inout SurfaceData s)` (`f.positionWS`, `normalWS`, `normalGeoWS`, `viewDist`,
   `footprint`, `screenUV`, `color` = vertex colour R AO / G wear / B band mask); optional hooks `BW_HAS_LIGHT_HOOK` +
   `void BwMainLightHook(inout half3 color, half3 dirWS)`, `BW_HAS_CLIP_HOOK`, `BW_ALPHA_OUT`, `BW_USE_NORMAL_TS`;
   `BwShadeEx` (the 3-argument `BwShade` stays); ambient is regional (`BwAmbientAt`), the reflection the analytic sky
   (`BwEnvironment(R, N, roughness, positionWS)`). Triplanar helpers in `BwLitCore` (`BwTriSetup`, `BwTriDetune`,
   `BwTriSample`, `BwTriNormal`, `BwRecolour`; `BwTri` keeps unwrapped coordinates and `BwWrap64` wraps at the fetch, so
   every detune k is seam-free); `BwCommon` noise (`BwValueNoise3`, `BwFbmFiltered3` for unthresholded macro fields,
   `BwGradNoise3`, `BwFbmGrad3` for thresholded masks, `BwMacroSigned`, `BwMacroSigned3`, `BwPxFade`, `BwBoxCover`,
   `BwPulseTri`) and the surface state `_BwSurface` (x rain wetness, y interior factor, z lamp gain; one writer, the
   AtmosphereDriver). Filter lines by the footprint ACROSS them (`abs(ddx(q)) + abs(ddy(q))` on their own axis), never by
   the isotropic distance footprint.
5. Tests: add EditMode tests for the beat's pure geometry (dimensions of 14.5 acceptance) next to the other tests.
6. Verify: `python -I Tools/fastcompile.py all`, a Unity build, `-capture-set beats` and the beat's key shots, look at
   them, `Tools/qa/image_metrics.py`, the sight-line evidence (11.13). See `docs/BUILDER_BRIEF.md`.

## 5. Commands (from the project root, PowerShell; quote the root, it has spaces)
**Fast type check** (no Unity; needs one prior Unity compile for `Assembly-CSharp-Editor.csproj`; blind to asmdef
references and shaders):
- `python -I Tools/fastcompile.py all` (also `runtime`, `editor`, `tests`; `--only <folders under Assets/_Game>`,
  `--extra <file.cs>`, `--keep`); output in `Tools/fastcompile/runs/<id>/` (`FC_RUN` or pid + time, deleted after the
  run unless `--keep`); Runtime is compiled with the player defines (no `UNITY_EDITOR`); package DLLs are filtered by
  each asmdef's references; the mscorlib compat shim is added when the csproj has no mscorlib.
- `python -I Tools/gen_hlsl.py` (palette + constants HLSL) and `python -I Tools/gen_hlsl.py --check` (byte-identical
  to `ShaderGen`).

**Unity batch** (always through the queue):
- `powershell -NoProfile -File Tools\unity-queue.ps1 -Name compile -Method Megastructure.EditorTools.BatchTools.CompileCheck`
- `... -Name rebuild -Method Megastructure.EditorTools.BatchTools.RebuildAll -TimeoutMin 30` (project settings, HLSL,
  materials, scene; idempotent)
- `... -Name validate -Method Megastructure.EditorTools.BatchTools.Validate`
- `... -Name build -Method Megastructure.EditorTools.BatchTools.BuildWindows [-BuildDir Builds\<owner>]`
- other methods: `ApplyBuildSettings`, `GenerateHlsl`, `RebuildMaterials`, `RebuildScene`, `CaptureInEditor`
- EditMode tests: `... -Name tests -NoQuit -ExtraArgs "-runTests,-testPlatform,EditMode,-testResults,<abs path>\QA\tests\editmode.xml"`
  (no `-quit` with `-runTests`; the stall watchdog is switched off automatically). Call it from a PowerShell session
  (`powershell -NoProfile -File ...`), never through `Start-Process -ArgumentList`: that splits the absolute results path
  at the spaces of the project path. PlayMode: the same with `-testPlatform,PlayMode` (2 of its 4 tests skip by design
  in batch mode).

**Player** (always through `run-player.ps1`; never `-batchmode`/`-nographics` for anything that renders):
- autotests (14.10.4): `powershell -NoProfile -File Tools\run-player.ps1 -Name autotest15 -PlayerArgs "-autotest"` and
  `-Name autotest25 -PlayerArgs "-autotest,-cinematic"` (long jobs: gpu-guard, no stall killer)
- wall bot: `-Name wallbot -PlayerArgs "-wallbot"`; soak: `-Name soak -PlayerArgs "-soak,1800"` (time limit raised to
  the soak length + 15 min)
- captures: `-Name keys -PlayerArgs "-capture,QA\captures\<run>,-capture-set,keyshots"`; comma lists in one string:
  `-ArgString "-capture QA\captures\<run> -capture-only KS04,KS05"`; 4K and sequences are routed through gpu-guard. The
  `keyshots` set (also the default set of `-capture`, and part of `stills`, `all`, `everything`) always includes the 4K
  variant of each shot, so run-player treats it as a 4K job (gpu-guard, 6500 MiB). Phase 2 proof:
  `-Name p2keys -ArgString "-capture QA\captures\p2_keys -capture-set keyshots -capture-only KS01,KS04,KS20"`
- performance: `-Name perf4k -PlayerArgs "-perf-log,QA\perf\perf4k.csv,-perf-rt,3840x2160,-uncapped"` (without `-uncapped` the
  perf log runs at the 60 fps QA cap)
- 14.10 acceptance sequences (in `statics` / `pans` / `transitions`): `seq_static_KS04` (sea flicker), `seq_plumes_KS04` (60 s
  at 4 fps, plume count), `seq_static_KS08` (plume flicker), `seq_static_KS20` (ledge flicker at s 1452), `seq_pan_KS04_sea`
  (sea beyond 1 km), `seq_pan_KS09_p58` / `seq_pan_KS09_p90` (grating), `seq_pan_KS20`, `seq_pan_s1470_city`, `seq_lamps_b01`
  (the light pool through the beat-1 lamps s 30-54); e.g. `-ArgString "-capture QA\captures\<run> -capture-set statics,pans -capture-only KS09,KS20"`
- QA overrides: `-atmo-s 1470` (schedule at s 1470 for a ledge framing), free poses `pose:NAME:x:y:z:yaw:pitch:fov`
- ending gate (fix wave X2): `-ArgString "-capture <dir> -capture-set edges -capture-only seq_end"` (1080p, about 2 min);
  `-capture-set ride -seq-res 960x540` (real ride, about 2 min of frames); `-capture-set tprec` then
  `python -I Tools/qa/tprec.py <dir>` (Lucas-Kanade tile shifts, limit 0.1 px; add `-dither off` and a near-origin control
  `-capture-set pans -capture-only seq_pan_b01,seq_pan_rail_slow`); perf: `-PlayerArgs "-perf-poses,KS17,ride,KS28"` does not
  work (commas): use `-ArgString "-perf-poses KS17,ride,KS28 -perf-log <csv> -uncapped [-perf-rt 3840x2160]"`; soak
  `-soak 1800 -speed 2.5` now rides and reaches the ending in its last loop
- QA rigs (section 4.0d): `-ArgString "-capture QA\captures\<run> -capture-set kraken"` once `QA_KrakenRig` registers the set
  (gpu-guard, 4K); the sea-disturbance QA writers (kraken stand-ins until the choreography exists, one per run):
  `-sea-dist-qa near|far` (BwSea foam, shadow, wake; sea owner) or `-mist-qa-sea static|cycle` (`-mist-qa-t0 T`; sprays;
  mist owner)
- audio dump: `-PlayerArgs "-cinematic,-audio-dump,QA\audio\dump.wav,-audio-dump-seconds,60"`
- look-dev loop: `powershell -NoProfile -File Tools\lookdev.ps1 -Slot <owner> -Run r01 -Shots keyshots`
- GPU state: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools\gpu-guard.ps1 -Status`

**Offline QA** (`python -I`, numpy + Pillow; ffmpeg on PATH for the encode test):
- `python -I Tools/qa/run_all.py QA/captures/<run> [--audio <wav|dir>] [--undithered QA/captures/<run>_nodither]`
- single tools: `image_metrics.py`, `contact_sheet.py`, `sequences.py`, `encode_test.py`, `audio_analysis.py`
- encode test end to end (captures both twins through gpu-guard, measures, deletes frames):
  `bash Tools/qa/run_encode_clips.sh`
- tool self-test (synthetic data, 92 assertions): `python -I Tools/qa/selftest.py` (`--no-encode`, `--no-4k`)

**Metrics** (Part 15.3): `python -I Tools/metrics.py [--md QA/metrics.md] [--json QA/metrics.json] [--list]`; the start
is the first line of `STATE.md` (`Run started at <ISO>`), `--anchor` overrides it.

## 6. Thresholds and conventions the tools encode
- 14.10.3 per-beat mean luma (levels) 28-70, 85-150, 95-150, 100-165, 28-80, 14-85, 20-90; clipped (any channel >= 254)
  at most 1.5, 0.4, 0.8, 0.6, 0.3, 0.1, 1.0 %; magenta 0; fireflies (luma > 8 x 5x5 median) 0; BandScore longest run
  <= 6 px at 1080p (12 at 4K), mean run <= 0.5/slope below 0.1 level/px; sequences: luma step <= 1.2 levels/frame (2.0
  in the sun break), activation diff <= 0.5, EV slope <= 0.30 EV/s, pan std <= 2.0 (1.5 beat 6 / ledge), flicker
  block std <= 3 (1.5 ledge); encode: dithered run <= 0.6 x undithered on the 4K x264 CRF 23 decode; audio: true
  peak <= -1 dBTP, sliding 3 s RMS within 1.5 dB of the `BulwarkConst.TargetRmsDb` targets.
- Capture tags: key shots `bNN_<Beat>_KSnn_<tag>[_WxH]`, beat stills `bNN_<Beat>_<pos>`, sequences `seq_<name>/`.
- QA runs cap the frame rate at 60 (2.5.3); only `-perf-log -uncapped` runs uncapped.
- Pan shimmer (Wave B): exact rotation-homography compensation; per 64 px block the raw temporal std, except where the
  bilinear interpolation floor (frame 0 warped forth and back) exceeds half the limit (2-4 px periods): there max(low-pass
  sigma 1.5 std, block-mean std). Horizon pairs (`"mode": "trend"`): lines fitted to rows h-7..h-2 and h+2..h+7 (1080p),
  extrapolated to h, Lab dE <= 2.0. Beat-7 level free looks on the ledge (pitch 0, s >= 1423) whose geometric sky fraction
  exceeds 0.5 report the mean luma as INFO (14.10.5 ruling); off-path material sheets (`mat_*`) carry no beat band; KS03 has
  its own band 70-120 (`BulwarkConst.KS03LumaMin/Max`).
