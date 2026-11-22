# IMPORT PLAN — framework import for Nocturne (Part 3.1, 3.2, 14.8 P0)

Researcher report, read-only audit of the three finished games, written 2026-11-13.
Roots used below (quote them, they contain spaces):

| Short | Absolute root |
|---|---|
| **T** (Thread, primary) | `W:\YouTube\Megastructures\3rd Megastructures\3rd Megastructures` (git HEAD f45fb67, clean tree) |
| **R** (The Ring) | `W:\YouTube\Megastructures\2nd Megastructures\2nd Megastructures` (HEAD e91b12a, clean) |
| **A** (Abyss) | `W:\YouTube\Megastructures\1st Megastructures\1st Megastructures` (HEAD 2d7ae96; only untracked QA thumbnails) |
| **N** (Nocturne) | `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures` |

`RT` = `Assets\_Game\Scripts\Runtime`, `ED` = `Assets\_Game\Scripts\Editor`, `SH` = `Assets\_Game\Shaders`, `TE` = `Assets\_Game\Scripts\Tests\EditMode`.

---

## 0. Summary (the decisions the lead should take)

1. **Backbone = Thread** (GameBootstrap, ProgressDirector, BeatDirector, `ThreadServices` interfaces, WorldContext/IBeatBuilder, the whole QA runtime, the grade stack, the editor tools, the audio DSP blocks). Every other file is adapted to talk to it.
2. **Player = hybrid, as Part 3.1 prescribes, glued by Thread's interfaces.** The Ring's `FilletPath`, `GazeSchedule`, `WalkPlanner`, `AutoWalker`, `PlayerController` (CharacterController r 0.35, h 1.8, as 14.4 demands) + Thread's `CameraRig`, `ThreadInput`, `QualityBootstrap`, `PlayerMath.StopRamp`. The Ring player implements Thread's `IPlayerState`, so Thread's AutoTestRunner / WallBot / SoakRunner / CaptureHarness work unchanged. A 100-line bridge `NocRig.cs` (trimmed `RingState`/`RingRig`/`RingEvents`) lets the Ring player and UI compile almost verbatim.
3. **PathGuide = Thread's generic collider builder** (segments derived from a half-width function, CoverageCheck, KillCount used by Thread QA by reflection) fed by a new pure `NocLanes` that exposes the Ring PathGuide API (`LaneHalfWidth`, `DistanceOutsideLanes`, `LaneSamplePoints`, `CentreAt`). Ring's PathGuide itself is 90 % Ring lane tables (bridge, ramp, pad) and is not copied (lesson 11: colliders from the walkable-width function).
4. **Geometry = Abyss MeshKit** (wins over Thread's and Ring's MeshKit); Thread's `WorldContext.AddMesh` must stop calling `MeshKit.TriangleCount` (Thread-only API).
5. **One namespace `Megastructure`** (flatten Thread's `Megastructure.Player/QA/Rendering/UI/Audio`; keep `Megastructure.EditorTools` because Part 7.1 names `Megastructure.EditorTools.BatchTools.RebuildAll`, and `Megastructure.Tests`). Rename only game-prefixed names (`Thread*`/`Ring*`/`Ab*` -> `Noc*`); unprefixed classes keep their names. Update every reflective type-name string (list in 4.9).
6. **Grade deltas (14.7 H1) are small**: Thread's grade already has saturation (it *is* the colour matrix `M = (1-k) 1 L^T + k I` applied in linear HDR before the tone curve), contrast and vignette. Change only: per-pixel dither amplitude `2.0 x lerp(1, 0.6, saturate((Y' - 0.35)/0.4))`, R2 offset frozen by default (`-dither static|temporal|off`), bloom clamp 64 -> 30, remove the darkness gain, picture fade via `GradeState.FadeBlack = 1 - UiDirector.PictureMul` (`UiDirector.ShaderFade = true`).
7. **Cinematic = constant 2.5 m/s**: Ring WalkPlanner with holds, glances, braking and SwayOverlay deleted, `CineAccelTime` 1.5 -> 0.5 s, `LookOmega` 3 -> 2.5, yaw/pitch rate limits 22 / 18 deg/s (Thread's `LimitRate`), goal stop by Thread's `StopRamp` (T = 6/v0), all state in **double** (Ring's float integrator drifts up to ~0.5 m over 1 500 m, arithmetic in 4.3).
8. **Fix the inherited defects listed in section 6** during the copy (audio generator on the listener, `Shader.WarmupAllShaders` only, uncapped QA frame rate, temporal dither default, shadows on, `DepthFormat.Default`, D3D12 in Ring's setup, duplicate F11/Alt+Enter handling, `-progress` in metres, run-player not routing `-capture-set sequences/encode` through gpu-guard, no per-owner build folder).
9. **Smoke gate**: real 1 500 m path, three trivial graybox builders, new QA flag `-autotest-until 400` (beats 1-3 incl. corner C1, 160 s at 2.5 m/s), then the full `-autotest` at 1.5 and 2.5 m/s through gpu-guard with a 60 fps cap.
10. Import in 11 steps with a fast check + real Unity compile after each (section 7). Do not copy any `.meta` file; let Unity create new GUIDs.

---

## 1. Ground truth, corrections to the brief

| Item | Finding | Evidence |
|---|---|---|
| URP package folder | It is **hash-named**: `N\Library\PackageCache\com.unity.render-pipelines.universal@8457e85b8184` (version 17.6.0 in its `package.json` line 4), not `...@17.6.0`. Core is `com.unity.render-pipelines.core@2d66c71e606e`. Same hashes as Thread. | `ls N\Library\PackageCache` |
| Thread test count | `T\QA\reports\tests_final.xml`: total 218, passed 218 (the brief says 206; the final report also says 218). | file header |
| "Names that never existed" | They exist, in other games: `PerfMonitor` (T and R `RT\QA\PerfMonitor.cs`), `CaptureHarness` (T, R), `InstancedField` (R `RT\Rendering\InstancedField.cs`), `BackdropRig` (A `RT\Rendering\BackdropRig.cs`), `Validate` (T `ED\BatchTools.cs` `Validate()`, 37 checks). What does not exist anywhere: a **grating mesh generator** (A has `Shaders\Grating.shader` and `MeshKit.GridQuad`, no grating mesh). | file listings |
| Abyss `QaSequences/QaBots/QaDrivers/KeyShots` | Exist, but depend on Abyss's lift, walker and GazeCues (`A\RT\QA\QaSequences.cs` -> LiftController, WalkPlanner, QaCaptureRunner ...). Thread's CaptureHarness + ShotPlan + WallBot already cover sequences, bots and drivers. Take only the `pose:` parser of `KeyShots.cs` (around line 504-540) and the pure maths of `CompositionLint.cs`. | dependency scan |
| Lead's Phase-0 tools | `N\Tools\unity-queue.ps1` (mutex `Global\NocturneUnityQueue_6Megastructures`, `[NOC]`/`[NOC-FAIL]` tags), `N\Tools\run-player.ps1` (exe `Builds\Nocturne\Nocturne.exe`, gpu-guard route), `N\Tools\metrics.py` (anchor search across all folders, subagent/workflow folders by session id, per-record timestamp filter) are already adapted (commit 2669ac8). Audit items are in 4.1. | files |
| This session's transcript | `C:\Users\<user>\.claude\projects\W--YouTube-Megastructures-6-Megastructures-6-Megastructures\<session>-...jsonl` starts on 2026-10-29 (the Thread chat continued) with 1 500 records whose `cwd` is the Thread folder before 105 records in N. **Any cwd-based file exclusion (Abyss `metrics.py`, `cwds and not all(cwd_matches...)`) would drop the whole main session**; only a per-record timestamp filter (the lead's version) is correct. | read-only scan of cwd fields |
| Thread namespaces | Thread uses sub-namespaces (`Megastructure.Player`, `.QA`, `.Rendering`, `.UI`, `.Audio`, `.Beats`); Abyss and Ring are flat `Megastructure`. Reflection strings depend on them (4.9). | grep `namespace` |
| Forward+ keyword | URP 17.6 `ShaderLibrary\Core.hlsl` lines 13-22: `_CLUSTER_LIGHT_LOOP` -> `USE_CLUSTER_LIGHT_LOOP 1` and **defines `_ADDITIONAL_LIGHTS`**; `Runtime\ForwardLights.cs` 576-579 sets `ClusterLightLoop` and clears `AdditionalLightsPixel` under Forward+. So the Ring template's `#if defined(_ADDITIONAL_LIGHTS)` light loop works in Forward+. | package source |
| Depth format | `Runtime\UniversalRendererData.cs` line 59 `enum DepthFormat`, line 101 `Depth_32_Stencil_8 = GraphicsFormat.D32_SFloat_S8_UInt`, field line 146, property line 276 (falls back to Default if unsupported). Thread sets `DepthFormat.Default`; set `Depth_32_Stencil_8` explicitly (lesson 5). | package source |

---

## 2. Conflicts between the three codebases and the winner

| Concern | T (Thread) | R (Ring) | A (Abyss) | Winner / rule |
|---|---|---|---|---|
| Mesh kit | `RT\Mesh\MeshKit.cs` (own `MeshBuilder` with `Add()`, `Color`, tangents; ChamferedBox, Lathe, Sweep, Torus, `TriangleCount(Mesh)`) | `RT\Core\MeshKit.cs` (+ `MeshValidation` class inside) | `RT\Geometry\MeshBuilder.cs`, `MeshKit*.cs` (4 partial files), `MeshValidation.cs`, `VertexBake.cs`, `CorridorKit.cs` (`Vertex()`, `Color32`, matrix stack, chamfer box/member, loft, extrude, I-beam, helix/tube/ribbon, lathe, cylinder, tapered, sphere, disc, annulus, torus, hex bolt, grid quad, AO + edge-wear bakes) | **A**. Colour convention R = AO, G = edge wear matches the Ring lit template. Replace `MeshKit.TriangleCount(mesh)` in WorldContext by a local `sum GetIndexCount(i)/3`. |
| QA stack | CaptureHarness, ShotPlan, QaProjection (double, Unity-free), CaptureWriter, AutoTestRunner, WallBot, SoakRunner, QaRunnerBase, QaCommon, QaJson, QaReport, PerfMonitor | CaptureHarness, CapturePlans, AutoTest, WallTest, CompositionCheck, QaHooks, ... | QaHarness, QaCaptureRunner, KeyShots, QaSequences, QaBots, QaDrivers, QaSoak, QaPerf, CompositionLint, QaJson | **T**. From A only: `CompositionLint` maths (near-plane box clipping, depth-grid visibility) -> `NocVisibility`, and the `pose:NAME:x:y:z:yaw:pitch:fov` parser -> ShotPlan. Nothing from R (its QaHooks forced the cinematic walk speed, Part 3.1 (c)). |
| Two `QaJson` | T `RT\QA\QaJson.cs` | - | A `RT\QA\QaJson.cs` | **T** (adapt `CompositionLint.Write` to it). |
| CliArgs | flags, `StartS`, `TryGetRes` | `Str()`, `Capturing`, `WallTest` | `-freeze-clock`, `-clock`, `-render-audio`, `-dither`, `-dither-amp`, `-tier` | **T** + A's flag names + Nocturne flags (4.5). Ring call sites: `Str("x")` -> `Get("-x")`, `Capturing` -> `CaptureDir != null`, `WallTest` -> `WallBot`. |
| Systems | GameBootstrap (reflection builders), ProgressDirector, BeatDirector | GameBootstrap, WorldBuilder, BeatDirector, ProgressDirector, AtmosphereDriver | GameBootstrap, BeatDirector, ProgressDirector, BeatBase | **T**; Ring `AtmosphereDriver` is folded in as the per-frame schedule/slew/global writer called by ProgressDirector (single writer, deterministic order). |
| Builder contract | `IBeatBuilder { Name; Build(WorldContext) }` | `IBeatBuilder { Order; Name; IEnumerator Build(BuildContext) }` | `BeatBase` | **T**. |
| Shared state | `ThreadServices` interfaces + `GradeState` + `ThreadClock` | `RingState`, `RingRig`, `RingEvents` statics | `GameState`, `SceneClock`, `ClockDriver` | **T** for services and clock; a trimmed `NocRig.cs` (NocState/NocRig/NocEvents from R) only as the bridge for the Ring player and UI; A's `SceneClock.Wrapped(period)` merged into `NocClock`. Do not copy `GameState`/`ClockDriver`. |
| Player | corridor coords, helix ThreadMath | CharacterController, FilletPath, WalkPlanner, GazeSchedule | CharacterController + lift | **R** player/walker/gaze + **T** CameraRig/ThreadInput/QualityBootstrap/StopRamp (Part 3.1). |
| Path collider builder | generic from `HalfWidth(s)` | Ring lane tables | `CorridorKit` helpers | **T** builder + new `NocLanes` + A `CorridorKit` helpers (layers changed to 30/31). |
| Post / dither | `ThreadGrade*` | `RingFinalDither.shader` | `FinalDither.shader` | **T** only (Part 3.1 rule 6). |
| UI | FadeController, GlyphText, PauseOverlay (UGUI glyphs) | UiDirector, UiTimeline (UI Toolkit) | UiViews, TitleCard, FadeController | **R** (14.11), implementing T's `IFadeUI`. |
| Fullscreen keys | `QualityBootstrap` (F11, Alt+Enter fallback) | `PlayerController.SystemInput` (F11, Alt+Enter) | - | **T QualityBootstrap**; delete the Ring copy (otherwise a double toggle). |
| Input | `ThreadInput` static readers | inline `InputAction`s in PlayerController | - | **T `NocInput`** as the single reader (Move, Brisk, C, Esc, look, menu). |
| DSP blocks | `ThreadDsp.cs` lines 1-334: XorShift32, PinkKellet, SvfTpt (Simper), DcBlocker, SineTable, FdnReverb8 | `AudioDsp.cs`: DspLimiter (look-ahead, 5 ms, 80 ms), DspBrown, DspSmoothRandom, DspSmooth ... | `Dsp.cs`, `FdnReverb.cs`, SoftLimiter | **T** blocks + R `DspLimiter`, `DspBrown`, `DspSmoothRandom`, `DspSmooth` (Thread has no look-ahead limiter; 14.3 wants one). |
| Indirect draws | - | `InstancedField(+Driver,+Handle)`, `TerrainIndirectField` (thin wrapper) | `InstancedBatch` (<= 511 per draw, explicit bounds), `InstancedRegistry(+Driver)`, `DetailTier` | **A** for P0 (signs 24 000 = 47 draws of 511; vehicles 3 900 = 8 draws). Defer R's indirect field until a measured need. |
| Metrics | no anchor | anchor in STATE.md, folder match | `--since anchor`, but excludes files with a foreign cwd | **lead's N version** (anchor search, per-record filter). |
| Fast compile | `fastcompile.py` (blind to asmdef references, single output dir) | `fastcheck.py` (honours asmdef references; fixed R blocker 2026-10-30 04:10) | `fastcompile.ps1` + `FC_RUN` per-run dir | **R fastcheck.py** + a per-run output folder (A's idea). |
| Corridor layer | 30 "Corridor" (`PlayerTuning.CorridorLayer`) | - | 8 Corridor / 9 Safety (`CorridorKit`) | **30 / 31**. |
| Graphics API | D3D11 default (`ProjectConfigurator.RequestedApi`) | `ProjectSetup.UseD3D12 = true` | D3D11 | **D3D11** (Part 8.1); never copy R's `ProjectSetup` API code. |
| Encode test | `Tools\qa\encode_test.py` (`accurate_rnd+full_chroma_int`, BT.709 tv tags) | `Tools\encode_test.py` | `Tools\qa\encode_test.py` (same flags + `bitexact`) | **T**, plus the H1 step-detector gate (4.8). |

---

## 3. Naming, namespace, tags

- Namespace: every runtime file `namespace Megastructure`, editor `Megastructure.EditorTools`, tests `Megastructure.Tests`. When flattening Thread files delete `using Megastructure.Player;`/`.QA`/`.Rendering`/`.UI`/`.Audio`/`.Beats`.
- Prefix: `Thread*` / `Ring*` / `Ab*` -> `Noc*`; game data classes take the 14.12 names `NocturneConstants`, `NocturnePalette`; unprefixed classes keep their names (only one of each is imported).
- Log tags: `[THREAD]`, `[Ring]`, `[ABYSS]` -> `[NOC]`; `[THREAD-FAIL]`, `[ABYSS-ERROR]` -> `[NOC-FAIL]` (the lead's queue and run-player grep exactly these).
- Shader globals `_Th*`, `_Ring*`, `_Ab*` -> `_Noc*`; material properties of the grade `_ThGrade*` -> `_NocGrade*`; HLSL macro families `TC_*` -> `NC_*` (constants), `PAL_*` (palette) kept; `TH_`/`Th` helper prefixes -> `NOC_`/`Noc`. Lesson 5: a global name must never equal a material property name (CI check: grep `Shader.SetGlobal*` ids against every `Properties` block).
- Shader names `Megastructure/Noc*` (14.2.5).

---

## 4. File-by-file import table

Columns: source -> destination (`N\...`), new name, compile dependencies inside the copied set, what to strip (game content), what to adapt. "Verbatim" = copy, then only the global renames of section 3.

### 4.0 Assemblies (Thread layout, verbatim)

| # | Source | Destination | Notes |
|---|---|---|---|
| 0.1 | `T\RT\Megastructure.Runtime.asmdef` | `N\RT\Megastructure.Runtime.asmdef` | refs Unity.InputSystem, RP Core/Universal Runtime, UnityEngine.UI. UI Toolkit needs no reference. |
| 0.2 | `T\ED\Megastructure.Editor.asmdef` | `N\ED\` | keeps `Unity.RenderPipelines.GPUDriven.Runtime` (R blocker: `GPUResidentDrawerMode` fails without it). |
| 0.3 | `T\TE\Megastructure.Tests.EditMode.asmdef` | `N\TE\` | verbatim. |
| 0.4 | `T\Assets\_Game\Scripts\Tests\PlayMode\Megastructure.Tests.PlayMode.asmdef` | `N\...\Tests\PlayMode\` | verbatim. |
| 0.5 | `T\Assets\_Game\Beats\Megastructure.Beats.asmref` | `N\Assets\_Game\Beats\` | `{"reference": "Megastructure.Runtime"}`. |

### 4.1 Tools (`N\Tools`)

| # | Source | New | Status / adaptation |
|---|---|---|---|
| 1.1 | `T\Tools\unity-queue.ps1` | same | **Done by the lead.** Keep `-StallMin 0` for every `-runTests` call (lesson 2); compile-only gate = call without `-Method` (Unity opens, compiles, quits). |
| 1.2 | `T\Tools\run-player.ps1` | same | **Done.** Fix: the heavy detector only looks for `-soak -perf-log -autotest -wallbot -sequence` and `-capture-4k`; Thread's harness runs long jobs via `-capture-set sequences|transitions|statics|pans|edges|encode|all|everything` -> also route those through gpu-guard. The gpu-guard path has no log-stall watchdog: acceptable (gpu-guard has `-TimeoutMin`), pass `-TimeoutMin` per job (soak 40, autotest 1.5 m/s 25, cinematic 15, wallbot 40). |
| 1.3 | `W:\YouTube\Megastructures\Tools\gpu-guard.ps1` | already in `N\Tools` (identical) | Verify once that a job's exit code passes through: run `-FilePath cmd.exe -ArgumentString "/c exit 3"` and expect 3. `Start-Process -PassThru` + `WaitForExit(ms)` can return a null `ExitCode` unless the handle was cached (PowerShell issue 20400; SO answer 23797762). Do not edit the shared script; if the code is lost, run-player must judge the run by the player's `report.json` instead. |
| 1.4 | `T\Tools\metrics.py` | same | **Done (lead's anchor version).** Run it now once on the real transcripts (lesson 16) and check: main session counted only after 2026-11-13T19:22:09+01:00, subagents/workflows of session <session> included, folders 4/5 and `W--YouTube-Megastructures` excluded. |
| 1.5 | `T\Tools\pricing.json` | same | Done; re-fetch the price page at Part 15.3. |
| 1.6 | `R\Tools\fastcheck.py` | `fastcheck.py` | Asmdef-aware fast type check. Add a per-run output folder `Temp\fastcheck\<pid>` (parallel builders). Still blind to shaders; needs one prior Unity compile so `Assembly-CSharp-Editor.csproj` lists the references. |
| 1.7 | `T\Tools\gen_hlsl.py` | same | Read `NocturneConstants.cs` / `NocturnePalette.cs`, write `SH\Generated\NocConstantsGen.hlsl` (`NC_*`) and `SH\Generated\NocPalette.hlsl` (`PAL_*`), byte-identical to `ShaderGen` (needs `.gitattributes * -text`, present). |
| 1.8 | `T\Tools\qa\*.py`, `run_encode_clips.sh` | `N\Tools\qa\` | `qa_common.py`: parse `NocturneConstants.cs` (line 77 constant path), delete Thread `_FALLBACK` literals (lines ~117-145: KF tags, BeatNames, beacon), replace with Nocturne's; `regions_default.json`: rewrite from 14.10 (banding/aliasing regions per beat). `encode_test.py`: add the 14.7 H1 gate (4.8). `image_metrics.py`: BandScore slope floor per 14.10, magenta, NaN-like, isolated pixels > 8x 5x5 median outside neon masks. |
| 1.9 | `A\Tools\lookdev.ps1` | same | Per-slot build `Builds\lookdev_<Slot>` via env `NOC_BUILD_DIR` (add to BatchTools, 4.9), exe `Nocturne.exe`, shots via `-capture-set`/`-capture-only` (one value per call, lesson 1), route through run-player (never start the exe directly: gpu-guard + one player rule). |
| 1.10 | `T\PLAY_WINDOWS.bat` | `N\PLAY_WINDOWS.bat` | `Thread` -> `Nocturne` (3 places). |
| 1.11 | `R\docs\BUILDER_BRIEF.md`, `CRITIC_BRIEF.md` | `N\docs\` | Templates for subagent briefs; replace Ring paths, lenses (a)-(f) of Part 11.5. |

### 4.2 Core (`N\RT\Core`)

| # | Source | New | Depends on | Strip | Adapt |
|---|---|---|---|---|---|
| 2.1 | `T\RT\Core\CliArgs.cs` | `CliArgs` | NocturneConstants | `-freeze-ambient` (now `-freeze-clock`), `-freeze-dither` kept only as alias | `-progress X` is a **fraction** (s = 1500 X, 14.12) — breaking change vs Thread (metres); full flag list 4.5. |
| 2.2 | `T\RT\Core\ThreadClock.cs` | `NocClock.cs` (`NocClock` + `GradeState`) | - | `GradeState.Darkness` | `FreezeDither` default **true**; add `DitherMode {Static, Temporal, Off}`, `Wrapped(period)` (from `A\RT\Core\SceneClock.cs`), rain clock `t_r` + `FreezeRain`, `-clock T` start. |
| 2.3 | `T\RT\Core\ThreadServices.cs` | `NocServices.cs` | - | `FootSurface {Salt, Stone, Terrace, Metal, Pier}` | Surfaces of 14.4/14.3: `WetConcrete, WetStone, Steel, Glass, Garden, PlazaStone`; doc strings (goal s 1 497). Interfaces unchanged (IPlayerState, ICameraControl, IAudioHooks, IFadeUI). |
| 2.4 | `T\RT\Core\WorldContext.cs` | `WorldContext` | MaterialLibrary | `GroundGroup`, `FrameState.aVirt/darkness/groundActive/nearActive` | Add `CityRoot`, `BackdropRoot`; triangle count without `MeshKit.TriangleCount`; log tag. |
| 2.5 | `T\RT\Core\MaterialLibrary.cs` | `MaterialLibrary` | - | all Thread fields | Graybox fields (Graybox, Grade) first, later the 14.2.5 list; add `IEnumerable<Material> All()` (reflection over Material fields, load time) for WarmupLoader. |
| 2.6 | `T\RT\Core\ShaderIds.cs` | `ShaderIds` | - | every Thread id | `_NocTime`, `_NocFog*`, `_NocSky*`, `_NocAmb*`, `_NocScreen`, `_NocFade` (A's `_AbFade`), `_NocKoiK` ... |
| 2.7 | `T\RT\Core\ThreadMath.cs` (+ `R\RT\Core\RingMath.cs`) | `NocMath.cs` | - | helix, thread law, ascent, sun, bank, PathPoint/PathY/HalfWidth (replaced by FilletPath/NocLanes) | keep `D3`, `Deg`, `Lerp`, `Smoothstep`, `Smootherstep(01)`, `Clamp01`; add from Ring `Pcg`, `U01`, `ValueNoise`, `CriticalDamp`, `CriticalDampAngle`, `Slew` (double overloads). |
| 2.8 | `T\RT\Core\ThreadConstants.cs` | `NocturneConstants.cs` | - | all values, Thread `GazeCue` | 14.12 table; `KeyShot` struct shape kept (tag, s, x, y, z, yaw, pitch, fov, lumaMin, lumaMax); `GazeKeys` (14.5, all `Rel`), `Waypoints`, `CornerRadii`, `SharpCum`, `BeatBoundaries {0,120,400,560,880,1080,1260,1500}`, `GoalTriggerS 1497`, `GoalS 1500`, `CameraNear 0.05`, `CameraFar 12000`, `Msaa 4`, `DitherPeak 2.0`, `DitherLadder 2.5`, `DitherTile 64`, `DitherSigma 1.5`, `BloomClamp 30`. |
| 2.9 | `T\RT\Core\ThreadPalette.cs` | `NocturnePalette.cs` (+ enum `NocPal`) | - | Thread table | 14.2.1 tokens; keep `Table`, `Linear`, `SrgbToLinear` API (ShaderGen, MaterialFactory, gen_hlsl use them). |
| 2.10 | `R\RT\Core\RingState.cs` + `R\RT\Core\RingRig.cs` | `NocRig.cs` (`NocState`, `NocRig`, `NocEvents`) | - | Ark/Sunset/Night/StarV/Set/Gust/ArkCue, `BuildContext`, `RingSurface`, `HatchTriggered`, director chain | Keep S, T, Dt, Capture, SequenceMode, Paused, Cinematic, AutoTest, GoalReached, Beat, FrameIndex; Player, Controller, Camera, WorldReady, ForcedS/T, PlayerVelocity, Yaw/Pitch; events LoadComplete, BeatEntered, GoalReached, EndingComplete, PlayerMoved, PauseChanged, CinematicChanged, Footstep. Writers: GameBootstrap (WorldReady, LoadComplete), ProgressDirector (S, T, Dt), BeatDirector (BeatEntered). |
| - | `T\RT\Core\FogMath.cs`, `T\RT\Sky\*` | not copied | | Thread sky/fog | Write `NocFogMath` (C# mirror of `NocAtmosphere.hlsl`) new. |

### 4.3 Player (`N\RT\Player`)

| # | Source | New | Depends on | Strip | Adapt |
|---|---|---|---|---|---|
| 3.1 | `R\RT\Player\FilletPath.cs` | `FilletPath` | - | 3-radius ctor (`RingConstants.Waypoints`, `RingWorld.Cumulative`) | Keep the generic ctor `(waypoints, cum, radii)`; **s = filleted distance d** (14.4); `SharpS/DAtSharp` kept only for the 14.4 conversion test. Recommended: double precision (positions stay < 2 km, but see 3.4). Inputs: waypoints P0..P8, sharp cum `{0, 120, 195, 410.3, 570.3, 890.3, 1096.74, 1289.62, 1536.06}`, radii `{0,0,24,0,0,0,30,30,0}`. Collinear P1/P3/P4/P5 get no fillet (theta < 1e-4). Expected: length 1500.007 (inside 1500.00 +- 0.05), C1 tangent s 171.00 / 208.70, C2 1056.44 / 1103.56, C3 1236.44 / 1283.56, P3 at s 400.00. |
| 3.2 | new (API of `R\RT\Player\PathGuide.cs` lines 137-217) | `NocLanes.cs` | FilletPath, NocturneConstants | Ring lanes | Pure: `LaneHalfWidth(s)` from the 14.4 lane list (alley 1.85/2.65/1.95, crossing 10.0, median 11.0, tongue `min(12, max(2.15, w(x)-2))`, bridge 2.15, gate funnel 3.5 -> 2.15, garden 6.0, promenade 10.0, plaza 12.0 then funnel to 3.0), `DistanceOutsideLanes(p)`, `LaneSamplePoints(step)`, `CentreAt(s)`. Collider gap = 2 (halfWidth + 0.35). |
| 3.3 | `R\RT\Player\GazeSchedule.cs` | `GazeSchedule` | NocMath, NocturneConstants | `Holds`, `HoldSteps`, `HoldRelease`, `Glances`, `Step`, `StepPose`, `SwayOverlay`, `RingConstants.StarOutYaw`, the static `Evaluate(s, out..)` that uses `AutoWalker.Path.DAtSharp` | Keys from `NocturneConstants.GazeKeys` (double); `Evaluate(s, heading)` unchanged in form; `LateralOffset(s)` returns 0 (Part 14 asks for none; key-shot eyes are on the centre line). |
| 3.4 | `R\RT\Player\WalkPlanner.cs` | `WalkPlanner` | FilletPath, GazeSchedule, NocMath, PlayerMath(StopRamp) | holds, brake-into-hold, glances, release | Cinematic edits (a)-(f), section 4.4. All distance/speed state in **double**: with float `_d += v dt` at 2.5 m/s, 60 fps the increment is 0.041667 m = 341.33 ulp in [1024, 2048) (ulp 1.22e-4), so every step rounds down by 0.33 ulp: -4.1e-5 m x 11 400 frames = -0.46 m; in [512, 1024) +0.25 m. |
| 3.5 | `R\RT\Player\AutoWalker.cs` | `AutoWalker` | WalkPlanner, PlayerController, CameraRig, NocRig, CliArgs | `RingConstants.Walk` default for QA, `CliArgs.Str("speed")`, `GameBootstrap.Restarting/CinematicOnLoad` (keep only if the Ring restart path is kept) | `Path = new FilletPath(NocturneConstants.Waypoints, SharpCum, CornerRadii)`; `BeginQa(speed, seed, fromS, lateralTarget)` (lateral for WallBot); cinematic cruise = 2.5 always; rate-limited look applied through `ICameraControl.SetLook`. |
| 3.6 | `R\RT\Player\PlayerController.cs` | `PlayerController` (implements `IPlayerState`) | NocInput, NocRig, AutoWalker, CameraRig, NocLanes, PlayerMath, NocServices, QualityBootstrap(no) | inline InputActions, F11/Alt+Enter code (`SystemInput` lines ~285-300), grass/footbridge surface logic, `RingEvents.SurfaceAt` | Input via `NocInput`; Esc -> `NocEvents.RaisePause`; C -> cinematic toggle; IPlayerState (sketch 4.6); goal: at S >= 1497 input and drive are replaced by `StopRamp` (smoothstep, T = 6/v0) to s 1500, `IFadeUI.BeginEnding`, `IAudioHooks.BeginEnding`, `NocEvents.RaiseGoal`; footsteps -> `IAudioHooks.Footstep(surface(s), pan, gain)` with stride `lerp(0.75, 0.85, InverseLerp(1.5, 2.5, v))`; safety y < -3 or outside lanes > 1 m -> snap (Ring `Safety`). |
| 3.7 | `T\RT\Player\CameraRig.cs` | `CameraRig` | NocInput, PlayerMath, NocServices, NocSchedule(Fov) | Thread gaze cue start pitch | near 0.05, far 12 000 (Thread 0.1 / 250 000); `Fov(s)` = 14.2.4 vFOV schedule; add `SnapEye()` no-op (Ring Teleport calls it); 0.2 m camera sphere check (`Physics.CheckSphere(eye, 0.2, CorridorMask)`) counted for QA (Part 10 item 8). |
| 3.8 | `T\RT\Player\ThreadInput.cs` | `NocInput.cs` | - | - | verbatim + rename. |
| 3.9 | `T\RT\Player\QualityBootstrap.cs` | `QualityBootstrap` | CliArgs, NocInput | - | `Application.targetFrameRate = 60` when `-autotest`, `-soak`, `-wallbot` or a capture sequence runs (Part 2.5.3); uncapped only with `-perf-log`. Thread sets -1 always. |
| 3.10 | `T\RT\Player\PlayerMath.cs` | `PlayerMath.cs` | NocMath, NocturneConstants | `EyeFollower` (no ramps/stairs: eye = feet + 1.7), helix head-bob terms, `Surface(s)` Thread table, `FootstepCadence` if footsteps move to the controller | keep `PlayerTuning` (CorridorLayer 30, MouseDegPerCount 0.08, PitchLimit 88, thresholds), `CritDamp`, `WrapDeg`, `StopRamp` (GoalS -> 1500; check: T = 2 x 3 / v0 = 6/v0; 2.4 s at 2.5, 4.0 s at 1.5), head bob 0.008 m, never roll. |
| 3.11 | `T\RT\Player\PathGuide.cs` | `PathGuide` | FilletPath, NocLanes, PlayerTuning, CorridorKit, UiDirector(Blink) | helix chord constants, ThreadMath calls, `FadeController.Blink` | Segments along s from `NocLanes.LaneHalfWidth`: floor box 1 m thick (top y 0), walls 6 m high at `halfWidth + 0.35` (inner face), segment <= 10 m straight, <= 2 m on arcs and funnels (chord deviation `L^2/8R`: 2 m on r 24 = 2 cm, inside the 3 cm `WallGap`), end caps, kill triggers 5 m below (y -5; spec safety y < -3 is the controller's). Keep `CoverageCheck` and `KillCount` (Thread QA reads them by reflection). |
| - | `T\RT\Player\AutoWalker.cs`, `PlayerController.cs`; `A\RT\Player\*` incl. `LiftController` | not copied | | | Nocturne has no ramps, stairs or lifts (14.4). |

### 4.4 Cinematic edits (a)-(f) — concrete

(a) **Constant speed.** `WalkPlanner`: `CineAccelTime 0.5` (was 1.5 f, `R\RT\Player\WalkPlanner.cs` line 25), `QaAccelTime 0.2`, cruise = `NocturneConstants.Brisk` (2.5) for the cinematic, phases `Accel, Cruise, Stop, Done` only. The smootherstep accel integral is `T/2` of time lost: 0.25 s at T 0.5 (14.1: trigger at 598.8 + 0.25 = 599.05 s) and 0.1 s at T 0.2 (autotest 1.5 m/s: 998.0 + 0.1 = 998.1 s). Goal: when `_d >= 1497`, `ramp = StopRamp.Begin(_d, _v)`; then `_d = ramp.PositionAt(t)`, `_v = ramp.SpeedAt(t)` (`v0 (1 - smoothstep(0, T, t))`, T = 6/v0: 2.4 s, stops at 1500 at 601.45 s). Delete `GazeSchedule.Holds`, `HoldSteps`, `HoldRelease`, `Glances`, `SwayOverlay`, `NextStopD`, `StartBrake`, `BeginHold/EndHold`, `RingConstants.CinematicHolds` (R `RingConstants.cs` line 166). Thread's `MinSlowFactor`/`CinematicPlan` and Abyss's `WalkPlanner.ProfileSpeed`/hold flag are not imported at all.

(b) **Gaze keyed to metres.** Keys (s, yaw rel, pitch) are already s-based in Ring's design; replace the table by 14.5's, follower `LookOmega 2.5` (was 3), then rate limit:
```csharp
// after CriticalDampAngle / CriticalDamp (omega 2.5)
static void LimitRate(ref double x, ref double v, double x0, double maxRate, double dt)
{   double step = x - x0, lim = maxRate * dt;           // Thread AutoWalker.LimitRate
    if (step > lim) { x = x0 + lim; v = maxRate; } else if (step < -lim) { x = x0 - lim; v = -maxRate; } }
LimitRate(ref _yaw, ref _yawV, y0, 22.0, dt); LimitRate(ref _pitch, ref _pitchV, p0, 18.0, dt);
```
EditMode tests: simulate the follower over s 0..1500 at 2.5 m/s, dt 1/60: max |dyaw/dt| <= 22, |dpitch/dt| <= 18, peaks about 19.3 / 17.9 (14.5); no consecutive key pair with different poses closer than 1 m (de-duplicate the repeated (585, -2, +2) at the beat 3/4 seam); for every KS on the path the follower pose (start 40 m earlier) is within 8 deg yaw / 6 deg pitch of the key pose (readability, 14.5) and the 11.13 audit uses that follower pose.

(c) **Real-time couplings.** Keep sway (time, <= 0.3 deg, Ring value-noise, clamped to a 0.3 deg radius) and the start lateral-residual decay (time, start only). Everything parametric (EV, fog sigma, rain rho, bloom, vFOV, reverb, pad chords) is a function of s with Rule P (24 m smootherstep); slew limiters 0.30 EV/s and 0.1/s bloom (14.6) never bind at 2.5 m/s (peak EV slope 0.062 EV/s), bypassed in capture stills, kept in sequences (Ring AtmosphereDriver convention). Offline audio: `OfflineRenderSettings.Speed` default `WalkSpeed` -> render both 1.5 and 2.5 (`NocturneScore.Render(s0, s1, speed)`). Tram/trains launch by s then real time (14.7 H7, capture `tau = max(0, (s - s_launch)/2.5)`). Do not import any QA hook that forces a walk speed.

(d) **Tests and docs.** Rewrite Ring `TE\PathScheduleTests.cs` (11 tests: holds 49 s, goal 636.8 s) to: cinematic trigger 599.05 +- 2 s, stop 601.45 +- 2 s; 1.5 m/s trigger 998.1 +- 3 s, rest 1002.1 s; ending +17 s. Replace Thread's `AutoTestRunner` constants (`573.33 +- 8`, `344 +- 5`, rest at 860) with these.

(e) **Brisk flag.** Never derive "brisk" from `speed > 2.0` (it flips once during the 0.5 s ramp). `Brisk = AutoWalker cinematic active || Shift held`; every speed-dependent parameter (stride, step gain, audio) is a continuous function `InverseLerp(1.5, 2.5, v)`.

(f) **Lateral offset.** `GazeSchedule.LateralOffset(s) = 0` for Nocturne (no lateral request in 14.5; e.g. KS16 eye (901.74, 1.7, 271.44) is on the leg x = 901.74). Speed is ds/dt along the filleted centre line, so the 3D speed equals 2.5 m/s.

### 4.5 CliArgs flag set (Part 11.1 + 14.12)

`-autotest`, `-beat N`, `-progress X` (fraction of 1500), `-capture <dir>`, `-res WxH`, `-cinematic`, `-seed N`, `-quit-after S`, `-perf-log <file>`, `-nofade`, `-rain X` (0..1, 0 off), `-freeze-rain`, `-dither static|temporal|off` (default static; `-freeze-dither` = static), `-dither-amp X` (encode ladder 2.0/2.5), `-sequence NAME` (boundaries, statics, signs, rain, koi, traffic, encode -> ShotPlan sets), `-tier N`, `-freeze-clock`, `-clock T`, `-render-audio <file>`, `-audio-dump <file>`; QA extras kept from Thread: `-brisk`, `-speed X`, `-wallbot`, `-soak S`, `-capture-set`, `-capture-only` (one value per call), `-capture-4k`, `-capture-warmup/settle`, `-report-dir`, `-grade-off`; new `-autotest-until S` (smoke gate) and `-uifont theme|legacy` (Ring).

### 4.6 IPlayerState on the Ring controller (sketch)

```csharp
public sealed class PlayerController : MonoBehaviour, IPlayerState
{
    double _s, _lat; float _speed; bool _finished, _frozen, _drive; double _driveSpeed, _driveLat;
    public double S => _s;  public double Lateral => _lat;  public float Speed => _speed;
    public bool Brisk => AutoWalker.Mode == WalkMode.Cinematic || NocInput.Brisk();
    public bool Finished => _finished;
    public Vector3 EyePosition => transform.position + new Vector3(0f, NocturneConstants.EyeHeight + bob, 0f);
    public bool Frozen { get => _frozen; set => _frozen = value; }
    public void Teleport(double s, double lat) { var p = AutoWalker.Path; var xz = p.PointAt(s) + p.RightAt(s) * lat;
        TeleportFeet(new Vector3(xz.x, 0f, xz.y), p.HeadingAt(s)); _s = s; }   // CC disabled/enabled + Physics.SyncTransforms
    public void SetDrive(bool active, float speed, float latTarget)          // Thread QA (AutoTestRunner, WallBot, SoakRunner)
    { if (active) AutoWalker.BeginQa(speed, CliArgs.Seed, float.NaN, latTarget); else AutoWalker.Request(WalkMode.None); }
    void LateUpdate() { _s = AutoWalker.Path.Project(xz, (float)_s, 25f, out _); _lat = AutoWalker.Path.Lateral(xz, (float)_s); } // hint window keeps s monotone across corners
}
```
WallBot: 14.10 walks 0.40 m inside both lane edges; Thread's `PushBeyond 0.5` (outside) must become `-0.40` (inside) or keep pushing outward only as an extra collider test. The planner's `MaxLag 0.6` waits when the player lags its target; measure the lag along the tangent only, otherwise a wall contact stalls the bot.

### 4.7 Rendering (`N\RT\Rendering`, `N\SH`)

| # | Source | New | Depends on | Strip | Adapt |
|---|---|---|---|---|---|
| 7.1 | `T\RT\Rendering\ThreadGradeFeature.cs` | `NocGradeFeature` | NocGradePass, BlueNoise, GradeState | - | rename only (`[DisallowMultipleRendererFeature("Noc Grade")]`). |
| 7.2 | `T\RT\Rendering\ThreadGradePass.cs` | `NocGradePass` | NocGradeMath, R2Sequence, BlueNoise, NocClock, GradeState | `GradeState.Darkness` use | pass/property names; `g1.w` = `GradeState.DitherPeak` (2.0; 0 when `-dither off`); delete `DitherAmplitude(lsb, darkness)` call. Keep everything else (Render Graph, `requiresIntermediateTexture`, R8G8B8A8_SRGB output, FinalBlit 1:1, pooled PassData, capture contract). |
| 7.3 | `T\RT\Rendering\ThreadGradeMath.cs` | `NocGradeMath` | NocMath | `DitherDarkD0/D1`, `ThreadConstants.DitherDarkGain` | `BloomClamp 64 -> 30` (14.2.4 clamp column); new `DitherAmplitude(peak, lumaDisplay) = peak * Lerp(1.0, 0.6, Clamp01((Y' - 0.35)/0.4))`; `Display()` unchanged (saturation = matrix, contrast 0 = identity). |
| 7.4 | `T\RT\Rendering\BlueNoise.cs`, `R2Sequence.cs` | same | NocturneConstants (DitherTile 64, DitherSigma 1.5) | - | verbatim (seed 20261029 is arbitrary and fine). |
| 7.5 | `T\SH\ThreadGrade.shader` | `SH\NocGrade.shader` (`Megastructure/NocGrade`) | `Generated\NocConstantsGen.hlsl` (`NC_DitherTile`) | - | Step 6 per-pixel amplitude (below); passes `NocBloomPrefilter/Down/Up`, `NocGrade`; props `_NocGrade*`. |
| 7.6 | `A\RT\Rendering\BackdropRig.cs` | `BackdropRig` | NocServices/NocRig.Camera | `GameState` | NocSky dome (r 11 000, rotation locked, `gridStep 0` for the sky: exact follow). |
| 7.7 | `A\RT\Rendering\InstancedBatch.cs`, `InstancedRegistry.cs`, `InstancedRegistryDriver.cs` | same | ShaderIds (`NocFade`) | `_AbFade` | Add `SubmitAllFor(Camera)` (RenderParams.camera = capture camera) and call it from CaptureHarness before each render (lesson 8: code-drawn fields must be re-submitted per capture camera). Signs/vehicles are opaque/additive meshes, chunks <= 511, explicit bounds (14.7 H2). |
| 7.8 | `A\RT\Rendering\DetailTier.cs` | same (optional in P0) | InstancedBatch, ShaderIds | `QaHooks.TierOverride` | `-tier` via CliArgs. |
| 7.9 | `A\RT\Core\WarmupLoader.cs` | `RT\Rendering\WarmupLoader.cs` | MaterialLibrary.All, InstancedRegistry | key-light shadow frames (`AbyssConst.YawPitch`, lines ~120-130) | 3 frames per material behind black (14.6), plus the koi, trees, trains, rain pass; `_AbPictureFade` -> `GradeState.FadeBlack = 1`. Replaces Thread's `Shader.WarmupAllShaders()` only. |
| 7.10 | `R\SH\Include\RingLitCore.hlsl` | `SH\Include\NocLitCore.hlsl` | NocAtmosphere.hlsl | `RingBend.hlsl`, `RingGround.hlsl` includes, bend theta fields | Flat world: `positionWS == positionFlatWS`, theta 0; keep hooks (`NocSurfaceHook`, `NOC_HAS_VERTEX_HOOK`, `NOC_HAS_CLIP_HOOK`, `NOC_INDIRECT`), `_ENVIRONMENTREFLECTIONS_OFF`. |
| 7.11 | `R\SH\Include\RingLitPass.hlsl` | `NocLitPass.hlsl` | Lighting.hlsl, NocLighting | bend calls | Forward: `c = NocShade(...)`, then `NocFog(c, posWS)`, `NocFloor(c)` **last** (14.2.2 order: shade, cloud mix, fog, floor). |
| 7.12 | `R\SH\Include\RingLighting.hlsl` | `NocLighting.hlsl` | NocAtmosphere | `RingBendRotate`, `RingKeyAtten`, `_RingSet` tunnel mask | Trilight ambient from `_NocAmbSky/Equator/Ground` x LightCal; reflection = `NocSkyColor(reflect)` toward fog colour with roughness; Forward+ loop kept (non-main directional loop over `URP_FP_DIRECTIONAL_LIGHTS_COUNT` + `LIGHT_LOOP_BEGIN`); add the analytic koi term `_NocKoiK` (14.2.3). |
| 7.13 | `R\SH\RingCeramic.shader` | `SH\NocGraybox.shader` (reference material, then NocFacade/NocWet/...) | NocLitCore, NocLitPass | Ring panel grooves | multi_compile: keep `_ADDITIONAL_LIGHTS` and `_CLUSTER_LIGHT_LOOP` (+ instancing); **drop** `_MAIN_LIGHT_SHADOWS*`, `_ADDITIONAL_LIGHT_SHADOWS`, `_SHADOWS_SOFT` (no shadow maps, 14.2.3). Keep 4 passes (ForwardLit, ShadowCaster, DepthOnly, DepthNormals). |
| 7.14 | `T\SH\ThreadLine.shader` | `SH\NocLine.shader` | NocCommon (`_NocScreen.y` min width), NocAtmosphere, NocLighting | `ThreadCommon.hlsl`, `ThreadPatterns.hlsl`, `ThreadFlatPasses.hlsl`, `ThreadShadeFlat`, cable shadow, `_BelowFade/_BelowHaze`, shadow multi_compiles | Keep `ThLineTubeVertex`, `ThLineRibbonVertex`, `ThLineRibbonCoverage`, A2C path, 1.75 px minimum (`max(1.5, 1.75 H/1080)`), energy floor 0.35 (14.2.5 NocLine); colour = emissive/lit by NocLighting, then NocFog, NocFloor. |
| new | - | `SH\Include\NocAtmosphere.hlsl`, `NocCommon.hlsl`, `Generated\NocConstantsGen.hlsl`, `Generated\NocPalette.hlsl` | | | 14.2.2 interface; generated files by ShaderGen/gen_hlsl. |
| - | T ThreadFlat/ThreadEmissive/ThreadSky + Include, A FinalDither, R RingFinalDither and all other Ring/Abyss shaders | not copied | | | |

**NocGrade step 6 (14.7 H1 delta):**
```hlsl
static const float3 kNocGradeLuma = float3(0.2126, 0.7152, 0.0722);   // Rec.709 weights on the ENCODED values (Y', as 14.10 measures luma)
float3 s = NocGradeLinearToSrgb(c);                                     // display-referred, pre-dither
float lumaD = dot(s, kNocGradeLuma);
float amp = _NocGradeP1.w * lerp(1.0, 0.6, saturate((lumaD - 0.35) / 0.4));   // P1.w = 2.0 (ladder 2.5), 0 = -dither off
float n = NocGradeTpdf(input.positionCS.xy) * (amp / 255.0);            // R2 offset is (32,32) when frozen (DitherFrame -> 0)
float3 edge = saturate(min(s, 1.0 - s) * 510.0);                        // exact black/white stay exact (fades)
s = saturate(s + n * edge);
return float4(NocGradeSrgbToLinear(s), 1.0);
```
Values: amp 2.0 for Y' <= 0.35, 1.6 at 0.55, 1.2 for Y' >= 0.75. Tests (GradeTests pattern): CPU mirror equals these, monotone non-increasing, shader-constant check; dither frozen by default (`NocClock.FreezeDither == true` without flags, offset constant over 1 000 frames); `-dither temporal` changes the offset each frame; `-dither off` gives amp 0. Floor (Y' about 10 codes) keeps every non-fade pixel >= 8 codes above black, so the 2-code TPDF never clips at 0. Pitfall: the dither is monochrome (same n on R, G, B, Thread R6 rationale: luma survives 4:2:0); Cb/Cr then get only the indirect per-channel rounding noise. H1 gates Cb and Cr too: if that sub-gate fails, test decorrelated per-channel offsets of the same tile (e.g. R2 point n, n+1, n+2) before raising the amplitude.

Other grade mappings: `GradeState.ExposureEV` = EV schedule; `BloomThreshold 0.9/0.8`, `BloomIntensity`, `BloomScatter` from 14.2.4 (threshold applies to exposed linear values, already true in the prefilter); `Saturation` = +6..+10 (factor 1.06..1.10, URP convention); `Contrast = 0`; `Vignette 0.16..0.22`; `FadeBlack = 1 - UiDirector.PictureMul`.

### 4.8 Systems, UI, Audio, QA, Editor

| # | Source | New | Depends on | Strip | Adapt |
|---|---|---|---|---|---|
| 8.1 | `T\RT\Systems\GameBootstrap.cs` | same | WorldContext, ProgressDirector, NocClock, CliArgs, WarmupLoader, NocRig | `palette`, `groundGroup`, `sun` fields | `FreezeDither` from `-dither`; WarmupLoader instead of `Shader.WarmupAllShaders`; set `NocRig.WorldReady`, raise `NocEvents.LoadComplete`; builder discovery unchanged. |
| 8.2 | `T\RT\Systems\ProgressDirector.cs` | same | NocMath, NocturneConstants, ShaderIds, NocClock, NocServices, WorldContext, AtmosphereDriver | sun colours, ground sink, deck/bank/thread uniforms | Keep S, Override(S), `Frame`, `Apply()`, `beginCameraRendering` hook, updater loop, `-dither-amp`; writes `NocState.S/T/Dt`. |
| 8.3 | `T\RT\Systems\BeatDirector.cs` | same | ProgressDirector, NocServices, NocturneConstants, NocRig | - | `BeatEntered` at the exact boundary (autotest order at s 0, 120, 400, ...), content activation window with 8 m forward lead / 12 m back hysteresis (14.6); raise `NocEvents.BeatEntered`; goal at 1497. |
| 8.4 | `R\RT\Directors\AtmosphereDriver.cs` | `RT\Systems\AtmosphereDriver.cs` | NocSchedule (new, 14.2.4 table + Rule P), NocMath, ShaderIds, GradeState, NocState | RingSchedule, RingShadowParams, ship/cloud shadows, URP Volume caching (`CacheVolume`, `ApplyVolume`), sunset branch | Pattern kept: one writer, slew limiters (EV 0.30/s, bloom 0.1/s, vFOV 1 deg/s), capture bypass for stills, globals per frame, lights (sky fill directional, **no shadows**), LightCal; called by ProgressDirector, no own execution order. |
| 8.5 | `R\RT\UI\UiDirector.cs`, `UiTimeline.cs` | same (implements `IFadeUI`, registers `NocServices.Fade`) | NocRig, NocMath, NocturneConstants, NocturnePalette, CliArgs, NocInput, MaterialLibrary/NocAssets (font, PanelSettings) | `RingAssets`, `RingPalette.Entries` lookup, `GazeSchedule` reference | `ShaderFade = true` (fade inside the grade before the dither); timings and texts from 14.11 (black 0-0.6, picture 0.6-3.1, title 0.8-2.0/hold 4.0/out 5.4, 67 px, tracking 9 px, 33 % height; hint at 6.0 s for 6 s, 0.6 s fades, gone within 0.4 s, never in cinematic/autotest; ending: picture 1.5 s, fade 4.0 s, line in 2 s/hold 6 s/out 2 s, 51 px, ends at 17.0 s, "One lamp is still lit."). Ring's ending (3.0 s dwell, 1.5 s black, 18.5 s) must be re-timed. `IFadeUI.Black`, `BeginIntro`, `BeginEnding`, `HideHint`, `Paused`, `SetBlackImmediate`, plus `Blink()` for PathGuide. |
| 8.6 | `R\ED\Batch\ProjectSetup.cs` (`CreateOrUpdateUi`, `ThemeText`, `UiRuntimeShaders`, Always-Included update) | `ED\NocUiSetup.cs` | EditorCommon | `UseD3D12`, quality/shadow constants | Writes `Assets\_Game\Settings\NocTheme.tss` (`@import url("unity-theme://default");`) and `NocPanel.asset` (ScaleWithScreenSize 1920x1080, match height, sorting 100); adds the 9 UI runtime shaders to Always Included. |
| 8.7 | `T\RT\Audio\ThreadDsp.cs` lines 1-334 + `R\RT\Audio\AudioDsp.cs` (`DspLimiter` from line 253, `DspBrown`, `DspSmooth`, `DspSmoothRandom`) | `RT\Audio\NocDsp.cs` | NocturneConstants (sample rate) | class `ThreadDsp` (lines 335-1079: Thread drone/pad/hum/stars engine) | Limiter: ceiling -1.5 dBFS, 5 ms look-ahead, 80 ms release (14.3); 20 Hz DC blocker. |
| 8.8 | `T\RT\Audio\ThreadSynth.cs` | `NocSynth.cs` | NocDsp, NocAudioParams | - | **Generator mode on a clip-less AudioSource is the primary** (lesson 14: never on the listener; Thread used the listener filter first). Footsteps mixed inside the generator (one limiter for everything; FootstepBank clips as voices). |
| 8.9 | `T\RT\Audio\WavWriter.cs`, `FootstepBank.cs`, `OfflineAudioRenderer.cs`, `AudioDirector.cs` | same | NocDsp, NocAudioParams, NocServices, NocturneConstants | Thread surfaces (Salt/Stone/Terrace/Metal/Pier, metal brightness sets), Thread swell windows, beat RMS targets | FootstepBank recipes of 14.3 (wet concrete, wet stone, steel, glass, garden); OfflineAudioRenderer keeps the block loop, meters, `IAudioBlockSink`; renders at a given speed. |
| 8.10 | `T\RT\Audio\ThreadAudioParams.cs` | `NocAudioParams.cs` | NocMath | all Thread layers | New layer targets of 14.3 (pure functions of s and t, NocturneScore). |
| 8.11 | `T\RT\QA\QaJson.cs`, `QaReport.cs` | same | CliArgs | - | verbatim. |
| 8.12 | `T\RT\QA\QaCommon.cs` | same | CliArgs, NocturneConstants, NocServices, PerfMonitor, QaJson | Thread speeds | `PathGuideProbe.TypeName` = `"Megastructure.PathGuide"` (line 166, reflection). |
| 8.13 | `T\RT\QA\QaRunnerBase.cs`, `AutoTestRunner.cs`, `WallBot.cs`, `SoakRunner.cs` | same | QaCommon, QaJson, QaReport, BeatDirector, GameBootstrap, NocServices, AutoWalker | 573.33/344 s, rest at 860, eye-on-grade checks (flat world) | Expected times 4.4 (d); AutoTestRunner drives the **AutoWalker** (Part 9: same walker in tests; `-cinematic -autotest` = cinematic mode, non-cancellable) and waits for the 17 s ending, then exit 0; `-autotest-until S`; WallBot 0.40 m inside, both edges, at 2.5 m/s (2 x 600 s at 60 fps). |
| 8.14 | `T\RT\QA\PerfMonitor.cs`, `CaptureWriter.cs` | same | QaCommon, QaJson, NocClock | - | `-perf-log` uncapped; record VRAM via report fields (2.5.4 budget, nvidia-smi outside). |
| 8.15 | `T\RT\QA\QaProjection.cs` | same | NocMath (D3) | `ThreadColumn`, `KeyFrameId`, `Evaluate(tag...)` Thread subjects | Keep `QaView`, `AzElDir`, `Basis`, `ProjectDir/Point`, `PixelX/YTop`, `FocalPx`, `PanStepDeg`, `InsideWithMargin`, `DiscAtInfinity`, `Cylinder`, `Points`; add box subjects. |
| 8.16 | `T\RT\QA\ShotPlan.cs` | same | QaProjection, NocturneConstants | Thread extra stills, transition windows | Sets: keyshots (14.10 table: 1080, 4K, 1680x1050, 2560x1080), beats (start/mid/end + free-look yaw +90/-90/180, pitch 0/+60, -60 for beats 3-4), sequences per 14.10 (`boundaries`, `statics`, `signs`, `rain`, `koi`, `traffic`, `encode`), `pose:NAME:x:y:z:yaw:pitch:fov` (from `A\RT\QA\KeyShots.cs` ~504). |
| 8.17 | `T\RT\QA\CaptureHarness.cs` | same | ShotPlan, CaptureWriter, QaProjection, NocClock, ProgressDirector, NocServices, InstancedRegistryDriver | Thread FogMath luma use | capture mode: `-freeze-clock`, koi wave time `(s - 1260)/1.5`, rain `t_r` frozen with `-freeze-rain`, call `SubmitAllFor(captureCam)` for instanced batches. Keep RGBA16F MSAA 4 + D32 RT, async readback, parity set. |
| 8.18 | `A\RT\QA\CompositionLint.cs` (`ProjectPoint`, `ProjectBounds` with near-plane clipping, `Visibility`, `Check`, `ShotStatus`) | `RT\QA\NocVisibility.cs` | QaJson (Thread), NocMath | `LoggedConflicts`, `Run` (BeatBase/KeyShots), Abyss QaJson | Base of the EditMode 11.13 audit writing `QA\reports\visibility.json` (follower pose included). |
| 8.19 | `T\ED\EditorCommon.cs` | same (`NocPaths`, `NocLog`, `EditorUtil`) | - | Thread paths | `ProductName "Nocturne"`, `CompanyName "Megastructures"`, shader list (NocGrade, NocLine, NocGraybox, later NocSky/NocFacade/...), `NocPalette.hlsl`, `NocConstantsGen.hlsl`, `NocTheme.tss`, `NocPanel.asset`, layer 30 "Corridor", 31 "Safety". Player.log then lives at `%USERPROFILE%\AppData\LocalLow\Megastructures\Nocturne\Player.log`. |
| 8.20 | `T\ED\BatchTools.cs` | same | all editor files, CliArgs | `BuildMac` optional, Thread Validate items (skybox ThreadSky, Thread materials) | `NOC_BUILD_DIR` env for the output folder (lesson 10, A `BatchTools.cs` line ~26 pattern); Validate adds: shadows off, depth `Depth_32_Stencil_8`, D3D11 only, camera dithering off, `renderPostProcessing false`, GRD off, UI shaders always included, sky = NocSky dome (no `RenderSettings.skybox` dependency), material/shader errors via `ShaderUtil.ShaderHasError` (lesson 4). |
| 8.21 | `T\ED\ProjectConfigurator.cs` | same | EditorCommon, MaterialFactory, CliArgs | - | D3D11 explicit (already default), splash off, linear, input handler 1 (already set), fullscreen window, resizable, run in background, 48 kHz audio (Ring `ProjectSetup` AudioSampleRate pattern), Always Included = `Megastructure/*` + UI shaders. |
| 8.22 | `T\ED\PipelineConfigurator.cs` | same | EditorCommon, NocturneConstants | `ShadowDistance 160`, `ShadowResolution 4096`, cascades, `m_MainLightShadowsSupported true`, soft shadows | HDR on, `_64Bits`, MSAA 4, render scale 1, depth texture on (rain), opaque texture off, **all shadows off**, GRD Disabled, SRP Batcher on, Forward+, depth priming off, `depthAttachmentFormat = DepthFormat.Depth_32_Stencil_8` (explicit), `GradeFeatureType = "Megastructure.NocGradeFeature"` (line 23, reflection), camera: post off, dithering off, AA none. |
| 8.23 | `T\ED\SceneBuilder.cs` | same | EditorCommon, MaterialLibrary, NocturneConstants | Thread component strings (lines 30-35), ThreadLighting, sun | Strings: `Megastructure.QualityBootstrap`, `Megastructure.UiDirector`, `Megastructure.CaptureHarness`, `Megastructure.AutoTestRunner`, `Megastructure.PerfMonitor`, `Megastructure.WallBot`, `Megastructure.SoakRunner`, player `Megastructure.PlayerController` + `CharacterController` + `Megastructure.AutoWalker`, camera `Megastructure.CameraRig` + `AudioListener`, generator object `AudioSource` + `Megastructure.NocSynth` + `Megastructure.AudioDirector`, `Megastructure.PathGuide`, UIDocument + NocPanel; camera clear = Floor colour; near 0.05, far 12 000. |
| 8.24 | `T\ED\MaterialFactory.cs`, `ShaderGen.cs`, `PaletteWriter.cs`, `TemplateCleanup.cs`, `CaptureWatchdog.cs` | same | EditorCommon, MaterialLibrary, NocturnePalette, NocturneConstants | Thread `Specs()`, `MaterialConstants` tints | Specs for Grade + graybox first; ShaderGen writes `NocPalette.hlsl`/`NocConstantsGen.hlsl`; TemplateCleanup deletes SampleScene, TutorialInfo, Readme, Mobile_* and SampleSceneProfile (Part 7.2). |

### 4.9 Strings that a rename silently breaks (compile does not catch them)

`PipelineConfigurator.GradeFeatureType` (`T\ED\PipelineConfigurator.cs` line 23) and `ProjectTests` line 36; `QaCommon.PathGuideProbe.TypeName` (`T\RT\QA\QaCommon.cs` line 166); `SceneBuilder` component lists (lines 30-35); `ProjectTests` `InvokeEditorStatic("Megastructure.EditorTools.ShaderGen", ...)` (lines 155-156, 222-223); shader names in `NocPaths`; `[THREAD]` greps in tools; CaptureWatchdog EditorPrefs keys (harmless). SceneBuilder already logs a Problem for a missing type, so RebuildAll fails loudly; add an EditMode test that every listed name resolves.

---

## 5. Minimal closed set for the three-beat graybox autotest (Part 3.1 rule 2)

Runtime: CliArgs, NocturneConstants (path, beats, speeds, camera, dither, UI timings, gaze keys), NocturnePalette, NocMath, NocClock(+GradeState), NocServices, NocRig, WorldContext, MaterialLibrary, ShaderIds; FilletPath, NocLanes, GazeSchedule, WalkPlanner, AutoWalker, PlayerController, PlayerMath, CameraRig, NocInput, QualityBootstrap, PathGuide, CorridorKit, MeshBuilder + MeshKit (4 files) + MeshValidation + VertexBake; GameBootstrap, ProgressDirector (skeleton), BeatDirector; NocGradeFeature/Pass/Math, BlueNoise, R2Sequence; UiDirector, UiTimeline; QaJson, QaReport, QaCommon, QaRunnerBase, AutoTestRunner, PerfMonitor (QaCommon calls `PerfMonitor.FlushActive`). Audio optional (all hooks are null-tolerant). Editor: EditorCommon, BatchTools, ProjectConfigurator, PipelineConfigurator, SceneBuilder, MaterialFactory, ShaderGen, PaletteWriter, TemplateCleanup, NocUiSetup. Shaders: NocGrade, NocGraybox (+ NocLitCore/Pass/Lighting, NocAtmosphere stub with the 14.2.2 functions returning the fog formula and the sky gradient), generated HLSL. Beats: `Beats\Beat01_Graybox\` .. `Beat03_Graybox\` builders (floor slab along the whole path, coloured marker slabs at s 120/400, lane-edge barriers). Tests: ProjectTests subset, GradeTests, path tests (length 1500.00 +- 0.05, tangent points, heading table of 14.0).

Run: RebuildAll -> Validate -> BuildWindows -> `run-player.ps1 -Name smoke -PlayerArgs "-autotest,-brisk,-autotest-until,400"` (beats 1-3 in order, corner C1, 0 errors, ~160 s + load) -> full `-autotest` (1.5 m/s, <= 1 030 s) and `-autotest -cinematic` (2.5 m/s, <= 620 s), both through gpu-guard with the 60 fps cap.

---

## 6. Known bugs and lessons the import must fix

1. **Audio generator on the listener** (`T\RT\Audio\ThreadSynth.cs` header: lives on the AudioListener; generator mode only as fallback) -> generator on a clip-less AudioSource first (lesson 14; R research 05).
2. **Shader warm-up**: Thread only `Shader.WarmupAllShaders()` -> Abyss WarmupLoader, 3 frames per material behind black (14.6).
3. **Uncapped QA**: Thread QualityBootstrap `targetFrameRate = -1`; AutoTestRunner uses `captureDeltaTime 1/60` and renders as fast as it can -> 60 fps cap in autotest/soak/wallbot/sequences (2.5.3) and gpu-guard routing.
4. **Dither default temporal** in Thread (R2 offset advances unless `-freeze-dither`) -> static by default (14.7 H1).
5. **Dark gain / amplitude 1.0** -> per-pixel 2.0 x lerp(1, 0.6, ...) (4.7).
6. **Shadows on, depth Default** in PipelineConfigurator -> shadows off, `Depth_32_Stencil_8`.
7. **D3D12** in Ring `ProjectSetup.UseD3D12 = true` -> never copy; D3D11 (8.1).
8. **Duplicate fullscreen/pause/cinematic handling** when mixing Ring PlayerController with Thread QualityBootstrap/AutoWalker -> one owner each (4.3, section 2).
9. **`-progress` in metres** in Thread -> fraction (14.12).
10. **run-player** does not route `-capture-set sequences/encode/...` through gpu-guard -> extend the heavy detector.
11. **No per-owner build folder** in Thread BatchTools (lesson 10) -> `NOC_BUILD_DIR`.
12. **Stall watchdog** default 6 min in queue/run-player killed silent runs (lesson 2) -> `-StallMin 0` for tests; soak/long jobs via gpu-guard time limits.
13. **Fast compile blind to asmdef references** (Ring blocker: missing GPUDriven reference failed the first Unity compile) -> Ring fastcheck.py.
14. **Float distance integrator** in Ring WalkPlanner/AutoWalker -> double (4.3, 3.4 arithmetic).
15. **Abyss metrics cwd exclusion** would drop this continued session -> keep the lead's per-record anchor filter; never use Abyss's file-level cwd test.
16. **Thread metrics without anchor** counted from the first transcript record (would include Thread's records since 2026-10-29) -> anchor version (done).
17. **Reflection strings** (4.9).
18. **Ring QaHooks forced the cinematic walk speed** -> not imported (3.1 c).
19. **Instanced fields missing in RT captures** -> `SubmitAllFor(camera)` (lesson 8).
20. **Ring UI fade drawn after the dither** (overlay re-quantises fades without dither) -> `ShaderFade = true` + `GradeState.FadeBlack`.
21. **Thread Validate expects `RenderSettings.skybox == ThreadSky`** -> NocSky is a BackdropRig dome; rewrite that check.
22. **Thread CameraRig hard-codes near 0.1 / far 250 000 from constants** -> 0.05 / 12 000 (14.12).
23. **ThreadLine coupled to the Thread flat recipe, cable shadow and helix fades** -> strip (4.7, 7.14).
24. **Ring lighting tunnel mask `_RingSet` and ship shadow `RingKeyAtten`** would darken lights -> delete.
25. **Thread BeatDirector one-sided hysteresis 12 m** -> 8 m forward / 12 m back for activation (14.6).
26. **PowerShell exit codes** through gpu-guard: verify once (1.3).
27. **Line endings**: byte-identical generated HLSL needs `* -text` (present in `N\.gitattributes`).
28. **Thread autotest/tests numbers and docs quoting 573/344 s, Ring 636.8 s / 49 s holds** -> rewrite (4.4 d).

---

## 7. Ordered import procedure with a compile gate after each step

Gate F = `python -I Tools/fastcheck.py all` (after the first Unity compile). Gate U = `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity-queue.ps1 -Name imp_<n>` (no `-Method`: compile and quit; check for `error CS` and `[NOC-FAIL]`), from step 6 on `-Method Megastructure.EditorTools.BatchTools.CompileCheck`. Gate T = `... -Name tests_<n> -NoQuit -StallMin 0 -ExtraArgs "-runTests,-testPlatform,EditMode,-testResults,<abs N>\QA\reports\tests_<n>.xml"`. Commit after every green gate. Never copy `.meta`, `Library`, `Builds`, `QA`, `Logs`, `.csproj/.sln`.

| Step | Content | Gate |
|---|---|---|
| 1 | Folders; 4 asmdefs + asmref (4.0); Tools 1.6-1.11 | U (empty assemblies compile) |
| 2 | Core pure C#: NocMath, NocturneConstants (graybox subset), NocturnePalette, NocClock, NocServices, NocRig, CliArgs, WorldContext (no MeshKit call yet), MaterialLibrary, ShaderIds; BlueNoise, R2Sequence, NocGradeMath | U, then F works from here |
| 3 | Abyss geometry (MeshBuilder, MeshKit x4, MeshValidation, VertexBake, CorridorKit), BackdropRig, InstancedBatch, InstancedRegistry(+Driver); WorldContext triangle count | F, U; port A `MeshKitTests` (33 tests) |
| 4 | Player: FilletPath, NocLanes, GazeSchedule, WalkPlanner, PlayerMath, NocInput, CameraRig, QualityBootstrap, AutoWalker, PlayerController, PathGuide | F, U, T (path + gaze + StopRamp tests) |
| 5 | Systems + UI: GameBootstrap, ProgressDirector, BeatDirector, AtmosphereDriver skeleton, WarmupLoader, UiDirector, UiTimeline | F, U |
| 6 | Rendering: NocGradeFeature, NocGradePass, NocGrade.shader, gen_hlsl run, NocAtmosphere/NocCommon/NocLit*/NocGraybox | U (shader import errors appear in the log) |
| 7 | Editor tools (8.19-8.24, NocUiSetup) | U with CompileCheck; `-Method ...RebuildAll`; `-Method ...Validate` (0 problems) |
| 8 | QA runtime (8.11-8.18) | F, U, T (QaTests patterns, GradeTests) |
| 9 | Audio (8.7-8.10) | F, U, T (DSP block tests, limiter ceiling, DC) |
| 10 | NocLine.shader, DetailTier (optional) | U + Validate |
| 11 | Three graybox builders; `-Method ...BuildWindows`; smoke `-autotest-until 400`; full autotest at 1.5 and 2.5 m/s via run-player (gpu-guard); log scan zero errors | player exit 0; commit "Phase 2 framework import" |

---

## 8. EditMode test patterns to bring along

- `T\TE\GradeTests.cs` (21): blue noise permutation/determinism/spectrum, R2, Neutral, contrast, vignette, bloom pyramid, shader-constant mirror; replace the dark-gain test by the 4.7 amplitude tests.
- `T\TE\QaTests.cs` (35): keep the ShotPlan/QaProjection/CaptureWriter/QaJson generic ones; drop sky/thread-column tests.
- `T\TE\AudioTests.cs` (12): DSP block tests (SVF stability up to 20 kHz, FDN decay, DC, WAV writer); add limiter ceiling -1.5 dBFS.
- `T\TE\ProjectTests.cs` (12): settings contract, byte-identical generated HLSL vs `gen_hlsl.py`, grade feature present, input handler, graphics API.
- `T\TE\PlayerTests.cs` (13): StopRamp and wrap/critdamp parts.
- `R\TE\PathScheduleTests.cs` (11): rewrite for 1 500 m path, keys and timings (4.4 d).
- `A\TE\MeshKitTests.cs` (33): normals/winding/degenerates/index format/bounds.
- `T\Assets\_Game\Scripts\Tests\PlayMode\InputTests.cs`: `Keyboard.current`/`Mouse.current` not null (Part 7.6).
Thread HEAD passed 218/218 (`T\QA\reports\tests_final.xml`).

---

## 9. Sources

Local (primary, read 2026-11-13):
- `T\Assets\_Game\Scripts\Runtime\**`, `T\Assets\_Game\Scripts\Editor\*.cs`, `T\Assets\_Game\Shaders\ThreadGrade.shader`, `ThreadLine.shader`, `T\Tools\*`, `T\DEVELOPMENT.md` (D-001..D-082), `T\FINAL_REPORT.md`, `T\STATE.md`, `T\QA\reports\tests_final.xml` — dependency scan (scripted), APIs, constants, defects.
- `R\Assets\_Game\Scripts\Runtime\Player\*.cs`, `UI\*.cs`, `Directors\AtmosphereDriver.cs`, `Terrain\TerrainIndirectField.cs`, `Rendering\InstancedField*.cs`, `Audio\AudioDsp.cs`, `RingAudioEngine.cs`, `Core\RingState.cs`, `RingRig.cs`, `RingConstants.cs`, `Editor\Batch\ProjectSetup.cs`, `Shaders\Include\RingLit*.hlsl`, `RingLighting.hlsl`, `RingCeramic.shader`, `Tools\fastcheck.py`, `DEVELOPMENT.md` (ADR-001..013, research decisions, blockers), `FINAL_REPORT.md`.
- `A\Assets\_Game\Scripts\Runtime\Geometry\*.cs`, `Rendering\BackdropRig.cs`, `InstancedBatch.cs`, `InstancedRegistry*.cs`, `DetailTier.cs`, `Core\GameState.cs`, `SceneClock.cs`, `ClockDriver.cs`, `WarmupLoader.cs`, `CliArgs.cs`, `QA\CompositionLint.cs`, `KeyShots.cs`, `QaHooks.cs`, `Tools\metrics.py`, `Tools\qa\encode_test.py`, `Tools\lookdev.ps1`, `DEVELOPMENT.md` (ADR table, encode/dither decision: static 2.0 LSB TPDF), `FINAL_REPORT.md`.
- `N\Library\PackageCache\com.unity.render-pipelines.universal@8457e85b8184\ShaderLibrary\Core.hlsl` lines 13-22; `Runtime\ForwardLights.cs` lines 576-579; `Runtime\UniversalRendererData.cs` lines 59-101, 146, 276; `package.json` line 4 — Forward+ keyword semantics, depth format enum.
- `N\Tools\unity-queue.ps1`, `run-player.ps1`, `metrics.py`, `gpu-guard.ps1`; `N\MASTER_PROMPT.md` Parts 3, 8, 11, 14 — current state of the lead's adapted tools and the specification.

Web (accessed 2026-11-13):
- https://github.com/PowerShell/PowerShell/issues/20400 — `Start-Process -PassThru` Process objects can lack `ExitCode`; cache `Handle` first (gpu-guard verification item 1.3).
- https://kiwix.lema.org/stackoverflow.com_en_all_2023-11/a/23797762 (Stack Overflow answer mirror) — same `ExitCode` null behaviour and the `$p.Handle` workaround.
